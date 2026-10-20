using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace Tool_Equipment_Control_System
{
    public partial class Picture_Update : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(
            ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        datacon dc = new datacon();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        string tool_id;
        string tool_sub;
        string sys_status;

        // nามสกุลที่รองรับทั้งหมด
        static readonly string[] AllowedExt = { ".jpg", ".jpeg", ".png" };

        protected List<string> ImagePathsMain { get; set; }
        protected List<string> ImagePathsFix { get; set; }
        protected List<string> ImagePathsother { get; set; }

        // ─────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            tool_id = Request.QueryString["tool_id"];
            tool_sub = Request.QueryString["tool_sub"];
            sys_status = Request.QueryString["sys_status"];

            if (!IsPostBack)
            {
                ViewState["OtherNew"] = "";
                ViewState["num"] = "";
                ViewState["URl_pic"] = "";
            }

            Get_data();
            RegisterPostBackControl();
        }

        private void RegisterPostBackControl()
        {
            var sm = ScriptManager.GetCurrent(this);
            sm.RegisterPostBackControl(btn_save_Sub);
            sm.RegisterPostBackControl(btn_save_other);
            sm.RegisterPostBackControl(btn_save_Main);
            //sm.RegisterPostBackControl(btn_delete_pictrueOther);
        }

        // ─────────────────────────────────────────
        protected void Get_data()
        {
            string str = "SELECT * FROM Tool_Equipment_List_View"
                       + " WHERE tool_id = " + tool_id
                       + " AND sub_no = " + tool_sub
                       + " AND sys_status = " + sys_status;
            dc.QryDT(conn, str);

            if (dc.dt.Rows.Count > 0)
            {
                txt_id.Text = dc.dt.Rows[0]["ID"].ToString();
                txt_toolno_model.Text = dc.dt.Rows[0]["tool_no"].ToString();
                txt_no.Text = dc.dt.Rows[0]["sub_no"].ToString();
                txt_fixassetNo.Text = dc.dt.Rows[0]["FixAcces"].ToString();
                txt_subFix.Text = dc.dt.Rows[0]["sub_fix"].ToString();
                sys_status = dc.dt.Rows[0]["sys_status"].ToString();
            }

            // ── แสดง Section Fix Asset เฉพาะเมื่อมีคำว่า "BFM" ──
            string fixVal = txt_fixassetNo.Text.Trim();
            pnl_fixasset_section.Visible =
                fixVal.IndexOf("BFM", StringComparison.OrdinalIgnoreCase) >= 0;

            Get_picture();
        }

        // ─────────────────────────────────────────
        protected void Get_picture()
        {
            var fs = new Get_Folder_Structure_Picture(tool_id, tool_sub, sys_status);

            string pathMainView = fs.get_folder_root_view_toolMain();
            string folderSub = fs.get_folder_root();
            string pathSubView = fs.get_folder_root_view_toolSub();
            string folderOther = fs.get_folder_root_toolOther();
            string pathOtherView = fs.get_folder_root_view_toolOther();

            string cacheBuster = DateTime.Now.Ticks.ToString();
            string fixAsset = txt_fixassetNo.Text.Trim();

            ImagePathsMain = new List<string>();
            ImagePathsFix = new List<string>();
            ImagePathsother = new List<string>();

            // ── Main ──────────────────────────────
            ImagePathsMain.Add(
                pathMainView != "" ? pathMainView + "?v=" + cacheBuster : "images/FilesType/jpeg.png");

            // ── Sub Fix : รองรับ .jpg และ .png ────
            // รวมไฟล์ทุกสกุลที่รองรับ
            var filesSub = new List<string>();
            foreach (string ext in AllowedExt)
                filesSub.AddRange(Directory.GetFiles(folderSub, "* Sub*" + ext, SearchOption.TopDirectoryOnly));

            int maxSub = int.Parse(txt_subFix.Text == "" ? "0" : txt_subFix.Text);

            // Map subNo -> view path  (ถ้ามีซ้ำต่างสกุลเก็บอันแรก — ไม่ควรเกิดหลังจาก Save ที่ลบสกุลเก่าแล้ว)
            var subMap = new Dictionary<int, string>();
            foreach (string fullPath in filesSub)
            {
                string fname = Path.GetFileNameWithoutExtension(fullPath);
                for (int n = maxSub; n >= 1; n--)
                {
                    if (fname.EndsWith("Sub" + n, StringComparison.OrdinalIgnoreCase)
                     || fname.EndsWith("Sub " + n, StringComparison.OrdinalIgnoreCase)
                     || fname.EndsWith(" Sub" + n, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!subMap.ContainsKey(n))
                            subMap[n] = pathSubView + "/" + Path.GetFileName(fullPath) + "?v=" + cacheBuster;
                        break;
                    }
                }
            }

            for (int j = 1; j <= maxSub; j++)
                ImagePathsFix.Add(subMap.ContainsKey(j) ? subMap[j] : "images/FilesType/jpeg.png");

            // ── Other : รองรับ .jpg และ .png ──────
            var filesOther = new List<string>();
            foreach (string ext in AllowedExt)
                filesOther.AddRange(Directory.GetFiles(folderOther, "*" + ext, SearchOption.TopDirectoryOnly));

            // เรียงตามชื่อ กัน order สลับ
            filesOther.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (string f in filesOther)
                ImagePathsother.Add(pathOtherView + Path.GetFileName(f));

            rptImages.DataSource = ImagePathsMain;
            rptImages.DataBind();

            rptSub.DataSource = ImagePathsFix;
            rptSub.DataBind();

            rptOther.DataSource = ImagePathsother;
            rptOther.DataBind();
        }

        // ─────────────────────────────────────────
        // SAVE MAIN
        // ─────────────────────────────────────────
        protected void btn_save_Main_Click(object sender, EventArgs e)
        {
            if (!UploadImg_main.HasFile) { Alert("กรุณาเลือกไฟล์รูปก่อน"); return; }

            string ext = Path.GetExtension(UploadImg_main.FileName).ToLowerInvariant();
            if (!IsAllowedExt(ext)) { Alert("รองรับเฉพาะไฟล์ .jpg หรือ .png เท่านั้น"); return; }

            try
            {
                var fs = new Get_Folder_Structure_Picture(tool_id, tool_sub, sys_status);
                string baseNoExt = RemoveExtension(fs.folder_root_toolMain());
                string savePath = baseNoExt + ext;

                // ลบทุกสกุลที่ชื่อเดียวกัน (ป้องกันไฟล์ค้าง)
                DeleteAllExtensions(baseNoExt);

                UploadImg_main.SaveAs(savePath);
                AlertReload("อัปโหลด Picture Main สำเร็จ");
            }
            catch (Exception ex) { Alert(ex.Message); }
        }

        // ─────────────────────────────────────────
        // SAVE SUB
        // ─────────────────────────────────────────
        protected void btn_save_Sub_Click(object sender, EventArgs e)
        {
            int maxSub = 0;
            int.TryParse(txt_subFix.Text ?? "0", out maxSub);

            int subNo = 0;
            int.TryParse(hid_selectedSub.Value ?? "0", out subNo);

            string noofTool = txt_no.Text.Trim();

            if (subNo <= 0 || subNo > maxSub)
            {
                Alert("ค่า Sub ไม่ถูกต้อง (" + subNo + "/" + maxSub + ")");
                return;
            }

            if (!UploadImagesSubNew.HasFile) { Alert("กรุณาเลือกไฟล์ภาพก่อนบันทึก"); return; }

            string ext = Path.GetExtension(UploadImagesSubNew.FileName).ToLowerInvariant();
            if (!IsAllowedExt(ext)) { Alert("รองรับเฉพาะไฟล์ .jpg หรือ .png เท่านั้น"); return; }

            try
            {
                var fs = new Get_Folder_Structure_Picture(tool_id, tool_sub, sys_status);
                string baseNoExt = fs.folder_root_toolSub() + " No." + noofTool + " Sub" + subNo;
                // string baseNoExt = fs.folder_root_toolSub() + " Sub" + subNo;
                string savePath = baseNoExt + ext;

                // ลบทุกสกุลที่ชื่อเดียวกัน (ป้องกันไฟล์ค้าง)
                DeleteAllExtensions(baseNoExt);

                UploadImagesSubNew.SaveAs(savePath);
                AlertReload("อัปโหลด Sub " + subNo + " สำเร็จ");
            }
            catch (Exception ex) { Alert(ex.Message); }
        }

        // ─────────────────────────────────────────
        // PICTURE OTHER - Open (click thumbnail)
        // ─────────────────────────────────────────
        protected void btn_pictureOther_Click(object sender, EventArgs e)
        {
            //ImageButton btn = (ImageButton)sender;
            //imgOther.ImageUrl = btn.ImageUrl;
            //btn_delete_pictrueOther.Visible = true;

            //ScriptManager.RegisterStartupScript(this, GetType(), "showCurrent",
                //"document.getElementById('other_current_wrap').style.display='block';", true);

            //ViewState["URl_pic"] = btn.ImageUrl;
            ViewState["OtherNew"] = "1";

            OpenModal("modal_UploadPictureOther");
        }

        protected void btn_NewPictureOther_Click(object sender, EventArgs e)
        {
            //imgOther.ImageUrl = "";
            //btn_delete_pictrueOther.Visible = false;

            //ScriptManager.RegisterStartupScript(this, GetType(), "hideCurrent",
                //"document.getElementById('other_current_wrap').style.display='none';", true);

            ViewState["OtherNew"] = "1";
            OpenModal("modal_UploadPictureOther");
        }

        // ─────────────────────────────────────────
        // SAVE OTHER
        // ─────────────────────────────────────────
        protected void btn_save_other_Click(object sender, EventArgs e)
        {
            var fs = new Get_Folder_Structure_Picture(tool_id, tool_sub, sys_status);
            string folder = fs.get_folder_root_toolOther();
            string mode = ViewState["OtherNew"].ToString(); // "1" = New, "0" = Edit

            if (!FileUploadOther.HasFile) { Alert("กรุณาเลือกไฟล์ภาพก่อนบันทึก"); return; }

            // ─ Edit (replace) ─────────────────────────────────────
            if (mode == "0")
            {
                // ลบรูปเดิม (ไฟล์ที่กดมา)
                string oldUrl = ViewState["URl_pic"].ToString();
                string oldName = Path.GetFileName(oldUrl.Split('?')[0]); // ตัด query string ออก
                string oldPath = folder + @"\" + oldName;
                TryDelete(oldPath);

                // ลบทุกสกุลที่ชื่อ stem เดียวกัน (กันไฟล์ต่างสกุลค้าง)
                string oldStem = folder + @"\" + Path.GetFileNameWithoutExtension(oldName);
                DeleteAllExtensions(oldStem);

                // Save รูปใหม่ (รูปแรกเท่านั้นสำหรับ Edit)
                string newExt = Path.GetExtension(FileUploadOther.PostedFile.FileName).ToLowerInvariant();
                if (!IsAllowedExt(newExt)) { Alert("รองรับเฉพาะไฟล์ .jpg หรือ .png เท่านั้น"); return; }

                string newName = Path.GetFileName(FileUploadOther.PostedFile.FileName);
                string newPath = folder + @"\" + newName;

                // ถ้าชื่อซ้ำกับไฟล์อื่น (ต่าง stem) ให้ทับได้เลย เพราะ Edit เจตนา replace
                FileUploadOther.SaveAs(newPath);
                AlertReload("แก้ไขรูป Other สำเร็จ");
                return;
            }

            // ─ New (multi-upload) ──────────────────────────────────
            if (mode == "1")
            {
                int saved = 0;
                HttpFileCollection files = Request.Files;

                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFile pf = files[i];
                    if (pf.ContentLength == 0) continue;

                    string ext = Path.GetExtension(pf.FileName).ToLowerInvariant();
                    if (!IsAllowedExt(ext)) continue;

                    string newName = Path.GetFileName(pf.FileName);
                    string stem = Path.GetFileNameWithoutExtension(newName);
                    string stemPath = folder + @"\" + stem;

                    // ลบทุกสกุลที่ stem เดียวกันก่อน (ป้องกันชื่อซ้ำต่างสกุล)
                    DeleteAllExtensions(stemPath);

                    string savePath = folder + @"\" + newName;
                    pf.SaveAs(savePath);
                    saved++;
                }

                if (saved > 0)
                    AlertReload("อัปโหลด " + saved + " รูปสำเร็จ");
                else
                    Alert("ไม่พบไฟล์ .jpg/.png ที่ถูกต้อง");
            }
        }

        // ─────────────────────────────────────────
        // DELETE OTHER
        // ─────────────────────────────────────────
        //protected void btn_delete_pictrueOther_Click(object sender, EventArgs e)
        //{
        //    var fs = new Get_Folder_Structure_Picture(tool_id, tool_sub, sys_status);
        //    string folder = fs.get_folder_root_toolOther();
        //    string url = imgOther.ImageUrl;
        //    string name = Path.GetFileName(url.Split('?')[0]);
        //    string path = folder + @"\" + name;

        //    try
        //    {
        //        if (File.Exists(path))
        //        {
        //            File.Delete(path);
        //            AlertReload("ลบรูปสำเร็จ");
        //        }
        //        else
        //        {
        //            Alert("ไม่พบไฟล์รูปที่ต้องการลบ");
        //        }
        //    }
        //    catch (Exception ex) { Alert(ex.Message); }
        //}
        // -----------------------------------------
        // DELETE รูปจาก gallery ใน Modal
        // -----------------------------------------
        protected void btnDelOtherItem_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string url = btn.CommandArgument;
            string name = Path.GetFileName(url.Split('?')[0]);

            var fs = new Get_Folder_Structure_Picture(tool_id, tool_sub, sys_status);
            string folder = fs.get_folder_root_toolOther();
            string path = folder + @"\" + name;

            try { if (File.Exists(path)) File.Delete(path); }
            catch (Exception ex) { Alert(ex.Message); return; }

            // Rebind gallery ใน modal ทันที
            var newList = new List<string>();
            var filesOther = new List<string>();
            foreach (string ext in AllowedExt)
                filesOther.AddRange(Directory.GetFiles(folder, "*" + ext, SearchOption.TopDirectoryOnly));
            filesOther.Sort(StringComparer.OrdinalIgnoreCase);

            string pathOtherView = fs.get_folder_root_view_toolOther();
            string cacheBuster = DateTime.Now.Ticks.ToString();
            foreach (string f in filesOther)
                newList.Add(pathOtherView + Path.GetFileName(f) + "?v=" + cacheBuster);

            BindOtherModal(newList);

            // อัป thumbnail หน้าหลักด้วย
            rptOther.DataSource = newList;
            rptOther.DataBind();
        }
        // -----------------------------------------
        // Bind gallery ใน Modal Other
        // -----------------------------------------
        private void BindOtherModal(List<string> paths)
        {
            rptOtherModal.DataSource = paths;
            rptOtherModal.DataBind();

            lb_other_count.Text = paths.Count.ToString();
            pnl_no_other.Visible = (paths.Count == 0);
        }

        // ─────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────

        // ตรวจนามสกุล
        private bool IsAllowedExt(string ext)
        {
            return AllowedExt.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }

        // ลบทุกสกุล (.jpg .jpeg .png) ที่ stem เดียวกัน
        // baseNoExt = path ไม่มีนามสกุล เช่น "C:\pics\ToolMain"
        private void DeleteAllExtensions(string baseNoExt)
        {
            foreach (string ext in AllowedExt)
            {
                string candidate = baseNoExt + ext;
                TryDelete(candidate);
            }
        }

        private void TryDelete(string filePath)
        {
            try { if (File.Exists(filePath)) File.Delete(filePath); }
            catch { /* ignore */ }
        }

        // ตัดนามสกุลออก เผื่อ folder_root_toolMain() คืนมาพร้อมนามสกุลแล้ว
        private string RemoveExtension(string path)
        {
            string ext = Path.GetExtension(path);
            return ext != "" ? Path.ChangeExtension(path, null) : path;
        }

        private void Alert(string msg)
        {
            sb.Clear();
            sb.Append("<script type='text/javascript'>");
            sb.Append("alert('" + msg.Replace("'", "\\'") + "');");
            sb.Append("</script>");
            ScriptManager.RegisterClientScriptBlock(this, GetType(), "alert", sb.ToString(), false);
        }

        private void AlertReload(string msg)
        {
            sb.Clear();
            sb.Append("<script type='text/javascript'>");
            sb.Append("alert('" + msg.Replace("'", "\\'") + "');");
            sb.Append("window.location.href = window.location.href;");
            sb.Append("</script>");
            ScriptManager.RegisterClientScriptBlock(this, GetType(), "alertReload", sb.ToString(), false);
        }

        private void OpenModal(string modalId)
        {
            sb.Clear();
            sb.Append("<script type='text/javascript'>");
            sb.Append("$('#" + modalId + "').modal('show');");
            sb.Append("</script>");
            ScriptManager.RegisterClientScriptBlock(this, GetType(), "modal_" + modalId, sb.ToString(), false);
        }
    }
}