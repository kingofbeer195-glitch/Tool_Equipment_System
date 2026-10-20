<%@ page title="" language="C#" masterpagefile="~/Tool_Equipment.Master"
    autoeventwireup="true" codefile="tool_equipment_detail.aspx.cs"
    inherits="Tool_Equipment_Control_System.tool_equipment_detail"
    enableeventvalidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <%-- ══ CSS ══ --%>
    <link href="CSS/bootstrap.css" rel="stylesheet" />
    <link href="CSS/jquery-ui.css" rel="stylesheet" />
    <link href="CSS/select2.min.css" rel="stylesheet" />          
    <link href="CSS/font-awesome4.7.0.min.css" rel="stylesheet" />
    <link href="DateTimePicker/2-bootstrap-datetimepicker.min.css" rel="stylesheet" />
    <link href="DateTimePicker/3-bootstrap-datetimepicker-standalone.css" rel="stylesheet" />

    <%-- ══ JS ══ --%>
    <script src="JS/jquery-1.10.2.js"></script>
    <script src="JS/jquery-ui.js"></script>
    <script src="Scripts/script.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>
    <script src="Scripts/select2.min.js"></script>              
    <script src="DateTimePicker/1-moment.min.js"></script>
    <script src="DateTimePicker/4-bootstrap-datetimepicker.min.js"></script>

    <%--MODAL READ ME--%>
    <style>
        #modal_ReadMe .modal-dialog { width: auto; max-width: 640px; margin: 40px auto; }
        #modal_ReadMe .modal-content { border-radius: 10px; overflow: hidden; }
        #modal_ReadMe .modal-header {
            background: #660000; color: #fff; border-bottom: none; padding: 12px 18px;
        }
        #modal_ReadMe .modal-header .close {
            color: #fff; opacity: 0.9; text-shadow: none; font-size: 26px;
        }
        #modal_ReadMe .modal-title { color: #fff; font-size: 17px; font-weight: 700; }
        #modal_ReadMe .modal-body { padding: 18px 22px; font-size: 13px; color: #2c3e50; }
        #modal_ReadMe .rm-item {
            display: flex; align-items: flex-start; gap: 10px; padding: 8px 0;
            border-bottom: 1px solid #eef0f4;
        }
        #modal_ReadMe .rm-item:last-child { border-bottom: none; }
        #modal_ReadMe .rm-ico {
            width: 30px; height: 30px; flex-shrink: 0;
            display: flex; align-items: center; justify-content: center;
            background: #f0f4fb; border-radius: 6px; color: #1a3a6b; font-size: 14px;
        }
        #modal_ReadMe .rm-txt strong { color: #1a3a6b; display: block; margin-bottom: 2px; }
        #modal_ReadMe .rm-txt span { color: #6b7a99; }
    </style>
    <%--RESET & BASE / PAGE WRAPPER / SECTION CARD--%>
    <style>
        /* ══════════════════════════════════════════════════════════
           RESET & BASE
        ══════════════════════════════════════════════════════════ */
        *, *::before, *::after {
            box-sizing: border-box;
        }
        /* ══════════════════════════════════════════════════════════
           PAGE WRAPPER
        ══════════════════════════════════════════════════════════ */
        .detail-page {
            /*max-width: 1600px;*/
            margin: 12px auto;
            padding: 0 12px 24px;
            font-family: 'Segoe UI', Tahoma, sans-serif;
            font-size: 13px;
        }
        /* ══════════════════════════════════════════════════════════
           SECTION CARD
        ══════════════════════════════════════════════════════════ */
        .sec-card {
            background: #fff;
            border: 1px solid #dce3ee;
            border-radius: 8px;
            margin-bottom: 10px;
            overflow: visible;
        }

        .sec-header {
            display: flex;
            align-items: center;
            gap: 8px;
            padding: 7px 14px;
            /*background: #f0f4fb;*/
            border-bottom: 1px solid #dce3ee;
            border-radius: 8px 8px 0 0;
            font-weight: 700;
            font-size: 18px;
            color: #1a3a6b;
        }

            .sec-header .sec-icon {
                font-size: 15px;
            }

        .sec-body {
            padding: 10px 12px;
        }
    </style>

    <%--PAGE HEADER BAR--%>
    <style>
        /* ══════════════════════════════════════════════════════════
           PAGE HEADER BAR
        ══════════════════════════════════════════════════════════ */
        .page-header-bar {
            display: flex;
            align-items: center;
            justify-content: space-between;
            padding: 8px 16px;
            background: #660000;
            border-radius: 8px;
            margin-bottom: 10px;
            color: #fff;
        }

            .page-header-bar .page-title {
                font-size: 22px;
                font-weight: 700;
                letter-spacing: 0.3px;
            }

            .page-header-bar .action-btns {
                display: flex;
                align-items: center;
                gap: 6px;
            }

                .page-header-bar .action-btns a,
                .page-header-bar .action-btns span {
                    color: #fff !important;
                    font-size: 16px;
                    padding: 3px 6px;
                    border-radius: 4px;
                    transition: background 0.15s;
                    cursor: pointer;
                    text-decoration: none;
                }

                    .page-header-bar .action-btns a:hover {
                        background: rgba(255,255,255,0.2);
                    }
    </style>

    <%--FROM FIELD--%>
    <style>
        /* ══════════════════════════════════════════════════════════
           FORM FIELD
           ทุก field ใช้ .field-group ครอบ label + control
        ══════════════════════════════════════════════════════════ */
        .field-group {
            display: flex;
            flex-direction: column;
            gap: 1px;
            min-width: 0; /* ป้องกัน overflow ใน grid */
        }

        .field-label {
            font-size: 12px;
            font-weight: 700;
            color: #6b7a99;
            text-transform: uppercase;
            letter-spacing: 0.4px;
            white-space: nowrap;
            padding-left: 2px;
        }

        .field-control {
            height: 30px !important;
            border: 1.5px solid #c8d6e5 !important;
            border-radius: 5px !important;
            font-size: 13px !important;
            font-weight: 600 !important;
            color: #2c3e50 !important;
            padding: 3px 8px !important;
            width: 100% !important;
            background: #fff !important;
            transition: border-color 0.2s;
        }

            .field-control:focus {
                border-color: #1565c0 !important;
                box-shadow: 0 0 0 3px rgba(21,101,192,0.1) !important;
                outline: none !important;
            }

            .field-control[disabled],
            .field-control[readonly] {
                /*background: #f0f4f8 !important;*/
                color: #555 !important;
            }

            .field-control.text-center {
                text-align: center !important;
            }

        select.field-control {
            text-align: center !important;
        }

        textarea.field-control {
            height: auto !important;
            min-height: 70px;
            resize: vertical;
            font-size: 13px !important;
        }
        /* open_edit = เปิด edit mode */
        .open_edit .field-control,
        .field-control.open_edit {
            /*background: #fbffa8 !important;*/
            background: #fffde7 !important;
            border-color: #f9a825 !important;
            color: #e65100 !important;
        }

        .txt_detailColor{background: #c2c8e25e !important;}
        .chosen-disabled {
            opacity: 1 !important;          
            cursor: default;
        }
        .chosen-results 
        {
            text-align:center;

        }

        /* Enabled → ส้ม */
        .chosen-container-single.chosen-enabled .chosen-single {
            background: #fffde7 !important;
            border-color: #f9a825 !important;
            color: #e65100 !important;
        }
        .chosen-container-single.chosen-enabled .chosen-single span {
            color: #e65100 !important;
        }
        .chosen-container-single.chosen-enabled .chosen-single div b {
            filter: invert(45%) sepia(90%) saturate(500%) hue-rotate(10deg);
        }
        .chosen-container-single.chosen-enabled .chosen-drop {
            border-color: #ffa726 !important;
        }
        .chosen-container-single.chosen-enabled .chosen-results li.highlighted {
            background: #c2c8e25e !important;
            color: #e65100 !important;
            text-align:center;
        }

         /*Disabled*/
        .chosen-container-single .chosen-single {
            background: #c2c8e25e !important;
            /*border-color: #90caf9 !important;*/
            /*color: #1565c0 !important;*/
        }
        .chosen-container-single .chosen-single span {
            /*color: #1565c0 !important;*/
        }
        .chosen-container-single .chosen-single div b {
            filter: invert(40%) sepia(80%) saturate(400%) hue-rotate(180deg);
        }

    </style>

    <%--SELECT2 OVERRIDE (คง look เดิมของ Chosen)--%>
    <style>
        /* ── โครง single select ── */
        /*.select2-container { width: 100% !important; }*/

        .select2-container--default .select2-selection--single {
            height: 30px !important;
            border: 1.5px solid #c8d6e5 !important;
            border-radius: 5px !important;
            background: #c2c8e25e !important;          /* disabled = ฟ้าอ่อน (เดิม) */
        }
        .select2-container--default .select2-selection--single .select2-selection__rendered {
            line-height: 28px !important;
            font-size: 13px !important;
            font-weight: 600 !important;
            color: #2c3e50 !important;
            text-align: center !important;
            padding-left: 8px !important;
            padding-right: 22px !important;
        }
        .select2-container--default .select2-selection--single .select2-selection__arrow {
            height: 28px !important;
        }

        /* ── ผลลัพธ์ใน dropdown จัดกลาง (เหมือน .chosen-results เดิม) ── */
        .select2-results__option { text-align: center; font-size: 13px; }
        .select2-container--default .select2-results__option--highlighted[aria-selected] {
            background-color: #c2c8e25e !important;
            color: #e65100 !important;
        }
        .select2-dropdown { border-color: #c8d6e5 !important; }

        /* ── Enabled → ส้ม (แทน .chosen-enabled เดิม) ──
           select2 ใส่ .select2-container ต่อจาก <select>
           เราจะ toggle class .s2-enabled ที่ container ด้วย JS */
        .select2-container.s2-enabled .select2-selection--single {
            background: #fffde7 !important;
            border-color: #f9a825 !important;
        }
        .select2-container.s2-enabled .select2-selection--single .select2-selection__rendered {
            color: #e65100 !important;
        }
        .select2-container.s2-enabled .select2-selection--single .select2-selection__arrow b {
            border-color: #e65100 transparent transparent transparent !important;
        }

        /* กล่อง search ในdropdown ชิดซ้ายตามปกติ */
        .select2-search--dropdown .select2-search__field {
            text-align: left;
            border: 1.5px solid #c8d6e5 !important;
            border-radius: 5px !important;
        }
    </style>

    <%--DATETIMEPICKER--%>
    <style>
        /* ══════════════════════════════════════════════════════════
           DATETIMEPICKER
        ══════════════════════════════════════════════════════════ */
        .dp-wrap {
            display: flex;
            align-items: center;
            width: 100%;
            height: 30px;
        }

            .dp-wrap .form-control {
                flex: 1;
                height: 30px !important;
                border: 1.5px solid #c8d6e5 !important;
                border-right: 0 !important;
                border-radius: 5px 0 0 5px !important;
                font-size: 13px !important;
                font-weight: 600 !important;
                color: #2c3e50 !important;
                text-align: center !important;
                /*background: #f0f4f8 !important;*/
            }

                .dp-wrap .form-control:focus {
                    border-color: #1565c0 !important;
                    outline: none;
                }

            .dp-wrap .input-group-addon {
                width: 32px !important;
                height: 30px !important;
                padding: 0 !important;
                line-height: 28px !important;
                text-align: center !important;
                background: #1565c0 !important;
                border: 1.5px solid #1565c0 !important;
                border-radius: 0 5px 5px 0 !important;
                color: #fff !important;
                cursor: pointer;
                transition: background 0.15s;
            }

                .dp-wrap .input-group-addon:hover {
                    background: #0d47a1 !important;
                }

            .dp-wrap.dp-disabled .input-group-addon {
                background: #b0bec5 !important;
                border-color: #b0bec5 !important;
                cursor: default;
            }
        /* dropdown calendar */
        .bootstrap-datetimepicker-widget {
            border-radius: 8px !important;
            box-shadow: 0 6px 20px rgba(0,0,0,0.15) !important;
            border: 1px solid #c8d6e5 !important;
            font-size: 13px !important;
        }

            .bootstrap-datetimepicker-widget table td.active,
            .bootstrap-datetimepicker-widget table td.active:hover {
                background: #1565c0 !important;
                border-radius: 50% !important;
            }

            .bootstrap-datetimepicker-widget table td.today::before {
                border-bottom-color: #1565c0 !important;
            }

            .bootstrap-datetimepicker-widget table th {
                color: #1565c0 !important;
                font-weight: 700 !important;
            }
    </style>

    <%--PICTURE SECTION--%>
    <style>
        /* ══════════════════════════════════════════════════════════
           PICTURE SECTION
        ══════════════════════════════════════════════════════════ */
        /* Picture container ครอบทั้ง main + thumbs */
        .pic-container {
            display: flex;
            flex-direction: column;
            border: 1.5px solid #dce3ee;
            border-radius: 8px;
            overflow: hidden;
            background: #f5f7fa;
        }
        /* Main image area */
        .pic-main {
            width: 100%;
            aspect-ratio: 4/3;
            display: flex;
            align-items: center;
            justify-content: center;
            background: #f5f7fa;
            overflow: hidden;
            cursor: zoom-in;
            /* ลบ border ออก เพราะ container ครอบแทน */
            border: none;
            border-radius: 0;
            border-bottom: 1px solid #dce3ee; /* คั่นระหว่าง main กับ thumbs */
            padding: 10px;
            border-radius: 10px;
            max-height: 180px;
        }

            .pic-main img {
                max-width: 100%;
                max-height: 100%;
                object-fit: contain;
                transition: transform 0.2s;
            }

            .pic-main:hover img {
                transform: scale(1.03);
            }

        .pic-thumbs {
            display: flex;
            gap: 6px;
            overflow-x: auto;
            padding: 8px;
            background: #fff;
            min-height: 60px;
            align-items: center;
        }

            .pic-thumbs img {
                height: 40px;
                width: 40px;
                object-fit: cover;
                border: 2px solid #dce3ee;
                border-radius: 4px;
                cursor: pointer;
                flex-shrink: 0;
                transition: border-color 0.15s, transform 0.15s;
            }

                .pic-thumbs img:hover {
                    border-color: #1565c0;
                    transform: scale(1.08);
                }

                .pic-thumbs img.selected {
                    border-color: #1565c0;
                    box-shadow: 0 0 0 2px #1565c040;
                }

            .pic-thumbs::-webkit-scrollbar {
                height: 6px;
            }

            .pic-thumbs::-webkit-scrollbar-thumb {
                background: #b0bec5;
                border-radius: 2px;
            }
    </style>

    <%--MODAL PICTURE (แสดงรูปขนาดจริง)--%>
    <style>
        #modal_ViewPicture .modal-dialog {
            width: auto;
            max-width: 90%;
            margin: 30px auto;
        }
        #modal_ViewPicture .modal-content {
            border-radius: 10px;
            overflow: hidden;
        }
        #modal_ViewPicture .modal-header {
            background: #660000;
            color: #fff;
            border-bottom: none;
            padding: 10px 16px;
        }
        #modal_ViewPicture .modal-header .close {
            color: #fff;
            opacity: 0.9;
            text-shadow: none;
            font-size: 26px;
        }
        #modal_ViewPicture .modal-title {
            color: #fff;
            font-size: 16px;
            font-weight: 700;
        }
        #modal_ViewPicture .modal-body {
            text-align: center;
            background: #f5f7fa;
            padding: 12px;
            max-height: 80vh;
            overflow: auto;
        }
        #modal_ViewPicture .modal-body img {
            max-width: 100%;
            height: auto;
        }
    </style>

    <%--FUNCTION BUTTONS--%>
    <style>
        /* ══════════════════════════════════════════════════════════
           FUNCTION BUTTONS
        ══════════════════════════════════════════════════════════ */
        .func-btns {
            display: flex;
            align-items: center;
            gap: 10px;
            padding: 6px 0 2px;
            border-top: 1px solid #eef0f4;
            margin-top: 4px;
        }

            .func-btns .func-label {
                font-size: 12px;
                font-weight: 700;
                color: #6b7a99;
                white-space: nowrap;
            }

        .img-btn {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            width: 44px;
            height: 44px;
            border: 1.5px solid #c8d6e5;
            border-radius: 8px;
            background: #fff;
            cursor: pointer;
            transition: all 0.15s;
            padding: 6px;
        }

            .img-btn:hover {
                background: #e3f0ff;
                border-color: #1565c0;
            }

            .img-btn img {
                width: 100%;
                height: 100%;
                object-fit: contain;
            }
    </style>

    <%--SECTION: TOP (Picture + Detail) — responsive split--%>
    <style>
        /* ══════════════════════════════════════════════════════════
           SECTION: TOP (Picture + Detail) — responsive split
        ══════════════════════════════════════════════════════════ */
        .top-split {
            display: grid;
            grid-template-columns: 350px 1fr; /* picture fixed, detail flexible */
            gap: 12px;
            align-items: start;
        }

        @media (max-width: 900px) {
            .top-split {
                grid-template-columns: 1fr;
            }
        }
        /* ── Detail side inner grids ── */
        .detail-rows {
            display: flex;
            flex-direction: column;
            gap: 8px;
        }
        /* Row: 4 columns  ID / ToolNo / No. / Category */
        .row-4col {
            display: grid;
            grid-template-columns: 100px 1fr 80px 180px;
            gap: 8px;
        }
        /* Row: 1 column  Tool Name */
        .row-1col {
            display: grid;
            grid-template-columns: 1fr;
        }
        /* Row: 3 columns  Status / BFM / Control No */
        .row-3col {
            display: grid;
            grid-template-columns: 1fr 1fr 1fr;
            gap: 8px;
        }
        /* Row: 4 columns  dates */
        .row-4col-date {
            display: grid;
            grid-template-columns: 185px 1fr 185px 140px;
            gap: 8px;
        }

        @media (max-width: 1200px) {
            .row-4col {
                grid-template-columns: 90px 1fr 70px 120px;
            }

            .row-4col-date {
                grid-template-columns: 1fr 90px 1fr 110px;
            }
        }

        @media (max-width: 900px) {
            .row-4col, .row-3col, .row-4col-date {
                grid-template-columns: 1fr 1fr;
            }
        }

        @media (max-width: 560px) {
            .row-4col, .row-3col, .row-4col-date {
                grid-template-columns: 1fr;
            }
        }
    </style>

    <%--SECTION: LOCATION / SECTION: OTHER DETAIL --%>
    <style>
        /* ══════════════════════════════════════════════════════════
           SECTION: LOCATION
        ══════════════════════════════════════════════════════════ */
        .loc-row1 {
            display: grid;
            grid-template-columns: 1fr 140px 1fr 1fr;
            gap: 8px;
            margin-bottom: 8px;
        }

        .loc-row2 {
            display: grid;
            grid-template-columns: 220px 1fr 1fr;
            gap: 8px;
        }

        @media (max-width: 900px) {
            .loc-row1, .loc-row2 {
                grid-template-columns: 1fr 1fr;
            }
        }

        @media (max-width: 560px) {
            .loc-row1, .loc-row2 {
                grid-template-columns: 1fr;
            }
        }
        /* ══════════════════════════════════════════════════════════
           SECTION: OTHER DETAIL
        ══════════════════════════════════════════════════════════ */
        .other-row1 {
            display: grid;
            grid-template-columns: 2fr 1fr 1fr;
            gap: 8px;
            margin-bottom: 8px;
        }

        .other-row2 {
            display: grid;
            grid-template-columns: 220px 140px 140px 140px 170px 1fr;
            gap: 8px;
            margin-bottom: 8px;
        }

        .other-row3 {
            display: grid;
            grid-template-columns: 160px 1fr 1fr;
            gap: 8px;
        }

        @media (max-width: 1200px) {
            .other-row2 {
                grid-template-columns: 1fr 1fr 1fr;
            }
        }

        @media (max-width: 900px) {
            .other-row1, .other-row2, .other-row3 {
                grid-template-columns: 1fr 1fr;
            }
        }

        @media (max-width: 560px) {
            .other-row1, .other-row2, .other-row3 {
                grid-template-columns: 1fr;
            }
        }
    </style>

    <%-- ══════════════════════════════════════════════════════════
         JAVASCRIPT
    ══════════════════════════════════════════════════════════ --%>

    <%-- Select2 init + ranking search (เหมือน EditLocation) --%>
    <script>
        // ── normalize + ranking (ยกมาจาก EditLocation) ──
        function normalizeText(s) {
            return (s || '').toLowerCase().replace(/[.\,\-_\/]/g, '');
        }
        function isWordStart(rawText, rawTerm) {
            var term = normalizeText(rawTerm).trim();
            if (term === '') return false;
            var words = (rawText || '').toLowerCase().split(/\s+/);
            for (var i = 0; i < words.length; i++) {
                if (normalizeText(words[i]).indexOf(term) === 0) return true;
            }
            return false;
        }
        function matchScore(rawText, rawTerm) {
            var text = normalizeText(rawText);
            var term = normalizeText(rawTerm).trim();
            if (term === '') return 0;
            var idx = text.indexOf(term);
            if (idx === -1) return -1;
            if (text === term) return 0;
            if (idx === 0) return 1;
            if (isWordStart(rawText, rawTerm)) return 2;
            return 3 + idx;
        }
        function rankMatcher(params, data) {
            if ($.trim(params.term) === '') return data;
            if (typeof data.text === 'undefined') return null;
            var score = matchScore(data.text, params.term);
            if (score < 0) return null;
            var modified = $.extend({}, data, true);
            modified._score = score;
            return modified;
        }
        function rankSorter(results) {
            return results.sort(function (a, b) {
                var sa = (typeof a._score === 'undefined') ? 999 : a._score;
                var sb = (typeof b._score === 'undefined') ? 999 : b._score;
                if (sa !== sb) return sa - sb;
                return (a.text || '').localeCompare(b.text || '');
            });
        }

        // ── สี ส้ม/ฟ้า ตาม enabled ── (แทน updateAllChosenColors เดิม)
        function updateAllSelect2Colors() {
            $('select.chzn-select').each(function () {
                var $c = $(this).next('.select2-container');
                $c.toggleClass('s2-enabled', !$(this).prop('disabled'));
            });
        }

        function initSelect2() {
            $('.chzn-select').each(function () {
                var $el = $(this);
                if ($el.hasClass('select2-hidden-accessible')) {
                    $el.select2('destroy');
                }
                $el.select2({
                    width: '100%',
                    dropdownAutoWidth: true,
                    minimumResultsForSearch: 0,   // แสดงช่องค้นหาเสมอ
                    sorter: rankSorter,
                    matcher: rankMatcher
                });
            });

            // โฟกัสช่องค้นหาเมื่อเปิด
            $('.chzn-select').off('select2:open.detail').on('select2:open.detail', function () {
                setTimeout(function () {
                    var f = document.querySelector('.select2-container--open .select2-search__field');
                    if (f) f.focus();
                }, 0);
            });

            updateAllSelect2Colors();

            $('select.chzn-select').off('change.s2color').on('change.s2color', function () {
                updateAllSelect2Colors();
            });
        }

        $(function () { initSelect2(); });

        // re-init หลัง UpdatePanel partial postback
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                initSelect2();
                updateAllSelect2Colors();
            });
        }
    </script>


    <%-- DateTimePicker --%>
    <script>
        function initDatePickers() {
            moment.locale('en');
            var opts = {
                format: 'DD-MMM-YYYY',
                useCurrent: false,
                showClear: true,
                showClose: true,
                locale: 'en',
                icons: {
                    time: 'glyphicon glyphicon-time',
                    date: 'glyphicon glyphicon-calendar',
                    up: 'glyphicon glyphicon-chevron-up',
                    down: 'glyphicon glyphicon-chevron-down',
                    previous: 'glyphicon glyphicon-chevron-left',
                    next: 'glyphicon glyphicon-chevron-right',
                    today: 'glyphicon glyphicon-screenshot',
                    clear: 'glyphicon glyphicon-trash',
                    close: 'glyphicon glyphicon-remove'
                }
            };
            $('#opDatePicker').datetimepicker(opts);
            $('#DueDatePicker').datetimepicker(opts);
            /* set ค่าเริ่มต้น — รองรับหลาย format จาก DB */
            var fmts = ['DD-MMM-YYYY', 'DD MMM YYYY', 'YYYY-MM-DD', 'DD/MM/YYYY'];
            var rawOp = $('#<%= txt_operationDate.ClientID %>').val();
            var rawDue = $('#<%= txt_duedate.ClientID %>').val();
            var mOp = moment(rawOp, fmts, false);
            var mDue = moment(rawDue, fmts, false);
            if (mOp.isValid()) $('#opDatePicker').data('DateTimePicker').date(mOp);
            if (mDue.isValid()) $('#DueDatePicker').data('DateTimePicker').date(mDue);
            /* sync disabled state */
            var opDis = $('#<%= txt_operationDate.ClientID %>').prop('disabled');
            var dueDis = $('#<%= txt_duedate.ClientID %>').prop('disabled');
            $('#opDatePicker').toggleClass('dp-disabled', opDis);
            $('#DueDatePicker').toggleClass('dp-disabled', dueDis);
        }
        $(function () { initDatePickers(); });
        if (typeof Sys !== 'undefined') { Sys.Application.add_load(initDatePickers); }
    </script>

    <%-- Image switcher --%>
    <script>
        function changeMainImage(src) {
            document.getElementById('mainImage').src = src;
            document.querySelectorAll('.pic-thumbs img').forEach(function (img) {
                img.classList.toggle('selected', img.getAttribute('src') === src);
            });
        }
        window.onload = function () {
            var init = '<%= ImagePaths.Count > 0 ? ImagePaths[0] : "" %>';
            if (init) changeMainImage(init);
        };

        function openPictureModal(src) {
            if (!src) return;
            document.getElementById('modalPictureImg').src = src;
            $('#modal_ViewPicture').modal('show');
        }
    </script>

    <%-- ══════════════════════════════════════════════════════════
         SERVER CONTROLS
    ══════════════════════════════════════════════════════════ --%>
    <asp:ScriptManager ID="ScriptManager1" EnablePageMethods="true" runat="server" />

    <asp:UpdateProgress ID="UpdateProgress1" runat="server"
        AssociatedUpdatePanelID="UpdatePanel1">
        <progresstemplate>
            <div class="overlay">
                <div class="overlayContent">
                    <div style="position: fixed; font-size: 36px; text-align: center;">
                        Loading...<br />
                        <i class="fa fa-spinner fa-spin"></i>
                    </div>
                </div>
            </div>
        </progresstemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <contenttemplate>
            <div class="detail-page" style="margin-top: 30px; float: left; width: calc(100vh + 310px);">

                <%-- ══ PAGE HEADER BAR ══ --%>
                <div id="toolHeader" runat="server" class="page-header-bar">
                    <span class="page-title">🔧 Tool &amp; Equipment Detail</span>
                    <div class="action-btns">
                        <asp:LinkButton ID="btn_edit_data" Visible="true"
                            runat="server" OnClick="btn_edit_data_Click"
                            title="แก้ไข">
                            <span class="glyphicon glyphicon-edit"></span>
                        </asp:LinkButton>
                        <asp:LinkButton ID="btn_SaveEdit_data" runat="server" Visible="false"
                            OnClick="btn_SaveEdit_data_Click" title="บันทึก">
                            <span class="glyphicon glyphicon-save"></span>
                        </asp:LinkButton>
                        <asp:LinkButton ID="btn_cancelEdit" runat="server" Visible="false" title="ยกเลิก">
                            <span class="glyphicon glyphicon-remove-circle"
                                onclick="window.location.reload()"></span>
                        </asp:LinkButton>
                        <%-- ปุ่ม Help / Read me --%>
                        <span class="glyphicon glyphicon-question-sign"
                            title="วิธีใช้งาน" onclick="$('#modal_ReadMe').modal('show');"></span>
                    </div>
                </div>

                <%-- ══ SECTION 1 : Picture + Detail & Status ══ --%>
                <div class="sec-card">
                    <div class="sec-header" style="background: #bdc5e9;">
                        <span class="sec-icon">📋</span> Detail &amp; Status
                    </div>
                    <div class="sec-body">
                        <div class="top-split">

                            <%-- ── Picture ── --%>
                            <div>
                                <%-- Header: ชื่อ + ปุ่มแก้ไข --%>
                                <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 6px;">
                                    <span style="font-weight: 700; color: #1a3a6b; font-size: 12px;">📷 Picture
                                    </span>
                                    <asp:LinkButton ID="btn_picture" runat="server" Visible="true"
                                        OnClick="btn_picture_Click" title="แก้ไขรูปภาพ"
                                        style="font-size: 14px; color: #1565c0;">
                                        <span class="glyphicon glyphicon-edit"></span>
                                    </asp:LinkButton>
                                </div>

                                <%-- Container ครอบ main + thumbs --%>
                                <div class="pic-container">

                                    <%-- Main Image --%>
                                    <div class="pic-main">
                                        <img id="mainImage" src="<%=ImagePaths[0]%>" alt="Main Image"
                                            onclick="openPictureModal(this.src)" />
                                    </div>

                                    <%-- Thumbnail Strip --%>
                                    <div class="pic-thumbs">
                                        <% for (int i = 0; i < ImagePaths.Count; i++) { %>
                                        <img src="<%=ImagePaths[i]%>"
                                            alt="Thumbnail <%=i+1%>"
                                            onclick="changeMainImage('<%=ImagePaths[i]%>')"
                                            <%=i==0 ? "class=\"selected\"" : ""%> />
                                        <% } %>
                                    </div>

                                </div>
                                <%-- /pic-container --%>
                            </div>

                            <%-- ── Detail rows ── --%>
                            <div class="detail-rows">

                                <%-- Row 1 : ID / Tool No / No. / Category --%>
                                <div class="row-4col">
                                    <div class="field-group">
                                        <span class="field-label">ID</span>
                                        <asp:TextBox ID="txt_id" runat="server"
                                            CssClass="field-control text-center txt_detailColor" Enabled="false" />
                                    </div>
                                    <div class="field-group">
                                        <span class="field-label">Tool No / Model</span>
                                        <asp:TextBox ID="txt_toolno_model" runat="server"
                                            CssClass="field-control text-center txt_detailColor" Enabled="false" />
                                    </div>
                                    <div class="field-group">
                                        <span class="field-label">No.</span>
                                        <asp:TextBox ID="txt_no" runat="server"
                                            CssClass="field-control text-center txt_detailColor" Enabled="false" />
                                    </div>
                                    <div class="field-group">
                                        <span class="field-label">Category</span>
                                        <asp:TextBox ID="txt_category" runat="server"
                                            CssClass="field-control text-center txt_detailColor" Enabled="false" />
                                    </div>
                                </div>

                                <%-- Row 2 : Tool Name --%>
                                <div class="row-1col">
                                    <div class="field-group">
                                        <span class="field-label">Tool Name / Equipment Name</span>
                                        <asp:TextBox ID="txtToolname" runat="server"
                                            CssClass="field-control txt_detailColor" Enabled="false" />
                                    </div>
                                </div>

                                <%-- Row 3 : Status / BFM No. / Control No. --%>
                                <div class="row-3col">
                                    <div class="field-group">
                                        <span class="field-label" style="margin-bottom:5px;">Status</span>
                                        <asp:DropDownList ID="ddl_status" runat="server"
                                            CssClass="chzn-select"
                                            DataSourceID="sql_status"
                                            DataValueField="status_id" DataTextField="status_name"
                                            AppendDataBoundItems="true"
                                            Enabled="false" Width="100%" />
                                        <asp:SqlDataSource ID="sql_status" runat="server"
                                            ConnectionString="<%$ ConnectionStrings:tool_equipment %>"
                                            SelectCommand="SELECT * FROM tool_equipment_status" />
                                    </div>
                                    <div class="field-group">
                                        <span class="field-label">BFM No.</span>
                                        <asp:TextBox ID="txt_fixassetNo" runat="server"
                                            CssClass="field-control text-center txt_detailColor" Enabled="false" />
                                    </div>
                                    <div class="field-group">
                                        <span class="field-label">Control No.</span>
                                        <asp:TextBox ID="txt_controlNo" runat="server"
                                            CssClass="field-control text-center txt_detailColor" Enabled="false" />
                                    </div>
                                </div>

                                <%-- Row 4 : Operating Date / Useful Life / Due Date / Period --%>
                                <div class="row-4col-date">

                                    <div class="field-group">
                                        <span class="field-label">Operating Date</span>
                                        <div class="input-group date dp-wrap" id="opDatePicker">
                                            <asp:TextBox ID="txt_operationDate" runat="server"
                                                CssClass="form-control txt_detailColor"
                                                placeholder="DD-MMM-YYYY"
                                                Enabled="false" />
                                            <span class="input-group-addon" style="margin-top:5px;">
                                                <span class="glyphicon glyphicon-calendar"></span>
                                            </span>
                                        </div>
                                    </div>

                                    <div class="field-group">
                                        <span class="field-label">Useful Life (Yr)</span>
                                        <asp:TextBox ID="txt_useTime" runat="server" style="margin-top: 3px;"
                                            CssClass="field-control text-center  txt_detailColor" Enabled="false" />
                                    </div>

                                    <div class="field-group">
                                        <span class="field-label">Due Date Calibration</span>
                                        <div class="input-group date dp-wrap" id="DueDatePicker">
                                            <asp:TextBox ID="txt_duedate" runat="server"
                                                CssClass="form-control  txt_detailColor"
                                                placeholder="DD-MMM-YYYY"
                                                Enabled="false" />
                                            <span class="input-group-addon" style="margin-top:5px;">
                                                <span class="glyphicon glyphicon-calendar"></span>
                                            </span>
                                        </div>
                                    </div>
                                    <div class="field-group">
                                        <span class="field-label" style="margin-bottom:3px;">Cal. Period</span>
                                        <asp:DropDownList ID="ddl_calPeriod" runat="server"
                                            CssClass="chzn-select"
                                            DataSourceID="sql_calPeriod"
                                            DataTextField="period_Name" DataValueField="period_Id"
                                            Enabled="false" Width="100%" />
                                        <asp:SqlDataSource ID="sql_calPeriod" runat="server"
                                            ConnectionString="<%$ ConnectionStrings:tool_equipment %>"
                                            SelectCommand="SELECT * FROM tool_eduipment_system.dbo.Calibration_Period WHERE is_use = 1" />
                                    </div>
                                </div>

                                <%-- Row 5 : Function buttons --%>
                                <%--<div class="func-btns">
                                    <span class="func-label">Function :</span>
                                    <asp:LinkButton ID="btn_borrow" runat="server"
                                        CssClass="img-btn" ToolTip="ยืมอุปกรณ์"
                                        OnClick="btn_borrow_Click">
                                        <img src="images/return.png" alt="ยืม" />
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btn_withdraw" runat="server"
                                        CssClass="img-btn" ToolTip="เบิกอุปกรณ์"
                                        OnClick="btn_withdraw_Click">
                                        <img src="images/borrow1.png" alt="เบิก" />
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btn_storage" runat="server"
                                        CssClass="img-btn" ToolTip="จัดเก็บอุปกรณ์"
                                        OnClick="btn_storage_Click">
                                        <img src="images/storage.png" alt="จัดเก็บ" />
                                    </asp:LinkButton>
                                </div>--%>

                            </div>
                            <%-- /detail-rows --%>
                        </div>
                        <%-- /top-split --%>
                    </div>
                    <%-- /sec-body --%>
                </div>
                <%-- /sec-card detail --%>

                <%-- ══ SECTION 2 : Location ══ --%>
                <div class="sec-card">
                    <div class="sec-header" style="background: #ebe3d0;">
                        <span class="sec-icon">📍</span> Location
                    </div>
                    <div class="sec-body">

                        <%-- Row 1 : CPT Model / Location / Cell / Station --%>
                        <div class="loc-row1">
                            <div class="field-group">
                                <span class="field-label">CPT Model</span>
                                <asp:DropDownList ID="ddl_model" runat="server"
                                    CssClass="chzn-select"
                                    DataSourceID="sql_model"
                                    DataTextField="model_name" DataValueField="model_id"
                                    Enabled="false" Width="100%" />
                                <asp:SqlDataSource ID="sql_model" runat="server"
                                    ConnectionString="<%$ ConnectionStrings:jig_tool %>"
                                    SelectCommand="SELECT * FROM draw_master_model ORDER BY model_name" />
                            </div>
                            <div class="field-group">
                                <span class="field-label">Location</span>
                                <asp:DropDownList ID="ddl_location" runat="server"
                                    CssClass="chzn-select"
                                    DataSourceID="Sql_ddl_location"
                                    DataValueField="location1_id" DataTextField="location1_name"
                                    AutoPostBack="true" Enabled="false"
                                    OnSelectedIndexChanged="ddl_location_SelectedIndexChanged"
                                    Width="100%" />
                                <asp:SqlDataSource ID="Sql_ddl_location" runat="server"
                                    ConnectionString="<%$ ConnectionStrings:jig_tool %>"
                                    SelectCommand="SELECT location1_id, location1_name FROM master_location1 WHERE location1_id IN (97)" />
                            </div>
                            <div class="field-group">
                                <span class="field-label">Cell</span>
                                <asp:DropDownList ID="ddl_cell" runat="server"
                                    CssClass="chzn-select"
                                    AutoPostBack="true" AppendDataBoundItems="true"
                                    Enabled="false" Width="100%">
                                    <asp:ListItem Text="-" Value="" />
                                </asp:DropDownList>
                            </div>
                            <div class="field-group">
                                <span class="field-label">Station</span>
                                <asp:DropDownList ID="ddl_station" runat="server"
                                    CssClass="chzn-select"
                                    AutoPostBack="true" Enabled="false" Width="100%" />
                            </div>
                        </div>

                        <%-- Row 2 : Dept Code / Dept Name / Responsible --%>
                        <div class="loc-row2">
                            <div class="field-group">
                                <span class="field-label">Dept Code</span>
                                <asp:HiddenField ID="hd_divID" runat="server" />
                                <asp:HiddenField ID="hd_deptID" runat="server" />
                                <asp:DropDownList ID="ddl_dept" runat="server"
                                    CssClass="chzn-select"
                                    AutoPostBack="true" Enabled="false"
                                    OnSelectedIndexChanged="ddl_dept_SelectedIndexChanged"
                                    Width="100%" />
                            </div>
                            <div class="field-group">
                                <span class="field-label">Dept Name</span>
                                <asp:TextBox ID="txt_deptname" runat="server" style="margin-top:0px;"
                                    CssClass="field-control text-center txt_detailColor" Enabled="false" />
                            </div>
                            <div class="field-group">
                                <span class="field-label">Responsible</span>
                                <asp:DropDownList ID="ddl_responsible" runat="server"
                                    CssClass="chzn-select"
                                    AutoPostBack="true" AppendDataBoundItems="true"
                                    Enabled="false" Width="100%" />
                            </div>
                        </div>

                    </div>
                    <%-- /sec-body --%>
                </div>
                <%-- /sec-card location --%>

                <%-- ══ SECTION 3 : Other Detail ══ --%>
                <div class="sec-card">
                    <div class="sec-header" style="background: #e9d3e1;">
                        <span class="sec-icon">📋</span> Other Detail
                    </div>
                    <div class="sec-body">

                        <%-- Row 1 : Asset Name / Serial No. / Equipment Model --%>
                        <div class="other-row1">
                            <div class="field-group">
                                <span class="field-label">Asset Name</span>
                                <asp:TextBox ID="txt_assetName" runat="server"
                                    CssClass="field-control txt_detailColor" Enabled="false" />
                            </div>
                            <div class="field-group">
                                <span class="field-label">Serial No.</span>
                                <asp:TextBox ID="txt_serialNo" runat="server"
                                    CssClass="field-control text-center txt_detailColor" Enabled="false" />
                            </div>
                            <div class="field-group">
                                <span class="field-label">Equipment Model</span>
                                <asp:TextBox ID="txt_equipmentModel" runat="server"
                                    CssClass="field-control text-center txt_detailColor" Enabled="false" />
                            </div>
                        </div>

                        <%-- Row 2 : Check Type / Components / Paper / BOI / Price / PR/PO --%>
                        <div class="other-row2">
                            <div class="field-group">
                                <span class="field-label">Check Type</span>
                                <asp:DropDownList ID="ddl_checktype" runat="server"
                                    CssClass="chzn-select"
                                    AppendDataBoundItems="true" Enabled="false" Width="100%">
                                    <asp:ListItem Value="">-</asp:ListItem>
                                    <asp:ListItem Value="1">Tangible (มีรูปร่าง)</asp:ListItem>
                                    <asp:ListItem Value="2">Intangible (ไม่มีรูปร่าง)</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="field-group">
                                <span class="field-label">Components</span>
                                <asp:TextBox ID="txt_subfix" runat="server" style="margin-top:0px;"
                                    CssClass="field-control text-center txt_detailColor" Enabled="false" />
                            </div>
                            <div class="field-group">
                                <span class="field-label">Paper</span>
                                <asp:DropDownList ID="ddl_paper" runat="server"
                                    CssClass="chzn-select"
                                    DataSourceID="sql_paper"
                                    DataValueField="paper_id" DataTextField="paper_name"
                                    AppendDataBoundItems="true" Enabled="false" Width="100%" />
                                <asp:SqlDataSource ID="sql_paper" runat="server"
                                    ConnectionString="<%$ ConnectionStrings:tool_equipment %>"
                                    SelectCommand="SELECT * FROM Master_Paper" />
                            </div>
                            <div class="field-group">
                                <span class="field-label">BOI / Non BOI</span>
                                <asp:DropDownList ID="ddl_boi" runat="server"
                                    CssClass="chzn-select"
                                    DataSourceID="sql_boi"
                                    DataValueField="boi_id" DataTextField="boi_name"
                                    AppendDataBoundItems="true" Enabled="false" Width="100%" />
                                <asp:SqlDataSource ID="sql_boi" runat="server"
                                    ConnectionString="<%$ ConnectionStrings:tool_equipment %>"
                                    SelectCommand="SELECT * FROM Master_BOI" />
                            </div>
                            <div class="field-group">
                                <span class="field-label">Price</span>
                                <asp:TextBox ID="txt_price" runat="server" style="margin-top:0px;"
                                    CssClass="field-control text-center txt_detailColor" Enabled="false" />
                            </div>
                            <div class="field-group">
                                <span class="field-label">PR / PO</span>
                                <asp:TextBox ID="txt_prpo" runat="server" style="margin-top:0px;"
                                    CssClass="field-control text-center txt_detailColor" Enabled="false" />
                            </div>
                        </div>

                        <%-- Row 3 : Cost Center ID / Cost Center Name / Manufacture --%>
                        <div class="other-row3">
                            <div class="field-group">
                                <span class="field-label">Cost Center ID</span>
                                <asp:DropDownList ID="ddl_costcenterID" runat="server"
                                    CssClass="chzn-select"
                                    AppendDataBoundItems="true" AutoPostBack="true"
                                    Enabled="false"
                                    OnSelectedIndexChanged="ddl_costcenterID_SelectedIndexChanged"
                                    Width="100%">
                                    <asp:ListItem Value="0">-</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="field-group">
                                <span class="field-label">Cost Center Name</span>
                                <asp:TextBox ID="txt_costcenterName" runat="server" style="margin-top:0px;"
                                    CssClass="field-control" Enabled="false" />
                            </div>
                            <div class="field-group">
                                <span class="field-label">Manufacture</span>
                                <asp:TextBox ID="txt_manufacture" runat="server" style="margin-top:0px;"
                                    CssClass="field-control text-center txt_detailColor" Enabled="false" />
                            </div>
                        </div>

                    </div>
                    <%-- /sec-body --%>
                </div>
                <%-- /sec-card other --%>

                <%-- ══ SECTION 4 : Remark ══ --%>
                <div class="sec-card">
                    <div class="sec-header" style="background: #f6a0abc2;">
                        <span class="sec-icon">📝</span> Remark
                    </div>
                    <div class="sec-body">
                        <asp:TextBox ID="txt_remark" runat="server"
                            placeholder="กรอกหมายเหตุ..."
                            TextMode="MultiLine" Rows="3"
                            Enabled="false"
                            CssClass="field-control"
                            Style="height: 70px; resize: vertical;" />
                    </div>
                </div>

                <asp:Label ID="lb_test" runat="server" ForeColor="Red" />

            </div>
            <%-- /detail-page --%>
        </contenttemplate>

        <triggers>
            <%-- เพิ่ม PostBackTrigger ที่นี่ถ้าจำเป็น --%>
        </triggers>
    </asp:UpdatePanel>

    <%-- Modal แสดงรูปขนาดจริง --%>
    <div class="modal fade" id="modal_ViewPicture" tabindex="-1" role="dialog" style="background: none; box-shadow: none; border: none;">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                    <h4 class="modal-title">📷 Picture</h4>
                </div>
                <div class="modal-body">
                    <img id="modalPictureImg" src="" alt="Full size" />
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
                        <div class="rm-ico"><span class="glyphicon glyphicon-edit"></span></div>
                        <div class="rm-txt">
                            <strong>แก้ไขข้อมูล</strong>
                            <span>กดไอคอนดินสอมุมขวาบนเพื่อเปิดโหมดแก้ไข ช่องที่แก้ได้จะเปลี่ยนเป็นสีส้ม เมื่อเสร็จกดไอคอนบันทึก หรือกดยกเลิกเพื่อคืนค่าเดิม (เฉพาะสิทธิ์ Admin)</span>
                        </div>
                    </div>

                    <div class="rm-item">
                        <div class="rm-ico"><span class="glyphicon glyphicon-search"></span></div>
                        <div class="rm-txt">
                            <strong>ค้นหาใน Dropdown</strong>
                            <span>ช่องแบบ Dropdown (Status, Model, Location, Dept, Responsible ฯลฯ) พิมพ์เพื่อค้นหาได้ ระบบจะจัดลำดับรายการที่ตรงที่สุดขึ้นก่อน</span>
                        </div>
                    </div>

                    <div class="rm-item">
                        <div class="rm-ico"><span class="glyphicon glyphicon-picture"></span></div>
                        <div class="rm-txt">
                            <strong>ดูรูปขนาดจริง</strong>
                            <span>กดที่รูปภาพหลักเพื่อเปิดดูขนาดจริง และกดที่รูปเล็ก (Thumbnail) ด้านล่างเพื่อสลับรูปที่แสดง</span>
                        </div>
                    </div>

                    <div class="rm-item">
                        <div class="rm-ico"><span class="glyphicon glyphicon-camera"></span></div>
                        <div class="rm-txt">
                            <strong>แก้ไขรูปภาพ</strong>
                            <span>กดไอคอนดินสอที่หัวข้อ Picture เพื่อไปยังหน้าอัปโหลด/แก้ไขรูปภาพของรายการนี้</span>
                        </div>
                    </div>

                    <div class="rm-item">
                        <div class="rm-ico"><span class="glyphicon glyphicon-calendar"></span></div>
                        <div class="rm-txt">
                            <strong>เลือกวันที่</strong>
                            <span>ช่อง Operating Date และ Due Date Calibration กดไอคอนปฏิทินเพื่อเลือกวันที่ (ใช้ได้ในโหมดแก้ไข)</span>
                        </div>
                    </div>

                </div>
            </div>
        </div>
    </div>

</asp:Content>

