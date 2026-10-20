<%@ Page Title="" Language="C#" MasterPageFile="~/Tool_Equipment.Master"
    AutoEventWireup="true" CodeFile="Picture_Update.aspx.cs"
    Inherits="Tool_Equipment_Control_System.Picture_Update"
    EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <link href="CSS/bootstrap.css" rel="stylesheet" />
    <link href="CSS/font-awesome4.7.0.min.css" rel="stylesheet" />

    <script src="js/jquery-1.10.2.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>

    <style>
        /* ---- Layout ---- */
        .pu-wrap {
            margin-top: 30px;
            margin-left: 10px;
            margin-right: 10px;
        }
        .pu-header {
            background-color: #660000;
            color: #fff;
            padding: 7px 12px;
            border-radius: 4px 4px 0 0;
            font-weight: bold;
            font-size: 18px;
            display: flex;
            align-items: center;
            gap: 8px;
        }
        .btn-back {
            margin-left: auto;
            background: rgba(255,255,255,0.15);
            color: #fff;
            border: 1px solid rgba(255,255,255,0.4);
            border-radius: 4px;
            padding: 3px 12px;
            font-size: 13px;
            font-weight: normal;
            cursor: pointer;
            transition: background .15s;
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            gap: 5px;
        }
        .btn-back:hover {
            background: rgba(255,255,255,0.28);
            color: #fff;
            text-decoration: none;
        }
        .pu-card {
            background: #fff;
            border-radius: 0 0 4px 4px;
            border: 1px solid #ddd;
            border-top: none;
            padding: 12px;
        }
        /* ---- Tool Info Bar ---- */
        .info-bar {
            display: flex;
            flex-wrap: wrap;
            gap: 8px;
            padding: 8px 0 12px;
            border-bottom: 1px solid #eee;
            margin-bottom: 14px;
        }
        .info-field { flex: 0 0 auto; }
        .info-field label {
            font-size: 14px;
            color: #888;
            display: block;
            margin-bottom: 2px;
        }
        .info-field .form-control[disabled] {
            background: #f5f5f5;
            color: #555;
            font-weight: bold;
            text-align: center;
        }
        /* ---- Section ---- */
        .pic-section { margin-bottom: 14px; }
        .pic-section-title {
            font-weight: 700;
            font-size: 16px;
            color: #555;
            display: flex;
            align-items: center;
            gap: 6px;
            margin-bottom: 8px;
        }
        .pic-section-title .btn-add { padding: 2px 8px; font-size: 14px; }
        /* ---- Thumbnail Strip ---- */
        .thumb-strip {
            display: flex;
            flex-wrap: wrap;
            gap: 8px;
            min-height: 90px;
            align-items: flex-start;
        }
        .thumb-item {
            position: relative;
            width: 110px;
            height: 90px;
            border: 2px solid #ccc;
            border-radius: 4px;
            overflow: hidden;
            cursor: pointer;
            transition: border-color .15s;
            background: #f5f5f5;
        }
        .thumb-item:hover { border-color: #660000; }
        .thumb-item img {
            width: 100%;
            height: 100%;
            object-fit: cover;
        }
        .thumb-item .thumb-label {
            position: absolute;
            bottom: 0; left: 0; right: 0;
            background: rgba(0,0,0,.45);
            color: #fff;
            font-size: 10px;
            text-align: center;
            padding: 2px 0;
        }
        .thumb-item.thumb-empty img {
            object-fit: contain;
            padding: 10px;
            opacity: .4;
        }
        /* ── thumb clickable wrapper (แก้ Fix Asset bug) ── */
        .thumb-click {
            display: block;
            width: 100%;
            height: 100%;
            border: none;
            padding: 0;
            background: none;
            cursor: pointer;
        }
        .thumb-click img {
            width: 100%;
            height: 100%;
            object-fit: cover;
            display: block;
        }
        /* ---- Dropzone ---- */
        .dz-box {
            position: relative;
            border: 2px dashed #bbb;
            border-radius: 8px;
            padding: 22px 16px;
            text-align: center;
            color: #666;
            background: #fafafa;
            transition: border-color .15s, background .15s;
            cursor: pointer;
            user-select: none;
            min-height: 80px;
        }
        .dz-box.dragover {
            border-color: #337ab7;
            background: #edf5ff;
        }
        .dz-box .dz-msg { font-size: 13px; pointer-events: none; }
        .dz-box .dz-msg .fa { font-size: 22px; color: #bbb; display: block; margin-bottom: 4px; }
        .dz-box .dz-msg a { pointer-events: auto; color: #337ab7; }
        .dz-box input[type=file] {
            position: absolute;
            inset: 0;
            width: 100%; height: 100%;
            opacity: 0;
            cursor: pointer;
            z-index: 2;
        }
        /* ---- Preview Grid (Other modal multi) ---- */
        .preview-grid {
            display: flex;
            flex-wrap: wrap;
            gap: 8px;
            margin-top: 10px;
        }
        .preview-thumb {
            position: relative;
            width: 90px; height: 80px;
            border: 1px solid #ccc;
            border-radius: 4px;
            overflow: hidden;
        }
        .preview-thumb img { width: 100%; height: 100%; object-fit: cover; }
        .preview-thumb .remove-btn {
            position: absolute;
            top: 2px; right: 2px;
            background: rgba(180,0,0,.75);
            color: #fff;
            border: none;
            border-radius: 50%;
            width: 18px; height: 18px;
            font-size: 11px;
            line-height: 18px;
            text-align: center;
            cursor: pointer;
            padding: 0;
        }
        .preview-thumb .remove-btn:hover { background: #c00; }
        /* ---- Single Preview ---- */
        .single-preview { text-align: center; margin-top: 10px; }
        .single-preview img {
            max-width: 100%;
            max-height: 240px;
            border-radius: 4px;
            box-shadow: 0 1px 4px rgba(0,0,0,.15);
            display: none;
        }
        /* ---- Sub picker buttons ---- */
        .sub-btn-wrap {
            display: flex;
            flex-wrap: wrap;
            gap: 6px;
            margin-bottom: 12px;
        }
        .sub-btn {
            min-width: 56px;
            padding: 5px 10px;
            font-size: 12px;
            border: 1px solid #bbb;
            border-radius: 4px;
            background: #f5f5f5;
            cursor: pointer;
            transition: all .12s;
        }
        .sub-btn.active { background: #337ab7; color: #fff; border-color: #337ab7; }
        .sub-btn.has-file { border-color: #5cb85c; }
        .sub-btn.active.has-file { background: #5cb85c; border-color: #4cae4c; }
        /* ---- Modal : backdrop + center fix ---- */
        /*
            ลบ data-backdrop="static" และ data-keyboard="false" ออกจาก HTML
            แทนด้วย JS options ให้ควบคุมได้ครบ
        */
        /* Modal backdrop เป็นสีดำ 50% (Bootstrap default) — ไม่ต้องแก้ CSS */
        /* Modal ตรงกลางทุกขนาดจอ */
        .modal-dialog {
            margin: 30px auto;
        }
        @media (max-width: 767px) {
            .modal-dialog { margin: 10px auto; width: 95% !important; }
        }
        /* ---- Modal header colors ---- */
        .modal-header.hdr-dark {
            background: #660000;
            border-radius: 4px 4px 0 0;
        }
        .modal-header.hdr-dark .modal-title,
        .modal-header.hdr-dark h4 { color: #fff; }
        .modal-header.hdr-dark .close { color: #fff; opacity: .8; }
        .modal-header.hdr-dark .close:hover { opacity: 1; }
        /* ---- file-name badge ---- */
        .fname-badge {
            font-size: 11px;
            color: #555;
            background: #eee;
            padding: 2px 7px;
            border-radius: 10px;
            margin-top: 6px;
            display: inline-block;
        }
        /* ---- Other Gallery ---- */
        .other-gallery {
            display: flex;
            flex-wrap: wrap;
            gap: 10px;
            min-height: 60px;
        }
        .og-item {
            position: relative;
            width: 110px; flex: 0 0 110px;
            background: #f5f5f5;
            border: 1px solid #ddd;
            border-radius: 6px;
            overflow: hidden;
            box-shadow: 0 1px 3px rgba(0,0,0,.08);
            transition: box-shadow .15s;
        }
        .og-item:hover { box-shadow: 0 2px 8px rgba(0,0,0,.18); }
        .og-item img {
            width: 110px; height: 90px;
            object-fit: cover;
            display: block;
            cursor: zoom-in;
        }
        .og-name {
            font-size: 10px;
            color: #555;
            padding: 3px 22px 3px 5px;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            background: #fff;
            border-top: 1px solid #eee;
        }
        .og-del {
            position: absolute;
            top: 4px; right: 4px;
            background: rgba(180,0,0,.75);
            color: #fff;
            border: none;
            border-radius: 50%;
            width: 20px; height: 20px;
            font-size: 11px;
            line-height: 20px;
            text-align: center;
            cursor: pointer;
            padding: 0;
            display: flex;
            align-items: center;
            justify-content: center;
            text-decoration: none;
        }
        .og-del:hover { background: #c00; color: #fff; text-decoration: none; }
    </style>

    <asp:ScriptManager ID="ScriptManager1" EnablePageMethods="true" runat="server" />

    <%-- Loading overlay --%>
    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="up_main_panel">
        <ProgressTemplate>
            <div class="overlay">
                <div class="overlayContent">
                    <div style="position:fixed;font-size:36px;margin:auto;text-align:center;">
                        Loading...<br /><i class="fa fa-spinner fa-spin"></i>
                    </div>
                </div>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <%-- ====================================================
         MAIN PAGE
         ==================================================== --%>
    <asp:UpdatePanel ID="up_main_panel" runat="server">
        <ContentTemplate>
            <div class="pu-wrap">

                <%-- Header --%>
                <div class="pu-header">
                    <img src="images/add-image.png" style="height:18px;filter:invert(1);" alt="" />
                    Picture Update
                    <a href="javascript:history.back();" class="btn-back" style="align-items:baseline;">
                        <span class="glyphicon glyphicon-backward"></span> Back
                    </a>
                </div>

                <div class="pu-card">

                    <%-- Tool Info Bar --%>
                    <div class="info-bar">
                        <div class="info-field">
                            <label>ID</label>
                            <asp:TextBox runat="server" ID="txt_id" CssClass="form-control input-sm" Style="width:100px;" Enabled="false" />
                        </div>
                        <div class="info-field">
                            <label>Tool No / Model</label>
                            <asp:TextBox runat="server" ID="txt_toolno_model" CssClass="form-control input-sm" Style="width:240px;" Enabled="false" />
                        </div>
                        <div class="info-field">
                            <label>No.</label>
                            <asp:TextBox runat="server" ID="txt_no" CssClass="form-control input-sm" Style="width:80px;" Enabled="false" />
                        </div>
                        <div class="info-field">
                            <label>BFM No.</label>
                            <asp:TextBox runat="server" ID="txt_fixassetNo" CssClass="form-control input-sm" Style="width:160px;" Enabled="false" />
                        </div>
                        <div class="info-field">
                            <label>Sub Asset No.</label>
                            <asp:TextBox runat="server" ID="txt_subFix" CssClass="form-control input-sm" Style="width:100px;" Enabled="false" />
                        </div>
                    </div>

                    <%-- Section 1 : Picture Main --%>
                    <div class="pic-section">
                        <div class="pic-section-title">
                            <span style="color:#660000;">&#9632;</span> Picture (Main)
                            <asp:LinkButton runat="server" ID="btnOpenMain" CssClass="btn btn-primary btn-xs btn-add"
                                CausesValidation="false"
                                OnClientClick="openMainModal(); return false;">
                                <span class="glyphicon glyphicon-upload"> Upload
                            </asp:LinkButton>
                        </div>
                        <div class="thumb-strip" id="strip_main">
                            <asp:Repeater ID="rptImages" runat="server">
                                <ItemTemplate>
                                    <div class="thumb-item <%# (Container.DataItem.ToString().Contains("FilesType")) ? "thumb-empty" : "" %>">
                                        <%-- ใช้ <button> + <img> แทน ImageButton เพื่อหลีกเลี่ยง postback --%>
                                        <button type="button" class="thumb-click"
                                            onclick="openMainModal(); return false;">
                                            <img src='<%# Container.DataItem %>' alt="Main" />
                                        </button>
                                        <div class="thumb-label">Main</div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>

                    <hr style="margin:10px 0;" />

                    <%-- Section 2 : Picture Support Fix Asset --%>
                    <asp:Panel runat="server" ID="pnl_fixasset_section">
                        <div class="pic-section">
                            <div class="pic-section-title">
                                <span style="color:#337ab7;">&#9632;</span> Picture Support Fix Asset Control
                                <asp:LinkButton runat="server" ID="btnOpenFix" CssClass="btn btn-info btn-xs btn-add"
                                    CausesValidation="false"
                                    OnClientClick="openSubModal(); return false;">
                                    <span class="glyphicon glyphicon-upload"></span> Upload
                                </asp:LinkButton>
                            </div>
                            <div class="thumb-strip" id="strip_sub">
                                <asp:Repeater ID="rptSub" runat="server">
                                    <ItemTemplate>
                                        <div class="thumb-item <%# (Container.DataItem.ToString().Contains("FilesType")) ? "thumb-empty" : "" %>">
                                            <%--
                                                FIX BUG: เดิมใช้ ImageButton ซึ่ง ASP.NET render เป็น <input type="image">
                                                เมื่อคลิก จะทำ postback ก่อน return false จึงไม่ทัน
                                                แก้โดยใช้ <button type="button"> + <img> แทน → ไม่มี postback
                                            --%>
                                            <button type="button" class="thumb-click"
                                                onclick="openSubModal(<%# Container.ItemIndex + 1 %>); return false;">
                                                <img src='<%# Container.DataItem %>' alt='Sub <%# Container.ItemIndex + 1 %>' />
                                            </button>
                                            <div class="thumb-label">Sub <%# Container.ItemIndex + 1 %></div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>

                        <hr style="margin:10px 0;" />
                    </asp:Panel>

                    <%-- Section 3 : Picture Other --%>
                    <div class="pic-section">
                        <div class="pic-section-title">
                            <span style="color:#5cb85c;">&#9632;</span> Picture (Other)
                            <asp:LinkButton runat="server" ID="btn_pictureOther" CssClass="btn btn-success btn-xs btn-add"
                                OnClick="btn_NewPictureOther_Click">
                                <span class="glyphicon glyphicon-upload"> Upload
                            </asp:LinkButton>
                        </div>
                        <div class="thumb-strip" id="strip_other">
                            <asp:Repeater ID="rptOther" runat="server">
                                <ItemTemplate>
                                    <div class="thumb-item">
                                        <asp:ImageButton runat="server" ID="imgOtherBtn"
                                            ImageUrl='<%# Container.DataItem %>'
                                            OnClick="btn_pictureOther_Click"
                                            Style="width:100%;height:100%;object-fit:cover;" />
                                        <div class="thumb-label">Other</div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>

                    <asp:Label runat="server" ID="lb_test" />
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>


    <%-- ====================================================
         MODAL 1 : PICTURE MAIN
         ==================================================== --%>
    <%--
        FIX 1.1-1.4:
        - ลบ data-backdrop="static" → ให้ JS จัดการ (backdrop=true = มี overlay สีดำ, ปิดได้)
        - ลบ data-keyboard="false"  → กด ESC ได้
    --%>
    <div class="modal fade" id="modal_UploadPictureMain" role="dialog" style="background: none; border: 0; box-shadow: none; width:620px; padding-left:0px !important;">
        <asp:UpdatePanel runat="server" ID="up_main">
            <ContentTemplate>
                <%-- modal-dialog อยู่ในนี้ → Bootstrap จะ center ให้อัตโนมัติ --%>
                <div class="modal-dialog modal-md">
                    <div class="modal-content">

                        <div class="modal-header hdr-dark">
                            <button type="button" class="close" data-dismiss="modal">&times;</button>
                            <h4 class="modal-title">
                                <i class="fa fa-image"></i>&nbsp; Update Picture Main
                            </h4>
                        </div>

                        <div class="modal-body">
                            <p class="text-muted" style="font-size:12px;margin-bottom:10px;">
                                รูปภาพหลักโดยรวมของ Machine &nbsp;|&nbsp;
                                รองรับ <strong>.jpg</strong>, <strong>.png</strong> (อัปโหลดได้ 1 รูป)
                            </p>

                            <div class="dz-box" id="dz_main">
                                <div class="dz-msg">
                                    <i class="fa fa-cloud-upload"></i>
                                    ลากไฟล์มาวางที่นี่ หรือ <a href="javascript:void(0)">เลือกไฟล์</a>
                                </div>
                                <asp:FileUpload ID="UploadImg_main" runat="server"
                                    ClientIDMode="Static"
                                    accept=".jpg,.jpeg,.png"
                                    CssClass="file-input-overlay js-file" />
                            </div>

                            <div id="fname_main"></div>
                            <div class="single-preview">
                                <img id="prev_main" alt="Preview" />
                            </div>
                        </div>

                        <div class="modal-footer">
                            <asp:Button runat="server" ID="btn_save_Main" CssClass="btn btn-primary"
                                Text="บันทึก" OnClick="btn_save_Main_Click" />
                            <button type="button" class="btn btn-default" data-dismiss="modal">ยกเลิก</button>
                        </div>

                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:PostBackTrigger ControlID="btn_save_Main" />
            </Triggers>
        </asp:UpdatePanel>
    </div>


    <%-- ====================================================
         MODAL 2 : PICTURE SUB FIX ASSET
         ==================================================== --%>
    <div class="modal fade" id="modal_UploadPictureSub" role="dialog" style="background: none; border: 0; box-shadow: none; width:620px; padding-left:0px !important;">
        <asp:UpdatePanel runat="server" ID="up_SubPicker">
            <ContentTemplate>

                <asp:HiddenField runat="server" ID="hid_selectedSub" ClientIDMode="Static" />

                <div class="modal-dialog modal-md">
                    <div class="modal-content">

                        <div class="modal-header hdr-dark" style="background:#1a4f8a;">
                            <button type="button" class="close" data-dismiss="modal">&times;</button>
                            <h4 class="modal-title">
                                <i class="fa fa-wrench"></i>&nbsp; Update Picture Support Fix Asset
                            </h4>
                        </div>

                        <div class="modal-body">
                            <p class="text-muted" style="font-size:12px;margin-bottom:10px;">
                                อัปโหลดรูปภาพตาม Sub Asset No. &nbsp;|&nbsp;
                                รองรับ <strong>.jpg</strong>, <strong>.png</strong> (ครั้งละ 1 รูปต่อ Sub)
                            </p>

                            <div style="margin-bottom:8px;font-size:12px;color:#555;">เลือก Sub ที่ต้องการอัปโหลด:</div>
                            <div class="sub-btn-wrap" id="sub_btn_wrap"></div>

                            <div class="dz-box" id="dz_sub">
                                <div class="dz-msg">
                                    <i class="fa fa-cloud-upload"></i>
                                    ลากไฟล์มาวางที่นี่ หรือ <a href="javascript:void(0)">เลือกไฟล์</a>
                                </div>
                                <asp:FileUpload ID="UploadImagesSubNew" runat="server"
                                    ClientIDMode="Static"
                                    accept=".jpg,.jpeg,.png" />
                            </div>

                            <div id="fname_sub"></div>
                            <div class="single-preview">
                                <img id="prev_sub" alt="Preview" />
                            </div>
                        </div>

                        <div class="modal-footer">
                            <asp:Button runat="server" ID="btn_save_Sub" ClientIDMode="Static"
                                CssClass="btn btn-primary"
                                Text="บันทึก" OnClick="btn_save_Sub_Click" />
                            <button type="button" class="btn btn-default" data-dismiss="modal">ยกเลิก</button>
                        </div>

                    </div>
                </div>

            </ContentTemplate>
            <Triggers>
                <asp:PostBackTrigger ControlID="btn_save_Sub" />
            </Triggers>
        </asp:UpdatePanel>
    </div>


    <%-- ====================================================
         MODAL 3 : PICTURE OTHER
         ==================================================== --%>
    <div class="modal fade" id="modal_UploadPictureOther" role="dialog" style="background: none; border: 0; box-shadow: none; width: 920px; left: 35%; padding-left:0px !important;" >
        <asp:UpdatePanel runat="server" ID="up_other">
            <ContentTemplate>
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">

                        <div class="modal-header hdr-dark" style="background:#3d6b35;">
                            <button type="button" class="close" data-dismiss="modal">&times;</button>
                            <h4 class="modal-title">
                                <i class="fa fa-picture-o"></i>&nbsp; Picture Other
                            </h4>
                        </div>

                        <div class="modal-body" style="padding:14px;">

                            <div style="margin-bottom:12px;">
                                <div style="font-size:12px;font-weight:bold;color:#3d6b35;margin-bottom:8px;">
                                    <i class="fa fa-folder-open-o"></i> รูปภาพในโฟลเดอร์
                                    <asp:Label runat="server" ID="lb_other_count"
                                        CssClass="badge" Style="background:#3d6b35;margin-left:6px;" />
                                </div>

                                <asp:Panel runat="server" ID="pnl_no_other" Visible="false">
                                    <div style="color:#aaa;font-size:12px;padding:12px 0;">
                                        <i class="fa fa-info-circle"></i> ยังไม่มีรูปภาพในโฟลเดอร์
                                    </div>
                                </asp:Panel>

                                <div class="other-gallery" id="other_gallery">
                                    <asp:Repeater runat="server" ID="rptOtherModal">
                                        <ItemTemplate>
                                            <div class="og-item" id='<%# "og_" + Container.ItemIndex %>'>
                                                <img src='<%# Container.DataItem %>' alt=""
                                                    onclick="ogExpand(this.src)"
                                                    title="คลิกขยาย" />
                                                <div class="og-name"
                                                    title='<%# System.IO.Path.GetFileName(Container.DataItem.ToString().Split(new char[]{'?'})[0]) %>'>
                                                    <%# System.IO.Path.GetFileName(Container.DataItem.ToString().Split(new char[]{'?'})[0]) %>
                                                </div>
                                                <asp:LinkButton runat="server" ID="btnDelOther"
                                                    CssClass="og-del"
                                                    CommandArgument='<%# Container.DataItem %>'
                                                    OnClick="btnDelOtherItem_Click"
                                                    OnClientClick="return confirm('ลบรูปนี้?');">
                                                    <i class="fa fa-trash"></i>
                                                </asp:LinkButton>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </div>

                            <hr style="margin:10px 0;" />

                            <div style="font-size:12px;font-weight:bold;color:#555;margin-bottom:8px;">
                                <i class="fa fa-cloud-upload"></i> อัปโหลดรูปใหม่
                                <span style="font-weight:normal;color:#aaa;">(รองรับ .jpg, .png — เลือกได้หลายรูป)</span>
                            </div>

                            <div class="dz-box" id="dz_other">
                                <div class="dz-msg">
                                    <i class="fa fa-cloud-upload"></i>
                                    ลากไฟล์มาวางที่นี่ หรือ <a href="javascript:void(0)">เลือกไฟล์</a>
                                </div>
                                <asp:FileUpload ID="FileUploadOther" runat="server"
                                    ClientIDMode="Static"
                                    accept=".jpg,.jpeg,.png"
                                    AllowMultiple="true" />
                            </div>

                            <div class="preview-grid" id="prev_other_grid"></div>

                            <%-- Lightbox overlay — z-index สูงกว่า modal (1050) --%>
                            <div id="og_lightbox" onclick="this.style.display='none'" style="
                                display:none;position:fixed;inset:0;background:rgba(0,0,0,.82);
                                z-index:9999;cursor:zoom-out;align-items:center;justify-content:center;">
                                <img id="og_lightbox_img" style="max-width:90vw;max-height:88vh;
                                    border-radius:6px;box-shadow:0 4px 24px rgba(0,0,0,.5);" />
                            </div>

                            <asp:Label runat="server" ID="lb_test1" />
                        </div>

                        <div class="modal-footer">
                            <asp:Button runat="server" ID="btn_save_other" CssClass="btn btn-success"
                                Text="อัปโหลด" OnClick="btn_save_other_Click" />
                            <button type="button" class="btn btn-default" data-dismiss="modal">ปิด</button>
                        </div>

                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:PostBackTrigger ControlID="btn_save_other" />
            </Triggers>
        </asp:UpdatePanel>
    </div>


    <%-- ====================================================
         JAVASCRIPT
         ==================================================== --%>
    <script type="text/javascript">

        /* ═══════════════════════════════════════════════════
           MODAL INIT — เรียกด้วย JS options แทน data-attribute
           backdrop : true  = มี overlay สีดำ (ปิดได้เมื่อคลิก backdrop)
           keyboard : true  = กด ESC ปิด Modal ได้
        ═══════════════════════════════════════════════════ */
        function initModals() {
            $('#modal_UploadPictureMain').modal({ backdrop: true, keyboard: true, show: false });
            $('#modal_UploadPictureSub').modal({ backdrop: true, keyboard: true, show: false });
            $('#modal_UploadPictureOther').modal({ backdrop: true, keyboard: true, show: false });
        }

        /* ─────────────────────────────────────────────────────
           HELPER : wire a dropzone box to a file input
        ───────────────────────────────────────────────────── */
        function wireDropzone(box, input, multi, onFiles) {
            if (!box || !input) return;
            ['dragover', 'drop'].forEach(function (ev) {
                document.addEventListener(ev, function (e) {
                    if (e.target !== input) e.preventDefault();
                }, false);
            });
            box.addEventListener('dragover', function (e) {
                e.preventDefault(); e.stopPropagation();
                box.classList.add('dragover');
            });
            box.addEventListener('dragleave', function (e) {
                e.preventDefault(); e.stopPropagation();
                box.classList.remove('dragover');
            });
            box.addEventListener('drop', function (e) {
                e.preventDefault(); e.stopPropagation();
                box.classList.remove('dragover');
                if (e.dataTransfer && e.dataTransfer.files.length) {
                    try { input.files = e.dataTransfer.files; } catch (ex) { }
                    onFiles(e.dataTransfer.files);
                }
            });
            input.addEventListener('change', function () {
                if (this.files && this.files.length) onFiles(this.files);
            });
        }

        /* ─── HELPER : show single preview ─── */
        function showSinglePreview(file, imgEl, nameEl) {
            if (!file) return;
            var ok = /image\/(jpeg|png)/.test(file.type) || /\.(jpg|jpeg|png)$/i.test(file.name);
            if (!ok) { alert('รองรับเฉพาะไฟล์ .jpg หรือ .png'); return; }
            if (nameEl) nameEl.innerHTML = '<span class="fname-badge"><i class="fa fa-file-image-o"></i> ' + file.name + '</span>';
            if (imgEl) {
                var r = new FileReader();
                r.onload = function (ev) { imgEl.src = ev.target.result; imgEl.style.display = 'block'; };
                r.readAsDataURL(file);
            }
        }

        /* ─── HELPER : show multi preview grid ─── */
        function showMultiPreview(files, gridEl) {
            if (!gridEl) return;
            Array.prototype.forEach.call(files, function (file) {
                var ok = /image\/(jpeg|png)/.test(file.type) || /\.(jpg|jpeg|png)$/i.test(file.name);
                if (!ok) return;
                var wrap = document.createElement('div');
                wrap.className = 'preview-thumb';
                var img = document.createElement('img');
                var r = new FileReader();
                r.onload = function (ev) { img.src = ev.target.result; };
                r.readAsDataURL(file);
                wrap.appendChild(img);
                gridEl.appendChild(wrap);
            });
        }

        /* ════════════════════════════════════════
           MODAL 1 – PICTURE MAIN
        ════════════════════════════════════════ */
        function openMainModal() {
            $('#modal_UploadPictureMain').modal('show');
        }
        function initMainModal() {
            var box = document.getElementById('dz_main');
            var input = document.getElementById('UploadImg_main');
            var prev = document.getElementById('prev_main');
            var fname = document.getElementById('fname_main');
            if (!box || !input) return;
            wireDropzone(box, input, false, function (files) {
                showSinglePreview(files[0], prev, fname);
            });
        }

        /* ════════════════════════════════════════
           MODAL 2 – PICTURE SUB FIX
        ════════════════════════════════════════ */
        var _subPendingFiles = {};
        var _activeSub = 1;

        function openSubModal(subNo) {
            _activeSub = subNo || 1;
            $('#modal_UploadPictureSub').modal('show');
        }
        function initSubModal() {
            var box = document.getElementById('dz_sub');
            var input = document.getElementById('UploadImagesSubNew');
            var prev = document.getElementById('prev_sub');
            var fname = document.getElementById('fname_sub');
            var hidSub = document.getElementById('hid_selectedSub');
            var btnWrap = document.getElementById('sub_btn_wrap');

            var txtSubEl = document.getElementById('<%=txt_subFix.ClientID%>');
            var maxSub = txtSubEl ? (parseInt(txtSubEl.value) || 0) : 0;

            if (!box || !input || !btnWrap) return;

            btnWrap.innerHTML = '';
            for (var i = 1; i <= maxSub; i++) {
                (function (n) {
                    var btn = document.createElement('button');
                    btn.type = 'button';
                    btn.className = 'sub-btn'
                        + (n === _activeSub ? ' active' : '')
                        + (_subPendingFiles[n] ? ' has-file' : '');
                    btn.textContent = 'Sub ' + n;
                    btn.id = 'sub_btn_' + n;
                    btn.addEventListener('click', function () { selectSub(n); });
                    btnWrap.appendChild(btn);
                })(i);
            }

            if (hidSub) hidSub.value = _activeSub;
            renderSubPreview();

            wireDropzone(box, input, false, function (files) {
                var file = files[0];
                var ok = /image\/(jpeg|png)/.test(file.type) || /\.(jpg|jpeg|png)$/i.test(file.name);
                if (!ok) { alert('รองรับเฉพาะไฟล์ .jpg หรือ .png'); return; }
                _subPendingFiles[_activeSub] = file;
                var btn = document.getElementById('sub_btn_' + _activeSub);
                if (btn) btn.classList.add('has-file');
                showSinglePreview(file, prev, fname);
            });
        }
        function selectSub(n) {
            var old = document.getElementById('sub_btn_' + _activeSub);
            if (old) old.classList.remove('active');
            _activeSub = n;
            var hidSub = document.getElementById('hid_selectedSub');
            if (hidSub) hidSub.value = n;
            var cur = document.getElementById('sub_btn_' + n);
            if (cur) cur.classList.add('active');
            renderSubPreview();
        }
        function renderSubPreview() {
            var prev = document.getElementById('prev_sub');
            var fname = document.getElementById('fname_sub');
            var file = _subPendingFiles[_activeSub];
            if (prev) { prev.style.display = 'none'; prev.src = ''; }
            if (fname) fname.innerHTML = '';
            if (file) showSinglePreview(file, prev, fname);
        }

        /* ════════════════════════════════════════
           MODAL 3 – PICTURE OTHER
        ════════════════════════════════════════ */
        function initOtherModal() {
            var box = document.getElementById('dz_other');
            var input = document.getElementById('FileUploadOther');
            var grid = document.getElementById('prev_other_grid');
            if (!box || !input) return;
            wireDropzone(box, input, true, function (files) {
                if (grid) grid.innerHTML = '';
                showMultiPreview(files, grid);
            });
        }

        /* ─── Bootstrap Modal events ─── */
        $(document).on('shown.bs.modal', '#modal_UploadPictureMain', function () {
            var p = document.getElementById('prev_main');
            var f = document.getElementById('fname_main');
            if (p) { p.src = ''; p.style.display = 'none'; }
            if (f) f.innerHTML = '';
            initMainModal();
        });
        $(document).on('shown.bs.modal', '#modal_UploadPictureSub', function () {
            initSubModal();
        });
        $(document).on('shown.bs.modal', '#modal_UploadPictureOther', function () {
            var g = document.getElementById('prev_other_grid');
            if (g) g.innerHTML = '';
            initOtherModal();
        });

        /* ─── Re-init after partial postback ─── */
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                initModals();
                initMainModal();
                initSubModal();
                initOtherModal();
            });
        }

        /* ─── Lightbox for Other gallery ─── */
        function ogExpand(src) {
            var lb = document.getElementById('og_lightbox');
            var img = document.getElementById('og_lightbox_img');
            if (!lb || !img) return;
            img.src = src;
            lb.style.display = 'flex';
        }

        /* ─── First load ─── */
        document.addEventListener('DOMContentLoaded', function () {
            initModals();
            initMainModal();
            initSubModal();
            initOtherModal();
        });

    </script>

</asp:Content>