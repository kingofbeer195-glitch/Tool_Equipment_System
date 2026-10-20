using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tool_Equipment_Control_System
{
    public partial class Authorize : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        SqlConnection conn_jig = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString);
        // user_master DB (ตาราง sys_user_master) — ใช้ในแท็บ New User
        SqlConnection conn_user = new SqlConnection(ConfigurationManager.ConnectionStrings["User"].ConnectionString);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        datacon dc = new datacon();

        // ===== Config: E-Mail แจ้งผลอนุมัติ New User =====
        private const string SMTP_HOST = "150.61.249.70";
        private const int SMTP_PORT = 25;
        private const string MAIL_FROM = "engineer_pde@mail.canon";

        // ── โหมดทดสอบ ────────────────────────────────
        //  true  = ส่งเข้า TEST_MAIL_TO อย่างเดียว (ไม่ส่งหา user จริง)
        //  false = ส่งจริงไปหา user
        //  *** ก่อนขึ้น Production เปลี่ยนเป็น false ***
        private const bool TEST_MODE = false;
        private const string TEST_MAIL_TO = "anirut@mail.canon";

        // ===== Auto All config (step 30,31 = J3 Up Approve) =====
        private const int AUTO_ALL_STATUS_ID_1 = 30;
        private const int AUTO_ALL_STATUS_ID_2 = 31;
        private const int AUTO_ALL_DIV_ID = 2;
        private static readonly string[] AUTO_ALL_POSITION_CODES = { "J3", "J4", "M1", "M2" };

        // ===== Field: เก็บ emp_id ที่กำลัง Edit =====
        private string EditingEmpId
        {
            get { return ViewState["EditingEmpId"] as string ?? ""; }
            set { ViewState["EditingEmpId"] = value; }
        }

        // ========================================================
        //  Flow Authorization — state ใน ViewState
        // ========================================================
        // document_id ที่เลือกอยู่ (string เช่น "AAD1")
        private string SelectedDocumentId
        {
            get { return ViewState["SelDocId"] as string ?? ""; }
            set { ViewState["SelDocId"] = value; }
        }
        // รายชื่อผู้อนุมัติ (source of truth ใน ViewState) : columns = status_id, emp_id, emp_name, dept_code, src
        //   src = "AUTO" (จาก admin) / "MANUAL" (เพิ่มมือ) / "DB" (โหลดจาก Authorize_list เดิม)
        private DataTable ApproverTable
        {
            get { return ViewState["ApvTable"] as DataTable; }
            set { ViewState["ApvTable"] = value; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // ★ bind CheckBoxList เสมอทุก request (ก่อน ViewState restore)
            //    selection จะถูก restore จาก ViewState ให้อัตโนมัติหลังจากนี้
            BindAddLevels();
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = "Authorize";

            if (!IsPostBack)
            {
                hfActiveTab.Value = "tab-flow";
                BindAdmins();
                BindProcessList();
                BindNewUsers();
            }
        }

        // ========================================================
        //  HRMS Lookup — เมื่อใส่ emp_id ครบ 6 หลัก
        // ========================================================
        protected void txtEmpId_TextChanged(object sender, EventArgs e)
        {
            string emp = (txtEmpId.Text ?? "").Trim();

            ClearEmpInfo();
            //pnlAddLevelWrap.Visible = false;   // ซ่อนไว้ก่อน
            ShowLevelPanel(false);

            if (emp.Length != 6)
            {
                if (emp.Length > 0)
                    ShowAlert("alert-danger", "กรุณากรอกรหัสพนักงานให้ครบ 6 หลัก");
                return;
            }

            if (AdminExists(emp))
            {
                txtEmpId.Style["background-color"] = "#f8d7da";
                ShowAlert("alert-danger", "พนักงานรหัส " + emp + " เป็น Admin อยู่แล้ว");
                return;
            }

            string name, pos, dept, deptCode, band, email;
            if (!GetDataHRMS_CSharp(emp, out name, out pos, out dept, out deptCode, out band, out email))
            {
                txtEmpId.Style["background-color"] = "#f8d7da";
                ShowAlert("alert-danger", "ไม่พบข้อมูลพนักงานรหัส " + emp);
                return;
            }

            txtEmpName.Text = name;
            txtPosition.Text = pos;
            txtDept.Text = dept;
            txtDeptCode.Text = deptCode;
            hdtxtBand.Value = band;
            hdtxtEmail.Value = email;
            txtEmpId.Style["background-color"] = "#d4edda";

            // ★ Search สำเร็จ -> โชว์ checkbox level + ปุ่ม Save
            //pnlAddLevelWrap.Visible = true;
            ShowLevelPanel(true);
            BindAddLevels();
        }

        // คืนค่าข้อมูลพนักงานผ่าน out params (reuse ได้ทั้ง Admin + Flow)
        private bool GetDataHRMS_CSharp(string userId, out string name, out string pos,
            out string dept, out string deptCode, out string band, out string email)
        {
            name = pos = dept = deptCode = band = email = "";

            string url = "http://cptsvs547:1000/middleware/oracle/hrms";

            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            string sql = "select GNAME_ENG, FNAME_ENG, POSN_ENAME, BAND, DEPT_CODE, DEPT_ABB_NAME, EMAIL " +
                         "from ADMIN.v_emp_data_all_cpt where UPPER(emp_no)=UPPER('" + userId + "')";

            byte[] data = System.Text.Encoding.UTF8.GetBytes("command=" + sql);

            using (var stream = request.GetRequestStream())
                stream.Write(data, 0, data.Length);

            using (var response = (System.Net.HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                string json = reader.ReadToEnd();
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                if (result.data == null || result.data.Count == 0)
                    return false;

                var u = result.data[0];

                name = (string)u.GNAME_ENG + " " + (string)u.FNAME_ENG;
                pos = (string)u.POSN_ENAME;
                dept = (string)u.DEPT_ABB_NAME;
                deptCode = (string)u.DEPT_CODE;
                band = (string)u.BAND;
                email = (string)u.EMAIL;
                return true;
            }
        }

        // HRMS lookup รุ่นที่คืน Division ด้วย (ใช้ในแท็บ New User)
        // คืน div = DIV_ABB_NAME (DIV_NAME), dept = DEPT_ABB_NAME
        private bool GetDataHRMS_NewUser(string userId, out string name, out string pos,
            out string div, out string dept, out string deptCode, out string email)
        {
            name = pos = div = dept = deptCode = email = "";

            string url = "http://cptsvs547:1000/middleware/oracle/hrms";

            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 15000;

            string sql = "select GNAME_ENG, FNAME_ENG, POSN_ENAME, BAND, DIV_NAME, DIV_ABB_NAME, " +
                         "DEPT_CODE, DEPT_ABB_NAME, EMAIL " +
                         "from ADMIN.v_emp_data_all_cpt where UPPER(emp_no)=UPPER('" + userId + "')";

            byte[] data = System.Text.Encoding.UTF8.GetBytes("command=" + sql);

            try
            {
                using (var stream = request.GetRequestStream())
                    stream.Write(data, 0, data.Length);

                using (var response = (System.Net.HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string json = reader.ReadToEnd();
                    dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                    if (result.data == null || result.data.Count == 0)
                        return false;

                    var u = result.data[0];

                    name = (string)u.GNAME_ENG + " " + (string)u.FNAME_ENG;
                    pos = (string)u.POSN_ENAME;
                    string divName = (string)u.DIV_NAME;
                    string divAbb = (string)u.DIV_ABB_NAME;
                    div = string.IsNullOrEmpty(divAbb) ? divName
                          : (divName + " (" + divAbb + ")");
                    dept = (string)u.DEPT_ABB_NAME;
                    deptCode = (string)u.DEPT_CODE;
                    email = (string)u.EMAIL;
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[HRMS NewUser Error] " + userId + " : " + ex.Message);
                return false;
            }
        }
        private void ClearEmpInfo()
        {
            txtEmpName.Text = "";
            txtPosition.Text = "";
            txtDept.Text = "";
            txtDeptCode.Text = "";
            hdtxtBand.Value = "";
            hdtxtEmail.Value = "";
            txtEmpId.Style["background-color"] = "#fff";
        }


        // ========================================================
        //  Bind Admin list
        // ========================================================
        private void BindAdmins()
        {
            DataTable dt = new DataTable();
            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT emp_id, fname, lname, ext, dept_code, level_1, level_2, level_3, level_4 " +
                "FROM Equipment_Admin WHERE ISNULL(status_use,1)=1 ORDER BY emp_id", c))
            {
                c.Open();
                new SqlDataAdapter(cmd).Fill(dt);
            }

            rptAdmins.DataSource = dt;
            rptAdmins.DataBind();

            lblNoAdmin.Visible = (dt.Rows.Count == 0);

            // ผูก CheckBoxList ของ row ที่กำลัง Edit
            if (!string.IsNullOrEmpty(EditingEmpId))
                BindEditRow(dt);
        }



        // ========================================================
        //  Master Level — checkbox สำหรับ Add panel
        // ========================================================
        private DataTable GetMasterLevels()
        {
            DataTable dt = new DataTable();
            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT level_id, level_name, level_detail FROM Master_Level_Admin ORDER BY level_id", c))
            {
                c.Open();
                new SqlDataAdapter(cmd).Fill(dt);
            }
            return dt;
        }
        // cache master levels กันยิง DB ซ้ำในรอบ render เดียว
        private DataTable _mstCache;
        private DataTable MasterLevels
        {
            get
            {
                if (_mstCache == null) _mstCache = GetMasterLevels();
                return _mstCache;
            }
        }

        private string LevelName(int lv)
        {
            DataRow r = MasterLevels.Select("level_id=" + lv).FirstOrDefault();
            if (r == null) return (lv == 4) ? "Super Admin" : "Level" + lv;
            string name = Convert.ToString(r["level_name"]);
            return string.IsNullOrEmpty(name) ? ((lv == 4) ? "Super Admin" : "Level" + lv) : name;
        }

        private string LevelDetail(int lv)
        {
            DataRow r = MasterLevels.Select("level_id=" + lv).FirstOrDefault();
            return (r == null) ? "" : Convert.ToString(r["level_detail"]);
        }

        // badge: Level1 / Level2 / Level3 / Super Admin + tooltip จาก level_detail
        public string BuildLevelBadges(object l1, object l2, object l3, object l4)
        {
            bool[] v = { ToBool(l1), ToBool(l2), ToBool(l3), ToBool(l4) };
            var b = new System.Text.StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                int lv = i + 1;
                string label = (lv == 4) ? "Super Admin" : "Level" + lv;
                string css = v[i] ? "lvl-badge lvl-on" : "lvl-badge lvl-off";
                string tip = HttpUtility.HtmlAttributeEncode(LevelDetail(lv));
                b.Append("<span class='" + css + "' title='" + tip + "'>" +
                         HttpUtility.HtmlEncode(label) + "</span>");
            }
            return b.ToString();
        }
        private void BindAddLevels()
        {
            // ถ้า postback และมี items อยู่แล้ว -> ไม่ต้อง rebind (กัน selection หาย)
            if (Page.IsPostBack && cblAddLevel.Items.Count > 0)
                return;

            DataTable dt = GetMasterLevels();
            cblAddLevel.Items.Clear();
            foreach (DataRow r in dt.Rows)
            {
                int lv = Convert.ToInt32(r["level_id"]);
                string name = Convert.ToString(r["level_name"]);
                string detail = Convert.ToString(r["level_detail"]);
                string label = (lv == 4) ? "Super Admin"
                                         : "Level " + lv + (string.IsNullOrEmpty(name) ? "" : " : " + name);

                ListItem li = new ListItem(label, lv.ToString());
                li.Attributes["title"] = detail;
                cblAddLevel.Items.Add(li);
            }
            cblAddLevel.Attributes["onclick"] = "onLevelClickCBL(this);";
        }
        private void ShowLevelPanel(bool show)
        {
            pnlAddLevelWrap.Style["display"] = show ? "block" : "none";
        }



        // ========================================================
        //  Repeater commands: Edit / Cancel / Save / Delete
        // ========================================================
        protected void rptAdmins_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            hfActiveTab.Value = "tab-admin";
            string empId = (string)e.CommandArgument;

            switch (e.CommandName)
            {
                case "EDIT_ADMIN":
                    EditingEmpId = empId;
                    BindAdmins();
                    break;

                case "CANCEL_ADMIN":
                    // เก็บ History การ Cancel
                    SaveHistory("CANCEL", empId, null);
                    EditingEmpId = "";
                    BindAdmins();
                    ShowAlert("alert-info", "ยกเลิกการแก้ไข emp_id " + empId);
                    break;

                case "SAVE_ADMIN":
                    SaveEditedLevels(empId, e.Item);
                    break;

                case "DELETE_ADMIN":
                    DeleteAdmin(empId);
                    break;
            }
        }


        // บันทึกการแก้ไข level จาก CheckBoxList
        private void SaveEditedLevels(string empId, RepeaterItem item)
        {
            var cbl = item.FindControl("cblEditLevel") as CheckBoxList;
            if (cbl == null) return;

            bool[] nv = new bool[4];
            for (int lv = 1; lv <= 4; lv++)
            {
                ListItem li = cbl.Items.FindByValue(lv.ToString());
                nv[lv - 1] = (li != null && li.Selected);
            }
            // Level 4 -> บังคับ 1-3 ติ๊กทั้งหมด (server-side guard)
            if (nv[3]) { nv[0] = nv[1] = nv[2] = true; }

            // อ่านค่าเดิมก่อน update (ใส่ History)
            bool[] old = GetCurrentLevels(empId);

            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            {
                c.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Equipment_Admin SET level_1=@l1, level_2=@l2, level_3=@l3, level_4=@l4 " +
                    "WHERE emp_id=@emp", c))
                {
                    cmd.Parameters.AddWithValue("@l1", nv[0]);
                    cmd.Parameters.AddWithValue("@l2", nv[1]);
                    cmd.Parameters.AddWithValue("@l3", nv[2]);
                    cmd.Parameters.AddWithValue("@l4", nv[3]);
                    cmd.Parameters.AddWithValue("@emp", empId);
                    cmd.ExecuteNonQuery();
                }
            }

            SaveHistory("EDIT", empId, old, nv);
            EditingEmpId = "";
            BindAdmins();
            ShowAlert("alert-success", "บันทึกการแก้ไข Level ของ " + empId + " สำเร็จ");
        }
        private bool[] GetCurrentLevels(string empId)
        {
            bool[] v = new bool[4];
            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT level_1, level_2, level_3, level_4 FROM Equipment_Admin WHERE emp_id=@emp", c))
            {
                cmd.Parameters.AddWithValue("@emp", empId);
                c.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                    if (rd.Read())
                        for (int i = 0; i < 4; i++) v[i] = ToBool(rd[i]);
            }
            return v;
        }
        private void DeleteAdmin(string empId)
        {
            bool[] old = GetCurrentLevels(empId);

            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "DELETE FROM Equipment_Admin WHERE emp_id=@emp", c))
            {
                cmd.Parameters.AddWithValue("@emp", empId);
                c.Open();
                cmd.ExecuteNonQuery();
            }

            SaveHistory("DELETE", empId, old);
            if (EditingEmpId == empId) EditingEmpId = "";
            BindAdmins();
            ShowAlert("alert-success", "ลบ Admin " + empId + " สำเร็จ");
        }

        // ========================================================
        //  Add New Admin
        // ========================================================
        protected void btnSaveAdmin_Click(object sender, EventArgs e)
        {
            hfActiveTab.Value = "tab-admin";
            string emp = (txtEmpId.Text ?? "").Trim();

            if (emp.Length != 6)
            {
                ShowAlert("alert-danger", "กรุณากรอกรหัสพนักงานให้ครบ 6 หลัก");
                return;
            }
            if (string.IsNullOrEmpty(txtEmpName.Text))
            {
                ShowAlert("alert-danger", "กรุณากดดึงข้อมูลพนักงานก่อน (ไม่พบชื่อ)");
                return;
            }
            if (AdminExists(emp))
            {
                ShowAlert("alert-danger", "พนักงานรหัส " + emp + " เป็น Admin อยู่แล้ว");
                return;
            }

            bool[] nv = new bool[4];
            for (int lv = 1; lv <= 4; lv++)
            {
                ListItem li = cblAddLevel.Items.FindByValue(lv.ToString());
                nv[lv - 1] = (li != null && li.Selected);
            }
            if (nv[3]) { nv[0] = nv[1] = nv[2] = true; }   // server guard

            if (!nv[0] && !nv[1] && !nv[2] && !nv[3])
            {
                ShowAlert("alert-danger", "กรุณาเลือกระดับสิทธิ์ (Level) อย่างน้อย 1 ระดับ");
                return;
            }

            // แยกชื่อ/นามสกุล จาก GNAME_ENG + FNAME_ENG ที่รวมไว้
            string fname = txtEmpName.Text.Trim();
            string lname = "";
            int sp = fname.IndexOf(' ');
            if (sp > 0)
            {
                lname = fname.Substring(sp + 1).Trim();
                fname = fname.Substring(0, sp).Trim();
            }

            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO Equipment_Admin (emp_id, fname, lname, dept_code, status_use, " +
                "level_1, level_2, level_3, level_4) " +
                "VALUES (@emp, @fn, @ln, @dept, 1, @l1, @l2, @l3, @l4)", c))
            {
                cmd.Parameters.AddWithValue("@emp", emp);
                cmd.Parameters.AddWithValue("@fn", fname);
                cmd.Parameters.AddWithValue("@ln", lname);
                cmd.Parameters.AddWithValue("@dept", txtDeptCode.Text.Trim());
                cmd.Parameters.AddWithValue("@l1", nv[0]);
                cmd.Parameters.AddWithValue("@l2", nv[1]);
                cmd.Parameters.AddWithValue("@l3", nv[2]);
                cmd.Parameters.AddWithValue("@l4", nv[3]);
                c.Open();
                cmd.ExecuteNonQuery();
            }

            SaveHistory("ADD", emp, null, nv, fname, lname, txtDeptCode.Text.Trim());

            // reset form
            txtEmpId.Text = "";
            ClearEmpInfo();
            ShowLevelPanel(false);
            BindAdmins();
            ShowAlert("alert-success", "เพิ่ม Admin " + emp + " สำเร็จ");
        }
        private bool AdminExists(string emp)
        {
            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Equipment_Admin WHERE emp_id=@emp", c))
            {
                cmd.Parameters.AddWithValue("@emp", emp);
                c.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // ========================================================
        //  History (Admin)
        // ========================================================
        private void SaveHistory(string action, string empId, bool[] oldLv,
            bool[] newLv = null, string fname = null, string lname = null, string deptCode = null)
        {
            // ถ้าไม่ส่ง fname/dept มา ให้ดึงจาก table
            if (fname == null)
            {
                using (SqlConnection c = new SqlConnection(conn.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT fname, lname, dept_code FROM Equipment_Admin WHERE emp_id=@emp", c))
                {
                    cmd.Parameters.AddWithValue("@emp", empId);
                    c.Open();
                    using (SqlDataReader rd = cmd.ExecuteReader())
                        if (rd.Read())
                        {
                            fname = rd["fname"] as string;
                            lname = rd["lname"] as string;
                            deptCode = rd["dept_code"] as string;
                        }
                }
            }

            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO Equipment_Admin_History " +
                "(action_type, emp_id, fname, lname, dept_code, " +
                " level_1, level_2, level_3, level_4, " +
                " old_level_1, old_level_2, old_level_3, old_level_4, action_by, action_date) " +
                "VALUES (@a, @emp, @fn, @ln, @dept, " +
                " @l1, @l2, @l3, @l4, @o1, @o2, @o3, @o4, @by, GETDATE())", c))
            {
                cmd.Parameters.AddWithValue("@a", action);
                cmd.Parameters.AddWithValue("@emp", empId);
                cmd.Parameters.AddWithValue("@fn", (object)fname ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ln", (object)lname ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dept", (object)deptCode ?? DBNull.Value);

                AddLvParams(cmd, "@l", newLv);
                AddLvParams(cmd, "@o", oldLv);

                cmd.Parameters.AddWithValue("@by", (object)GetCurrentUser() ?? DBNull.Value);
                c.Open();
                cmd.ExecuteNonQuery();
            }
        }
        private void AddLvParams(SqlCommand cmd, string prefix, bool[] lv)
        {
            for (int i = 1; i <= 4; i++)
            {
                object val = (lv == null) ? (object)DBNull.Value : lv[i - 1];
                cmd.Parameters.AddWithValue(prefix + i, val);
            }
        }

        // emp_id ของผู้ใช้ปัจจุบัน — ปรับให้ตรงกับ session SSO ของระบบคุณ
        private string GetCurrentUser()
        {
            return Session["emp_id"] as string ?? "";
        }



        // ========================================================
        //  Alert helper
        // ========================================================
        private void ShowAlert(string cssType, string msg)
        {
            pnlAlert.Visible = true;
            divAlert.Attributes["class"] = "alert " + cssType;
            divAlert.InnerText = msg;
        }

        // helper สำหรับ template
        public bool IsEditing(string empId)
        {
            return EditingEmpId == empId;
        }
        private static bool ToBool(object o)
        {
            return o != null && o != DBNull.Value && Convert.ToBoolean(o);
        }

        // ผูก CheckBoxList ใน edit row ให้ตรงกับ Master + ติ๊กค่าปัจจุบัน
        private void BindEditRow(DataTable dt)
        {
            foreach (RepeaterItem item in rptAdmins.Items)
            {
                var keyLnk = item.FindControl("btnSaveEdit") as LinkButton;
                if (keyLnk == null) continue;
                string empId = keyLnk.CommandArgument;
                if (empId != EditingEmpId) continue;

                var cbl = item.FindControl("cblEditLevel") as CheckBoxList;
                if (cbl == null) return;

                DataTable mst = GetMasterLevels();
                cbl.Items.Clear();
                foreach (DataRow r in mst.Rows)
                {
                    int lv = Convert.ToInt32(r["level_id"]);
                    string name = Convert.ToString(r["level_name"]);
                    string detail = Convert.ToString(r["level_detail"]);
                    string label = (lv == 4) ? "Super Admin"
                                             : "Level" + lv + (string.IsNullOrEmpty(name) ? "" : " : " + name);

                    ListItem li = new ListItem(label, lv.ToString());
                    li.Attributes["title"] = detail;   // ★ tooltip บน label ของ CheckBoxList
                    cbl.Items.Add(li);
                }

                // ติ๊กค่าปัจจุบันจาก dt
                DataRow row = dt.Select("emp_id='" + empId.Replace("'", "''") + "'").FirstOrDefault();
                if (row != null)
                {
                    for (int lv = 1; lv <= 4; lv++)
                    {
                        ListItem li = cbl.Items.FindByValue(lv.ToString());
                        if (li != null) li.Selected = ToBool(row["level_" + lv]);
                    }
                }

                // Level 4 auto-tick 1-3 ฝั่ง client
                cbl.Attributes["onclick"] = "onLevelClickCBL(this);";
                return;
            }
        }


        // ============================================================================
        // ============================================================================
        //                       FLOW AUTHORIZATION
        // ============================================================================
        // ============================================================================

        // ---- map document_id (AAD1..AAD4) -> ชื่อ column ใน Master_Status ----
        private string GetSysColumn(string documentId)
        {
            switch ((documentId ?? "").Trim().ToUpper())
            {
                case "AAD1": return "sys_borrow";    // Borrow
                case "AAD2": return "sys_change";    // Change
                case "AAD3": return "sys_withdraw";  // Withdraw
                case "AAD4": return "sys_storage";   // Storage
                default: return null;
            }
        }

        // ---- โหลด Process ลง dropdown ----
        private void BindProcessList()
        {
            DataTable dt = new DataTable();
            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT document_id, document_name + ' : ' + Comment as document_name" +
                " FROM Master_Document " +
                "WHERE ISNULL(status_use,1)=1 ORDER BY document_id", c))
            {
                c.Open();
                new SqlDataAdapter(cmd).Fill(dt);
            }

            ddlProcess.Items.Clear();
            ddlProcess.Items.Add(new ListItem("-- เลือก Process --", ""));
            foreach (DataRow r in dt.Rows)
            {
                ddlProcess.Items.Add(new ListItem(
                    Convert.ToString(r["document_name"]),
                    Convert.ToString(r["document_id"])));
            }
        }

        // ---- เปลี่ยน Process -> โหลด Flow steps + รายชื่อ ----
        protected void ddlProcess_SelectedIndexChanged(object sender, EventArgs e)
        {
            hfActiveTab.Value = "tab-flow";
            pnlAlert.Visible = false;

            string docId = (ddlProcess.SelectedValue ?? "").Trim();
            if (string.IsNullOrEmpty(docId))
            {
                pnlFlow.Visible = false;
                SelectedDocumentId = "";
                ApproverTable = null;
                return;
            }

            string sysCol = GetSysColumn(docId);
            if (sysCol == null)
            {
                pnlFlow.Visible = false;
                ShowAlert("alert-danger", "ยังไม่ได้ map Process นี้กับ column ใน Master_Status");
                return;
            }

            SelectedDocumentId = docId;
            litFlowProcessName.Text = ddlProcess.SelectedItem.Text;

            // สร้างตารางรายชื่อใหม่ แล้วเติมค่า auto/db
            DataTable apv = NewApproverTable();
            LoadAutoAndDbApprovers(docId, sysCol, apv);
            ApproverTable = apv;

            BindFlowSteps(docId, sysCol);
            pnlFlow.Visible = true;
        }

        private DataTable NewApproverTable()
        {
            DataTable t = new DataTable();
            t.Columns.Add("status_id", typeof(int));
            t.Columns.Add("emp_id", typeof(string));
            t.Columns.Add("emp_name", typeof(string));
            t.Columns.Add("dept_code", typeof(string));
            t.Columns.Add("src", typeof(string));   // AUTO / MANUAL / DB
            return t;
        }

        // ดึง Flow steps (status) ของ process
        private DataTable GetFlowSteps(string docId, string sysCol)
        {
            DataTable dt = new DataTable();
            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT status_id, status_name FROM Master_Status " +
                "WHERE [" + sysCol + "]=1 AND ISNULL(status_use,1)=1 AND status_id not in (100,999)" +
                " ORDER BY status_id", c))
            {
                c.Open();
                new SqlDataAdapter(cmd).Fill(dt);
            }
            return dt;
        }

        // เติมรายชื่อ auto (admin level_1) สำหรับ status 50/90
        // + โหลดรายชื่อเดิมจาก Authorize_list (DB) สำหรับ manual step
        private void LoadAutoAndDbApprovers(string docId, string sysCol, DataTable apv)
        {
            DataTable steps = GetFlowSteps(docId, sysCol);

            // 1) auto admin -> ใส่ให้ status 50/90 ที่ปรากฏใน flow
            DataTable admins = GetLevel1Admins();
            foreach (DataRow s in steps.Rows)
            {
                int sid = Convert.ToInt32(s["status_id"]);
                if (sid == 50 || sid == 90)
                {
                    foreach (DataRow a in admins.Rows)
                    {
                        apv.Rows.Add(sid,
                            Convert.ToString(a["emp_id"]),
                            Convert.ToString(a["emp_name"]),
                            Convert.ToString(a["dept_code"]),
                            "AUTO");
                    }
                }
            }

            // 2) โหลดรายชื่อเดิมจาก Authorize_list (เฉพาะ manual step = ไม่ใช่ 1/50/90)
            DataTable dbRows = new DataTable();
            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT emp_id, status_id FROM Authorize_list WHERE document_id=@doc", c))
            {
                cmd.Parameters.AddWithValue("@doc", docId);
                c.Open();
                new SqlDataAdapter(cmd).Fill(dbRows);
            }

            foreach (DataRow d in dbRows.Rows)
            {
                int sid = Convert.ToInt32(d["status_id"]);
                if (sid == 1 || sid == 50 || sid == 90) continue;   // auto/skip ไม่โหลดจาก DB
                string emp = Convert.ToString(d["emp_id"]);

                // กันซ้ำ
                if (apv.Select("status_id=" + sid + " AND emp_id='" + emp.Replace("'", "''") + "'").Length > 0)
                    continue;

                string name, pos, dept, deptCode, band, email;
                if (GetDataHRMS_CSharp(emp, out name, out pos, out dept, out deptCode, out band, out email))
                    apv.Rows.Add(sid, emp, name, deptCode, "DB");
                else
                    apv.Rows.Add(sid, emp, emp, "", "DB");   // ดึงชื่อไม่ได้ ใช้ emp_id แทน
            }
        }

        private DataTable GetLevel1Admins()
        {
            DataTable dt = new DataTable();
            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT emp_id, (ISNULL(fname,'') + ' ' + ISNULL(lname,'')) AS emp_name, dept_code " +
                "FROM Equipment_Admin WHERE level_1=1 AND ISNULL(status_use,1)=1 ORDER BY emp_id", c))
            {
                c.Open();
                new SqlDataAdapter(cmd).Fill(dt);
            }
            return dt;
        }

        // bind repeater flow steps
        private void BindFlowSteps(string docId, string sysCol)
        {
            DataTable steps = GetFlowSteps(docId, sysCol);

            // ใส่ column ช่วยสำหรับแสดง badge mode
            steps.Columns.Add("step_mode", typeof(string));
            steps.Columns.Add("step_mode_label", typeof(string));
            foreach (DataRow r in steps.Rows)
            {
                int sid = Convert.ToInt32(r["status_id"]);
                if (sid == 1) { r["step_mode"] = "SKIP"; r["step_mode_label"] = "ทุกคน"; }
                else if (sid == 50 || sid == 90) { r["step_mode"] = "AUTO"; r["step_mode_label"] = "Auto Admin"; }
                else { r["step_mode"] = "MANUAL"; r["step_mode_label"] = "กำหนดเอง"; }
            }

            _flowStepCount = steps.Rows.Count;
            rptFlowSteps.DataSource = steps;
            rptFlowSteps.DataBind();
        }

        // ---- helpers สำหรับ template ----
        private int _flowStepCount;

        public bool IsSkipStep(object statusId) { return Convert.ToInt32(statusId) == 1; }
        public bool IsAutoStep(object statusId) { int s = Convert.ToInt32(statusId); return s == 50 || s == 90; }
        public bool IsManualStep(object statusId)
        {
            int s = Convert.ToInt32(statusId);
            return s != 1 && s != 50 && s != 90;
        }
        public bool IsAutoAllStep(object statusId)
        {
            if (Convert.ToInt32(statusId) == AUTO_ALL_STATUS_ID_1 || Convert.ToInt32(statusId) == AUTO_ALL_STATUS_ID_2)
            {
                return true;
            }               
            
            return false;
        }
        public bool IsLastStep(int itemIndex) { return itemIndex >= _flowStepCount - 1; }

        public string GetStepBadgeCss(object mode)
        {
            switch (Convert.ToString(mode))
            {
                case "AUTO": return "badge-auto";
                case "MANUAL": return "badge-manual";
                default: return "badge-skip";
            }
        }

        // รายชื่อใน step (เรียกจาก template)
        public DataView GetApprovers(object statusId)
        {
            DataTable t = ApproverTable;
            if (t == null) return new DataTable().DefaultView;
            DataView dv = new DataView(t);
            dv.RowFilter = "status_id=" + Convert.ToInt32(statusId);
            dv.Sort = "emp_id";
            return dv;
        }
        public int GetApproverCount(object statusId)
        {
            DataTable t = ApproverTable;
            if (t == null) return 0;
            return t.Select("status_id=" + Convert.ToInt32(statusId)).Length;
        }

        // ---- commands ใน rptFlowSteps: เพิ่ม/ลบ รายชื่อ ----
        protected void rptFlowSteps_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            hfActiveTab.Value = "tab-flow";

            if (e.CommandName == "ADD_APV")
            {
                int sid = Convert.ToInt32(e.CommandArgument);
                var txt = e.Item.FindControl("txtApvEmp") as TextBox;
                string emp = (txt == null ? "" : (txt.Text ?? "").Trim());
                AddManualApprover(sid, emp);
            }
            else if (e.CommandName == "REMOVE_APV")
            {
                // CommandArgument = "status_id|emp_id"
                string[] parts = Convert.ToString(e.CommandArgument).Split('|');
                if (parts.Length == 2)
                    RemoveApprover(Convert.ToInt32(parts[0]), parts[1]);
            }
            else if (e.CommandName == "AUTO_ALL")
            {
                int sid = Convert.ToInt32(e.CommandArgument);
                RunAutoAll(sid);
            }
        }

        private void AddManualApprover(int statusId, string emp)
        {
            if (emp.Length != 6)
            {
                ShowAlert("alert-danger", "กรุณากรอกรหัสพนักงานให้ครบ 6 หลัก");
                RebindFlowFromState();
                return;
            }

            DataTable t = ApproverTable;
            if (t == null) { RebindFlowFromState(); return; }

            // กันซ้ำใน step เดียวกัน
            if (t.Select("status_id=" + statusId + " AND emp_id='" + emp.Replace("'", "''") + "'").Length > 0)
            {
                ShowAlert("alert-danger", "รหัส " + emp + " มีอยู่ใน step นี้แล้ว");
                RebindFlowFromState();
                return;
            }

            string name, pos, dept, deptCode, band, email;
            if (!GetDataHRMS_CSharp(emp, out name, out pos, out dept, out deptCode, out band, out email))
            {
                ShowAlert("alert-danger", "ไม่พบข้อมูลพนักงานรหัส " + emp);
                RebindFlowFromState();
                return;
            }

            t.Rows.Add(statusId, emp, name, deptCode, "MANUAL");
            ApproverTable = t;
            RebindFlowFromState();
            ShowAlert("alert-success", "เพิ่ม " + name + " (" + emp + ") สำเร็จ");
        }

        private void RemoveApprover(int statusId, string emp)
        {
            DataTable t = ApproverTable;
            if (t == null) { RebindFlowFromState(); return; }

            DataRow[] rows = t.Select("status_id=" + statusId + " AND emp_id='" + emp.Replace("'", "''") + "'");
            foreach (DataRow r in rows) t.Rows.Remove(r);

            ApproverTable = t;
            RebindFlowFromState();
            ShowAlert("alert-info", "ลบรหัส " + emp + " ออกจาก step แล้ว");
        }
        // ============================================================
        //  AUTO ALL (step 30 = J3 Up Approve)
        //  - ลบคนใน Authorize_list ที่ position_code ไม่ใช่ J3/J4/M1/M2
        //  - เพิ่มคน div_id=2 ที่ position_code = J3/J4/M1/M2 ซึ่งยังไม่มี
        //  - เขียน DB ทันที + ลง History แล้ว reload ApproverTable
        // ============================================================
        private void RunAutoAll(int statusId)
        {
            string docId = SelectedDocumentId;
            if (string.IsNullOrEmpty(docId))
            {
                ShowAlert("alert-danger", "กรุณาเลือก Process ก่อน");
                return;
            }

            string sysCol = GetSysColumn(docId);
            if (sysCol == null) { RebindFlowFromState(); return; }

            string docName = ddlProcess.SelectedItem != null ? ddlProcess.SelectedItem.Text : "";
            string statusName = GetStatusName(statusId);

            // ---------- 1) หา set ของ position_code ที่ valid (J3/J4/M1/M2) ----------
            // valid position_id list (int) จาก Position_master
            HashSet<int> validPositionIds = GetValidPositionIds();

            // ---------- 2) รายชื่อปัจจุบันใน Authorize_list (doc + status 30) ----------
            // key = emp_id (trim) -> position_id (int?, null ถ้าหาไม่เจอใน user_master)
            DataTable current = new DataTable();
            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT emp_id FROM Authorize_list " +
                "WHERE document_id=@doc AND status_id=@sid", c))
            {
                cmd.Parameters.AddWithValue("@doc", docId);
                cmd.Parameters.AddWithValue("@sid", statusId);
                c.Open();
                new SqlDataAdapter(cmd).Fill(current);
            }

            // emp_id ที่มีอยู่แล้ว (trim, upper) เพื่อกันซ้ำตอน add
            HashSet<string> existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow r in current.Rows)
                existing.Add(Convert.ToString(r["emp_id"]).Trim());

            // map emp_id -> position_id จาก user_master สำหรับคนที่อยู่ใน list ปัจจุบัน
            Dictionary<string, int?> curPosition = GetPositionIdsForEmps(existing);

            int deleted = 0, added = 0;

            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            {
                c.Open();
                using (SqlTransaction tx = c.BeginTransaction())
                {
                    try
                    {
                        // ---------- 3) DELETE: คนที่ position ไม่ valid ----------
                        foreach (string emp in existing)
                        {
                            int? pid;
                            bool has = curPosition.TryGetValue(emp, out pid);
                            bool valid = has && pid.HasValue && validPositionIds.Contains(pid.Value);

                            if (!valid)
                            {
                                using (SqlCommand del = new SqlCommand(
                                    "DELETE FROM Authorize_list " +
                                    "WHERE document_id=@doc AND status_id=@sid AND LTRIM(RTRIM(emp_id))=@emp",
                                    c, tx))
                                {
                                    del.Parameters.AddWithValue("@doc", docId);
                                    del.Parameters.AddWithValue("@sid", statusId);
                                    del.Parameters.AddWithValue("@emp", emp);
                                    del.ExecuteNonQuery();
                                }
                                InsertAuthHistoryTx(c, tx, "DELETE", docId, docName,
                                    statusId, statusName, emp);
                                deleted++;
                            }
                        }

                        // ---------- 4) ADD: คน div_id=2 + position valid ที่ยังไม่มี ----------
                        DataTable candidates = GetAutoAllCandidates();  // emp_id, fname, lname, dept_code
                        foreach (DataRow r in candidates.Rows)
                        {
                            string emp = Convert.ToString(r["emp_id"]).Trim();
                            if (string.IsNullOrEmpty(emp)) continue;
                            if (existing.Contains(emp)) continue;   // มีอยู่แล้ว ไม่ add

                            using (SqlCommand ins = new SqlCommand(
                                "INSERT INTO Authorize_list (emp_id, document_id, status_id) " +
                                "VALUES (@emp, @doc, @sid)", c, tx))
                            {
                                ins.Parameters.AddWithValue("@emp", emp);
                                ins.Parameters.AddWithValue("@doc", docId);
                                ins.Parameters.AddWithValue("@sid", statusId);
                                ins.ExecuteNonQuery();
                            }

                            InsertAuthHistoryTx(c, tx, "ADD", docId, docName,
                                statusId, statusName, emp);
                            existing.Add(emp);   // กันซ้ำในรอบเดียวกัน
                            added++;
                        }

                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        ShowAlert("alert-danger", "Auto All ไม่สำเร็จ: " + ex.Message);
                        return;
                    }
                }
            }

            // ---------- 5) reload ApproverTable จาก DB ใหม่ ----------
            ReloadApproverTableFromDb(docId, sysCol);
            RebindFlowFromState();
            ShowAlert("alert-success",
                "Auto All เสร็จสิ้น — เพิ่ม " + added + " คน, ลบ " + deleted + " คน");
        }

        // position_id ทั้งหมดที่ position_code อยู่ใน J3/J4/M1/M2
        private HashSet<int> GetValidPositionIds()
        {
            HashSet<int> ids = new HashSet<int>();

            // สร้าง IN (...) จาก AUTO_ALL_POSITION_CODES แบบ parameterized
            var pnames = new List<string>();
            for (int i = 0; i < AUTO_ALL_POSITION_CODES.Length; i++)
                pnames.Add("@p" + i);

            using (SqlConnection c = new SqlConnection(conn_user.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT position_id FROM Position_master " +
                "WHERE LTRIM(RTRIM(position_code)) IN (" + string.Join(",", pnames) + ")", c))
            {
                for (int i = 0; i < AUTO_ALL_POSITION_CODES.Length; i++)
                    cmd.Parameters.AddWithValue("@p" + i, AUTO_ALL_POSITION_CODES[i]);
                c.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                    while (rd.Read())
                        ids.Add(Convert.ToInt32(rd["position_id"]));
            }
            return ids;
        }

        // map emp_id -> position_id (จาก sys_user_master) เฉพาะ emp ที่สนใจ
        private Dictionary<string, int?> GetPositionIdsForEmps(HashSet<string> emps)
        {
            var map = new Dictionary<string, int?>(StringComparer.OrdinalIgnoreCase);
            if (emps.Count == 0) return map;

            using (SqlConnection c = new SqlConnection(conn_user.ConnectionString))
            {
                c.Open();
                foreach (string emp in emps)
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT TOP 1 position_id FROM sys_user_master " +
                        "WHERE LTRIM(RTRIM(emp_id))=@emp", c))
                    {
                        cmd.Parameters.AddWithValue("@emp", emp);
                        object o = cmd.ExecuteScalar();
                        if (o == null || o == DBNull.Value)
                            map[emp] = null;
                        else
                            map[emp] = Convert.ToInt32(o);
                    }
                }
            }
            return map;
        }

        // พนักงาน div_id=2 + position_code valid (candidates ที่จะ add)
        private DataTable GetAutoAllCandidates()
        {
            DataTable dt = new DataTable();

            var pnames = new List<string>();
            for (int i = 0; i < AUTO_ALL_POSITION_CODES.Length; i++)
                pnames.Add("@p" + i);

            using (SqlConnection c = new SqlConnection(conn_user.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT u.emp_id, u.fname, u.lname, u.dept_code " +
                "FROM sys_user_master u " +
                "INNER JOIN Position_master p ON u.position_id = p.position_id " +
                "WHERE u.div_id=@div " +
                "  AND LTRIM(RTRIM(p.position_code)) IN (" + string.Join(",", pnames) + ") " +
                "  AND u.emp_id IS NOT NULL AND LTRIM(RTRIM(u.emp_id)) <> '' " +
                "ORDER BY u.emp_id", c))
            {
                cmd.Parameters.AddWithValue("@div", AUTO_ALL_DIV_ID);
                for (int i = 0; i < AUTO_ALL_POSITION_CODES.Length; i++)
                    cmd.Parameters.AddWithValue("@p" + i, AUTO_ALL_POSITION_CODES[i]);
                c.Open();
                new SqlDataAdapter(cmd).Fill(dt);
            }
            return dt;
        }

        // ลง History ผ่าน transaction เดียวกัน (emp_name/dept_code ดึงจาก HRMS + fallback)
        private void InsertAuthHistoryTx(SqlConnection c, SqlTransaction tx,
            string action, string docId, string docName,
            int statusId, string statusName, string emp)
        {
            string empName = "", deptCode = "";

            // ลอง HRMS ก่อน
            string name, pos, dept, dc, band, email;
            if (GetDataHRMS_CSharp(emp, out name, out pos, out dept, out dc, out band, out email))
            {
                empName = name;
                deptCode = dc;
            }
            else
            {
                // fallback: ดึงชื่อจาก sys_user_master
                try
                {
                    using (SqlConnection cu = new SqlConnection(conn_user.ConnectionString))
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT TOP 1 fname, lname, dept_code FROM sys_user_master " +
                        "WHERE LTRIM(RTRIM(emp_id))=@emp", cu))
                    {
                        cmd.Parameters.AddWithValue("@emp", emp);
                        cu.Open();
                        using (SqlDataReader rd = cmd.ExecuteReader())
                            if (rd.Read())
                            {
                                string fn = rd["fname"] == DBNull.Value ? "" : Convert.ToString(rd["fname"]);
                                string ln = rd["lname"] == DBNull.Value ? "" : Convert.ToString(rd["lname"]);
                                empName = (fn + " " + ln).Trim();
                                deptCode = rd["dept_code"] == DBNull.Value ? "" : Convert.ToString(rd["dept_code"]);
                            }
                    }
                }
                catch { /* ปล่อยว่างได้ */ }
                if (string.IsNullOrEmpty(empName)) empName = emp;
            }

            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO Authorize_list_History " +
                "(action_type, document_id, document_name, status_id, status_name, " +
                " emp_id, emp_name, dept_code, action_by, action_date) " +
                "VALUES (@a, @doc, @docn, @sid, @sn, @emp, @en, @dc, @by, GETDATE())", c, tx))
            {
                cmd.Parameters.AddWithValue("@a", action);
                cmd.Parameters.AddWithValue("@doc", docId);
                cmd.Parameters.AddWithValue("@docn", (object)docName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@sid", statusId);
                cmd.Parameters.AddWithValue("@sn", (object)statusName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@emp", emp);
                cmd.Parameters.AddWithValue("@en", (object)empName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dc", (object)deptCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@by", (object)GetCurrentUser() ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        // reload ApproverTable จาก DB (auto+db) หลังกด Auto All
        private void ReloadApproverTableFromDb(string docId, string sysCol)
        {
            DataTable apv = NewApproverTable();
            LoadAutoAndDbApprovers(docId, sysCol, apv);
            ApproverTable = apv;
        }

        // rebind flow โดยใช้ document ที่เลือกไว้ + state เดิม
        private void RebindFlowFromState()
        {
            string docId = SelectedDocumentId;
            if (string.IsNullOrEmpty(docId)) { pnlFlow.Visible = false; return; }

            string sysCol = GetSysColumn(docId);
            if (sysCol == null) { pnlFlow.Visible = false; return; }

            litFlowProcessName.Text = ddlProcess.SelectedItem != null ? ddlProcess.SelectedItem.Text : "";
            BindFlowSteps(docId, sysCol);
            pnlFlow.Visible = true;
        }

        // ---- Save Authorize ----
        protected void btnSaveAuthorize_Click(object sender, EventArgs e)
        {
            hfActiveTab.Value = "tab-flow";

            string docId = SelectedDocumentId;
            if (string.IsNullOrEmpty(docId))
            {
                ShowAlert("alert-danger", "กรุณาเลือก Process ก่อน");
                return;
            }

            DataTable t = ApproverTable;
            if (t == null) t = NewApproverTable();

            // อ่านรายชื่อเดิมจาก DB เพื่อทำ History (diff add/delete)
            DataTable before = new DataTable();
            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT emp_id, status_id FROM Authorize_list WHERE document_id=@doc", c))
            {
                cmd.Parameters.AddWithValue("@doc", docId);
                c.Open();
                new SqlDataAdapter(cmd).Fill(before);
            }

            string docName = ddlProcess.SelectedItem != null ? ddlProcess.SelectedItem.Text : "";

            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            {
                c.Open();
                using (SqlTransaction tx = c.BeginTransaction())
                {
                    try
                    {
                        // ลบของเดิมทั้ง process แล้ว insert ใหม่ (replace ทั้งชุด)
                        using (SqlCommand del = new SqlCommand(
                            "DELETE FROM Authorize_list WHERE document_id=@doc", c, tx))
                        {
                            del.Parameters.AddWithValue("@doc", docId);
                            del.ExecuteNonQuery();
                        }

                        foreach (DataRow r in t.Rows)
                        {
                            int sid = Convert.ToInt32(r["status_id"]);
                            if (sid == 1) continue;   // skip step ไม่บันทึก
                            string emp = Convert.ToString(r["emp_id"]);

                            using (SqlCommand ins = new SqlCommand(
                                "INSERT INTO Authorize_list (emp_id, document_id, status_id) " +
                                "VALUES (@emp, @doc, @sid)", c, tx))
                            {
                                ins.Parameters.AddWithValue("@emp", emp);
                                ins.Parameters.AddWithValue("@doc", docId);
                                ins.Parameters.AddWithValue("@sid", sid);
                                ins.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        ShowAlert("alert-danger", "บันทึกไม่สำเร็จ: " + ex.Message);
                        return;
                    }
                }
            }

            // ---- History diff ----
            SaveAuthorizeHistoryDiff(docId, docName, before, t);

            ShowAlert("alert-success", "บันทึก Authorize ของ Process \"" + docName + "\" สำเร็จ");
            RebindFlowFromState();
        }

        // เทียบ before(DB) กับ after(state) -> ADD/DELETE ลง History
        private void SaveAuthorizeHistoryDiff(string docId, string docName, DataTable before, DataTable after)
        {
            // key = status_id|emp_id  (ข้าม skip step)
            HashSet<string> beforeSet = new HashSet<string>();
            foreach (DataRow r in before.Rows)
            {
                int sid = Convert.ToInt32(r["status_id"]);
                if (sid == 1) continue;
                beforeSet.Add(sid + "|" + Convert.ToString(r["emp_id"]));
            }

            HashSet<string> afterSet = new HashSet<string>();
            foreach (DataRow r in after.Rows)
            {
                int sid = Convert.ToInt32(r["status_id"]);
                if (sid == 1) continue;
                afterSet.Add(sid + "|" + Convert.ToString(r["emp_id"]));
            }

            // ADD = after - before
            foreach (string k in afterSet)
                if (!beforeSet.Contains(k))
                    InsertAuthHistory("ADD", docId, docName, k, after);

            // DELETE = before - after
            foreach (string k in beforeSet)
                if (!afterSet.Contains(k))
                    InsertAuthHistory("DELETE", docId, docName, k, after);
        }

        private void InsertAuthHistory(string action, string docId, string docName, string key, DataTable after)
        {
            string[] p = key.Split('|');
            int sid = Convert.ToInt32(p[0]);
            string emp = p[1];

            // หา name/dept จาก after ก่อน (ถ้าเป็น delete อาจไม่มี -> ดึง HRMS)
            string empName = "", deptCode = "", statusName = GetStatusName(sid);
            DataRow[] hit = after.Select("status_id=" + sid + " AND emp_id='" + emp.Replace("'", "''") + "'");
            if (hit.Length > 0)
            {
                empName = Convert.ToString(hit[0]["emp_name"]);
                deptCode = Convert.ToString(hit[0]["dept_code"]);
            }
            else
            {
                string name, pos, dept, dc, band, email;
                if (GetDataHRMS_CSharp(emp, out name, out pos, out dept, out dc, out band, out email))
                { empName = name; deptCode = dc; }
            }

            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO Authorize_list_History " +
                "(action_type, document_id, document_name, status_id, status_name, " +
                " emp_id, emp_name, dept_code, action_by, action_date) " +
                "VALUES (@a, @doc, @docn, @sid, @sn, @emp, @en, @dc, @by, GETDATE())", c))
            {
                cmd.Parameters.AddWithValue("@a", action);
                cmd.Parameters.AddWithValue("@doc", docId);
                cmd.Parameters.AddWithValue("@docn", (object)docName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@sid", sid);
                cmd.Parameters.AddWithValue("@sn", (object)statusName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@emp", emp);
                cmd.Parameters.AddWithValue("@en", (object)empName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dc", (object)deptCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@by", (object)GetCurrentUser() ?? DBNull.Value);
                c.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private string GetStatusName(int statusId)
        {
            using (SqlConnection c = new SqlConnection(conn.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT status_name FROM Master_Status WHERE status_id=@sid", c))
            {
                cmd.Parameters.AddWithValue("@sid", statusId);
                c.Open();
                object o = cmd.ExecuteScalar();
                return o == null ? "" : Convert.ToString(o);
            }
        }

        // class สำหรับแถบสีซ้ายของ step (ใช้ใน markup: is-manual / is-auto / is-skip)
        public string GetStepItemCss(object mode)
        {
            switch (Convert.ToString(mode))
            {
                case "AUTO": return "is-auto";
                case "MANUAL": return "is-manual";
                default: return "is-skip";
            }
        }


        // ============================================================================
        // ============================================================================
        //                       NEW USER — ยืนยันการ Register
        // ============================================================================
        // ============================================================================
        //  ดึงจาก user_master.dbo.sys_user_master โดย tool_equipment = 50
        //  status = 50 : New User / ร้องขอเข้าระบบ
        //  status = 1  : ร้องขอเข้าระบบ (มี user เดิม แต่ยังไม่มีสิทธิ์)
        //  ข้อมูล Division / Dept / Position ดึงสดจาก HRMS ด้วย emp_id
        // ============================================================================

        // โหลดรายชื่อ New User + เติมข้อมูล HRMS แล้ว bind repeater
        private void BindNewUsers()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("emp_id", typeof(string));
            dt.Columns.Add("req_status", typeof(int));      // status ในระบบ (50/1)
            dt.Columns.Add("status_label", typeof(string)); // ป้ายแสดงสถานะ
            dt.Columns.Add("emp_name", typeof(string));
            dt.Columns.Add("division", typeof(string));
            dt.Columns.Add("dept", typeof(string));
            dt.Columns.Add("position", typeof(string));
            dt.Columns.Add("email", typeof(string));

            // 1) ดึงรายชื่อที่ร้องขอ (tool_equipment = 50, status 50 หรือ 1)
            DataTable req = new DataTable();
            using (SqlConnection c = new SqlConnection(conn_user.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT emp_id, user_name, fname, lname, email, status " +
                "FROM sys_user_master " +
                "WHERE tool_equipment = 50 AND status IN (1, 50) " +
                "ORDER BY status DESC, emp_id", c))
            {
                c.Open();
                new SqlDataAdapter(cmd).Fill(req);
            }

            // 2) เติมข้อมูล HRMS (Division/Dept/Position/Email) รายคน
            foreach (DataRow r in req.Rows)
            {
                string empId = Convert.ToString(r["emp_id"]).Trim();
                int st = (r["status"] == DBNull.Value) ? 0 : Convert.ToInt32(r["status"]);

                string name, pos, div, dept, deptCode, email;
                bool ok = GetDataHRMS_NewUser(empId, out name, out pos, out div, out dept, out deptCode, out email);

                if (!ok || string.IsNullOrEmpty(name))
                {
                    // HRMS ดึงไม่ได้ → fallback ใช้ค่าจาก sys_user_master
                    string fn = r["fname"] == DBNull.Value ? "" : Convert.ToString(r["fname"]);
                    string ln = r["lname"] == DBNull.Value ? "" : Convert.ToString(r["lname"]);
                    name = (fn + " " + ln).Trim();
                    if (name == "") name = empId;
                    pos = pos ?? "";
                    div = div ?? "";
                    dept = dept ?? "";
                }

                // Email : ใช้จาก HRMS ก่อน ถ้าไม่มีใช้จาก sys_user_master
                if (string.IsNullOrEmpty(email))
                    email = r["email"] == DBNull.Value ? "" : Convert.ToString(r["email"]).Trim();

                string label = (st == 50)
                             ? "New User / ร้องขอเข้าระบบ"
                             : "ร้องขอเข้าระบบ";

                dt.Rows.Add(empId, st, label, name, div, dept, pos, email);
            }

            rptNewUsers.DataSource = dt;
            rptNewUsers.DataBind();
            lblNoNewUser.Visible = (dt.Rows.Count == 0);
        }

        // escape ค่าที่เอาไปใส่ใน JS string (กัน quote/newline พัง)
        public string JsEscape(object o)
        {
            string s = o == null ? "" : Convert.ToString(o);
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"")
                    .Replace("'", "\\'").Replace("\r", "").Replace("\n", " ");
        }

        // repeater command: Approve
        protected void rptNewUsers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            hfActiveTab.Value = "tab-newuser";

            if (e.CommandName == "APPROVE_USER")
            {
                string empId = Convert.ToString(e.CommandArgument).Trim();
                ApproveNewUser(empId);
            }
        }

        // ยืนยันการ Register : update สิทธิ์ + ส่งอีเมลแจ้งผล (ถ้ามี email)
        private void ApproveNewUser(string empId)
        {
            if (string.IsNullOrEmpty(empId))
            {
                ShowAlert("alert-danger", "ไม่พบรหัสพนักงาน");
                return;
            }

            // อ่านข้อมูล user ก่อน update (เอา email + ชื่อไว้ส่งเมล)
            string userEmail = "", fname = "", lname = "";
            using (SqlConnection c = new SqlConnection(conn_user.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT email, fname, lname FROM sys_user_master " +
                "WHERE emp_id=@emp AND tool_equipment = 50 AND status IN (1,50)", c))
            {
                cmd.Parameters.AddWithValue("@emp", empId);
                c.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        userEmail = rd["email"] == DBNull.Value ? "" : Convert.ToString(rd["email"]).Trim();
                        fname = rd["fname"] == DBNull.Value ? "" : Convert.ToString(rd["fname"]);
                        lname = rd["lname"] == DBNull.Value ? "" : Convert.ToString(rd["lname"]);
                    }
                    else
                    {
                        rd.Close();
                        ShowAlert("alert-danger", "ไม่พบรายการร้องขอของรหัส " + empId + " (อาจถูกอนุมัติไปแล้ว)");
                        BindNewUsers();
                        return;
                    }
                }
            }

            // update : tool_equipment = 1, status = 1, Regist_date = now()
            try
            {
                using (SqlConnection c = new SqlConnection(conn_user.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE sys_user_master " +
                    "SET tool_equipment = 1, status = 1, Regist_date = GETDATE() " +
                    "WHERE emp_id=@emp AND tool_equipment = 50 AND status IN (1,50)", c))
                {
                    cmd.Parameters.AddWithValue("@emp", empId);
                    c.Open();
                    int n = cmd.ExecuteNonQuery();
                    if (n == 0)
                    {
                        ShowAlert("alert-danger", "ไม่สามารถอนุมัติได้ (รายการอาจถูกอนุมัติไปแล้ว)");
                        BindNewUsers();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert("alert-danger", "อนุมัติไม่สำเร็จ: " + ex.Message);
                return;
            }

            // ส่งอีเมลแจ้งผล — ถ้า user ไม่มี email ให้ข้าม (ยังถือว่าอนุมัติสำเร็จ)
            string mailNote = "";
            string toName = (fname + " " + lname).Trim();
            if (TEST_MODE)
            {
                // โหมดทดสอบ: ส่งไป anirut อย่างเดียว (ไม่สนว่ามี email จริงไหม)
                try
                {
                    SendApproveMail(TEST_MAIL_TO, toName, empId);
                    mailNote = " [TEST MODE] ส่งเมลไปที่ " + TEST_MAIL_TO + " อย่างเดียว";
                }
                catch (Exception ex)
                {
                    mailNote = " (แต่ส่งเมลไม่สำเร็จ: " + ex.Message + ")";
                }
            }
            else if (!string.IsNullOrEmpty(userEmail) && userEmail.Contains("@"))
            {
                try
                {
                    SendApproveMail(userEmail, toName, empId);
                    mailNote = " และส่งเมลแจ้งผู้ใช้แล้ว";
                }
                catch (Exception ex)
                {
                    mailNote = " (แต่ส่งเมลไม่สำเร็จ: " + ex.Message + ")";
                }
            }
            else
            {
                mailNote = " (ผู้ใช้ไม่มี Email จึงไม่ได้ส่งแจ้ง)";
            }

            BindNewUsers();
            ShowAlert("alert-success", "อนุมัติ (Register) รหัส " + empId + " สำเร็จ" + mailNote);
        }

        // ส่งอีเมลแจ้งผลการอนุมัติ
        private void SendApproveMail(string toEmail, string toName, string empId)
        {
            using (var msg = new System.Net.Mail.MailMessage())
            {
                msg.From = new System.Net.Mail.MailAddress(MAIL_FROM, "ENG4 - Jig Tool & Equipment System");
                msg.To.Add(toEmail);
                msg.Subject = (TEST_MODE ? "[TEST] " : "") +
                              "[Jig Tool & Equipment System] อนุมัติการลงทะเบียนเข้าระบบ";
                msg.SubjectEncoding = System.Text.Encoding.UTF8;
                msg.BodyEncoding = System.Text.Encoding.UTF8;
                msg.IsBodyHtml = true;

                string greet = string.IsNullOrEmpty(toName) ? "เรียนผู้ใช้งาน" : ("เรียน คุณ" + toName);
                msg.Body =
                    "<div style='font-family:Segoe UI,Tahoma,Arial,sans-serif; font-size:14px; color:#1e293b;'>" +
                    "<p style='font-size:18px; font-weight:700; margin:0 0 8px;'>อนุมัติการลงทะเบียนเรียบร้อย</p>" +
                    "<p>" + Server.HtmlEncode(greet) + ",</p>" +
                    "<p>บัญชีผู้ใช้ของคุณ (รหัส <b>" + Server.HtmlEncode(empId) + "</b>) " +
                        "ได้รับการอนุมัติให้เข้าใช้งานระบบ <b>Jig Tool &amp; Equipment System</b> แล้ว</p>" +
                    "<p>คุณสามารถเข้าสู่ระบบด้วย Username / Password ที่ลงทะเบียนไว้ได้ทันที</p>" +
                    "<hr style='border:none; border-top:1px solid #cbd5e1; margin:14px 0;' />" +
                    "<p style='font-size:12px; color:#888; margin:0;'>" +
                        "อีเมลฉบับนี้ส่งจากระบบอัตโนมัติ กรุณาอย่าตอบกลับ</p>" +
                    "</div>";

                using (var smtp = new System.Net.Mail.SmtpClient(SMTP_HOST, SMTP_PORT))
                {
                    smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    smtp.EnableSsl = false;
                    smtp.Send(msg);
                }
            }
        }
    }
}
