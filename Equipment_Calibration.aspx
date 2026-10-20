<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="Equipment_Calibration.aspx.cs"
    Inherits="Tool_Equipment_Control_System.Equipment_Calibration" %>

<asp:Content ID="Calibration_Home" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <link href="css/bootstrap_sum.css" rel="stylesheet">
    <link href="CSS/w3v4.10.css" rel="stylesheet" />
    <link href="css/styles.css" rel="stylesheet">

    <style>
        .cal { --ink:#1c2b3a; --muted:#66727f; --line:#e3e8ee; color:var(--ink); font-size:13px; margin:30px 14px 40px; }
        .cal-header { display:flex; align-items:center; gap:12px; flex-wrap:wrap; margin-bottom:14px; }
        .cal-header h1 { margin:0; font-size:22px; font-weight:700; }
        .cal-muted { color:var(--muted); }

        /* แจ้งเตือนพิเศษ < 3 วัน */
        .cal-alert { background:#fdecec; border:2px solid #c93b3b; border-left:8px solid #c93b3b; border-radius:10px;
                     padding:12px 16px; margin-bottom:14px; }
        .cal-alert.pulse { animation:calPulse 1.2s ease-in-out 3; }
        @keyframes calPulse { 0%,100% { box-shadow:0 0 0 0 rgba(201,59,59,.0); } 50% { box-shadow:0 0 0 8px rgba(201,59,59,.25); } }
        .cal-alert-title { font-weight:800; color:#a32020; font-size:14.5px; margin-bottom:6px; display:flex; align-items:center; gap:8px; }
        .cal-alert ul { margin:0; padding-left:20px; max-height:200px; overflow-y:auto; }
        .cal-alert li { margin:3px 0; }

        /* ปุ่มกรอง (การ์ด) */
        .cal-chips { display:grid; grid-template-columns:repeat(auto-fit, minmax(170px, 1fr)); gap:10px; margin-bottom:14px; }
        .cal-chip { display:block; background:#fff; border:1px solid var(--line); border-left:5px solid #9aa5b1; border-radius:10px;
                    padding:10px 12px; color:var(--ink) !important; text-decoration:none !important; transition:box-shadow .15s, transform .15s; }
        .cal-chip:hover { box-shadow:0 3px 10px rgba(0,0,0,.08); transform:translateY(-1px); }
        .cal-chip.active { box-shadow:0 0 0 2px #1a3a5c inset; background:#f4f8fd; }
        .cal-chip .c-label { font-size:12px; font-weight:700; color:var(--muted); }
        .cal-chip .c-val { font-size:26px; font-weight:800; line-height:1.2; }
        .chip-all { border-left-color:#1a3a5c; } .chip-over { border-left-color:#c93b3b; } .chip-over .c-val { color:#c93b3b; }
        .chip-alert { border-left-color:#e0442f; } .chip-alert .c-val { color:#e0442f; }
        .chip-week { border-left-color:#e0782f; } .chip-week .c-val { color:#e0782f; }
        .chip-month { border-left-color:#d99a06; } .chip-month .c-val { color:#b07c00; }
        .chip-nodate { border-left-color:#9aa5b1; }

        .cal-filter { display:flex; gap:12px; align-items:flex-end; flex-wrap:wrap; background:#fff; border:1px solid var(--line);
                      border-radius:10px; padding:10px 14px; margin-bottom:14px; }
        .cal-filter label { display:block; font-size:12px; color:var(--muted); margin-bottom:3px; font-weight:600; }

        /* ตาราง */
        .cal-card { background:#fff; border:1px solid var(--line); border-radius:10px; padding:12px 14px; }
        .cal-card-title { font-weight:700; font-size:14px; margin-bottom:8px; }
        .cal-table { width:100%; border-collapse:collapse; min-width:1300px; }
        .cal-table th { background:#33475b; color:#fff; padding:7px 8px; text-align:center; font-size:12.5px; white-space:nowrap; font-weight:600; }
        .cal-table td { padding:6px 8px; border-bottom:1px solid var(--line); text-align:center; vertical-align:middle; }
        .cal-table td.l { text-align:left; }
        .cal-table tr.row-over td { background:#fdecec; }
        .cal-table tr.row-alert td { background:#fff1ec; }
        .cal-table tr.row-week td { background:#fff8e6; }
        .cal-table tr.row-nodate td { color:var(--muted); }
        .cal-table .pager td { border:0; }
        .cal-table .pager table td { padding:2px 6px; }

        .cal-tag { display:inline-block; padding:2px 9px; border-radius:12px; font-size:11.5px; font-weight:700; white-space:nowrap; }
        .tag-over { background:#c93b3b; color:#fff; }
        .tag-alert { background:#e0442f; color:#fff; }
        .tag-week { background:#fde3cf; color:#9a4a0c; }
        .tag-month { background:#fdf0c8; color:#7a5a00; }
        .tag-ok { background:#e3f4ea; color:#16884a; }
        .tag-nodate { background:#eef1f5; color:#66727f; }
    </style>

    <script>
        function calPulse() {
            var a = document.getElementById('calAlertBox');
            if (!a) return;
            a.classList.remove('pulse'); void a.offsetWidth; a.classList.add('pulse');
            a.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    </script>

    <asp:ScriptManager runat="server" ID="scriptManager"></asp:ScriptManager>

    <asp:UpdatePanel runat="server" ID="UP1">
        <ContentTemplate>
            <div class="cal">
                <div class="cal-header">
                    <h1><span class="glyphicon glyphicon-scale"></span>&nbsp;Equipment : เครื่องมือวัดที่ต้องสอบเทียบ</h1>
                    <span class="cal-muted">Category = Measurement · ไม่รวม Transfer / Scrap · ข้อมูล ณ <%= DateTime.Now.ToString("dd-MMM-yyyy HH:mm", System.Globalization.CultureInfo.GetCultureInfo("en-US")) %></span>
                </div>

                <%-- แจ้งเตือนพิเศษ : < 3 วัน --%>
                <asp:Panel ID="pnlAlert" runat="server" Visible="false">
                    <div id="calAlertBox" class="cal-alert">
                        <div class="cal-alert-title">
                            <span class="glyphicon glyphicon-warning-sign"></span>
                            <asp:Literal ID="litAlertTitle" runat="server" />
                        </div>
                        <ul><asp:Literal ID="litAlertList" runat="server" /></ul>
                    </div>
                </asp:Panel>

                <%-- ปุ่มกรอง --%>
                <div class="cal-chips">
                    <asp:LinkButton ID="chipAll" runat="server" CssClass="cal-chip chip-all" CommandArgument="all" OnClick="btnFilter_Click">
                        <div class="c-label"><span class="glyphicon glyphicon-list"></span> เครื่องมือวัดทั้งหมด</div>
                        <div class="c-val"><asp:Label ID="lblCntAll" runat="server" Text="0" /></div>
                    </asp:LinkButton>
                    <asp:LinkButton ID="chipAlert" runat="server" CssClass="cal-chip chip-alert" CommandArgument="alert" OnClick="btnFilter_Click">
                        <div class="c-label"><span class="glyphicon glyphicon-fire"></span> แจ้งเตือน &lt; 3 วัน</div>
                        <div class="c-val"><asp:Label ID="lblCntAlert" runat="server" Text="0" /></div>
                    </asp:LinkButton>
                    <asp:LinkButton ID="chipOver" runat="server" CssClass="cal-chip chip-over" CommandArgument="overdue" OnClick="btnFilter_Click">
                        <div class="c-label"><span class="glyphicon glyphicon-exclamation-sign"></span> เกินกำหนด</div>
                        <div class="c-val"><asp:Label ID="lblCntOver" runat="server" Text="0" /></div>
                    </asp:LinkButton>
                    <asp:LinkButton ID="chipWeek" runat="server" CssClass="cal-chip chip-week" CommandArgument="d7" OnClick="btnFilter_Click">
                        <div class="c-label"><span class="glyphicon glyphicon-bell"></span> &lt; 7 วัน</div>
                        <div class="c-val"><asp:Label ID="lblCntWeek" runat="server" Text="0" /></div>
                    </asp:LinkButton>
                    <asp:LinkButton ID="chipMonth" runat="server" CssClass="cal-chip chip-month" CommandArgument="m1" OnClick="btnFilter_Click">
                        <div class="c-label"><span class="glyphicon glyphicon-calendar"></span> &lt; 1 เดือน</div>
                        <div class="c-val"><asp:Label ID="lblCntMonth" runat="server" Text="0" /></div>
                    </asp:LinkButton>
                    <asp:LinkButton ID="chipNoDate" runat="server" CssClass="cal-chip chip-nodate" CommandArgument="nodate" OnClick="btnFilter_Click">
                        <div class="c-label"><span class="glyphicon glyphicon-question-sign"></span> ไม่มี Due Date</div>
                        <div class="c-val"><asp:Label ID="lblCntNoDate" runat="server" Text="0" /></div>
                    </asp:LinkButton>
                </div>

                <%-- ค้นหา --%>
                <asp:Panel ID="pnlFilter" runat="server" DefaultButton="btnSearch" CssClass="cal-filter">
                    <div>
                        <label>ค้นหา (ID / Fix Asset / Control No / Name / Tool No / Serial)</label>
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control input-sm" Width="320px" />
                    </div>
                    <div>
                        <label>Dept</label>
                        <asp:DropDownList ID="ddlDept" runat="server" CssClass="form-control input-sm" Width="220px" />
                    </div>
                    <div>
                        <asp:Button ID="btnSearch" runat="server" Text="ค้นหา" CssClass="btn btn-sm btn-primary" OnClick="btnSearch_Click" />
                        <asp:Button ID="btnReset" runat="server" Text="ล้าง" CssClass="btn btn-sm btn-default" OnClick="btnReset_Click" />
                    </div>
                </asp:Panel>

                <div class="cal-card">
                    <div class="cal-card-title">
                        รายการ : <asp:Literal ID="litFilterName" runat="server" /> —
                        <asp:Label ID="lblRecCount" runat="server" Text="0" style="color:#1f6feb;" /> รายการ
                        <span class="cal-muted" style="font-weight:400; font-size:12px;">(เรียงตาม Due Date ใกล้สุดก่อน)</span>
                    </div>
                    <div style="overflow-x:auto;">
                        <asp:GridView ID="gvCalib" runat="server" CssClass="cal-table" GridLines="None"
                            AutoGenerateColumns="false" AllowPaging="true" PageSize="50"
                            OnPageIndexChanging="gvCalib_PageIndexChanging" OnRowDataBound="gvCalib_RowDataBound"
                            EmptyDataText="ไม่พบรายการ">
                            <PagerStyle CssClass="pager" HorizontalAlign="Center" />
                            <Columns>
                                <asp:TemplateField HeaderText="#" ItemStyle-Width="40px">
                                    <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="สถานะสอบเทียบ" ItemStyle-Width="140px">
                                    <ItemTemplate><asp:Literal ID="litDays" runat="server" /></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Due Date" ItemStyle-Width="110px">
                                    <ItemTemplate><asp:Literal ID="litDue" runat="server" /></ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="ID" HeaderText="ID" />
                                <asp:BoundField DataField="FixAcces" HeaderText="Fix Asset" />
                                <asp:BoundField DataField="control_no" HeaderText="Control No" />
                                <asp:BoundField DataField="asset_name" HeaderText="Name" ItemStyle-CssClass="l" />
                                <asp:BoundField DataField="tool_no" HeaderText="Tool No / Model" ItemStyle-CssClass="l" />
                                <asp:BoundField DataField="serial_no" HeaderText="Serial No" />
                                <asp:BoundField DataField="dept_code" HeaderText="Dept" />
                                <asp:BoundField DataField="model_name" HeaderText="CPT Model" />
                                <asp:BoundField DataField="cell" HeaderText="Cell" />
                                <asp:BoundField DataField="station" HeaderText="Station" />
                                <asp:BoundField DataField="responsible" HeaderText="Responsible" ItemStyle-CssClass="l" />
                                <asp:BoundField DataField="status" HeaderText="Status" />
                                <asp:TemplateField HeaderText="Detail" ItemStyle-Width="60px">
                                    <ItemTemplate>
                                        <asp:HyperLink ID="lnkDetail" runat="server" Target="_blank" ToolTip="ดูรายละเอียด">
                                            <img src="images/eye-tracking.png" alt="" style="width:22px;" />
                                        </asp:HyperLink>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
