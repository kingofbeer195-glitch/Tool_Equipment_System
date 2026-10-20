using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tool_Equipment_Control_System
{
    public partial class Tanaoroshi : System.Web.UI.Page
    {
        // ─── Connections ──────────────────────────────────────────
        SqlConnection conn_user = new SqlConnection(ConfigurationManager.ConnectionStrings["user"].ConnectionString);
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        SqlConnection conn_jig = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString);
        datacon dc = new datacon();

        // check_id mapping : 1=OK, 2=Location(Change), 3=Not Found(NG)
        private const int RESULT_OK = 1;
        private const int RESULT_CHANGE = 2;
        private const int RESULT_NG = 3;

        // ═══════════════════════════════════════════════════════════
        //  Page Load
        // ═══════════════════════════════════════════════════════════
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = "Tanaoroshi";

            if (Session["emp_id"] == null || Session["emp_id"].ToString() == "")
            { Response.Redirect("Default.aspx"); return; }

            if (!IsPostBack)
            {
                Step1_Init();
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  STEP 1 : ตรวจ emp_id ของผู้ login
        //  - admin == "99"  → login ด้วย dept_code → ให้กรอก emp_id เอง
        //  - user ปกติ      → ใช้ Session["emp_id"] ยิง HRMS ทันที
        // ═══════════════════════════════════════════════════════════
        private void Step1_Init()
        {
            string emp_id = Session["emp_id"].ToString();
            //string admin = Session["admin"] != null ? Session["admin"].ToString() : "";
            string loginVal = emp_id;   // ค่าที่ login เข้ามา
                                        // แยกด้วยความยาว: 4 หลัก = dept code, 6 หลัก = emp id
            bool loginByDept = loginVal.Length == 4;

            if (loginByDept)
            {
                // login ด้วย dept code → รอ user กรอก emp_id
                pnl_login_emp.Visible = true;
                pnl_step2.Visible = false;
                lbl_emp_notice.Text = "กรุณากรอกรหัสพนักงาน [Employee ID]";
                txt_emp_id.ReadOnly = false;
                txt_emp_id.Focus();
                return;
            }

            // user ปกติ → ยิง HRMS ด้วย emp_id ของ session เลย
            txt_emp_id.Text = emp_id;
            bool found = GetDataHRMS_CSharp(emp_id);

            if (!found)
            {
                pnl_login_emp.Visible = true;
                pnl_step2.Visible = false;
                lbl_emp_notice.Text = "ไม่พบ Employee ID นี้ในระบบ";
                txt_emp_id.Style["background-color"] = "#ff000082";
                return;
            }

            Confirm_emp_and_load();
        }

        // ─── กรอก emp_id แล้ว PostBack (กรณี admin) ────────────────
        protected void txt_emp_id_TextChanged(object sender, EventArgs e)
        {
            string dept_login = Session["emp_id"].ToString();
            string empId = txt_emp_id.Text.Trim();

            if (empId.Length < 6)
            {
                Clear_emp_ui();
                lbl_emp_notice.Text = "กรุณากรอกรหัสพนักงาน [Employee ID]";
                return;
            }

            bool found = GetDataHRMS_CSharp(empId);
            if (!found)
            {
                lbl_emp_notice.Text = "ไม่พบ Employee ID นี้ในระบบ";
                txt_emp_id.Style["background-color"] = "#ff000082";
                Clear_emp_ui();
                return;
            }

            string dept_code = txt_dept_code.Text.Trim();
            if (dept_code != dept_login)
            {
                lbl_emp_notice.Text = "Dept Code ไม่ตรงกับที่ Login";
                txt_emp_id.Style["background-color"] = "#ff000082";
                //Clear_emp_ui();
                return;
            }

            Confirm_emp_and_load();
        }

        // ─── ยืนยัน emp + เก็บ dept_code + ไป Step 2 ───────────────
        private void Confirm_emp_and_load()
        {
            txt_emp_id.ReadOnly = true;
            txt_emp_id.Style["background-color"] = "#d4edda";
            lbl_emp_notice.Text = "";

            string deptCode = txt_dept_code.Text.Trim();
            ViewState["DEPT_CODE"] = deptCode;
            ViewState["EMP_ID"] = txt_emp_id.Text.Trim();

            // ── หา emp_id ทั้งหมดของ dept นี้ จาก Responsible_Dept.dept_code_real ──
            Load_responsible(deptCode);

            pnl_login_emp.Visible = true;
            pnl_step2.Visible = true;

            Load_tana_date_ddl();
            Set_ddl_model_cell();
            Load_tab1();
            Load_tab2();
        }
        // โหลด Responsible_Dept → เก็บ emp_id list + เติม ddl_Responsible
        private void Load_responsible(string deptCode)
        {
            dc.QryDT_P(conn, @"
                SELECT emp_id, ISNULL(pic_dept, emp_id) AS pic_dept
                FROM Responsible_Dept
                WHERE dept_code_real = @deptCodeReal AND emp_id IS NOT NULL
                ORDER BY emp_id",
                new SqlParameter("@deptCodeReal", SafeInt(deptCode)));

            var ids = new List<string>();
            ddl_Responsible.Items.Clear();
            ddl_Responsible.Items.Add(new ListItem("-- All Responsible --", ""));
            foreach (DataRow r in dc.dt.Rows)
            {
                string emp = r["emp_id"].ToString().Trim();
                ids.Add(emp);
                ddl_Responsible.Items.Add(new ListItem(
                    emp + " - " + r["pic_dept"].ToString(), emp));
            }
            ViewState["RSP_EMP_IDS"] = string.Join(",", ids);
        }

        // ─── ยิง HRMS middleware (reuse จาก borrow_tool2) ──────────
        private bool GetDataHRMS_CSharp(string userId)
        {
            if (!IsValidEmpNo(userId)) return false;

            // เบอร์ต่อภายในจาก sys_user_master
            //string strTel = "SELECT ext FROM sys_user_master WHERE emp_id = @emp";
            //dc.QryDT_P(conn_user, strTel, new SqlParameter("@emp", userId));
            //if (dc.dt.Rows.Count > 0)
            //    txt_tel.Text = dc.dt.Rows[0]["ext"].ToString();

            string sql =
                "select GNAME_ENG, FNAME_ENG, POSN_ENAME, BAND, DEPT_CODE, DEPT_ABB_NAME, EMAIL " +
                "from ADMIN.v_emp_data_all_cpt where UPPER(emp_no)=UPPER('" + userId + "')";

            try
            {
                string url = "http://cptsvs547:1000/middleware/oracle/hrms";
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                byte[] data = Encoding.UTF8.GetBytes("command=" + HttpUtility.UrlEncode(sql));
                using (var stream = request.GetRequestStream())
                    stream.Write(data, 0, data.Length);

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string json = reader.ReadToEnd();
                    dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    if (result.data == null || result.data.Count == 0) return false;

                    var u = result.data[0];
                    txt_emp_name.Text = u.GNAME_ENG + " " + u.FNAME_ENG;
                    txt_position.Text = u.POSN_ENAME;
                    txt_dept.Text = u.DEPT_ABB_NAME;
                    txt_dept_code.Text = u.DEPT_CODE;
                    return true;
                }
            }
            catch (Exception ex)
            {
                ShowToast(ex.Message, "HRMS Error", "error", 8000);
                return false;
            }
        }

        private bool IsValidEmpNo(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            return s.All(c => char.IsLetterOrDigit(c));
        }

        private void Clear_emp_ui()
        {
            txt_emp_name.Text = "";
            txt_position.Text = "";
            txt_dept.Text = "";
            txt_dept_code.Text = "";
            pnl_step2.Visible = false;
        }

        // ═══════════════════════════════════════════════════════════
        //  STEP 2 : โหลด 2 Tab ตาม dept_code
        // ═══════════════════════════════════════════════════════════
        private string GetDeptCode()
        {
            return ViewState["DEPT_CODE"] != null ? ViewState["DEPT_CODE"].ToString() : "";
        }

        // ─── เติม dropdown Model / Cell (เรียกตอน Confirm emp) ────
        private void Set_ddl_model_cell()
        {
            // Model
            var pm = new List<SqlParameter>();
            dc.QryDT_P(conn, @"
                SELECT DISTINCT H.model_id, ISNULL(VM.model_name,'-') AS model_name
                FROM Equipment_tana_history H
                LEFT JOIN jig_tool_system.dbo.draw_master_model VM ON H.model_id = VM.model_id
                WHERE VM.model_name IS NOT NULL" + BuildPicRspFilter("H", pm) + @"
                ORDER BY model_name", pm.ToArray());

            ddl_model.Items.Clear();
            ddl_model.Items.Add(new ListItem("-- All Model --", ""));
            foreach (DataRow r in dc.dt.Rows)
                ddl_model.Items.Add(new ListItem(r["model_name"].ToString(), r["model_id"].ToString()));

            // Cell
            var pc = new List<SqlParameter>();
            dc.QryDT_P(conn, @"
                SELECT DISTINCT H.cell_id, ISNULL(VC.location2_name,'-') AS cell_name
                FROM Equipment_tana_history H
                LEFT JOIN jig_tool_system.dbo.master_location2 VC
                    ON H.cell_id = VC.location2_id AND H.location_id = VC.location1_id
                WHERE VC.location2_name IS NOT NULL" + BuildPicRspFilter("H", pc) + @"
                ORDER BY cell_name", pc.ToArray());
            ddl_cell.Items.Clear();
            ddl_cell.Items.Add(new ListItem("-- All Cell --", ""));
            foreach (DataRow r in dc.dt.Rows)
                ddl_cell.Items.Add(new ListItem(r["cell_name"].ToString(), r["cell_id"].ToString()));
        }

        // โหลดรายการเดือน-ปี จาก tana_date + เลือกเดือนปัจจุบันเป็น default
        private void Load_tana_date_ddl()
        {
            if (IsPostBack && ddl_tana_date.Items.Count > 0) return;

            var prms = new List<SqlParameter>();
            string rsp = BuildPicRspFilter("H", prms);

            dc.QryDT_P(conn, @"
                SELECT H.tana_date, MAX(TRY_CONVERT(date, '01-' + H.tana_date, 106)) AS sort_dt
                FROM Equipment_tana_history H
                WHERE H.tana_date IS NOT NULL AND H.tana_date <> ''" + rsp + @"
                GROUP BY H.tana_date
                ORDER BY sort_dt DESC", prms.ToArray());

            ddl_tana_date.Items.Clear();
            ddl_tana_date.Items.Add(new ListItem("-- All --", ""));
            foreach (DataRow r in dc.dt.Rows)
            {
                string v = r["tana_date"].ToString();
                ddl_tana_date.Items.Add(new ListItem(v, v));
            }

            string curLabel = DateTime.Now.ToString("MMM-yyyy",
                System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            if (ddl_tana_date.Items.FindByValue(curLabel) != null)
                ddl_tana_date.SelectedValue = curLabel;
        }

        // filter tana_date ตามเดือนที่เลือก (value = yyyy-MM)
        private string BuildDateFilter(string alias, List<SqlParameter> prms)
        {
            string v = ddl_tana_date.SelectedValue;
            if (string.IsNullOrEmpty(v)) return "";      // All
            prms.Add(new SqlParameter("@tanaDate", v));
            return " AND " + alias + ".tana_date = @tanaDate ";
        }

        // ─── event Filter Model/Cell (มีผลทั้ง 2 Tab) ─────────────
        protected void ddl_filter_Changed(object sender, EventArgs e)
        {
            if (pnl_step2.Visible) { Load_tab1(); Load_tab2(); }
        }

        // ─── Tab 1 : Equipment_tana_list + จำนวน/สถานะ ────────────
        //  จำนวน = ตรวจแล้ว(check_id NOT NULL) / ทั้งหมด ต่อ list_id
        private void Load_tab1()
        {
            var prms = new List<SqlParameter>();
            string whereModel = "", whereCell = "";
            if (!string.IsNullOrEmpty(ddl_model.SelectedValue))
            { whereModel = " AND TL.model_id = @modelId"; prms.Add(new SqlParameter("@modelId", SafeInt(ddl_model.SelectedValue))); }
            if (!string.IsNullOrEmpty(ddl_cell.SelectedValue))
            { whereCell = " AND TL.cell_id = @cellId"; prms.Add(new SqlParameter("@cellId", SafeInt(ddl_cell.SelectedValue))); }
            string rspFilter = BuildPicRspFilter("TL", prms);
            string dateFilter = BuildDateFilter("TL", prms);

            string sql = @"
                SELECT TL.id, TL.tana_date, TL.dept_code, TL.pic_rsp,
                       TL.model_id, TL.location_id, TL.cell_id,
                       ISNULL(VM.model_name, '-')     AS model_name,
                       ISNULL(VL.location1_name, '-') AS location_name,
                       ISNULL(VC.location2_name, '-') AS cell_name,
                       ISNULL(RSP.pic_dept, TL.pic_rsp) AS pic_rsp_name,
                       (SELECT COUNT(*) FROM Equipment_tana_history H WHERE H.list_id = TL.id) AS total_qty,
                       (SELECT COUNT(*) FROM Equipment_tana_history H WHERE H.list_id = TL.id AND H.check_id IS NOT NULL) AS checked_qty
                FROM Equipment_tana_list TL
                LEFT JOIN jig_tool_system.dbo.draw_master_model VM ON TL.model_id = VM.model_id
                LEFT JOIN jig_tool_system.dbo.master_location1  VL ON TL.location_id = VL.location1_id
                LEFT JOIN jig_tool_system.dbo.master_location2  VC ON TL.cell_id = VC.location2_id AND TL.location_id = VC.location1_id
                LEFT JOIN Responsible_Dept RSP ON TL.pic_rsp = RSP.emp_id
                WHERE 1=1" + rspFilter + dateFilter + whereModel + whereCell + @"
                ORDER BY TL.tana_date DESC, VM.model_name";

            dc.QryDT_P(conn, sql, prms.ToArray());
            gv_tab1.DataSource = dc.dt;
            gv_tab1.DataBind();
            lbl_tab1_count.Text = dc.dt.Rows.Count.ToString("N0");
        }

        // ─── RowDataBound Tab1 : แสดง 2/11 + Completed ────────────
        protected void gv_tab1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            int total = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "total_qty"));
            int done = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "checked_qty"));

            Label lbStat = e.Row.FindControl("lb_list_status") as Label;
            LinkButton lbQty = e.Row.FindControl("lb_qty") as LinkButton;

            if (lbQty != null)
                lbQty.Text = "<span class='qty-badge'>" + done + "/" + total + "</span>";

            if (lbStat != null)
            {
                if (total > 0 && done >= total)
                {
                    lbStat.Text = "<span class='badge-completed'>Completed</span>";
                    e.Row.CssClass = "row-completed";
                }
                else if (done > 0)
                    lbStat.Text = "<span class='badge-progress'>In Progress</span>";
                else
                    lbStat.Text = "<span class='badge-pending'>Pending</span>";
            }
        }

        // ─── Tab 2 : Equipment_tana_history (List All) + status filter ─
        private void Load_tab2()
        {
            var prms = new List<SqlParameter>();
            //string extra = BuildPicRspFilter("H", prms) + BuildDateFilter("H", prms);
            string extra = BuildTab2ScopeFilter("H", prms) + BuildDateFilter("H", prms);


            if (!string.IsNullOrEmpty(ddl_model.SelectedValue))
            { extra += " AND H.model_id = @modelId"; prms.Add(new SqlParameter("@modelId", SafeInt(ddl_model.SelectedValue))); }
            if (!string.IsNullOrEmpty(ddl_cell.SelectedValue))
            { extra += " AND H.cell_id = @cellId"; prms.Add(new SqlParameter("@cellId", SafeInt(ddl_cell.SelectedValue))); }

            // status filter : ค่าจาก hidden field (set โดยปุ่ม chip)
            string st = hf_status_filter.Value;
            if (st == "1") extra += " AND H.check_id = 1";
            else if (st == "2") extra += " AND H.check_id = 2";
            else if (st == "3") extra += " AND (H.check_id = 3 OR H.check_id IS NULL)";
            else if (st == "4") { extra += " AND H.dept_new = @deptOut"; prms.Add(new SqlParameter("@deptOut", GetDeptCode())); }

            // scan keyword filter (asset_no หรือ control_no)
            string scanKw = ViewState["SCAN_KEYWORD"] != null ? ViewState["SCAN_KEYWORD"].ToString() : "";
            if (!string.IsNullOrEmpty(scanKw))
            {
                extra += " AND (H.asset_no = @scan OR H.control_no = @scan)";
                prms.Add(new SqlParameter("@scan", scanKw));
            }

            dc.QryDT_P(conn, BuildHistorySql() + extra + " ORDER BY H.asset_no", prms.ToArray());
            gv_tab2.DataSource = dc.dt;
            gv_tab2.DataBind();
            lbl_tab2_count.Text = dc.dt.Rows.Count.ToString("N0");

            // อัปเดตตัวเลข chip (นับจากทั้ง dept ไม่ติด status filter แต่ติด model/cell)
            Refresh_status_counts();
        }

        // ─── นับจำนวนแต่ละสถานะ (สำหรับ chip) ─────────────────────
        private void Refresh_status_counts()
        {
            var prms = new List<SqlParameter>();
            //string extra = BuildPicRspFilter("H", prms) + BuildDateFilter("H", prms);
            string extra = BuildTab2ScopeFilter("H", prms) + BuildDateFilter("H", prms);

            prms.Add(new SqlParameter("@deptOutCnt", GetDeptCode()));

            if (!string.IsNullOrEmpty(ddl_model.SelectedValue))
            { extra += " AND model_id = @modelId"; prms.Add(new SqlParameter("@modelId", SafeInt(ddl_model.SelectedValue))); }
            if (!string.IsNullOrEmpty(ddl_cell.SelectedValue))
            { extra += " AND cell_id = @cellId"; prms.Add(new SqlParameter("@cellId", SafeInt(ddl_cell.SelectedValue))); }

            dc.QryDT_P(conn, @"
                SELECT COUNT(*) AS total_all,
                    SUM(CASE WHEN H.check_id = 1 THEN 1 ELSE 0 END) AS ok_cnt,
                    SUM(CASE WHEN H.check_id = 2 THEN 1 ELSE 0 END) AS change_cnt,
                    SUM(CASE WHEN H.check_id = 3 OR H.check_id IS NULL THEN 1 ELSE 0 END) AS ng_cnt,
                    SUM(CASE WHEN H.dept_new = @deptOutCnt THEN 1 ELSE 0 END) AS outdept_cnt
                FROM Equipment_tana_history H
                WHERE 1=1" + extra, prms.ToArray());

            if (dc.dt.Rows.Count > 0)
            {
                DataRow r = dc.dt.Rows[0];
                lbl_cnt_all.Text = r["total_all"].ToString();
                lbl_cnt_ok.Text = r["ok_cnt"].ToString();
                lbl_cnt_change.Text = r["change_cnt"].ToString();
                lbl_cnt_outdept.Text = r["outdept_cnt"].ToString();
                lbl_cnt_ng.Text = r["ng_cnt"].ToString();
            }
        }

        // ─── ปุ่ม status chip (OK/Change/NG/All) ──────────────────
        protected void btn_status_filter_Click(object sender, EventArgs e)
        {
            LinkButton b = (LinkButton)sender;
            hf_status_filter.Value = b.CommandArgument;   // "", "1", "2", "3"
            Load_tab2();
            ScriptManager.RegisterStartupScript(this, GetType(),
                "actchip", "setActiveChip('" + b.CommandArgument + "'); switchTab(2);", true);
        }

        // SQL กลางของ history (ใช้ทั้ง Tab2 + scan lookup)
        private string BuildHistorySql()
        {
            return @"
                SELECT H.id, H.tana_date, H.dept_code, H.pic_rsp,
                       H.asset_no, H.sub_asset_no, H.control_no, H.name,
                       H.tool_id, H.sys_status, H.tool_sub_no,
                       H.model_id, H.location_id, H.cell_id, H.station_id,
                       H.check_id,H.check_by,H.check_name,H.remark,
                       ISNULL(VM.model_name, '-')     AS model_name,
                       ISNULL(VL.location1_name, '-') AS location_name,
                       ISNULL(VC.location2_name, '-') AS cell_name,
                       ISNULL(VS.location3_name, '-') AS station_name,
                       ISNULL(CHK.pic_dept, H.check_by) AS check_by_name,                   
                       ISNULL(RS.tanaResult_Name, CASE WHEN H.check_id IS NULL THEN 'Not Found' ELSE '' END) AS result_name                      
                FROM   Equipment_tana_history H
                LEFT JOIN jig_tool_system.dbo.draw_master_model VM ON H.model_id = VM.model_id
                LEFT JOIN jig_tool_system.dbo.master_location1  VL ON H.location_id = VL.location1_id
                LEFT JOIN jig_tool_system.dbo.master_location2  VC ON H.cell_id = VC.location2_id AND H.location_id = VC.location1_id
                LEFT JOIN jig_tool_system.dbo.master_location3  VS ON H.station_id = VS.location3_id AND H.location_id = VS.location1_id
                LEFT JOIN tanaResult_Master RS ON H.check_id = RS.tanaResult_ID
                LEFT JOIN Responsible_Dept CHK ON H.check_by = CHK.emp_id
                WHERE 1=1"; // filter pic_rsp
        }

        // ═══════════════════════════════════════════════════════════
        //  STEP 3 (ใหม่) : Scan → filter Tab List All ให้เหลือที่เจอ
        // ═══════════════════════════════════════════════════════════
        protected void btn_scan_go_Click(object sender, EventArgs e)
        {
            string scan = txt_scan.Text.Trim();
            if (string.IsNullOrEmpty(scan)) return;

            ViewState["SCAN_KEYWORD"] = scan;

            var prms = new List<SqlParameter>();
            string rsp = BuildPicRspFilter("H", prms);
            string dateFilter = BuildDateFilter("H", prms);
            prms.Add(new SqlParameter("@scan", scan));
            dc.QryDT_P(conn, BuildHistorySql() + rsp + dateFilter + " AND (H.asset_no=@scan OR H.control_no=@scan)", prms.ToArray());

            if (dc.dt.Rows.Count == 0)
            {
                // fallback : ค้นทั้งระบบ (ไม่ติด filter dept/pic_rsp) เผื่อชิ้นถูกโยกย้ายระหว่างงวด
                var prms2 = new List<SqlParameter>();
                string dateFilter2 = BuildDateFilter("H", prms2);
                prms2.Add(new SqlParameter("@scan", scan));
                dc.QryDT_P(conn, BuildHistorySql() + dateFilter2 + " AND (H.asset_no=@scan OR H.control_no=@scan)", prms2.ToArray());

                if (dc.dt.Rows.Count == 0)
                {
                    ViewState["SCAN_KEYWORD"] = null;
                    ShowToast("ไม่พบ Asset No. / Control No. : " + scan, "ไม่พบข้อมูล", "warn", 4000);
                    return;
                }

                // เจอนอกขอบเขต dept → แจ้งเตือน แล้วเปิด modal รายการแรก
                int distinctOut = dc.dt.AsEnumerable()
                    .Select(r => r["id"].ToString()).Distinct().Count();

                if (distinctOut > 1)
                {
                    ShowToast("พบ " + distinctOut + " รายการ (นอก Dept) — เลือกตรวจในหน้ารายการทั้งหมด", "พบนอก Dept", "warn", 5000);
                    // ไม่มี tab สำหรับรายการนอก dept → เปิด modal รายการแรกให้ก่อน
                }
                else
                {
                    ShowToast("รายการนี้อยู่นอก Dept ของคุณ (อาจถูกโยกย้ายระหว่างงวด)", "พบนอก Dept", "warn", 5000);
                }

                string outId = dc.dt.Rows[0]["id"].ToString();
                Load_detail_by_id_any(outId);   // ← โหลดแบบไม่ติด filter dept
                txt_scan.Text = "";
                ScriptManager.RegisterStartupScript(this, GetType(),
                    "openScanOut_" + DateTime.UtcNow.Ticks,
                    "openTanaModal(); focusScanBox();", true);
                return;
            }

            // เจอหลายรายการ → filter tab แล้วให้เลือก Check เอง
            if (dc.dt.Rows.Count > 1)
            {
                Load_tab2();
                txt_scan.Text = "";
                ShowToast("พบ " + dc.dt.Rows.Count + " รายการ กรุณาเลือกกด Check", "พบหลายรายการ", "info", 4000);
                ScriptManager.RegisterStartupScript(this, GetType(),
                    "afterScan_" + DateTime.UtcNow.Ticks,
                    "switchTab(2); highlightScanned(); focusScanBox();", true);
                return;
            }

            // เจอรายการเดียว → เปิด modal รายละเอียดทันที
            string historyId = dc.dt.Rows[0]["id"].ToString();
            Load_detail_by_id(historyId);
            txt_scan.Text = "";
            ScriptManager.RegisterStartupScript(this, GetType(),
                "openScan_" + DateTime.UtcNow.Ticks,
                "openTanaModal(); focusScanBox();", true);
        }

        // ─── ปุ่ม Check ในแถว Tab List All → เปิด modal รายละเอียด ──
        protected void btn_check_row_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            GridViewRow row = (GridViewRow)btn.NamingContainer;
            string historyId = (row.FindControl("hd_row_id") as HiddenField).Value;

            Load_detail_by_id(historyId);
            ScriptManager.RegisterStartupScript(this, GetType(),
                "openDetail_" + DateTime.UtcNow.Ticks, "openTanaModal();", true);
        }

        // ─── ปุ่ม "แสดงทั้งหมด" : ล้าง scan filter ─────────────────
        protected void btn_scan_clear_Click(object sender, EventArgs e)
        {
            ViewState["SCAN_KEYWORD"] = null;
            txt_scan.Text = "";
            hf_status_filter.Value = "";   // เคลียร์ status filter ที่ค้างจากการกด chip
            Load_tab2();
            ScriptManager.RegisterStartupScript(this, GetType(),
                "clr_" + DateTime.UtcNow.Ticks,
                "switchTab(2); setActiveChip(''); focusScanBox();", true);
        }
        // คลิกจำนวน → เปิด modal list รายการใน list นี้
        protected void lb_qty_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            GridViewRow row = (GridViewRow)btn.NamingContainer;
            string listId = (row.FindControl("hd_list_id") as HiddenField).Value;

            var prms = new List<SqlParameter>();
            string rsp = BuildPicRspFilter("H", prms);
            prms.Add(new SqlParameter("@listId", SafeInt(listId)));
            dc.QryDT_P(conn, BuildHistorySql() + rsp + " AND H.list_id=@listId ORDER BY H.asset_no", prms.ToArray());

            gv_list_detail.DataSource = dc.dt;
            gv_list_detail.DataBind();
            lbl_list_modal_title.Text = "รายการใน List #" + listId
                + " (" + dc.dt.Rows.Count + " รายการ)";

            // ── ดึง period + responsible ของ list นี้มาโชว์ header ──
            var pInfo = new List<SqlParameter> { new SqlParameter("@listId", SafeInt(listId)) };
            dc.QryDT_P(conn, @"
                SELECT TL.tana_date,
                       TL.pic_rsp,
                       ISNULL(RSP.pic_dept, TL.pic_rsp) AS pic_rsp_name,
                       ISNULL(VM.model_name, '-') AS model_name,
                       ISNULL(VC.location2_name, '-') AS cell_name
                FROM Equipment_tana_list TL
                LEFT JOIN Responsible_Dept RSP ON TL.pic_rsp = RSP.emp_id
                LEFT JOIN jig_tool_system.dbo.draw_master_model VM ON TL.model_id = VM.model_id
                LEFT JOIN jig_tool_system.dbo.master_location2 VC ON TL.cell_id = VC.location2_id AND TL.location_id = VC.location1_id
                WHERE TL.id = @listId", pInfo.ToArray());

            if (dc.dt.Rows.Count > 0)
            {
                DataRow ri = dc.dt.Rows[0];
                lbl_lm_period.Text = ri["tana_date"].ToString();
                string rsp_ri = ri["pic_rsp"].ToString();
                string rspName = ri["pic_rsp_name"].ToString();
                lbl_lm_resp.Text = string.IsNullOrEmpty(rsp_ri) ? "-"
                    : (rsp_ri + (string.IsNullOrEmpty(rspName) || rspName == rsp_ri ? "" : " - " + rspName));
                lbl_lm_model.Text = ri["model_name"].ToString();
                lbl_lm_cell.Text = ri["cell_name"].ToString();
            }
            else { lbl_lm_period.Text = "-"; lbl_lm_resp.Text = "-"; lbl_lm_model.Text = "-"; lbl_lm_cell.Text = "-"; }

            ScriptManager.RegisterStartupScript(this, GetType(),
                "openList_" + DateTime.UtcNow.Ticks, "openListModal();", true);
        }

        // ─── RowDataBound Tab2 : ระบายสีตามผลตรวจนับ ──────────────
        protected void gv_tab2_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            object chk = DataBinder.Eval(e.Row.DataItem, "check_id");
            int checkId = (chk == null || chk == DBNull.Value) ? 0 : Convert.ToInt32(chk);

            if (checkId == 1) e.Row.Attributes["style"] = "background:#f0fff4;";      // OK เขียว
            else if (checkId == 2) e.Row.Attributes["style"] = "background:#fffbea;"; // Change เหลือง
            else if (checkId == 3) e.Row.Attributes["style"] = "background:#fdecec;"; // NG แดง

            // badge สีตามผลตรวจนับ (ใช้สีเดียวกับปุ่ม result)
            Label badge = e.Row.FindControl("lb_result_badge") as Label;
            if (badge != null)
            {
                string resultName = Convert.ToString(DataBinder.Eval(e.Row.DataItem, "result_name"));
                string cls, txt;
                if (checkId == 1) { cls = "res-badge res-ok"; txt = string.IsNullOrEmpty(resultName) ? "Check OK" : resultName; }
                else if (checkId == 2) { cls = "res-badge res-change"; txt = string.IsNullOrEmpty(resultName) ? "Change Location" : resultName; }
                else { cls = "res-badge res-ng"; txt = string.IsNullOrEmpty(resultName) ? "Not Found" : resultName; }
                badge.Text = "<span class='" + cls + "'>" + Server.HtmlEncode(txt) + "</span>";
            }
        }

        protected void gv_list_detail_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            object chk = DataBinder.Eval(e.Row.DataItem, "check_id");
            int checkId = (chk == null || chk == DBNull.Value) ? 0 : Convert.ToInt32(chk);
            string resultName = Convert.ToString(DataBinder.Eval(e.Row.DataItem, "result_name"));

            // ── 3. ผลตรวจนับ แยกสี ──
            Label badge = e.Row.FindControl("lb_ld_result") as Label;
            if (badge != null)
            {
                string cls, txt;
                if (checkId == 1) { cls = "res-badge res-ok"; txt = string.IsNullOrEmpty(resultName) ? "Check OK" : resultName; }
                else if (checkId == 2) { cls = "res-badge res-change"; txt = string.IsNullOrEmpty(resultName) ? "Change Location" : resultName; }
                else { cls = "res-badge res-ng"; txt = string.IsNullOrEmpty(resultName) ? "Not Found" : resultName; }
                badge.Text = "<span class='" + cls + "'>" + Server.HtmlEncode(txt) + "</span>";
            }

            // ── 2.1 รูป thumbnail ──
            Image img = e.Row.FindControl("img_ld") as Image;
            if (img != null)
            {
                string url = "images/FilesType/jpeg.png";  // default
                try
                {
                    string toolId = Convert.ToString(DataBinder.Eval(e.Row.DataItem, "tool_id"));
                    string toolSub = Convert.ToString(DataBinder.Eval(e.Row.DataItem, "tool_sub_no"));
                    string sysStatus = Convert.ToString(DataBinder.Eval(e.Row.DataItem, "sys_status"));

                    var fs = new Get_Folder_Structure_Picture(toolId, toolSub, sysStatus);
                    string webPath = fs.get_folder_root_view_toolMain();
                    string physical = fs.get_folder_root();
                    if (!string.IsNullOrEmpty(physical) && Directory.Exists(physical)
                        && Directory.GetFiles(physical, "*.jpg", SearchOption.TopDirectoryOnly).Length > 0)
                        url = webPath;
                }
                catch { }
                img.ImageUrl = url;
            }

            // ── Remark ──
            Label rmk = e.Row.FindControl("lb_ld_remark") as Label;
            if (rmk != null)
            {
                string remark = Convert.ToString(DataBinder.Eval(e.Row.DataItem, "remark"));
                rmk.Text = Server.HtmlEncode(remark ?? "");
            }
        }

        // ─── โหลดรายละเอียดชิ้นเดียวตาม history.id ─────────────────
        private void Load_detail_by_id(string historyId)
        {
            var prms = new List<SqlParameter>();
            string rsp = BuildPicRspFilter("H", prms);
            prms.Add(new SqlParameter("@id", SafeInt(historyId)));
            dc.QryDT_P(conn, BuildHistorySql() + rsp + " AND H.id=@id", prms.ToArray());

            if (dc.dt.Rows.Count == 0)
            { ShowToast("ไม่พบข้อมูล", "Error", "error"); return; }

            Bind_detail_row(dc.dt.Rows[0], false);
        }

        // โหลดรายละเอียดโดยไม่ติด filter pic_rsp (สำหรับชิ้นที่อยู่นอก dept)
        private void Load_detail_by_id_any(string historyId)
        {
            var prms = new List<SqlParameter>();
            prms.Add(new SqlParameter("@id", SafeInt(historyId)));
            dc.QryDT_P(conn, BuildHistorySql() + " AND H.id=@id", prms.ToArray());

            if (dc.dt.Rows.Count == 0)
            { ShowToast("ไม่พบข้อมูล", "Error", "error"); return; }

            Bind_detail_row(dc.dt.Rows[0], true);   // ← นอก dept
        }

        private void Bind_detail_row(DataRow r , bool isOutOfDept)
        {
            ViewState["OUT_OF_DEPT"] = isOutOfDept;

            hd_history_id.Value = r["id"].ToString();
            hd_tool_id.Value = r["tool_id"].ToString();
            hd_tool_sub.Value = r["tool_sub_no"].ToString();
            hd_sys_status.Value = r["sys_status"].ToString();

            // ── ชุดข้อมูลหลัก (Asset/Control/Name) เอาจาก Tana History อย่างเดียว ──
            lbl_d_asset.Text = r["asset_no"].ToString();
            lbl_d_control.Text = r["control_no"].ToString();
            lbl_d_name.Text = r["name"].ToString();

            // ── ค่าฝั่ง Tana History (ใช้เทียบ) ──
            lbl_d_model.Text = r["model_name"].ToString();
            lbl_d_cell.Text = r["cell_name"].ToString();
            lbl_d_station.Text = r["station_name"].ToString();

            string prevResult = r["result_name"].ToString();
            int checkId = r["check_id"] == DBNull.Value ? 0 : Convert.ToInt32(r["check_id"]);
            lbl_scan_status.Text = checkId == 0
                ? "<span class='result-big result-wait'>ยังไม่ได้ตรวจ</span>"
                : "<span class='result-big result-done'>เคยนับแล้ว : " + Server.HtmlEncode(prevResult) + "</span>";

            Load_picture(r["tool_id"].ToString(), r["tool_sub_no"].ToString(), r["sys_status"].ToString());

            pnl_scan_detail.Visible = true;
            pnl_change_location.Visible = false;
            pnl_ng_remark.Visible = false;

            // ── ค่าปัจจุบันในระบบ (เฉพาะ Dept code / Model / Cell / Station) ──
            dc.QryDT_P(conn, @"
                SELECT ISNULL(T.dept_code,'-')  AS dept_code,
                       ISNULL(T.model_name,'-') AS model_name,
                       ISNULL(T.cell,'-')       AS cell_name,
                       ISNULL(T.station,'-')    AS station_name
                FROM Tool_Equipment_List_View T
                WHERE T.tool_id = @toolId AND T.sub_no = @toolSub AND T.sys_status = @sysStatus",
                new SqlParameter("@toolId", SafeInt(r["tool_id"].ToString())),
                new SqlParameter("@toolSub", r["tool_sub_no"].ToString()),
                new SqlParameter("@sysStatus", r["sys_status"].ToString()));

            string sysDept = "-", sysModel = "-", sysCell = "-", sysStation = "-";
            if (dc.dt.Rows.Count > 0)
            {
                DataRow s = dc.dt.Rows[0];
                sysDept = s["dept_code"].ToString();
                sysModel = s["model_name"].ToString();
                sysCell = s["cell_name"].ToString();
                sysStation = s["station_name"].ToString();
            }

            lbl_d_dept.Text = r["dept_code"].ToString();
            lbl_s_dept.Text = sysDept;
            lbl_s_model.Text = sysModel;
            lbl_s_cell.Text = sysCell;
            lbl_s_station.Text = sysStation;

            // เทียบเฉพาะ Dept Code / Model / Cell / Station : ตรง = match(เขียว), ไม่ตรง = mismatch(แดง)
            bool mmDept = !string.Equals(r["dept_code"].ToString(), sysDept, StringComparison.OrdinalIgnoreCase);
            bool mmModel = !string.Equals(r["model_name"].ToString(), sysModel, StringComparison.OrdinalIgnoreCase);
            bool mmCell = !string.Equals(r["cell_name"].ToString(), sysCell, StringComparison.OrdinalIgnoreCase);
            bool mmStation = !string.Equals(r["station_name"].ToString(), sysStation, StringComparison.OrdinalIgnoreCase);

            lbl_s_dept.CssClass = mmDept ? "v mismatch" : "v match";
            lbl_s_model.CssClass = mmModel ? "v mismatch" : "v match";
            lbl_s_cell.CssClass = mmCell ? "v mismatch" : "v match";
            lbl_s_station.CssClass = mmStation ? "v mismatch" : "v match";

            bool anyMismatch = mmDept || mmModel || mmCell || mmStation;
            pnl_mismatch_warn.Visible = anyMismatch;

            // ── ปิด/เปิดปุ่มตามสถานะตรวจ + ปิด Check OK เมื่อ Location ไม่ตรง ──
            bool alreadyChecked = checkId != 0;
            //btn_result_ok.Enabled = !alreadyChecked && !anyMismatch;   // ← ปิด OK ถ้า mismatch
            btn_result_change.Enabled = !alreadyChecked;
            btn_result_ng.Enabled = !alreadyChecked;
            btn_clear_check.Visible = alreadyChecked;

            if (isOutOfDept)
            {
                btn_result_ok.Text = "<span class='glyphicon glyphicon-map-marker'></span> Confirm New Location";
                btn_result_ok.OnClientClick = "return confirmResult('ok');";
                btn_result_ok.Enabled = !alreadyChecked;   // เปิดได้แม้ location mismatch
            }
            else
            {
                btn_result_ok.Text = "<span class='glyphicon glyphicon-ok'></span> Check OK";
                btn_result_ok.OnClientClick = "return confirmResult('ok');";
                btn_result_ok.Enabled = !alreadyChecked && !anyMismatch;
            }
        }

        private void Load_picture(string toolId, string toolSub, string sysStatus)
        {
            try
            {
                var fs = new Get_Folder_Structure_Picture(toolId, toolSub, sysStatus);
                string webPath = fs.get_folder_root_view_toolMain();
                string physical = fs.get_folder_root();

                if (!string.IsNullOrEmpty(physical) && Directory.Exists(physical))
                {
                    var files = Directory.GetFiles(physical, "*.jpg", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        img_scan.ImageUrl = webPath;
                        img_scan.Visible = true;
                        return;
                    }
                }
                img_scan.ImageUrl = "images/FilesType/jpeg.png";
                img_scan.Visible = true;
            }
            catch
            {
                img_scan.ImageUrl = "images/FilesType/jpeg.png";
                img_scan.Visible = true;
            }
        }

        // ─── ปุ่มผลลัพธ์ 3 ปุ่ม (update DB ทันทีทีละชิ้น) ──────────

        // Check OK → update check_id=1
        protected void btn_result_ok_Click(object sender, EventArgs e)
        {
            bool isOut = ViewState["OUT_OF_DEPT"] != null && (bool)ViewState["OUT_OF_DEPT"];

            pnl_change_location.Visible = false;
            pnl_ng_remark.Visible = false;

            if (isOut)
            {
                Update_result_ok_newdept();
                ShowToast("ยืนยัน New Location แล้ว", "สำเร็จ", "success", 1500);
            }
            else
            {
                Update_result_simple(RESULT_OK);
                ShowToast("บันทึกผล Check OK แล้ว", "สำเร็จ", "success", 1500);
            }
            Close_modal_and_reload();
        }

        // Not Found → เปิด panel กรอก remark (ยังไม่บันทึก)
        protected void btn_result_ng_Click(object sender, EventArgs e)
        {
            pnl_change_location.Visible = false;
            txt_ng_remark.Text = "";
            pnl_ng_remark.Visible = true;
            ScriptManager.RegisterStartupScript(this, GetType(),
                "keepOpenNg_" + DateTime.UtcNow.Ticks, "openTanaModal();", true);
        }
        // ยืนยัน Not Found → update check_id=3 + remark
        protected void btn_ng_confirm_Click(object sender, EventArgs e)
        {
            Update_result_ng(txt_ng_remark.Text.Trim());
            ShowToast("บันทึกผล Not Found แล้ว", "สำเร็จ", "warn", 1500);
            Close_modal_and_reload();
        }
        private void Update_result_ng(string remark)
        {
            int historyId = SafeInt(hd_history_id.Value);
            if (historyId <= 0) return;
            string empId = ViewState["EMP_ID"] != null ? ViewState["EMP_ID"].ToString() : "";
            string checkName = FormatName(txt_emp_name.Text.Trim());

            dc.RunCommand_P(conn,
                @"UPDATE Equipment_tana_history
          SET check_id = @checkId, check_by = @by, check_name = @checkName,
              check_date = GETDATE(), remark = @remark
          WHERE id = @id",
                new SqlParameter("@checkId", RESULT_NG),
                new SqlParameter("@by", (object)empId ?? DBNull.Value),
                new SqlParameter("@checkName", (object)checkName ?? DBNull.Value),
                new SqlParameter("@remark", string.IsNullOrEmpty(remark) ? (object)DBNull.Value : remark),
                new SqlParameter("@id", historyId));
        }
        // Confirm New Location : check_id=1 + dept_new = dept ของผู้กด confirm
        private void Update_result_ok_newdept()
        {
            int historyId = SafeInt(hd_history_id.Value);
            if (historyId <= 0) return;
            string empId = ViewState["EMP_ID"] != null ? ViewState["EMP_ID"].ToString() : "";
            string checkName = FormatName(txt_emp_name.Text.Trim());
            string deptCode = GetDeptCode();   // dept ของผู้ที่กด confirm

            dc.RunCommand_P(conn,
                @"UPDATE Equipment_tana_history
          SET check_id = @checkId, check_by = @by, check_name = @checkName,
              check_date = GETDATE(), dept_new = @deptNew
          WHERE id = @id",
                new SqlParameter("@checkId", RESULT_OK),
                new SqlParameter("@by", (object)empId ?? DBNull.Value),
                new SqlParameter("@checkName", (object)checkName ?? DBNull.Value),
                new SqlParameter("@deptNew", string.IsNullOrEmpty(deptCode) ? (object)DBNull.Value : deptCode),
                new SqlParameter("@id", historyId));
        }

        // Change Location → แสดง dropdown (ยังไม่บันทึก)
        protected void btn_result_change_Click(object sender, EventArgs e)
        {
            Load_change_dropdowns();
            pnl_change_location.Visible = true;
            pnl_ng_remark.Visible = false;
            ScriptManager.RegisterStartupScript(this, GetType(),
                "keepOpen_" + DateTime.UtcNow.Ticks, "openTanaModal(); initChangeSelect2();", true);
        }

        // ยืนยัน Change Location → update check_id=2 + ค่าใหม่
        protected void btn_change_confirm_Click(object sender, EventArgs e)
        {
            Update_result_change();
            ShowToast("บันทึก Change Location แล้ว", "สำเร็จ", "success", 1500);
            Close_modal_and_reload();
        }
        // Clear : ยกเลิกการตรวจนับชิ้นนี้ → เปิด 3 ปุ่มใหม่
        protected void btn_clear_check_Click(object sender, EventArgs e)
        {
            int historyId = SafeInt(hd_history_id.Value);
            if (historyId <= 0) return;

            dc.RunCommand_P(conn,
                @"UPDATE Equipment_tana_history
                  SET check_id = NULL, check_by = NULL, check_date = NULL, check_name = NULL,
                      dept_new = NULL, pic_rsp_new = NULL,
                      model_new = NULL, model_new_txt = NULL,
                      cell_new = NULL, cell_new_txt = NULL,
                      station_new = NULL, station_new_txt = NULL, remark = NULL
                  WHERE id = @id",
                new SqlParameter("@id", historyId));

            ShowToast("ล้างผลการตรวจแล้ว สามารถตรวจใหม่ได้", "Clear", "info", 1500);

            // โหลด detail ใหม่ (จะเปิด 3 ปุ่มให้เองเพราะ check_id = NULL)
            Load_detail_by_id(historyId.ToString());
            Load_tab1();
            Load_tab2();
            ScriptManager.RegisterStartupScript(this, GetType(),
                "reopen_" + DateTime.UtcNow.Ticks, "openTanaModal();", true);
        }

        // ─── UPDATE : Check OK / Not Found ────────────────────────
        private void Update_result_simple(int checkId)
        {
            int historyId = SafeInt(hd_history_id.Value);
            if (historyId <= 0) return;
            string empId = ViewState["EMP_ID"] != null ? ViewState["EMP_ID"].ToString() : "";
            string checkName = FormatName(txt_emp_name.Text.Trim());

            dc.RunCommand_P(conn,
                @"UPDATE Equipment_tana_history
                  SET check_id = @checkId, check_by = @by, check_name = @checkName, check_date = GETDATE()
                  WHERE id = @id",
                new SqlParameter("@checkId", checkId),
                new SqlParameter("@by", (object)empId ?? DBNull.Value),
                new SqlParameter("@checkName", (object)checkName ?? DBNull.Value),
                new SqlParameter("@id", historyId));
        }

        // ─── UPDATE : Change Location (id ถ้าเลือก / text ถ้า Add New) ─
        private void Update_result_change()
        {
            int historyId = SafeInt(hd_history_id.Value);
            if (historyId <= 0) return;
            string empId = ViewState["EMP_ID"] != null ? ViewState["EMP_ID"].ToString() : "";
            string checkName = FormatName(txt_emp_name.Text.Trim());

            // Dept / Responsible (dropdown ปกติ — เก็บ value)
            string deptNew = ddl_new_dept.SelectedValue;
            string respNew = ddl_new_resp.SelectedValue;

            // Model : ถ้า toggle=txt ใช้ค่า textbox / ไม่งั้นใช้ dropdown id
            bool modelIsTxt = hd_model_mode.Value == "txt";
            object modelNewId = (!modelIsTxt && !string.IsNullOrEmpty(ddl_new_model.SelectedValue))
                                ? (object)SafeInt(ddl_new_model.SelectedValue) : DBNull.Value;
            object modelNewTxt = modelIsTxt && !string.IsNullOrEmpty(txt_model_new.Text.Trim())
                                ? (object)txt_model_new.Text.Trim() : DBNull.Value;

            bool cellIsTxt = hd_cell_mode.Value == "txt";
            object cellNewId = (!cellIsTxt && !string.IsNullOrEmpty(ddl_new_cell.SelectedValue))
                                ? (object)SafeInt(ddl_new_cell.SelectedValue) : DBNull.Value;
            object cellNewTxt = cellIsTxt && !string.IsNullOrEmpty(txt_cell_new.Text.Trim())
                                ? (object)txt_cell_new.Text.Trim() : DBNull.Value;

            bool stationIsTxt = hd_station_mode.Value == "txt";
            object stationNewId = (!stationIsTxt && !string.IsNullOrEmpty(ddl_new_station.SelectedValue))
                                ? (object)SafeInt(ddl_new_station.SelectedValue) : DBNull.Value;
            object stationNewTxt = stationIsTxt && !string.IsNullOrEmpty(txt_station_new.Text.Trim())
                                ? (object)txt_station_new.Text.Trim() : DBNull.Value;

            string remark = txt_change_remark.Text.Trim();

            dc.RunCommand_P(conn,
                @"UPDATE Equipment_tana_history SET
                    check_id = 2,
                    dept_new = @deptNew,
                    pic_rsp_new = @respNew,
                    model_new = @modelNewId,     model_new_txt   = @modelNewTxt,
                    cell_new = @cellNewId,       cell_new_txt    = @cellNewTxt,
                    station_new = @stationNewId, station_new_txt = @stationNewTxt,
                    remark = @remark,
                    check_by = @by, check_name = @checkName, check_date = GETDATE()
                  WHERE id = @id",
                new SqlParameter("@deptNew", string.IsNullOrEmpty(deptNew) ? (object)DBNull.Value : deptNew),
                new SqlParameter("@respNew", string.IsNullOrEmpty(respNew) ? (object)DBNull.Value : respNew),
                new SqlParameter("@modelNewId", modelNewId),
                new SqlParameter("@modelNewTxt", modelNewTxt),
                new SqlParameter("@cellNewId", cellNewId),
                new SqlParameter("@cellNewTxt", cellNewTxt),
                new SqlParameter("@stationNewId", stationNewId),
                new SqlParameter("@stationNewTxt", stationNewTxt),
                new SqlParameter("@remark", string.IsNullOrEmpty(remark) ? (object)DBNull.Value : remark),
                new SqlParameter("@by", (object)empId ?? DBNull.Value),
                new SqlParameter("@checkName", (object)checkName ?? DBNull.Value),
                new SqlParameter("@id", historyId));
        }

        // ─── ปิด modal + reload Tab (หลังกดผลเสร็จ) ────────────────
        private void Close_modal_and_reload()
        {
            pnl_scan_detail.Visible = false;
            pnl_change_location.Visible = false;
            pnl_ng_remark.Visible = false;
            // reset toggle state
            hd_model_mode.Value = "ddl";
            hd_cell_mode.Value = "ddl";
            hd_station_mode.Value = "ddl";
            Load_tab1();
            Load_tab2();
            ScriptManager.RegisterStartupScript(this, GetType(),
                "closeDetail_" + DateTime.UtcNow.Ticks,
                "$('#modalScan').modal('hide'); switchTab(2); focusScanBox();", true);
        }

        // ─── Dropdown สำหรับ Change Location ──────────────────────
        private void Load_change_dropdowns()
        {
            // Model
            dc.QryDT_P(conn_jig, "SELECT model_id, model_name FROM draw_master_model WHERE model_check=1 ORDER BY model_name");
            ddl_new_model.Items.Clear();
            ddl_new_model.Items.Add(new ListItem("-- เลือก Model --", ""));
            foreach (DataRow r in dc.dt.Rows)
                ddl_new_model.Items.Add(new ListItem(r["model_name"].ToString(), r["model_id"].ToString()));

            // Cell (location2) — ผูกกับ location1 = 97 (fix)
            dc.QryDT_P(conn_jig, "SELECT location2_id, location2_name FROM master_location2 WHERE location1_id=97 ORDER BY location2_name");
            ddl_new_cell.Items.Clear();
            ddl_new_cell.Items.Add(new ListItem("-- เลือก Cell --", ""));
            foreach (DataRow r in dc.dt.Rows)
                ddl_new_cell.Items.Add(new ListItem(r["location2_name"].ToString(), r["location2_id"].ToString()));

            // Station (location3)
            dc.QryDT_P(conn_jig, "SELECT location3_id, location3_name FROM master_location3 WHERE location1_id=97 ORDER BY location3_name");
            ddl_new_station.Items.Clear();
            ddl_new_station.Items.Add(new ListItem("-- เลือก Station --", ""));
            foreach (DataRow r in dc.dt.Rows)
                ddl_new_station.Items.Add(new ListItem(r["location3_name"].ToString(), r["location3_id"].ToString()));

            // Dept — จาก tana (distinct dept ที่มีในระบบ) + master dept
            dc.QryDT_P(conn, @"
                SELECT DISTINCT D.dept_code, ISNULL(D.dept_name2, D.dept_code) AS dept_name
                FROM View_dept_master D
                ORDER BY dept_name");
            ddl_new_dept.Items.Clear();
            ddl_new_dept.Items.Add(new ListItem("-- เลือก Dept --", ""));
            foreach (DataRow r in dc.dt.Rows)
                ddl_new_dept.Items.Add(new ListItem(r["dept_name"].ToString(), r["dept_code"].ToString()));

            // Responsible — จาก tana (Responsible_Dept ที่เคยใช้)
            dc.QryDT_P(conn, @"
                SELECT DISTINCT emp_id, ISNULL(pic_dept, emp_id) AS pic_name
                FROM Responsible_Dept
                WHERE emp_id IS NOT NULL
                ORDER BY pic_name");
            ddl_new_resp.Items.Clear();
            ddl_new_resp.Items.Add(new ListItem("-- เลือก Responsible --", ""));
            foreach (DataRow r in dc.dt.Rows)
                ddl_new_resp.Items.Add(new ListItem(r["pic_name"].ToString(), r["emp_id"].ToString()));

            // reset textbox Add New + toggle state
            txt_model_new.Text = "";
            txt_cell_new.Text = "";
            txt_station_new.Text = "";
            hd_model_mode.Value = "ddl";
            hd_cell_mode.Value = "ddl";
            hd_station_mode.Value = "ddl";
            txt_change_remark.Text = "";
        }

        // ─── ปุ่ม Clear Filter : ล้าง Responsible / Model / Cell ────
        protected void btn_clear_filter_Click(object sender, EventArgs e)
        {
            if (ddl_Responsible.Items.Count > 0) ddl_Responsible.SelectedIndex = 0;
            if (ddl_model.Items.Count > 0) ddl_model.SelectedIndex = 0;
            if (ddl_cell.Items.Count > 0) ddl_cell.SelectedIndex = 0;

            Load_tab1();
            Load_tab2();
            ScriptManager.RegisterStartupScript(this, GetType(),
                "clrflt_" + DateTime.UtcNow.Ticks,
                "initFilterSelect2();", true);
        }

        // filter หลักของ tab2 : pic_rsp ในขอบเขต dept  OR  ชิ้นที่ถูก confirm มาที่ dept นี้ (dept_new)
        private string BuildTab2ScopeFilter(string alias, List<SqlParameter> prms)
        {
            string rspFilter = BuildPicRspFilter(alias, prms);   // " AND alias.pic_rsp IN (...) "

            string deptCode = GetDeptCode();
            if (string.IsNullOrEmpty(deptCode)) return rspFilter;

            prms.Add(new SqlParameter("@deptNewScope", deptCode));
            // แปลง " AND (cond) " → " AND ( (cond) OR alias.dept_new=@deptNewScope ) "
            string inner = rspFilter.Trim();
            if (inner.StartsWith("AND ")) inner = inner.Substring(4);
            return " AND ( " + inner + " OR " + alias + ".dept_new = @deptNewScope ) ";
        }

        // ═══════════════════════════════════════════════════════════
        //  Helpers
        // ═══════════════════════════════════════════════════════════

        // เก็บ emp_id ทั้งหมดของ dept นี้ (จาก Responsible_Dept)
        private List<string> GetRspEmpIds()
        {
            var ids = new List<string>();
            if (ViewState["RSP_EMP_IDS"] != null)
            {
                string s = ViewState["RSP_EMP_IDS"].ToString();
                if (!string.IsNullOrEmpty(s))
                {
                    ids.AddRange(s.Split(','));
                }
            }
            return ids;
        }

        // สร้าง IN clause + parameters สำหรับ pic_rsp
        // selectedEmp : ถ้าเลือกใน ddl_Responsible = ตัวเดียว, ถ้า All = ทั้งหมด
        private string BuildPicRspFilter(string alias, List<SqlParameter> prms)
        {
            List<string> ids;
            string sel = ddl_Responsible.SelectedValue;   // "" = All
            if (!string.IsNullOrEmpty(sel))
                ids = new List<string> { sel };
            else
                ids = GetRspEmpIds();

            if (ids.Count == 0) return " AND 1=0 ";        // ไม่มี emp = ไม่แสดง

            var ph = new List<string>();
            for (int i = 0; i < ids.Count; i++)
            {
                string p = "@rsp" + i;
                ph.Add(p);
                prms.Add(new SqlParameter(p, ids[i].Trim()));
            }
            return " AND " + alias + ".pic_rsp IN (" + string.Join(",", ph) + ") ";
        }

        // 3 ตัวแรกของ dept code ผู้ตรวจ ใช้ค้นแบบเดียวกัน (5510 -> 551)
        private string GetDeptPrefix()
        {
            string d = GetDeptCode();
            return d.Length >= 3 ? d.Substring(0, 3) : d;
        }
        private int SafeInt(string s) { int v; return int.TryParse(s, out v) ? v : -1; }

        public static string FormatName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return string.Empty;

            string[] parts = fullName.Trim()
                                     .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                return fullName;

            string firstName = parts[0].ToLower();
            firstName = char.ToUpper(firstName[0]) + firstName.Substring(1);

            char lastInitial = char.ToUpper(parts[parts.Length - 1][0]);

            return string.Format("{0} {1}.", firstName, lastInitial);
        }

        private void ShowToast(string detail, string title, string type, int duration = 3000)
        {
            string script = string.Format(
                "showToast('{0}',{{title:'{1}',type:'{2}',duration:{3}}});",
                HttpUtility.JavaScriptStringEncode(detail ?? ""),
                HttpUtility.JavaScriptStringEncode(title ?? ""),
                HttpUtility.JavaScriptStringEncode(type ?? "info"),
                duration);
            ScriptManager.RegisterStartupScript(this, GetType(),
                "toast_" + DateTime.UtcNow.Ticks, script, true);
        }
    }
}
