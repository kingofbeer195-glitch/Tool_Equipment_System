using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tool_Equipment_Control_System
{
    public partial class pendingStatus : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        SqlConnection conn_jig = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        datacon dc = new datacon();

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = "Equipment Status";

            if (Session["emp_id"] == null || Session["emp_id"].ToString() == "")
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                Check_Admin();

                txtDateTo.Text = DateTime.Today.ToString("dd-MMM-yyyy");
                txtDateFrom.Text = DateTime.Today.AddDays(-14).ToString("dd-MMM-yyyy");

                Bind_ddlProcess();
                Bind_ddlStatus();
                
                Requester_getdata();
                
                //Bind_RequestList();
            }
        }
        protected void Check_Admin()
        {
            if (Session["admin"] == null) return;

            string Admin = Session["admin"].ToString();

            if (Admin == "99")
            {
                //pnlMenu.Visible = true;

            }
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
            DataTable dt_Request_List = null;
            string empId = txtEmpId.Text.Trim();
            string empName = FormatName(txtEmpName.Text.Trim());

            // ✅ เช็ค Admin
            bool isAdmin = (Session["admin"] != null && Session["admin"].ToString() == "99");

            if (pnlNewEmp.Visible != true)
            {
                try
                {
                    string str = "";
                    string where = "";

                    // ── เงื่อนไข "รายการที่ตัวเองร้องขอ" (ใช้ทั้ง User และ Admin) ──
                    string ownCond;
                    if (chkbox_not.Checked)
                    {
                        // ✅ ยังไม่เสร็จ : ตัด AAD4 ออก (Admin เห็น AAD4 จากเงื่อนไขด้านล่าง)
                        ownCond = " A.issue_empid = @emp AND A.status_id NOT IN(99,100,999)" +
                                  " AND A.Doc_No NOT LIKE 'AAD4-%'";
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ddlProcess.SelectedValue))
                            where += " AND A.document_id = '" + ddlProcess.SelectedValue + "'";
                        if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                            where += " AND A.status_id = '" + ddlStatus.SelectedValue + "'";
                        if (!string.IsNullOrEmpty(txtDateFrom.Text))
                        {
                            DateTime dateFrom = Convert.ToDateTime(txtDateFrom.Text);
                            where += " and A.issue_date >= '" + dateFrom.ToString() + "'";
                        }
                        if (!string.IsNullOrEmpty(txtDateTo.Text))
                        {
                            DateTime dateTo = Convert.ToDateTime(txtDateTo.Text).AddDays(1);
                            where += " and A.issue_date <= '" + dateTo.ToString() + "'";
                        }

                        // ✅ ตัด AAD4 ออก (Admin เห็น AAD4 จากเงื่อนไขด้านล่าง)
                        where += " AND A.Doc_No NOT LIKE 'AAD4-%'";

                        ownCond = " A.issue_empid = @emp " + where;
                    }

                    if (isAdmin)
                    {
                        // ✅ Admin : รายการที่ตัวเองร้องขอ (เช่น แจ้งเปลี่ยนเครื่องมือวัด AAD2)
                        //           + AAD4 ทั้งหมดที่ยังไม่เสร็จ (ทุกคน ไม่ผูก issue_empid) แบบเดิม
                        str = "Select * From View_Equipment_Request_List A" +
                              " Where ( " + ownCond + " )" +
                              "    OR ( A.Doc_No LIKE 'AAD4-%' AND A.status_id NOT IN(99,100,999) )" +
                              " Order By Doc_No";
                    }
                    else
                    {
                        str = "Select * From View_Equipment_Request_List A" +
                              " Where " + ownCond +
                              " Order By Doc_No";
                    }

                    dc.QryDT_P(conn, str, new SqlParameter("@emp", SqlDbType.NVarChar) { Value = emp_id });
                    dt_Request_List = dc.dt;

                    dt_Request_List.Columns.Add("empName", typeof(string));
                    foreach (DataRow row in dt_Request_List.Rows)
                        if (!isAdmin || Convert.ToString(row["issue_empid"]).Trim() == emp_id) { row["empName"] = empName; }

                    lblTotal.Text = dt_Request_List.Rows.Count.ToString();
                }
                catch (Exception ex)
                {
                    alert_toast("เกิดข้อผิดพลาด : " + ex.Message, "Error", "error", "10000");
                }
            }

            gvRequestList.DataSource = dt_Request_List;
            gvRequestList.DataBind();
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
        protected void txtDateFrom_TextChanged(object sender, EventArgs e)
        {
            Bind_RequestList();
        }
        protected void txtDateTo_TextChanged(object sender, EventArgs e)
        {
            Bind_RequestList();
        }

        // ---- Check Box
        protected void chkNot_CheckedChanged(object sender, EventArgs e)
        {
            Bind_RequestList();
        }

        protected void Bind_ddlProcess()
        {
            string str = "Select * From Master_Document Where status_use = '1'";
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
            string str = "Select * From Master_Status Where status_use = '1' order by status_id";
            dc.QryDT(conn, str);

            ddlStatus.DataSource = dc.dt;
            ddlStatus.DataTextField = "status_name";
            ddlStatus.DataValueField = "status_id";
            ddlStatus.DataBind();

            // เพิ่ม -- Select --
            ddlStatus.Items.Insert(0, new ListItem("-- ทั้งหมด --", ""));
        }


        protected void gvRequestList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // ✅ ดึงค่า status จาก DataRowView
                DataRowView drv = (DataRowView)e.Row.DataItem;
                int status_id = Convert.ToInt32(drv["status_id"]);

                // ✅ FindControl ต้องใช้กับ e.Row เท่านั้น
                Button btnEdit = (Button)e.Row.FindControl("btnEdit");
                Button btnView = (Button)e.Row.FindControl("btnView");
                Label lbNextStatus = (Label)e.Row.FindControl("lb_nextStatus");

                if (status_id == 1)
                {
                    btnEdit.Visible = true;
                    btnView.Visible = false;
                }
                else
                {
                    btnEdit.Visible = false;
                    btnView.Visible = true;
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
        protected void gvRequestList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string Doc_No = e.CommandArgument.ToString();
            string encrypted = HttpUtility.UrlEncode(Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes(Doc_No)));

            // ✅ split เอาส่วนหน้า
            string prefix = Doc_No.Contains("-") ? Doc_No.Split('-')[0] : "";


            if (e.CommandName == "EditRow")
            {
                if (prefix == "AAD1")
                {
                    Response.Redirect("borrow_tool2.aspx?k=" + encrypted);
                }
                else if (prefix == "AAD2")
                {
                    Response.Redirect("NotificationOfChange.aspx?k=" + encrypted);
                }
                else if (prefix == "AAD3")
                {
                    Response.Redirect("withdraw2.aspx?k=" + encrypted);
                }
                else if (prefix == "AAD4")
                {
                    Response.Redirect("storage.aspx?k=" + encrypted);
                }
            }
            else if (e.CommandName == "ViewRow")
            {
                if (prefix == "AAD1")
                {
                    Response.Redirect("borrow_tool2.aspx?k=" + encrypted + "&mode=view");
                }
                else if (prefix == "AAD2")
                {
                    Response.Redirect("NotificationOfChange.aspx?k=" + encrypted + "&mode=view");
                }
                else if (prefix == "AAD3")
                {
                    Response.Redirect("withdraw2.aspx?k=" + encrypted + "&mode=view");
                }
                else if (prefix == "AAD4")
                {
                    Response.Redirect("storage.aspx?k=" + encrypted + "&mode=view");
                }
            }
            else if (e.CommandName == "HistoryRow")
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
            else if (e.CommandName == "ViewItems")
            {
                if (prefix == "AAD1")
                {
                    ShowItems_AAD1(Doc_No);
                }
                else if (prefix == "AAD2")
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

            // Stage 3 : Confirm (AAD) Process50 , status_id = 50
            AppendStage(sb, "AAD Confirm",
                r["process50_empid"], GetCol(r, "process50_date", "process50_date"),
                HasValue(GetCol(r, "process50_date", "process50_date")), "อนุมัติการร้องขอ");

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

            // แจ้งเปลี่ยนเครื่องมือวัด (change_type = gauge) : 28 QC Check → 31 Approve 2 (J3 Up)
            dc.QryDT(conn, "SELECT change_type FROM NotificationofChange_Control_List " +
                           "WHERE Doc_No = '" + Doc_No.Replace("'", "''") + "'");
            bool isGauge = dc.dt.Rows.Count > 0 && dc.dt.Rows[0]["change_type"] != DBNull.Value
                           && dc.dt.Rows[0]["change_type"].ToString().Trim() == "gauge";

            if (isGauge)
            {
                // ผู้อนุมัติขั้น 1 (QC) = process30 , ขั้น 2 (J3 Up) = process31
                AppendStage(sb, "QC Check",
                    HasCol(r, "process30_empid") ? r["process30_empid"] : DBNull.Value, GetCol(r, "process30_date", "process30_date"),
                    HasValue(GetCol(r, "process30_date", "process30_date")), "QC ยืนยันการเปลี่ยนเครื่องมือวัด");
                AppendStage(sb, "Approve 2 (J3 Up)",
                    HasCol(r, "process31_empid") ? r["process31_empid"] : DBNull.Value, GetCol(r, "process31_date", "process31_date"),
                    HasValue(GetCol(r, "process31_date", "process31_date")), "อนุมัติการร้องขอ");
            }
            else
            {
            // Stage 3 : J3 Up Approve (Leader Team : Requester) Process30 , status_id = 30
            AppendStage(sb, "J3 Up Approve (Requester)",
                r["process30_empid"], GetCol(r, "process30_date", "process30_date"),
                HasValue(GetCol(r, "process30_date", "process30_date")), "อนุมัติการร้องขอ");
            }

            // Stage 4 : Confirm (AAD) Process50 , status_id = 50
            AppendStage(sb, "AAD Confirm",
                r["process50_empid"], GetCol(r, "process50_date", "process50_date"),
                HasValue(GetCol(r, "process50_date", "process50_date")), "อนุมัติการร้องขอ");

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
        protected void ShowHistory_AAD3(string Doc_No , string prefix)
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

            // Stage 3 : Confirm (AAD) Process50 , status_id = 50
            AppendStage(sb, "AAD Confirm",
                r["process50_empid"], GetCol(r, "process50_date", "process50_date"),
                HasValue(GetCol(r, "process50_date", "process50_date")), "อนุมัติการร้องขอ");

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


        protected void btnHistClose_Click(object sender, EventArgs e)
        {
            pnlHistoryModal.Visible = false;
        }
        protected void btnSearch_Click(object sender, EventArgs e)
        {

        }
        protected void btnClear_Click(object sender, EventArgs e)
        {

        }


        string FormatName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return "";

            var parts = fullName.Trim().Split(' ');

            //alert_toast(fullName + " / " + parts, "test", "warn", "5000");

            if (parts.Length >= 2)
            {
                //return parts[0] + " " + parts[1].Substring(0, 1) + ".";
                string initial = string.IsNullOrWhiteSpace(parts[1])
                    ? ""
                    : parts[1].Substring(0, 1) + ".";

                return parts[0] + " " + initial;
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

                case "J3 Up Approve":
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