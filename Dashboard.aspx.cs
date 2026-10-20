// ═══════════════════════════════════════════════════════════════════════════
//  Dashboard.aspx.cs
//  ภาพรวมระบบ Tool & Equipment
// ───────────────────────────────────────────────────────────────────────────
//  Tab 1 : Jig Tool & Equipment — สรุปรายการ แยกประเภท / ราคา / อายุการใช้งาน
//          (Tool_Equipment_List_View , ไม่นับ Transfer 11 / Scrap 12 — แสดงแยกเป็นตัวเลข)
//  Tab 2 : Request — สรุปเอกสาร AAD1 - AAD4 (View_Equipment_Request_List)
//  กราฟเป็น HTML/CSS ล้วน (ไม่ใช้ library ภายนอก)
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
    public partial class Dashboard : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        datacon dc = new datacon();

        private const int STATUS_TRANSFER = 11;
        private const int STATUS_SCRAP = 12;

        // ช่วงอายุการใช้งาน (เดือน) — index ตรงกับ bucket ใน SQL
        private static readonly string[] AGE_LABELS = { "< 1 ปี", "1 - 3 ปี", "3 - 5 ปี", "5 - 10 ปี", "≥ 10 ปี", "ไม่ระบุ" };
        private static readonly string[] AGE_COLORS = { "#16884a", "#2f9e6e", "#d99a06", "#e0782f", "#c93b3b", "#9aa5b1" };

        private static readonly string[] PALETTE =
            { "#1f6feb", "#16884a", "#c0660e", "#7a3fb0", "#0f7c73", "#b4412f", "#946200", "#33475b", "#d14d8f", "#4a7bb7" };

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = "Dashboard";

            if (Session["emp_id"] == null || Session["emp_id"].ToString() == "")
            { Response.Redirect("Default.aspx"); return; }

            if (!IsPostBack)
            {
                BindFilters();
                BindEquipment();
                BindRequest();
            }
        }

        // ═══════════════════════════════════════════════════════
        //  Filters
        // ═══════════════════════════════════════════════════════
        private void BindFilters()
        {
            dc.QryDT(conn, "SELECT dept_id, dept_code, dept_name2 FROM View_dept_master WHERE division = '2' ORDER BY dept_code");
            ddlDept.Items.Clear();
            ddlDept.Items.Add(new ListItem("-- ทุก Dept --", ""));
            foreach (DataRow r in dc.dt.Rows)
                ddlDept.Items.Add(new ListItem(Convert.ToString(r["dept_name2"]), Convert.ToString(r["dept_id"])));

            dc.QryDT(conn, "SELECT DISTINCT YEAR(issue_date) AS y FROM View_Equipment_Request_List WHERE issue_date IS NOT NULL ORDER BY y DESC");
            ddlYear.Items.Clear();
            foreach (DataRow r in dc.dt.Rows)
                ddlYear.Items.Add(new ListItem(Convert.ToString(r["y"]), Convert.ToString(r["y"])));
            ddlYear.Items.Add(new ListItem("-- ทุกปี --", "0"));

            string cy = DateTime.Today.Year.ToString();
            if (ddlYear.Items.FindByValue(cy) != null) ddlYear.SelectedValue = cy;
        }

        protected void ddlEquipFilter_Changed(object sender, EventArgs e)
        {
            BindEquipment();
            KeepTab("tabEquip");
        }

        protected void ddlYear_Changed(object sender, EventArgs e)
        {
            BindRequest();
            KeepTab("tabRequest");
        }

        private void KeepTab(string tab)
        {
            hfTab.Value = tab;
        }

        // ═══════════════════════════════════════════════════════
        //  Tab 1 : Jig Tool & Equipment
        // ═══════════════════════════════════════════════════════
        private class EqRow
        {
            public string Cat, Status;
            public int Sys, Bucket, Cnt, NoPrice, CntMon;
            public decimal Amt, SumMon;
        }

        private void BindEquipment()
        {
            var prms = new List<SqlParameter>();
            string where = "";
            if (ddlDept.SelectedValue != "")
            {
                where += " AND V.dept_id = @dept";
                prms.Add(new SqlParameter("@dept", ddlDept.SelectedValue));
            }
            if (ddlSystem.SelectedValue != "")
            {
                where += " AND V.sys_status = @sys";
                prms.Add(new SqlParameter("@sys", ddlSystem.SelectedValue));
            }

            string sql = @"
                SELECT cat, st, sys_status, bucket,
                       COUNT(*)                                        AS cnt,
                       SUM(ISNULL(pr, 0))                              AS amt,
                       SUM(CASE WHEN pr IS NULL THEN 1 ELSE 0 END)    AS no_price,
                       SUM(ISNULL(mon, 0))                             AS sum_mon,
                       SUM(CASE WHEN mon IS NULL THEN 0 ELSE 1 END)   AS cnt_mon
                FROM (
                    SELECT ISNULL(NULLIF(LTRIM(RTRIM(CAST(V.category AS nvarchar(100)))), ''), '(ไม่ระบุ)') AS cat,
                           ISNULL(NULLIF(LTRIM(RTRIM(CAST(V.status AS nvarchar(100)))), ''), '(ไม่ระบุ)')   AS st,
                           ISNULL(V.sys_status, 0) AS sys_status,
                           TRY_CONVERT(decimal(18, 2), REPLACE(CAST(V.price AS nvarchar(50)), ',', '')) AS pr,
                           CASE WHEN TRY_CONVERT(date, V.start_use) IS NULL THEN NULL
                                ELSE DATEDIFF(MONTH, TRY_CONVERT(date, V.start_use), GETDATE()) END AS mon,
                           CASE WHEN TRY_CONVERT(date, V.start_use) IS NULL THEN 5
                                WHEN DATEDIFF(MONTH, TRY_CONVERT(date, V.start_use), GETDATE()) < 12  THEN 0
                                WHEN DATEDIFF(MONTH, TRY_CONVERT(date, V.start_use), GETDATE()) < 36  THEN 1
                                WHEN DATEDIFF(MONTH, TRY_CONVERT(date, V.start_use), GETDATE()) < 60  THEN 2
                                WHEN DATEDIFF(MONTH, TRY_CONVERT(date, V.start_use), GETDATE()) < 120 THEN 3
                                ELSE 4 END AS bucket
                    FROM Tool_Equipment_List_View V
                    WHERE V.ID IS NOT NULL AND V.status_id NOT IN (" + STATUS_TRANSFER + "," + STATUS_SCRAP + @")" + where + @"
                ) B
                GROUP BY cat, st, sys_status, bucket";

            dc.QryDT_P(conn, sql, prms.ToArray());
            var rows = new List<EqRow>();
            foreach (DataRow r in dc.dt.Rows)
            {
                rows.Add(new EqRow
                {
                    Cat = Convert.ToString(r["cat"]),
                    Status = Convert.ToString(r["st"]),
                    Sys = ToInt(r["sys_status"]),
                    Bucket = ToInt(r["bucket"]),
                    Cnt = ToInt(r["cnt"]),
                    Amt = ToDec(r["amt"]),
                    NoPrice = ToInt(r["no_price"]),
                    SumMon = ToDec(r["sum_mon"]),
                    CntMon = ToInt(r["cnt_mon"])
                });
            }

            // Transfer / Scrap (ไม่นับรวมในภาพรวม)
            var prms2 = new List<SqlParameter>();
            foreach (SqlParameter p in prms) prms2.Add(new SqlParameter(p.ParameterName, p.Value));
            dc.QryDT_P(conn,
                "SELECT V.status_id, COUNT(*) AS cnt FROM Tool_Equipment_List_View V" +
                " WHERE V.ID IS NOT NULL AND V.status_id IN (" + STATUS_TRANSFER + "," + STATUS_SCRAP + ")" + where +
                " GROUP BY V.status_id", prms2.ToArray());
            int nTransfer = 0, nScrap = 0;
            foreach (DataRow r in dc.dt.Rows)
            {
                if (ToInt(r["status_id"]) == STATUS_TRANSFER) nTransfer = ToInt(r["cnt"]);
                if (ToInt(r["status_id"]) == STATUS_SCRAP) nScrap = ToInt(r["cnt"]);
            }

            int total = rows.Sum(x => x.Cnt);
            decimal amt = rows.Sum(x => x.Amt);
            int noPrice = rows.Sum(x => x.NoPrice);
            int cntMon = rows.Sum(x => x.CntMon);
            decimal avgYear = cntMon == 0 ? 0 : rows.Sum(x => x.SumMon) / cntMon / 12m;
            int jig = rows.Where(x => x.Sys == 1).Sum(x => x.Cnt);
            int equip = rows.Where(x => x.Sys == 2).Sum(x => x.Cnt);
            int old10 = rows.Where(x => x.Bucket == 4).Sum(x => x.Cnt);

            // ── KPI ──
            var kpi = new StringBuilder();
            kpi.Append(Kpi("รายการทั้งหมด", total.ToString("N0"), "Jig " + jig.ToString("N0") + " · Equipment " + equip.ToString("N0"), "kpi-blue", "glyphicon-th-list"));
            kpi.Append(Kpi("มูลค่ารวม (บาท)", amt.ToString("N0"),
                       noPrice > 0 ? "ไม่ระบุราคา " + noPrice.ToString("N0") + " รายการ" : "ครบทุกรายการ", "kpi-green", "glyphicon-usd"));
            kpi.Append(Kpi("อายุใช้งานเฉลี่ย", avgYear.ToString("N1") + " ปี",
                       "ใช้งาน ≥ 10 ปี " + old10.ToString("N0") + " รายการ", "kpi-amber", "glyphicon-time"));
            kpi.Append(Kpi("Transfer / Scrap", nTransfer.ToString("N0") + " / " + nScrap.ToString("N0"),
                       "ไม่นับรวมในภาพรวม", "kpi-gray", "glyphicon-transfer"));
            litEqKpi.Text = kpi.ToString();

            // ── แยกประเภท : จำนวน / มูลค่า / อายุ ──
            var byCat = rows.GroupBy(x => x.Cat)
                .Select(g => new
                {
                    Cat = g.Key,
                    Cnt = g.Sum(x => x.Cnt),
                    Amt = g.Sum(x => x.Amt),
                    AvgY = g.Sum(x => x.CntMon) == 0 ? (decimal?)null : g.Sum(x => x.SumMon) / g.Sum(x => x.CntMon) / 12m,
                    Buckets = Enumerable.Range(0, AGE_LABELS.Length).Select(b => g.Where(x => x.Bucket == b).Sum(x => x.Cnt)).ToArray()
                })
                .OrderByDescending(x => x.Cnt).ToList();

            int maxCnt = byCat.Count == 0 ? 0 : byCat.Max(x => x.Cnt);
            decimal maxAmt = byCat.Count == 0 ? 0 : byCat.Max(x => x.Amt);

            var t = new StringBuilder();
            t.Append("<table class='db-table'><thead><tr>" +
                     "<th class='l'>ประเภท</th><th>จำนวน</th><th class='l' style='width:22%'>สัดส่วนจำนวน</th>" +
                     "<th>มูลค่ารวม (บาท)</th><th class='l' style='width:18%'>สัดส่วนมูลค่า</th><th>อายุเฉลี่ย (ปี)</th>" +
                     "<th class='l' style='width:22%'>ช่วงอายุการใช้งาน</th></tr></thead><tbody>");
            int ci = 0;
            foreach (var c in byCat)
            {
                string color = PALETTE[ci++ % PALETTE.Length];
                t.Append("<tr><td class='l'><span class='db-dot' style='background:" + color + "'></span>" + H(c.Cat) + "</td>");
                t.Append("<td><b>" + c.Cnt.ToString("N0") + "</b></td>");
                t.Append("<td class='l'>" + Bar(c.Cnt, maxCnt, color, Pct(c.Cnt, total)) + "</td>");
                t.Append("<td>" + c.Amt.ToString("N0") + "</td>");
                t.Append("<td class='l'>" + Bar(c.Amt, maxAmt, color, Pct(c.Amt, amt)) + "</td>");
                t.Append("<td>" + (c.AvgY.HasValue ? c.AvgY.Value.ToString("N1") : "-") + "</td>");
                t.Append("<td class='l'>" + StackBar(c.Buckets) + "</td></tr>");
            }
            if (byCat.Count == 0) t.Append("<tr><td colspan='7' class='db-empty'>ไม่พบข้อมูล</td></tr>");
            else
                t.Append("<tr class='db-total'><td class='l'>รวม</td><td>" + total.ToString("N0") + "</td><td></td><td>" +
                         amt.ToString("N0") + "</td><td></td><td>" + avgYear.ToString("N1") + "</td><td class='l'>" +
                         StackBar(Enumerable.Range(0, AGE_LABELS.Length).Select(b => rows.Where(x => x.Bucket == b).Sum(x => x.Cnt)).ToArray()) +
                         "</td></tr>");
            t.Append("</tbody></table>");
            litEqByCat.Text = t.ToString();
            litAgeLegend.Text = AgeLegend();

            // ── ช่วงอายุการใช้งาน (รวม) ──
            var ages = new StringBuilder();
            int[] totB = Enumerable.Range(0, AGE_LABELS.Length).Select(b => rows.Where(x => x.Bucket == b).Sum(x => x.Cnt)).ToArray();
            decimal[] amtB = Enumerable.Range(0, AGE_LABELS.Length).Select(b => rows.Where(x => x.Bucket == b).Sum(x => x.Amt)).ToArray();
            int maxB = totB.Length == 0 ? 0 : totB.Max();
            for (int b = 0; b < AGE_LABELS.Length; b++)
            {
                ages.Append("<div class='db-hrow'><span class='db-hlabel'>" + H(AGE_LABELS[b]) + "</span>" +
                            Bar(totB[b], maxB, AGE_COLORS[b], totB[b].ToString("N0") + " (" + Pct(totB[b], total) + ")") +
                            "<span class='db-hsub'>" + amtB[b].ToString("N0") + " ฿</span></div>");
            }
            litEqAge.Text = ages.ToString();

            // ── แยกสถานะ ──
            var byStatus = rows.GroupBy(x => x.Status).Select(g => new { St = g.Key, Cnt = g.Sum(x => x.Cnt) })
                               .OrderByDescending(x => x.Cnt).ToList();
            int maxSt = byStatus.Count == 0 ? 0 : byStatus.Max(x => x.Cnt);
            var s = new StringBuilder();
            int si = 0;
            foreach (var x in byStatus)
                s.Append("<div class='db-hrow'><span class='db-hlabel'>" + H(x.St) + "</span>" +
                         Bar(x.Cnt, maxSt, PALETTE[si++ % PALETTE.Length], x.Cnt.ToString("N0") + " (" + Pct(x.Cnt, total) + ")") + "</div>");
            if (byStatus.Count == 0) s.Append("<div class='db-empty'>ไม่พบข้อมูล</div>");
            litEqStatus.Text = s.ToString();
        }

        // ═══════════════════════════════════════════════════════
        //  Tab 2 : Request (AAD1 - AAD4)
        // ═══════════════════════════════════════════════════════
        private class RqRow
        {
            public string Doc, DocName, StatusName;
            public int Status, Period, Cnt;
        }

        private void BindRequest()
        {
            int year = ToInt(ddlYear.SelectedValue);
            bool byYear = year == 0;

            string sql = @"
                SELECT V.document_id,
                       ISNULL(D.document_name, V.document_id)              AS doc_name,
                       V.status_id,
                       ISNULL(S.status_name, CAST(V.status_id AS varchar(10))) AS status_name,
                       " + (byYear ? "YEAR(V.issue_date)" : "MONTH(V.issue_date)") + @" AS period,
                       COUNT(*) AS cnt
                FROM View_Equipment_Request_List V
                LEFT JOIN Master_Document D ON D.document_id = V.document_id
                LEFT JOIN Master_Status   S ON S.status_id   = V.status_id
                WHERE V.document_id IN ('AAD1','AAD2','AAD3','AAD4')" +
                (byYear ? "" : " AND YEAR(V.issue_date) = @y") + @"
                GROUP BY V.document_id, D.document_name, V.status_id, S.status_name, " +
                (byYear ? "YEAR(V.issue_date)" : "MONTH(V.issue_date)");

            dc.QryDT_P(conn, sql, new SqlParameter("@y", year));
            var rows = new List<RqRow>();
            foreach (DataRow r in dc.dt.Rows)
                rows.Add(new RqRow
                {
                    Doc = Convert.ToString(r["document_id"]).Trim(),
                    DocName = Convert.ToString(r["doc_name"]).Trim(),
                    Status = ToInt(r["status_id"]),
                    StatusName = Convert.ToString(r["status_name"]).Trim(),
                    Period = ToInt(r["period"]),
                    Cnt = ToInt(r["cnt"])
                });

            string[] docs = { "AAD1", "AAD2", "AAD3", "AAD4" };
            string[] docColor = { "#1f6feb", "#c0660e", "#16884a", "#7a3fb0" };

            // ── Card ต่อประเภทเอกสาร ──
            var cards = new StringBuilder();
            for (int i = 0; i < docs.Length; i++)
            {
                var d = rows.Where(x => x.Doc == docs[i]).ToList();
                string name = d.Count > 0 ? d[0].DocName : docs[i];
                int all = d.Sum(x => x.Cnt);
                int draft = d.Where(x => x.Status == 1).Sum(x => x.Cnt);
                int done = d.Where(x => x.Status == 100).Sum(x => x.Cnt);
                int cancel = d.Where(x => x.Status == 999).Sum(x => x.Cnt);
                int wip = all - draft - done - cancel;

                cards.Append("<div class='rq-card' style='border-top-color:" + docColor[i] + "'>" +
                             "<div class='rq-head'><span class='rq-doc' style='background:" + docColor[i] + "'>" + docs[i] + "</span>" +
                             "<span class='rq-name'>" + H(name) + "</span></div>" +
                             "<div class='rq-total'>" + all.ToString("N0") + "<small> เอกสาร</small></div>" +
                             "<div class='rq-split'>" +
                             RqStat("ระหว่างดำเนินการ", wip, "#d99a06") +
                             RqStat("เสร็จสิ้น", done, "#16884a") +
                             RqStat("Draft", draft, "#66727f") +
                             RqStat("ยกเลิก", cancel, "#c93b3b") +
                             "</div>" + Bar(wip, all, docColor[i], "ค้าง " + Pct(wip, all)) + "</div>");
            }
            litRqCards.Text = cards.ToString();

            // ── ตาราง เอกสาร × สถานะ ──
            var statuses = rows.GroupBy(x => x.Status).OrderBy(g => g.Key)
                               .Select(g => new { Id = g.Key, Name = g.First().StatusName }).ToList();
            var m = new StringBuilder();
            m.Append("<table class='db-table'><thead><tr><th class='l'>เอกสาร</th>");
            foreach (var st in statuses) m.Append("<th>" + H(st.Name) + "</th>");
            m.Append("<th>รวม</th></tr></thead><tbody>");
            int maxCell = rows.Count == 0 ? 0 : rows.GroupBy(x => x.Doc + "|" + x.Status).Max(g => g.Sum(x => x.Cnt));
            for (int i = 0; i < docs.Length; i++)
            {
                var d = rows.Where(x => x.Doc == docs[i]).ToList();
                m.Append("<tr><td class='l'><span class='db-dot' style='background:" + docColor[i] + "'></span>" + docs[i] +
                         (d.Count > 0 ? " <small class='db-muted'>" + H(d[0].DocName) + "</small>" : "") + "</td>");
                foreach (var st in statuses)
                {
                    int v = d.Where(x => x.Status == st.Id).Sum(x => x.Cnt);
                    m.Append("<td>" + Heat(v, maxCell, docColor[i]) + "</td>");
                }
                m.Append("<td><b>" + d.Sum(x => x.Cnt).ToString("N0") + "</b></td></tr>");
            }
            m.Append("<tr class='db-total'><td class='l'>รวม</td>");
            foreach (var st in statuses) m.Append("<td>" + rows.Where(x => x.Status == st.Id).Sum(x => x.Cnt).ToString("N0") + "</td>");
            m.Append("<td>" + rows.Sum(x => x.Cnt).ToString("N0") + "</td></tr></tbody></table>");
            litRqStatus.Text = statuses.Count == 0 ? "<div class='db-empty'>ไม่พบข้อมูล</div>" : m.ToString();

            // ── แนวโน้มรายเดือน (ปีที่เลือก) / รายปี (ทุกปี) ──
            List<int> periods = byYear
                ? rows.Select(x => x.Period).Where(p => p > 0).Distinct().OrderBy(p => p).ToList()
                : Enumerable.Range(1, 12).ToList();
            var perTotals = periods.Select(p => rows.Where(x => x.Period == p).Sum(x => x.Cnt)).ToList();
            int maxPer = perTotals.Count == 0 ? 0 : perTotals.Max();

            var tr = new StringBuilder();
            tr.Append("<div class='trend'>");
            for (int pi = 0; pi < periods.Count; pi++)
            {
                int p = periods[pi];
                tr.Append("<div class='trend-col' title='" + PeriodLabel(p, byYear) + " : " + perTotals[pi] + " เอกสาร'>" +
                          "<div class='trend-val'>" + (perTotals[pi] > 0 ? perTotals[pi].ToString("N0") : "") + "</div>" +
                          "<div class='trend-stack'>");
                for (int i = docs.Length - 1; i >= 0; i--)
                {
                    int v = rows.Where(x => x.Period == p && x.Doc == docs[i]).Sum(x => x.Cnt);
                    if (v == 0 || maxPer == 0) continue;
                    double h = 100.0 * v / maxPer;
                    tr.Append("<div class='trend-seg' style='height:" + h.ToString("0.##", CultureInfo.InvariantCulture) +
                              "%;background:" + docColor[i] + "' title='" + docs[i] + " : " + v + "'></div>");
                }
                tr.Append("</div><div class='trend-lbl'>" + PeriodLabel(p, byYear) + "</div></div>");
            }
            if (periods.Count == 0) tr.Append("<div class='db-empty'>ไม่พบข้อมูล</div>");
            tr.Append("</div><div class='db-legend'>");
            for (int i = 0; i < docs.Length; i++)
                tr.Append("<span><i style='background:" + docColor[i] + "'></i>" + docs[i] + "</span>");
            tr.Append("</div>");
            litRqTrend.Text = tr.ToString();
            litTrendTitle.Text = byYear ? "จำนวนเอกสารรายปี" : "จำนวนเอกสารรายเดือน ปี " + year;
        }

        // ═══════════════════════════════════════════════════════
        //  HTML helpers
        // ═══════════════════════════════════════════════════════
        private static string H(string s) { return HttpUtility.HtmlEncode(s ?? ""); }

        private static int ToInt(object o)
        {
            if (o == null || o == DBNull.Value) return 0;
            int v; return int.TryParse(Convert.ToString(o), out v) ? v : 0;
        }
        private static decimal ToDec(object o)
        {
            if (o == null || o == DBNull.Value) return 0;
            decimal v; return decimal.TryParse(Convert.ToString(o), NumberStyles.Any, CultureInfo.InvariantCulture, out v) ? v : 0;
        }

        private static string Pct(decimal v, decimal total)
        {
            if (total == 0) return "0%";
            return (v * 100m / total).ToString("0.#", CultureInfo.InvariantCulture) + "%";
        }

        private static string Kpi(string label, string value, string sub, string css, string icon)
        {
            return "<div class='kpi " + css + "'><span class='kpi-ico glyphicon " + icon + "'></span>" +
                   "<div class='kpi-label'>" + H(label) + "</div><div class='kpi-value'>" + H(value) + "</div>" +
                   "<div class='kpi-sub'>" + H(sub) + "</div></div>";
        }

        private static string Bar(decimal v, decimal max, string color, string text)
        {
            double w = max == 0 ? 0 : (double)(v * 100m / max);
            if (v > 0 && w < 1.5) w = 1.5;
            return "<div class='db-bar'><div class='db-bar-fill' style='width:" + w.ToString("0.##", CultureInfo.InvariantCulture) +
                   "%;background:" + color + "'></div><span class='db-bar-txt'>" + H(text) + "</span></div>";
        }

        private static string StackBar(int[] buckets)
        {
            int sum = buckets.Sum();
            if (sum == 0) return "<span class='db-muted'>-</span>";
            var sb = new StringBuilder("<div class='db-stack'>");
            for (int b = 0; b < buckets.Length; b++)
            {
                if (buckets[b] == 0) continue;
                double w = 100.0 * buckets[b] / sum;
                sb.Append("<div style='width:" + w.ToString("0.##", CultureInfo.InvariantCulture) + "%;background:" + AGE_COLORS[b] +
                          "' title='" + AGE_LABELS[b] + " : " + buckets[b] + " รายการ'>" + (w >= 12 ? buckets[b].ToString() : "") + "</div>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        private static string AgeLegend()
        {
            var sb = new StringBuilder("<div class='db-legend'>");
            for (int b = 0; b < AGE_LABELS.Length; b++)
                sb.Append("<span><i style='background:" + AGE_COLORS[b] + "'></i>" + H(AGE_LABELS[b]) + "</span>");
            sb.Append("</div>");
            return sb.ToString();
        }

        private static string RqStat(string label, int v, string color)
        {
            return "<div class='rq-stat'><b style='color:" + color + "'>" + v.ToString("N0") + "</b><span>" + H(label) + "</span></div>";
        }

        private static string Heat(int v, int max, string color)
        {
            if (v == 0) return "<span class='db-muted'>-</span>";
            double a = max == 0 ? 0 : 0.12 + 0.6 * v / max;
            return "<span class='heat' style='background:" + HexToRgba(color, a) + "'>" + v.ToString("N0") + "</span>";
        }

        private static string HexToRgba(string hex, double a)
        {
            hex = hex.TrimStart('#');
            int r = Convert.ToInt32(hex.Substring(0, 2), 16), g = Convert.ToInt32(hex.Substring(2, 2), 16), b = Convert.ToInt32(hex.Substring(4, 2), 16);
            return "rgba(" + r + "," + g + "," + b + "," + a.ToString("0.##", CultureInfo.InvariantCulture) + ")";
        }

        private static string PeriodLabel(int p, bool byYear)
        {
            if (byYear) return p.ToString();
            return CultureInfo.GetCultureInfo("en-US").DateTimeFormat.GetAbbreviatedMonthName(p);
        }
    }
}
