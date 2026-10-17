<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="EditLocation.aspx.cs"
    Inherits="Tool_Equipment_Control_System.EditLocation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <link rel="stylesheet" href="CSS/jquery-ui.css">
    <link href="CSS/select2.min.css" rel="stylesheet" />
    <link href="CSS/bootstrap_sum.css" rel="stylesheet">
    <link href="CSS/bootstrap.css" rel="stylesheet" />
    <link href="CSS/bootstrap.min.css" rel="stylesheet" />
    <link href="CSS/w3v4.10.css" rel="stylesheet" />
    <link rel="stylesheet" href="Content/font-awesome.css">
    <link href="CSS/styles.css" rel="stylesheet">
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
    <script src="Scripts/jquery-3.6.0.min.js"></script>
    <script src="Scripts/script.js"></script>
    <script src="Scripts/select2.min.js"></script>
    <script src="Scripts/jquery-ui.1.9.1.min.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>
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
            --accent:       #0d9488;
            --accent-lt:    #ecfdf9;
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

        .page-wrapper {
            width: calc(100% - 20px);
            margin: 30px 10px 0 10px;
            padding-bottom: 80px; 
        }

        /* ── Header ── */
        .page-header {
            background: linear-gradient(135deg, #0f766e 0%, #0d9488 60%, #14b8a6 100%);
            color: #fff;
            padding: 5px 24px 12px;
            border-radius: var(--radius);
            margin-bottom: 10px;
            box-shadow: 0 4px 20px rgba(13,148,136,.30);
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
            /*overflow:visible;*/
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

        /* ── Buttons ── */
        .btn {
            display: inline-flex; align-items: center; gap: 6px;
            padding: 8px 18px; border-radius: var(--radius-sm);
            font-size: 13px; font-weight: 500; cursor: pointer;
            border: none; transition: all .15s; text-decoration: none;
        }
        .btn-primary  { background: var(--accent); color: #fff; box-shadow: 0 2px 8px rgba(13,148,136,.25); }
        .btn-primary:hover { background: #0f766e; }
        .btn-success  { background: var(--accent2); color: #fff; box-shadow: 0 2px 8px rgba(5,150,105,.20); }
        .btn-success:hover { background: #047857; }
        .btn-outline  { background: #fff; color: var(--text-sub); border: 1px solid var(--border); }
        .btn-outline:hover { border-color: var(--accent); color: var(--accent); background: var(--accent-lt); }
        .btn-sm       { padding: 5px 12px; font-size: 12px; }
        .btn-danger   { background: var(--danger); color: #fff; box-shadow: 0 2px 8px rgba(220,38,38,.20); }
        .btn-danger:hover { background: #b91c1c; }

        /* ══════════════════════════════════════
           ACTION BAR (sticky bottom)
        ══════════════════════════════════════ */
        .action-bar {
            position: fixed; 
            bottom: 10px;
            right: 10px;
            background: #ffffff78;
            box-shadow: 0 -4px 20px rgba(0,0,0,.08);
            padding: 10px 24px;
            display: flex; align-items: center;
            justify-content: space-between; gap: 12px;
            z-index: 100;
            width:700px;
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
            background: var(--accent);
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

        ::-webkit-scrollbar { width: 12px; height: 12px; }
        ::-webkit-scrollbar-track { background: #e2e6ed; border-radius: 5px; }
        ::-webkit-scrollbar-thumb { background: #64748b; border-radius: 5px; border: 2px solid #e2e6ed;  }
        ::-webkit-scrollbar-thumb:hover { background: #334155; }
        ::-webkit-scrollbar-corner { background: #e2e6ed; } 
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
        .form-group input[type="text"] {
            border: 1.5px solid #c8d6e5;
            border-radius: 5px;
            padding: 4px 10px;
            font-size: 14px;
            color: #2c3e50;
            background: #fff;
            transition: border-color 0.2s;
            height: 28px;
        }
        .form-group input:focus {
            outline: none;
            border-color: #2980b9;
            box-shadow: 0 0 0 3px rgba(41,128,185,0.12);
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
        .tbl-wrap 
        { 
            overflow-x: auto;
            overflow-y: auto;
            position: relative;
            border: 1px solid #ddd;
            /*overflow:visible !important;*/
        }
        table.data-table {
            width: 100%;
            border-collapse: collapse;
            font-size: 12px;
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
            background-color: #0f766ee3;
            padding: 2px 4px;
            text-align:center;
        }
        .rowGv1 {
            font-size: 12px;
            padding: 4px 6px;
            background-color: #ffffff;
            text-align:center;
            vertical-align: middle;
        }
        .rowGv_toolNo{
            font-size: 12px;
            padding: 2px 4px;
            background-color: #ffffff;
            text-align:center;
            min-width:140px;
        }
        .img_tool {
            display: inline-block;
            width: 25px;
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
        .placeholder-cell {
            color: #cbd5e1;
            font-size: 11px;
            font-style: italic;
        }

        /* ── Edit Item : Current / New rows ── */
        .edit-compare-table th,
        .edit-compare-table td {
            text-align: center !important;
            vertical-align: middle !important;
            border: 1px solid #b9c2cf;
        }
        .edit-compare-table { border: 1px solid #b9c2cf; border-collapse: collapse; }

        .row-current td { background: #f7f8fa !important; color: var(--text-sub); }
        .row-current td:first-child { font-weight: 700; color: var(--text-sub); }
        .row-new td { background: var(--accent-lt) !important; }
        .row-new td:first-child { font-weight: 700; color: var(--accent); }

        .field-cell { display: flex; align-items: center; gap: 4px; justify-content: center; }
        .btn-toggle-field {
            border: 1px solid var(--border);
            background: #fff;
            border-radius: 5px;
            width: 24px; height: 24px;
            font-size: 12px;
            cursor: pointer;
            flex-shrink: 0;
            line-height: 1;
        }
        .btn-toggle-field:hover { border-color: var(--accent); background: var(--accent-lt); }
        .btn-toggle-field.active { background: var(--accent); color:#fff; border-color: var(--accent); }
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
        h4 { color: aliceblue; }  
        .modal-body{ max-height : 800px; }
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
        .badge-normal { background-color: #28a745; }
        .badge-spare { background-color: #ffc107; color: #000; }
        .badge-borrowing { background-color: #dc3545; color: #fff; }
    </style>

    <%--Show & HideLoading--%>
    <script>
        function hideLoading() { $('#loadingOverlay').hide(); }
        function showLoading(msg) {
            if (msg) { $('#loadingOverlayText').text(msg); }
            else { $('#loadingOverlayText').text('กำลังบันทึกข้อมูล...'); }
            $('#loadingOverlay').show();
        }
    </script>

    <%--Loading overlay--%>
    <style>
        .loading {
            height: 100vh; width: 100%;
            display: flex; align-items: center; justify-content: center;
            background: hsl(220deg 29% 90% / 50%);
            flex-direction:column; text-align:center;
        }
        .pl { width: 6em; height: 6em; }
        .pl__ring { animation: ringA 2s linear infinite; }
        .pl__ring--a { stroke: #f42f25; }
        .pl__ring--b { animation-name: ringB; stroke: #f49725; }
        .pl__ring--c { animation-name: ringC; stroke: #255ff4; }
        .pl__ring--d { animation-name: ringD; stroke: #f42582; }
        @keyframes ringA {
            from, 4% { stroke-dasharray: 0 660; stroke-width: 20; stroke-dashoffset: -330; }
            12% { stroke-dasharray: 60 600; stroke-width: 30; stroke-dashoffset: -335; }
            32% { stroke-dasharray: 60 600; stroke-width: 30; stroke-dashoffset: -595; }
            40%, 54% { stroke-dasharray: 0 660; stroke-width: 20; stroke-dashoffset: -660; }
            62% { stroke-dasharray: 60 600; stroke-width: 30; stroke-dashoffset: -665; }
            82% { stroke-dasharray: 60 600; stroke-width: 30; stroke-dashoffset: -925; }
            90%, to { stroke-dasharray: 0 660; stroke-width: 20; stroke-dashoffset: -990; }
        }
        @keyframes ringB {
            from, 12% { stroke-dasharray: 0 220; stroke-width: 20; stroke-dashoffset: -110; }
            20% { stroke-dasharray: 20 200; stroke-width: 30; stroke-dashoffset: -115; }
            40% { stroke-dasharray: 20 200; stroke-width: 30; stroke-dashoffset: -195; }
            48%, 62% { stroke-dasharray: 0 220; stroke-width: 20; stroke-dashoffset: -220; }
            70% { stroke-dasharray: 20 200; stroke-width: 30; stroke-dashoffset: -225; }
            90% { stroke-dasharray: 20 200; stroke-width: 30; stroke-dashoffset: -305; }
            98%, to { stroke-dasharray: 0 220; stroke-width: 20; stroke-dashoffset: -330; }
        }
        @keyframes ringC {
            from { stroke-dasharray: 0 440; stroke-width: 20; stroke-dashoffset: 0; }
            8% { stroke-dasharray: 40 400; stroke-width: 30; stroke-dashoffset: -5; }
            28% { stroke-dasharray: 40 400; stroke-width: 30; stroke-dashoffset: -175; }
            36%, 58% { stroke-dasharray: 0 440; stroke-width: 20; stroke-dashoffset: -220; }
            66% { stroke-dasharray: 40 400; stroke-width: 30; stroke-dashoffset: -225; }
            86% { stroke-dasharray: 40 400; stroke-width: 30; stroke-dashoffset: -395; }
            94%, to { stroke-dasharray: 0 440; stroke-width: 20; stroke-dashoffset: -440; }
        }
        @keyframes ringD {
            from, 8% { stroke-dasharray: 0 440; stroke-width: 20; stroke-dashoffset: 0; }
            16% { stroke-dasharray: 40 400; stroke-width: 30; stroke-dashoffset: -5; }
            36% { stroke-dasharray: 40 400; stroke-width: 30; stroke-dashoffset: -175; }
            44%, 50% { stroke-dasharray: 0 440; stroke-width: 20; stroke-dashoffset: -220; }
            58% { stroke-dasharray: 40 400; stroke-width: 30; stroke-dashoffset: -225; }
            78% { stroke-dasharray: 40 400; stroke-width: 30; stroke-dashoffset: -395; }
            86%, to { stroke-dasharray: 0 440; stroke-width: 20; stroke-dashoffset: -440; }
        }
    </style>

    <%-- Style : DropDown / Textbox ในตาราง Edit --%>
    <style>
        .ddl-edit {
            border: 1px solid var(--border);
            border-radius: var(--radius-sm);
            padding: 3px 6px;
            font-size: 12px;
            color: var(--text);
            background: #fff;
            min-width: 130px;
            cursor: pointer;
        }
        .ddl-edit:focus {
            outline: none;
            border-color: var(--accent);
            box-shadow: 0 0 0 2px rgba(13,148,136,.15);
        }
        input.ddl-edit { cursor: text; }

        /* ══════════════════════════════════════
           Req.5 : Textbox กรอกเอง (Manual) ให้ข้อความอยู่กลาง
        ══════════════════════════════════════ */
        input.ddl-edit,
        .field-cell input[type="text"] {
            text-align: center;
        }

        /* ══════════════════════════════════════
           Req.3 / Req.4 : Select2 (Search dropdown) สำหรับ
           Responsible / Model / Cell / Station
           - รายการที่แสดงตอนเปิด ddl : ชิดซ้าย
           - ข้อความที่เลือกแล้ว (ตอนปิด) : อยู่กลาง
        ══════════════════════════════════════ */
        .select2-container { min-width: 150px; }
        .select2-container .select2-selection--single {
            height: 28px !important;
            border: 1px solid var(--border) !important;
            border-radius: var(--radius-sm) !important;
        }
        .select2-container .select2-selection--single .select2-selection__rendered {
            line-height: 26px !important;
            text-align: center;         /* Req.4 : ข้อความที่เลือกแล้วอยู่กลาง */
            padding-left: 8px;
            padding-right: 20px;
        }
        .select2-container .select2-selection--single .select2-selection__arrow {
            height: 26px !important;
        }
        .select2-dropdown {
            border-color: var(--accent) !important;
        }
        .select2-search--dropdown .select2-search__field {
            text-align: left;           /* กล่องค้นหา : ชิดซ้ายตามปกติ */
            border: 1px solid var(--border) !important;
            border-radius: var(--radius-sm) !important;
        }
        .select2-results__option {
            text-align: left;           /* Req.4 : รายการในลิสต์ที่เปิดอยู่ ชิดซ้าย */
            font-size: 12px;
        }
        .select2-results__option--highlighted[aria-selected] {
            background-color: var(--accent) !important;
        }
        /* กรณีอยู่ใน field-cell (Model/Cell/Station ที่มีปุ่มสลับโหมดข้างๆ) */
        .field-cell .select2-container { flex: 1 1 auto; min-width: 0; }

        /* ══════════════════════════════════════
           Req.1.3.1 : ไฮไลท์ช่องที่ยังไม่มีข้อมูล / ยังไม่ได้เลือกตอนกด Save
        ══════════════════════════════════════ */
        .field-error,
        .select2-selection--single.field-error {
            border-color: var(--danger) !important;
            background: var(--danger-lt) !important;
            box-shadow: 0 0 0 2px rgba(220,38,38,.15) !important;
        }
    </style>

    <%--Date Style--%>
    <style>        
        .input-group-text {
            cursor: pointer;
            background-color: #f8f9fa;
        }
        .date-wrap {
            display: flex;
            align-items: center;
            /*background: rgba(255,255,255,0.15);*/
            background-color:white;
            border-radius: 10px;
            /*padding: 10px 12px;*/
            /*margin-bottom: 20px;*/
        }
        /* input zone */
        .date-field {
            position: relative;
            width: 100%;
        }    
        .date-input-wrapper {
            position: relative;
            /*pointer-events: none;*/
            /*background-color: #f8f9fa;*/
            /*cursor: default;*/
        }

        .date-input {
            padding-right: 35px; /* ✅ เว้นที่ให้ icon */
        }

        .calendar-icon {
            position: absolute;
            right: 5px;
            top: 50%;
            transform: translateY(-50%);
            cursor: pointer;
            color: #666;
            font-size: 14px;
            opacity:0.6;
        }
        .bootstrap-datetimepicker-widget {
            z-index: 99999 !important;
        }
        .bootstrap-datetimepicker-widget {
            width: 280px !important;
            min-width: 280px !important;
            max-width: 280px !important;
        }

    </style>

    <%--Date Picker--%>
    <script type="text/javascript">

        function initDatePicker() {

            var txtDueDate = $('#<%= txtDueDate.ClientID %>');

            // ✅ guard กัน init ซ้ำ
            if (txtDueDate.data("DateTimePicker")) return;

            function formatDate(date) {
                var day = ("0" + date.getDate()).slice(-2);
                var monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
                    "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
                var month = monthNames[date.getMonth()];
                var year = date.getFullYear();

                return day + "-" + month + "-" + year;
            }

            txtDueDate.datetimepicker({
                format: 'DD-MMM-YYYY',
                allowInputToggle: false,
                ignoreReadonly: true
            });

            txtDueDate.on("dp.change", function (e) {

                if (e.date) {

                    $(this).val(
                        e.date.format('DD-MMM-YYYY')
                    );

                    console.log(
                        "Date Selected : " +
                        e.date.format('DD-MMM-YYYY')
                    );
                }
            });

            txtDueDate.on("dp.show", function () {
                var widget = $(".bootstrap-datetimepicker-widget:last");
                var input = txtDueDate;
                var offset = input.offset();

                widget.appendTo("body");

                // ต้องรอให้ widget render เสร็จก่อนถึงจะวัดความสูงได้
                var widgetHeight = widget.outerHeight();

                widget.css({
                    "top": (offset.top - widgetHeight) + "px",  // ✅ ขึ้นบน = top ของ input ลบความสูงปฏิทิน
                    "left": offset.left + "px",
                    "bottom": "auto",
                    "position": "absolute",
                    "z-index": "999999"
                });
            });

            $(".calendarFrom").off("click").on("click", function () {
                txtDueDate.data("DateTimePicker").show();
            });

            $(".clearDueDate").off("click").on("click", function () {
                var picker = txtDueDate.data("DateTimePicker");
                if (picker) picker.clear();   // เคลียร์ค่าใน picker
                txtDueDate.val("");           // เคลียร์ textbox ให้ว่างจริง -> SQL จะเป็น NULL
            });

            // sync สถานะปุ่ม toggle Due Date หลัง postback
            (function () {
                var hd = document.getElementById('<%= hdModeDueDate.ClientID %>');
                var btn = document.getElementById('btnToggleDueDate');
                var wrapper = document.querySelector('.date-input-wrapper');
                if (!hd || !btn || !wrapper) return;
                if (hd.value === 'on') {
                    btn.classList.add('active');
                    wrapper.style.opacity = '1';
                    wrapper.style.pointerEvents = '';
                } else {
                    btn.classList.remove('active');
                    wrapper.style.opacity = '0.4';
                    wrapper.style.pointerEvents = 'none';
                }
            })();

        }

        // ✅ โหลดครั้งแรก
        $(document).ready(function () {
            initDatePicker();
            updateRemarkCount();
        });

        // ✅ 🔥 สำคัญมาก: หลัง PostBack (UpdatePanel / AutoPostBack)
        if (typeof (Sys) !== "undefined") {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                initDatePicker();
                updateRemarkCount();
            });
        }
    </script>

    <%--Count txtRemark--%>
    <script>
        function updateRemarkCount() {

            var txt = document.getElementById('<%= txtRemark.ClientID %>');

            if (!txt) return;

            var len = txt.value.length;

            $('#remarkCount').text(len);

            if (len >= 200) {
                $('#remarkCount').css('color', 'red');
            }
            else {
                $('#remarkCount').css('color', '#666');
            }
        }
    </script>

    <%--Text Remark--%>
    <style>
        .remark-box {
            width: 100%;
            min-height: 70px;
            border: 1px solid #c8d6e5;
            border-radius: 6px;
            padding: 8px;
            resize: vertical;
        }

        .remark-box:focus {
            border-color: #0d9488;
            box-shadow: 0 0 0 3px rgba(13,148,136,.12);
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
        
                    <div id="loadingOverlayText" style="margin-top:20px; font-size:18px;">
                        กำลังบันทึกข้อมูล...
                    </div>
                </div>              
            </div>

            <!-- Toast Container -->
            <div class="toast-container" id="toastContainer"></div>

            <!-- flag: มีรายการที่กำลังแก้ไขค้างอยู่หรือไม่ (ใช้ตอนกด Edit ทับรายการเดิม) -->
            <script>
                var hasPendingEditItem = <%= (ViewState["EditLocationItem"] != null) ? "true" : "false" %>;
            </script>

            <div id="form1" runat="server">

                <div class="page-wrapper">

                    <!-- ── HEADER ── -->
                    <div class="page-header" style="margin-top:-20px;">
                        <img src="images/location-pin.png" alt="icon" onerror="this.style.display='none'" />
                        <div>
                            <h3>Edit Location</h3>
                            <p>แก้ไข Dept / Responsible / Model / Cell / Station ของ Jig Tool &amp; Equipment (ทีละ 1 รายการ)</p>
                        </div>
                    </div>

                    <%------------ SECTION 1 : Search & Select --------------%>           
                    <asp:Panel ID="pnlSearch_Add" runat="server" Visible="true" class="card" style="">

                        <div class="import-card">

                            <div class="import-card-head">
                                <div class="import-card-head-left">
                                    <div class="card-icon">📍</div>
                                    <div>
                                        <div class="card-title">Search : ค้นหารายการที่ต้องการแก้ไข Location</div>
                                    </div>
                                </div>
                            </div>

                            <div class="card-body" style="margin-bottom:10px;">

                                <div id="tabFixAsset" class="grid_search">

                                    <div class="form-row" style="margin-left:15px;">                            
                                        <div class="form-group" style="min-width:200px; align-self:center;">
                                            <asp:TextBox ID="txtFixAsset" runat="server" Width="200px" 
                                                placeholder="ค้นหารายการ"
                                                Style="text-align:center; background:#00a7ff40;"/>
                                        </div>
                                        <asp:Button ID="btnSearchFixAsset" runat="server" Text="Search"
                                            CssClass="btn btn-primary"
                                            OnClick="btnSearch_Click"
                                            OnClientClick="showLoading('กำลังค้นหาข้อมูล...');"
                                            style="align-self:center; margin-top:-5px;"/>                               
                                    </div>
                            
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

                                                <asp:BoundField  DataField="RowNo"  
                                                                 HeaderText="#" HeaderStyle-CssClass="headerGv1"
                                                                 ItemStyle-CssClass="rowGv1" ItemStyle-Width="40px" />

                                                <%-- เปลี่ยนจาก ButtonField "Add" เป็น TemplateField "Edit"
                                                     เพื่อให้ใส่ OnClientClick ยืนยันทับรายการเดิมได้ --%>
                                                <asp:TemplateField HeaderText="Edit" ItemStyle-Width="65px"
                                                    HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1">
                                                    <ItemTemplate>
                                                        <asp:Button ID="btnEditItem" runat="server" Text="Edit"
                                                            CssClass="btn btn-success btn-sm"
                                                            CommandName="EditItem"
                                                            CommandArgument="<%# Container.DataItemIndex %>"
                                                            OnClientClick="return confirmEditReplace();" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Detail" HeaderStyle-CssClass="headerGv1" ItemStyle-Width="65px"
                                                    ItemStyle-CssClass="rowGv1" ItemStyle-HorizontalAlign="Center" ItemStyle-Wrap="false">
                                                    <ItemTemplate>
                                                        <asp:LinkButton ID="btn_detailView" data-toggle="tooltip" title="View Detail" runat="server"
                                                            OnClick="btn_detailView_Click" CssClass="img_tool">
                                                            <img src="images/eye-tracking.png" />
                                                        </asp:LinkButton>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

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

                                                <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="headerGv1" ItemStyle-Width="140px"
                                                    ItemStyle-CssClass="rowGv1" ItemStyle-HorizontalAlign="Center">
                                                    <ItemTemplate>   
                                                        <asp:HiddenField ID="hdstatusId" runat="server" Value='<%# Eval("status_id") %>' />
    
                                                        <span class='badge <%# GetStatusBadgeClass(Eval("DisplayStatus").ToString()) %>'>
                                                            <%# Eval("DisplayStatus") %>
                                                        </span>

                                                    </ItemTemplate>
                                                </asp:TemplateField>
                
                                                <asp:BoundField  DataField="sub_no" ItemStyle-Width="50px"  
                                                                 HeaderText="No" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                                <asp:BoundField  DataField="FixAcces" ItemStyle-Width="110px"  
                                                                 HeaderText="Fix Asset" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                                <asp:BoundField  DataField="control_no" ItemStyle-Width="130px"     
                                                                 HeaderText="Control No" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                                <asp:BoundField  DataField="tool_no"      
                                                                 HeaderText="Tool No / Model" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                                <asp:BoundField  DataField="dept_code" ItemStyle-Width="65px"  
                                                                 HeaderText="Dept" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                                <asp:BoundField  DataField="model_name"
                                                                 HeaderText="CPT Model" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                                <asp:BoundField  DataField="cell" 
                                                                 HeaderText="Cell" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                                <asp:BoundField  DataField="station"
                                                                 HeaderText="Station" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>

                                            </Columns>

                                        </asp:GridView>
                                    </div>
                                </div>

                            </div>

                        </div>

                    </asp:Panel>

                    <!-- ── SECTION 2 : Edit Item (ทีละ 1 รายการ) : Current / New ── -->
                    <asp:Panel ID="editItemCard" runat="server" Visible="false" CssClass="import-card">

                        <div class="import-card-head">
                            <div class="import-card-head-left">
                                <div class="card-icon">📍</div>
                                <div>
                                    <div class="card-title">Edit Location : รายการที่กำลังแก้ไข</div>
                                </div>
                            </div>
                            <asp:Button ID="btnClearEditItem" runat="server" Text="✖ ยกเลิกรายการนี้"
                                CssClass="btn btn-outline btn-sm"
                                OnClick="btnClearEditItem_Click"
                                OnClientClick="return confirm('ยืนยันยกเลิกรายการที่กำลังแก้ไข (ข้อมูลที่ยังไม่บันทึกจะหายไป) ?');" />
                        </div>

                        <div class="card-body" style="padding:15px;">

                            <div class="page-note" style="margin-bottom:10px;">
                                <span class="badge" style="background:#7462ff;">Fix Asset : <asp:Label ID="lblFixAsset" runat="server" /></span>
                                <span class="badge" style="background:#7462ff;">Control No : <asp:Label ID="lblControlNo" runat="server" /></span>
                                <span class="badge" style="background:#7462ff;">Tool No / Model : <asp:Label ID="lblToolNo" runat="server" /></span>
                                <span class="badge" style="background:#7462ff;">No : <asp:Label ID="lblToolSub" runat="server" /></span>
                            </div>

                            <div class="tbl-wrap" style="">
                                <table class="data-table edit-compare-table" style="min-width:1100px;">
                                    <thead>
                                        <tr>
                                            <th class="headerGv1" style="width:110px;"></th>
                                            <th class="headerGv2">Status</th>
                                            <th class="headerGv2">Dept Code</th>
                                            <th class="headerGv2">Responsible</th>
                                            <th class="headerGv2">Model</th>
                                            <th class="headerGv2">Cell</th>
                                            <th class="headerGv2">Station</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <%-- แถวที่ 1 : ข้อมูลปัจจุบัน --%>
                                        <tr class="row-current">
                                            <td class="rowGv1">ปัจจุบัน (Current)</td>
                                            <td class="rowGv1"><asp:Label ID="lblCurStatus" runat="server" /></td>
                                            <td class="rowGv1"><asp:Label ID="lblCurDept" runat="server" /></td>
                                            <td class="rowGv1"><asp:Label ID="lblCurResponsible" runat="server" style="min-width:200px;" /></td>
                                            <td class="rowGv1"><asp:Label ID="lblCurModel" runat="server" /></td>
                                            <td class="rowGv1"><asp:Label ID="lblCurCell" runat="server" /></td>
                                            <td class="rowGv1"><asp:Label ID="lblCurStation" runat="server" /></td>
                                        </tr>

                                        <%-- แถวที่ 2 : ข้อมูลที่จะแก้ไขใหม่ --%>
                                        <tr class="row-new">
                                            <td class="rowGv1">แก้ไขใหม่ (New)</td>

                                            <%-- Status : Dropdown ธรรมดา ไม่มี Search, ไม่บังคับเลือก (Req.1.1.2 / 1.2) --%>
                                            <td class="rowGv1">
                                                <asp:DropDownList ID="ddlNewStatus" runat="server" CssClass="ddl-edit" style="text-align:center; width:140px;"/>
                                            </td>

                                            <%-- Dept : Dropdown เท่านั้น (ไม่มีปุ่มกรอกเอง) --%>
                                            <%--<td class="rowGv1">
                                                <asp:DropDownList ID="ddlNewDept" runat="server" CssClass="ddl-edit" style="text-align:center; width:120px;"/>
                                            </td>--%>

                                            <%-- Responsible : Dropdown เท่านั้น (จาก Responsible_Dept) : Req.3/4 เพิ่ม Search ผ่าน select2 --%>
                                           <%-- <td class="rowGv1">
                                                <asp:DropDownList ID="ddlNewResponsible" runat="server" CssClass="ddl-edit select2-searchable" style="min-width:150px; text-align:center;" />
                                            </td>--%>

                                            <%-- Dept : เพิ่ม AutoPostBack เพื่อกรอง Responsible --%>
                                            <td class="rowGv1">
                                                <asp:DropDownList ID="ddlNewDept" runat="server" CssClass="ddl-edit" style="text-align:center; width:120px;"
                                                    AutoPostBack="true" OnSelectedIndexChanged="ddlNewDept_SelectedIndexChanged" />
                                            </td>

                                            <%-- Responsible : ซ่อนจนกว่าจะเลือก Dept --%>
                                            <td class="rowGv1" style="min-width:180px;">
                                                <asp:Label ID="lblRespPlaceholder" runat="server" Text="-- กรุณาเลือก Dept ก่อน --" CssClass="placeholder-cell" />
                                                <asp:DropDownList ID="ddlNewResponsible" runat="server" CssClass="ddl-edit select2-searchable" style="min-width:180px; text-align:center;" Visible="false" />
                                            </td>

                                            <%-- Model : Dropdown / Textbox toggle : Req.3/4 เพิ่ม Search ผ่าน select2 --%>
                                            <td class="rowGv1">
                                                <div class="field-cell">
                                                    <asp:DropDownList ID="ddlNewModel" runat="server" CssClass="ddl-edit select2-searchable" style="min-width:150px; text-align:center;" />
                                                    <asp:TextBox ID="txtNewModel" runat="server" CssClass="ddl-edit"
                                                        placeholder="ระบุ Model ID" Style="display:none;" />
                                                    <asp:HiddenField ID="hdModeModel" runat="server" Value="ddl" />
                                                    <button type="button" class="btn-toggle-field" title="สลับ Dropdown / กรอกเอง"
                                                        data-ddlid="<%= ddlNewModel.ClientID %>"
                                                        data-txtid="<%= txtNewModel.ClientID %>"
                                                        data-hdid="<%= hdModeModel.ClientID %>"
                                                        onclick="toggleField(this); return false;">✏️</button>
                                                </div>
                                            </td>

                                            <%-- Cell : Dropdown / Textbox toggle : Req.3/4 เพิ่ม Search ผ่าน select2 --%>
                                            <td class="rowGv1">
                                                <div class="field-cell">
                                                    <asp:DropDownList ID="ddlNewCell" runat="server" CssClass="ddl-edit select2-searchable" style="min-width:150px;  text-align:center;" />
                                                    <asp:TextBox ID="txtNewCell" runat="server" CssClass="ddl-edit"
                                                        placeholder="ระบุ Cell ID" Style="display:none;" />
                                                    <asp:HiddenField ID="hdModeCell" runat="server" Value="ddl" />
                                                    <button type="button" class="btn-toggle-field" title="สลับ Dropdown / กรอกเอง"
                                                        data-ddlid="<%= ddlNewCell.ClientID %>"
                                                        data-txtid="<%= txtNewCell.ClientID %>"
                                                        data-hdid="<%= hdModeCell.ClientID %>"
                                                        onclick="toggleField(this); return false;">✏️</button>
                                                </div>
                                            </td>

                                            <%-- Station : Dropdown / Textbox toggle : Req.3/4 เพิ่ม Search ผ่าน select2 --%>
                                            <td class="rowGv1">
                                                <div class="field-cell">
                                                    <asp:DropDownList ID="ddlNewStation" runat="server" CssClass="ddl-edit select2-searchable" style="min-width:150px;  text-align:center;" />
                                                    <asp:TextBox ID="txtNewStation" runat="server" CssClass="ddl-edit"
                                                        placeholder="ระบุ Station ID" Style="display:none;" />
                                                    <asp:HiddenField ID="hdModeStation" runat="server" Value="ddl" />
                                                    <button type="button" class="btn-toggle-field" title="สลับ Dropdown / กรอกเอง"
                                                        data-ddlid="<%= ddlNewStation.ClientID %>"
                                                        data-txtid="<%= txtNewStation.ClientID %>"
                                                        data-hdid="<%= hdModeStation.ClientID %>"
                                                        onclick="toggleField(this); return false;">✏️</button>
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>

                            <div class="tbl-wrap" style="margin-top:10px;">
                                <table class="data-table edit-compare-table" style="min-width:1100px;">
                                    <thead>
                                        <tr>
                                            <th class="headerGv1" style="width:80px;"></th>
                                            <th class="headerGv2" style="width:220px;">Due Date (*Calibration)</th>
                                            <th class="headerGv2" style="width:Auto;">Remark</th>

                                        </tr>
                                    </thead>
                                    <tbody>
                                        <%-- แถวที่ 1 : ข้อมูลปัจจุบัน --%>
                                        <tr class="row-current">
                                            <td class="rowGv1">ปัจจุบัน (Current)</td>
                                            <td class="rowGv1"><asp:Label ID="lblCurDueDate" runat="server" /></td>
                                            <td class="rowGv1" style="text-align:left !important;"><asp:Label ID="lblCurRemark" runat="server"/></td>
                                        </tr>

                                        <%-- แถวที่ 2 : ข้อมูลที่จะแก้ไขใหม่ --%>
                                        <tr class="row-new">
                                            <td class="rowGv1">แก้ไขใหม่ (New)</td>

                                            <td class="rowGv1">
                                                <div class="field-cell">
                                                    <div class="date-input-wrapper" style="flex:1 1 auto;">
                                                        <asp:TextBox ID="txtDueDate" runat="server"
                                                            CssClass="form-control date-input"
                                                            Style="text-align:center; background: #ffffa0;"
                                                            onkeydown="return false;" />
                                                        <span class="calendar-icon calendarFrom">📅</span>
                                                        <%--<span class="calendar-icon clearDueDate" style="right:28px;" title="ล้างวันที่">✖</span>--%>
                                                    </div>
                                                    <asp:HiddenField ID="hdModeDueDate" runat="server" Value="off" />
                                                    <button type="button" class="btn-toggle-field" id="btnToggleDueDate"
                                                        title="สลับ แก้ไข / ไม่แก้ไข Due Date"
                                                        onclick="toggleDueDate(this); return false;">✏️</button>
                                                </div>
                                            </td>
                                            <td class="rowGv1">

                                                <div style="text-align:left;font-size:11px;color:#666;">
                                                    (จำนวนตัวอักษร :
                                                    <span id="remarkCount">0</span>/200 )
                                                </div>

                                                <div style="position:relative;">
                                                    <asp:TextBox ID="txtRemark"
                                                    runat="server"
                                                    CssClass="remark-box"
                                                    TextMode="MultiLine"
                                                    Rows="2"
                                                    MaxLength="200"
                                                    onkeyup="updateRemarkCount();" />
                                                     
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>

                        </div>

                    </asp:Panel>

                </div><!-- /page-wrapper -->

                <!-- ── ACTION BAR ── -->
                <div class="action-bar">
                    <div class="action-bar-left">
                        <div class="status-pill">
                            <div class="status-dot"></div>
                            <asp:Label ID="lblActionStatus" runat="server" Text="รอเลือกรายการสำหรับแก้ไข Location" />
                        </div>
                    </div>
                    <div class="action-bar-right">
                        <button type="button" class="btn btn-outline"
                            onclick="history.back()">ยกเลิก</button>
                        <asp:Button ID="btnSave" runat="server"
                            Text="📍 บันทึกการแก้ไข"
                            CssClass="btn btn-success"
                            Style="padding:8px 28px;"
                            OnClientClick="return confirmSave();"
                            Enabled="false" />

                        <%-- ปุ่มจริงที่ทำการบันทึกลง DB (ซ่อนไว้) ถูกกดผ่าน JS หลังผู้ใช้กด "ยืนยันบันทึก" ใน Modal สรุป --%>
                        <asp:Button ID="btnSaveConfirmed" runat="server" Text=""
                            Style="display:none;"
                            UseSubmitBehavior="false"
                            OnClick="btnSaveConfirmed_Click" />
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

            <%-- Modal สรุปรายละเอียดก่อนยืนยันบันทึก (ข้อ 1.1) --%>
            <div class="modal fade" id="modal_ConfirmSave" tabindex="-1" role="dialog">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header" style="background:var(--accent); color:#fff;">
                            <button type="button" class="close" data-dismiss="modal" style="color:#fff; opacity:1;">&times;</button>
                            <h4 class="modal-title">📍 ยืนยันการแก้ไข Location</h4>
                        </div>
                        <div class="modal-body">
                            <%-- รายละเอียดรายการที่กำลังแก้ไข (ค่าคงที่ ไม่เปลี่ยน before/after) --%>
                            <div class="page-note" style="margin-bottom:10px;">
                                <span class="badge" style="background:#7462ff;">Tool No / Model : <span id="sumInfoToolNo"></span></span>
                                <span class="badge" style="background:#7462ff;">Fix Asset : <span id="sumInfoFixAsset"></span></span>
                                <span class="badge" style="background:#7462ff;">Control No : <span id="sumInfoControlNo"></span></span>
                                <span class="badge" style="background:#7462ff;">No : <span id="sumInfoToolSub"></span></span>
                            </div>

                            <div class="tbl-wrap" style="overflow:visible;">
                                <table class="data-table edit-compare-table" style="width:100%;">
                                    <thead>
                                        <tr>
                                            <th class="headerGv1"></th>
                                            <th class="headerGv1">ปัจจุบัน</th>
                                            <th class="headerGv2">แก้ไขใหม่</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr><td class="rowGv1" style="font-weight:600;">Status</td><td class="rowGv1" id="sumCurStatus"></td><td class="rowGv1" id="sumNewStatus"></td></tr>
                                        <tr><td class="rowGv1" style="font-weight:600;">Dept</td><td class="rowGv1" id="sumCurDept"></td><td class="rowGv1" id="sumNewDept"></td></tr>
                                        <tr><td class="rowGv1" style="font-weight:600;">Responsible</td><td class="rowGv1" id="sumCurResp"></td><td class="rowGv1" id="sumNewResp"></td></tr>
                                        <tr><td class="rowGv1" style="font-weight:600;">Model</td><td class="rowGv1" id="sumCurModel"></td><td class="rowGv1" id="sumNewModel"></td></tr>
                                        <tr><td class="rowGv1" style="font-weight:600;">Cell</td><td class="rowGv1" id="sumCurCell"></td><td class="rowGv1" id="sumNewCell"></td></tr>
                                        <tr><td class="rowGv1" style="font-weight:600;">Station</td><td class="rowGv1" id="sumCurStation"></td><td class="rowGv1" id="sumNewStation"></td></tr>
                                        <tr><td class="rowGv1" style="font-weight:600;">Due Date</td><td class="rowGv1" id="sumCurDueDate"></td><td class="rowGv1" id="sumNewDueDate"></td></tr>
                                        <tr><td class="rowGv1" style="font-weight:600;">Remark</td><td class="rowGv1" id="sumCurRemark"></td><td class="rowGv1" id="sumNewRemark"></td></tr>
                                    </tbody>
                                </table>
                            </div>
                            <p id="sumManualNote" style="color:var(--warn); font-size:12px; margin-top:10px; display:none;">
                                ⚠️ มีบาง field ที่กรอกข้อมูลเอง (Manual) ระบบจะเพิ่มรายการใหม่ลง Master Data ให้อัตโนมัติหากยังไม่มีในระบบ
                            </p>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-outline" data-dismiss="modal">ยกเลิก</button>
                            <button type="button" class="btn btn-success" onclick="submitConfirmedSave();">✔ ยืนยันบันทึก</button>
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
           Req.1 : Edit (Search grid) : ยืนยันทับรายการเดิม
           ที่ยังไม่ได้ Save (hasPendingEditItem ถูก render จาก server)
        ════════════════════════════════ */
        function confirmEditReplace() {
            if (typeof hasPendingEditItem !== 'undefined' && hasPendingEditItem) {
                if (!confirm(
                    '⚠️ มีรายการที่กำลังแก้ไขค้างอยู่ (ยังไม่ได้บันทึก)\n\n' +
                    'ถ้าเลือกรายการใหม่นี้ รายการเดิมที่ยังไม่บันทึกจะถูกยกเลิกทันที\n' +
                    'ต้องการเปลี่ยนไปแก้ไขรายการใหม่นี้แทนหรือไม่ ?'
                )) {
                    return false;
                }
            }
            showLoading('กำลังโหลดข้อมูลรายการ...');
            return true;
        }

        /* ════════════════════════════════
           Req.3/4 : Select2 (Searchable Dropdown) สำหรับ
           Responsible / Model / Cell / Station
        ════════════════════════════════ */
        function initSelect2Fields() {
            $('.select2-searchable').each(function () {
                var $el = $(this);
                if ($el.hasClass('select2-hidden-accessible')) {
                    $el.select2('destroy');
                }
                $el.select2({
                    width: 'resolve',
                    dropdownAutoWidth: true,
                    sorter: rankSorter,          // จัดอันดับผลลัพธ์
                    matcher: rankMatcher         // filter + เก็บคะแนนไว้ให้ sorter
                });
            });

            $('.select2-searchable').off('select2:open.editLocation').on('select2:open.editLocation', function () {
                setTimeout(function () {
                    var searchField = document.querySelector('.select2-container--open .select2-search__field');
                    if (searchField) searchField.focus();
                }, 0);
            });

            syncToggleButtonsWithSelect2();
        }

        // ลบ . , - _ และช่องว่างส่วนเกิน ออกก่อนเทียบ
        function normalizeText(s) {
            return (s || '').toLowerCase().replace(/[.\,\-_\/]/g, '');
        }

        // คำนวณคะแนนความเกี่ยวข้อง : ยิ่งน้อยยิ่งตรง (มาก่อน)
        function matchScore(rawText, rawTerm) {
            var text = normalizeText(rawText);
            var term = normalizeText(rawTerm).trim();
            if (term === '') return 0;

            var idx = text.indexOf(term);
            if (idx === -1) return -1;              // ไม่พบ -> ตัดทิ้ง

            if (text === term) return 0;            // ตรงเป๊ะทั้งคำ
            if (idx === 0) return 1;                // ขึ้นต้นด้วยคำค้นหา (St.1 ...)

            // ขึ้นต้นของ "คำ" ใดคำหนึ่ง — เทียบกับข้อความต้นฉบับที่ space ยังอยู่
            // แต่เนื่องจาก text ถูกลบตัวคั่นไปแล้ว ให้เช็ค word-boundary จากต้นฉบับแทน
            if (isWordStart(rawText, rawTerm)) return 2;   // ACC St.1

            return 3 + idx;                          // substring กลางคำ
        }

        function rankMatcher(params, data) {
            // ไม่มีคำค้นหา -> แสดงทั้งหมด (ตามลำดับเดิม)
            if ($.trim(params.term) === '') return data;
            if (typeof data.text === 'undefined') return null;

            var score = matchScore(data.text, params.term);
            if (score < 0) return null;

            // clone แล้วแนบคะแนนไว้ให้ sorter อ่าน
            var modified = $.extend({}, data, true);
            modified._score = score;
            return modified;
        }
        function rankSorter(results) {
            return results.sort(function (a, b) {
                var sa = (typeof a._score === 'undefined') ? 999 : a._score;
                var sb = (typeof b._score === 'undefined') ? 999 : b._score;
                if (sa !== sb) return sa - sb;              // คะแนนน้อย = ตรงกว่า
                return (a.text || '').localeCompare(b.text || ''); // คะแนนเท่ากัน -> เรียง ก-ฮ / A-Z
            });
        }
        // เช็คว่า term ไปตรงกับ "ต้นคำ" ในข้อความต้นฉบับหรือไม่ (space เป็นตัวแบ่งคำจริงๆ)
        function isWordStart(rawText, rawTerm) {
            var term = normalizeText(rawTerm).trim();
            if (term === '') return false;
            var words = (rawText || '').toLowerCase().split(/\s+/);
            for (var i = 0; i < words.length; i++) {
                if (normalizeText(words[i]).indexOf(term) === 0) return true;
            }
            return false;
        }

        // ค้นหาแบบไม่สนตัวพิมพ์เล็ก/ใหญ่ และค้นได้ทั้งไทย/อังกฤษปนกัน (ค้นแบบ contains ธรรมดา)
        //function select2ThaiEngMatcher(params, data) {
        //    if ($.trim(params.term) === '') return data;
        //    if (typeof data.text === 'undefined') return null;
        //    var term = params.term.toLowerCase();
        //    var text = data.text.toLowerCase();
        //    if (text.indexOf(term) > -1) return data;
        //    return null;
        //}

        // เมื่อ toggleField ซ่อน/แสดง <select> เดิม ต้องซ่อน/แสดง .select2-container ที่ select2 สร้างขึ้นแทนด้วย
        function syncToggleButtonsWithSelect2() {
            document.querySelectorAll('.btn-toggle-field').forEach(function (btn) {
                var ddlId = btn.getAttribute('data-ddlid');
                var ddl = document.getElementById(ddlId);
                if (!ddl) return;
                var $ddl = $(ddl);
                var $container = $ddl.next('.select2-container');
                var hd = document.getElementById(btn.getAttribute('data-hdid'));
                if (hd && hd.value === 'txt') {
                    $container.hide();
                } else {
                    $container.show();
                }
            });
        }

        $(function () {
            initSelect2Fields();
        });

        // Req.1.3.1 : เคลียร์ไฮไลท์สีแดงทันทีที่ผู้ใช้แก้ไข/เลือกช่องนั้นแล้ว
        $(document).on('change select2:select', '#form1 select, #form1 input.ddl-edit', function () {
            $(this).removeClass('field-error');
            $(this).next('.select2-container').find('.select2-selection--single').removeClass('field-error');
        });

        // ต้อง re-init select2 ทุกครั้งหลัง UpdatePanel ทำ partial postback (DOM ของ dropdown ถูกสร้างใหม่)
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                initSelect2Fields();
                hideLoading();   // ปิด overlay หลัง Search / Edit / Save เสร็จทุกครั้ง
            });
        }

        /* ════════════════════════════════
           Toggle Dropdown <-> Textbox ต่อ field
           (แก้ให้รองรับ select2 : ต้องซ่อน/แสดง .select2-container แทน <select> เดิม
            เพราะ select2 จะซ่อน <select> ต้นฉบับด้วย CSS ของตัวเองอยู่แล้วเสมอ)
        ════════════════════════════════ */
        function toggleField(btn) {
            var ddl = document.getElementById(btn.getAttribute('data-ddlid'));
            var txt = document.getElementById(btn.getAttribute('data-txtid'));
            var hd = document.getElementById(btn.getAttribute('data-hdid'));
            if (!ddl || !txt || !hd) return;

            var $ddl = $(ddl);
            var $container = $ddl.next('.select2-container');

            if (hd.value === 'ddl') {
                if ($container.length) { $container.hide(); } else { ddl.style.display = 'none'; }
                txt.style.display = '';
                txt.value = '';
                txt.focus();
                hd.value = 'txt';
                btn.classList.add('active');
            } else {
                txt.style.display = 'none';
                if ($container.length) { $container.show(); } else { ddl.style.display = ''; }
                hd.value = 'ddl';
                btn.classList.remove('active');
            }
        }

        function toggleDueDate(btn) {
            var hd = document.getElementById('<%= hdModeDueDate.ClientID %>');
            var wrapper = document.querySelector('.date-input-wrapper');
            var txt = document.getElementById('<%= txtDueDate.ClientID %>');
            if (!hd || !wrapper || !txt) return;

            if (hd.value === 'off') {
                // เปิดโหมดแก้ไข
                hd.value = 'on';
                btn.classList.add('active');
                wrapper.style.opacity = '1';
                wrapper.style.pointerEvents = '';
            } else {
                // ปิดโหมดแก้ไข -> คงค่าเดิม, ล้างค่าที่เลือก
                hd.value = 'off';
                btn.classList.remove('active');
                wrapper.style.opacity = '0.4';
                wrapper.style.pointerEvents = 'none';
                var picker = $(txt).data("DateTimePicker");
                if (picker) picker.clear();
                txt.value = '';
            }
        }

        /* ════════════════════════════════
           Save : เปิด Modal สรุป -> ผู้ใช้กดยืนยันในนั้น
           (ปุ่ม บันทึกการแก้ไข ที่มองเห็น จะไม่ postback ตรงๆ)
           หมายเหตุ : ไม่มีการบังคับเลือกให้ครบทุกช่องอีกต่อไป —
           field ไหนไม่เลือก ระบบจะคงค่าเดิมไว้ (ไม่ Update / ไม่แจ้งเตือน)
        ════════════════════════════════ */
        function fieldModeVal(key) {
            var hdId = key === 'Model' ? '<%= hdModeModel.ClientID %>'
                : key === 'Cell' ? '<%= hdModeCell.ClientID %>'
                    : '<%= hdModeStation.ClientID %>';
            var hd = document.getElementById(hdId);
            return hd ? hd.value : 'ddl';
        }
        function fieldDisplayValue(key) {
            var mode = fieldModeVal(key);
            if (mode === 'txt') {
                var txtId = key === 'Model' ? '<%= txtNewModel.ClientID %>'
                    : key === 'Cell' ? '<%= txtNewCell.ClientID %>'
                        : '<%= txtNewStation.ClientID %>';
                var txt = document.getElementById(txtId);
                var val = txt ? txt.value.trim() : '';
                return val ? (val + ' (กรอกเอง)') : '';
            } else {
                var ddlId = key === 'Model' ? '<%= ddlNewModel.ClientID %>'
                    : key === 'Cell' ? '<%= ddlNewCell.ClientID %>'
                        : '<%= ddlNewStation.ClientID %>';
                var ddl = document.getElementById(ddlId);
                return (ddl && ddl.selectedIndex >= 0 && ddl.value !== '') ? ddl.options[ddl.selectedIndex].text : '';
            }
        }
        function selectText(id) {
            var el = document.getElementById(id);
            return (el && el.selectedIndex >= 0 && el.value !== '') ? el.options[el.selectedIndex].text : '';
        }
        function labelText(id) {
            var el = document.getElementById(id);
            return el ? el.innerText : '';
        }

        function confirmSave() {
            // ── รายละเอียดรายการที่กำลังแก้ไข (ค่าคงที่) : Tool No / Model, Fix Asset, Control No ──
            document.getElementById('sumInfoToolNo').innerText = labelText('<%= lblToolNo.ClientID %>');
            document.getElementById('sumInfoFixAsset').innerText = labelText('<%= lblFixAsset.ClientID %>');
            document.getElementById('sumInfoControlNo').innerText = labelText('<%= lblControlNo.ClientID %>');
            document.getElementById('sumInfoToolSub').innerText = labelText('<%= lblToolSub.ClientID %>');

            // เติมข้อมูลลง Modal สรุป (field ไหนไม่ได้เลือก จะแสดง "(ไม่เปลี่ยน)" แทน)
            document.getElementById('sumCurStatus').innerText = labelText('<%= lblCurStatus.ClientID %>');
            document.getElementById('sumNewStatus').innerText = selectText('<%= ddlNewStatus.ClientID %>') || '(ไม่เปลี่ยน)';
            document.getElementById('sumCurDept').innerText = labelText('<%= lblCurDept.ClientID %>');
            document.getElementById('sumCurResp').innerText = labelText('<%= lblCurResponsible.ClientID %>');
            document.getElementById('sumCurModel').innerText = labelText('<%= lblCurModel.ClientID %>');
            document.getElementById('sumCurCell').innerText = labelText('<%= lblCurCell.ClientID %>');
            document.getElementById('sumCurStation').innerText = labelText('<%= lblCurStation.ClientID %>');

            document.getElementById('sumNewDept').innerText = selectText('<%= ddlNewDept.ClientID %>') || '(ไม่เปลี่ยน)';
            document.getElementById('sumNewResp').innerText = selectText('<%= ddlNewResponsible.ClientID %>') || '(ไม่เปลี่ยน)';
            document.getElementById('sumNewModel').innerText = fieldDisplayValue('Model') || '(ไม่เปลี่ยน)';
            document.getElementById('sumNewCell').innerText = fieldDisplayValue('Cell') || '(ไม่เปลี่ยน)';
            document.getElementById('sumNewStation').innerText = fieldDisplayValue('Station') || '(ไม่เปลี่ยน)';

            // Due Date
            var curDue = labelText('<%= lblCurDueDate.ClientID %>');
            var newDueEl = document.getElementById('<%= txtDueDate.ClientID %>');
            var dueMode = document.getElementById('<%= hdModeDueDate.ClientID %>');
            var newDue = (dueMode && dueMode.value === 'on' && newDueEl) ? newDueEl.value.trim() : '';
            document.getElementById('sumCurDueDate').innerText = curDue;
            document.getElementById('sumNewDueDate').innerText = newDue || '(ไม่เปลี่ยน)';

            // Remark
            var curRemark = labelText('<%= lblCurRemark.ClientID %>');
            var newRemarkEl = document.getElementById('<%= txtRemark.ClientID %>');
            var newRemark = newRemarkEl ? newRemarkEl.value.trim() : '';
            document.getElementById('sumCurRemark').innerText = curRemark;
            //document.getElementById('sumNewRemark').innerText = newRemark || '(ไม่เปลี่ยน)';
            document.getElementById('sumNewRemark').innerText =
                (newRemark === curRemark || !newRemark)
                    ? '(ไม่เปลี่ยน)'
                    : newRemark;

            var anyManual = (fieldModeVal('Model') === 'txt') || (fieldModeVal('Cell') === 'txt') || (fieldModeVal('Station') === 'txt');
            document.getElementById('sumManualNote').style.display = anyManual ? '' : 'none';

            var dueWarn = getDueDateWarning();
            if (dueWarn) {
                if (!confirm('⚠️ ' + dueWarn + '\n\nต้องการบันทึกต่อหรือไม่ ?')) {
                    return false; // ผู้ใช้ยกเลิก -> ไม่เปิด modal สรุป
                }
            }

            $('#modal_ConfirmSave').modal('show');
            return false; // ห้าม postback จากปุ่มที่มองเห็น ให้ยืนยันใน Modal ก่อน
        }

        function submitConfirmedSave() {
            $('#modal_ConfirmSave').modal('hide');
            showLoading();
            document.getElementById('<%= btnSaveConfirmed.ClientID %>').click();
        }

        function getDueDateWarning() {
            var el = document.getElementById('<%= txtDueDate.ClientID %>');
            var newVal = el ? el.value.trim() : '';
            if (!newVal) return null; // ไม่ได้เลือก -> ไม่เตือน

            // Current Due Date จาก label ฝั่งซ้าย
            var curEl = document.getElementById('<%= lblCurDueDate.ClientID %>');
            var curVal = curEl ? curEl.innerText.trim() : '';
            if (curVal === '-') curVal = '';

            var newD = moment(newVal, 'DD-MMM-YYYY', true);
            if (!newD.isValid()) return null;

            // กรณี: Current ไม่มีค่า แต่เลือก New
            if (!curVal) {
                return 'Due Date เดิมไม่มีค่า แต่คุณกำลังกำหนด Due Date ใหม่ (' + newVal + ')';
            }

            var curD = moment(curVal, 'DD-MMM-YYYY', true);
            if (curD.isValid() && newD.isSameOrBefore(curD)) {
                return 'Due Date ใหม่ (' + newVal + ') น้อยกว่าหรือเท่ากับ Due Date เดิม (' + curVal + ')';
            }
            return null;
        }
    </script>

</asp:Content>