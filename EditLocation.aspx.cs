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

namespace Tool_Equipment_Control_System
{
    public partial class EditLocation : System.Web.UI.Page
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
            Page.Title = "Edit Location";
            if (Session["emp_id"] == null || Session["emp_id"].ToString() == "")
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                ViewState["DEPT_MASTER"] = GetDeptMaster();
                ViewState["MODEL_MASTER"] = GetModelMaster();
                //ViewState["RESP_MASTER"] = GetResponsibleMaster();
                ViewState["STATUS_MASTER"] = GetStatusMaster();
            }

            // Visible ไม่ถูกเก็บใน ViewState โดยอัตโนมัติ ต้อง set ทุก request
            editItemCard.Visible = (GetCurrentEditItem() != null);
        }

        // ══════════════════════════════
        //  MASTER DATA
        // ══════════════════════════════
        private DataTable GetDeptMaster()
        {
            string str_dept =
                "select LTRIM(RTRIM(dept_code)) as dept_code" +
                " from View_dept_master" +
                " where division = 2" +
                " order by dept_code";
            dc.QryDT(conn, str_dept);
            return dc.dt;
        }
        private DataTable GetModelMaster()
        {
            string str_model =
                "Select * From jig_tool_system.dbo.draw_master_model" +
                " where model_check = 1 order by model_name";
            dc.QryDT(conn, str_model);
            return dc.dt;
        }

        // ── Responsible master : Responsible_Dept (Value = emp_id, Text = pic_dept) ──
        //private DataTable GetResponsibleMaster()
        //{
        //    string str_resp =
        //        //"select emp_id, pic_dept" +
        //        //" from tool_eduipment_system.dbo.Responsible_Dept" +
        //        //" where emp_id is not null and pic_dept is not null" +
        //        //" order by pic_dept";
        //        " select emp_id, emp_id + ' ' + pic_dept + ' (' + CONVERT(nvarchar(4), dept_code_real) + ')'  as pic_dept" +
        //        " from tool_eduipment_system.dbo.Responsible_Dept" +
        //        " where emp_id is not null and pic_dept is not null" +
        //        " group by emp_id , pic_dept , dept_code_real" +
        //        " order by emp_id";

        //    dc.QryDT(conn, str_resp);
        //    return dc.dt;
        //}
        // เปลี่ยน GetResponsibleMaster() ให้รับ deptCode มากรอง (Req.2)
        private DataTable GetResponsibleMaster(string deptCode)
        {
            string str_resp =
                "select emp_id, pic_dept + ' (' + TRY_CAST(dept_code_real AS nvarchar(4)) + ')' as pic_dept" +
                " from tool_eduipment_system.dbo.Responsible_Dept" +
                " where emp_id is not null and pic_dept is not null" +
                " and dept_code = TRY_CAST(@dept AS INT)" +
                " order by pic_dept";
            dc.QryDT_P(conn, str_resp,
                new SqlParameter("@dept", SqlDbType.NVarChar) { Value = deptCode });
            return dc.dt;
        }
        // Req.1/2 : ซ่อน/แสดง + โหลด Responsible ตาม Dept ที่เลือก
        private void LoadResponsibleForDept(string deptCode, string selectResponsibleId)
        {
            ddlNewResponsible.Items.Clear();

            if (string.IsNullOrEmpty(deptCode))
            {
                ddlNewResponsible.Visible = false;
                lblRespPlaceholder.Visible = true;
                return;
            }

            ddlNewResponsible.Items.Add(new ListItem("-- เลือก Responsible --", ""));
            DataTable respMaster = GetResponsibleMaster(deptCode);
            foreach (DataRow dr in respMaster.Rows)
                ddlNewResponsible.Items.Add(new ListItem(dr["pic_dept"].ToString(), dr["emp_id"].ToString()));

            if (!string.IsNullOrEmpty(selectResponsibleId) && ddlNewResponsible.Items.FindByValue(selectResponsibleId) != null)
                ddlNewResponsible.SelectedValue = selectResponsibleId;

            ddlNewResponsible.Visible = true;
            lblRespPlaceholder.Visible = false;
        }

        // Event ของ ddlNewDept (AutoPostBack)
        protected void ddlNewDept_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadResponsibleForDept(ddlNewDept.SelectedValue, null);
        }

        // ── Status master : tool_equipment_status (Value = status_id, Text = status_name) ──
        // ⚠️ ASSUMPTION : ตาราง tool_equipment_status อยู่บน connection เดียวกับ master data อื่นๆ (conn)
        //    หากอยู่คนละ database กรุณาเติม prefix database ให้ตรงกับ schema จริง
        private DataTable GetStatusMaster()
        {
            string str_status =
                "select status_id, status_name" +
                " from tool_equipment_status order by status_id";
            dc.QryDT(conn, str_status);
            return dc.dt;
        }

        // ✅ Cell / Station ผูกกับ location1_id เดิมของอุปกรณ์แต่ละตัว (ไม่ hardcode)
        private DataTable GetCellMaster(string locationId)
        {
            string str_cell =
                "select location2_id, location2_name" +
                " from jig_tool_system.dbo.master_location2" +
                " where is_use = 1 and location1_id = @location1_id and location2_id != 0" +
                " order by location2_name";
            dc.QryDT_P(conn, str_cell,
                new SqlParameter("@location1_id", SqlDbType.NVarChar) { Value = locationId });
            return dc.dt;
        }

        private DataTable GetStationMaster(string locationId)
        {
            string str_station =
                "select location3_id, location3_name" +
                " from jig_tool_system.dbo.master_location3" +
                " where location1_id = @location1_id and location3_id != 0" +
                " order by location3_name";
            dc.QryDT_P(conn, str_station,
                new SqlParameter("@location1_id", SqlDbType.NVarChar) { Value = locationId });
            return dc.dt;
        }

        // ── หา Responsible ปัจจุบันของอุปกรณ์ จาก rsp_empID ตัวจริงของแถวอุปกรณ์นั้น ──
        // (แก้ไข : เดิมเทียบผ่าน dept_code_real ซึ่งไม่ตรงกับ Responsible จริงของอุปกรณ์แต่ละตัว
        //  ตอนนี้ item.OldResponsibleId ถูกเซ็ตมาจากคอลัมน์ rsp_empID ของ Tool_Equipment_List_View แล้ว
        //  ฟังก์ชันนี้มีหน้าที่แค่ resolve ชื่อ (pic_dept) จาก emp_id นั้น)
        private void FillCurrentResponsible(EditLocationItem item)
        {
            item.OldResponsibleName = "";

            if (string.IsNullOrEmpty(item.OldResponsibleId)) return;

            string str_resp =
                "select top 1 emp_id, pic_dept" +
                " from tool_eduipment_system.dbo.Responsible_Dept" +
                " where emp_id = @emp_id";

            dc.QryDT_P(conn, str_resp,
                new SqlParameter("@emp_id", SqlDbType.NVarChar) { Value = item.OldResponsibleId });

            if (dc.dt.Rows.Count > 0)
            {
                item.OldResponsibleName = dc.dt.Rows[0]["pic_dept"].ToString();
            }
        }

        // ══════════════════════════════
        //  SEARCH
        // ══════════════════════════════
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            SearchData();
        }
        private void SearchData()
        {
            if (string.IsNullOrEmpty(txtFixAsset.Text))
            {
                alert_toast("กรุณากรอกข้อมูลที่ต้องการค้นหา", "แจ้งเตือน", "warn", "5000");
                return;
            }

            string dept_check = txtFixAsset.Text.Trim();

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
                        Button btnEdit = e.Row.Cells[1].Controls
                            .OfType<Button>()
                            .FirstOrDefault();

                        if (btnEdit != null)
                        {
                            btnEdit.Enabled = false;
                            btnEdit.ToolTip = "อุปกรณ์นี้อยู่ระหว่างการยืม";
                            btnEdit.CssClass = "btn btn-secondary btn-sm";
                        }
                    }
                }
            }
        }
        protected void gvSearchResult_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "EditItem") return;

            int rowIndex = Convert.ToInt32(e.CommandArgument);
            GridViewRow row = gvSearchResult.Rows[rowIndex];

            HiddenField hdtoolId = (HiddenField)row.FindControl("hdtoolId");
            HiddenField hdtoolSub = (HiddenField)row.FindControl("hdtoolSub");
            HiddenField hdsystemId = (HiddenField)row.FindControl("hdsystemId");

            if (hdtoolId == null || hdtoolSub == null || hdsystemId == null)
            {
                alert_toast("ข้อมูลในตารางผิดพลาด", "แจ้งเตือน", "error", "5000");
                return;
            }

            string toolId = hdtoolId.Value;
            string toolSub = hdtoolSub.Value;
            string systemId = hdsystemId.Value;

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

            // หมายเหตุ: การยืนยัน "ทับรายการเดิมที่ยังไม่ Save" ทำที่ฝั่ง Client (confirmEditReplace)
            // ก่อน submit แล้ว ดังนั้นเมื่อ RowCommand นี้ทำงาน แปลว่าผู้ใช้ยืนยันแล้ว
            var item = new EditLocationItem
            {
                ToolId = toolId,
                ToolSub = toolSub,
                SystemId = systemId,
                FixAssetNo = dr["FixAcces"].ToString(),
                ControlNo = dr["control_no"].ToString(),
                ToolNo = dr["tool_no"].ToString(),

                // ⚠️ Req.1.1.1 : สถานะปัจจุบันดึงตรงจาก Tool_Equipment_List_View [status_id]/[status]
                OldStatusId = dtSearch.Columns.Contains("status_id") && dr["status_id"] != DBNull.Value
                    ? dr["status_id"].ToString() : "",
                OldStatusName = dtSearch.Columns.Contains("status") && dr["status"] != DBNull.Value
                    ? dr["status"].ToString() : "",

                OldDeptCode = dr["dept_code"].ToString(),
                // ⚠️ ต้องมีคอลัมน์ rsp_empID ใน Tool_Equipment_List_View (ผูกกับ rsp_empID จริงของ toolSub_list / Equipment_sub_id)
                OldResponsibleId = dtSearch.Columns.Contains("rsp_empID") && dr["rsp_empID"] != DBNull.Value
                    ? dr["rsp_empID"].ToString() : "",
                OldModelId = dr["model_id"].ToString(),
                OldModelName = dr["model_name"].ToString(),
                OldLocation1Id = dr["location1_id"].ToString(),
                OldCellId = dr["location2_id"].ToString(),
                OldCellName = dr["cell"].ToString(),
                OldStationId = dr["station_id"].ToString(),
                OldStationName = dr["station"].ToString(),
                OldDueDate = dr["DueDate1"].ToString(),
                OldRemark = dr["Remark"].ToString()
            };

            FillCurrentResponsible(item);
            SetCurrentEditItem(item);
            BindEditItem();
            alert_toast("โหลดรายการสำหรับแก้ไข Location เรียบร้อย", "สำเร็จ", "success", "3000");

            //txtFixAsset.Text = "";
            //ViewState["SearchResult"] = null;
            //gvSearchResult.DataSource = null;
            //gvSearchResult.DataBind();
        }

        // ══════════════════════════════
        //  EDIT ITEM (ทีละ 1 รายการ)
        // ══════════════════════════════
        protected void btnClearEditItem_Click(object sender, EventArgs e)
        {
            SetCurrentEditItem(null);
            BindEditItem();
            alert_toast("ยกเลิกรายการที่กำลังแก้ไขแล้ว", "แจ้งเตือน", "info", "2500");
        }

        private void BindEditItem()
        {
            var item = GetCurrentEditItem();

            if (item == null)
            {
                editItemCard.Visible = false;
                btnSave.Enabled = false;
                lblActionStatus.Text = "รอเลือกรายการสำหรับแก้ไข Location";
                return;
            }

            txtDueDate.Attributes["readonly"] = "readonly";
            editItemCard.Visible = true;

            lblFixAsset.Text = item.FixAssetNo;
            lblControlNo.Text = item.ControlNo;
            lblToolNo.Text = item.ToolNo;
            lblToolSub.Text = item.ToolSub;

            lblCurStatus.Text = string.IsNullOrEmpty(item.OldStatusName) ? "-" : item.OldStatusName;
            lblCurDept.Text = string.IsNullOrEmpty(item.OldDeptCode) ? "-" : item.OldDeptCode;
            lblCurResponsible.Text = string.IsNullOrEmpty(item.OldResponsibleName) ? "-" : item.OldResponsibleName;
            lblCurModel.Text = string.IsNullOrEmpty(item.OldModelName) ? "-" : item.OldModelName;
            lblCurCell.Text = string.IsNullOrEmpty(item.OldCellName) ? "-" : item.OldCellName;
            lblCurStation.Text = string.IsNullOrEmpty(item.OldStationName) ? "-" : item.OldStationName;
            lblCurDueDate.Text = string.IsNullOrEmpty(item.OldDueDate) ? "-" : item.OldDueDate;
            lblCurRemark.Text = string.IsNullOrEmpty(item.OldRemark) ? "-" : item.OldRemark;

            // ── Status dropdown (Req.1.1.2 : dropdown ธรรมดา ไม่มี Search, ไม่บังคับเลือก) ──
            DataTable statusMaster = ViewState["STATUS_MASTER"] as DataTable;
            ddlNewStatus.Items.Clear();
            ddlNewStatus.Items.Add(new ListItem("-- เลือก Status --", ""));
            if (statusMaster != null)
                foreach (DataRow dr in statusMaster.Rows)
                    ddlNewStatus.Items.Add(new ListItem(dr["status_name"].ToString(), dr["status_id"].ToString()));
            //if (!string.IsNullOrEmpty(item.OldStatusId) && ddlNewStatus.Items.FindByValue(item.OldStatusId) != null)
            //    ddlNewStatus.SelectedValue = item.OldStatusId;

            // ── Dept dropdown (default = ค่าปัจจุบัน) ──
            DataTable deptMaster = ViewState["DEPT_MASTER"] as DataTable;
            ddlNewDept.Items.Clear();
            ddlNewDept.Items.Add(new ListItem("-- เลือก Dept --", ""));
            if (deptMaster != null)
                foreach (DataRow dr in deptMaster.Rows)
                    ddlNewDept.Items.Add(new ListItem(dr["dept_code"].ToString(), dr["dept_code"].ToString()));
            //if (!string.IsNullOrEmpty(item.OldDeptCode) && ddlNewDept.Items.FindByValue(item.OldDeptCode) != null)
            //    ddlNewDept.SelectedValue = item.OldDeptCode;

            // ── Responsible dropdown ──
            DataTable respMaster = ViewState["RESP_MASTER"] as DataTable;
            ddlNewResponsible.Items.Clear();
            ddlNewResponsible.Items.Add(new ListItem("-- เลือก Responsible --", ""));
            if (respMaster != null)
                foreach (DataRow dr in respMaster.Rows)
                    ddlNewResponsible.Items.Add(new ListItem(dr["pic_dept"].ToString(), dr["emp_id"].ToString()));
            //if (!string.IsNullOrEmpty(item.OldResponsibleId) && ddlNewResponsible.Items.FindByValue(item.OldResponsibleId) != null)
            //    ddlNewResponsible.SelectedValue = item.OldResponsibleId;

            // ── Model dropdown ──
            DataTable modelMaster = ViewState["MODEL_MASTER"] as DataTable;
            ddlNewModel.Items.Clear();
            ddlNewModel.Items.Add(new ListItem("-- เลือก Model --", ""));
            if (modelMaster != null)
                foreach (DataRow dr in modelMaster.Rows)
                    ddlNewModel.Items.Add(new ListItem(dr["model_name"].ToString(), dr["model_id"].ToString()));
            //if (!string.IsNullOrEmpty(item.OldModelId) && ddlNewModel.Items.FindByValue(item.OldModelId) != null)
            //    ddlNewModel.SelectedValue = item.OldModelId;

            // ── Cell dropdown (ผูกกับ location1_id เดิมของอุปกรณ์ตัวนี้) ──
            ddlNewCell.Items.Clear();
            ddlNewCell.Items.Add(new ListItem("-- เลือก Cell --", ""));
            try
            {
                DataTable dtCell = GetCellMaster(item.OldLocation1Id);
                foreach (DataRow dr in dtCell.Rows)
                    ddlNewCell.Items.Add(new ListItem(dr["location2_name"].ToString(), dr["location2_id"].ToString()));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetCellMaster error: " + ex.Message);
            }
            //if (!string.IsNullOrEmpty(item.OldCellId) && ddlNewCell.Items.FindByValue(item.OldCellId) != null)
            //    ddlNewCell.SelectedValue = item.OldCellId;

            // ── Station dropdown (ผูกกับ location1_id เดิมของอุปกรณ์ตัวนี้) ──
            ddlNewStation.Items.Clear();
            ddlNewStation.Items.Add(new ListItem("-- เลือก Station --", ""));
            try
            {
                DataTable dtStation = GetStationMaster(item.OldLocation1Id);
                foreach (DataRow dr in dtStation.Rows)
                    ddlNewStation.Items.Add(new ListItem(dr["location3_name"].ToString(), dr["location3_id"].ToString()));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetStationMaster error: " + ex.Message);
            }
            //if (!string.IsNullOrEmpty(item.OldStationId) && ddlNewStation.Items.FindByValue(item.OldStationId) != null)
            //    ddlNewStation.SelectedValue = item.OldStationId;

            // ── รีเซ็ตโหมด manual textbox กลับเป็น dropdown ทุกครั้งที่โหลดรายการใหม่ ──
            ResetFieldToDropdownMode(ddlNewModel, txtNewModel, hdModeModel);
            ResetFieldToDropdownMode(ddlNewCell, txtNewCell, hdModeCell);
            ResetFieldToDropdownMode(ddlNewStation, txtNewStation, hdModeStation);
            txtDueDate.Text = "";
            txtRemark.Text = string.IsNullOrEmpty(item.OldRemark) ? "-" : item.OldRemark;
            hdModeDueDate.Value = "off";

            btnSave.Enabled = true;
            lblActionStatus.Text = "กำลังแก้ไขรายการ : " + item.ToolNo + " (No " + item.ToolSub + ")";

            ScriptManager.RegisterStartupScript(this, GetType(), "syncRemarkCount", "if(typeof updateRemarkCount==='function'){updateRemarkCount();}", true);
        }

        private void ResetFieldToDropdownMode(DropDownList ddl, TextBox txt, HiddenField hdMode)
        {
            hdMode.Value = "ddl";
            txt.Text = "";
            txt.Style["display"] = "none";
            ddl.Style["display"] = "";
        }

        // ⚠️ ASSUMPTION (ข้อ 1.2.1.2 / 1.2.1.3): การเพิ่ม Cell/Station แบบ Manual
        // จะผูกกับ location1_id = 97 ตามที่ระบุมาตรงๆ ในสเปก (ไม่ใช้ item.OldLocation1Id)
        // หากที่จริงต้องอิงจาก location1_id ของอุปกรณ์แต่ละตัว กรุณาแก้ค่าคงที่นี้
        private const int MANUAL_UPSERT_LOCATION1_ID = 97;

        // ── แปลง string เป็น int แบบปลอดภัย ใช้ตอน fallback กลับไปใช้ค่าเดิม (Old*) เมื่อ field นั้นไม่ได้ถูกเลือก/กรอก ──
        private int ParseIntOrZero(string s)
        {
            int v;
            return int.TryParse(s, out v) ? v : 0;
        }

        // ══════════════════════════════
        //  SAVE (เรียกจากปุ่มซ่อน btnSaveConfirmed หลังผู้ใช้กด "ยืนยันบันทึก" ใน Modal)
        //  หมายเหตุ (แก้ไขเพิ่มเติม) :
        //   1) ไม่ต้องแจ้งเตือน/บังคับให้เลือกครบทุก field อีกต่อไป
        //   2) field ไหนไม่ได้เลือก/กรอก -> คงค่าเดิมไว้ (ไม่ Update field นั้น)
        //   3) Update เฉพาะ field ที่มีการเลือก/กรอกค่าใหม่จริงเท่านั้น
        // ══════════════════════════════
        protected void btnSaveConfirmed_Click(object sender, EventArgs e)
        {
            var item = GetCurrentEditItem();
            if (item == null)
            {
                alert_toast("ไม่มีรายการสำหรับบันทึก", "แจ้งเตือน", "warn", "4000");
                return;
            }

            // ── Status : Dropdown เท่านั้น (ไม่บังคับเลือก ถ้าไม่เลือกให้คงค่าเดิม) ──
            string statusIdToSave = !string.IsNullOrEmpty(ddlNewStatus.SelectedValue) ? ddlNewStatus.SelectedValue : item.OldStatusId;
            string statusNameToSave = !string.IsNullOrEmpty(ddlNewStatus.SelectedValue) && ddlNewStatus.SelectedItem != null
                ? ddlNewStatus.SelectedItem.Text : item.OldStatusName;
            item.NewStatusId = statusIdToSave;
            item.NewStatusName = statusNameToSave;

            // ── Dept : ถ้าไม่เลือก ให้คงค่าเดิม ──
            item.NewDeptCode = !string.IsNullOrEmpty(ddlNewDept.SelectedValue)
                ? ddlNewDept.SelectedValue
                : item.OldDeptCode;

            // ── Responsible : ถ้าไม่เลือก ให้คงค่าเดิม ──
            if (!string.IsNullOrEmpty(ddlNewResponsible.SelectedValue))
            {
                item.NewResponsibleId = ddlNewResponsible.SelectedValue;
                item.NewResponsibleName = ddlNewResponsible.SelectedItem != null ? ddlNewResponsible.SelectedItem.Text : "";
            }
            else
            {
                item.NewResponsibleId = item.OldResponsibleId;
                item.NewResponsibleName = item.OldResponsibleName;
            }

            bool modelManual = hdModeModel.Value == "txt";
            bool cellManual = hdModeCell.Value == "txt";
            bool stationManual = hdModeStation.Value == "txt";

            string modelManualName = txtNewModel.Text.Trim();
            string cellManualName = txtNewCell.Text.Trim();
            string stationManualName = txtNewStation.Text.Trim();

            string empId = Session["emp_id"].ToString();

            // --- Due Date
            // ใหม่: ถ้าโหมดปิด -> คงค่าเดิม (ไม่แตะ duedate)
            bool dueDateEdit = hdModeDueDate.Value == "on";
            object dueDateValue;
            if (dueDateEdit)
            {
                item.NewDueDate = txtDueDate.Text.Trim();
                dueDateValue = ParseDueDate(txtDueDate.Text);
            }
            else
            {
                item.NewDueDate = item.OldDueDate;      // history เก็บค่าเดิม
                dueDateValue = ParseDueDate(item.OldDueDate);  // update ด้วยค่าเดิม = ไม่เปลี่ยน
            }

            //alert_toast("Due Date = " + txtDueDate.Text,"Debug","warn","10000");

            DateTime oldDueDate;
            DateTime newDueDate;
            
            string remark = txtRemark.Text.Trim();
            if (remark.Length > 200)
            {
                alert_toast("Remark ต้องไม่เกิน 200 ตัวอักษร","แจ้งเตือน","warn","4000");
                return;
            }
            if (remark.ToString() != item.OldRemark.ToString())
            {
                item.NewRemark = txtRemark.Text.Trim();
            }
            else
            {
                item.NewRemark = item.OldRemark;      // history เก็บค่าเดิม
            }

            conn.Open();
            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                // ── 1.3.x : Resolve Dept -> dept_id, division (จาก View_dept_master) ──
                string deptId; int division;
                if (!TryResolveDept(item.NewDeptCode, trans, out deptId, out division))
                {
                    trans.Rollback();
                    conn.Close();
                    alert_toast("ไม่พบ Dept นี้ใน View_dept_master กรุณาตรวจสอบข้อมูล Dept", "ผิดพลาด", "error", "6000");
                    ScriptManager.RegisterStartupScript(this, GetType(), "hideLoadingErrDept", "hideLoading();", true);
                    return;
                }

                // ── 1.2.1.1 : Resolve/Upsert Model (เฉพาะกรณีมีการเลือก/กรอก ไม่งั้นคงค่าเดิม) ──
                int modelId;
                if (modelManual && !string.IsNullOrEmpty(modelManualName))
                {
                    modelId = GetOrCreateModelId(modelManualName, trans);
                    item.NewModelName = modelManualName;
                }
                else if (!modelManual && !string.IsNullOrEmpty(ddlNewModel.SelectedValue))
                {
                    modelId = Convert.ToInt32(ddlNewModel.SelectedValue);
                    item.NewModelName = ddlNewModel.SelectedItem.Text;
                }
                else
                {
                    // ไม่ได้เลือก/กรอก -> คงค่าเดิม ไม่ต้อง Upsert
                    modelId = ParseIntOrZero(item.OldModelId);
                    item.NewModelName = item.OldModelName;
                }
                item.NewModelId = modelId.ToString();

                // ── 1.2.1.2 : Resolve/Upsert Cell (ต้องทำก่อน Station เพราะ master_location3 ผูกกับ location2_id) ──
                int cellId;
                if (cellManual && !string.IsNullOrEmpty(cellManualName))
                {
                    cellId = GetOrCreateCellId(cellManualName, MANUAL_UPSERT_LOCATION1_ID, trans);
                    item.NewCellName = cellManualName;
                }
                else if (!cellManual && !string.IsNullOrEmpty(ddlNewCell.SelectedValue))
                {
                    cellId = Convert.ToInt32(ddlNewCell.SelectedValue);
                    item.NewCellName = ddlNewCell.SelectedItem.Text;
                }
                else
                {
                    // ไม่ได้เลือก/กรอก -> คงค่าเดิม ไม่ต้อง Upsert
                    cellId = ParseIntOrZero(item.OldCellId);
                    item.NewCellName = item.OldCellName;
                }
                item.NewCellId = cellId.ToString();

                // ── 1.2.1.3 : Resolve/Upsert Station ──
                int stationId;
                if (stationManual && !string.IsNullOrEmpty(stationManualName))
                {
                    stationId = GetOrCreateStationId(stationManualName, MANUAL_UPSERT_LOCATION1_ID, 1, trans);
                    item.NewStationName = stationManualName;
                }
                else if (!stationManual && !string.IsNullOrEmpty(ddlNewStation.SelectedValue))
                {
                    stationId = Convert.ToInt32(ddlNewStation.SelectedValue);
                    item.NewStationName = ddlNewStation.SelectedItem.Text;
                }
                else
                {
                    // ไม่ได้เลือก/กรอก -> คงค่าเดิม ไม่ต้อง Upsert
                    stationId = ParseIntOrZero(item.OldStationId);
                    item.NewStationName = item.OldStationName;
                }
                item.NewStationId = stationId.ToString();

                // ── 1.3 : Update ตัวจริง (field ที่ไม่เลือกจะถูก SET ด้วยค่าเดิมของมันเอง = ไม่เปลี่ยนแปลง) ──
                if (item.SystemId == "1")
                {
                    string strUpd = @"
                        UPDATE jig_tool_system.dbo.toolSub_list
                        SET dept_code = @DeptId,
                            division = @Division,
                            rsp_empID = @RespId,
                            model_id = @ModelId,
                            location2_id = @CellId,
                            location3_id = @StationId,
                            status_id = @StatusId,
                            duedate = @DueDate,
                            Remark = @Remark
                        WHERE tool_id = @ToolId AND toolSub_id = @ToolSub";

                    int rowsAffectedMain;
                    using (var cmd = new SqlCommand(strUpd, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@DeptId", deptId);
                        cmd.Parameters.AddWithValue("@Division", division);
                        cmd.Parameters.AddWithValue("@RespId", item.NewResponsibleId);
                        cmd.Parameters.AddWithValue("@ModelId", modelId);
                        cmd.Parameters.AddWithValue("@CellId", cellId);
                        cmd.Parameters.AddWithValue("@StationId", stationId);
                        cmd.Parameters.AddWithValue("@StatusId", string.IsNullOrEmpty(statusIdToSave) ? (object)DBNull.Value : Convert.ToInt32(statusIdToSave));
                        cmd.Parameters.AddWithValue("@DueDate", dueDateValue);
                        cmd.Parameters.AddWithValue("@Remark", remark);
                        cmd.Parameters.AddWithValue("@ToolId", item.ToolId);
                        cmd.Parameters.AddWithValue("@ToolSub", item.ToolSub);
                        rowsAffectedMain = cmd.ExecuteNonQuery();
                    }
                    // ── 7.1 : ต้อง Update ได้จริง (พบแถวที่ตรงกัน) ถึงจะให้ไปต่อ ไม่งั้น rollback ทั้งหมด ──
                    if (rowsAffectedMain != 1)
                        throw new Exception("ไม่พบข้อมูลอุปกรณ์นี้ใน toolSub_list (Update ไม่สำเร็จ) กรุณาตรวจสอบข้อมูลอีกครั้ง");
                }
                else if (item.SystemId == "2")
                {
                    string strUpd = @"
                        UPDATE tool_eduipment_system.dbo.Equipment_sub_id
                        SET dept_code = @DeptId,
                            division = @Division,
                            rsp_empID = @RespId,
                            model_id = @ModelId,
                            cell_id = @CellId,
                            station_id = @StationId,
                            status_use = @StatusId,
                            duedate = @DueDate,
                            Remark = @Remark
                        WHERE Equipment_id = @ToolId AND Equipment_sub_id = @ToolSub";

                    int rowsAffectedMain;
                    using (var cmd = new SqlCommand(strUpd, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@DeptId", deptId);
                        cmd.Parameters.AddWithValue("@Division", division);
                        cmd.Parameters.AddWithValue("@RespId", item.NewResponsibleId);
                        cmd.Parameters.AddWithValue("@ModelId", modelId);
                        cmd.Parameters.AddWithValue("@CellId", cellId);
                        cmd.Parameters.AddWithValue("@StationId", stationId);
                        cmd.Parameters.AddWithValue("@StatusId", string.IsNullOrEmpty(statusIdToSave) ? (object)DBNull.Value : Convert.ToInt32(statusIdToSave));
                        cmd.Parameters.AddWithValue("@DueDate", dueDateValue);
                        cmd.Parameters.AddWithValue("@Remark", remark);
                        cmd.Parameters.AddWithValue("@ToolId", item.ToolId);
                        cmd.Parameters.AddWithValue("@ToolSub", item.ToolSub);                        
                        rowsAffectedMain = cmd.ExecuteNonQuery();
                    }
                    // ── 7.1 : ต้อง Update ได้จริง (พบแถวที่ตรงกัน) ถึงจะให้ไปต่อ ไม่งั้น rollback ทั้งหมด ──
                    if (rowsAffectedMain != 1)
                        throw new Exception("ไม่พบข้อมูลอุปกรณ์นี้ใน Equipment_sub_id (Update ไม่สำเร็จ) กรุณาตรวจสอบข้อมูลอีกครั้ง");
                }
                else
                {
                    // ── 7.1 : ไม่รู้จัก SystemId นี้ ห้าม Commit เพราะไม่มีตารางไหนถูก Update เลย ──
                    throw new Exception("SystemId ของรายการนี้ไม่ถูกต้อง (" + item.SystemId + ") ไม่สามารถบันทึกได้");
                }

                // ── 1.4 : Insert History (schema ตรงตาม CREATE TABLE ที่ยืนยันแล้ว) ──
                string strHistory = @"
                    INSERT INTO tool_eduipment_system.dbo.History_Change_Location
                    (Tool_id, Tool_sub, System_id, Fix_Asset_No, Control_No, Tool_No,
                     Old_Status_Id, New_Status_Id,
                     Old_DeptCode, New_DeptCode,
                     Old_Responsible, New_Responsible,
                     Old_Model_Id, Old_Model_Name, New_Model_Id, New_Model_Text, HD_Model,
                     Old_Cell_Id, Old_Cell_Name, New_Cell_Id, New_Cell_Text, HD_Cell,
                     Old_Station_Id, Old_Station_Name, New_Station_Id, New_Station_Text, HD_Station,
                     Old_Location1_id, Edit_By, Edit_Date, Old_DueDate, New_DueDate, Old_Remark, New_Remark)
                    VALUES
                    (@ToolId, @ToolSub, @SystemId, @FixAsset, @ControlNo, @ToolNo,
                     @OldStatusId, @NewStatusId,
                     @OldDept, @NewDept,
                     @OldResp, @NewResp,
                     @OldModelId, @OldModelName, @NewModelId, @NewModelText, @HDModel,
                     @OldCellId, @OldCellName, @NewCellId, @NewCellText, @HDCell,
                     @OldStationId, @OldStationName, @NewStationId, @NewStationText, @HDStation,
                     @OldLoc1, @EditBy, GETDATE() , @OldDueDate, @NewDueDate, @OldRemark, @NewRemark)";

                using (var cmd = new SqlCommand(strHistory, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@ToolId", item.ToolId);
                    cmd.Parameters.AddWithValue("@ToolSub", item.ToolSub);
                    cmd.Parameters.AddWithValue("@SystemId", item.SystemId);
                    cmd.Parameters.AddWithValue("@FixAsset", (object)item.FixAssetNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ControlNo", (object)item.ControlNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToolNo", (object)item.ToolNo ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@OldStatusId", string.IsNullOrEmpty(item.OldStatusId) ? (object)DBNull.Value : Convert.ToInt32(item.OldStatusId));
                    cmd.Parameters.AddWithValue("@NewStatusId", string.IsNullOrEmpty(item.NewStatusId) ? (object)DBNull.Value : Convert.ToInt32(item.NewStatusId));

                    cmd.Parameters.AddWithValue("@OldDept", (object)item.OldDeptCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NewDept", (object)item.NewDeptCode ?? DBNull.Value);

                    // Old_Responsible / New_Responsible เป็นคอลัมน์เดียว (nvarchar(100)) จึงรวม empId + ชื่อไว้ด้วยกัน
                    cmd.Parameters.AddWithValue("@OldResp", (object)BuildResponsibleDisplay(item.OldResponsibleId, item.OldResponsibleName) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NewResp", (object)BuildResponsibleDisplay(item.NewResponsibleId, item.NewResponsibleName) ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@OldModelId", string.IsNullOrEmpty(item.OldModelId) ? (object)DBNull.Value : item.OldModelId);
                    cmd.Parameters.AddWithValue("@OldModelName", (object)item.OldModelName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NewModelId", modelId.ToString());
                    cmd.Parameters.AddWithValue("@NewModelText", (object)item.NewModelName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HDModel", hdModeModel.Value);

                    cmd.Parameters.AddWithValue("@OldCellId", string.IsNullOrEmpty(item.OldCellId) ? (object)DBNull.Value : Convert.ToInt32(item.OldCellId));
                    cmd.Parameters.AddWithValue("@OldCellName", (object)item.OldCellName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NewCellId", cellId);
                    cmd.Parameters.AddWithValue("@NewCellText", (object)item.NewCellName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HDCell", hdModeCell.Value);

                    cmd.Parameters.AddWithValue("@OldStationId", string.IsNullOrEmpty(item.OldStationId) ? (object)DBNull.Value : Convert.ToInt32(item.OldStationId));
                    cmd.Parameters.AddWithValue("@OldStationName", (object)item.OldStationName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NewStationId", stationId);
                    cmd.Parameters.AddWithValue("@NewStationText", (object)item.NewStationName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HDStation", hdModeStation.Value);

                    cmd.Parameters.AddWithValue("@OldLoc1", string.IsNullOrEmpty(item.OldLocation1Id) ? (object)DBNull.Value : Convert.ToInt32(item.OldLocation1Id));
                    cmd.Parameters.AddWithValue("@EditBy", empId);

                    cmd.Parameters.AddWithValue("@OldDueDate", ParseDueDate(item.OldDueDate));
                    cmd.Parameters.AddWithValue("@NewDueDate", ParseDueDate(item.NewDueDate));
                    cmd.Parameters.AddWithValue("@OldRemark",(object)item.OldRemark ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NewRemark", remark);
                    int rowsAffectedHistory = cmd.ExecuteNonQuery();

                    // ── 7.1 : ถ้าบันทึก History ไม่ได้ ก็ต้องไม่ Commit การเปลี่ยน Location เลย ──
                    if (rowsAffectedHistory != 1)
                        throw new Exception("บันทึกประวัติการแก้ไข (History_Change_Location) ไม่สำเร็จ");
                }

                trans.Commit();

                alert_toast("บันทึกข้อมูลแก้ไข Location สำเร็จ", "สำเร็จ", "success", "2500");

                SetCurrentEditItem(null);
                BindEditItem();

                SearchData();

                //ScriptManager.RegisterStartupScript(
                //    this, GetType(),
                //    "reloadAfterSave",
                //    "setTimeout(function(){ window.location.href = window.location.href.split('?')[0]; }, 1500);",
                //    true
                //);

            }
            catch (Exception ex)
            {
                trans.Rollback();
                alert_toast("เกิดข้อผิดพลาด: " + ex.Message, "ผิดพลาด", "error", "6000");
                ScriptManager.RegisterStartupScript(this, GetType(), "hideLoadingErr", "hideLoading();", true);
            }
            finally
            {
                conn.Close();
            }
        }

        // ── รวม emp_id + ชื่อ Responsible เป็นข้อความเดียว สำหรับคอลัมน์ Old_Responsible/New_Responsible ──
        private string BuildResponsibleDisplay(string empId, string name)
        {
            if (string.IsNullOrEmpty(empId) && string.IsNullOrEmpty(name)) return null;
            if (string.IsNullOrEmpty(name)) return empId;
            if (string.IsNullOrEmpty(empId)) return name;
            return empId + " - " + name;
        }

        // ── 1.3.1.1 / 1.3.2.1 : หา dept_id / division จาก Dept Code ──
        private bool TryResolveDept(string deptCode, SqlTransaction trans, out string deptId, out int division)
        {
            deptId = null;
            division = 0;

            string str = "SELECT dept_id, division FROM tool_eduipment_system.dbo.View_dept_master WHERE dept_code = @code";
            using (var cmd = new SqlCommand(str, conn, trans))
            {
                cmd.Parameters.AddWithValue("@code", deptCode);
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        deptId = rdr["dept_id"].ToString();
                        division = rdr["division"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["division"]);
                        return true;
                    }
                }
            }
            return false;
        }

        // ── 1.2.1.1 : หา/เพิ่ม Model ใน draw_master_model ──
        // ⚠️ Req.8 : ใช้ WITH (UPDLOCK, HOLDLOCK) ตอน SELECT ตรวจสอบชื่อซ้ำ ภายใน transaction เดียวกับตอน Insert
        //    เพื่อกันไม่ให้ 2 ผู้ใช้เพิ่ม Model ชื่อเดียวกันพร้อมกันแล้วได้ id ซ้ำ/ row ซ้ำ
        //    (transaction ที่มาทีหลังจะถูก "ล็อครอ" จนกว่า transaction แรก commit แล้วจะเห็นแถวที่เพิ่งถูก insert และใช้ id เดิมแทน)
        private int GetOrCreateModelId(string modelName, SqlTransaction trans)
        {
            string strCheck = "SELECT model_id, model_check FROM jig_tool_system.dbo.draw_master_model WITH (UPDLOCK, HOLDLOCK) WHERE model_name = @name";
            using (var cmd = new SqlCommand(strCheck, conn, trans))
            {
                cmd.Parameters.AddWithValue("@name", modelName);
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        int existingId = Convert.ToInt32(rdr["model_id"]);
                        int modelCheck = rdr["model_check"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["model_check"]);
                        rdr.Close();

                        if (modelCheck == 0)
                        {
                            string strUpdCheck = "UPDATE jig_tool_system.dbo.draw_master_model SET model_check = 1 WHERE model_id = @id";
                            using (var cmdUpd = new SqlCommand(strUpdCheck, conn, trans))
                            {
                                cmdUpd.Parameters.AddWithValue("@id", existingId);
                                if (cmdUpd.ExecuteNonQuery() != 1)
                                    throw new Exception("อัพเดทสถานะ Model (model_check) ไม่สำเร็จ");
                            }
                        }
                        return existingId;
                    }
                }
            }

            // ไม่พบ -> เพิ่มใหม่ (model_id = MAX + 1) ยังอยู่ภายใต้ล็อคเดียวกันจาก SELECT ด้านบน จึงไม่มีเลขซ้ำ
            int newId;
            string strMax = "SELECT ISNULL(MAX(model_id), 0) + 1 FROM jig_tool_system.dbo.draw_master_model WITH (UPDLOCK, HOLDLOCK)";
            using (var cmdMax = new SqlCommand(strMax, conn, trans))
            {
                newId = Convert.ToInt32(cmdMax.ExecuteScalar());
            }

            string strIns = "INSERT INTO jig_tool_system.dbo.draw_master_model (model_id, model_name, model_check) VALUES (@id, @name, 1)";
            using (var cmdIns = new SqlCommand(strIns, conn, trans))
            {
                cmdIns.Parameters.AddWithValue("@id", newId);
                cmdIns.Parameters.AddWithValue("@name", modelName);
                if (cmdIns.ExecuteNonQuery() != 1)
                    throw new Exception("เพิ่มข้อมูล Model ใหม่ (" + modelName + ") ไม่สำเร็จ");
            }
            return newId;
        }

        // ── 1.2.1.2 : หา/เพิ่ม Cell ใน master_location2 (where location1_id = @location1Id) ──
        // ⚠️ Req.8 : WITH (UPDLOCK, HOLDLOCK) กันการเพิ่ม Cell ชื่อซ้ำพร้อมกันจากหลาย session
        private int GetOrCreateCellId(string cellName, int location1Id, SqlTransaction trans)
        {
            string strCheck = "SELECT location2_id, is_use FROM jig_tool_system.dbo.master_location2 WITH (UPDLOCK, HOLDLOCK) WHERE location1_id = @loc1 AND location2_name = @name";
            using (var cmd = new SqlCommand(strCheck, conn, trans))
            {
                cmd.Parameters.AddWithValue("@loc1", location1Id);
                cmd.Parameters.AddWithValue("@name", cellName);
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        int existingId = Convert.ToInt32(rdr["location2_id"]);
                        int isUse = rdr["is_use"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["is_use"]);
                        rdr.Close();

                        if (isUse == 0)
                        {
                            string strUpd = "UPDATE jig_tool_system.dbo.master_location2 SET is_use = 1 WHERE location1_id = @loc1 AND location2_id = @id";
                            using (var cmdUpd = new SqlCommand(strUpd, conn, trans))
                            {
                                cmdUpd.Parameters.AddWithValue("@loc1", location1Id);
                                cmdUpd.Parameters.AddWithValue("@id", existingId);
                                if (cmdUpd.ExecuteNonQuery() != 1)
                                    throw new Exception("อัพเดทสถานะ Cell (is_use) ไม่สำเร็จ");
                            }
                        }
                        return existingId;
                    }
                }
            }

            // ไม่พบ -> เพิ่มใหม่ (location2_id = MAX + 1 ภายใน location1_id เดียวกัน) ยังอยู่ภายใต้ล็อคจาก SELECT ด้านบน
            int newId;
            string strMax = "SELECT ISNULL(MAX(location2_id), 0) + 1 FROM jig_tool_system.dbo.master_location2 WITH (UPDLOCK, HOLDLOCK) WHERE location1_id = @loc1";
            using (var cmdMax = new SqlCommand(strMax, conn, trans))
            {
                cmdMax.Parameters.AddWithValue("@loc1", location1Id);
                newId = Convert.ToInt32(cmdMax.ExecuteScalar());
            }

            string strIns = "INSERT INTO jig_tool_system.dbo.master_location2 (location1_id, location2_id, location2_name, is_use) VALUES (@loc1, @id, @name, 1)";
            using (var cmdIns = new SqlCommand(strIns, conn, trans))
            {
                cmdIns.Parameters.AddWithValue("@loc1", location1Id);
                cmdIns.Parameters.AddWithValue("@id", newId);
                cmdIns.Parameters.AddWithValue("@name", cellName);
                if (cmdIns.ExecuteNonQuery() != 1)
                    throw new Exception("เพิ่มข้อมูล Cell ใหม่ (" + cellName + ") ไม่สำเร็จ");
            }
            return newId;
        }

        // ── 1.2.1.3 : หา/เพิ่ม Station ใน master_location3 ──
        // แก้ตาม Req.6 : ตรวจสอบ location3_name ว่าตรงกับที่กรอกหรือไม่ โดยเทียบเฉพาะ location1_id = 97 (ไม่กรองด้วย location2_id)
        //   - ถ้าตรง -> ใช้ location3_id เดิม
        //   - ถ้าไม่ตรง (ไม่พบ) -> หา MAX(location3_id) ภายใน location1_id = 97 แล้ว +1 เพื่อสร้างใหม่
        //   location2_id ยังคงถูกใส่ตอน Insert เพื่อผูก Station กับ Cell ที่เพิ่ง resolve ไว้ (คอลัมน์นี้ NOT NULL ใน schema)
        // ⚠️ Req.8 : WITH (UPDLOCK, HOLDLOCK) กันการเพิ่ม Station ชื่อซ้ำพร้อมกันจากหลาย session
        private int GetOrCreateStationId(string stationName, int location1Id, int location2Id, SqlTransaction trans)
        {
            string strCheck = "SELECT location3_id FROM jig_tool_system.dbo.master_location3 WITH (UPDLOCK, HOLDLOCK) WHERE location1_id = @loc1 AND location3_name = @name";
            using (var cmd = new SqlCommand(strCheck, conn, trans))
            {
                cmd.Parameters.AddWithValue("@loc1", location1Id);
                cmd.Parameters.AddWithValue("@name", stationName);
                object result = cmd.ExecuteScalar();
                if (result != null) return Convert.ToInt32(result);
            }

            // ไม่พบ -> เพิ่มใหม่ (location3_id = MAX + 1 ภายใน location1_id = 97 เท่านั้น) ยังอยู่ภายใต้ล็อคจาก SELECT ด้านบน
            int newId;
            string strMax = "SELECT ISNULL(MAX(location3_id), 0) + 1 FROM jig_tool_system.dbo.master_location3 WITH (UPDLOCK, HOLDLOCK) WHERE location1_id = @loc1";
            using (var cmdMax = new SqlCommand(strMax, conn, trans))
            {
                cmdMax.Parameters.AddWithValue("@loc1", location1Id);
                newId = Convert.ToInt32(cmdMax.ExecuteScalar());
            }

            string strIns = "INSERT INTO jig_tool_system.dbo.master_location3 (location1_id, location2_id, location3_id, location3_name) VALUES (@loc1, @loc2, @id, @name)";
            using (var cmdIns = new SqlCommand(strIns, conn, trans))
            {
                cmdIns.Parameters.AddWithValue("@loc1", location1Id);
                cmdIns.Parameters.AddWithValue("@loc2", location2Id);
                cmdIns.Parameters.AddWithValue("@id", newId);
                cmdIns.Parameters.AddWithValue("@name", stationName);
                if (cmdIns.ExecuteNonQuery() != 1)
                    throw new Exception("เพิ่มข้อมูล Station ใหม่ (" + stationName + ") ไม่สำเร็จ");
            }
            return newId;
        }

        // ══════════════════════════════
        //  PICTURE / DETAIL (เหมือนหน้าอื่นในระบบ)
        // ══════════════════════════════
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

        // ══════════════════════════════
        //  HELPERS
        // ══════════════════════════════
        private EditLocationItem GetCurrentEditItem()
        {
            return ViewState["EditLocationItem"] as EditLocationItem;
        }
        private void SetCurrentEditItem(EditLocationItem item)
        {
            ViewState["EditLocationItem"] = item;
        }

        // ── Parse Due Date ให้ตรง format ที่ datepicker ส่งมา (dd-MMM-yyyy, เดือนอังกฤษ) ──
        // ใช้ InvariantCulture กัน server ที่ตั้ง culture เป็น th-TH parse ไม่ผ่านแล้วกลายเป็นค่าว่าง
        private object ParseDueDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return DBNull.Value;

            DateTime d;
            string[] formats = { "dd-MMM-yyyy", "d-MMM-yyyy", "dd/MM/yyyy", "yyyy-MM-dd" };

            if (DateTime.TryParseExact(s.Trim(), formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out d))
                return d;

            // fallback เผื่อรูปแบบอื่น
            if (DateTime.TryParse(s.Trim(),
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out d))
                return d;

            return DBNull.Value;
        }

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
        protected string GetStatusBadgeClass(string displayStatus)
        {
            if (displayStatus == "อยู่ระหว่างยืม")
                return "badge-borrowing";

            if (displayStatus.StartsWith("Spare"))
                return "badge-spare";

            return "badge-normal";
        }

        [Serializable]
        public class EditLocationItem
        {
            public string ToolId { get; set; }
            public string ToolSub { get; set; }
            public string SystemId { get; set; }
            public string FixAssetNo { get; set; }
            public string ControlNo { get; set; }
            public string ToolNo { get; set; }

            // ── ค่าปัจจุบัน (Current) ─────────────────────────────
            public string OldStatusId { get; set; }
            public string OldStatusName { get; set; }
            public string OldDeptCode { get; set; }
            public string OldResponsibleId { get; set; }
            public string OldResponsibleName { get; set; }
            public string OldModelId { get; set; }
            public string OldModelName { get; set; }
            public string OldLocation1Id { get; set; }
            public string OldCellId { get; set; }
            public string OldCellName { get; set; }
            public string OldStationId { get; set; }
            public string OldStationName { get; set; }
            public string OldDueDate { get; set; }
            public string OldRemark { get; set; }

            // ── ค่าที่เลือก/กรอกใหม่ (เติมตอนกด Save) ─────────────
            public string NewStatusId { get; set; }
            public string NewStatusName { get; set; }
            public string NewDeptCode { get; set; }
            public string NewResponsibleId { get; set; }
            public string NewResponsibleName { get; set; }
            public string NewModelId { get; set; }
            public string NewModelName { get; set; }
            public string NewCellId { get; set; }
            public string NewCellName { get; set; }
            public string NewStationId { get; set; }
            public string NewStationName { get; set; }
            public string NewDueDate { get; set; }
            public string NewRemark { get; set; }
        }
    }
}
