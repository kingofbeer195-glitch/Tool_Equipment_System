using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace Tool_Equipment_Control_System
{
    public partial class History_Status : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        datacon dc = new datacon();

        // เก็บ Tab ที่กำลังเลือกอยู่ (Request / ChangeLocation)
        protected string ActiveTab
        {
            get { return ViewState["ActiveTab"] as string ?? "Request"; }
            set { ViewState["ActiveTab"] = value; }
        }

        // โหมด Filter วันที่ : "Period" (ค่าเริ่มต้น) / "All"
        protected string ReqDateMode
        {
            get { return ViewState["ReqDateMode"] as string ?? "Period"; }
            set { ViewState["ReqDateMode"] = value; }
        }
        protected string CLDateMode
        {
            get { return ViewState["CLDateMode"] as string ?? "Period"; }
            set { ViewState["CLDateMode"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = "History";

            if (Session["emp_id"] == null || Session["emp_id"].ToString() == "")
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                // ค่าเริ่มต้นทั้ง 2 Tab : Period + 7 วันล่าสุด
                SetReqDateMode("Period");   // อิง issue_date
                SetCLDateMode("Period");    // อิง Edit_Date

                Bind_ddlProcess();
                Bind_ddlStatus();
                Bind_ddlDept();

                Bind_HistoryList();
            }

            UpdateTabDisplay();
        }

        // =====================================================
        //  Tab Switching
        // =====================================================
        protected void UpdateTabDisplay()
        {
            bool isRequest = ActiveTab == "Request";

            pnlTabRequest.Visible = isRequest;
            pnlTabChangeLocation.Visible = !isRequest;

            btnTabRequest.CssClass = isRequest ? "tab-btn active" : "tab-btn";
            btnTabChangeLocation.CssClass = !isRequest ? "tab-btn active" : "tab-btn";
        }
        protected void btnTabRequest_Click(object sender, EventArgs e)
        {
            ActiveTab = "Request";
            UpdateTabDisplay();
        }
        protected void btnTabChangeLocation_Click(object sender, EventArgs e)
        {
            ActiveTab = "ChangeLocation";
            UpdateTabDisplay();
            Bind_ChangeLocationList();
        }

        // =====================================================
        //  Bind DropDownLists
        // =====================================================
        protected void Bind_ddlProcess()
        {
            string str = "Select document_id , document_name + ' : ' + Comment as Request_Type" +
                         " From Master_Document Where status_use = '1' and document_id != 'AAD5'";
            dc.QryDT(conn, str);

            ddlProcess.DataSource = dc.dt;
            ddlProcess.DataTextField = "Request_Type";
            ddlProcess.DataValueField = "document_id";
            ddlProcess.DataBind();
            ddlProcess.Items.Insert(0, new ListItem("-- ทั้งหมด --", ""));
        }
        protected void Bind_ddlStatus()
        {
            string str = "Select * From Master_Status Where status_use = '1' order by status_id ";
            dc.QryDT(conn, str);

            ddlStatus.DataSource = dc.dt;
            ddlStatus.DataTextField = "status_name";
            ddlStatus.DataValueField = "status_id";
            ddlStatus.DataBind();
            ddlStatus.Items.Insert(0, new ListItem("-- ทั้งหมด --", ""));
        }
        protected void Bind_ddlDept()
        {
            string str = "SELECT dept_code, dept_name + ' (' + TRIM(dept_code) + ')' as dept_name" +
                         " FROM View_dept_master" +
                         " Order By dept_name";
            dc.QryDT(conn, str);

            ddlDept.DataSource = dc.dt;
            ddlDept.DataTextField = "dept_name";
            ddlDept.DataValueField = "dept_code";
            ddlDept.DataBind();
            ddlDept.Items.Insert(0, new ListItem("-- ทั้งหมด --", ""));
        }


        #region -- Tab : History Request
        // =====================================================
        //  Tab : History Request
        // =====================================================

        // ---  Bind History List  (ดึงทั้งหมดจาก View ตรงๆ ไม่ผูกสิทธิ์)
        protected void Bind_HistoryList()
        {
            DataTable dt_History = null;

            try
            {
                string keyword = txtSearchRequest.Text.Trim();
                List<string> docNos = null; // เก็บ Doc_No ที่เจอจาก View_Equipment_Request_List_All (อาจมีหลายรายการ)

                // ===== STEP 1: ถ้ามี keyword ให้ค้นหาใน View_Equipment_Request_List_All ก่อน =====
                if (!string.IsNullOrEmpty(keyword))
                {
                    string kw = keyword.Replace("'", "''");
                    string strAll = "SELECT DISTINCT Doc_No FROM View_Equipment_Request_List_All " +
                                     "WHERE FixAsset LIKE '%" + kw + "%' " +
                                     "OR ControlNo LIKE '%" + kw + "%' " +
                                     "OR Tool_No LIKE '%" + kw + "%'";
                    dc.QryDT(conn, strAll);

                    if (dc.dt == null || dc.dt.Rows.Count == 0)
                    {
                        alert_toast("ไม่พบข้อมูล Fix Asset / Control No / Tool No : " + keyword, "ค้นหา", "warn", "4000");
                        gvHistoryList.DataSource = null;
                        gvHistoryList.DataBind();
                        lblTotal.Text = "0";
                        return; // จบเลย ไม่ต้อง query ต่อ
                    }

                    docNos = new List<string>();
                    foreach (DataRow r in dc.dt.Rows)
                    {
                        string doc = SafeStr(r["Doc_No"]);
                        if (doc != "")
                            docNos.Add("'" + doc.Replace("'", "''") + "'");
                    }
                }

                // ===== Filter เดิม (Process / Status / Dept / Date) =====
                string where = "";

                if (!string.IsNullOrEmpty(ddlProcess.SelectedValue))
                    where += " AND V.document_id = '" + ddlProcess.SelectedValue.Replace("'", "''") + "'";

                if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                    where += " AND V.status_id = '" + ddlStatus.SelectedValue.Replace("'", "''") + "'";

                if (!string.IsNullOrEmpty(ddlDept.SelectedValue))
                    where += " AND V.dept_code = '" + ddlDept.SelectedValue.Replace("'", "''") + "'";

                // ===== Filter EmpId (ค้นหาใน View_Equipment_Request_List) =====
                if (!string.IsNullOrEmpty(txtSearchEmpId.Text.Trim()))
                    where += " AND V.issue_empid LIKE '%" + txtSearchEmpId.Text.Trim().Replace("'", "''") + "%'";

                // ===== Filter Issue Date : เฉพาะโหมด Period (All = ไม่สนใจวันที่) =====
                if (ReqDateMode == "Period")
                {
                    DateTime dFrom, dTo;
                    if (DateTime.TryParse(txtDateFrom.Text.Trim(), out dFrom))
                        where += " AND V.issue_date >= '" + dFrom.ToString("yyyy-MM-dd") + " 00:00:00'";
                    if (DateTime.TryParse(txtDateTo.Text.Trim(), out dTo))
                        where += " AND V.issue_date <= '" + dTo.ToString("yyyy-MM-dd") + " 23:59:59'";
                }

                // ===== STEP 2: ถ้ามี Doc_No จาก step 1 ให้ filter ด้วย IN (...) =====
                if (docNos != null)
                    where += " AND V.Doc_No IN (" + string.Join(",", docNos) + ")";

                // ===== STEP 3: Query เหมือนเดิมที่ View_Equipment_Request_List =====
                string str = "SELECT V.* FROM View_Equipment_Request_List V WHERE 1=1 " + where +
                             " ORDER BY V.issue_date DESC, V.Doc_No";

                dc.QryDT(conn, str);
                dt_History = dc.dt;

                if (docNos != null && (dt_History == null || dt_History.Rows.Count == 0))
                {
                    alert_toast("พบข้อมูลอุปกรณ์ แต่ไม่พบประวัติการทำรายการที่เกี่ยวข้อง", "ค้นหา", "warn", "4000");
                }
            }
            catch (Exception ex)
            {
                alert_toast("เกิดข้อผิดพลาด : " + ex.Message, "Error", "error", "10000");
            }

            gvHistoryList.DataSource = dt_History;
            gvHistoryList.DataBind();

            lblTotal.Text = (dt_History == null ? 0 : dt_History.Rows.Count).ToString();
        }


        // ---  Filter Events (Tab 1 : History Request)
        protected void ddlProcess_SelectedIndexChanged(object sender, EventArgs e) { Bind_HistoryList(); }
        protected void ddlStatus_SelectedIndexChanged(object sender, EventArgs e) { Bind_HistoryList(); }
        protected void ddlDept_SelectedIndexChanged(object sender, EventArgs e) { Bind_HistoryList(); }
        protected void btnSearch_Click(object sender, EventArgs e) { Bind_HistoryList(); }
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ddlProcess.ClearSelection();
            ddlStatus.ClearSelection();
            ddlDept.ClearSelection();
            SetReqDateMode("Period");   // กลับเป็น Period + 7 วัน
            txtSearchRequest.Text = "";
            Bind_HistoryList();
        }
        protected void btnHistClose_Click(object sender, EventArgs e)
        {           
            pnlHistoryModal.Visible = false;
        }
        

        // ---  GridView Events (Tab 1)
        protected void gvHistoryList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // History เป็น read-only ไม่มีปุ่ม Confirm/Return
            // เผื่ออนาคตจะปรับแต่ง row ตาม status ใส่ logic ตรงนี้ได้
        }

        protected void gvHistoryList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string Doc_No = e.CommandArgument.ToString();
            string encrypted = HttpUtility.UrlEncode(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Doc_No)));
            string prefix = Doc_No.Contains("-") ? Doc_No.Split('-')[0] : "";

            if (e.CommandName == "ViewRow")
            {
                string targetUrl = "";

                if (prefix == "AAD1")
                    targetUrl = "borrow_tool2.aspx?k=" + encrypted + "&mode=view";
                else if (prefix == "AAD2")
                    targetUrl = "NotificationOfChange.aspx?k=" + encrypted + "&mode=view";
                else if (prefix == "AAD3")
                    targetUrl = "withdraw2.aspx?k=" + encrypted + "&mode=view";
                else if (prefix == "AAD4")
                    targetUrl = "storage.aspx?k=" + encrypted + "&mode=view";

                if (!string.IsNullOrEmpty(targetUrl))
                {
                    string jsUrl = HttpUtility.JavaScriptStringEncode(targetUrl);
                    string key = "openNewTab_" + DateTime.UtcNow.Ticks;
                    string script = "window.open('" + jsUrl + "', '_blank');";

                    ScriptManager.RegisterStartupScript(this, this.GetType(), key, script, true);
                }
            }

            if (e.CommandName == "HistoryRow")
            {
                if (prefix == "AAD1")
                {
                    ShowHistory_AAD1(Doc_No, prefix);
                }
                else if (prefix == "AAD2")
                {
                    ShowHistory_AAD2(Doc_No, prefix);
                }
                else if (prefix == "AAD3")
                {
                    ShowHistory_AAD3(Doc_No, prefix);
                }
                else if (prefix == "AAD4")
                {
                    ShowHistory_AAD4(Doc_No, prefix);
                }

            }

            if (e.CommandName == "ViewItems")
            {
                if (prefix == "AAD1")
                    ShowItems_AAD1(Doc_No);
                else if (prefix == "AAD3")
                    ShowItems_AAD3(Doc_No);
            }

            if (e.CommandName == "ViewIssuer")
            {
                string[] data = e.CommandArgument.ToString().Split('|');

                string issue_empid = data[0];
                string doc_no = data[1];

                ShowIssuer(issue_empid, doc_no);
            }
        }


        #endregion

        #region -- Tab : History Change Location
        // =====================================================
        //  Tab : History Change Location
        // =====================================================

        #endregion

        #region -- Modal : Issue Name
        // ---  Modal : Issue Name
        protected void ShowIssuer(string empId, string Doc_No)
        {
            empId = (empId ?? "").Trim();
            lblIsId.Text = empId;

            bool ok = GetIssuerHRMS(empId, Doc_No);

            if (!ok)
            {
                // เคลียร์ค่าเก่า + แสดง error
                lblIsName.Text = lblIsPos.Text = lblIsBand.Text =
                lblIsDept.Text = lblIsDeptCode.Text = lblIsEmail.Text = lblIsTel.Text = "-";
                lblIsErr.Visible = true;
            }
            else
            {
                lblIsErr.Visible = false;
            }

            ScriptManager.RegisterStartupScript(
                this, GetType(), "showIssuerModal",
                "$('#modal_Issuer').modal('show');", true);
        }

        // ดึง HRMS → bind เข้า label ของ modal (แยกจากตัวเดิมที่ใช้กับฟอร์มเพิ่มข้อมูล)
        private bool GetIssuerHRMS(string userId, string Doc_No)
        {
            try
            {
                string prefix = Doc_No.Contains("-") ? Doc_No.Split('-')[0] : "";
                string str_phone = "";

                if (prefix == "AAD1")
                {
                    str_phone = "Select phone From Borrow_Control_List where Doc_No ='" + Doc_No + "'";
                }
                else if (prefix == "AAD2")
                {
                    str_phone = "Select phone From NotificationofChange_Control_List where Doc_No ='" + Doc_No + "'";
                }
                else if (prefix == "AAD3")
                {
                    str_phone = "Select phone From Withdraw_Control_List where Doc_No ='" + Doc_No + "'";
                }
                else if (prefix == "AAD4")
                {

                }
                dc.QryDT(conn, str_phone);
                DataTable dt_phone = dc.dt;

                lblIsTel.Text = dt_phone.Rows[0]["phone"].ToString();

                string url = "http://cptsvs547:1000/middleware/oracle/hrms";
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                string sql = "select GNAME_ENG, FNAME_ENG, POSN_ENAME, BAND, DEPT_CODE, DEPT_ABB_NAME, EMAIL " +
                             "from ADMIN.v_emp_data_all_cpt where UPPER(emp_no)=UPPER('" + userId.Replace("'", "''") + "')";

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

                    lblIsName.Text = (u.GNAME_ENG + " " + u.FNAME_ENG).ToString().Trim();
                    lblIsPos.Text = SafeS(u.POSN_ENAME);
                    lblIsBand.Text = SafeS(u.BAND);
                    lblIsDept.Text = SafeS(u.DEPT_ABB_NAME);
                    lblIsDeptCode.Text = SafeS(u.DEPT_CODE);
                    lblIsEmail.Text = SafeS(u.EMAIL);
                    return true;
                }
            }
            catch (Exception ex)
            {
                lblIsErr.Text = "เกิดข้อผิดพลาดในการดึงข้อมูล: " + ex.Message;
                return false;
            }
        }
        #endregion

        #region -- Modal : History Time Line >> History Request
        // --- History Modal
        protected void ShowHistory_AAD1(string Doc_No, string prefix)
        {
            // ดึง record จาก View เดิม (1 แถวต่อ Doc_No)
            string str = "SELECT * FROM View_Equipment_Request_List " +
                         "WHERE Doc_No = '" + Doc_No.Replace("'", "''") + "'";
            dc.QryDT(conn, str);

            if (dc.dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูล History ของ " + Doc_No, "History", "warn", "5000");
                return;
            }

            DataRow r = dc.dt.Rows[0];
            lblHistDocNo.Text = Doc_No;

            int curStatus = Convert.ToInt32(r["status_id"]);
            bool isCancel = curStatus == 999; // ปรับตาม id ของ Cancel จริง

            var sb = new StringBuilder();
            sb.Append("<ul class='timeline'>");

            // Stage 1 : Issue_Draft
            AppendStage(sb, "Issue (Draft)",
                r["issue_empid"], r["issue_date"],
                true, "เริ่มต้นคำขอ");

            // Stage 2 : Send Request
            AppendStage(sb, "Send Request",
                r["issue_empid"], r["send_date"],
                true, "ส่งคำร้องขอ");

            // Stage 3 : Confirm (AAD)
            AppendStage(sb, "AAD Confirm",
                r["process50_empid"], GetCol(r, "process50_date", "process50_date"),
                HasValue(GetCol(r, "process50_date", "process50_date")), "อนุมัติคำขอ");

            // Stage 4 : Return / Completed
            object completed = HasCol(r, "completed_date") ? r["completed_date"] : DBNull.Value;
            object retEmp = HasCol(r, "return_empid") ? r["return_empid"] : DBNull.Value;
            AppendStage(sb, "Return / Completed",
                retEmp, completed,
                HasValue(completed), "คืนของ / ปิดงาน");

            // ถ้า Cancel ให้แสดงสถานะยกเลิกท้ายสุด
            if (isCancel)
            {
                sb.Append("<li class='tl-item'>");
                sb.Append("<span class='tl-dot cancel'></span>");
                sb.Append("<div class='tl-title' style='color:#e74c3c;'>❌ Cancelled</div>");
                sb.Append("<div class='tl-meta'>คำขอถูกยกเลิก</div>");
                sb.Append("</li>");
            }

            sb.Append("</ul>");
            litTimeline.Text = sb.ToString();

            pnlHistoryModal.Visible = true;
        }
        protected void ShowHistory_AAD2(string Doc_No, string prefix)
        {
            // ดึง record จาก View เดิม (1 แถวต่อ Doc_No)
            string str = "SELECT * FROM View_Equipment_Request_List " +
                         "WHERE Doc_No = '" + Doc_No.Replace("'", "''") + "'";
            dc.QryDT(conn, str);

            if (dc.dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูล History ของ " + Doc_No, "History", "warn", "5000");
                return;
            }

            DataRow r = dc.dt.Rows[0];
            lblHistDocNo.Text = Doc_No;

            int curStatus = Convert.ToInt32(r["status_id"]);
            bool isCancel = curStatus == 999; // ปรับตาม id ของ Cancel จริง

            var sb = new StringBuilder();
            sb.Append("<ul class='timeline'>");

            // Stage 1 : Issue_Draft
            AppendStage(sb, "Issue (Draft)",
                r["issue_empid"], r["issue_date"],
                true, "เริ่มต้นคำขอ");

            // Stage 2 : Send Request
            AppendStage(sb, "Send Request",
                r["issue_empid"], r["send_date"],
                true, "ส่งคำร้องขอ");

            // Stage 3 : J3 Up Approve (Leader Team : Requester)
            AppendStage(sb, "J3 Up Approve (Requester)",
                r["process20_empid"], GetCol(r, "process20_date", "process20_date"),
                HasValue(GetCol(r, "process20_date", "process20_date")), "อนุมัติการร้องขอ");

            // Stage 4 : Confirm (AAD)
            AppendStage(sb, "AAD Confirm",
                r["confirm_empid"], GetCol(r, "confim_date", "confirm_date"),
                HasValue(GetCol(r, "confim_date", "confirm_date")), "อนุมัติคำขอ");

            // Stage 5 : Update Data
            object completed = HasCol(r, "completed_date") ? r["completed_date"] : DBNull.Value;
            //object retEmp = HasCol(r, "return_empid") ? r["return_empid"] : DBNull.Value;
            AppendStage(sb, "Update Location",
                "Auto", completed,
                HasValue(completed), "ระบบอัพเดตข้อมูล");

            // ถ้า Cancel ให้แสดงสถานะยกเลิกท้ายสุด
            if (isCancel)
            {
                sb.Append("<li class='tl-item'>");
                sb.Append("<span class='tl-dot cancel'></span>");
                sb.Append("<div class='tl-title' style='color:#e74c3c;'>❌ Cancelled</div>");
                sb.Append("<div class='tl-meta'>คำขอถูกยกเลิก</div>");
                sb.Append("</li>");
            }

            sb.Append("</ul>");
            litTimeline.Text = sb.ToString();

            pnlHistoryModal.Visible = true;
        }
        protected void ShowHistory_AAD3(string Doc_No, string prefix)
        {
            // ดึง record จาก View เดิม (1 แถวต่อ Doc_No)
            string str = "SELECT * FROM View_Equipment_Request_List " +
                         "WHERE Doc_No = '" + Doc_No.Replace("'", "''") + "'";
            dc.QryDT(conn, str);

            if (dc.dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูล History ของ " + Doc_No, "History", "warn", "5000");
                return;
            }

            DataRow r = dc.dt.Rows[0];
            lblHistDocNo.Text = Doc_No;

            int curStatus = Convert.ToInt32(r["status_id"]);
            bool isCancel = curStatus == 999; // ปรับตาม id ของ Cancel จริง

            var sb = new StringBuilder();
            sb.Append("<ul class='timeline'>");

            // Stage 1 : Issue_Draft
            AppendStage(sb, "Issue (Draft)",
                r["issue_empid"], r["issue_date"],
                true, "เริ่มต้นคำขอ");

            // Stage 2 : Send Request
            AppendStage(sb, "Send Request",
                r["issue_empid"], r["send_date"],
                true, "ส่งคำร้องขอ");

            // Stage 3 : Confirm (AAD / J3)
            AppendStage(sb, "AAD Confirm",
                r["process50_empid"], GetCol(r, "process50_date", "process50_date"),
                HasValue(GetCol(r, "process50_date", "process50_date")), "อนุมัติคำขอ");

            // Stage 4 : Update Location
            object completed = HasCol(r, "completed_date") ? r["completed_date"] : DBNull.Value;
            //object retEmp = HasCol(r, "return_empid") ? r["return_empid"] : DBNull.Value;
            AppendStage(sb, "Update Location",
                "Auto", completed,
                HasValue(completed), "ระบบอัพเดตข้อมูล");

            // ถ้า Cancel ให้แสดงสถานะยกเลิกท้ายสุด
            if (isCancel)
            {
                sb.Append("<li class='tl-item'>");
                sb.Append("<span class='tl-dot cancel'></span>");
                sb.Append("<div class='tl-title' style='color:#e74c3c;'>❌ Cancelled</div>");
                sb.Append("<div class='tl-meta'>คำขอถูกยกเลิก</div>");
                sb.Append("</li>");
            }

            sb.Append("</ul>");
            litTimeline.Text = sb.ToString();

            pnlHistoryModal.Visible = true;
        }
        protected void ShowHistory_AAD4(string Doc_No, string prefix)
        {
            // ดึง record จาก View เดิม (1 แถวต่อ Doc_No)
            string str = "SELECT * FROM View_Equipment_Request_List " +
                         "WHERE Doc_No = '" + Doc_No.Replace("'", "''") + "'";
            dc.QryDT(conn, str);

            if (dc.dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูล History ของ " + Doc_No, "History", "warn", "5000");
                return;
            }

            DataRow r = dc.dt.Rows[0];
            lblHistDocNo.Text = Doc_No;

            int curStatus = Convert.ToInt32(r["status_id"]);
            bool isCancel = curStatus == 999; // ปรับตาม id ของ Cancel จริง

            var sb = new StringBuilder();
            sb.Append("<ul class='timeline'>");

            // Stage 1 : Issue_Draft
            AppendStage(sb, "Issue (Draft)",
                r["issue_empid"], r["issue_date"],
                true, "เริ่มต้นคำขอ");

            // Stage 2 : Send Request
            AppendStage(sb, "Send Request",
                r["issue_empid"], r["send_date"],
                true, "ส่งคำร้องขอ");

            // Stage 3 : AAD Check Process10 , status_id = 10
            AppendStage(sb, "AAD Check",
                r["process10_empid"], GetCol(r, "process10_date", "process10_date"),
                HasValue(GetCol(r, "process10_date", "process10_date")), "ยืนยันรายการ");

            // Stage 4 : J3 Up (AAD) Process20 , status_id = 20
            AppendStage(sb, "J3 Up Approve (AAD)",
                r["process20_empid"], GetCol(r, "process20_date", "process20_date"),
                HasValue(GetCol(r, "process20_date", "process20_date")), "อนุมัติรายการร้องขอ");

            // Stage 5 : Genba Check Process25 , status_id = 25
            AppendStage(sb, "ตรวจสอบรายการร้องขอ",
                r["process25_empid1"], GetCol(r, "process25_date", "process25_date"),
                HasValue(GetCol(r, "process25_date", "process25_date")), "ยืนยันรายการ");

            // Stage 6 : J3 Up (Requester) Process30 , status_id = 30
            AppendStage(sb, "J3 Up Approve (Requester)",
                r["process30_empid"], GetCol(r, "process30_date", "process30_date"),
                HasValue(GetCol(r, "process30_date", "process30_date")), "อนุมัติรายการ");

            // Stage 6 : AAD Confirm Process50 , status_id = 50
            AppendStage(sb, "AAD Confirm",
                r["process50_empid"], GetCol(r, "process50_date", "process50_date"),
                HasValue(GetCol(r, "process50_date", "process50_date")), "ยืนยันรายการ");

            // Stage 7 : Update Location
            object completed = HasCol(r, "completed_date") ? r["completed_date"] : DBNull.Value;
            //object retEmp = HasCol(r, "return_empid") ? r["return_empid"] : DBNull.Value;
            AppendStage(sb, "Update Location",
                "Auto", completed,
                HasValue(completed), "ระบบอัพเดตข้อมูล");

            // ถ้า Cancel ให้แสดงสถานะยกเลิกท้ายสุด
            if (isCancel)
            {
                sb.Append("<li class='tl-item'>");
                sb.Append("<span class='tl-dot cancel'></span>");
                sb.Append("<div class='tl-title' style='color:#e74c3c;'>❌ Cancelled</div>");
                sb.Append("<div class='tl-meta'>คำขอถูกยกเลิก</div>");
                sb.Append("</li>");
            }

            sb.Append("</ul>");
            litTimeline.Text = sb.ToString();

            pnlHistoryModal.Visible = true;
        }
        #endregion

        #region -- Helper --
        // helper: append 1 stage
        private void AppendStage(StringBuilder sb, string title, object empid, object date, bool done, string desc)
        {
            bool hasDate = HasValue(date);
            string dotClass = hasDate ? "done" : "wait";
            string itemClass = hasDate ? "tl-item" : "tl-item tl-pending";

            sb.Append("<li class='" + itemClass + "'>");
            sb.Append("<span class='tl-dot " + dotClass + "'></span>");
            sb.Append("<div class='tl-title'>" + (hasDate ? "✔ " : "○ ") + HttpUtility.HtmlEncode(title) + "</div>");

            string emp = empid == null || empid == DBNull.Value ? "" : empid.ToString().Trim();
            if (emp != "")
                sb.Append("<div class='tl-meta'>👤 " + HttpUtility.HtmlEncode(emp) + " &nbsp;|&nbsp; " + HttpUtility.HtmlEncode(desc) + "</div>");
            else
                sb.Append("<div class='tl-meta'>" + HttpUtility.HtmlEncode(desc) + "</div>");

            if (hasDate)
                sb.Append("<div class='tl-date'>📅 " + Convert.ToDateTime(date).ToString("dd-MMM-yyyy HH:mm") + "</div>");
            else
                sb.Append("<div class='tl-date' style='color:#aaa;'>— รอดำเนินการ —</div>");

            sb.Append("</li>");
        }
        private bool HasValue(object o)
        {
            return o != null && o != DBNull.Value && o.ToString().Trim() != "";
        }
        private bool HasCol(DataRow r, string col)
        {
            return r.Table.Columns.Contains(col);
        }
        private object GetCol(DataRow r, string col1, string col2)
        {
            if (r.Table.Columns.Contains(col1) && HasValue(r[col1])) return r[col1];
            if (r.Table.Columns.Contains(col2) && HasValue(r[col2])) return r[col2];
            return DBNull.Value;
        }
        #endregion


        protected void btnCLHistClose_Click(object sender, EventArgs e)
        {
            pnlCLHistoryModal.Visible = false;
        }


        // =====================================================
        //  Bind History Change Location List (Tab 2)
        // =====================================================
        protected void Bind_ChangeLocationList()
        {
            DataTable dt_CL = null;

            try
            {
                string where = "";

                // ค้นหาจาก Fix_Asset_No / Control_No / Tool_No
                if (!string.IsNullOrEmpty(txtSearchCL.Text.Trim()))
                {
                    string kw = txtSearchCL.Text.Trim().Replace("'", "''");
                    where += " AND (H.Fix_Asset_No LIKE '%" + kw + "%'" +
                             " OR H.Control_No LIKE '%" + kw + "%'" +
                             " OR H.Tool_No LIKE '%" + kw + "%')";
                }

                // Filter Edit By
                if (!string.IsNullOrEmpty(txtEditBy.Text.Trim()))
                    where += " AND H.Edit_By LIKE '%" + txtEditBy.Text.Trim().Replace("'", "''") + "%'";

                // Filter Edit Date : เฉพาะโหมด Period (All = ไม่สนใจวันที่)
                if (CLDateMode == "Period")
                {
                    DateTime dFrom, dTo;

                    if (DateTime.TryParse(txtCLDateFrom.Text.Trim(), out dFrom))
                        where += " AND H.Edit_Date >= '" + dFrom.ToString("yyyy-MM-dd") + " 00:00:00'";

                    if (DateTime.TryParse(txtCLDateTo.Text.Trim(), out dTo))
                        where += " AND H.Edit_Date <= '" + dTo.ToString("yyyy-MM-dd") + " 23:59:59'";
                }

                string str = "SELECT H.*, " +
                             " SO.status_name AS Old_Status_Name, SN.status_name AS New_Status_Name" +
                             " FROM tool_eduipment_system.dbo.History_Change_Location H" +
                             " LEFT JOIN tool_equipment_status SO ON H.Old_Status_Id = SO.status_id" +
                             " LEFT JOIN tool_equipment_status SN ON H.New_Status_Id = SN.status_id" +
                             " WHERE 1 = 1 " + where +
                             " ORDER BY H.Edit_Date DESC, H.History_id DESC";

                dc.QryDT(conn, str);
                dt_CL = dc.dt;
            }
            catch (Exception ex)
            {
                alert_toast("เกิดข้อผิดพลาด : " + ex.Message, "Error", "error", "10000");
            }

            gvChangeLocationList.DataSource = dt_CL;
            gvChangeLocationList.DataBind();

            lblTotalCL.Text = (dt_CL == null ? 0 : dt_CL.Rows.Count).ToString();
        }

        protected void gvChangeLocationList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewCLHistory")
            {
                string[] parts = e.CommandArgument.ToString().Split('|');
                if (parts.Length == 3)
                    ShowCLHistory(parts[0], parts[1], parts[2]);
            }
        }

        private string GetColStr(DataRow r, string col)
        {
            return (r.Table.Columns.Contains(col) && r[col] != DBNull.Value) ? r[col].ToString().Trim() : "";
        }

        // =====================================================
        //  Filter Events (Tab 2 : History Change Location)
        // =====================================================
        protected void btnSearchCL_Click(object sender, EventArgs e) { Bind_ChangeLocationList(); }

        protected void btnClearCL_Click(object sender, EventArgs e)
        {
            txtSearchCL.Text = "";
            txtEditBy.Text = "";
            SetCLDateMode("Period");    // กลับเป็น Period + 7 วัน
            Bind_ChangeLocationList();
        }


        // =====================================================
        //  Date Mode Toggle : Period | All (ทั้ง 2 Tab)
        // =====================================================
        protected void btnReqPeriod_Click(object sender, EventArgs e) { SetReqDateMode("Period"); Bind_HistoryList(); }
        protected void btnReqAll_Click(object sender, EventArgs e)    { SetReqDateMode("All");    Bind_HistoryList(); }
        protected void btnCLPeriod_Click(object sender, EventArgs e)  { SetCLDateMode("Period");  Bind_ChangeLocationList(); }
        protected void btnCLAll_Click(object sender, EventArgs e)     { SetCLDateMode("All");     Bind_ChangeLocationList(); }

        // Period : เปิดช่องวันที่ + รีเซ็ตเป็น 7 วันล่าสุด / All : ปิดช่องวันที่
        private void SetReqDateMode(string mode)
        {
            ReqDateMode = mode;
            ApplyDateMode(mode, btnReqPeriod, btnReqAll, txtDateFrom, txtDateTo);
        }
        private void SetCLDateMode(string mode)
        {
            CLDateMode = mode;
            ApplyDateMode(mode, btnCLPeriod, btnCLAll, txtCLDateFrom, txtCLDateTo);
        }
        private void ApplyDateMode(string mode, LinkButton btnPeriod, LinkButton btnAll, TextBox txtFrom, TextBox txtTo)
        {
            bool isPeriod = mode == "Period";

            btnPeriod.CssClass = isPeriod ? "dm-btn active" : "dm-btn";
            btnAll.CssClass = isPeriod ? "dm-btn" : "dm-btn active";

            txtFrom.Enabled = isPeriod;
            txtTo.Enabled = isPeriod;

            if (isPeriod)
            {
                txtFrom.Text = DateTime.Today.AddDays(-7).ToString("dd-MMM-yyyy");
                txtTo.Text = DateTime.Today.ToString("dd-MMM-yyyy");
            }
        }


        // =====================================================
        //  Item List Modal
        // =====================================================
        protected void ShowItems_AAD1(string Doc_No)
        {
            lblItemsDocNo.Text = Doc_No;

            string str =
                " SELECT ROW_NUMBER() OVER(ORDER BY A.No) as No, A.FixAsset, A.ControlNo, A.tool_id, A.sub_id, A.system_id , B.toolName, B.tool_no " +
                " FROM Borrow_List A " +
                " left join Tool_Equipment_List_View B on A.tool_id = B.tool_id AND A.sub_id = B.sub_no AND A.system_id = B.sys_status " +
                " WHERE Doc_No = '" + Doc_No.Replace("'", "''") + "' " +
                " ORDER BY No";
            dc.QryDT(conn, str);

            BindItemsRepeater();

            ScriptManager.RegisterStartupScript(
                this, GetType(), "showItemsModal",
                "$('#modal_ItemsList').modal('show');", true);
        }
        protected void ShowItems_AAD3(string Doc_No)
        {
            lblItemsDocNo.Text = Doc_No;

            string str =
                " SELECT ROW_NUMBER() OVER(ORDER BY A.No) as No, A.FixAsset, A.ControlNo, A.tool_id, A.sub_id, A.system_id , B.toolName, B.tool_no " +
                " FROM Withdraw_List A " +
                " left join Tool_Equipment_List_View B on A.tool_id = B.tool_id AND A.sub_id = B.sub_no AND A.system_id = B.sys_status " +
                " WHERE Doc_No = '" + Doc_No.Replace("'", "''") + "' " +
                " ORDER BY No";
            dc.QryDT(conn, str);

            BindItemsRepeater();

            ScriptManager.RegisterStartupScript(
                this, GetType(), "showItemsModal",
                "$('#modal_ItemsList').modal('show');", true);
        }

        // รวม logic bind repeater + PicUrl ไว้ที่เดียว (เดิม AAD1/AAD3 ซ้ำกัน)
        private void BindItemsRepeater()
        {
            if (dc.dt.Rows.Count == 0)
            {
                rptItems.DataSource = null;
                rptItems.DataBind();
                lblNoItems.Visible = true;
                return;
            }

            lblNoItems.Visible = false;

            DataTable dt = dc.dt.Copy();
            if (!dt.Columns.Contains("PicUrl"))
                dt.Columns.Add("PicUrl", typeof(string));

            foreach (DataRow r in dt.Rows)
            {
                r["PicUrl"] = GetPicUrl(
                    SafeStr(r["tool_id"]),
                    SafeStr(r["sub_id"]),
                    SafeStr(r["system_id"]));
            }

            rptItems.DataSource = dt;
            rptItems.DataBind();
        }

        private string GetPicUrl(string tool_id, string tool_sub, string sys_status)
        {
            try
            {
                var fsp = new Get_Folder_Structure_Picture(tool_id, tool_sub, sys_status);
                string physicalFolder = fsp.get_folder_root();
                string webPath = fsp.get_folder_root_view_toolMain();

                if (Directory.Exists(physicalFolder))
                {
                    string[] files = Directory.GetFiles(physicalFolder, "*.jpg", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                        return webPath;
                }
            }
            catch { }

            return "images/FilesType/jpeg.png";
        }

        private string SafeStr(object o)
        {
            return (o == null || o == DBNull.Value) ? "" : o.ToString().Trim();
        }

        // =====================================================
        //  Requester Detail Modal
        // =====================================================
        
        private string SafeS(dynamic o)
        {
            return o == null ? "-" : o.ToString().Trim();
        }

        // =====================================================
        //  Helpers
        // =====================================================
        protected void alert_toast(string detail, string title, string type, string showtime)
        {
            if (type == "warn" || type == "error" || type == "success" || type == "info")
            {
                int durationMs;
                if (!int.TryParse(showtime, out durationMs))
                    durationMs = 3000;

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

        private string FormatDate(string val)
        {
            DateTime d;
            return DateTime.TryParse(val, out d) ? d.ToString("dd-MMM-yyyy") : val;
        }

        protected string GetStatusClass(string status)
        {
            switch (status.Trim())
            {
                case "Wait Issue": return "status-draft";
                case "J3 Up Approve": return "status-approve";
                case "AAD Confirm": return "status-confirm";
                case "During Borrow": return "status-borrow";
                case "Completed": return "status-completed";
                case "Cancel": return "status-cancel";
                default: return "";
            }
        }


        // =====================================================
        //  History Change Location : Diff helpers
        // =====================================================

        // { Label, Old column, New column }
        private static readonly string[][] LocFields =
        {
    new[] { "Model",   "Old_Model_Name",   "New_Model_Text"   },
    new[] { "Cell",    "Old_Cell_Name",    "New_Cell_Text"    },
    new[] { "Station", "Old_Station_Name", "New_Station_Text" }
};

        private static readonly string[][] OtherFields =
        {
    new[] { "Status",      "Old_Status_Name", "New_Status_Name" },
    new[] { "Dept Code",   "Old_DeptCode",    "New_DeptCode"    },
    new[] { "Responsible", "Old_Responsible", "New_Responsible" },
    new[] { "Due Date",    "Old_DueDate",     "New_DueDate"     },
    new[] { "Remark",      "Old_Remark",      "New_Remark"      }
};

        private DataRow ToRow(object dataItem)
        {
            DataRowView drv = dataItem as DataRowView;
            return drv != null ? drv.Row : dataItem as DataRow;
        }

        private string Enc(string s) { return HttpUtility.HtmlEncode(s ?? ""); }

        // ค่าว่าง -> "-" สีจาง
        protected string Txt(object o)
        {
            string s = SafeStr(o);
            return s == "" ? "<span class='cl-muted'>-</span>" : Enc(s);
        }

        // Location : แสดง path เดิม / ใหม่ และไฮไลต์เฉพาะ segment ที่เปลี่ยน
        protected string BuildLocationDiff(object dataItem)
        {
            DataRow r = ToRow(dataItem);
            if (r == null) return "";

            int n = LocFields.Length;
            string[] olds = new string[n];
            string[] news = new string[n];
            bool[] chg = new bool[n];
            bool anyChange = false;

            for (int i = 0; i < n; i++)
            {
                olds[i] = GetColStr(r, LocFields[i][1]);
                news[i] = GetColStr(r, LocFields[i][2]);
                chg[i] = olds[i] != news[i];
                if (chg[i]) anyChange = true;
            }

            var sb = new StringBuilder("<div class='cl-loc'>");

            if (!anyChange)
            {
                sb.Append("<span class='cl-tag cl-tag-same'>คงเดิม</span>");
                sb.Append(BuildPath(olds, null, ""));
            }
            else
            {
                sb.Append("<span class='cl-tag cl-tag-old'>เดิม</span>");
                sb.Append(BuildPath(olds, chg, "is-old"));
                sb.Append("<span class='cl-tag cl-tag-new'>ใหม่</span>");
                sb.Append(BuildPath(news, chg, "is-new"));
            }

            sb.Append("</div>");
            return sb.ToString();
        }

        private string BuildPath(string[] vals, bool[] chg, string chgClass)
        {
            var sb = new StringBuilder("<div class='cl-path'>");
            for (int i = 0; i < vals.Length; i++)
            {
                if (i > 0) sb.Append("<span class='cl-sep'>›</span>");

                string cls = "cl-seg";
                if (chg != null && chg[i]) cls += " " + chgClass;
                string title = " title='" + LocFields[i][0] + "'";

                if (vals[i] == "")
                    sb.Append("<span class='" + cls + " is-empty'" + title + ">ว่าง</span>");
                else
                    sb.Append("<span class='" + cls + "'" + title + ">" + Enc(vals[i]) + "</span>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        // ฟิลด์อื่น : แสดงเฉพาะที่เปลี่ยน  เดิม ➜ ใหม่
        protected string BuildFieldDiff(object dataItem)
        {
            DataRow r = ToRow(dataItem);
            if (r == null) return "";

            var sb = new StringBuilder();
            int count = 0;

            foreach (string[] f in OtherFields)
            {
                string o = GetColStr(r, f[1]);
                string nw = GetColStr(r, f[2]);

                if (f[0] == "Due Date") { o = FormatDate(o); nw = FormatDate(nw); }
                if (o == nw) continue;

                count++;
                sb.Append("<div class='cl-fd-row'><span class='cl-fd-k'>" + Enc(f[0]) + "</span><span class='cl-fd-v'>");

                if (o == "")
                    sb.Append("<span class='cl-add'>+ เพิ่ม</span><span class='cl-new'>" + Enc(nw) + "</span>");
                else if (nw == "")
                    sb.Append("<span class='cl-old'>" + Enc(o) + "</span><span class='cl-arrow'>➜</span><span class='cl-empty'>ว่าง</span>");
                else
                    sb.Append("<span class='cl-old'>" + Enc(o) + "</span><span class='cl-arrow'>➜</span><span class='cl-new'>" + Enc(nw) + "</span>");

                sb.Append("</span></div>");
            }

            if (count == 0)
                return "<span class='cl-none'>— ไม่มีการเปลี่ยนแปลงอื่น —</span>";

            return "<div>" + sb.ToString() + "</div>";
        }

        // หมายเหตุของการแก้ไข (คอลัมน์ Remark ของ History)
        protected string BuildNote(object dataItem)
        {
            DataRow r = ToRow(dataItem);
            if (r == null) return "";
            string remark = GetColStr(r, "Remark");
            return remark == "" ? "" : "<div class='cl-note'>📝 " + Enc(remark) + "</div>";
        }

        // =====================================================
        //  Modal : History Change Location (Timeline)
        // =====================================================
        protected void ShowCLHistory(string tool_id, string tool_sub, string system_id)
        {
            tool_id = (tool_id ?? "").Trim();
            tool_sub = (tool_sub ?? "").Trim();
            system_id = (system_id ?? "").Trim();

            DataTable dt_CLHist = null;

            try
            {
                string str = "SELECT H.*, " +
                             " SO.status_name AS Old_Status_Name, SN.status_name AS New_Status_Name" +
                             " FROM tool_eduipment_system.dbo.History_Change_Location H" +
                             " LEFT JOIN tool_equipment_status SO ON H.Old_Status_Id = SO.status_id" +
                             " LEFT JOIN tool_equipment_status SN ON H.New_Status_Id = SN.status_id" +
                             " WHERE H.Tool_id = '" + tool_id.Replace("'", "''") + "'" +
                             " AND H.Tool_sub = '" + tool_sub.Replace("'", "''") + "'" +
                             " AND H.System_id = '" + system_id.Replace("'", "''") + "'" +
                             " ORDER BY H.Edit_Date DESC, H.History_id DESC";
                dc.QryDT(conn, str);
                dt_CLHist = dc.dt;
            }
            catch (Exception ex)
            {
                alert_toast("เกิดข้อผิดพลาดในการดึงประวัติ : " + ex.Message, "Error", "error", "10000");
            }

            // หัว Modal : แสดง FA / CT / TL (ถ้าไม่มีให้ fallback เป็น id)
            string header = tool_id + " / " + tool_sub + " / " + system_id;
            if (dt_CLHist != null && dt_CLHist.Rows.Count > 0)
            {
                DataRow last = dt_CLHist.Rows[0];
                var parts = new List<string>();
                string fa = GetColStr(last, "Fix_Asset_No");
                string ct = GetColStr(last, "Control_No");
                string tl = GetColStr(last, "Tool_No");
                if (fa != "") parts.Add("FA " + fa);
                if (ct != "") parts.Add("CT " + ct);
                if (tl != "") parts.Add("TL " + tl);
                if (parts.Count > 0) header = string.Join(" · ", parts.ToArray());
            }
            lblCLHistDocNo.Text = Enc(header);

            var sb = new StringBuilder();

            if (dt_CLHist == null || dt_CLHist.Rows.Count == 0)
            {
                sb.Append("<div class='no-data' style='text-align:center;color:#8a95a1;padding:24px;'>ไม่พบประวัติการเปลี่ยนแปลงของรายการนี้</div>");
            }
            else
            {
                sb.Append("<ul class='timeline'>");

                int total = dt_CLHist.Rows.Count;
                int i = 0;
                foreach (DataRow r in dt_CLHist.Rows)
                {
                    int seq = total - i;     // แถวแรก (ล่าสุด) = ครั้งที่ total
                    bool isLatest = (i == 0);
                    i++;

                    sb.Append("<li class='tl-item'>");
                    sb.Append("<span class='tl-dot " + (isLatest ? "" : "done") + "'></span>");
                    sb.Append("<div class='tl-title'>ครั้งที่ " + seq + " · แก้ไขโดย " + Enc(GetColStr(r, "Edit_By")) +
                              (isLatest ? " <span class='cl-tag cl-tag-new' style='margin-left:6px;'>ล่าสุด</span>" : "") +
                              "</div>");
                    sb.Append("<div class='tl-date'>" + Convert.ToDateTime(r["Edit_Date"]).ToString("dd-MMM-yyyy HH:mm") + "</div>");

                    sb.Append("<div class='cl-tl-box'>");
                    sb.Append("<div class='cl-sub'>Location (Model › Cell › Station)</div>");
                    sb.Append(BuildLocationDiff(r));
                    sb.Append("<div class='cl-sub'>ข้อมูลอื่นที่เปลี่ยน</div>");
                    sb.Append(BuildFieldDiff(r));
                    sb.Append(BuildNote(r));
                    sb.Append("</div>");

                    sb.Append("</li>");
                }

                sb.Append("</ul>");
            }

            litCLTimeline.Text = sb.ToString();
            pnlCLHistoryModal.Visible = true;
        }
    }
}
