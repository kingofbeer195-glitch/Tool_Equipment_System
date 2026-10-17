<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="Equipment_Setting.aspx.cs"
    Inherits="Tool_Equipment_Control_System.Equipment_Setting" %>

<asp:Content ID="Setting_Home" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <link href="CSS/select2.min.css" rel="stylesheet" />
    <link href="CSS/fontsgoogleapis.css" rel="stylesheet">

    <script src="Scripts/jquery-3.6.0.min.js"></script>
    <script src="Scripts/select2.min.js"></script>

    <%--Alert : toast (ชุดเดียวกับ History_Status)--%>
    <script>
        (function () {
            function ensureContainer() {
                var c = document.getElementById('toast-container');
                if (!c) { c = document.createElement('div'); c.id = 'toast-container'; document.body.appendChild(c); }
                return c;
            }
            window.showToast = function (message, options) {
                options = options || {};
                var title = options.title || '', type = options.type || 'info', duration = options.duration || 3000;
                var container = ensureContainer();
                var toast = document.createElement('div');
                toast.className = 'toast ' + type;
                toast.setAttribute('role', 'status');
                var content = document.createElement('div');
                content.style.display = 'flex'; content.style.flexDirection = 'column';
                if (title) { var t = document.createElement('div'); t.className = 'title'; t.textContent = title; content.appendChild(t); }
                var msg = document.createElement('div'); msg.className = 'message'; msg.textContent = message; content.appendChild(msg);
                var closeBtn = document.createElement('button'); closeBtn.className = 'close'; closeBtn.innerHTML = '&times;';
                closeBtn.addEventListener('click', function () { if (toast.parentNode) container.removeChild(toast); });
                toast.appendChild(content); toast.appendChild(closeBtn); container.appendChild(toast);
                if (duration > 0) setTimeout(function () { if (toast.parentNode) container.removeChild(toast); }, duration);
            };
        })();
    </script>

    <%--Select2 : ใช้ได้หลัง partial postback + dropdown อยู่ใน Modal--%>
    <script>
        function initStSelect2() {
            if (!window.jQuery || !jQuery.fn.select2) return;
            jQuery('.st-s2').each(function () {
                var $el = jQuery(this);
                if ($el.hasClass('select2-hidden-accessible')) $el.select2('destroy');
                var $box = $el.closest('.st-overlay .st-box');   // ใน Modal → dropdown ต้องอยู่ใน Modal (z-index)
                $el.select2({
                    width: '100%',
                    placeholder: $el.data('ph') || '',
                    allowClear: !$el.prop('multiple') && !!$el.data('ph'),
                    dropdownParent: $box.length ? $box : jQuery(document.body)
                });
            });
        }
        function closeEditor() {
            var m = document.getElementById('<%= pnlEditor.ClientID %>');
            if (m) m.style.display = 'none';
            return false;
        }
        function confirmDelete(name) {
            return confirm('⚠️ ลบกฎ "' + name + '" ?\n\nกฎจะถูกลบถาวร (ยังดูได้ในประวัติ)\nกด OK เพื่อลบ');
        }
        jQuery(function () {
            initStSelect2();
            if (window.Sys && Sys.WebForms) Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initStSelect2);
        });
    </script>

    <style>
        :root {
            --ink:#1c2b3a; --ink-2:#33475b; --muted:#66727f;
            --paper:#f3f5f8; --line:#dde3ea; --line-soft:#edf0f4;
            --navy:#1a3a5c; --accent:#f5a623; --danger:#c93b3b; --ok:#16884a;
        }
        body { background:var(--paper); font-family:'IBM Plex Sans Thai','Segoe UI',Tahoma,sans-serif; }

        .st-wrap { margin:10px; color:var(--ink); }
        .st-header {
            display:flex; align-items:center; gap:14px;
            background:linear-gradient(135deg,#1a3a5c 0%,#2980b9 100%); color:#fff;
            border-radius:10px; padding:12px 18px; margin-bottom:10px;
        }
        .st-header h1 { margin:0; font-size:22px; font-weight:700; }
        .st-header small { display:block; font-size:12.5px; opacity:.85; font-weight:400; }
        .st-header .ico { font-size:28px; }

        /* Tabs */
        .st-tabs { display:flex; gap:4px; }
        .st-tab { padding:8px 16px; border-radius:8px 8px 0 0; background:#c8d9e8; color:var(--navy); font-weight:600; font-size:14px; }
        .st-tab.active { background:#fff; }
        .st-tab.soon { background:#e7ebf0; color:#9aa5b1; }

        .st-card { background:#fff; border-radius:0 10px 10px 10px; box-shadow:0 1px 6px rgba(0,0,0,.08); padding:14px 16px; }
        .st-grid2 { display:grid; grid-template-columns:minmax(0,1fr) 360px; gap:14px; align-items:start; }
        @media (max-width:1100px) { .st-grid2 { grid-template-columns:1fr; } }

        .st-sec { font-size:15px; font-weight:700; color:var(--navy); margin:0 0 8px; display:flex; align-items:center; gap:8px; }
        .st-count { background:var(--navy); color:#fff; border-radius:999px; padding:1px 9px; font-size:12px; }

        /* คำอธิบายการ Match */
        .st-howto {
            background:#eef5ff; border:1px solid #bcd1f7; border-left:5px solid #1f6feb;
            border-radius:8px; padding:8px 12px; font-size:12.5px; color:var(--ink-2); margin-bottom:10px;
        }
        .st-howto b { color:var(--navy); }
        .st-howto ul { margin:4px 0 0; padding-left:18px; }

        /* Toolbar */
        .st-toolbar { display:flex; flex-wrap:wrap; align-items:center; gap:8px; margin-bottom:10px; }
        .st-input { height:34px; border:1.5px solid #c8d6e5; border-radius:6px; padding:3px 10px; font-size:13.5px; min-width:260px; }
        .st-btn {
            display:inline-flex; align-items:center; gap:6px; height:34px; padding:0 14px;
            border:1px solid transparent; border-radius:8px; font-size:13.5px; font-weight:700;
            cursor:pointer; text-decoration:none !important; white-space:nowrap;
        }
        .st-btn-primary { background:#1f6feb; color:#fff !important; }   .st-btn-primary:hover { background:#1858c4; }
        .st-btn-ok      { background:var(--ok); color:#fff !important; } .st-btn-ok:hover { background:#11703c; }
        .st-btn-light   { background:#fff; color:var(--ink-2) !important; border-color:#c9d2dc; } .st-btn-light:hover { background:var(--paper); }
        .st-btn-sm { height:28px; padding:0 10px; font-size:12px; }
        .st-btn-danger-lt { background:#fff; color:var(--danger) !important; border-color:#efb9b9; } .st-btn-danger-lt:hover { background:#fbeceb; }
        .st-chk { font-size:13px; color:var(--ink-2); }
        .st-chk input { margin-right:4px; }

        /* Rules table */
        .st-tbl-wrap { overflow:auto; max-height:calc(100vh - 330px); border:1px solid var(--line); border-radius:8px; }
        table.st-tbl { width:100%; border-collapse:collapse; font-size:13px; }
        table.st-tbl th { position:sticky; top:0; z-index:2; background:var(--navy); color:#fff; padding:8px 10px; text-align:left; white-space:nowrap; font-weight:600; }
        table.st-tbl td { padding:8px 10px; border-bottom:1px solid var(--line-soft); vertical-align:top; }
        table.st-tbl tr:hover td { background:#f7faff; }
        table.st-tbl tr.st-row-off td { background:#f6f7f9; color:#98a2ad; }
        table.st-tbl tr.st-row-off .st-chip { opacity:.55; }
        .st-name { font-weight:700; color:var(--ink); }
        .st-remark { font-size:12px; color:var(--muted); margin-top:2px; }
        .st-upd { font-size:11px; color:#98a2ad; margin-top:4px; }
        .st-id { font-size:11px; color:#98a2ad; }

        .st-k { font-size:11px; font-weight:700; color:var(--muted); margin-right:3px; text-transform:uppercase; }
        .st-chip { display:inline-block; margin:1px 3px 1px 0; padding:1px 8px; border-radius:999px; font-size:12px; font-weight:600; border:1px solid transparent; }
        .chip-dept { background:#e8f1fd; color:#1858c4; border-color:#bcd1f7; }
        .chip-cell { background:#e3f4e8; color:#16703d; border-color:#b6e0c4; }
        .chip-sta  { background:#f8eefb; color:#7e2178; border-color:#dfc0e6; }
        .st-op { display:inline-block; margin:0 4px; font-size:11px; font-weight:800; color:#c47f00; }
        .st-paren { white-space:normal; }
        .st-badge { display:inline-block; padding:1px 8px; border-radius:4px; background:#fff4e0; color:#a15c00; border:1px solid #f5c26b; font-size:11px; font-weight:700; white-space:nowrap; }
        .st-muted { color:#98a2ad; font-size:12px; }

        ol.st-resp { margin:0; padding-left:18px; }
        ol.st-resp li { font-size:12.5px; }
        ol.st-resp li.is-auto { font-weight:700; color:#16703d; }

        .st-toggle { display:inline-block; min-width:62px; text-align:center; padding:3px 8px; border-radius:999px; font-size:12px; font-weight:700;
                     background:#eef1f4; color:#66727f !important; border:1px solid #d9dde2; text-decoration:none !important; }
        .st-toggle.on { background:#e3f4e8; color:#16703d !important; border-color:#b6e0c4; }
        .st-actions { display:flex; gap:6px; white-space:nowrap; }

        /* Test box */
        .st-test { border:1px solid var(--line); border-radius:10px; padding:12px; background:#fbfcfe; }
        .st-test .st-field { margin-bottom:8px; }
        .st-test label, .st-field label { display:block; font-size:12px; font-weight:700; color:var(--muted); margin-bottom:3px; }
        .st-test-res { margin-top:10px; padding:10px 12px; border-radius:8px; font-size:13px; line-height:1.6; }
        .st-test-res.hit  { background:#e3f4e8; border:1px solid #b6e0c4; }
        .st-test-res.none { background:#fff7e0; border:1px solid #f1d98c; }
        .st-test-res.warn { background:#fbeceb; border:1px solid #efb9b9; color:#a82f2f; }

        /* History */
        .st-hist { margin-top:14px; }
        .st-hist table.st-tbl td { font-size:12px; }
        .st-act { display:inline-block; padding:1px 8px; border-radius:4px; font-size:11px; font-weight:700; background:#eef1f4; color:#4a5b6d; }

        /* ── Modal ── */
        .st-overlay { position:fixed; inset:0; z-index:10000; background:rgba(20,30,42,.45); align-items:center; justify-content:center; }
        .st-box { display:flex; flex-direction:column; width:880px; max-width:96vw; max-height:92vh; background:#fff; border-radius:12px; overflow:hidden; box-shadow:0 20px 50px rgba(20,30,42,.3); }
        .st-mhead { display:flex; align-items:center; justify-content:space-between; padding:14px 20px; border-bottom:1px solid var(--line-soft); font-size:16px; font-weight:700; }
        .st-x { width:32px; height:32px; border:none; background:none; font-size:24px; color:var(--muted); cursor:pointer; border-radius:6px; }
        .st-x:hover { background:var(--paper); }
        .st-mbody { padding:16px 20px; overflow-y:auto; }
        .st-mfoot { display:flex; justify-content:flex-end; gap:8px; padding:12px 20px; border-top:1px solid var(--line-soft); }

        .st-form { display:grid; grid-template-columns:1fr 1fr; gap:10px 16px; }
        .st-form .full { grid-column:1 / -1; }
        .st-field .st-input { width:100%; min-width:0; }
        .st-field select.st-input { height:34px; }
        .st-group { border:1px solid var(--line); border-radius:10px; padding:10px 12px; }
        .st-group-title { font-size:13px; font-weight:700; color:var(--navy); margin-bottom:8px; }
        .st-group-title small { font-weight:400; color:var(--muted); }
        .st-rbl label { font-size:13px; font-weight:600; margin:0 14px 0 4px; display:inline; color:var(--ink-2); }

        .st-resp-edit { list-style:none; margin:8px 0 0; padding:0; }
        .st-resp-edit li { display:flex; align-items:center; gap:8px; padding:6px 8px; border:1px solid var(--line); border-radius:6px; margin-bottom:4px; font-size:13px; }
        .st-resp-edit li.is-auto { border-color:#b6e0c4; background:#f1faf4; }
        .st-resp-edit .no { width:22px; height:22px; border-radius:50%; background:var(--navy); color:#fff; display:inline-flex; align-items:center; justify-content:center; font-size:11px; font-weight:700; }
        .st-resp-edit li.is-auto .no { background:var(--ok); }
        .st-resp-edit .nm { flex:1; }
        .st-resp-edit .auto { font-size:11px; color:#16703d; font-weight:700; }
        .st-icon-btn { display:inline-flex; align-items:center; justify-content:center; width:26px; height:26px; border-radius:6px; border:1px solid var(--line); background:#fff; color:var(--ink-2) !important; text-decoration:none !important; font-size:12px; }
        .st-icon-btn:hover { background:var(--paper); }
        .st-icon-btn.del { color:var(--danger) !important; }
        .st-err { background:#fbeceb; border:1px solid #efb9b9; border-left:5px solid var(--danger); color:#8f2b22; border-radius:8px; padding:8px 12px; font-size:13px; margin-bottom:10px; }

        .st-denied { margin:40px auto; max-width:520px; text-align:center; background:#fff; border-radius:12px; padding:30px; box-shadow:0 1px 6px rgba(0,0,0,.08); }

        /* Select2 ให้เข้าชุด */
        .st-box .select2-container .select2-selection--multiple { min-height:34px; border:1.5px solid #c8d6e5; border-radius:6px; }
        .st-box .select2-container .select2-selection--single { height:34px; border:1.5px solid #c8d6e5; border-radius:6px; }
        .st-box .select2-container .select2-selection--single .select2-selection__rendered { line-height:31px; }
        .st-box .select2-container .select2-selection--single .select2-selection__arrow { height:32px; }
        .select2-container--open .select2-dropdown { z-index:10050; }

        /* Toast */
        #toast-container { position:fixed; top:16px; right:16px; display:flex; flex-direction:column; gap:8px; z-index:10100; }
        .toast { min-width:260px; max-width:560px; padding:12px 14px; border-radius:8px; color:#fff; box-shadow:0 6px 18px rgba(0,0,0,.15); display:flex; align-items:start; gap:10px; font-size:15px; }
        .toast .title { font-weight:600; margin-bottom:4px; }
        .toast .close { margin-left:auto; background:transparent; border:none; color:inherit; font-size:18px; cursor:pointer; }
        .toast.success { background:#16a34a; } .toast.error { background:#dc2626; } .toast.warn { background:#d97706; } .toast.info { background:#2563eb; }
    </style>

    <asp:ScriptManager runat="server" ID="scriptManager1" EnablePageMethods="true"></asp:ScriptManager>

    <div id="toast-container" aria-live="polite" aria-atomic="true"></div>

    <%-- ไม่มีสิทธิ์ --%>
    <asp:Panel ID="pnlDenied" runat="server" Visible="false">
        <div class="st-denied">
            <div style="font-size:40px;">🔒</div>
            <h3 style="margin:8px 0;">ไม่มีสิทธิ์เข้าใช้งาน</h3>
            <p style="color:#66727f;">หน้า System Setup ใช้ได้เฉพาะ Admin Level 4</p>
            <a href="MenuList2.aspx" class="st-btn st-btn-light">← กลับหน้าเมนู</a>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlMain" runat="server">
    <asp:UpdatePanel runat="server" ID="UP1">
        <ContentTemplate>
            <div class="st-wrap">

                <div class="st-header">
                    <span class="ico">⚙️</span>
                    <h1>System Setup<small>จัดการเงื่อนไขต่าง ๆ ของระบบ Tool &amp; Equipment</small></h1>
                </div>

                <div class="st-tabs">
                    <span class="st-tab active">🤖 Auto Assign : Status Use / ผู้รับผิดชอบ</span>
                    <span class="st-tab soon" title="เตรียมไว้สำหรับการตั้งค่าอื่นในอนาคต">＋ การตั้งค่าอื่น</span>
                </div>

                <div class="st-card">
                    <div class="st-grid2">

                        <%-- ===== ซ้าย : รายการกฎ ===== --%>
                        <div>
                            <div class="st-howto">
                                <b>วิธีทำงาน</b> — ตอน AAD Confirm ระบบเลือก Status Use / ผู้รับผิดชอบ จากกฎที่ <b>ตรงและเจาะจงที่สุด</b>
                                <ul>
                                    <li>หลายค่าในช่องเดียวกัน = <b>หรือ</b> (Dept 5510 หรือ 5515) · Dept กับ Cell/Station = <b>และ</b></li>
                                    <li>ตรงหลายกฎ : <span class="st-badge">Dept + Location</span> &gt; <span class="st-badge">Location</span> &gt; <span class="st-badge">Dept</span> — เท่ากันใช้ Priority มากกว่า</li>
                                    <li>ไม่ตรงกฎใด → ใช้ค่าเดิม (ผู้รับผิดชอบจาก Responsible_Dept · Dept 5505 = Spare / อื่น ๆ = Use)</li>
                                </ul>
                            </div>

                            <div class="st-toolbar">
                                <asp:LinkButton ID="btnAddRule" runat="server" CssClass="st-btn st-btn-primary" OnClick="btnAddRule_Click">＋ เพิ่มกฎ</asp:LinkButton>
                                <asp:TextBox ID="txtFilter" runat="server" CssClass="st-input" placeholder="ค้นหา ชื่อกฎ / Dept / Cell / ผู้รับผิดชอบ" AutoComplete="off" />
                                <asp:LinkButton ID="btnFilter" runat="server" CssClass="st-btn st-btn-light" OnClick="btnFilter_Click">🔍 ค้นหา</asp:LinkButton>
                                <asp:CheckBox ID="chkShowInactive" runat="server" CssClass="st-chk" Text="แสดงกฎที่ปิดอยู่" Checked="true"
                                    AutoPostBack="true" OnCheckedChanged="chkShowInactive_CheckedChanged" />
                            </div>

                            <div class="st-sec">รายการกฎ <span class="st-count"><asp:Label ID="lblRuleCount" runat="server" Text="0" /></span></div>

                            <div class="st-tbl-wrap">
                                <asp:GridView ID="gvRules" runat="server" CssClass="st-tbl" GridLines="None" CellSpacing="0"
                                    AutoGenerateColumns="false" OnRowCommand="gvRules_RowCommand" OnRowDataBound="gvRules_RowDataBound"
                                    EmptyDataText="<div style='padding:24px;text-align:center;color:#98a2ad;'>ยังไม่มีกฎ — กด ＋ เพิ่มกฎ</div>">
                                    <Columns>
                                        <asp:TemplateField HeaderText="สถานะ" ItemStyle-Width="80px">
                                            <ItemTemplate>
                                                <asp:LinkButton runat="server" CommandName="ToggleRule" CommandArgument='<%# Eval("rule_id") %>'
                                                    CssClass='<%# Eval("active_css") %>' ToolTip="กดเพื่อเปิด/ปิดกฎ"><%# Eval("active_text") %></asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="กฎ" ItemStyle-Width="200px">
                                            <ItemTemplate>
                                                <div class="st-name"><%# HttpUtility.HtmlEncode(Eval("rule_name").ToString()) %></div>
                                                <div class="st-id">#<%# Eval("rule_id") %> · Priority <%# Eval("priority") %></div>
                                                <div class="st-remark"><%# HttpUtility.HtmlEncode(Eval("remark").ToString()) %></div>
                                                <div class="st-upd"><%# HttpUtility.HtmlEncode(Eval("update_info").ToString()) %></div>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="เงื่อนไข">
                                            <ItemTemplate>
                                                <div><%# Eval("cond_html") %></div>
                                                <div style="margin-top:4px;"><span class="st-badge"><%# Eval("score_text") %></span></div>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Status Use" ItemStyle-Width="100px">
                                            <ItemTemplate><%# Eval("status_name").ToString() == "" ? "<span class='st-muted'>—</span>" : HttpUtility.HtmlEncode(Eval("status_name").ToString()) %></ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="ผู้รับผิดชอบ (⭐ = Auto)" ItemStyle-Width="230px">
                                            <ItemTemplate><%# Eval("resp_html") %></ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="" ItemStyle-Width="120px">
                                            <ItemTemplate>
                                                <div class="st-actions">
                                                    <asp:LinkButton runat="server" CommandName="EditRule" CommandArgument='<%# Eval("rule_id") %>'
                                                        CssClass="st-btn st-btn-light st-btn-sm">✏️ แก้ไข</asp:LinkButton>
                                                    <asp:LinkButton runat="server" CommandName="DeleteRule" CommandArgument='<%# Eval("rule_id") %>'
                                                        CssClass="st-btn st-btn-danger-lt st-btn-sm" ToolTip="ลบกฎ"
                                                        OnClientClick='<%# "return confirmDelete(" + HttpUtility.JavaScriptStringEncode(Eval("rule_name").ToString(), true) + ");" %>'>🗑</asp:LinkButton>
                                                </div>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </div>

                        <%-- ===== ขวา : ทดสอบ + ประวัติ ===== --%>
                        <div>
                            <div class="st-test">
                                <div class="st-sec">🧪 ทดสอบกฎ</div>
                                <div class="st-field">
                                    <label>Dept</label>
                                    <asp:DropDownList ID="ddlTestDept" runat="server" CssClass="st-input st-s2" data-ph="-- Dept --" />
                                </div>
                                <div class="st-field">
                                    <label>Cell</label>
                                    <asp:DropDownList ID="ddlTestCell" runat="server" CssClass="st-input st-s2" data-ph="-- Cell (ไม่ระบุ) --" />
                                </div>
                                <div class="st-field">
                                    <label>Station</label>
                                    <asp:DropDownList ID="ddlTestStation" runat="server" CssClass="st-input st-s2" data-ph="-- Station (ไม่ระบุ) --" />
                                </div>
                                <asp:LinkButton ID="btnTest" runat="server" CssClass="st-btn st-btn-primary" OnClick="btnTest_Click">▶ ทดสอบ</asp:LinkButton>
                                <asp:Literal ID="litTestResult" runat="server" />
                            </div>

                            <div class="st-hist">
                                <div class="st-sec">🕓 ประวัติล่าสุด</div>
                                <div class="st-tbl-wrap" style="max-height:360px;">
                                    <asp:GridView ID="gvHistory" runat="server" CssClass="st-tbl" GridLines="None" CellSpacing="0"
                                        AutoGenerateColumns="false" EmptyDataText="<div style='padding:14px;color:#98a2ad;'>ยังไม่มีประวัติ</div>">
                                        <Columns>
                                            <asp:TemplateField HeaderText="เมื่อ / โดย">
                                                <ItemTemplate>
                                                    <%# Eval("action_date", "{0:dd-MMM-yy HH:mm}") %><br />
                                                    <span class="st-muted"><%# HttpUtility.HtmlEncode(Eval("action_by").ToString()) %></span>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="รายการ">
                                                <ItemTemplate>
                                                    <span class="st-act"><%# Eval("action") %></span> <span class="st-id">#<%# Eval("rule_id") %></span>
                                                    <div style="white-space:pre-wrap; margin-top:2px;"><%# HttpUtility.HtmlEncode(Eval("detail").ToString()) %></div>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                                </div>
                            </div>
                        </div>

                    </div>
                </div>
            </div>

            <%-- ===== Modal : เพิ่ม / แก้ไขกฎ ===== --%>
            <asp:Panel ID="pnlEditor" runat="server" CssClass="st-overlay" Style="display:none;">
                <div class="st-box">
                    <div class="st-mhead">
                        <asp:Label ID="lblEditorTitle" runat="server" Text="เพิ่มกฎ Auto Assign" />
                        <button type="button" class="st-x" onclick="return closeEditor();" aria-label="ปิด">&times;</button>
                    </div>

                    <div class="st-mbody">
                        <asp:Literal ID="litEditorError" runat="server" />

                        <div class="st-form">
                            <div class="st-field">
                                <label>ชื่อกฎ *</label>
                                <asp:TextBox ID="txtRuleName" runat="server" CssClass="st-input" MaxLength="100" placeholder="เช่น 5510/5515 QC" />
                            </div>
                            <div class="st-field" style="display:flex; gap:10px; align-items:flex-end;">
                                <div style="flex:1;">
                                    <label>Priority <span class="st-muted">(ใช้เมื่อเจาะจงเท่ากัน · มากกว่าชนะ)</span></label>
                                    <asp:TextBox ID="txtPriority" runat="server" CssClass="st-input" Text="0" MaxLength="5" />
                                </div>
                                <asp:CheckBox ID="chkActive" runat="server" CssClass="st-chk" Text="เปิดใช้งาน" Checked="true" />
                            </div>

                            <%-- เงื่อนไข --%>
                            <div class="st-group full">
                                <div class="st-group-title">เงื่อนไข <small>— เลือกได้หลายค่า (ค่าในช่องเดียวกัน = หรือ)</small></div>
                                <div class="st-form">
                                    <div class="st-field full">
                                        <label>Dept</label>
                                        <asp:ListBox ID="lbDept" runat="server" SelectionMode="Multiple" CssClass="st-input st-s2" data-ph="ทุก Dept (ไม่ระบุ)" />
                                    </div>
                                    <div class="st-field">
                                        <label>Cell</label>
                                        <asp:ListBox ID="lbCell" runat="server" SelectionMode="Multiple" CssClass="st-input st-s2" data-ph="ไม่ระบุ Cell" />
                                    </div>
                                    <div class="st-field">
                                        <label>Station</label>
                                        <asp:ListBox ID="lbStation" runat="server" SelectionMode="Multiple" CssClass="st-input st-s2" data-ph="ไม่ระบุ Station" />
                                    </div>
                                    <div class="st-field full">
                                        <label>ถ้าเลือกทั้ง Cell และ Station</label>
                                        <asp:RadioButtonList ID="rblLocLogic" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" CssClass="st-rbl">
                                            <asp:ListItem Value="OR" Selected="True" Text="ตรง Cell หรือ Station อย่างใดอย่างหนึ่ง (OR)" />
                                            <asp:ListItem Value="AND" Text="ต้องตรงทั้ง Cell และ Station (AND)" />
                                        </asp:RadioButtonList>
                                    </div>
                                </div>
                            </div>

                            <%-- ผลลัพธ์ --%>
                            <div class="st-group full">
                                <div class="st-group-title">ผลลัพธ์ <small>— กำหนดอย่างน้อย 1 อย่าง</small></div>
                                <div class="st-form">
                                    <div class="st-field">
                                        <label>Status Use</label>
                                        <asp:DropDownList ID="ddlStatusUse" runat="server" CssClass="st-input" />
                                    </div>
                                    <div class="st-field">
                                        <label>เพิ่มผู้รับผิดชอบ <span class="st-muted">(คนแรก = เลือกอัตโนมัติ)</span></label>
                                        <div style="display:flex; gap:6px;">
                                            <div style="flex:1; min-width:0;">
                                                <asp:DropDownList ID="ddlRespAdd" runat="server" CssClass="st-input st-s2" data-ph="-- เลือกผู้รับผิดชอบ --" />
                                            </div>
                                            <asp:LinkButton ID="btnRespAdd" runat="server" CssClass="st-btn st-btn-ok" OnClick="btnRespAdd_Click">＋</asp:LinkButton>
                                        </div>
                                    </div>
                                    <div class="st-field full">
                                        <asp:PlaceHolder ID="litRespEmpty" runat="server">
                                            <div class="st-muted" style="padding:6px 0;">— ยังไม่กำหนด : ใช้รายชื่อตาม Dept (Responsible_Dept) —</div>
                                        </asp:PlaceHolder>
                                        <ul class="st-resp-edit">
                                            <asp:Repeater ID="rptResp" runat="server" OnItemCommand="rptResp_ItemCommand">
                                                <ItemTemplate>
                                                    <li class='<%# Eval("li_css") %>'>
                                                        <span class="no"><%# Convert.ToInt32(Eval("idx")) + 1 %></span>
                                                        <span class="nm"><%# HttpUtility.HtmlEncode(Eval("label").ToString()) %></span>
                                                        <%# Convert.ToInt32(Eval("idx")) == 0 ? "<span class='auto'>⭐ Auto</span>" : "" %>
                                                        <asp:LinkButton runat="server" CommandName="Up" CommandArgument='<%# Eval("idx") %>' CssClass="st-icon-btn" ToolTip="เลื่อนขึ้น">▲</asp:LinkButton>
                                                        <asp:LinkButton runat="server" CommandName="Down" CommandArgument='<%# Eval("idx") %>' CssClass="st-icon-btn" ToolTip="เลื่อนลง">▼</asp:LinkButton>
                                                        <asp:LinkButton runat="server" CommandName="Remove" CommandArgument='<%# Eval("idx") %>' CssClass="st-icon-btn del" ToolTip="เอาออก">✕</asp:LinkButton>
                                                    </li>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </ul>
                                    </div>
                                </div>
                            </div>

                            <div class="st-field full">
                                <label>หมายเหตุ</label>
                                <asp:TextBox ID="txtRemark" runat="server" CssClass="st-input" MaxLength="250" placeholder="เช่น เหตุผลของกฎ / ผู้ขอให้ตั้ง" />
                            </div>
                        </div>
                    </div>

                    <div class="st-mfoot">
                        <asp:LinkButton ID="btnEditorCancel" runat="server" CssClass="st-btn st-btn-light" OnClick="btnEditorCancel_Click">ยกเลิก</asp:LinkButton>
                        <asp:LinkButton ID="btnEditorSave" runat="server" CssClass="st-btn st-btn-ok" OnClick="btnEditorSave_Click">💾 บันทึกกฎ</asp:LinkButton>
                    </div>
                </div>
            </asp:Panel>

        </ContentTemplate>
    </asp:UpdatePanel>
    </asp:Panel>
</asp:Content>
