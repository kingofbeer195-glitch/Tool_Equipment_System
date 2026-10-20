using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Tool_Equipment_Control_System
{
    public partial class tool_equipment_tana_check : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        SqlConnection conn_jig = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString);
        datacon dc = new datacon();

        string dept_code;
        string id_tana;
        string rsp_id;
        string model_id;
        string location_id;
        string cell_id;
        string tana_date;
        string emp_id;
     
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["data_tana"] == null || Session["data_tana"].ToString() == "")
                Response.Redirect("tool_equipment_tanaoroshi.aspx");

           
            emp_id = Session["emp_id"].ToString();
            get_data_tana();

            if (!IsPostBack)
            {
               
                Get_gv_tana_check();
            }

        // RegisterPostbackControl();
        }
        protected void RegisterPostbackControl()
        {
            ScriptManager.GetCurrent(this).RegisterAsyncPostBackControl(gv_tana_check);
            foreach (GridViewRow row in gv_tana_check.Rows)
            {
                DropDownList ddl_location_new = (DropDownList)row.FindControl("ddl_location_new");
                ScriptManager.GetCurrent(this).RegisterPostBackControl(ddl_location_new);
               // ScriptManager.GetCurrent(this).RegisterAsyncPostBackControl(ddl_location_new);
            }
        }

        protected void get_data_tana()
        {
            Dictionary<string, string> myData = (Dictionary<string, string>)Session["data_tana"];

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

            //Get ddl_model
            sql = " select* from master_location3	where location1_id =" + location_id;
            dc.QryDT(conn_jig, sql);
            if (dc.dt.Rows.Count > 0)
            {
                ddl_station.DataSource = dc.dt;
                ddl_station.DataValueField = "location3_id";
                ddl_station.DataTextField = "location3_name";
                ddl_station.DataBind();

            }
        }
        protected void Get_gv_tana_check()
        {
            string where = "";
            if (model_id != "" && model_id != "0")
                where += " and model_id = '" + model_id + "' ";
            if (location_id != "" && location_id != "0")
                where += " and location_id = '" + location_id + "' ";
            if (cell_id != "" && cell_id != "0")
                where += " and cell_id = '" + cell_id + "' ";

            string sql = "select* from View_Equipment_tana_history where tana_date = '" + tana_date + "' " +
            "and dept_code = '" + dept_code + "' and pic_rsp = '" + rsp_id + "'  " + where;// and model_id = '"+model_id+"'   and cell_id = '"+cell_id+"' ";
                       
            dc.QryDT(conn,sql);
         
           // lb_test.Text = sql;

            gv_tana_check.DataSource = dc.dt;
            gv_tana_check.DataBind();
        }
        protected void gv_tana_check_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                HiddenField hd_img_path = (HiddenField)e.Row.FindControl("hd_img_path");
                HiddenField hd_tool_id = (HiddenField)e.Row.FindControl("hd_tool_id");
                HiddenField hd_sys_status = (HiddenField)e.Row.FindControl("hd_sys_status");
                HiddenField hd_tool_sub = (HiddenField)e.Row.FindControl("hd_tool_sub_no");
                HiddenField hd_check_id = (HiddenField)e.Row.FindControl("hd_check_id");

                HiddenField hd_rsp_new = (HiddenField)e.Row.FindControl("hd_rsp_new");
                HiddenField hd_model_new = (HiddenField)e.Row.FindControl("hd_model_new");
                HiddenField hd_location_new = (HiddenField)e.Row.FindControl("hd_location_new");
                HiddenField hd_cell_new = (HiddenField)e.Row.FindControl("hd_cell_new");
                HiddenField hd_station_new = (HiddenField)e.Row.FindControl("hd_station_new");

                DropDownList ddl_rsp_new = (DropDownList)e.Row.FindControl("ddl_rsp_new");
                DropDownList ddl_model_new = (DropDownList)e.Row.FindControl("ddl_model_new");
                DropDownList ddl_location_new = (DropDownList)e.Row.FindControl("ddl_location_new");
                DropDownList ddl_cell_new = (DropDownList)e.Row.FindControl("ddl_cell_new");
                DropDownList ddl_station_new = (DropDownList)e.Row.FindControl("ddl_station_new");

                ImageButton img_tool = (ImageButton)e.Row.FindControl("img_tool");
                // Label lb_check = (Label)e.Row.FindControl("lb_check");
                System.Web.UI.HtmlControls.HtmlInputRadioButton rb_check = (System.Web.UI.HtmlControls.HtmlInputRadioButton)e.Row.FindControl("rb_check");
                System.Web.UI.HtmlControls.HtmlInputRadioButton rb_location = (System.Web.UI.HtmlControls.HtmlInputRadioButton)e.Row.FindControl("rb_location");
                System.Web.UI.HtmlControls.HtmlInputRadioButton rb_notFound = (System.Web.UI.HtmlControls.HtmlInputRadioButton)e.Row.FindControl("rb_notFound");
                //System.Web.UI.HtmlControls.HtmlInputGenericControl lb_check = (System.Web.UI.HtmlControls.HtmlInputGenericControl)e.Row.FindControl("lb_check");
                HtmlGenericControl lb_check = (HtmlGenericControl)e.Row.FindControl("lb_check");
                HtmlGenericControl lb_loca = (HtmlGenericControl)e.Row.FindControl("lb_loca");
                HtmlGenericControl lb_not_found = (HtmlGenericControl)e.Row.FindControl("lb_not_found");
                
                string[] tool_id = hd_tool_id.Value.Split(',');
                string[] tool_sub = hd_tool_sub.Value.Split(',');
                string[] sys_status = hd_sys_status.Value.Split(',');
                string check_id = hd_check_id.Value;

                string scriptKey = "SelectButtonScript_" + e.Row.RowIndex;
                string script = "";

                if (check_id == "1")
                {
                    script = "selectButtonFromCodeBehind('" + lb_check.ClientID + "','');";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), scriptKey, script, true);
                }
                else if (check_id == "2")
                {
                    script = "selectButtonFromCodeBehind('" + lb_loca.ClientID + "','Location');";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), scriptKey, script, true);

                }
                else if (check_id == "3"){
                    script = "selectButtonFromCodeBehind('" + lb_not_found.ClientID + "','Not_Found');";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), scriptKey, script, true);
                }


                if(hd_rsp_new.Value != "")
                    ddl_rsp_new.SelectedValue = hd_rsp_new.Value;

                if(hd_model_new.Value != "")
                    ddl_model_new.SelectedValue = hd_model_new.Value;

                if(hd_location_new.Value != "")
                {
                    ddl_location_new.SelectedValue = hd_location_new.Value;
                 //   script = "chang_cell_new('"+ ddl_location_new.ID + "');";
                  //  ScriptManager.RegisterStartupScript(this, this.GetType(), "ContentPlaceHolder1_gv_tana_check_ddl_cell_new_"+ e.Row.RowIndex, script, true);
                }
                  

                if(hd_cell_new.Value != "")
                    ddl_cell_new.SelectedValue= hd_cell_new.Value;

                if(hd_station_new.Value != "")
                    ddl_station_new.SelectedValue = hd_station_new.Value;

                //Get Picture Tool 
                Get_Folder_Structure_Picture Folder_Structure_Picture = new Get_Folder_Structure_Picture(tool_id[0], tool_sub[0], sys_status[0]);
                string get_path_toolSub = Folder_Structure_Picture.get_folder_root();
                string path_picture_toolMain = Folder_Structure_Picture.get_folder_root_view_toolMain();
                //    lb_testPic.Text = path_picture_toolMain;
                //lb_check.Text = check_id;
                string[] files_main = Directory.GetFiles(get_path_toolSub, "*.jpg", SearchOption.TopDirectoryOnly);

                if (files_main.Length > 0)
                {
                    hd_img_path.Value = path_picture_toolMain;
                    img_tool.ImageUrl = path_picture_toolMain;
                }
                else
                {
                    hd_img_path.Value = "images/FilesType/jpeg.png";
                    img_tool.ImageUrl = "images/FilesType/jpeg.png";
                }
               
                
            }
        }
        protected void btn_pic_Click(object sender, EventArgs e)
        {

            sb.Append(@"<script type='text/javascript'>");
            sb.Append("$('#modal_UploadPictureMain').modal('show')");
            sb.Append(@"</script>");
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "closemodal", sb.ToString(), false);

        }
        protected void btn_save_Click(object sender ,EventArgs e)
        {
            try
            {
                string update = "";
                string result_check = "";
               // int NG = 0; 
                int not = 0;
                foreach (GridViewRow row in gv_tana_check.Rows)
                {
                    // Find the HiddenField controls in the current row
                    //HiddenField selectedValueField = (HiddenField)row.FindControl("SelectedValue");
                    HiddenField hd_id_his = (HiddenField)row.FindControl("hd_id_his");
                    DropDownList ddl_rsp_new = (DropDownList)row.FindControl("ddl_rsp_new");
                    DropDownList ddl_model_new = (DropDownList)row.FindControl("ddl_model_new");
                    DropDownList ddl_location_new = (DropDownList)row.FindControl("ddl_location_new");
                    DropDownList ddl_cell_new = (DropDownList)row.FindControl("ddl_cell_new");
                    DropDownList ddl_station_new = (DropDownList)row.FindControl("ddl_station_new");
                    HiddenField hd_ddl_cell_new = (HiddenField)row.FindControl("hd_ddl_cell_new");
                    HiddenField hd_ddl_station_new = (HiddenField)row.FindControl("hd_ddl_station_new");
                    TextBox txt_remark = (TextBox)row.FindControl("txt_remark");

                    System.Web.UI.HtmlControls.HtmlInputRadioButton rb_check = (System.Web.UI.HtmlControls.HtmlInputRadioButton)row.FindControl("rb_check");
                    System.Web.UI.HtmlControls.HtmlInputRadioButton rb_location = (System.Web.UI.HtmlControls.HtmlInputRadioButton)row.FindControl("rb_location");
                    System.Web.UI.HtmlControls.HtmlInputRadioButton rb_notFound = (System.Web.UI.HtmlControls.HtmlInputRadioButton)row.FindControl("rb_notFound");
                
                    string id_his = hd_id_his.Value;
                    string check_id = "";
                   

                    if (rb_notFound.Checked)  //NG
                    {
                        check_id = "3";
                        result_check = "3";
                       // NG ++;
                    }
                    else if (rb_check.Checked)   //OK
                    {
                        check_id = "1";
                    }
                    else if (rb_location.Checked) //location
                    {
                        check_id = "2";
                    }

                    if(check_id != "")
                    {
                        update += " UPDATE [dbo].[0] SET [check_id] = '" + check_id + "' ";

                        string cell_id = (ddl_cell_new.SelectedValue == "" ? hd_ddl_cell_new.Value : ddl_cell_new.SelectedValue);
                        string station_id = (ddl_station_new.SelectedValue == "" ? hd_ddl_station_new.Value : ddl_station_new.SelectedValue);

                        if (ddl_rsp_new.SelectedValue != "")
                            update += " ,[pic_rsp_new] = N'" + ddl_rsp_new.SelectedValue.ToString()+"' ";

                        if (ddl_model_new.SelectedValue != "")
                            update += " ,[model_new] = " + ddl_model_new.SelectedValue.ToString();

                        if (ddl_location_new.SelectedValue != "")
                            update += " ,[location_new] = " + ddl_location_new.SelectedValue.ToString();

                        if (cell_id != "")
                            update += " ,[cell_new] = " + cell_id.ToString(); 
                        
                        if (station_id != "")
                            update += " ,[station_new]= " + station_id.ToString();

                        if (txt_remark.Text != "")
                            update += " ,[remark] = '" + txt_remark.Text.Trim() + "'";

                        update += " where id =" + id_his;

                        //  update += "<br/>";

                    }
                    else
                    {
                        not++;
                    }

                } 
                

                if(update != "")
                {
                    update += " UPDATE [dbo].[Equipment_tana_list] SET [status_check] = 2" ;
                    update += " ,[check_by]  = '"+emp_id+"'";
                    update += " ,[check_date] = GETDATE() ";
                    update += " ,[tana_status] = 2 ";
                    update += " ,[check_result] = " +(result_check != "3" ? (not > 0 ? "2" : "1") : result_check ) ;
                    update += " Where id = " + id_tana;
                }


               

                if(update != "")
                {
                   // lb_test.Text = update; return;
                    dc.RunCommand(conn, update);
                    alert_completed_reload();
                }

            }
            catch (Exception ex)
            {
                alert_Messend(ex.Message.ToString());
            }
           

        }
        protected void ddl_location_new_SelectedIndexChanged(object sender, EventArgs e)
        {
         
            GridViewRow row = (sender as DropDownList).NamingContainer as GridViewRow;
            DropDownList ddl_location_new = (DropDownList)gv_tana_check.Rows[row.RowIndex].FindControl("ddl_location_new");
            DropDownList ddl_cell_new = (DropDownList)gv_tana_check.Rows[row.RowIndex].FindControl("ddl_cell_new");
            DropDownList ddl_station_new = (DropDownList)gv_tana_check.Rows[row.RowIndex].FindControl("ddl_station_new");
          
            string location1 = ddl_location_new.SelectedValue.ToString();
            //alert_Messend(location_id+"//"+location1);
            if (location1 != "")
            {
                string str = " select* from master_location2 where location1_id = " + location1 + " and is_use=1";
               // lb_test.Text = str; return;
                dc.QryDT(conn_jig, str);

                if (dc.dt.Rows.Count > 0)
                {
                    ddl_cell_new.DataSource = dc.dt;
                    ddl_cell_new.DataValueField = "location2_id";
                    ddl_cell_new.DataTextField = "location2_name";
                    ddl_cell_new.DataBind();
                }
                // lb_test.Text = str ;
                str = " select* from master_location3	where location1_id = " + location1;
                dc.QryDT(conn_jig, str);
                if (dc.dt.Rows.Count > 0)
                {
                    ddl_station_new.DataSource = dc.dt;
                    ddl_station_new.DataValueField = "location3_id";
                    ddl_station_new.DataTextField = "location3_name";
                    ddl_station_new.DataBind();
                }
                get_data_tana();
               // Get_gv_tana_check();

            }
           
        }

        protected void alert_completed_reload()
        {
            sb.Append(@"<script type='text/javascript'>");
            sb.Append(" alert('Completed.') ;window.location.href = window.location.href;");  //;
            sb.Append(@"</script>");
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "closemodal", sb.ToString(), false);
            return;
        }
        protected void alert_Messend(string Message)
        {
            sb.Append(@"<script type='text/javascript'>");
            sb.Append("alert('Error Message : " + Message + "') ;");
            sb.Append(@"</script>");
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "closemodal", sb.ToString(), false);
            return;
        }
        
       

        [WebMethod]
        public static List<CellItem> GetCellsByLocation(string locationId)
        {
            // สร้าง connection string จาก web.config
            string connString = ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString;

            List<CellItem> cells = new List<CellItem>();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                // แก้ไขการใช้ SQL Parameter ให้ถูกต้อง เพื่อป้องกัน SQL Injection
                string sqlQuery = "SELECT location2_id, location2_name FROM master_location2 WHERE location1_id = @id AND is_use = 1";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    // เพิ่ม parameter เพื่อป้องกัน SQL Injection
                    cmd.Parameters.AddWithValue("@id", locationId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cells.Add(new CellItem
                            {
                                location2_id = reader["location2_id"].ToString(),
                                location2_name = reader["location2_name"].ToString()
                            });
                        }
                    }
                }
            }
            return cells;
        }

        [WebMethod]
        public static List<stattionItem> GetStation(string location)
        {
           
            string connString = ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString;

            List<stattionItem> station = new List<stattionItem>();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sqlstation = " select location3_id,location3_name from master_location3 where location1_id =  @id_statiob ";
                using (SqlCommand cmd = new SqlCommand(sqlstation, conn))
                {
                    // เพิ่ม parameter เพื่อป้องกัน SQL Injection
                    cmd.Parameters.AddWithValue("@id_statiob", location);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            station.Add(new stattionItem
                            {
                                location3_id = reader["location3_id"].ToString(),
                                location3_name = reader["location3_name"].ToString()
                            });
                        }
                    }
                }
            }
            return station;
        }

    }
    public class CellItem
    {
        public string location2_id { get; set; }
        public string location2_name { get; set; }
    } 
    public class stattionItem
    {
        public string location3_id { get; set; }
        public string location3_name { get; set; }
    }
}