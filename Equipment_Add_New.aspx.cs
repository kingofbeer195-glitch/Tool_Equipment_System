using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClosedXML.Excel;
using System.Net.Mail;

namespace Tool_Equipment_Control_System
{
    public partial class Equipment_Add_New : System.Web.UI.Page
    {
        private const string TEMPLATE_PATH =
            @"\\cptfile02\Pro\PRJ2023008\100_System Format\08_Tool&Equipment\Format_AddNew_REV00.xlsx";

        private const string VS_DT = "ImportedEquipmentDT";
        private const string VS_STATUS = "StatusListXml";

        private string ConnStr { get { return ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString; } }
        private string ConnStrJig { get { return ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString; } }

        // ══════════════════════════════
        //  ViewState XML Helpers
        // ══════════════════════════════
        private DataTable ImportedDT
        {
            get { return VsGetDT(VS_DT); }
            set { VsSetDT(VS_DT, value); }
        }

        private DataTable VsGetDT(string key)
        {
            if (ViewState[key] == null) return null;
            try
            {
                var dt = new DataTable();
                dt.ReadXml(new StringReader((string)ViewState[key]));
                return dt;
            }
            catch { return null; }
        }

        private void VsSetDT(string key, DataTable value)
        {
            if (value == null) { ViewState.Remove(key); return; }
            if (string.IsNullOrEmpty(value.TableName)) value.TableName = key;
            DataTable safe = MakeSafeForXml(value);
            var sw = new StringWriter();
            safe.WriteXml(sw, XmlWriteMode.WriteSchema);
            ViewState[key] = sw.ToString();
        }

        private static DataTable MakeSafeForXml(DataTable src)
        {
            var dst = new DataTable(src.TableName);
            foreach (DataColumn c in src.Columns)
                dst.Columns.Add(c.ColumnName, typeof(string));
            foreach (DataRow r in src.Rows)
            {
                var nr = dst.NewRow();
                foreach (DataColumn c in src.Columns)
                    nr[c.ColumnName] = (r[c] == null || r[c] == DBNull.Value) ? "" : r[c].ToString();
                dst.Rows.Add(nr);
            }
            return dst;
        }

        // ══════════════════════════════
        //  PAGE LOAD
        // ══════════════════════════════
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = "New Register";
            if (Session["emp_id"] == null || Session["emp_id"].ToString() == "")
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                DataTable dt = ImportedDT;
                if (dt != null && dt.Rows.Count > 0)
                {
                    BindPreview(dt);
                    HideDropzone();
                }
            }
        }

        // ══════════════════════════════
        //  LOAD TEMPLATE
        // ══════════════════════════════
        protected void btnLoadTemplate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(TEMPLATE_PATH))
                { ShowError("ไม่พบไฟล์ Template: " + TEMPLATE_PATH); return; }
                byte[] bytes = File.ReadAllBytes(TEMPLATE_PATH);
                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("Content-Disposition", "attachment; filename=\"" + Path.GetFileName(TEMPLATE_PATH) + "\"");
                Response.BinaryWrite(bytes);
                Response.Flush();
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex) { ShowError("Load Template error: " + ex.Message); }
        }

        // ══════════════════════════════
        //  IMPORT EXCEL
        // ══════════════════════════════
        protected void btnImportExcel_Click(object sender, EventArgs e)
        {
            if (!fileUpload.HasFile) { ShowError("กรุณาเลือกไฟล์ Excel ก่อน"); return; }
            string ext = Path.GetExtension(fileUpload.FileName).ToLower();
            if (ext != ".xlsx" && ext != ".xls") { ShowError("รองรับเฉพาะไฟล์ .xlsx / .xls"); return; }

            try
            {
                DataTable dt = ReadExcel(new MemoryStream(fileUpload.FileBytes));
                if (dt == null || dt.Rows.Count == 0) { ShowError("ไม่พบข้อมูลในไฟล์"); return; }

                ImportedDT = dt;
                BindPreview(dt);
                lblActionStatus.Text = "พร้อมบันทึก — " + dt.Rows.Count + " รายการ";
                btnSave.Enabled = true;
                HideDropzone();
                ShowSuccess("Import สำเร็จ " + dt.Rows.Count + " รายการ");
            }
            catch (Exception ex) { ShowError("เกิดข้อผิดพลาด: " + ex.Message); }
        }

        // ══════════════════════════════
        //  READ EXCEL
        // ══════════════════════════════
        private DataTable ReadExcel(Stream stream)
        {
            var dt = new DataTable("EquipmentData");

            using (var wb = new XLWorkbook(stream))
            {
                var ws = wb.Worksheet(1);
                if (!ws.RowsUsed().Any()) return dt;

                var headerRow = ws.FirstRowUsed();
                int totalCols = headerRow.LastCellUsed().Address.ColumnNumber;

                for (int c = 1; c <= totalCols; c++)
                {
                    string col = headerRow.Cell(c).GetValue<string>().Trim();
                    if (string.IsNullOrEmpty(col)) col = "Column" + c;
                    dt.Columns.Add(EnsureUnique(dt, col));
                }

                foreach (var row in ws.RowsUsed().Skip(1))
                {
                    var dr = dt.NewRow();
                    bool hasData = false;
                    for (int c = 1; c <= totalCols; c++)
                    {
                        string val = row.Cell(c).GetValue<string>().Trim();
                        dr[c - 1] = val;
                        if (!string.IsNullOrEmpty(val)) hasData = true;
                    }
                    if (hasData) dt.Rows.Add(dr);
                }
            }

            // ── Extra Columns (string ทั้งหมด) ──
            string[] extraCols = {
                "MATCH_STATUS", "CHK_STATUS", "UPDATE_STATUS", "UPDATE_ERROR",
                "SQL_ToolId",   "SQL_SubId",  "SQL_SubRecordId", "SQL_System", "SQL_ToolNo",
                "SQL_DuplicateStatus",
                "SQL_DeptCode", "SQL_DeptID", "SQL_DivID", "SQL_DeptCode_ID", "EDIT_Dept",
                "SQL_FixAsset", "EDIT_SUBFIX", "SQL_ControlNo", "EDIT_CONTROLNO",
                "SQL_MODEL",    "SQL_MODEL_ID",    "EDIT_MODEL",
                "SQL_Location", "SQL_Location_ID", "EDIT_LOCATION",
                "SQL_Cell",     "SQL_Cell_ID",     "EDIT_CELL",
                "SQL_Station",  "SQL_Station_ID",  "EDIT_STATION",
                "SQL_Responsible", "EDIT_Responsible",
                "SQL_BOI",   "SQL_BOI_ID",   "EDIT_BOI",
                "SQL_PAPER", "SQL_PAPER_ID", "EDIT_PAPER",
                "SQL_STATUS","SQL_STATUS_ID","EDIT_STATUS",
                "EDIT_OPERATION_DATE","EDIT_DUE_DATE","EDIT_REMARK","EDIT_PERIOD"
            };
            foreach (string col in extraCols)
                if (!dt.Columns.Contains(col))
                    dt.Columns.Add(col, typeof(string));

            MergeExcelWithSql(dt);
            return dt;
        }

        // ══════════════════════════════════════════════════════════════════
        //  MERGE — Lookup ทุก row
        // ══════════════════════════════════════════════════════════════════
        private void MergeExcelWithSql(DataTable dt)
        {
            foreach (DataRow r in dt.Rows)
            {
                // ── Equipment_No ──
                string equipNo = ColVal(r, "Equipment_No (Model)", "Equipment_No");
                string assetNo = ColVal(r, "Asset_No");
                string controlNo = ColVal(r, "Control_No");

                string displayId = "", subId = "", source = "0", rawId = "", dupStatus = "";

                // ── Step 1: ค้น conn_jig -> tool_master ──
                bool foundJig = LookupJig(equipNo, out rawId, out displayId);

                if (foundJig)
                {
                    source = "1";
                    // Step 1.1: ค้น toolSub_list ด้วย Asset_No
                    string subRaw = "";
                    bool foundSub = LookupToolSub(assetNo, out subRaw);
                    if (foundSub)
                    {
                        subId = "S" + subRaw.PadLeft(5, '0');
                        dupStatus = "Same";
                    }
                    else
                    {
                        dupStatus = "เพิ่ม Tool";
                    }
                }
                else
                {
                    // ── Step 2: ค้น conn -> Equipment_master_list ──
                    bool foundEqp = LookupEquipment(equipNo, out rawId, out displayId);
                    if (foundEqp)
                    {
                        source = "2";
                        // Step 2.1: ค้น Equipment_sub_id ด้วย Asset_No หรือ Control_No
                        string subRaw = "";
                        bool foundSub = LookupEquipmentSub(assetNo, controlNo, out subRaw);
                        if (foundSub)
                        {
                            subId = "ES" + subRaw.PadLeft(5, '0');
                            dupStatus = "Same";
                        }
                        else
                        {
                            dupStatus = "เพิ่ม Equipment";
                        }
                    }
                    else
                    {
                        source = "0";
                        displayId = "New";
                        dupStatus = "New";
                    }
                }

                r["SQL_ToolId"] = displayId;
                r["SQL_SubId"] = source;
                r["SQL_SubRecordId"] = subId;
                r["SQL_ToolNo"] = rawId;
                r["SQL_DuplicateStatus"] = dupStatus;

                // MATCH_STATUS
                if (source == "0")
                    r["MATCH_STATUS"] = "New";
                else if (source == "1")
                    r["MATCH_STATUS"] = "JIG DB";
                else
                    r["MATCH_STATUS"] = "EQP DB";
            }
        }

        // ── Lookup: tool_master (conn_jig) ──
        private bool LookupJig(string equipNo, out string rawId, out string displayId)
        {
            rawId = ""; displayId = "";
            if (string.IsNullOrEmpty(equipNo)) return false;
            try
            {
                using (var conn = new SqlConnection(ConnStrJig))
                using (var cmd = new SqlCommand(
                    "SELECT TOP 1 tool_id FROM tool_master WHERE tool_no = @no", conn))
                {
                    cmd.Parameters.AddWithValue("@no", equipNo);
                    conn.Open();
                    object val = cmd.ExecuteScalar();
                    if (val != null && val != DBNull.Value)
                    {
                        rawId = val.ToString();
                        displayId = "T" + rawId.PadLeft(5, '0');
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        // ── Lookup: Equipment_master_list (conn) ──
        private bool LookupEquipment(string equipNo, out string rawId, out string displayId)
        {
            rawId = ""; displayId = "";
            if (string.IsNullOrEmpty(equipNo)) return false;
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                using (var cmd = new SqlCommand(
                    "SELECT TOP 1 equipment_id FROM Equipment_master_list WHERE equipment_no = @no", conn))
                {
                    cmd.Parameters.AddWithValue("@no", equipNo);
                    conn.Open();
                    object val = cmd.ExecuteScalar();
                    if (val != null && val != DBNull.Value)
                    {
                        rawId = val.ToString();
                        displayId = "E" + rawId.PadLeft(5, '0');
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        // ── Lookup: toolSub_list (conn_jig) ด้วย Asset_No ──
        private bool LookupToolSub(string assetNo, out string subRawId)
        {
            subRawId = "";
            if (string.IsNullOrEmpty(assetNo)) return false;
            try
            {
                using (var conn = new SqlConnection(ConnStrJig))
                using (var cmd = new SqlCommand(
                    "SELECT TOP 1 toolSub_id FROM toolSub_list WHERE FixAcces = @asset", conn))
                {
                    cmd.Parameters.AddWithValue("@asset", assetNo);
                    conn.Open();
                    object val = cmd.ExecuteScalar();
                    if (val != null && val != DBNull.Value)
                    {
                        subRawId = val.ToString();
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        // ── Lookup: Equipment_sub_id (conn) ด้วย Asset_No หรือ Control_No ──
        private bool LookupEquipmentSub(string assetNo, string controlNo, out string subRawId)
        {
            subRawId = "";
            if (string.IsNullOrEmpty(assetNo) && string.IsNullOrEmpty(controlNo)) return false;
            try
            {
                string sql = @"SELECT TOP 1 Equipment_sub_id
                               FROM Equipment_sub_id
                               WHERE fix_asset = @asset OR control_no = @ctrl";
                using (var conn = new SqlConnection(ConnStr))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@asset", string.IsNullOrEmpty(assetNo) ? (object)DBNull.Value : assetNo);
                    cmd.Parameters.AddWithValue("@ctrl", string.IsNullOrEmpty(controlNo) ? (object)DBNull.Value : controlNo);
                    conn.Open();
                    object val = cmd.ExecuteScalar();
                    if (val != null && val != DBNull.Value)
                    {
                        subRawId = val.ToString();
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        // ══════════════════════════════
        //  BIND PREVIEW
        // ══════════════════════════════
        private void BindPreview(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                pnlEmpty.Visible = true; pnlPreview.Visible = false;
                lblRowCount.Visible = false; btnClearPreview.Visible = false;
                return;
            }

            // คอลัมน์ที่แสดง — เรียงลำดับ: Match columns ก่อน แล้วตามด้วย Excel columns
            var matchCols = new List<string> {
                "MATCH_STATUS", "SQL_ToolId", "SQL_SubRecordId", "SQL_DuplicateStatus",
                "SQL_SubId",    // hidden
                "SQL_ToolNo"    // hidden
            };

            var excelCols = new List<string>();
            foreach (DataColumn col in dt.Columns)
            {
                string n = col.ColumnName;
                if (!n.StartsWith("SQL_") && !n.StartsWith("EDIT_")
                    && n != "MATCH_STATUS" && n != "UPDATE_STATUS"
                    && n != "UPDATE_ERROR" && n != "CHK_STATUS")
                    excelCols.Add(n);
            }

            var showCols = new List<string>();
            showCols.AddRange(matchCols);
            showCols.AddRange(excelCols);

            DataTable dtView = dt.DefaultView.ToTable(false, showCols.ToArray());
            gvPreview.DataSource = dtView;
            gvPreview.DataBind();

            lblRowCount.Text = dt.Rows.Count + " รายการ";
            lblRowCount.Visible = true;
            btnClearPreview.Visible = true;
            pnlEmpty.Visible = false;
            pnlPreview.Visible = true;
        }

        // ══════════════════════════════
        //  ROW DATA BOUND
        // ══════════════════════════════
        protected void gvPreview_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                // ✅ ค้น index จาก e.Row (Header row) ตอนนี้ cell.Text ยังเป็นชื่อ column จริง
                int idxSubId = FindCellIndex(e.Row, "SQL_SubId");
                int idxToolNo = FindCellIndex(e.Row, "SQL_ToolNo");
                int idxMatch = FindCellIndex(e.Row, "MATCH_STATUS");
                int idxToolId = FindCellIndex(e.Row, "SQL_ToolId");
                int idxSubRec = FindCellIndex(e.Row, "SQL_SubRecordId");
                int idxDup = FindCellIndex(e.Row, "SQL_DuplicateStatus");

                // เปลี่ยน text ก่อน
                if (idxMatch >= 0) e.Row.Cells[idxMatch].Text = "แหล่งข้อมูล";
                if (idxToolId >= 0) e.Row.Cells[idxToolId].Text = "ID หลัก";
                if (idxSubRec >= 0) e.Row.Cells[idxSubRec].Text = "Sub ID";
                if (idxDup >= 0) e.Row.Cells[idxDup].Text = "สถานะ";

                // ซ่อน — ทำหลังสุด
                if (idxSubId >= 0) e.Row.Cells[idxSubId].Style["display"] = "none";
                if (idxToolNo >= 0) e.Row.Cells[idxToolNo].Style["display"] = "none";

                // ✅ เก็บ index ลง ViewState เพื่อให้ Data rows ใช้ต่อ
                ViewState["_ci_SubId"] = idxSubId;
                ViewState["_ci_ToolNo"] = idxToolNo;
                ViewState["_ci_Match"] = idxMatch;
                ViewState["_ci_ToolId"] = idxToolId;
                ViewState["_ci_SubRec"] = idxSubRec;
                ViewState["_ci_Dup"] = idxDup;
                return;
            }

            if (e.Row.RowType != DataControlRowType.DataRow) return;

            var drv = e.Row.DataItem as DataRowView;
            if (drv == null) return;

            // ✅ อ่าน index จาก ViewState — ตรงกับที่ Header ซ่อนไว้
            int colSubId = ViewState["_ci_SubId"] != null ? (int)ViewState["_ci_SubId"] : -1;
            int colToolNo = ViewState["_ci_ToolNo"] != null ? (int)ViewState["_ci_ToolNo"] : -1;
            int colMatch = ViewState["_ci_Match"] != null ? (int)ViewState["_ci_Match"] : -1;
            int colId = ViewState["_ci_ToolId"] != null ? (int)ViewState["_ci_ToolId"] : -1;
            int colSub = ViewState["_ci_SubRec"] != null ? (int)ViewState["_ci_SubRec"] : -1;
            int colDup = ViewState["_ci_Dup"] != null ? (int)ViewState["_ci_Dup"] : -1;

            // ซ่อน hidden cols — ตรงกับ header
            if (colSubId >= 0) e.Row.Cells[colSubId].Style["display"] = "none";
            if (colToolNo >= 0) e.Row.Cells[colToolNo].Style["display"] = "none";

            string source = drv["SQL_SubId"].ToString();
            string dupStatus = drv["SQL_DuplicateStatus"].ToString();

            // Row background
            if (source == "0")
                e.Row.Style["background-color"] = "#f0fdf4";
            else if (dupStatus == "Same")
                e.Row.Style["background-color"] = "#fff7ed";

            // MATCH_STATUS badge
            if (colMatch >= 0)
            {
                string badge = source == "0" ? "<span class='badge-new'>✨ New</span>"
                             : source == "1" ? "<span class='badge-jig'>🔧 JIG</span>"
                             : "<span class='badge-eqp'>⚙️ EQP</span>";
                e.Row.Cells[colMatch].Text = badge;
            }

            // SQL_ToolId
            if (colId >= 0)
            {
                string idVal = drv["SQL_ToolId"].ToString();
                string cls = source == "1" ? "id-tag jig"
                             : source == "2" ? "id-tag eqp" : "id-tag new";
                e.Row.Cells[colId].Text = (string.IsNullOrEmpty(idVal) || idVal == "New")
                    ? "<span class='id-tag new'>-</span>"
                    : "<span class='" + cls + "'>" + idVal + "</span>";
            }

            // SQL_SubRecordId
            if (colSub >= 0)
            {
                string subVal = drv["SQL_SubRecordId"].ToString();
                e.Row.Cells[colSub].Text = string.IsNullOrEmpty(subVal)
                    ? "<span class='cell-empty'>-</span>"
                    : "<span class='id-tag sub'>" + subVal + "</span>";
            }

            // SQL_DuplicateStatus
            if (colDup >= 0)
            {
                string dupBadge = dupStatus == "Same" ? "<span class='badge-same'>⚠️ Same</span>"
                                : dupStatus == "เพิ่ม Tool" ? "<span class='badge-addtool'>➕ เพิ่ม Tool</span>"
                                : dupStatus == "เพิ่ม Equipment" ? "<span class='badge-addeqp'>➕ เพิ่ม Eqp</span>"
                                : dupStatus == "New" ? "<span class='badge-new'>✨ New</span>"
                                : "<span class='cell-empty'>-</span>";
                e.Row.Cells[colDup].Text = dupBadge;
            }

            // เซลล์ทั่วไป — ถ้าว่างแสดง "-"
            for (int i = 0; i < e.Row.Cells.Count; i++)
            {
                if (i == colMatch || i == colId || i == colSub || i == colDup) continue;
                if (i == colSubId || i == colToolNo) continue;
                var cell = e.Row.Cells[i];
                if (cell.Style["display"] == "none") continue;
                if (cell.Controls.Count == 0 && string.IsNullOrWhiteSpace(cell.Text))
                    cell.Text = "<span class='cell-empty'>-</span>";
            }
        }

        // ✅ ค้น index จาก GridViewRow โดยตรง (ใช้ตอน Header event เท่านั้น)
        private int FindCellIndex(GridViewRow row, string colName)
        {
            for (int i = 0; i < row.Cells.Count; i++)
                if (row.Cells[i].Text.Equals(colName, StringComparison.OrdinalIgnoreCase))
                    return i;
            return -1;
        }

        // ══════════════════════════════
        //  SEND EMAIL
        // ══════════════════════════════
        private void SendEmailNotification(DataTable dt, Guid batchId, int saved)
        {
            try
            {
                string empId = Session["emp_id"] != null ? Session["emp_id"].ToString() : "SYSTEM";
                string today = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                // ── สร้างตารางสรุปผลใน Email ──
                var sbTable = new StringBuilder();

                // นับสถิติ
                int cntNew = 0, cntSame = 0, cntAddTool = 0, cntAddEqp = 0;
                foreach (DataRow r in dt.Rows)
                {
                    string dup = r.Table.Columns.Contains("SQL_DuplicateStatus")
                        ? r["SQL_DuplicateStatus"].ToString() : "";
                    if (dup == "New") cntNew++;
                    else if (dup == "Same") cntSame++;
                    else if (dup == "เพิ่ม Tool") cntAddTool++;
                    else if (dup == "เพิ่ม Equipment") cntAddEqp++;
                }

                // ── สร้าง HTML Body ──
                sbTable.AppendLine(@"
                    <html><body style='font-family:Segoe UI,Arial,sans-serif; font-size:13px; color:#1e293b;'>

                    <div style='background:linear-gradient(135deg,#1d4ed8,#3b82f6);
                                color:#fff; padding:16px 24px; border-radius:8px; margin-bottom:20px;'>
                        <h2 style='margin:0; font-size:18px;'>⚙️ Equipment Registration Notification</h2>
                        <p style='margin:4px 0 0; opacity:.85; font-size:13px;'>
                            แจ้งเตือน: มีการลงทะเบียน Equipment ใหม่ รอการ Confirm
                        </p>
                    </div>

                    <table style='border-collapse:collapse; width:100%; margin-bottom:20px;'>
                        <tr>
                            <td style='padding:6px 12px; color:#475569; width:140px;'>📅 วันที่บันทึก</td>
                            <td style='padding:6px 12px; font-weight:600;'>" + today + @"</td>
                        </tr>
                        <tr style='background:#f8fafc;'>
                            <td style='padding:6px 12px; color:#475569;'>👤 บันทึกโดย</td>
                            <td style='padding:6px 12px; font-weight:600;'>" + empId + @"</td>
                        </tr>
                        <tr>
                            <td style='padding:6px 12px; color:#475569;'>🔖 Batch ID</td>
                            <td style='padding:6px 12px; font-size:11px; color:#64748b;'>" + batchId.ToString() + @"</td>
                        </tr>
                    </table>

                    <!-- สรุปสถิติ -->
                    <table style='border-collapse:collapse; width:100%; margin-bottom:20px;
                                  border:1px solid #e2e8f0; border-radius:8px; overflow:hidden;'>
                        <tr style='background:#1d4ed8; color:#fff;'>
                            <th style='padding:10px 14px; text-align:center;'>รายการทั้งหมด</th>
                            <th style='padding:10px 14px; text-align:center; background:#15803d;'>✨ New</th>
                            <th style='padding:10px 14px; text-align:center; background:#c2410c;'>⚠️ Same</th>
                            <th style='padding:10px 14px; text-align:center; background:#1d4ed8;'>➕ เพิ่ม Tool</th>
                            <th style='padding:10px 14px; text-align:center; background:#059669;'>➕ เพิ่ม Eqp</th>
                        </tr>
                        <tr style='text-align:center; font-size:20px; font-weight:700;'>
                            <td style='padding:12px; border:1px solid #e2e8f0;'>" + saved + @"</td>
                            <td style='padding:12px; border:1px solid #e2e8f0; color:#15803d;'>" + cntNew + @"</td>
                            <td style='padding:12px; border:1px solid #e2e8f0; color:#c2410c;'>" + cntSame + @"</td>
                            <td style='padding:12px; border:1px solid #e2e8f0; color:#1d4ed8;'>" + cntAddTool + @"</td>
                            <td style='padding:12px; border:1px solid #e2e8f0; color:#059669;'>" + cntAddEqp + @"</td>
                        </tr>
                    </table> ");

                int rowNum = 1;

                sbTable.AppendLine("</table>");

                // ── Signature ──
                string signature =
                    "<br><br>" +
                    "<div style='border-top:1px solid #e2e8f0; padding-top:14px; " +
                                "color:#94a3b8; font-size:11px; line-height:1.8;'>" +
                    "************************************************<br>" +
                    "This auto-email was sent from Tool &amp; Equipment System.<br>" +
                    "Best regard.<br>" +
                    "--------------------- Original Message Ends --------------------<br>" +
                    "************************************************" +
                    "</div>" +
                    "</body></html>";

                string mailBody = sbTable.ToString() + signature;

                // ── ส่ง Email ──
                var mail = new MailMessage();
                mail.From = new MailAddress("engineer_pde@mail.canon");
                mail.To.Add("anirut@mail.canon");

                // CC ถ้าต้องการ
                // mail.CC.Add("supervisor@mail.canon");

                mail.Subject = "[Equipment] Register New Item " + DateTime.Now.ToString("dd/MM/yyyy");
                mail.Body = mailBody;
                mail.IsBodyHtml = true; // ✅ ส่งเป็น HTML

                var client = new SmtpClient();
                client.Host = "150.61.249.70";
                client.Port = 25;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                client.Send(mail);

                System.Diagnostics.Debug.WriteLine("[Email] Sent successfully to anirut@mail.canon");
            }
            catch (Exception ex)
            {
                // ไม่ให้ Email error หยุดการทำงานหลัก — แค่ log ไว้
                System.Diagnostics.Debug.WriteLine("[Email Error] " + ex.Message);
                ShowError("บันทึกสำเร็จ แต่ส่ง Email ไม่สำเร็จ: " + ex.Message);
            }
        }

        // ── Helper ──
        private static string HtmlEncode(string s)
        {
            return string.IsNullOrEmpty(s) ? "-"
                : HttpUtility.HtmlEncode(s);
        }

        private static string ColValRow(DataRow r, params string[] cols)
        {
            foreach (string c in cols)
                if (r.Table.Columns.Contains(c))
                {
                    string v = r[c] == null ? "" : r[c].ToString().Trim();
                    if (!string.IsNullOrEmpty(v)) return v;
                }
            return "-";
        }

        // ══════════════════════════════
        //  HELPERS
        // ══════════════════════════════
        // ── Helper: หา col index จาก DataTable column order (ไม่ขึ้นกับ header text) ──
        private int GetColIndexByName(string colName)
        {
            if (gvPreview.HeaderRow == null) return -1;
            for (int i = 0; i < gvPreview.HeaderRow.Cells.Count; i++)
            {
                string cellText = gvPreview.HeaderRow.Cells[i].Text;
                if (cellText.Equals(colName, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }
        private int GetColIndex(GridView gv, string colName)
        {
            if (gv.HeaderRow == null) return -1;
            for (int i = 0; i < gv.HeaderRow.Cells.Count; i++)
                if (gv.HeaderRow.Cells[i].Text.Equals(colName, StringComparison.OrdinalIgnoreCase))
                    return i;
            return -1;
        }

        private static string EnsureUnique(DataTable dt, string name)
        {
            if (!dt.Columns.Contains(name)) return name;
            int i = 1;
            while (dt.Columns.Contains(name + "_" + i)) i++;
            return name + "_" + i;
        }

        private static string ColVal(DataRow r, params string[] cols)
        {
            foreach (string c in cols)
                if (r.Table.Columns.Contains(c))
                {
                    string v = r[c] == null ? "" : r[c].ToString().Trim();
                    if (!string.IsNullOrEmpty(v)) return v;
                }
            return "";
        }

        // ══════════════════════════════
        //  CLEAR PREVIEW
        // ══════════════════════════════
        protected void btnClearPreview_Click(object sender, EventArgs e)
        {
            ImportedDT = null;
            ViewState.Remove(VS_STATUS);
            gvPreview.DataSource = null;
            gvPreview.DataBind();
            pnlEmpty.Visible = true;
            pnlPreview.Visible = false;
            lblRowCount.Visible = false;
            btnClearPreview.Visible = false;
            btnSave.Enabled = false;
            lblActionStatus.Text = "รอ Import ข้อมูล";
            ShowDropzone();
        }

        // ══════════════════════════════
        //  SAVE
        // ══════════════════════════════
        protected void btnSave_Click(object sender, EventArgs e)
        {
            DataTable dt = ImportedDT;
            if (dt == null || dt.Rows.Count == 0)
            { ShowError("ไม่มีข้อมูลให้บันทึก"); return; }

            // สร้าง batch_id ใหม่ทุกครั้งที่กด Save
            Guid batchId = Guid.NewGuid();
            string empId = Session["emp_id"] != null ? Session["emp_id"].ToString() : "SYSTEM";

            int saved = 0, errors = 0;
            var sbErr = new StringBuilder();

            using (var conn = new SqlConnection(ConnStr))
            {
                try { conn.Open(); }
                catch (Exception ex) { ShowError("DB Error: " + ex.Message); return; }

                int seq = 1;
                foreach (DataRow r in dt.Rows)
                {
                    try
                    {
                        SaveRow(r, conn, batchId, seq, empId);
                        saved++;
                    }
                    catch (Exception ex)
                    {
                        errors++;
                        sbErr.AppendLine("แถว " + seq + ": " + ex.Message);
                    }
                    seq++;
                }
            }

            if (errors == 0)
            {
                // ✅ ส่ง Email ก่อน clear data
                SendEmailNotification(dt, batchId, saved);

                ImportedDT = null;
                lblActionStatus.Text = "บันทึกสำเร็จ " + saved + " รายการ ✅";
                btnSave.Enabled = false;
                ShowSuccess("บันทึกสำเร็จ " + saved + " รายการ");
                ShowDropzone();
                // reset preview
                gvPreview.DataSource = null;
                gvPreview.DataBind();
                pnlEmpty.Visible = true;
                pnlPreview.Visible = false;
                lblRowCount.Visible = false;
                btnClearPreview.Visible = false;
            }
            else
            {
                lblActionStatus.Text = "สำเร็จ " + saved + " / ผิดพลาด " + errors;
                ShowError("สำเร็จ " + saved + " รายการ, ผิดพลาด " + errors + " รายการ");
                System.Diagnostics.Debug.WriteLine(sbErr.ToString());
            }
        }

        // ── Insert 1 แถว ──
        private void SaveRow(DataRow r, SqlConnection conn,
            Guid batchId, int seq, string empId)
        {
            string sql = @"
        INSERT INTO equipment_add_new_request (
            batch_id,           batch_seq,
            status,             equipment_no,       name,
            dept_code,          asset_no,           control_no,
            sub_fix,            boi,                paper,
            serial_no,          manufacture,        price,
            pr_po,              operation_date,     cpt_model,
            location,           cell,               station,
            responsible,        calibration_date,   cal_period,
            remark,
            match_status,       match_source,       sql_tool_id,
            sql_sub_record_id,  sql_tool_no_raw,    duplicate_status,
            status_confirm,     created_by,         created_date
        ) VALUES (
            @batch_id,          @batch_seq,
            @status,            @equipment_no,      @name,
            @dept_code,         @asset_no,          @control_no,
            @sub_fix,           @boi,               @paper,
            @serial_no,         @manufacture,       @price,
            @pr_po,             @operation_date,    @cpt_model,
            @location,          @cell,              @station,
            @responsible,       @calibration_date,  @cal_period,
            @remark,
            @match_status,      @match_source,      @sql_tool_id,
            @sql_sub_record_id, @sql_tool_no_raw,   @duplicate_status,
            'Pending',          @created_by,        GETDATE()
        )";

            using (var cmd = new SqlCommand(sql, conn))
            {
                // ── Batch ──
                cmd.Parameters.AddWithValue("@batch_id", batchId);
                cmd.Parameters.AddWithValue("@batch_seq", seq);

                // ── Excel data ──
                cmd.Parameters.AddWithValue("@status", NzStr(r, "Status"));
                cmd.Parameters.AddWithValue("@equipment_no", NzStr(r, "Equipment_No (Model)", "Equipment_No"));
                cmd.Parameters.AddWithValue("@name", NzStr(r, "Name"));
                cmd.Parameters.AddWithValue("@dept_code", NzStr(r, "Dept_Code"));
                cmd.Parameters.AddWithValue("@asset_no", NzStr(r, "Asset_No"));
                cmd.Parameters.AddWithValue("@control_no", NzStr(r, "Control_No"));
                cmd.Parameters.AddWithValue("@sub_fix", NzStr(r, "Sub_Fix"));
                cmd.Parameters.AddWithValue("@boi", NzStr(r, "BOI"));
                cmd.Parameters.AddWithValue("@paper", NzStr(r, "Paper"));
                cmd.Parameters.AddWithValue("@serial_no", NzStr(r, "Serial_No"));
                cmd.Parameters.AddWithValue("@manufacture", NzStr(r, "Manufacture"));
                cmd.Parameters.AddWithValue("@price", ParseDecimal(r, "Price"));
                cmd.Parameters.AddWithValue("@pr_po", NzStr(r, "PR_PO"));
                cmd.Parameters.AddWithValue("@operation_date", ParseDate(r, "Operation_Date"));
                cmd.Parameters.AddWithValue("@cpt_model", NzStr(r, "CPT_Model"));
                cmd.Parameters.AddWithValue("@location", NzStr(r, "Location"));
                cmd.Parameters.AddWithValue("@cell", NzStr(r, "Cell"));
                cmd.Parameters.AddWithValue("@station", NzStr(r, "Station"));
                cmd.Parameters.AddWithValue("@responsible", NzStr(r, "Responsible"));
                cmd.Parameters.AddWithValue("@calibration_date", ParseDate(r, "Calibration_Date"));
                cmd.Parameters.AddWithValue("@cal_period", NzStr(r, "Cal_Period"));
                cmd.Parameters.AddWithValue("@remark", NzStr(r, "Remark", "REMARK"));

                // ── Lookup result ──
                cmd.Parameters.AddWithValue("@match_status", NzStr(r, "MATCH_STATUS"));
                cmd.Parameters.AddWithValue("@match_source", NzStr(r, "SQL_SubId"));
                cmd.Parameters.AddWithValue("@sql_tool_id", NzStr(r, "SQL_ToolId"));
                cmd.Parameters.AddWithValue("@sql_sub_record_id", NzStr(r, "SQL_SubRecordId"));
                cmd.Parameters.AddWithValue("@sql_tool_no_raw", NzStr(r, "SQL_ToolNo"));
                cmd.Parameters.AddWithValue("@duplicate_status", NzStr(r, "SQL_DuplicateStatus"));

                // ── Audit ──
                cmd.Parameters.AddWithValue("@created_by", empId);

                cmd.ExecuteNonQuery();
            }
        }

        // ── JS ──
        private void HideDropzone()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "hideDz_" + DateTime.Now.Ticks,
                "setTimeout(function(){ hideDzWrapper(); },50);", true);
        }
        private void ShowDropzone()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "showDz_" + DateTime.Now.Ticks,
                "setTimeout(function(){ showDzWrapper(); },50);", true);
        }

        private static object Nz(DataRow r, params string[] cols)
        {
            foreach (string c in cols)
            {
                if (!r.Table.Columns.Contains(c)) continue;
                string v = r[c] == null ? "" : r[c].ToString().Trim();
                if (!string.IsNullOrEmpty(v)) return v;
            }
            return DBNull.Value;
        }

        // ── NzStr: คืน DBNull ถ้าว่าง (แทน Nz เดิมที่คืน object) ──
        private static object NzStr(DataRow r, params string[] cols)
        {
            foreach (string c in cols)
            {
                if (!r.Table.Columns.Contains(c)) continue;
                string v = r[c] == null ? "" : r[c].ToString().Trim();
                if (!string.IsNullOrEmpty(v)) return v;
            }
            return DBNull.Value;
        }

        private static object ParseDate(DataRow r, string col)
        {
            if (!r.Table.Columns.Contains(col)) return DBNull.Value;
            string s = r[col] == null ? "" : r[col].ToString().Trim();
            if (string.IsNullOrEmpty(s)) return DBNull.Value;
            DateTime dt;
            return DateTime.TryParse(s, out dt) ? (object)dt : DBNull.Value;
        }

        private static object ParseDecimal(DataRow r, string col)
        {
            if (!r.Table.Columns.Contains(col)) return DBNull.Value;
            string s = r[col] == null ? "" : r[col].ToString().Trim().Replace(",", "");
            if (string.IsNullOrEmpty(s)) return DBNull.Value;
            decimal d;
            return decimal.TryParse(s, out d) ? (object)d : DBNull.Value;
        }

        private void ShowError(string msg)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "err_" + DateTime.Now.Ticks,
                "setTimeout(function(){ showToast('error'," +
                HttpUtility.JavaScriptStringEncode(msg, true) + "); },200);", true);
        }

        private void ShowSuccess(string msg)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "ok_" + DateTime.Now.Ticks,
                "setTimeout(function(){ showToast('success'," +
                HttpUtility.JavaScriptStringEncode(msg, true) + "); },200);", true);
        }
    }
}