<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="Transfer.aspx.cs"
    Inherits="Tool_Equipment_Control_System.Transfer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <link rel="stylesheet" href="CSS/jquery-ui.css">
    <link href="CSS/select2.min.css" rel="stylesheet" />
    <link href="CSS/bootstrap_sum.css" rel="stylesheet">
    <link href="CSS/bootstrap.css" rel="stylesheet" />
    <%--<link href="CSS/bootstrap3.3.7.min.css" rel="stylesheet" />--%>
    <link href="CSS/bootstrap.min.css" rel="stylesheet" />
    <link href="CSS/w3v4.10.css" rel="stylesheet" />
    <link rel="stylesheet" href="Content/font-awesome.css">
    <link href="CSS/styles.css" rel="stylesheet">
    <%--<link href="CSS/styleMenu.css" rel="stylesheet" />--%>
    <link href="CSS/font-awesome.css" rel="stylesheet" />
    <link href="DataTables-1.10.18/css/dataTables.css" rel="stylesheet">
    <link href="Content/chosen.css" rel="stylesheet">
    <link href="CSS/fontsgoogleapis.css" rel="stylesheet">
    <link href="CSS/all.min.css" rel="stylesheet">
    <script src="DataTables-1.10.18/js/dataTables.js"></script>
    <script src="DataTables-1.10.18/FixedColumns-3.2.5/js/fixedColumns.dataTables.js"></script>
    <link href="CSS/font-awesome4.7.0.min.css" rel="stylesheet" />
    <link href="DateTimePicker/2-bootstrap-datetimepicker.min.css" rel="stylesheet" />
    <link href="DateTimePicker/3-bootstrap-datetimepicker-standalone.css" rel="stylesheet" />

    <script src="Scripts/chosen.jquery.js"></script>
    <%--<script src="Scripts/jquery-1.9.1.js"></script>--%>
    <script src="Scripts/jquery-3.6.0.min.js"></script>
    <script src="Scripts/script.js"></script>    
    <script src="Scripts/select2.min.js"></script>
    <script src="Scripts/jquery-ui.1.9.1.min.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>
    <%--<script src="Scripts/bootstrap.bundle.min.js"></script>--%>
    <script src="DateTimePicker/1-moment.min.js"></script>
    <script src="DateTimePicker/4-bootstrap-datetimepicker.min.js"></script>

    <style>    
        /* ══════════════════════════════════════
           RESET & VARIABLES
        ══════════════════════════════════════ */
        *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

        :root {
            --bg:           #f0f2f5;
            --surface:      #ffffff;
            --card:         #ffffff;
            --card-head:    #f7f8fa;
            --border:       #e2e6ed;
            --border-light: #edf0f4;
            --accent:       #2563eb;
            --accent-lt:    #eff4ff;
            --accent2:      #059669;
            --accent2-lt:   #ecfdf5;
            --danger:       #dc2626;
            --danger-lt:    #fff1f1;
            --warn:         #d97706;
            --warn-lt:      #fffbeb;
            --text:         #1e293b;
            --text-sub:     #475569;
            --muted:        #94a3b8;
            --radius:       10px;
            --radius-sm:    6px;
            --shadow-sm:    0 1px 3px rgba(0,0,0,.08), 0 1px 2px rgba(0,0,0,.04);
            --shadow:       0 4px 16px rgba(0,0,0,.08);
            --shadow-lg:    0 8px 32px rgba(0,0,0,.10);
        }

        body {
            background: var(--bg);
            color: var(--text);
            font-size: 14px;
            font-family: 'Segoe UI', Tahoma, sans-serif;
            min-height: 100vh;
        }

        /* ══════════════════════════════════════
           LAYOUT
        ══════════════════════════════════════ */
        .page-wrapper {
            width: calc(100% - 20px);
            margin: 30px 10px 0 10px;
            padding-bottom: 80px; 
        }

        /* ── Header ── */
        .page-header {
            background: linear-gradient(135deg, #1d4ed8 0%, #2563eb 60%, #3b82f6 100%);
            color: #fff;
            padding: 5px 24px 12px;
            border-radius: var(--radius);
            margin-bottom: 10px;
            box-shadow: 0 4px 20px rgba(37,99,235,.30);
            display: flex;
            align-items: center;
            gap: 14px;
        }
        .page-header img { height: 50px; filter: drop-shadow(0 2px 4px rgba(0,0,0,.3)); }
        .page-header h3  { font-size: 24px; font-weight: 700; margin: 0; }
        .page-header p   { font-size: 13px; opacity: .80; margin: 2px 0 0; }

        /* ── Import Card ── */
        .import-card {
            background: var(--card);
            border: 1px solid var(--border);
            border-radius: var(--radius);
            box-shadow: var(--shadow-sm);
            margin-bottom: 10px;
            overflow: hidden;
        }
        .import-card-head {
            background: var(--card-head);
            border-bottom: 1px solid var(--border);
            padding: 5px 15px;
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 12px;
        }
        .import-card-head-left {
            display: flex;
            align-items: center;
            gap: 10px;
        }
        .card-icon {
            width: 38px; height: 38px;
            border-radius: var(--radius-sm);
            background: var(--accent-lt);
            display: flex; align-items: center; justify-content: center;
            font-size: 18px;
            flex-shrink: 0;
        }
        .card-title { font-size: 16px; font-weight: 600; color: var(--text); }
        .card-sub   { font-size: 12px; color: var(--text-sub); margin-top: 1px; }
        .import-card-body { padding: 16px 20px; }

        /* ── Drag & Drop Zone ── */
        #dzWrapper { transition: all .35s ease; overflow: hidden; }
        #dzWrapper.dz-hidden { max-height: 0 !important; opacity: 0; margin: 0; padding: 0; }

        .dropzone {
            border: 2px dashed var(--border);
            border-radius: var(--radius);
            padding: 28px 20px;
            text-align: center;
            cursor: pointer;
            transition: border-color .2s, background .2s;
            position: relative;
            background: #fafbfc;
        }
        .dropzone:hover, .dropzone.dragover {
            border-color: var(--accent);
            background: var(--accent-lt);
        }
        .dropzone input[type=file] {
            position: absolute; inset: 0;
            opacity: 0; cursor: pointer;
            width: 100%; height: 100%;
        }
        .dz-icon  { font-size: 36px; margin-bottom: 8px; }
        .dz-title { font-size: 15px; font-weight: 600; color: var(--text); }
        .dz-sub   { font-size: 12px; color: var(--muted); margin-top: 4px; }
        .dz-badge {
            display: inline-block;
            margin-top: 8px;
            background: var(--accent-lt);
            border: 1px solid #bfdbfe;
            color: var(--accent);
            font-size: 11px;
            padding: 2px 10px;
            border-radius: 20px;
            font-weight: 600;
            letter-spacing: .5px;
        }

        /* File indicator */
        .file-indicator {
            display: none; align-items: center; gap: 10px;
            background: var(--accent-lt);
            border: 1px solid #bfdbfe;
            border-radius: var(--radius-sm);
            padding: 9px 14px;
            margin-top: 10px;
            font-size: 12px;
        }
        .file-indicator.show { display: flex; }
        .file-name { flex: 1; color: var(--accent); font-weight: 500; }
        .file-size { color: var(--muted); }

        /* ── Buttons ── */
        .btn {
            display: inline-flex; align-items: center; gap: 6px;
            padding: 8px 18px; border-radius: var(--radius-sm);
            font-size: 13px; font-weight: 500; cursor: pointer;
            border: none; transition: all .15s; text-decoration: none;
        }
        .btn-primary  { background: var(--accent); color: #fff; box-shadow: 0 2px 8px rgba(37,99,235,.25); }
        .btn-primary:hover { background: #1d4ed8; box-shadow: 0 4px 12px rgba(37,99,235,.35); }
        .btn-success  { background: var(--accent2); color: #fff; box-shadow: 0 2px 8px rgba(5,150,105,.20); }
        .btn-success:hover { background: #047857; }
        .btn-outline  { background: #fff; color: var(--text-sub); border: 1px solid var(--border); }
        .btn-outline:hover { border-color: var(--accent); color: var(--accent); background: var(--accent-lt); }
        .btn-ghost    { background: transparent; color: var(--muted); border: 1px solid var(--border); }
        .btn-ghost:hover { color: var(--danger); border-color: var(--danger); background: var(--danger-lt); }
        .btn-full     { width: 100%; justify-content: center; }
        .btn-sm       { padding: 5px 12px; font-size: 12px; }

        /* ── Action Row ── */
        .action-row {
            display: flex;
            gap: 10px;
            margin-top: 12px;
            align-items: center;
        }
        .action-row .btn:first-child { flex: 1; justify-content: center; }

        /* ══════════════════════════════════════
           PREVIEW SECTION
        ══════════════════════════════════════ */
        .preview-section {
            background: var(--card);
            border: 1px solid var(--border);
            border-radius: var(--radius);
            box-shadow: var(--shadow-sm);
            overflow: hidden;
            display: flex;
            flex-direction: column;
        }

        .preview-section-head {
            background: var(--card-head);
            border-bottom: 1px solid var(--border);
            padding: 10px 18px;
            display: flex; align-items: center;
            justify-content: space-between; gap: 10px;
            flex-shrink: 0;
        }
        .preview-section-title {
            font-size: 15px; font-weight: 600; color: var(--text);
            display: flex; align-items: center; gap: 8px;
        }

        .badge-count {
            background: var(--accent2-lt);
            border: 1px solid #a7f3d0;
            color: var(--accent2);
            font-size: 12px;
            padding: 2px 12px;
            border-radius: 20px;
            font-weight: 600;
        }

        /* ── GridView Wrapper — fills remaining viewport height ── */
        .preview-section-body {
            flex: 1;
            overflow: auto;
            padding: 0;
            /* คำนวณความสูงที่เหลือ: viewport - header - import card - action bar - margins */
            max-height: calc(100vh - 320px);
            min-height: 180px;
        }

        /* ── GridView ── */
        .preview-grid {
            width: 100%;
            border-collapse: collapse;
            font-size: 12.5px;
        }
        .preview-grid th {
            background: #f1f5f9;
            color: var(--text-sub);
            font-weight: 600;
            font-size: 11px;
            text-transform: uppercase;
            letter-spacing: .6px;
            padding: 9px 12px;
            text-align: left;
            white-space: nowrap;
            border-bottom: 2px solid var(--border);
            position: sticky;
            top: 0;
            z-index: 2;
        }
        .preview-grid td {
            padding: 7px 12px;
            border-bottom: 1px solid var(--border-light);
            color: var(--text);
            white-space: nowrap;
            vertical-align: middle;
        }
        .preview-grid tbody tr:hover td { background: #f8fafc; }
        .preview-grid tbody tr:nth-child(even) td { background: #fafbfc; }
        .preview-grid tbody tr:nth-child(even):hover td { background: #f1f5f9; }

        /* Row number */
        .preview-grid td:first-child,
        .preview-grid th:first-child {
            color: var(--muted); font-size: 11px;
            text-align: center; width: 36px;
            background: #f8fafc;
        }

        /* MATCH_STATUS column */
        .status-ok  {
            color: var(--accent2); font-weight: 600; font-size: 11px;
        }
        .status-err {
            color: var(--danger); font-weight: 600; font-size: 11px;
        }

        /* ── Empty State ── */
        .empty-state {
            text-align: center; padding: 50px 20px; color: var(--muted);
        }
        .empty-state-icon { font-size: 44px; margin-bottom: 10px; }
        .empty-state-text { font-size: 14px; }

        /* ══════════════════════════════════════
           ACTION BAR (sticky bottom)
        ══════════════════════════════════════ */
        .action-bar {
            position: fixed; 
            bottom: 10px;
            /*left: 0;*/
            right: 10px;
            background: #ffffff78;
            /*border-top: 1px solid var(--border);*/
            box-shadow: 0 -4px 20px rgba(0,0,0,.08);
            padding: 10px 24px;
            display: flex; align-items: center;
            justify-content: space-between; gap: 12px;
            z-index: 100;
            width:500px;
            border-radius:10px;
            border: 1px solid #49494999;
        }
        .action-bar-left  { display: flex; gap: 10px; align-items: center; }
        .action-bar-right { display: flex; gap: 8px; }

        .status-pill {
            display: flex; align-items: center; gap: 7px;
            font-size: 13px; color: var(--text-sub);
        }
        .status-dot {
            width: 7px; height: 7px; border-radius: 50%;
            background: var(--accent2);
            animation: pulse 2s infinite;
        }
        @keyframes pulse { 0%,100%{opacity:1} 50%{opacity:.3} }

        /* ══════════════════════════════════════
           TOAST
        ══════════════════════════════════════ */
        .toast-container {
            position: fixed; top: 75px; right: 20px;
            z-index: 9999; display: flex; flex-direction: column; gap: 8px;
        }
        .toast {
            background: #fff;
            border: 1px solid var(--border);
            border-radius: var(--radius-sm);
            padding: 11px 16px;
            font-size: 13px;
            display: flex; align-items: center; gap: 10px;
            box-shadow: var(--shadow-lg);
            animation: toastIn .3s ease;
            min-width: 220px;
            max-width: 340px;
            color: var(--text);
        }
        .toast.success { border-left: 3px solid var(--accent2); }
        .toast.error   { border-left: 3px solid var(--danger); }
        @keyframes toastIn {
            from { opacity:0; transform:translateX(16px); }
            to   { opacity:1; transform:translateX(0); }
        }

        /* ══════════════════════════════════════
           SCROLLBAR
        ══════════════════════════════════════ */
        ::-webkit-scrollbar { width: 12px; height: 12px; }
        ::-webkit-scrollbar-track { background: #e2e6ed; border-radius: 5px; }
        ::-webkit-scrollbar-thumb { background: #64748b; border-radius: 5px; border: 2px solid #e2e6ed;  }
        ::-webkit-scrollbar-thumb:hover { background: #334155; }
        ::-webkit-scrollbar-corner { background: #e2e6ed; } 
    </style>

    <style>
        /* ── ID Tags ── */
        .id-tag        { display:inline-block; padding:2px 8px; border-radius:20px; font-size:11px; font-weight:700; }
        .id-tag.jig    { background:#eff4ff; color:#2563eb; border:1px solid #bfdbfe; }
        .id-tag.eqp    { background:#ecfdf5; color:#059669; border:1px solid #a7f3d0; }
        .id-tag.new    { background:#f1f5f9; color:#94a3b8; border:1px solid #e2e8f0; }
        .id-tag.sub    { background:#faf5ff; color:#7c3aed; border:1px solid #ddd6fe; }

        /* ── Match / Dup Badges ── */
        .badge-new     { background:#f0fdf4; color:#15803d; border:1px solid #bbf7d0; padding:2px 8px; border-radius:20px; font-size:11px; font-weight:700; }
        .badge-jig     { background:#eff4ff; color:#2563eb; border:1px solid #bfdbfe; padding:2px 8px; border-radius:20px; font-size:11px; font-weight:700; }
        .badge-eqp     { background:#ecfdf5; color:#059669; border:1px solid #a7f3d0; padding:2px 8px; border-radius:20px; font-size:11px; font-weight:700; }
        .badge-same    { background:#fff7ed; color:#c2410c; border:1px solid #fed7aa; padding:2px 8px; border-radius:20px; font-size:11px; font-weight:700; }
        .badge-addtool { background:#eff4ff; color:#2563eb; border:1px solid #bfdbfe; padding:2px 8px; border-radius:20px; font-size:11px; font-weight:700; }
        .badge-addeqp  { background:#ecfdf5; color:#059669; border:1px solid #a7f3d0; padding:2px 8px; border-radius:20px; font-size:11px; font-weight:700; }

        /* ── Empty cell ── */
        .cell-empty    { color:#cbd5e1; font-size:12px; }

        /* ── Status Dropdown ── */
        .ddl-status {
            border: 1px solid var(--border);
            border-radius: var(--radius-sm);
            padding: 3px 6px;
            font-size: 12px;
            color: var(--text);
            background: #fff;
            min-width: 130px;
            cursor: pointer;
        }
        .ddl-status:focus {
            outline: none;
            border-color: var(--accent);
            box-shadow: 0 0 0 2px rgba(37,99,235,.15);
        }
        /* ══════════════════════════════════════
           CONFIRM DIALOG
        ══════════════════════════════════════ */
        .confirm-overlay {
            position: fixed; inset: 0;
            background: rgba(15,23,42,.45);
            backdrop-filter: blur(3px);
            z-index: 9000;
            display: flex; align-items: center; justify-content: center;
            animation: fadeIn .2s ease;
        }
        .confirm-box {
            background: #fff;
            border-radius: 14px;
            box-shadow: 0 20px 60px rgba(0,0,0,.20);
            padding: 20px 20px 20px;
            width: 360px; max-width: 90vw;
            animation: slideUp .25s ease;
        }
        .confirm-icon  { font-size: 40px; text-align: center; margin-bottom: 10px; }
        .confirm-title {
            font-size: 17px; font-weight: 700;
            color: var(--text); text-align: center; margin-bottom: 6px;
        }
        .confirm-msg {
            font-size: 13px; color: var(--text-sub);
            text-align: center; line-height: 1.6; margin-bottom: 20px;
        }
        .confirm-stats {
            background: var(--accent-lt);
            border: 1px solid #bfdbfe;
            border-radius: var(--radius-sm);
            padding: 10px 14px;
            margin-bottom: 18px;
            font-size: 12px; color: var(--accent);
            display: flex; gap: 18px; justify-content: center;
        }
        .confirm-stat-item { text-align: center; }
        .confirm-stat-num  { font-size: 20px; font-weight: 700; display: block; }
        .confirm-stat-lbl  { font-size: 11px; opacity: .75; }
        .confirm-actions   { display: flex; gap: 10px; }
        .confirm-actions .btn { flex: 1; justify-content: center; font-size: 13px; }

        @keyframes fadeIn  { from{opacity:0} to{opacity:1} }
        @keyframes slideUp { from{opacity:0;transform:translateY(16px)} to{opacity:1;transform:translateY(0)} }

    </style>

    <%--===== FORM ELEMENTS =====--%> 
    <style>
        .form-row {
            display: flex;
            flex-wrap: wrap;
            gap: 12px;
            align-items: flex-end;
        }
        .form-group { display: flex; flex-direction: column; margin-bottom:5px; }
        .form-group label {
            font-size: 12px;
            font-weight: 600;
            color: #555;
            text-transform: uppercase;
            letter-spacing: 0.4px;
            margin-left:5px;
            margin-bottom:-5px;
        }
        .form-group input[type="text"],
        .form-group select {
            border: 1.5px solid #c8d6e5;
            border-radius: 5px;
            padding: 4px 10px;
            font-size: 14px;
            color: #2c3e50;
            background: #fff;
            transition: border-color 0.2s;
            height: 28px;
        }
        .form-group input:focus,
        .form-group select:focus {
            outline: none;
            border-color: #2980b9;
            box-shadow: 0 0 0 3px rgba(41,128,185,0.12);
        }
        .form-group input[readonly] {
            background: #f5f8fb;
            color: #666;
        }
        .page-note {
            padding: 5px 10px;
            display: flex;
            flex-wrap: wrap;
            align-items: center;
            gap: 10px;
        }
        .page-note .badge {
            color: #fff;
            border-radius: 6px;
            padding: 4px 10px;
            font-size: 12px;
            font-weight: 500;
            text-shadow:none;
        }
    </style>

    <%--Grid--%>
    <style>
        .grid_search{
            display: grid;
            grid-template-columns: 320px Auto;
            gap: 5px;
            align-items: center;
        }
    </style>

    <%--Table--%>
    <style>
        /* ===== TABLES ===== */
        .tbl-wrap 
        { 
            /*max-height: 250px;*/
            overflow-x: auto;
            overflow-y: auto;
            position: relative;
            border: 1px solid #ddd;
        }
        table.data-table {
            width: 100%;
            border-collapse: collapse;
            font-size: 12px;
            /*min-width: 1600px;*/
        }
        table.data-table thead tr {
            background: #1a3a5c;
            color: #fff;
        }
        table.data-table thead th {
            padding: 9px 10px;
            text-align: left;
            font-weight: 600;
            white-space: nowrap;
            letter-spacing: 0.3px;
        }
        table.data-table tbody tr { border-bottom: 1px solid #d5d5d5; }
        table.data-table tbody tr:hover { background: #f0f7ff; }
        table.data-table tbody td { padding: 8px 10px; vertical-align: middle; }
        table.data-table tbody tr:nth-child(even) { background: #f8fbfe; }
        table.data-table tbody tr:nth-child(even):hover { background: #f0f7ff; }

        .no-data { text-align: center; color: #aaa; padding: 24px; font-style: italic; }

        .headerGv1 {
            color: #ffffff;
            font-weight: 800;
            background-color: #5e5e5e;
            padding: 2px 4px;
            text-align:center;
        }
        .headerGv2 {
            color: #ffffff;
            font-weight: 800;
            background-color: #3b753ae3;
            padding: 2px 4px;
            text-align:center;
        }
        .headerGvOld {
            color: #5f5a5a;
            font-weight: 800;
            background-color: #e4e366;
            padding: 2px 4px;
            text-align:center;
        }
        .rowGv1 {
            font-size: 12px;
            padding: 2px 4px;
            background-color: #ffffff;
            text-align:center;
        }
        .rowGv2 {
            font-size: 12px;
            padding: 2px 4px;
            background-color: #ffffff;
            text-align:left;
        }
        .rowGv_toolNo{
            font-size: 12px;
            padding: 2px 4px;
            background-color: #ffffff;
            text-align:center;
            min-width:140px;
        }
        .img_tool {
            display: inline-block; /* ทำให้เป็น block-level element เพื่อกำหนดขนาดได้ */
            width: 25px; /* กำหนดความกว้างของรูปภาพ */
        }        
        .fixed-header th {
            position: sticky;
            top: 0;
            z-index: 3;
            border-bottom: 2px solid #ccc;
        }        
        .pager-top-right {
            position: sticky;
            top: 0;
            z-index: 4;
            background: white;
            text-align: right;
            padding: 5px 10px;
            border-bottom: 1px solid #ccc;
        }
    </style>

    <%-- modal Picture--%>
    <style>
        .modal.fade {
            background: fixed;
            border:none;
            box-shadow:none;
            left:10%;
            width:auto;
        }
        h4 {
            color: aliceblue;
        }  
        .modal-body{
            max-height : 800px;
        }
  
    </style>

    <%--status badge--%>
    <style>
        .badge {
            display: inline-block;
            padding: 4px 10px;
            font-size: 12px;
            font-weight: 600;
            border-radius: 12px;
            color: #fff;
        }

        .badge-normal {
            background-color: #28a745; /* เขียว */
        }

        .badge-spare {
            background-color: #ffc107; /* เหลือง */
            color: #000;
        }
        .badge-borrowing {
            background-color: #dc3545; /* แดง */
            color: #fff;
        }
        .badge-issue{
            background: #b7cfe9bd;
            border: 1px solid #2749d7;
            border-radius: 10px;
            padding: 2px 10px;
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

    </script>

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

    <%-- Script : DropDown Selection--%>
    <script>
        function initSotoSelect2() {
            $('.ddl-status').each(function () {
                var $el = $(this);
                if ($el.hasClass('select2-hidden-accessible')) {
                    $el.select2('destroy');
                }
                $el.select2({
                    width: '95%',
                    placeholder: '-- เลือก Soto Cell --',
                    allowClear: false,
                    dropdownAutoWidth: false
                });
            });

            // ✅ Focus ช่อง search ทันทีที่เปิด dropdown
            $('.ddl-status').off('select2:open.myOpen').on('select2:open.myOpen', function () {
                setTimeout(function () {
                    var searchField = document.querySelector('.select2-container--open .select2-search__field');
                    if (searchField) searchField.focus();
                }, 10);
            });

            // ล้าง highlight สีเตือนเมื่อเลือกค่าใหม่แล้ว
            $('.ddl-status').off('change.myHighlight').on('change.myHighlight', function () {
                $(this).removeClass('ddl-missing');
                $(this).next('.select2-container').find('.select2-selection').removeClass('ddl-missing');
            });
        }

        function pageLoad() {
            adjustPreviewHeight();
            initSotoSelect2();   // ← เรียกทุกครั้งหลัง async postback ของ UpdatePanel
        }

        window.addEventListener('load', function () {
            initSotoSelect2();   // ← เรียกตอน full page load ครั้งแรก
        });
    </script>

    <%-- Style : DropDown Selection--%>
    <style>
        .select2-container .select2-selection--single {
            height: 28px !important;
            border: 1px solid var(--border) !important;
            border-radius: var(--radius-sm) !important;
        }
        .select2-container--default .select2-selection--single .select2-selection__rendered {
            line-height: 26px !important;
            font-size: 12px;
        }
        .select2-container--default .select2-selection--single .select2-selection__arrow {
            height: 26px !important;
        }
        .select2-selection.ddl-missing,
        .select2-selection--single.ddl-missing {
            background: #fff7ed !important;
            border: 1px solid #f97316 !important;
            box-shadow: 0 0 0 2px rgba(249,115,22,.15);
        }
        /* ── ลดขนาด font ในรายการ dropdown (options list) ── */
        .select2-container--default .select2-results__option {
            font-size: 12px;
            padding: 5px 10px;
        }
        .select2-container--default .select2-search--dropdown .select2-search__field {
            font-size: 12px;
            padding: 4px 8px;
        }
        .select2-container--default .select2-results__group {
            font-size: 12px;
        }
    </style>


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
                        กำลังบันทึกข้อมูล...
                    </div>
   
                </div>              
    
            </div>

            <!-- Toast Container -->
            <div class="toast-container" id="toastContainer"></div>

            <div id="form1" runat="server">

                <div class="page-wrapper">

                    <!-- ── HEADER ── -->
                    <div class="page-header" style="margin-top:-20px;">
                        <img src="images/exchange.png" alt="icon" />
                        <div>
                            <h3>Equipment Transfer</h3>
                            <p>โอนย้ายผู้รับผิดชอบอุปกรณ์</p>
                        </div>
                    </div>

                    <%------------ SECTION 1 : Search & Add List --------------%>           
                    <asp:Panel ID="pnlSearch_Add" runat="server" Visible="true" class="card" style="">

                        <div class="import-card">

                            <%--Header Search & Add List--%>
                            <div class="import-card-head">
                                <div class="import-card-head-left">
                                    <div class="card-icon">📤</div>
                                    <div>
                                        <div class="card-title">Search & Add List : ค้นหาข้อมูลและเพิ่มรายการสำหรับ Transfer </div>
                                    </div>
                                </div>
                            </div>

                            <div class="card-body" style="margin-bottom:10px;">

                                <!-- Tab: Search -->
                                <div id="tabFixAsset" class="grid_search">

                                    <%--Search & Button Search--%>
                                    <div class="form-row" style="margin-left:15px;">                            
                                        <div class="form-group" style="min-width:200px; align-self:center;">
                                            <asp:TextBox ID="txtFixAsset" runat="server" Width="200px" 
                                                placeholder="ค้นหารายการ"
                                                Style="text-align:center; background:#00a7ff40;"/>
                                        </div>
                                        <asp:Button ID="btnSearchFixAsset" runat="server" Text="Search"
                                            CssClass="btn btn-primary"
                                            OnClick="btnSearch_Click"
                                            style="align-self:center; margin-top:-5px;"/>                               
                                    </div>
                            
                                    <%--Search Note--%>
                                    <div class="form-row" style="align-content:center;">
                                        <div class="page-note">
                                            <span class="badge" style="background:#ff5d5dd4;">ตัวอย่างการค้นหา</span>
                                            <span class="badge">BFM00012345 [Fix Asset]</span>
                                            <span class="badge">1341001P [Control No]</span>
                                            <span class="badge">CPT-2024-U01800 [Computer Name]</span>
                                            <span class="badge">SZ99-FK2-7577-1J [Tool No]</span>
                                            <span class="badge">SURFACE GO4 [Equipment Model]</span>
                                        </div>
                                    </div>

                                </div>

                                <%--<!-- Search Result Grid --%>
                                <div class="tbl-wrap" style="max-height: 165px;">
                                    <div class="pager-top-right">

                                        <asp:GridView ID="gvSearchResult" runat="server"
                                            CssClass="data-table fixed-header"
                                            AutoGenerateColumns="false"
                                            OnRowCommand="gvSearchResult_RowCommand"
                                            OnRowDataBound="gvSearchResult_RowDataBound"
                                            style="min-width:1300px;"
                                            EmptyDataText="ไม่พบรายการ">

                                            <EmptyDataRowStyle CssClass="no-data" />
                    
                                            <Columns>

                                                <%--Item No--%>
                                                <asp:BoundField  DataField="RowNo"  
                                                                 HeaderText="#" HeaderStyle-CssClass="headerGv1"
                                                                 ItemStyle-CssClass="rowGv1" ItemStyle-Width="40px" />

                                                <%--Add Button--%>
                                                <asp:ButtonField HeaderText="Add" ButtonType="Button" Text="Add" ItemStyle-Width="65px"
                                                    CommandName="AddItem"     
                                                    HeaderStyle-CssClass="headerGv1"
                                                    ItemStyle-CssClass="rowGv1"
                                                    ControlStyle-CssClass="btn btn-success btn-sm" /> 

                                                <%--Link Detail--%>
                                                <asp:TemplateField HeaderText="Detail" HeaderStyle-CssClass="headerGv1" ItemStyle-Width="65px"
                                                    ItemStyle-CssClass="rowGv1" ItemStyle-HorizontalAlign="Center" ItemStyle-Wrap="false">
                                                    <ItemTemplate>
                                                        <asp:LinkButton ID="btn_detailView" data-toggle="tooltip" title="View Detail" runat="server"
                                                            OnClick="btn_detailView_Click" CssClass="img_tool">
                                                            <img src="images/eye-tracking.png" />
                                                        </asp:LinkButton>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <%--Picture Button & Hidden Data--%>
                                                <asp:TemplateField HeaderText="Picture" HeaderStyle-CssClass="headerGv1" ItemStyle-Width="65px"
                                                    ItemStyle-CssClass="rowGv1" ItemStyle-HorizontalAlign="Center">
                                                    <ItemTemplate>   
                                                        <asp:HiddenField ID="hdtoolId" runat="server" Value='<%# Eval("tool_id") %>' />
                                                        <asp:HiddenField ID="hdtoolSub" runat="server" Value='<%# Eval("sub_no") %>' />
                                                        <asp:HiddenField ID="hdsystemId" runat="server" Value='<%# Eval("sys_status") %>' />
                                                        <asp:HiddenField ID="hdmodelId" runat="server" Value='<%# Eval("model_id") %>' />
                                                        <asp:HiddenField ID="hdlocationId" runat="server" Value='<%# Eval("location1_id") %>' />
                                                        <asp:HiddenField ID="hdcellId" runat="server" Value='<%# Eval("location2_id") %>' />
                                                        <asp:HiddenField ID="hdstationId" runat="server" Value='<%# Eval("station_id") %>' />

                                                        <asp:LinkButton ID="btn_pic" title="View Picture" data-toggle="tooltip" runat="server"
                                                            OnClick="btn_pic_Click"
                                                            CssClass="img_tool">
                                                            <img src="images/photo-gallery.png" />
                                                        </asp:LinkButton>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <%--Display Status--%>
                                                <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="headerGv1" ItemStyle-Width="140px"
                                                    ItemStyle-CssClass="rowGv1" ItemStyle-HorizontalAlign="Center">
                                                    <ItemTemplate>   
                                                        <asp:HiddenField ID="hdstatusId" runat="server" Value='<%# Eval("status_id") %>' />
    
                                                        <span class='badge <%# GetStatusBadgeClass(Eval("DisplayStatus").ToString()) %>'>
                                                            <%# Eval("DisplayStatus") %>
                                                        </span>

                                                    </ItemTemplate>
                                                </asp:TemplateField>
                
                                                <%--Sub No--%>
                                                <asp:BoundField  DataField="sub_no" ItemStyle-Width="50px"  
                                                                 HeaderText="No" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                                <%--Fix Asset--%>
                                                <asp:BoundField  DataField="FixAcces" ItemStyle-Width="110px"  
                                                                 HeaderText="Fix Asset" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                                <%--Control No--%> 
                                                <asp:BoundField  DataField="control_no" ItemStyle-Width="130px"     
                                                                 HeaderText="Control No" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                                <%--Tool No / Model--%>
                                                <asp:BoundField  DataField="tool_no"      
                                                                 HeaderText="Tool No / Model" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                                <%--Dept Code--%>
                                                <asp:BoundField  DataField="dept_code" ItemStyle-Width="65px"  
                                                                 HeaderText="Dept" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                                <%--CPT Model--%>
                                                <asp:BoundField  DataField="model_name"
                                                                 HeaderText="CPT Model" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                                <%--Cell--%>
                                                <asp:BoundField  DataField="cell" 
                                                                 HeaderText="Cell" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                                <%--Station--%>
                                                <asp:BoundField  DataField="station"
                                                                 HeaderText="Station" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>

                                            </Columns>

                                        </asp:GridView>
                                    </div>
                                </div>

                            </div>

                        </div>

                    </asp:Panel>

                    <!-- ── Transfer List ── -->
                    <div class="import-card">

                        <div class="import-card-head">
                            <div class="import-card-head-left">
                                <div class="card-icon">📋</div>
                                <div>
                                    <div class="card-title">Transfer List : รายการโอนย้าย Jig Tool & Equipment</div>
                                </div>
                            </div>
                        </div>
    
                        <%--Transfer List--%>
                        <div class="tbl-wrap" style="max-height: calc(100vh - 520px);">
                            <asp:GridView ID="gvTransferList" runat="server"
                                CssClass="data-table change-table fixed-header"
                                style="margin: 5px 10px 10px 10px;"
                                AutoGenerateColumns="false"
                                OnRowCommand="gvTransferList_RowCommand"
                                OnRowDataBound="gvTransferList_RowDataBound"
                                EmptyDataText="ไม่มีรายการแจ้งเปลี่ยนแปลง">

                                    <EmptyDataRowStyle CssClass="no-data" />

                                    <Columns>

                                        <%--Item No--%>
                                        <asp:BoundField  DataField="RowNo"  
                                                            HeaderText="#" HeaderStyle-CssClass="headerGv1"
                                                            ItemStyle-CssClass="rowGv1" ItemStyle-Width="40px" />

                                        <%--Remove--%>
                                        <asp:TemplateField HeaderText="Remove" ItemStyle-Width="65px"
                                            HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1">
                                            <ItemTemplate>
                                                <asp:Button ID="btnRemove" runat="server" Text="Remove"
                                                    CssClass="btn btn-danger btn-sm"
                                                    OnClientClick="return confirmRemove();"
                                                    CommandName="RemoveItem" CommandArgument="<%# Container.DataItemIndex %>" />
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <%--Detail--%>
                                        <asp:TemplateField HeaderText="Detail" HeaderStyle-CssClass="headerGv1" ItemStyle-Width="55px"
                                            ItemStyle-CssClass="rowGv1" ItemStyle-HorizontalAlign="Center">
                                            <ItemTemplate>
                                                <asp:LinkButton ID="btn_Detail" title="View Detail" runat="server"
                                                    OnClick="btn_detailView_Click" CssClass="img_tool">
                                                    <img src="images/eye-tracking.png" />
                                                </asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <%--Picture + keys (ชื่อ hidden ตรงกับ code-behind)--%>
                                        <asp:TemplateField HeaderText="Pic" HeaderStyle-CssClass="headerGv1" ItemStyle-Width="55px"
                                            ItemStyle-CssClass="rowGv1" ItemStyle-HorizontalAlign="Center">
                                            <ItemTemplate>
                                                <asp:HiddenField ID="hdtoolId" runat="server" Value='<%# Eval("ToolId") %>' />
                                                <asp:HiddenField ID="hdtoolSub" runat="server" Value='<%# Eval("ToolSub") %>' />
                                                <asp:HiddenField ID="hdSystemId" runat="server" Value='<%# Eval("SystemId") %>' />
                                                <asp:LinkButton ID="btn_Picture" title="View Picture" runat="server"
                                                    OnClick="btn_pic_Click" CssClass="img_tool">
                                                    <img src="images/photo-gallery.png" />
                                                </asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <%--Fix Asset--%>
                                        <asp:BoundField DataField="FixAssetNo" ItemStyle-Width="110px"
                                            HeaderText="Fix Asset" HeaderStyle-CssClass="headerGv1" 
                                            ItemStyle-CssClass="rowGv1 cell-fixasset" />

                                        <%--Control No--%>
                                        <asp:BoundField DataField="ControlNo" ItemStyle-Width="120px"
                                            HeaderText="Control No" HeaderStyle-CssClass="headerGv1" 
                                            ItemStyle-CssClass="rowGv1 cell-controlno" />

                                        <%--Tool No--%>
                                        <asp:BoundField DataField="ToolNo"
                                            HeaderText="Tool No / Model" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv_toolNo" />

                                        <%--Sub--%>
                                        <asp:BoundField DataField="ToolSub" ItemStyle-Width="45px"
                                            HeaderText="No" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" /> 

                                        <%--Change to Section (Soto)--%>
                                        <asp:TemplateField HeaderText="Change to Soto" ItemStyle-Width="320px"
                                            HeaderStyle-CssClass="headerGv2" ItemStyle-CssClass="rowGv1">
                                            <ItemTemplate>
                                                <asp:DropDownList ID="ddlSoto" runat="server" CssClass="ddl-status" style="text-align:center;" />
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <%--═══════════ โซน CURRENT (4 ช่อง, label อย่างเดียว) ═══════════--%>

                                        <%--Current Dept (เริ่มโซน → zone-divider)--%>
                                        <asp:BoundField DataField="DeptCode" ItemStyle-Width="80px" HeaderText="Dept"
                                            HeaderStyle-CssClass="headerGv1 zone-divider" ItemStyle-CssClass="rowGv1 zone-divider" />

                                        <%--Current Model--%>
                                        <asp:BoundField DataField="ModelName" ItemStyle-Width="150px" HeaderText="Model"
                                            HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />

                                        <%--Current Cell--%>
                                        <asp:BoundField DataField="CellName" ItemStyle-Width="180px" HeaderText="Cell"
                                            HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />

                                        <%--Current Station--%>
                                        <asp:BoundField DataField="StationName" ItemStyle-Width="180px" HeaderText="Station"
                                            HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />                

                                    </Columns>
                                </asp:GridView>
                            </div>
                        </div> 

                    </div>              

                </div><!-- /page-wrapper -->

                <!-- ── ACTION BAR ── -->
                <div class="action-bar">
                    <div class="action-bar-left">
                        <div class="status-pill">
                            <div class="status-dot"></div>
                            <asp:Label ID="lblActionStatus" runat="server" Text="รอ Import ข้อมูล" />
                        </div>
                    </div>
                    <div class="action-bar-right">
                        <button type="button" class="btn btn-outline"
                            onclick="history.back()">ยกเลิก</button>
                        <asp:Button ID="btnSave" runat="server"
                            Text="💾 บันทึกข้อมูล"
                            CssClass="btn btn-success"
                            Style="padding:8px 28px;"
                            OnClick="btnSave_Click"
                            OnClientClick="return confirmSave();"
                            Enabled="false" />
                    </div>
                </div>
                

            </div><!-- /form1 -->

            <%-- Modal Show Pictrue--%> 
            <div class="modal fade" id="modal_UploadPictureMain" tabindex="-1" role="dialog" style="padding:0px; min-width: 85%" >
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header theme-bg-color1 theme-text-color2">
                            <button type="button" class="close" data-dismiss="modal">&times;</button>
                            <h4 class="modal-title">&nbsp;Pictrue</h4>
                        </div>
                        <div class="modal-body" style="text-align-last: center;">
                            <asp:Label runat="server" ID="lb_testPic"></asp:Label>
                            <asp:Image runat="server" ID="imgMain" Width="100%" />
                        </div>
                    </div>
                </div>
            </div>

        </ContentTemplate>

        <Triggers>

        </Triggers>
    </asp:UpdatePanel>

    <!-- ── JAVASCRIPT ── -->
    <script>
        /* ════════════════════════════════
           ตัวแปร global
        ════════════════════════════════ */
        var dz = null;

        //window.addEventListener('load', function () {
        //    dz = document.getElementById('dropzone');
        //    adjustPreviewHeight();
        //});

        window.addEventListener('resize', adjustPreviewHeight);

        /* ════════════════════════════════
           ปรับความสูง preview ตามหน้าจอ
        ════════════════════════════════ */
        function adjustPreviewHeight() {
            var body = document.getElementById('previewBody');
            if (!body) return;

            var vh = window.innerHeight;
            var actionBarH = 64;   /* ✅ เพิ่มเป็น 64 เผื่อ border */
            var pageHeaderH = document.querySelector('.page-header')
                ? document.querySelector('.page-header').offsetHeight + 16 : 80;
            var importCardH = document.querySelector('#dzWrapper.dz-hidden')
                ? (document.querySelector('.import-card')
                    ? document.querySelector('.import-card-head').offsetHeight + 33 : 60)
                : (document.querySelector('.import-card')
                    ? document.querySelector('.import-card').offsetHeight + 16 : 140);
            var previewHeadH = document.querySelector('.preview-section-head')
                ? document.querySelector('.preview-section-head').offsetHeight : 46;
            var margin = 46;

            var maxH = vh - actionBarH - pageHeaderH - importCardH - previewHeadH - margin - 50;
            if (maxH < 160) maxH = 160;
            body.style.maxHeight = maxH + 'px';
        }

        /* ════════════════════════════════
           ซ่อน / แสดง Drop Zone
        ════════════════════════════════ */
        function hideDzWrapper() {
            var w = document.getElementById('dzWrapper');
            if (w) w.classList.add('dz-hidden');
            setTimeout(adjustPreviewHeight, 50);
        }
        function showDzWrapper() {
            var w = document.getElementById('dzWrapper');
            if (w) w.classList.remove('dz-hidden');
            setTimeout(adjustPreviewHeight, 50);
        }

        /* ════════════════════════════════
           Drag & Drop handlers
        ════════════════════════════════ */
        function handleDragOver(e) {
            e.preventDefault(); e.stopPropagation();
            if (dz) dz.classList.add('dragover');
        }
        function handleDragLeave(e) {
            e.preventDefault();
            if (dz) dz.classList.remove('dragover');
        }
        function handleDrop(e) {
            e.preventDefault(); e.stopPropagation();
            if (dz) dz.classList.remove('dragover');
            if (e.dataTransfer.files.length > 0)
                showFileInfo(e.dataTransfer.files[0]);
        }
        function handleFileSelect(e) {
            if (e.target.files.length > 0)
                showFileInfo(e.target.files[0]);
        }
        function showFileInfo(file) {
            document.getElementById('fileName').textContent = file.name;
            document.getElementById('fileSize').textContent = (file.size / 1024).toFixed(1) + ' KB';
            document.getElementById('fileIndicator').classList.add('show');
        }
        function clearFile() {
            document.getElementById('fileIndicator').classList.remove('show');
            document.getElementById('fileName').textContent = '—';
            document.getElementById('fileSize').textContent = '—';
        }

        /* ════════════════════════════════
           Toast
        ════════════════════════════════ */
        function showToast(type, msg) {
            var c = document.getElementById('toastContainer');
            var t = document.createElement('div');
            t.className = 'toast ' + type;
            t.innerHTML = (type === 'success' ? '✅' : '❌') + ' <span>' + msg + '</span>';
            c.appendChild(t);
            setTimeout(function () {
                t.style.opacity = '0';
                t.style.transition = 'opacity .3s';
                setTimeout(function () { t.remove(); }, 300);
            }, 3500);
        }

        /* ════════════════════════════════
           Confirm Save Dialog
        ════════════════════════════════ */

        var _saveConfirmed = false;

        function confirmSave() {
            if (_saveConfirmed) {
                _saveConfirmed = false;
                return true;
            }

            // ✅ เลือกเฉพาะแถวข้อมูลจริง ไม่มีทางไปโดนแถว header
            var rows = document.querySelectorAll('.change-table .transfer-row');
            var total = rows.length;

            if (total === 0) return false;

            var itemsHtml = '';
            var missingSection = 0;

            rows.forEach(function (row, i) {
                var fixAsset = row.getAttribute('data-fixasset') || '-';
                var controlNo = row.getAttribute('data-controlno') || '-';

                var ddl = row.querySelector('select.ddl-status');
                var sectionText = '-';

                if (ddl) {
                    sectionText = ddl.options[ddl.selectedIndex]
                        ? ddl.options[ddl.selectedIndex].text
                        : '-';

                    var $selection = $(ddl).next('.select2-container').find('.select2-selection');

                    if (!ddl.value) {
                        missingSection++;
                        $(ddl).addClass('ddl-missing');
                        $selection.addClass('ddl-missing');
                    } else {
                        $(ddl).removeClass('ddl-missing');
                        $selection.removeClass('ddl-missing');
                    }
                }

                itemsHtml +=
                    '<tr>' +
                    '<td style="text-align:center;">' + (i + 1) + '</td>' +
                    '<td>' + fixAsset + '</td>' +
                    '<td>' + controlNo + '</td>' +
                    '<td style="color:#2563eb;font-weight:600;">' + sectionText + '</td>' +
                    '</tr>';
            });

            if (missingSection > 0) {
                showToast('error', 'กรุณาเลือก Soto Cell ให้ครบทุกรายการ (ขาดอีก ' + missingSection + ' รายการ)');
                return false;
            }

            var overlay = document.createElement('div');
            overlay.className = 'confirm-overlay';
            overlay.id = 'confirmOverlay';
            overlay.innerHTML =
                '<div class="confirm-box" style="width:560px;">' +
                '<div class="confirm-icon">💾</div>' +
                '<div class="confirm-title">ยืนยันการโอนย้ายอุปกรณ์</div>' +
                '<div class="confirm-msg">รายการทั้งหมด <b>' + total + '</b> รายการ จะถูกโอนย้ายไปยัง Section ที่เลือก</div>' +
                '<div style="max-height:260px; overflow-y:auto; border:1px solid #e2e6ed; border-radius:8px; margin-bottom:18px;">' +
                '<table style="width:100%; border-collapse:collapse; font-size:12px;">' +
                '<thead><tr style="background:#f1f5f9;">' +
                '<th style="padding:6px 8px; text-align:center;">#</th>' +
                '<th style="padding:6px 8px; text-align:center;">Fix Asset</th>' +
                '<th style="padding:6px 8px; text-align:center;">Control No</th>' +
                '<th style="padding:6px 8px; text-align:center;">Change to Section</th>' +
                '</tr></thead>' +
                '<tbody>' + itemsHtml + '</tbody>' +
                '</table>' +
                '</div>' +
                '<div class="confirm-actions">' +
                '<button type="button" class="btn btn-outline" onclick="closeConfirm()">✕ ยกเลิก</button>' +
                '<button type="button" class="btn btn-success" onclick="doSave()">💾 ยืนยันบันทึก</button>' +
                '</div>' +
                '</div>';

            document.body.appendChild(overlay);
            overlay.addEventListener('click', function (ev) {
                if (ev.target === overlay) closeConfirm();
            });

            return false;
        }

        function closeConfirm() {
            var o = document.getElementById('confirmOverlay');
            if (o) o.remove();
        }

        function doSave() {
            closeConfirm();
            _saveConfirmed = true;
            showLoading();                                  // ✅ เปิด loading ทันทีที่กดยืนยัน
            __doPostBack('<%= btnSave.UniqueID %>', '');
        }
    </script>

</asp:Content>