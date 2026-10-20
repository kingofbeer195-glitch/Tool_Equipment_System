using ClosedXML.Excel;
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
    public partial class tool_equipment_tana_report : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        //SqlConnection conn_jig = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString);

        datacon dc = new datacon();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        string dept_code;
        string id_tana;
        string rsp_id;
        string model_id;
        string location_id;
        string cell_id;
        string tana_date;
        string emp_id;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["data_tana_report"] == null || Session["data_tana_report"].ToString() == "")
                Response.Redirect("tool_equipment_tanaoroshi.aspx");


            emp_id = Session["emp_id"].ToString();
            Get_data_tana_Approve();

            if (!IsPostBack)
            {

                Get_gv_tana_report();

            }
            //this.RegisterPostBackControl();

        }
        private void RegisterPostBackControl()
        {
            ScriptManager.GetCurrent(this).RegisterPostBackControl(btn_excel);
            //foreach (GridViewRow row in GridViewCheck.Rows)
            //{
            //    LinkButton btn_pmNo = row.FindControl("btn_pmNo") as LinkButton;
            //    ScriptManager.GetCurrent(this).RegisterPostBackControl(btn_pmNo);
            //}
        }
        protected void Get_data_tana_Approve()
        {

            Dictionary<string, string> myData = (Dictionary<string, string>)Session["data_tana_report"];

            dept_code = myData["dept_code"].ToString();
            rsp_id = myData["rsp_id"].ToString();
            model_id = myData["model_id"].ToString();
            location_id = myData["location_id"].ToString();
            cell_id = myData["cell_id"].ToString();
            tana_date = myData["tana_date"].ToString();
            id_tana = myData["id_tana"].ToString();

            string sql = " select * from [View_Equipment_tana_list] where id = " + id_tana;
            dc.QryDT(conn, sql);
            if (dc.dt.Rows.Count > 0)
            {
                txt_model.Text = dc.dt.Rows[0]["model_name"].ToString();
                txt_checkBy.Text = (dc.dt.Rows[0]["check_byName"].ToString() == "" ? Session["user_name"].ToString() : dc.dt.Rows[0]["check_byName"].ToString());
                txt_check_date.Text = (dc.dt.Rows[0]["check_date"].ToString() == "" ? DateTime.Now.ToString("dd-MMM-yyyy") : Convert.ToDateTime(dc.dt.Rows[0]["check_date"]).ToString("dd-MMM-yyyy"));
                txt_rsp.Text = dc.dt.Rows[0]["pic_dept"].ToString();
                txt_dept.Text = dc.dt.Rows[0]["dept_name_full"].ToString();
            }

           
        }
        protected DataTable tb_tana()
        {
            
            DataTable dt = new DataTable();
            string where = "";
            if (model_id != "" && model_id != "0")
                where += " and model_id = '" + model_id + "' ";
            if (location_id != "" && location_id != "0")
                where += " and location_id = '" + location_id + "' ";
            if (cell_id != "" && cell_id != "0")
                where += " and cell_id = '" + cell_id + "' ";

            string sql = "select* from View_Equipment_tana_history where tana_date = '" + tana_date + "' " +
            "and dept_code = '" + dept_code + "' and pic_rsp = '" + rsp_id + "'  " + where;// and model_id = '"+model_id+"'   and cell_id = '"+cell_id+"' ";

            dc.QryDT(conn, sql);

            //lb_test.Text = sql;
            return dc.dt;
        }
        protected void Get_gv_tana_report()
        {
            DataTable dataTable = tb_tana();

            tana_report.DataSource = dataTable;
            tana_report.DataBind();
        }
        protected void tana_report_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                HiddenField hd_tool_id = (HiddenField)e.Row.FindControl("hd_tool_id");
                HiddenField hd_sys_status = (HiddenField)e.Row.FindControl("hd_sys_status");
                HiddenField hd_tool_sub = (HiddenField)e.Row.FindControl("hd_tool_sub_no");
                HiddenField hd_check_id = (HiddenField)e.Row.FindControl("hd_check_id");
                Image img_tool = (Image)e.Row.FindControl("img_tool");

                string[] tool_id = hd_tool_id.Value.Split(',');
                string[] tool_sub = hd_tool_sub.Value.Split(',');
                string[] sys_status = hd_sys_status.Value.Split(',');

                //Get Picture Tool 
                Get_Folder_Structure_Picture Folder_Structure_Picture = new Get_Folder_Structure_Picture(tool_id[0], tool_sub[0], sys_status[0]);
                string get_path_toolSub = Folder_Structure_Picture.get_folder_root();
                string path_picture_toolMain = Folder_Structure_Picture.get_folder_root_view_toolMain();
                //    lb_testPic.Text = path_picture_toolMain;
                //lb_check.Text = check_id;
                string[] files_main = Directory.GetFiles(get_path_toolSub, "*.jpg", SearchOption.TopDirectoryOnly);

                if (files_main.Length > 0)
                {
                 
                    img_tool.ImageUrl = path_picture_toolMain;
                }
                else
                {
                 
                    img_tool.ImageUrl = "images/FilesType/jpeg.png";
                }
            }
        }
        protected void btn_excel_Cilck(object sender, EventArgs e)
        {
            get_excel();
        }
        protected void get_excel()
        {
            try
            {
                DataTable dt = tb_tana(); // GridViewToDataTable(tana_report);
               
                if (dt.Rows.Count == 0)
                {
                    alert_Messend("Not Have Data");
                    return;
                }
                //gv_test.DataSource = dt;
                //gv_test.DataBind();
                //return;
              

                using (var workbook = new XLWorkbook())
                {
                    
                    var ws = workbook.Worksheets.Add("Sheet1");
              
                    ws.Cell("A1").Value = "No";
                    ws.Cell("B1").Value = "Picture";
                    ws.Cell("C1").Value = "Result";
                    ws.Cell("D1").Value = "Asset No";
                    ws.Cell("E1").Value = "Seb Asset No.";
                    ws.Cell("F1").Value = "Control No";
                    ws.Cell("G1").Value = "Name";
                    ws.Cell("H1").Value = "Model";
                    ws.Cell("I1").Value = "Location";
                    ws.Cell("J1").Value = "Cell";
                    ws.Cell("K1").Value = "Station";
                    ws.Cell("L1").Value = "Remark";

                    //กำหนดขนาด column
                    ws.Column("A").Width = 10;
                    ws.Column("B").Width = 40;
                    ws.Column("C").Width = 20;
                    ws.Column("D").Width = 20;
                    ws.Column("E").Width = 20;
                    ws.Column("F").Width = 20;
                    ws.Column("G").Width = 40;
                    ws.Column("H").Width = 20;
                    ws.Column("I").Width = 20;
                    ws.Column("J").Width = 20;
                    ws.Column("K").Width = 20;
                    ws.Column("L").Width = 40;


                    //กำหนดขนาด row
                    ws.Row(1).Height = 30   ;
                    ws.Range("A1:L1").Style.Fill.BackgroundColor = XLColor.LightYellow;
                 
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                       
                        ws.Cell(2 + i, 1).Value = i + 1;
                        ws.Cell(2 + i, 3).Value = dt.Rows[i]["check_name"].ToString();
                        ws.Cell(2 + i, 4).Value = dt.Rows[i]["asset_no"].ToString();
                        ws.Cell(2 + i, 5).Value = dt.Rows[i]["sub_asset_no"].ToString();
                        ws.Cell(2 + i, 6).Value = dt.Rows[i]["control_no"].ToString();
                        ws.Cell(2 + i, 7).Value = dt.Rows[i]["name"].ToString();
                        ws.Cell(2 + i, 8).Value = dt.Rows[i]["model_name"].ToString();
                        ws.Cell(2 + i, 9).Value = dt.Rows[i]["location_name"].ToString();
                        ws.Cell(2 + i, 10).Value = dt.Rows[i]["cell_name"].ToString();
                        ws.Cell(2 + i, 11).Value = dt.Rows[i]["station_name"].ToString();
                        ws.Cell(2 + i, 12).Value = dt.Rows[i]["remark"].ToString();

                        string tool_id = dt.Rows[i]["tool_id"].ToString();
                        string tool_sub = dt.Rows[i]["tool_sub_no"].ToString();
                        string sys_status = dt.Rows[i]["sys_status"].ToString().Trim();

                        Get_Folder_Structure_Picture Folder_Structure_Picture = new Get_Folder_Structure_Picture(tool_id, tool_sub, sys_status);
                        string path_picture_toolMain = Folder_Structure_Picture.get_folder_root_view_toolMain();
                     
                        int MAX_WIDTH = 220;
                        int MAX_HEIGHT = 220;

                        if (path_picture_toolMain != "")    
                        { 
                            string imagePath = Server.MapPath(path_picture_toolMain);
                            var image_pic1 = ws.AddPicture(imagePath);
                            Double xRatio = (double)image_pic1.Width / MAX_WIDTH;
                            Double yRatio = (double)image_pic1.Height / MAX_HEIGHT;
                            Double ratio = Math.Max(xRatio, yRatio);
                            int nnx = (int)Math.Floor(image_pic1.Width / ratio);
                            int nny = (int)Math.Floor(image_pic1.Height / ratio);

                            double cellWidth = 270;
                            double cellHeight = 226;
                            double leftMargin = ((cellWidth - nnx) / 2);
                            double topMargin = ((cellHeight - nny) / 2);

                            if (leftMargin < 0)
                            {
                                leftMargin = leftMargin * -1;
                            }
                            if (topMargin < 0)
                            {
                                topMargin = topMargin * -1;
                            }
                            //  image_pic1.MoveTo(ws.Cell("B1"));
                            image_pic1.MoveTo(ws.Cell(2 + i, 2), Convert.ToInt32(leftMargin), Convert.ToInt32(topMargin)).WithSize(nnx, nny);
                            ws.Row(2 + i).Height = nny;
                           
                        }

                    }
                 
                    ws.Cells().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cells().Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    Response.Clear();
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;filename=tanaoroshi_report.xlsx");

                    using (MemoryStream MyMemoryStream = new MemoryStream())
                    {
                        workbook.SaveAs(MyMemoryStream);
                        MyMemoryStream.WriteTo(Response.OutputStream);
                    }

                    // This will prevent any other content from being written to the response.
                    Response.Flush();
                    Response.End();
                }
    
            }
            catch (Exception ex)
            {
                alert_Messend(ex.Message.ToString());
            }
        }
        public DataTable GridViewToDataTable(GridView gv)
        {
            // Create the DataTable with columns from the GridView headers
            DataTable dt = new DataTable();
            if (gv.HeaderRow != null)
            {
                for (int i = 0; i < gv.HeaderRow.Cells.Count; i++)
                {
                    dt.Columns.Add(gv.HeaderRow.Cells[i].Text);
                }
            }

            // Loop through each row of the GridView
            foreach (GridViewRow row in gv.Rows)
            {
                DataRow dr = dt.NewRow();
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    dr[i] = row.Cells[i].Text;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
        protected void alert_Messend(string Message)
        {
            sb.Append(@"<script type='text/javascript'>");
            sb.Append("alert('" + Message + "') ;");
            sb.Append(@"</script>");
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "closemodal", sb.ToString(), false);
            return;
        }
        protected void alert_completed_reload()
        {

            sb.Append(@"<script type='text/javascript'>");
            sb.Append(" alert('Completed.') ;window.location.href = window.location.href;");  //;
            sb.Append(@"</script>");
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "closemodal", sb.ToString(), false);
            return;
        }
    }
}