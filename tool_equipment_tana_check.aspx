<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="tool_equipment_tana_check.aspx.cs"
    Inherits="Tool_Equipment_Control_System.tool_equipment_tana_check" EnableEventValidation="false" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxtoolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">


    <%--<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css">--%>

    <link href="CSS/bootstrap.css" rel="stylesheet" />
    <%--<link href="CSS/bootstrap-multiselect.css" rel="stylesheet" />--%>
    <link href="CSS/jquery-ui.css" rel="stylesheet" />

    <link href="CSS/GridviewScroll.css" rel="stylesheet" />
    <link href="CSS/select2.min.css" rel="stylesheet" />
    <link href="DataTables-1.10.18/dataTables.dataTables.css" rel="stylesheet" />
    <script src="JS/jquery-1.9.1.js"></script>
    <%--<script src="JS/jquery-3.7.1.min.js"></script>--%>
    <script src="JS/jquery-ui.js"></script>

    <script src="JS/bootstrap.min.js"></script>
    <%--<script src="JS/jquery.min.js"></script>--%>
    <script src="Scripts/bootstrap.min.js"></script>
    <script src="Scripts/bootstrap-multiselect.js"></script>
    <script src="JS/gridviewScroll.min.js"></script>
    <script src="Scripts/select2.min.js"></script>
    <script src="DataTables-1.10.18/dataTables.min.js"></script>

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

        /*.modal {
            display: none;*/ /* Hidden by default */
        /*position: fixed;*/ /* Stay in place */
        /*z-index: 1;*/ /* Sit on top */
        /*left: 0;
            top: 0;
            width: 100%;*/ /* Full width */
        /*height: 100%;*/ /* Full height */
        /*overflow: auto;*/ /* Enable scroll if needed */
        /*background-color: rgb(0,0,0);*/ /* Fallback color */
        /*background-color: rgba(0,0,0,0.4);*/ /* Black w/ opacity */
        /*}

        .modal-content {
            background-color: #fefefe;
            margin: 15% auto;*/ /* 15% from the top and centered */
        /*padding: 20px;
            border: 1px solid #888;
            width: 80%;*/ /* Could be more or less, depending on screen size */
        /*}*/
    </style>

    <%--Scroll Bar GridView--%>
    <script type="text/javascript">

        //เรียกใช้งาน ทุกครั้งที่มีการ load หน้าใหม่
        $(window).load(function () {

            scanner();
            gridviewScroll();
            // Set_datePicker();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(gridviewScroll);
            // Sys.WebForms.PageRequestManager.getInstance().add_endRequest(Set_datePicker);

            $(window).resize(function () {
                gridviewScroll();
            });
        });

        function gridviewScroll() {
            var gridWidth = $(window).width();
            var gridHeight = $(window).height();
            $('#<%=gv_tana_check.ClientID%>').gridviewScroll({
                width: gridWidth - 150,
                height: gridHeight - 250,
                //arrowsize: 30,
            });
        }
        function scanner() {
            var scannerTextBox = document.getElementById('<%= ScannerTextBox.ClientID %>');

            // If the TextBox exists, set the focus on it
            if (scannerTextBox) {
                scannerTextBox.value = '';
                scannerTextBox.focus();
            }
        }

        function Set_datePicker() {
            $('.date').datetimepicker({ format: 'DD-MMM-YYYY' });
        }

    </script>

    <%-- Gridview button Colum Check--%>
    <script type="text/javascript">
        // ฟังก์ชันสำหรับเรียกใช้จากโค้ด C#
        function selectButtonFromCodeBehind(buttonID,status) {
           var button = document.getElementById(buttonID);
            if (button) {
                // เรียกใช้ฟังก์ชันหลักเพื่อจัดการการเลือก
                selectButton(button, status);
            }
        }
        function selectButtonFromFrontend(button,status) {
            selectButton(button, status);
            return false;
        }

        function selectButton(button, selectStatus) {
            // Find the parent fieldset of the clicked button
            var fieldset = button.closest('fieldset');
            if (!fieldset) {
                console.error("Could not find the parent fieldset.");
                return false;
            }

            // Get all labels in the same group
            var labelsInGroup = fieldset.querySelectorAll('.myButton');

            // Remove 'selected' class from all labels in the group
            for (var i = 0; i < labelsInGroup.length; i++) {
                labelsInGroup[i].classList.remove('selected');
            }

            // Add 'selected' class to the clicked label
            button.classList.add('selected');

            // Uncheck all radio buttons in the group first
            var radioButtons = fieldset.querySelectorAll('input[type="radio"]');
            for (var i = 0; i < radioButtons.length; i++) {
                radioButtons[i].checked = false;
            }

            // Find the radio button inside the clicked label and set it as checked
            var radioButton = button.querySelector('input[type="radio"]');
            if (radioButton) {
                radioButton.checked = true;
            } else {
                radioButton.checked = false;
            }


            const row = button.closest('tr');

            var ddl_model_new = row.querySelector('[id*="ddl_model_new"]');
            var ddl_rsp_new = row.querySelector('[id*="ddl_rsp_new"]');
            var ddl_location_new = row.querySelector('[id*="ddl_location_new"]');
            var ddl_cell_new = row.querySelector('[id*="ddl_cell_new"]');
            var ddl_station_new = row.querySelector('[id*="ddl_station_new"]');
            var txt_remark = row.querySelector('[id*="txt_remark"]');

            if (selectStatus == 'Location' || selectStatus == 'Not_Found') {
                ddl_model_new.className = "form-control";
                ddl_rsp_new.className = "form-control";
                ddl_location_new.className = "form-control";
                ddl_cell_new.className = "form-control";
                ddl_station_new.className = "form-control";
                txt_remark.className = "form-control";

            }
            else {
                ddl_model_new.className = "form-control disabled-field";
                ddl_rsp_new.className = "form-control disabled-field";
                ddl_location_new.className = "form-control disabled-field";
                ddl_cell_new.className = "form-control disabled-field";
                ddl_station_new.className = "form-control disabled-field";
                txt_remark.className = "form-control disabled-field";

            }

            scanner();
            return false;
        }


    </script>

    <%--Event Scanner--%>
    <script type="text/javascript">
        function searchAndHighlight(searchText) {
            var grid = document.getElementById('<%= gv_tana_check.ClientID %>');
            if (!grid) return;

            var scannerTextBox = document.getElementById('<%= ScannerTextBox.ClientID %>');
            var searchValue = searchText.trim().toLowerCase();

            // Get all rows in the GridView
            var rows = grid.getElementsByTagName('tr');
            var foundRows = []; // Change to an array to store all matching rows

            // If the search value is empty, remove all highlights and show all rows
            if (searchValue === '') {
                for (var i = 1; i < rows.length; i++) {
                    rows[i].style.backgroundColor = '';
                    rows[i].style.display = ''; // Show all rows
                }
                return; // Exit the function
            }

            // Loop through all data rows in the GridView (skip header row)
            for (var i = 1; i < rows.length; i++) {
                var row = rows[i];

                // Get the text from the search columns (adjust indices as needed)
                var assetNoCell = row.cells[3];
                var controlNoCell = row.cells[5];

                // Check if the scanner value matches any of the columns
                if (assetNoCell && (assetNoCell.innerText.trim().toLowerCase() === searchValue) ||
                    controlNoCell && (controlNoCell.innerText.trim().toLowerCase() === searchValue)) {

                    foundRows.push(row); // Add the matching row to the array
                }
            }

            // Remove highlight from all previous rows
            for (var i = 1; i < rows.length; i++) {
                rows[i].style.backgroundColor = '';
                //  rows[i].style.display = 'none'; // Initially hide all rows
            }

            // Highlight all found rows and scroll to the first one
            if (foundRows.length > 0) {
                foundRows.forEach(function (row) {
                    row.style.display = ''; // Show the found row
                    row.style.backgroundColor = '#d3f2d3'; // Apply highlight
                });
                // Scroll to the first found row
                foundRows[0].scrollIntoView({ behavior: 'smooth', block: 'center' });
            } else {
                // If no row is found, show all rows and show a message
                for (var i = 1; i < rows.length; i++) {
                    rows[i].style.display = ''; // Show all rows
                }
                alert('ไม่พบข้อมูล!!');
            }

            // This ensures the full barcode string is processed before clearing the textbox.
            // The delay helps prevent the next scan from appending to the current text.
            setTimeout(function () {
                scannerTextBox.value = '';
                scannerTextBox.focus(); // Set focus back to the textbox for the next scan
            }, 100);

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
                            <table style="width: 95%">
                                <tr>
                                    <td style="width: 100px"><b>Asset No.:</b></td>
                                    <td>
                                        <asp:TextBox runat="server" ID="txt_assetNo" Width="150px" CssClass="form-control"></asp:TextBox></td>
                                    <td style="width: 150px"><b>&nbsp;Control No.:</b></td>
                                    <td>
                                        <asp:TextBox runat="server" ID="txt_control" Width="150px" CssClass="form-control"></asp:TextBox></td>
                                    <td style="width: 100px"><b>&nbsp;Name :</b></td>
                                    <td>
                                        <asp:TextBox runat="server" ID="txt_name" Width="250px" CssClass="form-control"></asp:TextBox></td>

                                    <td style="width: 100px"><b>&nbsp;Station :</b></td>
                                    <td>
                                        <asp:DropDownList runat="server" ID="ddl_station" Width="150px"
                                            CssClass="form-control">
                                        </asp:DropDownList></td>
                                    <td style="width: 100px; text-align: center">
                                        <asp:LinkButton runat="server" ID="btn_excel" ToolTip="Excel File"><i class="glyphicon glyphicon-download-alt"></i></asp:LinkButton></td>

                                </tr>

                            </table>
                        </div>
                        <br />
                        <div class="form-inline">
                            <asp:TextBox ID="ScannerTextBox" runat="server" CssClass="form-control" onchange="searchAndHighlight(this.value);"></asp:TextBox>
                        </div>
                        <br />
                        <div class="form-inline" style="text-align: center">
                            <asp:LinkButton runat="server" ID="btn_save" OnClick="btn_save_Click" CssClass="btn btn-success" Text="Save"></asp:LinkButton>
                            <asp:LinkButton runat="server" ID="btn_cancel" PostBackUrl="~/tool_equipment_tanaoroshi.aspx" CssClass="btn btn-danger" Text="Cancel"></asp:LinkButton>
                        </div>
                        <br />
                        <div class="form-inline" style="margin-bottom: 20px;">
                            <asp:GridView runat="server" ID="gv_tana_check"
                                CssClass="tableGv" AllowSorting="true"
                                PageSize="20" PagerSettings-Mode="NumericFirstLast"
                                PagerSettings-FirstPageText="First" OnRowDataBound="gv_tana_check_RowDataBound"
                                PagerSettings-LastPageText="Last"
                                PagerSettings-NextPageText="Next" Width="100%"
                                PagerSettings-Visible="true"
                                PagerStyle-CssClass="pagegvMasterList"
                                EmptyDataText="No Data Found" AutoGenerateColumns="false"
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
                                    <asp:TemplateField HeaderText="Picture" ItemStyle-Width="100px" >
                                        <ItemTemplate>
                                            <asp:HiddenField runat="server" ID="hd_tool_id" Value='<%#Eval("tool_id") %>' />
                                            <asp:HiddenField runat="server" ID="hd_sys_status" Value='<%#Eval("sys_status") %>' />
                                            <asp:HiddenField runat="server" ID="hd_tool_sub_no" Value='<%#Eval("tool_sub_no") %>' />
                                            <asp:HiddenField runat="server" ID="hd_img_path" />
                                            <%--OnClientClick ="modal_pic(this)" --%>
                                           
                                            <asp:ImageButton runat="server" ID="img_tool" Width="100%" OnClientClick="modal_pic(this);return false ;"/>
                                            <%--<asp:LinkButton runat="server" ID="btn_pic" OnClientClick="modal_pic(this);return false ;"><i class="glyphicon glyphicon-picture"></i></asp:LinkButton>--%>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Check" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <asp:HiddenField runat="server" ID="hd_check_id" Value='<%#Eval("check_id") %>' />
                                            <%--<asp:Label runat="server" ID="test_check"></asp:Label>--%>
                                            <fieldset class="radio-group" style="display: flex; flex-direction: column; gap: 5px;">
                                                <label runat="server" id="lb_check" class="myButton" onclick="return selectButtonFromFrontend(this,'');">
                                                    <input type="radio" id="rb_check" runat="server" name='<%# "resultCheck_" + Container.DataItemIndex %>' />
                                                    Check
                                                </label>
                                                <label runat="server" id="lb_loca" class="myButton" onclick="return selectButton(this,'Location');" >
                                                    <input type="radio" id="rb_location" runat="server" name='<%# "resultCheck_" + Container.DataItemIndex %>' />
                                                    Location
                                                </label>
                                                <label runat="server" id="lb_not_found" class="myButton" onclick="return selectButton(this,'Not_Found');">
                                                    <input type="radio" id="rb_notFound" runat="server" name='<%# "resultCheck_" + Container.DataItemIndex %>' />
                                                    Not Found
                                                </label>
                                            </fieldset>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Asset No.">
                                        <ItemTemplate>
                                            <asp:Label runat="server" ID="lb_assetNo" Text='<%#Eval("asset_no") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Sub Asset No.">
                                        <ItemTemplate>
                                            <asp:Label runat="server" ID="lb_sub_asset" Text='<%#Eval("sub_asset_no") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Control No." ItemStyle-Width="150px">
                                        <ItemTemplate>
                                            <asp:Label runat="server" ID="lb_cotrolNo" Text='<%#Eval("control_no") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Name" ItemStyle-Width="200px">
                                        <ItemTemplate>
                                            <asp:Label runat="server" ID="lb_name" Text='<%#Eval("name") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Model" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <asp:HiddenField runat="server" ID="hd_model_id" Value='<%#Eval("model_id") %>' />
                                            <asp:Label runat="server" ID="lb_model" Text='<%#Eval("model_name") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Location" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <asp:HiddenField runat="server" ID="hd_location_id" Value='<%#Eval("location_id") %>' />
                                            <asp:Label runat="server" ID="lb_location" Text='<%#Eval("location_name") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Cell" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <asp:HiddenField runat="server" ID="hd_cell_id" Value='<%#Eval("cell_id") %>' />
                                            <asp:Label runat="server" ID="lb_cell" Text='<%#Eval("cell_name") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Station" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <asp:HiddenField runat="server" ID="hd_station_id" Value='<%#Eval("station_id") %>' />
                                            <asp:Label runat="server" ID="lb_station" Text='<%#Eval("station_name") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Responsible" ItemStyle-Width="150px">
                                        <ItemTemplate>
                                            <asp:HiddenField runat="server" ID="hd_rsp_new" Value='<%#Eval("pic_rsp_new") %>' />
                                            <asp:DropDownList runat="server" ID="ddl_rsp_new"
                                                CssClass="form-control disabled-field"
                                                DataSourceID="sql_rsp"
                                                DataValueField="emp_id"
                                                DataTextField="pic_dept" AppendDataBoundItems="true">
                                                <asp:ListItem Value="">-</asp:ListItem>
                                            </asp:DropDownList>
                                            <asp:SqlDataSource runat="server" ID="sql_rsp" ConnectionString="<%$ ConnectionStrings:tool_equipment %>"
                                                SelectCommand=" select * from Responsible_Dept "></asp:SqlDataSource>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Model" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <asp:HiddenField runat="server" ID="hd_model_new" Value='<%#Eval("model_new") %>' />
                                            <asp:DropDownList runat="server" ID="ddl_model_new"
                                                CssClass="form-control disabled-field"
                                                DataSourceID="sql_model" Width="100%"
                                                DataValueField="model_id"
                                                DataTextField="model_name"
                                                AppendDataBoundItems="true">
                                                <asp:ListItem Value="">-</asp:ListItem>
                                            </asp:DropDownList>
                                            <asp:SqlDataSource runat="server" ID="sql_model"
                                                ConnectionString="<%$ ConnectionStrings:jig_tool %>"
                                                SelectCommand=" select* from draw_master_model "></asp:SqlDataSource>

                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Location" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <asp:HiddenField runat="server" ID="hd_location_new" Value='<%#Eval("location_new") %>' />
                                            <asp:DropDownList runat="server" ID="ddl_location_new"
                                                CssClass="form-control disabled-field"
                                                DataSourceID="sql_location"
                                                DataValueField="location1_id" Width="100%" AppendDataBoundItems="true" onchange="updateCellDropdown(this)"
                                                DataTextField="location1_name">
                                                <asp:ListItem Value="">-</asp:ListItem>
                                            </asp:DropDownList>
                                            <asp:SqlDataSource runat="server" ID="sql_location"
                                                ConnectionString="<%$ ConnectionStrings:jig_tool %>"
                                                SelectCommand=" select* from master_location1 where is_use = 1 "></asp:SqlDataSource>

                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Cell" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <asp:HiddenField runat="server" ID="hd_cell_new" Value='<%#Eval("cell_new") %>' />
                                            <asp:DropDownList runat="server" ID="ddl_cell_new" Width="100%"
                                                CssClass="form-control disabled-field" onchange="chang_cell_new(this)">
                                            </asp:DropDownList>
                                            <asp:HiddenField runat="server" ID="hd_ddl_cell_new" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Station" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <asp:HiddenField runat="server" ID="hd_station_new" Value='<%#Eval("station_new") %>' />
                                            <asp:DropDownList runat="server" ID="ddl_station_new"
                                                Width="100%" CssClass="form-control disabled-field"
                                                onchange="chang_station_new(this)">
                                            </asp:DropDownList>
                                            <asp:HiddenField runat="server" ID="hd_ddl_station_new" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Remark" ItemStyle-Width="200px">
                                        <ItemTemplate>
                                            <asp:TextBox runat="server" ID="txt_remark" Text='<%#Eval("remark") %>' CssClass="form-control disabled-field"></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                </Columns>
                            </asp:GridView>

                        </div>
                        <asp:Label runat="server" ID="lb_test" ForeColor="Red"></asp:Label>

                        <script type="text/javascript">
                            function modal_pic(pic) {
                                const btn_pic = pic.id;

                                const row = pic.closest('tr');
                                const img_path = row.querySelector('[id*="hd_img_path"]')

                                //const img_Main = document.getElementById('imgMain');
                                const img_Main = document.getElementById('<%=imgMain.ClientID %>');

                                if (img_path.value) {
                                    img_Main.src = img_path.value;
                                    $('#modal_UploadPictureMain').modal('show');
                                }

                            }
                        </script>

                        <%-- Modal Show Pictrue--%>
                        <div class="modal fade" id="modal_UploadPictureMain" style="width: 1000px; margin-left: -500px" tabindex="-1" role="dialog" aria-hidden="true">
                            <div class="modal-dialog modal-sm4">
                                <div class="modal-content">
                                    <div class="modal-header theme-bg-color1 theme-text-color2">
                                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                                        <h4 class="modal-title">&nbsp;Pictrue</h4>
                                    </div>
                                    <div class="modal-body">
                                        <asp:Label runat="server" ID="lb_testPic"></asp:Label>
                                        <asp:Image runat="server" ID="imgMain" Width="80%" />
                                    </div>
                                </div>
                            </div>
                        </div>

                        <script type="text/javascript">


                            function chang_cell_new(ddl_cell_new) {
                                const cell_new_id = ddl_cell_new.value;
                                const row = ddl_cell_new.closest('tr');
                                const hd_ddl_cell_new = row.querySelector('[id*="hd_ddl_cell_new"]');
                                hd_ddl_cell_new.innerHTML = '';
                                if (cell_new_id != '') {
                                    hd_ddl_cell_new.value = cell_new_id;
                                } else {
                                    alert('ไม่มีข้อมูล');
                                }
                            }

                            function chang_station_new(ddl_station_new) {
                                const station_new_id = ddl_station_new.value;
                                const row = ddl_station_new.closest('tr');
                                const hd_ddl_station_new = row.querySelector('[id*="hd_ddl_station_new"]');
                                hd_ddl_station_new.innerHTML = '';
                                if (station_new_id != '') {
                                    hd_ddl_station_new.value = station_new_id;
                                    //  alert(station_new_id);
                                } else {
                                    alert('ไม่มีข้อมูล');
                                }
                            }

                            // ฟังก์ชัน JavaScript ที่จะถูกเรียกเมื่อค่า DropDownList ของ Location เปลี่ยน
                            function updateCellDropdown(locationDropdown) {
                                // ดึงค่า location1_id ที่ถูกเลือก
                                const selectedLocationId = locationDropdown.value;

                                // หาแถว (row) ที่ DropDownList อยู่
                                const row = locationDropdown.closest('tr');

                                // หา DropDownList ของ Cell ในแถวเดียวกันโดยใช้ id ที่ถูกสร้าง
                                const ddlCellNew = row.querySelector('[id*="ddl_cell_new"]');

                                // หา DropDownList ของ Station ในแถวเดียวกันโดยใช้ id ที่ถูกสร้าง
                                const ddlStationNew = row.querySelector('[id*="ddl_station_new"]');

                                ////เชื่อมต่อ API ddl_Cell
                                if (ddlCellNew) {
                                    // ทำการเคลียร์ option เดิมทั้งหมด
                                    ddlCellNew.innerHTML = '';

                                    // เพิ่ม option ค่าเริ่มต้น
                                    const defaultCellOption = document.createElement('option');
                                    defaultCellOption.value = '';
                                    defaultCellOption.text = '-';
                                    ddlCellNew.appendChild(defaultCellOption);

                                    // หากมีการเลือกค่าที่ถูกต้อง
                                    if (selectedLocationId) {
                                        // เปลี่ยน URL ให้ใช้ชื่อไฟล์ที่ถูกต้อง
                                        const url = 'tool_equipment_tana_check.aspx/GetCellsByLocation';

                                        // เรียก Web Method จากฝั่ง Server โดยใช้ fetch API
                                        fetch(url, {
                                            method: 'POST',
                                            headers: {
                                                'Content-Type': 'application/json',
                                                'Accept': 'application/json'
                                            },
                                            body: JSON.stringify({ locationId: selectedLocationId })
                                        })
                                            .then(response => {
                                                if (!response.ok) {
                                                    // เพิ่มการแสดงสถานะ Error ที่ชัดเจน
                                                    return response.text().then(text => {
                                                        throw new Error('Network response was not ok, status: ' + response.status + ' | ' + text);
                                                    });
                                                }
                                                return response.json();
                                            })
                                            .then(data => {
                                                // วนลูปเพื่อเพิ่ม option ใหม่จากข้อมูลที่ได้
                                                // ASP.NET Web Method จะห่อข้อมูลไว้ใน property 'd'
                                                if (data.d) {
                                                    data.d.forEach(cell => {
                                                        const option = document.createElement('option');
                                                        option.value = cell.location2_id;
                                                        option.text = cell.location2_name;
                                                        ddlCellNew.appendChild(option);
                                                    });
                                                }
                                            })
                                            .catch(error => {
                                                // แสดง error ใน console เพื่อการ debug ที่ง่ายขึ้น
                                                alert('Cell Error : .' + error);
                                            });
                                    }


                                }

                                ////เชื่อมต่อ API ddl_station

                                if (ddlStationNew) {
                                    ddlStationNew.innerHTML = '';

                                    // เพิ่ม option ค่าเริ่มต้น
                                    const defaultCellOptionstation = document.createElement('option');
                                    defaultCellOptionstation.value = '';
                                    defaultCellOptionstation.text = '-';
                                    ddlStationNew.appendChild(defaultCellOptionstation);


                                    if (selectedLocationId) {
                                        // เปลี่ยน URL ให้ใช้ชื่อไฟล์ที่ถูกต้อง
                                        const url2 = 'tool_equipment_tana_check.aspx/GetStation';

                                        // เรียก Web Method จากฝั่ง Server โดยใช้ fetch API
                                        fetch(url2, {
                                            method: 'POST',
                                            headers: {
                                                'Content-Type': 'application/json',
                                                'Accept': 'application/json'
                                            },
                                            body: JSON.stringify({ location: selectedLocationId })
                                        })
                                            .then(response => {
                                                if (!response.ok) {
                                                    // เพิ่มการแสดงสถานะ Error ที่ชัดเจน
                                                    return response.text().then(text => {
                                                        throw new Error('Network response was not ok, status: ' + response.status + ' | ' + text);
                                                    });
                                                }
                                                return response.json();
                                            })
                                            .then(data => {
                                                // วนลูปเพื่อเพิ่ม option ใหม่จากข้อมูลที่ได้
                                                // ASP.NET Web Method จะห่อข้อมูลไว้ใน property 'd'
                                                if (data.d) {
                                                    data.d.forEach(cell => {
                                                        const optionstation = document.createElement('option');
                                                        optionstation.value = cell.location3_id;
                                                        optionstation.text = cell.location3_name;
                                                        ddlStationNew.appendChild(optionstation);
                                                    });
                                                }
                                            })
                                            .catch(error => {
                                                // แสดง error ใน console เพื่อการ debug ที่ง่ายขึ้น
                                                console.error('Station Error : ', error);
                                                //  alert('Station Error : ' + error);
                                            });
                                    }
                                }

                            }


                        </script>
                    </div>
                </asp:Panel>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btn_save" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="gv_tana_check" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
