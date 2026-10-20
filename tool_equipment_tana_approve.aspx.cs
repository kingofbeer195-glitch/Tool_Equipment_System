using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tool_Equipment_Control_System
{
    public partial class tool_equipment_tana_approve : System.Web.UI.Page
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
            if (Session["data_tana"] == null || Session["data_tana"].ToString() == "")
                Response.Redirect("tool_equipment_tanaoroshi.aspx");


            emp_id = Session["emp_id"].ToString();
            Get_data_tana_Approve();

            if (!IsPostBack)
            {

                Get_gv_tana_check();
            }


        }
        protected void Get_data_tana_Approve()
        {

            Dictionary<string, string> myData = (Dictionary<string, string>)Session["data_tana_approve"];

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

            ////Get ddl_model
            //sql = " select* from master_location3	where location1_id =" + location_id;
            //dc.QryDT(conn_jig, sql);
            //if (dc.dt.Rows.Count > 0)
            //{
            //    ddl_station.DataSource = dc.dt;
            //    ddl_station.DataValueField = "location3_id";
            //    ddl_station.DataTextField = "location3_name";
            //    ddl_station.DataBind();

            //}

        }
        protected void Get_gv_tana_approve()
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

            dc.QryDT(conn, sql);

            // lb_test.Text = sql;

            tana_approve.DataSource = dc.dt;
            tana_approve.DataBind();
        }


    }
}