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
    public partial class Transfer : System.Web.UI.Page
    {
        SqlConnection conn_user = new SqlConnection(ConfigurationManager.ConnectionStrings["user"].ConnectionString);
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        SqlConnection conn_jig = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        datacon dc = new datacon();

        // ══════════════════════════════
        //  PAGE LOAD
        // ══════════════════════════════
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = "Transfer";
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

            var list = GetTransferList();
            SyncSotoSelectionsToList(list);

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

            list.Add(new TransferItem
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
                StationName = dr["station"].ToString()
            });

            SaveTransferList(list);
            BindTransferList();
            alert_toast("เพิ่มรายการเรียบร้อย", "สำเร็จ", "success", "3000");

            // ✅ ล้างผลการค้นหา + ช่อง input ทุกครั้งหลัง Add สำเร็จ
            txtFixAsset.Text = "";
            ViewState["SearchResult"] = null;
            gvSearchResult.DataSource = null;
            gvSearchResult.DataBind();
        }
        protected void gvTransferList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            TransferItem item = e.Row.DataItem as TransferItem;
            if (item == null) return;

            e.Row.Attributes["class"] += " transfer-row";
            e.Row.Attributes["data-fixasset"] = item.FixAssetNo ?? "-";
            e.Row.Attributes["data-controlno"] = item.ControlNo ?? "-";

            DropDownList ddlSoto = e.Row.FindControl("ddlSoto") as DropDownList;
            if (ddlSoto == null) return;

            ddlSoto.Items.Clear();
            ddlSoto.Items.Add(new ListItem("-- เลือก Soto Cell --", ""));

            try
            {
                using (var connLocal = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString))
                using (var cmd = new SqlCommand(
                    "SELECT location1_id, location2_id, location2_name" +
                    " FROM master_location2" +
                    " WHERE location1_id = 98 AND is_use = 1" +
                    " ORDER BY location2_name", connLocal))
                {
                    connLocal.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ddlSoto.Items.Add(new ListItem(
                                reader["location2_name"].ToString(),
                                reader["location2_id"].ToString()));
                        }
                    }
                }

                // ✅ คืนค่าที่เคยเลือกไว้กลับมา (กัน selection หาย)
                if (!string.IsNullOrEmpty(item.SotoId))
                {
                    ListItem existing = ddlSoto.Items.FindByValue(item.SotoId);
                    if (existing != null) ddlSoto.SelectedValue = item.SotoId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("RowDataBound error: " + ex.Message);
            }
        }
        protected void gvTransferList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "RemoveItem") return;

            int idx = Convert.ToInt32(e.CommandArgument);
            var list = GetTransferList();

            SyncSotoSelectionsToList(list);

            if (idx >= 0 && idx < list.Count)
                list.RemoveAt(idx);

            SaveTransferList(list);
            BindTransferList();
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

            //lb_test.Text = "Tool ID : " + tool_id + " , Sub ID : " + tool_sub + " , System ID : " + sys_status;

            Get_Folder_Structure_Picture Folder_Structure_Picture = new Get_Folder_Structure_Picture(tool_id, tool_sub, sys_status);
            string get_path_toolSub = Folder_Structure_Picture.get_folder_root();
            string path_picture_toolMain = Folder_Structure_Picture.get_folder_root_view_toolMain();

            string[] files_main = Directory.GetFiles(get_path_toolSub, "*.jpg", SearchOption.TopDirectoryOnly);

            if (files_main.Length > 0)
            {
                string fullPathToImage = Path.Combine(get_path_toolSub, files_main[0]); // สร้าง full path ไปยังไฟล์รูปภาพจริง

                try
                {
                    // โหลดรูปภาพเพื่อดึงขนาด
                    using (System.Drawing.Image img = System.Drawing.Image.FromFile(fullPathToImage))
                    {
                        int imageWidth = img.Width;
                        int imageHeight = img.Height;
                        int maxWidth = 500;
                        int maxHeight = 500;
                        float keep_test;

                        // กำหนด ImageUrl ให้กับคอนโทรล imgMain
                        //alert_toast(fullPathToImage, "แจ้งตือน", "error", "20000");
                        imgMain.ImageUrl = path_picture_toolMain;

                        imgMain.Width = imageWidth;
                        imgMain.Height = imageHeight;

                        // ถ้าคุณต้องการกำหนดขนาดของ imgMain ตามขนาดจริงของรูปภาพ
                        if (imageWidth > maxWidth && imageHeight <= maxHeight)
                        {
                            //lb_testPic.Text = "IF_1 : " + imageWidth.ToString() + ":" + imageHeight.ToString();
                            keep_test = imageHeight - (((imageWidth - maxWidth) / imageWidth) * imageHeight);
                            imgMain.Height = Convert.ToInt32(keep_test);
                            imgMain.Width = maxWidth;
                        }
                        else if (imageWidth <= maxWidth && imageHeight > maxHeight)
                        {
                            //lb_testPic.Text = "IF_2 : " + imageWidth.ToString() + ":" + imageHeight.ToString();
                            keep_test = imageWidth - (((imageHeight - maxHeight) / imageHeight) * imageWidth);
                            imgMain.Width = Convert.ToInt32(keep_test);
                            imgMain.Height = maxHeight;
                        }
                        else if (imageWidth > maxWidth && imageHeight > maxHeight)
                        {
                            //lb_testPic.Text = "IF_3 : " + imageWidth.ToString() + ":" + imageHeight.ToString();
                            if (imageWidth > imageHeight)
                            {
                                //lb_testPic.Text = "IF_3-1 : " + imageWidth.ToString() + ":" + imageHeight.ToString();
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
                                //lb_testPic.Text = "IF_3-2 : " + imageWidth.ToString() + ":" + imageHeight.ToString();
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
                        else
                        {
                            //lb_testPic.Text = "IF_4 : " + imageWidth.ToString() + ":" + imageHeight.ToString();
                        }

                    }
                }
                catch (Exception ex)
                {
                    imgMain.Height = 500;
                    imgMain.Width = 500;
                    imgMain.ImageUrl = "images/FilesType/jpeg.png"; // แสดงภาพ default หากเกิดข้อผิดพลาด
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
            //return
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

            // keyword เป็น static string ทั้งหมด — ค่าจริงไปทาง parameter
            string keyword =
                "FixAcces   LIKE '%' + @kw + '%'" +
                " OR control_no LIKE '%' + @kw + '%'" +
                " OR tool_no    LIKE '%' + @kw + '%'";

            string str =
                "SELECT * FROM Tool_Equipment_List_View" +
                " WHERE (" + keyword + ")" +
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
            var list = GetTransferList();
            if (list.Count == 0)
            {
                alert_toast("ไม่มีรายการสำหรับบันทึก", "แจ้งเตือน", "warn", "4000");
                return; // ← กรณีนี้ปกติจะไม่เกิดเพราะ client validate กันไว้แล้ว
            }

            for (int i = 0; i < gvTransferList.Rows.Count; i++)
            {
                GridViewRow row = gvTransferList.Rows[i];
                DropDownList ddl = row.FindControl("ddlSoto") as DropDownList;
                if (ddl != null && i < list.Count)
                    list[i].SotoId = ddl.SelectedValue;
            }

            if (list.Any(x => string.IsNullOrEmpty(x.SotoId)))
            {
                alert_toast("กรุณาเลือก Soto Cell ให้ครบทุกรายการ", "แจ้งเตือน", "warn", "5000");
                ScriptManager.RegisterStartupScript(this, GetType(), "hideLoadingWarn", "hideLoading();", true);
                return;
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
                        INSERT INTO History_Transfer
                        (Tool_id, Tool_sub, System_id, Fix_Asset_No, Control_No, Tool_No,
                         Old_Location1_id, Old_Location2_id, Old_Cell, Old_Station,
                         New_Location2_id, New_Section_Name, Transfer_By, Transfer_Date)
                        VALUES
                        (@ToolId, @ToolSub, @SystemId, @FixAsset, @ControlNo, @ToolNo,
                         @OldLoc1, @OldLoc2, @OldCell, @OldStation,
                         @NewLoc2, @NewSectionName, @TransferBy, GETDATE())";

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
                        cmd.Parameters.AddWithValue("@NewLoc2", Convert.ToInt32(item.SotoId));
                        cmd.Parameters.AddWithValue("@NewSectionName", (object)item.SotoId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@TransferBy", empId);
                        cmd.ExecuteNonQuery();
                    }

                    // 2) Update status = 11 + location ตาม system_id
                    if (item.SystemId == "1")
                    {
                        string strUpd = @"
                            UPDATE jig_tool_system.dbo.toolSub_list
                            SET status_id = 11,
                                location1_id = 98,
                                location2_id = @Loc2
                            WHERE tool_id = @ToolId AND toolSub_id = @ToolSub";

                        using (var cmd = new SqlCommand(strUpd, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@Loc2", Convert.ToInt32(item.SotoId));
                            cmd.Parameters.AddWithValue("@ToolId", item.ToolId);
                            cmd.Parameters.AddWithValue("@ToolSub", item.ToolSub);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else if (item.SystemId == "2")
                    {
                        string strUpd = @"
                            UPDATE tool_eduipment_system.dbo.Equipment_sub_id
                            SET status_use = 11,
                                location_id = 98,
                                cell_id = @Loc2
                            WHERE Equipment_id = @ToolId AND Equipment_sub_id = @ToolSub";

                        using (var cmd = new SqlCommand(strUpd, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@Loc2", Convert.ToInt32(item.SotoId));
                            cmd.Parameters.AddWithValue("@ToolId", item.ToolId);
                            cmd.Parameters.AddWithValue("@ToolSub", item.ToolSub);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                trans.Commit();
                alert_toast("บันทึกข้อมูลสำเร็จ", "สำเร็จ", "success", "2500");

                ViewState["TransferList"] = null;

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

        private void SyncSotoSelectionsToList(List<TransferItem> list)
        {
            for (int i = 0; i < gvTransferList.Rows.Count && i < list.Count; i++)
            {
                DropDownList ddl = gvTransferList.Rows[i].FindControl("ddlSoto") as DropDownList;
                if (ddl != null)
                {
                    list[i].SotoId = ddl.SelectedValue;
                }
            }
        }
        private List<TransferItem> GetTransferList()
        {
            return ViewState["TransferList"] as List<TransferItem> ?? new List<TransferItem>();
        }
        private void SaveTransferList(List<TransferItem> list)
        {
            ViewState["TransferList"] = list;
        }
        private void BindTransferList()
        {
            var list = GetTransferList();
            for (int i = 0; i < list.Count; i++) list[i].RowNo = i + 1;

            gvTransferList.DataSource = list;
            gvTransferList.DataBind();

            btnSave.Enabled = list.Count > 0;
            lblActionStatus.Text = list.Count > 0
                ? "มีรายการรอบันทึก " + list.Count + " รายการ"
                : "รอ Import ข้อมูล";
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

                // ✅ เรียกตรงกับ JS signature เดิม: showToast(type, msg)
                string script = "showToast('" + jsType + "', '" + jsDetail + "');";

                string key = "toast_" + DateTime.UtcNow.Ticks;
                ScriptManager.RegisterStartupScript(this, this.GetType(), key, script, true);
            }
        }

        // ── Helper : เลือก CSS class ตาม DisplayStatus ────────────────────────
        protected string GetStatusBadgeClass(string displayStatus)
        {
            if (displayStatus == "อยู่ระหว่างยืม")
                return "badge-borrowing";          // สีแดง/ส้ม

            if (displayStatus.StartsWith("Spare"))
                return "badge-spare";              // สีเหลือง (มีอยู่แล้ว)

            return "badge-normal";                 // สีเขียว (มีอยู่แล้ว)
        }



        [Serializable]
        public class TransferItem
        {
            public int RowNo { get; set; }
            public string ToolId { get; set; }
            public string ToolSub { get; set; }
            public string SystemId { get; set; }
            public string ModelId { get; set; }
            public string LocationId { get; set; }   // location1_id (system_id=1) / location_id (system_id=2)
            public string CellId { get; set; }       // location2_id (system_id=1) / cell_id (system_id=2) - ค่าเดิม
            public string StationId { get; set; }
            public string FixAssetNo { get; set; }
            public string ControlNo { get; set; }
            public string ToolNo { get; set; }
            public string DeptCode { get; set; }
            public string ModelName { get; set; }
            public string CellName { get; set; }
            public string StationName { get; set; }
            public string SotoId { get; set; } // ค่าที่เลือกใหม่ (location2_id / cell_id)
        }

    }
}