<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="NotificationOfChange.aspx.cs"
    Inherits="Tool_Equipment_Control_System.NotificationOfChange" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%--
    ═══════════════════════════════════════════════════════════════════════
      NotificationOfChange.aspx — แจ้งเปลี่ยนแปลง Fixed Asset / Calibration
      UI ชุดเดียวกับ borrow_tool2.aspx (token / card / action bar / modal)
    ───────────────────────────────────────────────────────────────────────
      STEP 1 ผู้ร้องขอ | STEP 2 รูปแบบ | STEP 3 ค้นหา | STEP 4 รายการ
      STEP 5 เหตุผล   | STEP 6 ผู้อนุมัติ | Reject/Cancel History
      Modals : Send (สรุป+Approver) , Approve (checkbox) , Reject/Cancel ,
               ยืนยันเปลี่ยนโหมด , แผนกผู้อนุมัติเปลี่ยน , รูป , HRMS
      ⚠️ ลำดับคอลัมน์ gvSearchResult / gvChangeList ห้ามเปลี่ยน
         (.cs ใช้ Cells[1] = Add , Columns[1] = Remove , Cells[0] = คู่สลับ)
    ═══════════════════════════════════════════════════════════════════════
    --%>

    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <%-- ไฟล์ CSS/JS เดิมทั้งหมด (ลำดับเดิม) --%>
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

    <%--═════════════ Script (logic เดิมทั้งหมด) ═════════════--%>

    <%-- Toast : error/warn กลางจอ + backdrop --%>
    <script>
        (function () {
            function ensureC(center) {
                var id = center ? 'toast-center' : 'toast-container';
                var c = document.getElementById(id);
                if (!c) { c = document.createElement('div'); c.id = id; document.body.appendChild(c); }
                return c;
            }
            function ensureBackdrop() {
                var b = document.getElementById('toast-center-backdrop');
                if (!b) {
                    b = document.createElement('div'); b.id = 'toast-center-backdrop';
                    b.onclick = function () {
                        var cont = document.getElementById('toast-center');
                        if (cont) cont.innerHTML = '';
                        b.style.display = 'none';
                    };
                    document.body.appendChild(b);
                }
                return b;
            }
            function updateBackdrop() {
                var cont = document.getElementById('toast-center');
                var b = ensureBackdrop();
                var has = cont && cont.children.length > 0;
                b.style.display = has ? 'block' : 'none';
            }
            window.showToast = function (msg, opt) {
                opt = opt || {};
                var title = opt.title || '', type = opt.type || 'info',
                    dur = opt.duration != null ? opt.duration : 3000;

                var center = (type === 'error' || type === 'warn');
                var cont = ensureC(center);

                var toast = document.createElement('div'); toast.className = 'toast ' + type;
                var body = document.createElement('div'); body.style.flex = '1';
                if (title) { var t = document.createElement('div'); t.className = 'title'; t.textContent = title; body.appendChild(t); }
                var m = document.createElement('div'); m.className = 'message'; m.textContent = msg; body.appendChild(m);
                var btn = document.createElement('button'); btn.className = 'close'; btn.innerHTML = '&times;';
                btn.onclick = function () {
                    if (toast.parentNode) cont.removeChild(toast);
                    if (center) updateBackdrop();
                };
                toast.appendChild(body); toast.appendChild(btn); cont.appendChild(toast);

                if (center) updateBackdrop();

                if (dur > 0) setTimeout(function () {
                    if (toast.parentNode) cont.removeChild(toast);
                    if (center) updateBackdrop();
                }, dur);
            };
        })();
    </script>

    <%-- Issue Date --%>
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
        $(function () { Set_datePicker(); });
        var raw = $('#txtIssuedate').val();
        if (raw) {
            var m = moment(raw, 'DD MMM YYYY');
            if (m.isValid()) {
                $('#IssueDatePicker').data("DateTimePicker").date(m);
            }
        }
    </script>

    <%-- HRMS (client) --%>
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
                        document.getElementById('<%= txtEmpId.ClientID %>').style.backgroundColor = "#d4edda";
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

    <%-- Confirm --%>
    <script>
        function checkHasItems() {
            var h = document.getElementById('<%= hfHasItems.ClientID %>');
            if (h && h.value === '1') return true;
            showToast('ไม่พบข้อมูลในรายการแจ้งเปลี่ยนแปลง — กรุณาค้นหาและเพิ่มอุปกรณ์ใน Step 3',
                { title: 'ไม่พบข้อมูลในรายการ', type: 'warn', duration: 10000 });
            return false;
        }
        function confirmDraft() {
            if (!checkHasItems()) return false;
            return confirm("⚠️ ยืนยันการบันทึก Draft\nคุณต้องการบันทึกข้อมูลหรือไม่?\nกด OK เพื่อดำเนินการต่อ");
        }
        function Reject() {
            return confirm("⚠️ ยืนยันการ Reject การแจ้งเปลี่ยนแปลง\nคุณต้องการ Reject การแจ้งเปลี่ยนแปลงอุปกรณ์หรือไม่?\nกด OK เพื่อดำเนินการต่อ");
        }
        function confirmRemoveSwap() {
            return confirm("⚠️ ยืนยันการลบรายการ\n1.หากรายการนี้เป็น [แจ้งเปลี่ยน Location] ระบบจะลบเพียงรายการที่เลือก\n2.หากรายการนี้เป็น [แจ้งสลับ Tool/Equipment] ระบบจะลบทั้งคู่\nกด OK เพื่อดำเนินการต่อ");
        }
        function confirmCancel() {
            return confirm("⚠️ ยืนยันการยกเลิกการร้องขอแจ้งเปลี่ยนแปลง\nคุณต้องการยกเลิกรายการนี้หรือไม่?\nกด OK เพื่อดำเนินการต่อ");
        }
    </script>

    <%-- Step 5 : ตัวนับ + ช่องบังคับตามเหตุผล --%>
    <script>
        var REMARK_MAX = 1000;

        function reasonReqIds() {
            var h = document.getElementById('<%= hfReasonReq.ClientID %>');
            return (h && h.value) ? h.value.split(',') : [];
        }
        function isReasonRequired() {
            var d = document.getElementById('<%= ddlReason_Change.ClientID %>');
            return !!d && d.value !== '' && reasonReqIds().indexOf(d.value) !== -1;
        }
        function countChar(el) {
            el = el || document.getElementById('<%= txt_ChangeComment.ClientID %>');
            if (!el) return;

            // ตัวนับ
            var len = el.value.length;
            var c = document.getElementById('charCount');
            if (c) {
                c.innerText = len;
                c.style.color = len >= REMARK_MAX ? '#c93b3b' : (len > REMARK_MAX * 0.9 ? '#c0660e' : '#66727f');
            }

            // ช่องบังคับ : แสดง * จำเป็น + พื้นเหลืองจนกว่าจะครบ 10 ตัวอักษร
            var req = isReasonRequired();
            var mark = document.getElementById('reasonReqMark');
            if (mark) mark.style.display = req ? 'inline' : 'none';
            el.classList.toggle('fld-required', req && el.value.trim().length < 10);
        }

        $(function () { countChar(); });
        if (typeof (Sys) !== "undefined") {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () { countChar(); });
        }
    </script>

    <%-- Select2 --%>
    <script>
        function initSelect2() {
            $('.ddlSearch').each(function () {
                var $s = $(this);
                if ($s.css('display') === 'none') return;            // โหมดพิมพ์ → ไม่สร้าง select2
                if (!$s.hasClass('select2-hidden-accessible')) $s.select2({ width: 'resolve' });
            });
        }
    </script>

    <%-- Toggle ddl ↔ textbox (Model / Cell / Station) --%>
    <script>
        $(document).on("click", ".btnToggle", function () {
            var container = $(this).closest("div");
            var ddl = container.find("select");
            var txt = container.find("input[type='text']");
            var hidden = container.find("input[type='hidden']");
            var btn = $(this);

            if (ddl.is(":visible")) {
                if (ddl.hasClass("select2-hidden-accessible")) ddl.select2("destroy");
                ddl.hide();
                txt.show();
                btn.text("-");
                hidden.val("txt");
            } else {
                ddl.show();
                txt.hide();
                btn.text("+");
                ddl.select2({ width: 'resolve' });
                hidden.val("ddl");
            }
        });

        function restoreModelState() {
            $(".btnToggle").each(function () {
                var container = $(this).closest("div");
                var ddl = container.find("select");
                var txt = container.find("input[type='text']");
                var hidden = container.find("input[type='hidden']");
                var btn = $(this);

                if (hidden.val() === "txt") {
                    if (ddl.hasClass("select2-hidden-accessible")) ddl.select2("destroy");
                    ddl.hide();
                    txt.show();
                    btn.text("-");
                    hidden.val("txt");
                } else {
                    ddl.show();
                    txt.hide();
                    btn.text("+");
                    ddl.select2({ width: 'resolve' });
                    hidden.val("ddl");
                }
            });
        }

        $(document).ready(function () { restoreModelState(); });

        if (typeof (Sys) !== "undefined") {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () { restoreModelState(); });
        }
    </script>

    <%-- Loading Overlay + กันกดซ้ำ --%>
    <script>
        window.__lockUI = false;

        function showLoading(title) {
            var o = document.getElementById('loadingOverlay');
            if (!o) return;
            var t = o.querySelector('.ov-title');
            if (t) t.textContent = title || 'กำลังประมวลผล...';
            o.style.display = 'flex';
        }
        function hideLoading() {
            if (window.__lockUI) return;                 // ✅ กำลังเปลี่ยนหน้า → ห้ามปิด Overlay
            var o = document.getElementById('loadingOverlay');
            if (o) o.style.display = 'none';
        }
        // ✅ เรียกหลังบันทึกสำเร็จ : ล็อกทั้งหน้าจนกว่าจะเปลี่ยนหน้า
        function lockUI(title) {
            window.__lockUI = true;
            showLoading(title || 'บันทึกสำเร็จ กำลังเปลี่ยนหน้า...');
        }

        if (typeof (Sys) !== "undefined") {
            var _prm = Sys.WebForms.PageRequestManager.getInstance();

            // ✅ ยกเลิก Postback ใหม่ ถ้ายังมีรายการก่อนหน้าประมวลผลอยู่ หรือกำลังเปลี่ยนหน้า
            _prm.add_initializeRequest(function (sender, args) {
                if (window.__lockUI || _prm.get_isInAsyncPostBack()) args.set_cancel(true);
            });
            _prm.add_beginRequest(function () { showLoading(); });
            _prm.add_endRequest(function () { hideLoading(); });
        }
    </script>

    <%-- กันข้อความลักษณะ HTML (Request Validation) + ดัก Error ของ Async Postback --%>
    <script>
        (function () {
            var warned = false;

            function cleanInput(el) {
                var v = el.value;
                if (v.indexOf('<') === -1 && v.indexOf('&#') === -1) return;

                var pos = el.selectionStart;
                var nv = v.replace(/</g, '＜').replace(/&#/g, '& #');
                el.value = nv;
                try { el.selectionStart = el.selectionEnd = pos + (nv.length - v.length); } catch (e) { }

                if (!warned && typeof showToast === 'function') {
                    warned = true;
                    showToast('ระบบเปลี่ยนเครื่องหมาย < เป็น ＜ อัตโนมัติ เพื่อป้องกันข้อผิดพลาดของระบบ',
                        { title: 'ปรับข้อความอัตโนมัติ', type: 'info', duration: 5000 });
                }
            }

            document.addEventListener('input', function (e) {
                var t = e.target;
                if (!t) return;
                if (t.tagName === 'TEXTAREA' || (t.tagName === 'INPUT' && t.type === 'text')) cleanInput(t);
            }, true);

            // ดัก Error ที่ Server ตอบกลับระหว่าง Async Postback
            if (typeof (Sys) !== "undefined") {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) {
                    if (!args.get_error()) return;
                    args.set_errorHandled(true);
                    if (typeof hideLoading === 'function') hideLoading();
                    if (typeof showToast === 'function') {
                        showToast('เกิดข้อผิดพลาดที่ Server กรุณาลองใหม่อีกครั้ง หากยังพบปัญหาให้แจ้ง Admin\n(' +
                            args.get_error().message + ')',
                            { title: 'เกิดข้อผิดพลาด', type: 'error', duration: 15000 });
                    }
                });
            }
        })();
    </script>

    <%-- Height Page --%>
    <script>
        function adjustHeight(isFirstLoad) {
            if (isFirstLoad) {
                const params = new URLSearchParams(window.location.search);
                const issue_no = params.get("issue_no");
                if (issue_no === "0") return;
            }
            document.body.style.height = "auto";
            var realHeight = document.documentElement.scrollHeight;
            document.body.style.height = (realHeight + 180) + "px";
        }
        $(document).ready(function () { adjustHeight(true); });
        if (typeof (Sys) !== "undefined") {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () { adjustHeight(false); });
        }
    </script>

    <%-- J3 Approver Picker (2 กล่อง L / R) --%>
    <script>
        var j3State = {
            L: { data: [], selected: [], active: -1 },
            R: { data: [], selected: [], active: -1 }
        };

        var j3Palette = ['#b4412f', '#1f6feb', '#16884a', '#7a3fb0', '#c0660e', '#0f7c73', '#33475b', '#946200'];
        function j3Color(name) { var s = 0; for (var i = 0; i < name.length; i++) s += name.charCodeAt(i); return j3Palette[s % j3Palette.length]; }
        function j3Initials(name) { if (!name) return '?'; var p = name.trim().split(/\s+/); return (p.length === 1 ? p[0].substr(0, 2) : p[0][0] + p[1][0]).toUpperCase(); }

        function j3El(side, base) { return document.getElementById(base + side); }
        function j3Hidden(side) { return document.getElementById(side === 'L' ? '<%= hfJ3SelectedL.ClientID %>' : '<%= hfJ3SelectedR.ClientID %>'); }
        function j3Source(side) { return document.getElementById(side === 'L' ? '<%= hfJ3SourceL.ClientID %>' : '<%= hfJ3SourceR.ClientID %>'); }

        function j3Init() {

            // ✅ แสดงกล่อง Approver 2 ตามค่า A2 (กันถูกซ่อนหลัง async postback)
            var a2 = document.getElementById('<%= hfFlowDeptA2.ClientID %>');
            var boxR = document.getElementById('j3BoxR');
            // แจ้งเปลี่ยนเครื่องมือวัด (gauge) : มี 2 ส่วนเสมอ (QC + J3 Up)
            var cm = document.getElementById('<%= hfChangeMode.ClientID %>');
            var isGauge = cm && cm.value === 'gauge';
            if (boxR) boxR.style.display = (isGauge || (a2 && a2.value)) ? 'block' : 'none';

            ['L', 'R'].forEach(function (side) {
                var srcEl = j3Source(side), selEl = j3Hidden(side);
                var st = j3State[side];
                try { st.data = JSON.parse(srcEl.value || '[]'); } catch (e) { st.data = []; }
                st.selected = (selEl && selEl.value) ? selEl.value.split(',').filter(Boolean) : [];
                j3RenderChips(side);
                var dd = j3El(side, 'j3Dropdown'); if (dd) dd.style.display = 'none';
            });
        }

        function j3FocusInput(side) { var i = j3El(side, 'j3Search'); if (i) i.focus(); }

        function j3SyncHidden(side) {
            var el = j3Hidden(side);
            if (el) el.value = j3State[side].selected.join(',');
        }

        function j3Esc(s) {
            return String(s == null ? '' : s)
                .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;').replace(/'/g, '&#39;');
        }
        function j3Hl(text, q) {
            var t = j3Esc(text);
            if (!q) return t;
            var i = String(text).toLowerCase().indexOf(q);
            if (i < 0) return t;
            return j3Esc(text.substr(0, i)) + '<mark>' + j3Esc(text.substr(i, q.length)) + '</mark>' + j3Esc(text.substr(i + q.length));
        }

        function j3RenderChips(side) {
            var box = j3El(side, 'j3Chips'); if (!box) return;
            var st = j3State[side];
            box.innerHTML = '';

            st.selected.forEach(function (id) {
                var d = st.data.find(function (x) { return x.id === id; });
                if (!d) return;
                var sub = [j3Pos(d), d.email].filter(Boolean).join(' | ');
                var row = document.createElement('div');
                row.className = 'j3-sel';
                row.innerHTML =
                    '<span class="j3-avatar" style="background:' + j3Color(d.name) + '">' + j3Esc(j3Initials(d.name)) + '</span>' +
                    '<div class="j3-sel-meta">' +
                    '<span class="j3-sel-name"><span class="j3-id">' + j3Esc(d.id) + '</span>' + j3Esc(d.name) + '</span>' +
                    (sub ? '<span class="j3-sel-sub" title="' + j3Esc(sub) + '">' + j3Esc(sub) + '</span>' : '') +
                    '</div>' +
                    '<button type="button" class="j3-sel-x" title="นำออก" onclick="j3Remove(event,\'' + side + '\',\'' + j3Esc(id) + '\')">&times;</button>';
                box.appendChild(row);
            });

            if (st.selected.length === 0) {
                box.innerHTML = '<div class="j3-sel-empty">ยังไม่ได้เลือกผู้อนุมัติ — พิมพ์ค้นหาด้านบน</div>';
            }

            // ตัวนับ + สถานะการ์ด
            var cnt = document.getElementById('j3Count' + side);
            var n = st.selected.length;
            if (cnt) {
                cnt.className = 'j3-count ' + (n > 0 ? 'ok' : 'req');
                cnt.textContent = n > 0 ? ('✓ เลือกแล้ว ' + n + ' คน') : 'ต้องเลือกอย่างน้อย 1 คน';
            }
            var card = j3El(side, 'j3Box');
            if (card) card.classList.toggle('is-done', n > 0);

            j3SyncHidden(side);
        }

        function j3ShortName(name) {
            if (!name) return '';
            var p = name.trim().split(/\s+/);
            if (p.length === 1) return p[0];
            return p[0] + ' ' + p[1].charAt(0) + '.';
        }

        function j3Remove(ev, side, id) {
            ev.preventDefault(); ev.stopPropagation();
            var st = j3State[side];
            st.selected = st.selected.filter(function (x) { return x !== id; });
            j3RenderChips(side);
            var dd = j3El(side, 'j3Dropdown');
            if (dd && dd.style.display !== 'none') j3Filter(side);   // refresh เฉพาะตอน dropdown เปิดอยู่
        }

        function j3Add(side, id) {
            var st = j3State[side];
            if (st.selected.indexOf(id) === -1) st.selected.push(id);
            j3RenderChips(side);
            var i = j3El(side, 'j3Search'); if (i) i.value = '';
            var dd = j3El(side, 'j3Dropdown');
            if (dd) dd.style.display = 'none';
        }

        function j3Pos(d) {
            var code = (d.posCode || '').trim();
            var name = (d.position || '').trim();
            if (code && name) return code + ' ' + name;
            return code || name;
        }

        function j3Filter(side) {
            var st = j3State[side];
            var q = (j3El(side, 'j3Search').value || '').toLowerCase().trim();
            var dd = j3El(side, 'j3Dropdown'); if (!dd) return;
            st.active = -1;

            if (st.data.length === 0) {
                dd.innerHTML = '<div class="j3-empty">ไม่มีรายชื่อผู้อนุมัติของแผนกนี้ — กรุณาติดต่อ Admin</div>';
                dd.style.display = 'block'; return;
            }

            var list = st.data.filter(function (d) {
                if (st.selected.indexOf(d.id) !== -1) return false;
                if (!q) return true;
                return (d.name + ' ' + d.id + ' ' + d.email + ' ' + (d.position || '') + ' ' + (d.posCode || '')).toLowerCase().indexOf(q) !== -1;
            });

            if (list.length === 0) {
                dd.innerHTML = '<div class="j3-empty">' +
                    (st.selected.length === st.data.length ? 'เลือกครบทุกคนแล้ว' : 'ไม่พบรายชื่อที่ตรงกับ "' + j3Esc(q) + '"') +
                    '</div>';
                dd.style.display = 'block'; return;
            }

            dd.innerHTML = '';
            list.forEach(function (d) {
                var sub = [j3Pos(d), d.dept, d.email].filter(Boolean).join(' · ');
                var it = document.createElement('div');
                it.className = 'j3-item';
                it.setAttribute('data-id', d.id);
                it.onclick = function (e) { e.stopPropagation(); j3Add(side, d.id); };
                it.innerHTML =
                    '<span class="j3-avatar" style="background:' + j3Color(d.name) + '">' + j3Esc(j3Initials(d.name)) + '</span>' +
                    '<span class="j3-meta">' +
                    '<span class="j3-name"><span class="j3-id">' + j3Hl(d.id, q) + '</span>' + j3Hl(d.name, q) + '</span>' +
                    '<span class="j3-sub2">' + j3Hl(sub, q) + '</span>' +
                    '</span>' +
                    '<span class="j3-plus">+</span>';
                dd.appendChild(it);
            });
            dd.style.display = 'block';
        }

        function j3KeyDown(e, side) {
            var dd = j3El(side, 'j3Dropdown'); if (!dd || dd.style.display === 'none') return;
            var st = j3State[side];
            var items = dd.querySelectorAll('.j3-item');
            if (e.key === 'ArrowDown') { e.preventDefault(); st.active = Math.min(st.active + 1, items.length - 1); j3Hi(items, st); }
            else if (e.key === 'ArrowUp') { e.preventDefault(); st.active = Math.max(st.active - 1, 0); j3Hi(items, st); }
            else if (e.key === 'Enter') { e.preventDefault(); if (st.active >= 0 && items[st.active]) j3Add(side, items[st.active].getAttribute('data-id')); }
            else if (e.key === 'Backspace') { var inp = j3El(side, 'j3Search'); if (inp.value === '' && st.selected.length > 0) { st.selected.pop(); j3RenderChips(side); j3Filter(side); } }
        }
        function j3Hi(items, st) { items.forEach(function (x, i) { x.classList.toggle('active', i === st.active); }); }

        document.addEventListener('click', function (e) {
            ['L', 'R'].forEach(function (side) {
                var p = j3El(side, 'j3Picker'), dd = j3El(side, 'j3Dropdown');
                if (p && dd && !p.contains(e.target)) dd.style.display = 'none';
            });
        });

        if (typeof (Sys) !== "undefined") {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                if (document.getElementById('j3PickerL')) j3Init();
            });
        }
    </script>

    <%-- Approve Modal --%>
    <script>

        window.apvStep = window.apvStep || 1;
        window.apvSwap = window.apvSwap || false;

        function apvBoxes() {
            return document.querySelectorAll('#<%= gvApprove.ClientID %> .apv-cb input[type=checkbox]');
        }

        function closeApproveModal() {
            var m = document.getElementById('<%= pnlApproveModal.ClientID %>');
            if (m) m.style.display = 'none';
        }

        function apvToggleAll(src) {
            apvBoxes().forEach(function (c) { c.checked = src.checked; });
            apvUpdateCommentReq();
        }

        // ✅ โหมดสลับ : ติ๊ก/เอาออก ทั้งคู่พร้อมกัน
        function apvOnCheck(e) {
            var c = e.target, p = c.getAttribute('data-pair');
            if (window.apvSwap && p) {
                apvBoxes().forEach(function (x) {
                    if (x !== c && x.getAttribute('data-pair') === p) x.checked = c.checked;
                });
            }
            apvUpdateCommentReq();
        }

        // อัปเดตสี/ป้ายรายแถว + คืนสรุปสถานะ
        function apvState() {
            var s = { total: 0, checked: 0, mismatch: false };
            apvBoxes().forEach(function (c) {
                s.total++; if (c.checked) s.checked++;
                var tr = c.closest('tr'); if (!tr) return;
                tr.classList.toggle('apv-row-off', !c.checked);
                var b = tr.querySelector('.apv-row-badge');
                if (b) {
                    b.className = 'res-badge apv-row-badge ' + (c.checked ? 'res-ok' : 'res-ng');
                    b.textContent = c.checked ? '✓ อนุมัติ' : '✗ ไม่อนุมัติ';
                }
                var diff = window.apvStep === 2 && ((c.getAttribute('data-a1') === 'Y') !== c.checked);
                tr.classList.toggle('apv-mismatch', diff);
                if (diff) s.mismatch = true;
            });
            return s;
        }

        function apvUpdateCommentReq() {
            var s = apvState();

            // ✅ 7E-2 : ต้องใส่เหตุผล = A1 ไม่อนุมัติบางรายการ หรือ A2 เลือกไม่ตรงกับ A1
            var needCmt = !window.apvAllItems && (s.mismatch || (window.apvStep !== 2 && s.checked < s.total));
            var req = document.getElementById('apvCommentReq');
            if (req) req.style.display = needCmt ? 'inline' : 'none';

            var nt = document.getElementById('apvMismatchNotice');
            if (nt) nt.style.display = s.mismatch ? 'block' : 'none';

            var btn = document.getElementById('<%= btnApvConfirm.ClientID %>');
            if (btn) {
                btn.className = 'ab-btn ' + (s.mismatch ? 'ab-reject' : 'ab-send');
                btn.textContent = s.mismatch ? 'Reject กลับไปยังผู้ร้องขอ' : (window.apvConfirmText || 'ยืนยันอนุมัติ');
            }
            return s;
        }

        function apvValidate() {
            // แจ้งเปลี่ยนเครื่องมือวัด : ยืนยันทุกรายการ (ไม่มี checkbox / เหตุผล)
            if (window.apvAllItems) { showLoading(); return true; }
            var s = apvUpdateCommentReq();
            var cmt = document.getElementById('<%= txtApvComment.ClientID %>').value.trim();

            if (s.mismatch) {
                if (cmt.length < 10) {
                    showToast('กรุณาระบุเหตุผลการ Reject อย่างน้อย 10 ตัวอักษร', { title: 'ต้องระบุเหตุผล', type: 'warn', duration: 10000 });
                    return false;
                }
                if (!confirm('⚠️ รายการที่เลือกไม่ตรงกับ ' + (window.apvRefLabel || 'Approver 1') + '\nเอกสารจะถูก Reject กลับไปยังผู้ร้องขอ (Draft)\nกด OK เพื่อดำเนินการต่อ')) return false;
                showLoading(); return true;
            }
            if (s.checked === 0) {
                showToast('กรุณาเลือกอย่างน้อย 1 รายการ', { title: 'ยังไม่เลือกรายการ', type: 'warn', duration: 10000 });
                return false;
            }
            if (window.apvStep !== 2 && s.checked < s.total && cmt === '') {     // ✅ 7E-2
                showToast('กรุณาระบุเหตุผลกรณีไม่อนุมัติบางรายการ', { title: 'ต้องระบุเหตุผล', type: 'warn', duration: 10000 });
                return false;
            }
            showLoading(); return true;
        }

        function apvBindEvents() {
            apvBoxes().forEach(function (c) { c.onclick = apvOnCheck; });
            apvUpdateCommentReq();
        }

        if (typeof (Sys) !== "undefined") {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                if (document.getElementById('<%= gvApprove.ClientID %>')) apvBindEvents();
        });
        }

        </script>

    <%-- Manual (PDF ตาม Step) --%>
    <script>

        var MANUAL_BASE = '/Tool_Equipment_Control_System/ManualPath/08_Tool%26Equipment/02_Change/';
        function openManualStep(step) {
            window.open(MANUAL_BASE + 'Step' + step + '.pdf', '_blank');
            return false;
        }

        // คู่มือตามชื่อไฟล์ เช่น openManualFile('Approver') , openManualFile('AAD_Confirm')
        function openManualFile(name) {
            window.open(MANUAL_BASE + encodeURIComponent(name) + '.pdf', '_blank');
            return false;
        }

        // คู่มือของขั้นปัจจุบัน (Approver1 / Approver2 / Confirm) — ค่ามาจาก Server ตาม status
        function openApvManual() {
            var h = document.getElementById('<%= hfManualFile.ClientID %>');
            return openManualFile((h && h.value) ? h.value : 'Approver1');
        }
    </script>

    <%-- Modal / Mode switch / Reject --%>
    <script>
        function scrollBackToRow(rowIndex) {
            setTimeout(function () {
                var row = document.getElementById('rowSearch_' + rowIndex);
                if (row) row.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }, 200);
        }

        function closeSendModal() {
            var m = document.getElementById('<%= pnlSendModal.ClientID %>');
            if (m) m.style.display = 'none';
        }

        function openModeConfirm() {
            var m = document.getElementById('modeConfirmModal');
            if (m) m.style.display = 'flex';
        }
        function closeModeConfirm() {
            var m = document.getElementById('modeConfirmModal');
            if (m) m.style.display = 'none';
        }
        // ยกเลิกจาก dialog → ไม่ค้างเป้าหมายเครื่องมือวัด
        function cancelModeConfirm() { _modeTarget = null; closeModeConfirm(); }
        var _modeTarget = null;   // 'measure' = ยืนยันจากปุ่มเครื่องมือวัด / null = สวิตช์ปกติ
        function confirmToggle() {
            closeModeConfirm();
            if (_modeTarget === 'measure') { _modeTarget = null; doMeasurePostBack(); return; }
            var track = document.getElementById('<%= msTrack.ClientID %>');
            if (track) doToggleAnim(track);
        }

        /* Admin : เลือก "แจ้งเปลี่ยนเครื่องมือวัด" */
        function selectMeasureMode() {
            var track = document.getElementById('<%= msTrack.ClientID %>');
            if (track && (track.getAttribute('data-lock') === '1' || track.getAttribute('data-busy') === '1')) return;
            if (track && track.classList.contains('mode-measure')) return;   // อยู่โหมดนี้แล้ว

            var hasItems = document.getElementById('<%= hfHasItems.ClientID %>').value === '1';
            if (hasItems) { _modeTarget = 'measure'; openModeConfirm(); return; }
            doMeasurePostBack();
        }
        function doMeasurePostBack() {
            var track = document.getElementById('<%= msTrack.ClientID %>');
            if (track) track.setAttribute('data-busy', '1');
            showLoading();
            __doPostBack('<%= btnModeMeasure.UniqueID %>', '');
        }

        function openApvChangeConfirm() {
            var m = document.getElementById('apvChangeConfirm');
            if (m) m.style.display = 'flex';
        }
        function closeApvChangeConfirm() {
            var m = document.getElementById('apvChangeConfirm');
            if (m) m.style.display = 'none';
        }

        function openRejectModal(mode, title) {
            document.getElementById('<%= hfRejectMode.ClientID %>').value = mode;
            var t = document.getElementById('<%= litRejectTitle.ClientID %>');
            if (t && title) t.innerText = title;
            var m = document.getElementById('<%= pnlRejectModal.ClientID %>');
            if (m) m.style.display = 'flex';
        }
        function closeRejectModal() {
            var m = document.getElementById('<%= pnlRejectModal.ClientID %>');
            if (m) m.style.display = 'none';
        }
        function countRejectChar(el) {
            var len = el.value.length;
            var c = document.getElementById('rejectCharCount');
            if (c) {
                c.innerText = len;
                c.style.color = len < 10 ? '#c93b3b' : '#16884a';
            }
        }
        function validateRejectComment() {
            var el = document.getElementById('<%= txt_rejectComment.ClientID %>');
            var val = el ? el.value.trim() : '';
            if (val.length < 10) {
                if (typeof showToast === 'function')
                    showToast('กรุณากรอก Comment อย่างน้อย 10 ตัวอักษร', { title: 'ข้อมูลไม่ครบ', type: 'warn', duration: 8000 });
                if (el) el.focus();
                return false;
            }
            showLoading();
            return true;
        }

        function toggleMode() {
            var track = document.getElementById('<%= msTrack.ClientID %>');
            if (!track || track.getAttribute('data-lock') === '1') return;
            if (track.getAttribute('data-busy') === '1') return;

            var hasItems = document.getElementById('<%= hfHasItems.ClientID %>').value === '1';
            if (hasItems) { openModeConfirm(); return; }
            doToggleAnim(track);
        }

        function doToggleAnim(track) {
            track.setAttribute('data-busy', '1');
            var isChange = track.classList.contains('mode-change');
            track.classList.remove('mode-change', 'mode-swap');
            track.classList.add(isChange ? 'mode-swap' : 'mode-change');
            track.classList.add('ms-clicked');

            var lblL = document.getElementById('<%= msLabelL.ClientID %>');
            var lblR = document.getElementById('<%= msLabelR.ClientID %>');
            if (lblL && lblR) {
                lblL.classList.toggle('active', !isChange);
                lblR.classList.toggle('active', isChange);
            }
            setTimeout(function () {
                showLoading();
                __doPostBack('<%= btnModeToggle.UniqueID %>', '');
            }, 260);
        }

        function closeConfirmModal() {
            var m = document.getElementById('<%= pnlConfirmModal.ClientID %>');
            if (m) m.style.display = 'none';
        }

        // ✅ ข้อ 3 : เตือนให้ตรวจ Status Use / ผู้รับผิดชอบ ก่อนบันทึก
        function cfmConfirm() {
            var grid = document.getElementById('<%= gvConfirmSummary.ClientID %>');
            var empty = grid ? grid.querySelectorAll('select.aad-ddl:not([disabled])') : [];
            var missing = 0;
            for (var i = 0; i < empty.length; i++) if (!empty[i].value) missing++;

            if (missing > 0) {
                showToast('ยังมีช่อง Status Use / ผู้รับผิดชอบ ที่ไม่มีข้อมูล ' + missing + ' ช่อง (ขอบแดง)',
                    { title: 'ข้อมูลไม่ครบ', type: 'warn', duration: 8000 });
                return false;
            }
            if (!confirm('⚠️ ยืนยัน AAD Confirm\n\nตรวจสอบ Status Use และ ผู้รับผิดชอบ ครบทุกรายการแล้วใช่หรือไม่?\n' +
                'เมื่อยืนยันแล้วจะแก้ไขไม่ได้\n\nกด OK เพื่อดำเนินการต่อ')) return false;

            showLoading('กำลังยืนยัน AAD Confirm...');
            return true;
        }

        // เปลี่ยนค่าแล้วเอาขอบแดงออกทันที
        document.addEventListener('change', function (e) {
            var s = e.target;
            if (s && s.classList && s.classList.contains('aad-ddl'))
                s.classList.toggle('aad-ddl-empty', !s.value);
        }, true);
    </script>

    <%-- ============================================================
         DESIGN TOKENS (ชุดเดียวกับ borrow_tool2 / ApproveStatus)
         ink #1c2b3a | paper #f3f5f8 | line #dde3ea | muted #66727f
         ปุ่ม : Send / Approve เขียว | AAD Confirm น้ำเงิน | Reject แดง | Draft ขาวมีขอบ
         ============================================================ --%>
    <style>
        body { font-family: 'IBM Plex Sans Thai', 'Segoe UI', Tahoma, sans-serif; }

        .bw {
            --ink: #1c2b3a; --ink-2: #33475b; --muted: #66727f;
            --paper: #f3f5f8; --surface: #ffffff;
            --line: #dde3ea; --line-soft: #edf0f4;
            --focus: #1f6feb;
            --edit-bg: #eef5ff; --edit-line: #a9c6ee;
            --send: #16884a; --send-h: #11703c;
            --confirm: #1f6feb; --danger: #c93b3b;
            --r-sm: 6px; --r-md: 10px;

            font-family: 'IBM Plex Sans Thai', 'Segoe UI', Tahoma, sans-serif;
            color: var(--ink);
            padding: 10px 10px 96px;
        }
        .bw *, .bw *::before, .bw *::after { box-sizing: border-box; }

        /* ---------- Page header ---------- */
        .bw-header {
            display: flex; align-items: center; flex-wrap: wrap; gap: 12px 16px;
            padding: 12px 20px; margin-bottom: 5px;
            background: #fff; border-left: 10px solid #96c7a7; border-radius: var(--r-md);
        }
        .bw-header img { height: 40px; }
        .bw-header h1 { margin: 0; font-size: 22px; font-weight: 700; line-height: 1.2; color: var(--ink); }
        .bw-header p  { margin: 2px 0 0; font-size: 13px; color: var(--muted); }
        .bw-meta { margin-left: auto; display: flex; align-items: center; flex-wrap: wrap; gap: 10px 18px; }
        .meta-item { display: flex; flex-direction: column; gap: 2px; }
        .meta-label { font-size: 11.5px; font-weight: 600; color: var(--muted); }
        .meta-doc { font-size: 16px; font-weight: 700; color: var(--confirm); font-variant-numeric: tabular-nums; }

        /* Mode badge : ทรงเดียวกับ status-badge */
        .bw .mode-badge {
            display: inline-flex; align-items: center; gap: 6px;
            padding: 3px 12px 3px 9px; border-radius: 999px;
            font-size: 13px; font-weight: 700; white-space: nowrap;
            border: 1px solid transparent;
        }
        .bw .mode-badge::before { content: ''; width: 7px; height: 7px; border-radius: 50%; background: currentColor; }
        .bw .mode-issue   { background: #e3f4e8; color: #16703d; border-color: #b6e0c4; }  /* สร้าง/แก้ไข */
        .bw .mode-view    { background: #f0f1f3; color: #4b5663; border-color: #d9dde2; }  /* ดูอย่างเดียว */
        .bw .mode-approve { background: #e8f0fe; color: #1a5bc4; border-color: #bcd1f7; }  /* อนุมัติ */
        /* AAD Confirm : ที่มาของค่า Auto Assign (เหมือนหน้า Withdraw) */
        .rule-note { margin-top:4px; font-size:11.5px; color:#66727f; white-space:normal; max-width:260px; }
        .rule-note.hit { color:#7a4f00; background:#fff4e0; border:1px solid #f5c26b; border-radius:4px; padding:2px 6px; }
        .bw .mode-qc { background: #f4effc; color: #6f42c1; border-color: #d9c8f3; }  /* QC Check (แจ้งเปลี่ยนเครื่องมือวัด) */
        .bw .mode-confirm { background: #f5e9f8; color: #92278f; border-color: #dfc0e6; }  /* AAD Confirm */
        .readme-btn { font-size: 22px; color: var(--muted); cursor: pointer; }
        .readme-btn:hover { color: var(--confirm); }

        .bw .status-badge {
            display: inline-flex; align-items: center; gap: 6px;
            padding: 3px 12px 3px 9px; border-radius: 999px;
            font-size: 13px; font-weight: 700; white-space: nowrap;
            background: #eef1f4; color: #4b5663; border: 1px solid #d8dee5;
        }
        .bw .status-badge::before { content: ''; width: 7px; height: 7px; border-radius: 50%; background: currentColor; }
        .bw .status-draft     { background: #fff7e0; color: #946200; border-color: #f1d98c; }
        .bw .status-approve   { background: #e8f0fe; color: #1a5bc4; border-color: #bcd1f7; }
        .bw .status-confirm   { background: #f5e9f8; color: #92278f; border-color: #dfc0e6; }
        .bw .status-borrow    { background: #ecebfb; color: #4a3fb0; border-color: #cdc8f1; }
        .bw .status-completed { background: #e3f4e8; color: #16703d; border-color: #b6e0c4; }
        .bw .status-cancel    { background: #f0f1f3; color: #6b7480; border-color: #d9dde2; }

        /* ---------- Card (Step) ---------- */
        .bw-card {
            background: var(--surface);
            border: 1px solid var(--line);
            border-radius: var(--r-md);
            margin-bottom: 5px;
        }
        .bw-card-head {
            display: flex; align-items: center; gap: 12px;
            padding: 6px; margin: 0px 12px;
            border-bottom: 1px solid var(--muted); 
        }
        .step-no {
            flex: 0 0 28px; width: 28px; height: 28px;
            display: inline-flex; align-items: center; justify-content: center;
            border-radius: 50%; background: var(--ink); color: #fff;
            font-size: 13px; font-weight: 700;
        }
        .bw-card-title h2 { margin: -8px 0px; font-size: 18px; font-weight: 700; color: var(--ink); display: flex; align-items: center; gap: 8px; }
        .bw-card-title p  { margin: 1px 0 0; font-size: 12.5px; color: var(--muted); }
        .bw-card-head .head-right { margin-left: auto; }
        .bw-card-body { padding: 14px 18px; }

        /* ปุ่มเปิดคู่มือ PDF ตาม Step */
        .manual-btn {
            display: inline-flex; align-items: center; justify-content: center;
            width: 26px; height: 24px;
            background: #fff; border: 1px solid var(--line); border-radius: 6px;
            color: var(--muted) !important; text-decoration: none !important; font-size: 12px;
        }
        .manual-btn:hover { border-color: var(--confirm); color: var(--confirm) !important; }

        /* ---------- Fields ---------- */
        .bw-fields { display: flex; flex-wrap: wrap; gap: 12px 14px; align-items: flex-end; }
        .bw-field { display: flex; flex-direction: column; gap: 5px; }
        .bw-field > label, .bw-label { margin: 0; font-size: 12px; font-weight: 600; color: var(--muted); }
        .bw .fld {
            height: 34px; padding: 0 10px; margin: 0;
            font-family: inherit; font-size: 14px; color: var(--ink); text-align: center;
            background: var(--surface);
            border: 1px solid #c9d2dc; border-radius: var(--r-sm); box-shadow: none;
        }
        .bw .fld:focus, .bw textarea.fld-area:focus {
            outline: none; border-color: var(--focus); box-shadow: 0 0 0 3px rgba(31,111,235,.18);
        }
        .bw .fld[readonly] { background: var(--line); border-color: transparent; color: var(--ink-2); }
        .bw .fld-edit { background: var(--edit-bg); border-color: var(--edit-line); font-weight: 600; }
        .bw textarea.fld-area {
            width: 100%; min-height: 64px; padding: 8px 10px;
            font-family: inherit; font-size: 14px; color: var(--ink);
            border: 1px solid #c9d2dc; border-radius: var(--r-sm); resize: vertical;
        }
        .char-count { font-size: 12px; color: var(--muted); font-weight: 400; }
        .char-count b { font-variant-numeric: tabular-nums; }

        #IssueDatePicker { display: flex; }
        #IssueDatePicker .fld { border-top-right-radius: 0; border-bottom-right-radius: 0; }
        #IssueDatePicker .input-group-addon {
            display: inline-flex; align-items: center; justify-content: center;
            width: 36px; height: 34px; padding: 0;
            background: var(--paper); border: 1px solid #c9d2dc; border-left: none;
            border-radius: 0 var(--r-sm) var(--r-sm) 0; color: var(--muted); cursor: pointer;
        }

        .bw-notice {
            margin-top: 10px; padding: 8px 12px;
            border-left: 3px solid #d9822b; border-radius: var(--r-sm);
            background: #fdf3e7; color: #8a4b0c; font-size: 13px;
        }
        .bw-notice.danger { border-left-color: var(--danger); background: #fbeceb; color: #8f2b22; }
        .bw-hint {
            display: inline-block; padding: 4px 10px; border-radius: var(--r-sm);
            background: #f4f7fb; border: 1px solid var(--line);
            font-size: 12px; color: var(--ink-2);
        }
        .draft-banner {
            margin-bottom: 12px; padding: 8px 12px;
            border: 1px dashed #e2b957; border-radius: var(--r-sm);
            background: #fff8e6; color: #7a5b16; font-size: 13px;
        }
        .draft-banner strong { color: #946200; }

        /* ---------- Step 2 : Mode switch ---------- */
        .mode-switch { display: inline-flex; align-items: center; gap: 16px; user-select: none; }
        .ms-label { display: inline-flex; flex-direction: column; align-items: center; min-width: 150px; line-height: 1.2; padding: 4px 12px; }
        .ms-label .ms-title { font-size: 14px; font-weight: 700; padding: 4px 10px; transition: color .2s; }
        .ms-label .ms-sub   { font-size: 12px; font-weight: 500; transition: color .2s; }
        .ms-label:not(.active) .ms-title, .ms-label:not(.active) .ms-sub { color: #a7b0ba; }
        .ms-label.active .ms-title { color: var(--ink); }
        .ms-label.active .ms-sub   { color: var(--muted); }

        .ms-track {
            position: relative; width: 100px; height: 38px; flex-shrink: 0;
            background: #16884a; border-radius: 24px;
            display: flex; align-items: center; cursor: pointer;
            box-shadow: inset 0 1px 4px rgba(0,0,0,.2);
        }
        .ms-icon { position: absolute; width: 40px; text-align: center; color: rgba(255,255,255,.85); font-size: 17px; z-index: 1; }
        .ms-icon-left  { left: 6px; top: 11px; }
        .ms-icon-right { right: 6px; top: 11px; }
        .ms-knob {
            position: absolute; top: 4px; left: 4px; z-index: 2;
            width: 42px; height: 30px; border-radius: 20px;
            background: #fff; color: var(--ink); font-size: 17px;
            display: flex; align-items: center; justify-content: center;
            box-shadow: 0 2px 6px rgba(0,0,0,.3);
            transition: left .25s cubic-bezier(.4,0,.2,1);
        }
        .ms-knob-icon-left, .ms-knob-icon-right { display: none; }
        .ms-track.mode-change .ms-knob { left: 5px; }
        .ms-track.mode-change .ms-knob-icon-left { display: inline; }
        .ms-track.mode-change .ms-icon-left { visibility: hidden; }
        .ms-track.mode-swap .ms-knob { left: 52px; }
        .ms-track.mode-swap .ms-knob-icon-right { display: inline; }
        .ms-track.mode-swap .ms-icon-right { visibility: hidden; }
        .ms-track.ms-clicked { animation: trackPulse .35s ease; }

        /* โหมดแจ้งเปลี่ยนเครื่องมือวัด (Admin) : สวิตช์เป็นกลาง + ปุ่มเครื่องมือวัด active */
        .ms-track.mode-measure { background: #9aa5b1; }
        .ms-track.mode-measure .ms-knob { left: 29px; }
        .ms-track.mode-measure .ms-knob-icon-left, .ms-track.mode-measure .ms-knob-icon-right { display: none; }
        .ms-measure {
            display: inline-flex; flex-direction: column; align-items: center; min-width: 170px; line-height: 1.2;
            padding: 6px 14px; margin-left: 8px; border: 2px dashed #c9d2dc; border-radius: 12px;
            cursor: pointer; transition: border-color .2s, background .2s;
        }
        .ms-measure .ms-title { font-size: 14px; font-weight: 700; padding: 2px 6px; color: #a7b0ba; }
        .ms-measure .ms-sub   { font-size: 12px; font-weight: 500; color: #a7b0ba; }
        .ms-measure:hover { border-color: #6f42c1; }
        .ms-measure.active { border: 2px solid #6f42c1; background: #f4effc; }
        .ms-measure.active .ms-title { color: #6f42c1; }
        .ms-measure.active .ms-sub   { color: var(--muted); }

        /* เตือนชื่อ Tool No / Model ไม่ตรงคู่ (โหมดเครื่องมือวัด) */
        .toolname-warn {
            margin-top: 4px; padding: 2px 6px; border-radius: 6px; display: inline-block;
            background: #fff3cd; color: #8a5a00; border: 1px solid #f0c36d;
            font-size: 11px; font-weight: 600; white-space: normal;
        }
        @keyframes trackPulse {
            0%   { box-shadow: inset 0 1px 4px rgba(0,0,0,.2), 0 0 0 0 rgba(22,136,74,.5); }
            50%  { box-shadow: inset 0 1px 4px rgba(0,0,0,.2), 0 0 0 6px rgba(22,136,74,.2); }
            100% { box-shadow: inset 0 1px 4px rgba(0,0,0,.2), 0 0 0 0 rgba(22,136,74,0); }
        }

        /* ---------- Search (Step 3) ---------- */
        .search-bar { display: flex; flex-wrap: wrap; align-items: flex-end; gap: 12px 14px; margin-bottom: 10px; }
        .search-hint { display: flex; flex-wrap: wrap; align-items: center; gap: 6px; font-size: 12px; color: var(--muted); }
        .search-hint code {
            padding: 2px 8px; border-radius: 4px;
            background: var(--paper); border: 1px solid var(--line);
            font-family: inherit; font-size: 12px; color: var(--ink-2);
        }
        .search-hint code i { font-style: normal; color: var(--muted); }

        .bw .tb-btn {
            display: inline-flex; align-items: center; justify-content: center; gap: 6px;
            height: 34px !important; padding: 0 16px;
            font-family: inherit; font-size: 14px; font-weight: 600;
            border-radius: var(--r-sm); border: 1px solid transparent;
            cursor: pointer; text-decoration: none; text-shadow: none;
        }
        .bw .tb-primary { background: var(--ink); color: #fff; }
        .bw .tb-primary:hover { background: #0f1b27; color: #fff; }

        /* ---------- Tables ---------- */
        .bw .tbl-wrap { overflow: auto; position: relative; border: 1px solid var(--line); border-radius: 8px; }
        .bw table.data-table { width: 100%; border-collapse: separate; border-spacing: 0; font-size: 12.5px; }
        .bw .data-table th {
            position: sticky; top: 0; z-index: 3;
            background: #1a3a5c; color: #fff;
            font-weight: 700; font-size: 12px; text-align: center;
            padding: 9px 10px; white-space: nowrap; border: none;
        }
        .bw .data-table td {
            padding: 6px 8px; vertical-align: middle; text-align: center;
            background: #fff; border: none; border-bottom: 1px solid var(--line-soft);
        }
        .bw .data-table tr:nth-child(odd) td { background: #f4f7fb; }
        .bw .data-table tr:hover td { background: #e3edf9; }
        .bw .data-table td.rowGv2 { text-align: left; }
        .bw .data-table td.rowGv_toolNo { min-width: 140px; font-weight: 600; }
        .bw .no-data { text-align: center; color: var(--muted); padding: 28px; font-style: normal; }

        .bw .data-table th.hdr-use     { background: #16884a; }
        .bw .data-table th.hdr-current { background: #4a5b6d; }
        .bw .data-table tr td.col-use     { background: #eef8f1; }
        .bw .data-table tr td.col-current { background: #f3f5f8; color: var(--muted); }
        .bw .data-table .zone-divider { border-left: 3px solid #1a3a5c; }
        .bw .data-table th.headerGvOld { background: #fdf1e3; color: #7a4a0e; }

        .bw .data-table .btn {
            display: inline-flex; align-items: center; justify-content: center;
            height: 26px !important; min-width: 64px; padding: 0 10px;
            font-size: 12px; font-weight: 700; border-radius: 5px; border: 1px solid transparent;
            text-shadow: none; box-shadow: none;
        }
        .bw .data-table .btn-success   { background: var(--send); color: #fff; }
        .bw .data-table .btn-success:hover { background: var(--send-h); }
        .bw .data-table .btn-danger    { background: #fff; color: var(--danger); border-color: #efb9b9; }
        .bw .data-table .btn-danger:hover  { background: #fbeceb; }
        .bw .data-table .btn-secondary { background: #e4e8ec; color: #8a95a1; cursor: not-allowed; }
        .bw .data-table .btnToggle {
            min-width: 22px; width: 22px; height: 22px !important; padding: 0 !important; margin-left: 4px;
            background: var(--ink); color: #fff; border-radius: 4px; font-size: 13px !important;
        }
        .bw .img_tool { display: inline-block; width: 24px; opacity: .8; }
        .bw .img_tool:hover { opacity: 1; }

        .bw .data-table .badge {
            display: inline-block; padding: 2px 10px; border-radius: 999px;
            font-size: 11.5px; font-weight: 700; text-shadow: none; color: #fff;
        }
        .bw .badge-normal    { background: #e3f4e8 !important; color: #16703d !important; border: 1px solid #b6e0c4; }
        .bw .badge-spare     { background: #fff7e0 !important; color: #946200 !important; border: 1px solid #f1d98c; }
        .bw .badge-borrowing { background: #fbeceb !important; color: #a82f2f !important; border: 1px solid #efb9b9; }

        .ddl_edit { font-size: 12px; text-align: center; }
        .ddl-warning { background-color: #fff3cd; border: 1px solid #ffc107; }
        .disabled-dropdown { background-color: #e9ecef; pointer-events: none; }
        select.ddl-warning + .select2-container .select2-selection--single,
        .ddl-warning + .select2-container > .select2-selection {
            border: 2px solid #ffc107 !important; background-color: #fff3cd !important;
        }

        /* ---------- Step 4 : ผลอนุมัติรายรายการ ---------- */
        .res-badge {
            display: inline-flex; align-items: center; gap: 4px;
            padding: 2px 10px; border-radius: 999px;
            font-size: 11.5px; font-weight: 700; white-space: nowrap; border: 1px solid transparent;
        }
        .res-ok     { background: #e3f4e8; color: #16703d; border-color: #b6e0c4; }
        .res-ng     { background: #fbeceb; color: #a82f2f; border-color: #efb9b9; }
        .res-wait   { background: #fff7e0; color: #946200; border-color: #f1d98c; }
        .res-review { background: #fdf1e3; color: #9e530a; border-color: #f0c79a; }
        .res-none   { background: #f0f1f3; color: #6b7480; border-color: #d9dde2; }
        .res-steps  { margin-top: 4px; display: flex; gap: 4px; justify-content: center; }
        .item-result-summary {
            display: flex; flex-wrap: wrap; align-items: center; gap: 8px;
            margin: 0 0 10px; font-size: 13px; color: var(--ink-2);
        }

        /* ---------- Rules / Note ---------- */
        .bw-rules {
            margin-top: 12px; padding: 10px 14px;
            background: #fdf3e7; border-left: 3px solid #d9833b; border-radius: var(--r-sm);
            font-size: 13px; color: var(--ink-2);
        }
        .bw-rules strong { display: block; margin-bottom: 4px; color: var(--ink); }
        .bw-rules ol { margin: 0; padding-left: 20px; }
        .bw-rules li { margin: 2px 0; line-height: 1.5; }

        /* ---------- Step 6 : Approver Flow ---------- */
        .card-approver { overflow: visible !important; }
        .apvflow-content { min-height: 90px; }
        .apvflow-needgen {
            padding: 12px 14px; border: 1px dashed #c9d2dc; border-radius: var(--r-sm);
            background: var(--paper); color: var(--muted); font-size: 13px;
        }      
        /* แผนกใต้ badge ขั้น */
        .apv-step-dept { display: block; margin-top: 4px; font-size: 11px; color: var(--muted); white-space: normal; line-height: 1.3; }
        /* สถานะใน Send Modal */
        .apv-st-mail { background: #e8f0fe; color: #1a5bc4; border-color: #bcd1f7; }
        /* ---------- Step 6 : J3 Picker ---------- */
        .j3-wrap { display: flex; gap: 40px; align-items: stretch; }
        .j3-card {
            position: relative; flex: 1; min-width: 0;
            padding: 14px 16px 12px;
            background: #fff; border: 1px solid var(--line); border-radius: var(--r-md);
            border-top: 4px solid #1f6feb;
            transition: border-color .2s, box-shadow .2s;
        }
        .j3-card-r { border-top-color: #c0660e; }
        .j3-card.is-done { box-shadow: 0 0 0 3px rgba(22,136,74,.12); }

        /* ลูกศรเชื่อม 1 → 2 (แสดงเฉพาะตอนมีการ์ดขวา) */
        .j3-card-r::before {
            content: '\2192';
            position: absolute; left: -32px; top: 22px;
            width: 24px; height: 24px; border-radius: 50%;
            display: flex; align-items: center; justify-content: center;
            background: var(--paper); border: 1px solid var(--line);
            color: var(--muted); font-size: 14px; font-weight: 700;
        }

        .j3-card-head { display: flex; align-items: center; gap: 10px; margin-bottom: 12px; }
        .j3-step {
            flex: 0 0 30px; width: 30px; height: 30px; border-radius: 50%;
            display: inline-flex; align-items: center; justify-content: center;
            background: #1f6feb; color: #fff; font-size: 14px; font-weight: 700;
        }
        .j3-card-r .j3-step { background: #c0660e; }
        .j3-card-title { min-width: 0; }
        .j3-title { font-size: 15px; font-weight: 700; color: var(--ink); line-height: 1.2; }
        .j3-title small { margin-left: 6px; font-size: 12px; font-weight: 400; color: var(--muted); }
        .j3-sub { margin-top: 2px; font-size: 12.5px; color: var(--muted); }
        .j3-sub b { color: var(--ink-2); }

        .j3-count {
            margin-left: auto; flex-shrink: 0;
            padding: 3px 10px; border-radius: 999px;
            font-size: 12px; font-weight: 700; white-space: nowrap;
        }
        .j3-count.req { background: #fbeceb; color: #a82f2f; border: 1px solid #efb9b9; }
        .j3-count.ok  { background: #e3f4e8; color: #16703d; border: 1px solid #b6e0c4; }

        /* ช่องค้นหา */
        .j3-picker { position: relative; width: 100%; }
        .j3-inputbox {
            display: flex; align-items: center; gap: 8px;
            height: 40px; padding: 0 12px;
            background: var(--paper); border: 1px solid #c9d2dc; border-radius: 8px; cursor: text;
            transition: border-color .15s, box-shadow .15s, background .15s;
        }
        .j3-inputbox:focus-within { background: #fff; border-color: var(--focus); box-shadow: 0 0 0 3px rgba(31,111,235,.15); }
        .j3-search-ico { top: 0; color: #9aa6b2; font-size: 13px; }
        .j3-input {
            flex: 1; min-width: 0; height: 30px !important; margin: 0 !important; padding: 0 !important;
            border: none !important; outline: none; box-shadow: none !important;
            background: transparent !important; font-size: 13.5px; color: var(--ink);
        }
        .j3-input::placeholder { color: #9aa6b2; }

        /* dropdown (เปิดขึ้นบน เพราะอยู่ใกล้ action bar) */
        .j3-dropdown {
            position: absolute; z-index: 10001; left: 0; right: 0;
            bottom: calc(100% + 6px);
            max-height: 260px; overflow-y: auto; padding: 4px;
            background: #fff; border: 1px solid var(--line); border-radius: var(--r-md);
            box-shadow: 0 -12px 32px rgba(20,30,42,.18);
        }
        .j3-item {
            display: flex; align-items: center; gap: 10px;
            padding: 8px 10px; border-radius: 6px; cursor: pointer;
        }
        .j3-item:hover, .j3-item.active { background: #eef5ff; }
        .j3-item .j3-plus { margin-left: auto; color: #9aa6b2; font-size: 18px; font-weight: 700; }
        .j3-item:hover .j3-plus, .j3-item.active .j3-plus { color: var(--confirm); }
        .j3-meta { display: flex; flex-direction: column; min-width: 0; text-align: left; }
        .j3-name { font-size: 13px; font-weight: 600; color: var(--ink); }
        .j3-name .j3-id { color: var(--muted); font-weight: 500; margin-right: 4px; font-variant-numeric: tabular-nums; }
        .j3-item .j3-sub2 { font-size: 11.5px; color: var(--muted); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        .j3-item mark { padding: 0; background: #fff1a8; color: inherit; border-radius: 2px; }
        .j3-empty { padding: 16px; text-align: center; color: var(--muted); font-size: 13px; }

        /* รายชื่อที่เลือก */
        .j3-selected { display: flex; flex-direction: column; gap: 6px; margin-top: 10px; }
        .j3-sel {
            display: flex; align-items: center; gap: 10px;
            padding: 7px 8px 7px 10px;
            background: #f7faff; border: 1px solid #d6e4fb; border-radius: 8px;
        }
        .j3-card-r .j3-sel { background: #fffaf3; border-color: #f1dcc0; }
        .j3-sel-meta { flex: 1; min-width: 0; display: flex; flex-direction: column; text-align: left; }
        .j3-sel-name { font-size: 13px; font-weight: 600; color: var(--ink); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        .j3-sel-name .j3-id { color: var(--muted); font-weight: 500; margin-right: 4px; }
        .j3-sel-sub { font-size: 11.5px; color: var(--muted); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        .j3-sel-x {
            flex-shrink: 0; width: 28px; height: 28px; padding: 0;
            border: none; border-radius: 6px; background: transparent;
            color: #8a95a1; font-size: 18px; line-height: 1; cursor: pointer;
        }
        .j3-sel-x:hover { background: #fbeceb; color: var(--danger); }
        .j3-sel-empty {
            padding: 12px; text-align: center; font-size: 12.5px; color: #9aa6b2;
            border: 1px dashed #d4dbe3; border-radius: 8px;
        }

        .j3-avatar {
            width: 32px; height: 32px; flex-shrink: 0; border-radius: 50%;
            display: inline-flex; align-items: center; justify-content: center;
            color: #fff; font-weight: 700; font-size: 12px;
        }

        @media (max-width: 900px) {
            .j3-wrap { flex-direction: column; gap: 34px; }
            .j3-card-r::before { content: '\2193'; left: 50%; top: -30px; transform: translateX(-50%); }
        }

        /* ---------- Step 6 : Approver Table ---------- */
        .apv-wrap { max-height: 340px; }
        .bw table.apv-table { min-width: 860px; }
        .bw .apv-table th { background: #33475b; }
        .bw .apv-table td { padding: 9px 10px; border-bottom: 1px solid var(--line-soft); }
        .bw .apv-table td.apv-left { text-align: left; }

        /* สีกลุ่ม (ทับ zebra เดิม) */
        .bw .apv-table tr.apv-a1 td { background: #fff; }
        .bw .apv-table tr.apv-a2 td { background: #fffcf7; }
        .bw .apv-table tr.apv-a1 td:first-child { box-shadow: inset 4px 0 0 #1f6feb; }
        .bw .apv-table tr.apv-a2 td:first-child { box-shadow: inset 4px 0 0 #c0660e; }
        .bw .apv-table tr.grp-start:not(:first-child) td { border-top: 2px solid #c9d2dc; }
        .bw .apv-table tr:hover td { background: #eef5ff; }
        .bw .apv-table tr.apv-actor td { font-weight: 600; }

        /* badge ขั้น */
        .apv-step {
            display: inline-flex; align-items: center; gap: 6px;
            padding: 3px 10px; border-radius: 999px;
            font-size: 12px; font-weight: 700; white-space: nowrap;
        }
        .apv-step i {
            width: 18px; height: 18px; border-radius: 50%;
            display: inline-flex; align-items: center; justify-content: center;
            color: #fff; font-style: normal; font-size: 11px;
        }
        .apv-step-1 { background: #e8f0fe; color: #1a5bc4; }  .apv-step-1 i { background: #1f6feb; }
        .apv-step-2 { background: #fdf1e3; color: #9e530a; }  .apv-step-2 i { background: #c0660e; }

        /* คน */
        .apv-person { display: flex; align-items: center; gap: 10px; min-width: 200px; }
        .apv-person .j3-avatar { width: 34px; height: 34px; font-size: 12px; }
        .apv-person-meta { display: flex; flex-direction: column; line-height: 1.3; }
        .apv-person-name { font-size: 13px; font-weight: 600; color: var(--ink); }
        .apv-person-id { font-size: 11.5px; color: var(--muted); font-variant-numeric: tabular-nums; }

        /* ตำแหน่ง */
        .apv-pos-code {
            display: inline-block; margin-right: 6px; padding: 1px 7px; border-radius: 4px;
            background: var(--paper); border: 1px solid var(--line);
            font-size: 11px; font-weight: 700; color: var(--ink-2);
        }
        .apv-pos-name { font-size: 12.5px; color: var(--ink-2); }

        /* ติดต่อ */
        .apv-contact { display: flex; flex-direction: column; gap: 2px; font-size: 12px; }
        .apv-contact span, .apv-contact a { display: inline-flex; align-items: center; gap: 6px; }
        .apv-contact .glyphicon { top: 0; font-size: 11px; color: #9aa6b2; }
        .apv-contact a { color: var(--confirm); text-decoration: none; }
        .apv-contact a:hover { text-decoration: underline; }
        .apv-muted { color: #b0b9c3; }

        /* สถานะ */
        .apv-st {
            display: inline-flex; align-items: center; gap: 6px;
            padding: 3px 11px 3px 9px; border-radius: 999px;
            font-size: 12px; font-weight: 700; white-space: nowrap; border: 1px solid transparent;
        }
        .apv-st::before { content: ''; width: 7px; height: 7px; border-radius: 50%; background: currentColor; }
        .apv-st-approved  { background: #e3f4e8; color: #16703d; border-color: #b6e0c4; }
        .apv-st-delegated { background: #e8f0fe; color: #1a5bc4; border-color: #bcd1f7; }
        .apv-st-wait      { background: #fff7e0; color: #946200; border-color: #f1d98c; }
        .apv-st-reject    { background: #fbeceb; color: #a82f2f; border-color: #efb9b9; }
        .apv-st-other     { background: #f0f1f3; color: #6b7480; border-color: #d9dde2; }
        .apv-date { font-size: 12.5px; color: var(--ink-2); font-variant-numeric: tabular-nums; }

        /* แถบความคืบหน้า */
        .apv-progress { display: flex; align-items: stretch; gap: 10px; margin-bottom: 12px; }
        .apv-prog-step {
            flex: 1; display: flex; align-items: center; gap: 10px;
            padding: 10px 14px; border-radius: var(--r-md);
            background: var(--paper); border: 1px solid var(--line);
        }
        .apv-prog-ico {
            flex: 0 0 30px; width: 30px; height: 30px; border-radius: 50%;
            display: inline-flex; align-items: center; justify-content: center;
            background: #c9d2dc; color: #fff; font-weight: 700; font-size: 14px;
        }
        .apv-prog-text { display: flex; flex-direction: column; line-height: 1.3; min-width: 0; }
        .apv-prog-title { font-size: 13.5px; font-weight: 700; color: var(--ink); }
        .apv-prog-sub { font-size: 12px; color: var(--muted); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        .apv-prog-step.done    { background: #f1faf4; border-color: #b6e0c4; }
        .apv-prog-step.done    .apv-prog-ico { background: #16884a; }
        .apv-prog-step.current { background: #fffbef; border-color: #f1d98c; }
        .apv-prog-step.current .apv-prog-ico { background: #d99a06; }
        .apv-prog-arrow { display: flex; align-items: center; color: #9aa6b2; font-size: 18px; font-weight: 700; }

        @media (max-width: 900px) {
            .apv-progress { flex-direction: column; }
            .apv-prog-arrow { justify-content: center; transform: rotate(90deg); }
        }

        /* ---------- Approve Modal : สถานะรายแถว ---------- */
        .bw .data-table tr.apv-row-off td { background: #fbeceb !important; color: #8f2b22; }
        .bw .data-table tr.apv-row-off td:first-child { box-shadow: inset 5px 0 0 #c93b3b; }
        .bw .data-table tr.apv-row-off td.rowGv_toolNo { text-decoration: line-through; }
        .bw .data-table tr.apv-mismatch td { background: #fff3cd !important; color: inherit; }
        .bw .data-table tr.apv-mismatch td:first-child { box-shadow: inset 5px 0 0 #d99a06; }
        .bw .data-table tr.apv-pair-start td { border-top: 2px solid #1a3a5c; }
        .apv-pair-lbl { display: block; margin-bottom: 3px; font-size: 11px; font-weight: 700; color: var(--muted); }
        .apv-mismatch-notice {
            margin-bottom: 12px; padding: 10px 14px;
            background: #fbeceb; border: 1px solid #efb9b9; border-left: 4px solid #c93b3b;
            border-radius: 8px; font-size: 13px; color: #8f2b22;
        }
        .apv-mismatch-notice b { display: block; margin-bottom: 2px; }
        /* ---------- Banner ถูก Reject (โหมด Issue) ---------- */
        .reject-banner {
            margin-bottom: 5px; padding: 12px 18px;
            background: #fbeceb; border: 1px solid #efb9b9; border-left: 6px solid #c93b3b;
            border-radius: 10px; font-size: 13.5px; color: #6e2119;
        }
        .reject-banner > b { display: block; font-size: 15px; color: #a82f2f; margin-bottom: 4px; }
        .reject-banner-reason { margin-top: 6px; padding-top: 6px; border-top: 1px dashed #e8b4b4; }
        .reject-banner-hint { margin-top: 6px; color: #33475b; }

        /* ---------- Action Bar ---------- */
        .action-bar {
            position: fixed; right: 16px; bottom: 16px; z-index: 100;
            display: flex; align-items: center; justify-content: space-between; gap: 16px;
            padding: 10px 14px;
            background: #90b3ff63; border: 1px solid #7ca0dfa6; border-radius: 12px;
            box-shadow: 0 8px 24px rgba(28,43,58,.16);
        }
        .action-bar-left, .action-bar-right { display: flex; align-items: center; gap: 8px; }

        .ab-btn {
            display: inline-flex; align-items: center; justify-content: center; gap: 6px;
            height: 38px; padding: 0 18px;
            font-family: 'IBM Plex Sans Thai', 'Segoe UI', Tahoma, sans-serif;
            font-size: 14px; font-weight: 700; text-decoration: none; text-shadow: none;
            border-radius: 8px; border: 1px solid transparent; cursor: pointer;
            transition: background .15s, border-color .15s;
        }
        .ab-btn:hover, .ab-btn:focus { text-decoration: none; }
        .ab-send    { background: #16884a; color: #fff; }  .ab-send:hover    { background: #11703c; color: #fff; }
        .ab-confirm { background: #1f6feb; color: #fff; }  .ab-confirm:hover { background: #1858c4; color: #fff; }
        .ab-reject  { background: #c93b3b; color: #fff; }  .ab-reject:hover  { background: #a82f2f; color: #fff; }
        .ab-draft   { background: #fff; color: #33475b; border-color: #c9d2dc; }
        .ab-draft:hover { background: #f3f5f8; color: #1c2b3a; }
        .ab-cancel  { background: transparent; color: #a82f2f; font-weight: 600; }
        .ab-cancel:hover { background: #fbeceb; color: #a82f2f; }
        .ab-btn:focus-visible { outline: 2px solid #1f6feb; outline-offset: 2px; }

        .tooltip-wrap { position: relative; display: inline-block; }
        .tooltip-wrap .tooltip-box {
            visibility: hidden; opacity: 0;
            position: absolute; z-index: 9999; bottom: 125%; left: 50%;
            transform: translateX(-50%) translateY(4px);
            width: max-content; max-width: 220px; padding: 6px 10px;
            background: #1c2b3a; color: #fff; border-radius: 6px;
            font-size: 12px; white-space: nowrap; pointer-events: none;
            transition: opacity .15s, transform .15s;
        }
        .tooltip-wrap .tooltip-box::after {
            content: ""; position: absolute; top: 100%; left: 50%; margin-left: -6px;
            border: 6px solid transparent; border-top-color: #1c2b3a;
        }
        .tooltip-wrap:hover .tooltip-box { visibility: visible; opacity: 1; transform: translateX(-50%) translateY(0); }

        /* ---------- Custom modal (Send / Approve / Reject) ---------- */
        .modal-overlay {
            position: fixed; inset: 0; z-index: 10000;
            background: rgba(20,30,42,.45);
            align-items: center; justify-content: center;
        }
        .modal-box {
            display: flex; flex-direction: column;
            width: 700px; max-width: 94vw; max-height: 90vh;
            background: #fff; border-radius: 12px; overflow: hidden;
            box-shadow: 0 20px 50px rgba(20,30,42,.3);
        }
        .modal-head {
            display: flex; align-items: center; justify-content: space-between; gap: 12px;
            padding: 14px 20px; background: #fff; border-bottom: 1px solid var(--line-soft);
            font-size: 15px; font-weight: 700; color: var(--ink);
        }
        .modal-head .doc { color: var(--confirm); }
        .modal-x {
            width: 32px; height: 32px; border-radius: 6px;
            background: none; border: none; color: var(--muted); font-size: 24px; line-height: 1; cursor: pointer;
        }
        .modal-x:hover { background: var(--paper); color: var(--ink); }
        .modal-box > .modal-content {
            padding: 16px 20px; overflow-y: auto;
            border: none; box-shadow: none; border-radius: 0; background: #fff;
        }
        .modal-foot {
            display: flex; justify-content: flex-end; gap: 8px;
            padding: 12px 20px; border-top: 1px solid var(--line-soft);
        }
        .msec-title { display: flex; align-items: center; gap: 8px; margin: 0 0 8px; font-size: 14px; font-weight: 700; color: var(--ink); }
        .msec-title small { font-size: 12px; font-weight: 400; color: var(--muted); }
        .msec-title .req { color: var(--danger); font-size: 12px; font-weight: 600; }

        #<%= gvApprove.ClientID %> .apv-cb input[type=checkbox], #apvCheckAll {
            width: 18px; height: 18px; cursor: pointer; accent-color: #16884a;
        }

        /* Confirm dialog (เปลี่ยนโหมด / แผนกผู้อนุมัติเปลี่ยน) */
        .mode-confirm-overlay {
            position: fixed; inset: 0; z-index: 10040; display: none;
            align-items: center; justify-content: center;
            background: rgba(20,30,42,.5);
        }
        .mode-confirm-box {
            max-width: 460px; padding: 24px 28px; text-align: center;
            background: #fff; border-radius: 12px; box-shadow: 0 20px 50px rgba(20,30,42,.3);
            font-family: 'IBM Plex Sans Thai', 'Segoe UI', Tahoma, sans-serif;
        }
        .mode-confirm-icon {
            width: 48px; height: 48px; margin: 0 auto 10px; border-radius: 50%;
            display: flex; align-items: center; justify-content: center;
            background: #fdf1e3; font-size: 24px;
        }
        .mode-confirm-title { font-size: 17px; font-weight: 700; color: #1c2b3a; margin-bottom: 8px; }
        .mode-confirm-text { font-size: 14px; color: #33475b; line-height: 1.6; margin-bottom: 20px; }
        .mode-confirm-actions { display: flex; gap: 10px; justify-content: center; }
        .btn-mc {
            display: inline-flex; align-items: center; height: 38px; padding: 0 20px;
            border-radius: 8px; font-size: 14px; font-weight: 700; cursor: pointer; border: 1px solid transparent;
            text-decoration: none !important;
        }
        .btn-mc-cancel { background: #fff; color: #33475b; border-color: #c9d2dc; }
        .btn-mc-cancel:hover { background: #f3f5f8; }
        .btn-mc-ok { background: #c0660e; color: #fff !important; }
        .btn-mc-ok:hover { background: #9e530a; }

        /* ---------- Bootstrap modal (รูป / HRMS) ---------- */
        .bw-modal.modal {
            position: fixed; top: 0 !important; left: 0; right: 0; bottom: 0;
            width: auto; margin: 0; background: none; border: none; box-shadow: none; overflow-y: auto;
        }
        .bw-modal .modal-dialog { width: 760px; max-width: 94%; margin: 60px auto; }
        .bw-modal .modal-content {
            border: none; border-radius: 12px; overflow: hidden;
            box-shadow: 0 20px 50px rgba(20,30,42,.3);
            font-family: 'IBM Plex Sans Thai', 'Segoe UI', Tahoma, sans-serif;
        }
        .bw-modal .modal-header {
            display: flex; align-items: center; justify-content: space-between;
            padding: 14px 20px; background: #fff; border-bottom: 1px solid #edf0f4;
        }
        .bw-modal .modal-title { margin: 0; font-size: 15px; font-weight: 700; color: #1c2b3a; }
        .bw-modal .modal-header .close { order: 2; opacity: .6; font-size: 24px; text-shadow: none; color: #1c2b3a; }
        .bw-modal .modal-header .close:hover { opacity: 1; }
        .bw-modal .modal-body { max-height: 72vh; overflow-y: auto; padding: 16px 20px; color: #1c2b3a; }
        .bw-modal .modal-footer { display: flex; justify-content: flex-end; gap: 8px; padding: 12px 20px; border-top: 1px solid #edf0f4; }

        /* ---------- Toast ---------- */
        #toast-container { position: fixed; top: 16px; right: 16px; display: flex; flex-direction: column; gap: 8px; z-index: 9999; }
        .toast {
            min-width: 260px; max-width: 560px; padding: 12px 14px;
            border-radius: 8px; color: #fff; box-shadow: 0 6px 18px rgba(0,0,0,.15);
            display: flex; align-items: start; gap: 10px;
            animation: slide-in 200ms ease-out;
            font-family: 'IBM Plex Sans Thai', 'Segoe UI', Tahoma, sans-serif; font-size: 15px;
        }
        .toast .title { font-weight: 700; margin-bottom: 4px; }
        .toast .message { opacity: .95; white-space: pre-line; }
        .toast.success { background: #16884a; }
        .toast.error   { background: #c93b3b; }
        .toast.warn    { background: #c0660e; }
        .toast.info    { background: #1f6feb; }
        .toast .close { margin-left: auto; background: transparent; border: none; color: inherit; font-size: 18px; cursor: pointer; line-height: 1; }
        @keyframes slide-in { from { opacity: 0; transform: translateY(-10px); } to { opacity: 1; transform: translateY(0); } }

        #toast-center {
            position: fixed; top: 20%; left: 50%; transform: translate(-50%, -50%); z-index: 10050;
            display: flex; flex-direction: column; gap: 10px; align-items: center; pointer-events: none;
        }
        #toast-center .toast {
            pointer-events: auto; min-width: 460px; max-width: 900px;
            padding: 16px 20px; border-radius: 12px; font-size: 15px;
            box-shadow: 0 16px 40px rgba(0,0,0,.3); animation: toastCenterIn .2s ease-out;
        }
        #toast-center .toast .title { font-size: 16px; }
        @keyframes toastCenterIn { from { opacity: 0; transform: scale(.94); } to { opacity: 1; transform: scale(1); } }
        #toast-center-backdrop {
            position: fixed; inset: 0; z-index: 10045; display: none;
            background: rgba(20,30,42,.25); backdrop-filter: blur(2px);
        }

        /* ---------- Loading ---------- */
        .overlay {
            position: fixed; inset: 0; z-index: 9998;
            display: flex; align-items: center; justify-content: center;
            background: rgba(20,30,42,.45); backdrop-filter: blur(2px);
        }
        .overlayContent {
            min-width: 280px; min-height:180px; padding: 26px 40px; text-align: center;
            background: #fff; border-radius: 12px; box-shadow: 0 12px 40px rgba(0,0,0,.28);
            font-family: 'IBM Plex Sans Thai', 'Segoe UI', Tahoma, sans-serif;
        }
        .overlayContent .spin-ring {
            width: 44px; height: 44px; margin: 0 auto 14px;
            border: 4px solid #e2e8f0; border-top-color: #1c2b3a; border-radius: 50%;
            animation: spin .8s linear infinite;
        }
        .overlayContent .ov-title { font-size: 16px; font-weight: 700; color: #1c2b3a; margin-bottom: 4px; }
        .overlayContent .ov-sub   { font-size: 12.5px; color: #66727f; margin-bottom: 14px; }
        .ov-bar { height: 6px; background: #e2e8f0; border-radius: 6px; overflow: hidden; }
        .ov-bar > i { display: block; height: 100%; width: 40%; border-radius: 6px; background: #16884a; animation: barrun 1.1s ease-in-out infinite; }
        @keyframes spin { to { transform: rotate(360deg); } }
        @keyframes barrun { 0% { margin-left: -40%; } 100% { margin-left: 100%; } }

        /* ---------- Select2 ---------- */
        .select2-container--default .select2-selection--single {
            height: 28px; font-size: 12px; text-align: center;
            border: 1px solid #c9d2dc; border-radius: 5px; padding-top: 1px;
        }
        .select2-container--default .select2-selection--single .select2-selection__rendered { line-height: 24px; text-align: center; }
        .select2-container--default .select2-selection--single .select2-selection__arrow { height: 26px; }
        .select2-container--default .select2-results__option { font-size: 12px; }
        .select2-container--default .select2-search--dropdown .select2-search__field { font-size: 13px; background: #eef5ff; }
        .select2-results { text-align: center; }

        @media (max-width: 900px) {
            .bw-meta { margin-left: 0; }
            #j3Wrap { flex-direction: column; }
            .action-bar { left: 16px; flex-direction: column; align-items: stretch; }
            .action-bar-left, .action-bar-right { flex-wrap: wrap; }
        }

        /* ---------- Read Me Modal ---------- */
        .bw-modal .rm-dialog { width: 860px; }
        .bw-modal .modal-title { display: flex; align-items: center; gap: 8px; }
        .bw-modal .modal-header .close { margin-left: auto; }

        .rm-tabs {
            display: flex; gap: 4px; padding: 0 20px;
            background: #fff; border-bottom: 1px solid #edf0f4; overflow-x: auto;
        }
        .rm-tab {
            padding: 10px 14px; margin-bottom: -1px;
            background: none; border: none; border-bottom: 3px solid transparent;
            font-family: inherit; font-size: 13.5px; font-weight: 600; color: #66727f;
            white-space: nowrap; cursor: pointer;
        }
        .rm-tab:hover { color: #1c2b3a; }
        .rm-tab.active { color: #1f6feb; border-bottom-color: #1f6feb; }

        .rm-body { font-size: 13.5px; line-height: 1.6; }
        .rm-pane { display: none; }
        .rm-pane.active { display: block; animation: rmFade .15s ease-out; }
        @keyframes rmFade { from { opacity: 0; } to { opacity: 1; } }

        /* ขั้นตอน */
        .rm-step {
            display: flex; align-items: flex-start; gap: 12px;
            padding: 10px 14px; border-bottom: 2px dashed #e2e8f0;
        }
        .rm-step:last-of-type { border-bottom: none; }
        .rm-step .step-no { margin-top: 1px; }
        .rm-step-text { flex: 1; min-width: 0; }
        .rm-step-text b { font-size: 14px; color: #1c2b3a; }
        .rm-step-text p { margin: 2px 0 0; color: #33475b; }
        .rm-step .manual-btn { flex-shrink: 0; margin-top: 2px; }

        .rm-kbd {
            display: inline-block; padding: 0 8px; margin: 0 2px;
            border: 1px solid #c9d2dc; border-radius: 5px; background: #fff;
            font-size: 12px; font-weight: 700; color: #33475b; line-height: 20px; white-space: nowrap;
        }
        .rm-kbd-green { background: #16884a; border-color: #16884a; color: #fff; }
        .rm-kbd-red-o { color: #a82f2f; border-color: #efb9b9; }

        .rm-box {
            /*margin: 12px;*/ padding: 12px 14px;
            background:#f0e2d0ba; border: 1px solid #e2e8f0; border-radius: 10px;
        }
        .rm-box > b { display: block; margin-bottom: 4px; color: #1c2b3a; }
        .rm-box ol { margin: 0; padding-left: 20px; }
        .rm-box li { margin: 3px 0; }
        .rm-btnrow { margin: 6px 0; color: #33475b; }

        .rm-note {
            margin:12px; padding: 8px 12px;
            border-left: 3px solid #d9822b; border-radius: 6px;
            background: #fdf3e7; color: #8a4b0c; font-size: 13px;
        }

        /* รูปแบบการแจ้ง */
        .rm-compare { display: flex; gap: 12px; }
        .rm-card { flex: 1; padding: 12px 14px; border: 1px solid #dde3ea; border-radius: 10px; border-top: 4px solid #16884a; }
        .rm-card + .rm-card { border-top-color: #1f6feb; }
        .rm-card-title { display: flex; align-items: center; gap: 8px; margin-bottom: 6px; font-weight: 700; color: #1c2b3a; }
        .rm-card-title .glyphicon { top: 0; color: #66727f; }
        .rm-card ul { margin: 0; padding-left: 18px; color: #33475b; }
        .rm-card li { margin: 3px 0; }

        /* flow */
        .rm-flow {
            display: flex; align-items: flex-start; flex-wrap: wrap; gap: 6px;
            padding: 14px; border: 1px solid #e2e8f0; border-radius: 10px; background: #fff;
        }
        .rm-flow-node { display: flex; flex-direction: column; align-items: center; gap: 4px; }
        .rm-flow-node small { font-size: 11px; color: #66727f; white-space: nowrap; }
        .rm-flow-arrow { padding-top: 2px; color: #9aa6b2; font-size: 20px; font-weight: 700; line-height: 1; }

        /* legend */
        .rm-legend { width: 100%; margin-bottom: 12px; border-collapse: collapse; }
        .rm-legend td { padding: 7px 10px; border-bottom: 1px solid #edf0f4; vertical-align: middle; color: #33475b; }
        .rm-legend td:first-child { width: 190px; }
        .rm-legend tr:last-child td { border-bottom: none; }
        .rm-swatch { display: inline-block; width: 44px; height: 22px; border: 1px solid; border-radius: 5px; }

        @media (max-width: 700px) {
            .rm-compare { flex-direction: column; }
            .rm-legend td:first-child { width: auto; }
        }
        .apv-return-notice {
            margin-bottom: 12px; padding: 10px 14px;
            background: #fff7e0; border: 1px solid #f1d98c; border-left: 4px solid #d99a06;
            border-radius: 8px; font-size: 13px; color: #6b4d00;
        }
        .apv-return-notice b { display: block; margin-bottom: 2px; color: #946200; }
        .apv-return-detail { margin-top: 6px; padding-top: 6px; border-top: 1px dashed #e6cf85; font-size: 12.5px; }
        .bw .data-table tr.apv-diff td { background: #fff3cd !important; }
        .bw .data-table tr.apv-diff td:first-child { box-shadow: inset 4px 0 0 #d99a06; }
        .prev-mk {
            display: inline-block; padding: 1px 7px; border-radius: 999px;
            font-size: 11px; font-weight: 700; background: #f0f1f3; color: #6b7480;
        }
        .prev-y { background: #e3f4e8; color: #16703d; }
        .prev-n { background: #fbeceb; color: #a82f2f; }

        .bw textarea.fld-area.fld-required { background: #fff3cd; border-color: #ffc107; }

        /* ✅ 6F-5 : Overlay ต้องอยู่เหนือ Modal (10000) และ Toast ต้องอยู่เหนือ Overlay */
        .overlay               { z-index: 10060 !important; }
        #toast-container       { z-index: 10070 !important; }
        #toast-center-backdrop { z-index: 10075 !important; }
        #toast-center          { z-index: 10080 !important; }

        /* ---------- แถบนำทางโหมด Approve / Confirm ---------- */
        .mode-guide {
            display: flex; align-items: center; flex-wrap: wrap; gap: 10px 16px;
            margin-bottom: 5px; padding: 10px 18px;
            background: #eef5ff; border: 1px solid #bcd1f7; border-left: 6px solid #1f6feb;
            border-radius: 10px; font-size: 13px; color: #1c2b3a;
        }
        .mode-guide.mg-confirm { background: #f8eefb; border-color: #dfc0e6; border-left-color: #92278f; }
        .mode-guide .mg-title { font-size: 14.5px; font-weight: 700; margin-right: 4px; }
        .mode-guide ol { display: flex; flex-wrap: wrap; gap: 4px 14px; margin: 0; padding: 0; list-style: none; counter-reset: mg; }
        .mode-guide li { counter-increment: mg; display: inline-flex; align-items: center; gap: 6px; }
        .mode-guide li::before {
            content: counter(mg);
            width: 20px; height: 20px; border-radius: 50%;
            display: inline-flex; align-items: center; justify-content: center;
            background: #1f6feb; color: #fff; font-size: 11px; font-weight: 700;
        }
        .mode-guide.mg-confirm li::before { background: #92278f; }
        .mode-guide .mg-btn {
            margin-left: auto; display: inline-flex; align-items: center; gap: 6px;
            height: 32px; padding: 0 14px; border-radius: 8px;
            background: #fff; border: 1px solid #1f6feb; color: #1f6feb !important;
            font-weight: 700; text-decoration: none !important; white-space: nowrap;
        }
        .mode-guide .mg-btn:hover { background: #1f6feb; color: #fff !important; }
        .mode-guide.mg-confirm .mg-btn { border-color: #92278f; color: #92278f !important; }
        .mode-guide.mg-confirm .mg-btn:hover { background: #92278f; color: #fff !important; }

        .apv-prog-dept { margin-left: 4px; font-size: 11.5px; font-weight: 600; color: var(--muted); }

        /* ---------- AAD Confirm Modal ---------- */
        /*.bw .data-table th.hdr-aad { background: #92278f; }*/
        .bw .data-table tr td.col-aad { background: #f8eefb; }
        .bw .data-table td.col-aad select { min-width: 170px; font-size: 12px; }
        .cfm-summary { display: flex; flex-wrap: wrap; gap: 8px; align-items: center; margin-bottom: 10px; font-size: 13px; }

        /* ✅ Modal AAD Confirm : ช่องที่ AAD ต้องตรวจ */
        .bw .data-table td.col-aad select.aad-ddl {
            min-width: 180px; height: 30px;
            background: #fff3cd !important; border: 2px solid #ffc107 !important;
            border-radius: 6px; font-size: 12.5px; font-weight: 700; color: #6b4d00;
        }
        .bw .data-table td.col-aad select.aad-ddl:focus {
            outline: none; box-shadow: 0 0 0 3px rgba(255,193,7,.35);
        }
        .bw .data-table td.col-aad select.aad-ddl-empty {
            background: #fbeceb !important; border-color: #c93b3b !important; color: #a82f2f;
        }
        .bw .data-table th.hdr-aad { background: #c79100; color: #fff; }   /* หัวคอลัมน์โทนเหลืองเข้มให้เข้าชุด */

        /* กล่องเตือนใน Modal */
        .cfm-warn {
            display: flex; gap: 12px; align-items: flex-start;
            margin: 0 0 12px; padding: 12px 16px;
            background: #fff7e0; border: 1px solid #f1d98c; border-left: 6px solid #d99a06;
            border-radius: 10px; font-size: 13.5px; color: #5c4300;
        }
        .cfm-warn .cfm-warn-ico { font-size: 22px; line-height: 1; }
        .cfm-warn b.cfm-warn-title { display: block; font-size: 15px; color: #946200; margin-bottom: 4px; }
        .cfm-warn ul { margin: 4px 0 0; padding-left: 18px; }
        .cfm-warn li { margin: 2px 0; }
        .cfm-warn .cfm-chip {
            display: inline-block; padding: 0 8px; border-radius: 4px;
            background: #fff3cd; border: 1px solid #ffc107; font-weight: 700; color: #6b4d00;
        }

    </style>

    <asp:ScriptManager runat="server" ID="scriptManager1" EnablePageMethods="true" AsyncPostBackTimeout="300"></asp:ScriptManager>

    <asp:UpdatePanel runat="server" ID="UP1">
        <ContentTemplate>

            <%-- Loading Overlay --%>
            <div id="loadingOverlay" class="overlay" style="display:none;">
                <div class="overlayContent">
                    <div class="spin-ring"></div>
                    <div class="ov-title">กำลังประมวลผล...</div>
                    <div class="ov-sub">กรุณารอสักครู่</div>
                    <div class="ov-bar"><i></i></div>
                </div>
            </div>

            <div id="toast-container" aria-live="polite" aria-atomic="true"></div>

            <div class="bw">

                <%-- ===== Page Header ===== --%>
                <div class="bw-header">
                    <img src="images/location.png" alt="" />
                    <div>
                        <h1>แจ้งเปลี่ยนแปลง</h1>
                        <p>Fixed Asset / Calibration — ย้ายสถานที่ใช้งาน หรือสลับอุปกรณ์ระหว่างแผนก</p>
                    </div>

                    <div class="bw-meta">
                        <div class="meta-item">
                            <span class="meta-label">Issue No.</span>
                            <span class="meta-doc"><asp:Literal ID="litIssueNo" runat="server" Text="-" /></span>
                        </div>
                        <div class="meta-item">
                            <span class="meta-label">สถานะ</span>
                            <asp:Literal ID="litStatus" runat="server" Text="<span class='status-badge status-draft'>Wait Issue</span>" />
                        </div>
                        <div class="meta-item">
                            <span class="meta-label">โหมด</span>
                            <asp:Label ID="lb_mode" runat="server" Text="Issue" CssClass="mode-badge mode-issue" />
                        </div>
                        <span class="glyphicon glyphicon-question-sign readme-btn"
                              title="วิธีใช้งาน" onclick="$('#modal_ReadMe').modal('show');"></span>
                    </div>

                    <asp:HiddenField ID="hdIsAdmin" runat="server" Value="0" />
                    <asp:HiddenField ID="hfReadMeTab" runat="server" Value="rmSteps" />
                    <asp:HiddenField ID="hfManualFile" runat="server" Value="" />

                </div>

                <asp:Literal ID="litRejectBanner" runat="server" />

                <asp:Literal ID="litModeGuide" runat="server" />

                <%--=====================================================--%>
                <%-- STEP 1 : ผู้ร้องขอ --%>
                <%--=====================================================--%>
                <div class="bw-card">

                    <div class="bw-card-head">
                        <asp:Literal ID="step_requester" runat="server" Text="<span class='step-no'>1</span>" />
                        <div class="bw-card-title">
                            <h2>ผู้ร้องขอ
                                <asp:LinkButton ID="btnManual1" runat="server" CssClass="manual-btn"
                                    OnClientClick="return openManualStep(1);" ToolTip="คู่มือ Step 1">
                                    <span class="glyphicon glyphicon-book"></span>
                                </asp:LinkButton>
                            </h2>
                            <p>กรอกรหัสพนักงานและเบอร์ติดต่อ ระบบจะดึงชื่อและแผนกให้อัตโนมัติ</p>
                        </div>
                    </div>

                    <div class="bw-card-body">

                        <asp:Panel ID="pnlDraftBanner" runat="server" Visible="false">
                            <div class="draft-banner">
                                พบรายการ <strong>Draft</strong> ที่ยังไม่ได้ส่ง
                                (Issue No.: <strong><asp:Literal ID="litDraftIssueNo" runat="server" /></strong>
                                วันที่ <asp:Literal ID="litDraftDate" runat="server" />)
                                ระบบโหลดรายการนั้นขึ้นมาให้แล้ว
                            </div>
                        </asp:Panel>

                        <div class="bw-fields">
                            <div class="bw-field">
                                <label for="<%= txtEmpId.ClientID %>">รหัสพนักงาน *</label>
                                <asp:TextBox ID="txtEmpId" runat="server" onkeyup="fetchRequesterName();"
                                    CssClass="fld fld-edit" MaxLength="6" Width="120px"
                                    placeholder="6 หลัก" AutoComplete="off" AutoPostBack="true"
                                    OnTextChanged="txtEmpId_TextChanged" />
                            </div>
                            <div class="bw-field">
                                <label for="<%= txtTel.ClientID %>">เบอร์ติดต่อ *</label>
                                <asp:TextBox ID="txtTel" runat="server" CssClass="fld fld-edit"
                                    MaxLength="4" Width="90px" placeholder="4 หลัก" />
                            </div>
                            <div class="bw-field">
                                <label>ชื่อ-สกุล</label>
                                <asp:TextBox ID="txtEmpName" runat="server" ReadOnly="true" Width="240px" CssClass="fld" placeholder="—" />
                            </div>
                            <div class="bw-field">
                                <label>ตำแหน่ง</label>
                                <asp:TextBox ID="txtPosition" runat="server" ReadOnly="true" Width="200px" CssClass="fld" placeholder="—" />
                            </div>
                            <div class="bw-field">
                                <label>แผนก</label>
                                <asp:TextBox ID="txtDept" runat="server" ReadOnly="true" Width="120px" CssClass="fld" placeholder="—" />
                            </div>
                            <div class="bw-field">
                                <label>Dept Code</label>
                                <asp:TextBox ID="txtDeptCode" runat="server" ReadOnly="true" Width="90px" CssClass="fld" placeholder="—" />
                            </div>
                            <div class="bw-field">
                                <label>วันที่ขอ</label>
                                <div class="input-group date dp-compact" id="IssueDatePicker">
                                    <asp:TextBox ID="txtIssueDate" runat="server" Width="120px"
                                        CssClass="fld" placeholder="วันที่" ReadOnly="true" AutoPostBack="true" />
                                    <span class="input-group-addon"><span class="glyphicon glyphicon-calendar"></span></span>
                                </div>
                            </div>

                            <asp:HiddenField ID="hdtxtBand" runat="server" />
                            <asp:HiddenField ID="hdtxtEmail" runat="server" />
                            <asp:HiddenField ID="hdNewMember_ck" runat="server" Value="-" />
                        </div>

                        <asp:Panel ID="pnlNewEmp" runat="server" Visible="false">
                            <div class="bw-notice"><asp:Label ID="lblempNotice" runat="server"></asp:Label></div>
                        </asp:Panel>
                    </div>

                </div>

                <%--=====================================================--%>
                <%-- STEP 2 : รูปแบบการแจ้ง --%>
                <%--=====================================================--%>
                <asp:Panel ID="pnlChangeMode" runat="server" CssClass="bw-card">

                    <div class="bw-card-head">
                        <asp:Literal ID="step_mode" runat="server" Text="<span class='step-no'>2</span>" />
                        <div class="bw-card-title">
                            <h2>รูปแบบการแจ้ง
                                <asp:LinkButton ID="btnManual2" runat="server" CssClass="manual-btn"
                                    OnClientClick="return openManualStep(2);" ToolTip="คู่มือ Step 2">
                                    <span class="glyphicon glyphicon-book"></span>
                                </asp:LinkButton>
                            </h2>
                            <p>กดสวิตช์เพื่อสลับรูปแบบ — การสลับจะล้างรายการที่เพิ่มไว้ทั้งหมด</p>
                        </div>
                    </div>

                    <div class="bw-card-body">
                        <asp:HiddenField ID="hfChangeMode" runat="server" Value="change" />
                        <asp:HiddenField ID="hfHasItems" runat="server" Value="0" />

                        <div class="mode-switch">
                            <span class="ms-label ms-left" id="msLabelL" runat="server">
                                <span class="ms-title">แจ้งเปลี่ยน Location</span>
                                <span class="ms-sub">ทุกรายการต้องเป็น Dept เดียวกัน</span>
                            </span>

                            <div class="ms-track" id="msTrack" runat="server" onclick="toggleMode();" title="คลิกเพื่อสลับรูปแบบ">
                                <span class="glyphicon glyphicon-random ms-icon ms-icon-left"></span>
                                <span class="glyphicon glyphicon-transfer ms-icon ms-icon-right"></span>
                                <span class="ms-knob" id="msKnob">
                                    <i class="glyphicon glyphicon-random ms-knob-icon-left" style="top:0px;"></i>
                                    <i class="glyphicon glyphicon-transfer ms-knob-icon-right" style="top:0px;"></i>
                                </span>
                            </div>

                            <span class="ms-label ms-right" id="msLabelR" runat="server">
                                <span class="ms-title">แจ้งสลับ Tool / Equipment</span>
                                <span class="ms-sub">จับคู่ทีละ 2 รายการ (ไม่เกิน 2 Dept)</span>
                            </span>

                            <%-- Admin เท่านั้น : แจ้งเปลี่ยนเครื่องมือวัด (Flow เดียวกับแจ้งสลับ) --%>
                            <span class="ms-measure" id="msMeasure" runat="server" visible="false"
                                onclick="selectMeasureMode();" title="Admin : แจ้งเปลี่ยนเครื่องมือวัด">
                                <span class="ms-title"><span class="glyphicon glyphicon-scale"></span> แจ้งเปลี่ยนเครื่องมือวัด</span>
                                <span class="ms-sub">จับคู่ทีละ 2 รายการ · Admin เท่านั้น</span>
                            </span>
                        </div>

                        <asp:LinkButton ID="btnModeToggle" runat="server" style="display:none;" OnClick="btnModeToggle_Click" />
                        <asp:LinkButton ID="btnModeChange" runat="server" style="display:none;" OnClick="btnModeChange_Click" />
                        <asp:LinkButton ID="btnModeSwap" runat="server" style="display:none;" OnClick="btnModeSwap_Click" />
                        <asp:LinkButton ID="btnModeMeasure" runat="server" style="display:none;" OnClick="btnModeMeasure_Click" />
                    </div>

                </asp:Panel>

                <%--=====================================================--%>
                <%-- STEP 3 : ค้นหาและเพิ่มอุปกรณ์ --%>
                <%--=====================================================--%>
                <asp:Panel ID="pnlSearch_Add" runat="server" Visible="true" CssClass="bw-card">

                    <div class="bw-card-head">
                        <asp:Literal ID="step_searchAdd" runat="server" Text="<span class='step-no'>3</span>" />
                        <div class="bw-card-title">
                            <h2>ค้นหาและเพิ่มอุปกรณ์
                                <asp:LinkButton ID="btnManual3" runat="server" CssClass="manual-btn"
                                    OnClientClick="return openManualStep(3);" ToolTip="คู่มือ Step 3">
                                    <span class="glyphicon glyphicon-book"></span>
                                </asp:LinkButton>
                            </h2>
                            <p>อุปกรณ์ที่ติดเอกสารอื่นอยู่จะกด Add ไม่ได้ — คลิกป้ายสถานะสีแดงเพื่อเปิดดูเอกสารนั้น</p>
                        </div>
                    </div>

                    <div class="bw-card-body">

                        <div class="search-bar">
                            <div class="bw-field">
                                <label for="<%= txtFixAsset.ClientID %>">คำค้นหา</label>
                                <asp:TextBox ID="txtFixAsset" runat="server" Width="240px"
                                    CssClass="fld fld-edit" placeholder="Fix Asset / Control No / Tool No" />
                            </div>
                            <asp:Button ID="btnSearchFixAsset" runat="server" Text="ค้นหา"
                                CssClass="tb-btn tb-primary" OnClick="btnSearch_Click" />

                            <div class="search-hint">
                                ตัวอย่าง:
                                <code>BFM00012345 <i>Fix Asset</i></code>
                                <code>1341001P <i>Control No</i></code>
                                <code>CPT-2024-U01800 <i>Computer Name</i></code>
                                <code>SZ99-FK2-7577-1J <i>Tool No</i></code>
                                <code>SURFACE GO4 <i>Model</i></code>
                            </div>
                        </div>

                        <%-- ลำดับคอลัมน์เดิม (Cells[1] = Add) --%>
                        <div class="tbl-wrap" style="max-height: 200px;">
                            <asp:GridView ID="gvSearchResult" runat="server"
                                CssClass="data-table fixed-header" GridLines="None" CellSpacing="0"
                                AutoGenerateColumns="false"
                                OnRowCommand="gvSearchResult_RowCommand"
                                OnRowDataBound="gvSearchResult_RowDataBound"
                                style="min-width:1300px;"
                                EmptyDataText="ยังไม่มีผลการค้นหา — พิมพ์คำค้นหาแล้วกดค้นหา">
                                <EmptyDataRowStyle CssClass="no-data" />
                                <Columns>
                                    <asp:BoundField DataField="RowNo" HeaderText="#"
                                        HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" ItemStyle-Width="40px" />

                                    <asp:ButtonField HeaderText="เพิ่ม" ButtonType="Button" Text="Add" ItemStyle-Width="70px"
                                        CommandName="AddItem"
                                        HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1"
                                        ControlStyle-CssClass="btn btn-success btn-sm" />

                                    <asp:TemplateField HeaderText="รายละเอียด" HeaderStyle-CssClass="headerGv1" ItemStyle-Width="70px"
                                        ItemStyle-CssClass="rowGv1" ItemStyle-HorizontalAlign="Center" ItemStyle-Wrap="false">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="btn_detailView" title="ดูรายละเอียด" runat="server"
                                                OnClick="btn_detailView_Click" CssClass="img_tool">
                                                <img src="images/eye-tracking.png" alt="" />
                                            </asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="รูป" HeaderStyle-CssClass="headerGv1" ItemStyle-Width="60px"
                                        ItemStyle-CssClass="rowGv1" ItemStyle-HorizontalAlign="Center">
                                        <ItemTemplate>
                                            <asp:HiddenField ID="hdtoolId" runat="server" Value='<%# Eval("tool_id") %>' />
                                            <asp:HiddenField ID="hdtoolSub" runat="server" Value='<%# Eval("sub_no") %>' />
                                            <asp:HiddenField ID="hdsystemId" runat="server" Value='<%# Eval("sys_status") %>' />
                                            <asp:HiddenField ID="hdmodelId" runat="server" Value='<%# Eval("model_id") %>' />
                                            <asp:HiddenField ID="hdlocationId" runat="server" Value='<%# Eval("location1_id") %>' />
                                            <asp:HiddenField ID="hdcellId" runat="server" Value='<%# Eval("location2_id") %>' />
                                            <asp:HiddenField ID="hdstationId" runat="server" Value='<%# Eval("station_id") %>' />
                                            <asp:LinkButton ID="btn_pic" title="ดูรูป" runat="server"
                                                OnClick="btn_pic_Click" CssClass="img_tool">
                                                <img src="images/photo-gallery.png" alt="" />
                                            </asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="สถานะ" HeaderStyle-CssClass="headerGv1" 
                                        ItemStyle-CssClass="rowGv1" ItemStyle-HorizontalAlign="Center">
                                        <ItemTemplate>
                                            <asp:HiddenField ID="hdstatusId" runat="server" Value='<%# Eval("status_id") %>' />
                                            <%# BuildStatusBadge(Eval("DisplayStatus"), Eval("ProcUrl")) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:BoundField DataField="sub_no" ItemStyle-Width="50px"
                                        HeaderText="No" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />
                                    <asp:BoundField DataField="FixAcces" ItemStyle-Width="110px"
                                        HeaderText="Fix Asset" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />
                                    <asp:BoundField DataField="control_no" ItemStyle-Width="130px"
                                        HeaderText="Control No" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />
                                    <asp:BoundField DataField="tool_no"
                                        HeaderText="Tool No / Model" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv_toolNo" />
                                    <asp:TemplateField HeaderText="Dept" HeaderStyle-CssClass="headerGv1"
                                        ItemStyle-CssClass="rowGv1" ItemStyle-Width="90px">
                                        <ItemTemplate>
                                            <%# FormatDeptDisplay(Eval("dept_code"), Eval("dept_code_real")) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="model_name"
                                        HeaderText="CPT Model" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />
                                    <asp:BoundField DataField="cell"
                                        HeaderText="Cell" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />
                                    <asp:BoundField DataField="station"
                                        HeaderText="Station" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />
                                </Columns>
                            </asp:GridView>
                        </div>

                    </div>

                </asp:Panel>

                <%--=====================================================--%>
                <%-- STEP 4 : รายการเปลี่ยนแปลง --%>
                <%--=====================================================--%>
                <div class="bw-card">

                    <div class="bw-card-head">
                        <asp:Literal ID="step_changelist" runat="server" Text="<span class='step-no'>4</span>" />
                        <div class="bw-card-title">
                            <h2>รายการเปลี่ยนแปลง
                                <asp:LinkButton ID="btnManual4" runat="server" CssClass="manual-btn"
                                    OnClientClick="return openManualStep(4);" ToolTip="คู่มือ Step 4">
                                    <span class="glyphicon glyphicon-book"></span>
                                </asp:LinkButton>
                            </h2>
                            <p>ระบุปลายทางในช่องสีเขียว — กด + เพื่อพิมพ์ค่าใหม่ที่ไม่มีในรายการ</p>
                        </div>
                        <span class="head-right">
                            <asp:Literal ID="request_remark" runat="server"
                                Text="<span class='bw-hint'>ถ้า ใช้งานที่ : Dept ไม่ใช่ส่วนงาน EPM ให้เลือก AAD (5505)</span>" />
                        </span>
                    </div>

                    <div class="bw-card-body">

                        <asp:Literal ID="usedept_remark" runat="server" Visible="false"
                            Text="<div class='bw-notice danger' style='margin:0 0 10px;'>Current Dept นี้อยู่นอก Div : EPM — Use : Dept ทุกรายการต้องเป็น Dept เดียวกัน</div>" />
              
                        <asp:Literal ID="litItemResultSummary" runat="server" />

                        <%-- ลำดับคอลัมน์เดิม (Columns[1] = Remove , Cells[0] = ป้ายคู่สลับ) --%>
                        <div class="tbl-wrap" style="max-height: 360px;">
                            <asp:GridView ID="gvChangeList" runat="server"
                                CssClass="data-table change-table fixed-header" GridLines="None" CellSpacing="0"
                                style="min-width:2000px;"
                                AutoGenerateColumns="false"
                                OnRowDataBound="gvChangeList_RowDataBound"
                                OnRowCommand="gvChangeList_RowCommand"
                                EmptyDataText="ยังไม่มีอุปกรณ์ในรายการ — ค้นหาแล้วกด Add ในขั้นตอนที่ 3">
                                <EmptyDataRowStyle CssClass="no-data" />
                                <Columns>
                                    <asp:BoundField DataField="RowNo" HeaderText="#"
                                        HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" ItemStyle-Width="65px" />

                                    <asp:TemplateField HeaderText="ลบ" ItemStyle-Width="80px"
                                        HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1">
                                        <ItemTemplate>
                                            <asp:Button ID="btnRemove" runat="server" Text="Remove"
                                                CssClass="btn btn-danger btn-sm"
                                                OnClientClick="return confirmRemoveSwap();"
                                                CommandName="RemoveItem"
                                                CommandArgument='<%# Eval("ToolId") + "|" + Eval("ToolSub") + "|" + Eval("SystemId") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="ผลอนุมัติ" ItemStyle-Width="160px" Visible="false"
                                        HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1">
                                        <ItemTemplate><asp:Literal ID="litApvResult" runat="server" /></ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="รายละเอียด" HeaderStyle-CssClass="headerGv1" ItemStyle-Width="70px"
                                        ItemStyle-CssClass="rowGv1" ItemStyle-HorizontalAlign="Center">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="btn_Detail" title="ดูรายละเอียด" runat="server"
                                                OnClick="btn_detailView_Click" CssClass="img_tool">
                                                <img src="images/eye-tracking.png" alt="" />
                                            </asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="รูป" HeaderStyle-CssClass="headerGv1" ItemStyle-Width="55px"
                                        ItemStyle-CssClass="rowGv1" ItemStyle-HorizontalAlign="Center">
                                        <ItemTemplate>
                                            <asp:HiddenField ID="hdtoolId" runat="server" Value='<%# Eval("ToolId") %>' />
                                            <asp:HiddenField ID="hdtoolSub" runat="server" Value='<%# Eval("ToolSub") %>' />
                                            <asp:HiddenField ID="hdsystemId" runat="server" Value='<%# Eval("SystemId") %>' />
                                            <asp:LinkButton ID="btn_Picture" title="ดูรูป" runat="server"
                                                OnClick="btn_pic_Click" CssClass="img_tool">
                                                <img src="images/photo-gallery.png" alt="" />
                                            </asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:BoundField DataField="FixAssetNo" ItemStyle-Width="110px"
                                        HeaderText="Fix Asset" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />
                                    <asp:BoundField DataField="ControlNo" ItemStyle-Width="120px"
                                        HeaderText="Control No" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />
                                    <asp:BoundField DataField="ToolNo"
                                        HeaderText="Tool No / Model" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv_toolNo" />
                                    <asp:BoundField DataField="ToolSub" ItemStyle-Width="45px"
                                        HeaderText="No" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />

                                    <%--═══ ปลายทาง (USE) ═══--%>
                                    <asp:TemplateField HeaderText="ใช้งานที่ : Dept"
                                        HeaderStyle-CssClass="hdr-use zone-divider" ItemStyle-CssClass="col-use zone-divider">
                                        <ItemTemplate>
                                            <div class="d-flex align-items-center" style="min-width:180px;">
                                                <asp:DropDownList ID="ddlDept" runat="server"
                                                    CssClass="form-control form-control-sm ddl_edit ddlSearch" Style="width:90%;"
                                                    AutoPostBack="true" OnSelectedIndexChanged="ddlDept_SelectedIndexChanged">
                                                </asp:DropDownList>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="ใช้งานที่ : Model" HeaderStyle-CssClass="hdr-use" ItemStyle-CssClass="col-use">
                                        <ItemTemplate>
                                            <div class="d-flex align-items-center" style="min-width:200px;">
                                                <asp:HiddenField ID="hfModelMode" runat="server" Value="ddl" />
                                                <asp:DropDownList ID="ddlModel" runat="server"
                                                    CssClass="form-control form-control-sm ddl_edit ddlSearch" Style="width:85%;">
                                                </asp:DropDownList>
                                                <asp:TextBox ID="txtModel" runat="server" placeHolder="Model (ใหม่)" MaxLength="50" 
                                                    CssClass="form-control form-control-sm ddl_edit" Style="width:85%; display:none;">
                                                </asp:TextBox>
                                                <button id="btnModelToggle" runat="server" type="button" class="btn btnToggle" title="สลับเป็นพิมพ์ Model ใหม่">+</button>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="ใช้งานที่ : Cell" HeaderStyle-CssClass="hdr-use" ItemStyle-CssClass="col-use">
                                        <ItemTemplate>
                                            <div class="d-flex align-items-center" style="min-width:200px;">
                                                <asp:HiddenField ID="hfCellMode" runat="server" Value="ddl" />
                                                <asp:DropDownList ID="ddlCell" runat="server"
                                                    CssClass="form-control form-control-sm ddl_edit ddlSearch" Style="width:85%;">
                                                </asp:DropDownList>
                                                <asp:TextBox ID="txtCell" runat="server" placeHolder="Cell (ใหม่)" MaxLength="50"
                                                    CssClass="form-control form-control-sm ddl_edit" Style="width:85%; display:none;">
                                                </asp:TextBox>
                                                <button id="btnCellToggle" runat="server" type="button" class="btn btnToggle" title="สลับเป็นพิมพ์ Cell ใหม่">+</button>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="ใช้งานที่ : Station" HeaderStyle-CssClass="hdr-use" ItemStyle-CssClass="col-use">
                                        <ItemTemplate>
                                            <div class="d-flex align-items-center" style="min-width:210px;">
                                                <asp:HiddenField ID="hfStationMode" runat="server" Value="ddl" />
                                                <asp:DropDownList ID="ddlStation" runat="server"
                                                    CssClass="form-control form-control-sm ddl_edit ddlSearch" Style="width:85%;">
                                                </asp:DropDownList>
                                                <asp:TextBox ID="txtStation" runat="server" placeHolder="Station (ใหม่)" MaxLength="50"
                                                    CssClass="form-control form-control-sm ddl_edit" Style="width:85%; display:none;">
                                                </asp:TextBox>
                                                <button id="btnStationToggle" runat="server" type="button" class="btn btnToggle" title="สลับเป็นพิมพ์ Station ใหม่">+</button>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%--═══ ปัจจุบัน (CURRENT) ═══--%>
                                    <asp:TemplateField HeaderText="ปัจจุบัน : Dept" ItemStyle-Width="110px"
                                        HeaderStyle-CssClass="hdr-current zone-divider" ItemStyle-CssClass="col-current zone-divider">
                                        <ItemTemplate>
                                            <%# FormatDeptDisplay(Eval("DeptCode"), Eval("DeptCodeReal")) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="ModelName" ItemStyle-Width="120px" HeaderText="ปัจจุบัน : Model"
                                        HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />
                                    <asp:BoundField DataField="CellName" ItemStyle-Width="120px" HeaderText="ปัจจุบัน : Cell"
                                        HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />
                                    <asp:BoundField DataField="StationName" ItemStyle-Width="120px" HeaderText="ปัจจุบัน : Station"
                                        HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>

                </div>

                <%--=====================================================--%>
                <%-- STEP 5 : เหตุผล --%>
                <%--=====================================================--%>
                <div class="bw-card">

                    <div class="bw-card-head">
                        <asp:Literal ID="step_comment" runat="server" Text="<span class='step-no'>5</span>" />
                        <div class="bw-card-title">
                            <h2>เหตุผลการแจ้งเปลี่ยนแปลง
                                <asp:LinkButton ID="btnManual5" runat="server" CssClass="manual-btn"
                                    OnClientClick="return openManualStep(5);" ToolTip="คู่มือ Step 5">
                                    <span class="glyphicon glyphicon-book"></span>
                                </asp:LinkButton>
                            </h2>
                            <p>เลือกเหตุผล และอธิบายเพิ่มเติม (ช่องสีเหลือง = บังคับกรอกอย่างน้อย 10 ตัวอักษร)</p>
                        </div>
                    </div>

                    <div class="bw-card-body">

                        <asp:HiddenField ID="hfReasonReq" runat="server" Value="" />

                        <div class="bw-fields" style="align-items:flex-start;">
                            <div class="bw-field" style="width:220px;">
                                <label for="<%= ddlReason_Change.ClientID %>">เหตุผล *</label>
                                <asp:DropDownList ID="ddlReason_Change" runat="server" CssClass="fld" Style="width:220px;"
                                    onchange="countChar();">
                                </asp:DropDownList>
                            </div>

                            <div class="bw-field" style="flex:1; min-width:280px;">
                                <label for="<%= txt_ChangeComment.ClientID %>">
                                    รายละเอียด
                                    <span id="reasonReqMark" style="display:none; color:#c93b3b; font-weight:700;">* จำเป็น (อย่างน้อย 10 ตัวอักษร)</span>
                                    <span class="char-count">(<b id="charCount">0</b> / 1000 ตัวอักษร)</span>
                                </label>
                                <asp:TextBox ID="txt_ChangeComment" runat="server"
                                    TextMode="MultiLine" Rows="3"
                                    CssClass="fld-area" oninput="countChar(this)"
                                    placeholder="เช่น ย้ายเครื่องไปใช้ที่ Cell ใหม่ตามแผนผลิต ..." />
                            </div>
                        </div>

                        <div id="borrow_note" runat="server">
                            <div class="bw-rules">
                                <strong>กรณีเกี่ยวกับคอมพิวเตอร์ ต้องติดต่อหน่วยงานที่รับผิดชอบ</strong>
                                <ol>
                                    <li>ลด Station, โยกย้าย หรือยกเลิกคอมพิวเตอร์ — ติดต่อ Eng System ทุกกรณี</li>
                                    <li>เพิ่มคอมพิวเตอร์ Tooling — ติดต่อ Eng System</li>
                                    <li>เพิ่มคอมพิวเตอร์ Special Project — ติดต่อ AAD IT Support</li>
                                </ol>
                            </div>
                        </div>

                    </div>

                </div>

                <%--=====================================================--%>
                <%-- STEP 6 : ผู้อนุมัติ --%>
                <%--=====================================================--%>
                <asp:Panel ID="pnlApproverFlow" runat="server" CssClass="bw-card card-approver">

                    <div class="bw-card-head">
                        <asp:Literal ID="step_approver" runat="server" Text="<span class='step-no'>6</span>" />
                        <div class="bw-card-title">
                            <h2><asp:Literal ID="litStep6Title" runat="server" Text="ผู้อนุมัติ (J3 Up)" />
                                <asp:LinkButton ID="btnManual6" runat="server" CssClass="manual-btn"
                                    OnClientClick="return openManualStep(6);" ToolTip="คู่มือ Step 6">
                                    <span class="glyphicon glyphicon-book"></span>
                                </asp:LinkButton>
                            </h2>
                            <p>ระบบกำหนดแผนกผู้อนุมัติให้อัตโนมัติ — เลือกอย่างน้อย 1 คนต่อแผนก</p>
                        </div>
                    </div>

                    <div class="bw-card-body">
                        <asp:HiddenField ID="hfFlowGenerated" runat="server" Value="0" />

                        <asp:Panel ID="pnlFlowNeedGen" runat="server" Visible="true">
                            <div class="apvflow-needgen">
                                เพิ่มอุปกรณ์ในขั้นตอนที่ 4 และเลือก <b>ใช้งานที่ : Dept</b> ให้ครบทุกรายการ
                                แล้วระบบจะแสดงช่องเลือกผู้อนุมัติที่นี่
                            </div>
                        </asp:Panel>

                        <asp:Panel ID="pnlFlowContent" runat="server" Visible="false" CssClass="apvflow-content">

                            <div id="j3Wrap" class="j3-wrap">

                                <%-- ===== Approver 1 ===== --%>
                                <div id="j3BoxL" class="j3-card">
                                    <div class="j3-card-head">
                                        <span class="j3-step">1</span>
                                        <div class="j3-card-title">
                                            <div class="j3-title"><asp:Literal ID="litJ3TitleL" runat="server" Text="Approver 1" /></div>
                                            <div class="j3-sub">แผนก <b><asp:Literal ID="litDeptL" runat="server" Text="-" /></b></div>
                                        </div>
                                        <span id="j3CountL" class="j3-count req">ต้องเลือกอย่างน้อย 1 คน</span>
                                    </div>

                                    <div class="j3-picker" id="j3PickerL">
                                        <div class="j3-inputbox" id="j3InputBoxL" onclick="j3FocusInput('L')">
                                            <span class="glyphicon glyphicon-search j3-search-ico"></span>
                                            <input type="text" id="j3SearchL" class="j3-input" autocomplete="off"
                                                placeholder="ค้นหาชื่อ / รหัส / ตำแหน่ง ..." oninput="j3Filter('L')"
                                                onkeydown="j3KeyDown(event,'L')" onfocus="j3Filter('L')" />
                                        </div>
                                        <div id="j3DropdownL" class="j3-dropdown" style="display:none;"></div>
                                    </div>

                                    <div id="j3ChipsL" class="j3-selected"></div>

                                    <asp:HiddenField ID="hfJ3SelectedL" runat="server" Value="" />
                                    <asp:HiddenField ID="hfJ3SourceL" runat="server" Value="[]" />
                                </div>

                                <%-- ===== Approver 2 (แสดงเมื่อมี 2 แผนก) ===== --%>
                                <div id="j3BoxR" class="j3-card j3-card-r" style="display:none;">
                                    <div class="j3-card-head">
                                        <span class="j3-step">2</span>
                                        <div class="j3-card-title">
                                            <div class="j3-title"><asp:Literal ID="litJ3TitleR" runat="server" Text="Approver 2" /></div>
                                            <div class="j3-sub">แผนก <b><asp:Literal ID="litDeptR" runat="server" Text="-" /></b></div>
                                        </div>
                                        <span id="j3CountR" class="j3-count req">ต้องเลือกอย่างน้อย 1 คน</span>
                                    </div>

                                    <div class="j3-picker" id="j3PickerR">
                                        <div class="j3-inputbox" id="j3InputBoxR" onclick="j3FocusInput('R')">
                                            <span class="glyphicon glyphicon-search j3-search-ico"></span>
                                            <input type="text" id="j3SearchR" class="j3-input" autocomplete="off"
                                                placeholder="ค้นหาชื่อ / รหัส / ตำแหน่ง ..." oninput="j3Filter('R')"
                                                onkeydown="j3KeyDown(event,'R')" onfocus="j3Filter('R')" />
                                        </div>
                                        <div id="j3DropdownR" class="j3-dropdown" style="display:none;"></div>
                                    </div>

                                    <div id="j3ChipsR" class="j3-selected"></div>

                                    <asp:HiddenField ID="hfJ3SelectedR" runat="server" Value="" />
                                    <asp:HiddenField ID="hfJ3SourceR" runat="server" Value="[]" />
                                </div>
                            </div>

                            <asp:Panel ID="Panel1" runat="server" Visible="false">
                                <div class="bw-notice danger">ไม่พบรายชื่อผู้อนุมัติสำหรับแผนกนี้ — กรุณาติดต่อ Admin</div>
                            </asp:Panel>

                            <asp:HiddenField ID="hfFlowDeptA1" runat="server" Value="" />
                            <asp:HiddenField ID="hfFlowDeptA2" runat="server" Value="" />

                        </asp:Panel>

                        <%-- ตารางผู้อนุมัติ (view / confirm / approve) --%>
                        <asp:Panel ID="pnlApproverTable" runat="server" Visible="false">
                            <asp:Literal ID="litApvProgress" runat="server" />

                            <div class="tbl-wrap apv-wrap">
                                <asp:GridView ID="gvApproverFlow" runat="server"
                                    CssClass="data-table apv-table" GridLines="None" CellSpacing="0"
                                    AutoGenerateColumns="false"
                                    OnRowDataBound="gvApproverFlow_RowDataBound"
                                    EmptyDataText="ไม่พบรายชื่อผู้อนุมัติ">
                                    <EmptyDataRowStyle CssClass="no-data" />
                                    <Columns>
                                        <asp:TemplateField HeaderText="ขั้น" ItemStyle-Width="120px">
                                            <ItemTemplate><asp:Literal ID="litStepBadge_view" runat="server" /></ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="ผู้อนุมัติ" ItemStyle-CssClass="apv-left">
                                            <ItemTemplate><asp:Literal ID="litPerson" runat="server" /></ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="ตำแหน่ง" ItemStyle-CssClass="apv-left">
                                            <ItemTemplate><asp:Literal ID="litPosition" runat="server" /></ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="ติดต่อ" ItemStyle-CssClass="apv-left">
                                            <ItemTemplate><asp:Literal ID="litContact" runat="server" /></ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="สถานะ" ItemStyle-Width="140px">
                                            <ItemTemplate><asp:Literal ID="litApvStatus" runat="server" /></ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="วันที่อนุมัติ" ItemStyle-Width="150px">
                                            <ItemTemplate><asp:Literal ID="litApvDate" runat="server" /></ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </asp:Panel>
                    </div>

                </asp:Panel>

                <%--======================================================================--%>
                <%-- ประวัติ Reject / Cancel --%>
                <%--======================================================================--%>
                <asp:Panel ID="pnl_rejectComment" runat="server" Visible="false" CssClass="bw-card">

                    <div class="bw-card-head">
                        <div class="bw-card-title">
                            <h2>ประวัติ Reject / Cancel</h2>
                            <p>เรียงจากล่าสุด</p>
                        </div>
                    </div>

                    <div class="bw-card-body">
                        <div class="tbl-wrap" style="max-height: 160px;">
                            <asp:GridView ID="gv_rejectHistory" runat="server"
                                CssClass="data-table change-table fixed-header" GridLines="None" CellSpacing="0"
                                AutoGenerateColumns="false"
                                EmptyDataText="ยังไม่มีประวัติการ Reject / Cancel">
                                <EmptyDataRowStyle CssClass="no-data" />
                                <Columns>
                                    <asp:BoundField DataField="Reject_Count" ItemStyle-Width="60px"
                                        HeaderText="ครั้งที่" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1" />
                                    <asp:BoundField DataField="Type" ItemStyle-Width="80px"
                                        HeaderText="ประเภท" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1" />
                                    <asp:BoundField DataField="Reject_by" ItemStyle-Width="160px"
                                        HeaderText="โดย" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1" />
                                    <asp:BoundField DataField="status_name" ItemStyle-Width="130px"
                                        HeaderText="ขั้นตอน" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1" />
                                    <asp:BoundField DataField="Reject_Date" ItemStyle-Width="120px"
                                        HeaderText="วันที่" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1"
                                        DataFormatString="{0:dd-MMM-yyyy}" />
                                    <asp:BoundField DataField="Reject_detail"
                                        HeaderText="เหตุผล" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv2" />
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>

                </asp:Panel>

                <%--======================================================================--%>
                <%-- ACTION BAR --%>
                <%--======================================================================--%>
                <div class="action-bar" id="ButtonBar" runat="server">

                    <div class="action-bar-left">
                        <div id="CancelButton" runat="server" Visible="false">
                            <span class="tooltip-wrap">
                                <asp:LinkButton ID="btnCancel" runat="server" CssClass="ab-btn ab-cancel"
                                    OnClientClick="openRejectModal('cancel','ระบุเหตุผลการยกเลิกคำขอ'); return false;">
                                    <span class="glyphicon glyphicon-ban-circle"></span> ยกเลิกคำขอ
                                </asp:LinkButton>
                                <span class="tooltip-box">ยกเลิกเอกสารทั้งฉบับ</span>
                            </span>
                        </div>
                    </div>

                    <div class="action-bar-right">
                        <div id="DraftButton" runat="server" Visible="true">
                            <span class="tooltip-wrap">
                                <asp:LinkButton ID="btnDraft" runat="server" CssClass="ab-btn ab-draft"
                                    OnClick="btnDraft_Click" OnClientClick="return confirmDraft();">
                                    <span class="glyphicon glyphicon-floppy-disk"></span> Save Draft
                                </asp:LinkButton>
                                <span class="tooltip-box">บันทึกไว้ก่อน ยังไม่ส่ง</span>
                            </span>
                        </div>

                        <div id="RejectButton" runat="server" Visible="false">
                            <span class="tooltip-wrap">
                                <asp:LinkButton ID="btnReject" runat="server" CssClass="ab-btn ab-reject"
                                    OnClientClick="openRejectModal('reject','ระบุเหตุผลการ Reject'); return false;">
                                    <span class="glyphicon glyphicon-remove"></span> Reject
                                </asp:LinkButton>
                                <span class="tooltip-box">ส่งกลับให้ผู้ขอแก้ไข</span>
                            </span>
                        </div>

                        <div id="SendButton" runat="server" Visible="true">
                            <span class="tooltip-wrap">
                                <asp:LinkButton ID="btnSend" runat="server" CssClass="ab-btn ab-send" 
                                    OnClick="btnSend_Click" OnClientClick="return checkHasItems();">
                                    <span class="glyphicon glyphicon-send"></span> Send Request
                                </asp:LinkButton>
                                <span class="tooltip-box">ตรวจสรุปและส่งให้ผู้อนุมัติ</span>
                            </span>
                        </div>

                        <div id="ConfirmButton" runat="server" Visible="false">
                            <span class="tooltip-wrap">
                                <asp:LinkButton ID="btnConfirm" runat="server" CssClass="ab-btn ab-confirm"
                                    OnClick="btnConfirm_Click">
                                    <span class="glyphicon glyphicon-ok-sign"></span> AAD Confirm
                                </asp:LinkButton>
                                <span class="tooltip-box">ยืนยันและรออัปเดตข้อมูล</span>
                            </span>
                        </div>

                        <div id="ApproveButton" runat="server" Visible="false">
                            <span class="tooltip-wrap">
                                <asp:LinkButton ID="btnApprove" runat="server" CssClass="ab-btn ab-send"
                                    OnClick="btnApprove_Click" OnClientClick="return Approve();">
                                    <span class="glyphicon glyphicon-ok-sign"></span> Approve
                                </asp:LinkButton>
                                <span class="tooltip-box">เลือกรายการที่อนุมัติ</span>
                            </span>
                        </div>
                    </div>

                </div>

                <%-- ===== Modal : รูปอุปกรณ์ ===== --%>
                <div class="modal fade bw-modal" id="modal_UploadPictureMain" tabindex="-1" role="dialog">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h4 class="modal-title">รูปอุปกรณ์</h4>
                                <button type="button" class="close" data-dismiss="modal" aria-label="ปิด">&times;</button>
                            </div>
                            <div class="modal-body" style="text-align:center;">
                                <asp:Label runat="server" ID="lb_testPic"></asp:Label>
                                <asp:Image runat="server" ID="imgMain" Width="100%" Style="border-radius:8px; max-width:100%;" />
                            </div>
                        </div>
                    </div>
                </div>

                <%-- ===== Modal : ยืนยันส่ง (สรุป + Approver) ===== --%>
                <asp:Panel ID="pnlSendModal" runat="server" CssClass="modal-overlay" Style="display:none;">

                    <div class="modal-box" style="width:95vw; max-height:90vh;">
                        <div class="modal-head">
                            <span>ตรวจสอบก่อนส่ง — แจ้งเปลี่ยนแปลง Fixed Asset / Calibration</span>
                            <button type="button" class="modal-x" onclick="closeSendModal();" aria-label="ปิด">&times;</button>
                        </div>

                        <div class="modal-content" style="min-height:60vh;">
                            <div class="msec-title">สรุปรายการ <small>ปัจจุบัน → ปลายทาง</small></div>
                            <div class="tbl-wrap" style="max-height:280px; margin-bottom:16px;">
                                <asp:GridView ID="gvSummary" runat="server"
                                    CssClass="data-table fixed-header" GridLines="None" CellSpacing="0"
                                    style="min-width:1500px;"
                                    AutoGenerateColumns="false" EmptyDataText="ไม่มีรายการ">
                                    <EmptyDataRowStyle CssClass="no-data" />
                                    <Columns>
                                        <asp:BoundField DataField="RowNo" HeaderText="#" ItemStyle-Width="35px"
                                            HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />
                                        <asp:BoundField DataField="FixAssetNo" HeaderText="Fix Asset" ItemStyle-Width="100px"
                                            HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />
                                        <asp:BoundField DataField="ControlNo" HeaderText="Control No" ItemStyle-Width="120px"
                                            HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />
                                        <asp:BoundField DataField="ToolNo" HeaderText="Tool No" ItemStyle-Width="200px"
                                            HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv_toolNo" />
                                        <asp:BoundField DataField="OldDept" HeaderText="ปัจจุบัน : Dept" ItemStyle-Width="90px"
                                            HeaderStyle-CssClass="hdr-current zone-divider" ItemStyle-CssClass="col-current zone-divider" />
                                        <asp:BoundField DataField="OldModel" HeaderText="ปัจจุบัน : Model" ItemStyle-Width="100px"
                                            HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />
                                        <asp:BoundField DataField="OldCell" HeaderText="ปัจจุบัน : Cell" ItemStyle-Width="90px"
                                            HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />
                                        <asp:BoundField DataField="OldStation" HeaderText="ปัจจุบัน : Station" ItemStyle-Width="100px"
                                            HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />
                                        <asp:BoundField DataField="NewDept" HeaderText="ปลายทาง : Dept" ItemStyle-Width="90px"
                                            HeaderStyle-CssClass="hdr-use zone-divider" ItemStyle-CssClass="col-use zone-divider" />
                                        <asp:BoundField DataField="NewModel" HeaderText="ปลายทาง : Model" ItemStyle-Width="100px"
                                            HeaderStyle-CssClass="hdr-use" ItemStyle-CssClass="col-use" />
                                        <asp:BoundField DataField="NewCell" HeaderText="ปลายทาง : Cell" ItemStyle-Width="90px"
                                            HeaderStyle-CssClass="hdr-use" ItemStyle-CssClass="col-use" />
                                        <asp:BoundField DataField="NewStation" HeaderText="ปลายทาง : Station" ItemStyle-Width="100px"
                                            HeaderStyle-CssClass="hdr-use" ItemStyle-CssClass="col-use" />
                                    </Columns>
                                </asp:GridView>
                            </div>

                            <div class="msec-title">ผู้อนุมัติ <small>จะได้รับอีเมลแจ้งตามลำดับขั้น</small></div>
                            <div class="tbl-wrap apv-wrap">
                                <asp:GridView ID="gvSendSummary" runat="server"
                                    CssClass="data-table apv-table" GridLines="None" CellSpacing="0"
                                    AutoGenerateColumns="false"
                                    OnRowDataBound="gvSendSummary_RowDataBound"
                                    EmptyDataText="ไม่พบรายชื่อผู้อนุมัติ">
                                    <EmptyDataRowStyle CssClass="no-data" />
                                    <Columns>
                                        <asp:TemplateField HeaderText="ขั้น" ItemStyle-Width="150px">
                                            <ItemTemplate><asp:Literal ID="litStepBadge" runat="server" /></ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="ผู้อนุมัติ" ItemStyle-CssClass="apv-left">
                                            <ItemTemplate><asp:Literal ID="litPerson" runat="server" /></ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="ตำแหน่ง" ItemStyle-CssClass="apv-left">
                                            <ItemTemplate><asp:Literal ID="litPosition" runat="server" /></ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="ติดต่อ" ItemStyle-CssClass="apv-left">
                                            <ItemTemplate><asp:Literal ID="litContact" runat="server" /></ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="เมื่อส่ง" ItemStyle-Width="170px">
                                            <ItemTemplate><asp:Literal ID="litSendState" runat="server" /></ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>

                            <asp:Panel ID="pnlNoApprover" runat="server" Visible="false">
                                <div class="bw-notice danger">ไม่พบรายชื่อผู้อนุมัติสำหรับแผนกนี้ — กรุณาติดต่อ Admin</div>
                            </asp:Panel>
                        </div>

                        <div class="modal-foot">
                            <asp:LinkButton ID="btnModalCancel" runat="server" CssClass="ab-btn ab-draft"
                                OnClientClick="closeSendModal(); return false;" Text="กลับไปแก้ไข" />
                            <asp:LinkButton ID="btnModalConfirm" runat="server" CssClass="ab-btn ab-send"
                                OnClick="btnModalConfirm_Click" OnClientClick="showLoading();"
                                Text="ยืนยันส่งรายการ" />
                        </div>
                    </div>

                </asp:Panel>

                <%-- ===== Modal : Approve (เลือกรายการ) ===== --%>
                <asp:Panel ID="pnlApproveModal" runat="server" CssClass="modal-overlay" Style="display:none;">

                    <div class="modal-box" style="width:95vw; max-height:90vh;">
                        <div class="modal-head">
                            <span><asp:Literal ID="litApvHead" runat="server" Text="อนุมัติการแจ้งเปลี่ยนแปลง" /> <span class="doc"><asp:Literal ID="litApvDoc" runat="server" /></span></span>
                            <span style="display:inline-flex; align-items:center; gap:8px; margin-left:auto;">
                                <a href="#" class="manual-btn" style="width:auto; padding:0 10px; gap:4px;"
                                   title="คู่มือการอนุมัติ" onclick="return openApvManual();">
                                    <span class="glyphicon glyphicon-book"></span> คู่มือ
                                </a>
                                <button type="button" class="modal-x" onclick="closeApproveModal();" aria-label="ปิด">&times;</button>
                            </span>
                        </div>

                        <div class="modal-content" style="min-height:40vh;">
                            <div class="msec-title">
                                เลือกรายการที่อนุมัติ
                                <small>รายการที่ไม่เลือก = Reject และต้องระบุเหตุผล</small>
                                <span id="apvStepBadge" style="margin-left:auto;"></span>
                            </div>

                            <asp:Literal ID="litApvNotice" runat="server" />

                            <div id="apvMismatchNotice" class="apv-mismatch-notice" style="display:none;">
                                <b>⚠️ รายการที่เลือกไม่ตรงกับ <asp:Literal ID="litApvMismatchRef" runat="server" Text="Approver 1" /></b>
                                เมื่อกดยืนยัน เอกสารจะถูก <b>Reject กลับไปยังผู้ร้องขอ (Draft)</b> เพื่อให้จัดทำข้อมูล Request ใหม่
                                — ต้องระบุเหตุผลอย่างน้อย 10 ตัวอักษร
                            </div>

                            <div class="tbl-wrap" style="max-height:340px; margin-bottom:14px;">
                                <asp:GridView ID="gvApprove" runat="server"
                                    CssClass="data-table fixed-header" GridLines="None" CellSpacing="0"
                                    style="min-width:1500px;"
                                    AutoGenerateColumns="false" DataKeyNames="RowKey"
                                    EmptyDataText="ไม่มีรายการ" OnRowDataBound="gvApprove_RowDataBound">
                                    <EmptyDataRowStyle CssClass="no-data" />
                                    <Columns>
                                        <asp:TemplateField ItemStyle-Width="46px" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1">
                                            <HeaderTemplate>
                                                <input type="checkbox" id="apvCheckAll" onclick="apvToggleAll(this)" checked title="เลือกทั้งหมด" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="cbApv" runat="server" CssClass="apv-cb" />
                                                <asp:HiddenField ID="hfRowKey" runat="server" Value='<%# Eval("RowKey") %>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="รอบก่อน" ItemStyle-Width="130px" Visible="false"
                                            HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1">
                                            <ItemTemplate><asp:Literal ID="litPrev" runat="server" /></ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="สถานะ" ItemStyle-Width="150px"
                                            HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1">
                                            <ItemTemplate><asp:Literal ID="litRowState" runat="server" /></ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:BoundField DataField="RowNo" HeaderText="#" ItemStyle-Width="35px"
                                            HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />
                                        <asp:BoundField DataField="FixAssetNo" HeaderText="Fix Asset" ItemStyle-Width="100px"
                                            HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />
                                        <asp:BoundField DataField="ControlNo" HeaderText="Control No" ItemStyle-Width="120px"
                                            HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />
                                        <asp:BoundField DataField="ToolNo" HeaderText="Tool No" ItemStyle-Width="180px"
                                            HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv_toolNo" />

                                        <asp:BoundField DataField="OldDept" HeaderText="ปัจจุบัน : Dept" ItemStyle-Width="90px"
                                            HeaderStyle-CssClass="hdr-current zone-divider" ItemStyle-CssClass="col-current zone-divider" />
                                        <asp:BoundField DataField="OldModel" HeaderText="ปัจจุบัน : Model" ItemStyle-Width="100px"
                                            HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />
                                        <asp:BoundField DataField="OldCell" HeaderText="ปัจจุบัน : Cell" ItemStyle-Width="90px"
                                            HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />
                                        <asp:BoundField DataField="OldStation" HeaderText="ปัจจุบัน : Station" ItemStyle-Width="100px"
                                            HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />

                                        <asp:BoundField DataField="NewDept" HeaderText="ปลายทาง : Dept" ItemStyle-Width="90px"
                                            HeaderStyle-CssClass="hdr-use zone-divider" ItemStyle-CssClass="col-use zone-divider" />
                                        <asp:BoundField DataField="NewModel" HeaderText="ปลายทาง : Model" ItemStyle-Width="100px"
                                            HeaderStyle-CssClass="hdr-use" ItemStyle-CssClass="col-use" />
                                        <asp:BoundField DataField="NewCell" HeaderText="ปลายทาง : Cell" ItemStyle-Width="90px"
                                            HeaderStyle-CssClass="hdr-use" ItemStyle-CssClass="col-use" />
                                        <asp:BoundField DataField="NewStation" HeaderText="ปลายทาง : Station" ItemStyle-Width="100px"
                                            HeaderStyle-CssClass="hdr-use" ItemStyle-CssClass="col-use" />
                                    </Columns>
                                </asp:GridView>
                            </div>

                            <div class="bw-field" id="apvCommentWrap">
                                <label for="<%= txtApvComment.ClientID %>">
                                    เหตุผล (กรณีไม่อนุมัติบางรายการ)
                                    <span id="apvCommentReq" style="color:#c93b3b; display:none;"> * จำเป็น</span>
                                </label>
                                <asp:TextBox ID="txtApvComment" runat="server" TextMode="MultiLine" Rows="3"
                                    CssClass="fld-area" placeholder="ระบุเหตุผลกรณีไม่อนุมัติบางรายการ..." />
                            </div>
                        </div>

                        <div class="modal-foot">
                            <asp:LinkButton ID="btnApvCancel" runat="server" CssClass="ab-btn ab-draft"
                                OnClientClick="closeApproveModal(); return false;" Text="ยกเลิก" />
                            <asp:LinkButton ID="btnApvConfirm" runat="server" CssClass="ab-btn ab-send"
                                OnClick="btnApproveConfirm_Click" OnClientClick="return apvValidate();"
                                Text="ยืนยันอนุมัติ" />
                        </div>
                    </div>

                </asp:Panel>

                <%-- ===== Modal : Reject / Cancel ===== --%>
                <asp:Panel ID="pnlRejectModal" runat="server" CssClass="modal-overlay" Style="display:none;">

                    <asp:HiddenField ID="hfRejectMode" runat="server" Value="" />

                    <div class="modal-box" style="width:560px;">
                        <div class="modal-head">
                            <span><asp:Literal ID="litRejectTitle" runat="server" Text="ระบุเหตุผล" /></span>
                            <button type="button" class="modal-x" onclick="closeRejectModal();" aria-label="ปิด">&times;</button>
                        </div>

                        <div class="modal-content">
                            <div class="bw-field">
                                <label for="<%= txt_rejectComment.ClientID %>">
                                    เหตุผล *
                                    <span class="char-count">(<b id="rejectCharCount" style="color:#c93b3b;">0</b> / 1000 — อย่างน้อย 10)</span>
                                </label>
                                <asp:TextBox ID="txt_rejectComment" runat="server"
                                    TextMode="MultiLine" Rows="5" MaxLength="1000"
                                    CssClass="fld-area" onkeyup="countRejectChar(this)"
                                    Style="min-height:110px;"
                                    placeholder="ผู้ขอจะเห็นข้อความนี้ในประวัติ Reject / Cancel" />
                            </div>
                        </div>

                        <div class="modal-foot">
                            <asp:LinkButton ID="btnRejectModalCancel" runat="server" CssClass="ab-btn ab-draft"
                                OnClientClick="closeRejectModal(); return false;" Text="ยกเลิก" />
                            <asp:LinkButton ID="btnRejectModalConfirm" runat="server" CssClass="ab-btn ab-reject"
                                OnClick="btnRejectModalConfirm_Click" OnClientClick="return validateRejectComment();"
                                Text="ยืนยัน" />
                        </div>
                    </div>

                </asp:Panel>

                <%-- ===== Modal : ยืนยัน AAD Confirm ===== --%>
                <asp:Panel ID="pnlConfirmModal" runat="server" CssClass="modal-overlay" Style="display:none;">
                    <div class="modal-box" style="width:95vw; max-height:90vh;">
                        <div class="modal-head">
                            <span>ยืนยัน AAD Confirm <span class="doc"><asp:Literal ID="litCfmDoc" runat="server" /></span></span>
                            <span style="display:inline-flex; align-items:center; gap:8px; margin-left:auto;">
                                <a href="#" class="manual-btn" style="width:auto; padding:0 10px; gap:4px;"
                                   title="คู่มือ AAD Confirm" onclick="return openApvManual();">
                                    <span class="glyphicon glyphicon-book"></span> คู่มือ
                                </a>
                                <button type="button" class="modal-x" onclick="closeConfirmModal();" aria-label="ปิด">&times;</button>
                            </span>
                        </div>

                        <div class="modal-content" style="min-height:45vh;">
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

                            <div class="msec-title">สรุปรายการ <small>ปัจจุบัน → ปลายทาง</small></div>

                            <div class="tbl-wrap" style="max-height:440px;">
                                <asp:GridView ID="gvConfirmSummary" runat="server"
                                    CssClass="data-table fixed-header" GridLines="None" CellSpacing="0"
                                    style="min-width:1800px;" AutoGenerateColumns="false"
                                    OnRowDataBound="gvConfirmSummary_RowDataBound" EmptyDataText="ไม่มีรายการ">
                                    <EmptyDataRowStyle CssClass="no-data" />
                                    <Columns>
                                        <asp:BoundField DataField="RowNo" HeaderText="#" ItemStyle-Width="35px" />
                                        <asp:TemplateField HeaderText="ผล" ItemStyle-Width="110px">
                                            <ItemTemplate><asp:Literal ID="litCfmResult" runat="server" /></ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:BoundField DataField="FixAssetNo" HeaderText="Fix Asset" ItemStyle-Width="100px" />
                                        <asp:BoundField DataField="ControlNo" HeaderText="Control No" ItemStyle-Width="110px" />
                                        <asp:BoundField DataField="ToolNo" HeaderText="Tool No" ItemStyle-CssClass="rowGv_toolNo" />

                                        <%--═══ AAD : เลือกเพิ่มเติม (หน้า ปลายทาง : Dept) ═══--%>
                                        <asp:TemplateField HeaderText="Status Use *"
                                            HeaderStyle-CssClass="hdr-aad zone-divider" ItemStyle-CssClass="col-aad zone-divider">
                                            <ItemTemplate>
                                                <asp:HiddenField ID="hfCfmKey" runat="server" Value='<%# Eval("RowKey") %>' />
                                                <asp:DropDownList ID="ddlStatusUse" runat="server" CssClass="form-control form-control-sm ddl_edit" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="ผู้รับผิดชอบ *"
                                            HeaderStyle-CssClass="hdr-aad" ItemStyle-CssClass="col-aad">
                                            <ItemTemplate>
                                                <asp:DropDownList ID="ddlResponsible" runat="server" CssClass="form-control form-control-sm ddl_edit" />
                                                <asp:Literal ID="litRuleNote" runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:BoundField DataField="NewDept" HeaderText="ปลายทาง : Dept"
                                            HeaderStyle-CssClass="hdr-use zone-divider" ItemStyle-CssClass="col-use zone-divider" />
                                        <asp:BoundField DataField="NewModel" HeaderText="ปลายทาง : Model"
                                            HeaderStyle-CssClass="hdr-use" ItemStyle-CssClass="col-use" />
                                        <asp:BoundField DataField="NewCell" HeaderText="ปลายทาง : Cell"
                                            HeaderStyle-CssClass="hdr-use" ItemStyle-CssClass="col-use" />
                                        <asp:BoundField DataField="NewStation" HeaderText="ปลายทาง : Station"
                                            HeaderStyle-CssClass="hdr-use" ItemStyle-CssClass="col-use" />

                                        <asp:BoundField DataField="OldDept" HeaderText="ปัจจุบัน : Dept"
                                            HeaderStyle-CssClass="hdr-current zone-divider" ItemStyle-CssClass="col-current zone-divider" />
                                        <asp:BoundField DataField="OldModel" HeaderText="ปัจจุบัน : Model"
                                            HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />
                                        <asp:BoundField DataField="OldCell" HeaderText="ปัจจุบัน : Cell"
                                            HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />
                                        <asp:BoundField DataField="OldStation" HeaderText="ปัจจุบัน : Station"
                                            HeaderStyle-CssClass="hdr-current" ItemStyle-CssClass="col-current" />
                                    </Columns>
                                </asp:GridView>
                            </div>

                            <div class="bw-notice" style="margin-top:10px;">
                                เมื่อยืนยัน เอกสารจะเปลี่ยนเป็น <b>Wait Update</b> และระบบจะอัปเดต Location เฉพาะรายการที่อนุมัติ
                                — รายการที่ไม่อนุมัติ (แถวสีแดง) จะไม่ถูกอัปเดต
                            </div>
                        </div>

                        <div class="modal-foot">
                            <asp:LinkButton ID="btnConfirmModalCancel" runat="server" CssClass="ab-btn ab-draft"
                                OnClientClick="closeConfirmModal(); return false;" Text="ยกเลิก" />
                            <asp:LinkButton ID="btnConfirmModalOk" runat="server" CssClass="ab-btn ab-confirm"
                                OnClick="btnConfirmModalOk_Click" OnClientClick="return cfmConfirm();"
                                Text="ยืนยัน AAD Confirm" />
                        </div>
                    </div>
                </asp:Panel>

                <%-- ===== Dialog : ยืนยันเปลี่ยนรูปแบบ ===== --%>
                <div id="modeConfirmModal" class="mode-confirm-overlay" style="display:none;">
                    <div class="mode-confirm-box">
                        <div class="mode-confirm-icon">⚠️</div>
                        <div class="mode-confirm-title">เปลี่ยนรูปแบบการแจ้งเปลี่ยนแปลง ?</div>
                        <div class="mode-confirm-text">
                            รายการใน Step 4 และรายชื่อผู้อนุมัติใน Step 6 <b>จะถูกล้างทั้งหมด</b><br />
                            และต้องค้นหาเพิ่มใหม่
                        </div>
                        <div class="mode-confirm-actions">
                            <button type="button" class="btn-mc btn-mc-cancel" onclick="cancelModeConfirm()">ยกเลิก</button>
                            <button type="button" class="btn-mc btn-mc-ok" onclick="confirmToggle()">ล้างและเปลี่ยน</button>
                        </div>
                    </div>
                </div>

                <%-- ===== Dialog : แผนกผู้อนุมัติเปลี่ยน ===== --%>
                <div id="apvChangeConfirm" class="mode-confirm-overlay" style="display:none;">
                    <div class="mode-confirm-box">
                        <div class="mode-confirm-icon">⚠️</div>
                        <div class="mode-confirm-title">แผนกผู้อนุมัติเปลี่ยน</div>
                        <div class="mode-confirm-text">
                            การเปลี่ยน รหัสพนักงาน / Dept ปัจจุบัน / Dept ปลายทาง ทำให้แผนกผู้อนุมัติเปลี่ยน<br />
                            ระบบจะ<b>ล้างรายชื่อผู้อนุมัติที่เลือกไว้</b> — กรุณาเลือกใหม่ใน Step 6
                        </div>
                        <div class="mode-confirm-actions">
                            <asp:LinkButton ID="btnApvChangeOk" runat="server" CssClass="btn-mc btn-mc-ok"
                                OnClick="btnApvChangeOk_Click" OnClientClick="closeApvChangeConfirm(); showLoading();"
                                Text="ตกลง" />
                        </div>
                    </div>
                </div>

            </div>

        </ContentTemplate>
        <Triggers>
        </Triggers>
    </asp:UpdatePanel>

    <%-- ===== Modal HRMS (นอก UP1) ===== --%>
    <div class="bw" style="padding:0;">

        <div class="modal fade bw-modal" id="hrmsCheckModal" tabindex="-1" role="dialog" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">ข้อมูลพนักงาน (HRMS)</h4>
                        <button type="button" class="close" data-dismiss="modal" aria-label="ปิด">&times;</button>
                    </div>
                    <div class="modal-body">
                        <table class="data-table" style="width:100%;" id="hrmsCheckTable">
                            <thead><tr><th style="width:40%;">Column</th><th>Value</th></tr></thead>
                            <tbody id="hrmsCheckBody"></tbody>
                        </table>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="ab-btn ab-draft" data-dismiss="modal">ปิด</button>
                    </div>
                </div>
            </div>
        </div>

        <%-- ===== Modal : วิธีใช้งาน (Read Me) ===== --%>
        <div class="modal fade bw-modal" id="modal_ReadMe" tabindex="-1" role="dialog" aria-hidden="true">
            <div class="modal-dialog rm-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">
                            <span class="glyphicon glyphicon-question-sign" style="color:#1f6feb; top:2px;"></span>
                            วิธีใช้งาน — แจ้งเปลี่ยนแปลง Fixed Asset / Calibration
                        </h4>
                        <button type="button" class="close" data-dismiss="modal" aria-label="ปิด">&times;</button>
                    </div>

                    <div class="rm-tabs">
                        <button type="button" class="rm-tab active" data-pane="rmSteps"  onclick="rmTab(this,'rmSteps')">ขั้นตอน</button>
                        <button type="button" class="rm-tab"        data-pane="rmMode"   onclick="rmTab(this,'rmMode')">รูปแบบการแจ้ง</button>
                        <button type="button" class="rm-tab"        data-pane="rmFlow"   onclick="rmTab(this,'rmFlow')">การอนุมัติ</button>
                        <button type="button" class="rm-tab"        data-pane="rmLegend" onclick="rmTab(this,'rmLegend')">สถานะ / สัญลักษณ์</button>
                    </div>

                    <div class="modal-body rm-body">

                        <%-- ── TAB 1 : ขั้นตอน ── --%>
                        <div id="rmSteps" class="rm-pane active">
                            <div class="rm-step">
                                <span class="step-no">1</span>
                                <div class="rm-step-text">
                                    <b>ผู้ร้องขอ</b>
                                    <p>กรอกรหัสพนักงาน 6 หลัก ระบบดึงชื่อ ตำแหน่ง แผนกให้อัตโนมัติ และกรอกเบอร์ติดต่อ 4 หลัก</p>
                                </div>
                                <a href="#" class="manual-btn" onclick="return openManualStep(1);" title="คู่มือ Step 1"><span class="glyphicon glyphicon-book"></span></a>
                            </div>
                            <div class="rm-step">
                                <span class="step-no">2</span>
                                <div class="rm-step-text">
                                    <b>รูปแบบการแจ้ง</b>
                                    <p>เลือก <i>แจ้งเปลี่ยน Location</i> หรือ <i>แจ้งสลับ Tool / Equipment</i> — การสลับรูปแบบจะล้างรายการใน Step 4 ทั้งหมด</p>
                                </div>
                                <a href="#" class="manual-btn" onclick="return openManualStep(2);" title="คู่มือ Step 2"><span class="glyphicon glyphicon-book"></span></a>
                            </div>
                            <div class="rm-step">
                                <span class="step-no">3</span>
                                <div class="rm-step-text">
                                    <b>ค้นหาและเพิ่มอุปกรณ์</b>
                                    <p>ค้นหาด้วย Fix Asset / Control No / Tool No แล้วกด <span class="rm-kbd rm-kbd-green">Add</span>
                                       อุปกรณ์ที่ติดกระบวนการอื่นอยู่ (ป้ายสีแดง) จะเพิ่มไม่ได้ — คลิกป้ายเพื่อเปิดดูเอกสารนั้น</p>
                                </div>
                                <a href="#" class="manual-btn" onclick="return openManualStep(3);" title="คู่มือ Step 3"><span class="glyphicon glyphicon-book"></span></a>
                            </div>
                            <div class="rm-step">
                                <span class="step-no">4</span>
                                <div class="rm-step-text">
                                    <b>รายการเปลี่ยนแปลง</b>
                                    <p>ระบุปลายทางในช่องสีเขียว (<i>ใช้งานที่ : Dept / Model / Cell / Station</i>)
                                       ถ้าไม่มีค่าในรายการ กด <span class="rm-kbd">+</span> เพื่อพิมพ์ค่าใหม่
                                       — ถ้า Dept ปลายทางไม่ใช่ส่วนงาน EPM ให้เลือก <b>AAD (5505)</b></p>
                                </div>
                                <a href="#" class="manual-btn" onclick="return openManualStep(4);" title="คู่มือ Step 4"><span class="glyphicon glyphicon-book"></span></a>
                            </div>
                            <div class="rm-step">
                                <span class="step-no">5</span>
                                <div class="rm-step-text">
                                    <b>เหตุผล</b>
                                    <p>เลือกเหตุผล — บางเหตุผลบังคับให้กรอกรายละเอียดอย่างน้อย 10 ตัวอักษร</p>
                                </div>
                                <a href="#" class="manual-btn" onclick="return openManualStep(5);" title="คู่มือ Step 5"><span class="glyphicon glyphicon-book"></span></a>
                            </div>
                            <div class="rm-step">
                                <span class="step-no">6</span>
                                <div class="rm-step-text">
                                    <b>ผู้อนุมัติ (J3 Up)</b>
                                    <p>ระบบกำหนดแผนกผู้อนุมัติให้อัตโนมัติเมื่อเลือก <i>ใช้งานที่ : Dept</i> ครบทุกรายการ
                                       — เลือกผู้อนุมัติอย่างน้อย 1 คนต่อแผนก (คนใดคนหนึ่งอนุมัติได้)</p>
                                </div>
                                <a href="#" class="manual-btn" onclick="return openManualStep(6);" title="คู่มือ Step 6"><span class="glyphicon glyphicon-book"></span></a>
                            </div>

                            <div class="rm-box">
                                <b>ปุ่มด้านล่างขวา</b>
                                <div class="rm-btnrow">
                                    <span class="rm-kbd">Save Draft</span> บันทึกไว้ก่อน ยังไม่ส่ง (แก้ไขต่อได้)
                                </div>
                                <div class="rm-btnrow">
                                    <span class="rm-kbd rm-kbd-green">Send Request</span> ตรวจสรุปรายการ + ผู้อนุมัติ แล้วส่ง — ระบบส่งอีเมลแจ้ง Approver 1
                                </div>
                                <div class="rm-btnrow">
                                    <span class="rm-kbd rm-kbd-red-o">ยกเลิกคำขอ</span> ยกเลิกทั้งฉบับ ต้องระบุเหตุผลอย่างน้อย 10 ตัวอักษร
                                </div>
                            </div>
                        </div>

                        <%-- ── TAB 2 : รูปแบบการแจ้ง ── --%>
                        <div id="rmMode" class="rm-pane" style="background:#fff;">
                            <div class="rm-compare">
                                <div class="rm-card" style="background: #e4faee;">
                                    <div class="rm-card-title"><span class="glyphicon glyphicon-random"></span> แจ้งเปลี่ยน Location</div>
                                    <ul>
                                        <li>ย้ายสถานที่ใช้งานของอุปกรณ์</li>
                                        <li>ไม่จำกัดจำนวนรายการ</li>
                                        <li><b>ปัจจุบัน : Dept</b> ทุกรายการต้องเป็นกลุ่มเดียวกัน</li>
                                        <li><b>ใช้งานที่ : Dept</b> ทุกรายการต้องเป็นกลุ่มเดียวกัน</li>
                                        <li>ระบุปลายทางเองทุกช่อง</li>
                                    </ul>
                                </div>
                                <div class="rm-card" style="background: #cde1ff;">
                                    <div class="rm-card-title"><span class="glyphicon glyphicon-transfer"></span> แจ้งสลับ Tool / Equipment</div>
                                    <ul>
                                        <li>สลับอุปกรณ์ระหว่าง 2 แผนก</li>
                                        <li>จับคู่ทีละ 2 รายการตามลำดับที่ Add (1-2, 3-4, ...)</li>
                                        <li>คู่เป็น Dept เดียวกันหรือต่างกันได้ แต่ทั้งเอกสารมีได้ <b>ไม่เกิน 2 Dept</b></li>
                                        <li>ระบบใส่ปลายทางให้อัตโนมัติ (สลับตำแหน่งกัน)</li>
                                        <li>กด Remove หนึ่งรายการ → ลบทั้งคู่</li>
                                    </ul>
                                </div>
                            </div>
                            <div class="rm-note">
                                <b>กลุ่ม Dept</b> เทียบจาก 3 หลักแรกของรหัสแผนก เช่น 5331 กับ 5332 ถือเป็นกลุ่มเดียวกัน
                                <%--— ยกเว้น <b>5505</b> ต้องตรงทั้ง 4 หลัก--%>
                            </div>
                        </div>

                        <%-- ── TAB 3 : การอนุมัติ ── --%>
                        <div id="rmFlow" class="rm-pane" style="background:#fff;">
                            <div style="display:flex; flex-wrap:wrap; gap:8px; justify-content:flex-end; margin-bottom:10px;">
                                <a href="#" class="manual-btn" style="width:auto; padding:0 12px; gap:6px; height:30px;"
                                   onclick="return openManualFile('Approver1');">
                                    <span class="glyphicon glyphicon-book"></span> คู่มือ Approver 1 (PDF)
                                </a>
                                <a href="#" class="manual-btn" style="width:auto; padding:0 12px; gap:6px; height:30px;"
                                   onclick="return openManualFile('Approver2');">
                                    <span class="glyphicon glyphicon-book"></span> คู่มือ Approver 2 (PDF)
                                </a>
                                <a href="#" class="manual-btn" style="width:auto; padding:0 12px; gap:6px; height:30px;"
                                   onclick="return openManualFile('Confirm');">
                                    <span class="glyphicon glyphicon-book"></span> คู่มือ AAD Confirm (PDF)
                                </a>
                            </div>
                            <div class="rm-flow">
                                <div class="rm-flow-node"><span class="status-badge status-draft">Draft</span><small>ผู้ร้องขอ</small></div>
                                <div class="rm-flow-arrow">&#8250;</div>
                                <div class="rm-flow-node"><span class="status-badge status-approve">Approver 1</span><small>J3 Up แผนกที่ 1</small></div>
                                <div class="rm-flow-arrow">&#8250;</div>
                                <div class="rm-flow-node"><span class="status-badge status-approve">Approver 2</span><small>เฉพาะกรณี 2 แผนก</small></div>
                                <div class="rm-flow-arrow">&#8250;</div>
                                <div class="rm-flow-node"><span class="status-badge status-confirm">AAD Confirm</span><small>AAD ตรวจสอบ</small></div>
                                <div class="rm-flow-arrow">&#8250;</div>
                                <div class="rm-flow-node"><span class="status-badge status-completed">Completed</span><small>อัปเดตข้อมูลแล้ว</small></div>
                            </div>

                            <div class="rm-box" style="background: #fff; border: 1px solid #4388fc; margin-bottom: 5px;}">
                                <b>การอนุมัติแบบเลือกรายการ</b>
                                <ol>
                                    <li>ผู้อนุมัติติ๊กเลือกรายการที่อนุมัติ — รายการที่ไม่เลือกถือว่า Reject และต้องระบุเหตุผล</li>
                                    <li>กรณี 2 แผนก : Approver 2 จะเห็นรายการตามที่ Approver 1 เลือกไว้เป็นค่าเริ่มต้น</li>
                                    <li>ถ้า Approver 2 เลือก<b>ไม่ตรง</b>กับ Approver 1 → ปุ่มจะเปลี่ยนเป็น <b>Reject</b> และเอกสารกลับเป็น Draft ให้ผู้ร้องขอจัดทำ Request ใหม่ (อีเมลแจ้งผู้ร้องขอ + CC Approver 1, 2)</li>
                                    <li>โหมด <b>แจ้งสลับ</b> : ต้องอนุมัติ / ไม่อนุมัติ <b>ทั้งคู่</b> — ติ๊กตัวใดตัวหนึ่ง อีกตัวในคู่จะเปลี่ยนตาม</li>
                                    <li>ถ้าเลือกตรงกัน → อนุมัติสมบูรณ์ ส่งต่อ AAD Confirm (รายการที่ Reject จะไม่ถูกอัปเดต Location)</li>
                                </ol>
                            </div>

                            <div class="rm-box" style="background: #fff; border: 1px solid #4388fc; margin-bottom: 5px;}">
                                <b>ระบบกำหนดแผนกผู้อนุมัติอย่างไร</b>
                                <ol>
                                    <li>ถ้า Current หรือ Use เป็น <b>5505</b> ฝั่งใดฝั่งหนึ่ง → อนุมัติโดยแผนกอีกฝั่ง (1 แผนก)</li>
                                    <li>ถ้าแผนกผู้ร้องขอตรงกับ Current หรือ Use → อนุมัติโดยแผนกฝั่งที่ไม่ใช่ของผู้ร้องขอ</li>
                                    <li>ถ้าไม่ตรงทั้งสองฝั่ง → 2 แผนก : Approver 1 = Current Dept, Approver 2 = Use Dept</li>
                                    <li>ถ้า Current กับ Use เป็นกลุ่มเดียวกัน → เหลือ 1 แผนก</li>
                                </ol>
                                <div class="rm-note" style="margin-top:8px;">
                                    เมื่อเปลี่ยน รหัสพนักงาน / Dept ปัจจุบัน / Dept ปลายทาง จนแผนกผู้อนุมัติเปลี่ยน
                                    ระบบจะล้างรายชื่อที่เลือกไว้ใน Step 6 และต้องเลือกใหม่
                                </div>
                            </div>

                            <div class="rm-box">
                                <b>Reject</b>
                                <p style="margin:4px 0 0;">เอกสารจะกลับเป็น Draft ให้ผู้ร้องขอแก้ไขและส่งใหม่ — เหตุผลแสดงในตาราง "ประวัติ Reject / Cancel"</p>
                            </div>
                        </div>

                        <%-- ── TAB 4 : สถานะ / สัญลักษณ์ ── --%>
                        <div id="rmLegend" class="rm-pane" style="background:#fff;">
                            <table class="rm-legend" style="border: 1px solid #91c4a9;">
                                <tr><td><span class="status-badge status-draft">Wait Issue</span></td><td>Draft — ยังไม่ส่ง แก้ไขได้</td></tr>
                                <tr><td><span class="status-badge status-approve">J3 Up Approve</span></td><td>รอผู้อนุมัติ (Approver 1 / 2)</td></tr>
                                <tr><td><span class="status-badge status-confirm">AAD Confirm</span></td><td>อนุมัติครบ รอ AAD ยืนยัน</td></tr>
                                <tr><td><span class="status-badge status-borrow">Wait Update</span></td><td>AAD ยืนยันแล้ว รอระบบอัปเดต Location</td></tr>
                                <tr><td><span class="status-badge status-completed">Completed</span></td><td>เสร็จสมบูรณ์</td></tr>
                                <tr><td><span class="status-badge status-cancel">Cancel</span></td><td>ยกเลิกเอกสาร</td></tr>
                            </table>

                            <table class="rm-legend" style="border: 1px solid #6b7ae7;">
                                <tr><td><span class="badge badge-normal">Normal</span></td><td>อุปกรณ์พร้อมใช้งาน เพิ่มได้</td></tr>
                                <tr><td><span class="badge badge-spare">Spare : ...</span></td><td>อุปกรณ์สถานะ Spare เพิ่มได้</td></tr>
                                <tr><td><span class="badge badge-borrowing">AAD1 : ยืม</span></td><td>ติดกระบวนการอื่นอยู่ (ยืม / แจ้งเปลี่ยน / เบิก / จัดเก็บ) เพิ่มไม่ได้</td></tr>
                            </table>

                            <table class="rm-legend" style="border: 1px solid #d58545">
                                <tr><td><span class="rm-swatch" style="background:#eef8f1; border-color:#b6e0c4;"></span></td><td>ช่องสีเขียว — ปลายทาง (Use) ที่ต้องระบุ</td></tr>
                                <tr><td><span class="rm-swatch" style="background:#fff3cd; border-color:#ffc107;"></span></td><td>ช่องสีเหลือง — ข้อมูลยังไม่ครบ / ไม่ถูกต้อง</td></tr>
                                <tr><td><span class="rm-swatch" style="background:#ffe5e5; border-color:#dc3545;"></span></td><td>แถวสีแดง (โหมดสลับ) — คู่ยังไม่ครบ 2 รายการ</td></tr>
                            </table>
                        </div>
                    </div>

                    <div class="modal-footer">
                        <button type="button" class="ab-btn ab-draft" data-dismiss="modal">ปิด</button>
                    </div>
                </div>
            </div>
        </div>

    </div>

    <script>
        window.showModal = function (data) {
            var tbody = document.getElementById('hrmsCheckBody');
            if (!tbody) { console.warn('hrmsCheckBody not found'); return; }
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
        function rmTab(btn, paneId) {
            var modal = document.getElementById('modal_ReadMe');
            modal.querySelectorAll('.rm-tab').forEach(function (b) { b.classList.remove('active'); });
            modal.querySelectorAll('.rm-pane').forEach(function (p) { p.classList.remove('active'); });
            btn.classList.add('active');
            document.getElementById(paneId).classList.add('active');
            modal.querySelector('.rm-body').scrollTop = 0;
        }

        // เปิดทุกครั้ง : เริ่มที่แท็บตามโหมด (Issue = ขั้นตอน , Approve / Confirm = การอนุมัติ)
        $(document).on('show.bs.modal', '#modal_ReadMe', function () {
            var hf = document.getElementById('<%= hfReadMeTab.ClientID %>');
            var paneId = (hf && hf.value) ? hf.value : 'rmSteps';
            var btn = this.querySelector('.rm-tab[data-pane="' + paneId + '"]') || this.querySelector('.rm-tab');
            if (btn) rmTab(btn, btn.getAttribute('data-pane') || 'rmSteps');
        });
    </script>

</asp:Content>