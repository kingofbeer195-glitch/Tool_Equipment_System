<%@ Page Title="Auto Bot" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="AutoBot.aspx.cs"
    Inherits="Tool_Equipment_Control_System.AutoBot" EnableEventValidation="false" %>

<asp:Content ID="AutoBot" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

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


    <style type="text/css">
        .bot-wrap { font-family: 'Segoe UI', Tahoma, Arial, sans-serif; font-size: 13px; color: #1e293b; }

        /* ── Header ── */
        .bot-header {
            background: linear-gradient(135deg,#1d4ed8,#3b82f6);
            color: #fff; padding: 16px 24px; border-radius: 8px; margin-bottom: 16px;
        }
        .bot-header h2 { margin: 0; font-size: 18px; }
        .bot-header p  { margin: 4px 0 0; opacity: .85; font-size: 13px; }

        /* ── Tabs ── */
        .bot-tabs { border-bottom: 2px solid #e2e8f0; margin-bottom: 16px; }
        .bot-tab {
            display: inline-block; padding: 10px 20px; margin-right: 4px; border: none;
            background: #f1f5f9; color: #475569; font-size: 13px; font-weight: 600;
            border-radius: 8px 8px 0 0; cursor: pointer; text-decoration: none;
        }
        .bot-tab:hover { background: #e2e8f0; color: #1e293b; text-decoration: none; }
        .bot-tab.active { background: #1d4ed8; color: #fff; }

        /* ── Toolbar ── */
        .bot-toolbar {
            display: block; padding: 12px 14px; background: #f8fafc;
            border: 1px solid #e2e8f0; border-radius: 8px; margin-bottom: 14px;
        }
        .bot-toolbar .fld { display: inline-block; margin-right: 14px; vertical-align: middle; }
        .bot-toolbar label { display: block; font-size: 11px; color: #64748b; margin-bottom: 3px; }
        .bot-toolbar input[type=text], .bot-toolbar select {
            height: 30px; padding: 3px 8px; border: 1px solid #cbd5e1; border-radius: 6px; font-size: 13px;
        }

        .btn {
            height: 32px; padding: 0 16px; border: none; border-radius: 6px;
            font-size: 13px; font-weight: 600; cursor: pointer; color: #fff;
        }
        .btn-primary { background: #1d4ed8; }
        .btn-primary:hover { background: #1e40af; }
        .btn-success { background: #15803d; }
        .btn-success:hover { background: #166534; }
        .btn-light   { background: #e2e8f0; color: #334155; }
        .btn-light:hover { background: #cbd5e1; }
        .btn-mini {
            height: 26px; padding: 0 10px; border: none; border-radius: 5px;
            background: #0ea5e9; color: #fff; font-size: 11px; font-weight: 600; cursor: pointer;
        }
        .btn-mini:hover { background: #0284c7; }

        /* ── Grid ── */
        .bot-grid { width: 100%; border-collapse: collapse; background: #fff; }
        .bot-grid th {
            background: #1d4ed8; color: #fff; padding: 9px 10px; font-size: 12px;
            text-align: center; white-space: nowrap;
        }
        .bot-grid td { padding: 7px 10px; border-bottom: 1px solid #e2e8f0; font-size: 12.5px; }
        .bot-grid tr:hover td { background: #f1f5f9; }
        .row-overdue td { background: #fef2f2; }
        .row-overdue:hover td { background: #fee2e2; }

        .pill {
            display: inline-block; padding: 2px 9px; border-radius: 999px;
            font-size: 11px; font-weight: 600;
        }
        .pill-warn { background: #ffedd5; color: #c2410c; }
        .pill-late { background: #fee2e2; color: #b91c1c; }
        .pill-ok   { background: #dcfce7; color: #15803d; }

        /* ── Alert box ── */
        .bot-msg { padding: 11px 16px; border-radius: 8px; margin-bottom: 14px; font-size: 13px; }
        .msg-ok   { background: #dcfce7; border: 1px solid #86efac; color: #15803d; }
        .msg-err  { background: #fee2e2; border: 1px solid #fca5a5; color: #b91c1c; }
        .msg-info { background: #e0f2fe; border: 1px solid #7dd3fc; color: #0369a1; }

        .bot-empty { padding: 34px; text-align: center; color: #94a3b8; }
        .bot-count { float: right; color: #64748b; font-size: 12px; padding-top: 8px; }
    </style>

    <script type="text/javascript">
        // เลือก / ไม่เลือก ทั้งหมด
        function BotToggleAll(src, gridId) {
            var grid = document.getElementById(gridId);
            if (!grid) return;
            var inputs = grid.getElementsByTagName('input');
            for (var i = 0; i < inputs.length; i++) {
                if (inputs[i].type === 'checkbox' && inputs[i] !== src) {
                    inputs[i].checked = src.checked;
                }
            }
        }

        function BotConfirmSend() {
            return confirm('ยืนยันการส่ง E-Mail แจ้งเตือนสำหรับรายการที่เลือก ?');
        }
    </script>


    <asp:ScriptManager runat="server" ID="scriptManager" EnablePageMethods="true"></asp:ScriptManager>

    <asp:UpdatePanel runat="server" ID="UP1">
         <ContentTemplate>

            <div class="bot-wrap" style="margin-top: 30px; margin-left: 10px; width: calc(100vw - 110px);">

                <div class="bot-header">
                    <h2>&#129302; Auto Bot</h2>
                    <p>ระบบแจ้งเตือนอัตโนมัติ &ndash; Tool &amp; Equipment System</p>
                </div>

                <%-- ══════════════ TABS ══════════════ --%>
                <div class="bot-tabs">
                    <asp:LinkButton ID="tabAlert" runat="server" CssClass="bot-tab active"
                        OnClick="Tab_Click" CommandArgument="0" CausesValidation="false">&#128226; Auto Alert</asp:LinkButton>
                    <asp:LinkButton ID="tabLog" runat="server" CssClass="bot-tab"
                        OnClick="Tab_Click" CommandArgument="1" CausesValidation="false">&#128203; Send Log</asp:LinkButton>
                    <asp:LinkButton ID="tabSetting" runat="server" CssClass="bot-tab"
                        OnClick="Tab_Click" CommandArgument="2" CausesValidation="false">&#9881; Setting</asp:LinkButton>
                </div>

                <asp:Panel ID="pnlMsg" runat="server" Visible="false">
                    <div runat="server" id="divMsg" class="bot-msg msg-ok">
                        <asp:Literal ID="litMsg" runat="server" />
                    </div>
                </asp:Panel>

                <asp:MultiView ID="mvBot" runat="server" ActiveViewIndex="0">

                    <%-- ══════════════ TAB 1 : AUTO ALERT ══════════════ --%>
                    <asp:View ID="vAlert" runat="server">

                        <div class="bot-toolbar">
                            <div class="fld">
                                <label>ค้นหา (Doc No. / Emp ID / Dept)</label>
                                <asp:TextBox ID="txtKeyword" runat="server" Width="240" />
                            </div>
                            <div class="fld">
                                <label>สถานะกำหนดคืน</label>
                                <asp:DropDownList ID="ddlDue" runat="server">
                                    <asp:ListItem Value="ALL" Text="ทั้งหมด" />
                                    <asp:ListItem Value="OVERDUE" Text="เกินกำหนดคืน" />
                                    <asp:ListItem Value="NEAR" Text="ใกล้ครบกำหนด (&le; 3 วัน)" />
                                </asp:DropDownList>
                            </div>
                            <div class="fld">
                                <label>&nbsp;</label>
                                <asp:Button ID="btnSearch" runat="server" CssClass="btn btn-light"
                                    Text="🔎 ค้นหา" OnClick="btnSearch_Click" />
                                <asp:Button ID="btnReload" runat="server" CssClass="btn btn-light"
                                    Text="🔄 Refresh" OnClick="btnReload_Click" />
                            </div>
                            <div class="fld">
                                <label>&nbsp;</label>
                                <asp:Button ID="btnSendSelected" runat="server" CssClass="btn btn-success"
                                    Text="📨 ส่ง E-Mail (Manual)" OnClick="btnSendSelected_Click"
                                    OnClientClick="return BotConfirmSend();" />
                            </div>
                            <span class="bot-count"><asp:Literal ID="litCount" runat="server" /></span>
                        </div>

                        <asp:GridView ID="gvAlert" runat="server" AutoGenerateColumns="false"
                            CssClass="bot-grid" GridLines="None" DataKeyNames="Doc_No"
                            OnRowDataBound="gvAlert_RowDataBound"
                            OnRowCommand="gvAlert_RowCommand"
                            EmptyDataText="ไม่พบรายการที่ต้องแจ้งเตือน"
                            EmptyDataRowStyle-CssClass="bot-empty">
                            <Columns>
                                <asp:TemplateField HeaderStyle-Width="40">
                                    <HeaderTemplate>
                                        <input type="checkbox" id="chkAll"
                                            onclick="BotToggleAll(this,'<%= gvAlert.ClientID %>');" />
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkSel" runat="server" />
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Center" />
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="No." HeaderStyle-Width="45">
                                    <ItemTemplate>
                                        <asp:Literal ID="litNo" runat="server" />
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Center" />
                                </asp:TemplateField>

                                <asp:BoundField DataField="Doc_No" HeaderText="Doc No." ItemStyle-HorizontalAlign="Center" />
                                <asp:BoundField DataField="document_id" HeaderText="Doc ID" ItemStyle-HorizontalAlign="Center" />
                                <asp:BoundField DataField="document_name" HeaderText="Document Name" />
                                <asp:BoundField DataField="status_name" HeaderText="Status" ItemStyle-HorizontalAlign="Center" />
                                <asp:BoundField DataField="issue_empid" HeaderText="Issue Emp ID" ItemStyle-HorizontalAlign="Center" />
                                <asp:BoundField DataField="dept_code" HeaderText="Dept" ItemStyle-HorizontalAlign="Center" />

                                <asp:TemplateField HeaderText="Return Due Date" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <asp:Literal ID="litDue" runat="server" />
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="สถานะ" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <asp:Literal ID="litDueTag" runat="server" />
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="ส่งซ้ำ" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="90">
                                    <ItemTemplate>
                                        <asp:Button ID="btnRowSend" runat="server" CssClass="btn-mini"
                                            Text="📨 Send" CommandName="SendOne"
                                            CommandArgument='<%# Eval("Doc_No") %>'
                                            OnClientClick="return confirm('ส่ง E-Mail แจ้งเตือนเอกสารนี้ ?');" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </asp:View>

                    <%-- ══════════════ TAB 2 : SEND LOG ══════════════ --%>
                    <asp:View ID="vLog" runat="server">
                        <asp:GridView ID="gvLog" runat="server" AutoGenerateColumns="false"
                            CssClass="bot-grid" GridLines="None"
                            EmptyDataText="ยังไม่มีประวัติการส่ง" EmptyDataRowStyle-CssClass="bot-empty">
                            <Columns>
                                <asp:BoundField DataField="send_date" HeaderText="วันที่ส่ง"
                                    DataFormatString="{0:dd/MM/yyyy HH:mm}" ItemStyle-HorizontalAlign="Center" />
                                <asp:BoundField DataField="Doc_No" HeaderText="Doc No." ItemStyle-HorizontalAlign="Center" />
                                <asp:BoundField DataField="document_id" HeaderText="Doc ID" ItemStyle-HorizontalAlign="Center" />
                                <asp:BoundField DataField="mail_to" HeaderText="ส่งถึง" />
                                <asp:BoundField DataField="send_type" HeaderText="ประเภท" ItemStyle-HorizontalAlign="Center" />
                                <asp:BoundField DataField="send_result" HeaderText="ผลลัพธ์" ItemStyle-HorizontalAlign="Center" />
                                <asp:BoundField DataField="send_by" HeaderText="ส่งโดย" ItemStyle-HorizontalAlign="Center" />
                            </Columns>
                        </asp:GridView>
                    </asp:View>

                    <%-- ══════════════ TAB 3 : SETTING ══════════════ --%>
                    <asp:View ID="vSetting" runat="server">
                        <div class="bot-msg msg-info">
                            ตั้งค่าปลายทางการส่ง E-Mail (ใช้กับทั้ง Auto และ Manual)
                        </div>
                        <div class="bot-toolbar">
                            <div class="fld">
                                <label>To (คั่นด้วย ;)</label>
                                <asp:TextBox ID="txtMailTo" runat="server" Width="380" />
                            </div>
                            <div class="fld">
                                <label>CC (คั่นด้วย ;)</label>
                                <asp:TextBox ID="txtMailCc" runat="server" Width="380" />
                            </div>
                            <div class="fld">
                                <label>&nbsp;</label>
                                <asp:Button ID="btnSaveSetting" runat="server" CssClass="btn btn-primary"
                                    Text="&#128190; บันทึก" OnClick="btnSaveSetting_Click" />
                            </div>
                        </div>
                    </asp:View>

                </asp:MultiView>
            </div>

        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
