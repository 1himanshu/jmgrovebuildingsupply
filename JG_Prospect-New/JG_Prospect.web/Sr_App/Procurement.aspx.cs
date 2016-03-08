﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using JG_Prospect.BLL;
using JG_Prospect.Common.modal;
using System.IO;
using JG_Prospect.Common;
using System.Web.Services;
using System.Net.Mail;
using System.Configuration;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using JG_Prospect.Common.Logger;
using System.Drawing;
using System.Web.Script.Serialization;

namespace JG_Prospect.Sr_App
{
    public partial class Procurement : System.Web.UI.Page
    {
        string flag = "";
        private Boolean IsPageRefresh = false;
        int estimateId = 0, customerId = 0, productTypeId = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["VendorId"] = "1";
            setPermissions();
            if (!IsPostBack)
            {
                if (Request.QueryString["FileToOpen"] != null)
                {
                    // string FileToOpen = Request.QueryString["FileToOpen"].Replace("jgp.jmgroveconstruction.com.192-185-6-42.secure23.win.hostgator.com~", "..");
                    string FileToOpen = Convert.ToString(Request.QueryString["FileToOpen"]);
                    //ClientScript.RegisterClientScriptBlock(Page.GetType(), "Myscript", "<script language='javascript'>window.open('" + Request.QueryString["FileToOpen"].ToString() + "', null, 'width=487px,height=455px,center=1,resize=0,scrolling=1,location=no');</script>");
                    ClientScript.RegisterClientScriptBlock(Page.GetType(), "Myscript", "<script language='javascript'>window.open('" + FileToOpen + "', null, 'width=487px,height=455px,center=1,resize=0,scrolling=1,location=no');</script>");
                }

                StringBuilder strerror = new StringBuilder();
                try
                {

                    Session["DisableCustid"] = "";
                    //pnlMaterialList.Visible = false;
                    strerror.Append("before sold jobs");
                    bindSoldJobs();
                    strerror.Append("after sold jobs");
                    //bindVendors();
                    strerror.Append("before delete evendar");
                    bindfordeletevender();
                    strerror.Append("after delete evendar");
                    if (Request.QueryString["UserId"] != null)
                    {
                        Session[JG_Prospect.Common.SessionKey.Key.UserId.ToString()] = Convert.ToString(Request.QueryString["UserId"]);
                    }
                    if (Request.QueryString["success"] != null)
                    {
                        if (Convert.ToString(Request.QueryString["success"]) == "0")
                        {
                            ScriptManager.RegisterStartupScript(this, GetType(), "success", "alert('Transaction is unsuccessful')", true);
                        }
                        else if (Convert.ToString(Request.QueryString["success"]) == "1")
                        {
                            if (Request.QueryString["FileToOpen"] != null)
                            {
                                string filetoopen = "../CustomerDocs/Pdfs/" + Request.QueryString["FileToOpen"] + ".pdf";
                                // filetoopen = Convert.ToString(Session["FilePath"]);
                                //ClientScript.RegisterClientScriptBlock(Page.GetType(), "Myscript", "<script language='javascript'>window.open('" + Request.QueryString["FileToOpen"].ToString() + "', null, 'width=487px,height=455px,center=1,resize=0,scrolling=1,location=no');</script>");
                                ClientScript.RegisterClientScriptBlock(Page.GetType(), "Myscript", "<script language='javascript'>window.open('" + filetoopen + "', null, 'width=487px,height=455px,center=1,resize=0,scrolling=1,location=no');</script>");
                            }
                            //ScriptManager.RegisterStartupScript(this, GetType(), "success", "alert('Transaction is successful')", true);
                        }
                    }
                    strerror.Append("before bind material");
                    bindMaterialList();
                    strerror.Append("after bind material");
                    strerror.Append("before set button");
                    SetButtonText();
                    strerror.Append("after set button");
                    strerror.Append("before bind");
                    bind();
                    strerror.Append("after bind");
                    strerror.Append("before vendors");
                    bindAllVendors();
                    strerror.Append("after vendors");
                    strerror.Append("before vendors category");
                    //bindvendorcategory();
                    strerror.Append("after vendors category");
                    strerror.Append("before folder delete vendors category");
                    bindfordeletevender();
                    strerror.Append("after folder delete vendors category");
                    ScriptManager.RegisterStartupScript(this, GetType(), "initialize", "initialize();", true);
                    //lnkVendorCategory.ForeColor = System.Drawing.Color.DarkGray;
                    //lnkVendorCategory.Enabled = false;
                    //lnkVendor.Enabled = true;
                    //lnkVendor.ForeColor = System.Drawing.Color.Blue;
                    // lblerrornew.Text = Convert.ToString(strerror);
                    BindProductCategory();
                    GetAllVendorSubCat();
                    BindVendorByProdCat(ddlprdtCategory.SelectedValue.ToString());
                    BindVendorSubCatByVendorCat(ddlVndrCategory.SelectedValue.ToString());
                    string ManufacturerType = GetManufacturerType();
                    FilterVendors("", "ManufacturerType", ManufacturerType, "");
                }
                catch (Exception ex)
                {
                    lblerrornew.Text = ex.Message + ex.StackTrace;
                }
            }
            else
            {
                IsPageRefresh = true;
            }
        }

        // Modification

        protected void BindProductCategory()
        {
            ddlprdtCategory.Items.Clear();
            DataSet ds = new DataSet();
            ds = AdminBLL.Instance.GetProductCategory();
            ddlprdtCategory.DataSource = ds;
            ddlprdtCategory.DataTextField = "ProductName";
            ddlprdtCategory.DataValueField = "ProductId";
            ddlprdtCategory.DataBind();
            ddlprdtCategory.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "Select"));

            ddlProductCatgoryPopup.DataSource = ds;
            ddlProductCatgoryPopup.DataTextField = "ProductName";
            ddlProductCatgoryPopup.DataValueField = "ProductId";
            ddlProductCatgoryPopup.DataBind();
            ddlProductCatgoryPopup.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "Select"));

        }

        public string GetManufacturerType()
        {
            string MType = "";
            if (rdoRetailWholesale.Checked)
                MType = rdoRetailWholesale.Text;
            else if (rdoManufacturer.Checked)
                MType = rdoManufacturer.Text;
            return MType;
        }
        protected void rdoRetailWholesale_CheckedChanged(object sender, EventArgs e)
        {
            string ManufacturerType = GetManufacturerType();
            if (ddlVendorSubCategory.SelectedValue.ToString() != "Select")
            {
                FilterVendors(ddlVendorSubCategory.SelectedValue.ToString(), "VendorSubCategory", ManufacturerType, ddlVndrCategory.SelectedValue.ToString());
            }
            else if (ddlVndrCategory.SelectedValue.ToString() != "Select")
            {
                FilterVendors(ddlVndrCategory.SelectedValue.ToString(), "VendorCategory", ManufacturerType, "");
            }
            else if (ddlprdtCategory.SelectedValue.ToString() != "Select")
            {
                FilterVendorByProductCategory();
            }
            else
            {
                FilterVendors("", "ManufacturerType", ManufacturerType, "");
            }
            //ResetFilterDDL();
        }

        protected void rdoManufacturer_CheckedChanged(object sender, EventArgs e)
        {
            string ManufacturerType = GetManufacturerType();
            if (ddlVendorSubCategory.SelectedValue.ToString() != "Select")
            {
                FilterVendors(ddlVendorSubCategory.SelectedValue.ToString(), "VendorSubCategory", ManufacturerType, ddlVndrCategory.SelectedValue.ToString());
            }
            else if (ddlVndrCategory.SelectedValue.ToString() != "Select")
            {
                FilterVendors(ddlVndrCategory.SelectedValue.ToString(), "VendorCategory", ManufacturerType, "");
            }
            else if (ddlprdtCategory.SelectedValue.ToString() != "Select")
            {
                FilterVendorByProductCategory();
            }
            else
            {
                FilterVendors("", "ManufacturerType", ManufacturerType, "");
            }
        }

        public void ResetFilterDDL()
        {
            ddlprdtCategory.SelectedIndex = -1;
            ddlVndrCategory.SelectedIndex = -1;
            ddlVendorSubCategory.SelectedIndex = -1;
            ddlProductCatgoryPopup.SelectedIndex = -1;
        }
        protected void ddlprdtCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlVndrCategory.SelectedIndex = -1;
            ddlVendorSubCategory.SelectedIndex = -1;
            BindVendorByProdCat(ddlprdtCategory.SelectedValue.ToString());
            BindVendorCatPopup();
            if (ddlprdtCategory.SelectedValue.ToString() != "Select")
            {
                ddlProductCatgoryPopup.SelectedValue = ddlprdtCategory.SelectedValue;
                FilterVendorByProductCategory();
            }
            else
            {
                string ManufacturerType = GetManufacturerType();
                FilterVendors("", "ManufacturerType", ManufacturerType, "");
            }

        }

        public void BindVendorByProdCat(string ProductId)
        {
            DataSet ds = new DataSet();
            ds = AdminBLL.Instance.GetVendorCategory(ProductId);
            ddlVndrCategory.DataSource = ds;
            ddlVndrCategory.DataTextField = "VendorCategoryName";
            ddlVndrCategory.DataValueField = "VendorCategoryId";
            ddlVndrCategory.DataBind();
            ddlVndrCategory.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "Select"));
        }

        public void FilterVendorByProductCategory()
        {
            StringBuilder strVendorCategory = new StringBuilder();
            string FilterParams = "";
            if (ddlVndrCategory.Items.Count > 1)
            {
                for (int i = 1; i < ddlVndrCategory.Items.Count; i++)
                {
                    strVendorCategory.Append(ddlVndrCategory.Items[i].Value.ToString()).Append(",");
                }

                FilterParams = strVendorCategory.Remove(strVendorCategory.Length - 1, 1).ToString();
                string ManufacturerType = GetManufacturerType();
                FilterVendors(FilterParams, "ProductCategory", ManufacturerType, "");
            }
            else
            {
                grdVendorList.DataSource = null;
                grdVendorList.DataBind();
            }
        }
        protected void ddlVndrCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlVendorSubCategory.SelectedIndex = -1;
            BindVendorSubCatByVendorCat(ddlVndrCategory.SelectedValue.ToString());
            string ManufacturerType = GetManufacturerType();

            if (ddlVndrCategory.SelectedValue.ToString() != "Select")
            {
                ddlVendorCatPopup.SelectedValue = ddlVndrCategory.SelectedValue;
                FilterVendors(ddlVndrCategory.SelectedValue.ToString(), "VendorCategory", ManufacturerType, "");
            }
            else
            {
                FilterVendorByProductCategory();
            }
        }

        public void BindVendorSubCatByVendorCat(string VendorCatId)
        {
            DataSet ds = new DataSet();
            ds = AdminBLL.Instance.GetVendorSubCategory(VendorCatId);
            ddlVendorSubCategory.DataSource = ds;
            ddlVendorSubCategory.DataTextField = "VendorSubCategoryName";
            ddlVendorSubCategory.DataValueField = "VendorSubCategoryId";
            ddlVendorSubCategory.DataBind();
            ddlVendorSubCategory.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "Select"));

        }

        protected void ddlVendorSubCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            string ManufacturerType = GetManufacturerType();
            if (ddlVendorSubCategory.SelectedValue.ToString() != "Select")
                FilterVendors(ddlVendorSubCategory.SelectedValue.ToString(), "VendorSubCategory", ManufacturerType, ddlVndrCategory.SelectedValue.ToString());
            else if (ddlVndrCategory.SelectedValue.ToString() != "Select")
                FilterVendors(ddlVndrCategory.SelectedValue.ToString(), "VendorCategory", ManufacturerType, "");
            else
                FilterVendorByProductCategory();

        }

        public void FilterVendors(string FilterParams, string FilterBy, string ManufacturerType, string VendorCategoryId)
        {
            grdVendorList.DataSource = null;
            grdVendorList.DataBind();
            DataSet ds = new DataSet();
            ds = VendorBLL.Instance.GetVendorList(FilterParams, FilterBy, ManufacturerType, VendorCategoryId);
            grdVendorList.DataSource = ds;
            grdVendorList.DataBind();
        }

        public class NameValue
        {
            //Address
            public string key { get; set; }
            public string value { get; set; }
        }
        public class AddressClass
        {
            public string AddressType { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string Zip { get; set; }
        }
        public class EmailClass
        {
            public string EmailType { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public List<ContactClass> Contact { get; set; }
        }
        public class ContactClass
        {
            public string Extension { get; set; }
            public string Number { get; set; }
        }
        // New Code
        [System.Web.Services.WebMethodAttribute(), System.Web.Script.Services.ScriptMethodAttribute()]
        public static void PostVendorDetails(string vendorid, List<AddressClass> Address, List<EmailClass> VendorEmails)
        {
            //string vendorid = "1";
            DataTable dtVendorEmail = new DataTable("VendorEmail");
            if (dtVendorEmail.Columns.Count < 1)
            {
                dtVendorEmail.Columns.Add("VendorId");
                dtVendorEmail.Columns.Add("EmailType");
                dtVendorEmail.Columns.Add("SeqNo");
                dtVendorEmail.Columns.Add("Email");
                dtVendorEmail.Columns.Add("FName");
                dtVendorEmail.Columns.Add("LName");
                dtVendorEmail.Columns.Add("Contact");
            }
            DataRow drow = dtVendorEmail.NewRow();
            if (VendorEmails.Count > 0)
            {
                for (int i = 0; i < VendorEmails.Count; i++)
                {
                    drow["VendorId"] = vendorid;
                    drow["EmailType"] = VendorEmails[i].EmailType.ToString();
                    drow["SeqNo"] = i + 1;
                    drow["Email"] = VendorEmails[i].Email.ToString();
                    drow["FName"] = VendorEmails[i].FirstName.ToString();
                    drow["LName"] = VendorEmails[i].LastName.ToString();
                    JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
                    string serializedContactJson = jsSerializer.Serialize(VendorEmails[i].Contact);
                    drow["Contact"] = serializedContactJson;
                    dtVendorEmail.Rows.Add(drow);
                    drow = dtVendorEmail.NewRow();
                }
            }

            HttpContext.Current.Session["dtVendorEmail"] = dtVendorEmail as DataTable;

            DataTable dtVendorAddress = new DataTable("VendorAddress");
            if (dtVendorAddress.Columns.Count < 1)
            {
                dtVendorAddress.Columns.Add("VendorId");
                dtVendorAddress.Columns.Add("AddressType");
                dtVendorAddress.Columns.Add("Address");
                dtVendorAddress.Columns.Add("City");
                dtVendorAddress.Columns.Add("Zip");
            }
            DataRow AddressRow = dtVendorAddress.NewRow();
            if (Address.Count > 0)
            {
                for (int i = 0; i < Address.Count; i++)
                {
                    AddressRow["VendorId"] = vendorid;
                    AddressRow["AddressType"] = Address[i].AddressType.ToString();
                    AddressRow["Address"] = Address[i].Address.ToString();
                    AddressRow["City"] = Address[i].City.ToString();
                    AddressRow["Zip"] = Address[i].Zip.ToString();
                    dtVendorAddress.Rows.Add(AddressRow);
                    AddressRow = dtVendorAddress.NewRow();
                }
            }
            HttpContext.Current.Session["dtVendorAddress"] = dtVendorAddress as DataTable;

            //JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            //string deserializedJson = jsSerializer.Serialize(formVars);
            //DataTable dtContact4 = new DataTable();
            //dtContact4 = GetAllContact4Values(formVars);
            //HttpContext.Current.Session["dtContact4"] = dtContact4 as DataTable;
            // return "";
        }


        protected void btnSave_Click(object sender, EventArgs e)
        {
            //Save all Data...
            flag = "";
            SaveAllData();


            //clearcontrols();
        }


        protected void SaveAllData()
        {
            Vendor objvendor = new Vendor();
            if (ddlVndrCategory.SelectedValue == "Select")
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('please select Vendor Category');", true);
                return;
            }
            else if (ddlVendorSubCategory.SelectedValue == "Select")
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('please select Vendor Sub Category');", true);
                return;

            }
            else
            {

                //if (ddlVendorName.SelectedValue != "")
                //    objvendor.vendor_id = Convert.ToInt32(ddlVendorName.SelectedValue);
                //else
                objvendor.vendor_id = Convert.ToInt32(txtVendorId.Text);

                objvendor.vendor_name = txtVendorNm.Text;

                objvendor.vendor_category_id = Convert.ToInt32(ddlVndrCategory.SelectedValue);

               
                objvendor.fax = txtVendorFax.Text;
                objvendor.mail = txtprimaryemail.Text;
                objvendor.contract_person = txtFName.Text + " " + txtLName.Text;
                objvendor.contract_number = txtContact1.Text;
                objvendor.ContactExten = txtContactExten1.Text;
                objvendor.address = txtPrimaryAddress.Text;
                objvendor.notes = "";
                objvendor.ManufacturerType = (ddlmanufacturertype.SelectedValue == "Select") ? "" : ddlmanufacturertype.SelectedValue;
                objvendor.BillingAddress = txtBillingAddr.Text;
                objvendor.TaxId = txtTaxId.Text;
                objvendor.ExpenseCategory = "";
                objvendor.AutoTruckInsurance = "";
                objvendor.vendor_subcategory_id = Convert.ToInt32(ddlVendorSubCategory.SelectedValue);
                objvendor.VendorStatus = (ddlVendorStatus.SelectedValue == "Select") ? "" : ddlVendorStatus.SelectedValue;
                objvendor.Website = txtWebsite.Text;
                bool res = VendorBLL.Instance.savevendor(objvendor);
                if (flag == "")
                {
                    if (res)
                    {
                        objvendor.tblVendorEmail = (DataTable)HttpContext.Current.Session["dtVendorEmail"];
                        bool emailres = VendorBLL.Instance.InsertVendorEmail(objvendor);
                        objvendor.tblVendorAddress = (DataTable)HttpContext.Current.Session["dtVendorAddress"];
                        bool addressres = VendorBLL.Instance.InsertVendorAddress(objvendor);
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Vendor Saved/Updated Successfully');", true);
                        clear();
                    }
                }
            }
        }
        protected void clear()
        {
            txtContact1.Text = txtContactExten1.Text = txtFName.Text = txtLName.Text = txtVendorFax.Text = txtprimaryemail.Text = txtVendorNm.Text = null;
            txtWebsite.Text = txtTaxId.Text = txtPrimaryAddress.Text = txtPrimaryCity.Text = txtPrimaryZip.Text = null;
            txtSecAddress.Text = txtSecCity.Text = txtSeczip.Text = null;
            txtBillingAddr.Text = txtBillingCity.Text = txtBillingZip.Text = null;
            txtPrimaryContactExten0.Text = txtPrimaryContact0.Text = txtSecContactExten0.Text = txtSecContact0.Text = txtAltContactExten0.Text = txtAltContact0.Text = null;
            //ddlVndrCategory.ClearSelection();
            ddlVendorStatus.ClearSelection();
            ddlmanufacturertype.ClearSelection();
            btnSave.Text = "Save";
        }
        protected void lnkVendorName_Click(object sender, EventArgs e)
        {
            LinkButton lnkbtnVendorName = sender as LinkButton;
            GridViewRow gr = (GridViewRow)lnkbtnVendorName.Parent.Parent;
            HiddenField hdnVendorId = (HiddenField)gr.FindControl("hdnVendorId");
            EditVendor(Convert.ToInt16(hdnVendorId.Value));
            updtpnlAddVender.Update();
        }
        protected void lnkAddVendorCategory1_Click(object sender, EventArgs e)
        {

        }


        protected void bindAllVendors()
        {
            //lstVendors.Items.Clear();
            //DataSet ds = new DataSet();
            //ds = VendorBLL.Instance.fetchallvendordetails();
            //lstVendors.DataSource = ds;
            //lstVendors.DataTextField = "VendorName";
            //lstVendors.DataValueField = "VendorId";

            //lstVendors.DataBind();


            //ddlVendorName.Items.Clear();
            //DataSet ds = new DataSet();
            //ds = VendorBLL.Instance.fetchallvendordetails();
            //ddlVendorName.DataSource = ds;
            //ddlVendorName.DataTextField = "VendorName";
            //ddlVendorName.DataValueField = "VendorId";

            //ddlVendorName.DataBind();


        }
        //protected void bindvendor(int selectedVendorCategoryId)
        //{
        //    //lstVendors.Items.Clear();

        //    //DataSet dsVendorNames = VendorBLL.Instance.fetchVendorNamesByVendorCategory(selectedVendorCategoryId);
        //    //lstVendors.DataSource = dsVendorNames;
        //    //lstVendors.DataTextField = "VendorName";
        //    //lstVendors.DataValueField = "VendorId";
        //    //lstVendors.DataBind();


        //    ddlVendorName.Items.Clear();

        //    DataSet dsVendorNames = VendorBLL.Instance.fetchVendorNamesByVendorCategory(selectedVendorCategoryId);
        //    ddlVendorName.DataSource = dsVendorNames;
        //    ddlVendorName.DataTextField = "VendorName";
        //    ddlVendorName.DataValueField = "VendorId";
        //    ddlVendorName.DataBind();






        //    //DataSet ds = new DataSet();
        //    //ds = VendorBLL.Instance.fetchallvendordetails();
        //    //lstVendors.DataSource = ds.Tables[0];
        //    //lstVendors.DataTextField = ds.Tables[0].Columns[1].ToString();
        //    //lstVendors.DataValueField = ds.Tables[0].Columns[0].ToString();

        //    //lstVendors.DataBind();


        //}
        protected void bindvendorcategory()
        {
            DataSet ds = new DataSet();
            ds = VendorBLL.Instance.fetchallvendorcategory();
            ddlVndrCategory.DataSource = ds;
            ddlVndrCategory.DataTextField = ds.Tables[0].Columns[1].ToString();
            ddlVndrCategory.DataValueField = ds.Tables[0].Columns[0].ToString();
            ddlVndrCategory.DataBind();
            ddlVndrCategory.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "Select"));

            ddlVendorCatPopup.DataSource = ds;
            ddlVendorCatPopup.DataTextField = ds.Tables[0].Columns[1].ToString();
            ddlVendorCatPopup.DataValueField = ds.Tables[0].Columns[0].ToString();
            ddlVendorCatPopup.DataBind();
            ddlVendorCatPopup.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "Select"));

        }

        public void BindVendorCatPopup()
        {
            DataSet ds = new DataSet();
            ds = VendorBLL.Instance.fetchallvendorcategory();
            ddlVendorCatPopup.DataSource = ds;
            ddlVendorCatPopup.DataTextField = ds.Tables[0].Columns[1].ToString();
            ddlVendorCatPopup.DataValueField = ds.Tables[0].Columns[0].ToString();
            ddlVendorCatPopup.DataBind();
            ddlVendorCatPopup.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "Select"));
        }

        //protected void lstVendors_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    DataSet ds = new DataSet();
        //    ds = VendorBLL.Instance.FetchvendorDetails(Convert.ToInt32(ddlVendorName.SelectedValue));

        //    txtVendorNm.Text = ds.Tables[0].Rows[0]["VendorName"].ToString();
        //    ddlVndrCategory.SelectedValue = ds.Tables[0].Rows[0]["VendorCategoryId"].ToString();
        //    txtcontactperson.Text = ds.Tables[0].Rows[0]["ContactPerson"].ToString();
        //    txtcontactnumber.Text = ds.Tables[0].Rows[0]["ContactNumber"].ToString();
        //    txtfax.Text = ds.Tables[0].Rows[0]["Fax"].ToString();
        //    txtmail.Text = ds.Tables[0].Rows[0]["Email"].ToString();
        //    txtaddress.Text = ds.Tables[0].Rows[0]["Address"].ToString();
        //    txtNotes.Text = ds.Tables[0].Rows[0]["Notes"].ToString();
        //    ddlMenufacturer.SelectedValue = ds.Tables[0].Rows[0]["ManufacturerType"].ToString();
        //    txtBillingAddress.Text = ds.Tables[0].Rows[0]["BillingAddress"].ToString();
        //    txtTaxId.Text = ds.Tables[0].Rows[0]["TaxId"].ToString();
        //    txtExpenseCat.Text = ds.Tables[0].Rows[0]["ExpenseCategory"].ToString();
        //    txtAutoInsurance.Text = ds.Tables[0].Rows[0]["AutoTruckInsurance"].ToString();
        //    txtVendorId.Text = ddlVendorName.SelectedValue;
        //    btnSave.Text = "Update";
        //}




        //protected void btndelete_Click(object sender, EventArgs e)
        //{
        //    bool result = VendorBLL.Instance.deletevendorcategory(Convert.ToInt32(ddlvendercategoryname.SelectedValue));
        //    bindfordeletevender();
        //    bindvendorcategory();
        //    lstVendors.Items.Clear();
        //    clearcontrols();

        //}

        //protected void bindfordeletevender()
        //{
        //    DataSet ds = new DataSet();
        //    ds = VendorBLL.Instance.fetchallvendorcategory();
        //    ddlvendercategoryname.DataSource = ds;
        //    ddlvendercategoryname.DataTextField = ds.Tables[0].Columns[1].ToString();
        //    ddlvendercategoryname.DataValueField = ds.Tables[0].Columns[0].ToString();
        //    ddlvendercategoryname.DataBind();


        //}


        protected void btndeletevender_Click(object sender, EventArgs e)
        {
            bool result = VendorBLL.Instance.deletevendorcategory(Convert.ToInt32(ddlvendercategoryname.SelectedValue));
            if (result)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Vendor Category has been deleted Successfully');", true);

            }
            else
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Vendor Category cannot be deleted, delete all vendors of this category');", true);
            bindvendorcategory();
            bindfordeletevender();
        }

        protected void clearcontrols()
        {
            //txtVendorNm.Text = "";
            //txtcontactperson.Text = "";
            //txtcontactnumber.Text = "";
            //txtfax.Text = "";
            //txtmail.Text = "";
            //txtaddress.Text = "";
            //ddlMenufacturer.SelectedValue = "0";
            //txtBillingAddress.Text = "";
            //txtTaxId.Text = "";
            //txtExpenseCat.Text = "";
            //txtAutoInsurance.Text = "";
        }

        //protected void ddlVendorCategory_SelectedIndexChanged1(object sender, EventArgs e)
        //{
        //    if (ddlVendorCategory.SelectedValue != "Select")
        //    {
        //        int selectedVendorCategoryId = Convert.ToInt16(ddlVendorCategory.SelectedValue);
        //        bindvendor(selectedVendorCategoryId);
        //    }
        //    else
        //    {
        //        ddlVendorName.Items.Clear();
        //    }

        //    clearcontrols();
        //    btnSave.Text = "Save";


        //    //  flag = "Autosave";
        //    //  SaveAllData(); 

        //}

        protected void VerifyAdminPermission(object sender, EventArgs e)
        {
            int cResult = CustomBLL.Instance.WhetherVendorInCustomMaterialListExists(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//,productTypeId,estimateId);
            if (cResult == 1)
            {
                if (!string.IsNullOrEmpty(txtAdminPassword.Text))
                {
                    string adminCode = AdminBLL.Instance.GetAdminCode();
                    if (adminCode != txtAdminPassword.Text.Trim())
                    {
                        CVAdmin.ErrorMessage = "Invalid Admin Code";
                        CVAdmin.ForeColor = System.Drawing.Color.Red;
                        CVAdmin.IsValid = false;
                        CVAdmin.Visible = true;
                        //popupAdmin_permission.Show();
                        return;
                    }
                    else
                    {
                        int result = CustomBLL.Instance.UpdateAdminPermissionOfCustomMaterialList(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]), JGConstant.PERMISSION_STATUS_GRANTED, Convert.ToString(Session["loginid"]));//, productTypeId, estimateId);
                        if (result == 0)
                        {
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First save Material List');", true);
                        }
                        else
                        {
                            //lnkAdminPermission.Enabled = false;
                            //lnkAdminPermission.ForeColor = System.Drawing.Color.DarkGray;
                            //popupAdmin_permission.TargetControlID = "hdnAdmin";
                            SetButtonText();
                            DisableVendorNameAndAmount();
                        }
                    }
                }
                else
                {
                    CVAdmin.ErrorMessage = "Please Enter Admin Code";
                    CVAdmin.ForeColor = System.Drawing.Color.Red;
                    CVAdmin.IsValid = false;
                    CVAdmin.Visible = true;
                    //popupAdmin_permission.Show();
                    return;
                }
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First save Material List and enter all vendor names');", true);
            }
            //message mail is not sent to categories
        }


        protected void VerifyForemanPermission(object sender, EventArgs e)
        {
            int cResult = CustomBLL.Instance.WhetherCustomMaterialListExists(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//, productTypeId, estimateId);
            if (cResult == 1)
            {
                if (!string.IsNullOrEmpty(txtForemanPassword.Text))
                {
                    string adminCode = AdminBLL.Instance.GetForemanCode();
                    if (adminCode != txtForemanPassword.Text.Trim())
                    {
                        CVForeman.ErrorMessage = "Invalid Foreman Code";
                        CVForeman.ForeColor = System.Drawing.Color.Red;
                        CVForeman.IsValid = false;
                        CVForeman.Visible = true;
                        //popupForeman_permission.Show();
                        return;
                    }
                    else
                    {
                        int result = CustomBLL.Instance.UpdateForemanPermissionOfCustomMaterialList2(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]), JGConstant.PERMISSION_STATUS_GRANTED, Convert.ToString(Session["loginid"]));//, productTypeId, estimateId);
                        if (result == 0)
                        {
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First save Material List');", true);
                        }
                        else
                        {
                            //lnkForemanPermission.Enabled = false;
                            //lnkForemanPermission.ForeColor = System.Drawing.Color.DarkGray;
                            //popupForeman_permission.TargetControlID = "hdnForeman";
                            SetButtonText();
                        }
                    }
                }
                else
                {
                    CVForeman.ErrorMessage = "Please Enter Foreman Code";
                    CVForeman.ForeColor = System.Drawing.Color.Red;
                    CVForeman.IsValid = false;
                    CVForeman.Visible = true;
                    //popupForeman_permission.Show();
                    return;
                }
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First save Material List');", true);
            }
        }

        protected void btnXSrSalesmanF_Click(object sender, EventArgs e)
        {
            //popupSrSalesmanPermissionF.Hide();
        }

        protected void ddlVendorCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlVndrCategory = (DropDownList)sender;

            string selectedCategory = ddlVndrCategory.SelectedItem.Text;
            string emailStatus = CustomBLL.Instance.GetEmailStatusOfCustomMaterialList(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//, productTypeId, estimateId);
            int counter = 1;
            //foreach (GridViewRow r in grdcustom_material_list.Rows)
            //{
            //    if (selectedCategory == "Select")
            //    {
            //        ddlVndrCategory.SelectedIndex = -1;
            //        ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Please select a vendor category');", true);

            //    }
            //    else if (((DropDownList)r.FindControl("ddlVndrCategory")).SelectedItem.Text == selectedCategory)
            //    {
            //        if (counter == 2)
            //        {
            //            ddlVndrCategory.SelectedIndex = -1;
            //            ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('This Vendor Category is already selected');", true);

            //        }
            //        counter++;

            //    }
            //    if (emailStatus == JGConstant.EMAIL_STATUS_VENDORCATEGORIES)
            //    {
            //        DropDownList ddlVendorName = (DropDownList)r.FindControl("ddlVendorName");
            //        DropDownList ddlVndrCategorySelected = (DropDownList)r.FindControl("ddlVndrCategory");
            //        LinkButton lnkQuote = (LinkButton)r.FindControl("lnkQuote");
            //        if (ddlVndrCategory == ddlVndrCategorySelected)
            //        {
            //            int selectedCategoryID = Convert.ToInt16(ddlVndrCategory.SelectedItem.Value);
            //            DataSet ds = GetVendorNames(selectedCategoryID);
            //            ddlVendorName.DataSource = ds;
            //            ddlVendorName.SelectedIndex = -1;
            //            ddlVendorName.DataTextField = "VendorName";
            //            ddlVendorName.DataValueField = "VendorId";
            //            ddlVendorName.DataBind();
            //            ddlVendorName.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "0"));
            //            ddlVendorName.SelectedIndex = 0;

            //            lnkQuote.Text = "";
            //        }
            //    }
            //}
        }

        protected void ddlVendorName_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlVendorName = (DropDownList)sender;
            string selectedName = ddlVendorName.SelectedItem.Text;
            int vendorId = Convert.ToInt16(ddlVendorName.SelectedValue.ToString());

            //foreach (GridViewRow r in grdcustom_material_list.Rows)
            //{
            //    if (selectedName == "Select")
            //    {
            //        ddlVendorName.SelectedIndex = -1;
            //        ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Please select a vendor name');", true);

            //    }
            //}
            DataSet ds = VendorBLL.Instance.GetVendorQuoteByVendorId(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]), vendorId);
            if (ds.Tables[0].Rows.Count <= 0)
            {
                ddlVendorName.SelectedIndex = -1;
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First attach quote for this vendor');", true);

            }

            //foreach (GridViewRow r in grdcustom_material_list.Rows)
            //{
            //    DropDownList ddlVendorName1 = (DropDownList)r.FindControl("ddlVendorName");
            //    {
            //        DataSet dsVendorQuoute = VendorBLL.Instance.GetVendorQuoteByVendorId(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]), Convert.ToInt16(ddlVendorName1.SelectedValue));
            //        LinkButton lnkQuote = (LinkButton)r.FindControl("lnkQuote");
            //        if (dsVendorQuoute.Tables[0].Rows.Count > 0)
            //        {
            //            lnkQuote.Text = dsVendorQuoute.Tables[0].Rows[0]["DocName"].ToString();
            //            lnkQuote.CommandArgument = dsVendorQuoute.Tables[0].Rows[0]["TempName"].ToString();
            //        }
            //        else
            //        {
            //            lnkQuote.Text = "";
            //            lnkQuote.CommandArgument = "";
            //        }
            //    }
            //}
        }

        protected void Add_Click(object sender, EventArgs e)
        {
            List<CustomMaterialList> cmList = BindEmptyRowToMaterialList();
            ViewState["CustomMaterialList"] = cmList;
            BindCustomMaterialList(cmList);
        }

        protected void VerifySrSalesmanPermissionF(object sender, EventArgs e)
        {
            int cResult = CustomBLL.Instance.WhetherCustomMaterialListExists(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//, productTypeId, estimateId);
            if (cResult == 1)
            {
                if (!string.IsNullOrEmpty(txtSrSalesmanPasswordF.Text))
                {
                    string salesmanCode = Session["loginpassword"].ToString();
                    if (salesmanCode != txtSrSalesmanPasswordF.Text.Trim())
                    {
                        CVSrSalesmanF.ErrorMessage = "Invalid Sr. Salesman Code";
                        CVSrSalesmanF.ForeColor = System.Drawing.Color.Red;
                        CVSrSalesmanF.IsValid = false;
                        CVSrSalesmanF.Visible = true;
                        //popupSrSalesmanPermissionF.Show();
                        return;
                    }
                    else
                    {
                        int result = CustomBLL.Instance.UpdateSrSalesmanPermissionOfCustomMaterialListF(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]), JGConstant.PERMISSION_STATUS_GRANTED, Convert.ToString(Session["loginid"]));//, productTypeId, estimateId);
                        if (result == 0)
                        {
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First save Material List');", true);
                        }
                        else
                        {
                            //lnkSrSalesmanPermissionF.Enabled = false;
                            //lnkSrSalesmanPermissionF.ForeColor = System.Drawing.Color.DarkGray;
                            //popupSrSalesmanPermissionF.TargetControlID = "hdnSrF";
                            SetButtonText();
                        }
                    }
                }
                else
                {
                    CVSrSalesmanF.ErrorMessage = "Please Enter Sr. Salesman Code";
                    CVSrSalesmanF.ForeColor = System.Drawing.Color.Red;
                    CVSrSalesmanF.IsValid = false;
                    CVSrSalesmanF.Visible = true;
                    //popupSrSalesmanPermissionF.Show();
                    return;

                }
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First save Material List');", true);
            }
        }

        private void bindSoldJobs()
        {
            DataSet ds = new_customerBLL.Instance.GetSoldjobsforprocurement();
            //DataSet ds1 = VendorBLL.Instance.GetAllvendorDetails();

            if (ds != null)
            {
                grdsoldjobs.DataSource = ds;
                grdsoldjobs.Columns[7].Visible = false;
                grdsoldjobs.DataBind();
            }
        }
        private void bindVendors()
        {
            DataSet ds = VendorBLL.Instance.fetchAllVendorCategoryHavingVendors();
            if (ds != null)
            {
                //grdvendors.DataSource = ds;
                //grdvendors.DataBind();
                //grdvendors.Columns[1].Visible = false;
            }
        }
        protected void ddlstatus_selectedindexchanged(object sender, EventArgs e)
        {
            DropDownList ddlstatus = sender as DropDownList;
            GridViewRow gr = (GridViewRow)ddlstatus.Parent.Parent;
            LinkButton lblcustid = (LinkButton)gr.FindControl("lnkcustomerid");
            LinkButton lnkmateriallist = (LinkButton)gr.FindControl("lnkmateriallist");
            HiddenField hdnproductid = (HiddenField)gr.FindControl("hdnproductid");
            Label lblProductType = (Label)gr.FindControl("lblProductType");

            string soldjobId = lnkmateriallist.Text.Trim().Split('M')[0].Trim();
            int custId = Convert.ToInt16(lblcustid.Text.ToString().Substring(1));
            int userId = Convert.ToInt16(Session[JG_Prospect.Common.SessionKey.Key.UserId.ToString()]);

            if (ddlstatus.SelectedValue != JGConstant.ZERO.ToString())
            {
                new_customerBLL.Instance.AddCustomerFollowUp(custId, DateTime.Now, ddlstatus.SelectedItem.Text, userId, false, 0, "");
                new_customerBLL.Instance.UpdateStatusOfCustomer(soldjobId, Convert.ToInt16(ddlstatus.SelectedValue));//, Convert.ToInt16(lblProductType.Text), Convert.ToInt16(hdnproductid.Value));
            }
            else if (ddlstatus.SelectedValue == JGConstant.ZERO.ToString())
            {
                new_customerBLL.Instance.AddCustomerFollowUp(custId, DateTime.Now, JGConstant.CUSTOMER_STATUS_ORDERED, userId, false, 0, "");
                new_customerBLL.Instance.UpdateStatusOfCustomer(soldjobId, 13);//, Convert.ToInt16(lblProductType.Text), Convert.ToInt16(hdnproductid.Value));
            }
            bindSoldJobs();
        }
        protected void grdvendors_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                try
                {
                    string vendorCategoryId = e.Row.Cells[1].Text;

                    DataSet dsVendorNames = VendorBLL.Instance.fetchVendorNamesByVendorCategory(Convert.ToInt16(vendorCategoryId));
                    DropDownList drpVendorName = (DropDownList)e.Row.FindControl("drpVendorName");
                    drpVendorName.DataSource = dsVendorNames;
                    drpVendorName.DataTextField = "VendorName";
                    drpVendorName.DataValueField = "VendorId";
                    drpVendorName.DataBind();
                    drpVendorName.SelectedIndex = 1;
                    DataSet dsVendorDetails = new DataSet();
                    if (drpVendorName.SelectedValue != "")
                    {
                        dsVendorDetails = VendorBLL.Instance.fetchVendorDetailsByVendorId(Convert.ToInt16(drpVendorName.SelectedValue));
                    }
                    else
                    {
                        dsVendorDetails = null;
                    }
                    if (dsVendorDetails != null)
                    {
                        Label lblContactPerson = (Label)e.Row.FindControl("lblContactPerson");
                        lblContactPerson.Text = dsVendorDetails.Tables[0].Rows[0]["ContactPerson"].ToString();
                        Label lblContactNumber = (Label)e.Row.FindControl("lblContactNumber");
                        lblContactNumber.Text = dsVendorDetails.Tables[0].Rows[0]["ContactNumber"].ToString();
                        Label lblFax = (Label)e.Row.FindControl("lblFax");
                        lblFax.Text = dsVendorDetails.Tables[0].Rows[0]["Fax"].ToString();
                        Label lblEmail = (Label)e.Row.FindControl("lblEmail");
                        lblEmail.Text = dsVendorDetails.Tables[0].Rows[0]["Email"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    //
                }
            }
        }
        protected void bindfordeletevender()
        {
            DataSet ds = new DataSet();
            ds = VendorBLL.Instance.fetchallvendorcategory();
            ddlvendercategoryname.DataSource = ds;
            ddlvendercategoryname.DataTextField = ds.Tables[0].Columns[1].ToString();
            ddlvendercategoryname.DataValueField = ds.Tables[0].Columns[0].ToString();
            ddlvendercategoryname.DataBind();


        }

        protected void btnsave_Click(object sender, EventArgs e)
        {
            Vendor_Catalog objcatalog = new Vendor_Catalog();

            objcatalog.catalog_name = txtname.Text;
            bool res = VendorBLL.Instance.savevendorcatalogdetails(objcatalog);
            if (res)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Data has been inserted Successfully');", true);
                bindfordeletevender();

            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('There is some error');", true);
            }

        }

        protected void btndelete_Click(object sender, EventArgs e)
        {
            bool result = VendorBLL.Instance.deletevendorcategory(Convert.ToInt32(ddlvendercategoryname.SelectedValue));
            if (result)
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Vendor Category has been deleted Successfully');", true);
            else
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Vendor Category cannot be deleted, delete all vendors of this category');", true);

            bindfordeletevender();

        }

        protected void btnNewVendor_Click(object sender, EventArgs e)
        {
            NewVendorCategory objNewVendor = new NewVendorCategory();

            objNewVendor.VendorName = txtnewVendorCat.Text;
            string vendorCatId = VendorBLL.Instance.SaveNewVendorCategory(objNewVendor);
            objNewVendor.VendorId = vendorCatId;
            objNewVendor.ProductId = ddlProductCatgoryPopup.SelectedValue.ToString();
            objNewVendor.ProductName = ddlProductCatgoryPopup.SelectedItem.Text;
            bool res = VendorBLL.Instance.SaveNewVendorProduct(objNewVendor);
            if (res)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Data has been inserted Successfully');", true);
                BindVendorCatAfterAddNew();
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('There is some error');", true);
            }
        }
        public void BindVendorCatAfterAddNew()
        {
            if (ddlprdtCategory.SelectedValue.ToString() == "Select")
            {
                DataSet ds = new DataSet();
                ds = VendorBLL.Instance.fetchallvendorcategory();
                ddlVndrCategory.DataSource = ds;
                ddlVndrCategory.DataTextField = ds.Tables[0].Columns[1].ToString();
                ddlVndrCategory.DataValueField = ds.Tables[0].Columns[0].ToString();
                ddlVndrCategory.DataBind();
                ddlVndrCategory.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "Select"));
                BindVendorCatPopup();
                ddlVndrCategory.Items.Clear();
                ddlVendorCatPopup.Items.Clear();
            }
            else
            {
                BindVendorByProdCat(ddlprdtCategory.SelectedValue.ToString());
            }
        }
        protected void btnNewVendorSubCat_Click(object sender, EventArgs e)
        {
            VendorSubCategory objVendorSubCat = new VendorSubCategory();
            objVendorSubCat.VendorCatId = ddlVendorCatPopup.SelectedValue.ToString();
            objVendorSubCat.Name = txtVendorSubCat.Text;
            bool res = VendorBLL.Instance.SaveNewVendorSubCat(objVendorSubCat);
            if (res)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Data has been inserted Successfully');", true);
                BindvendorSubCatAfter();
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('There is some error');", true);
            }
        }

        public void BindvendorSubCatAfter()
        {
            if (ddlVndrCategory.SelectedValue.ToString() == "Select")
            {
                DataSet ds = new DataSet();
                ds = VendorBLL.Instance.GetVendorSubCategory();
                ddlVendorSubCategory.DataSource = ds;
                ddlVendorSubCategory.DataTextField = "VendorSubCategoryName";
                ddlVendorSubCategory.DataValueField = "VendorSubCategoryId";
                ddlVendorSubCategory.DataBind();
                ddlVendorSubCategory.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "Select"));
            }
            else
            {
                BindVendorSubCatByVendorCat(ddlVndrCategory.SelectedValue.ToString());
            }
        }
        protected void GetAllVendorSubCat()
        {
            DataSet ds = new DataSet();
            ds = VendorBLL.Instance.GetVendorSubCategory();
            ddlvendorsubcatpopup.DataSource = ds;
            ddlvendorsubcatpopup.DataTextField = "VendorSubCategoryName";
            ddlvendorsubcatpopup.DataValueField = "VendorSubCategoryId";
            ddlvendorsubcatpopup.DataBind();
            ddlvendorsubcatpopup.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "Select"));
        }
        protected void lnkdeletevendersubcategory_Click(object sender, EventArgs e)
        {
            GetAllVendorSubCat();
        }

        protected void btndeleteVendorSubCat_Click(object sender, EventArgs e)
        {
            VendorSubCategory objVendorSubCat = new VendorSubCategory();

            objVendorSubCat.Id = ddlvendorsubcatpopup.SelectedValue.ToString();

            bool res = VendorBLL.Instance.DeleteVendorSubCat(objVendorSubCat);
            if (res)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Data has been Deleted Successfully');", true);

            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('There is some error');", true);
            }

        }

        protected void btnaddvendors_Click(object sender, EventArgs e)
        {
            Response.Redirect("Vendors.aspx");
        }

        //protected void Menu1_MenuItemClick(object sender, MenuEventArgs e)
        //{
        //    int index = Int32.Parse(e.Item.Value);
        //    MultiView1.ActiveViewIndex = index;
        //}

        protected void lnkaddvendorquotes_Click(object sender, EventArgs e)
        {
            LinkButton lnkquotes = sender as LinkButton;

            GridViewRow gr = (GridViewRow)lnkquotes.Parent.Parent;
            LinkButton lblcustid = (LinkButton)gr.FindControl("lnkcustomerid");
            LinkButton lnksoldjobid = (LinkButton)gr.FindControl("lnksoldjobid");
            HiddenField hdnproductid = (HiddenField)gr.FindControl("hdnproductid");
            LinkButton lnkmateriallist = (LinkButton)gr.FindControl("lnkmateriallist");
            Label lblProductType = (Label)gr.FindControl("lblProductType");
            string soldjobId = lnkmateriallist.Text.Trim().Split('M')[0].Trim();
            ViewState[ViewStateKey.Key.CustomerId.ToString()] = lblcustid.Text;
            int custId = Convert.ToInt16(ViewState[ViewStateKey.Key.CustomerId.ToString()].ToString().Substring(1));
            Session[SessionKey.Key.JobId.ToString()] = soldjobId;
            // ViewState[ViewStateKey.Key.SoldJobNo.ToString()] = soldjobId;
            //ViewState[ViewStateKey.Key.ProductId.ToString()] = hdnproductid.Value;
            string emailStatus = CustomBLL.Instance.GetEmailStatusOfCustomMaterialList(soldjobId);//, Convert.ToInt16(lblProductType.Text),  Convert.ToInt16(hdnproductid.Value));
            if (emailStatus == JGConstant.EMAIL_STATUS_VENDORCATEGORIES)
            {
                ViewState[ViewStateKey.Key.ProductTypeId.ToString()] = Convert.ToInt16(lblProductType.Text);
                Response.Redirect("~/Sr_App/AttachQuotes.aspx");
                // Response.Redirect("~/Sr_App/AttachQuotes.aspx?CustomerId=" + custId + "&ProductId=" + hdnproductid.Value + "&ProductTypeId=" + Convert.ToInt16(lblProductType.Text));
            }
            else if (emailStatus == JGConstant.EMAIL_STATUS_VENDOR)
            {
                ViewState[ViewStateKey.Key.ProductTypeId.ToString()] = Convert.ToInt16(lblProductType.Text);
                Response.Redirect("~/Sr_App/AttachQuotes.aspx?EmailStatus=" + emailStatus);
                // Response.Redirect("~/Sr_App/AttachQuotes.aspx?CustomerId=" + custId + "&ProductId=" + hdnproductid.Value + "&ProductTypeId=" + Convert.ToInt16(lblProductType.Text) + "&EmailStatus=" + emailStatus);
            }
            //else if (lblProductType.Text == JGConstant.PRODUCT_SHUTTER)
            //{
            //    ViewState[ViewStateKey.Key.ProductTypeId.ToString()] = (int)JGConstant.ProductType.shutter;

            //    Response.Redirect("~/Sr_App/AttachQuotes.aspx?CustomerId=" + custId + "&ProductId=" + hdnproductid.Value + "&ProductTypeId=" + (int)JGConstant.ProductType.shutter);
            //}
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First send email to all vendor categories');", true);
            }

        }

        //protected void grdAttachQuotes_RowCommand(object sender, GridViewCommandEventArgs e)
        //{
        //    if (e.CommandName.ToLower() == "viewfile")
        //    {
        //        string file = Convert.ToString(e.CommandArgument);
        //        string domainName = Request.Url.GetLeftPart(UriPartial.Authority);

        //        ClientScript.RegisterClientScriptBlock(Page.GetType(), "Myscript", "<script language='javascript'>window.open('" + domainName + "/CustomerDocs/VendorQuotes/" + file + "', null, 'width=487px,height=455px,center=1,resize=0,scrolling=1,location=no');</script>");
        //    }
        //    else if (e.CommandName.ToLower() == "removefile")
        //    {
        //        string file = Convert.ToString(e.CommandArgument);
        //        CustomBLL.Instance.RemoveAttachedQuote(file);
        //        binddata();
        //    }
        //}
        protected void lnkcustomerid_Click(object sender, EventArgs e)
        {
            LinkButton lnkcustid = sender as LinkButton;
            Response.Redirect("~/Sr_App/Customer_Profile.aspx?CustomerId=" + lnkcustid.Text.Substring(1));
        }

        protected void lnksoldjobid_Click(object sender, EventArgs e)
        {
            LinkButton lnksoldjobid = sender as LinkButton;
            GridViewRow gr = (GridViewRow)lnksoldjobid.Parent.Parent;
            //Newly Added .....
            HiddenField hdnProductTypeId = (HiddenField)gr.FindControl("hdnProductTypeId");

            // Label lblProductType = (Label)gr.FindControl("lblProductType");
            LinkButton lnkcustomerid = (LinkButton)gr.FindControl("lnkcustomerid");
            LinkButton lnkmateriallist = (LinkButton)gr.FindControl("lnkmateriallist");
            HiddenField hdnproductid = (HiddenField)gr.FindControl("hdnproductid");
            int customerId = Convert.ToInt16(lnkcustomerid.Text.Trim().Substring(1));
            DataSet ds = existing_customerBLL.Instance.GetExistingCustomerDetailById(customerId);
            DataRow dr = ds.Tables[0].Rows[0];
            Session[SessionKey.Key.CustomerName.ToString()] = dr["CustomerName"].ToString();
            string soldjobId = lnkmateriallist.Text.Trim().Split('M')[0].Trim();
            // DataSet dssoldJobs = new_customerBLL.Instance.GetProductAndEstimateIdOfSoldJob(soldjobId);
            int productId = Convert.ToInt16(hdnproductid.Value); //Convert.ToInt16(dssoldJobs.Tables[0].Rows[0]["EstimateId"].ToString());
            //int productId = Convert.ToInt16(lnksoldjobid.Text.Trim().Substring(2));


            //Need to check where to redirect...and find JGConstant.ONE value ....
            //if (lblProductType.Text != JGConstant.ONE.ToString())
            //{
            //    Response.Redirect("Custom.aspx?ProductTypeId=" + Convert.ToInt16(lblProductType.Text) + "&ProductId=" + productId + "&CustomerId=" + customerId);

            //}
            if (hdnProductTypeId.Value == JGConstant.ONE.ToString())
            {
                Response.Redirect("Custom.aspx?ProductTypeId=" + Convert.ToInt16(hdnProductTypeId.Value) + "&ProductId=" + productId + "&CustomerId=" + customerId);

            }

        }
        protected void lnkmateriallist_Click(object sender, EventArgs e)
        {
            LinkButton lnkmateriallist = sender as LinkButton;

            GridViewRow gr = (GridViewRow)lnkmateriallist.Parent.Parent;

            LinkButton lnksoldjobid = (LinkButton)gr.FindControl("lnksoldjobid");
            Label lblProductType = (Label)gr.FindControl("lblProductType");
            string soldjobId = lnkmateriallist.Text.Trim().Split('M')[0].Trim();
            //int productId = Convert.ToInt16(lnksoldjobid.Text.Trim().Substring(2));
            DataSet dssoldJobs = new_customerBLL.Instance.GetProductAndEstimateIdOfSoldJob(soldjobId);
            int productId = Convert.ToInt16(dssoldJobs.Tables[0].Rows[0]["EstimateId"].ToString());

            Session[SessionKey.Key.JobId.ToString()] = soldjobId;
            setPermissions();
            bindMaterialList();
            SetButtonText();
            bind();




            /////////////////////Only required to transfer to Material List/////////
            LinkButton lnkcustomerid = (LinkButton)gr.FindControl("lnkcustomerid");
            HiddenField hdnProductTypeId = (HiddenField)gr.FindControl("hdnProductTypeId");
            HiddenField hdnproductid = (HiddenField)gr.FindControl("hdnproductid");
            int customerId = Convert.ToInt16(lnkcustomerid.Text.Trim().Substring(1));
            int productIdNew = Convert.ToInt16(Convert.ToString(hdnproductid.Value));
            int ProductTypeId = Convert.ToInt16(Convert.ToString(hdnProductTypeId.Value));
            /////////////////////////////////////////////////////////////////////////////
            #region For Local
            // Response.Redirect("/Sr_App/Custom_MaterialList.aspx?" + QueryStringKey.Key.ProductId.ToString() + "=" + productId + "&" + QueryStringKey.Key.CustomerId.ToString() + "=" + customerId + "&" + QueryStringKey.Key.ProductTypeId.ToString() + "=" + (int)JGConstant.ProductType.custom + "&" + QueryStringKey.Key.SoldJobId.ToString() + "=" + soldjobId);
            #endregion

            #region For Live Server
            Response.Redirect("~/Sr_App/Custom_MaterialList.aspx?" + QueryStringKey.Key.ProductId.ToString() + "=" + productId + "&" + QueryStringKey.Key.CustomerId.ToString() + "=" + customerId + "&" + QueryStringKey.Key.ProductTypeId.ToString() + "=" + (int)JGConstant.ProductType.custom + "&" + QueryStringKey.Key.SoldJobId.ToString() + "=" + soldjobId);
            #endregion
            //lnkVendorCategory.ForeColor = System.Drawing.Color.DarkGray;
            //lnkVendorCategory.Enabled = false;
            //lnkVendor.Enabled = true;
            //lnkVendor.ForeColor = System.Drawing.Color.Blue;

            //pnlMaterialList.Visible = true;
            //Response.Redirect("/Sr_App/Custom_MaterialList.aspx");
            //Response.Redirect("/Sr_App/Custom_MaterialList.aspx?" + QueryStringKey.Key.ProductId.ToString() + "=" + productId + "&" + QueryStringKey.Key.CustomerId.ToString() + "=" + customerId + "&" + QueryStringKey.Key.ProductTypeId.ToString() + "=" + Convert.ToInt16(lblProductType.Text.ToString()));
            // }
            // else
            // {
            //    ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('No Quotes are attached. Please attach quotes.');", true);
            // }
            // }
            // else
            // {
            //Response.Redirect("/Sr_App/Custom_MaterialList.aspx?" + QueryStringKey.Key.ProductId.ToString() + "=" + productId + "&" + QueryStringKey.Key.CustomerId.ToString() + "=" + customerId + "&" + QueryStringKey.Key.ProductTypeId.ToString() + "=" + (int)JGConstant.ProductType.custom);
            //}

        }
        // }

        protected void bind()
        {
            DataSet ds = new DataSet();
            ds = AdminBLL.Instance.FetchContractTemplate(0);
            if (ds != null)
            {
                //HeaderEditor.Content = ds.Tables[0].Rows[0][0].ToString();
                //lblMaterials.Text = ds.Tables[0].Rows[0][1].ToString();
                //FooterEditor.Content = ds.Tables[0].Rows[0][2].ToString();
            }
        }


        private void bindMaterialList()
        {
            DataSet ds = CustomBLL.Instance.GetCustom_MaterialList(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//,productTypeId,estimateId);
            List<CustomMaterialList> cmList = new List<CustomMaterialList>();
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                {
                    DataRow dr = ds.Tables[0].Rows[j];
                    CustomMaterialList cm = new CustomMaterialList();
                    cm.Id = Convert.ToInt16(dr["Id"]);
                    cm.MaterialList = dr["MaterialList"].ToString();
                    cm.VendorCategoryId = Convert.ToInt16(dr["VendorCategoryId"]);
                    cm.VendorCategoryName = dr["VendorCategoryNm"].ToString();
                    if (dr["VendorId"].ToString() != "")
                        cm.VendorId = Convert.ToInt16(dr["VendorId"]);
                    cm.VendorName = dr["VendorName"].ToString();
                    if (dr["Amount"].ToString() != "")
                        cm.Amount = Convert.ToDecimal(dr["Amount"]);
                    cm.DocName = dr["DocName"].ToString();
                    cm.TempName = dr["TempName"].ToString();
                    cm.IsForemanPermission = dr["IsForemanPermission"].ToString();
                    cm.IsSrSalemanPermissionF = dr["IsSrSalemanPermissionF"].ToString();
                    cm.IsAdminPermission = dr["IsAdminPermission"].ToString();
                    cm.IsSrSalemanPermissionA = dr["IsSrSalemanPermissionA"].ToString();
                    cm.Status = JGConstant.CustomMaterialListStatus.Unchanged;
                    cmList.Add(cm);
                }

                ViewState["CustomMaterialList"] = cmList;

                BindCustomMaterialList(cmList);
            }
            else
            {
                //List<CustomMaterialList> cmList1 = new List<CustomMaterialList>();

                //CustomMaterialList cm1 = new CustomMaterialList();
                //cm1.Id = 0;
                //cm1.MaterialList = "";
                //cm1.VendorCategoryId = 0;
                //cm1.VendorCategoryName = "";
                //cm1.VendorId = 0;
                //cm1.VendorName = "";
                //cm1.Amount = 0;
                //cm1.DocName = "";
                //cm1.TempName = "";
                //cm1.IsForemanPermission = "";
                //cm1.IsSrSalemanPermissionF = "";
                //cm1.IsAdminPermission = "";
                //cm1.IsSrSalemanPermissionA = "";
                //cm1.Status = JGConstant.CustomMaterialListStatus.Unchanged;
                //cmList1.Add(cm1);
                List<CustomMaterialList> cmList1 = BindEmptyRowToMaterialList();

                ViewState["CustomMaterialList"] = cmList1;
                BindCustomMaterialList(cmList1);
            }

        }

        private List<CustomMaterialList> GetMaterialListFromGrid()
        {
            List<CustomMaterialList> itemList = new List<CustomMaterialList>();

            //for (int i = 0; i < grdcustom_material_list.Rows.Count; i++)
            //{
            //    CustomMaterialList cm = new CustomMaterialList();
            //    HiddenField hdnEmailStatus = (HiddenField)grdcustom_material_list.Rows[i].FindControl("hdnEmailStatus");
            //    if (hdnEmailStatus.Value.ToString() != "")
            //        cm.EmailStatus = hdnEmailStatus.Value;

            //    HiddenField hdnForemanPermission = (HiddenField)grdcustom_material_list.Rows[i].FindControl("hdnForemanPermission");
            //    if (hdnForemanPermission.Value.ToString() != "")
            //        cm.IsForemanPermission = hdnForemanPermission.Value;

            //    HiddenField hdnSrSalesmanPermissionF = (HiddenField)grdcustom_material_list.Rows[i].FindControl("hdnSrSalesmanPermissionF");
            //    if (hdnSrSalesmanPermissionF.Value.ToString() != "")
            //        cm.IsSrSalemanPermissionF = hdnSrSalesmanPermissionF.Value;

            //    HiddenField hdnAdminPermission = (HiddenField)grdcustom_material_list.Rows[i].FindControl("hdnAdminPermission");
            //    if (hdnAdminPermission.Value.ToString() != "")
            //        cm.IsAdminPermission = hdnAdminPermission.Value;

            //    HiddenField hdnSrSalesmanPermissionA = (HiddenField)grdcustom_material_list.Rows[i].FindControl("hdnSrSalesmanPermissionA");
            //    if (hdnSrSalesmanPermissionA.Value.ToString() != "")
            //        cm.IsSrSalemanPermissionA = hdnSrSalesmanPermissionA.Value;

            //    HiddenField hdnMaterialListId = (HiddenField)grdcustom_material_list.Rows[i].FindControl("hdnMaterialListId");
            //    if (hdnMaterialListId.Value.ToString() != "")
            //        cm.Id = Convert.ToInt16(hdnMaterialListId.Value);
            //    TextBox txtMateriallist = (TextBox)grdcustom_material_list.Rows[i].FindControl("txtMateriallist");
            //    cm.MaterialList = txtMateriallist.Text;

            //    DropDownList ddlVendorCategory = (DropDownList)grdcustom_material_list.Rows[i].FindControl("ddlVendorCategory");
            //    if (ddlVendorCategory.SelectedIndex != -1)
            //    {
            //        cm.VendorCategoryId = Convert.ToInt16(ddlVendorCategory.SelectedValue);
            //        cm.VendorCategoryName = ddlVendorCategory.SelectedItem.Text;
            //    }
            //    DropDownList ddlVendorName = (DropDownList)grdcustom_material_list.Rows[i].FindControl("ddlVendorName");
            //    if (ddlVendorName.SelectedIndex != -1)
            //    {
            //        cm.VendorId = Convert.ToInt16(ddlVendorName.SelectedValue);
            //        cm.VendorName = ddlVendorName.SelectedItem.Text;
            //    }

            //    LinkButton lnkQuote = (LinkButton)grdcustom_material_list.Rows[i].FindControl("lnkQuote");
            //    if (lnkQuote.Text != "")
            //    {
            //        cm.DocName = lnkQuote.Text;
            //        cm.TempName = lnkQuote.CommandArgument;
            //    }
            //    TextBox txtAmount = (TextBox)grdcustom_material_list.Rows[i].FindControl("txtAmount");
            //    if (txtAmount.Text != "")
            //        cm.Amount = Convert.ToDecimal(txtAmount.Text);

            //    itemList.Add(cm);
            //}
            return itemList;
        }

        private List<CustomMaterialList> GetMaterialListFromViewState()
        {
            List<CustomMaterialList> itemList = null;

            if (ViewState["CustomMaterialList"] == null)
            {
                itemList = new List<CustomMaterialList>();
            }
            else
            {
                itemList = ViewState["CustomMaterialList"] as List<CustomMaterialList>;
            }
            return itemList;
        }

        private List<CustomMaterialList> BindEmptyRowToMaterialList()
        {
            List<CustomMaterialList> cmList1 = new List<CustomMaterialList>();
            cmList1 = GetMaterialListFromGrid();
            CustomMaterialList cm1 = new CustomMaterialList();
            cm1.Id = 0;
            cm1.MaterialList = "";
            cm1.VendorCategoryId = 0;
            cm1.VendorCategoryName = "";
            cm1.VendorId = 0;
            cm1.VendorName = "";
            cm1.Amount = 0;
            cm1.DocName = "";
            cm1.TempName = "";
            cm1.IsForemanPermission = "";
            cm1.IsSrSalemanPermissionF = "";
            cm1.IsAdminPermission = "";
            cm1.IsSrSalemanPermissionA = "";
            cm1.Status = JGConstant.CustomMaterialListStatus.Unchanged;
            cmList1.Add(cm1);

            return cmList1;
        }

        protected void BindCustomMaterialList(List<CustomMaterialList> itemList = null)
        {
            if (itemList == null)
            {
                itemList = GetMaterialListFromViewState();
            }
            List<CustomMaterialList> cmList = itemList.Where(c => c.Status != JGConstant.CustomMaterialListStatus.Deleted).ToList();
            //grdcustom_material_list.DataSource = cmList;
            //grdcustom_material_list.DataBind();
            int j = 0;
            string emailStatus = CustomBLL.Instance.GetEmailStatusOfCustomMaterialList(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//, productTypeId, estimateId);

            //foreach (GridViewRow r in grdcustom_material_list.Rows)
            //{
            //    CustomMaterialList cml = cmList[j];
            //    if (cml.Status != JGConstant.CustomMaterialListStatus.Deleted)
            //    {
            //        Label lblsrno = (Label)r.FindControl("lblsrno");

            //        DropDownList ddlVendorCategory1 = (DropDownList)r.FindControl("ddlVendorCategory");
            //        DropDownList ddlVendorName = (DropDownList)r.FindControl("ddlVendorName");
            //        TextBox txtAmount = (TextBox)r.FindControl("txtAmount");
            //        LinkButton lnkQuote = (LinkButton)r.FindControl("lnkQuote");
            //        HiddenField hdnMaterialListId = (HiddenField)r.FindControl("hdnMaterialListId");
            //        HiddenField hdnEmailStatus = (HiddenField)r.FindControl("hdnEmailStatus");
            //        HiddenField hdnForemanPermission = (HiddenField)r.FindControl("hdnForemanPermission");
            //        HiddenField hdnSrSalesmanPermissionF = (HiddenField)r.FindControl("hdnSrSalesmanPermissionF");
            //        HiddenField hdnAdminPermission = (HiddenField)r.FindControl("hdnAdminPermission");
            //        HiddenField hdnSrSalesmanPermissionA = (HiddenField)r.FindControl("hdnSrSalesmanPermissionA");

            //        lblsrno.Text = (j + 1).ToString();
            //        if (cml.VendorCategoryId.ToString() != "")
            //        {
            //            ddlVendorCategory1.SelectedValue = cml.VendorCategoryId.ToString();
            //        }
            //        else
            //        {
            //            ddlVendorCategory1.SelectedIndex = -1;
            //        }
            //        if (cml.VendorId.ToString() != "")
            //        {
            //            int selectedCategoryID = Convert.ToInt16(ddlVendorCategory1.SelectedItem.Value);
            //            DataSet ds = GetVendorNames(selectedCategoryID);
            //            ddlVendorName.DataSource = ds;
            //            ddlVendorName.SelectedIndex = -1;
            //            ddlVendorName.DataTextField = "VendorName";
            //            ddlVendorName.DataValueField = "VendorId";
            //            ddlVendorName.DataBind();
            //            ddlVendorName.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "0"));

            //            ddlVendorName.SelectedValue = cml.VendorId.ToString();

            //        }
            //        else
            //        {
            //            ddlVendorName.SelectedIndex = -1;
            //        }

            //        if (cml.Amount.ToString() != "")
            //        {
            //            txtAmount.Text = cml.Amount.ToString();

            //        }
            //        else
            //        {
            //            txtAmount.Text = string.Empty;
            //        }
            //        if (Convert.ToInt16(cml.Id.ToString()) != 0)
            //        {
            //            hdnMaterialListId.Value = cml.Id.ToString();
            //        }
            //        else
            //        {
            //            hdnMaterialListId.Value = "0";
            //        }
            //        if (cml.IsForemanPermission != "")
            //        {
            //            hdnForemanPermission.Value = cml.IsForemanPermission;
            //        }
            //        else
            //        {
            //            hdnForemanPermission.Value = "";
            //        }
            //        if (cml.IsSrSalemanPermissionF != "")
            //        {
            //            hdnSrSalesmanPermissionF.Value = cml.IsSrSalemanPermissionF;
            //        }
            //        else
            //        {
            //            hdnSrSalesmanPermissionF.Value = "";
            //        }
            //        if (cml.IsAdminPermission != "")
            //        {
            //            hdnAdminPermission.Value = cml.IsAdminPermission;
            //        }
            //        else
            //        {
            //            hdnAdminPermission.Value = "";
            //        }
            //        if (cml.IsSrSalemanPermissionA != "")
            //        {
            //            hdnSrSalesmanPermissionA.Value = cml.IsSrSalemanPermissionA;
            //        }
            //        else
            //        {
            //            hdnSrSalesmanPermissionA.Value = "";
            //        }
            //        if (cml.EmailStatus != "")
            //        {
            //            hdnEmailStatus.Value = cml.EmailStatus;
            //        }
            //        else
            //        {
            //            hdnEmailStatus.Value = "";
            //        }
            //    }
            //    if (emailStatus == JGConstant.EMAIL_STATUS_VENDORCATEGORIES)
            //    {
            //        EnableVendorNameAndAmount();
            //    }
            //    j++;
            //}
        }

        private void EnableVendorNameAndAmount()
        {
            //foreach (GridViewRow r in grdcustom_material_list.Rows)
            //{
            //    DropDownList ddlVendorName = (DropDownList)r.FindControl("ddlVendorName");
            //    ddlVendorName.Enabled = true;
            //    TextBox txtAmount = (TextBox)r.FindControl("txtAmount");
            //    txtAmount.Enabled = true;
            //}
        }

        public DataSet GetVendorNames(int vendorcategoryId)
        {
            DataSet ds = new DataSet();
            ds = VendorBLL.Instance.fetchVendorNamesByVendorCategory(vendorcategoryId);
            return ds;
        }

        public void showVendorCategoriesPermissions()
        {
            //lnkForemanPermission.Visible = true;
            //lnkSrSalesmanPermissionF.Visible = true;
            //lnkAdminPermission.Visible = false;
            //lnkSrSalesmanPermissionA.Visible = false;
            setPermissions();
        }
        public void SetButtonText()
        {
            string EmailStatus = CustomBLL.Instance.GetEmailStatusOfCustomMaterialList(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//, productTypeId, estimateId);
            int result = CustomBLL.Instance.WhetherCustomMaterialListExists(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//, productTypeId, estimateId);
            if (result == 0) //if list doesn't exists
            {
                //btnSendMail.Text = "Save";
                showVendorCategoriesPermissions();
            }
            else  //if list exists
            {
                if (EmailStatus == JGConstant.EMAIL_STATUS_NONE || EmailStatus == string.Empty)       //if no email was sent
                {
                    int permissionStatusCategories = CustomBLL.Instance.CheckPermissionsForCategories(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//, productTypeId, estimateId);
                    if (permissionStatusCategories == 0)        //if no permissions were granted for categories
                    {
                        //btnSendMail.Text = "Save";
                    }
                    else                //if permissions were granted for categories
                    {
                        //btnSendMail.Text = "Send Mail To Vendor Category(s)";
                        //grdcustom_material_list.Columns[6].Visible = false;
                    }
                    showVendorCategoriesPermissions();
                }

                else if (EmailStatus == JGConstant.EMAIL_STATUS_VENDOR)    //if both mails are sent
                {
                    setControlsAfterSendingBothMails();
                    showVendorPermissions();
                }
                else        //if mails were sent to categories
                {
                    int permissionStatus = CustomBLL.Instance.CheckPermissionsForVendors(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//, productTypeId, estimateId);
                    if (permissionStatus == 0)  //if permissions were not granted for vendors
                    {
                        //btnSendMail.Text = "Save";
                        showVendorPermissions();
                        EnableVendorNameAndAmount();
                        //grdcustom_material_list.Columns[6].Visible = true;
                    }
                    else         //if permissions were granted for vendors
                    {
                        //btnSendMail.Text = "Send Mail To Vendor(s)";
                        setControlsForVendorsAfterSave();
                        showVendorPermissions();
                        EnableVendorNameAndAmount();
                    }
                }
            }
        }

        protected void setControlsForVendorsAfterSave()
        {
            //foreach (GridViewRow gr in grdcustom_material_list.Rows)
            //{
            //    TextBox txtMateriallist = (TextBox)gr.FindControl("txtMateriallist");
            //    txtMateriallist.Enabled = false;

            //    TextBox txtAmount = (TextBox)gr.FindControl("txtAmount");
            //    txtAmount.Enabled = false;
            //    DropDownList ddlVendorCategory = (DropDownList)gr.FindControl("ddlVendorCategory");
            //    ddlVendorCategory.Enabled = false;
            //    int selectedCategoryID = Convert.ToInt16(ddlVendorCategory.SelectedItem.Value);

            //    DropDownList ddlVendorName = (DropDownList)gr.FindControl("ddlVendorName");
            //    ddlVendorName.Enabled = false;
            //}
            //grdcustom_material_list.Columns[6].Visible = false;
        }
        [WebMethod]
        public static string Exists(string value)
        {
            if (value == AdminBLL.Instance.GetAdminCode())
            {
                return "True";
            }
            else
            {
                return "false";
            }
        }
        public void showVendorPermissions()
        {
            //lnkForemanPermission.Visible = false;
            //lnkSrSalesmanPermissionF.Visible = false;
            //lnkAdminPermission.Visible = true;
            //lnkSrSalesmanPermissionA.Visible = true;

            setPermissions();
        }
        protected void setControlsForVendorCategoriesAfterSave()
        {
            //foreach (GridViewRow gr in grdcustom_material_list.Rows)
            //{
            //    TextBox txtMateriallist = (TextBox)gr.FindControl("txtMateriallist");
            //    txtMateriallist.Enabled = false;

            //    TextBox txtAmount = (TextBox)gr.FindControl("txtAmount");
            //    txtAmount.Enabled = false;
            //    DropDownList ddlVendorCategory = (DropDownList)gr.FindControl("ddlVendorCategory");
            //    ddlVendorCategory.Enabled = false;

            //    DropDownList ddlVendorName = (DropDownList)gr.FindControl("ddlVendorName");
            //    ddlVendorName.Enabled = false;
            //}
        }
        protected void setControlsAfterSendingBothMails()
        {
            //btnSendMail.Visible = false;
            //grdcustom_material_list.Columns[6].Visible = false;
            //lnkAdminPermission.Enabled = false;
            //lnkForemanPermission.Enabled = false;
            //lnkSrSalesmanPermissionA.Enabled = false;
            //lnkSrSalesmanPermissionF.Enabled = false;
            //foreach (GridViewRow r in grdcustom_material_list.Rows)
            //{
            //    TextBox txtMateriallist = (TextBox)r.FindControl("txtMateriallist");
            //    txtMateriallist.Enabled = false;
            //    DropDownList ddlVendorCategory = (DropDownList)r.FindControl("ddlVendorCategory");
            //    ddlVendorCategory.Enabled = false;
            //    DropDownList ddlVendorName = (DropDownList)r.FindControl("ddlVendorName");
            //    ddlVendorName.Enabled = false;
            //    TextBox txtAmount = (TextBox)r.FindControl("txtAmount");
            //    txtAmount.Enabled = false;
            //    LinkButton lnkQuote = (LinkButton)r.FindControl("lnkQuote");
            //    lnkQuote.Enabled = true;
            //}
        }
        public void setPermissions()
        {
            DataSet ds = CustomBLL.Instance.GetAllPermissionOfCustomMaterialList(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//, productTypeId, estimateId);
            if (ds.Tables[0].Rows.Count > 0)
            {
                //if (lnkForemanPermission.Visible == true)
                //{
                //    if (Convert.ToChar(ds.Tables[0].Rows[0]["IsForemanPermission"].ToString().Trim()) == JGConstant.PERMISSION_STATUS_GRANTED)
                //    {
                //        lnkForemanPermission.Enabled = false;
                //        lnkForemanPermission.ForeColor = System.Drawing.Color.DarkGray;
                //        popupForeman_permission.TargetControlID = "hdnForeman";
                //    }
                //    if (Convert.ToChar(ds.Tables[0].Rows[0]["IsSrSalemanPermissionF"].ToString().Trim()) == JGConstant.PERMISSION_STATUS_GRANTED)
                //    {
                //        lnkSrSalesmanPermissionF.Enabled = false;
                //        lnkSrSalesmanPermissionF.ForeColor = System.Drawing.Color.DarkGray;
                //        popupSrSalesmanPermissionF.TargetControlID = "hdnSrF";
                //    }
                //}
                //if (lnkAdminPermission.Visible == true)
                //{
                //    if (Convert.ToChar(ds.Tables[0].Rows[0]["IsAdminPermission"].ToString().Trim()) == JGConstant.PERMISSION_STATUS_GRANTED)
                //    {
                //        lnkAdminPermission.Enabled = false;
                //        lnkAdminPermission.ForeColor = System.Drawing.Color.DarkGray;
                //        popupAdmin_permission.TargetControlID = "hdnAdmin";
                //    }
                //    if (Convert.ToChar(ds.Tables[0].Rows[0]["IsSrSalemanPermissionA"].ToString().Trim()) == JGConstant.PERMISSION_STATUS_GRANTED)
                //    {
                //        lnkSrSalesmanPermissionA.Enabled = false;
                //        lnkSrSalesmanPermissionA.ForeColor = System.Drawing.Color.DarkGray;
                //        popupSrSalesmanPermissionA.TargetControlID = "hdnSrA";
                //    }
                //}
            }
        }

        protected void grdsoldjobs_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            try
            {
                StringBuilder strerr = new StringBuilder();
                string Foreman = string.Empty;
                string Adm = string.Empty;
                string SLE1 = string.Empty;
                string SLE2 = string.Empty;
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    LinkButton lnkmateriallist = (LinkButton)e.Row.FindControl("lnkmateriallist");
                    LinkButton lnksoldjobid = (LinkButton)e.Row.FindControl("lnksoldjobid");
                    Label lblProductType = (Label)e.Row.FindControl("lblProductType");
                    Label lblStatus = (Label)e.Row.FindControl("lblStatus");
                    DropDownList ddlstatus = (DropDownList)e.Row.FindControl("ddlstatus");
                    LinkButton lnkcustomerid = (LinkButton)e.Row.FindControl("lnkcustomerid");
                    HiddenField hdnStatusId = (HiddenField)e.Row.FindControl("hdnStatusId");
                    HiddenField hdnJobSeqId = (HiddenField)e.Row.FindControl("hdnJobSeqId");
                    Label lblReason = (Label)e.Row.FindControl("lblReason");
                    Label lblADMPassword = (Label)e.Row.FindControl("lblADMPassword");
                    Label lblfrmPassword = (Label)e.Row.FindControl("lblfrmPassword");
                    Label lblSalePassword = (Label)e.Row.FindControl("lblSalePassword");
                    // GridView grdAttachQuotes = (GridView)e.Row.FindControl("grdAttachQuotes");

                    //if (lblProductType.Text == JGConstant.PRODUCT_CUSTOM)
                    //{
                    //    lnkmateriallist.Enabled = true;
                    //    lnksoldjobid.Enabled = true;
                    //}
                    //else
                    //{
                    //    lnkmateriallist.Enabled = false;
                    //    lnksoldjobid.Enabled = false;
                    //}
                    //if (lblStatus.Text.ToLower().Contains("ordered") || lblStatus.Text.ToLower().Contains("received “storage location?") || lblStatus.Text.ToLower().Contains("on standby @ vendor link to vendor profile") || lblStatus.Text.ToLower().Contains("being delivered to job site"))
                    //{
                    //    ddlstatus.Visible = true;
                    //    DataSet ds = new_customerBLL.Instance.FetchAllStatus();
                    //    string filter = " StatusId in(18,19,20)";
                    //    ds.Tables[0].DefaultView.RowFilter = filter;
                    //    ddlstatus.DataSource = ds.Tables[0].DefaultView;
                    //    ddlstatus.DataTextField = "StatusName";
                    //    ddlstatus.DataValueField = "StatusId";
                    //    ddlstatus.DataBind();
                    //    ddlstatus.Items.Insert(0, new System.Web.UI.WebControls.ListItem(JGConstant.SELECT, "0"));
                    //    if (Convert.ToInt16(hdnStatusId.Value) == JGConstant.STATUS_ID_RECEIVED_STORAGE_LOCATION || Convert.ToInt16(hdnStatusId.Value) == JGConstant.STATUS_ID_ON_STANDBY_VENDOR_LINK_TO_VENDOR_PROFILE || Convert.ToInt16(hdnStatusId.Value) == JGConstant.STATUS_ID_BEING_DELEIVERED_TO_JOBSITE)
                    //    {
                    //        ddlstatus.SelectedValue = hdnStatusId.Value;
                    //    }
                    //}

                    string SoldJobId = lnkcustomerid.Text + "-" + lnksoldjobid.Text;
                    strerr.Append(SoldJobId);
                    DataSet ds = new DataSet();
                    strerr.Append("Before call method");
                    ds = AdminBLL.Instance.GetMaterialList(SoldJobId);
                    strerr.Append("After call method");
                    if (ds.Tables.Count > 0)
                    {
                        strerr.Append("Into ds");
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            if (Convert.ToString(ds.Tables[0].Rows[0][0]) != "")
                            {
                                Foreman = Convert.ToString(ds.Tables[0].Rows[0][0]);
                            }
                            else
                            {
                                Foreman = "N";
                            }
                            if (Convert.ToString(ds.Tables[0].Rows[0][1]) != "")
                            {
                                SLE1 = Convert.ToString(ds.Tables[0].Rows[0][1]);
                            }
                            else
                            {
                                SLE1 = "N";
                            }
                            if (Convert.ToString(ds.Tables[0].Rows[0][2]) != "")
                            {
                                Adm = Convert.ToString(ds.Tables[0].Rows[0][2]);
                            }
                            else
                            {
                                Adm = "N";
                            }
                            if (Convert.ToString(ds.Tables[0].Rows[0][3]) != "")
                            {
                                SLE2 = Convert.ToString(ds.Tables[0].Rows[0][3]);
                            }
                            else
                            {
                                SLE2 = "N";
                            }
                        }
                        else
                        {
                            Foreman = "N";
                            SLE1 = "N";
                            Adm = "N";
                            SLE2 = "N";
                        }
                    }
                    if (lblStatus.Text == "Material Confirmation(1)")
                    {

                        lblADMPassword.Visible = false;
                        lblfrmPassword.Visible = true;
                        lblSalePassword.Visible = true;
                        if (Foreman == "G")
                        {
                            lblfrmPassword.ForeColor = Color.Green;
                        }
                        else
                        {
                            lblfrmPassword.ForeColor = Color.Black;
                        }
                        if (SLE1 == "G")
                        {
                            lblSalePassword.ForeColor = Color.Green;
                        }
                        else
                        {
                            lblSalePassword.ForeColor = Color.Black;
                        }
                        if (SLE1 == "G")
                        {
                            lblSalePassword.ForeColor = Color.Green;
                        }
                        else
                        {
                            lblSalePassword.ForeColor = Color.Black;
                        }
                        if (Adm == "G")
                        {
                            lblADMPassword.ForeColor = Color.Green;
                        }
                        else
                        {
                            lblADMPassword.ForeColor = Color.Black;
                        }
                    }
                    if (lblStatus.Text == "Procurring Quotes(2)")
                    {
                        lblADMPassword.Visible = true;
                        lblfrmPassword.Visible = false;
                        lblSalePassword.Visible = true;
                        if (Foreman == "G")
                        {
                            lblfrmPassword.ForeColor = Color.Green;
                        }
                        else
                        {
                            lblfrmPassword.ForeColor = Color.Black;
                        }
                        if (SLE1 == "G")
                        {
                            lblSalePassword.ForeColor = Color.Green;
                        }
                        else
                        {
                            lblSalePassword.ForeColor = Color.Black;
                        }
                        if (SLE1 == "G")
                        {
                            lblSalePassword.ForeColor = Color.Green;
                        }
                        else
                        {
                            lblSalePassword.ForeColor = Color.Black;
                        }
                        if (Adm == "G")
                        {
                            lblADMPassword.ForeColor = Color.Green;
                        }
                        else
                        {
                            lblADMPassword.ForeColor = Color.Black;
                        }
                    }
                    if (lblStatus.Text == "Ordered(3)")
                    {
                        lblADMPassword.Visible = true;
                        lblfrmPassword.Visible = false;
                        lblSalePassword.Visible = true;
                        lblADMPassword.ForeColor = Color.Green;
                        lblSalePassword.ForeColor = Color.Green;
                        if (Foreman == "G")
                        {
                            lblfrmPassword.ForeColor = Color.Green;
                        }
                        else
                        {
                            lblfrmPassword.ForeColor = Color.Black;
                        }
                        if (SLE1 == "G")
                        {
                            lblSalePassword.ForeColor = Color.Green;
                        }
                        else
                        {
                            lblSalePassword.ForeColor = Color.Black;
                        }
                        if (SLE1 == "G")
                        {
                            lblSalePassword.ForeColor = Color.Green;
                        }
                        else
                        {
                            lblSalePassword.ForeColor = Color.Black;
                        }
                        if (Adm == "G")
                        {
                            lblADMPassword.ForeColor = Color.Green;
                        }
                        else
                        {
                            lblADMPassword.ForeColor = Color.Black;
                        }
                    }
                    if (lblReason.Text != "")
                    {
                        //e.Row.BackColor = Color.Gray;
                        lblStatus.Text = "Disabled" + Environment.NewLine + lblReason.Text;
                    }
                    int customerId = Convert.ToInt16(lnkcustomerid.Text.Trim().Substring(1));
                    //int soldJobId = Convert.ToInt16(lnksoldjobid.Text.Trim().Substring(2));

                    string soldjobId = lnkmateriallist.Text.Trim().Split('M')[0].Trim();
                    //e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(grdsoldjobs, "Select$" + e.Row.RowIndex);
                    //e.Row.ToolTip = "Click to select this row.";
                    //bindgrid(customerId, soldjobId, grdAttachQuotes, lblProductType.Text);
                }
                else
                {
                    // strerr.Append("No ds found");
                }

                //lblerrornew.Text = Convert.ToString(strerr);
            }
            catch (Exception ex)
            {
                //lblerrornew.Text = ex.Message + ex.StackTrace;
            }
        }

        //public void bindgrid(int customerId, string soldJobId, GridView grdAttachQuotes,string productType)
        //public void bindgrid(int customerId, string soldJobId, string productType)
        //{
        //    DataSet ds = null;
        //    DataSet ds1 = new_customerBLL.Instance.GetProductAndEstimateIdOfSoldJob(soldJobId);
        //    int estimateId = Convert.ToInt16(ds1.Tables[0].Rows[0]["EstimateId"].ToString());
        //    int productTypeId=0;
        //    if(productType == JGConstant.PRODUCT_CUSTOM)
        //    {
        //        productTypeId =(int)JGConstant.ProductType.custom;
        //    }
        //    else
        //    {
        //        productTypeId =(int)JGConstant.ProductType.shutter ;
        //    }

        //    ds = CustomBLL.Instance.GetAllAttachedQuotes(customerId, estimateId, productTypeId);
        //    //grdAttachQuotes.DataSource = ds;
        //    //grdAttachQuotes.DataBind();
        //}
        //protected void grdAttachQuotes_RowDataBound(object sender, GridViewRowEventArgs e)
        //{
        //    if (e.Row.RowType == DataControlRowType.DataRow)
        //    {
        //        LinkButton lnkQuote = (LinkButton)e.Row.FindControl("lnkQuote");
        //    }
        //}

        protected void drpVendorName_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList drpVendorName = sender as DropDownList;

            GridViewRow gr = (GridViewRow)drpVendorName.Parent.Parent;

            DataSet dsVendorDetails = VendorBLL.Instance.fetchVendorDetailsByVendorId(Convert.ToInt16(drpVendorName.SelectedValue));
            Label lblContactPerson = (Label)gr.FindControl("lblContactPerson");
            lblContactPerson.Text = dsVendorDetails.Tables[0].Rows[0]["ContactPerson"].ToString();
            Label lblContactNumber = (Label)gr.FindControl("lblContactNumber");
            lblContactNumber.Text = dsVendorDetails.Tables[0].Rows[0]["ContactNumber"].ToString();
            Label lblFax = (Label)gr.FindControl("lblFax");
            lblFax.Text = dsVendorDetails.Tables[0].Rows[0]["Fax"].ToString();
            Label lblEmail = (Label)gr.FindControl("lblEmail");
            lblEmail.Text = dsVendorDetails.Tables[0].Rows[0]["Email"].ToString();
        }

        protected void saveCustom_MaterialList(List<CustomMaterialList> cmList)
        {
            bool result = false;
            CustomBLL.Instance.DeleteCustomMaterialList(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//, productTypeId, estimateId);
            foreach (CustomMaterialList cm in cmList)
            {

                result = CustomBLL.Instance.AddCustomMaterialList(cm, Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//,productTypeId,estimateId);
            }

            ViewState["CustomMaterialList"] = cmList;
        }

        protected void UpdateEmailStatus(string status)
        {
            List<CustomMaterialList> cmList = GetMaterialListFromGrid();
            foreach (CustomMaterialList cm in cmList)
            {
                cm.EmailStatus = status;
            }
            ViewState["CustomMaterialList"] = cmList;
        }

        //protected void btnSendMail_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        string status = CustomBLL.Instance.GetEmailStatusOfCustomMaterialList(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//, productTypeId, estimateId);
        //        List<CustomMaterialList> cmList = new List<CustomMaterialList>();
        //        foreach (GridViewRow r in grdcustom_material_list.Rows)
        //        {
        //            CustomMaterialList cm = new CustomMaterialList();
        //            DropDownList ddlVendorCategory = (DropDownList)r.FindControl("ddlVendorCategory");
        //            cm.VendorCategoryId = Convert.ToInt16(ddlVendorCategory.SelectedValue);
        //            TextBox txtMateriallist = (TextBox)r.FindControl("txtMateriallist");
        //            HiddenField hdnMaterialListId = (HiddenField)r.FindControl("hdnMaterialListId");
        //            HiddenField hdnEmailStatus = (HiddenField)r.FindControl("hdnEmailStatus");
        //            HiddenField hdnForemanPermission = (HiddenField)r.FindControl("hdnForemanPermission");
        //            HiddenField hdnSrSalesmanPermissionF = (HiddenField)r.FindControl("hdnSrSalesmanPermissionF");
        //            HiddenField hdnAdminPermission = (HiddenField)r.FindControl("hdnAdminPermission");
        //            HiddenField hdnSrSalesmanPermissionA = (HiddenField)r.FindControl("hdnSrSalesmanPermissionA");

        //            if (txtMateriallist.Text == "")
        //            {
        //                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Please fill Material List(s).');", true);
        //            }
        //            else
        //            {
        //                cm.MaterialList = txtMateriallist.Text;
        //            }

        //            if (hdnMaterialListId.Value != "")
        //            {
        //                cm.Id = Convert.ToInt16(hdnMaterialListId.Value);
        //            }
        //            else
        //            {
        //                cm.Id = 0;
        //            }
        //            DropDownList ddlVendorName = (DropDownList)r.FindControl("ddlVendorName");
        //            TextBox txtAmount = (TextBox)r.FindControl("txtAmount");

        //            if (status == "C") //mail was already sent to vendor categories
        //            {
        //                if (ddlVendorName.SelectedItem.Text == "Select")
        //                {
        //                    //ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Please select vendor name.');", true);
        //                    //return;
        //                }
        //                else
        //                {
        //                    cm.VendorName = ddlVendorName.SelectedItem.Text;
        //                    cm.VendorId = Convert.ToInt16(ddlVendorName.SelectedValue);

        //                    DataSet ds = VendorBLL.Instance.getVendorEmailId(ddlVendorName.SelectedItem.Text);
        //                    cm.VendorEmail = ds.Tables[0].Rows[0][0].ToString();
        //                }

        //                if (txtAmount.Text == "")
        //                {
        //                    ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Please enter amount.');", true);
        //                    return;
        //                }
        //                else
        //                {
        //                    cm.Amount = Convert.ToDecimal(txtAmount.Text);
        //                }
        //                if (lnkAdminPermission.Enabled == true)
        //                {
        //                    cm.IsAdminPermission = JGConstant.PERMISSION_STATUS_NOTGRANTED.ToString();
        //                }
        //                else
        //                {
        //                    cm.IsAdminPermission = JGConstant.PERMISSION_STATUS_GRANTED.ToString();
        //                }
        //                if (lnkSrSalesmanPermissionA.Enabled == true)
        //                {
        //                    cm.IsSrSalemanPermissionA = JGConstant.PERMISSION_STATUS_NOTGRANTED.ToString();
        //                }
        //                else
        //                {
        //                    cm.IsSrSalemanPermissionA = JGConstant.PERMISSION_STATUS_GRANTED.ToString();
        //                }
        //                cm.IsForemanPermission = JGConstant.PERMISSION_STATUS_GRANTED.ToString();
        //                cm.IsSrSalemanPermissionF = JGConstant.PERMISSION_STATUS_GRANTED.ToString();

        //                cm.EmailStatus = JGConstant.EMAIL_STATUS_VENDORCATEGORIES;
        //            }
        //            else // mail was not sent to vendor categories
        //            {
        //                cm.VendorName = "";
        //                cm.VendorEmail = "";
        //                cm.IsAdminPermission = JGConstant.PERMISSION_STATUS_NOTGRANTED.ToString();
        //                cm.IsSrSalemanPermissionA = JGConstant.PERMISSION_STATUS_NOTGRANTED.ToString();
        //                if (lnkForemanPermission.Enabled == true)
        //                {
        //                    cm.IsForemanPermission = JGConstant.PERMISSION_STATUS_NOTGRANTED.ToString();
        //                }
        //                else
        //                {
        //                    cm.IsForemanPermission = JGConstant.PERMISSION_STATUS_GRANTED.ToString();
        //                }
        //                if (lnkSrSalesmanPermissionF.Enabled == true)
        //                {
        //                    cm.IsSrSalemanPermissionF = JGConstant.PERMISSION_STATUS_NOTGRANTED.ToString();
        //                }
        //                else
        //                {
        //                    cm.IsSrSalemanPermissionF = JGConstant.PERMISSION_STATUS_GRANTED.ToString();
        //                }

        //                cm.EmailStatus = JGConstant.EMAIL_STATUS_NONE;
        //            }
        //            cmList.Add(cm);
        //        }
        //        if (btnSendMail.Text == "Save")
        //        {
        //            int existsList = CustomBLL.Instance.WhetherCustomMaterialListExists(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//, productTypeId, estimateId);
        //            if (existsList == 0)
        //            {
        //                saveCustom_MaterialList(cmList);
        //            }
        //            else
        //            {
        //                EnableVendorNameAndAmount();
        //                int permissionStatusCategories = CustomBLL.Instance.CheckPermissionsForCategories(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//, productTypeId, estimateId);
        //                if (permissionStatusCategories == 0)
        //                {
        //                    saveCustom_MaterialList(cmList);
        //                    ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('All lists are saved.');", true);
        //                    return;
        //                }
        //                else
        //                {
        //                    int permissionStatusVendors = CustomBLL.Instance.CheckPermissionsForVendors(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//, productTypeId, estimateId);
        //                    if (permissionStatusVendors == 0)
        //                    {
        //                        saveCustom_MaterialList(cmList);
        //                        ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('All lists are saved.');", true);
        //                        return;
        //                    }
        //                    else
        //                    {
        //                        ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('After giving permissions lists cann't be changed');", true);
        //                        return;
        //                    }
        //                }
        //            }
        //            ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('All lists are saved.');", true);
        //        }

        //        else if (btnSendMail.Text == "Send Mail To Vendor Category(s)")
        //        {

        //            int permissionStatus = CustomBLL.Instance.CheckPermissionsForCategories(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//, productTypeId, estimateId);
        //            if (permissionStatus == 1)
        //            {
        //                bool emailStatusVendorCategory = sendEmailToVendorCategories(cmList);

        //                if (emailStatusVendorCategory == true)
        //                {
        //                    bool result = CustomBLL.Instance.UpdateEmailStatusOfCustomMaterialList(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]), JGConstant.EMAIL_STATUS_VENDORCATEGORIES);//, productTypeId, estimateId);
        //                    UpdateEmailStatus(JGConstant.EMAIL_STATUS_VENDORCATEGORIES.ToString());
        //                    btnSendMail.Text = "Save";
        //                    setControlsForVendors();
        //                    grdcustom_material_list.Columns[6].Visible = true;
        //                    ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Email is sent to all vendor categories');", true);

        //                }
        //            }
        //            else
        //            {
        //                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First grant Foreman and Sr. Salesman permission');", true);
        //            }
        //        }
        //        else
        //        {
        //            int permissionStatus = CustomBLL.Instance.CheckPermissionsForVendors(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//, productTypeId, estimateId);
        //            if (permissionStatus == 1)
        //            {
        //                int statusQuotes = CustomBLL.Instance.WhetherVendorQuotesExists(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));
        //                if (statusQuotes == 1)
        //                {

        //                    bool emailStatusVendor = sendEmailToVendors(cmList);
        //                    if (emailStatusVendor == true)
        //                    {
        //                        bool result = CustomBLL.Instance.UpdateEmailStatusOfCustomMaterialList(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]), JGConstant.EMAIL_STATUS_VENDOR);//, productTypeId, estimateId);
        //                        UpdateEmailStatus(JGConstant.EMAIL_STATUS_VENDOR.ToString());
        //                        btnSendMail.Text = "Save";
        //                        ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Email is sent to all vendors');", true);
        //                        setControlsAfterSendingBothMails();

        //                        DeleteExistingWorkorders();
        //                        GenerateWorkOrder();

        //                    }
        //                }
        //                else
        //                {
        //                    ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First attach quotes.');", true);
        //                }
        //            }
        //            else
        //            {
        //                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First grant Admin and Sr. Salesman permission');", true);
        //            }

        //        }
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        private void DeleteExistingWorkorders()
        {
            string path = Server.MapPath("/CustomerDocs/Pdfs/");
            string soldjobId = Session["jobId"].ToString();
            bool result = CustomBLL.Instance.DeleteWorkorders(soldjobId);
        }

        protected void GenerateWorkOrder()
        {
            string path = Server.MapPath("/CustomerDocs/Pdfs/");

            string originalWorkOrderFilename = "WorkOrder" + ".pdf";
            string soldjobId = Session["jobId"].ToString();
            // DataSet dssoldJobs = new_customerBLL.Instance.GetProductAndEstimateIdOfSoldJob(soldjobId);
            int productId = estimateId;// Convert.ToInt16(dssoldJobs.Tables[0].Rows[0]["EstimateId"].ToString());
            DataSet ds = new_customerBLL.Instance.GetProductAndEstimateIdOfSoldJob(soldjobId);
            if (ds != null)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        string tempWorkOrderFilename = "WorkOrder" + DateTime.Now.Ticks + ".pdf";
                        DataRow dr = ds.Tables[0].Rows[i];
                        GeneratePDF(path, tempWorkOrderFilename, false, createWorkOrder("Work Order-" + dr["CustomerId"].ToString(), Convert.ToInt16(dr["CustomerId"].ToString()), Convert.ToInt16(dr["EstimateId"].ToString()), Convert.ToInt16(dr["ProductId"].ToString()), soldjobId));

                        new_customerBLL.Instance.AddCustomerDocs(Convert.ToInt32(dr["CustomerId"].ToString()), Convert.ToInt16(dr["EstimateId"].ToString()), originalWorkOrderFilename, "WorkOrder", tempWorkOrderFilename, Convert.ToInt16(dr["ProductId"].ToString()), 0);

                        string url = ConfigurationManager.AppSettings["URL"].ToString();
                        ClientScript.RegisterClientScriptBlock(Page.GetType(), "Myscript", "<script language='javascript'>window.open('" + url + "/CustomerDocs/Pdfs/" + tempWorkOrderFilename + "', null, 'width=487px,height=455px,center=1,resize=0,scrolling=1,location=no');</script>");
                    }
                }
            }
        }

        private string createWorkOrder(string InvoiceNo, int customerId, int estimateId, int productTypeId, string soldJobId)
        {
            return pdf_BLL.Instance.CreateWorkOrder(InvoiceNo, estimateId, productTypeId, customerId, soldJobId, 3);
        }

        private void GeneratePDF(string path, string fileName, bool download, string text)//download set to false in calling method
        {
            var document = new Document();
            FileStream FS = new FileStream(path + fileName, FileMode.Create);
            try
            {
                if (download)
                {
                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    PdfWriter.GetInstance(document, Response.OutputStream);
                }
                else
                {
                    PdfWriter.GetInstance(document, FS);
                }
                StringBuilder strB = new StringBuilder();
                strB.Append(text);
                //string filePath = Server.MapPath("/CustomerDocs/Pdfs/wkhtmltopdf.exe");
                //byte[] byteData = ConvertHtmlToByte(strB.ToString(), "", "", filePath);
                //if (byteData != null)
                //{
                //    StreamByteToPDF(byteData, Server.MapPath("/CustomerDocs/Pdfs/") + fileName);
                //}

                using (TextReader sReader = new StringReader(strB.ToString()))
                {
                    document.Open();
                    List<IElement> list = HTMLWorker.ParseToList(sReader, new StyleSheet());
                    foreach (IElement elm in list)
                    {
                        document.Add(elm);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Instance.writeToLog(ex, "Custom", "");
                //LogManager.Instance.WriteToFlatFile(ex.Message, "Custom",1);// Request.ServerVariables["remote_addr"].ToString());

            }
            finally
            {
                if (document.IsOpen())
                    document.Close();
            }
        }

        protected void setControlsForVendors()
        {
            DataSet ds1 = CustomBLL.Instance.GetCustom_MaterialList(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//,productTypeId,estimateId);
            decimal amount = 0;
            int vendorId = 0, i = 0;
            //foreach (GridViewRow gr in grdcustom_material_list.Rows)
            //{
            //    TextBox txtMateriallist = (TextBox)gr.FindControl("txtMateriallist");
            //    txtMateriallist.Enabled = false;
            //    TextBox txtAmount = (TextBox)gr.FindControl("txtAmount");
            //    txtAmount.Enabled = true;
            //    DropDownList ddlVendorCategory = (DropDownList)gr.FindControl("ddlVendorCategory");
            //    ddlVendorCategory.Enabled = false;
            //    int selectedCategoryID = Convert.ToInt16(ddlVendorCategory.SelectedItem.Value);
            //    DropDownList ddlVendorName = (DropDownList)gr.FindControl("ddlVendorName");
            //    ddlVendorName.Enabled = true;
            //    DataSet ds = GetVendorNames(selectedCategoryID);
            //    ddlVendorName.DataSource = ds;
            //    ddlVendorName.SelectedIndex = -1;
            //    ddlVendorName.DataTextField = "VendorName";
            //    ddlVendorName.DataValueField = "VendorId";
            //    ddlVendorName.DataBind();
            //    ddlVendorName.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "0"));
            //    ddlVendorName.SelectedIndex = 0;
            //    if (ds1.Tables[0].Rows[i]["Amount"].ToString() != "")
            //    {
            //        amount = Convert.ToDecimal(ds1.Tables[0].Rows[i]["Amount"].ToString());
            //        txtAmount.Text = amount.ToString();
            //    }
            //    if (ds1.Tables[0].Rows[i]["VendorId"].ToString() != "")
            //    {
            //        ddlVendorName.SelectedIndex = -1;
            //        vendorId = Convert.ToInt16(ds1.Tables[0].Rows[i]["VendorId"].ToString());
            //        ddlVendorName.SelectedValue = vendorId.ToString();
            //    }
            //    i++;
            //}
            //lnkAdminPermission.Visible = true;
            //lnkSrSalesmanPermissionA.Visible = true;
            //lnkForemanPermission.Visible = false;
            //lnkSrSalesmanPermissionF.Visible = false;
        }

        protected bool sendEmailToVendors(List<CustomMaterialList> cmList)
        {
            bool emailstatus = true;
            string htmlBody = string.Empty;
            string mailNotSendIds = string.Empty;
            int emailCounter = 0;
            try
            {
                //loop for each vendor
                if (cmList != null && Session[SessionKey.Key.JobId.ToString()] != null)
                {
                    foreach (CustomMaterialList cm in cmList)
                    {
                        DataSet dsVendorQuoute = VendorBLL.Instance.GetVendorQuoteByVendorId(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]), cm.VendorId);
                        string quoteTempName = "", quoteOriginalName = "";
                        if (dsVendorQuoute.Tables[0].Rows.Count > 0)
                        {
                            quoteTempName = dsVendorQuoute.Tables[0].Rows[0]["TempName"].ToString();
                            quoteOriginalName = dsVendorQuoute.Tables[0].Rows[0]["DocName"].ToString();
                        }
                        MailMessage m = new MailMessage();
                        SmtpClient sc = new SmtpClient();

                        string userName = ConfigurationManager.AppSettings["VendorUserName"].ToString();
                        string password = ConfigurationManager.AppSettings["VendorPassword"].ToString();

                        m.From = new MailAddress(userName, "JMGROVECONSTRUCTION");
                        string mailId = cm.VendorEmail;
                        m.To.Add(new MailAddress(mailId, cm.VendorName));
                        m.Subject = "J.M. Grove " + Convert.ToString(Session[SessionKey.Key.JobId.ToString()]) + " quote acceptance ";
                        m.IsBodyHtml = true;
                        DataSet dsEmailTemplate = fetchVendorEmailTemplate();

                        if (dsEmailTemplate != null)
                        {
                            string templateHeader = dsEmailTemplate.Tables[0].Rows[0][0].ToString();
                            StringBuilder tHeader = new StringBuilder();
                            tHeader.Append(templateHeader);

                            var replacedHeader = tHeader//.Replace("imgHeader", "<img src=cid:myImageHeader height=10% width=80%>")
                                                       .Replace("src=\"../img/Email art header.png\"", "src=cid:myImageHeader")
                                                       .Replace("lblJobId", Convert.ToString(Session[SessionKey.Key.JobId.ToString()]))
                                                       .Replace("lblCustomerId", "C" + customerId.ToString());
                            htmlBody = replacedHeader.ToString();

                            string templateBody = dsEmailTemplate.Tables[0].Rows[0][1].ToString();

                            StringBuilder tbody = new StringBuilder();
                            tbody.Append(templateBody);

                            var replacedBody = tbody.Replace("lblMaterialList", cm.MaterialList)
                                                    .Replace("lblAmount", cm.Amount.ToString());

                            htmlBody += replacedBody.ToString();

                            string templateFooter = dsEmailTemplate.Tables[0].Rows[0][2].ToString();
                            StringBuilder tFooter = new StringBuilder();
                            tFooter.Append(templateFooter);

                            var replacedFooter = tFooter.Replace("src=\"../img/JG-Logo-white.gif\"", "src=cid:myImageLogo")
                                                               .Replace("src=\"../img/Email footer.png\"", "src=cid:myImageFooter");
                            htmlBody += replacedFooter.ToString();
                        }
                        AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");

                        if (quoteTempName != "")
                        {
                            string sourceDir = Server.MapPath("~/CustomerDocs/VendorQuotes/");
                            Attachment attachment = new Attachment(sourceDir + "\\" + quoteTempName);
                            attachment.Name = quoteOriginalName;
                            m.Attachments.Add(attachment);
                        }
                        string imageSourceHeader = Server.MapPath(@"~\img") + @"\Email art header.png";
                        LinkedResource theEmailImageHeader = new LinkedResource(imageSourceHeader);
                        theEmailImageHeader.ContentId = "myImageHeader";

                        string imageSourceLogo = Server.MapPath(@"~\img") + @"\JG-Logo-white.gif";
                        LinkedResource theEmailImageLogo = new LinkedResource(imageSourceLogo);
                        theEmailImageLogo.ContentId = "myImageLogo";

                        string imageSourceFooter = Server.MapPath(@"~\img") + @"\Email footer.png";
                        LinkedResource theEmailImageFooter = new LinkedResource(imageSourceFooter);
                        theEmailImageFooter.ContentId = "myImageFooter";

                        //Add the Image to the Alternate view
                        htmlView.LinkedResources.Add(theEmailImageHeader);
                        htmlView.LinkedResources.Add(theEmailImageLogo);
                        htmlView.LinkedResources.Add(theEmailImageFooter);

                        m.AlternateViews.Add(htmlView);
                        m.Body = htmlBody;

                        sc.UseDefaultCredentials = false;
                        sc.Host = "jmgrove.fatcow.com";
                        sc.Port = 25;
                        sc.Credentials = new System.Net.NetworkCredential(userName, password);
                        sc.EnableSsl = false; // runtime encrypt the SMTP communications using SSL
                        try
                        {
                            sc.Send(m);
                            emailCounter += 1;
                        }
                        catch (Exception ex)
                        {
                            mailNotSendIds += mailId + " , ";
                            CustomBLL.Instance.UpdateEmailStatusOfCustomMaterialList(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]), JGConstant.EMAIL_STATUS_VENDORCATEGORIES);//, productTypeId, estimateId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            if (emailCounter == 0)
                emailstatus = false;
            else
                emailstatus = true;

            if (mailNotSendIds != string.Empty)
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Failed to send email to : " + mailNotSendIds + "');", true);

            return emailstatus;
        }

        public DataSet fetchVendorEmailTemplate()
        {
            DataSet ds = new DataSet();
            ds = AdminBLL.Instance.FetchContractTemplate(100);
            return ds;
        }

        //public DataSet fetchVendorCategoryEmailTemplate()
        //{
        //    DataSet ds = new DataSet();
        //    ds = AdminBLL.Instance.FetchContractTemplate(0);
        //    return ds;
        //}

        //public DataSet fetchVendorCategoryEmailTemplate()
        //{
        //    DataSet ds = new DataSet();
        //    ds = AdminBLL.Instance.FetchContractTemplate(0);
        //    return ds;
        //}

        protected bool sendEmailToVendorCategories(List<CustomMaterialList> cmList)
        {
            bool emailStatus = true;
            string mailNotSendIds = string.Empty;
            string htmlBody = string.Empty;
            int emailCounter = 0;
            try
            {
                if (cmList != null && Convert.ToString(Session[SessionKey.Key.JobId.ToString()]) != null)
                {
                    //loop for each vendor category on procurement page
                    foreach (CustomMaterialList cm in cmList)
                    {
                        //to fetch all vendors within a category
                        DataSet dsVendorsListByCategory = VendorBLL.Instance.fetchVendorListByCategoryForEmail(cm.VendorCategoryId);

                        if (dsVendorsListByCategory != null)
                        {
                            //loop for all vendors within a category
                            for (int counter = 0; counter < dsVendorsListByCategory.Tables[0].Rows.Count; counter++)
                            {
                                DataRow dr = dsVendorsListByCategory.Tables[0].Rows[counter];
                                string mailId = dr["Email"].ToString();
                                string vendorName = dr["VendorName"].ToString();

                                MailMessage m = new MailMessage();
                                SmtpClient sc = new SmtpClient();

                                string userName = ConfigurationManager.AppSettings["VendorCategoryUserName"].ToString();
                                string password = ConfigurationManager.AppSettings["VendorCategoryPassword"].ToString();

                                m.From = new MailAddress(userName, "JMGROVECONSTRUCTION");
                                m.To.Add(new MailAddress(mailId, vendorName));
                                m.Subject = "J.M. Grove " + Convert.ToString(Session[SessionKey.Key.JobId.ToString()]) + " quote request ";
                                m.IsBodyHtml = true;
                                DataSet dsEmailTemplate = fetchVendorCategoryEmailTemplate();

                                if (dsEmailTemplate != null)
                                {
                                    string templateHeader = dsEmailTemplate.Tables[0].Rows[0][0].ToString();
                                    StringBuilder tHeader = new StringBuilder();
                                    tHeader.Append(templateHeader);
                                    var replacedHeader = tHeader//.Replace("imgHeader", "<img src=cid:myImageHeader height=10% width=80%>")
                                                                   .Replace("src=\"../img/Email art header.png\"", "src=cid:myImageHeader")
                                                                .Replace("lblJobId", Convert.ToString(Session[SessionKey.Key.JobId.ToString()]).ToString())
                                                                .Replace("lblCustomerId", "C" + customerId.ToString());
                                    htmlBody = replacedHeader.ToString();
                                    htmlBody += "</br></br></br>";
                                    string templateBody = dsEmailTemplate.Tables[0].Rows[0][1].ToString();

                                    string materialList = cm.MaterialList;


                                    StringBuilder tbody = new StringBuilder();
                                    tbody.Append(templateBody);

                                    var replacedBody = tbody.Replace("lblMaterialList", materialList);

                                    htmlBody += replacedBody.ToString();

                                    htmlBody += "</br></br></br>";

                                    string templateFooter = dsEmailTemplate.Tables[0].Rows[0][2].ToString();
                                    StringBuilder tFooter = new StringBuilder();
                                    tFooter.Append(templateFooter);
                                    var replacedFooter = tFooter.Replace("src=\"../img/JG-Logo-white.gif\"", "src=cid:myImageLogo")
                                                               .Replace("src=\"../img/Email footer.png\"", "src=cid:myImageFooter");
                                    htmlBody += replacedFooter.ToString();
                                }
                                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");

                                string imageSourceHeader = Server.MapPath(@"~\img") + @"\Email art header.png";
                                LinkedResource theEmailImageHeader = new LinkedResource(imageSourceHeader);
                                theEmailImageHeader.ContentId = "myImageHeader";

                                string imageSourceLogo = Server.MapPath(@"~\img") + @"\JG-Logo-white.gif";
                                LinkedResource theEmailImageLogo = new LinkedResource(imageSourceLogo);
                                theEmailImageLogo.ContentId = "myImageLogo";

                                string imageSourceFooter = Server.MapPath(@"~\img") + @"\Email footer.png";
                                LinkedResource theEmailImageFooter = new LinkedResource(imageSourceFooter);
                                theEmailImageFooter.ContentId = "myImageFooter";

                                //Add the Image to the Alternate view
                                htmlView.LinkedResources.Add(theEmailImageHeader);
                                htmlView.LinkedResources.Add(theEmailImageLogo);
                                htmlView.LinkedResources.Add(theEmailImageFooter);

                                m.AlternateViews.Add(htmlView);
                                m.Body = htmlBody;
                                sc.UseDefaultCredentials = false;
                                sc.Host = "jmgrove.fatcow.com";
                                sc.Port = 25;


                                sc.Credentials = new System.Net.NetworkCredential(userName, password);
                                sc.EnableSsl = false; // runtime encrypt the SMTP communications using SSL
                                try
                                {
                                    sc.Send(m);
                                    emailCounter += 1;
                                }
                                catch (Exception ex)
                                {
                                    mailNotSendIds += mailId + " , ";
                                    CustomBLL.Instance.UpdateEmailStatusOfCustomMaterialList(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]), JGConstant.EMAIL_STATUS_NONE);//, productTypeId, estimateId);
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('" + ex.Message + "');", true);
            }
            if (mailNotSendIds != string.Empty)
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Failed to send email to : " + mailNotSendIds + "');", true);
            if (emailCounter == 0)
                emailStatus = false;
            else
                emailStatus = true;

            return emailStatus;
        }


        public DataSet fetchVendorCategoryEmailTemplate()
        {
            DataSet ds = new DataSet();
            ds = AdminBLL.Instance.FetchContractTemplate(0);
            return ds;
        }

        protected void btnClose_Click(object sender, EventArgs e)
        {
            Response.Redirect("Procurement.aspx");
        }

        protected void btnXAdmin_Click(object sender, EventArgs e)
        {
            //popupAdmin_permission.Hide();
        }


        protected void btnXSrSalesmanA_Click(object sender, EventArgs e)
        {
            //popupSrSalesmanPermissionA.Hide();
        }


        protected void VerifySrSalesmanPermissionA(object sender, EventArgs e)
        {
            int cResult = CustomBLL.Instance.WhetherVendorInCustomMaterialListExists(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]));//, productTypeId, estimateId);
            if (cResult == 1)
            {
                if (!string.IsNullOrEmpty(txtSrSalesmanPasswordA.Text))
                {
                    string salesmanCode = Session["loginpassword"].ToString();
                    if (salesmanCode != txtSrSalesmanPasswordA.Text.Trim())
                    {
                        CVSrSalesmanA.ErrorMessage = "Invalid Sr. Salesman Code";
                        CVSrSalesmanA.ForeColor = System.Drawing.Color.Red;
                        CVSrSalesmanA.IsValid = false;
                        CVSrSalesmanA.Visible = true;
                        //popupSrSalesmanPermissionA.Show();
                        return;
                    }
                    else
                    {
                        int result = CustomBLL.Instance.UpdateSrSalesmanPermissionOfCustomMaterialList(Convert.ToString(Session[SessionKey.Key.JobId.ToString()]).ToString(), JGConstant.PERMISSION_STATUS_GRANTED, Convert.ToString(Session["loginid"]));//, productTypeId, estimateId);
                        if (result == 0)
                        {
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First save Material List');", true);
                        }
                        else
                        {
                            //lnkSrSalesmanPermissionA.Enabled = false;
                            //lnkSrSalesmanPermissionA.ForeColor = System.Drawing.Color.DarkGray;
                            //popupSrSalesmanPermissionA.TargetControlID = "hdnSrA";
                            SetButtonText();
                            DisableVendorNameAndAmount();
                        }
                    }
                }
                else
                {
                    CVSrSalesmanA.ErrorMessage = "Please Enter Sr. Salesman Code";
                    CVSrSalesmanA.ForeColor = System.Drawing.Color.Red;
                    CVSrSalesmanA.IsValid = false;
                    CVSrSalesmanA.Visible = true;
                    //popupSrSalesmanPermissionA.Show();
                    return;
                }
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First save Material List and enter all vendor names');", true);
            }
        }


        private void DisableVendorNameAndAmount()
        {
            //foreach (GridViewRow r in grdcustom_material_list.Rows)
            //{
            //    DropDownList ddlVendorName = (DropDownList)r.FindControl("ddlVendorName");
            //    ddlVendorName.Enabled = false;
            //    TextBox txtAmount = (TextBox)r.FindControl("txtAmount");
            //    txtAmount.Enabled = false;
            //}
        }

        protected void btnXForeman_Click(object sender, EventArgs e)
        {
            //popupForeman_permission.Hide();
        }

        protected void grdcustom_material_list_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                string fileName = Convert.ToString(e.CommandArgument);
                if (e.CommandName.Equals("View", StringComparison.InvariantCultureIgnoreCase))
                {
                    string domainName = Request.Url.GetLeftPart(UriPartial.Authority);

                    ClientScript.RegisterClientScriptBlock(Page.GetType(), "Myscript", "<script language='javascript'>window.open('" + domainName + "/CustomerDocs/VendorQuotes/" + fileName + "', null, 'width=487px,height=455px,center=1,resize=0,scrolling=1,location=no');</script>");
                }
            }
            catch (Exception ex)
            {
            }
        }

        protected void grdcustom_material_list_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Label lblDisable = (Label)e.Row.FindControl("lblDisable");
                Label lblsrno = (Label)e.Row.FindControl("lblsrno");
                lblsrno.Text = Convert.ToString(Convert.ToInt16(lblsrno.Text) + 1);
                DropDownList ddlVendorCategory = (DropDownList)e.Row.FindControl("ddlVendorCategory");
                DataSet dsVendorCategory = GetVendorCategories();
                Label lblReason = (Label)e.Row.FindControl("lblReason");
                ddlVendorCategory.DataSource = GetVendorCategories();
                ddlVendorCategory.DataSource = dsVendorCategory;
                ddlVendorCategory.DataTextField = "VendorCategoryNm";
                ddlVendorCategory.DataValueField = "VendorCategpryId";
                ddlVendorCategory.DataBind();
                ddlVendorCategory.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "0"));
                ddlVendorCategory.SelectedIndex = 0;
                if (lblDisable.Text != "")
                {
                    foreach (TableCell cell in e.Row.Cells)
                    {
                        // cell.BackColor = Color.Red;
                        lblReason.Visible = true;
                    }
                }
                else
                {
                    lblReason.Visible = false;
                }
            }
            //if (btnSendMail.Text == "Send Mail To Vendor(s)")
            //{
            //    grdcustom_material_list.Columns[6].Visible = false;
            //}
        }

        protected void grdcustom_material_list_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            List<CustomMaterialList> cmList = GetMaterialListFromGrid();
            if (cmList.Count > 1)
            {
                CustomMaterialList cm = cmList[e.RowIndex];
                cm.Status = JGConstant.CustomMaterialListStatus.Deleted;
                UpdateMaterialList(cm, e.RowIndex);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Atleast one row must be there in Custom- Material List');", true);
            }
        }

        public DataSet GetVendorCategories()
        {
            DataSet ds = new DataSet();
            ds = VendorBLL.Instance.fetchAllVendorCategoryHavingVendors();
            return ds;
        }

        protected void UpdateMaterialList(CustomMaterialList item, int rowIndex = 0)
        {
            List<CustomMaterialList> itemList = GetMaterialListFromGrid();

            switch (item.Status)
            {
                case JGConstant.CustomMaterialListStatus.Unchanged:
                    break;
                case JGConstant.CustomMaterialListStatus.Added:
                    itemList.Add(item);
                    break;
                case JGConstant.CustomMaterialListStatus.Deleted:
                    itemList[rowIndex].Status = JGConstant.CustomMaterialListStatus.Deleted;
                    break;
                case JGConstant.CustomMaterialListStatus.Modified:
                    itemList[rowIndex] = item;
                    break;
                default:
                    break;
            }

            ViewState["CustomMaterialList"] = itemList;
            BindCustomMaterialList(itemList);
        }

        protected void lnkVendorCategory_Click(object sender, EventArgs e)
        {
            //pnlEmailTemplateForVendorCategories.Visible = true;
            //pnlEmailTemplateForVendors.Visible = false;
            //lnkVendorCategory.ForeColor = System.Drawing.Color.DarkGray;
            //lnkVendorCategory.Enabled = false;
            //lnkVendor.Enabled = true;
            //lnkVendor.ForeColor = System.Drawing.Color.Blue;
            bind();
        }

        protected void lnkVendor_Click(object sender, EventArgs e)
        {
            //pnlEmailTemplateForVendors.Visible = true;
            //pnlEmailTemplateForVendorCategories.Visible = false;
            //lnkVendor.ForeColor = System.Drawing.Color.DarkGray;
            //lnkVendor.Enabled = false;
            //lnkVendorCategory.Enabled = true;
            //lnkVendorCategory.ForeColor = System.Drawing.Color.Blue;
            bindVendorTemplate();
        }

        protected void bindVendorTemplate()
        {
            DataSet ds = new DataSet();
            ds = AdminBLL.Instance.FetchContractTemplate(100);
            if (ds != null)
            {
                //HeaderEditorVendor.Content = ds.Tables[0].Rows[0][0].ToString();
                //lblMaterialsVendor.Text = ds.Tables[0].Rows[0][1].ToString();
                //FooterEditorVendor.Content = ds.Tables[0].Rows[0][2].ToString();
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            //string Editor_contentHeader = HeaderEditor.Content;
            //string Editor_contentFooter = FooterEditor.Content;
            bool result = true;//AdminBLL.Instance.UpdateEmailVendorCategoryTemplate(Editor_contentHeader, Editor_contentFooter);
            if (result)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('EmailVendor Template Updated Successfully');", true);
            }
        }

        protected void btnUpdateVendor_Click(object sender, EventArgs e)
        {
            //string Editor_contentHeader = HeaderEditorVendor.Content;
            //string Editor_contentFooter = FooterEditorVendor.Content;
            bool result = true;//AdminBLL.Instance.UpdateEmailVendorTemplate(Editor_contentHeader, Editor_contentFooter);
            if (result)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('EmailVendor Template Updated Successfully');", true);
            }
        }

        protected void btnAddcategory_Click(object sender, EventArgs e)
        {

        }

        protected void btndeletecategory_Click(object sender, EventArgs e)
        {

        }

        protected void btnNewMinus_Click(object sender, EventArgs e)
        {

        }

        protected void grdsoldjobs_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridViewRow rowData = grdsoldjobs.Rows[grdsoldjobs.SelectedIndex];
            foreach (GridViewRow row in grdsoldjobs.Rows)
            {
                if (row.RowIndex == grdsoldjobs.SelectedIndex)
                {
                    if (row.BackColor == ColorTranslator.FromHtml("#FF0000"))
                    {
                        Session["DisableCustid"] = "";
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('Disabled row cannot be selected.')", true);
                        return;
                    }
                    else
                    {
                        // row.BackColor = ColorTranslator.FromHtml("#FF0000");
                        row.ToolTip = string.Empty;
                        string str_Custid = Convert.ToString(grdsoldjobs.DataKeys[rowData.RowIndex].Values[0]);
                        str_Custid = str_Custid.Replace("C", "");
                        int Custid = Convert.ToInt32(str_Custid);
                        Session["DisableCustid"] = "";
                        Session["DisableCustid"] = Custid;
                    }
                }
            }
        }

        protected void btnSaveDisable_Click(object sender, EventArgs e)
        {
            AdminBLL.Instance.DisableCustomer(Convert.ToInt32(Session["DisableCustid"]), txtReasonDisable.Text);
            bindSoldJobs();
        }

        protected void btnDisable_Click(object sender, EventArgs e)
        {
            if (Convert.ToString(Session["DisableCustid"]) == "")
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "DisableAlert", "alert('Select Customer from grid to disable.')", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "overlay", "overlay();", true);
            }
        }

        //protected void ddlVendorName_SelectedIndexChanged1(object sender, EventArgs e)
        //{
        //    if (Convert.ToString(ddlVendorName.SelectedValue) != "")
        //    {
        //        EditVendor(Convert.ToInt16(ddlVendorName.SelectedValue));
        //    }
        //    // flag = "Autosave";
        //    // SaveAllData(); 
        //}

        public void EditVendor(int VendorIdToEdit)
        {
            DataSet ds = new DataSet();
            ds = VendorBLL.Instance.FetchvendorDetails(VendorIdToEdit);

            txtVendorNm.Text = ds.Tables[0].Rows[0]["VendorName"].ToString();
            //ddlVndrCategory.SelectedValue = ds.Tables[0].Rows[0]["VendorCategoryId"].ToString();
            string Name = ds.Tables[0].Rows[0]["ContactPerson"].ToString();

            // Split Name
            string[] splittedName = Name.Split(' ');
            txtFName.Text = splittedName[0].ToString();
            txtLName.Text = splittedName[1].ToString();

            string ContactNo = ds.Tables[0].Rows[0]["ContactNumber"].ToString();

            // Spilt Contact No.

            txtVendorFax.Text = ds.Tables[0].Rows[0]["Fax"].ToString();
            txtprimaryemail.Text = ds.Tables[0].Rows[0]["Email"].ToString();
            txtPrimaryAddress.Text = ds.Tables[0].Rows[0]["Address"].ToString();
            //txtNotes.Text = ds.Tables[0].Rows[0]["Notes"].ToString();
            ddlVendorStatus.SelectedValue = ds.Tables[0].Rows[0]["VendorStatus"].ToString();
            txtTaxId.Text = ds.Tables[0].Rows[0]["TaxId"].ToString();
            ddlmanufacturertype.SelectedValue = ds.Tables[0].Rows[0]["ManufacturerType"].ToString();
            txtBillingAddr.Text = ds.Tables[0].Rows[0]["BillingAddress"].ToString();
          
            //txtExpenseCat.Text = ds.Tables[0].Rows[0]["ExpenseCategory"].ToString();
            //txtAutoInsurance.Text = ds.Tables[0].Rows[0]["AutoTruckInsurance"].ToString();
            txtVendorId.Text = ds.Tables[0].Rows[0]["VendorId"].ToString();
            btnSave.Text = "Update";

        }




        //protected void txtVendorNm_TextChanged(object sender, EventArgs e)
        //{
        //    flag = "Autosave";
        //    SaveAllData(); 
        //}

        //protected void txtcontactperson_TextChanged(object sender, EventArgs e)
        //{
        //    flag = "Autosave";
        //    SaveAllData(); 
        //}

        //protected void ddlMenufacturer_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    flag = "Autosave";
        //    SaveAllData(); 
        //}

        //protected void txtBillingAddress_TextChanged(object sender, EventArgs e)
        //{
        //    flag = "Autosave";
        //    SaveAllData(); 
        //}

        //protected void txtExpenseCat_TextChanged(object sender, EventArgs e)
        //{
        //    flag = "Autosave";
        //    SaveAllData(); 
        //}

        //protected void txtAutoInsurance_TextChanged(object sender, EventArgs e)
        //{
        //    flag = "Autosave";
        //    SaveAllData(); 
        //}
        //protected void txtcontactnumber_TextChanged(object sender, EventArgs e)
        //{
        //    flag = "Autosave";
        //    SaveAllData(); 
        //}
        //protected void txtfax_TextChanged(object sender, EventArgs e)
        //{
        //    flag = "Autosave";
        //    SaveAllData(); 
        //}
        //protected void txtmail_TextChanged(object sender, EventArgs e)
        //{
        //    flag = "Autosave";
        //    SaveAllData(); 
        //}
        //protected void txtaddress_TextChanged(object sender, EventArgs e)
        //{
        //    flag = "Autosave";
        //    SaveAllData(); 
        //}


        //protected void txtNotes_TextChanged(object sender, EventArgs e)
        //{
        //    flag = "Autosave";
        //    SaveAllData(); 
        //}

        //protected void txtVendorId_TextChanged(object sender, EventArgs e)
        //{
        //    flag = "Autosave";
        //    SaveAllData(); 
        //}

        //protected void txtTaxId_TextChanged(object sender, EventArgs e)
        //{
        //    flag = "Autosave";
        //    SaveAllData(); 
        //}


    }
}