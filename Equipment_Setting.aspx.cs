using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tool_Equipment_Control_System
{
    /// <summary>
    /// System Setup : จัดการเงื่อนไขของระบบ
    /// Tab 1 : Auto Assign — กฎเลือก Status Use / ผู้รับผิดชอบ อัตโนมัติ
    ///         Withdraw : จาก Dept / Cell / Station            (sp_AutoAssign_Resolve : SQL/Auto_Assign_Rule.sql)
    ///         Storage  : จาก Status / OK-NG / Repair / เหตุผล / Cell / Station
    ///                    (sp_AutoAssign_Resolve_Storage : SQL/Auto_Assign_Rule_Storage.sql)
    /// </summary>
    public partial class Equipment_Setting : System.Web.UI.Page
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString;

        // ── รายการผู้รับผิดชอบของกฎที่กำลังแก้ไข (เรียงตามลำดับ : ตัวแรก = Auto) ──
        private List<string> EditResp
        {
            get { return ViewState["EDIT_RESP"] as List<string> ?? new List<string>(); }
            set { ViewState["EDIT_RESP"] = value; }
        }
        private int EditRuleId
        {
            get { return ViewState["EDIT_RULE_ID"] == null ? 0 : (int)ViewState["EDIT_RULE_ID"]; }
            set { ViewState["EDIT_RULE_ID"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = "System Setup";

            if (Session["emp_id"] == null || Session["emp_id"].ToString() == "")
                Response.Redirect("Default.aspx");

            // สิทธิ์ : Admin (99) + Equipment_Admin.level_4 (เหมือนเมนู System Setup)
            bool allowed = IsSuperAdmin();
            pnlDenied.Visible = !allowed;
            pnlMain.Visible = allowed;
            if (!allowed) return;

            if (!IsPostBack)
            {
                LoadMasters();
                BindMasterControls();
                BindRules();
                BindHistory();
            }
        }

        #region --- Permission / Data helpers ---

        private string EmpId { get { return Session["emp_id"] != null ? Session["emp_id"].ToString().Trim() : ""; } }

        private bool IsSuperAdmin()
        {
            if (Session["admin"] == null || Session["admin"].ToString().Trim() != "99") return false;
            DataTable dt = Query("SELECT level_4 FROM Equipment_Admin WHERE LTRIM(RTRIM(emp_id)) = @emp AND status_use = 1",
                                 P("@emp", EmpId));
            return dt.Rows.Count > 0 && dt.Rows[0]["level_4"] != DBNull.Value && Convert.ToBoolean(dt.Rows[0]["level_4"]);
        }

        private static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        private DataTable Query(string sql, params SqlParameter[] ps)
        {
            var dt = new DataTable();
            using (var c = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(sql, c))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
            }
            return dt;
        }

        private static string S(object o) { return o == null || o == DBNull.Value ? "" : o.ToString().Trim(); }
        private static string H(string s) { return HttpUtility.HtmlEncode(s ?? ""); }

        #endregion

        #region --- Masters (Dept / Cell / Station / Status / ผู้รับผิดชอบ) ---

        private DataTable DeptM { get { return (DataTable)ViewState["M_DEPT"]; } }
        private DataTable CellM { get { return (DataTable)ViewState["M_CELL"]; } }
        private DataTable StationM { get { return (DataTable)ViewState["M_STATION"]; } }
        private DataTable StatusM { get { return (DataTable)ViewState["M_STATUS"]; } }
        private DataTable RespM { get { return (DataTable)ViewState["M_RESP"]; } }
        private DataTable ReasonM { get { return (DataTable)ViewState["M_REASON"]; } }

        // ── ค่าคงที่ของ Storage (ตรงกับ Dropdown ใน storage.aspx Step 3) ──
        private static readonly string[] ST_ITEMS = { "Spare", "Not Use" };
        private static readonly string[] OKNG_ITEMS = { "OK", "NG" };
        private static readonly string[] REPAIR_ITEMS = { "ซ่อม", "ไม่ซ่อม/ซ่อมไม่ได้" };

        private static DataTable FixedM(string[] items)
        {
            var dt = new DataTable();
            dt.Columns.Add("id"); dt.Columns.Add("name");
            foreach (string s in items) dt.Rows.Add(s, s);
            return dt;
        }

        private void LoadMasters()
        {
            ViewState["M_DEPT"] = Query(
                "SELECT LTRIM(RTRIM(dept_code)) AS id, LTRIM(RTRIM(dept_code)) + ' : ' + ISNULL(dept_name2, '') AS name" +
                " FROM View_dept_master WHERE division = 2 ORDER BY dept_code");

            ViewState["M_CELL"] = Query(
                "SELECT CAST(location2_id AS nvarchar(50)) AS id, LTRIM(RTRIM(location2_name)) AS name" +
                " FROM jig_tool_system.dbo.master_location2" +
                " WHERE is_use = 1 AND location1_id = 97 AND location2_id != 0 ORDER BY location2_name");

            ViewState["M_STATION"] = Query(
                "SELECT CAST(location3_id AS nvarchar(50)) AS id, LTRIM(RTRIM(location3_name)) AS name" +
                " FROM jig_tool_system.dbo.master_location3" +
                " WHERE location1_id = 97 AND location3_id != 0 ORDER BY location3_name");

            ViewState["M_REASON"] = Query(
                "SELECT CAST(reason_ID AS nvarchar(20)) AS id, LTRIM(RTRIM(reason_Name)) AS name" +
                " FROM Reason_Borrow_Master WHERE storageFlow = 'true' ORDER BY order_storage");

            ViewState["M_STATUS"] = Query(
                "SELECT CAST(status_id AS nvarchar(20)) AS id, LTRIM(RTRIM(status_name)) AS name" +
                " FROM tool_equipment_status ORDER BY status_id");

            // ผู้รับผิดชอบ 1 คน 1 แถว (คนเดียวอาจอยู่หลาย Dept → รวม Dept ไว้แสดง)
            DataTable raw = Query(
                "SELECT LTRIM(RTRIM(emp_id)) AS emp_id, LTRIM(RTRIM(ISNULL(pic_dept, ''))) AS pic_dept," +
                "       LTRIM(RTRIM(CAST(dept_code AS nvarchar(20)))) AS dept_code" +
                " FROM Responsible_Dept WHERE emp_id IS NOT NULL AND LTRIM(RTRIM(emp_id)) <> ''" +
                " ORDER BY pic_dept, emp_id");

            var resp = new DataTable();
            resp.Columns.Add("id"); resp.Columns.Add("name"); resp.Columns.Add("label"); resp.Columns.Add("depts");
            foreach (var g in raw.Rows.Cast<DataRow>().GroupBy(r => S(r["emp_id"])))
            {
                string name = g.Select(r => S(r["pic_dept"])).FirstOrDefault(n => n != "") ?? "";
                string depts = string.Join(", ", g.Select(r => S(r["dept_code"])).Where(d => d != "").Distinct().ToArray());
                resp.Rows.Add(g.Key, name, (name == "" ? g.Key : name + " (" + g.Key + ")") + (depts == "" ? "" : " · " + depts), depts);
            }
            ViewState["M_RESP"] = resp;
        }

        private string NameOf(DataTable m, string id)
        {
            if (m == null) return id;
            foreach (DataRow r in m.Rows) if (S(r["id"]) == id) return S(r["name"]);
            return id;
        }

        private string RespLabel(string empId)
        {
            foreach (DataRow r in RespM.Rows)
                if (S(r["id"]) == empId)
                    return S(r["name"]) == "" ? empId : S(r["name"]) + " (" + empId + ")";
            return empId;
        }

        private static void FillList(ListControl c, DataTable m, string textCol, string firstText)
        {
            c.Items.Clear();
            if (firstText != null) c.Items.Add(new ListItem(firstText, ""));
            foreach (DataRow r in m.Rows) c.Items.Add(new ListItem(S(r[textCol]), S(r["id"])));
        }

        private void BindMasterControls()
        {
            // Modal กฎ
            FillList(lbDept, DeptM, "name", null);
            FillList(lbCell, CellM, "name", null);
            FillList(lbStation, StationM, "name", null);
            FillList(lbStItem, FixedM(ST_ITEMS), "name", null);
            FillList(lbOkNg, FixedM(OKNG_ITEMS), "name", null);
            FillList(lbRepair, FixedM(REPAIR_ITEMS), "name", null);
            FillList(lbReason, ReasonM, "name", null);
            FillList(ddlStatusUse, StatusM, "name", "— ไม่กำหนด (ใช้ค่าเดิมของระบบ) —");
            FillList(ddlRespAdd, RespM, "label", "-- เลือกผู้รับผิดชอบ --");

            // กล่องทดสอบ
            FillList(ddlTestDept, DeptM, "name", "-- Dept --");
            FillList(ddlTestCell, CellM, "name", "-- Cell (ไม่ระบุ) --");
            FillList(ddlTestStation, StationM, "name", "-- Station (ไม่ระบุ) --");
            FillList(ddlTestStItem, FixedM(ST_ITEMS), "name", "-- Status (ไม่ระบุ) --");
            FillList(ddlTestOkNg, FixedM(OKNG_ITEMS), "name", "-- OK/NG (ไม่ระบุ) --");
            FillList(ddlTestRepair, FixedM(REPAIR_ITEMS), "name", "-- Repair (ไม่ระบุ) --");
            FillList(ddlTestReason, ReasonM, "name", "-- เหตุผล (ไม่ระบุ) --");
        }

        #endregion

        #region --- รายการกฎ ---

        private class RuleInfo
        {
            public int Id; public string Name; public string StatusId; public string LocLogic; public string ApplyTo = "WITHDRAW";
            public int Priority; public bool Active; public string Remark; public string UpdateBy; public string UpdateDate;
            public List<string> Depts = new List<string>();
            public List<string> Cells = new List<string>();
            public List<string> Stations = new List<string>();
            public List<string> Resps = new List<string>();
            // Storage
            public List<string> StItems = new List<string>();
            public List<string> OkNgs = new List<string>();
            public List<string> Repairs = new List<string>();
            public List<string> Reasons = new List<string>();
            public bool IsStorage { get { return ApplyTo == "STORAGE"; } }
        }

        private List<RuleInfo> LoadRuleInfos(int onlyRuleId)
        {
            string w = onlyRuleId > 0 ? " WHERE rule_id = @id" : "";
            var p = onlyRuleId > 0 ? new[] { P("@id", onlyRuleId) } : new SqlParameter[0];

            DataTable dr = Query("SELECT * FROM Auto_Assign_Rule" + w + " ORDER BY is_active DESC, priority DESC, rule_id", p.Select(x => new SqlParameter(x.ParameterName, x.Value)).ToArray());
            DataTable dc = Query("SELECT rule_id, cond_type, cond_value FROM Auto_Assign_Rule_Cond" + w + " ORDER BY cond_id", p.Select(x => new SqlParameter(x.ParameterName, x.Value)).ToArray());
            DataTable dp = Query("SELECT rule_id, emp_id FROM Auto_Assign_Rule_Resp" + w + " ORDER BY rule_id, seq", p.Select(x => new SqlParameter(x.ParameterName, x.Value)).ToArray());

            var map = new Dictionary<int, RuleInfo>();
            var list = new List<RuleInfo>();
            foreach (DataRow r in dr.Rows)
            {
                var ri = new RuleInfo
                {
                    Id = Convert.ToInt32(r["rule_id"]),
                    Name = S(r["rule_name"]),
                    StatusId = S(r["status_use_id"]),
                    LocLogic = S(r["loc_logic"]) == "AND" ? "AND" : "OR",
                    ApplyTo = dr.Columns.Contains("apply_to") && S(r["apply_to"]) == "STORAGE" ? "STORAGE" : "WITHDRAW",
                    Priority = r["priority"] == DBNull.Value ? 0 : Convert.ToInt32(r["priority"]),
                    Active = r["is_active"] != DBNull.Value && Convert.ToBoolean(r["is_active"]),
                    Remark = S(r["remark"]),
                    UpdateBy = S(r["update_by"]) != "" ? S(r["update_by"]) : S(r["create_by"]),
                    UpdateDate = r["update_date"] != DBNull.Value ? Convert.ToDateTime(r["update_date"]).ToString("dd-MMM-yyyy HH:mm")
                               : (r["create_date"] != DBNull.Value ? Convert.ToDateTime(r["create_date"]).ToString("dd-MMM-yyyy HH:mm") : "")
                };
                map[ri.Id] = ri; list.Add(ri);
            }
            foreach (DataRow r in dc.Rows)
            {
                RuleInfo ri; if (!map.TryGetValue(Convert.ToInt32(r["rule_id"]), out ri)) continue;
                string t = S(r["cond_type"]), v = S(r["cond_value"]);
                if (t == "DEPT") ri.Depts.Add(v); else if (t == "CELL") ri.Cells.Add(v); else if (t == "STATION") ri.Stations.Add(v);
                else if (t == "STATUS_ITEM") ri.StItems.Add(v); else if (t == "OKNG") ri.OkNgs.Add(v);
                else if (t == "REPAIR") ri.Repairs.Add(v); else if (t == "REASON") ri.Reasons.Add(v);
            }
            foreach (DataRow r in dp.Rows)
            {
                RuleInfo ri; if (!map.TryGetValue(Convert.ToInt32(r["rule_id"]), out ri)) continue;
                ri.Resps.Add(S(r["emp_id"]));
            }
            return list;
        }

        private static int Score(RuleInfo r)
        {
            if (r.IsStorage) return StorageScore(r);
            return (r.Depts.Count > 0 ? 1 : 0) + (r.Cells.Count + r.Stations.Count > 0 ? 2 : 0);
        }

        /// <summary>Storage : จำนวนชนิดเงื่อนไข (ตรงกับ score ใน sp_AutoAssign_Resolve_Storage)</summary>
        private static int StorageScore(RuleInfo r)
        {
            return (r.StItems.Count > 0 ? 1 : 0) + (r.OkNgs.Count > 0 ? 1 : 0) + (r.Repairs.Count > 0 ? 1 : 0) +
                   (r.Reasons.Count > 0 ? 1 : 0) + (r.Cells.Count + r.Stations.Count > 0 ? 1 : 0);
        }

        private static string ScoreText(RuleInfo r)
        {
            return r.IsStorage ? StorageScore(r) + " เงื่อนไข" : ScoreText(Score(r));
        }

        private static string ScoreText(int s)
        {
            switch (s) { case 3: return "Dept + Location"; case 2: return "Location"; case 1: return "Dept"; default: return "-"; }
        }

        private string Chips(IEnumerable<string> ids, DataTable m, string cls, bool showId)
        {
            var sb = new StringBuilder();
            foreach (string id in ids)
            {
                string nm = NameOf(m, id);
                string text = showId ? id : nm;
                sb.Append("<span class='st-chip " + cls + "' title='" + H(nm) + "'>" + H(text) + "</span>");
            }
            return sb.ToString();
        }

        /// <summary>เงื่อนไขแบบอ่านง่าย : Dept [..] AND (Cell [..] OR Station [..])</summary>
        private string ConditionHtml(RuleInfo r)
        {
            var parts = new List<string>();
            if (r.Depts.Count > 0)
                parts.Add("<span class='st-k'>Dept</span>" + Chips(r.Depts, DeptM, "chip-dept", true));

            var loc = new List<string>();
            if (r.Cells.Count > 0) loc.Add("<span class='st-k'>Cell</span>" + Chips(r.Cells, CellM, "chip-cell", false));
            if (r.Stations.Count > 0) loc.Add("<span class='st-k'>Station</span>" + Chips(r.Stations, StationM, "chip-sta", false));

            if (r.StItems.Count > 0) parts.Add("<span class='st-k'>Status</span>" + Chips(r.StItems, FixedM(ST_ITEMS), "chip-st", false));
            if (r.OkNgs.Count > 0) parts.Add("<span class='st-k'>OK/NG</span>" + Chips(r.OkNgs, FixedM(OKNG_ITEMS), "chip-okng", false));
            if (r.Repairs.Count > 0) parts.Add("<span class='st-k'>Repair</span>" + Chips(r.Repairs, FixedM(REPAIR_ITEMS), "chip-rp", false));
            if (r.Reasons.Count > 0) parts.Add("<span class='st-k'>เหตุผล</span>" + Chips(r.Reasons, ReasonM, "chip-rs", false));

            if (loc.Count == 1) parts.Add(loc[0]);
            else if (loc.Count == 2)
                parts.Add("<span class='st-paren'>( " + loc[0] + " <span class='st-op'>" + r.LocLogic + "</span> " + loc[1] + " )</span>");

            return string.Join(" <span class='st-op'>AND</span> ", parts.ToArray());
        }

        private string RespHtml(List<string> resps)
        {
            if (resps.Count == 0) return "<span class='st-muted'>— ไม่กำหนด (ใช้รายชื่อตาม Dept) —</span>";
            var sb = new StringBuilder("<ol class='st-resp'>");
            for (int i = 0; i < resps.Count; i++)
                sb.Append("<li" + (i == 0 ? " class='is-auto' title='เลือกอัตโนมัติ'" : "") + ">" +
                          (i == 0 ? "⭐ " : "") + H(RespLabel(resps[i])) + "</li>");
            sb.Append("</ol>");
            return sb.ToString();
        }

        private void BindRules()
        {
            List<RuleInfo> rules = LoadRuleInfos(0);

            // ── ตัวกรอง : ชื่อกฎ / หมายเหตุ / Dept / ผู้รับผิดชอบ / ชื่อ Cell-Station ──
            string kw = txtFilter.Text.Trim().ToLower();
            bool showInactive = chkShowInactive.Checked;
            string applyFilter = ddlFilterApply.SelectedValue;   // "" = ทั้งหมด

            var dt = new DataTable();
            foreach (string c in new[] { "rule_id", "rule_name", "remark", "cond_html", "status_name", "resp_html",
                                         "score_text", "priority", "update_info", "row_css", "active_text", "active_css", "apply_html" })
                dt.Columns.Add(c);

            foreach (RuleInfo r in rules)
            {
                if (!showInactive && !r.Active) continue;
                if (applyFilter != "" && r.ApplyTo != applyFilter) continue;
                if (kw != "")
                {
                    string hay = (r.Name + " " + r.Remark + " " + string.Join(" ", r.Depts.ToArray()) + " " +
                                  string.Join(" ", r.Resps.ToArray()) + " " + string.Join(" ", r.Resps.Select(RespLabel).ToArray()) + " " +
                                  string.Join(" ", r.Cells.Select(x => NameOf(CellM, x)).ToArray()) + " " +
                                  string.Join(" ", r.Stations.Select(x => NameOf(StationM, x)).ToArray()) + " " +
                                  string.Join(" ", r.StItems.Concat(r.OkNgs).Concat(r.Repairs).ToArray()) + " " +
                                  string.Join(" ", r.Reasons.Select(x => NameOf(ReasonM, x)).ToArray()) + " " +
                                  (r.IsStorage ? "storage" : "withdraw")).ToLower();
                    if (!hay.Contains(kw)) continue;
                }

                dt.Rows.Add(
                    r.Id, r.Name, r.Remark, ConditionHtml(r),
                    r.StatusId == "" ? "" : NameOf(StatusM, r.StatusId),
                    RespHtml(r.Resps),
                    ScoreText(r), r.Priority,
                    (r.UpdateBy == "" ? "" : r.UpdateBy + " · ") + r.UpdateDate,
                    r.Active ? "" : "st-row-off",
                    r.Active ? "ใช้งาน" : "ปิด",
                    r.Active ? "st-toggle on" : "st-toggle",
                    r.IsStorage ? "<span class='st-apply st-apply-s'>Storage</span>" : "<span class='st-apply st-apply-w'>Withdraw</span>");
            }

            gvRules.DataSource = dt;
            gvRules.DataBind();
            lblRuleCount.Text = dt.Rows.Count.ToString();
        }

        protected void gvRules_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            DataRowView drv = (DataRowView)e.Row.DataItem;
            string css = S(drv["row_css"]);
            if (css != "") e.Row.CssClass = css;
        }

        protected void btnFilter_Click(object sender, EventArgs e) { BindRules(); }
        protected void chkShowInactive_CheckedChanged(object sender, EventArgs e) { BindRules(); }
        protected void ddlFilterApply_SelectedIndexChanged(object sender, EventArgs e) { BindRules(); }

        protected void gvRules_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;

            switch (e.CommandName)
            {
                case "EditRule": OpenEditor(id); break;
                case "ToggleRule": ToggleRule(id); break;
                case "DeleteRule": DeleteRule(id); break;
            }
        }

        #endregion

        #region --- Modal : เพิ่ม / แก้ไขกฎ ---

        protected void btnAddRule_Click(object sender, EventArgs e) { OpenEditor(0); }

        private void OpenEditor(int ruleId)
        {
            EditRuleId = ruleId;
            lblEditorTitle.Text = ruleId == 0 ? "เพิ่มกฎ Auto Assign" : "แก้ไขกฎ #" + ruleId;

            txtRuleName.Text = ""; txtRemark.Text = ""; txtPriority.Text = "0";
            ddlStatusUse.ClearSelection(); rblLocLogic.SelectedValue = "OR"; chkActive.Checked = true;
            foreach (ListItem li in lbDept.Items) li.Selected = false;
            foreach (ListItem li in lbCell.Items) li.Selected = false;
            foreach (ListItem li in lbStation.Items) li.Selected = false;
            foreach (ListBox lb in new[] { lbStItem, lbOkNg, lbRepair, lbReason })
                foreach (ListItem li in lb.Items) li.Selected = false;
            rblApplyTo.SelectedValue = "WITHDRAW";
            var resp = new List<string>();

            if (ruleId > 0)
            {
                RuleInfo r = LoadRuleInfos(ruleId).FirstOrDefault();
                if (r == null) { Toast("ไม่พบกฎ #" + ruleId + " (อาจถูกลบแล้ว)", "ไม่พบข้อมูล", "warn"); BindRules(); return; }

                txtRuleName.Text = r.Name; txtRemark.Text = r.Remark; txtPriority.Text = r.Priority.ToString();
                if (ddlStatusUse.Items.FindByValue(r.StatusId) != null) ddlStatusUse.SelectedValue = r.StatusId;
                rblLocLogic.SelectedValue = r.LocLogic; chkActive.Checked = r.Active;
                SelectValues(lbDept, r.Depts); SelectValues(lbCell, r.Cells); SelectValues(lbStation, r.Stations);
                SelectValues(lbStItem, r.StItems); SelectValues(lbOkNg, r.OkNgs); SelectValues(lbRepair, r.Repairs); SelectValues(lbReason, r.Reasons);
                rblApplyTo.SelectedValue = r.ApplyTo;
                resp = r.Resps;
            }

            EditResp = resp;
            BindEditResp();
            litEditorError.Text = "";
            ShowEditor();
        }

        private static void SelectValues(ListBox lb, List<string> vals)
        {
            foreach (string v in vals)
            {
                ListItem li = lb.Items.FindByValue(v);
                if (li == null) { li = new ListItem(v + " (ไม่พบใน master)", v); lb.Items.Add(li); }
                li.Selected = true;
            }
        }

        private static List<string> SelectedValues(ListBox lb)
        {
            return lb.Items.Cast<ListItem>().Where(i => i.Selected && i.Value != "").Select(i => i.Value).Distinct().ToList();
        }

        private void BindEditResp()
        {
            var dt = new DataTable();
            dt.Columns.Add("emp_id"); dt.Columns.Add("label"); dt.Columns.Add("idx", typeof(int)); dt.Columns.Add("li_css");
            List<string> list = EditResp;
            for (int i = 0; i < list.Count; i++)
                dt.Rows.Add(list[i], RespLabel(list[i]), i, i == 0 ? "is-auto" : "");
            rptResp.DataSource = dt;
            rptResp.DataBind();
            litRespEmpty.Visible = list.Count == 0;
        }

        protected void btnRespAdd_Click(object sender, EventArgs e)
        {
            string id = ddlRespAdd.SelectedValue;
            List<string> list = EditResp;
            if (id != "" && !list.Contains(id)) list.Add(id);
            EditResp = list;
            ddlRespAdd.ClearSelection();
            BindEditResp();
            ShowEditor();
        }

        protected void rptResp_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int i;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out i)) { ShowEditor(); return; }
            List<string> list = EditResp;
            if (i < 0 || i >= list.Count) { ShowEditor(); return; }

            if (e.CommandName == "Up" && i > 0) { string t = list[i - 1]; list[i - 1] = list[i]; list[i] = t; }
            else if (e.CommandName == "Down" && i < list.Count - 1) { string t = list[i + 1]; list[i + 1] = list[i]; list[i] = t; }
            else if (e.CommandName == "Remove") list.RemoveAt(i);

            EditResp = list;
            BindEditResp();
            ShowEditor();
        }

        protected void btnEditorSave_Click(object sender, EventArgs e)
        {
            string name = txtRuleName.Text.Trim();
            string applyTo = rblApplyTo.SelectedValue == "STORAGE" ? "STORAGE" : "WITHDRAW";
            bool isStorage = applyTo == "STORAGE";
            List<string> depts = SelectedValues(lbDept), cells = SelectedValues(lbCell), stations = SelectedValues(lbStation);
            List<string> stItems = SelectedValues(lbStItem), okngs = SelectedValues(lbOkNg), repairs = SelectedValues(lbRepair), reasons = SelectedValues(lbReason);
            // เงื่อนไขที่ไม่ใช้กับประเภทนี้ → ไม่บันทึก (กันค่าค้างจากการสลับประเภท)
            if (isStorage) depts = new List<string>();
            else { stItems = new List<string>(); okngs = new List<string>(); repairs = new List<string>(); reasons = new List<string>(); }
            List<string> resps = EditResp;
            string statusId = ddlStatusUse.SelectedValue;
            string locLogic = rblLocLogic.SelectedValue == "AND" ? "AND" : "OR";
            int priority; if (!int.TryParse(txtPriority.Text.Trim(), out priority)) priority = 0;

            // ── ตรวจข้อมูล ──
            var errs = new List<string>();
            if (name == "") errs.Add("กรุณาตั้งชื่อกฎ");
            if (!isStorage && depts.Count + cells.Count + stations.Count == 0)
                errs.Add("ต้องมีเงื่อนไขอย่างน้อย 1 อย่าง (Dept / Cell / Station)");
            if (isStorage && stItems.Count + okngs.Count + repairs.Count + reasons.Count + cells.Count + stations.Count == 0)
                errs.Add("ต้องมีเงื่อนไขอย่างน้อย 1 อย่าง (Status / OK-NG / Repair / เหตุผล / Cell / Station)");
            if (statusId == "" && resps.Count == 0) errs.Add("ต้องกำหนดผลลัพธ์อย่างน้อย 1 อย่าง (Status Use หรือ ผู้รับผิดชอบ)");

            // กฎที่เงื่อนไขซ้ำกันทุกตัว + เปิดใช้งาน → ระบบเลือกไม่ได้ว่าจะใช้อันไหน
            if (chkActive.Checked && errs.Count == 0)
            {
                string sig = Signature(applyTo, depts, cells, stations, locLogic, stItems, okngs, repairs, reasons);
                RuleInfo dup = LoadRuleInfos(0).FirstOrDefault(r => r.Active && r.Id != EditRuleId && Signature(r) == sig);
                if (dup != null) errs.Add("เงื่อนไขซ้ำกับกฎ #" + dup.Id + " \"" + dup.Name + "\" ที่เปิดใช้งานอยู่ — แก้กฎเดิม หรือปิดกฎเดิมก่อน");
            }

            if (errs.Count > 0)
            {
                litEditorError.Text = "<div class='st-err'>" + string.Join("<br/>", errs.Select(H).ToArray()) + "</div>";
                ShowEditor();
                return;
            }

            string before = EditRuleId > 0 ? RuleSummary(EditRuleId) : "";
            int ruleId = EditRuleId;

            using (var c = new SqlConnection(connStr))
            {
                c.Open();
                using (SqlTransaction tx = c.BeginTransaction())
                {
                    try
                    {
                        if (ruleId == 0)
                        {
                            using (var cmd = new SqlCommand(
                                "INSERT INTO Auto_Assign_Rule (rule_name, status_use_id, loc_logic, priority, is_active, remark, create_by, create_date, apply_to)" +
                                " VALUES (@n, @st, @ll, @p, @a, @rm, @by, GETDATE(), @ap); SELECT CAST(SCOPE_IDENTITY() AS int);", c, tx))
                            {
                                AddRuleParams(cmd, name, statusId, locLogic, priority);
                                cmd.Parameters.AddWithValue("@ap", applyTo);
                                ruleId = (int)cmd.ExecuteScalar();
                            }
                        }
                        else
                        {
                            using (var cmd = new SqlCommand(
                                "UPDATE Auto_Assign_Rule SET rule_name=@n, status_use_id=@st, loc_logic=@ll, priority=@p, is_active=@a," +
                                " remark=@rm, update_by=@by, update_date=GETDATE(), apply_to=@ap WHERE rule_id=@id", c, tx))
                            {
                                AddRuleParams(cmd, name, statusId, locLogic, priority);
                                cmd.Parameters.AddWithValue("@ap", applyTo);
                                cmd.Parameters.AddWithValue("@id", ruleId);
                                if (cmd.ExecuteNonQuery() == 0) throw new InvalidOperationException("ไม่พบกฎ #" + ruleId + " (อาจถูกลบแล้ว)");
                            }
                            Exec(c, tx, "DELETE FROM Auto_Assign_Rule_Cond WHERE rule_id=@id", P("@id", ruleId));
                            Exec(c, tx, "DELETE FROM Auto_Assign_Rule_Resp WHERE rule_id=@id", P("@id", ruleId));
                        }

                        foreach (string v in depts) InsertCond(c, tx, ruleId, "DEPT", v);
                        foreach (string v in cells) InsertCond(c, tx, ruleId, "CELL", v);
                        foreach (string v in stations) InsertCond(c, tx, ruleId, "STATION", v);
                        foreach (string v in stItems) InsertCond(c, tx, ruleId, "STATUS_ITEM", v);
                        foreach (string v in okngs) InsertCond(c, tx, ruleId, "OKNG", v);
                        foreach (string v in repairs) InsertCond(c, tx, ruleId, "REPAIR", v);
                        foreach (string v in reasons) InsertCond(c, tx, ruleId, "REASON", v);

                        for (int i = 0; i < resps.Count; i++)
                            Exec(c, tx, "INSERT INTO Auto_Assign_Rule_Resp (rule_id, emp_id, seq) VALUES (@id, @emp, @seq)",
                                 P("@id", ruleId), P("@emp", resps[i]), P("@seq", i + 1));

                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { tx.Rollback(); } catch { }
                        litEditorError.Text = "<div class='st-err'>บันทึกไม่สำเร็จ : " + H(ex.Message) + "</div>";
                        ShowEditor();
                        return;
                    }
                }
            }

            string after = RuleSummary(ruleId);
            WriteHistory(ruleId, EditRuleId == 0 ? "CREATE" : "UPDATE", EditRuleId == 0 ? after : before + "\n→\n" + after);

            HideEditor();
            Toast("บันทึกกฎ #" + ruleId + " เรียบร้อย", "บันทึกสำเร็จ", "success");
            BindRules();
            BindHistory();
        }

        private void AddRuleParams(SqlCommand cmd, string name, string statusId, string locLogic, int priority)
        {
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@st", statusId == "" ? (object)DBNull.Value : Convert.ToInt32(statusId));
            cmd.Parameters.AddWithValue("@ll", locLogic);
            cmd.Parameters.AddWithValue("@p", priority);
            cmd.Parameters.AddWithValue("@a", chkActive.Checked);
            cmd.Parameters.AddWithValue("@rm", txtRemark.Text.Trim() == "" ? (object)DBNull.Value : txtRemark.Text.Trim());
            cmd.Parameters.AddWithValue("@by", EmpId);
        }

        private static void InsertCond(SqlConnection c, SqlTransaction tx, int ruleId, string type, string value)
        {
            Exec(c, tx, "INSERT INTO Auto_Assign_Rule_Cond (rule_id, cond_type, cond_value) VALUES (@id, @t, @v)",
                 P("@id", ruleId), P("@t", type), P("@v", value));
        }

        private static int Exec(SqlConnection c, SqlTransaction tx, string sql, params SqlParameter[] ps)
        {
            using (var cmd = new SqlCommand(sql, c, tx))
            {
                cmd.Parameters.AddRange(ps);
                return cmd.ExecuteNonQuery();
            }
        }

        private static string Signature(string applyTo, List<string> d, List<string> c, List<string> s, string loc,
                                        List<string> st, List<string> ok, List<string> rp, List<string> rs)
        {
            Func<List<string>, string> j = l => string.Join(",", l.OrderBy(x => x).ToArray());
            return "A:" + applyTo + "|D:" + j(d) + "|C:" + j(c) + "|S:" + j(s) + "|L:" + (c.Count > 0 && s.Count > 0 ? loc : "") +
                   "|ST:" + j(st) + "|OK:" + j(ok) + "|RP:" + j(rp) + "|RS:" + j(rs);
        }

        private static string Signature(RuleInfo r)
        {
            return Signature(r.ApplyTo, r.Depts, r.Cells, r.Stations, r.LocLogic, r.StItems, r.OkNgs, r.Repairs, r.Reasons);
        }

        protected void btnEditorCancel_Click(object sender, EventArgs e) { HideEditor(); }

        private void ShowEditor()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "showEditor",
                "document.getElementById('" + pnlEditor.ClientID + "').style.display='flex'; if(window.initStSelect2) initStSelect2();" +
                " if(window.applyToToggle) applyToToggle();", true);
        }

        private void HideEditor()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "hideEditor",
                "document.getElementById('" + pnlEditor.ClientID + "').style.display='none';", true);
        }

        #endregion

        #region --- เปิด/ปิด , ลบ , ประวัติ ---

        private void ToggleRule(int ruleId)
        {
            RuleInfo r = LoadRuleInfos(ruleId).FirstOrDefault();
            if (r == null) { BindRules(); return; }

            // เปิดใช้งาน → ตรวจว่าเงื่อนไขไม่ซ้ำกับกฎอื่นที่เปิดอยู่
            if (!r.Active)
            {
                string sig = Signature(r);
                RuleInfo dup = LoadRuleInfos(0).FirstOrDefault(x => x.Active && x.Id != r.Id && Signature(x) == sig);
                if (dup != null)
                {
                    Toast("เปิดไม่ได้ : เงื่อนไขซ้ำกับกฎ #" + dup.Id + " \"" + dup.Name + "\"", "เงื่อนไขซ้ำ", "warn");
                    return;
                }
            }

            using (var c = new SqlConnection(connStr))
            {
                c.Open();
                Exec(c, null, "UPDATE Auto_Assign_Rule SET is_active = @a, update_by = @by, update_date = GETDATE() WHERE rule_id = @id",
                     P("@a", !r.Active), P("@by", EmpId), P("@id", ruleId));
            }
            WriteHistory(ruleId, r.Active ? "INACTIVE" : "ACTIVE", RuleSummary(ruleId));
            Toast((r.Active ? "ปิด" : "เปิด") + "ใช้งานกฎ #" + ruleId, "อัปเดตแล้ว", "success");
            BindRules();
            BindHistory();
        }

        private void DeleteRule(int ruleId)
        {
            string summary = RuleSummary(ruleId);
            using (var c = new SqlConnection(connStr))
            {
                c.Open();
                Exec(c, null, "DELETE FROM Auto_Assign_Rule WHERE rule_id = @id", P("@id", ruleId));   // Cond / Resp ลบตาม (CASCADE)
            }
            WriteHistory(ruleId, "DELETE", summary);
            Toast("ลบกฎ #" + ruleId + " แล้ว", "ลบสำเร็จ", "success");
            BindRules();
            BindHistory();
        }

        /// <summary>สรุปกฎเป็นข้อความ (เก็บใน History)</summary>
        private string RuleSummary(int ruleId)
        {
            RuleInfo r = LoadRuleInfos(ruleId).FirstOrDefault();
            if (r == null) return "(ไม่พบกฎ)";
            Func<List<string>, DataTable, string> n = (l, m) => string.Join(", ", l.Select(x => NameOf(m, x)).ToArray());
            return "[" + r.Name + "] " + (r.IsStorage ? "Storage" : "Withdraw") + " · " + (r.Active ? "ใช้งาน" : "ปิด") + " · P=" + r.Priority +
                   (r.StItems.Count > 0 ? " · Status: " + string.Join(", ", r.StItems.ToArray()) : "") +
                   (r.OkNgs.Count > 0 ? " · OK/NG: " + string.Join(", ", r.OkNgs.ToArray()) : "") +
                   (r.Repairs.Count > 0 ? " · Repair: " + string.Join(", ", r.Repairs.ToArray()) : "") +
                   (r.Reasons.Count > 0 ? " · เหตุผล: " + n(r.Reasons, ReasonM) : "") +
                   (r.Depts.Count > 0 ? " · Dept: " + string.Join(", ", r.Depts.ToArray()) : "") +
                   (r.Cells.Count > 0 ? " · Cell: " + n(r.Cells, CellM) : "") +
                   (r.Stations.Count > 0 ? " · Station: " + n(r.Stations, StationM) : "") +
                   (r.Cells.Count > 0 && r.Stations.Count > 0 ? " (" + r.LocLogic + ")" : "") +
                   " · → Status Use: " + (r.StatusId == "" ? "-" : NameOf(StatusM, r.StatusId)) +
                   " · ผู้รับผิดชอบ: " + (r.Resps.Count == 0 ? "-" : string.Join(" > ", r.Resps.ToArray()));
        }

        private void WriteHistory(int ruleId, string action, string detail)
        {
            try
            {
                using (var c = new SqlConnection(connStr))
                {
                    c.Open();
                    Exec(c, null, "INSERT INTO Auto_Assign_Rule_History (rule_id, action, detail, action_by, action_date)" +
                                  " VALUES (@id, @a, @d, @by, GETDATE())",
                         P("@id", ruleId), P("@a", action), P("@d", detail), P("@by", EmpId));
                }
            }
            catch { /* ประวัติบันทึกไม่ได้ ไม่ให้กระทบงานหลัก */ }
        }

        private void BindHistory()
        {
            DataTable dt = Query("SELECT TOP 30 rule_id, action, detail, action_by, action_date" +
                                 " FROM Auto_Assign_Rule_History ORDER BY action_date DESC, hist_id DESC");
            gvHistory.DataSource = dt;
            gvHistory.DataBind();
        }

        #endregion

        #region --- ทดสอบกฎ (เรียก sp_AutoAssign_Resolve ตัวเดียวกับหน้าใช้งานจริง) ---

        protected void btnTest_Click(object sender, EventArgs e)
        {
            if (rblTestApply.SelectedValue == "STORAGE") { TestStorage(); return; }

            string dept = ddlTestDept.SelectedValue, cell = ddlTestCell.SelectedValue, sta = ddlTestStation.SelectedValue;
            if (dept == "" && cell == "" && sta == "")
            {
                litTestResult.Text = "<div class='st-test-res warn'>เลือก Dept / Cell / Station อย่างน้อย 1 ช่อง</div>";
                return;
            }

            DataTable dt;
            using (var c = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("sp_AutoAssign_Resolve", c))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@dept", dept);
                cmd.Parameters.AddWithValue("@cell_id", cell == "" ? (object)DBNull.Value : cell);
                cmd.Parameters.AddWithValue("@station_id", sta == "" ? (object)DBNull.Value : sta);
                dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
            }

            var sb = new StringBuilder();
            if (dt.Rows.Count == 0)
            {
                // ไม่ตรงกฎ → ระบบใช้ค่าเดิม
                var deptResp = RespM.Rows.Cast<DataRow>()
                    .Where(r => S(r["depts"]).Split(',').Select(x => x.Trim()).Contains(dept))
                    .Select(r => S(r["label"])).ToList();
                sb.Append("<div class='st-test-res none'><b>ไม่ตรงกฎใด</b> — ระบบใช้ค่าเดิม");
                sb.Append("<div>Status : " + (dept == "5505" ? "Spare" : "Use") + " (ตาม Dept)</div>");
                sb.Append("<div>ผู้รับผิดชอบ : " + (deptResp.Count == 0 ? "<span class='st-muted'>ไม่มีใน Responsible_Dept ของ Dept นี้</span>"
                                                    : H(deptResp[0]) + " <span class='st-muted'>(คนแรกของ Responsible_Dept)</span>") + "</div></div>");
            }
            else
            {
                DataRow f = dt.Rows[0];
                sb.Append("<div class='st-test-res hit'><b>ตรงกฎ #" + H(S(f["rule_id"])) + " : " + H(S(f["rule_name"])) + "</b>");
                sb.Append(" <span class='st-badge'>" + H(ScoreText(Convert.ToInt32(f["score"]))) + "</span>");
                sb.Append("<div>Status : " + (S(f["status_name"]) == "" ? "<span class='st-muted'>ไม่กำหนด → ค่าเดิม (" + (dept == "5505" ? "Spare" : "Use") + ")</span>" : H(S(f["status_name"]))) + "</div>");
                sb.Append("<div>ผู้รับผิดชอบ :");
                var names = dt.Rows.Cast<DataRow>().Where(r => S(r["emp_id"]) != "").ToList();
                if (names.Count == 0) sb.Append(" <span class='st-muted'>ไม่กำหนด → รายชื่อตาม Dept</span>");
                else
                {
                    sb.Append("<ol class='st-resp'>");
                    for (int i = 0; i < names.Count; i++)
                    {
                        string nm = S(names[i]["pic_dept"]), id = S(names[i]["emp_id"]);
                        sb.Append("<li" + (i == 0 ? " class='is-auto'" : "") + ">" + (i == 0 ? "⭐ " : "") +
                                  H(nm == "" ? id : nm + " (" + id + ")") + (i == 0 ? " <span class='st-muted'>← เลือกอัตโนมัติ</span>" : "") + "</li>");
                    }
                    sb.Append("</ol>");
                }
                sb.Append("</div></div>");
            }
            litTestResult.Text = sb.ToString();
        }

        /// <summary>ทดสอบกฎ Storage (เรียก sp_AutoAssign_Resolve_Storage ตัวเดียวกับ storage.aspx)</summary>
        private void TestStorage()
        {
            string st = ddlTestStItem.SelectedValue, ok = ddlTestOkNg.SelectedValue, rp = ddlTestRepair.SelectedValue,
                   rs = ddlTestReason.SelectedValue, cell = ddlTestCell.SelectedValue, sta = ddlTestStation.SelectedValue;
            if (st == "" && ok == "" && rp == "" && rs == "" && cell == "" && sta == "")
            {
                litTestResult.Text = "<div class='st-test-res warn'>เลือก Status / OK-NG / Repair / เหตุผล / Cell / Station อย่างน้อย 1 ช่อง</div>";
                return;
            }

            DataTable dt = new DataTable();
            try
            {
                using (var c = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("sp_AutoAssign_Resolve_Storage", c))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@status_item", st == "" ? (object)DBNull.Value : st);
                    cmd.Parameters.AddWithValue("@okng", ok == "" ? (object)DBNull.Value : ok);
                    cmd.Parameters.AddWithValue("@repair", rp == "" ? (object)DBNull.Value : rp);
                    cmd.Parameters.AddWithValue("@reason", rs == "" ? (object)DBNull.Value : rs);
                    cmd.Parameters.AddWithValue("@cell_id", cell == "" ? (object)DBNull.Value : cell);
                    cmd.Parameters.AddWithValue("@station_id", sta == "" ? (object)DBNull.Value : sta);
                    using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
                }
            }
            catch (SqlException ex)
            {
                litTestResult.Text = "<div class='st-test-res warn'>เรียก sp_AutoAssign_Resolve_Storage ไม่ได้ — รัน SQL/Auto_Assign_Rule_Storage.sql ก่อน<br/>" + H(ex.Message) + "</div>";
                return;
            }

            var sb = new StringBuilder();
            if (dt.Rows.Count == 0)
            {
                sb.Append("<div class='st-test-res none'><b>ไม่ตรงกฎ Storage ใด</b>");
                sb.Append("<div>Status Use / ผู้รับผิดชอบ : <span class='st-muted'>ว่าง — AAD ต้องเลือกเองใน Modal AAD Confirm</span></div></div>");
            }
            else
            {
                DataRow f = dt.Rows[0];
                sb.Append("<div class='st-test-res hit'><b>ตรงกฎ #" + H(S(f["rule_id"])) + " : " + H(S(f["rule_name"])) + "</b>");
                sb.Append(" <span class='st-badge'>" + H(S(f["score"])) + " เงื่อนไข</span>");
                sb.Append("<div>Status Use : " + (S(f["status_name"]) == "" ? "<span class='st-muted'>ไม่กำหนด → AAD เลือกเอง</span>" : H(S(f["status_name"]))) + "</div>");
                sb.Append("<div>ผู้รับผิดชอบ :");
                var names = dt.Rows.Cast<DataRow>().Where(r => S(r["emp_id"]) != "").ToList();
                if (names.Count == 0) sb.Append(" <span class='st-muted'>ไม่กำหนด → AAD เลือกเอง</span>");
                else
                {
                    sb.Append("<ol class='st-resp'>");
                    for (int i = 0; i < names.Count; i++)
                    {
                        string nm = S(names[i]["pic_dept"]), id = S(names[i]["emp_id"]);
                        sb.Append("<li" + (i == 0 ? " class='is-auto'" : "") + ">" + (i == 0 ? "⭐ " : "") +
                                  H(nm == "" ? id : nm + " (" + id + ")") + (i == 0 ? " <span class='st-muted'>← เลือกอัตโนมัติ</span>" : "") + "</li>");
                    }
                    sb.Append("</ol>");
                }
                sb.Append("</div></div>");
            }
            litTestResult.Text = sb.ToString();
        }

        #endregion

        private void Toast(string detail, string title, string type)
        {
            string script = "if(typeof showToast==='function'){ showToast('" + HttpUtility.JavaScriptStringEncode(detail) +
                            "',{title:'" + HttpUtility.JavaScriptStringEncode(title) + "',type:'" + type + "',duration:4000}); }";
            ScriptManager.RegisterStartupScript(this, GetType(), "toast_" + DateTime.UtcNow.Ticks, script, true);
        }
    }
}
