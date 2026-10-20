<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="setting_tanaoroshi.aspx.cs"
    Inherits="Tool_Equipment_Control_System.setting_tanaoroshi" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">

<link href="CSS/bootstrap.css" rel="stylesheet" />
<link href="CSS/jquery-ui.css" rel="stylesheet" />
<link href="CSS/font-awesome4.7.0.min.css" rel="stylesheet" />
<link href="DateTimePicker/2-bootstrap-datetimepicker.min.css" rel="stylesheet" />
<link href="DateTimePicker/3-bootstrap-datetimepicker-standalone.css" rel="stylesheet" />

<script src="JS/jquery-ui.js"></script>
<script src="Scripts/bootstrap.min.js"></script>
<script src="DateTimePicker/1-moment.min.js"></script>
<script src="DateTimePicker/4-bootstrap-datetimepicker.min.js"></script>

<style>
    /* ── Base ── */
    body,.main{background:#f0f4f8;font-size:13px;font-family:"Segoe UI",Arial,sans-serif;}

    /* ── Page ── */
    .tana-page{display:flex;flex-direction:column;height:calc(100vh - 80px);margin:30px 12px 0 12px;overflow:hidden;}
    .tana-card{background:#fff;border-radius:10px;box-shadow:0 2px 10px rgba(0,0,0,.08);
               display:flex;flex-direction:column;overflow:hidden;flex:1 1 auto;min-height:0;}

    /* ── Header ── */
    .tana-header{background:linear-gradient(135deg,#1a6e82,#0d4f5e);color:#fff;
                 padding:10px 18px;display:flex;align-items:center;gap:10px;flex-shrink:0;}
    .tana-header .title-text{font-size:18px;font-weight:700;}

    /* ── Filter Bar ── */
    .filter-bar{background:#f7fafc;border-bottom:1px solid #e2e8f0;padding:10px 16px;
                display:flex;align-items:center;flex-wrap:wrap;gap:8px;flex-shrink:0;}
    .filter-bar .filter-group{display:flex;align-items:center;gap:5px;}
    .filter-bar label{font-weight:600;color:#4a5568;white-space:nowrap;margin:0;font-size:12px;}
    .filter-bar .form-control{height:30px;font-size:12px;border-radius:5px;border:1px solid #cbd5e0;padding:2px 8px;}
    .filter-bar .form-control:focus{border-color:#1a6e82;box-shadow:0 0 0 2px rgba(26,110,130,.15);}
    .filter-bar select.form-control{width:auto;min-width:120px;}
    .filter-divider{width:1px;height:26px;background:#cbd5e0;margin:0 4px;}

    /* ── Setting Date : Pill style ── */
    .date-pill-group{flex-direction:column;align-items:flex-start;gap:4px;}
    .date-pill-label{display:flex;align-items:center;gap:5px;font-size:10px;font-weight:700;
                      color:#8a96a3;text-transform:uppercase;letter-spacing:.4px;margin:0 0 1px 2px;}
    .date-pill-label .glyphicon{font-size:11px;}
    .date-pill{display:flex;align-items:center;background:#eef2f6;border:1px solid #dde3ea;
               border-radius:20px;padding:4px 12px;gap:8px;transition:border-color .15s,box-shadow .15s;}
    .date-pill:focus-within{border-color:#1a6e82;box-shadow:0 0 0 2px rgba(26,110,130,.12);}
    .date-pill .glyphicon{color:#8a96a3;font-size:13px;}
    .date-pill .pill-input{border:none !important;background:transparent !important;box-shadow:none !important;
                            text-align:center;font-weight:600;color:#2d3748;width:104px;padding:0 !important;
                            height:auto !important;font-size:13px;cursor:pointer;}

    /* ── Buttons ── */
    .btn-t{border:none;color:#fff;border-radius:5px;font-size:12px;font-weight:600;
           padding:5px 14px;height:30px;line-height:20px;cursor:pointer;transition:background .2s;display:inline-block;text-decoration:none;}
    .btn-search  {background:#1a6e82;} .btn-search:hover  {background:#155f70;color:#fff;}
    .btn-set-tana{background:#2d8a5f;height:32px;} .btn-set-tana:hover{background:#24724e;color:#fff;}
    .btn-export  {background:#d97706;} .btn-export:hover  {background:#b45309;color:#fff;}
    .btn-exp-all {background:#6b46c1;} .btn-exp-all:hover {background:#553c9a;color:#fff;}
    .btn-print   {background:#0f766e;} .btn-print:hover   {background:#0c5f58;color:#fff;}
    .btn-list-row{background:#0ea5e9;border:none;color:#fff;border-radius:4px;
                  padding:2px 8px;font-size:11px;cursor:pointer;}
    .btn-list-row:hover{background:#0284c7;}

    /* ── Tabs ── */
    .tana-tabs{display:flex;border-bottom:2px solid #e2e8f0;padding:0 16px;background:#f7fafc;flex-shrink:0;}
    .tana-tab{padding:8px 18px;font-size:12px;font-weight:600;color:#718096;border:none;background:transparent;
              cursor:pointer;border-bottom:2px solid transparent;margin-bottom:-2px;transition:all .2s;}
    .tana-tab:hover{color:#1a6e82;}
    .tana-tab.active{color:#1a6e82;border-bottom-color:#1a6e82;}
    .tab-badge{display:inline-block;background:#e2e8f0;color:#4a5568;border-radius:10px;
               padding:1px 7px;font-size:10px;margin-left:5px;}
    .tana-tab.active .tab-badge{background:#1a6e82;color:#fff;}

    /* ── Tab Panels ── */
    .tab-panel{display:none;flex:1 1 auto;min-height:0;flex-direction:column;overflow:hidden;}
    .tab-panel.active{display:flex;}

    /* ── Action Bar ── */
    .action-bar{background:#f7fafc;border-bottom:1px solid #e2e8f0;padding:8px 16px;
                display:flex;align-items:center;gap:10px;flex-shrink:0;flex-wrap:wrap;}
    .status-filter-group{display:flex;gap:8px;align-items:center;margin-left:auto;}
    .status-chip{display:flex;align-items:center;gap:6px;border:1px solid #e2e8f0;background:#fff;
                 border-radius:20px;padding:5px 14px;font-size:12px;font-weight:600;color:#4a5568;
                 cursor:pointer;transition:all .15s;}
    .status-chip:hover{border-color:#1a6e82;color:#1a6e82;}
    .status-chip .chip-count{display:inline-block;background:#edf2f7;color:#4a5568;border-radius:10px;
                              padding:1px 8px;font-size:11px;font-weight:700;}
    .status-chip.active{color:#fff;border-color:transparent;}
    .status-chip.active .chip-count{background:rgba(255,255,255,.28);color:#fff;}
    .status-chip.active{background:#1a6e82;}
    .status-chip.chip-done.active{background:#276749;}
    .status-chip.chip-notset.active{background:#c53030;}

    /* ── GridView ── */
    .grid-wrapper{flex:1 1 auto;min-height:0;overflow:auto;}
    .tableGv{min-width:100%;border-collapse:collapse;font-size:12px;white-space:nowrap;}
    .tableGv th{background:#2b7a8e;color:#fff;font-weight:600;text-align:center;
                padding:7px 12px;border:1px solid #237a8e;position:sticky;top:0;z-index:2;}
    .tableGv td{text-align:center;padding:5px 12px;border:1px solid #e2e8f0;color:#2d3748;vertical-align:middle;}
    .tableGv tr:nth-child(even) td{background:#f7fafc;}
    .tableGv tr:hover td{background:#ebf8ff;}

    /* Status badges */
    .badge-done  {display:inline-block;background:#276749;color:#fff;border-radius:12px;padding:2px 10px;font-size:11px;font-weight:600;}
    .badge-notset{display:inline-block;background:#c53030;color:#fff;border-radius:12px;padding:2px 10px;font-size:11px;font-weight:600;}
    .row-done   td{background:#f0fff4 !important;}
    .row-notset td{background:#fff5f5 !important;}
    .row-done:hover   td{background:#c6f6d5 !important;}
    .row-notset:hover td{background:#fed7d7 !important;}

    /* ── GridView Tab2 ── */
    .tableGv2{min-width:100%;border-collapse:collapse;font-size:12px;white-space:nowrap;}
    .tableGv2 th{background:#6b46c1;color:#fff;font-weight:600;text-align:center;
                 padding:7px 12px;border:1px solid #553c9a;position:sticky;top:0;z-index:2;}
    .tableGv2 td{text-align:center;padding:5px 12px;border:1px solid #e2e8f0;color:#2d3748;vertical-align:middle;}
    .tableGv2 tr:nth-child(even) td{background:#faf5ff;}
    .tableGv2 tr:hover td{background:#e9ddf6;}

    /* ── Modal ── */
    .modal-tana .modal-dialog{width:92%;max-width:1080px;margin:30px auto;}
    .modal-tana .modal-content{border:none;border-radius:12px;overflow:hidden;
        box-shadow:0 20px 60px rgba(0,0,0,.3);}
    .modal-tana .modal-header{background:linear-gradient(135deg,#1a6e82,#0d4f5e);
        color:#fff;padding:14px 20px;border:none;display:flex;align-items:center;}
    .modal-tana .modal-header .close{color:#fff;opacity:.75;font-size:26px;
        text-shadow:none;margin-top:-2px;transition:opacity .15s;}
    .modal-tana .modal-header .close:hover{opacity:1;}
    .modal-tana .modal-title{font-size:15px;font-weight:700;letter-spacing:.3px;}
    .modal-tana .modal-body{padding:0;background:#f7fafc;max-height:none;}

    .modal-info{display:grid;grid-template-columns:repeat(auto-fit,minmax(150px,1fr));
        gap:10px 0;background:#fff;border-bottom:1px solid #e2e8f0;padding:14px 20px;}
    .modal-info .info-item{padding:0 14px;border-left:3px solid #1a6e82;margin:0;}
    .modal-info .info-label{font-size:10px;font-weight:700;color:#8a96a3;
        text-transform:uppercase;letter-spacing:.5px;margin-bottom:2px;}
    .modal-info .info-value{font-size:13px;font-weight:600;color:#2d3748;}

    .modal-grid-wrap{max-height:52vh;overflow-y:auto;overflow-x:auto;padding:0 20px 18px;}
    .tableModal{white-space:normal;table-layout:fixed;width:100%;border-collapse:separate;border-spacing:0;
        font-size:12px;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 1px 4px rgba(0,0,0,.06);margin-top:14px;}
    .tableModal th{background:#2b7a8e;color:#fff;padding:9px 12px;text-align:center;font-weight:600;border:none;
        position:sticky;top:0;z-index:2;}
    .tableModal td{padding:7px 12px;border:none;border-bottom:1px solid #edf2f7;text-align:center;color:#2d3748;}
    .tableModal td,.tableModal th{word-break:break-word;}
    .tableModal tr:last-child td{border-bottom:none;}
    .tableModal tbody tr:hover td{background:#ebf8ff;}
    .pill-status{display:inline-block;background:#e6f6ec;color:#276749;border-radius:12px;padding:2px 10px;font-size:11px;font-weight:700;}
    .tableModal .empty-modal{padding:30px;color:#a0aec0;font-size:13px;}
    .modal-tana .modal-footer{background:#fff;border-top:1px solid #e2e8f0;padding:10px 20px;}
    .modal-tana .modal-footer .btn{border-radius:6px;font-size:12px;font-weight:600;padding:6px 18px;}
    .modal-count{background:rgba(255,255,255,.22);border-radius:12px;padding:2px 10px;font-size:11px;font-weight:700;margin-left:10px;}

    /* ── Confirm Modal (Export/Print) ── */
    .confirm-list{max-height:50vh;overflow:auto;padding:6px 20px 16px;}
    .confirm-tbl{width:100%;border-collapse:collapse;font-size:12px;}
    .confirm-tbl th{background:#edf2f7;color:#4a5568;padding:8px 10px;text-align:left;font-weight:700;border-bottom:2px solid #cbd5e0;}
    .confirm-tbl td{padding:6px 10px;border-bottom:1px solid #edf2f7;color:#2d3748;}

    /* Loading Overlay */
    .overlay{position:fixed;inset:0;background:rgba(15,23,42,.45);z-index:9998;
             display:flex;align-items:center;justify-content:center;backdrop-filter:blur(2px);}
    .overlayContent{background:#fff;border-radius:14px;padding:26px 40px;text-align:center;
                    min-width:280px;height:auto;
                    box-shadow:0 12px 40px rgba(0,0,0,.28);}
    .overlayContent .spin-ring{width:46px;height:46px;margin:0 auto 14px;border:4px solid #e2e8f0;
                    border-top-color:#1a6e82;border-radius:50%;animation:spin .8s linear infinite;}
    .overlayContent .ov-title{font-size:16px;font-weight:700;color:#2d3748;margin-bottom:4px;}
    .overlayContent .ov-sub{font-size:12px;color:#718096;margin-bottom:14px;}
    /* progress bar วิ่ง (indeterminate) */
    .ov-bar{height:6px;background:#e2e8f0;border-radius:6px;overflow:hidden;}
    .ov-bar > i{display:block;height:100%;width:40%;border-radius:6px;
                background:linear-gradient(90deg,#1a6e82,#2d8a5f);
                animation:barrun 1.1s ease-in-out infinite;}
    @keyframes spin{to{transform:rotate(360deg);}}
    @keyframes barrun{0%{margin-left:-40%;}100%{margin-left:100%;}}

    /* Toast */
    #toast-container{position:fixed;top:16px;right:16px;display:flex;flex-direction:column;gap:8px;z-index:9999;}
    .toast{min-width:260px;max-width:420px;padding:10px 14px;border-radius:8px;color:#fff;
           box-shadow:0 6px 18px rgba(0,0,0,.15);display:flex;align-items:flex-start;gap:10px;
           animation:toast-in .2s ease-out;font-size:13px;}
    .toast .title{font-weight:700;margin-bottom:3px;} .toast .message{opacity:.93;}
    .toast.success{background:#276749;} .toast.error{background:#c53030;}
    .toast.warn{background:#b7791f;}    .toast.info{background:#2b6cb0;}
    .toast .close{margin-left:auto;background:transparent;border:none;color:inherit;font-size:16px;cursor:pointer;}
    @keyframes toast-in{from{opacity:0;transform:translateY(-8px);}to{opacity:1;transform:translateY(0);}}
</style>

<script>

    /* Toast */
    (function () {
        function ensureC() {
            var c = document.getElementById('toast-container');
            if (!c) { c = document.createElement('div'); c.id = 'toast-container'; document.body.appendChild(c); } return c;
        }
        window.showToast = function (msg, opt) {
            opt = opt || {}; var title = opt.title || '', type = opt.type || 'info', dur = opt.duration != null ? opt.duration : 3000;
            var cont = ensureC(), toast = document.createElement('div'); toast.className = 'toast ' + type;
            var body = document.createElement('div'); body.style.flex = '1';
            if (title) { var t = document.createElement('div'); t.className = 'title'; t.textContent = title; body.appendChild(t); }
            var m = document.createElement('div'); m.className = 'message'; m.textContent = msg; body.appendChild(m);
            var btn = document.createElement('button'); btn.className = 'close'; btn.innerHTML = '&times;';
            btn.onclick = function () { if (toast.parentNode) cont.removeChild(toast); };
            toast.appendChild(body); toast.appendChild(btn); cont.appendChild(toast);
            if (dur > 0) setTimeout(function () { if (toast.parentNode) cont.removeChild(toast); }, dur);
        };
    })();

    /* Tabs (คงแท็บข้าม postback ด้วย hidden field) */
    function switchTab(id) {
        document.querySelectorAll('.tana-tab').forEach(function (t) { t.classList.remove('active'); });
        document.querySelectorAll('.tab-panel').forEach(function (p) { p.classList.remove('active'); });
        var b = document.getElementById('tab-btn-' + id), p = document.getElementById('tab-panel-' + id);
        if (b) b.classList.add('active');
        if (p) p.classList.add('active');
        var hf = document.getElementById('<%=hf_tab.ClientID%>');
        if (hf) hf.value = id;
    }
    function restoreTab() {
        var hf = document.getElementById('<%=hf_tab.ClientID%>');
        switchTab(hf && hf.value ? hf.value : 1);
        restoreDoneChecks();
        hideFullOverlay();        // ★ ปิด overlay หลัง postback ทุกครั้ง
    }
    $(function () {
        restoreTab();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(restoreTab);
    });

    /* Check All (Tab 1) */
    function toggleAll(src) {
        var tbl = document.getElementById('<%=gv_list.ClientID%>');
        if (!tbl) return;
        tbl.querySelectorAll('input[type=checkbox]').forEach(function (c) { c.checked = src.checked; });
    }

    /* Filter สถานะ (Tab 1) */
    function filterStatus(type, btn) {
        document.querySelectorAll('.status-chip').forEach(function (b) { b.classList.remove('active'); });
        if (btn) btn.classList.add('active');
        var tbl = document.getElementById('<%=gv_list.ClientID%>');
        if (!tbl) return;
        tbl.querySelectorAll('tr').forEach(function (row) {
            if (row.querySelector('th')) return;
            var show = true;
            if (type === 'done') show = row.classList.contains('row-done');
            else if (type === 'notset') show = row.classList.contains('row-notset');
            row.style.display = show ? '' : 'none';
        });
    }

    /* DatePicker */
    $(window).load(function () {
        initDP();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () { initDP(); });
    });
    function initDP() {
        moment.locale('en');
        $('#SettingDatePicker').datetimepicker({ format: 'DD-MMM-YYYY', useCurrent: false, showClear: true, showClose: true });
        var raw = $('#<%=txt_set_date.ClientID%>').val();
        if (raw) { var m = moment(raw, 'DD-MMM-YYYY'); if (m.isValid()) $('#SettingDatePicker').data('DateTimePicker').date(m); }
    }

    /* Modal (list) */
    function openTanaModal() {
        var $m = $('#modalTanaList');
        if (!$m.parent().is('body')) $m.appendTo('body');
        $m.modal({ backdrop: true, keyboard: true, show: true });
    }

    /* ═══ Tab 2 : checkbox ค้างข้าม postback ═══
       เก็บ list_id ที่ติ๊กไว้ใน hidden field (hf_selected_ids) */
    function getDoneRows() {
        var tbl = document.getElementById('<%=gv_done.ClientID%>');
        return tbl ? Array.prototype.slice.call(tbl.querySelectorAll('tr')).filter(function (r) { return !r.querySelector('th'); }) : [];
    }
    function rowListId(row) {
        var h = row.querySelector('input[type=hidden]');
        return h ? h.value : '';
    }
    function rowCheckbox(row) {
        return row.querySelector('input[type=checkbox]');
    }
    function syncSelectedIds() {
        var excludeOn = document.getElementById('<%=hf_exclude.ClientID%>').value === '1';
        var ids = [];
        getDoneRows().forEach(function (row) {
            if (excludeOn && rowIsExcluded(row)) return;   // ข้ามรายการที่ถูกตัด
            var c = rowCheckbox(row);
            if (c && c.checked) { var id = rowListId(row); if (id) ids.push(id); }
        });
        var hf = document.getElementById('<%=hf_selected_ids.ClientID%>');
        if (hf) hf.value = ids.join(',');
        var lbl = document.getElementById('lblSelCount');
        if (lbl) lbl.innerText = ids.length ? ('เลือก ' + ids.length + ' รายการ') : '';
    }
    function onDoneCheck() { syncSelectedIds(); }
    function toggleAllDone(src) {
        var excludeOn = document.getElementById('<%=hf_exclude.ClientID%>').value === '1';
        getDoneRows().forEach(function (row) {
            if (row.style.display === 'none') return;
            if (excludeOn && rowIsExcluded(row)) { var cc = rowCheckbox(row); if (cc) cc.checked = false; return; }
            var c = rowCheckbox(row); if (c) c.checked = src.checked;
        });
        syncSelectedIds();
    }

    /* คืน checkbox หลัง postback จากค่าใน hidden field */
    function restoreDoneChecks() {
        var hf = document.getElementById('<%=hf_selected_ids.ClientID%>');
        if (!hf) return;
        var set = {}; (hf.value || '').split(',').forEach(function (x) { if (x) set[x] = 1; });
        getDoneRows().forEach(function (row) {
            var c = rowCheckbox(row), id = rowListId(row);
            if (c && set[id]) c.checked = true;
        });

        // คืนสภาพปุ่ม exclude (active + re-apply)
        var ex = document.getElementById('<%=hf_exclude.ClientID%>');
        var btn = document.getElementById('chipExclude');
        if (ex && btn) btn.classList.toggle('active', ex.value === '1');
        applyExclude();
    }

    /* ค้นหาข้อความใน Gridview (Tab 2) */
    function filterDoneGrid(kw) {
        var tbl = document.getElementById('<%=gv_done.ClientID%>');
        if (!tbl) return;
        kw = (kw || '').toLowerCase().trim();
        var found = 0;
        tbl.querySelectorAll('tr').forEach(function (row) {
            if (row.querySelector('th')) return;
            var show = kw === '' || row.innerText.toLowerCase().indexOf(kw) > -1;
            row.style.display = show ? '' : 'none';
            if (show) found++;
        });
        var lbl = document.getElementById('lblDoneFound');
        if (lbl) lbl.innerText = kw === '' ? '' : ('พบ ' + found + ' รายการ');
    }

    /* ═══ Confirm modal ก่อน Export / Print ═══ */
    function collectSelected() {
        var excludeOn = document.getElementById('<%=hf_exclude.ClientID%>').value === '1';
        var rows = [];
        getDoneRows().forEach(function (row) {
            var c = rowCheckbox(row);
            if (c && c.checked && row.style.display !== 'none') {
                if (excludeOn && rowIsExcluded(row)) return;   // ตัดออกจากรายการ export/print
                var cells = row.querySelectorAll('td');
                rows.push({
                    id: rowListId(row),
                    dept: cells[5] ? cells[5].innerText.trim() : '',
                    resp: cells[6] ? cells[6].innerText.trim() : '',
                    model: cells[7] ? cells[7].innerText.trim() : '',
                    cell: cells[9] ? cells[9].innerText.trim() : ''
                });
            }
        });
        return rows;
    }
    function showConfirm(mode) {   // mode = 'export' | 'print'
        var rows = collectSelected();
        if (rows.length === 0) {
            showToast('กรุณาเลือกรายการก่อน', { title: 'No Selection', type: 'warn' });
            return;
        }
        var body = '';
        rows.forEach(function (r, i) {
            body += '<tr><td>' + (i + 1) + '</td><td>' + r.dept + '</td><td>' + r.resp +
                '</td><td>' + r.model + '</td><td>' + r.cell + '</td></tr>';
        });
        document.getElementById('confirmBody').innerHTML = body;
        document.getElementById('confirmCount').innerText = rows.length;
        document.getElementById('confirmTitle').innerText =
            (mode === 'print' ? 'ยืนยันการพิมพ์' : 'ยืนยันการ Export');

        var btn = document.getElementById('btnConfirmAction');
        btn.innerText = (mode === 'print' ? '🖨 ยืนยัน Print' : '⬇ ยืนยัน Export');
        btn.onclick = function () {
            $('#modalConfirm').modal('hide');
            if (mode === 'print') doPrint(rows);
            else doExport();
        };

        var $m = $('#modalConfirm');
        if (!$m.parent().is('body')) $m.appendTo('body');
        $m.modal({ backdrop: true, keyboard: true, show: true });
    }

    function showFullOverlay(title, sub) {
        document.getElementById('fullOvTitle').innerText = title;
        document.getElementById('fullOvSub').innerText = sub;
        document.getElementById('fullOverlay').style.display = 'flex';
    }
    function hideFullOverlay() {
        document.getElementById('fullOverlay').style.display = 'none';
    }

    // ปิด overlay เมื่อ download เริ่ม (หน้าไม่ reload แต่ focus กลับ)
    //window.addEventListener('pageshow', hideFullOverlay);
    //window.addEventListener('focus', function () { setTimeout(hideFullOverlay, 500); });
    function readCookie(name) {
        var m = document.cookie.match('(^|;)\\s*' + name + '\\s*=\\s*([^;]+)');
        return m ? m.pop() : '';
    }
    function deleteCookie(name) {
        document.cookie = name + '=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/';
    }
    function doExport() {
        syncSelectedIds();
        deleteCookie('fileDownloadToken');   // เคลียร์ค่าเก่าก่อน
        showFullOverlay('กำลัง Export...', 'กำลังสร้างไฟล์ Excel กรุณารอสักครู่');

        // poll เช็ค cookie ที่ server จะฝังเมื่อไฟล์พร้อม
        var tries = 0;
        var timer = setInterval(function () {
            tries++;
            if (readCookie('fileDownloadToken')) {   // ไฟล์ถูกส่งแล้ว
                deleteCookie('fileDownloadToken');
                hideFullOverlay();
                clearInterval(timer);
            } else if (tries > 120) {                // กันค้าง : timeout 60 วิ
                hideFullOverlay();
                clearInterval(timer);
            }
        }, 500);

        __doPostBack('<%=btn_export_all.UniqueID%>', '');
    }
    function doPrint(rows) {
        var ids = rows.map(function (r) { return r.id; }).filter(function (x) { return x; });
        if (ids.length === 0) return;

        // เปิดแท็บว่างทันที (กัน popup block เพราะเรียกจาก user click โดยตรง)
        var printWin = window.open('', '_blank');
        if (!printWin) {
            showToast('เบราว์เซอร์บล็อกการเปิดหน้าต่าง กรุณาอนุญาต popup', { title: 'Popup Blocked', type: 'warn' });
            return;
        }

        // ใส่หน้า loading ให้แท็บใหม่ทันที — ผู้ใช้เห็น spinner ไม่ใช่หน้าว่าง
        printWin.document.write(
            '<!DOCTYPE html><html><head><meta charset="utf-8"><title>กำลังเตรียมหน้าพิมพ์...</title>' +
            '<style>' +
            'body{font-family:Calibri,Arial,sans-serif;display:flex;align-items:center;' +
            'justify-content:center;height:100vh;margin:0;background:#f7fafc;}' +
            '.box{text-align:center;}' +
            '.ring{width:52px;height:52px;margin:0 auto 18px;border:5px solid #e2e8f0;' +
            'border-top-color:#1a6e82;border-radius:50%;animation:spin .8s linear infinite;}' +
            '.t{font-size:18px;font-weight:700;color:#2d3748;}' +
            '.s{font-size:13px;color:#718096;margin-top:6px;}' +
            '@keyframes spin{to{transform:rotate(360deg);}}' +
            '</style></head><body><div class="box">' +
            '<div class="ring"></div>' +
            '<div class="t">กำลังเตรียมหน้าพิมพ์...</div>' +
            '<div class="s">กำลังดึงข้อมูลและรูปภาพ กรุณารอสักครู่</div>' +
            '</div></body></html>');
        printWin.document.close();

        // โหลดเนื้อหาจริงเข้าแท็บเดียวกัน (แทนที่หน้า loading เมื่อ server ตอบ)
        printWin.location.href = 'print_tanaoroshi.aspx?ids=' + ids.join(',');

        // overlay ที่หน้าหลักไม่ต้องแล้ว เพราะ loading ไปอยู่ในแท็บ print แทน
    } 

    function startRowExport() {
        deleteCookie('fileDownloadToken');
        showFullOverlay('กำลัง Export...', 'กำลังสร้างไฟล์ Excel');
        var tries = 0;
        var timer = setInterval(function () {
            tries++;
            if (readCookie('fileDownloadToken')) {
                deleteCookie('fileDownloadToken'); hideFullOverlay(); clearInterval(timer);
            } else if (tries > 120) { hideFullOverlay(); clearInterval(timer); }
        }, 500);
        return true;   // ให้ postback ทำงานต่อ
    }

    /* คำที่ต้องตัดออก (case-insensitive) */
    var EXCLUDE_WORDS = ['daisha', 'agv', 'ups', 'transformer'];

    /* เช็คว่าแถวนี้เข้าเงื่อนไขตัดออกไหม — ดูจาก Cell (คอลัมน์ที่ 10, index 9) */
    function rowIsExcluded(row) {
        var cells = row.querySelectorAll('td');
        var cellText = cells[9] ? cells[9].innerText.toLowerCase() : '';
        return EXCLUDE_WORDS.some(function (w) { return cellText.indexOf(w) > -1; });
    }

    /* toggle ปุ่มตัดรายการ */
    function toggleExclude(btn) {
        var hf = document.getElementById('<%=hf_exclude.ClientID%>');
        var on = !(hf.value === '1');          // สลับสถานะ
        hf.value = on ? '1' : '0';
        btn.classList.toggle('active', on);
        applyExclude();
    }

    /* บังคับ uncheck แถวที่เข้าเงื่อนไข เมื่อปุ่มเปิดอยู่ */
    function applyExclude() {
        var on = document.getElementById('<%=hf_exclude.ClientID%>').value === '1';
        if (!on) { syncSelectedIds(); return; }

        getDoneRows().forEach(function (row) {
            if (rowIsExcluded(row)) {
                var c = rowCheckbox(row);
                if (c) c.checked = false;      // ตัดออก
            }
        });
        syncSelectedIds();
    }

    function checkOnlyNotSet() {
        var tbl = document.getElementById('<%=gv_list.ClientID%>');
        if (!tbl) return;
        tbl.querySelectorAll('tr').forEach(function (row) {
            if (row.querySelector('th')) return;
            var c = row.querySelector('input[type=checkbox]');
            if (c) c.checked = row.classList.contains('row-notset');
        });
    }

    function showClearConfirm() {
        // ใช้ทุกแถวที่มองเห็นใน Tab 2 (ตาม filter ปัจจุบัน)
        var rows = [];
        getDoneRows().forEach(function (row) {
            if (row.style.display === 'none') return;
            var cells = row.querySelectorAll('td');
            rows.push({
                dept: cells[5] ? cells[5].innerText.trim() : '',
                resp: cells[6] ? cells[6].innerText.trim() : '',
                model: cells[7] ? cells[7].innerText.trim() : '',
                cell: cells[9] ? cells[9].innerText.trim() : ''
            });
        });
        if (rows.length === 0) {
            showToast('ไม่มีรายการให้ Clear', { title: 'No Data', type: 'warn' });
            return;
        }
        var body = '';
        rows.forEach(function (r, i) {
            body += '<tr><td>' + (i + 1) + '</td><td>' + r.dept + '</td><td>' +
                r.resp + '</td><td>' + r.model + '</td><td>' + r.cell + '</td></tr>';
        });
        document.getElementById('clearBody').innerHTML = body;
        document.getElementById('clearCount').innerText = rows.length;
        document.getElementById('btnClearConfirm').onclick = function () {
            $('#modalClear').modal('hide');
            showFullOverlay('กำลัง Clear...', 'กำลังลบข้อมูล กรุณารอสักครู่');
            __doPostBack('<%=btn_clear_tana.UniqueID%>', '');
        };
        var $m = $('#modalClear');
        if (!$m.parent().is('body')) $m.appendTo('body');
        $m.modal({ backdrop: true, keyboard: true, show: true });
    }

</script>

<asp:ScriptManager runat="server" ID="scriptManager" EnablePageMethods="true" />

<asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
    <ProgressTemplate>
        <div class="overlay">
            <div class="overlayContent">
                <div class="spin-ring"></div>
                <div class="ov-title">กำลังประมวลผล...</div>
                <div class="ov-sub">กรุณารอสักครู่ ระบบกำลังดึงข้อมูล</div>
                <div class="ov-bar"><i></i></div>
            </div>
        </div>
    </ProgressTemplate>
</asp:UpdateProgress>

<div id="fullOverlay" class="overlay" style="display:none;">
    <div class="overlayContent">
        <div class="spin-ring"></div>
        <div class="ov-title" id="fullOvTitle">กำลัง Export...</div>
        <div class="ov-sub" id="fullOvSub">กำลังสร้างไฟล์ Excel กรุณารอสักครู่</div>
        <div class="ov-bar"><i></i></div>
    </div>
</div>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
<ContentTemplate>

<asp:HiddenField runat="server" ID="hf_tab" Value="1" />
<asp:HiddenField runat="server" ID="hf_selected_ids" Value="" />
<asp:HiddenField runat="server" ID="hf_exclude" Value="0" />

<div class="tana-page">
    <div class="tana-card">

        <div class="tana-header">
            <span class="glyphicon glyphicon-calendar" style="font-size:22px;opacity:.9;"></span>
            <span class="title-text">SETTING TANAOROSHI</span>
        </div>

        <%-- Filter Bar (ใช้ร่วม 2 แท็บ) --%>
        <div class="filter-bar">
            <div class="filter-group">
                <label>Month</label>
                <asp:DropDownList runat="server" ID="ddl_month" CssClass="form-control" style="width:110px;"></asp:DropDownList>
            </div>
            <div class="filter-divider"></div>

            <div class="filter-group date-pill-group">
                <label class="date-pill-label">
                    <span class="glyphicon glyphicon-calendar"></span>FROM (EDIT DATE)
                </label>
                <div class="input-group date date-pill" id="SettingDatePicker">
                    <span class="glyphicon glyphicon-calendar"></span>
                    <asp:TextBox runat="server" ID="txt_set_date" CssClass="form-control pill-input" ReadOnly="true"></asp:TextBox>
                    <span class="glyphicon glyphicon-calendar"></span>
                </div>
            </div>

            <asp:LinkButton runat="server" ID="btn_search" CssClass="btn-t btn-search" OnClick="btn_search_Click">
                <span class="glyphicon glyphicon-search"></span>&nbsp;Search
            </asp:LinkButton>

            <div class="filter-divider"></div>

            <div class="filter-group">
                <label>Responsible</label>
                <asp:DropDownList runat="server" ID="ddl_resp" CssClass="form-control" style="width:150px;"
                    OnSelectedIndexChanged="ddl_resp_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
            </div>
            <div class="filter-group" runat="server" ID="filter_dept">
                <label>Dept</label>
                <asp:DropDownList runat="server" ID="ddl_dept" CssClass="form-control" style="width:150px;" Visible="false"
                    OnSelectedIndexChanged="ddl_dept_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
            </div>
            <div class="filter-group" runat="server" ID="filter_model">
                <label>Model</label>
                <asp:DropDownList runat="server" ID="ddl_model" CssClass="form-control" style="width:130px;"
                    OnSelectedIndexChanged="ddl_filter_Changed" AutoPostBack="true"></asp:DropDownList>
            </div>
            <div class="filter-group" runat="server" ID="filter_cell">
                <label>Cell</label>
                <asp:DropDownList runat="server" ID="ddl_cell" CssClass="form-control" style="width:140px;"
                    OnSelectedIndexChanged="ddl_filter_Changed" AutoPostBack="true"></asp:DropDownList>
            </div>
        </div>

        <asp:Panel runat="server" ID="div_list" Visible="false"
            style="display:flex;flex-direction:column;flex:1 1 auto;min-height:0;overflow:hidden;">

            <div class="tana-tabs">
                <button id="tab-btn-1" class="tana-tab active" onclick="switchTab(1);return false;">
                    <span class="glyphicon glyphicon-list"></span>&nbsp;Tanaoroshi List
                    <span class="tab-badge"><asp:Label runat="server" ID="lbl_total_count" Text="0"></asp:Label></span>
                </button>
                <button id="tab-btn-2" class="tana-tab" onclick="switchTab(2);return false;">
                    <span class="glyphicon glyphicon-ok-circle"></span>&nbsp;Set Done
                    <span class="tab-badge"><asp:Label runat="server" ID="lbl_done_count" Text="0"></asp:Label></span>
                </button>
            </div>

            <%-- ══ TAB 1 ══ --%>
            <div id="tab-panel-1" class="tab-panel active">
                <div class="action-bar">
                    <asp:LinkButton runat="server" ID="btn_set_tana" CssClass="btn-t btn-set-tana" OnClick="btn_set_tana_Click">
                        <span class="glyphicon glyphicon-ok-sign"></span>&nbsp;Set Tanaoroshi
                    </asp:LinkButton>

                    <button type="button" class="btn-t btn-set-tana" style="background:#c53030;"
                            onclick="checkOnlyNotSet();return false;">
                        <span class="glyphicon glyphicon-check"></span>&nbsp;เลือกเฉพาะที่ยังไม่ Set
                    </button>

                    <div class="status-filter-group" id="statusFilterGroup">
                        <button type="button" id="chipAll" class="status-chip active" onclick="filterStatus('all',this);return false;">
                            รายการทั้งหมด <span class="chip-count"><asp:Label runat="server" ID="lbl_chip_total" Text="0"></asp:Label></span>
                        </button>
                        <button type="button" id="chipDone" class="status-chip chip-done" onclick="filterStatus('done',this);return false;">
                            Set Done <span class="chip-count"><asp:Label runat="server" ID="lbl_chip_done" Text="0"></asp:Label></span>
                        </button>
                        <button type="button" id="chipNotSet" class="status-chip chip-notset" onclick="filterStatus('notset',this);return false;">
                            ยังไม่ Set <span class="chip-count"><asp:Label runat="server" ID="lbl_chip_notset" Text="0"></asp:Label></span>
                        </button>
                    </div>
                </div>

                <div class="grid-wrapper" style="height: calc(100vh - 335px); background: #fff; padding: 0px 10px;">
                    <asp:GridView runat="server" ID="gv_list" CssClass="tableGv"
                        AllowPaging="false" EmptyDataText="No data found"
                        AutoGenerateColumns="false" GridLines="Both"
                        OnRowDataBound="gv_list_RowDataBound">
                        <HeaderStyle Wrap="false" HorizontalAlign="Center" />
                        <RowStyle HorizontalAlign="Center" />
                        <Columns>
                            <asp:TemplateField HeaderText="No." ItemStyle-Width="45px">
                                <ItemTemplate>
                                    <asp:HiddenField runat="server" ID="hd_tool_id"     Value='<%#Eval("tool_id") %>' />
                                    <asp:HiddenField runat="server" ID="hd_ID"          Value='<%#Eval("ID") %>' />
                                    <asp:HiddenField runat="server" ID="hd_sys_status"  Value='<%#Eval("sys_status") %>' />
                                    <asp:HiddenField runat="server" ID="hd_tool_sub_no" Value='<%#Eval("sub_no") %>' />
                                    <asp:HiddenField runat="server" ID="hd_dept_code"   Value='<%#Eval("dept_code") %>' />
                                    <asp:HiddenField runat="server" ID="hd_rsp_empid"   Value='<%#Eval("rsp_empID") %>' />
                                    <asp:HiddenField runat="server" ID="hd_asset_name"  Value='<%#Eval("asset_name") %>' />
                                    <asp:HiddenField runat="server" ID="hd_location"    Value='<%#Eval("location1_id") %>' />
                                    <%# Container.DataItemIndex + 1 %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField ItemStyle-Width="40px">
                                <HeaderTemplate>
                                    <input type="checkbox" id="chk_all" onclick="toggleAll(this)" checked="checked" title="Check All" />
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:CheckBox runat="server" ID="chk_row" Checked="true" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Status" ItemStyle-Width="85px">
                                <ItemTemplate><asp:Label runat="server" ID="lb_status_flag"></asp:Label></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Dept" ItemStyle-Width="80px">
                                <ItemTemplate><asp:Label runat="server" ID="lb_dept_code" Text='<%#Eval("dept_code") %>'></asp:Label></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Asset No." ItemStyle-Width="120px">
                                <ItemTemplate><asp:Label runat="server" ID="lb_assetNo" Text='<%#Eval("FixAcces") %>'></asp:Label></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Sub Asset" ItemStyle-Width="110px">
                                <ItemTemplate><asp:Label runat="server" ID="lb_sub" Text='<%#Eval("sub_fix") %>'></asp:Label></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Control No." ItemStyle-Width="110px">
                                <ItemTemplate><asp:Label runat="server" ID="lb_ctrl" Text='<%#Eval("control_no") %>'></asp:Label></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Name" ItemStyle-Width="220px">
                                <ItemTemplate><asp:Label runat="server" ID="lb_name" Text='<%#Eval("toolName") %>'></asp:Label></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Model" ItemStyle-Width="130px">
                                <ItemTemplate>
                                    <asp:HiddenField runat="server" ID="hd_model" Value='<%#Eval("model_id") %>' />
                                    <asp:Label runat="server" ID="lb_model" Text='<%#Eval("model_name") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Cell" ItemStyle-Width="100px">
                                <ItemTemplate>
                                    <asp:HiddenField runat="server" ID="hd_cell" Value='<%#Eval("location2_id") %>' />
                                    <asp:Label runat="server" ID="lb_cell" Text='<%#Eval("cell") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Station" ItemStyle-Width="100px">
                                <ItemTemplate>
                                    <asp:HiddenField runat="server" ID="hd_station" Value='<%#Eval("station_id") %>' />
                                    <asp:Label runat="server" ID="lb_station" Text='<%#Eval("station") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div><%-- /tab-panel-1 --%>

            <%-- ══ TAB 2 ══ --%>
            <div id="tab-panel-2" class="tab-panel">
                <div class="action-bar">

                    <span style="font-size:12px;color:#718096;">
                        Month:&nbsp;<strong><asp:Label runat="server" ID="lbl_done_month" Text=""></asp:Label></strong>
                    </span>

                    <%-- ปุ่ม Export All ซ่อนไว้ ให้ confirm modal เรียกผ่าน __doPostBack --%>
                    <asp:LinkButton runat="server" ID="btn_export_all" CssClass="btn-t btn-exp-all"
                        OnClick="btn_export_all_Click" style="display:none;"></asp:LinkButton>

                    <button type="button" class="btn-t btn-exp-all" onclick="showConfirm('export');return false;">
                        <span class="glyphicon glyphicon-download-alt"></span>&nbsp;Export All
                    </button>
                    <button type="button" class="btn-t btn-print" onclick="showConfirm('print');return false;">
                        <span class="glyphicon glyphicon-print"></span>&nbsp;Print
                    </button>
                    <span id="lblSelCount" style="font-size:11px;color:#0f766e;font-weight:700;white-space:nowrap;"></span>

                    <div class="filter-group" style="margin-left:14px;">
                        <span class="glyphicon glyphicon-search" style="color:#8a96a3;"></span>
                        <input type="text" id="txtSearchDone" class="form-control" style="width:220px;height:30px;font-size:12px;"
                               placeholder="ค้นหาในรายการ..." onkeyup="filterDoneGrid(this.value);" />
                        <span id="lblDoneFound" style="font-size:11px;color:#718096;white-space:nowrap;"></span>
                    </div>

                    <%-- ปุ่ม toggle : ตัดรายการ Daisha / AGV / UPS --%>
                    <button type="button" id="chipExclude" class="status-chip chip-notset"
                            style="margin-left:10px;" onclick="toggleExclude(this);return false;">
                        <span class="glyphicon glyphicon-ban-circle"></span>&nbsp;ไม่เลือกรายการ Daisha, AGV, UPS, Transformer
                    </button>

                    <%-- ปุ่มจริงซ่อนไว้ ให้ modal เรียกผ่าน postback --%>
                    <asp:LinkButton runat="server" ID="btn_clear_tana" style="display:none;"
                        OnClick="btn_clear_tana_Click"></asp:LinkButton>

                    <button type="button" class="btn-t" style="background:#c53030; margin-left:auto;"
                            onclick="showClearConfirm();return false;">
                        <span class="glyphicon glyphicon-trash"></span>&nbsp;Clear Tana
                    </button>

                    
                </div>

                <div class="grid-wrapper" style="height: calc(100vh - 335px); background: #fff; padding: 0px 10px;">
                    <asp:GridView runat="server" ID="gv_done" CssClass="tableGv2"
                        AllowPaging="false" EmptyDataText="No data found"
                        AutoGenerateColumns="false" GridLines="Both"
                        OnRowDataBound="gv_done_RowDataBound">
                        <HeaderStyle Wrap="false" HorizontalAlign="Center" />
                        <RowStyle HorizontalAlign="Center" BackColor="#e2e5ed" />
                        <Columns>
                            <asp:TemplateField HeaderText="No." ItemStyle-Width="45px">
                                <ItemTemplate>
                                    <asp:HiddenField runat="server" ID="hd_list_id" Value='<%#Eval("id") %>' />
                                    <%# Container.DataItemIndex + 1 %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField ItemStyle-Width="40px">
                                <HeaderTemplate>
                                    <input type="checkbox" id="chk_all_done" onclick="toggleAllDone(this);" title="Check All" />
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <input type="checkbox" onclick="onDoneCheck();" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="List" ItemStyle-Width="50px">
                                <ItemTemplate>
                                    <asp:LinkButton runat="server" ID="btn_show_history" CssClass="btn-list-row"
                                        CausesValidation="false" OnClick="btn_show_history_Click">
                                        <span class="glyphicon glyphicon-th-list"></span>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Excel" ItemStyle-Width="55px">
                                <ItemTemplate>
                                    <asp:LinkButton runat="server" ID="btn_export_row" CssClass="btn-t btn-export"
                                        style="height:26px;padding:2px 8px;font-size:11px;"
                                        OnClientClick="startRowExport();"
                                        CausesValidation="false" OnClick="btn_export_row_Click">
                                        <span class="glyphicon glyphicon-download-alt"></span>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="tana_date"      HeaderText="Tana Date"   ItemStyle-Width="100px" />
                            <asp:BoundField DataField="dept_code"      HeaderText="Dept Code"   ItemStyle-Width="85px" />
                            <asp:BoundField DataField="pic_rsp_display"        HeaderText="Responsible" ItemStyle-Width="110px" />
                            <asp:BoundField DataField="model_name"     HeaderText="Model"       ItemStyle-Width="130px" />
                            <asp:BoundField DataField="location1_name" HeaderText="Location"    ItemStyle-Width="120px" />
                            <asp:BoundField DataField="location2_name" HeaderText="Cell"        ItemStyle-Width="100px" />
                        </Columns>
                    </asp:GridView>
                </div>

            </div><%-- /tab-panel-2 --%>

        </asp:Panel>
    </div>
</div>

<%-- ══ Modal : History List ══ --%>
<div class="modal fade modal-tana" id="modalTanaList" tabindex="-1" role="dialog"
           style="background: none; box-shadow: none; border: none; left: 305px; width: calc(100vw - 50px);">>
    <div class="modal-dialog modal-lg" role="document" style="width:75%;">
        <div class="modal-content">
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal">&times;</button>
                <h4 class="modal-title">
                    <span class="glyphicon glyphicon-th-list"></span>&nbsp;
                    <asp:Label runat="server" ID="lbl_modal_title" Text="Tanaoroshi Item List"></asp:Label>
                    <span class="modal-count"><asp:Label runat="server" ID="lbl_modal_count" Text="0"></asp:Label>&nbsp;items</span>
                </h4>
            </div>
            <div class="modal-body">
                <div class="modal-info">
                    <div class="info-item"><div class="info-label">List ID</div><div class="info-value"><asp:Label runat="server" ID="lbl_m_listid" Text="-"></asp:Label></div></div>
                    <div class="info-item"><div class="info-label">Tana Date</div><div class="info-value"><asp:Label runat="server" ID="lbl_m_date" Text="-"></asp:Label></div></div>
                    <div class="info-item"><div class="info-label">Dept Code</div><div class="info-value"><asp:Label runat="server" ID="lbl_m_dept" Text="-"></asp:Label></div></div>
                    <div class="info-item"><div class="info-label">Responsible</div><div class="info-value"><asp:Label runat="server" ID="lbl_m_rsp" Text="-"></asp:Label></div></div>
                    <div class="info-item"><div class="info-label">Cell</div><div class="info-value"><asp:Label runat="server" ID="lbl_m_cell" Text="-"></asp:Label></div></div>
                </div>
                <div class="modal-grid-wrap">
                    <asp:GridView runat="server" ID="gv_modal" CssClass="tableModal" AutoGenerateColumns="false"
                        GridLines="None" EmptyDataText="No data found" OnRowDataBound="gv_modal_RowDataBound">
                        <HeaderStyle Wrap="false" HorizontalAlign="Center" />
                        <RowStyle HorizontalAlign="Center" />
                        <EmptyDataRowStyle CssClass="empty-modal" HorizontalAlign="Center" />
                        <Columns>
                            <asp:TemplateField HeaderText="No." ItemStyle-Width="36px">
                                <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="asset_no"     HeaderText="Asset No."   ItemStyle-Width="110px" />
                            <asp:BoundField DataField="control_no"   HeaderText="Control No." ItemStyle-Width="110px" />
                            <asp:BoundField DataField="name"         HeaderText="Name"        ItemStyle-Width="200px" />
                            <asp:BoundField DataField="model_name"   HeaderText="Model"       ItemStyle-Width="90px" />
                            <asp:BoundField DataField="location2_name" HeaderText="Cell"      ItemStyle-Width="90px" />
                            <asp:BoundField DataField="location3_name" HeaderText="Station"   ItemStyle-Width="90px" />
                            <%-- อ้างอิงค่าปัจจุบัน (ถ้า location ถูกแก้ทีหลัง) --%>
                            <asp:BoundField DataField="cur_cell"     HeaderText="Cell (ปัจจุบัน)"    ItemStyle-Width="90px" />
                            <asp:BoundField DataField="cur_station"  HeaderText="Station (ปัจจุบัน)" ItemStyle-Width="90px" />
                            <asp:TemplateField HeaderText="อ้างอิง" ItemStyle-Width="90px">
                                <ItemTemplate><asp:Label runat="server" ID="lb_ref"></asp:Label></ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
            </div>
        </div>
    </div>
</div>

<%-- ══ Modal : Confirm Export / Print ══ --%>
<div class="modal fade modal-tana" id="modalConfirm" tabindex="-1" role="dialog"
    style="background: none; box-shadow: none; border: none; left: 305px; width: calc(100vw - 50px);">
    <div class="modal-dialog" role="document" style="width:75%;">
        <div class="modal-content">
            <div class="modal-header">                
                <h4 class="modal-title">
                    <span class="glyphicon glyphicon-check"></span>&nbsp;
                    <span id="confirmTitle">ยืนยัน</span>
                    <span class="modal-count"><span id="confirmCount">0</span>&nbsp;รายการ</span>
                </h4>
                <%--<button type="button" class="close" data-dismiss="modal">&times;</button>--%>
            </div>
            <div class="modal-body">
                <div class="confirm-list">
                    <table class="confirm-tbl">
                        <thead><tr><th>No.</th><th>Dept</th><th>Responsible</th><th>Model</th><th>Cell</th></tr></thead>
                        <tbody id="confirmBody"></tbody>
                    </table>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-default" data-dismiss="modal">ยกเลิก</button>
                <button type="button" class="btn btn-primary" id="btnConfirmAction">ยืนยัน</button>
            </div>
        </div>
    </div>
</div>

    <%--Modal Cancal--%>
  <div class="modal fade modal-tana" id="modalClear" tabindex="-1" role="dialog"
     style="background:none;left:305px;width:calc(100vw - 50px);">
      <div class="modal-dialog" role="document" style="width:70%;">
        <div class="modal-content">
          <div class="modal-header" style="background:linear-gradient(135deg,#c53030,#9b2c2c);">
            <h4 class="modal-title">
              <span class="glyphicon glyphicon-trash"></span>&nbsp;ยืนยันการ Clear Tanaoroshi
              <span class="modal-count"><span id="clearCount">0</span>&nbsp;รายการ</span>
            </h4>
          </div>
          <div class="modal-body">
            <div style="padding:12px 20px;color:#c53030;font-weight:600;">
              ⚠ ข้อมูลที่ลบไม่สามารถกู้คืนได้ กรุณาตรวจสอบก่อนยืนยัน
            </div>
            <div class="confirm-list">
              <table class="confirm-tbl">
                <thead><tr><th>No.</th><th>Dept</th><th>Responsible</th><th>Model</th><th>Cell</th></tr></thead>
                <tbody id="clearBody"></tbody>
              </table>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-default" data-dismiss="modal">ยกเลิก</button>
            <button type="button" class="btn btn-danger" id="btnClearConfirm">ยืนยันลบ</button>
          </div>
        </div>
      </div>
    </div>

<Triggers>
    <asp:PostBackTrigger ControlID="btn_export_all" />
</Triggers>

</ContentTemplate>
</asp:UpdatePanel>
</asp:Content>