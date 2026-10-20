using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Tool_Equipment_Control_System
{
    public partial class MenuList : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        SqlConnection conn_jig = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        datacon dc = new datacon();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["emp_id"] == null || Session["emp_id"].ToString() == "")
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                Check_Dept_Login();
                Check_Admin();
                Check_ApproveMenu();
                Set_RequestStatusCount();
                Set_ApproveCount();
                Set_StatusCard();
            }
        }
        protected void Check_Dept_Login()
        {
            string emp_id = Session["emp_id"].ToString();
            //string str = "";
            string str = "SELECT * FROM View_dept_master where dept_code='" + emp_id + "'";

            dc.QryDT(conn, str);

            if (dc.dt.Rows.Count > 0)
            {
                //img_new.Enabled = false;
                //img_new.classList.add("disabled-picture");
            }
        }
        protected void Check_Admin()
        {
            // 1.1 เมนูกำลังพัฒนา : แสดงทั้ง User และ Admin
            menuChangeRequest.Visible = true;   // แจ้งเปลี่ยน
            menuTanaoroshi.Visible = true;      // Tanaoroshi

            if (Session["admin"] == null) return;

            string Admin = Session["admin"].ToString();

            if (Admin == "99")
            {
                // 1.2 เมนูเฉพาะ Admin
                menuStorage.Visible = true;     // จัดเก็บอุปกรณ์ (เปิดใช้งานแล้ว แต่ยังให้เฉพาะ Admin)
                menuDashboard.Visible = true;   // Dashboard
                menuEquipment.Visible = true;   // Equipment
                menuEquipment.Visible = true;
                pnlMenu.Visible = true;

                //Set_StatusCard();
                Set_AdminMenuByLevel();
            }
        }
        protected void Set_AdminMenuByLevel()
        {
            string emp_id = Session["emp_id"].ToString();

            string str = "SELECT level_1, level_2, level_3, level_4 " +
                         "FROM Equipment_Admin " +
                         "WHERE emp_id = @emp_id AND status_use = 1";

            bool lv1 = false, lv2 = false, lv3 = false, lv4 = false;

            using (SqlCommand cmd = new SqlCommand(str, conn))
            {
                cmd.Parameters.AddWithValue("@emp_id", emp_id);
                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        lv1 = dr["level_1"] != DBNull.Value && Convert.ToBoolean(dr["level_1"]);
                        lv2 = dr["level_2"] != DBNull.Value && Convert.ToBoolean(dr["level_2"]);
                        lv3 = dr["level_3"] != DBNull.Value && Convert.ToBoolean(dr["level_3"]);
                        lv4 = dr["level_4"] != DBNull.Value && Convert.ToBoolean(dr["level_4"]);
                    }
                }
                conn.Close();
            }

            // แสดง Section Admin & Configuration
            pnlMenu.Visible = true;

            if (lv4)
            {
                // level_4 = True : คงสิทธิ์มองเห็นปกติ (เห็นทุกเมนู)
                return;
            }

            // ไม่ใช่ level_4 : เปิดเฉพาะเมนูตามสิทธิ์ ที่เหลือซ่อน
            SettingTana.Visible = lv2;   // Setting Tana
            EditLocation.Visible = lv1;   // Edit Location
            Authorization.Visible = lv3;   // Authorize

            NewEquipment.Visible = false;
            TranferEquipment.Visible = false;
            ScrapEquipment.Visible = false;
            Setting.Visible = false;
            AutoBot.Visible = false;
        }
        protected void Check_ApproveMenu()
        {
            string emp_id = Session["emp_id"].ToString();
            string str = "Select * From Authorize_list Where emp_id = '" + emp_id + "'";
            dc.QryDT(conn, str);
            DataTable dt = dc.dt;

            // ✅ เช็คว่าไม่มีข้อมูล
            if (dt == null || dt.Rows.Count == 0)
            {
                approveMenu.Visible = false;
            }
        }
        protected void Set_RequestStatusCount()
        {
            string emp_id = Session["emp_id"].ToString();

            // 1.3 ถ้า emp_id เป็น 4 หลัก -> ไม่แสดง
            if (emp_id.Length == 4)
            {
                badgeRequest.Visible = false;
                return;
            }

            string str = " SELECT COUNT(*) AS cnt FROM View_Equipment_Request_List " +
                         " WHERE issue_empid = @emp_id AND status_id NOT IN (99,100,999)";

            using (SqlCommand cmd = new SqlCommand(str, conn))
            {
                cmd.Parameters.AddWithValue("@emp_id", emp_id);
                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();

                if (count > 0)
                {
                    badgeRequest.InnerText = count > 99 ? "99+" : count.ToString();
                    badgeRequest.Visible = true;
                }
                else
                {
                    badgeRequest.Visible = false;
                }
            }
        }
        protected void Set_ApproveCount()
        {
            string emp_id = Session["emp_id"].ToString();

            // Login ด้วย Dept Code (4 หลัก) → หน้า Approve ต้องกรอก Emp ID ก่อน จึงไม่แสดง badge
            if (emp_id.Length == 4)
            {
                badgeApprove.Visible = false;
                return;
            }

            // ⚠️ เงื่อนไขต้องตรงกับ ApproveStatus.BuildApproveCondition()
            string str =
                " SELECT COUNT(*) FROM View_Equipment_Request_List V " +
                " WHERE " +
                // 1.2.1 Admin (Equipment_Admin.level_1 = 1)
                "  ( EXISTS (SELECT 1 FROM Equipment_Admin E " +
                "            WHERE LTRIM(RTRIM(E.emp_id)) = @emp_id AND E.level_1 = 1) " +
                "    AND (    (V.document_id = 'AAD1' AND V.status_id IN (50, 90)) " +
                "          OR (V.document_id = 'AAD2' AND V.status_id IN (50, 99)) " +
                "          OR (V.document_id = 'AAD3' AND V.status_id IN (50, 99)) " +
                "          OR (V.document_id = 'AAD4' AND V.status_id IN (10, 50, 99)) ) ) " +
                // 1.2.2 Approver AAD2 (status 30 = step 1, status 31 = step 2 / gauge : 28 = QC , 31 = Approve 2)
                "  OR ( V.document_id = 'AAD2' " +
                "       AND (   ( V.status_id IN (30, 31) " +
                "                 AND NOT EXISTS (SELECT 1 FROM NotificationofChange_Control_List G " +
                "                                 WHERE G.Doc_No = V.Doc_No AND G.change_type = 'gauge') " +
                "                 AND EXISTS (SELECT 1 FROM View_NotiChange_ApproverCheck N " +
                "                             WHERE N.Doc_No = V.Doc_No AND N.emp_id = @emp_id " +
                "                               AND (   (V.status_id = 30 AND N.approve_step = 1) " +
                "                                    OR (V.status_id = 31 AND N.approve_step = 2) ) ) ) " +
                // แจ้งเปลี่ยนเครื่องมือวัด (gauge) : 28 = QC Check , 31 = Approve 2
                "            OR ( V.status_id IN (28, 31) " +
                "                 AND EXISTS (SELECT 1 FROM NotificationofChange_Control_List G " +
                "                             JOIN NotificationofChange_Approver A " +
                "                               ON A.Doc_No = G.Doc_No AND A.status_id = G.status_id " +
                "                             WHERE G.Doc_No = V.Doc_No AND G.change_type = 'gauge' " +
                "                               AND LTRIM(RTRIM(A.emp_id)) = @emp_id) ) ) ) ";

            using (SqlCommand cmd = new SqlCommand(str, conn))
            {
                cmd.Parameters.AddWithValue("@emp_id", emp_id);
                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();

                if (count > 0)
                {
                    badgeApprove.InnerText = count > 99 ? "99+" : count.ToString();
                    badgeApprove.Visible = true;
                }
                else
                {
                    badgeApprove.Visible = false;
                }
            }
        }

        protected void Set_StatusCard()
        {
            string emp_id = Session["emp_id"].ToString();
            bool isAdmin = Session["admin"] != null && Session["admin"].ToString() == "99";

            // 1) ทุกคน : AAD2 ที่ตนเองเป็น Approver (ตรงกับ ApproveStatus 1.2.2)
            int myApprove = 0;
            if (emp_id.Length != 4)   // Login ด้วย Dept Code → ข้าม
            {
                string strMy =
                    " SELECT COUNT(*) FROM View_Equipment_Request_List V " +
                    " WHERE V.document_id = 'AAD2' " +
                "       AND (   ( V.status_id IN (30, 31) " +
                "                 AND NOT EXISTS (SELECT 1 FROM NotificationofChange_Control_List G " +
                "                                 WHERE G.Doc_No = V.Doc_No AND G.change_type = 'gauge') " +
                "                 AND EXISTS (SELECT 1 FROM View_NotiChange_ApproverCheck N " +
                "                             WHERE N.Doc_No = V.Doc_No AND N.emp_id = @emp_id " +
                "                               AND (   (V.status_id = 30 AND N.approve_step = 1) " +
                "                                    OR (V.status_id = 31 AND N.approve_step = 2) ) ) ) " +
                // แจ้งเปลี่ยนเครื่องมือวัด (gauge) : 28 = QC Check , 31 = Approve 2
                "            OR ( V.status_id IN (28, 31) " +
                "                 AND EXISTS (SELECT 1 FROM NotificationofChange_Control_List G " +
                "                             JOIN NotificationofChange_Approver A " +
                "                               ON A.Doc_No = G.Doc_No AND A.status_id = G.status_id " +
                "                             WHERE G.Doc_No = V.Doc_No AND G.change_type = 'gauge' " +
                "                               AND LTRIM(RTRIM(A.emp_id)) = @emp_id) ) ) ";

                using (SqlCommand cmd = new SqlCommand(strMy, conn))
                {
                    cmd.Parameters.AddWithValue("@emp_id", emp_id);
                    conn.Open();
                    myApprove = Convert.ToInt32(cmd.ExecuteScalar());
                    conn.Close();
                }
            }

            // 2) Admin : รายการเดิม 50 / 90 / 99
            var counts = new Dictionary<int, int> { { 50, 0 }, { 90, 0 }, { 99, 0 } };
            if (isAdmin)
            {
                string str = " SELECT status_id, COUNT(*) AS cnt " +
                             " FROM View_Equipment_Request_List " +
                             " WHERE status_id IN (50, 90, 99) " +
                             " GROUP BY status_id; ";

                using (SqlCommand cmd = new SqlCommand(str, conn))
                {
                    conn.Open();
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            int st = Convert.ToInt32(rd["status_id"]);
                            if (counts.ContainsKey(st))
                                counts[st] = Convert.ToInt32(rd["cnt"]);
                        }
                    }
                    conn.Close();
                }
            }

            // 3) แสดงเฉพาะการ์ดที่ > 0
            SetCard(cardMyApprove, valMyApprove, myApprove);
            SetCard(card50, valWaitApprove, counts[50]);
            SetCard(card90, valBorrowing, counts[90]);
            SetCard(card99, valWaitUpdate, counts[99]);

            // ⚠️ ห้ามใช้ card.Visible ตัดสิน (getter คืน false ถ้า parent ยังซ่อนอยู่)
            pnlStatusCard.Visible = myApprove > 0 || counts[50] > 0
                                 || counts[90] > 0 || counts[99] > 0;
        }
        private void SetCard(HtmlAnchor card, HtmlGenericControl val, int count)
        {
            val.InnerText = count.ToString();
            card.Visible = count > 0;
        }
    }
}
