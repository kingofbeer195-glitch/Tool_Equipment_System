<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master" AutoEventWireup="true"
    CodeFile="tool_equipment_tana_report.aspx.cs" Inherits="Tool_Equipment_Control_System.tool_equipment_tana_report" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxtoolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <link href="CSS/bootstrap.css" rel="stylesheet" />

    <link href="CSS/jquery-ui.css" rel="stylesheet" />
    <link href="CSS/font-awesome4.7.0.min.css" rel="stylesheet" />
    <link href="CSS/GridviewScroll.css" rel="stylesheet" />
    <link href="CSS/select2.min.css" rel="stylesheet" />
    <link href="DataTables-1.10.18/dataTables.dataTables.css" rel="stylesheet" />
    <script src="JS/jquery-1.9.1.js"></script>
    <script src="JS/jquery-ui.js"></script>

    <%--<script src="JS/bootstrap.min.js"></script>--%>
    <script src="Scripts/bootstrap.min.js"></script>
    <%--<script src="Scripts/bootstrap-multiselect.js"></script>--%>
    <script src="JS/gridviewScroll.min.js"></script>
    <%--<script src="Scripts/select2.min.js"></script>--%>
    <%--<script src="DataTables-1.10.18/dataTables.min.js"></script>--%>
    <script src="DataTables-1.10.18/jquery.dataTables.1.10.19.min.js"></script>
    <%--Layout Style--%>
    <style>
        .grid-Body {
            display: grid;
            grid-template-columns: 99%;
            /*grid-template-rows: 800px;*/
            grid-template-rows: 100%;
            /*border-radius: 5px;*/
            /*margin-right: 105px;*/
            margin-top: 70px;
            margin-left: 10px;
            /*background-color:white;*/
            overflow: hidden;
        }

        .grid-filter {
            display: grid;
            grid-template-columns: 80% 20%;
            grid-template-rows: 80px;
        }

            .grid-filter > div {
                border: solid 1px;
            }

        .myButton {
            padding: 4px 10px;
            margin-bottom: 0;
            font-size: 14px;
            line-height: 20px;
            color: #333333;
            text-align: center;
            text-shadow: 0 1px 1px rgba(255, 255, 255, 0.75);
            vertical-align: middle;
            background-color: #f5f5f5;
            border: 1px solid #cccccc;
            -webkit-border-radius: 4px;
            -moz-border-radius: 4px;
            border-radius: 4px;
            width: 100px !important;
        }
    </style>

    <%-- Style Gridview page Button  --%>
    <style>
        .dataTables_scrollBody, .tableGv {
            margin-left: 0px !important;
        }

            .tableGv th, .tableGv td {
                text-align: center !important;
                height: 35px !important;
                border-color: darkgray;
                padding: 5px;
            }

        .pagegvMasterList {
            /*background-color : #f2f2f2;*/
            padding: 5px;
            /*text-align:center;*/
            /*text-align:right;*/
            border-top: 1px solid #ddd;
            font-family: Arial,sans-serif;
        }
            /*.pagegvMasterList table{
            margin:0 auto;
        }*/
            .pagegvMasterList a, pagegvMasterList span {
                display: inline-block;
                padding: 2px 9px;
                margin: 0 2px;
                border: 1px solid #ccc;
                background-color: #fff;
                color: #337ab7;
                text-decoration: none;
                border-radius: 4px;
                transition: all 0.3s ease;
            }

                .pagegvMasterList a:hover {
                    background-color: #e0e0e0;
                    color: #23527c;
                    border-color: #a0a0a0;
                }

            .pagegvMasterList span {
                display: inline-block;
                padding: 2px 9px;
                margin: 0 2px;
                border: 1px solid #ccc;
                border-radius: 4px;
                background-color: #337ab7;
                color: #fff;
                border-color: #337ab7;
                font-weight: bold;
                cursor: default;
            }

        .myButton {
            display: block;
            width: 100%;
            padding: 8px 12px;
            border: 1px solid #ccc;
            border-radius: 8px; /* Rounded corners */
            background-color: #f0f0f0;
            cursor: pointer;
            transition: background-color 0.3s, color 0.3s;
            text-align: center;
            font-family: Arial, sans-serif;
            color: black;
        }

            .myButton:hover {
                background-color: #e0e0e0;
            }

            /* Hide the default radio button */
            .myButton input[type="radio"] {
                display: none;
            }

            /* Style for the selected button */
            .myButton.selected {
                background-color: #98FB98; /* Light green for selected state */
                color: black;
            }

        .btn-success {
            background-color: green;
            color: white;
        }

        /* Add custom styling for the GridView and the button group */
        .gv-button-group {
            display: flex;
            flex-direction: column;
            gap: 5px; /* Spacing between the buttons */
        }

        .txt_boxfix {
            cursor: not-allowed !important;
            background-color: silver !important;
            opacity: 1 !important;
        }

        .form-control.disabled-field {
            background-color: #e9ecef;
            opacity: 0.6;
            pointer-events: none;
        }

        .modal.fade {
            background: fixed;
        }
    </style>

    <%--Scroll Bar GridView--%>
    <script type="text/javascript">

        //เรียกใช้งาน ทุกครั้งที่มีการ load หน้าใหม่
        $(window).load(function () {
            gridviewScroll();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(gridviewScroll);

            $(window).resize(function () {
                gridviewScroll();
            });
        });

        function gridviewScroll() {
            var gridWidth = $(window).width();
            var gridHeight = $(window).height();
            $('#<%=tana_report.ClientID%>').gridviewScroll({
                width: gridWidth - 150,
                height: gridHeight - 250,
                //arrowsize: 30,
            });
        }

    </script>


    <asp:ScriptManager runat="server" ID="scriptManager" EnablePageMethods="true"></asp:ScriptManager>
    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
        <ProgressTemplate>
            <div class="overlay">
                <div class="overlayContent">
                    <div style="border-width: 0px; position: fixed; font-size: 36px; margin: auto; text-align: center">
                        Loading...
                        <br />
                        <i class="fa fa-spinner fa-spin"></i>
                    </div>
                </div>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="grid-Body">
                <asp:Panel ID="Panel1" runat="server">
                    <div class=" container " style="width: 100%; padding-left: 0px; padding-right: 0px;">
                        <%-- Topic : CIP Confirm Contorl List --%>
                        <div class="form-inline" style="border-radius: 10px 10px 0px 0px; padding-left: 10px; background-color: #660000;">
                            <div class="form-group tdLabel">
                                <%--<asp:Image ID="jig_list" runat="server" ImageUrl="~/images/iconmaster.png" Width="64" Height="64" CssClass=" form-control-static auto-style1" />--%>
                                <asp:Label ID="lbl_list" runat="server"
                                    Font-Bold="true" Font-Size="X-Large"
                                    CssClass="form-control-static"
                                    ForeColor="#ffffff">
                                    <span class="glyphicon glyphicon-list-alt">
                                    </span>&nbsp;JIG TOOL & EQUIPMENT : TANAOROSHI</asp:Label>
                            </div>
                        </div>
                        <br />
                        <div class="form-inline">
                            <table style="width: 95%">
                                <tr>
                                    <td style="width: 100px"><b>Model : </b></td>
                                    <td>
                                        <asp:TextBox runat="server" ID="txt_model" Width="100px" disabled="true" CssClass="form-control"></asp:TextBox></td>
                                    <td style="width: 150px"><b>&nbsp;Check By : </b></td>
                                    <td>
                                        <asp:TextBox runat="server" ID="txt_checkBy" Width="150px" disabled="true" CssClass="form-control"></asp:TextBox></td>
                                    <td style="width: 180px"><b>&nbsp;Check Date : </b></td>
                                    <td>
                                        <asp:TextBox runat="server" ID="txt_check_date" Width="150px" disabled="true" CssClass="form-control"></asp:TextBox></td>
                                    <td style="width: 150px"><b>&nbsp;Responsible : </b></td>
                                    <td>
                                        <asp:TextBox runat="server" ID="txt_rsp" Width="150px" disabled="true" CssClass="form-control"></asp:TextBox></td>
                                    <td style="width: 80px"><b>&nbsp;Dept : </b></td>
                                    <td>
                                        <asp:TextBox runat="server" ID="txt_dept" Width="150px" disabled="true" CssClass="form-control"></asp:TextBox></td>
                                </tr>
                            </table>
                        </div>
                        <br />
                        <div class="form-inline">
                            <asp:LinkButton runat="server" ID="btn_cancel" ToolTip="Back" PostBackUrl="~/tool_equipment_tanaoroshi.aspx" CssClass="btn btn-danger" ><span class="glyphicon glyphicon-arrow-left"></span></asp:LinkButton>
                       &nbsp;<asp:ImageButton runat="server" ID="btn_excel" OnClick="btn_excel_Cilck" ImageUrl="~/images/excel.png" Width="3%" />
                            </div>
                        <br />

                        <asp:GridView runat="server" ID="tana_report" AutoGenerateColumns="false"
                            CssClass="tableGv" AllowSorting="true"
                            PageSize="20" PagerSettings-Mode="NumericFirstLast"
                            PagerSettings-FirstPageText="First" OnRowDataBound="tana_report_RowDataBound"
                            PagerSettings-LastPageText="Last"
                            PagerSettings-NextPageText="Next" Width="100%" 
                            PagerSettings-Visible="true"
                            PagerStyle-CssClass="pagegvMasterList"
                            EmptyDataText="No Data Found"
                            EmptyDataRowStyleH-orizontalAlign="Center"
                            GridLines="Both">
                            <EmptyDataRowStyle CssClass="danger text-danger" />
                            <HeaderStyle BackColor="darkseagreen" Wrap="false" HorizontalAlign="Center" Font-Size="Small" />
                            <RowStyle HorizontalAlign="Center" Font-Size="Small" />
                            <PagerSettings />
                            <Columns>
                                <asp:TemplateField HeaderText="No.">
                                    <ItemTemplate>
                                        <asp:HiddenField runat="server" ID="hd_id_his" Value='<%#Eval("id") %>' />
                                        <%# Container.DataItemIndex + 1 %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Picture" ItemStyle-Width="150px">
                                    <ItemTemplate>
                                        <asp:HiddenField runat="server" ID="hd_tool_id" Value='<%#Eval("tool_id") %>' />
                                        <asp:HiddenField runat="server" ID="hd_sys_status" Value='<%#Eval("sys_status") %>' />
                                        <asp:HiddenField runat="server" ID="hd_tool_sub_no" Value='<%#Eval("tool_sub_no") %>' />
                                        <asp:Image runat="server" ID="img_tool" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField HeaderText="Result" ItemStyle-Width="100px" DataField="check_name" />
                                <asp:BoundField HeaderText="Asset No" DataField="asset_no" ItemStyle-Width="150px" />
                                <asp:BoundField HeaderText="Seb Asset No." DataField="sub_asset_no" ItemStyle-Width="150px"/>
                                <asp:BoundField HeaderText="Control No" DataField="control_no" ItemStyle-Width="150px" />
                                <asp:BoundField HeaderText="Name" DataField="name" ItemStyle-Width="300px" />
                                <asp:BoundField HeaderText="Model" DataField="model_name" ItemStyle-Width="150px" />
                                <asp:BoundField HeaderText="Location" DataField="location_name" ItemStyle-Width="100px" />
                                <asp:BoundField HeaderText="Cell" DataField="cell_name" ItemStyle-Width="100px" />
                                <asp:BoundField HeaderText="Station" DataField="station_name" ItemStyle-Width="100px" />
                                <asp:BoundField HeaderText="Remark" DataField="remark" ItemStyle-Width="150px"/>

                            </Columns>
                        </asp:GridView>
                    </div>
                    <asp:GridView runat="server" ID="gv_test"></asp:GridView>
                    <asp:Label runat="server" ID="lb_test" ForeColor="Red"></asp:Label>
                </asp:Panel>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="btn_excel" />
        </Triggers>
    </asp:UpdatePanel>

</asp:Content>
