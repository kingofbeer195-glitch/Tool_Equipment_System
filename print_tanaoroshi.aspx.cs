using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Web;
using System.Linq;

namespace Tool_Equipment_Control_System
{
    public partial class print_tanaoroshi : System.Web.UI.Page
    {
        private SqlConnection conn = new SqlConnection(
            ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["emp_id"] == null) { Response.Redirect("Default.aspx"); return; }

            string ids = Request.QueryString["ids"];
            if (string.IsNullOrEmpty(ids)) { ltl_content.Text = "<p>No data</p>"; return; }

            string[] listIds = ids.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            const int ROWS_PER_PAGE = 10;

            // เตรียมข้อมูลทุก List (ข้ามอันที่ไม่มีข้อมูล)
            var lists = new System.Collections.Generic.List<DataTable>();
            foreach (string listId in listIds)
            {
                DataTable dt = Get_history_by_key(listId.Trim());
                if (dt != null && dt.Rows.Count > 0) lists.Add(dt);
            }

            int totalLists = lists.Count;              // M = จำนวน List ทั้งหมด
            var sb = new System.Text.StringBuilder();

            for (int li = 0; li < totalLists; li++)
            {
                DataTable dt = lists[li];
                int listNo = li + 1;                   // ลำดับ List (1..M)

                int contentPages = (int)Math.Ceiling((double)dt.Rows.Count / ROWS_PER_PAGE);

                for (int p = 0; p < contentPages; p++)
                {
                    int pageInList = p + 1;            // เลขหน้าจริงของ List นี้ (เริ่มใหม่ทุก List)
                    var pageRows = dt.AsEnumerable()
                                     .Skip(p * ROWS_PER_PAGE)
                                     .Take(ROWS_PER_PAGE)
                                     .CopyToDataTable();

                    sb.Append(Render_page(
                        pageRows, dt.Rows[0],
                        pageInList,                    // มุมขวาบนสุด : หน้าจริงของ List
                        listNo, totalLists,            // เหนือ Checked By : N/M lists (เฉพาะหน้าแรก)
                        p == 0,                        // showTitle : เฉพาะหน้าแรกของ List
                        p * ROWS_PER_PAGE));
                }

                // เติมหน้าว่างถ้าหน้าของ List นี้เป็นคี่ (ให้ List ถัดไปเริ่มหน้า-หน้า)
                // หน้าว่าง = ไม่แสดงเลขหน้า (ข้อ 1.1)
                if (contentPages % 2 == 1)
                    sb.Append("<div class='sheet blank-sheet'></div>");
            }

            ltl_content.Text = sb.Length > 0 ? sb.ToString() : "<p>No data</p>";
        }

        // ── หน้าเนื้อหา ──
        // ── หน้าเนื้อหา ──
        private string Render_page(DataTable pageRows, DataRow head,
                                   int pageInList, int listNo, int totalLists,
                                   bool showTitle, int rowOffset)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("<div class='sheet'>");

            // ข้อ 1 : มุมขวาบนสุด = เลขหน้าจริงของ List นี้ (ทุกหน้าของ List)
            sb.Append("<div class='page-no'>" + pageInList + "</div>");

            if (showTitle)
            {
                sb.Append("<table class='head-tbl'><tr>");
                sb.Append("<td><span class='title'>CALIBRATION &amp; FIXED ASSET MONTHLY CHECK</span><br/>");
                sb.Append("MONTH : " + HttpUtility.HtmlEncode(head["tana_date"].ToString()) + "<br/>");
                sb.Append("RESPONSIBLE : " + HttpUtility.HtmlEncode(head["pic_rsp_name"].ToString()) + "</td>");
                sb.Append("<td class='sign'>");
                // ข้อ 2 : N/M lists — เฉพาะหน้าแรกของ List
                sb.Append("<div class='list-no'>" + listNo + "/" + totalLists + "</div>");
                sb.Append("Checked By : <span class='sign-line'></span><br/>");
                sb.Append("Checked Date : <span class='sign-line'></span></td>");
                sb.Append("</tr></table>");
            }
            else
            {
                sb.Append("<div class='cont-gap'></div>");
                sb.Append("<table class='head-tbl'><tr>");
                sb.Append("<td><span class='title'>CALIBRATION &amp; FIXED ASSET MONTHLY CHECK</span><br/>");
                sb.Append("MONTH : " + HttpUtility.HtmlEncode(head["tana_date"].ToString()) + "<br/>");
                sb.Append("RESPONSIBLE : " + HttpUtility.HtmlEncode(head["pic_rsp_name"].ToString()) + "</td>");
                sb.Append("<td class='sign'>");
                // ข้อ 2 : N/M lists — เฉพาะหน้าแรกของ List
                sb.Append("<div class='list-no'>" + listNo + "/" + totalLists + "</div>");
                //sb.Append("Checked By : <span class='sign-line'></span><br/>");
                //sb.Append("Checked Date : <span class='sign-line'></span></td>");
                sb.Append("</tr></table>");
            }

            sb.Append("<table class='data'><thead><tr>");
            sb.Append("<th class='c-no'>No.</th><th class='c-asset'>ASSET - NO.</th>");
            sb.Append("<th class='c-ctrl'>CONTROL NO.</th><th class='c-pic'>PICTURE</th>");
            sb.Append("<th class='c-name'>NAME</th><th class='c-model'>CPT Model</th>");
            sb.Append("<th class='c-cell'>Cell</th><th class='c-stn'>Station</th>");
            sb.Append("<th class='c-chk'>Check (OK/NG)</th></tr></thead><tbody>");

            for (int i = 0; i < pageRows.Rows.Count; i++)
            {
                DataRow r = pageRows.Rows[i];
                string imgUrl = Get_picture_url(r["tool_id"].ToString(),
                                                r["tool_sub_no"].ToString(),
                                                r["sys_status"].ToString());
                sb.Append("<tr>");
                sb.Append("<td>" + (rowOffset + i + 1) + "</td>");
                sb.Append("<td>" + HttpUtility.HtmlEncode(r["asset_no"].ToString()) + "</td>");
                sb.Append("<td>" + HttpUtility.HtmlEncode(r["control_no"].ToString()) + "</td>");
                sb.Append("<td>" + (string.IsNullOrEmpty(imgUrl) ? "" : "<img src='" + imgUrl + "' />") + "</td>");
                sb.Append("<td>" + HttpUtility.HtmlEncode(r["name"].ToString()) + "</td>");
                sb.Append("<td>" + HttpUtility.HtmlEncode(r["model_name"].ToString()) + "</td>");
                sb.Append("<td>" + HttpUtility.HtmlEncode(r["location2_name"].ToString()) + "</td>");
                sb.Append("<td>" + HttpUtility.HtmlEncode(r["location3_name"].ToString()) + "</td>");
                sb.Append("<td>&nbsp;</td></tr>");
            }

            sb.Append("</tbody></table></div>");
            return sb.ToString();
        }

        // ── หน้าว่าง (มีแต่เลขหน้า) ──
        //private string Render_blank_page(int pageNo, int totalPages)
        //{
        //    return "<div class='sheet blank-sheet'>" +
        //           "<div class='page-no'>" + pageNo + "/" + totalPages + "</div>" +
        //           "<div style='text-align:center;color:#ccc;margin-top:40%;'>(เว้นว่าง)</div></div>";
        //}
        private string Get_picture_url(string toolId, string toolSub, string sysStatus)
        {
            try
            {
                var fs = new Get_Folder_Structure_Picture(toolId, toolSub, sysStatus);
                return fs.get_folder_root_view_toolMain();   // web path สำหรับ <img src>
            }
            catch { return ""; }
        }

        // ── copy มาจาก setting_tanaoroshi.aspx.cs (query เดียวกัน) ──
        private DataTable Get_history_by_key(string listId)
        {
            string sql = @"
                SELECT H.tana_date, H.dept_code, H.pic_rsp
                          + ISNULL(' ' + RD.pic_dept, '')
                          + ISNULL(' (' + CAST(RD.dept_code_real AS NVARCHAR(10)) + ')', '') AS pic_rsp_name,
                       H.asset_no, H.sub_asset_no, H.control_no, H.name,
                       H.tool_id, H.sys_status, H.tool_sub_no,
                       ISNULL(VM.model_name, '-')      AS model_name,
                       ISNULL(VC.location2_name, '-')  AS location2_name,
                       ISNULL(VS.location3_name, '-')  AS location3_name
                FROM   Equipment_tana_history H
                OUTER APPLY (
                    SELECT TOP 1 pic_dept, dept_code_real
                    FROM   Responsible_Dept
                    WHERE  emp_id = H.pic_rsp
                    ORDER BY CASE WHEN dept_code = TRY_CAST(H.dept_code AS INT) THEN 0 ELSE 1 END
                ) RD
                LEFT JOIN View_dept_master DM ON DM.dept_code = H.dept_code
                LEFT JOIN jig_tool_system.dbo.draw_master_model VM ON VM.model_id = H.model_id
                LEFT JOIN jig_tool_system.dbo.master_location2 VC
                       ON VC.location2_id = H.cell_id AND VC.location1_id = H.location_id
                LEFT JOIN jig_tool_system.dbo.master_location3 VS
                       ON VS.location3_id = H.station_id AND VS.location1_id = H.location_id
                WHERE  H.list_id = @listId
                ORDER BY H.asset_no";

            var dt = new DataTable();
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@listId", SqlDbType.Int).Value = Convert.ToInt32(listId);
                using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
            }
            return dt;
        }
    }
}