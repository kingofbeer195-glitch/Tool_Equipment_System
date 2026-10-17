<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="withdraw2.aspx.cs"
    Inherits="Tool_Equipment_Control_System.Withdraw_Tool" EnableEventValidation="false" %>

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
                closeBtn.addEventListener('click', () => {
                    container.removeChild(toast);
                });
                toast.appendChild(content);
                toast.appendChild(closeBtn);
                container.appendChild(toast);
                if (duration > 0) {
                    setTimeout(() => {
                        if (toast.parentNode) container.removeChild(toast);
                    }, duration);
                }
            };
        })();
    </script>

    <%--Issue Date--%>
    <script type="text/javascript">
        function Set_datePicker() {
            moment.locale('en');
            $('#IssueDatePicker').datetimepicker({
                format: 'DD-MMM-YYYY',
                useCurrent: false,
                showClear: true,
                showClose: true
            }).on('dp.change', function (e) {
                __doPostBack('<%= txtIssueDate.UniqueID %>', '');
            });
        }
        $(function () {
            Set_datePicker();
        });
        var raw = $('#txtIssuedate').val();
        if (raw) {
            var m = moment(raw, 'DD MMM YYYY');
            if (m.isValid()) {
                $('#IssueDatePicker').data("DateTimePicker").date(m);
            }
        }
    </script>

    <%-- ตรวจสอบการแก้ไข Request ID --%>
    <script type="text/javascript">
        function fetchRequesterName() {
        }
        function getDataHRMS(userId, id) {
            if (userId.length < 6) {
                return;
            }
            var body = {
                command: "select GNAME_ENG, FNAME_ENG, POSN_ENAME, BAND, DEPT_ABB_NAME, EMAIL " +
                    "from ADMIN.v_emp_data_all_cpt " +
                    "where UPPER(emp_no) = UPPER('" + userId + "')"
            };
            var url = "http://cptsvs547:1000/middleware/oracle/hrms";
            $.ajax({
                url: url,
                type: "POST",
                data: body,
                cache: false,
                success: function (result) {
                    if (result.data && result.data.length > 0) {
                        var dataUser = result.data[0];
                        document.getElementById('<%= txtEmpId.ClientID %>').style.backgroundColor = "#d4edda";
                        document.getElementById('<%= txtEmpName.ClientID %>').value = dataUser.GNAME_ENG + " " + dataUser.FNAME_ENG;
                        document.getElementById('<%= txtPosition.ClientID %>').value = dataUser.POSN_ENAME;
                        document.getElementById('<%= txtDept.ClientID %>').value = dataUser.DEPT_ABB_NAME;
                        document.getElementById('<%= hdtxtBand.ClientID %>').value = dataUser.BAND;
                        document.getElementById('<%= hdtxtEmail.ClientID %>').value = dataUser.EMAIL;
                    }
                    else {
                        document.getElementById('<%= txtEmpName.ClientID %>').value = "";
                        document.getElementById('<%= txtPosition.ClientID %>').value = "";
                        document.getElementById('<%= txtDept.ClientID %>').value = "";
                        document.getElementById('<%= hdtxtBand.ClientID %>').value = "";
                        document.getElementById('<%= hdtxtEmail.ClientID %>').value = "";
                    }
                },
                error: function () {
                    alert("Error calling HRMS API");
                }
            });
        }
    </script>

    <%--Alert Confirm--%>    
    <script>
        function confirmDraft() {
            return confirm("⚠️ ยืนยันการบันทึก Draft\nคุณต้องการบันทึกข้อมูลหรือไม่?\nกด OK เพื่อดำเนินการต่อ");
        }
        function sendtoApprove() {
            return confirm("⚠️ ยืนยันการส่งรายการการแจ้งเบิกอุปกรณ์\nคุณต้องการส่งการร้องขอการแจ้งเบิกอุปกรณ์หรือไม่?\nกด OK เพื่อดำเนินการต่อ");
        }
        function Reject() {
            return confirm("⚠️ ยืนยันการ Reject การแจ้งเบิกอุปกรณ์\nคุณต้องการ Reject การแจ้งเบิกอุปกรณ์หรือไม่?\nกด OK เพื่อดำเนินการต่อ");
        }
        function AADConfirm() {
            return confirm("⚠️ อนุมัติรายการการแจ้งเบิกอุปกรณ์\nคุณต้องการอนุมัติการแจ้งเบิกอุปกรณ์หรือไม่?\nกด OK เพื่อดำเนินการต่อ");
        }
        function confirmRemove() {
            return confirm("⚠️ ลบรายการออกจาก Request List\nคุณต้องการลบรายการออกจาก Request List หรือไม่?\nกด OK เพื่อดำเนินการต่อ");
        }
        function confirmCancel() {
            return confirm("⚠️ ยืนยันการยกเลิกการร้องขอเบิก\nคุณต้องการยกเลิกรายการนี้หรือไม่?\nกด OK เพื่อดำเนินการต่อ");
        }
    </script>

    <%--Count Comment / Reject--%>    
    <script>
        function countChar(val) {
            var len = val.value.length;
            document.getElementById('charCount').innerText = len;
            if (len < 10) {
                document.getElementById('charCount').style.color = 'red';
            } else {
                document.getElementById('charCount').style.color = '#0d5b2b';
            }
        }
        function countChar_Reject(val) {
            var len = val.value.length;
            document.getElementById('charCount_Reject').innerText = len;
            if (len < 10) {
                document.getElementById('charCount_Reject').style.color = 'red';
            } else {
                document.getElementById('charCount_Reject').style.color = '#0d5b2b';
            }
        }
    </script>

    <%--Drop Down Search + สลับ Dropdown / Textbox (Manual) — ชุดเดียวกับ borrow_tool2--%>
    <script>
        /* ═════════ ค้นหาแบบจัดอันดับ (แบบ EditLocation) ═════════ */
        function normalizeText(s) {
            return (s || '').toLowerCase().replace(/[.\,\-_\/]/g, '');
        }
        function isWordStart(rawText, rawTerm) {
            var term = normalizeText(rawTerm).trim();
            if (term === '') return false;
            var words = (rawText || '').toLowerCase().split(/\s+/);
            for (var i = 0; i < words.length; i++)
                if (normalizeText(words[i]).indexOf(term) === 0) return true;
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
            if (data.id === '') return null;
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
        function highlightResult(data) {
            if (!data.id) return data.text;
            var term = $.trim($('.select2-container--open .select2-search__field').val() || '');
            var text = data.text || '';
            if (!term) return text;
            var i = text.toLowerCase().indexOf(term.toLowerCase());
            if (i < 0) return text;
            var $s = $('<span></span>');
            $s.append(document.createTextNode(text.substring(0, i)));
            $s.append($('<b class="s2-hit"></b>').text(text.substr(i, term.length)));
            $s.append(document.createTextNode(text.substring(i + term.length)));
            return $s;
        }
        function select2Opts() {
            return {
                width: 'resolve',
                dropdownAutoWidth: true,
                matcher: rankMatcher,
                sorter: rankSorter,
                templateResult: highlightResult,
                language: { noResults: function () { return 'ไม่พบรายการที่ค้นหา'; } }
            };
        }

        /* ═════════ โหมด Dropdown / Textbox ต่อช่อง ═════════
           ค่าโหมดอยู่ใน HiddenField (hfModelMode / hfCellMode / hfStationMode)
           = แหล่งความจริงเดียว — ทั้งฝั่ง Server และ JS อ่านจากตัวนี้               */
        function applyFieldMode($wrap) {
            var ddl = $wrap.find('select');
            var txt = $wrap.find("input[type='text']");
            var hf = $wrap.find("input[type='hidden']");
            var btn = $wrap.find('.btnToggle');
            if (!ddl.length || !txt.length || !hf.length) return;

            // ล้าง select2 เดิมก่อนเสมอ (กันกล่องซ้อน)
            if (ddl.hasClass('select2-hidden-accessible')) ddl.select2('destroy');

            if (hf.val() === 'txt') {
                ddl.hide();                 // ❌ ไม่ init select2 ในโหมดกรอกเอง
                txt.show();
                btn.text('-');
            } else {
                txt.hide();
                ddl.show();
                ddl.select2(select2Opts());
                btn.text('+');
            }
        }

        function initAllDropdowns() {
            // 1) ช่องที่มีปุ่มสลับ (Model / Cell / Station)
            $('.btnToggle').each(function () {
                applyFieldMode($(this).closest('div'));
            });

            // 2) .ddlSearch อื่นที่ไม่มีปุ่มสลับ (ถ้ามีในอนาคต)
            $('.ddlSearch').each(function () {
                var $d = $(this);
                if ($d.closest('div').find('.btnToggle').length) return;
                if ($d.hasClass('select2-hidden-accessible')) $d.select2('destroy');
                $d.select2(select2Opts());
            });
        }

        // ✅ กันโค้ดเก่าที่ยังเรียกชื่อเดิม
        window.initSelect2 = initAllDropdowns;
        window.restoreModelState = initAllDropdowns;

        // ── กดปุ่ม + / - ──
        $(document).on('click', '.btnToggle', function () {
            if ($(this).is(':disabled')) return;
            var $wrap = $(this).closest('div');
            var hf = $wrap.find("input[type='hidden']");
            hf.val(hf.val() === 'txt' ? 'ddl' : 'txt');
            applyFieldMode($wrap);
            if (hf.val() === 'txt') $wrap.find("input[type='text']").focus();
        });

        // ── เปิด dropdown → โฟกัสช่องค้นหา ──
        $(document).on('select2:open', function () {
            setTimeout(function () {
                var f = document.querySelector('.select2-container--open .select2-search__field');
                if (f) f.focus();
            }, 0);
        });

        // ── โหลดครั้งแรก + หลังทุก Partial Postback (ลงทะเบียนครั้งเดียว) ──
        $(function () {
            initAllDropdowns();
            if (typeof (Sys) !== "undefined" && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initAllDropdowns);
            }
        });
    </script>

    <%--Show & HideLoading--%>
    <script>
        function hideLoading() { $('#loadingOverlay').hide(); }
        function showLoading() { $('#loadingOverlay').show(); }
    </script>

    <%--Check Box--%>
    <script type="text/javascript">
        function toggleFixAssetCheck(clicked) {
            var chkSpare = document.getElementById('<%= chkSpare.ClientID %>');
            var chkAll = document.getElementById('<%= chkAll.ClientID %>');
            if (clicked === chkSpare && chkSpare.checked) {
                chkAll.checked = false;
            }
            if (clicked === chkAll && chkAll.checked) {
                chkSpare.checked = false;
            }
            if (!chkSpare.checked && !chkAll.checked) {
                chkSpare.checked = true;
            }
        }
    </script>

    <%--Close Modal--%>    
    <script>
        function scrollBackToRow(rowIndex) {
            setTimeout(function () {
                var row = document.getElementById('rowSearch_' + rowIndex);
                if (row) {
                    row.scrollIntoView({ behavior: 'smooth', block: 'center' });
                }
            }, 200);
        }
    </script>

    <%--Height Page--%>
    <script>
        function adjustHeight(isFirstLoad) {
            if (isFirstLoad) {
                const params = new URLSearchParams(window.location.search);
                const issue_no = params.get("issue_no");
                if (issue_no === "0") return;
            }
            document.body.style.height = "auto";
            var realHeight = document.documentElement.scrollHeight;
            document.body.style.height = (realHeight + 80) + "px";
        }
        $(document).ready(function () {
            adjustHeight(true);
        });
        if (typeof (Sys) !== "undefined") {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                adjustHeight(false);
            });
        }
    </script>

    <%--Alert : toast--%>
    <style>
         #toast-container { position: fixed; top: 16px; right: 16px; display: flex; flex-direction: column; gap: 8px; z-index: 9999; }
         .toast { min-width: 260px; max-width: 560px; padding: 12px 14px; border-radius: 8px; color: #fff; box-shadow: 0 6px 18px rgba(0,0,0,0.15); display: flex; align-items: start; gap: 10px; animation: slide-in 200ms ease-out; font-family: system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", Arial, "Noto Sans", sans-serif; font-size: 16px; }
         .toast .title { font-weight: 600; margin-bottom: 6px; }
         .toast .message { opacity: 0.95; }
         .toast.success { background: #16a34a; }
         .toast.error { background: #dc2626; }
         .toast.warn { background: #d97706; }
         .toast.info { background: #2563eb; }
         .toast .close { margin-left: auto; background: transparent; border: none; color: inherit; font-size: 18px; cursor: pointer; line-height: 1; }
         @keyframes slide-in { from { opacity: 0; transform: translateY(-10px); } to { opacity: 1; transform: translateY(0); } }
     </style>

    <%--Style Default--%>
    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }
        .page-header { background: linear-gradient(135deg, #1a3a5c 0%, #2980b9 100%); color: #fff; padding: 14px 24px; display: flex; align-items: center; gap: 12px; box-shadow: 0 2px 8px rgba(0,0,0,0.2); }
        .page-header .icon { font-size: 22px; }
        .page-header h1 { font-size: 17px; font-weight: 600; letter-spacing: 0.3px; }
        .page-header .sub { font-size: 11px; opacity: 0.75; margin-top: 2px; }
        .container { max-width: 1280px; margin: 20px auto; padding: 0 16px 40px; }
        .card { background: #fff; border-radius: 8px; box-shadow: 0 1px 6px rgba(0,0,0,0.08); margin-bottom: 12px; overflow: hidden; }
        .card-header { background: #f3e5e5; border-bottom: 2px solid #d0dbe8; padding: 5px 20px 5px 10px; display: flex; align-items: center; gap: 8px; font-weight: 600; font-size: 14px; color: #1a3a5c; }
        .card-header .badge { background: #2980b9; color: #fff; border-radius: 15px; padding: 4px 12px; font-size: 12px; font-weight: 500; }
        .card-body { padding: 10px 10px 0px 10px; }
        .form-row { display: flex; flex-wrap: wrap; gap: 12px; align-items: flex-end; }
        .form-group { display: flex; flex-direction: column; margin-bottom:5px; }
        .form-group label { font-size: 12px; font-weight: 600; color: #555; text-transform: uppercase; letter-spacing: 0.4px; margin-left:5px; margin-bottom:-5px; }
        .form-group input[type="text"], .form-group select { border: 1.5px solid #c8d6e5; border-radius: 5px; padding: 4px 10px; font-size: 14px; color: #2c3e50; background: #fff; transition: border-color 0.2s; height: 28px; }
        .form-group input:focus, .form-group select:focus { outline: none; border-color: #2980b9; box-shadow: 0 0 0 3px rgba(41,128,185,0.12); }
        .form-group input[readonly] { background: #f5f8fb; color: #666; }
        .emp-name-display { font-size: 13px; color: #27ae60; font-weight: 600; min-height: 20px; padding: 4px 0; }
        .emp-name-display.new-emp { color: #e67e22; }
        .status-badge { display: inline-block; padding: 4px 12px; border-radius: 12px; font-size: 14px; font-weight: 700; letter-spacing: 0.5px; }
        .status-draft { background: #fef9e7; color: #b7950b; border: 1px solid #f0d060; }
        .status-approve  { background: #eaf6fb; color: #1a6a9a; border: 1px solid #aad4ea; }
        .status-confirm  { background: #f6e8f9; color: #b42fad; border: 1px solid #c6a0d2; }
        .status-completed  { background: #d9f0de; color: #289b52; border: 1px solid #59a461; }
        .status-cancel  { background: #e7e7e7; color: #4c4c4c; border: 1px solid #636363; }
        .status-borrow  { background: #bebbdc; color: #474187; border: 1px solid #4b3ecf; }
        .btn { border: none; border-radius: 10px; padding: 0px 16px; font-size: 14px; font-weight: 600; cursor: pointer; transition: all 0.18s; display: inline-flex; align-items: center; gap: 5px; height: 34px !important; }
        .btn-primary   { background: #2980b9; color: #fff; }
        .btn-primary:hover   { background: #1a6a9a; }
        .btn-success   { background: #27ae60; color: #fff; }
        .btn-success:hover   { background: #1e8449; }
        .btn-warning   { background: #f39c12; color: #fff; }
        .btn-warning:hover   { background: #d68910; }
        .btn-danger    { background: #e74c3c; color: #fff; }
        .btn-danger:hover    { background: #c0392b; }
        .btn-secondary { background: #7f8c8d; color: #fff; }
        .btn-secondary:hover { background: #626567; }
        .btn-sm { padding: 4px 10px; height: 25px !important; font-size: 11px; }
        .btn:disabled { opacity: 0.5; cursor: not-allowed; }
        .search-tabs { display: flex; gap: 4px; margin-bottom: 12px; border-bottom: 2px solid #d0dbe8; }
        .tbl-wrap { overflow-x: auto; overflow-y: auto; position: relative; border: 1px solid #ddd; }
        table.data-table { width: 100%; border-collapse: collapse; font-size: 12px; }
        table.data-table thead tr { background: #1a3a5c; color: #fff; }
        table.data-table thead th { padding: 9px 10px; text-align: left; font-weight: 600; white-space: nowrap; letter-spacing: 0.3px; }
        table.data-table tbody tr { border-bottom: 1px solid #e8eef4; }
        table.data-table tbody tr:hover { background: #f0f7ff; }
        table.data-table tbody td { padding: 8px 10px; vertical-align: middle; border-bottom:1px solid #d5d5d5; }
        table.data-table tbody tr:nth-child(even) { background: #f8fbfe; }
        table.data-table tbody tr:nth-child(even):hover { background: #f0f7ff; }
        .no-data { text-align: center; color: #aaa; padding: 24px; font-style: italic; }
        .change-table input[type="text"] { width: 100%; border: 1.5px solid #c8d6e5; border-radius: 4px; padding: 4px 7px; font-size: 12px; min-width: 90px; }
        .change-table input:focus { outline: none; border-color: #2980b9; }
        .draft-banner { background: #fffbea; border: 1.5px dashed #f0d060; border-radius: 6px; padding: 5px 14px; margin-bottom: 10px; display: flex; align-items: center; gap: 10px; font-size: 13px; color: #7d6608; }
        .draft-banner strong { color: #b7950b; }
        .action-bar { position: fixed; bottom: 10px; right: 10px; background: #dbc8c8d6; border-top: 1px solid var(--border); box-shadow: 0 -4px 20px rgba(0,0,0,.08); padding: 10px 24px; display: flex; align-items: center; justify-content: space-between; gap: 12px; z-index: 100; border-radius:10px; }
        .action-bar-left  { display: flex; gap: 10px; align-items: center; }
        .action-bar-right { display: flex; gap: 8px; }
        .alert { border-radius: 6px; padding: 9px 14px; font-size: 12px; margin-bottom: 12px; display: none; }
        .alert-info    { background: #eaf6fb; color: #1a6a9a; border-left: 4px solid #2980b9; }
        .alert-success { background: #eafaf1; color: #1e8449; border-left: 4px solid #27ae60; }
        .alert-danger  { background: #fdedec; color: #922b21; border-left: 4px solid #e74c3c; }
        .sep { height: 1px; background: #e8eef4; margin: 12px 0; }
        @media (max-width: 768px) { .form-row { flex-direction: column; } .action-bar { flex-direction: column; } }
    </style>

    <%-- modal Picture--%>
    <style>
        .modal.fade { background: fixed; border:none; box-shadow:none; left:10%; width:auto; }
        h4 { color: aliceblue; }  
        .modal-body{ max-height : 800px; }
    </style>

    <%--Loading overlay--%>
    <style>
        .loading { height: 100vh; width: 100%; display: flex; align-items: center; justify-content: center; background: hsl(220deg 29% 90% / 50%); flex-direction:column; text-align:center; }
        .pl { width: 6em; height: 6em; }
        .pl__ring { animation: ringA 2s linear infinite; }
        .pl__ring--a { stroke: #f42f25; }
        .pl__ring--b { animation-name: ringB; stroke: #f49725; }
        .pl__ring--c { animation-name: ringC; stroke: #255ff4; }
        .pl__ring--d { animation-name: ringD; stroke: #f42582; }
        @keyframes ringA { from, 4% { stroke-dasharray: 0 660; stroke-width: 20; stroke-dashoffset: -330; } 12% { stroke-dasharray: 60 600; stroke-width: 30; stroke-dashoffset: -335; } 32% { stroke-dasharray: 60 600; stroke-width: 30; stroke-dashoffset: -595; } 40%, 54% { stroke-dasharray: 0 660; stroke-width: 20; stroke-dashoffset: -660; } 62% { stroke-dasharray: 60 600; stroke-width: 30; stroke-dashoffset: -665; } 82% { stroke-dasharray: 60 600; stroke-width: 30; stroke-dashoffset: -925; } 90%, to { stroke-dasharray: 0 660; stroke-width: 20; stroke-dashoffset: -990; } }
        @keyframes ringB { from, 12% { stroke-dasharray: 0 220; stroke-width: 20; stroke-dashoffset: -110; } 20% { stroke-dasharray: 20 200; stroke-width: 30; stroke-dashoffset: -115; } 40% { stroke-dasharray: 20 200; stroke-width: 30; stroke-dashoffset: -195; } 48%, 62% { stroke-dasharray: 0 220; stroke-width: 20; stroke-dashoffset: -220; } 70% { stroke-dasharray: 20 200; stroke-width: 30; stroke-dashoffset: -225; } 90% { stroke-dasharray: 20 200; stroke-width: 30; stroke-dashoffset: -305; } 98%, to { stroke-dasharray: 0 220; stroke-width: 20; stroke-dashoffset: -330; } }
        @keyframes ringC { from { stroke-dasharray: 0 440; stroke-width: 20; stroke-dashoffset: 0; } 8% { stroke-dasharray: 40 400; stroke-width: 30; stroke-dashoffset: -5; } 28% { stroke-dasharray: 40 400; stroke-width: 30; stroke-dashoffset: -175; } 36%, 58% { stroke-dasharray: 0 440; stroke-width: 20; stroke-dashoffset: -220; } 66% { stroke-dasharray: 40 400; stroke-width: 30; stroke-dashoffset: -225; } 86% { stroke-dasharray: 40 400; stroke-width: 30; stroke-dashoffset: -395; } 94%, to { stroke-dasharray: 0 440; stroke-width: 20; stroke-dashoffset: -440; } }
        @keyframes ringD { from, 8% { stroke-dasharray: 0 440; stroke-width: 20; stroke-dashoffset: 0; } 16% { stroke-dasharray: 40 400; stroke-width: 30; stroke-dashoffset: -5; } 36% { stroke-dasharray: 40 400; stroke-width: 30; stroke-dashoffset: -175; } 44%, 50% { stroke-dasharray: 0 440; stroke-width: 20; stroke-dashoffset: -220; } 58% { stroke-dasharray: 40 400; stroke-width: 30; stroke-dashoffset: -225; } 78% { stroke-dasharray: 40 400; stroke-width: 30; stroke-dashoffset: -395; } 86%, to { stroke-dasharray: 0 440; stroke-width: 20; stroke-dashoffset: -440; } }
    </style>

    <%--Select2 : Dropdown Search (แบบ EditLocation / ชุดเดียวกับ borrow_tool2)--%>
    <style>
        /* กล่องที่ปิดอยู่ : ข้อความที่เลือก "อยู่กลาง" */
        .select2-container--default .select2-selection--single {
            height: 28px;
            font-size: 12px;
            border: 1px solid #ced4da;
            border-radius: 5px;
        }
        .select2-container--default .select2-selection--single .select2-selection__rendered {
            line-height: 26px;
            text-align: center;
            padding-left: 8px;
            padding-right: 20px;
        }
        .select2-container--default .select2-selection--single .select2-selection__arrow { height: 26px; }

        /* dropdown ที่เปิด : รายการ "ชิดซ้าย" อ่านง่าย */
        .select2-dropdown { border-color: var(--nv-600, #2980b9) !important; }
        .select2-results { text-align: left; }
        .select2-container--default .select2-results__option {
            font-size: 12px;
            text-align: left;
            padding: 5px 10px;
        }
        .select2-container--default .select2-results__option--highlighted[aria-selected] {
            background-color: var(--nv-600, #2980b9) !important;
        }
        /*.select2-results__option--highlighted .s2-hit { color: #fff; }*/

        /* กล่องค้นหา */
        .select2-container--default .select2-search--dropdown .select2-search__field {
            font-size: 13px;
            text-align: left;
            height: 28px;
            border: 1px solid #c8d6e5 !important;
            border-radius: 5px;
            background: #e7f2fb;
        }

        /* คำที่ตรงกับคำค้น */
        /*.s2-hit {
            font-weight: 700;
            color: #b4412f;
            background: #fff3cd;
            border-radius: 2px;
        }*/

        /* คำที่ตรงกับคำค้น : แถวปกติ → ตัวแดงเข้ม + พื้นเหลืองอ่อน */
        .s2-hit {
            font-weight: 700;
            color: #b4412f;
            background: #fff3cd;
            border-radius: 2px;
            padding: 0 1px;
        }

        /* แถวที่ hover / เลือก (พื้นน้ำเงิน) → ตัดพื้นเหลืองทิ้ง ใช้ตัวขาว + ขีดเส้นใต้แทน */
        .select2-container--default .select2-results__option--highlighted[aria-selected] .s2-hit {
            color: #fff !important;
            background: transparent !important;
            text-decoration: underline;
            text-underline-offset: 2px;
        }
    </style>

    <style>
        .badge { display: inline-block; padding: 4px 10px; font-size: 12px; font-weight: 600; border-radius: 12px; color: #fff; }
        .badge-normal { background-color: #28a745; }
        .badge-spare { background-color: #ffc107; color: #000; }
        .badge-borrowing { background-color: #dc3545; color: #fff; }
        .badge-issue{ background: #b7cfe9bd; border: 1px solid #2749d7; border-radius: 10px; padding: 2px 10px; }
    </style>

    <style>
        .grid-headerTopic { display: grid; grid-template-columns: 70px auto 180px 130px 40px; grid-template-rows: 50px; background-color:#ffa6a6; border-radius:10px; margin: 10px 10px 10px 10px; }
        .grid_search{ display: grid; grid-template-columns: 320px Auto; gap: 5px; }
        .grid_search2{ display: grid; grid-template-rows: 30% Auto; }
        .grid_footer{ display: grid; grid-template-columns: 140px Auto 130px 170px; gap: 4px; }
        .header_Style1 { border-radius: 10px; align-content: center; font-size: 30px; margin-bottom:0px; }
        #IssueDatePicker .input-group-addon { width: 25%; }
        #IssueDatePicker.dp-compact .input-group-addon { height: 28px !important; padding: 0 8px !important; line-height: 30px !important; display: inline-block; vertical-align: middle; }
        .headerGv1 { color: #ffffff; font-weight: 800; background-color: #5e5e5e; padding: 2px 4px; text-align:center; }
        .headerGv2 { color: #ffffff; font-weight: 800; background-color: #3b753ae3; padding: 2px 4px; text-align:center; }
        .headerGvOld { color: #5f5a5a; font-weight: 800; background-color: #e4e366; padding: 2px 4px; text-align:center; }
        .rowGv1 { font-size: 12px; padding: 2px 4px; background-color: #ffffff; text-align:center; }
        .rowGv2 { font-size: 12px; padding: 2px 4px; background-color: #ffffff; text-align:left; }
        .rowGv_toolNo{ font-size: 12px; padding: 2px 4px; background-color: #ffffff; text-align:center; min-width:140px; }
        .img_tool { display: inline-block; width: 25px; }        
        .fixed-header th { position: sticky; top: 0; z-index: 3; border-bottom: 2px solid #ccc; }        
        .pager-top-right { position: sticky; top: 0; z-index: 4; background: white; text-align: right; padding: 5px 10px; border-bottom: 1px solid #ccc; }        
        .grid-pager { text-align: right; }
        .page-note { padding: 5px 10px; display: flex; flex-wrap: wrap; align-items: center; gap: 10px; }
        .page-note .badge { color: #fff; border-radius: 6px; padding: 4px 10px; font-size: 12px; font-weight: 500; text-shadow:none; }
        .ddl_edit{ font-size: 12px; text-align: center; }      
        .ddl-warning { background-color: #fff3cd; border: 1px solid #ffc107; }        
        .disabled-dropdown { background-color: #e9ecef; pointer-events: none; }
        select.ddl-warning + .select2-container .select2-selection--single { border: 2px solid #ffc107 !important; background-color: #fff3cd !important; }
        .ddl-warning + .select2-container > .select2-selection { border: 2px solid #ffc107 !important; background-color: #fff3cd !important; }
    </style>

    <%--tooltip--%>
    <style>
        .tooltip-wrap { position: relative; display: inline-block; }
        .tooltip-wrap .tooltip-box { visibility: hidden; opacity: 0; width: max-content; max-width: 220px; background: #2c3e50; color: #fff; text-align: center; border-radius: 6px; padding: 7px 12px; font-size: 12px; white-space: nowrap; position: absolute; z-index: 9999; bottom: 125%; left: 50%; transform: translateX(-50%) translateY(4px); transition: opacity .2s ease, transform .2s ease; box-shadow: 0 4px 12px rgba(0,0,0,.25); pointer-events: none; }
        .tooltip-wrap .tooltip-box::after { content: ""; position: absolute; top: 100%; left: 50%; margin-left: -6px; border-width: 6px; border-style: solid; border-color: #2c3e50 transparent transparent transparent; }
        .tooltip-wrap:hover .tooltip-box { visibility: visible; opacity: 1; transform: translateX(-50%) translateY(0); }
    </style>

    <%--style column gvBorrowlist (zone divider)--%>
    <style>
        .zone-divider { border-left: 4px solid #1a3a5c !important; }
        .col-current { background-color: #f1f3f5 !important; color: #555; font-size: 12px; text-align: center; }
        .col-use { background-color: #eafaf1 !important; text-align: center; }
        .hdr-current { background-color: #6c757d !important; color: #fff !important; font-weight: 700; text-align: center; padding: 4px; }
        .hdr-use { background-color: #27ae60 !important; color: #fff !important; font-weight: 700; text-align: center; padding: 4px; }
        .hdr-use.zone-divider { border-left: 4px solid #1a3a5c !important; }

        /* Step 3 : เส้นแบ่งโซน — ขวาสุดของ Use (ปิดโซน) */
        .zone-end { border-right: 4px solid #1a3a5c !important; }

        /* Step 3 : Fix คอลัมน์แนว X ( # , Fix Asset , Control No , Tool No / Model , No )
           ค่า left คำนวณด้วย JS (fixStickyColumns) ตามความกว้างจริงของคอลัมน์ก่อนหน้า
           Remove / Detail / Pic ไม่ Fix → เลื่อนลอดใต้คอลัมน์ที่ Fix */
        /* GridView ใส่ style="border-collapse:collapse" แบบ inline → ต้อง !important
           (collapse = เส้นขอบเป็นของตาราง ไม่ติดไปกับคอลัมน์ที่ Fix → เส้นขวาของ No เลื่อนหาย / มีขีดทับ) */
        table.fx-table { border-collapse: separate !important; border-spacing: 0; }
        table.fx-table td.fx-col { position: sticky; z-index: 2; background-color: #ffffff; }
        table.fx-table th.fx-col { position: sticky; top: 0; z-index: 6; }     /* มุมซ้ายบน : อยู่เหนือทั้งแถวและคอลัมน์ */
        /* Step 3 : เส้นขอบสีดำทั้งหมด ขนาดเท่ากัน 1px — ระหว่างรายการ = เส้นประ / แบ่งโซน = เส้นทึบ */
        /* GridView ใส่ rules="all" border="1" → ทุกเซลล์มีเส้น 4 ด้าน , แบบ separate เส้นติดกันจะซ้อนเป็น 2px
           → ให้แต่ละเซลล์วาดแค่ ขวา + ล่าง (ซ้าย/บน ให้กรอบนอกวาด) = เส้นเดียว 1px ทุกเส้น */
        .step3-wrap { border: 1px solid #000 !important; }
        table.fx-table { border: 0 !important; }
        table.fx-table th, table.fx-table td {
            border-top: 0 !important; border-left: 0 !important;
            border-right: 1px solid #000 !important;
        }
        table.fx-table th { border-bottom: 1px solid #000 !important; }
        table.fx-table td { border-bottom: 1px dashed #000 !important; }
        table.fx-table tr > :last-child { border-right: 0 !important; }               /* ขวาสุด → ใช้กรอบนอก */
        table.fx-table tr:last-child > td { border-bottom: 0 !important; }            /* ล่างสุด → ใช้กรอบนอก */
        table.fx-table .zone-divider { border-left: 0 !important; }                   /* ใช้เส้นขวาของคอลัมน์ก่อนหน้าแทน */
        table.fx-table td.fx-toolno { max-width: 220px; white-space: normal; word-break: break-word; }
    </style>

    <%--READ ME + MANUAL--%>
    <style>
        .manual-btn { display:inline-flex; align-items:center; gap:5px; background:#fff; color:#c0392b; font-weight:600; font-size:13px; padding:5px 12px; border-radius:20px; text-decoration:none; margin:0 6px; transition:all .15s; cursor:pointer; }
        .manual-btn:hover { background:#c0392b; color:#fff; text-decoration:none; }
        .manual-menu { display:none; position:absolute; top:100%; right:0; margin-top:4px; background:#fff; border:1px solid #d0dbe8; border-radius:8px; box-shadow:0 4px 16px rgba(0,0,0,.15); z-index:2000; min-width:170px; overflow:hidden; }
        .manual-menu a, .manual-menu .aspNetDisabled, .manual-menu a[id] { display:flex; align-items:center; gap:8px; padding:9px 14px; font-size:13px; color:#2c3e50; text-decoration:none; }
        .manual-menu a:hover { background:#fdecea; color:#c0392b; }
        .manual-menu.show { display:block; }
        .readme-btn { font-size:24px; color:#4f4b4b; cursor:pointer; transition:color .15s, transform .15s; }
        .readme-btn:hover { color:#c0392b; transform:scale(1.1); }

        #modal_ReadMe .modal-dialog { width:auto; max-width:640px; margin:40px auto; }
        #modal_ReadMe .modal-content { border-radius:10px; overflow:hidden; }
        #modal_ReadMe .modal-header { background:linear-gradient(135deg,#1a3a5c 0%,#c0392b 100%); border-bottom:none; padding:12px 18px; }
        #modal_ReadMe .modal-header .close { color:#fff; opacity:.9; text-shadow:none; font-size:26px; }
        #modal_ReadMe .modal-title { color:#fff !important; font-size:17px; font-weight:700; }
        #modal_ReadMe .modal-body { padding:18px 22px; font-size:13px; color:#2c3e50; background:#fff; max-height:none; }
        #modal_ReadMe .rm-item { display:flex; align-items:flex-start; gap:10px; padding:8px 0; border-bottom:1px solid #eef0f4; }
        #modal_ReadMe .rm-item:last-child { border-bottom:none; }
        #modal_ReadMe .rm-ico { width:30px; height:30px; flex-shrink:0; display:flex; align-items:center; justify-content:center; background:#fdecea; border-radius:6px; color:#c0392b; font-size:14px; }
        #modal_ReadMe .rm-txt strong { color:#1a3a5c; display:block; margin-bottom:2px; }
        #modal_ReadMe .rm-txt span { color:#6b7a99; }

        #modal_UploadManual .modal-dialog { width:auto; max-width:480px; margin:60px auto; }
        #modal_UploadManual .modal-header { background:linear-gradient(135deg,#1a3a5c 0%,#f39c12 100%); border-bottom:none; }
        #modal_UploadManual .modal-title { color:#fff !important; }
        #modal_UploadManual .modal-body { background:#fff; max-height:none; padding:18px; }

        /* เมนู Manual แยกกลุ่ม (เหมือน borrow_tool2) */
        .manual-menu { min-width:210px; }
        .manual-menu .manual-sep { height:1px; background:#edf0f4; margin:4px 0; }
        .manual-menu .manual-grp { padding:6px 14px 2px; font-size:11px; color:#66727f; font-weight:700; }

        /* แถบนำทาง Status 50 : AAD Confirm */
        .mode-guide {
            display:flex; align-items:center; flex-wrap:wrap; gap:10px 18px;
            margin:0 10px 12px; padding:10px 16px; border-radius:8px;
            background:#fdecea; border:1px solid #f1b8b1; border-left:6px solid #c0392b; color:#1a3a5c;
        }
        .mode-guide .mg-title { font-weight:700; }
        .mode-guide ol { display:flex; flex-wrap:wrap; gap:4px 18px; margin:0; padding-left:18px; font-size:13px; }
        .mode-guide .mg-btn {
            margin-left:auto; display:inline-flex; align-items:center; gap:5px;
            padding:5px 14px; border-radius:16px; background:#c0392b; color:#fff !important;
            font-weight:600; font-size:13px; text-decoration:none !important;
        }
        .mode-guide .mg-btn:hover { background:#a93226; }
    </style>

    <%--AAD Confirm Modal (อ้างอิง NotificationOfChange)--%>
    <style>
        .wcf-overlay {
            position:fixed; inset:0; z-index:10000;
            background:rgba(20,30,42,.45);
            align-items:center; justify-content:center;
        }
        .wcf-box {
            display:flex; flex-direction:column;
            width:95vw; max-height:90vh;
            background:#fff; border-radius:12px; overflow:hidden;
            box-shadow:0 20px 50px rgba(20,30,42,.3);
        }
        .wcf-head {
            display:flex; align-items:center; justify-content:space-between; gap:12px;
            padding:14px 20px; border-bottom:1px solid #edf0f4;
            font-size:15px; font-weight:700; color:#1c2b3a;
        }
        .wcf-head .wcf-doc { color:#c0392b; }
        .wcf-x {
            width:32px; height:32px; border-radius:6px;
            background:none; border:none; color:#66727f; font-size:24px; line-height:1; cursor:pointer;
        }
        .wcf-x:hover { background:#f3f5f8; color:#1c2b3a; }
        .wcf-body { padding:16px 20px; overflow-y:auto; min-height:45vh; }
        .wcf-foot { display:flex; justify-content:flex-end; gap:8px; padding:12px 20px; border-top:1px solid #edf0f4; }
        .wcf-sec { display:flex; align-items:center; gap:8px; margin:0 0 8px; font-size:14px; font-weight:700; color:#1c2b3a; }
        .wcf-sec small { font-size:12px; font-weight:400; color:#66727f; }
        .wcf-summary { display:flex; flex-wrap:wrap; gap:8px; align-items:center; margin-bottom:10px; font-size:13px; }
        .wcf-badge { display:inline-flex; align-items:center; gap:4px; padding:2px 10px; border-radius:999px; font-size:11.5px; font-weight:700; border:1px solid transparent; }
        .wcf-ok { background:#e3f4e8; color:#16703d; border-color:#b6e0c4; }
        .wcf-notice {
            margin-top:10px; padding:8px 12px; border-left:3px solid #d9822b; border-radius:6px;
            background:#fdf3e7; color:#8a4b0c; font-size:13px;
        }

        /* โซนคอลัมน์ : AAD (เหลือง) / ปลายทาง (เขียว) / ปัจจุบัน (เทา) */
        table.wcf-table thead th.hdr-aad, table.wcf-table th.hdr-aad { background:#c79100; color:#fff; }
        table.wcf-table th.hdr-use     { background:#16884a; color:#fff; }
        table.wcf-table th.hdr-current { background:#4a5b6d; color:#fff; }
        table.wcf-table tbody td.col-aad, table.wcf-table td.col-aad { background:#f8eefb; }
        table.wcf-table td.col-use     { background:#eef8f1; }
        table.wcf-table td.col-current { background:#f3f5f8; color:#66727f; }
        table.wcf-table .zone-divider  { border-left:3px solid #1a3a5c; }

        table.wcf-table td.col-aad select.aad-ddl {
            min-width:180px; height:30px; padding:2px 6px;
            background:#fff3cd !important; border:2px solid #ffc107 !important;
            border-radius:6px; font-size:12.5px; font-weight:700; color:#6b4d00;
        }
        table.wcf-table td.col-aad select.aad-ddl:focus { outline:none; box-shadow:0 0 0 3px rgba(255,193,7,.35); }
        /* ข้อมูล Requester จาก Snapshot (Requester_Info) เมื่อไม่พบใน HRMS */
        input.fld-snapshot { background:#fff4e0 !important; border-color:#f5c26b !important; color:#7a4f00 !important; }

        /* Use : Dept (Real Dept) */
        table.wcf-table .use-dept .ud-code { white-space:nowrap; }
        table.wcf-table .use-dept small { color:#66727f; }

        /* หมายเหตุกฎ Auto Assign ใต้ Dropdown ผู้รับผิดชอบ */
        table.wcf-table .rule-note { margin-top:4px; font-size:11.5px; color:#66727f; white-space:normal; max-width:260px; }
        table.wcf-table .rule-note.hit { color:#7a4f00; background:#fff4e0; border:1px solid #f5c26b; border-radius:4px; padding:2px 6px; }
        table.wcf-table td.col-aad select.aad-ddl-empty {
            background:#fbeceb !important; border-color:#c93b3b !important; color:#a82f2f;
        }

        .cfm-warn {
            display:flex; gap:12px; align-items:flex-start;
            margin:0 0 12px; padding:12px 16px;
            background:#fff7e0; border:1px solid #f1d98c; border-left:6px solid #d99a06;
            border-radius:10px; font-size:13.5px; color:#5c4300;
        }
        .cfm-warn .cfm-warn-ico { font-size:22px; line-height:1; }
        .cfm-warn b.cfm-warn-title { display:block; font-size:15px; color:#946200; margin-bottom:4px; }
        .cfm-warn ul { margin:4px 0 0; padding-left:18px; }
        .cfm-warn .cfm-chip {
            display:inline-block; padding:0 8px; border-radius:4px;
            background:#fff3cd; border:1px solid #ffc107; font-weight:700; color:#6b4d00;
        }
    </style>

    <%--Manual open / menu--%>
    <script>
        // path ผ่าน Virtual Directory /ManualPath (& ต้อง encode = %26)
        // → \\cptfile02\Pro\PRJ2023008\50_Flow_Manual\01_Manual\08_Tool&Equipment\03_Withdraw
        var MANUAL_BASE = '/Tool_Equipment_Control_System/ManualPath/08_Tool%26Equipment/03_Withdraw/';
        var MANUAL_FILES = ['Step1', 'Step2', 'Step3', 'Step4', 'SaveDraft', 'SendRequest', 'CancelRequest', 'Confirm', 'Reject'];

        function openManualFile(name) {
            if (MANUAL_FILES.indexOf(name) < 0) return false;          // whitelist
            // ?v= กัน browser cache ไฟล์เก่าหลัง Upload ใหม่
            window.open(MANUAL_BASE + name + '.pdf?v=' + new Date().getTime(), '_blank');
            hideManualMenu();
            return false;                                               // กัน postback / href="#"
        }
        function hideManualMenu() {
            var m = document.getElementById('manualMenu');
            if (m) m.classList.remove('show');
        }
        function manualClick(e) {
            if (e) e.stopPropagation();
            var m = document.getElementById('manualMenu');
            if (m) m.classList.toggle('show');
            return false;
        }
        // ── Modal AAD Confirm ──
        function closeConfirmModal() {
            var m = document.getElementById('<%= pnlConfirmModal.ClientID %>');
            if (m) m.style.display = 'none';
        }
        // เตือนให้ตรวจ Status Use / ผู้รับผิดชอบ ก่อนบันทึก
        function cfmConfirm() {
            var grid = document.getElementById('<%= gvConfirmSummary.ClientID %>');
            var ddls = grid ? grid.querySelectorAll('select.aad-ddl:not([disabled])') : [];
            var missing = 0;
            for (var i = 0; i < ddls.length; i++) if (!ddls[i].value) missing++;

            if (missing > 0) {
                var msg = 'ยังมีช่อง Status Use / ผู้รับผิดชอบ ที่ไม่มีข้อมูล ' + missing + ' ช่อง (ขอบแดง)';
                if (typeof showToast === 'function') showToast(msg, { title: 'ข้อมูลไม่ครบ', type: 'warn', duration: 8000 });
                else alert(msg);
                return false;
            }
            if (!confirm('⚠️ ยืนยัน AAD Confirm\n\nตรวจสอบ Status Use และ ผู้รับผิดชอบ ครบทุกรายการแล้วใช่หรือไม่?\n' +
                'เมื่อยืนยันแล้วจะแก้ไขไม่ได้\n\nกด OK เพื่อดำเนินการต่อ')) return false;

            document.getElementById('loadingOverlay').style.display = 'block';
            return true;
        }
        // เปลี่ยนค่าแล้วเอาขอบแดงออกทันที
        document.addEventListener('change', function (e) {
            var s = e.target;
            if (s && s.classList && s.classList.contains('aad-ddl'))
                s.classList.toggle('aad-ddl-empty', !s.value);

            // ผู้รับผิดชอบเปลี่ยน → Use : Dept แสดง (Real Dept) ของคนที่เลือก
            if (s && s.getAttribute && s.getAttribute('data-real') !== null) {
                var map = {};
                try { map = JSON.parse(s.getAttribute('data-real') || '{}'); } catch (err) { }
                var dept = s.getAttribute('data-dept') || '';
                var real = map[s.value] || '';
                var text = (!real) ? dept : (!dept ? real : (dept === real ? dept : dept + ' (' + real + ')'));
                var tr = s.parentNode;
                while (tr && tr.tagName !== 'TR') tr = tr.parentNode;
                var code = tr ? tr.querySelector('.use-dept .ud-code') : null;
                if (code) code.textContent = text;
            }
        }, true);

        // คลิกที่อื่น → ปิดเมนู
        document.addEventListener('click', function (e) {
            var wrap = document.getElementById('manualMenu');
            var btn = document.querySelector('.manual-btn');
            if (!wrap) return;
            if (wrap.contains(e.target) || (btn && btn.contains(e.target))) return;
            wrap.classList.remove('show');
        });
    </script>

    <%--=====================================================================
    WITHDRAW — THEME OVERRIDE  (โครงเดียวกับ borrow_tool2 / โทนแดง #c0392b ของหน้าเบิกเดิม)
    ────────────────────────────────────────────────────────────────
    วิธีใช้ : วางทั้งบล็อกนี้ไว้ "ท้ายสุด" ของกลุ่ม <style> ในหน้า
             (ต่อจาก <style> ก้อนสุดท้าย ก่อน <asp:ScriptManager>)
    ผลลัพธ์ : ความสว่าง/ความนุ่มของสีเท่ากับ borrow_tool2 แต่ส่วนหัว/การ์ด/แถบปุ่ม ใช้โทนแดงของหน้าเบิก
             (ปุ่ม / สถานะ / ตาราง / toast ใช้สี semantic ชุดเดียวกับ borrow)
    ย้อนกลับ: ลบบล็อกนี้ทิ้ง หน้าจะกลับเป็นสีเดิมทันที
    =====================================================================--%>
    <style>
        :root {
            /* Navy scale */
            --nv-900:#12293f; --nv-800:#1a3a5c; --nv-700:#245177; --nv-600:#2980b9;
            --nv-500:#4a90c2; --nv-200:#aecbe6; --nv-100:#dbe7f2; --nv-050:#eef4fa;
            /* Rose scale : โทนแดงของหน้าเบิก (เดิม #ffa6a6 / #f3e5e5 / #dbc8c8 / #c0392b) แต่นุ่มลง */
            --rs-900:#5c1f1a; --rs-800:#7d2a22; --rs-700:#a5342a; --rs-600:#c0392b;
            --rs-200:#f6cbc6; --rs-100:#fae3e0; --rs-050:#fdf4f3; --rs-line:#efd3cf;
            /* input */
            --field-focus-bg:#e7f2fb; --field-focus-ring:rgba(41,128,185,.18);
            /* neutral */
            --text:#2c3e50; --text-sub:#5b6b7d; --border:#d0dbe8; --border-soft:#e3ebf3;
            --surface:#ffffff; --surface-2:#f7fafd;
            /* semantic */
            --ok:#2e9e5b;     --ok-bg:#e6f6ec;     --ok-line:#b7e2c6;
            --warn:#d98a1f;   --warn-bg:#fdf3e2;   --warn-line:#f2d9a8;
            --danger:#d64b3d; --danger-bg:#fbeceb; --danger-line:#f2c2bd;
            --info:#2980b9;   --info-bg:#eaf3fb;   --info-line:#bcd9ef;
            /* amber soft (แทนเหลืองสะท้อน) */
            --amber-soft-bg:#fdf6e3; --amber-soft-line:#ecd9a6; --amber-soft-text:#7a5b16;
            /* shadow */
            --sh-1:0 1px 4px rgba(26,58,92,.08); --sh-2:0 4px 14px rgba(26,58,92,.12);
        }

        /* ===== HEADER ===== */
        .page-header{ background:linear-gradient(135deg,var(--rs-800) 0%,var(--rs-600) 100%) !important; }
        .grid-headerTopic{ background-color:var(--rs-200) !important; }          /* เดิม #ffa6a6 */
        .header_Style1{ color:var(--rs-900) !important; }

        /* ===== CARD ===== */
        .card{ box-shadow:var(--sh-1) !important; }
        .card-header{
            background:var(--rs-050) !important;                                 /* เดิม #f3e5e5 */
            border-bottom:1px solid var(--rs-line) !important;
            color:var(--nv-800) !important;
        }
        .card-header .badge{ background:var(--nv-600) !important; color:#fff !important; }

        /* ===== FORM / INPUT ===== */
        .form-group input:focus,
        .form-group select:focus{
            border-color:var(--nv-600) !important;
            box-shadow:0 0 0 3px var(--field-focus-ring) !important;
        }
        .form-group input[readonly]{ background:var(--surface-2) !important; }
        /* กล่อง input ที่เดิมใช้ #00a7ff40 → ทำเป็นโทนฟ้าเดียวกัน (คุมด้วย attribute) */
        input[style*="00a7ff40"]{ background-color:var(--field-focus-bg) !important; }

        /* ===== BUTTONS ===== */
        .btn-primary{ background:var(--nv-600) !important; }
        .btn-primary:hover{ background:var(--nv-700) !important; }
        .btn-success{ background:var(--ok) !important; }
        .btn-success:hover{ background:#25824a !important; }
        .btn-warning{ background:var(--warn) !important; }
        .btn-warning:hover{ background:#c07a17 !important; }
        .btn-danger{ background:var(--danger) !important; }
        .btn-danger:hover{ background:#c0392f !important; }
        .btn-secondary{ background:#7c8896 !important; }
        .btn-secondary:hover{ background:#68727e !important; }

        /* ===== STATUS BADGE ===== */
        .status-draft{     background:var(--warn-bg) !important; color:var(--warn) !important; border:1px solid var(--warn-line) !important; }
        .status-approve{   background:var(--info-bg) !important; color:var(--nv-700) !important; border:1px solid var(--info-line) !important; }
        .status-confirm{   background:#f2e9f7 !important; color:#8b3fa0 !important; border:1px solid #d8b8e4 !important; }
        .status-completed{ background:var(--ok-bg) !important; color:var(--ok) !important; border:1px solid var(--ok-line) !important; }
        .status-cancel{    background:#ececec !important; color:#4c4c4c !important; border:1px solid #cfcfcf !important; }
        .status-borrow{    background:#dbe4f5 !important; color:var(--nv-700) !important; border:1px solid #b9c8e6 !important; }

        /* ===== SEARCH TABS ===== */
        .search-tab.active{ color:var(--nv-600) !important; border-bottom-color:var(--nv-600) !important; }
        .search-tab:hover{ color:var(--nv-600) !important; }

        /* ===== TABLES ===== */
        table.data-table thead tr{ background:var(--nv-800) !important; }
        table.data-table tbody tr:hover{ background:var(--nv-100) !important; }
        table.data-table tbody tr:nth-child(even){ background:var(--surface-2) !important; }
        table.data-table tbody tr:nth-child(even):hover{ background:var(--nv-100) !important; }

        /* ===== ZONE : Use / Current ===== */
        .hdr-use{ background-color:var(--ok) !important; }
        .col-use{ background-color:var(--ok-bg) !important; }
        .hdr-current{ background-color:var(--nv-700) !important; }
        .col-current{ background-color:var(--nv-050) !important; color:var(--text-sub) !important; }
        .zone-divider{ border-left:4px solid var(--nv-800) !important; }
        .hdr-use.zone-divider{ border-left:4px solid var(--nv-800) !important; }

        /* ===== ALERT ===== */
        .alert-info{    background:var(--info-bg) !important; color:var(--nv-700) !important; border-left:4px solid var(--nv-600) !important; }
        .alert-success{ background:var(--ok-bg) !important;  color:#1e7a45 !important;      border-left:4px solid var(--ok) !important; }
        .alert-danger{  background:var(--danger-bg) !important; color:#8f2b22 !important;   border-left:4px solid var(--danger) !important; }

        /* ===== ACTION BAR ===== */
        .action-bar{
            background:rgba(246,203,198,.72) !important;                        /* โทน --rs-200 (เดิม #dbc8c8d6) */
            box-shadow:var(--sh-2) !important;
            border-top:1px solid var(--rs-line) !important;
        }
        .status-dot{ background:var(--nv-500) !important; }

        /* ===== NOTE : เหลืองอำพันนุ่ม (แทน #e4ff5d) ===== */
        /* badge เหลืองสะท้อนใน note → อำพันนุ่ม */
        .page-note .badge[style*="e4ff5d"]{
            background:var(--amber-soft-bg) !important;
            color:var(--amber-soft-text) !important;
            border:1px solid var(--amber-soft-line) !important;
            text-shadow:none !important;
        }
        /* badge หัวข้อ "หมายเหตุ / ตัวอย่าง" สีแดง → โทน danger เดียวกัน */
        .page-note .badge[style*="ff5d5d"],
        .page-note .badge[style*="ff5d5dd4"]{
            background:var(--danger) !important;
        }

        /* ===== BADGE (Spare / Normal / Borrowing) ปรับให้กลมกลืน ===== */
        .badge-normal{ background-color:var(--ok) !important; }
        .badge-spare{ background-color:#e9b949 !important; color:#3d2f00 !important; }
        .badge-borrowing{ background-color:var(--danger) !important; }
        .badge-issue{
            background:#dbe7f2 !important;
            border:1px solid var(--nv-600) !important;
        }

        /* ===== SELECT2 : กล่องค้นหา ===== */
        .select2-container--default .select2-search--dropdown .select2-search__field{
            background:var(--field-focus-bg) !important;
        }

        /* ===== TOAST : ปรับให้ตรง semantic ใหม่ ===== */
        .toast.success{ background:var(--ok) !important; }
        .toast.error{   background:var(--danger) !important; }
        .toast.warn{    background:var(--warn) !important; }
        .toast.info{    background:var(--info) !important; }
    </style>

    <%--Step 3 : Fix คอลัมน์แนว X--%>
    <script>
        function fixStickyColumns() {
            var tables = document.querySelectorAll('table.fx-table');
            for (var t = 0; t < tables.length; t++) {
                var tbl = tables[t];
                if (!tbl.rows.length) continue;
                var head = tbl.rows[0], n = head.cells.length, left = 0;
                for (var i = 0; i < n; i++) {
                    var hc = head.cells[i];
                    if (!/(^|\s)fx-col(\s|$)/.test(hc.className)) continue;
                    for (var r = 0; r < tbl.rows.length; r++) {
                        var row = tbl.rows[r];
                        if (row.cells.length !== n) continue;            // แถวว่าง / colspan
                        row.cells[i].style.left = left + 'px';
                    }
                    left += hc.getBoundingClientRect().width;
                }
            }
        }
        (function () {
            function run() { setTimeout(fixStickyColumns, 0); }
            var hooked = false;
            function hookAjax() {                                       // Sys มีหลัง ScriptManager โหลดแล้ว
                if (hooked || !(window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager)) return;
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(run);
                hooked = true;
            }
            function init() { hookAjax(); run(); }
            if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init); else init();
            window.addEventListener('load', init);
            window.addEventListener('resize', run);
        })();
    </script>

    <asp:ScriptManager runat="server" ID="scriptManager1" EnablePageMethods="true"></asp:ScriptManager>

    <asp:UpdatePanel runat="server" ID="UP1">

        <ContentTemplate>
            
            <%--Loading Overlay--%>
            <div id="loadingOverlay" style="display:none; position:fixed; z-index:9999; top:0; left:0; width:100%; height:100%; background:rgba(0,0,0,0.6); color:white; text-align:center; font-size:20px;">
                <div class="loading">
                    <svg class="pl" width="240" height="240" viewBox="0 0 240 240">
                        <circle class="pl__ring pl__ring--a" cx="120" cy="120" r="105" fill="none" stroke="#000" stroke-width="20" stroke-dasharray="0 660" stroke-dashoffset="-330" stroke-linecap="round"></circle>
                        <circle class="pl__ring pl__ring--b" cx="120" cy="120" r="35" fill="none" stroke="#000" stroke-width="20" stroke-dasharray="0 220" stroke-dashoffset="-110" stroke-linecap="round"></circle>
                        <circle class="pl__ring pl__ring--c" cx="85" cy="120" r="70" fill="none" stroke="#000" stroke-width="20" stroke-dasharray="0 440" stroke-linecap="round"></circle>
                        <circle class="pl__ring pl__ring--d" cx="155" cy="120" r="70" fill="none" stroke="#000" stroke-width="20" stroke-dasharray="0 440" stroke-linecap="round"></circle>
                    </svg>
                    <div style="margin-top:20px; font-size:18px;">กำลังบันทึกข้อมูล...</div>
                </div>              
            </div>

            <%---- Toast Container ----%>
            <div id="toast-container" aria-live="polite" aria-atomic="true"></div>

            <div>
                <%--Header--%>
                <div class="grid-headerTopic">
                    <div style="text-align:center; align-content:center;">
                        <img class="" style="height:38px;" src="images/borrow1.png" />
                    </div>
                    <label class="header_Style1" style="">
                        การร้องขอ<u>เบิกอุปกรณ์</u> : Jig Tool , Equipment </label>
                    <asp:Label ID="lb_mode" runat="server" Text="Mode : Issue"
                        style=" text-align: center; align-content: center; background: #fff; margin: 10px 10px; border-radius: 30px; font-weight: 600; color: #4f4b4b;">
                    </asp:Label>

                    <%-- ช่องที่ 1 : Manual (เปิดได้ทุกไฟล์ / Admin มี Upload) --%>
                    <div style="display:flex; align-items:center; justify-content:center; position:relative;">
                        <a href="#" class="manual-btn" onclick="return manualClick(event);" title="คู่มือการใช้งาน">
                            <span class="glyphicon glyphicon-book"></span> Manual
                        </a>

                        <div id="manualMenu" class="manual-menu">
                            <div class="manual-grp">ผู้ร้องขอ</div>
                            <a href="#" onclick="return openManualFile('Step1');"><span class="glyphicon glyphicon-user"></span> Step 1 : Requester</a>
                            <a href="#" onclick="return openManualFile('Step2');"><span class="glyphicon glyphicon-search"></span> Step 2 : Search &amp; Add</a>
                            <a href="#" onclick="return openManualFile('Step3');"><span class="glyphicon glyphicon-list-alt"></span> Step 3 : Request List</a>
                            <a href="#" onclick="return openManualFile('Step4');"><span class="glyphicon glyphicon-comment"></span> Step 4 : Comment</a>
                            <a href="#" onclick="return openManualFile('SaveDraft');"><span class="glyphicon glyphicon-pencil"></span> Save Draft</a>
                            <a href="#" onclick="return openManualFile('SendRequest');"><span class="glyphicon glyphicon-send"></span> Send Request</a>
                            <a href="#" onclick="return openManualFile('CancelRequest');"><span class="glyphicon glyphicon-remove-circle"></span> Cancel</a>
                            <div class="manual-sep"></div>
                            <div class="manual-grp">AAD</div>
                            <a href="#" onclick="return openManualFile('Confirm');"><span class="glyphicon glyphicon-ok-sign"></span> AAD Confirm</a>
                            <a href="#" onclick="return openManualFile('Reject');"><span class="glyphicon glyphicon-remove-sign"></span> Reject</a>

                            <asp:PlaceHolder ID="phManualUpload" runat="server" Visible="false">
                                <div class="manual-sep"></div>
                                <a href="#" onclick="$('#modal_UploadManual').modal('show'); hideManualMenu(); return false;">
                                    <span class="glyphicon glyphicon-upload"></span> Upload Manual
                                </a>
                            </asp:PlaceHolder>
                        </div>
                    </div>

                    <%-- ช่องที่ 2 : Read Me --%>
                    <div style="display:flex; align-items:center; justify-content:start;">
                        <span class="glyphicon glyphicon-question-sign readme-btn"
                            title="วิธีใช้งาน" onclick="$('#modal_ReadMe').modal('show');"></span>
                    </div>

                    <asp:HiddenField ID="hdIsAdmin" runat="server" Value="0" />
                </div>

                <%-- แถบนำทาง + ปุ่มคู่มือ (Status 50 = AAD Confirm) --%>
                <asp:Literal ID="litModeGuide" runat="server" Visible="false" />

                <%--=====================================================--%>
                <%------------ SECTION 1 : Employee ID (Issue By) ---------%>
                <%--=====================================================--%>
                <div class="card" style="margin-left:10px; margin-right:10px;">

                    <div class="card-header">
                        <span>
                            <asp:Literal ID="step_requester" runat="server" Text="<span class='badge'>Step 1</span>" />
                             &nbsp;Requester : ลงชื่อผู้ร้องขอ
                        </span>
                        <span style="margin-left:auto; font-size:16px; color:#555; font-weight:400;">
                            Issue No.: 
                            <strong><span class="badge-issue"><asp:Literal ID="litIssueNo" runat="server" Text="-" /></span></strong>
                            &nbsp;|&nbsp;
                            Status : <asp:Literal ID="litStatus" runat="server" Text="<span class='status-badge status-draft'>Wait Issue</span>" />
                        </span>
                    </div>

                    <div class="card-body">

                        <asp:Panel ID="pnlDraftBanner" runat="server" Visible="false">
                            <div class="draft-banner">
                                ขออภัยพบรายการ <strong> Draft </strong> ที่ยังไม่ได้ส่ง
                                (Issue No.: <strong><asp:Literal ID="litDraftIssueNo" runat="server" /></strong>
                                โดยวันที่ : <asp:Literal ID="litDraftDate" runat="server" />)
                                &nbsp;โดยระบบได้โหลดรายการนั้นขึ้นมาแล้ว
                            </div>
                        </asp:Panel>
                        
                        <div class="form-row">

                            <%--Emp Id--%>
                            <div class="form-group" style="width:10%; margin-left:10px;">
                                <label>EMP ID *</label>
                                <asp:TextBox ID="txtEmpId" runat="server" onkeyup="fetchRequesterName();"
                                    CssClass="" MaxLength="6" Width="100%"
                                    placeholder="รหัสพนักงาน" Style="text-align:center; background-color:#00a7ff40;"
                                    AutoComplete="off"  AutoPostBack ="true"
                                    OnTextChanged="txtEmpId_TextChanged" />
                            </div>

                            <%--Tel / Phone--%>
                            <div class="form-group" style="width:10%;">
                                <label>TEL *</label>
                                <asp:TextBox ID="txtTel" runat="server" 
                                    CssClass="" MaxLength="9" Width="100%"
                                    placeholder="เบอร์ติดต่อ" Style="text-align:center; background-color:#00a7ff40;" />
                            </div>

                            <%--Emp Name--%>
                            <div class="form-group" style="width:22%;">
                                <label>Employee Name</label>
                                <asp:TextBox ID="txtEmpName" runat="server" ReadOnly="true" Width="100%"
                                    CssClass="" Style="text-align:center;" placeholder="ชื่อพนักงาน" />
                            </div>

                            <%--Postion--%>
                            <div class="form-group" style="width:16%;">
                                <label>Position</label>
                                <asp:TextBox ID="txtPosition" runat="server" ReadOnly="true" Width="100%"
                                    CssClass="" Style="text-align:center;" placeholder="ตำแหน่ง" />
                            </div>

                            <%--Dept Name--%>
                            <div class="form-group" style="width:10%;">
                                <label> Department</label>
                                <asp:TextBox ID="txtDept" runat="server" ReadOnly="true" Width="100%"
                                    CssClass="" Style="text-align:center;" placeholder="แผนก" />
                            </div>

                            <%--Dept Code--%>
                            <div class="form-group" style="width:9%;">
                                <label> Dept Code</label>
                                <asp:TextBox ID="txtDeptCode" runat="server" ReadOnly="true" Width="100%"
                                    CssClass="" Style="text-align:center;" placeholder="Dept Code" />
                            </div>

                            <%--Issue Date--%>
                            <div class="form-group" style="width:14%;">
                                <label>Issue Date</label>
                                <div class="input-group date dp-compact" id="IssueDatePicker">
                                    <asp:TextBox ID="txtIssueDate" runat="server" Width="75%"
                                        CssClass="" Style="text-align:center; border-top-right-radius:0px; border-bottom-right-radius:0px;" 
                                        placeholder="วันที่ Issue" ReadOnly="true" AutoPostBack="true"/>
                                    <span class="input-group-addon" style="width:32px; margin-top: -5px;">
                                        <span class="glyphicon glyphicon-calendar"></span>
                                    </span>
                                </div>
                            </div>
                            <asp:HiddenField ID="hdtxtBand" runat="server" />
                            <asp:HiddenField ID="hdtxtEmail" runat="server" />
                            <asp:HiddenField ID="hdNewMember_ck" runat="server" Value="-"/>                                       
                        </div>
                        <asp:Panel ID="pnlNewEmp" runat="server" Visible="false">
                            <div class="emp-name-display new-emp" style="margin-top:-10px; margin-left:10px;">
                                <asp:label ID="lblempNotice" runat="server"></asp:label>
                            </div>
                        </asp:Panel>
                    </div>

                </div>

                <%--=====================================================--%>
                <%------------ SECTION 2 : Search & Add List --------------%>
                <%--=====================================================--%>
                <asp:Panel ID="pnlSearch_Add" runat="server" Visible="true" class="card" style="margin-left:10px; margin-right:10px;">
                    <div class="card-header">
                        <span>
                            <asp:Literal ID="step_searchAdd" runat="server" Text="<span class='badge'>Step 2</span>" />
                             &nbsp;Search & Add List : ค้นหาข้อมูลและเพิ่มรายการการร้องขอเบิกอุปกรณ์
                        </span>
                    </div>
                    <div class="card-body" style="margin-bottom:10px;">
                        <div id="tabFixAsset" class="grid_search">
                            <div class="grid_search2">
                                <div class="form-group" style="margin-bottom:5px; margin-left:60px; flex-direction:row; align-items:center;">
                                    <asp:CheckBox ID="chkSpare" runat="server" Text=" Spare" Checked="true" onclick="toggleFixAssetCheck(this);" />
                                    &nbsp;&nbsp;
                                    <asp:CheckBox ID="chkAll" runat="server" Text=" All" onclick="toggleFixAssetCheck(this);" />
                                </div>
                                <div class="form-row" style="margin-left:15px;">                            
                                    <div class="form-group" style="min-width:200px; align-self:center;">
                                        <asp:TextBox ID="txtFixAsset" runat="server" Width="200px" placeholder="ค้นหารายการ"
                                            Style="text-align:center; background:#00a7ff40;"/>
                                    </div>
                                    <asp:Button ID="btnSearchFixAsset" runat="server" Text="Search" CssClass="btn btn-primary"
                                        OnClick="btnSearch_Click" style="align-self:center; margin-top:-5px;"/>                            
                                </div>
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
                                        <asp:BoundField  DataField="RowNo" HeaderText="#" HeaderStyle-CssClass="headerGv1"
                                                         ItemStyle-CssClass="rowGv1" ItemStyle-Width="40px" />
                                        <asp:ButtonField HeaderText="Add" ButtonType="Button" Text="Add" ItemStyle-Width="65px"
                                            CommandName="AddItem" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"
                                            ControlStyle-CssClass="btn btn-success btn-sm" /> 
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
                                                    OnClick="btn_pic_Click" CssClass="img_tool">
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
                                        <asp:BoundField  DataField="sub_no" ItemStyle-Width="50px" HeaderText="No" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                        <asp:BoundField  DataField="FixAcces" ItemStyle-Width="110px" HeaderText="Fix Asset" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                        <asp:BoundField  DataField="control_no" ItemStyle-Width="130px" HeaderText="Control No" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                        <asp:BoundField  DataField="tool_no" HeaderText="Tool No / Model" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                        <asp:BoundField  DataField="dept_code" ItemStyle-Width="65px" HeaderText="Dept" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                        <asp:BoundField  DataField="model_name" HeaderText="CPT Model" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                        <asp:BoundField  DataField="cell" HeaderText="Cell" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                        <asp:BoundField  DataField="station" HeaderText="Station" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"/>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </div>
                    </div>
                </asp:Panel>  

                <%--=====================================================--%>
                <%------------ SECTION 3 : Request List -------------------%>
                <%--=====================================================--%>
                <div class="card" style="margin-left:10px; margin-right:10px;">
                    <div class="card-header">
                        <span>
                            <asp:Literal ID="step_borrowlist" runat="server" Text="<span class='badge'>Step 3</span>" />
                             &nbsp;Request List : รายการที่ร้องขอเบิก , ลงข้อมูลสถานที่นำไปใช้งาน
                        </span>
                         <span style="margin-left:auto; font-size:11px; font-weight:400; color:#555;">
                             <asp:Literal ID="request_remark" runat="server" 
                                 Text=" <span class='badge' style='background:#e4ff5d; color:#000; font-size:12px; text-shadow: none; '> 
                                 หมายเหตุ : เลือก Use : Dept = AAD (5505) กรณี Requester นำอุปกรณ์ไปใช้งานนอกส่วนงาน EPM </span>" />
                         </span>
                    </div>
                    <div class="card-body" style="margin-bottom:10px;">
                        <div class="tbl-wrap step3-wrap" style="max-height: 350px;">
                            <asp:GridView ID="gvBorrowList" runat="server" DataKeyNames="ToolId,ToolSub,SystemId"
                                CssClass="data-table change-table fixed-header fx-table"
                                style="min-width:1800px;"
                                AutoGenerateColumns="false"
                                OnRowDataBound="gvBorrowList_RowDataBound"
                                OnRowCommand="gvBorrowList_RowCommand"
                                EmptyDataText="ไม่มีรายการแจ้งเปลี่ยนแปลง">
                                <EmptyDataRowStyle CssClass="no-data" />
                                <Columns>
                                    <%--Item No--%>
                                    <asp:BoundField DataField="RowNo" HeaderText="#"
                                        HeaderStyle-CssClass="headerGv1 fx-col" ItemStyle-CssClass="rowGv1 fx-col" ItemStyle-Width="40px" />

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
                                            <asp:HiddenField ID="hdToolSub" runat="server" Value='<%# Eval("ToolSub") %>' />
                                            <asp:HiddenField ID="hdSystemId" runat="server" Value='<%# Eval("SystemId") %>' />
                                            <asp:LinkButton ID="btn_Picture" title="View Picture" runat="server"
                                                OnClick="btn_pic_Click" CssClass="img_tool">
                                                <img src="images/photo-gallery.png" />
                                            </asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%--Fix Asset--%>
                                    <asp:BoundField DataField="FixAssetNo" ItemStyle-Width="110px"
                                        HeaderText="Fix Asset" HeaderStyle-CssClass="headerGv1 fx-col" ItemStyle-CssClass="rowGv1 fx-col" />

                                    <%--Control No--%>
                                    <asp:BoundField DataField="ControlNo" ItemStyle-Width="120px"
                                        HeaderText="Control No" HeaderStyle-CssClass="headerGv1 fx-col" ItemStyle-CssClass="rowGv1 fx-col" />

                                    <%--Tool No--%>
                                    <asp:BoundField DataField="ToolNo"
                                        HeaderText="Tool No / Model" HeaderStyle-CssClass="headerGv1 fx-col" ItemStyle-CssClass="rowGv_toolNo fx-col fx-toolno" />

                                    <%--Sub--%>
                                    <asp:BoundField DataField="ToolSub" ItemStyle-Width="45px"
                                        HeaderText="No" HeaderStyle-CssClass="headerGv1 fx-col fx-last zone-end" ItemStyle-CssClass="rowGv1 fx-col fx-last zone-end" />                                    

                                    <%--═══════════ โซน CURRENT (4 ช่อง, label อย่างเดียว) ═══════════--%>

                                    <%--Current Dept--%>
                                    <asp:BoundField DataField="DeptCode" ItemStyle-Width="100px" HeaderText="Current Dept"
                                        HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />

                                    <%--Current Model--%>
                                    <asp:BoundField DataField="ModelName" ItemStyle-Width="120px" HeaderText="Current Model"
                                        HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />

                                    <%--Current Cell--%>
                                    <asp:BoundField DataField="CellName" ItemStyle-Width="120px" HeaderText="Current Cell"
                                        HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />

                                    <%--Current Station--%>
                                    <asp:BoundField DataField="StationName" ItemStyle-Width="120px" HeaderText="Current Station"
                                        HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />

                                    <%--═══════════ โซน USE (4 ช่อง) ═══════════--%>

                                    <%--Use Dept (dropdown) (เริ่มโซน → zone-divider)--%>
                                    <asp:TemplateField HeaderText="Use : Dept" HeaderStyle-CssClass="hdr-use zone-divider" ItemStyle-CssClass="col-use zone-divider">
                                        <ItemTemplate>
                                            <div class="d-flex align-items-center" style="min-width:160px;">
                                                <asp:DropDownList ID="ddlDept" runat="server"
                                                    CssClass="form-control form-control-sm ddl_edit ddlSearch" Style="width:95%;" > 
                                                </asp:DropDownList>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%--Use Model (dropdown)--%>
                                    <asp:TemplateField HeaderText="Use : Model" HeaderStyle-CssClass="hdr-use" ItemStyle-CssClass="col-use">
                                        <ItemTemplate>
                                            <div class="d-flex align-items-center" style="min-width:170px;">
                                                <asp:HiddenField ID="hfModelMode" runat="server" Value="ddl" />
                                                <asp:DropDownList ID="ddlModel" runat="server"
                                                    CssClass="form-control form-control-sm ddl_edit ddlSearch" Style="width:85%;">
                                                </asp:DropDownList>
                                                <asp:TextBox ID="txtModel" runat="server" placeHolder="Model (New)"
                                                    CssClass="form-control form-control-sm ddl_edit" Style="width:85%; display:none;">
                                                </asp:TextBox>
                                                <button id="btnModelToggle" runat="server" type="button" class="btn btn-primary btnToggle"
                                                    style="padding:4px 6px; height:20px !important; font-size:12px;">+</button>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%--Use Cell (dropdown)--%>
                                    <asp:TemplateField HeaderText="Use : Cell" HeaderStyle-CssClass="hdr-use" ItemStyle-CssClass="col-use">
                                        <ItemTemplate>
                                            <div class="d-flex align-items-center" style="min-width:190px;">
                                                <asp:HiddenField ID="hfCellMode" runat="server" Value="ddl" />
                                                <asp:DropDownList ID="ddlCell" runat="server"
                                                    CssClass="form-control form-control-sm ddl_edit ddlSearch" Style="width:85%;">
                                                </asp:DropDownList>
                                                <asp:TextBox ID="txtCell" runat="server" placeHolder="Cell (New)"
                                                    CssClass="form-control form-control-sm ddl_edit" Style="width:85%; display:none;">
                                                </asp:TextBox>
                                                <button id="btnCellToggle" runat="server" type="button" class="btn btn-primary btnToggle"
                                                    style="padding:4px 6px; height:20px !important; font-size:12px;">+</button>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%--Use Station (dropdown)--%>
                                    <asp:TemplateField HeaderText="Use : Station" HeaderStyle-CssClass="hdr-use zone-end" ItemStyle-CssClass="col-use zone-end">
                                        <ItemTemplate>
                                            <div class="d-flex align-items-center" style="min-width:210px;">
                                                <asp:HiddenField ID="hfStationMode" runat="server" Value="ddl" />
                                                <asp:DropDownList ID="ddlStation" runat="server"
                                                    CssClass="form-control form-control-sm ddl_edit ddlSearch" Style="width:85%;">
                                                </asp:DropDownList>
                                                <asp:TextBox ID="txtStation" runat="server" placeHolder="Station (New)"
                                                    CssClass="form-control form-control-sm ddl_edit" Style="width:85%; display:none;">
                                                </asp:TextBox>
                                                <button id="btnStationToggle" runat="server" type="button" class="btn btn-primary btnToggle"
                                                    style="padding:4px 6px; height:20px !important; font-size:12px;">+</button>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>                        
                    </div>
                </div>

                <%--=====================================================--%>
                <%------------ SECTION 4 : Comment Withdraw ---------------%>
                <%--=====================================================--%>
                <div class="card" style="margin-left:10px; margin-right:10px;">
                    <div class="card-header">
                        <span>
                            <asp:Literal ID="step_comment" runat="server" Text="<span class='badge'>Step 4</span>" />
                             &nbsp;Comment : เหตุผลการขอเบิกอุปกรณ์
                        </span>
                        <span style="margin-left:auto; font-size:11px; font-weight:400; color:#555;">
                            <asp:Literal ID="borrow_remark" runat="server" 
                                Text="<span>( บันทึกข้อมูลชั่วคราวได้ด้วยปุ่ม Save Draft / ส่งรายการร้องขอด้วยปุ่ม Request)</span>"  />
                        </span>
                    </div>
                    <div class="card-body" style="margin-bottom:10px;">                        
                        <div class="form-row" style="align-items: self-start;">
                            <div class="form-group" style="width:180px; margin-left:10px; gap:5px;">
                                <label>เหตุผลการเบิก *</label>
                                <asp:DropDownList ID="ddlReason_Borrow" runat="server" AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlReason_Borrow_SelectedIndexChanged"
                                    CssClass="form-control form-control-sm ddl_edit"
                                    Style="min-width:170px; padding: 2px 5px;">
                                </asp:DropDownList>
                            </div>
                            <div class="form-group" style="width: calc(100% - 250px); margin-left:10px;">
                                <span style="text-align:left; font-size:12px; font-weight:600; color:#555;">Comment &nbsp;
                                    <span style="text-align:right; font-size:12px; font-weight:500; color:#666;">
                                        (จำนวนตัวอักษร :
                                        <span id="charCount" style="color:#ff0303;">0</span>
                                        / 200 )
                                    </span>
                                </span>
                                <asp:TextBox ID="txt_borrowComment" runat="server" 
                                    TextMode="MultiLine" Rows="4" CssClass="form-control" MaxLength="200"
                                    onkeyup="countChar(this)" style ="width:100%; Height:50px !important;"
                                    placeholder="กรุณาใส่เหตุผลในการร้องข้อเบิก">
                                </asp:TextBox>
                            </div>
                        </div>
                        <div ID="borrow_note" runat="server" visible="false">
                            <div class="form-row" style="align-content:center;">
                                <div class="page-note">
                                    <span class="badge" style="background:#ff5d5d;">หมายเหตุ :</span>
                                    <div style="display: inline-grid;text-align-last: left; gap:5px;">
                                        <span class="badge" style="background:#e4ff5d; color:#000; font-size:13px;">หมายเหตุ 1</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <%--======================================================================--%>
                <%------------ SECTION 5 : Reject Comment , Reject History -----------------%>
                <%--======================================================================--%>
                <asp:Panel ID="pnl_rejectComment" runat="server" Visible="false" class="card" style="margin-left:10px; margin-right:10px;">
                    <div class="card-header">
                        <span>Reject , Cancel : Comment & History</span>
                        <span style="margin-left:auto; font-size:11px; font-weight:400; color:#555;">
                            <asp:Literal ID="reject_comment" runat="server" 
                                Text="<span>กรณี Reject , Cancel : ใส่ข้อมูลเหตุผลการ Reject , Cancel</span>"  />    
                        </span>                    
                    </div>
                    <asp:panel ID="rejectComment" runat="server" Visible="true" class="card-body">                    
                        <span ID="rejectH" runat="server" style="text-align:left; font-size:12px; font-weight:500;">Comment</span>
                        <span ID="rejectCountAlert" runat="server" style="text-align:right; font-size:12px; font-weight:500; color:#666;">
                            (จำนวนตัวอักษร :
                            <span id="charCount_Reject" style="color:#ff0303;">0</span>
                            / 200 )
                            <span style="color:red;">** ลงข้อมูลอย่างน้อย 10 ตัวอักษร **</span>
                        </span>
                        <asp:TextBox ID="txt_rejectComment" runat="server" 
                            TextMode="MultiLine" Rows="4" CssClass="form-control" MaxLength="200"
                            onkeyup="countChar_Reject(this)" style ="width:100%; Height:60px !important;"
                            placeholder="กรุณาใส่เหตุผลในการ Reject">
                        </asp:TextBox>
                    </asp:panel>
                    <div class="card-body" style="margin-bottom:10px;"> 
                        <span>History</span>
                        <div class="tbl-wrap" style="max-height: 150px;">
                            <asp:GridView ID="gv_rejectHistory" runat="server"
                                CssClass="data-table change-table fixed-header"
                                AutoGenerateColumns="false"
                                EmptyDataText="ไม่มีประวัติการ Reject">
                                <EmptyDataRowStyle CssClass="no-data" />
                                <Columns>
                                    <asp:BoundField  DataField="Reject_Count" ItemStyle-Width="60px" HeaderText="No" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1"/>
                                    <asp:BoundField  DataField="Type" ItemStyle-Width="80px" HeaderText="Type" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1"/>
                                    <asp:BoundField  DataField="Reject_by" ItemStyle-Width="180px" HeaderText="By" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1"/>
                                    <asp:BoundField  DataField="status_name" ItemStyle-Width="140px" HeaderText="Process" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1"/>
                                    <asp:BoundField  DataField="Reject_Date" ItemStyle-Width="140px" HeaderText="Date" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1" DataFormatString="{0:dd-MMM-yyyy HH:MM}"/>
                                    <asp:BoundField  DataField="Reject_detail" HeaderText="Detail" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv2"/>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>
                </asp:Panel>

                <!-- ── ACTION BAR / BUTTON BAR ── -->
                <div class="action-bar" id="ButtonBar" runat="server"> 

                    <div class="action-bar-left" style="">
                        <%-- Cancel Button --%>
                        <div id="CancelButton" runat="server" Visible="false">
                            <span class="tooltip-wrap" style="">
                                <asp:LinkButton ID="btnCancel" runat="server"
                                    CssClass="btn btn-secondary" style="font-size:12px; text-shadow:none;"
                                    OnClick="btnCancel_Click" OnClientClick="return confirmCancel();"
                                    title="ยกเลิกการร้องขอเบิก">
                                    <span class="glyphicon glyphicon-ban-circle"></span>
                                    Cancel Request
                                </asp:LinkButton>   
                                <span class="tooltip-box">ยกเลิกการร้องขอ</span>  
                            </span>
                        </div>
                    </div>

                    <div class="action-bar-right" style="">
                        <%-- Draft Button --%>
                        <div id="DraftButton" runat="server" Visible="true">
                            <span class="tooltip-wrap" style="">
                                <asp:LinkButton ID="btnDraft" runat="server" 
                                    CssClass="btn btn-warning" style="margin-right:10px;"
                                    OnClick="btnDraft_Click" OnClientClick="return confirmDraft();"
                                    title="บันทึกข้อมูลชั่วคราว ยังไม่ส่งร้องขอเบิก">
                                    <span class="glyphicon glyphicon-exclamation-sign"></span>
                                        Save Draft                                
                                </asp:LinkButton>
                                <span class="tooltip-box">บันทึกข้อมูลชั่วคราว</span>  
                            </span>
                        </div>

                        <%-- Reject Button --%>
                        <div id="RejectButton" runat="server" Visible="false">
                            <span class="tooltip-wrap" style="">
                                <asp:LinkButton ID="btnReject" runat="server"
                                    CssClass="btn btn-danger" OnClick="btnReject_Click" style="margin-right:10px;"
                                    OnClientClick="return Reject();" title="Reject การร้องขอ">
                                    <span class="glyphicon glyphicon-remove"></span>
                                        Reject                               
                                </asp:LinkButton>
                                <span class="tooltip-box">ปฎิเสธการอนุมัติ</span>  
                            </span>
                        </div>

                        <%-- Send Request Button --%>
                        <div id="SendButton" runat="server" Visible="true">
                            <span class="tooltip-wrap" style="">
                                <asp:LinkButton ID="btnSend" runat="server"
                                    CssClass="btn btn-success" OnClick="btnSend_Click"
                                    OnClientClick="return sendtoApprove();" title="ส่งรายการเพื่อขอร้องเบิก">                                                                        
                                    <span class="glyphicon glyphicon-send"></span>
                                        Send Request
                                </asp:LinkButton> 
                                <span class="tooltip-box">ส่งรายการร้องขอ</span>  
                            </span>
                        </div>

                        <%-- Confirm Button --%>
                        <div id="ConfirmButton" runat="server" Visible="false">
                            <span class="tooltip-wrap" style="">
                                <%-- เปิด Modal ยืนยัน Status Use / ผู้รับผิดชอบ (ยืนยันจริงใน Modal) --%>
                                <asp:LinkButton ID="btnConfirm" runat="server"
                                    CssClass="btn btn-success" OnClick="btnConfirm_Click"
                                    data-toggle="tooltip" data-placement="top"
                                    title="ยืนยันการร้องขอ">                                                                        
                                    <span class="glyphicon glyphicon-ok-sign"></span>
                                        AAD Confirm
                                </asp:LinkButton>
                                <span class="tooltip-box">ยืนยันการร้องขอ</span>  
                            </span>
                        </div>

                        <%-- Confirm Button 2 --%>
                        <div id="ConfirmButton2" runat="server" Visible="false">
                            <span class="tooltip-wrap" style="">
                                <%-- ตรวจข้อมูลแล้วเปิด Modal ยืนยัน Status Use / ผู้รับผิดชอบ (บันทึกจริงใน Modal) --%>
                                <asp:LinkButton ID="btnConfirm2" runat="server"
                                    CssClass="btn btn-success" OnClick="btnConfirm2_Click"
                                    data-toggle="tooltip" data-placement="top"
                                    title="ยืนยันการร้องขอ">                                                                        
                                    <span class="glyphicon glyphicon-ok-sign"></span>
                                        AAD Confirm
                                </asp:LinkButton>
                                <span class="tooltip-box">ยืนยันการร้องขอ</span>  
                            </span>
                        </div>
                    </div>                       

                </div>
            </div>

            <%--=======================END===========================--%>

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

            <%-- ===== Modal : ยืนยัน AAD Confirm (Status 50) — อ้างอิง NotificationOfChange ===== --%>
            <asp:Panel ID="pnlConfirmModal" runat="server" CssClass="wcf-overlay" Style="display:none;">
                <div class="wcf-box">
                    <div class="wcf-head">
                        <span>ยืนยัน AAD Confirm <span class="wcf-doc"><asp:Literal ID="litCfmDoc" runat="server" /></span></span>
                        <span style="display:inline-flex; align-items:center; gap:8px; margin-left:auto;">
                            <a href="#" class="manual-btn" style="border:1px solid #c0392b;" title="คู่มือ AAD Confirm"
                               onclick="return openManualFile('Confirm');">
                                <span class="glyphicon glyphicon-book"></span> คู่มือ
                            </a>
                            <button type="button" class="wcf-x" onclick="closeConfirmModal();" aria-label="ปิด">&times;</button>
                        </span>
                    </div>

                    <div class="wcf-body">
                        <asp:Literal ID="litCfmSummary" runat="server" />

                        <div class="cfm-warn">
                            <span class="cfm-warn-ico">⚠️</span>
                            <div>
                                <b class="cfm-warn-title">กรุณาตรวจสอบ Status Use และ ผู้รับผิดชอบ ทุกรายการก่อนกดยืนยัน</b>
                                ช่อง <span class="cfm-chip">สีเหลือง</span> คือข้อมูลที่จะถูกบันทึกและใช้อัปเดตอุปกรณ์ — ระบบเลือกค่าเริ่มต้นให้แล้ว แต่ต้องตรวจให้ถูกต้อง
                                <ul>
                                    <li>ช่อง <b style="color:#a82f2f;">ขอบแดง</b> = ยังไม่มีข้อมูล ต้องแก้ก่อนยืนยัน</li>
                                </ul>
                            </div>
                        </div>

                        <div class="wcf-sec">Step 3 : Request List <small>(Current → Use)</small></div>

                        <div class="tbl-wrap" style="max-height:440px; overflow:auto;">
                            <asp:GridView ID="gvConfirmSummary" runat="server"
                                CssClass="data-table wcf-table" GridLines="None" CellSpacing="0"
                                style="min-width:1900px;" AutoGenerateColumns="false"
                                OnRowDataBound="gvConfirmSummary_RowDataBound" EmptyDataText="ไม่มีรายการ"
                                UseAccessibleHeader="true">
                                <Columns>
                                    <asp:BoundField DataField="RowNo" HeaderText="#" ItemStyle-Width="35px" />
                                    <asp:BoundField DataField="FixAssetNo" HeaderText="Fix Asset" ItemStyle-Width="100px" />
                                    <asp:BoundField DataField="ControlNo" HeaderText="Control No" ItemStyle-Width="110px" />
                                    <asp:BoundField DataField="ToolNo" HeaderText="Tool No" />

                                    <%--═══ AAD : เลือก Status Use / ผู้รับผิดชอบ ═══--%>
                                    <asp:TemplateField HeaderText="Status Use *"
                                        HeaderStyle-CssClass="hdr-aad zone-divider" ItemStyle-CssClass="col-aad zone-divider">
                                        <ItemTemplate>
                                            <asp:HiddenField ID="hfCfmKey" runat="server" Value='<%# Eval("RowKey") %>' />
                                            <asp:DropDownList ID="ddlStatusUse" runat="server" CssClass="form-control aad-ddl" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="ผู้รับผิดชอบ *"
                                        HeaderStyle-CssClass="hdr-aad" ItemStyle-CssClass="col-aad">
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlResponsible" runat="server" CssClass="form-control aad-ddl" />
                                            <asp:Literal ID="litRuleNote" runat="server" />
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%--═══ Current (เหมือน Step 3 : Request List) ═══--%>
                                    <asp:BoundField DataField="OldStatus" HeaderText="Current Status"
                                        HeaderStyle-CssClass="hdr-current zone-divider" ItemStyle-CssClass="col-current zone-divider" />
                                    <asp:BoundField DataField="OldResp" HeaderText="Current ผู้รับผิดชอบ"
                                        HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />
                                    <asp:BoundField DataField="OldDept" HeaderText="Current Dept"
                                        HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />
                                    <asp:BoundField DataField="OldModel" HeaderText="Current Model"
                                        HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />
                                    <asp:BoundField DataField="OldCell" HeaderText="Current Cell"
                                        HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />
                                    <asp:BoundField DataField="OldStation" HeaderText="Current Station"
                                        HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />

                                    <%--═══ Use (เหมือน Step 3) : Dept แสดง (Real Dept) ของผู้รับผิดชอบที่เลือก ═══--%>
                                    <asp:TemplateField HeaderText="Use : Dept"
                                        HeaderStyle-CssClass="hdr-use zone-divider" ItemStyle-CssClass="col-use zone-divider">
                                        <ItemTemplate>
                                            <span class="use-dept" title="Use Dept (Real Dept ตามผู้รับผิดชอบ : Responsible_Dept.dept_code_real)">
                                                <asp:Literal ID="litUseDept" runat="server" />
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="NewModel" HeaderText="Use : Model"
                                        HeaderStyle-CssClass="hdr-use" ItemStyle-CssClass="col-use" />
                                    <asp:BoundField DataField="NewCell" HeaderText="Use : Cell"
                                        HeaderStyle-CssClass="hdr-use" ItemStyle-CssClass="col-use" />
                                    <asp:BoundField DataField="NewStation" HeaderText="Use : Station"
                                        HeaderStyle-CssClass="hdr-use" ItemStyle-CssClass="col-use" />
                                </Columns>
                            </asp:GridView>
                        </div>

                        <div class="wcf-notice">
                            เมื่อยืนยัน เอกสารจะเปลี่ยนเป็น <b>Wait Update</b> และบันทึก Status / ผู้รับผิดชอบ (เดิม → ใหม่) ของทุกรายการ
                        </div>
                    </div>

                    <div class="wcf-foot">
                        <asp:LinkButton ID="btnConfirmModalCancel" runat="server" CssClass="btn btn-secondary"
                            OnClientClick="closeConfirmModal(); return false;" Text="ยกเลิก" />
                        <asp:LinkButton ID="btnConfirmModalOk" runat="server" CssClass="btn btn-success"
                            OnClick="btnConfirmModalOk_Click" OnClientClick="return cfmConfirm();">
                            <span class="glyphicon glyphicon-ok-sign"></span> ยืนยัน AAD Confirm
                        </asp:LinkButton>
                    </div>
                </div>
            </asp:Panel>

            <%-- Modal Read me --%>
            <div class="modal fade" id="modal_ReadMe" tabindex="-1" role="dialog">
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
                                <div class="rm-ico"><span class="glyphicon glyphicon-user"></span></div>
                                <div class="rm-txt"><strong>Step 1 : Requester</strong>
                                    <span>กรอกรหัสพนักงาน (6 หลัก) และเบอร์ติดต่อ ระบบจะดึงชื่อ/แผนกจาก HRMS ให้อัตโนมัติ</span></div>
                            </div>
                            <div class="rm-item">
                                <div class="rm-ico"><span class="glyphicon glyphicon-search"></span></div>
                                <div class="rm-txt"><strong>Step 2 : Search & Add</strong>
                                    <span>ค้นหาอุปกรณ์ด้วย Fix Asset, Control No, Tool No ฯลฯ แล้วกด Add เพื่อเพิ่มเข้ารายการเบิก</span></div>
                            </div>
                            <div class="rm-item">
                                <div class="rm-ico"><span class="glyphicon glyphicon-list-alt"></span></div>
                                <div class="rm-txt"><strong>Step 3 : Request List</strong>
                                    <span>ระบุสถานที่นำไปใช้งาน (Dept / Model / Cell / Station) กดปุ่ม + เพื่อสลับเป็นกรอกค่าใหม่เองได้ และ Remove เพื่อลบรายการ</span></div>
                            </div>
                            <div class="rm-item">
                                <div class="rm-ico"><span class="glyphicon glyphicon-comment"></span></div>
                                <div class="rm-txt"><strong>Step 4 : Comment</strong>
                                    <span>เลือกเหตุผลการเบิกและกรอกเหตุผล (บางเหตุผลบังคับกรอก Comment อย่างน้อย 10 ตัวอักษร)</span></div>
                            </div>
                            <div class="rm-item">
                                <div class="rm-ico"><span class="glyphicon glyphicon-floppy-disk"></span></div>
                                <div class="rm-txt"><strong>บันทึก / ส่งรายการ</strong>
                                    <span>Save Draft เก็บชั่วคราว, Send Request ส่งให้อนุมัติ ปุ่มที่แสดงจะขึ้นกับสถานะและสิทธิ์ผู้ใช้</span></div>
                            </div>
                            <div class="rm-item">
                                <div class="rm-ico"><span class="glyphicon glyphicon-book"></span></div>
                                <div class="rm-txt"><strong>Manual</strong>
                                    <span>กดปุ่ม Manual มุมขวาบน แล้วเลือกหัวข้อ (Step 1–4 / Save Draft / Send / Cancel / AAD Confirm / Reject) เพื่อเปิดคู่มือ (PDF)</span></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <%-- Modal Upload Manual (Admin) --%>
            <div class="modal fade" id="modal_UploadManual" tabindex="-1" role="dialog">
                <div class="modal-dialog" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                            <h4 class="modal-title"><span class="glyphicon glyphicon-upload"></span>&nbsp;Upload Manual (PDF)</h4>
                        </div>
                        <div class="modal-body">
                            <p style="font-size:12px; color:#666; margin-bottom:10px;">
                                เลือกคู่มือที่ต้องการแทนที่ แล้วเลือกไฟล์ PDF — <b>ไฟล์เดิมจะถูกแทนที่</b>
                            </p>
                            <asp:DropDownList ID="ddlManualFile" runat="server" CssClass="form-control"
                                Style="height:auto !important; padding:6px; margin-bottom:8px;">
                                <asp:ListItem Value="Step1" Text="Step 1 : Requester (Step1.pdf)" />
                                <asp:ListItem Value="Step2" Text="Step 2 : Search &amp; Add (Step2.pdf)" />
                                <asp:ListItem Value="Step3" Text="Step 3 : Request List (Step3.pdf)" />
                                <asp:ListItem Value="Step4" Text="Step 4 : Comment (Step4.pdf)" />
                                <asp:ListItem Value="SaveDraft" Text="Save Draft (SaveDraft.pdf)" />
                                <asp:ListItem Value="SendRequest" Text="Send Request (SendRequest.pdf)" />
                                <asp:ListItem Value="CancelRequest" Text="Cancel (CancelRequest.pdf)" />
                                <asp:ListItem Value="Confirm" Text="AAD Confirm (Confirm.pdf)" />
                                <asp:ListItem Value="Reject" Text="Reject (Reject.pdf)" />
                            </asp:DropDownList>
                            <asp:FileUpload ID="fuManual" runat="server" accept="application/pdf"
                                CssClass="form-control" Style="height:auto !important; padding:6px;" />
                        </div>
                        <div class="modal-footer" style="border-top:1px solid #eee;">
                            <button type="button" class="btn btn-secondary" data-dismiss="modal" style="text-shadow:none;">Cancel</button>
                            <asp:LinkButton ID="btnUploadManual" runat="server" CssClass="btn btn-warning"
                                OnClick="btnUploadManual_Click" Style="text-shadow:none;">
                                <span class="glyphicon glyphicon-upload"></span>&nbsp;Upload
                            </asp:LinkButton>
                        </div>
                    </div>
                </div>
            </div>

        </ContentTemplate>

        <Triggers>
            <asp:PostBackTrigger ControlID="btnUploadManual" />
        </Triggers>

    </asp:UpdatePanel> 

    <%-- ===== modal HRMS Check Column (นอก UP1) ===== --%>
    <div class="modal fade" id="hrmsCheckModal" tabindex="-1" role="dialog" aria-hidden="true">
      <div class="modal-dialog modal-lg" role="document">
        <div class="modal-content">
          <div class="modal-header" style="background:#1a3a5c;">
            <h4 class="modal-title">ข้อมูลพนักงาน (HRMS)</h4>
            <button type="button" class="close" data-dismiss="modal" style="color:#fff;opacity:1;">&times;</button>
          </div>
          <div class="modal-body">
            <table class="data-table" style="width:100%;" id="hrmsCheckTable">
              <thead><tr><th style="width:40%;">Column</th><th>Value</th></tr></thead>
              <tbody id="hrmsCheckBody"></tbody>
            </table>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-dismiss="modal">ปิด</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Script for Modal Check Column -->
    <script>
        window.showModal = function (data) {
            var tbody = document.getElementById('hrmsCheckBody');
            if (!tbody) {
                console.warn('hrmsCheckBody not found');
                return;
            }
            tbody.innerHTML = '';
            if (data && typeof data === 'object') {
                Object.keys(data).forEach(function (key) {
                    var val = data[key];
                    if (val === null || val === undefined) val = '';
                    var tr = document.createElement('tr');
                    var td1 = document.createElement('td');
                    td1.style.fontWeight = '600';
                    td1.textContent = key;
                    var td2 = document.createElement('td');
                    td2.textContent = String(val);
                    tr.appendChild(td1);
                    tr.appendChild(td2);
                    tbody.appendChild(tr);
                });
            }
            $('#hrmsCheckModal').modal('show');
        };
    </script>

</asp:Content>