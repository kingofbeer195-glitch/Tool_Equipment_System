<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" 
    CodeFile="MenuList2.aspx.cs"
    Inherits="Tool_Equipment_Control_System.MenuList" %>
<%--CodeFile="MenuList.aspx.cs"--%>
<asp:Content ID="Manu_List" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Tool &amp; Equipment Control System</title>


    <link href="CSS/w3v4.10.css" rel="stylesheet" />
    <link href="CSS/fontsgoogleapis.css" rel="stylesheet" />
    <link href="DataTables-1.10.18/css/dataTables.css" rel="stylesheet">
    <link href="Content/chosen.css" rel="stylesheet">

    <script src="Scripts/chosen.jquery.js"></script>
    <script src="Scripts/jquery-1.9.1.js"></script>
    <script src="Scripts/script.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>
    <script src="DataTables-1.10.18/js/dataTables.js"></script>
    <script src="DataTables-1.10.18/FixedColumns-3.2.5/js/fixedColumns.dataTables.js"></script>

    <style>                    
        *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

        :root {
            --bg:       #0f1117;
            --surface:  #181c27;
            --card:     #1e2332;
            --border:   #2a3045;
            --accent:   #f5a623;
            --accent2:  #3b82f6;
            --danger:   #ef4444;
            --success:  #22c55e;
            --text:     #e8ecf4;
            --muted:    #8b95b0;
            --font:     'IBM Plex Sans Thai', sans-serif;
            --mono:     'IBM Plex Mono', monospace;
        }

        body {
            background: var(--bg);
            color: var(--text);
            font-family: var(--font);
            min-height: 100vh;
            overflow-x: hidden;
        }

        /* GRID BACKGROUND  */
        body::before {
            content: '';
            position: fixed; inset: 0; z-index: 0;
            background-image:
                linear-gradient(rgba(59,130,246,0.04) 1px, transparent 1px),
                linear-gradient(90deg, rgba(59,130,246,0.04) 1px, transparent 1px);
            background-size: 40px 40px;
            pointer-events: none;
        }

        /* TOPBAR  */
        .topbar {
            position: sticky; top: 0; z-index: 100;
            display: flex; align-items: center; justify-content: space-between;
            padding: 0 32px;
            height: 60px;
            background: rgba(15,17,23,0.92);
            backdrop-filter: blur(12px);
            border-bottom: 1px solid var(--border);
        }
        .topbar-brand {
            display: flex; align-items: center; gap: 12px;
        }
        .brand-icon {
            width: 32px; height: 32px;
            background: var(--accent);
            border-radius: 6px;
            display: flex; align-items: center; justify-content: center;
            font-size: 16px;
        }
        .brand-name {
            font-size: 15px; font-weight: 700;
            letter-spacing: 0.04em;
            color: var(--text);
        }
        .brand-sub {
            font-size: 11px; color: var(--muted);
            font-family: var(--mono);
            letter-spacing: 0.08em;
            text-transform: uppercase;
        }
        .topbar-user {
            display: flex; align-items: center; gap: 10px;
            padding: 6px 14px;
            background: var(--card);
            border: 1px solid var(--border);
            border-radius: 8px;
            cursor: pointer;
            transition: border-color .2s;
        }
        .topbar-user:hover { border-color: var(--accent); }
        .user-avatar {
            width: 30px; height: 30px;
            background: linear-gradient(135deg, var(--accent2), var(--accent));
            border-radius: 50%;
            display: flex; align-items: center; justify-content: center;
            font-size: 12px; font-weight: 700; color: #fff;
        }
        .user-info { line-height: 1.3; }
        .user-name  { font-size: 13px; font-weight: 600; }
        .user-id    { font-size: 11px; color: var(--muted); font-family: var(--mono); }

        /* MAIN  */
        .main {
            position: relative; z-index: 1;
            max-width: 100%;
            margin: 30px 10px 0px 10px;
            padding: 0px 15px 15px;
            background-color : white;
            border-radius:5px;
        }

        /* PAGE HEADER  */
        .page-header {
            display: flex; align-items: center; gap: 16px;
            margin-bottom: 20px;
            padding-bottom: 0px;
            border-bottom: 1px solid var(--border);
        }
        .page-badge {
            font-family: var(--mono);
            font-size: 12px; font-weight: 600;
            color: var(--accent);
            background: rgba(245,166,35,0.12);
            border: 1px solid rgba(245,166,35,0.3);
            padding: 4px 10px;
            border-radius: 4px;
            letter-spacing: 0.1em;
            text-transform: uppercase;
        }
        .page-title {
            font-size: 24px; font-weight: 700;
            color: var(--text);
        }
        .page-title span { color: var(--accent); }

        /*  SECTION  */
        .section { margin-bottom: 28px; }
        .section-label {
            display: flex; align-items: center; 
            /*gap: 10px;*/
            margin-bottom: 16px;
            /*justify-content: center;*/
        }
        .section-label-line {
            width: 5px; height: 30px;            
            border-radius: 3px 0px 0px 3px;
        }
        .section-label-line.orange { background: var(--accent); }
        .section-label-line.blue   { background: var(--accent2); }
        .section-label-line.red    { background: var(--danger); }
        .section-title {
            font-size: 20px; font-weight: 600;
            letter-spacing: 0.12em;
            text-transform: uppercase;
            color: #595d66;
            height: 30px;
            border-radius: 0px 3px 3px 0px;
            padding-right: 5px;
        }

        /*  MENU GRID  */
        .menu-grid {
            display: grid;
            /*grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));*/
            grid-template-columns: repeat(auto-fit, minmax(200px, 200px));
            /*justify-content: center;*/ /* จัดให้อยู่กลาง */
            margin-left:20px;
            gap: 16px;
        }
        .menu-grid.col-5 { grid-template-columns: repeat(5, 1fr); }
        .menu-grid.col-3 { grid-template-columns: repeat(3, 1fr); }
        .menu-grid.col-5-admin { grid-template-columns: repeat(5, 1fr); }

        /* MENU CARD */
        .menu-card {
            position: relative;
            display: flex; flex-direction: column;
            align-items: center; justify-content: center;
            gap: 8px;
            padding: 20px 10px 14px;
            background: var(--card);
            border: 1px solid var(--border);
            border-radius: 12px;
            cursor: pointer;
            text-decoration: none;
            color: var(--text);
            overflow: hidden;
            transition: transform 1s ease, border-color 1s, box-shadow 1s;
            animation: fadeUp .4s ease both;
        }
        .menu-card::before {
            content: '';
            position: absolute; top: 0; left: 0; right: 0;
            height: 5px;
            opacity: 0;
            transition: opacity .2s;
        }
        .menu-card:hover {
            transform: translateY(-4px);
            box-shadow: 0 16px 40px rgba(0,0,0,0.4);
            color:#abc7ff;
            text-decoration: none;
        }
        .menu-card:hover::before { opacity: 1; }
        
        .menu-card.disabled {
            pointer-events: none;   /* ห้ามคลิก */
            opacity: 0.5;           /* ทำให้จาง */
            cursor: not-allowed;    /* เปลี่ยน cursor */
            /*filter: grayscale(100%);*/
            filter: opacity(0.5);
        }

        /* color variants */
        .card-orange { --clr: var(--accent); }
        .card-blue   { --clr: var(--accent2); }
        .card-red    { --clr: var(--danger); }
        .card-green  { --clr: var(--success); }

        .menu-card.card-orange:hover { border-color: var(--accent); box-shadow: 0 16px 40px rgba(245,166,35,0.15); }
        .menu-card.card-blue:hover   { border-color: var(--accent2); box-shadow: 0 16px 40px rgba(59,130,246,0.15); }
        .menu-card.card-red:hover    { border-color: var(--danger); box-shadow: 0 16px 40px rgba(239,68,68,0.12); }
        .menu-card.card-green:hover  { border-color: var(--success); box-shadow: 0 16px 40px rgba(34,197,94,0.12); }

        .menu-card::before { background: var(--clr, var(--accent)); }

        /* icon wrap */
        .card-icon {
            width: 65px; height: 45px;
            border-radius: 14px;
            display: flex; align-items: center; justify-content: center;
            font-size: 26px;
            background: rgba(255,255,255,0.05);
            border: 1px solid var(--border);
            transition: background .2s, transform .2s;
        }
        .menu-card:hover .card-icon {
            background: rgba(255,255,255,0.09);
            transform: scale(1.08);
            /*text-decoration: none;*/
        }
        .card-orange .card-icon { background: rgba(245,166,35,0.12); border-color: rgba(245,166,35,0.3); }
        .card-blue   .card-icon { background: rgba(59,130,246,0.12); border-color: rgba(59,130,246,0.3); }
        .card-red    .card-icon { background: rgba(239,68,68,0.12);  border-color: rgba(239,68,68,0.3); }
        .card-green  .card-icon { background: rgba(34,197,94,0.12);  border-color: rgba(34,197,94,0.3); }

        .card-text { text-align: center; }
        .card-name {
            font-size: 20px; font-weight: 600;
            line-height: 1.2;
            margin-bottom : 5px;
            color:#ffffff;
        }
        .card-desc {
            font-size: 14px; color: #ffffff;
            margin-top: 4px; line-height: 1.4;
            /*background: #ffffff;*/
            /*padding: 4px 15px;*/
            /*border-radius: 5px;*/
            border-top: 1px solid;
            padding-top: 5px;
        }

        /* animation stagger */
        .menu-card:nth-child(1) { animation-delay: .05s; }
        .menu-card:nth-child(2) { animation-delay: .10s; }
        .menu-card:nth-child(3) { animation-delay: .15s; }
        .menu-card:nth-child(4) { animation-delay: .20s; }
        .menu-card:nth-child(5) { animation-delay: .25s; }

        @keyframes fadeUp {
            from { opacity: 0; transform: translateY(16px); }
            to   { opacity: 1; transform: translateY(0); }
        }

        /* RESPONSIVE  */
        @media (max-width: 900px) {
            .menu-grid.col-5,
            .menu-grid.col-5-admin { grid-template-columns: repeat(3, 1fr); }
        }
        @media (max-width: 600px) {
            .menu-grid.col-5,
            .menu-grid.col-3,
            .menu-grid.col-5-admin { grid-template-columns: repeat(2, 1fr); }
            .topbar { padding: 0 16px; }
            .main   { padding: 24px 16px 48px; }
        }
    </style>

     <%--Dark mode Active--%>
    <style>
       
        body.dark .page-title,
        body.dark .section-title{
            color:#ffffff !important;
        }
        body.dark .card-name,
        body.dark .card-desc{
            color:var(--card);
        }
        body.dark .page-header{
            border-color: #ffffff
            /*border-bottom: 1px solid #ffffff*/
        }
        body.dark .menu-card{
            background:#ffffff;
        }

    </style>

    <%--Menu : กำลังพัฒนา (Under Development)--%>
    <style>
        .menu-card.is-dev {
            background: repeating-linear-gradient(135deg, #262b3b 0 10px, #1e2332 10px 20px);
            border: 1px dashed var(--accent);
        }
        .menu-card.is-dev .card-icon {
            filter: grayscale(55%);
            opacity: .75;
        }
        .menu-card.is-dev:hover {
            border-color: var(--accent);
            box-shadow: 0 16px 40px rgba(245,166,35,0.18);
        }
        .menu-card.is-dev::before { background: var(--accent); }

        /* ป้าย "กำลังพัฒนา" มุมซ้ายบน (มุมขวาบนใช้กับ badge แจ้งเตือน) */
        .dev-tag {
            position: absolute;
            top: 8px; left: 8px;
            z-index: 4;
            display: inline-flex; align-items: center; gap: 4px;
            padding: 2px 8px;
            font-size: 11px; font-weight: 700; line-height: 1.5;
            letter-spacing: .03em;
            color: #1e2332;
            background: var(--accent);
            border-radius: 999px;
            box-shadow: 0 2px 6px rgba(245,166,35,.35);
        }

        /* ยังไม่มีหน้าให้เข้า : คลิกไม่ได้ */
        .menu-card.is-dev.is-locked {
            cursor: not-allowed;
            pointer-events: none;
            opacity: .7;
        }

        /* Dark mode : การ์ดเป็นสีขาว */
        body.dark .menu-card.is-dev {
            background: repeating-linear-gradient(135deg, #ffffff 0 10px, #f4f5f7 10px 20px);
        }
    </style>

    <%--Notification Badge--%>
    <style>
        .menu-badge {
            position: absolute;
            top: 12px; right: 12px;
            min-width: 30px; height: 30px;
            padding: 0 9px;
            display: inline-flex; align-items: center; justify-content: center;
            font-family: var(--mono);
            font-size: 15px; font-weight: 700; line-height: 1;
            color: #fff;
            border-radius: 999px;               /* 1 หลัก = วงกลม, 2-3 หลัก = แคปซูล */
            background: linear-gradient(135deg, var(--bd1), var(--bd2));
            box-shadow: 0 0 0 3px var(--card),  /* ขอบตัดกับพื้นการ์ด */
                        0 4px 12px var(--bd-glow);
            z-index: 5;
            animation: badgePop .35s cubic-bezier(.34,1.56,.64,1) both,
                       badgeBounce 3.5s ease-in-out 1s infinite;
        }

        /* วงแหวนกระเพื่อม */
        .menu-badge::after {
            content: '';
            position: absolute; inset: 0;
            border-radius: inherit;
            border: 2px solid var(--bd1);
            animation: badgeRing 1.8s ease-out infinite;
            pointer-events: none;
        }

        /* สีตามประเภท */
        .badge-approve { --bd1: #ff4d4f; --bd2: #d4145a; --bd-glow: rgba(239,68,68,.55); }
        .badge-request { --bd1: #38bdf8; --bd2: #2563eb; --bd-glow: rgba(59,130,246,.55); }

        /* Dark mode : การ์ดเป็นสีขาว */
        body.dark .menu-badge {
            box-shadow: 0 0 0 3px #ffffff, 0 4px 12px var(--bd-glow);
        }

        @keyframes badgePop {
            from { transform: scale(0); }
            to   { transform: scale(1); }
        }
        @keyframes badgeRing {
            0%   { transform: scale(1);   opacity: .9; }
            100% { transform: scale(1.7); opacity: 0; }
        }
        @keyframes badgeBounce {
            0%, 88%, 100% { transform: translateY(0); }
            91% { transform: translateY(-5px); }
            94% { transform: translateY(0); }
            97% { transform: translateY(-2px); }
        }
    </style>

    <%--Status card : Notification Zone--%>
    <style>
        .notify-zone { margin: 0 0 26px 20px; }

        /* Header */
        .notify-head {
            display: flex; align-items: center; gap: 10px;
            margin-bottom: 12px;
        }
        .notify-head .nh-bell {
            width: 34px; height: 34px;
            display: flex; align-items: center; justify-content: center;
            font-size: 18px;
            background: #fff4e0;
            border: 1px solid #f5c26b;
            border-radius: 10px;
            animation: bellRing 3s ease-in-out infinite;
            transform-origin: 50% 10%;
        }
        .notify-head .nh-title {
            font-size: 17px; font-weight: 700;
            color: #3a3f4b; letter-spacing: .03em;
        }
        .notify-head .nh-sub { font-size: 12px; color: #8b95b0; }

        /* Grid */
        .status-row {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(230px, 270px));
            gap: 14px;
        }

        /* Card */
        a.status-card {
            position: relative;
            display: flex; align-items: center; gap: 14px;
            padding: 14px 18px 14px 20px;
            background: linear-gradient(135deg, var(--sc-soft) 0%, #ffffff 65%);
            border: 1px solid var(--sc-line);
            border-radius: 14px;
            box-shadow: 0 2px 8px rgba(20,24,40,.06);
            text-decoration: none;
            overflow: hidden;
            transition: transform .2s ease, box-shadow .2s ease, border-color .2s ease;
            animation: fadeUp .4s ease both;
        }
        a.status-card::before {                 /* แถบสีด้านซ้าย */
            content: '';
            position: absolute; left: 0; top: 0; bottom: 0;
            width: 5px;
            background: var(--sc);
        }
        a.status-card:hover {
            transform: translateY(-3px);
            border-color: var(--sc);
            box-shadow: 0 10px 24px var(--sc-glow);
            text-decoration: none;
        }

        /* Icon */
        .status-card .sc-icon {
            width: 46px; height: 46px; flex: 0 0 46px;
            border-radius: 12px;
            display: flex; align-items: center; justify-content: center;
            font-size: 22px;
            background: var(--sc);
            box-shadow: 0 4px 10px var(--sc-glow);
        }

        /* Text */
        .status-card .sc-body { display: flex; flex-direction: column; gap: 3px; }
        .status-card .sc-label {
            font-size: 14px; font-weight: 700; color: #2b2f3a;
            line-height: 1.25;
        }
        .status-card .sc-hint {
            font-size: 11px; font-weight: 600; color: var(--sc);
            opacity: .8;
            transition: letter-spacing .2s, opacity .2s;
        }
        a.status-card:hover .sc-hint { opacity: 1; letter-spacing: .04em; }

        /* Number */
        .status-card .sc-value {
            margin-left: auto;
            font-family: var(--mono);
            font-size: 34px; font-weight: 700; line-height: 1;
            color: var(--sc);
        }

        /* Pulse dot : มีงานรอ */
        .status-card .sc-pulse {
            position: absolute; top: 10px; right: 10px;
            width: 8px; height: 8px; border-radius: 50%;
            background: var(--sc);
        }
        .status-card .sc-pulse::after {
            content: '';
            position: absolute; inset: 0;
            border-radius: 50%;
            background: var(--sc);
            animation: ping 1.6s cubic-bezier(0,0,.2,1) infinite;
        }

        /* Color variants */
        .sc-30 { --sc: #2563eb; --sc-soft: rgba(37,99,235,.10);  --sc-line: rgba(37,99,235,.25);  --sc-glow: rgba(37,99,235,.28); }
        .sc-50 { --sc: #b42fad; --sc-soft: rgba(180,47,173,.10); --sc-line: rgba(180,47,173,.25); --sc-glow: rgba(180,47,173,.28); }
        .sc-90 { --sc: #5b4fc7; --sc-soft: rgba(91,79,199,.10);  --sc-line: rgba(91,79,199,.25);  --sc-glow: rgba(91,79,199,.28); }
        .sc-99 { --sc: #64748b; --sc-soft: rgba(100,116,139,.12); --sc-line: rgba(100,116,139,.28); --sc-glow: rgba(100,116,139,.25); }

        @keyframes ping {
            75%, 100% { transform: scale(2.6); opacity: 0; }
        }
        @keyframes bellRing {
            0%, 88%, 100% { transform: rotate(0); }
            90% { transform: rotate(14deg); }
            93% { transform: rotate(-12deg); }
            96% { transform: rotate(8deg); }
        }

        @media (max-width: 700px) {
            .status-row { grid-template-columns: 1fr; }
            .notify-zone { margin-left: 0; }
        }
    </style>

    <asp:ScriptManager runat="server" ID="scriptManager" EnablePageMethods="true"></asp:ScriptManager>

    <asp:UpdatePanel runat="server" ID="UP1">

        <ContentTemplate>

            <div id="form1" runat="server">

               <!-- ── Main ───────────────────────────────────────── -->
                <main class="main" style="min-height: calc(100vh - 80px);">

                    <div class="page-header">
                        <%--<span class="page-badge">MENU LIST</span>--%>
                        <img src="images/menu.png" alt="icon"  style="width:50px;"/>
                        <h1 class="page-title" style="color:#504d49db;">Jig Tool &amp; Equipment <span>: Menu List</span></h1>
                    </div>

                    <%-- Notification Zone (แสดงเฉพาะที่มีรายการ) --%>
                    <asp:Panel ID="pnlStatusCard" runat="server" Visible="false">
                        <div class="notify-zone">

                            <div class="notify-head">
                                <div class="nh-bell">🔔</div>
                                <div>
                                    <div class="nh-title">การแจ้งเตือน</div>
                                    <div class="nh-sub">รายการที่รอดำเนินการ — คลิกเพื่อดูรายละเอียด</div>
                                </div>
                            </div>

                            <div class="status-row">

                                <a href="ApproveStatus.aspx?doc=AAD2" class="status-card sc-30"
                                   id="cardMyApprove" runat="server" visible="false">
                                    <span class="sc-pulse"></span>
                                    <div class="sc-icon">📝</div>
                                    <div class="sc-body">
                                        <span class="sc-label">รอคุณอนุมัติ</span>
                                        <span class="sc-hint">ดูรายการ →</span>
                                    </div>
                                    <span class="sc-value" runat="server" id="valMyApprove">0</span>
                                </a>

                                <a href="ApproveStatus.aspx?status=50" class="status-card sc-50"
                                   id="card50" runat="server" visible="false">
                                    <span class="sc-pulse"></span>
                                    <div class="sc-icon">⏳</div>
                                    <div class="sc-body">
                                        <span class="sc-label">ร้องขอเบิกอุปกรณ์</span>
                                        <span class="sc-hint">AAD Confirm →</span>
                                    </div>
                                    <span class="sc-value" runat="server" id="valWaitApprove">0</span>
                                </a>

                                <a href="ApproveStatus.aspx?status=90" class="status-card sc-90"
                                   id="card90" runat="server" visible="false">
                                    <span class="sc-pulse"></span>
                                    <div class="sc-icon">📦</div>
                                    <div class="sc-body">
                                        <span class="sc-label">อยู่ระหว่างการยืม</span>
                                        <span class="sc-hint">During Borrow →</span>
                                    </div>
                                    <span class="sc-value" runat="server" id="valBorrowing">0</span>
                                </a>

                                <a href="ApproveStatus.aspx?status=99" class="status-card sc-99"
                                   id="card99" runat="server" visible="false">
                                    <span class="sc-pulse"></span>
                                    <div class="sc-icon">🔄</div>
                                    <div class="sc-body">
                                        <span class="sc-label">รออัพเดตข้อมูล</span>
                                        <span class="sc-hint">Wait Update →</span>
                                    </div>
                                    <span class="sc-value" runat="server" id="valWaitUpdate">0</span>
                                </a>

                            </div>
                        </div>
                    </asp:Panel>

                    <!--  SECTION 1: เบิกยืม-จัดเก็บ  -->
                    <section class="section">

                        <div class="section-label">
                            <div class="section-label-line orange"></div>
                            <span class="section-title" style="background: #f5a6233d;">🔧 เบิกยืม-จัดเก็บอุปกรณ์</span>
                        </div>
                        <div class="menu-grid">

                            <%--Menu : Status--%>
                            <a href="EquipmentStatus2.aspx" class="menu-card card-blue">
                                <span class="menu-badge badge-request" id="badgeRequest" runat="server" visible="false"
                                    title="คำขอของคุณที่อยู่ระหว่างดำเนินการ"></span>
                                <div class="card-icon">
                                    <img src="images/Toolstatus.png" alt="icon" style="max-width:95%;" />
                                </div>
                                <div class="card-text">
                                    <div class="card-name">Request Status</div>
                                    <div class="card-desc">ติดตามสถานะอุปกรณ์<br/>ติดตามสถานะการร้องขอ</div>
                                </div>
                            </a>                                                          

                            <%--Menu : Borrow ยืมอุปกรณ์--%>
                            <a href="borrow_tool2.aspx?issue_no=0" class="menu-card card-orange">
                                <div class="card-icon">
                                    <img src="images/return.png" alt="icon" />
                                </div>
                                <div class="card-text">
                                    <div class="card-name">ยืมอุปกรณ์</div>
                                    <div class="card-desc">ยื่นคำขอยืมอุปกรณ์<br/>(ยืมใช้ชั่วคราว)</div>
                                </div>
                            </a>                            

                            <%--Menu : Withdraw เบิกอุปกรณ์--%>
                            <a href="withdraw2.aspx?issue_no=0" class="menu-card card-orange">                                
                                <div class="card-icon">
                                    <img src="images/borrow1.png" alt="icon" style="max-width:95%;" />
                                </div>
                                <div class="card-text">
                                    <div class="card-name">เบิกอุปกรณ์</div>
                                    <div class="card-desc">ยื่นคำขอเบิกอุปกรณ์<br/>จาก Storage</div>
                                </div>
                            </a>

                            <%--Menu : Change Request แจ้งเปลี่ยนแปลง (กำลังพัฒนา : User + Admin)--%>
                            <div id="menuChangeRequest" runat="server" visible="false">
                                <a href="NotificationOfChange.aspx?issue_no=0" class="menu-card card-orange is-dev" title="เมนูนี้อยู่ระหว่างการพัฒนา">
                                    <span class="dev-tag">🚧 กำลังพัฒนา</span>
                                    <div class="card-icon">
                                        <img src="images/location.png" alt="icon" />
                                    </div>
                                    <div class="card-text">
                                        <div class="card-name">แจ้งเปลี่ยนแปลง</div>
                                        <div class="card-desc">ยื่นคำขอแจ้งเปลี่ยนแปลง<br/>สถานที่ใช้งานอุปกรณ์</div>
                                    </div>
                                </a>
                            </div>

                            <%--Menu : Storage จัดเก็บอุปกรณ์ (เปิดใช้งาน : เฉพาะ Admin)--%>
                            <div id="menuStorage" runat="server" visible="false">
                                <a href="storage.aspx?issue_no=0" class="menu-card card-orange">
                                    <div class="card-icon">
                                        <img src="images/storage.png" alt="icon" />
                                    </div>
                                    <div class="card-text">
                                        <div class="card-name">จัดเก็บอุปกรณ์</div>
                                        <div class="card-desc">ยื่นคำขอจัดเก็บอุปกรณ์<br/>ที่ Storage</div>
                                    </div>
                                </a>
                            </div>

                            <%--Menu : Tanaoroshi (กำลังพัฒนา : User + Admin)--%>
                            <div id="menuTanaoroshi" runat="server" visible="false">
                                <a href="tool_equipment_tanaoroshi.aspx" class="menu-card card-orange is-dev" title="เมนูนี้อยู่ระหว่างการพัฒนา">
                                    <span class="dev-tag">🚧 กำลังพัฒนา</span>
                                    <div class="card-icon" style="opacity:1;">
                                        <img src="images/tanaoroshi2.png" alt="icon" style="max-width:160%;" />
                                    </div>
                                    <div class="card-text">
                                        <div class="card-name">Tanaoroshi</div>
                                        <div class="card-desc">ตรวจนับอุปกรณ์คงคลัง<br/>และยืนยันจำนวนอุปกรณ์</div>
                                    </div>
                                </a>
                            </div>

                        </div>
                    </section>

                    <!--  SECTION 2: Tool & Equipment Control  -->
                    <section class="section">

                        <div class="section-label">
                            <div class="section-label-line blue"></div>
                            <span class="section-title" style="background:#3b82f630;">🏷️ Tool &amp; Equipment Control</span>
                        </div>

                        <div class="menu-grid">

                            <%--Menu : Approve--%>
                            <a href="ApproveStatus.aspx" class="menu-card card-blue" ID="approveMenu" runat="server">
                                <span class="menu-badge badge-approve" id="badgeApprove" runat="server" visible="false"
                                    title="รายการที่รอคุณอนุมัติ"></span>
                                <div class="card-icon">
                                    <img src="images/approved.png" alt="icon" style="max-width:95%;" />
                                </div>
                                <div class="card-text">
                                    <div class="card-name">Approve</div>
                                    <div class="card-desc">อนุมัติการร้องขอ<br/>จัดการอุปกรณ์</div>
                                </div>
                            </a>                            

                            <%--Menu : Master List--%>
                            <a href="Tool_equipment_MasterList.aspx" class="menu-card card-green">
                                <div class="card-icon">
                                    <img src="images/machine-data.png" alt="icon" style="max-width:95%;"/>
                                </div>
                                <div class="card-text">
                                    <div class="card-name">Master List</div>
                                    <div class="card-desc">ข้อมูลหลักอุปกรณ์<br/>รายการและรายละเอียด</div>
                                </div>
                            </a>                            

                            <%--Menu : History--%>
                            <a href="History_Status.aspx" class="menu-card card-green">
                                <div class="card-icon">
                                    <img src="images/Toolhistory.png" alt="icon" style="max-width:95%;" />
                                </div>
                                <div class="card-text">
                                    <div class="card-name">History</div>
                                    <div class="card-desc">ตรวจสอบประวัติ<br/>การควบคุมอุปกรณ์</div>
                                </div>
                            </a>

                            <%--Menu : Dashboard (กำลังพัฒนา : Admin / ยังไม่มีหน้า → คลิกไม่ได้)--%>
                            <div id="menuDashboard" runat="server" visible="false">
                                <a class="menu-card card-blue is-dev is-locked" aria-disabled="true" title="เมนูนี้อยู่ระหว่างการพัฒนา">
                                    <span class="dev-tag">🚧 กำลังพัฒนา</span>
                                    <div class="card-icon">
                                        <img src="images/Dashboard_Icon.png" alt="icon" style="max-width:95%;" />
                                    </div>
                                    <div class="card-text">
                                        <div class="card-name">Dashboard</div>
                                        <div class="card-desc">ภาพรวมสถิติ <br/>และการใช้งานอุปกรณ์</div>
                                    </div>
                                </a>
                            </div>

                            <%--Menu : Equipment (กำลังพัฒนา : Admin)--%>
                            <div id="menuEquipment" runat="server" visible="false">
                                <a href="CalibrationManagement.aspx" class="menu-card card-green is-dev" title="เมนูนี้อยู่ระหว่างการพัฒนา">
                                    <span class="dev-tag">🚧 กำลังพัฒนา</span>
                                    <div class="card-icon">
                                        <img src="images/electrical.png" alt="icon" style="max-width:120%;" />
                                    </div>
                                    <div class="card-text">
                                        <div class="card-name">Equipment</div>
                                        <div class="card-desc">เครื่องมือวัด<br/>ระบบควบคุมและแจ้งเตือน</div>
                                    </div>
                                </a>   
                            </div>

                        </div>

                    </section>

                    <!--  SECTION 3: Admin  -->
                    <asp:Panel ID="pnlMenu" runat="server" Visible="false">

                        <section class="section">

                            <div class="section-label">
                                <div class="section-label-line red"></div>
                                <span class="section-title" style="background:#ef444440;">🛡️ Admin &amp; Configuration</span>
                            </div>

                            <div class="menu-grid">

                                <a id="SettingTana" runat="server" href="setting_tanaoroshi.aspx" class="menu-card card-red">
                                    <div class="card-icon">
                                        <img src="images/inventory-management.png" alt="icon" style="max-width:100%;" />
                                    </div>
                                    <div class="card-text">
                                        <div class="card-name">Setting Tana</div>
                                        <div class="card-desc">กำหนดวันและรายการ<br/>ตรวจนับสินค้าคงคลัง</div>
                                    </div>
                                </a>

                                <a id="NewEquipment" runat="server" href="Equipment_Add_New.aspx"  class="menu-card card-red">
                                    <div class="card-icon">
                                        <img src="images/add-product.png" alt="icon" style="max-width:100%;" />
                                    </div>
                                    <div class="card-text">
                                        <div class="card-name">New Equipment</div>
                                        <div class="card-desc">ลงทะเบียน<br/>อุปกรณ์ใหม่</div>
                                    </div>
                                </a>

                                <a id="EditLocation" runat="server" href="EditLocation.aspx" class="menu-card card-red">
                                    <div class="card-icon">
                                        <img src="images/changeLocation.png" alt="icon" style="max-width:100%;" />
                                    </div>
                                    <div class="card-text">
                                        <div class="card-name">Edit Location</div>
                                        <div class="card-desc">เปลี่ยนแปลงสถานที่ใช้งาน<br/>ผู้รับผิดชอบ</div>
                                    </div>
                                </a>

                                <a id="TranferEquipment" runat="server" href="Transfer.aspx" class="menu-card card-red">
                                    <div class="card-icon">
                                        <img src="images/exchange.png" alt="icon" style="max-width:100%;" />
                                    </div>
                                    <div class="card-text">
                                        <div class="card-name">Transfer Soto</div>
                                        <div class="card-desc">โอนย้ายผู้รับผิดชอบ<br/>Soto Cell</div>
                                    </div>
                                </a>

                                <a id="ScrapEquipment" runat="server" href="Scrap.aspx" class="menu-card card-red">
                                    <div class="card-icon">
                                        <img src="images/waste.png" alt="icon" style="max-width:100%;" />
                                    </div>
                                    <div class="card-text">
                                        <div class="card-name">Scrap</div>
                                        <div class="card-desc">แจ้งรายการ<br/>กำจัดอุปกรณ์</div>
                                    </div>
                                </a>
                               
                                <a id="Authorization" runat="server" href="Authorize.aspx" class="menu-card card-red">
                                    <div class="card-icon">
                                        <img src="images/key-person.png" alt="icon" style="max-width:100%;" />
                                    </div>
                                    <div class="card-text">
                                        <div class="card-name">Authorize</div>
                                        <div class="card-desc">กำหนดสิทธิ์<br/>Admin & Approve</div>
                                    </div>
                                </a>

                                <%--Menu : System Setup (กำลังพัฒนา : Admin)--%>
                                <a id="Setting" runat="server" href="Equipment_Setting.aspx" class="menu-card card-red is-dev" title="เมนูนี้อยู่ระหว่างการพัฒนา">
                                    <span class="dev-tag">🚧 กำลังพัฒนา</span>
                                    <div class="card-icon">
                                        <img src="images/settings.png" alt="icon" style="max-width:95%;" />
                                    </div>
                                    <div class="card-text">
                                        <div class="card-name">System Setup</div>
                                        <div class="card-desc">กำหนดค่าและจัดการ<br/>ข้อมูลพื้นฐานของระบบ</div>
                                    </div>
                                </a>

                                <%--Menu : Auto Bot (กำลังพัฒนา : Admin)--%>
                                <a id="AutoBot" runat="server" href="AutoBot.aspx" class="menu-card card-red is-dev" title="เมนูนี้อยู่ระหว่างการพัฒนา">
                                    <span class="dev-tag">🚧 กำลังพัฒนา</span>
                                    <div class="card-icon">
                                        <img src="images/autobot.png" alt="icon" style="max-width:90%;" />
                                    </div>
                                    <div class="card-text">
                                        <div class="card-name">Auto Bot</div>
                                        <div class="card-desc">รายการอัพเดตข้อมูล<br/>Auto</div>
                                    </div>
                                </a>





                                <%--<a href="CIPConfirm.aspx" class="menu-card card-red">
                                    <div class="card-icon">✅</div>
                                    <div class="card-text">
                                        <div class="card-name">CIP Confirm</div>
                                        <div class="card-desc">ยืนยัน Continuous<br/>Improvement Process</div>
                                    </div>
                                </a>--%>

                            </div>
                        </section>

                    </asp:Panel>
                </main>
            </div>
        </ContentTemplate>

    </asp:UpdatePanel>
</asp:Content>
