// ═══════════════════════════════════════════════════════════════════════════
//  NotificationOfChange.aspx.cs
//  แจ้งเปลี่ยนแปลง Fixed Asset / Calibration
// ───────────────────────────────────────────────────────────────────────────
//  โครงสร้างการทำงานหลัก:
//    • Page_Load                 : เข้าหน้าตาม mode (new / edit / view / confirm / approve)
//    • สร้าง/แก้เอกสาร            : Search → Add → เลือกโหมด change/swap → Draft/Send
//    • Send                      : เลือก J3 Approver (modal) → บันทึก
//    • Approve (sequential)      : status 30 (A1=Current) → 31 (A2=Use) → 50
//                                  A2 เลือกต่าง A1 → กลับ 30 ให้ทบทวนใหม่
//
//  กฎแผนก Approve (ResolveDeptApprove):
//    • เทียบกลุ่มด้วย 3 หลักแรก (IsSameDeptGroup) — ยกเว้น 5505 เทียบเต็ม 4 หลัก
//    • Div=2 → ใช้ dept นั้น / ไม่มี Div=2 → ใช้ 5505
//    • 2 dept (Div=2 ต่างกลุ่ม) → A1=Current, A2=Use
//
//  ตารางหลักที่เกี่ยวข้อง:
//    • NotificationofChange_Control_List  (header: status_id, dept_code_approve/2, change_type)
//    • NotificationofChange_List          (รายการ: Use_*, reject_flag, a1/a2_selected)
//    • NotificationofChange_Approver      (ผู้ approve: emp_id, dept_code, approve_flag)
// ═══════════════════════════════════════════════════════════════════════════

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Windows.Interop;
//using System.Text.RegularExpressions;


namespace Tool_Equipment_Control_System
{
    public partial class NotificationOfChange : System.Web.UI.Page
    {
        SqlConnection conn_user = new SqlConnection(ConfigurationManager.ConnectionStrings["user"].ConnectionString);
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        SqlConnection conn_jig = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        datacon dc = new datacon();

        private string _lastSummaryStep = "";
        // field จำ step แถวก่อนหน้า
        private string _lastApvFlowStep = "";

        private const int MAX_LOC_TEXT = 50;                          // Model / Cell / Station แบบพิมพ์
        private DataTable _cellM, _stationM;                           // cache master ต่อการ Bind 1 ครั้ง
        private HashSet<string> _hlTyped = new HashSet<string>();      // ช่องที่ถูกเด้งกลับเป็น Dropdown

        private const int MAX_REASON_REMARK = 1000;   // ต้องไม่เกินขนาดคอลัมน์ reason_remark

        private string _apvDeptLabelL = "", _apvDeptLabelR = "";   // แผนกผู้อนุมัติ (ตาราง View / Approve / Confirm)

        private DataTable _statusUseM, _respM;   // master Status Use / ผู้รับผิดชอบ (โหลดครั้งเดียวต่อ Request)

        private const string STATUS_USE = "1";   // tool_equipment_status : Use
        private const string STATUS_SPARE = "2";   // tool_equipment_status : Spare

        // ══════════════════════════════════════════════
        //  E-Mail CONFIG (อ้างอิงจาก AutoBot)
        // ══════════════════════════════════════════════
        private const string SMTP_HOST = "150.61.249.70";
        private const int SMTP_PORT = 25;
        private const string MAIL_FROM = "engineer_pde@mail.canon";
        private const string MAIL_CC_ALWAYS = "anirut@mail.canon";

        private const string APPROVE_LINK = "http://cptsvs552.canon-pt.co.th/Tool_Equipment_Control_System/ApproveStatus.aspx";
        private const string REQUEST_LINK = "http://cptsvs552.canon-pt.co.th/Tool_Equipment_Control_System/EquipmentStatus2.aspx";

        private const string HRMS_URL = "http://cptsvs547:1000/middleware/oracle/hrms";

        // ── โหมด ──────────────────────────────────
        //  true  = Debug : ส่ง Hard Code ไป anirut@mail.canon + แสดงรายชื่อ Approver ในเนื้อหา
        //  false = Production : ส่งเข้า To จริง (Approver), CC = anirut@mail.canon, ไม่โชว์รายชื่อในเนื้อหา
        //  *** ก่อนขึ้น Production เปลี่ยนเป็น false ***
        private const bool MAIL_DEBUG = true;
        private const string DEBUG_MAIL_TO = "anirut@mail.canon";

        protected DataTable ChangeListDT
        {
            get
            {
                if (ViewState["ChangeListDT"] == null)
                {
                    ViewState["ChangeListDT"] = CreateEmptyChangeTable();
                }
                return (DataTable)ViewState["ChangeListDT"];
            }
            set
            {
                ViewState["ChangeListDT"] = value;
            }
        }

        [Serializable]  
        public class DeptApproveResult
        {
            public string A1 { get; set; }   // dept_code_approve  (Current side)
            public string A2 { get; set; }   // dept_code_approve2 (Use side), null ถ้า 1 dept
            public bool IsTwoDept { get { return !string.IsNullOrEmpty(A2); } }
        }

        /// <summary>จุดเริ่มต้นของหน้า — โหลดข้อมูลตาม mode (new/edit/view/approve) และ query string</summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = "แจ้งเปลี่ยนแปลงอุปกรณ์";

            string Doc_NO = null;
            string mode = "edit"; // default
            //ViewState["MODE"] = mode;

            if (Session["emp_id"] == null || Session["emp_id"].ToString() == "")
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                ViewState["FORM_ID"] = Guid.NewGuid().ToString("N");   // ✅ 6F-5 : Done-Guard
                ViewState["MODE"] = PageMode;

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
                    BindChangeList();
                }

                ViewState["MODE"] = mode;
                ViewState["DEPT_MASTER"] = GetDeptMaster();
                ViewState["MODEL_MASTER"] = GetModelMaster();
                GetReasonMaster();

                if (Doc_NO == "0")
                {
                    Requester_getdata();

                    // Admin เข้าหน้าสร้างใหม่ → เลือก "แจ้งเปลี่ยนเครื่องมือวัด" ให้อัตโนมัติ
                    string startMode = IsAdmin ? MODE_MEASURE : "change";
                    hfChangeMode.Value = startMode;
                    ViewState["CHANGE_MODE"] = startMode;
                    UpdateModeButtonUI(startMode);

                    // กำหนดค่าใน TextBox "วันที่ยืม" เป็นวันที่ปัจจุบัน
                    txtIssueDate.Text = DateTime.Today.ToString("dd-MMM-yyyy");
                    //txtIssueDate.Text = DateTime.Today.ToString();
                }
                else
                {
                    // ✅ ป้องกันเข้าหน้าแบบไม่ถูกต้อง
                    if (string.IsNullOrEmpty(Doc_NO))
                    {
                        alert_toast("ไม่พบเลขเอกสาร", "Error", "error", "10000");
                        return;
                    }

                    litIssueNo.Text = Doc_NO;
                    string status_id = status_show(Doc_NO);
                    ViewState["STATUS_ID"] = status_id;

                    LoadRequester(Doc_NO);
                    LoadChangeMode(Doc_NO);
                    LoadChangeList(Doc_NO);
                    UpdateUseDeptRemark();
                    LoadApproverFlow(Doc_NO);
                    Bind_History_Reject(Doc_NO);

                    if (mode == "view")
                    {
                        SetViewMode(status_id);
                    }
                    else if (mode == "confirm")
                    {
                        SetConfirmMode(status_id);
                    }
                    else if (mode == "approve")
                    {
                        SetApproveMode(status_id);
                    }
                    else
                    {
                        if (CancelButton != null) CancelButton.Visible = true;
                        if (status_id == "1") litRejectBanner.Text = BuildLastRejectBanner(Doc_NO);
                        //pnl_rejectComment.Visible = true;
                        //Show Comment History เฉพาะเมื่อมีประวัติ Reject/Cancel
                        bool hasReject = gv_rejectHistory.Rows.Count > 0;
                        pnl_rejectComment.Visible = hasReject;
                    }
                }


                //GetDataUser_check();
            }

            txt_ChangeComment.Attributes["maxlength"] = MAX_REASON_REMARK.ToString();
        }

        // ── โหมดรายการ (change_type) ──────────────────
        //  change  = แจ้งเปลี่ยน Location
        //  swap    = แจ้งสลับ Tool / Equipment (จับคู่)
        //  gauge   = แจ้งเปลี่ยนเครื่องมือวัด (Admin เท่านั้น) — Flow/รูปแบบเดียวกับ swap
        //            (ใช้ค่าสั้น ≤ 6 ตัวอักษร ให้พอดีคอลัมน์ change_type เดิมที่เก็บ "change")
        private const string MODE_MEASURE = "gauge";

        private bool IsAdmin
        {
            get { return Session["admin"] != null && Session["admin"].ToString() == "99"; }
        }
        // โหมดที่ทำงานแบบจับคู่ (swap / measure)
        private static bool IsPairMode(string mode) { return mode == "swap" || mode == MODE_MEASURE; }
        private bool IsPairMode() { return IsPairMode(hfChangeMode.Value); }
        private bool IsMeasureMode() { return hfChangeMode.Value == MODE_MEASURE; }
        private string PairModeLabel() { return IsMeasureMode() ? "แจ้งเปลี่ยนเครื่องมือวัด" : "แจ้งสลับ"; }

        private string PageMode
        {
            get
            {
                string m = Request.QueryString["mode"];
                return string.IsNullOrEmpty(m) ? "edit" : m.ToLower();
            }
        }
        private string CurrentMode
        {
            get
            {
                string m = ViewState["MODE"] as string;
                return string.IsNullOrEmpty(m) ? PageMode : m;
            }
        }

        #region --- Requester Data ---

        // --- Requester_getdata() — เรียกตอน Page_Load (!IsPostBack)
        protected void Requester_getdata()
        {
            string emp_id = Session["emp_id"].ToString();
            string admin = Session["admin"].ToString();

            // ---------- Admin : ลงข้อมูลผู้ร้องขอให้อัตโนมัติจาก User ที่ Login (ไม่ต้องกรอกรหัสพนักงาน) ----------
            if (admin == "99")
            {
                ViewState["OGN_DEPTCODE"] = "";
                ViewState["OGN_DEPTNAME"] = "";
                txtEmpId.Text = emp_id;

                if (GetDataHRMS_CSharp(emp_id))
                {
                    SaveEmployeeToViewState();
                    txtEmpId.ReadOnly = true;
                    pnlNewEmp.Visible = false;
                    return;
                }

                // fallback : Login เป็นรหัสที่ไม่ใช่พนักงาน (ไม่พบใน HRMS) → ให้กรอกเองแบบเดิม
                txtEmpId.Text = "";
                ClearEmployeeUI();
                pnlNewEmp.Visible = true;
                lblempNotice.Text = "กรุณากรอกรหัสพนักงาน [Employee ID]";
                txtEmpId.ReadOnly = false;
                return;
            }

            // ---------- User ปกติ ----------
            // เก็บ dept ของ login-user ไว้เทียบทีหลัง
            string str_dept = "SELECT * FROM View_dept_master WHERE dept_code = '" + emp_id + "'";
            dc.QryDT(conn, str_dept);

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

            // ใส่ emp_id ลง textbox แล้วดึง HRMS
            //txtEmpId.Text = emp_id;

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

        // --- Requester_getdata_edit() — ตรวจ dept (User ปกติเท่านั้น)
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

        #endregion

        #region --- Load Data : Change Control List ---

        // เรียกข้อมูลถ้ามีข้อมูลในระบบแล้ว Change Control List
        protected string status_show(string Doc_No)
        {
            string str_status =
                " SELECT A.status_id, B.status_name, B.Style_CSS" +
                " FROM NotificationofChange_Control_List A" +
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
        /// <summary>โหลดข้อมูล Requester ของเอกสารจาก DB</summary>
        private void LoadRequester(string Doc_No)
        {
            string str_emp = "";
            str_emp = "select * from NotificationofChange_Control_List where Doc_No = N'" + Doc_No + "'";

            dc.QryDT(conn, str_emp);

            string emp_id = dc.dt.Rows[0]["issue_empid"].ToString();
            DateTime issue_date = Convert.ToDateTime(dc.dt.Rows[0]["issue_date"]);
            string reason_id = dc.dt.Rows[0]["reason_id"].ToString();
            string comment_text = dc.dt.Rows[0]["reason_remark"].ToString();
            string phone = dc.dt.Rows[0]["phone"] != DBNull.Value
                   ? dc.dt.Rows[0]["phone"].ToString()
                   : "";

            GetDataHRMS_CSharp(emp_id);
            //ShowEmployeeUI();
            txtEmpId.Text = emp_id;
            txtIssueDate.Text = issue_date.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            txtEmpId.ReadOnly = true;
            txtTel.Text = phone;

            SaveEmployeeToViewState();

            // ✅ R32 : reason_id = 0 หรือเหตุผลถูกปิดใช้งาน → ไม่ Error ให้กลับเป็น -- Select --
            if (ddlReason_Change.Items.FindByValue(reason_id) != null)
                ddlReason_Change.SelectedValue = reason_id;
            else
                ddlReason_Change.SelectedIndex = 0;

            txt_ChangeComment.Text = comment_text;

        }
        /// <summary>โหลดโหมด change/swap ของเอกสารจาก DB มาตั้งค่าปุ่ม</summary>
        private void LoadChangeMode(string Doc_No)
        {
            string str = "select change_type from NotificationofChange_Control_List where Doc_No = @Doc_No";
            dc.QryDT_P(conn, str,
                new SqlParameter("@Doc_No", SqlDbType.NVarChar) { Value = Doc_No });

            string mode = "change";
            if (dc.dt.Rows.Count > 0 && dc.dt.Rows[0]["change_type"] != DBNull.Value)
            {
                string v = dc.dt.Rows[0]["change_type"].ToString().Trim();
                if (v == "swap") mode = "swap";
                else if (v == MODE_MEASURE) mode = MODE_MEASURE;
            }

            hfChangeMode.Value = mode;
            ViewState["CHANGE_MODE"] = mode;
            UpdateModeButtonUI(mode);
        }
        /// <summary>โหลดรายการเปลี่ยนแปลง (NotificationofChange_List) เข้า ChangeListDT</summary>
        private void LoadChangeList(string Doc_No)
        {
            ViewState["TWO_DEPT"] = HasSecondDept(Doc_No);
            DataTable dt = ChangeListDT;

            string str_Changelist = " Select A.No , A.Doc_No , A.tool_id , A.sub_id , A.system_id , A.FixAsset , A.ControlNo , A.Use_Dept " +
                                    " , A.Use_Model_Id , A.New_Model ,A.HD_Model " +
                                    " , A.Use_Cell_Id , A.New_Cell , A.HD_Cell" +
                                    " , A.Use_Station_Id , A.New_Station , A.HD_Station" +
                                    " , A.reject_flag , A.a1_selected , A.a2_selected , A.Use_Status_Id , A.Resp_EmpId , A.Resp_Name" +
                                    " ,B.tool_no , B.dept_code, B.dept_code_real , B.model_name , B.model_id" +
                                    " ,B.location1_id , B.location , B.location2_id , B.cell , B.station_id , B.station , A.status_id" +
                                    " from NotificationofChange_List A" +
                                    " left join Tool_Equipment_List_View B on A.tool_id = B.tool_id and A.sub_id = B.sub_no and A.system_id = B.sys_status" +
                                    " Where Doc_No = N'" + Doc_No + "'" +
                                    " Order by A.No ";

            dc.QryDT(conn, str_Changelist);
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
                newRow["DeptCodeReal"] = r["dept_code_real"];
                newRow["ModelName"] = r["model_name"];
                newRow["ModelId"] = r["model_id"];
                newRow["CellName"] = r["cell"];
                newRow["CellId"] = r["location2_id"];
                newRow["StationName"] = r["station"];
                newRow["StationId"] = r["station_id"];

                // ✅ Approve tracking
                newRow["RejectFlag"] = r["reject_flag"] == DBNull.Value ? "" : r["reject_flag"];
                newRow["A1Selected"] = r["a1_selected"] == DBNull.Value ? "" : r["a1_selected"];
                newRow["A2Selected"] = r["a2_selected"] == DBNull.Value ? "" : r["a2_selected"];

                // ✅ AAD Confirm
                newRow["Use_StatusId"] = r["Use_Status_Id"] == DBNull.Value ? "" : r["Use_Status_Id"].ToString();
                newRow["Resp_EmpId"] = r["Resp_EmpId"] == DBNull.Value ? "" : r["Resp_EmpId"].ToString();
                newRow["Resp_Name"] = r["Resp_Name"] == DBNull.Value ? "" : r["Resp_Name"].ToString();

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

                // ค่าใน DB ถือว่าผ่านการสลับ/แก้ไขแล้ว
                newRow["SwapApplied"] = "1";   

                dt.Rows.Add(newRow);
            }

            // ✅ swap : กำหนด PairKey ตามลำดับ (คู่ละ 2) — change mode ไม่ใช้ ไม่มีผล
            int pkSeq = 0;
            foreach (DataRow r in dt.Rows)
            {
                r["PairKey"] = (pkSeq / 2) + 1;
                pkSeq++;
            }

            ChangeListDT = dt;
            BindChangeList();
        }

        // ── โหลด Approver ที่เคยบันทึก กลับเข้า Step 6 ──
        private void LoadApproverFlow(string Doc_No)
        {
            // อ่าน dept approve จาก header
            dc.QryDT_P(conn,
                "SELECT dept_code_approve, dept_code_approve2 FROM NotificationofChange_Control_List WHERE Doc_No=@d",
                new SqlParameter("@d", SqlDbType.NVarChar) { Value = Doc_No });

            if (dc.dt.Rows.Count == 0) return;

            string a1 = GetVal(dc.dt.Rows[0], "dept_code_approve");
            string a2 = GetVal(dc.dt.Rows[0], "dept_code_approve2");

            // ยังไม่เคย generate/ส่ง → ไม่มี dept approve → คง needGen
            if (string.IsNullOrEmpty(a1)) return;

            var deptList = new System.Collections.Generic.List<string>();
            deptList.Add(a1);
            if (!string.IsNullOrEmpty(a2)) deptList.Add(a2);
            ViewState["DEPT_APPROVE_LIST"] = deptList;

            hfFlowDeptA1.Value = a1;
            hfFlowDeptA2.Value = a2 ?? "";
            hfFlowGenerated.Value = "1";
            pnlFlowNeedGen.Visible = false;
            pnlFlowContent.Visible = true;

            // ✅ 1) bind source ก่อน (มันจะล้าง selected)
            BindJ3Approver(deptList, true);   // ไม่ init ตอนนี้

            // ✅ 2) อ่าน emp ที่เลือกไว้ แล้ว set selected "หลัง" bind
            dc.QryDT_P(conn,
                "SELECT emp_id, status_id FROM NotificationofChange_Approver WHERE Doc_No=@d",
                new SqlParameter("@d", SqlDbType.NVarChar) { Value = Doc_No });

            var selL = new System.Collections.Generic.List<string>();
            var selR = new System.Collections.Generic.List<string>();
            foreach (DataRow r in dc.dt.Rows)
            {
                string id = GetVal(r, "emp_id");
                string st = GetVal(r, "status_id");
                if (st == "31") selR.Add(id); else selL.Add(id);
            }
            hfJ3SelectedL.Value = string.Join(",", selL.ToArray());
            hfJ3SelectedR.Value = string.Join(",", selR.ToArray());

            // bind source + chips (BindJ3Approver จะ set source/toggle/j3Init)
            //BindJ3Approver(deptList);

            // ✅ 3) เรียก j3Init อีกครั้ง ให้ render chips จาก selected ที่เพิ่ง set
            ScriptManager.RegisterStartupScript(this, GetType(), "j3ReinitLoad",
                "if(typeof j3Init==='function'){ j3Init(); }", true);
        }

        // ── โชว์/ซ่อน remark ตาม Div ของ Current Dept ──
        private void UpdateUseDeptRemark()
        {
            if (hfChangeMode.Value != "change") { usedept_remark.Visible = false; return; }

            string currentDept = GetFirstCurrentDept();
            if (string.IsNullOrEmpty(currentDept)) { usedept_remark.Visible = false; return; }

            string division = GetDeptDivision(currentDept);
            usedept_remark.Visible = (division != "2" && division != "");
        }
        /// <summary>โหลดประวัติการ Reject มาแสดง</summary>
        protected void Bind_History_Reject(string Doc_No)
        {
            string str = " select a.Reject_Count, a.emp_id + ' ' + B.fname as Reject_by , A.Doc_no , A.Status_id , C.status_name , A.Reject_Date , A.Reject_detail , A.Type" +
                         " from Reject_Detail_All A" +
                         " left join user_master.dbo.sys_user_master B on A.emp_id = B.emp_id" +
                         " left join Master_Status C on A.status_id = C.status_id" +
                         " Where Doc_No = '" + Doc_No + "'" +
                         " Order by Reject_Count desc";
            dc.QryDT(conn, str);
            gv_rejectHistory.DataSource = dc.dt;
            gv_rejectHistory.DataBind();
        }

        #endregion

        #region ===== Button Click =====
        protected void btn_pic_Click(object sender, EventArgs e)
        {
            GridViewRow row = (sender as LinkButton).NamingContainer as GridViewRow;
            HiddenField hdtool_id = (HiddenField)row.FindControl("hdtoolId");
            HiddenField hdtool_sub = (HiddenField)row.FindControl("hdtoolSub");
            HiddenField hd_sys_status = (HiddenField)row.FindControl("hdsystemId");

            if (hdtool_id == null || hdtool_sub == null || hd_sys_status == null)
            {
                alert_toast("ข้อมูลในตารางผิดพลาด", "ไม่สามารถเปิดรูปได้", "error", "10000");
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
        /// <summary>ยืนยัน Approve จาก modal — ตรวจสิทธิ์ อ่าน checkbox แล้วแยกไป Approve 1/2</summary>
        protected void btnApproveConfirm_Click(object sender, EventArgs e)
        {
            string ad; if (AlreadyDone("APPROVE", out ad)) { ShowAlreadyDone(ad); return; }
            string Doc_No = litIssueNo.Text.Trim();
            string empId = Session["emp_id"] != null ? Session["emp_id"].ToString() : "";
            int step = ViewState["APV_STEP"] != null ? Convert.ToInt32(ViewState["APV_STEP"]) : 0;

            // ── เช็คสิทธิ์อีกรอบ (กัน tamper) ──
            int rStep, rStatus; string rFlag;
            if (!CheckApproverRight(Doc_No, empId, out rStep, out rStatus, out rFlag) || rStep != step)
            {
                alert_toast("สิทธิ์ไม่ถูกต้อง หรือสถานะเปลี่ยนไปแล้ว กรุณารีเฟรช", "ไม่มีสิทธิ์", "warn", "10000");
                return;
            }

            // ── ดึงค่า Model/Cell/Station ที่แก้ (ส่วน 2) ──
            //SyncGridViewToChangeListDT();

            // ── อ่าน checkbox : RowKey ที่เลือก ──
            var selectedKeys = new System.Collections.Generic.HashSet<string>();
            foreach (GridViewRow gr in gvApprove.Rows)
            {
                if (gr.RowType != DataControlRowType.DataRow) continue;
                CheckBox cb = gr.FindControl("cbApv") as CheckBox;
                HiddenField hfKey = gr.FindControl("hfRowKey") as HiddenField;
                if (cb != null && hfKey != null && cb.Checked)
                    selectedKeys.Add(hfKey.Value);
            }

            string comment = txtApvComment.Text.Trim();

            // ── นับ active + unselected ──
            int activeCount = 0, unselCount = 0;
            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                activeCount++;
                string key = GetVal(r, "ToolId") + "|" + GetVal(r, "ToolSub") + "|" + GetVal(r, "SystemId");
                if (!selectedKeys.Contains(key)) unselCount++;
            }

            bool a2Mismatch = (step == 2) && IsA2Mismatch(selectedKeys);

            // ✅ ข้อ 5 : โหมดสลับต้องเลือกเป็นคู่
            if (IsPairMode() && !ValidateApprovePairs(selectedKeys))
            {
                ReopenApproveModal();
                return;
            }

            if (a2Mismatch)
            {
                // ✅ ข้อ 4 : A2 เลือกไม่ตรง A1 → Reject → ต้องมีเหตุผล ≥10
                if (comment.Length < 10)
                {
                    alert_toast("กรุณาระบุเหตุผลการ Reject อย่างน้อย 10 ตัวอักษร", "ต้องระบุเหตุผล", "warn", "10000");
                    ReopenApproveModal();
                    return;
                }
            }
            else
            {
                if (selectedKeys.Count == 0)
                {
                    alert_toast("กรุณาเลือกอย่างน้อย 1 รายการ", "ยังไม่เลือก", "warn", "10000");
                    ReopenApproveModal();
                    return;
                }
                // ✅ 7E-2 : บังคับเหตุผลเฉพาะ A1 ที่ไม่อนุมัติบางรายการ
                //          A2 ที่เลือกตรงกับ A1 ไม่ต้องใส่ (A1 ใส่เหตุผลไว้แล้ว)
                if (step == 1 && unselCount > 0 && string.IsNullOrEmpty(comment))
                {
                    alert_toast("กรุณาระบุเหตุผลกรณีไม่อนุมัติบางรายการ", "ต้องระบุเหตุผล", "warn", "10000");
                    ReopenApproveModal();
                    return;
                }
            }

            // ══════════════════════════════════════════
            if (step == 1) DoApprove1(Doc_No, empId, selectedKeys, comment);
            else if (step == 2) DoApprove2(Doc_No, empId, selectedKeys, comment);
        }
        /// <summary>Approve ขั้นที่ 1 (status 30) — a1_selected + reject/ปลด reject → 31 หรือ 50</summary>
        private void DoApprove1(string Doc_No, string empId, HashSet<string> selectedKeys, string comment)
        {
            bool twoDept = HasSecondDept(Doc_No);
            int nextStatus = twoDept ? 31 : 50;
            int unselCount = 0;

            conn.Open();
            SqlTransaction tx = conn.BeginTransaction();
            try
            {
                foreach (DataRow r in ChangeListDT.Rows)
                {
                    if (GetVal(r, "CompareData") == "Delete") continue;
                    bool sel = selectedKeys.Contains(RowKey(r));
                    if (!sel) unselCount++;

                    // ✅ เลือก → ปลด reject (กรณีรอบ 2 ที่ A2 เคย reject) / ไม่เลือก → reject
                    // ✅ ล้าง a2_selected ทุกครั้ง ให้ A2 เลือกใหม่ในรอบนี้
                    string sql =
                        "UPDATE NotificationofChange_List SET a1_selected=@a1, a2_selected=NULL, " +
                        (sel ? "reject_flag=NULL, status_id=@st" : "reject_flag='reject', status_id=999") +
                        " WHERE Doc_No=@doc AND tool_id=@tid AND sub_id=@sid AND system_id=@sys";

                    using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@a1", sel ? "Y" : "N");
                        cmd.Parameters.AddWithValue("@st", nextStatus);
                        AddKeyParams(cmd, Doc_No, r);
                        cmd.ExecuteNonQuery();
                    }
                }

                SetApproverFlag(tx, Doc_No, empId, 1);
                if (twoDept) ResetApprover(tx, Doc_No, 2);          // A2 เริ่มรอบใหม่เสมอ
                UpdateHeaderStatus(tx, Doc_No, nextStatus, empId, 1);

                if (unselCount > 0 && !string.IsNullOrEmpty(comment))
                    InsertApproveHistory(tx, Doc_No, "30", "[Approver 1] " + comment, "Reject");

                tx.Commit();
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                alert_toast("Approve 1 ผิดพลาด : " + ex.Message, "Error", "error", "10000");
                ReopenApproveModal();
                return;
            }
            finally { if (conn.State == ConnectionState.Open) conn.Close(); }

            MarkDone("APPROVE", Doc_No);

            // ── E-Mail (หลัง commit) — best-effort ──
            bool partial = unselCount > 0;

            if (twoDept)
            {
                // ส่งต่อ A2 + CC Requester
                try
                {
                    var cc = new List<string>();
                    if (!partial) AddDistinctMail(cc, GetRequesterEmail());
                    string extra = partial
                        ? BuildReasonHtml(comment) + BuildItemMailTable(GetItemResultTable(Doc_No), true)
                        : null;
                    SendApproverMail(Doc_No, "31", null, null, null, extra, cc);
                }
                catch (Exception exMail) { System.Diagnostics.Debug.WriteLine("[Mail A2] " + exMail.Message); }

                // A1 ไม่ครบ → แจ้ง Requester (CC Admin Level 1)
                if (partial)
                {
                    try { SendResultMail(Doc_No, MailKind.Partial, 1, comment); }
                    catch (Exception exMail) { System.Diagnostics.Debug.WriteLine("[Mail Partial A1] " + exMail.Message); }
                }
            }
            else
            {
                // ✅ แผนกเดียว → status 50 : To AAD Team , CC Requester
                try { SendAadConfirmRequestMail(Doc_No, 1, partial ? comment : null); }
                catch (Exception exMail) { System.Diagnostics.Debug.WriteLine("[Mail AAD A1] " + exMail.Message); }
            }

            // ✅ 7D : Toast + กลับหน้า Approve Status (ตกหล่นจากรอบแก้อีเมล)
            string msg = twoDept
                ? (partial ? "อนุมัติบางรายการ — ส่งต่อ Approver 2 แล้ว" : "อนุมัติแล้ว — ส่งต่อ Approver 2 แล้ว")
                : (partial ? "อนุมัติบางรายการ — รอ AAD Confirm" : "อนุมัติสมบูรณ์ — รอ AAD Confirm");

            RedirectAfterApprove(Doc_No, msg, partial ? "warn" : "success");
        }

        /// <summary>Approve ขั้นที่ 2 (status 31) — เทียบ a1/a2 ทุกแถว : ตรง→50 / ต่าง→กลับ 30</summary>
        private void DoApprove2(string Doc_No, string empId, HashSet<string> selectedKeys, string comment)
        {
            int unselCount = 0;
            List<ApvDiff> diffs = null;
            bool returned = false;

            conn.Open();
            SqlTransaction tx = conn.BeginTransaction();
            try
            {
                // ── 1) บันทึก a2_selected ──
                foreach (DataRow r in ChangeListDT.Rows)
                {
                    if (GetVal(r, "CompareData") == "Delete") continue;
                    bool sel = selectedKeys.Contains(RowKey(r));
                    if (!sel) unselCount++;

                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE NotificationofChange_List SET a2_selected=@a2" +
                        " WHERE Doc_No=@doc AND tool_id=@tid AND sub_id=@sid AND system_id=@sys", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@a2", sel ? "Y" : "N");
                        AddKeyParams(cmd, Doc_No, r);
                        cmd.ExecuteNonQuery();
                    }
                }

                // ── 2) เทียบทุกแถว (รวมแถวที่ A1 reject) ──
                diffs = GetA1A2Diff(tx, Doc_No);

                if (diffs.Count == 0)
                {
                    // ✅ ตรงกัน → 50
                    SetApproverFlag(tx, Doc_No, empId, 2);
                    UpdateHeaderStatus(tx, Doc_No, 50, empId, 2);

                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE NotificationofChange_List SET status_id=50" +
                        " WHERE Doc_No=@doc AND ISNULL(reject_flag,'') <> 'reject'", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@doc", Doc_No);
                        cmd.ExecuteNonQuery();
                    }

                    if (!string.IsNullOrEmpty(comment))
                        InsertApproveHistory(tx, Doc_No, "31", "[Approver 2] " + comment, "Reject");
                }
                else
                {
                    // ✅ 4.1 : ไม่ตรง A1 → Reject กลับ Requester (status 1) แบบเดียวกับ DoReject
                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE NotificationofChange_List" +
                        " SET status_id=1, reject_flag=NULL, a1_selected=NULL, a2_selected=NULL" +
                        " WHERE Doc_No=@doc", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@doc", Doc_No);
                        cmd.ExecuteNonQuery();
                    }

                    ResetApprover(tx, Doc_No, 1);
                    ResetApprover(tx, Doc_No, 2);
                    UpdateHeaderStatus(tx, Doc_No, 1, empId, 0);
                    InsertApproveHistory(tx, Doc_No, "31", BuildReturnDetail(diffs, comment), "Reject");
                    returned = true;
                }

                tx.Commit();
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                alert_toast("Approve 2 ผิดพลาด : " + ex.Message, "Error", "error", "10000");
                ReopenApproveModal();
                return;
            }
            finally { if (conn.State == ConnectionState.Open) conn.Close(); }

            MarkDone("APPROVE", Doc_No);

            if (!returned)
            {
                // ✅ ครบ 2 แผนก → status 50 : To AAD Team , CC Requester
                try { SendAadConfirmRequestMail(Doc_No, 2, unselCount > 0 ? comment : null); }
                catch (Exception exMail) { System.Diagnostics.Debug.WriteLine("[Mail AAD A2] " + exMail.Message); }

                RedirectAfterApprove(Doc_No, "อนุมัติสมบูรณ์ (ผ่านทั้ง 2 แผนก) — ส่งต่อ AAD แล้ว");
                return;
            }

            // ── 4.1 / 4.3 : เมล Reject → To Requester , CC A1+A2 พร้อมรายการที่ไม่ตรง ──
            try { SendResultMail(Doc_No, MailKind.RejectMismatch, 2, comment, BuildDiffMailHtml(diffs)); }
            catch (Exception exMail) { System.Diagnostics.Debug.WriteLine("[Mail Reject A2] " + exMail.Message); }

            RedirectAfterApprove(Doc_No, "เลือกรายการไม่ตรงกับ Approver 1 — Reject กลับไปยังผู้ร้องขอแล้ว", "warn");
        }
        /// <summary>แสดงรายละเอียดของรายการที่เลือก</summary>
        protected void btn_detailView_Click(object sender, EventArgs e)
        {
            GridViewRow row = (sender as LinkButton).NamingContainer as GridViewRow;
            HiddenField hdtool_id = (HiddenField)row.FindControl("hdtoolId");
            HiddenField hdtool_sub = (HiddenField)row.FindControl("hdtoolSub");
            HiddenField hd_sys_status = (HiddenField)row.FindControl("hdsystemId");

            if (hdtool_id == null || hdtool_sub == null || hd_sys_status == null)
            {
                alert_toast("ข้อมูลในตารางผิดพลาด", "แจ้งตือน", "error", "10000");
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
        /// <summary>ค้นหา Tool/Equipment เพื่อเพิ่มในรายการ</summary>
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string dept_check = (txtFixAsset.Text ?? "").Trim();
            txtFixAsset.Text = dept_check;

            if (dept_check.Length == 0)
            {
                alert_toast("กรุณากรอกข้อมูลที่ต้องการค้นหา", "แจ้งเตือน", "warn", "10000");
                return;
            }

            // (ถ้าต้องการกันค้นคำสั้นเกินไป — R21) เปิดบรรทัดนี้
            if (dept_check.Length < 4) { alert_toast("กรุณากรอกอย่างน้อย 4 ตัวอักษร", "แจ้งเตือน", "warn", "10000"); return; }

            // keyword เป็น static string ทั้งหมด — ค่าจริงไปทาง parameter
            string keyword =
                "FixAcces   LIKE '%' + @kw + '%'" +
                " OR control_no LIKE '%' + @kw + '%'" +
                " OR tool_no    LIKE '%' + @kw + '%'";

            string str =
                "SELECT * FROM Tool_Equipment_List_View" +
                " WHERE (" + keyword + ")" +
                " ORDER BY tool_no";

            var prms = new System.Collections.Generic.List<SqlParameter>
            {
                new SqlParameter("@kw", SqlDbType.NVarChar) { Value = dept_check }
            };
            //if (dept_check.Length == 4)
            //    prms.Add(new SqlParameter("@dept", SqlDbType.NVarChar) { Value = dept_check });

            dc.QryDT_P(conn, str, prms.ToArray());
            DataTable dt = dc.dt;

            if (!dt.Columns.Contains("RowNo"))
            {
                dt.Columns.Add("RowNo", typeof(int));
                dt.Columns.Add("DisplayStatus", typeof(string));
                dt.Columns.Add("ProcUrl", typeof(string));   // ✅ url เอกสาร process
            }

            // ── ดึงรายการที่กำลังยืมอยู่ (status ≠ 1, 100, 999) ──
            //var borrowingSet = new System.Collections.Generic.HashSet<string>(
            //    StringComparer.OrdinalIgnoreCase);

            string strBorrow =
                "SELECT DISTINCT tool_id, sub_id, system_id" +
                " FROM Borrow_List" +
                " WHERE status_id NOT IN (1,100,999)";

            dc.QryDT_P(conn, strBorrow);

            //foreach (DataRow br in dc.dt.Rows)
            //{
            //    string key = br["tool_id"] + "|" + br["sub_id"] + "|" + br["system_id"];
            //    borrowingSet.Add(key);
            //}
            string curDoc = litIssueNo.Text.Trim();
            int rowNo = 1;
            foreach (DataRow row in dt.Rows)
            {
                row["RowNo"] = rowNo++;

                string tid = row["tool_id"].ToString();
                string sid = row["sub_no"].ToString();
                string sys = row["sys_status"].ToString();

                ProcInfo proc = GetToolProcessInfo(tid, sid, sys, curDoc);

                if (proc != null)
                {
                    row["DisplayStatus"] = proc.Label;
                    row["ProcUrl"] = proc.Url;
                }
                else
                {
                    int statusId = Convert.ToInt32(row["status_id"]);
                    row["DisplayStatus"] = (statusId == 1 || statusId == 2)
                        ? row["status"].ToString()
                        : "Spare : " + row["status"];
                    row["ProcUrl"] = "";
                }
            }

            ViewState["SearchResult"] = dt;

            gvSearchResult.DataSource = dt;
            gvSearchResult.DataBind();
        }
        
        /// <summary>บันทึกร่าง (status 1)</summary>
        protected void btnDraft_Click(object sender, EventArgs e)
        {
            string dd; if (AlreadyDone("DRAFT", out dd)) { ShowAlreadyDone(dd); return; }
            string empId = txtEmpId.Text.Trim();
            string empName = txtEmpName.Text.Trim();
            DataTable dt = ChangeListDT;
            string Doc_No = litIssueNo.Text.Trim();
            string status_id = "1";  //Wait Issue

            // ✅ ตรวจสอบ Employee ID ต้องเป็นตัวเลข 6 หลัก
            if (!System.Text.RegularExpressions.Regex.IsMatch(empId, @"^\d{6}$"))
            {
                alert_toast(
                    "Employee ID ต้องเป็นตัวเลข 6 หลัก", "รหัสพนักงานผิดพลาด", "warn", "10000"
                );
                txtEmpId.Focus();
                return;
            }

            // ✅ ตรวจสอบเบอร์ติดต่อ
            if (!ValidateTel()) return;

            if (!ValidateRemarkLength()) return;

            // ✅ ตรวจสอบ Dept Code กรณี Log-In ด้วย Dept Code
            if (pnlNewEmp.Visible == true)
            {
                alert_toast("กรุณาตรวจสอบ Comment ใน Step 1", "แจ้งเตือน", "warn", "10000");
                return;
            }

            // ✅ ตรวจสอบ Employee ID , Employee Name
            if (string.IsNullOrEmpty(empId) || string.IsNullOrEmpty(empName))
            {
                alert_toast("กรุณาตรวจสอบ Employee ID (ไม่มี Employee ID นี้ในระบบ) ", "แจ้งเตือน", "warn", "10000");
                return;
            }

            // ✅ นับเฉพาะรายการที่ยังอยู่ (ไม่รวมแถวที่ mark Delete หลังสลับโหมด)
            if (CountActiveRows() == 0)
            {
                alert_toast("ไม่พบข้อมูลในรายการแจ้งเปลี่ยนแปลง — กรุณาค้นหาและเพิ่มอุปกรณ์ใน Step 3",
                            "ไม่พบข้อมูลในรายการ", "warn", "10000");
                return;
            }

            // ✅ ตรวจ Use:Dept ตรงกัน (Div≠2)
            Save_ChangeListDT();   // sync ก่อนตรวจ
            if (!ValidateUseDeptSame()) return;
            if (!ValidateTypedValues()) return;

            // ก่อน saveData_ReturnDoc("1")
            if (hfFlowGenerated.Value == "1" && ViewState["DEPT_APV"] == null)
            {
                // re-resolve เผื่อ ViewState หาย
                var apv = ResolveDeptApprove();
                ViewState["DEPT_APV"] = apv;
                var dl = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrEmpty(apv.A1)) dl.Add(apv.A1);
                if (!string.IsNullOrEmpty(apv.A2)) dl.Add(apv.A2);
                ViewState["DEPT_APPROVE_LIST"] = dl;
            }

            // -- Accept to Save Data (draft = status 1)
            string savedDoc = saveData_ReturnDoc("1");
            if (string.IsNullOrEmpty(savedDoc)) return;
            MarkDone("DRAFT", savedDoc);

            // ✅ บันทึก approver ถ้า Generate + เลือกไว้แล้ว (draft ไม่บังคับ)
            if (hfFlowGenerated.Value == "1")
                SaveApproverFlow(savedDoc);
            else
                ClearApproverFlowDb(savedDoc);   // ✅ Step 6 ถูกล้าง → ลบ Approver เก่าใน DB

            // redirect
            string jsDocD = HttpUtility.JavaScriptStringEncode(savedDoc);
            string scriptD =
                "lockUI();" +
                "if(typeof showToast==='function'){ showToast('Doc No : " + jsDocD + "',{title:'Save Draft',type:'info',duration:2000}); }" +
                "setTimeout(function(){ window.location.href='EquipmentStatus2.aspx'; },2000);";
            ScriptManager.RegisterStartupScript(this, GetType(), "draftDone", scriptD, true);
        }

        private void ClearApproverFlowDb(string Doc_No)
        {
            using (var c = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString))
            using (var cmd = new SqlCommand("DELETE FROM NotificationofChange_Approver WHERE Doc_No=@d", c))
            {
                cmd.Parameters.AddWithValue("@d", Doc_No);
                c.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>ส่งเอกสาร — validate แล้วเปิด modal สรุป</summary>
        protected void btnSend_Click(object sender, EventArgs e)
        {
            if (!ValidateBeforeSend()) return;

            var deptApv = ResolveDeptApprove();
            ViewState["DEPT_APV"] = deptApv;

            var deptList = new List<string>();
            if (!string.IsNullOrEmpty(deptApv.A1)) deptList.Add(deptApv.A1);
            if (!string.IsNullOrEmpty(deptApv.A2)) deptList.Add(deptApv.A2);
            ViewState["DEPT_APPROVE_LIST"] = deptList;

            BuildSummaryTable();
            BindApproverSummary();
            ViewState["SEND_SNAPSHOT"] = BuildSendSnapshot();   // ✅ 6F-1

            ReopenModal();   // เปิด modal + คืนสถานะกล่อง Approver 2 + j3Init
        }

        /// <summary>เปิด Approve Modal — ตรวจสิทธิ์ผู้ login และเตรียมรายการ checkbox</summary>
        protected void btnApprove_Click(object sender, EventArgs e)
        {
            string Doc_No = litIssueNo.Text.Trim();
            string empId = Session["emp_id"] != null ? Session["emp_id"].ToString() : "";

            // ✅ เช็คสิทธิ์ + ขั้น (View)
            int step; string flag; int docStatus;
            if (!CheckApproverRight(Doc_No, empId, out step, out docStatus, out flag))
            {
                alert_toast("คุณไม่มีสิทธิ์อนุมัติเอกสารนี้ หรือยังไม่ถึงขั้นของคุณ", "ไม่มีสิทธิ์", "warn", "10000");
                return;
            }

            ViewState["APV_STEP"] = step;          // 1 หรือ 2
            ViewState["APV_DOCSTATUS"] = docStatus;

            litApvDoc.Text = Doc_No;
            BuildApproveTable();                    // ตาราง + checkbox default

            // badge บอกขั้น
            string badge = (step == 1)
                ? "<span class='badge' style='background:#2980b9;'>Approve 1 (Current Dept)</span>"
                : "<span class='badge' style='background:#e67e22;'>Approve 2 (Use Dept)</span>";
            ScriptManager.RegisterStartupScript(this, GetType(), "apvBadge",
                "var b=document.getElementById('apvStepBadge'); if(b) b.innerHTML=\"" + badge + "\";", true);

            // เปิด modal
            ScriptManager.RegisterStartupScript(this, GetType(), "openApvModal",
                "window.apvStep=" + step + "; window.apvSwap=" + (IsPairMode() ? "true" : "false") + ";" +
                "document.getElementById('" + pnlApproveModal.ClientID + "').style.display='flex'; " +
                "if(typeof apvBindEvents==='function') apvBindEvents();", true);
        }

        // ── เช็คสิทธิ์ approver จาก View ──
        private bool CheckApproverRight(string Doc_No, string empId, out int step, out int docStatus, out string flag)
        {
            step = 0; docStatus = 0; flag = "";
            if (string.IsNullOrEmpty(empId)) return false;

            string sql =
                "SELECT approve_step, doc_status, approve_flag FROM View_NotiChange_ApproverCheck" +
                " WHERE Doc_No=@doc AND emp_id=@emp";
            dc.QryDT_P(conn, sql,
                new SqlParameter("@doc", SqlDbType.NVarChar) { Value = Doc_No },
                new SqlParameter("@emp", SqlDbType.NVarChar) { Value = empId });

            if (dc.dt.Rows.Count == 0) return false;

            DataRow r = dc.dt.Rows[0];
            step = r["approve_step"] == DBNull.Value ? 0 : Convert.ToInt32(r["approve_step"]);
            docStatus = r["doc_status"] == DBNull.Value ? 0 : Convert.ToInt32(r["doc_status"]);
            flag = r["approve_flag"] == DBNull.Value ? "" : r["approve_flag"].ToString();

            // ✅ 6C-12 : Requester อนุมัติเอกสารตัวเองไม่ได้
            dc.QryDT_P(conn, "SELECT issue_empid FROM NotificationofChange_Control_List WHERE Doc_No=@d",
                new SqlParameter("@d", SqlDbType.NVarChar) { Value = Doc_No });
            if (dc.dt.Rows.Count > 0 && GetVal(dc.dt.Rows[0], "issue_empid") == empId.Trim())
                return false;

            // สิทธิ์ต้องตรงขั้นกับ status : step1↔30, step2↔31
            if (step == 1 && docStatus == 30) return true;
            if (step == 2 && docStatus == 31) return true;
            return false;
        }

        /// <summary>สร้างตารางรายการใน Approve Modal พร้อม default checkbox ตามขั้น A1/A2</summary>
        private void BuildApproveTable()
        {
            DataTable src = ChangeListDT;
            DataTable deptM = (DataTable)ViewState["DEPT_MASTER"];
            DataTable modelM = (DataTable)ViewState["MODEL_MASTER"];
            DataTable cellM = GetCellMaster();
            DataTable stationM = GetStationMaster();
            int step = ViewState["APV_STEP"] != null ? Convert.ToInt32(ViewState["APV_STEP"]) : 1;

            DataTable dt = new DataTable();
            foreach (string c in new[] { "RowKey", "FixAssetNo", "ControlNo", "ToolNo",
                                 "OldDept", "OldModel", "OldCell", "OldStation",
                                 "NewDept", "NewModel", "NewCell", "NewStation", "PrevA1", "PrevA2" })
            dt.Columns.Add(c, typeof(string));
            dt.Columns.Add("RowNo", typeof(int));
            dt.Columns.Add("DefChecked", typeof(bool));
            dt.Columns.Add("IsDiff", typeof(bool));
            dt.Columns.Add("PairNo", typeof(int));
            dt.Columns.Add("PairSlot", typeof(string));

            // รอบส่งกลับ = A1 กำลังอนุมัติ และมี a2_selected ค้างจาก A2
            bool isReturn = false;
            if (step == 1)
                foreach (DataRow r in src.Rows)
                    if (GetVal(r, "CompareData") != "Delete" && GetVal(r, "A2Selected") != "") { isReturn = true; break; }

            // (2) ก่อน int no = 1;  → ลำดับคู่สำหรับแสดง
            bool swap = (IsPairMode());
            var pairRank = new Dictionary<int, int>();
            var pairSeen = new Dictionary<int, int>();
            if (swap)
            {
                var ks = new List<int>();
                foreach (DataRow r in src.Rows)
                {
                    if (GetVal(r, "CompareData") == "Delete") continue;
                    int k = r["PairKey"] == DBNull.Value ? 0 : Convert.ToInt32(r["PairKey"]);
                    if (!ks.Contains(k)) ks.Add(k);
                }
                ks.Sort();
                for (int i = 0; i < ks.Count; i++) pairRank[ks[i]] = i + 1;
            }

            int no = 1;
            foreach (DataRow r in src.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;

                DataRow n = dt.NewRow();
                n["RowKey"] = RowKey(r);
                n["RowNo"] = no++;
                n["FixAssetNo"] = GetVal(r, "FixAssetNo");
                n["ControlNo"] = GetVal(r, "ControlNo");
                n["ToolNo"] = GetVal(r, "ToolNo");
                n["OldDept"] = FormatDeptDisplay(GetVal(r, "DeptCode"), GetVal(r, "DeptCodeReal"));
                n["OldModel"] = GetVal(r, "ModelName");
                n["OldCell"] = GetVal(r, "CellName");
                n["OldStation"] = GetVal(r, "StationName");
                n["NewDept"] = ResolveDeptName(deptM, GetVal(r, "Use_DeptCode"));
                n["NewModel"] = GetVal(r, "HD_Model") == "txt" ? GetVal(r, "Add_Model")
                                : ResolveName(modelM, "model_id", "model_name", GetVal(r, "Use_ModelId"));
                n["NewCell"] = GetVal(r, "HD_Cell") == "txt" ? GetVal(r, "Add_Cell")
                                : ResolveName(cellM, "location2_id", "location2_name", GetVal(r, "Use_CellId"));
                n["NewStation"] = GetVal(r, "HD_Station") == "txt" ? GetVal(r, "Add_Station")
                                : ResolveName(stationM, "location3_id", "location3_name", GetVal(r, "Use_StationId"));

                string a1 = GetVal(r, "A1Selected");
                string a2 = GetVal(r, "A2Selected");
                n["PrevA1"] = a1;
                n["PrevA2"] = a2;
                n["IsDiff"] = isReturn && a1 != a2;

                // default :
                //   step 1 → ตาม reject_flag (รอบส่งกลับ = ตามที่ A2 เลือก)
                //   step 2 → ตามที่ A1 เลือก
                bool def = GetVal(r, "RejectFlag") != "reject";
                if (step == 2) def = (a1 != "N");
                n["DefChecked"] = def;

                // (3) ในลูป ก่อน dt.Rows.Add(n);
                if (swap)
                {
                    int pk = r["PairKey"] == DBNull.Value ? 0 : Convert.ToInt32(r["PairKey"]);
                    int seen = pairSeen.ContainsKey(pk) ? pairSeen[pk] : 0;
                    n["PairNo"] = pairRank.ContainsKey(pk) ? pairRank[pk] : 0;
                    n["PairSlot"] = (seen == 0) ? "A" : "B";
                    pairSeen[pk] = seen + 1;
                }
                else { n["PairNo"] = 0; n["PairSlot"] = ""; }

                dt.Rows.Add(n);
            }

            gvApprove.Columns[1].Visible = isReturn;   // คอลัมน์ "รอบก่อน"
            gvApprove.DataSource = dt;
            gvApprove.DataBind();

            litApvNotice.Text = isReturn ? BuildReturnNotice() : "";
        }

        /// <summary>ตรวจข้อมูลทั้งหมดก่อนส่ง — ใช้ทั้งตอนกด Send และตอนกดยืนยันใน Modal</summary>
        private bool ValidateBeforeSend()
        {
            string empId = txtEmpId.Text.Trim();
            string empName = txtEmpName.Text.Trim();

            // ── Step 1 ──
            if (!System.Text.RegularExpressions.Regex.IsMatch(empId, @"^\d{6}$"))
            {
                alert_toast("Employee ID ต้องเป็นตัวเลข 6 หลัก", "แจ้งเตือน", "warn", "10000");
                txtEmpId.Focus();
                return false;
            }
            if (!ValidateTel()) return false;
            if (pnlNewEmp.Visible)
            {
                alert_toast("กรุณาตรวจสอบ Comment ใน Step 1", "แจ้งเตือน", "warn", "10000");
                return false;
            }
            if (empId == "" || empName == "")
            {
                alert_toast("กรุณาตรวจสอบ Employee ID (ไม่มี Employee ID นี้ในระบบ)", "แจ้งเตือน", "warn", "10000");
                return false;
            }

            // ── Step 4 ──
            if (CountActiveRows() == 0)
            {
                alert_toast("ไม่พบข้อมูลในรายการแจ้งเปลี่ยนแปลง — กรุณาค้นหาและเพิ่มอุปกรณ์ใน Step 3",
                            "ไม่พบข้อมูลในรายการ", "warn", "10000");
                return false;
            }
            Save_ChangeListDT();
            if (!ValidateTypedValues()) return false;
            if (Check_blankData())
            {
                alert_toast("กรุณาลงข้อมูลให้ครบถ้วน (ช่องสีเหลือง)", "ข้อมูลไม่ครบ", "warn", "10000");
                return false;
            }
            if (!ValidateUseDeptSame()) return false;
            if (IsPairMode() && !ValidateSwapPairs()) return false;

            // measure : ชื่อ Tool No / Model ไม่ตรงคู่ → เตือนอย่างเดียว ไม่ block การส่ง
            if (IsMeasureMode())
            {
                int mm = CountToolNameMismatchPairs();
                if (mm > 0)
                    alert_toast("มี " + mm + " คู่ที่ Tool No / Model ไม่ตรงกัน\n(ระบบให้ดำเนินการต่อได้)",
                                "ชื่อเครื่องมือวัดไม่ตรงกัน", "warn", "8000");
            }

            // ── Step 5 ──
            if (string.IsNullOrEmpty(ddlReason_Change.SelectedValue) || ddlReason_Change.SelectedValue == "0")
            {
                SetWarning_DDL(ddlReason_Change, true);
                alert_toast("กรุณาเลือกเหตุผลการแจ้งเปลี่ยนแปลง", "แจ้งเตือน", "warn", "10000");
                return false;
            }
            SetWarning_DDL(ddlReason_Change, false);
            if (!ValidateChangeComment()) return false;
            if (!ValidateRemarkLength()) return false;

            // ── Step 6 ──
            if (!ValidateApproverFlow()) return false;

            // ── อุปกรณ์ติดกระบวนการอื่น ──
            if (duringBorrowCheck())
            {
                BindChangeList();
                return false;
            }
            return true;
        }

        /// <summary>ข้อมูลที่ใช้ส่ง ณ ตอนเปิดหน้าสรุป — ใช้เทียบตอนกดยืนยัน</summary>
        private string BuildSendSnapshot()
        {
            var sb = new StringBuilder();
            sb.Append(hfChangeMode.Value).Append('|')
              .Append(ddlReason_Change.SelectedValue).Append('|')
              .Append((txt_ChangeComment.Text ?? "").Trim()).Append('|')
              .Append(hfJ3SelectedL.Value).Append('|')
              .Append(hfJ3SelectedR.Value).Append('#');

            string[] cols = { "ToolId", "ToolSub", "SystemId", "Use_DeptCode",
                      "HD_Model", "Use_ModelId", "Add_Model",
                      "HD_Cell", "Use_CellId", "Add_Cell",
                      "HD_Station", "Use_StationId", "Add_Station" };

            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                foreach (string c in cols) sb.Append(GetVal(r, c)).Append('|');
                sb.Append('#');
            }
            return sb.ToString();
        }

        // RowDataBound → set default checked
        protected void gvApprove_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            DataRowView drv = (DataRowView)e.Row.DataItem;

            int step = ViewState["APV_STEP"] != null ? Convert.ToInt32(ViewState["APV_STEP"]) : 1;
            bool def = drv["DefChecked"] != DBNull.Value && Convert.ToBoolean(drv["DefChecked"]);
            string a1 = drv["PrevA1"].ToString();
            int pairNo = drv["PairNo"] == DBNull.Value ? 0 : Convert.ToInt32(drv["PairNo"]);

            CheckBox cb = e.Row.FindControl("cbApv") as CheckBox;
            if (cb != null)
            {
                cb.Checked = def;
                cb.InputAttributes["data-a1"] = (a1 == "N") ? "N" : "Y";          // ใช้เทียบ A2 ↔ A1
                if (pairNo > 0) cb.InputAttributes["data-pair"] = pairNo.ToString(); // เลือกเป็นคู่
            }

            Literal litPrev = e.Row.FindControl("litPrev") as Literal;
            if (litPrev != null)
                litPrev.Text = PrevMark("A1", drv["PrevA1"].ToString()) + " &rarr; " + PrevMark("A2", drv["PrevA2"].ToString());

            // ✅ ข้อ 3 : ป้ายสถานะรายแถว (+ คู่ / ผลของ A1 ตอนขั้น 2)
            Literal litState = e.Row.FindControl("litRowState") as Literal;
            if (litState != null)
                litState.Text =
                    (pairNo > 0 ? "<span class='apv-pair-lbl'>คู่ " + pairNo + " (" + drv["PairSlot"] + ")</span>" : "") +
                    "<span class='res-badge apv-row-badge " + (def ? "res-ok" : "res-ng") + "'>" +
                    (def ? "✓ อนุมัติ" : "✗ ไม่อนุมัติ") + "</span>" +
                    (step == 2 ? "<div class='res-steps'>" + PrevMark("A1", a1) + "</div>" : "");

            var css = new List<string>();
            if (!def) css.Add("apv-row-off");
            if (drv["IsDiff"] != DBNull.Value && Convert.ToBoolean(drv["IsDiff"])) css.Add("apv-diff");
            if (pairNo > 0 && drv["PairSlot"].ToString() == "A" && e.Row.RowIndex > 0) css.Add("apv-pair-start");
            e.Row.CssClass = string.Join(" ", css.ToArray());
        }
        private string PrevMark(string who, string v)
        {
            if (v == "Y") return "<span class='prev-mk prev-y'>" + who + " ✓</span>";
            if (v == "N") return "<span class='prev-mk prev-n'>" + who + " ✗</span>";
            return "<span class='prev-mk'>" + who + " –</span>";
        }
        // กล่องแจ้งเตือน + เหตุผลการส่งกลับล่าสุด
        private string BuildReturnNotice()
        {
            string detail = "";
            dc.QryDT_P(conn,
                "SELECT TOP 1 Reject_detail, Reject_Date FROM Reject_Detail_All" +
                " WHERE Doc_No=@d AND Type='Return' ORDER BY Reject_Count DESC",
                new SqlParameter("@d", SqlDbType.NVarChar) { Value = litIssueNo.Text.Trim() });
            if (dc.dt.Rows.Count > 0) detail = GetVal(dc.dt.Rows[0], "Reject_detail");

            return "<div class='apv-return-notice'>" +
                     "<b>⚠️ เอกสารถูกส่งกลับจาก Approver 2</b>" +
                     "<div>Approver 2 เลือกรายการไม่ตรงกับรอบก่อน — ค่าเริ่มต้นของ checkbox เป็นตามที่ Approver 2 เลือก " +
                     "(แถวไฮไลต์ = รายการที่เลือกต่างกัน) กรุณาทบทวนแล้วยืนยันใหม่</div>" +
                     (detail != "" ? "<div class='apv-return-detail'>" + Server.HtmlEncode(detail) + "</div>" : "") +
                   "</div>";
        }

        /// <summary>AAD Confirm : เปิด Modal สรุป + เลือก Status Use / ผู้รับผิดชอบ</summary>
        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!IsAadEditable())
            {
                alert_toast("AAD Confirm ได้เฉพาะเอกสารสถานะรอ AAD Confirm (โหมด Confirm)", "ไม่สามารถยืนยันได้", "warn", "10000");
                return;
            }

            BuildConfirmSummary();
            ReopenConfirmModal();
        }

        /// <summary>ยืนยันจาก Modal — ตรวจ Dropdown / บันทึก Status Use + ผู้รับผิดชอบ รายแถว + Header 99</summary>
        protected void btnConfirmModalOk_Click(object sender, EventArgs e)
        {
            string cd; if (AlreadyDone("CONFIRM", out cd)) { ShowAlreadyDone(cd); return; }

            if (!IsAadEditable())
            {
                alert_toast("AAD Confirm ได้เฉพาะเอกสารสถานะรอ AAD Confirm (โหมด Confirm)", "ไม่สามารถยืนยันได้", "warn", "10000");
                return;
            }

            // ── อ่าน + ตรวจ Dropdown ใน Modal (ไม่ครบ → เปิด Modal ค้างไว้ พร้อมช่องสีเหลือง) ──
            if (!ReadAndValidateConfirmModal())
            {
                ReopenConfirmModal();
                return;
            }

            string Doc_No = litIssueNo.Text.Trim();
            string empId = Session["emp_id"] != null ? Session["emp_id"].ToString() : "";
            bool ok = false;

            conn.Open();
            SqlTransaction tx = conn.BeginTransaction();
            try
            {
                // ── Header → 99 (เฉพาะเอกสารที่ยังเป็น 50) ──
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE NotificationofChange_Control_List" +
                    " SET status_id = 99, update_date = @dt, confirm_empid = @emp, confim_date = @dt" +
                    " WHERE Doc_No = @Doc_No AND status_id = 50", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@dt", DateTime.Now);
                    cmd.Parameters.AddWithValue("@emp", empId);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException("สถานะเอกสารเปลี่ยนไปแล้ว (ไม่ใช่ 50) กรุณารีเฟรชหน้า");
                }

                // ── รายการที่อนุมัติ → 99 + Status Use / ผู้รับผิดชอบ ──
                foreach (DataRow r in ChangeListDT.Rows)
                {
                    if (GetVal(r, "CompareData") == "Delete") continue;
                    if (GetVal(r, "RejectFlag") == "reject") continue;

                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE NotificationofChange_List" +
                        " SET status_id = 99, Use_Status_Id = @st, Resp_EmpId = @rp, Resp_Name = @rn" +
                        " WHERE Doc_No=@doc AND tool_id=@tid AND sub_id=@sid AND system_id=@sys", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@st", ToDbVal(r, "Use_StatusId"));
                        cmd.Parameters.AddWithValue("@rp", ToDbVal(r, "Resp_EmpId"));
                        cmd.Parameters.AddWithValue("@rn", ToDbVal(r, "Resp_Name"));
                        AddKeyParams(cmd, Doc_No, r);
                        cmd.ExecuteNonQuery();
                    }
                }

                tx.Commit();
                ok = true;
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                alert_toast("AAD Confirm ไม่สำเร็จ : " + ex.Message, "Error", "error", "10000");
            }
            finally { if (conn.State == ConnectionState.Open) conn.Close(); }

            if (!ok) return;

            MarkDone("CONFIRM", Doc_No);

            // ── อีเมล : To Requester , CC Admin Level 1 ──
            try { SendResultMail(Doc_No, MailKind.Confirmed, 3, null); }
            catch (Exception exMail) { System.Diagnostics.Debug.WriteLine("[Mail Confirm] " + exMail.Message); }

            string jsDoc = HttpUtility.JavaScriptStringEncode(Doc_No);
            string url = HttpUtility.JavaScriptStringEncode(ResolveUrl("~/ApproveStatus.aspx"));
            string script =
                "var m=document.getElementById('" + pnlConfirmModal.ClientID + "'); if(m) m.style.display='none';" +
                "lockUI();" +
                "if(typeof showToast==='function'){ showToast('Doc No : " + jsDoc + "',{title:'AAD Confirm เรียบร้อย',type:'success',duration:2000}); }" +
                "setTimeout(function(){ window.location.replace('" + url + "'); },2000);";
            ScriptManager.RegisterStartupScript(this, GetType(), "confirmDone", script, true);
        }

        /// <summary>Reject เอกสารทั้งฉบับ พร้อมบันทึกเหตุผล</summary>
        protected void btnReject_Click(object sender, EventArgs e)
        {
            string Doc_No = litIssueNo.Text.Trim();
            string comment = txt_rejectComment.Text.Trim();
            //string mode = ViewState["MODE"].ToString();
            string mode = CurrentMode;
            string empId = "";
            string empName = "";
            string link_path = "EquipmentStatus2.aspx";

            // ✅ อ่าน mode
            if (!string.IsNullOrEmpty(Request.QueryString["mode"]))
            {
                mode = Request.QueryString["mode"].ToLower();
            }

            if (mode == "confirm" || mode == "approve")
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
                alert_toast("กรุณากรอก Comment อย่างน้อย 10 ตัวอักษร", "Warning", "warn", "10000");

                txt_rejectComment.Style["border-color"] = "#ffc107";
                txt_rejectComment.Style["background-color"] = "#fff3cd";

                txt_rejectComment.Focus();
                return;
            }

            // ✅ ผ่านแล้ว → ทำ Reject ต่อ
            //Search Status
            string status_id = ViewState["STATUS_ID"].ToString();

            //Search Max Reject Count
            int Reject_Count = GetNextRejectCount(Doc_No);

            DateTime rejectDate = DateTime.Now;

            conn.Open();

            try
            {
                // ── Update NotificationofChange_Control_List → status 1 ──
                #region
                string sqlHeader =
                    "UPDATE NotificationofChange_Control_List" +
                    " SET status_id = 1 , update_date = @update_date" +
                    " WHERE Doc_No = @Doc_No";

                using (SqlCommand cmd = new SqlCommand(sqlHeader, conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@update_date", rejectDate);
                    cmd.ExecuteNonQuery();
                }
                #endregion

                // ── Update NotificationofChange_List → status 1 ──
                #region
                string sqlList =
                    "UPDATE NotificationofChange_List" +
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
                    cmd.Parameters.AddWithValue("@Doc_Type", "AAD2");
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
            }
            catch (Exception ex)
            {
                alert_toast("เกิดข้อผิดพลาด : " + ex.Message, "Error", "error", "10000");
            }
            finally
            {
                string alert_info = "Reject";
                conn.Close(); // ✅ ปิดทุกครั้ง

                //alert_toast("Doc No : " + Doc_No, alert_info, "warn", "2000");

                string jsDoc = HttpUtility.JavaScriptStringEncode(Doc_No);
                string script =
                    "lockUI();" +
                    "if (typeof showToast === 'function') {" +
                    "  showToast('Doc No : " + jsDoc + "', { title: 'Reject Done', type: 'warn', duration: 2000 });" +
                    "}" +
                    "setTimeout(function () { window.location.href = '" + link_path + "'; }, 2000);";

                ScriptManager.RegisterStartupScript(
                    this, this.GetType(), "redirectScript", script, true);
            }
        }
        /// <summary>ยกเลิกเอกสาร (status 999)</summary>
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            string Doc_No = litIssueNo.Text.Trim();
            string comment = txt_rejectComment.Text.Trim();
            //string mode = ViewState["MODE"].ToString();
            string mode = CurrentMode;
            string empId = "";
            string empName = "";
            string link_path = "EquipmentStatus2.aspx";

            // ✅ อ่าน mode
            if (!string.IsNullOrEmpty(Request.QueryString["mode"]))
            {
                mode = Request.QueryString["mode"].ToLower();
            }

            if (mode == "confirm" || mode == "approve")
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

            // ✅ ต้องมี Doc_No ถึงจะ Cancel ได้ -> redirect menulist
            if (Doc_No == "-" || string.IsNullOrEmpty(Doc_No))
            {
                alert_toast("ไม่พบเลขเอกสาร ไม่สามารถยกเลิกได้", "Cancel Error", "error", "10000");
                return;
            }

            // ✅ ต้องมี Comment ถึงจะ Cancel ได้
            if (string.IsNullOrEmpty(comment) || comment.Length < 10)
            {
                alert_toast("กรุณากรอก Comment อย่างน้อย 10 ตัวอักษร", "Warning", "warn", "10000");

                txt_rejectComment.Style["border-color"] = "#ffc107";
                txt_rejectComment.Style["background-color"] = "#fff3cd";

                txt_rejectComment.Focus();
                return;
            }

            conn.Open();
            try
            {
                // ── Update Withdraw_Control_List → status 999 ───────────────────────
                #region
                string sqlHeader =
                    "UPDATE NotificationofChange_Control_List" +
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

                // ── Update Borrow_List → status 999 ──────────────────────────────
                #region
                string sqlList =
                    "UPDATE NotificationofChange_List" +
                    " SET status_id = 999" +
                    " WHERE Doc_No = @Doc_No";

                using (SqlCommand cmd = new SqlCommand(sqlList, conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.ExecuteNonQuery();
                }
                #endregion

                //Search Max Control No
                #region
                int Reject_Count = 0;
                string str_max = " select max(Reject_Count)+1 as MaxReject" +
                                 " from Reject_Detail_All" +
                                 " where Doc_No = '" + Doc_No + "'";
                dc.QryDT(conn, str_max);

                if (dc.dt.Rows[0]["MaxReject"] == DBNull.Value)
                {
                    Reject_Count = 1;
                }
                else
                {
                    Reject_Count = Convert.ToInt32(dc.dt.Rows[0]["MaxReject"].ToString());
                }
                #endregion

                // --- Update Cancel History
                #region
                DateTime rejectDate = DateTime.Now;
                //string emp_id = txtEmpId.Text.Trim();
                string status_id = ViewState["STATUS_ID"].ToString().Trim();

                string sqlCancel = @"
                    INSERT INTO Reject_Detail_All
                    (
                        Doc_No , Doc_Type , status_id , Reject_Count ,
                        emp_id , Reject_Date , Reject_detail , Type
                    )
                    VALUES
                    (
                        @Doc_No , @Doc_Type , @status_id , @Reject_Count ,
                        @emp_id , @Reject_Date , @Reject_detail , @Type
                    )";

                using (SqlCommand cmd = new SqlCommand(sqlCancel, conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@Doc_Type", "AAD2");
                    cmd.Parameters.AddWithValue("@status_id", status_id);
                    cmd.Parameters.AddWithValue("@Reject_Count", Reject_Count);
                    cmd.Parameters.AddWithValue("@emp_id", empId);
                    cmd.Parameters.AddWithValue("@Reject_Date", rejectDate);
                    cmd.Parameters.AddWithValue("@Reject_detail", comment);
                    cmd.Parameters.AddWithValue("@Type", "Cancel");
                    cmd.ExecuteNonQuery();
                }
                #endregion

            }
            catch (Exception ex)
            {
                alert_toast("Cancel Error : " + ex.Message, "Error", "error", "10000");
                return;
            }
            finally
            {
                conn.Close();
            }

            // ── Redirect หลัง Cancel สำเร็จ ──────────────────────────────────────
            string script =
                "lockUI();" +
                "setTimeout(function() {" +
                "  window.location.href = '" + link_path + @"';" +
                "}, 2000);";

            ScriptManager.RegisterStartupScript(
                this, this.GetType(),
                "cancelScript", script, true
            );

            alert_toast("Doc No : " + Doc_No + " — ยกเลิกเรียบร้อยแล้ว",
                        "Cancel Complete", "info", "2000");
        }

        /// <summary>ยืนยันจาก modal Send — ตรวจซ้ำ / เทียบ snapshot / กันกดซ้ำ แล้วบันทึก</summary>
        protected void btnModalConfirm_Click(object sender, EventArgs e)
        {
            // ✅ 6F-5 : กดซ้ำหลังส่งไปแล้ว
            string doneDoc;
            if (AlreadyDone("SEND", out doneDoc)) { ShowAlreadyDone(doneDoc); return; }

            // ✅ 6F-1 : ตรวจทุกอย่างใหม่ (อุปกรณ์อาจถูกเอกสารอื่นจอง / สิทธิ์ผู้อนุมัติอาจเปลี่ยน)
            //          ถ้าไม่ผ่าน Modal จะปิดเอง (UpdatePanel render ใหม่) และเห็นข้อความเตือนบนหน้าหลัก
            if (!ValidateBeforeSend()) return;

            // ✅ 6F-1 : ข้อมูลต้องตรงกับที่เห็นในหน้าสรุป
            string snap = BuildSendSnapshot();
            if ((ViewState["SEND_SNAPSHOT"] as string) != snap)
            {
                BuildSummaryTable();
                BindApproverSummary();
                ViewState["SEND_SNAPSHOT"] = snap;
                alert_toast("ข้อมูลมีการเปลี่ยนแปลงหลังเปิดหน้าสรุป ระบบแสดงสรุปใหม่แล้ว กรุณาตรวจสอบอีกครั้งก่อนยืนยัน",
                            "ข้อมูลเปลี่ยนแปลง", "warn", "10000");
                ReopenModal();
                return;
            }

            var deptList = ViewState["DEPT_APPROVE_LIST"] as List<string>;
            if (deptList == null || deptList.Count == 0)
            {
                alert_toast("ไม่พบข้อมูลแผนก Approve กรุณาลองใหม่", "Error", "error", "10000");
                return;
            }

            // ── บันทึก header + list (status 30) ──
            string Doc_No = saveData_ReturnDoc("30");
            if (string.IsNullOrEmpty(Doc_No)) return;

            // ── บันทึก approver (A1 = 30 , A2 = 31) ──
            SaveApproverFlow(Doc_No);

            MarkDone("SEND", Doc_No);   // ✅ 6F-5 : ก่อนส่งเมล — Request ที่ซ้ำเข้ามาจะไม่ส่งเมลซ้ำ

            // ── เมลถึง Approver 1 (best-effort) ──
            try { SendApproverMail(Doc_No, "30"); }
            catch (Exception exMail) { System.Diagnostics.Debug.WriteLine("[SendApproverMail] " + exMail.Message); }

            // ── redirect ──
            string jsDoc = HttpUtility.JavaScriptStringEncode(Doc_No);
            string url = HttpUtility.JavaScriptStringEncode(ResolveUrl("~/EquipmentStatus2.aspx"));
            string script =
                "lockUI();" +
                "if(typeof showToast==='function'){ showToast('Doc No : " + jsDoc + "',{title:'Send Complete',type:'info',duration:2000}); }" +
                "setTimeout(function(){ window.location.replace('" + url + "'); },2000);";
            ScriptManager.RegisterStartupScript(this, GetType(), "sendDoneScript", script, true);
        }

        protected void btnRejectModalConfirm_Click(object sender, EventArgs e)
        {
            string rd; if (AlreadyDone("REJECT", out rd)) { ShowAlreadyDone(rd); return; }
            string comment = txt_rejectComment.Text.Trim();

            // validate ซ้ำฝั่ง server (กัน bypass JS)
            if (string.IsNullOrEmpty(comment) || comment.Length < 10)
            {
                alert_toast("กรุณากรอก Comment อย่างน้อย 10 ตัวอักษร", "ข้อมูลไม่ครบ", "warn", "10000");
                ReopenRejectModal();
                return;
            }

            string mode = hfRejectMode.Value;
            if (mode == "reject")
                DoReject(comment);
            else if (mode == "cancel")
                DoCancel(comment);
            else
            {
                alert_toast("ไม่พบโหมดการทำงาน", "Error", "error", "10000");
            }
        }

        // เปิด modal ซ้ำ (กรณี validate ไม่ผ่าน)
        private void ReopenRejectModal()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "reopenReject",
                "document.getElementById('" + pnlRejectModal.ClientID + "').style.display='flex';", true);
        }

        // helper
        private System.Collections.Generic.List<string> SplitIds(string raw)
        {
            return (raw ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).Where(x => x.Length > 0).Distinct().ToList();
        }

        /// <summary>เปิด modal Send ซ้ำ (กรณี validate ไม่ผ่าน) พร้อม re-show ฝั่งขวา</summary>
        private void ReopenModal()
        {
            var deptList = ViewState["DEPT_APPROVE_LIST"] as System.Collections.Generic.List<string>
                   ?? new System.Collections.Generic.List<string>();

            // re-show กล่องขวาถ้ามี 2 แผนก
            string toggleR = (deptList.Count >= 2)
                ? "var r=document.getElementById('j3BoxR'); if(r) r.style.display='block';"
                : "var r=document.getElementById('j3BoxR'); if(r) r.style.display='none';";

            string script =
                "document.getElementById('" + pnlSendModal.ClientID + "').style.display='flex';" +
                toggleR +
                "if(typeof j3Init==='function'){ j3Init(); }";   // re-init chips/dropdown จาก HiddenField

            ScriptManager.RegisterStartupScript(this, GetType(), "reopenModal", script, true);
        }
        private void DoReject(string comment)
        {
            string Doc_No = litIssueNo.Text.Trim();
            //string mode = ViewState["MODE"].ToString();
            string mode = CurrentMode;
            string empId = "", empName = "", link_path = "EquipmentStatus2.aspx";

            if (!string.IsNullOrEmpty(Request.QueryString["mode"]))
                mode = Request.QueryString["mode"].ToLower();

            if (mode == "confirm" || mode == "approve")
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
            empName = empName.Split(' ')[0];

            string status_id = ViewState["STATUS_ID"].ToString();
            int Reject_Count = GetNextRejectCount(Doc_No);
            DateTime rejectDate = DateTime.Now;

            conn.Open();
            try
            {
                // ── UPDATE Control_List → status 1 + ล้างผู้อนุมัติรอบก่อน (7F-4) ──
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE NotificationofChange_Control_List" +
                    " SET status_id=1, update_date=@d," +
                    "     approve_empid=NULL, approve_date=NULL, approve_empid2=NULL, approve_date2=NULL" +
                    " WHERE Doc_No=@doc", conn))
                {
                    cmd.Parameters.AddWithValue("@doc", Doc_No);
                    cmd.Parameters.AddWithValue("@d", rejectDate);
                    cmd.ExecuteNonQuery();
                }

                // ── ผู้อนุมัติทุกขั้นกลับเป็น wait (7F-4) ──
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE NotificationofChange_Approver SET approve_flag='wait', approve_date=NULL WHERE Doc_No=@doc", conn))
                {
                    cmd.Parameters.AddWithValue("@doc", Doc_No);
                    cmd.ExecuteNonQuery();
                }

                // ── UPDATE List → status 1 ──
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE NotificationofChange_List" +
                    " SET status_id=1, reject_flag=NULL, a1_selected=NULL, a2_selected=NULL" +   // ✅ เริ่มใหม่ทั้งหมด
                    " WHERE Doc_No=@doc", conn))
                {
                    cmd.Parameters.AddWithValue("@doc", Doc_No);
                    cmd.ExecuteNonQuery();
                }
                // ── INSERT Reject History ──
                string sql = @"INSERT INTO Reject_Detail_All
                                (Doc_No, Doc_Type, status_id, Reject_Count, emp_id, name, Reject_Date, Reject_detail, Type)
                                VALUES (@Doc_No, @Doc_Type, @status_id, @Reject_Count, @emp_id, @name, @Reject_Date, @Reject_detail, @Type)";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@Doc_Type", "AAD2");
                    cmd.Parameters.AddWithValue("@status_id", status_id);
                    cmd.Parameters.AddWithValue("@Reject_Count", Reject_Count);
                    cmd.Parameters.AddWithValue("@emp_id", empId);
                    cmd.Parameters.AddWithValue("@name", empName);
                    cmd.Parameters.AddWithValue("@Reject_Date", rejectDate);
                    cmd.Parameters.AddWithValue("@Reject_detail", comment);
                    cmd.Parameters.AddWithValue("@Type", "Reject");
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                alert_toast("เกิดข้อผิดพลาด : " + ex.Message, "Error", "error", "10000");
                conn.Close();
                ReopenRejectModal();
                return;
            }
            finally { if (conn.State == ConnectionState.Open) conn.Close(); }

            MarkDone("REJECT", Doc_No);

            // ── 3.1 Reject → To Requester , CC Approver ตามขั้น ──
            if (mode == "approve" || mode == "confirm")
            {
                int rejStep = status_id == "30" ? 1 : (status_id == "31" ? 2 : 3);
                try { SendResultMail(Doc_No, MailKind.Reject, rejStep, comment); }
                catch (Exception exMail) { System.Diagnostics.Debug.WriteLine("[Mail Reject] " + exMail.Message); }
            }

            string jsDoc = HttpUtility.JavaScriptStringEncode(Doc_No);
            string script =
                "lockUI();" +
                "if(typeof showToast==='function'){ showToast('Doc No : " + jsDoc + "',{title:'Reject Done',type:'warn',duration:2000}); }" +
                "setTimeout(function(){ window.location.href='" + link_path + "'; },2000);";
            ScriptManager.RegisterStartupScript(this, GetType(), "redirectScript", script, true);
        }
        private void DoCancel(string comment)
        {
            if (PageMode != "edit")
            {
                alert_toast("ยกเลิกคำขอได้เฉพาะโหมด Issue เท่านั้น", "ไม่มีสิทธิ์", "warn", "10000");
                return;
            }

            string Doc_No = litIssueNo.Text.Trim();
            //string mode = ViewState["MODE"].ToString();
            string mode = CurrentMode;
            string empId = "", empName = "", link_path = "EquipmentStatus2.aspx";

            if (!string.IsNullOrEmpty(Request.QueryString["mode"]))
                mode = Request.QueryString["mode"].ToLower();

            if (mode == "confirm" || mode == "approve")
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
            empName = empName.Split(' ')[0];

            // ต้องมี Doc_No ถึง cancel ได้
            if (Doc_No == "-" || string.IsNullOrEmpty(Doc_No))
            {
                alert_toast("ไม่พบเลขเอกสาร ไม่สามารถยกเลิกได้", "Cancel Error", "error", "10000");
                return;
            }

            conn.Open();
            try
            {
                // ── UPDATE Control_List → status 999 ──
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE NotificationofChange_Control_List" +
                    " SET status_id=999, update_date=@update_date, cancel_empid=@cancel_empid, cancel_date=@cancel_date" +
                    " WHERE Doc_No=@Doc_No", conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@cancel_empid", empId);
                    cmd.Parameters.AddWithValue("@cancel_date", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }

                // ── UPDATE List → status 999 ──
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE NotificationofChange_List SET status_id=999 WHERE Doc_No=@Doc_No", conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.ExecuteNonQuery();
                }

                // ── Max Reject Count ──
                int Reject_Count = 0;
                string str_max = " select max(Reject_Count)+1 as MaxReject" +
                                 " from Reject_Detail_All where Doc_No = '" + Doc_No + "'";
                dc.QryDT(conn, str_max);
                Reject_Count = (dc.dt.Rows[0]["MaxReject"] == DBNull.Value)
                    ? 1 : Convert.ToInt32(dc.dt.Rows[0]["MaxReject"].ToString());

                // ── INSERT Cancel History ──
                DateTime rejectDate = DateTime.Now;
                string status_id = ViewState["STATUS_ID"].ToString().Trim();

                string sqlCancel = @"INSERT INTO Reject_Detail_All
                    (Doc_No, Doc_Type, status_id, Reject_Count, emp_id, Reject_Date, Reject_detail, Type)
                    VALUES (@Doc_No, @Doc_Type, @status_id, @Reject_Count, @emp_id, @Reject_Date, @Reject_detail, @Type)";
                using (SqlCommand cmd = new SqlCommand(sqlCancel, conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@Doc_Type", "AAD2");
                    cmd.Parameters.AddWithValue("@status_id", status_id);
                    cmd.Parameters.AddWithValue("@Reject_Count", Reject_Count);
                    cmd.Parameters.AddWithValue("@emp_id", empId);
                    cmd.Parameters.AddWithValue("@Reject_Date", rejectDate);
                    cmd.Parameters.AddWithValue("@Reject_detail", comment);
                    cmd.Parameters.AddWithValue("@Type", "Cancel");
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                alert_toast("Cancel Error : " + ex.Message, "Error", "error", "10000");
                conn.Close();
                ReopenRejectModal();
                return;
            }
            finally { if (conn.State == ConnectionState.Open) conn.Close(); }

            MarkDone("REJECT", Doc_No);

            // ── Redirect ──
            string jsDoc = HttpUtility.JavaScriptStringEncode(Doc_No);
            string script =
                "lockUI();" +
                "if(typeof showToast==='function'){ showToast('Doc No : " + jsDoc + " — ยกเลิกเรียบร้อย',{title:'Cancel Complete',type:'info',duration:2000}); }" +
                "setTimeout(function(){ window.location.href='" + link_path + "'; },2000);";
            ScriptManager.RegisterStartupScript(this, GetType(), "cancelScript", script, true);
        }

        /// <summary>บันทึก J3 Approver ของ 1 แผนกลง NotificationofChange_Approver</summary>
        private void InsertApproverGroup(string Doc_No, string deptApprove,
            System.Collections.Generic.List<string> empIds, string statusId)
        {
            string sql = @"
                INSERT INTO NotificationofChange_Approver
                ( Doc_No, emp_id, emp_name, email, position_name, position_code, tel, dept_code, status_id, approve_flag, create_date )
                VALUES ( @Doc_No, @emp_id, @emp_name, @email, @position_name, @position_code, @tel, @dept_code, @status_id, @approve_flag, @create_date )";

            deptApprove = (deptApprove ?? "").Trim();

            string qSql;
            if (deptApprove == "5505")
                qSql = "SELECT TOP 1 Name, email, position_name, position_code FROM View_Authorize_J3_Approve" +   
                       " WHERE document_id='AAD2' AND status_id=30 AND LTRIM(RTRIM(dept_code))=@dept AND emp_id=@emp";
            else
                qSql = "SELECT TOP 1 Name, email, position_name, position_code FROM View_Authorize_J3_Approve" +
                       " WHERE document_id='AAD2' AND status_id=30" +
                       "   AND LEFT(LTRIM(RTRIM(dept_code)),3)=@deptPrefix" +
                       "   AND LTRIM(RTRIM(dept_code)) <> '5505' AND emp_id=@emp";

            foreach (string empId in empIds)
            {
                string name = null, email = null, pos = null, posCode = null;
                using (SqlCommand q = new SqlCommand(qSql, conn))
                {
                    if (deptApprove == "5505")
                        q.Parameters.AddWithValue("@dept", deptApprove);
                    else
                        q.Parameters.AddWithValue("@deptPrefix", deptApprove.Substring(0, Math.Min(3, deptApprove.Length)));
                    q.Parameters.AddWithValue("@emp", empId);
                    using (SqlDataReader rd = q.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            name = rd["Name"] == DBNull.Value ? null : rd["Name"].ToString();
                            email = rd["email"] == DBNull.Value ? null : rd["email"].ToString();
                            pos = rd["position_name"] == DBNull.Value ? null : rd["position_name"].ToString();
                            posCode = rd["position_code"] == DBNull.Value ? null : rd["position_code"].ToString();
                        }
                    }
                }
                if (name == null && email == null) continue;

                // tel จาก sys_user_master
                string tel = "";
                dc.QryDT(conn_user, "SELECT ext FROM sys_user_master WHERE emp_id='" + empId + "'");
                if (dc.dt.Rows.Count > 0) tel = GetVal(dc.dt.Rows[0], "ext");

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@emp_id", empId);
                    cmd.Parameters.AddWithValue("@emp_name", (object)name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@email", (object)email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@position_name", (object)pos ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@position_code", (object)posCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@tel", string.IsNullOrEmpty(tel) ? (object)DBNull.Value : tel);
                    cmd.Parameters.AddWithValue("@dept_code", deptApprove);
                    cmd.Parameters.AddWithValue("@status_id", statusId);
                    cmd.Parameters.AddWithValue("@approve_flag", "wait");
                    cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ── บันทึก J3 Approver ที่เลือก ──
        private void SaveJ3Approver(string Doc_No, string deptApprove, System.Collections.Generic.List<string> empIds)
        {
            conn.Open();
            try
            {
                // ล้างของเดิม
                using (SqlCommand del = new SqlCommand(
                    "DELETE FROM NotificationofChange_Approver WHERE Doc_No=@Doc_No", conn))
                {
                    del.Parameters.AddWithValue("@Doc_No", Doc_No);
                    del.ExecuteNonQuery();
                }

                string sql = @"
                    INSERT INTO NotificationofChange_Approver
                    ( Doc_No, emp_id, emp_name, email, dept_code, status_id, approve_flag, create_date )
                    VALUES
                    ( @Doc_No, @emp_id, @emp_name, @email, @dept_code, @status_id, @approve_flag, @create_date )";

                foreach (string empId in empIds)
                {
                    // ✅ query ชื่อ/email กลับ — match dept แบบกลุ่มเดียวกัน (เหมือน BindJ3Approver)
                    string qSql;
                    if ((deptApprove ?? "").Trim() == "5505")
                    {
                        qSql = "SELECT TOP 1 Name, email FROM View_Authorize_J3_Approve" +
                               " WHERE document_id='AAD2' AND status_id=30 AND LTRIM(RTRIM(dept_code))=@dept AND emp_id=@emp";
                    }
                    else
                    {
                        qSql = "SELECT TOP 1 Name, email FROM View_Authorize_J3_Approve" +
                               " WHERE document_id='AAD2' AND status_id=30" +
                               "   AND LEFT(LTRIM(RTRIM(dept_code)),3)=@deptPrefix" +
                               "   AND LTRIM(RTRIM(dept_code)) <> '5505'" +
                               "   AND emp_id=@emp";
                    }

                    string name = null, email = null;
                    using (SqlCommand q = new SqlCommand(qSql, conn))
                    {
                        if ((deptApprove ?? "").Trim() == "5505")
                            q.Parameters.AddWithValue("@dept", deptApprove.Trim());
                        else
                            q.Parameters.AddWithValue("@deptPrefix",
                                (deptApprove ?? "").Trim().Substring(0, Math.Min(3, (deptApprove ?? "").Trim().Length)));

                        q.Parameters.AddWithValue("@emp", empId);
                        using (SqlDataReader rd = q.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                name = rd["Name"] == DBNull.Value ? null : rd["Name"].ToString();
                                email = rd["email"] == DBNull.Value ? null : rd["email"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                alert_toast("บันทึก Approver ผิดพลาด : " + ex.Message, "Error", "error", "10000");
            }
            finally
            {
                conn.Close();
            }
        }

        #endregion

        #region ===== Select / Text Changed =====
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

                // ✅ lock EMP ID หลังกรอกสำเร็จ (กัน re-generate รัว) — ต้อง reload ถ้าจะเปลี่ยน
                txtEmpId.ReadOnly = true;
                txtEmpId.Style["background-color"] = "#d4edda";

                // Admin : ไม่ต้องตรวจ dept → ซ่อน notice ได้เลย
                if (admin == "99")
                {
                    pnlNewEmp.Visible = false;
                    return;
                }

                // User ปกติ : ตรวจ dept
                Requester_getdata_edit();
                //AutoGenerateApproverFlow();
            }
            catch (Exception ex)
            {
                alert_toast(ex.ToString(), "txtEmpId_TextChanged", "warn", "10000");
            }
        }
        // ── กดปุ่ม "แจ้งเปลี่ยน Location" ──
        protected void btnModeChange_Click(object sender, EventArgs e)
        {
            ApplyChangeMode("change");
        }
        // ── คลิกที่ตัวสวิตช์ → สลับไปอีกโหมด ──
        protected void btnModeToggle_Click(object sender, EventArgs e)
        {
            string target = (hfChangeMode.Value == "change") ? "swap" : "change";
            SwitchChangeType(target);
        }
        // สลับโหมด + ล้าง list (ใช้ได้ทั้ง 2 ทิศ)
        private void SwitchChangeType(string newMode)
        {
            if (PageMode != "edit") return;   // ✅ R14 : สลับได้เฉพาะโหมด Issue

            // ล้าง Change List ทั้งหมด
            DataTable dt = ChangeListDT;
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                DataRow r = dt.Rows[i];
                if (GetVal(r, "CompareData") == "Delete") continue;
                if (GetVal(r, "CompareData") == "Insert") r.Delete();
                else r["CompareData"] = "Delete";
            }
            dt.AcceptChanges();
            ChangeListDT = dt;

            hfChangeMode.Value = newMode;
            ViewState["CHANGE_MODE"] = newMode;
            UpdateModeButtonUI(newMode);

            // ✅ ข้อ 2 : ล้าง Step 6 (Approver)
            ResetApproverFlowUI();

            // ✅ ล้างผลค้นหา Step 3 + Remark
            ViewState["SearchResult"] = null;
            gvSearchResult.DataSource = null;
            gvSearchResult.DataBind();
            usedept_remark.Visible = false;

            BindChangeList_Display();
        }

        // set remark ตามโหมด
        //private void UpdateChangeTypeRemark(string mode)
        //{
        //    if (mode == "swap")
        //        changetype_remark.Text =
        //            "<span class='badge' style='background:#e4ff5d; color:#555; font-size:13px; text-shadow:none;'>" +
        //            "หมายเหตุ : \"แจ้งสลับ\" จับคู่ทีละ 2 รายการ (1-2, 3-4, ...) ต้องมี 2 Dept เท่านั้น และแต่ละคู่ต้องคนละ Dept</span>";
        //    else
        //        changetype_remark.Text =
        //            "<span class='badge' style='background:#e4ff5d; color:#555; font-size:13px; text-shadow:none;'>" +
        //            "หมายเหตุ : \"แจ้งเปลี่ยน Location\" ต้องทำรายการเฉพาะ Dept Code เดียวกันเท่านั้น [ไม่จำกัดจำนวนรายการ]</span>";
        //}

        // ── กดปุ่ม "แจ้งสลับ" ──

        protected void btnModeSwap_Click(object sender, EventArgs e)
        {
            // ถ้าเป็น swap อยู่แล้ว → ไม่ทำอะไร
            if (hfChangeMode.Value == "swap")
            {
                UpdateModeButtonUI("swap");
                return;
            }

            SyncGridViewToChangeListDT();

            // มีรายการ active ไหม
            int activeCount = 0;
            foreach (DataRow r in ChangeListDT.Rows)
                if (GetVal(r, "CompareData") != "Delete") activeCount++;

            if (activeCount > 0)
            {
                // ✅ มีรายการ → เปิด modal ยืนยันก่อน (ยังไม่เปลี่ยน)
                ScriptManager.RegisterStartupScript(this, GetType(), "openSwapConfirm",
                    "openSwapConfirm();", true);
                return;
            }

            // ไม่มีรายการ → เปลี่ยนเลย
            DoSwitchToSwap();
        }
        // ── ยืนยันจาก modal → ล้าง + เปลี่ยนเป็น swap ──
        protected void btnConfirmSwap_Click(object sender, EventArgs e)
        {
            DoSwitchToSwap();
        }
        // ── ล้าง Change List + ตั้งโหมด swap ──
        private void DoSwitchToSwap()
        {
            DataTable dt = ChangeListDT;
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                DataRow r = dt.Rows[i];
                if (GetVal(r, "CompareData") == "Delete") continue;
                if (GetVal(r, "CompareData") == "Insert") r.Delete();
                else r["CompareData"] = "Delete";
            }
            dt.AcceptChanges();
            ChangeListDT = dt;
            BindChangeList_Display();

            hfChangeMode.Value = "swap";
            ViewState["CHANGE_MODE"] = "swap";
            UpdateModeButtonUI("swap");

        }

        // ── ตั้งโหมด + จัดการล้าง/เตือน ──
        private void ApplyChangeMode(string newMode)
        {
            string oldMode = hfChangeMode.Value;

            // ✅ กดซ้ำโหมดเดิม → ไม่ทำอะไร
            if (newMode == oldMode)
            {
                UpdateModeButtonUI(newMode);
                return;
            }

            // ✅ Sync ค่าใน GridView กลับ DT ก่อน (กันข้อมูลหายตอน postback)
            SyncGridViewToChangeListDT();

            if (IsPairMode(newMode))
            {
                // ── ไป swap / measure → เตือน + ล้าง Change List ──
                DataTable dt = ChangeListDT;

                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    DataRow r = dt.Rows[i];
                    if (GetVal(r, "CompareData") == "Delete") continue;

                    if (GetVal(r, "CompareData") == "Insert")
                        r.Delete();
                    else
                        r["CompareData"] = "Delete";
                }
                dt.AcceptChanges();

                ChangeListDT = dt;
                BindChangeList_Display();

            }
            else
            {
                // ── ไป change → ไม่ล้าง ──
                BindChangeList_Display();
            }

            hfChangeMode.Value = newMode;
            ViewState["CHANGE_MODE"] = newMode;
            UpdateModeButtonUI(newMode);
        }

        // ── อัปเดตหน้าตาปุ่ม (active/inactive) ──
        private void UpdateModeButtonUI(string mode)
        {
            if (mode == "swap")
            {
                msLabelL.Attributes["class"] = "ms-label ms-left";
                msLabelR.Attributes["class"] = "ms-label ms-right active";
                msTrack.Attributes["class"] = "ms-track mode-swap";
            }
            else if (mode == MODE_MEASURE)
            {
                // ไม่ active ฝั่งใดของสวิตช์ — active ที่ปุ่มเครื่องมือวัดแทน
                msLabelL.Attributes["class"] = "ms-label ms-left";
                msLabelR.Attributes["class"] = "ms-label ms-right";
                msTrack.Attributes["class"] = "ms-track mode-measure";
            }
            else
            {
                msLabelL.Attributes["class"] = "ms-label ms-left active";
                msLabelR.Attributes["class"] = "ms-label ms-right";
                msTrack.Attributes["class"] = "ms-track mode-change";
            }

            // ปุ่ม "แจ้งเปลี่ยนเครื่องมือวัด" : เห็นเฉพาะ Admin (หรือเอกสารที่เป็นโหมดนี้อยู่แล้ว เพื่อให้ผู้ Approve เห็นรูปแบบ)
            msMeasure.Visible = IsAdmin || mode == MODE_MEASURE;
            msMeasure.Attributes["class"] = "ms-measure" + (mode == MODE_MEASURE ? " active" : "");
        }

        // ── กดปุ่ม "แจ้งเปลี่ยนเครื่องมือวัด" (Admin) ──
        protected void btnModeMeasure_Click(object sender, EventArgs e)
        {
            if (!IsAdmin) return;
            if (IsMeasureMode()) { UpdateModeButtonUI(MODE_MEASURE); return; }
            SwitchChangeType(MODE_MEASURE);   // ล้างรายการ + Approver เหมือนการสลับโหมดปกติ
        }

        /// <summary>เมื่อเลือก Use:Dept → ตรวจว่าทุกรายการเป็นกลุ่ม dept เดียวกัน (เตือน+ลงสี)</summary>
        protected void ddlDept_SelectedIndexChanged(object sender, EventArgs e)
        {
            SyncGridViewToChangeListDT();

            // เฉพาะโหมด change
            if (hfChangeMode.Value != "change")
            {
                BindChangeList_Display();
                return;
            }

            // ✅ บังคับ Use:Dept กลุ่มเดียวกันเสมอ (ไม่เช็ค division แล้ว)
            string refUseDept = GetFirstUseDept();
            if (string.IsNullOrEmpty(refUseDept))
            {
                BindChangeList_Display();
                return;
            }

            bool mismatch = false;
            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                string ud = GetVal(r, "Use_DeptCode");
                if (string.IsNullOrEmpty(ud)) continue;
                if (!IsSameDeptGroup(ud, refUseDept))
                {
                    mismatch = true;
                    break;
                }
            }

            BindChangeList_Display();
            AutoGenerateApproverFlow();

            if (mismatch)
            {
                HighlightUseDeptMismatch(refUseDept);
                alert_toast(
                    "Use : Dept ทุกรายการต้องเป็นกลุ่ม Dept เดียวกัน\n[อ้างอิงรายการแรก: " + refUseDept + "]",
                    "Use Dept ไม่ตรงกัน", "warn", "10000");
            }
        }


        #endregion

        #region ===== Gridview =====

        // --- Gridview : gvSearchResult
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

                    // ติด process ใด ๆ (ขึ้นต้น AAD) → disable Add
                    bool locked = displayStatus.StartsWith("AAD");

                    if (locked)
                    {
                        Button btnAdd = e.Row.Cells[1].Controls.OfType<Button>().FirstOrDefault();
                        if (btnAdd != null)
                        {
                            btnAdd.Enabled = false;
                            btnAdd.ToolTip = "อุปกรณ์นี้อยู่ระหว่างดำเนินการ : " + displayStatus;
                            btnAdd.CssClass = "btn btn-secondary btn-sm";
                        }
                    }
                }
            }
        }
        /// <summary>เพิ่มรายการจากผลค้นหา — ตรวจ dept กลุ่มเดียวกัน, จำกัด swap 2 รายการ, auto-swap</summary>
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
                string ds = dtSearch.Rows[rowIndex]["DisplayStatus"].ToString();
                if (ds.StartsWith("AAD"))
                {
                    alert_toast("อุปกรณ์นี้อยู่ระหว่างดำเนินการ : " + ds + "\nไม่สามารถเพิ่มรายการได้",
                                "ไม่สามารถเพิ่มได้", "warn", "10000");
                    return;
                }
            }

            // ── ตรวจซ้ำ (ข้าม row ที่ถูก mark Delete) ──
            foreach (DataRow br in ChangeListDT.Rows)
            {
                if (GetVal(br, "CompareData") == "Delete") continue;

                if (GetVal(br, "ToolId") == toolId &&
                    GetVal(br, "ToolSub") == subId &&
                    GetVal(br, "SystemId") == systemId)
                {
                    alert_toast("มีรายการนี้แล้วในรายการแจ้งเปลี่ยนแปลงแล้ว", "พบรายการซ้ำ", "warn", "10000");
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
                alert_toast("Add List Error : ไม่พบข้อมูลใน Database", "Error", "error", "10000");
                return;
            }

            // ── Dept สำหรับตรวจกลุ่ม : dept_code = 5505 → 5505 (ไม่ใช้ real 5330 / 5390) ──
            string newDept = ApvDept(
                tool.Rows[0]["dept_code"] == DBNull.Value ? "" : tool.Rows[0]["dept_code"].ToString(),
                tool.Rows[0]["dept_code_real"] == DBNull.Value ? "" : tool.Rows[0]["dept_code_real"].ToString());

            if (hfChangeMode.Value == "change")
            {
                string existDept = null;
                foreach (DataRow r in ChangeListDT.Rows)
                {
                    if (GetVal(r, "CompareData") == "Delete") continue;
                    existDept = ApvDept(r);
                    break;
                }
                if (existDept != null && !IsSameDeptGroup(existDept, newDept))
                {
                    alert_toast("ไม่สามารถเพิ่มได้ Current Dept ต้องเป็นกลุ่มเดียวกันทุกรายการ\n" +
                                "[ปัจจุบัน: " + existDept + " , รายการที่เพิ่ม: " + newDept + "]",
                                "Current Dept ไม่ตรงกัน", "warn", "10000");
                    return;
                }
            }
            else if (IsPairMode())
            {
                if (!ValidateSwapAddDept(newDept)) return;
            }

            // ✅ Sync GridView → BorrowListDT ก่อน Add (ไม่ทำ Compare/DB query)
            SyncGridViewToChangeListDT();

            // ── เพิ่ม row ใหม่ ──
            DataTable dt = ChangeListDT;

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
            newRow["DeptCodeReal"] = tool.Rows[0]["dept_code_real"];
            newRow["ModelName"] = tool.Rows[0]["model_name"];
            newRow["ModelId"] = tool.Rows[0]["model_id"];
            newRow["CellName"] = tool.Rows[0]["cell"];
            newRow["CellId"] = tool.Rows[0]["location2_id"];
            newRow["StationName"] = tool.Rows[0]["station"];
            newRow["StationId"] = tool.Rows[0]["station_id"];
            newRow["CompareData"] = "Insert"; // ✅ mark Insert ทันที

            if (IsPairMode())
                newRow["PairKey"] = GetPairKeyForNewRow();

            dt.Rows.Add(newRow);
            ChangeListDT = dt;

            // ✅ โหมด swap : จับคู่ + สลับใหม่ทุกคู่
            if (IsPairMode())
                AutoSwapAllPairs();

            // ✅ measure : คู่ที่เพิ่งครบ ชื่อ Tool No / Model ไม่ตรง → เตือน (ยังทำต่อได้)
            bool warnToolName = false;
            if (IsMeasureMode() && newRow["PairKey"] != DBNull.Value)
            {
                int pk = Convert.ToInt32(newRow["PairKey"]);
                foreach (DataRow r in dt.Rows)
                {
                    if (r == newRow || GetVal(r, "CompareData") == "Delete" || r["PairKey"] == DBNull.Value) continue;
                    if (Convert.ToInt32(r["PairKey"]) == pk)
                    { warnToolName = IsToolNameMismatch(GetVal(r, "ToolNo"), GetVal(newRow, "ToolNo")); break; }
                }
            }

            BindChangeList_Display(); // ✅ แสดงเฉพาะ row ที่ไม่ใช่ Delete
            AutoGenerateApproverFlow();

            txtFixAsset.Text = "";

            // Clear Table Search
            ViewState["SearchResult"] = null;
            gvSearchResult.DataSource = null;
            gvSearchResult.DataBind();

            if (warnToolName)
                alert_toast("Tool No / Model ของคู่นี้ไม่ตรงกัน กรุณาตรวจสอบ\n(สามารถดำเนินการต่อได้)",
                            "ชื่อเครื่องมือวัดไม่ตรงกัน", "warn", "8000");
        }

        // Gridview : gvChangeList
        protected void gvChangeList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //string mode = ViewState["MODE"].ToString();
            string mode = CurrentMode;

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

                // ✅ swap : แสดงการจับคู่
                if (IsPairMode())
                    ApplyPairVisual(e.Row, cRow);

                // ✅ measure : คู่ครบแล้ว Tool No / Model ต้องชื่อเดียวกัน — ไม่ตรง = เตือน (ยังทำต่อได้)
                if (IsMeasureMode())
                    ApplyToolNameWarn(e.Row, cRow);

                // ✅ ผลอนุมัติรายรายการ + ไฮไลต์แถว
                Literal litRes = e.Row.FindControl("litApvResult") as Literal;
                if (litRes != null) litRes.Text = BuildItemResultBadge(cRow);

                string resState = ItemResultState(cRow);
                if (resState == "ng" || resState == "review")
                {
                    string bg = resState == "ng" ? "#fbeceb" : "#fff3cd";
                    string bar = resState == "ng" ? "#c93b3b" : "#d99a06";
                    foreach (TableCell c in e.Row.Cells) c.Style["background-color"] = bg;
                    e.Row.Cells[0].Style["box-shadow"] = "inset 5px 0 0 " + bar;
                }

                e.Row.Attributes["id"] = "row_" + e.Row.RowIndex;

                #region --- Use Dept ---

                DropDownList ddlDept = e.Row.FindControl("ddlDept") as DropDownList;
                if (ddlDept != null)
                {
                    ddlDept.Style["display"] = "inline-block";
                    ddlDept.DataSource = (DataTable)ViewState["DEPT_MASTER"];
                    ddlDept.DataTextField = "dept_name2";
                    ddlDept.DataValueField = "dept_code";
                    ddlDept.DataBind();
                    ddlDept.Items.Insert(0, new ListItem("-- Select --", ""));

                    string v = ColVal(cRow, "Use_DeptCode");
                    // ✅ R25 : Dept ที่ไม่อยู่ใน Master (เช่น 5505 จากคู่สลับ) → เพิ่มให้แสดงได้
                    if (!string.IsNullOrEmpty(v) && ddlDept.Items.FindByValue(v) == null)
                        ddlDept.Items.Add(new ListItem(v, v));
                    if (!string.IsNullOrEmpty(v)) ddlDept.SelectedValue = v;

                    // ✅ 4E-7 : โหมดสลับ ล็อก Use Dept (กำหนดอัตโนมัติจากคู่)
                    if (IsPairMode())
                    {
                        ddlDept.Enabled = false;
                        ddlDept.ToolTip = "โหมดสลับ : Dept ปลายทางกำหนดอัตโนมัติจากคู่สลับ";
                    }
                }

                #endregion

                #region --- Use Model / Cell / Station ---

                BindLocField(e.Row, cRow, "ddlModel", "txtModel", "hfModelMode", "btnModelToggle",
                    "HD_Model", "Use_ModelId", "Add_Model",
                    (DataTable)ViewState["MODEL_MASTER"], "model_name", "model_id", "Model");

                BindLocField(e.Row, cRow, "ddlCell", "txtCell", "hfCellMode", "btnCellToggle",
                    "HD_Cell", "Use_CellId", "Add_Cell",
                    _cellM ?? GetCellMaster(), "location2_name", "location2_id", "Cell");

                BindLocField(e.Row, cRow, "ddlStation", "txtStation", "hfStationMode", "btnStationToggle",
                    "HD_Station", "Use_StationId", "Add_Station",
                    _stationM ?? GetStationMaster(), "location3_name", "location3_id", "Station");

                #endregion

                // ══════════════════════════════════════════════════════
                // ✅ Approve Mode : ปลดล็อคเฉพาะ Use:Model / Cell / Station
                //    (Use:Dept + ที่เหลือคงล็อคจาก MakeRowReadonly)
                // ══════════════════════════════════════════════════════
                if (mode == "approve" && ViewState["APPROVE_EDIT"] != null
                    && ViewState["APPROVE_EDIT"].ToString() == "1")
                {
                    EnableApproveEditControls(e.Row);
                }
            }
        }
        /// <summary>ลบรายการออกจาก Change List</summary>
        protected void gvChangeList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "RemoveItem") return;

            // ✅ Key ของรายการมากับปุ่มเลย (ToolId|ToolSub|SystemId)
            //    เดิมใช้ CommandArgument = ลำดับแถว แล้วไปหา HiddenField ใน gvChangeList.Rows[index]
            //    → ถ้าตารางถูก Bind ใหม่ระหว่าง postback (ลำดับแถวเปลี่ยน / แถวไม่ตรงกับที่กด)
            //      จะหา HiddenField ไม่เจอ = "ไม่พบข้อมูล Hidden Field" หรือเสี่ยงลบผิดแถว
            string toolId, toolSub, systemId;
            if (!TryGetRemoveKey(e, out toolId, out toolSub, out systemId))
            {
                alert_toast("ไม่พบข้อมูลรายการที่ต้องการลบ กรุณาลองใหม่อีกครั้ง", "Remove Error", "error", "10000");
                BindChangeList_Display();   // รีเฟรชตารางให้ตรงกับข้อมูลล่าสุด
                return;
            }

            // ── STEP 1 : Sync GridView → ChangeListDT (ไม่ Compare) ──
            SyncGridViewToChangeListDT();

            DataTable dt = ChangeListDT;

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
                alert_toast("ไม่พบรายการที่ต้องการลบ", "Remove Error", "error", "10000");
                return;
            }

            // ── STEP 3 : ลบ ──
            if (IsPairMode())
            {
                // หาสมาชิกคู่เดียวกันทั้งหมด
                int pk = targetRow["PairKey"] == DBNull.Value ? 0 : Convert.ToInt32(targetRow["PairKey"]);

                var mates = new System.Collections.Generic.List<DataRow>();
                foreach (DataRow r in dt.Rows)
                {
                    if (GetVal(r, "CompareData") == "Delete") continue;
                    int rpk = r["PairKey"] == DBNull.Value ? 0 : Convert.ToInt32(r["PairKey"]);
                    if (rpk == pk) mates.Add(r);
                }

                foreach (DataRow r in mates) RemoveOrMark(r);
                dt.AcceptChanges();

                if (mates.Count >= 2)
                    alert_toast("ลบทั้งคู่เรียบร้อย (" + mates.Count + " รายการ)", "ลบคู่สลับ", "info", "4000");
                else
                    alert_toast("รายการนี้ยังจับคู่ไม่ครบ — ลบรายการเดียว", "ลบรายการ", "info", "4000");
            }
            else
            {
                RemoveOrMark(targetRow);
                dt.AcceptChanges();
            }

            // ── STEP 4 : บันทึกและ Bind ──
            ChangeListDT = dt;

            // swap : re-swap คู่ที่เหลือ (เผื่อค่า Use เพี้ยน)
            if (IsPairMode())
                AutoSwapAllPairs();

            BindChangeList_Display();
            AutoGenerateApproverFlow();
        }

        // ── อ่าน Key ของรายการที่กด Remove ──
        //  1) จาก CommandArgument "ToolId|ToolSub|SystemId" (หลัก)
        //  2) fallback : HiddenField ในแถวของปุ่มที่กดจริง (NamingContainer — ไม่ใช้ลำดับแถว)
        private bool TryGetRemoveKey(GridViewCommandEventArgs e, out string toolId, out string toolSub, out string systemId)
        {
            toolId = toolSub = systemId = null;

            string arg = Convert.ToString(e.CommandArgument);
            string[] parts = arg.Split('|');
            if (parts.Length == 3 && parts[0] != "" && parts[1] != "" && parts[2] != "")
            {
                toolId = parts[0]; toolSub = parts[1]; systemId = parts[2];
                return true;
            }

            Control src = e.CommandSource as Control;
            GridViewRow row = src != null ? src.NamingContainer as GridViewRow : null;
            if (row == null) return false;

            HiddenField hId = row.FindControl("hdtoolId") as HiddenField;
            HiddenField hSub = row.FindControl("hdtoolSub") as HiddenField;
            HiddenField hSys = row.FindControl("hdsystemId") as HiddenField;
            if (hId == null || hSub == null || hSys == null || hId.Value == "") return false;

            toolId = hId.Value; toolSub = hSub.Value; systemId = hSys.Value;
            return true;
        }

        // ── ปลดล็อค Model/Cell/Station (ddl+txt+toggle) สำหรับ Approve Mode ──
        private void EnableApproveEditControls(GridViewRow row)
        {
            // Model
            var ddlModel = row.FindControl("ddlModel") as DropDownList;
            var txtModel = row.FindControl("txtModel") as TextBox;
            if (ddlModel != null) ddlModel.Enabled = true;
            if (txtModel != null) txtModel.Enabled = true;

            // Cell
            var ddlCell = row.FindControl("ddlCell") as DropDownList;
            var txtCell = row.FindControl("txtCell") as TextBox;
            if (ddlCell != null) ddlCell.Enabled = true;
            if (txtCell != null) txtCell.Enabled = true;

            // Station
            var ddlStation = row.FindControl("ddlStation") as DropDownList;
            var txtStation = row.FindControl("txtStation") as TextBox;
            if (ddlStation != null) ddlStation.Enabled = true;
            if (txtStation != null) txtStation.Enabled = true;

            // ปุ่ม toggle ddl↔textbox (HtmlButton) — ปลด Disabled
            EnableHtmlBtn(row, "btnModelToggle");
            EnableHtmlBtn(row, "btnCellToggle");
            EnableHtmlBtn(row, "btnStationToggle");

            // ✅ Use:Dept ยังล็อค (ไม่ปลด ddlDept)
        }

        // ปลด Disabled ปุ่ม toggle (HtmlButton)
        private void EnableHtmlBtn(GridViewRow row, string id)
        {
            var btn = row.FindControl(id) as System.Web.UI.HtmlControls.HtmlButton;
            if (btn != null) btn.Disabled = false;
        }

        #endregion

        #region ===== Tabel Data =====

        // --- Create Change Table
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
            dt.Columns.Add("DeptCodeReal", typeof(string));
            dt.Columns.Add("ModelName", typeof(string));
            dt.Columns.Add("ModelId", typeof(string));
            dt.Columns.Add("CellName", typeof(string));
            dt.Columns.Add("CellId", typeof(string));
            dt.Columns.Add("StationName", typeof(string));
            dt.Columns.Add("StationId", typeof(string));

            // ✅ Approve tracking
            dt.Columns.Add("RejectFlag", typeof(string));   // 'reject'
            dt.Columns.Add("A1Selected", typeof(string));   // Y/N
            dt.Columns.Add("A2Selected", typeof(string));   // Y/N

            dt.Columns.Add("PairKey", typeof(int));   // ✅ ประจำตัวคู่ (swap)

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

            // "1" = คู่นี้สลับค่าแล้ว
            dt.Columns.Add("SwapApplied", typeof(string));

            // ✅ AAD Confirm
            dt.Columns.Add("Use_StatusId", typeof(string));
            dt.Columns.Add("Resp_EmpId", typeof(string));
            dt.Columns.Add("Resp_Name", typeof(string));

            return dt;
        }

        // --- Sync GridView → ChangeListDT เท่านั้น (ไม่ Compare)
        private void SyncGridViewToChangeListDT()
        {
            DataTable ChangeDT = ChangeListDT;

            foreach (GridViewRow gvRow in gvChangeList.Rows)
            {
                if (gvRow.RowType != DataControlRowType.DataRow) continue;

                HiddenField hId = gvRow.FindControl("hdtoolId") as HiddenField;
                HiddenField hSub = gvRow.FindControl("hdtoolSub") as HiddenField;
                HiddenField hSys = gvRow.FindControl("hdsystemId") as HiddenField;

                if (hId == null || hSub == null || hSys == null) continue;

                string key = hId.Value + "|" + hSub.Value + "|" + hSys.Value;

                DataRow dr = null;
                foreach (DataRow r in ChangeDT.Rows)
                {
                    if (GetVal(r, "CompareData") == "Delete") continue;
                    if (GetVal(r, "ToolId") + "|" + GetVal(r, "ToolSub") + "|" + GetVal(r, "SystemId") == key)
                    { dr = r; break; }
                }
                if (dr == null) continue;

                DropDownList ddlDept = gvRow.FindControl("ddlDept") as DropDownList;
                if (ddlDept != null && !IsPairMode())          // ✅ swap : Use Dept มาจากคู่สลับ
                    dr["Use_DeptCode"] = string.IsNullOrEmpty(ddlDept.SelectedValue) ? (object)DBNull.Value : ddlDept.SelectedValue;

                DropDownList ddlModel = gvRow.FindControl("ddlModel") as DropDownList;
                TextBox txtModel = gvRow.FindControl("txtModel") as TextBox;
                HiddenField hfModelMode = gvRow.FindControl("hfModelMode") as HiddenField;
                if (hfModelMode != null)
                    dr["HD_Model"] = string.IsNullOrEmpty(hfModelMode.Value) ? (object)DBNull.Value : hfModelMode.Value;
                if (ddlModel != null)
                    dr["Use_ModelId"] = string.IsNullOrEmpty(ddlModel.SelectedValue) ? (object)DBNull.Value : ddlModel.SelectedValue;
                if (txtModel != null)
                    dr["Add_Model"] = string.IsNullOrEmpty(txtModel.Text) ? (object)DBNull.Value : txtModel.Text;

                DropDownList ddlCell = gvRow.FindControl("ddlCell") as DropDownList;
                TextBox txtCell = gvRow.FindControl("txtCell") as TextBox;
                HiddenField hfCellMode = gvRow.FindControl("hfCellMode") as HiddenField;
                if (hfCellMode != null)
                    dr["HD_Cell"] = string.IsNullOrEmpty(hfCellMode.Value) ? (object)DBNull.Value : hfCellMode.Value;
                if (ddlCell != null)
                    dr["Use_CellId"] = string.IsNullOrEmpty(ddlCell.SelectedValue) ? (object)DBNull.Value : ddlCell.SelectedValue;
                if (txtCell != null)
                    dr["Add_Cell"] = string.IsNullOrEmpty(txtCell.Text) ? (object)DBNull.Value : txtCell.Text;

                DropDownList ddlStation = gvRow.FindControl("ddlStation") as DropDownList;
                TextBox txtStation = gvRow.FindControl("txtStation") as TextBox;
                HiddenField hfStationMode = gvRow.FindControl("hfStationMode") as HiddenField;
                if (hfStationMode != null)
                    dr["HD_Station"] = string.IsNullOrEmpty(hfStationMode.Value) ? (object)DBNull.Value : hfStationMode.Value;
                if (ddlStation != null)
                    dr["Use_StationId"] = string.IsNullOrEmpty(ddlStation.SelectedValue) ? (object)DBNull.Value : ddlStation.SelectedValue;
                if (txtStation != null)
                    dr["Add_Station"] = string.IsNullOrEmpty(txtStation.Text) ? (object)DBNull.Value : txtStation.Text;
            }

            ChangeListDT = ChangeDT;
        }

        // --- Save Data on List   
        private void Save_ChangeListDT()
        {
            // ══════════════════════════════════════════════════════════════════════
            // STEP 1 : Sync GridView → ChangeListDT
            // ══════════════════════════════════════════════════════════════════════
            SyncGridViewToChangeListDT();

            DataTable ChangeDT = ChangeListDT;

            // ══════════════════════════════════════════════════════════════════════
            // STEP 2 : ถ้าไม่มี Doc_No → ทุก row เป็น Insert
            // ══════════════════════════════════════════════════════════════════════
            string docNo = litIssueNo.Text.Trim();

            if (docNo == "-" || string.IsNullOrEmpty(docNo))
            {
                foreach (DataRow r in ChangeDT.Rows)
                {
                    string cmp = GetVal(r, "CompareData");
                    // ✅ ไม่ overwrite row ที่ mark Delete อยู่แล้ว
                    if (cmp != "Delete")
                        r["CompareData"] = "Insert";
                }

                ChangeListDT = ChangeDT;
                return;
            }

            // ══════════════════════════════════════════════════════════════════════
            // STEP 3 : Query DB
            // ══════════════════════════════════════════════════════════════════════
            string strDB =
                "SELECT tool_id, sub_id, system_id," +
                "       Use_Dept, " +
                "       Use_Model_Id, New_Model, HD_Model," +
                "       Use_Cell_Id, New_Cell, HD_Cell," +
                "       Use_Station_Id, New_Station, HD_Station" +
                " FROM  NotificationofChange_List" +
                " WHERE Doc_No = N'" + docNo + "'";

            dc.QryDT(conn, strDB);
            DataTable dtDB = dc.dt.Copy();

            // ══════════════════════════════════════════════════════════════════════
            // STEP 4 : เก็บ Delete keys จาก ChangeListDT ก่อน loop
            //          กัน DB loop เพิ่ม row ซ้ำ
            // ══════════════════════════════════════════════════════════════════════
            var deleteKeySet = new System.Collections.Generic.HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (DataRow r in ChangeDT.Rows)
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
            // STEP 5 : Compare dtDB ↔ ChangeListDT
            // ══════════════════════════════════════════════════════════════════════
            var matchedBorrowIdx = new System.Collections.Generic.HashSet<int>();

            foreach (DataRow dbRow in dtDB.Rows)
            {
                string db_toolId = GetVal(dbRow, "tool_id");
                string db_subId = GetVal(dbRow, "sub_id");
                string db_sysId = GetVal(dbRow, "system_id");
                string db_statusId = GetVal(dbRow, "status_id");
                string dbKey = db_toolId + "|" + db_subId + "|" + db_sysId;

                // ✅ ถ้า key นี้ถูก mark Delete ไว้แล้ว → ข้ามเลย ไม่เพิ่มซ้ำ
                if (deleteKeySet.Contains(dbKey)) continue;

                DataRow matchRow = null;
                int matchIdx = -1;

                for (int i = 0; i < ChangeDT.Rows.Count; i++)
                {
                    DataRow br = ChangeDT.Rows[i];
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
                    // ── อยู่ใน DB แต่ไม่มีใน ChangeListDT → Delete ────────────────
                    DataRow delRow = ChangeDT.NewRow();
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
                    ChangeDT.Rows.Add(delRow);
                }
                else
                {
                    matchedBorrowIdx.Add(matchIdx);

                    bool changed =
                        GetVal(matchRow, "Use_Dept") != GetVal(dbRow, "status_id") ||
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

            // ── มีใน ChangeListDT แต่ไม่มีใน DB → Insert ────────────────────────
            for (int i = 0; i < ChangeDT.Rows.Count; i++)
            {
                if (matchedBorrowIdx.Contains(i)) continue;

                string cmp = GetVal(ChangeDT.Rows[i], "CompareData");

                // ✅ ไม่ overwrite Delete row
                if (cmp != "Delete" && cmp != "Same" && cmp != "Update")
                    ChangeDT.Rows[i]["CompareData"] = "Insert";
            }

            ChangeListDT = ChangeDT;

        }

        // ══════════════════════════════════════════════════════════════
        // Auto-Swap : เอา Current ของอีกตัวมาเป็น Use ของตัวเอง (โหมด swap)
        // ══════════════════════════════════════════════════════════════
        private void AutoSwapLocation()
        {
            DataTable dt = ChangeListDT;

            // หา 2 row ที่ active (ไม่ใช่ Delete)
            var rows = new System.Collections.Generic.List<DataRow>();
            foreach (DataRow r in dt.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                rows.Add(r);
            }

            if (rows.Count != 2) return;

            DataRow A = rows[0];
            DataRow B = rows[1];

            // เตรียม master สำหรับ match id (Dept ใช้ code, ที่เหลือใช้ id)
            DataTable deptM = (DataTable)ViewState["DEPT_MASTER"];
            DataTable modelM = (DataTable)ViewState["MODEL_MASTER"];
            DataTable cellM = GetCellMaster();
            DataTable stationM = GetStationMaster();

            // A ← Current ของ B
            ApplySwapToRow(A, B, deptM, modelM, cellM, stationM);
            // B ← Current ของ A
            ApplySwapToRow(B, A, deptM, modelM, cellM, stationM);

            // mark ให้ save รู้ว่ามีการเปลี่ยน (row ที่มาจาก DB ต้องเป็น Update)
            MarkSwapCompare(A);
            MarkSwapCompare(B);

            ChangeListDT = dt;
        }

        // เอา Current ของ src มาเป็น Use ของ target
        private void ApplySwapToRow(DataRow target, DataRow src,
                                    DataTable deptM, DataTable modelM, DataTable cellM, DataTable stationM)
        {
            // ---------- DEPT ----------
            // Current dept ของ src เก็บใน DeptCode (code) → match dept_code ใน master
            string srcDeptCode = GetVal(src, "DeptCode");
            if (MasterHasValue(deptM, "dept_code", srcDeptCode))
                target["Use_DeptCode"] = srcDeptCode;
            else
                target["Use_DeptCode"] = srcDeptCode; // Dept ไม่มี textbox mode → ใส่ code ตรงๆ

            // ---------- MODEL ----------
            string srcModelId = GetVal(src, "ModelId");
            string srcModelName = GetVal(src, "ModelName");
            if (!string.IsNullOrEmpty(srcModelId) && MasterHasValue(modelM, "model_id", srcModelId))
            {
                target["HD_Model"] = "ddl";
                target["Use_ModelId"] = srcModelId;
                target["Add_Model"] = DBNull.Value;
            }
            else
            {
                target["HD_Model"] = "txt";
                target["Add_Model"] = srcModelName;
                target["Use_ModelId"] = DBNull.Value;
            }

            // ---------- CELL ----------
            string srcCellId = GetVal(src, "CellId");
            string srcCellName = GetVal(src, "CellName");
            if (!string.IsNullOrEmpty(srcCellId) && MasterHasValue(cellM, "location2_id", srcCellId))
            {
                target["HD_Cell"] = "ddl";
                target["Use_CellId"] = srcCellId;
                target["Add_Cell"] = DBNull.Value;
            }
            else
            {
                target["HD_Cell"] = "txt";
                target["Add_Cell"] = srcCellName;
                target["Use_CellId"] = DBNull.Value;
            }

            // ---------- STATION ----------
            string srcStationId = GetVal(src, "StationId");
            string srcStationName = GetVal(src, "StationName");
            if (!string.IsNullOrEmpty(srcStationId) && MasterHasValue(stationM, "location3_id", srcStationId))
            {
                target["HD_Station"] = "ddl";
                target["Use_StationId"] = srcStationId;
                target["Add_Station"] = DBNull.Value;
            }
            else
            {
                target["HD_Station"] = "txt";
                target["Add_Station"] = srcStationName;
                target["Use_StationId"] = DBNull.Value;
            }
        }

        // ตรวจว่า master มีค่านี้ไหม
        private bool MasterHasValue(DataTable master, string col, string val)
        {
            if (master == null || string.IsNullOrEmpty(val)) return false;
            if (!master.Columns.Contains(col)) return false;

            foreach (DataRow r in master.Rows)
            {
                if (r[col] == DBNull.Value) continue;
                if (r[col].ToString().Trim() == val.Trim()) return true;
            }
            return false;
        }

        // mark CompareData หลัง swap : Insert คงเป็น Insert, ที่เหลือ→Update
        private void MarkSwapCompare(DataRow r)
        {
            string cmp = GetVal(r, "CompareData");
            if (cmp == "Insert") return;      // ยังไม่มีใน DB → คง Insert
            r["CompareData"] = "Update";
        }

        // ── active rows ตามลำดับ (ข้าม Delete) ──
        private System.Collections.Generic.List<DataRow> GetActiveOrdered()
        {
            var list = new System.Collections.Generic.List<DataRow>();
            foreach (DataRow r in ChangeListDT.Rows)
                if (GetVal(r, "CompareData") != "Delete") list.Add(r);
            return list;
        }

        // ── validate Dept ตอน Add (swap) ──
        private bool ValidateSwapAddDept(string newDept)
        {
            var groups = new System.Collections.Generic.List<string>();
            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                AddDeptGroupDistinct(groups, ApvDept(r));
            }

            bool inGroup = false;
            foreach (string g in groups)
                if (IsSameDeptGroup(g, newDept)) { inGroup = true; break; }

            if (!inGroup && groups.Count >= 2)
            {
                alert_toast("โหมด '" + PairModeLabel() + "' มีได้ไม่เกิน 2 Dept\n[รายการที่เพิ่ม: " + newDept + " เป็น Dept ที่ 3]",
                            "เกิน 2 Dept", "warn", "10000");
                return false;
            }
            return true;   // ✅ คู่สลับเป็น Dept เดียวกันได้
        }

        // ── สลับทุกคู่ (0,1)(2,3)... ──
        private void AutoSwapAllPairs()
        {
            DataTable dt = ChangeListDT;
            DataTable deptM = (DataTable)ViewState["DEPT_MASTER"];
            DataTable modelM = (DataTable)ViewState["MODEL_MASTER"];
            DataTable cellM = GetCellMaster();
            DataTable stationM = GetStationMaster();

            var map = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<DataRow>>();
            foreach (DataRow r in dt.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                if (r["PairKey"] == DBNull.Value) continue;
                int pk = Convert.ToInt32(r["PairKey"]);
                if (!map.ContainsKey(pk)) map[pk] = new System.Collections.Generic.List<DataRow>();
                map[pk].Add(r);
            }

            if (!dt.Columns.Contains("SwapApplied")) dt.Columns.Add("SwapApplied", typeof(string));

            foreach (var kv in map)
            {
                var m = kv.Value;
                if (m.Count == 2)
                {
                    // ✅ คู่ที่สลับแล้ว → ไม่เขียนทับ (ค่าที่ผู้ใช้แก้ไว้ยังอยู่)
                    if (GetVal(m[0], "SwapApplied") == "1" && GetVal(m[1], "SwapApplied") == "1") continue;

                    ApplySwapToRow(m[0], m[1], deptM, modelM, cellM, stationM);
                    ApplySwapToRow(m[1], m[0], deptM, modelM, cellM, stationM);
                    MarkSwapCompare(m[0]);
                    MarkSwapCompare(m[1]);
                    m[0]["SwapApplied"] = "1";
                    m[1]["SwapApplied"] = "1";
                }
                else if (m.Count == 1)
                {
                    ClearUseValues(m[0]);
                    m[0]["SwapApplied"] = DBNull.Value;
                }
            }

            ChangeListDT = dt;
        }

        // ── เคลียร์ค่า Use (สำหรับตัวที่ยังจับคู่ไม่ครบ) ──
        private void ClearUseValues(DataRow r)
        {
            r["Use_DeptCode"] = DBNull.Value;
            r["HD_Model"] = "ddl"; r["Use_ModelId"] = DBNull.Value; r["Add_Model"] = DBNull.Value;
            r["HD_Cell"] = "ddl"; r["Use_CellId"] = DBNull.Value; r["Add_Cell"] = DBNull.Value;
            r["HD_Station"] = "ddl"; r["Use_StationId"] = DBNull.Value; r["Add_Station"] = DBNull.Value;
            MarkSwapCompare(r);
        }

        // ── แสดงการจับคู่ในตาราง (เรียกจาก RowDataBound) ──
        private void ApplyPairVisual(GridViewRow row, DataRow cRow)
        {
            if (!cRow.Table.Columns.Contains("PairDisplayNo")) return;

            int pairNo = cRow["PairDisplayNo"] == DBNull.Value ? 0 : Convert.ToInt32(cRow["PairDisplayNo"]);
            string slot = ColVal(cRow, "PairSlot");
            bool invalid = cRow["PairInvalid"] != DBNull.Value && Convert.ToBoolean(cRow["PairInvalid"]);
            bool isFirst = (slot == "A");

            string bg = (pairNo % 2 == 1) ? "#eef6ff" : "#fff5e6";
            foreach (TableCell c in row.Cells) c.Style["background-color"] = bg;

            if (isFirst && row.RowIndex > 0)
                row.Style["border-top"] = "3px solid #1a3a5c";

            row.Cells[0].Text =
                "<div style='font-weight:700;'>คู่ " + pairNo + "</div>" +
                "<div style='font-size:11px;color:#555;'>(" + slot + ")</div>";

            if (invalid)
            {
                foreach (TableCell c in row.Cells) c.Style["background-color"] = "#ffe5e5";
                row.Style["border-left"] = "6px solid #dc3545";
            }
        }

        // ── measure : เทียบชื่อ Tool No / Model ของคู่ (ไม่สนตัวพิมพ์เล็ก-ใหญ่ / ช่องว่างซ้ำ) ──
        private static string NormToolName(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            return string.Join(" ", s.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
        }
        private static bool IsToolNameMismatch(string a, string b)
        {
            return NormToolName(a) != NormToolName(b);
        }

        // ── measure : แสดง Text เตือนในช่อง Tool No / Model เมื่อชื่อไม่ตรงกับคู่ (ไม่ block การทำงาน) ──
        private void ApplyToolNameWarn(GridViewRow row, DataRow cRow)
        {
            if (!cRow.Table.Columns.Contains("PairPartnerToolNo")) return;
            if (cRow["PairPartnerToolNo"] == DBNull.Value) return;           // คู่ยังไม่ครบ → ยังไม่เทียบ

            string mine = ColVal(cRow, "ToolNo");
            string partner = cRow["PairPartnerToolNo"].ToString();
            if (!IsToolNameMismatch(mine, partner)) return;

            int idx = -1;
            for (int i = 0; i < gvChangeList.Columns.Count; i++)
            {
                BoundField bf = gvChangeList.Columns[i] as BoundField;
                if (bf != null && bf.DataField == "ToolNo") { idx = i; break; }
            }
            if (idx < 0 || idx >= row.Cells.Count) return;

            TableCell cell = row.Cells[idx];
            cell.Text = Server.HtmlEncode(mine) +
                "<div class='toolname-warn'><span class='glyphicon glyphicon-warning-sign'></span> " +
                "ชื่อไม่ตรงกับคู่ : " + Server.HtmlEncode(string.IsNullOrEmpty(partner) ? "-" : partner) + "</div>";
            cell.ToolTip = "Tool No / Model ไม่ตรงกับคู่ — ตรวจสอบก่อนดำเนินการ (ยังสามารถทำต่อได้)";
        }

        // ── measure : จำนวนคู่ที่ชื่อ Tool No / Model ไม่ตรงกัน ──
        private int CountToolNameMismatchPairs()
        {
            var map = new Dictionary<int, List<DataRow>>();
            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete" || r["PairKey"] == DBNull.Value) continue;
                int pk = Convert.ToInt32(r["PairKey"]);
                if (!map.ContainsKey(pk)) map[pk] = new List<DataRow>();
                map[pk].Add(r);
            }
            int cnt = 0;
            foreach (var m in map.Values)
                if (m.Count == 2 && IsToolNameMismatch(GetVal(m[0], "ToolNo"), GetVal(m[1], "ToolNo"))) cnt++;
            return cnt;
        }

        // ── validate ก่อน Send (swap) ──
        private bool ValidateSwapPairs()
        {
            var keys = new System.Collections.Generic.List<int>();
            var map = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<DataRow>>();
            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                int pk = r["PairKey"] == DBNull.Value ? 0 : Convert.ToInt32(r["PairKey"]);
                if (!map.ContainsKey(pk)) { map[pk] = new System.Collections.Generic.List<DataRow>(); keys.Add(pk); }
                map[pk].Add(r);
            }

            if (map.Count == 0)
            { alert_toast("โหมด '" + PairModeLabel() + "' ต้องมีอย่างน้อย 1 คู่ (2 รายการ)", "ข้อมูลไม่ครบ", "warn", "10000"); return false; }

            keys.Sort();
            int dispPair = 1;
            var groups = new System.Collections.Generic.List<string>();

            foreach (int pk in keys)
            {
                var m = map[pk];
                if (m.Count != 2)
                { alert_toast("คู่ที่ " + dispPair + " จับคู่ไม่ครบ (ต้องมี 2 รายการ)", "คู่ไม่ครบ", "warn", "10000"); return false; }

                string dA = ApvDept(m[0]);
                string dB = ApvDept(m[1]);

                AddDeptGroupDistinct(groups, dA);
                AddDeptGroupDistinct(groups, dB);
                dispPair++;
            }

            if (groups.Count < 1 || groups.Count > 2)
            {
                alert_toast("โหมด '" + PairModeLabel() + "' มีได้ไม่เกิน 2 Dept [พบ " + groups.Count + " Dept]",
                            "จำนวน Dept ไม่ถูกต้อง", "warn", "10000");
                return false;
            }

            return true;
        }

        #endregion

        #region ===== Data Update =====

        /// <summary>บันทึกเอกสาร (ใช้กับ Draft status=1) — UPDATE ถ้ามี Doc_No / INSERT ถ้าใหม่</summary>
        protected void saveData(string status_id)
        {
            DataTable dt = ChangeListDT;
            string Doc_No = litIssueNo.Text.Trim();

            int reason_id = 0;
            string comment_text = txt_ChangeComment.Text.Trim();
            string phone = txtTel.Text.Trim();

            DateTime issue_date = Convert.ToDateTime(txtIssueDate.Text);

            if (ddlReason_Change.SelectedIndex > 0)
                int.TryParse(ddlReason_Change.SelectedValue, out reason_id);

            // ════════════════════════════════════════════════════════════════
            // UPDATE : มี Doc_No อยู่แล้ว
            // ════════════════════════════════════════════════════════════════
            if (Doc_No != "-" && !string.IsNullOrEmpty(Doc_No))
            {
                Save_ChangeListDT(); // Update Data on Table : BorrowListDT

                // -- ตรวจสอบรายการ "กำลังยืม"
                if (duringBorrowCheck())
                {
                    BindChangeList();
                    return;            // หรือ return ""; ในฟังก์ชันที่คืนค่า string
                }

                // ── Update header status ──
                conn.Open();
                try
                {
                    // -- Click Draft button
                    if (status_id == "1")
                    {
                        string sqlHeader =
                            "UPDATE NotificationofChange_Control_List SET status_id = @status_id , issue_date = @issue_date , " +
                            " phone = @phone , change_type = @change_type , " +
                            " reason_id = @reason_id , reason_remark = @reason_remark , update_date = @update_date " +
                            " WHERE Doc_No = @Doc_No";

                        using (SqlCommand cmd = new SqlCommand(sqlHeader, conn))
                        {
                            cmd.Parameters.AddWithValue("@status_id", status_id);
                            cmd.Parameters.AddWithValue("@issue_date", issue_date);
                            cmd.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                            cmd.Parameters.AddWithValue("@change_type", hfChangeMode.Value);   // ✅ เพิ่ม
                            cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                            cmd.Parameters.AddWithValue("@reason_id", reason_id);
                            cmd.Parameters.AddWithValue("@reason_remark", comment_text);
                            cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // -- Click Send button
                }
                catch (Exception ex)
                {
                    alert_toast("Error :" + ex, "บันทึกไม่สำเร็จ", "error", "10000");
                }
                finally
                {
                    conn.Close();
                }

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

        // ══════════════════════════════════════════════════════════════
        // บันทึก status=30 แล้วคืน Doc_No (ไม่ redirect) — สำหรับ flow modal J3
        // คืน "" ถ้าล้มเหลว
        // ══════════════════════════════════════════════════════════════
        private string saveData_ReturnDoc(string status_id)
        {

            if (CountActiveRows() == 0)
            {
                alert_toast("ไม่พบข้อมูลในรายการแจ้งเปลี่ยนแปลง", "ไม่พบข้อมูลในรายการ", "warn", "10000");
                return "";
            }

            DataTable dt = ChangeListDT;
            string Doc_No = litIssueNo.Text.Trim();

            int reason_id = 0;
            string comment_text = txt_ChangeComment.Text.Trim();
            string phone = txtTel.Text.Trim();
            DateTime issue_date = Convert.ToDateTime(txtIssueDate.Text);
            //string deptApprove = ViewState["DEPT_APPROVE"] != null ? ViewState["DEPT_APPROVE"].ToString() : "";
            //var deptList = ViewState["DEPT_APPROVE_LIST"] as System.Collections.Generic.List<string>
            //   ?? new System.Collections.Generic.List<string>();
            //string deptApprove1 = deptList.Count >= 1 ? deptList[0] : null;
            //string deptApprove2 = deptList.Count >= 2 ? deptList[1] : null;
            var deptApv = ViewState["DEPT_APV"] as DeptApproveResult ?? ResolveDeptApprove();
            string deptApprove1 = deptApv.A1;   // Current side
            string deptApprove2 = deptApv.A2;   // Use side (null ถ้า 1 dept)

            if (ddlReason_Change.SelectedIndex > 0)
                int.TryParse(ddlReason_Change.SelectedValue, out reason_id);

            // ════════ UPDATE : มี Doc_No แล้ว (draft → send) ════════
            if (Doc_No != "-" && !string.IsNullOrEmpty(Doc_No))
            {
                Save_ChangeListDT();

                if (duringBorrowCheck())
                {
                    BindChangeList();
                    return "";            // หรือ return ""; ในฟังก์ชันที่คืนค่า string
                }

                conn.Open();
                try
                {
                    string sqlHeader =
                        "UPDATE NotificationofChange_Control_List SET status_id=@status_id, issue_date=@issue_date, " +
                        " phone=@phone, change_type=@change_type, dept_code_approve=@dept_code_approve, dept_code_approve2=@dept_code_approve2, " +
                        " reason_id=@reason_id, reason_remark=@reason_remark, update_date=@update_date, send_date=@send_date " +
                        " WHERE Doc_No=@Doc_No" +
                        (status_id == "30" ? " AND status_id = 1" : "");      // ✅ ส่งได้เฉพาะเอกสารที่ยังเป็น Draft

                    using (SqlCommand cmd = new SqlCommand(sqlHeader, conn))
                    {
                        cmd.Parameters.AddWithValue("@status_id", status_id);
                        cmd.Parameters.AddWithValue("@issue_date", issue_date);
                        cmd.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                        cmd.Parameters.AddWithValue("@change_type", hfChangeMode.Value);
                        //cmd.Parameters.AddWithValue("@dept_code_approve", string.IsNullOrEmpty(deptApprove) ? (object)DBNull.Value : deptApprove);
                        cmd.Parameters.AddWithValue("@dept_code_approve", (object)deptApprove1 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@dept_code_approve2", (object)deptApprove2 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@reason_id", reason_id);
                        cmd.Parameters.AddWithValue("@reason_remark", comment_text);
                        cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@send_date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                        //cmd.ExecuteNonQuery();
                        int affected = cmd.ExecuteNonQuery();

                        if (status_id == "30" && affected == 0)
                        {
                            alert_toast("เอกสารนี้ถูกส่งไปแล้ว หรือสถานะเปลี่ยนไปแล้ว — ระบบไม่บันทึกซ้ำ",
                                        "ส่งซ้ำ", "warn", "10000");
                            return "";      // finally ยังปิด conn ให้
                        }
                    }
                }
                catch (Exception ex)
                {
                    alert_toast("Error :" + ex.Message, "บันทึกไม่สำเร็จ", "error", "10000");
                    return "";
                }
                finally
                {
                    conn.Close();
                }

                // Sync list แบบไม่ redirect
                bool ok = Sync_ChangeList_NoRedirect(Doc_No, status_id);

                // ✅ บังคับ status ทุก row ให้ตรง header (กัน row "Same" ค้าง status เดิม)
                if (ok)
                {
                    conn.Open();
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(
                            "UPDATE NotificationofChange_List SET status_id=@st WHERE Doc_No=@doc AND status_id<>999", conn))
                        {
                            cmd.Parameters.AddWithValue("@st", status_id);
                            cmd.Parameters.AddWithValue("@doc", Doc_No);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    finally { if (conn.State == ConnectionState.Open) conn.Close(); }
                }

                return ok ? Doc_No : "";
            }

            // ════════ INSERT : เอกสารใหม่ ════════
            else
            {
                return SaveData_InsertBranch_ReturnDoc(status_id);
            }
        }

        // ── บันทึก Approver Flow (ล้างเก่า → insert ใหม่ A1=30, A2=31) ──
        private void SaveApproverFlow(string Doc_No)
        {
            var deptList = ViewState["DEPT_APPROVE_LIST"] as System.Collections.Generic.List<string>
                           ?? new System.Collections.Generic.List<string>();
            if (deptList.Count == 0) return;

            var empL = SplitIds(hfJ3SelectedL.Value);
            var empR = SplitIds(hfJ3SelectedR.Value);

            conn.Open();
            try
            {
                using (SqlCommand del = new SqlCommand(
                    "DELETE FROM NotificationofChange_Approver WHERE Doc_No=@d", conn))
                { del.Parameters.AddWithValue("@d", Doc_No); del.ExecuteNonQuery(); }

                InsertApproverGroup(Doc_No, deptList[0], empL, "30");
                if (deptList.Count >= 2)
                    InsertApproverGroup(Doc_No, deptList[1], empR, "31");
            }
            catch (Exception ex)
            {
                alert_toast("บันทึก Approver ผิดพลาด : " + ex.Message, "Error", "error", "10000");
            }
            finally { conn.Close(); }
        }

        // --- SaveData_InsertBranch() — transaction-wrapped + retry
        private void SaveData_InsertBranch(string status_id)
        {
            // ---- เตรียมข้อมูล ทำครั้งเดียว ก่อนเข้าลูป retry ----
            Save_ChangeListDT();

            if (duringBorrowCheck())
            {
                BindChangeList();
                return;            // หรือ return ""; ในฟังก์ชันที่คืนค่า string
            }

            DataTable dt = ChangeListDT;
            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูลในรายการแจ้งเปลี่ยนแปลง", "ข้อมูลไม่ครบ", "warn", "10000");
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
                            Doc_No = "AAD2-" + issueYear_2 + "-" + issueNo.ToString("00000");

                            // 2) INSERT header (ถ้า Doc_No ซ้ำ → throw 2627 ที่นี่)
                            InsertControlHeader_Tx(c, tx, issueNo, Doc_No, status_id);

                            // 3) INSERT รายการ Withdraw_List ทั้งหมด (ใน tx เดียวกัน)
                            InsertChangeRows_Tx(c, tx, dt, Doc_No, status_id);

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
                                        "บันทึกไม่สำเร็จ", "error", "12000");
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
        // ══════════════════════════════════════════════════════════════
        // INSERT เอกสารใหม่ (transaction + retry) — คืน Doc_No, ไม่ redirect
        // ══════════════════════════════════════════════════════════════
        private string SaveData_InsertBranch_ReturnDoc(string status_id)
        {
            Save_ChangeListDT();

            if (duringBorrowCheck())
            {
                BindChangeList();
                return "";            // หรือ return ""; ในฟังก์ชันที่คืนค่า string
            }

            DataTable dt = ChangeListDT;
            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูลในรายการแจ้งเปลี่ยนแปลง", "ข้อมูลไม่ครบ", "warn", "10000");
                return "";
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
                            int issueNo = GetNextIssueNo_Tx(c, tx, issueYear);
                            Doc_No = "AAD2-" + issueYear_2 + "-" + issueNo.ToString("00000");

                            InsertControlHeader_Tx(c, tx, issueNo, Doc_No, status_id);
                            InsertChangeRows_Tx(c, tx, dt, Doc_No, status_id);

                            tx.Commit();
                            success = true;
                            break;
                        }
                        catch (SqlException ex)
                        {
                            try { tx.Rollback(); } catch { }
                            if (IsDuplicateKeyError(ex))
                            {
                                if (attempt == MAX_RETRY)
                                {
                                    alert_toast("ระบบไม่ว่าง มีการบันทึกพร้อมกัน กรุณาลองใหม่", "บันทึกไม่สำเร็จ", "error", "10000");
                                    return "";
                                }
                                continue;
                            }
                            alert_toast("เกิดข้อผิดพลาด : " + ex.Message, "Error", "error", "10000");
                            return "";
                        }
                        catch (Exception ex)
                        {
                            try { tx.Rollback(); } catch { }
                            alert_toast("เกิดข้อผิดพลาด : " + ex.Message, "Error", "error", "10000");
                            return "";
                        }
                    }
                }
            }

            return success ? Doc_No : "";
        }

        // --- INSERT header ภายใน transaction ปล่อย SqlException หลุดออกไปให้ลูป retry จับ(ไม่ try/catch ที่นี่)
        private void InsertControlHeader_Tx(SqlConnection c, SqlTransaction tx, int issueNo, string Doc_No, string status_id)
        {
            string issue_empid = txtEmpId.Text;
            string dept_code = txtDeptCode.Text;
            string phone = txtTel.Text.Trim();
            int reason_id = 0;
            string comment_text = txt_ChangeComment.Text.Trim();

            if (ddlReason_Change.SelectedIndex > 0)
                int.TryParse(ddlReason_Change.SelectedValue, out reason_id);

            var deptApv = ViewState["DEPT_APV"] as DeptApproveResult ?? ResolveDeptApprove();
            string deptApprove1 = deptApv.A1;
            string deptApprove2 = deptApv.A2;

            string sql;
            if (status_id == "1")
            {
                // ✅ เพิ่ม dept_code_approve / dept_code_approve2
                sql = @"INSERT INTO NotificationofChange_Control_List
                        ( issue_no, issue_date, Doc_No, status_id, issue_empid,
                          dept_code, phone, change_type, dept_code_approve, dept_code_approve2,
                          reason_id, reason_remark, update_date )
                        VALUES
                        ( @issue_no, @issue_date, @Doc_No, @status_id, @issue_empid,
                          @dept_code, @phone, @change_type, @dept_code_approve, @dept_code_approve2,
                          @reason_id, @reason_remark, @update_date )";
            }
            else if (status_id == "30")
            {
                sql = @"INSERT INTO NotificationofChange_Control_List
                        ( issue_no, issue_date, Doc_No, status_id, issue_empid,
                          dept_code, phone, change_type, dept_code_approve, dept_code_approve2,
                          reason_id, reason_remark, update_date, send_date )
                        VALUES
                        ( @issue_no, @issue_date, @Doc_No, @status_id, @issue_empid,
                          @dept_code, @phone, @change_type, @dept_code_approve, @dept_code_approve2,
                          @reason_id, @reason_remark, @update_date, @send_date )";
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
                cmd.Parameters.AddWithValue("@phone",
                    string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                cmd.Parameters.AddWithValue("@change_type", hfChangeMode.Value);

                // ✅ ผูกทั้ง 2 case เสมอ (เดิมอยู่ใน if 30 เท่านั้น)
                cmd.Parameters.AddWithValue("@dept_code_approve", (object)deptApprove1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dept_code_approve2", (object)deptApprove2 ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@reason_id", reason_id);
                cmd.Parameters.AddWithValue("@reason_remark", comment_text);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);

                if (status_id == "30")
                    cmd.Parameters.AddWithValue("@send_date", DateTime.Now);

                cmd.ExecuteNonQuery();
            }
        }

        // --- INSERT รายการ Borrow_List ทั้งหมด ภายใน transaction ปล่อย exception หลุดให้ caller rollback(กัน half-document)
        private void InsertChangeRows_Tx(SqlConnection c, SqlTransaction tx, DataTable dt, string Doc_No, string status_id)
        {
            string sql = @"
                INSERT INTO NotificationofChange_List
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

        // --- ตรวจสอบ Withdraw Table (Database) กับ BorrowList Table -> และ Update ลง Database
        private void Sync_BorrowList(string Doc_No, string status_id)
        {
            DataTable dt = ChangeListDT;

            // --- ตรวจสอบการลงข้อมูล
            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("ไม่มีข้อมูลใน ChangeListDT", "Sync Error", "error", "5000");
                return;
            }

            // ── Renumber RowNo เฉพาะ row ที่ไม่ใช่ Delete ────────────────────────
            int no = 1;
            foreach (DataRow r in dt.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                r["RowNo"] = no++;
            }

            ChangeListDT = dt;

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
                                    INSERT INTO NotificationofChange_List
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
                                    UPDATE NotificationofChange_List SET
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
                                        DELETE FROM NotificationofChange_List
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
                        case "Same":
                        {
                            // ข้อมูลไม่เปลี่ยน แต่ต้องอัปเดต No (เรียงใหม่หลัง Remove) + status ตาม header
                            string sql = @"UPDATE NotificationofChange_List
                                SET No=@No, status_id=@status_id, update_date=@update_date
                                WHERE Doc_No=@Doc_No AND tool_id=@tool_id AND sub_id=@sub_id AND system_id=@system_id";
                            using (SqlCommand cmd = new SqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                                cmd.Parameters.AddWithValue("@No", ToDbVal(row, "RowNo"));
                                cmd.Parameters.AddWithValue("@tool_id", ToDbVal(row, "ToolId"));
                                cmd.Parameters.AddWithValue("@sub_id", ToDbVal(row, "ToolSub"));
                                cmd.Parameters.AddWithValue("@system_id", ToDbVal(row, "SystemId"));
                                cmd.Parameters.AddWithValue("@status_id", status_id);
                                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                                cmd.ExecuteNonQuery();
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
                        case "30": alert_info = "Send Complete"; break;
                        case "50": alert_info = "Approve Complete"; break;
                        case "99": alert_info = "Confirm Complete"; break;
                        case "100": alert_info = "Update Complete"; break;
                        case "999": alert_info = "Cancel Request"; break;
                    }

                    alert_toast("Doc No : " + Doc_No, alert_info, "info", "10000");

                    string script =
                        "lockUI();" +
                        "setTimeout(function() {" +
                        "  window.location.href = 'EquipmentStatus2.aspx';" +
                        "}, 2000);";

                    ScriptManager.RegisterStartupScript(
                        this, this.GetType(), "redirectScript", script, true);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════
        // เหมือน Sync_BorrowList แต่ไม่ alert/redirect — คืน true ถ้าสำเร็จ
        // ══════════════════════════════════════════════════════════════
        private bool Sync_ChangeList_NoRedirect(string Doc_No, string status_id)
        {
            DataTable dt = ChangeListDT;

            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("ไม่มีข้อมูลใน ChangeListDT", "Sync Error", "error", "10000");
                return false;
            }

            // Renumber RowNo (ข้าม Delete)
            int no = 1;
            foreach (DataRow r in dt.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                r["RowNo"] = no++;
            }
            ChangeListDT = dt;

            int cntError = 0;
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
                                INSERT INTO NotificationofChange_List
                                ( Doc_No, No, FixAsset, ControlNo, tool_id, sub_id, system_id, Use_Dept,
                                    Use_Model_Id, New_Model, HD_Model, Use_Cell_Id, New_Cell, HD_Cell,
                                    Use_Station_Id, New_Station, HD_Station, status_id, update_date )
                                VALUES
                                ( @Doc_No, @No, @FixAsset, @ControlNo, @tool_id, @sub_id, @system_id, @Use_Dept,
                                    @Use_Model_Id, @New_Model, @HD_Model, @Use_Cell_Id, @New_Cell, @HD_Cell,
                                    @Use_Station_Id, @New_Station, @HD_Station, @status_id, @update_date )";
                            using (SqlCommand cmd = new SqlCommand(sql, conn))
                            {
                                AddChangeRowParams(cmd, row, Doc_No, status_id);
                                cmd.ExecuteNonQuery();
                            }
                            break;
                        }
                        case "Update":
                        {
                            string sql = @"
                                UPDATE NotificationofChange_List SET
                                    No=@No, Use_Dept=@Use_Dept, Use_Model_Id=@Use_Model_Id, New_Model=@New_Model, HD_Model=@HD_Model,
                                    Use_Cell_Id=@Use_Cell_Id, New_Cell=@New_Cell, HD_Cell=@HD_Cell,
                                    Use_Station_Id=@Use_Station_Id, New_Station=@New_Station, HD_Station=@HD_Station,
                                    status_id=@status_id, update_date=@update_date
                                WHERE Doc_No=@Doc_No AND tool_id=@tool_id AND sub_id=@sub_id AND system_id=@system_id";
                            using (SqlCommand cmd = new SqlCommand(sql, conn))
                            {
                                AddChangeRowParams(cmd, row, Doc_No, status_id);
                                cmd.ExecuteNonQuery();
                            }
                            break;
                        }
                        case "Delete":
                        {
                            string sql = @"
                                DELETE FROM NotificationofChange_List
                                WHERE Doc_No=@Doc_No AND tool_id=@tool_id AND sub_id=@sub_id AND system_id=@system_id";
                            using (SqlCommand cmd = new SqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                                cmd.Parameters.AddWithValue("@tool_id", ToDbVal(row, "ToolId"));
                                cmd.Parameters.AddWithValue("@sub_id", ToDbVal(row, "ToolSub"));
                                cmd.Parameters.AddWithValue("@system_id", ToDbVal(row, "SystemId"));
                                cmd.ExecuteNonQuery();
                            }
                            break;
                        }
                        case "Same":
                        {
                            // ข้อมูลไม่เปลี่ยน แต่ต้องอัปเดต No (เรียงใหม่หลัง Remove) + status ตาม header
                            string sql = @"UPDATE NotificationofChange_List
                                SET No=@No, status_id=@status_id, update_date=@update_date
                                WHERE Doc_No=@Doc_No AND tool_id=@tool_id AND sub_id=@sub_id AND system_id=@system_id";
                            using (SqlCommand cmd = new SqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                                cmd.Parameters.AddWithValue("@No", ToDbVal(row, "RowNo"));
                                cmd.Parameters.AddWithValue("@tool_id", ToDbVal(row, "ToolId"));
                                cmd.Parameters.AddWithValue("@sub_id", ToDbVal(row, "ToolSub"));
                                cmd.Parameters.AddWithValue("@system_id", ToDbVal(row, "SystemId"));
                                cmd.Parameters.AddWithValue("@status_id", status_id);
                                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                                cmd.ExecuteNonQuery();
                            }
                            break;
                        }
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
            }

            return cntError == 0;
        }

        // helper: เติม parameter ครบชุดสำหรับ Insert/Update NotificationofChange_List
        private void AddChangeRowParams(SqlCommand cmd, DataRow row, string Doc_No, string status_id)
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
        }

        #endregion

        #region ===== Bind Table =====

        // --- Data Borrow List
        private void BindChangeList()
        {
            BindChangeList_Display();
        }

        // ✅ Bind GridView เฉพาะ row ที่ user เห็น (ไม่รวม Delete)
        private void BindChangeList_Display()
        {
            _cellM = GetCellMaster().Copy();
            _stationM = GetStationMaster().Copy();

            DataTable dt = ChangeListDT;
            DataTable dtDisplay = dt.Clone();

            bool swap = (IsPairMode());
            if (swap)
            {
                dtDisplay.Columns.Add("PairDisplayNo", typeof(int));
                dtDisplay.Columns.Add("PairSlot", typeof(string));
                dtDisplay.Columns.Add("PairInvalid", typeof(bool));
                dtDisplay.Columns.Add("PairPartnerToolNo", typeof(string));   // measure : Tool No ของคู่ (ไว้เตือนชื่อไม่ตรง)
            }

            var active = new System.Collections.Generic.List<DataRow>();
            foreach (DataRow r in dt.Rows)
                if (GetVal(r, "CompareData") != "Delete") active.Add(r);

            if (swap)
            {
                var keys = new System.Collections.Generic.List<int>();
                var map = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<DataRow>>();
                foreach (DataRow r in active)
                {
                    int pk = r["PairKey"] == DBNull.Value ? 0 : Convert.ToInt32(r["PairKey"]);
                    if (!map.ContainsKey(pk)) { map[pk] = new System.Collections.Generic.List<DataRow>(); keys.Add(pk); }
                    map[pk].Add(r);
                }
                keys.Sort();

                int rowNo = 1, dispPair = 1;
                foreach (int pk in keys)
                {
                    var m = map[pk];
                    bool invalid = (m.Count != 2);
                    //if (m.Count == 2)
                    //    invalid = IsSameDeptGroup(GetVal(m[0], "DeptCodeReal"), GetVal(m[1], "DeptCodeReal"));

                    for (int s = 0; s < m.Count; s++)
                    {
                        DataRow nr = dtDisplay.NewRow();
                        foreach (DataColumn col in dt.Columns)
                            nr[col.ColumnName] = m[s][col.ColumnName];
                        nr["RowNo"] = rowNo++;
                        nr["PairDisplayNo"] = dispPair;
                        nr["PairSlot"] = (s == 0) ? "A" : "B";
                        nr["PairInvalid"] = invalid;
                        if (m.Count == 2)
                            nr["PairPartnerToolNo"] = GetVal(m[1 - s], "ToolNo");
                        dtDisplay.Rows.Add(nr);
                    }
                    dispPair++;
                }
            }
            else
            {
                int rowNo = 1;
                foreach (DataRow r in active)
                {
                    DataRow nr = dtDisplay.NewRow();
                    nr.ItemArray = (object[])r.ItemArray.Clone();
                    nr["RowNo"] = rowNo++;
                    dtDisplay.Rows.Add(nr);
                }
            }

            // ✅ แสดงคอลัมน์ "ผลอนุมัติ" (Columns[2]) เมื่อมีการ Approve แล้ว
            bool hasApv = false;
            foreach (DataRow r in dtDisplay.Rows)
                if (HasApproveData(r)) { hasApv = true; break; }

            gvChangeList.Columns[2].Visible = hasApv;
            litItemResultSummary.Text = hasApv ? BuildItemResultSummary(dtDisplay) : "";            

            gvChangeList.DataSource = dtDisplay;
            gvChangeList.DataBind();

            int activeCnt = 0;
            foreach (DataRow r in ChangeListDT.Rows)
                if (GetVal(r, "CompareData") != "Delete") activeCnt++;
            hfHasItems.Value = activeCnt > 0 ? "1" : "0";
        }

        // ── สร้างตารางสรุป New / Old Location สำหรับ modal ──
        private void BuildSummaryTable()
        {
            DataTable src = ChangeListDT;

            // เตรียม master สำหรับแปลง id → name (New zone)
            DataTable deptM = (DataTable)ViewState["DEPT_MASTER"];
            DataTable modelM = (DataTable)ViewState["MODEL_MASTER"];
            DataTable cellM = GetCellMaster();
            DataTable stationM = GetStationMaster();

            DataTable dt = new DataTable();
            dt.Columns.Add("RowNo", typeof(int));
            dt.Columns.Add("FixAssetNo", typeof(string));
            dt.Columns.Add("ControlNo", typeof(string));
            dt.Columns.Add("ToolNo", typeof(string));
            dt.Columns.Add("OldDept", typeof(string));
            dt.Columns.Add("OldModel", typeof(string));
            dt.Columns.Add("OldCell", typeof(string));
            dt.Columns.Add("OldStation", typeof(string));
            dt.Columns.Add("NewDept", typeof(string));
            dt.Columns.Add("NewModel", typeof(string));
            dt.Columns.Add("NewCell", typeof(string));
            dt.Columns.Add("NewStation", typeof(string));

            int no = 1;
            foreach (DataRow r in src.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;

                DataRow n = dt.NewRow();
                n["RowNo"] = no++;
                n["FixAssetNo"] = GetVal(r, "FixAssetNo");
                n["ControlNo"] = GetVal(r, "ControlNo");
                n["ToolNo"] = GetVal(r, "ToolNo");

                // Old = ค่าปัจจุบัน (แสดง dept แบบมีวงเล็บ)
                n["OldDept"] = FormatDeptDisplay(GetVal(r, "DeptCode"), GetVal(r, "DeptCodeReal"));
                n["OldModel"] = GetVal(r, "ModelName");
                n["OldCell"] = GetVal(r, "CellName");
                n["OldStation"] = GetVal(r, "StationName");

                // New = Use : ถ้า txt → ค่า text / ถ้า ddl → แปลง id เป็น name
                n["NewDept"] = ResolveDeptName(deptM, GetVal(r, "Use_DeptCode"));
                n["NewModel"] = (GetVal(r, "HD_Model") == "txt")
                    ? GetVal(r, "Add_Model")
                    : ResolveName(modelM, "model_id", "model_name", GetVal(r, "Use_ModelId"));
                n["NewCell"] = (GetVal(r, "HD_Cell") == "txt")
                    ? GetVal(r, "Add_Cell")
                    : ResolveName(cellM, "location2_id", "location2_name", GetVal(r, "Use_CellId"));
                n["NewStation"] = (GetVal(r, "HD_Station") == "txt")
                    ? GetVal(r, "Add_Station")
                    : ResolveName(stationM, "location3_id", "location3_name", GetVal(r, "Use_StationId"));

                dt.Rows.Add(n);
            }

            gvSummary.DataSource = dt;
            gvSummary.DataBind();
        }

        // helper: หา name จาก master ทั่วไป
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

        // helper: dept name (dept_name2 มี code อยู่แล้ว)
        private string ResolveDeptName(DataTable deptM, string deptCode)
        {
            if (deptM == null || string.IsNullOrEmpty(deptCode)) return "";
            foreach (DataRow r in deptM.Rows)
            {
                if (r["dept_code"] == DBNull.Value) continue;
                if (r["dept_code"].ToString().Trim() == deptCode.Trim())
                    return r["dept_name2"] == DBNull.Value ? deptCode : r["dept_name2"].ToString().Trim();
            }
            return deptCode;
        }

        #endregion

        #region --- Dept Code Approve ---
        // ══════════════════════════════════════════════════════════════
        // หา Dept Approve แยก side (A1=Current, A2=Use)
        // ใช้กฎ Div/5505 เดิม กับ Current-rep และ Use-rep แยกกัน
        // ══════════════════════════════════════════════════════════════
        private DeptApproveResult ResolveDeptApprove()
        {
            var res = new DeptApproveResult();

            var rows = new System.Collections.Generic.List<DataRow>();
            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                rows.Add(r);
            }
            if (rows.Count == 0) return res;

            // ✅ โหมดสลับ : ต้องมี 2 แผนกเสมอ → A1 = กลุ่ม Dept แรก , A2 = อีกกลุ่ม
            if (IsPairMode())
            {
                var groups = new System.Collections.Generic.List<string>();
                foreach (DataRow r in rows) AddDeptGroupDistinct(groups, ApvDept(r));

                // *** 2 แผนก และมี 5505 อยู่ในนั้น → ไม่ต้องให้ 5505 Approve (เหลือแผนกอื่นแผนกเดียว)
                if (groups.Count == 2 && (groups[0] == "5505" || groups[1] == "5505"))
                {
                    res.A1 = (groups[0] == "5505") ? groups[1] : groups[0];
                    return res;
                }

                if (groups.Count >= 1) res.A1 = groups[0];
                if (groups.Count >= 2) res.A2 = groups[1];
                return res;
            }

            // Current-rep / Use-rep (รายการแรก — change บังคับกลุ่มเดียวกันหมดแล้ว, swap ใช้รายการแรก)
            //string curDept = GetVal(rows[0], "DeptCodeReal");
            string curDept = ApvDept(rows[0]);                 // ✅ เดิม : GetVal(rows[0], "DeptCodeReal")
            string useDept = GetVal(rows[0], "Use_DeptCode");

            // ══════════════════════════════════════════════════════════
            // กฎ 1 (priority สูงสุด) : มี 5505 ฝั่งใดฝั่งหนึ่ง → ไม่สน Requester
            // ══════════════════════════════════════════════════════════
            bool curIs5505_top = (curDept == "5505");
            bool useIs5505_top = (useDept == "5505");

            if (curIs5505_top || useIs5505_top)
            {
                // 1.2 : ทั้งสองฝั่ง = 5505 → Approve 5505
                if (curIs5505_top && useIs5505_top)
                {
                    res.A1 = "5505";
                    return res;
                }

                // 1.1 : ฝั่งเดียว = 5505 → Approve dept ที่ไม่ใช่ 5505 (1 dept)
                res.A1 = curIs5505_top ? useDept : curDept;
                return res;
            }

            // Dept ของ Requester
            string reqDept = txtDeptCode.Text.Trim();

            string curDiv = GetDeptDivision(curDept);
            string useDiv = GetDeptDivision(useDept);
            string reqDiv = GetDeptDivision(reqDept);

            // ══════════════════════════════════════════════════════════
            // 2.1 : Requester + Use + Current เป็น Division=2 ทั้งหมด
            // ══════════════════════════════════════════════════════════
            if (reqDiv == "2" && curDiv == "2" && useDiv == "2")
            {
                bool reqEqCur = IsSameDeptGroup(reqDept, curDept);
                bool reqEqUse = IsSameDeptGroup(reqDept, useDept);

                // 2.1.1 : Requester == Use หรือ == Current → Approve dept ที่ "ไม่ใช่" ของ Requester
                if (reqEqCur || reqEqUse)
                {
                    // dept ที่เหลือ (ไม่ใช่กลุ่ม Requester)
                    string other = reqEqCur ? useDept : curDept;

                    // ถ้าอีกฝั่งก็เป็นกลุ่มเดียวกับ Requester → ไม่มีใครต้อง approve นอกกลุ่ม
                    if (IsSameDeptGroup(other, reqDept))
                    {
                        // ทั้ง Current+Use = Requester → approve ตัวเอง 1 dept
                        res.A1 = reqDept;
                    }
                    else
                    {
                        res.A1 = other;
                    }
                    return res;
                }

                // 2.1.2 : Requester != Use และ != Current → 2 dept (A1=Current, A2=Use)
                res.A1 = curDept;
                res.A2 = useDept;

                // ถ้า Current/Use กลุ่มเดียวกัน → เหลือ dept เดียว
                if (IsSameDeptGroup(curDept, useDept))
                {
                    res.A1 = curDept;
                    res.A2 = null;
                }
                return res;
            }

            // ══════════════════════════════════════════════════════════
            // 2.2 : Requester Division=1 , Use & Current Division=2
            // ══════════════════════════════════════════════════════════
            if (reqDiv == "1" && curDiv == "2" && useDiv == "2")
            {
                //bool curIs5505 = (curDept == "5505");
                //bool useIs5505 = (useDept == "5505");

                //// 2.2.1 : Current หรือ Use = 5505 → Approve dept ที่ไม่ใช่ 5505 (1 dept)
                //if (curIs5505 || useIs5505)
                //{
                //    res.A1 = curIs5505 ? useDept : curDept;
                //    return res;
                //}

                // 2.2.2 : ไม่มี 5505 → 2 dept (A1=Current, A2=Use)
                res.A1 = curDept;
                res.A2 = useDept;

                if (IsSameDeptGroup(curDept, useDept))
                {
                    res.A1 = curDept;
                    res.A2 = null;
                }
                return res;
            }

            // ══════════════════════════════════════════════════════════
            // Fallback : กฎเดิม (Div/5505) — เผื่อเคสอื่นที่ไม่เข้า 2.1 / 2.2
            // ══════════════════════════════════════════════════════════
            var distinct = new System.Collections.Generic.List<string>();
            AddDeptGroupDistinct(distinct, curDept);
            AddDeptGroupDistinct(distinct, useDept);

            int count5505 = 0;
            foreach (string d in distinct) if (d == "5505") count5505++;

            if (count5505 >= 2) { res.A1 = "5505"; return res; }

            var noneFive = new System.Collections.Generic.List<string>();
            foreach (string d in distinct) if (d != "5505") noneFive.Add(d);

            var div2 = new System.Collections.Generic.List<string>();
            foreach (string d in noneFive)
                if (GetDeptDivision(d) == "2") div2.Add(d);

            if (div2.Count == 0)
            {
                res.A1 = "5505";
            }
            else if (div2.Count == 1)
            {
                res.A1 = div2[0];
            }
            else
            {
                res.A1 = MatchSide(div2, curDept);
                res.A2 = MatchSide(div2, useDept);
                if (string.IsNullOrEmpty(res.A1) || string.IsNullOrEmpty(res.A2))
                {
                    res.A1 = div2[0];
                    res.A2 = div2[1];
                }
            }

            return res;
        }

        // เพิ่ม dept ถ้าไม่ซ้ำกลุ่ม
        private void AddDeptGroupDistinct(System.Collections.Generic.List<string> list, string dept)
        {
            dept = (dept ?? "").Trim();
            if (string.IsNullOrEmpty(dept)) return;
            foreach (string x in list)
                if (IsSameDeptGroup(x, dept)) return;   // มีกลุ่มนี้แล้ว
            list.Add(dept);
        }

        // หาตัวใน list ที่กลุ่มเดียวกับ target
        private string MatchSide(System.Collections.Generic.List<string> list, string target)
        {
            target = (target ?? "").Trim();
            foreach (string d in list)
                if (IsSameDeptGroup(d, target)) return d;
            return null;
        }





        // helper : เพิ่ม dept ถ้าไม่ว่าง
        private void AddDeptIfValid(System.Collections.Generic.List<string> list, string dept)
        {
            if (!string.IsNullOrEmpty(dept) && !list.Contains(dept.Trim()))
                list.Add(dept.Trim());
        }

        #endregion

        #region --- Set View Mode ---
        private void SetViewMode(string status_id)
        {
            //--- Hide Manual Step 1-6
            SetStepManualVisible(false);

            // --- Show Text Mode
            lb_mode.Text = "Viewer";
            lb_mode.CssClass = "mode-badge mode-view";

            // --- Disabled Tel
            LockTelField();

            //Hide Step
            step_requester.Visible = false;
            step_mode.Visible = false;
            step_changelist.Visible = false;
            step_comment.Visible = false;            
            step_approver.Visible = false;

            // Disable Type (กันทั้ง postback + JS overlay)
            btnModeChange.Enabled = false;
            btnModeSwap.Enabled = false;
            btnModeChange.OnClientClick = "return false;";
            btnModeSwap.OnClientClick = "return false;";

            // lock ตัวสวิตช์ (คลิกไม่ได้)
            msTrack.Attributes["data-lock"] = "1";
            msTrack.Style["opacity"] = "0.6";
            msTrack.Style["cursor"] = "not-allowed";

            // Disable Search & Add list Panel
            pnlSearch_Add.Visible = false;

            //Gridview Borrow List
            gvChangeList.Columns[1].Visible = false;

            //Hide Button
            if (status_id == "99" || status_id == "100" || status_id == "999") //Update , Completed , Cancel
            {
                if (ButtonBar != null) ButtonBar.Visible = false;
            }
            else
            {
                if (CancelButton != null) CancelButton.Visible = false;   // ยกเลิกได้เฉพาะโหมด Issue
                if (DraftButton != null) DraftButton.Visible = false;
                if (RejectButton != null) RejectButton.Visible = false;
                if (SendButton != null) SendButton.Visible = false;
                if (ConfirmButton != null) ConfirmButton.Visible = false;
                if (ApproveButton != null) ApproveButton.Visible = false;
            }

            //Comment
            ddlReason_Change.Enabled = false;
            txt_ChangeComment.Enabled = false;

            //Borrow Note
            borrow_note.Visible = true;

            //Show Comment History เฉพาะเมื่อมีประวัติ Reject/Cancel
            bool hasReject = gv_rejectHistory.Rows.Count > 0;
            pnl_rejectComment.Visible = hasReject;

            LockApproverFlow();

        }
        /// <summary>ตั้งค่าหน้าเป็นโหมด Confirm</summary>
        private void SetConfirmMode(string status_id)
        {
            //--- Hide Manual Step 1-6
            SetStepManualVisible(false);

            // --- AAD Manual (50 = Confirm.pdf)
            if (status_id == "50")
            {
                litModeGuide.Text = BuildModeGuide(status_id);
                hfManualFile.Value = ManualFileByStatus(status_id);
            }
            hfReadMeTab.Value = "rmFlow";   // Read Me เปิดที่แท็บ การอนุมัติ

            // --- Show Text Mode
            lb_mode.Text = "Confirm";
            lb_mode.CssClass = "mode-badge mode-confirm";

            // --- Disabled Tel
            LockTelField();

            //Hide Step
            step_requester.Visible = false;
            step_changelist.Visible = false;
            step_comment.Visible = false;

            // Disable Type (กันทั้ง postback + JS overlay)
            btnModeChange.Enabled = false;
            btnModeSwap.Enabled = false;
            btnModeChange.OnClientClick = "return false;";
            btnModeSwap.OnClientClick = "return false;";

            // lock ตัวสวิตช์ (คลิกไม่ได้)
            msTrack.Attributes["data-lock"] = "1";
            msTrack.Style["opacity"] = "0.6";
            msTrack.Style["cursor"] = "not-allowed";

            // Disable Search & Add list Panel
            pnlSearch_Add.Visible = false;

            //Gridview Borrow List
            gvChangeList.Columns[1].Visible = false;

            //Hide Button
            if (status_id == "99" || status_id == "100" || status_id == "999") //Update , Completed , Cancel
            {
                if (ButtonBar != null) ButtonBar.Visible = false;
            }
            else
            {
                if (CancelButton != null) CancelButton.Visible = false;   // ยกเลิกได้เฉพาะโหมด Issue
                if (DraftButton != null) DraftButton.Visible = false;
                if (RejectButton != null) RejectButton.Visible = true;
                if (SendButton != null) SendButton.Visible = false;
                if (ConfirmButton != null) ConfirmButton.Visible = true;
                if (ApproveButton != null) ApproveButton.Visible = false;
            }

            //Comment (ปิด)
            ddlReason_Change.Enabled = false;
            txt_ChangeComment.Enabled = false;

            //Borrow Note
            borrow_note.Visible = true;

            //Show Comment History เฉพาะเมื่อมีประวัติ Reject/Cancel
            bool hasReject = gv_rejectHistory.Rows.Count > 0;
            pnl_rejectComment.Visible = hasReject;

            LockApproverFlow();
        }
        /// <summary>ตั้งค่าหน้าเป็นโหมด Approve — เปิดปุ่ม Approve, เปิด edit Model/Cell/Station</summary>
        private void SetApproveMode(string status_id)
        {
            //--- Hide Manual Step 1-6
            SetStepManualVisible(false);

            // --- Approver Manual (30 = Approver1.pdf , 31 = Approver2.pdf)
            if (status_id == "30" || status_id == "31")
            {
                litModeGuide.Text = BuildModeGuide(status_id);
                hfManualFile.Value = ManualFileByStatus(status_id);
            }

            hfReadMeTab.Value = "rmFlow";   // Read Me เปิดที่แท็บ การอนุมัติ

            // --- Show Text Mode
            lb_mode.Text = "Approve";
            lb_mode.CssClass = "mode-badge mode-approve";

            // --- Disabled Tel
            LockTelField();

            //Hide Step
            step_requester.Visible = false;
            step_mode.Visible = false;
            step_changelist.Visible = false;
            step_comment.Visible = false;
            step_approver.Visible = false;

            // Disable Type (กันทั้ง postback + JS overlay)
            btnModeChange.Enabled = false;
            btnModeSwap.Enabled = false;
            btnModeChange.OnClientClick = "return false;";
            btnModeSwap.OnClientClick = "return false;";
            btnModeChange.Attributes["style"] = "pointer-events:none; opacity:0.7;";
            btnModeSwap.Attributes["style"] = "pointer-events:none; opacity:0.7;";

            // lock ตัวสวิตช์ (คลิกไม่ได้)
            msTrack.Attributes["data-lock"] = "1";
            msTrack.Style["opacity"] = "0.6";
            msTrack.Style["cursor"] = "not-allowed";

            // Hide Search & Add list Panel
            pnlSearch_Add.Visible = false;

            //Gridview Borrow List
            gvChangeList.Columns[1].Visible = false;

            //Hide Button
            if (status_id == "99" || status_id == "100" || status_id == "999") //Update , Completed , Cancel
            {
                if (ButtonBar != null) ButtonBar.Visible = false;
            }
            else
            {
                if (CancelButton != null) CancelButton.Visible = false;   // ยกเลิกได้เฉพาะโหมด Issue
                if (DraftButton != null) DraftButton.Visible = false;
                if (RejectButton != null) RejectButton.Visible = true;
                if (SendButton != null) SendButton.Visible = false;
                if (ConfirmButton != null) ConfirmButton.Visible = false;
                if (ApproveButton != null) ApproveButton.Visible = true;
            }

            //Comment (ปิด)
            ddlReason_Change.Enabled = false;
            txt_ChangeComment.Enabled = false;

            //Borrow Note
            borrow_note.Visible = true;

            //Show Comment History เฉพาะเมื่อมีประวัติ Reject/Cancel
            bool hasReject = gv_rejectHistory.Rows.Count > 0;
            pnl_rejectComment.Visible = hasReject;

            // ✅ เปิด flag ให้ RowDataBound ปลดล็อค Model/Cell/Station
            //    เฉพาะ status ที่ยัง approve ได้ (ไม่ใช่ completed/cancel)
            if (status_id != "99" && status_id != "100" && status_id != "999")
                ViewState["APPROVE_EDIT"] = "0";  // ✅ ล็อกไว้ก่อน — Approver แก้ Model / Cell / Station ไม่ได้ (เปิดภายหลังให้เปลี่ยนเป็น "1")
            else
                ViewState["APPROVE_EDIT"] = "0";

            LockApproverFlow();
        }
        /// <summary>ปิดการแก้ไข control ทั้งหมดในแถว (recursive)</summary>
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
        private void LockTelField()
        {
            txtTel.ReadOnly = true;
            txtTel.CssClass = "fld";   // ตัด fld-edit (พื้นฟ้า) → ใช้สไตล์ readonly สีเทา
        }

        #endregion

        #region ===== Get Master =====
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
        /// <summary>โหลด master Model</summary>
        private DataTable GetModelMaster()
        {
            DataTable dt_model = new DataTable();

            string str_model = "Select * From jig_tool_system.dbo.draw_master_model where model_check = 1 order by model_name";
            dc.QryDT(conn, str_model);
            dt_model = dc.dt;
            return dt_model;
        }
        /// <summary>โหลด master เหตุผล</summary>
        private void GetReasonMaster()
        {
            DataTable dt_reasonChange = new DataTable();

            string str_reasonChange = " select reason_ID , reason_Name , changeFlow , comment_change , order_change" +
                                      " from Reason_Borrow_Master" +
                                      " Where changeFlow = 'true'" +
                                      " order by order_change";
            dc.QryDT(conn, str_reasonChange);
            dt_reasonChange = dc.dt;

            //DataTable modelMaster = (DataTable)ViewState["MODEL_MASTER"];
            ddlReason_Change.DataSource = dt_reasonChange;
            ddlReason_Change.DataTextField = "reason_Name";
            ddlReason_Change.DataValueField = "reason_ID";
            ddlReason_Change.DataBind();

            // เพิ่ม -- Select --
            ddlReason_Change.Items.Insert(0, new ListItem("-- Select --", ""));

            // ✅ 5C-7 : รายการ reason_ID ที่บังคับกรอกรายละเอียด → ใช้ฝั่ง JS
            var reqIds = new List<string>();
            foreach (DataRow r in dt_reasonChange.Rows)
            {
                if (r["comment_change"] != DBNull.Value && Convert.ToBoolean(r["comment_change"]))
                    reqIds.Add(r["reason_ID"].ToString().Trim());
            }
            hfReasonReq.Value = string.Join(",", reqIds.ToArray());

        }
        /// <summary>โหลด master Cell</summary>
        private DataTable GetCellMaster()
        {
            string str_cell =
                "select * from jig_tool_system.dbo.master_location2" +
                " where is_use = 1 and location1_id = 97 and location2_id != 0" +
                " order by location2_name";

            dc.QryDT_P(conn, str_cell);
            return dc.dt;
        }
        /// <summary>โหลด master Station</summary>
        private DataTable GetStationMaster()
        {
            string str_station =
                "select * from jig_tool_system.dbo.master_location3" +
                " where location1_id = 97 and location3_id != 0" +
                " order by location3_name";

            dc.QryDT_P(conn, str_station);
            return dc.dt;
        }

        // ── ดึงรายชื่อ J3 Approver ตาม dept_code_approve → CheckBoxList ──
        private void BindJ3Approver(List<string> deptList, bool skipInit = false)
        {
            hfJ3SourceL.Value = "[]"; hfJ3SelectedL.Value = "";
            hfJ3SourceR.Value = "[]"; hfJ3SelectedR.Value = "";
            litDeptL.Text = "-"; litDeptR.Text = "-";

            if (deptList == null || deptList.Count == 0)
            {
                pnlNoApprover.Visible = true;
                ScriptManager.RegisterStartupScript(this, GetType(), "j3HideR",
                    "var r=document.getElementById('j3BoxR'); if(r) r.style.display='none';", true);
                return;
            }

            pnlNoApprover.Visible = false;

            // ── กล่องซ้าย (แผนก 1) ──
            string deptL = deptList[0];
            litDeptL.Text = GetDeptGroupLabel(deptL);   // เดิม: litDeptL.Text = deptL;
            hfJ3SourceL.Value = BuildJ3Json(deptL);

            // ── กล่องขวา (แผนก 2 ถ้ามี) ──
            string showR;
            if (deptList.Count >= 2)
            {
                string deptR = deptList[1];
                litDeptR.Text = GetDeptGroupLabel(deptR);   // เดิม: litDeptR.Text = deptR;
                hfJ3SourceR.Value = BuildJ3Json(deptR);
                showR = "var r=document.getElementById('j3BoxR'); if(r) r.style.display='block';";
            }
            else
            {
                showR = "var r=document.getElementById('j3BoxR'); if(r) r.style.display='none';";
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "j3ToggleR", showR, true);

            if (!skipInit)
                ScriptManager.RegisterStartupScript(this, GetType(), "j3InitCall",
                    "if(typeof j3Init==='function'){ j3Init(); }", true);
        }

        // ── สร้าง JSON approver ของ dept กลุ่มเดียวกัน (3 หลัก, ยกเว้น 5505) — ไม่รวม Requester ──
        private string BuildJ3Json(string deptApprove)
        {
            deptApprove = (deptApprove ?? "").Trim();
            if (string.IsNullOrEmpty(deptApprove)) return "[]";

            string reqId = (txtEmpId.Text ?? "").Trim();   // ✅ 6C-12

            string sql =
                "SELECT emp_id, Name, position_code, position_name, dept_code, email FROM View_Authorize_J3_Approve" +
                " WHERE document_id='AAD2' AND status_id=30" +
                (deptApprove == "5505"
                    ? " AND LTRIM(RTRIM(dept_code)) = @dept"
                    : " AND LEFT(LTRIM(RTRIM(dept_code)),3) = @deptPrefix AND LTRIM(RTRIM(dept_code)) <> '5505'") +
                " AND LTRIM(RTRIM(emp_id)) <> @req" +
                " ORDER BY emp_id";

            var pars = new List<SqlParameter>
                {
                    new SqlParameter("@req", SqlDbType.NVarChar) { Value = reqId }
                };
            if (deptApprove == "5505")
                pars.Add(new SqlParameter("@dept", SqlDbType.NVarChar) { Value = deptApprove });
            else
                pars.Add(new SqlParameter("@deptPrefix", SqlDbType.NVarChar)
                { Value = deptApprove.Substring(0, Math.Min(3, deptApprove.Length)) });

            dc.QryDT_P(conn, sql, pars.ToArray());

            var sb = new System.Text.StringBuilder("[");
            for (int i = 0; i < dc.dt.Rows.Count; i++)
            {
                DataRow r = dc.dt.Rows[i];
                if (i > 0) sb.Append(",");
                sb.Append("{\"id\":\"" + JsEsc(r["emp_id"]) + "\",\"name\":\"" + JsEsc(r["Name"]) +
                          "\",\"email\":\"" + JsEsc(r["email"]) + "\",\"dept\":\"" + JsEsc(r["dept_code"]) +
                          "\",\"posCode\":\"" + JsEsc(r["position_code"]) +
                          "\",\"position\":\"" + JsEsc(r["position_name"]) + "\"}");
            }
            sb.Append("]");
            return sb.ToString();
        }

        // escape string สำหรับใส่ใน JSON
        private string JsEsc(object o)
        {
            if (o == null || o == DBNull.Value) return "";
            return o.ToString().Trim()
                .Replace("\\", "\\\\").Replace("\"", "\\\"")
                .Replace("\r", "").Replace("\n", " ");
        }

        #endregion

        #region ===== Step 6 : Approver Flow  (J3 Up Picker)  =====

        // ══ Auto Generate Approver Flow ══
        // เรียกท้าย postback ที่กระทบ dept approve (Use:Dept / add-remove / EMP ID)
        private void AutoGenerateApproverFlow()
        {
            int activeCount = 0; bool useDeptBlank = false;
            var groups = new System.Collections.Generic.List<string>();
            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                activeCount++;
                if (string.IsNullOrEmpty(GetVal(r, "Use_DeptCode"))) useDeptBlank = true;
                AddDeptGroupDistinct(groups, ApvDept(r));
            }

            if (IsPairMode())
            {
                bool ready = groups.Count == 2 || IsSwapPairsValidSilent();
                if (!ready) { ResetApproverFlowUI(); return; }
            }
            else
            {
                if (activeCount == 0 || useDeptBlank) { ResetApproverFlowUI(); return; }
                if (!IsUseDeptSameSilent()) { ResetApproverFlowUI(); return; }
            }

            // 2) คำนวณ dept approve ใหม่
            var deptApv = ResolveDeptApprove();
            string newA1 = deptApv.A1 ?? "";
            string newA2 = deptApv.A2 ?? "";

            string oldA1 = hfFlowDeptA1.Value ?? "";
            string oldA2 = hfFlowDeptA2.Value ?? "";

            bool a1Same = IsSameDeptGroup(newA1, oldA1) || (newA1 == "" && oldA1 == "");
            bool a2Same = IsSameDeptGroup(newA2, oldA2) || (newA2 == "" && oldA2 == "");
            bool deptChanged = !a1Same || !a2Same;
            bool everGenerated = (hfFlowGenerated.Value == "1");
            bool hasSelected = SplitIds(hfJ3SelectedL.Value).Count > 0
                            || SplitIds(hfJ3SelectedR.Value).Count > 0;

            // dept ไม่เปลี่ยน + เคย generate แล้ว → คงเดิม ไม่ทำอะไร
            if (everGenerated && !deptChanged) return;

            // dept เปลี่ยน + เคยเลือกชื่อแล้ว → เตือน modal (ยังไม่ล้าง รอผู้ใช้ยืนยัน)
            if (everGenerated && deptChanged && hasSelected)
            {
                // เก็บ dept ใหม่ค้างไว้ใน ViewState รอ apply หลังยืนยัน
                ViewState["PENDING_APV"] = deptApv;
                ScriptManager.RegisterStartupScript(this, GetType(), "openApvChangeConfirm",
                    "openApvChangeConfirm();", true);
                return;
            }

            // นอกนั้น (ครั้งแรก / เปลี่ยนแต่ยังไม่เลือกใคร) → generate เงียบ
            DoGenerateApproverFlow(deptApv);
        }

        // สร้าง flow จริง (ล้างรายชื่อเดิม + bind ใหม่)
        private void DoGenerateApproverFlow(DeptApproveResult deptApv)
        {
            ViewState["DEPT_APV"] = deptApv;

            var deptList = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrEmpty(deptApv.A1)) deptList.Add(deptApv.A1);
            if (!string.IsNullOrEmpty(deptApv.A2)) deptList.Add(deptApv.A2);
            ViewState["DEPT_APPROVE_LIST"] = deptList;

            hfFlowDeptA1.Value = deptApv.A1 ?? "";
            hfFlowDeptA2.Value = deptApv.A2 ?? "";
            hfFlowGenerated.Value = "1";
            pnlFlowNeedGen.Visible = false;
            pnlFlowContent.Visible = true;

            // ล้างรายชื่อที่เลือก (dept เปลี่ยน = เริ่มใหม่)
            hfJ3SelectedL.Value = "";
            hfJ3SelectedR.Value = "";

            BindJ3Approver(deptList);
        }

        // เคลียร์ Step 6 กลับสถานะ "รอ"
        private void ResetApproverFlowUI()
        {
            hfFlowGenerated.Value = "0";
            hfFlowDeptA1.Value = "";
            hfFlowDeptA2.Value = "";
            hfJ3SelectedL.Value = "";
            hfJ3SelectedR.Value = "";
            hfJ3SourceL.Value = "[]";
            hfJ3SourceR.Value = "[]";
            litDeptL.Text = "-";
            litDeptR.Text = "-";

            ViewState["DEPT_APV"] = null;
            ViewState["DEPT_APPROVE_LIST"] = null;
            ViewState["PENDING_APV"] = null;

            pnlFlowContent.Visible = false;
            pnlFlowNeedGen.Visible = true;
        }

        // ── validate Step 6 ก่อน Send : รายชื่อล่าสุดจาก DB / ไม่ใช่ Requester / แผนกละ ≥ 1 คน ──
        private bool ValidateApproverFlow()
        {
            if (hfFlowGenerated.Value != "1")
            {
                alert_toast("กรุณาเพิ่มรายการและเลือก ใช้งานที่ : Dept ให้ครบ แล้วเลือกผู้อนุมัติใน Step 6",
                            "ยังไม่ได้เลือกผู้อนุมัติ", "warn", "10000");          // ✅ R41
                ScrollToApprover();
                return false;
            }

            bool hasA2 = !string.IsNullOrEmpty(hfFlowDeptA2.Value);
            string reqId = txtEmpId.Text.Trim();

            // ✅ R42 : ดึงรายชื่อ J3 ล่าสุดจาก DB (คนที่ถูกถอดสิทธิ์จะหลุดออก)
            hfJ3SourceL.Value = BuildJ3Json(hfFlowDeptA1.Value);
            if (hasA2) hfJ3SourceR.Value = BuildJ3Json(hfFlowDeptA2.Value);

            var removed = new List<string>();
            CleanSelected(hfJ3SelectedL, hfJ3SourceL.Value, reqId, "Approver 1", removed);
            if (hasA2) CleanSelected(hfJ3SelectedR, hfJ3SourceR.Value, reqId, "Approver 2", removed);

            ScriptManager.RegisterStartupScript(this, GetType(), "j3Refresh",
                "if(typeof j3Init==='function'){ j3Init(); }", true);

            if (removed.Count > 0)
            {
                alert_toast("รายชื่อต่อไปนี้ถูกนำออกจากผู้อนุมัติ กรุณาตรวจสอบ / เลือกใหม่ :\n" +
                            string.Join("\n", removed.ToArray()),
                            "รายชื่อผู้อนุมัติเปลี่ยน", "warn", "15000");
                ScrollToApprover();
                return false;
            }

            if (SplitIds(hfJ3SelectedL.Value).Count == 0)
            {
                alert_toast("กรุณาเลือกผู้อนุมัติ Approver 1 อย่างน้อย 1 คน", "ยังไม่เลือก Approver 1", "warn", "10000");
                FocusApprover("L");
                return false;
            }
            if (hasA2 && SplitIds(hfJ3SelectedR.Value).Count == 0)
            {
                alert_toast("กรุณาเลือกผู้อนุมัติ Approver 2 อย่างน้อย 1 คน", "ยังไม่เลือก Approver 2", "warn", "10000");
                FocusApprover("R");
                return false;
            }
            return true;
        }

        // ตัด Requester + คนที่ไม่อยู่ในรายชื่อล่าสุด ออกจากรายการที่เลือก
        private void CleanSelected(HiddenField hfSel, string sourceJson, string reqId, string label, List<string> removed)
        {
            var valid = new HashSet<string>();
            try
            {
                dynamic arr = Newtonsoft.Json.JsonConvert.DeserializeObject(sourceJson ?? "[]");
                if (arr != null) foreach (var o in arr) valid.Add(Convert.ToString(o.id).Trim());
            }
            catch { }

            var keep = new List<string>();
            foreach (string id in SplitIds(hfSel.Value))
            {
                if (id == reqId)
                {
                    removed.Add("• " + label + " : " + id + " (เป็นผู้ร้องขอ ไม่สามารถอนุมัติเอกสารตัวเองได้)");
                    continue;
                }
                if (!valid.Contains(id))
                {
                    removed.Add("• " + label + " : " + id + " (ไม่มีสิทธิ์อนุมัติแล้ว)");
                    continue;
                }
                keep.Add(id);
            }
            hfSel.Value = string.Join(",", keep.ToArray());
        }


        private void ScrollToApprover()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "scrollApv",
                "var el=document.getElementById('" + pnlApproverFlow.ClientID + "');" +
                "if(el){ el.scrollIntoView({behavior:'smooth',block:'center'}); }", true);
        }



        private void FocusApprover(string side)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "focusApv",
                "var el=document.getElementById('" + pnlApproverFlow.ClientID + "');" +
                "if(el){ el.scrollIntoView({behavior:'smooth',block:'center'}); }" +
                "var i=document.getElementById('j3Search" + side + "'); if(i){ setTimeout(function(){i.focus();},300); }", true);
        }

        // ── bind สรุปรายชื่อใน Modal Send (read-only) ──
        // ── bind สรุปรายชื่อใน Modal Send (read-only) ──
        private void BindApproverSummary()
        {
            var deptList = ViewState["DEPT_APPROVE_LIST"] as List<string> ?? new List<string>();

            DataTable dt = new DataTable();
            dt.Columns.Add("OrderNo", typeof(int));
            dt.Columns.Add("StepLabel", typeof(string));
            dt.Columns.Add("emp_id", typeof(string));
            dt.Columns.Add("emp_name", typeof(string));
            dt.Columns.Add("position_code", typeof(string));
            dt.Columns.Add("position_name", typeof(string));
            dt.Columns.Add("dept", typeof(string));
            dt.Columns.Add("tel", typeof(string));
            dt.Columns.Add("email", typeof(string));

            int no = 1;
            AddApvRows(dt, ref no, "Approver 1", hfJ3SourceL.Value, hfJ3SelectedL.Value);
            if (deptList.Count >= 2)
                AddApvRows(dt, ref no, "Approver 2", hfJ3SourceR.Value, hfJ3SelectedR.Value);

            ViewState["SEND_SUMMARY_DT"] = dt;
            _lastSummaryStep = "";
            gvSendSummary.DataSource = dt;
            gvSendSummary.DataBind();

            pnlNoApprover.Visible = (dt.Rows.Count == 0);
        }
        // parse JSON source + selected ids → เติมแถวลง dt
        // parse JSON source + selected ids → เติมแถวลง dt
        private void AddApvRows(DataTable dt, ref int no, string stepLabel, string sourceJson, string selectedCsv)
        {
            var ids = SplitIds(selectedCsv);
            if (ids.Count == 0) return;

            dynamic arr = null;
            try { arr = Newtonsoft.Json.JsonConvert.DeserializeObject(sourceJson ?? "[]"); }
            catch { arr = null; }

            foreach (string id in ids)
            {
                string name = id, email = "", dept = "", posCode = "", posName = "";
                if (arr != null)
                {
                    foreach (var o in arr)
                    {
                        if (Convert.ToString(o.id) != id) continue;
                        name = Convert.ToString(o.name);
                        email = Convert.ToString(o.email);
                        dept = Convert.ToString(o.dept);
                        posCode = Convert.ToString(o.posCode);
                        posName = Convert.ToString(o.position);
                        break;
                    }
                }

                DataRow r = dt.NewRow();
                r["OrderNo"] = no++;
                r["StepLabel"] = stepLabel;
                r["emp_id"] = id;
                r["emp_name"] = name;
                r["position_code"] = posCode;
                r["position_name"] = posName;
                r["dept"] = dept;
                r["tel"] = GetUserExt(id);
                r["email"] = email;
                dt.Rows.Add(r);
            }
        }

        // แปลง source JSON + selected ids → DataTable (emp_id, emp_name)
        private DataTable BuildSummaryApvTable(string sourceJson, string selectedCsv)
        {
            var dt = new DataTable();
            dt.Columns.Add("emp_id", typeof(string));
            dt.Columns.Add("emp_name", typeof(string));

            var ids = SplitIds(selectedCsv);
            if (ids.Count == 0) return dt;

            // parse JSON แบบง่าย (reuse Newtonsoft)
            try
            {
                dynamic arr = Newtonsoft.Json.JsonConvert.DeserializeObject(sourceJson ?? "[]");
                foreach (string id in ids)
                {
                    string name = id;
                    if (arr != null)
                        foreach (var o in arr)
                            if (Convert.ToString(o.id) == id) { name = Convert.ToString(o.name); break; }
                    dt.Rows.Add(id, name);
                }
            }
            catch { foreach (string id in ids) dt.Rows.Add(id, id); }

            return dt;
        }

        // ── โหลด Approver เป็นตาราง (โหมด view/confirm/approve) ──
        // ── โหลด Approver เป็นตาราง (โหมด view/confirm/approve) ──
        private void BindApproverFlowTable(string Doc_No)
        {

            // ✅ แผนกของ Approver 1 / 2 จาก Header
            dc.QryDT_P(conn,
                "SELECT dept_code_approve, dept_code_approve2 FROM NotificationofChange_Control_List WHERE Doc_No=@d",
                new SqlParameter("@d", SqlDbType.NVarChar) { Value = Doc_No });
            if (dc.dt.Rows.Count > 0)
            {
                string d1 = GetVal(dc.dt.Rows[0], "dept_code_approve");
                string d2 = GetVal(dc.dt.Rows[0], "dept_code_approve2");
                _apvDeptLabelL = d1 == "" ? "" : GetDeptGroupLabel(d1);
                _apvDeptLabelR = d2 == "" ? "" : GetDeptGroupLabel(d2);
            }

            string sql =
                " SELECT emp_id, emp_name, position_name, position_code, tel, email," +
                "        status_id, approve_flag, approve_date" +
                " FROM NotificationofChange_Approver" +
                " WHERE Doc_No = @doc" +
                " ORDER BY CASE WHEN status_id = 30 THEN 0 ELSE 1 END," +
                "          CASE WHEN approve_flag = 'approved' THEN 0 ELSE 1 END, emp_name";

            dc.QryDT_P(conn, sql,
                new SqlParameter("@doc", SqlDbType.NVarChar) { Value = Doc_No });

            DataTable src = dc.dt;

            DataTable dt = new DataTable();
            dt.Columns.Add("OrderNo", typeof(int));
            dt.Columns.Add("StepLabel", typeof(string));
            dt.Columns.Add("emp_id", typeof(string));
            dt.Columns.Add("emp_name", typeof(string));
            dt.Columns.Add("position_name", typeof(string));
            dt.Columns.Add("position_code", typeof(string));
            dt.Columns.Add("tel", typeof(string));
            dt.Columns.Add("email", typeof(string));
            dt.Columns.Add("approve_flag", typeof(string));
            dt.Columns.Add("approve_date", typeof(DateTime));

            int no = 1;
            foreach (DataRow r in src.Rows)
            {
                DataRow n = dt.NewRow();
                n["OrderNo"] = no++;
                n["StepLabel"] = (GetVal(r, "status_id") == "31") ? "Approver 2" : "Approver 1";
                n["emp_id"] = GetVal(r, "emp_id");
                n["emp_name"] = GetVal(r, "emp_name");
                n["position_name"] = GetVal(r, "position_name");
                n["position_code"] = GetVal(r, "position_code");
                n["tel"] = GetVal(r, "tel");
                n["email"] = GetVal(r, "email");
                n["approve_flag"] = GetVal(r, "approve_flag");
                n["approve_date"] = (r["approve_date"] == DBNull.Value) ? (object)DBNull.Value : r["approve_date"];
                dt.Rows.Add(n);
            }

            litApvProgress.Text = BuildApvProgress(dt);

            _lastApvFlowStep = "";
            gvApproverFlow.DataSource = dt;
            gvApproverFlow.DataBind();
        }
        // ── แถบความคืบหน้าแต่ละขั้น ──
        private string BuildApvProgress(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return "";

            var steps = new List<string>();
            foreach (DataRow r in dt.Rows)
            {
                string s = r["StepLabel"].ToString();
                if (!steps.Contains(s)) steps.Add(s);
            }

            var sb = new StringBuilder("<div class='apv-progress'>");
            bool prevDone = true;

            for (int i = 0; i < steps.Count; i++)
            {
                string step = steps[i];
                DataRow actor = null;
                int total = 0;
                foreach (DataRow r in dt.Rows)
                {
                    if (r["StepLabel"].ToString() != step) continue;
                    total++;
                    if (r["approve_flag"].ToString().Trim().ToLower() == "approved") actor = r;
                }

                string state, ico, sub;
                if (actor != null)
                {
                    state = "done"; ico = "&#10003;";
                    string dateTxt = actor["approve_date"] == DBNull.Value ? "" :
                        " · " + Convert.ToDateTime(actor["approve_date"]).ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture);
                    sub = "อนุมัติโดย " + Server.HtmlEncode(actor["emp_name"].ToString()) + dateTxt;
                }
                else if (prevDone)
                {
                    state = "current"; ico = (i + 1).ToString();
                    sub = "รออนุมัติจาก " + total + " คน (คนใดคนหนึ่งอนุมัติได้)";
                }
                else
                {
                    state = ""; ico = (i + 1).ToString();
                    sub = "รอขั้นก่อนหน้า";
                }

                if (i > 0) sb.Append("<div class='apv-prog-arrow'>&#8250;</div>");

                string deptLbl = (step == "Approver 2") ? _apvDeptLabelR : _apvDeptLabelL;

                sb.Append("<div class='apv-prog-step " + state + "'>" +
                            "<span class='apv-prog-ico'>" + ico + "</span>" +
                            "<div class='apv-prog-text'>" +
                              "<span class='apv-prog-title'>" + Server.HtmlEncode(step) +
                                (deptLbl != "" ? " <span class='apv-prog-dept'>แผนก " + Server.HtmlEncode(deptLbl) + "</span>" : "") +
                              "</span>" +
                              "<span class='apv-prog-sub' title='" + sub + "'>" + sub + "</span>" +
                            "</div>" +
                          "</div>");

                prevDone = (actor != null);
            }

            sb.Append("</div>");
            return sb.ToString();
        }

        // ── avatar : สี/ตัวย่อ ชุดเดียวกับ j3Color / j3Initials ฝั่ง JS ──
        private static readonly string[] _avatarPalette =
            { "#b4412f", "#1f6feb", "#16884a", "#7a3fb0", "#c0660e", "#0f7c73", "#33475b", "#946200" };

        private string AvatarColor(string name)
        {
            int s = 0;
            foreach (char ch in (name ?? "")) s += ch;
            return _avatarPalette[s % _avatarPalette.Length];
        }

        private string Initials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            string[] p = name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string r = p.Length == 1
                ? p[0].Substring(0, Math.Min(2, p[0].Length))
                : p[0].Substring(0, 1) + p[1].Substring(0, 1);
            return r.ToUpper();
        }

        // ── badge สถานะ approve (สี + ข้อความไทย) ──
        protected void gvApproverFlow_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            DataRowView drv = (DataRowView)e.Row.DataItem;

            string step = Convert.ToString(drv["StepLabel"]);
            string empId = Convert.ToString(drv["emp_id"]);
            string name = Convert.ToString(drv["emp_name"]);
            string posCode = Convert.ToString(drv["position_code"]).Trim();
            string posName = Convert.ToString(drv["position_name"]).Trim();
            string tel = Convert.ToString(drv["tel"]).Trim();
            string email = Convert.ToString(drv["email"]).Trim();
            string flag = Convert.ToString(drv["approve_flag"]).Trim().ToLower();
            bool isA2 = (step == "Approver 2");
            bool grpStart = (step != _lastApvFlowStep);

            // ── class แถว : กลุ่ม / เริ่มกลุ่ม / คนที่กดอนุมัติจริง ──
            string css = isA2 ? "apv-a2" : "apv-a1";
            if (grpStart) css += " grp-start";
            if (flag == "approved") css += " apv-actor";
            e.Row.CssClass = css;         

            RenderApvRowCells(e.Row, drv, grpStart, "litStepBadge_view",isA2 ? _apvDeptLabelR : _apvDeptLabelL);   

            // ── สถานะ ──
            Literal lit = e.Row.FindControl("litApvStatus") as Literal;
            if (lit != null)
            {
                string text, cls;
                switch (flag)
                {
                    case "approved": text = "อนุมัติแล้ว"; cls = "apv-st-approved"; break;
                    case "delegated":
                    case "acknowledge":
                    case "acknowledg": text = "อนุมัติโดยกลุ่ม"; cls = "apv-st-delegated"; break;
                    case "reject": text = "ปฏิเสธ"; cls = "apv-st-reject"; break;
                    case "wait": text = "รออนุมัติ"; cls = "apv-st-wait"; break;
                    default: text = flag == "" ? "—" : flag; cls = "apv-st-other"; break;
                }
                lit.Text = "<span class='apv-st " + cls + "'>" + Server.HtmlEncode(text) + "</span>";
            }

            // ── วันที่ ──
            Literal litDate = e.Row.FindControl("litApvDate") as Literal;
            if (litDate != null)
                litDate.Text = (drv["approve_date"] == DBNull.Value)
                    ? "<span class='apv-muted'>—</span>"
                    : "<span class='apv-date'>" +
                      Convert.ToDateTime(drv["approve_date"]).ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture) +
                      "</span>";

            _lastApvFlowStep = step;
        }

        protected void gvSendSummary_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            DataRowView drv = (DataRowView)e.Row.DataItem;

            string step = Convert.ToString(drv["StepLabel"]);
            bool isA2 = (step == "Approver 2");
            bool grpStart = (step != _lastSummaryStep);

            e.Row.CssClass = (isA2 ? "apv-a2" : "apv-a1") + (grpStart ? " grp-start" : "");

            // แผนกของกลุ่ม : ใช้ label เดียวกับ Step 6 (litDeptL / litDeptR)
            string deptLabel = isA2 ? litDeptR.Text : litDeptL.Text;
            RenderApvRowCells(e.Row, drv, grpStart, "litStepBadge", deptLabel);

            // ── เมื่อส่ง ──
            Literal litState = e.Row.FindControl("litSendState") as Literal;
            if (litState != null)
                litState.Text = isA2
                    ? "<span class='apv-st apv-st-wait'>รอ Approver 1 อนุมัติ</span>"
                    : "<span class='apv-st apv-st-mail'>ได้รับอีเมลทันที</span>";

            _lastSummaryStep = step;
        }

        #endregion

        #region --- Helper : Check Tel / Phone / Blank Data ---

        // --- Check Text Tel (Phone)
        private bool ValidateTel()
        {
            string tel = txtTel.Text.Trim();

            if (!System.Text.RegularExpressions.Regex.IsMatch(tel, @"^\d{4,}(,\d{4,})*$"))
            {
                alert_toast("กรุณากรอกเบอร์ติดต่อ (Tel) เป็นตัวเลข 4 หลัก",
                            "ข้อมูลไม่ครบ", "warn", "10000");
                txtTel.Focus();
                return false;   // ไม่ผ่าน
            }
            return true;        // ผ่าน
        }

        // --- Check Change Request Comment
        private bool ValidateChangeComment()
        {
            // ── 1) reason ที่เลือก ──
            string reasonId = ddlReason_Change.SelectedValue;

            // ยังไม่เลือก reason → ปล่อยให้ check reason แยกจัดการ (ไม่เกี่ยว comment)
            if (string.IsNullOrEmpty(reasonId) || reasonId == "0")
                return true;

            // ── 2) query ว่า reason นี้บังคับ comment ไหม ──
            string str =
                "select comment_change from Reason_Borrow_Master" +
                " where reason_id = @reason_id";

            dc.QryDT_P(conn, str,
                new SqlParameter("@reason_id", SqlDbType.NVarChar) { Value = reasonId });

            bool requireComment = false;
            if (dc.dt.Rows.Count > 0 && dc.dt.Rows[0]["comment_change"] != DBNull.Value)
                requireComment = Convert.ToBoolean(dc.dt.Rows[0]["comment_change"]);

            // ── 3) ไม่บังคับ → ผ่าน ──
            if (!requireComment)
                return true;

            // ── 4) บังคับ → เช็ค comment ──
            string comment = txt_ChangeComment.Text.Trim();

            // 4.1 ไม่มี comment
            if (string.IsNullOrEmpty(comment))
            {
                alert_toast("เหตุผลการแจ้งเปลี่ยนแปลงนี้ต้องระบุ Comment", "กรุณากรอก Comment", "warn", "10000");
                txt_ChangeComment.Style["border-color"] = "#ffc107";
                txt_ChangeComment.Style["background-color"] = "#fff3cd";
                txt_ChangeComment.Focus();
                return false;
            }

            // 4.2 มีแต่ไม่ถึง 10 ตัวอักษร
            if (comment.Length < 10)
            {
                alert_toast("กรุณากรอก Comment อย่างน้อย 10 ตัวอักษร", "Comment สั้นเกินไป", "warn", "10000");
                txt_ChangeComment.Style["border-color"] = "#ffc107";
                txt_ChangeComment.Style["background-color"] = "#fff3cd";
                txt_ChangeComment.Focus();
                return false;
            }

            // ── 5) ผ่านครบ → คืน style ปกติ ──
            txt_ChangeComment.Style["border-color"] = "";
            txt_ChangeComment.Style["background-color"] = "";
            return true;
        }

        // --- Change Blank Data
        private Boolean Check_blankData()
        {
            DataTable dt = ChangeListDT;
            bool hasBlank = false;

            foreach (GridViewRow row in gvChangeList.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow) continue;

                // ══════════════════════════════════════════
                // USE : Dept
                // ══════════════════════════════════════════
                DropDownList ddlDept = row.FindControl("ddlDept") as DropDownList;

                bool blank_dept = !IsPairMode()
                                    && (ddlDept == null || string.IsNullOrEmpty(ddlDept.SelectedValue));
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

        // ── ตรวจ Use:Dept กลุ่มเดียวกันทุกรายการ (บังคับเสมอ) ──
        private bool ValidateUseDeptSame()
        {
            if (hfChangeMode.Value != "change") return true;

            string refUseDept = GetFirstUseDept();
            if (string.IsNullOrEmpty(refUseDept)) return true;   // ยังไม่เลือก → check blank เดิมจัดการ

            bool mismatch = false;
            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                string ud = GetVal(r, "Use_DeptCode");
                if (!IsSameDeptGroup(ud, refUseDept))
                {
                    mismatch = true;
                    break;
                }
            }

            if (mismatch)
            {
                HighlightUseDeptMismatch(refUseDept);
                alert_toast(
                    "Use : Dept ทุกรายการต้องเป็นกลุ่ม Dept เดียวกัน\n[อ้างอิงรายการแรก: " + refUseDept + "]",
                    "Use Dept ไม่ตรงกัน", "warn", "10000");
                return false;
            }
            return true;
        }

        #endregion

        #region --- Helper : Request Detail ---
        private bool GetDataHRMS_CSharp(string userId)
        {
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

            string sql = "select GNAME_ENG, FNAME_ENG, POSN_ENAME, BAND, DEPT_CODE, DEPT_ABB_NAME, EMAIL " +
                         "from ADMIN.v_emp_data_all_cpt where UPPER(emp_no)=UPPER('" + userId + "')";

            byte[] data = Encoding.UTF8.GetBytes("command=" + sql);

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
                return true;
            }
        }
        /// <summary>ล้าง UI ข้อมูลพนักงาน</summary>
        private void ClearEmployeeUI()
        {
            txtEmpName.Text = "";
            txtPosition.Text = "";
            txtDept.Text = "";
            hdtxtBand.Value = "";
            hdtxtEmail.Value = "";
        }
        /// <summary>แสดง UI ข้อมูลพนักงาน</summary>
        private void ShowEmployeeUI()
        {
            txtEmpName.Text = ViewState["EMP_NAME"].ToString();
            txtPosition.Text = ViewState["EMP_POSITION"].ToString();
            txtDept.Text = ViewState["EMP_DEPT"].ToString();
            hdtxtBand.Value = ViewState["EMP_BAND"].ToString();
            hdtxtEmail.Value = ViewState["EMP_EMAIL"].ToString();
        }
        /// <summary>เก็บข้อมูลพนักงานลง ViewState</summary>
        private void SaveEmployeeToViewState()
        {
            ViewState["EMP_ID"] = txtEmpId.Text;
            ViewState["EMP_NAME"] = txtEmpName.Text;
            ViewState["EMP_POSITION"] = txtPosition.Text;
            ViewState["EMP_DEPT"] = txtDept.Text;
            ViewState["EMP_BAND"] = hdtxtBand.Value;
            ViewState["EMP_EMAIL"] = hdtxtEmail.Value;
        }

        #endregion

        #region --- Helper : Alert

        // --- Helper : Alert
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

        // ── Helper : DropDownList warning ─────────────────────────────────────────
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

        // ── Helper : TextBox warning ───────────────────────────────────────────────
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
                case "30": alert_info = "Send Complete"; break;
                case "50": alert_info = "Approve Complete"; break;
                case "99": alert_info = "Confirm Complete"; break;
                case "999": alert_info = "Cancel Request"; break;
            }

            string link_path = (status_id == "99" || status_id == "50") ? "ApproveStatus.aspx" : "EquipmentStatus2.aspx";

            string jsDoc = HttpUtility.JavaScriptStringEncode(Doc_No);
            string jsInfo = HttpUtility.JavaScriptStringEncode(alert_info);

            string script =
                "lockUI();" +
                "if (typeof showToast === 'function') {" +
                "  showToast('Doc No : " + jsDoc + "', { title: '" + jsInfo + "', type: 'info', duration: 2000 });" +
                "}" +
                "setTimeout(function () { window.location.href = '" + link_path + "'; }, 2000);";

            ScriptManager.RegisterStartupScript(this, this.GetType(), "redirectScript", script, true);
        }

        #endregion

        #region --- Helper : GetNext Reject / GetNext IssueNo ---

        /// <summary>หาเลข Reject Count ถัดไป</summary>
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
        /// <summary>หาเลข issue_no ถัดไป (ใน transaction)</summary>
        private int GetNextIssueNo_Tx(SqlConnection c, SqlTransaction tx, int issueYear)
        {
            string sql =
                "select ISNULL(MAX(issue_no), 0) + 1 as MaxIssueNo" +
                " from NotificationofChange_Control_List WITH (UPDLOCK, HOLDLOCK)" +
                " where year(issue_date) = @yr";

            using (var cmd = new SqlCommand(sql, c, tx))
            {
                cmd.Parameters.Add(new SqlParameter("@yr", SqlDbType.Int) { Value = issueYear });
                object val = cmd.ExecuteScalar();
                return Convert.ToInt32(val);
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

        #endregion\

        #region --- Helper : Borrow List CHeck ---
        // <summary>ตรวจรายการที่ติดกระบวนการอื่น — แจ้งเตือนเองพร้อมระบุกระบวนการ + เลขเอกสาร</summary>
        private bool duringBorrowCheck()
        {
            string curDoc = litIssueNo.Text.Trim();
            var msgs = new List<string>();
            int no = 0;

            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                no++;
                if (r["ToolId"] == DBNull.Value || r["ToolSub"] == DBNull.Value || r["SystemId"] == DBNull.Value) continue;

                ProcInfo proc = GetToolProcessInfo(GetVal(r, "ToolId"), GetVal(r, "ToolSub"), GetVal(r, "SystemId"), curDoc);
                if (proc == null) continue;

                r["StatusId"] = "50";   // แถวขึ้นสีเหลืองขอบแดง
                string name = GetVal(r, "FixAssetNo") != "" ? GetVal(r, "FixAssetNo")
                            : (GetVal(r, "ControlNo") != "" ? GetVal(r, "ControlNo") : GetVal(r, "ToolNo"));
                msgs.Add("• รายการที่ " + no + " " + name + " → " + proc.Label + " [" + proc.DocNo + "]");
            }

            if (msgs.Count == 0) return false;

            alert_toast("อุปกรณ์ต่อไปนี้อยู่ระหว่างดำเนินการในเอกสารอื่น ไม่สามารถแจ้งเปลี่ยนแปลงได้\n" +
                        "กรุณา Remove ออกจากรายการก่อน :\n" + string.Join("\n", msgs.ToArray()),
                        "พบรายการอยู่ระหว่างดำเนินการ", "warn", "20000");
            return true;
        }
        #endregion

        #region ===== E-Mail : แจ้ง Approver หลัง Send (status 30) =====

        /// <summary>เมลแจ้งผู้อนุมัติขั้น approverStatusId ("30" = A1 , "31" = A2)</summary>
        private void SendApproverMail(string Doc_No, string approverStatusId,
                                      string subjectTag = null, string title = null, string subtitle = null,
                                      string extraHtml = null, List<string> ccEmails = null)
        {
            List<MailPerson> apvList = GetApproverPeople(Doc_No, approverStatusId);

            var toEmails = new List<string>();
            foreach (var p in apvList) AddDistinctMail(toEmails, p.Email);

            string htmlBody = BuildApproverMailBody(Doc_No, txtEmpName.Text.Trim(), txtEmpId.Text.Trim(),
                txtDept.Text.Trim(), apvList, title, subtitle, extraHtml);

            var people = new List<MailPerson>();
            foreach (var p in apvList) people.Add(AsRole(p, "To : Approver"));

            SendMailCore(Doc_No, subjectTag ?? "แจ้งอนุมัติเอกสารเปลี่ยนแปลง", htmlBody, toEmails, ccEmails, people);
        }

        // (3) ตารางรายการที่ A1/A2 เลือกไม่ตรง (ย้ายมาจาก SendReturnMail — ลบ SendReturnMail ทิ้งได้)
        private string BuildDiffMailHtml(List<ApvDiff> diffs)
        {
            string c = "padding:6px 12px; border:1px solid #e2e8f0;";
            var sb = new StringBuilder("<p style='margin:0 0 6px;'><b>รายการที่ Approver 1 / 2 เลือกไม่ตรงกัน</b></p>" +
                "<table style='border-collapse:collapse; margin:0 0 16px; font-size:13px;'><tr style='background:#f1f5f9;'>" +
                "<th style='" + c + " text-align:left;'>Fix Asset</th><th style='" + c + " text-align:left;'>Control No</th>" +
                "<th style='" + c + "'>Approver 1</th><th style='" + c + "'>Approver 2</th></tr>");
            foreach (var d in diffs)
                sb.Append("<tr style='background:#fff8e1;'>" +
                    "<td style='" + c + "'>" + Server.HtmlEncode(d.FixAsset) + "</td>" +
                    "<td style='" + c + "'>" + Server.HtmlEncode(d.ControlNo) + "</td>" +
                    "<td style='" + c + " text-align:center;'>" + MailMark(d.A1) + "</td>" +
                    "<td style='" + c + " text-align:center;'>" + MailMark(d.A2) + "</td></tr>");
            sb.Append("</table>");
            return sb.ToString();
        }

        private string MailMark(string v)
        {
            return v == "Y" ? "<span style='color:#16703d; font-weight:700;'>✓ อนุมัติ</span>"
                            : "<span style='color:#a82f2f; font-weight:700;'>✗ ไม่อนุมัติ</span>";
        }

        /// <summary>สร้างเนื้อหา HTML ของเมลแจ้ง Approver</summary>
        private string BuildApproverMailBody(string Doc_No, string reqName, string reqId,string reqDept, List<MailPerson> apvList,
                                             string title = null, string subtitle = null, string extraHtml = null,
                                             string linkUrl = null, string linkText = null)
        {
            string link = linkUrl ?? APPROVE_LINK;
            string lText = linkText ?? "&#128203; เข้าสู่หน้าอนุมัติ (Approve Status)";
            string today = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            title = title ?? "แจ้งขออนุมัติการเปลี่ยนแปลง Fixed Asset / Calibration";
            subtitle = subtitle ?? "มีเอกสารรอการอนุมัติ กรุณาตรวจสอบและดำเนินการ";

            StringBuilder sb = new StringBuilder();
                    sb.AppendLine(@"
                        <html><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8' /></head>
                        <body style='font-family:Segoe UI,Tahoma,Arial,sans-serif; font-size:14px; color:#1e293b;'>

                        <p style='font-size:18px; font-weight:700; margin:0 0 4px;'>" + Server.HtmlEncode(title) + @"</p>
                        <p style='margin:0 0 18px; color:#475569;'>" + Server.HtmlEncode(subtitle) + @"</p>

                        <table style='border-collapse:collapse; margin-bottom:18px;'>
                            <tr><td style='padding:3px 14px 3px 0; color:#475569; white-space:nowrap;'>เลขที่เอกสาร : </td>
                                <td style='padding:3px 0; font-weight:700; color:#1d4ed8;'>" + Server.HtmlEncode(Doc_No) + @"</td></tr>
                            <tr><td style='padding:3px 14px 3px 0; color:#475569; white-space:nowrap;'>ผู้ร้องขอ : </td>
                                <td style='padding:3px 0; font-weight:600;'>" + Server.HtmlEncode(reqName + " (" + reqId + ")") + @"</td></tr>
                            <tr><td style='padding:3px 14px 3px 0; color:#475569; white-space:nowrap;'>แผนก : </td>
                                <td style='padding:3px 0; font-weight:600;'>" + Server.HtmlEncode(reqDept) + @"</td></tr>
                            <tr><td style='padding:3px 14px 3px 0; color:#475569; white-space:nowrap;'>วันที่แจ้ง : </td>
                                <td style='padding:3px 0; font-weight:600;'>" + today + @"</td></tr>
                        </table>
                        " + (extraHtml ?? "") + @"
                        <p style='margin:0 0 18px;'>
                            <a href='" + link + @"'
                               style='display:inline-block; background:#1d4ed8; color:#fff; text-decoration:none;
                                      padding:10px 22px; border-radius:6px; font-weight:600;'>
                               " + lText + @"
                            </a>
                        </p>
                        <p style='margin:0 0 18px; font-size:12px; color:#64748b;'>
                            หรือคัดลอกลิงก์นี้ : <br>
                            <a href='" + link + @"'>" + link + @"</a>
                        </p>");

            // ── Debug : แสดงรายชื่อ Approver ที่ส่งไป ──
            if (MAIL_DEBUG && apvList != null && apvList.Count > 0)
            {
                sb.AppendLine(@"
                    <hr style='border:none; border-top:1px solid #cbd5e1; margin:14px 0;' />
                    <p style='font-weight:700; margin:0 0 6px; color:#c2410c;'>[DEBUG MODE] รายชื่อผู้อนุมัติที่จะได้รับเมล (Production)</p>
                    <table style='border-collapse:collapse; margin-bottom:10px; font-size:13px;'>
                        <tr style='background:#f1f5f9;'>
                            <th style='padding:6px 12px; text-align:left; border:1px solid #e2e8f0;'>ชื่อ</th>
                            <th style='padding:6px 12px; text-align:left; border:1px solid #e2e8f0;'>Emp ID</th>
                            <th style='padding:6px 12px; text-align:left; border:1px solid #e2e8f0;'>E-Mail</th>
                        </tr>");

                foreach (var p in apvList)
                {
                    string mailShow = (!string.IsNullOrEmpty(p.Email) && p.Email.Contains("@"))
                        ? p.Email
                        : "<span style='color:#b91c1c;'>ไม่พบ E-Mail</span>";
                    sb.AppendLine(
                        "<tr>" +
                        "<td style='padding:6px 12px; border:1px solid #e2e8f0;'>" + Server.HtmlEncode(p.Name) + "</td>" +
                        "<td style='padding:6px 12px; border:1px solid #e2e8f0;'>" + Server.HtmlEncode(p.EmpId) + "</td>" +
                        "<td style='padding:6px 12px; border:1px solid #e2e8f0;'>" + mailShow + "</td>" +
                        "</tr>");
                }
                sb.AppendLine("</table>");
            }

            sb.AppendLine(@"
                <br>
                <div style='color:#94a3b8; font-size:11px; line-height:1.8;'>
                ****************************************************************<br>
                This auto-email was sent from Tool &amp; Equipment System.<br>
                Best regard.<br>
                --------------------- Original Message Ends --------------------<br>
                ****************************************************************
                </div></body></html>");

            return sb.ToString();
        }

        // ── HRMS person model ──
        private class MailPerson
        {
            public string EmpId;
            public string Name;
            public string Email;
            public string Position; 
            public string Tel;
            public string Role;
        }

        /// <summary>ดึงชื่อ/อีเมลจาก HRMS (คืน null ถ้าไม่พบ — แต่ยังคืน object ถ้ามี id อย่างเดียว)</summary>
        private MailPerson GetHrmsPerson(string empId)
        {
            if (string.IsNullOrEmpty(empId)) return null;
            empId = empId.Trim();

            MailPerson p = new MailPerson { EmpId = empId, Name = empId, Email = "" };

            try
            {
                // tel จาก sys_user_master
                string strTel = "Select ext From sys_user_master Where emp_id='" + empId + "'";
                dc.QryDT(conn_user, strTel);
                if (dc.dt.Rows.Count > 0) p.Tel = GetVal(dc.dt.Rows[0], "ext");

                string sql =
                    "select GNAME_ENG, FNAME_ENG, POSN_ENAME, EMAIL " +
                    "from ADMIN.v_emp_data_all_cpt where UPPER(emp_no)=UPPER('" + empId + "')";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(HRMS_URL);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 15000;

                byte[] data = Encoding.UTF8.GetBytes("command=" + HttpUtility.UrlEncode(sql));
                using (Stream stream = request.GetRequestStream())
                    stream.Write(data, 0, data.Length);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string json = reader.ReadToEnd();
                    dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                    if (result != null && result.data != null && result.data.Count > 0)
                    {
                        var u = result.data[0];
                        p.Name = Convert.ToString(u.GNAME_ENG) + " " + Convert.ToString(u.FNAME_ENG);
                        p.Email = Convert.ToString(u.EMAIL);
                        p.Position = Convert.ToString(u.POSN_ENAME);
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[GetHrmsPerson] " + empId + " : " + ex.Message);
            }

            return p;
        }

        // ── เพิ่มอีเมลไม่ให้ซ้ำ ──
        private static void AddDistinctMail(System.Collections.Generic.List<string> list, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            value = value.Trim();
            foreach (string s in list)
                if (string.Equals(s, value, StringComparison.OrdinalIgnoreCase)) return;
            list.Add(value);
        }

        // ดึง emp_id ของ approver เฉพาะ status ที่ระบุ
        private System.Collections.Generic.List<string> GetApproverIdsByStatus(string Doc_No, string statusId)
        {
            var ids = new System.Collections.Generic.List<string>();
            using (var c = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString))
            {
                c.Open();
                using (var cmd = new SqlCommand(
                    "SELECT emp_id FROM NotificationofChange_Approver WHERE Doc_No=@doc AND status_id=@st", c))
                {
                    cmd.Parameters.AddWithValue("@doc", Doc_No);
                    cmd.Parameters.AddWithValue("@st", statusId);
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                            if (rd["emp_id"] != DBNull.Value) ids.Add(rd["emp_id"].ToString().Trim());
                }
            }
            return ids;
        }

        private enum MailKind { Reject, RejectMismatch, Partial, Approved, Confirmed }

        private string StepName(int step)
        {
            return step == 1 ? "Approver 1" : (step == 2 ? "Approver 2" : "AAD Confirm");
        }

        /// <summary>เมลผลการดำเนินการ — To : Requester (ไม่มีอีเมล → Admin แทน) , CC : Admin Level 1 , Link : Equipment Status</summary>
        private void SendResultMail(string Doc_No, MailKind kind, int step, string comment, string extraHtml = null)
        {
            string stepName = StepName(step);
            string subjectTag, title, subtitle;
            switch (kind)
            {
                case MailKind.Reject:
                    subjectTag = "เอกสารถูก Reject (" + stepName + ")";
                    title = "เอกสารแจ้งเปลี่ยนแปลงถูก Reject";
                    subtitle = stepName + " Reject เอกสาร — เอกสารกลับเป็น Draft กรุณาแก้ไขแล้วส่งใหม่";
                    break;
                case MailKind.Partial:
                    subjectTag = "อนุมัติไม่ครบทุกรายการ (" + stepName + ")";
                    title = "อนุมัติไม่ครบทุกรายการ";
                    subtitle = stepName + " ไม่อนุมัติบางรายการ — รายการที่ไม่อนุมัติจะไม่ถูกอัปเดต Location";
                    break;
                case MailKind.Confirmed:
                    subjectTag = "AAD Confirm เรียบร้อย";
                    title = "AAD Confirm เรียบร้อย";
                    subtitle = "เอกสารผ่านการยืนยันจาก AAD แล้ว — รอระบบอัปเดต Location";
                    break;
                case MailKind.RejectMismatch:
                    subjectTag = "เอกสารถูก Reject (Approver 1 / 2 เลือกไม่ตรงกัน)";
                    title = "เอกสารถูก Reject — Approver 1 และ Approver 2 อนุมัติรายการไม่ตรงกัน";
                    subtitle = "เอกสารกลับเป็น Draft แล้ว กรุณาตรวจสอบรายการ ปรับแก้หรือจัดทำ Request ใหม่ แล้วกด Send Request อีกครั้ง";
                    break;
                default:
                    subjectTag = "อนุมัติเรียบร้อย (" + stepName + ")";
                    title = "เอกสารได้รับการอนุมัติ";
                    subtitle = stepName + " อนุมัติแล้ว";
                    break;
            }

            List<MailPerson> admins = GetAdminLevel1();
            string reqMail = GetRequesterEmail();
            bool noReq = !IsMail(reqMail);

            // ── ผู้รับ ──
            var to = new List<string>();
            var people = new List<MailPerson>();
            if (noReq)
            {
                to = MailsOf(admins);                                         // ✅ 7H-4 : แทนด้วย Admin
                foreach (var a in admins) people.Add(AsRole(a, "To (แทน Requester)"));
            }
            else
            {
                to.Add(reqMail);
                people.Add(RequesterPerson(reqMail, "To : Requester"));
            }
            List<string> cc = MailsOf(admins);                                 // ✅ 7F-5 / 7H-1 : CC Admin Level 1
            foreach (var a in admins) people.Add(AsRole(a, "CC : Admin Level 1"));

            // ── เนื้อหา ──
            var sb = new StringBuilder();
            if (noReq) sb.Append(BuildNoRequesterMailWarning(true));
            if (admins.Count == 0) sb.Append(BuildNoAdminWarning());
            sb.Append(BuildActorHtml(stepName));
            if (!string.IsNullOrEmpty(comment)) sb.Append(BuildReasonHtml(comment));
            if (!string.IsNullOrEmpty(extraHtml)) sb.Append(extraHtml);
            sb.Append(BuildItemMailTable(GetItemResultTable(Doc_No),
                kind != MailKind.Reject && kind != MailKind.RejectMismatch));

            string body = BuildApproverMailBody(Doc_No, txtEmpName.Text.Trim(), txtEmpId.Text.Trim(),
                txtDept.Text.Trim(), null, title, subtitle, sb.ToString(),
                REQUEST_LINK, "&#128196; เปิดหน้ารายการเอกสาร (Equipment Status)");

            SendMailCore(Doc_No, subjectTag, body, to, cc, people);
        }

        /// <summary>อนุมัติครบ (status 50) — To : AAD Team (Admin Level 1) , CC : Requester , Link : Approve Status</summary>
        private void SendAadConfirmRequestMail(string Doc_No, int step, string comment)
        {
            string stepName = StepName(step);
            List<MailPerson> admins = GetAdminLevel1();
            string reqMail = GetRequesterEmail();
            bool noReq = !IsMail(reqMail);

            // ── ผู้รับ ──
            List<string> to = MailsOf(admins);
            var cc = new List<string>();
            var people = new List<MailPerson>();
            foreach (var a in admins) people.Add(AsRole(a, "To : AAD Team"));
            if (!noReq)
            {
                cc.Add(reqMail);
                people.Add(RequesterPerson(reqMail, "CC : Requester"));
            }

            // ── เนื้อหา ──
            var sb = new StringBuilder();
            if (noReq) sb.Append(BuildNoRequesterMailWarning(false));
            if (admins.Count == 0) sb.Append(BuildNoAdminWarning());
            sb.Append(BuildActorHtml(stepName));
            if (!string.IsNullOrEmpty(comment)) sb.Append(BuildReasonHtml(comment));
            sb.Append(BuildItemMailTable(GetItemResultTable(Doc_No), true));

            string body = BuildApproverMailBody(Doc_No, txtEmpName.Text.Trim(), txtEmpId.Text.Trim(),
                txtDept.Text.Trim(), null,
                "แจ้งรอ AAD Confirm — เอกสารอนุมัติครบแล้ว",
                stepName + " อนุมัติแล้ว กรุณาตรวจสอบรายการและดำเนินการ AAD Confirm",
                sb.ToString(),
                APPROVE_LINK, "&#128203; เข้าสู่หน้าอนุมัติ (Approve Status)");

            SendMailCore(Doc_No, "รอ AAD Confirm", body, to, cc, people);
        }

        private string BuildActorHtml(string stepName)
        {
            string id = Session["emp_id"] != null ? Session["emp_id"].ToString() : "";
            string name = Session["user_name"] != null ? Session["user_name"].ToString() : "";
            return "<p style='margin:0 0 12px;'><b>ดำเนินการโดย :</b> " +
                   Server.HtmlEncode(name + " (" + id + ")") + " — " + Server.HtmlEncode(stepName) + "</p>";
        }

        private string BuildReasonHtml(string comment)
        {
            return "<div style='margin:0 0 14px; padding:10px 14px; background:#fdf3e7; " +
                   "border-left:4px solid #d9822b; border-radius:4px;'><b>เหตุผล :</b> " +
                   Server.HtmlEncode(comment).Replace("\n", "<br>") + "</div>";
        }

        // รายการในเอกสาร + ผลอนุมัติรายแถว (อ่านจาก DB หลัง commit)
        private DataTable GetItemResultTable(string Doc_No)
        {
            string sql =
                "SELECT A.No, A.FixAsset, A.ControlNo, B.tool_no, B.dept_code_real AS cur_dept, A.Use_Dept," +
                "       A.reject_flag, A.a1_selected, A.a2_selected" +
                " FROM NotificationofChange_List A" +
                " LEFT JOIN Tool_Equipment_List_View B" +
                "   ON A.tool_id = B.tool_id AND A.sub_id = B.sub_no AND A.system_id = B.sys_status" +
                " WHERE A.Doc_No = @d ORDER BY A.No";

            var dt = new DataTable();
            using (var c = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString))
            using (var cmd = new SqlCommand(sql, c))
            {
                cmd.Parameters.AddWithValue("@d", Doc_No);
                using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
            }
            return dt;
        }

        private string BuildItemMailTable(DataTable dt, bool showResult)
        {
            string th = "padding:6px 10px; border:1px solid #e2e8f0; text-align:left; background:#f1f5f9;";
            string td = "padding:6px 10px; border:1px solid #e2e8f0;";
            int ok = 0, ng = 0;
            var rows = new StringBuilder();

            foreach (DataRow r in dt.Rows)
            {
                bool rej = GetVal(r, "reject_flag") == "reject";
                string a1 = GetVal(r, "a1_selected"), a2 = GetVal(r, "a2_selected");
                string by = (a1 == "N" && a2 == "N") ? "Approver 1 และ 2"
                          : (a1 == "N" ? "Approver 1" : (a2 == "N" ? "Approver 2" : ""));
                if (rej) ng++; else ok++;

                rows.Append("<tr" + (showResult && rej ? " style='background:#fdecec;'" : "") + ">");
                rows.Append("<td style='" + td + "'>" + Server.HtmlEncode(GetVal(r, "No")) + "</td>");
                rows.Append("<td style='" + td + "'>" + Server.HtmlEncode(GetVal(r, "FixAsset")) + "</td>");
                rows.Append("<td style='" + td + "'>" + Server.HtmlEncode(GetVal(r, "ControlNo")) + "</td>");
                rows.Append("<td style='" + td + "'>" + Server.HtmlEncode(GetVal(r, "tool_no")) + "</td>");
                rows.Append("<td style='" + td + "'>" + Server.HtmlEncode(GetVal(r, "cur_dept")) + " &rarr; " +
                            Server.HtmlEncode(GetVal(r, "Use_Dept")) + "</td>");
                if (showResult)
                {
                    rows.Append("<td style='" + td + " text-align:center;'>" + (rej
                        ? "<span style='color:#a82f2f; font-weight:700;'>✗ ไม่อนุมัติ</span>" +
                          (by != "" ? "<br><small style='color:#64748b;'>โดย " + by + "</small>" : "")
                        : "<span style='color:#16703d; font-weight:700;'>✓ อนุมัติ</span>") + "</td>");
                }
                rows.Append("</tr>");
            }

            string head = showResult
                ? "<p style='margin:0 0 6px;'><b>ผลการอนุมัติ :</b> อนุมัติ " + ok + " รายการ · " +
                  "<span style='color:#a82f2f;'>ไม่อนุมัติ " + ng + " รายการ</span></p>"
                : "<p style='margin:0 0 6px;'><b>รายการในเอกสาร</b> (" + dt.Rows.Count + " รายการ)</p>";

            return head +
                "<table style='border-collapse:collapse; margin:0 0 16px; font-size:13px;'><tr>" +
                "<th style='" + th + "'>#</th><th style='" + th + "'>Fix Asset</th>" +
                "<th style='" + th + "'>Control No</th><th style='" + th + "'>Tool No</th>" +
                "<th style='" + th + "'>Dept (ปัจจุบัน → ปลายทาง)</th>" +
                (showResult ? "<th style='" + th + " text-align:center;'>ผล</th>" : "") +
                "</tr>" + rows + "</table>";
        }

        private List<string> GetApproverEmails(string Doc_No, bool includeA2)
        {
            var list = new List<string>();
            string sql = "SELECT email FROM NotificationofChange_Approver WHERE Doc_No=@d AND (status_id=30" +
                         (includeA2 ? " OR status_id=31" : "") + ")";
            using (var c = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString))
            using (var cmd = new SqlCommand(sql, c))
            {
                cmd.Parameters.AddWithValue("@d", Doc_No);
                c.Open();
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        if (rd["email"] != DBNull.Value && IsMail(rd["email"].ToString()))
                            AddDistinctMail(list, rd["email"].ToString());
            }
            return list;
        }

        private string GetRequesterEmail()
        {
            string m = (hdtxtEmail.Value ?? "").Trim();
            if (IsMail(m)) return m;

            string id = txtEmpId.Text.Trim();
            if (id == "") return "";
            MailPerson p = GetHrmsPerson(id);
            return (p != null && IsMail(p.Email)) ? p.Email.Trim() : "";
        }

        private static bool IsMail(string s) { return !string.IsNullOrEmpty(s) && s.Trim().Contains("@"); }

        private static bool ContainsMail(List<string> list, string v)
        {
            foreach (string s in list)
                if (string.Equals(s.Trim(), (v ?? "").Trim(), StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        /// <summary>ส่งเมลกลาง — dedupe To/CC , CC anirut เสมอ , Debug ส่งเข้า DEBUG_MAIL_TO
        /// สร้างเนื้อหาใน Request นี้ แล้วส่ง SMTP ใน Background Thread (หน้าจอไม่ต้องรอ)</summary>
        private void SendMailCore(string Doc_No, string subjectTag, string htmlBody,
                                  List<string> to, List<string> cc, List<MailPerson> debugPeople = null)
        {
            var toList = new List<string>();
            if (to != null) foreach (string a in to) if (IsMail(a)) AddDistinctMail(toList, a);

            var ccList = new List<string>();
            if (cc != null)
                foreach (string a in cc)
                    if (IsMail(a) && !ContainsMail(toList, a)) AddDistinctMail(ccList, a);

            if (toList.Count == 0) toList.Add(MAIL_CC_ALWAYS);
            if (!ContainsMail(toList, MAIL_CC_ALWAYS)) AddDistinctMail(ccList, MAIL_CC_ALWAYS);

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(MAIL_FROM);
            mail.SubjectEncoding = Encoding.UTF8;
            mail.BodyEncoding = Encoding.UTF8;
            mail.IsBodyHtml = true;
            mail.Subject = (MAIL_DEBUG ? "[TEST] " : "") + "[Tool & Equipment] " + subjectTag + " " + Doc_No;

            if (MAIL_DEBUG)
            {
                mail.To.Add(DEBUG_MAIL_TO);
                htmlBody = InjectDebugRecipients(htmlBody, toList, ccList, debugPeople);
            }
            else
            {
                foreach (string a in toList) mail.To.Add(a);
                foreach (string a in ccList) mail.CC.Add(a);
            }
            mail.Body = htmlBody;

            // ✅ 7D : ส่งเบื้องหลัง — ห้ามใช้ Session / Control / Server ใน Thread นี้
            string logDoc = Doc_No;
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    SmtpClient client = new SmtpClient
                    {
                        Host = SMTP_HOST,
                        Port = SMTP_PORT,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        Timeout = 30000
                    };
                    client.Send(mail);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("[SendMailCore] " + logDoc + " : " + ex.Message);
                }
                finally
                {
                    mail.Dispose();
                }
            });
        }

        private string InjectDebugRecipients(string html, List<string> to, List<string> cc, List<MailPerson> people = null)
        {
            string cell = "padding:5px 10px; border:1px solid #e2e8f0;";
            var sb = new StringBuilder();
            sb.Append("<hr style='border:none; border-top:1px solid #cbd5e1; margin:14px 0;' />" +
                      "<p style='font-weight:700; margin:0 0 6px; color:#c2410c;'>[DEBUG MODE] ผู้รับจริงเมื่อขึ้น Production</p>");

            if (people != null && people.Count > 0)
            {
                sb.Append("<table style='border-collapse:collapse; margin:0 0 8px; font-size:13px;'>" +
                          "<tr style='background:#f1f5f9;'>" +
                          "<th style='" + cell + " text-align:left;'>บทบาท</th>" +
                          "<th style='" + cell + " text-align:left;'>ชื่อ</th>" +
                          "<th style='" + cell + " text-align:left;'>Emp ID</th>" +
                          "<th style='" + cell + " text-align:left;'>E-Mail</th></tr>");
                foreach (var p in people)
                {
                    string mailShow = IsMail(p.Email) ? Server.HtmlEncode(p.Email)
                                                      : "<span style='color:#b91c1c;'>ไม่พบ E-Mail</span>";
                    sb.Append("<tr><td style='" + cell + "'>" + Server.HtmlEncode(p.Role ?? "") + "</td>" +
                              "<td style='" + cell + "'>" + Server.HtmlEncode(p.Name ?? "") + "</td>" +
                              "<td style='" + cell + "'>" + Server.HtmlEncode(p.EmpId ?? "") + "</td>" +
                              "<td style='" + cell + "'>" + mailShow + "</td></tr>");
                }
                sb.Append("</table>");
            }

            sb.Append("<table style='border-collapse:collapse; font-size:13px;'>" +
                      "<tr><td style='padding:4px 12px 4px 0; color:#475569;'>To :</td><td>" +
                          Server.HtmlEncode(string.Join(", ", to.ToArray())) + "</td></tr>" +
                      "<tr><td style='padding:4px 12px 4px 0; color:#475569;'>CC :</td><td>" +
                          Server.HtmlEncode(string.Join(", ", cc.ToArray())) + "</td></tr></table>");

            int i = html.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
            return i >= 0 ? html.Insert(i, sb.ToString()) : html + sb.ToString();
        }

        /// <summary>ผู้อนุมัติของขั้นที่ระบุ (30 = A1 , 31 = A2) จาก NotificationofChange_Approver — ไม่เรียก HRMS</summary>
        private List<MailPerson> GetApproverPeople(string Doc_No, string statusId)
        {
            var list = new List<MailPerson>();
            using (var c = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT emp_id, emp_name, email, position_name, tel FROM NotificationofChange_Approver" +
                " WHERE Doc_No=@doc AND status_id=@st ORDER BY emp_name", c))
            {
                cmd.Parameters.AddWithValue("@doc", Doc_No);
                cmd.Parameters.AddWithValue("@st", statusId);
                c.Open();
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        list.Add(new MailPerson
                        {
                            EmpId = rd["emp_id"].ToString().Trim(),
                            Name = rd["emp_name"] == DBNull.Value ? rd["emp_id"].ToString().Trim() : rd["emp_name"].ToString().Trim(),
                            Email = rd["email"] == DBNull.Value ? "" : rd["email"].ToString().Trim(),
                            Position = rd["position_name"] == DBNull.Value ? "" : rd["position_name"].ToString().Trim(),
                            Tel = rd["tel"] == DBNull.Value ? "" : rd["tel"].ToString().Trim()
                        });
            }
            return list;
        }

        // ── Admin Level 1 (AAD Team) — โหลดครั้งเดียวต่อ Request ──
        private List<MailPerson> _adminLv1;

        private List<MailPerson> GetAdminLevel1()
        {
            if (_adminLv1 != null) return _adminLv1;

            var list = new List<MailPerson>();
            // ⚠️ ปรับชื่อตาราง / คอลัมน์ให้ตรงกับของจริง (ดูหัวข้อ 0)
            string sql = "SELECT A.emp_id , A.fname + ' ' + A.lname as name , B.email" +
                         " FROM Equipment_Admin A" +
                         " left join user_master.dbo.sys_user_master B on A.emp_id = B.emp_id" +
                         " WHERE A.level_1 = 'True' And A.status_use = 1 AND B.email is not Null And B.email != ''" +
                         " ORDER BY emp_id";

            try
            {
                using (var c = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString))
                using (var cmd = new SqlCommand(sql, c))
                {
                    c.Open();
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                            list.Add(new MailPerson
                            {
                                EmpId = rd["emp_id"] == DBNull.Value ? "" : rd["emp_id"].ToString().Trim(),
                                Name = rd["name"] == DBNull.Value ? "" : rd["name"].ToString().Trim(),
                                Email = rd["email"] == DBNull.Value ? "" : rd["email"].ToString().Trim(),
                                Role = "Admin Level 1"
                            });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[GetAdminLevel1] " + ex.Message);
            }

            _adminLv1 = list;
            return list;
        }

        private List<string> MailsOf(List<MailPerson> people)
        {
            var list = new List<string>();
            foreach (var p in people) if (IsMail(p.Email)) AddDistinctMail(list, p.Email);
            return list;
        }

        private MailPerson AsRole(MailPerson p, string role)
        {
            return new MailPerson { EmpId = p.EmpId, Name = p.Name, Email = p.Email, Role = role };
        }

        private MailPerson RequesterPerson(string mail, string role)
        {
            return new MailPerson { EmpId = txtEmpId.Text.Trim(), Name = txtEmpName.Text.Trim(), Email = mail, Role = role };
        }

        /// <summary>กล่องเตือน : Requester ไม่มี E-Mail</summary>
        private string BuildNoRequesterMailWarning(bool sentToAdminInstead)
        {
            string who = Server.HtmlEncode(txtEmpName.Text.Trim() + " (" + txtEmpId.Text.Trim() + ")");
            return "<div style='margin:0 0 14px; padding:10px 14px; background:#fbeceb; border-left:4px solid #c93b3b; " +
                   "border-radius:4px; color:#8f2b22;'><b>⚠️ ผู้ร้องขอไม่มี E-Mail ในระบบ</b><br>" + who + " — " +
                   (sentToAdminInstead
                       ? "อีเมลฉบับนี้จึงส่งถึง Admin แทน กรุณาแจ้งผลให้ผู้ร้องขอทราบ"
                       : "ไม่สามารถ CC ถึงผู้ร้องขอได้ กรุณาแจ้งผู้ร้องขอโดยตรง") +
                   " และอัปเดต E-Mail ใน HRMS</div>";
        }

        /// <summary>กล่องเตือน : ไม่พบรายชื่อ Admin Level 1</summary>
        private string BuildNoAdminWarning()
        {
            return "<div style='margin:0 0 14px; padding:10px 14px; background:#fff7e0; border-left:4px solid #d99a06; " +
                   "border-radius:4px; color:#6b4d00;'><b>⚠️ ไม่พบรายชื่อ Admin Level 1</b><br>" +
                   "กรุณาตรวจสอบตาราง Equipment_Admin (level_1 = True)</div>";
        }

        #endregion

        #region --- Helper : On Process Check ---
        // ข้อมูล process ที่อุปกรณ์ติดอยู่
        private class ProcInfo
        {
            public string Label;   // "AAD3 : เบิก"
            public string DocNo;   // "AAD3-26-00039"
            public string Url;     // ลิงก์ view (ประกอบ k + mode แล้ว)
        }

        // คืน null ถ้าว่าง / ไม่ null = ติด process
        private ProcInfo GetToolProcessInfo(string toolId, string subId, string systemId, string currentDocNo)
        {
            var checks = new[]
            {
                new { Tbl="Borrow_List",               Label="AAD1 : ยืม",           Page="borrow_tool2.aspx",       ExcludeDoc=false },
                new { Tbl="NotificationofChange_List", Label="AAD2 : แจ้งเปลี่ยนแปลง", Page="NotificationOfChange.aspx", ExcludeDoc=true  },
                new { Tbl="Withdraw_List",             Label="AAD3 : เบิก",          Page="withdraw2.aspx",          ExcludeDoc=false },
                new { Tbl="Storage_List",              Label="AAD4 : จัดเก็บ",        Page="storage.aspx",            ExcludeDoc=false },
            };

            foreach (var c in checks)
            {
                string sql =
                    "SELECT TOP 1 Doc_No FROM " + c.Tbl +
                    " WHERE tool_id=@tid AND sub_id=@sid AND system_id=@sys" +
                    "   AND status_id > 1 AND status_id < 100";

                bool doExclude = c.ExcludeDoc && !string.IsNullOrEmpty(currentDocNo) && currentDocNo != "-";
                if (doExclude) sql += " AND Doc_No <> @doc";

                var prms = new System.Collections.Generic.List<SqlParameter>
        {
            new SqlParameter("@tid", SqlDbType.NVarChar){ Value = toolId },
            new SqlParameter("@sid", SqlDbType.NVarChar){ Value = subId },
            new SqlParameter("@sys", SqlDbType.NVarChar){ Value = systemId },
        };
                if (doExclude) prms.Add(new SqlParameter("@doc", SqlDbType.NVarChar) { Value = currentDocNo });

                dc.QryDT_P(conn, sql, prms.ToArray());
                if (dc.dt.Rows.Count > 0)
                {
                    string docNo = GetVal(dc.dt.Rows[0], "Doc_No");
                    string k = Convert.ToBase64String(Encoding.UTF8.GetBytes(docNo));
                    string url = c.Page + "?k=" + HttpUtility.UrlEncode(k) + "&mode=view";
                    return new ProcInfo { Label = c.Label, DocNo = docNo, Url = url };
                }
            }
            return null;
        }

        // สร้าง badge : ถ้าติด process (มี url) → เป็นลิงก์เปิด tab view
        protected string BuildStatusBadge(object displayStatus, object procUrl)
        {
            string ds = displayStatus == null ? "" : displayStatus.ToString();
            string url = procUrl == null ? "" : procUrl.ToString();
            string cls = GetStatusBadgeClass(ds);

            // ไม่ติด process → badge ธรรมดา
            if (string.IsNullOrEmpty(url))
                return "<span class='badge " + cls + "'>" + Server.HtmlEncode(ds) + "</span>";

            // ติด process → badge เป็นลิงก์ + ไอคอนลิงก์
            string safeUrl = Server.HtmlEncode(url);
            return "<a href='#' onclick=\"window.open('" + safeUrl + "','_blank');return false;\" " +
                   "class='badge " + cls + "' style='text-decoration:none; cursor:pointer;' " +
                   "title='คลิกเพื่อดูเอกสาร (mode view)'>" +
                   Server.HtmlEncode(ds) + " &#128279;</span>".Replace("</span>", "</a>");
        }

        #endregion

        #region --- Helper : Step 6

        // ── model แถวที่ A1/A2 เลือกต่างกัน ──
        private class ApvDiff
        {
            public string FixAsset, ControlNo, A1, A2;
        }
        private string RowKey(DataRow r)
        {
            return GetVal(r, "ToolId") + "|" + GetVal(r, "ToolSub") + "|" + GetVal(r, "SystemId");
        }
        private void AddKeyParams(SqlCommand cmd, string Doc_No, DataRow r)
        {
            cmd.Parameters.AddWithValue("@doc", Doc_No);
            cmd.Parameters.AddWithValue("@tid", GetVal(r, "ToolId"));
            cmd.Parameters.AddWithValue("@sid", GetVal(r, "ToolSub"));
            cmd.Parameters.AddWithValue("@sys", GetVal(r, "SystemId"));
        }

        // เทียบ a1/a2 ทุกแถวของเอกสาร (ไม่กรอง 999 แล้ว)
        private List<ApvDiff> GetA1A2Diff(SqlTransaction tx, string Doc_No)
        {
            var list = new List<ApvDiff>();
            using (SqlCommand cmd = new SqlCommand(
                "SELECT FixAsset, ControlNo, a1_selected, a2_selected FROM NotificationofChange_List" +
                " WHERE Doc_No=@d AND ISNULL(a1_selected,'') <> ISNULL(a2_selected,'')" +
                " ORDER BY No", conn, tx))
            {
                cmd.Parameters.AddWithValue("@d", Doc_No);
                using (SqlDataReader rd = cmd.ExecuteReader())
                    while (rd.Read())
                        list.Add(new ApvDiff
                        {
                            FixAsset = rd["FixAsset"] == DBNull.Value ? "" : rd["FixAsset"].ToString().Trim(),
                            ControlNo = rd["ControlNo"] == DBNull.Value ? "" : rd["ControlNo"].ToString().Trim(),
                            A1 = rd["a1_selected"] == DBNull.Value ? "" : rd["a1_selected"].ToString().Trim(),
                            A2 = rd["a2_selected"] == DBNull.Value ? "" : rd["a2_selected"].ToString().Trim()
                        });
            }
            return list;
        }

        // เช็ค Use:Dept กลุ่มเดียวกัน (ไม่ alert)
        private bool IsUseDeptSameSilent()
        {
            string refUseDept = GetFirstUseDept();
            if (string.IsNullOrEmpty(refUseDept)) return false;
            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                if (!IsSameDeptGroup(GetVal(r, "Use_DeptCode"), refUseDept)) return false;
            }
            return true;
        }

        // เช็ค swap pair ครบ + 2 dept (ไม่ alert)
        private bool IsSwapPairsValidSilent()
        {
            var keys = new System.Collections.Generic.List<int>();
            var map = new System.Collections.Generic.Dictionary<int, int>();
            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                int pk = r["PairKey"] == DBNull.Value ? 0 : Convert.ToInt32(r["PairKey"]);
                map[pk] = map.ContainsKey(pk) ? map[pk] + 1 : 1;
            }
            var groups = new System.Collections.Generic.List<string>();
            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                AddDeptGroupDistinct(groups, ApvDept(r));
            }
            foreach (var kv in map) if (kv.Value != 2) return false;   // มีคู่ไม่ครบ
            return groups.Count >= 1 && groups.Count <= 2;
        }

        protected void btnApvChangeOk_Click(object sender, EventArgs e)
        {
            var deptApv = ViewState["PENDING_APV"] as DeptApproveResult ?? ResolveDeptApprove();
            DoGenerateApproverFlow(deptApv);   // ล้าง + regenerate
            ViewState["PENDING_APV"] = null;
        }

        #endregion

        #region --- กันกดซ้ำ (Done-Guard) ---

        // key ต่อ "หน้าฟอร์ม 1 ครั้งที่เปิด" + action
        private string FormKey(string action)
        {
            if (ViewState["FORM_ID"] == null) ViewState["FORM_ID"] = Guid.NewGuid().ToString("N");
            return "NOC_DONE_" + ViewState["FORM_ID"] + "_" + action;
        }

        private bool AlreadyDone(string action, out string doc)
        {
            doc = Session[FormKey(action)] as string;
            return !string.IsNullOrEmpty(doc);
        }

        private void MarkDone(string action, string doc)
        {
            Session[FormKey(action)] = string.IsNullOrEmpty(doc) ? "-" : doc;
        }

        /// <summary>กดซ้ำหลังบันทึกไปแล้ว → แจ้ง + ล็อกหน้า + พาไปหน้ารายการ (ไม่บันทึก/ไม่ส่งเมลซ้ำ)</summary>
        private void ShowAlreadyDone(string doc)
        {
            string link = ResolveUrl(PageMode == "edit" ? "~/EquipmentStatus2.aspx" : "~/ApproveStatus.aspx");
            string script =
                "lockUI('รายการนี้บันทึกไปแล้ว กำลังเปลี่ยนหน้า...');" +
                "if(typeof showToast==='function'){ showToast('รายการนี้ถูกบันทึกไปแล้ว (Doc No : " +
                    HttpUtility.JavaScriptStringEncode(doc) + ") — ระบบไม่บันทึกซ้ำ',{title:'กดซ้ำ',type:'info',duration:2500}); }" +
                "setTimeout(function(){ window.location.replace('" + HttpUtility.JavaScriptStringEncode(link) + "'); },2000);";
            ScriptManager.RegisterStartupScript(this, GetType(), "alreadyDone", script, true);
        }

        #endregion

        #region --- AAD Confirm : Modal + Status Use / ผู้รับผิดชอบ ---

        /// <summary>master Status Use (tool_equipment_status.status_name)</summary>
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
                "SELECT dept_code, pic_dept, emp_id FROM Responsible_Dept" +
                " WHERE emp_id IS NOT NULL AND LTRIM(RTRIM(emp_id)) <> ''" +
                " ORDER BY dept_code, emp_id");
            _respM = dc.dt.Copy();
            return _respM;
        }

        /// <summary>ผู้รับผิดชอบของ Use Dept (WHERE dept_code = Use Dept)</summary>
        private List<DataRow> RespOfDept(string useDept)
        {
            var list = new List<DataRow>();
            int d;
            if (!int.TryParse((useDept ?? "").Trim(), out d)) return list;
            foreach (DataRow m in GetResponsibleMaster().Rows)
                if (m["dept_code"] != DBNull.Value && Convert.ToInt32(m["dept_code"]) == d) list.Add(m);
            return list;
        }

        private string RespText(DataRow m)
        {
            string name = m["pic_dept"] == DBNull.Value ? "" : m["pic_dept"].ToString().Trim();
            string id = m["emp_id"].ToString().Trim();
            return name == "" ? id : name + " (" + id + ")";
        }

        private string RespName(string useDept, string empId)
        {
            foreach (DataRow m in RespOfDept(useDept))
                if (m["emp_id"].ToString().Trim() == empId)
                    return m["pic_dept"] == DBNull.Value ? "" : m["pic_dept"].ToString().Trim();
            return "";
        }

        /// <summary>AAD Confirm ได้เฉพาะ mode=confirm และ status 50</summary>
        private bool IsAadEditable()
        {
            return CurrentMode == "confirm" && Convert.ToString(ViewState["STATUS_ID"]) == "50";
        }

        /// <summary>เพิ่มคอลัมน์ใน ChangeListDT (เผื่อ ViewState ของหน้าที่เปิดค้างไว้ก่อนอัปเดตโค้ด)</summary>
        private void EnsureAadCols(DataTable dt)
        {
            if (!dt.Columns.Contains("Use_StatusId")) dt.Columns.Add("Use_StatusId", typeof(string));
            if (!dt.Columns.Contains("Resp_EmpId")) dt.Columns.Add("Resp_EmpId", typeof(string));
            if (!dt.Columns.Contains("Resp_Name")) dt.Columns.Add("Resp_Name", typeof(string));
        }

        /// <summary>ตาราง + Dropdown ใน Modal AAD Confirm</summary>
        private void BuildConfirmSummary()
        {
            DataTable src = ChangeListDT;
            EnsureAadCols(src);
            DataTable deptM = (DataTable)ViewState["DEPT_MASTER"];
            DataTable modelM = (DataTable)ViewState["MODEL_MASTER"];
            DataTable cellM = GetCellMaster().Copy();
            DataTable stationM = GetStationMaster().Copy();

            var dt = new DataTable();
            foreach (string c in new[] { "RowKey", "FixAssetNo", "ControlNo", "ToolNo",
                                 "OldDept", "OldModel", "OldCell", "OldStation",
                                 "NewDept", "NewModel", "NewCell", "NewStation",
                                 "UseDept", "StatusId", "RespEmpId", "RespName" })
                dt.Columns.Add(c, typeof(string));
            dt.Columns.Add("RowNo", typeof(int));
            dt.Columns.Add("Rejected", typeof(bool));

            int no = 1, nOk = 0, nNg = 0;
            foreach (DataRow r in src.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                bool rej = GetVal(r, "RejectFlag") == "reject";
                if (rej) nNg++; else nOk++;

                DataRow n = dt.NewRow();
                n["RowKey"] = RowKey(r);
                n["RowNo"] = no++;
                n["Rejected"] = rej;
                n["FixAssetNo"] = GetVal(r, "FixAssetNo");
                n["ControlNo"] = GetVal(r, "ControlNo");
                n["ToolNo"] = GetVal(r, "ToolNo");
                n["OldDept"] = FormatDeptDisplay(GetVal(r, "DeptCode"), GetVal(r, "DeptCodeReal"));
                n["OldModel"] = GetVal(r, "ModelName");
                n["OldCell"] = GetVal(r, "CellName");
                n["OldStation"] = GetVal(r, "StationName");
                n["NewDept"] = ResolveDeptName(deptM, GetVal(r, "Use_DeptCode"));
                n["NewModel"] = GetVal(r, "HD_Model") == "txt" ? GetVal(r, "Add_Model")
                              : ResolveName(modelM, "model_id", "model_name", GetVal(r, "Use_ModelId"));
                n["NewCell"] = GetVal(r, "HD_Cell") == "txt" ? GetVal(r, "Add_Cell")
                              : ResolveName(cellM, "location2_id", "location2_name", GetVal(r, "Use_CellId"));
                n["NewStation"] = GetVal(r, "HD_Station") == "txt" ? GetVal(r, "Add_Station")
                              : ResolveName(stationM, "location3_id", "location3_name", GetVal(r, "Use_StationId"));
                n["UseDept"] = GetVal(r, "Use_DeptCode");
                n["StatusId"] = GetVal(r, "Use_StatusId");
                n["RespEmpId"] = GetVal(r, "Resp_EmpId");
                n["RespName"] = GetVal(r, "Resp_Name");
                dt.Rows.Add(n);
            }

            litCfmDoc.Text = Server.HtmlEncode(litIssueNo.Text.Trim());
            litCfmSummary.Text =
                "<div class='cfm-summary'><b>สรุป :</b>" +
                "<span class='res-badge res-ok'>✓ อัปเดต Location " + nOk + " รายการ</span>" +
                (nNg > 0 ? "<span class='res-badge res-ng'>✗ ไม่อนุมัติ " + nNg + " รายการ (ไม่อัปเดต)</span>" : "") +
                "</div>";

            gvConfirmSummary.DataSource = dt;
            gvConfirmSummary.DataBind();
        }

        /// <summary>bind ผล + Dropdown Status Use / ผู้รับผิดชอบ ในแถว Modal</summary>
        protected void gvConfirmSummary_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            DataRowView drv = (DataRowView)e.Row.DataItem;
            bool rej = drv["Rejected"] != DBNull.Value && Convert.ToBoolean(drv["Rejected"]);

            var lit = e.Row.FindControl("litCfmResult") as Literal;
            if (lit != null)
                lit.Text = rej ? "<span class='res-badge res-ng'>✗ ไม่อนุมัติ</span>"
                               : "<span class='res-badge res-ok'>✓ อนุมัติ</span>";

            var ddlSt = e.Row.FindControl("ddlStatusUse") as DropDownList;
            var ddlRp = e.Row.FindControl("ddlResponsible") as DropDownList;
            if (ddlSt == null || ddlRp == null) return;

            ddlSt.Items.Clear();
            ddlRp.Items.Clear();

            if (rej)
            {
                ddlSt.Items.Add(new ListItem("—", ""));
                ddlRp.Items.Add(new ListItem("—", ""));
                ddlSt.Enabled = ddlRp.Enabled = false;
                e.Row.CssClass = "apv-row-off";
                return;
            }

            // ── Status Use : ค่าเริ่มต้นตาม Use Dept (5505 = Spare , อื่น = Use) ──
            ddlSt.Items.Add(new ListItem("-- Select --", ""));
            foreach (DataRow m in GetStatusUseMaster().Rows)
                ddlSt.Items.Add(new ListItem(m["status_name"].ToString().Trim(), m["status_id"].ToString().Trim()));

            string st = Convert.ToString(drv["StatusId"]);
            if (st == "") st = DefaultStatusUse(Convert.ToString(drv["UseDept"]));     // ✅ ข้อ 1
            if (ddlSt.Items.FindByValue(st) != null) ddlSt.SelectedValue = st;

            // ── ผู้รับผิดชอบ : WHERE dept_code = Use Dept , ค่าเริ่มต้น = รายชื่อแรก ──
            foreach (DataRow m in RespOfDept(Convert.ToString(drv["UseDept"])))
                ddlRp.Items.Add(new ListItem(RespText(m), m["emp_id"].ToString().Trim()));

            string rp = Convert.ToString(drv["RespEmpId"]);
            if (rp != "" && ddlRp.Items.FindByValue(rp) == null)        // ค่าที่เคยเลือกแต่ไม่อยู่ใน master แล้ว
            {
                string nm = Convert.ToString(drv["RespName"]);
                ddlRp.Items.Insert(0, new ListItem(nm == "" ? rp : nm + " (" + rp + ")", rp));
            }
            if (ddlRp.Items.Count == 0)
                ddlRp.Items.Add(new ListItem("-- ไม่มีผู้รับผิดชอบของ Dept นี้ --", ""));

            if (rp != "") ddlRp.SelectedValue = rp;
            else ddlRp.SelectedIndex = 0;     // ✅ แสดงรายชื่อแรก

            // ✅ ข้อ 2 : ช่องที่ต้องตรวจ = สีเหลือง / ยังไม่มีค่า = ขอบแดง
            ddlSt.CssClass = "form-control form-control-sm ddl_edit aad-ddl" + (ddlSt.SelectedValue == "" ? " aad-ddl-empty" : "");
            ddlRp.CssClass = "form-control form-control-sm ddl_edit aad-ddl" + (ddlRp.SelectedValue == "" ? " aad-ddl-empty" : "");
        }

        /// <summary>อ่าน Dropdown ใน Modal → ChangeListDT + ตรวจครบ (ลงสีเหลืองช่องที่ขาด)</summary>
        private bool ReadAndValidateConfirmModal()
        {
            DataTable dt = ChangeListDT;
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
                if (hf == null || ddlSt == null || ddlRp == null || !ddlSt.Enabled) continue;   // แถว reject ข้าม

                DataRow row = null;
                foreach (DataRow r in dt.Rows)
                    if (GetVal(r, "CompareData") != "Delete" && RowKey(r) == hf.Value) { row = r; break; }
                if (row == null) continue;

                // ── เก็บค่าที่เลือก ──
                row["Use_StatusId"] = ddlSt.SelectedValue == "" ? (object)DBNull.Value : ddlSt.SelectedValue;
                row["Resp_EmpId"] = ddlRp.SelectedValue == "" ? (object)DBNull.Value : ddlRp.SelectedValue;
                string nm = RespName(GetVal(row, "Use_DeptCode"), ddlRp.SelectedValue);
                row["Resp_Name"] = nm == "" ? (object)DBNull.Value : nm;

                // ── ตรวจครบ (ขอบแดงเฉพาะช่องที่ขาด) ──
                bool noSt = ddlSt.SelectedValue == "";
                bool noRp = ddlRp.SelectedValue == "";
                ddlSt.CssClass = "form-control form-control-sm ddl_edit aad-ddl" + (noSt ? " aad-ddl-empty" : "");
                ddlRp.CssClass = "form-control form-control-sm ddl_edit aad-ddl" + (noRp ? " aad-ddl-empty" : "");

                if (noSt || noRp)
                    msgs.Add("• รายการที่ " + no + (GetVal(row, "FixAssetNo") != "" ? " (" + GetVal(row, "FixAssetNo") + ")" : "") + " : " +
                             (noSt ? "ยังไม่เลือก Status Use " : "") +
                             (noRp ? "ไม่มีผู้รับผิดชอบของ Use Dept " + GetVal(row, "Use_DeptCode") + " (ตรวจตาราง Responsible_Dept)" : ""));
            }
            ChangeListDT = dt;

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

        /// <summary>ค่าเริ่มต้น Status Use : ปลายทาง 5505 → Spare , Dept อื่น → Use</summary>
        private string DefaultStatusUse(string useDept)
        {
            return (useDept ?? "").Trim() == "5505" ? STATUS_SPARE : STATUS_USE;
        }

        #endregion

        // ── Current Dept ของรายการแรก (ข้าม Delete) ──
        private string GetFirstCurrentDept()
        {
            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                return GetVal(r, "DeptCodeReal");   // ✅ เปลี่ยนจาก DeptCode
            }
            return "";
        }

        // ── Use:Dept ของรายการแรก (ข้าม Delete) ──
        private string GetFirstUseDept()
        {
            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                return GetVal(r, "Use_DeptCode");
            }
            return "";
        }

        // ── หา division จาก dept_code ──
        private string GetDeptDivision(string deptCode)
        {
            if (string.IsNullOrEmpty(deptCode)) return "";

            string str = "SELECT division FROM View_dept_master WHERE dept_code = @dept_code";
            dc.QryDT_P(conn, str,
                new SqlParameter("@dept_code", SqlDbType.NVarChar) { Value = deptCode });

            if (dc.dt.Rows.Count > 0 && dc.dt.Rows[0]["division"] != DBNull.Value)
                return dc.dt.Rows[0]["division"].ToString().Trim();

            return "";
        }

        // ── ลงสีเหลือง ddlDept ที่ไม่ตรงตัวอ้างอิง ──
        private void HighlightUseDeptMismatch(string refUseDept)
        {
            foreach (GridViewRow row in gvChangeList.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow) continue;

                DropDownList ddlDept = row.FindControl("ddlDept") as DropDownList;
                if (ddlDept == null) continue;

                string ud = ddlDept.SelectedValue;

                // ว่าง หรือ ตรงตัวอ้างอิง → ปกติ / ไม่ตรง → เตือน
                bool warn = !string.IsNullOrEmpty(ud) && !IsSameDeptGroup(ud, refUseDept);

                SetWarning_DDL(ddlDept, warn);
            }
        }

        // ── แสดง Dept : ถ้า dept_code != dept_code_real → "5505 (5330)" ──
        protected string FormatDeptDisplay(object deptCode, object deptCodeReal)
        {
            string dc_ = deptCode == null || deptCode == DBNull.Value ? "" : deptCode.ToString().Trim();
            string real = deptCodeReal == null || deptCodeReal == DBNull.Value ? "" : deptCodeReal.ToString().Trim();

            if (string.IsNullOrEmpty(real)) return dc_;
            if (string.IsNullOrEmpty(dc_)) return real;

            if (!string.Equals(dc_, real, StringComparison.OrdinalIgnoreCase))
                return dc_ + " (" + real + ")";

            return dc_;   // เท่ากัน → แสดงปกติ
        }

        // ══════════════════════════════════════════════════════════════
        // เทียบ dept 2 ตัวว่าอยู่กลุ่มเดียวกันไหม
        //   - ปกติ : เทียบ 3 หลักแรก (หลัก 4 ไม่มีผล)
        //   - ยกเว้น : ถ้าตัวใดตัวหนึ่ง = "5505" เป๊ะ → เทียบ 4 หลักเต็ม
        // ══════════════════════════════════════════════════════════════
        private bool IsSameDeptGroup(string a, string b)
        {
            a = (a ?? "").Trim();
            b = (b ?? "").Trim();

            if (a.Length == 0 || b.Length == 0) return false;

            // ยกเว้น 5505 → ต้องเท่ากันเป๊ะทั้ง 4 หลัก
            if (a == "5505" || b == "5505")
                return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

            // ปกติ → เทียบ 3 หลักแรก
            string pa = a.Length >= 3 ? a.Substring(0, 3) : a;
            string pb = b.Length >= 3 ? b.Substring(0, 3) : b;
            return string.Equals(pa, pb, StringComparison.OrdinalIgnoreCase);
        }

        // ── หา dept ทั้งหมดในกลุ่มเดียวกัน (สำหรับ label) ──
        private string GetDeptGroupLabel(string deptApprove)
        {
            deptApprove = (deptApprove ?? "").Trim();
            if (string.IsNullOrEmpty(deptApprove)) return "-";

            // 5505 → แสดงเดี่ยว
            if (deptApprove == "5505") return "5505";

            // ดึง dept ทั้งกลุ่ม 3 หลัก (ไม่รวม 5505) ที่มี approver จริง
            string sql =
                "SELECT DISTINCT LTRIM(RTRIM(dept_code)) AS dc FROM View_Authorize_J3_Approve" +
                " WHERE document_id='AAD2' AND status_id=30" +
                "   AND LEFT(LTRIM(RTRIM(dept_code)),3)=@prefix" +
                "   AND LTRIM(RTRIM(dept_code)) <> '5505'" +
                " ORDER BY dc";

            dc.QryDT_P(conn, sql, new SqlParameter("@prefix", SqlDbType.NVarChar)
            { Value = deptApprove.Substring(0, Math.Min(3, deptApprove.Length)) });

            if (dc.dt.Rows.Count == 0) return deptApprove;   // ไม่เจอ → แสดงตัวเดียว

            var list = new System.Collections.Generic.List<string>();
            foreach (DataRow r in dc.dt.Rows)
                list.Add(r["dc"].ToString().Trim());

            return string.Join(", ", list);
        }

        // มี dept ที่ 2 ไหม
        private bool HasSecondDept(string Doc_No)
        {
            dc.QryDT_P(conn, "SELECT dept_code_approve2 FROM NotificationofChange_Control_List WHERE Doc_No=@d",
                new SqlParameter("@d", SqlDbType.NVarChar) { Value = Doc_No });
            if (dc.dt.Rows.Count == 0) return false;
            return dc.dt.Rows[0]["dept_code_approve2"] != DBNull.Value
                && !string.IsNullOrEmpty(dc.dt.Rows[0]["dept_code_approve2"].ToString().Trim());
        }

        // ตัวเอง = approved , คนอื่นในขั้นเดียวกันที่ยัง wait = delegated (อ้างจาก status_id ของ approver : 30/31)
        private void SetApproverFlag(SqlTransaction tx, string Doc_No, string empId, int step)
        {
            string st = step == 1 ? "30" : "31";

            using (SqlCommand c = new SqlCommand(
                "UPDATE NotificationofChange_Approver SET approve_flag='approved', approve_date=@dt" +
                " WHERE Doc_No=@doc AND emp_id=@emp AND status_id=@st", conn, tx))
            {
                c.Parameters.AddWithValue("@dt", DateTime.Now);
                c.Parameters.AddWithValue("@doc", Doc_No);
                c.Parameters.AddWithValue("@emp", empId);
                c.Parameters.AddWithValue("@st", st);
                c.ExecuteNonQuery();
            }

            using (SqlCommand c = new SqlCommand(
                "UPDATE NotificationofChange_Approver SET approve_flag='delegated', approve_date=@dt" +
                " WHERE Doc_No=@doc AND emp_id<>@emp AND status_id=@st AND approve_flag='wait'", conn, tx))
            {
                c.Parameters.AddWithValue("@dt", DateTime.Now);
                c.Parameters.AddWithValue("@doc", Doc_No);
                c.Parameters.AddWithValue("@emp", empId);
                c.Parameters.AddWithValue("@st", st);
                c.ExecuteNonQuery();
            }
        }

        // reset approver ของขั้นนั้นกลับ wait
        private void ResetApprover(SqlTransaction tx, string Doc_No, int step)
        {
            using (SqlCommand c = new SqlCommand(
                "UPDATE NotificationofChange_Approver SET approve_flag='wait', approve_date=NULL" +
                " WHERE Doc_No=@doc AND status_id=@st", conn, tx))
            {
                c.Parameters.AddWithValue("@doc", Doc_No);
                c.Parameters.AddWithValue("@st", step == 1 ? "30" : "31");
                c.ExecuteNonQuery();
            }
        }

        // update header : approveStep 1/2 = บันทึกผู้อนุมัติ , 0 + status 30 = ส่งกลับ (ล้างผู้อนุมัติ)
        private void UpdateHeaderStatus(SqlTransaction tx, string Doc_No, int status, string empId, int approveStep)
        {
            string setApv = "";
            if (approveStep == 1) setApv = ", approve_empid=@emp, approve_date=@dt";
            else if (approveStep == 2) setApv = ", approve_empid2=@emp, approve_date2=@dt";
            else if (status == 30 || status == 1) setApv = ", approve_empid=NULL, approve_date=NULL, approve_empid2=NULL, approve_date2=NULL";

            using (SqlCommand c = new SqlCommand(
                "UPDATE NotificationofChange_Control_List SET status_id=@st, update_date=@dt" + setApv +
                " WHERE Doc_No=@doc", conn, tx))
            {
                c.Parameters.AddWithValue("@st", status);
                c.Parameters.AddWithValue("@dt", DateTime.Now);
                if (approveStep == 1 || approveStep == 2)
                    c.Parameters.AddWithValue("@emp", (object)empId ?? DBNull.Value);
                c.Parameters.AddWithValue("@doc", Doc_No);
                c.ExecuteNonQuery();
            }
        }
        // บันทึกเหตุผลลง Reject_Detail_All (แสดงในตาราง ประวัติ Reject / Cancel)
        private void InsertApproveHistory(SqlTransaction tx, string Doc_No, string statusId, string detail, string type)
        {
            int count;
            using (SqlCommand c = new SqlCommand(
                "SELECT ISNULL(MAX(Reject_Count),0)+1 FROM Reject_Detail_All WHERE Doc_No=@d", conn, tx))
            {
                c.Parameters.AddWithValue("@d", Doc_No);
                count = Convert.ToInt32(c.ExecuteScalar());
            }

            string empId = Session["emp_id"] != null ? Session["emp_id"].ToString() : "";
            string name = Session["user_name"] != null ? Session["user_name"].ToString().Split(' ')[0] : "";
            if (detail != null && detail.Length > 1000) detail = detail.Substring(0, 997) + "...";

            using (SqlCommand c = new SqlCommand(@"INSERT INTO Reject_Detail_All
                (Doc_No, Doc_Type, status_id, Reject_Count, emp_id, name, Reject_Date, Reject_detail, Type)
                VALUES (@Doc_No, 'AAD2', @status_id, @cnt, @emp_id, @name, @dt, @detail, @type)", conn, tx))
            {
                c.Parameters.AddWithValue("@Doc_No", Doc_No);
                c.Parameters.AddWithValue("@status_id", statusId);
                c.Parameters.AddWithValue("@cnt", count);
                c.Parameters.AddWithValue("@emp_id", empId);
                c.Parameters.AddWithValue("@name", name);
                c.Parameters.AddWithValue("@dt", DateTime.Now);
                c.Parameters.AddWithValue("@detail", detail ?? "");
                c.Parameters.AddWithValue("@type", type);
                c.ExecuteNonQuery();
            }
        }

        private string BuildReturnDetail(List<ApvDiff> diffs, string comment)
        {
            var sb = new StringBuilder("[Approver 2] Reject : เลือกรายการไม่ตรงกับ Approver 1 (" + diffs.Count + " รายการ) : ");
            sb.Append(string.Join(", ", diffs.Select(d =>
                (d.FixAsset != "" ? d.FixAsset : d.ControlNo) +
                " (A1 " + (d.A1 == "Y" ? "✓" : "✗") + " / A2 " + (d.A2 == "Y" ? "✓" : "✗") + ")").ToArray()));
            if (!string.IsNullOrEmpty(comment)) sb.Append(" | เหตุผล : " + comment);
            sb.Append(" | กรุณาตรวจสอบรายการและจัดทำ Request ใหม่");
            return sb.ToString();
        }

        /// <summary>เปิด Approve Modal ซ้ำ (กรณี validate ไม่ผ่าน)</summary>
        private void ReopenApproveModal()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "reopenApv",
                "document.getElementById('" + pnlApproveModal.ClientID + "').style.display='flex';" +
                "if(typeof apvBindEvents==='function') apvBindEvents();", true);
        }

        /// <summary>แสดง toast แล้ว redirect ไปหน้า Approve Status</summary>
        private void RedirectAfterApprove(string Doc_No, string msg, string type = "success")
        {
            string url = HttpUtility.JavaScriptStringEncode(ResolveUrl("~/ApproveStatus.aspx"));
            string jsDoc = HttpUtility.JavaScriptStringEncode(Doc_No);
            string jsMsg = HttpUtility.JavaScriptStringEncode(msg);

            string script =
                "var m=document.getElementById('" + pnlApproveModal.ClientID + "'); if(m) m.style.display='none';" +
                "lockUI();" +
                "if(typeof showToast==='function'){ showToast('" + jsMsg + "',{title:'Doc " + jsDoc + "',type:'" + type + "',duration:2000}); }" +
                "setTimeout(function(){ window.location.replace('" + url + "'); },2000);";

            ScriptManager.RegisterStartupScript(this, GetType(), "apvRedirect", script, true);
        }

        // ── ล็อก Step 6 : ซ่อนปุ่ม Generate + picker read-only (ลบ/เพิ่มไม่ได้) ──
        private void LockApproverFlow()
        {
            string Doc_No = litIssueNo.Text.Trim();

            // ซ่อนปุ่ม + picker
            pnlFlowNeedGen.Visible = false;
            pnlFlowContent.Visible = false;

            // โชว์ตาราง
            pnlApproverTable.Visible = true;
            BindApproverFlowTable(Doc_No);
        }

        // ลบ (Insert → ลบเลย / มีใน DB → mark Delete)
        private void RemoveOrMark(DataRow r)
        {
            if (GetVal(r, "CompareData") == "Insert")
                r.Delete();
            else
                r["CompareData"] = "Delete";
        }
        // หา PairKey สำหรับแถวใหม่ : เติมคู่ที่มีตัวเดียวก่อน, ไม่มีก็สร้างคู่ใหม่
        private int GetPairKeyForNewRow()
        {
            var counts = new System.Collections.Generic.Dictionary<int, int>();
            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                if (r["PairKey"] == DBNull.Value) continue;
                int pk = Convert.ToInt32(r["PairKey"]);
                if (pk <= 0) continue;
                counts[pk] = counts.ContainsKey(pk) ? counts[pk] + 1 : 1;
            }

            int openKey = -1, maxKey = 0;
            foreach (var kv in counts)
            {
                if (kv.Key > maxKey) maxKey = kv.Key;
                if (kv.Value == 1 && (openKey == -1 || kv.Key < openKey)) openKey = kv.Key;
            }
            return openKey != -1 ? openKey : maxKey + 1;
        }

        // อ่าน StepLabel จาก DataSource ที่ bind ไว้
        private string GetSummaryStepAt(int rowIndex)
        {
            DataTable dt = ViewState["SEND_SUMMARY_DT"] as DataTable;
            if (dt != null && rowIndex >= 0 && rowIndex < dt.Rows.Count)
                return dt.Rows[rowIndex]["StepLabel"].ToString();
            return "";
        }

        private bool CanCancelRequest(string status_id)
        {
            // เงื่อนไข 1 : 1 < status_id < 100
            int st;
            if (!int.TryParse(status_id, out st)) return false;
            if (st <= 1 || st >= 100) return false;

            // เงื่อนไข 2 : admin หรือ เจ้าของเอกสาร (issue_empid = login)
            string loginId = Session["emp_id"] != null ? Session["emp_id"].ToString().Trim() : "";
            string admin = Session["admin"] != null ? Session["admin"].ToString().Trim() : "";
            string issueEmpId = txtEmpId.Text.Trim();

            bool isAdmin = (admin == "99");
            bool isOwner = !string.IsNullOrEmpty(loginId) && loginId == issueEmpId;

            return isAdmin || isOwner;
        }

        /// <summary>render ช่องมาตรฐานของตารางผู้อนุมัติ — ใช้ร่วม gvApproverFlow / gvSendSummary</summary>
        private void RenderApvRowCells(GridViewRow row, DataRowView drv, bool grpStart,
                                       string stepLitId, string deptLabel)
        {
            string step = Convert.ToString(drv["StepLabel"]);
            string empId = Convert.ToString(drv["emp_id"]);
            string name = Convert.ToString(drv["emp_name"]);
            string posCode = Convert.ToString(drv["position_code"]).Trim();
            string posName = Convert.ToString(drv["position_name"]).Trim();
            string tel = Convert.ToString(drv["tel"]).Trim();
            string email = Convert.ToString(drv["email"]).Trim();
            bool isA2 = (step == "Approver 2");

            // ── ขั้น : แสดงเฉพาะแถวแรกของกลุ่ม (+ แผนก ถ้ามี) ──
            Literal litStep = row.FindControl(stepLitId) as Literal;
            if (litStep != null)
                litStep.Text = !grpStart ? "" :
                    "<span class='apv-step " + (isA2 ? "apv-step-2" : "apv-step-1") + "'><i>" +
                    (isA2 ? "2" : "1") + "</i>" + Server.HtmlEncode(step) + "</span>" +
                    (string.IsNullOrEmpty(deptLabel) ? "" :
                     "<span class='apv-step-dept'>แผนก " + Server.HtmlEncode(deptLabel) + "</span>");

            // ── ผู้อนุมัติ ──
            Literal litPerson = row.FindControl("litPerson") as Literal;
            if (litPerson != null)
                litPerson.Text =
                    "<div class='apv-person'>" +
                      "<span class='j3-avatar' style='background:" + AvatarColor(name) + "'>" + Server.HtmlEncode(Initials(name)) + "</span>" +
                      "<div class='apv-person-meta'>" +
                        "<span class='apv-person-name'>" + Server.HtmlEncode(name) + "</span>" +
                        "<span class='apv-person-id'>" + Server.HtmlEncode(empId) + "</span>" +
                      "</div>" +
                    "</div>";

            // ── ตำแหน่ง ──
            Literal litPos = row.FindControl("litPosition") as Literal;
            if (litPos != null)
                litPos.Text =
                    (posCode != "" ? "<span class='apv-pos-code'>" + Server.HtmlEncode(posCode) + "</span>" : "") +
                    (posName != "" ? "<span class='apv-pos-name'>" + Server.HtmlEncode(posName) + "</span>" : "") +
                    (posCode == "" && posName == "" ? "<span class='apv-muted'>—</span>" : "");

            // ── ติดต่อ ──
            Literal litContact = row.FindControl("litContact") as Literal;
            if (litContact != null)
                litContact.Text =
                    "<div class='apv-contact'>" +
                      (tel != ""
                          ? "<span><i class='glyphicon glyphicon-earphone'></i>" + Server.HtmlEncode(tel) + "</span>"
                          : "<span class='apv-muted'><i class='glyphicon glyphicon-earphone'></i>—</span>") +
                      (email != ""
                          ? "<a href='mailto:" + Server.HtmlEncode(email) + "'><i class='glyphicon glyphicon-envelope'></i>" + Server.HtmlEncode(email) + "</a>"
                          : "<span class='apv-muted'><i class='glyphicon glyphicon-envelope'></i>—</span>") +
                    "</div>";
        }
        // เบอร์ภายในจาก sys_user_master
        private string GetUserExt(string empId)
        {
            if (string.IsNullOrEmpty(empId)) return "";
            dc.QryDT_P(conn_user, "SELECT ext FROM sys_user_master WHERE emp_id=@e",
                new SqlParameter("@e", SqlDbType.NVarChar) { Value = empId.Trim() });
            return dc.dt.Rows.Count > 0 ? GetVal(dc.dt.Rows[0], "ext") : "";
        }

        private bool IsTwoDeptDoc() { return ViewState["TWO_DEPT"] is bool && (bool)ViewState["TWO_DEPT"]; }

        private int DocStatusInt()
        {
            int st; int.TryParse(Convert.ToString(ViewState["STATUS_ID"]), out st); return st;
        }

        private bool HasApproveData(DataRow r)
        {
            return ColVal(r, "A1Selected") != "" || ColVal(r, "A2Selected") != "" || ColVal(r, "RejectFlag") == "reject";
        }

        /// ok / ng / waitA2 / waitA1 / review / wait
        private string ItemResultState(DataRow r)
        {
            string a1 = ColVal(r, "A1Selected"), a2 = ColVal(r, "A2Selected"), rf = ColVal(r, "RejectFlag");
            if (a1 == "" && a2 == "" && rf != "reject") return "wait";

            int st = DocStatusInt();
            bool two = IsTwoDeptDoc();

            // รอบส่งกลับจาก A2 (status 30 + มี a2 ค้าง)
            if (st == 30 && a2 != "")
                return (a1 != a2) ? "review" : (a2 == "Y" ? "waitA1" : "ng");

            if (rf == "reject") return "ng";
            if (a1 == "Y" && (!two || a2 == "Y" || st >= 50)) return "ok";
            if (a1 == "Y") return "waitA2";
            return "wait";
        }

        private string BuildItemResultBadge(DataRow r)
        {
            string cls, text;
            switch (ItemResultState(r))
            {
                case "ok": cls = "res-ok"; text = "✓ อนุมัติ"; break;
                case "ng": cls = "res-ng"; text = "✗ ไม่อนุมัติ"; break;
                case "waitA2": cls = "res-wait"; text = "รอ Approver 2"; break;
                case "waitA1": cls = "res-wait"; text = "รอ Approver 1 ยืนยัน"; break;
                case "review": cls = "res-review"; text = "⚠ A1/A2 เลือกไม่ตรงกัน"; break;
                default: cls = "res-none"; text = "รออนุมัติ"; break;
            }

            string steps = PrevMark("A1", ColVal(r, "A1Selected"));
            if (IsTwoDeptDoc()) steps += PrevMark("A2", ColVal(r, "A2Selected"));

            return "<span class='res-badge " + cls + "'>" + text + "</span>" +
                   "<div class='res-steps'>" + steps + "</div>";
        }

        private string BuildItemResultSummary(DataTable dt)
        {
            int ok = 0, ng = 0, other = 0;
            foreach (DataRow r in dt.Rows)
            {
                string s = ItemResultState(r);
                if (s == "ok") ok++; else if (s == "ng") ng++; else other++;
            }
            return "<div class='item-result-summary'><b>ผลการอนุมัติ :</b>" +
                   "<span class='res-badge res-ok'>✓ อนุมัติ " + ok + "</span>" +
                   "<span class='res-badge res-ng'>✗ ไม่อนุมัติ " + ng + "</span>" +
                   (other > 0 ? "<span class='res-badge res-wait'>รอดำเนินการ " + other + "</span>" : "") +
                   (ng > 0 ? "<span style='color:var(--muted);'>— แถวสีแดง = ไม่อนุมัติ (จะไม่ถูกอัปเดต Location)</span>" : "") +
                   "</div>";
        }

        // A2 เลือกไม่ตรงกับ A1 ?
        private bool IsA2Mismatch(HashSet<string> selectedKeys)
        {
            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                bool a1 = GetVal(r, "A1Selected") != "N";
                if (a1 != selectedKeys.Contains(RowKey(r))) return true;
            }
            return false;
        }

        // ✅ ข้อ 5 : โหมดสลับ ต้องอนุมัติ/ไม่อนุมัติ ทั้งคู่
        private bool ValidateApprovePairs(HashSet<string> selectedKeys)
        {
            var map = new SortedDictionary<int, int[]>();   // pk → [total, selected]
            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                int pk = r["PairKey"] == DBNull.Value ? 0 : Convert.ToInt32(r["PairKey"]);
                if (!map.ContainsKey(pk)) map[pk] = new int[2];
                map[pk][0]++;
                if (selectedKeys.Contains(RowKey(r))) map[pk][1]++;
            }

            int disp = 1;
            foreach (var kv in map)
            {
                if (kv.Value[1] != 0 && kv.Value[1] != kv.Value[0])
                {
                    alert_toast("คู่ที่ " + disp + " เลือกไม่ครบคู่ — โหมดสลับต้องอนุมัติ/ไม่อนุมัติทั้งคู่",
                                "ต้องเลือกเป็นคู่", "warn", "10000");
                    return false;
                }
                disp++;
            }
            return true;
        }
        private string BuildLastRejectBanner(string Doc_No)
        {
            dc.QryDT_P(conn,
                "SELECT TOP 1 A.emp_id, A.name, A.Reject_Date, A.Reject_detail, B.status_name" +
                " FROM Reject_Detail_All A LEFT JOIN Master_Status B ON A.status_id = B.status_id" +
                " WHERE A.Doc_No=@d AND A.Type='Reject' ORDER BY A.Reject_Count DESC",
                new SqlParameter("@d", SqlDbType.NVarChar) { Value = Doc_No });
            if (dc.dt.Rows.Count == 0) return "";

            DataRow r = dc.dt.Rows[0];
            string date = r["Reject_Date"] == DBNull.Value ? "" :
                Convert.ToDateTime(r["Reject_Date"]).ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture);

            return "<div class='reject-banner'>" +
                     "<b>⚠️ เอกสารนี้ถูก Reject กลับมา — กรุณาจัดทำข้อมูล Request ใหม่</b>" +
                     "<div>โดย " + Server.HtmlEncode(GetVal(r, "name") + " (" + GetVal(r, "emp_id") + ")") +
                     " · ขั้น " + Server.HtmlEncode(GetVal(r, "status_name")) + " · " + date + "</div>" +
                     "<div class='reject-banner-reason'><b>เหตุผล :</b> " + Server.HtmlEncode(GetVal(r, "Reject_detail")) + "</div>" +
                     "<div class='reject-banner-hint'>ตรวจสอบรายการใน Step 4 แก้ไข / เพิ่ม / ลบรายการตามเหตุผล " +
                     "แล้วกด <b>Send Request</b> เพื่อส่งอนุมัติใหม่</div>" +
                   "</div>";
        }
        private int CountActiveRows()
        {
            int n = 0;
            foreach (DataRow r in ChangeListDT.Rows)
                if (GetVal(r, "CompareData") != "Delete") n++;
            return n;
        }

        /// <summary>Dept ที่ใช้คิด Approve : dept_code = 5505 → 5505 เสมอ , นอกนั้นใช้ dept_code_real</summary>
        private static string ApvDept(string deptCode, string deptCodeReal)
        {
            string dc = (deptCode ?? "").Trim();
            string real = (deptCodeReal ?? "").Trim();
            if (dc == "5505") return "5505";
            return real != "" ? real : dc;
        }

        private string ApvDept(DataRow r)
        {
            return ApvDept(GetVal(r, "DeptCode"), GetVal(r, "DeptCodeReal"));
        }

        /// <summary>ตรวจช่องพิมพ์ Model/Cell/Station : เกิน 50 ตัวอักษร / ชื่อซ้ำกับ Dropdown → เด้งกลับ Dropdown</summary>
        private bool ValidateTypedValues()
        {
            DataTable modelM = (DataTable)ViewState["MODEL_MASTER"];
            DataTable cellM = GetCellMaster().Copy();
            DataTable stationM = GetStationMaster().Copy();

            var tooLong = new List<string>();
            var moved = new List<string>();
            int no = 0;

            foreach (DataRow r in ChangeListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                no++;
                string tag = "รายการที่ " + no + (GetVal(r, "FixAssetNo") != "" ? " (" + GetVal(r, "FixAssetNo") + ")" : "");

                CheckTyped(r, "HD_Model", "Add_Model", "Use_ModelId", "Model", modelM, "model_id", "model_name", tag, tooLong, moved);
                CheckTyped(r, "HD_Cell", "Add_Cell", "Use_CellId", "Cell", cellM, "location2_id", "location2_name", tag, tooLong, moved);
                CheckTyped(r, "HD_Station", "Add_Station", "Use_StationId", "Station", stationM, "location3_id", "location3_name", tag, tooLong, moved);
            }

            if (tooLong.Count > 0)
            {
                BindChangeList_Display();
                alert_toast("ข้อความที่พิมพ์ต้องไม่เกิน " + MAX_LOC_TEXT + " ตัวอักษร :\n" + string.Join("\n", tooLong.ToArray()),
                            "ข้อความยาวเกินกำหนด", "warn", "15000");
                return false;
            }
            if (moved.Count > 0)
            {
                BindChangeList_Display();
                alert_toast("ชื่อที่พิมพ์มีอยู่แล้วใน Dropdown — ระบบเปลี่ยนเป็นเลือกจาก Dropdown ให้แล้ว (ช่องสีเหลือง)\n" +
                            "กรุณาตรวจสอบ แล้วกดบันทึกอีกครั้ง :\n" + string.Join("\n", moved.ToArray()),
                            "ชื่อซ้ำกับรายการที่มีอยู่", "warn", "15000");
                return false;
            }
            return true;
        }

        private void CheckTyped(DataRow r, string hdCol, string txtCol, string idCol, string label,
            DataTable master, string mIdCol, string mNameCol, string tag, List<string> tooLong, List<string> moved)
        {
            if (GetVal(r, hdCol) != "txt") return;

            string v = GetVal(r, txtCol);                       // GetVal = Trim แล้ว
            r[txtCol] = (v == "") ? (object)DBNull.Value : v;

            if (v.Length > MAX_LOC_TEXT)
            {
                tooLong.Add("• " + tag + " " + label + " : " + v.Length + " ตัวอักษร");
                return;
            }
            if (v == "" || master == null) return;

            foreach (DataRow m in master.Rows)
            {
                string name = m[mNameCol] == DBNull.Value ? "" : m[mNameCol].ToString().Trim();
                if (!string.Equals(name, v, StringComparison.OrdinalIgnoreCase)) continue;

                r[hdCol] = "ddl";
                r[idCol] = m[mIdCol].ToString().Trim();
                r[txtCol] = DBNull.Value;
                _hlTyped.Add(RowKey(r) + "|" + label);
                moved.Add("• " + tag + " " + label + " : " + name);
                return;
            }
        }

        /// <summary>ตรวจความยาวรายละเอียดเหตุผล — ใช้ทั้ง Draft และ Send (ก่อนเปิด Modal)</summary>
        private bool ValidateRemarkLength()
        {
            // textarea ส่ง \r\n มา 2 ตัวต่อบรรทัด → ปรับเป็น \n ให้นับตรงกับหน้าจอ
            string v = (txt_ChangeComment.Text ?? "").Replace("\r\n", "\n").Trim();
            txt_ChangeComment.Text = v;

            if (v.Length <= MAX_REASON_REMARK)
            {
                SetWarning_TextBox(txt_ChangeComment, false);
                return true;
            }

            alert_toast("รายละเอียดเหตุผลต้องไม่เกิน " + MAX_REASON_REMARK + " ตัวอักษร (ปัจจุบัน " + v.Length + " ตัวอักษร)",
                        "ข้อความยาวเกินกำหนด", "warn", "10000");
            SetWarning_TextBox(txt_ChangeComment, true);
            txt_ChangeComment.Focus();
            return false;
        }

        /// <summary>แถบนำทาง + ปุ่มคู่มือ สำหรับโหมด Approve / Confirm</summary>
        /// <summary>แถบนำทาง + ปุ่มคู่มือ ตามสถานะ (30 = Approver 1 , 31 = Approver 2 , 50 = AAD Confirm)</summary>
        private string BuildModeGuide(string status_id)
        {
            string file = ManualFileByStatus(status_id);
            if (file == "") return "";

            bool isConfirm = (file == "Confirm");
            string css = isConfirm ? "mode-guide mg-confirm" : "mode-guide";
            string title, btnText;
            string[] steps;

            switch (file)
            {
                case "Approver1":
                    title = "ขั้นตอน Approver 1";
                    btnText = "คู่มือ Approver 1";
                    steps = new[] { "ตรวจรายการร้องขอ",
                            "เหตุผลการร้องขอ",
                            "กด Approve แล้วติ๊กเลือกรายการที่อนุมัติ",
                            "ไม่อนุมัติทั้งฉบับ กด Reject" };
                    break;
                case "Approver2":
                    title = "ขั้นตอน Approver 2";
                    btnText = "คู่มือ Approver 2";
                    steps = new[] { "ตรวจรายการร้องขอ",
                            "ดูผลของ Approver 1 (A1 ✓ / ✗) ในหน้าต่างอนุมัติ",
                            "เลือกตรงกับ A1 = อนุมัติ / ไม่ตรง = Reject กลับผู้ร้องขอ",
                            "ไม่อนุมัติทั้งฉบับ กด Reject" };
                    break;
                default:   // Confirm
                    title = "ขั้นตอน AAD Confirm";
                    btnText = "คู่มือ AAD Confirm";
                    steps = new[] { "ตรวจรายการร้องขอ",
                            "ตรวจผลอนุมัติรายแถว (Step 4)",
                            "กด AAD Confirm หรือ Reject" };
                    break;
            }

            var sb = new StringBuilder();
            sb.Append("<div class='" + css + "'>");
            sb.Append("<span class='mg-title'>" + Server.HtmlEncode(title) + "</span><ol>");
            foreach (string s in steps) sb.Append("<li>" + Server.HtmlEncode(s) + "</li>");
            sb.Append("</ol>");
            sb.Append("<a href='#' class='mg-btn' onclick=\"return openManualFile('" + file + "');\">" +
                      "<span class='glyphicon glyphicon-book'></span> " + Server.HtmlEncode(btnText) + "</a>");
            sb.Append("</div>");
            return sb.ToString();
        }

        /// <summary>ปุ่มคู่มือ Step 1–6 เป็นของ Requester — แสดงเฉพาะโหมด Issue</summary>
        private void SetStepManualVisible(bool show)
        {
            foreach (LinkButton b in new[] { btnManual1, btnManual2, btnManual3, btnManual4, btnManual5, btnManual6 })
                if (b != null) b.Visible = show;
        }










        // ============================================================
        // Update to Database
        // ============================================================




        // --- Update / Insert Withdraw List
        private void InsertBorrowList(string Doc_No, string status_id)
        {
            DataTable dt = ChangeListDT;
            conn.Open();

            try
            {
                foreach (DataRow row in dt.Rows)
                {
                    string sql = @"
                    INSERT INTO NotificationofChange_List
                    (
                        Doc_No , No , FixAsset , ControlNo ,
                        tool_id , sub_id , system_id ,
                        Use_Dept ,
                        Use_Model_Id , New_Model , HD_Model ,
                        Use_Cell_Id , New_Cell , HD_Cell ,
                        Use_Station_Id , New_Station , HD_Station ,
                        status_id
                    )
                    VALUES
                    (
                        @Doc_No , @No , @FixAsset , @ControlNo ,
                        @tool_id , @sub_id , @system_id ,
                        @Use_Dept ,
                        @Use_Model_Id , @New_Model , @HD_Model ,
                        @Use_Cell_Id , @New_Cell , @HD_Cell ,
                        @Use_Station_Id , @New_Station , @HD_Station ,
                        @status_id
                        
                    )";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                        cmd.Parameters.AddWithValue("@No", row["RowNo"]);
                        cmd.Parameters.AddWithValue("@FixAsset", row["FixAssetNo"]);
                        cmd.Parameters.AddWithValue("@ControlNo", row["ControlNo"]);
                        cmd.Parameters.AddWithValue("@tool_id", row["ToolId"]);
                        cmd.Parameters.AddWithValue("@sub_id", row["ToolSub"]);
                        cmd.Parameters.AddWithValue("@system_id", row["SystemId"]);
                        cmd.Parameters.AddWithValue("@Use_Dept", row["Use_DeptCode"]);
                        cmd.Parameters.AddWithValue("@Use_Model_Id", row["Use_ModelId"]);
                        cmd.Parameters.AddWithValue("@New_Model", row["Add_Model"]);
                        cmd.Parameters.AddWithValue("@HD_Model", row["HD_Model"]);
                        cmd.Parameters.AddWithValue("@Use_Cell_Id", row["Use_CellId"]);
                        cmd.Parameters.AddWithValue("@New_Cell", row["Add_Cell"]);
                        cmd.Parameters.AddWithValue("@HD_Cell", row["HD_Cell"]);
                        cmd.Parameters.AddWithValue("@Use_Station_Id", row["Use_StationId"]);
                        cmd.Parameters.AddWithValue("@New_Station", row["Add_Station"]);
                        cmd.Parameters.AddWithValue("@HD_Station", row["HD_Station"]);
                        cmd.Parameters.AddWithValue("@status_id", status_id);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                alert_toast("เกิดข้อผิดพลาด : " + ex.Message, "Error", "error", "10000");
            }
            finally
            {
                string alert_info = "";
                switch (status_id)
                {
                    case "1":
                        alert_info = "Save Draft";
                        break;
                    case "50":
                        alert_info = "Send Complete";
                        break;
                    case "90":
                        alert_info = "Confirm Complete";
                        break;
                    case "99":
                        alert_info = "Update Complete";
                        break;
                    case "999":
                        alert_info = "Cancel Request";
                        break;
                }
                conn.Close(); // ✅ ปิดทุกครั้ง

                string link_path = "EquipmentStatus2.aspx";
                if (status_id == "99") link_path = "ApproveStatus.aspx";

                alert_toast("Doc No : " + Doc_No, alert_info, "info", "10000");


                string script = @"
                    document.getElementById('loadingOverlay').style.display = 'flex';

                    if (typeof alert_toast === 'function') {
                        alert_toast('Doc No : " + Doc_No + @"', 'Save Draft', 'info', '10000');
                    }
                    setTimeout(function () {
                        window.location.href = '" + link_path + @"';
                    }, 2000);
                ";
                //window.location.href = 'EquipmentStatus2.aspx';
                ScriptManager.RegisterStartupScript(
                    this,
                    this.GetType(),
                    "redirectScript",
                    script,
                    true
                );
            }
        }











        // ── Helper : เลือก CSS class ตาม DisplayStatus ────────────────────────
        protected string GetStatusBadgeClass(string displayStatus)
        {
            if (displayStatus.StartsWith("AAD"))
                return "badge-borrowing";   // สีแดง — ติด process

            if (displayStatus.StartsWith("Spare"))
                return "badge-spare";

            return "badge-normal";
        }


        // ── Helper : อ่านค่าจาก DataRow ป้องกัน DBNull ───────────────────────
        private string ColVal(DataRow row, string colName)
        {
            if (!row.Table.Columns.Contains(colName)) return "";
            return row[colName] == DBNull.Value ? "" : row[colName].ToString().Trim();
        }
        // ── Helper : อ่าน string จาก DataRow ป้องกัน DBNull (.NET 4.0) ───────────
        private string GetVal(DataRow row, string colName)
        {
            if (row == null) return "";
            if (!row.Table.Columns.Contains(colName)) return "";
            return row[colName] == DBNull.Value ? "" : row[colName].ToString().Trim();
        }
        // ── Helper : แปลงค่าจาก DataRow → object (DBNull ถ้าว่าง) ───────────────
        private object ToDbVal(DataRow row, string colName)
        {
            if (!row.Table.Columns.Contains(colName)) return DBNull.Value;
            if (row[colName] == DBNull.Value) return DBNull.Value;
            if (string.IsNullOrEmpty(row[colName].ToString().Trim())) return DBNull.Value;
            return row[colName];
        }

        private void BindLocField(GridViewRow row, DataRow cRow, string ddlId, string txtId, string hfId, string btnId,
                                  string hdCol, string idCol, string txtCol, DataTable master, string mText, string mValue, string label)
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
                if (!string.IsNullOrEmpty(v) && ddl.Items.FindByValue(v) != null) ddl.SelectedValue = v;
                ddl.Style["display"] = isTxt ? "none" : "inline-block";
                if (_hlTyped.Contains(RowKey(cRow) + "|" + label)) SetWarning_DDL(ddl, true);
            }
            if (txt != null)
            {
                txt.MaxLength = MAX_LOC_TEXT;
                txt.Text = ColVal(cRow, txtCol);
                txt.Style["display"] = isTxt ? "inline-block" : "none";
            }
            if (hf != null) hf.Value = isTxt ? "txt" : "ddl";
            if (btn != null) btn.InnerText = isTxt ? "-" : "+";
        }
        /// <summary>ไฟล์คู่มือตามสถานะ : 30 = Approver1 , 31 = Approver2 , 50 = Confirm (สถานะอื่นคืน "")</summary>
        private string ManualFileByStatus(string status_id)
        {
            switch ((status_id ?? "").Trim())
            {
                case "30": return "Approver1";
                case "31": return "Approver2";
                case "50": return "Confirm";
                default: return "";
            }
        }



        // ============================================================










        protected void btnHidden_Click(object sender, EventArgs e)
        {

            //// รับค่าที่ส่งมาจาก JavaScript
            //string requestId = Request.Form["__EVENTARGUMENT"];

            //// Register ค่านี้เพื่อบอก ASP.NET ว่าถูกต้อง
            //ClientScript.RegisterForEventValidation(btnHidden.UniqueID, requestId);

            //Requester_getdata_edit();
        }

    }
}