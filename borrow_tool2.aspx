<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="borrow_tool2.aspx.cs"
    Inherits="Tool_Equipment_Control_System.Borrow_Tool" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">    

     <%--<title> Equipment Borrow </title>--%>

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
    <script src="Scripts/jquery-1.9.1.js"></script>
    <script src="Scripts/jquery-3.6.0.min.js"></script>
    <script src="Scripts/script.js"></script>    
    <script src="Scripts/select2.min.js"></script>
    <script src="Scripts/jquery-ui.1.9.1.min.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>
    <%--<script src="Scripts/bootstrap.bundle.min.js"></script>--%>
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

            /**
             * แสดง Toast
             * @param {string} message - ข้อความหลัก
             * @param {{title?:string,type?:'success'|'error'|'warn'|'info',duration?:number}} [options]
             */
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
        // กำหนดให้ Datepicker แสดงเมื่อคลิกที่ TextBox
        function Set_datePicker() {
            // กำหนด locale ถ้าต้องการ (en หรือ th)
            moment.locale('en'); // หรือ 'th'

            $('#IssueDatePicker').datetimepicker({
                format: 'DD-MMM-YYYY',   // ตรงกับที่คุณต้องการ เช่น 18-Mar-2026
                useCurrent: false,       // ไม่บังคับ set วันนี้อัตโนมัติ
                showClear: true,
                showClose: true

            }).on('dp.change', function (e) {

                // Trigger PostBack ให้ ASP.NET รู้ว่าค่าเปลี่ยน
                __doPostBack('<%= txtIssueDate.UniqueID %>', '');
            });
        }

        // เรียกตอนหน้าโหลด
        $(function () {
            Set_datePicker();
        });
        var raw = $('#txtIssuedate').val();
        if (raw) {
            var m = moment(raw, 'DD MMM YYYY'); // มีเว้นวรรค ไม่ใช่ DD-MMM-YYYY
            if (m.isValid()) {
                $('#IssueDatePicker').data("DateTimePicker").date(m);
            }
        }

    </script>

    <%-- ตรวจสอบการแก้ไข Request ID --%>
    <script type="text/javascript">

        function fetchRequesterName() {

            ////////var requestId = document.getElementById('<%=txtEmpId.ClientID%>').value;
            ////////var empId = document.getElementById('<%= txtEmpId.ClientID %>').value;
            //if (empId.length === 6) {
                //getDataHRMS(empId, 1); // ✅ เรียก AJAX โดยตรง

                <%--__doPostBack('<%=btnHidden.UniqueID%>', requestId);--%>
            //}
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

                        // เปลี่ยนสี Textbox : EmpId
                        document.getElementById('<%= txtEmpId.ClientID %>').style.backgroundColor = "#d4edda";

                        // ✅ SET ค่า TextBox WebForms
                        document.getElementById('<%= txtEmpName.ClientID %>').value = dataUser.GNAME_ENG + " " + dataUser.FNAME_ENG;
                        document.getElementById('<%= txtPosition.ClientID %>').value = dataUser.POSN_ENAME;                        
                        document.getElementById('<%= txtDept.ClientID %>').value = dataUser.DEPT_ABB_NAME;                        

                        //Hidden Field
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
            return confirm(
                "⚠️ ยืนยันการบันทึก Draft\n" +
                "คุณต้องการบันทึกข้อมูลหรือไม่?\n" +
                "กด OK เพื่อดำเนินการต่อ"
            );
        }
        function sendtoApprove() {
            return confirm(
                "⚠️ ยืนยันการส่งรายการการแจ้งยืมอุปกรณ์\n" +
                "คุณต้องการส่งการร้องขอการแจ้งยืมอุปกรณ์หรือไม่?\n" +
                "กด OK เพื่อดำเนินการต่อ"
            );
        }
        function Reject() {
            return confirm(
                "⚠️ ยืนยันการ Reject การแจ้งยืมอุปกรณ์\n" +
                "คุณต้องการ Reject การแจ้งยืมอุปกรณ์หรือไม่?\n" +
                "กด OK เพื่อดำเนินการต่อ"
            );
        }
        function AADConfirm() {
            return confirm(
                "⚠️ อนุมัติรายการการแจ้งยืมอุปกรณ์\n" +
                "คุณต้องการอนุมัติการแจ้งยืมอุปกรณ์หรือไม่?\n" +
                "กด OK เพื่อดำเนินการต่อ"
            );
        }
        function Return() {
            return confirm(
                "⚠️ ยืนยันการนำคืนอุปกรณ์\n" +
                "คุณต้องการยืนยันการนำคืนอุปกรณ์หรือไม่?\n" +
                "กด OK เพื่อดำเนินการต่อ"
            );
        }
        function confirmRemove() {
            return confirm(
                "⚠️ ลบรายการออกจาก Borrow List\n" +
                "คุณต้องการลบรายการออกจาก Borrow List หรือไม่?\n" +
                "กด OK เพื่อดำเนินการต่อ"
            );
        }
        function confirmCancel() {
            return confirm(
                "⚠️ ยืนยันการยกเลิกการร้องขอยืม\n" +
                "คุณต้องการยกเลิกรายการนี้หรือไม่?\n" +
                "กด OK เพื่อดำเนินการต่อ"
            );
        }
    </script>

    <%--Count Borrow Comment--%>    
    <script>
        function countChar(val) {
            var len = val.value.length;

            document.getElementById('charCount').innerText = len;

            // ✅ เปลี่ยนสีเกิน 10 ตัว
            if (len < 10) {
                document.getElementById('charCount').style.color = 'red';
            } else {
                document.getElementById('charCount').style.color = '#0d5b2b';
            }
        }
        function countChar_Reject(val) {
            var len = val.value.length;

            document.getElementById('charCount_Reject').innerText = len;

            // ✅ เปลี่ยนสีเกิน 10 ตัว
            if (len < 10) {
                document.getElementById('charCount_Reject').style.color = 'red';
            } else {
                document.getElementById('charCount_Reject').style.color = '#0d5b2b';
            }
        }
    </script>

    <%--Drop Down Search + สลับ Dropdown / Textbox (Manual)--%>
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
        function hideLoading() {
            $('#loadingOverlay').hide();
        }

        function showLoading() {
            $('#loadingOverlay').show();
        }

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

            // ✅ กันกรณี user uncheck ทั้งคู่ → ให้ Spare กลับมา
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
                    row.scrollIntoView({
                        behavior: 'smooth',
                        block: 'center'
                    });
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

            // ✅ reset ค่าเดิมก่อน (สำคัญมาก)
            document.body.style.height = "auto";

            // ✅ คำนวณใหม่จาก content จริง
            var realHeight = document.documentElement.scrollHeight;

            // ✅ set ใหม่ (+100)
            document.body.style.height = (realHeight + 70) + "px";
        }

        // ✅ โหลดครั้งแรก
        $(document).ready(function () {
            adjustHeight(true);
        });

        // ✅ PostBack / UpdatePanel
        if (typeof (Sys) !== "undefined") {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                adjustHeight(false);
            });
        }
    </script>

    <%--Modal : เหตุผล Reject / Cancel--%>
    <script>
        function rjmEl(id) { return document.getElementById(id); }

        // ตั้งข้อความตามโหมด (ไม่ล้างค่าที่พิมพ์ — ใช้ตอน server เปิดซ้ำ)
        function setRejectModalMode(mode) {
            var isRej = (mode === 'reject');
            var m = rjmEl('<%= pnlRejectModal.ClientID %>');
            rjmEl('<%= hfRejectMode.ClientID %>').value = mode;
            if (m) m.classList.toggle('rjm-cancel', !isRej);
            rjmEl('rjmTitle').innerText = isRej ? 'Reject การร้องขอยืม' : 'ยกเลิกการร้องขอยืม (Cancel)';
            rjmEl('rjmDesc').innerText = isRej
                ? 'เอกสารจะถูกส่งกลับเป็น Draft ให้ผู้ร้องขอแก้ไขแล้วส่งใหม่'
                : 'เอกสารจะถูกยกเลิกถาวร ไม่สามารถนำกลับมาดำเนินการต่อได้';
            rjmEl('rjmOkText').innerText = isRej ? 'ยืนยัน Reject' : 'ยืนยันยกเลิก';
        }

        function showRejectModal() {
            var m = rjmEl('<%= pnlRejectModal.ClientID %>');
            if (!m) return;
            m.style.display = 'flex';
            var t = rjmEl('<%= txt_rejectComment.ClientID %>');
            if (t) { countChar_Reject(t); setTimeout(function () { t.focus(); }, 50); }
        }

        // กดปุ่ม Reject / Cancel Request
        function openRejectModal(mode) {
            setRejectModalMode(mode);
            var t = rjmEl('<%= txt_rejectComment.ClientID %>');
            if (t) { t.value = ''; t.classList.remove('rjm-err'); }
            rjmEl('rjmErr').style.display = 'none';
            showRejectModal();
            return false;                  // ไม่ Postback
        }

        function closeRejectModal() {
            var m = rjmEl('<%= pnlRejectModal.ClientID %>');
            if (m) m.style.display = 'none';
            return false;
        }

        // ตรวจก่อนส่ง (Server ตรวจซ้ำอีกรอบ)
        function rejectModalValidate() {
            var t = rjmEl('<%= txt_rejectComment.ClientID %>');
            var len = t ? t.value.trim().length : 0;
            if (len < 10 || len > 200) {
                if (t) { t.classList.add('rjm-err'); t.focus(); }
                var err = rjmEl('rjmErr');
                err.innerText = len > 200 ? 'เหตุผลต้องไม่เกิน 200 ตัวอักษร' : 'กรุณาระบุเหตุผลอย่างน้อย 10 ตัวอักษร';
                err.style.display = 'block';
                return false;
            }
            var mode = rjmEl('<%= hfRejectMode.ClientID %>').value;
            if (!confirm(mode === 'reject' ? '⚠️ ยืนยัน Reject เอกสารนี้ ?' : '⚠️ ยืนยันยกเลิกเอกสารนี้ ?')) return false;
            showLoading();                 // overlay ถูก render ใหม่หลัง Postback → หายเอง
            return true;
        }

        // Esc ปิด Modal
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') closeRejectModal();
        });
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

    <%--Style Default--%>
    <style>
        /* ===== RESET & BASE ===== */
        * { box-sizing: border-box; margin: 0; padding: 0; }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }

        /* ===== PAGE HEADER ===== */
        .page-header {
            background: linear-gradient(135deg, #1a3a5c 0%, #2980b9 100%);
            color: #fff;
            padding: 14px 24px;
            display: flex;
            align-items: center;
            gap: 12px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.2);
        }
        .page-header .icon { font-size: 22px; }
        .page-header h1 { font-size: 17px; font-weight: 600; letter-spacing: 0.3px; }
        .page-header .sub { font-size: 11px; opacity: 0.75; margin-top: 2px; }

        /* ===== LAYOUT ===== */
        .container { max-width: 1280px; margin: 20px auto; padding: 0 16px 40px; }

        .card {
            background: #fff;
            border-radius: 8px;
            box-shadow: 0 1px 6px rgba(0,0,0,0.08);
            margin-bottom: 12px;
            overflow: hidden;
        }
        .card-header {
            background: #f0f4f8;
            border-bottom: 2px solid #d0dbe8;
            padding: 5px 20px 5px 10px;
            display: flex;
            align-items: center;
            gap: 8px;
            font-weight: 600;
            font-size: 14px;
            color: #1a3a5c;
        }
        .card-header .badge {
            background: #2980b9;
            color: #fff;
            border-radius: 15px;
            padding: 4px 12px;
            font-size: 12px;
            font-weight: 500;
        }
        .card-body { padding: 10px 10px 0px 10px; }

        /* ===== FORM ELEMENTS ===== */
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
        .emp-name-display {
            font-size: 13px;
            color: #27ae60;
            font-weight: 600;
            min-height: 20px;
            padding: 4px 0;
        }
        .emp-name-display.new-emp { color: #e67e22; }

        /* ===== STATUS BADGE ===== */
        .status-badge {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 14px;
            font-weight: 700;
            letter-spacing: 0.5px;
        }
        .status-draft { background: #fef9e7; color: #b7950b; border: 1px solid #f0d060; }
        .status-approve  { background: #eaf6fb; color: #1a6a9a; border: 1px solid #aad4ea; }
        .status-confirm  { background: #f6e8f9; color: #b42fad; border: 1px solid #c6a0d2; }
        .status-completed  { background: #d9f0de; color: #289b52; border: 1px solid #59a461; }
        .status-cancel  { background: #e7e7e7; color: #4c4c4c; border: 1px solid #636363; }
        .status-borrow  { background: #bebbdc; color: #474187; border: 1px solid #4b3ecf; }

        /* ===== BUTTONS ===== */
        .btn {
            border: none;
            border-radius: 10px;
            padding: 0px 16px;
            font-size: 14px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.18s;
            display: inline-flex;
            align-items: center;
            gap: 5px;
            height: 34px !important;
        }
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

        /* ===== SEARCH PANEL ===== */
        .search-tabs {
            display: flex;
            gap: 4px;
            margin-bottom: 12px;
            border-bottom: 2px solid #d0dbe8;
        }
        .search-tab {
            padding: 7px 16px;
            border: none;
            background: none;
            font-size: 12px;
            font-weight: 600;
            color: #7f8c8d;
            cursor: pointer;
            border-bottom: 2px solid transparent;
            margin-bottom: -2px;
            transition: all 0.15s;
        }
        .search-tab.active {
            color: #2980b9;
            border-bottom-color: #2980b9;
        }
        .search-tab:hover { color: #2980b9; }

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
        table.data-table tbody tr { border-bottom: 1px solid #e8eef4; }
        table.data-table tbody tr:hover { background: #f0f7ff; }
        table.data-table tbody td { padding: 8px 10px; vertical-align: middle; }
        table.data-table tbody tr:nth-child(even) { background: #f8fbfe; }
        table.data-table tbody tr:nth-child(even):hover { background: #f0f7ff; }

        .no-data { text-align: center; color: #aaa; padding: 24px; font-style: italic; }

        /* ===== CHANGE LIST TABLE - editable columns ===== */
        .change-table input[type="text"] {
            width: 100%;
            border: 1.5px solid #c8d6e5;
            border-radius: 4px;
            padding: 4px 7px;
            font-size: 12px;
            min-width: 90px;
        }
        .change-table input:focus {
            outline: none;
            border-color: #2980b9;
        }

        /* ===== DRAFT HISTORY ===== */
        .draft-banner {
            background: #fffbea;
            border: 1.5px dashed #f0d060;
            border-radius: 6px;
            padding: 5px 14px;
            margin-bottom: 10px;
            display: flex;
            align-items: center;
            gap: 10px;
            font-size: 13px;
            color: #7d6608;
        }
        .draft-banner strong { color: #b7950b; }

        /* ══════════════════════════════════════
           ACTION BAR (sticky bottom)
        ══════════════════════════════════════ */
        .action-bar {
            position: fixed; bottom: 10px; right: 10px;   /*left: 18%;*/
            background: #aec7e9ab;
            border-top: 1px solid var(--border);
            box-shadow: 0 -4px 20px rgba(0,0,0,.08);
            padding: 10px 24px;
            display: flex; align-items: center;
            justify-content: space-between; gap: 12px;
            z-index: 100;
            border-radius:10px;
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

        /* ===== ALERT ===== */
        .alert {
            border-radius: 6px;
            padding: 9px 14px;
            font-size: 12px;
            margin-bottom: 12px;
            display: none;
        }
        .alert-info    { background: #eaf6fb; color: #1a6a9a; border-left: 4px solid #2980b9; }
        .alert-success { background: #eafaf1; color: #1e8449; border-left: 4px solid #27ae60; }
        .alert-danger  { background: #fdedec; color: #922b21; border-left: 4px solid #e74c3c; }

        /* ===== SEPARATOR ===== */
        .sep { height: 1px; background: #e8eef4; margin: 12px 0; }

        /* ===== RESPONSIVE ===== */
        @media (max-width: 768px) {
            .form-row { flex-direction: column; }
            .action-bar { flex-direction: column; }
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

    <%--Select2 : Dropdown Search (แบบ EditLocation)--%>
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

    <style>
        .grid-headerTopic {
            display: grid;
            grid-template-columns: 70px auto 180px 130px 40px;  /* icon | title | mode | manual | readme */
            grid-template-rows: 50px;
            background-color:#94c2ff;
            border-radius:10px;
            margin: 10px 10px 10px 10px;
        }
        .grid_search{
            display: grid;
            grid-template-columns: 320px Auto;
            gap: 5px;
        }
        .grid_search2{
            display: grid;
            grid-template-rows: 30% Auto;
        }
        .grid_footer{
            display: grid;
            grid-template-columns: 140px Auto 130px 170px;
            gap: 4px;
        }
        .header_Style1 {
            border-radius: 10px;
            align-content: center;
            font-size: 30px;
            margin-bottom:0px;
        }
        #IssueDatePicker .input-group-addon {
            width: 25%;
        }
        #IssueDatePicker.dp-compact .input-group-addon {
            height: 28px !important;
            padding: 0 8px !important; /* ไม่มี padding บน-ล่าง จะเตี้ยลงจริง */
            line-height: 30px !important; /* ทำให้ไอคอนอยู่กลางแนวตั้ง */
            display: inline-block; /* กันบางธีมที่ตั้ง display อื่นไว้ */
            vertical-align: middle;
        }
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
        .grid-pager {
            text-align: right;
        }
        .page-note {
            /*background: linear-gradient(135deg, #1a3a5c 0%, #2980b9 100%);*/
            /*color: #fff;*/
            padding: 5px 10px;
            display: flex;
            flex-wrap: wrap;
            align-items: center;
            gap: 10px;
        }
        .page-note .badge {
            /*background: #2980b9;*/
            color: #fff;
            border-radius: 6px;
            padding: 4px 10px;
            font-size: 12px;
            font-weight: 500;
            text-shadow:none;
        }
        .ddl_edit{
            font-size: 12px;
            text-align: center;
        }      
        .ddl-warning {
            background-color: #fff3cd; /* เหลืองอ่อน */
            border: 1px solid #ffc107;
        }        
        .disabled-dropdown {
            background-color: #e9ecef;
            pointer-events: none;
        }
        /* ✅ Target select2 container ที่อยู่ถัดจาก ddl-warning */
        select.ddl-warning + .select2-container .select2-selection--single {
            border: 2px solid #ffc107 !important;
            background-color: #fff3cd !important;
        }

        /* ✅ กรณี select2 container อยู่ใน sibling ถัดไป */
        .ddl-warning + .select2-container > .select2-selection {
            border: 2px solid #ffc107 !important;
            background-color: #fff3cd !important;
        }
    </style>

    <%--tooltop--%>
    <style>
        .tooltip-wrap {
            position: relative;
            display: inline-block;
        }
        .tooltip-wrap .tooltip-box {
            visibility: hidden;
            opacity: 0;
            width: max-content;
            max-width: 220px;
            background: #2c3e50;
            color: #fff;
            text-align: center;
            border-radius: 6px;
            padding: 7px 12px;
            font-size: 12px;
            white-space: nowrap;

            position: absolute;
            z-index: 9999;
            bottom: 125%;            /* แสดงด้านบนปุ่ม */
            left: 50%;
            transform: translateX(-50%) translateY(4px);
            transition: opacity .2s ease, transform .2s ease;
            box-shadow: 0 4px 12px rgba(0,0,0,.25);
            pointer-events: none;
        }
        /* ลูกศรชี้ลง */
        .tooltip-wrap .tooltip-box::after {
            content: "";
            position: absolute;
            top: 100%;
            left: 50%;
            margin-left: -6px;
            border-width: 6px;
            border-style: solid;
            border-color: #2c3e50 transparent transparent transparent;
        }
        .tooltip-wrap:hover .tooltip-box {
            visibility: visible;
            opacity: 1;
            transform: translateX(-50%) translateY(0);
        }
    </style>

    <%--style column gvBorrowlist--%>
    <style>
        .zone-divider {
            border-left: 4px solid #1a3a5c !important;
        }

        .col-current {
            background-color: #f1f3f5 !important;
            color: #555;
            font-size: 12px;
            text-align: center;
        }
        .col-use {
            background-color: #eafaf1 !important;
            text-align: center;
        }

        .hdr-current {
            background-color: #6c757d !important;
            color: #fff !important;
            font-weight: 700;
            text-align: center;
            padding: 4px;
        }
        .hdr-use {
            background-color: #27ae60 !important;
            color: #fff !important;
            font-weight: 700;
            text-align: center;
            padding: 4px;
        }
        .hdr-use.zone-divider {
            border-left: 4px solid #1a3a5c !important;
        }
    </style>

    <%--READ ME + MANUAL--%>
    <style>
        .manual-btn {
            display:inline-flex; align-items:center; gap:5px;
            background:#fff; color:#2980b9; font-weight:600; font-size:13px;
            padding:5px 12px; border-radius:20px; text-decoration:none;
            margin:0 6px; transition:all .15s; cursor:pointer;
        }
        .manual-btn:hover { background:#2980b9; color:#fff; text-decoration:none; }

        .manual-menu {
            display:none; position:absolute; top:100%; right:0; margin-top:4px;
            background:#fff; border:1px solid #d0dbe8; border-radius:8px;
            box-shadow:0 4px 16px rgba(0,0,0,.15); z-index:2000; min-width:200px;
            overflow:hidden;
        }
        .manual-menu a {
            display:flex; align-items:center; gap:8px;
            padding:9px 14px; font-size:13px; color:#2c3e50; text-decoration:none;
        }
        .manual-menu a:hover { background:#eaf6fb; color:#2980b9; }
        .manual-menu.show { display:block; }

        .readme-btn {
            font-size:24px; color:#4f4b4b; cursor:pointer;
            transition:color .15s, transform .15s;
        }
        .readme-btn:hover { color:#2980b9; transform:scale(1.1); }

        /* Read Me modal — override h4 aliceblue เดิม */
        #modal_ReadMe .modal-dialog { width:auto; max-width:640px; margin:40px auto; }
        #modal_ReadMe .modal-content { border-radius:10px; overflow:hidden; }
        #modal_ReadMe .modal-header {
            background:linear-gradient(135deg,#1a3a5c 0%,#2980b9 100%); border-bottom:none; padding:12px 18px;
        }
        #modal_ReadMe .modal-header .close { color:#fff; opacity:.9; text-shadow:none; font-size:26px; }
        #modal_ReadMe .modal-title { color:#fff !important; font-size:17px; font-weight:700; }
        #modal_ReadMe .modal-body { padding:18px 22px; font-size:13px; color:#2c3e50; background:#fff; max-height:none; }
        #modal_ReadMe .rm-item {
            display:flex; align-items:flex-start; gap:10px; padding:8px 0;
            border-bottom:1px solid #eef0f4;
        }
        #modal_ReadMe .rm-item:last-child { border-bottom:none; }
        #modal_ReadMe .rm-ico {
            width:30px; height:30px; flex-shrink:0;
            display:flex; align-items:center; justify-content:center;
            background:#eaf6fb; border-radius:6px; color:#2980b9; font-size:14px;
        }
        #modal_ReadMe .rm-txt strong { color:#1a3a5c; display:block; margin-bottom:2px; }
        #modal_ReadMe .rm-txt span { color:#6b7a99; }

        /* Upload Manual modal */
        #modal_UploadManual .modal-dialog { width:auto; max-width:480px; margin:60px auto; }
        #modal_UploadManual .modal-header {
            background:linear-gradient(135deg,#1a3a5c 0%,#f39c12 100%); border-bottom:none;
        }
        #modal_UploadManual .modal-title { color:#fff !important; }
        #modal_UploadManual .modal-body { background:#fff; max-height:none; padding:18px; }
    </style>

    <%--Manual open / menu--%>
    <script>
        // path ผ่าน Virtual Directory /ManualPath (& ต้อง encode = %26)
        var MANUAL_BASE = '/Tool_Equipment_Control_System/ManualPath/08_Tool%26Equipment/01_Borrow/';
        var MANUAL_FILES = ['Step1', 'Step2', 'Step3', 'Step4', 'SaveDraft', 'SendRequest', 'CancelRequest', 'Confirm', 'Return' , 'Reject'];

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
    BORROW — THEME OVERRIDE  (Navy #1a3a5c based)
    ────────────────────────────────────────────────────────────────
    วิธีใช้ : วางทั้งบล็อกนี้ไว้ "ท้ายสุด" ของกลุ่ม <style> ในหน้า
             (ต่อจาก <style> ก้อนสุดท้าย ก่อน <asp:ScriptManager>)
    ผลลัพธ์ : override สีเดิมทั้งหมดให้เป็นระบบ token โทน Navy
    ย้อนกลับ: ลบบล็อกนี้ทิ้ง หน้าจะกลับเป็นสีเดิมทันที
    =====================================================================--%>
    <style>
        :root {
            /* Navy scale */
            --nv-900:#12293f; --nv-800:#1a3a5c; --nv-700:#245177; --nv-600:#2980b9;
            --nv-500:#4a90c2; --nv-200:#aecbe6; --nv-100:#dbe7f2; --nv-050:#eef4fa;
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
        .page-header{ background:linear-gradient(135deg,var(--nv-800) 0%,var(--nv-600) 100%) !important; }
        .grid-headerTopic{ background-color:var(--nv-200) !important; }
        .header_Style1{ color:var(--nv-800) !important; }

        /* ===== CARD ===== */
        .card{ box-shadow:var(--sh-1) !important; }
        .card-header{
            background:var(--nv-050) !important;
            border-bottom:1px solid var(--border) !important;
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
            background:rgba(174,203,230,.72) !important;
            box-shadow:var(--sh-2) !important;
            border-top:1px solid var(--border) !important;
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

    <style>
        /* badge ที่เป็นลิงก์ไปเอกสาร */
        a.badge.badge-link { text-decoration: none; cursor: pointer; }
        a.badge.badge-link:hover { filter: brightness(.9); text-decoration: underline; color: #fff; }
        .proc-doc { margin-top: 3px; font-size: 11px; color: var(--danger); font-weight: 600; }

        /* แถวที่ติดกระบวนการ → พื้นแดงอ่อน */
        #<%= gvSearchResult.ClientID %> tr.row-locked td { background: #fbeceb !important; }
    </style>

    <style>
        /* ===== Modal : เหตุผล Reject / Cancel ===== */
        .rjm-overlay{
            position:fixed; inset:0; z-index:2000;
            background:rgba(18,41,63,.55);
            align-items:center; justify-content:center; padding:16px;
        }
        .rjm-box{
            width:100%; max-width:560px; background:#fff; border-radius:10px;
            box-shadow:0 12px 40px rgba(0,0,0,.25); overflow:hidden;
            animation:rjmIn .18s ease-out;
        }
        @keyframes rjmIn{ from{opacity:0; transform:translateY(-12px);} to{opacity:1; transform:none;} }

        .rjm-head{
            display:flex; align-items:center; gap:8px; padding:12px 16px;
            background:linear-gradient(135deg,#8f2b22 0%,var(--danger) 100%);
            color:#fff; font-size:16px; font-weight:700;
        }
        .rjm-x{ margin-left:auto; background:none; border:none; color:#fff; font-size:24px; line-height:1; cursor:pointer; opacity:.9; }
        .rjm-body{ padding:16px 18px 6px; }
        .rjm-doc{ font-size:13px; color:var(--text-sub); margin-bottom:6px; }
        .rjm-desc{
            font-size:13px; padding:8px 12px; border-radius:6px; margin-bottom:12px;
            background:var(--danger-bg); border-left:4px solid var(--danger); color:#8f2b22;
        }
        .rjm-label{ display:block; font-size:13px; font-weight:600; color:var(--text); margin-bottom:4px; }
        .rjm-count{ font-weight:400; font-size:12px; color:var(--text-sub); }
        .rjm-text{ width:100% !important; height:96px !important; resize:vertical; font-size:13px; }
        .rjm-text.rjm-err{ border-color:var(--warn) !important; background:#fff3cd !important; }
        .rjm-err-msg{ color:var(--danger); font-size:12px; margin-top:4px; }
        .rjm-foot{ display:flex; justify-content:flex-end; gap:8px; padding:12px 16px; border-top:1px solid var(--border-soft); }

        /* โหมด Cancel → โทนเทา */
        .rjm-overlay.rjm-cancel .rjm-head{ background:linear-gradient(135deg,#4c4c4c 0%,#7c8896 100%); }
        .rjm-overlay.rjm-cancel .rjm-desc{ background:#ececec; border-left-color:#7c8896; color:#4c4c4c; }
        .rjm-overlay.rjm-cancel .rjm-ok{ background:#7c8896 !important; }
    </style>

    <%--Manual (Step) + Mode Guide + Requester Source--%>
    <style>
        /* ปุ่มคู่มือที่หัว Step */
        .step-manual{
            display:inline-flex; align-items:center; gap:4px;
            margin-left:10px; padding:2px 10px; border-radius:12px;
            font-size:12px; font-weight:600; text-decoration:none !important;
            color:var(--nv-600); background:#fff; border:1px solid var(--info-line);
            transition:all .15s;
        }
        .step-manual:hover{ background:var(--nv-600); color:#fff; }

        /* เมนู Manual ที่ Header */
        .manual-menu .manual-sep{ height:1px; background:var(--border-soft); margin:4px 0; }
        .manual-menu .manual-grp{ padding:6px 14px 2px; font-size:11px; color:var(--text-sub); font-weight:700; }

        /* แถบนำทางตามโหมด (Confirm / Return) */
        .mode-guide{
            display:flex; align-items:center; flex-wrap:wrap; gap:10px 18px;
            margin:0 10px 12px; padding:10px 16px; border-radius:8px;
            background:var(--info-bg); border:1px solid var(--info-line); color:var(--nv-800);
        }
        .mode-guide.mg-return{ background:var(--ok-bg); border-color:var(--ok-line); }
        .mode-guide .mg-title{ font-weight:700; }
        .mode-guide ol{ display:flex; flex-wrap:wrap; gap:4px 18px; margin:0; padding-left:18px; font-size:13px; }
        .mode-guide .mg-btn{
            margin-left:auto; display:inline-flex; align-items:center; gap:5px;
            padding:5px 14px; border-radius:16px; background:var(--nv-600); color:#fff !important;
            font-weight:600; font-size:13px; text-decoration:none !important;
        }
        .mode-guide.mg-return .mg-btn{ background:var(--ok); }

        /* แจ้งเตือนแหล่งข้อมูล Requester */
        .req-src-banner{
            margin:0 10px 8px; padding:8px 14px; border-radius:6px; font-size:13px;
            background:var(--amber-soft-bg); border-left:4px solid var(--warn); color:var(--amber-soft-text);
        }
        .req-src-banner.req-src-none{ background:var(--danger-bg); border-left-color:var(--danger); color:#8f2b22; }
        .req-src-banner b{ display:block; margin-bottom:2px; }

        /* ช่องที่แสดงข้อมูลจาก Snapshot (ต้องอยู่หลัง .form-group input[readonly]) */
        .form-group input.fld-snapshot{
            background:var(--amber-soft-bg) !important;
            border-color:var(--amber-soft-line) !important;
            color:var(--amber-soft-text) !important;
        }
    </style>
  

    <%--Step 3 : Borrow List — รูปแบบตารางเดียวกับ withdraw2 (Current → Use , Fix คอลัมน์ , เส้นดำ 1px)--%>
    <style>
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

            <%---- Toast Container ----%>
            <div id="toast-container" aria-live="polite" aria-atomic="true"></div>

            <div>

                <%--Header--%>
                <div class="grid-headerTopic">
                    <div style="text-align:center; align-content:center;">
                        <img class="" style="height:38px;" src="images/return.png" />
                    </div>

                    <label class="header_Style1" style="">
                        การร้องขอ<u>ยืมอุปกรณ์</u> : Jig Tool , Equipment </label>

                    <asp:Label ID="lb_mode" runat="server" Text="Mode : Issue"
                        style=" text-align: center; align-content: center;
                                background: #fff; margin: 10px 10px;
                                border-radius: 30px; font-weight: 600; color: #4f4b4b;">
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
                            <a href="#" onclick="return openManualFile('Step3');"><span class="glyphicon glyphicon-list-alt"></span> Step 3 : Borrow List</a>
                            <a href="#" onclick="return openManualFile('Step4');"><span class="glyphicon glyphicon-comment"></span> Step 4 : Comment</a>
                            <a href="#" onclick="return openManualFile('SaveDraft');"><span class="glyphicon glyphicon-pencil"></span> Save Draft</a>
                            <a href="#" onclick="return openManualFile('SendRequest');"><span class="glyphicon glyphicon-send"></span> Send Request</a>
                            <a href="#" onclick="return openManualFile('CancelRequest');"><span class="glyphicon glyphicon-remove-circle"></span> Cancel</a>
                            <div class="manual-sep"></div>
                            <div class="manual-grp">AAD</div>
                            <a href="#" onclick="return openManualFile('Confirm');"><span class="glyphicon glyphicon-ok-sign"></span> AAD Confirm</a>
                            <a href="#" onclick="return openManualFile('Return');"><span class="glyphicon glyphicon-import"></span> Return</a>
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

                    <%-- HiddenField : บอก JS ว่าเป็น admin ไหม --%>
                    <asp:HiddenField ID="hdIsAdmin" runat="server" Value="0" />
                </div>

                <%-- แถบนำทาง + ปุ่มคู่มือ (Status 50 = Confirm / Status 90 = Return) --%>
                <asp:Literal ID="litModeGuide" runat="server" Visible="false" />

                <%--=====================================================--%>
                <%------------ SECTION 1 : Employee ID (Issue By) ---------%>
                <%--=====================================================--%>

                <div class="card" style="margin-left:10px; margin-right:10px;">

                    <div class="card-header">
                        <span>
                            <asp:Literal ID="step_requester" runat="server" Text="<span class='badge'>Step 1</span>" />
                             &nbsp;Requester : ลงชื่อผู้ร้องขอ
                            <%--<a id="btnManual1" runat="server" href="#" class="step-manual"
                               onclick="return openManualFile('Step1');" title="คู่มือ Step 1">
                                <span class="glyphicon glyphicon-book"></span> คู่มือ
                            </a>--%>
                        </span>

                        <!-- Issue No. and Status -->
                        <span style="margin-left:auto; font-size:16px; color:#555; font-weight:400;">
                            Issue No.: 
                            <strong>
                                <span class="badge-issue">
                                    <asp:Literal ID="litIssueNo" runat="server" Text="-" />
                                </span>       
                            </strong>
                            &nbsp;|&nbsp;
                            Status : <asp:Literal ID="litStatus" runat="server" Text="<span class='status-badge status-draft'>Wait Issue</span>" />
                        </span>
                    </div>

                    <div class="card-body">

                        <!-- แจ้งเตือน : ข้อมูลผู้ร้องขอมาจาก Record (ไม่พบใน HRMS) -->
                        <asp:Panel ID="pnlReqSource" runat="server" Visible="false">
                            <asp:Literal ID="litReqSource" runat="server" />
                        </asp:Panel>

                        <!-- Draft Banner () -->
                        <asp:Panel ID="pnlDraftBanner" runat="server" Visible="false">
                            <div class="draft-banner">
                                ขออภัยพบรายการ <strong> Draft </strong> ที่ยังไม่ได้ส่ง
                                (Issue No.: <strong><asp:Literal ID="litDraftIssueNo" runat="server" /></strong>
                                โดยวันที่ : <asp:Literal ID="litDraftDate" runat="server" />)
                                &nbsp;โดยระบบได้โหลดรายการนั้นขึ้นมาแล้ว
                            </div>
                        </asp:Panel>

                        <div class="form-row">

                            <!-- Emp ID Input -->
                            <div class="form-group" style="width:10%; margin-left:10px;">
                                <label>EMP ID *</label>
                                <asp:TextBox ID="txtEmpId" runat="server" onkeyup="fetchRequesterName();"
                                    CssClass="" MaxLength="6" Width="100%"
                                    placeholder="รหัสพนักงาน" Style="text-align:center; background-color:#00a7ff40;"
                                    AutoComplete="off"  AutoPostBack ="true"
                                    OnTextChanged="txtEmpId_TextChanged" />
                            </div>

                            <!-- Tel Input -->
                            <div class="form-group" style="width:8%;">
                                <label>TEL *</label>
                                <asp:TextBox ID="txtTel" runat="server" 
                                    CssClass="" MaxLength="4" Width="100%"
                                    placeholder="เบอร์ติดต่อ" Style="text-align:center; background-color:#00a7ff40;" />
                            </div>

                            <!-- Employee Name -->
                            <div class="form-group" style="width:22%;">
                                <label>EMP Name</label>
                                <asp:TextBox ID="txtEmpName" runat="server" ReadOnly="true" Width="100%"
                                    CssClass="" Style="text-align:center;" placeholder="ชื่อพนักงาน" />
                            </div>

                            <!-- Position -->
                            <div class="form-group" style="width:17%;">
                                <label>Position</label>
                                <asp:TextBox ID="txtPosition" runat="server" ReadOnly="true" Width="100%"
                                    CssClass="" Style="text-align:center;" placeholder="ตำแหน่ง" />
                            </div>

                            <!-- Department -->
                            <div class="form-group" style="width:10%;">
                                <label> Department</label>
                                <asp:TextBox ID="txtDept" runat="server" ReadOnly="true" Width="100%"
                                    CssClass="" Style="text-align:center;" placeholder="แผนก" />
                            </div>

                            <!-- Dept Code -->
                            <div class="form-group" style="width:9%;">
                                <label> Dept Code</label>
                                <asp:TextBox ID="txtDeptCode" runat="server" ReadOnly="true" Width="100%"
                                    CssClass="" Style="text-align:center;" placeholder="Dept Code" />
                            </div>

                            <!-- Issue Date (readonly = today) -->
                            <div class="form-group" style="width:15%;">
                                <label>Issue Date</label>
                                <div class="input-group date dp-compact" id="IssueDatePicker">
                                    <asp:TextBox ID="txtIssueDate" runat="server" Width="75%"
                                        CssClass="" Style="text-align:center; border-top-right-radius:0px; border-bottom-right-radius:0px;" 
                                        placeholder="วันที่ Issue" ReadOnly="true"
                                        AutoPostBack="true"/>
                                    <span class="input-group-addon" style="width:32px; margin-top: -5px;">
                                        <span class="glyphicon glyphicon-calendar"></span>
                                    </span>
                                </div>
                            </div>

                            <!--HD : Band -->
                            <asp:HiddenField ID="hdtxtBand" runat="server" />
                            <!--HD : E-Mail -->
                            <asp:HiddenField ID="hdtxtEmail" runat="server" />
                            <!-- Hidden field : New Member -->
                            <asp:HiddenField ID="hdNewMember_ck" runat="server" Value="-"/>                                       

                        </div>

                        <!-- Emp notice -->
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
                             &nbsp;Search & Add List : ค้นหาข้อมูลและเพิ่มรายการการร้องขอยืมอุปกรณ์
                           <%-- <a id="btnManual2" runat="server" href="#" class="step-manual"
                               onclick="return openManualFile('Step2');" title="คู่มือ Step 2">
                                <span class="glyphicon glyphicon-book"></span> คู่มือ
                            </a>--%>
                        </span>

                    </div>

                    <div class="card-body" style="margin-bottom:10px;">

                        <!-- Tab: Search -->
                        <div id="tabFixAsset" class="grid_search">

                            <div class="grid_search2">

                                <!-- ✅ Checkbox Group -->
                                <div class="form-group" style="margin-bottom:5px; margin-left:60px; flex-direction:row; align-items:center;">
                                    <asp:CheckBox ID="chkSpare" runat="server" Text=" Spare"
                                        Checked="true"
                                        onclick="toggleFixAssetCheck(this);" />

                                    &nbsp;&nbsp;

                                    <asp:CheckBox ID="chkAll" runat="server" Text=" All"
                                        onclick="toggleFixAssetCheck(this);" />
                                </div>

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
                                    <%--<span class="badge">5505 [Dept Code]</span>--%>
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

                                        <%--Display Status : ติดกระบวนการ AAD1–AAD4 → badge แดง + ลิงก์เอกสาร--%>
                                        <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="headerGv1" ItemStyle-Width="150px"
                                            ItemStyle-CssClass="rowGv1" ItemStyle-HorizontalAlign="Center">
                                            <ItemTemplate>
                                                <asp:HiddenField ID="hdstatusId" runat="server" Value='<%# Eval("status_id") %>' />
                                                <%# BuildStatusBadge(Eval("DisplayStatus"), Eval("ProcUrl"), Eval("ProcDoc")) %>
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

                </asp:Panel>  

                <%--=====================================================--%>
                <%------------ SECTION 3 : Borrow List --------------------%>
                <%--=====================================================--%>

                <div class="card" style="margin-left:10px; margin-right:10px;">

                    <div class="card-header">
                        <span>
                            <asp:Literal ID="step_borrowlist" runat="server" Text="<span class='badge'>Step 3</span>" />
                             &nbsp;Borrow List : รายการที่ร้องขอยืม , ลงข้อมูลสถานที่นำไปใช้งาน
                            <%--<a id="btnManual3" runat="server" href="#" class="step-manual"
                               onclick="return openManualFile('Step3');" title="คู่มือ Step 3">
                                <span class="glyphicon glyphicon-book"></span> คู่มือ
                            </a>--%>
                        </span>
                    </div>

                    <div class="card-body" style="margin-bottom:10px;">
                        
                        <%--Borrow List--%>
                        <div class="tbl-wrap step3-wrap" style="max-height: 350px;">
                            <asp:GridView ID="gvBorrowList" runat="server" DataKeyNames="ToolId,ToolSub,SystemId"
                                CssClass="data-table change-table fixed-header fx-table"
                                style="min-width:1800px;"
                                AutoGenerateColumns="false"
                                OnRowDataBound="gvBorrowList_RowDataBound"
                                OnRowCommand="gvBorrowList_RowCommand"
                                EmptyDataText="ไม่มีรายการแจ้งเปลี่ยนแปลง">

                                <%--OnRowCommand="gvChangeList_RowCommand"--%>
                                <%--OnRowDataBound="gvChangeList_RowDataBound"--%>

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

                                    <%--Picture + keys--%>
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
                                        HeaderStyle-CssClass="hdr-current"
                                        ItemStyle-CssClass="col-current" />

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

                                    <%--Use Dept = txtDeptCode ผู้ยืม (เริ่มโซน → zone-divider)--%>
                                    <asp:TemplateField HeaderText="Use : Dept"
                                        HeaderStyle-CssClass="hdr-use zone-divider" ItemStyle-CssClass="col-use zone-divider"
                                        ItemStyle-Width="90px">
                                        <ItemTemplate>
                                            <asp:Label ID="lblDeptNew" runat="server" />
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
                <%------------ SECTION 4 : Comment Borrow -----------------%>
                <%--=====================================================--%>

                <div class="card" style="margin-left:10px; margin-right:10px;">

                    <div class="card-header">
                        <span>
                            <asp:Literal ID="step_comment" runat="server" Text="<span class='badge'>Step 4</span>" />
                             &nbsp;Comment : เหตุผลการขอยืมอุปกรณ์
                           <%-- <a id="btnManual4" runat="server" href="#" class="step-manual"
                               onclick="return openManualFile('Step4');" title="คู่มือ Step 4">
                                <span class="glyphicon glyphicon-book"></span> คู่มือ
                            </a>--%>
                        </span>

                        <span style="margin-left:auto; font-size:11px; font-weight:400; color:#555;">
                            <asp:Literal ID="borrow_remark" runat="server"  />    
                        </span>
                    </div>

                    <div class="card-body" style="margin-bottom:10px;">                        

                        <div class="form-row" style="align-items: self-start;">

                            <%--Dropdown : Reason Borrow--%>
                            <div class="form-group" style="width:180px; margin-left:10px; gap:5px;">
                                <label>เหตุผลการยืม *</label>
                                <asp:DropDownList ID="ddlReason_Borrow" runat="server" AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlReason_Borrow_SelectedIndexChanged"
                                    CssClass="form-control form-control-sm ddl_edit"
                                    Style="min-width:170px; padding: 2px 5px;">
                                </asp:DropDownList>
                            </div>

                            <%-- Reason Borrow : Comment --%>
                            <div class="form-group" style="width: calc(100% - 250px); margin-left:10px;">
                                <span style="text-align:left; font-size:12px; font-weight:600; color:#555;">Comment &nbsp;
                                    <span style="text-align:right; font-size:12px; font-weight:500; color:#666;">
                                        (จำนวนตัวอักษร :
                                        <span id="charCount" style="color:#ff0303;">
                                            0
                                        </span>
                                        / 200 )
                                        <span style="color:red;">** ลงข้อมูลอย่างน้อย 10 ตัวอักษร **</span>
                                    </span>
                                </span>

                                <asp:TextBox ID="txt_borrowComment" runat="server" 
                                    TextMode="MultiLine"
                                    Rows="4"
                                    CssClass="form-control"
                                    MaxLength="200"
                                    onkeyup="countChar(this)"
                                    style ="width:100%; Height:50px !important;"
                                    placeholder="กรุณาใส่เหตุผลในการร้องข้อยืม">

                                </asp:TextBox>
                            </div>

                        </div>

                        <%--Note--%>
                        <div ID="borrow_note" runat="server">
                            <div class="form-row" style="align-content:center;">
                                <div class="page-note">
                                    <span class="badge" style="background:#ff5d5d;">หมายเหตุ :</span>                                    
                                    <div style="display: inline-grid;text-align-last: left; gap:5px;">
                                        <span class="badge" style="background:#e4ff5d; color:#000; font-size:13px;">1.การยืมกำหนดระยะเวลาการยืม 7 วัน นับตั้งแต่วันที่ยืม</span>                                 
                                        <span class="badge" style="background:#e4ff5d; color:#000; font-size:13px;">2.กรณีที่ผู้ยืมจำเป็นต้องใช้เครื่องมือในระยะเวลาที่ติดต่อกัน แต่ครบกำหนดการคืน ให้นำเครื่องมือมาคืน และให้ยืมใหม่</span>
                                    </div>
                                    <%--<span class="badge" style="background:#e4ff5d; color:#000; font-size:13px;">3.ระบบการยืมนี้ใช้กรณีที่ยืมเครื่องมือชั่วคราวเท่านั้น</span>--%>
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
                        <span><span class="glyphicon glyphicon-time"></span>&nbsp;ประวัติ Reject / Cancel</span>
                        <span style="margin-left:auto; font-size:11px; font-weight:400; color:#555;">
                            เรียงจากล่าสุด
                        </span>
                    </div>

                    <div class="card-body" style="margin-bottom:10px;">
                        <div class="tbl-wrap" style="max-height: 150px;">
                            <asp:GridView ID="gv_rejectHistory" runat="server"
                                CssClass="data-table change-table fixed-header"
                                AutoGenerateColumns="false"
                                EmptyDataText="ไม่มีประวัติการ Reject">

                                <EmptyDataRowStyle CssClass="no-data" />

                                <Columns>
                                    <asp:BoundField DataField="Reject_Count" ItemStyle-Width="60px"
                                        HeaderText="No" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1"/>
                                    <asp:BoundField DataField="Type" ItemStyle-Width="80px"
                                        HeaderText="Type" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1"/>
                                    <asp:BoundField DataField="Reject_by" ItemStyle-Width="180px"
                                        HeaderText="By" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1"/>
                                    <asp:BoundField DataField="status_name" ItemStyle-Width="140px"
                                        HeaderText="Process" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1"/>
                                    <%-- ✅ แก้ HH:MM → HH:mm (MM = เดือน) --%>
                                    <asp:BoundField DataField="Reject_Date" ItemStyle-Width="140px"
                                        HeaderText="Date" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1"
                                        DataFormatString="{0:dd-MMM-yyyy HH:mm}" HtmlEncode="false"/>
                                    <asp:BoundField DataField="Reject_detail"
                                        HeaderText="Detail" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv2"/>
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
                            <span class="tooltip-wrap">
                                <asp:LinkButton ID="btnCancel" runat="server"
                                    CssClass="btn btn-secondary" style="font-size:12px; text-shadow:none;"
                                    OnClientClick="return openRejectModal('cancel');">
                                    <span class="glyphicon glyphicon-ban-circle"></span>
                                    Cancel Request
                                </asp:LinkButton>
                                <span class="tooltip-box">ยกเลิกการร้องขอ</span>
                            </span>
                        </div>
                    </div>

                    <div class="action-bar-right" style="">

                        <%--Draft Button--%>   <%-- ปุ่ม Save Draft : แสดงเฉพาะ User ปกติ (ซ่อนเมื่อ ADMIN) --%>
                        <div id="DraftButton" runat="server" Visible="true">
                            <span class="tooltip-wrap" style="">
                                <asp:LinkButton ID="btnDraft" runat="server" 
                                    CssClass="btn btn-warning" style="margin-right:10px;"
                                    OnClick="btnDraft_Click"
                                    OnClientClick="return confirmDraft();"
                                    title="บันทึกข้อมูลชั่วคราว ยังไม่ส่งร้องขอเบิก">
                                    <span class="glyphicon glyphicon-exclamation-sign"></span>
                                        Save Draft                                
                                </asp:LinkButton>
                                <span class="tooltip-box">บันทึกข้อมูลชั่วคราว</span>  
                            </span>
                        </div>

                        <%--Reject Button--%>
                        <div id="RejectButton" runat="server" Visible="false">
                            <span class="tooltip-wrap">
                                <asp:LinkButton ID="btnReject" runat="server"
                                    CssClass="btn btn-danger" style="margin-right:10px;"
                                    OnClientClick="return openRejectModal('reject');">
                                    <span class="glyphicon glyphicon-remove"></span>
                                        Reject
                                </asp:LinkButton>
                                <span class="tooltip-box">ปฎิเสธการร้องขอ</span>
                            </span>
                        </div>

                        <%--Send Request Button--%>
                        <div id="SendButton" runat="server" Visible="true">
                            <span class="tooltip-wrap" style="">
                                <asp:LinkButton ID="btnSend" runat="server"
                                    CssClass="btn btn-success"
                                    OnClick="btnSend_Click"
                                    OnClientClick="return sendtoApprove();"
                                    title="ส่งรายการเพื่อขอร้องเบิก">                                                                        
                                    <span class="glyphicon glyphicon-send"></span>
                                        Send Request
                                </asp:LinkButton> 
                                <span class="tooltip-box">ส่งรายการร้องขอ</span>  
                            </span>
                        </div>
            
                        <%--Confirm Button--%>
                        <div id="ConfirmButton" runat="server" Visible="false">
                            <span class="tooltip-wrap" style="">
                                <asp:LinkButton ID="btnConfirm" runat="server" 
                                    CssClass="btn btn-success"
                                    OnClick="btnConfirm_Click"
                                    OnClientClick="return AADConfirm();"
                                    data-toggle="tooltip"
                                    data-placement="top"
                                    title="ยืนยันการร้องขอ">                                                                        
                                    <span class="glyphicon glyphicon-ok-sign"></span>
                                        AAD Confirm
                                </asp:LinkButton>
                                <span class="tooltip-box">ยืนยันการร้องขอ</span>  
                            </span>
                        </div>

                        <%--Confirm Button--%>
                        <div id="ConfirmButton2" runat="server" Visible="false">
                            <span class="tooltip-wrap" style="">
                                <asp:LinkButton ID="btnConfirm2" runat="server"
                                    CssClass="btn btn-success"
                                    OnClick="btnConfirm2_Click"
                                    OnClientClick="return AADConfirm();"
                                    data-toggle="tooltip"
                                    data-placement="top"
                                    title="ยืนยันการร้องขอ">                                                                        
                                    <span class="glyphicon glyphicon-ok-sign"></span>
                                        AAD Confirm
                                </asp:LinkButton>
                                <span class="tooltip-box">ยืนยันการร้องขอ</span>  
                            </span>
                        </div>
        
                        <%--Return Button--%>
                        <div id="ReturnButton" runat="server" Visible="false">
                            <span class="tooltip-wrap" style="">
                                <asp:LinkButton ID="btnReturn" runat="server"
                                    CssClass="btn btn-success"
                                    OnClick="btnReturn_Click"
                                    
                                    data-toggle="tooltip"
                                    data-placement="top"
                                    title="ยืนยันการส่งคืนอุปกรณ์">                                                                        
                                    <span class="glyphicon glyphicon-ok-sign"></span>
                                        Return Confirm
                                </asp:LinkButton>
                                <span class="tooltip-box">ยืนยันการคืนอุปกรณ์</span>  
                            </span>
                        </div>
                    </div>  
                
                </div>
            </div>

            <%--( บันทึกข้อมูลชั่วคราวได้ด้วยปุ่ม Save Draft / ส่งรายการร้องขอด้วยปุ่ม Request Borrow)--%>

            <%--=======================END===========================--%>

            <%--<asp:Label runat="server" ID="lb_test" ForeColor="Red" Style="margin-left:10px;"></asp:Label>--%>

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

            <%-- Modal : Return Confirmation --%>
            <div class="modal fade" id="modal_Return" tabindex="-1" role="dialog" 
                data-backdrop="static" data-keyboard="false" style="left:0; width:auto;">
                <div class="modal-dialog" style="max-width:520px; margin:60px auto;">
                    <div class="modal-content">

                        <div class="modal-header" style="background:linear-gradient(135deg,#1a3a5c 0%,#27ae60 100%);">
                            <button type="button" class="close" data-dismiss="modal" 
                                onclick="return false;">&times;</button>
                            <h4 class="modal-title" style="color:#fff;">
                                <span class="glyphicon glyphicon-import"></span>&nbsp;ยืนยันการคืนอุปกรณ์
                            </h4>
                        </div>

                        <div class="modal-body" style="text-align:left; max-height:none;">

                            <%-- ผู้รับคืน (Receiver) = Session emp_id --%>
                            <div class="card" style="margin-bottom:10px;">
                                <div class="card-header" style="font-size:13px;">
                                    <span class="glyphicon glyphicon-user" style="color:#27ae60;"></span>
                                    &nbsp;ผู้รับคืน (Receiver)
                                </div>
                                <div class="card-body" style="padding:10px;">
                                    <div class="form-row">
                                        <div class="form-group" style="width:35%; margin-left:5px;">
                                            <label>EMP ID</label>
                                            <asp:TextBox ID="txtReceiveEmpId" runat="server" ReadOnly="true"
                                                Width="100%" Style="text-align:center; background:#eafaf1;" />
                                        </div>
                                        <div class="form-group" style="width:60%;">
                                            <label>EMP Name</label>
                                            <asp:TextBox ID="txtReceiveEmpName" runat="server" ReadOnly="true"
                                                Width="100%" Style="text-align:center;" />
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <%-- ผู้คืน (Returner) = กรอก emp id 6 หลัก --%>
                            <div class="card" style="margin-bottom:5px;">
                                <div class="card-header" style="font-size:13px;">
                                    <span class="glyphicon glyphicon-log-out" style="color:#2980b9;"></span>
                                    &nbsp;ผู้คืน (Returner) *
                                </div>
                                <div class="card-body" style="padding:10px;">
                                    <div class="form-row">
                                        <div class="form-group" style="width:35%; margin-left:5px;">
                                            <label>EMP ID *</label>
                                            <asp:TextBox ID="txtReturnEmpId" runat="server" MaxLength="6"
                                                Width="100%" placeholder="รหัสพนักงาน 6 หลัก"
                                                Style="text-align:center; background:#00a7ff40;"
                                                AutoComplete="off" AutoPostBack="true"
                                                OnTextChanged="txtReturnEmpId_TextChanged" />
                                        </div>
                                        <div class="form-group" style="width:60%;">
                                            <label>EMP Name</label>
                                            <asp:TextBox ID="txtReturnEmpName" runat="server" ReadOnly="true"
                                                Width="100%" Style="text-align:center;" placeholder="ชื่อพนักงาน" />
                                        </div>
                                    </div>
                                    <div class="form-row" style="margin-top:5px;">
                                        <div class="form-group" style="width:60%; margin-left:5px;">
                                            <label>Department</label>
                                            <asp:TextBox ID="txtReturnDept" runat="server" ReadOnly="true"
                                                Width="100%" Style="text-align:center;" placeholder="แผนก" />
                                        </div>
                                        <div class="form-group" style="width:35%;">
                                            <label>Dept Code</label>
                                            <asp:TextBox ID="txtReturnDeptCode" runat="server" ReadOnly="true"
                                                Width="100%" Style="text-align:center;" placeholder="Dept Code" />
                                        </div>
                                    </div>
                                </div>
                            </div>

                        </div>

                        <div class="modal-footer" style="text-align:right;">
                            <asp:LinkButton ID="btnReturnCancel" runat="server" CssClass="btn btn-secondary"
                                OnClientClick="$('#modal_Return').modal('hide'); return false;"
                                Style="text-shadow:none;">
                                <span class="glyphicon glyphicon-remove"></span>&nbsp;Cancel
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnReturnConfirm" runat="server" CssClass="btn btn-success"
                                OnClick="btnReturnConfirm_Click" Style="text-shadow:none;">
                                <span class="glyphicon glyphicon-ok-sign"></span>&nbsp;Confirm Return
                            </asp:LinkButton>
                        </div>

                    </div>
                </div>
            </div>

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
                                    <span>ค้นหาอุปกรณ์ด้วย Fix Asset, Control No, Tool No ฯลฯ แล้วกด Add เพื่อเพิ่มเข้ารายการยืม</span></div>
                            </div>
                            <div class="rm-item">
                                <div class="rm-ico"><span class="glyphicon glyphicon-list-alt"></span></div>
                                <div class="rm-txt"><strong>Step 3 : Borrow List</strong>
                                    <span>ระบุสถานที่นำไปใช้งาน (Model / Cell / Station) กดปุ่ม + เพื่อสลับเป็นกรอกค่าใหม่เองได้ และ Remove เพื่อลบรายการ</span></div>
                            </div>
                            <div class="rm-item">
                                <div class="rm-ico"><span class="glyphicon glyphicon-comment"></span></div>
                                <div class="rm-txt"><strong>Step 4 : Comment</strong>
                                    <span>เลือกเหตุผลการยืมและกรอกเหตุผลอย่างน้อย 10 ตัวอักษร (บางเหตุผลบังคับกรอก Comment)</span></div>
                            </div>
                            <div class="rm-item">
                                <div class="rm-ico"><span class="glyphicon glyphicon-floppy-disk"></span></div>
                                <div class="rm-txt"><strong>บันทึก / ส่งรายการ</strong>
                                    <span>Save Draft เก็บชั่วคราว, Send Request ส่งให้อนุมัติ ปุ่มที่แสดงจะขึ้นกับสถานะและสิทธิ์ผู้ใช้</span></div>
                            </div>
                            <div class="rm-item">
                                <div class="rm-ico"><span class="glyphicon glyphicon-book"></span></div>
                                <div class="rm-txt"><strong>Manual</strong>
                                    <span>กดปุ่ม "คู่มือ" ที่หัวแต่ละ Step หรือปุ่ม Manual มุมขวาบน เพื่อเปิดคู่มือ (PDF)</span></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <%-- Modal Upload Manual (Admin เท่านั้น) --%>
            <div class="modal fade" id="modal_UploadManual" tabindex="-1" role="dialog">
                <div class="modal-dialog" role="document" >
                    <div class="modal-content" >
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
                                <asp:ListItem Value="Step3" Text="Step 3 : Borrow List (Step3.pdf)" />
                                <asp:ListItem Value="Step4" Text="Step 4 : Comment (Step4.pdf)" />
                                <asp:ListItem Value="SaveDraft" Text="Save Draft (SaveDraft.pdf)" />
                                <asp:ListItem Value="SendRequest" Text="Send Request (SendRequest.pdf)" />
                                <asp:ListItem Value="CancelRequest" Text="Cancel (CancelRequest.pdf)" />
                                <asp:ListItem Value="Confirm" Text="AAD Confirm (Confirm.pdf)" />
                                <asp:ListItem Value="Return" Text="Return (Return.pdf)" />
                                <asp:ListItem Value="Reject" Text="Return (Reject.pdf)" />
                            </asp:DropDownList>
                            <asp:FileUpload ID="fuManual" runat="server" accept="application/pdf"
                                CssClass="form-control" Style="height:auto !important; padding:6px;" />
                            <asp:Label ID="lblUploadMsg" runat="server" Style="display:block; margin-top:8px; font-size:12px;" />
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
           
            <%-- Modal : เหตุผล Reject / Cancel --%>
            <asp:Panel ID="pnlRejectModal" runat="server" CssClass="rjm-overlay" Style="display:none;">
                <div class="rjm-box" role="dialog" aria-modal="true" aria-labelledby="rjmTitle">

                    <div class="rjm-head">
                        <span class="glyphicon glyphicon-alert"></span>
                        <span id="rjmTitle">Reject การร้องขอยืม</span>
                        <button type="button" class="rjm-x" onclick="return closeRejectModal();" aria-label="ปิด">&times;</button>
                    </div>

                    <div class="rjm-body">
                        <div class="rjm-doc">
                            Issue No. : <b><asp:Literal ID="litRjmDoc" runat="server" /></b>
                        </div>
                        <div id="rjmDesc" class="rjm-desc">เอกสารจะถูกส่งกลับเป็น Draft ให้ผู้ร้องขอแก้ไข</div>

                        <label class="rjm-label" for="<%= txt_rejectComment.ClientID %>">
                            เหตุผล <span style="color:var(--danger);">*</span>
                            <span class="rjm-count">
                                (<span id="charCount_Reject">0</span> / 200 ตัวอักษร · อย่างน้อย 10)
                            </span>
                        </label>
                        <asp:TextBox ID="txt_rejectComment" runat="server"
                            TextMode="MultiLine" Rows="4"
                            CssClass="form-control rjm-text"
                            maxlength="200"
                            onkeyup="countChar_Reject(this)" oninput="countChar_Reject(this)"
                            placeholder="กรุณาระบุเหตุผล อย่างน้อย 10 ตัวอักษร" />
                        <div id="rjmErr" class="rjm-err-msg" style="display:none;">กรุณาระบุเหตุผลอย่างน้อย 10 ตัวอักษร</div>

                        <asp:HiddenField ID="hfRejectMode" runat="server" Value="" />
                    </div>

                    <div class="rjm-foot">
                        <button type="button" class="btn btn-secondary" style="text-shadow:none;"
                            onclick="return closeRejectModal();">ปิด</button>
                        <asp:LinkButton ID="btnRejectModalConfirm" runat="server" CssClass="btn btn-danger rjm-ok"
                            Style="text-shadow:none;"
                            OnClientClick="return rejectModalValidate();"
                            OnClick="btnRejectModalConfirm_Click">
                            <span class="glyphicon glyphicon-ok"></span>&nbsp;<span id="rjmOkText">ยืนยัน Reject</span>
                        </asp:LinkButton>
                    </div>

                </div>
            </asp:Panel>

        </ContentTemplate>

        <Triggers>
            <asp:PostBackTrigger ControlID="btnUploadManual" />
        </Triggers>

    </asp:UpdatePanel>  
    
   

</asp:Content>


