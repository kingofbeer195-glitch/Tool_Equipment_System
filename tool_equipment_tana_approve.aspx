<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master" AutoEventWireup="true"
    CodeFile="tool_equipment_tana_approve.aspx.cs" Inherits="Tool_Equipment_Control_System.tool_equipment_tana_approve" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">




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

                        <asp:GridView runat="server" ID="tana_approve" AutoGenerateColumns="false"
                            CssClass="tableGv" AllowSorting="true"
                            PageSize="20" PagerSettings-Mode="NumericFirstLast"
                            PagerSettings-FirstPageText="First"
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
                                    <asp:TemplateField HeaderText="Picture">
                                        <ItemTemplate>
                                            <asp:HiddenField runat="server" ID="hd_tool_id" Value='<%#Eval("tool_id") %>' />
                                            <asp:HiddenField runat="server" ID="hd_sys_status" Value='<%#Eval("sys_status") %>' />
                                            <asp:HiddenField runat="server" ID="hd_tool_sub_no" Value='<%#Eval("tool_sub_no") %>' />
                                            <asp:HiddenField runat="server" ID="hd_img_path" />
                                            <%--OnClientClick ="modal_pic(this)" --%>
                                            <asp:LinkButton runat="server" ID="btn_pic" OnClientClick="modal_pic(this);return false ;"><i class="glyphicon glyphicon-picture"></i></asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                               <asp:BoundField HeaderText="Result" DataField="" />
                               <asp:BoundField HeaderText="Asset No" DataField="" />
                               <asp:BoundField HeaderText="Seb Asset No." DataField="" />
                               <asp:BoundField HeaderText="Control No" DataField="" />
                               <asp:BoundField HeaderText="Name" DataField="" />
                               <asp:BoundField HeaderText="Model" DataField="" />
                               <asp:BoundField HeaderText="Location" DataField="" />
                               <asp:BoundField HeaderText="Cell" DataField="" />
                               <asp:BoundField HeaderText="Station" DataField="" />
                               <asp:BoundField HeaderText="Remark" DataField="" />

                            </Columns>
                        </asp:GridView>
                    </div>
                </asp:Panel>

            </div>

        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
