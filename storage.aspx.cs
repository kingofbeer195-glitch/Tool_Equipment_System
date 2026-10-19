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
using System.Windows.Interop;
//using System.Text.RegularExpressions;


namespace Tool_Equipment_Control_System
{
    public partial class Storage : System.Web.UI.Page
    {
        SqlConnection conn_user = new SqlConnection(ConfigurationManager.ConnectionStrings["user"].ConnectionString);
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        SqlConnection conn_jig = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        datacon dc = new datacon();

        // ── Manual (อ้างอิง Borrow2) ───────────────────────────
        private const string MANUAL_FOLDER =
            @"\\cptfile02\Pro\PRJ2023008\50_Flow_Manual\01_Manual\08_Tool&Equipment\04_Storage";
        private static readonly string[] MANUAL_FILES = { "Step1", "Step2", "Step3", "Step4", "SaveDraft", "SendRequest", "CancelRequest", "Confirm", "Reject" };

        protected DataTable StorageListDT
        {
            get
            {
                if (ViewState["StorageListDT"] == null)
                {
                    ViewState["StorageListDT"] = CreateEmptyChangeTable();
                }
                return (DataTable)ViewState["StorageListDT"];
            }
            set
            {
                ViewState["StorageListDT"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = "จัดเก็บอุปกรณ์";

            string Doc_NO = null;
            string mode = "edit"; // default
            ViewState["MODE"] = mode;
            hdIsAdmin.Value = (Session["admin"] != null && Session["admin"].ToString() == "99") ? "99" : "0";
            phManualUpload.Visible = (hdIsAdmin.Value == "99");

            //Temp Storage
            SetStorageAdminUI();

            // Admin : เปิด EMP ID ให้กรอกได้เสมอ (ทุก postback)
            if (Session["admin"] != null && Session["admin"].ToString() == "99")
            {
                txtEmpId.ReadOnly = false;
                txtEmpId.Style["background-color"] = "#00a7ff40";
            }

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
                    BindStorageList();
                }

                ViewState["MODE"] = mode;
                ViewState["DEPT_MASTER"] = GetDeptMaster();
                ViewState["MODEL_MASTER"] = GetModelMaster();

                if (Doc_NO == "0")
                {
                    Requester_getdata();
                    // กำหนดค่าใน TextBox "วันที่ยืม" เป็นวันที่ปัจจุบัน
                    txtIssueDate.Text = DateTime.Today.ToString("dd-MMM-yyyy");
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
                    LoadStorageList(Doc_NO);
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
                    else if (mode == "approve")
                    {
                        SetApproveMode(status_id);
                    }
                    else
                    {
                        if (CancelButton != null) CancelButton.Visible = true;
                        ShowRejectHistoryIfAny();
                    }
                }
            }
        }

        #region --- Requester Data ---
        // --- Requester_getdata() — เรียกตอน Page_Load (!IsPostBack)
        protected void Requester_getdata()
        {
            string emp_id = Session["emp_id"].ToString();
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

            //string str_dept = "SELECT * FROM View_dept_master WHERE dept_code = '" + emp_id + "'";
            //dc.QryDT(conn, str_dept);

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

            txtEmpId.Text = emp_id;
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

        private DataTable GetUserMaster()
        {
            if (ViewState["USER_MASTER"] != null) return (DataTable)ViewState["USER_MASTER"];

            string sql =
                "SELECT emp_id, fname FROM user_master.dbo.sys_user_master" +
                " WHERE status = 1 AND LEN(emp_id) >= 6" +   // ✅ emp_id >= 6 หลัก
                " ORDER BY fname";
            dc.QryDT(conn, sql);
            ViewState["USER_MASTER"] = dc.dt;
            return dc.dt;
        }

        #endregion

        #region --- Load Data : Storage Control List ---
        // เรียกข้อมูลถ้ามีข้อมูลในระบบแล้ว Storage Control List
        protected string status_show(string Doc_No)
        {
            string str_status =
                " SELECT A.status_id, B.status_name, B.Style_CSS" +
                " FROM Storage_Control_List A" +
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
            string str_emp = "";
            str_emp = "select * from Storage_Control_List where Doc_No = N'" + Doc_No + "'";

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

            ddlReason_Storage.SelectedValue = reason_id;
            txt_StorageComment.Text = comment_text;

        }
        private void LoadStorageList(string Doc_No)
        {
            DataTable dt = StorageListDT;
            string str_Changelist =
                " Select A.No , A.Doc_No , A.tool_id , A.sub_id , A.system_id , A.FixAsset , A.ControlNo , A.Use_Dept " +
                " , A.Use_Model_Id , A.New_Model ,A.HD_Model " +
                " , A.Use_Cell_Id , A.New_Cell , A.HD_Cell" +
                " , A.Use_Station_Id , A.New_Station , A.HD_Station" +
                " ,B.tool_no , B.dept_code , B.model_name , B.model_id" +
                " ,B.location1_id , B.location , B.location2_id , B.cell , B.station_id , B.station , A.status_id" +
                " , A.Reason , A.service_unit , A.status_item , A.OK_NG " +          // ✅ 4 ฟิลด์ที่ต้องดึงเพิ่ม
                " , A.Repair_Status , A.Repair_Reason , A.Repair_DueDate , A.RepairConfirm_By " +
                " from Storage_List A" +
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
                newRow["ModelName"] = r["model_name"];
                newRow["ModelId"] = r["model_id"];
                newRow["CellName"] = r["cell"];
                newRow["CellId"] = r["location2_id"];
                newRow["StationName"] = r["station"];
                newRow["StationId"] = r["station_id"];

                // ✅ Reason / Service Unit / Status / OK-NG — ดึงมาจาก Storage_List โดยตรง
                newRow["Reason"] = r["Reason"] == DBNull.Value ? "" : r["Reason"].ToString();
                newRow["Service_Unit"] = r["service_unit"] == DBNull.Value ? "" : r["service_unit"].ToString();
                newRow["Status_Item"] = r["status_item"] == DBNull.Value ? "" : r["status_item"].ToString();
                newRow["OK_NG"] = r["OK_NG"] == DBNull.Value ? "" : r["OK_NG"].ToString();

                // Repair
                newRow["Repair_Status"] = r["Repair_Status"];
                newRow["Repair_Reason"] = r["Repair_Reason"];
                newRow["Repair_DueDate"] = r["Repair_DueDate"] == DBNull.Value
                    ? "" : Convert.ToDateTime(r["Repair_DueDate"]).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                newRow["RepairConfirm_By"] = r["RepairConfirm_By"];

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

                // Compare Data
                newRow["CompareData"] = "";

                dt.Rows.Add(newRow);
            }

            StorageListDT = dt;
            BindStorageList();
        }
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
        private void SetStorageAdminUI()
        {
            // Step 4 : hide comment section
            pnlComment.Visible = false;
            
            // ปุ่ม : เหลือแต่ AAD Confirm
            if (DraftButton != null) DraftButton.Visible = true;
            if (SendButton != null) SendButton.Visible = false;
            if (CancelButton != null) CancelButton.Visible = false;
            if (RejectButton != null) RejectButton.Visible = false;
            if (ApproveButton != null) ApproveButton.Visible = false;
            if (ConfirmButton != null) ConfirmButton.Visible = true;
        }

        #endregion

        #region ===== Button Click =====
        protected void btn_pic_Click(object sender, EventArgs e)
        {
            GridViewRow row = (sender as LinkButton).NamingContainer as GridViewRow;
            HiddenField hdtool_id = (HiddenField)row.FindControl("hdtoolId");
            HiddenField hdtool_sub = FindHd(row, "hdtoolSub", "hdToolSub");
            HiddenField hd_sys_status = FindHd(row, "hdsystemId", "hdSystemId");

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
            HiddenField hdtool_sub = FindHd(row, "hdtoolSub", "hdToolSub");
            HiddenField hd_sys_status = FindHd(row, "hdsystemId", "hdSystemId");

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
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFixAsset.Text))
            {
                alert_toast("กรุณากรอกข้อมูลที่ต้องการค้นหา", "แจ้งตือน", "warn", "5000");
                return;
            }

            string dept_check = txtFixAsset.Text.Trim();

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

            // ── อุปกรณ์ที่ติดกระบวนการ AAD1–AAD4 (query เดียว) — อ้างอิง Borrow2 ──
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
        protected void btnDraft_Click(object sender, EventArgs e)
        {
            string empId = txtEmpId.Text.Trim();
            string empName = txtEmpName.Text.Trim();
            DataTable dt = StorageListDT;
            string Doc_No = litIssueNo.Text.Trim();
            string status_id = "1";  //Wait Issue

            #region --- Check Data before SAVE : Draft ---

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

            // ✅ ตรวจสอบ Table StorageListDT ต้องมีข้อมูล
            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูลในรายการ Storage List", "alert-info", "warn", "5000");
                return;
            }

            #endregion

            // -- Accept to Save Data
            saveData(status_id);
        }
        protected void btnSend_Click(object sender, EventArgs e)
        {
            string empId = txtEmpId.Text.Trim();
            string empName = txtEmpName.Text.Trim();
            DataTable dt = StorageListDT;
            string Doc_No = litIssueNo.Text.Trim();
            string status_id = "10";  // AAD Check

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

            // ✅ ตรวจสอบ Table StorageListDT ต้องมีข้อมูล
            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูลในรายการ Change List", "alert-info", "warn", "5000");
                return;
            }

            // Check Null , Not Selection Data
            Save_StorageListDT(); // Save ก่อน ป้องกัน Mode/Text หาย

            string firstRowId, firstFieldId;
            Boolean checkblank = Check_blankData(out firstRowId, out firstFieldId);
            if (checkblank)
            {
                alert_toast("กรุณาลงข้อมูลให้ครบถ้วน (ช่องสีเหลือง)", "ข้อมูลไม่ครบ", "warn", "5000");

                if (!string.IsNullOrEmpty(firstRowId))
                {
                    string fieldArg = string.IsNullOrEmpty(firstFieldId) ? "null" : "'" + firstFieldId + "'";
                    string script = "scrollAndFocusWarning('" + firstRowId + "', " + fieldArg + ");";

                    ScriptManager.RegisterStartupScript(
                        this, this.GetType(),
                        "scrollWarn_" + DateTime.UtcNow.Ticks,
                        script, true);
                }
                return;
            }

            // ตรวจสอบ NG Item
            DataTable dtSend = StorageListDT;
            bool anyNG = false, anyBlank = false;

            foreach (DataRow r in dtSend.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                string okng = GetVal(r, "OK_NG");
                if (string.IsNullOrEmpty(okng)) anyBlank = true;
                else if (okng.Equals("NG", StringComparison.OrdinalIgnoreCase)) anyNG = true;
            }
            if (anyBlank)
            {
                alert_toast("กรุณาเลือก OK/NG ให้ครบทุกรายการ", "ข้อมูลไม่ครบ", "warn", "5000");
                return;
            }

            // --- status_id : 5 -> ENG Confirm / status_id : 10 -> AAD Check
            status_id = anyNG ? "5" : "10";

            #endregion

            // ตรวจสอบว่าในข้อมูลมี รายการที่ "กำลังยืมอยู่หรือไม่"
            if (duringBorrowCheck())
            {
                BindStorageList();
            }
            else
            {
                // -- Accept to Save Data
                saveData(status_id);
            }
        }
        protected void btnApprove_Click(object sender, EventArgs e)
        {
            string Doc_No = litIssueNo.Text.Trim();
            string mode = ViewState["MODE"].ToString();
            string empId = "";
            string empName = "";
            string link_path = "EquipmentStatus2.aspx";
            string status_id = "50";  //AAD Confirm

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

            // ✅ ตรวจรายการที่ติดกระบวนการอื่นก่อน Approve
            if (duringBorrowCheck())
            {
                BindStorageList();
                return;
            }

            conn.Open();

            try
            {
                // ── Update Withdraw_Control_List → status 99 : Wait Auto Update Location ──
                #region
                string sqlHeader =
                    "UPDATE Storage_Control_List" +
                    " SET status_id = 50 , update_date = @update_date , approve_empid = @approve_empid , approve_date = @approve_date" +
                    " WHERE Doc_No = @Doc_No";

                using (SqlCommand cmd = new SqlCommand(sqlHeader, conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@approve_empid", empId);
                    cmd.Parameters.AddWithValue("@approve_date", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
                #endregion

                // ── Update Withdraw_List → status 99 ──
                #region
                string sqlList =
                    "UPDATE Storage_List" +
                    " SET status_id = 50" +
                    " WHERE Doc_No = @Doc_No";

                using (SqlCommand cmd = new SqlCommand(sqlList, conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
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
                string alert_info = "Approved";
                conn.Close(); // ✅ ปิดทุกครั้ง

                //alert_toast("Doc No : " + Doc_No, alert_info, "success", "2000");

                string jsDoc = HttpUtility.JavaScriptStringEncode(Doc_No);
                string script =
                    "if (typeof showFullOverlay === 'function') showFullOverlay('กำลังบันทึก...', 'กรุณารอสักครู่');" +
                    "if (typeof showToast === 'function') {" +
                    "  showToast('Doc No : " + jsDoc + "', { title: 'Confirm Request', type: 'success', duration: 2000 });" +
                    "}" +
                    "setTimeout(function () { window.location.href = 'ApproveStatus.aspx'; }, 2000);";

                ScriptManager.RegisterStartupScript(
                    this, this.GetType(), "redirectScript", script, true);
            }
        }
        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            string empId = txtEmpId.Text.Trim();
            string empName = txtEmpName.Text.Trim();
            DataTable dt = StorageListDT;
            string status_id = "99";  // AAD Confirm

            // ── 1) EMP ID 6 หลัก ──
            if (!System.Text.RegularExpressions.Regex.IsMatch(empId, @"^\d{6}$"))
            {
                alert_toast("Employee ID ต้องเป็นตัวเลข 6 หลัก", "ข้อมูลไม่ครบ", "warn", "5000");
                txtEmpId.Focus(); return;
            }

            // ── 2) Tel ──
            if (!ValidateTel()) return;

            // ── 3) มีรายการ ──
            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูลในรายการ Storage List", "ข้อมูลไม่ครบ", "warn", "5000");
                return;
            }

            // ── 4) เก็บค่าจาก grid ก่อนเช็ค ──
            Save_StorageListDT();

            // ── 5) เช็คช่องว่างในแต่ละแถว + ไฮไลต์ ──
            string firstRowId, firstFieldId;
            if (Check_blankData_Confirm(out firstRowId, out firstFieldId))
            {
                alert_toast("กรุณาลงข้อมูลให้ครบถ้วน (ช่องสีเหลือง)", "ข้อมูลไม่ครบ", "warn", "5000");
                if (!string.IsNullOrEmpty(firstRowId))
                {
                    string fieldArg = string.IsNullOrEmpty(firstFieldId) ? "null" : "'" + firstFieldId + "'";
                    ScriptManager.RegisterStartupScript(this, GetType(),
                        "scrollWarn_" + DateTime.UtcNow.Ticks,
                        "scrollAndFocusWarning('" + firstRowId + "', " + fieldArg + ");", true);
                }
                return;
            }

            // ── 6) รายการกำลังยืม ──
            if (duringBorrowCheck())
            {
                BindStorageList();
                return;
            }

            // ── 7) มี Doc_No → Update / ไม่มี → Insert ──
            string Doc_No = litIssueNo.Text.Trim();

            // ✅ บอก Sync/Insert ว่าอย่าเพิ่ง redirect (Confirm flow จะ redirect เอง)
            ViewState["SKIP_REDIRECT"] = "1";

            if (Doc_No != "-" && !string.IsNullOrEmpty(Doc_No))
            {
                // UPDATE header → status 99
                conn.Open();
                try
                {
                    string sqlHeader =
                        "UPDATE Storage_Control_List SET status_id = 99 , issue_date = @issue_date , " +
                        " phone = @phone , update_date = @update_date , send_date = @send_date , " +
                        " confirm_empid = @confirm_empid , confirm_date = @confirm_date " +
                        " WHERE Doc_No = @Doc_No";
                    using (SqlCommand cmd = new SqlCommand(sqlHeader, conn))
                    {
                        cmd.Parameters.AddWithValue("@issue_date", Convert.ToDateTime(txtIssueDate.Text));
                        cmd.Parameters.AddWithValue("@phone",
                            string.IsNullOrEmpty(txtTel.Text.Trim()) ? (object)DBNull.Value : txtTel.Text.Trim());
                        cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@send_date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@confirm_empid", empId);
                        cmd.Parameters.AddWithValue("@confirm_date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                { alert_toast("Error : " + ex.Message, "บันทึกไม่สำเร็จ", "error", "8000"); }
                finally { conn.Close(); }

                Sync_BorrowList(Doc_No, status_id);   // save list (status 99) — ไม่ redirect แล้ว
            }
            else
            {
                SaveData_InsertBranch(status_id);      // สร้าง Doc_No ใหม่ (status 99) — ไม่ redirect แล้ว
                Doc_No = ViewState["NEW_DOC_NO"] != null ? ViewState["NEW_DOC_NO"].ToString() : Doc_No;
            }

            // ✅ ถ้า save list ล้มเหลว จะมี flag SAVE_FAILED — หยุด ไม่ทำ location
            if (ViewState["SAVE_FAILED"] != null && ViewState["SAVE_FAILED"].ToString() == "1")
            {
                ViewState["SAVE_FAILED"] = null;
                return;
            }

            // ✅ Step 2 : status 99 → 100 + update location (tx เดียว)
            AAD_Confirm_Location(Doc_No);
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
                // ── Update Storage_Control_List → status 1 ──
                #region
                string sqlHeader =
                    "UPDATE Storage_Control_List" +
                    " SET status_id = 1 , update_date = @update_date" +
                    " WHERE Doc_No = @Doc_No";

                using (SqlCommand cmd = new SqlCommand(sqlHeader, conn))
                {
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.Parameters.AddWithValue("@update_date", rejectDate);
                    cmd.ExecuteNonQuery();
                }
                #endregion

                // ── Update Storage_List → status 1 ──
                #region
                string sqlList =
                    "UPDATE Storage_List" +
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
                    cmd.Parameters.AddWithValue("@Doc_Type", "AAD4");
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
                        "if (typeof showFullOverlay === 'function') showFullOverlay('กำลังบันทึก...', 'กรุณารอสักครู่');" +
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
                // ── Update Withdraw_Control_List → status 999 ───────────────────────
                #region
                string sqlHeader =
                    "UPDATE Storage_Control_List" +
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
                    "UPDATE Storage_List" +
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
                    cmd.Parameters.AddWithValue("@Doc_Type", "AAD4");
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
                ReopenRejectModal("cancel");
                return;
            }
            finally
            {
                conn.Close();
            }

            // ── Redirect หลัง Cancel สำเร็จ ──────────────────────────────────────
            string script =
                "if (typeof showFullOverlay === 'function') showFullOverlay('กำลังบันทึก...', 'กรุณารอสักครู่');" +
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

        protected void ddlOKNG_SelectedIndexChanged(object sender, EventArgs e)
        {
            SyncGridViewToStorageListDT();

            DropDownList ddl = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddl.NamingContainer;

            HiddenField hdToolId = row.FindControl("hdtoolId") as HiddenField;
            HiddenField hdToolSub = row.FindControl("hdtoolSub") as HiddenField;
            HiddenField hdSysId = row.FindControl("hdSystemId") as HiddenField;

            if (hdToolId != null && hdToolSub != null && hdSysId != null)
            {
                DataRow dr = FindRowByKey(hdToolId.Value + "|" + hdToolSub.Value + "|" + hdSysId.Value);
                if (dr != null)
                {
                    string okng = ddl.SelectedValue;

                    if (okng.Equals("OK", StringComparison.OrdinalIgnoreCase))
                    {
                        dr["Repair_Status"] = "-";        // OK → ค่า "-"
                        dr["RepairConfirm_By"] = "-";
                    }
                    else if (okng.Equals("NG", StringComparison.OrdinalIgnoreCase))
                    {
                        dr["Repair_Status"] = DBNull.Value;   // NG → เปิดให้เลือกใหม่
                        dr["RepairConfirm_By"] = DBNull.Value;
                    }
                    else
                    {
                        dr["Repair_Status"] = DBNull.Value;
                        dr["RepairConfirm_By"] = DBNull.Value;
                    }

                    if (GetVal(dr, "CompareData") != "Insert") dr["CompareData"] = "Update";
                }
            }

            StorageListDT = StorageListDT;
            BindStorageList_Display();
        }

        protected void ddlReasonRow_SelectedIndexChanged(object sender, EventArgs e)
        {
            //// persist current grid state, then rebind so Service enable/disable refreshes
            //SyncGridViewToStorageListDT();
            //BindStorageList_Display();


        }

        #endregion

        #region ===== Gridview =====

        // --- Gridview : gvSearchResult
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
        protected void gvSearchResult_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "AddItem") return;

            int rowIndex = Convert.ToInt32(e.CommandArgument);
            GridViewRow row = gvSearchResult.Rows[rowIndex];

            HiddenField hdtoolId = (HiddenField)row.FindControl("hdtoolId");
            HiddenField hdtoolSub = FindHd(row, "hdtoolSub", "hdToolSub");
            HiddenField hdsystemId = FindHd(row, "hdsystemId", "hdSystemId");

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

            // ── ตรวจซ้ำ (ข้าม row ที่ถูก mark Delete) ──
            foreach (DataRow br in StorageListDT.Rows)
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
            SyncGridViewToStorageListDT();

            // ── เพิ่ม row ใหม่ ──
            DataTable dt = StorageListDT;

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
            newRow["CellName"] = tool.Rows[0]["cell"];
            newRow["CellId"] = tool.Rows[0]["location2_id"];
            newRow["StationName"] = tool.Rows[0]["station"];
            newRow["StationId"] = tool.Rows[0]["station_id"];

            // ✅ Auto ค่าตอน Add
            newRow["Use_DeptCode"] = "5505";                      // Dept = AAD
            newRow["Use_ModelId"] = tool.Rows[0]["model_id"];    // current Model
            newRow["HD_Model"] = "ddl";

            newRow["CompareData"] = "Insert"; // ✅ mark Insert ทันที

            dt.Rows.Add(newRow);
            StorageListDT = dt;
            BindStorageList_Display(); // ✅ แสดงเฉพาะ row ที่ไม่ใช่ Delete

            // Clear Table Search
            ViewState["SearchResult"] = null;
            gvSearchResult.DataSource = null;
            gvSearchResult.DataBind();
        }

        // Gridview : gvStorageList
        protected void gvStorageList_RowDataBound(object sender, GridViewRowEventArgs e)
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

                // ✅ ติดกระบวนการอื่น (จาก duringBorrowCheck) → ไฮไลต์ทุกโหมด
                string procLabel = ColVal(cRow, "ProcLabel");
                if (procLabel != "")
                {
                    foreach (TableCell cell in e.Row.Cells)
                        cell.Style["background-color"] = "#fff3cd";
                    e.Row.Style["border-left"] = "8px solid #dc3545";
                    e.Row.ToolTip = "อยู่ระหว่างดำเนินการ : " + procLabel;
                }

                // highlight row ที่กำลังยืม (เดิม)
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

                #region --- Per-row: Reason / Service / Status / OK-NG ---

                DropDownList ddlReason = e.Row.FindControl("ddlReason") as DropDownList;
                TextBox txtService = e.Row.FindControl("txt_service") as TextBox;
                DropDownList ddlStatus = e.Row.FindControl("ddlStatus") as DropDownList;
                DropDownList ddlOKNG = e.Row.FindControl("ddlOKNG") as DropDownList;

                // Reason dropdown (bind + restore)
                if (ddlReason != null && ddlReason.Items.Count == 0)
                {
                    ddlReason.DataSource = GetReasonMaster();
                    ddlReason.DataTextField = "reason_Name";
                    ddlReason.DataValueField = "reason_ID";
                    ddlReason.DataBind();
                    ddlReason.Items.Insert(0, new ListItem("- Select -", ""));
                }
                if (ddlReason != null)
                {
                    string v = ColVal(cRow, "Reason");
                    if (!string.IsNullOrEmpty(v) && ddlReason.Items.FindByValue(v) != null)
                        ddlReason.SelectedValue = v;

                    //// enable Service only when reason text = EOL
                    //string reasonText = ddlReason.SelectedItem != null ? ddlReason.SelectedItem.Text.Trim() : "";
                    //alert_toast(reasonText, "Reason", "info", "5000");

                    //bool isEOL = reasonText.Equals("EOL", StringComparison.OrdinalIgnoreCase);
                    //if (txtService != null) txtService.Enabled = isEOL;

                    //// trigger on change so Service toggles without full logic in JS
                    ////ddlReason.AutoPostBack = true;
                    ////ddlReason.SelectedIndexChanged += ddlReasonRow_SelectedIndexChanged;
                }

                if (txtService != null) txtService.Text = ColVal(cRow, "Service_Unit");

                if (ddlStatus != null)
                {
                    string v = ColVal(cRow, "Status_Item");
                    if (!string.IsNullOrEmpty(v) && ddlStatus.Items.FindByValue(v) != null)
                        ddlStatus.SelectedValue = v;
                }

                if (ddlOKNG != null)
                {
                    string v = ColVal(cRow, "OK_NG");
                    if (!string.IsNullOrEmpty(v) && ddlOKNG.Items.FindByValue(v) != null)
                        ddlOKNG.SelectedValue = v;
                }

                #endregion

                #region --- Repair Status / Confirm By (dropdown, OK-NG driven) ---
                string okng = ColVal(cRow, "OK_NG");
                bool isNG = okng.Equals("NG", StringComparison.OrdinalIgnoreCase);

                DropDownList ddlRepairStatus = e.Row.FindControl("ddlRepairStatus") as DropDownList;
                DropDownList ddlConfirmBy = e.Row.FindControl("ddlConfirmBy") as DropDownList;

                // Repair Status
                if (ddlRepairStatus != null)
                {
                    if (isNG)
                    {
                        string v = ColVal(cRow, "Repair_Status");
                        ddlRepairStatus.SelectedValue =
                            (!string.IsNullOrEmpty(v) && v != "-" && ddlRepairStatus.Items.FindByValue(v) != null) ? v : "";
                    }
                    else
                    {
                        ddlRepairStatus.ClearSelection();   // OK / ยังไม่เลือก → ปิด + ว่าง
                    }
                    ddlRepairStatus.Enabled = isNG;         // default false (ยังไม่เลือก = ปิด)
                }

                // Confirm By
                if (ddlConfirmBy != null)
                {
                    if (ddlConfirmBy.Items.Count == 0)
                    {
                        ddlConfirmBy.DataSource = GetUserMaster();
                        ddlConfirmBy.DataTextField = "fname";
                        ddlConfirmBy.DataValueField = "emp_id";
                        ddlConfirmBy.DataBind();
                        ddlConfirmBy.Items.Insert(0, new ListItem("- Select -", ""));
                    }
                    if (isNG)
                    {
                        string cv = ColVal(cRow, "RepairConfirm_By");
                        ddlConfirmBy.SelectedValue =
                            (!string.IsNullOrEmpty(cv) && cv != "-" && ddlConfirmBy.Items.FindByValue(cv) != null) ? cv : "";
                    }
                    else
                    {
                        ddlConfirmBy.ClearSelection();
                    }
                    ddlConfirmBy.Enabled = isNG;
                }

                // ✅ NG → ช่องที่ยังว่างเป็นสีเหลือง (แบบเตือนข้อมูลไม่ครบ) ให้รู้ว่าต้องกรอก
                if (mode == "edit")
                {
                    SetWarning_DDL(ddlRepairStatus, isNG && ddlRepairStatus != null && ddlRepairStatus.SelectedValue == "");
                    SetWarning_DDL(ddlConfirmBy, isNG && ddlConfirmBy != null && ddlConfirmBy.SelectedValue == "");
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

                Label lblDeptNew = e.Row.FindControl("lblDeptNew") as Label;
                if (lblDeptNew != null) lblDeptNew.Text = "5505";
            }
        }
        /// <summary>
        /// Key ของแถวที่กด Remove (อ้างอิง Borrow2)
        ///   1) gvStorageList.DataKeys (DataKeyNames="ToolId,ToolSub,SystemId")
        ///   2) สำรอง : HiddenField ในคอลัมน์ Pic (hdtoolId / hdtoolSub / hdSystemId)
        /// </summary>
        private bool GetRemoveRowKeys(int rowIndex, out string toolId, out string toolSub, out string systemId)
        {
            toolId = toolSub = systemId = "";

            if (gvStorageList.DataKeys != null && rowIndex < gvStorageList.DataKeys.Count && gvStorageList.DataKeys[rowIndex] != null)
            {
                var v = gvStorageList.DataKeys[rowIndex].Values;
                toolId = Convert.ToString(v["ToolId"]).Trim();
                toolSub = Convert.ToString(v["ToolSub"]).Trim();
                systemId = Convert.ToString(v["SystemId"]).Trim();
                if (toolId != "") return true;
            }

            GridViewRow gvRow = gvStorageList.Rows[rowIndex];
            HiddenField hdToolId = FindHd(gvRow, "hdtoolId");
            HiddenField hdToolSub = FindHd(gvRow, "hdtoolSub", "hdToolSub");
            HiddenField hdSystemId = FindHd(gvRow, "hdSystemId", "hdsystemId");
            if (hdToolId == null || hdToolSub == null || hdSystemId == null) return false;

            toolId = hdToolId.Value.Trim();
            toolSub = hdToolSub.Value.Trim();
            systemId = hdSystemId.Value.Trim();
            return toolId != "";
        }
        protected void gvStorageList_RowCommand(object sender, GridViewCommandEventArgs e)
        {

            if (e.CommandName != "RemoveItem") return;

            int rowIndex = Convert.ToInt32(e.CommandArgument);

            if (rowIndex < 0 || rowIndex >= gvStorageList.Rows.Count) return;

            // ✅ Key ของแถว : DataKeys ก่อน (ไม่หายตอน postback) → สำรอง HiddenField
            //    (เดิม FindControl("hdsystemId") ไม่ตรงกับ ID จริง "hdSystemId" → ขึ้น "ไม่พบข้อมูล Hidden Field")
            string toolId, toolSub, systemId;
            if (!GetRemoveRowKeys(rowIndex, out toolId, out toolSub, out systemId))
            {
                alert_toast("ไม่พบ Key ของรายการที่ต้องการลบ กรุณารีเฟรชหน้า", "Remove Error", "error", "5000");
                return;
            }

            // ── STEP 1 : Sync GridView → StorageListDT (ไม่ Compare) ──
            SyncGridViewToStorageListDT();

            DataTable dt = StorageListDT;

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
            StorageListDT = dt;
            BindStorageList_Display();
        }

        #endregion

        #region ===== Table Data =====

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
            dt.Columns.Add("ModelName", typeof(string));
            dt.Columns.Add("ModelId", typeof(string));
            dt.Columns.Add("CellName", typeof(string));
            dt.Columns.Add("CellId", typeof(string));
            dt.Columns.Add("StationName", typeof(string));
            dt.Columns.Add("StationId", typeof(string));

            //Status
            dt.Columns.Add("Reason", typeof(string));
            dt.Columns.Add("Service_Unit", typeof(string));
            dt.Columns.Add("Status_Item", typeof(string));
            dt.Columns.Add("OK_NG", typeof(string));

            //Repair Confirm
            dt.Columns.Add("Repair_Status", typeof(string));
            dt.Columns.Add("Repair_Reason", typeof(string));
            dt.Columns.Add("Repair_DueDate", typeof(string));
            dt.Columns.Add("RepairConfirm_By", typeof(string));

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

        // --- Sync GridView → StorageListDT เท่านั้น (ไม่ Compare)
        private void SyncGridViewToStorageListDT()
        {
            DataTable ChangeDT = StorageListDT;

            // map GridView row (ไม่รวม Delete) → BorrowListDT index จริง
            var displayToReal = new System.Collections.Generic.Dictionary<int, int>();
            int displayIdx = 0;

            for (int i = 0; i < ChangeDT.Rows.Count; i++)
            {
                if (GetVal(ChangeDT.Rows[i], "CompareData") == "Delete") continue;
                displayToReal[displayIdx++] = i;
            }

            foreach (GridViewRow gvRow in gvStorageList.Rows)
            {
                if (gvRow.RowType != DataControlRowType.DataRow) continue;

                int gvIdx = gvRow.DataItemIndex;
                if (!displayToReal.ContainsKey(gvIdx)) continue;

                int realIdx = displayToReal[gvIdx];
                DataRow dr = ChangeDT.Rows[realIdx];

                // Dept
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

                #region --- Cell / Station --
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

                #endregion

                #region ---// Reason / Service / Status / OK-NG / Repair / ConfirmBy ---

                DropDownList ddlReason = gvRow.FindControl("ddlReason") as DropDownList;
                TextBox txtService = gvRow.FindControl("txt_service") as TextBox;
                DropDownList ddlStatus = gvRow.FindControl("ddlStatus") as DropDownList;
                DropDownList ddlOKNG = gvRow.FindControl("ddlOKNG") as DropDownList;
                DropDownList ddlRepairStatus = gvRow.FindControl("ddlRepairStatus") as DropDownList;
                DropDownList ddlConfirmBy = gvRow.FindControl("ddlConfirmBy") as DropDownList;
                DropDownList ddlOKNGrow = gvRow.FindControl("ddlOKNG") as DropDownList;

                if (ddlReason != null)
                    dr["Reason"] = string.IsNullOrEmpty(ddlReason.SelectedValue)
                                   ? (object)DBNull.Value : ddlReason.SelectedValue;

                // ✅ Service Unit — เก็บเสมอ (ไม่ผูก EOL)
                if (txtService != null)
                    dr["Service_Unit"] = string.IsNullOrEmpty(txtService.Text)
                         ? (object)DBNull.Value : txtService.Text.Trim().ToUpper();

                if (ddlStatus != null)
                    dr["Status_Item"] = (ddlStatus.SelectedValue == "0" || string.IsNullOrEmpty(ddlStatus.SelectedValue))
                                        ? (object)DBNull.Value : ddlStatus.SelectedValue;

                if (ddlOKNG != null)
                    dr["OK_NG"] = (ddlOKNG.SelectedValue == "0" || string.IsNullOrEmpty(ddlOKNG.SelectedValue))
                                  ? (object)DBNull.Value : ddlOKNG.SelectedValue;

                // ✅ Repair Status / Confirm By — จาก dropdown ใหม่

                bool rowIsNG = ddlOKNGrow != null &&
                ddlOKNGrow.SelectedValue.Equals("NG", StringComparison.OrdinalIgnoreCase);

                if (rowIsNG)
                {
                    // NG → เก็บค่าที่เลือกจาก dropdown
                    if (ddlRepairStatus != null)
                        dr["Repair_Status"] = string.IsNullOrEmpty(ddlRepairStatus.SelectedValue)
                                              ? (object)DBNull.Value : ddlRepairStatus.SelectedValue;
                    if (ddlConfirmBy != null)
                        dr["RepairConfirm_By"] = string.IsNullOrEmpty(ddlConfirmBy.SelectedValue)
                                                 ? (object)DBNull.Value : ddlConfirmBy.SelectedValue;
                }
                // OK / ยังไม่เลือก → ไม่ sync ทับ, คงค่า "-" หรือ null ที่ ddlOKNG_SelectedIndexChanged set ไว้

                #endregion

                //DropDownList ddlRepairStatus = gvRow.FindControl("ddlRepairStatus") as DropDownList;
                //DropDownList ddlConfirmBy = gvRow.FindControl("ddlConfirmBy") as DropDownList;

                //if (ddlRepairStatus != null)
                //    dr["Repair_Status"] = string.IsNullOrEmpty(ddlRepairStatus.SelectedValue)
                //                          ? (object)DBNull.Value : ddlRepairStatus.SelectedValue;
                //if (ddlConfirmBy != null)
                //    dr["RepairConfirm_By"] = string.IsNullOrEmpty(ddlConfirmBy.SelectedValue)
                //                             ? (object)DBNull.Value : ddlConfirmBy.SelectedValue;

            }

            StorageListDT = ChangeDT;
        }

        // --- Save Data on List   
        private void Save_StorageListDT()
        {
            // ══════════════════════════════════════════════════════════════════════
            // STEP 1 : Sync GridView → StorageListDT
            // ══════════════════════════════════════════════════════════════════════
            SyncGridViewToStorageListDT();

            DataTable ChangeDT = StorageListDT;

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

                StorageListDT = ChangeDT;
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
            "       Use_Station_Id, New_Station, HD_Station," +
            "       Reason, service_unit, status_item, OK_NG," +           // ← เพิ่ม
            "       Repair_Status, RepairConfirm_By" +                     // ← เพิ่ม
            " FROM  Storage_List" +
            " WHERE Doc_No = N'" + docNo + "'";

            dc.QryDT(conn, strDB);
            DataTable dtDB = dc.dt.Copy();

            // ══════════════════════════════════════════════════════════════════════
            // STEP 4 : เก็บ Delete keys จาก StorageListDT ก่อน loop
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
            // STEP 5 : Compare dtDB ↔ StorageListDT
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
                    // ── อยู่ใน DB แต่ไม่มีใน StorageListDT → Delete ────────────────
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
                        GetVal(matchRow, "Use_DeptCode") != GetVal(dbRow, "Use_Dept") ||
                        GetVal(matchRow, "Use_ModelId") != GetVal(dbRow, "Use_Model_Id") ||
                        GetVal(matchRow, "Add_Model") != GetVal(dbRow, "New_Model") ||
                        GetVal(matchRow, "HD_Model") != GetVal(dbRow, "HD_Model") ||
                        GetVal(matchRow, "Use_CellId") != GetVal(dbRow, "Use_Cell_Id") ||
                        GetVal(matchRow, "Add_Cell") != GetVal(dbRow, "New_Cell") ||
                        GetVal(matchRow, "HD_Cell") != GetVal(dbRow, "HD_Cell") ||
                        GetVal(matchRow, "Use_StationId") != GetVal(dbRow, "Use_Station_Id") ||
                        GetVal(matchRow, "Add_Station") != GetVal(dbRow, "New_Station") ||
                        GetVal(matchRow, "HD_Station") != GetVal(dbRow, "HD_Station") ||
                        // ✅ เพิ่ม 6 ฟิลด์
                        GetVal(matchRow, "Reason") != GetVal(dbRow, "Reason") ||
                        GetVal(matchRow, "Service_Unit") != GetVal(dbRow, "service_unit") ||
                        GetVal(matchRow, "Status_Item") != GetVal(dbRow, "status_item") ||
                        GetVal(matchRow, "OK_NG") != GetVal(dbRow, "OK_NG") ||
                        GetVal(matchRow, "Repair_Status") != GetVal(dbRow, "Repair_Status") ||
                        GetVal(matchRow, "RepairConfirm_By") != GetVal(dbRow, "RepairConfirm_By");

                            matchRow["CompareData"] = changed ? "Update" : "Same";
                }
            }

            // ── มีใน StorageListDT แต่ไม่มีใน DB → Insert ────────────────────────
            for (int i = 0; i < ChangeDT.Rows.Count; i++)
            {
                if (matchedBorrowIdx.Contains(i)) continue;

                string cmp = GetVal(ChangeDT.Rows[i], "CompareData");

                // ✅ ไม่ overwrite Delete row
                if (cmp != "Delete" && cmp != "Same" && cmp != "Update")
                    ChangeDT.Rows[i]["CompareData"] = "Insert";
            }

            StorageListDT = ChangeDT;

        }

        #endregion

        #region ===== Data Update =====

        protected void saveData(string status_id)
        {
            DataTable dt = StorageListDT;
            string Doc_No = litIssueNo.Text.Trim();

            int reason_id = 0;
            string comment_text = txt_StorageComment.Text.Trim();
            string phone = txtTel.Text.Trim();

            DateTime issue_date = Convert.ToDateTime(txtIssueDate.Text);

            if (ddlReason_Storage.SelectedIndex > 0)
                int.TryParse(ddlReason_Storage.SelectedValue, out reason_id);

            // ════════════════════════════════════════════════════════════════
            // UPDATE : มี Doc_No อยู่แล้ว
            // ════════════════════════════════════════════════════════════════
            if (Doc_No != "-" && !string.IsNullOrEmpty(Doc_No))
            {
                Save_StorageListDT(); // Update Data on Table : BorrowListDT

                // -- ตรวจสอบรายการ "กำลังยืม"
                if (duringBorrowCheck())
                {
                    BindStorageList();
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
                            "UPDATE Storage_Control_List SET status_id = @status_id , issue_date = @issue_date , " +
                            " phone = @phone , " +
                            " reason_id = @reason_id , reason_remark = @reason_remark , update_date = @update_date " +
                            " WHERE Doc_No = @Doc_No";

                        using (SqlCommand cmd = new SqlCommand(sqlHeader, conn))
                        {
                            cmd.Parameters.AddWithValue("@status_id", status_id);
                            cmd.Parameters.AddWithValue("@issue_date", issue_date);
                            cmd.Parameters.AddWithValue("@phone",string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                            cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                            cmd.Parameters.AddWithValue("@reason_id", reason_id);
                            cmd.Parameters.AddWithValue("@reason_remark", comment_text);
                            cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // -- Click Send button
                    else if (status_id == "10" || status_id == "5")
                    {
                        string sqlHeader =
                            "UPDATE Storage_Control_List SET status_id = @status_id , issue_date = @issue_date , " +
                            " phone = @phone , " +
                            " reason_id = @reason_id , reason_remark = @reason_remark , update_date = @update_date , send_date = @send_date " +
                            " WHERE Doc_No = @Doc_No";

                        using (SqlCommand cmd = new SqlCommand(sqlHeader, conn))
                        {
                            cmd.Parameters.AddWithValue("@status_id", status_id);
                            cmd.Parameters.AddWithValue("@issue_date", issue_date);
                            cmd.Parameters.AddWithValue("@phone",string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
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
            Save_StorageListDT();

            if (duringBorrowCheck())
            {
                BindStorageList();
                return;
            }

            DataTable dt = StorageListDT;
            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("ไม่พบข้อมูลในรายการ Storage List", "ไม่พบข้อมูล", "warn", "5000");
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
                            Doc_No = "AAD4-" + issueYear_2 + "-" + issueNo.ToString("00000");

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

            if (!success)
            {
                ViewState["SAVE_FAILED"] = "1";
                return;
            }

            // ✅ เก็บ Doc_No ใหม่ให้ btnConfirm ใช้ต่อ
            ViewState["NEW_DOC_NO"] = Doc_No;

            // ✅ Confirm flow : ข้าม redirect
            if (ViewState["SKIP_REDIRECT"] != null && ViewState["SKIP_REDIRECT"].ToString() == "1")
                return;

            // ── สำเร็จ → alert + redirect ──
            ShowSaveSuccessAndRedirect(Doc_No, status_id);
        }

        // --- INSERT header ภายใน transaction ปล่อย SqlException หลุดออกไปให้ลูป retry จับ(ไม่ try/catch ที่นี่)
        private void InsertControlHeader_Tx(SqlConnection c, SqlTransaction tx, int issueNo, string Doc_No, string status_id)
        {
            string issue_empid = txtEmpId.Text;
            string dept_code = txtDeptCode.Text;
            string phone = txtTel.Text.Trim();
            int reason_id = 0;
            string comment_text = txt_StorageComment.Text.Trim();

            if (ddlReason_Storage.SelectedIndex > 0)
                int.TryParse(ddlReason_Storage.SelectedValue, out reason_id);

            string sql;
            if (status_id == "1")
            {
                sql = @"INSERT INTO Storage_Control_List
                ( issue_no, issue_date, Doc_No, status_id, issue_empid,
                  dept_code,phone, reason_id, reason_remark, update_date )
                VALUES
                ( @issue_no, @issue_date, @Doc_No, @status_id, @issue_empid,
                  @dept_code, @phone, @reason_id, @reason_remark, @update_date )";
            }
            else if (status_id == "5" || status_id == "10" || status_id == "50" || status_id == "99")
            {
                sql = @"INSERT INTO Storage_Control_List
                ( issue_no, issue_date, Doc_No, status_id, issue_empid,
                  dept_code,phone, reason_id, reason_remark, update_date, send_date )
                VALUES
                ( @issue_no, @issue_date, @Doc_No, @status_id, @issue_empid,
                  @dept_code,@phone, @reason_id, @reason_remark, @update_date, @send_date )";
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

                if (status_id == "5" || status_id == "10" || status_id == "50" || status_id == "99")
                    cmd.Parameters.AddWithValue("@send_date", DateTime.Now);

                cmd.ExecuteNonQuery();   // ← Doc_No ซ้ำ throw 2627 ที่นี่
            }
        }

        // --- INSERT รายการ Borrow_List ทั้งหมด ภายใน transaction ปล่อย exception หลุดให้ caller rollback(กัน half-document)
        private void InsertChangeRows_Tx(SqlConnection c, SqlTransaction tx, DataTable dt, string Doc_No, string status_id)
        {
            string sql = @"
                INSERT INTO Storage_List
                ( Doc_No , No , FixAsset , ControlNo ,
                  tool_id , sub_id , system_id ,
                  Use_Dept ,
                  Use_Model_Id , New_Model , HD_Model ,
                  Use_Cell_Id , New_Cell , HD_Cell ,
                  Use_Station_Id , New_Station , HD_Station ,
                  Reason , service_unit , status_item , OK_NG ,
                  status_id , Repair_Status , Repair_Reason , Repair_DueDate , RepairConfirm_By )
                VALUES
                ( @Doc_No , @No , @FixAsset , @ControlNo ,
                  @tool_id , @sub_id , @system_id ,
                  @Use_Dept ,
                  @Use_Model_Id , @New_Model , @HD_Model ,
                  @Use_Cell_Id , @New_Cell , @HD_Cell ,
                  @Use_Station_Id , @New_Station , @HD_Station ,
                  @Reason , @service_unit , @status_item , @OK_NG ,
                  @status_id , @Repair_Status , @Repair_Reason , @Repair_DueDate , @RepairConfirm_By )";

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
                    cmd.Parameters.AddWithValue("@Reason", ToDbVal(row, "Reason"));
                    cmd.Parameters.AddWithValue("@service_unit", ToDbVal(row, "Service_Unit"));
                    cmd.Parameters.AddWithValue("@status_item", ToDbVal(row, "Status_Item"));
                    cmd.Parameters.AddWithValue("@OK_NG", ToDbVal(row, "OK_NG"));
                    cmd.Parameters.AddWithValue("@status_id", status_id);
                    cmd.Parameters.AddWithValue("@Repair_Status", ToDbVal(row, "Repair_Status"));
                    cmd.Parameters.AddWithValue("@Repair_Reason", ToDbVal(row, "Repair_Reason"));
                    cmd.Parameters.AddWithValue("@Repair_DueDate", ToDbVal(row , "Repair_DueDate"));
                    cmd.Parameters.AddWithValue("@RepairConfirm_By", ToDbVal(row, "RepairConfirm_By"));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // --- ตรวจสอบ Withdraw Table (Database) กับ BorrowList Table -> และ Update ลง Database
        private void Sync_BorrowList(string Doc_No, string status_id)
        {
            DataTable dt = StorageListDT;

            // --- ตรวจสอบการลงข้อมูล
            if (dt == null || dt.Rows.Count == 0)
            {
                alert_toast("ไม่มีข้อมูลใน StorageListDT", "Sync Error", "error", "5000");
                return;
            }

            // ── Renumber RowNo เฉพาะ row ที่ไม่ใช่ Delete ────────────────────────
            int no = 1;
            foreach (DataRow r in dt.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                r["RowNo"] = no++;
            }

            StorageListDT = dt;

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
                                    INSERT INTO Storage_List
                                    ( Doc_No, No, FixAsset, ControlNo,
                                      tool_id, sub_id, system_id, Use_Dept,
                                      Use_Model_Id, New_Model, HD_Model,
                                      Use_Cell_Id, New_Cell, HD_Cell,
                                      Use_Station_Id, New_Station, HD_Station,
                                      Reason, service_unit, status_item, OK_NG,
                                      Repair_Status, Repair_Reason, Repair_DueDate, RepairConfirm_By,
                                      status_id , update_date )
                                    VALUES
                                    ( @Doc_No, @No, @FixAsset, @ControlNo,
                                      @tool_id, @sub_id, @system_id, @Use_Dept,
                                      @Use_Model_Id, @New_Model, @HD_Model,
                                      @Use_Cell_Id, @New_Cell, @HD_Cell,
                                      @Use_Station_Id, @New_Station, @HD_Station,
                                      @Reason, @service_unit, @status_item, @OK_NG,
                                      @Repair_Status, @Repair_Reason, @Repair_DueDate, @RepairConfirm_By,
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
                                    cmd.Parameters.AddWithValue("@Reason", ToDbVal(row, "Reason"));
                                    cmd.Parameters.AddWithValue("@service_unit", ToDbVal(row, "Service_Unit"));
                                    cmd.Parameters.AddWithValue("@status_item", ToDbVal(row, "Status_Item"));
                                    cmd.Parameters.AddWithValue("@OK_NG", ToDbVal(row, "OK_NG"));
                                    cmd.Parameters.AddWithValue("@Repair_Status", ToDbVal(row, "Repair_Status"));
                                    cmd.Parameters.AddWithValue("@Repair_Reason", ToDbVal(row, "Repair_Reason"));
                                    cmd.Parameters.AddWithValue("@Repair_DueDate", ToDbVal(row, "Repair_DueDate"));
                                    cmd.Parameters.AddWithValue("@RepairConfirm_By", ToDbVal(row, "RepairConfirm_By"));
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
                                    UPDATE Storage_List SET
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
                                        Reason          = @Reason,
                                        service_unit    = @service_unit,
                                        status_item     = @status_item,
                                        OK_NG           = @OK_NG,
                                        Repair_Status   = @Repair_Status,
                                        Repair_Reason   = @Repair_Reason,
                                        Repair_DueDate  = @Repair_DueDate,
                                        RepairConfirm_By= @RepairConfirm_By,
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
                                    cmd.Parameters.AddWithValue("@Reason", ToDbVal(row, "Reason"));
                                    cmd.Parameters.AddWithValue("@service_unit", ToDbVal(row, "Service_Unit"));
                                    cmd.Parameters.AddWithValue("@status_item", ToDbVal(row, "Status_Item"));
                                    cmd.Parameters.AddWithValue("@OK_NG", ToDbVal(row, "OK_NG"));
                                    cmd.Parameters.AddWithValue("@Repair_Status", ToDbVal(row, "Repair_Status"));
                                    cmd.Parameters.AddWithValue("@Repair_Reason", ToDbVal(row, "Repair_Reason"));
                                    cmd.Parameters.AddWithValue("@Repair_DueDate", ToDbVal(row, "Repair_DueDate"));
                                    cmd.Parameters.AddWithValue("@RepairConfirm_By", ToDbVal(row, "RepairConfirm_By"));
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
                                        DELETE FROM Storage_List
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

                // หลัง foreach loop, ก่อน catch — บังคับ status ทั้งเอกสารให้ตรง
                string sqlStatusAll = "UPDATE Storage_List SET status_id = @status_id WHERE Doc_No = @Doc_No";
                using (SqlCommand cmd = new SqlCommand(sqlStatusAll, conn))
                {
                    cmd.Parameters.AddWithValue("@status_id", status_id);
                    cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                    cmd.ExecuteNonQuery();
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
                    // ✅ Confirm flow : ข้าม redirect ให้ btnConfirm ทำ location ต่อ
                    if (ViewState["SKIP_REDIRECT"] != null && ViewState["SKIP_REDIRECT"].ToString() == "1")
                    {
                        // ไม่ redirect — return เฉยๆ
                    }
                    else
                    {
                        string alert_info = "";
                        string link_path = "EquipmentStatus2.aspx";
                        switch (status_id)
                        {
                            case "1": alert_info = "Save Draft"; break;
                            case "99": alert_info = "Confirm Complete"; link_path = "ApproveStatus.aspx"; break;
                            case "999": alert_info = "Cancel Request"; break;
                        }

                        alert_toast("Doc No : " + Doc_No, alert_info, "info", "2000");

                        string script =
                            "if (typeof showFullOverlay === 'function') showFullOverlay('กำลังบันทึก...', 'กรุณารอสักครู่');" +
                            "setTimeout(function() {" +
                            "  window.location.href = '" + link_path + "';" +
                            "}, 2000);";

                        ScriptManager.RegisterStartupScript(
                            this, this.GetType(), "redirectScript", script, true);
                    }
                }
                else
                {
                    // ✅ มี error → บอก caller ว่าล้มเหลว
                    ViewState["SAVE_FAILED"] = "1";
                }
            }
        }

        #endregion

        #region ===== Bind Table =====

        // --- Data Storage List
        private void BindStorageList()
        {
            BindStorageList_Display();
        }

        // ✅ Bind GridView เฉพาะ row ที่ user เห็น (ไม่รวม Delete)
        private void BindStorageList_Display()
        {
            DataTable dt = StorageListDT;
            DataTable dtDisplay = dt.Clone(); // โครงสร้างเหมือนกัน ข้อมูลว่าง

            int rowNo = 1;
            foreach (DataRow r in dt.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;

                DataRow newR = dtDisplay.NewRow();
                newR.ItemArray = (object[])r.ItemArray.Clone();
                newR["RowNo"] = rowNo++;
                dtDisplay.Rows.Add(newR);
            }

            gvStorageList.DataSource = dtDisplay;
            gvStorageList.DataBind();
        }

        #endregion

        #region --- Update Location
        // ══════════════════════════════════════════════════════════════════════
        // AAD Confirm : status 99 → 100 + Update Location (Cell / Station)
        // ทั้งหมดใน transaction เดียว (cross-DB, server เดียวกัน)
        // ══════════════════════════════════════════════════════════════════════
        private void AAD_Confirm_Location(string Doc_No)
        {
            if (string.IsNullOrEmpty(Doc_No) || Doc_No == "-")
            {
                alert_toast("ไม่พบเลขเอกสาร ไม่สามารถ Update Location ได้", "Error", "error", "8000");
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString;

            // ── ดึงรายการจาก Storage_List (ปลายทางจริง ไม่ใช่ StorageListDT) ──
            string strList =
                " SELECT tool_id, sub_id, system_id," +
                "        HD_Cell,  Use_Cell_Id,  New_Cell," +
                "        HD_Station, Use_Station_Id, New_Station" +
                " FROM Storage_List" +
                " WHERE Doc_No = @Doc_No";

            dc.QryDT_P(conn, strList,
                new SqlParameter("@Doc_No", SqlDbType.NVarChar) { Value = Doc_No });
            DataTable dtList = dc.dt.Copy();

            // counter กันชนภายในเอกสาร (location1_id = 97)
            int nextCellId = -1;
            int nextStationId = -1;

            using (var c = new SqlConnection(connStr))
            {
                c.Open();
                using (var tx = c.BeginTransaction())
                {
                    try
                    {
                        // ── 1) status 99 → 100 (header + list) ──
                        using (var cmd = new SqlCommand(
                            "UPDATE Storage_Control_List SET status_id = 100, update_date = @d WHERE Doc_No = @Doc_No",
                            c, tx))
                        {
                            cmd.Parameters.AddWithValue("@d", DateTime.Now);
                            cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new SqlCommand(
                            "UPDATE Storage_List SET status_id = 100 WHERE Doc_No = @Doc_No",
                            c, tx))
                        {
                            cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                            cmd.ExecuteNonQuery();
                        }

                        // ── 2) Loop update location ทีละ row ──
                        foreach (DataRow r in dtList.Rows)
                        {
                            string toolId = GetVal(r, "tool_id");
                            string subId = GetVal(r, "sub_id");
                            string systemId = GetVal(r, "system_id");

                            string hdCell = GetVal(r, "HD_Cell");
                            string hdStation = GetVal(r, "HD_Station");

                            // ───────────── CELL ─────────────
                            int cellIdToUse = -1;
                            bool doCell = false;

                            if (hdCell == "ddl")
                            {
                                string v = GetVal(r, "Use_Cell_Id");
                                if (!string.IsNullOrEmpty(v))
                                {
                                    cellIdToUse = Convert.ToInt32(v);
                                    doCell = true;
                                }
                            }
                            else if (hdCell == "txt")
                            {
                                string newName = GetVal(r, "New_Cell");
                                if (!string.IsNullOrEmpty(newName))
                                {
                                    if (nextCellId == -1)
                                        nextCellId = GetMaxLocation2_Tx(c, tx) + 1;
                                    else
                                        nextCellId++;

                                    InsertMasterLocation2_Tx(c, tx, nextCellId, newName);
                                    cellIdToUse = nextCellId;
                                    doCell = true;
                                }
                            }

                            if (doCell)
                                UpdateToolLocation_Tx(c, tx, systemId, toolId, subId, "cell", cellIdToUse);

                            // ───────────── STATION ─────────────
                            int stationIdToUse = -1;
                            bool doStation = false;

                            if (hdStation == "ddl")
                            {
                                string v = GetVal(r, "Use_Station_Id");
                                if (!string.IsNullOrEmpty(v))
                                {
                                    stationIdToUse = Convert.ToInt32(v);
                                    doStation = true;
                                }
                            }
                            else if (hdStation == "txt")
                            {
                                string newName = GetVal(r, "New_Station");
                                if (!string.IsNullOrEmpty(newName))
                                {
                                    if (nextStationId == -1)
                                        nextStationId = GetMaxLocation3_Tx(c, tx) + 1;
                                    else
                                        nextStationId++;

                                    InsertMasterLocation3_Tx(c, tx, nextStationId, newName);
                                    stationIdToUse = nextStationId;
                                    doStation = true;
                                }
                            }

                            if (doStation)
                                UpdateToolLocation_Tx(c, tx, systemId, toolId, subId, "station", stationIdToUse);

                            // ✅ Update fields คงที่ (dept_code/division/rsp_empID/status) — ทุก row เสมอ
                            // ⚠️ HARDCODE ภายใน — แก้ทีหลัง
                            UpdateToolFixedFields_Tx(c, tx, systemId, toolId, subId);
                        }

                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { tx.Rollback(); } catch { }
                        alert_toast("Update Location Error : " + ex.Message, "Error", "error", "10000");
                        return;
                    }
                }
            }

            // ── สำเร็จ → alert + redirect ──
            string jsDoc = HttpUtility.JavaScriptStringEncode(Doc_No);
            string script =
                "if (typeof showFullOverlay === 'function') showFullOverlay('กำลังบันทึก...', 'กรุณารอสักครู่');" +
                "if (typeof showToast === 'function') {" +
                "  showToast('Doc No : " + jsDoc + "', { title: 'Confirm Complete', type: 'success', duration: 2000 });" +
                "}" +
                "setTimeout(function () { window.location.href = 'ApproveStatus.aspx'; }, 2000);";

            ScriptManager.RegisterStartupScript(this, this.GetType(), "redirectScript", script, true);
        }

        // ── Max location2_id (Cell) ของ location1_id = 97 ──
        private int GetMaxLocation2_Tx(SqlConnection c, SqlTransaction tx)
        {
            string sql =
                "SELECT ISNULL(MAX(location2_id), 0) FROM jig_tool_system.dbo.master_location2" +
                " WITH (UPDLOCK, HOLDLOCK) WHERE location1_id = 97";
            using (var cmd = new SqlCommand(sql, c, tx))
                return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // ── Max location3_id (Station) ของ location1_id = 97 ──
        private int GetMaxLocation3_Tx(SqlConnection c, SqlTransaction tx)
        {
            string sql =
                "SELECT ISNULL(MAX(location3_id), 0) FROM jig_tool_system.dbo.master_location3" +
                " WITH (UPDLOCK, HOLDLOCK) WHERE location1_id = 97";
            using (var cmd = new SqlCommand(sql, c, tx))
                return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // ── Insert Cell ใหม่ (master_location2) ──
        private void InsertMasterLocation2_Tx(SqlConnection c, SqlTransaction tx, int newId, string name)
        {
            string sql =
                "INSERT INTO jig_tool_system.dbo.master_location2" +
                " (location1_id, location2_id, location2_name, is_use)" +
                " VALUES (97, @id, @name, 1)";
            using (var cmd = new SqlCommand(sql, c, tx))
            {
                cmd.Parameters.AddWithValue("@id", newId);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.ExecuteNonQuery();
            }
        }

        // ── Insert Station ใหม่ (master_location3, location2_id = 1) ──
        private void InsertMasterLocation3_Tx(SqlConnection c, SqlTransaction tx, int newId, string name)
        {
            string sql =
                "INSERT INTO jig_tool_system.dbo.master_location3" +
                " (location1_id, location2_id, location3_id, location3_name)" +
                " VALUES (97, 1, @id, @name)";
            using (var cmd = new SqlCommand(sql, c, tx))
            {
                cmd.Parameters.AddWithValue("@id", newId);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.ExecuteNonQuery();
            }
        }

        // ── Update ปลายทางตาม system_id ──
        //    field = "cell" หรือ "station"
        private void UpdateToolLocation_Tx(SqlConnection c, SqlTransaction tx,
            string systemId, string toolId, string subId, string field, int locId)
        {
            string sql;

            if (systemId == "1")
            {
                // jig_tool_system.dbo.toolSub_list
                string col = (field == "cell") ? "location2_id" : "location3_id";
                sql =
                    "UPDATE jig_tool_system.dbo.toolSub_list SET " + col + " = @locId" +
                    " WHERE tool_id = @toolId AND toolSub_id = @subId";
            }
            else if (systemId == "2")
            {
                // tool_eduipment_system.dbo.Equipment_sub_id
                string col = (field == "cell") ? "cell_id" : "station_id";
                sql =
                    "UPDATE tool_eduipment_system.dbo.Equipment_sub_id SET " + col + " = @locId" +
                    " WHERE Equipment_id = @toolId AND Equipment_sub_id = @subId";
            }
            else
            {
                return; // system_id อื่น ไม่ทำ
            }

            using (var cmd = new SqlCommand(sql, c, tx))
            {
                cmd.Parameters.AddWithValue("@locId", locId);
                cmd.Parameters.AddWithValue("@toolId", toolId);
                cmd.Parameters.AddWithValue("@subId", subId);
                cmd.ExecuteNonQuery();
            }
        }



        // ── Update fields คงที่ตาม system_id (dept_code / division / rsp_empID / status) ──
        // ⚠️ HARDCODE : dept_code=26, division=2, rsp_empID=000198, status=2
        // ⚠️ TODO : เปลี่ยนเป็นค่าจริง/parameter ภายหลัง อย่าปล่อย hardcode ขึ้น production ถาวร
        private void UpdateToolFixedFields_Tx(SqlConnection c, SqlTransaction tx,
            string systemId, string toolId, string subId)
        {
            string sql;

            if (systemId == "1")
            {
                // jig_tool_system.dbo.toolSub_list — status column ชื่อ status_id
                sql =
                    "UPDATE jig_tool_system.dbo.toolSub_list SET " +
                    " dept_code = @dept_code, division = @division, rsp_empID = @rsp_empID, status_id = @status " +
                    " WHERE tool_id = @toolId AND toolSub_id = @subId";
            }
            else if (systemId == "2")
            {
                // tool_eduipment_system.dbo.Equipment_sub_id — status column ชื่อ status_use
                sql =
                    "UPDATE tool_eduipment_system.dbo.Equipment_sub_id SET " +
                    " dept_code = @dept_code, division = @division, rsp_empID = @rsp_empID, status_use = @status " +
                    " WHERE Equipment_id = @toolId AND Equipment_sub_id = @subId";
            }
            else
            {
                return; // system_id อื่น ไม่ทำ
            }

            using (var cmd = new SqlCommand(sql, c, tx))
            {
                // ⚠️ HARDCODE ค่าคงที่ — แก้ทีหลัง
                cmd.Parameters.AddWithValue("@dept_code", 26);
                cmd.Parameters.AddWithValue("@division", 2);
                cmd.Parameters.AddWithValue("@rsp_empID", "000198");
                cmd.Parameters.AddWithValue("@status", 2);

                cmd.Parameters.AddWithValue("@toolId", toolId);
                cmd.Parameters.AddWithValue("@subId", subId);
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region --- Set View Mode ---
        private void SetViewMode(string status_id)
        {
            // --- Show Text Mode
            lb_mode.Text = "Mode : Viewer";

            //Hide Step
            step_requester.Visible = false;
            step_Storagelist.Visible = false;
            step_comment.Visible = false;

            // Disable Search & Add list Panel
            pnlSearch_Add.Visible = false;

            //Gridview Borrow List
            gvStorageList.Columns[1].Visible = false;

            //Hide Button
            if (status_id == "99" || status_id == "100" || status_id == "999") //Update , Completed , Cancel
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
                if (ApproveButton != null) ApproveButton.Visible = false;
            }

            //Comment
            ddlReason_Storage.Enabled = false;
            txt_StorageComment.Enabled = false;

            //Borrow Note
            //borrow_note.Visible = true;

            //Show Comment History
            ShowRejectHistoryIfAny();

        }
        private void SetConfirmMode(string status_id)
        {
            // --- Show Text Mode
            lb_mode.Text = "Mode : Approve";

            //Hide Step
            step_requester.Visible = false;
            step_Storagelist.Visible = false;
            step_comment.Visible = false;

            // Disable Search & Add list Panel
            pnlSearch_Add.Visible = false;

            //Gridview Borrow List
            gvStorageList.Columns[1].Visible = false;

            //Hide Button
            if (status_id == "99" || status_id == "100" || status_id == "999") //Update , Completed , Cancel
            {
                if (ButtonBar != null) ButtonBar.Visible = false;
            }
            else
            {
                if (CancelButton != null) CancelButton.Visible = true;
                if (DraftButton != null) DraftButton.Visible = false;
                if (RejectButton != null) RejectButton.Visible = true;
                if (SendButton != null) SendButton.Visible = false;
                if (ConfirmButton != null) ConfirmButton.Visible = true;
                if (ApproveButton != null) ApproveButton.Visible = false;
            }

            //Borrow Note
            //borrow_note.Visible = true;

            //Show Comment History
            ShowRejectHistoryIfAny();
        }
        private void SetApproveMode(string status_id)
        {
            // --- Show Text Mode
            lb_mode.Text = "Mode : Approve";

            //Hide Step
            step_requester.Visible = false;
            step_Storagelist.Visible = false;
            step_comment.Visible = false;

            // Disable Search & Add list Panel
            pnlSearch_Add.Visible = false;

            //Gridview Borrow List
            gvStorageList.Columns[1].Visible = false;

            //Hide Button
            if (status_id == "99" || status_id == "100" || status_id == "999") //Update , Completed , Cancel
            {
                if (ButtonBar != null) ButtonBar.Visible = false;
            }
            else
            {
                if (CancelButton != null) CancelButton.Visible = true;
                if (DraftButton != null) DraftButton.Visible = false;
                if (RejectButton != null) RejectButton.Visible = true;
                if (SendButton != null) SendButton.Visible = false;
                if (ConfirmButton != null) ConfirmButton.Visible = false;
                if (ApproveButton != null) ApproveButton.Visible = true;
            }

            //Borrow Note
            //borrow_note.Visible = true;

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

                if (c is Button && ((Button)c).ID == "btnRepairConfirm")
                    continue;

                // ✅ recursive ต่อ
                MakeRowReadonly(c);
            }
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
        private DataTable GetModelMaster()
        {
            DataTable dt_model = new DataTable();

            string str_model = "Select * From jig_tool_system.dbo.draw_master_model where model_check = 1 order by model_name";
            dc.QryDT(conn, str_model);
            dt_model = dc.dt;
            return dt_model;
        }
        private DataTable GetReasonMaster()
        {
            DataTable dt_reasonStorage = new DataTable();

            string str_reasonStorage = " select reason_ID , reason_Name , storageFlow , comment_storage , order_storage" +
                                      " from Reason_Borrow_Master" +
                                      " Where storageFlow = 'true'" +
                                      " order by order_storage";
            dc.QryDT(conn, str_reasonStorage);
            dt_reasonStorage = dc.dt;

            //DataTable modelMaster = (DataTable)ViewState["MODEL_MASTER"];
            //ddlReason_Storage.DataSource = dt_reasonStorage;
            //ddlReason_Storage.DataTextField = "reason_Name";
            //ddlReason_Storage.DataValueField = "reason_ID";
            //ddlReason_Storage.DataBind();

            // เพิ่ม -- Select --
            //ddlReason_Storage.Items.Insert(0, new ListItem("-- Select --", ""));
            return dt_reasonStorage;
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

        #region --- Helper : Check Tel / Phone / Blank Data ---

        // --- Check Text Tel (Phone)
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

        // --- Check Change Request Comment
        private bool ValidateChangeComment()
        {
            // ── 1) reason ที่เลือก ──
            string reasonId = ddlReason_Storage.SelectedValue;

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
            string comment = txt_StorageComment.Text.Trim();

            // 4.1 ไม่มี comment
            if (string.IsNullOrEmpty(comment))
            {
                alert_toast("เหตุผลการแจ้งเปลี่ยนแปลงนี้ต้องระบุ Comment", "กรุณากรอก Comment", "warn", "5000");
                txt_StorageComment.Style["border-color"] = "#ffc107";
                txt_StorageComment.Style["background-color"] = "#fff3cd";
                txt_StorageComment.Focus();
                return false;
            }

            // 4.2 มีแต่ไม่ถึง 10 ตัวอักษร
            if (comment.Length < 10)
            {
                alert_toast("กรุณากรอก Comment อย่างน้อย 10 ตัวอักษร", "Comment สั้นเกินไป", "warn", "5000");
                txt_StorageComment.Style["border-color"] = "#ffc107";
                txt_StorageComment.Style["background-color"] = "#fff3cd";
                txt_StorageComment.Focus();
                return false;
            }

            // ── 5) ผ่านครบ → คืน style ปกติ ──
            txt_StorageComment.Style["border-color"] = "";
            txt_StorageComment.Style["background-color"] = "";
            return true;
        }

        // --- Change Blank Data
        private Boolean Check_blankData(out string firstInvalidRowId, out string firstInvalidFieldId)
        {
            DataTable dt = StorageListDT;
            bool hasBlank = false;
            firstInvalidRowId = null;
            firstInvalidFieldId = null;

            foreach (GridViewRow row in gvStorageList.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow) continue;

                // ══════════════ Reason ══════════════
                DropDownList ddlReason = row.FindControl("ddlReason") as DropDownList;
                bool blankReason = ddlReason == null || string.IsNullOrEmpty(ddlReason.SelectedValue);
                SetWarning_DDL(ddlReason, blankReason);
                if (blankReason)
                {
                    hasBlank = true;
                    if (firstInvalidRowId == null)
                    {
                        firstInvalidRowId = "row_" + row.RowIndex;
                        firstInvalidFieldId = ddlReason != null ? ddlReason.ClientID : null;
                    }
                }

                // ══════════════ Service Unit (บังคับเมื่อ Reason = EOL) ══════════════
                TextBox txtService = row.FindControl("txt_service") as TextBox;
                bool isEOL = !blankReason && ddlReason.SelectedItem != null
                             && ddlReason.SelectedItem.Text.Trim().Equals("EOL", StringComparison.OrdinalIgnoreCase);
                bool blankService = isEOL && (txtService == null || string.IsNullOrEmpty(txtService.Text.Trim()));
                SetWarning_TextBox(txtService, blankService);
                if (blankService)
                {
                    hasBlank = true;
                    if (firstInvalidRowId == null)
                    {
                        firstInvalidRowId = "row_" + row.RowIndex;
                        firstInvalidFieldId = txtService != null ? txtService.ClientID : null;
                    }
                }

                // ══════════════ Status ══════════════
                DropDownList ddlStatus = row.FindControl("ddlStatus") as DropDownList;
                bool blankStatus = ddlStatus == null || string.IsNullOrEmpty(ddlStatus.SelectedValue) || ddlStatus.SelectedValue == "0";
                SetWarning_DDL(ddlStatus, blankStatus);
                if (blankStatus)
                {
                    hasBlank = true;
                    if (firstInvalidRowId == null)
                    {
                        firstInvalidRowId = "row_" + row.RowIndex;
                        firstInvalidFieldId = ddlStatus != null ? ddlStatus.ClientID : null;
                    }
                }

                // ══════════════ OK/NG ══════════════
                DropDownList ddlOKNG = row.FindControl("ddlOKNG") as DropDownList;
                bool blankOKNG = ddlOKNG == null || string.IsNullOrEmpty(ddlOKNG.SelectedValue) || ddlOKNG.SelectedValue == "0";
                SetWarning_DDL(ddlOKNG, blankOKNG);
                if (blankOKNG)
                {
                    hasBlank = true;
                    if (firstInvalidRowId == null)
                    {
                        firstInvalidRowId = "row_" + row.RowIndex;
                        firstInvalidFieldId = ddlOKNG != null ? ddlOKNG.ClientID : null;
                    }
                }
            }

            return hasBlank;
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
                "if (typeof showFullOverlay === 'function') showFullOverlay('กำลังบันทึก...', 'กรุณารอสักครู่');" +
                "if (typeof showToast === 'function') {" +
                "  showToast('Doc No : " + jsDoc + "', { title: '" + jsInfo + "', type: 'info', duration: 2000 });" +
                "}" +
                "setTimeout(function () { window.location.href = '" + link_path + "'; }, 2000);";

            ScriptManager.RegisterStartupScript(this, this.GetType(), "redirectScript", script, true);
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
                " from Storage_Control_List WITH (UPDLOCK, HOLDLOCK)" +
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
        /// <summary>หา HiddenField ในแถว — รองรับชื่อ ID ที่ต่างกันระหว่าง Step 2 / Step 3 (hdsystemId / hdSystemId)</summary>
        private static HiddenField FindHd(Control row, params string[] ids)
        {
            if (row == null) return null;
            foreach (string id in ids)
            {
                HiddenField h = row.FindControl(id) as HiddenField;
                if (h != null) return h;
            }
            return null;
        }

        /// <summary>
        /// ตรวจรายการที่ติดกระบวนการอื่น (AAD1–AAD4) — อ้างอิง Borrow2
        /// แจ้งเตือนเองพร้อมระบุรายการ / กระบวนการ / เลขเอกสาร และ mark แถว (ProcLabel) ให้ไฮไลต์
        /// </summary>
        private bool duringBorrowCheck()
        {
            DataTable dt = StorageListDT;
            if (!dt.Columns.Contains("ProcLabel")) dt.Columns.Add("ProcLabel", typeof(string));

            var procMap = GetProcessMap(litIssueNo.Text.Trim());
            var msgs = new List<string>();
            int no = 0;

            foreach (DataRow r in dt.Rows)
            {
                if (r.RowState == DataRowState.Deleted) continue;
                if (GetVal(r, "CompareData") == "Delete") continue;
                no++;
                r["ProcLabel"] = "";
                if (r["ToolId"] == DBNull.Value || r["ToolSub"] == DBNull.Value || r["SystemId"] == DBNull.Value) continue;

                ProcInfo proc;
                if (!procMap.TryGetValue(ProcKey(r["ToolId"], r["ToolSub"], r["SystemId"]), out proc)) continue;

                r["ProcLabel"] = proc.Label + " [" + proc.DocNo + "]";
                string name = GetVal(r, "FixAssetNo") != "" ? GetVal(r, "FixAssetNo")
                            : (GetVal(r, "ControlNo") != "" ? GetVal(r, "ControlNo") : GetVal(r, "ToolNo"));
                msgs.Add("• รายการที่ " + no + " " + name + " → " + proc.Label + " [" + proc.DocNo + "]");
            }

            StorageListDT = dt;
            if (msgs.Count == 0) return false;

            alert_toast("อุปกรณ์ต่อไปนี้อยู่ระหว่างดำเนินการในเอกสารอื่น ไม่สามารถจัดเก็บได้\n" +
                        "กรุณา Remove ออกจากรายการก่อน :\n" + string.Join("\n", msgs.ToArray()),
                        "พบรายการอยู่ระหว่างดำเนินการ", "warn", "20000");
            return true;
        }
        #endregion

        #region --- Helper : FindRowByKey ---
        private DataRow FindRowByKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            string[] p = key.Split('|');
            if (p.Length < 3) return null;
            foreach (DataRow r in StorageListDT.Rows)
            {
                if (GetVal(r, "CompareData") == "Delete") continue;
                if (GetVal(r, "ToolId") == p[0] && GetVal(r, "ToolSub") == p[1] && GetVal(r, "SystemId") == p[2])
                    return r;
            }
            return null;
        }
        #endregion

        private Boolean Check_blankData_Confirm(out string firstInvalidRowId, out string firstInvalidFieldId)
        {
            bool hasBlank = false;
            string firstRowLocal = null;    // ← local แทน out
            string firstFieldLocal = null;

            foreach (GridViewRow row in gvStorageList.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow) continue;

                Action<string, string> markFirst = (rowId, fieldId) =>
                {
                    hasBlank = true;
                    if (firstRowLocal == null)
                    { firstRowLocal = rowId; firstFieldLocal = fieldId; }
                };
                string rid = "row_" + row.RowIndex;

                // Status
                DropDownList ddlStatus = row.FindControl("ddlStatus") as DropDownList;
                bool blankStatus = ddlStatus == null || string.IsNullOrEmpty(ddlStatus.SelectedValue) || ddlStatus.SelectedValue == "0";
                SetWarning_DDL(ddlStatus, blankStatus);
                if (blankStatus) markFirst(rid, ddlStatus != null ? ddlStatus.ClientID : null);

                // OK/NG
                DropDownList ddlOKNG = row.FindControl("ddlOKNG") as DropDownList;
                bool blankOKNG = ddlOKNG == null || string.IsNullOrEmpty(ddlOKNG.SelectedValue) || ddlOKNG.SelectedValue == "0";
                SetWarning_DDL(ddlOKNG, blankOKNG);
                if (blankOKNG) markFirst(rid, ddlOKNG != null ? ddlOKNG.ClientID : null);

                bool isNG = !blankOKNG && ddlOKNG.SelectedValue.Equals("NG", StringComparison.OrdinalIgnoreCase);

                // Repair Status + Confirm By — เฉพาะ NG
                DropDownList ddlRepairStatus = row.FindControl("ddlRepairStatus") as DropDownList;
                DropDownList ddlConfirmBy = row.FindControl("ddlConfirmBy") as DropDownList;

                bool blankRepair = isNG && (ddlRepairStatus == null || string.IsNullOrEmpty(ddlRepairStatus.SelectedValue));
                SetWarning_DDL(ddlRepairStatus, blankRepair);
                if (blankRepair) markFirst(rid, ddlRepairStatus != null ? ddlRepairStatus.ClientID : null);

                bool blankConfirm = isNG && (ddlConfirmBy == null || string.IsNullOrEmpty(ddlConfirmBy.SelectedValue));
                SetWarning_DDL(ddlConfirmBy, blankConfirm);
                if (blankConfirm) markFirst(rid, ddlConfirmBy != null ? ddlConfirmBy.ClientID : null);

                // เหตุผล (Reason)
                DropDownList ddlReason = row.FindControl("ddlReason") as DropDownList;
                bool blankReason = ddlReason == null || string.IsNullOrEmpty(ddlReason.SelectedValue);
                SetWarning_DDL(ddlReason, blankReason);
                if (blankReason) markFirst(rid, ddlReason != null ? ddlReason.ClientID : null);

                // Storage : Cell — มี ddl หรือ txt ก็ได้
                DropDownList ddlCell = row.FindControl("ddlCell") as DropDownList;
                TextBox txtCell = row.FindControl("txtCell") as TextBox;
                bool cellHasVal =
                    (ddlCell != null && ddlCell.Style["display"] != "none" && !string.IsNullOrEmpty(ddlCell.SelectedValue)) ||
                    (txtCell != null && txtCell.Style["display"] != "none" && !string.IsNullOrEmpty(txtCell.Text.Trim()));
                SetWarning_DDL(ddlCell, !cellHasVal);
                SetWarning_TextBox(txtCell, !cellHasVal);
                if (!cellHasVal) markFirst(rid, ddlCell != null ? ddlCell.ClientID : null);

                // Storage : Station — มี ddl หรือ txt ก็ได้
                DropDownList ddlStation = row.FindControl("ddlStation") as DropDownList;
                TextBox txtStation = row.FindControl("txtStation") as TextBox;
                bool stationHasVal =
                    (ddlStation != null && ddlStation.Style["display"] != "none" && !string.IsNullOrEmpty(ddlStation.SelectedValue)) ||
                    (txtStation != null && txtStation.Style["display"] != "none" && !string.IsNullOrEmpty(txtStation.Text.Trim()));
                SetWarning_DDL(ddlStation, !stationHasVal);
                SetWarning_TextBox(txtStation, !stationHasVal);
                if (!stationHasVal) markFirst(rid, ddlStation != null ? ddlStation.ClientID : null);
            }

            firstInvalidRowId = firstRowLocal;      // ← assign out หลังจบ loop
            firstInvalidFieldId = firstFieldLocal;
            return hasBlank;
        }


        // ============================================================
        // Update to Database
        // ============================================================




        // --- Update / Insert Withdraw Control List
        private void InsertControlList(int issueNo, string Doc_No, string status_id)
        {
            //int issueNo = Convert.ToInt32(issue_no);
            DateTime issueDate = DateTime.Parse(txtIssueDate.Text);
            string issue_empid = txtEmpId.Text;
            string dept_code = txtDeptCode.Text;
            int reason_id = 0;
            string comment_text = txt_StorageComment.Text.Trim();

            if (ddlReason_Storage.SelectedIndex > 0)
            {
                int.TryParse(ddlReason_Storage.SelectedValue, out reason_id);
            }

            conn.Open();

            try
            {
                // --- Click Draft Button
                if (status_id == "1")
                {
                    string sql = @"
                    INSERT INTO Withdraw_Control_List
                    (
                        issue_no , issue_date , Doc_No , status_id , issue_empid , dept_code , reason_id , reason_remark , update_date
                    )
                    VALUES
                    (
                        @issue_no , @issue_date , @Doc_No , @status_id , @issue_empid , @dept_code , @reason_id , @reason_remark , @update_date
                    )";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@issue_no", issueNo);
                        cmd.Parameters.AddWithValue("@issue_date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                        cmd.Parameters.AddWithValue("@status_id", status_id);
                        cmd.Parameters.AddWithValue("@issue_empid", issue_empid);
                        cmd.Parameters.AddWithValue("@dept_code", dept_code);
                        cmd.Parameters.AddWithValue("@reason_id", reason_id);
                        cmd.Parameters.AddWithValue("@reason_remark", comment_text);
                        cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                }

                // --- Click Send Button
                else if (status_id == "50")
                {
                    string sql = @"
                    INSERT INTO Withdraw_Control_List
                    (
                        issue_no , issue_date , Doc_No , status_id , issue_empid , dept_code , reason_id , reason_remark , update_date , send_date
                    )
                    VALUES
                    (
                        @issue_no , @issue_date , @Doc_No , @status_id , @issue_empid , @dept_code , @reason_id , @reason_remark , @update_date , @send_date
                    )";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@issue_no", issueNo);
                        cmd.Parameters.AddWithValue("@issue_date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                        cmd.Parameters.AddWithValue("@status_id", status_id);
                        cmd.Parameters.AddWithValue("@issue_empid", issue_empid);
                        cmd.Parameters.AddWithValue("@dept_code", dept_code);
                        cmd.Parameters.AddWithValue("@reason_id", reason_id);
                        cmd.Parameters.AddWithValue("@reason_remark", comment_text);
                        cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@send_date", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                }

                // --- Click AAD Confirm Button (AAD Confirm2)
                else if (status_id == "99")
                {
                    string emp_confirm = Session["emp_id"].ToString().Trim();

                    string sql = @"
                    INSERT INTO Withdraw_Control_List
                    (
                        issue_no , issue_date , Doc_No , status_id , issue_empid , dept_code , reason_id , reason_remark , update_date , send_date , confirm_empid , confirm_date
                    )
                    VALUES
                    (
                        @issue_no , @issue_date , @Doc_No , @status_id , @issue_empid , @dept_code , @reason_id , @reason_remark , @update_date , @send_date , @confirm_empid , @confirm_date
                    )";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@issue_no", issueNo);
                        cmd.Parameters.AddWithValue("@issue_date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                        cmd.Parameters.AddWithValue("@status_id", status_id);
                        cmd.Parameters.AddWithValue("@issue_empid", issue_empid);
                        cmd.Parameters.AddWithValue("@dept_code", dept_code);
                        cmd.Parameters.AddWithValue("@reason_id", reason_id);
                        cmd.Parameters.AddWithValue("@reason_remark", comment_text);
                        cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@send_date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@confirm_empid", emp_confirm);
                        cmd.Parameters.AddWithValue("@confirm_date", DateTime.Now);
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
                conn.Close();

                // Insert Data to Sql Server
                InsertBorrowList(Doc_No, status_id);
            }
        }
        // --- Update / Insert Withdraw List
        private void InsertBorrowList(string Doc_No, string status_id)
        {
            DataTable dt = StorageListDT;
            conn.Open();

            try
            {
                foreach (DataRow row in dt.Rows)
                {
                    string sql = @"
                    INSERT INTO Storage_List
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

                alert_toast("Doc No : " + Doc_No, alert_info, "info", "2000");


                string script = @"
                    if (typeof showFullOverlay === 'function') showFullOverlay('กำลังบันทึก...', 'กรุณารอสักครู่');

                    if (typeof alert_toast === 'function') {
                        alert_toast('Doc No : " + Doc_No + @"', 'Save Draft', 'info', '2000');
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






        




        #region --- Helper : On Process Check (AAD1–AAD4) — อ้างอิง Borrow2 ---

        [Serializable]
        private class ProcInfo
        {
            public string Label;   // "AAD1 : ยืม"
            public string DocNo;   // "AAD1-26-00039"
            public string Url;     // borrow_tool2.aspx?k=...&mode=view
        }

        private static string ProcKey(object toolId, object subId, object systemId)
        {
            return Convert.ToString(toolId).Trim() + "|" + Convert.ToString(subId).Trim() + "|" + Convert.ToString(systemId).Trim();
        }

        /// <summary>
        /// อุปกรณ์ทั้งหมดที่ติดกระบวนการอยู่ (1 &lt; status_id &lt; 100) — key = tool_id|sub_id|system_id
        /// ไม่รวมเอกสารจัดเก็บฉบับปัจจุบัน (currentDocNo) / ตัวเดียวติดหลายเอกสาร → เอาตามลำดับ AAD1 → AAD4
        /// </summary>
        private Dictionary<string, ProcInfo> GetProcessMap(string currentDocNo)
        {
            var map = new Dictionary<string, ProcInfo>(StringComparer.OrdinalIgnoreCase);

            string sql = @"
                SELECT 1 AS ord, Doc_No, tool_id, sub_id, system_id FROM Borrow_List
                 WHERE status_id > 1 AND status_id < 100
                UNION ALL
                SELECT 2, Doc_No, tool_id, sub_id, system_id FROM NotificationofChange_List
                 WHERE status_id > 1 AND status_id < 100
                UNION ALL
                SELECT 3, Doc_No, tool_id, sub_id, system_id FROM Withdraw_List
                 WHERE status_id > 1 AND status_id < 100
                UNION ALL
                SELECT 4, Doc_No, tool_id, sub_id, system_id FROM Storage_List
                 WHERE status_id > 1 AND status_id < 100 AND Doc_No <> @doc
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

        #region --- Manual : Upload (Admin) — อ้างอิง Borrow2 ---

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

        #endregion

        // ── Helper : เลือก CSS class ตาม DisplayStatus ────────────────────────
        protected string GetStatusBadgeClass(string displayStatus)
        {
            displayStatus = displayStatus ?? "";

            if (displayStatus.StartsWith("AAD") || displayStatus == "อยู่ระหว่างยืม")
                return "badge-borrowing";          // แดง — ติดกระบวนการ

            if (displayStatus.StartsWith("Spare"))
                return "badge-spare";              // สีเหลือง (มีอยู่แล้ว)

            return "badge-normal";                 // สีเขียว (มีอยู่แล้ว)
        }

        // ✅ ใช้แทน local function — .NET 4.0 compatible
        private string GetRowVal(DataRowView row, string colName)
        {
            if (!row.Row.Table.Columns.Contains(colName)) return "";
            return row[colName] == DBNull.Value ? "" : row[colName].ToString().Trim();
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



        // ============================================================

        
       

      
        



        // =================== Not Use ??

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
