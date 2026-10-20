<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="Equipment_Add_New.aspx.cs"
    Inherits="Tool_Equipment_Control_System.Equipment_Add_New" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>New Equipment Registration</title>

    <link href="CSS/bootstrap.css" rel="stylesheet" />
    <link href="CSS/w3v4.10.css" rel="stylesheet" />
    <link href="DataTables-1.10.18/css/dataTables.css" rel="stylesheet">
    <link href="Content/chosen.css" rel="stylesheet">

    <script src="Scripts/chosen.jquery.js"></script>
    <script src="Scripts/jquery-1.9.1.js"></script>
    <script src="Scripts/script.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>
    <script src="DataTables-1.10.18/js/dataTables.js"></script>
    <script src="DataTables-1.10.18/FixedColumns-3.2.5/js/fixedColumns.dataTables.js"></script>

    <style>
        /* ══════════════════════════════════════
           RESET & VARIABLES
        ══════════════════════════════════════ */
        *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

        :root {
            --bg:           #f0f2f5;
            --surface:      #ffffff;
            --card:         #ffffff;
            --card-head:    #f7f8fa;
            --border:       #e2e6ed;
            --border-light: #edf0f4;
            --accent:       #2563eb;
            --accent-lt:    #eff4ff;
            --accent2:      #059669;
            --accent2-lt:   #ecfdf5;
            --danger:       #dc2626;
            --danger-lt:    #fff1f1;
            --warn:         #d97706;
            --warn-lt:      #fffbeb;
            --text:         #1e293b;
            --text-sub:     #475569;
            --muted:        #94a3b8;
            --radius:       10px;
            --radius-sm:    6px;
            --shadow-sm:    0 1px 3px rgba(0,0,0,.08), 0 1px 2px rgba(0,0,0,.04);
            --shadow:       0 4px 16px rgba(0,0,0,.08);
            --shadow-lg:    0 8px 32px rgba(0,0,0,.10);
        }

        body {
            background: var(--bg);
            color: var(--text);
            font-size: 14px;
            font-family: 'Segoe UI', Tahoma, sans-serif;
            min-height: 100vh;
        }

        /* ══════════════════════════════════════
           LAYOUT
        ══════════════════════════════════════ */
        .page-wrapper {
            width: calc(100% - 20px);
            margin: 30px 10px 0 10px;
            padding-bottom: 80px; 
        }

        /* ── Header ── */
        .page-header {
            background: linear-gradient(135deg, #1d4ed8 0%, #2563eb 60%, #3b82f6 100%);
            color: #fff;
            padding: 5px 24px 12px;
            border-radius: var(--radius);
            margin-bottom: 10px;
            box-shadow: 0 4px 20px rgba(37,99,235,.30);
            display: flex;
            align-items: center;
            gap: 14px;
        }
        .page-header img { height: 38px; filter: drop-shadow(0 2px 4px rgba(0,0,0,.3)); }
        .page-header h3  { font-size: 24px; font-weight: 700; margin: 0; }
        .page-header p   { font-size: 13px; opacity: .80; margin: 2px 0 0; }

        /* ── Import Card ── */
        .import-card {
            background: var(--card);
            border: 1px solid var(--border);
            border-radius: var(--radius);
            box-shadow: var(--shadow-sm);
            margin-bottom: 10px;
            overflow: hidden;
        }
        .import-card-head {
            background: var(--card-head);
            border-bottom: 1px solid var(--border);
            padding: 10px 20px;
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 12px;
        }
        .import-card-head-left {
            display: flex;
            align-items: center;
            gap: 10px;
        }
        .card-icon {
            width: 38px; height: 38px;
            border-radius: var(--radius-sm);
            background: var(--accent-lt);
            display: flex; align-items: center; justify-content: center;
            font-size: 18px;
            flex-shrink: 0;
        }
        .card-title { font-size: 16px; font-weight: 600; color: var(--text); }
        .card-sub   { font-size: 12px; color: var(--text-sub); margin-top: 1px; }
        .import-card-body { padding: 16px 20px; }

        /* ── Drag & Drop Zone ── */
        #dzWrapper { transition: all .35s ease; overflow: hidden; }
        #dzWrapper.dz-hidden { max-height: 0 !important; opacity: 0; margin: 0; padding: 0; }

        .dropzone {
            border: 2px dashed var(--border);
            border-radius: var(--radius);
            padding: 28px 20px;
            text-align: center;
            cursor: pointer;
            transition: border-color .2s, background .2s;
            position: relative;
            background: #fafbfc;
        }
        .dropzone:hover, .dropzone.dragover {
            border-color: var(--accent);
            background: var(--accent-lt);
        }
        .dropzone input[type=file] {
            position: absolute; inset: 0;
            opacity: 0; cursor: pointer;
            width: 100%; height: 100%;
        }
        .dz-icon  { font-size: 36px; margin-bottom: 8px; }
        .dz-title { font-size: 15px; font-weight: 600; color: var(--text); }
        .dz-sub   { font-size: 12px; color: var(--muted); margin-top: 4px; }
        .dz-badge {
            display: inline-block;
            margin-top: 8px;
            background: var(--accent-lt);
            border: 1px solid #bfdbfe;
            color: var(--accent);
            font-size: 11px;
            padding: 2px 10px;
            border-radius: 20px;
            font-weight: 600;
            letter-spacing: .5px;
        }

        /* File indicator */
        .file-indicator {
            display: none; align-items: center; gap: 10px;
            background: var(--accent-lt);
            border: 1px solid #bfdbfe;
            border-radius: var(--radius-sm);
            padding: 9px 14px;
            margin-top: 10px;
            font-size: 12px;
        }
        .file-indicator.show { display: flex; }
        .file-name { flex: 1; color: var(--accent); font-weight: 500; }
        .file-size { color: var(--muted); }

        /* ── Buttons ── */
        .btn {
            display: inline-flex; align-items: center; gap: 6px;
            padding: 8px 18px; border-radius: var(--radius-sm);
            font-size: 13px; font-weight: 500; cursor: pointer;
            border: none; transition: all .15s; text-decoration: none;
        }
        .btn-primary  { background: var(--accent); color: #fff; box-shadow: 0 2px 8px rgba(37,99,235,.25); }
        .btn-primary:hover { background: #1d4ed8; box-shadow: 0 4px 12px rgba(37,99,235,.35); }
        .btn-success  { background: var(--accent2); color: #fff; box-shadow: 0 2px 8px rgba(5,150,105,.20); }
        .btn-success:hover { background: #047857; }
        .btn-outline  { background: #fff; color: var(--text-sub); border: 1px solid var(--border); }
        .btn-outline:hover { border-color: var(--accent); color: var(--accent); background: var(--accent-lt); }
        .btn-ghost    { background: transparent; color: var(--muted); border: 1px solid var(--border); }
        .btn-ghost:hover { color: var(--danger); border-color: var(--danger); background: var(--danger-lt); }
        .btn-full     { width: 100%; justify-content: center; }
        .btn-sm       { padding: 5px 12px; font-size: 12px; }

        /* ── Action Row ── */
        .action-row {
            display: flex;
            gap: 10px;
            margin-top: 12px;
            align-items: center;
        }
        .action-row .btn:first-child { flex: 1; justify-content: center; }

        /* ══════════════════════════════════════
           PREVIEW SECTION
        ══════════════════════════════════════ */
        .preview-section {
            background: var(--card);
            border: 1px solid var(--border);
            border-radius: var(--radius);
            box-shadow: var(--shadow-sm);
            overflow: hidden;
            display: flex;
            flex-direction: column;
        }

        .preview-section-head {
            background: var(--card-head);
            border-bottom: 1px solid var(--border);
            padding: 10px 18px;
            display: flex; align-items: center;
            justify-content: space-between; gap: 10px;
            flex-shrink: 0;
        }
        .preview-section-title {
            font-size: 15px; font-weight: 600; color: var(--text);
            display: flex; align-items: center; gap: 8px;
        }

        .badge-count {
            background: var(--accent2-lt);
            border: 1px solid #a7f3d0;
            color: var(--accent2);
            font-size: 12px;
            padding: 2px 12px;
            border-radius: 20px;
            font-weight: 600;
        }

        /* ── GridView Wrapper — fills remaining viewport height ── */
        .preview-section-body {
            flex: 1;
            overflow: auto;
            padding: 0;
            /* คำนวณความสูงที่เหลือ: viewport - header - import card - action bar - margins */
            max-height: calc(100vh - 320px);
            min-height: 180px;
        }

        /* ── GridView ── */
        .preview-grid {
            width: 100%;
            border-collapse: collapse;
            font-size: 12.5px;
        }
        .preview-grid th {
            background: #f1f5f9;
            color: var(--text-sub);
            font-weight: 600;
            font-size: 11px;
            text-transform: uppercase;
            letter-spacing: .6px;
            padding: 9px 12px;
            text-align: left;
            white-space: nowrap;
            border-bottom: 2px solid var(--border);
            position: sticky;
            top: 0;
            z-index: 2;
        }
        .preview-grid td {
            padding: 7px 12px;
            border-bottom: 1px solid var(--border-light);
            color: var(--text);
            white-space: nowrap;
            vertical-align: middle;
        }
        .preview-grid tbody tr:hover td { background: #f8fafc; }
        .preview-grid tbody tr:nth-child(even) td { background: #fafbfc; }
        .preview-grid tbody tr:nth-child(even):hover td { background: #f1f5f9; }

        /* Row number */
        .preview-grid td:first-child,
        .preview-grid th:first-child {
            color: var(--muted); font-size: 11px;
            text-align: center; width: 36px;
            background: #f8fafc;
        }

        /* MATCH_STATUS column */
        .status-ok  {
            color: var(--accent2); font-weight: 600; font-size: 11px;
        }
        .status-err {
            color: var(--danger); font-weight: 600; font-size: 11px;
        }

        /* ── Empty State ── */
        .empty-state {
            text-align: center; padding: 50px 20px; color: var(--muted);
        }
        .empty-state-icon { font-size: 44px; margin-bottom: 10px; }
        .empty-state-text { font-size: 14px; }

        /* ══════════════════════════════════════
           ACTION BAR (sticky bottom)
        ══════════════════════════════════════ */
        .action-bar {
            position: fixed; bottom: 0; left: 0; right: 0;
            background: #ffffff;
            border-top: 1px solid var(--border);
            box-shadow: 0 -4px 20px rgba(0,0,0,.08);
            padding: 10px 24px;
            display: flex; align-items: center;
            justify-content: space-between; gap: 12px;
            z-index: 100;
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

        /* ══════════════════════════════════════
           TOAST
        ══════════════════════════════════════ */
        .toast-container {
            position: fixed; bottom: 68px; right: 20px;
            z-index: 9999; display: flex; flex-direction: column; gap: 8px;
        }
        .toast {
            background: #fff;
            border: 1px solid var(--border);
            border-radius: var(--radius-sm);
            padding: 11px 16px;
            font-size: 13px;
            display: flex; align-items: center; gap: 10px;
            box-shadow: var(--shadow-lg);
            animation: toastIn .3s ease;
            min-width: 220px;
            max-width: 340px;
            color: var(--text);
        }
        .toast.success { border-left: 3px solid var(--accent2); }
        .toast.error   { border-left: 3px solid var(--danger); }
        @keyframes toastIn {
            from { opacity:0; transform:translateX(16px); }
            to   { opacity:1; transform:translateX(0); }
        }

        /* ══════════════════════════════════════
           SCROLLBAR
        ══════════════════════════════════════ */
        ::-webkit-scrollbar { width: 10px; height: 10px; }
        ::-webkit-scrollbar-track { background: #e2e6ed; border-radius: 5px; }
        ::-webkit-scrollbar-thumb { background: #64748b; border-radius: 5px; border: 2px solid #e2e6ed;  }
        ::-webkit-scrollbar-thumb:hover { background: #334155; }
        ::-webkit-scrollbar-corner { background: #e2e6ed; } 
    </style>

    <style>
        /* ── ID Tags ── */
        .id-tag        { display:inline-block; padding:2px 8px; border-radius:20px; font-size:11px; font-weight:700; }
        .id-tag.jig    { background:#eff4ff; color:#2563eb; border:1px solid #bfdbfe; }
        .id-tag.eqp    { background:#ecfdf5; color:#059669; border:1px solid #a7f3d0; }
        .id-tag.new    { background:#f1f5f9; color:#94a3b8; border:1px solid #e2e8f0; }
        .id-tag.sub    { background:#faf5ff; color:#7c3aed; border:1px solid #ddd6fe; }

        /* ── Match / Dup Badges ── */
        .badge-new     { background:#f0fdf4; color:#15803d; border:1px solid #bbf7d0; padding:2px 8px; border-radius:20px; font-size:11px; font-weight:700; }
        .badge-jig     { background:#eff4ff; color:#2563eb; border:1px solid #bfdbfe; padding:2px 8px; border-radius:20px; font-size:11px; font-weight:700; }
        .badge-eqp     { background:#ecfdf5; color:#059669; border:1px solid #a7f3d0; padding:2px 8px; border-radius:20px; font-size:11px; font-weight:700; }
        .badge-same    { background:#fff7ed; color:#c2410c; border:1px solid #fed7aa; padding:2px 8px; border-radius:20px; font-size:11px; font-weight:700; }
        .badge-addtool { background:#eff4ff; color:#2563eb; border:1px solid #bfdbfe; padding:2px 8px; border-radius:20px; font-size:11px; font-weight:700; }
        .badge-addeqp  { background:#ecfdf5; color:#059669; border:1px solid #a7f3d0; padding:2px 8px; border-radius:20px; font-size:11px; font-weight:700; }

        /* ── Empty cell ── */
        .cell-empty    { color:#cbd5e1; font-size:12px; }

        /* ── Status Dropdown ── */
        .ddl-status {
            border: 1px solid var(--border);
            border-radius: var(--radius-sm);
            padding: 3px 6px;
            font-size: 12px;
            color: var(--text);
            background: #fff;
            min-width: 130px;
            cursor: pointer;
        }
        .ddl-status:focus {
            outline: none;
            border-color: var(--accent);
            box-shadow: 0 0 0 2px rgba(37,99,235,.15);
        }
        /* ══════════════════════════════════════
           CONFIRM DIALOG
        ══════════════════════════════════════ */
        .confirm-overlay {
            position: fixed; inset: 0;
            background: rgba(15,23,42,.45);
            backdrop-filter: blur(3px);
            z-index: 9000;
            display: flex; align-items: center; justify-content: center;
            animation: fadeIn .2s ease;
        }
        .confirm-box {
            background: #fff;
            border-radius: 14px;
            box-shadow: 0 20px 60px rgba(0,0,0,.20);
            padding: 28px 28px 22px;
            width: 360px; max-width: 90vw;
            animation: slideUp .25s ease;
        }
        .confirm-icon  { font-size: 40px; text-align: center; margin-bottom: 10px; }
        .confirm-title {
            font-size: 17px; font-weight: 700;
            color: var(--text); text-align: center; margin-bottom: 6px;
        }
        .confirm-msg {
            font-size: 13px; color: var(--text-sub);
            text-align: center; line-height: 1.6; margin-bottom: 20px;
        }
        .confirm-stats {
            background: var(--accent-lt);
            border: 1px solid #bfdbfe;
            border-radius: var(--radius-sm);
            padding: 10px 14px;
            margin-bottom: 18px;
            font-size: 12px; color: var(--accent);
            display: flex; gap: 18px; justify-content: center;
        }
        .confirm-stat-item { text-align: center; }
        .confirm-stat-num  { font-size: 20px; font-weight: 700; display: block; }
        .confirm-stat-lbl  { font-size: 11px; opacity: .75; }
        .confirm-actions   { display: flex; gap: 10px; }
        .confirm-actions .btn { flex: 1; justify-content: center; font-size: 13px; }

        @keyframes fadeIn  { from{opacity:0} to{opacity:1} }
        @keyframes slideUp { from{opacity:0;transform:translateY(16px)} to{opacity:1;transform:translateY(0)} }

    </style>

    <asp:ScriptManager runat="server" ID="scriptManager" EnablePageMethods="true"></asp:ScriptManager>

    <asp:UpdatePanel runat="server" ID="UP1">
        <ContentTemplate>

            <div id="form1" runat="server">
                <div class="page-wrapper">

                    <!-- ── HEADER ── -->
                    <div class="page-header">
                        <img src="images/add-product.png" alt="icon" />
                        <div>
                            <h3>Equipment Registration</h3>
                            <p>นำเข้าข้อมูล Equipment ผ่านไฟล์ Excel</p>
                        </div>
                    </div>

                    <!-- ── IMPORT CARD ── -->
                    <div class="import-card">
                        <div class="import-card-head">
                            <div class="import-card-head-left">
                                <div class="card-icon">📤</div>
                                <div>
                                    <div class="card-title">Import Excel</div>
                                    <div class="card-sub">ลาก &amp; วาง หรือกดเลือกไฟล์ .xlsx / .xls</div>
                                </div>
                            </div>
                            <asp:Button ID="btnLoadTemplate" runat="server"
                                Text="📋 Load Template"
                                CssClass="btn btn-outline btn-sm"
                                OnClick="btnLoadTemplate_Click" />
                        </div>
                        <div class="import-card-body">

                            <!-- Drop Zone wrapper — ซ่อน/แสดงด้วย JS -->
                            <div id="dzWrapper">
                                <div class="dropzone" id="dropzone"
                                     ondragover="handleDragOver(event)"
                                     ondragleave="handleDragLeave(event)"
                                     ondrop="handleDrop(event)">
                                    <asp:FileUpload ID="fileUpload" runat="server"
                                        Accept=".xlsx,.xls"
                                        onchange="handleFileSelect(event)" />
                                    <div class="dz-icon">📂</div>
                                    <div class="dz-title">ลากไฟล์ Excel มาวางที่นี่</div>
                                    <div class="dz-sub">หรือ คลิกเพื่อเลือกไฟล์จากเครื่อง</div>
                                    <span class="dz-badge">.xlsx &nbsp;/&nbsp; .xls</span>
                                </div>

                                <div class="file-indicator" id="fileIndicator">
                                    <span>📄</span>
                                    <span class="file-name" id="fileName">—</span>
                                    <span class="file-size" id="fileSize">—</span>
                                </div>

                                <div class="action-row">
                                    <asp:Button ID="btnImportExcel" runat="server"
                                        Text="📤 Import & Preview"
                                        CssClass="btn btn-primary"
                                        OnClick="btnImportExcel_Click" />
                                    <button type="button" class="btn btn-ghost"
                                        onclick="clearFile()">✕ ล้างไฟล์</button>
                                </div>
                            </div><!-- /dzWrapper -->

                        </div>
                    </div><!-- /import-card -->

                    <!-- ── PREVIEW SECTION ── -->
                    <div class="preview-section" id="previewSection">
                        <div class="preview-section-head">
                            <div class="preview-section-title">
                                <span>📊</span> ข้อมูลที่ Import
                            </div>
                            <div style="display:flex; align-items:center; gap:10px;">
                                <asp:Label ID="lblRowCount" runat="server"
                                    CssClass="badge-count"
                                    Text="0 รายการ"
                                    Visible="false" />
                                <asp:Button ID="btnClearPreview" runat="server"
                                    Text="🗑 ล้าง"
                                    CssClass="btn btn-outline btn-sm"
                                    OnClick="btnClearPreview_Click"
                                    Visible="false" />
                            </div>
                        </div>

                        <div class="preview-section-body" id="previewBody">
                            <!-- Empty state -->
                            <asp:Panel ID="pnlEmpty" runat="server" Visible="true">
                                <div class="empty-state">
                                    <div class="empty-state-icon">📭</div>
                                    <div class="empty-state-text">ยังไม่มีข้อมูล — กรุณา Import Excel ก่อน</div>
                                </div>
                            </asp:Panel>

                            <!-- Preview GridView -->
                            <asp:Panel ID="pnlPreview" runat="server" Visible="false">
                                <asp:GridView ID="gvPreview" runat="server"
                                    AutoGenerateColumns="true"
                                    CssClass="preview-grid"
                                    GridLines="None"
                                    OnRowDataBound="gvPreview_RowDataBound">
                                    <EmptyDataTemplate>
                                        <div class="empty-state">
                                            <div class="empty-state-icon">📭</div>
                                            <div class="empty-state-text">ไม่พบข้อมูลในไฟล์</div>
                                        </div>
                                    </EmptyDataTemplate>
                                </asp:GridView>
                            </asp:Panel>
                        </div>
                    </div><!-- /preview-section -->

                </div><!-- /page-wrapper -->

                <!-- ── ACTION BAR ── -->
                <div class="action-bar">
                    <div class="action-bar-left">
                        <div class="status-pill">
                            <div class="status-dot"></div>
                            <asp:Label ID="lblActionStatus" runat="server" Text="รอ Import ข้อมูล" />
                        </div>
                    </div>
                    <div class="action-bar-right">
                        <button type="button" class="btn btn-outline"
                            onclick="history.back()">ยกเลิก</button>
                        <asp:Button ID="btnSave" runat="server"
                            Text="💾 บันทึกข้อมูล"
                            CssClass="btn btn-success"
                            Style="padding:8px 28px;"
                            OnClick="btnSave_Click"
                            OnClientClick="return confirmSave();"
                            Enabled="false" />
                    </div>
                </div>

                <!-- Toast Container -->
                <div class="toast-container" id="toastContainer"></div>

            </div><!-- /form1 -->

        </ContentTemplate>

        <Triggers>
            <asp:PostBackTrigger ControlID="btnImportExcel" />
            <asp:PostBackTrigger ControlID="btnLoadTemplate" />
            <asp:PostBackTrigger ControlID="btnClearPreview" />
            <asp:PostBackTrigger ControlID="btnSave" />
        </Triggers>
    </asp:UpdatePanel>

    <!-- ── JAVASCRIPT ── -->
    <script>
        /* ════════════════════════════════
           ตัวแปร global
        ════════════════════════════════ */
        var dz = null;

        window.addEventListener('load', function () {
            dz = document.getElementById('dropzone');
            adjustPreviewHeight();
            // ตรวจสอบ server-side render: ถ้ามี preview อยู่แล้ว ให้ซ่อน dropzone
            var pnlPreview = document.getElementById('<%= pnlPreview.ClientID %>');
            if (pnlPreview && pnlPreview.style.display !== 'none') {
                hideDzWrapper();
            }
        });

        window.addEventListener('resize', adjustPreviewHeight);

        /* ════════════════════════════════
           ปรับความสูง preview ตามหน้าจอ
        ════════════════════════════════ */
        function adjustPreviewHeight() {
            var body = document.getElementById('previewBody');
            if (!body) return;

            var vh = window.innerHeight;
            var actionBarH = 64;   /* ✅ เพิ่มเป็น 64 เผื่อ border */
            var pageHeaderH = document.querySelector('.page-header')
                ? document.querySelector('.page-header').offsetHeight + 16 : 80;
            var importCardH = document.querySelector('#dzWrapper.dz-hidden')
                ? (document.querySelector('.import-card')
                    ? document.querySelector('.import-card-head').offsetHeight + 33 : 60)
                : (document.querySelector('.import-card')
                    ? document.querySelector('.import-card').offsetHeight + 16 : 140);
            var previewHeadH = document.querySelector('.preview-section-head')
                ? document.querySelector('.preview-section-head').offsetHeight : 46;
            var margin = 46;

            var maxH = vh - actionBarH - pageHeaderH - importCardH - previewHeadH - margin - 50;
            if (maxH < 160) maxH = 160;
            body.style.maxHeight = maxH + 'px';
        }

        /* ════════════════════════════════
           ซ่อน / แสดง Drop Zone
        ════════════════════════════════ */
        function hideDzWrapper() {
            var w = document.getElementById('dzWrapper');
            if (w) w.classList.add('dz-hidden');
            setTimeout(adjustPreviewHeight, 50);
        }
        function showDzWrapper() {
            var w = document.getElementById('dzWrapper');
            if (w) w.classList.remove('dz-hidden');
            setTimeout(adjustPreviewHeight, 50);
        }

        /* ════════════════════════════════
           Drag & Drop handlers
        ════════════════════════════════ */
        function handleDragOver(e) {
            e.preventDefault(); e.stopPropagation();
            if (dz) dz.classList.add('dragover');
        }
        function handleDragLeave(e) {
            e.preventDefault();
            if (dz) dz.classList.remove('dragover');
        }
        function handleDrop(e) {
            e.preventDefault(); e.stopPropagation();
            if (dz) dz.classList.remove('dragover');
            if (e.dataTransfer.files.length > 0)
                showFileInfo(e.dataTransfer.files[0]);
        }
        function handleFileSelect(e) {
            if (e.target.files.length > 0)
                showFileInfo(e.target.files[0]);
        }
        function showFileInfo(file) {
            document.getElementById('fileName').textContent = file.name;
            document.getElementById('fileSize').textContent = (file.size / 1024).toFixed(1) + ' KB';
            document.getElementById('fileIndicator').classList.add('show');
        }
        function clearFile() {
            document.getElementById('fileIndicator').classList.remove('show');
            document.getElementById('fileName').textContent = '—';
            document.getElementById('fileSize').textContent = '—';
        }

        /* ════════════════════════════════
           หลัง PostBack — ซ่อน DZ ถ้ามี data
        ════════════════════════════════ */
        function pageLoad() {
            // ใช้ pageLoad (ASP.NET ScriptManager callback) แทน load
            // เพื่อ handle UpdatePanel re-render
            adjustPreviewHeight();
            var pnlPreview = document.getElementById('<%= pnlPreview.ClientID %>');
            if (pnlPreview) {
                // Panel visible = element มีอยู่ใน DOM
                hideDzWrapper();
            } else {
                showDzWrapper();
            }
        }

        /* ════════════════════════════════
           Toast
        ════════════════════════════════ */
        function showToast(type, msg) {
            var c = document.getElementById('toastContainer');
            var t = document.createElement('div');
            t.className = 'toast ' + type;
            t.innerHTML = (type === 'success' ? '✅' : '❌') + ' <span>' + msg + '</span>';
            c.appendChild(t);
            setTimeout(function () {
                t.style.opacity = '0';
                t.style.transition = 'opacity .3s';
                setTimeout(function () { t.remove(); }, 300);
            }, 3500);
        }

        /* ════════════════════════════════
           Confirm Save Dialog
        ════════════════════════════════ */
        var _saveConfirmed = false;

        function confirmSave() {
            if (_saveConfirmed) {
                _saveConfirmed = false;
                return true; // ผ่านไป postback จริง
            }

            // สร้าง dialog
            var badge = document.querySelector('.badge-count');
            var total = badge ? badge.textContent.replace(/[^0-9]/g, '') : '?';

            var cells = document.querySelectorAll('.preview-grid td');
            var cntNew = 0, cntSame = 0, cntAdd = 0;
            cells.forEach(function (c) {
                var t = c.textContent.trim();
                if (t.indexOf('New') >= 0) cntNew++;
                if (t.indexOf('Same') >= 0) cntSame++;
                if (t.indexOf('เพิ่ม') >= 0) cntAdd++;
            });

            var overlay = document.createElement('div');
            overlay.className = 'confirm-overlay';
            overlay.id = 'confirmOverlay';
            overlay.innerHTML =
                '<div class="confirm-box">' +
                '<div class="confirm-icon">💾</div>' +
                '<div class="confirm-title">ยืนยันการบันทึกข้อมูล</div>' +
                '<div class="confirm-msg">ข้อมูลจะถูกบันทึกเข้าสู่ระบบ<br>และรอการ Confirm จากผู้มีอำนาจ</div>' +
                '<div class="confirm-stats">' +
                '<div class="confirm-stat-item">' +
                '<span class="confirm-stat-num">' + total + '</span>' +
                '<span class="confirm-stat-lbl">รายการทั้งหมด</span>' +
                '</div>' +
                '<div class="confirm-stat-item">' +
                '<span class="confirm-stat-num" style="color:#15803d">' + cntNew + '</span>' +
                '<span class="confirm-stat-lbl">New</span>' +
                '</div>' +
                '<div class="confirm-stat-item">' +
                '<span class="confirm-stat-num" style="color:#c2410c">' + cntSame + '</span>' +
                '<span class="confirm-stat-lbl">Same</span>' +
                '</div>' +
                '<div class="confirm-stat-item">' +
                '<span class="confirm-stat-num" style="color:#2563eb">' + cntAdd + '</span>' +
                '<span class="confirm-stat-lbl">เพิ่มใหม่</span>' +
                '</div>' +
                '</div>' +
                '<div class="confirm-actions">' +
                '<button type="button" class="btn btn-outline" onclick="closeConfirm()">✕ ยกเลิก</button>' +
                '<button type="button" class="btn btn-success" onclick="doSave()">💾 ยืนยันบันทึก</button>' +
                '</div>' +
                '</div>';

            document.body.appendChild(overlay);
            overlay.addEventListener('click', function (ev) {
                if (ev.target === overlay) closeConfirm();
            });

            return false; // หยุด postback
        }

        function closeConfirm() {
            var o = document.getElementById('confirmOverlay');
            if (o) o.remove();
        }

        function doSave() {
            closeConfirm();
            // ✅ ใช้ __doPostBack โดยตรง — ไม่ผ่าน OnClientClick อีกรอบ
            __doPostBack('<%= btnSave.UniqueID %>', '');
        }
    </script>

</asp:Content>