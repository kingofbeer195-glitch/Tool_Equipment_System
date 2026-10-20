using ClosedXML.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tool_Equipment_Control_System
{
    public partial class UpdateEquipmentData : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tool_equipment"].ConnectionString);
        SqlConnection conn_jig = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString);
        datacon dc = new datacon();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // ===== ViewState key สำหรับเก็บ DataTable =====
        private const string VS_TABLE = "ImportedTable";

        // ===== DataTable ที่ Import =====
        private DataTable ImportedTable
        {
            get { return ViewState[VS_TABLE] as DataTable; }
            set { ViewState[VS_TABLE] = value; }
        }

        // ----------------------------------------------------------------
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                pnlGrid.Visible = false;
                //BindGridView(ImportedTable);
                //ViewState["ImportedTable"] = ImportedTable; // Keep
                ViewState["MODEL_MASTER"] = GetModelMaster();
                ViewState["DEPT_MASTER"] = GetDeptMaster();
                ViewState["LOCATION_MASTER"] = GetLocationMaster();
                ViewState["BOI_MASTER"] = GetBoiMaster();
                ViewState["STATUS_MASTER"] = GetStatusMaster();
                ViewState["PERIOD_MASTER"] = GetPeriodMaster();
            }
        }

        // Upload & Preview
        protected void btnUpload_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "";
            lblSaveResult.Text = "";

            if (!fileUpload.HasFile)
            {
                ShowAlert(lblStatus, "กรุณาเลือกไฟล์ก่อน", "warning");
                return;
            }

            string ext = Path.GetExtension(fileUpload.FileName).ToLower();
            if (ext != ".xlsx" && ext != ".xls" && ext != ".csv")
            {
                ShowAlert(lblStatus, "รองรับเฉพาะไฟล์ .xlsx, .xls, .csv เท่านั้น", "danger");
                return;
            }

            try
            {
                DataTable dt = null;

                if (ext == ".csv")
                    dt = ReadCsv(fileUpload.FileContent);
                else
                    dt = ReadExcel(fileUpload.FileContent);

                if (dt == null || dt.Rows.Count == 0)
                {
                    ShowAlert(lblStatus, "ไม่พบข้อมูลในไฟล์ หรือไฟล์ว่างเปล่า", "warning");
                    return;
                }

                ImportedTable = dt;
                BindGridView(ImportedTable);

                lblFileName.Text = "ไฟล์: " + fileUpload.FileName;
                lblRowCount.Text = dt.Rows.Count.ToString("N0");
                pnlGrid.Visible = true;

                ShowAlert(lblStatus, "โหลดข้อมูลสำเร็จ " + dt.Rows.Count.ToString("N0") + " รายการ", "success");
            }
            catch (Exception ex)
            {
                ShowAlert(lblStatus, "เกิดข้อผิดพลาด: " + ex.Message, "danger");
            }
        }

        // Helper: Read Excel (.xlsx / .xls)
        private DataTable ReadExcel(Stream stream)
        {
            DataTable dt = new DataTable();
            DataTable sqlData_T = new DataTable();
            DataTable sqlData_M = new DataTable();
            //DataTable sqlData = new DataTable();

            using (XLWorkbook wb = new XLWorkbook(stream))
            {
                IXLWorksheet ws = wb.Worksheet(1); // ClosedXML index เริ่มที่ 1

                if (ws.RowsUsed().Count() == 0)
                    return dt;

                // ===== Header (Row 1) =====
                IXLRow headerRow = ws.FirstRowUsed();
                int totalCols = headerRow.LastCellUsed().Address.ColumnNumber;

                for (int c = 1; c <= totalCols; c++)
                {
                    string colName = headerRow.Cell(c).GetValue<string>().Trim();
                    if (string.IsNullOrEmpty(colName))
                        colName = "Column" + c;

                    dt.Columns.Add(EnsureUnique(dt, colName));
                }

                // ===== Data (Row 2+) =====
                foreach (IXLRow row in ws.RowsUsed().Skip(1))
                {
                    DataRow dr = dt.NewRow();
                    bool hasData = false;

                    for (int c = 1; c <= totalCols; c++)
                    {
                        //string val = row.Cell(c).GetValue<string>().Trim();

                        string colName = dt.Columns[c - 1].ColumnName;

                        string val = row.Cell(c).GetValue<string>().Trim();

                        // ✅ Fix เฉพาะ REMARK
                        if (colName.Equals("REMARK", StringComparison.OrdinalIgnoreCase))
                        {
                            val = FixThaiEncoding(val);
                        }
                        dr[c - 1] = val;

                        if (!string.IsNullOrEmpty(val))
                            hasData = true;
                    }

                    if (hasData)
                        dt.Rows.Add(dr);
                }
            }

            // --- Query Tool & Equipment Data
            sqlData_T = GetSqlData_T();
            sqlData_M = GetSqlData_M();
            DataTable sqlData = sqlData_T.Copy(); // clone โครง + ข้อมูล T
            sqlData.Merge(sqlData_M, true, MissingSchemaAction.Ignore);


            // Add New Column
            #region
            dt.Columns.Add("MATCH_STATUS");     //Result Search Fix Asset or Control No
            dt.Columns.Add("CHK_STATUS" , typeof(bool));       //Check Box Status 
            dt.Columns.Add("UPDATE_STATUS");    //Result Update
            dt.Columns.Add("UPDATE_ERROR");     //Update Error Detail

            dt.Columns.Add("SQL_ToolId");
            dt.Columns.Add("SQL_SubId");
            dt.Columns.Add("SQL_System");
            dt.Columns.Add("SQL_ToolNo");

            dt.Columns.Add("SQL_DeptCode");
            dt.Columns.Add("SQL_DeptID");
            dt.Columns.Add("SQL_DivID");
            dt.Columns.Add("SQL_DeptCode_ID");
            dt.Columns.Add("EDIT_Dept");

            dt.Columns.Add("SQL_FixAsset");
            dt.Columns.Add("EDIT_SUBFIX");
            dt.Columns.Add("SQL_ControlNo");
            dt.Columns.Add("EDIT_CONTROLNO");

            dt.Columns.Add("SQL_MODEL");
            dt.Columns.Add("SQL_MODEL_ID");
            dt.Columns.Add("EDIT_MODEL"); 

            dt.Columns.Add("SQL_Location");
            dt.Columns.Add("SQL_Location_ID");
            dt.Columns.Add("EDIT_LOCATION");

            dt.Columns.Add("SQL_Cell");
            dt.Columns.Add("SQL_Cell_ID");
            dt.Columns.Add("EDIT_CELL");

            dt.Columns.Add("SQL_Station");
            dt.Columns.Add("SQL_Station_ID");
            dt.Columns.Add("EDIT_STATION");

            dt.Columns.Add("SQL_Responsible");
            dt.Columns.Add("EDIT_Responsible");

            dt.Columns.Add("SQL_BOI");
            dt.Columns.Add("SQL_BOI_ID");
            dt.Columns.Add("EDIT_BOI");
           
            dt.Columns.Add("SQL_PAPER");
            dt.Columns.Add("SQL_PAPER_ID");
            dt.Columns.Add("EDIT_PAPER");

            dt.Columns.Add("SQL_STATUS");
            dt.Columns.Add("SQL_STATUS_ID");
            dt.Columns.Add("EDIT_STATUS", typeof(string));

            dt.Columns.Add("EDIT_OPERATION_DATE");
            dt.Columns.Add("EDIT_DUE_DATE");
            dt.Columns.Add("EDIT_REMARK");
            dt.Columns.Add("EDIT_PERIOD");

            #endregion

            MergeExcelWithSql(dt,sqlData);

            return dt;

        }

        // Helper: Bind SQL Data — ดึงข้อมูลจาก Database Jig Tool
        private DataTable GetSqlData_T()
        {
            DataTable sqlData = new DataTable();

            //Query Data List Tool on SQL
            #region
            string str = " SELECT A.tool_id , A.toolSub_id" +
                         " , A.dept_code as dept_id , A.division as div_id , B.dept_code " +
                         " , A.FixAcces , A.control_no , assetName " +
                         " , C.tool_no , A.serialNo as serial_no , A.Manufacturing" +
                         " , A.cost , A.po , A.start_use" +
                         " , A.model_id , D.model_name" +
                         " , A.location1_id, E.location1_name" +
                         " , A.location2_id, F.location2_name" +
                         " , A.location3_id, G.location3_name" +
                         " , A.rsp_empID , H.pic_dept + ' (' + B.dept_name + ')' AS Responsible" +
                         " , A.duedate" +
                         " , A.boi , Case When A.boi is NULL or A.boi = '' Then '-' Else I.boi_name End As Boi_Name" +
                         " , A.status_id , J.status_name" +
                         " , A.paper , Case When A.paper = 1 Then 'Paper' Else '-' End AS Paper_text" +
                         " , A.Remark , '1' as Sys_Status" +
                         " FROM jig_tool_system.dbo.toolSub_list A" +
                         " left join View_dept_master B on A.dept_code = B.dept_id And A.division = B.division" +
                         " left join jig_tool_system.dbo.tool_master C on A.tool_id = C.tool_id" +
                         " left join jig_tool_system.dbo.draw_master_model D on A.model_id = D.model_id" +
                         " left join jig_tool_system.dbo.master_location1 E on A.location1_id = E.location1_id" +
                         " left join jig_tool_system.dbo.master_location2 F on A.location1_id = F.location1_id And A.location2_id = F.location2_id" +
                         " left join jig_tool_system.dbo.master_location3 G on A.location1_id = G.location1_id And A.location3_id = G.location3_id" +
                         " left join Responsible_Dept H on A.rsp_empID = H.emp_id " +
                         " left join Master_BOI I on A.boi = I.boi_id " +
                         " left join tool_equipment_status J on A.status_id = J.status_id" +
                         " group by A.tool_id , A.toolSub_id , A.dept_code , A.division , B.dept_code" +
                         " , A.FixAcces , A.control_no , assetName , C.tool_no , A.serialNo , A.Manufacturing" +
                         " , A.cost , A.po , A.start_use , A.model_id , D.model_name , A.location1_id, E.location1_name" +
                         " , A.location2_id, F.location2_name , A.location3_id , G.location3_name , A.rsp_empID , H.pic_dept ,B.dept_name" +
                         " , A.duedate , A.boi , I.boi_name , A.status_id , J.status_name , A.paper , A.Remark ";
            #endregion

            dc.QryDT(conn, str);
            sqlData = dc.dt;

            return sqlData;
        }

        // Helper: Bind SQL Data — ดึงข้อมูลจาก Database Equipment
        private DataTable GetSqlData_M()
        {
            DataTable sqlData_M = new DataTable();

            //Query Data List Tool on SQL
            #region
            string str = "select A.Equipment_id as tool_id , A.Equipment_sub_id as toolSub_id" +
                         " , A.dept_code as dept_id, A.division as div_id, B.dept_code" +
                         " , A.fix_asset as FixAcces ,A.control_no, A.assetName , C.equipment_no as tool_no" +
                         " , A.serial_no , A.manufacturing as Manufacturing , A.price as cost , A.po , A.operation_date  as start_use" +
                         " , A.model_id , D.model_name" +
                         " , A.location_id as location1_id, E.location1_name" +
                         " , A.cell_id as location2_id , F.location2_name" +
                         " , A.station_id as location3_id, G.location3_name" +
                         " , A.rsp_empID , H.pic_dept + ' (' + B.dept_name + ')' AS Responsible" +
                         " , A.duedate, A.boi, Case When A.boi is NULL or A.boi = '' Then '-' Else I.boi_name End As Boi_Name" +
                         " , A.status_use as status_id,J.status_name" +
                         " , A.paper,Case When A.paper = 1 Then 'Paper' Else '-' End AS Paper_text" +
                         " , A.Remark , '2' as Sys_Status" +
                         " from Equipment_sub_id A" +
                         " left join View_dept_master B on A.dept_code = B.dept_id And A.division = B.division" +
                         " left join Equipment_master_list C on A.Equipment_id = C.Equipment_id" +
                         " left join jig_tool_system.dbo.draw_master_model D on A.model_id = D.model_id" +
                         " left join jig_tool_system.dbo.master_location1 E on A.location_id = E.location1_id" +
                         " left join jig_tool_system.dbo.master_location2 F on A.location_id = F.location1_id And A.cell_id = F.location2_id" +
                         " left join jig_tool_system.dbo.master_location3 G on A.location_id = G.location1_id And A.station_id = G.location3_id" +
                         " left join Responsible_Dept H on A.rsp_empID = H.emp_id" +
                         " left join Master_BOI I on A.boi = I.boi_id" +
                         " left join tool_equipment_status J on A.status_use = J.status_id" +
                         " group by A.Equipment_id, A.Equipment_sub_id" +
                         " , A.dept_code , A.division , B.dept_code" +
                         " , A.fix_asset,A.control_no, A.assetName , C.equipment_no" +
                         " , A.serial_no , A.manufacturing , A.price , A.po , A.operation_date" +
                         " , A.model_id , D.model_name" +
                         " , A.location_id, E.location1_name" +
                         " , A.cell_id, F.location2_name" +
                         " , A.station_id, G.location3_name" +
                         " , A.rsp_empID , H.pic_dept ,B.dept_name" +
                         " , A.duedate, A.boi, I.boi_name" +
                         " , A.status_use , J.status_name" +
                         " , A.paper , A.Remark";
            #endregion

            dc.QryDT(conn, str);
            sqlData_M = dc.dt;

            return sqlData_M;
        }

        // Helper: Join Excel ↔ SQL
        private void MergeExcelWithSql(DataTable dtExcel, DataTable dtSql)
        {
            // --- Lookup Key : Fix Asset
            var fixAssetLookup = dtSql.AsEnumerable()
                .Where(r =>
                    r["FixAcces"] != DBNull.Value &&
                    !string.IsNullOrWhiteSpace(r["FixAcces"].ToString()) &&
                    r["FixAcces"].ToString().Trim() != "-"
                )
                .ToLookup(r => r["FixAcces"].ToString().Trim());

            // --- Lookup Key : Control No
            var controlNoLookup = dtSql.AsEnumerable()
                .Where(r =>
                    r["control_no"] != DBNull.Value &&
                    !string.IsNullOrWhiteSpace(r["control_no"].ToString())
                )
                .ToLookup(r => r["control_no"].ToString().Trim());

            // --- Lookup Key : Tool No / Equipment No
            //var toolnoLookup = dtSql.AsEnumerable()
            //    .Where(r =>
            //        r["MODEL"] != DBNull.Value &&
            //        !string.IsNullOrWhiteSpace(r["MODEL"].ToString())
            //    )
            //    .ToLookup(r => r["MODEL"].ToString().Trim());

            foreach (DataRow row in dtExcel.Rows)
            {
                string fixAsset = row["ASSET-NO"].ToString().Trim();
                //string controlNo = row["CONTROL NO"].ToString().Trim();
                string controlNo = GetControlNoFromRow(row);
                //string toolNo = row["CONTROL NO"].ToString().Trim();

                // ===== 1) ใช้ FixAcces ก่อน =====
                if (!string.IsNullOrWhiteSpace(fixAsset) && fixAsset != "-")
                {
                    if (fixAssetLookup.Contains(fixAsset))
                    {
                        var matches = fixAssetLookup[fixAsset];

                        if (matches.Count() == 1)
                        {
                            DataRow sqlRow = matches.First();
                            MapSqlToExcelRow(row, sqlRow);
                            row["MATCH_STATUS"] = "FOUND_BY_FIXASSET";
                        }
                        else
                        {
                            row["MATCH_STATUS"] = "DUPLICATE_FIXASSET_IN_SQL";
                        }
                        continue;
                    }
                }

                // ===== 2) Fallback ใช้ Control No =====
                if (!string.IsNullOrWhiteSpace(controlNo) && controlNo != "-")
                {
                    if (controlNoLookup.Contains(controlNo))
                    {
                        var matches = controlNoLookup[controlNo];

                        if (matches.Count() == 1)
                        {
                            DataRow sqlRow = matches.First();
                            MapSqlToExcelRow(row, sqlRow);
                            row["MATCH_STATUS"] = "FOUND_BY_CONTROLNO";
                        }
                        else
                        {
                            row["MATCH_STATUS"] = "DUPLICATE_CONTROLNO_IN_SQL";
                        }
                        continue;
                    }
                }

                // ===== 3) ไม่พบ =====
                row["MATCH_STATUS"] = "NOT_FOUND";
            }
        }

        // Helper: MapSqlToExcelRow
        private void MapSqlToExcelRow(DataRow excelRow, DataRow sqlRow)
        {
            //Update Data on  excelRow
            #region
            excelRow["SQL_ToolId"] = sqlRow["tool_id"];
            excelRow["SQL_SubId"] = sqlRow["toolSub_id"];
            excelRow["SQL_System"] = sqlRow["Sys_Status"];
            excelRow["SQL_ToolNo"] = sqlRow["tool_no"];

            excelRow["SQL_DeptCode"] = sqlRow["dept_code"];
            excelRow["SQL_DeptID"] = sqlRow["dept_id"];
            excelRow["SQL_DivID"] = sqlRow["div_id"];
            excelRow["SQL_DeptCode_ID"] = sqlRow["dept_id"] + "_" + sqlRow["div_id"];

            excelRow["SQL_FixAsset"] = sqlRow["FixAcces"];
            excelRow["SQL_ControlNo"] = sqlRow["control_no"];

            excelRow["SQL_MODEL"] = sqlRow["model_name"];
            excelRow["SQL_MODEL_ID"] = sqlRow["model_id"];            
            //excelRow["EDIT_MODEL"] = sqlRow["model_name"]; // ค่าเริ่มต้น

            excelRow["SQL_Location"] = sqlRow["location1_name"];
            excelRow["SQL_Location_ID"] = sqlRow["location1_id"];

            excelRow["SQL_Cell"] = sqlRow["location2_name"];
            excelRow["SQL_Cell_ID"] = sqlRow["location2_id"];

            excelRow["SQL_Station"] = sqlRow["location3_name"];
            excelRow["SQL_Station_ID"] = sqlRow["location3_id"];

            excelRow["SQL_Responsible"] = sqlRow["Responsible"];

            excelRow["SQL_BOI"] = sqlRow["Boi_Name"];
            excelRow["SQL_BOI_ID"] = sqlRow["boi"];

            excelRow["SQL_PAPER"] = sqlRow["Paper_text"];
            excelRow["SQL_PAPER_ID"] = sqlRow["paper"];

            excelRow["SQL_STATUS"] = sqlRow["status_name"];
            excelRow["SQL_STATUS_ID"] = sqlRow["status_id"];

            #endregion

        }

        // Helper: BindGridView — สร้าง column อัตโนมัติจาก DataTable
        private void BindGridView(DataTable dt)
        {
            gvData.DataSource = dt;
            gvData.DataBind();

            // ✅ สำคัญที่สุด เพื่อให้ sticky header ทำงาน
            if (gvData.HeaderRow != null)
                gvData.HeaderRow.TableSection = TableRowSection.TableHeader;

        }      

        // GridView RowDataBound
        protected void gvData_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //Header
            if (e.Row.RowType == DataControlRowType.Header)
            {
                foreach (TableCell cell in e.Row.Cells)
                {
                    string text = cell.Text;

                    if (text.StartsWith("SQL_"))
                    {
                        cell.BackColor = System.Drawing.Color.DarkGreen;
                        cell.ForeColor = System.Drawing.Color.White;
                    }
                    else if (text == "MATCH_STATUS")
                    {
                        cell.BackColor = System.Drawing.Color.DarkOrange;
                        cell.ForeColor = System.Drawing.Color.White;
                    }
                }
            }

            //Row
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                // ✅ ให้แต่ละแถวมี id ตาม RowIndex
                e.Row.Attributes["id"] = "row_" + e.Row.RowIndex;

                string status = DataBinder.Eval(e.Row.DataItem, "MATCH_STATUS").ToString();

                if (status == "FOUND_BY_FIXASSET")
                    e.Row.BackColor = Color.Honeydew;
                else if (status == "FOUND_BY_CONTROLNO")
                    e.Row.BackColor = Color.LightCyan;
                else if (status.Contains("DUPLICATE"))
                    e.Row.BackColor = Color.Khaki;
                else if (status == "NOT_FOUND")
                    e.Row.BackColor = Color.MistyRose;

                // ===== Selection ROW =====
                DataRowView row = (DataRowView)e.Row.DataItem;

                // ===== ดึงค่า Location สำหรับ Cell Status =====
                string sqlLocation = DataBinder.Eval(e.Row.DataItem, "SQL_Location_ID").ToString().Trim();

                // ===== Selection Object =====
                CheckBox chk = e.Row.FindControl("chkRow") as CheckBox;
                TextBox txtFixAsset = e.Row.FindControl("txtFixAsset") as TextBox;
                Label lblFixAsset = e.Row.FindControl("lblFixAsset") as Label;
                TextBox txtSubfix = e.Row.FindControl("txtSubfix") as TextBox;
                Label lblSubfix = e.Row.FindControl("lblSubfix") as Label;
                TextBox txtControlNo = e.Row.FindControl("txtControlNo") as TextBox;
                Label lblControlNo = e.Row.FindControl("lblControlNo") as Label;
                TextBox txtToolNo = e.Row.FindControl("txtToolNo") as TextBox;
                Label lblToolNo = e.Row.FindControl("lblToolNo") as Label;
                DropDownList ddlStatus = e.Row.FindControl("ddlStatus") as DropDownList;
                Label lblStatus = e.Row.FindControl("lblStatus") as Label;
                DropDownList ddlDept = e.Row.FindControl("ddlDept") as DropDownList;
                Label lblDept = e.Row.FindControl("lblDept") as Label;
                DropDownList ddlModel = e.Row.FindControl("ddlModel") as DropDownList;
                Label lblModel = e.Row.FindControl("lblModel") as Label;
                DropDownList ddlLocation = e.Row.FindControl("ddlLocation") as DropDownList;
                Label lblLocation = e.Row.FindControl("lblLocation") as Label;
                DropDownList ddlCell = e.Row.FindControl("ddlCell") as DropDownList;
                Label lblCell = e.Row.FindControl("lblCell") as Label;
                DropDownList ddlStation = e.Row.FindControl("ddlStation") as DropDownList;
                Label lblStation = e.Row.FindControl("lblStation") as Label;
                DropDownList ddlResponsible = e.Row.FindControl("ddlResponsible") as DropDownList;
                Label lblResponsible = e.Row.FindControl("lblResponsible") as Label;
                DropDownList ddlBoi = e.Row.FindControl("ddlBoi") as DropDownList;
                Label lblBoi = e.Row.FindControl("lblBoi") as Label;
                DropDownList ddlPaper = e.Row.FindControl("ddlPaper") as DropDownList;
                Label lblPaper = e.Row.FindControl("lblPaper") as Label;
                DropDownList ddlPeriod = e.Row.FindControl("ddlPeriod") as DropDownList;
                Label lblPeriod = e.Row.FindControl("lblPeriod") as Label;
                Label lbloperationDate = e.Row.FindControl("lbl_operationDate") as Label;


                // Check Box
                if (chk != null)
                {
                    chk.Checked = row["CHK_STATUS"] != DBNull.Value &&
                                      Convert.ToBoolean(row["CHK_STATUS"]);
                }

                // --- Fix Asset
                #region

                // ✅ ดึงค่าจาก DataTable
                string assetNo = row["ASSET-NO"] == DBNull.Value
                    ? ""
                    : row["ASSET-NO"].ToString();

                lblFixAsset.Text = row["ASSET-NO"].ToString();

                if (txtFixAsset != null)
                {
                    string excelAsset = row["ASSET-NO"].ToString().Trim();     // จาก Excel
                    string sqlAsset = row["SQL_FixAsset"].ToString().Trim();   // จาก SQL

                    // ===== CASE : Blank =====
                    if (string.IsNullOrEmpty(excelAsset) && string.IsNullOrEmpty(sqlAsset))
                    {
                        SetStatus(txtFixAsset, "Blank", "#6c757d");
                        //return;
                    }
                    // ===== CASE : Match / Not Match =====
                    else if (excelAsset.Equals(sqlAsset, StringComparison.OrdinalIgnoreCase))
                    {
                        SetStatus(txtFixAsset, sqlAsset, "#28a745"); // เขียว
                    }
                    else
                    {
                        SetStatus(txtFixAsset, sqlAsset, "#dc3545"); // แดง
                    }
                }
                #endregion

                // --- Sub Fix
                lblSubfix.Text = row["INTERVAL"].ToString();
                if (txtSubfix != null)
                {
                    string subFix = ExtractSub(row["INTERVAL"]);

                    if (!string.IsNullOrEmpty(subFix))
                    {
                        txtSubfix.Text = subFix;
                        //SetRowStatus(txtSubfix, "MATCH_SQL");
                    }
                    else
                    {
                        txtSubfix.Text = "1";
                        //SetRowStatus(txtSubfix, "NOT_FOUND");
                    }
                }

                // --- Control No
                #region

                // ✅ ดึงค่าจาก DataTable
                string controlNo = row["CONTROL NO"] == DBNull.Value
                    ? ""
                    : row["CONTROL NO"].ToString();                

                if (txtControlNo != null)
                {
                    string excelControlNo = row["CONTROL NO"].ToString().Trim();     // จาก Excel
                    string sqlControlNo = row["SQL_ControlNo"].ToString().Trim();   // จาก SQL

                    if (string.IsNullOrEmpty(excelControlNo) || excelControlNo == "-")
                    {
                        lblControlNo.Text = ExtractControlNo(row["INTERVAL"].ToString(),row["REMARK"].ToString());
                    }
                    else
                    {
                        lblControlNo.Text = row["CONTROL NO"].ToString();
                    }

                    // ===== CASE : Blank =====
                    if (string.IsNullOrEmpty(excelControlNo) && string.IsNullOrEmpty(sqlControlNo))
                    {
                        SetStatus(txtControlNo, "Blank", "#6c757d");
                        //return;
                    }
                    // ===== CASE : Match / Not Match =====
                    else if (excelControlNo.Equals(sqlControlNo, StringComparison.OrdinalIgnoreCase))
                    {
                        SetStatus(txtControlNo, sqlControlNo, "#28a745"); // เขียว
                    }
                    else
                    {
                        SetStatus(txtControlNo, sqlControlNo, "#dc3545"); // แดง
                    }
                }

                #endregion

                // --- Tool / Equipment No
                #region

                // ✅ ดึงค่าจาก DataTable
                lblToolNo.Text = row["MODEL"].ToString();

                if (txtToolNo != null)
                {
                    string T_Model = row["MODEL"].ToString().Trim();     // จาก Excel
                    string sqlToolNo = row["SQL_ToolNo"].ToString().Trim();   // จาก SQL

                    // ===== CASE : Blank =====
                    if (string.IsNullOrEmpty(T_Model) && string.IsNullOrEmpty(sqlToolNo))
                    {
                        SetStatus(txtToolNo, "Blank", "#6c757d");
                        //return;
                    }
                    // ===== CASE : Match / Not Match =====
                    else if (T_Model.Equals(sqlToolNo, StringComparison.OrdinalIgnoreCase))
                    {
                        SetStatus(txtToolNo, sqlToolNo, "#28a745"); // เขียว
                    }
                    else
                    {
                        SetStatus(txtToolNo, sqlToolNo, "#dc3545"); // แดง
                    }
                }
                #endregion

                // --- EDIT Status
                #region

                // ✅ ดึงค่าจาก DataTable
                string Status = row["STATUS"] == DBNull.Value
                    ? ""
                    : row["STATUS"].ToString();

                lblStatus.Text = row["STATUS"].ToString();

                if (ddlStatus != null)
                {
                    DataTable StatusMaster = (DataTable)ViewState["STATUS_MASTER"];
                    ddlStatus.DataSource = StatusMaster;
                    ddlStatus.DataTextField = "status_name";
                    ddlStatus.DataValueField = "status_id";
                    ddlStatus.DataBind();
                    ddlStatus.Items.Insert(0, new ListItem("-- Select --", ""));

                    string editStatus = row["EDIT_STATUS"] == DBNull.Value
                            ? ""
                            : row["EDIT_STATUS"].ToString();

                    string sqlStatusId = row["SQL_STATUS_ID"] == DBNull.Value
                        ? ""
                        : row["SQL_STATUS_ID"].ToString();

                    // ===== ดึงค่าแต่ละฝั่ง =====
                    string statusName = DataBinder.Eval(e.Row.DataItem, "STATUS").ToString().Trim();
                    string sqlstatusId = DataBinder.Eval(e.Row.DataItem, "SQL_STATUS_ID").ToString().Trim();
                    string sqlstatus = DataBinder.Eval(e.Row.DataItem, "SQL_STATUS").ToString().Trim();

                    if (statusName == "WAIT CONCIDER")
                    {
                        statusName = "Wait Consider";
                    }
                    else if (statusName == "Scrap")
                    {
                        statusName = "Wait Scrap";
                    }

                    // ===== CASE 1: SQL_STATUS (ID) ถูกต้อง =====
                    if (!string.IsNullOrEmpty(sqlstatus) && string.Equals(statusName, sqlstatus, StringComparison.OrdinalIgnoreCase))
                    {
                        ddlStatus.SelectedValue = sqlstatusId;
                        SetRowStatus(ddlStatus, "MATCH_SQL");
                    }

                    // ===== CASE 2: หา status จาก STATUS =====
                    else if (!string.IsNullOrEmpty(statusName))
                    {
                        ListItem item = ddlStatus.Items
                            .Cast<ListItem>()
                            .FirstOrDefault(i => i.Text.Equals(statusName, StringComparison.OrdinalIgnoreCase));

                        if (item != null)
                        {
                            ddlStatus.SelectedValue = item.Value;
                            SetRowStatus(ddlStatus, "MATCH_FROM_EXCEL");
                            //return;
                        }
                    }

                    // ===== CASE 3: ไม่เจอ =====
                    else
                    {
                        ddlStatus.SelectedIndex = 0;
                        SetRowStatus(ddlStatus, "NOT_FOUND");
                    }
                }

                #endregion

                // --- EDIT DEPT
                #region

                // ✅ ดึงค่าจาก DataTable
                string Dept = row["DEPT CODE"] == DBNull.Value
                    ? ""
                    : row["DEPT CODE"].ToString();

                lblDept.Text = row["DEPT CODE"].ToString();

                if (ddlDept != null)
                {
                    DataTable deptMaster = (DataTable)ViewState["DEPT_MASTER"];
                    ddlDept.DataSource = deptMaster;
                    ddlDept.DataTextField = "dept_code";
                    ddlDept.DataValueField = "ID";
                    ddlDept.DataBind();

                    // เพิ่ม -- Select --
                    ddlDept.Items.Insert(0, new ListItem("-- Select --", ""));

                    // ===== ดึงค่าแต่ละฝั่ง =====
                    string dept = DataBinder.Eval(e.Row.DataItem, "DEPT CODE").ToString().Trim();
                    //string sqldeptId = DataBinder.Eval(e.Row.DataItem, "SQL_DeptCode_ID").ToString().Trim();
                    string sqldept = DataBinder.Eval(e.Row.DataItem, "SQL_DeptCode").ToString().Trim();

                    // ===== CASE 1: SQL_DEPT (ID) ถูกต้อง =====
                    if (!string.IsNullOrEmpty(sqldept) && (dept == sqldept))
                    {
                        ddlDept.SelectedValue = sqldept;
                        SetRowStatus(ddlDept, "MATCH_SQL");
                    }

                    // ===== CASE 2: หา dept จาก Dept Code =====
                    else if (!string.IsNullOrEmpty(dept))
                    {
                        ListItem item = ddlDept.Items
                            .Cast<ListItem>()
                            .FirstOrDefault(i => i.Text.Equals(dept, StringComparison.OrdinalIgnoreCase));

                        if (item != null)
                        {
                            ddlDept.SelectedValue = item.Value;
                            SetRowStatus(ddlDept, "MATCH_FROM_EXCEL");
                            //return;
                        }
                    }

                    // ===== CASE 3: ไม่เจอ =====
                    else
                    {
                        ddlDept.SelectedIndex = 0;
                        SetRowStatus(ddlDept, "NOT_FOUND");
                    }
                }

                #endregion

                // --- EDIT MODEL
                #region

                // ✅ ดึงค่าจาก DataTable
                string Model = row["CPT MODEL"] == DBNull.Value
                    ? ""
                    : row["CPT MODEL"].ToString();

                lblModel.Text = row["CPT MODEL"].ToString();

                if (ddlModel != null)
                {
                    DataTable modelMaster = (DataTable)ViewState["MODEL_MASTER"];
                    ddlModel.DataSource = modelMaster;
                    ddlModel.DataTextField = "model_name";
                    ddlModel.DataValueField = "model_id";
                    ddlModel.DataBind();

                    // เพิ่ม -- Select --
                    ddlModel.Items.Insert(0, new ListItem("-- Select --", ""));

                    // ===== ดึงค่าแต่ละฝั่ง =====
                    string cptModel = DataBinder.Eval(e.Row.DataItem, "CPT MODEL").ToString().Trim();

                    if (cptModel == "T713E-T713" || cptModel == "T713-T513")
                        cptModel = cptModel.Replace("-", "/");

                    if (cptModel != "00K-ACC" && cptModel != "T-NEO")
                        cptModel = cptModel.Replace("-", "");

                    string sqlModelName = DataBinder.Eval(e.Row.DataItem, "SQL_MODEL").ToString().Trim();
                    string sqlModelId = DataBinder.Eval(e.Row.DataItem, "SQL_MODEL_ID").ToString().Trim();

                    // ===== CASE 1: SQL_MODEL (model_id) ถูกต้อง =====
                    if (!string.IsNullOrEmpty(sqlModelId) && cptModel == sqlModelName)
                    {
                        ddlModel.SelectedValue = sqlModelId;
                        SetRowStatus(ddlModel, "MATCH_SQL");
                    }

                    // ===== CASE 2: หา model_id จาก CPT MODEL (model_name) =====
                    else if (!string.IsNullOrEmpty(cptModel))
                    {
                        ListItem item = ddlModel.Items
                            .Cast<ListItem>()
                            .FirstOrDefault(i => i.Text.Equals(cptModel, StringComparison.OrdinalIgnoreCase));

                        if (item != null)
                        {
                            ddlModel.SelectedValue = item.Value;
                            SetRowStatus(ddlModel, "MATCH_FROM_EXCEL");
                        }
                    }

                    // ===== CASE 3: ไม่เจอ =====
                    else
                    {
                        // ✅ --- Fuzzy Compare ---
                        string inputNorm = Normalize(cptModel);

                        double bestScore = 0;
                        string bestValue = "";

                        foreach (DataRow r in modelMaster.Rows)
                        {
                            string dbName = r["model_name"].ToString();
                            string dbNorm = Normalize(dbName);

                            double score = Similarity(inputNorm, dbNorm);

                            if (score > bestScore)
                            {
                                bestScore = score;
                                bestValue = r["model_id"].ToString();
                            }
                        }

                        // ✅ ถ้าใกล้เคียง >= 80%
                        if (bestScore >= 0.8)
                        {
                            ddlModel.SelectedValue = bestValue;

                            // ✅ custom status
                            SetRowStatus(ddlModel, "SIMILAR_80");

                            // debug (optional)
                            // lblStation.Text += " (~" + (bestScore * 100).ToString("F0") + "%)";
                        }
                        else
                        {
                            ddlModel.SelectedIndex = 0;
                            SetRowStatus(ddlModel, "NOT_FOUND");
                        }
                        //ddlModel.SelectedIndex = 0;
                        //SetRowStatus(ddlModel, "NOT_FOUND");
                    }
                }
                #endregion

                // --- EDIT Location
                if (ddlLocation != null)
                {
                    string deptCode = DataBinder.Eval(e.Row.DataItem, "DEPT CODE").ToString().Trim();
                    string location_spare = "99";
                    string location_inline = "97";

                    DataTable locationMaster = (DataTable)ViewState["LOCATION_MASTER"];
                    ddlLocation.DataSource = locationMaster;
                    ddlLocation.DataTextField = "location1_name";
                    ddlLocation.DataValueField = "location1_id";
                    ddlLocation.DataBind();

                    // เพิ่ม -- Select --
                    ddlLocation.Items.Insert(0, new ListItem("-- Select --", ""));

                    // ===== CASE 1: Dept Code = 5505 (AAD) -> Spare (99) =====
                    if (!string.IsNullOrEmpty(deptCode) && deptCode == "5505")
                    {
                        ddlLocation.SelectedValue = location_spare;
                        SetRowStatus(ddlLocation, "MATCH_FROM_EXCEL");
                    }
                    // ===== CASE 2: ไม่ใช่ 5505 -> Inline (97) =====
                    else if (!string.IsNullOrEmpty(deptCode) && deptCode != "5505")
                    {
                        ddlLocation.SelectedValue = location_inline;
                        SetRowStatus(ddlLocation, "MATCH_FROM_EXCEL");
                    }
                    // ===== CASE 3: Location ID (SQL) มีค่า =====
                    else if (!string.IsNullOrEmpty(sqlLocation))
                    {
                        ddlLocation.SelectedValue = sqlLocation;
                        SetRowStatus(ddlLocation, "MATCH_SQL");
                    }
                    // ===== CASE 4: ไม่เจอ =====
                    else
                    {
                        ddlLocation.SelectedIndex = 0;
                        SetRowStatus(ddlLocation, "NOT_FOUND");
                    }
                }

                // --- EDIT CELL
                #region

                // ✅ ดึงค่าจาก DataTable
                string Cell = row["CELL"] == DBNull.Value
                    ? ""
                    : row["CELL"].ToString();

                lblCell.Text = row["CELL"].ToString();

                if (ddlCell != null)
                {
                    // ===== ดึงค่าแต่ละฝั่ง =====
                    string cellName = DataBinder.Eval(e.Row.DataItem, "CELL").ToString().Trim();
                    string ddlLocation_id = ddlLocation.SelectedValue;

                    //===== Bind Dropdown List : Cell =====
                    DataTable cellMaster = GetCellMaster(cellName, ddlLocation_id);
                    DataTable Location_Cell = (DataTable)ViewState["LOCATION_CELL"];

                    ddlCell.DataSource = cellMaster;
                    ddlCell.DataTextField = "location2_name";
                    ddlCell.DataValueField = "location2_id";
                    ddlCell.DataBind();
                    // เพิ่ม -- Select --
                    ddlCell.Items.Insert(0, new ListItem("-- Select --", ""));

                    // --- Select Cell & Location
                    if (Location_Cell.Rows.Count > 0)
                    {
                        ddlCell.SelectedValue = Location_Cell.Rows[0]["location2_id"].ToString();
                        ddlLocation.SelectedValue = Location_Cell.Rows[0]["location1_id"].ToString();
                        SetRowStatus(ddlCell, "MATCH_SQL");
                    }
                    else
                    {
                        // ✅ --- Fuzzy Compare ---
                        string inputNorm = Normalize(cellName);

                        double bestScore = 0;
                        string bestValue = "";

                        foreach (DataRow r in cellMaster.Rows)
                        {
                            string dbName = r["location2_name"].ToString();
                            string dbNorm = Normalize(dbName);

                            double score = Similarity(inputNorm, dbNorm);

                            if (score > bestScore)
                            {
                                bestScore = score;
                                bestValue = r["location2_id"].ToString();
                            }
                        }

                        // ✅ ถ้าใกล้เคียง >= 80%
                        if (bestScore >= 0.8)
                        {
                            ddlCell.SelectedValue = bestValue;

                            // ✅ custom status
                            SetRowStatus(ddlCell, "SIMILAR_80");

                            // debug (optional)
                            // lblStation.Text += " (~" + (bestScore * 100).ToString("F0") + "%)";
                        }
                        else
                        {
                            SetRowStatus(ddlCell, "NOT_FOUND");
                        }
                        //SetRowStatus(ddlCell, "NOT_FOUND");
                    }

                }
                #endregion

                // --- EDIT STATION
                #region

                // ✅ ดึงค่าจาก DataTable

                lblStation.Text = row["STATION"].ToString();

                if (ddlStation != null)
                {
                    //// ===== ดึงค่าแต่ละฝั่ง =====
                    string stationName = DataBinder.Eval(e.Row.DataItem, "STATION").ToString().Trim();
                    string ddlLocation_id = ddlLocation.SelectedValue;

                    // ===== Bind Dropdown List : Cell =====
                    DataTable stationMaster = GetStationMaster(stationName, ddlLocation_id);
                    DataTable Location_Station = (DataTable)ViewState["LOCATION_STATION"];

                    ddlStation.DataSource = stationMaster;
                    ddlStation.DataTextField = "location3_name";
                    ddlStation.DataValueField = "location3_id";
                    ddlStation.DataBind();

                    // เพิ่ม -- Select --
                    ddlStation.Items.Insert(0, new ListItem("-- Select --", ""));

                    // --- Select Cell & Location
                    if (Location_Station.Rows.Count > 0)
                    {
                        ddlStation.SelectedValue = Location_Station.Rows[0]["location3_id"].ToString();
                        SetRowStatus(ddlStation, "MATCH_SQL");
                    }
                    else
                    {
                        // ✅ --- Fuzzy Compare ---
                        string inputNorm = Normalize(stationName);

                        double bestScore = 0;
                        string bestValue = "";

                        foreach (DataRow r in stationMaster.Rows)
                        {
                            string dbName = r["location3_name"].ToString();
                            string dbNorm = Normalize(dbName);

                            double score = Similarity(inputNorm, dbNorm);

                            if (score > bestScore)
                            {
                                bestScore = score;
                                bestValue = r["location3_id"].ToString();
                            }
                        }

                        // ✅ ถ้าใกล้เคียง >= 80%
                        if (bestScore >= 0.8)
                        {
                            ddlStation.SelectedValue = bestValue;

                            // ✅ custom status
                            SetRowStatus(ddlStation, "SIMILAR_80");

                            // debug (optional)
                            // lblStation.Text += " (~" + (bestScore * 100).ToString("F0") + "%)";
                        }
                        else
                        {
                            SetRowStatus(ddlStation, "NOT_FOUND");
                        }

                        //SetRowStatus(ddlStation, "NOT_FOUND");
                    }
                }
                #endregion

                // --- EDIT Responsible
                lblResponsible.Text = row["RESPONSIBLE"].ToString();
                if (ddlResponsible != null)
                {
                    //// ===== ดึงค่าแต่ละฝั่ง =====
                    string responName = DataBinder.Eval(e.Row.DataItem, "RESPONSIBLE").ToString().Trim();
                    if (!string.IsNullOrEmpty(responName))
                    {
                        // 1️. เอาข้อความหลังจุด (.)
                        int dotIndex = responName.IndexOf('.');
                        if (dotIndex >= 0 && dotIndex + 1 < responName.Length)
                        {
                            responName = responName.Substring(dotIndex + 1);
                        }

                        // 2️. ตัดทิ้งตั้งแต่เจอ (
                        int bracketIndex = responName.IndexOf('(');
                        if (bracketIndex >= 0)
                        {
                            responName = responName.Substring(0, bracketIndex);
                        }
                        // 3️. ตัดช่องว่างหัว-ท้าย
                        responName = responName.Trim();
                        if (responName == "Aunchalee")
                            responName = "Anchalee";
                    }

                    // ===== Bind Dropdown List : Cell =====
                    DataTable responMaster = GetResponMaster(responName);

                    ddlResponsible.DataSource = responMaster;
                    ddlResponsible.DataTextField = "responsible_Name";
                    ddlResponsible.DataValueField = "emp_id";
                    ddlResponsible.DataBind();

                    // เพิ่ม -- Select --
                    ddlResponsible.Items.Insert(0, new ListItem("-- Select --", ""));
                    
                    DataTable Responsible = (DataTable)ViewState["RESPONSIBLE"];
                    // --- Select Cell & Location
                    if (Responsible.Rows.Count > 0)
                    {
                        ddlResponsible.SelectedValue = Responsible.Rows[0]["emp_id"].ToString();
                        SetRowStatus(ddlResponsible, "MATCH_SQL");
                    }
                    else
                    {
                        SetRowStatus(ddlResponsible, "NOT_FOUND");
                    }
                }

                // --- EDIT BOI
                lblBoi.Text = row["BOI / NON BOI"].ToString();
                if (ddlBoi != null)
                {
                    DataTable BoiMaster = (DataTable)ViewState["BOI_MASTER"];
                    ddlBoi.DataSource = BoiMaster;
                    ddlBoi.DataTextField = "boi_name";
                    ddlBoi.DataValueField = "boi_id";
                    ddlBoi.DataBind();

                    // เพิ่ม -- Select --
                    ddlBoi.Items.Insert(0, new ListItem("-- Select --", ""));

                    // ===== ดึงค่าแต่ละฝั่ง =====
                    string boi = DataBinder.Eval(e.Row.DataItem, "BOI / NON BOI").ToString().Trim();
                    string sqlboiName = DataBinder.Eval(e.Row.DataItem, "SQL_BOI").ToString().Trim();
                    string sqlboiId = DataBinder.Eval(e.Row.DataItem, "SQL_BOI_ID").ToString().Trim();
                    boi = NonText(boi);

                    // ===== CASE 1: SQL_BOI (ID) ถูกต้อง =====
                    if (!string.IsNullOrEmpty(sqlboiId) && sqlboiName == boi)
                    {
                        ddlBoi.SelectedValue = sqlboiId;
                        SetRowStatus(ddlBoi, "MATCH_SQL");
                    }

                    // ===== CASE 2: หา dept จาก Dept Code =====
                    else if (!string.IsNullOrEmpty(boi))
                    {
                        ListItem item = ddlBoi.Items
                            .Cast<ListItem>()
                            .FirstOrDefault(i => i.Text.Equals(boi, StringComparison.OrdinalIgnoreCase));

                        if (item != null)
                        {
                            ddlBoi.SelectedValue = item.Value;
                            SetRowStatus(ddlBoi, "MATCH_FROM_EXCEL");
                            //return;
                        }
                    }

                    // ===== CASE 3: ไม่เจอ =====
                    else
                    {
                        ddlBoi.SelectedIndex = 1;
                        SetRowStatus(ddlBoi, "NOT_FOUND");
                    }
                }

                // --- EDIT PAPER
                lblPaper.Text = row["PAPER"].ToString();
                if (ddlPaper != null)
                {

                    // ✅ เพิ่มรายการแบบ Manual
                    ddlPaper.Items.Add(new ListItem("-", "0"));
                    ddlPaper.Items.Add(new ListItem("Paper", "1"));

                    // ===== ดึงค่าแต่ละฝั่ง =====
                    string paper = DataBinder.Eval(e.Row.DataItem, "PAPER").ToString().Trim();
                    string sqlpaperId = DataBinder.Eval(e.Row.DataItem, "SQL_PAPER_ID").ToString().Trim();

                    // ===== CASE 1: SQL_Paper (ID) ถูกต้อง =====
                    if (!string.IsNullOrEmpty(sqlpaperId) && ddlPaper.Items.FindByValue(sqlpaperId) != null)
                    {
                        ddlPaper.SelectedValue = sqlpaperId;
                        SetRowStatus(ddlPaper, "MATCH_SQL");
                    }

                    // ===== CASE 2: หา dept จาก Dept Code =====
                    else if (!string.IsNullOrEmpty(paper))
                    {
                        ListItem item = ddlPaper.Items
                            .Cast<ListItem>()
                            .FirstOrDefault(i => i.Text.Equals(paper, StringComparison.OrdinalIgnoreCase));

                        if (item != null)
                        {
                            ddlPaper.SelectedValue = item.Value;
                            SetRowStatus(ddlPaper, "MATCH_FROM_EXCEL");
                            //return;
                        }
                    }

                    // ===== CASE 3: ไม่เจอ =====
                    else
                    {
                        ddlPaper.SelectedIndex = 0;
                        SetRowStatus(ddlPaper, "NOT_FOUND");
                    }
                }

                // --- Calibration Period
                lblPeriod.Text = row["INTERVAL"].ToString();
                if (ddlPeriod != null)
                {
                    DataTable periodMaster = (DataTable)ViewState["PERIOD_MASTER"];
                    ddlPeriod.DataSource = periodMaster;
                    ddlPeriod.DataTextField = "period_Name";
                    ddlPeriod.DataValueField = "period_Id";
                    ddlPeriod.DataBind();

                    // เพิ่ม -- Select --
                    ddlPeriod.Items.Insert(0, new ListItem("- Select -", ""));

                    string period = ExtractPeriod(row["INTERVAL"]);

                    if (!string.IsNullOrEmpty(period) && period != "-")
                    {
                        ListItem item = ddlPeriod.Items.FindByText(period);

                        if (item != null)
                        {
                            ddlPeriod.ClearSelection();
                            item.Selected = true;

                            SetRowStatus(ddlPeriod, "MATCH_SQL");
                        }
                        else
                        {
                            ddlPeriod.SelectedValue = "1";
                            SetRowStatus(ddlPeriod, "NOT_FOUND");
                        }
                    }
                    else
                    {
                        // ✅ เผื่อ ExtractPeriod ไม่เจอเลย
                        ddlPeriod.SelectedValue = "0";
                        SetRowStatus(ddlPeriod, "NOT_FOUND");
                    }

                }



                // Flag of Check Box All
                bool isMatch =
                        txtFixAsset != null && txtFixAsset.BackColor == System.Drawing.ColorTranslator.FromHtml("#28a745") &&
                        txtControlNo != null && txtControlNo.BackColor == System.Drawing.ColorTranslator.FromHtml("#28a745") &&
                        txtToolNo != null && txtToolNo.BackColor == System.Drawing.ColorTranslator.FromHtml("#28a745");

                if (isMatch)
                {
                    // ✅ ทำเครื่องหมายว่า row นี้เลือกได้
                    //e.Row.CssClass += "selectable-row";
                    chk.Checked = true;
                }

            }
        }

        // --- Function Compare
        public static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;

            for (int j = 1; j <= m; j++)
            {
                for (int i = 1; i <= n; i++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost
                    );
                }
            }

            return d[n, m];
        }
        public static double Similarity(string s1, string s2)
        {
            int dist = LevenshteinDistance(s1, s2);
            int maxLen = Math.Max(s1.Length, s2.Length);
            return 1.0 - (double)dist / maxLen;
        }
        public static string Normalize(string input)
        {
            return input.ToLower()
                .Replace("#", "")
                .Replace(".", "")
                .Replace(" ", "")
                .Trim();
        }

        // --- Fucntion translate tp UTF8
        public string FixThaiEncoding(string input)
        {

            if (string.IsNullOrWhiteSpace(input))
                return input;

            // ✅ detect text เพี้ยน
            if (input.Contains("à¸"))
            {
                byte[] bytes = Encoding.GetEncoding("latin1").GetBytes(input);
                return Encoding.UTF8.GetString(bytes);
            }

            return input;

        }


        protected string FormatDate(object val)
        {

            if (val == null || val == DBNull.Value)
                return "";

            DateTime dt;

            // ✅ กรณีมาจาก SQL อยู่แล้ว
            if (val is DateTime)
                return ((DateTime)val).ToString("dd-MMM-yyyy");

            string text = val.ToString().Trim();

            if (string.IsNullOrEmpty(text))
                return "";

            // 1️⃣ ลอง parse แบบปกติก่อน
            if (DateTime.TryParse(text, out dt))
                return dt.ToString("dd-MMM-yyyy");

            // 2️⃣ ลอง parse แบบกำหนด format ชัดเจน
            string[] normalFormats =
            {
                "dd/MM/yyyy",
                "MM/dd/yyyy",
                "dd-MM-yyyy",
                "MM-dd-yyyy",
                "yyyy-MM-dd"
            };

            if (DateTime.TryParseExact(
                text,
                normalFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dt))
            {
                return dt.ToString("dd-MMM-yyyy");
            }

            // 3️⃣ ❗ แก้เคส “วัน–เดือนสลับ” (เช่น 18/04/2014)
            char[] separators = { '/', '-' };
            string[] parts = text.Split(separators);
            int p1, p2, year;

            if (parts.Length == 3 &&
                int.TryParse(parts[0], out p1) &&
                int.TryParse(parts[1], out p2) &&
                int.TryParse(parts[2], out year))
            {
                // กรณี month เกิน 12 → สลับวัน ↔ เดือน
                if (p1 > 12 && p2 <= 12)
                {
                    // p1 = day, p2 = month
                    string fixedDate = string.Format("{0:D2}/{1:D2}/{2}", p2, p1, year);

                    if (DateTime.TryParseExact(
                        fixedDate,
                        "MM/dd/yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out dt))
                    {
                        return dt.ToString("dd-MMM-yyyy");
                    }
                }
            }

            // ❌ ไม่สามารถแก้ได้จริง
            return "";

        }
        protected string FormatDueDate(object val)
        {

            if (val == null || val == DBNull.Value)
                return "";

            DateTime dt;

            // ✅ กรณีมาจาก SQL อยู่แล้ว
            if (val is DateTime)
            {
                dt = (DateTime)val;
                return (dt.Year < 2023) ? "-" : dt.ToString("dd-MMM-yyyy");
            }
                //return (val.Year < 2023) ? "-" : ((DateTime)val).ToString("dd-MMM-yyyy");
            //return ((DateTime)val).ToString("dd-MMM-yyyy");
            //return (dt.Year < 2023) ? "-" : dt.ToString("dd-MMM-yyyy");

            string text = val.ToString().Trim();

            if (string.IsNullOrEmpty(text))
                return "";

            // 1️⃣ ลอง parse แบบปกติก่อน
            if (DateTime.TryParse(text, out dt))
            {
                //dt = (DateTime)val;
                return (dt.Year < 2023) ? "-" : dt.ToString("dd-MMM-yyyy");
            }
                //return dt.ToString("dd-MMM-yyyy");

            // 2️⃣ ลอง parse แบบกำหนด format ชัดเจน
            string[] normalFormats =
            {
                "dd/MM/yyyy",
                "MM/dd/yyyy",
                "dd-MM-yyyy",
                "MM-dd-yyyy",
                "yyyy-MM-dd"
            };

            if (DateTime.TryParseExact(
                text,
                normalFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dt))
            {
                return (dt.Year < 2023) ? "-" : dt.ToString("dd-MMM-yyyy");
                //return dt.ToString("dd-MMM-yyyy");
            }

            // 3️⃣ ❗ แก้เคส “วัน–เดือนสลับ” (เช่น 18/04/2014)
            char[] separators = { '/', '-' };
            string[] parts = text.Split(separators);
            int p1, p2, year;

            if (parts.Length == 3 &&
                int.TryParse(parts[0], out p1) &&
                int.TryParse(parts[1], out p2) &&
                int.TryParse(parts[2], out year))
            {
                // กรณี month เกิน 12 → สลับวัน ↔ เดือน
                if (p1 > 12 && p2 <= 12)
                {
                    // p1 = day, p2 = month
                    string fixedDate = string.Format("{0:D2}/{1:D2}/{2}", p2, p1, year);

                    if (DateTime.TryParseExact(
                        fixedDate,
                        "MM/dd/yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out dt))
                    {
                        return (dt.Year < 2023) ? "-" : dt.ToString("dd-MMM-yyyy");
                        //return dt.ToString("dd-MMM-yyyy");
                    }
                }
            }

            // ❌ ไม่สามารถแก้ได้จริง
            return "";

        }
        protected string FormatRemark(object interval,object remark)
        {
            string intervalText = interval == null || interval == DBNull.Value
                ? ""
                : interval.ToString().Trim();

            string remarkText = remark == null || remark == DBNull.Value
                ? ""
                : remark.ToString().Trim();

            // ✅ ถ้าเป็น "-" ให้ถือว่าไม่มีค่า
            if (intervalText == "-") intervalText = "";
            if (remarkText == "-") remarkText = "";

            if (!string.IsNullOrEmpty(intervalText) &&
                !string.IsNullOrEmpty(remarkText))
            {
                return intervalText + ", " + remarkText;
            }

            if (!string.IsNullOrEmpty(intervalText))
                return intervalText;

            if (!string.IsNullOrEmpty(remarkText))
                return remarkText;

            return "";
        }

        // --- Check NON-BOI
        protected string NonText(object val)
        {
            if (val == null || val == DBNull.Value)
                return "";

            string text = val.ToString().Trim();

            if (string.IsNullOrEmpty(text))
                return "";

            // ✅ ถ้ามีคำว่า NON ไม่สนพิมพ์เล็กใหญ่
            if (text.IndexOf("NON", StringComparison.OrdinalIgnoreCase) >= 0)
                return "NON";

            // ✅ กรณีอื่น ให้คืนค่าเดิม
            return text;
        }

        // --- Get Control No
        private string GetControlNoFromRow(DataRow row)
        {
            string controlNo = row["CONTROL NO"].ToString().Trim();

            // ✅ ถ้ามีค่าใช้งานได้
            if (!string.IsNullOrEmpty(controlNo) && controlNo != "-")
            {
                return controlNo;
            }

            // ✅ fallback → ดึงจาก REMARK
            return ExtractControlNo(row["INTERVAL"],row["REMARK"]);
        }
        protected string ExtractControlNo(object val ,object remark)
        {
            if ((val == null || val == DBNull.Value) && (remark == null || remark == DBNull.Value))
                return "";

            //string text = val.ToString();
            //string text = val.ToString().ToUpper();
            string text1 = val.ToString().ToUpper().Trim();
            string text2 = remark.ToString().ToUpper().Trim();

            if (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(text2))
                return "";

            // ✅ Clean text
            text1 = text1.Replace(" ", "")
                       .Replace(",", "")
                       .Replace(".", "");
            text2 = text2.Replace(" ", "")
                       .Replace(",", "")
                       .Replace(".", "");

            // ✅ Debug
            //Console.WriteLine(text);

            // หา CPT-YYYY-U##### (ไม่สนตัวพิมพ์ใหญ่/เล็ก)
            Match m1 = Regex.Match(
                text1,
                @"CPT-\d{4}-U\d{5}",
                RegexOptions.IgnoreCase);

            if (m1.Success)
            {
                return m1.Value;
            }
            else
            {
                Match m2 = Regex.Match(
                text2,
                @"CPT-\d{4}-U\d{5}",
                RegexOptions.IgnoreCase);

                if (m2.Success)
                {
                    return m2.Value;
                }
            }
            return "-";
        }

        // --- Get Calibration Period
        protected string ExtractPeriod(object val)
        {
            if (val == null || val == DBNull.Value)
                return "";

            string text = val.ToString().ToUpper().Trim();

            if (string.IsNullOrEmpty(text))
                return "";

            Match m = Regex.Match(
                text,
                @"\b\d+\s*(MONTHS?|YEARS?)\b|\b\d+(MONTHS?|YEARS?)\b",
                RegexOptions.IgnoreCase
            );

            if (m.Success)
            {
                string result = m.Value.ToUpper();

                // ✅ แยก 6MONTH → 6 MONTH
                result = Regex.Replace(result, @"(?<=\d)(?=[A-Z])", " ");

                // ✅ normalize
                result = result.Replace("MONTHS", "Month")
                               .Replace("MONTH", "Month")
                               .Replace("YEARS", "Year")
                               .Replace("YEAR", "Year");

                return result.Trim();
            }

            return "-";
        }

        // --- Get Sub Fix
        protected string ExtractSub(object val)
        {
            if (val == null || val == DBNull.Value)
                return "";

            string text = val.ToString().ToUpper().Trim();

            if (string.IsNullOrEmpty(text))
                return "";

            // ✅ หา pattern "SUB 2" หรือ "SUB2"
            Match m = Regex.Match(
                text,
                @"SUB\s*([0-9]+)",
                RegexOptions.IgnoreCase
            );

            if (m.Success)
            {
                return m.Groups[1].Value; // 👉 ดึงเฉพาะตัวเลข
            }

            return "";
        }


        protected void ddlLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlLocation = sender as DropDownList;
            if (ddlLocation == null) return;

            // ✅ หา GridViewRow ของ dropdown นี้
            GridViewRow row = ddlLocation.NamingContainer as GridViewRow;
            if (row == null) return;

            // ✅ เก็บ RowIndex ของแถวนี้
            //ViewState["SCROLL_ROW_INDEX"] = row.RowIndex;

            int rowIndex = row.RowIndex;

            // ✅ Register scroll script ที่นี่เลย
            string script =
                "setTimeout(function() {" +
                "  var row = document.getElementById('row_" + rowIndex + "');" +
                "  if (row) {" +
                "    row.scrollIntoView({ behavior: 'smooth', block: 'center' });" +
                "  }" +
                "}, 100);";

            ScriptManager.RegisterStartupScript(
                this,
                this.GetType(),
                "scrollBack_" + rowIndex, // ✅ key ไม่ซ้ำ
                script,
                true);


            string locationId = ddlLocation.SelectedValue;

            // ======================
            // ✅ ddlCell
            // ======================
            DropDownList ddlCell = row.FindControl("ddlCell") as DropDownList;
            if (ddlCell != null)
            {
                ddlCell.Items.Clear();

                if (!string.IsNullOrEmpty(locationId))
                {
                    DataTable dtCell = GetCellMasterByLocation(locationId);
                    ddlCell.DataSource = dtCell;
                    ddlCell.DataTextField = "location2_name";
                    ddlCell.DataValueField = "location2_id";
                    ddlCell.DataBind();
                }

                ddlCell.Items.Insert(0, new ListItem("-- Select --", ""));
                ddlCell.SelectedIndex = 0;
            }

            // ======================
            // ✅ ddlStation
            // ======================
            DropDownList ddlStation = row.FindControl("ddlStation") as DropDownList;
            if (ddlStation != null)
            {
                ddlStation.Items.Clear();

                if (!string.IsNullOrEmpty(locationId))
                {
                    DataTable dtStation = GetStationMasterByLocation(locationId);
                    ddlStation.DataSource = dtStation;
                    ddlStation.DataTextField = "location3_name";
                    ddlStation.DataValueField = "location3_id";
                    ddlStation.DataBind();
                }

                ddlStation.Items.Insert(0, new ListItem("-- Select --", ""));
                ddlStation.SelectedIndex = 0;
            }

            // ======================
            // ✅ Reset ค่าใน DataRow (สำคัญ)
            // ======================
            DataRowView drv = row.DataItem as DataRowView;
            if (drv != null)
            {
                drv["EDIT_CELL"] = DBNull.Value;
                drv["EDIT_STATION"] = DBNull.Value;
            }
        }


        // Query : Master List
        private DataTable GetModelMaster()
        {
            DataTable dt_model = new DataTable();

            string str_model = "Select * From jig_tool_system.dbo.draw_master_model where model_check = 1 order by model_name";
            dc.QryDT(conn, str_model);
            dt_model = dc.dt;
            return dt_model;
        }
        private DataTable GetDeptMaster()
        {
            DataTable dt_dept = new DataTable();

            string str_dept = "select LTRIM(RTRIM(dept_code)) as ID , LTRIM(RTRIM(dept_code)) as dept_code from View_dept_master order by dept_code";
            dc.QryDT(conn, str_dept);
            dt_dept = dc.dt;
            return dt_dept;
        }
        private DataTable GetLocationMaster()
        {
            DataTable dt_location = new DataTable();

            string str_location = "select * from jig_tool_system.dbo.master_location1 where is_use = 1";
            dc.QryDT(conn, str_location);
            dt_location = dc.dt;
            return dt_location;
        }
        private DataTable GetCellMaster(string cellName,string sqlLocation)
        {
            DataTable dt_cell = new DataTable();
            ViewState["CELL_ID"] = 0;
            //string location1_id;

            string str_cell_search = "select * from jig_tool_system.dbo.master_location2 where is_use = 1 and location2_name = '" + cellName + "'";
            dc.QryDT(conn, str_cell_search);
            ViewState["LOCATION_CELL"] = dc.dt;
            //dt_cell = dc.dt;

            if (dc.dt.Rows.Count > 0)
            {
                ViewState["CELL_ID"] = dc.dt.Rows[0]["location2_id"].ToString();
                sqlLocation = dc.dt.Rows[0]["location1_id"].ToString();
                //ViewState["LOCATION_ID"] = dc.dt.Rows[0]["location1_id"].ToString();
            }

            string str_cell = "select * from jig_tool_system.dbo.master_location2 where is_use = 1 and location1_id = '" + sqlLocation + "' order by location2_name";
            dc.QryDT(conn, str_cell);
            dt_cell = dc.dt;
            return dt_cell;
        }
        public static DataTable GetCellMasterByLocation(string locationId)
        {

            DataTable dt = new DataTable();

            string sql = @"
                            select *
                            from jig_tool_system.dbo.master_location2
                            where is_use = 1
                              and location1_id = @locationId
                            order by location2_name";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@locationId", locationId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            return dt;

        }
        public static DataTable GetStationMasterByLocation(string locationId)
        {

            DataTable dt = new DataTable();

            string sql = @"
                            select *
                            from jig_tool_system.dbo.master_location3
                            where location1_id = @locationId
                            order by location3_name";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["jig_tool"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@locationId", locationId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            return dt;

        }
        private DataTable GetStationMaster(string stationName, string sqlLocation)
        {
            DataTable dt_station = new DataTable();
            //ViewState["STATION_ID"] = 0;

            string str_station_search = "select * from jig_tool_system.dbo.master_location3 where location3_name = '" + stationName + "'";
            dc.QryDT(conn, str_station_search);
            ViewState["LOCATION_STATION"] = dc.dt;
            //dt_cell = dc.dt;

            if (dc.dt.Rows.Count > 0)
            {
                ViewState["STATION_ID"] = dc.dt.Rows[0]["location3_id"].ToString();
                sqlLocation = dc.dt.Rows[0]["location1_id"].ToString();
            }

            string str_station = "select * from jig_tool_system.dbo.master_location3 where location1_id = '" + sqlLocation + "' order by location3_name";
            dc.QryDT(conn, str_station);
            dt_station = dc.dt;
            return dt_station;
        }
        private DataTable GetResponMaster(string responName)
        {
            DataTable dt_respon = new DataTable();

            string str_respon_search = " SELECT * FROM ( SELECT A.emp_id , A.pic_dept + ' (' + B.dept_name + ')' AS responsible_Name " +
                                        " FROM Responsible_Dept A " +
                                        " LEFT JOIN View_dept_master B ON A.dept_code = B.dept_code) t" +
                                        " WHERE responsible_Name LIKE '%" + responName + "%' " +
                                        " group by emp_id , responsible_name" +
                                        " ORDER BY responsible_Name;";

            dc.QryDT(conn, str_respon_search);
            ViewState["RESPONSIBLE"] = dc.dt;

            string str_respon = "select A.emp_id , A.pic_dept + ' (' + B.dept_name +')' as responsible_Name" +
                                " from Responsible_Dept A" +
                                " left join View_dept_master B on A.dept_code = B.dept_code" +
                                " group by A.emp_id , A.pic_dept , B.dept_name " +
                                " order by responsible_Name";
            dc.QryDT(conn, str_respon);
            dt_respon = dc.dt;
            return dt_respon;
        }
        private DataTable GetBoiMaster()
        {
            DataTable dt_boi = new DataTable();

            string str_boi = "select * from Master_BOI";
            dc.QryDT(conn, str_boi);
            dt_boi = dc.dt;
            return dt_boi;
        }
        private DataTable GetStatusMaster()
        {
            DataTable dt_status = new DataTable();

            string str_status = "select * from tool_equipment_status";
            dc.QryDT(conn, str_status);
            dt_status = dc.dt;
            return dt_status;
        }
        private DataTable GetPeriodMaster()
        {
            DataTable dt_period = new DataTable();

            string str_period = "Select * From Calibration_Period Where is_use = '1' Order By period_Id";
            dc.QryDT(conn, str_period);
            dt_period = dc.dt;
            return dt_period;
        }


        // Helper: แสดง Bootstrap Alert ใน Label
        private void ShowAlert(System.Web.UI.WebControls.Label lbl, string message, string type)
        {
            lbl.Text = "<div class=\"alert alert-" + type + " alert-dismissible\" style=\"margin-top:10px;\">"
                     + "<button type=\"button\" class=\"close\" data-dismiss=\"alert\">&times;</button>"
                     + HttpUtility.HtmlEncode(message)
                     + "</div>";
        }

        // Helper: Dropdown Matching Status (Color)
        private void SetRowStatus(DropDownList ddl , string status)
        {
            if (ddl == null) return;

            switch (status)
            {
                case "MATCH_SQL":
                    ddl.Style["background-color"] = "#d4edda"; // เขียวอ่อน
                    ddl.Style["border"] = "1px solid #28a745";
                    break;

                case "MATCH_FROM_EXCEL":
                    ddl.Style["background-color"] = "#9ED3DC"; // ฟ้าอ่อน
                    ddl.Style["border"] = "1px solid #17a2b8";
                    break;

                case "NOT_FOUND":
                    ddl.Style["background-color"] = "#f8d7da"; // แดงอ่อน
                    ddl.Style["border"] = "1px solid #dc3545";
                    break;

                case "SIMILAR_80":
                    ddl.Style["background-color"] = "#fff3cd"; // เหลืองอ่อน
                    ddl.Style["border"] = "1px solid #65670a";
                    break;

                default:
                    ddl.Style.Clear();
                    ddl.Enabled = true;
                    break;
            }
        }

        // Helper: Textbox Matching Status (Color)
        private void SetStatus(TextBox txt, string text, string bgColor)
        {
            txt.Text = text;
            txt.BackColor = System.Drawing.ColorTranslator.FromHtml(bgColor);
            txt.ForeColor = System.Drawing.Color.White;
            txt.Font.Bold = true;
        }



        // ----------------------------------------------------------------
        // Button : Preview -> Show Data Update Preview
        // ----------------------------------------------------------------
        protected void btnPreview_Click(object sender, EventArgs e)
        {
            //DataTable dt = ImportedTable;
            //List<int> selectedIndexes = GetSelectedRowIndexes();

            //if (selectedIndexes.Count == 0)
            //{
            //    ShowAlert(lblStatus, "กรุณาเลือกรายการก่อน", "warning");
            //    return;
            //}

            //StringBuilder sb = new StringBuilder();

            //sb.AppendLine("PREVIEW UPDATE RESULT");
            //sb.AppendLine("---------------------");

            //foreach (int idx in selectedIndexes)
            //{
            //    DataRow dr = dt.Rows[idx];

            //    sb.AppendLine(string.Format(
            //        "ROW {0} | EDIT_STATUS = {1} | EDIT_DEPT = {2}",
            //        idx + 1,
            //        dr["EDIT_STATUS"],
            //        dr["EDIT_DEPT"]
            //    ));
            //}

            //txtPreview.Text = sb.ToString();   // TextBox / Label แสดงผล

        }


        // ----------------------------------------------------------------
        // Button : Save Selected Rows to SQL Server
        // ----------------------------------------------------------------
        protected void btnSaveSelected_Click(object sender, EventArgs e)
        {
            DataTable dt = ImportedTable;

            foreach (GridViewRow row in gvData.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow) continue;

                int idx = row.DataItemIndex;
                DataRow dr = dt.Rows[idx];

                //CheckBox Box
                CheckBox chk = row.FindControl("chkRow") as CheckBox;
                dr["CHK_STATUS"] = (chk != null && chk.Checked);

                //Sub Fix
                TextBox txtSubfix = row.FindControl("txtSubfix") as TextBox;
                if (txtSubfix != null)
                    dr["EDIT_SUBFIX"] = txtSubfix.Text.Trim();

                //Control No
                TextBox txtControlNo = row.FindControl("txtControlNo") as TextBox;
                if (txtControlNo != null)
                    dr["EDIT_CONTROLNO"] = txtControlNo.Text.Trim();

                //Status
                DropDownList ddlStatus = row.FindControl("ddlStatus") as DropDownList;
                if (ddlStatus != null)
                    dr["EDIT_STATUS"] = ddlStatus.SelectedValue;

                //Dept
                DropDownList ddlDept = row.FindControl("ddlDept") as DropDownList;
                if (ddlDept != null)
                    dr["EDIT_Dept"] = ddlDept.SelectedValue;

                //Model
                DropDownList ddlModel = row.FindControl("ddlModel") as DropDownList;
                if (ddlModel != null)
                    dr["EDIT_MODEL"] = ddlModel.SelectedValue;

                //Location
                DropDownList ddlLocation = row.FindControl("ddlLocation") as DropDownList;
                if (ddlLocation != null)
                    dr["EDIT_Location"] = ddlLocation.SelectedValue;

                //Cell
                DropDownList ddlCell = row.FindControl("ddlCell") as DropDownList;
                if (ddlCell != null)
                    dr["EDIT_CELL"] = ddlCell.SelectedValue;

                //Station
                DropDownList ddlStation = row.FindControl("ddlStation") as DropDownList;
                if (ddlStation != null)
                    dr["EDIT_STATION"] = ddlStation.SelectedValue;

                //Responsible
                DropDownList ddlResponsible = row.FindControl("ddlResponsible") as DropDownList;
                if (ddlResponsible != null)
                    dr["EDIT_Responsible"] = ddlResponsible.SelectedValue;

                //Boi
                DropDownList ddlBoi = row.FindControl("ddlBoi") as DropDownList;
                if (ddlBoi != null)
                    dr["EDIT_BOI"] = ddlBoi.SelectedValue;

                //Paper
                DropDownList ddlPaper = row.FindControl("ddlPaper") as DropDownList;
                if (ddlPaper != null)
                    dr["EDIT_PAPER"] = ddlPaper.SelectedValue;

                //Period
                DropDownList ddlPeriod = row.FindControl("ddlPeriod") as DropDownList;
                if (ddlPeriod != null)
                    dr["EDIT_PERIOD"] = ddlPeriod.SelectedValue;

                //Operation Date
                Label lbl_operationDate = row.FindControl("lbl_operationDate") as Label;
                if (lbl_operationDate != null)
                    dr["EDIT_OPERATION_DATE"] = lbl_operationDate.Text;

                //Due Date
                Label lbl_duedate = row.FindControl("lbl_duedate") as Label;
                if (lbl_duedate != null)
                    dr["EDIT_DUE_DATE"] = lbl_duedate.Text;

                //Remark
                Label lbl_remark = row.FindControl("lbl_remark") as Label;
                if (lbl_remark != null)
                    dr["EDIT_REMARK"] = lbl_remark.Text;

            }

            // ✅ จากนี้ dt มีค่าทุกอย่างที่ user เลือกแล้ว

            ImportedTable = dt;
            SaveToDatabase(ImportedTable);

        }

        protected void SaveToDatabase(DataTable dt)
        {
            string update = "";
            
            foreach (DataRow row in dt.Rows)
            {
                // ✅ ถ้าไม่ได้เลือก (Check Box : Update)
                if (!Convert.ToBoolean(row["CHK_STATUS"]))
                    continue;

                update = "";

                // --- Query Seart Dept_id , division
                string str_dept = "select dept_id ,division from View_dept_master where dept_code = '" + row["EDIT_Dept"].ToString() + "'";
                dc.QryDT(conn, str_dept);
                DataTable sqlDeptCode = dc.dt;

                // --- Search Sub fix in [INTERVAL]
                //int subfix = GetSubValue(row["INTERVAL"].ToString());

                string toolId = row["SQL_ToolId"].ToString();
                string subId = row["SQL_SubId"].ToString();
                string systemId = row["SQL_System"].ToString();

                if (string.IsNullOrWhiteSpace(toolId))
                {
                    row["UPDATE_STATUS"] = "No Tool Id";
                    continue;
                }

                if (string.IsNullOrWhiteSpace(subId))
                {
                    row["UPDATE_STATUS"] = "No Sub Id";
                    continue;
                }

                if (string.IsNullOrWhiteSpace(systemId))
                {
                    row["UPDATE_STATUS"] = "No System Id";
                    continue;
                }

                if (row["SQL_System"].ToString() == "1")
                {
                    try
                    {

                        update += " Update jig_tool_system.dbo.toolSub_list SET ";
                        //---Status
                        update += " status_id = " + row["EDIT_STATUS"].ToString() + " ";
                        //---Control No
                        update += " , control_no = '" + row["EDIT_CONTROLNO"].ToString() + "' ";
                        //---Sub Fix
                        update += " , sub_fix = " + row["EDIT_SUBFIX"].ToString() + " ";
                        //---dept code
                        update += " , dept_code = " + sqlDeptCode.Rows[0]["dept_id"].ToString() + " ";
                        //---division
                        update += " , division = " + sqlDeptCode.Rows[0]["division"].ToString() + " ";
                        //---Model
                        update += " , model_id = " + row["EDIT_MODEL"].ToString() + " ";
                        //---Location
                        update += " , location1_id = " + row["EDIT_LOCATION"].ToString() + " ";
                        //---Cell
                        update += " , location2_id = " + row["EDIT_CELL"].ToString() + " ";
                        //---Station
                        update += " , location3_id = " + row["EDIT_STATION"].ToString() + " ";
                        //---Responsible
                        update += " , rsp_empID = '" + row["EDIT_Responsible"].ToString() + "' ";
                        //---BOI
                        update += " , boi = " + row["EDIT_BOI"].ToString() + " ";
                        //---Paper
                        update += " , paper = " + row["EDIT_PAPER"].ToString() + " ";
                        //---Fix Asset Name
                        update += " , assetName = '" + row["NAME"].ToString() + "' ";
                        //---Serial No
                        update += " , serialNo = '" + row["SERIAL NO"].ToString() + "' ";
                        //---Manufacture
                        update += " , Manufacturing = '" + row["MANUFACTURE"].ToString() + "' ";
                        //---Price
                        update += " , cost = '" + row["PRICE"].ToString() + "' ";
                        //---PR PO
                        update += " , po = '" + row["PR NO"].ToString() + "' ";
                        //---Operation Date
                        update += " , start_use = '" + row["EDIT_OPERATION_DATE"] + "' ";
                        //---Period
                        //update += " , period_id = " + row["EDIT_PERIOD"] + " ";

                        //--- Equipment Due Date
                        if (row["EDIT_DUE_DATE"] == DBNull.Value || row["EDIT_DUE_DATE"].ToString() == "-")
                        {
                            update += " , duedate = NULL ";
                        }
                        else
                        {
                            DateTime dueDate = Convert.ToDateTime(row["EDIT_DUE_DATE"]);
                            update += " , duedate = '" + row["EDIT_DUE_DATE"] + "' ";
                            //update += " , duedate = '" + dueDate.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                        }

                        //---Remark
                        update += " , Remark = '" + row["EDIT_REMARK"].ToString() + "' ";

                        //---confirm remark
                        update += " , confirm_remark = 1";

                        //---use_status
                        update += " , use_status = 1";

                        //--- Where Tool ID & Tool Sub ID
                        update += " WHERE tool_id = " + row["SQL_ToolId"].ToString() + " and toolSub_id = " + row["SQL_SubId"].ToString() + " ";

                        // --- Update on SQL
                        dc.RunCommand(conn, update);
                        // --- Update on List
                        row["UPDATE_STATUS"] = "Completed";
                    }
                    catch (Exception ex)
                    {
                        //show_error2("Error : " + ex.Message);
                        row["UPDATE_STATUS"] = "Error";
                        row["UPDATE_ERROR"] = ex.Message;
                    }
                }

                else if (row["SQL_System"].ToString() == "2")
                {
                    try
                    {
                        update += " Update Equipment_sub_id SET ";
                        //---Status
                        update += " status_use = " + row["EDIT_STATUS"].ToString() + " ";
                        //---Control No
                        update += " , control_no = '" + row["EDIT_CONTROLNO"].ToString() + "' ";
                        //---Sub Fix
                        update += " , sub_fix = " + row["EDIT_SUBFIX"].ToString() + " ";
                        //---dept code
                        update += " , dept_code = " + sqlDeptCode.Rows[0]["dept_id"].ToString() + " ";
                        //---division
                        update += " , division = " + sqlDeptCode.Rows[0]["division"].ToString() + " ";
                        //---Model
                        update += " , model_id = " + row["EDIT_MODEL"].ToString() + " ";
                        //---Location
                        update += " , location_id = " + row["EDIT_LOCATION"].ToString() + " ";
                        //---Cell
                        update += " , cell_id = " + row["EDIT_CELL"].ToString() + " ";
                        //---Station
                        update += " , station_id = " + row["EDIT_STATION"].ToString() + " ";
                        //---Responsible
                        update += " , rsp_empID = '" + row["EDIT_Responsible"].ToString() + "' ";
                        //---BOI
                        update += " , boi = " + row["EDIT_BOI"].ToString() + " ";
                        //---Paper
                        update += " , paper = " + row["EDIT_PAPER"].ToString() + " ";
                        //---Fix Asset Name
                        update += " , assetName = '" + row["NAME"].ToString() + "' ";
                        //---Serial No
                        update += " , serial_no = '" + row["SERIAL NO"].ToString() + "' ";
                        //---Manufacture
                        update += " , manufacturing = '" + row["MANUFACTURE"].ToString() + "' ";
                        //---Price
                        update += " , price = '" + row["PRICE"].ToString() + "' ";
                        //---PR PO
                        update += " , po = '" + row["PR NO"].ToString() + "' ";
                        //---Operation Date
                        update += " , operation_date = '" + row["EDIT_OPERATION_DATE"] + "' ";
                        //---Period
                        update += " , period_id = " + row["EDIT_PERIOD"] + " ";

                        //--- Equipment Due Date
                        if (row["EDIT_DUE_DATE"] == DBNull.Value || row["EDIT_DUE_DATE"].ToString() == "-")
                        {
                            update += " , duedate = NULL ";
                        }
                        else
                        {
                            DateTime dueDate = Convert.ToDateTime(row["EDIT_DUE_DATE"]);
                            update += " , duedate = '" + row["EDIT_DUE_DATE"] + "' ";
                            //update += " , duedate = '" + dueDate.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                        }

                        //---Remark
                        update += " , Remark = '" + row["EDIT_REMARK"].ToString() + "' ";

                        //---confirm remark
                        update += " , confirm_remark = 1";

                        //--- Where Tool ID & Tool Sub ID
                        update += " WHERE Equipment_id = " + row["SQL_ToolId"].ToString() + " and Equipment_sub_id = " + row["SQL_SubId"].ToString() + " ";

                        // --- Update on SQL
                        dc.RunCommand(conn, update);
                        // --- Update on List
                        row["UPDATE_STATUS"] = "Completed";
                    }
                    catch (Exception ex)
                    {
                        //show_error2("Error : " + ex.Message);
                        row["UPDATE_STATUS"] = "Error";
                        row["UPDATE_ERROR"] = ex.Message;
                    }
                }
               
            }
        }

        int GetSubValue(string interval)
        {
            if (string.IsNullOrEmpty(interval))
                return 0;

            Match m = Regex.Match(interval, @"Sub\s*(\d+)", RegexOptions.IgnoreCase);

            if (m.Success)
                return int.Parse(m.Groups[1].Value);

            return 1;
        }


        // ----------------------------------------------------------------
        // Button : Export Data of Error Update
        // ----------------------------------------------------------------
        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            DataTable dt = ImportedTable;

            using (var wb = new ClosedXML.Excel.XLWorkbook())
            {
                wb.Worksheets.Add(dt, "Update_Result");

                Response.Clear();
                Response.ContentType =
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader(
                    "content-disposition",
                    "attachment;filename=Update_Result.xlsx");

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    ms.WriteTo(Response.OutputStream);
                }
                Response.End();
            }
        }

        // ----------------------------------------------------------------
        // Clear / Reset
        // ----------------------------------------------------------------
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ImportedTable = null;
            gvData.DataSource = null;
            gvData.DataBind();
            pnlGrid.Visible = false;
            lblStatus.Text = "";
            lblSaveResult.Text = "";
        }

        // ================================================================
        // Alert : Toast
        // ================================================================
        protected void alert_toast(string detail, string title, string type, string showtime)
        {
            if (type == "warn" || type == "error" || type == "success" || type == "info")
            {

                int durationMs;
                if (!int.TryParse(showtime, out durationMs))
                    durationMs = 3000; // default 3 วิ

                // Encode ค่าที่เป็น string เพื่อกัน JS พัง/XSS
                string jsDetail = HttpUtility.JavaScriptStringEncode(detail ?? string.Empty);
                string jsTitle = HttpUtility.JavaScriptStringEncode(title ?? string.Empty);
                string jsType = HttpUtility.JavaScriptStringEncode(type ?? "info");

                // ประกอบสคริปต์ (duration ไม่ต้อง encode และไม่ต้องใส่ quote)
                string script =
                    "showToast('" + jsDetail + "', { " +
                        "title: '" + jsTitle + "', " +
                        "type: '" + jsType + "', " +
                        "duration: " + durationMs + " " +
                    "});";

                // ใช้ key ไม่ซ้ำ ป้องกันการชนกัน
                string key = "toast_" + DateTime.UtcNow.Ticks;
                ScriptManager.RegisterStartupScript(this, this.GetType(), key, script, true);
            }
        }





        // ================================================================
        // Helper: Read CSV
        // ================================================================
        private DataTable ReadCsv(Stream stream)
        {
            DataTable dt = new DataTable();

            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true))
            {
                bool isHeader = true;
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line.Trim()))
                        continue;

                    string[] fields = ParseCsvLine(line);

                    if (isHeader)
                    {
                        foreach (string f in fields)
                        {
                            string colName = f.Trim().Trim('"');
                            if (string.IsNullOrEmpty(colName))
                                colName = "Column" + (dt.Columns.Count + 1).ToString();
                            dt.Columns.Add(EnsureUnique(dt, colName));
                        }
                        isHeader = false;
                    }
                    else
                    {
                        DataRow row = dt.NewRow();
                        for (int i = 0; i < dt.Columns.Count && i < fields.Length; i++)
                            row[i] = fields[i].Trim().Trim('"');
                        dt.Rows.Add(row);
                    }
                }
            }
            return dt;
        }

        // Simple CSV parser รองรับ quoted fields
        private string[] ParseCsvLine(string line)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            StringBuilder current = new StringBuilder();

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString());
            return result.ToArray();
        }

        

        // ================================================================
        // Helper: ป้องกัน column ชื่อซ้ำ
        // ================================================================
        private string EnsureUnique(DataTable dt, string name)
        {
            if (!dt.Columns.Contains(name))
                return name;

            int i = 2;
            while (dt.Columns.Contains(name + "_" + i.ToString()))
                i++;

            return name + "_" + i.ToString();
        }
       

    }
}
