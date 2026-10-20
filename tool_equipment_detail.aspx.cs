//using Spire.Xls;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tool_Equipment_Control_System
{
    public partial class tool_equipment_detail : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        SqlConnection conn_jig = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        datacon dc = new datacon();

        string tool_id;
        string tool_sub;
        string sys_status;

        DataTable tb_data = new DataTable();

        protected List<string> ImagePaths { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        {
            tool_id = Request.QueryString["tool_id"];
            tool_sub = Request.QueryString["tool_sub"];
            sys_status = Request.QueryString["sys_status"];

            if (Session["emp_id"] == null || Session["emp_id"].ToString() == "")
                Response.Redirect("Default.aspx");

            SQL_data_tool();

            if (!IsPostBack)
            {
                Check_Admin();
                Get_data_tool();
            }

            Get_image();

        }
        protected void Check_Admin()
        {
            if (Session["admin"] == null) return;

            string Admin = Session["admin"].ToString();

            if (Admin == "99")
            {
                btn_edit_data.Visible = true;
                btn_picture.Visible = true;
            }
            else
            {
                btn_edit_data.Visible = false;
                btn_picture.Visible = false;
            }

        }
        protected void SQL_data_tool()
        {
            string str = "";
            str += " SELECT* from Tool_Equipment_List_View WHERE tool_id = " + tool_id + " and sub_no = " + tool_sub + " and sys_status=" + sys_status;
            dc.QryDT(conn, str);
            tb_data = dc.dt;
            // lb_test.Text = str;
        }
        protected void Get_data_tool()
        {
            if (tb_data.Rows.Count > 0)
            {
                //---------------------------------------------
                // ----- Detail & Status
                //---------------------------------------------
                #region
                txt_id.Text = tb_data.Rows[0]["ID"].ToString();
                txt_toolno_model.Text = tb_data.Rows[0]["tool_no"].ToString();
                txt_no.Text = "No." + tb_data.Rows[0]["sub_no"].ToString();
                txt_category.Text = tb_data.Rows[0]["category"].ToString();

                txtToolname.Text = tb_data.Rows[0]["toolName"].ToString();

                ddl_status.SelectedValue = tb_data.Rows[0]["status_id"].ToString();
                txt_fixassetNo.Text = tb_data.Rows[0]["FixAcces"].ToString();




                if (tb_data.Rows[0]["operation_date"].ToString() != "")
                {
                    string str_date = " select dbo.GET_date_use('" + tb_data.Rows[0]["operation_date"].ToString() + "') ";
                    dc.QryDT(conn, str_date);
                    DataTable date = dc.dt;
                    if (date.Rows.Count > 0)
                    {
                        txt_useTime.Text = date.Rows[0][0].ToString();
                    }
                }
                txt_duedate.Text = tb_data.Rows[0]["DueDate1"].ToString();


                var periodId = tb_data.Rows[0]["period_Id"].ToString();
                ddl_calPeriod.SelectedValue = string.IsNullOrWhiteSpace(periodId) ? "0" : periodId;

                //ddl_calPeriod.SelectedValue = tb_data.Rows[0]["period_Id"].ToString();

                #endregion

                //---------------------------------------------
                // ----- Location
                //---------------------------------------------
                #region
                // Model
                ddl_model.SelectedValue = tb_data.Rows[0]["model_id"].ToString();

                // Query list Cell , Station
                string location_id = tb_data.Rows[0]["location1_id"].ToString();
                get_ddl_cell_station(location_id);

                // Show List Location , Cell , Station
                // bind ddl_location ก่อน เพื่อเช็คว่ามีค่าในรายการไหม (SqlDataSource โหลดแค่ location 97)
                ddl_location.DataBind();
                // ค่าที่ไม่อยู่ในรายการ (เช่น Transfer = location 98, Cell ที่ is_use=0) -> เพิ่มเข้า list แทนการ error
                SafeSelect(ddl_location, location_id,
                    "select location1_name from master_location1 where location1_id = @id");
                SafeSelect(ddl_cell, tb_data.Rows[0]["location2_id"].ToString(),
                    "select location2_name from master_location2 where location2_id = @id");
                SafeSelect(ddl_station, tb_data.Rows[0]["station_id"].ToString(),
                    "select location3_name from master_location3 where location3_id = @id");

                //Query List Dept Code
                string div = tb_data.Rows[0]["division"].ToString();
                Get_ddl_dept(div);

                // Show dept code
                string dept_code = tb_data.Rows[0]["dept_code"].ToString();
                ddl_dept.SelectedValue = dept_code;

                //Query List Responsible
                get_ddl_responsible(dept_code);

                txt_deptname.Text = tb_data.Rows[0]["dept"].ToString();
                ddl_responsible.SelectedValue = tb_data.Rows[0]["rsp_empID"].ToString();

                #endregion

                //---------------------------------------------
                // ----- Other Detail
                //---------------------------------------------
                #region


                txt_assetName.Text = tb_data.Rows[0]["asset_name"].ToString();

                var checkTypeObj = tb_data.Rows[0]["check_type"];
                // Value = "1" > Tangible(มีรูปร่าง)
                // Value = "2" > Intangible(ไม่มีรูปร่าง)
                if (checkTypeObj == DBNull.Value || string.IsNullOrEmpty(checkTypeObj.ToString()))
                {
                    ddl_checktype.SelectedValue = "0";
                }
                else
                {
                    ddl_checktype.SelectedValue = checkTypeObj.ToString();
                }

                ddl_boi.SelectedValue = tb_data.Rows[0]["boi_id"].ToString();
                txt_subfix.Text = tb_data.Rows[0]["sub_fix"].ToString();
                ddl_paper.SelectedValue = tb_data.Rows[0]["paper"].ToString();

                //Class Code , Sub-Class Name
                //Get_ddl_classCD();
                //ddl_classCD.SelectedValue = tb_data.Rows[0]["class_cd"].ToString();
                //txt_classNM.Text = tb_data.Rows[0]["classCD_name"].ToString();

                txt_prpo.Text = tb_data.Rows[0]["prpo"].ToString();
                txt_price.Text = parsedPrice(tb_data.Rows[0]["price"].ToString());
                txt_operationDate.Text = tb_data.Rows[0]["operationDate"].ToString();

                //Cost Center ID , Cost Center Name
                Get_ddl_costcenterID(div);

                var checkTypeObj_costcenter = tb_data.Rows[0]["costcenterID"];

                if (IsTransferItem())
                {
                    // Transfer (location 98 = Supplier) -> ไม่มี Cost Center
                    ddl_costcenterID.SelectedValue = "0";
                    txt_costcenterName.Text = "-";
                }
                else if (checkTypeObj_costcenter == DBNull.Value || string.IsNullOrEmpty(checkTypeObj_costcenter.ToString()))
                {
                    ddl_costcenterID.SelectedValue = "0";
                    txt_costcenterName.Text = "-";
                }
                else if (checkTypeObj_costcenter.ToString() == "0" || checkTypeObj_costcenter.ToString() == "-")
                {
                    ddl_costcenterID.SelectedValue = "0";
                    txt_costcenterName.Text = "-";
                }
                else
                {
                    ddl_costcenterID.SelectedValue = checkTypeObj_costcenter.ToString();
                    string costcenterID = tb_data.Rows[0]["costcenterID"].ToString();
                    string str_costcenterName = "select* from View_dept_master where dept_code = '" + costcenterID + "'";
                    dc.QryDT(conn, str_costcenterName);
                    txt_costcenterName.Text = dc.dt.Rows[0]["dept_name_full"].ToString();
                }


                //Control No , Serial No , Equipment Model , Manuafaxcture
                txt_controlNo.Text = DisplayString(tb_data.Rows[0]["control_no"]);
                txt_serialNo.Text = DisplayString(tb_data.Rows[0]["serial_no"]);
                txt_equipmentModel.Text = DisplayString(tb_data.Rows[0]["EquipmentModel"]);
                txt_manufacture.Text = DisplayString(tb_data.Rows[0]["manufacturing"]);

                // Remark
                txt_remark.Text = tb_data.Rows[0]["Remark"].ToString();

                #endregion
            }
            Get_image();

        }
        protected void Get_image()
        {

            Get_Folder_Structure_Picture Folder_Structure_Picture = new Get_Folder_Structure_Picture(tool_id, tool_sub, sys_status);
            string path_picture_toolMain = Folder_Structure_Picture.get_folder_root_view_toolMain();

            string get_path_toolSub = Folder_Structure_Picture.get_folder_root();
            string path_picture_toolSub = Folder_Structure_Picture.get_folder_root_view_toolSub();

            string get_path_toolOther = Folder_Structure_Picture.get_folder_root_toolOther();
            string path_picture_toolOther = Folder_Structure_Picture.get_folder_root_view_toolOther();

            ImagePaths = new List<string>();

            string[] files_main = Directory.GetFiles(get_path_toolSub, "*.jpg", SearchOption.TopDirectoryOnly);
            if (files_main.Length > 0)
            {
                ImagePaths.Add(path_picture_toolMain);
            }
            else
            {
                ImagePaths.Add("images/FilesType/jpeg.png");
            }

            string[] file_sub = Directory.GetFiles(get_path_toolSub, "* Sub*.jpg", SearchOption.TopDirectoryOnly);
            if (file_sub.Length > 0)
            {
                for (int i = 0; i < file_sub.Length; i++)
                {
                    FileInfo f = new FileInfo(file_sub[i]);
                    ImagePaths.Add(path_picture_toolSub + "/" + f.Name.ToString());
                }
            }

            string[] files_other = Directory.GetFiles(get_path_toolOther, "*.jpg", SearchOption.TopDirectoryOnly);

            if (files_other.Length > 0)
            {
                for (int i = 0; i < files_other.Length; i++)
                {
                    FileInfo f = new FileInfo(files_other[i]);
                    ImagePaths.Add(path_picture_toolOther + f.Name.ToString());
                }
            }
        }

        protected void Get_ddl_dept(string div)
        {
            string str = " select* from View_dept_master order by dept_name2 "; // + (div == "" ? "" : " where division = "+ div);
            dc.QryDT(conn, str);
            //ViewState["DEPT_MASTER"] = dc.dt;

            if (dc.dt.Rows.Count > 0)
            {
                ddl_dept.DataSource = dc.dt;
                ddl_dept.DataTextField = "dept_name2";
                ddl_dept.DataValueField = "dept_code";
                ddl_dept.DataBind();
            }
        }

        private const string LOCATION_SUPPLIER = "98"; // Transfer -> Supplier

        // รายการที่ Transfer แล้ว (location1_id = 98)
        private bool IsTransferItem()
        {
            return tb_data != null && tb_data.Rows.Count > 0
                && tb_data.Columns.Contains("location1_id")
                && tb_data.Rows[0]["location1_id"].ToString().Trim() == LOCATION_SUPPLIER;
        }

        protected void Get_ddl_costcenterID(string div)
        {
            string str = " select* from View_dept_master order by dept_name2 "; // + (div == "" ? "" : " where division = "+ div);
            dc.QryDT(conn, str);
            //ViewState["DEPT_MASTER"] = dc.dt;

            foreach (DataRow row in dc.dt.Rows)
            {
                if (row["dept_code"] != DBNull.Value)
                    row["dept_code"] = row["dept_code"].ToString().Trim();
            }

            if (dc.dt.Rows.Count > 0)
            {
                ddl_costcenterID.DataSource = dc.dt;
                ddl_costcenterID.DataTextField = "dept_code";
                ddl_costcenterID.DataValueField = "dept_code";
                ddl_costcenterID.DataBind();
            }
        }
        // เลือกค่าใน DropDownList แบบปลอดภัย: ถ้าไม่มีในรายการ ให้ดึงชื่อจาก DB แล้วเพิ่มเข้าไป
        // (กัน ArgumentOutOfRangeException ของรายการที่ Transfer/Scrap หรือ master ถูกปิดใช้งาน)
        private void SafeSelect(DropDownList ddl, string value, string nameSql)
        {
            if (string.IsNullOrEmpty(value)) return;

            if (ddl.Items.FindByValue(value) == null)
            {
                string text = value;
                bool opened = false;
                try
                {
                    using (var cmd = new SqlCommand(nameSql, conn_jig))
                    {
                        cmd.Parameters.AddWithValue("@id", value);
                        if (conn_jig.State != ConnectionState.Open) { conn_jig.Open(); opened = true; }
                        object o = cmd.ExecuteScalar();
                        if (o != null && o != DBNull.Value) text = o.ToString();
                    }
                }
                catch { /* ใช้ value เป็นชื่อแทน */ }
                finally { if (opened) conn_jig.Close(); }

                ddl.Items.Add(new ListItem(text, value));
            }

            ddl.ClearSelection();
            ddl.SelectedValue = value;
        }

        protected void get_ddl_cell_station(string location_id)
        {
            if (location_id != "")
            {
                ddl_cell.Items.Clear();
                string str = " select* from master_location2 where location1_id = " + location_id + " and is_use=1 order by location2_name ";
                dc.QryDT(conn_jig, str);
                if (dc.dt.Rows.Count > 0)
                {
                    ddl_cell.DataSource = dc.dt;
                    ddl_cell.DataValueField = "location2_id";
                    ddl_cell.DataTextField = "location2_name";
                    ddl_cell.DataBind();

                }

                ddl_station.Items.Clear();
                str = " select* from master_location3 where location1_id = " + location_id + " order by location3_name";
                dc.QryDT(conn_jig, str);
                if (dc.dt.Rows.Count > 0)
                {
                    ddl_station.DataSource = dc.dt;
                    ddl_station.DataValueField = "location3_id";
                    ddl_station.DataTextField = "location3_name";
                    ddl_station.DataBind();
                }
                // lb_test.Text = str;
                //  }
            }
        }
        protected void get_ddl_responsible(string dept_code)
        {
            //string div_id = hd_divID.Value.ToString();
            string str = "select * , CONVERT(VARCHAR(10), emp_id) + ' ' + pic_dept + ' (' + CONVERT(VARCHAR(10), dept_code_real) +')' as name" +
                         " from Responsible_Dept where dept_code = '" + dept_code + "'";
            dc.QryDT(conn, str);
            if (dc.dt.Rows.Count > 0)
            {
                ddl_responsible.DataSource = dc.dt;
                ddl_responsible.DataTextField = "name";
                ddl_responsible.DataValueField = "emp_id";
                ddl_responsible.DataBind();
                hd_divID.Value = dc.dt.Rows[0]["div_id"].ToString();
                hd_deptID.Value = dc.dt.Rows[0]["dept_id"].ToString();
            }

        }

        //protected void Get_ddl_classCD()
        //{
        //    string str = " SELECT * FROM classCD_Asset_master order by classCD_id ";
        //    dc.QryDT(conn, str);

        //    foreach (DataRow row in dc.dt.Rows)
        //    {
        //        if (row["classCD_id"] != DBNull.Value)
        //            row["classCD_id"] = row["classCD_id"].ToString().Trim();
        //    }

        //    if (dc.dt.Rows.Count > 0)
        //    {
        //        ddl_classCD.DataSource = dc.dt;
        //        ddl_classCD.DataTextField = "classCD_id";
        //        ddl_classCD.DataValueField = "classCD_id";
        //        ddl_classCD.DataBind();
        //    }
        //}

        protected void control_tag()
        {
            //Button
            btn_edit_data.Visible = false;
            btn_SaveEdit_data.Visible = true;
            btn_cancelEdit.Visible = true;

            //Detail & Status
            ddl_status.Enabled = true;
            txt_fixassetNo.Enabled = true; txt_fixassetNo.CssClass = (txt_fixassetNo.CssClass += " open_edit").Trim();    //BFM No.
            txt_controlNo.Enabled = true; txt_controlNo.CssClass = (txt_controlNo.CssClass += " open_edit").Trim(); //Control No

            txt_operationDate.Enabled = true; txt_operationDate.CssClass = (txt_operationDate.CssClass += " open_edit").Trim(); //Operating Date
            txt_duedate.Enabled = true; txt_duedate.CssClass = (txt_duedate.CssClass += " open_edit").Trim(); //Due Date
            ddl_calPeriod.Enabled = true; //Cal Period

            //Location
            ddl_model.Enabled = true;
            ddl_location.Enabled = true;
            ddl_cell.Enabled = true;
            ddl_station.Enabled = true;
            ddl_dept.Enabled = true;
            ddl_responsible.Enabled = true;

            //Other Detail
            //-- Row 1            
            //txt_assetName.Enabled = true; txt_assetName.CssClass = (txt_assetName.CssClass += " open_edit").Trim();                   //Asset Name
            //txt_serialNo.Enabled = true; txt_serialNo.CssClass = (txt_serialNo.CssClass += " open_edit").Trim();                      //Serial No
            //txt_equipmentModel.Enabled = true; txt_equipmentModel.CssClass = (txt_equipmentModel.CssClass += " open_edit").Trim();    //Equipment Model

            //-- Row 2
            ddl_checktype.Enabled = true; //Check Type
            txt_subfix.Enabled = true; txt_subfix.CssClass = (txt_subfix.CssClass += " open_edit").Trim();  //Componenets
            ddl_paper.Enabled = true; //Check Paper
            ddl_boi.Enabled = true; //Check BOI
            txt_price.Enabled = true; txt_price.CssClass = (txt_price.CssClass += " open_edit").Trim(); //Price
            txt_prpo.Enabled = true; txt_prpo.CssClass = (txt_prpo.CssClass += " open_edit").Trim();    //PR/PO

            //-- Row3
            //ddl_costcenterID.Enabled = true;
            //ddl_classCD.Enabled = true; //Class Code
            //txt_equipmentModel.Enabled = true; txt_equipmentModel.CssClass = (txt_equipmentModel.CssClass += " open_edit").Trim();    //Serial No
            txt_manufacture.Enabled = true; txt_manufacture.CssClass = (txt_manufacture.CssClass += " open_edit").Trim();   //Equipment Model

            //remark
            txt_remark.Enabled = true; txt_remark.CssClass = (txt_remark.CssClass += " open_edit").Trim();
        }
        protected void data_tool_Edit()
        {
            string update = "";

            if (sys_status == "1") // jig_tool_system → toolSub_list
            {
                update += " UPDATE [jig_tool_system].[dbo].[toolSub_list] SET ";

                // Detail & Status
                update += " [status_id] = '" + ddl_status.SelectedValue + "' ";

                // Tool Name ← เพิ่ม
                if (tb_data.Rows[0]["toolName"].ToString() != txtToolname.Text.Trim())
                    update += " ,[tool_name] = '" + txtToolname.Text.Trim().Replace("'", "''") + "' ";

                // Due Date
                if (tb_data.Rows[0]["duedate"].ToString() != txt_duedate.Text)
                {
                    update += txt_duedate.Text == ""
                        ? " ,[duedate] = NULL "
                        : " ,[duedate] = '" + txt_duedate.Text + "' ";
                }

                // Cal Period ← เพิ่ม
                if (tb_data.Rows[0]["period_Id"].ToString() != ddl_calPeriod.SelectedValue)
                    update += " ,[period_id] = '" + ddl_calPeriod.SelectedValue + "' ";

                // Operating Date
                if (tb_data.Rows[0]["operationDate"].ToString() != txt_operationDate.Text)
                {
                    update += txt_operationDate.Text == ""
                        ? " ,[start_use] = NULL "
                        : " ,[start_use] = '" + txt_operationDate.Text + "' ";
                }

                // Location
                if (tb_data.Rows[0]["model_id"].ToString() != ddl_model.SelectedValue)
                    update += " ,[model_id] = '" + ddl_model.SelectedValue + "' ";
                if (tb_data.Rows[0]["location1_id"].ToString() != ddl_location.SelectedValue)
                    update += " ,[location1_id] = '" + ddl_location.SelectedValue + "' ";
                if (tb_data.Rows[0]["location2_id"].ToString() != ddl_cell.SelectedValue)
                    update += " ,[location2_id] = '" + ddl_cell.SelectedValue + "' ";
                if (tb_data.Rows[0]["station_id"].ToString() != ddl_station.SelectedValue)
                    update += " ,[location3_id] = '" + ddl_station.SelectedValue + "' ";

                if (tb_data.Rows[0]["dept_code"].ToString() != ddl_dept.SelectedValue)
                {
                    update += " ,[dept_code] = '" + hd_deptID.Value + "' ";
                    update += " ,[division]  = '" + hd_divID.Value + "' ";
                }
                if (tb_data.Rows[0]["rsp_empID"].ToString() != ddl_responsible.SelectedValue)
                {
                    update += string.IsNullOrEmpty(ddl_responsible.SelectedValue)
                        ? " ,[rsp_empID] = NULL "
                        : " ,[rsp_empID] = '" + ddl_responsible.SelectedValue + "' ";
                }

                // Other Detail
                if (tb_data.Rows[0]["FixAcces"].ToString() != txt_fixassetNo.Text)
                    update += " ,[FixAcces] = '" + txt_fixassetNo.Text.Replace("'", "''") + "' ";
                if (tb_data.Rows[0]["check_type"].ToString() != ddl_checktype.SelectedValue)
                    update += " ,[check_type_fix] = '" + ddl_checktype.SelectedValue + "' ";
                if (tb_data.Rows[0]["boi_id"].ToString() != ddl_boi.SelectedValue)
                    update += " ,[boi] = '" + ddl_boi.SelectedValue + "' ";
                if (tb_data.Rows[0]["sub_fix"].ToString() != txt_subfix.Text.Trim())
                    update += " ,[sub_fix] = '" + txt_subfix.Text.Trim().Replace("'", "''") + "' ";
                if (tb_data.Rows[0]["paper"].ToString() != ddl_paper.SelectedValue)
                    update += " ,[paper] = '" + ddl_paper.SelectedValue + "' ";
                if (tb_data.Rows[0]["prpo"].ToString() != txt_prpo.Text.Trim())
                    update += " ,[po] = '" + txt_prpo.Text.Trim().Replace("'", "''") + "' ";
                if (parsedPrice(tb_data.Rows[0]["price"].ToString()) != parsedPrice(txt_price.Text))
                    update += " ,[cost] = '" + ParsePriceFromText(txt_price.Text) + "' ";
                if (tb_data.Rows[0]["control_no"].ToString() != txt_controlNo.Text.Trim())
                    update += " ,[control_no] = '" + txt_controlNo.Text.Trim().Replace("'", "''") + "' ";
                if (tb_data.Rows[0]["serial_no"].ToString() != txt_serialNo.Text.Trim())
                    update += " ,[serialNo] = '" + txt_serialNo.Text.Trim().Replace("'", "''") + "' ";
                if (tb_data.Rows[0]["manufacturing"].ToString() != txt_manufacture.Text.Trim())
                    update += " ,[manufacturing] = '" + txt_manufacture.Text.Trim().Replace("'", "''") + "' ";

                // Remark
                if (tb_data.Rows[0]["Remark"].ToString() != txt_remark.Text.Trim())
                    update += " ,[Remark] = '" + txt_remark.Text.Trim().Replace("'", "''") + "' ";

                update += " WHERE [tool_id] = " + tool_id + " AND toolSub_id = " + tool_sub;
            }
            else if (sys_status == "2") // tool_eduipment_system → Equipment_sub_id
            {
                update += " UPDATE [tool_eduipment_system].[dbo].[Equipment_sub_id] SET ";

                // Detail & Status
                update += " [status_use] = '" + ddl_status.SelectedValue + "' ";

                // Tool Name ← เพิ่ม
                if (tb_data.Rows[0]["toolName"].ToString() != txtToolname.Text.Trim())
                    update += " ,[tool_name] = '" + txtToolname.Text.Trim().Replace("'", "''") + "' ";

                // Due Date
                if (tb_data.Rows[0]["duedate"].ToString() != txt_duedate.Text)
                {
                    update += txt_duedate.Text == ""
                        ? " ,[duedate] = NULL "
                        : " ,[duedate] = '" + txt_duedate.Text + "' ";
                }

                // Cal Period ← เพิ่ม
                if (tb_data.Rows[0]["period_Id"].ToString() != ddl_calPeriod.SelectedValue)
                    update += " ,[period_id] = '" + ddl_calPeriod.SelectedValue + "' ";

                // Operating Date
                if (tb_data.Rows[0]["operationDate"].ToString() != txt_operationDate.Text)
                {
                    update += txt_operationDate.Text == ""
                        ? " ,[operation_date] = NULL "
                        : " ,[operation_date] = '" + txt_operationDate.Text + "' ";
                }

                // Location
                if (tb_data.Rows[0]["model_id"].ToString() != ddl_model.SelectedValue)
                    update += " ,[model_id] = '" + ddl_model.SelectedValue + "' ";
                if (tb_data.Rows[0]["location1_id"].ToString() != ddl_location.SelectedValue)
                    update += " ,[location_id] = '" + ddl_location.SelectedValue + "' ";
                if (tb_data.Rows[0]["location2_id"].ToString() != ddl_cell.SelectedValue)
                    update += " ,[cell_id] = '" + ddl_cell.SelectedValue + "' ";
                if (tb_data.Rows[0]["station_id"].ToString() != ddl_station.SelectedValue)
                    update += " ,[station_id] = '" + ddl_station.SelectedValue + "' ";

                if (tb_data.Rows[0]["dept_code"].ToString() != ddl_dept.SelectedValue)
                {
                    update += " ,[dept_code] = '" + hd_deptID.Value + "' ";
                    update += " ,[division]  = '" + hd_divID.Value + "' ";
                }
                if (tb_data.Rows[0]["rsp_empID"].ToString() != ddl_responsible.SelectedValue)
                {
                    update += string.IsNullOrEmpty(ddl_responsible.SelectedValue)
                        ? " ,[rsp_empID] = NULL "
                        : " ,[rsp_empID] = '" + ddl_responsible.SelectedValue + "' ";
                }

                // Other Detail
                if (tb_data.Rows[0]["FixAcces"].ToString() != txt_fixassetNo.Text)
                    update += " ,[fix_asset] = '" + txt_fixassetNo.Text.Replace("'", "''") + "' ";
                if (tb_data.Rows[0]["check_type"].ToString() != ddl_checktype.SelectedValue)
                    update += " ,[check_type_fix] = '" + ddl_checktype.SelectedValue + "' ";
                if (tb_data.Rows[0]["boi_id"].ToString() != ddl_boi.SelectedValue)
                    update += " ,[boi] = '" + ddl_boi.SelectedValue + "' ";
                if (tb_data.Rows[0]["sub_fix"].ToString() != txt_subfix.Text.Trim())
                    update += " ,[sub_fix] = '" + txt_subfix.Text.Trim().Replace("'", "''") + "' ";
                if (tb_data.Rows[0]["paper"].ToString() != ddl_paper.SelectedValue)
                    update += " ,[paper] = '" + ddl_paper.SelectedValue + "' ";
                if (tb_data.Rows[0]["prpo"].ToString() != txt_prpo.Text.Trim())
                    update += " ,[po] = '" + txt_prpo.Text.Trim().Replace("'", "''") + "' ";
                if (parsedPrice(tb_data.Rows[0]["price"].ToString()) != parsedPrice(txt_price.Text))
                    update += " ,[price] = '" + ParsePriceFromText(txt_price.Text) + "' ";
                // Transfer: Cost Center แสดง "-" เฉยๆ ไม่เขียนทับค่าเดิมใน DB
                if (!IsTransferItem() && tb_data.Rows[0]["costcenterID"].ToString() != ddl_costcenterID.SelectedValue)
                    update += " ,[costcenterID] = '" + ddl_costcenterID.SelectedValue + "' ";
                if (tb_data.Rows[0]["control_no"].ToString() != txt_controlNo.Text.Trim())
                    update += " ,[control_no] = '" + txt_controlNo.Text.Trim().Replace("'", "''") + "' ";
                if (tb_data.Rows[0]["serial_no"].ToString() != txt_serialNo.Text.Trim())
                    update += " ,[serial_no] = '" + txt_serialNo.Text.Trim().Replace("'", "''") + "' ";
                if (tb_data.Rows[0]["manufacturing"].ToString() != txt_manufacture.Text.Trim())
                    update += " ,[manufacturing] = '" + txt_manufacture.Text.Trim().Replace("'", "''") + "' ";

                // Remark
                if (tb_data.Rows[0]["Remark"].ToString() != txt_remark.Text.Trim())
                    update += " ,[Remark] = '" + txt_remark.Text.Trim().Replace("'", "''") + "' ";

                update += " WHERE [Equipment_id] = " + tool_id + " AND [Equipment_sub_id] = " + tool_sub;
            }

            if (update != "")
            {
                try
                {
                    // lb_test.Text = update; return; // debug
                    dc.RunCommand(conn, update);
                    alert_completed_reload();
                }
                catch (Exception ex)
                {
                    alert_Messend(ex.Message.ToString());
                }
            }
        }
        protected void btn_edit_data_Click(object sender, EventArgs e)
        {
            ddl_status.Enabled = true;
            ddl_calPeriod.Enabled = true;
            ddl_model.Enabled = true;
            ddl_location.Enabled = true;
            ddl_cell.Enabled = true;
            ddl_station.Enabled = true;
            ddl_dept.Enabled = true;

            //Get_ddl_dept("");
            Get_image();
            control_tag();

            RefreshChosenColors();

        }
        protected void btn_SaveEdit_data_Click(object sender, EventArgs e)
        {
            ddl_status.Enabled = false;
            ddl_calPeriod.Enabled = false;
            ddl_model.Enabled = false;
            ddl_location.Enabled = false;
            ddl_cell.Enabled = false;
            ddl_station.Enabled = false;
            ddl_dept.Enabled = false;

            data_tool_Edit();

            RefreshChosenColors();

        }
        protected void btn_picture_Click(object sender, EventArgs e)
        {
            string tool_id = Request.QueryString["tool_id"];
            string tool_sub = Request.QueryString["tool_sub"];
            string sys_status = Request.QueryString["sys_status"];
            string url = "http://cptsvs552.canon-pt.co.th/Tool_Equipment_Control_System/picture_update.aspx?tool_id=" + tool_id + "&tool_sub=" + tool_sub + "&sys_status=" + sys_status;

            Response.Redirect(url);

        }
        protected void btn_borrow_Click(object sender, EventArgs e)
        {
            string tool_id = Request.QueryString["tool_id"];
            string tool_sub = Request.QueryString["tool_sub"];
            string sys_status = Request.QueryString["sys_status"];
            string url = "http://cptsvs552.canon-pt.co.th/Tool_Equipment_Control_System/Borrow_Tool.aspx?tool_id=" + tool_id + "&tool_sub=" + tool_sub + "&sys_status=" + sys_status;

            Response.Redirect(url);

        }
        protected void btn_withdraw_Click(object sender, EventArgs e)
        {
            string tool_id = Request.QueryString["tool_id"];
            string tool_sub = Request.QueryString["tool_sub"];
            string sys_status = Request.QueryString["sys_status"];
            string url = "http://cptsvs552.canon-pt.co.th/Tool_Equipment_Control_System/withdraw.aspx?tool_id=" + tool_id + "&tool_sub=" + tool_sub + "&sys_status=" + sys_status;

            Response.Redirect(url);

        }
        protected void btn_storage_Click(object sender, EventArgs e)
        {
            string tool_id = Request.QueryString["tool_id"];
            string tool_sub = Request.QueryString["tool_sub"];
            string sys_status = Request.QueryString["sys_status"];
            string url = "http://cptsvs552.canon-pt.co.th/Tool_Equipment_Control_System/storage.aspx?tool_id=" + tool_id + "&tool_sub=" + tool_sub + "&sys_status=" + sys_status;

            Response.Redirect(url);

        }

        protected void ddl_dept_SelectedIndexChanged(object sender, EventArgs e)
        {
            string dept_code = ddl_dept.SelectedValue.ToString();
            string str = " select* from View_dept_master WHERE dept_code = " + dept_code;
            dc.QryDT(conn, str);

            string dept_name = dc.dt.Rows[0]["dept_name_full"].ToString();
            txt_deptname.Text = dept_name;

            //Div Code (Hide)
            string div_id = dc.dt.Rows[0]["division"].ToString();
            string dept_id = dc.dt.Rows[0]["dept_id"].ToString();
            hd_divID.Value = div_id;
            hd_deptID.Value = dept_id;

            ddl_responsible.Items.Clear();
            get_ddl_responsible(dept_code);

        }
        protected void ddl_costcenterID_SelectedIndexChanged(object sender, EventArgs e)
        {
            string dept_code = ddl_costcenterID.SelectedValue.ToString();

            if (dept_code == "-" || dept_code == "0")
            {
                txt_costcenterName.Text = "-";
            }
            else
            {
                string str = " select* from View_dept_master WHERE dept_code = " + dept_code;
                dc.QryDT(conn, str);
                string dept_name = dc.dt.Rows[0]["dept_name_full"].ToString();
                txt_costcenterName.Text = dept_name;
            }
            //alert_Messend(dept_code);
        }
        protected void ddl_location_SelectedIndexChanged(object sender, EventArgs e)
        {
            string location_id = ddl_location.SelectedValue.ToString();

            //alert_Messend(ViewState["LOCATION_ID"].ToString());

            get_ddl_cell_station(location_id);
        }

        //protected void ddl_classCD_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    string class_cd = ddl_classCD.SelectedValue.ToString();

        //    if (class_cd == "-")
        //    {
        //        txt_classNM.Text = "-";
        //    }
        //    else
        //    {
        //        string str = " SELECT * FROM classCD_Asset_master where classCD_id = '" + class_cd + "'";
        //        dc.QryDT(conn, str);
        //        string classCD_Name = dc.dt.Rows[0]["classCD_Name"].ToString();
        //        txt_classNM.Text = classCD_Name;
        //    }

        //}

        protected string parsedPrice(object price)
        {
            string parsed = "";
            object priceObject = price;

            // 2. ตรวจสอบว่าไม่ใช่ค่าว่าง (NULL)
            if (priceObject != DBNull.Value)
            {
                decimal parsedPrice;
                // 3. ใช้ Decimal.TryParse เพื่อแปลงอย่างปลอดภัย (รองรับกรณีข้อมูลเป็นตัวอักษรปนมา)
                if (Decimal.TryParse(priceObject.ToString(), out parsedPrice))
                {
                    // 4. ถ้าแปลงสำเร็จ, ทำการจัดรูปแบบ "N2"
                    parsed = parsedPrice.ToString("N2");
                }
                else
                {
                    // กรณีข้อมูลมีรูปแบบตัวเลขที่ไม่ถูกต้อง ให้แสดงเป็น 0.00 หรือค่าว่าง
                    parsed = "0.00";
                }
            }
            else
            {
                // 5. ถ้าข้อมูลเป็น NULL ให้แสดงเป็น 0.00 หรือค่าว่าง
                parsed = "0.00";
            }
            return parsed;
        }
        protected decimal? ParsePriceFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            text = text.Trim();

            // กรณีคุณใช้ "-" เป็นตัวแทนของค่าว่าง
            if (text == "-") return null;

            // ตัดคอมม่าออกก่อน parse
            var raw = text.Replace(",", "");

            // ใช้ Invariant เพื่อแน่ใจว่า "." เป็นทศนิยม
            decimal d; // ประกาศตัวแปรก่อน
            if (decimal.TryParse(raw,
                                 NumberStyles.Number | NumberStyles.AllowDecimalPoint,
                                 CultureInfo.InvariantCulture,
                                 out d))

            {
                // ปรับเป็น 2 ตำแหน่งตาม schema
                return Math.Round(d, 2, MidpointRounding.AwayFromZero);
            }

            // parse ไม่ได้
            return null;
        }

        protected string DisplayString(object value)
        {
            if (value == null || value == DBNull.Value) return "-";

            string s = value.ToString().Trim();
            return string.IsNullOrEmpty(s) ? "-" : s;
        }
        private void RefreshChosenColors()
        {
            ScriptManager.RegisterStartupScript(this, GetType(),
                "updateColors", "updateAllSelect2Colors();", true);
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



    }
}
