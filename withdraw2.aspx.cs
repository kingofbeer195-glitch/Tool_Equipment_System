using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
//using System.Text.RegularExpressions;


namespace Tool_Equipment_Control_System
{
    public partial class Withdraw_Tool : System.Web.UI.Page
    {
        SqlConnection conn_user = new SqlConnection(ConfigurationManager.ConnectionStrings["user"].ConnectionString);
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        SqlConnection conn_jig = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        datacon dc = new datacon();

        // ── Manual (รูปแบบเดียวกับ borrow_tool2 : แยกไฟล์ตาม Step) ──────────
        private const string MANUAL_FOLDER =
            @"\\cptfile02\Pro\PRJ2023008\50_Flow_Manual\01_Manual\08_Tool&Equipment\03_Withdraw";
        private static readonly string[] MANUAL_FILES = { "Step1", "Step2", "Step3", "Step4", "SaveDraft", "SendRequest", "CancelRequest", "Confirm", "Reject" };

        // ── AAD Confirm : Status Use / ผู้รับผิดชอบ (อ้างอิง NotificationOfChange) ──
        private DataTable _statusUseM, _respM;      // master (โหลดครั้งเดียวต่อ Request)
        private const string STATUS_USE = "1";      // tool_equipment_status : Use
        private const string STATUS_SPARE = "2";    // tool_equipment_status : Spare

        // ── Requester_Info : Doc_Type ของหน้าเบิก ──
        private const string DOC_TYPE = "AAD3";

        protected DataTable BorrowListDT
        {
            get
            {
                if (ViewState["BorrowListDT"] == null)
                {
                    ViewState["BorrowListDT"] = CreateEmptyChangeTable();
                }
                return (DataTable)ViewState["BorrowListDT"];
            }
            set
            {
                ViewState["BorrowListDT"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = "เบิกอุปกรณ์";

            string Doc_NO = null;
            string mode = "edit"; // default
            ViewState["MODE"] = mode;
            hdIsAdmin.Value = (Session["admin"] != null && Session["admin"].ToString() == "99") ? "99" : "0";
            phManualUpload.Visible = (hdIsAdmin.Value == "99");

            if (Session["emp_id"] == null || Session["emp_id"].ToString() == "")
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                // ✅ อ่าน Doc No (Encrypt / Base64)
                if (!string.IsNullOrEmpty(Request.QueryString["k"]))
                {
                    Doc_NO = Encoding.UTF8.GetString(
                        Convert.FromBase64String(Request.QueryString["k"])
                    );
                }
                // ✅ อ่าน mode
                if (!string.IsNullOrEmpty(Request.QueryString["mode"]))
                {
                    mode = Request.QueryString["mode"].ToLower();
                }
                // ✅ กรณีเดิม (Backward compatible)
                else if (!string.IsNullOrEmpty(Request.QueryString["issue_no"]))
                {
                    Doc_NO = Request.QueryString["issue_no"];
                    BindBorrowList();
                }

                ViewState["MODE"] = mode;
                ViewState["DEPT_MASTER"] = GetDeptMaster();
                ViewState["MODEL_MASTER"] = GetModelMaster();

                GetReasonBorrowMaster();

                if (Doc_NO == "0")
                {
                    Requester_getdata();

                    // กำหนดค่าใน TextBox "วันที่ยืม" เป็นวันที่ปัจจุบัน
                    txtIssueDate.Text = DateTime.Today.ToString("dd-MMM-yyyy");

                    // ✅ ตรวจสอบ ADMIN เพื่อกำหนดการแสดงปุ่ม
                    SetButtonVisibilityByRole();
                }
                else
                {
                    // ✅ ป้องกันเข้าหน้าแบบไม่ถูกต้อง
                    if (string.IsNullOrEmpty(Doc_NO))
                    {
                        alert_toast("ไม่พบเลขเอกสาร", "Error", "error", "5000");
                        return;
                    }

                    litIssueNo.Text = Doc_NO;
                    string status_id = status_show(Doc_NO);
                    ViewState["STATUS_ID"] = status_id;

                    LoadRequester(Doc_NO);
                    LoadBorrowList(Doc_NO);
                    Bind_History_Reject(Doc_NO);
                    litRjmDoc.Text = Server.HtmlEncode(Doc_NO);

                    if (mode == "view")
                    {
                        SetViewMode(status_id);
                    }
                    else if (mode == "confirm")
                    {
                        SetConfirmMode(status_id);
                    }
                    else if (mode == "return")
                    {
                        SetReturnMode(status_id);
                    }
                    else
                    {
                        if (CancelButton != null) CancelButton.Visible = true;
                        ShowRejectHistoryIfAny();
                    }
                }
            }

            //GetDataUser_check();
        }

        // ============================================================
        // Requester_getdata() — เรียกตอน Page_Load (!IsPostBack)
        // ============================================================
        protected void Requester_getdata()
        {
            string emp_id = Session["emp_id"] != null ? Session["emp_id"].ToString() : "";
            string admin = Session["admin"] != null ? Session["admin"].ToString() : "";

            // ---------- Admin : ล็อกอินด้วย dept code ----------
            if (admin == "99")
            {
                // ยังไม่มี EmpId → รอให้ user กรอก
                pnlNewEmp.Visible = true;
                lblempNotice.Text = "กรุณากรอกรหัสพนักงาน [Employee ID]";
                txtEmpId.ReadOnly = false;
                return;
            }

            // ---------- User ปกติ ----------
            // ✅ parameterized
            string str_dept = "SELECT * FROM View_dept_master WHERE dept_code = @dept_code";
            dc.QryDT_P(conn, str_dept,
                new SqlParameter("@dept_code", SqlDbType.NVarChar) { Value = emp_id });

            if (dc.dt.Rows.Count > 0)
            {
                ViewState["OGN_DEPTCODE"] = dc.dt.Rows[0]["dept_code"].ToString().Trim();
                ViewState["OGN_DEPTNAME"] = dc.dt.Rows[0]["dept_name"].ToString().Trim();
            }
            else
            {
                ViewState["OGN_DEPTCODE"] = "";
                ViewState["OGN_DEPTNAME"] = "";

                // ใส่ emp_id ลง textbox แล้วดึง HRMS
                txtEmpId.Text = emp_id;
            }

            bool found = GetDataHRMS_CSharp(emp_id);

            if (!found)
            {
                pnlNewEmp.Visible = true;
                lblempNotice.Text = "ไม่พบ Employee ID นี้ในระบบ";
                txtEmpId.Style["background-color"] = "#ff000082";
                ClearEmployeeUI();
                return;
            }

            // เก็บค่าไว้ใช้ต่อ
            SaveEmployeeToViewState();

            txtEmpId.ReadOnly = true;
            pnlNewEmp.Visible = false;
        }

        // ============================================================
        // Requester_getdata_edit() — ตรวจ dept (User ปกติเท่านั้น)
        // ============================================================
        protected void Requester_getdata_edit()
        {
            string admin = Session["admin"].ToString();
            string deptLogin = ViewState["OGN_DEPTNAME"] != null
                                ? ViewState["OGN_DEPTNAME"].ToString()
                                : "";

            // Admin ไม่ตรวจ dept
            if (admin == "99")
            {
                pnlNewEmp.Visible = false;
                return;
            }

            if (string.IsNullOrEmpty(txtEmpName.Text))
            {
                pnlNewEmp.Visible = true;
                lblempNotice.Text = "ขออภัย ไม่พบ Employee ID นี้ในระบบ";
                txtEmpId.Style["background-color"] = "#ff000082";
                return;
            }

            if (txtDept.Text != deptLogin)
            {
                pnlNewEmp.Visible = true;
                lblempNotice.Text = string.Format(
                    "ขออภัย Dept ไม่ตรงกัน [ Login: {0}  ≠  Employee: {1} ]",
                    deptLogin, txtDept.Text);
                txtEmpId.Style["background-color"] = "#ff000082";
                return;
            }

            pnlNewEmp.Visible = false;
        }

        // ============================================================
        // ตรวจสอบ ADMIN
        //   - ADMIN (admin == "99") : ซ่อน btnSaveDraft, btnSend → แสดงเฉพาะ btnConfirm
        //   - User ปกติ            : แสดง btnSaveDraft, btnSend → ซ่อน btnConfirm
        // ============================================================
        private void SetButtonVisibilityByRole()
        {
            string admin = Session["admin"] != null ? Session["admin"].ToString() : "";

            if (admin == "99")
            {
                // ADMIN → ข้าม Save Draft / Request Borrow → แสดงแค่ btnConfirm
                if (DraftButton != null) DraftButton.Visible = false;
                if (SendButton != null) SendButton.Visible = false;
                if (ConfirmButton2 != null) ConfirmButton2.Visible = true;
            }
            else
            {
                // User ปกติ → แสดง btnSaveDraft, btnSend → ซ่อน btnConfirm
                if (DraftButton != null) DraftButton.Visible = true;
                if (SendButton != null) SendButton.Visible = true;
                if (ConfirmButton2 != null) ConfirmButton2.Visible = false;
            }
        }

        // ============================================================
        // txtEmpId_TextChanged — Admin กรอก EmpId แล้ว PostBack
        // ============================================================
        protected void txtEmpId_TextChanged(object sender, EventArgs e)
        {
            string empId = txtEmpId.Text.Trim();
            string admin = Session["admin"].ToString();

            try
            {
                if (empId.Length < 6)
                {
                    ClearEmployeeUI();
                    pnlNewEmp.Visible = true;
                    lblempNotice.Text = "กรุณากรอกรหัสพนักงาน [Employee ID]";
                    return;
                }

                bool found = GetDataHRMS_CSharp(empId);

                if (!found)
                {
                    pnlNewEmp.Visible = true;
                    lblempNotice.Text = "ไม่พบ Employee ID นี้ในระบบ";
                    txtEmpId.Style["background-color"] = "#ff000082";
                    ClearEmployeeUI();
                    return;
                }

                // เก็บค่าไว้
                SaveEmployeeToViewState();

                // Admin : ไม่ต้องตรวจ dept → ซ่อน notice ได้เลย
                if (admin == "99")
                {
                    pnlNewEmp.Visible = false;
                    return;
                }

                // User ปกติ : ตรวจ dept
                Requester_getdata_edit();
            }
            catch (Exception ex)
            {
                alert_toast(ex.ToString(), "txtEmpId_TextChanged", "warn", "10000");
            }
        }
        private bool GetDataHRMS_CSharp(string userId)
        {
            // ✅ กันก่อนส่ง: ถ้า format ไม่ถูก ไม่ยิง middleware เลย
            if (!IsValidEmpNo(userId))
                return false;

            string url = "http://cptsvs547:1000/middleware/oracle/hrms";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            string str_tel = "Select ext From sys_user_master Where emp_id = '" + userId + "'";
            dc.QryDT(conn_user, str_tel);
            if (dc.dt.Rows.Count > 0)
            {
                txtTel.Text = dc.dt.Rows[0]["ext"].ToString();
            }

            // userId ผ่าน whitelist [A-Za-z0-9] แล้ว → ปลอดภัยพอจะต่อใน string
            // (Oracle bind variable ทำที่ middleware ไม่ได้ จึงพึ่ง validation)
            string sql =
                    "select GNAME_ENG, FNAME_ENG, POSN_ENAME, BAND, DEPT_CODE, DEPT_ABB_NAME, EMAIL " +
                    "from ADMIN.v_emp_data_all_cpt where UPPER(emp_no)=UPPER('" + userId + "')";

            // ✅ URL-encode payload กัน &/= ในอนาคต
            byte[] data = Encoding.UTF8.GetBytes("command=" + HttpUtility.UrlEncode(sql));

            using (var stream = request.GetRequestStream())
                stream.Write(data, 0, data.Length);

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                string json = reader.ReadToEnd();
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                if (result.data == null || result.data.Count == 0)
                    return false;

                var u = result.data[0];

                txtEmpName.Text = u.GNAME_ENG + " " + u.FNAME_ENG;
                txtPosition.Text = u.POSN_ENAME;
                txtDept.Text = u.DEPT_ABB_NAME;
                txtDeptCode.Text = u.DEPT_CODE;

                hdtxtBand.Value = u.BAND;
                hdtxtEmail.Value = u.EMAIL;

                txtEmpId.Style["background-color"] = "#d4edda";
                ViewState["REQ_SOURCE"] = "HRMS";       // ✅ ข้อมูลมาจาก HRMS → บันทึก Snapshot ได้
                return true;
            }
        }

        // *** Develop Check Column
        private Dictionary<string, object> GetDataHRMS_CSharp_check(string userId)
        {
            // ✅ กันก่อนส่ง
            if (!IsValidEmpNo(userId))
                return null;

            string url = "http://cptsvs547:1000/middleware/oracle/hrms";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            string sql = "select * from ADMIN.v_emp_data_all_cpt where UPPER(emp_no)=UPPER('" + userId + "')";

            byte[] data = Encoding.UTF8.GetBytes("command=" + HttpUtility.UrlEncode(sql));

            using (var stream = request.GetRequestStream())
                stream.Write(data, 0, data.Length);

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                string json = reader.ReadToEnd();
                var result = Newtonsoft.Json.Linq.JObject.Parse(json);

                var rows = result["data"] as Newtonsoft.Json.Linq.JArray;
                if (rows == null || rows.Count == 0)
                    return null;

                var u = (Newtonsoft.Json.Linq.JObject)rows[0];

                // ✅ วน property ทุกตัวออกมาเป็น Dictionary
                var dict = new Dictionary<string, object>();
                foreach (var prop in u.Properties())
                {
                    dict[prop.Name] = prop.Value != null && prop.Value.Type != Newtonsoft.Json.Linq.JTokenType.Null
                        ? prop.Value.ToString()
                        : "";
                }
                return dict;
            }
        }

        private void GetDataUser_check()
        {
            var empData = GetDataHRMS_CSharp_check(txtEmpId.Text.Trim());

            if (empData != null)
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(empData);
                ScriptManager.RegisterStartupScript(
                    UP1, UP1.GetType(), "showModal",
                    "showModal(" + json + ");", true
                );
            }
        }
        private void ClearEmployeeUI()
        {
            txtEmpName.Text = "";
            txtPosition.Text = "";
            txtDept.Text = "";
            hdtxtBand.Value = "";
            hdtxtEmail.Value = "";
        }
        private void ShowEmployeeUI()
        {
            txtEmpName.Text = ViewState["EMP_NAME"].ToString();
            txtPosition.Text = ViewState["EMP_POSITION"].ToString();
            txtDept.Text = ViewState["EMP_DEPT"].ToString();
            hdtxtBand.Value = ViewState["EMP_BAND"].ToString();
            hdtxtEmail.Value = ViewState["EMP_EMAIL"].ToString();
        }

        // ============================================================
        // เรียกข้อมูลถ้ามีข้อมูลในระบบแล้ว Withdraw Control List
        // ============================================================
        protected string status_show(string Doc_No)
        {
            string str_status =
                " SELECT A.status_id, B.status_name, B.Style_CSS" +
                " FROM Withdraw_Control_List A" +
                " LEFT JOIN Master_Status B ON A.status_id = B.status_id" +
                " WHERE Doc_No = @Doc_No";

            dc.QryDT_P(conn, str_status,
                new SqlParameter("@Doc_No", SqlDbType.NVarChar) { Value = Doc_No });

            // ✅ กัน crash ถ้าไม่พบเอกสาร
            if (dc.dt.Rows.Count == 0)
            {
                litStatus.Text = "<span class='status-badge status-cancel'>ไม่พบเอกสาร</span>";
                if (CancelButton != null) CancelButton.Visible = false;
                return "";
            }

            string status_id = dc.dt.Rows[0]["status_id"].ToString();
            string statusName = dc.dt.Rows[0]["status_name"].ToString();
            string styleCss = dc.dt.Rows[0]["Style_CSS"].ToString();

            litStatus.Text = "<span class='status-badge " + styleCss + "'>" + statusName + "</span>";

            return status_id;
        }
        private void LoadRequester(string Doc_No)
        {
            string str_emp = "select * from Withdraw_Control_List where Doc_No = @Doc_No";
            dc.QryDT_P(conn, str_emp,
                new SqlParameter("@Doc_No", SqlDbType.NVarChar) { Value = Doc_No });

            if (dc.dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูลผู้ขอ", "Error", "error", "5000");
                return;
            }

            string emp_id = dc.dt.Rows[0]["issue_empid"].ToString();
            DateTime issue_date = Convert.ToDateTime(dc.dt.Rows[0]["issue_date"]);
            string reason_id = dc.dt.Rows[0]["reason_id"].ToString();
            string comment_text = dc.dt.Rows[0]["reason_remark"].ToString();

            string phone = dc.dt.Rows[0]["phone"] != DBNull.Value
                   ? dc.dt.Rows[0]["phone"].ToString()
                   : "";

            // ══ 1) HRMS ก่อน ══
            bool fromHrms = false;
            try { fromHrms = GetDataHRMS_CSharp(emp_id.Trim()); }
            catch (Exception exHrms) { System.Diagnostics.Debug.WriteLine("[HRMS] " + exHrms.Message); }

            if (fromHrms)
            {
                SaveRequesterInfo_IfMissing(Doc_No, phone);   // เอกสารเก่าที่ยังไม่มี Snapshot → เก็บไว้เลย
            }
            else
            {
                // ══ 2) ไม่เจอใน HRMS (ลาออก / ย้าย) → อ่านจาก Requester_Info ══
                DateTime? savedAt;
                if (LoadRequesterSnapshot(Doc_No, out savedAt))
                    alert_toast("ไม่พบรหัส " + emp_id.Trim() + " ใน HRMS — แสดงข้อมูลผู้ร้องขอที่บันทึกไว้" +
                                (savedAt.HasValue ? " ณ " + savedAt.Value.ToString("dd-MMM-yyyy HH:mm") : ""),
                                "ข้อมูลผู้ร้องขอ (Snapshot)", "info", "8000");
                else
                    alert_toast("ไม่พบรหัส " + emp_id.Trim() + " ใน HRMS และไม่มีข้อมูลที่บันทึกไว้",
                                "ไม่พบข้อมูลผู้ร้องขอ", "warn", "8000");
            }
            txtEmpId.Text = emp_id;
            txtTel.Text = phone;
            txtIssueDate.Text = issue_date.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            txtEmpId.ReadOnly = true;

            SaveEmployeeToViewState();

            ddlReason_Borrow.SelectedValue = reason_id;
            txt_borrowComment.Text = comment_text;
        }
        private void LoadBorrowList(string Doc_No)
        {
            DataTable dt = BorrowListDT;
            string str_borrowlist =
                " Select A.No , A.Doc_No , A.tool_id , A.sub_id , A.system_id , A.FixAsset , A.ControlNo , A.Use_Dept " +
                " , A.Use_Model_Id , A.New_Model ,A.HD_Model " +
                " , A.Use_Cell_Id , A.New_Cell , A.HD_Cell" +
                " , A.Use_Station_Id , A.New_Station , A.HD_Station" +
                " ,B.tool_no , B.dept_code , B.model_name , B.model_id" +
                " ,B.location1_id , B.location , B.location2_id , B.cell , B.station_id , B.station , A.status_id" +
                " from Withdraw_List A" +
                " left join Tool_Equipment_List_View B on A.tool_id = B.tool_id and A.sub_id = B.sub_no and A.system_id = B.sys_status" +
                " Where Doc_No = @Doc_No" +
                " Order by A.No ";

            dc.QryDT_P(conn, str_borrowlist,
                new SqlParameter("@Doc_No", SqlDbType.NVarChar) { Value = Doc_No });

            DataTable list = dc.dt;

            foreach (DataRow r in list.Rows)
            {
                DataRow newRow = dt.NewRow();

                newRow["RowNo"] = r["No"];
                newRow["ToolId"] = r["tool_id"];
                newRow["ToolSub"] = r["sub_id"];
                newRow["SystemId"] = r["system_id"];
                newRow["FixAssetNo"] = r["FixAsset"];
                newRow["ControlNo"] = r["ControlNo"];
                newRow["ToolNo"] = r["tool_no"];
                newRow["DeptCode"] = r["dept_code"];
                newRow["ModelName"] = r["model_name"];
                newRow["ModelId"] = r["model_id"];
                newRow["CellName"] = r["cell"];
                newRow["CellId"] = r["location2_id"];
                newRow["StationName"] = r["station"];
                newRow["StationId"] = r["station_id"];

                // ✅ Use values
                newRow["Use_DeptCode"] = r["Use_Dept"];
                newRow["Use_ModelId"] = r["Use_Model_Id"];
                newRow["Use_CellId"] = r["Use_Cell_Id"];
                newRow["Use_StationId"] = r["Use_Station_Id"];

                // ✅ New values
                newRow["Add_Model"] = r["New_Model"];
                newRow["Add_Cell"] = r["New_Cell"];
                newRow["Add_Station"] = r["New_Station"];

                // ✅ Hidden values
                newRow["HD_Model"] = r["HD_Model"];
                newRow["HD_Cell"] = r["HD_Cell"];
                newRow["HD_Station"] = r["HD_Station"];

                // ✅ Status
                newRow["StatusId"] = r["status_id"];

                //Compare Data
                newRow["CompareData"] = "";

                dt.Rows.Add(newRow);
            }

            BorrowListDT = dt;
            BindBorrowList();
        }
        protected void Bind_History_Reject(string Doc_No)
        {
            string str =
                " select a.Reject_Count, a.emp_id + ' ' + A.name as Reject_by , A.Doc_no , A.Status_id , C.status_name , A.Reject_Date , A.Reject_detail , A.Type" +
                " from Reject_Detail_All A" +
                " left join user_master.dbo.sys_user_master B on A.emp_id = B.emp_id" +
                " left join Master_Status C on A.status_id = C.status_id" +
                " Where Doc_No = @Doc_No" +
                " Order by Reject_Count desc";

            dc.QryDT_P(conn, str,
                new SqlParameter("@Doc_No", SqlDbType.NVarChar) { Value = Doc_No });

            gv_rejectHistory.DataSource = dc.dt;
            gv_rejectHistory.DataBind();
        }

        
        #region --- Button Function ---

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFixAsset.Text))
            {
                alert_toast("กรุณากรอกข้อมูลที่ต้องการค้นหา", "แจ้งตือน", "warn", "5000");
                return;
            }

            string dept_check = txtFixAsset.Text.Trim();
            string where_chkbox = chkSpare.Checked ? " AND status_id > 1 " : "";          

            // keyword เป็น static string ทั้งหมด — ค่าจริงไปทาง parameter
            string keyword =
                  "FixAcces   LIKE '%' + @kw + '%'" +
                " OR control_no LIKE '%' + @kw + '%'" +
                " OR tool_no    LIKE '%' + @kw + '%'";

            if (dept_check.Length == 4)
                keyword += " OR dept_code = @dept";

            string str =
                "SELECT * FROM Tool_Equipment_List_View" +
                " WHERE (" + keyword + ")" + where_chkbox + " AND status_id Not IN(11,12) " +
                " ORDER BY tool_no";

            var prms = new System.Collections.Generic.List<SqlParameter>
            {
                new SqlParameter("@kw", SqlDbType.NVarChar) { Value = dept_check }
            };
            if (dept_check.Length == 4)
                prms.Add(new SqlParameter("@dept", SqlDbType.NVarChar) { Value = dept_check });

            dc.QryDT_P(conn, str, prms.ToArray());
            DataTable dt = dc.dt;

            if (!dt.Columns.Contains("RowNo"))
            {
                dt.Columns.Add("RowNo", typeof(int));
                dt.Columns.Add("DisplayStatus", typeof(string));
            }

            // ── ดึงรายการที่กำลังยืมอยู่ (status ≠ 1, 100, 999) ──
            var borrowingSet = new System.Collections.Generic.HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            string strBorrow =
                "SELECT DISTINCT tool_id, sub_id, system_id" +
                " FROM Borrow_List" +
                " WHERE status_id NOT IN (1,100,999)";

            dc.QryDT_P(conn, strBorrow);

            foreach (DataRow br in dc.dt.Rows)
            {
                string key = br["tool_id"] + "|" + br["sub_id"] + "|" + br["system_id"];
                borrowingSet.Add(key);
            }

            int rowNo = 1;
            foreach (DataRow row in dt.Rows)
            {
                row["RowNo"] = rowNo++;

                string checkKey = row["tool_id"] + "|" + row["sub_no"] + "|" + row["sys_status"];

                if (borrowingSet.Contains(checkKey))
                {
                    row["DisplayStatus"] = "อยู่ระหว่างยืม";
                }
                else
                {
                    int statusId = Convert.ToInt32(row["status_id"]);
                    row["DisplayStatus"] = (statusId == 1 || statusId == 2)
                        ? row["status"].ToString()
                        : "Spare : " + row["status"];
                }
            }

            ViewState["SearchResult"] = dt;

            gvSearchResult.DataSource = dt;
            gvSearchResult.DataBind();
        }
        protected void btnDraft_Click(object sender, EventArgs e)
        {
            string empId = txtEmpId.Text.Trim();
            string empName = txtEmpName.Text.Trim();
            DataTable dt = BorrowListDT;
            string Doc_No = litIssueNo.Text.Trim();
            string status_id = "1";  //Wait Issue

            // ✅ ตรวจสอบ Employee ID ต้องเป็นตัวเลข 6 หลัก
            if (!System.Text.RegularExpressions.Regex.IsMatch(empId, @"^\d{6}$"))
            {
                alert_toast(
                    "Employee ID ต้องเป็นตัวเลข 6 หลัก", "alert-info", "warn", "5000"
                );
                txtEmpId.Focus();
                return;
            }

            // ✅ ตรวจสอบเบอร์ติดต่อ
            if (!ValidateTel()) return;

            // ✅ ตรวจสอบ Dept Code กรณี Log-In ด้วย Dept Code
            if (pnlNewEmp.Visible == true)
            {
                alert_toast("กรุณาตรวจสอบ Comment ใน Step 1", "alert-info", "warn", "5000");
                return;
            }

            // ✅ ตรวจสอบ Employee ID , Employee Name
            if (string.IsNullOrEmpty(empId) || string.IsNullOrEmpty(empName))
            {
                alert_toast("กรุณาตรวจสอบ Employee ID (ไม่มี Employee ID นี้ในระบบ) ", "alert-info", "warn", "5000");
                return;
            }

            // ✅ ตรวจสอบ Table BorrowListDT ต้องมีข้อมูล
            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูลในรายการ Borrow List", "alert-info", "warn", "5000");
                return;
            }

            // -- Accept to Save Data
            saveData(status_id);
        }
        protected void btnSend_Click(object sender, EventArgs e)
        {
            string empId = txtEmpId.Text.Trim();
            string empName = txtEmpName.Text.Trim();
            DataTable dt = BorrowListDT;
            string Doc_No = litIssueNo.Text.Trim();
            string status_id = "50";  //AAD Confirm

            #region --- ตรวจสอบการลงข้อมุล ---
            // ✅ ตรวจสอบ Employee ID ต้องเป็นตัวเลข 6 หลัก
            if (!System.Text.RegularExpressions.Regex.IsMatch(empId, @"^\d{6}$"))
            {
                alert_toast(
                    "Employee ID ต้องเป็นตัวเลข 6 หลัก", "alert-info", "warn", "5000"
                );
                txtEmpId.Focus();
                return;
            }

            // ✅ ตรวจสอบเบอร์ติดต่อ
            if (!ValidateTel()) return;

            // ✅ ตรวจสอบ Dept Code กรณี Log-In ด้วย Dept Code
            if (pnlNewEmp.Visible == true)
            {
                alert_toast("กรุณาตรวจสอบ Comment ใน Step 1", "alert-info", "warn", "5000");
                return;
            }

            // ✅ ตรวจสอบ Employee ID , Employee Name
            if (string.IsNullOrEmpty(empId) || string.IsNullOrEmpty(empName))
            {
                alert_toast("กรุณาตรวจสอบ Employee ID (ไม่มี Employee ID นี้ในระบบ) ", "alert-info", "warn", "5000");
                return;
            }

            // ✅ ตรวจสอบ Table BorrowListDT ต้องมีข้อมูล
            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูลในรายการ Request List", "alert-info", "warn", "5000");
                return;
            }

            // Check Null , Not Selection Data
            Save_BorrowListDT(status_id); // Save ก่อน ป้องกัน Mode/Text หาย
            Boolean checkblank = Check_blankData();
            if (checkblank)
            {
                alert_toast("กรุณาลงข้อมูลให้ครบถ้วน (ช่องสีเหลือง)", "ข้อมูลไม่ครบ", "warn", "5000");
                return;
            }

            // ✅ Check Withdraw Reason
            if (ddlReason_Borrow == null || string.IsNullOrEmpty(ddlReason_Borrow.SelectedValue) || ddlReason_Borrow.SelectedValue == "0")
            {
                bool blank = ddlReason_Borrow == null || string.IsNullOrEmpty(ddlReason_Borrow.SelectedValue) || ddlReason_Borrow.SelectedValue == "0";

                SetWarning_DDL(ddlReason_Borrow, blank);

                alert_toast("กรุณาเลือกเหตุผลการแจ้งเปลี่ยนแปลง", "alert-info", "warn", "3000");
                return;
            }

            // ✅ ตรวจ Comment ตาม reason
            if (!ValidateBorrowComment()) return;

            #endregion

            // ตรวจสอบว่าในข้อมูลมี รายการที่ "กำลังยืมอยู่หรือไม่"
            if (duringBorrowCheck())
            {
                BindBorrowList();
                alert_toast("พบรายการอยู่ระหว่างการยืม : ไม่สามารถเบิกได้", "พบรายการอยู่่ระหว่างการยืม", "warn", "5000");
            }
            else
            {
                // -- Accept to Save Data
                saveData(status_id);
            }
        }
        /// <summary>AAD Confirm (Status 50) : เปิด Modal ยืนยัน Status Use / ผู้รับผิดชอบ</summary>
        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!IsAadEditable())
            {
                alert_toast("AAD Confirm ได้เฉพาะเอกสารสถานะรอ AAD Confirm (โหมด Confirm)", "ไม่สามารถยืนยันได้", "warn", "10000");
                return;
            }

            ViewState["CFM_FLOW"] = "status50";
            BuildConfirmSummary();
            ReopenConfirmModal();
        }

        /// <summary>Admin สร้างเอกสารแล้ว AAD Confirm ทันที (ConfirmButton2 → status 99)</summary>
        private bool IsDirectConfirmAllowed()
        {
            string admin = Session["admin"] != null ? Session["admin"].ToString().Trim() : "";
            return admin == "99";
        }

        /// <summary>ยืนยันจาก Modal — ตรวจ Dropdown / บันทึก Status Use + ผู้รับผิดชอบ (เดิม/ใหม่) รายแถว + Header 99</summary>
        protected void btnConfirmModalOk_Click(object sender, EventArgs e)
        {
            string flow = Convert.ToString(ViewState["CFM_FLOW"]);

            // ════ ทางที่ 1 : Admin สร้างเอกสาร + AAD Confirm ทันที (ConfirmButton2) ════
            if (flow == "direct")
            {
                if (!IsDirectConfirmAllowed())
                {
                    alert_toast("ไม่มีสิทธิ์ AAD Confirm", "Permission Denied", "error", "5000");
                    return;
                }
                if (!ReadAndValidateConfirmModal())
                {
                    ReopenConfirmModal();
                    return;
                }

                // INSERT เอกสาร status 99 — InsertWithdrawRows_Tx เก็บ Old/New Status + ผู้รับผิดชอบ
                saveData("99");
                return;
            }

            // ════ ทางที่ 2 : Status 50 → AAD Confirm (ConfirmButton) ════
            if (!IsAadEditable())
            {
                alert_toast("AAD Confirm ได้เฉพาะเอกสารสถานะรอ AAD Confirm (โหมด Confirm)", "ไม่สามารถยืนยันได้", "warn", "10000");
                return;
            }

            // ── อ่าน + ตรวจ Dropdown ใน Modal (ไม่ครบ → เปิด Modal ค้างไว้ พร้อมขอบแดง) ──
            if (!ReadAndValidateConfirmModal())
            {
                ReopenConfirmModal();
                return;
            }

            string Doc_No = litIssueNo.Text.Trim();
            string empId = Session["emp_id"] != null ? Session["emp_id"].ToString().Trim() : "";
            DateTime now = DateTime.Now;
            bool ok = false;

            conn.Open();
            SqlTransaction tx = conn.BeginTransaction();
            try
            {
                // ── Header → 99 : Wait Auto Update Location (เฉพาะเอกสารที่ยังเป็น 50 กันกดซ้ำ) ──
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Withdraw_Control_List" +
                    " SET status_id = 99 , update_date = @dt , confirm_empid = @emp , confirm_date = @dt" +
                    " WHERE Doc_No = @Doc_No AND status_id = 50", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@dt", now);
                    cmd.Parameters.AddWithValue("@emp", empId);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException("สถานะเอกสารเปลี่ยนไปแล้ว (ไม่ใช่ 50) กรุณารีเฟรชหน้า");
                }

                // ── รายการ → 99 + Status / ผู้รับผิดชอบ : เดิม (snapshot จากอุปกรณ์ตอนนี้) / ใหม่ (ที่ AAD เลือก) ──
                foreach (DataRow r in BorrowListDT.Rows)
                {
                    if (GetVal(r, "CompareData") == "Delete") continue;

                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE W SET W.status_id = 99 , W.update_date = @dt ," +
                        "       W.Old_Status_Id = V.status_id , W.New_Status_Id = @st ," +
                        "       W.Old_Responsible = CAST(V.rsp_empID AS nvarchar(50)) , W.New_Responsible = @rp" +
                        " FROM Withdraw_List W" +
                        " LEFT JOIN Tool_Equipment_List_View V" +
                        "   ON W.tool_id = V.tool_id AND W.sub_id = V.sub_no AND W.system_id = V.sys_status" +
                        " WHERE W.Doc_No = @doc AND W.tool_id = @tid AND W.sub_id = @sid AND W.system_id = @sys", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@dt", now);
                        cmd.Parameters.AddWithValue("@st", ToDbVal(r, "Use_StatusId"));
                        cmd.Parameters.AddWithValue("@rp", ToDbVal(r, "Resp_EmpId"));
                        cmd.Parameters.AddWithValue("@doc", Doc_No);
                        cmd.Parameters.AddWithValue("@tid", ToDbVal(r, "ToolId"));
                        cmd.Parameters.AddWithValue("@sid", ToDbVal(r, "ToolSub"));
                        cmd.Parameters.AddWithValue("@sys", ToDbVal(r, "SystemId"));
                        cmd.ExecuteNonQuery();
                    }
                }

                // ── กันตกหล่น : รายการอื่นในเอกสาร → 99 (เหมือนพฤติกรรมเดิม) ──
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Withdraw_List SET status_id = 99 WHERE Doc_No = @Doc_No AND ISNULL(status_id, 0) <> 99", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.ExecuteNonQuery();
                }

                // ── ประวัติเปลี่ยน Location (History_Change_Location) : Current → Use ของ Request List ──
                InsertLocationHistory_Tx(conn, tx, BorrowListDT, Doc_No, empId);

                tx.Commit();
                ok = true;
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                alert_toast("AAD Confirm ไม่สำเร็จ : " + ex.Message, "Error", "error", "10000");
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close();
            }

            if (!ok) return;

            string jsDoc = HttpUtility.JavaScriptStringEncode(Doc_No);
            string script =
                "var m=document.getElementById('" + pnlConfirmModal.ClientID + "'); if(m) m.style.display='none';" +
                "document.getElementById('loadingOverlay').style.display = 'block';" +
                "if (typeof showToast === 'function') {" +
                "  showToast('Doc No : " + jsDoc + "', { title: 'AAD Confirm เรียบร้อย', type: 'success', duration: 2000 });" +
                "}" +
                "setTimeout(function () { window.location.href = 'ApproveStatus.aspx'; }, 2000);";

            ScriptManager.RegisterStartupScript(this, this.GetType(), "redirectScript", script, true);
        }
        protected void btnConfirm2_Click(object sender, EventArgs e)
        {
            string empId = txtEmpId.Text.Trim();
            string empName = txtEmpName.Text.Trim();
            DataTable dt = BorrowListDT;
            string Doc_No = litIssueNo.Text.Trim();
            string status_id = "99";  //Wait update ***Auto Update Location***

            #region --- ตรวจสอบการลงข้อมุล ---
            // ✅ ตรวจสอบ Employee ID ต้องเป็นตัวเลข 6 หลัก
            if (!System.Text.RegularExpressions.Regex.IsMatch(empId, @"^\d{6}$"))
            {
                alert_toast(
                    "Employee ID ต้องเป็นตัวเลข 6 หลัก", "alert-info", "warn", "5000"
                );
                txtEmpId.Focus();
                return;
            }

            // ✅ ตรวจสอบเบอร์ติดต่อ
            if (!ValidateTel()) return;

            // ✅ ตรวจสอบ Dept Code กรณี Log-In ด้วย Dept Code
            if (pnlNewEmp.Visible == true)
            {
                alert_toast("กรุณาตรวจสอบ Comment ใน Step 1", "alert-info", "warn", "5000");
                return;
            }

            // ✅ ตรวจสอบ Employee ID , Employee Name
            if (string.IsNullOrEmpty(empId) || string.IsNullOrEmpty(empName))
            {
                alert_toast("กรุณาตรวจสอบ Employee ID (ไม่มี Employee ID นี้ในระบบ) ", "alert-info", "warn", "5000");
                return;
            }

            // ✅ ตรวจสอบ Table BorrowListDT ต้องมีข้อมูล
            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูลในรายการ Request List", "alert-info", "warn", "5000");
                return;
            }

            // Check Null , Not Selection Data
            Save_BorrowListDT(status_id); // Save ก่อน ป้องกัน Mode/Text หาย
            Boolean checkblank = Check_blankData();
            if (checkblank)
            {
                alert_toast("กรุณาลงข้อมูลให้ครบถ้วน (ช่องสีเหลือง)", "ข้อมูลไม่ครบ", "warn", "5000");
                return;
            }

            // ✅ Check Withdraw Reason
            if (ddlReason_Borrow == null || string.IsNullOrEmpty(ddlReason_Borrow.SelectedValue) || ddlReason_Borrow.SelectedValue == "0")
            {
                bool blank = ddlReason_Borrow == null || string.IsNullOrEmpty(ddlReason_Borrow.SelectedValue) || ddlReason_Borrow.SelectedValue == "0";

                SetWarning_DDL(ddlReason_Borrow, blank);

                alert_toast("กรุณาเลือกเหตุผลการแจ้งเปลี่ยนแปลง", "alert-info", "warn", "3000");
                return;
            }
            #endregion

            // -- ผ่านการตรวจ → เปิด Modal ยืนยัน Status Use / ผู้รับผิดชอบ (บันทึกจริงใน btnConfirmModalOk_Click)
            ViewState["CFM_FLOW"] = "direct";
            BuildConfirmSummary();
            ReopenConfirmModal();
        }
        // ══════════════════════════════════════════════════════════════
        // Modal เหตุผล : ยืนยัน Reject / Cancel (อ้างอิง Borrow2)
        //   ตรวจโหมด / สถานะ / ความยาวเหตุผล แล้วส่งต่อให้ btnReject_Click / btnCancel_Click เดิม
        // ══════════════════════════════════════════════════════════════
        protected void btnRejectModalConfirm_Click(object sender, EventArgs e)
        {
            string mode = hfRejectMode.Value;
            string comment = (txt_rejectComment.Text ?? "").Replace("\r\n", "\n").Trim();
            string status = Convert.ToString(ViewState["STATUS_ID"]).Trim();
            string pageMode = (Request.QueryString["mode"] ?? "edit").ToLower();

            if (mode != "reject" && mode != "cancel")
            {
                alert_toast("ไม่พบโหมดการทำงาน กรุณาลองใหม่", "Error", "error", "5000");
                return;
            }

            // ── สถานะที่ปิดไปแล้ว (ปุ่มไม่ควรแสดง) ──
            if (status == "99" || status == "100" || status == "999")
            {
                alert_toast("เอกสารนี้ดำเนินการเสร็จสิ้น/ยกเลิกแล้ว ไม่สามารถ " + (mode == "reject" ? "Reject" : "Cancel") + " ได้",
                            "ไม่สามารถดำเนินการได้", "warn", "8000");
                return;
            }
            if (mode == "reject" && pageMode != "confirm" && pageMode != "approve")
            {
                alert_toast("Reject ได้เฉพาะโหมด Approve / Confirm", "ไม่สามารถ Reject ได้", "warn", "8000");
                return;
            }

            // ── เหตุผล ──
            if (comment.Length < 10 || comment.Length > 200)
            {
                alert_toast(comment.Length > 200
                                ? "เหตุผลต้องไม่เกิน 200 ตัวอักษร (ปัจจุบัน " + comment.Length + ")"
                                : "กรุณาระบุเหตุผลอย่างน้อย 10 ตัวอักษร",
                            "ข้อมูลไม่ครบ", "warn", "5000");
                ReopenRejectModal(mode);
                return;
            }
            txt_rejectComment.Text = comment;

            if (mode == "reject") btnReject_Click(sender, e);
            else btnCancel_Click(sender, e);
        }

        /// <summary>เปิด Modal ซ้ำหลัง Postback (คงค่าที่พิมพ์ไว้)</summary>
        private void ReopenRejectModal(string mode)
        {
            string js = "setRejectModalMode('" + HttpUtility.JavaScriptStringEncode(mode) + "');" +
                        "var t=document.getElementById('" + txt_rejectComment.ClientID + "'); if(t) t.classList.add('rjm-err');" +
                        "showRejectModal();";
            ScriptManager.RegisterStartupScript(this, GetType(), "reopenReject", js, true);
        }

        /// <summary>Section 5 : แสดงเฉพาะเมื่อมีประวัติ Reject / Cancel (เรียกหลัง Bind_History_Reject)</summary>
        private void ShowRejectHistoryIfAny()
        {
            pnl_rejectComment.Visible = gv_rejectHistory.Rows.Count > 0;
        }

        protected void btnReject_Click(object sender, EventArgs e)
        {
            string Doc_No = litIssueNo.Text.Trim();
            string comment = txt_rejectComment.Text.Trim();
            string mode = ViewState["MODE"].ToString();
            string empId = "";
            string empName = "";
            string link_path = "EquipmentStatus2.aspx";

            // ✅ อ่าน mode
            if (!string.IsNullOrEmpty(Request.QueryString["mode"]))
            {
                mode = Request.QueryString["mode"].ToLower();
            }

            if (mode == "confirm")
            {
                empId = Session["emp_id"].ToString();
                empName = Session["user_name"].ToString();
                link_path = "ApproveStatus.aspx";
            }
            else
            {
                empId = txtEmpId.Text.Trim();
                empName = txtEmpName.Text;
                link_path = "EquipmentStatus2.aspx";
            }

            // ✅ แยกด้วย space แล้วเอาตัวแรก
            empName = empName.Split(' ')[0];

            if (string.IsNullOrEmpty(comment) || comment.Length < 10)
            {
                alert_toast("กรุณากรอก Comment อย่างน้อย 10 ตัวอักษร", "Warning", "warn", "3000");

                ReopenRejectModal("reject");
                return;
            }

            // ✅ ผ่านแล้ว → ทำ Reject ต่อ
            //Search Status
            string status_id = ViewState["STATUS_ID"].ToString();

            //Search Max Reject Count
            int Reject_Count = GetNextRejectCount(Doc_No);

            DateTime rejectDate = DateTime.Now;

            bool rejectOk = false;
            conn.Open();

            try
            {
                // ── Update Withdraw_Control_List → status 1 ──
                #region
                string sqlHeader =
                    "UPDATE Withdraw_Control_List" +
                    " SET status_id = 1 , update_date = @update_date" +
                    " WHERE Doc_No = @Doc_No";

                using (SqlCommand cmd = new SqlCommand(sqlHeader, conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@update_date", rejectDate);
                    cmd.ExecuteNonQuery();
                }
                #endregion

                // ── Update Withdraw_List → status 1 ──
                #region
                string sqlList =
                    "UPDATE Withdraw_List" +
                    " SET status_id = 1" +
                    " WHERE Doc_No = @Doc_No";

                using (SqlCommand cmd = new SqlCommand(sqlList, conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.ExecuteNonQuery();
                }
                #endregion

                // --- Update Reject History
                #region
                string sql = @"
                    INSERT INTO Reject_Detail_All
                    (
                        Doc_No , Doc_Type , status_id , Reject_Count ,
                        emp_id , name , Reject_Date , Reject_detail , Type
                    )
                    VALUES
                    (
                        @Doc_No , @Doc_Type , @status_id , @Reject_Count ,
                        @emp_id , @name , @Reject_Date , @Reject_detail ,@Type
                    )";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@Doc_Type", "AAD3");
                    cmd.Parameters.AddWithValue("@status_id", status_id);
                    cmd.Parameters.AddWithValue("@Reject_Count", Reject_Count);
                    cmd.Parameters.AddWithValue("@emp_id", empId);
                    cmd.Parameters.AddWithValue("@name", empName);
                    cmd.Parameters.AddWithValue("@Reject_Date", rejectDate);
                    cmd.Parameters.AddWithValue("@Reject_detail", comment);
                    cmd.Parameters.AddWithValue("@Type", "Reject");

                    cmd.ExecuteNonQuery();
                }
                #endregion

                rejectOk = true;
            }
            catch (Exception ex)
            {
                alert_toast("เกิดข้อผิดพลาด : " + ex.Message, "Error", "error", "10000");
            }
            finally
            {
                conn.Close(); // ✅ ปิดทุกครั้ง

                if (!rejectOk)
                {
                    ReopenRejectModal("reject");   // ✅ error → ไม่ redirect , เปิด Modal ให้ลองใหม่
                }
                else
                {
                    string alert_info = "Reject";

                    //alert_toast("Doc No : " + Doc_No, alert_info, "warn", "2000");

                    string jsDoc = HttpUtility.JavaScriptStringEncode(Doc_No);
                    string script =
                        "document.getElementById('loadingOverlay').style.display = 'block';" +
                        "if (typeof showToast === 'function') {" +
                        "  showToast('Doc No : " + jsDoc + "', { title: 'Reject Done', type: 'warn', duration: 2000 });" +
                        "}" +
                        "setTimeout(function () { window.location.href = '" + link_path + "'; }, 2000);";

                    ScriptManager.RegisterStartupScript(
                        this, this.GetType(), "redirectScript", script, true);
                }
            }
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            string Doc_No = litIssueNo.Text.Trim();
            string comment = txt_rejectComment.Text.Trim();
            string mode = ViewState["MODE"].ToString();
            string empId = "";
            string empName = "";
            string link_path = "EquipmentStatus2.aspx";

            // ✅ อ่าน mode
            if (!string.IsNullOrEmpty(Request.QueryString["mode"]))
            {
                mode = Request.QueryString["mode"].ToLower();
            }

            if (mode == "confirm")
            {
                empId = Session["emp_id"].ToString();
                empName = Session["user_name"].ToString();
                link_path = "ApproveStatus.aspx";
            }
            else
            {
                empId = txtEmpId.Text.Trim();
                empName = txtEmpName.Text;
                link_path = "EquipmentStatus2.aspx";
            }

            // ✅ แยกด้วย space แล้วเอาตัวแรก
            empName = empName.Split(' ')[0];

            // ✅ ต้องมี Doc_No ถึงจะ Cancel ได้
            if (Doc_No == "-" || string.IsNullOrEmpty(Doc_No))
            {
                alert_toast("ไม่พบเลขเอกสาร ไม่สามารถยกเลิกได้", "Cancel Error", "error", "5000");
                return;
            }

            // ✅ ต้องมี Comment ถึงจะ Cancel ได้
            if (string.IsNullOrEmpty(comment) || comment.Length < 10)
            {
                alert_toast("กรุณากรอก Comment อย่างน้อย 10 ตัวอักษร", "Warning", "warn", "3000");

                ReopenRejectModal("cancel");
                return;
            }

            conn.Open();
            try
            {
                // ── Update Withdraw_Control_List → status 999 ──
                #region
                string sqlHeader =
                    "UPDATE Withdraw_Control_List" +
                    " SET status_id = 999 , update_date = @update_date , cancel_empid = @cancel_empid , cancel_date = @cancel_date " +
                    " WHERE Doc_No = @Doc_No";

                using (SqlCommand cmd = new SqlCommand(sqlHeader, conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@cancel_empid", empId);
                    cmd.Parameters.AddWithValue("@cancel_date", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
                #endregion

                // ── Update Withdraw_List → status 999 ──
                #region
                string sqlList =
                    "UPDATE Withdraw_List" +
                    " SET status_id = 999" +
                    " WHERE Doc_No = @Doc_No";

                using (SqlCommand cmd = new SqlCommand(sqlList, conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.ExecuteNonQuery();
                }
                #endregion

                // --- Search Max Reject Count
                int Reject_Count = GetNextRejectCount(Doc_No);

                // --- Update Cancel History
                DateTime rejectDate = DateTime.Now;
                string status_id = ViewState["STATUS_ID"].ToString().Trim();

                string sqlCancel = @"
                    INSERT INTO Reject_Detail_All
                    (
                        Doc_No , Doc_Type , status_id , Reject_Count ,
                        emp_id , name , Reject_Date , Reject_detail , Type
                    )
                    VALUES
                    (
                        @Doc_No , @Doc_Type , @status_id , @Reject_Count ,
                        @emp_id , @name , @Reject_Date , @Reject_detail , @Type
                    )";

                using (SqlCommand cmd = new SqlCommand(sqlCancel, conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@Doc_Type", "AAD3");
                    cmd.Parameters.AddWithValue("@status_id", status_id);
                    cmd.Parameters.AddWithValue("@Reject_Count", Reject_Count);
                    cmd.Parameters.AddWithValue("@emp_id", empId);
                    cmd.Parameters.AddWithValue("@name", empName);
                    cmd.Parameters.AddWithValue("@Reject_Date", rejectDate);
                    cmd.Parameters.AddWithValue("@Reject_detail", comment);
                    cmd.Parameters.AddWithValue("@Type", "Cancel");
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                alert_toast("Cancel Error : " + ex.Message, "Error", "error", "10000");
                ReopenRejectModal("cancel");
                return;
            }
            finally
            {
                conn.Close();
            }

            // ── Redirect หลัง Cancel สำเร็จ ──
            string jsLink = HttpUtility.JavaScriptStringEncode(link_path);
            string script =
                "document.getElementById('loadingOverlay').style.display = 'block';" +
                "setTimeout(function() { window.location.href = '" + jsLink + "'; }, 2000);";

            ScriptManager.RegisterStartupScript(
                this, this.GetType(), "cancelScript", script, true);

            alert_toast("Doc No : " + Doc_No + " — ยกเลิกเรียบร้อยแล้ว",
                        "Cancel Complete", "info", "2000");
        }
        protected void btn_pic_Click(object sender, EventArgs e)
        {
            GridViewRow row = (sender as LinkButton).NamingContainer as GridViewRow;
            HiddenField hdtool_id = (HiddenField)row.FindControl("hdtoolId");
            HiddenField hdtool_sub = (HiddenField)row.FindControl("hdtoolSub");
            HiddenField hd_sys_status = (HiddenField)row.FindControl("hdsystemId");

            if (hdtool_id == null || hdtool_sub == null || hd_sys_status == null)
            {
                alert_toast("ข้อมูลในตารางผิดพลาด", "ไม่สามารถเปิดรูปได้", "error", "5000");
                return;
            }

            string tool_id = hdtool_id.Value;
            string tool_sub = hdtool_sub.Value;
            string sys_status = hd_sys_status.Value;

            Get_Folder_Structure_Picture Folder_Structure_Picture = new Get_Folder_Structure_Picture(tool_id, tool_sub, sys_status);
            string get_path_toolSub = Folder_Structure_Picture.get_folder_root();
            string path_picture_toolMain = Folder_Structure_Picture.get_folder_root_view_toolMain();

            string[] files_main = Directory.GetFiles(get_path_toolSub, "*.jpg", SearchOption.TopDirectoryOnly);

            if (files_main.Length > 0)
            {
                string fullPathToImage = Path.Combine(get_path_toolSub, files_main[0]);

                try
                {
                    using (System.Drawing.Image img = System.Drawing.Image.FromFile(fullPathToImage))
                    {
                        int imageWidth = img.Width;
                        int imageHeight = img.Height;
                        int maxWidth = 500;
                        int maxHeight = 500;
                        float keep_test;

                        imgMain.ImageUrl = path_picture_toolMain;

                        imgMain.Width = imageWidth;
                        imgMain.Height = imageHeight;

                        if (imageWidth > maxWidth && imageHeight <= maxHeight)
                        {
                            keep_test = imageHeight - (((imageWidth - maxWidth) / imageWidth) * imageHeight);
                            imgMain.Height = Convert.ToInt32(keep_test);
                            imgMain.Width = maxWidth;
                        }
                        else if (imageWidth <= maxWidth && imageHeight > maxHeight)
                        {
                            keep_test = imageWidth - (((imageHeight - maxHeight) / imageHeight) * imageWidth);
                            imgMain.Width = Convert.ToInt32(keep_test);
                            imgMain.Height = maxHeight;
                        }
                        else if (imageWidth > maxWidth && imageHeight > maxHeight)
                        {
                            if (imageWidth > imageHeight)
                            {
                                keep_test = imageHeight - (((imageWidth - maxWidth) / imageWidth) * imageHeight);
                                imgMain.Height = Convert.ToInt32(keep_test);
                                imgMain.Width = maxWidth;
                                if (imageHeight > maxHeight)
                                {
                                    keep_test = imageWidth - (((imageHeight - maxHeight) / imageHeight) * imageWidth);
                                    imgMain.Width = Convert.ToInt32(keep_test);
                                    imgMain.Height = maxHeight;
                                }
                            }
                            else if (imageWidth <= imageHeight)
                            {
                                keep_test = imageWidth - (((imageHeight - maxHeight) / imageHeight) * imageWidth);
                                imgMain.Width = Convert.ToInt32(keep_test);
                                imgMain.Height = maxHeight;
                                if (imageWidth > maxWidth)
                                {
                                    keep_test = imageHeight - (((imageWidth - maxWidth) / imageWidth) * imageHeight);
                                    imgMain.Height = Convert.ToInt32(keep_test);
                                    imgMain.Width = maxWidth;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    imgMain.Height = 500;
                    imgMain.Width = 500;
                    imgMain.ImageUrl = "images/FilesType/jpeg.png";
                }
            }
            else
            {
                imgMain.Height = 500;
                imgMain.Width = 500;
                imgMain.ImageUrl = "images/FilesType/jpeg.png";
            }

            ScriptManager.RegisterStartupScript(
                this, GetType(),
                "showModal",
                "$('#modal_UploadPictureMain').modal('show');",
                true
            );
        }
        protected void btn_detailView_Click(object sender, EventArgs e)
        {
            GridViewRow row = (sender as LinkButton).NamingContainer as GridViewRow;
            HiddenField hdtool_id = (HiddenField)row.FindControl("hdtoolId");
            HiddenField hdtool_sub = (HiddenField)row.FindControl("hdtoolSub");
            HiddenField hd_sys_status = (HiddenField)row.FindControl("hdsystemId");

            if (hdtool_id == null || hdtool_sub == null || hd_sys_status == null)
            {
                alert_toast("ข้อมูลในตารางผิดพลาด", "แจ้งตือน", "error", "5000");
                return;
            }

            string tool_id = hdtool_id.Value;
            string tool_sub = hdtool_sub.Value;
            string sys_status = hd_sys_status.Value;

            string url = "Tool_equipment_detail.aspx?tool_id=" + tool_id + "&tool_sub=" + tool_sub + "&sys_status=" + sys_status;

            sb.Append(@"<script type='text/javascript'>");
            sb.Append("window.open('" + url + "','_blank');");
            sb.Append(@"</script>");
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "closemodal", sb.ToString(), false);
        }

        protected void btnUploadManual_Click(object sender, EventArgs e)
        {
            if (Session["admin"] == null || Session["admin"].ToString().Trim() != "99")
            {
                alert_toast("ไม่มีสิทธิ์อัปโหลด", "Permission Denied", "error", "5000");
                return;
            }

            string reopen = "$('#modal_UploadManual').modal('show');";

            // ✅ whitelist ชื่อไฟล์ (กัน path traversal จาก value ที่ถูกแก้)
            string target = ddlManualFile.SelectedValue;
            if (Array.IndexOf(MANUAL_FILES, target) < 0)
            {
                alert_toast("ชื่อไฟล์คู่มือไม่ถูกต้อง", "ข้อมูลไม่ถูกต้อง", "warn", "5000");
                ScriptManager.RegisterStartupScript(this, GetType(), "reopenUpload", reopen, true);
                return;
            }

            if (!fuManual.HasFile)
            {
                alert_toast("กรุณาเลือกไฟล์ PDF", "ข้อมูลไม่ครบ", "warn", "5000");
                ScriptManager.RegisterStartupScript(this, GetType(), "reopenUpload", reopen, true);
                return;
            }

            // ✅ นามสกุล + เนื้อไฟล์ต้องขึ้นต้นด้วย %PDF
            string ext = Path.GetExtension(fuManual.FileName).ToLower();
            byte[] head = new byte[4];
            Stream fs = fuManual.PostedFile.InputStream;
            int n = fs.Read(head, 0, 4);
            fs.Position = 0;
            bool isPdf = n == 4 && head[0] == 0x25 && head[1] == 0x50 && head[2] == 0x44 && head[3] == 0x46;

            if (ext != ".pdf" || !isPdf)
            {
                alert_toast("อนุญาตเฉพาะไฟล์ PDF เท่านั้น", "ไฟล์ไม่ถูกต้อง", "warn", "5000");
                ScriptManager.RegisterStartupScript(this, GetType(), "reopenUpload", reopen, true);
                return;
            }

            string fullPath = Path.Combine(MANUAL_FOLDER, target + ".pdf");

            try
            {
                if (!Directory.Exists(MANUAL_FOLDER))
                    Directory.CreateDirectory(MANUAL_FOLDER);

                fuManual.PostedFile.SaveAs(fullPath);   // ทับไฟล์เดิม

                alert_toast("อัปโหลด " + target + ".pdf สำเร็จ", "Upload Complete", "success", "3000");
                ScriptManager.RegisterStartupScript(this, GetType(), "closeUpload",
                    "$('#modal_UploadManual').modal('hide');", true);
            }
            catch (UnauthorizedAccessException)
            {
                alert_toast("ไม่มีสิทธิ์เขียนไฟล์ที่ปลายทาง (ติดต่อ IT ตั้งสิทธิ์ App Pool)",
                            "Access Denied", "error", "8000");
                ScriptManager.RegisterStartupScript(this, GetType(), "reopenUpload",
                    "$('#modal_UploadManual').modal('show');", true);
            }
            catch (Exception ex)
            {
                alert_toast("อัปโหลดไม่สำเร็จ : " + ex.Message, "Error", "error", "8000");
                ScriptManager.RegisterStartupScript(this, GetType(), "reopenUpload",
                    "$('#modal_UploadManual').modal('show');", true);
            }
        }

        #endregion

        #region --- Change Item Click ---
        protected void ddlReason_Borrow_SelectedIndexChanged(object sender, EventArgs e)
        {
            //string reason_Id = ddlReason_Borrow.SelectedValue.ToString();
            string reasonId = ddlReason_Borrow.SelectedValue;
            string str = " select reason_ID , reason_Name , withdrawFlow , comment_withdraw , order_withdraw" +
                         " from Reason_Borrow_Master" +
                         " where reason_id = @reason_id";
            dc.QryDT_P(conn, str,
                new SqlParameter("@reason_id", SqlDbType.NVarChar) { Value = reasonId });
            //dc.QryDT_P(conn, str);

            bool comment = false;

            if (dc.dt.Rows.Count > 0 && dc.dt.Rows[0]["comment_withdraw"] != DBNull.Value)
            {
                comment = Convert.ToBoolean(dc.dt.Rows[0]["comment_withdraw"]);
            }

            //alert_toast("Comment : " + comment.ToString() , "Check Process", "info", "10000");

            if (comment)
            {
                txt_borrowComment.Style["background-color"] = "#fff3cd"; // เหลืองอ่อน
            }
            else
            {
                txt_borrowComment.Style["background-color"] = "#ffffff"; // ขาว
            }
        }
        #endregion

        #region --- Gridview ---

        // Gridview : gvSearchResult
        protected void gvSearchResult_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Attributes["id"] = "rowSearch_" + e.Row.RowIndex;

                // ── ✅ ถ้า DisplayStatus = อยู่ระหว่างยืม → disable ปุ่ม Add ──
                DataRowView rowView = e.Row.DataItem as DataRowView;
                if (rowView != null)
                {
                    string displayStatus = rowView["DisplayStatus"].ToString();

                    if (displayStatus == "อยู่ระหว่างยืม")
                    {
                        Button btnAdd = e.Row.Cells[1].Controls
                            .OfType<Button>()
                            .FirstOrDefault();

                        if (btnAdd != null)
                        {
                            btnAdd.Enabled = false;
                            btnAdd.ToolTip = "อุปกรณ์นี้อยู่ระหว่างการยืม";
                            btnAdd.CssClass = "btn btn-secondary btn-sm";
                        }
                    }
                }
            }
        }
        protected void gvSearchResult_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "AddItem") return;

            int rowIndex = Convert.ToInt32(e.CommandArgument);
            GridViewRow row = gvSearchResult.Rows[rowIndex];

            HiddenField hdtoolId = (HiddenField)row.FindControl("hdtoolId");
            HiddenField hdtoolSub = (HiddenField)row.FindControl("hdtoolSub");
            HiddenField hdsystemId = (HiddenField)row.FindControl("hdsystemId");

            string toolId = hdtoolId.Value;
            string subId = hdtoolSub.Value;
            string systemId = hdsystemId.Value;

            // ── ตรวจ DisplayStatus ──
            DataTable dtSearch = ViewState["SearchResult"] as DataTable;
            if (dtSearch != null && rowIndex < dtSearch.Rows.Count)
            {
                if (dtSearch.Rows[rowIndex]["DisplayStatus"].ToString() == "อยู่ระหว่างยืม")
                {
                    alert_toast("อุปกรณ์นี้อยู่ระหว่างการยืม ไม่สามารถเพิ่มรายการได้",
                                "ไม่สามารถเพิ่มได้", "warn", "5000");
                    return;
                }
            }

            // ── ตรวจซ้ำ (ข้าม row ที่ถูก mark Delete) ──
            foreach (DataRow br in BorrowListDT.Rows)
            {
                if (GetVal(br, "CompareData") == "Delete") continue;

                if (GetVal(br, "ToolId") == toolId &&
                    GetVal(br, "ToolSub") == subId &&
                    GetVal(br, "SystemId") == systemId)
                {
                    alert_toast("มีรายการนี้แล้วใน Borrow List", "พบรายการซ้ำ", "info", "5000");
                    return;
                }
            }

            // ── Query ดึงข้อมูล Tool ──
            string str =
               "SELECT * FROM Tool_Equipment_List_View" +
               " WHERE tool_id = @tool_id" +
               "   AND sub_no  = @sub_no" +
               "   AND sys_status = @sys_status" +
               " ORDER BY tool_no";

            dc.QryDT_P(conn, str,
                new SqlParameter("@tool_id", SqlDbType.NVarChar) { Value = toolId },
                new SqlParameter("@sub_no", SqlDbType.NVarChar) { Value = subId },
                new SqlParameter("@sys_status", SqlDbType.NVarChar) { Value = systemId });

            DataTable tool = dc.dt;

            if (tool == null || tool.Rows.Count == 0)
            {
                alert_toast("Add List Error : ไม่พบข้อมูลใน Database", "Error", "error", "5000");
                return;
            }

            // ✅ Sync GridView → BorrowListDT ก่อน Add (ไม่ทำ Compare/DB query)
            SyncGridViewToBorrowListDT();

            // ── เพิ่ม row ใหม่ ──
            DataTable dt = BorrowListDT;

            int nextRowNo = 1;
            foreach (DataRow r in dt.Rows)
                if (GetVal(r, "CompareData") != "Delete") nextRowNo++;

            DataRow newRow = dt.NewRow();
            newRow["RowNo"] = nextRowNo;
            newRow["ToolId"] = tool.Rows[0]["tool_id"];
            newRow["ToolSub"] = tool.Rows[0]["sub_no"];
            newRow["SystemId"] = tool.Rows[0]["sys_status"];
            newRow["FixAssetNo"] = tool.Rows[0]["FixAcces"];
            newRow["ControlNo"] = tool.Rows[0]["control_no"];
            newRow["ToolNo"] = tool.Rows[0]["tool_no"];
            newRow["DeptCode"] = tool.Rows[0]["dept_code"];
            newRow["ModelName"] = tool.Rows[0]["model_name"];
            newRow["ModelId"] = tool.Rows[0]["model_id"];
            //newRow["LocationName"] = tool.Rows[0]["location"];
            //newRow["LocationId"] = tool.Rows[0]["location1_id"];
            newRow["CellName"] = tool.Rows[0]["cell"];
            newRow["CellId"] = tool.Rows[0]["location2_id"];
            newRow["StationName"] = tool.Rows[0]["station"];
            newRow["StationId"] = tool.Rows[0]["station_id"];
            newRow["CompareData"] = "Insert"; // ✅ mark Insert ทันที

            dt.Rows.Add(newRow);
            BorrowListDT = dt;
            BindBorrowList_Display(); // ✅ แสดงเฉพาะ row ที่ไม่ใช่ Delete

            // Clear Table Search
            ViewState["SearchResult"] = null;
            gvSearchResult.DataSource = null;
            gvSearchResult.DataBind();
        }

        // Gridview : gvBorrowList
        protected void gvBorrowList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            string mode = ViewState["MODE"].ToString();

            if (mode != "edit" && e.Row.RowType == DataControlRowType.DataRow)
            {
                MakeRowReadonly(e.Row);

                LinkButton btnDetail = (LinkButton)e.Row.FindControl("btn_detailView");
                if (btnDetail != null) btnDetail.Enabled = true;

                LinkButton btnPic = (LinkButton)e.Row.FindControl("btn_pic");
                if (btnPic != null) btnPic.Enabled = true;
            }

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView row = (DataRowView)e.Row.DataItem;
                if (row == null) return;
                DataRow cRow = row.Row;

                // highlight row ที่กำลังยืม
                if (mode == "edit")
                {
                    string statusId = ColVal(cRow, "StatusId");
                    if (statusId != "" && statusId != "1" && statusId != "100" && statusId != "999")
                    {
                        foreach (TableCell cell in e.Row.Cells)
                            cell.Style["background-color"] = "#fff3cd";
                        e.Row.Style["border-left"] = "8px solid #dc3545";
                    }
                }

                e.Row.Attributes["id"] = "row_" + e.Row.RowIndex;

                #region --- Use Dept ---

                DropDownList ddlDept = e.Row.FindControl("ddlDept") as DropDownList;

                if (ddlDept != null)
                {
                    ddlDept.Style["display"] = "inline-block";
                    if (ddlDept.Items.Count == 0)
                    {
                        DataTable m = (DataTable)ViewState["DEPT_MASTER"];
                        ddlDept.DataSource = m;
                        ddlDept.DataTextField = "dept_name2";
                        ddlDept.DataValueField = "dept_code";
                        ddlDept.DataBind();
                        ddlDept.Items.Insert(0, new ListItem("-- Select --", ""));
                    }
                    string v = ColVal(cRow, "Use_DeptCode");
                    if (!string.IsNullOrEmpty(v) && ddlDept.Items.FindByValue(v) != null)
                        ddlDept.SelectedValue = v;
                }
                #endregion

                #region --- Use Model ---

                DropDownList ddlModel = e.Row.FindControl("ddlModel") as DropDownList;
                TextBox txtModel = e.Row.FindControl("txtModel") as TextBox;
                HiddenField hfModelMode = e.Row.FindControl("hfModelMode") as HiddenField;

                if (ColVal(cRow, "HD_Model") == "txt")
                {
                    if (hfModelMode != null) hfModelMode.Value = "txt";
                    if (ddlModel != null) ddlModel.Style["display"] = "none";
                    if (txtModel != null) { txtModel.Style["display"] = "inline-block"; txtModel.Text = ColVal(cRow, "Add_Model"); }
                }
                else
                {
                    if (hfModelMode != null) hfModelMode.Value = "ddl";
                    if (txtModel != null) txtModel.Style["display"] = "none";
                    if (ddlModel != null)
                    {
                        ddlModel.Style["display"] = "inline-block";
                        if (ddlModel.Items.Count == 0)
                        {
                            DataTable m = (DataTable)ViewState["MODEL_MASTER"];
                            ddlModel.DataSource = m;
                            ddlModel.DataTextField = "model_name";
                            ddlModel.DataValueField = "model_id";
                            ddlModel.DataBind();
                            ddlModel.Items.Insert(0, new ListItem("-- Select --", ""));
                        }
                        string v = ColVal(cRow, "Use_ModelId");
                        if (!string.IsNullOrEmpty(v) && ddlModel.Items.FindByValue(v) != null)
                            ddlModel.SelectedValue = v;
                    }
                }

                #endregion

                #region --- Use Cell ---

                DropDownList ddlCell = e.Row.FindControl("ddlCell") as DropDownList;
                TextBox txtCell = e.Row.FindControl("txtCell") as TextBox;
                HiddenField hfCellMode = e.Row.FindControl("hfCellMode") as HiddenField;

                if (ColVal(cRow, "HD_Cell") == "txt")
                {
                    if (hfCellMode != null) hfCellMode.Value = "txt";
                    if (ddlCell != null) ddlCell.Style["display"] = "none";
                    if (txtCell != null) { txtCell.Style["display"] = "inline-block"; txtCell.Text = ColVal(cRow, "Add_Cell"); }
                }
                else
                {
                    if (hfCellMode != null) hfCellMode.Value = "ddl";
                    if (txtCell != null) txtCell.Style["display"] = "none";
                    if (ddlCell != null)
                    {
                        ddlCell.Style["display"] = "inline-block";
                        ddlCell.DataSource = GetCellMaster();
                        ddlCell.DataTextField = "location2_name";
                        ddlCell.DataValueField = "location2_id";
                        ddlCell.DataBind();
                        ddlCell.Items.Insert(0, new ListItem("-- Select --", ""));
                        string v = ColVal(cRow, "Use_CellId");
                        if (!string.IsNullOrEmpty(v) && ddlCell.Items.FindByValue(v) != null)
                            ddlCell.SelectedValue = v;
                    }
                }

                #endregion

                #region --- STATION ---

                DropDownList ddlStation = e.Row.FindControl("ddlStation") as DropDownList;
                TextBox txtStation = e.Row.FindControl("txtStation") as TextBox;
                HiddenField hfStationMode = e.Row.FindControl("hfStationMode") as HiddenField;

                if (ColVal(cRow, "HD_Station") == "txt")
                {
                    if (hfStationMode != null) hfStationMode.Value = "txt";
                    if (ddlStation != null) ddlStation.Style["display"] = "none";
                    if (txtStation != null) { txtStation.Style["display"] = "inline-block"; txtStation.Text = ColVal(cRow, "Add_Station"); }
                }
                else
                {
                    if (hfStationMode != null) hfStationMode.Value = "ddl";
                    if (txtStation != null) txtStation.Style["display"] = "none";
                    if (ddlStation != null)
                    {
                        ddlStation.Style["display"] = "inline-block";
                        ddlStation.DataSource = GetStationMaster();
                        ddlStation.DataTextField = "location3_name";
                        ddlStation.DataValueField = "location3_id";
                        ddlStation.DataBind();
                        ddlStation.Items.Insert(0, new ListItem("-- Select --", ""));
                        string v = ColVal(cRow, "Use_StationId");
                        if (!string.IsNullOrEmpty(v) && ddlStation.Items.FindByValue(v) != null)
                            ddlStation.SelectedValue = v;
                    }
                }

                #endregion
            }
        }
        /// <summary>
        /// Key ของแถวที่กด Remove
        ///   1) gvBorrowList.DataKeys (DataKeyNames="ToolId,ToolSub,SystemId" → อยู่ใน ControlState ไม่หายตอน postback)
        ///   2) สำรอง : HiddenField ในคอลัมน์ Pic (ID ต้องตรง markup : hdtoolId / hdToolSub / hdSystemId)
        /// </summary>
        private bool GetRemoveRowKeys(int rowIndex, out string toolId, out string toolSub, out string systemId)
        {
            toolId = toolSub = systemId = "";

            if (gvBorrowList.DataKeys != null && rowIndex < gvBorrowList.DataKeys.Count && gvBorrowList.DataKeys[rowIndex] != null)
            {
                var v = gvBorrowList.DataKeys[rowIndex].Values;
                toolId = Convert.ToString(v["ToolId"]).Trim();
                toolSub = Convert.ToString(v["ToolSub"]).Trim();
                systemId = Convert.ToString(v["SystemId"]).Trim();
                if (toolId != "") return true;
            }

            GridViewRow gvRow = gvBorrowList.Rows[rowIndex];
            HiddenField hdToolId = gvRow.FindControl("hdtoolId") as HiddenField;
            HiddenField hdToolSub = gvRow.FindControl("hdToolSub") as HiddenField;
            HiddenField hdSystemId = gvRow.FindControl("hdSystemId") as HiddenField;
            if (hdToolId == null || hdToolSub == null || hdSystemId == null) return false;

            toolId = hdToolId.Value.Trim();
            toolSub = hdToolSub.Value.Trim();
            systemId = hdSystemId.Value.Trim();
            return toolId != "";
        }

        protected void gvBorrowList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "RemoveItem") return;

            int rowIndex = Convert.ToInt32(e.CommandArgument);

            if (rowIndex < 0 || rowIndex >= gvBorrowList.Rows.Count) return;

            // ── Key ของแถว : DataKeys (เก็บใน ControlState) ก่อน → ไม่มีค่อยอ่าน HiddenField ──
            string toolId, toolSub, systemId;
            if (!GetRemoveRowKeys(rowIndex, out toolId, out toolSub, out systemId))
            {
                alert_toast("ไม่พบ Key ของรายการที่ต้องการลบ (ToolId / ToolSub / SystemId)", "Remove Error", "error", "5000");
                return;
            }

            // ── STEP 1 : Sync GridView → BorrowListDT (ไม่ Compare) ──
            SyncGridViewToBorrowListDT();

            DataTable dt = BorrowListDT;

            // ── STEP 2 : ค้นหา row ด้วย Key ──
            DataRow targetRow = null;

            foreach (DataRow r in dt.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;

                if (GetVal(r, "ToolId") == toolId &&
                    GetVal(r, "ToolSub") == toolSub &&
                    GetVal(r, "SystemId") == systemId)
                {
                    targetRow = r;
                    break;
                }
            }

            if (targetRow == null)
            {
                alert_toast("ไม่พบรายการที่ต้องการลบ", "Remove Error", "error", "5000");
                return;
            }

            // ── STEP 3 : ลบหรือ mark Delete ──
            string cmp = GetVal(targetRow, "CompareData");

            if (cmp == "Insert")
            {
                // ยังไม่มีใน DB → ลบออกเลย
                targetRow.Delete();
                dt.AcceptChanges();
            }
            else
            {
                // มีใน DB → mark Delete ไว้ก่อน จะ DELETE จริงตอน saveData
                targetRow["CompareData"] = "Delete";
            }

            // ── STEP 4 : บันทึกและ Bind ──
            BorrowListDT = dt;
            BindBorrowList_Display();
        }

        #endregion

        #region --- DataTable BorrowListDT : เก็บข้อมูล Withdraw List ---

        // Create Table Date
        private DataTable CreateEmptyChangeTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("RowNo", typeof(int));
            dt.Columns.Add("ToolId", typeof(string));
            dt.Columns.Add("ToolSub", typeof(string));
            dt.Columns.Add("SystemId", typeof(string));
            dt.Columns.Add("FixAssetNo", typeof(string));
            dt.Columns.Add("ControlNo", typeof(string));
            dt.Columns.Add("ToolNo", typeof(string));
            dt.Columns.Add("DeptCode", typeof(string));
            dt.Columns.Add("ModelName", typeof(string));
            dt.Columns.Add("ModelId", typeof(string));
            dt.Columns.Add("CellName", typeof(string));
            dt.Columns.Add("CellId", typeof(string));
            dt.Columns.Add("StationName", typeof(string));
            dt.Columns.Add("StationId", typeof(string));

            //Use
            dt.Columns.Add("Use_DeptCode", typeof(string));

            dt.Columns.Add("Use_ModelId", typeof(string));
            dt.Columns.Add("Add_Model", typeof(string));
            dt.Columns.Add("HD_Model", typeof(string));

            dt.Columns.Add("Use_CellId", typeof(string));
            dt.Columns.Add("Add_Cell", typeof(string));
            dt.Columns.Add("HD_Cell", typeof(string));

            dt.Columns.Add("Use_StationId", typeof(string));
            dt.Columns.Add("Add_Station", typeof(string));
            dt.Columns.Add("HD_Station", typeof(string));

            //For Remove
            dt.Columns.Add("StatusId", typeof(string));

            //Compare Data
            dt.Columns.Add("CompareData", typeof(string));

            return dt;
        }

        // ✅ Sync GridView → BorrowListDT เท่านั้น (ไม่ Compare)
        private void SyncGridViewToBorrowListDT()
        {
            DataTable borrowDT = BorrowListDT;

            // map GridView row (ไม่รวม Delete) → BorrowListDT index จริง
            var displayToReal = new System.Collections.Generic.Dictionary<int, int>();
            int displayIdx = 0;

            for (int i = 0; i < borrowDT.Rows.Count; i++)
            {
                if (GetVal(borrowDT.Rows[i], "CompareData") == "Delete") continue;
                displayToReal[displayIdx++] = i;
            }

            foreach (GridViewRow gvRow in gvBorrowList.Rows)
            {
                if (gvRow.RowType != DataControlRowType.DataRow) continue;

                int gvIdx = gvRow.DataItemIndex;
                if (!displayToReal.ContainsKey(gvIdx)) continue;

                int realIdx = displayToReal[gvIdx];
                DataRow dr = borrowDT.Rows[realIdx];

                // Dept Code
                DropDownList ddlDept = gvRow.FindControl("ddlDept") as DropDownList;

                if (ddlDept != null)
                    dr["Use_DeptCode"] = string.IsNullOrEmpty(ddlDept.SelectedValue)
                                        ? (object)DBNull.Value : ddlDept.SelectedValue;

                // Model
                DropDownList ddlModel = gvRow.FindControl("ddlModel") as DropDownList;
                TextBox txtModel = gvRow.FindControl("txtModel") as TextBox;
                HiddenField hfModelMode = gvRow.FindControl("hfModelMode") as HiddenField;

                if (hfModelMode != null)
                    dr["HD_Model"] = string.IsNullOrEmpty(hfModelMode.Value)
                                        ? (object)DBNull.Value : hfModelMode.Value;
                if (ddlModel != null)
                    dr["Use_ModelId"] = string.IsNullOrEmpty(ddlModel.SelectedValue)
                                        ? (object)DBNull.Value : ddlModel.SelectedValue;
                if (txtModel != null)
                    dr["Add_Model"] = string.IsNullOrEmpty(txtModel.Text)
                                        ? (object)DBNull.Value : txtModel.Text;

                // Cell
                DropDownList ddlCell = gvRow.FindControl("ddlCell") as DropDownList;
                TextBox txtCell = gvRow.FindControl("txtCell") as TextBox;
                HiddenField hfCellMode = gvRow.FindControl("hfCellMode") as HiddenField;

                if (hfCellMode != null)
                    dr["HD_Cell"] = string.IsNullOrEmpty(hfCellMode.Value)
                                       ? (object)DBNull.Value : hfCellMode.Value;
                if (ddlCell != null)
                    dr["Use_CellId"] = string.IsNullOrEmpty(ddlCell.SelectedValue)
                                       ? (object)DBNull.Value : ddlCell.SelectedValue;
                if (txtCell != null)
                    dr["Add_Cell"] = string.IsNullOrEmpty(txtCell.Text)
                                       ? (object)DBNull.Value : txtCell.Text;

                // Station
                DropDownList ddlStation = gvRow.FindControl("ddlStation") as DropDownList;
                TextBox txtStation = gvRow.FindControl("txtStation") as TextBox;
                HiddenField hfStationMode = gvRow.FindControl("hfStationMode") as HiddenField;

                if (hfStationMode != null)
                    dr["HD_Station"] = string.IsNullOrEmpty(hfStationMode.Value)
                                          ? (object)DBNull.Value : hfStationMode.Value;
                if (ddlStation != null)
                    dr["Use_StationId"] = string.IsNullOrEmpty(ddlStation.SelectedValue)
                                          ? (object)DBNull.Value : ddlStation.SelectedValue;
                if (txtStation != null)
                    dr["Add_Station"] = string.IsNullOrEmpty(txtStation.Text)
                                          ? (object)DBNull.Value : txtStation.Text;
            }

            BorrowListDT = borrowDT;
        }

        // ✅ Bind GridView เฉพาะ row ที่ user เห็น (ไม่รวม Delete)
        private void BindBorrowList_Display()
        {
            DataTable dt = BorrowListDT;
            DataTable dtDisplay = dt.Clone();

            int rowNo = 1;
            foreach (DataRow r in dt.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;

                DataRow newR = dtDisplay.NewRow();
                newR.ItemArray = (object[])r.ItemArray.Clone();
                newR["RowNo"] = rowNo++;
                dtDisplay.Rows.Add(newR);
            }

            gvBorrowList.DataSource = dtDisplay;
            gvBorrowList.DataBind();
        }
        private Boolean Check_blankData()
        {
            DataTable dt = BorrowListDT;
            bool hasBlank = false;

            foreach (GridViewRow row in gvBorrowList.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow) continue;

                // ══════════════════════════════════════════
                // USE : Dept
                // ══════════════════════════════════════════
                DropDownList ddlDept = row.FindControl("ddlDept") as DropDownList;

                bool blank_dept = ddlDept == null || string.IsNullOrEmpty(ddlDept.SelectedValue);
                SetWarning_DDL(ddlDept, blank_dept);
                if (blank_dept) hasBlank = true;                

                // ══════════════════════════════════════════
                // USE : MODEL
                // ══════════════════════════════════════════
                DropDownList ddlModel = row.FindControl("ddlModel") as DropDownList;
                TextBox txtModel = row.FindControl("txtModel") as TextBox;
                HiddenField hfModelMode = row.FindControl("hfModelMode") as HiddenField;

                if (hfModelMode != null && hfModelMode.Value == "txt")
                {
                    bool blank = txtModel == null || string.IsNullOrWhiteSpace(txtModel.Text);
                    SetWarning_TextBox(txtModel, blank);
                    SetWarning_DDL(ddlModel, false);
                    if (blank) hasBlank = true;
                }
                else
                {
                    bool blank = ddlModel == null || string.IsNullOrEmpty(ddlModel.SelectedValue);
                    SetWarning_DDL(ddlModel, blank);
                    SetWarning_TextBox(txtModel, false);
                    if (blank) hasBlank = true;
                }

                // ══════════════════════════════════════════
                // USE : CELL
                // ══════════════════════════════════════════
                DropDownList ddlCell = row.FindControl("ddlCell") as DropDownList;
                TextBox txtCell = row.FindControl("txtCell") as TextBox;
                HiddenField hfCellMode = row.FindControl("hfCellMode") as HiddenField;

                if (hfCellMode != null && hfCellMode.Value == "txt")
                {
                    bool blank = txtCell == null || string.IsNullOrWhiteSpace(txtCell.Text);
                    SetWarning_TextBox(txtCell, blank);
                    SetWarning_DDL(ddlCell, false);
                    if (blank) hasBlank = true;
                }
                else
                {
                    bool blank = ddlCell == null || string.IsNullOrEmpty(ddlCell.SelectedValue);
                    SetWarning_DDL(ddlCell, blank);
                    SetWarning_TextBox(txtCell, false);
                    if (blank) hasBlank = true;
                }

                // ══════════════════════════════════════════
                // USE : STATION
                // ══════════════════════════════════════════
                DropDownList ddlStation = row.FindControl("ddlStation") as DropDownList;
                TextBox txtStation = row.FindControl("txtStation") as TextBox;
                HiddenField hfStationMode = row.FindControl("hfStationMode") as HiddenField;

                if (hfStationMode != null && hfStationMode.Value == "txt")
                {
                    bool blank = txtStation == null || string.IsNullOrWhiteSpace(txtStation.Text);
                    SetWarning_TextBox(txtStation, blank);
                    SetWarning_DDL(ddlStation, false);
                    if (blank) hasBlank = true;
                }
                else
                {
                    bool blank = ddlStation == null || string.IsNullOrEmpty(ddlStation.SelectedValue);
                    SetWarning_DDL(ddlStation, blank);
                    SetWarning_TextBox(txtStation, false);
                    if (blank) hasBlank = true;
                }
            }

            return hasBlank;
        }

        #endregion

        #region --- Update to Database ---

        protected void saveData(string status_id)
        {
            DataTable dt = BorrowListDT;
            string Doc_No = litIssueNo.Text.Trim();

            int reason_id = 0;
            string comment_text = txt_borrowComment.Text.Trim();
            string phone = txtTel.Text.Trim();

            DateTime issue_date = Convert.ToDateTime(txtIssueDate.Text);

            if (ddlReason_Borrow.SelectedIndex > 0)
                int.TryParse(ddlReason_Borrow.SelectedValue, out reason_id);

            // ════════════════════════════════════════════════════════════════
            // UPDATE : มี Doc_No อยู่แล้ว
            // ════════════════════════════════════════════════════════════════
            if (Doc_No != "-" && !string.IsNullOrEmpty(Doc_No))
            {
                Save_BorrowListDT(status_id); // Update Data on Table : BorrowListDT

                // -- ตรวจสอบรายการ "กำลังยืม"
                if (duringBorrowCheck())
                {
                    BindBorrowList();
                    alert_toast("พบรายการอยู่ระหว่างการยืม : ไม่สามารถยืมได้", "พบรายการอยู่่ระหว่างการยืม", "warn", "5000");
                    return;
                }

                // ── Update header status ──
                conn.Open();
                try
                {
                    // -- Click Draft button
                    if (status_id == "1")
                    {
                        string sqlHeader =
                            "UPDATE Withdraw_Control_List SET status_id = @status_id  , " +
                            " phone = @phone , " +
                            " reason_id = @reason_id , reason_remark = @reason_remark , update_date = @update_date " +
                            " WHERE Doc_No = @Doc_No";

                        using (SqlCommand cmd = new SqlCommand(sqlHeader, conn))
                        {
                            cmd.Parameters.AddWithValue("@status_id", status_id);
                            cmd.Parameters.AddWithValue("@phone",
                                string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                            cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                            cmd.Parameters.AddWithValue("@reason_id", reason_id);
                            cmd.Parameters.AddWithValue("@reason_remark", comment_text);
                            cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // -- Click Send button
                    else if (status_id == "50")
                    {
                        string sqlHeader =
                            "UPDATE Withdraw_Control_List SET status_id = @status_id  , " +
                            " phone = @phone , " +
                            " reason_id = @reason_id , reason_remark = @reason_remark , update_date = @update_date , send_date = @send_date " +
                            " WHERE Doc_No = @Doc_No";

                        using (SqlCommand cmd = new SqlCommand(sqlHeader, conn))
                        {
                            cmd.Parameters.AddWithValue("@status_id", status_id);
                            cmd.Parameters.AddWithValue("@phone",
                                string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                            cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                            cmd.Parameters.AddWithValue("@reason_id", reason_id);
                            cmd.Parameters.AddWithValue("@reason_remark", comment_text);
                            cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                            cmd.Parameters.AddWithValue("@send_date", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    alert_toast("Error :" + ex, "บันทึกไม่สำเร็จ", "error", "8000");
                }
                finally
                {
                    conn.Close();
                }

                // ✅ อัปเดตข้อมูล Requester ณ วันที่บันทึก/ส่ง
                SaveRequesterInfo_Safe(Doc_No, true, phone);

                Sync_BorrowList(Doc_No, status_id);  // Insert / Update / Delete
            }

            // ════════════════════════════════════════════════════════════════
            // INSERT : ยังไม่มี Doc_No → ไปทาง retry branch
            // ════════════════════════════════════════════════════════════════
            else
            {
                SaveData_InsertBranch(status_id);
            }
        }

        // --- SaveData_InsertBranch() — transaction-wrapped + retry
        private void SaveData_InsertBranch(string status_id)
        {
            // ---- เตรียมข้อมูล ทำครั้งเดียว ก่อนเข้าลูป retry ----
            Save_BorrowListDT(status_id);

            if (duringBorrowCheck())
            {
                BindBorrowList();
                alert_toast("พบรายการอยู่ระหว่างการยืม : ไม่สามารถยืมได้",
                            "พบรายการอยู่่ระหว่างการยืม", "warn", "5000");
                return;
            }

            DataTable dt = BorrowListDT;
            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูลในรายการ Request List", "ข้อมูลไม่ครบ", "warn", "5000");
                return;
            }

            DateTime issueDate = DateTime.Parse(txtIssueDate.Text);
            int issueYear = issueDate.Year;
            string issueYear_2 = (issueYear % 100).ToString("00");

            const int MAX_RETRY = 5;
            string Doc_No = null;
            bool success = false;

            string connStr = ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString;

            for (int attempt = 1; attempt <= MAX_RETRY; attempt++)
            {
                using (var c = new SqlConnection(connStr))
                {
                    c.Open();
                    using (var tx = c.BeginTransaction())
                    {
                        try
                        {
                            // 1) อ่านเลขล่าสุด "ภายใน transaction" (ทุกรอบอ่านใหม่)
                            int issueNo = GetNextIssueNo_Tx(c, tx, issueYear);
                            Doc_No = "AAD3-" + issueYear_2 + "-" + issueNo.ToString("00000");

                            // 2) INSERT header (ถ้า Doc_No ซ้ำ → throw 2627 ที่นี่)
                            InsertControlHeader_Tx(c, tx, issueNo, Doc_No, status_id);

                            // 3) INSERT รายการ Withdraw_List ทั้งหมด (ใน tx เดียวกัน)
                            InsertWithdrawRows_Tx(c, tx, dt, Doc_No, status_id);

                            // 3.1) ✅ เก็บข้อมูล Requester (ใน tx เดียวกัน)
                            SaveRequesterInfo(c, tx, Doc_No, true, txtTel.Text.Trim());

                            // 3.2) ✅ AAD Confirm (99) → ประวัติเปลี่ยน Location (ใน tx เดียวกัน)
                            if (status_id == "99")
                                InsertLocationHistory_Tx(c, tx, dt, Doc_No,
                                    Session["emp_id"] != null ? Session["emp_id"].ToString().Trim() : "");

                            // 4) ทุกอย่างผ่าน → commit
                            tx.Commit();
                            success = true;
                            break;
                        }
                        catch (SqlException ex)
                        {
                            try { tx.Rollback(); } catch { }

                            // ── ถ้าเป็น duplicate key → retry ──
                            if (IsDuplicateKeyError(ex))
                            {
                                if (attempt == MAX_RETRY)
                                {
                                    alert_toast(
                                        "ระบบไม่ว่าง มีการบันทึกพร้อมกันหลายรายการ กรุณาลองใหม่อีกครั้ง",
                                        "บันทึกไม่สำเร็จ", "error", "8000");
                                    return;
                                }
                                continue;
                            }

                            alert_toast("เกิดข้อผิดพลาด : " + ex.Message, "Error", "error", "10000");
                            return;
                        }
                        catch (Exception ex)
                        {
                            try { tx.Rollback(); } catch { }
                            alert_toast("เกิดข้อผิดพลาด : " + ex.Message, "Error", "error", "10000");
                            return;
                        }
                    }
                }
            }

            if (!success) return;

            // ── สำเร็จ → alert + redirect ──
            ShowSaveSuccessAndRedirect(Doc_No, status_id);
        }

        // --- INSERT header ภายใน transaction (ปล่อย SqlException หลุดให้ลูป retry จับ)
        private void InsertControlHeader_Tx(SqlConnection c, SqlTransaction tx, int issueNo, string Doc_No, string status_id)
        {
            string issue_empid = txtEmpId.Text;
            string phone = txtTel.Text.Trim();
            int reason_id = 0;            
            string comment_text = txt_borrowComment.Text.Trim();
            string dept_code = txtDeptCode.Text.Trim();

            if (ddlReason_Borrow.SelectedIndex > 0)
                int.TryParse(ddlReason_Borrow.SelectedValue, out reason_id);

            string sql;
            if (status_id == "1")
            {
                sql = @"INSERT INTO Withdraw_Control_List
                ( issue_no , issue_date , Doc_No , status_id , issue_empid ,
                  dept_code , phone , reason_id , reason_remark , update_date )
                VALUES
                ( @issue_no , @issue_date , @Doc_No , @status_id , @issue_empid ,
                  @dept_code , @phone , @reason_id , @reason_remark , @update_date )";
            }
            else if (status_id == "50")
            {
                sql = @"INSERT INTO Withdraw_Control_List
                ( issue_no , issue_date , Doc_No , status_id , issue_empid ,
                  dept_code , phone , reason_id , reason_remark , update_date , send_date )
                VALUES
                ( @issue_no , @issue_date , @Doc_No , @status_id , @issue_empid ,
                  @dept_code , @phone , @reason_id , @reason_remark , @update_date , @send_date )";
            }
            else if (status_id == "99")
            {
                sql = @"INSERT INTO Withdraw_Control_List
                ( issue_no , issue_date , Doc_No , status_id , issue_empid ,
                  dept_code , phone , reason_id , reason_remark , update_date , send_date ,
                  confirm_empid , confirm_date )
                VALUES
                ( @issue_no , @issue_date , @Doc_No , @status_id , @issue_empid ,
                  @dept_code , @phone , @reason_id , @reason_remark , @update_date , @send_date ,
                  @confirm_empid , @confirm_date )";
            }
            else
            {
                throw new InvalidOperationException("Unknown status_id: " + status_id);
            }

            using (var cmd = new SqlCommand(sql, c, tx))
            {
                cmd.Parameters.AddWithValue("@issue_no", issueNo);
                cmd.Parameters.AddWithValue("@issue_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                cmd.Parameters.AddWithValue("@status_id", status_id);
                cmd.Parameters.AddWithValue("@issue_empid", issue_empid);
                cmd.Parameters.AddWithValue("@dept_code", dept_code);

                // ✅ phone : ถ้าว่างเก็บ DBNull
                cmd.Parameters.AddWithValue("@phone",
                    string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);

                cmd.Parameters.AddWithValue("@reason_id", reason_id);
                cmd.Parameters.AddWithValue("@reason_remark", comment_text);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);

                if (status_id == "50" || status_id == "99")
                    cmd.Parameters.AddWithValue("@send_date", DateTime.Now);

                if (status_id == "99")
                {
                    cmd.Parameters.AddWithValue("@confirm_empid",Session["emp_id"].ToString().Trim());
                    cmd.Parameters.AddWithValue("@confirm_date", DateTime.Now);
                }

                cmd.ExecuteNonQuery();   // ← Doc_No ซ้ำ throw 2627 ที่นี่
            }
        }

        // --- INSERT รายการ Withdraw_List ทั้งหมด ภายใน transaction (กัน half-document)
        private void InsertWithdrawRows_Tx(SqlConnection c, SqlTransaction tx, DataTable dt, string Doc_No, string status_id)
        {
            string sql = @"
                INSERT INTO Withdraw_List
                ( Doc_No , No , FixAsset , ControlNo ,
                  tool_id , sub_id , system_id ,
                  Use_Dept ,
                  Use_Model_Id , New_Model , HD_Model ,
                  Use_Cell_Id , New_Cell , HD_Cell ,
                  Use_Station_Id , New_Station , HD_Station ,
                  status_id , update_date ,
                  Old_Status_Id , New_Status_Id , Old_Responsible , New_Responsible )
                SELECT
                  @Doc_No , @No , @FixAsset , @ControlNo ,
                  @tool_id , @sub_id , @system_id ,
                  @Use_Dept ,
                  @Use_Model_Id , @New_Model , @HD_Model ,
                  @Use_Cell_Id , @New_Cell , @HD_Cell ,
                  @Use_Station_Id , @New_Station , @HD_Station ,
                  @status_id , @update_date ,
                  -- AAD Confirm (99) : Old = snapshot ปัจจุบันของอุปกรณ์ , New = ค่าที่เลือกใน Modal
                  CASE WHEN @status_id = 99 THEN V.status_id END ,
                  @New_Status_Id ,
                  CASE WHEN @status_id = 99 THEN CAST(V.rsp_empID AS nvarchar(50)) END ,
                  @New_Responsible
                FROM (SELECT 1 AS x) D
                OUTER APPLY (SELECT TOP 1 status_id , rsp_empID FROM Tool_Equipment_List_View
                             WHERE tool_id = @tool_id AND sub_no = @sub_id AND sys_status = @system_id) V";

            foreach (DataRow row in dt.Rows)
            {
                // ข้าม row ที่ mark Delete (เผื่อมีหลุดมา)
                if (GetVal(row, "CompareData") == "Delete") continue;

                using (var cmd = new SqlCommand(sql, c, tx))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@No", ToDbVal(row, "RowNo"));
                    cmd.Parameters.AddWithValue("@FixAsset", ToDbVal(row, "FixAssetNo"));
                    cmd.Parameters.AddWithValue("@ControlNo", ToDbVal(row, "ControlNo"));
                    cmd.Parameters.AddWithValue("@tool_id", ToDbVal(row, "ToolId"));
                    cmd.Parameters.AddWithValue("@sub_id", ToDbVal(row, "ToolSub"));
                    cmd.Parameters.AddWithValue("@system_id", ToDbVal(row, "SystemId"));
                    cmd.Parameters.AddWithValue("@Use_Dept", ToDbVal(row, "Use_DeptCode"));
                    cmd.Parameters.AddWithValue("@Use_Model_Id", ToDbVal(row, "Use_ModelId"));
                    cmd.Parameters.AddWithValue("@New_Model", ToDbVal(row, "Add_Model"));
                    cmd.Parameters.AddWithValue("@HD_Model", ToDbVal(row, "HD_Model"));
                    cmd.Parameters.AddWithValue("@Use_Cell_Id", ToDbVal(row, "Use_CellId"));
                    cmd.Parameters.AddWithValue("@New_Cell", ToDbVal(row, "Add_Cell"));
                    cmd.Parameters.AddWithValue("@HD_Cell", ToDbVal(row, "HD_Cell"));
                    cmd.Parameters.AddWithValue("@Use_Station_Id", ToDbVal(row, "Use_StationId"));
                    cmd.Parameters.AddWithValue("@New_Station", ToDbVal(row, "Add_Station"));
                    cmd.Parameters.AddWithValue("@HD_Station", ToDbVal(row, "HD_Station"));
                    cmd.Parameters.AddWithValue("@status_id", status_id);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);

                    // ค่าจาก Modal AAD Confirm (มีเฉพาะกรณี Admin AAD Confirm → 99)
                    bool isConfirm = status_id == "99";
                    cmd.Parameters.AddWithValue("@New_Status_Id", isConfirm ? ToDbVal(row, "Use_StatusId") : DBNull.Value);
                    cmd.Parameters.AddWithValue("@New_Responsible", isConfirm ? ToDbVal(row, "Resp_EmpId") : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // --- Save Data on List
        private void Save_BorrowListDT(string status_id)
        {
            // ══════════════════════════════════════════════════════════════════════
            // STEP 1 : Sync GridView → BorrowListDT
            // ══════════════════════════════════════════════════════════════════════
            SyncGridViewToBorrowListDT();

            DataTable borrowDT = BorrowListDT;

            // ══════════════════════════════════════════════════════════════════════
            // STEP 2 : ถ้าไม่มี Doc_No → ทุก row เป็น Insert
            // ══════════════════════════════════════════════════════════════════════
            string docNo = litIssueNo.Text.Trim();

            if (docNo == "-" || string.IsNullOrEmpty(docNo))
            {
                foreach (DataRow r in borrowDT.Rows)
                {
                    string cmp = GetVal(r, "CompareData");
                    // ✅ ไม่ overwrite row ที่ mark Delete อยู่แล้ว
                    if (cmp != "Delete")
                        r["CompareData"] = "Insert";
                }

                BorrowListDT = borrowDT;
                return;
            }

            // ══════════════════════════════════════════════════════════════════════
            // STEP 3 : Query DB
            // ══════════════════════════════════════════════════════════════════════
            string strDB =
               "SELECT tool_id, sub_id, system_id," +
               "       Use_Dept," +
               "       Use_Model_Id, New_Model, HD_Model," +
               "       Use_Cell_Id, New_Cell, HD_Cell," +
               "       Use_Station_Id, New_Station, HD_Station," +
               "       status_id" +
               " FROM  Withdraw_List" +
               " WHERE Doc_No = @Doc_No";

            dc.QryDT_P(conn, strDB,
                new SqlParameter("@Doc_No", SqlDbType.NVarChar) { Value = docNo });
            DataTable dtDB = dc.dt.Copy();

            // ══════════════════════════════════════════════════════════════════════
            // STEP 4 : เก็บ Delete keys จาก BorrowListDT ก่อน loop
            //          กัน DB loop เพิ่ม row ซ้ำ
            // ══════════════════════════════════════════════════════════════════════
            var deleteKeySet = new System.Collections.Generic.HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (DataRow r in borrowDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete")
                {
                    string k = GetVal(r, "ToolId") + "|" +
                               GetVal(r, "ToolSub") + "|" +
                               GetVal(r, "SystemId");
                    deleteKeySet.Add(k);
                }
            }

            // ══════════════════════════════════════════════════════════════════════
            // STEP 5 : Compare dtDB ↔ BorrowListDT
            // ══════════════════════════════════════════════════════════════════════
            var matchedBorrowIdx = new System.Collections.Generic.HashSet<int>();

            foreach (DataRow dbRow in dtDB.Rows)
            {
                string db_toolId = GetVal(dbRow, "tool_id");
                string db_subId = GetVal(dbRow, "sub_id");
                string db_sysId = GetVal(dbRow, "system_id");
                string dbKey = db_toolId + "|" + db_subId + "|" + db_sysId;

                // ✅ ถ้า key นี้ถูก mark Delete ไว้แล้ว → ข้ามเลย ไม่เพิ่มซ้ำ
                if (deleteKeySet.Contains(dbKey)) continue;

                DataRow matchRow = null;
                int matchIdx = -1;

                for (int i = 0; i < borrowDT.Rows.Count; i++)
                {
                    DataRow br = borrowDT.Rows[i];
                    if (GetVal(br, "CompareData") == "Delete") continue;

                    if (GetVal(br, "ToolId") == db_toolId &&
                        GetVal(br, "ToolSub") == db_subId &&
                        GetVal(br, "SystemId") == db_sysId)
                    {
                        matchRow = br;
                        matchIdx = i;
                        break;
                    }
                }

                if (matchRow == null)
                {
                    // ── อยู่ใน DB แต่ไม่มีใน BorrowListDT → Delete ────────────────
                    DataRow delRow = borrowDT.NewRow();
                    delRow["ToolId"] = db_toolId;
                    delRow["ToolSub"] = db_subId;
                    delRow["SystemId"] = db_sysId;
                    delRow["RowNo"] = DBNull.Value;
                    delRow["Use_DeptCode"] = GetVal(dbRow, "Use_Dept");
                    delRow["Use_ModelId"] = GetVal(dbRow, "Use_Model_Id");
                    delRow["Add_Model"] = GetVal(dbRow, "New_Model");
                    delRow["HD_Model"] = GetVal(dbRow, "HD_Model");
                    delRow["Use_CellId"] = GetVal(dbRow, "Use_Cell_Id");
                    delRow["Add_Cell"] = GetVal(dbRow, "New_Cell");
                    delRow["HD_Cell"] = GetVal(dbRow, "HD_Cell");
                    delRow["Use_StationId"] = GetVal(dbRow, "Use_Station_Id");
                    delRow["Add_Station"] = GetVal(dbRow, "New_Station");
                    delRow["HD_Station"] = GetVal(dbRow, "HD_Station");
                    delRow["CompareData"] = "Delete";
                    borrowDT.Rows.Add(delRow);
                }
                else
                {
                    matchedBorrowIdx.Add(matchIdx);

                    bool changed =
                        GetVal(matchRow, "StatusId") != status_id ||
                        GetVal(matchRow, "Use_DeptCode") != GetVal(dbRow, "Use_Dept") ||
                        GetVal(matchRow, "Use_ModelId") != GetVal(dbRow, "Use_Model_Id") ||
                        GetVal(matchRow, "Add_Model") != GetVal(dbRow, "New_Model") ||
                        GetVal(matchRow, "HD_Model") != GetVal(dbRow, "HD_Model") ||
                        GetVal(matchRow, "Use_CellId") != GetVal(dbRow, "Use_Cell_Id") ||
                        GetVal(matchRow, "Add_Cell") != GetVal(dbRow, "New_Cell") ||
                        GetVal(matchRow, "HD_Cell") != GetVal(dbRow, "HD_Cell") ||
                        GetVal(matchRow, "Use_StationId") != GetVal(dbRow, "Use_Station_Id") ||
                        GetVal(matchRow, "Add_Station") != GetVal(dbRow, "New_Station") ||
                        GetVal(matchRow, "HD_Station") != GetVal(dbRow, "HD_Station");

                    matchRow["CompareData"] = changed ? "Update" : "Same";
                }
            }

            // ── มีใน BorrowListDT แต่ไม่มีใน DB → Insert ────────────────────────
            for (int i = 0; i < borrowDT.Rows.Count; i++)
            {
                if (matchedBorrowIdx.Contains(i)) continue;

                string cmp = GetVal(borrowDT.Rows[i], "CompareData");

                // ✅ ไม่ overwrite Delete row
                if (cmp != "Delete" && cmp != "Same" && cmp != "Update")
                    borrowDT.Rows[i]["CompareData"] = "Insert";
            }

            BorrowListDT = borrowDT;

        }

        // --- ตรวจสอบ Withdraw Table (Database) กับ BorrowList Table -> และ Update ลง Database
        private void Sync_BorrowList(string Doc_No, string status_id)
        {
            DataTable dt = BorrowListDT;

            // --- ตรวจสอบการลงข้อมูล
            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("ไม่มีข้อมูลใน RequestListDT", "Sync Error", "error", "5000");
                return;
            }

            // ── Renumber RowNo เฉพาะ row ที่ไม่ใช่ Delete ──
            int no = 1;
            foreach (DataRow r in dt.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                r["RowNo"] = no++;
            }

            BorrowListDT = dt;

            int cntInsert = 0, cntUpdate = 0, cntDelete = 0, cntError = 0;

            conn.Open();
            try
            {
                foreach (DataRow row in dt.Rows)
                {
                    string compare = GetVal(row, "CompareData");

                    switch (compare)
                    {
                        case "Insert":
                            {
                                string sql = @"
                                    INSERT INTO Withdraw_List
                                    ( Doc_No, No, FixAsset, ControlNo,
                                      tool_id, sub_id, system_id, Use_Dept,
                                      Use_Model_Id, New_Model, HD_Model,
                                      Use_Cell_Id, New_Cell, HD_Cell,
                                      Use_Station_Id, New_Station, HD_Station,
                                      status_id , update_date )
                                    VALUES
                                    ( @Doc_No, @No, @FixAsset, @ControlNo,
                                      @tool_id, @sub_id, @system_id, @Use_Dept,
                                      @Use_Model_Id, @New_Model, @HD_Model,
                                      @Use_Cell_Id, @New_Cell, @HD_Cell,
                                      @Use_Station_Id, @New_Station, @HD_Station,
                                      @status_id , @update_date )";

                                using (SqlCommand cmd = new SqlCommand(sql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                                    cmd.Parameters.AddWithValue("@No", ToDbVal(row, "RowNo"));
                                    cmd.Parameters.AddWithValue("@FixAsset", ToDbVal(row, "FixAssetNo"));
                                    cmd.Parameters.AddWithValue("@ControlNo", ToDbVal(row, "ControlNo"));
                                    cmd.Parameters.AddWithValue("@tool_id", ToDbVal(row, "ToolId"));
                                    cmd.Parameters.AddWithValue("@sub_id", ToDbVal(row, "ToolSub"));
                                    cmd.Parameters.AddWithValue("@system_id", ToDbVal(row, "SystemId"));
                                    cmd.Parameters.AddWithValue("@Use_Dept", ToDbVal(row, "Use_DeptCode"));
                                    cmd.Parameters.AddWithValue("@Use_Model_Id", ToDbVal(row, "Use_ModelId"));
                                    cmd.Parameters.AddWithValue("@New_Model", ToDbVal(row, "Add_Model"));
                                    cmd.Parameters.AddWithValue("@HD_Model", ToDbVal(row, "HD_Model"));
                                    cmd.Parameters.AddWithValue("@Use_Cell_Id", ToDbVal(row, "Use_CellId"));
                                    cmd.Parameters.AddWithValue("@New_Cell", ToDbVal(row, "Add_Cell"));
                                    cmd.Parameters.AddWithValue("@HD_Cell", ToDbVal(row, "HD_Cell"));
                                    cmd.Parameters.AddWithValue("@Use_Station_Id", ToDbVal(row, "Use_StationId"));
                                    cmd.Parameters.AddWithValue("@New_Station", ToDbVal(row, "Add_Station"));
                                    cmd.Parameters.AddWithValue("@HD_Station", ToDbVal(row, "HD_Station"));
                                    cmd.Parameters.AddWithValue("@status_id", status_id);
                                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                                    cmd.ExecuteNonQuery();
                                    cntInsert++;
                                }
                                break;
                            }

                        case "Update":
                            {
                                string sql = @"
                                    UPDATE Withdraw_List SET
                                        No              = @No,
                                        Use_Dept        = @Use_Dept,
                                        Use_Model_Id    = @Use_Model_Id,
                                        New_Model       = @New_Model,
                                        HD_Model        = @HD_Model,
                                        Use_Cell_Id     = @Use_Cell_Id,
                                        New_Cell        = @New_Cell,
                                        HD_Cell         = @HD_Cell,
                                        Use_Station_Id  = @Use_Station_Id,
                                        New_Station     = @New_Station,
                                        HD_Station      = @HD_Station,
                                        status_id       = @status_id,
                                        update_date     = @update_date
                                    WHERE Doc_No    = @Doc_No
                                      AND tool_id   = @tool_id
                                      AND sub_id    = @sub_id
                                      AND system_id = @system_id";

                                using (SqlCommand cmd = new SqlCommand(sql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                                    cmd.Parameters.AddWithValue("@No", ToDbVal(row, "RowNo"));
                                    cmd.Parameters.AddWithValue("@tool_id", ToDbVal(row, "ToolId"));
                                    cmd.Parameters.AddWithValue("@sub_id", ToDbVal(row, "ToolSub"));
                                    cmd.Parameters.AddWithValue("@system_id", ToDbVal(row, "SystemId"));
                                    cmd.Parameters.AddWithValue("@Use_Dept", ToDbVal(row, "Use_DeptCode"));
                                    cmd.Parameters.AddWithValue("@Use_Model_Id", ToDbVal(row, "Use_ModelId"));
                                    cmd.Parameters.AddWithValue("@New_Model", ToDbVal(row, "Add_Model"));
                                    cmd.Parameters.AddWithValue("@HD_Model", ToDbVal(row, "HD_Model"));
                                    cmd.Parameters.AddWithValue("@Use_Cell_Id", ToDbVal(row, "Use_CellId"));
                                    cmd.Parameters.AddWithValue("@New_Cell", ToDbVal(row, "Add_Cell"));
                                    cmd.Parameters.AddWithValue("@HD_Cell", ToDbVal(row, "HD_Cell"));
                                    cmd.Parameters.AddWithValue("@Use_Station_Id", ToDbVal(row, "Use_StationId"));
                                    cmd.Parameters.AddWithValue("@New_Station", ToDbVal(row, "Add_Station"));
                                    cmd.Parameters.AddWithValue("@HD_Station", ToDbVal(row, "HD_Station"));
                                    cmd.Parameters.AddWithValue("@status_id", status_id);
                                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                                    cmd.ExecuteNonQuery();
                                    cntUpdate++;
                                }
                                break;
                            }

                        case "Delete":
                            {
                                string sql = @"
                                        DELETE FROM Withdraw_List
                                        WHERE Doc_No    = @Doc_No
                                          AND tool_id   = @tool_id
                                          AND sub_id    = @sub_id
                                          AND system_id = @system_id";

                                using (SqlCommand cmd = new SqlCommand(sql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                                    cmd.Parameters.AddWithValue("@tool_id", ToDbVal(row, "ToolId"));
                                    cmd.Parameters.AddWithValue("@sub_id", ToDbVal(row, "ToolSub"));
                                    cmd.Parameters.AddWithValue("@system_id", ToDbVal(row, "SystemId"));
                                    cmd.ExecuteNonQuery();
                                    cntDelete++;
                                }
                                break;
                            }

                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                cntError++;
                alert_toast("Sync Error : " + ex.Message, "Error", "error", "10000");
            }
            finally
            {
                conn.Close();

                if (cntError == 0)
                {
                    string alert_info = "";
                    switch (status_id)
                    {
                        case "1": alert_info = "Save Draft"; break;
                        case "50": alert_info = "Send Complete"; break;
                        case "99": alert_info = "Confirm Complete"; break;
                        case "100": alert_info = "Update Complete"; break;
                        case "999": alert_info = "Cancel Request"; break;
                    }

                    string link_path = (status_id == "99") ? "ApproveStatus.aspx" : "EquipmentStatus2.aspx";

                    alert_toast("Doc No : " + Doc_No, alert_info, "info", "2000");

                    string jsLink = HttpUtility.JavaScriptStringEncode(link_path);
                    string script =
                        "document.getElementById('loadingOverlay').style.display = 'block';" +
                        "setTimeout(function() { window.location.href = '" + jsLink + "'; }, 2000);";

                    ScriptManager.RegisterStartupScript(
                        this, this.GetType(), "redirectScript", script, true);
                }
            }
        }

        #endregion

        #region --- AAD Confirm : Modal + Status Use / ผู้รับผิดชอบ (อ้างอิง NotificationOfChange) ---

        /// <summary>master Status Use (tool_equipment_status)</summary>
        private DataTable GetStatusUseMaster()
        {
            if (_statusUseM != null) return _statusUseM;
            dc.QryDT_P(conn, "SELECT status_id, status_name FROM tool_equipment_status ORDER BY status_id");
            _statusUseM = dc.dt.Copy();
            return _statusUseM;
        }

        /// <summary>master ผู้รับผิดชอบทั้งหมด (กรองตาม Dept ในหน่วยความจำ)</summary>
        private DataTable GetResponsibleMaster()
        {
            if (_respM != null) return _respM;
            dc.QryDT_P(conn,
                "SELECT dept_code, pic_dept, emp_id, dept_code_real FROM Responsible_Dept" +
                " WHERE emp_id IS NOT NULL AND LTRIM(RTRIM(emp_id)) <> ''" +
                " ORDER BY dept_code, emp_id");
            _respM = dc.dt.Copy();
            return _respM;
        }

        /// <summary>ผู้รับผิดชอบของ Use Dept (WHERE dept_code = Use Dept)</summary>
        private List<DataRow> RespOfDept(string useDept)
        {
            var list = new List<DataRow>();
            string d = (useDept ?? "").Trim();
            if (d == "") return list;
            foreach (DataRow m in GetResponsibleMaster().Rows)
                if (m["dept_code"] != DBNull.Value && m["dept_code"].ToString().Trim() == d) list.Add(m);
            return list;
        }

        private string RespText(DataRow m)
        {
            string name = m["pic_dept"] == DBNull.Value ? "" : m["pic_dept"].ToString().Trim();
            string id = m["emp_id"].ToString().Trim();
            return name == "" ? id : name + " (" + id + ")";
        }

        /// <summary>Real Dept ของผู้รับผิดชอบ (Responsible_Dept.dept_code_real)
        ///          คนเดียวอยู่หลาย Dept → ใช้แถวของ Use Dept ก่อน ไม่มีค่อยใช้แถวแรก</summary>
        private string RealDeptOf(string empId, string useDept)
        {
            empId = (empId ?? "").Trim(); useDept = (useDept ?? "").Trim();
            if (empId == "") return "";
            string first = null;
            foreach (DataRow m in GetResponsibleMaster().Rows)
            {
                if (m["emp_id"].ToString().Trim() != empId) continue;
                string real = m["dept_code_real"] == DBNull.Value ? "" : m["dept_code_real"].ToString().Trim();
                if (m["dept_code"] != DBNull.Value && m["dept_code"].ToString().Trim() == useDept) return real;
                if (first == null) first = real;
            }
            return first ?? "";
        }

        /// <summary>แสดง Dept : ถ้า dept_code != dept_code_real → "5505 (5330)" (รูปแบบเดียวกับ NotificationOfChange)</summary>
        private static string FormatDeptDisplay(string deptCode, string real)
        {
            deptCode = (deptCode ?? "").Trim(); real = (real ?? "").Trim();
            if (real == "") return deptCode;
            if (deptCode == "") return real;
            return string.Equals(deptCode, real, StringComparison.OrdinalIgnoreCase) ? deptCode : deptCode + " (" + real + ")";
        }

        /// <summary>AAD Confirm ได้เฉพาะ mode=confirm และ status 50</summary>
        private bool IsAadEditable()
        {
            // ViewState["MODE"] ถูก reset ทุก Page_Load → อ่านจาก QueryString แทน
            string mode = (Request.QueryString["mode"] ?? "").ToLower();
            return mode == "confirm" && Convert.ToString(ViewState["STATUS_ID"]) == "50";
        }

        /// <summary>เพิ่มคอลัมน์ใน BorrowListDT (เผื่อ ViewState ของหน้าที่เปิดค้างไว้ก่อนอัปเดตโค้ด)</summary>
        private void EnsureAadCols(DataTable dt)
        {
            if (!dt.Columns.Contains("Use_StatusId")) dt.Columns.Add("Use_StatusId", typeof(string));
            if (!dt.Columns.Contains("Resp_EmpId")) dt.Columns.Add("Resp_EmpId", typeof(string));
        }

        private string RowKey(DataRow r)
        {
            return GetVal(r, "ToolId") + "|" + GetVal(r, "ToolSub") + "|" + GetVal(r, "SystemId");
        }

        private string ResolveName(DataTable master, string idCol, string nameCol, string id)
        {
            if (master == null || string.IsNullOrEmpty(id)) return "";
            foreach (DataRow r in master.Rows)
            {
                if (r[idCol] == DBNull.Value) continue;
                if (r[idCol].ToString().Trim() == id.Trim())
                    return r[nameCol] == DBNull.Value ? "" : r[nameCol].ToString().Trim();
            }
            return "";
        }

        private string DeptDisplay(string code)
        {
            string name = ResolveName((DataTable)ViewState["DEPT_MASTER"], "dept_code", "dept_name2", code);
            return name == "" ? code : code + " : " + name;
        }

        /// <summary>Status / ผู้รับผิดชอบ ปัจจุบันของอุปกรณ์ในรายการ (key = tool|sub|sys)
        ///          อ่านจาก Tool_Equipment_List_View ตาม key ในรายการ → ใช้ได้ทั้งเอกสารเดิม และเอกสารใหม่ที่ยังไม่มี Doc_No</summary>
        private Dictionary<string, DataRow> GetCurrentStatusResp(DataTable list)
        {
            var map = new Dictionary<string, DataRow>();
            var keys = new List<string>();
            foreach (DataRow r in list.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                string k = RowKey(r);
                if (k != "||" && !keys.Contains(k)) keys.Add(k);
            }
            if (keys.Count == 0) return map;

            // WHERE (tool_id=@t0 AND sub_no=@s0 AND sys_status=@y0) OR ... (ใช้ index ของ view ได้)
            var ps = new List<SqlParameter>();
            var conds = new List<string>();
            for (int i = 0; i < keys.Count; i++)
            {
                string[] p = keys[i].Split('|');
                conds.Add("(V.tool_id = @t" + i + " AND V.sub_no = @s" + i + " AND V.sys_status = @y" + i + ")");
                ps.Add(new SqlParameter("@t" + i, SqlDbType.NVarChar) { Value = p[0] });
                ps.Add(new SqlParameter("@s" + i, SqlDbType.NVarChar) { Value = p[1] });
                ps.Add(new SqlParameter("@y" + i, SqlDbType.NVarChar) { Value = p[2] });
            }

            string sql =
                "SELECT CAST(V.tool_id AS nvarchar(50)) + '|' + CAST(V.sub_no AS nvarchar(50)) + '|' +" +
                "       CAST(V.sys_status AS nvarchar(50)) AS k," +
                "       S.status_name, CAST(V.rsp_empID AS nvarchar(50)) AS rsp_empID, R.pic_dept" +
                " FROM Tool_Equipment_List_View V" +
                " LEFT JOIN tool_equipment_status S ON S.status_id = V.status_id" +
                " OUTER APPLY (SELECT TOP 1 pic_dept FROM Responsible_Dept RD" +
                "              WHERE LTRIM(RTRIM(RD.emp_id)) = LTRIM(RTRIM(CAST(V.rsp_empID AS nvarchar(50))))) R" +
                " WHERE " + string.Join(" OR ", conds.ToArray());

            dc.QryDT_P(conn, sql, ps.ToArray());

            if (dc.dt == null) return map;
            foreach (DataRow r in dc.dt.Copy().Rows)
            {
                string k = GetVal(r, "k");
                if (!map.ContainsKey(k)) map[k] = r;
            }
            return map;
        }

        /// <summary>ตาราง + Dropdown ใน Modal AAD Confirm</summary>
        private void BuildConfirmSummary()
        {
            DataTable src = BorrowListDT;
            EnsureAadCols(src);
            BorrowListDT = src;

            DataTable modelM = (DataTable)ViewState["MODEL_MASTER"];
            DataTable cellM = GetCellMaster().Copy();
            DataTable stationM = GetStationMaster().Copy();
            var cur = GetCurrentStatusResp(src);

            var dt = new DataTable();
            foreach (string c in new[] { "RowKey", "FixAssetNo", "ControlNo", "ToolNo",
                                         "OldDept", "OldModel", "OldCell", "OldStation", "OldStatus", "OldResp",
                                         "NewDept", "NewModel", "NewCell", "NewStation",
                                         "UseDept", "StatusId", "RespEmpId",
                                         "RuleName", "RuleStatusId", "RuleResp", "RuleRespNames" })
                dt.Columns.Add(c, typeof(string));
            dt.Columns.Add("RowNo", typeof(int));

            // กฎ Auto Assign (Equipment_Setting) : cache ต่อชุด Dept|Cell|Station
            var ruleCache = new Dictionary<string, DataTable>();

            int no = 1;
            foreach (DataRow r in src.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;

                DataRow c;
                cur.TryGetValue(RowKey(r), out c);

                DataRow n = dt.NewRow();
                n["RowKey"] = RowKey(r);
                n["RowNo"] = no++;
                n["FixAssetNo"] = GetVal(r, "FixAssetNo");
                n["ControlNo"] = GetVal(r, "ControlNo");
                n["ToolNo"] = GetVal(r, "ToolNo");

                n["OldDept"] = GetVal(r, "DeptCode");
                n["OldModel"] = GetVal(r, "ModelName");
                n["OldCell"] = GetVal(r, "CellName");
                n["OldStation"] = GetVal(r, "StationName");
                n["OldStatus"] = c == null ? "" : GetVal(c, "status_name");
                string rspId = c == null ? "" : GetVal(c, "rsp_empID");
                string rspNm = c == null ? "" : GetVal(c, "pic_dept");
                n["OldResp"] = rspId == "" ? "" : (rspNm == "" ? rspId : rspNm + " (" + rspId + ")");

                n["NewDept"] = DeptDisplay(GetVal(r, "Use_DeptCode"));
                n["NewModel"] = GetVal(r, "HD_Model") == "txt" ? GetVal(r, "Add_Model")
                              : ResolveName(modelM, "model_id", "model_name", GetVal(r, "Use_ModelId"));
                n["NewCell"] = GetVal(r, "HD_Cell") == "txt" ? GetVal(r, "Add_Cell")
                              : ResolveName(cellM, "location2_id", "location2_name", GetVal(r, "Use_CellId"));
                n["NewStation"] = GetVal(r, "HD_Station") == "txt" ? GetVal(r, "Add_Station")
                              : ResolveName(stationM, "location3_id", "location3_name", GetVal(r, "Use_StationId"));

                n["UseDept"] = GetVal(r, "Use_DeptCode");
                n["StatusId"] = GetVal(r, "Use_StatusId");
                n["RespEmpId"] = GetVal(r, "Resp_EmpId");

                // ── กฎ Auto Assign : จับคู่ด้วย ID (Cell/Station ที่พิมพ์เอง = ไม่มี ID → ใช้แค่ Dept) ──
                string cellId = GetVal(r, "HD_Cell") == "txt" ? "" : GetVal(r, "Use_CellId");
                string staId = GetVal(r, "HD_Station") == "txt" ? "" : GetVal(r, "Use_StationId");
                string ck = GetVal(r, "Use_DeptCode") + "|" + cellId + "|" + staId;
                DataTable rule;
                if (!ruleCache.TryGetValue(ck, out rule))
                {
                    rule = ResolveAutoAssign(GetVal(r, "Use_DeptCode"), cellId, staId);
                    ruleCache[ck] = rule;
                }
                if (rule.Rows.Count > 0)
                {
                    var ids = new List<string>();
                    var names = new List<string>();
                    foreach (DataRow rr in rule.Rows)
                    {
                        string id = GetVal(rr, "emp_id");
                        if (id == "" || ids.Contains(id)) continue;
                        string nm = GetVal(rr, "pic_dept");
                        ids.Add(id);
                        names.Add(nm == "" ? id : nm + " (" + id + ")");
                    }
                    n["RuleName"] = GetVal(rule.Rows[0], "rule_name");
                    n["RuleStatusId"] = GetVal(rule.Rows[0], "status_use_id");
                    n["RuleResp"] = string.Join("|", ids.ToArray());
                    n["RuleRespNames"] = string.Join("|", names.ToArray());
                }
                dt.Rows.Add(n);
            }

            string doc = litIssueNo.Text.Trim();
            litCfmDoc.Text = Server.HtmlEncode(doc == "" || doc == "-" ? "(เอกสารใหม่)" : doc);
            litCfmSummary.Text =
                "<div class='wcf-summary'><b>สรุป :</b>" +
                "<span class='wcf-badge wcf-ok'>✓ เบิกอุปกรณ์ " + dt.Rows.Count + " รายการ</span></div>";

            gvConfirmSummary.DataSource = dt;
            gvConfirmSummary.DataBind();
            if (gvConfirmSummary.HeaderRow != null)
                gvConfirmSummary.HeaderRow.TableSection = TableRowSection.TableHeader;   // ให้ CSS thead ทำงาน
        }

        /// <summary>bind Dropdown Status Use / ผู้รับผิดชอบ ในแถว Modal</summary>
        protected void gvConfirmSummary_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            DataRowView drv = (DataRowView)e.Row.DataItem;

            var ddlSt = e.Row.FindControl("ddlStatusUse") as DropDownList;
            var ddlRp = e.Row.FindControl("ddlResponsible") as DropDownList;
            if (ddlSt == null || ddlRp == null) return;

            ddlSt.Items.Clear();
            ddlRp.Items.Clear();

            // ── Status Use : ค่าเริ่มต้นตาม Use Dept (5505 = Spare , อื่น = Use) ──
            ddlSt.Items.Add(new ListItem("-- Select --", ""));
            foreach (DataRow m in GetStatusUseMaster().Rows)
                ddlSt.Items.Add(new ListItem(m["status_name"].ToString().Trim(), m["status_id"].ToString().Trim()));

            // ค่าที่เลือกไว้แล้ว → กฎ Auto Assign → ค่าเดิมของระบบ (5505 = Spare , อื่น = Use)
            string ruleName = Convert.ToString(drv["RuleName"]);
            string ruleSt = Convert.ToString(drv["RuleStatusId"]);
            string st = Convert.ToString(drv["StatusId"]);
            if (st == "" && ruleSt != "") st = ruleSt;
            if (st == "") st = DefaultStatusUse(Convert.ToString(drv["UseDept"]));
            if (ddlSt.Items.FindByValue(st) != null) ddlSt.SelectedValue = st;

            // ── ผู้รับผิดชอบ : คนตามกฎ (⭐ แนะนำ) อยู่บนสุด + รายชื่อทั้งหมดของ Use Dept ต่อท้าย ──
            string[] ruleIds = Convert.ToString(drv["RuleResp"]).Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            string[] ruleNames = Convert.ToString(drv["RuleRespNames"]).Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < ruleIds.Length; i++)
                ddlRp.Items.Add(new ListItem("⭐ " + (i < ruleNames.Length ? ruleNames[i] : ruleIds[i]) + " — แนะนำ", ruleIds[i]));

            foreach (DataRow m in RespOfDept(Convert.ToString(drv["UseDept"])))
            {
                string id = m["emp_id"].ToString().Trim();
                if (ddlRp.Items.FindByValue(id) == null)
                    ddlRp.Items.Add(new ListItem(RespText(m), id));
            }

            string rp = Convert.ToString(drv["RespEmpId"]);
            if (rp != "" && ddlRp.Items.FindByValue(rp) == null)
                ddlRp.Items.Insert(0, new ListItem(rp, rp));
            if (ddlRp.Items.Count == 0)
                ddlRp.Items.Add(new ListItem("-- ไม่มีผู้รับผิดชอบของ Dept นี้ --", ""));

            if (rp != "") ddlRp.SelectedValue = rp;
            else ddlRp.SelectedIndex = 0;          // มีกฎ = คนแรกของกฎ (Auto) / ไม่มีกฎ = คนแรกของ Dept

            // ── หมายเหตุใต้ Dropdown : มาจากกฎไหน ──
            var note = e.Row.FindControl("litRuleNote") as Literal;
            if (note != null)
            {
                if (ruleName != "")
                    note.Text = "<div class='rule-note hit' title='ตั้งค่าที่ System Setup'>🤖 กฎ : " + Server.HtmlEncode(ruleName) +
                                (ruleIds.Length > 1 ? " · <b>แนะนำ " + ruleIds.Length + " คน</b> (เลือกคนแรกให้แล้ว)" :
                                 ruleIds.Length == 1 ? " · เลือกให้อัตโนมัติ" : " · ไม่กำหนดผู้รับผิดชอบ") + "</div>";
                else
                    note.Text = "<div class='rule-note'>ไม่ตรงกฎ Auto Assign — ใช้รายชื่อตาม Dept</div>";
            }

            // ── Use : Dept = "Use Dept (Real Dept ของผู้รับผิดชอบที่เลือก)" + เปลี่ยนตามเมื่อเลือกคนใหม่ (JS) ──
            string useDept = Convert.ToString(drv["UseDept"]);
            var realMap = new StringBuilder("{");
            foreach (ListItem li in ddlRp.Items)
            {
                if (li.Value == "") continue;
                if (realMap.Length > 1) realMap.Append(",");
                realMap.Append("\"" + HttpUtility.JavaScriptStringEncode(li.Value) + "\":\"" +
                               HttpUtility.JavaScriptStringEncode(RealDeptOf(li.Value, useDept)) + "\"");
            }
            realMap.Append("}");
            ddlRp.Attributes["data-real"] = realMap.ToString();
            ddlRp.Attributes["data-dept"] = useDept;

            var litUse = e.Row.FindControl("litUseDept") as Literal;
            if (litUse != null)
            {
                string deptName = ResolveName((DataTable)ViewState["DEPT_MASTER"], "dept_code", "dept_name2", useDept);
                litUse.Text = "<b class='ud-code'>" + Server.HtmlEncode(FormatDeptDisplay(useDept, RealDeptOf(ddlRp.SelectedValue, useDept))) + "</b>" +
                              (deptName == "" ? "" : "<br/><small>" + Server.HtmlEncode(deptName) + "</small>");
            }

            // ช่องที่ต้องตรวจ = สีเหลือง / ยังไม่มีค่า = ขอบแดง
            ddlSt.CssClass = "form-control aad-ddl" + (ddlSt.SelectedValue == "" ? " aad-ddl-empty" : "");
            ddlRp.CssClass = "form-control aad-ddl" + (ddlRp.SelectedValue == "" ? " aad-ddl-empty" : "");
        }

        /// <summary>อ่าน Dropdown ใน Modal → BorrowListDT + ตรวจครบ</summary>
        private bool ReadAndValidateConfirmModal()
        {
            DataTable dt = BorrowListDT;
            EnsureAadCols(dt);
            var msgs = new List<string>();
            int no = 0;

            foreach (GridViewRow gr in gvConfirmSummary.Rows)
            {
                if (gr.RowType != DataControlRowType.DataRow) continue;
                no++;

                var hf = gr.FindControl("hfCfmKey") as HiddenField;
                var ddlSt = gr.FindControl("ddlStatusUse") as DropDownList;
                var ddlRp = gr.FindControl("ddlResponsible") as DropDownList;
                if (hf == null || ddlSt == null || ddlRp == null) continue;

                DataRow row = null;
                foreach (DataRow r in dt.Rows)
                    if (GetVal(r, "CompareData") != "Delete" && RowKey(r) == hf.Value) { row = r; break; }
                if (row == null) continue;

                row["Use_StatusId"] = ddlSt.SelectedValue == "" ? (object)DBNull.Value : ddlSt.SelectedValue;
                row["Resp_EmpId"] = ddlRp.SelectedValue == "" ? (object)DBNull.Value : ddlRp.SelectedValue;

                bool noSt = ddlSt.SelectedValue == "";
                bool noRp = ddlRp.SelectedValue == "";
                ddlSt.CssClass = "form-control aad-ddl" + (noSt ? " aad-ddl-empty" : "");
                ddlRp.CssClass = "form-control aad-ddl" + (noRp ? " aad-ddl-empty" : "");

                if (noSt || noRp)
                    msgs.Add("• รายการที่ " + no + (GetVal(row, "FixAssetNo") != "" ? " (" + GetVal(row, "FixAssetNo") + ")" : "") + " : " +
                             (noSt ? "ยังไม่เลือก Status Use " : "") +
                             (noRp ? "ไม่มีผู้รับผิดชอบของ Use Dept " + GetVal(row, "Use_DeptCode") + " (ตรวจตาราง Responsible_Dept)" : ""));
            }
            BorrowListDT = dt;

            if (msgs.Count == 0) return true;
            alert_toast("กรุณาเลือกข้อมูลให้ครบ (ช่องสีเหลือง) :\n" + string.Join("\n", msgs.ToArray()),
                        "ข้อมูล AAD Confirm ไม่ครบ", "warn", "15000");
            return false;
        }

        private void ReopenConfirmModal()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "reopenConfirm",
                "document.getElementById('" + pnlConfirmModal.ClientID + "').style.display='flex';", true);
        }

        /// <summary>กฎ Auto Assign (หน้า Equipment_Setting) → sp_AutoAssign_Resolve
        ///          คืน 1 แถวต่อผู้รับผิดชอบ (เรียง seq) / ไม่ตรงกฎ หรือยังไม่ได้ติดตั้ง SP = ตารางว่าง (ใช้ค่าเดิม)</summary>
        private DataTable ResolveAutoAssign(string dept, string cellId, string stationId)
        {
            var dt = new DataTable();
            if ((dept ?? "").Trim() == "" && (cellId ?? "") == "" && (stationId ?? "") == "") return dt;
            try
            {
                using (var c = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString))
                using (var cmd = new SqlCommand("sp_AutoAssign_Resolve", c))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@dept", (dept ?? "").Trim());
                    cmd.Parameters.AddWithValue("@cell_id", string.IsNullOrEmpty(cellId) ? (object)DBNull.Value : cellId);
                    cmd.Parameters.AddWithValue("@station_id", string.IsNullOrEmpty(stationId) ? (object)DBNull.Value : stationId);
                    using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                // ไม่ให้ AAD Confirm ใช้งานไม่ได้เพราะกฎ → กลับไปใช้ค่าเดิม
                System.Diagnostics.Debug.WriteLine("[AutoAssign] " + ex.Message);
                dt = new DataTable();
            }
            return dt;
        }

        /// <summary>ค่าเริ่มต้น Status Use : ปลายทาง 5505 → Spare , Dept อื่น → Use</summary>
        private string DefaultStatusUse(string useDept)
        {
            return (useDept ?? "").Trim() == "5505" ? STATUS_SPARE : STATUS_USE;
        }

        /// <summary>แถบนำทาง + ปุ่มคู่มือ (Status 50 = AAD Confirm) — รูปแบบเดียวกับ borrow_tool2</summary>
        private string BuildModeGuide(string file)
        {
            string title = "ขั้นตอน AAD Confirm";
            string btnText = "คู่มือ AAD Confirm";
            string[] steps = { "ตรวจข้อมูลผู้ร้องขอและรายการเบิก",
                               "ตรวจสถานที่นำไปใช้งาน (Use : Model / Cell / Station)",
                               "กด AAD Confirm แล้วยืนยัน Status Use / ผู้รับผิดชอบ — ไม่อนุมัติกด Reject (เหตุผล ≥ 10 ตัวอักษร)" };

            var sb = new StringBuilder("<div class='mode-guide'>");
            sb.Append("<span class='mg-title'>" + Server.HtmlEncode(title) + "</span><ol>");
            foreach (string s in steps) sb.Append("<li>" + Server.HtmlEncode(s) + "</li>");
            sb.Append("</ol>");
            sb.Append("<a href='#' class='mg-btn' onclick=\"return openManualFile('" + file + "');\">" +
                      "<span class='glyphicon glyphicon-book'></span> " + Server.HtmlEncode(btnText) + "</a>");
            sb.Append("</div>");
            return sb.ToString();
        }

        #endregion

        #region --- History_Change_Location : บันทึกตอน AAD Confirm (→ 99 Wait Update) ---

        private static object IntOrNull(string v)
        {
            int n;
            return int.TryParse((v ?? "").Trim(), out n) ? (object)n : DBNull.Value;
        }

        /// <summary>
        /// บันทึกประวัติเปลี่ยน Location 1 แถวต่อรายการ (รูปแบบเดียวกับหน้า EditLocation)
        ///   เดิม (Old_*) : Dept / Model / Cell / Station จาก Request List ฝั่ง Current
        ///                  Status / ผู้รับผิดชอบ / Location1 → snapshot จาก Tool_Equipment_List_View ณ ตอนยืนยัน
        ///   ใหม่ (New_*) : ฝั่ง Use ของ Request List + Status Use / ผู้รับผิดชอบ ที่เลือกใน Modal
        ///   ผู้รับผิดชอบเก็บแบบ "empId - ชื่อ" (เหมือน EditLocation) , Doc_No = เลขเอกสารเบิก
        /// ใช้ connection / transaction เดียวกับการยืนยัน → บันทึกไม่ได้ = ยืนยันไม่สำเร็จทั้งเอกสาร
        /// </summary>
        private void InsertLocationHistory_Tx(SqlConnection c, SqlTransaction tx, DataTable dt, string Doc_No, string editBy)
        {
            const string sql = @"
                INSERT INTO tool_eduipment_system.dbo.History_Change_Location
                (Tool_id, Tool_sub, System_id, Fix_Asset_No, Control_No, Tool_No,
                 Old_Status_Id, New_Status_Id,
                 Old_DeptCode, New_DeptCode,
                 Old_Responsible, New_Responsible,
                 Old_Model_Id, Old_Model_Name, New_Model_Id, New_Model_Text, HD_Model,
                 Old_Cell_Id, Old_Cell_Name, New_Cell_Id, New_Cell_Text, HD_Cell,
                 Old_Station_Id, Old_Station_Name, New_Station_Id, New_Station_Text, HD_Station,
                 Old_Location1_id, Edit_By, Edit_Date, Remark, Doc_No)
                SELECT
                 @ToolId, @ToolSub, @SystemId, @FixAsset, @ControlNo, @ToolNo,
                 V.status_id, @NewStatusId,
                 @OldDept, @NewDept,
                 CASE WHEN ISNULL(LTRIM(RTRIM(CAST(V.rsp_empID AS nvarchar(50)))), '') = '' THEN NULL
                      WHEN ISNULL(RO.pic_dept, '') = '' THEN LTRIM(RTRIM(CAST(V.rsp_empID AS nvarchar(50))))
                      ELSE LTRIM(RTRIM(CAST(V.rsp_empID AS nvarchar(50)))) + ' - ' + RO.pic_dept END,
                 CASE WHEN ISNULL(@NewResp, '') = '' THEN NULL
                      WHEN ISNULL(RN.pic_dept, '') = '' THEN @NewResp
                      ELSE @NewResp + ' - ' + RN.pic_dept END,
                 @OldModelId, @OldModelName,
                 CASE WHEN @HDModel = 'txt' THEN NULL ELSE @NewModelId END,
                 CASE WHEN @HDModel = 'txt' THEN @AddModel ELSE M.model_name END, @HDModel,
                 @OldCellId, @OldCellName,
                 CASE WHEN @HDCell = 'txt' THEN NULL ELSE @NewCellId END,
                 CASE WHEN @HDCell = 'txt' THEN @AddCell ELSE L2.location2_name END, @HDCell,
                 @OldStationId, @OldStationName,
                 CASE WHEN @HDStation = 'txt' THEN NULL ELSE @NewStationId END,
                 CASE WHEN @HDStation = 'txt' THEN @AddStation ELSE L3.location3_name END, @HDStation,
                 V.location1_id, @EditBy, GETDATE(), @Remark, @DocNo
                FROM (SELECT 1 AS x) D
                OUTER APPLY (SELECT TOP 1 status_id, rsp_empID, location1_id FROM Tool_Equipment_List_View
                             WHERE tool_id = @ToolId AND sub_no = @ToolSub AND sys_status = @SystemId) V
                OUTER APPLY (SELECT TOP 1 pic_dept FROM Responsible_Dept
                             WHERE LTRIM(RTRIM(emp_id)) = LTRIM(RTRIM(CAST(V.rsp_empID AS nvarchar(50))))) RO
                OUTER APPLY (SELECT TOP 1 pic_dept FROM Responsible_Dept
                             WHERE LTRIM(RTRIM(emp_id)) = @NewResp) RN
                OUTER APPLY (SELECT TOP 1 model_name FROM jig_tool_system.dbo.draw_master_model
                             WHERE CAST(model_id AS nvarchar(50)) = @NewModelId) M
                OUTER APPLY (SELECT TOP 1 location2_name FROM jig_tool_system.dbo.master_location2
                             WHERE location2_id = @NewCellId) L2
                OUTER APPLY (SELECT TOP 1 location3_name FROM jig_tool_system.dbo.master_location3
                             WHERE location3_id = @NewStationId) L3;";

            string remark = Cut("เบิกอุปกรณ์ " + Doc_No +
                                (txt_borrowComment.Text.Trim() == "" ? "" : " : " + txt_borrowComment.Text.Trim()), 500);

            foreach (DataRow r in dt.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;

                string hdModel = GetVal(r, "HD_Model") == "txt" ? "txt" : "ddl";
                string hdCell = GetVal(r, "HD_Cell") == "txt" ? "txt" : "ddl";
                string hdStation = GetVal(r, "HD_Station") == "txt" ? "txt" : "ddl";

                using (var cmd = new SqlCommand(sql, c, tx))
                {
                    cmd.Parameters.Add("@ToolId", SqlDbType.NVarChar, 50).Value = GetVal(r, "ToolId");
                    cmd.Parameters.Add("@ToolSub", SqlDbType.NVarChar, 50).Value = GetVal(r, "ToolSub");
                    cmd.Parameters.Add("@SystemId", SqlDbType.NVarChar, 10).Value = GetVal(r, "SystemId");
                    cmd.Parameters.Add("@FixAsset", SqlDbType.NVarChar, 100).Value = DbStr(GetVal(r, "FixAssetNo"), 100);
                    cmd.Parameters.Add("@ControlNo", SqlDbType.NVarChar, 100).Value = DbStr(GetVal(r, "ControlNo"), 100);
                    cmd.Parameters.Add("@ToolNo", SqlDbType.NVarChar, 200).Value = DbStr(GetVal(r, "ToolNo"), 200);

                    cmd.Parameters.Add("@NewStatusId", SqlDbType.Int).Value = IntOrNull(GetVal(r, "Use_StatusId"));
                    cmd.Parameters.Add("@OldDept", SqlDbType.NVarChar, 50).Value = DbStr(GetVal(r, "DeptCode"), 50);
                    cmd.Parameters.Add("@NewDept", SqlDbType.NVarChar, 50).Value = DbStr(GetVal(r, "Use_DeptCode"), 50);
                    cmd.Parameters.Add("@NewResp", SqlDbType.NVarChar, 50).Value = Cut(GetVal(r, "Resp_EmpId"), 50);

                    cmd.Parameters.Add("@OldModelId", SqlDbType.NVarChar, 50).Value = DbStr(GetVal(r, "ModelId"), 50);
                    cmd.Parameters.Add("@OldModelName", SqlDbType.NVarChar, 200).Value = DbStr(GetVal(r, "ModelName"), 200);
                    cmd.Parameters.Add("@NewModelId", SqlDbType.NVarChar, 50).Value = DbStr(GetVal(r, "Use_ModelId"), 50);
                    cmd.Parameters.Add("@AddModel", SqlDbType.NVarChar, 200).Value = DbStr(GetVal(r, "Add_Model"), 200);
                    cmd.Parameters.Add("@HDModel", SqlDbType.NVarChar, 10).Value = hdModel;

                    cmd.Parameters.Add("@OldCellId", SqlDbType.Int).Value = IntOrNull(GetVal(r, "CellId"));
                    cmd.Parameters.Add("@OldCellName", SqlDbType.NVarChar, 200).Value = DbStr(GetVal(r, "CellName"), 200);
                    cmd.Parameters.Add("@NewCellId", SqlDbType.Int).Value = IntOrNull(GetVal(r, "Use_CellId"));
                    cmd.Parameters.Add("@AddCell", SqlDbType.NVarChar, 200).Value = DbStr(GetVal(r, "Add_Cell"), 200);
                    cmd.Parameters.Add("@HDCell", SqlDbType.NVarChar, 10).Value = hdCell;

                    cmd.Parameters.Add("@OldStationId", SqlDbType.Int).Value = IntOrNull(GetVal(r, "StationId"));
                    cmd.Parameters.Add("@OldStationName", SqlDbType.NVarChar, 200).Value = DbStr(GetVal(r, "StationName"), 200);
                    cmd.Parameters.Add("@NewStationId", SqlDbType.Int).Value = IntOrNull(GetVal(r, "Use_StationId"));
                    cmd.Parameters.Add("@AddStation", SqlDbType.NVarChar, 200).Value = DbStr(GetVal(r, "Add_Station"), 200);
                    cmd.Parameters.Add("@HDStation", SqlDbType.NVarChar, 10).Value = hdStation;

                    cmd.Parameters.Add("@EditBy", SqlDbType.NVarChar, 50).Value = Cut(editBy, 50);
                    cmd.Parameters.Add("@Remark", SqlDbType.NVarChar, 500).Value = remark;
                    cmd.Parameters.Add("@DocNo", SqlDbType.NVarChar, 30).Value = Doc_No;

                    if (cmd.ExecuteNonQuery() != 1)
                        throw new InvalidOperationException("บันทึกประวัติเปลี่ยน Location (History_Change_Location) ไม่สำเร็จ : " + GetVal(r, "ControlNo"));
                }
            }
        }

        #endregion

        #region --- Requester Snapshot (Requester_Info) — รูปแบบเดียวกับ borrow_tool2 ---

        private static string Cut(string s, int max)
        {
            s = (s ?? "").Trim();
            return s.Length > max ? s.Substring(0, max) : s;
        }

        private static object DbStr(string s, int max)
        {
            string v = Cut(s, max);
            return v == "" ? (object)DBNull.Value : v;
        }

        /// <summary>
        /// บันทึกข้อมูล Requester ลง Requester_Info (ใช้ connection / transaction ที่ส่งมา — tx = null ได้)
        ///   overwrite = true  : Save Draft / Send / Confirm → เขียนทับ (ข้อมูล ณ วันที่ส่งล่าสุด)
        ///   overwrite = false : เปิดเอกสารเก่า → เพิ่มเฉพาะที่ยังไม่มี หรือแถว BACKFILL
        /// ข้ามเมื่อข้อมูลบนหน้าจอไม่ได้มาจาก HRMS (กันเขียนทับด้วย Snapshot เอง)
        /// </summary>
        private void SaveRequesterInfo(SqlConnection c, SqlTransaction tx, string Doc_No, bool overwrite, string tel)
        {
            string empId = txtEmpId.Text.Trim();
            if (string.IsNullOrEmpty(Doc_No) || Doc_No == "-") return;
            if (empId == "" || txtEmpName.Text.Trim() == "") return;
            if (Convert.ToString(ViewState["REQ_SOURCE"]) != "HRMS") return;

            const string sql = @"
                UPDATE dbo.Requester_Info
                   SET emp_id = @emp, emp_name = @name, position_name = @pos, band = @band,
                       dept_code = @dc, dept_name = @dn, email = @mail, tel = @tel,
                       source = 'HRMS', update_date = GETDATE()
                 WHERE Doc_No = @doc AND (@ow = 1 OR source = 'BACKFILL');

                IF NOT EXISTS (SELECT 1 FROM dbo.Requester_Info WHERE Doc_No = @doc)
                    INSERT INTO dbo.Requester_Info
                          (Doc_No, Doc_Type, emp_id, emp_name, position_name, band,
                           dept_code, dept_name, email, tel, source, create_date)
                    VALUES (@doc, @type, @emp, @name, @pos, @band,
                           @dc, @dn, @mail, @tel, 'HRMS', GETDATE());";

            using (var cmd = new SqlCommand(sql, c, tx))
            {
                cmd.Parameters.Add("@doc", SqlDbType.NVarChar, 30).Value = Doc_No;
                cmd.Parameters.Add("@type", SqlDbType.NVarChar, 10).Value = DOC_TYPE;
                cmd.Parameters.Add("@emp", SqlDbType.NVarChar, 10).Value = Cut(empId, 10);
                cmd.Parameters.Add("@name", SqlDbType.NVarChar, 150).Value = DbStr(txtEmpName.Text, 150);
                cmd.Parameters.Add("@pos", SqlDbType.NVarChar, 150).Value = DbStr(txtPosition.Text, 150);
                cmd.Parameters.Add("@band", SqlDbType.NVarChar, 20).Value = DbStr(hdtxtBand.Value, 20);
                cmd.Parameters.Add("@dc", SqlDbType.NVarChar, 10).Value = DbStr(txtDeptCode.Text, 10);
                cmd.Parameters.Add("@dn", SqlDbType.NVarChar, 100).Value = DbStr(txtDept.Text, 100);
                cmd.Parameters.Add("@mail", SqlDbType.NVarChar, 150).Value = DbStr(hdtxtEmail.Value, 150);
                cmd.Parameters.Add("@tel", SqlDbType.NVarChar, 50).Value = DbStr(tel ?? txtTel.Text, 50);
                cmd.Parameters.Add("@ow", SqlDbType.Bit).Value = overwrite;
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>บันทึกแบบแยก connection — ถ้าพังไม่ให้กระทบการบันทึกเอกสาร</summary>
        private void SaveRequesterInfo_Safe(string Doc_No, bool overwrite, string tel)
        {
            try
            {
                using (var c = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString))
                {
                    c.Open();
                    SaveRequesterInfo(c, null, Doc_No, overwrite, tel);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Requester_Info] " + Doc_No + " : " + ex.Message);
            }
        }

        /// <summary>เปิดเอกสารเก่า + เจอใน HRMS → เติม Snapshot ถ้ายังไม่มี</summary>
        private void SaveRequesterInfo_IfMissing(string Doc_No, string tel)
        {
            SaveRequesterInfo_Safe(Doc_No, false, tel);
        }

        /// <summary>อ่าน Snapshot ลงหน้าจอ — คืน true ถ้าพบ</summary>
        private bool LoadRequesterSnapshot(string Doc_No, out DateTime? savedAt)
        {
            savedAt = null;

            dc.QryDT_P(conn,
                "SELECT emp_name, position_name, band, dept_code, dept_name, email, source," +
                "       ISNULL(update_date, create_date) AS saved_at" +
                " FROM Requester_Info WHERE Doc_No = @d",
                new SqlParameter("@d", SqlDbType.NVarChar) { Value = Doc_No });

            if (dc.dt == null || dc.dt.Rows.Count == 0) return false;

            DataRow r = dc.dt.Rows[0];
            txtEmpName.Text = GetVal(r, "emp_name");
            txtPosition.Text = GetVal(r, "position_name");
            txtDept.Text = GetVal(r, "dept_name");
            txtDeptCode.Text = GetVal(r, "dept_code");
            hdtxtBand.Value = GetVal(r, "band");
            hdtxtEmail.Value = GetVal(r, "email");
            if (r["saved_at"] != DBNull.Value) savedAt = Convert.ToDateTime(r["saved_at"]);

            ViewState["REQ_SOURCE"] = "SNAPSHOT";
            ViewState["REQ_SNAP_TYPE"] = GetVal(r, "source");   // HRMS / BACKFILL

            // ช่องที่มาจาก Snapshot → สีอำพัน
            foreach (TextBox t in new[] { txtEmpName, txtPosition, txtDept, txtDeptCode })
                if (!t.CssClass.Contains("fld-snapshot")) t.CssClass = (t.CssClass + " fld-snapshot").Trim();

            return true;
        }

        #endregion

        #region --- Get Master ---

        private DataTable GetDeptMaster()
        {
            DataTable dt_dept = new DataTable();

            string str_dept = " Select LTRIM(RTRIM(dept_code)) as dept_code , dept_name2" +
                              " From View_dept_master Where division = 2" +
                              " order by dept_code";
            dc.QryDT(conn, str_dept);
            dt_dept = dc.dt;
            return dt_dept;
        }
        private DataTable GetModelMaster()
        {
            DataTable dt_model = new DataTable();

            string str_model = "Select * From jig_tool_system.dbo.draw_master_model where model_check = 1 order by model_name";
            dc.QryDT(conn, str_model);
            dt_model = dc.dt;
            return dt_model;
        }
        private void GetReasonBorrowMaster()
        {
            DataTable dt_reasonBorrow = new DataTable();

            string str_reasonBorrow = " select reason_ID , reason_Name , withdrawFlow , comment_withdraw , order_withdraw" +
                                      " from Reason_Borrow_Master" +
                                      " Where withdrawFlow = 'true'" +
                                      " order by order_withdraw";
            dc.QryDT(conn, str_reasonBorrow);
            dt_reasonBorrow = dc.dt;

            //DataTable modelMaster = (DataTable)ViewState["MODEL_MASTER"];
            ddlReason_Borrow.DataSource = dt_reasonBorrow;
            ddlReason_Borrow.DataTextField = "reason_Name";
            ddlReason_Borrow.DataValueField = "reason_ID";
            ddlReason_Borrow.DataBind();

            // เพิ่ม -- Select --
            ddlReason_Borrow.Items.Insert(0, new ListItem("-- Select --", ""));
        }
        private DataTable GetCellMaster()
        {
            string str_cell =
                "select * from jig_tool_system.dbo.master_location2" +
                " where is_use = 1 and location1_id = 97 and location2_id != 0" +
                " order by location2_name";

            dc.QryDT_P(conn, str_cell);
            return dc.dt;
        }
        private DataTable GetStationMaster()
        {
            string str_station =
                "select * from jig_tool_system.dbo.master_location3" +
                " where location1_id = 97 and location3_id != 0" +
                " order by location3_name";

            dc.QryDT_P(conn, str_station);
            return dc.dt;
        }

        #endregion
        
        #region --- Page Mode : Condition ---
        
        private void SetViewMode(string status_id)
        {
            // --- Show Text Mode
            lb_mode.Text = "Mode : Viewer";

            //Hide Step
            step_requester.Visible = false;
            step_borrowlist.Visible = false;
            step_comment.Visible = false;

            // Disable Search & Add list Panel
            pnlSearch_Add.Visible = false;

            //Gridview Borrow List
            gvBorrowList.Columns[1].Visible = false;

            //Hide Button
            if (status_id == "99" || status_id == "100" || status_id == "999") //During Process , Completed , Cancel
            {
                if (ButtonBar != null) ButtonBar.Visible = false;
            }
            else
            {
                if (CancelButton != null) CancelButton.Visible = true;
                if (DraftButton != null) DraftButton.Visible = false;
                if (RejectButton != null) RejectButton.Visible = false;
                if (SendButton != null) SendButton.Visible = false;
                if (ConfirmButton != null) ConfirmButton.Visible = false;
                if (ConfirmButton2 != null) ConfirmButton2.Visible = false;
            }

            //Comment
            ddlReason_Borrow.Enabled = false;
            txt_borrowComment.Enabled = false;

            //Borrow Note
            borrow_note.Visible = true;

            //Show Comment History
            ShowRejectHistoryIfAny();
        }
        private void SetConfirmMode(string status_id)
        {
            // --- Show Text Mode
            lb_mode.Text = "Mode : Approve";

            //Hide Step
            step_requester.Visible = false;
            step_borrowlist.Visible = false;
            step_comment.Visible = false;

            // Disable Search & Add list Panel
            pnlSearch_Add.Visible = false;

            //Gridview Borrow List
            gvBorrowList.Columns[1].Visible = false;

            //Hide Button
            if (CancelButton != null) CancelButton.Visible = true;
            if (DraftButton != null) DraftButton.Visible = false;
            if (RejectButton != null) RejectButton.Visible = true;
            if (SendButton != null) SendButton.Visible = false;
            if (ConfirmButton != null) ConfirmButton.Visible = true;
            if (ConfirmButton2 != null) ConfirmButton2.Visible = false;

            // แถบนำทาง + ปุ่มคู่มือ AAD Confirm (Status 50)
            if (status_id == "50")
            {
                litModeGuide.Text = BuildModeGuide("Confirm");
                litModeGuide.Visible = true;
            }

            //Borrow Note
            borrow_note.Visible = true;

            //Show Comment History
            ShowRejectHistoryIfAny();
        }
        private void SetReturnMode(string status_id)
        {
            // --- Show Text Mode
            lb_mode.Text = "Mode : Return";

            //Hide Step
            step_requester.Visible = false;
            step_borrowlist.Visible = false;
            step_comment.Visible = false;

            // Disable Search & Add list Panel
            pnlSearch_Add.Visible = false;

            //Gridview Borrow List
            gvBorrowList.Columns[1].Visible = false;

            //Hide Button
            if (DraftButton != null) DraftButton.Visible = false;
            if (RejectButton != null) RejectButton.Visible = false;
            if (SendButton != null) SendButton.Visible = false;
            if (ConfirmButton != null) ConfirmButton.Visible = false;
            if (ConfirmButton2 != null) ConfirmButton2.Visible = false;
            if (CancelButton != null) CancelButton.Visible = false;

            //Borrow Note
            borrow_note.Visible = true;

            //Show Comment History
            ShowRejectHistoryIfAny();
        }
        private void MakeRowReadonly(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                // ✅ ข้าม LinkButton ทั้งหมด (Detail / Picture จะรอด)
                if (c is LinkButton)
                {
                    continue;
                }

                // ✅ ปิดเฉพาะ input
                if (c is DropDownList)
                {
                    ((DropDownList)c).Enabled = false;
                }

                if (c is TextBox)
                {
                    ((TextBox)c).Enabled = false;
                }

                // ✅ HTML Button (+)
                System.Web.UI.HtmlControls.HtmlButton htmlBtn =
                    c as System.Web.UI.HtmlControls.HtmlButton;

                if (htmlBtn != null)
                {
                    htmlBtn.Disabled = true;
                }

                // ✅ recursive ต่อ
                MakeRowReadonly(c);
            }
        }

        #endregion


        #region --- Helper : Check Duplicate-key ---
        // --- SQL Server error: 2627 = unique constraint, 2601 = unique index
        private static bool IsDuplicateKeyError(SqlException ex)
        {
            foreach (SqlError err in ex.Errors)
            {
                if (err.Number == 2627 || err.Number == 2601)
                    return true;
            }
            return false;
        }

        #endregion

        #region --- Helper : GetNext Reject / GetNext IssueNo ---

        private int GetNextRejectCount(string Doc_No)
        {
            string str_max =
                "select ISNULL(MAX(Reject_Count), 0) + 1 as MaxReject" +
                " from Reject_Detail_All" +
                " where Doc_No = @Doc_No";

            dc.QryDT_P(conn, str_max,
                new SqlParameter("@Doc_No", SqlDbType.NVarChar) { Value = Doc_No });

            return Convert.ToInt32(dc.dt.Rows[0]["MaxReject"]);
        }
        private int GetNextIssueNo_Tx(SqlConnection c, SqlTransaction tx, int issueYear)
        {
            string sql =
                "select ISNULL(MAX(issue_no), 0) + 1 as MaxIssueNo" +
                " from Withdraw_Control_List WITH (UPDLOCK, HOLDLOCK)" +
                " where year(issue_date) = @yr";

            using (var cmd = new SqlCommand(sql, c, tx))
            {
                cmd.Parameters.Add(new SqlParameter("@yr", SqlDbType.Int) { Value = issueYear });
                object val = cmd.ExecuteScalar();
                return Convert.ToInt32(val);
            }
        }

        #endregion

        #region --- Helper : Alert ---
        protected void alert_toast(string detail, string title, string type, string showtime)
        {
            if (type == "warn" || type == "error" || type == "success" || type == "info")
            {
                int durationMs;
                if (!int.TryParse(showtime, out durationMs))
                    durationMs = 3000; // default 3 วิ

                // Encode ค่าที่เป็น string เพื่อกัน JS พัง/XSS
                string jsDetail = HttpUtility.JavaScriptStringEncode(detail ?? string.Empty);
                string jsTitle = HttpUtility.JavaScriptStringEncode(title ?? string.Empty);
                string jsType = HttpUtility.JavaScriptStringEncode(type ?? "info");

                string script =
                    "showToast('" + jsDetail + "', { " +
                        "title: '" + jsTitle + "', " +
                        "type: '" + jsType + "', " +
                        "duration: " + durationMs + " " +
                    "});";

                string key = "toast_" + DateTime.UtcNow.Ticks;
                ScriptManager.RegisterStartupScript(this, this.GetType(), key, script, true);
            }
        }

        // ── Helper : DropDownList warning ──
        private void SetWarning_DDL(DropDownList ddl, bool isWarning)
        {
            if (ddl == null) return;

            if (isWarning)
            {
                ddl.Style["border"] = "2px solid #ffc107";
                ddl.Style["background-color"] = "#fff3cd";

                if (!ddl.CssClass.Contains("ddl-warning"))
                    ddl.CssClass += " ddl-warning";
            }
            else
            {
                ddl.Style["border"] = "";
                ddl.Style["background-color"] = "";

                ddl.CssClass = ddl.CssClass
                    .Replace(" ddl-warning", "")
                    .Replace("ddl-warning", "")
                    .Trim();
            }
        }

        // ── Helper : TextBox warning ──
        private void SetWarning_TextBox(TextBox txt, bool isWarning)
        {
            if (txt == null) return;
            if (isWarning)
            {
                txt.Style["border-color"] = "#ffc107";
                txt.Style["background-color"] = "#fff3cd";
            }
            else
            {
                txt.Style["border-color"] = "";
                txt.Style["background-color"] = "";
            }
        }

        // --- alert + redirect (เรียกหลัง commit สำเร็จเท่านั้น)
        private void ShowSaveSuccessAndRedirect(string Doc_No, string status_id)
        {
            string alert_info = "";
            switch (status_id)
            {
                case "1": alert_info = "Save Draft"; break;
                case "50": alert_info = "Send Complete"; break;
                case "99": alert_info = "Confirm Complete"; break;
                case "100": alert_info = "Update Complete"; break;
                case "999": alert_info = "Cancel Request"; break;
            }

            string link_path = (status_id == "99") ? "ApproveStatus.aspx" : "EquipmentStatus2.aspx";

            string jsDoc = HttpUtility.JavaScriptStringEncode(Doc_No);
            string jsInfo = HttpUtility.JavaScriptStringEncode(alert_info);

            string script =
                "document.getElementById('loadingOverlay').style.display = 'block';" +
                "if (typeof showToast === 'function') {" +
                "  showToast('Doc No : " + jsDoc + "', { title: '" + jsInfo + "', type: 'info', duration: 2000 });" +
                "}" +
                "setTimeout(function () { window.location.href = '" + link_path + "'; }, 2000);";

            ScriptManager.RegisterStartupScript(this, this.GetType(), "redirectScript", script, true);
        }

        #endregion

        #region --- Helper : เก็บค่า Employee ลง ViewState ---

        private void SaveEmployeeToViewState()
        {
            ViewState["EMP_ID"] = txtEmpId.Text;
            ViewState["EMP_NAME"] = txtEmpName.Text;
            ViewState["EMP_POSITION"] = txtPosition.Text;
            ViewState["EMP_DEPT"] = txtDept.Text;
            ViewState["EMP_BAND"] = hdtxtBand.Value;
            ViewState["EMP_EMAIL"] = hdtxtEmail.Value;
        }
        // ── Helper : เลือก CSS class ตาม DisplayStatus ──
        protected string GetStatusBadgeClass(string displayStatus)
        {
            if (displayStatus == "อยู่ระหว่างยืม")
                return "badge-borrowing";          // สีแดง/ส้ม

            if (displayStatus.StartsWith("Spare"))
                return "badge-spare";              // สีเหลือง

            return "badge-normal";                 // สีเขียว
        }

        // ✅ ใช้แทน local function — .NET 4.0 compatible
        private string GetRowVal(DataRowView row, string colName)
        {
            if (!row.Row.Table.Columns.Contains(colName)) return "";
            return row[colName] == DBNull.Value ? "" : row[colName].ToString().Trim();
        }

        // ── Helper : อ่านค่าจาก DataRow ป้องกัน DBNull ──
        private string ColVal(DataRow row, string colName)
        {
            if (!row.Table.Columns.Contains(colName)) return "";
            return row[colName] == DBNull.Value ? "" : row[colName].ToString().Trim();
        }
        // ── Helper : อ่าน string จาก DataRow ป้องกัน DBNull (.NET 4.0) ──
        private string GetVal(DataRow row, string colName)
        {
            if (row == null) return "";
            if (!row.Table.Columns.Contains(colName)) return "";
            return row[colName] == DBNull.Value ? "" : row[colName].ToString().Trim();
        }
        // ── Helper : แปลงค่าจาก DataRow → object (DBNull ถ้าว่าง) ──
        private object ToDbVal(DataRow row, string colName)
        {
            if (!row.Table.Columns.Contains(colName)) return DBNull.Value;
            if (row[colName] == DBNull.Value) return DBNull.Value;
            if (string.IsNullOrEmpty(row[colName].ToString().Trim())) return DBNull.Value;
            return row[colName];
        }

        #endregion

        #region --- Helper : Validate emp_no สำหรับ HRMS ---

        //emp_no มี format ตายตัว: ตัวอักษร/ตัวเลข ความยาว 6–10 อะไรที่ไม่เข้า pattern = ปฏิเสธ ไม่ส่งไป middleware เลย

        private static readonly System.Text.RegularExpressions.Regex _empNoPattern =
            new System.Text.RegularExpressions.Regex(
                @"^[A-Za-z0-9]{6,10}$",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        private bool IsValidEmpNo(string empNo)
        {
            return !string.IsNullOrEmpty(empNo) && _empNoPattern.IsMatch(empNo);
        }

        #endregion


        // ============================================================
        // Helper : Check Tel / Comment
        // ============================================================
        private bool ValidateTel()
        {
            string tel = txtTel.Text.Trim();

            if (!System.Text.RegularExpressions.Regex.IsMatch(tel, @"^\d{4,}(,\d{4,})*$"))
            {
                alert_toast("กรุณากรอกเบอร์ติดต่อ (Tel) เป็นตัวเลข 4 หลัก",
                            "ข้อมูลไม่ครบ", "warn", "5000");
                txtTel.Focus();
                return false;   // ไม่ผ่าน
            }
            return true;        // ผ่าน
        }
        private bool ValidateBorrowComment()
        {
            // ── 1) reason ที่เลือก ──
            string reasonId = ddlReason_Borrow.SelectedValue;

            // ยังไม่เลือก reason → ปล่อยให้ check reason แยกจัดการ (ไม่เกี่ยว comment)
            if (string.IsNullOrEmpty(reasonId) || reasonId == "0")
                return true;

            // ── 2) query ว่า reason นี้บังคับ comment ไหม ──
            string str =
                "select comment_withdraw from Reason_Borrow_Master" +
                " where reason_id = @reason_id";

            dc.QryDT_P(conn, str,
                new SqlParameter("@reason_id", SqlDbType.NVarChar) { Value = reasonId });

            bool requireComment = false;
            if (dc.dt.Rows.Count > 0 && dc.dt.Rows[0]["comment_withdraw"] != DBNull.Value)
                requireComment = Convert.ToBoolean(dc.dt.Rows[0]["comment_withdraw"]);

            // ── 3) ไม่บังคับ → ผ่าน ──
            if (!requireComment)
                return true;

            // ── 4) บังคับ → เช็ค comment ──
            string comment = txt_borrowComment.Text.Trim();

            // 4.1 ไม่มี comment
            if (string.IsNullOrEmpty(comment))
            {
                alert_toast("เหตุผลการร้องขอเบิกนี้ต้องระบุ Comment", "กรุณากรอก Comment", "warn", "5000");
                txt_borrowComment.Style["border-color"] = "#ffc107";
                txt_borrowComment.Style["background-color"] = "#fff3cd";
                txt_borrowComment.Focus();
                return false;
            }

            // 4.2 มีแต่ไม่ถึง 10 ตัวอักษร
            if (comment.Length < 10)
            {
                alert_toast("กรุณากรอก Comment อย่างน้อย 10 ตัวอักษร", "Comment สั้นเกินไป", "warn", "5000");
                txt_borrowComment.Style["border-color"] = "#ffc107";
                txt_borrowComment.Style["background-color"] = "#fff3cd";
                txt_borrowComment.Focus();
                return false;
            }

            // ── 5) ผ่านครบ → คืน style ปกติ ──
            txt_borrowComment.Style["border-color"] = "";
            txt_borrowComment.Style["background-color"] = "";
            return true;
        }


        // ============================================================
        // duringBorrowCheck() — parameterized + SqlDbType.Int
        // ── เช็คตาราง Borrow_List (การเบิกต้องเช็คว่าของถูกยืมอยู่หรือไม่) ──
        // ============================================================
        private bool duringBorrowCheck()
        {
            DataTable dt = BorrowListDT;
            bool borrowCheck = false;

            foreach (DataRow r in dt.Rows)
            {
                if (r["ToolId"] == DBNull.Value || r["ToolSub"] == DBNull.Value || r["SystemId"] == DBNull.Value)
                    continue;

                if (GetVal(r, "CompareData") == "Delete") continue;

                string strDB =
                    "SELECT * FROM Borrow_List" +
                    " WHERE tool_id = @tool_id" +
                    "   AND sub_id = @sub_id" +
                    "   AND system_id = @system_id" +
                    "   AND status_id IN (50, 90)";

                dc.QryDT_P(conn, strDB,
                    new SqlParameter("@tool_id", SqlDbType.Int) { Value = Convert.ToInt32(r["ToolId"]) },
                    new SqlParameter("@sub_id", SqlDbType.Int) { Value = Convert.ToInt32(r["ToolSub"]) },
                    new SqlParameter("@system_id", SqlDbType.Int) { Value = Convert.ToInt32(r["SystemId"]) });

                DataTable duringBorrow = dc.dt;

                if (duringBorrow.Rows.Count > 0)
                {
                    r["StatusId"] = duringBorrow.Rows[0]["status_id"];
                    borrowCheck = true;
                }
            }
            return borrowCheck;
        }


        // --- Data Borrow List
        private void BindBorrowList()
        {
            BindBorrowList_Display();
        }

    }
}
