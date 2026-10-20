<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="Dashboard.aspx.cs"
    Inherits="Tool_Equipment_Control_System.Dashboard" %>

<asp:Content ID="Dashboard_Home" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <link href="css/bootstrap_sum.css" rel="stylesheet">
    <link href="CSS/w3v4.10.css" rel="stylesheet" />
    <link href="css/styles.css" rel="stylesheet">

    <style>
        .db { --ink:#1c2b3a; --muted:#66727f; --line:#e3e8ee; --bg:#f5f7fa; color:var(--ink); font-size:13px; }
        .db-wrap { margin:30px 14px 40px; }
        .db-header { display:flex; align-items:center; gap:12px; flex-wrap:wrap; margin-bottom:14px; }
        .db-header h1 { margin:0; font-size:22px; font-weight:700; }
        .db-header .db-sub { color:var(--muted); font-size:12px; }

        /* Tabs */
        .db-tabs { display:flex; gap:4px; border-bottom:2px solid #1a3a5c; margin-bottom:16px; }
        .db-tab { border:0; background:#e8edf3; color:#33475b; padding:9px 18px; border-radius:8px 8px 0 0;
                  font-weight:700; cursor:pointer; font-size:13.5px; }
        .db-tab.active { background:#1a3a5c; color:#fff; }
        .db-panel { display:none; }
        .db-panel.active { display:block; }

        /* Filter */
        .db-filter { display:flex; gap:14px; align-items:flex-end; flex-wrap:wrap; background:#fff; border:1px solid var(--line);
                     border-radius:10px; padding:10px 14px; margin-bottom:14px; }
        .db-filter label { display:block; font-size:12px; color:var(--muted); margin-bottom:3px; font-weight:600; }
        .db-filter select { min-width:200px; height:32px; }

        /* KPI */
        .kpi-row { display:grid; grid-template-columns:repeat(auto-fit, minmax(220px, 1fr)); gap:12px; margin-bottom:14px; }
        .kpi { position:relative; background:#fff; border:1px solid var(--line); border-left:5px solid #1f6feb; border-radius:10px; padding:12px 14px; }
        .kpi-label { font-size:12px; color:var(--muted); font-weight:600; }
        .kpi-value { font-size:26px; font-weight:800; line-height:1.25; }
        .kpi-sub { font-size:11.5px; color:var(--muted); }
        .kpi-ico { position:absolute; right:12px; top:12px; font-size:22px; opacity:.18; }
        .kpi-blue { border-left-color:#1f6feb; } .kpi-green { border-left-color:#16884a; }
        .kpi-amber { border-left-color:#d99a06; } .kpi-gray { border-left-color:#9aa5b1; }

        /* Card */
        .db-grid2 { display:grid; grid-template-columns:1fr 1fr; gap:14px; }
        @media (max-width: 1100px) { .db-grid2 { grid-template-columns:1fr; } }
        .db-card { background:#fff; border:1px solid var(--line); border-radius:10px; padding:12px 14px; margin-bottom:14px; }
        .db-card h3 { font-size:15px; font-weight:700; margin:0 0 10px; display:flex; align-items:center; gap:8px; }
        .db-card h3 small { font-weight:400; color:var(--muted); font-size:12px; }

        /* Table */
        .db-table { width:100%; border-collapse:collapse; }
        .db-table th { background:#33475b; color:#fff; font-weight:600; padding:7px 8px; text-align:center; font-size:12.5px; white-space:nowrap; }
        .db-table td { padding:6px 8px; border-bottom:1px solid var(--line); text-align:center; vertical-align:middle; }
        .db-table .l { text-align:left; }
        .db-table tr.db-total td { background:#f1f4f8; font-weight:700; border-top:2px solid #c9d2dc; }
        .db-dot { display:inline-block; width:10px; height:10px; border-radius:50%; margin-right:6px; }
        .db-muted { color:var(--muted); }
        .db-empty { color:var(--muted); text-align:center; padding:18px; }

        /* Bars */
        .db-bar { position:relative; height:20px; background:#eef1f5; border-radius:5px; overflow:hidden; min-width:80px; flex:1; }
        .db-bar-fill { height:100%; border-radius:5px; }
        .db-bar-txt { position:absolute; left:8px; top:1px; font-size:11.5px; font-weight:700; color:#1c2b3a; text-shadow:0 0 3px #fff; }
        .db-stack { display:flex; height:20px; border-radius:5px; overflow:hidden; min-width:120px; }
        .db-stack div { color:#fff; font-size:11px; font-weight:700; text-align:center; line-height:20px; }
        .db-hrow { display:flex; align-items:center; gap:10px; margin-bottom:7px; }
        .db-hlabel { width:150px; flex-shrink:0; font-weight:600; font-size:12.5px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
        .db-hsub { width:110px; flex-shrink:0; text-align:right; color:var(--muted); font-size:12px; }
        .db-legend { display:flex; gap:14px; flex-wrap:wrap; margin-top:8px; font-size:12px; color:var(--muted); }
        .db-legend i { display:inline-block; width:12px; height:12px; border-radius:3px; margin-right:5px; vertical-align:-1px; }

        /* Request cards */
        .rq-row { display:grid; grid-template-columns:repeat(auto-fit, minmax(240px, 1fr)); gap:12px; margin-bottom:14px; }
        .rq-card { background:#fff; border:1px solid var(--line); border-top:5px solid #1f6feb; border-radius:10px; padding:12px 14px; }
        .rq-head { display:flex; align-items:center; gap:8px; margin-bottom:6px; }
        .rq-doc { color:#fff; font-weight:800; font-size:12px; padding:2px 8px; border-radius:5px; }
        .rq-name { font-weight:700; font-size:13px; }
        .rq-total { font-size:28px; font-weight:800; line-height:1.2; }
        .rq-total small { font-size:12px; font-weight:400; color:var(--muted); }
        .rq-split { display:grid; grid-template-columns:repeat(4, 1fr); gap:4px; margin:8px 0; }
        .rq-stat { text-align:center; background:#f5f7fa; border-radius:6px; padding:4px 2px; }
        .rq-stat b { display:block; font-size:16px; }
        .rq-stat span { font-size:11px; color:var(--muted); }
        .heat { display:inline-block; min-width:34px; padding:2px 6px; border-radius:5px; font-weight:700; }

        /* Trend */
        .trend { display:flex; align-items:flex-end; gap:8px; height:220px; padding:4px 4px 0; border-bottom:1px solid var(--line); }
        .trend-col { flex:1; display:flex; flex-direction:column; align-items:center; height:100%; min-width:28px; }
        .trend-val { font-size:11.5px; font-weight:700; height:18px; }
        .trend-stack { flex:1; width:70%; max-width:46px; display:flex; flex-direction:column; justify-content:flex-end; }
        .trend-seg { width:100%; }
        .trend-seg:first-child { border-radius:4px 4px 0 0; }
        .trend-lbl { font-size:11.5px; color:var(--muted); padding:4px 0; }
    </style>

    <script>
        function dbTab(id) {
            document.querySelectorAll('.db-tab').forEach(function (b) { b.classList.toggle('active', b.getAttribute('data-tab') === id); });
            document.querySelectorAll('.db-panel').forEach(function (p) { p.classList.toggle('active', p.id === id); });
            var hf = document.getElementById('<%= hfTab.ClientID %>');
            if (hf) hf.value = id;
            return false;
        }
        function dbRestoreTab() {
            var hf = document.getElementById('<%= hfTab.ClientID %>');
            dbTab(hf && hf.value ? hf.value : 'tabEquip');
        }
        document.addEventListener('DOMContentLoaded', dbRestoreTab);
        window.addEventListener('load', function () {
            if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager)
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(dbRestoreTab);
        });
    </script>

    <asp:ScriptManager runat="server" ID="scriptManager"></asp:ScriptManager>

    <asp:UpdatePanel runat="server" ID="UP1">
        <ContentTemplate>
            <asp:HiddenField ID="hfTab" runat="server" Value="tabEquip" />

            <div class="db db-wrap">
                <div class="db-header">
                    <h1><span class="glyphicon glyphicon-dashboard"></span>&nbsp;Dashboard</h1>
                    <span class="db-sub">ภาพรวมสถิติและการใช้งานอุปกรณ์ — ข้อมูล ณ <%= DateTime.Now.ToString("dd-MMM-yyyy HH:mm", System.Globalization.CultureInfo.GetCultureInfo("en-US")) %></span>
                </div>

                <div class="db-tabs">
                    <button type="button" class="db-tab active" data-tab="tabEquip" onclick="return dbTab('tabEquip');">
                        <span class="glyphicon glyphicon-wrench"></span> Jig Tool &amp; Equipment
                    </button>
                    <button type="button" class="db-tab" data-tab="tabRequest" onclick="return dbTab('tabRequest');">
                        <span class="glyphicon glyphicon-file"></span> Request
                    </button>
                </div>

                <%-- ═══════════ Tab 1 : Jig Tool & Equipment ═══════════ --%>
                <div id="tabEquip" class="db-panel active">
                    <div class="db-filter">
                        <div>
                            <label>Dept</label>
                            <asp:DropDownList ID="ddlDept" runat="server" CssClass="form-control input-sm"
                                AutoPostBack="true" OnSelectedIndexChanged="ddlEquipFilter_Changed" />
                        </div>
                        <div>
                            <label>ระบบ</label>
                            <asp:DropDownList ID="ddlSystem" runat="server" CssClass="form-control input-sm"
                                AutoPostBack="true" OnSelectedIndexChanged="ddlEquipFilter_Changed">
                                <asp:ListItem Value="" Text="-- ทั้งหมด --" />
                                <asp:ListItem Value="1" Text="Jig Tool" />
                                <asp:ListItem Value="2" Text="Equipment" />
                            </asp:DropDownList>
                        </div>
                        <div class="db-muted" style="font-size:12px;">ไม่นับรวม Transfer / Scrap</div>
                    </div>

                    <div class="kpi-row"><asp:Literal ID="litEqKpi" runat="server" /></div>

                    <div class="db-card">
                        <h3><span class="glyphicon glyphicon-tags"></span> แยกตามประเภท <small>จำนวน / มูลค่า / อายุการใช้งาน</small></h3>
                        <div style="overflow-x:auto;"><asp:Literal ID="litEqByCat" runat="server" /></div>
                        <asp:Literal ID="litAgeLegend" runat="server" />
                    </div>

                    <div class="db-grid2">
                        <div class="db-card">
                            <h3><span class="glyphicon glyphicon-time"></span> อายุการใช้งาน <small>นับจาก Operation Date — จำนวน (%) / มูลค่า</small></h3>
                            <asp:Literal ID="litEqAge" runat="server" />
                        </div>
                        <div class="db-card">
                            <h3><span class="glyphicon glyphicon-tasks"></span> แยกตามสถานะ</h3>
                            <asp:Literal ID="litEqStatus" runat="server" />
                        </div>
                    </div>
                </div>

                <%-- ═══════════ Tab 2 : Request ═══════════ --%>
                <div id="tabRequest" class="db-panel">
                    <div class="db-filter">
                        <div>
                            <label>ปีที่ร้องขอ</label>
                            <asp:DropDownList ID="ddlYear" runat="server" CssClass="form-control input-sm"
                                AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed" />
                        </div>
                        <div class="db-muted" style="font-size:12px;">
                            AAD1 ยืมอุปกรณ์ · AAD2 แจ้งเปลี่ยนแปลง · AAD3 เบิกอุปกรณ์ · AAD4 จัดเก็บ (ตามชื่อใน Master_Document)
                        </div>
                    </div>

                    <div class="rq-row"><asp:Literal ID="litRqCards" runat="server" /></div>

                    <div class="db-card">
                        <h3><span class="glyphicon glyphicon-th"></span> เอกสาร × สถานะ</h3>
                        <div style="overflow-x:auto;"><asp:Literal ID="litRqStatus" runat="server" /></div>
                    </div>

                    <div class="db-card">
                        <h3><span class="glyphicon glyphicon-stats"></span> <asp:Literal ID="litTrendTitle" runat="server" /></h3>
                        <asp:Literal ID="litRqTrend" runat="server" />
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
