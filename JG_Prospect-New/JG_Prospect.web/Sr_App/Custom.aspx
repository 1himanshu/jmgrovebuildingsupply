﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Sr_App/SR_app.Master" AutoEventWireup="true"
    CodeBehind="Custom.aspx.cs" Inherits="JG_Prospect.Sr_App.Product_Line.Custom" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../css/dropzone/css/basic.css" rel="stylesheet" />
    <link href="../css/dropzone/css/dropzone.css" rel="stylesheet" />
    <script type="text/javascript" src="http://code.jquery.com/jquery-1.8.2.js"></script>
    <script src="../js/jquery-latest.js" type="text/javascript"></script>
    <script type="text/javascript" src="../../Scripts/jquery.MultiFile.js"></script>
    <script type="text/javascript" src="../js/dropzone.js"></script>

    <script type="text/javascript">
        function uploadComplete() {

            if (Checkfiles() == true) {
                var btnImageUploadClick = document.getElementById("ctl00_ContentPlaceHolder1_btnImageUploadClick");
                btnImageUploadClick.click();
            }
        }
        function uploadComplete2() {


            var btnImageUploadClick = document.getElementById("ctl00_ContentPlaceHolder1_btnImageUploadClick");
            btnImageUploadClick.click();

        }
        function Checkfiles() {

            var fup = document.getElementById('ctl00_ContentPlaceHolder1_ajaxFileUpload_ctl02');
            var fileName = fup.value;
            var ext = fileName.substring(fileName.lastIndexOf('.') + 1).toString().toLowerCase();
            if (ext == "gif" || ext == "jpeg" || ext == "jpg" || ext == "tiff" || ext == "tif" || ext == "bmp" || ext == "png") {
                if ($('#ContentPlaceHolder1_hidCount').val().length != 0) {
                    if (parseInt($('#ContentPlaceHolder1_hidCount').val()) == 5) {
                        alert('You can not upload more than five image.');
                        $('#ctl00_ContentPlaceHolder1_ajaxFileUpload_ctl02').val('');
                        fup.focus();
                        return false;
                    }
                }
                return true;
            }
            else {
                alert('Upload Gif,JPG,JPEG,TIFF,TIF,PNG and BMP images only');
                $('#ctl00_ContentPlaceHolder1_ajaxFileUpload_ctl02').val('');
                fup.focus();
                return false;
            }
        }
        function readURL(input) {
            debugger;
            var fileExtension = ['jpeg', 'jpg', 'png', 'gif', 'bmp'];
            if ($.inArray($(input).val().split('.').pop().toLowerCase(), fileExtension) == -1) {
                alert("Invalid formats are not allowed.");
                $(input).val("");
                return false;
            }
        }

        $(document).ready(function () {
            $('#ContentPlaceHolder1_txtworkarea').focus();
        });



        function ValidateImage() {
            var count = $('#<%=hidCount.ClientID %>').val();
            //alert(count);
            if (count < 2) {
                alert('Upload atleast two image.');
                return false;
            }
        }

        function IsExists(pagePath, dataString, textboxid, errorlableid) {
            $.ajax({
                type: "POST",
                url: pagePath,
                data: dataString,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                error:
          function (XMLHttpRequest, textStatus, errorThrown) {
              $(errorlableid).show();
              $(errorlableid).html("Error");
          },
                success:
          function (result) {
              if (result != null) {
                  var flg = (result.d);

                  if (flg == "True") {
                      $(errorlableid).show();
                      $(errorlableid).html('Verified');
                      document.getElementById('<%=txtProposalCost.ClientID %>').value = document.getElementById('<%=txtAmount.ClientID %>').value;
                      $('#mask').hide();
                      $('#<%=pnlpopup.ClientID %>').hide();
                  }
                  else {
                      $(errorlableid).show();
                      $(errorlableid).html('failure');
                  }
              }
          }
            });
  }

  function focuslost() {
      if (document.getElementById('<%= txtAmount.ClientID%>').value == '') {
          alert('Please enter proposal cost!');
          return false;
      }
      else if (document.getElementById('<%= txtauthpass.ClientID%>').value == '') {
          alert('Please enter admin code!');
          return false;
      }
      else {
          var pagePath = "Custom.aspx/Exists";
          var dataString = "{ 'value':'" + document.getElementById('<%= txtauthpass.ClientID%>').value + "' }";
          var textboxid = "#<%= txtauthpass.ClientID%>";
          var errorlableid = "#<%= lblError.ClientID%>";

          IsExists(pagePath, dataString, textboxid, errorlableid);
          return true;
      }
}
function ShowPopup() {


    $('#ContentPlaceHolder1_txtProposalCost').attr('readonly', 'readonly');
    $('#ContentPlaceHolder1_txtAmount').focus();
    if (document.getElementById('<%=txtProposalCost.ClientID %>').value != '') {
        document.getElementById('<%=txtAmount.ClientID %>').value = document.getElementById('<%=txtProposalCost.ClientID %>').value;
    }
    $('#mask').show();
    $('#<%=pnlpopup.ClientID %>').show();
}
function HidePopup() {

    $('#ContentPlaceHolder1_txtAmount, #ContentPlaceHolder1_txtauthpass').val('');
    $('#ContentPlaceHolder1_lblError').text('');

    $('#mask').hide();
    $('#<%=pnlpopup.ClientID %>').hide();
}
$(".btnClose").live('click', function () {


    $('#<%=txtAmount.ClientID %>, #<%=txtauthpass.ClientID %>, #<%=lblError.ClientID %>').val('');

    HidePopup();
});

//Technical Interview : Yogesh Keraliya
//Code for drag and drop image upload.
//File Upload response from the server
Dropzone.options.dropzoneForm = {
    maxFiles: 5,
    url: "dropzoneimageupload.aspx",
    thumbnailWidth: 100,
    thumbnailHeight: 100,
    init: function () {
        this.on("maxfilesexceeded", function (data) {
            //var res = eval('(' + data.xhr.responseText + ')');
            alert('you are reached maximum pictures limit.');
        });

        // when file is uploaded successfully store its corresponding server side file name to preview element to remove later from server.
        this.on("success", function (file, response) {
            var filename = response.split("^");
            $(file.previewTemplate).append('<span class="server_file">' + filename[0] + '</span>');

            AddLocationPictoViewState(filename[0]);

        });

        //when file is removed from dropzone element, remove its corresponding server side file.
        this.on("removedfile", function (file) {
            var server_file = $(file.previewTemplate).children('.server_file').text();
            RemoveLocPicFromServer(server_file);
        });

        // When is added to dropzone element, add its remove link.
        this.on("addedfile", function (file) {

            // Create the remove button
            var removeButton = Dropzone.createElement("<a><small>Remove file</smalll></a>");

            // Capture the Dropzone instance as closure.
            var _this = this;

            // Listen to the click event
            removeButton.addEventListener("click", function (e) {
                // Make sure the button click doesn't submit the form:
                e.preventDefault();
                e.stopPropagation();
                // Remove the file preview.
                _this.removeFile(file);
            });

            // Add the button to the file preview element.
            file.previewElement.appendChild(removeButton);
        });
    }
};

function RemoveLocPicFromServer(filename) {

    var param = { serverfilename: filename };

    $.ajax({
        type: "POST",
        data: JSON.stringify(param),
        url: "dropzoneimageupload.aspx/RemoveLocationPic",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnLocationPicRemoveSuccess,
        error: OnLocationPicRemoveError
    });

}

function OnLocationPicRemoveSuccess(data) {
    var result = data.d;
    if (result) {

        RemoveLocationPicFromViewState(result);
    }

}
function OnLocationPicRemoveError() {

}

//Add uploaded location picture to viewstate of page to save later.
function AddLocationPictoViewState(serverfilename) {

    var images;

    if ($('#<%= locimages.ClientID %>').val()) {
        images = $('#<%= locimages.ClientID %>').val() + serverfilename + "^";
    }
    else {
        images = serverfilename + "^";
    }
   
    $('#<%= locimages.ClientID %>').val(images);
    console.log($('#<%= locimages.ClientID %>').val());
}
function RemoveLocationPicFromViewState(filename) {
    console.log($('#<%= locimages.ClientID %>').val());
    if ($('#<%= locimages.ClientID %>').val()) {

        //split images added by ^ seperator
        var images = $('#<%= locimages.ClientID %>').val().split("^");

        console.log(images);

        if (images.length > 0) {
            //find index of filename and remove it.
            var index = images.indexOf(filename);

            if (index > -1) {
                images.splice(index, 1);
            }
            console.log(images);
                    

            //join remaining images.
            if (images.length > 0) {
                $('#<%= locimages.ClientID %>').val(images.join("^"));
            }
            else {
                $('#<%= locimages.ClientID %>').val("");
            }
        }

    }
}
 

    </script>
    <style type="text/css">
        .style2 {
            width: 100%;
        }

        .dz-max-files-reached {
            background-color: #ffffff;
        }

        #mask {
            position: fixed;
            left: 0px;
            top: 0px;
            z-index: 4;
            opacity: 0.4;
            -ms-filter: "progid:DXImageTransform.Microsoft.Alpha(Opacity=40)"; /* first!*/
            filter: alpha(opacity=40); /* second!*/
            background-color: gray;
            display: none;
            width: 100%;
            height: 100%;
        }
    </style>

    <%--Style for adding multiple product categories - controls are in AddProductLinesControl.ascx--%>
    <style>
        .product-categories {
            margin: 10px 20px;
            border: 1px solid #ccc;
            padding: 10px;
        }

            .product-categories .head .expand {
                float: right;
                padding: 10px;
                cursor: pointer;
            }

            .product-categories .body {
                width: 100% !important;
            }

            .product-categories table {
                width: 100%;
            }

                .product-categories table tr td {
                    vertical-align: top;
                    padding: 5px;
                    background: url('../img/line.png') repeat-x 50% bottom;
                }

            .product-categories .head table tr td {
                width: 15%;
            }

                .product-categories .head table tr td:nth-child(even) {
                    width: 85%;
                }

            .product-categories .body table tr td {
                width: 15%;
            }

                .product-categories .body table tr td:nth-child(even) {
                    width: 35%;
                }

                .product-categories .body table tr td strong {
                    float: right;
                    margin-top: 8px;
                }

            .product-categories label {
                margin: 10px 0;
                display: inline-block;
                font-weight: normal;
            }

            .product-categories input,
            .product-categories textarea,
            .product-categories select {
                padding: 5px;
                margin: 0;
                border-radius: 5px;
                border: #b5b4b4 1px solid;
                line-height: 14px;
                width: 300px;
            }
    </style>
    <script>
        function uploadError_multiple() {
            // alert("Error");
        }
        function uploadStarted_multiple() {
            // alert("uploadStarted");
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="right_panel">
        <!-- appointment tabs section start -->
        <ul class="appointment_tab">
            <li><a href="home.aspx">Personal Appointment</a></li>
            <li><a href="MasterAppointment.aspx">Master Appointment</a></li>
            <li><a href="#">Construction Calendar</a></li>
            <li><a href="CallSheet.aspx">Call Sheet</a></li>
        </ul>
        <h1 id="h1Heading" runat="server"></h1>
        <div class="form_panel_custom" id="customDiv">
            <span>
                <label>
                    Customer Id:
                </label>
                <b>
                    <asp:Label ID="lblmsg" runat="server" Visible="true"></asp:Label></b> </span>
            <ul>
                <li style="width: 49%;">
                    <table id="tblcustom" runat="server" border="0" cellspacing="0" cellpadding="0">
                        <tr>
                            <td>
                                <label>
                                    Customer: <span>*</span></label>
                                <asp:TextBox ID="txtCustomer" runat="server" MaxLength="35" onkeypress="return isAlphaKey(event);"
                                    onkeyup="javascript:Alpha(this)" TabIndex="1"></asp:TextBox>
                                <label>
                                </label>
                                <asp:RequiredFieldValidator ID="rfvcustomer" runat="server" ForeColor="Red" ValidationGroup="save"
                                    ControlToValidate="txtCustomer" ErrorMessage="Enter Customer Name"></asp:RequiredFieldValidator>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <label>
                                    Proposal Terms:
                                </label>
                                <asp:TextBox ID="txtProposalTerm" runat="server" TextMode="MultiLine" autocomplete="false"
                                    TabIndex="3"></asp:TextBox>
                                <label>
                                    <asp:RequiredFieldValidator ID="rfvTerms" runat="server" ForeColor="Red" ValidationGroup="save"
                                        ControlToValidate="txtProposalTerm" ErrorMessage="Enter Proposal Term"></asp:RequiredFieldValidator>
                                </label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <label>
                                    Proposal Costs: <span>*</span></label><strong>$</strong>
                                <%--<asp:TextBox ID="txtProposalCost" AutoCompleteType="Disabled" runat="server" onclick="ShowPopup()"
                                    onkeypress="ShowPopup()" TabIndex="5"></asp:TextBox>--%>
                                <asp:TextBox ID="txtProposalCost" AutoCompleteType="Disabled" runat="server" TabIndex="5"></asp:TextBox>
                                <label>
                                </label>
                                <asp:RequiredFieldValidator ID="rfvProposalCost" runat="server" ForeColor="Red" ValidationGroup="save"
                                    ControlToValidate="txtProposalCost" ErrorMessage="Enter Proposal Cost"></asp:RequiredFieldValidator>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <label>
                                    Customer Attachment:</label>
                                <ajaxToolkit:AsyncFileUpload ID="AsyncFileUploadCustomerAttachment" runat="server" ClientIDMode="AutoID" ThrobberID="abc"
                                    OnUploadedComplete="AsyncFileUploadCustomerAttachment_UploadedComplete" CompleteBackColor="White"
                                    Style="width: 22% !important;" OnClientUploadComplete="uploadComplete2" />


                                <%-- <asp:FileUpload ID="fileAttachment" runat="server" class="multi" TabIndex="6" />--%>

                                <label>

                                    <asp:LinkButton ID="lnkDownload" runat="server" Text="" Visible="true" OnClick="lnkDownload_Click"></asp:LinkButton>

                                </label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <label>
                                    Customer Supplied Material:</label>
                                <asp:TextBox ID="txtCustSupMaterial" runat="server" Enabled="false"></asp:TextBox>
                                <label>
                                    <asp:CheckBox ID="chkCustSupMaterial" runat="server" Text="N/A" AutoPostBack="true"
                                        Checked="true" TextAlign="Right" OnCheckedChanged="chkCustSupMaterial_CheckedChanged" />
                                </label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <label>
                                    Material Storage:</label>
                                <asp:DropDownList ID="ddlMaterialStorage" runat="server" Enabled="false">
                                    <asp:ListItem>Driveway</asp:ListItem>
                                    <asp:ListItem>Garage</asp:ListItem>
                                    <asp:ListItem>Front Yard</asp:ListItem>
                                    <asp:ListItem>Back Yard</asp:ListItem>
                                    <asp:ListItem>Lside</asp:ListItem>
                                    <asp:ListItem>Rside</asp:ListItem>
                                    <asp:ListItem>Other</asp:ListItem>
                                </asp:DropDownList>
                                <label>
                                    <asp:CheckBox ID="chkMaterialStorage" runat="server" Text="N/A" TextAlign="Right" AutoPostBack="true"
                                        Checked="true" OnCheckedChanged="chkMaterialStorage_CheckedChanged" />
                                </label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <label>
                                    Waste/Dump Storage Location:</label>
                                <asp:DropDownList ID="ddlDumpStorageLocation" runat="server" Enabled="false">
                                    <asp:ListItem>Driveway</asp:ListItem>
                                    <asp:ListItem>Garage</asp:ListItem>
                                    <asp:ListItem>Front Yard</asp:ListItem>
                                    <asp:ListItem>Back Yard</asp:ListItem>
                                    <asp:ListItem>Lside</asp:ListItem>
                                    <asp:ListItem>Rside</asp:ListItem>
                                </asp:DropDownList>
                                <label>
                                    <asp:CheckBox ID="chkDumpStorageLocation" runat="server" Text="N/A" TextAlign="Right" AutoPostBack="true"
                                        Checked="true" OnCheckedChanged="chkDumpStorageLocation_CheckedChanged" />
                                </label>
                            </td>
                        </tr>
                    </table>
                </li>
                <li style="width: 49%;" class="last">
                    <table border="0" cellspacing="0" cellpadding="0">
                        <tr>
                            <td>
                                <label>
                                    Work Area:                      
                                    <asp:TextBox ID="txtworkarea" runat="server" MaxLength="35" TabIndex="2"></asp:TextBox>
                                    <label>
                                    </label>
                                    <asp:RequiredFieldValidator ID="rfvworkarea" runat="server" ForeColor="Red" ValidationGroup="save"
                                        ControlToValidate="txtworkarea" ErrorMessage="Enter Work Area"></asp:RequiredFieldValidator>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <span>
                                    Location Image: (Upload maximum 5 images) 
                                    <input id="locimages" runat="server" type="hidden" />
                                </span>
                                <div class="dropzone" style="overflow: auto; max-height: 300px;" id="dropzoneForm">
                                    <div class="fallback">
                                        <input name="file" type="file" multiple />
                                        <input type="submit" value="Upload" />
                                    </div>
                                </div>
                                <table style="display: none;" cellpadding="0" cellspacing="0" class="style2">
                                    <tr>
                                        <td style="width: 20%;">Location Image
                                        </td>
                                        <td style="width: 60%; display: none;">

                                            <ajaxToolkit:AsyncFileUpload ID="ajaxFileUpload" runat="server" ClientIDMode="AutoID"
                                                OnUploadedComplete="ajaxFileUpload_UploadedComplete" ThrobberID="imgLoad" CompleteBackColor="White"
                                                OnClientUploadComplete="uploadComplete" Style="width: 92% !important; margin-right: 6px" />

                                            <asp:Button ID="btnImageUploadClick" ClientIDMode="AutoID" runat="server" CausesValidation="false"
                                                Text="hidden" Style="display: none" OnClick="btnImageUploadClick_Click" />

                                            <%--<asp:FileUpload ID="FileUpload1" runat="server" onchange="readURL(this);" TabIndex="4"/>--%>
                                            <%--<asp:RequiredFieldValidator ID="reqUpload" runat="server" ControlToValidate="FileUpload1" 
                                                ErrorMessage="Upload atleast two image." Display="Dynamic" ForeColor="Red" SetFocusOnError="true" ValidationGroup="save">
                                            </asp:RequiredFieldValidator>--%>
                                        </td>
                                        <td></td>
                                        <td style="width: 20%;">
                                            <%--<asp:Button ID="bntAdd" runat="server" Text="Attach" Width="50px" OnClick="bntAdd_Click"
                                                OnClientClick="return ValidateAddImage()" />--%>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="3">
                                            <asp:UpdatePanel ID="pnlUpdate" runat="server">
                                                <ContentTemplate>
                                                    <asp:GridView runat="server" ID="gvCategory" AutoGenerateColumns="false" OnRowCommand="gvCategory_RowCommand"
                                                        DataKeyNames="RowSerialNo" AllowPaging="true" OnRowDataBound="gvCategory_RowDataBound"
                                                        PageSize="1" OnPageIndexChanging="gvCategory_PageIndexChanging">
                                                        <EmptyDataTemplate>
                                                            <asp:Label ID="lblNoDataFound" runat="server" Text="Image Not Found."></asp:Label>
                                                        </EmptyDataTemplate>
                                                        <Columns>
                                                            <asp:TemplateField>
                                                                <ItemStyle Width="90%" />
                                                                <HeaderTemplate>
                                                                    <asp:Label ID="Image" runat="server" Text="Image" Font-Bold="true"></asp:Label>
                                                                </HeaderTemplate>
                                                                <ItemTemplate>
                                                                    <asp:Image ID="imglocation" runat="server" ImageUrl='<%#Eval("LocationPicture")%>'
                                                                        Height="100px" Width="100px" />
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                            <asp:TemplateField>
                                                                <ItemStyle Width="10%" />
                                                                <HeaderTemplate>
                                                                </HeaderTemplate>
                                                                <ItemTemplate>
                                                                    <asp:LinkButton ID="lnkCategoryDelete" runat="server" Text="X" CommandArgument='<%#Eval("RowSerialNo")%>'
                                                                        CommandName="DeleteRec" CausesValidation="false" OnClientClick='javascript:return confirm("Are you sure want to delete this entry?");'></asp:LinkButton>
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                        </Columns>
                                                    </asp:GridView>
                                                    <asp:HiddenField ID="hidCount" runat="server" />
                                                </ContentTemplate>

                                            </asp:UpdatePanel>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <label>
                                    Special Instructions / Exemptions:
                                </label>
                                <asp:TextBox ID="txtspecialIns" runat="server" TextMode="MultiLine" TabIndex="7"></asp:TextBox>
                                <label>
                                </label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:CheckBox ID="chkPermit" runat="server" Text="Permit Required" />
                                <asp:CheckBox ID="chkHabitat" runat="server" Text="Habitat For Humanity Pick Up" />
                            </td>
                        </tr>
                        <tr>
                            <td></td>
                        </tr>
                    </table>
                </li>
            </ul>
            <div class="btn_sec">
                <asp:Button ID="btnsave" runat="server" Text="Save" ValidationGroup="save" OnClientClick="return ValidateImage()"
                    OnClick="btnsave_Click" TabIndex="8" />
                <asp:Button ID="btnexit" runat="server" Text="Exit" ValidationGroup="exit" TabIndex="9"
                    OnClick="btnexit_Click" />
                <asp:HiddenField ID="hidProdId" runat="server" />
                <asp:HiddenField ID="hidProdType" runat="server" />
            </div>

            <div>
                <asp:PlaceHolder ID="placeHolderProductLines" runat="server"></asp:PlaceHolder>
                <div class="btn_sec">
                    <asp:Button ID="btnAddProductLine" type="submit" runat="server" Text="Add Product Category" OnClick="btnAddProductLine_Click" />
                </div>
            </div>

            <div id="mask">
            </div>
            <asp:Panel ID="pnlpopup" runat="server" BackColor="White" Height="175px" Width="300px"
                Style="z-index: 111; background-color: White; position: absolute; left: 35%; top: 12%; border: outset 2px gray; padding: 5px; display: none">
                <table width="100%" style="width: 100%; height: 100%;" cellpadding="0" cellspacing="5">
                    <tr style="background-color: #b5494c">
                        <td colspan="2" style="color: White; font-weight: bold; font-size: 1.2em; padding: 3px"
                            align="center">Admin Verification <a id="closebtn" style="color: white; float: right; text-decoration: none"
                                class="btnClose" href="#">X</a>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" style="width: 45%; text-align: center;">
                            <asp:Label ID="LabelValidate" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td align="right" style="width: 45%">Amount:
                        </td>
                        <td>
                            <asp:TextBox ID="txtAmount" runat="server" onkeypress="return isNumericKey(event);"
                                MaxLength="20" Text=""></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td align="right">Admin Password:
                        </td>
                        <td>
                            <asp:TextBox ID="txtauthpass" runat="server" TextMode="Password" Text=""></asp:TextBox>
                            <asp:Label ID="lblError" runat="server" Text=""></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <input type="button" class="btnVerify" value="Verify" onclick="javascript: return focuslost();" />
                            &nbsp;&nbsp;
                            <input type="button" class="btnClose" value="Cancel" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <%--<ajaxToolkit:ModalPopupExtender ID="myModalPopupExtender" runat="server" TargetControlID="txtProposalCost"
                    PopupControlID="pnlVerify" CancelControlID="btnCancel">
                </ajaxToolkit:ModalPopupExtender>
                <asp:Panel ID="pnlVerify" runat="server" BackColor="Gray" height="200" width="450" Style="display: none;">
                    <iframe id="frameeditexpanse" runat="server" frameborder="0" src="CustomVerification.aspx" height="210" width="450">
                    </iframe>
                  <input id="btnCancel" value="Cancel" type="button" style="display:none;" />
                </asp:Panel>--%>
        </div>
    </div>
</asp:Content>
