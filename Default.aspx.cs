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
    public partial class Default : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["User"].ConnectionString);
        datacon dc = new datacon();
        protected void Page_Load(object sender, EventArgs e)
        {
            string emp_id = Request.QueryString["emp_id"];
            string baseUrl = "http://cptsvs552.canon-pt.co.th";

            Assign_Session(emp_id);

            //lb_test.Text += "emp_id : " + Session["emp_id"].ToString() + "<br/>";
            //lb_test.Text += "user_name : " + Session["user_name"].ToString() + "<br/>";
            //lb_test.Text += "positionId : " + Session["positionId"].ToString() + "<br/>";
            //lb_test.Text += "eng_group_id : " + Session["eng_group_id"].ToString() + "<br/>";
            //lb_test.Text += "oth_group_id : " + Session["oth_group_id"].ToString() + "<br/>";
            //lb_test.Text += "admin : " + Session["admin"].ToString() + "<br/>";
            //lb_test.Text += "dept : " + Session["dept"].ToString() + "<br/>";
            //lb_test.Text += "div : " + Session["div"].ToString() + "<br/>";

            if (Session["emp_id"] == null)
            {
                string url = (Session["url_problem"] == null ? "" : Session["url_problem"].ToString());
                string AbsolutePath = HttpContext.Current.Request.Url.AbsolutePath;

                if (AbsolutePath.ToLower().Contains("test"))
                {
                    Response.Redirect("" + baseUrl + "/User_master_test/login.aspx");
                }
                else
                {
                    Response.Redirect("" + baseUrl + "/Tool_Equipment_Control_System/Equipment_login.aspx");
                }
            }
            else
            {
                string url = baseUrl + "/Tool_Equipment_Control_System/MenuList.aspx";
                if (url != "")
                    Response.Redirect(url);

            }


        }
        public void Assign_Session(string emp_id)
        {
            login_getUser getUser = new login_getUser();
            getUser.Get_forSession(emp_id);
          //  Session["user_id"] = getUser.USER_ID;
            Session["emp_id"] = getUser.EMP_ID;
            Session["user_name"] = getUser.USER_NAME;
            Session["positionId"] = getUser.POSITION_ID;
            Session["eng_group_id"] = getUser.ENG_GROUP_ID;
            Session["oth_group_id"] = getUser.OTH_GROUP_ID;
            //Session["user_group"] = getUser.USER_GROUP;
            Session["admin"] = getUser.USER_ADMIN;
            Session["dept"] = getUser.USER_DEPT;
            Session["div"] = getUser.DIVITION_ID;
            //Label1.Text = "test";
        }
    }
}