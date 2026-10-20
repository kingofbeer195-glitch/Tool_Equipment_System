<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="tool_equipment_tanaoroshi.aspx.cs"
    Inherits="Tool_Equipment_Control_System.Tanaoroshi" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">

<link href="CSS/bootstrap.css" rel="stylesheet" />
<link href="CSS/jquery-ui.css" rel="stylesheet" />
<link href="CSS/font-awesome4.7.0.min.css" rel="stylesheet" />
<link href="CSS/select2.min.css" rel="stylesheet" />

<script src="JS/jquery-ui.js"></script>
<script src="Scripts/bootstrap.min.js"></script>
<script src="Scripts/select2.min.js"></script>

<style>
    body,.main{background:#f0f4f8;font-family:"Segoe UI",Arial,sans-serif;font-size:13px;}

    .tana-page{margin:30px 12px 0;}
    .tana-card{background:#fff;border-radius:10px;box-shadow:0 2px 10px rgba(0,0,0,.08);overflow:hidden;}

    .tana-header{background:linear-gradient(135deg,#660000,#8b1a1a);color:#fff;padding:12px 18px;
                 display:flex;align-items:center;gap:10px;}
    .tana-header .title-text{font-size:18px;font-weight:700;}

    /* Step badges */
    .step-bar{display:flex;gap:0;background:#f7fafc;border-bottom:1px solid #e2e8f0;padding:0;}
    .step-item{flex:1;padding:10px 16px;text-align:center;font-size:12px;font-weight:600;color:#a0aec0;
               border-bottom:3px solid transparent;position:relative;}
    .step-item.active{color:#8b1a1a;border-bottom-color:#8b1a1a;background:#fff;}
    .step-item .step-num{display:inline-block;width:22px;height:22px;line-height:22px;border-radius:50%;
                         background:#cbd5e0;color:#fff;font-weight:700;margin-right:6px;}
    .step-item.active .step-num{background:#8b1a1a;}

    /* Step 1 : login emp */
    .emp-panel{padding: 12px 20px 5px;}
    .emp-grid{display:flex; gap:10px;}
    .emp-field label{font-size:11px;font-weight:700;color:#718096;text-transform:uppercase;letter-spacing:.4px;margin-bottom:3px;display:block;}
    .emp-field .form-control{height:34px;font-size:13px;}
    .emp-notice{color:#c53030;font-size:12px;font-weight:600;margin-top:8px;}

    /* Section */
    .section-bar{padding:10px 18px;display:flex;align-items:center;gap:5px;flex-wrap:wrap;background:#fafafa;border-bottom:1px solid #eee;}
    /*.section-bar{padding:10px 18px;display:grid;grid-template-columns: 500px auto;align-items:center;gap:10px;flex-wrap:wrap;background:#fafafa;border-bottom:1px solid #eee;}*/
    .btn-t{border:none;color:#fff;border-radius:6px;font-size:13px;font-weight:600;padding:6px 14px;cursor:pointer;transition:background .2s;text-decoration:none;display:inline-block;}
    .btn-scan{background:#8b1a1a;} .btn-scan:hover{background:#6d1414;color:#fff;}
    .btn-search{background:#1a6e82;} .btn-search:hover{background:#155f70;color:#fff;}

    /* Filter group ใน section-bar */
    .flt-group{display:flex;align-items:center;gap:5px;margin-left:14px;}
    .flt-group label{font-size:12px;font-weight:600;color:#4a5568;margin:0;}

    /* Scan inline (บนหน้า แทนปุ่ม) */
    .scan-inline{display:flex;align-items:center;gap:12px;background:#fff;border:2px solid #8b1a1a;
                 border-radius:10px;padding:5px 16px;}
    .scan-inline .glyphicon-qrcode{font-size:32px;color:#8b1a1a;}
    .scan-input{border:none;outline:none;font-size:16px;font-weight:700;letter-spacing:.5px;color:#2d3748;width:320px;}
    .btn-clear{background:#718096;} .btn-clear:hover{background:#5a6678;color:#fff;}

    /* ปุ่ม Check ในแถว */
    .btn-check{display:inline-block;background:#798990;color:#fff;border-radius:5px;padding:3px 10px;
               font-size:11px;font-weight:600;text-decoration:none;}
    .btn-check:hover{background:#1968b1;color:#fff;}
    /* แถวที่ scan เจอ */
    .row-scanned td{background:#fffbea !important;}
    .row-scanned:hover td{background:#fef3c7 !important;}

    /* Status chips (ชิดขวาแถว tabs) */
    .status-chips{display:flex;gap:8px;align-items:center;margin-left:auto;padding:6px 0;}
    .schip{border:1px solid #e2e8f0;background:#fff;border-radius:20px;padding:5px 14px;font-size:12px;
           font-weight:600;color:#4a5568;cursor:pointer;text-decoration:none;transition:all .15s;}
    .schip:hover{border-color:#8b1a1a;color:#8b1a1a;}
    .schip .cnt{display:inline-block;background:#edf2f7;color:#4a5568;border-radius:10px;padding:1px 8px;font-size:11px;font-weight:700;margin-left:4px;}
    .schip.active{color:#fff;border-color:transparent;}
    .schip.active .cnt{background:rgba(255,255,255,.28);color:#fff;}
    .schip-all.active{background:#4a5568;}
    .schip-ok.active{background:#276749;}
    .schip-change.active{background:#b7791f;}
    .schip-outdept.active{background:#6b46c1;}
    .schip-ng.active{background:#c53030;}

    /* Qty badge + list status */
    .qty-badge{display:inline-block;background:#ebf8ff;color:#2b6cb0;border-radius:12px;padding:2px 10px;font-weight:700;}
    .badge-completed{display:inline-block;background:#276749;color:#fff;border-radius:12px;padding:2px 10px;font-size:11px;font-weight:700;}
    .badge-progress{display:inline-block;background:#b7791f;color:#fff;border-radius:12px;padding:2px 10px;font-size:11px;font-weight:700;}
    .badge-pending{display:inline-block;background:#a0aec0;color:#fff;border-radius:12px;padding:2px 10px;font-size:11px;font-weight:700;}
    .row-completed td{background:#f0fff4 !important;}

    /* ผลตรวจนับ badge (Tab List All) — สีเดียวกับปุ่ม result */
    .res-badge{display:inline-block;color:#fff;border-radius:12px;padding:3px 12px;
               font-size:11px;font-weight:700;letter-spacing:.3px;}
    .res-ok{background:#276749;}      /* Check OK */
    .res-change{background:#b7791f;}  /* Change Location */
    .res-ng{background:#c53030;}      /* Not Found */

    .btn-clearflt{background:#718096;} .btn-clearflt:hover{background:#5a6678;color:#fff;}

    /* Tabs */
    .tabs{display:flex;align-items:center;border-bottom:2px solid #e2e8f0;padding:0 16px;background:#f7fafc;}
    .tab{padding:9px 18px;font-size:13px;font-weight:600;color:#718096;background:transparent;border:none;
         cursor:pointer;border-bottom:2px solid transparent;margin-bottom:-2px;}
    .tab.active{color:#8b1a1a;border-bottom-color:#8b1a1a;}
    .tab-badge{display:inline-block;background:#e2e8f0;color:#4a5568;border-radius:10px;padding:1px 8px;font-size:11px;margin-left:5px;}
    .tab.active .tab-badge{background:#8b1a1a;color:#fff;}
    .tab-panel{display:none;} .tab-panel.active{display:block;}

    /* Grid */
    .grid-wrap{height:calc(100vh - 380px);overflow:auto;background: #fff;}
    .tableGv{width:100%;border-collapse:collapse;font-size:12px;white-space:nowrap;}
    .tableGv th{background:#5f90c6;color:#fff;padding:7px 12px;text-align:center;position:sticky;top:0;z-index:2;border:1px solid #4a7db0;}
    .tableGv td{padding:5px 12px;text-align:center;border:1px solid #e2e8f0;}
    .tableGv tr:nth-child(even) td{background:#f7fafc;}

    /* Modal scan */
    .modal-scan .modal-dialog{width:94%;max-width:1000px;margin:24px auto;}
    .modal-scan .modal-content{border:none;border-radius:12px;overflow:hidden;box-shadow:0 20px 60px rgba(0,0,0,.35);}
    .modal-scan .modal-header{background:linear-gradient(135deg,#660000,#8b1a1a);color:#fff;padding:14px 20px;border:none;}
    .modal-scan .modal-header .close{color:#fff;opacity:.8;font-size:26px;}
    .modal-scan .modal-title{font-size:16px;font-weight:700;}
    .modal-scan .modal-body{padding:18px 20px;background:#f7fafc;max-height:76vh;overflow:auto;}

    /* modal หลักจางลงตอน confirm เด้ง */
    .modal-scan.dimmed .modal-content{
        opacity:.55;
        transition:opacity .2s;
        filter:saturate(.7);
    }

    .scan-box{display:flex;gap:10px;align-items:center;background:#fff;border:2px solid #8b1a1a;border-radius:10px;padding:12px 16px;margin-bottom:14px;}
    .scan-box .glyphicon{font-size:24px;color:#8b1a1a;}
    .scan-box input{flex:1;border:none;font-size:20px;font-weight:700;letter-spacing:1px;outline:none;color:#2d3748;}
    .scan-status{margin-bottom:12px;min-height:22px;}
    .pill-wait{background:#edf2f7;color:#4a5568;border-radius:12px;padding:3px 12px;font-size:12px;font-weight:700;}
    .pill-done{background:#e6fffa;color:#234e52;border-radius:12px;padding:3px 12px;font-size:12px;font-weight:700;}
    .pill-ng{background:#fdecec;color:#c53030;border-radius:12px;padding:3px 12px;font-size:12px;font-weight:700;}

    /* Scan detail card */
    .detail-card{background:#fff;border-radius:10px;box-shadow:0 1px 5px rgba(0,0,0,.08);padding:16px;margin-bottom:14px;display:flex;gap:18px;}
    .detail-img{width:200px;height:200px;flex-shrink:0;border:1px solid #e2e8f0;border-radius:8px;object-fit:contain;background:#fafafa;}
    .detail-info{flex:1;}
    .detail-row{display:flex;padding:5px 0;border-bottom:1px solid #f0f0f0;}
    .detail-row .k{width:110px;font-weight:700;color:#718096;font-size:12px;}
    .detail-row .v{flex:1;color:#2d3748;font-weight:600;font-size:14px;}

    /* Result buttons */
    .result-btns{display:flex;gap:10px;margin-top:14px;}
    .btn-result{flex:1;border:none;color:#fff;border-radius:8px;font-size:15px;font-weight:700;padding:14px;cursor:pointer;transition:transform .1s;text-align:center;}
    .btn-result:active{transform:scale(.97);}
    .btn-result[disabled], .btn-result:disabled{opacity:.45;cursor:not-allowed;}
    .btn-ok{background:#276749;} .btn-ok:hover{background:#1f5138;color:#fff;}
    .btn-change{background:#b7791f;} .btn-change:hover{background:#95610f;color:#fff;}
    .btn-ng{background:#c53030;} .btn-ng:hover{background:#9b2c2c;color:#fff;}
    /* ปุ่ม result ที่ถูก disable (LinkButton render เป็น <a disabled="disabled">) */
    .btn-result[disabled],
    .btn-result.aspNetDisabled{
        background:#cbd5e0 !important;
        color:#718096 !important;
        cursor:not-allowed;
        opacity:1;
        box-shadow:none;
        pointer-events:none;
    }
    .btn-result[disabled] .glyphicon,
    .btn-result.aspNetDisabled .glyphicon{color:#a0aec0;}


    /* Change location panel */
    .change-panel{background:#fffbea;border:1px solid #f6e05e;border-radius:10px;padding:16px;margin-top:12px;}
    .change-title{font-size:14px;font-weight:700;color:#975a16;margin-bottom:12px;}
    .change-grid2{display:grid;grid-template-columns:repeat(3,1fr);gap:12px;margin-bottom:12px;}
    .chg-field label{font-size:11px;font-weight:700;color:#975a16;display:block;margin-bottom:3px;}
    .chg-field .form-control{height:34px;font-size:13px;}

    /*cancel button*/
    .btn-clear-small{display:inline-block;background:#718096;color:#fff;
      border-radius:5px;padding:4px 12px;font-size:12px;font-weight:600;
      text-decoration:none;}
    .btn-clear-small:hover{background:#5a6678;color:#fff;}
    .mismatch-warn{background:#fff5f5;border:1px solid #fc8181;color:#c53030;
      border-radius:8px;padding:8px 12px;margin-bottom:12px;font-weight:700;font-size:13px;}
    .detail-side-title{font-size:12px;font-weight:800;color:#8b1a1a;
      text-transform:uppercase;letter-spacing:.4px;margin-bottom:6px;
      border-bottom:2px solid #8b1a1a;padding-bottom:3px;}
    /* ผลเทียบ */
    .v.match{color:#276749;background:#f0fff4;border-radius:4px;padding:0 6px;font-weight:700;}
    .v.mismatch{color:#c53030;background:#fdecec;border-radius:4px;padding:0 6px;font-weight:700;}

    /* Add New : dropdown + textbox + toggle */
    .addnew-row{display:flex;gap:4px;align-items:center;}
    .addnew-row .form-control{flex:1;min-width:0;}
    .addnew-txt{flex:1;}
    .btnToggle{width:34px;height:34px;flex-shrink:0;border:none;border-radius:6px;background:#975a16;color:#fff;font-size:18px;font-weight:700;cursor:pointer;line-height:1;}
    .btnToggle:hover{background:#7a4712;}
    /* select2 ให้เต็มความกว้างในกล่อง */
    .addnew-row .select2-container{flex:1;min-width:0;}

    /* Confirm modal */
    .confirm-box{padding:26px 22px;text-align:center;border-radius:12px 12px 0 0;
             border-top:6px solid transparent;transition:all .2s;}
    .confirm-box.theme-ok{background:#f0fff4;border-top-color:#276749;}
    .confirm-box.theme-change{background:#fffbea;border-top-color:#b7791f;}
    .confirm-box.theme-ng{background:#fff5f5;border-top-color:#c53030;}
    .confirm-icon{font-size:60px;margin-bottom:14px;} 
    .confirm-icon.ok{color:#276749;}
    .confirm-icon.change{color:#b7791f;}
    .confirm-icon.ng{color:#c53030;}
    .confirm-msg{font-size:20px;font-weight:800;color:#2d3748;margin-bottom:6px;}
    .confirm-sub{font-size:15px;color:#718096;}
    .confirm-asset{display:inline-block;background:#edf2f7;border-radius:6px;
                   padding:6px 16px;font-weight:700;font-size:16px;color:#2d3748;margin-top:10px;}

    /* แถวปุ่ม confirm */
    .confirm-actions{display:flex;gap:10px;padding:14px 18px 18px;background:#fff;}
    .btn-confirm-cancel{flex:1;border:2px solid #cbd5e0;background:#fff;color:#4a5568;
        border-radius:10px;font-size:16px;font-weight:700;padding:14px;cursor:pointer;transition:all .15s;}
    .btn-confirm-cancel:hover{background:#edf2f7;}

    /* ปุ่มยืนยันใหญ่ + เปลี่ยนสีตามธีม */
    .btn-confirm-yes{flex:2;border:none;color:#fff;border-radius:10px;
        font-size:18px;font-weight:800;letter-spacing:.5px;padding:16px;cursor:pointer;
        transition:transform .1s;box-shadow:0 4px 12px rgba(0,0,0,.18);}
    .btn-confirm-yes:active{transform:scale(.97);}
    .btn-confirm-yes.yes-ok{background:#276749;}
    .btn-confirm-yes.yes-ok:hover{background:#1f5138;}
    .btn-confirm-yes.yes-change{background:#b7791f;}
    .btn-confirm-yes.yes-change:hover{background:#95610f;}
    .btn-confirm-yes.yes-ng{background:#c53030;}
    .btn-confirm-yes.yes-ng:hover{background:#9b2c2c;}
      
    /* ล็อกปุ่ม result ทั้งกลุ่มตอน confirm เปิด */
    .result-btns.btns-locked{pointer-events:none;opacity:.5;filter:grayscale(.3);}

    /* Toast */
    #toast-container{position:fixed;top:16px;right:16px;display:flex;flex-direction:column;gap:8px;z-index:99999;}
    .toast{min-width:260px;max-width:420px;padding:10px 14px;border-radius:8px;color:#fff;box-shadow:0 6px 18px rgba(0,0,0,.15);display:flex;gap:10px;font-size:13px;}
    .toast .title{font-weight:700;margin-bottom:3px;}
    .toast.success{background:#276749;}.toast.error{background:#c53030;}.toast.warn{background:#b7791f;}.toast.info{background:#2b6cb0;}
    .toast .close{margin-left:auto;background:transparent;border:none;color:inherit;font-size:16px;cursor:pointer;}

    /*qty modal*/
    .qty-link{border:none;background:none;cursor:pointer;text-decoration:none;}
    .qty-link:hover .qty-badge{background:#bee3f8;text-decoration:underline;}

    /* สถานะผลตรวจ ตัวใหญ่ */
    .result-big{display:inline-block;font-size:20px;font-weight:800;letter-spacing:.5px;
      border-radius:10px;padding:10px 26px;}
    .result-wait{background:#fff5f5;color:#c53030;border:2px solid #fc8181;}
    .result-done{background:#e6fffa;color:#234e52;border:2px solid #38b2ac;}

    /* หัวข้อชุดข้อมูลหลัก */
    .detail-head{border-bottom:2px solid #e2e8f0;padding-bottom:6px;margin-bottom:10px;}

    /* ตารางเปรียบเทียบ */
    .compare-head{font-size:13px;font-weight:800;color:#8b1a1a;margin:6px 0 8px;}
    .compare-grid{display:grid;grid-template-columns:1fr 1fr;gap:14px;}
    .cmp-col{border:1px solid #e2e8f0;border-radius:8px;padding:10px 12px;}
    .cmp-title{font-size:12px;font-weight:800;text-transform:uppercase;letter-spacing:.4px;
      padding-bottom:4px;margin-bottom:6px;border-bottom:2px solid #cbd5e0;color:#4a5568;}
    .cmp-src .cmp-title{color:#8b1a1a;border-bottom-color:#8b1a1a;}

    /*List Tana*/
    .list-modal-sub{margin-top:8px;display:flex;align-items:center;gap:10px;font-size:13px;}
    .list-modal-sub .lm-chip{background:rgba(255,255,255,.18);border-radius:16px;padding:4px 14px;color:#fff;}
    .list-modal-sub .lm-chip b{font-weight:800;}
    .list-modal-sub .lm-sep{color:rgba(255,255,255,.6);}

    .ld-thumb{width:72px;height:72px;object-fit:contain;background:#fafafa;
    border:1px solid #e2e8f0;border-radius:6px;cursor:zoom-in;transition:transform .1s;}
    .ld-thumb:hover{transform:scale(1.08);border-color:#8b1a1a;}

    /* Loading Overlay */
    .overlay{position:fixed;inset:0;background:rgba(15,23,42,.45);z-index:99997;
             display:flex;align-items:center;justify-content:center;backdrop-filter:blur(2px);}
    .overlayContent{background:#fff;border-radius:14px;padding:26px 40px;text-align:center;
                    min-width:280px;height:auto;box-shadow:0 12px 40px rgba(0,0,0,.28);}
    .overlayContent .spin-ring{width:46px;height:46px;margin:0 auto 14px;border:4px solid #e2e8f0;
                    border-top-color:#1a6e82;border-radius:50%;animation:spin .8s linear infinite;}
    .overlayContent .ov-title{font-size:16px;font-weight:700;color:#2d3748;margin-bottom:4px;}
    .overlayContent .ov-sub{font-size:12px;color:#718096;margin-bottom:14px;}
    .ov-bar{height:6px;background:#e2e8f0;border-radius:6px;overflow:hidden;}
    .ov-bar > i{display:block;height:100%;width:40%;border-radius:6px;
                background:linear-gradient(90deg,#1a6e82,#2d8a5f);
                animation:barrun 1.1s ease-in-out infinite;}
    @keyframes spin{to{transform:rotate(360deg);}}
    @keyframes barrun{0%{margin-left:-40%;}100%{margin-left:100%;}}

</style>

<script>
    /* Toast */
    (function () {
        function ensureC() { var c = document.getElementById('toast-container'); if (!c) { c = document.createElement('div'); c.id = 'toast-container'; document.body.appendChild(c); } return c; }
        window.showToast = function (msg, opt) {
            opt = opt || {}; var title = opt.title || '', type = opt.type || 'info', dur = opt.duration != null ? opt.duration : 3000;
            var cont = ensureC(), t = document.createElement('div'); t.className = 'toast ' + type; var b = document.createElement('div'); b.style.flex = '1';
            if (title) { var h = document.createElement('div'); h.className = 'title'; h.textContent = title; b.appendChild(h); }
            var m = document.createElement('div'); m.textContent = msg; b.appendChild(m);
            var x = document.createElement('button'); x.className = 'close'; x.innerHTML = '&times;'; x.onclick = function () { if (t.parentNode) cont.removeChild(t); };
            t.appendChild(b); t.appendChild(x); cont.appendChild(t);
            if (dur > 0) setTimeout(function () { if (t.parentNode) cont.removeChild(t); }, dur);
        };
    })();

    /* Tabs */
    function switchTab(id) {
        document.querySelectorAll('.tab').forEach(function (t) { t.classList.remove('active'); });
        document.querySelectorAll('.tab-panel').forEach(function (p) { p.classList.remove('active'); });
        var b = document.getElementById('tab-btn-' + id), p = document.getElementById('tab-panel-' + id);
        if (b) b.classList.add('active'); if (p) p.classList.add('active');
    }

    /* ไฮไลต์ chip ที่เลือก (arg: "" | "1" | "2" | "3") */
    function setActiveChip(arg) {
        var map = { '': '<%=chip_all.ClientID%>', '1': '<%=chip_ok.ClientID%>', '2': '<%=chip_change.ClientID%>', '3': '<%=chip_ng.ClientID%>', '4': '<%=chip_outdept.ClientID%>' };
        document.querySelectorAll('.schip').forEach(function (c) { c.classList.remove('active'); });
        var el = document.getElementById(map[arg]);
        if (el) el.classList.add('active');
    }

    /* Modal : เปิดโมดัลรายละเอียด (จัดการ backdrop ซ้อน) */
    function openTanaModal() {
        var $m = $('#modalScan');
        if (!$m.parent().is('body')) $m.appendTo('body');
        $m.modal({ backdrop: true, keyboard: true, show: true });
    }
    function focusScanBox() {
        setTimeout(function () {
            var el = document.getElementById('<%=txt_scan.ClientID%>');
            if (el) { el.focus(); el.select(); }
        }, 150);
    }
    /* Enter ในช่อง scan = กดปุ่มค้นหา (ไม่ submit ทั้งฟอร์ม) */
    function scanKey(e) {
        if (e.keyCode === 13) {
            e.preventDefault();
            e.stopPropagation();
            document.getElementById('<%=btn_scan_go.ClientID%>').click();
            return false;
        }
    }
    /* highlight แถวที่ scan เจอใน Tab List All */
    function highlightScanned() {
        var tbl = document.getElementById('<%=gv_tab2.ClientID%>');
        if (!tbl) return;
        tbl.querySelectorAll('tr').forEach(function (row) {
            if (row.querySelector('th')) return;
            row.classList.add('row-scanned');
        });
    }
    /* ล้าง backdrop ซ้อนหลัง postback */
    $(function () {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            if ($('.modal-backdrop').length > 1) {
                $('.modal-backdrop').not(':last').remove();
            }
        });
    });

    /* ═══ Confirm Modal ก่อน update ═══
       return false เพื่อกัน postback รอบแรก → ปุ่มใน modal ค่อย trigger จริง */
    var _pendingBtn = null;
    function confirmResult(kind) {
        var asset = document.getElementById('<%=lbl_d_asset.ClientID%>');
        var assetText = asset ? asset.innerText : '';

        var icon = document.getElementById('confirmIcon');
        var msg = document.getElementById('confirmMsg');
        var sub = document.getElementById('confirmSub');
        var assetBox = document.getElementById('confirmAsset');
        var yesBtn = document.getElementById('btnConfirmYes');
        var box = document.getElementById('confirmBox');

        // reset theme class
        box.className = 'confirm-box';
        yesBtn.className = 'btn-confirm-yes';

        if (kind === 'ok') {
            box.classList.add('theme-ok');
            yesBtn.classList.add('yes-ok');
            icon.className = 'confirm-icon ok';
            icon.innerHTML = '<span class="glyphicon glyphicon-ok-circle"></span>';
            msg.innerText = 'ยืนยันผล : Check OK';
            sub.innerText = 'อุปกรณ์นี้ตรวจสอบแล้ว อยู่ครบ';
            yesBtn.innerHTML = '<span class="glyphicon glyphicon-ok"></span> ยืนยัน Check OK';
            _pendingBtn = '<%=btn_result_ok.ClientID%>';
        } else if (kind === 'ng') {
            box.classList.add('theme-ng');
            yesBtn.classList.add('yes-ng');
            icon.className = 'confirm-icon ng';
            icon.innerHTML = '<span class="glyphicon glyphicon-remove-circle"></span>';
            msg.innerText = 'ยืนยันผล : Not Found';
            sub.innerText = 'ไม่พบอุปกรณ์นี้ (นับเป็น NG)';
            yesBtn.innerHTML = '<span class="glyphicon glyphicon-remove"></span> ยืนยัน Not Found';
            _pendingBtn = '<%=btn_ng_confirm.ClientID%>';
        } else if (kind === 'change') {
            box.classList.add('theme-change');
            yesBtn.classList.add('yes-change');
            icon.className = 'confirm-icon change';
            icon.innerHTML = '<span class="glyphicon glyphicon-transfer"></span>';
            msg.innerText = 'ยืนยัน : Change Location';
            sub.innerText = 'บันทึกตำแหน่งใหม่ของอุปกรณ์นี้';
            yesBtn.innerHTML = '<span class="glyphicon glyphicon-ok"></span> ยืนยัน Location';
            _pendingBtn = '<%=btn_change_confirm.ClientID%>';
            }

        if (kind === 'ok' || kind === 'ng') {
            var cp = document.querySelector('.change-panel');
            if (cp) cp.style.display = 'none';
        }

        assetBox.innerText = assetText;

        // ── ล็อกปุ่ม result ทั้งหมดไว้ระหว่างเปิด confirm ──
        lockResultButtons(true);

        var $c = $('#modalConfirm');
        if (!$c.parent().is('body')) $c.appendTo('body');
        $c.modal('show');
        return false;
    }

    // ล็อก/ปลดล็อกปุ่ม result 3 ปุ่ม
    function lockResultButtons(lock) {
        var rb = document.querySelector('.result-btns');
        if (rb) rb.classList.toggle('btns-locked', lock);
    }
    $(function () {
        // ยกเลิก confirm → clear _pendingBtn + ปลดล็อกปุ่ม 3 ปุ่มให้กดได้
        $(document).on('click', '#btnConfirmCancel', function () {
            _pendingBtn = null;
            lockResultButtons(false);
            $('#modalConfirm').modal('hide');
        });
    });

    $(function () {
        $('#modalConfirm').on('hidden.bs.modal', function () {
            $('#modalScan').removeClass('dimmed');
            lockResultButtons(false);   // ← ปลดล็อกเมื่อปิด confirm
        });
    });

    /* ปุ่มยืนยันใน modal → trigger ปุ่มจริง (postback) */
    $(function () {
        $(document).on('click', '#btnConfirmYes', function () {
            $('#modalConfirm').modal('hide');
            if (_pendingBtn) {
                var el = document.getElementById(_pendingBtn);
                if (el) {
                    // เอา OnClientClick ออกชั่วคราว ไม่ให้เด้ง confirm ซ้ำ
                    var oldClick = el.onclick; el.onclick = null;
                    el.click();
                    el.onclick = oldClick;
                }
            }
        });
    });

    $(function () {
        $('#modalConfirm').on('hidden.bs.modal', function () {
            $('#modalScan').removeClass('dimmed');
        });
    });

    /* ═══ Select2 ใน Change panel ═══ */
    function initChangeSelect2() {
        try {
            $('#modalScan .ddlSearch').each(function () {
                if (!$(this).hasClass('select2-hidden-accessible')) {
                    $(this).select2({ width: 'resolve', dropdownParent: $('#modalScan') });
                }
            });
        } catch (e) { }
    }

    /* ═══ Add New toggle (pattern borrow_tool2) ═══ */
    $(document).on('click', '.btnToggle', function () {
        var row = $(this).closest('.addnew-row');
        var ddl = row.find('select');
        var txt = row.find("input[type='text']");
        var hidden = row.find("input[type='hidden']");
        var btn = $(this);

        if (ddl.is(':visible')) {
            if (ddl.hasClass('select2-hidden-accessible')) ddl.select2('destroy');
            ddl.hide(); txt.show(); btn.text('−'); hidden.val('txt');
        } else {
            ddl.show(); txt.hide(); btn.text('+'); hidden.val('ddl');
            ddl.select2({ width: 'resolve', dropdownParent: $('#modalScan') });
        }
    });

    /* คืน state Add New หลัง postback */
    function restoreAddNewState() {
        $('.addnew-row').each(function () {
            var row = $(this);
            var ddl = row.find('select');
            var txt = row.find("input[type='text']");
            var hidden = row.find("input[type='hidden']");
            var btn = row.find('.btnToggle');

            if (hidden.val() === 'txt') {
                if (ddl.hasClass('select2-hidden-accessible')) ddl.select2('destroy');
                ddl.hide(); txt.show(); btn.text('−');
            } else {
                ddl.show(); txt.hide(); btn.text('+');
                if (!ddl.hasClass('select2-hidden-accessible'))
                    ddl.select2({ width: 'resolve', dropdownParent: $('#modalScan') });
            }
        });
    }
    $(function () {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            initChangeSelect2();
            restoreAddNewState();
        });
    });

    //Qty Modal
    function openListModal() {
        var $m = $('#modalListDetail');
        if (!$m.parent().is('body')) $m.appendTo('body');
        $m.modal({ backdrop: true, keyboard: true, show: true });
    }

    //ddl Search
    function initFilterSelect2() {
        $('.fltSearch').each(function () {
            if (!$(this).hasClass('select2-hidden-accessible'))
                $(this).select2({ width: 'resolve' });
        });
    }
    $(function () { initFilterSelect2(); });
    $(function () {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            initFilterSelect2();
        });
    });

    function zoomImg(el) {
        var big = document.getElementById('imgZoomBig');
        big.src = el.src;
        var $m = $('#modalImgZoom');
        if (!$m.parent().is('body')) $m.appendTo('body');
        $m.modal({ backdrop: true, keyboard: true, show: true });
    }

    /* Loading Overlay : ทุก async postback ของ UpdatePanel */
    $(function () {
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_beginRequest(function () {
            var o = document.getElementById('loadingOverlay');
            if (o) o.style.display = 'flex';
        });
        prm.add_endRequest(function () {
            var o = document.getElementById('loadingOverlay');
            if (o) o.style.display = 'none';
        });
    });

</script>

<asp:ScriptManager runat="server" ID="scriptManager" EnablePageMethods="true" />

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
<ContentTemplate>

<%-- hidden context ของชิ้นที่ scan --%>
<asp:HiddenField runat="server" ID="hd_history_id" />
<asp:HiddenField runat="server" ID="hd_tool_id" />
<asp:HiddenField runat="server" ID="hd_tool_sub" />
<asp:HiddenField runat="server" ID="hd_sys_status" />

<div class="tana-page">
    <div class="tana-card">
        <div class="tana-header">
            <span class="glyphicon glyphicon-check" style="font-size:22px;"></span>
            <span class="title-text">TANAOROSHI (ตรวจนับอุปกรณ์)</span>
        </div>

        <%-- Step bar --%>
        <%--<div class="step-bar">
            <div class="step-item active"><span class="step-num">1</span>ยืนยันผู้ตรวจนับ</div>
            <div class="step-item active"><span class="step-num">2</span>รายการอุปกรณ์</div>
            <div class="step-item active"><span class="step-num">3</span>Scan ตรวจนับ</div>
        </div>--%>

        <%-- ══ STEP 1 : login emp ══ --%>
        <asp:Panel runat="server" ID="pnl_login_emp" Visible="false">
            <div class="emp-panel" style="border-bottom: 1px solid #811313;">
                <div class="emp-grid">
                    <div class="emp-field" style="width: 120px;">
                        <label>Employee ID</label>
                        <asp:TextBox runat="server" ID="txt_emp_id" CssClass="form-control" style="text-align:center;"
                            AutoPostBack="true" OnTextChanged="txt_emp_id_TextChanged"></asp:TextBox>
                    </div>
                    <div class="emp-field" style="width: 280px;">
                        <label>ชื่อ-สกุล</label>
                        <asp:TextBox runat="server" ID="txt_emp_name" CssClass="form-control" style="text-align:center;" ReadOnly="true"></asp:TextBox>
                    </div>
                    <div class="emp-field" style="width: 280px;">
                        <label>ตำแหน่ง</label>
                        <asp:TextBox runat="server" ID="txt_position" CssClass="form-control" style="text-align:center;" ReadOnly="true"></asp:TextBox>
                    </div>
                    <div class="emp-field" style="width: 120px;">
                        <label>Dept</label>
                        <asp:TextBox runat="server" ID="txt_dept" CssClass="form-control" style="text-align:center;" ReadOnly="true"></asp:TextBox>
                    </div>
                    <div class="emp-field" style="width: 120px;">
                        <label>Dept Code</label>
                        <asp:TextBox runat="server" ID="txt_dept_code" CssClass="form-control" style="text-align:center;" ReadOnly="true"></asp:TextBox>
                    </div>
                </div>
                <div class="emp-notice"><asp:Label runat="server" ID="lbl_emp_notice"></asp:Label></div>
            </div>
        </asp:Panel>

        <%-- ══ STEP 2 + 3 ══ --%>
        <asp:Panel runat="server" ID="pnl_step2" Visible="false">

            <div class="section-bar" style="border-bottom: 1px solid #730909;"> 
                
                <%-- Filter --%>
                <div class="flt-group">
                    <label>Period : </label>
                    <asp:DropDownList runat="server" ID="ddl_tana_date" CssClass="form-control fltSearch" style="width:100px;"
                        AutoPostBack="true" OnSelectedIndexChanged="ddl_filter_Changed"></asp:DropDownList>
                </div>
                <div class="flt-group">
                    <label>Responsible : </label>
                    <asp:DropDownList runat="server" ID="ddl_Responsible" CssClass="form-control fltSearch" style="width:200px;"
                        AutoPostBack="true" OnSelectedIndexChanged="ddl_filter_Changed"></asp:DropDownList>
                </div>
                <div class="flt-group">
                    <label>Model : </label>
                    <asp:DropDownList runat="server" ID="ddl_model" CssClass="form-control fltSearch" style="width:160px;"
                        AutoPostBack="true" OnSelectedIndexChanged="ddl_filter_Changed"></asp:DropDownList>
                </div>
                <div class="flt-group">
                    <label>Cell : </label>
                    <asp:DropDownList runat="server" ID="ddl_cell" CssClass="form-control fltSearch" style="width:250px;"
                        AutoPostBack="true" OnSelectedIndexChanged="ddl_filter_Changed"></asp:DropDownList>
                </div>

                <%-- Clear Filter : Responsible / Model / Cell --%>
                <asp:LinkButton runat="server" ID="btn_clear_filter" CssClass="btn-t btn-clearflt"
                    style="margin-left:14px;" OnClick="btn_clear_filter_Click">
                    <span class="glyphicon glyphicon-remove-circle"></span>&nbsp;Reset
                </asp:LinkButton>

            </div>
                
            <%--Barcode Scan--%>
            <div class="scan-inline" style="margin: 10px 40px; width:640px;">
                <span class="glyphicon glyphicon-qrcode"></span>
                <asp:TextBox runat="server" ID="txt_scan" CssClass="scan-input" style="height:35px;"
                    placeholder="Scan หรือกรอก Asset No. / Control No."
                    onkeydown="scanKey(event);"></asp:TextBox>
                <asp:LinkButton runat="server" ID="btn_scan_go" CssClass="btn-t btn-scan"
                    OnClick="btn_scan_go_Click">
                    <span class="glyphicon glyphicon-search"></span>&nbsp;ค้นหา
                </asp:LinkButton>
                <asp:LinkButton runat="server" ID="btn_scan_clear" CssClass="btn-t btn-clear"
                    OnClick="btn_scan_clear_Click">
                    <span class="glyphicon glyphicon-refresh"></span>&nbsp;แสดงทั้งหมด
                </asp:LinkButton>
            </div>
                
                         

            <asp:HiddenField runat="server" ID="hf_status_filter" Value="" />

            <div class="tabs">
                <button id="tab-btn-1" type="button" class="tab active" onclick="switchTab(1);return false;">
                    Tanaoroshi List <span class="tab-badge"><asp:Label runat="server" ID="lbl_tab1_count" Text="0"></asp:Label></span>
                </button>
                <button id="tab-btn-2" type="button" class="tab" onclick="switchTab(2);return false;">
                    List All <span class="tab-badge"><asp:Label runat="server" ID="lbl_tab2_count" Text="0"></asp:Label></span>
                </button>

                <%-- Status filter chips : ชิดขวา --%>
                <div class="status-chips">
                    <asp:LinkButton runat="server" ID="chip_all" CssClass="schip schip-all active"
                        CommandArgument="" OnClick="btn_status_filter_Click">
                        รายการทั้งหมด <span class="cnt"><asp:Label runat="server" ID="lbl_cnt_all" Text="0"></asp:Label></span>
                    </asp:LinkButton>
                    <asp:LinkButton runat="server" ID="chip_ok" CssClass="schip schip-ok"
                        CommandArgument="1" OnClick="btn_status_filter_Click">
                        Check OK <span class="cnt"><asp:Label runat="server" ID="lbl_cnt_ok" Text="0"></asp:Label></span>
                    </asp:LinkButton>
                    <asp:LinkButton runat="server" ID="chip_change" CssClass="schip schip-change"
                        CommandArgument="2" OnClick="btn_status_filter_Click">
                        Change Location <span class="cnt"><asp:Label runat="server" ID="lbl_cnt_change" Text="0"></asp:Label></span>
                    </asp:LinkButton>
                    <asp:LinkButton runat="server" ID="chip_outdept" CssClass="schip schip-outdept"
                        CommandArgument="4" OnClick="btn_status_filter_Click">
                        Other Dept <span class="cnt"><asp:Label runat="server" ID="lbl_cnt_outdept" Text="0"></asp:Label></span>
                    </asp:LinkButton>
                    <asp:LinkButton runat="server" ID="chip_ng" CssClass="schip schip-ng"
                        CommandArgument="3" OnClick="btn_status_filter_Click">
                        Not Found <span class="cnt"><asp:Label runat="server" ID="lbl_cnt_ng" Text="0"></asp:Label></span>
                    </asp:LinkButton>
                </div>
            </div>

            <%-- Tab 1 : Equipment_tana_list --%>
            <div id="tab-panel-1" class="tab-panel active">
                <div class="grid-wrap">
                    <asp:GridView runat="server" ID="gv_tab1" CssClass="tableGv"
                        AutoGenerateColumns="false" GridLines="Both" EmptyDataText="ไม่พบข้อมูล"
                        OnRowDataBound="gv_tab1_RowDataBound">
                        <Columns>
                            <asp:TemplateField HeaderText="No."><ItemTemplate><%# Container.DataItemIndex+1 %></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Status" ItemStyle-Width="120px">
                                <ItemTemplate><asp:Label runat="server" ID="lb_list_status"></asp:Label></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="จำนวน (ตรวจ/ทั้งหมด)" ItemStyle-Width="120px">
                                <ItemTemplate>
                                    <asp:HiddenField runat="server" ID="hd_list_id" Value='<%#Eval("id") %>' />
                                    <asp:LinkButton runat="server" ID="lb_qty" CssClass="qty-link"
                                        CausesValidation="false" OnClick="lb_qty_Click"></asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="tana_date"    HeaderText="Tana Date" ItemStyle-Width="120px" />
                            <asp:BoundField DataField="dept_code"    HeaderText="Dept" ItemStyle-Width="100px" />
                            <asp:TemplateField HeaderText="Responsible">
                              <ItemTemplate>
                                <%# Eval("pic_rsp") %>
                                <%# string.IsNullOrEmpty(Eval("pic_rsp_name","{0}"))
                                    ? "" : " - " + Eval("pic_rsp_name") %>
                              </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="model_name"   HeaderText="Model" />
                            <%--<asp:BoundField DataField="location_name" HeaderText="Location" />--%>
                            <asp:BoundField DataField="cell_name"    HeaderText="Cell" />                            
                            
                        </Columns>
                    </asp:GridView>
                </div>
            </div>

            <%-- Tab 2 : Equipment_tana_history (List All) --%>
            <div id="tab-panel-2" class="tab-panel">
                <div class="grid-wrap">
                    <asp:GridView runat="server" ID="gv_tab2" CssClass="tableGv"
                        AutoGenerateColumns="false" GridLines="Both" EmptyDataText="ไม่พบข้อมูล"
                        OnRowDataBound="gv_tab2_RowDataBound">
                        <Columns>
                            <asp:TemplateField HeaderText="No."><ItemTemplate><%# Container.DataItemIndex+1 %></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Check" ItemStyle-Width="70px">
                                <ItemTemplate>
                                    <asp:HiddenField runat="server" ID="hd_row_id" Value='<%#Eval("id") %>' />
                                    <asp:LinkButton runat="server" ID="btn_check_row" CssClass="btn-check"
                                        CausesValidation="false" OnClick="btn_check_row_Click">
                                        <span class="glyphicon glyphicon-edit"></span> Check
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="ผลตรวจนับ" ItemStyle-Width="120px">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lb_result_badge"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="ผู้ตรวจนับ">
                              <ItemTemplate>
                                <%# Eval("check_by") %>
                                <%# string.IsNullOrEmpty(Eval("check_by","{0}"))
                                    ? "" : " - " + Eval("check_name") %>
                              </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="asset_no"     HeaderText="Asset No." />
                            <asp:BoundField DataField="control_no"   HeaderText="Control No." />
                            <asp:BoundField DataField="name"         HeaderText="Name" />
                            <asp:BoundField DataField="model_name"   HeaderText="Model" />
                            <asp:BoundField DataField="cell_name"    HeaderText="Cell" />
                            <asp:BoundField DataField="station_name" HeaderText="Station" />
                            
                        </Columns>
                    </asp:GridView>
                </div>
            </div>

        </asp:Panel>
    </div>
</div>

<%-- ══ MODAL : SCAN (Step 3) ══ --%>
<div class="modal fade modal-scan" id="modalScan" tabindex="-1" role="dialog"
    style="background: none; box-shadow: none; border: none; left: 305px; width: calc(100vw - 50px);">
    <div class="modal-dialog" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal">&times;</button>
                <h4 class="modal-title"><span class="glyphicon glyphicon-qrcode"></span>&nbsp;Scan ตรวจนับอุปกรณ์</h4>
            </div>
            <div class="modal-body">

                <div class="scan-status"><asp:Label runat="server" ID="lbl_scan_status"></asp:Label></div>

                <%-- รายละเอียดชิ้นที่เลือก --%>
                <asp:Panel runat="server" ID="pnl_scan_detail" Visible="false">

                  <%-- สถานะผลตรวจ ตัวใหญ่ชัดเจน --%>
                  <div class="scan-status" style="text-align:center;margin-bottom:14px;">
                    <asp:Label runat="server" ID="Label1"></asp:Label>
                  </div>

                   <asp:Panel runat="server" ID="pnl_mismatch_warn" Visible="false" CssClass="mismatch-warn">
                      <span class="glyphicon glyphicon-warning-sign"></span>
                      &nbsp;ข้อมูล Dept Code / Model / Cell / Station ไม่ตรงกับระบบ (ช่องที่แดง) *อาจมีการ Update ข้อมูลหลัง Setting Tanaoroshi
                    </asp:Panel>

                  <div class="detail-card">
                    <asp:Image runat="server" ID="img_scan" CssClass="detail-img" Visible="false" />
                    <div class="detail-info" style="flex:1;">

                      <%-- ── ชุดข้อมูลหลัก (Tana History) : Asset / Control / Name ── --%>
                      <div class="detail-head">
                        <div class="detail-row"><div class="k">Asset No.</div><div class="v"><asp:Label runat="server" ID="lbl_d_asset"></asp:Label></div></div>
                        <div class="detail-row"><div class="k">Control No.</div><div class="v"><asp:Label runat="server" ID="lbl_d_control"></asp:Label></div></div>
                        <div class="detail-row"><div class="k">Name</div><div class="v"><asp:Label runat="server" ID="lbl_d_name"></asp:Label></div></div>
                      </div>

                      <%-- ── เปรียบเทียบ Model / Cell / Station (2 ฝั่ง) ── --%>
                      <div class="compare-head">เปรียบเทียบ Location</div>
                          <div class="compare-grid">
                              <div class="cmp-col cmp-src">
                                <div class="cmp-title">Tana_History</div>
                                <div class="detail-row"><div class="k">Dept Code</div><div class="v"><asp:Label runat="server" ID="lbl_d_dept"></asp:Label></div></div>
                                <div class="detail-row"><div class="k">Model</div><div class="v"><asp:Label runat="server" ID="lbl_d_model"></asp:Label></div></div>
                                <div class="detail-row"><div class="k">Cell</div><div class="v"><asp:Label runat="server" ID="lbl_d_cell"></asp:Label></div></div>
                                <div class="detail-row"><div class="k">Station</div><div class="v"><asp:Label runat="server" ID="lbl_d_station"></asp:Label></div></div>
                              </div>
                              <div class="cmp-col cmp-sys">
                                <div class="cmp-title">ในระบบ (ปัจจุบัน)</div>
                                <div class="detail-row"><div class="k">Dept Code</div><asp:Label runat="server" ID="lbl_s_dept" CssClass="v"></asp:Label></div>
                                <div class="detail-row"><div class="k">Model</div><asp:Label runat="server" ID="lbl_s_model" CssClass="v"></asp:Label></div>
                                <div class="detail-row"><div class="k">Cell</div><asp:Label runat="server" ID="lbl_s_cell" CssClass="v"></asp:Label></div>
                                <div class="detail-row"><div class="k">Station</div><asp:Label runat="server" ID="lbl_s_station" CssClass="v"></asp:Label></div>
                              </div>
                        </div>
                    </div>
                  </div>

                    <%-- 3 ปุ่มผลลัพธ์ (มี confirm modal ก่อน update) --%>
                    <div class="result-btns">
                        <asp:LinkButton runat="server" ID="btn_result_ok" CssClass="btn-result btn-ok"
                            OnClientClick="return confirmResult('ok');" OnClick="btn_result_ok_Click">
                            <span class="glyphicon glyphicon-ok"></span> Check OK
                        </asp:LinkButton>
                        <asp:LinkButton runat="server" ID="btn_result_change" CssClass="btn-result btn-change" OnClick="btn_result_change_Click">
                            <span class="glyphicon glyphicon-transfer"></span> Change Location
                        </asp:LinkButton>
                        <asp:LinkButton runat="server" ID="btn_result_ng" CssClass="btn-result btn-ng"
                            OnClick="btn_result_ng_Click">
                            <span class="glyphicon glyphicon-remove"></span> Not Found
                        </asp:LinkButton>
                    </div>
                    
                    <%--Cancel Button--%>
                    <div style="text-align:right;margin-top:8px;">
                      <asp:LinkButton runat="server" ID="btn_clear_check" Visible="false"
                          CssClass="btn-clear-small" CausesValidation="false"
                          OnClientClick="return confirm('ยกเลิกผลการตรวจนับของชิ้นนี้?');"
                          OnClick="btn_clear_check_Click">
                        <span class="glyphicon glyphicon-erase"></span> Clear
                      </asp:LinkButton>
                    </div>

                    <%-- Change Location : Dept/Resp + Model/Cell/Station (Select2 + Add New) --%>
                    <asp:Panel runat="server" ID="pnl_change_location" Visible="false" CssClass="change-panel">
                        <div class="change-title"><span class="glyphicon glyphicon-transfer"></span>&nbsp;เปลี่ยน Location ใหม่</div>

                        <div class="change-grid2">
                            <div class="chg-field">
                                <label>Dept ใหม่</label>
                                <asp:DropDownList runat="server" ID="ddl_new_dept" CssClass="form-control ddlSearch"></asp:DropDownList>
                            </div>
                            <div class="chg-field">
                                <label>Responsible ใหม่</label>
                                <asp:DropDownList runat="server" ID="ddl_new_resp" CssClass="form-control ddlSearch"></asp:DropDownList>
                            </div>
                        </div>

                        <div class="change-grid2">
                            <%-- Model : dropdown + Add New toggle --%>
                            <div class="chg-field addnew-group">
                                <label>Model ใหม่</label>
                                <div class="addnew-row">
                                    <asp:DropDownList runat="server" ID="ddl_new_model" CssClass="form-control ddlSearch"></asp:DropDownList>
                                    <asp:TextBox runat="server" ID="txt_model_new" CssClass="form-control addnew-txt" placeholder="พิมพ์ Model ใหม่" style="display:none;"></asp:TextBox>
                                    <asp:HiddenField runat="server" ID="hd_model_mode" Value="ddl" />
                                    <button type="button" class="btnToggle">+</button>
                                </div>
                            </div>
                            <%-- Cell --%>
                            <div class="chg-field addnew-group">
                                <label>Cell ใหม่</label>
                                <div class="addnew-row">
                                    <asp:DropDownList runat="server" ID="ddl_new_cell" CssClass="form-control ddlSearch"></asp:DropDownList>
                                    <asp:TextBox runat="server" ID="txt_cell_new" CssClass="form-control addnew-txt" placeholder="พิมพ์ Cell ใหม่" style="display:none;"></asp:TextBox>
                                    <asp:HiddenField runat="server" ID="hd_cell_mode" Value="ddl" />
                                    <button type="button" class="btnToggle">+</button>
                                </div>
                            </div>
                            <%-- Station --%>
                            <div class="chg-field addnew-group">
                                <label>Station ใหม่</label>
                                <div class="addnew-row">
                                    <asp:DropDownList runat="server" ID="ddl_new_station" CssClass="form-control ddlSearch"></asp:DropDownList>
                                    <asp:TextBox runat="server" ID="txt_station_new" CssClass="form-control addnew-txt" placeholder="พิมพ์ Station ใหม่" style="display:none;"></asp:TextBox>
                                    <asp:HiddenField runat="server" ID="hd_station_mode" Value="ddl" />
                                    <button type="button" class="btnToggle">+</button>
                                </div>
                            </div>
                        </div>

                        <div style="margin-bottom:10px;">
                            <label style="font-size:11px;font-weight:700;color:#975a16;">หมายเหตุ</label>
                            <asp:TextBox runat="server" ID="txt_change_remark" CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox>
                        </div>

                        <asp:LinkButton runat="server" ID="btn_change_confirm" CssClass="btn-t btn-change"
                            OnClientClick="return confirmResult('change');" OnClick="btn_change_confirm_Click">
                            <span class="glyphicon glyphicon-ok"></span>&nbsp;ยืนยัน Change Location
                        </asp:LinkButton>

                    </asp:Panel>

                    <%-- Not Found : กรอกหมายเหตุก่อนยืนยัน --%>
                    <asp:Panel runat="server" ID="pnl_ng_remark" Visible="false" CssClass="change-panel" style="background:#fff5f5;border-color:#fc8181;">
                        <div class="change-title" style="color:#c53030;"><span class="glyphicon glyphicon-remove-circle"></span>&nbsp;Not Found — ระบุหมายเหตุ</div>
                        <div style="margin-bottom:10px;">
                            <label style="font-size:11px;font-weight:700;color:#c53030;">หมายเหตุ (สาเหตุที่ไม่พบ)</label>
                            <asp:TextBox runat="server" ID="txt_ng_remark" CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox>
                        </div>
                        <asp:LinkButton runat="server" ID="btn_ng_confirm" CssClass="btn-t btn-ng"
                            OnClientClick="return confirmResult('ng');" OnClick="btn_ng_confirm_Click">
                            <span class="glyphicon glyphicon-ok"></span>&nbsp;ยืนยัน Not Found
                        </asp:LinkButton>
                    </asp:Panel>
                </asp:Panel>

            </div>
            <div class="modal-footer" style="background:#fff;">
                <button type="button" class="btn btn-default" data-dismiss="modal">ปิด</button>
            </div>
        </div>
    </div>
</div>

<%-- ══ Bootstrap Confirm Modal (ยืนยันก่อน update) ══ --%>
<div class="modal fade" id="modalConfirm" tabindex="-1" role="dialog" data-backdrop="static"
    style="background: none; box-shadow: none; border: none; width: 1050px; left: calc((100vw / 2) - 250px);">
    <div class="modal-dialog modal-sm" role="document">
        <div class="modal-content" style="border:none;border-radius:12px;overflow:hidden; width: 400px;">
            <div id="confirmBox" class="confirm-box">
                <div id="confirmIcon" class="confirm-icon ok"><span class="glyphicon glyphicon-ok-circle"></span></div>
                <div id="confirmMsg" class="confirm-msg">ยืนยันผลการตรวจนับ</div>
                <div id="confirmSub" class="confirm-sub"></div>
                <div id="confirmAsset" class="confirm-asset"></div>
            </div>
            <div class="confirm-actions">
                <button type="button" class="btn-confirm-cancel" id="btnConfirmCancel" data-dismiss="modal">
                    <span class="glyphicon glyphicon-remove"></span> ยกเลิก
                </button>
                <button type="button" class="btn-confirm-yes yes-ok" id="btnConfirmYes">
                    <span class="glyphicon glyphicon-ok"></span> ยืนยัน
                </button>
            </div>
        </div>
    </div>
</div>

<%-- Modal : รายการใน list (คลิกจากคอลัมน์จำนวน) --%>
<div class="modal fade modal-scan" id="modalListDetail" tabindex="-1" role="dialog" style="width: 1050px;left: calc(-240px + 50vw); background: none; box-shadow: none; border: none;">
  <div class="modal-dialog" role="document" style="">
    <div class="modal-content">
      <div class="modal-header">
        <button type="button" class="close" data-dismiss="modal">&times;</button>
        <h4 class="modal-title">
            <span class="glyphicon glyphicon-list"></span>&nbsp;
            <asp:Label runat="server" ID="lbl_list_modal_title"></asp:Label>
        </h4>
        <div class="list-modal-sub" style="width:auto; text-align:center;">
            <span class="lm-chip"><span class="glyphicon glyphicon-calendar"></span>
                Month : <b><asp:Label runat="server" ID="lbl_lm_period"></asp:Label></b></span>
            <span class="lm-sep">|</span>
            <span class="lm-chip"><span class="glyphicon glyphicon-user"></span>
                Responsible : <b><asp:Label runat="server" ID="lbl_lm_resp"></asp:Label></b></span>
            <span class="lm-sep">|</span>
            <span class="lm-chip"><span class="glyphicon glyphicon-th-large"></span>
                Model : <b><asp:Label runat="server" ID="lbl_lm_model"></asp:Label></b></span>
            <span class="lm-sep">|</span>
            <span class="lm-chip"><span class="glyphicon glyphicon-map-marker"></span>
                Cell : <b><asp:Label runat="server" ID="lbl_lm_cell"></asp:Label></b></span>
        </div>
      </div>
      <div class="modal-body">
        <div class="grid-wrap">
          <asp:GridView runat="server" ID="gv_list_detail" CssClass="tableGv" style="min-width:1500px;"
            AutoGenerateColumns="false" GridLines="Both" EmptyDataText="ไม่พบข้อมูล"
            OnRowDataBound="gv_list_detail_RowDataBound">
            <Columns>
                <asp:TemplateField HeaderText="No.">
                    <ItemTemplate><%# Container.DataItemIndex+1 %></ItemTemplate>
                </asp:TemplateField>

                <%-- 2.2 ผลตรวจนับ (แยกสี) --%>
                <asp:TemplateField HeaderText="ผลตรวจนับ" ItemStyle-Width="120px">
                    <ItemTemplate><asp:Label runat="server" ID="lb_ld_result"></asp:Label></ItemTemplate>
                </asp:TemplateField>

                <%-- 2.2 ผู้ตรวจสอบ --%>
                <asp:TemplateField HeaderText="ผู้ตรวจสอบ">
                    <ItemTemplate>
                        <%# Eval("check_by") %>
                        <%# string.IsNullOrEmpty(Eval("check_by","{0}"))
                            ? "" : " - " + Eval("check_name") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <%-- 2.1 รูป (คลิกซูม) --%>
                <asp:TemplateField HeaderText="รูป" ItemStyle-Width="100px">
                    <ItemTemplate>
                        <asp:Image runat="server" ID="img_ld" CssClass="ld-thumb"
                            onclick="zoomImg(this);" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField DataField="asset_no"   HeaderText="Asset No." />
                <asp:BoundField DataField="control_no" HeaderText="Control No." />
                <asp:BoundField DataField="name"       HeaderText="Name" />
                <asp:BoundField DataField="station_name" HeaderText="Station" />
                <asp:TemplateField HeaderText="Remark">
                    <ItemTemplate><asp:Label runat="server" ID="lb_ld_remark"></asp:Label></ItemTemplate>
                </asp:TemplateField>
            </Columns>
          </asp:GridView>
        </div>
      </div>
    </div>
  </div>
</div>

<%--pop-up : picture--%>
<div class="modal fade" id="modalImgZoom" tabindex="-1" role="dialog"
     style="background:none;box-shadow:none;border:none;">
  <div class="modal-dialog" role="document" style="max-width:640px;">
    <div class="modal-content" style="border:none;border-radius:12px;overflow:hidden;">
      <div class="modal-header" style="background:linear-gradient(135deg,#660000,#8b1a1a);color:#fff;border:none;">
        <button type="button" class="close" data-dismiss="modal" style="color:#fff;opacity:.9;">&times;</button>
        <h4 class="modal-title"><span class="glyphicon glyphicon-picture"></span>&nbsp;รูปอุปกรณ์</h4>
      </div>
      <div class="modal-body" style="text-align:center;background:#f7fafc;padding:18px;">
        <img id="imgZoomBig" src="" style="max-width:100%;max-height:70vh;border-radius:8px;" />
      </div>
    </div>
  </div>
</div>

<div class="overlay" id="loadingOverlay" style="display:none;">
    <div class="overlayContent">
        <div class="spin-ring"></div>
        <div class="ov-title">กำลังประมวลผล...</div>
        <div class="ov-sub">กรุณารอสักครู่ ระบบกำลังดึงข้อมูล</div>
        <div class="ov-bar"><i></i></div>
    </div>
</div>

</ContentTemplate>
</asp:UpdatePanel>
</asp:Content>