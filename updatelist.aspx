<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="updatelist.aspx.cs"
    Inherits="Tool_Equipment_Control_System.UpdateEquipmentData" %>

<asp:Content ID="Tool_Equipment_Home" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <link href="css/bootstrap_sum.css" rel="stylesheet">
    <link href="CSS/bootstrap.css" rel="stylesheet" />
    <link href="CSS/w3v4.10.css" rel="stylesheet" />
    <link rel="stylesheet" href="Content/font-awesome.css">
    <link href="css/styles.css" rel="stylesheet">
    <link href="CSS/styleMenu.css" rel="stylesheet" />
    <link href="CSS/font-awesome.css" rel="stylesheet" />
    <link href="DataTables-1.10.18/css/dataTables.css" rel="stylesheet">
    <link href="Content/chosen.css" rel="stylesheet">
    <link href="CSS/fontsgoogleapis.css" rel="stylesheet">
    <link href="CSS/all.min.css" rel="stylesheet">
    <script src="DataTables-1.10.18/js/dataTables.js"></script>
    <script src="DataTables-1.10.18/FixedColumns-3.2.5/js/fixedColumns.dataTables.js"></script>
    <script src="Scripts/chosen.jquery.js"></script>
    <link href="CSS/font-awesome4.7.0.min.css" rel="stylesheet" />

    <script src="Scripts/jquery-1.9.1.js"></script>
    <script src="Scripts/script.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>

    <style>    
        /* ===== Upload Zone ===== */
        .upload-zone {
            border: 2px dashed #4a90d9;
            border-radius: 10px;
            background-color: #f0f7ff;
            padding: 40px 20px;
            text-align: center;
            cursor: pointer;
            transition: background-color 0.2s, border-color 0.2s;
            position: relative;
        }
        .upload-zone.dragover {
            background-color: #d6eaff;
            border-color: #1a6bbf;
        }
        .upload-zone i {
            font-size: 48px;
            color: #4a90d9;
            margin-bottom: 10px;
            display: block;
        }
        .upload-zone p {
            margin: 0;
            color: #555;
            font-size: 15px;
        }
        .upload-zone .file-hint {
            font-size: 12px;
            color: #999;
            margin-top: 6px;
        }
        .upload-zone input[type="file"] {
            position: absolute;
            top: 0; left: 0;
            width: 100%; height: 100%;
            opacity: 0;
            cursor: pointer;
        }

        /* ===== File Info Badge ===== */
        #fileInfo {
            display: none;
            margin-top: 10px;
        }

        /* ===== GridView wrapper ===== */
        .grid-wrapper {
            /*overflow-x: auto;*/
            /*margin-top: 20px;*/
            
            max-height: 500px;        /* ปรับความสูงได้ */
            overflow: auto;
            position: relative;
            border: 1px solid #ddd;

        }
        /* บังคับให้มี Scroll แนวนอนเมื่อ column เยอะ */
        .grid-wrapper table {
            /*width: 100%;*/
            /*border-collapse: collapse;*/
            /*font-size: 13px;*/
            
            min-width: 2200px;        /* ให้พอกับ column Excel */
            border-collapse: separate !important;
            border-spacing: 0;

        }        
        /* ✅ Fix Header */
        .grid-wrapper thead th {
            position: sticky;
            top: 0;
            background: #2c6fad;
            color: #ffffff;
            z-index: 20;
            text-align: center;
            white-space: nowrap;
            padding:4px 8px !important;
            font-size:12px;
        }
        /* ไม่ให้ cell ขึ้นบรรทัดใหม่ */
        .grid-wrapper th {
            /*background-color: #2c6fad;*/
            /*color: #fff;*/
            /*padding: 8px 10px;*/
            /*text-align: center;*/
            white-space: nowrap;
        }        
        /* Fix Header checkbox ให้ชัด */
        .grid-wrapper thead th input {
            cursor: pointer;
        }
        .grid-wrapper td,tr {
            padding: 6px 10px !important;
            /*border-bottom: 1px solid #ddd;*/
            /*vertical-align: middle;*/
            text-align: center;
            align-content: center;
            font-size:12px;
        }
        /*.grid-wrapper tr:hover {
            background-color: #f0f7ff;
        }
        .grid-wrapper td:first-child,
        .grid-wrapper th:first-child {
            text-align: center;
            width: 50px;
        }*/

        /* ===== Action Bar ===== */
        .action-bar {
            margin-top: 15px;
            display: flex;
            align-items: center;
            gap: 10px;
            flex-wrap: wrap;
        }
        .action-bar .select-info {
            font-size: 13px;
            color: #555;
        }

        /* ===== Status message ===== */
        #statusMsg {
            display: none;
            margin-top: 10px;
        }
    </style>

    <%--GridView + Scroll + Sticky Header--%> 
    <style>
        .grid-wrapper {
            width: 100%;
            max-height: 730px;        /* Scroll Y */
            overflow: auto;
        }

        .grid-wrapper table {
            min-width: 1800px;        /* Scroll X */
            border-collapse: separate;
        }

        .grid-wrapper thead th {
            position: sticky;
            top: 0;
            background-color: #2c6fad;
            color: white;
            z-index: 10;
            white-space: nowrap;
            padding : 2px 5px;
        }
    </style>

    <style>
        .ddl_edit{
            font-size: 12px;
            text-align: center;
        }        
    .status-similar {
        background: #fff3cd;
        color: #856404;
    }
    </style>

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



    <asp:ScriptManager runat="server" ID="scriptManager" EnablePageMethods="true"></asp:ScriptManager>

    <asp:UpdatePanel runat="server" ID="UP1">
        <ContentTemplate>

            <%---- Toast Container ----%>
            <div id="toast-container" aria-live="polite" aria-atomic="true"></div>

            <div class="container-fluid" style="padding: 5px 20px; margin-top:30px;">

                <!-- ===== Page Header ===== -->
                <div class="row">
                    <div class="col-md-12">
                        <h4 style="color:#2c6fad; font-weight:bold;">
                            <i class="fa fa-upload"></i> Import Equipment Data
                        </h4>
                        <hr />
                    </div>
                </div>

                <!-- ===== Upload Section ===== -->
                <div class="row">
                    <div class="col-md-8 col-md-offset-2">

                        <div class="upload-zone" id="dropZone">
                            <asp:FileUpload ID="fileUpload" runat="server"
                                accept=".xlsx,.xls,.csv"
                                ClientIDMode="Static" />
                            <i class="fa fa-file-excel-o"></i>
                            <p><strong>คลิกเพื่อเลือกไฟล์</strong> หรือ ลากไฟล์มาวางที่นี่</p>
                            <p class="file-hint">รองรับไฟล์ Excel (.xlsx, .xls) และ CSV (.csv) เท่านั้น</p>
                        </div>

                        <!-- File info after selected -->
                        <div id="fileInfo" class="alert alert-info" style="margin-top:10px;">
                            <i class="fa fa-file-text-o"></i>
                            <span id="fileName"></span>
                            <span class="pull-right" style="cursor:pointer;" id="clearFile">
                                <i class="fa fa-times"></i> ล้าง
                            </span>
                        </div>

                        <!-- Upload Button -->
                        <div style="margin-top:12px; text-align:center;">
                            <asp:Button ID="btnUpload" runat="server" Text="&#xf093;  Upload &amp; Preview"
                                CssClass="btn btn-primary btn-md"
                                OnClick="btnUpload_Click"
                                UseSubmitBehavior="true"
                                style="font-family:inherit;" />
                        </div>

                        <!-- Status Message -->
                        <asp:Label ID="lblStatus" runat="server" ClientIDMode="Static"></asp:Label>
                        <div id="statusMsg"></div>

                    </div>
                </div>

                <!-- ===== GridView Section ===== -->
                <asp:Panel ID="pnlGrid" runat="server" Visible="false">
                    <div class="row" style="margin-top:25px;">

                        <div class="col-md-12">
                            <div style="background:#fff; border:1px solid #ddd; border-radius:8px; padding:15px;">

                                <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:10px;">
                                    <h5 style="margin:0; color:#333;">
                                        <i class="fa fa-table"></i>
                                        ข้อมูลในไฟล์
                                        <asp:Label ID="lblRowCount" runat="server"
                                            CssClass="badge" style="background:#2c6fad;"></asp:Label>
                                        รายการ
                                    </h5>
                                    <asp:Label ID="lblFileName" runat="server"
                                        style="font-size:12px; color:#888;"></asp:Label>
                                </div>

                                <!-- Action Bar (Select All + Save Button) -->
                                <div class="action-bar">

                                    <label style="margin:0; cursor:pointer; font-size:13px;">
                                        <input type="checkbox" id="chkHeaderAll" title="เลือกทั้งหมด" onclick="toggleAll(this)" />
                                        <strong> เลือกทั้งหมด</strong>
                                    </label>

                                    <span class="select-info">
                                        เลือกแล้ว: <strong id="selectedCount">0</strong> รายการ
                                    </span>

                                    <%--Save Button--%>
                                    <asp:Button ID="btnSaveSelected" runat="server"
                                        Text="&#xf0c7;  บันทึกข้อมูลที่เลือก"
                                        CssClass="btn btn-success btn-sm"
                                        OnClick="btnSaveSelected_Click"
                                        OnClientClick="return confirmSave();"
                                        style="font-family:inherit;" />
                                    <%--Clear Button--%>
                                    <asp:Button ID="btnClear" runat="server"
                                        Text="&#xf021;  ล้างข้อมูล"
                                        CssClass="btn btn-default btn-sm"
                                        OnClick="btnClear_Click"
                                        style="font-family:inherit;" />
                                    <%--Preview Button--%>
                                    <asp:Button ID="btnPreview" runat="server"
                                        Text="Preview Update"
                                        CssClass="btn btn-info btn-sm"
                                        OnClick="btnPreview_Click"
                                        style="font-family:inherit;"/>
                                    <%--Export Error Button--%>                                                
                                    <asp:Button ID="btnExportExcel"
                                        runat="server"
                                        Text="Export Error Excel"
                                        CssClass="btn btn-warning btn-sm"
                                        OnClick="btnExportExcel_Click"
                                        style="font-family:inherit;"/>

                                </div>

                                <!-- GridView -->
                                <div class="grid-wrapper">
                                    <asp:GridView ID="gvData" runat="server"
                                        CssClass="table table-bordered table-hover"
                                        AutoGenerateColumns="False"
                                        OnRowDataBound="gvData_RowDataBound"
                                        EmptyDataText="ไม่พบข้อมูล">
                                        <Columns>

                                            <%--CheckBox Column--%>
                                            <asp:TemplateField>
                                                <%--<HeaderTemplate>
                                                    <input type="checkbox" id="chkHeaderAll" title="เลือกทั้งหมด" onclick="toggleAll(this)" />
                                                </HeaderTemplate>--%>
                                                <ItemTemplate>
                                                    <asp:CheckBox ID="chkRow" runat="server"
                                                        CssClass="row-checkbox" />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%--Row Number--%>
                                            <asp:TemplateField HeaderText="#">
                                                <ItemTemplate>
                                                    <%# Container.DataItemIndex + 1 %>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%--Fix Asset--%>
                                            <asp:TemplateField HeaderText="Fix Asset">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblFixAsset" runat="server"></asp:Label>
                                                    </br>
                                                    <asp:TextBox ID="txtFixAsset"
                                                        runat="server"
                                                        CssClass="statusBox fix-asset"
                                                        ReadOnly="true"
                                                        Width="95px"
                                                        Style="text-align:center; font-size:12px;" />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%--Sub Fix--%>
                                            <asp:TemplateField HeaderText="Sub Fix">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblSubfix" runat="server"></asp:Label>
                                                    </br>
                                                    <asp:TextBox ID="txtSubfix"
                                                        runat="server"
                                                        CssClass="statusBox fix-asset"
                                                        ReadOnly="false"
                                                        Width="50px"
                                                        Style="text-align:center; font-size:12px;" />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%--Control No--%>
                                            <asp:TemplateField HeaderText="Control No">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblControlNo" runat="server"></asp:Label>
                                                    </br>
                                                    <asp:TextBox ID="txtControlNo"
                                                        runat="server"
                                                        CssClass="statusBox control-no"
                                                        ReadOnly="false"
                                                        Width="110px"
                                                        Style="text-align:center; font-size:12px;" />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%--Tool No--%>
                                            <asp:TemplateField HeaderText="Tool / Equipmemt">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblToolNo" runat="server"></asp:Label>
                                                    </br>
                                                    <asp:TextBox ID="txtToolNo"
                                                        runat="server"
                                                        CssClass="statusBox tool-no"
                                                        ReadOnly="true"
                                                        Width="140px"
                                                        Style="text-align:center; font-size:12px;" />
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%--Edit Status--%>
                                            <asp:TemplateField HeaderText="EDIT STATUS">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblStatus" runat="server"></asp:Label>
                                                    </br>
                                                    <asp:DropDownList ID="ddlStatus"
                                                        runat="server"
                                                        CssClass="form-control form-control-sm ddl_edit"
                                                        Style="min-width:80px;">
                                                    </asp:DropDownList>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%--Edit Dept--%>
                                            <asp:TemplateField HeaderText="EDIT DEPT">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblDept" runat="server"></asp:Label>
                                                    </br>
                                                    <asp:DropDownList ID="ddlDept"
                                                        runat="server"
                                                        CssClass="form-control form-control-sm ddl_edit"
                                                        Style="min-width:60px;">
                                                    </asp:DropDownList>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%--Edit Model--%>
                                            <asp:TemplateField HeaderText="EDIT MODEL">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblModel" runat="server"></asp:Label>
                                                    </br>
                                                    <asp:DropDownList ID="ddlModel"
                                                        runat="server"
                                                        CssClass="form-control form-control-sm ddl_edit"
                                                        Style="min-width:115px;">
                                                    </asp:DropDownList>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%--Edit Location--%>
                                            <asp:TemplateField HeaderText="EDIT LOCATION">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblLocation" runat="server"></asp:Label>
                                                    </br>
                                                    <asp:DropDownList ID="ddlLocation"
                                                        runat="server"
                                                        CssClass="form-control form-control-sm ddl_edit"
                                                        Style="min-width:90px;"
                                                        AutoPostBack="true"
                                                        OnSelectedIndexChanged="ddlLocation_SelectedIndexChanged">
                                                    </asp:DropDownList>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%--Edit Cell--%>
                                            <asp:TemplateField HeaderText="EDIT Cell">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblCell" runat="server"></asp:Label>
                                                    </br>
                                                    <asp:DropDownList ID="ddlCell"
                                                        runat="server"
                                                        CssClass="form-control form-control-sm ddl_edit"
                                                        Style="min-width:130px;">
                                                    </asp:DropDownList>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%--Edit Station--%>
                                            <asp:TemplateField HeaderText="EDIT Station">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblStation" runat="server"></asp:Label>
                                                    </br>
                                                    <asp:DropDownList ID="ddlStation"
                                                        runat="server"
                                                        CssClass="form-control form-control-sm ddl_edit"
                                                        Style="min-width:130px;">
                                                    </asp:DropDownList>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%--Edit Responsible--%>
                                            <asp:TemplateField HeaderText="EDIT Responsible">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblResponsible" runat="server"></asp:Label>
                                                    </br>
                                                    <asp:DropDownList ID="ddlResponsible"
                                                        runat="server"
                                                        CssClass="form-control form-control-sm ddl_edit"
                                                        Style="min-width:155px;">
                                                    </asp:DropDownList>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%--Edit BOI--%>
                                            <asp:TemplateField HeaderText="EDIT BOI">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblBoi" runat="server"></asp:Label>
                                                    </br>
                                                    <asp:DropDownList ID="ddlBoi"
                                                        runat="server"
                                                        CssClass="form-control form-control-sm ddl_edit"
                                                        Style="min-width:60px;">
                                                    </asp:DropDownList>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%--Edit PAPER--%>
                                            <asp:TemplateField HeaderText="EDIT PAPER">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblPaper" runat="server"></asp:Label>
                                                    </br>
                                                    <asp:DropDownList ID="ddlPaper"
                                                        runat="server"
                                                        CssClass="form-control form-control-sm ddl_edit"
                                                        Style="min-width:60px;">
                                                    </asp:DropDownList>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%--Edit Calibration Period--%>
                                            <asp:TemplateField HeaderText="EDIT PERIOD">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblPeriod" runat="server"></asp:Label>
                                                    </br>
                                                    <asp:DropDownList ID="ddlPeriod"
                                                        runat="server"
                                                        CssClass="form-control form-control-sm ddl_edit"
                                                        Style="min-width:60px;">
                                                    </asp:DropDownList>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%------------------------------------------------------%>
                                            <%-------------------SQL Data Detail--------------------%>
                                            <%------------------------------------------------------%>

                                            <%-- SQL : TOOL ID --%>
                                            <asp:TemplateField HeaderText="Tool_Id">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_toolId" runat="server" Text='<%# Eval("SQL_ToolId") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- SQL : TOOL SUB ID --%>
                                            <asp:TemplateField HeaderText="Tool_SubId">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_toolsubId" runat="server" Text='<%# Eval("SQL_SubId") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- SQL : SYSTEM ID --%>
                                            <asp:TemplateField HeaderText="System_Id">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_systemId" runat="server" Text='<%# Eval("SQL_System") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- SQL : DEPT ID --%>
                                            <asp:TemplateField HeaderText="SQL_DeptCode">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_sqldeptcode" runat="server" Text='<%# Eval("SQL_DeptCode") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- SQL : MODEL --%>
                                            <asp:TemplateField HeaderText="SQL_MODEL">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_sqlmodel" runat="server" Text='<%# Eval("SQL_MODEL") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- SQL : CELL --%>
                                            <asp:TemplateField HeaderText="SQL_Cell">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_sqlcell" runat="server" Text='<%# Eval("SQL_Cell") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- SQL : STATION --%>
                                            <asp:TemplateField HeaderText="SQL_Station">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_sqlstation" runat="server" Text='<%# Eval("SQL_Station") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- SQL : RESPONSIBLE --%>
                                            <asp:TemplateField HeaderText="SQL_Responsible">
                                                 <ItemTemplate>
                                                     <asp:Label ID="lbl_sqlresponsible" runat="server" Text='<%# Eval("SQL_Responsible") %>'></asp:Label>
                                                 </ItemTemplate>
                                             </asp:TemplateField>

                                            <%-- SQL : BOI --%>
                                            <asp:TemplateField HeaderText="SQL_Boi">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_sqlboi" runat="server" Text='<%# Eval("SQL_BOI") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- SQL : PAPER --%>
                                            <asp:TemplateField HeaderText="SQL_Paper">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_sqlpaper" runat="server" Text='<%# Eval("SQL_PAPER") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>


                                            <%--------------------------------------------------------%>
                                            <%-------------------Excel Data Detail--------------------%>
                                            <%--------------------------------------------------------%>

                                            <%-- Fix Asset Name --%>
                                            <asp:TemplateField HeaderText="Name">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_fixassetName" runat="server" Text='<%# Eval("NAME") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- MODEL (Tool No or Model No) --%>
                                            <asp:TemplateField HeaderText="Name">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_modelName" runat="server" Text='<%# Eval("MODEL") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- SERIAL NO --%>
                                            <asp:TemplateField HeaderText="SERIAL NO">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_serialNo" runat="server" Text='<%# Eval("SERIAL NO") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- MANUFACTURE --%>
                                            <asp:TemplateField HeaderText="MANUFACTURE">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_manufacture" runat="server" Text='<%# Eval("MANUFACTURE") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- Price --%>
                                            <asp:TemplateField HeaderText="Price">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_price" runat="server" Text='<%# Eval("PRICE") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- PR NO --%>
                                            <asp:TemplateField HeaderText="PR NO">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_prpo" runat="server" Text='<%# Eval("PR NO") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- OPERATING DATE --%>
                                            <asp:TemplateField HeaderText="OPERATING DATE">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_operationDate" runat="server"
                                                               Text='<%# FormatDate(Eval("OPERATING DATE")) %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- Due Date --%>  <%--Text='<%# Eval("DUE DATE") %>'--%>             
                                            <asp:TemplateField HeaderText="DUE DATE">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_duedate" runat="server"
                                                               Text='<%# FormatDueDate(Eval("DUE DATE")) %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                            <%-- Interval --%>
                                            <%--<asp:TemplateField HeaderText="INTERVAL">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_interval" runat="server"
                                                               Text='<%# Eval("INTERVAL") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>--%>

                                            <%-- Remark --%>
                                            <asp:TemplateField HeaderText="REMAKE">
                                                <ItemTemplate>
                                                    <asp:Label ID="lbl_remark" runat="server"
                                                               Text='<%# FormatRemark(Eval("INTERVAL"),Eval("REMARK")) %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>

                                        </Columns>
                                        <HeaderStyle BackColor="#2c6fad" ForeColor="White" />
                                        <AlternatingRowStyle BackColor="#f9fbff" />
                                    </asp:GridView>
                                </div>

                                <!-- Save Result Message -->
                                <asp:Label ID="lblSaveResult" runat="server"
                                    style="margin-top:10px; display:block;"></asp:Label>

                            </div>
                        </div>

                    </div>
                </asp:Panel>

            </div>

        </ContentTemplate>
        
        <Triggers>
            <asp:PostBackTrigger ControlID="btnUpload" />
            <asp:PostBackTrigger ControlID="btnExportExcel" />
        </Triggers>


    </asp:UpdatePanel>

    <script type="text/javascript">
        // ===== Drag & Drop =====
        var dropZone = document.getElementById('dropZone');
        var fileInput = document.getElementById('fileUpload');

        if (dropZone) {
            dropZone.addEventListener('dragover', function (e) {
                e.preventDefault();
                dropZone.classList.add('dragover');
            });
            dropZone.addEventListener('dragleave', function () {
                dropZone.classList.remove('dragover');
            });
            dropZone.addEventListener('drop', function (e) {
                e.preventDefault();
                dropZone.classList.remove('dragover');
                if (e.dataTransfer.files.length > 0) {
                    // Set file to input (HTML5 way)
                    var dt = new DataTransfer();
                    dt.items.add(e.dataTransfer.files[0]);
                    fileInput.files = dt.files;
                    showFileInfo(e.dataTransfer.files[0].name);
                }
            });
        }

        // ===== File selected via click =====
        if (fileInput) {
            fileInput.addEventListener('change', function () {
                if (this.files.length > 0) {
                    showFileInfo(this.files[0].name);
                }
            });
        }

        function showFileInfo(name) {
            var info = document.getElementById('fileInfo');
            var nameSpan = document.getElementById('fileName');
            if (info && nameSpan) {
                nameSpan.textContent = ' ' + name;
                info.style.display = 'block';
            }
        }

        // ===== Clear File =====
        var clearBtn = document.getElementById('clearFile');
        if (clearBtn) {
            clearBtn.addEventListener('click', function () {
                if (fileInput) fileInput.value = '';
                var info = document.getElementById('fileInfo');
                if (info) info.style.display = 'none';
            });
        }

        // ===== Select All Checkbox (Header) =====
        $(document).on('change', '#chkSelectAll, #chkHeaderAll', function () {
            var checked = $(this).prop('checked');
            $('.row-checkbox input[type=checkbox]').prop('checked', checked);
            // sync both checkboxes
            $('#chkSelectAll').prop('checked', checked);
            $('#chkHeaderAll').prop('checked', checked);
            updateSelectedCount();
        });

        // ===== Individual row checkbox =====
        $(document).on('change', '.row-checkbox input[type=checkbox]', function () {
            updateSelectedCount();
            var total = $('.row-checkbox input[type=checkbox]').length;
            var checked = $('.row-checkbox input[type=checkbox]:checked').length;
            $('#chkSelectAll').prop('checked', total === checked);
            $('#chkHeaderAll').prop('checked', total === checked);
        });

        function updateSelectedCount() {
            var count = $('.row-checkbox input[type=checkbox]:checked').length;
            $('#selectedCount').text(count);
        }

        // ===== Confirm before save =====
        function confirmSave() {
            var count = parseInt($('#selectedCount').text());
            if (count === 0) {
                alert('กรุณาเลือกรายการที่ต้องการบันทึกก่อน');
                return false;
            }
            return confirm('ยืนยันการบันทึกข้อมูล ' + count + ' รายการ ลง SQL Server?');
        }

        // ===== Init count on load =====
        $(function () {
            updateSelectedCount();
        });
    </script>

</asp:Content>
