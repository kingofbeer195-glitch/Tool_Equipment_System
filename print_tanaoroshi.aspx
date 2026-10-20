<%@ Page Language="C#" AutoEventWireup="true"
    CodeFile="print_tanaoroshi.aspx.cs"
    Inherits="Tool_Equipment_Control_System.print_tanaoroshi" %>
<!DOCTYPE html>
<html>
<head runat="server">
<meta charset="UTF-8">
<title>Calibration & Fixed Asset Monthly Check</title>

<style>
    @page { size: A5 portrait; margin: 6mm 10mm; }  
    /*@page:first {margin: 6mm 10mm; }*/  

    body { font-family: Calibri, Arial, sans-serif; font-size: 7.5pt; margin: 0; }  /* ข้อ 4 : ลด font */

    /* แต่ละหน้า = 1 sheet เต็มหน้ากระดาษพอดี */
    .sheet {
        position: relative;
        box-sizing: border-box;
        min-height: 100%;
        page-break-after: always;
    }
    .sheet:last-child { page-break-after: auto; }

    /* ข้อ 1-2 : เลขหน้ามุมขวาบนสุด ทุกหน้า อยู่แถวแรกของกระดาษ */
    .page-no {
        position: absolute; top: 0; right: 0;
        font-size: 8pt; font-weight: bold;
    }

    .head-tbl { width: 100%; margin: 14px 0 6px; }  /* เว้นบนให้พ้นเลขหน้า */
    .head-tbl td { /*vertical-align: top;*/ font-size: 8pt; }
    .title { font-size: 10pt; font-weight: bold;}
    .sign { text-align: right; }
    .sign-line { display: inline-block; border-bottom: 1px solid #000; width: 90px; }

    table.data { width: 100%; border-collapse: collapse; table-layout:fixed;}
    table.data th {
        background: #808080; color: #fff; font-size: 7pt; font-weight: bold;
        border: 1px solid #555; padding: 3px 2px; text-align: center;
    }
    table.data td {
        border: 1px solid #999; padding: 2px; text-align: center;
        vertical-align: middle; font-size: 7pt; word-break: break-word; height: 52px;
    }
    table.data td , table.data th {
        word-break: break-word; overflow:hidden;
    }

    table.data thead { display: table-header-group; }
    table.data tr { page-break-inside: avoid; }
    table.data img { max-width: 65px; max-height: 42px; }

    .c-no { width: 4%; } .c-asset { width: 14%; } .c-ctrl { width: 12%; }
    .c-pic { width: 13%; } .c-name { width: 22%; } .c-model { width: 9%; }
    .c-cell { width: 8%; } .c-stn { width: 9%; } .c-chk { width: 9%; }

    .blank-sheet { page-break-after: always; }

    /* เว้นบนสำหรับหน้าที่ 2,3... ของ List เดียวกัน (ไม่มี title block) */
    .cont-gap { height: 20px; }

    @media screen {
        body { background: #eee; padding: 20px; }
        .sheet { background: #fff; padding: 14px; margin: 0 auto 16px; max-width: 148mm;
                 min-height: 200mm; box-shadow: 0 2px 8px rgba(0,0,0,.2); }
        .no-print { text-align: center; margin-bottom: 16px; }
        .no-print button { padding: 8px 20px; font-size: 14px; cursor: pointer;
                           background: #1a6e82; color: #fff; border: none; border-radius: 5px; }
    }
    @media print {
        .no-print { display: none; }
        .sheet { filter: grayscale(100%); }
        table.data th { background: #808080 !important; color: #fff !important;
                        -webkit-print-color-adjust: exact; print-color-adjust: exact; }
    }

    /* N/M lists เหนือ Checked By (เฉพาะหน้าแรกของ List) */
    .list-no { font-size: 9pt; font-weight: bold; margin-top: 24px;margin-bottom: 14px; }
</style>

</head>
<body>
<form id="form1" runat="server">
    <div class="no-print">
        <button type="button" onclick="window.print();">🖨 Print</button>
        <button type="button" onclick="window.close();">Close</button>
    </div>

    <asp:Literal runat="server" ID="ltl_content"></asp:Literal>
</form>

<script>

    //function padOddSheets() {
    //    // ความสูงพื้นที่พิมพ์ A5 (297mm/2 - margin) ≈ 132mm ≈ 499px @96dpi
    //    var PAGE_H = 499;

    //    document.querySelectorAll('.sheet').forEach(function (s) {
    //        var pages = Math.ceil(s.offsetHeight / PAGE_H);
    //        if (pages % 2 === 1) {                     // หน้าคี่ → เติมหน้าว่าง 1 หน้า
    //            var blank = document.createElement('div');
    //            blank.style.cssText = 'page-break-before:always;height:1px;';
    //            s.parentNode.insertBefore(blank, s.nextSibling);
    //        }
    //    });
    //}

    //window.onload = function () {
    //    setTimeout(function () {
    //        padOddSheets();
    //        window.print();
    //    }, 800);
    //};

    window.onload = function () {
        // รอรูปทุกใบโหลดเสร็จก่อน (กัน print ตอนรูปยังไม่ขึ้น)
        var imgs = Array.prototype.slice.call(document.images);
        var pending = imgs.filter(function (im) { return !im.complete; }).length;

        function done() {
            // แจ้งหน้าแม่ให้ปิด overlay
            try { if (window.opener && window.opener.printReady) window.opener.printReady(); } catch (e) { }
            setTimeout(function () { window.print(); }, 300);
        }

        if (pending === 0) { done(); return; }

        imgs.forEach(function (im) {
            if (im.complete) return;
            im.addEventListener('load', checkDone);
            im.addEventListener('error', checkDone);   // รูปเสียก็นับว่าเสร็จ ไม่ให้ค้าง
        });

        function checkDone() {
            pending--;
            if (pending <= 0) done();
        }

        // safety timeout : ถ้ารูปบางใบค้าง เกิน 15 วิ ก็ print เลย
        setTimeout(done, 15000);
    };

</script>
</body>
</html>