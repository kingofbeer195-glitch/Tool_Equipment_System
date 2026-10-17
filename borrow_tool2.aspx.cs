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
    public partial class Borrow_Tool : System.Web.UI.Page
    {
        SqlConnection conn_user = new SqlConnection(ConfigurationManager.ConnectionStrings["user"].ConnectionString);
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        SqlConnection conn_jig = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        datacon dc = new datacon();

        private DataTable _cellM, _stationM;   // master Cell / Station — โหลดครั้งเดียวต่อการ Bind

        // ── Manual ─────────────────────────────────────────────
        private const string MANUAL_FOLDER =
            @"\\cptfile02\Pro\PRJ2023008\50_Flow_Manual\01_Manual\08_Tool&Equipment\01_Borrow";
        private static readonly string[] MANUAL_FILES = { "Step1", "Step2", "Step3", "Step4", "SaveDraft", "SendRequest", "CancelRequest", "Confirm", "Return" , "Reject" };

        // ── Requester Snapshot ─────────────────────────────────
        private const string DOC_TYPE = "AAD1";
        private bool _hrmsError = false;   // HRMS ไม่ตอบสนอง (ต่างจาก "ไม่พบพนักงาน")

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
            Page.Title = "ยืมอุปกรณ์";

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

                ViewState["DEPT_MASTER"] = GetDeptMaster();
                ViewState["LOCATION_MASTER"] = GetLocationMaster();
                ViewState["MODEL_MASTER"] = GetModelMaster();
                ViewState["MODE"] = mode;
                GetReasonBorrowMaster();

                if (Doc_NO == "0")
                {
                    Requester_getdata();

                    // กำหนดค่าใน TextBox "วันที่ยืม" เป็นวันที่ปัจจุบัน
                    txtIssueDate.Text = DateTime.Today.ToString("dd-MMM-yyyy");

                    // ✅ [ข้อ 4.2] ตรวจสอบ ADMIN เพื่อกำหนดการแสดงปุ่ม
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
                        ShowRejectHistoryIfAny();               // ✅ เดิม : pnl_rejectComment.Visible = true;
                    }
                }
            }
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
                //lblempNotice.Text = "ไม่พบ Employee ID นี้ในระบบ";
                lblempNotice.Text = _hrmsError
                    ? "ไม่สามารถเชื่อมต่อระบบ HRMS ได้ กรุณาลองใหม่อีกครั้ง"
                    : "ไม่พบ Employee ID นี้ในระบบ";
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
                    //lblempNotice.Text = "ไม่พบ Employee ID นี้ในระบบ";
                    lblempNotice.Text = _hrmsError
                        ? "ไม่สามารถเชื่อมต่อระบบ HRMS ได้ กรุณาลองใหม่อีกครั้ง"
                        : "ไม่พบ Employee ID นี้ในระบบ";
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

            string str_tel = "Select ext From sys_user_master Where emp_id = '" + userId + "'";
            dc.QryDT(conn_user, str_tel);
            if (dc.dt.Rows.Count > 0)
                txtTel.Text = dc.dt.Rows[0]["ext"].ToString();

            // userId ผ่าน whitelist [A-Za-z0-9] แล้ว
            string sql =
                "select GNAME_ENG, FNAME_ENG, POSN_ENAME, BAND, DEPT_CODE, DEPT_ABB_NAME, EMAIL " +
                "from ADMIN.v_emp_data_all_cpt where UPPER(emp_no)=UPPER('" + userId + "')";

            try
            {
                var request = (HttpWebRequest)WebRequest.Create("http://cptsvs547:1000/middleware/oracle/hrms");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 10000;   // ✅ ไม่ให้หน้าค้างนานถ้า HRMS ล่ม

                byte[] data = Encoding.UTF8.GetBytes("command=" + HttpUtility.UrlEncode(sql));
                using (var stream = request.GetRequestStream())
                    stream.Write(data, 0, data.Length);

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(reader.ReadToEnd());

                    if (result == null || result.data == null || result.data.Count == 0)
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
            catch (Exception ex)
            {
                _hrmsError = true;
                System.Diagnostics.Debug.WriteLine("[HRMS] " + userId + " : " + ex.Message);
                return false;
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
        protected string status_show(string Doc_No)
        {
            string str_status =
                " SELECT A.status_id, B.status_name, B.Style_CSS" +
                " FROM Borrow_Control_List A" +
                " LEFT JOIN Master_Status B ON A.status_id = B.status_id" +
                " WHERE Doc_No = @Doc_No";

            dc.QryDT_P(conn, str_status,
                new SqlParameter("@Doc_No", SqlDbType.NVarChar) { Value = Doc_No });

            // ✅ กัน crash ถ้าไม่พบเอกสาร
            if (dc.dt.Rows.Count == 0)
            {
                litStatus.Text = "<span class='status-badge status-cancel'>ไม่พบเอกสาร</span>";
                btnCancel.Visible = false;
                return "";
            }

            string status_id = dc.dt.Rows[0]["status_id"].ToString();
            string statusName = dc.dt.Rows[0]["status_name"].ToString();
            string styleCss = dc.dt.Rows[0]["Style_CSS"].ToString();

            litStatus.Text = "<span class='status-badge " + styleCss + "'>" + statusName + "</span>";

            // ✅ แสดงปุ่ม Cancel เฉพาะ status ที่ยังยกเลิกได้ (1=Draft, 50=Sent)
            //btnCancel.Visible = (status_id == "1" || status_id == "50");

            return status_id;
        }
        private void LoadRequester(string Doc_No)
        {
            string str_emp = "select * from Borrow_Control_List where Doc_No = @Doc_No";
            dc.QryDT_P(conn, str_emp,
                new SqlParameter("@Doc_No", SqlDbType.NVarChar) { Value = Doc_No });

            if (dc.dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูลผู้ขอ", "Error", "error", "5000");
                return;
            }

            DataRow h = dc.dt.Rows[0];
            string emp_id = h["issue_empid"].ToString().Trim();
            DateTime issue_date = Convert.ToDateTime(h["issue_date"]);
            string reason_id = h["reason_id"].ToString();
            string comment_text = h["reason_remark"].ToString();
            string headDept = h["dept_code"] == DBNull.Value ? "" : h["dept_code"].ToString().Trim();
            string phone = h["phone"] != DBNull.Value ? h["phone"].ToString() : "";

            txtEmpId.Text = emp_id;

            // ══ 1) HRMS ก่อน ══
            if (GetDataHRMS_CSharp(emp_id))
            {
                pnlReqSource.Visible = false;
                SaveRequesterInfo_IfMissing(Doc_No, phone);   // เอกสารเก่าที่ยังไม่มี Snapshot → เก็บไว้เลย
            }
            else
            {
                // ══ 2) ไม่เจอ → อ่านจาก Requester_Info ══
                DateTime? savedAt;
                bool fromSnap = LoadRequesterSnapshot(Doc_No, out savedAt);

                if (!fromSnap)
                {
                    ClearEmployeeUI();
                    txtDeptCode.Text = headDept;               // อย่างน้อยมี Dept Code จาก Header
                }
                ShowRequesterNotice(emp_id, fromSnap, savedAt);
            }

            txtTel.Text = phone;
            txtIssueDate.Text = issue_date.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            txtEmpId.ReadOnly = true;

            SaveEmployeeToViewState();

            if (ddlReason_Borrow.Items.FindByValue(reason_id) != null)
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
                " from Borrow_List A" +
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
                newRow["LocationName"] = r["location"];
                newRow["LocationId"] = r["location1_id"];
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

        // --- Search List
        protected void Bind_SearchData()
        {
            //DataTable dt = dt_search;

            gvSearchResult.DataSource = ViewState["SearchResult"];
            gvSearchResult.DataBind();
        }
        protected void btnSearch_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(txtFixAsset.Text))
            {
                alert_toast("กรุณากรอกข้อมูลที่ต้องการค้นหา", "แจ้งเตือน", "warn", "5000");
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
                " WHERE (" + keyword + ")" + where_chkbox + "AND status_id not IN (11,12) " +
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
                dt.Columns.Add("ProcUrl", typeof(string));   // ✅ url เอกสารที่ติดอยู่
                dt.Columns.Add("ProcDoc", typeof(string));   // ✅ เลขเอกสาร
            }

            // ── อุปกรณ์ที่ติดกระบวนการ AAD1–AAD4 (query เดียว) ──
            var procMap = GetProcessMap(litIssueNo.Text.Trim());

            int rowNo = 1;
            foreach (DataRow row in dt.Rows)
            {
                row["RowNo"] = rowNo++;

                ProcInfo proc;
                if (procMap.TryGetValue(ProcKey(row["tool_id"], row["sub_no"], row["sys_status"]), out proc))
                {
                    row["DisplayStatus"] = proc.Label;
                    row["ProcUrl"] = proc.Url;
                    row["ProcDoc"] = proc.DocNo;
                }
                else
                {
                    int statusId = Convert.ToInt32(row["status_id"]);
                    row["DisplayStatus"] = (statusId == 1 || statusId == 2)
                        ? row["status"].ToString()
                        : "Spare : " + row["status"];
                    row["ProcUrl"] = "";
                    row["ProcDoc"] = "";
                }
            }

            ViewState["SearchResult"] = dt;
            gvSearchResult.DataSource = dt;
            gvSearchResult.DataBind();
        }
        protected void saveData(string status_id)
        {
            DataTable dt = BorrowListDT;
            string Doc_No = litIssueNo.Text.Trim();

            int reason_id = 0;
            string comment_text = txt_borrowComment.Text.Trim();
            //string comment_text = Request.Unvalidated["txt_borrowComment"];
            //string comment_text = Server.HtmlEncode(txt_borrowComment.Text.Trim());
            string phone = txtTel.Text.Trim();

            if (ddlReason_Borrow.SelectedIndex > 0)
                int.TryParse(ddlReason_Borrow.SelectedValue, out reason_id);

            // ════════════════════════════════════════════════════════════════
            // UPDATE : มี Doc_No อยู่แล้ว
            // ════════════════════════════════════════════════════════════════
            if (Doc_No != "-" && !string.IsNullOrEmpty(Doc_No))
            {
                Save_BorrowListDT();

                if (duringBorrowCheck())
                {
                    BindBorrowList();
                    return;
                }

                // ── Update header status ──────────────────────────────────────
                conn.Open();
                try
                {
                    if (status_id == "1")
                    {
                        string sqlHeader =
                            " UPDATE Borrow_Control_List SET status_id = @status_id , " +
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
                    else if (status_id == "50")
                    {
                        string sqlHeader =
                            " UPDATE Borrow_Control_List SET status_id = @status_id , " +
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

                    // ✅ เพิ่ม: update status ของ detail ทุก row ที่ยังไม่ถูก reject/cancel
                    //    (ไม่แตะ row ที่ status 999 = cancel)
                    string sqlDetailStatus =
                        "UPDATE Borrow_List SET status_id = @status_id" +
                        " WHERE Doc_No = @Doc_No AND status_id NOT IN (999)";

                    using (SqlCommand cmd = new SqlCommand(sqlDetailStatus, conn))
                    {
                        cmd.Parameters.AddWithValue("@status_id", status_id);
                        cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                        cmd.ExecuteNonQuery();
                    }

                    // ✅ อัปเดตข้อมูล Requester ณ วันที่ส่ง
                    SaveRequesterInfo(conn, null, Doc_No, true, phone);
                }
                catch (Exception ex)
                {
                    alert_toast("Error :" + ex,"บันทึกไม่สำเร็จ", "error", "8000");
                }

                finally
                {
                    conn.Close();
                }

                Sync_BorrowList(Doc_No, status_id);
            }

            // ════════════════════════════════════════════════════════════════
            // INSERT : ยังไม่มี Doc_No → ไปทาง retry branch (จัดการเองหมด)
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
            Save_BorrowListDT();

            if (duringBorrowCheck())
            {
                BindBorrowList();
                return;
            }

            DataTable dt = BorrowListDT;
            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูลในรายการ Borrow List", "ข้อมูลไม่ครบ", "warn", "5000");
                return;
            }

            int issueYear = DateTime.Now.Year;
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
                            Doc_No = "AAD1-" + issueYear_2 + "-" + issueNo.ToString("00000");

                            // 2) INSERT header (ถ้า Doc_No ซ้ำ → throw 2627 ที่นี่)
                            InsertControlHeader_Tx(c, tx, issueNo, Doc_No, status_id);

                            // 3) INSERT รายการ Borrow_List ทั้งหมด (ใน tx เดียวกัน)
                            InsertBorrowRows_Tx(c, tx, dt, Doc_No, status_id);

                            // 3.1) ✅ เก็บข้อมูล Requester (ใน tx เดียวกัน)
                            SaveRequesterInfo(c, tx, Doc_No, true, txtTel.Text.Trim());

                            // 4) ทุกอย่างผ่าน → commit
                            tx.Commit();
                            success = true;
                            break;
                        }
                        catch (SqlException ex) 
                        {
                            // rollback ก่อนเสมอ
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
                                // ไม่ใช่รอบสุดท้าย → วนต่อ (continue ไป attempt ถัดไป)
                                continue;
                            }

                            // ── SqlException อื่นที่ไม่ใช่ duplicate → แจ้ง error แล้วจบ ──
                            alert_toast("เกิดข้อผิดพลาด : " + ex.Message, "Error", "error", "10000");
                            return;
                        }
                        catch (Exception ex)
                        {
                            // error อื่น → rollback ทั้งหมด (header + borrow list ไม่เหลือครึ่งใบ)
                            try { tx.Rollback(); } catch { }
                            alert_toast("เกิดข้อผิดพลาด : " + ex.Message, "Error", "error", "10000");
                            return;
                        }
                    }
                }
            }

            if (!success) return;

            // ── สำเร็จ → alert + redirect (ย้ายออกมาจาก InsertBorrowList เดิม) ──
            ShowSaveSuccessAndRedirect(Doc_No, status_id);
        }

        // --- INSERT header ภายใน transaction ปล่อย SqlException หลุดออกไปให้ลูป retry จับ(ไม่ try/catch ที่นี่)
        private void InsertControlHeader_Tx(SqlConnection c, SqlTransaction tx, int issueNo, string Doc_No, string status_id)
        {
            string issue_empid = txtEmpId.Text;
            string dept_code = txtDeptCode.Text;
            string phone = txtTel.Text.Trim();
            int reason_id = 0;
            string comment_text = txt_borrowComment.Text.Trim();

            if (ddlReason_Borrow.SelectedIndex > 0)
                int.TryParse(ddlReason_Borrow.SelectedValue, out reason_id);

            string sql;
            if (status_id == "1")
            {
                sql = @"INSERT INTO Borrow_Control_List
                ( issue_no, issue_date, Doc_No, status_id, issue_empid,
                  dept_code,phone, reason_id, reason_remark, update_date )
                VALUES
                ( @issue_no, @issue_date, @Doc_No, @status_id, @issue_empid,
                  @dept_code, @phone, @reason_id, @reason_remark, @update_date )";
            }
            else if (status_id == "50")
            {
                sql = @"INSERT INTO Borrow_Control_List
                ( issue_no, issue_date, Doc_No, status_id, issue_empid,
                  dept_code,phone, reason_id, reason_remark, update_date, send_date )
                VALUES
                ( @issue_no, @issue_date, @Doc_No, @status_id, @issue_empid,
                  @dept_code,@phone, @reason_id, @reason_remark, @update_date, @send_date )";
            }
            else if (status_id == "90")
            {
                sql = @"INSERT INTO Borrow_Control_List
                ( issue_no, issue_date, Doc_No, status_id, issue_empid,
                  dept_code,phone, reason_id, reason_remark, update_date, send_date,
                  confirm_empid, confim_date )
                VALUES
                ( @issue_no, @issue_date, @Doc_No, @status_id, @issue_empid,
                  @dept_code,@phone, @reason_id, @reason_remark, @update_date, @send_date,
                  @confirm_empid, @confim_date )";
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

                // ✅ phone : ถ้าว่างเก็บ DBNull (กัน insert ค่าว่างเปล่า)
                cmd.Parameters.AddWithValue("@phone",
                    string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);

                cmd.Parameters.AddWithValue("@reason_id", reason_id);
                cmd.Parameters.AddWithValue("@reason_remark", comment_text);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);

                if (status_id == "50" || status_id == "90")
                    cmd.Parameters.AddWithValue("@send_date", DateTime.Now);

                if (status_id == "90")
                {
                    cmd.Parameters.AddWithValue("@confirm_empid",
                        Session["emp_id"].ToString().Trim());
                    cmd.Parameters.AddWithValue("@confim_date", DateTime.Now);
                }

                cmd.ExecuteNonQuery();   // ← Doc_No ซ้ำ throw 2627 ที่นี่
            }
        }

        // --- INSERT รายการ Borrow_List ทั้งหมด ภายใน transaction ปล่อย exception หลุดให้ caller rollback(กัน half-document)
        private void InsertBorrowRows_Tx(SqlConnection c, SqlTransaction tx,DataTable dt, string Doc_No, string status_id)
        {
            string sql = @"
                INSERT INTO Borrow_List
                ( Doc_No , No , FixAsset , ControlNo ,
                  tool_id , sub_id , system_id ,
                  Use_Dept ,
                  Use_Model_Id , New_Model , HD_Model ,
                  Use_Cell_Id , New_Cell , HD_Cell ,
                  Use_Station_Id , New_Station , HD_Station ,
                  status_id )
                VALUES
                ( @Doc_No , @No , @FixAsset , @ControlNo ,
                  @tool_id , @sub_id , @system_id ,
                  @Use_Dept ,
                  @Use_Model_Id , @New_Model , @HD_Model ,
                  @Use_Cell_Id , @New_Cell , @HD_Cell ,
                  @Use_Station_Id , @New_Station , @HD_Station ,
                  @status_id )";

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
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private Boolean Check_blankData()
        {
            DataTable dt = BorrowListDT;
            bool hasBlank = false;

            foreach (GridViewRow row in gvBorrowList.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow) continue;

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

        // --- Save Data on List
        private void Save_BorrowListDT()
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
               "       Use_Model_Id, New_Model, HD_Model," +
               "       Use_Cell_Id, New_Cell, HD_Cell," +
               "       Use_Station_Id, New_Station, HD_Station" +
               " FROM  Borrow_List" +
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
        private void Sync_BorrowList(string Doc_No, string status_id)
        {
            DataTable dt = BorrowListDT;

            // --- ตรวจสอบการลงข้อมูล
            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("ไม่มีข้อมูลใน BorrowListDT", "Sync Error", "error", "5000");
                return;
            }

            // ── Renumber RowNo เฉพาะ row ที่ไม่ใช่ Delete ────────────────────────
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
                                    INSERT INTO Borrow_List
                                    ( Doc_No, No, FixAsset, ControlNo,
                                      tool_id, sub_id, system_id, Use_Dept,
                                      Use_Model_Id, New_Model, HD_Model,
                                      Use_Cell_Id, New_Cell, HD_Cell,
                                      Use_Station_Id, New_Station, HD_Station,
                                      status_id )
                                    VALUES
                                    ( @Doc_No, @No, @FixAsset, @ControlNo,
                                      @tool_id, @sub_id, @system_id, @Use_Dept,
                                      @Use_Model_Id, @New_Model, @HD_Model,
                                      @Use_Cell_Id, @New_Cell, @HD_Cell,
                                      @Use_Station_Id, @New_Station, @HD_Station,
                                      @status_id )";

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
                                    cmd.ExecuteNonQuery();
                                    cntInsert++;
                                }
                                break;
                            }

                        case "Update":
                            {
                                string sql = @"
                                    UPDATE Borrow_List SET
                                        No              = @No,
                                        Use_Model_Id    = @Use_Model_Id,
                                        New_Model       = @New_Model,
                                        HD_Model        = @HD_Model,
                                        Use_Cell_Id     = @Use_Cell_Id,
                                        New_Cell        = @New_Cell,
                                        HD_Cell         = @HD_Cell,
                                        Use_Station_Id  = @Use_Station_Id,
                                        New_Station     = @New_Station,
                                        HD_Station      = @HD_Station,
                                        status_id       = @status_id
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
                                    cmd.ExecuteNonQuery();
                                    cntUpdate++;
                                }
                                break;
                            }

                        case "Delete":
                            {
                                string sql = @"
                                        DELETE FROM Borrow_List
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
                        case "90": alert_info = "Confirm Complete"; break;
                        case "100": alert_info = "Return Complete"; break;
                        case "999": alert_info = "Cancel Request"; break;
                    }

                    alert_toast("Doc No : " + Doc_No, alert_info, "info", "2000");

                    string script =
                        "document.getElementById('loadingOverlay').style.display = 'block';" +
                        "setTimeout(function() {" +
                        "  window.location.href = 'EquipmentStatus2.aspx';" +
                        "}, 2000);";

                    ScriptManager.RegisterStartupScript(
                        this, this.GetType(), "redirectScript", script, true);
                }
            }
        }

        //=====================================================================
        // duringBorrowCheck() — เปลี่ยนเป็น SqlDbType.Int
        // ---------------------------------------------------------------------
        // column tool_id/sub_id/system_id เป็น int → ส่ง Int ตรง type
        // กัน implicit convert ที่ทำให้ index ใช้ไม่ได้
        // ===================================================================== */
        /// <summary>ตรวจรายการที่ติดกระบวนการอื่น (AAD1–AAD4) — แจ้งเตือนเองพร้อมระบุกระบวนการ + เลขเอกสาร</summary>
        private bool duringBorrowCheck()
        {
            var procMap = GetProcessMap(litIssueNo.Text.Trim());
            var msgs = new List<string>();
            int no = 0;

            foreach (DataRow r in BorrowListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                no++;
                if (r["ToolId"] == DBNull.Value || r["ToolSub"] == DBNull.Value || r["SystemId"] == DBNull.Value) continue;

                ProcInfo proc;
                if (!procMap.TryGetValue(ProcKey(r["ToolId"], r["ToolSub"], r["SystemId"]), out proc)) continue;

                r["StatusId"] = "50";   // แถวใน Borrow List ขึ้นสีเหลือง + ขอบแดง (RowDataBound เดิม)
                string name = GetVal(r, "FixAssetNo") != "" ? GetVal(r, "FixAssetNo")
                            : (GetVal(r, "ControlNo") != "" ? GetVal(r, "ControlNo") : GetVal(r, "ToolNo"));
                msgs.Add("• รายการที่ " + no + " " + name + " → " + proc.Label + " [" + proc.DocNo + "]");
            }

            if (msgs.Count == 0) return false;

            alert_toast("อุปกรณ์ต่อไปนี้อยู่ระหว่างดำเนินการในเอกสารอื่น ไม่สามารถยืมได้\n" +
                        "กรุณา Remove ออกจากรายการก่อน :\n" + string.Join("\n", msgs.ToArray()),
                        "พบรายการอยู่ระหว่างดำเนินการ", "warn", "20000");
            return true;
        }


        #region --- Button Click ---
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

            //lb_test.Text = "Tool ID : " + tool_id + " , Sub ID : " + tool_sub + " , System ID : " + sys_status;

            Get_Folder_Structure_Picture Folder_Structure_Picture = new Get_Folder_Structure_Picture(tool_id, tool_sub, sys_status);
            string get_path_toolSub = Folder_Structure_Picture.get_folder_root();
            string path_picture_toolMain = Folder_Structure_Picture.get_folder_root_view_toolMain();

            string[] files_main = Directory.GetFiles(get_path_toolSub, "*.jpg", SearchOption.TopDirectoryOnly);

            if (files_main.Length > 0)
            {
                string fullPathToImage = Path.Combine(get_path_toolSub, files_main[0]); // สร้าง full path ไปยังไฟล์รูปภาพจริง

                try
                {
                    // โหลดรูปภาพเพื่อดึงขนาด
                    using (System.Drawing.Image img = System.Drawing.Image.FromFile(fullPathToImage))
                    {
                        int imageWidth = img.Width;
                        int imageHeight = img.Height;
                        int maxWidth = 500;
                        int maxHeight = 500;
                        float keep_test;

                        // กำหนด ImageUrl ให้กับคอนโทรล imgMain
                        //alert_toast(fullPathToImage, "แจ้งตือน", "error", "20000");
                        imgMain.ImageUrl = path_picture_toolMain;

                        imgMain.Width = imageWidth;
                        imgMain.Height = imageHeight;

                        // ถ้าคุณต้องการกำหนดขนาดของ imgMain ตามขนาดจริงของรูปภาพ
                        if (imageWidth > maxWidth && imageHeight <= maxHeight)
                        {
                            //lb_testPic.Text = "IF_1 : " + imageWidth.ToString() + ":" + imageHeight.ToString();
                            keep_test = imageHeight - (((imageWidth - maxWidth) / imageWidth) * imageHeight);
                            imgMain.Height = Convert.ToInt32(keep_test);
                            imgMain.Width = maxWidth;
                        }
                        else if (imageWidth <= maxWidth && imageHeight > maxHeight)
                        {
                            //lb_testPic.Text = "IF_2 : " + imageWidth.ToString() + ":" + imageHeight.ToString();
                            keep_test = imageWidth - (((imageHeight - maxHeight) / imageHeight) * imageWidth);
                            imgMain.Width = Convert.ToInt32(keep_test);
                            imgMain.Height = maxHeight;
                        }
                        else if (imageWidth > maxWidth && imageHeight > maxHeight)
                        {
                            //lb_testPic.Text = "IF_3 : " + imageWidth.ToString() + ":" + imageHeight.ToString();
                            if (imageWidth > imageHeight)
                            {
                                //lb_testPic.Text = "IF_3-1 : " + imageWidth.ToString() + ":" + imageHeight.ToString();
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
                                //lb_testPic.Text = "IF_3-2 : " + imageWidth.ToString() + ":" + imageHeight.ToString();
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
                        else
                        {
                            //lb_testPic.Text = "IF_4 : " + imageWidth.ToString() + ":" + imageHeight.ToString();
                        }

                    }
                }
                catch (Exception ex)
                {
                    imgMain.Height = 500;
                    imgMain.Width = 500;
                    imgMain.ImageUrl = "images/FilesType/jpeg.png"; // แสดงภาพ default หากเกิดข้อผิดพลาด
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
            //return
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
        protected void btnDraft_Click(object sender, EventArgs e)
        {
            string empId = txtEmpId.Text.Trim();
            string empName = txtEmpName.Text.Trim();
            DataTable dt = BorrowListDT;
            string Doc_No = litIssueNo.Text.Trim();
            string status_id = "1";  //Wait Issue

            //alert_toast(Doc_No, "Check Doc No", "warn", "5000");

            // ✅ ตรวจสอบ Employee ID ต้องเป็นตัวเลข 6 หลัก
            if (!System.Text.RegularExpressions.Regex.IsMatch(empId, @"^\d{6}$"))
            {
                alert_toast(
                    "Employee ID ต้องเป็นตัวเลข 6 หลัก", "alert-info", "warn", "5000"
                );
                txtEmpId.Focus();
                return;
            }

            // ✅ ตรวจสอบเยอร์ติดต่อ
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

            // ✅ ตรวจสอบเยอร์ติดต่อ
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

            // Check Null , Not Selection Data
            Save_BorrowListDT(); // Save ก่อน ป้องกัน Mode/Text หาย
            Boolean checkblank = Check_blankData();
            if (checkblank)
            {
                alert_toast("กรุณาลงข้อมูลให้ครบถ้วน (ช่องสีเหลือง)", "ข้อมูลไม่ครบ", "warn", "5000");
                /*BindBorrowList();*/ // Re-bind เพื่อให้ warning style แสดง
                return;
            }

            // ✅ Check Borrow Reason
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
            }
            else
            {
                // -- Accept to Save Data
                saveData(status_id);
            }
        }
        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            string Doc_No = litIssueNo.Text.Trim();
            string mode = "edit";
            string empId = "";

            if (!string.IsNullOrEmpty(Request.QueryString["mode"]))
                mode = Request.QueryString["mode"].ToLower();

            empId = (mode == "confirm") ? Session["emp_id"].ToString() : txtEmpId.Text.Trim();

            conn.Open();
            SqlTransaction tx = conn.BeginTransaction();
            try
            {
                // ── Update Borrow_Control_List → status 90 ──
                string sqlHeader =
                    "UPDATE Borrow_Control_List" +
                    " SET status_id = 90 , update_date = @update_date , confirm_empid = @confirm_empid , confim_date = @confim_date" +
                    " WHERE Doc_No = @Doc_No";
                using (SqlCommand cmd = new SqlCommand(sqlHeader, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@confirm_empid", empId);
                    cmd.Parameters.AddWithValue("@confim_date", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }

                // ── Update Borrow_List → status 90 ──
                string sqlList =
                    "UPDATE Borrow_List SET status_id = 90 WHERE Doc_No = @Doc_No";
                using (SqlCommand cmd = new SqlCommand(sqlList, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                alert_toast("เกิดข้อผิดพลาด : " + ex.Message, "Error", "error", "10000");
                return;   // ✅ ไม่ redirect เมื่อ fail
            }
            finally
            {
                conn.Close();
            }

            alert_toast("Doc No : " + Doc_No, "Confirm Borrow", "success", "2000");

            string script = @"
                document.getElementById('loadingOverlay').style.display = 'block';
                setTimeout(function () {
                    window.location.href = 'ApproveStatus.aspx';
                }, 2000);";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "redirectScript", script, true);
        }
        protected void btnConfirm2_Click(object sender, EventArgs e)
        {
            string empId = txtEmpId.Text.Trim();
            string empName = txtEmpName.Text.Trim();
            DataTable dt = BorrowListDT;
            string Doc_No = litIssueNo.Text.Trim();
            string status_id = "90";  //During Borrow

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

            // ✅ ตรวจสอบเยอร์ติดต่อ
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

            // Check Null , Not Selection Data
            Save_BorrowListDT(); // Save ก่อน ป้องกัน Mode/Text หาย
            Boolean checkblank = Check_blankData();
            if (checkblank)
            {
                alert_toast("กรุณาลงข้อมูลให้ครบถ้วน (ช่องสีเหลือง)", "ข้อมูลไม่ครบ", "warn", "5000");
                /*BindBorrowList();*/ // Re-bind เพื่อให้ warning style แสดง
                return;
            }

            // ✅ Check Borrow Reason
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

            //alert_toast("ตรวจสอบการลงข้อมุล -> OK", "Check Process", "success", "3000");

            // -- Accept to Save Data
            saveData(status_id);

        }
        protected void btnReturn_Click(object sender, EventArgs e)
        {
            // ── ผู้รับคืน (Receiver) = Session emp_id ──
            string receiveEmpId = Session["emp_id"] != null ? Session["emp_id"].ToString().Trim() : "";

            txtReceiveEmpId.Text = receiveEmpId;
            txtReceiveEmpName.Text = Session["user_name"] != null
                                        ? Session["user_name"].ToString()
                                        : GetEmpNameById(receiveEmpId);

            // ── เคลียร์ฟิลด์ผู้คืน ──
            txtReturnEmpId.Text = "";
            txtReturnEmpName.Text = "";
            txtReturnDept.Text = "";
            txtReturnDeptCode.Text = "";

            // ── เปิด Modal ──
            ScriptManager.RegisterStartupScript(
                this, GetType(), "showReturnModal",
                "$('#modal_Return').modal('show');", true);
        }
        protected void btnReturnConfirm_Click(object sender, EventArgs e)
        {
            string Doc_No = litIssueNo.Text.Trim();
            string receiveEmpId = txtReceiveEmpId.Text.Trim();   // ผู้รับคืน (Session)
            string returnEmpId = txtReturnEmpId.Text.Trim();     // ผู้คืน (กรอก)

            // ── Validate ผู้คืน : ต้องเป็นตัวเลข 6 หลัก ──
            if (!System.Text.RegularExpressions.Regex.IsMatch(returnEmpId, @"^\d{6}$"))
            {
                alert_toast("กรุณากรอกรหัสผู้คืนเป็นตัวเลข 6 หลัก", "ข้อมูลไม่ครบ", "warn", "5000");
                ScriptManager.RegisterStartupScript(this, GetType(), "reopenReturnModal",
                    "$('#modal_Return').modal('show');", true);
                return;
            }

            // ── Validate : ต้องดึงชื่อผู้คืนได้ ──
            if (string.IsNullOrEmpty(txtReturnEmpName.Text))
            {
                alert_toast("ไม่พบข้อมูลผู้คืน กรุณาตรวจสอบรหัสพนักงาน", "ข้อมูลไม่ครบ", "warn", "5000");
                ScriptManager.RegisterStartupScript(this, GetType(), "reopenReturnModal",
                    "$('#modal_Return').modal('show');", true);
                return;
            }

            conn.Open();
            try
            {
                // ── Update Borrow_Control_List → status 100 ──
                string sqlHeader =
                    "UPDATE Borrow_Control_List" +
                    " SET status_id = 100 , update_date = @update_date ," +
                    "     return_empid = @return_empid , return_date = @return_date ," +
                    "     receive_empid = @receive_empid" +
                    " WHERE Doc_No = @Doc_No";

                using (SqlCommand cmd = new SqlCommand(sqlHeader, conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@return_empid", returnEmpId);
                    cmd.Parameters.AddWithValue("@return_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@receive_empid", receiveEmpId);
                    cmd.ExecuteNonQuery();
                }

                // ── Update Borrow_List → status 100 ──
                string sqlList =
                    "UPDATE Borrow_List SET status_id = 100 WHERE Doc_No = @Doc_No";

                using (SqlCommand cmd = new SqlCommand(sqlList, conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                alert_toast("เกิดข้อผิดพลาด : " + ex.Message, "Error", "error", "10000");
                conn.Close();
                ScriptManager.RegisterStartupScript(this, GetType(), "reopenReturnModal",
                    "$('#modal_Return').modal('show');", true);
                return;
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close();
            }

            // ── สำเร็จ → ปิด Modal + redirect ──
            alert_toast("Doc No : " + Doc_No, "Return Done", "success", "2000");

            string script = @"
                $('#modal_Return').modal('hide');
                document.getElementById('loadingOverlay').style.display = 'block';
                setTimeout(function () {
                    window.location.href = 'ApproveStatus.aspx';
                }, 2000);";

            ScriptManager.RegisterStartupScript(this, GetType(), "redirectScript", script, true);
        }
        protected void btnUploadManual_Click(object sender, EventArgs e)
        {
            if (Session["admin"] == null || Session["admin"].ToString() != "99")
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
                ScriptManager.RegisterStartupScript(this, GetType(), "reopenUpload", reopen, true);
            }
            catch (Exception ex)
            {
                alert_toast("อัปโหลดไม่สำเร็จ : " + ex.Message, "Error", "error", "8000");
                ScriptManager.RegisterStartupScript(this, GetType(), "reopenUpload", reopen, true);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // Modal เหตุผล : ยืนยัน Reject / Cancel
        // ══════════════════════════════════════════════════════════════
        protected void btnRejectModalConfirm_Click(object sender, EventArgs e)
        {
            string mode = hfRejectMode.Value;
            string comment = (txt_rejectComment.Text ?? "").Replace("\r\n", "\n").Trim();
            string status = Convert.ToString(ViewState["STATUS_ID"]);
            string pageMode = (Request.QueryString["mode"] ?? "edit").ToLower();

            if (mode != "reject" && mode != "cancel")
            {
                alert_toast("ไม่พบโหมดการทำงาน กรุณาลองใหม่", "Error", "error", "5000");
                return;
            }

            // ── สิทธิ์ / สถานะ (กันปุ่มถูกเรียกจากหน้าที่ไม่ควรทำได้) ──
            if (mode == "reject" && (pageMode != "confirm" || status != "50"))
            {
                alert_toast("Reject ได้เฉพาะเอกสารที่รอ AAD Confirm (โหมด Approve)", "ไม่สามารถ Reject ได้", "warn", "8000");
                return;
            }
            if (mode == "cancel" && (pageMode == "return" || (status != "1" && status != "50")))
            {
                alert_toast("ยกเลิกได้เฉพาะเอกสารสถานะ Draft หรือรอ AAD Confirm", "ไม่สามารถยกเลิกได้", "warn", "8000");
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

            if (mode == "reject") DoReject(comment);
            else DoCancel(comment);
        }

        /// <summary>เปิด Modal ซ้ำหลัง Postback (คงค่าที่พิมพ์ไว้)</summary>
        private void ReopenRejectModal(string mode)
        {
            string js = "setRejectModalMode('" + HttpUtility.JavaScriptStringEncode(mode) + "');" +
                        "var t=document.getElementById('" + txt_rejectComment.ClientID + "'); if(t) t.classList.add('rjm-err');" +
                        "showRejectModal();";
            ScriptManager.RegisterStartupScript(this, GetType(), "reopenReject", js, true);
        }

        /// <summary>ผู้ดำเนินการ : โหมด confirm = ผู้ login / อื่น ๆ = ผู้ร้องขอ</summary>
        private void GetActor(out string empId, out string empName)
        {
            string pageMode = (Request.QueryString["mode"] ?? "edit").ToLower();
            if (pageMode == "confirm")
            {
                empId = Convert.ToString(Session["emp_id"]).Trim();
                empName = Convert.ToString(Session["user_name"]);
            }
            else
            {
                empId = txtEmpId.Text.Trim();
                empName = txtEmpName.Text;
            }
            empName = (empName ?? "").Trim().Split(' ')[0];
        }

        /// <summary>Reject → เอกสารกลับเป็น Draft (status 1) + บันทึกประวัติ</summary>
        private void DoReject(string comment)
        {
            string Doc_No = litIssueNo.Text.Trim();
            string status_id = Convert.ToString(ViewState["STATUS_ID"]);
            string empId, empName;
            GetActor(out empId, out empName);

            int Reject_Count = GetNextRejectCount(Doc_No);
            DateTime now = DateTime.Now;

            conn.Open();
            SqlTransaction tx = conn.BeginTransaction();
            try
            {
                // ── Header → 1 (เฉพาะที่ยังรอ Confirm อยู่) ──
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Borrow_Control_List SET status_id = 1, update_date = @dt" +
                    " WHERE Doc_No = @Doc_No AND status_id = 50", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@dt", now);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException("สถานะเอกสารเปลี่ยนไปแล้ว (ไม่ใช่รอ AAD Confirm) กรุณารีเฟรชหน้า");
                }

                // ── รายการ → 1 ──
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Borrow_List SET status_id = 1 WHERE Doc_No = @Doc_No", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.ExecuteNonQuery();
                }

                InsertRejectHistory(tx, Doc_No, status_id, Reject_Count, empId, empName, now, comment, "Reject");
                tx.Commit();
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                alert_toast("Reject ไม่สำเร็จ : " + ex.Message, "Error", "error", "10000");
                ReopenRejectModal("reject");
                return;
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close();
            }

            RedirectAfterRejectCancel(Doc_No, "Reject Done", "warn", "ApproveStatus.aspx");
        }

        /// <summary>Cancel → เอกสารสถานะ 999 + บันทึกประวัติ</summary>
        private void DoCancel(string comment)
        {
            string Doc_No = litIssueNo.Text.Trim();
            if (Doc_No == "-" || string.IsNullOrEmpty(Doc_No))
            {
                alert_toast("ไม่พบเลขเอกสาร ไม่สามารถยกเลิกได้", "Cancel Error", "error", "5000");
                return;
            }

            string status_id = Convert.ToString(ViewState["STATUS_ID"]).Trim();
            string empId, empName;
            GetActor(out empId, out empName);

            int Reject_Count = GetNextRejectCount(Doc_No);
            DateTime now = DateTime.Now;

            conn.Open();
            SqlTransaction tx = conn.BeginTransaction();
            try
            {
                // ── Header → 999 (เฉพาะ Draft / รอ Confirm) ──
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Borrow_Control_List" +
                    " SET status_id = 999, update_date = @dt, cancel_empid = @emp, cancel_date = @dt" +
                    " WHERE Doc_No = @Doc_No AND status_id IN (1, 50)", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@dt", now);
                    cmd.Parameters.AddWithValue("@emp", empId);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException("สถานะเอกสารเปลี่ยนไปแล้ว (ยกเลิกไม่ได้) กรุณารีเฟรชหน้า");
                }

                // ── รายการ → 999 ──
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Borrow_List SET status_id = 999 WHERE Doc_No = @Doc_No", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.ExecuteNonQuery();
                }

                InsertRejectHistory(tx, Doc_No, status_id, Reject_Count, empId, empName, now, comment, "Cancel");
                tx.Commit();
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                alert_toast("Cancel ไม่สำเร็จ : " + ex.Message, "Error", "error", "10000");
                ReopenRejectModal("cancel");
                return;
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close();
            }

            string pageMode = (Request.QueryString["mode"] ?? "edit").ToLower();
            RedirectAfterRejectCancel(Doc_No, "Cancel Complete", "info",
                pageMode == "confirm" ? "ApproveStatus.aspx" : "EquipmentStatus2.aspx");
        }

        private void InsertRejectHistory(SqlTransaction tx, string Doc_No, string statusId, int count,
                                            string empId, string empName, DateTime dt, string detail, string type)
        {
            const string sql = @"
                INSERT INTO Reject_Detail_All
                ( Doc_No , Doc_Type , status_id , Reject_Count ,
                  emp_id , name , Reject_Date , Reject_detail , Type )
                VALUES
                ( @Doc_No , 'AAD1' , @status_id , @Reject_Count ,
                  @emp_id , @name , @Reject_Date , @Reject_detail , @Type )";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                cmd.Parameters.AddWithValue("@status_id", statusId);
                cmd.Parameters.AddWithValue("@Reject_Count", count);
                cmd.Parameters.AddWithValue("@emp_id", empId);
                cmd.Parameters.AddWithValue("@name", empName);
                cmd.Parameters.AddWithValue("@Reject_Date", dt);
                cmd.Parameters.AddWithValue("@Reject_detail", detail);
                cmd.Parameters.AddWithValue("@Type", type);
                cmd.ExecuteNonQuery();
            }
        }

        private void RedirectAfterRejectCancel(string Doc_No, string title, string type, string link)
        {
            string jsDoc = HttpUtility.JavaScriptStringEncode(Doc_No);
            string script =
                "closeRejectModal();" +
                "document.getElementById('loadingOverlay').style.display = 'block';" +
                "if (typeof showToast === 'function') {" +
                "  showToast('Doc No : " + jsDoc + "', { title: '" + HttpUtility.JavaScriptStringEncode(title) +
                "', type: '" + type + "', duration: 2000 });" +
                "}" +
                "setTimeout(function () { window.location.replace('" + HttpUtility.JavaScriptStringEncode(link) + "'); }, 2000);";
            ScriptManager.RegisterStartupScript(this, GetType(), "rejectCancelDone", script, true);
        }

        // ── Helper : ดึงชื่อพนักงานจาก emp_id (fallback ผู้รับคืน) ──
        private string GetEmpNameById(string empId)
        {
            if (string.IsNullOrEmpty(empId)) return "";
            try
            {
                string str = "select emp_id + ' ' + name as fullname" +
                             " from user_master.dbo.sys_user_master where emp_id = @emp_id";
                dc.QryDT_P(conn, str,
                    new SqlParameter("@emp_id", SqlDbType.NVarChar) { Value = empId });
                return dc.dt.Rows.Count > 0 ? dc.dt.Rows[0]["fullname"].ToString() : empId;
            }
            catch { return empId; }
        }

        #endregion

        #region --- Change Item Click ---
        protected void ddlReason_Borrow_SelectedIndexChanged(object sender, EventArgs e)
        {
            //string reason_Id = ddlReason_Borrow.SelectedValue.ToString();
            string reasonId = ddlReason_Borrow.SelectedValue;
            string str = " select reason_ID , reason_Name , borrowFlow , comment_borrow , order_borrow" +
                         " from Reason_Borrow_Master" +
                         " where reason_id = @reason_id";
            dc.QryDT_P(conn, str,
                new SqlParameter("@reason_id", SqlDbType.NVarChar) { Value = reasonId });
            //dc.QryDT_P(conn, str);

            bool comment = false;

            if (dc.dt.Rows.Count > 0 && dc.dt.Rows[0]["comment_borrow"] != DBNull.Value)
            {
                comment = Convert.ToBoolean(dc.dt.Rows[0]["comment_borrow"]);
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
        // ============================================================
        // txtReturnEmpId_TextChanged — ผู้คืนกรอก EmpId แล้ว PostBack
        // ============================================================
        protected void txtReturnEmpId_TextChanged(object sender, EventArgs e)
        {
            string empId = txtReturnEmpId.Text.Trim();

            try
            {
                if (empId.Length < 6)
                {
                    txtReturnEmpName.Text = "";
                    txtReturnDept.Text = "";
                    txtReturnDeptCode.Text = "";
                    txtReturnEmpId.Style["background-color"] = "#00a7ff40";
                }
                else
                {
                    bool found = GetReturnerHRMS(empId);

                    if (!found)
                    {
                        txtReturnEmpName.Text = "";
                        txtReturnDept.Text = "";
                        txtReturnDeptCode.Text = "";
                        txtReturnEmpId.Style["background-color"] = "#ff000082";
                        alert_toast("ไม่พบ Employee ID นี้ในระบบ", "Warning", "warn", "5000");
                    }
                }
            }
            catch (Exception ex)
            {
                alert_toast(ex.Message, "txtReturnEmpId_TextChanged", "warn", "10000");
            }

            // ✅ เปิด Modal ค้างไว้หลัง PostBack
            ScriptManager.RegisterStartupScript(
                this, GetType(), "reopenReturnModal",
                "$('#modal_Return').modal('show');", true);
        }

        // ============================================================
        // GetReturnerHRMS — ดึง HRMS ลง textbox ของ Modal ผู้คืน
        //   (อ้างอิง logic เดียวกับ GetDataHRMS_CSharp แต่เขียนคนละ control)
        // ============================================================
        private bool GetReturnerHRMS(string userId)
        {
            if (!IsValidEmpNo(userId))
                return false;

            string url = "http://cptsvs547:1000/middleware/oracle/hrms";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            string sql =
                "select GNAME_ENG, FNAME_ENG, POSN_ENAME, BAND, DEPT_CODE, DEPT_ABB_NAME, EMAIL " +
                "from ADMIN.v_emp_data_all_cpt where UPPER(emp_no)=UPPER('" + userId + "')";

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

                txtReturnEmpName.Text = u.GNAME_ENG + " " + u.FNAME_ENG;
                txtReturnDept.Text = u.DEPT_ABB_NAME;
                txtReturnDeptCode.Text = u.DEPT_CODE;

                txtReturnEmpId.Style["background-color"] = "#d4edda";
                return true;
            }
        }

        #endregion

        protected void ddlLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlLocation = sender as DropDownList;
            if (ddlLocation == null) return;

            // ✅ หา GridViewRow ของ dropdown นี้
            GridViewRow row = ddlLocation.NamingContainer as GridViewRow;

            if (row == null) return;

            // ✅ เก็บ RowIndex ของแถวนี้
            //ViewState["SCROLL_ROW_INDEX"] = row.RowIndex;

            int rowIndex = row.RowIndex;

            // ✅ Register scroll script ที่นี่เลย
            #region
            string script =
                "setTimeout(function() {" +
                "  var row = document.getElementById('row_" + rowIndex + "');" +
                "  if (row) {" +
                "    row.scrollIntoView({ behavior: 'smooth', block: 'center' });" +
                "  }" +
                "}, 100);";

            ScriptManager.RegisterStartupScript(
                this,
                this.GetType(),
                "scrollBack_" + rowIndex, // ✅ key ไม่ซ้ำ
                script,
                true
            );
            #endregion

            string locationId = ddlLocation.SelectedValue;

            // ======================
            // ✅ Use Cell
            // ======================
            DropDownList ddlCell = row.FindControl("ddlCell") as DropDownList;
            if (ddlCell != null)
            {
                ddlCell.Items.Clear();

                if (!string.IsNullOrEmpty(locationId))
                {
                    DataTable dtCell = GetCellMaster();
                    ddlCell.DataSource = dtCell;
                    ddlCell.DataTextField = "location2_name";
                    ddlCell.DataValueField = "location2_id";
                    ddlCell.DataBind();
                }

                ddlCell.Items.Insert(0, new ListItem("-- Select --", ""));
                ddlCell.SelectedIndex = 0;
            }

            // ======================
            // ✅ ddlStation
            // ======================
            DropDownList ddlStation = row.FindControl("ddlStation") as DropDownList;
            if (ddlStation != null)
            {
                ddlStation.Items.Clear();

                if (!string.IsNullOrEmpty(locationId))
                {
                    DataTable dtStation = GetStationMaster();
                    ddlStation.DataSource = dtStation;
                    ddlStation.DataTextField = "location3_name";
                    ddlStation.DataValueField = "location3_id";
                    ddlStation.DataBind();
                }

                ddlStation.Items.Insert(0, new ListItem("-- Select --", ""));
                ddlStation.SelectedIndex = 0;
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

            // ── ตรวจ DisplayStatus : ติดกระบวนการ AAD ใด ๆ → ไม่ให้ Add ──
            DataTable dtSearch = ViewState["SearchResult"] as DataTable;
            if (dtSearch != null && rowIndex < dtSearch.Rows.Count)
            {
                string ds = Convert.ToString(dtSearch.Rows[rowIndex]["DisplayStatus"]);
                if (ds.StartsWith("AAD"))
                {
                    string doc = dtSearch.Columns.Contains("ProcDoc") ? Convert.ToString(dtSearch.Rows[rowIndex]["ProcDoc"]) : "";
                    alert_toast("อุปกรณ์นี้อยู่ระหว่างดำเนินการ : " + ds + (doc != "" ? " [" + doc + "]" : "") +
                                "\nไม่สามารถเพิ่มรายการได้", "ไม่สามารถเพิ่มได้", "warn", "8000");
                    return;
                }
            }

            // ── ตรวจซ้ำ (ข้าม row ที่ถูก mark Delete) ────────────────────────────
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

            // ── Query ดึงข้อมูล Tool ──────────────────────────────────────────────
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

            // ── เพิ่ม row ใหม่ ────────────────────────────────────────────────────
            DataTable dt = BorrowListDT;

            // นับ RowNo จาก row ที่ไม่ใช่ Delete
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
            newRow["LocationName"] = tool.Rows[0]["location"];
            newRow["LocationId"] = tool.Rows[0]["location1_id"];
            newRow["CellName"] = tool.Rows[0]["cell"];
            newRow["CellId"] = tool.Rows[0]["location2_id"];
            newRow["StationName"] = tool.Rows[0]["station"];
            newRow["StationId"] = tool.Rows[0]["station_id"];
            newRow["CompareData"] = "Insert"; // ✅ mark Insert ทันที

            dt.Rows.Add(newRow);
            BorrowListDT = dt;
            //Save_BorrowListDT();
            BindBorrowList_Display(); // ✅ แสดงเฉพาะ row ที่ไม่ใช่ Delete

            // Clear Table Search
            txtFixAsset.Text = "";
            ViewState["SearchResult"] = null;
            gvSearchResult.DataSource = null;
            gvSearchResult.DataBind();

        }
        protected void gvSearchResult_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            e.Row.Attributes["id"] = "rowSearch_" + e.Row.RowIndex;

            DataRowView rowView = e.Row.DataItem as DataRowView;
            if (rowView == null) return;

            string displayStatus = Convert.ToString(rowView["DisplayStatus"]);
            if (!displayStatus.StartsWith("AAD")) return;   // ไม่ติดกระบวนการ

            // ✅ ติดกระบวนการใด ๆ → ปิดปุ่ม Add + ไฮไลต์แถว
            e.Row.CssClass = "row-locked";

            Button btnAdd = e.Row.Cells[1].Controls.OfType<Button>().FirstOrDefault();
            if (btnAdd != null)
            {
                btnAdd.Enabled = false;
                btnAdd.ToolTip = "อุปกรณ์นี้อยู่ระหว่างดำเนินการ : " + displayStatus +
                                 " [" + Convert.ToString(rowView["ProcDoc"]) + "]";
                btnAdd.CssClass = "btn btn-secondary btn-sm";
            }
        }
        protected void gvBorrowList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "RemoveItem") return;

            int rowIndex = Convert.ToInt32(e.CommandArgument);

            if (rowIndex < 0 || rowIndex >= gvBorrowList.Rows.Count) return;

            GridViewRow gvRow = gvBorrowList.Rows[rowIndex];

            HiddenField hdToolId = gvRow.FindControl("hdtoolId") as HiddenField;
            HiddenField hdToolSub = gvRow.FindControl("hdtoolSub") as HiddenField;
            HiddenField hdSystemId = gvRow.FindControl("hdsystemId") as HiddenField;

            if (hdToolId == null || hdToolSub == null || hdSystemId == null)
            {
                alert_toast("ไม่พบข้อมูล Hidden Field", "Remove Error", "error", "5000");
                return;
            }

            string toolId = hdToolId.Value;
            string toolSub = hdToolSub.Value;
            string systemId = hdSystemId.Value;

            // ── STEP 1 : Sync GridView → BorrowListDT (ไม่ Compare) ──────────────
            SyncGridViewToBorrowListDT();

            DataTable dt = BorrowListDT;

            // ── STEP 2 : ค้นหา row ด้วย Key ──────────────────────────────────────
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

            // ── STEP 3 : ลบหรือ mark Delete ──────────────────────────────────────
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

            // ── STEP 4 : บันทึกและ Bind ──────────────────────────────────────────
            // ✅ ไม่เรียก Save_BorrowListDT() เพราะจะ query DB แล้วเพิ่ม row กลับมา
            BorrowListDT = dt;
            BindBorrowList_Display();
        }
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

                // ── USE : Dept = txtDeptCode ผู้ยืม ──
                Label lblDeptNew = e.Row.FindControl("lblDeptNew") as Label;
                if (lblDeptNew != null) lblDeptNew.Text = txtDeptCode.Text;

                // ── USE : Model / Cell / Station (bind Dropdown ทุกโหมด) ──
                BindLocField(e.Row, cRow, "ddlModel", "txtModel", "hfModelMode", "btnModelToggle",
                    "HD_Model", "Use_ModelId", "Add_Model",
                    (DataTable)ViewState["MODEL_MASTER"], "model_name", "model_id");

                BindLocField(e.Row, cRow, "ddlCell", "txtCell", "hfCellMode", "btnCellToggle",
                    "HD_Cell", "Use_CellId", "Add_Cell",
                    _cellM ?? GetCellMaster(), "location2_name", "location2_id");

                BindLocField(e.Row, cRow, "ddlStation", "txtStation", "hfStationMode", "btnStationToggle",
                    "HD_Station", "Use_StationId", "Add_Station",
                    _stationM ?? GetStationMaster(), "location3_name", "location3_id");
            }
        }

        /// <summary>
        /// bind ช่อง Use : Model / Cell / Station
        ///   - Dropdown : bind รายการ "เสมอ" ไม่ว่าโหมดไหน (สลับกลับมาแล้วต้องมีให้เลือก)
        ///   - Textbox  : ค่าที่กรอกเอง
        ///   - HiddenField = โหมดปัจจุบัน (ddl / txt) — JS ใช้ตัวนี้จัดการแสดงผล
        /// </summary>
        private void BindLocField(GridViewRow row, DataRow cRow,
                                    string ddlId, string txtId, string hfId, string btnId,
                                    string hdCol, string idCol, string txtCol,
                                    DataTable master, string mText, string mValue)
        {
            var ddl = row.FindControl(ddlId) as DropDownList;
            var txt = row.FindControl(txtId) as TextBox;
            var hf = row.FindControl(hfId) as HiddenField;
            var btn = row.FindControl(btnId) as System.Web.UI.HtmlControls.HtmlButton;
            bool isTxt = ColVal(cRow, hdCol) == "txt";

            if (ddl != null)
            {
                if (ddl.Items.Count == 0 && master != null)
                {
                    ddl.DataSource = master;
                    ddl.DataTextField = mText;
                    ddl.DataValueField = mValue;
                    ddl.DataBind();
                    ddl.Items.Insert(0, new ListItem("-- Select --", ""));
                }

                string v = ColVal(cRow, idCol);
                if (!string.IsNullOrEmpty(v) && ddl.Items.FindByValue(v) != null)
                    ddl.SelectedValue = v;

                ddl.Style["display"] = isTxt ? "none" : "inline-block";
            }

            if (txt != null)
            {
                txt.Text = ColVal(cRow, txtCol);
                txt.Style["display"] = isTxt ? "inline-block" : "none";
            }

            if (hf != null) hf.Value = isTxt ? "txt" : "ddl";
            if (btn != null) btn.InnerText = isTxt ? "-" : "+";
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

                // Location
                //DropDownList ddlLocation = gvRow.FindControl("ddlLocation") as DropDownList;
                //if (ddlLocation != null)
                //    dr["Use_LocationId"] = string.IsNullOrEmpty(ddlLocation.SelectedValue)
                //                           ? (object)DBNull.Value : ddlLocation.SelectedValue;

                // Cell
                DropDownList ddlCell = gvRow.FindControl("ddlCell") as DropDownList;
                TextBox txtCell = gvRow.FindControl("txtCell") as TextBox;
                HiddenField hfCellMode = gvRow.FindControl("hfCellMode") as HiddenField;

                if (hfCellMode != null)
                    dr["HD_Cell"] = string.IsNullOrEmpty(hfCellMode.Value)
                                       ? (object)DBNull.Value : hfCellMode.Value;
                if (ddlCell != null)
                {
                    dr["Use_CellId"] = string.IsNullOrEmpty(ddlCell.SelectedValue)
                           ? (object)DBNull.Value : ddlCell.SelectedValue;
                }

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
                {
                    dr["Use_StationId"] = string.IsNullOrEmpty(ddlStation.SelectedValue)
                              ? (object)DBNull.Value : ddlStation.SelectedValue;
                }

                if (txtStation != null)
                    dr["Add_Station"] = string.IsNullOrEmpty(txtStation.Text)
                                          ? (object)DBNull.Value : txtStation.Text;
            }

            BorrowListDT = borrowDT;
        }

        // ✅ Bind GridView เฉพาะ row ที่ user เห็น (ไม่รวม Delete)
        private void BindBorrowList_Display()
        {
            _cellM = GetCellMaster().Copy();        // ✅ query ครั้งเดียว (เดิม query ทุกแถว)
            _stationM = GetStationMaster().Copy();

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

        // --- Data Borrow List
        private void BindBorrowList()
        {
            BindBorrowList_Display();
        }


        #region --- Helper : Check Deplicate-key Data ---

        //=====================================================================
        // --- SQL Server error: 2627 = unique constraint, 2601 = unique index
        //=====================================================================
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


        #region --- Set Page Mode ---

        private void SetViewMode(string status_id)
        {

            SetStepManualVisible(false);

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
            if (status_id == "90" || status_id == "100" || status_id == "999") //During Borrow , Completed , Cancel
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

            //Show Comment History : เฉพาะเมื่อมีประวัติ
            ShowRejectHistoryIfAny();

        }
        private void SetConfirmMode(string status_id)
        {
            SetStepManualVisible(false);
            if (status_id == "50") litModeGuide.Text = BuildModeGuide("Confirm");

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
            if (ReturnButton != null) ReturnButton.Visible = false;            

            //Borrow Note
            borrow_note.Visible = true;

            //Show Comment History : เฉพาะเมื่อมีประวัติ
            ShowRejectHistoryIfAny();
        }
        private void SetReturnMode(string status_id)
        {
            SetStepManualVisible(false);
            if (status_id == "90") litModeGuide.Text = BuildModeGuide("Return");

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
            // CancelButton , DraftButton , RejectButton , SendButton , ConfirmButton , ConfirmButton2 , ReturnButton
            DraftButton.Visible = false;
            RejectButton.Visible = false;
            SendButton.Visible = false;
            ConfirmButton.Visible = false;
            CancelButton.Visible = false;
            ReturnButton.Visible = true;

            //Borrow Note
            borrow_note.Visible = true;

            //Show Comment History : เฉพาะเมื่อมีประวัติ
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

        /// <summary>ปุ่มคู่มือ Step 1–4 เป็นของผู้ร้องขอ — แสดงเฉพาะโหมด Issue</summary>
        private void SetStepManualVisible(bool show)
        {
            //foreach (HtmlAnchor a in new[] { btnManual1, btnManual2, btnManual3, btnManual4 })
            //    if (a != null) a.Visible = show;
        }

        /// <summary>แถบนำทาง + ปุ่มคู่มือ : "Confirm" (Status 50) / "Return" (Status 90)</summary>
        private string BuildModeGuide(string file)
        {
            string title, btnText, css = "mode-guide";
            string[] steps;

            if (file == "Return")
            {
                css += " mg-return";
                title = "ขั้นตอนการรับคืนอุปกรณ์";
                btnText = "คู่มือ Return";
                steps = new[] { "ตรวจรายการอุปกรณ์ที่นำมาคืน",
                        "กด Return Confirm แล้วกรอกรหัสผู้คืน (6 หลัก)",
                        "กด Confirm Return เพื่อปิดเอกสาร" };
            }
            else
            {
                title = "ขั้นตอน AAD Confirm";
                btnText = "คู่มือ AAD Confirm";
                steps = new[] { "ตรวจข้อมูลผู้ร้องขอและรายการยืม",
                        "ตรวจสถานที่นำไปใช้งาน (Use : Model / Cell / Station)",
                        "อนุมัติกด AAD Confirm / ไม่อนุมัติกด Reject (เหตุผล ≥ 10 ตัวอักษร)" };
            }

            var sb = new StringBuilder("<div class='" + css + "'>");
            sb.Append("<span class='mg-title'>" + Server.HtmlEncode(title) + "</span><ol>");
            foreach (string s in steps) sb.Append("<li>" + Server.HtmlEncode(s) + "</li>");
            sb.Append("</ol>");
            sb.Append("<a href='#' class='mg-btn' onclick=\"return openManualFile('" + file + "');\">" +
                      "<span class='glyphicon glyphicon-book'></span> " + Server.HtmlEncode(btnText) + "</a>");
            sb.Append("</div>");
            return sb.ToString();
        }

        /// <summary>Section 5 : แสดงเฉพาะเมื่อมีประวัติ Reject / Cancel (เรียกหลัง Bind_History_Reject)</summary>
        private void ShowRejectHistoryIfAny()
        {
            pnl_rejectComment.Visible = gv_rejectHistory.Rows.Count > 0;
        }

        #endregion

        #region --- Get Master ---

        private DataTable GetDeptMaster()
        {
            DataTable dt_dept = new DataTable();

            string str_dept = "select LTRIM(RTRIM(dept_code)) as ID , LTRIM(RTRIM(dept_code)) as dept_code from View_dept_master order by dept_code";
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
        private DataTable GetLocationMaster()
        {
            DataTable dt_location = new DataTable();

            string str_location = "select * from jig_tool_system.dbo.master_location1 where is_use = 1 and location1_id IN(97,99)";
            dc.QryDT(conn, str_location);
            dt_location = dc.dt;
            return dt_location;
        }
        private void GetReasonBorrowMaster()
        {
            DataTable dt_reasonBorrow = new DataTable();

            string str_reasonBorrow = " select reason_ID , reason_Name , borrowFlow , comment_borrow , order_borrow" +
                                      " from Reason_Borrow_Master" +
                                      " Where borrowFlow = 'true'" +
                                      " order by order_borrow";
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

        #region --- Create Table Date ---

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
            dt.Columns.Add("LocationName", typeof(string));
            dt.Columns.Add("LocationId", typeof(string));
            dt.Columns.Add("CellName", typeof(string));
            dt.Columns.Add("CellId", typeof(string));
            dt.Columns.Add("StationName", typeof(string));
            dt.Columns.Add("StationId", typeof(string));

            //Use
            dt.Columns.Add("Use_DeptCode", typeof(string));

            dt.Columns.Add("Use_ModelId", typeof(string));
            dt.Columns.Add("Add_Model", typeof(string));
            dt.Columns.Add("HD_Model", typeof(string));

            dt.Columns.Add("Use_LocationId", typeof(string));

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

        #endregion

        #region --- Requester Snapshot (Requester_Info) ---

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
        /// ข้ามเมื่อข้อมูลบนหน้าจอมาจาก Snapshot เอง (กันเขียนทับ source ผิด)
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

        /// <summary>เปิดเอกสารเก่า + เจอใน HRMS → เติม Snapshot ถ้ายังไม่มี (ไม่ให้หน้าพังถ้าบันทึกไม่ได้)</summary>
        private void SaveRequesterInfo_IfMissing(string Doc_No, string tel)
        {
            try
            {
                using (var c = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString))
                {
                    c.Open();
                    SaveRequesterInfo(c, null, Doc_No, false, tel);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Requester_Info backfill] " + Doc_No + " : " + ex.Message);
            }
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

            if (dc.dt.Rows.Count == 0) return false;

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

        /// <summary>แถบแจ้งเตือนว่าไม่พบใน HRMS</summary>
        private void ShowRequesterNotice(string empId, bool fromSnap, DateTime? savedAt)
        {
            string id = Server.HtmlEncode(empId);
            string why = _hrmsError
                ? "ไม่สามารถเชื่อมต่อระบบ HRMS ได้ในขณะนี้"
                : "ไม่พบรหัสพนักงาน " + id + " ในระบบ HRMS (อาจลาออก / ย้ายหน่วยงาน)";

            if (fromSnap)
            {
                string when = savedAt.HasValue
                    ? savedAt.Value.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture) : "-";
                string partial = Convert.ToString(ViewState["REQ_SNAP_TYPE"]) == "BACKFILL"
                    ? " (ข้อมูลย้อนหลังจาก User Master — บางช่องอาจว่าง)" : "";

                litReqSource.Text =
                    "<div class='req-src-banner'>" +
                      "<b>⚠️ " + why + "</b>" +
                      "แสดงข้อมูลผู้ร้องขอจาก Record ของเอกสาร (บันทึกเมื่อ " + when + ")" + partial +
                    "</div>";
            }
            else
            {
                litReqSource.Text =
                    "<div class='req-src-banner req-src-none'>" +
                      "<b>⚠️ " + why + "</b>" +
                      "และไม่พบ Record ข้อมูลผู้ร้องขอของเอกสารนี้ — แสดงเฉพาะรหัสพนักงานและ Dept Code" +
                    "</div>";
            }

            txtEmpId.Style["background-color"] = fromSnap ? "#fdf6e3" : "#fbeceb";
            pnlReqSource.Visible = true;
        }

        #endregion

        #region --- Helper : On Process Check (AAD1–AAD4) ---

        [Serializable]
        private class ProcInfo
        {
            public string Label;   // "AAD3 : เบิก"
            public string DocNo;   // "AAD3-26-00039"
            public string Url;     // withdraw2.aspx?k=...&mode=view
        }

        private static string ProcKey(object toolId, object subId, object systemId)
        {
            return Convert.ToString(toolId).Trim() + "|" + Convert.ToString(subId).Trim() + "|" + Convert.ToString(systemId).Trim();
        }

        /// <summary>
        /// อุปกรณ์ทั้งหมดที่ติดกระบวนการอยู่ (1 &lt; status_id &lt; 100) — key = tool_id|sub_id|system_id
        /// ไม่รวมเอกสารยืมฉบับปัจจุบัน (currentDocNo) / ตัวเดียวติดหลายเอกสาร → เอาตามลำดับ AAD1 → AAD4
        /// </summary>
        private Dictionary<string, ProcInfo> GetProcessMap(string currentDocNo)
        {
            var map = new Dictionary<string, ProcInfo>(StringComparer.OrdinalIgnoreCase);

            string sql = @"
                SELECT 1 AS ord, Doc_No, tool_id, sub_id, system_id FROM Borrow_List
                 WHERE status_id > 1 AND status_id < 100 AND Doc_No <> @doc
                UNION ALL
                SELECT 2, Doc_No, tool_id, sub_id, system_id FROM NotificationofChange_List
                 WHERE status_id > 1 AND status_id < 100
                UNION ALL
                SELECT 3, Doc_No, tool_id, sub_id, system_id FROM Withdraw_List
                 WHERE status_id > 1 AND status_id < 100
                UNION ALL
                SELECT 4, Doc_No, tool_id, sub_id, system_id FROM Storage_List
                 WHERE status_id > 1 AND status_id < 100
                ORDER BY ord";

            dc.QryDT_P(conn, sql,
                new SqlParameter("@doc", SqlDbType.NVarChar)
                { Value = string.IsNullOrEmpty(currentDocNo) ? "-" : currentDocNo });

            foreach (DataRow r in dc.dt.Rows)
            {
                string key = ProcKey(r["tool_id"], r["sub_id"], r["system_id"]);
                if (map.ContainsKey(key)) continue;   // เก็บอันแรกตามลำดับ ord

                string docNo = GetVal(r, "Doc_No");
                string label, page;
                switch (Convert.ToInt32(r["ord"]))
                {
                    case 1: label = "AAD1 : ยืม"; page = "borrow_tool2.aspx"; break;
                    case 2: label = "AAD2 : แจ้งเปลี่ยนแปลง"; page = "NotificationOfChange.aspx"; break;
                    case 3: label = "AAD3 : เบิก"; page = "withdraw2.aspx"; break;
                    default: label = "AAD4 : จัดเก็บ"; page = "storage.aspx"; break;
                }

                string k = Convert.ToBase64String(Encoding.UTF8.GetBytes(docNo));
                map[key] = new ProcInfo
                {
                    Label = label,
                    DocNo = docNo,
                    Url = page + "?k=" + HttpUtility.UrlEncode(k) + "&mode=view"
                };
            }
            return map;
        }

        /// <summary>badge สถานะ : ติดกระบวนการ → ลิงก์เปิดเอกสาร (แท็บใหม่) + เลขเอกสาร</summary>
        protected string BuildStatusBadge(object displayStatus, object procUrl, object procDoc)
        {
            string ds = Convert.ToString(displayStatus);
            string url = Convert.ToString(procUrl);
            string doc = Convert.ToString(procDoc);
            string cls = GetStatusBadgeClass(ds);

            if (string.IsNullOrEmpty(url))
                return "<span class='badge " + cls + "'>" + HttpUtility.HtmlEncode(ds) + "</span>";

            return "<a href='" + HttpUtility.HtmlAttributeEncode(url) + "' target='_blank' rel='noopener'" +
                   " class='badge " + cls + " badge-link'" +
                   " title='อยู่ระหว่างดำเนินการ — คลิกเพื่อดูเอกสาร " + HttpUtility.HtmlAttributeEncode(doc) + "'>" +
                   HttpUtility.HtmlEncode(ds) + " &#128279;</a>" +
                   "<div class='proc-doc'>" + HttpUtility.HtmlEncode(doc) + "</div>";
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

                // ประกอบสคริปต์ (duration ไม่ต้อง encode และไม่ต้องใส่ quote)
                string script =
                    "showToast('" + jsDetail + "', { " +
                        "title: '" + jsTitle + "', " +
                        "type: '" + jsType + "', " +
                        "duration: " + durationMs + " " +
                    "});";

                // ใช้ key ไม่ซ้ำ ป้องกันการชนกัน
                string key = "toast_" + DateTime.UtcNow.Ticks;
                ScriptManager.RegisterStartupScript(this, this.GetType(), key, script, true);
            }
        }

        // ── Helper : DropDownList warning ─────────────────────────────
        private void SetWarning_DDL(DropDownList ddl, bool isWarning)
        {
            if (ddl == null) return;

            if (isWarning)
            {
                // ✅ ใส่ style ตรงๆ บน <select> — select2 จะ copy attribute นี้ไปด้วย
                ddl.Style["border"] = "2px solid #ffc107";
                ddl.Style["background-color"] = "#fff3cd";

                // ✅ เก็บ class ไว้ด้วยเผื่อ select2 ไม่ได้ active
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

        // ── Helper : TextBox warning ──────────────────────────────────
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

        //=====================================================================
        // alert + redirect (ย้ายออกมาจาก InsertBorrowList เดิม)
        // เรียกหลัง commit สำเร็จเท่านั้น
        //=====================================================================
        private void ShowSaveSuccessAndRedirect(string Doc_No, string status_id)
        {
            string alert_info = "";
            switch (status_id)
            {
                case "1": alert_info = "Save Draft"; break;
                case "50": alert_info = "Send Complete"; break;
                case "90": alert_info = "Confirm Complete"; break;
                case "100": alert_info = "Return Complete"; break;
                case "999": alert_info = "Cancel Request"; break;
            }

            string link_path = (status_id == "90") ? "ApproveStatus.aspx" : "EquipmentStatus2.aspx";

            string jsDoc = HttpUtility.JavaScriptStringEncode(Doc_No);
            string jsInfo = HttpUtility.JavaScriptStringEncode(alert_info);

            string script =
                "document.getElementById('loadingOverlay').style.display = 'block';" +
                "if (typeof alert_toast === 'function') {" +
                "  showToast('Doc No : " + jsDoc + "', { title: '" + jsInfo + "', type: 'info', duration: 2000 });" +
                "}" +
                "setTimeout(function () { window.location.href = '" + link_path + "'; }, 2000);";

            ScriptManager.RegisterStartupScript(this, this.GetType(), "redirectScript", script, true);
        }

        #endregion

        #region --- Helper : เก็บค่า Employee ลง ViewState

        private void SaveEmployeeToViewState()
        {
            ViewState["EMP_ID"] = txtEmpId.Text;
            ViewState["EMP_NAME"] = txtEmpName.Text;
            ViewState["EMP_POSITION"] = txtPosition.Text;
            ViewState["EMP_DEPT"] = txtDept.Text;
            ViewState["EMP_BAND"] = hdtxtBand.Value;
            ViewState["EMP_EMAIL"] = hdtxtEmail.Value;
        }
        // ── Helper : เลือก CSS class ตาม DisplayStatus ────────────────
        protected string GetStatusBadgeClass(string displayStatus)
        {
            displayStatus = displayStatus ?? "";

            if (displayStatus.StartsWith("AAD"))
                return "badge-borrowing";   // แดง — ติดกระบวนการ

            if (displayStatus.StartsWith("Spare"))
                return "badge-spare";

            return "badge-normal";
        }

        // ✅ ใช้แทน local function — .NET 4.0 compatible
        private string GetRowVal(DataRowView row, string colName)
        {
            if (!row.Row.Table.Columns.Contains(colName)) return "";
            return row[colName] == DBNull.Value ? "" : row[colName].ToString().Trim();
        }

        // ── Helper : อ่านค่าจาก DataRow ป้องกัน DBNull ──────────────────
        private string ColVal(DataRow row, string colName)
        {
            if (!row.Table.Columns.Contains(colName)) return "";
            return row[colName] == DBNull.Value ? "" : row[colName].ToString().Trim();
        }
        // ── Helper : อ่าน string จาก DataRow ป้องกัน DBNull (.NET 4.0) ────────
        private string GetVal(DataRow row, string colName)
        {
            if (row == null) return "";
            if (!row.Table.Columns.Contains(colName)) return "";
            return row[colName] == DBNull.Value ? "" : row[colName].ToString().Trim();
        }
        // ── Helper : แปลงค่าจาก DataRow → object (DBNull ถ้าว่าง) ──────────────
        private object ToDbVal(DataRow row, string colName)
        {
            if (!row.Table.Columns.Contains(colName)) return DBNull.Value;
            if (row[colName] == DBNull.Value) return DBNull.Value;
            if (string.IsNullOrEmpty(row[colName].ToString().Trim())) return DBNull.Value;
            return row[colName];
        }

        #endregion

        #region --- Helper : GetNext Reject / GetNextDocNo
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
                " from Borrow_Control_List WITH (UPDLOCK, HOLDLOCK)" +
                " where year(issue_date) = @yr";

            using (var cmd = new SqlCommand(sql, c, tx))
            {
                cmd.Parameters.Add(new SqlParameter("@yr", SqlDbType.Int) { Value = issueYear });
                object val = cmd.ExecuteScalar();
                return Convert.ToInt32(val);
            }
        }
        #endregion

        #region --- Helper : สร้าง field row ใน HTML
        // ── Helper : สร้าง field row ใน HTML ─────────────────────────────────────
        private void AppendField(System.Text.StringBuilder sb, string key, string val)
        {
            string valHtml = string.IsNullOrEmpty(val)
                ? "<span class='val-empty'>(empty)</span>"
                : "<span class='val'>" + HttpUtility.HtmlEncode(val) + "</span>";

            sb.Append("<div class='field'><span class='key'>" + key + "</span> : " + valHtml + "</div>");
        }
        // ── Helper : field row พร้อม highlight ค่าที่เปลี่ยน ──────────────────────
        private void AppendFieldChanged(System.Text.StringBuilder sb,
            string key, string newVal, string oldVal, bool isChanged)
        {
            string newHtml = string.IsNullOrEmpty(newVal)
                ? "<span class='val-empty'>(empty)</span>"
                : "<span class='val-new'>" + HttpUtility.HtmlEncode(newVal) + "</span>";

            string fieldClass = isChanged ? "field field-changed" : "field";

            sb.Append("<div class='" + fieldClass + "'>");
            sb.Append("<span class='key'>" + key + "</span> : ");
            sb.Append(newHtml);

            // แสดงค่าเดิมถัดจากลูกศร ถ้าเปลี่ยน
            if (isChanged && oldVal != null)
            {
                string oldHtml = string.IsNullOrEmpty(oldVal)
                    ? "<span class='val-empty'>(empty)</span>"
                    : "<span class='val-old'>" + HttpUtility.HtmlEncode(oldVal) + "</span>";

                sb.Append(" &nbsp;<span class='arrow'>←</span>&nbsp; ");
                sb.Append(oldHtml);
            }

            sb.Append("</div>");
        }
        #endregion

        #region --- Helper validate emp_no สำหรับ HRMS
        /*
           emp_no มี format ตายตัว (ปรับ regex ให้ตรงของจริง)
           ตัวอย่างนี้: ตัวอักษร/ตัวเลข ความยาว 6–10
           อะไรที่ไม่เข้า pattern = ปฏิเสธ ไม่ส่งไป middleware เลย
        */
        private static readonly System.Text.RegularExpressions.Regex _empNoPattern =
            new System.Text.RegularExpressions.Regex(
                @"^[A-Za-z0-9]{6,10}$",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        private bool IsValidEmpNo(string empNo)
        {
            return !string.IsNullOrEmpty(empNo) && _empNoPattern.IsMatch(empNo);
        }
        #endregion

        #region --- Helper : Check Tel / Comment Borrow ---
        private bool ValidateTel()
        {
            string tel = txtTel.Text.Trim();

            if (!System.Text.RegularExpressions.Regex.IsMatch(tel, @"^\d{4}$"))
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
                "select comment_borrow from Reason_Borrow_Master" +
                " where reason_id = @reason_id";

            dc.QryDT_P(conn, str,
                new SqlParameter("@reason_id", SqlDbType.NVarChar) { Value = reasonId });

            bool requireComment = false;
            if (dc.dt.Rows.Count > 0 && dc.dt.Rows[0]["comment_borrow"] != DBNull.Value)
                requireComment = Convert.ToBoolean(dc.dt.Rows[0]["comment_borrow"]);

            // ── 3) ไม่บังคับ → ผ่าน ──
            if (!requireComment)
                return true;

            // ── 4) บังคับ → เช็ค comment ──
            string comment = txt_borrowComment.Text.Trim();

            // 4.1 ไม่มี comment
            if (string.IsNullOrEmpty(comment))
            {
                alert_toast("เหตุผลการยืมนี้ต้องระบุ Comment", "กรุณากรอก Comment", "warn", "5000");
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
        #endregion




        // --- Debug
        // ══════════════════════════════════════════════════════════════════════
        // DEBUG : แสดงข้อมูล BorrowListDT
        // ══════════════════════════════════════════════════════════════════════
        private void Debug_BorrowListDT()
        {
            DataTable dt = BorrowListDT;

            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("BorrowListDT : ไม่มีข้อมูล (0 rows)", "DEBUG", "warn", "5000");
                return;
            }

            System.Text.StringBuilder html = new System.Text.StringBuilder();

            // ── HTML สำหรับ popup window ──────────────────────────────────────────
            html.Append("<html><head><meta charset='UTF-8'>");
            html.Append("<title>DEBUG : BorrowListDT</title>");
            html.Append("<style>");
            html.Append("body { font-family: Consolas, monospace; font-size: 12px; margin: 10px; background:#1e1e1e; color:#d4d4d4; }");
            html.Append("h2 { color:#569cd6; border-bottom:1px solid #444; padding-bottom:6px; }");
            html.Append(".row-block { background:#252526; border:1px solid #3c3c3c; border-radius:6px; margin-bottom:10px; padding:10px; }");
            html.Append(".row-title { font-size:13px; font-weight:bold; color:#9cdcfe; margin-bottom:6px; }");
            html.Append(".section { color:#c586c0; font-weight:bold; margin:6px 0 2px 0; }");
            html.Append(".field { margin-left:16px; line-height:1.8; }");
            html.Append(".field span.key { color:#9cdcfe; min-width:160px; display:inline-block; }");
            html.Append(".field span.val { color:#ce9178; }");
            html.Append(".field span.val-empty { color:#555; font-style:italic; }");
            html.Append(".badge { display:inline-block; padding:2px 10px; border-radius:10px; font-size:11px; font-weight:bold; margin-left:8px; }");
            html.Append(".Same   { background:#1e4620; color:#4ec94e; }");
            html.Append(".Update { background:#4d3b00; color:#ffc107; }");
            html.Append(".Insert { background:#003153; color:#4fc1ff; }");
            html.Append(".Delete { background:#4d0000; color:#f44747; }");
            html.Append(".summary { background:#252526; border:1px solid #569cd6; border-radius:6px; padding:10px; margin-bottom:14px; line-height:2; }");
            html.Append(".btn-close { position:fixed; top:10px; right:10px; background:#c0392b; color:#fff; border:none; padding:6px 16px; border-radius:6px; cursor:pointer; font-size:13px; }");
            html.Append("</style></head><body>");
            html.Append("<button class='btn-close' onclick='window.close()'>✕ ปิด</button>");
            html.Append("<h2>🔍 DEBUG : BorrowListDT &nbsp;<small style='color:#888;font-size:12px;'>Total : " + dt.Rows.Count + " rows</small></h2>");

            // ── Summary ───────────────────────────────────────────────────────────
            int cntSame = 0; int cntUpdate = 0;
            int cntInsert = 0; int cntDelete = 0;

            foreach (DataRow r in dt.Rows)
            {
                switch (GetVal(r, "CompareData"))
                {
                    case "Same": cntSame++; break;
                    case "Update": cntUpdate++; break;
                    case "Insert": cntInsert++; break;
                    case "Delete": cntDelete++; break;
                }
            }

            html.Append("<div class='summary'>");
            html.Append("  <span class='badge Same'>Same : " + cntSame + "</span>&nbsp;");
            html.Append("  <span class='badge Update'>Update : " + cntUpdate + "</span>&nbsp;");
            html.Append("  <span class='badge Insert'>Insert : " + cntInsert + "</span>&nbsp;");
            html.Append("  <span class='badge Delete'>Delete : " + cntDelete + "</span>");
            html.Append("</div>");

            // ── แต่ละ Row ─────────────────────────────────────────────────────────
            foreach (DataRow row in dt.Rows)
            {
                string cmp = GetVal(row, "CompareData");
                string badgeHtml = "<span class='badge " + cmp + "'>" + cmp + "</span>";

                html.Append("<div class='row-block'>");
                html.Append("<div class='row-title'>Row No : " + GetVal(row, "RowNo") + " " + badgeHtml + "</div>");

                // KEY
                html.Append("<div class='section'>▸ KEY</div>");
                AppendField(html, "ToolId", GetVal(row, "ToolId"));
                AppendField(html, "ToolSub", GetVal(row, "ToolSub"));
                AppendField(html, "SystemId", GetVal(row, "SystemId"));

                // INFO
                html.Append("<div class='section'>▸ INFO</div>");
                AppendField(html, "FixAssetNo", GetVal(row, "FixAssetNo"));
                AppendField(html, "ControlNo", GetVal(row, "ControlNo"));
                AppendField(html, "ToolNo", GetVal(row, "ToolNo"));
                AppendField(html, "DeptCode", GetVal(row, "DeptCode"));

                // ORIGINAL
                html.Append("<div class='section'>▸ ORIGINAL</div>");
                AppendField(html, "ModelName", GetVal(row, "ModelName"));
                AppendField(html, "LocationName", GetVal(row, "LocationName"));
                AppendField(html, "CellName", GetVal(row, "CellName"));
                AppendField(html, "StationName", GetVal(row, "StationName"));

                // USE
                html.Append("<div class='section'>▸ USE (from GridView)</div>");
                AppendField(html, "HD_Model", GetVal(row, "HD_Model"));
                AppendField(html, "Use_ModelId", GetVal(row, "Use_ModelId"));
                AppendField(html, "Add_Model", GetVal(row, "Add_Model"));
                AppendField(html, "Use_LocationId", GetVal(row, "Use_LocationId"));
                AppendField(html, "HD_Cell", GetVal(row, "HD_Cell"));
                AppendField(html, "Use_CellId", GetVal(row, "Use_CellId"));
                AppendField(html, "Add_Cell", GetVal(row, "Add_Cell"));
                AppendField(html, "HD_Station", GetVal(row, "HD_Station"));
                AppendField(html, "Use_StationId", GetVal(row, "Use_StationId"));
                AppendField(html, "Add_Station", GetVal(row, "Add_Station"));

                // COMPARE
                html.Append("<div class='section'>▸ COMPARE</div>");
                AppendField(html, "CompareData", GetVal(row, "CompareData"));
                AppendField(html, "StatusId", GetVal(row, "StatusId"));

                html.Append("</div>"); // row-block
            }

            html.Append("</body></html>");

            // ── เปิด popup window ใหม่ผ่าน JavaScript ────────────────────────────
            string htmlEncoded = html.ToString()
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", "")
                .Replace("\n", "");

            string script =
                "var w = window.open('', '_blank', 'width=700,height=700,scrollbars=yes,resizable=yes');" +
                "w.document.open();" +
                "w.document.write('" + htmlEncoded + "');" +
                "w.document.close();";

            ScriptManager.RegisterStartupScript(
                this, this.GetType(),
                "debug_" + DateTime.UtcNow.Ticks,
                script, true
            );

            alert_toast(
                "เปิด Debug Popup แล้ว | " + dt.Rows.Count + " rows",
                "DEBUG", "info", "3000"
            );
        }

        // ── Debug เฉพาะ row ที่ CompareData ไม่ใช่ Same ─────────────────────────
        private void Debug_BorrowListDT_Changed()
        {
            DataTable dt = BorrowListDT;

            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("BorrowListDT : ไม่มีข้อมูล (0 rows)", "DEBUG", "warn", "5000");
                return;
            }

            // ── กรอง เฉพาะ row ที่ไม่ใช่ Same ───────────────────────────────────
            var changedRows = new System.Collections.Generic.List<DataRow>();
            foreach (DataRow r in dt.Rows)
            {
                string cmp = GetVal(r, "CompareData");
                if (cmp != "Same" && !string.IsNullOrEmpty(cmp))
                    changedRows.Add(r);
            }

            if (changedRows.Count == 0)
            {
                alert_toast("ทุก Row เป็น Same — ไม่มีการเปลี่ยนแปลง", "DEBUG", "success", "4000");
                return;
            }

            // ── ดึงข้อมูล DB มาเปรียบเทียบ field-level ───────────────────────────
            string docNo = litIssueNo.Text.Trim();
            DataTable dtDB = new DataTable();

            if (docNo != "-" && !string.IsNullOrEmpty(docNo))
            {
                string strDB =
                    "SELECT tool_id, sub_id, system_id," +
                    "       Use_Model_Id, New_Model, HD_Model," +
                    "       Use_Location_Id," +
                    "       Use_Cell_Id, New_Cell, HD_Cell," +
                    "       Use_Station_Id, New_Station, HD_Station" +
                    " FROM  Borrow_List" +
                    " WHERE Doc_No = N'" + docNo + "'";

                dc.QryDT(conn, strDB);
                dtDB = dc.dt.Copy();
            }

            // ── สร้าง HTML ────────────────────────────────────────────────────────
            System.Text.StringBuilder html = new System.Text.StringBuilder();

            html.Append("<html><head><meta charset='UTF-8'>");
            html.Append("<title>DEBUG : Changed Rows</title>");
            html.Append("<style>");
            html.Append("body { font-family:Consolas,monospace; font-size:12px; margin:10px; background:#1e1e1e; color:#d4d4d4; }");
            html.Append("h2 { color:#569cd6; border-bottom:1px solid #444; padding-bottom:6px; }");
            html.Append(".row-block { background:#252526; border:1px solid #3c3c3c; border-radius:6px; margin-bottom:12px; padding:10px; }");
            html.Append(".row-title { font-size:13px; font-weight:bold; color:#9cdcfe; margin-bottom:8px; }");
            html.Append(".section { color:#c586c0; font-weight:bold; margin:8px 0 4px 0; }");
            html.Append(".field { margin-left:16px; line-height:2; display:flex; gap:8px; align-items:center; }");
            html.Append(".key { color:#9cdcfe; min-width:160px; display:inline-block; }");
            html.Append(".val-new { color:#ce9178; }");
            html.Append(".val-old { color:#888; text-decoration:line-through; font-size:11px; }");
            html.Append(".val-empty { color:#555; font-style:italic; }");
            html.Append(".changed-row { background:#2d2d1a; border-color:#665c00; }");  // highlight แถวที่ update
            html.Append(".arrow { color:#ffc107; }");
            html.Append(".badge { display:inline-block; padding:2px 10px; border-radius:10px; font-size:11px; font-weight:bold; margin-left:8px; }");
            html.Append(".Same   { background:#1e4620; color:#4ec94e; }");
            html.Append(".Update { background:#4d3b00; color:#ffc107; }");
            html.Append(".Insert { background:#003153; color:#4fc1ff; }");
            html.Append(".Delete { background:#4d0000; color:#f44747; }");
            html.Append(".field-changed { background:#3d3000; border-radius:4px; padding:2px 6px; }");
            html.Append(".summary { background:#252526; border:1px solid #569cd6; border-radius:6px; padding:10px; margin-bottom:14px; line-height:2.2; }");
            html.Append(".btn-close { position:fixed; top:10px; right:10px; background:#c0392b; color:#fff; border:none; padding:6px 16px; border-radius:6px; cursor:pointer; font-size:13px; }");
            html.Append("</style></head><body>");

            html.Append("<button class='btn-close' onclick='window.close()'>✕ ปิด</button>");
            html.Append("<h2>🔄 DEBUG : Changed Rows &nbsp;<small style='color:#888;font-size:12px;'>Total Changed : "
                        + changedRows.Count + " rows</small></h2>");

            // ── Summary ───────────────────────────────────────────────────────────
            int cntUpdate = 0; int cntInsert = 0; int cntDelete = 0;
            foreach (DataRow r in changedRows)
            {
                switch (GetVal(r, "CompareData"))
                {
                    case "Update": cntUpdate++; break;
                    case "Insert": cntInsert++; break;
                    case "Delete": cntDelete++; break;
                }
            }

            html.Append("<div class='summary'>");
            html.Append("  <span class='badge Update'>Update : " + cntUpdate + "</span>&nbsp;");
            html.Append("  <span class='badge Insert'>Insert : " + cntInsert + "</span>&nbsp;");
            html.Append("  <span class='badge Delete'>Delete : " + cntDelete + "</span>");
            html.Append("</div>");

            // ── field mapping : BorrowListDT → DB ────────────────────────────────
            // key = ชื่อ field ใน BorrowListDT, value = ชื่อ column ใน dtDB
            var fieldMap = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Use_ModelId",    "Use_Model_Id"    },
                { "Add_Model",      "New_Model"        },
                { "HD_Model",       "HD_Model"         },
                { "Use_LocationId", "Use_Location_Id"  },
                { "Use_CellId",     "Use_Cell_Id"      },
                { "Add_Cell",       "New_Cell"         },
                { "HD_Cell",        "HD_Cell"          },
                { "Use_StationId",  "Use_Station_Id"   },
                { "Add_Station",    "New_Station"       },
                { "HD_Station",     "HD_Station"       }
            };

            // ── วน แต่ละ changed row ──────────────────────────────────────────────
            foreach (DataRow row in changedRows)
            {
                string cmp = GetVal(row, "CompareData");
                string toolId = GetVal(row, "ToolId");
                string toolSub = GetVal(row, "ToolSub");
                string systemId = GetVal(row, "SystemId");

                string blockClass = (cmp == "Update") ? "row-block changed-row" : "row-block";
                string badgeHtml = "<span class='badge " + cmp + "'>" + cmp + "</span>";

                html.Append("<div class='" + blockClass + "'>");
                html.Append("<div class='row-title'>");
                html.Append("Row No : " + GetVal(row, "RowNo") + " " + badgeHtml);
                html.Append(" &nbsp;|&nbsp; ");
                html.Append("Tool: <span style='color:#ce9178'>" + toolId + "-" + toolSub + "</span>");
                html.Append(" &nbsp;|&nbsp; ");
                html.Append("FixAsset: <span style='color:#ce9178'>" + GetVal(row, "FixAssetNo") + "</span>");
                html.Append(" &nbsp;|&nbsp; ");
                html.Append("ToolNo: <span style='color:#ce9178'>" + GetVal(row, "ToolNo") + "</span>");
                html.Append("</div>");

                // ── หา DB row ที่ตรงกัน ───────────────────────────────────────────
                DataRow dbRow = null;
                foreach (DataRow dr in dtDB.Rows)
                {
                    if (GetVal(dr, "tool_id") == toolId &&
                        GetVal(dr, "sub_id") == toolSub &&
                        GetVal(dr, "system_id") == systemId)
                    {
                        dbRow = dr;
                        break;
                    }
                }

                // ── INSERT : แสดงค่าใหม่อย่างเดียว ──────────────────────────────
                if (cmp == "Insert")
                {
                    html.Append("<div class='section'>▸ ค่าที่จะ Insert</div>");
                    foreach (var kv in fieldMap)
                    {
                        string newVal = GetVal(row, kv.Key);
                        AppendFieldChanged(html, kv.Key, newVal, null, false);
                    }
                }

                // ── DELETE : แสดงค่าใน DB ────────────────────────────────────────
                else if (cmp == "Delete")
                {
                    html.Append("<div class='section'>▸ ค่าที่จะ Delete (จาก DB)</div>");
                    if (dbRow != null)
                    {
                        foreach (var kv in fieldMap)
                        {
                            string dbVal = GetVal(dbRow, kv.Value);
                            AppendFieldChanged(html, kv.Key, dbVal, null, false);
                        }
                    }
                    else
                    {
                        html.Append("<div class='field'><span style='color:#f44747'>ไม่พบข้อมูลใน DB</span></div>");
                    }
                }

                // ── UPDATE : เปรียบเทียบ field ต่อ field ─────────────────────────
                else if (cmp == "Update")
                {
                    html.Append("<div class='section'>▸ Field ที่เปลี่ยนแปลง (เหลือง = เปลี่ยน)</div>");
                    foreach (var kv in fieldMap)
                    {
                        string newVal = GetVal(row, kv.Key);
                        string oldVal = (dbRow != null) ? GetVal(dbRow, kv.Value) : "";
                        bool isChg = (newVal != oldVal);

                        AppendFieldChanged(html, kv.Key, newVal, oldVal, isChg);
                    }
                }

                html.Append("</div>"); // row-block
            }

            html.Append("</body></html>");

            // ── เปิด popup window ─────────────────────────────────────────────────
            string htmlEncoded = html.ToString()
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", "")
                .Replace("\n", "");

            string script =
                "var w = window.open('', '_blank', 'width=800,height=750,scrollbars=yes,resizable=yes');" +
                "w.document.open();" +
                "w.document.write('" + htmlEncoded + "');" +
                "w.document.close();";

            ScriptManager.RegisterStartupScript(
                this, this.GetType(),
                "debugChanged_" + DateTime.UtcNow.Ticks,
                script, true
            );

            alert_toast(
                "Changed : " + changedRows.Count + " rows | Update:" + cntUpdate +
                " Insert:" + cntInsert + " Delete:" + cntDelete,
                "DEBUG", "warn", "4000"
            );
        }       

    

    }
}
