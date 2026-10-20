using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tool_Equipment_Control_System
{
    public partial class setting_tanaoroshi : System.Web.UI.Page
    {
        // ─── Constants ────────────────────────────────────────────
        private const string DEPT_AAD = "5505";
        private const string FIX_LOCATION1_ID = "97"; // Fix location1_id = 97

        // ─── Fields ───────────────────────────────────────────────
        private SqlConnection conn = new SqlConnection(
            ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        private datacon dc = new datacon();

        private string emp_id;
        private string div_id;
        private string dept_id;

        // ─── Page Load ────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["emp_id"] == null || Session["emp_id"].ToString() == "")
            { Response.Redirect("Default.aspx"); return; }

            emp_id = Session["emp_id"].ToString();
            div_id = Session["div"] != null ? Session["div"].ToString() : "";
            dept_id = Session["dept"] != null ? Session["dept"].ToString() : "";

            if (!IsPostBack)
            {
                Set_ddl_month();
                Set_ddl_responsible();
                ddl_resp.Enabled = false;
                //ddl_dept.Visible = false;
                ddl_dept.Enabled = false;
                filter_dept.Visible = false;
                //filter_dept.Enabled = false;
                filter_model.Visible = false;
                filter_cell.Visible = false;
            }

            // Export All ต้อง full postback (ส่งไฟล์กลับ) — async postback จะ parse ไม่ได้
            ScriptManager.GetCurrent(this).RegisterPostBackControl(btn_export_all);
        }

        // ─── DDL : Month ──────────────────────────────────────────
        private void Set_ddl_month()
        {
            txt_set_date.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            int y = DateTime.Now.Year;
            int m = DateTime.Now.Month;
            for (int i = 1; i <= 12; i++)
            {
                string text = DateTimeFormatInfo.CurrentInfo.GetMonthName(i).Substring(0, 3) + "-" + y;
                ListItem item = new ListItem(text, text);
                if (i == m) item.Selected = true;
                ddl_month.Items.Add(item);
            }
        }

        // ─── DDL : Responsible ─────────────────────────────────────
        private void Set_ddl_responsible()
        {
            string sql = @"SELECT A.rsp_empID, B.pic_dept
                           FROM   Tool_Equipment_List_View AS A
                           LEFT JOIN Responsible_Dept AS B ON A.rsp_empID = B.emp_id
                           WHERE  A.rsp_empID IS NOT NULL
                           GROUP BY A.rsp_empID, B.pic_dept
                           ORDER BY B.pic_dept";
            dc.QryDT_P(conn, sql);

            ddl_resp.Items.Clear();
            ddl_resp.Items.Add(new ListItem("-- select --", ""));
            foreach (DataRow row in dc.dt.Rows)
                ddl_resp.Items.Add(new ListItem(
                    row["pic_dept"].ToString(), row["rsp_empID"].ToString()));
        }

        // ─── Event : Responsible Changed ───────────────────────────
        protected void ddl_resp_SelectedIndexChanged(object sender, EventArgs e)
        {
            string empId = ddl_resp.SelectedValue;

            if (string.IsNullOrEmpty(empId))
            {
                //ddl_dept.Visible = false;
                filter_dept.Visible = false;
                if (div_list.Visible) { BindGvList(); Load_tana_done(); }
                return;
            }

            string sql = @"SELECT A.dept_code, B.dept_name2
                           FROM   Responsible_Dept A
                           LEFT JOIN View_dept_master B ON A.dept_code = B.dept_code
                           WHERE  A.emp_id = @empId";
            dc.QryDT_P(conn, sql, new SqlParameter("@empId", empId));

            ddl_dept.Items.Clear();
            ddl_dept.Items.Add(new ListItem("-- All --", "ALL"));
            foreach (DataRow row in dc.dt.Rows)
                ddl_dept.Items.Add(new ListItem(
                    row["dept_name2"].ToString(), row["dept_code"].ToString()));

            ddl_dept.SelectedIndex = 0;
            //ddl_dept.Visible = true;
            filter_dept.Visible = true;

            if (div_list.Visible) { BindGvList(); Load_tana_done(); }
        }

        // ─── Event : Dept Changed ─────────────────────────────────
        protected void ddl_dept_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (div_list.Visible) { BindGvList(); Load_tana_done(); }
        }

        // ─── Event : Model / Cell filter Changed ──────────────────
        protected void ddl_filter_Changed(object sender, EventArgs e)
        {
            if (div_list.Visible) { BindGvList(); Load_tana_done(); }
        }

        // ─── Event : Search ───────────────────────────────────────
        protected void btn_search_Click(object sender, EventArgs e)
        {
            Get_list_by_rsp();
        }

        private void Get_list_by_rsp()
        {
            string dateTana = ddl_month.SelectedValue;
            string setDate = txt_set_date.Text.Trim();

            if (string.IsNullOrEmpty(dateTana))
            { ShowToast("Please select Month of Tanaoroshi", "Required Field", "warn"); return; }
            if (string.IsNullOrEmpty(setDate))
            { ShowToast("Please enter Setting Date", "Required Field", "warn"); return; }

            ddl_resp.SelectedIndex = 0;
            //ddl_dept.Visible = false;
            filter_dept.Visible = false;
            filter_model.Visible = true;
            filter_cell.Visible = true;
            Set_ddl_model_cell();

            BindGvList();
            ddl_resp.Enabled = true;            

            int total = dc.dt.Rows.Count;
            lbl_done_month.Text = dateTana;

            Load_tana_done();

            div_list.Visible = total > 0;
            if (total > 0) ShowToast("Found " + total + " item(s)", "Search Result", "success");
            else ShowToast("No data found", "No Result", "warn");
        }

        // ═══════════════════════════════════════════════════════════
        //  ★ ROOT CAUSE FIX : Build SQL (Tab 1)
        //  เทียบสถานะ Set/NotSet ด้วย identity 3 คอลัมน์เท่านั้น
        //  (tool_id + tool_sub_no + sys_status) — ไม่เอา dept/location
        //  เพราะอาจถูกแก้ทีหลัง ทำให้ join ไม่เจอ แสดงผิดเป็น NotSet
        // ═══════════════════════════════════════════════════════════
        private string BuildTab1Sql(out List<SqlParameter> prms)
        {
            prms = new List<SqlParameter>();

            string dateTana = ddl_month.SelectedValue;
            string picRsp = ddl_resp.SelectedValue;
            string deptCode = ddl_dept.Visible ? ddl_dept.SelectedValue : "";
            //string deptCode = filter_dept.Visible = false ? ddl_dept.SelectedValue : "";
            string modelId = ddl_model.SelectedValue;
            string cellId = ddl_cell.SelectedValue;

            prms.Add(new SqlParameter("@dateTana", (object)dateTana ?? DBNull.Value));

            string whereRsp = "";
            if (!string.IsNullOrEmpty(picRsp))
            { whereRsp = " AND V.rsp_empID = @picRsp"; prms.Add(new SqlParameter("@picRsp", picRsp)); }

            string whereDept = "";
            if (!string.IsNullOrEmpty(deptCode) && deptCode != "ALL")
            { whereDept = " AND V.dept_code = @deptCode"; prms.Add(new SqlParameter("@deptCode", deptCode)); }

            string whereModel = "";
            if (!string.IsNullOrEmpty(modelId))
            { whereModel = " AND V.model_id = @modelId"; prms.Add(new SqlParameter("@modelId", SafeInt(modelId))); }

            string whereCell = "";
            if (!string.IsNullOrEmpty(cellId))
            { whereCell = " AND V.location2_id = @cellId"; prms.Add(new SqlParameter("@cellId", SafeInt(cellId))); }

            return @"
                SELECT V.*,
                       CASE WHEN H.tool_id IS NOT NULL THEN 'Done' ELSE 'NotSet' END AS tana_status_flag
                FROM   Tool_Equipment_List_View V
                LEFT JOIN (
                        SELECT DISTINCT tool_id, tool_sub_no, sys_status
                        FROM   Equipment_tana_history
                        WHERE  tana_date = @dateTana
                ) H ON  H.tool_id     = V.tool_id
                    AND H.tool_sub_no = V.sub_no
                    AND H.sys_status  = V.sys_status
                WHERE ((LEFT(V.FixAcces,3)='BFM') OR (V.control_no IS NOT NULL))
                  AND V.location1_id = " + FIX_LOCATION1_ID
                + whereRsp + whereDept + whereModel + whereCell + @"
                OPTION(RECOMPILE)";
        }

        private void BindGvList()
        {
            List<SqlParameter> prms;
            string sql = BuildTab1Sql(out prms);
            dc.QryDT_P(conn, sql, prms.ToArray());
            gv_list.DataSource = dc.dt;
            gv_list.DataBind();
            RefreshStatusCounts();
        }

        private void RefreshStatusCounts()
        {
            int total = dc.dt != null ? dc.dt.Rows.Count : 0;
            int done = dc.dt != null
                ? dc.dt.AsEnumerable().Count(r => r["tana_status_flag"].ToString() == "Done")
                : 0;
            int notSet = total - done;

            lbl_chip_total.Text = total.ToString("N0");
            lbl_chip_done.Text = done.ToString("N0");
            lbl_chip_notset.Text = notSet.ToString("N0");
            lbl_total_count.Text = total.ToString("N0");
        }

        // ─── Load Tab 2 (Equipment_tana_list) ─────────────────────
        private void Load_tana_done()
        {
            List<SqlParameter> prms;
            string sql = BuildTab2Sql(out prms);
            dc.QryDT_P(conn, sql, prms.ToArray());
            gv_done.DataSource = dc.dt;
            gv_done.DataBind();
            lbl_done_count.Text = dc.dt.Rows.Count.ToString("N0");
        }

        // ─── Clear Tanaoroshi (ลบทั้งเดือน ตาม filter) ─────────────
        protected void btn_clear_tana_Click(object sender, EventArgs e)
        {
            string monthTana = ddl_month.SelectedValue;
            if (string.IsNullOrEmpty(monthTana))
            { ShowToast("Please select Month", "Required Field", "warn"); return; }

            Clear_tana();
        }
        private void Clear_tana()
        {
            try
            {
                // STEP 1 : หา list id ที่จะลบ (ตาม filter Tab 2)
                List<SqlParameter> prms;
                string selectIdSql = "SELECT TL.id FROM Equipment_tana_list TL "
                                   + BuildTab2WhereForClear(out prms);

                datacon dcId = new datacon();
                dcId.QryDT_P(conn, selectIdSql, prms.ToArray());

                if (dcId.dt.Rows.Count == 0)
                { ShowToast("ไม่มีรายการให้ Clear", "No Data", "warn"); return; }

                var ids = dcId.dt.AsEnumerable()
                            .Select(r => Convert.ToInt32(r["id"])).ToList();

                // STEP 2 : ลบด้วย id list (history ก่อน → header) ครอบ transaction
                var idPrms = new List<SqlParameter>();
                var names = new List<string>();
                for (int i = 0; i < ids.Count; i++)
                {
                    string p = "@cid" + i;
                    names.Add(p);
                    idPrms.Add(new SqlParameter(p, ids[i]));
                }
                string inClause = string.Join(",", names);

                string sql = @"
            BEGIN TRY BEGIN TRANSACTION;
                DELETE FROM Equipment_tana_history WHERE list_id IN (" + inClause + @");
                DELETE FROM Equipment_tana_list    WHERE id      IN (" + inClause + @");
            COMMIT; END TRY
            BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH";

                dc.RunCommand_P(conn, sql, idPrms.ToArray());

                BindGvList();
                Load_tana_done();
                ShowToast("Clear Tanaoroshi เรียบร้อย " + ids.Count + " list", "Completed", "success");
            }
            catch (Exception ex) { Alert_message(ex.Message); }
        }
        private string BuildTab2WhereForClear(out List<SqlParameter> prms)
        {
            prms = new List<SqlParameter>();

            string dateTana = ddl_month.SelectedValue;
            string picRsp = ddl_resp.SelectedValue;
            string deptCode = ddl_dept.Visible ? ddl_dept.SelectedValue : "";
            string modelId = ddl_model.SelectedValue;
            string cellId = ddl_cell.SelectedValue;

            prms.Add(new SqlParameter("@dateTana", (object)dateTana ?? DBNull.Value));
            string sql = " WHERE TL.tana_date = @dateTana";

            if (!string.IsNullOrEmpty(picRsp))
            { sql += " AND TL.pic_rsp = @picRsp"; prms.Add(new SqlParameter("@picRsp", picRsp)); }

            if (!string.IsNullOrEmpty(deptCode) && deptCode != "ALL")
            { sql += " AND TL.dept_code = @deptCode"; prms.Add(new SqlParameter("@deptCode", deptCode)); }

            if (!string.IsNullOrEmpty(modelId))
            { sql += " AND TL.model_id = @modelId"; prms.Add(new SqlParameter("@modelId", SafeInt(modelId))); }

            if (!string.IsNullOrEmpty(cellId))
            { sql += " AND TL.cell_id = @cellId"; prms.Add(new SqlParameter("@cellId", SafeInt(cellId))); }

            return sql;
        }

        // Tab 2 SQL (ใช้ร่วมกับ Get_tana_done_dt)
        private string BuildTab2Sql(out List<SqlParameter> prms)
        {
            prms = new List<SqlParameter>();

            string dateTana = ddl_month.SelectedValue;
            string picRsp = ddl_resp.SelectedValue;
            string deptCode = ddl_dept.Visible ? ddl_dept.SelectedValue : "";
            //string deptCode = filter_dept.Visible = false ? ddl_dept.SelectedValue : "";
            string modelId = ddl_model.SelectedValue;
            string cellId = ddl_cell.SelectedValue;

            prms.Add(new SqlParameter("@dateTana", (object)dateTana ?? DBNull.Value));

            string whereRsp = "";
            if (!string.IsNullOrEmpty(picRsp))
            { whereRsp = " AND TL.pic_rsp = @picRsp"; prms.Add(new SqlParameter("@picRsp", picRsp)); }

            string whereDept = "";
            if (!string.IsNullOrEmpty(deptCode) && deptCode != "ALL")
            { whereDept = " AND TL.dept_code = @deptCode"; prms.Add(new SqlParameter("@deptCode", deptCode)); }

            string whereModel = "";
            if (!string.IsNullOrEmpty(modelId))
            { whereModel = " AND TL.model_id = @modelId"; prms.Add(new SqlParameter("@modelId", SafeInt(modelId))); }

            string whereCell = "";
            if (!string.IsNullOrEmpty(cellId))
            { whereCell = " AND TL.cell_id = @cellId"; prms.Add(new SqlParameter("@cellId", SafeInt(cellId))); }

            return @"
                SELECT TL.id, TL.tana_date, TL.dept_code, TL.pic_rsp,
                       TL.model_id, TL.location_id, TL.cell_id,
                       TL.tana_status, TL.pic_set_tana,
                       ISNULL(VM.model_name, '-') AS model_name,
                       VL.location1_name, VC.location2_name,
                       -- ★ Responsible : 001234 Anirut P. (5330)
                       TL.pic_rsp
                         + ISNULL(' ' + RD.pic_dept, '')
                         + ISNULL(' (' + CAST(RD.dept_code_real AS NVARCHAR(10)) + ')', '') AS pic_rsp_display
                FROM   Equipment_tana_list TL
                LEFT JOIN jig_tool_system.dbo.draw_master_model VM ON TL.model_id = VM.model_id
                LEFT JOIN jig_tool_system.dbo.master_location1  VL ON TL.location_id = VL.location1_id
                LEFT JOIN jig_tool_system.dbo.master_location2  VC ON TL.cell_id = VC.location2_id AND TL.location_id = VC.location1_id
                OUTER APPLY (
                    SELECT TOP 1 pic_dept, dept_code_real
                    FROM   Responsible_Dept
                    WHERE  emp_id = TL.pic_rsp
                    ORDER BY CASE WHEN dept_code = TRY_CAST(TL.dept_code AS INT) THEN 0 ELSE 1 END
                ) RD
                WHERE  TL.tana_date = @dateTana"
                + whereRsp + whereDept + whereModel + whereCell + @"
                ORDER BY TL.dept_code, VM.model_name, VC.location2_name";
        }

        private DataTable Get_tana_done_dt()
        {
            List<SqlParameter> prms;
            string sql = BuildTab2Sql(out prms);
            datacon dc2 = new datacon();
            dc2.QryDT_P(conn, sql, prms.ToArray());
            return dc2.dt;
        }

        // ═══════════════════════════════════════════════════════════
        //  Get History by list_id (สำหรับ Modal / Export ทีละ list)
        //  แสดง Location จาก history (snapshot) + แนบค่าปัจจุบันจาก view
        //  พร้อมธง ref_status = OK / CHANGED / MISSING
        // ═══════════════════════════════════════════════════════════
        private DataTable Get_history_by_key(string listId)
        {
            string sql = @"
                SELECT H.tana_date, H.dept_code, H.pic_rsp,
                       H.pic_rsp
                          + ISNULL(' ' + RD.pic_dept, '')
                          + ISNULL(' (' + CAST(RD.dept_code_real AS NVARCHAR(10)) + ')', '') AS pic_rsp_name,
                       H.asset_no, H.sub_asset_no, H.control_no, H.name,
                       H.tool_id, H.sys_status, H.tool_sub_no,
                       ISNULL(VM.model_name, '-')      AS model_name,
                       ISNULL(VC.location2_name, '-')  AS location2_name,
                       ISNULL(VS.location3_name, '-')  AS location3_name,
                       -- ค่าปัจจุบันจาก view (อ้างอิง) เทียบด้วย identity 3 คอลัมน์
                       ISNULL(CUR.cell,      '(ไม่พบในระบบ)') AS cur_cell,
                       ISNULL(CUR.station,   '(ไม่พบในระบบ)') AS cur_station,
                       ISNULL(CUR.dept_code, '-')             AS cur_dept,
                       CASE
                         WHEN CUR.tool_id IS NULL THEN 'MISSING'
                         WHEN ISNULL(CUR.location2_id,-1) <> ISNULL(H.cell_id,-1)
                           OR ISNULL(CUR.station_id,-1)   <> ISNULL(H.station_id,-1)
                           OR ISNULL(CUR.dept_code,'')    <> ISNULL(H.dept_code,'')
                         THEN 'CHANGED' ELSE 'OK'
                       END AS ref_status
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
                LEFT JOIN Tool_Equipment_List_View CUR
                       ON CUR.tool_id    = H.tool_id
                      AND CUR.sub_no     = H.tool_sub_no
                      AND CUR.sys_status = H.sys_status
                WHERE  H.list_id = @listId
                ORDER BY H.asset_no";

            datacon dc2 = new datacon();
            dc2.QryDT_P(conn, sql, new SqlParameter("@listId", SafeInt(listId)));
            return dc2.dt;
        }

        // ═══════════════════════════════════════════════════════════
        //  Get History หลาย list_id ในครั้งเดียว (สำหรับ Export All)
        //  1 query แทนการวน query ทีละ list — เร็วขึ้นมาก
        // ═══════════════════════════════════════════════════════════
        private DataTable Get_history_by_listids(List<string> listIds)
        {
            var ints = listIds.Select(SafeInt).Where(v => v > 0).Distinct().ToList();
            if (ints.Count == 0) return new DataTable();

            // สร้าง parameter @id0,@id1,... กัน injection (IN clause ใช้ param ตรง ๆ ไม่ได้)
            var prms = new List<SqlParameter>();
            var names = new List<string>();
            for (int i = 0; i < ints.Count; i++)
            {
                string p = "@id" + i;
                names.Add(p);
                prms.Add(new SqlParameter(p, ints[i]));
            }

            string sql = @"
                SELECT H.list_id, H.tana_date, H.dept_code, H.pic_rsp,
                       H.pic_rsp
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
                WHERE  H.list_id IN (" + string.Join(",", names) + @")
                ORDER BY H.list_id, H.asset_no";

            datacon dc2 = new datacon();
            dc2.QryDT_P(conn, sql, prms.ToArray());
            return dc2.dt;
        }

        // ─── RowDataBound Tab 1 : Status badge ────────────────────
        protected void gv_list_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            string flag = DataBinder.Eval(e.Row.DataItem, "tana_status_flag") as string ?? "NotSet";
            Label lbStatus = e.Row.FindControl("lb_status_flag") as Label;

            if (lbStatus != null)
            {
                if (flag == "Done")
                {
                    lbStatus.Text = "<span class=\"badge-done\">Set Done</span>";
                    e.Row.CssClass = "row-done";
                    //var chk = e.Row.FindControl("chk_row") as CheckBox;
                    //if (chk != null) chk.Checked = false;   // ★ set แล้ว → ไม่ติ๊ก
                }
                else
                {
                    lbStatus.Text = "<span class=\"badge-notset\">Not Set</span>";
                    e.Row.CssClass = "row-notset";
                }
            }
        }

        // ─── RowDataBound Tab 2 : register export button as postback ─
        protected void gv_done_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            LinkButton btnExp = e.Row.FindControl("btn_export_row") as LinkButton;
            if (btnExp != null)
                ScriptManager.GetCurrent(this).RegisterPostBackControl(btnExp);
        }

        // ─── Modal : show history ─────────────────────────────────
        protected void btn_show_history_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            GridViewRow row = (GridViewRow)btn.NamingContainer;
            string listId = GetCell(row, "hd_list_id");

            DataTable dt = Get_history_by_key(listId);
            gv_modal.DataSource = dt;
            gv_modal.DataBind();

            lbl_m_listid.Text = listId;
            lbl_modal_count.Text = dt.Rows.Count.ToString("N0");

            if (dt.Rows.Count > 0)
            {
                lbl_m_date.Text = dt.Rows[0]["tana_date"].ToString();
                lbl_m_dept.Text = dt.Rows[0]["dept_code"].ToString();
                lbl_m_rsp.Text = dt.Rows[0]["pic_rsp_name"].ToString();
                lbl_m_cell.Text = dt.Rows[0]["location2_name"].ToString();
            }
            else
            {
                lbl_m_date.Text = lbl_m_dept.Text = lbl_m_rsp.Text = lbl_m_cell.Text = "-";
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(),
                "openModal_" + DateTime.UtcNow.Ticks, "openTanaModal();", true);
        }

        // ─── Modal RowDataBound : ระบายสีแถวที่ location เปลี่ยน / หาย ─
        protected void gv_modal_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            string refStatus = (DataBinder.Eval(e.Row.DataItem, "ref_status") ?? "OK").ToString();
            if (refStatus == "CHANGED")
                e.Row.Attributes["style"] = "background:#fff8e1;";      // เหลือง : location เปลี่ยน
            else if (refStatus == "MISSING")
                e.Row.Attributes["style"] = "background:#fdecec;";      // แดง : ชิ้นนี้หายจาก view

            // status pill
            var lb = e.Row.FindControl("lb_ref") as Label;
            if (lb != null)
            {
                if (refStatus == "CHANGED")
                    lb.Text = "<span class=\"pill-status\" style=\"background:#fff5e6;color:#b7791f;\">Location เปลี่ยน</span>";
                else if (refStatus == "MISSING")
                    lb.Text = "<span class=\"pill-status\" style=\"background:#fdecec;color:#c53030;\">ไม่พบในระบบ</span>";
                else
                    lb.Text = "<span class=\"pill-status\">OK</span>";
            }
        }

        // ─── Export ทีละ row ──────────────────────────────────────
        protected void btn_export_row_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            GridViewRow row = (GridViewRow)btn.NamingContainer;
            string listId = GetCell(row, "hd_list_id");

            DataTable dt = Get_history_by_key(listId);
            if (dt == null || dt.Rows.Count == 0)
            { ShowToast("No data to export", "Export", "warn"); return; }

            string month = dt.Rows[0]["tana_date"].ToString();
            string rspName = dt.Rows[0]["pic_rsp_name"].ToString();
            string cellName = dt.Rows[0]["location2_name"].ToString();

            string fileName = "Calibration_Check_" + cellName.Replace(" ", "_") + "_" + month + ".xlsx";

            using (var wb = new XLWorkbook())
            {
                Write_check_sheet(wb, Safe_sheet_name(cellName, wb), dt, month, rspName);
                Set_sheet_number_header(wb);
                SendExcel(wb, fileName);
            }
        }

        // ─── Export All (เฉพาะที่ติ๊ก — ส่ง ids มาจาก hidden field) ──
        protected void btn_export_all_Click(object sender, EventArgs e)
        {
            var ids = (hf_selected_ids.Value ?? "")
                      .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                      .ToList();

            if (ids.Count == 0)
            { ShowToast("กรุณาเลือกรายการที่ต้องการ Export", "No Selection", "warn"); return; }

            Server.ScriptTimeout = 600;

            string dateTana = ddl_month.SelectedValue;
            string fileName = "Calibration_Check_All_" + dateTana + ".xlsx";

            // ดึงทั้งหมดด้วย query เดียว แล้วแบ่ง sheet ใน memory
            DataTable allDt = Get_history_by_listids(ids);
            if (allDt.Rows.Count == 0)
            { ShowToast("No data to export", "Export", "warn"); return; }

            var groups = allDt.AsEnumerable()
                              .GroupBy(r => r["list_id"].ToString())
                              .ToList();

            using (var wb = new XLWorkbook())
            {
                int idx = 1;
                foreach (var g in groups)
                {
                    DataTable sheetDt = g.CopyToDataTable();
                    string month = sheetDt.Rows[0]["tana_date"].ToString();
                    string rspName = sheetDt.Rows[0]["pic_rsp_name"].ToString();
                    string cellName = sheetDt.Rows[0]["location2_name"].ToString();
                    string modelNm = sheetDt.Rows[0]["model_name"].ToString();

                    string sheetName = Safe_sheet_name(idx + "_" + modelNm + "_" + cellName, wb);
                    Write_check_sheet(wb, sheetName, sheetDt, month, rspName);
                    idx++;
                }

                if (!wb.Worksheets.Any())
                { ShowToast("No data to export", "Export", "warn"); return; }

                Set_sheet_number_header(wb);
                SendExcel(wb, fileName);
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  Write Excel sheet (ฟอร์ม Calibration & Fixed Asset Check)
        // ═══════════════════════════════════════════════════════════
        private void Write_check_sheet(XLWorkbook wb, string sheetName, DataTable dt,
                                       string month, string rspName)
        {
            var ws = wb.Worksheets.Add(sheetName);

            // Title block
            ws.Cell(2, 1).Value = "CALIBRATION & FIXED ASSET MONTHLY CHECK";
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Font.FontSize = 11;

            ws.Cell(3, 1).Value = "MONTH : " + month;
            ws.Cell(4, 1).Value = "RESPONSIBLE : " + rspName;
            ws.Range(3, 1, 4, 1).Style.Font.FontSize = 10;

            ws.Cell(3, 7).Value = "Checked By :";
            ws.Cell(4, 7).Value = "Checked Date :";
            ws.Range(3, 7, 4, 7).Style.Font.FontSize = 10;
            ws.Range(3, 7, 4, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Range(3, 8, 4, 9).Style.Border.BottomBorder = XLBorderStyleValues.Thin;

            // Header row
            int hRow = 7;
            string[] cols = { "No.", "ASSET - NO.", "CONTROL NO.", "PICTURE", "NAME",
                              "CPT Model", "Cell", "Station", "Check (OK/NG)" };
            for (int c = 0; c < cols.Length; c++)
            {
                var cell = ws.Cell(hRow, c + 1);
                cell.Value = cols[c];
                cell.Style.Font.FontName = "Calibri";
                cell.Style.Font.FontSize = 8;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#808080");
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }

            // Data rows
            int startRow = hRow + 1;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow r = dt.Rows[i];
                int n = startRow + i;

                ws.Cell(n, 1).Value = i + 1;
                ws.Cell(n, 2).Value = r["asset_no"].ToString();
                ws.Cell(n, 3).Value = r["control_no"].ToString();
                ws.Cell(n, 4).Value = "";
                ws.Cell(n, 5).Value = r["name"].ToString();
                ws.Cell(n, 6).Value = r["model_name"].ToString();
                ws.Cell(n, 7).Value = r["location2_name"].ToString();
                ws.Cell(n, 8).Value = r["location3_name"].ToString();
                ws.Cell(n, 9).Value = "";

                ws.Row(n).Height = 65;

                Insert_picture_to_cell(ws, n, 4,
                    r["tool_id"].ToString(),
                    r["tool_sub_no"].ToString(),
                    r["sys_status"].ToString());
            }

            int lastRow = startRow + dt.Rows.Count - 1;

            var data = ws.Range(startRow, 1, lastRow, 9);
            data.Style.Font.FontName = "Calibri";
            data.Style.Font.FontSize = 9;
            data.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            data.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            data.Style.Alignment.WrapText = true;

            var all = ws.Range(hRow, 1, lastRow, 9);
            all.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            all.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Column(1).Width = 4;
            ws.Column(2).Width = 12.5;
            ws.Column(3).Width = 13;
            ws.Column(4).Width = 14;
            ws.Column(5).Width = 22;
            ws.Column(6).Width = 9;
            ws.Column(7).Width = 8;
            ws.Column(8).Width = 10;
            ws.Column(9).Width = 10;

            ws.PageSetup.PaperSize = XLPaperSize.A5Paper;
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            ws.PageSetup.Margins.Left = 0.25;
            ws.PageSetup.Margins.Right = 0.25;
            ws.PageSetup.Margins.Top = 0.75;
            ws.PageSetup.Margins.Bottom = 0.75;
            ws.PageSetup.Margins.Header = 0.3;
            ws.PageSetup.Margins.Footer = 0.3;
            ws.PageSetup.Scale = 62;
            ws.PageSetup.Header.Right.AddText("&P");
            ws.PageSetup.SetRowsToRepeatAtTop(hRow, hRow);
            ws.PageSetup.ShowGridlines = false;
            ws.PageSetup.PageOrder = XLPageOrderValues.DownThenOver;
        }

        // ─── แทรกรูปลง cell (ใช้ cache thumbnail) ─────────────────
        private void Insert_picture_to_cell(IXLWorksheet ws, int rowN, int colN,
                                            string toolId, string toolSub, string sysStatus)
        {
            try
            {
                var fs = new Get_Folder_Structure_Picture(toolId, toolSub, sysStatus);
                string folder = Resolve_physical_path(fs.get_folder_root());
                if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return;

                var files = Directory.GetFiles(folder, "*.jpg", SearchOption.TopDirectoryOnly);
                if (files.Length == 0) return;

                const int maxW = 92, maxH = 66;
                int w, h;
                using (var ms = Get_thumbnail_cached(files[0], maxW, maxH, out w, out h))
                {
                    ws.AddPicture(ms)
                      .MoveTo(ws.Cell(rowN, colN), (maxW - w) / 2 + 4, (maxH - h) / 2 + 4)
                      .WithSize(w, h);
                }
            }
            catch { /* ไม่มีรูป/ไฟล์เสีย → ปล่อยว่าง ไม่ให้ export ทั้งไฟล์พัง */ }
        }

        // ─── Cache thumbnail : gen ครั้งแรก ครั้งถัดไปอ่าน cache ───
        private MemoryStream Get_thumbnail_cached(string filePath, int maxW, int maxH, out int outW, out int outH)
        {
            outW = outH = 0;
            var fi = new FileInfo(filePath);
            string key = filePath + "|" + fi.LastWriteTimeUtc.Ticks + "|" + maxW + "x" + maxH;

            string hash;
            using (var md5 = System.Security.Cryptography.MD5.Create())
                hash = BitConverter.ToString(md5.ComputeHash(
                    System.Text.Encoding.UTF8.GetBytes(key))).Replace("-", "");

            string cacheDir = Server.MapPath("~/App_Data/thumb_cache");
            if (!Directory.Exists(cacheDir)) Directory.CreateDirectory(cacheDir);
            string cacheFile = Path.Combine(cacheDir, hash + ".jpg");

            if (File.Exists(cacheFile))
            {
                using (var img = System.Drawing.Image.FromFile(cacheFile))
                { outW = img.Width; outH = img.Height; }
                return new MemoryStream(File.ReadAllBytes(cacheFile));
            }

            using (var ms = Make_thumbnail(filePath, maxW, maxH, out outW, out outH))
            {
                try { File.WriteAllBytes(cacheFile, ms.ToArray()); } catch { }
                return new MemoryStream(ms.ToArray());
            }
        }

        private string Resolve_physical_path(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "";
            path = path.Trim();
            if (path.StartsWith(@"\\") || (path.Length > 1 && path[1] == ':')) return path;
            if (path.StartsWith("~") || path.StartsWith("/"))
            {
                try { return HttpContext.Current.Server.MapPath(path); } catch { return ""; }
            }
            try { return HttpContext.Current.Server.MapPath("~/" + path.TrimStart('/', '\\')); }
            catch { return ""; }
        }

        private MemoryStream Make_thumbnail(string filePath, int maxW, int maxH, out int outW, out int outH)
        {
            outW = outH = 0;
            using (var src = System.Drawing.Image.FromFile(filePath))
            {
                double ratio = Math.Min((double)maxW / src.Width, (double)maxH / src.Height);
                if (ratio > 1) ratio = 1;

                outW = Math.Max(1, (int)(src.Width * ratio));
                outH = Math.Max(1, (int)(src.Height * ratio));

                using (var bmp = new System.Drawing.Bitmap(outW, outH))
                {
                    using (var g = System.Drawing.Graphics.FromImage(bmp))
                    {
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.DrawImage(src, 0, 0, outW, outH);
                    }

                    var codec = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                                  .First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
                    var encParams = new System.Drawing.Imaging.EncoderParameters(1);
                    encParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                        System.Drawing.Imaging.Encoder.Quality, 75L);

                    var ms = new MemoryStream();
                    bmp.Save(ms, codec, encParams);
                    ms.Position = 0;
                    return ms;
                }
            }
        }

        // ─── Send Excel ───────────────────────────────────────────
        private void SendExcel(XLWorkbook wb, string fileName)
        {
            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename=\"" + fileName + "\"");

            // ── ปิด overlay ฝั่ง client : ฝัง cookie ให้ JS ตรวจเจอ ──
            var ck = new HttpCookie("fileDownloadToken", DateTime.Now.Ticks.ToString());
            ck.Path = "/";
            Response.Cookies.Add(ck);

            using (var ms = new MemoryStream())
            {
                wb.SaveAs(ms);
                Response.BinaryWrite(ms.ToArray());
            }
            Response.Flush();
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        private string Safe_sheet_name(string name, XLWorkbook wb = null)
        {
            string[] invalid = { "\\", "/", "*", "[", "]", ":", "?" };
            foreach (string c in invalid) name = name.Replace(c, "_");
            if (string.IsNullOrWhiteSpace(name)) name = "Sheet";
            if (name.Length > 31) name = name.Substring(0, 31);
            if (wb == null) return name;

            string baseName = name;
            int n = 2;
            while (wb.Worksheets.Any(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                string suffix = "_" + n++;
                name = (baseName.Length + suffix.Length > 31
                            ? baseName.Substring(0, 31 - suffix.Length) : baseName) + suffix;
            }
            return name;
        }

        private void Set_sheet_number_header(XLWorkbook wb)
        {
            int total = wb.Worksheets.Count;
            int i = 1;
            foreach (var ws in wb.Worksheets)
            {
                var c = ws.Cell(1, 8);
                c.Value = i + "/" + total;
                c.Style.Font.FontSize = 9;
                c.Style.Font.Bold = true;
                c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                i++;
            }
        }

        // ─── Set Tanaoroshi ───────────────────────────────────────
        protected void btn_set_tana_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddl_resp.SelectedValue))
            {
                ShowToast("กรุณาเลือก Responsible ก่อนกด Set Tanaoroshi", "Required Field", "warn");
                return;
            }
            Set_tana();
        }

        protected void Set_tana()
        {
            try
            {
                string monthTana = ddl_month.SelectedValue;
                string picRsp = ddl_resp.SelectedValue;

                // ★ ดึง key ที่ set ไปแล้วในรอบเดือนนี้ มาไว้เช็คกันซ้ำ
                HashSet<string> existingKeys = Get_existing_tana_keys(monthTana);
                int skippedDup = 0;   // ★ นับรายการที่ข้ามเพราะซ้ำ

                DataTable tanaList = Create_tana_list();
                DataTable tanaHis = Create_tana_his();

                for (int i = 0; i < gv_list.Rows.Count; i++)
                {
                    GridViewRow row = gv_list.Rows[i];
                    CheckBox chk = row.FindControl("chk_row") as CheckBox;
                    if (chk == null || !chk.Checked) continue;

                    string rowDeptCode = GetHidden(row, "hd_dept_code");
                    string model = GetHidden(row, "hd_model");
                    string location = GetHidden(row, "hd_location");
                    string cell = GetHidden(row, "hd_cell");
                    string station = GetHidden(row, "hd_station");
                    string toolId = GetHidden(row, "hd_tool_id");
                    string sysStatus = GetHidden(row, "hd_sys_status");
                    string toolSubNo = GetHidden(row, "hd_tool_sub_no");
                    string rowPicRsp = string.IsNullOrEmpty(picRsp) ? GetHidden(row, "hd_rsp_empid") : picRsp;

                    // ★ ถ้า identity นี้ถูก set ไปแล้ว → ข้าม ไม่ insert ซ้ำ
                    string key = Make_tana_key(toolId, toolSubNo, sysStatus);
                    if (existingKeys.Contains(key)) { skippedDup++; continue; }

                    // ★ กันซ้ำภายใน batch เดียวกันด้วย (แถวซ้ำใน gridview)
                    existingKeys.Add(key);

                    string assetNo = GetCell(row, "lb_assetNo");
                    string subAsset = GetCell(row, "lb_sub");
                    string controlNo = GetCell(row, "lb_ctrl");
                    string toolName = GetHidden(row, "hd_asset_name");

                    DataRow drList = tanaList.NewRow();
                    drList["tana_date"] = monthTana;
                    drList["pic_rsp"] = rowPicRsp;
                    drList["div_id"] = div_id;
                    drList["dept_id"] = dept_id;
                    drList["dept_code"] = rowDeptCode;
                    drList["tana_status"] = "1";
                    drList["pic_set_tana"] = emp_id;
                    drList["cell_id"] = cell;
                    drList["model_id"] = rowDeptCode == DEPT_AAD ? "0" : model;
                    drList["location_id"] = location;
                    tanaList.Rows.Add(drList);

                    DataRow drHis = tanaHis.NewRow();
                    drHis["tana_date"] = monthTana;
                    drHis["pic_rsp"] = rowPicRsp;
                    drHis["dept_code"] = rowDeptCode;
                    drHis["asset_no"] = assetNo;
                    drHis["sub_asset_no"] = subAsset;
                    drHis["control_no"] = controlNo;
                    drHis["name"] = toolName;
                    drHis["model_id"] = model;
                    drHis["location_id"] = location;
                    drHis["cell_id"] = cell;
                    drHis["station_id"] = station;
                    drHis["tool_id"] = toolId;
                    drHis["sys_status"] = sysStatus;
                    drHis["tool_sub_no"] = toolSubNo;
                    tanaHis.Rows.Add(drHis);
                }

                // ★ ไม่มีรายการใหม่เลย (ทั้งหมดซ้ำ หรือไม่ได้เลือก)
                if (tanaList.Rows.Count == 0)
                {
                    if (skippedDup > 0)
                        ShowToast("ทุกรายการที่เลือกถูก Set Tana ไปแล้ว (" + skippedDup + " รายการ)",
                                  "ไม่มีรายการใหม่", "warn");
                    else
                        ShowToast("Please select at least one item", "No Selection", "warn");
                    return;
                }

                DataTable listNormal = tanaList.AsEnumerable()
                    .Where(r => r["dept_code"].ToString() != DEPT_AAD).CopyToDataTable_Safe();
                DataTable listAAD = tanaList.AsEnumerable()
                    .Where(r => r["dept_code"].ToString() == DEPT_AAD).CopyToDataTable_Safe();

                if (listNormal != null && listNormal.Rows.Count > 0)
                    listNormal = RemoveDuplicateRows(listNormal,
                        new[] { "tana_date", "dept_id", "dept_code", "model_id", "cell_id" });

                if (listAAD != null && listAAD.Rows.Count > 0)
                    listAAD = RemoveDuplicateRows(listAAD, new[] { "tana_date", "cell_id" });

                DataTable tanaListFinal = Create_tana_list();
                if (listNormal != null) foreach (DataRow r in listNormal.Rows) tanaListFinal.ImportRow(r);
                if (listAAD != null) foreach (DataRow r in listAAD.Rows) tanaListFinal.ImportRow(r);

                Insert_into(tanaListFinal, tanaHis, skippedDup);   // ★ ส่ง skippedDup ต่อ
            }
            catch (Exception ex) { Alert_message(ex.Message); }
        }

        // ═══════════════════════════════════════════════════════════
        //  ดึง identity ที่ถูก set ไปแล้วในรอบเดือนนี้ (กันซ้ำ)
        //  key = tool_id | tool_sub_no | sys_status
        // ═══════════════════════════════════════════════════════════
        private HashSet<string> Get_existing_tana_keys(string monthTana)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string sql = @"SELECT DISTINCT tool_id, tool_sub_no, sys_status
                   FROM   Equipment_tana_history
                   WHERE  tana_date = @dateTana";

            datacon dc2 = new datacon();
            dc2.QryDT_P(conn, sql, new SqlParameter("@dateTana", monthTana));

            foreach (DataRow r in dc2.dt.Rows)
                set.Add(Make_tana_key(r["tool_id"], r["tool_sub_no"], r["sys_status"]));

            return set;
        }

        private static string Make_tana_key(object toolId, object toolSub, object sysStatus)
        {
            return (toolId ?? "") + "|" + (toolSub ?? "") + "|" + (sysStatus ?? "");
        }

        private static DataTable RemoveDuplicateRows(DataTable original, string[] keyColumns)
        {
            return original.AsEnumerable()
                .GroupBy(row => string.Join("|", keyColumns.Select(col => row[col].ToString())))
                .Select(g => g.First())
                .CopyToDataTable();
        }

        private const int SQL_INSERT_BATCH_SIZE = 900;

        // ─── Insert into DB (มี transaction ครอบ) ─────────────────
        // ★ เพิ่ม parameter skippedDup (default 0 กันโค้ดอื่นที่เรียกอยู่พัง)
        private void Insert_into(DataTable tbList, DataTable tbHis, int skippedDup = 0)
        {
            if (tbList.Rows.Count == 0) return;

            // STEP 1 : insert header → รับ id
            DataTable idMap = InsertHeaderAndGetIds(tbList);

            // STEP 2 : lookup map ตาม group key
            var lookup = new Dictionary<string, int>();
            foreach (DataRow r in idMap.Rows)
            {
                string deptCode = r["dept_code"].ToString();
                string key = deptCode == DEPT_AAD
                    ? string.Join("|", r["tana_date"], r["cell_id"])
                    : string.Join("|", r["tana_date"], deptCode, r["model_id"], r["cell_id"]);
                lookup[key] = Convert.ToInt32(r["id"]);
            }

            // STEP 3 : ใส่ list_id ให้ history
            foreach (DataRow r in tbHis.Rows)
            {
                string deptCode = r["dept_code"].ToString();
                string key = deptCode == DEPT_AAD
                    ? string.Join("|", r["tana_date"], r["cell_id"])
                    : string.Join("|", r["tana_date"], deptCode, r["model_id"], r["cell_id"]);
                r["list_id"] = lookup.ContainsKey(key) ? (object)lookup[key] : DBNull.Value;
            }

            // STEP 4 : insert history (batched + transaction + escape ')
            if (tbHis.Rows.Count > 0)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append(" BEGIN TRY BEGIN TRANSACTION; ");
                AppendBatchedInsert(sb, tbHis,
                    " INSERT INTO [dbo].[Equipment_tana_history]" +
                    "([tana_date],[pic_rsp],[asset_no],[sub_asset_no],[control_no],[name]," +
                    "[model_id],[location_id],[cell_id],[station_id],[dept_code],[tool_id],[sys_status],[tool_sub_no],[list_id]) VALUES ",
                    r => "('" + Esc(r["tana_date"]) + "','" + Esc(r["pic_rsp"]) + "','" + Esc(r["asset_no"]) + "','" +
                         Esc(r["sub_asset_no"]) + "','" + Esc(r["control_no"]) + "','" + Esc(r["name"]) + "','" +
                         Esc(r["model_id"]) + "','" + Esc(r["location_id"]) + "','" + Esc(r["cell_id"]) + "','" +
                         Esc(r["station_id"]) + "','" + Esc(r["dept_code"]) + "','" + Esc(r["tool_id"]) + "','" +
                         Esc(r["sys_status"]) + "','" + Esc(r["tool_sub_no"]) + "'," +
                         (r["list_id"] == DBNull.Value ? "NULL" : "'" + Esc(r["list_id"]) + "'") + ")");
                sb.Append(" COMMIT; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH ");

                dc.RunCommand_P(conn, sb.ToString());
            }

            BindGvList();
            Load_tana_done();

            // ★ toast สรุปผล : insert สำเร็จ + ข้ามซ้ำกี่รายการ
            string msg = "Set Tanaoroshi สำเร็จ " + tbHis.Rows.Count + " รายการ";
            if (skippedDup > 0) msg += " (ข้ามที่ Set ไว้แล้ว " + skippedDup + " รายการ)";
            ShowToast(msg, "Completed", "success");
        }

        // Insert header ผ่าน SP (escape ' กัน error)
        private DataTable InsertHeaderAndGetIds(DataTable tbList)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(" DECLARE @tanaList AS dbo.Tana_ListType ");
            AppendBatchedInsert(sb, tbList,
                " INSERT INTO @tanaList ([tana_date],[div_id],[dept_id],[pic_rsp]," +
                "[model_id],[location_id],[tana_status],[pic_set_tana],[dept_code],[cell_id]) VALUES ",
                r => "('" + Esc(r["tana_date"]) + "','" + Esc(r["div_id"]) + "','" + Esc(r["dept_id"]) + "','" +
                     Esc(r["pic_rsp"]) + "','" + Esc(r["model_id"]) + "','" + Esc(r["location_id"]) + "','" +
                     Esc(r["tana_status"]) + "','" + Esc(r["pic_set_tana"]) + "','" +
                     Esc(r["dept_code"]) + "','" + Esc(r["cell_id"]) + "')");
            sb.Append(" EXEC sp_InsertTanaList @tanaList ");

            dc.QryDT_P(conn, sb.ToString());
            return dc.dt;
        }

        private void AppendBatchedInsert(System.Text.StringBuilder sb, DataTable table,
            string insertHeader, Func<DataRow, string> rowValueBuilder)
        {
            int total = table.Rows.Count;
            for (int start = 0; start < total; start += SQL_INSERT_BATCH_SIZE)
            {
                int end = Math.Min(start + SQL_INSERT_BATCH_SIZE, total);
                sb.Append(insertHeader);
                for (int i = start; i < end; i++)
                {
                    sb.Append(rowValueBuilder(table.Rows[i]));
                    if (i < end - 1) sb.Append(",");
                }
                sb.Append(" ");
            }
        }

        // ─── Helpers ──────────────────────────────────────────────
        private static string Esc(object o)
        {
            string s = (o == null) ? "" : o.ToString();
            return s.Replace("'", "''");
        }
        private int SafeInt(string s) { int v; return int.TryParse(s, out v) ? v : -1; }

        private string GetHidden(GridViewRow row, string id)
        {
            var hf = row.FindControl(id) as HiddenField;
            return hf != null ? hf.Value : "";
        }
        private string GetCell(GridViewRow row, string id)
        {
            var hf = row.FindControl(id) as HiddenField;
            if (hf != null) return hf.Value;
            var lb = row.FindControl(id) as Label;
            return lb != null ? lb.Text : "";
        }

        private DataTable Create_tana_list()
        {
            var dt = new DataTable();
            dt.Columns.Add("tana_date", typeof(string));
            dt.Columns.Add("div_id", typeof(string));
            dt.Columns.Add("dept_id", typeof(string));
            dt.Columns.Add("dept_code", typeof(string));
            dt.Columns.Add("pic_rsp", typeof(string));
            dt.Columns.Add("model_id", typeof(string));
            dt.Columns.Add("location_id", typeof(string));
            dt.Columns.Add("cell_id", typeof(string));
            dt.Columns.Add("tana_status", typeof(string));
            dt.Columns.Add("pic_set_tana", typeof(string));
            dt.Columns.Add("list_id", typeof(string));
            return dt;
        }

        private DataTable Create_tana_his()
        {
            var dt = new DataTable();
            dt.Columns.Add("tana_date", typeof(string));
            dt.Columns.Add("pic_rsp", typeof(string));
            dt.Columns.Add("dept_code", typeof(string));
            dt.Columns.Add("asset_no", typeof(string));
            dt.Columns.Add("sub_asset_no", typeof(string));
            dt.Columns.Add("control_no", typeof(string));
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("model_id", typeof(string));
            dt.Columns.Add("location_id", typeof(string));
            dt.Columns.Add("cell_id", typeof(string));
            dt.Columns.Add("station_id", typeof(string));
            dt.Columns.Add("tool_id", typeof(string));
            dt.Columns.Add("sys_status", typeof(string));
            dt.Columns.Add("tool_sub_no", typeof(string));
            dt.Columns.Add("list_id", typeof(string));
            return dt;
        }

        // ─── Alerts ───────────────────────────────────────────────
        private void ShowToast(string detail, string title, string type, int duration = 3000)
        {
            string script = string.Format(
                "showToast('{0}',{{title:'{1}',type:'{2}',duration:{3}}});",
                HttpUtility.JavaScriptStringEncode(detail ?? ""),
                HttpUtility.JavaScriptStringEncode(title ?? ""),
                HttpUtility.JavaScriptStringEncode(type ?? "info"),
                duration);
            ScriptManager.RegisterStartupScript(this, this.GetType(),
                "toast_" + DateTime.UtcNow.Ticks, script, true);
        }

        private void Alert_message(string message)
        {
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert_msg",
                "alert('" + HttpUtility.JavaScriptStringEncode(message) + "');", true);
        }

        // ─── Fill Model / Cell dropdown ───────────────────────────
        private void Set_ddl_model_cell()
        {
            string sqlM = @"SELECT DISTINCT V.model_id, V.model_name
                    FROM Tool_Equipment_List_View V
                    WHERE ((LEFT(V.FixAcces,3)='BFM') OR (V.control_no IS NOT NULL))
                      AND V.location1_id = " + FIX_LOCATION1_ID + @"
                      AND V.model_name IS NOT NULL
                    ORDER BY V.model_name";
            dc.QryDT_P(conn, sqlM);
            ddl_model.Items.Clear();
            ddl_model.Items.Add(new ListItem("-- All --", ""));
            foreach (DataRow r in dc.dt.Rows)
                ddl_model.Items.Add(new ListItem(r["model_name"].ToString(), r["model_id"].ToString()));

            string sqlC = @"SELECT DISTINCT V.location2_id, V.cell
                    FROM Tool_Equipment_List_View V
                    WHERE ((LEFT(V.FixAcces,3)='BFM') OR (V.control_no IS NOT NULL))
                      AND V.location1_id = " + FIX_LOCATION1_ID + @"
                      AND V.cell IS NOT NULL
                    ORDER BY V.cell";
            dc.QryDT_P(conn, sqlC);
            ddl_cell.Items.Clear();
            ddl_cell.Items.Add(new ListItem("-- All --", ""));
            foreach (DataRow r in dc.dt.Rows)
                ddl_cell.Items.Add(new ListItem(r["cell"].ToString(), r["location2_id"].ToString()));
        }
    }

    // ─── Extension ────────────────────────────────────────────────
    internal static class DataTableExtensions
    {
        public static DataTable CopyToDataTable_Safe(
            this System.Collections.Generic.IEnumerable<DataRow> source)
        {
            if (source == null || !source.Any()) return null;
            return source.CopyToDataTable();
        }
    }
}
