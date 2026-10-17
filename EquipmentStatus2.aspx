<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="EquipmentStatus2.aspx.cs"
    Inherits="Tool_Equipment_Control_System.pendingStatus" %>

<asp:Content ID="Tool_Equipment_Home" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <link href="css/bootstrap_sum.css" rel="stylesheet">
    <link href="CSS/w3v4.10.css" rel="stylesheet" />
    <link href="css/styles.css" rel="stylesheet">
    <link href="Content/chosen.css" rel="stylesheet">
    <link href="CSS/fontsgoogleapis.css" rel="stylesheet">
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

    <%-- ================= Toast ================= --%>
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
                closeBtn.addEventListener('click', () => container.removeChild(toast));

                toast.appendChild(content);
                toast.appendChild(closeBtn);
                container.appendChild(toast);

                if (duration > 0) {
                    setTimeout(() => { if (toast.parentNode) container.removeChild(toast); }, duration);
                }
            };
        })();
    </script>

    <%-- ================= HRMS (client) ================= --%>
    <script type="text/javascript">
        function fetchRequesterName() { }

        function getDataHRMS(userId, id) {
            if (userId.length < 6) return;

            var body = {
                command: "select GNAME_ENG, FNAME_ENG, POSN_ENAME, BAND, DEPT_ABB_NAME, EMAIL " +
                    "from ADMIN.v_emp_data_all_cpt " +
                    "where UPPER(emp_no) = UPPER('" + userId + "')"
            };

            $.ajax({
                url: "http://cptsvs547:1000/middleware/oracle/hrms",
                type: "POST", data: body, cache: false,
                success: function (result) {
                    if (result.data && result.data.length > 0) {
                        var u = result.data[0];
                        document.getElementById('<%= txtEmpId.ClientID %>').style.backgroundColor = "#e3f4e8";
                        document.getElementById('<%= txtEmpName.ClientID %>').value = u.GNAME_ENG + " " + u.FNAME_ENG;
                        document.getElementById('<%= txtPosition.ClientID %>').value = u.POSN_ENAME;
                        document.getElementById('<%= txtDept.ClientID %>').value = u.DEPT_ABB_NAME;
                        document.getElementById('<%= hdtxtBand.ClientID %>').value = u.BAND;
                        document.getElementById('<%= hdtxtEmail.ClientID %>').value = u.EMAIL;
                    } else {
                        document.getElementById('<%= txtEmpName.ClientID %>').value = "";
                        document.getElementById('<%= txtPosition.ClientID %>').value = "";
                        document.getElementById('<%= txtDept.ClientID %>').value = "";
                        document.getElementById('<%= hdtxtBand.ClientID %>').value = "";
                        document.getElementById('<%= hdtxtEmail.ClientID %>').value = "";
                    }
                },
                error: function () { alert("Error calling HRMS API"); }
            });
        }
    </script>

    <%-- ================= Date Picker ================= --%>
    <script type="text/javascript">
        function initDatePicker() {
            var txtFrom = $('#<%= txtDateFrom.ClientID %>');
            var txtTo = $('#<%= txtDateTo.ClientID %>');

            if (txtFrom.data("DateTimePicker")) return;   // กัน init ซ้ำ

            var today = new Date();
            var twoWeeksAgo = new Date();
            twoWeeksAgo.setDate(today.getDate() - 14);

            function formatDate(date) {
                var day = ("0" + date.getDate()).slice(-2);
                var m = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
                return day + "-" + m[date.getMonth()] + "-" + date.getFullYear();
            }

            if (!txtFrom.val()) txtFrom.val(formatDate(twoWeeksAgo));
            if (!txtTo.val()) txtTo.val(formatDate(today));

            txtFrom.datetimepicker({ format: 'DD-MMM-YYYY' });
            txtTo.datetimepicker({ format: 'DD-MMM-YYYY' });

            $(".calendarFrom").off("click").on("click", function () { txtFrom.data("DateTimePicker").show(); });
            $(".calendarTo").off("click").on("click", function () { txtTo.data("DateTimePicker").show(); });

            txtFrom.on("dp.change", function (e) {
                txtTo.data("DateTimePicker").minDate(e.date);
                __doPostBack("<%= txtDateFrom.UniqueID %>", "");
            });
            txtTo.on("dp.change", function (e) {
                txtFrom.data("DateTimePicker").maxDate(e.date);
                __doPostBack("<%= txtDateTo.UniqueID %>", "");
            });
        }

        $(document).ready(function () { initDatePicker(); });

        if (typeof (Sys) !== "undefined") {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () { initDatePicker(); });
        }
    </script>

    <%-- ============================================================
         DESIGN TOKENS (ชุดเดียวกับ ApproveStatus)
         ============================================================ --%>
    <style>
        .aps {
            --ink: #1c2b3a;
            --ink-2: #33475b;
            --muted: #66727f;
            --paper: #f3f5f8;
            --surface: #ffffff;
            --line: #dde3ea;
            --line-soft: #edf0f4;
            --focus: #1f6feb;

            --edit:    #1f6feb;  --edit-h:    #1858c4;
            --approve: #16884a;

            --r-sm: 6px; --r-md: 10px;

            font-family: 'IBM Plex Sans Thai', 'Segoe UI', Tahoma, sans-serif;
            color: var(--ink);
            padding: 20px 10px 32px;
            margin: 10px 0px;
            height: calc(100vh - 50px);
        }
        .aps *, .aps *::before, .aps *::after { box-sizing: border-box; }

        /* ---------- Page header ---------- */
        .aps-header { display: flex; align-items: center; gap: 14px; }
        .aps-header img { height: 40px; }
        .aps-header h1 { margin: 0; font-size: 22px; font-weight: 700; line-height: 1.2; color: var(--ink); }
        .aps-header p { margin: 2px 0 0; font-size: 13px; color: var(--muted); }

        /* ---------- Panel ---------- */
        .aps-panel {
            background: var(--surface);
            border: 1px solid var(--line);
            border-radius: var(--r-md);
            margin-bottom: 8px;
        }
        .aps-panel-head {
            display: flex; align-items: center; justify-content: space-between;
            padding: 8px 18px;
            border-bottom: 1px solid var(--line-soft);
        }
        .aps-panel-head h2 { margin: 0; font-size: 15px; font-weight: 700; color: var(--ink); }
        .aps-count {
            display: inline-block; margin-left: 6px;
            padding: 1px 9px; border-radius: 999px;
            background: var(--approve);
            color: var(--body-color);
            font-size: 12px; font-weight: 700;
            font-variant-numeric: tabular-nums;
        }
        .aps-panel-body { padding: 14px 18px; }

        /* ---------- Fields ---------- */
        .aps-fields { display: flex; flex-wrap: wrap; gap: 12px 14px; align-items: flex-end; }
        .aps-field { display: flex; flex-direction: column; gap: 5px; }
        .aps-field > label { margin: 0; font-size: 12px; font-weight: 600; color: var(--muted); }
        .aps .fld {
            height: 36px; padding: 0 10px;
            font-family: inherit; font-size: 14px; color: var(--ink);
            background: var(--surface);
            border: 1px solid #c9d2dc; border-radius: var(--r-sm);
            box-shadow: none; margin: 0;
            transition: border-color .15s, box-shadow .15s;
        }
        .aps .fld:focus { outline: none; border-color: var(--focus); box-shadow: 0 0 0 3px rgba(31,111,235,.18); }
        .aps .fld[readonly] { background: var(--line); border-color: transparent; color: var(--ink-2); }
        .aps .fld-emp { font-weight: 700; text-align: center; letter-spacing: .05em; }
        .aps select.fld { padding-right: 6px; min-width: 150px; }

        .aps-notice {
            margin-top: 12px; padding: 9px 12px;
            border-left: 3px solid #d9822b; border-radius: var(--r-sm);
            background: #fdf3e7; color: #8a4b0c; font-size: 13px;
        }

        /* ---------- Toolbar ---------- */
        .aps-toolbar {
            display: flex; flex-wrap: wrap; gap: 12px 14px; align-items: flex-end;
            padding: 14px 18px;
            border-bottom: 1px solid var(--line-soft);
        }

        /* Checkbox : เฉพาะคำขอที่ยังดำเนินการอยู่ */
        .chk-chip {
            display: inline-flex; align-items: center; gap: 8px;
            height: 36px; padding: 0 12px;
            background: var(--paper); border: 1px solid var(--muted);
            border-radius: var(--r-sm);
            font-size: 13px; font-weight: 600; color: var(--ink);
            cursor: pointer;
        }
        .chk-chip input[type=checkbox] { width: 18px; height: 18px; margin: 0; cursor: pointer; accent-color: var(--primary-color, #1f6feb); }
        .chk-chip label { margin: 0; cursor: pointer; font-weight: 600; }
        .chk-chip.is-on { background: var(--primary-color, #1f6feb); border-color: var(--primary-color, #1f6feb); }
        .chk-chip.is-on label { color: var(--primary-color-light, #fff); }

        /* Date input */
        .date-input-wrapper { position: relative; }
        .aps .date-input { padding-right: 32px; text-align: center; }
        .calendar-icon {
            position: absolute; right: 8px; top: 50%; transform: translateY(-50%);
            cursor: pointer; font-size: 14px; opacity: .6;
        }
        .calendar-icon:hover { opacity: 1; }

        /* Toolbar buttons */
        .aps .tb-btn {
            display: inline-flex; align-items: center; justify-content: center; gap: 6px;
            height: 36px; padding: 0 16px;
            font-family: inherit; font-size: 14px; font-weight: 600;
            border-radius: var(--r-sm); border: 1px solid transparent;
            cursor: pointer; text-decoration: none; text-shadow: none;
            transition: background .15s, border-color .15s, color .15s;
        }
        .aps .tb-primary { background: var(--ink); color: #fff; }
        .aps .tb-primary:hover { background: #0f1b27; color: #fff; }
        .aps .tb-ghost { background: transparent; color: var(--ink-2); border-color: var(--line); }
        .aps .tb-ghost:hover { background: var(--paper); color: var(--ink); text-decoration: none; }

        /* ---------- Table ---------- */
        .tbl-wrap { max-height: calc(100vh - 390px); overflow: auto; position: relative; margin: 0px 10px; }

        .aps .gv { width: 100%; min-width: 1320px; border-collapse: separate; border-spacing: 0; font-size: 13.5px; }
        .aps .gv th {
            position: sticky; top: 0; z-index: 2;
            background: #1a3a5c; color: var(--surface);
            font-size: 12.5px; font-weight: 700; text-align: left;
            padding: 10px 12px; white-space: nowrap;
            border-bottom: 1px solid var(--line);
        }
        .aps .gv td {
            padding: 10px 12px; vertical-align: middle;
            border-bottom: 1px solid var(--line-soft);
            background: var(--surface);
        }
        .aps .gv tr.alt-row td { background: #eaf0f7; }
        .aps .gv tr:hover td { background: #d6e5f8; }

        .aps .gv .c-center { text-align: center; }
        .aps .gv .c-no { color: var(--muted); font-variant-numeric: tabular-nums; }
        .aps .gv .c-doc { font-weight: 700; font-variant-numeric: tabular-nums; white-space: nowrap; }
        .aps .gv .c-date { white-space: nowrap; font-variant-numeric: tabular-nums; color: var(--ink-2); }
        .aps .gv .req-comment { display: block; font-size: 12px; color: var(--muted); margin-top: 2px; }
        .aps .gv .c-progress { font-size: 12.5px; color: var(--ink-2); line-height: 1.4; }
        .aps .gv .emp-id   { display: block; font-weight: 600; font-variant-numeric: tabular-nums; }
        .aps .gv .emp-name { display: block; font-size: 12px; color: var(--muted); }

        /* Action column : ติดซ้ายสุดเสมอ */
        .aps .gv th.c-action, .aps .gv td.c-action {
            position: sticky; left: 0px;
            text-align: center; white-space: nowrap;
        }
        .aps .gv th.c-action { z-index: 3; border-right: 1px solid #0f2740; }
        .aps .gv td.c-action {
            z-index: 1;
            border-right: 1px solid var(--line);
            box-shadow: 6px 0 8px -6px rgba(28,43,58,.22);
        }

        .no-data { text-align: center; color: var(--muted); padding: 48px 16px; font-size: 14px; }

        /* ---------- Status pill (CssClass มาจาก GetStatusClass) ---------- */
        .aps .gv .badge {
            display: inline-flex; align-items: center; gap: 6px;
            padding: 3px 10px 3px 8px; border-radius: 999px;
            font-size: 12px; font-weight: 700; line-height: 1.5;
            white-space: nowrap; text-shadow: none;
            background: #eef1f4; color: #4b5663; border: 1px solid #d8dee5;
        }
        .aps .gv .badge::before { content: ''; width: 7px; height: 7px; border-radius: 50%; background: currentColor; }
        .aps .gv .badge.status-draft     { background: #fff7e0; color: #946200; border-color: #f1d98c; }
        .aps .gv .badge.status-approve   { background: #e8f0fe; color: #1a5bc4; border-color: #bcd1f7; }
        .aps .gv .badge.status-confirm   { background: #f5e9f8; color: #92278f; border-color: #dfc0e6; }
        .aps .gv .badge.status-borrow    { background: #ecebfb; color: #4a3fb0; border-color: #cdc8f1; }
        .aps .gv .badge.status-completed { background: #e3f4e8; color: #16703d; border-color: #b6e0c4; }
        .aps .gv .badge.status-cancel    { background: #f0f1f3; color: #6b7480; border-color: #d9dde2; }

           /* ---------- Row buttons ---------- */
        .aps .act-btn {
            display: inline-flex; align-items: center; justify-content: center; gap: 5px;
            min-width: 96px; height: 32px; padding: 0 12px; margin: 0 2px;
            font-family: inherit; font-size: 13px; font-weight: 700;
            border-radius: var(--r-sm); border: 1px solid transparent;
            color: #fff; cursor: pointer; text-decoration: none; text-shadow: none;
            transition: background .15s, border-color .15s;
        }
        .aps .act-edit { background: var(--edit); }
        .aps .act-edit:hover { background: var(--edit-h); color: #fff; }
        .aps .act-view { background: var(--surface); color: var(--ink-2); border-color: #c9d2dc; }
        .aps .act-view:hover { background: var(--paper); color: var(--ink); border-color: #9aa8b6; }

        /* Items / Timeline */
        .aps .chip-btn {
            display: inline-flex; align-items: center; gap: 4px;
            padding: 3px 10px; border-radius: 999px;
            background: var(--paper); border: 1px solid var(--line);
            color: var(--ink-2); font-size: 13px; font-weight: 700;
            font-variant-numeric: tabular-nums; text-decoration: none;
        }
        .aps .chip-btn:hover { border-color: #9aa8b6; color: var(--ink); text-decoration: none; }

        .aps .icon-btn {
            width: 32px; height: 32px; padding: 0;
            border: 1px solid transparent; border-radius: var(--r-sm);
            background: transparent; font-size: 16px; cursor: pointer;
        }
        .aps .icon-btn:hover { background: var(--paper); border-color: var(--line); }

        .aps a:focus-visible, .aps button:focus-visible, .aps input[type=submit]:focus-visible {
            outline: 2px solid var(--focus); outline-offset: 2px;
        }

        /* ---------- History timeline modal ---------- */
        .hist-overlay {
            position: fixed; inset: 0; z-index: 9998;
            background: rgba(20,30,42,.45);
            display: flex; align-items: center; justify-content: center;
        }
        .hist-modal {
            width: 560px; max-width: 94%; max-height: 82vh;
            display: flex; flex-direction: column; overflow: hidden;
            background: #fff; border-radius: 12px;
            box-shadow: 0 20px 50px rgba(20,30,42,.3);
            font-family: 'IBM Plex Sans Thai', 'Segoe UI', Tahoma, sans-serif;
        }
        .hist-modal-header {
            display: flex; justify-content: space-between; align-items: center;
            padding: 14px 20px; border-bottom: 1px solid #edf0f4;
            font-size: 15px; font-weight: 700; color: #1c2b3a;
        }
        .hist-modal-header .doc { color: #1f6feb; font-variant-numeric: tabular-nums; }
        .hist-close {
            width: 32px; height: 32px; border-radius: 6px;
            display: inline-flex; align-items: center; justify-content: center;
            color: #66727f; font-size: 22px; text-decoration: none;
        }
        .hist-close:hover { background: #f3f5f8; color: #1c2b3a; text-decoration: none; }
        .hist-modal-body { padding: 20px 24px; overflow-y: auto; }

        .timeline { list-style: none; margin: 0; padding: 0; position: relative; }
        .timeline:before { content: ''; position: absolute; left: 11px; top: 8px; bottom: 8px; width: 2px; background: #dde3ea; }
        .tl-item { position: relative; padding: 0 0 20px 38px; }
        .tl-item:last-child { padding-bottom: 0; }
        .tl-dot {
            position: absolute; left: 4px; top: 3px;
            width: 16px; height: 16px; border-radius: 50%;
            background: #1f6feb; border: 3px solid #fff; box-shadow: 0 0 0 2px #1f6feb; z-index: 1;
        }
        .tl-dot.done   { background: #16884a; box-shadow: 0 0 0 2px #16884a; }
        .tl-dot.cancel { background: #c93b3b; box-shadow: 0 0 0 2px #c93b3b; }
        .tl-dot.wait   { background: #fff;    box-shadow: 0 0 0 2px #b8c2cc; }
        .tl-title { font-size: 14px; font-weight: 700; color: #1c2b3a; }
        .tl-meta  { font-size: 12.5px; color: #33475b; margin-top: 2px; }
        .tl-date  { font-size: 12px; color: #66727f; margin-top: 2px; font-variant-numeric: tabular-nums; }
        .tl-pending .tl-title { color: #8a95a1; font-weight: 600; }
        .tl-pending .tl-meta  { color: #8a95a1; }

        /* ---------- Bootstrap modal (Items) ---------- */
        .aps-modal.modal {
            position: fixed; top: 0 !important; left: 0; right: 0; bottom: 0;
            width: auto; margin: 0;
            background: none; border: none; box-shadow: none;
            overflow-y: auto;
        }
        .aps-modal .modal-dialog { width: 860px; max-width: 94%; margin: 60px auto; }
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
        .aps-modal .modal-header .close { order: 2; opacity: .6; font-size: 24px; text-shadow: none; }
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

        .aps-modal .btn-close-foot {
            height: 34px; padding: 0 18px; border-radius: 6px;
            background: #fff; border: 1px solid #c9d2dc; color: #33475b;
            font-weight: 600; cursor: pointer;
        }
        .aps-modal .btn-close-foot:hover { background: #f3f5f8; }

        .ui-autocomplete { z-index: 9999 !important; font-size: 13px; }

        @media (max-width: 760px) {
            .aps { padding: 14px 10px 24px; }
            .aps-field, .aps .fld { width: 100% !important; }
        }
        @media (prefers-reduced-motion: reduce) {
            .aps *, .hist-modal * { transition: none !important; }
        }
    </style>

    <asp:ScriptManager runat="server" ID="scriptManager" EnablePageMethods="true"></asp:ScriptManager>

    <asp:UpdatePanel runat="server" ID="UP1">
        <ContentTemplate>
        <div class="aps">

            <%-- ===== Header ===== --%>
            <div class="aps-header" style="padding: 10px 20px; margin-bottom: 8px; border-radius: 10px; background: #fff; border-left: 10px solid #295a8f;">
                <img src="images/Toolstatus.png" alt="" />
                <div>
                    <h1>REQUEST STATUS</h1>
                    <p>ติดตามคำขอ Jig Tool &amp; Equipment ของคุณว่าอยู่ขั้นตอนไหน</p>
                </div>
            </div>

            <%-- ===== Section 1 : ผู้ใช้งาน ===== --%>
            <div class="aps-panel" style="border-left: 10px solid #295a8f;">
                <div class="aps-panel-body">
                    <div class="aps-fields">

                        <div class="aps-field">
                            <label for="<%= txtEmpId.ClientID %>">Employee ID</label>
                            <asp:TextBox ID="txtEmpId" runat="server" onkeyup="fetchRequesterName();"
                                CssClass="fld fld-emp" MaxLength="6" Width="120px"
                                placeholder="รหัสพนักงาน" AutoComplete="off" AutoPostBack="true"
                                OnTextChanged="txtEmpId_TextChanged" />
                        </div>

                        <div class="aps-field">
                            <label>ชื่อ-สกุล</label>
                            <asp:TextBox ID="txtEmpName" runat="server" ReadOnly="true" Width="240px"
                                CssClass="fld" style="text-align: center;" placeholder="—" />
                        </div>

                        <div class="aps-field">
                            <label>ตำแหน่ง</label>
                            <asp:TextBox ID="txtPosition" runat="server" ReadOnly="true" Width="220px"
                                CssClass="fld" style="text-align: center;" placeholder="—" />
                        </div>

                        <div class="aps-field">
                            <label>แผนก</label>
                            <asp:TextBox ID="txtDept" runat="server" ReadOnly="true" Width="140px"
                                CssClass="fld" style="text-align: center;" placeholder="—" />
                        </div>

                        <div class="aps-field">
                            <label>Dept Code</label>
                            <asp:TextBox ID="txtDeptCode" runat="server" ReadOnly="true" Width="100px"
                                CssClass="fld" style="text-align: center;" placeholder="—" />
                        </div>

                        <asp:HiddenField ID="hdtxtBand" runat="server" />
                        <asp:HiddenField ID="hdtxtEmail" runat="server" />
                    </div>

                    <asp:Panel ID="pnlNewEmp" runat="server" Visible="false">
                        <div class="aps-notice">
                            <asp:Label ID="lblempNotice" runat="server"></asp:Label>
                        </div>
                    </asp:Panel>
                </div>
            </div>

            <%-- ===== Section 2 : รายการคำขอ ===== --%>
            <div class="aps-panel" style="height: calc(100vh - 245px);">

                <%-- Toolbar --%>
                <div class="aps-toolbar">

                    <div class="aps-field">
                        <label>👁️ มุมมอง</label>
                        <span class='chk-chip <%= chkbox_not.Checked ? "is-on" : "" %>'>
                            <asp:CheckBox ID="chkbox_not" runat="server" Checked="true"
                                AutoPostBack="true" OnCheckedChanged="chkNot_CheckedChanged"
                                Text="เฉพาะที่ยังดำเนินการอยู่" />
                        </span>
                    </div>

                    <div class="aps-field">
                        <label for="<%= ddlProcess.ClientID %>">⚙️ ประเภทคำขอ</label>
                        <asp:DropDownList ID="ddlProcess" runat="server" CssClass="fld" style="text-align: center;" Width="140px"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlProcess_SelectedIndexChanged" />
                    </div>

                    <div class="aps-field">
                        <label for="<%= ddlStatus.ClientID %>">📌 สถานะ</label>
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="fld" style="text-align: center;" Width="190px"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged" />
                    </div>

                    <div class="aps-field">
                        <label for="<%= txtDateFrom.ClientID %>">📅 ตั้งแต่วันที่</label>
                        <div class="date-input-wrapper">
                            <asp:TextBox ID="txtDateFrom" runat="server" CssClass="fld date-input" Width="150px"
                                AutoPostBack="true" OnTextChanged="txtDateFrom_TextChanged" AutoComplete="off" />
                            <span class="calendar-icon calendarFrom" title="เลือกวันที่">📅</span>
                        </div>
                    </div>

                    <div class="aps-field">
                        <label for="<%= txtDateTo.ClientID %>">📅 ถึงวันที่</label>
                        <div class="date-input-wrapper">
                            <asp:TextBox ID="txtDateTo" runat="server" CssClass="fld date-input" Width="150px"
                                AutoPostBack="true" OnTextChanged="txtDateTo_TextChanged" AutoComplete="off" />
                            <span class="calendar-icon calendarTo" title="เลือกวันที่">📅</span>
                        </div>
                    </div>

                    <asp:Button ID="btnSearch" runat="server" Text="ค้นหา"
                        CssClass="tb-btn tb-primary" OnClick="btnSearch_Click" />

                    <asp:LinkButton ID="btnClear" runat="server" OnClick="btnClear_Click"
                        CssClass="tb-btn tb-ghost">ล้างตัวกรอง</asp:LinkButton>
                </div>

                <%-- Grid --%>
                <asp:UpdatePanel ID="upGrid" runat="server" UpdateMode="Always">
                    <ContentTemplate>

                        <div class="aps-panel-head">
                            <h2>คำขอของฉัน<span class="aps-count"><asp:Label ID="lblTotal" runat="server" Text="0" /></span></h2>
                        </div>

                        <div class="tbl-wrap">
                            <asp:GridView ID="gvRequestList" runat="server"
                                CssClass="gv" GridLines="None" CellSpacing="0"
                                AutoGenerateColumns="False"
                                AlternatingRowStyle-CssClass="alt-row"
                                OnRowDataBound="gvRequestList_RowDataBound"
                                OnRowCommand="gvRequestList_RowCommand"
                                EmptyDataText="<div class='no-data'>ไม่มีคำขอในช่วงนี้ — ลองเปลี่ยนช่วงวันที่ หรือเอาเครื่องหมาย “เฉพาะที่ยังดำเนินการอยู่” ออก</div>">

                                <Columns>

                                    <%-- Action : ติดซ้ายสุด --%>
                                    <asp:TemplateField HeaderText="ดำเนินการ" HeaderStyle-CssClass="c-action" ItemStyle-CssClass="c-action">
                                        <ItemTemplate>
                                            <asp:HiddenField ID="hdstatusId" runat="server" Value='<%# Eval("status_id") %>' />

                                            <asp:Button ID="btnEdit" runat="server" Text="แก้ไข"
                                                CommandName="EditRow" CommandArgument='<%# Eval("Doc_No") %>'
                                                CssClass="act-btn act-edit" ToolTip="แก้ไขคำขอที่ยังไม่ได้ส่ง" />

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
                                    <asp:TemplateField HeaderText="ขั้นตอนถัดไป" ItemStyle-CssClass="c-progress" ItemStyle-Width="230px">
                                        <ItemTemplate>
                                            <asp:Label runat="server" ID="lb_nextStatus" Text='<%# Eval("detail") %>' />
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
                                            <asp:Label runat="server" ID="lb_empId" CssClass="emp-id" Text='<%# Eval("issue_empid") %>' />
                                            <asp:Label runat="server" ID="lb_empName" CssClass="emp-name" Text='<%# Eval("empName") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%-- Issue Date --%>
                                    <asp:TemplateField HeaderText="วันที่ขอ" ItemStyle-CssClass="c-date">
                                        <ItemTemplate>
                                            <asp:Label runat="server" ID="lb_issueDate" Text='<%# Eval("issue_date", "{0:dd-MMM-yyyy}") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%-- Completed Date --%>
                                    <asp:TemplateField HeaderText="วันที่เสร็จ" ItemStyle-CssClass="c-date">
                                        <ItemTemplate>
                                            <asp:Label runat="server" ID="lb_completedDate" Text='<%# Eval("completed_date", "{0:dd-MMM-yyyy}") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                </Columns>
                            </asp:GridView>
                        </div>

                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>

        </div>

        <%-- ===== History Modal ===== --%>
        <asp:Panel ID="pnlHistoryModal" runat="server" CssClass="hist-overlay" Visible="false">
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

        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content> 