<%@ page title="" language="C#" masterpagefile="~/Tool_Equipment.Master"
    autoeventwireup="true" codefile="Tool_equipment_MasterList.aspx.cs"
    inherits="Tool_Equipment_Control_System.Tool_equipment_MasterList"
    maintainscrollpositiononpostback="true"
    enableeventvalidation="false" %>


<asp:Content ID="Tool_Equipment_Masterlist" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <link href="CSS/bootstrap.css" rel="stylesheet" />
    <link href="CSS/bootstrap-multiselect.css" rel="stylesheet" />
    <link href="CSS/jquery-ui.css" rel="stylesheet" />
    <%--<link href="CSS/GridviewScroll.css" rel="stylesheet" />--%>
    <link href="Content/chosen.min.css" rel="stylesheet" />
    <link href="CSS/select2.min.css" rel="stylesheet" />
    <%--<link href="CSS/font-awesome4.7.0.min.css" rel="stylesheet" />--%>
    <%--<link href="DataTables-1.10.18/dataTables.dataTables.css" rel="stylesheet" />--%>


    <%--<script src="JS/jquery-3.7.1.min.js"></script>--%>
    <script src="JS/jquery-ui.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>
    <script src="Scripts/bootstrap-multiselect.js"></script>
    <%--<script src="JS/gridviewScroll.min.js"></script>--%>
    <script src="JS/chosen.jquery.js"></script>
    <script src="Scripts/select2.min.js"></script>

    <%--<script src="JS/jquery-1.9.1.js"></script>--%>
    <%--<script src="Scripts/select2.min.js"></script>
    <script src="Scripts/jquery.min.js"></script>--%>
    <%--<script src="DataTables-1.10.18/dataTables.min.js"></script>--%>

    
    <style>                 
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }

        :root {
            --sidebar: #121212;
            --bg: #0f1117;
            --surface: #181c27;
            --card: #1e2332;
            --border: #2a3045;
            --accent: #f5a623;
            --accent2: #3b82f6;
            --danger: #ef4444;
            --success: #22c55e;
            --text: #e8ecf4;
            --muted: #8b95b0;
            --font: 'IBM Plex Sans Thai', sans-serif;
            --mono: 'IBM Plex Mono', monospace;
        }
        /* MAIN  */
        .main {
            position: relative;
            z-index: 1;
            max-width: 100%;
            margin: 30px 10px 0px 10px;
            padding: 0px 15px 15px;
            background-color: white;
            border-radius: 5px;
        }

        .grid-headerTopic {
            display: grid;
            grid-template-columns: 70px auto;
            grid-template-rows: 60px;
            background-color: #94c2ff;
            border-radius: 10px;
            /*margin: 10px 10px 10px 10px;*/
        }

        .header_Style1 {
            border-radius: 10px;
            align-content: center;
            font-size: xx-large;
            margin-bottom: 0px;
            font-weight: 600;
            color: #4b4949;
        }

        .grid-Body {
            display: grid;
            grid-template-columns: 99%;
            grid-template-rows: 100%;
            margin-top: 30px;
            margin-left: 10px;
            overflow: hidden;
        }

        .table-mine {
            width: 100% !important;
        }

        .scrollbar {
            /*scrollbar-base-color: #ffffff;
            scrollbar-face-color: #dce5ef;
            scrollbar-3dlight-color: #f2f2f2;
            scrollbar-highlight-color: #ffffff;
            scrollbar-track-color: #f2f2f2;
            scrollbar-arrow-color: #f2f2f2;
            scrollbar-shadow-color: #dce5ef;
            scrollbar-dark-shadow-color: #f2f2f2;*/
        }

        .dataTables_scrollBody, .tableGv {
            margin-left: 0px !important;
        }

            .tableGv th, .tableGv td {
                text-align: center !important;
                height: 35px !important;
            }

        .headerGv1 {
            position:sticky !important;
            top:0 !important;
            z-index : 20 !important;

            color: #ffffff !important;
            font-weight: 800;
            background-color: #167bc2;
            padding: 2px 4px;
            font-size:12px;
        }

        .headerGv2 {
            position:sticky !important;
            top:0 !important;
            z-index : 10 !important;
            color: #ffffff !important;
            font-weight: 800;
            /*border-bottom : 2px solid #413d3d;*/
            background-color:#167bc2;
            /*border-right:1px solid #aaa;*/
            padding: 2px 10px;
            font-size:12px;
        }

        .rowGv1 {
            /*border-bottom : 1px solid #aaa;*/
            font-size: 12px;
            padding: 2px 4px;
            background-color: #ffffff;
        }

        .rowGv2 {
            /*border-bottom : 1px solid #aaa;*/
            font-size: 12px;
            padding: 2px 10px;
            /*border-right : 1px solid #aaa;*/
            background-color: #ffffff;
        }

        .rowGv3 {
            /*border-bottom : 1px solid #aaa;*/
            font-size: 12px;
            padding: 2px 10px;
            /*border-right : 1px solid #aaa;*/
            /*background-color: #ffffff;*/
            text-align-last: start;
        }

        .borderrightGv {
            /*border-right:1px solid #aaa;*/
        }
    </style>

    <%-- เพิ่ม Style ใหม่ สำหรับ GridView --%>
    <style>
        /* ── Wrapper : scroll container ── */
        .gv-wrapper {
            overflow: auto;
            max-height: calc(100vh - 320px);
            border: 1px solid #dee2e6;
            border-radius: 6px;
            margin-bottom: 0;
        }
            /* ── Scrollbar สวยงาม ── */
            .gv-wrapper::-webkit-scrollbar {
                width: 8px;
                height: 8px;
            }

            .gv-wrapper::-webkit-scrollbar-track {
                background: #f1f1f1;
                border-radius: 4px;
            }

            .gv-wrapper::-webkit-scrollbar-thumb {
                background: #b0b8c9;
                border-radius: 4px;
            }

                .gv-wrapper::-webkit-scrollbar-thumb:hover {
                    background: #7b8fa6;
                }
        /* ── Table base ── */
        .tableGv {
            width: 100% !important;
            border-collapse: separate !important;
            border-spacing: 0 !important;
            font-size: 12px;
        }
          .tableGv th.headerGv1 a,
        .tableGv th.headerGv2 a {
            color: #ffffff !important;
            text-decoration: none !important;
        }

        .tableGv th.headerGv1 a:hover,
        .tableGv th.headerGv1 a:visited,
        .tableGv th.headerGv2 a:hover,
        .tableGv th.headerGv2 a:visited {
            color: #ffffff !important;
        }

            .tableGv th, .tableGv td {
                white-space: nowrap;
                padding: 6px 10px !important;
                border-right: 1px solid #dee2e6;
                border-bottom: 1px solid #dee2e6;
                text-align: center;
            }
            /* ── Fix Header (sticky top) ── */
            .tableGv thead tr th {
                position: sticky !important;
                top: 0 !important;
                z-index: 10 !important;
                background-color: #1565c0 !important;
                color: #fff !important;
                font-weight: 700;
                font-size: 12px;
                border-bottom: 2px solid #0d47a1 !important;
                box-shadow: 0 2px 4px rgba(0,0,0,0.15);
            }
            /* ── Freeze Column 1-4 (left) ──
           col1=No(40px), col2=Action(70px), col3=Picture(60px), col4=Status(80px) */
            .tableGv th:nth-child(1), .tableGv td:nth-child(1) {
                position: sticky;
                left: 0;
                min-width: 40px;
                max-width: 40px;
                z-index: 2;
            }

            .tableGv th:nth-child(2), .tableGv td:nth-child(2) {
                position: sticky;
                left: 40px;
                min-width: 70px;
                max-width: 70px;
                z-index: 2;
            }

            .tableGv th:nth-child(3), .tableGv td:nth-child(3) {
                position: sticky;
                left: 110px;
                min-width: 65px;
                max-width: 65px;
                z-index: 2;
            }

            .tableGv th:nth-child(4), .tableGv td:nth-child(4) {
                position: sticky;
                left: 175px;
                min-width: 110px;
                max-width: 110px;
                z-index: 2;
                border-right: 2px solid #9ab5d4 !important;
            }

            /* Header frozen ต้อง z-index สูงกว่า (header + freeze) */
            .tableGv thead tr th:nth-child(1),
            .tableGv thead tr th:nth-child(2),
            .tableGv thead tr th:nth-child(3),
            .tableGv thead tr th:nth-child(4) {
                z-index: 20 !important;
            }
            /* ── สีแถว zebra stripe อ่านง่าย ── */
            .tableGv tbody tr:nth-child(even) td {
                background-color: #f0f5fb;
            }

            .tableGv tbody tr:nth-child(odd) td {
                background-color: #ffffff;
            }

            .tableGv tbody tr:hover td {
                background-color: #d6eaff !important;
                cursor: pointer;
            }
            /* Frozen column ต้อง set bg เองไม่ให้ scroll โปร่งแสง */
            .tableGv tbody tr:nth-child(even) td:nth-child(1),
            .tableGv tbody tr:nth-child(even) td:nth-child(2),
            .tableGv tbody tr:nth-child(even) td:nth-child(3),
            .tableGv tbody tr:nth-child(even) td:nth-child(4) {
                background-color: #f0f5fb;
            }

            .tableGv tbody tr:nth-child(odd) td:nth-child(1),
            .tableGv tbody tr:nth-child(odd) td:nth-child(2),
            .tableGv tbody tr:nth-child(odd) td:nth-child(3),
            .tableGv tbody tr:nth-child(odd) td:nth-child(4) {
                background-color: #ffffff;
            }

            .tableGv tbody tr:hover td:nth-child(1),
            .tableGv tbody tr:hover td:nth-child(2),
            .tableGv tbody tr:hover td:nth-child(3),
            .tableGv tbody tr:hover td:nth-child(4) {
                background-color: #d6eaff !important;
            }
        /* ── Pager ── */
        .pagegvMasterList {
            padding: 8px 12px;
            border-top: 2px solid #dee2e6;
            background: #f8f9fa;
            display: flex;
            align-items: center;
            gap: 4px;
        }

            .pagegvMasterList a, .pagegvMasterList span {
                display: inline-block;
                padding: 3px 10px;
                margin: 0 2px;
                border: 1px solid #c8d6e5;
                border-radius: 5px;
                font-size: 12px;
                font-weight: 600;
                text-decoration: none;
                color: #1565c0;
                background: #fff;
                transition: all 0.15s;
            }

                .pagegvMasterList a:hover {
                    background: #e3eeff;
                    border-color: #1565c0;
                }

            .pagegvMasterList span {
                background: #1565c0;
                color: #fff;
                border-color: #1565c0;
            }
        /* ── PageSize dropdown + Record Count bar ── */
        .gv-toolbar {
            flex: 0 0 auto;
            display: flex;
            align-items: center;
            justify-content: space-between;
            padding: 6px 12px;
            background: #eef3fa;
            border: 1px solid #c8d6e5;
            border-bottom: 2px solid #c8d6e5;
            border-radius: 6px 6px 0 0;
            flex-wrap: wrap;
            gap: 8px;
            min-height: 40px;
        }
            /* Zone ซ้าย: PageSize */
            .gv-toolbar-left {
                display: flex;
                align-items: center;
                gap: 6px;
            }
            .gv-toolbar-left label {
                font-size: 12px;
                color: #555;
                font-weight: 600;
                margin: 0;
                white-space: nowrap;
            }
            .gv-toolbar-left select {
                font-size: 12px;
                padding: 2px 8px;
                border-radius: 4px;
                border: 1px solid #c8d6e5;
                height: 26px;
                color: #2c3e50;
            }

        /* Zone กลาง: Pager buttons */
        .gv-toolbar-pager {
            display: flex;
            align-items: center;
            gap: 2px;
            flex-wrap: wrap;
        }
        .gv-toolbar-pager a,
        .gv-toolbar-pager span {
            display: inline-block;
            padding: 2px 9px;
            border: 1px solid #c8d6e5;
            border-radius: 4px;
            font-size: 12px;
            font-weight: 600;
            text-decoration: none;
            color: #1565c0;
            background: #fff;
            transition: all 0.15s;
            line-height: 1.6;
        }
        .gv-toolbar-pager a:hover { background: #e3eeff; border-color: #1565c0; }
        .gv-toolbar-pager span    { background: #1565c0; color: #fff; border-color: #1565c0; }

        /* Zone ขวา: Record count */
        .gv-toolbar-right {
            font-size: 12px;
            font-weight: 700;
            color: #1565c0;
            white-space: nowrap;
        }
        .gv-toolbar-right b {
            color: #e53935;
            font-size: 14px;
        }

            .gv-toolbar label {
                font-size: 12px;
                color: #555;
                font-weight: 600;
                margin: 0;
            }

            .gv-toolbar select {
                font-size: 12px;
                padding: 2px 6px;
                border-radius: 4px;
                border: 1px solid #c8d6e5;
                height: 26px;
            }

        .gv-record-count {
            font-size: 12px;
            font-weight: 700;
            color: #1565c0;
        }

            .gv-record-count span {
                color: #e53935;
            }

        /* ── Layout หลัก ── */
        .gv-container {
            display: flex;
            flex-direction: column;
            /* ความสูงรวมจาก toolbar ลงไปถึงล่างหน้าจอ */
            /*height: calc(100vh - 345px);*/ /* ปรับ 310px ตามความสูง header+filter ของคุณ */
            min-height: 300px;
            border: 1px solid #dee2e6;
            border-radius: 6px;
            overflow: visible;
        }

        /* ── Scroll Area (header + body เท่านั้น) ── */
        .gv-scroll-area {
            flex: 1 1 auto; /* กินพื้นที่ที่เหลือทั้งหมด */
            overflow: auto;
            /*overflow-x: auto;*/
            /*overflow-y: auto;*/
            position:relative;
        }

        /* ลบ / ซ่อน pager-bar ด้านล่าง */
        .gv-pager-bar { display: none !important; }

        /* ซ่อน pager ใน table เหมือนเดิม */
        .tableGv tr.pagegvMasterList { display: none !important; }

        /* ── Pager ติดล่าง ── */
        /*.gv-pager-bar {
            flex: 0 0 auto;*/ /* ความสูงคงที่ ไม่ยืดหด */
            /*background: #f8f9fa;
            border-top: 2px solid #dee2e6;
            padding: 6px 12px;
            display: flex;
            align-items: center;
            gap: 4px;
        }*/
    </style>

    <%--Filter Search Grdiview--%>
    <style>
        .gv-toolbar-filter {
            display: flex;
            align-items: center;
            gap: 6px;
        }
        .gv-toolbar-filter label {
            font-size: 12px;
            color: #555;
            font-weight: 600;
            margin: 0;
            white-space: nowrap;
        }
        .gv-toolbar-filter input {
            font-size: 12px;
            padding: 2px 8px;
            border-radius: 4px;
            border: 1px solid #c8d6e5;
            height: 26px;
            color: #2c3e50;
            width: 180px;
        }
        .gv-toolbar-filter input:focus {
            outline: none;
            border-color: #1565c0;
            box-shadow: 0 0 0 2px rgba(21,101,192,0.15);
        }
        .filter-count {
            font-size: 11px;
            color: #1565c0;
            font-weight: 600;
            white-space: nowrap;
        }
    </style>

    <%-- Style Filter  --%>
    <style>
        .filterRow {
            display: grid;
            grid-template-columns: 265px 1fr;
            gap: 15px;
            margin-bottom: 8px;
        }

    </style>

    <%-- Style Status  --%>
    <style>
        .status-dot {
            display: inline-flex; /* ใช้ flexbox เพื่อจัดวางเนื้อหาตรงกลาง */
            justify-content: center;
            align-items: center;
            width: auto; /* กำหนดความกว้างอัตโนมัติให้พอดีกับข้อความ */
            min-width: 60px; /* กำหนดความกว้างขั้นต่ำ */
            height: 24px; /* ความสูงของวงกลม/วงรี */
            padding: 2px 8px; /* เพิ่ม padding เพื่อให้มีพื้นที่รอบข้อความ */
            border-radius: 12px; /* ทำให้เป็นวงรี (หรือ 50% สำหรับวงกลมสมบูรณ์ถ้า width = height) */
            color: white; /* สีข้อความภายในวงกลม */
            font-weight: bold;
            font-size: 0.85em;
            text-align: center;
            white-space: nowrap; /* ไม่ให้ข้อความขึ้นบรรทัดใหม่ */
        }

        /* สไตล์สีสำหรับแต่ละสถานะ */
        .status-use {
            background-color: #28A745; /* เขียว */
        }

        .status-spare {
            background-color: #17A2B8; /* ฟ้า/น้ำเงินอ่อน */
        }

        .status-repair {
            background-color: #FFC107; /* ส้ม/เหลืองอมส้ม */
        }

        .status-ng {
            background-color: #DC3545; /* แดงเข้ม */
        }

        .status-eol {
            background-color: #343A40; /* เทาเข้ม/ดำ */
        }

        .status-donotuse {
            background-color: #6F42C1; /* ม่วงเข้ม */
        }

        .status-waitconsider {
            background-color: #6C757D; /* เทาอ่อน */
        }

        .status-scrap {
            background-color: #795548; /* น้ำตาลเข้ม */
        }

        .status-transfer {
            background-color: #0D9488; /* เขียวอมฟ้า */
        }

        .status-dispose {
            background-color: #607D8B; /* เทาอมน้ำเงิน */
        }

        /* สไตล์เริ่มต้นหากไม่มีสถานะที่ตรงกัน */
        .status-default {
            background-color: #cccccc; /* สีเทาอ่อนๆ */
            color: #333;
        }

        /* หากต้องการให้เซลล์นั้นจัดกึ่งกลาง */
        .gv_Cell_Status {
            text-align: center;
            vertical-align: middle;
        }
    </style>

    <%--FORM ELEMENTS--%>
    <style>
        .card {
            background: #fff;
            border-radius: 8px;
            box-shadow: 0 1px 6px rgba(0,0,0,0.08);
            margin-bottom: 16px;
            overflow: hidden;
            padding: 10px 10px 5px 10px;
        }

        .form-row {
            display: flex;
            flex-wrap: wrap;
            row-gap: 5px;
            column-gap: 12px;
            align-items: flex-end;
        }

        .form-group {
            display: flex;
            flex-direction: column;
            margin-bottom: 5px;
            gap: 4px;
        }

            .form-group label {
                font-size: 12px;
                font-weight: 600;
                color: #555;
                text-transform: uppercase;
                letter-spacing: 0.4px;
                margin-left: 5px;
                margin-bottom: -5px;
            }

            .form-group input[type="text"],
            .form-group select {
                border: 1.5px solid #c8d6e5;
                border-radius: 5px;
                padding: 4px 8px;
                font-size: 14px;
                color: #2c3e50;
                background: #fff;
                transition: border-color 0.2s;
                height: 30px;
                text-align: center;
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

        .ddlGroup option:first-child {
            color: #999;
        }

        .btn {
            border: none;
            border-radius: 8px;
            padding: 2px 12px;
            font-size: 12px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.18s;
            /*display: inline-flex;*/
            /*align-items: center;*/
            /*gap: 5px;*/
            /*height: 34px !important;*/
            width: 60px !important;
        }

        .btn-primary {
            background: #1976ff;
            color: #fff;
        }

            .btn-primary:hover {
                background: #1a6a9a;
            }

        .btn-success {
            background: #27ae60;
            color: #fff;
        }

            .btn-success:hover {
                background: #1e8449;
            }

        .btn-warning {
            background: #f39c12;
            color: #fff;
        }

            .btn-warning:hover {
                background: #d68910;
            }

        .btn-danger {
            background: #e74c3c;
            color: #fff;
        }

            .btn-danger:hover {
                background: #c0392b;
            }

        .btn-secondary {
            background: #7f8c8d;
            color: #fff;
        }

            .btn-secondary:hover {
                background: #626567;
            }

        .btn-sm {
            padding: 4px 10px;
            height: 25px !important;
            font-size: 11px;
        }

        .btn:disabled {
            opacity: 0.5;
            cursor: not-allowed;
        }

        .btn-export {
            width: 35px !important;
            align-content: center;
            margin: 5px;
        }

        .txtSearch {
            text-align: center;
            background: #00a7ff40;
        }
    </style>

    <%-- modal--%>
    <style>
        .modal.fade {
            background: fixed;
        }

        h4 {
            color: aliceblue;
        }
    </style>

    <%-- Gridview --%>
    <script type="text/javascript">

        // -- ตรวจสอบความสูงของ Filter Area
        window.onload = function () {
            var filter = document.querySelector(".filterRow");
            if (filter) {
                var height = filter.offsetHeight;
                console.log("Filter height = " + height + "px");

                // เอาไปใช้ปรับ GridView
                var grid = document.getElementById("<%= gv_masterList.ClientID %>");
                if (grid) {
                    grid.style.height = (window.innerHeight - height - 100) + "px";
                }
            }
        };

        // ── ย้าย Pager ออกจาก table มาใส่ pager-bar ──
        function movePager() {
            var $gv = $('#<%=gv_masterList.ClientID%>');
            var $pagerBar = $('#gv-pager-bar');

            var $pagerTr = $gv.find('tr').filter(function () {
                return $(this).find('td[colspan]').length > 0 &&
                    $(this).find('a, span').length > 0;
            }).last();

            $pagerBar.empty();

            if ($pagerTr.length > 0) {
                var $links = $pagerTr.find('a, span').clone(true, true);
                $pagerBar.append($links);
                $pagerTr.addClass('pagegvMasterList');
            } else {
                $pagerBar.append('<span style="color:#999;">หน้า 1 / 1</span>');
            }
        }

        // ── ปรับความสูง gv-container ตามหน้าจอ ──
        function adjustGridHeight() {
            <%--var $container = $('.gv-container');
            var $toolbar = $('.gv-toolbar');
            var height = $(".filterRow").outerHeight();
            //console.log("Filter height = " + height)

            // หา offset top ของ toolbar (จาก top ของหน้า)
            var toolbarOffset = $toolbar.offset() ? $toolbar.offset().top : 345;
            var windowH = $(window).height();
            var pagerH = 42;  // ความสูง pager-bar
            var bottomPad = 0;  // padding ด้านล่าง

            var newH = windowH - toolbarOffset - $toolbar.outerHeight() - pagerH - bottomPad;
            if (newH < 200) newH = 200; // min height

            $("#<%= gv_masterList.ClientID %>").height($(window).height() - height - 100);
            $('.gv-scroll-area').css('max-height', newH + 'px');--%>


            var filterHeight = $(".filterRow").outerHeight() || 0;
            //var $toolbar = $('.gv-toolbar');
            var toolbarHeight = $(".gv-toolbar").outerHeight() || 0;
            //var toolbarOffset = $toolbar.offset() ? $toolbar.offset().top : 100;

            var windowH = $(window).height();
            var pagerH = 42;     // ความสูง pager
            var padding = 20;    // กันขอบล่าง
            var top = 120;   //Header Top + Top Master Page

            // ✅ ความสูงที่เหลือจริง
            var newHeight = windowH - filterHeight - toolbarHeight - pagerH - padding -top;

            if (newHeight < 200) newHeight = 200;

            // ✅ กำหนด scroll area
            $(".gv-scroll-area").css({
                "height": newHeight + "px",
                "overflow-y": "auto"
            });

        }

        // ── Init ──
        $(window).load(function () {
            movePager();
            adjustGridHeight();
        });

        // ── Resize ──
        $(window).resize(function () {
            adjustGridHeight();
        });

        // ── หลัง UpdatePanel postback ──
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(function () {
            movePager();
            adjustGridHeight();
        });

        // ── กรองแถวใน GridView (client-side, ไม่ postback) ──
        function filterGridRows() {
            var input = document.getElementById("txtGridFilter");
            if (!input) return;
            var keyword = input.value.toLowerCase();
            var grid = document.getElementById("<%= gv_masterList.ClientID %>");
            if (!grid) return;

            var rows = grid.querySelectorAll("tbody tr");
            var visible = 0;

            rows.forEach(function (row) {
                // ข้ามแถว header และ pager
                if (row.querySelector("th") || row.classList.contains("pagegvMasterList")) return;

                var text = row.innerText.toLowerCase();
                if (text.indexOf(keyword) > -1) {
                    row.style.display = "";
                    visible++;
                } else {
                    row.style.display = "none";
                }
            });

            var count = document.getElementById("filterCount");
            if (count) {
                count.innerText = keyword ? ("พบ " + visible + " แถว") : "";
            }
        }
    </script>

    <%--CSS Image Action on Gridview--%>
    <style>
        .img_borrow {
            display: inline-block; /* ทำให้เป็น block-level element เพื่อกำหนดขนาดได้ */
            width: 25px; /* กำหนดความกว้างของรูปภาพ */
            /*border-radius:50%;*/
            /*border:none;*/
            /*height: 20px;*/ /* กำหนดความสูงของรูปภาพ */
            /*border:1px solid;*/
            /*border-radius:3px;*/
        }
    </style>

    <%--Dark mode Active--%>
    <style>
        body.dark .grid-headerTopic {
            background: #3c091f;
        }

        body.dark .card {
            background: var(--sidebar);
        }

        body.dark .header_Style1,
        body.dark .form-group label {
            color: #ffffff;
        }

        body.dark .txtSearch {
            background: #ffffff !important;
        }
    </style>

    <%--ddlReasponsible text-aling : left--%>
    <script>
        $(function () {

            $('#<%= ddlResponsible.ClientID %>').on('focus', function () {
            $(this).css('text-align', 'left');
        });

        $('#<%= ddlResponsible.ClientID %>').on('blur', function () {
            $(this).css('text-align', 'center');
        });

    });
    </script>

    <%--ddl text search--%>
    <script type="text/javascript">
        function initFilterChosen() {
            var $sels = $('#<%= ddl_status.ClientID %>, #<%= ddl_dept.ClientID %>, #<%= ddlResponsible.ClientID %>, #<%= ddl_category.ClientID %>');

            $sels.each(function () {
                var $s = $(this);
                if (!$s.data('chosen')) {
                    $s.chosen({
                        search_contains: true,        // ค้นหาแบบ contains ไม่ใช่แค่ขึ้นต้น
                        /*width: '100%',*/
                        allow_single_deselect: true   // มีกากบาทล้างค่า
                    });
                }
            });

            // สำคัญ: chosen ไม่ trigger postback เอง ต้อง force change
            $sels.off('change.filter').on('change.filter', function () {
                __doPostBack($(this).attr('name'), '');
            });
        }

        $(function () { initFilterChosen(); });

        // re-init หลัง UpdatePanel postback (chosen หายทุกครั้งที่ refresh partial)
        if (typeof Sys !== 'undefined' && Sys.WebForms) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                initFilterChosen();
            });
        }

        $('#<%= ddl_status.ClientID %>, #<%= ddl_dept.ClientID %>, #<%= ddlResponsible.ClientID %>, #<%= ddl_category.ClientID %>')
            .trigger('chosen:updated');
    </script>

    <script type="text/javascript">
        function initClearBtn() {
            var txt = document.getElementById('<%= txtSearchlist.ClientID %>');
            var btn = document.getElementById('btnClear');
            if (!txt || !btn) return;

            // toggle ตามค่าปัจจุบัน (เผื่อมีข้อความค้างหลัง postback)
            btn.style.display = txt.value.length > 0 ? 'block' : 'none';

            // ใช้ oninput ครอบคลุมทั้งพิมพ์/วาง/ลบ
            txt.oninput = function () {
                btn.style.display = txt.value.length > 0 ? 'block' : 'none';
            };
        }

        function clearSearch() {
            var txt = document.getElementById('<%= txtSearchlist.ClientID %>');
            var btn = document.getElementById('btnClear');
            if (txt) { txt.value = ''; txt.focus(); }
            if (btn) btn.style.display = 'none';
        }

        // โหลดแรก
        if (document.readyState !== 'loading') initClearBtn();
        else document.addEventListener('DOMContentLoaded', initClearBtn);

        // ผูกใหม่ทุก async postback (UpdatePanel re-render textbox)
        if (typeof (Sys) !== "undefined") {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initClearBtn);
        }
    </script>

    <%--ddl text search--%>
    <style>
        /* ── Chosen filter (4 dropdown) ── */
        .card .chosen-container {
            min-width: 150px;
            margin: 5px 0px;
        }

        .card .chosen-container-single .chosen-single {
            height: 30px !important;
            line-height: 28px !important;
            border: 1.5px solid #c8d6e5 !important;
            border-radius: 5px !important;
            background: #fff !important;
            color: #2c3e50 !important;
            font-size: 13px !important;
            padding-left: 8px !important;
            box-shadow: none !important;
            text-align: center;
        }

        .card .chosen-container-single .chosen-single span {
            line-height: 28px !important;
            text-align: center;
            margin-right: 20px;   /* เว้นที่ให้ลูกศร/กากบาท */
        }

        .card .chosen-container-single .chosen-single div b {
            top: 4px;
            position: relative;
        }

        /* focus/open */
        .card .chosen-container-active .chosen-single {
            border-color: #1565c0 !important;
            box-shadow: 0 0 0 3px rgba(21,101,192,0.12) !important;
        }

        /* ── รายการใน dropdown — กึ่งกลาง ── */
        .card .chosen-results li {
            text-align: center;
            font-size: 13px;
            padding: 6px 10px;
        }

        .card .chosen-results li.highlighted {
            background: #1976ff !important;
            color: #fff !important;
        }

        .card .chosen-results li.result-selected {
            background: #e3eeff !important;
            color: #1565c0 !important;
        }

        /* ── ช่องค้นหาใน dropdown ── */
        .card .chosen-container-single .chosen-search input {
            border: 1.5px solid #c8d6e5 !important;
            border-radius: 5px !important;
            text-align: center;
            padding: 4px 8px !important;
        }

        /* กากบาทล้างค่า */
        .card .chosen-container-single .chosen-single abbr {
            top: 9px;
        }
    </style>

    <%--ค้นหารายการ pop-up--%>
    <style>
        .help-icon {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            width: 16px;
            height: 16px;
            margin-left: 4px;
            border-radius: 50%;
            background: #1976ff;
            color: #fff;
            font-size: 11px;
            font-weight: 700;
            cursor: help;
            position: relative;
            vertical-align: middle;
            text-transform: none;
        }

        .help-tooltip {
            display: none;
            position: absolute;
            top: 130%;
            left: 50%;
            transform: translateX(-50%);
            z-index: 999;
            background: #2c3e50;
            color: #fff;
            text-align: left;
            font-size: 12px;
            font-weight: 500;
            line-height: 1.6;
            padding: 8px 12px;
            border-radius: 6px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.2);
            white-space: nowrap;
            letter-spacing: normal;
        }

        /* ลูกศรชี้ขึ้น */
        .help-tooltip::before {
            content: "";
            position: absolute;
            bottom: 100%;
            left: 50%;
            transform: translateX(-50%);
            border: 6px solid transparent;
            border-bottom-color: #2c3e50;
        }

        .help-tooltip b {
            color: #f5a623;
        }

        /* แสดงตอน hover หรือ focus (รองรับมือถือ tap) */
        .help-icon:hover .help-tooltip,
        .help-icon:focus .help-tooltip {
            display: block;
        }

        .search-box {
            position: relative;
            display: inline-block;
        }
        .txtSearch {
            width: 200px;
            height: 30px;
            padding-right: 28px;   /* เผื่อพื้นที่ X ชิดขวา */
            padding-left: 10px;
            text-align: left;      /* เดิม center → ข้อความชน X ; ซ้ายอ่านง่ายกว่า */
            background: #00a7ff40;
            box-sizing: border-box;
        }
        .clear-btn {
            position: absolute;
            right: 15px;
            top: 50%;
            transform: translateY(-50%);
            cursor: pointer;
            color: #888;
            font-size: 14px;
            display: none;
            line-height: 1;
            user-select: none;
            margin-right: 10px;
            font-weight: 600;
        }
        .clear-btn:hover {
            color: red;
        }
    </style>

    <%--READ ME--%>
    <style>
        .readme-btn {
            font-size: 24px;
            color: #4b4949;
            cursor: pointer;
            transition: color 0.15s, transform 0.15s;
        }
        .readme-btn:hover { color: #1976ff; transform: scale(1.1); }

        #modal_ReadMe .modal-dialog { width: auto; max-width: 640px; margin: 40px auto; }
        #modal_ReadMe .modal-content { border-radius: 10px; overflow: hidden; }
        #modal_ReadMe .modal-header {
            background: #1565c0; border-bottom: none; padding: 12px 18px;
        }
        #modal_ReadMe .modal-header .close {
            color: #fff; opacity: 0.9; text-shadow: none; font-size: 26px;
        }
        #modal_ReadMe .modal-title { color: #fff !important; font-size: 17px; font-weight: 700; }
        #modal_ReadMe .modal-body { padding: 18px 22px; font-size: 13px; color: #2c3e50; background:#fff; }
        #modal_ReadMe .rm-item {
            display: flex; align-items: flex-start; gap: 10px; padding: 8px 0;
            border-bottom: 1px solid #eef0f4;
        }
        #modal_ReadMe .rm-item:last-child { border-bottom: none; }
        #modal_ReadMe .rm-ico {
            width: 30px; height: 30px; flex-shrink: 0;
            display: flex; align-items: center; justify-content: center;
            background: #e3eeff; border-radius: 6px; color: #1565c0; font-size: 14px;
        }
        #modal_ReadMe .rm-txt strong { color: #1565c0; display: block; margin-bottom: 2px; }
        #modal_ReadMe .rm-txt span { color: #6b7a99; }
    </style>

    <asp:ScriptManager ID="ScriptManager1" EnablePageMethods="true" runat="server"></asp:ScriptManager>

    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
        <progresstemplate>

            <div class="overlay">
                <div class="overlayContent">
                    <div style="border-width: 0px; position: fixed; font-size: 36px; margin: auto; text-align: center">
                        Loading...
                                <br />
                        <i class="fa fa-spinner fa-spin"></i>
                    </div>
                </div>
            </div>

        </progresstemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <contenttemplate>

            <div id="masterlist" runat="server">
                <!-- ── Main ───────────────────────────────────────── -->
                <%--<main class="main" style="max-height: calc(100vh - 80px);">--%>

                <div class="grid-Body" style="min-height: calc(100vh - 80px);">
                    <asp:Panel ID="Panel1" runat="server" DefaultButton="btn_search">

                        <div class="container" style="width: 100%; padding-left: 0px; padding-right: 0px;">

                            <%-- Topic : Jig Tool Location Contorl List --%>
                            <div class="grid-headerTopic" style="margin-bottom: 10px; grid-template-columns: 70px auto 60px;">
                                <div style="text-align: center; align-content: center;">
                                    <img class="" style="height: 45px;" src="images/machine-data.png" />
                                </div>

                                <label class="header_Style1" style="">
                                    JIG TOOL & EQUIPMENT MASTER LIST
                                </label>

                                <%-- ปุ่ม Read me --%>
                                <div style="display:flex; align-items:center; justify-content:center;">
                                    <span class="glyphicon glyphicon-question-sign readme-btn"
                                        title="วิธีใช้งาน" onclick="$('#modal_ReadMe').modal('show');"></span>
                                </div>
                            </div>

                            <%-- Filter Function --%>
                            <div class="card">

                                <%--Filter--%>
                                <div class="filterRow">

                                    <%--Search Data--%>
                                    <div class="form-row" style="align-content: center; justify-content: center;">

                                        <%--Search Text--%>
                                        <div class="form-group">
                                            <%--<label class="">🔍 ค้นหารายการ </label>--%>
                                            <label class="">
                                                🔍 ค้นหารายการ
                                                <span class="help-icon" tabindex="0">?
                                                    <span class="help-tooltip">
                                                        <b>ข้อมูลที่สามารถค้นหาได้</b><br />
                                                        • ID<br />
                                                        • Tool No<br />
                                                        • Fix Asset<br />
                                                        • Asset Name<br />
                                                        • Control No<br />
                                                        • Serial No
                                                    </span>
                                                </span>
                                            </label>

                                            <div class="search-box">
                                                <asp:TextBox ID="txtSearchlist" runat="server"
                                                    CssClass="txtSearch"
                                                    placeholder="ค้นหารายการ" />
 
                                                <span id="btnClear" class="clear-btn"
                                                    onclick="clearSearch()">✕</span>
                                            </div>
                                        </div>

                                        <%--Button Search--%>                                        
                                        <asp:Button ID="btnSearch" runat="server" Text="🔍"
                                            CssClass="btn btn-primary" OnClick="btn_search_Click"
                                            style="width: 40px !important; border-radius:10px; margin-bottom:10px;"/>                                        

                                        <%--<button Search Hidden>--%>
                                        <asp:LinkButton runat="server" ID="btn_search" Visible="false"
                                            CssClass="btn btn-info" OnClick="btn_search_Click" Width="80px" ToolTip="ค้นหา">
                                            <i class="glyphicon glyphicon-search">Search</i>
                                        </asp:LinkButton>
                                     
                                    </div>

                                    <%--Filter Data--%>
                                    <div class="form-row" style="align-content:center;">

                                        <%-- Status Filter--%>
                                        <div class="form-group">
                                            <label class="">🔄 Status </label>
                                            <asp:DropDownList runat="server" ID="ddl_status"
                                                CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="text_search_TextChanged"
                                                AppendDataBoundItems="true" Width="160px">
                                            </asp:DropDownList>
                                        </div>

                                        <%-- Dept --%>
                                        <div class="form-group">
                                            <label>👥 Dept</label>
                                            <asp:DropDownList ID="ddl_dept" runat="server"
                                                CssClass="form-control" AutoPostBack="true"
                                                OnSelectedIndexChanged="text_search_TextChanged"
                                                AppendDataBoundItems="true" Width="160px">
                                            </asp:DropDownList>
                                        </div>

                                        <%--Responsible--%>
                                        <div class="form-group">
                                            <label>👤 Responsible </label>
                                            <asp:DropDownList ID="ddlResponsible" runat="server"
                                                CssClass="form-control" AutoPostBack="true"
                                                OnSelectedIndexChanged="text_search_TextChanged"
                                                AppendDataBoundItems="true" style="width:200px;">
                                            </asp:DropDownList>
                                        </div>

                                        <%--Category--%>
                                        <div class="form-group">
                                            <label>🏷️ Category</label>
                                            <asp:DropDownList ID="ddl_category" runat="server"
                                                CssClass="form-control" AutoPostBack="true"
                                                OnSelectedIndexChanged="text_search_TextChanged"
                                                AppendDataBoundItems="true" Width="160px">
                                            </asp:DropDownList>
                                        </div>

                                        <%--CPT MODEL--%>
                                        <div class="form-group">
                                            <label>📦 CPT MODEL </label>
                                            <asp:TextBox runat="server" ID="txt_cptmodel" placeholder="ค้นหา Model"
                                                CssClass="form-control" Width="150px" AutoPostBack="true"
                                                OnTextChanged="text_search_TextChanged">
                                            </asp:TextBox>
                                        </div>

                                        <%--Location--%>
                                        <%--<div class="form-group">
                                            <label>📍 Location </label>
                                            <asp:DropDownList runat="server" ID="ddl_location"
                                                CssClass="form-control"
                                                DataSourceID="Sql_ddl_location" DataValueField="location1_id" DataTextField="location1_name"
                                                AutoPostBack="true" OnSelectedIndexChanged="text_search_TextChanged"
                                                AppendDataBoundItems="true" Width="120px">
                                                <asp:ListItem Value="">-- ทั้งหมด --</asp:ListItem>
                                            </asp:DropDownList>
                                            <asp:SqlDataSource ID="Sql_ddl_location" runat="server"
                                                ConnectionString="<%$ ConnectionStrings:jig_tool %>"
                                                SelectCommand="SELECT location1_id, location1_name FROM master_location1 WHERE (location1_id IN (97, 99))"></asp:SqlDataSource>
                                        </div>--%>

                                        <%--Cell--%>
                                        <div class="form-group">
                                            <label>📌 Cell </label>
                                            <asp:TextBox runat="server" ID="txt_cell" placeholder="ค้นหา Cell"
                                                CssClass="form-control" Width="140px" AutoPostBack="true"
                                                OnTextChanged="text_search_TextChanged">
                                            </asp:TextBox>
                                        </div>

                                        <%--Cell--%>
                                        <div class="form-group">
                                            <label>📌 Station </label>
                                            <asp:TextBox runat="server" ID="txt_station" placeholder="ค้นหา Station"
                                                CssClass="form-control" Width="140px" AutoPostBack="true">
                                            </asp:TextBox>
                                            <%--OnTextChanged="text_search_TextChanged"--%>
                                        </div>                                        

                                        <%--Fix / Non Fix Filter--%>
                                        <div class="form-group">
                                            <label class="">📋 Fix/Non Fix </label>
                                            <asp:DropDownList runat="server" ID="ddl_nonFix"
                                                CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="text_search_TextChanged"
                                                AppendDataBoundItems="true" Width="120px">
                                                <asp:ListItem Value="">-- ทั้งหมด --</asp:ListItem>
                                                <asp:ListItem Value="0">Non Fix</asp:ListItem>
                                                <asp:ListItem Value="1">Fix</asp:ListItem>
                                            </asp:DropDownList>
                                        </div>

                                        <%--Button Clear Filter--%>
                                        <div class="form-group">
                                            <label class="" style="margin-bottom: -40px;"></label>
                                            <asp:LinkButton runat="server" ID="btn_clear"
                                                CssClass="btn btn-danger btn-export"
                                                OnClick="btn_clear_Click" ToolTip="Clear Filter">
                                                <i class="glyphicon glyphicon-erase"></i>
                                            </asp:LinkButton>
                                        </div>
                                    
                                        <%--Button Export Excel--%>
                                        <div class="form-group">
                                            <label class="" style="margin-bottom: -40px;"></label>
                                            <asp:LinkButton runat="server" ID="btn_excel"
                                                CssClass="btn btn-secondary btn-export" style="width: 80px !important; text-shadow: none; background: #6489c4 !important;"
                                                OnClick="btn_excel_Click" ToolTip="Export Data">
                                                <i class="glyphicon glyphicon-save"></i>
                                                Export
                                            </asp:LinkButton>
                                        </div>                                   

                                    </div>

                                </div>

                                <%--Gridview--%>
                                <div style="">

                                    <asp:HiddenField ID="gv_masterListSV" runat="server" />
                                    <asp:HiddenField ID="gv_masterListSH" runat="server" />

                                    <%-- Toolbar --%>
                                    <div class="gv-toolbar">
                                        <%-- Zone ซ้าย: PageSize --%>
                                        <div class="gv-toolbar-left" style="width:140px;">
                                            <label>แสดง</label>
                                            <asp:DropDownList ID="ddl_pageSize" runat="server"
                                                AutoPostBack="true"
                                                OnSelectedIndexChanged="ddl_pageSize_SelectedIndexChanged">
                                                <asp:ListItem Value="10">10</asp:ListItem>
                                                <asp:ListItem Value="20" Selected="True">20</asp:ListItem>
                                                <asp:ListItem Value="50">50</asp:ListItem>
                                            </asp:DropDownList>
                                            <label>รายการ / หน้า</label>
                                        </div>

                                        <%-- Zone กลาง: Pager (JS จะ inject ที่นี่) --%>
                                        <div class="gv-toolbar-pager" id="gv-pager-bar">
                                        </div>

                                        <%-- Zone ค้นหา client-side (กรองเฉพาะแถวที่แสดง) --%>
                                        <%--<div class="gv-toolbar-filter">
                                            <label>🔎 กรอง</label>
                                            <input type="text" id="txtGridFilter"
                                                placeholder="กรองในตาราง..."
                                                onkeyup="filterGridRows()" />
                                            <span id="filterCount" class="filter-count"></span>
                                        </div>--%>

                                        <%-- Zone ขวา: Record count --%>
                                        <div class="gv-toolbar-right">
                                            <asp:Label ID="lblRecordCount" runat="server" Text=""></asp:Label>
                                        </div>
                                    </div>

                                    <%-- GridView Container --%>
                                    <div class="gv-container">

                                        <%-- Scroll area (data+header เท่านั้น) --%>
                                        <div class="gv-scroll-area" id="gvScrollArea">
                                            <asp:GridView ID="gv_masterList" runat="server"
                                                AutoGenerateColumns="false"
                                                AllowPaging="True"
                                                AllowSorting="true"
                                                OnSorting="gv_masterList_Sorting"
                                                PageSize="20"
                                                PagerSettings-Mode="NumericFirstLast"
                                                PagerSettings-FirstPageText="&laquo; First"
                                                PagerSettings-LastPageText="Last &raquo;"
                                                PagerSettings-PreviousPageText=" &lsaquo; "
                                                PagerSettings-NextPageText=" &rsaquo; "
                                                PagerSettings-Position="Bottom"
                                                PagerSettings-Visible="true"
                                                PagerSettings-PageButtonCount="5"
                                                OnPageIndexChanging="GridView1_PageIndexChanging"
                                                PagerStyle-CssClass="pagegvMasterList"
                                                EmptyDataText="ไม่พบข้อมูล"
                                                CssClass="tableGv"
                                                EmptyDataRowStyle-HorizontalAlign="Center"
                                                GridLines="None"
                                                OnRowDataBound="gv_RowDataBound">
                                                <emptydatarowstyle cssclass="danger text-danger" />
                                                <headerstyle wrap="false" horizontalalign="Center" font-size="Small" />
                                                <rowstyle horizontalalign="Center" font-size="Small" />

                                                <columns>
                                                    <asp:TemplateField HeaderText="No" HeaderStyle-CssClass="headerGv1"
                                                        ItemStyle-CssClass="rowGv1" ItemStyle-HorizontalAlign="Center">
                                                        <itemtemplate>
                                                            <%# Container.DataItemIndex + 1 %>
                                                        </itemtemplate>
                                                    </asp:TemplateField>

                                                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="headerGv1"
                                                        ItemStyle-CssClass="rowGv1" ItemStyle-HorizontalAlign="Center" ItemStyle-Wrap="false">
                                                        <itemtemplate>
                                                            <asp:HiddenField runat="server" ID="hd_sys_status" Value='<%#Eval("sys_status") %>' />
                                                            <asp:HiddenField runat="server" ID="hdtool_id" Value='<%#Eval("tool_id") %>' />
                                                            <asp:HiddenField runat="server" ID="hdtool_sub" Value='<%#Eval("sub_no") %>' />
                                                            <asp:LinkButton ID="btn_detailView" data-toggle="tooltip" title="View Detail" runat="server" OnClick="btn_detailView_Click" CssClass="img_borrow">
                                                                <img src="images/eye-tracking.png" /></asp:LinkButton>
                                                        </itemtemplate>
                                                    </asp:TemplateField>

                                                    <%-- status New version--%>
                                                    <asp:TemplateField HeaderText="Picture" HeaderStyle-CssClass="headerGv1 borderrightGv"
                                                        ItemStyle-CssClass="rowGv1 borderrightGv" ItemStyle-HorizontalAlign="Center">
                                                        <itemtemplate>
                                                            <%--<asp:LinkButton ID="btn_pic" title="View Picture" data-toggle="tooltip" runat="server" OnClick="btn_pic_Click"><i class="glyphicon glyphicon-picture"></i></asp:LinkButton>--%>
                                                            <asp:LinkButton ID="btn_pic" title="View Picture" data-toggle="tooltip" runat="server" OnClick="btn_pic_Click" CssClass="img_borrow">
                                                                <img src="images/photo-gallery.png" />
                                                            </asp:LinkButton>
                                                        </itemtemplate>
                                                    </asp:TemplateField>

                                                    <asp:BoundField HeaderText="Status" DataField="status" SortExpression="status"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv1" />

                                                    <asp:BoundField HeaderText="ID" DataField="ID" SortExpression="ID"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="No." DataField="sub_no" SortExpression="sub_no"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2"
                                                        DataFormatString="No.{0}" />

                                                    <asp:BoundField HeaderText="Dept" DataField="dept_code" SortExpression="dept_code"
                                                        ItemStyle-Wrap="false" ItemStyle-Width="" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="Fix Asset" DataField="FixAcces" SortExpression="FixAcces"
                                                        ItemStyle-Wrap="false" ItemStyle-Width="" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="Control No." DataField="control_no" SortExpression="control_no"
                                                        ItemStyle-Wrap="false" ItemStyle-Width="" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <%--<asp:BoundField HeaderText="Name" DataField="asset_name" SortExpression="asset_name"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv3" HeaderStyle-CssClass="headerGv2" />--%>

                                                    <asp:TemplateField HeaderText="Name" SortExpression="asset_name" HeaderStyle-CssClass="headerGv2">
                                                        <ItemTemplate>
                                                            <asp:Label ID="lblName" runat="server" CssClass="rowGv3"
                                                                Text='<%# String.IsNullOrEmpty(Convert.ToString(Eval("asset_name")))
                                                                    ? Convert.ToString(Eval("toolName"))
                                                                    : Convert.ToString(Eval("asset_name")) %>'>
                                                            </asp:Label>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>

                                                    <asp:BoundField HeaderText="Tool No / Model" DataField="tool_no" SortExpression="tool_no"
                                                        ItemStyle-Wrap="false" ItemStyle-Width="" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="Serial No." DataField="serial_no" SortExpression="serial_no"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="Manufacturing" DataField="manufacturing" SortExpression="manufacturing"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="Price" DataField="price" SortExpression="price"
                                                        DataFormatString="{0:N2}"
                                                        ItemStyle-Wrap="false"  ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="Operation Date" DataField="start_use" SortExpression="start_use"                                                        
                                                        DataFormatString="{0:dd-MMM-yy}" HtmlEncode="false"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="Useful life (Year)" DataField="useful_date" SortExpression="useful_date"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="Location" DataField="location" SortExpression="location"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="CPT Model" DataField="model_name" SortExpression="model_name"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="Cell" DataField="cell" SortExpression="cell"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="Station" DataField="station" SortExpression="station"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="Responsible" DataField="responsible"  SortExpression="responsible"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="Due Date" DataField="duedate" SortExpression="duedate"                                                    
                                                        DataFormatString="{0:dd-MMM-yy}" HtmlEncode="false"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="Sub Fix" DataField="sub_fix" SortExpression="sub_fix"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="BOI" DataField="boi" SortExpression="boi"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="Paper" DataField="paper_name" SortExpression="paper_name"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="Category" DataField="category" SortExpression="category"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <asp:BoundField HeaderText="Remark" DataField="Remark" SortExpression="Remark"
                                                        ItemStyle-Wrap="false" ItemStyle-CssClass="rowGv2" HeaderStyle-CssClass="headerGv2" />

                                                    <%--      <asp:TemplateField HeaderText="Tool No." ItemStyle-Wrap="false" ItemStyle-CssClass="w3-border w3-border-light-gray" HeaderStyle-CssClass=" w3-border w3-border-light-gray">
                                                        <ItemTemplate>
                                                            <asp:HiddenField ID="tool_id" runat="server" Value='<%#Eval("tool_id") %>' />
                                                            <asp:Label ID="lb_ToolNo" runat="server" Text='<%#Eval("tool_no") %>' ForeColor="Purple" Font-Size="Small" Font-Bold="true"></asp:Label>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Tool Name" ItemStyle-CssClass="w3-border w3-border-light-gray" HeaderStyle-CssClass=" w3-border w3-border-light-gray">
                                                        <ItemTemplate>
                                                            <div style="width: 200px !important; text-align: left !important">
                                                                <asp:Label ID="Label2" runat="server" Text='<%#Eval("tool_nameEN") %>' Width="100%"></asp:Label>
                                                            </div>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:BoundField HeaderText="Group" DataField="group_name" ItemStyle-CssClass="w3-border w3-border-light-gray" HeaderStyle-CssClass=" w3-border w3-border-light-gray" ItemStyle-HorizontalAlign="Center" />
                                                    <asp:BoundField HeaderText="Rank" DataField="rank_id" ItemStyle-CssClass="w3-border w3-border-light-gray" HeaderStyle-CssClass=" w3-border w3-border-light-gray" ItemStyle-HorizontalAlign="Center" />
                                                    <asp:BoundField HeaderText="Location" DataField="location1" ItemStyle-HorizontalAlign="Center" ItemStyle-CssClass="w3-border w3-border-light-gray" HeaderStyle-CssClass=" w3-border w3-border-light-gray" />
                                                    <asp:BoundField HeaderText="Model " DataField="model_name2" ItemStyle-HorizontalAlign="Center" ItemStyle-CssClass="w3-border w3-border-light-gray" HeaderStyle-CssClass=" w3-border w3-border-light-gray" />
                                                    --%>
                                                </columns>
                                            </asp:GridView>
                                        </div>

                                    </div>
                                </div>

                                <asp:Label ID="test" runat="server" Text=""></asp:Label>
                            </div>
                        </div>
                    </asp:Panel>
                </div>

                <%--</main>--%>
            </div>


            <%-- Modal Show Pictrue--%>
            <%--<div class="modal fade" id="modal_UploadPictureMain" style="margin-left: -500px" tabindex="-1" role="dialog" aria-hidden="true">
                <div class="modal-dialog modal-sm4">
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
            </div>--%>

            <%-- Modal Show Pictrue--%> 
            <div class="modal fade" id="modal_UploadPictureMain" tabindex="-1" role="dialog" style="padding:0px; min-width: 85%; left:25%;" >
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

            <%-- Modal Read me / วิธีใช้งาน --%>
            <div class="modal fade" id="modal_ReadMe" tabindex="-1" role="dialog" style="background: none; box-shadow: none; border: none;">
                <div class="modal-dialog" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                            <h4 class="modal-title">📖 วิธีใช้งานหน้านี้</h4>
                        </div>
                        <div class="modal-body">

                            <div class="rm-item">
                                <div class="rm-ico"><span class="glyphicon glyphicon-search"></span></div>
                                <div class="rm-txt">
                                    <strong>ค้นหารายการ</strong>
                                    <span>พิมพ์ในช่อง 🔍 ค้นหารายการ แล้วกดปุ่มค้นหา ระบบค้นได้จาก ID, Tool No, Fix Asset, Asset Name, Control No และ Serial No</span>
                                </div>
                            </div>

                            <div class="rm-item">
                                <div class="rm-ico"><span class="glyphicon glyphicon-filter"></span></div>
                                <div class="rm-txt">
                                    <strong>กรองข้อมูล (Filter)</strong>
                                    <span>เลือกกรองตาม Status, Dept, Responsible, Category, Fix/Non Fix หรือพิมพ์กรอง CPT Model / Cell / Station ได้ ระบบกรองให้อัตโนมัติ</span>
                                </div>
                            </div>

                            <div class="rm-item">
                                <div class="rm-ico"><span class="glyphicon glyphicon-sort"></span></div>
                                <div class="rm-txt">
                                    <strong>เรียงลำดับ (Sort)</strong>
                                    <span>กดที่หัวคอลัมน์เพื่อเรียงข้อมูล กดซ้ำเพื่อสลับจากน้อยไปมาก (▲) เป็นมากไปน้อย (▼)</span>
                                </div>
                            </div>

                            <div class="rm-item">
                                <div class="rm-ico"><span class="glyphicon glyphicon-th-list"></span></div>
                                <div class="rm-txt">
                                    <strong>จำนวนแถว / หน้า</strong>
                                    <span>เลือกแสดง 10 / 20 / 50 รายการต่อหน้าได้ที่มุมซ้ายบนของตาราง และเปลี่ยนหน้าด้วยแถบเลขหน้า</span>
                                </div>
                            </div>

                            <div class="rm-item">
                                <div class="rm-ico"><span class="glyphicon glyphicon-eye-open"></span></div>
                                <div class="rm-txt">
                                    <strong>ดูรายละเอียด</strong>
                                    <span>กดไอคอนรูปตา (Action) ที่แต่ละแถวเพื่อเปิดหน้ารายละเอียดของรายการนั้นในแท็บใหม่</span>
                                </div>
                            </div>

                            <div class="rm-item">
                                <div class="rm-ico"><span class="glyphicon glyphicon-picture"></span></div>
                                <div class="rm-txt">
                                    <strong>ดูรูปภาพ</strong>
                                    <span>กดไอคอนรูปภาพ (Picture) ที่แต่ละแถวเพื่อดูรูปของรายการนั้นในหน้าต่างป็อปอัพ</span>
                                </div>
                            </div>

                            <div class="rm-item">
                                <div class="rm-ico"><span class="glyphicon glyphicon-save"></span></div>
                                <div class="rm-txt">
                                    <strong>Export Excel</strong>
                                    <span>กดปุ่ม Export เพื่อดาวน์โหลดข้อมูลตามเงื่อนไขการค้นหา/กรองปัจจุบันเป็นไฟล์ Excel</span>
                                </div>
                            </div>

                            <div class="rm-item">
                                <div class="rm-ico"><span class="glyphicon glyphicon-erase"></span></div>
                                <div class="rm-txt">
                                    <strong>ล้างตัวกรอง</strong>
                                    <span>กดปุ่มยางลบสีแดงเพื่อล้างการค้นหาและตัวกรองทั้งหมด กลับสู่มุมมองเริ่มต้น</span>
                                </div>
                            </div>

                        </div>
                    </div>
                </div>
            </div>


        </contenttemplate>
        <triggers>
             <asp:PostBackTrigger ControlID="btn_excel" />
        </triggers>
    </asp:UpdatePanel>



</asp:Content>
