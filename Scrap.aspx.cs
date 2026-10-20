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
    public partial class Scrap : System.Web.UI.Page
    {
        SqlConnection conn_user = new SqlConnection(ConfigurationManager.ConnectionStrings["user"].ConnectionString);
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        SqlConnection conn_jig = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        datacon dc = new datacon();

        // status_id ปลายทางสำหรับ Scrap (ตัดจำหน่าย)
        private const int SCRAP_STATUS_ID = 12;

        // ══════════════════════════════
        //  PAGE LOAD
        // ══════════════════════════════
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = "Scrap List";
            if (Session["emp_id"] == null || Session["emp_id"].ToString() == "")
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {

            }
        }

        protected void gvSearchResult_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Attributes["id"] = "rowSearch_" + e.Row.RowIndex;

                DataRowView rowView = e.Row.DataItem as DataRowView;
                if (rowView != null)
                {
                    string displayStatus = rowView["DisplayStatus"].ToString();

                    if (displayStatus == "อยู่ระหว่างยืม")
                    {
                        Button btnAdd = e.Row.Cells[1].Controls
                            .OfType<Button>()
                            .FirstOrDefault();

                        if (btnAdd != null)
                        {
                            btnAdd.Enabled = false;
                            btnAdd.ToolTip = "อุปกรณ์นี้อยู่ระหว่างการยืม";
                            btnAdd.CssClass = "btn btn-secondary btn-sm";
                        }
                    }
                }
            }
        }
        protected void gvSearchResult_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "AddItem") return;

            int rowIndex = Convert.ToInt32(e.CommandArgument);
            GridViewRow row = gvSearchResult.Rows[rowIndex];

            HiddenField hdtoolId = (HiddenField)row.FindControl("hdtoolId");
            HiddenField hdtoolSub = (HiddenField)row.FindControl("hdtoolSub");
            HiddenField hdsystemId = (HiddenField)row.FindControl("hdsystemId");
            HiddenField hdmodelId = (HiddenField)row.FindControl("hdmodelId");
            HiddenField hdlocationId = (HiddenField)row.FindControl("hdlocationId");
            HiddenField hdcellId = (HiddenField)row.FindControl("hdcellId");
            HiddenField hdstationId = (HiddenField)row.FindControl("hdstationId");

            if (hdtoolId == null || hdtoolSub == null || hdsystemId == null)
            {
                alert_toast("ข้อมูลในตารางผิดพลาด", "แจ้งเตือน", "error", "5000");
                return;
            }

            string toolId = hdtoolId.Value;
            string toolSub = hdtoolSub.Value;
            string systemId = hdsystemId.Value;

            var list = GetScrapList();

            if (list.Any(x => x.ToolId == toolId && x.ToolSub == toolSub && x.SystemId == systemId))
            {
                alert_toast("รายการนี้ถูกเพิ่มไปแล้ว", "แจ้งเตือน", "warn", "4000");
                return;
            }

            DataTable dtSearch = ViewState["SearchResult"] as DataTable;
            DataRow dr = dtSearch.Rows.Cast<DataRow>().FirstOrDefault(r =>
                r["tool_id"].ToString() == toolId &&
                r["sub_no"].ToString() == toolSub &&
                r["sys_status"].ToString() == systemId);

            if (dr == null)
            {
                alert_toast("ไม่พบข้อมูลต้นฉบับ กรุณาค้นหาใหม่", "แจ้งเตือน", "error", "5000");
                return;
            }

            list.Add(new ScrapItem
            {
                ToolId = toolId,
                ToolSub = toolSub,
                SystemId = systemId,
                ModelId = dr["model_id"].ToString(),
                LocationId = dr["location1_id"].ToString(),
                CellId = dr["location2_id"].ToString(),
                StationId = dr["station_id"].ToString(),
                FixAssetNo = dr["FixAcces"].ToString(),
                ControlNo = dr["control_no"].ToString(),
                ToolNo = dr["tool_no"].ToString(),
                DeptCode = dr["dept_code"].ToString(),
                ModelName = dr["model_name"].ToString(),
                CellName = dr["cell"].ToString(),
                StationName = dr["station"].ToString(),
                OldStatusId = dr["status_id"].ToString()
            });

            SaveScrapList(list);
            BindScrapList();
            alert_toast("เพิ่มรายการเรียบร้อย", "สำเร็จ", "success", "3000");

            // ✅ ล้างผลการค้นหา + ช่อง input ทุกครั้งหลัง Add สำเร็จ
            txtFixAsset.Text = "";
            ViewState["SearchResult"] = null;
            gvSearchResult.DataSource = null;
            gvSearchResult.DataBind();
        }
        protected void gvScrapList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            ScrapItem item = e.Row.DataItem as ScrapItem;
            if (item == null) return;

            e.Row.Attributes["class"] += " scrap-row";
            e.Row.Attributes["data-fixasset"] = item.FixAssetNo ?? "-";
            e.Row.Attributes["data-controlno"] = item.ControlNo ?? "-";
        }
        protected void gvScrapList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "RemoveItem") return;

            int idx = Convert.ToInt32(e.CommandArgument);
            var list = GetScrapList();

            if (idx >= 0 && idx < list.Count)
                list.RemoveAt(idx);

            SaveScrapList(list);
            BindScrapList();
        }

        protected void btn_pic_Click(object sender, EventArgs e)
        {
            GridViewRow row = (sender as LinkButton).NamingContainer as GridViewRow;
            HiddenField hdtool_id = (HiddenField)row.FindControl("hdtoolId");
            HiddenField hdtool_sub = (HiddenField)row.FindControl("hdtoolSub");
            HiddenField hd_sys_status = (HiddenField)row.FindControl("hdsystemId");

            if (hdtool_id == null || hdtool_sub == null || hd_sys_status == null)
            {
                alert_toast("ข้อมูลในตารางผิดพลาด", "ไม่สามารถเปิดรูปได้", "error", "5000");
                return;
            }

            string tool_id = hdtool_id.Value;
            string tool_sub = hdtool_sub.Value;
            string sys_status = hd_sys_status.Value;

            Get_Folder_Structure_Picture Folder_Structure_Picture = new Get_Folder_Structure_Picture(tool_id, tool_sub, sys_status);
            string get_path_toolSub = Folder_Structure_Picture.get_folder_root();
            string path_picture_toolMain = Folder_Structure_Picture.get_folder_root_view_toolMain();

            string[] files_main = Directory.GetFiles(get_path_toolSub, "*.jpg", SearchOption.TopDirectoryOnly);

            if (files_main.Length > 0)
            {
                string fullPathToImage = Path.Combine(get_path_toolSub, files_main[0]);

                try
                {
                    using (System.Drawing.Image img = System.Drawing.Image.FromFile(fullPathToImage))
                    {
                        int imageWidth = img.Width;
                        int imageHeight = img.Height;
                        int maxWidth = 500;
                        int maxHeight = 500;
                        float keep_test;

                        imgMain.ImageUrl = path_picture_toolMain;

                        imgMain.Width = imageWidth;
                        imgMain.Height = imageHeight;

                        if (imageWidth > maxWidth && imageHeight <= maxHeight)
                        {
                            keep_test = imageHeight - (((imageWidth - maxWidth) / imageWidth) * imageHeight);
                            imgMain.Height = Convert.ToInt32(keep_test);
                            imgMain.Width = maxWidth;
                        }
                        else if (imageWidth <= maxWidth && imageHeight > maxHeight)
                        {
                            keep_test = imageWidth - (((imageHeight - maxHeight) / imageHeight) * imageWidth);
                            imgMain.Width = Convert.ToInt32(keep_test);
                            imgMain.Height = maxHeight;
                        }
                        else if (imageWidth > maxWidth && imageHeight > maxHeight)
                        {
                            if (imageWidth > imageHeight)
                            {
                                keep_test = imageHeight - (((imageWidth - maxWidth) / imageWidth) * imageHeight);
                                imgMain.Height = Convert.ToInt32(keep_test);
                                imgMain.Width = maxWidth;
                                if (imageHeight > maxHeight)
                                {
                                    keep_test = imageWidth - (((imageHeight - maxHeight) / imageHeight) * imageWidth);
                                    imgMain.Width = Convert.ToInt32(keep_test);
                                    imgMain.Height = maxHeight;
                                }
                            }
                            else if (imageWidth <= imageHeight)
                            {
                                keep_test = imageWidth - (((imageHeight - maxHeight) / imageHeight) * imageWidth);
                                imgMain.Width = Convert.ToInt32(keep_test);
                                imgMain.Height = maxHeight;
                                if (imageWidth > maxWidth)
                                {
                                    keep_test = imageHeight - (((imageWidth - maxWidth) / imageWidth) * imageHeight);
                                    imgMain.Height = Convert.ToInt32(keep_test);
                                    imgMain.Width = maxWidth;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    imgMain.Height = 500;
                    imgMain.Width = 500;
                    imgMain.ImageUrl = "images/FilesType/jpeg.png";
                }
            }
            else
            {
                imgMain.Height = 500;
                imgMain.Width = 500;
                imgMain.ImageUrl = "images/FilesType/jpeg.png";
            }

            ScriptManager.RegisterStartupScript(
                this, GetType(),
                "showModal",
                "$('#modal_UploadPictureMain').modal('show');",
                true
            );
        }
        protected void btn_detailView_Click(object sender, EventArgs e)
        {
            GridViewRow row = (sender as LinkButton).NamingContainer as GridViewRow;
            HiddenField hdtool_id = (HiddenField)row.FindControl("hdtoolId");
            HiddenField hdtool_sub = (HiddenField)row.FindControl("hdtoolSub");
            HiddenField hd_sys_status = (HiddenField)row.FindControl("hdsystemId");

            if (hdtool_id == null || hdtool_sub == null || hd_sys_status == null)
            {
                alert_toast("ข้อมูลในตารางผิดพลาด", "แจ้งตือน", "error", "5000");
                return;
            }

            string tool_id = hdtool_id.Value;
            string tool_sub = hdtool_sub.Value;
            string sys_status = hd_sys_status.Value;

            string url = "Tool_equipment_detail.aspx?tool_id=" + tool_id + "&tool_sub=" + tool_sub + "&sys_status=" + sys_status;

            sb.Append(@"<script type='text/javascript'>");
            sb.Append("window.open('" + url + "','_blank');");
            sb.Append(@"</script>");
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "closemodal", sb.ToString(), false);
        }
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFixAsset.Text))
            {
                alert_toast("กรุณากรอกข้อมูลที่ต้องการค้นหา", "แจ้งตือน", "warn", "5000");
                return;
            }

            string dept_check = txtFixAsset.Text.Trim();

            string keyword =
                "FixAcces   LIKE '%' + @kw + '%'" +
                " OR control_no LIKE '%' + @kw + '%'" +
                " OR tool_no    LIKE '%' + @kw + '%'";

            string str =
                "SELECT * FROM Tool_Equipment_List_View" +
                " WHERE (" + keyword + ")" + " AND status_id Not IN(11,12) " +
                " ORDER BY tool_no";

            var prms = new System.Collections.Generic.List<SqlParameter>
            {
                new SqlParameter("@kw", SqlDbType.NVarChar) { Value = dept_check }
            };
            if (dept_check.Length == 4)
                prms.Add(new SqlParameter("@dept", SqlDbType.NVarChar) { Value = dept_check });

            dc.QryDT_P(conn, str, prms.ToArray());
            DataTable dt = dc.dt;

            if (!dt.Columns.Contains("RowNo"))
            {
                dt.Columns.Add("RowNo", typeof(int));
                dt.Columns.Add("DisplayStatus", typeof(string));
            }

            // ── ดึงรายการที่กำลังยืมอยู่ (status ≠ 1, 100, 999) ──
            var borrowingSet = new System.Collections.Generic.HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            string strBorrow =
                "SELECT DISTINCT tool_id, sub_id, system_id" +
                " FROM Borrow_List" +
                " WHERE status_id NOT IN (1,100,999)";

            dc.QryDT_P(conn, strBorrow);

            foreach (DataRow br in dc.dt.Rows)
            {
                string key = br["tool_id"] + "|" + br["sub_id"] + "|" + br["system_id"];
                borrowingSet.Add(key);
            }

            int rowNo = 1;
            foreach (DataRow row in dt.Rows)
            {
                row["RowNo"] = rowNo++;

                string checkKey = row["tool_id"] + "|" + row["sub_no"] + "|" + row["sys_status"];

                if (borrowingSet.Contains(checkKey))
                {
                    row["DisplayStatus"] = "อยู่ระหว่างยืม";
                }
                else
                {
                    int statusId = Convert.ToInt32(row["status_id"]);
                    row["DisplayStatus"] = (statusId == 1 || statusId == 2)
                        ? row["status"].ToString()
                        : "Spare : " + row["status"];
                }
            }

            ViewState["SearchResult"] = dt;

            gvSearchResult.DataSource = dt;
            gvSearchResult.DataBind();
        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            var list = GetScrapList();
            if (list.Count == 0)
            {
                alert_toast("ไม่มีรายการสำหรับบันทึก", "แจ้งเตือน", "warn", "4000");
                return; // ← กรณีนี้ปกติจะไม่เกิดเพราะ client validate กันไว้แล้ว
            }

            string empId = Session["emp_id"].ToString();

            conn.Open();
            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                foreach (var item in list)
                {
                    // 1) Insert History
                    string strHistory = @"
                        INSERT INTO History_Scrap
                        (Tool_id, Tool_sub, System_id, Fix_Asset_No, Control_No, Tool_No,
                         Old_Location1_id, Old_Location2_id, Old_Cell, Old_Station,
                         Old_Status_id, New_Status_id, Scrap_By, Scrap_Date)
                        VALUES
                        (@ToolId, @ToolSub, @SystemId, @FixAsset, @ControlNo, @ToolNo,
                         @OldLoc1, @OldLoc2, @OldCell, @OldStation,
                         @OldStatusId, @NewStatusId, @ScrapBy, GETDATE())";

                    using (var cmd = new SqlCommand(strHistory, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@ToolId", item.ToolId);
                        cmd.Parameters.AddWithValue("@ToolSub", item.ToolSub);
                        cmd.Parameters.AddWithValue("@SystemId", item.SystemId);
                        cmd.Parameters.AddWithValue("@FixAsset", (object)item.FixAssetNo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ControlNo", (object)item.ControlNo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ToolNo", (object)item.ToolNo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@OldLoc1", Convert.ToInt32(item.LocationId));
                        cmd.Parameters.AddWithValue("@OldLoc2", Convert.ToInt32(item.CellId));
                        cmd.Parameters.AddWithValue("@OldCell", (object)item.CellName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@OldStation", (object)item.StationName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@OldStatusId", string.IsNullOrEmpty(item.OldStatusId) ? (object)DBNull.Value : Convert.ToInt32(item.OldStatusId));
                        cmd.Parameters.AddWithValue("@NewStatusId", SCRAP_STATUS_ID);
                        cmd.Parameters.AddWithValue("@ScrapBy", empId);
                        cmd.ExecuteNonQuery();
                    }

                    // 2) Update status = 12 (Scrap) ตาม system_id
                    if (item.SystemId == "1")
                    {
                        string strUpd = @"
                            UPDATE jig_tool_system.dbo.toolSub_list
                            SET status_id = @NewStatusId
                            WHERE tool_id = @ToolId AND toolSub_id = @ToolSub";

                        using (var cmd = new SqlCommand(strUpd, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@NewStatusId", SCRAP_STATUS_ID);
                            cmd.Parameters.AddWithValue("@ToolId", item.ToolId);
                            cmd.Parameters.AddWithValue("@ToolSub", item.ToolSub);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else if (item.SystemId == "2")
                    {
                        string strUpd = @"
                            UPDATE tool_eduipment_system.dbo.Equipment_sub_id
                            SET status_use = @NewStatusId
                            WHERE Equipment_id = @ToolId AND Equipment_sub_id = @ToolSub";

                        using (var cmd = new SqlCommand(strUpd, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@NewStatusId", SCRAP_STATUS_ID);
                            cmd.Parameters.AddWithValue("@ToolId", item.ToolId);
                            cmd.Parameters.AddWithValue("@ToolSub", item.ToolSub);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                trans.Commit();
                alert_toast("บันทึกข้อมูลตัดจำหน่าย (Scrap) สำเร็จ", "สำเร็จ", "success", "2500");

                ViewState["ScrapList"] = null;

                // Loading จะยังค้างหน้าจอต่อจนกว่าจะ redirect เต็มรูปแบบ (ตั้งใจ ไม่ต้อง hideLoading)
                ScriptManager.RegisterStartupScript(
                    this, GetType(),
                    "reloadAfterSave",
                    "setTimeout(function(){ window.location.href = window.location.href.split('?')[0]; }, 1500);",
                    true
                );
            }
            catch (Exception ex)
            {
                trans.Rollback();
                alert_toast("เกิดข้อผิดพลาด: " + ex.Message, "ผิดพลาด", "error", "6000");

                // ✅ error → ต้องปิด loading ให้ user แก้ไขและลองใหม่ได้
                ScriptManager.RegisterStartupScript(this, GetType(), "hideLoadingErr", "hideLoading();", true);
            }
            finally
            {
                conn.Close();
            }
        }

        private List<ScrapItem> GetScrapList()
        {
            return ViewState["ScrapList"] as List<ScrapItem> ?? new List<ScrapItem>();
        }
        private void SaveScrapList(List<ScrapItem> list)
        {
            ViewState["ScrapList"] = list;
        }
        private void BindScrapList()
        {
            var list = GetScrapList();
            for (int i = 0; i < list.Count; i++) list[i].RowNo = i + 1;

            gvScrapList.DataSource = list;
            gvScrapList.DataBind();

            btnSave.Enabled = list.Count > 0;
            lblActionStatus.Text = list.Count > 0
                ? "มีรายการรอบันทึก " + list.Count + " รายการ"
                : "รอเพิ่มรายการสำหรับ Scrap";
        }

        // --- Helper : Alert
        protected void alert_toast(string detail, string title, string type, string showtime)
        {
            if (type == "warn" || type == "error" || type == "success" || type == "info")
            {
                int durationMs;
                if (!int.TryParse(showtime, out durationMs))
                    durationMs = 3000;

                string jsDetail = HttpUtility.JavaScriptStringEncode(detail ?? string.Empty);
                string jsType = HttpUtility.JavaScriptStringEncode(type ?? "info");

                string script = "showToast('" + jsType + "', '" + jsDetail + "');";

                string key = "toast_" + DateTime.UtcNow.Ticks;
                ScriptManager.RegisterStartupScript(this, this.GetType(), key, script, true);
            }
        }

        // ── Helper : เลือก CSS class ตาม DisplayStatus ────────────────────────
        protected string GetStatusBadgeClass(string displayStatus)
        {
            if (displayStatus == "อยู่ระหว่างยืม")
                return "badge-borrowing";

            if (displayStatus.StartsWith("Spare"))
                return "badge-spare";

            return "badge-normal";
        }

        [Serializable]
        public class ScrapItem
        {
            public int RowNo { get; set; }
            public string ToolId { get; set; }
            public string ToolSub { get; set; }
            public string SystemId { get; set; }
            public string ModelId { get; set; }
            public string LocationId { get; set; }   // location1_id (system_id=1) / location_id (system_id=2)
            public string CellId { get; set; }       // location2_id (system_id=1) / cell_id (system_id=2)
            public string StationId { get; set; }
            public string FixAssetNo { get; set; }
            public string ControlNo { get; set; }
            public string ToolNo { get; set; }
            public string DeptCode { get; set; }
            public string ModelName { get; set; }
            public string CellName { get; set; }
            public string StationName { get; set; }
            public string OldStatusId { get; set; } // status_id ก่อนตัดจำหน่าย (เก็บลง History)
        }

    }
}
