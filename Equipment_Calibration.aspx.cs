// ═══════════════════════════════════════════════════════════════════════════
//  Equipment_Calibration.aspx.cs
//  รายการเครื่องมือวัดที่ต้องสอบเทียบ (Category = Measurement)
// ───────────────────────────────────────────────────────────────────────────
//  • ที่มา : Tool_Equipment_List_View  (is_category = 11 หรือ category = 'Measurement')
//            ไม่รวม Transfer (11) / Scrap (12)
//  • Due Date : duedate → ถ้าแปลงไม่ได้ใช้ DueDate1
//  • ปุ่มกรอง : ทั้งหมด / เกินกำหนด / < 7 วัน / < 1 เดือน / ไม่มี Due Date
//  • แจ้งเตือนพิเศษ : รายการที่เหลือ < 3 วัน (รวมเกินกำหนด)
// ═══════════════════════════════════════════════════════════════════════════
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tool_Equipment_Control_System
{
    public partial class Equipment_Calibration : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        datacon dc = new datacon();

        private const int CATEGORY_MEASUREMENT = 11;
        private const int ALERT_DAYS = 3;        // แจ้งเตือนพิเศษ : เหลือน้อยกว่า 3 วัน (รวมเกินกำหนด)
        private const int WEEK_DAYS = 7;

        private static readonly CultureInfo EN = CultureInfo.GetCultureInfo("en-US");

        // ตัวกรองที่เลือก : all / overdue / d7 / m1 / nodate / alert
        private string FilterMode
        {
            get { return ViewState["CAL_FILTER"] as string ?? "all"; }
            set { ViewState["CAL_FILTER"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = "Equipment : Calibration";

            if (Session["emp_id"] == null || Session["emp_id"].ToString() == "")
            { Response.Redirect("Default.aspx"); return; }

            if (!IsPostBack)
            {
                BindDept();
                BindData();
            }
        }

        private void BindDept()
        {
            dc.QryDT(conn, "SELECT dept_id, dept_code, dept_name2 FROM View_dept_master WHERE division = '2' ORDER BY dept_code");
            ddlDept.Items.Clear();
            ddlDept.Items.Add(new ListItem("-- ทุก Dept --", ""));
            foreach (DataRow r in dc.dt.Rows)
                ddlDept.Items.Add(new ListItem(Convert.ToString(r["dept_name2"]), Convert.ToString(r["dept_id"])));
        }

        // ═══════════════════════════════════════════════════════
        //  Data
        // ═══════════════════════════════════════════════════════
        private DataTable LoadAll()
        {
            var prms = new List<SqlParameter>();
            string where = "";

            if (ddlDept.SelectedValue != "")
            {
                where += " AND V.dept_id = @dept";
                prms.Add(new SqlParameter("@dept", ddlDept.SelectedValue));
            }

            string kw = txtSearch.Text.Trim();
            if (kw != "")
            {
                where += " AND ( CAST(V.ID AS nvarchar(100)) LIKE @kw OR V.FixAcces LIKE @kw OR V.control_no LIKE @kw" +
                         "    OR V.asset_name LIKE @kw OR V.tool_no LIKE @kw OR V.serial_no LIKE @kw )";
                prms.Add(new SqlParameter("@kw", "%" + kw + "%"));
            }

            string sql = @"
                SELECT V.tool_id, V.sub_no, V.sys_status, V.ID, V.FixAcces, V.control_no, V.asset_name,
                       V.tool_no, V.serial_no, V.model_name, V.cell, V.station, V.dept_code, V.responsible, V.status,
                       D.due_date,
                       DATEDIFF(DAY, CAST(GETDATE() AS date), D.due_date) AS days_left
                FROM Tool_Equipment_List_View V
                CROSS APPLY (SELECT COALESCE(TRY_CONVERT(date, V.duedate), TRY_CONVERT(date, V.DueDate1)) AS due_date) D
                WHERE V.ID IS NOT NULL
                  AND V.status_id NOT IN (11, 12)
                  AND ( V.is_category = " + CATEGORY_MEASUREMENT + " OR LTRIM(RTRIM(V.category)) = 'Measurement' )" + where + @"
                ORDER BY CASE WHEN D.due_date IS NULL THEN 1 ELSE 0 END, D.due_date, V.ID";

            dc.QryDT_P(conn, sql, prms.ToArray());
            return dc.dt.Copy();
        }

        private void BindData()
        {
            DataTable all = LoadAll();
            DateTime today = DateTime.Today;
            DateTime oneMonth = today.AddMonths(1);

            int nAll = all.Rows.Count, nOver = 0, nWeek = 0, nMonth = 0, nNoDate = 0, nAlert = 0;
            var alertRows = new List<DataRow>();

            foreach (DataRow r in all.Rows)
            {
                if (r["due_date"] == DBNull.Value) { nNoDate++; continue; }
                DateTime due = Convert.ToDateTime(r["due_date"]);
                int d = Convert.ToInt32(r["days_left"]);
                if (d < 0) nOver++;
                if (d >= 0 && d < WEEK_DAYS) nWeek++;
                if (d >= 0 && due < oneMonth) nMonth++;
                if (d < ALERT_DAYS) { nAlert++; alertRows.Add(r); }
            }

            lblCntAll.Text = nAll.ToString("N0");
            lblCntOver.Text = nOver.ToString("N0");
            lblCntWeek.Text = nWeek.ToString("N0");
            lblCntMonth.Text = nMonth.ToString("N0");
            lblCntNoDate.Text = nNoDate.ToString("N0");
            lblCntAlert.Text = nAlert.ToString("N0");

            // ── แจ้งเตือนพิเศษ : < 3 วัน (รวมเกินกำหนด) ──
            pnlAlert.Visible = nAlert > 0;
            if (nAlert > 0)
            {
                var sb = new StringBuilder();
                foreach (DataRow r in alertRows.OrderBy(x => Convert.ToInt32(x["days_left"])).Take(30))
                {
                    int d = Convert.ToInt32(r["days_left"]);
                    sb.Append("<li><b>" + H(Convert.ToString(r["ID"])) + "</b>" +
                              (Convert.ToString(r["control_no"]) != "" ? " · " + H(Convert.ToString(r["control_no"])) : "") +
                              " — " + H(Convert.ToString(r["asset_name"])) +
                              " <span class='cal-tag " + (d < 0 ? "tag-over" : "tag-alert") + "'>" + DaysText(d) + "</span>" +
                              " <span class='cal-muted'>Due " + Convert.ToDateTime(r["due_date"]).ToString("dd-MMM-yyyy", EN) +
                              " · " + H(Convert.ToString(r["dept_code"])) + "</span></li>");
                }
                if (alertRows.Count > 30) sb.Append("<li class='cal-muted'>... และอีก " + (alertRows.Count - 30) + " รายการ</li>");
                litAlertList.Text = sb.ToString();
                litAlertTitle.Text = "มีเครื่องมือวัดเหลือเวลาสอบเทียบน้อยกว่า " + ALERT_DAYS + " วัน / เกินกำหนด " +
                                     nAlert.ToString("N0") + " รายการ — กรุณาดำเนินการโดยด่วน";
                if (!IsPostBack)   // เข้าหน้าครั้งแรก → กระพริบแจ้งเตือน
                    ScriptManager.RegisterStartupScript(this, GetType(), "calAlert",
                        "if(typeof calPulse==='function') calPulse();", true);
            }

            // ── กรองตามปุ่ม ──
            IEnumerable<DataRow> q = all.Rows.Cast<DataRow>();
            switch (FilterMode)
            {
                case "overdue": q = q.Where(r => r["due_date"] != DBNull.Value && Convert.ToInt32(r["days_left"]) < 0); break;
                case "d7": q = q.Where(r => r["due_date"] != DBNull.Value && Convert.ToInt32(r["days_left"]) >= 0 && Convert.ToInt32(r["days_left"]) < WEEK_DAYS); break;
                case "m1": q = q.Where(r => r["due_date"] != DBNull.Value && Convert.ToInt32(r["days_left"]) >= 0 && Convert.ToDateTime(r["due_date"]) < oneMonth); break;
                case "nodate": q = q.Where(r => r["due_date"] == DBNull.Value); break;
                case "alert": q = q.Where(r => r["due_date"] != DBNull.Value && Convert.ToInt32(r["days_left"]) < ALERT_DAYS); break;
            }
            DataTable view = q.Any() ? q.CopyToDataTable() : all.Clone();

            gvCalib.DataSource = view;
            gvCalib.DataBind();
            lblRecCount.Text = view.Rows.Count.ToString("N0");
            litFilterName.Text = FilterName(FilterMode);

            // ปุ่มที่เลือกอยู่
            var chips = new Dictionary<string, LinkButton>
            {
                { "all", chipAll }, { "overdue", chipOver }, { "d7", chipWeek },
                { "m1", chipMonth }, { "nodate", chipNoDate }, { "alert", chipAlert }
            };
            foreach (var kv in chips)
            {
                string baseCss = kv.Value.Attributes["data-css"] ?? kv.Value.CssClass.Replace(" active", "");
                kv.Value.Attributes["data-css"] = baseCss;
                kv.Value.CssClass = baseCss + (kv.Key == FilterMode ? " active" : "");
            }
        }

        // ═══════════════════════════════════════════════════════
        //  Events
        // ═══════════════════════════════════════════════════════
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            FilterMode = ((LinkButton)sender).CommandArgument;
            gvCalib.PageIndex = 0;
            BindData();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvCalib.PageIndex = 0;
            BindData();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlDept.SelectedIndex = 0;
            FilterMode = "all";
            gvCalib.PageIndex = 0;
            BindData();
        }

        protected void gvCalib_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvCalib.PageIndex = e.NewPageIndex;
            BindData();
        }

        protected void gvCalib_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            DataRowView drv = (DataRowView)e.Row.DataItem;

            Literal litDue = e.Row.FindControl("litDue") as Literal;
            Literal litDays = e.Row.FindControl("litDays") as Literal;
            HyperLink lnk = e.Row.FindControl("lnkDetail") as HyperLink;

            string css;
            if (drv["due_date"] == DBNull.Value)
            {
                css = "row-nodate";
                if (litDue != null) litDue.Text = "<span class='cal-muted'>—</span>";
                if (litDays != null) litDays.Text = "<span class='cal-tag tag-nodate'>ไม่มี Due Date</span>";
            }
            else
            {
                int d = Convert.ToInt32(drv["days_left"]);
                DateTime due = Convert.ToDateTime(drv["due_date"]);
                string tag = d < 0 ? "tag-over" : d < ALERT_DAYS ? "tag-alert" : d < WEEK_DAYS ? "tag-week"
                           : due < DateTime.Today.AddMonths(1) ? "tag-month" : "tag-ok";
                css = d < 0 ? "row-over" : d < ALERT_DAYS ? "row-alert" : d < WEEK_DAYS ? "row-week" : "";
                if (litDue != null) litDue.Text = due.ToString("dd-MMM-yyyy", EN);
                if (litDays != null) litDays.Text = "<span class='cal-tag " + tag + "'>" + DaysText(d) + "</span>";
            }
            e.Row.CssClass = css;

            if (lnk != null)
                lnk.NavigateUrl = "Tool_equipment_detail.aspx?tool_id=" + HttpUtility.UrlEncode(Convert.ToString(drv["tool_id"])) +
                                  "&tool_sub=" + HttpUtility.UrlEncode(Convert.ToString(drv["sub_no"])) +
                                  "&sys_status=" + HttpUtility.UrlEncode(Convert.ToString(drv["sys_status"]));
        }

        // ═══════════════════════════════════════════════════════
        //  Helpers
        // ═══════════════════════════════════════════════════════
        private static string H(string s) { return HttpUtility.HtmlEncode(s ?? ""); }

        private static string DaysText(int d)
        {
            if (d < 0) return "เกิน " + Math.Abs(d) + " วัน";
            if (d == 0) return "ครบกำหนดวันนี้";
            return "เหลือ " + d + " วัน";
        }

        private static string FilterName(string m)
        {
            switch (m)
            {
                case "overdue": return "เกินกำหนด";
                case "d7": return "ครบกำหนดภายใน 7 วัน";
                case "m1": return "ครบกำหนดภายใน 1 เดือน";
                case "nodate": return "ไม่มี Due Date";
                case "alert": return "แจ้งเตือน < 3 วัน (รวมเกินกำหนด)";
                default: return "ทั้งหมด";
            }
        }

    }
}
