<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="storage.aspx.cs"
    Inherits="Tool_Equipment_Control_System.Storage" EnableEventValidation="false" %>

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
    <%--<script src="Scripts/jquery-1.9.1.js"></script>--%>
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
                "⚠️ ยืนยันการส่งรายการการแจ้งเปลี่ยนแปลง\n" +
                "คุณต้องการส่งการร้องขอการแจ้งเปลี่ยนแปลงอุปกรณ์หรือไม่?\n" +
                "กด OK เพื่อดำเนินการต่อ"
            );
        }
        function Reject() {
            return confirm(
                "⚠️ ยืนยันการ Reject การแจ้งเปลี่ยนแปลง\n" +
                "คุณต้องการ Reject การแจ้งเปลี่ยนแปลงอุปกรณ์หรือไม่?\n" +
                "กด OK เพื่อดำเนินการต่อ"
            );
        }
        function AADConfirm() {
            return confirm(
                "⚠️ อนุมัติรายการการแจ้งเปลี่ยนอุปกรณ์\n" +
                "คุณต้องการอนุมัติการแจ้งเปลี่ยนแปลงอุปกรณ์หรือไม่?\n" +
                "กด OK เพื่อดำเนินการต่อ"
            );
        }
        function J3Approve() {
            return confirm(
                "⚠️ อนุมัติรายการการแจ้งเปลี่ยนอุปกรณ์\n" +
                "คุณต้องการอนุมัติการแจ้งเปลี่ยนแปลงอุปกรณ์หรือไม่?\n" +
                "กด OK เพื่อดำเนินการต่อ"
            );
        }
        function confirmRemove() {
            return confirm(
                "⚠️ ลบรายการออกจาก Change List\n" +
                "คุณต้องการลบรายการออกจาก Change List หรือไม่?\n" +
                "กด OK เพื่อดำเนินการต่อ"
            );
        }
        function confirmCancel() {
            return confirm(
                "⚠️ ยืนยันการยกเลิกการร้องขอแจ้งเปลี่ยนแปลง\n" +
                "คุณต้องการยกเลิกรายการนี้หรือไม่?\n" +
                "กด OK เพื่อดำเนินการต่อ"
            );
        }
    </script>   

    <%--Count Change Comment--%>    
    <script>
        function countChar(val) {
            var len = val.value.length;

            document.getElementById('charCount').innerText = len;

            // ✅ เปลี่ยนสีถ้าใกล้เต็ม (optional)
            if (len > 900) {
                document.getElementById('charCount').style.color = 'red';
            } else {
                document.getElementById('charCount').style.color = '#666';
            }
        }
    </script>

    <%--Drop Down Search--%>
    <script>
        function initSelect2() {
            $('.ddlSearch').select2({
                width: 'resolve'
                /*placeholder: 'เลือก Model',*/
                //allowClear: true
            });
        }

        // โหลดครั้งแรก
        $(document).ready(function () {
            initSelect2();
        });

        // 🔥 สำคัญ: ถ้า GridView Postback
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            initSelect2();
        });

        $(document).on('select2:open', function () {
            let searchField = document.querySelector('.select2-container--open .select2-search__field');

            if (searchField) {
                searchField.focus();
            }
        });
    </script>

    <%--Add New Model--%>
    <script>
        $(document).on("click", ".btnToggle", function () {

            var container = $(this).closest("div");

            var ddl = container.find("select");
            var txt = container.find("input[type='text']");
            var hidden = container.find("input[type='hidden']");
            var btn = $(this);


            if (ddl.is(":visible")) {

                // destroy select2 ก่อนซ่อน
                if (ddl.hasClass("select2-hidden-accessible")) {
                    ddl.select2("destroy");
                }

                ddl.hide();
                txt.show();

                //var selectedText = ddl.find("option:selected").text();
                //txt.val(selectedText);

                btn.text("-");

                // ✅ เก็บ state
                hidden.val("txt");

            } else {

                ddl.show();
                txt.hide();

                btn.text("+");

                // ✅ init select2 กลับ
                ddl.select2({
                    width: 'resolve'
                });

                // ✅ เก็บ state
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

                    // destroy select2 ก่อนซ่อน
                    if (ddl.hasClass("select2-hidden-accessible")) {
                        ddl.select2("destroy");
                    }

                    ddl.hide();
                    txt.show();
                    btn.text("-");

                    // ✅ เก็บ state
                    hidden.val("txt");

                } else {

                    ddl.show();
                    txt.hide();
                    btn.text("+");

                    // ✅ init select2 กลับ
                    ddl.select2({
                        width: 'resolve'
                    });

                    // ✅ เก็บ state
                    hidden.val("ddl");
                }
            });
        }

        // ✅ run ตอนโหลด
        $(document).ready(function () {
            restoreModelState();
        });

        // ✅ run หลัง UpdatePanel/PostBack
        if (typeof (Sys) !== "undefined") {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                restoreModelState();
            });
        }
    </script>

    <%--Show & HideLoading--%>
    <script>
        function showFullOverlay(title, sub) {
            document.getElementById('fullOvTitle').innerText = title || 'กำลังบันทึก...';
            document.getElementById('fullOvSub').innerText = sub || 'กรุณารอสักครู่';
            document.getElementById('fullOverlay').style.display = 'flex';
        }
        function hideFullOverlay() {
            var el = document.getElementById('fullOverlay');
            if (el) el.style.display = 'none';
        }
        // ปิด fullOverlay หลัง async postback จบทุกครั้ง
        if (typeof (Sys) !== "undefined") {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(hideFullOverlay);
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

    <%--Scroll & Focus ช่องที่แจ้งเตือน--%>
    <script>
        function scrollAndFocusWarning(rowId, fieldId) {
            setTimeout(function () {
                var row = document.getElementById(rowId);
                if (row) {
                    row.scrollIntoView({ behavior: 'smooth', block: 'center' });
                }

                if (fieldId) {
                    // รอให้ scroll เสร็จก่อนค่อย focus
                    setTimeout(function () {
                        var field = document.getElementById(fieldId);
                        if (!field) return;

                        // ถ้าเป็น select2 → เปิด dropdown เลย มองเห็นชัดกว่า focus เฉยๆ
                        if ($(field).hasClass('select2-hidden-accessible')) {
                            try { $(field).select2('open'); } catch (e) { }
                        } else if (typeof field.focus === 'function') {
                            field.focus();
                        }
                    }, 350);
                }
            }, 100);
        }
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
            background: #e8ffec;
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
        table.data-table tbody tr { border-bottom: 1px solid #d5d5d5; }
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
            background: #d3dda6a1;
            border-top: 1px solid var(--border);
            box-shadow: 0 -4px 20px rgba(0,0,0,.08);
            padding: 10px 24px;
            display: flex; align-items: center;
            justify-content: space-between; gap: 12px;
            z-index: 100;
            border-radius:10px;
            border:1px solid #a4b06ac9;
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

    <%--Loading Overlay--%>
    <style>
        .overlay{position:fixed;inset:0;background:rgba(15,23,42,.45);z-index:9998;
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

    <%--Drop Down Search (rank matcher)--%>
    <script>
        // ลบตัวคั่นออกก่อนเทียบ
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

        function initSelect2() {
            $('.ddlSearch').each(function () {
                var $el = $(this);
                if ($el.hasClass('select2-hidden-accessible')) {
                    $el.select2('destroy');
                }
                $el.select2({
                    width: 'resolve',
                    dropdownAutoWidth: true,
                    sorter: rankSorter,
                    matcher: rankMatcher
                });
            });
        }

        $(document).ready(function () {
            initSelect2();
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            initSelect2();
        });

        $(document).on('select2:open', function () {
            setTimeout(function () {
                var searchField = document.querySelector('.select2-container--open .select2-search__field');
                if (searchField) searchField.focus();
            }, 0);
        });
    </script>
     
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
            grid-template-columns: 70px auto 180px;
            grid-template-rows: 50px;
            background-color:#bfcc81;
            border-radius:10px;
            margin: 10px 10px 10px 10px;
        }
        .grid_search{
            display: grid;
            grid-template-columns: 320px Auto;
            gap: 5px;
            align-items: center;
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

    <%--Modal : Check Column--%>
    <style>
      .modal-overlay {
        position: fixed; inset: 0; background: rgba(0,0,0,0.45);
        z-index: 10000; align-items: center; justify-content: center;
      }
      .modal-box {
        background: #fff; border-radius: 10px; width: 700px; max-width: 92vw;
        max-height: 85vh; display: flex; flex-direction: column;
        box-shadow: 0 10px 40px rgba(0,0,0,0.3);
      }
      .modal-head {
        background: linear-gradient(135deg,#1a3a5c 0%,#2980b9 100%);
        color:#fff; padding:12px 18px; border-radius:10px 10px 0 0;
        display:flex; align-items:center; justify-content:space-between;
        font-weight:600;
      }
      .modal-x { background:none; border:none; color:#fff; font-size:24px; cursor:pointer; line-height:1; }
      .modal-content { padding:14px 18px; overflow-y:auto; }
      .modal-search {
        width:100%; padding:7px 12px; margin-bottom:10px;
        border:1.5px solid #c8d6e5; border-radius:6px; font-size:13px;
      }
      .modal-foot {
        padding:10px 18px; border-top:1px solid #e8eef4;
        display:flex; justify-content:flex-end;
      }
      .repair-radio label{display:block; font-size:15px; font-weight:500; margin:6px 0; cursor:pointer;}
        .repair-radio input[type=radio]{width:18px; height:18px; margin-right:8px; vertical-align:middle; cursor:pointer;}
    </style>

    <%--style column gvStoragelist (zone divider)--%>
    <style>
        .zone-divider { border-left: 3px solid #1a3a5c !important; }
        .col-current { background-color: #eafaf1 !important; color: #555; font-size: 12px; text-align: center; }
        .col-request { background-color: #dbffea !important; text-align: center; }
        .col-repair { background-color: #e1f3fe  !important; text-align: center; }
        .col-storage { background-color: #ffebeb  !important; text-align: center; }

        .hdr-current { background-color: #6c757d !important; color: #fff !important; font-weight: 700; text-align: center; padding: 4px; }
        .hdr-request { background-color: #27ae60 !important; color: #fff !important; font-weight: 700; text-align: center; padding: 4px; }
        .hdr-repair { background-color: #2980b9 !important; color: #fff !important; font-weight: 700; text-align: center; padding: 4px; }
        .hdr-storage { background-color: #ff7878 !important; color: #fff !important; font-weight: 700; text-align: center; padding: 4px; }
        .hdr-use.zone-divider { border-left: 3px solid #1a3a5c !important; }
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
  

    <asp:ScriptManager runat="server" ID="scriptManager1" EnablePageMethods="true"></asp:ScriptManager>

    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UP1">
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
            <div class="ov-title" id="fullOvTitle">กำลังบันทึก...</div>
            <div class="ov-sub" id="fullOvSub">กำลังบันทึกข้อมูล กรุณารอสักครู่</div>
            <div class="ov-bar"><i></i></div>
        </div>
    </div>

    <asp:UpdatePanel runat="server" ID="UP1">

        <ContentTemplate>                       

            <%---- Toast Container ----%>
            <div id="toast-container" aria-live="polite" aria-atomic="true"></div>

            <div>

                <%--Header--%>
                <div class="grid-headerTopic">
                    <div style="text-align:center; align-content:center;">
                        <img class="" style="height:38px;" src="images/storage.png" />
                    </div>
    
                    <label class="header_Style1" style="">
                        จัดเก็บอุปกรณ์ : Jig Tool / Equipment </label>

                    <asp:Label ID="lb_mode" runat="server" Text="Mode : Issue"
                        style=" text-align: center;
                                align-content: center;                                
                                background: #fff;
                                margin: 10px 10px;
                                border-radius: 30px;
                                font-weight: 600;
                                color: #4f4b4b;">
                    </asp:Label>
                </div>

                <%--=====================================================--%>
                <%------------ SECTION 1 : Employee ID (Issue By) ---------%>
                <%--=====================================================--%>

                <div class="card" style="margin-left:10px; margin-right:10px;">

                    <div class="card-header">
                        <span>
                            <asp:Literal ID="step_requester" runat="server" Text="<span class='badge'>Step 1</span>" />
                             &nbsp;Requester : ลงชื่อผู้ร้องขอ
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
                             &nbsp;Search & Add List : ค้นหาข้อมูลและเพิ่มรายการสำหรับร้องขอการจัดเก็บอุปกรณ์
                        </span>

                    </div>

                    <div class="card-body" style="margin-bottom:10px;">

                        <!-- Tab: Search -->
                        <div id="tabFixAsset" class="grid_search">

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
                                                    
                            <%--Search Note--%>
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

                                        <%--Display Status--%>
                                        <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="headerGv1" ItemStyle-Width="140px"
                                            ItemStyle-CssClass="rowGv1" ItemStyle-HorizontalAlign="Center">
                                            <ItemTemplate>   
                                                <asp:HiddenField ID="hdstatusId" runat="server" Value='<%# Eval("status_id") %>' />
    
                                                <span class='badge <%# GetStatusBadgeClass(Eval("DisplayStatus").ToString()) %>'>
                                                    <%# Eval("DisplayStatus") %>
                                                </span>

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
                <%------------ SECTION 3 : Storage List -------------------%>
                <%--=====================================================--%>
                <div class="card" style="margin-left:10px; margin-right:10px;">

                    <div class="card-header">
                        <span>
                            <asp:Literal ID="step_Storagelist" runat="server" Text="<span class='badge'>Step 3</span>" />
                             &nbsp;Storage List : รายการที่ร้องขอจัดเก็บอุปกรณ์ , ลงข้อมูลการร้องขอจัดเก็บอุปกรณ์
                        </span>

                        <span style="margin-left:auto; font-size:11px; font-weight:400; color:#555;">
                            <asp:Literal ID="request_remark" runat="server" 
                                Text=" <span class='badge' style='background:#e4ff5d; color:#000; font-size:12px; text-shadow: none; '> 
                                หมายเหตุ : [Auto] Use : Dept = AAD (5505) </span>" />
                        </span>
                    </div>

                    <div class="card-body" style="margin-bottom:10px;">
                        
                        <%--Storage List--%>
                        <div class="tbl-wrap" style="max-height: 350px;">
                            <asp:GridView ID="gvStorageList" runat="server"
                                CssClass="data-table change-table fixed-header"
                                style="min-width:2400px;"
                                AutoGenerateColumns="false"
                                OnRowDataBound="gvStorageList_RowDataBound"
                                OnRowCommand="gvStorageList_RowCommand"
                                EmptyDataText="ไม่มีรายการแจ้งเปลี่ยนแปลง">

                                <EmptyDataRowStyle CssClass="no-data" />

                                <Columns>

                                    <%--Item No--%>
                                    <asp:BoundField  DataField="RowNo"  
                                                     HeaderText="#" HeaderStyle-CssClass="headerGv1"
                                                     ItemStyle-CssClass="rowGv1" ItemStyle-Width="40px" />

                                    <%--Check Box--%>
                                 <%--   <asp:TemplateField HeaderText="Check" ItemStyle-Width="65px"
                                        HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1">
                                        <ItemTemplate>
                                            
                                        </ItemTemplate>
                                    </asp:TemplateField>--%>

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
                                            <asp:HiddenField ID="hdtoolSub" runat="server" Value='<%# Eval("ToolSub") %>' />
                                            <asp:HiddenField ID="hdSystemId" runat="server" Value='<%# Eval("SystemId") %>' />
                                            <asp:LinkButton ID="btn_Picture" title="View Picture" runat="server"
                                                OnClick="btn_pic_Click" CssClass="img_tool">
                                                <img src="images/photo-gallery.png" />
                                            </asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%--Fix Asset--%>
                                    <asp:BoundField DataField="FixAssetNo" ItemStyle-Width="110px"
                                        HeaderText="Fix Asset" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />

                                    <%--Control No--%>
                                    <asp:BoundField DataField="ControlNo" ItemStyle-Width="120px"
                                        HeaderText="Control No" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />

                                    <%--Tool No--%>
                                    <asp:BoundField DataField="ToolNo"
                                        HeaderText="Tool No / Model" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv_toolNo" />

                                    <%--Sub--%>
                                    <asp:BoundField DataField="ToolSub" ItemStyle-Width="45px"
                                        HeaderText="No" HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />                                     

                                    <%--═══════════ โซน Status ═══════════--%>

                                    <%--Status--%>
                                    <asp:TemplateField HeaderText="Status" ItemStyle-Width="120px"
                                        HeaderStyle-CssClass="hdr-request zone-divider" ItemStyle-CssClass="col-request zone-divider">
                                        <ItemTemplate>
                                            <div class="d-flex align-items-center"">
                                                <asp:DropDownList ID="ddlStatus" runat="server"
                                                    CssClass="form-control form-control-sm ddl_edit" Style="width:100%;">

                                                    <asp:ListItem Selected="True" Value="0"> - Select - </asp:ListItem>
                                                    <asp:ListItem Value="Spare"> Spare </asp:ListItem>
                                                    <asp:ListItem Value="Not Use"> Not Use </asp:ListItem>

                                                </asp:DropDownList>
                                            </div>  
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%--OK/NG--%>
                                    <asp:TemplateField HeaderText="OK/NG" ItemStyle-Width="120px"
                                        HeaderStyle-CssClass="hdr-request" ItemStyle-CssClass="col-request">
                                        <ItemTemplate>
                                            <div class="d-flex align-items-center">
                                                <asp:DropDownList ID="ddlOKNG" runat="server"
                                                    CssClass="form-control form-control-sm ddl_edit" Style="width:100%;"
                                                    AutoPostBack="true" OnSelectedIndexChanged="ddlOKNG_SelectedIndexChanged">
                                                    <asp:ListItem Selected="True" Value="0"> - Select - </asp:ListItem>
                                                    <asp:ListItem Value="OK"> OK </asp:ListItem>
                                                    <asp:ListItem Value="NG"> NG </asp:ListItem>
                                                </asp:DropDownList>
                                            </div>        
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%--═══════════ โซน Repair Status ═══════════--%>

                                    <%--Repair Status--%>
                                    <asp:TemplateField HeaderText="Repair Status" ItemStyle-Width="160px"
                                        HeaderStyle-CssClass="hdr-request zone-divider" ItemStyle-CssClass="col-request zone-divider">
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlRepairStatus" runat="server"
                                                CssClass="form-control form-control-sm ddl_edit" Style="width:100%;">
                                                <asp:ListItem Selected="True" Value=""> - Select - </asp:ListItem>
                                                <asp:ListItem Value="ซ่อม"> ซ่อม </asp:ListItem>
                                                <asp:ListItem Value="ไม่ซ่อม/ซ่อมไม่ได้"> ไม่ซ่อม/ซ่อมไม่ได้ </asp:ListItem>
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%--Confirm By (ddl + search)--%>
                                    <asp:TemplateField HeaderText="Confirm By" ItemStyle-Width="170px"
                                        HeaderStyle-CssClass="hdr-request" ItemStyle-CssClass="col-request">
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlConfirmBy" runat="server"
                                                CssClass="form-control form-control-sm ddl_edit ddlSearch" Style="width:100%;">
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%--Service Unit--%>
                                    <asp:TemplateField HeaderText="Service Unit" ItemStyle-Width="170px" 
                                        HeaderStyle-CssClass="hdr-request zone-divider" ItemStyle-CssClass="col-request zone-divider">                                    
                                        <ItemTemplate>                                    
                                            <asp:TextBox ID="txt_service" runat="server" 
                                                CssClass="form-control"
                                                MaxLength="100"
                                                style="width:100%; text-align:center; margin-bottom:5px; text-transform:uppercase;"
                                                placeholder="Service">
                                            </asp:TextBox>                                                                                           
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%--Reason--%>
                                    <asp:TemplateField HeaderText="เหตุผล" ItemStyle-Width="170px" 
                                        HeaderStyle-CssClass="hdr-request zone-divider" ItemStyle-CssClass="col-request zone-divider">                                    
                       
                                        <ItemTemplate>                                    
                                            <asp:DropDownList ID="ddlReason" runat="server"
                                                CssClass="form-control form-control-sm ddl_edit" Style="width:100%;">
                                            </asp:DropDownList>                                                                                           
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    
                                                                        

                                    

                                    <%--═══════════ โซน USE (4 ช่อง) ═══════════--%>

                                    <%--Storage Dept = txtDeptCode  (เริ่มโซน → zone-divider)--%>
                                    <asp:TemplateField HeaderText="Storage : Dept"
                                        HeaderStyle-CssClass="hdr-storage zone-divider" ItemStyle-CssClass="col-storage zone-divider"
                                        ItemStyle-Width="90px">
                                        <ItemTemplate>
                                            <asp:Label ID="lblDeptNew" runat="server" />
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%-- Storage Cell (dropdown)--%>
                                    <asp:TemplateField HeaderText="Storage : Cell" HeaderStyle-CssClass="hdr-storage" ItemStyle-CssClass="col-storage">
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

                                    <%-- Storage Station (dropdown)--%>
                                    <asp:TemplateField HeaderText="Storage : Station" HeaderStyle-CssClass="hdr-storage" ItemStyle-CssClass="col-storage">
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

                                    <%--═══════════ โซน CURRENT (4 ช่อง, label อย่างเดียว) ═══════════--%>

                                    <%--Current Dept (เริ่มโซน → zone-divider)--%>
                                    <asp:BoundField DataField="DeptCode" ItemStyle-Width="80px" HeaderText="Dept"
                                        HeaderStyle-CssClass="headerGv1 zone-divider" ItemStyle-CssClass="rowGv1 zone-divider" />

                                    <%--Current Model--%>
                                    <asp:BoundField DataField="ModelName" ItemStyle-Width="130px" HeaderText="Model"
                                        HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />

                                    <%--Current Cell--%>
                                    <asp:BoundField DataField="CellName" ItemStyle-Width="170px" HeaderText="Cell"
                                        HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />

                                    <%--Current Station--%>
                                    <asp:BoundField DataField="StationName" ItemStyle-Width="150px" HeaderText="Station"
                                        HeaderStyle-CssClass="headerGv1" ItemStyle-CssClass="rowGv1" />
                                    

                                </Columns>

                            </asp:GridView>
                        </div> 
                        
                        <%--Note--%>
                        <div ID="Div1" runat="server">
                            <div class="form-row" style="align-content:center;">
                                <div class="page-note">
                                    <span class="badge" style="background:#ff5d5d;">หมายเหตุ :</span>
                                    <div style="display: inline-grid;text-align-last: left; gap:5px;">
                                       <span class="badge" style="background:#e4ff5d; color:#000; font-size:13px;">1.กรณี EOL ให้ตรวจสอบและระบุ Unit ที่เป็น Service</span>
                                       <span class="badge" style="background:#e4ff5d; color:#000; font-size:13px;">2.กรณี Tool NG ต้องได้รับการ Confirm จากทาง ENG</span>
                                       <span class="badge" style="background:#e4ff5d; color:#000; font-size:13px;">3.ระบุการจัดการให้ชัดเจน กรณีที่ต้องการจัดเก็บเป็น Spare ต้องระบุ Model ,Cell ที่ใช้งาน</span>
                                       <span class="badge" style="background:#e4ff5d; color:#000; font-size:13px;">4.กรณีที่ไม่เก็บเป็น spare ไม่ใช้งานกับ Model อื่น ให้ยืนยันการ Scrap</span>
                                    </div>                                     
                                </div>
                            </div>
                        </div>

                    </div>
  
                </div>

                <%--=====================================================--%>
                <%------------ SECTION 4 : Comment Withdraw -----------------%>
                <%--=====================================================--%>

                <asp:Panel ID="pnlComment" runat="server" CssClass="card" style="margin-left:10px; margin-right:10px;">
                    <div class="card" style="margin-left:10px; margin-right:10px;">

                        <div class="card-header">
                            <span>
                                <asp:Literal ID="step_comment" runat="server" Text="<span class='badge'>Step 4</span>" />
                                 &nbsp;Comment : เหตุผลการแจ้งเปลี่ยนแปลง
                            </span>

                        </div>

                        <div class="card-body" style="margin-bottom:10px;">                        

                            <div class="form-row" style="align-items: self-start;">

                                <%--Dropdown : Reason Withdraw--%>
                                <div class="form-group" style="width:180px; margin-left:10px; gap:5px;">
                                    <label>เหตุผลการแจ้งเปลี่ยนแปลง *</label>
                                    <asp:DropDownList ID="ddlReason_Storage"
                                        runat="server"
                                        CssClass="form-control form-control-sm ddl_edit"
                                        Style="min-width:170px; padding: 2px 5px;">
                                    </asp:DropDownList>
                                </div>

                                <%-- Reason Withdraw : Comment --%>
                                <div class="form-group" style="width: calc(100% - 250px); margin-left:10px;">
                                    <span style="text-align:left; font-size:12px; font-weight:600; color:#555;">Comment &nbsp;
                                        <span style="text-align:right; font-size:12px; font-weight:500; color:#666;">
                                            (จำนวนตัวอักษร :
                                            <span id="charCount" style="color:#ff0303;">
                                                0
                                            </span>
                                            / 1000 )
                                        </span>
                                    </span>

                                    <asp:TextBox ID="txt_StorageComment" runat="server" 
                                        TextMode="MultiLine"
                                        Rows="4"
                                        CssClass="form-control"
                                        MaxLength="1000"
                                        onkeyup="countChar(this)"
                                        style ="width:100%; Height:50px !important;"
                                        placeholder="กรุณาใส่เหตุผลในการร้องข้อยืม">

                                    </asp:TextBox>
                                </div>

                            </div>
                        
                        </div>

                    </div>
                </asp:Panel>

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
                        <span ID="rejectH" runat="server" style="text-align:left; font-size:14px; font-weight:500;">Comment</span>
        
                        <span ID="rejectCountAlert" runat="server" style="text-align:right; font-size:14px; font-weight:500; color:#666;">
                            (จำนวนตัวอักษร :
                            <span id="charCount" style="color:#ff0303;">
                                0
                            </span>
                            / 1000 )
                        </span>

                        <asp:TextBox ID="txt_rejectComment" runat="server" 
                            TextMode="MultiLine"
                            Rows="4"
                            CssClass="form-control"
                            MaxLength="1000"
                            onkeyup="countChar(this)"
                            style ="width:100%; Height:60px !important;"
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
                                    <%--No--%>
                                    <asp:BoundField  DataField="Reject_Count" ItemStyle-Width="60px"     
                                                     HeaderText="No" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1"/>

                                    <%--Type--%>
                                    <asp:BoundField  DataField="Type" ItemStyle-Width="60px"     
                                                     HeaderText="Type" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1"/>

                                    <%--Reject By--%>
                                    <asp:BoundField  DataField="Reject_by" ItemStyle-Width="150px"     
                                                     HeaderText="By" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1"/>

                                    <%--Reject Process--%>
                                    <asp:BoundField  DataField="status_name" ItemStyle-Width="120px"     
                                                     HeaderText="Process" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1"/>

                                    <%--Reject Date--%>
                                    <asp:BoundField  DataField="Reject_Date" ItemStyle-Width="120px"     
                                                     HeaderText="Date" HeaderStyle-CssClass="headerGvOld" ItemStyle-CssClass="rowGv1"
                                                     DataFormatString="{0:dd-MMM-yyyy}"/>

                                    <%--Reject Detail--%>
                                    <asp:BoundField  DataField="Reject_detail" 
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
                                <asp:LinkButton ID="btnConfirm" runat="server" 
                                    CssClass="btn btn-success" OnClick="btnConfirm_Click"
                                    OnClientClick="return AADConfirm();" data-toggle="tooltip" data-placement="top"
                                    title="ยืนยันการร้องขอ">                                                                        
                                    <span class="glyphicon glyphicon-ok-sign"></span>
                                        AAD Confirm
                                </asp:LinkButton>
                                <span class="tooltip-box">ยืนยันการร้องขอ</span>  
                            </span>
                        </div>

                        <%-- Approve Button --%>
                        <div id="ApproveButton" runat="server" Visible="false">
                            <span class="tooltip-wrap" style="">
                                <asp:LinkButton ID="btnApprove" runat="server"
                                    CssClass="btn btn-success" OnClick="btnApprove_Click"
                                    OnClientClick="return Approve();" data-toggle="tooltip" data-placement="top"
                                    title="อนุมัติการร้องขอ">                                                                        
                                    <span class="glyphicon glyphicon-ok-sign"></span>
                                        Approve
                                </asp:LinkButton>
                                <span class="tooltip-box">อนุมัติการร้องขอ</span>  
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

        </ContentTemplate>

        <Triggers>
        </Triggers>

    </asp:UpdatePanel> 
    
    <%-- ===== วาง modal ตรงนี้ นอก UP1 ===== --%>
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
            if (!tbody) {           // กัน null ตรงจุดที่ error เดิมเกิด
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


