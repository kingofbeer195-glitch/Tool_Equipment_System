<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="History_Status.aspx.cs"
    Inherits="Tool_Equipment_Control_System.History_Status" %>

<asp:Content ID="History_Home" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <link href="css/bootstrap_sum.css" rel="stylesheet">
    <link href="CSS/w3v4.10.css" rel="stylesheet" />
    <link href="css/styles.css" rel="stylesheet">
    <link href="DataTables-1.10.18/css/dataTables.css" rel="stylesheet">
    <link href="Content/chosen.css" rel="stylesheet">
    <link href="CSS/fontsgoogleapis.css" rel="stylesheet">
    <link href="CSS/all.min.css">
    <link rel="stylesheet" href="CSS/jquery-ui.css">
    <link href="DateTimePicker/2-bootstrap-datetimepicker.min.css" rel="stylesheet" />
    <link href="DateTimePicker/3-bootstrap-datetimepicker-standalone.css" rel="stylesheet" />

    <script src="Scripts/jquery-1.9.1.js"></script>
    <script src="Scripts/chosen.jquery.js"></script>
    <script src="Scripts/jquery-ui.min.js"></script>
    <script src="Scripts/script.js"></script>
    <script src="DateTimePicker/1-moment.min.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>
    <script src="DateTimePicker/4-bootstrap-datetimepicker.min.js"></script>

    <%--Keep scroll position after async postback--%>
    <script type="text/javascript">
        (function () {
            // ปุ่ม/ตัวกรองที่ทำให้ผลลัพธ์เปลี่ยน -> ให้เริ่มที่บรรทัดแรกตามปกติ
            var resetIds = [
                '<%= btnSearch.ClientID %>', '<%= btnClear.ClientID %>',
                '<%= ddlProcess.ClientID %>', '<%= ddlStatus.ClientID %>', '<%= ddlDept.ClientID %>',
                '<%= btnSearchCL.ClientID %>', '<%= btnClearCL.ClientID %>',
                '<%= btnTabRequest.ClientID %>', '<%= btnTabChangeLocation.ClientID %>'
            ];

            var saved = null;

            function onBegin(sender, args) {
                var el = args.get_postBackElement();
                var id = el ? el.id : '';

                if ($.inArray(id, resetIds) >= 0) { saved = null; return; }

                saved = {
                    winX: window.pageXOffset || document.documentElement.scrollLeft,
                    winY: window.pageYOffset || document.documentElement.scrollTop,
                    boxes: {}
                };
                $('[data-keep-scroll]').each(function () {
                    saved.boxes[this.id] = { top: this.scrollTop, left: this.scrollLeft };
                });
            }

            function onEnd() {
                if (!saved) return;
                var s = saved;

                $('[data-keep-scroll]').each(function () {
                    var b = s.boxes[this.id];
                    if (b) { this.scrollTop = b.top; this.scrollLeft = b.left; }
                });
                window.scrollTo(s.winX, s.winY);

                // เผื่อ Bootstrap modal (Items / Requester) เปิดหลัง endRequest แล้วดันหน้า
                setTimeout(function () {
                    $('[data-keep-scroll]').each(function () {
                        var b = s.boxes[this.id];
                        if (b) { this.scrollTop = b.top; this.scrollLeft = b.left; }
                    });
                    window.scrollTo(s.winX, s.winY);
                }, 0);

                saved = null;
            }

            var prmScroll = Sys.WebForms.PageRequestManager.getInstance();
            prmScroll.add_beginRequest(onBegin);
            prmScroll.add_endRequest(onEnd);
        })();
    </script>
    
    <%--Alert : toast--%>
    <script>    
        (function () {
            function ensureContainer() {
                let c = document.getElementById('toast-container');
                if (!c) {
                    c = document.createElement('div');
                    c.id = 'toast-container';
                    document.body.appendChild(c);
                }
                return c;
            }
            window.showToast = function (message, options = {}) {
                const { title = '', type = 'info', duration = 3000 } = options;
                const container = ensureContainer();
                const toast = document.createElement('div');
                toast.className = `toast ${type}`;
                toast.setAttribute('role', 'status');
                const content = document.createElement('div');
                content.style.display = 'flex';
                content.style.flexDirection = 'column';
                if (title) {
                    const t = document.createElement('div');
                    t.className = 'title';
                    t.textContent = title;
                    content.appendChild(t);
                }
                const msg = document.createElement('div');
                msg.className = 'message';
                msg.textContent = message;
                content.appendChild(msg);
                const closeBtn = document.createElement('button');
                closeBtn.className = 'close';
                closeBtn.innerHTML = '&times;';
                closeBtn.addEventListener('click', () => { container.removeChild(toast); });
                toast.appendChild(content);
                toast.appendChild(closeBtn);
                container.appendChild(toast);
                if (duration > 0) {
                    setTimeout(() => { if (toast.parentNode) container.removeChild(toast); }, duration);
                }
            };
        })();
    </script>

    <%--Alert : toast--%>      
    <style> 
         #toast-container {
             position: fixed;
             top: 16px;
             right: 16px;
             display: flex;
             flex-direction: column;
             gap: 8px;
             z-index: 9999;
         }

         .toast {
             min-width: 260px;
             max-width: 560px;
             padding: 12px 14px;
             border-radius: 8px;
             color: #fff;
             box-shadow: 0 6px 18px rgba(0,0,0,0.15);
             display: flex;
             align-items: start;
             gap: 10px;
             animation: slide-in 200ms ease-out;
             font-family: system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", Arial, "Noto Sans", sans-serif;
             font-size: 16px;
         }

             .toast .title {
                 font-weight: 600;
                 margin-bottom: 6px;
             }

             .toast .message {
                 opacity: 0.95;
             }

             .toast.success {
                 background: #16a34a;
             }
             /* เขียว */
             .toast.error {
                 background: #dc2626;
             }
             /* แดง  */
             .toast.warn {
                 background: #d97706;
             }
             /* ส้ม  */
             .toast.info {
                 background: #2563eb;
             }
             /* น้ำเงิน */

             .toast .close {
                 margin-left: auto;
                 background: transparent;
                 border: none;
                 color: inherit;
                 font-size: 18px;
                 cursor: pointer;
                 line-height: 1;
             }

         @keyframes slide-in {
             from {
                 opacity: 0;
                 transform: translateY(-10px);
             }

             to {
                 opacity: 1;
                 transform: translateY(0);
             }
         }
     </style>

    <%--Close Modal--%>
    <script type="text/javascript">
        function initOverlayOutsideClick() {
            $('.hist-overlay').off('click.overlayClose').on('click.overlayClose', function (e) {
                // ปิดเฉพาะเมื่อคลิกที่ตัว overlay เอง ไม่ใช่คลิกใน .hist-modal
                if (e.target === this) {
                    if (this.id === '<%= pnlHistoryModal.ClientID %>') {
                    document.getElementById('<%= btnHistClose.ClientID %>').click();
                } else if (this.id === '<%= pnlCLHistoryModal.ClientID %>') {
                    document.getElementById('<%= btnCLHistClose.ClientID %>').click();
                }
            }
        });
        }
        $(document).ready(initOverlayOutsideClick);

        var prm3 = Sys.WebForms.PageRequestManager.getInstance();
        prm3.add_endRequest(function () {
            initOverlayOutsideClick();
        });
     </script>

    <%--Date Picker init--%>
    <script type="text/javascript">
        function initDatePickers() {
            try {
                $('#<%= txtDateFrom.ClientID %>').datetimepicker({ format: 'DD-MMM-YYYY' });
                $('#<%= txtDateTo.ClientID %>').datetimepicker({ format: 'DD-MMM-YYYY' });
                $('#<%= txtCLDateFrom.ClientID %>').datetimepicker({ format: 'DD-MMM-YYYY' });
                $('#<%= txtCLDateTo.ClientID %>').datetimepicker({ format: 'DD-MMM-YYYY' });
            } catch (e) { }

            $('.calendarFrom').off('click').on('click', function () {
                $('#<%= txtDateFrom.ClientID %>').focus();
            });
            $('.calendarTo').off('click').on('click', function () {
                $('#<%= txtDateTo.ClientID %>').focus();
            });
            $('.calendarCLFrom').off('click').on('click', function () {
                $('#<%= txtCLDateFrom.ClientID %>').focus();
            });
            $('.calendarCLTo').off('click').on('click', function () {
                $('#<%= txtCLDateTo.ClientID %>').focus();
            });
        }
        $(document).ready(initDatePickers);
        // ป้องกัน datepicker หายหลัง partial postback (UpdatePanel)
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(function () { initDatePickers(); });
    </script>

    <%--Date Style--%>
    <style>
        .input-group-text { cursor: pointer; background-color: #f8f9fa; }
        .date-wrap { display: flex; align-items: center; background-color:white; border-radius: 10px; }
        .date-field { position: relative; width: 100%; }
        .date-input-wrapper { position: relative; }
        .date-input { padding-right: 35px; }
        .calendar-icon {
            position: absolute; right: 5px; top: 50%;
            transform: translateY(-50%); cursor: pointer;
            color: #666; font-size: 14px; opacity:0.6;
        }
    </style>

    <style>
        body { background: #f4f6f9; font-family: 'Segoe UI', sans-serif; }

        /* === Filter Box === */
        .filter-box {
            background: #fff; border-radius: 8px;
            padding: 8px 20px; margin-bottom: 5px;
            box-shadow: 0 1px 4px rgba(0,0,0,.08);
        }
        .filter-box .form-group { margin-bottom: 10px; }
        .filter-box label { font-size: 12px; color: #555; font-weight: 600; }

        /* === GridView === */
        .grid-wrap {
            background: #fff; border-radius: 8px;
            padding: 12px; box-shadow: 0 1px 4px rgba(0,0,0,.08);
        }
        .grid-title { font-size: 14px; font-weight: 700; color: #1a3a5c; margin-bottom: 4px; }

        .no-data { text-align:center; color:#aaa; padding:30px; font-size:14px; }
        .ui-autocomplete { z-index: 9999 !important; font-size: 13px; }
    </style>

    <%--Header Topic--%>
    <style>
        .grid-topic {
            display: grid; grid-template-columns: 60px auto; grid-template-rows: 100%;
            background-color: #94c2ff; border-radius: 5px;
        }
        .grid-PictureTopic { align-self: center; text-align: center; }
        .PictureTopic { height: 40px; }
        .fontTopic { font-size: 1.6em; font-weight: bold; color: #605d5d; align-content: center; }
        .headerGv1 {
            color: #ffffff; font-weight: 800; background-color: #1A3A5C;
            padding: 2px 4px; text-align:center; font-size:13px;
        }
        .rowGv1 { font-size: 13px; padding: 2px 4px; background-color: #ffffff; text-align:center; }
        .fixed-header th { position: sticky; top: 0; z-index: 3; border-bottom: 2px solid #ccc; }
        .btn {
            border: none; border-radius: 8px; padding: 2px 8px;
            font-size: 12px; font-weight: 600; cursor: pointer;
            transition: all 0.18s;
            /*width: 90px !important;*/
        }
        .btn-primary { background: #2980b9; color: #fff; }
        .btn-primary:hover { background: #1a6a9a; }
        .btn-info { background: #16a085; color: #fff; }
        .btn-info:hover { background: #117a65; }
        .btn-warning { background: #f39c12; color: #fff; }
        .btn-warning:hover { background: #d68910; }
        .btn-danger { background: #e74c3c; color: #fff; }
        .btn-danger:hover { background: #c0392b; }
        .btn-secondary { background: #7f8c8d; color: #fff; }
        .btn-secondary:hover { background: #626567; }
        .btn-link { background: transparent; color: #1a6a9a; width:auto !important; }
        .btn-sm { padding: 4px 10px; height: 25px !important; font-size: 11px; }
        .btn:disabled { opacity: 0.5; cursor: not-allowed; }

        .status-badge { display: inline-block; padding: 4px 12px; border-radius: 12px; font-size: 12px; font-weight: 700; letter-spacing: 0.5px; }
        .status-draft { background: #fef9e7; color: #b7950b; border: 1px solid #f0d060;box-shadow: 3px 3px 4px; }
        .status-approve { background: #eaf6fb; color: #1a6a9a; border: 1px solid #aad4ea;box-shadow: 3px 3px 4px; }
        .status-confirm { background: #f6e8f9; color: #b42fad; border: 1px solid #c6a0d2;box-shadow: 3px 3px 4px; }
        .status-completed { background: #d9f0de; color: #289b52; border: 1px solid #59a461;box-shadow: 3px 3px 4px; }
        .status-cancel { background: #e7e7e7; color: #4c4c4c; border: 1px solid #636363;box-shadow: 3px 3px 4px; }
        .status-borrow { background: #bebbdc; color: #474187; border: 1px solid #4b3ecf;box-shadow: 3px 3px 4px; }
        .label, .badge {text-shadow : none; font-size:13px;}
        .tbl-wrap { overflow-x: auto; overflow-y: auto; position: relative; }
    </style>

    <%--Card / Filter--%>
    <style>
        .card { background: #fff; border-radius: 8px; box-shadow: 0 1px 6px rgba(0,0,0,0.08); margin-bottom: 16px; overflow: hidden; }
        .card-header {
            background: #f0f4f8; border-bottom: 2px solid #d0dbe8;
            padding: 5px 20px 5px 10px; display: flex; align-items: center; gap: 8px;
            font-weight: 600; font-size: 14px; color: #1a3a5c; height: 40px;
        }
        .card-body { padding: 10px 10px 0px 10px; }
        .form-row { display: flex; flex-wrap: wrap; gap: 12px; align-items: flex-end; }
        .form-group { display: flex; flex-direction: column; margin-bottom:5px; }
        .form-group label {
            font-size: 12px; font-weight: 600; color: #555;
            text-transform: uppercase; letter-spacing: 0.4px;
            margin-left:5px; margin-bottom:-5px; height:15px;
        }
        .form-group input[type="text"], .form-group select {
            border: 1.5px solid #c8d6e5; border-radius: 5px; padding: 3px 10px;
            font-size: 14px; color: #2c3e50; background: #fff;
            transition: border-color 0.2s; height: 34px; text-align:center;
        }
        .form-group input:focus, .form-group select:focus {
            outline: none; border-color: #2980b9; box-shadow: 0 0 0 3px rgba(41,128,185,0.12);
        }
    </style>

    <%--Tabs--%>
    <style>
        .tab-nav { display:flex; gap:4px; margin: 0 10px 0px 10px; }
        .tab-btn {
            padding:8px 22px; border:none; background:#dbe6f0; color:#1a3a5c;
            font-weight:700; font-size:13px; border-radius:8px 8px 0 0; cursor:pointer;
            text-decoration:none; display:inline-block;
        }
        .tab-btn:hover { background:#c8d9e8; text-decoration:none; color:#1a3a5c; }
        .tab-btn.active { background:#1a3a5c; color:#fff; }
        .tab-btn.active:hover { background:#1a3a5c; color:#fff; }
    </style>

    <%--Change compare highlight--%>
    <style>
        .chg-old { color:#999; text-decoration:line-through; font-size:11px; }
        .chg-new { color:#1a6a9a; font-weight:700; }
        .chg-arrow { color:#f39c12; font-weight:700; margin:0 3px; }
        .chg-badge-new { background:#d9f0de; color:#289b52; border:1px solid #59a461; padding:1px 8px; border-radius:10px; font-size:11px; font-weight:700; }
    </style>

    <%--Modal Timeline--%>
    <style>
        .hist-overlay { position: fixed; inset: 0; background: rgba(0,0,0,.5); z-index: 9998; display: flex; align-items: center; justify-content: center; }
        .hist-modal { background: #fff; width: 540px; max-width: 92%; max-height: 80vh; border-radius: 10px; overflow: hidden; box-shadow: 0 8px 30px rgba(0,0,0,.3); display: flex; flex-direction: column; }
        .hist-modal-header { background: #1a3a5c; color: #fff; padding: 12px 18px; font-size: 15px; font-weight: 700; display: flex; justify-content: space-between; align-items: center; }
        .hist-close { color: #fff; font-size: 22px; text-decoration: none; line-height: 1; }
        .hist-close:hover { color: #ff8a8a; }
        .hist-modal-body { padding: 20px 24px; overflow-y: auto; }
        .timeline { list-style: none; margin: 0; padding: 0; position: relative; }
        .timeline:before { content: ''; position: absolute; left: 15px; top: 6px; bottom: 6px; width: 2px; background: #d0dbe8; }
        .tl-item { position: relative; padding: 0 0 22px 46px; }
        .tl-item:last-child { padding-bottom: 0; }
        .tl-dot { position: absolute; left: 6px; top: 2px; width: 20px; height: 20px; border-radius: 50%; background: #2980b9; border: 3px solid #fff; box-shadow: 0 0 0 2px #2980b9; z-index: 1; }
        .tl-dot.done { background: #27ae60; box-shadow: 0 0 0 2px #27ae60; }
        .tl-dot.cancel { background: #e74c3c; box-shadow: 0 0 0 2px #e74c3c; }
        .tl-dot.wait { background: #bbb; box-shadow: 0 0 0 2px #bbb; }
        .tl-title { font-size: 14px; font-weight: 700; color: #1a3a5c; }
        .tl-meta { font-size: 12px; color: #666; margin-top: 2px; }
        .tl-date { font-size: 12px; color: #2980b9; font-weight: 600; }
        .tl-pending .tl-title { color: #999; font-weight: 600; }
    </style>

    <%--------------------------------%>
    <%--========= New Style ========--%>
    <%--------------------------------%>

    <%-- History Request Table : ชุดเดียวกับ ApproveStatus / EquipmentStatus --%>
    <style>
        .hs-grid {
            --ink: #1c2b3a; --ink-2: #33475b; --muted: #66727f;
            --paper: #f3f5f8; --surface: #ffffff;
            --line: #dde3ea; --line-soft: #edf0f4;
            --focus: #1f6feb; --approve: #16884a;
            --r-sm: 6px; --r-md: 10px;

            font-family: 'IBM Plex Sans Thai', 'Segoe UI', Tahoma, sans-serif;
            color: var(--ink);
            background: var(--surface);
            border: 1px solid var(--line);
            border-radius: var(--r-md);
            margin-top: -10px;
        }
        .hs-grid *, .hs-grid *::before, .hs-grid *::after { box-sizing: border-box; }

        /* หัวตาราง + จำนวน */
        .hs-grid .hs-head {
            display: flex; align-items: center; justify-content: space-between;
            padding: 8px 18px; border-bottom: 1px solid var(--line-soft);
        }
        .hs-grid .hs-head h2 { margin: 0; font-size: 15px; font-weight: 700; color: var(--ink); }
        .hs-grid .hs-count {
            display: inline-block; margin-left: 6px;
            padding: 1px 9px; border-radius: 999px;
            background: var(--approve); color: var(--body-color, #fff);
            font-size: 12px; font-weight: 700; font-variant-numeric: tabular-nums;
        }

        .hs-grid .hs-tbl-wrap { max-height: calc(100vh - 410px); overflow: auto; position: relative; margin: 0 10px 10px; }

        /* Table */
        .hs-grid .gv { width: 100%; min-width: 1320px; border-collapse: separate; border-spacing: 0; font-size: 13.5px; }
        .hs-grid .gv th {
            position: sticky; top: 0; z-index: 2;
            background: #1a3a5c; color: #fff;
            font-size: 12.5px; font-weight: 700; text-align: left;
            padding: 10px 12px; white-space: nowrap;
            border-bottom: 1px solid var(--line);
        }
        .hs-grid .gv td {
            padding: 10px 12px; vertical-align: middle;
            border-bottom: 1px solid var(--line-soft);
            background: var(--surface);
        }
        .hs-grid .gv tr.alt-row td { background: #eaf0f7; }
        .hs-grid .gv tr:hover td   { background: #d6e5f8; }

        .hs-grid .gv .c-center   { text-align: center; }
        .hs-grid .gv .c-no       { color: var(--muted); font-variant-numeric: tabular-nums; }
        .hs-grid .gv .c-doc      { font-weight: 700; font-variant-numeric: tabular-nums; white-space: nowrap; }
        .hs-grid .gv .c-date     { white-space: nowrap; font-variant-numeric: tabular-nums; color: var(--ink-2); }
        .hs-grid .gv .c-progress { font-size: 12.5px; color: var(--ink-2); line-height: 1.4; }
        .hs-grid .gv .req-comment { display: block; font-size: 12px; color: var(--muted); margin-top: 2px; }

        /* Action : ติดซ้ายสุด */
        .hs-grid .gv th.c-action, .hs-grid .gv td.c-action {
            position: sticky; left: 0; text-align: center; white-space: nowrap;
        }
        .hs-grid .gv th.c-action { z-index: 3; border-right: 1px solid #0f2740; }
        .hs-grid .gv td.c-action {
            z-index: 1; border-right: 1px solid var(--line);
            box-shadow: 6px 0 8px -6px rgba(28,43,58,.22);
        }

        .hs-grid .no-data { text-align: center; color: var(--muted); padding: 48px 16px; font-size: 14px; }

        /* Status pill */
        .hs-grid .gv .badge {
            display: inline-flex; align-items: center; gap: 6px;
            padding: 3px 10px 3px 8px; border-radius: 999px;
            font-size: 12px; font-weight: 700; line-height: 1.5;
            white-space: nowrap; text-shadow: none; box-shadow: none;
            background: #eef1f4; color: #4b5663; border: 1px solid #d8dee5;
        }
        .hs-grid .gv .badge::before { content: ''; width: 7px; height: 7px; border-radius: 50%; background: currentColor; }
        .hs-grid .gv .badge.status-draft     { background: #fff7e0; color: #946200; border-color: #f1d98c; }
        .hs-grid .gv .badge.status-approve   { background: #e8f0fe; color: #1a5bc4; border-color: #bcd1f7; }
        .hs-grid .gv .badge.status-confirm   { background: #f5e9f8; color: #92278f; border-color: #dfc0e6; }
        .hs-grid .gv .badge.status-borrow    { background: #ecebfb; color: #4a3fb0; border-color: #cdc8f1; }
        .hs-grid .gv .badge.status-completed { background: #e3f4e8; color: #16703d; border-color: #b6e0c4; }
        .hs-grid .gv .badge.status-cancel    { background: #f0f1f3; color: #6b7480; border-color: #d9dde2; }

        /* Buttons */
        .hs-grid .act-btn {
            display: inline-flex; align-items: center; justify-content: center;
            min-width: 96px; height: 32px; padding: 0 12px; margin: 0 2px;
            font-family: inherit; font-size: 13px; font-weight: 700;
            border-radius: var(--r-sm); cursor: pointer; text-shadow: none;
            transition: background .15s, border-color .15s;
        }
        .hs-grid .act-view { background: var(--surface); color: var(--ink-2); border: 1px solid #c9d2dc; }
        .hs-grid .act-view:hover { background: var(--paper); color: var(--ink); border-color: #9aa8b6; }

        .hs-grid .chip-btn {
            display: inline-flex; align-items: center; gap: 4px;
            padding: 3px 10px; border-radius: 999px;
            background: var(--paper); border: 1px solid var(--line);
            color: var(--ink-2); font-size: 13px; font-weight: 700;
            font-variant-numeric: tabular-nums; text-decoration: none;
        }
        .hs-grid .chip-btn:hover { border-color: #9aa8b6; color: var(--ink); text-decoration: none; }

        .hs-grid .icon-btn {
            width: 32px; height: 32px; padding: 0;
            border: 1px solid transparent; border-radius: var(--r-sm);
            background: transparent; font-size: 16px; cursor: pointer;
        }
        .hs-grid .icon-btn:hover { background: var(--paper); border-color: var(--line); }

        .hs-grid .link-btn { color: #1f6feb; font-weight: 600; text-decoration: none; font-variant-numeric: tabular-nums; }
        .hs-grid .link-btn:hover { text-decoration: underline; }

        .hs-grid a:focus-visible, .hs-grid input[type=submit]:focus-visible { outline: 2px solid var(--focus); outline-offset: 2px; }
    </style>

    <%-- Modals : pnlHistoryModal / modal_ItemsList / modal_Issuer --%>
    <style>
        /* ---------- History Timeline (asp:Panel) ---------- */
        .hs-hist.hist-overlay {
            position: fixed; inset: 0; z-index: 9998;
            background: rgba(20,30,42,.45);
            display: flex; align-items: center; justify-content: center;
        }
        .hs-hist .hist-modal {
            width: 560px; max-width: 94%; max-height: 82vh;
            display: flex; flex-direction: column; overflow: hidden;
            background: #fff; border-radius: 12px;
            box-shadow: 0 20px 50px rgba(20,30,42,.3);
            font-family: 'IBM Plex Sans Thai', 'Segoe UI', Tahoma, sans-serif;
        }
        .hs-hist .hist-modal-header {
            display: flex; justify-content: space-between; align-items: center;
            padding: 14px 20px; background: #fff; border-bottom: 1px solid #edf0f4;
            font-size: 15px; font-weight: 700; color: #1c2b3a;
        }
        .hs-hist .hist-modal-header .doc { color: #1f6feb; font-variant-numeric: tabular-nums; }
        .hs-hist .hist-close {
            width: 32px; height: 32px; border-radius: 6px;
            display: inline-flex; align-items: center; justify-content: center;
            color: #66727f; font-size: 22px; text-decoration: none;
        }
        .hs-hist .hist-close:hover { background: #f3f5f8; color: #1c2b3a; text-decoration: none; }
        .hs-hist .hist-modal-body { padding: 20px 24px; overflow-y: auto; }

        .hs-hist .timeline { list-style: none; margin: 0; padding: 0; position: relative; }
        .hs-hist .timeline:before {
            content: ''; position: absolute; left: 11px; top: 8px; bottom: 8px;
            width: 2px; background: #dde3ea;
        }
        .hs-hist .tl-item { position: relative; padding: 0 0 20px 38px; }
        .hs-hist .tl-item:last-child { padding-bottom: 0; }
        .hs-hist .tl-dot {
            position: absolute; left: 4px; top: 3px;
            width: 16px; height: 16px; border-radius: 50%;
            background: #1f6feb; border: 3px solid #fff; box-shadow: 0 0 0 2px #1f6feb; z-index: 1;
        }
        .hs-hist .tl-dot.done   { background: #16884a; box-shadow: 0 0 0 2px #16884a; }
        .hs-hist .tl-dot.cancel { background: #c93b3b; box-shadow: 0 0 0 2px #c93b3b; }
        .hs-hist .tl-dot.wait   { background: #fff;    box-shadow: 0 0 0 2px #b8c2cc; }
        .hs-hist .tl-title { font-size: 14px; font-weight: 700; color: #1c2b3a; }
        .hs-hist .tl-meta  { font-size: 12.5px; color: #33475b; margin-top: 2px; }
        .hs-hist .tl-date  { font-size: 12px; color: #66727f; font-weight: 400; margin-top: 2px; font-variant-numeric: tabular-nums; }
        .hs-hist .tl-pending .tl-title { color: #8a95a1; font-weight: 600; }
        .hs-hist .tl-pending .tl-meta  { color: #8a95a1; }

        /* ---------- Bootstrap modal (Items / Requester) ---------- */
        .aps-modal.modal {
            position: fixed; top: 0 !important; left: 0; right: 0; bottom: 0;
            width: auto; margin: 0;
            background: none; border: none; box-shadow: none;
            overflow-y: auto;
        }
        .aps-modal .modal-dialog { width: 860px; max-width: 94%; margin: 60px auto; }
        .aps-modal.modal-sm-w .modal-dialog { width: 520px; }
        .aps-modal .modal-content {
            border: none; border-radius: 12px; overflow: hidden;
            box-shadow: 0 20px 50px rgba(20,30,42,.3);
            font-family: 'IBM Plex Sans Thai', 'Segoe UI', Tahoma, sans-serif;
        }
        .aps-modal .modal-header {
            display: flex; align-items: center; justify-content: space-between;
            padding: 14px 20px; background: #fff; border-bottom: 1px solid #edf0f4;
        }
        .aps-modal .modal-title { margin: 0; font-size: 15px; font-weight: 700; color: #1c2b3a; }
        .aps-modal .modal-title .doc { color: #1f6feb; }
        .aps-modal .modal-header .close { order: 2; opacity: .6; font-size: 24px; text-shadow: none; color: #1c2b3a; }
        .aps-modal .modal-header .close:hover { opacity: 1; }
        .aps-modal .modal-body { max-height: 68vh; overflow-y: auto; padding: 16px 20px; }
        .aps-modal .modal-footer { padding: 12px 20px; border-top: 1px solid #edf0f4; text-align: right; }

        .aps-modal .tbl { width: 100%; border-collapse: collapse; font-size: 13.5px; }
        .aps-modal .tbl th {
            background: #f7f9fb; color: #33475b; font-size: 12.5px; font-weight: 700;
            padding: 9px 10px; border-bottom: 1px solid #dde3ea; text-align: center;
        }
        .aps-modal .tbl td { padding: 8px 10px; border-bottom: 1px solid #edf0f4; text-align: center; vertical-align: middle; }
        .aps-modal .tbl img { max-width: 140px; max-height: 110px; border-radius: 6px; background: #f3f5f8; }

        .aps-modal .kv { width: 100%; border-collapse: collapse; font-size: 14px; }
        .aps-modal .kv th {
            width: 32%; padding: 9px 12px; text-align: left;
            font-size: 12.5px; font-weight: 600; color: #66727f;
            border-bottom: 1px solid #edf0f4; background: none;
        }
        .aps-modal .kv td { padding: 9px 12px; color: #1c2b3a; font-weight: 600; border-bottom: 1px solid #edf0f4; }

        .aps-modal .btn-close-foot {
            height: 34px; padding: 0 18px; border-radius: 6px;
            background: #fff; border: 1px solid #c9d2dc; color: #33475b;
            font-weight: 600; cursor: pointer;
        }
        .aps-modal .btn-close-foot:hover { background: #f3f5f8; }
    </style>

    <%-- History Change Location : Old / New diff --%>
    <style>
        .hs-grid .gv.cl-gv { min-width: 1150px; }
        .hs-grid .gv.cl-gv td { vertical-align: top; }
        .hs-grid .hs-head .cl-legend { display:flex; align-items:center; gap:6px; font-size:12px; color:#66727f; }
        .hs-grid .hs-head .cl-legend .cl-tag { margin-left:8px; }

        /* แท็ก เดิม / ใหม่ / คงเดิม */
        .cl-tag { display:inline-block; min-width:44px; text-align:center; padding:1px 6px; border-radius:4px; font-size:11px; font-weight:700; line-height:18px; }
        .cl-tag-old  { background:#fdecec; color:#b42318; }
        .cl-tag-new  { background:#e3f4e8; color:#16703d; }
        .cl-tag-same { background:#eef1f4; color:#66727f; }

        /* อุปกรณ์ FA / CT / TL */
        .cl-asset { font-size:12.5px; line-height:1.6; }
        .cl-asset .k { display:inline-block; width:26px; font-size:11px; font-weight:700; color:#8a95a1; }
        .cl-asset .v { font-weight:600; font-variant-numeric: tabular-nums; }

        /* Location path */
        .cl-loc  { display:grid; grid-template-columns:52px 1fr; gap:5px 8px; align-items:center; }
        .cl-path { display:flex; flex-wrap:wrap; align-items:center; gap:3px; }
        .cl-seg  { padding:1px 7px; border-radius:4px; border:1px solid #e3e8ee; background:#f7f9fb; color:#33475b; font-size:12.5px; white-space:nowrap; }
        .cl-seg.is-old   { background:#fdecec; border-color:#f5c2c0; color:#b42318; text-decoration:line-through; text-decoration-color:rgba(180,35,24,.45); }
        .cl-seg.is-new   { background:#e3f4e8; border-color:#9fd3b0; color:#16703d; font-weight:700; }
        .cl-seg.is-empty { font-style:italic; color:#9aa4ae; background:#fff; border-style:dashed; }
        .cl-sep { color:#b8c2cc; font-size:12px; }

        /* ฟิลด์อื่นที่เปลี่ยน */
        .cl-fd-row { display:grid; grid-template-columns:84px 1fr; gap:8px; padding:3px 0; border-bottom:1px dashed #e6eaef; font-size:12.5px; }
        .cl-fd-row:last-child { border-bottom:none; }
        .cl-fd-k { color:#66727f; font-weight:600; }
        .cl-fd-v { display:flex; flex-wrap:wrap; align-items:center; gap:4px; }
        .cl-old  { color:#b42318; background:#fdecec; padding:0 6px; border-radius:4px; text-decoration:line-through; text-decoration-color:rgba(180,35,24,.45); }
        .cl-new  { color:#16703d; background:#e3f4e8; padding:0 6px; border-radius:4px; font-weight:700; }
        .cl-arrow { color:#d97706; font-weight:700; }
        .cl-add   { font-size:11px; font-weight:700; color:#16703d; }
        .cl-empty { font-style:italic; color:#9aa4ae; }
        .cl-none  { color:#b8c2cc; }
        .cl-muted { color:#b8c2cc; }
        .cl-note  { margin-top:6px; font-size:12px; color:#33475b; background:#fffbea; border-left:3px solid #f1d98c; padding:3px 8px; border-radius:0 4px 4px 0; }
        .cl-time  { color:#8a95a1; font-size:12px; }

        /* Modal timeline */
        .hs-hist .hist-modal.cl-modal { width:760px; }
        .hs-hist .cl-tl-box { margin-top:8px; padding:10px 12px; background:#fafbfc; border:1px solid #edf0f4; border-radius:8px; }
        .hs-hist .cl-sub { font-size:11px; font-weight:700; color:#8a95a1; text-transform:uppercase; letter-spacing:.4px; margin:8px 0 4px; }
        .hs-hist .cl-sub:first-child { margin-top:0; }
    </style>

    <asp:ScriptManager runat="server" ID="scriptManager" EnablePageMethods="true"></asp:ScriptManager>

    <asp:UpdatePanel runat="server" ID="UP1">
        <ContentTemplate>

            <%---- Toast Container ----%>
            <div id="toast-container" aria-live="polite" aria-atomic="true"></div>

            <%-- Header Topic --%>
            <div class="grid-topic" style="margin: 30px 10px 10px 10px; height:60px; background:#1a3a5c;">
                <div class="grid-PictureTopic">
                    <img src="images/Toolhistory.png" class="PictureTopic" />
                </div>
                <div class="fontTopic" style="color:#fff;">
                    History — ประวัติการควบคุมอุปกรณ์ [Jig Tool &amp; Equipment]
                </div>
            </div>

            <%-- ===== Tab Nav ===== --%>
            <div class="tab-nav">
                <asp:LinkButton ID="btnTabRequest" runat="server" CssClass="tab-btn active" OnClick="btnTabRequest_Click">📋 History Request</asp:LinkButton>
                <asp:LinkButton ID="btnTabChangeLocation" runat="server" CssClass="tab-btn" OnClick="btnTabChangeLocation_Click">📍 History Change Location</asp:LinkButton>
            </div>

            <%--=====================================================--%>
            <%------------ TAB 1 : History Request ---------------------%>
            <%--=====================================================--%>
            <asp:Panel ID="pnlTabRequest" runat="server">
            <div class="card" style="margin-left:10px; margin-right:10px; border-top-left-radius:0;">
                <div class="card-header">
                    <span> • History List</span>
                </div>

                <div class="card-body" style="height: calc(100vh - 225px);">

                    <!-- ===== Filter Box ===== -->
                    <div class="filter-box">
                        <div class="row">
                            <div class="form-row" style="margin-left:5px;">

                                <%--Keyword : FixAsset / ControlNo--%>
                                <div class="form-group" style="width:240px;">
                                    <label style="margin-bottom: 0px;">🔍 ค้นหารายการ</label>
                                    <asp:TextBox ID="txtSearchRequest" runat="server"
                                        CssClass="form-control" 
                                        placeholder="Fix Asset / Control No / Tool No"
                                        AutoComplete="off"
                                        Style="text-align:center; margin:0px;" />
                                </div>

                                <%--Search EmpId--%>
                                <div class="form-group" style="width:160px;">
                                    <label style="margin-bottom: 0px;">👤 Requester</label>
                                    <asp:TextBox ID="txtSearchEmpId" runat="server"
                                        CssClass="form-control" 
                                        placeholder="รหัสพนักงาน"
                                        AutoComplete="off"
                                        Style="text-align:center; margin:0px;" />
                                </div>

                                <%--Request Type--%>
                                <div class="form-group">
                                    <label style="margin-bottom: 0px;">⚙️ REQUEST TYPE</label>
                                    <asp:DropDownList ID="ddlProcess" runat="server"
                                        CssClass="form-control" AutoPostBack="true"
                                        OnSelectedIndexChanged="ddlProcess_SelectedIndexChanged">
                                    </asp:DropDownList>
                                </div>

                                <%--Status--%>
                                <div class="form-group">
                                    <label style="margin-bottom: 0px;">📌 STATUS</label>
                                    <asp:DropDownList ID="ddlStatus" runat="server"
                                        CssClass="form-control" AutoPostBack="true"
                                        OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged">
                                    </asp:DropDownList>
                                </div>

                                <%--Dept--%>
                                <div class="form-group">
                                    <label style="margin-bottom: 0px;">👥 DEPT</label>
                                    <asp:DropDownList ID="ddlDept" runat="server"
                                        CssClass="form-control" AutoPostBack="true"
                                        OnSelectedIndexChanged="ddlDept_SelectedIndexChanged">
                                    </asp:DropDownList>
                                </div>

                                <%--Date From--%>
                                <div class="form-group" style="width:150px;">
                                    <label style="margin-bottom:0px">📅 FROM (Issue Date)</label>
                                    <div class="date-input-wrapper">
                                        <asp:TextBox ID="txtDateFrom" runat="server"
                                            CssClass="form-control date-input"
                                            Style="text-align:center; margin: 0px;" AutoComplete="off" />
                                        <span class="calendar-icon calendarFrom">📅</span>
                                    </div>
                                </div>

                                <%--Date To--%>
                                <div class="form-group" style="width:150px;">
                                    <label style="margin-bottom:0px">📅 TO (Issue Date)</label>
                                    <div class="date-input-wrapper">
                                        <asp:TextBox ID="txtDateTo" runat="server"
                                            CssClass="form-control date-input"
                                            Style="text-align:center; margin: 0px;" AutoComplete="off" />
                                        <span class="calendar-icon calendarTo">📅</span>
                                    </div>
                                </div>

                                <%--Search Button--%>
                                <div class="form-group">
                                    <label style="margin-bottom:0px"></label>
                                    <asp:Button ID="btnSearch" runat="server" Text="🔍 ค้นหา"
                                        CssClass="btn btn-primary"
                                        Style="width:120px !important; margin-left:15px; box-shadow: 3px 3px 4px #aaa;"
                                        OnClick="btnSearch_Click" />
                                </div>

                                <%--Clear Filter Button--%>
                                <div class="form-group">
                                    <label style="margin-bottom:0px;"></label>
                                    <asp:LinkButton ID="btnClear" runat="server" OnClick="btnClear_Click"
                                        CssClass="btn btn-danger"
                                        Style="width:120px !important; align-content:center; margin-left:15px; box-shadow: 3px 3px 4px #aaa;">
                                        <span class="glyphicon glyphicon-filter"></span> ล้างค่าค้นหา
                                    </asp:LinkButton>
                                </div>

                            </div>
                        </div>
                    </div>

                    <!-- ===== GridView : รายการประวัติ ===== -->
                    <asp:UpdatePanel ID="upGrid" runat="server" UpdateMode="Always">
                        <ContentTemplate>
                            <div class="hs-grid">

                                <div class="hs-head">
                                    <h2>รายการประวัติ<span class="hs-count"><asp:Label ID="lblTotal" runat="server" Text="0" /></span></h2>
                                </div>

                                <div class="hs-tbl-wrap" id="wrapReq" data-keep-scroll="1">
                                    <asp:GridView ID="gvHistoryList" runat="server"
                                        CssClass="gv" GridLines="None" CellSpacing="0"
                                        AutoGenerateColumns="False"
                                        AlternatingRowStyle-CssClass="alt-row"
                                        OnRowDataBound="gvHistoryList_RowDataBound"
                                        OnRowCommand="gvHistoryList_RowCommand"
                                        EmptyDataText="<div class='no-data'>ไม่พบประวัติที่ตรงกับตัวกรองนี้ — ลองขยายช่วงวันที่หรือล้างตัวกรอง</div>">

                                        <Columns>

                                            <%-- Action : ติดซ้ายสุด --%>
                                            <asp:TemplateField HeaderText="ดำเนินการ" HeaderStyle-CssClass="c-action" ItemStyle-CssClass="c-action">
                                                <ItemTemplate>
                                                    <asp:Button ID="btnView" runat="server" Text="เปิดดู"
                                                        CommandName="ViewRow" CommandArgument='<%# Eval("Doc_No") %>'
                                                        CssClass="act-btn act-view" />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- No --%>
                                            <asp:TemplateField HeaderText="#" HeaderStyle-CssClass="c-center" ItemStyle-CssClass="c-center c-no" ItemStyle-Width="44px">
                                                <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- Request No --%>
                                            <asp:TemplateField HeaderText="Request No" ItemStyle-CssClass="c-doc">
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="lb_DocNo" Text='<%# Eval("Doc_No") %>' />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- Request Type --%>
                                            <asp:TemplateField HeaderText="ประเภทคำขอ" ItemStyle-Width="220px">
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="lb_requestType" Text='<%# Eval("document_name") %>' />
                                                    <asp:Label runat="server" ID="lb_requestComment" CssClass="req-comment" Text='<%# Eval("Comment") %>' />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- Status --%>
                                            <asp:TemplateField HeaderText="สถานะ">
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="lb_status" Text='<%# Eval("status_name") %>'
                                                        CssClass='<%# "badge " + GetStatusClass(Eval("status_name").ToString()) %>' />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- Progress --%>
                                            <asp:TemplateField HeaderText="ขั้นตอน" ItemStyle-CssClass="c-progress" ItemStyle-Width="220px">
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="lb_detail" Text='<%# Eval("detail") %>' />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- Items --%>
                                            <asp:TemplateField HeaderText="รายการ" HeaderStyle-CssClass="c-center" ItemStyle-CssClass="c-center">
                                                <ItemTemplate>
                                                    <asp:LinkButton runat="server" ID="btn_Items"
                                                        CommandName="ViewItems" CommandArgument='<%# Eval("Doc_No") %>'
                                                        CssClass="chip-btn" ToolTip="ดูรายการอุปกรณ์"
                                                        Text='<%# "📦 " + Eval("TotalCount") %>' />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- Timeline --%>
                                            <asp:TemplateField HeaderText="ประวัติ" HeaderStyle-CssClass="c-center" ItemStyle-CssClass="c-center">
                                                <ItemTemplate>
                                                    <asp:Button ID="btnHistory" runat="server" Text="🕓"
                                                        CommandName="HistoryRow" CommandArgument='<%# Eval("Doc_No") %>'
                                                        CssClass="icon-btn" ToolTip="ดู Timeline" />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- Requester --%>
                                            <asp:TemplateField HeaderText="ผู้ขอ">
                                                <ItemTemplate>
                                                    <asp:LinkButton runat="server" ID="btnIssuer"
                                                        CommandName="ViewIssuer"
                                                        CommandArgument='<%# Eval("issue_empid") + "|" + Eval("Doc_No") %>'
                                                        CssClass="link-btn" ToolTip="ดูข้อมูลผู้ขอ"
                                                        Text='<%# Eval("issue_empid") %>' />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- Dept --%>
                                            <asp:TemplateField HeaderText="แผนก">
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="lb_deptcode" Text='<%# Eval("dept_code") %>' />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- Issue Date --%>
                                            <asp:TemplateField HeaderText="วันที่ขอ" ItemStyle-CssClass="c-date">
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="lb_issueDate" Text='<%# Eval("issue_date", "{0:dd-MMM-yyyy}") %>' />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                        </Columns>
                                    </asp:GridView>
                                </div>

                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
            </asp:Panel>

            <%--=====================================================--%>
            <%------------ TAB 2 : History Change Location -----------%>
            <%--=====================================================--%>
            <asp:Panel ID="pnlTabChangeLocation" runat="server" Visible="false">
            <div class="card" style="margin-left:10px; margin-right:10px; border-top-left-radius:0; height: calc(100vh - 185px);">
                <div class="card-header">
                    <span> • History Change Location List</span>
                </div>

                <div class="card-body">

                    <!-- ===== Filter Box ===== -->
                    <div class="filter-box">
                        <div class="row">
                            <div class="form-row" style="margin-left:5px;">

                                <%--Search text--%>
                                <div class="form-group" style="width:310px;">
                                    <label style="margin-bottom:0px;">🔎 SEARCH (Fix Asset / Control / Tool No)</label>
                                    <asp:TextBox ID="txtSearchCL" runat="server" CssClass="form-control" placeholder="ค้นหา..." />
                                </div>

                                <%--Edit By--%>
                                <div class="form-group" style="width:150px;">
                                    <label style="margin-bottom:0px;">✏️ EDIT BY</label>
                                    <asp:TextBox ID="txtEditBy" runat="server" CssClass="form-control" placeholder="Emp ID" />
                                </div>

                                <%--Date From--%>
                                <div class="form-group" style="width:150px;">
                                    <label style="margin-bottom:0px">📅 FROM (Edit Date)</label>
                                    <div class="date-input-wrapper">
                                        <asp:TextBox ID="txtCLDateFrom" runat="server"
                                            CssClass="form-control date-input"
                                            Style="text-align:center;" AutoComplete="off" />
                                        <span class="calendar-icon calendarCLFrom">📅</span>
                                    </div>
                                </div>

                                <%--Date To--%>
                                <div class="form-group" style="width:150px;">
                                    <label style="margin-bottom:0px">📅 TO (Edit Date)</label>
                                    <div class="date-input-wrapper">
                                        <asp:TextBox ID="txtCLDateTo" runat="server"
                                            CssClass="form-control date-input"
                                            Style="text-align:center;" AutoComplete="off" />
                                        <span class="calendar-icon calendarCLTo">📅</span>
                                    </div>
                                </div>

                                <%--Search Button--%>
                                <div class="form-group">
                                    <label style="margin-bottom:0px"></label>
                                    <asp:Button ID="btnSearchCL" runat="server" Text="🔍 ค้นหา"
                                        CssClass="btn btn-primary"
                                        Style="width:120px !important; margin-left:15px; box-shadow: 3px 3px 4px #aaa;"
                                        OnClick="btnSearchCL_Click" />
                                </div>

                                <%--Clear Filter Button--%>
                                <div class="form-group">
                                    <label style="margin-bottom:0px;"></label>
                                    <asp:LinkButton ID="btnClearCL" runat="server" OnClick="btnClearCL_Click"
                                        CssClass="btn btn-danger"
                                        Style="width:120px !important; align-content:center; margin-left:15px; box-shadow: 3px 3px 4px #aaa;">
                                        <span class="glyphicon glyphicon-filter"></span> ล้างค่าค้นหา
                                    </asp:LinkButton>
                                </div>

                            </div>
                        </div>
                    </div>

                    <!-- ===== GridView ===== -->
                    <asp:UpdatePanel ID="upGridCL" runat="server" UpdateMode="Always">
                        <ContentTemplate>
                            <div class="hs-grid">

                                <div class="hs-head">
                                    <h2>รายการเปลี่ยนแปลง Location<span class="hs-count"><asp:Label ID="lblTotalCL" runat="server" Text="0" /></span></h2>
                                    <div class="cl-legend">
                                        <span class="cl-tag cl-tag-old">เดิม</span> ค่าก่อนแก้ไข
                                        <span class="cl-tag cl-tag-new">ใหม่</span> ค่าหลังแก้ไข
                                    </div>
                                </div>

                                <div class="hs-tbl-wrap" id="wrapCL" data-keep-scroll="1" style="max-height: calc(100vh - 360px);">
                                    <asp:GridView ID="gvChangeLocationList" runat="server"
                                        CssClass="gv cl-gv"  style="min-width:1500px;"
                                        GridLines="None" CellSpacing="0"
                                        AutoGenerateColumns="False"
                                        AlternatingRowStyle-CssClass="alt-row"
                                        OnRowCommand="gvChangeLocationList_RowCommand"
                                        EmptyDataText="<div class='no-data'>ไม่พบการเปลี่ยนแปลงในช่วงนี้ — ลองขยายช่วงวันที่หรือล้างตัวกรอง</div>">

                                        <Columns>

                                            <%-- History : ติดซ้ายสุด --%>
                                            <asp:TemplateField HeaderText="ประวัติ" HeaderStyle-CssClass="c-action" ItemStyle-CssClass="c-action">
                                                <ItemTemplate>
                                                    <asp:Button ID="btnCLHistory" runat="server" Text="🕓"
                                                        CommandName="ViewCLHistory"
                                                        CommandArgument='<%# Eval("Tool_id") + "|" + Eval("Tool_sub") + "|" + Eval("System_id") %>'
                                                        CssClass="icon-btn" ToolTip="ดูประวัติการเปลี่ยนแปลงทั้งหมดของอุปกรณ์นี้" />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- No --%>
                                            <asp:TemplateField HeaderText="#" HeaderStyle-CssClass="c-center" ItemStyle-CssClass="c-center c-no" ItemStyle-Width="44px">
                                                <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- อุปกรณ์ --%>
                                            <asp:TemplateField HeaderText="อุปกรณ์" ItemStyle-Width="180px">
                                                <ItemTemplate>
                                                    <div class="cl-asset">
                                                        <div><span class="k">FA</span><span class="v"><%# Txt(Eval("Fix_Asset_No")) %></span></div>
                                                        <div><span class="k">CT</span><span class="v"><%# Txt(Eval("Control_No")) %></span></div>
                                                        <div><span class="k">TL</span><span class="v"><%# Txt(Eval("Tool_No")) %></span></div>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- Location : เดิม vs ใหม่ --%>
                                            <asp:TemplateField HeaderText="Location (Model › Cell › Station)" ItemStyle-Width="450px">
                                                <ItemTemplate><%# BuildLocationDiff(Container.DataItem) %></ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- ข้อมูลอื่นที่เปลี่ยน --%>
                                            <asp:TemplateField HeaderText="ข้อมูลอื่นที่เปลี่ยน">
                                                <ItemTemplate>
                                                    <%# BuildFieldDiff(Container.DataItem) %>
                                                    <%# BuildNote(Container.DataItem) %>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- Edit By --%>
                                            <asp:TemplateField HeaderText="แก้ไขโดย" ItemStyle-Width="100px">
                                                <ItemTemplate><%# Txt(Eval("Edit_By")) %></ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- Edit Date --%>
                                            <asp:TemplateField HeaderText="วันที่แก้ไข" ItemStyle-CssClass="c-date" ItemStyle-Width="110px">
                                                <ItemTemplate>
                                                    <%# Eval("Edit_Date", "{0:dd-MMM-yyyy}") %><br />
                                                    <span class="cl-time"><%# Eval("Edit_Date", "{0:HH:mm}") %></span>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                        </Columns>
                                    </asp:GridView>
                                </div>

                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
            </asp:Panel>

            <%-- ===== History Timeline Modal ===== --%>
            <asp:Panel ID="pnlHistoryModal" runat="server" CssClass="hist-overlay hs-hist" Visible="false">
                <div class="hist-modal" role="dialog" aria-modal="true">
                    <div class="hist-modal-header">
                        <span>Timeline <asp:Label ID="lblHistDocNo" runat="server" CssClass="doc" /></span>
                        <asp:LinkButton ID="btnHistClose" runat="server" CssClass="hist-close"
                            OnClick="btnHistClose_Click" ToolTip="ปิด">&times;</asp:LinkButton>
                    </div>
                    <div class="hist-modal-body">
                        <asp:Literal ID="litTimeline" runat="server" />
                    </div>
                </div>
            </asp:Panel>

            <%-- ===== Items Modal ===== --%>
            <div class="modal fade aps-modal" id="modal_ItemsList" tabindex="-1" role="dialog" aria-hidden="true" data-backdrop="static">
                <div class="modal-dialog" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">รายการอุปกรณ์ <asp:Label ID="lblItemsDocNo" runat="server" CssClass="doc" /></h5>
                            <button type="button" class="close" data-dismiss="modal" aria-label="ปิด"><span>&times;</span></button>
                        </div>
                        <div class="modal-body">
                            <asp:UpdatePanel ID="upItems" runat="server">
                                <ContentTemplate>
                                    <asp:Repeater ID="rptItems" runat="server">
                                        <HeaderTemplate>
                                            <table class="tbl">
                                                <thead>
                                                    <tr>
                                                        <th style="width:44px;">#</th>
                                                        <th style="width:160px;">รูป</th>
                                                        <th>Fix Asset</th>
                                                        <th>Control No</th>
                                                        <th>Tool No / Equipment No</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                                    <tr>
                                                        <td><%# Eval("No") %></td>
                                                        <td>
                                                            <img src='<%# Eval("PicUrl") %>' alt=""
                                                                 onerror="this.src='images/FilesType/jpeg.png';" />
                                                        </td>
                                                        <td><%# Eval("FixAsset") %></td>
                                                        <td><%# Eval("ControlNo") %></td>
                                                        <td><%# Eval("tool_no") %></td>
                                                    </tr>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                                </tbody>
                                            </table>
                                        </FooterTemplate>
                                    </asp:Repeater>
                                    <asp:Label ID="lblNoItems" runat="server" Visible="false"
                                        Text="คำขอนี้ยังไม่มีรายการอุปกรณ์" CssClass="text-muted" />
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn-close-foot" data-dismiss="modal">ปิด</button>
                        </div>
                    </div>
                </div>
            </div>

            <%-- ===== Requester Modal ===== --%>
            <div class="modal fade aps-modal modal-sm-w" id="modal_Issuer" tabindex="-1" role="dialog" aria-hidden="true" data-backdrop="static">
                <div class="modal-dialog" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h4 class="modal-title">ข้อมูลผู้ขอ</h4>
                            <button type="button" class="close" data-dismiss="modal" aria-label="ปิด"><span>&times;</span></button>
                        </div>
                        <div class="modal-body">
                            <asp:UpdatePanel ID="upIssuer" runat="server">
                                <ContentTemplate>
                                    <table class="kv" id="requester_detail">
                                        <tr><th>Emp ID</th>    <td><asp:Label ID="lblIsId" runat="server" /></td></tr>
                                        <tr><th>ชื่อ-สกุล</th> <td><asp:Label ID="lblIsName" runat="server" /></td></tr>
                                        <tr><th>ตำแหน่ง</th>   <td><asp:Label ID="lblIsPos" runat="server" /></td></tr>
                                        <tr><th>Band</th>      <td><asp:Label ID="lblIsBand" runat="server" /></td></tr>
                                        <tr><th>แผนก</th>      <td><asp:Label ID="lblIsDept" runat="server" /></td></tr>
                                        <tr><th>Dept Code</th> <td><asp:Label ID="lblIsDeptCode" runat="server" /></td></tr>
                                        <tr><th>Email</th>     <td><asp:Label ID="lblIsEmail" runat="server" /></td></tr>
                                        <tr><th>โทร</th>       <td><asp:Label ID="lblIsTel" runat="server" /></td></tr>
                                    </table>
                                    <asp:Label ID="lblIsErr" runat="server" Visible="false"
                                        CssClass="text-danger" Text="ไม่พบข้อมูลพนักงานใน HRMS" />
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn-close-foot" data-dismiss="modal">ปิด</button>
                        </div>
                    </div>
                </div>
            </div>

            <%-- ===== History Change Location Timeline Modal ===== --%>
            <asp:Panel ID="pnlCLHistoryModal" runat="server" CssClass="hist-overlay hs-hist" Visible="false">
                <div class="hist-modal cl-modal" role="dialog" aria-modal="true">
                    <div class="hist-modal-header">
                        <span>ประวัติการเปลี่ยนแปลง <asp:Label ID="lblCLHistDocNo" runat="server" CssClass="doc" /></span>
                        <asp:LinkButton ID="btnCLHistClose" runat="server" CssClass="hist-close"
                            OnClick="btnCLHistClose_Click" ToolTip="ปิด">&times;</asp:LinkButton>
                    </div>
                    <div class="hist-modal-body">
                        <asp:Literal ID="litCLTimeline" runat="server" />
                    </div>
                </div>
            </asp:Panel>

        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>