using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClosedXML.Excel;

namespace Tool_Equipment_Control_System
{
    public partial class Tool_equipment_MasterList : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        datacon dc = new datacon();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        private string SortField
        {
            get { return ViewState["SortField"] as string ?? ""; }
            set { ViewState["SortField"] = value; }
        }
        private string SortDirection
        {
            get { return ViewState["SortDir"] as string ?? "ASC"; }
            set { ViewState["SortDir"] = value; }
        }
        public override void VerifyRenderingInServerForm(Control control)
        {
            // ข้าม verify เพื่อให้ RenderControl ทำงานได้ตอน export
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if(Session["emp_id"] == null || Session["emp_id"].ToString()=="")
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                Brind_data();this.RegisterPostBackControl();
                Bind_ddl_dept();
                Bind_ddl_category();
                Bind_ddl_status();
                Bind_ddl_responsible();
            }            
        }
        private void RegisterPostBackControl()
        {
            ScriptManager.GetCurrent(this).RegisterPostBackControl(gv_masterList);
            foreach (GridViewRow row in gv_masterList.Rows)
            {
                LinkButton btn_pic = row.FindControl("btn_pic") as LinkButton;
                ScriptManager.GetCurrent(this).RegisterAsyncPostBackControl(btn_pic);
            }
            
        }
        protected string get_where()
        {
            string where = "";

            // --- Status
            //  ไม่เลือก        -> แสดงทั้งหมด ยกเว้น Transfer (11) / Scrap (12)
            //  Transfer (11)  -> เฉพาะรายการที่มีใน History_Transfer
            //  Scrap (12)     -> เฉพาะรายการที่มีใน History_Scrap
            //  สถานะอื่น       -> status_id ตามที่เลือก
            string selStatus = ddl_status.SelectedValue;
            if (selStatus == STATUS_TRANSFER)
                where += " and status_id = 11 " + HistoryExists("History_Transfer");
            else if (selStatus == STATUS_SCRAP)
                where += " and status_id = 12 " + HistoryExists("History_Scrap");
            else if (selStatus != "")
                where += " and status_id = " + selStatus;
            else
                where += " and status_id not in (11,12) ";

            if (ddl_dept.SelectedValue != "")
                where += " and dept_id = " + ddl_dept.SelectedValue.ToString();

            if (ddlResponsible.SelectedValue != "")
                where += " and rsp_empID = " + ddlResponsible.SelectedValue.ToString();

            if (ddl_category.SelectedValue != "")
                where += " and is_category = "+ ddl_category.SelectedValue.ToString();

            if (txt_cptmodel.Text != "")
                where += " and model_name like '%" + txt_cptmodel.Text.Trim() + "%' ";

            //if (ddl_location.SelectedValue != "")
            //    where += " and location1_id = " + ddl_location.SelectedValue.ToString();

            if (txt_cell.Text != "")
                where += " and cell like '%" + txt_cell.Text.Trim() + "%' ";

            if (txt_station.Text != "")
                where += " and station like '%" + txt_station.Text.Trim() + "%' ";

            // fix /non fix
            if (ddl_nonFix.SelectedValue == "0")
            {
                where += " and (FixAcces is null or FixAcces = N'-') ";
            }
            else if (ddl_nonFix.SelectedValue == "1")
            {
                where += " and (FixAcces is not null AND FixAcces != N'-' )";
            }

            return where;
        }

        private const string STATUS_TRANSFER = "11";
        private const string STATUS_SCRAP = "12";

        // เงื่อนไข EXISTS: ต้องมีประวัติใน History table (Tool_id + Tool_sub + System_id ตรงกับ View)
        // ใช้ EXISTS เพื่อไม่ให้แถวซ้ำ กรณี 1 รายการมีประวัติหลายครั้ง
        // CAST เป็น nvarchar เพราะชนิดคอลัมน์ไม่ตรงกัน (History_Transfer.System_id = int, History_Scrap.System_id = nvarchar)
        private static string HistoryExists(string historyTable)
        {
            return " and EXISTS ( select 1 from dbo." + historyTable + " h "
                 + " where CAST(h.Tool_id AS nvarchar(50)) = CAST(Tool_Equipment_List_View.tool_id AS nvarchar(50)) "
                 + " and CAST(h.Tool_sub AS nvarchar(50)) = CAST(Tool_Equipment_List_View.sub_no AS nvarchar(50)) "
                 + " and CAST(h.System_id AS nvarchar(10)) = CAST(Tool_Equipment_List_View.sys_status AS nvarchar(10)) ) ";
        }
        private void Bind_ddl_status()
        {
            // --- รวม Transfer (11) / Scrap (12) ไว้ท้ายรายการ (ค่า default ยังไม่แสดง 2 สถานะนี้ — ดู get_where)
            string str2 = " select * from tool_equipment_status order by case when status_id in (11,12) then 1 else 0 end, status_id ";

            dc.QryDT(conn, str2);

            ddl_status.DataSource = dc.dt;
            ddl_status.DataTextField = "status_name";
            ddl_status.DataValueField = "status_id";
            ddl_status.DataBind();

            // เพิ่ม -- Select --
            ddl_status.Items.Insert(0, new ListItem("-- ทั้งหมด --", ""));
        }
        private void Bind_ddl_dept()
        {
            string str2 = " select * from View_dept_master Where division = '2' order by dept_code ";

            dc.QryDT(conn, str2);
            ddl_dept.DataSource = dc.dt;
            ddl_dept.DataTextField = "dept_name2";
            ddl_dept.DataValueField = "dept_id";
            ddl_dept.DataBind();

            // เพิ่ม -- Select --
            ddl_dept.Items.Insert(0, new ListItem("-- ทั้งหมด --", ""));
        }
        private void Bind_ddl_category()
        {
            string str2 = " select * from jig_tool_system.dbo.jt_category_master ";

            dc.QryDT(conn, str2);

            ddl_category.DataSource = dc.dt;
            ddl_category.DataTextField = "category";
            ddl_category.DataValueField = "id";
            ddl_category.DataBind();

            // เพิ่ม -- Select --
            ddl_category.Items.Insert(0, new ListItem("-- ทั้งหมด --", ""));
        }
        private void Bind_ddl_responsible()
        {
            string str2 = " select emp_id , emp_id + ' ' + pic_dept as respon from Responsible_Dept group by emp_id , pic_dept order by emp_id ";

            dc.QryDT(conn, str2);

            ddlResponsible.DataSource = dc.dt;
            ddlResponsible.DataTextField = "respon";
            ddlResponsible.DataValueField = "emp_id";
            ddlResponsible.DataBind();

            // เพิ่ม -- Select --
            ddlResponsible.Items.Insert(0, new ListItem("-- ทั้งหมด --", ""));
        }
        protected void Brind_data()
        {
            string str = "";
            string where = get_where();
            string keyword = "";
            string textsearch = txtSearchlist.Text;

            if (!string.IsNullOrEmpty(textsearch))
            {
                keyword += "( ID LIKE '%" + textsearch + "%'";
                keyword += " or FixAcces LIKE '%" + textsearch + "%'";
                keyword += " or asset_name LIKE '%" + textsearch + "%'";
                keyword += " or control_no LIKE '%" + textsearch + "%'";
                keyword += " or serial_no LIKE '%" + textsearch + "%'";
                keyword += " or tool_no LIKE '%" + textsearch + "%' ) And "  ;
            }

            // --- เงื่อนไข Status (รวม Transfer/Scrap) อยู่ใน get_where()
            str +="  select * ,( Case when DATEDIFF(YEAR, start_use,GETDATE()) > 0 then CAST(DATEDIFF(YEAR, start_use,GETDATE()) AS varchar(5)) +' Year 'Else '' end   ";
            str += "  + Case when DATEDIFF(MONTH , start_use,GETDATE()) % 12  > 0 then CAST(DATEDIFF(MONTH , start_use,GETDATE()) % 12 AS varchar(5) )+' Month ' else '' end ) as useful_date   ";
            str += "  from Tool_Equipment_List_View  where " + keyword + "ID is not null  "+ where;
            dc.QryDT(conn, str);

            //test.Text = str;
            //gv_masterList.DataSource = dc.dt;
            //gv_masterList.DataBind();

            if (!string.IsNullOrEmpty(SortField) && dc.dt.Columns.Contains(SortField))
            {
                dc.dt.DefaultView.Sort = "[" + SortField + "] " + SortDirection;
                gv_masterList.DataSource = dc.dt.DefaultView;
            }
            else
            {
                gv_masterList.DataSource = dc.dt;
            }
            gv_masterList.DataBind();


            int totalRecords = dc.dt.Rows.Count; // จะได้รับจำนวนข้อมูลทั้งหมดใน DataTable ของคุณ
            int currentPage = gv_masterList.PageIndex + 1;
            int pageSize = int.Parse(ddl_pageSize.SelectedValue);

            int startRecord = pageSize * (currentPage - 1) + 1;
            int endRecord = pageSize * currentPage;

            lblRecordCount.Text = "Showing " + startRecord.ToString() + " to " + endRecord.ToString() + " of " + totalRecords.ToString() +" entries";
            //lblRecordCount.Text = $"Showing {startRecord} to {endRecord} of {totalRecords} entries";

        }


        // เพิ่ม event ddl_pageSize
        protected void ddl_pageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            int pageSize = int.Parse(ddl_pageSize.SelectedValue);
            gv_masterList.PageSize = pageSize;
            gv_masterList.PageIndex = 0; // reset กลับหน้าแรก
            Brind_data(); // เรียก method bind ข้อมูลของคุณ
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gv_masterList.PageIndex = e.NewPageIndex;
            Brind_data(); // Your method to rebind the GridView with the new page data
        }
        protected void btn_search_Click(object sender, EventArgs e)
        {
            Brind_data();
        }
        protected void text_search_TextChanged(object sender, EventArgs e)
        {
            Brind_data();
        }
        protected void tddlResponsible_TextChanged(object sender, EventArgs e)
        {

            Brind_data();
        }

        protected void btn_detailView_Click(object sender, EventArgs e)
        {

            GridViewRow row = (sender as LinkButton).NamingContainer as GridViewRow;
            LinkButton LinkButton1 = (LinkButton)row.FindControl("LinkButton1");
            HiddenField hdtool_id = (HiddenField)row.FindControl("hdtool_id");
            HiddenField hdtool_sub = (HiddenField)row.FindControl("hdtool_sub");
            HiddenField hd_sys_status = (HiddenField)row.FindControl("hd_sys_status");
            string[] tool_id = hdtool_id.Value.Split(',');
            string[] tool_sub = hdtool_sub.Value.Split(',');
            string[] sys_status = hd_sys_status.Value.Split(',');
            //string oldUrl_detail = HttpContext.Current.Request.Url.AbsoluteUri;
            //Session["oldUrl_master"] = oldUrl_detail;
            string url = "Tool_equipment_detail.aspx?tool_id="+ tool_id[0]+"&tool_sub="+ tool_sub[0]+ "&sys_status="+ sys_status[0];
            //Response.Redirect("Tool_equipment_detail.aspx?tool_id="+ tool_id[0]+"&tool_sub="+ tool_sub[0]);
            sb.Append(@"<script type='text/javascript'>");
            sb.Append("window.open('" + url + "','_blank');");
            sb.Append(@"</script>");
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "closemodal", sb.ToString(), false);

            Brind_data();
        }
        protected void btn_borrow_Click(object sender, EventArgs e)
        {

            GridViewRow row = (sender as LinkButton).NamingContainer as GridViewRow;
            LinkButton LinkButton1 = (LinkButton)row.FindControl("LinkButton1");
            HiddenField hdtool_id = (HiddenField)row.FindControl("hdtool_id");
            HiddenField hdtool_sub = (HiddenField)row.FindControl("hdtool_sub");
            HiddenField hd_sys_status = (HiddenField)row.FindControl("hd_sys_status");
            string[] tool_id = hdtool_id.Value.Split(',');
            string[] tool_sub = hdtool_sub.Value.Split(',');
            string[] sys_status = hd_sys_status.Value.Split(',');
            //string oldUrl_detail = HttpContext.Current.Request.Url.AbsoluteUri;
            //Session["oldUrl_master"] = oldUrl_detail;
            //string url = "Tool_equipment_detail.aspx?tool_id=" + tool_id[0] + "&tool_sub=" + tool_sub[0] + "&sys_status=" + sys_status[0];
            string url = "borrow_Tool.aspx?tool_id=" + tool_id[0] + "&tool_sub=" + tool_sub[0] + "&sys_status=" + sys_status[0];
            //Response.Redirect("Tool_equipment_detail.aspx?tool_id="+ tool_id[0]+"&tool_sub="+ tool_sub[0]);
            sb.Append(@"<script type='text/javascript'>");
            sb.Append("window.open('" + url + "','_blank');");
            sb.Append(@"</script>");
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "closemodal", sb.ToString(), false);

            Brind_data();
        }
        protected void btn_withdraw_Click(object sender, EventArgs e)
        {

            GridViewRow row = (sender as LinkButton).NamingContainer as GridViewRow;
            LinkButton LinkButton1 = (LinkButton)row.FindControl("LinkButton1");
            HiddenField hdtool_id = (HiddenField)row.FindControl("hdtool_id");
            HiddenField hdtool_sub = (HiddenField)row.FindControl("hdtool_sub");
            HiddenField hd_sys_status = (HiddenField)row.FindControl("hd_sys_status");
            string[] tool_id = hdtool_id.Value.Split(',');
            string[] tool_sub = hdtool_sub.Value.Split(',');
            string[] sys_status = hd_sys_status.Value.Split(',');
            //string oldUrl_detail = HttpContext.Current.Request.Url.AbsoluteUri;
            //Session["oldUrl_master"] = oldUrl_detail;            
            string url = "withdraw.aspx?tool_id=" + tool_id[0] + "&tool_sub=" + tool_sub[0] + "&sys_status=" + sys_status[0];
            //Response.Redirect("Tool_equipment_detail.aspx?tool_id="+ tool_id[0]+"&tool_sub="+ tool_sub[0]);
            sb.Append(@"<script type='text/javascript'>");
            sb.Append("window.open('" + url + "','_blank');");
            sb.Append(@"</script>");
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "closemodal", sb.ToString(), false);

            Brind_data();
        }
        protected void btn_storage_Click(object sender, EventArgs e)
        {

            GridViewRow row = (sender as LinkButton).NamingContainer as GridViewRow;
            LinkButton LinkButton1 = (LinkButton)row.FindControl("LinkButton1");
            HiddenField hdtool_id = (HiddenField)row.FindControl("hdtool_id");
            HiddenField hdtool_sub = (HiddenField)row.FindControl("hdtool_sub");
            HiddenField hd_sys_status = (HiddenField)row.FindControl("hd_sys_status");
            string[] tool_id = hdtool_id.Value.Split(',');
            string[] tool_sub = hdtool_sub.Value.Split(',');
            string[] sys_status = hd_sys_status.Value.Split(',');
            //string oldUrl_detail = HttpContext.Current.Request.Url.AbsoluteUri;
            //Session["oldUrl_master"] = oldUrl_detail;            
            string url = "storage.aspx?tool_id=" + tool_id[0] + "&tool_sub=" + tool_sub[0] + "&sys_status=" + sys_status[0];
            //Response.Redirect("Tool_equipment_detail.aspx?tool_id="+ tool_id[0]+"&tool_sub="+ tool_sub[0]);
            sb.Append(@"<script type='text/javascript'>");
            sb.Append("window.open('" + url + "','_blank');");
            sb.Append(@"</script>");
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "closemodal", sb.ToString(), false);

            Brind_data();
        }
        protected void btn_pic_Click(object sender, EventArgs e)
        {
            GridViewRow row = (sender as LinkButton).NamingContainer as GridViewRow;
            HiddenField hdtool_id = (HiddenField)row.FindControl("hdtool_id");
            HiddenField hdtool_sub = (HiddenField)row.FindControl("hdtool_sub");
            HiddenField hd_sys_status = (HiddenField)row.FindControl("hd_sys_status");

            string[] tool_id = hdtool_id.Value.Split(',');
            string[] tool_sub = hdtool_sub.Value.Split(',');
            string[] sys_status = hd_sys_status.Value.Split(',');

            Get_Folder_Structure_Picture Folder_Structure_Picture = new Get_Folder_Structure_Picture(tool_id[0],tool_sub[0],sys_status[0]);
            string get_path_toolSub = Folder_Structure_Picture.get_folder_root();
            string path_picture_toolMain = Folder_Structure_Picture.get_folder_root_view_toolMain();
           // lb_testPic.Text = tool_id[0] + "/" + hdtool_sub.Value.ToString()+"/" + sys_status ;
             string[] files_main = Directory.GetFiles(get_path_toolSub, "*.jpg", SearchOption.TopDirectoryOnly);

            //if (files_main.Length > 0)
            //{
            // lb_testPic.Text = path_picture_toolMain;
            //imgMain.ImageUrl = path_picture_toolMain;

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

                        // ตอนนี้คุณมีขนาดรูปภาพแล้ว (imageWidth, imageHeight)
                        // คุณสามารถนำไปใช้กับ lb_testPic.Text หรือแสดงผลใน Console/Debug ได้
                        // ตัวอย่าง: lb_testPic.Text = $"Width: {imageWidth}px, Height: {imageHeight}px";
                        // หรือ Console.WriteLine($"Image dimensions: {imageWidth}x{imageHeight}");

                        // กำหนด ImageUrl ให้กับคอนโทรล imgMain
                        imgMain.ImageUrl = path_picture_toolMain;

                        imgMain.Width = imageWidth;
                        imgMain.Height = imageHeight;

                        //lb_testPic.Text = imageWidth.ToString() + ":" + imageHeight.ToString();

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
                        //else if (imageWidth < maxWidth && imageHeight < maxHeight)
                        //{
                        //    imgMain.Width = imageWidth;
                        //    imgMain.Height = imageHeight;
                        //}
                    }
                }
                catch (Exception ex)
                {
                    // จัดการข้อผิดพลาด เช่น ไฟล์ไม่ถูกต้อง หรือเข้าถึงไม่ได้
                    // Console.WriteLine($"Error loading image: {ex.Message}");
                    imgMain.ImageUrl = "images/FilesType/jpeg.png"; // แสดงภาพ default หากเกิดข้อผิดพลาด
                }
            }
            else
            {
                 imgMain.ImageUrl = "images/FilesType/jpeg.png";
            }

            Brind_data();

            sb.Append(@"<script type='text/javascript'>");
            sb.Append("$('#modal_UploadPictureMain').modal('show')");
            sb.Append(@"</script>");
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "closemodal", sb.ToString(), false);
            //return;
        }

        protected void btn_excel_Click(object sender, EventArgs e)
        {
            gv_masterList.AllowPaging = false;
            Brind_data();

            // ซ่อนคอลัมน์ Action(index 1) และ Picture(index 2) ก่อน export
            gv_masterList.Columns[1].Visible = false;
            gv_masterList.Columns[2].Visible = false;

            string style = @"<style> .text { mso-number-format:\@;  } </style> ";

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader("content-disposition",
                "attachment;filename=ToolEquipmentMasterList_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xls");
            HttpContext.Current.Response.Charset = "utf-8";
            HttpContext.Current.Response.ContentType = "application/ms-excel";

            byte[] BOM = { 0xEF, 0xBB, 0xBF };
            Response.BinaryWrite(BOM);

            System.IO.StringWriter stringWrite = new System.IO.StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);
            gv_masterList.RenderControl(htmlWrite);

            HttpContext.Current.Response.Write(style);
            HttpContext.Current.Response.Write(stringWrite.ToString());
            HttpContext.Current.Response.End();
        }
        protected void btn_clear_Click(object sender, EventArgs e)
        {
            txtSearchlist.Text = "";
            txt_cptmodel.Text = "";
            txt_cell.Text = "";
            txt_station.Text = "";

            ddl_status.SelectedValue = "";
            ddl_dept.SelectedValue = "";
            ddlResponsible.SelectedValue = "";
            ddl_category.SelectedValue = "";
            ddl_nonFix.SelectedValue = "";

            // reset sort + กลับหน้าแรก
            SortField = "";
            SortDirection = "ASC";
            gv_masterList.PageIndex = 0;

            Brind_data();
        }

        protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                foreach (TableCell cell in e.Row.Cells)
                {
                    LinkButton lb = cell.Controls.OfType<LinkButton>().FirstOrDefault();
                    if (lb == null) continue;

                    string expr = lb.CommandArgument;
                    string arrow = expr == SortField ? (SortDirection == "ASC" ? " ▲" : " ▼") : " ⇅";
                    cell.Controls.Add(new LiteralControl("<span style='font-size:10px;opacity:.8;'>" + arrow + "</span>"));
                }
            }

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // ตรวจสอบให้แน่ใจว่าคอลัมน์สถานะ (Cells[3]) มีข้อมูล
                if (e.Row.Cells.Count > 3)
                {
                    string statusText = e.Row.Cells[3].Text; // ดึงข้อความสถานะ

                    // สร้าง LiteralControl เพื่อเพิ่ม HTML เข้าไปในเซลล์
                    LiteralControl statusDot = new LiteralControl();
                    string cssClass = "";
                    string displayStatusText = statusText; // ข้อความที่จะแสดงในวงกลม/วงรี

                    switch (statusText)
                    {
                        case "Use":
                            cssClass = "status-dot status-use";
                            break;
                        case "Spare":
                            cssClass = "status-dot status-spare";
                            break;
                        case "Repair":
                            cssClass = "status-dot status-repair";
                            break;
                        case "NG":
                            cssClass = "status-dot status-ng";
                            break;
                        case "EOL":
                            cssClass = "status-dot status-eol";
                            break;
                        case "Do Not Use":
                            cssClass = "status-dot status-donotuse";
                            break;
                        case "Wait Consider":
                            cssClass = "status-dot status-waitconsider";
                            break;
                        case "Scrap":
                            cssClass = "status-dot status-scrap";
                            break;
                        case "Transfer":
                            cssClass = "status-dot status-transfer";
                            break;
                        case "Dispose":
                            cssClass = "status-dot status-dispose";
                            break;
                        default:
                            cssClass = "status-dot status-default"; // สำหรับสถานะอื่นๆ หรือหากไม่มี
                            break;
                    }

                    // สร้าง HTML สำหรับวงกลม/วงรี และข้อความสถานะภายใน
                    statusDot.Text = string.Format("<span class='{0}'>{1}</span>", cssClass, displayStatusText);

                    // ล้างเนื้อหาเดิมของเซลล์และเพิ่ม LiteralControl เข้าไป
                    e.Row.Cells[3].Controls.Clear(); // สำคัญ: ล้างเนื้อหาเดิมออกก่อน
                    e.Row.Cells[3].Controls.Add(statusDot);
                }
            }
        }
        protected void gv_masterList_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (SortField == e.SortExpression)
                SortDirection = SortDirection == "ASC" ? "DESC" : "ASC";
            else
            {
                SortField = e.SortExpression;
                SortDirection = "ASC";
            }

            gv_masterList.PageIndex = 0;
            Brind_data();
        }
    }
}