<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="CalibrationManagement.aspx.cs"
    Inherits="Tool_Equipment_Control_System.CalibrationManagement" %>

<asp:Content ID="Tool_Equipment_Home" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">


    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <link href="css/bootstrap_sum.css" rel="stylesheet">
    <%--<link href="CSS/bootstrap.css" rel="stylesheet" />--%>
    <%--<link href="CSS/bootstrap3.3.7.min.css" rel="stylesheet" />--%>
    <link href="CSS/w3v4.10.css" rel="stylesheet" />
    <%--<link rel="stylesheet" href="Content/font-awesome.css">--%>
    <link href="css/styles.css" rel="stylesheet">
    <%--<link href="CSS/styleMenu.css" rel="stylesheet" />--%>
    <%--<link href="CSS/font-awesome.css" rel="stylesheet" />--%>
    <link href="DataTables-1.10.18/css/dataTables.css" rel="stylesheet">
    <link href="Content/chosen.css" rel="stylesheet">
    <link href="CSS/fontsgoogleapis.css" rel="stylesheet">
    <%--<link href="CSS/all.min.css" rel="stylesheet">--%>  
    <link href="CSS/all.min.css">
    <link rel="stylesheet" href="CSS/jquery-ui.css">
    <link href="DateTimePicker/2-bootstrap-datetimepicker.min.css" rel="stylesheet" />
    <link href="DateTimePicker/3-bootstrap-datetimepicker-standalone.css" rel="stylesheet" />
    
    <script src="Scripts/jquery-1.9.1.js"></script>
    <%--<script src="DataTables-1.10.18/js/dataTables.js"></script>--%>
    <%--<script src="DataTables-1.10.18/FixedColumns-3.2.5/js/fixedColumns.dataTables.js"></script>--%>
    <script src="Scripts/chosen.jquery.js"></script>        
    <script src="Scripts/jquery-ui.min.js"></script>    
    <script src="Scripts/script.js"></script>
    <script src="DateTimePicker/1-moment.min.js"></script>
    <script src="DateTimePicker/4-bootstrap-datetimepicker.min.js"></script>
    <%--<script src="Scripts/bootstrap.min.js"></script>--%>

    <style>  
        body { background:#f4f6f9; font-family:'Segoe UI',sans-serif; }
        .page-header-bar {
            background:#1a3a5c; color:#fff;
            padding:14px 20px; margin-bottom:20px;
            border-radius:0 0 6px 6px;
        }
        .page-header-bar h4 { margin:0; font-size:18px; }
        /* ===== Summary Cards ===== */
        .card-wrap { display:flex; flex-wrap:wrap; gap:12px; margin-bottom:20px; }
        .stat-card {
            flex:1 1 160px; border-radius:8px; padding:14px 16px;
            color:#fff; min-width:140px; cursor:pointer;
            box-shadow:0 2px 6px rgba(0,0,0,.15);
            transition:transform .15s, box-shadow .15s;
        }
        .stat-card:hover { transform:translateY(-2px); box-shadow:0 4px 12px rgba(0,0,0,.2); }
        .stat-card .fn-label { font-size:12px; opacity:.85; margin-bottom:4px; }
        .stat-card .fn-count { font-size:28px; font-weight:700; line-height:1; }
        .stat-card .fn-sub   { font-size:11px; opacity:.75; margin-top:4px; }
        .card-overdue { background:linear-gradient(135deg,#c0392b,#7b241c); }
        .card-duesoon { background:linear-gradient(135deg,#e67e22,#a04000); }
        .card-sent    { background:linear-gradient(135deg,#2980b9,#1a5276); }
        .card-normal  { background:linear-gradient(135deg,#27ae60,#1a6b3c); }
        .card-total   { background:linear-gradient(135deg,#8e44ad,#5b2c6f); }
        /* ===== Alert Banner ===== */
        .calib-alert {
            border-left:4px solid #c0392b; background:#fdf3f2;
            padding:10px 15px; border-radius:4px; margin-bottom:15px;
        }
        .calib-alert .alert-title {
            color:#c0392b; font-weight:700; font-size:14px; margin-bottom:6px;
        }
        .calib-alert ul { margin:0; padding-left:18px; font-size:13px; color:#555; }
        /* ===== Filter Box ===== */
        .filter-box {
            background:#fff; border-radius:8px; padding:16px 20px;
            margin-bottom:18px; box-shadow:0 1px 4px rgba(0,0,0,.08);
        }
        .filter-box .form-group { margin-bottom:10px; }
        .filter-box label { font-size:12px; color:#555; font-weight:600; }
        /* ===== Grid ===== */
        .grid-wrap {
            background:#fff; border-radius:8px; padding:16px;
            box-shadow:0 1px 4px rgba(0,0,0,.08);
        }
        .grid-title { font-size:14px; font-weight:700; color:#1a3a5c; margin-bottom:12px; }
        .grid-legend { float:right; font-size:12px; color:#888; }
        .table th {
            background:#1a3a5c !important; color:#fff;
            font-size:12px; border:none !important; white-space:nowrap;
        }
        .table td { font-size:13px; vertical-align:middle !important; }
        .row-overdue { background:#fde8e6 !important; }
        .row-duesoon { background:#fef6e4 !important; }
        .row-sent    { background:#e8f4fd !important; }
        /* ===== Status Badges ===== */
        .badge-overdue { background:#c0392b; color:#fff; padding:2px 8px; border-radius:10px; font-size:11px; white-space:nowrap; }
        .badge-duesoon { background:#e67e22; color:#fff; padding:2px 8px; border-radius:10px; font-size:11px; white-space:nowrap; }
        .badge-sent    { background:#2980b9; color:#fff; padding:2px 8px; border-radius:10px; font-size:11px; white-space:nowrap; }
        .badge-normal  { background:#27ae60; color:#fff; padding:2px 8px; border-radius:10px; font-size:11px; white-space:nowrap; }
        .badge-nodate  { background:#95a5a6; color:#fff; padding:2px 8px; border-radius:10px; font-size:11px; white-space:nowrap; }
        /* ===== Days Remaining ===== */
        .days-overdue { color:#c0392b; font-weight:700; }
        .days-duesoon { color:#e67e22; font-weight:700; }
        .days-normal  { color:#27ae60; }
        .days-na      { color:#95a5a6; }
        /* ===== Modal ===== */
        .modal-header { background:#1a3a5c; color:#fff; border-radius:4px 4px 0 0; }
        .modal-header .close { color:#fff; opacity:.8; font-size:20px; }
        .modal-header .close:hover { opacity:1; }
        .new-due-preview {
            background:#e9ecef; color:#1a3a5c; font-weight:700;
            padding:6px 10px; border-radius:4px; font-size:14px; margin-top:4px;
        }
        /* ===== Toast ===== */
        .toast-bar {
            position:fixed; top:20px; right:20px; z-index:9999;
            padding:12px 22px; border-radius:6px; color:#fff;
            box-shadow:0 3px 10px rgba(0,0,0,.25); display:none;
            font-size:14px; min-width:260px;
        }
        .toast-success { background:#27ae60; }
        .toast-error   { background:#c0392b; }
    </style>

    <asp:ScriptManager runat="server" ID="scriptManager" EnablePageMethods="true"></asp:ScriptManager>

    <asp:UpdatePanel runat="server" ID="UP1">
        <ContentTemplate>

        <!-- Toast -->
        <div id="toastBar" class="toast-bar"></div>

        <!-- Hidden Fields (ClientIDMode=Static เพื่อให้ JS อ้างอิง ID ตรงๆ) -->
        <asp:HiddenField ID="hfSendID"         runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfReturnID"       runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfReturnInterval" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfEditID"         runat="server" ClientIDMode="Static" />

        <!-- ===== HEADER ===== -->
        <div class="page-header-bar" style="margin-top:30px;">
            <div style="display:flex;align-items:center;justify-content:space-between;flex-wrap:wrap;gap:8px;">
                <h4><span class="glyphicon glyphicon-time"></span>&nbsp; Calibration Management — ระบบแจ้งเตือนเครื่องมือวัด</h4>
                <a href="JigDashboard.aspx" class="btn btn-sm btn-default" style="color:#1a3a5c;">
                    <span class="glyphicon glyphicon-arrow-left"></span> กลับหน้าหลัก
                </a>
            </div>
        </div>

        <div class="container-fluid">

            <!-- ===== SUMMARY CARDS (คลิกเพื่อ Filter) ===== -->
            <div class="card-wrap">
                <div class="stat-card card-overdue" onclick="filterByAlert('Overdue')" title="คลิกเพื่อกรอง: เกินกำหนด">
                    <div class="fn-label"><span class="glyphicon glyphicon-warning-sign"></span> เกินกำหนด</div>
                    <div class="fn-count"><asp:Label ID="lblOverdue" runat="server" Text="0" /></div>
                    <div class="fn-sub">รายการ</div>
                </div>
                <div class="stat-card card-duesoon" onclick="filterByAlert('DueSoon')" title="คลิกเพื่อกรอง: ครบกำหนดใน 30 วัน">
                    <div class="fn-label"><span class="glyphicon glyphicon-bell"></span> ครบกำหนดใน 30 วัน</div>
                    <div class="fn-count"><asp:Label ID="lblDueSoon" runat="server" Text="0" /></div>
                    <div class="fn-sub">รายการ</div>
                </div>
                <div class="stat-card card-sent" onclick="filterByAlert('Sent')" title="คลิกเพื่อกรอง: ส่งแล้ว">
                    <div class="fn-label"><span class="glyphicon glyphicon-send"></span> ส่ง Calibration แล้ว</div>
                    <div class="fn-count"><asp:Label ID="lblSent" runat="server" Text="0" /></div>
                    <div class="fn-sub">รอผลการ Calibrate</div>
                </div>
                <div class="stat-card card-normal" onclick="filterByAlert('Normal')" title="คลิกเพื่อกรอง: ปกติ">
                    <div class="fn-label"><span class="glyphicon glyphicon-ok-circle"></span> ปกติ</div>
                    <div class="fn-count"><asp:Label ID="lblNormal" runat="server" Text="0" /></div>
                    <div class="fn-sub">รายการ</div>
                </div>
                <div class="stat-card card-total" onclick="filterByAlert('')" title="แสดงทั้งหมด">
                    <div class="fn-label"><span class="glyphicon glyphicon-list"></span> ทั้งหมด</div>
                    <div class="fn-count"><asp:Label ID="lblTotal" runat="server" Text="0" /></div>
                    <div class="fn-sub">เครื่องมือวัด</div>
                </div>
            </div>

            <!-- ===== ALERT BANNER ===== -->
            <asp:Panel ID="pnlAlert" runat="server" Visible="false">
                <div class="calib-alert">
                    <div class="alert-title">
                        <span class="glyphicon glyphicon-warning-sign"></span>
                        &nbsp;แจ้งเตือน: เครื่องมือวัดเกินกำหนด Calibration — กรุณาดำเนินการโดยด่วน
                    </div>
                    <ul>
                        <asp:Repeater ID="rptAlertList" runat="server">
                            <ItemTemplate>
                                <li>
                                    <strong><%# Eval("EquipCode") %></strong> —
                                    <%# Eval("EquipName") %>&nbsp;
                                    <span class="badge-overdue">
                                        เกิน <%# Math.Abs(Convert.ToInt32(Eval("DaysRemaining"))) %> วัน
                                    </span>
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </div>
            </asp:Panel>

            <!-- ===== FILTER BAR ===== -->
            <div class="filter-box">
                <div class="row">
                    <div class="col-md-3 col-sm-6">
                        <div class="form-group">
                            <label>ค้นหา (รหัส / ชื่อ)</label>
                            <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control input-sm"
                                placeholder="พิมพ์รหัสหรือชื่อเครื่องมือ..." />
                        </div>
                    </div>
                    <div class="col-md-2 col-sm-6">
                        <div class="form-group">
                            <label>ประเภท</label>
                            <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-control input-sm" />
                        </div>
                    </div>
                    <div class="col-md-2 col-sm-6">
                        <div class="form-group">
                            <label>สถานะ / แจ้งเตือน</label>
                            <asp:DropDownList ID="ddlAlertFilter" runat="server" CssClass="form-control input-sm">
                                <asp:ListItem Value=""        Text="ทั้งหมด" />
                                <asp:ListItem Value="Overdue" Text="เกินกำหนด" />
                                <asp:ListItem Value="DueSoon" Text="ครบกำหนดใน 30 วัน" />
                                <asp:ListItem Value="Sent"    Text="ส่ง Calibrate แล้ว" />
                                <asp:ListItem Value="Normal"  Text="ปกติ" />
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="col-md-2 col-sm-6">
                        <div class="form-group">
                            <label>&nbsp;</label><br />
                            <asp:Button ID="btnSearch" runat="server" Text="ค้นหา"
                                CssClass="btn btn-sm btn-primary" OnClick="btnSearch_Click" />
                            &nbsp;
                            <asp:Button ID="btnReset" runat="server" Text="ล้าง"
                                CssClass="btn btn-sm btn-default" OnClick="btnReset_Click" />
                        </div>
                    </div>
                    <div class="col-md-3 col-sm-6" style="text-align:right;">
                        <div class="form-group">
                            <label>&nbsp;</label><br />
                            <button type="button" class="btn btn-sm btn-success" onclick="openEquipModal(0)">
                                <span class="glyphicon glyphicon-plus"></span> เพิ่มเครื่องมือวัด
                            </button>
                        </div>
                    </div>
                </div>
            </div>

            <!-- ===== GRID ===== -->
            <div class="grid-wrap">
                <div class="grid-title">
                    รายการเครื่องมือวัด —
                    <asp:Label ID="lblRecCount" runat="server" Text="0" style="color:#2980b9;" /> รายการ
                    <span class="grid-legend">
                        <span style="color:#c0392b;">&#9632;</span> เกินกำหนด &nbsp;
                        <span style="color:#e67e22;">&#9632;</span> ครบกำหนดใน 30 วัน &nbsp;
                        <span style="color:#2980b9;">&#9632;</span> ส่ง Calibrate &nbsp;
                        <span style="color:#27ae60;">&#9632;</span> ปกติ
                    </span>
                </div>

                <asp:Label ID="lblGridMessage" runat="server" Visible="false"
                    CssClass="alert alert-success" style="display:block;margin-bottom:10px;" />

                <div style="overflow-x:auto;">
                    <asp:GridView ID="gvCalib" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-bordered table-hover"
                        OnRowDataBound="gvCalib_RowDataBound"
                        OnRowCommand="gvCalib_RowCommand"
                        DataKeyNames="CalibEquipID"
                        EmptyDataText="ไม่พบข้อมูลเครื่องมือวัด">
                        <EmptyDataRowStyle CssClass="text-center" ForeColor="Gray" />
                        <Columns>
                            <asp:TemplateField HeaderText="#" HeaderStyle-Width="36px"
                                ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="EquipCode" HeaderText="รหัส"
                                ItemStyle-Width="90px" ItemStyle-CssClass="text-nowrap" />

                            <asp:BoundField DataField="EquipName" HeaderText="ชื่อเครื่องมือวัด"
                                ItemStyle-Width="200px" />

                            <asp:BoundField DataField="SerialNo" HeaderText="Serial No."
                                ItemStyle-Width="110px" />

                            <asp:BoundField DataField="Category" HeaderText="ประเภท"
                                ItemStyle-Width="100px" />

                            <asp:BoundField DataField="Location" HeaderText="สถานที่"
                                ItemStyle-Width="90px" />

                            <asp:TemplateField HeaderText="รอบ (เดือน)"
                                HeaderStyle-Width="70px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate><%# Eval("CalibInterval") %> เดือน</ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Calibrate ล่าสุด"
                                HeaderStyle-Width="100px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <%# Eval("LastCalibDate") == DBNull.Value ? "-"
                                        : Convert.ToDateTime(Eval("LastCalibDate")).ToString("dd/MM/yyyy") %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Due Date"
                                HeaderStyle-Width="100px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <strong>
                                    <%# Eval("NextDueDate") == DBNull.Value ? "-"
                                        : Convert.ToDateTime(Eval("NextDueDate")).ToString("dd/MM/yyyy") %>
                                    </strong>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="คงเหลือ (วัน)"
                                HeaderStyle-Width="90px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <span class="<%# GetDaysClass(Eval("AlertLevel").ToString()) %>">
                                        <%# FormatDaysRemaining(Eval("DaysRemaining"), Eval("AlertLevel").ToString()) %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="สถานะ"
                                HeaderStyle-Width="120px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <span class="<%# GetBadgeClass(Eval("AlertLevel").ToString()) %>">
                                        <%# GetAlertText(Eval("AlertLevel").ToString()) %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="การดำเนินการ"
                                HeaderStyle-Width="190px" ItemStyle-HorizontalAlign="Center" ItemStyle-CssClass="text-nowrap">
                                <ItemTemplate>
                                    <%-- ส่ง Calib: แสดงเฉพาะ Active --%>
                                    <asp:PlaceHolder runat="server"
                                        Visible='<%# Eval("CurrentStatus").ToString() == "Active" %>'>
                                        <button type="button" class="btn btn-xs btn-primary btn-send"
                                            data-id='<%# Eval("CalibEquipID") %>'
                                            data-code='<%# JsAttr(Eval("EquipCode")) %>'
                                            data-name='<%# JsAttr(Eval("EquipName")) %>'>
                                            <span class="glyphicon glyphicon-send"></span> ส่ง Calib
                                        </button>
                                    </asp:PlaceHolder>

                                    <%-- รับคืน: แสดงเฉพาะ Sent --%>
                                    <asp:PlaceHolder runat="server"
                                        Visible='<%# Eval("CurrentStatus").ToString() == "Sent" %>'>
                                        <button type="button" class="btn btn-xs btn-success btn-return"
                                            data-id='<%# Eval("CalibEquipID") %>'
                                            data-code='<%# JsAttr(Eval("EquipCode")) %>'
                                            data-name='<%# JsAttr(Eval("EquipName")) %>'
                                            data-interval='<%# Eval("CalibInterval") %>'>
                                            <span class="glyphicon glyphicon-ok"></span> รับคืน
                                        </button>
                                    </asp:PlaceHolder>

                                    &nbsp;
                                    <asp:LinkButton runat="server" CssClass="btn btn-xs btn-default"
                                        CommandName="OpenHistory"
                                        CommandArgument='<%# Eval("CalibEquipID") + "|" + JsAttr(Eval("EquipCode")) + "|" + JsAttr(Eval("EquipName")) %>'>
                                        <span class="glyphicon glyphicon-list-alt"></span> ประวัติ
                                    </asp:LinkButton>
                                    &nbsp;
                                    <asp:LinkButton runat="server" CssClass="btn btn-xs btn-warning"
                                        CommandName="OpenEdit"
                                        CommandArgument='<%# Eval("CalibEquipID") %>'
                                        ToolTip="แก้ไขข้อมูล">
                                        <span class="glyphicon glyphicon-pencil"></span>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>

        </div><%-- /container-fluid --%>

        <!-- ============================================================ -->
        <!-- MODAL: นำส่ง Calibration                                     -->
        <!-- ============================================================ -->
        <div id="modalSend" class="modal fade" tabindex="-1" role="dialog">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                        <h4 class="modal-title">
                            <span class="glyphicon glyphicon-send"></span> นำส่ง Calibration
                        </h4>
                    </div>
                    <div class="modal-body">
                        <div class="alert alert-info" style="padding:8px 12px;font-size:13px;">
                            <span class="glyphicon glyphicon-info-sign"></span>
                            &nbsp;<strong id="lblSendEquipInfo">-</strong>
                        </div>
                        <div class="row">
                            <div class="col-sm-6">
                                <div class="form-group">
                                    <label>วันที่นำส่ง <span style="color:red;">*</span></label>
                                    <asp:TextBox ID="txtSendDate" runat="server"
                                        CssClass="form-control input-sm" TextMode="Date" />
                                </div>
                            </div>
                            <div class="col-sm-6">
                                <div class="form-group">
                                    <label>ผู้นำส่ง <span style="color:red;">*</span></label>
                                    <asp:TextBox ID="txtSendBy" runat="server"
                                        CssClass="form-control input-sm"
                                        placeholder="ชื่อผู้นำส่ง" MaxLength="100" />
                                </div>
                            </div>
                        </div>
                        <div class="form-group">
                            <label>บริษัท / ห้องปฏิบัติการที่รับ Calibrate <span style="color:red;">*</span></label>
                            <asp:TextBox ID="txtSendVendor" runat="server"
                                CssClass="form-control input-sm"
                                placeholder="เช่น Bureau Veritas, SGS, NIMT" MaxLength="200" />
                        </div>
                        <div class="form-group">
                            <label>หมายเหตุ</label>
                            <asp:TextBox ID="txtSendRemark" runat="server"
                                CssClass="form-control input-sm" TextMode="MultiLine" Rows="2"
                                placeholder="หมายเหตุเพิ่มเติม (ถ้ามี)" MaxLength="500" />
                        </div>
                        <asp:Label ID="lblSendError" runat="server" CssClass="text-danger"
                            Visible="false" style="font-size:13px;" />
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">ยกเลิก</button>
                        <asp:Button ID="btnConfirmSend" runat="server" Text="ยืนยันนำส่ง"
                            CssClass="btn btn-primary" OnClick="btnConfirmSend_Click"
                            OnClientClick="return validateSend();" />
                    </div>
                </div>
            </div>
        </div>

        <!-- ============================================================ -->
        <!-- MODAL: รับคืนจาก Calibration                                 -->
        <!-- ============================================================ -->
        <div id="modalReturn" class="modal fade" tabindex="-1" role="dialog">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                        <h4 class="modal-title">
                            <span class="glyphicon glyphicon-ok-circle"></span> รับคืนจาก Calibration
                        </h4>
                    </div>
                    <div class="modal-body">
                        <div class="alert alert-info" style="padding:8px 12px;font-size:13px;">
                            <span class="glyphicon glyphicon-info-sign"></span>
                            &nbsp;<strong id="lblReturnEquipInfo">-</strong>
                        </div>
                        <div class="row">
                            <div class="col-sm-6">
                                <div class="form-group">
                                    <label>วันที่รับคืน <span style="color:red;">*</span></label>
                                    <asp:TextBox ID="txtReturnDate" runat="server"
                                        CssClass="form-control input-sm" TextMode="Date"
                                        ClientIDMode="Static" />
                                </div>
                            </div>
                            <div class="col-sm-6">
                                <div class="form-group">
                                    <label>Due Date ถัดไป (คำนวณอัตโนมัติ)</label>
                                    <div class="new-due-preview" id="txtNewDuePreview">—</div>
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-sm-6">
                                <div class="form-group">
                                    <label>ผู้รับคืน <span style="color:red;">*</span></label>
                                    <asp:TextBox ID="txtReturnBy" runat="server"
                                        CssClass="form-control input-sm"
                                        placeholder="ชื่อผู้รับคืน" MaxLength="100" />
                                </div>
                            </div>
                            <div class="col-sm-6">
                                <div class="form-group">
                                    <label>เลขที่ใบรับรอง (Cert. No.) <span style="color:red;">*</span></label>
                                    <asp:TextBox ID="txtReturnCertNo" runat="server"
                                        CssClass="form-control input-sm"
                                        placeholder="CERT-2025-XXXX" MaxLength="100" />
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-sm-4">
                                <div class="form-group">
                                    <label>ผลการ Calibrate <span style="color:red;">*</span></label>
                                    <asp:DropDownList ID="ddlReturnResult" runat="server"
                                        CssClass="form-control input-sm">
                                        <asp:ListItem Value="Pass" Text="Pass &#10003;" />
                                        <asp:ListItem Value="Fail" Text="Fail &#10007;" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-sm-8">
                                <div class="form-group">
                                    <label>หมายเหตุ</label>
                                    <asp:TextBox ID="txtReturnRemark" runat="server"
                                        CssClass="form-control input-sm"
                                        placeholder="หมายเหตุ (ถ้ามี)" MaxLength="500" />
                                </div>
                            </div>
                        </div>
                        <asp:Label ID="lblReturnError" runat="server" CssClass="text-danger"
                            Visible="false" style="font-size:13px;" />
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">ยกเลิก</button>
                        <asp:Button ID="btnConfirmReturn" runat="server" Text="ยืนยันรับคืน"
                            CssClass="btn btn-success" OnClick="btnConfirmReturn_Click"
                            OnClientClick="return validateReturn();" />
                    </div>
                </div>
            </div>
        </div>


        <!-- ============================================================ -->
        <!-- MODAL: ประวัติ Calibration                                    -->
        <!-- ============================================================ -->
        <div id="modalHistory" class="modal fade" tabindex="-1" role="dialog">
            <div class="modal-dialog modal-lg" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                        <h4 class="modal-title">
                            <span class="glyphicon glyphicon-list-alt"></span>
                            ประวัติ Calibration — <span id="lblHistEquipName" style="font-size:14px;"></span>
                        </h4>
                    </div>
                    <div class="modal-body" style="max-height:65vh;overflow-y:auto;">
                        <asp:GridView ID="gvHistory" runat="server"
                            AutoGenerateColumns="false"
                            CssClass="table table-bordered table-condensed"
                            EmptyDataText="ยังไม่มีประวัติ Calibration">
                            <Columns>
                                <asp:BoundField DataField="ActionDateStr" HeaderText="วันที่"
                                    ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="90px" />
                                <asp:TemplateField HeaderText="การดำเนินการ"
                                    HeaderStyle-Width="80px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <span class='<%# Eval("Action").ToString()=="Send" ? "label label-primary" : "label label-success" %>'>
                                            <%# Eval("Action").ToString()=="Send" ? "นำส่ง" : "รับคืน" %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="ActionBy"  HeaderText="โดย"
                                    HeaderStyle-Width="110px" />
                                <asp:BoundField DataField="Vendor"    HeaderText="บริษัท Calib"
                                    HeaderStyle-Width="150px" />
                                <asp:BoundField DataField="CertNo"    HeaderText="Cert. No."
                                    HeaderStyle-Width="110px" />
                                <asp:TemplateField HeaderText="ผล"
                                    HeaderStyle-Width="55px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <%# Eval("CalibResult").ToString()=="Pass"
                                            ? "<span class='label label-success'>Pass</span>"
                                            : Eval("CalibResult").ToString()=="Fail"
                                            ? "<span class='label label-danger'>Fail</span>"
                                            : "<span style='color:#aaa;'>—</span>" %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Due Date ใหม่"
                                    HeaderStyle-Width="100px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <%# Eval("NewDueDate") == DBNull.Value ? "—"
                                            : Convert.ToDateTime(Eval("NewDueDate")).ToString("dd/MM/yyyy") %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="Remark" HeaderText="หมายเหตุ" />
                            </Columns>
                        </asp:GridView>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">ปิด</button>
                    </div>
                </div>
            </div>
        </div>

        <!-- ============================================================ -->
        <!-- MODAL: เพิ่ม / แก้ไข เครื่องมือวัด                           -->
        <!-- ============================================================ -->
        <div id="modalEquip" class="modal fade" tabindex="-1" role="dialog">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                        <h4 class="modal-title" id="modalEquipTitle">
                            <span class="glyphicon glyphicon-plus"></span> เพิ่มเครื่องมือวัด
                        </h4>
                    </div>
                    <div class="modal-body">
                        <div class="row">
                            <div class="col-sm-6">
                                <div class="form-group">
                                    <label>รหัสเครื่องมือ <span style="color:red;">*</span></label>
                                    <asp:TextBox ID="txtEditCode" runat="server"
                                        CssClass="form-control input-sm" placeholder="ME-001" MaxLength="50" />
                                </div>
                            </div>
                            <div class="col-sm-6">
                                <div class="form-group">
                                    <label>Serial No.</label>
                                    <asp:TextBox ID="txtEditSerial" runat="server"
                                        CssClass="form-control input-sm" placeholder="SN-XXXX" MaxLength="100" />
                                </div>
                            </div>
                        </div>
                        <div class="form-group">
                            <label>ชื่อเครื่องมือวัด <span style="color:red;">*</span></label>
                            <asp:TextBox ID="txtEditName" runat="server"
                                CssClass="form-control input-sm"
                                placeholder="ชื่อเครื่องมือวัด เช่น Vernier Caliper 0-150mm" MaxLength="200" />
                        </div>
                        <div class="row">
                            <div class="col-sm-4">
                                <div class="form-group">
                                    <label>ประเภท</label>
                                    <asp:TextBox ID="txtEditCategory" runat="server"
                                        CssClass="form-control input-sm"
                                        placeholder="เช่น Dimension, Torque" MaxLength="100" />
                                </div>
                            </div>
                            <div class="col-sm-4">
                                <div class="form-group">
                                    <label>สถานที่เก็บ</label>
                                    <asp:TextBox ID="txtEditLocation" runat="server"
                                        CssClass="form-control input-sm"
                                        placeholder="เช่น QC Lab, Production A" MaxLength="100" />
                                </div>
                            </div>
                            <div class="col-sm-4">
                                <div class="form-group">
                                    <label>รอบการ Calibrate <span style="color:red;">*</span></label>
                                    <asp:DropDownList ID="ddlEditInterval" runat="server"
                                        CssClass="form-control input-sm">
                                        <asp:ListItem Value="3"  Text="3 เดือน" />
                                        <asp:ListItem Value="6"  Text="6 เดือน" />
                                        <asp:ListItem Value="12" Text="12 เดือน (1 ปี)" Selected="True" />
                                        <asp:ListItem Value="24" Text="24 เดือน (2 ปี)" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                        </div>
                        <div class="form-group">
                            <label>วันที่ Calibrate ล่าสุด</label>
                            <asp:TextBox ID="txtEditLastCalib" runat="server"
                                CssClass="form-control input-sm" TextMode="Date" />
                            <span class="help-block" style="font-size:11px;color:#888;">
                                ระบบจะคำนวณ Due Date ใหม่โดยอัตโนมัติจากวันนี้ + รอบ Calibrate
                            </span>
                        </div>
                        <asp:Label ID="lblEquipError" runat="server" CssClass="text-danger"
                            Visible="false" style="font-size:13px;" />
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">ยกเลิก</button>
                        <asp:Button ID="btnSaveEquip" runat="server" Text="บันทึก"
                            CssClass="btn btn-success" OnClick="btnSaveEquip_Click"
                            OnClientClick="return validateEquip();" />
                    </div>
                </div>
            </div>
        </div>


        <!-- ============================================================ -->
        <!-- Scripts                                                       -->
        <!-- ============================================================ -->
        <script src="~/Scripts/jquery-3.7.1.min.js"></script>
        <script src="~/Scripts/bootstrap.min.js"></script>

        <script type="text/javascript">
            /* ---- ปุ่ม [ส่ง Calib] ในแถวตาราง ---- */
            $(document).on('click', '.btn-send', function () {
                var id = $(this).data('id');
                var code = $(this).data('code');
                var name = $(this).data('name');
                document.getElementById('hfSendID').value = id;
                document.getElementById('lblSendEquipInfo').textContent = code + '  —  ' + name;
                // ตั้งวันที่เป็นวันนี้
                document.getElementById('<%=txtSendDate.ClientID%>').value = getTodayISO();
                document.getElementById('<%=txtSendBy.ClientID%>').value = '';
                document.getElementById('<%=txtSendVendor.ClientID%>').value = '';
                document.getElementById('<%=txtSendRemark.ClientID%>').value = '';
                document.getElementById('<%=lblSendError.ClientID%>').style.display = 'none';
                $('#modalSend').modal('show');
            });
            /* ---- ปุ่ม [รับคืน] ในแถวตาราง ---- */
            $(document).on('click', '.btn-return', function () {
                var id = $(this).data('id');
                var code = $(this).data('code');
                var name = $(this).data('name');
                var interval = parseInt($(this).data('interval')) || 12;
                document.getElementById('hfReturnID').value = id;
                document.getElementById('hfReturnInterval').value = interval;
                document.getElementById('lblReturnEquipInfo').textContent = code + '  —  ' + name;
                document.getElementById('<%=txtReturnDate.ClientID%>').value = getTodayISO();
                document.getElementById('<%=txtReturnBy.ClientID%>').value = '';
                document.getElementById('<%=txtReturnCertNo.ClientID%>').value = '';
                document.getElementById('<%=txtReturnRemark.ClientID%>').value = '';
                document.getElementById('<%=lblReturnError.ClientID%>').style.display = 'none';
                calcNewDueDate();
                $('#modalReturn').modal('show');
            });
            /* ---- คำนวณ Due Date ใหม่เมื่อเปลี่ยนวันที่รับคืน ---- */
            document.addEventListener('DOMContentLoaded', function () {
                var rdInput = document.getElementById('txtReturnDate');
                if (rdInput) rdInput.addEventListener('change', calcNewDueDate);
            });
            function calcNewDueDate() {
                var retDateStr = document.getElementById('txtReturnDate').value;
                var interval = parseInt(document.getElementById('hfReturnInterval').value) || 12;
                var preview = document.getElementById('txtNewDuePreview');
                if (!retDateStr) { preview.textContent = '—'; return; }
                var d = new Date(retDateStr);
                d.setMonth(d.getMonth() + interval);
                var dd = ('0' + d.getDate()).slice(-2);
                var mm = ('0' + (d.getMonth() + 1)).slice(-2);
                var yyyy = d.getFullYear();
                preview.textContent = dd + '/' + mm + '/' + yyyy + '  (รอบ ' + interval + ' เดือน)';
            }
            /* ---- Modal ประวัติ (เรียกจาก RegisterStartupScript) ---- */
            function showHistoryModal(equipName) {
                document.getElementById('lblHistEquipName').textContent = equipName;
                $('#modalHistory').modal('show');
            }
            /* ---- Modal เพิ่ม/แก้ไขเครื่องมือ ---- */
            function openEquipModal(calibEquipID) {
                document.getElementById('hfEditID').value = calibEquipID;
                if (calibEquipID == 0) {
                    document.getElementById('modalEquipTitle').innerHTML =
                        '<span class="glyphicon glyphicon-plus"></span> เพิ่มเครื่องมือวัด';
                    document.getElementById('<%=txtEditCode.ClientID%>').value = '';
                    document.getElementById('<%=txtEditName.ClientID%>').value = '';
                    document.getElementById('<%=txtEditSerial.ClientID%>').value = '';
                    document.getElementById('<%=txtEditCategory.ClientID%>').value = '';
                    document.getElementById('<%=txtEditLocation.ClientID%>').value = '';
                    document.getElementById('<%=txtEditLastCalib.ClientID%>').value = '';
                    document.getElementById('<%=ddlEditInterval.ClientID%>').value = '12';
                } else {
                    document.getElementById('modalEquipTitle').innerHTML =
                        '<span class="glyphicon glyphicon-pencil"></span> แก้ไขเครื่องมือวัด';
                }
                document.getElementById('<%=lblEquipError.ClientID%>').style.display = 'none';
                $('#modalEquip').modal('show');
            }
            /* ---- คลิก Summary Card เพื่อ Filter ---- */
            function filterByAlert(level) {
                document.getElementById('<%=ddlAlertFilter.ClientID%>').value = level;
                document.getElementById('<%=btnSearch.ClientID%>').click();
            }
            /* ---- Toast Notification ---- */
            function showToast(msg, isSuccess) {
                var t = document.getElementById('toastBar');
                t.className = 'toast-bar ' + (isSuccess ? 'toast-success' : 'toast-error');
                t.textContent = msg;
                t.style.display = 'block';
                setTimeout(function () { t.style.display = 'none'; }, 4500);
            }
            /* ---- Validation ---- */
            function validateSend() {
                var d = document.getElementById('<%=txtSendDate.ClientID%>').value;
                var b = document.getElementById('<%=txtSendBy.ClientID%>').value.trim();
                var v = document.getElementById('<%=txtSendVendor.ClientID%>').value.trim();
                if (!d || !b || !v) {
                    alert('กรุณากรอกข้อมูลที่จำเป็น:\n- วันที่นำส่ง\n- ผู้นำส่ง\n- บริษัท / ห้องปฏิบัติการ');
                    return false;
                }
                return true;
            }
            function validateReturn() {
                var d = document.getElementById('txtReturnDate').value;
                var b = document.getElementById('<%=txtReturnBy.ClientID%>').value.trim();
                var c = document.getElementById('<%=txtReturnCertNo.ClientID%>').value.trim();
                if (!d || !b || !c) {
                    alert('กรุณากรอกข้อมูลที่จำเป็น:\n- วันที่รับคืน\n- ผู้รับคืน\n- เลขที่ใบรับรอง (Cert. No.)');
                    return false;
                }
                return true;
            }
            function validateEquip() {
                var code = document.getElementById('<%=txtEditCode.ClientID%>').value.trim();
                var name = document.getElementById('<%=txtEditName.ClientID%>').value.trim();
                if (!code || !name) {
                    alert('กรุณากรอก รหัสเครื่องมือ และ ชื่อเครื่องมือ');
                    return false;
                }
                return true;
            }
            /* ---- Utils ---- */
            function getTodayISO() {
                var d = new Date();
                return d.getFullYear() + '-' +
                    ('0' + (d.getMonth() + 1)).slice(-2) + '-' +
                    ('0' + d.getDate()).slice(-2);
            }
        </script>

        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
