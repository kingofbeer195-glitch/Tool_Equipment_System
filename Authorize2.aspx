<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" 
    CodeFile="Authorize2.aspx.cs"
    Inherits="Tool_Equipment_Control_System.Authorize" %>

<asp:Content ID="Manu_List" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Authorization Management</title>

    <link href="CSS/w3v4.10.css" rel="stylesheet" />
    <link href="CSS/fontsgoogleapis.css" rel="stylesheet" />
    <link href="DataTables-1.10.18/css/dataTables.css" rel="stylesheet" />
    <link href="Content/chosen.css" rel="stylesheet" />

    <%-- ★ jQuery ต้องมาก่อน plugin ทุกตัว --%>
    <script src="Scripts/jquery-1.9.1.js"></script>
    <script src="Scripts/chosen.jquery.js"></script>
    <script src="Scripts/script.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>
    <script src="DataTables-1.10.18/js/dataTables.js"></script>
    <script src="DataTables-1.10.18/FixedColumns-3.2.5/js/fixedColumns.dataTables.js"></script>

    <style>
        /* ========================================================
           AUTHORIZATION MANAGEMENT — UI REFRESH v2
           โทนน้ำเงินองค์กร (คงของเดิม)
           ======================================================== */

        :root {
            --navy-900:#12294d;   --navy-800:#1a3c6e;   --navy-600:#2d6aad;
            --navy-100:#e8f0fe;   --navy-050:#f5f8fd;
            --ink:#2b3648;        --ink-soft:#5a6a80;   --ink-faint:#8a97a8;
            --line:#a4c9fc;       --line-soft:#eef2f7;  --field:#cfd8e3;
            --bg:#eef1f6;         --card:#ffffff;
            --ok:#2e9e5b;         --ok-bg:#e6f6ec;      --ok-ink:#1c703c;
            --warn:#c98a12;       --warn-bg:#fdf4e0;    --warn-ink:#8a5e08;
            --danger:#d64550;     --danger-bg:#fdecec;  --danger-ink:#a12a33;
            --info:#2d6aad;       --info-bg:#e7f1fb;    --info-ink:#1a4a7d;
            --purple:#6f42c1;     --purple-bg:#ede7f6;  --purple-ink:#5e35b1;
            --shadow:0 1px 3px rgba(18,41,77,.06), 0 6px 20px rgba(18,41,77,.07);
            --radius:10px;        --radius-sm:7px;
        }

        body { background:var(--bg); }

        /* ---------- Reset (scope เฉพาะหน้านี้ ไม่กระทบ Master) ---------- */
        .auth-page, .auth-page * { box-sizing:border-box; }
        .auth-page * { margin:0; padding:0; }
        .auth-page {
            font-family:'Segoe UI','Leelawadee UI',Tahoma,sans-serif;
            font-size:14px; color:var(--ink);
            font-variant-numeric:tabular-nums;
        }
        .auth-page a { text-decoration:none; }

        /* ---------- Layout ---------- */
        .page-wrapper { max-width:1320px; margin:0 auto; padding:10px 15px; }

        .page-header {
            background:linear-gradient(120deg, var(--navy-900) 0%, var(--navy-600) 100%);
            color:#fff; padding:10px 25px; border-radius:var(--radius) var(--radius) 0 0;
            position:relative; overflow:hidden;
        }
        .page-header::after {
            content:""; position:absolute; right:-40px; top:-60px;
            width:220px; height:220px; border-radius:50%; background:rgba(255,255,255,.05);
        }
        .page-header .grid-headerTopic1 {
            display:grid; grid-template-columns:64px auto; align-items:center; gap:14px;
            position:relative; z-index:1;
        }
        .page-header .grid-headerTopic2 { display:grid; grid-template-rows:auto auto; align-content:center; }
        .page-header h2 { font-size:21px; font-weight:700; }
        .page-header p  { font-size:13.5px; opacity:.85; margin-top:2px; }
        .page-header img { height:52px; filter:drop-shadow(0 2px 4px rgba(0,0,0,.25)); }

        .main-card {
            background:var(--card); border-radius:0 0 var(--radius) var(--radius);
            box-shadow:var(--shadow); padding:14px 18px 20px;
        }
        .grid-headerTopic1, .grid-headerTopic2 { display:grid; }

        /* ---------- Tabs (segmented) ---------- */
        .tabs {
            display:flex; gap:4px; padding:5px; margin-bottom:16px; overflow-x:auto;
            background:var(--navy-050); border:1px solid var(--line); border-radius:var(--radius);
        }
        .tab-btn {
            flex:0 0 auto; padding:9px 16px; border:none; background:transparent; cursor:pointer;
            font-family:inherit; font-size:14px; font-weight:600; color:var(--ink-soft);
            border-radius:var(--radius-sm); display:inline-flex; align-items:center; gap:8px;
            transition:background .15s, color .15s, box-shadow .15s;
        }
        .tab-btn:hover { background:#fff; color:var(--navy-600); }
        .tab-btn.active { background:#fff; color:var(--navy-800); box-shadow:0 1px 3px rgba(18,41,77,.14); }
        .tab-btn:focus-visible { outline:2px solid var(--navy-600); outline-offset:2px; }
        .tab-count {
            min-width:20px; height:20px; padding:0 6px; border-radius:10px;
            background:var(--danger); color:#fff; font-size:11px; font-weight:700;
            display:inline-flex; align-items:center; justify-content:center;
        }
        .tab-count:empty { display:none; }

        .tab-panel { display:none; }
        .tab-panel.active { display:block; animation:fadeIn .15s ease; }
        #tab-flow.tab-panel { background:#d7e9f0; padding:5px; }   /* คงลุคเดิมของแท็บ Flow */
        @keyframes fadeIn { from{opacity:0;} to{opacity:1;} }

        /* ---------- Section header (Admin / New User) ---------- */
        .sec-head {
            display:flex; justify-content:space-between; align-items:center; gap:12px; flex-wrap:wrap;
            margin:20px 2px 10px;
        }
        .sec-head:first-child { margin-top:4px; }
        .sec-head h3 { font-size:16px; font-weight:700; color:var(--navy-800); display:flex; align-items:center; gap:8px; }
        .sec-tools { display:flex; align-items:center; gap:10px; flex-wrap:wrap; }
        .count-pill {
            font-size:12px; font-weight:700; padding:2px 9px; border-radius:20px;
            background:var(--navy-100); color:var(--navy-800);
        }
        .hint { font-size:12px; color:var(--ink-faint); }
        .text-warn { color:var(--warn-ink); font-weight:600; font-size:12px; }

        .search-box { position:relative; display:inline-block; }
        .search-box::before {
            content:"\1F50D"; position:absolute; left:11px; top:50%; transform:translateY(-50%);
            font-size:12px; opacity:.55; pointer-events:none;
        }
        .search-box input {
            width:260px; padding:7px 12px 7px 32px; font-family:inherit; font-size:13px;
            border:1px solid var(--field); border-radius:20px; background:#fff;
        }
        .search-box input:focus { outline:none; border-color:var(--navy-600); box-shadow:0 0 0 3px rgba(45,106,173,.14); }

        /* ---------- Form controls (ใช้ร่วมกับ Flow) ---------- */
        .form-row { display:flex; flex-wrap:wrap; gap:14px; margin-bottom:14px; align-items:flex-end; }
        .form-group { display:flex; flex-direction:column; gap:5px; }
        .form-group label { font-size:12px; font-weight:600; color:var(--ink-soft); }
        .form-control {
            padding:4px 11px; border:1px solid var(--field); border-radius:var(--radius-sm);
            font-size:13px; min-width:160px; background:#fff; text-align:center;
            transition:border-color .15s, box-shadow .15s;
        }
        .form-control:focus { outline:none; border-color:var(--navy-600); box-shadow:0 0 0 3px rgba(45,106,173,.14); }
        .form-control[readonly] { background:#f4f6f9; color:var(--ink-soft); }
        select.form-control { min-width:200px; height:35px !important; }

        /* ---------- Buttons ---------- */
        .btn {
            padding:8px 16px; border:none; border-radius:var(--radius-sm); cursor:pointer;
            font-family:inherit; font-size:13px; font-weight:600; line-height:1.2;
            display:inline-flex; align-items:center; gap:6px; white-space:nowrap;
            transition:filter .15s, background .15s, transform .05s;
        }
        .btn:active { transform:translateY(1px); }
        .btn:focus-visible { outline:2px solid var(--navy-600); outline-offset:2px; }
        .btn-primary   { background:var(--navy-600); color:#fff; }
        .btn-primary:hover { background:var(--navy-800); color:#fff; }
        .btn-success   { background:var(--ok); color:#fff; box-shadow:0 2px 6px rgba(46,158,91,.25); }
        .btn-success:hover { filter:brightness(1.06); color:#fff; }
        .btn-warning   { background:var(--warn-bg); color:var(--warn-ink); border:1px solid #f0dcae; }
        .btn-warning:hover { background:#fbecca; }
        .btn-danger    { background:var(--danger-bg); color:var(--danger-ink); border:1px solid #f4c9cc; }
        .btn-danger:hover { background:#fbdcde; }
        .btn-secondary { background:#eef1f6; color:var(--ink-soft); border:1px solid var(--field); }
        .btn-secondary:hover { background:#e2e7ef; }
        .btn-sm { padding:5px 11px; font-size:12px; }
        .btn-xs { padding:3px 9px; font-size:11px; }

        /* ---------- Alert ---------- */
        .alert {
            padding:11px 15px; border-radius:var(--radius-sm); font-size:13px; margin-bottom:14px;
            border:1px solid transparent; display:flex; align-items:center; gap:9px;
        }
        .alert::before { font-size:15px; line-height:1; }
        .alert-success { background:var(--ok-bg); color:var(--ok-ink); border-color:#bfe6cd; }
        .alert-success::before { content:"\2714"; }
        .alert-danger  { background:var(--danger-bg); color:var(--danger-ink); border-color:#f4c9cc; }
        .alert-danger::before { content:"\26A0"; }
        .alert-info    { background:var(--info-bg); color:var(--info-ink); border-color:#c5ddf3; }
        .alert-info::before { content:"\2139"; }
        .alert-warn    { background:var(--warn-bg); color:var(--warn-ink); border-color:#f0dcae; }
        .alert-warn::before { content:"\26A0"; }

        /* ---------- Table ---------- */
        .table-wrap { overflow-x:auto; border:1px solid var(--line); border-radius:var(--radius-sm); background:#fff; }
        table.grid { width:100%; border-collapse:collapse; font-size:13px; }
        table.grid th {
            background:var(--navy-050); color:var(--navy-800); padding:10px 14px; text-align:left;
            font-weight:700; white-space:nowrap; border-bottom:2px solid var(--line); font-size:12.5px;
        }
        table.grid td { padding:10px 14px; border-bottom:1px solid var(--line-soft); vertical-align:middle; }
        table.grid tr:last-child td { border-bottom:none; }
        table.grid tbody tr:hover td { background:var(--navy-050); }
        table.grid .col-no { width:44px; color:var(--ink-faint); }
        table.grid .col-action { text-align:right; white-space:nowrap; }
        table.grid tr.row-editing td { background:#fffbea; }
        table.grid tr.row-editing td:first-child { box-shadow:inset 3px 0 0 var(--warn); }

        /* sticky action column (New User) */
        table.grid .col-sticky { position:sticky; right:0; background:#fff; box-shadow:-6px 0 8px -6px rgba(18,41,77,.15); }
        table.grid th.col-sticky { background:var(--navy-050); }

        .person { display:flex; align-items:center; gap:10px; min-width:200px; }
        .avatar {
            width:34px; height:34px; border-radius:50%; flex:none;
            background:var(--navy-100); color:var(--navy-800); font-weight:800; font-size:14px;
            display:flex; align-items:center; justify-content:center;
        }
        .person-name { font-weight:600; color:var(--ink); }
        .person-sub  { font-size:12px; color:var(--ink-faint); margin-top:1px; }

        .actions { display:inline-flex; gap:6px; flex-wrap:wrap; justify-content:flex-end; }

        .empty-state {
            display:block; text-align:center; padding:30px 16px; color:var(--ink-faint);
            font-size:13px; background:#fff;
        }
        .empty-state::before { content:"\1F4ED"; display:block; font-size:28px; margin-bottom:6px; }

        /* ---------- Badges ---------- */
        .badge { display:inline-block; padding:3px 10px; border-radius:20px; font-size:11.5px; font-weight:700; white-space:nowrap; }
        .badge-auto    { background:var(--info-bg); color:var(--info-ink); }
        .badge-manual  { background:var(--purple-bg); color:var(--purple-ink); }
        .badge-skip    { background:#eef1f5; color:var(--ink-faint); }
        .badge-admin   { background:var(--purple-bg); color:var(--purple-ink); }
        .badge-approve { background:var(--info-bg); color:var(--info-ink); }
        .badge-active  { background:var(--ok-bg); color:var(--ok-ink); }
        .badge-step    { background:var(--warn-bg); color:var(--warn-ink); }

        /* ========================================================
           FLOW AUTHORIZATION — (คงเดิม ไม่แก้)
           ======================================================== */
        .section-title {
            font-size:15px; font-weight:600; color:var(--navy-800);
            margin-bottom:8px; border-bottom:1px solid var(--line);
            display:flex; align-items:center; gap:8px; padding:5px 5px;
        }
        .panel-add { background:var(--navy-050); border:1px solid var(--line); border-radius:var(--radius-sm); padding:18px 20px; margin:5px 10px 10px; }
        .panel-add-title { font-size:13px; font-weight:700; color:var(--navy-800); margin-bottom:12px; display:flex; align-items:center; gap:7px; }
        .flow-legend {
            display:flex; flex-wrap:wrap; gap:18px; padding:12px; margin:5px 10px 12px;
            background:#fff; border:1px solid var(--line); border-radius:var(--radius-sm);
            font-size:12px; color:var(--ink-soft);
        }
        .flow-legend .lg-item { display:flex; align-items:center; gap:8px; }
        .flow-legend .lg-swatch { width:14px; height:14px; border-radius:4px; flex:none; }
        .flow-legend .lg-manual { background:#7e57c2; }
        .flow-legend .lg-auto   { background:var(--navy-600); }
        .flow-legend .lg-skip   { background:#c3ccd8; }
        .flow-legend b { color:var(--ink); font-weight:700; }
        .flow-toolbar { display:flex; justify-content:space-between; align-items:center; gap:14px; margin:5px 10px 12px; flex-wrap:wrap; }
        .flow-toolbar .section-title { margin-bottom:0; border:none; padding:0; }
        .flow-process-name { color:var(--navy-600); }
        .step-container { position:relative; margin:5px 10px; }
        .step-item {
            position:relative; margin-left:34px; margin-bottom:0;
            border:1px solid var(--line); border-left:4px solid var(--navy-600);
            border-radius:var(--radius-sm); background:#fff; padding:14px 16px;
            box-shadow:0 1px 2px rgba(18,41,77,.04);
        }
        .step-item.is-manual { border-left-color:#7e57c2; }
        .step-item.is-auto   { border-left-color:var(--navy-600); }
        .step-item.is-skip   { border-left-color:#c3ccd8; }
        .step-header { display:flex; align-items:center; justify-content:space-between; margin-bottom:10px; gap:10px; }
        .step-title { display:flex; align-items:center; gap:10px; font-size:14px; font-weight:600; color:var(--ink); }
        .step-num {
            position:absolute; left:-38px; top:10px; width:30px; height:30px; background:#fff; color:var(--navy-800);
            border:2px solid var(--navy-600); border-radius:50%; display:flex; align-items:center; justify-content:center;
            font-size:13px; font-weight:800; box-shadow:0 1px 3px rgba(18,41,77,.12); z-index:2;
        }
        .is-manual .step-num { border-color:#7e57c2; color:#5e35b1; }
        .is-skip   .step-num { border-color:#c3ccd8; color:var(--ink-faint); }
        .flow-arrow { position:relative; height:22px; margin-left:34px; }
        .flow-arrow::before { content:""; position:absolute; left:-19px; top:-2px; bottom:-2px; width:2px; background:var(--line); }
        .flow-arrow > * { display:none; }
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
           ADMIN MANAGEMENT
           ======================================================== */

        /* Level guide */
        .level-guide { display:grid; grid-template-columns:repeat(4, 1fr); gap:10px; }
        .lvl-card {
            border:1px solid var(--line-soft); border-left:4px solid var(--navy-600);
            border-radius:var(--radius-sm); padding:10px 12px; background:#fff;
        }
        .lvl-card.tier-1 { border-left-color:#4a90d9; }
        .lvl-card.tier-2 { border-left-color:#2d6aad; }
        .lvl-card.tier-3 { border-left-color:#1a3c6e; }
        .lvl-card.tier-4 { border-left-color:var(--purple); background:#faf7ff; }
        .lvl-card .lvl-head { display:flex; align-items:center; gap:8px; margin-bottom:6px; }
        .lvl-card .lvl-chip {
            font-size:11px; font-weight:800; padding:2px 8px; border-radius:20px;
            background:var(--navy-100); color:var(--navy-800);
        }
        .lvl-card.tier-4 .lvl-chip { background:var(--purple-bg); color:var(--purple-ink); }
        .lvl-card .lvl-name { font-size:13.5px; font-weight:700; color:var(--ink); }
        .lvl-card ul { list-style:none; }
        .lvl-card li { font-size:12.5px; color:var(--ink-soft); line-height:1.6; padding-left:12px; position:relative; }
        .lvl-card li::before { content:""; position:absolute; left:0; top:.65em; width:5px; height:5px; border-radius:50%; background:var(--line); }

        /* Add admin — 3 steps */
        .add-card {
            border:1px solid var(--line); border-radius:var(--radius); background:#fff;
            padding:16px 18px; box-shadow:0 1px 2px rgba(18,41,77,.04);
        }
        .add-step { display:flex; gap:14px; align-items:flex-start; }
        .add-step + .add-step,
        .add-step-group .add-step { margin-top:16px; padding-top:16px; border-top:1px dashed var(--line); }
        .step-dot {
            width:26px; height:26px; border-radius:50%; flex:none; margin-top:1px;
            background:var(--navy-600); color:#fff; font-size:12.5px; font-weight:800;
            display:flex; align-items:center; justify-content:center;
        }
        .add-step-body { flex:1; min-width:0; }
        .add-step-title { font-size:13.5px; font-weight:700; color:var(--navy-800); margin-bottom:8px; }
        .id-row { display:flex; align-items:center; gap:10px; flex-wrap:wrap; }
        .emp-id-input {
            width:170px; min-width:0; height:40px; font-size:18px; font-weight:700;
            letter-spacing:3px; color:var(--navy-800);
        }

        .emp-card {
            display:grid; grid-template-columns:repeat(auto-fit, minmax(160px, 1fr)); gap:12px 16px;
            background:var(--navy-050); border:1px solid var(--line); border-radius:var(--radius-sm); padding:12px 14px;
        }
        .emp-field label { display:block; font-size:11.5px; font-weight:600; color:var(--ink-faint); margin-bottom:2px; }
        .emp-field .form-control,
        .emp-field .form-control[readonly] {
            width:100% !important; min-width:0; padding:0; border:none; background:transparent; box-shadow:none;
            text-align:left; font-size:14px; font-weight:600; color:var(--ink); height:auto;
        }

        .add-actions { display:flex; gap:8px; margin-top:14px; flex-wrap:wrap; }

        /* Level chips (CheckBoxList → ชิปกดเลือก; JS onLevelClickCBL ยังทำงานเหมือนเดิม) */
        .lvl-chips { display:flex; flex-wrap:wrap; gap:8px; }
        .lvl-chips > span { display:inline-flex; position:relative; }
        .lvl-chips input[type=checkbox] { position:absolute; opacity:0; width:1px; height:1px; }
        .lvl-chips label {
            display:inline-flex; align-items:center; gap:7px; cursor:pointer; user-select:none;
            padding:7px 14px; border:1.5px solid var(--field); border-radius:20px; background:#fff;
            color:var(--ink-soft); font-size:13px; font-weight:600; transition:border-color .12s, background .12s, color .12s;
        }
        .lvl-chips label::before {
            content:"\2713"; width:15px; height:15px; border-radius:4px; flex:none;
            border:1.5px solid #b6c3d4; background:#fff; color:transparent;
            font-size:11px; line-height:13px; text-align:center;
        }
        .lvl-chips label:hover { border-color:var(--navy-600); color:var(--navy-800); }
        .lvl-chips input:checked + label { border-color:var(--navy-600); background:var(--navy-100); color:var(--navy-800); }
        .lvl-chips input:checked + label::before { background:var(--navy-600); border-color:var(--navy-600); color:#fff; }
        .lvl-chips input:focus-visible + label { outline:2px solid var(--navy-600); outline-offset:2px; }
        /* Super Admin = ชิปสุดท้าย */
        .lvl-chips > span:last-child input:checked + label { border-color:var(--purple); background:var(--purple-bg); color:var(--purple-ink); }
        .lvl-chips > span:last-child input:checked + label::before { background:var(--purple); border-color:var(--purple); }
        /* edit row = ชิปเล็ก */
        .lvl-chips-sm { gap:6px; }
        .lvl-chips-sm label { padding:4px 10px; font-size:12px; }

        /* Level badges ในตาราง */
        .lvl-badge { display:inline-block; padding:3px 10px; border-radius:20px; font-size:11.5px; font-weight:700; margin:2px 4px 2px 0; cursor:default; }
        .lvl-on    { background:var(--navy-100); color:var(--navy-800); border:1px solid #c9dbf5; }
        .lvl-super { background:var(--purple-bg); color:var(--purple-ink); border:1px solid #d6c8f0; }
        .lvl-off   { background:#f2f4f7; color:var(--ink-faint); border:1px solid #e8ecf1; }
        .lvl-note  { font-size:11.5px; color:var(--ink-faint); margin-left:2px; }

        /* ========================================================
           NEW USER
           ======================================================== */
        .info-strip {
            display:flex; flex-wrap:wrap; gap:8px 20px; align-items:center; padding:10px 14px; margin-bottom:10px;
            background:var(--navy-050); border:1px solid var(--line); border-radius:var(--radius-sm);
            font-size:12.5px; color:var(--ink-soft);
        }
        .info-strip > span { display:inline-flex; align-items:center; gap:7px; }

        /* ---------- Responsive ---------- */
        @media (max-width:920px) { .level-guide { grid-template-columns:repeat(2, 1fr); } }
        @media (max-width:768px) {
            .form-row { flex-direction:column; align-items:stretch; }
            .form-control { min-width:100%; }
            .flow-toolbar { align-items:flex-start; }
            .sec-tools, .search-box, .search-box input { width:100%; }
            .add-step { gap:10px; }
        }
        @media (max-width:560px) { .level-guide { grid-template-columns:1fr; } }
        @media (prefers-reduced-motion:reduce) { .tab-panel.active { animation:none; } }
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

    <%-- Loading Overlay (อยู่นอก UpdatePanel → ไม่ถูก replace ตอน async postback) --%>
    <div id="loadingOverlay" style="display:none; position:fixed; z-index:9999; top:0; left:0;
        width:100%; height:100%; background:rgba(0,0,0,0.6); color:white; text-align:center; font-size:20px;">
        <div class="loading">
            <svg class="pl" width="240" height="240" viewBox="0 0 240 240">
                <circle class="pl__ring pl__ring--a" cx="120" cy="120" r="105" fill="none" stroke="#000" stroke-width="20" stroke-dasharray="0 660" stroke-dashoffset="-330" stroke-linecap="round"></circle>
                <circle class="pl__ring pl__ring--b" cx="120" cy="120" r="35" fill="none" stroke="#000" stroke-width="20" stroke-dasharray="0 220" stroke-dashoffset="-110" stroke-linecap="round"></circle>
                <circle class="pl__ring pl__ring--c" cx="85" cy="120" r="70" fill="none" stroke="#000" stroke-width="20" stroke-dasharray="0 440" stroke-linecap="round"></circle>
                <circle class="pl__ring pl__ring--d" cx="155" cy="120" r="70" fill="none" stroke="#000" stroke-width="20" stroke-dasharray="0 440" stroke-linecap="round"></circle>
            </svg>
            <div style="margin-top:20px; font-size:18px;">กำลังประมวลผล...</div>
        </div>
    </div>

    <asp:ScriptManager runat="server" ID="scriptManager" EnablePageMethods="true"></asp:ScriptManager>

    <%-- ★ Script ทั้งหมดอยู่ "หลัง" ScriptManager → Sys พร้อมใช้แล้ว --%>
    <script>
        // ===== Loading =====
        function showLoading() { document.getElementById('loadingOverlay').style.display = 'block'; }
        function hideLoading() { document.getElementById('loadingOverlay').style.display = 'none'; }

        // ===== Tabs =====
        function switchTab(tabId) {
            document.querySelectorAll('.tab-panel').forEach(function (p) { p.classList.remove('active'); });
            document.querySelectorAll('.tab-btn').forEach(function (b) {
                b.classList.remove('active'); b.setAttribute('aria-selected', 'false');
            });
            var panel = document.getElementById(tabId);
            if (panel) panel.classList.add('active');
            document.querySelectorAll('[data-tab="' + tabId + '"]').forEach(function (b) {
                b.classList.add('active'); b.setAttribute('aria-selected', 'true');
            });
            var hf = document.getElementById('<%= hfActiveTab.ClientID %>');
            if (hf) hf.value = tabId;
        }
        function restoreActiveTab() {
            var hf = document.getElementById('<%= hfActiveTab.ClientID %>');
            switchTab((hf && hf.value) ? hf.value : 'tab-flow');
        }

        // ===== New User: ยืนยันก่อน Approve =====
        function confirmApprove(empId, empName) {
            var who = empName ? (empName + " (" + empId + ")") : empId;
            return confirm("ยืนยันอนุมัติให้เข้าใช้ระบบ\n" + who + " ?");
        }

        // ===== Level: Super Admin -> ติ๊ก Level 1-3 =====
        function onLevelClickCBL(container) {
            var boxes = container.querySelectorAll('input[type=checkbox]');
            if (boxes.length < 4) return;
            var l4 = boxes[3];
            if (l4.checked) { boxes[0].checked = boxes[1].checked = boxes[2].checked = true; }
            if (!boxes[0].checked || !boxes[1].checked || !boxes[2].checked) { l4.checked = false; }
        }

        // ===== ค้นหาในตาราง (client-side) =====
        function filterTable(input, tableId, infoId) {
            var q = (input.value || '').toLowerCase().replace(/^\s+|\s+$/g, '');
            var tbl = document.getElementById(tableId);
            if (!tbl || !tbl.tBodies.length) return;
            var rows = tbl.tBodies[0].rows, shown = 0;
            for (var i = 0; i < rows.length; i++) {
                var hit = !q || rows[i].textContent.toLowerCase().indexOf(q) > -1;
                rows[i].style.display = hit ? '' : 'none';
                if (hit) shown++;
            }
            var info = document.getElementById(infoId);
            if (info) info.textContent = q ? ('พบ ' + shown + ' จาก ' + rows.length + ' รายการ') : '';
        }

        // ===== Hook =====
        window.addEventListener('load', restoreActiveTab);   // ไม่ทับ window.onload ของ Master
        (function () {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.add_beginRequest(showLoading);
            prm.add_endRequest(function () { hideLoading(); restoreActiveTab(); });
        })();
    </script>

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
                <div class="page-wrapper auth-page">

                    <!-- Header -->
                    <div class="page-header" style="margin:20px 0px 0px;">
                        <div class="grid-headerTopic1">
                            <img src="images/key-person.png" alt="" />
                            <div class="grid-headerTopic2">
                                <div><h2>Authorization Management</h2></div>
                                <div><p>จัดการสิทธิ์ Admin, Flow การอนุมัติ และคำขอเข้าใช้ระบบ</p></div>
                            </div>
                        </div>
                    </div>

                    <div class="main-card">

                        <!-- Alert -->
                        <asp:Panel ID="pnlAlert" runat="server" Visible="false">
                            <div id="divAlert" runat="server" class="alert"></div>
                        </asp:Panel>

                        <!-- ★ แจ้งเตือนเมื่อเปิด TEST_MODE ส่งเมล -->
                        <asp:Panel ID="pnlTestMode" runat="server" Visible="false">
                            <div class="alert alert-warn">
                                โหมดทดสอบ: Email แจ้งผลการอนุมัติจะถูกส่งไปที่ผู้ทดสอบเท่านั้น ไม่ส่งถึงผู้ใช้จริง
                            </div>
                        </asp:Panel>

                        <!-- Hidden fields (เดิม) -->
                        <asp:HiddenField ID="hfActiveTab"     runat="server" Value="tab-flow" />
                        <asp:HiddenField ID="hfEditFlowId"    runat="server" Value="" />
                        <asp:HiddenField ID="hfFlowStepsJson" runat="server" Value="[]" />

                        <!-- ===================== TABS ===================== -->
                        <div class="tabs" role="tablist">
                            <button type="button" class="tab-btn" role="tab" data-tab="tab-flow"  onclick="switchTab('tab-flow')">🔀 Flow Authorization</button>
                            <button type="button" class="tab-btn" role="tab" data-tab="tab-admin" onclick="switchTab('tab-admin')">🛡️ Admin Management</button>
                            <button type="button" class="tab-btn" role="tab" data-tab="tab-newuser" onclick="switchTab('tab-newuser')">
                                🧑‍💼 New User
                                <span class="tab-count" title="คำขอที่รออนุมัติ"><asp:Literal ID="litNewUserCount" runat="server" /></span>
                            </button>
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
                        <div id="tab-admin" class="tab-panel" role="tabpanel">

                            <!-- ===== ระดับสิทธิ์ ===== -->
                            <div class="sec-head">
                                <h3>📖 ระดับสิทธิ์</h3>
                                <span class="hint">เลือก Super Admin แล้วระบบจะให้สิทธิ์ Level 1–3 อัตโนมัติ</span>
                            </div>
                            <div class="level-guide">
                                <div class="lvl-card tier-1">
                                    <div class="lvl-head"><span class="lvl-chip">Level 1</span><span class="lvl-name">Data Admin</span></div>
                                    <ul><li>แก้ไขและจัดการรายการอุปกรณ์</li><li>อนุมัติคำร้องในระบบ</li></ul>
                                </div>
                                <div class="lvl-card tier-2">
                                    <div class="lvl-head"><span class="lvl-chip">Level 2</span><span class="lvl-name">Tana Admin</span></div>
                                    <ul><li>ตั้งค่า Tanaoroshi</li><li>ดูแลระบบ Tanaoroshi</li></ul>
                                </div>
                                <div class="lvl-card tier-3">
                                    <div class="lvl-head"><span class="lvl-chip">Level 3</span><span class="lvl-name">Flow Admin</span></div>
                                    <ul><li>กำหนดผู้อนุมัติของแต่ละ Process</li><li>กำหนดสิทธิ์ Admin</li></ul>
                                </div>
                                <div class="lvl-card tier-4">
                                    <div class="lvl-head"><span class="lvl-chip">👑 Super Admin</span></div>
                                    <ul><li>ได้สิทธิ์ Level 1–3 ทั้งหมด</li></ul>
                                </div>
                            </div>

                            <!-- ===== เพิ่ม Admin ===== -->
                            <div class="sec-head"><h3>➕ เพิ่ม Admin</h3></div>
                            <asp:Panel ID="pnlAddAdmin" runat="server" CssClass="add-card">

                                <!-- Step 1 : ค้นหา -->
                                <asp:Panel ID="pnlSearchEmp" runat="server" CssClass="add-step" DefaultButton="btnSearchEmp">
                                    <span class="step-dot">1</span>
                                    <div class="add-step-body">
                                        <div class="add-step-title">ค้นหาพนักงาน</div>
                                        <div class="id-row">
                                            <asp:TextBox ID="txtEmpId" runat="server" CssClass="form-control emp-id-input"
                                                MaxLength="6" placeholder="000000" autocomplete="off" />
                                            <asp:LinkButton ID="btnSearchEmp" runat="server" CssClass="btn btn-primary"
                                                OnClick="btnSearchEmp_Click">🔍 ค้นหา</asp:LinkButton>
                                            <span class="hint">กรอกรหัสพนักงาน 6 หลัก แล้วกด Enter หรือ ค้นหา</span>
                                        </div>
                                    </div>
                                </asp:Panel>

                                <asp:HiddenField ID="hdtxtBand"  runat="server" />
                                <asp:HiddenField ID="hdtxtEmail" runat="server" />

                                <!-- Step 2-3 : แสดงหลังค้นหาสำเร็จ -->
                                <asp:Panel ID="pnlAddLevelWrap" runat="server" CssClass="add-step-group" style="display:none;">

                                    <div class="add-step">
                                        <span class="step-dot">2</span>
                                        <div class="add-step-body">
                                            <div class="add-step-title">ตรวจสอบข้อมูลพนักงาน</div>
                                            <div class="emp-card">
                                                <div class="emp-field"><label>ชื่อ-นามสกุล</label>
                                                    <asp:TextBox ID="txtEmpName" runat="server" CssClass="form-control" ReadOnly="true" /></div>
                                                <div class="emp-field"><label>ตำแหน่ง</label>
                                                    <asp:TextBox ID="txtPosition" runat="server" CssClass="form-control" ReadOnly="true" /></div>
                                                <div class="emp-field"><label>แผนก</label>
                                                    <asp:TextBox ID="txtDept" runat="server" CssClass="form-control" ReadOnly="true" /></div>
                                                <div class="emp-field"><label>Dept Code</label>
                                                    <asp:TextBox ID="txtDeptCode" runat="server" CssClass="form-control" ReadOnly="true" /></div>
                                            </div>
                                        </div>
                                    </div>

                                    <div class="add-step">
                                        <span class="step-dot">3</span>
                                        <div class="add-step-body">
                                            <div class="add-step-title">เลือกระดับสิทธิ์ (อย่างน้อย 1 ระดับ)</div>
                                            <asp:CheckBoxList ID="cblAddLevel" runat="server"
                                                RepeatDirection="Horizontal" RepeatLayout="Flow"
                                                CssClass="lvl-cbl lvl-add-cbl lvl-chips" />
                                            <div class="add-actions">
                                                <asp:LinkButton ID="btnSaveAdmin" runat="server" CssClass="btn btn-success"
                                                    OnClick="btnSaveAdmin_Click">✓ บันทึก Admin</asp:LinkButton>
                                                <asp:LinkButton ID="btnClearEmp" runat="server" CssClass="btn btn-secondary"
                                                    OnClick="btnClearEmp_Click">ล้างฟอร์ม</asp:LinkButton>
                                            </div>
                                        </div>
                                    </div>

                                </asp:Panel>
                            </asp:Panel>

                            <!-- ===== รายการ Admin ===== -->
                            <div class="sec-head">
                                <h3>🛡️ รายการ Admin <span class="count-pill"><asp:Literal ID="litAdminCount" runat="server" /></span></h3>
                                <div class="sec-tools">
                                    <span id="admFilterInfo" class="hint"></span>
                                    <span class="search-box">
                                        <input type="text" placeholder="ค้นหารหัส ชื่อ หรือแผนก"
                                            onkeyup="filterTable(this,'tblAdmins','admFilterInfo')"
                                            onkeydown="return event.keyCode !== 13;" />
                                    </span>
                                </div>
                            </div>

                            <div class="table-wrap">
                                <asp:Repeater ID="rptAdmins" runat="server" OnItemCommand="rptAdmins_ItemCommand">
                                    <HeaderTemplate>
                                        <table class="grid" id="tblAdmins">
                                            <thead>
                                                <tr>
                                                    <th class="col-no">#</th>
                                                    <th>พนักงาน</th>
                                                    <th>แผนก</th>
                                                    <th>ระดับสิทธิ์</th>
                                                    <th class="col-action">จัดการ</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <tr class='<%# IsEditing((string)Eval("emp_id")) ? "row-editing" : "" %>'>
                                            <td class="col-no"><%# Container.ItemIndex + 1 %></td>
                                            <td>
                                                <div class="person">
                                                    <span class="avatar"><%# GetInitial(Eval("fname")) %></span>
                                                    <div>
                                                        <div class="person-name"><%# Server.HtmlEncode(Eval("fname") + " " + Eval("lname")) %></div>
                                                        <div class="person-sub"><%# Eval("emp_id") %></div>
                                                    </div>
                                                </div>
                                            </td>
                                            <td><%# Eval("dept_code") %></td>
                                            <td>
                                                <asp:Panel ID="pnlLevelRead" runat="server" Visible='<%# !IsEditing((string)Eval("emp_id")) %>'>
                                                    <asp:Literal ID="litLevels" runat="server"
                                                        Text='<%# BuildLevelBadges(Eval("level_1"), Eval("level_2"), Eval("level_3"), Eval("level_4")) %>' />
                                                </asp:Panel>
                                                <asp:Panel ID="pnlLevelEdit" runat="server" Visible='<%# IsEditing((string)Eval("emp_id")) %>'>
                                                    <asp:CheckBoxList ID="cblEditLevel" runat="server" RepeatDirection="Horizontal"
                                                        RepeatLayout="Flow" CssClass="lvl-cbl lvl-chips lvl-chips-sm" />
                                                </asp:Panel>
                                            </td>
                                            <td class="col-action">
                                                <asp:Panel ID="pnlBtnRead" runat="server" CssClass="actions" Visible='<%# !IsEditing((string)Eval("emp_id")) %>'>
                                                    <asp:LinkButton ID="btnEditAdmin" runat="server"
                                                        CommandName="EDIT_ADMIN" CommandArgument='<%# Eval("emp_id") %>'
                                                        CssClass="btn btn-warning btn-sm">✎ แก้ไข</asp:LinkButton>
                                                    <asp:LinkButton ID="btnDeleteAdmin" runat="server"
                                                        CommandName="DELETE_ADMIN" CommandArgument='<%# Eval("emp_id") %>'
                                                        CssClass="btn btn-danger btn-sm"
                                                        OnClientClick='<%# "return confirm(\"ลบสิทธิ์ Admin ของ " + JsEscape(Eval("fname") + " " + Eval("lname")) + " (" + Eval("emp_id") + ") ?\");" %>'>🗑 ลบ</asp:LinkButton>
                                                </asp:Panel>
                                                <asp:Panel ID="pnlBtnEdit" runat="server" CssClass="actions" Visible='<%# IsEditing((string)Eval("emp_id")) %>'>
                                                    <asp:LinkButton ID="btnSaveEdit" runat="server"
                                                        CommandName="SAVE_ADMIN" CommandArgument='<%# Eval("emp_id") %>'
                                                        CssClass="btn btn-success btn-sm">✓ บันทึก</asp:LinkButton>
                                                    <asp:LinkButton ID="btnCancelEdit" runat="server"
                                                        CommandName="CANCEL_ADMIN" CommandArgument='<%# Eval("emp_id") %>'
                                                        CssClass="btn btn-secondary btn-sm">ยกเลิก</asp:LinkButton>
                                                </asp:Panel>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                            </tbody>
                                        </table>
                                    </FooterTemplate>
                                </asp:Repeater>
                                <asp:Label ID="lblNoAdmin" runat="server" Visible="false" CssClass="empty-state"
                                    Text="ยังไม่มี Admin — เพิ่มได้จากฟอร์มด้านบน" />
                            </div>

                        </div><!-- /tab-admin -->

                        <!-- ===================== TAB: NEW USER ===================== -->
                        <div id="tab-newuser" class="tab-panel" role="tabpanel">

                            <div class="sec-head">
                                <h3>🧑‍💼 คำขอเข้าใช้ระบบ <span class="count-pill"><asp:Literal ID="litNewUserTotal" runat="server" /></span></h3>
                                <div class="sec-tools">
                                    <span id="nuFilterInfo" class="hint"></span>
                                    <span class="search-box">
                                        <input type="text" placeholder="ค้นหารหัส ชื่อ แผนก หรือ Email"
                                            onkeyup="filterTable(this,'tblNewUsers','nuFilterInfo')"
                                            onkeydown="return event.keyCode !== 13;" />
                                    </span>
                                </div>
                            </div>

                            <div class="info-strip">
                                <span><span class="badge badge-approve">🆕 New User</span> ผู้ใช้ใหม่ ยังไม่มีบัญชี</span>
                                <span><span class="badge badge-step">🔑 ขอสิทธิ์</span> มีบัญชีเดิม ขอสิทธิ์ใช้ระบบนี้</span>
                                <span>✉️ เมื่ออนุมัติ ระบบส่ง Email แจ้งผู้ใช้อัตโนมัติ</span>
                            </div>

                            <div class="table-wrap">
                                <asp:Repeater ID="rptNewUsers" runat="server" OnItemCommand="rptNewUsers_ItemCommand">
                                    <HeaderTemplate>
                                        <table class="grid" id="tblNewUsers">
                                            <thead>
                                                <tr>
                                                    <th class="col-no">#</th>
                                                    <th>ผู้ร้องขอ</th>
                                                    <th>ประเภทคำขอ</th>
                                                    <th>Division</th>
                                                    <th>Dept</th>
                                                    <th>Position</th>
                                                    <th class="col-action col-sticky">อนุมัติ</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <tr>
                                            <td class="col-no"><%# Container.ItemIndex + 1 %></td>
                                            <td>
                                                <div class="person">
                                                    <span class="avatar"><%# GetInitial(Eval("emp_name")) %></span>
                                                    <div>
                                                        <div class="person-name"><%# Server.HtmlEncode(Convert.ToString(Eval("emp_name"))) %></div>
                                                        <div class="person-sub"><%# Eval("emp_id") %> &nbsp;|&nbsp; <%# FormatEmail(Eval("email")) %></div>
                                                    </div>
                                                </div>
                                            </td>
                                            <td><%# GetReqStatusBadge(Eval("req_status")) %></td>
                                            <td><%# Eval("division") %></td>
                                            <td><%# Eval("dept") %></td>
                                            <td><%# Eval("position") %></td>
                                            <td class="col-action col-sticky">
                                                <asp:LinkButton ID="btnApprove" runat="server"
                                                    CssClass="btn btn-success btn-sm"
                                                    CommandName="APPROVE_USER"
                                                    CommandArgument='<%# Eval("emp_id") %>'
                                                    OnClientClick='<%# "return confirmApprove(\"" + Eval("emp_id") + "\", \"" + JsEscape(Eval("emp_name")) + "\");" %>'>
                                                    ✓ อนุมัติ
                                                </asp:LinkButton>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                            </tbody>
                                        </table>
                                    </FooterTemplate>
                                </asp:Repeater>
                                <asp:Label ID="lblNoNewUser" runat="server" Visible="false" CssClass="empty-state"
                                    Text="ไม่มีคำขอที่รออนุมัติ" />
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