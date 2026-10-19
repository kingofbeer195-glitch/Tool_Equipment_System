using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tool_Equipment_Control_System
{
    public partial class ApproveStatus : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        SqlConnection conn_jig = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        datacon dc = new datacon();

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = "Approve";

            if (Session["emp_id"] == null || Session["emp_id"].ToString() == "")
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                //txtDateTo.Text = DateTime.Today.ToString("dd-MMM-yyyy");
                //txtDateFrom.Text = DateTime.Today.AddDays(-14).ToString("dd-MMM-yyyy");
                SetViewMode("APPROVE");          // ✅ เริ่มต้นหน้า = Approve เสมอ
                Bind_ddlProcess();
                Bind_ddlStatus();
                Apply_StatusFromQuery();
                Bind_ddlDept();
                Requester_getdata();
                Check_Authorize();

                //Bind_RequestList();   
            }
        }

        // --- Check Permission Authorize
        protected void Check_Authorize()
        {
            string emp_id = txtEmpId.Text.Trim();
            //string empName = FormatName(txtEmpName.Text.Trim());

            if (pnlNewEmp.Visible != true)
            {
                try
                {
                    string str = "Select * From Authorize_list Where emp_id = '" + emp_id + "'";
                    dc.QryDT(conn, str);

                    DataTable dt = dc.dt;

                    // ✅ เช็คว่าไม่มีข้อมูล
                    if (dt == null || dt.Rows.Count == 0)
                    {
                        //alert_toast(emp_id + ": ไม่มีสิทธิสำหรับ Approve", "Authoriry Mistake", "info", "5000");
                        //Response.Redirect("http://cptsvs552.canon-pt.co.th/Tool_Equipment_Control_System/MenuList2.aspx");

                        string url = "http://cptsvs552.canon-pt.co.th/Tool_Equipment_Control_System/MenuList2.aspx";

                        string script = @"
                            alert('" + emp_id + @" : ไม่มีสิทธิสำหรับ Approve');
                            window.location='" + url + @"';
                        ";

                        ClientScript.RegisterStartupScript(this.GetType(), "alertRedirect", script, true);
                    }
                }
                catch (Exception ex)
                {
                    alert_toast("เกิดข้อผิดพลาด : " + ex.Message, "Error", "error", "10000");
                }
            }
        }

        // รับ ?status=50 / 90 / 99 จาก Status Card ใน MenuList
        private void Apply_StatusFromQuery()
        {
            string st = Request.QueryString["status"];
            if (!string.IsNullOrEmpty(st))
            {
                ListItem item = ddlStatus.Items.FindByValue(st.Trim());
                if (item != null) { ddlStatus.ClearSelection(); item.Selected = true; }
            }

            // ✅ Filter Request Type
            string doc = Request.QueryString["doc"];
            if (!string.IsNullOrEmpty(doc))
            {
                ListItem item = ddlProcess.Items.FindByValue(doc.Trim());
                if (item != null) { ddlProcess.ClearSelection(); item.Selected = true; }
            }
        }

        // --- Role (Admin / User) & View Mode (Approve / All)
        private bool IsAdmin
        {
            get { return ViewState["IS_ADMIN"] != null && (bool)ViewState["IS_ADMIN"]; }
        }
        private bool IsAllMode
        {
            // User ปกติบังคับเป็น Approve เสมอ
            get { return IsAdmin && Convert.ToString(ViewState["VIEW_MODE"]) == "ALL"; }
        }

        // ตรวจสิทธิ Admin จาก Equipment_Admin (level_1 = True) แล้วแสดง/ซ่อน Toggle
        private void ApplyUserRole()
        {
            string emp_id = txtEmpId.Text.Trim().Replace("'", "''");
            bool isAdmin = false;

            try
            {
                string str = "SELECT emp_id FROM Equipment_Admin " +
                             "WHERE LTRIM(RTRIM(emp_id)) = '" + emp_id + "' AND level_1 = 1";
                dc.QryDT(conn, str);
                isAdmin = dc.dt != null && dc.dt.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                alert_toast("ตรวจสอบสิทธิ Admin ไม่สำเร็จ : " + ex.Message, "Error", "error", "10000");
            }

            ViewState["IS_ADMIN"] = isAdmin;
            pnlViewToggle.Visible = isAdmin;   // ✅ User ปกติ → Hide Toggle

            if (!isAdmin)
                SetViewMode("APPROVE");
        }

        private void SetViewMode(string mode)
        {
            ViewState["VIEW_MODE"] = mode;
            btnModeApprove.CssClass = (mode == "APPROVE") ? "vt-btn active" : "vt-btn";
            btnModeAll.CssClass = (mode == "ALL") ? "vt-btn active" : "vt-btn";
        }

        protected void btnModeApprove_Click(object sender, EventArgs e)
        {
            SetViewMode("APPROVE");
            Bind_RequestList();
        }
        protected void btnModeAll_Click(object sender, EventArgs e)
        {
            SetViewMode("ALL");
            Bind_RequestList();
        }

        // เงื่อนไขโหมด Approve
        private string BuildApproveCondition(string empSafe)
        {
            List<string> conds = new List<string>();

            // 1.2.1 Admin : รายการที่ Admin ต้องดำเนินการ
            if (IsAdmin)
            {
                conds.Add(
                    "    (V.document_id = 'AAD1' AND V.status_id IN (50, 90)) " +
                    " OR (V.document_id = 'AAD2' AND V.status_id IN (50, 99)) " +
                    " OR (V.document_id = 'AAD3' AND V.status_id IN (50, 99)) " +
                    " OR (V.document_id = 'AAD4' AND V.status_id IN (10, 50, 99)) ");
            }

            // 1.2.2 Approver (ทุกคน รวมถึง Admin ที่มีสิทธิ Approve ด้วย)
            //   AAD2 : status 30 = ขั้น A1 (approve_step 1), status 31 = ขั้น A2 (approve_step 2)
            conds.Add(
                " V.document_id = 'AAD2' AND V.status_id IN (30, 31) " +
                " AND EXISTS ( SELECT 1 FROM View_NotiChange_ApproverCheck N " +
                "              WHERE N.Doc_No = V.Doc_No " +
                "                AND N.emp_id = '" + empSafe + "' " +
                "                AND ( (V.status_id = 30 AND N.approve_step = 1) " +
                "                   OR (V.status_id = 31 AND N.approve_step = 2) ) ) ");

            // TODO (NOTE) : AAD4 สำหรับ User ที่ไม่ใช่ Admin (J3 Up Approve / Genba Check)
            //               ยังไม่ทำในรอบนี้ — เพิ่ม conds.Add(...) ตรงนี้ภายหลัง

            return "(" + string.Join(") OR (", conds.ToArray()) + ")";
        }


        // --- Requeter , User
        protected void Requester_getdata()
        {
            string emp_id = Session["emp_id"].ToString();
            string str_dept = "SELECT * FROM View_dept_master where dept_code='" + emp_id + "'";
            dc.QryDT(conn, str_dept);

            if (dc.dt.Rows.Count > 0)
            {
                ViewState["OGN_DEPTCODE"] = dc.dt.Rows[0]["dept_code"].ToString().Trim();
                ViewState["OGN_DEPTNAME"] = dc.dt.Rows[0]["dept_name"].ToString().Trim();
                //txtDept.Text = dc.dt.Rows[0]["dept_name2"].ToString();

                //alert_toast("กรุณากรอกรหัสพนักงาน [Employee ID]", "Requester Alert", "warn", "10000");
                pnlNewEmp.Visible = true;
                lblempNotice.Text = "กรุณากรอกรหัสพนักงาน [Employee ID]";
            }
            else
            {

                // ใส่ค่า emp_id ลง textbox
                txtEmpId.Text = emp_id;

                // ✅ ดึงข้อมูลจาก HRMS (C#)
                bool found = GetDataHRMS_CSharp(emp_id);

                if (!found)
                {
                    pnlNewEmp.Visible = true;
                    ClearEmployeeUI();
                    lblempNotice.Text = "ไม่พบ Employee ID นี้ในระบบ";
                    txtEmpId.Style["background-color"] = "#ff000082";
                    return;
                }

                // ✅ เก็บค่าไว้ใช้ต่อ
                ViewState["EMP_ID"] = txtEmpId.Text;
                ViewState["EMP_NAME"] = txtEmpName.Text;
                ViewState["EMP_POSITION"] = txtPosition.Text;
                ViewState["EMP_DEPT"] = txtDept.Text;
                ViewState["EMP_DEPTCODE"] = txtDeptCode.Text;
                ViewState["EMP_BAND"] = hdtxtBand.Value;
                ViewState["EMP_EMAIL"] = hdtxtEmail.Value;

                txtEmpId.ReadOnly = true;

                ApplyUserRole();      // ✅ เพิ่ม
                Bind_RequestList();
            }
        }
        protected void Requester_getdata_edit()
        {
            string emp_id = txtEmpId.Text;
            string dept_code = ViewState["OGN_DEPTCODE"].ToString();
            string dept_name = ViewState["OGN_DEPTNAME"].ToString();

            if (txtEmpName.Text == "")
            {
                pnlNewEmp.Visible = true;
                lblempNotice.Text = "ขออภัยไม่พบ EmployeeID นี้ในระบบ";
                txtEmpId.Style["background-color"] = "#ff000082";
                return;
            }

            if (txtDept.Text != dept_name)
            {
                pnlNewEmp.Visible = true;
                lblempNotice.Text = "ขออภัยพบ EmployeeID นี้ Dept Code ไม่ตรงกับในระบบ [ Dept Code Login(" + dept_name + ") ≠ Dept Code EmployeeID ในระบบ (" + txtDept.Text + ") ]";
                txtEmpId.Style["background-color"] = "#ff000082";
                return;
            }

            pnlNewEmp.Visible = false;
            ApplyUserRole();      // ✅ เพิ่ม
            Bind_RequestList();
        }
        protected void txtEmpId_TextChanged(object sender, EventArgs e)
        {
            string empId = txtEmpId.Text.Trim();

            if (empId.Length < 6)
            {
                ClearEmployeeUI();
                return;
            }

            // ✅ ดึงข้อมูลจาก HRMS (C#)
            bool found = GetDataHRMS_CSharp(empId);

            if (!found)
            {
                pnlNewEmp.Visible = true;
                ClearEmployeeUI();
                lblempNotice.Text = "ไม่พบ Employee ID นี้ในระบบ";
                txtEmpId.Style["background-color"] = "#ff000082";
                return;
            }

            // ✅ เก็บค่าไว้ใช้ต่อ
            ViewState["EMP_ID"] = txtEmpId.Text;
            ViewState["EMP_NAME"] = txtEmpName.Text;
            ViewState["EMP_POSITION"] = txtPosition.Text;
            ViewState["EMP_DEPT"] = txtDept.Text;
            ViewState["EMP_BAND"] = hdtxtBand.Value;
            ViewState["EMP_EMAIL"] = hdtxtEmail.Value;

            // ✅ ตรวจ business logic
            Requester_getdata_edit();

        }
        private bool GetDataHRMS_CSharp(string userId)
        {
            string url = "http://cptsvs547:1000/middleware/oracle/hrms";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

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
        private void ClearEmployeeUI()
        {
            txtEmpName.Text = "";
            txtPosition.Text = "";
            txtDept.Text = "";
            txtDeptCode.Text = "";
            hdtxtBand.Value = "";
            hdtxtEmail.Value = "";
        }
        private void ShowEmployeeUI()
        {
            txtEmpName.Text = ViewState["EMP_NAME"].ToString();
            txtPosition.Text = ViewState["EMP_POSITION"].ToString();
            txtDept.Text = ViewState["EMP_DEPT"].ToString();
            txtDeptCode.Text = ViewState["EMP_DEPTCODE"].ToString();
            hdtxtBand.Value = ViewState["EMP_BAND"].ToString();
            hdtxtEmail.Value = ViewState["EMP_EMAIL"].ToString();
        }

        // --- Data Request List
        protected void Bind_RequestList()
        {
            string emp_id = txtEmpId.Text.Trim();
            DataTable dt_Approve_List = null;

            if (pnlNewEmp.Visible != true)
            {
                string str = "";
                string where = "";

                try
                {
                    if (!string.IsNullOrEmpty(ddlProcess.SelectedValue))
                        where += " AND V.document_id = '" + ddlProcess.SelectedValue + "'";

                    if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                        where += " AND V.status_id = '" + ddlStatus.SelectedValue + "'";

                    if (!string.IsNullOrEmpty(ddlDept.SelectedValue))
                        where += " AND V.dept_code = '" + ddlDept.SelectedValue + "'";

                    // ✅ Keyword : FixAsset / ControlNo
                    string keyword = txtKeyword.Text.Trim();
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        List<string> docNos = SearchDocNoByKeyword(keyword);

                        if (docNos.Count == 0)
                        {
                            // ไม่เจอ → เคลียร์ Grid + แจ้งเตือน
                            gvApporveList.DataSource = null;
                            gvApporveList.DataBind();
                            lblTotal.Text = "0";
                            alert_toast("ไม่พบรายการที่ตรงกับ \"" + keyword + "\" (FixAsset / ControlNo)",
                                        "ไม่พบข้อมูล", "warn", "5000");
                            return;
                        }

                        string inList = string.Join("','",
                            docNos.Select(d => d.Replace("'", "''")).ToArray());

                        where += " AND V.Doc_No IN ('" + inList + "') ";
                    }

                    string empSafe = emp_id.Replace("'", "''");

                    string modeWhere = IsAllMode
                        ? " V.status_id > 1 AND V.status_id < 100 "     // 1.3 All : Monitoring งานระหว่างดำเนินการ
                        : BuildApproveCondition(empSafe);               // 1.2 Approve

                    str = "SELECT V.* " +
                          " FROM View_Equipment_Request_List V " +
                          " WHERE (" + modeWhere + ") " + where +
                          " ORDER BY V.Doc_No";

                    dc.QryDT(conn, str);
                    dt_Approve_List = dc.dt;
                }
                catch (Exception ex)
                {
                    alert_toast("เกิดข้อผิดพลาด : " + ex.Message, "Error", "error", "10000");
                }
            }

            gvApporveList.DataSource = dt_Approve_List;
            gvApporveList.DataBind();

            // ✅ อัพเดตจำนวนรายการ
            int cnt = (dt_Approve_List == null) ? 0 : dt_Approve_List.Rows.Count;
            lblTotal.Text = cnt.ToString();

            // ✅ เจอ Doc_No แต่ไม่มีสิทธิ Approve → ไม่มีแถวใน Grid
            if (cnt == 0 && !string.IsNullOrEmpty(txtKeyword.Text.Trim()))
            {
                alert_toast("พบรายการ แต่ไม่อยู่ในสิทธิ์ Approve ของคุณ",
                            "ไม่พบรายการที่ต้องดำเนินการ", "info", "5000");
            }
        }
        protected void Bind_ddlProcess()
        {
            string str = "Select * From Master_Document Where status_use = '1' and document_id != 'AAD5'";
            dc.QryDT(conn, str);

            ddlProcess.DataSource = dc.dt;
            ddlProcess.DataTextField = "document_name";
            ddlProcess.DataValueField = "document_id";
            ddlProcess.DataBind();

            // เพิ่ม -- Select --
            ddlProcess.Items.Insert(0, new ListItem("-- ทั้งหมด --", ""));
        }
        protected void Bind_ddlStatus()
        {
            string emp_id = txtEmpId.Text.Trim();

            string str = "Select * From Master_Status Where status_use = '1'";

            //string str = " Select A.emp_id , A.document_id , A.status_id ,B.status_name" +
            //             " From Authorize_list A" +
            //             " Left Join Master_Status B on A.status_id = B.status_id" +
            //             " where A.emp_id = '" + emp_id + "' And B.status_use=1";
            dc.QryDT(conn, str);

            ddlStatus.DataSource = dc.dt;
            ddlStatus.DataTextField = "status_name";
            ddlStatus.DataValueField = "status_id";
            ddlStatus.DataBind();

            // เพิ่ม -- Select --
            ddlStatus.Items.Insert(0, new ListItem("-- ทั้งหมด --", ""));
        }
        protected void Bind_ddlDept()
        {
            //string emp_id = txtEmpId.Text.Trim();

            string str = "SELECT dept_code, dept_name + ' (' + TRIM(dept_code) + ')' as dept_name" +
                         " FROM View_dept_master" +
                         " Order By dept_name";
            dc.QryDT(conn, str);

            ddlDept.DataSource = dc.dt;
            ddlDept.DataTextField = "dept_name";
            ddlDept.DataValueField = "dept_code";
            ddlDept.DataBind();

            // เพิ่ม -- Select --
            ddlDept.Items.Insert(0, new ListItem("-- ทั้งหมด --", ""));
        }

        // --- Select Change
        protected void ddlProcess_SelectedIndexChanged(object sender, EventArgs e)
        {
            Bind_RequestList();
        }
        protected void ddlStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            Bind_RequestList();
        }
        protected void ddlDept_SelectedIndexChanged(object sender, EventArgs e)
        {
            Bind_RequestList();
        }


        protected void gvApporveList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // ✅ ดึงค่า status จาก DataRowView
                DataRowView drv = (DataRowView)e.Row.DataItem;
                int status_id = Convert.ToInt32(drv["status_id"]);

                // ✅ FindControl ต้องใช้กับ e.Row เท่านั้น
                Button btnConfirm = (Button)e.Row.FindControl("btnConfirm");
                Button btnApprove = (Button)e.Row.FindControl("btnApprove");
                Button btnReturn = (Button)e.Row.FindControl("btnReturn");
                Button btnUpdate = (Button)e.Row.FindControl("btnUpdate");
                //Button btnView = (Button)e.Row.FindControl("btnView");
                HyperLink btnView = (HyperLink)e.Row.FindControl("btnView");
                Label lbNextStatus = (Label)e.Row.FindControl("lb_nextStatus");

                // ✅ Default
                btnConfirm.Visible = false;
                btnReturn.Visible = true;

                // ENG Confirm
                if (status_id == 5)
                {
                    btnConfirm.Visible = true;
                    btnApprove.Visible = false;
                    btnReturn.Visible = false;
                }
                // AAD Check
                else if (status_id == 10)
                {
                    btnConfirm.Visible = true;
                    btnApprove.Visible = false;
                    btnReturn.Visible = false;
                }
                // J3 Up Approve -- AAD
                else if (status_id == 20)
                {
                    btnConfirm.Visible = false;
                    btnApprove.Visible = true;
                    btnReturn.Visible = false;
                }
                // Genba Check
                else if (status_id == 25)
                {
                    btnConfirm.Visible = false;
                    btnApprove.Visible = true;
                    btnReturn.Visible = false;
                }
                // J3 Up Approve -- Requester
                else if (status_id == 30 || status_id == 31)
                {
                    btnConfirm.Visible = false;
                    btnApprove.Visible = true;
                    btnReturn.Visible = false;
                }
                // AAD Confirm
                else if (status_id == 50)
                {
                    btnConfirm.Visible = true;
                    btnApprove.Visible = false;
                    btnReturn.Visible = false;
                }
                // Wait Return
                else if (status_id == 90)
                {
                    btnConfirm.Visible = false;
                    btnApprove.Visible = false;
                    btnReturn.Visible = true;
                }
                else
                {
                    btnConfirm.Visible = false;
                    btnApprove.Visible = false;
                    btnReturn.Visible = false;
                }

                // ✅ Wait Update (99) : เบิก (AAD3) / จัดเก็บ (AAD4) → ปุ่ม Update (Admin)
                string docPrefix = Convert.ToString(drv["Doc_No"]).Split('-')[0];
                if (btnUpdate != null)
                {
                    btnUpdate.Visible = status_id == 99 && (docPrefix == "AAD3" || docPrefix == "AAD4") &&
                                        Session["admin"] != null && Session["admin"].ToString().Trim() == "99";
                    if (btnUpdate.Visible) { btnConfirm.Visible = false; btnApprove.Visible = false; btnReturn.Visible = false; }
                }

                // ✅ โหมด All (View Mode) : เหลือแค่ปุ่ม View
                btnView.Visible = IsAllMode;
                if (IsAllMode)
                {
                    if (btnUpdate != null) btnUpdate.Visible = false;
                    btnConfirm.Visible = false;
                    btnApprove.Visible = false;
                    btnReturn.Visible = false;
                    btnView.NavigateUrl = GetViewUrl(drv["Doc_No"].ToString());
                }

                // ✅ กรณี status_id = 90
                if (status_id == 90)
                {
                    //DataRowView drv = (DataRowView)e.Row.DataItem;

                    // ✅ ดึง return_duedate
                    if (drv["return_duedate"] != DBNull.Value)
                    {
                        DateTime dueDate = Convert.ToDateTime(drv["return_duedate"]);
                        DateTime now = DateTime.Now;

                        TimeSpan diff = dueDate - now;

                        string remainText;

                        if (diff.TotalSeconds > 0)
                        {
                            remainText = string.Format("{0} วัน", diff.Days);
                        }
                        else
                        {
                            remainText = "เลยกำหนดแล้ว";
                        }

                        // ✅ แสดงผล
                        lbNextStatus.Text =
                            "กำหนดคืน : " + dueDate.ToString("dd-MMM-yy") +
                            "<br/>เหลือเวลา: " + remainText;

                        if (diff.TotalDays <= 3 && diff.TotalDays >= 0)
                        {
                            lbNextStatus.ForeColor = System.Drawing.Color.Orange;
                        }
                        else if (diff.TotalDays < 0)
                        {
                            lbNextStatus.ForeColor = System.Drawing.Color.Red;
                        }
                    }
                    else
                    {
                        lbNextStatus.Text = "-";
                    }
                }

            }
        }
        protected void gvApporveList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string Doc_No = e.CommandArgument.ToString();
            string encrypted = HttpUtility.UrlEncode(Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes(Doc_No)));
            // ✅ split เอาส่วนหน้า
            string prefix = Doc_No.Contains("-") ? Doc_No.Split('-')[0] : "";

            if (e.CommandName == "UpdateRow")
            {
                if (prefix == "AAD3")
                    Response.Redirect("withdraw2.aspx?k=" + encrypted + "&mode=update");
                else if (prefix == "AAD4")
                    Response.Redirect("storage.aspx?k=" + encrypted + "&mode=update");
                return;
            }

            if (e.CommandName == "ConfirmRow")
            {
                if (prefix == "AAD1")
                {
                    Response.Redirect("borrow_tool2.aspx?k=" + encrypted + "&mode=confirm");
                }
                else if (prefix == "AAD2")
                {
                    Response.Redirect("NotificationOfChange.aspx?k=" + encrypted + "&mode=confirm");
                }
                else if (prefix == "AAD3")
                {
                    Response.Redirect("withdraw2.aspx?k=" + encrypted + "&mode=confirm");
                }
                else if (prefix == "AAD4")
                {
                    Response.Redirect("storage.aspx?k=" + encrypted + "&mode=confirm");
                }

            }
            else if (e.CommandName == "ApproveRow")
            {
                if (prefix == "AAD2")
                {
                    Response.Redirect("NotificationOfChange.aspx?k=" + encrypted + "&mode=approve");
                }
                else if (prefix == "AAD4")
                {
                    Response.Redirect("storage.aspx?k=" + encrypted + "&mode=approve");
                }
            }
            else if (e.CommandName == "ReturnRow")
            {
                if (prefix == "AAD1")
                {
                    Response.Redirect("borrow_tool2.aspx?k=" + encrypted + "&mode=return");
                }                
            }
            //else if (e.CommandName == "ViewRow")
            //{
            //    if (prefix == "AAD1")
            //        Response.Redirect("borrow_tool2.aspx?k=" + encrypted + "&mode=view");
            //    else if (prefix == "AAD2")
            //        Response.Redirect("NotificationOfChange.aspx?k=" + encrypted + "&mode=view");
            //    else if (prefix == "AAD3")
            //        Response.Redirect("withdraw2.aspx?k=" + encrypted + "&mode=view");
            //    else if (prefix == "AAD4")
            //        Response.Redirect("storage.aspx?k=" + encrypted + "&mode=view");
            //}

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
                {
                    ShowItems_AAD1(Doc_No);
                }
                if (prefix == "AAD2")
                {
                    ShowItems_AAD2(Doc_No);
                }
                else if (prefix == "AAD3")
                {
                    ShowItems_AAD3(Doc_No);
                }
                else if (prefix == "AAD4")
                {
                    ShowItems_AAD4(Doc_No);
                }
            }

            if (e.CommandName == "ViewIssuer")
            {
                string[] data = e.CommandArgument.ToString().Split('|');

                string issue_empid = data[0];
                string doc_no = data[1];

                ShowIssuer(issue_empid , doc_no);
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Bind_RequestList();
        }
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ddlProcess.ClearSelection();
            ddlStatus.ClearSelection();
            ddlDept.ClearSelection();
            txtKeyword.Text = "";
            Bind_RequestList();
        }
        protected void btnHistClose_Click(object sender, EventArgs e)
        {
            pnlHistoryModal.Visible = false;
        }

        // --- ค้นหา Doc_No จาก FixAsset / ControlNo ใน 4 ตาราง List
        private List<string> SearchDocNoByKeyword(string keyword)
        {
            List<string> docNos = new List<string>();
            string kw = keyword.Trim().Replace("'", "''");

            string str =
                " SELECT DISTINCT Doc_No FROM ( " +
                "   SELECT Doc_No, FixAsset, ControlNo, status_id FROM Borrow_List " +
                "   UNION ALL " +
                "   SELECT Doc_No, FixAsset, ControlNo, status_id FROM NotificationofChange_List " +
                "   UNION ALL " +
                "   SELECT Doc_No, FixAsset, ControlNo, status_id FROM Withdraw_List " +
                "   UNION ALL " +
                "   SELECT Doc_no AS Doc_No, FixAsset, ControlNo, status_id FROM Storage_List " +
                " ) X " +
                " WHERE X.status_id NOT IN (1, 100, 999) " +
                "   AND ( X.FixAsset LIKE '%" + kw + "%' OR X.ControlNo LIKE '%" + kw + "%' ) " +
                "   AND X.Doc_No IS NOT NULL ";

            dc.QryDT(conn, str);

            foreach (DataRow r in dc.dt.Rows)
            {
                string d = SafeStr(r["Doc_No"]);
                if (d != "") docNos.Add(d);
            }
            return docNos;
        }


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
            // 1) Stage จาก Master_Status (query ก่อน แล้ว Copy ไว้ เพราะ dc.dt จะถูกเขียนทับ)
            dc.QryDT(conn, "SELECT status_id, status_name, detail FROM Master_Status " +
                           "WHERE sys_change = 1 ORDER BY status_id");
            DataTable dtStages = dc.dt.Copy();

            // 2) ข้อมูลเอกสาร
            string str = "SELECT * FROM View_Equipment_Request_List " +
                         "WHERE Doc_No = '" + Doc_No.Replace("'", "''") + "'";
            dc.QryDT(conn, str);

            if (dc.dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูล History ของ " + Doc_No, "History", "warn", "5000");
                return;
            }

            DataRow r = dc.dt.Rows[0];

            // ✅ ตรวจว่ามี Approver ขั้นที่ 2 หรือไม่
            dc.QryDT(conn, "SELECT dept_code_approve2 FROM NotificationofChange_Control_List " +
                           "WHERE Doc_No = '" + Doc_No.Replace("'", "''") + "'");
            bool hasApprove2 = dc.dt.Rows.Count > 0 && HasValue(dc.dt.Rows[0]["dept_code_approve2"]);

            lblHistDocNo.Text = Doc_No;

            int curStatus = Convert.ToInt32(r["status_id"]);
            bool isCancel = curStatus == 999; // ปรับตาม id ของ Cancel จริง

            var sb = new StringBuilder();
            sb.Append("<ul class='timeline'>");

            // Stage เริ่มต้น : Issue / Send (ไม่ใช่การเปลี่ยน status)
            AppendStage(sb, "Issue (Draft)", r["issue_empid"], r["issue_date"], true, "เริ่มต้นคำขอ");
            AppendStage(sb, "Send Request", r["issue_empid"], r["send_date"], true, "ส่งคำร้องขอ");

            // Stage ตาม Master_Status (ข้าม 1 = Draft และ 999 = Cancel)
            foreach (DataRow s in dtStages.Rows)
            {
                int sid = Convert.ToInt32(s["status_id"]);
                if (sid <= 1 || sid == 999) continue;
                if (sid == 31 && !hasApprove2) continue;   // ✅ ไม่มี Approver 2 → แสดงแค่ 30

                object stageDate = DBNull.Value;
                object stageEmp = DBNull.Value;

                string[] cols;
                if (StageColumns.TryGetValue(sid, out cols))
                {
                    stageDate = GetCol(r, cols[0], cols[0]);
                    stageEmp = cols[1] == null ? (object)"Auto"
                              : (HasCol(r, cols[1]) ? r[cols[1]] : DBNull.Value);
                }

                AppendStage(sb, SafeStr(s["status_name"]), stageEmp, stageDate,
                            HasValue(stageDate), SafeStr(s["detail"]));
            }

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
                sb.Append("<div class='tl-meta'>👤 " + HttpUtility.HtmlEncode(GetEmpLabel(emp)) + " &nbsp;|&nbsp; " + HttpUtility.HtmlEncode(desc) + "</div>");
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
        private Dictionary<string, string> _empLabelCache = new Dictionary<string, string>();

        // status_id → { date column, emp column } ใน View_Equipment_Request_List
        // emp column = null → แสดงเป็น "Auto"
        private static readonly Dictionary<int, string[]> StageColumns = new Dictionary<int, string[]>
        {
            { 10,  new[] { "process10_date",  "process10_empid"  } },
            { 20,  new[] { "process20_date",  "process20_empid"  } },
            { 25,  new[] { "process25_date",  "process25_empid1" } },
            { 30,  new[] { "process30_date",  "process30_empid"  } },
            { 31,  new[] { "process31_date",  "process31_empid"  } },
            { 50,  new[] { "process50_date",  "process50_empid"  } },
            { 90,  new[] { "process90_date",  "process90_empid"  } },
            { 100, new[] { "completed_date",  null               } },
        };

        private string GetEmpLabel(string empId)
        {
            empId = (empId ?? "").Trim();
            if (empId == "" || empId == "Auto") return empId;

            string label;
            if (_empLabelCache.TryGetValue(empId, out label)) return label;

            label = empId;   // ถ้าหาไม่เจอ / error → แสดงแค่ Emp ID เหมือนเดิม
            try
            {
                string url = "http://cptsvs547:1000/middleware/oracle/hrms";
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                string sql = "select GNAME_ENG, FNAME_ENG, DEPT_CODE " +
                             "from ADMIN.v_emp_data_all_cpt where UPPER(emp_no)=UPPER('" + empId.Replace("'", "''") + "')";

                byte[] data = Encoding.UTF8.GetBytes("command=" + sql);
                using (var stream = request.GetRequestStream())
                    stream.Write(data, 0, data.Length);

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(reader.ReadToEnd());

                    if (result.data != null && result.data.Count > 0)
                    {
                        var u = result.data[0];
                        string name = FormatName((u.GNAME_ENG + " " + u.FNAME_ENG).ToString().Trim());
                        string dept = SafeS(u.DEPT_CODE);
                        label = empId + " " + name + " [" + dept + "]";
                    }
                }
            }
            catch { }

            _empLabelCache[empId] = label;
            return label;
        }

        // --- Item List Modal
        protected void ShowItems_AAD1(string Doc_No)
        {
            lblItemsDocNo.Text = Doc_No;

            string str =
                " SELECT ROW_NUMBER() OVER(ORDER BY A.No) as No, A.FixAsset, A.ControlNo, A.tool_id, A.sub_id, A.system_id , B.toolName, B.tool_no " +
                " FROM Borrow_List A " +
                " left join Tool_Equipment_List_View B on A.tool_id = B.tool_id AND A.sub_id = B.sub_no AND A.system_id = B.sys_status " +
                " WHERE Doc_No = '" + Doc_No + "' " +
                " ORDER BY No";
            dc.QryDT(conn, str);   // ปรับชื่อ method query ให้ตรงกับที่ใช้จริง

            if (dc.dt.Rows.Count == 0)
            {
                rptItems.DataSource = null;
                rptItems.DataBind();
                lblNoItems.Visible = true;
            }
            else
            {
                lblNoItems.Visible = false;

                // เพิ่มคอลัมน์ PicUrl แล้วเติม web path ของรูปแต่ละแถว
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

            ScriptManager.RegisterStartupScript(
                this, GetType(), "showItemsModal",
                "$('#modal_ItemsList').modal('show');", true);
        }
        protected void ShowItems_AAD2(string Doc_No)
        {
            lblItemsDocNo.Text = Doc_No;

            string str =
                " SELECT ROW_NUMBER() OVER(ORDER BY A.No) as No, A.FixAsset, A.ControlNo, A.tool_id, A.sub_id, A.system_id , B.toolName, B.tool_no " +
                " FROM NotificationofChange_List A " +
                " left join Tool_Equipment_List_View B on A.tool_id = B.tool_id AND A.sub_id = B.sub_no AND A.system_id = B.sys_status " +
                " WHERE Doc_No = '" + Doc_No + "' " +
                " ORDER BY No";
            dc.QryDT(conn, str);   // ปรับชื่อ method query ให้ตรงกับที่ใช้จริง

            if (dc.dt.Rows.Count == 0)
            {
                rptItems.DataSource = null;
                rptItems.DataBind();
                lblNoItems.Visible = true;
            }
            else
            {
                lblNoItems.Visible = false;

                // เพิ่มคอลัมน์ PicUrl แล้วเติม web path ของรูปแต่ละแถว
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
                " WHERE Doc_No = '" + Doc_No + "' " +
                " ORDER BY No";
            dc.QryDT(conn, str);   // ปรับชื่อ method query ให้ตรงกับที่ใช้จริง

            if (dc.dt.Rows.Count == 0)
            {
                rptItems.DataSource = null;
                rptItems.DataBind();
                lblNoItems.Visible = true;
            }
            else
            {
                lblNoItems.Visible = false;

                // เพิ่มคอลัมน์ PicUrl แล้วเติม web path ของรูปแต่ละแถว
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

            ScriptManager.RegisterStartupScript(
                this, GetType(), "showItemsModal",
                "$('#modal_ItemsList').modal('show');", true);
        }
        protected void ShowItems_AAD4(string Doc_No)
        {
            lblItemsDocNo.Text = Doc_No;

            string str =
                " SELECT ROW_NUMBER() OVER(ORDER BY A.No) as No, A.FixAsset, A.ControlNo, A.tool_id, A.sub_id, A.system_id , B.toolName, B.tool_no " +
                " FROM Storage_List A " +
                " left join Tool_Equipment_List_View B on A.tool_id = B.tool_id AND A.sub_id = B.sub_no AND A.system_id = B.sys_status " +
                " WHERE Doc_No = '" + Doc_No + "' " +
                " ORDER BY No";
            dc.QryDT(conn, str);   // ปรับชื่อ method query ให้ตรงกับที่ใช้จริง

            if (dc.dt.Rows.Count == 0)
            {
                rptItems.DataSource = null;
                rptItems.DataBind();
                lblNoItems.Visible = true;
            }
            else
            {
                lblNoItems.Visible = false;

                // เพิ่มคอลัมน์ PicUrl แล้วเติม web path ของรูปแต่ละแถว
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

            ScriptManager.RegisterStartupScript(
                this, GetType(), "showItemsModal",
                "$('#modal_ItemsList').modal('show');", true);
        }

        // คืน web URL ของรูป (ว่าง = ใช้ default ผ่าน onerror)
        private string GetPicUrl(string tool_id, string tool_sub, string sys_status)
        {
            try
            {
                var fsp = new Get_Folder_Structure_Picture(tool_id, tool_sub, sys_status);
                string physicalFolder = fsp.get_folder_root();               // physical path เช็คว่ามีไฟล์
                string webPath = fsp.get_folder_root_view_toolMain();  // web path สำหรับ <img src>

                if (Directory.Exists(physicalFolder))
                {
                    string[] files = Directory.GetFiles(physicalFolder, "*.jpg", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                        return webPath;   // มีรูปจริง → คืน web path
                }
            }
            catch { }

            return "images/FilesType/jpeg.png"; // default
        }

        private string SafeStr(object o)
        {
            return (o == null || o == DBNull.Value) ? "" : o.ToString().Trim();
        }
        private string GetViewUrl(string Doc_No)
        {
            string k = HttpUtility.UrlEncode(Convert.ToBase64String(Encoding.UTF8.GetBytes(Doc_No)));
            string prefix = Doc_No.Contains("-") ? Doc_No.Split('-')[0] : "";

            switch (prefix)
            {
                case "AAD1": return "borrow_tool2.aspx?k=" + k + "&mode=view";
                case "AAD2": return "NotificationOfChange.aspx?k=" + k + "&mode=view";
                case "AAD3": return "withdraw2.aspx?k=" + k + "&mode=view";
                case "AAD4": return "storage.aspx?k=" + k + "&mode=view";
                default: return "";
            }
        }

        protected void ShowIssuer(string empId ,string Doc_No)
        {
            empId = (empId ?? "").Trim();
            lblIsId.Text = empId;

            bool ok = GetIssuerHRMS(empId , Doc_No);

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

                if(prefix == "AAD1")
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

        private string SafeS(dynamic o)
        {
            return o == null ? "-" : o.ToString().Trim();
        }

        string FormatName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return "";

            var parts = fullName.Trim().Split(' ');

            if (parts.Length >= 2)
            {
                return parts[0] + " " + parts[1].Substring(0, 1) + ".";
            }

            return fullName;
        }

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
        protected string GetStatusClass(string status)
        {
            switch (status.Trim())
            {
                case "Wait Issue":
                    return "status-draft";

                case "J3 Up Approve 1":
                    return "status-approve";

                case "J3 Up Approve 2":
                    return "status-approve";

                case "AAD Confirm":
                    return "status-confirm";

                case "During Borrow":
                    return "status-borrow";

                case "Completed":
                    return "status-completed";

                case "Cancel":
                    return "status-cancel";

                default:
                    return "";
            }
        }



    }
}