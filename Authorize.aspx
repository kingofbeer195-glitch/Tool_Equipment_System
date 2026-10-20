<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" 
    CodeFile="Authorize.aspx.cs"
    Inherits="Tool_Equipment_Control_System.Authorize" %>

<asp:Content ID="Manu_List" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Authorization Management</title>

    <link href="CSS/w3v4.10.css" rel="stylesheet" />
    <link href="CSS/fontsgoogleapis.css" rel="stylesheet" />
    <link href="DataTables-1.10.18/css/dataTables.css" rel="stylesheet">
    <link href="Content/chosen.css" rel="stylesheet">

    <script src="Scripts/chosen.jquery.js"></script>
    <script src="Scripts/jquery-1.9.1.js"></script>
    <script src="Scripts/script.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>
    <script src="DataTables-1.10.18/js/dataTables.js"></script>
    <script src="DataTables-1.10.18/FixedColumns-3.2.5/js/fixedColumns.dataTables.js"></script>

    <style>
        /* ========================================================
           AUTHORIZATION MANAGEMENT — UI REFRESH
           โทนน้ำเงินองค์กร (คงของเดิม) จัดระเบียบให้ชัดเจนขึ้น
           ======================================================== */

        /* ---------- Design tokens ---------- */
        :root {
            --navy-900:#12294d;   --navy-800:#1a3c6e;   --navy-600:#2d6aad;
            --navy-100:#e8f0fe;   --navy-050:#f5f8fd;
            --ink:#2b3648;        --ink-soft:#5a6a80;   --ink-faint:#8a97a8;
            --line:#a4c9fc;       --line-soft:#eef2f7;
            --bg:#eef1f6;         --card:#ffffff;
            --ok:#2e9e5b;         --ok-bg:#e6f6ec;      --ok-ink:#1c703c;
            --warn:#c98a12;       --warn-bg:#fdf4e0;    --warn-ink:#8a5e08;
            --danger:#d64550;     --danger-bg:#fdecec;  --danger-ink:#a12a33;
            --info:#2d6aad;       --info-bg:#e7f1fb;    --info-ink:#1a4a7d;
            --shadow:0 1px 3px rgba(18,41,77,.06), 0 6px 20px rgba(18,41,77,.07);
            --radius:10px;        --radius-sm:7px;
        }

        * { box-sizing:border-box; margin:0; padding:0; }
        body { font-family:'Segoe UI', Tahoma, sans-serif; font-size:14px; background:var(--bg); color:var(--ink); }
        a { text-decoration:none; }

        /* ---------- Layout ---------- */
        .page-wrapper { max-width:1320px; margin:0 auto; padding:10px 15px; }

        .page-header {
            background:linear-gradient(120deg, var(--navy-900) 0%, var(--navy-600) 100%);
            color:#fff; padding:5px 25px; border-radius:var(--radius) var(--radius) 0 0;
            position:relative; overflow:hidden;
        }
        .page-header::after {
            content:""; position:absolute; right:-40px; top:-60px;
            width:220px; height:220px; border-radius:50%;
            background:rgba(255,255,255,.05);
        }
        .page-header .grid-headerTopic1 {
            display:grid; grid-template-columns:64px auto; align-items:center; gap:14px;
            position:relative; z-index:1;
        }
        .page-header .grid-headerTopic2 { display:grid; grid-template-rows:auto auto; align-content:center; }
        .page-header h2 { font-size:21px; font-weight:700; letter-spacing:.2px; }
        .page-header p  { font-size:13.5px; opacity:.82; margin-top:2px; font-weight:400; }
        .page-header img { height:52px; filter:drop-shadow(0 2px 4px rgba(0,0,0,.25)); }

        .main-card {
            background:var(--card); border-radius:0 0 var(--radius) var(--radius);
            box-shadow:var(--shadow); padding: 10px 15px 15px;
        }

        /* keep old grid helpers valid (code-behind markup relies on class names) */
        .grid-headerTopic1 { display:grid; }
        .grid-headerTopic2 { display:grid; }

        /* ---------- Tabs ---------- */
        .tabs { display:flex; gap:4px; border-bottom:1px solid var(--line); margin-bottom:22px; }
        .tab-btn {
            padding:11px 20px 13px; border:none; background:none; cursor:pointer;
            font-size:15px; color:var(--ink-soft); font-weight:600;
            border-bottom:3px solid transparent; margin-bottom:-1px;
            display:inline-flex; align-items:center; gap:7px; transition:color .15s, border-color .15s;
        }
        .tab-btn:hover { color:var(--navy-600); }
        .tab-btn.active { color:var(--navy-800); border-bottom-color:var(--navy-600); }
        .tab-panel { display:none; animation:fadeIn .18s ease; background:#d7e9f0; padding:5px;}
        .tab-panel.active { display:block; }
        @keyframes fadeIn { from{opacity:0; transform:translateY(4px);} to{opacity:1; transform:none;} }

        /* ---------- Section title ---------- */
        .section-title {
            font-size:15px; font-weight:600; color:var(--navy-800);
            margin-bottom:8px; padding-bottom:8px; border-bottom:1px solid var(--line);
            display:flex; align-items:center; gap:8px; padding : 5px 5px;
        }

        /* ---------- Form controls ---------- */
        .form-row { display:flex; flex-wrap:wrap; gap:14px; margin-bottom:14px; align-items:flex-end; }
        .form-group { display:flex; flex-direction:column; gap:5px; }
        .form-group label { font-size:12px; font-weight:600; color:var(--ink-soft); letter-spacing:.2px; }
        .form-control {
            padding:4px 11px; border:1px solid #cfd8e3; border-radius:var(--radius-sm);
            font-size:13px; min-width:160px; background:#fff; transition:border-color .15s, box-shadow .15s;
            text-align:center;
        }
        .form-control:focus { outline:none; border-color:var(--navy-600); box-shadow:0 0 0 3px rgba(45,106,173,.14); }
        .form-control[readonly] { background:#f4f6f9; color:var(--ink-soft); }
        select.form-control { min-width:200px; height:35px !important}

        /* ---------- Buttons ---------- */
        .btn {
            padding:8px 16px; border:none; border-radius:var(--radius-sm); cursor:pointer;
            font-size:13px; font-weight:600; transition:filter .15s, transform .05s, box-shadow .15s;
            display:inline-flex; align-items:center; gap:6px; line-height:1.2;
        }
        .btn:active { transform:translateY(1px); }
        .btn-primary   { background:var(--navy-600); color:#fff; }
        .btn-primary:hover   { background:var(--navy-800); }
        .btn-success   { background:var(--ok); color:#fff; box-shadow:0 2px 6px rgba(46,158,91,.25); }
        .btn-success:hover   { filter:brightness(1.06); }
        .btn-warning   { background:var(--warn-bg); color:var(--warn-ink); border:1px solid #f0dcae; }
        .btn-warning:hover   { background:#fbecca; }
        .btn-danger    { background:var(--danger-bg); color:var(--danger-ink); border:1px solid #f4c9cc; }
        .btn-danger:hover    { background:#fbdcde; }
        .btn-secondary { background:#eef1f6; color:var(--ink-soft); border:1px solid var(--line); }
        .btn-secondary:hover { background:#e2e7ef; }
        .btn-sm { padding:5px 11px; font-size:12px; }
        .btn-xs { padding:3px 9px; font-size:11px; }

        /* ---------- Alert ---------- */
        .alert { padding:11px 15px; border-radius:var(--radius-sm); font-size:13px; margin-bottom:16px; border:1px solid transparent; display:flex; align-items:center; gap:9px; }
        .alert::before { font-size:15px; line-height:1; }
        .alert-success { background:var(--ok-bg); color:var(--ok-ink); border-color:#bfe6cd; }
        .alert-success::before { content:"\2714"; }
        .alert-danger  { background:var(--danger-bg); color:var(--danger-ink); border-color:#f4c9cc; }
        .alert-danger::before { content:"\26A0"; }
        .alert-info    { background:var(--info-bg); color:var(--info-ink); border-color:#c5ddf3; }
        .alert-info::before { content:"\2139"; }

        /* ---------- Table ---------- */
        .table-wrap { overflow-x:auto; border:1px solid var(--line); border-radius:var(--radius-sm); }
        table.grid { width:100%; border-collapse:collapse; font-size:13px; }
        table.grid th {
            background:var(--navy-050); color:var(--navy-800); padding:11px 14px; text-align:left;
            font-weight:700; white-space:nowrap; border-bottom:2px solid var(--line);
            font-size:12px; letter-spacing:.3px; text-transform:uppercase;
        }
        table.grid td { padding:11px 14px; border-bottom:1px solid var(--line-soft); vertical-align:middle; }
        table.grid tr:last-child td { border-bottom:none; }
        table.grid tbody tr { transition:background .12s; }
        table.grid tbody tr:hover td { background:var(--navy-050); }
        .empty-row td { text-align:center; color:var(--ink-faint); padding:22px; }

        /* ---------- Generic badge ---------- */
        .badge { display:inline-block; padding:3px 9px; border-radius:20px; font-size:11px; font-weight:700; letter-spacing:.2px; }
        .badge-auto   { background:var(--info-bg);  color:var(--info-ink); }
        .badge-manual { background:#ede7f6; color:#5e35b1; }
        .badge-skip   { background:#eef1f5; color:var(--ink-faint); }
        .badge-admin   { background:#ede7f6; color:#5e35b1; }
        .badge-approve { background:var(--info-bg); color:var(--info-ink); }
        .badge-active  { background:var(--ok-bg); color:var(--ok-ink); }
        .badge-step    { background:var(--warn-bg); color:var(--warn-ink); }

        /* ========================================================
           FLOW AUTHORIZATION  — vertical timeline
           ======================================================== */

        /* Process picker */
        .panel-add { background:var(--navy-050); border:1px solid var(--line); border-radius:var(--radius-sm); padding:18px 20px; margin: 5px 10px 10px; }
        .panel-add-title { font-size:13px; font-weight:700; color:var(--navy-800); margin-bottom:12px; display:flex; align-items:center; gap:7px; }

        /* legend explaining step types */
        .flow-legend {
            display:flex; flex-wrap:wrap; gap:18px; padding:12px 12px; margin: 5px 10px 12px;
            background:#fff; border:1px solid var(--line); border-radius:var(--radius-sm);
            font-size:12px; color:var(--ink-soft);
        }
        .flow-legend .lg-item { display:flex; align-items:center; gap:8px; }
        .flow-legend .lg-swatch { width:14px; height:14px; border-radius:4px; flex:none; }
        .flow-legend .lg-manual { background:#7e57c2; }
        .flow-legend .lg-auto   { background:var(--navy-600); }
        .flow-legend .lg-skip   { background:#c3ccd8; }
        .flow-legend b { color:var(--ink); font-weight:700; }

        /* flow header row (title + save) */
        .flow-toolbar { display:flex; justify-content:space-between; align-items:center; gap:14px; margin: 5px 10px 12px; flex-wrap:wrap; }
        .flow-toolbar .section-title { margin-bottom:0; border:none; padding:0; }
        .flow-process-name { color:var(--navy-600); }

        /* the timeline */
        .step-container { position:relative; margin:5px 10px; }

        .step-item {
            position:relative; margin-left:34px; margin-bottom:0;
            border:1px solid var(--line); border-left:4px solid var(--navy-600);
            border-radius:var(--radius-sm); background:#fff; padding:14px 16px;
            box-shadow:0 1px 2px rgba(18,41,77,.04);
        }
        /* colored spine per step type via modifier set in code? -> we key off badge inside */
        .step-item.is-manual { border-left-color:#7e57c2; }
        .step-item.is-auto   { border-left-color:var(--navy-600); }
        .step-item.is-skip   { border-left-color:#c3ccd8; }

        /* number node on the spine */
        .step-header { display:flex; align-items:center; justify-content:space-between; margin-bottom:10px; gap:10px; }
        .step-title { display:flex; align-items:center; gap:10px; font-size:14px; font-weight:600; color:var(--ink); }
        .step-num {
            position:absolute; left:-38px; top:10px;
            width:30px; height:30px; background:#fff; color:var(--navy-800);
            border:2px solid var(--navy-600); border-radius:50%;
            display:flex; align-items:center; justify-content:center;
            font-size:13px; font-weight:800; box-shadow:0 1px 3px rgba(18,41,77,.12); z-index:2;
        }
        .is-manual .step-num { border-color:#7e57c2; color:#5e35b1; }
        .is-skip   .step-num { border-color:#c3ccd8; color:var(--ink-faint); }

        /* connector line down to next step */
        .flow-arrow { position:relative; height:22px; margin-left:34px; }
        .flow-arrow::before {
            content:""; position:absolute; left:-19px; top:-2px; bottom:-2px;
            width:2px; background:var(--line);
        }
        .flow-arrow > * { display:none; } /* hide old chevron glyph, keep panel for code-behind */

        .approver-list {
            display:flex; flex-wrap:wrap; gap:7px; min-height:34px; padding:9px 10px;
            background:var(--navy-050); border:1px dashed #c9d6e6; border-radius:var(--radius-sm);
        }
        .approver-tag {
            display:inline-flex; align-items:center; gap:5px; background:#fff; color:var(--navy-800);
            border:1px solid #d3e0f0; border-radius:20px; padding:4px 10px; font-size:12px; font-weight:600;
            box-shadow:0 1px 2px rgba(18,41,77,.05);
        }
        .approver-tag small { color:var(--ink-faint); font-weight:500; }
        .approver-tag .remove-tag { cursor:pointer; color:var(--ink-faint); font-size:15px; line-height:1; margin-left:3px; }
        .approver-tag .remove-tag:hover { color:var(--danger); }
        .add-approver-row { display:flex; gap:7px; margin-top:10px; align-items:center; flex-wrap:wrap; }
        .step-note { font-size:12px; color:var(--ink-faint); font-style:italic; padding:4px 2px; }

        /* ========================================================
           ADMIN MANAGEMENT — level explainer cards
           ======================================================== */

        /* explainer strip: what each level can do */
        .level-guide { display:grid; grid-template-columns:repeat(4, 1fr); gap:12px; margin-bottom:12px; padding:0px 5px; }
        .lvl-card {
            border:1px solid var(--line); border-top:3px solid var(--navy-600);
            border-radius:var(--radius-sm); padding:8px 8px; background:#fff;
            box-shadow:0 1px 2px rgba(18,41,77,.04); margin:5px;
        }
        .lvl-card.tier-1 { border-top-color:#4a90d9; }
        .lvl-card.tier-2 { border-top-color:#2d6aad; }
        .lvl-card.tier-3 { border-top-color:#1a3c6e; }
        .lvl-card.tier-4 { border-top-color:#6f42c1; background:#faf7ff; }
        .lvl-card .lvl-head { display:flex; align-items:center; gap:8px; margin-bottom:7px; }
        .lvl-card .lvl-chip {
            font-size:11px; font-weight:800; padding:2px 9px; border-radius:20px;
            background:var(--navy-100); color:var(--navy-800); letter-spacing:.3px;
        }
        .lvl-card.tier-4 .lvl-chip { background:#ede7f6; color:#5e35b1; }
        .lvl-card .lvl-name { font-size:13px; font-weight:700; color:var(--ink); }
        .lvl-card p { font-size:12px; color:var(--ink-soft); line-height:1.55; }

        /* checkbox list styling (class name kept: lvl-cbl / lvl-add-cbl for JS hooks) */
        .lvl-cbl label { margin-right:18px; font-weight:600; color:var(--ink); display:inline-flex; align-items:center; cursor:pointer; }
        .lvl-cbl input[type=checkbox] { margin-right:6px; width:15px; height:15px; accent-color:var(--navy-600); cursor:pointer; }

        /* level badges inside table */
        .lvl-badge { display:inline-block; padding:3px 9px; border-radius:20px; font-size:11px; font-weight:700; margin-right:5px; cursor:default; }
        .lvl-on  { background:var(--ok-bg); color:var(--ok-ink); border:1px solid #bfe6cd; }
        .lvl-off { background:#f2f4f7; color:#c0c8d2; border:1px solid #e8ecf1; }

        /* ---------- Responsive ---------- */
        @media (max-width:920px) { .level-guide { grid-template-columns:repeat(2,1fr); } }
        @media (max-width:768px) {
            .form-row { flex-direction:column; align-items:stretch; }
            .form-control { min-width:100%; }
            .level-guide { grid-template-columns:1fr; }
            .flow-toolbar { align-items:flex-start; }
        }
        @media (prefers-reduced-motion:reduce) { .tab-panel { animation:none; } }
    </style>

    <%--Loading overlay--%>
    <style>
        .loading {
            height: 100vh;
            width: 100%;
            display: flex;
            align-items: center;
            justify-content: center;
            background: hsl(220deg 29% 90% / 50%);
    
            flex-direction:column;
            text-align:center;
        }
        .pl {
            width: 6em;
            height: 6em;
        }
        .pl__ring {
            animation: ringA 2s linear infinite;
        }
        .pl__ring--a {
            stroke: #f42f25;
        }
        .pl__ring--b {
            animation-name: ringB;
            stroke: #f49725;
        }
        .pl__ring--c {
            animation-name: ringC;
            stroke: #255ff4;
        }
        .pl__ring--d {
            animation-name: ringD;
            stroke: #f42582;
        }
        /* Animations */
        @keyframes ringA {
            from, 4% {
                stroke-dasharray: 0 660;
                stroke-width: 20;
                stroke-dashoffset: -330;
            }

            12% {
                stroke-dasharray: 60 600;
                stroke-width: 30;
                stroke-dashoffset: -335;
            }

            32% {
                stroke-dasharray: 60 600;
                stroke-width: 30;
                stroke-dashoffset: -595;
            }

            40%, 54% {
                stroke-dasharray: 0 660;
                stroke-width: 20;
                stroke-dashoffset: -660;
            }

            62% {
                stroke-dasharray: 60 600;
                stroke-width: 30;
                stroke-dashoffset: -665;
            }

            82% {
                stroke-dasharray: 60 600;
                stroke-width: 30;
                stroke-dashoffset: -925;
            }

            90%, to {
                stroke-dasharray: 0 660;
                stroke-width: 20;
                stroke-dashoffset: -990;
            }
        }
        @keyframes ringB {
            from, 12% {
                stroke-dasharray: 0 220;
                stroke-width: 20;
                stroke-dashoffset: -110;
            }

            20% {
                stroke-dasharray: 20 200;
                stroke-width: 30;
                stroke-dashoffset: -115;
            }

            40% {
                stroke-dasharray: 20 200;
                stroke-width: 30;
                stroke-dashoffset: -195;
            }

            48%, 62% {
                stroke-dasharray: 0 220;
                stroke-width: 20;
                stroke-dashoffset: -220;
            }

            70% {
                stroke-dasharray: 20 200;
                stroke-width: 30;
                stroke-dashoffset: -225;
            }

            90% {
                stroke-dasharray: 20 200;
                stroke-width: 30;
                stroke-dashoffset: -305;
            }

            98%, to {
                stroke-dasharray: 0 220;
                stroke-width: 20;
                stroke-dashoffset: -330;
            }
        }
        @keyframes ringC {
            from {
                stroke-dasharray: 0 440;
                stroke-width: 20;
                stroke-dashoffset: 0;
            }

            8% {
                stroke-dasharray: 40 400;
                stroke-width: 30;
                stroke-dashoffset: -5;
            }

            28% {
                stroke-dasharray: 40 400;
                stroke-width: 30;
                stroke-dashoffset: -175;
            }

            36%, 58% {
                stroke-dasharray: 0 440;
                stroke-width: 20;
                stroke-dashoffset: -220;
            }

            66% {
                stroke-dasharray: 40 400;
                stroke-width: 30;
                stroke-dashoffset: -225;
            }

            86% {
                stroke-dasharray: 40 400;
                stroke-width: 30;
                stroke-dashoffset: -395;
            }

            94%, to {
                stroke-dasharray: 0 440;
                stroke-width: 20;
                stroke-dashoffset: -440;
            }
        }
        @keyframes ringD {
            from, 8% {
                stroke-dasharray: 0 440;
                stroke-width: 20;
                stroke-dashoffset: 0;
            }

            16% {
                stroke-dasharray: 40 400;
                stroke-width: 30;
                stroke-dashoffset: -5;
            }

            36% {
                stroke-dasharray: 40 400;
                stroke-width: 30;
                stroke-dashoffset: -175;
            }

            44%, 50% {
                stroke-dasharray: 0 440;
                stroke-width: 20;
                stroke-dashoffset: -220;
            }

            58% {
                stroke-dasharray: 40 400;
                stroke-width: 30;
                stroke-dashoffset: -225;
            }

            78% {
                stroke-dasharray: 40 400;
                stroke-width: 30;
                stroke-dashoffset: -395;
            }

            86%, to {
                stroke-dasharray: 0 440;
                stroke-width: 20;
                stroke-dashoffset: -440;
            }
        }
    </style>

    <%--Show & HideLoading--%>
    <script>
        function hideLoading() {
            $('#loadingOverlay').hide();
        }
        function showLoading() {
            $('#loadingOverlay').show();
        }

        // ✅ แสดง overlay อัตโนมัติทุกครั้งที่เกิด async postback (ปุ่ม/Search/AutoPostBack)
        if (typeof (Sys) !== "undefined") {
            var _prm = Sys.WebForms.PageRequestManager.getInstance();

            _prm.add_beginRequest(function () {
                showLoading();
            });

            _prm.add_endRequest(function () {
                hideLoading();
            });
        }
    </script>

    <asp:ScriptManager runat="server" ID="scriptManager" EnablePageMethods="true"></asp:ScriptManager>

    <asp:UpdatePanel runat="server" ID="UP1">

        <ContentTemplate>            

            <%--Loading Overlay--%>
            <div id="loadingOverlay" style="display:none; position:fixed; z-index:9999; top:0; left:0;   
                width:100%; height:100%; background:rgba(0,0,0,0.6); color:white; text-align:center; font-size:20px;">

                <div class="loading">
                    <svg class="pl" width="240" height="240" viewBox="0 0 240 240">
                        <circle class="pl__ring pl__ring--a" cx="120" cy="120" r="105" fill="none" stroke="#000" stroke-width="20" stroke-dasharray="0 660" stroke-dashoffset="-330" stroke-linecap="round"></circle>
                        <circle class="pl__ring pl__ring--b" cx="120" cy="120" r="35" fill="none" stroke="#000" stroke-width="20" stroke-dasharray="0 220" stroke-dashoffset="-110" stroke-linecap="round"></circle>
                        <circle class="pl__ring pl__ring--c" cx="85" cy="120" r="70" fill="none" stroke="#000" stroke-width="20" stroke-dasharray="0 440" stroke-linecap="round"></circle>
                        <circle class="pl__ring pl__ring--d" cx="155" cy="120" r="70" fill="none" stroke="#000" stroke-width="20" stroke-dasharray="0 440" stroke-linecap="round"></circle>
                    </svg>
        
                    <div style="margin-top:20px; font-size:18px;">
                        กำลังประมวลผล...
                    </div>   
                </div>                              
            </div>

            <div id="form1" runat="server">                

                <div class="page-wrapper">

                    <!-- Header -->
                    <div class="page-header" style="margin:20px 0px 0px;">
                        <div class="grid-headerTopic1">
                            <img src="images/key-person.png" />
                            <div class="grid-headerTopic2">
                                <div>
                                    <h2>Authorization Management</h2>
                                </div>
                                <div>
                                    <p>จัดการสิทธิ์ Admin และกำหนด Flow การอนุมัติ (Authorization Flow)</p>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="main-card">

                        <!-- Alert -->
                        <asp:Panel ID="pnlAlert" runat="server" Visible="false">
                            <div id="divAlert" runat="server" class="alert"></div>
                        </asp:Panel>

                        <!-- Hidden fields for client state -->
                        <asp:HiddenField ID="hfActiveTab"       runat="server" Value="tab-flow" />
                        <asp:HiddenField ID="hfEditFlowId"      runat="server" Value="" />
                        <asp:HiddenField ID="hfFlowStepsJson"   runat="server" Value="[]" />

                        <!-- ===================== TABS ===================== -->
                        <div class="tabs">
                            <button type="button" class="tab-btn" data-tab="tab-flow"  onclick="switchTab('tab-flow')">🔀 Flow Authorization</button>
                            <button type="button" class="tab-btn" data-tab="tab-admin" onclick="switchTab('tab-admin')">🛡️ Admin Management</button>
                            <button type="button" class="tab-btn" data-tab="tab-newuser" onclick="switchTab('tab-newuser')">🧑‍💼 New User</button>
                        </div>

                        <!-- ===================== TAB: FLOW ===================== -->
                        <div id="tab-flow" class="tab-panel">

                            <!-- ---- เลือก Process ---- -->
                            <div class="panel-add">
                                <div class="panel-add-title">📋 เลือก Process เพื่อกำหนดสิทธิ์อนุมัติ</div>
                                <div class="form-row" style="margin-bottom:0;">
                                    <div class="form-group">
                                        <label>Process *</label>
                                        <asp:DropDownList ID="ddlProcess" runat="server" CssClass="form-control"
                                            AutoPostBack="true" OnSelectedIndexChanged="ddlProcess_SelectedIndexChanged"
                                            Width="320px" />
                                    </div>
                                </div>
                            </div>

                            <!-- ---- Flow Steps ของ Process ที่เลือก ---- -->
                            <asp:Panel ID="pnlFlow" runat="server" Visible="false">

                                <!-- legend: อธิบายประเภท step -->
                                <div class="flow-legend">
                                    <span style="font-weight:700; color:var(--ink);">ประเภทขั้นตอน:</span>
                                    <span class="lg-item"><span class="lg-swatch lg-manual"></span> <b>กำหนดเอง</b> — ระบุรายชื่อผู้อนุมัติเองในขั้นนี้</span>
                                    <span class="lg-item"><span class="lg-swatch lg-auto"></span> <b>Auto Admin</b> — ดึงรายชื่อจาก Admin (Level 1) อัตโนมัติ</span>
                                    <span class="lg-item"><span class="lg-swatch lg-skip"></span> <b>ทุกคน</b> — ทำได้ทุกคน ไม่ต้องกำหนดรายชื่อ</span>
                                </div>

                                <div class="flow-toolbar">
                                    <div class="section-title">
                                        📝 Flow การอนุมัติ: <span class="flow-process-name"><asp:Literal ID="litFlowProcessName" runat="server" /></span>
                                    </div>
                                    <asp:LinkButton ID="btnSaveAuthorize" runat="server" CssClass="btn btn-success"
                                        OnClick="btnSaveAuthorize_Click">
                                        ✓ บันทึก Authorize
                                    </asp:LinkButton>
                                </div>

                                <div class="step-container">
                                <asp:Repeater ID="rptFlowSteps" runat="server" OnItemCommand="rptFlowSteps_ItemCommand">
                                    <ItemTemplate>
                                        <div class="step-item <%# GetStepItemCss(Eval("step_mode")) %>">
                                            <span class="step-num"><%# Container.ItemIndex + 1 %></span>
                                            <div class="step-header">
                                                <span class="step-title">
                                                    <strong><%# Eval("status_name") %></strong>
                                                    <span class="badge <%# GetStepBadgeCss(Eval("step_mode")) %>"><%# Eval("step_mode_label") %></span>
                                                </span>
                                            </div>

                                            <!-- รายชื่อผู้อนุมัติใน step นี้ -->
                                            <asp:Panel ID="pnlApvWrap" runat="server" Visible='<%# !IsSkipStep(Eval("status_id")) %>'>
                                                <div class="approver-list">
                                                    <asp:Repeater ID="rptStepApprovers" runat="server"
                                                        DataSource='<%# GetApprovers(Eval("status_id")) %>'>
                                                        <ItemTemplate>
                                                            <span class="approver-tag">
                                                                <%# Eval("emp_name") %> <small>(<%# Eval("emp_id") %>)</small>
                                                                <asp:LinkButton ID="btnRemoveApv" runat="server"
                                                                    CssClass="remove-tag"
                                                                    CommandName="REMOVE_APV"
                                                                    CommandArgument='<%# Eval("status_id") + "|" + Eval("emp_id") %>'
                                                                    OnClientClick="return confirm('ลบผู้อนุมัติออก?');">
                                                                    &times;
                                                                </asp:LinkButton>
                                                            </span>
                                                        </ItemTemplate>
                                                    </asp:Repeater>
                                                    <asp:Label ID="lblNoApv" runat="server"
                                                        Visible='<%# GetApproverCount(Eval("status_id")) == 0 %>'
                                                        CssClass="step-note" Text="— ยังไม่มีรายชื่อ —" />
                                                </div>

                                                <!-- เพิ่มรายชื่อ (เฉพาะ manual step) -->
                                                <asp:Panel ID="pnlAddManual" runat="server" Visible='<%# IsManualStep(Eval("status_id")) %>'>
                                                    <div class="add-approver-row">
                                                        <asp:TextBox ID="txtApvEmp" runat="server" CssClass="form-control"
                                                            MaxLength="6" placeholder="รหัสพนักงาน 6 หลัก"
                                                            Width="170px" style="text-align:center;" />
                                                        <asp:LinkButton ID="btnAddApv" runat="server"
                                                            CssClass="btn btn-primary btn-sm"
                                                            CommandName="ADD_APV"
                                                            CommandArgument='<%# Eval("status_id") %>'>
                                                            + เพิ่มผู้อนุมัติ
                                                        </asp:LinkButton>

                                                        <%-- ★ ปุ่ม Auto All เฉพาะ step 30 (J3 Up Approve) --%>
                                                        <asp:LinkButton ID="btnAutoAll" runat="server"
                                                            CssClass="btn btn-warning btn-sm"
                                                            Visible='<%# IsAutoAllStep(Eval("status_id")) %>'
                                                            CommandName="AUTO_ALL"
                                                            CommandArgument='<%# Eval("status_id") %>'
                                                            OnClientClick="return confirm('ระบบจะลบผู้อนุมัติที่ตำแหน่งไม่ใช่ J3/J4/M1/M2 ออก และเพิ่มพนักงาน J3/J4/M1/M2 (div_id=2) ที่ยังไม่มีในรายชื่อ\nยืนยันหรือไม่?');">
                                                            ⚡ Auto All (J3/J4/M1/M2)
                                                        </asp:LinkButton>
                                                    </div>
                                                </asp:Panel>

                                                <asp:Panel ID="pnlAutoNote" runat="server" Visible='<%# IsAutoStep(Eval("status_id")) %>'>
                                                    <div class="step-note" style="margin-top:8px;">
                                                        🔒 รายชื่อดึงอัตโนมัติจาก Equipment_Admin (Level 1)
                                                    </div>
                                                </asp:Panel>
                                            </asp:Panel>

                                            <!-- step ที่ข้าม (status_id = 1) -->
                                            <asp:Panel ID="pnlSkipNote" runat="server" Visible='<%# IsSkipStep(Eval("status_id")) %>'>
                                                <div class="step-note">— ขั้นตอนนี้ทำได้ทุกคน ไม่ต้องกำหนดรายชื่อ —</div>
                                            </asp:Panel>
                                        </div>

                                        <asp:Panel ID="pnlArrow" runat="server" Visible='<%# !IsLastStep(Container.ItemIndex) %>'>
                                            <div class="flow-arrow">&#8595;</div>
                                        </asp:Panel>
                                    </ItemTemplate>
                                </asp:Repeater>
                                </div>

                                <div class="form-row" style="margin-top:20px; justify-content:flex-end;">
                                    <asp:LinkButton ID="btnSaveAuthorize2" runat="server" CssClass="btn btn-success"
                                        OnClick="btnSaveAuthorize_Click">
                                        ✓ บันทึก Authorize
                                    </asp:LinkButton>
                                </div>
                            </asp:Panel>

                        </div><!-- /tab-flow -->

                        <!-- ===================== TAB: ADMIN ===================== -->
                        <div id="tab-admin" class="tab-panel">

                            <!-- ===== Level explainer cards ===== -->
                            <div class="section-title">📖 คำอธิบายระดับสิทธิ์ (Level)</div>
                            <div class="level-guide">
                                <div class="lvl-card tier-1">
                                    <div class="lvl-head">
                                        <span class="lvl-chip">Level 1</span>
                                        <span class="lvl-name">Data Admin</span>
                                    </div>
                                    <p>• แก้ไขและจัดการรายการอุปกรณ์</p>
                                    <p>• อนุมัติการร้องขอในระบบ</p>
                                </div>
                                <div class="lvl-card tier-2">
                                    <div class="lvl-head">
                                        <span class="lvl-chip">Level 2</span>
                                        <span class="lvl-name">Tana Admin</span>
                                    </div>
                                    <p>• Setting Tanaoroshi</p>
                                    <p>• ดูแลระบบ Tanaoroshi</p>
                                </div>
                                <div class="lvl-card tier-3">
                                    <div class="lvl-head">
                                        <span class="lvl-chip">Level 3</span>
                                        <span class="lvl-name">Flow Admin</span>
                                    </div>
                                    <p>• กำหนดสิทธิ์การอนุมัติแต่ละ Process</p>
                                    <p>• กำหนดสิทธิ์ Admin</p>
                                </div>
                                <div class="lvl-card tier-4">
                                    <div class="lvl-head">
                                        <span class="lvl-chip">Super Admin</span>
                                        <span class="lvl-name"></span>
                                    </div>
                                    <p>• ครอบคลุมสิทธิ์ Level 1–3 ทั้งหมด (เมื่อเลือกจะติ๊ก Level 1–3 ให้อัตโนมัติ)</p>
                                </div>
                            </div>

                            <!-- ===== Add Admin Panel ===== -->
                            <asp:Panel ID="pnlAddAdmin" runat="server" CssClass="panel-add">
                                <div class="panel-add-title">➕ เพิ่ม Admin ใหม่</div>

                                <div class="form-row">
                                    <div class="form-group">
                                        <label>รหัสพนักงาน (6 หลัก) *</label>
                                        <asp:TextBox ID="txtEmpId" runat="server" CssClass="form-control" style="text-align: center;"
                                            MaxLength="6" placeholder="เช่น 123456"
                                            AutoPostBack="true" OnTextChanged="txtEmpId_TextChanged" Width="140px" />
                                    </div>
                                    <div class="form-group">
                                        <label>ชื่อ-นามสกุล</label>
                                        <asp:TextBox ID="txtEmpName" runat="server" CssClass="form-control" ReadOnly="true" Width="240px" style="text-align: center;"/>
                                    </div>
                                    <div class="form-group">
                                        <label>ตำแหน่ง</label>
                                        <asp:TextBox ID="txtPosition" runat="server" CssClass="form-control" ReadOnly="true" Width="180px" style="text-align: center;" />
                                    </div>
                                    <div class="form-group">
                                        <label>แผนก</label>
                                        <asp:TextBox ID="txtDept" runat="server" CssClass="form-control" ReadOnly="true" Width="160px" style="text-align: center;" />
                                    </div>
                                    <div class="form-group">
                                        <label>Dept Code</label>
                                        <asp:TextBox ID="txtDeptCode" runat="server" CssClass="form-control" ReadOnly="true" Width="100px" style="text-align: center;" />
                                    </div>
                                </div>

                                <asp:HiddenField ID="hdtxtBand" runat="server" />
                                <asp:HiddenField ID="hdtxtEmail" runat="server" />

                                <!-- Level checkboxes (แสดงหลัง Search พนักงานสำเร็จ) -->
                                <asp:Panel ID="pnlAddLevelWrap" runat="server" style="display:none;">
                                    <div class="form-row" style="align-items:flex-start;">
                                        <div class="form-group" style="min-width:100%;">
                                            <label>ระดับสิทธิ์ (Level) *</label>
                                            <asp:CheckBoxList ID="cblAddLevel" runat="server"
                                                RepeatDirection="Horizontal" RepeatLayout="Flow"
                                                CssClass="lvl-cbl lvl-add-cbl"
                                                style="background:#fff; border:1px solid #cfd8e3; border-radius:7px; padding:13px; display:block;" />
                                        </div>
                                    </div>

                                    <div class="form-row" style="margin-bottom:0;">
                                        <div class="form-group">
                                            <label>&nbsp;</label>
                                            <asp:LinkButton ID="btnSaveAdmin" runat="server" CssClass="btn btn-success" OnClick="btnSaveAdmin_Click">
                                                ✓ บันทึก Admin
                                            </asp:LinkButton>
                                        </div>
                                    </div>
                                </asp:Panel>
                            </asp:Panel>

                            <!-- ===== Admin List ===== -->
                            <div class="section-title">🛡️ รายการ Admin</div>
                            <div class="table-wrap" style="margin: 5px 10px;">
                                <asp:Repeater ID="rptAdmins" runat="server" OnItemCommand="rptAdmins_ItemCommand">
                                    <HeaderTemplate>
                                        <table class="grid">
                                            <thead>
                                                <tr>
                                                    <th style="width:40px;">#</th>
                                                    <th>รหัสพนักงาน</th>
                                                    <th>ชื่อ-นามสกุล</th>
                                                    <th>แผนก</th>
                                                    <th>ระดับสิทธิ์ (Level)</th>
                                                    <th style="width:160px;">จัดการ</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <tr>
                                            <td><%# Container.ItemIndex + 1 %></td>
                                            <td><%# Eval("emp_id") %></td>
                                            <td><strong><%# Eval("fname") %> <%# Eval("lname") %></strong></td>
                                            <td><%# Eval("dept_code") %></td>
                                            <td>
                                                <!-- Read mode -->
                                                <asp:Panel ID="pnlLevelRead" runat="server" Visible='<%# !IsEditing((string)Eval("emp_id")) %>'>
                                                    <asp:Literal ID="litLevels" runat="server"
                                                        Text='<%# BuildLevelBadges(Eval("level_1"), Eval("level_2"), Eval("level_3"), Eval("level_4")) %>' />
                                                </asp:Panel>
                                                <!-- Edit mode -->
                                                <asp:Panel ID="pnlLevelEdit" runat="server" Visible='<%# IsEditing((string)Eval("emp_id")) %>'>
                                                    <asp:CheckBoxList ID="cblEditLevel" runat="server" RepeatDirection="Horizontal"
                                                        RepeatLayout="Flow" CssClass="lvl-cbl" />
                                                </asp:Panel>
                                            </td>
                                            <td>
                                                <!-- Read mode buttons -->
                                                <asp:Panel ID="pnlBtnRead" runat="server" Visible='<%# !IsEditing((string)Eval("emp_id")) %>'>
                                                    <asp:LinkButton ID="btnEditAdmin" runat="server"
                                                        CommandName="EDIT_ADMIN" CommandArgument='<%# Eval("emp_id") %>'
                                                        CssClass="btn btn-warning btn-sm"
                                                        Style="margin: 4px;">✎ Edit</asp:LinkButton>
                                                    <asp:LinkButton ID="btnDeleteAdmin" runat="server"
                                                        CommandName="DELETE_ADMIN" CommandArgument='<%# Eval("emp_id") %>'
                                                        CssClass="btn btn-danger btn-sm" Style="margin: 4px;"
                                                        OnClientClick="return confirm('ยืนยันลบ Admin คนนี้?');">🗑 Delete</asp:LinkButton>
                                                </asp:Panel>
                                                <!-- Edit mode buttons -->
                                                <asp:Panel ID="pnlBtnEdit" runat="server" Visible='<%# IsEditing((string)Eval("emp_id")) %>'>
                                                    <asp:LinkButton ID="btnSaveEdit" runat="server"
                                                        CommandName="SAVE_ADMIN" CommandArgument='<%# Eval("emp_id") %>'
                                                        CssClass="btn btn-success btn-sm"
                                                        Style="margin: 4px;">✓ Save</asp:LinkButton>
                                                    <asp:LinkButton ID="btnCancelEdit" runat="server"
                                                        CommandName="CANCEL_ADMIN" CommandArgument='<%# Eval("emp_id") %>'
                                                        CssClass="btn btn-secondary btn-sm"
                                                        Style="margin: 4px;">✖ Cancel</asp:LinkButton>
                                                </asp:Panel>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                            </tbody>
                                        </table>
                                    </FooterTemplate>
                                </asp:Repeater>
                                <asp:Label ID="lblNoAdmin" runat="server" Visible="false" CssClass="alert alert-info"
                                    Text="ยังไม่มี Admin กรุณาเพิ่ม Admin" />
                            </div>
                        </div><!-- /tab-admin -->

                        <!-- ===================== TAB: NEW USER ===================== -->
                        <div id="tab-newuser" class="tab-panel">

                            <div class="section-title">🧑‍💼 ยืนยันการลงทะเบียนผู้ใช้ใหม่ (New User Request)</div>

                            <div class="flow-legend" style="margin:5px 10px 12px;">
                                <span style="font-weight:700; color:var(--ink);">สถานะ:</span>
                                <span class="lg-item"><span class="badge badge-approve">New User / ร้องขอเข้าระบบ</span> — ผู้ใช้ใหม่ (status = 50)</span>
                                <span class="lg-item"><span class="badge badge-step">ร้องขอเข้าระบบ</span> — มีบัญชีเดิม ขอสิทธิ์ (status = 1)</span>
                            </div>

                            <div class="table-wrap" style="margin: 5px 10px;">
                                <asp:Repeater ID="rptNewUsers" runat="server" OnItemCommand="rptNewUsers_ItemCommand">
                                    <HeaderTemplate>
                                        <table class="grid">
                                            <thead>
                                                <tr>
                                                    <th style="width:40px;">#</th>
                                                    <th style="width:120px;">Action</th>
                                                    <th>Emp Id</th>
                                                    <th>ชื่อ-นามสกุล</th>
                                                    <th>Division</th>
                                                    <th>Dept</th>
                                                    <th>Position</th>
                                                    <th>Email</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <tr>
                                            <td><%# Container.ItemIndex + 1 %></td>
                                            <td>
                                                <asp:LinkButton ID="btnApprove" runat="server"
                                                    CssClass="btn btn-success btn-sm"
                                                    CommandName="APPROVE_USER"
                                                    CommandArgument='<%# Eval("emp_id") %>'
                                                    OnClientClick='<%# "return confirmApprove(\"" + Eval("emp_id") + "\", \"" + JsEscape(Eval("emp_name")) + "\");" %>'>
                                                    ✓ Approve
                                                </asp:LinkButton>
                                            </td>
                                            <td><strong><%# Eval("emp_id") %></strong></td>
                                            <td><%# Eval("emp_name") %></td>
                                            <td><%# Eval("division") %></td>
                                            <td><%# Eval("dept") %></td>
                                            <td><%# Eval("position") %></td>
                                            <td><%# Eval("email") %></td>
                                        </tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                            </tbody>
                                        </table>
                                    </FooterTemplate>
                                </asp:Repeater>
                                <asp:Label ID="lblNoNewUser" runat="server" Visible="false" CssClass="alert alert-info"
                                    Text="ไม่มีรายการร้องขอลงทะเบียน" />
                            </div>

                        </div><!-- /tab-newuser -->

                    </div><!-- /main-card -->

                </div><!-- /page-wrapper -->

            </div>

            <script>
                // ========== TAB SWITCHING ==========
                function switchTab(tabId) {
                    document.querySelectorAll('.tab-panel').forEach(function (p) { p.classList.remove('active'); });
                    document.querySelectorAll('.tab-btn').forEach(function (b) { b.classList.remove('active'); });
                    var panel = document.getElementById(tabId);
                    if (panel) panel.classList.add('active');
                    document.querySelectorAll('[data-tab="' + tabId + '"]').forEach(function (b) { b.classList.add('active'); });
                    var hf = document.getElementById('<%= hfActiveTab.ClientID %>');
                    if (hf) hf.value = tabId;
                }

                function restoreActiveTab() {
                    var hf = document.getElementById('<%= hfActiveTab.ClientID %>');
                    var activeTab = (hf && hf.value) ? hf.value : 'tab-flow';
                    switchTab(activeTab);
                }

                // โหลดครั้งแรก
                window.onload = restoreActiveTab;

                // ★ สำคัญ: ทำงานทุกครั้งหลัง UpdatePanel postback จบ (กันเด้งกลับ tab-flow)
                if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                        restoreActiveTab();
                    });
                }

                // ===== New User: ยืนยันก่อน Approve =====
                function confirmApprove(empId, empName) {
                    var who = empName ? (empName + " (" + empId + ")") : empId;
                    return confirm("ยืนยันการ Register / อนุมัติสิทธิ์เข้าระบบของ\n" + who + " ?");
                }

                // ===== Edit row CheckBoxList: Super Admin -> Level1-3 =====
                function onLevelClickCBL(container) {
                    var boxes = container.querySelectorAll('input[type=checkbox]');
                    if (boxes.length < 4) return;
                    var l4 = boxes[3];
                    if (l4.checked) { boxes[0].checked = boxes[1].checked = boxes[2].checked = true; }
                    if (!boxes[0].checked || !boxes[1].checked || !boxes[2].checked) { l4.checked = false; }
                }
            </script>

        </ContentTemplate>

    </asp:UpdatePanel>
</asp:Content>