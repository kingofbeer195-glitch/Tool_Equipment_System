using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tool_Equipment_Control_System
{
    public partial class AutoBot : System.Web.UI.Page
    {
        // ══════════════════════════════
        //  CONFIG
        // ══════════════════════════════
        private const string DOC_ID = "AAD1";
        private const int STATUS_ID = 90;

        private const string SMTP_HOST = "150.61.249.70";
        private const int SMTP_PORT = 25;
        private const string MAIL_FROM = "engineer_pde@mail.canon";
        private const string MAIL_TO_DEFAULT = "anirut@mail.canon";
        private const string MAIL_CC_ALWAYS = "anirut@mail.canon";

        private const string HRMS_URL = "http://cptsvs547:1000/middleware/oracle/hrms";

        // ── โหมดทดสอบ ──────────────────────────────────
        //  true  = ส่งเข้า TEST_MAIL_TO อย่างเดียว (แต่ยังคำนวณ To/CC จริงและแสดงให้ดูใน Log)
        //  false = ส่งจริงตาม HRMS
        //  *** ก่อนขึ้น Production เปลี่ยนเป็น false ***
        private const bool TEST_MODE = false;
        private const string TEST_MAIL_TO = "anirut@mail.canon";

        private static string ConnStr
        {
            get
            {
                ConnectionStringSettings cs =
                    ConfigurationManager.ConnectionStrings["tool_equipment"];
                if (cs == null)
                    throw new ConfigurationErrorsException(
                        "ไม่พบ ConnectionString ชื่อ 'tool_equipment' ใน web.config");
                return cs.ConnectionString;
            }
        }

        private string CurrentEmpId
        {
            get { return Session["emp_id"] != null ? Session["emp_id"].ToString() : "SYSTEM"; }
        }

        // ══════════════════════════════
        //  HRMS MODEL
        // ══════════════════════════════
        private class HrmsUser
        {
            public string EmpId;
            public string Name;      // GNAME_ENG + FNAME_ENG
            public string Dept;      // DEPT_ABB_NAME (DEPT_CODE)
            public string Email;     // EMAIL

            public bool HasEmail
            {
                get { return !string.IsNullOrEmpty(Email) && Email.Contains("@"); }
            }
        }

        // cache ต่อ 1 request กัน HTTP ซ้ำ (คนเดิมโผล่หลายเอกสาร)
        private Dictionary<string, HrmsUser> _hrmsCache;
        private Dictionary<string, HrmsUser> HrmsCache
        {
            get
            {
                if (_hrmsCache == null)
                    _hrmsCache = new Dictionary<string, HrmsUser>(StringComparer.OrdinalIgnoreCase);
                return _hrmsCache;
            }
        }

        private static bool IsValidEmpNo(string userId)
        {
            return !string.IsNullOrEmpty(userId) &&
                   Regex.IsMatch(userId, "^[A-Za-z0-9]{1,20}$");
        }

        /// <summary>ดึงข้อมูลพนักงานจาก HRMS middleware (คืน null ถ้าไม่พบ)</summary>
        private HrmsUser GetHrmsUser(string empId)
        {
            if (string.IsNullOrEmpty(empId)) return null;
            empId = empId.Trim();

            if (HrmsCache.ContainsKey(empId)) return HrmsCache[empId];
            if (!IsValidEmpNo(empId)) { HrmsCache[empId] = null; return null; }

            HrmsUser user = null;
            try
            {
                string sql =
                    "select GNAME_ENG, FNAME_ENG, POSN_ENAME, BAND, DEPT_CODE, DEPT_ABB_NAME, EMAIL " +
                    "from ADMIN.v_emp_data_all_cpt where UPPER(emp_no)=UPPER('" + empId + "')";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(HRMS_URL);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 15000;

                byte[] data = Encoding.UTF8.GetBytes("command=" + HttpUtility.UrlEncode(sql));
                using (Stream stream = request.GetRequestStream())
                    stream.Write(data, 0, data.Length);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string json = reader.ReadToEnd();
                    dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                    if (result != null && result.data != null && result.data.Count > 0)
                    {
                        var u = result.data[0];
                        user = new HrmsUser();
                        user.EmpId = empId;
                        user.Name = Convert.ToString(u.GNAME_ENG) + " " + Convert.ToString(u.FNAME_ENG);
                        user.Dept = Convert.ToString(u.DEPT_ABB_NAME) +
                                    " (" + Convert.ToString(u.DEPT_CODE) + ")";
                        user.Email = Convert.ToString(u.EMAIL);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[HRMS Error] " + empId + " : " + ex.Message);
            }

            HrmsCache[empId] = user;
            return user;
        }

        /// <summary>emp_id ของ Admin ระดับ level_1</summary>
        private List<string> GetAdminLevel1EmpIds()
        {
            List<string> list = new List<string>();
            try
            {
                string sql =
                    "SELECT DISTINCT emp_id FROM Equipment_Admin " +
                    "WHERE level_1 = 1 AND ISNULL(status_use, 1) = 1 AND emp_id IS NOT NULL";

                using (SqlConnection cn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string id = Convert.ToString(dr["emp_id"]).Trim();
                            if (id.Length > 0) list.Add(id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Admin Error] " + ex.Message);
            }
            return list;
        }

        // ══════════════════════════════
        //  PAGE LIFECYCLE
        // ══════════════════════════════
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSetting();
                LoadAlertList();
            }
        }

        // ══════════════════════════════
        //  TAB
        // ══════════════════════════════
        protected void Tab_Click(object sender, EventArgs e)
        {
            LinkButton lb = sender as LinkButton;
            if (lb == null) return;

            int idx = 0;
            int.TryParse(lb.CommandArgument, out idx);
            mvBot.ActiveViewIndex = idx;

            tabAlert.CssClass = idx == 0 ? "bot-tab active" : "bot-tab";
            tabLog.CssClass = idx == 1 ? "bot-tab active" : "bot-tab";
            tabSetting.CssClass = idx == 2 ? "bot-tab active" : "bot-tab";

            HideMessage();

            if (idx == 0) LoadAlertList();
            else if (idx == 1) LoadLog();
            else LoadSetting();
        }

        // ══════════════════════════════
        //  LOAD DATA
        // ══════════════════════════════
        private DataTable GetAlertData()
        {
            DataTable dt = new DataTable();

            string sql =
                "SELECT status_name, Doc_No, document_id, document_name, " +
                "       issue_empid, dept_code, return_duedate " +
                "FROM   View_Equipment_Request_List " +
                "WHERE  document_id = @doc AND status_id = @sts ";

            string keyword = txtKeyword.Text.Trim();
            if (keyword.Length > 0)
                sql += "AND (Doc_No LIKE @kw OR issue_empid LIKE @kw OR dept_code LIKE @kw) ";

            sql += "ORDER BY return_duedate ASC, Doc_No ASC";

            using (SqlConnection cn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandTimeout = 120;
                cmd.Parameters.AddWithValue("@doc", DOC_ID);
                cmd.Parameters.AddWithValue("@sts", STATUS_ID);
                if (keyword.Length > 0)
                    cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");

                cn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd)) { da.Fill(dt); }
            }

            string mode = ddlDue.SelectedValue;
            if (mode != "ALL")
            {
                DateTime today = DateTime.Today;
                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    object v = dt.Rows[i]["return_duedate"];
                    if (v == null || v == DBNull.Value) { dt.Rows.RemoveAt(i); continue; }

                    int days = (Convert.ToDateTime(v).Date - today).Days;
                    bool keep = (mode == "OVERDUE") ? (days < 0) : (days >= 0 && days <= 3);
                    if (!keep) dt.Rows.RemoveAt(i);
                }
                dt.AcceptChanges();
            }

            return dt;
        }

        private void LoadAlertList()
        {
            try
            {
                DataTable dt = GetAlertData();
                gvAlert.DataSource = dt;
                gvAlert.DataBind();
                litCount.Text = "พบ " + dt.Rows.Count.ToString("#,##0") + " รายการ";
            }
            catch (Exception ex)
            {
                ShowMessage("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, false);
            }
        }

        private void LoadLog()
        {
            try
            {
                DataTable dt = new DataTable();
                string sql =
                    "SELECT TOP 500 send_date, Doc_No, document_id, mail_to, " +
                    "       send_type, send_result, send_by " +
                    "FROM   AutoAlert_Log ORDER BY send_date DESC";

                using (SqlConnection cn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd)) { da.Fill(dt); }
                }

                gvLog.DataSource = dt;
                gvLog.DataBind();
            }
            catch (Exception ex)
            {
                ShowMessage("โหลด Log ไม่สำเร็จ: " + ex.Message, false);
            }
        }

        // ══════════════════════════════
        //  GRID EVENTS
        // ══════════════════════════════
        protected void gvAlert_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            DataRowView drv = e.Row.DataItem as DataRowView;
            if (drv == null) return;

            Literal litNo = (Literal)e.Row.FindControl("litNo");
            if (litNo != null) litNo.Text = (e.Row.RowIndex + 1).ToString();

            Literal litDue = (Literal)e.Row.FindControl("litDue");
            Literal litTag = (Literal)e.Row.FindControl("litDueTag");

            object v = drv["return_duedate"];
            if (v == null || v == DBNull.Value)
            {
                if (litDue != null) litDue.Text = "-";
                if (litTag != null) litTag.Text = "<span class='pill pill-ok'>ไม่ระบุ</span>";
                return;
            }

            DateTime due = Convert.ToDateTime(v).Date;
            int days = (due - DateTime.Today).Days;

            if (litDue != null) litDue.Text = due.ToString("dd/MM/yyyy");

            if (litTag != null)
            {
                if (days < 0)
                {
                    litTag.Text = "<span class='pill pill-late'>เกิน " + Math.Abs(days) + " วัน</span>";
                    e.Row.CssClass = "row-overdue";
                }
                else if (days <= 3)
                    litTag.Text = "<span class='pill pill-warn'>เหลือ " + days + " วัน</span>";
                else
                    litTag.Text = "<span class='pill pill-ok'>เหลือ " + days + " วัน</span>";
            }
        }

        protected void gvAlert_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "SendOne") return;

            string docNo = Convert.ToString(e.CommandArgument);
            if (string.IsNullOrEmpty(docNo)) return;

            DataTable dt = GetAlertData();
            DataTable one = dt.Clone();
            foreach (DataRow r in dt.Rows)
            {
                if (Convert.ToString(r["Doc_No"]) == docNo) { one.ImportRow(r); break; }
            }

            if (one.Rows.Count == 0)
            {
                ShowMessage("ไม่พบเอกสาร " + docNo + " (อาจถูกปรับสถานะไปแล้ว)", false);
                LoadAlertList();
                return;
            }

            DoSend(one, "MANUAL");
        }

        // ══════════════════════════════
        //  BUTTONS
        // ══════════════════════════════
        protected void btnSearch_Click(object sender, EventArgs e) { HideMessage(); LoadAlertList(); }

        protected void btnReload_Click(object sender, EventArgs e)
        {
            HideMessage();
            txtKeyword.Text = "";
            ddlDue.SelectedValue = "ALL";
            LoadAlertList();
        }

        protected void btnSendSelected_Click(object sender, EventArgs e)
        {
            List<string> selected = new List<string>();
            foreach (GridViewRow row in gvAlert.Rows)
            {
                CheckBox chk = row.FindControl("chkSel") as CheckBox;
                if (chk != null && chk.Checked)
                    selected.Add(Convert.ToString(gvAlert.DataKeys[row.RowIndex].Value));
            }

            if (selected.Count == 0)
            {
                ShowMessage("กรุณาเลือกรายการที่ต้องการส่ง E-Mail อย่างน้อย 1 รายการ", false);
                return;
            }

            DataTable dt = GetAlertData();
            DataTable send = dt.Clone();
            foreach (DataRow r in dt.Rows)
            {
                if (selected.Contains(Convert.ToString(r["Doc_No"])))
                    send.ImportRow(r);
            }

            DoSend(send, "MANUAL");
        }

        // ══════════════════════════════
        //  RECIPIENTS
        // ══════════════════════════════
        /// <summary>
        /// To = E-Mail ของผู้ยืม (issue_empid ในรายการที่ส่ง) — ไม่ซ้ำ
        /// CC = E-Mail ของ Admin level_1 + ที่กรอกเพิ่มในแท็บ Setting
        /// </summary>
        private void BuildRecipients(DataTable dt, out List<string> to, out List<string> cc,
                                     out List<string> notFound)
        {
            to = new List<string>();
            cc = new List<string>();
            notFound = new List<string>();

            // ── To : ผู้ยืม ──
            foreach (DataRow r in dt.Rows)
            {
                string empId = Convert.ToString(r["issue_empid"]).Trim();
                if (empId.Length == 0) continue;

                HrmsUser u = GetHrmsUser(empId);
                if (u != null && u.HasEmail)
                    AddDistinct(to, u.Email);
                else
                    AddDistinct(notFound, empId);
            }

            // ── CC : Admin level_1 ──
            List<string> adminIds = GetAdminLevel1EmpIds();
            foreach (string adminId in adminIds)
            {
                HrmsUser u = GetHrmsUser(adminId);
                if (u != null && u.HasEmail) AddDistinct(cc, u.Email);
            }

            // ── CC เพิ่มเติมจากหน้า Setting ──
            string extraCc = txtMailCc.Text.Trim();
            if (extraCc.Length > 0)
            {
                string[] parts = extraCc.Split(new char[] { ';', ',' },
                                               StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++)
                {
                    string a = parts[i].Trim();
                    if (a.Contains("@")) AddDistinct(cc, a);
                }                
            }
            //Add CC 
            AddDistinct(cc, MAIL_CC_ALWAYS);

            // กัน To/CC ซ้ำกัน (ถ้าอยู่ใน To แล้ว ตัดออกจาก CC)
            for (int i = cc.Count - 1; i >= 0; i--)
            {
                if (ListContains(to, cc[i])) cc.RemoveAt(i);
            }
        }

        private static bool ListContains(List<string> list, string value)
        {
            foreach (string s in list)
                if (string.Equals(s, value, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static void AddDistinct(List<string> list, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            value = value.Trim();
            foreach (string s in list)
                if (string.Equals(s, value, StringComparison.OrdinalIgnoreCase)) return;
            list.Add(value);
        }

        private void DoSend(DataTable dt, string sendType)
        {
            List<string> to, cc, notFound;
            BuildRecipients(dt, out to, out cc, out notFound);

            if (to.Count == 0 && !TEST_MODE)
            {
                ShowMessage("ไม่พบ E-Mail ของผู้ยืมใน HRMS (emp_id: " +
                            string.Join(", ", notFound.ToArray()) + ")", false);
                return;
            }

            string err;
            bool ok = SendEmailNotification(dt, sendType, to, cc, out err);

            string toLog = string.Join(";", to.ToArray());
            if (TEST_MODE) toLog = "[TEST->" + TEST_MAIL_TO + "] real=" + toLog;

            WriteLog(dt, sendType, toLog, ok ? "SUCCESS" : ("FAIL: " + err));

            if (ok)
            {
                string msg = "ส่ง E-Mail สำเร็จ " + dt.Rows.Count + " รายการ";
                msg += TEST_MODE
                     ? " &nbsp;<b>[TEST MODE]</b> ส่งจริงไปที่ " + Server.HtmlEncode(TEST_MAIL_TO) +
                       "<br><small>To (จริง): " + Server.HtmlEncode(string.Join("; ", to.ToArray())) +
                       "<br>CC (จริง): " + Server.HtmlEncode(string.Join("; ", cc.ToArray())) + "</small>"
                     : "<br><small>To: " + Server.HtmlEncode(string.Join("; ", to.ToArray())) +
                       "<br>CC: " + Server.HtmlEncode(string.Join("; ", cc.ToArray())) + "</small>";

                if (notFound.Count > 0)
                    msg += "<br><small style='color:#c2410c;'>&#9888; ไม่พบ E-Mail ใน HRMS: " +
                           Server.HtmlEncode(string.Join(", ", notFound.ToArray())) + "</small>";

                ShowMessage(msg, true);
            }
            else
            {
                ShowMessage("ส่ง E-Mail ไม่สำเร็จ: " + Server.HtmlEncode(err), false);
            }

            LoadAlertList();
        }

        // ══════════════════════════════
        //  SEND EMAIL
        // ══════════════════════════════
        private bool SendEmailNotification(DataTable dt, string sendType,
                                           List<string> to, List<string> cc, out string error)
        {
            error = "";
            try
            {
                string empId = CurrentEmpId;
                string today = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                int cntOverdue = 0, cntNear = 0;
                foreach (DataRow r in dt.Rows)
                {
                    object v = r["return_duedate"];
                    if (v == null || v == DBNull.Value) continue;
                    int d = (Convert.ToDateTime(v).Date - DateTime.Today).Days;
                    if (d < 0) cntOverdue++; else cntNear++;
                }

                string htmlBody = BuildHtmlBody(dt, today, cntOverdue, cntNear, to, cc);
                string plainBody = BuildPlainTextBody(dt, sendType, empId, today, cntOverdue, cntNear, to, cc);

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(MAIL_FROM);

                    if (TEST_MODE)
                    {
                        mail.To.Add(TEST_MAIL_TO);
                    }
                    else
                    {
                        foreach (string a in to) mail.To.Add(a);
                        foreach (string a in cc) mail.CC.Add(a);
                        if (mail.To.Count == 0) mail.To.Add(MAIL_TO_DEFAULT);
                    }

                    mail.Subject = (TEST_MODE ? "[TEST] " : "") +
                                   "[Jig Tool & Equipment System] Return Alert (" +
                                   dt.Rows.Count + " รายการ) " + DateTime.Now.ToString("dd/MM/yyyy");
                    mail.SubjectEncoding = Encoding.UTF8;

                    // ⛔ ห้ามใส่ mail.Body / IsBodyHtml ที่นี่
                    //    ถ้าใส่ .NET จะเอา Body ไปเป็น part หนึ่งของ multipart ด้วย
                    //    ทำให้ HTML กลายเป็น part สุดท้าย → client เลือก HTML เสมอ
                    //    และ plain text ที่เราสร้างไว้จะไม่ถูกใช้งาน
                    AlternateView avText = AlternateView.CreateAlternateViewFromString(
                        plainBody, Encoding.UTF8, "text/plain");
                    AlternateView avHtml = AlternateView.CreateAlternateViewFromString(
                        htmlBody, Encoding.UTF8, "text/html");

                    avText.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                    avHtml.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;

                    mail.AlternateViews.Add(avText);   // ลำดับสำคัญ: plain ต้องมาก่อน html
                    mail.AlternateViews.Add(avHtml);

                    SmtpClient client = new SmtpClient();
                    client.Host = SMTP_HOST;
                    client.Port = SMTP_PORT;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Send(mail);
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                System.Diagnostics.Debug.WriteLine("[Email Error] " + ex.Message);
                return false;
            }
        }

        // ══════════════════════════════
        //  HTML BODY
        //  จัดเป็น "บล็อกต่อรายการ" (label : value) ไม่ใช้ตารางกว้างหลายคอลัมน์
        //  เหตุผล: Outlook ที่ผู้รับตั้งค่าอ่านแบบ plain text จะหยิบ part HTML
        //          มาแปลงเป็นข้อความเอง ตารางกว้างจะแตก แต่ตาราง 2 คอลัมน์
        //          จะถูกแปลงเป็น "หัวข้อ<tab>ค่า" ซึ่งยังอ่านรู้เรื่อง
        // ══════════════════════════════
        private string BuildHtmlBody(DataTable dt, string today, int cntOverdue, int cntNear,
                                     List<string> to, List<string> cc)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(@"
                <html><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8' /></head>
                <body style='font-family:Segoe UI,Tahoma,Arial,sans-serif; font-size:14px; color:#1e293b;'>

                <p style='font-size:18px; font-weight:700; margin:0 0 4px;'>Equipment Return Alert</p>
                <p style='margin:0 0 18px;'>แจ้งเตือน: รายการยืมอุปกรณ์ที่ถึงกำหนด / เกินกำหนดคืน</p>

                <table style='border-collapse:collapse; margin-bottom:18px;'>
                    <tr>
                        <td style='padding:3px 10px 3px 0; color:#475569; white-space:nowrap;'>วันที่แจ้งเตือน : </td>
                        <td style='padding:3px 0; font-weight:600;'>" + today + @"</td>
                    </tr>
                    <tr>
                        <td style='padding:3px 10px 3px 0; color:#475569; white-space:nowrap;'>รายการทั้งหมด : </td>
                        <td style='padding:3px 0; font-weight:600;'>" + dt.Rows.Count + @" รายการ
                    </tr>
                </table>

                <p style='font-weight:700; margin:0 0 6px;'>รายละเอียดการยืม</p>
                <hr style='border:none; border-top:1px solid #cbd5e1; margin:0 0 14px;' />");

            int no = 1;
            foreach (DataRow r in dt.Rows)
            {
                object v = r["return_duedate"];
                string dueTxt = "-";
                string dueColor = "#1e293b";

                if (v != null && v != DBNull.Value)
                {
                    DateTime due = Convert.ToDateTime(v).Date;
                    int d = (due - DateTime.Today).Days;
                    dueTxt = due.ToString("dd/MM/yyyy");
                    if (d < 0) { dueTxt += "  *** เกินกำหนด " + Math.Abs(d) + " วัน ***"; dueColor = "#b91c1c"; }
                    else if (d <= 3) { dueTxt += "  (!) เหลืออีก " + d + " วัน"; dueColor = "#c2410c"; }
                    else { dueTxt += "  (เหลืออีก " + d + " วัน)"; }
                }

                string borrowerId = Convert.ToString(r["issue_empid"]).Trim();
                HrmsUser u = GetHrmsUser(borrowerId);
                string nameTxt = (u != null && !string.IsNullOrEmpty(u.Name))
                               ? u.Name + " (" + borrowerId + ")" : borrowerId;
                string deptTxt = (u != null && !string.IsNullOrEmpty(u.Dept))
                               ? u.Dept : Convert.ToString(r["dept_code"]);

                sb.AppendLine(
                    "<p style='margin:0 0 4px; font-weight:700;'>[" + no + "] " +
                        Server.HtmlEncode(Convert.ToString(r["Doc_No"])) + "</p>" +
                    "<table style='border-collapse:collapse; margin:0 0 10px 0;'>" +
                    HtmlRow("Document Name : ", Convert.ToString(r["document_name"]), null) +
                    HtmlRow("Status : ", Convert.ToString(r["status_name"]), null) +
                    HtmlRow("ผู้ยืม : ", nameTxt, null) +
                    HtmlRow("Department : ", deptTxt, null) +
                    HtmlRow("Return Due : ", dueTxt, dueColor) +
                    "</table>" +
                    "<hr style='border:none; border-top:1px solid #e2e8f0; margin:0 0 14px;' />");
                no++;
            }

            sb.AppendLine(
                "<br>" +
                "<div style='color:#94a3b8; font-size:11px; line-height:1.8;'>" +
                "****************************************************************<br>" +
                "This auto-email was sent from Tool &amp; Equipment System.<br>" +
                "Best regard.<br>" +
                "--------------------- Original Message Ends --------------------<br>" +
                "****************************************************************" +
                "</div></body></html>");

            return sb.ToString();
        }

        /// <summary>แถว label : value ของตาราง 2 คอลัมน์ (แปลงเป็น plain text ได้สวย)</summary>
        private string HtmlRow(string label, string value, string color)
        {
            string style = "padding:2px 0; " +
                           (string.IsNullOrEmpty(color) ? "" : "color:" + color + "; font-weight:600;");
            return "<tr>" +
                   "<td style='padding:2px 14px 2px 0; color:#475569; white-space:nowrap; vertical-align:top;'>" +
                        Server.HtmlEncode(label) + "</td>" +
                   "<td style='" + style + "'>" + Server.HtmlEncode(value) + "</td>" +
                   "</tr>";
        }

        // ══════════════════════════════
        //  PLAIN TEXT BODY
        //  จัดเป็นบล็อกต่อรายการ ไม่ใช้ตารางแบบเว้นวรรค
        //  เพราะฟอนต์ไทย/อังกฤษความกว้างไม่เท่ากัน คอลัมน์จะเพี้ยน
        // ══════════════════════════════
        private string BuildPlainTextBody(DataTable dt, string sendType, string empId,
                                          string today, int cntOverdue, int cntNear,
                                          List<string> to, List<string> cc)
        {
            const string LINE = "================================================================";
            const string DASH = "----------------------------------------------------------------";

            StringBuilder t = new StringBuilder();

            t.AppendLine(LINE);
            t.AppendLine("  EQUIPMENT RETURN ALERT");
            t.AppendLine("  แจ้งเตือน: รายการยืมอุปกรณ์ที่ถึงกำหนด / เกินกำหนดคืน");
            t.AppendLine(LINE);
            t.AppendLine();
            t.AppendLine("วันที่แจ้งเตือน : " + today);
            t.AppendLine("ส่งโดย         : " + empId);
            t.AppendLine("ประเภทการส่ง   : " + sendType);
            t.AppendLine("เรียน (To)     : " + string.Join("; ", to.ToArray()));
            t.AppendLine("สำเนา (CC)     : " + string.Join("; ", cc.ToArray()));
            t.AppendLine();
            t.AppendLine("สรุป : ทั้งหมด " + dt.Rows.Count + " รายการ"
                         + "  |  เกินกำหนด " + cntOverdue + " รายการ"
                         + "  |  ใกล้ครบกำหนด " + cntNear + " รายการ");
            t.AppendLine();
            t.AppendLine(LINE);
            t.AppendLine("  รายละเอียดการยืม");
            t.AppendLine(LINE);
            t.AppendLine();

            int no = 1;
            foreach (DataRow r in dt.Rows)
            {
                object v = r["return_duedate"];
                string dueTxt = "-";
                string flag = "";

                if (v != null && v != DBNull.Value)
                {
                    DateTime due = Convert.ToDateTime(v).Date;
                    int d = (due - DateTime.Today).Days;
                    dueTxt = due.ToString("dd/MM/yyyy");
                    if (d < 0) flag = "  *** เกินกำหนด " + Math.Abs(d) + " วัน ***";
                    else if (d <= 3) flag = "  (!) เหลืออีก " + d + " วัน";
                    else flag = "  (เหลืออีก " + d + " วัน)";
                }

                string borrowerId = Convert.ToString(r["issue_empid"]).Trim();
                HrmsUser u = GetHrmsUser(borrowerId);
                string nameTxt = (u != null && !string.IsNullOrEmpty(u.Name))
                               ? u.Name + " (" + borrowerId + ")" : borrowerId;
                string deptTxt = (u != null && !string.IsNullOrEmpty(u.Dept))
                               ? u.Dept : Convert.ToString(r["dept_code"]);

                t.AppendLine("[" + no + "] Doc No. : " + Convert.ToString(r["Doc_No"]));
                t.AppendLine("    Document Name : " + Convert.ToString(r["document_name"]));
                t.AppendLine("    Status        : " + Convert.ToString(r["status_name"]));
                t.AppendLine("    ผู้ยืม         : " + nameTxt);
                t.AppendLine("    Department    : " + deptTxt);
                t.AppendLine("    Return Due    : " + dueTxt + flag);
                t.AppendLine(DASH);
                no++;
            }

            t.AppendLine();
            t.AppendLine("****************************************************************");
            t.AppendLine("This auto-email was sent from Tool & Equipment System.");
            t.AppendLine("Best regard.");
            t.AppendLine("--------------------- Original Message Ends --------------------");
            t.AppendLine("****************************************************************");

            return t.ToString();
        }

        // ══════════════════════════════
        //  LOG
        // ══════════════════════════════
        private void WriteLog(DataTable dt, string sendType, string mailTo, string result)
        {
            try
            {
                string sql =
                    "INSERT INTO AutoAlert_Log " +
                    "(send_date, Doc_No, document_id, mail_to, send_type, send_result, send_by) " +
                    "VALUES (GETDATE(), @doc, @docid, @to, @type, @result, @by)";

                using (SqlConnection cn = new SqlConnection(ConnStr))
                {
                    cn.Open();
                    foreach (DataRow r in dt.Rows)
                    {
                        using (SqlCommand cmd = new SqlCommand(sql, cn))
                        {
                            cmd.Parameters.AddWithValue("@doc", Convert.ToString(r["Doc_No"]));
                            cmd.Parameters.AddWithValue("@docid", Convert.ToString(r["document_id"]));
                            cmd.Parameters.AddWithValue("@to", mailTo.Length > 480 ? mailTo.Substring(0, 480) : mailTo);
                            cmd.Parameters.AddWithValue("@type", sendType);
                            cmd.Parameters.AddWithValue("@result", result.Length > 400 ? result.Substring(0, 400) : result);
                            cmd.Parameters.AddWithValue("@by", CurrentEmpId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Log Error] " + ex.Message);
            }
        }

        // ══════════════════════════════
        //  SETTING
        // ══════════════════════════════
        private void LoadSetting()
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT TOP 1 mail_to, mail_cc FROM AutoBot_Setting WHERE bot_key = @k", cn))
                {
                    cmd.Parameters.AddWithValue("@k", "AUTO_ALERT_AAD1");
                    cn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            txtMailTo.Text = Convert.ToString(dr["mail_to"]);
                            txtMailCc.Text = Convert.ToString(dr["mail_cc"]);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Setting Error] " + ex.Message);
            }
        }

        protected void btnSaveSetting_Click(object sender, EventArgs e)
        {
            try
            {
                string sql =
                    "UPDATE AutoBot_Setting SET mail_to = @to, mail_cc = @cc, " +
                    "       update_date = GETDATE(), update_by = @by " +
                    "WHERE  bot_key = @k; " +
                    "IF @@ROWCOUNT = 0 " +
                    "  INSERT INTO AutoBot_Setting (bot_key, mail_to, mail_cc, update_date, update_by) " +
                    "  VALUES (@k, @to, @cc, GETDATE(), @by);";

                using (SqlConnection cn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@k", "AUTO_ALERT_AAD1");
                    cmd.Parameters.AddWithValue("@to", txtMailTo.Text.Trim());
                    cmd.Parameters.AddWithValue("@cc", txtMailCc.Text.Trim());
                    cmd.Parameters.AddWithValue("@by", CurrentEmpId);
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
                ShowMessage("บันทึกการตั้งค่าเรียบร้อย", true);
            }
            catch (Exception ex)
            {
                ShowMessage("บันทึกไม่สำเร็จ: " + Server.HtmlEncode(ex.Message), false);
            }
        }

        // ══════════════════════════════
        //  UI HELPER
        // ══════════════════════════════
        private void ShowMessage(string msg, bool success)
        {
            pnlMsg.Visible = true;
            divMsg.Attributes["class"] = success ? "bot-msg msg-ok" : "bot-msg msg-err";
            litMsg.Text = (success ? "&#9989; " : "&#10060; ") + msg;
        }

        private void HideMessage()
        {
            pnlMsg.Visible = false;
            litMsg.Text = "";
        }
    }
}
