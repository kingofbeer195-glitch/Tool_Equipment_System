using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tool_Equipment_Control_System
{
    public partial class CalibrationManagement : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        SqlConnection conn_jig = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        datacon dc = new datacon();

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = "Calibration";

            if (Session["emp_id"] == null || Session["emp_id"].ToString() == "")
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
            }
        }



        protected void gvCalib_RowCommand(object sender, GridViewCommandEventArgs e)
        {

        }
        protected void gvCalib_RowDataBound(object sender, GridViewRowEventArgs e)
        {

        }



        protected void btnSearch_Click(object sender, EventArgs e)
        {

        }
        protected void btnReset_Click(object sender, EventArgs e)
        {

        }
        protected void btnConfirmSend_Click(object sender, EventArgs e)
        {
        }
        protected void btnConfirmReturn_Click(object sender, EventArgs e)
        {

        }
        protected void btnSaveEquip_Click(object sender, EventArgs e)
        {

        }

        // ============================================================
        // Protected helpers — ใช้ใน ASPX TemplateField
        // ============================================================
        protected string GetDaysClass(string alertLevel)
        {
            switch (alertLevel)
            {
                case "Overdue": return "days-overdue";
                case "DueSoon": return "days-duesoon";
                case "Normal": return "days-normal";
                default: return "days-na";
            }
        }
        protected string GetBadgeClass(string alertLevel)
        {
            switch (alertLevel)
            {
                case "Overdue": return "badge-overdue";
                case "DueSoon": return "badge-duesoon";
                case "Sent": return "badge-sent";
                case "Normal": return "badge-normal";
                default: return "badge-nodate";
            }
        }
        protected string GetAlertText(string alertLevel)
        {
            switch (alertLevel)
            {
                case "Overdue": return "เกินกำหนด";
                case "DueSoon": return "ใกล้ครบกำหนด";
                case "Sent": return "ส่ง Calibrate แล้ว";
                case "Normal": return "ปกติ";
                default: return "ยังไม่กำหนด";
            }
        }
        protected string FormatDaysRemaining(object daysObj, string alertLevel)
        {
            if (alertLevel == "NoDate" || daysObj == DBNull.Value || daysObj == null)
                return "—";

            int days = Convert.ToInt32(daysObj);

            if (alertLevel == "Sent")
                return "ส่งแล้ว";

            if (days < 0)
                //return $"เกิน {Math.Abs(days)} วัน";
                return string.Format("เกิน {0} วัน", Math.Abs(days));

            if (days == 0)
                return "วันนี้!";

            //return $"{days} วัน";
            return string.Format("{0} วัน", days);

        }
        // ช่วย encode ค่าสำหรับ HTML attribute (ป้องกัน XSS / quote breaking)
        protected string JsAttr(object val)
        {
            if (val == null)
                return "";

            return System.Web.HttpUtility.HtmlAttributeEncode(val.ToString());
        }

        // ============================================================
        // Private helpers
        // ============================================================
        //private void ShowToast(string msg, bool success)
        //{
        //    string js = $"showToast({System.Web.HttpUtility.JavaScriptStringEncode(msg, true)},{(success ? "true" : "false")});";
        //    ScriptManager.RegisterStartupScript(this, GetType(), "toast", js, true);
        //}

        //private void ShowModalError(Label lbl, string msg)
        //{
        //    lbl.Text = $"<span class='glyphicon glyphicon-exclamation-sign'></span> {msg}";
        //    lbl.Visible = true;
        //}

        //private void ReopenModal(string modalId)
        //{
        //    ScriptManager.RegisterStartupScript(this, GetType(), "reopen",
        //        $"$('#{modalId}').modal('show');", true);
        //}

        //private static object NullIfEmpty(string val)
        //    => string.IsNullOrWhiteSpace(val) ? (object)DBNull.Value : val.Trim();
    }
}
