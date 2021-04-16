using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using iCore_User.Modules;
using iCore_User.Modules.SecurityAuthentication;

namespace iCore_User.Areas.HSU_Portal.Controllers
{
    public class ToolsController : Controller
    {
        //====================================================================================================================
        iCore_Administrator.Modules.AuthenticationTester AAuth = new iCore_Administrator.Modules.AuthenticationTester();
        iCore_Administrator.Modules.SQL_Tranceiver Sq = new iCore_Administrator.Modules.SQL_Tranceiver();
        iCore_Administrator.Modules.PublicFunctions Pb = new iCore_Administrator.Modules.PublicFunctions();
        iCore_Administrator.Modules.EmailSender Email = new iCore_Administrator.Modules.EmailSender();
        iCore_Administrator.Modules.MimeTypeMap MTM = new iCore_Administrator.Modules.MimeTypeMap();
        iCore_Administrator.Modules.Crypto Cry = new iCore_Administrator.Modules.Crypto();
        //====================================================================================================================
        public ActionResult Index() { return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" }); }
        //====================================================================================================================
        public ActionResult RegisterForms()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 13;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string UserID = Session["User_UID"].ToString().Trim();
                DataTable DT = new DataTable();
                DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ID,UnicID,Name,Description,Template_Name,Status_Code From Users_04_Hospitality_SingleUser_RegisterForms Where (User_ID = '" + UserID + "') And (Removed = '0') Order By Name");
                ViewBag.DT = DT.Rows;
                ViewBag.UserID = UserID;
                // Customer Site URL Maker :
                string BasicURL = System.Configuration.ConfigurationManager.AppSettings["iCore_Customer_URL"];
                DataTable DT_Setting = new DataTable();
                DT_Setting = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Domain_Address From Users_03_Hospitality_SingleUser_BasicSetting Where (User_ID = '" + UserID + "')");
                if (DT_Setting.Rows.Count == 1)
                {
                    if (DT_Setting.Rows[0][0].ToString().Trim() != "")
                    {
                        BasicURL = DT_Setting.Rows[0][0].ToString().Trim();
                    }
                }
                BasicURL = BasicURL.Trim();
                if (BasicURL != "")
                {
                    try
                    {
                        BasicURL = BasicURL.Replace("///", "/").Trim();
                        if (BasicURL[BasicURL.Length - 1] == '/') { BasicURL = BasicURL.Substring(0, BasicURL.Length - 1); }
                        if (BasicURL[BasicURL.Length - 1] == '/') { BasicURL = BasicURL.Substring(0, BasicURL.Length - 1); }
                        if (BasicURL[BasicURL.Length - 1] == '/') { BasicURL = BasicURL.Substring(0, BasicURL.Length - 1); }
                        BasicURL = BasicURL.Trim();
                    }
                    catch (Exception)
                    {
                        BasicURL = "";
                    }
                }
                else
                {
                    BasicURL = "#";
                }
                BasicURL = BasicURL.Trim();
                if (BasicURL == "") { BasicURL = "#"; }
                ViewBag.BU = BasicURL + "/Login/HSURF/";
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" });
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_Grid(string PID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 13;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    PID = PID.Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ID,UnicID,Name,Description,Template_Name,Status_Code From Users_04_Hospitality_SingleUser_RegisterForms Where (User_ID = '" + PID + "') And (Removed = '0') Order By Name");
                    if (DT.Rows != null)
                    {
                        foreach (DataRow RW in DT.Rows)
                        {
                            ResSTR += "<tr>";
                            ResSTR += "<td style=\"text-align:center;\"><i class=\"fa fa-list-alt text-primary\" style=\"font-size:25px\"></i></td>";
                            ResSTR += "<td>" + RW[2].ToString().Trim() + "</td>";
                            ResSTR += "<td>" + RW[3].ToString().Trim() + "</td>";
                            ResSTR += "<td>" + RW[4].ToString().Trim() + "</td>";
                            if (RW[5].ToString().Trim() == "0")
                            {
                                ResSTR += "<td style=\"text-align:center\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px\">Disabled</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td style=\"text-align:center\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px\">Active</div>";
                                ResSTR += "</td>";
                            }
                            ResSTR += "<td style=\"text-align:center\">";
                            ResSTR += "<div class=\"btn btn-info\" style=\"font-size:12px\" onclick=\"Show_Designform('" + RW[0].ToString().Trim() + "','" + RW[2].ToString().Trim() + "')\">";
                            ResSTR += "<i class=\"fa fa-paint-brush\" style=\"margin-right:5px;margin-left:5px\"></i>";
                            ResSTR += "Form designer";
                            ResSTR += "</div>";
                            ResSTR += "</td>";
                            ResSTR += "<td style=\"text-align:center\">";
                            ResSTR += "<div class=\"btn-group dropleft\">";
                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                            ResSTR += "</button>";
                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Form_Remove('" + RW[0].ToString().Trim() + "','" + RW[2].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i> Remove form </a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Form_ChangeStatus('" + RW[0].ToString().Trim() + "','" + RW[2].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i> Change status </a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Form_Edit('" + RW[0].ToString().Trim() + "','" + RW[2].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i> Edit form </a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Form_ConfigurationST('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-cogs text-primary\" style=\"width:24px;font-size:14px\"></i>Form configuration</a>";
                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Form_CopyLink('1', '" + RW[0].ToString().Trim() + "')\"><i class=\"fa fa-copy text-primary\" style=\"width:24px;font-size:14px\"></i> Copy Customer URL link </a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Form_CopyLink('2', '" + RW[0].ToString().Trim() + "')\"><i class=\"fa fa-copy text-primary\" style=\"width:24px;font-size:14px\"></i> Copy Customer iFrame URL </a>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_AddForm(string PID, string NM, string TC, string TN, string DC)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 18;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    PID = PID.Trim();
                    NM = NM.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    TC = TC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    TN = TN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    DC = DC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    if ((PID == "0") || (PID == ""))
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while getting user information";
                    }
                    else
                    {
                        if (NM == "")
                        {
                            ResVal = "1"; ResSTR = "It is necessary to enter the name to add a new form";
                        }
                        else
                        {
                            NM = Pb.Text_UpperCase_AfterSpase(NM);
                        }
                        if (ResVal == "0")
                        {
                            DataTable DTtest = new DataTable();
                            DTtest = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ID From Users_04_Hospitality_SingleUser_RegisterForms Where (User_ID = '" + PID + "') And (Name = '" + NM + "') And (Removed = '0')");
                            if (DTtest.Rows.Count > 0) { ResVal = "1"; ResSTR = "The entered form name is duplicated, please try again after recheck"; }
                        }
                        if (ResVal == "0")
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            DataTable DTNR = new DataTable();
                            DTNR = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_04_Hospitality_SingleUser_RegisterForms OUTPUT Inserted.ID Values ('" + PID + "','EMAS','" + NM + "','" + DC + "','" + TC + "','" + TN + "','EMAS','EMAS','1','Active','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0')");
                            string RowID = DTNR.Rows[0][0].ToString().Trim();
                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_04_Hospitality_SingleUser_RegisterForms Set [UnicID] = '" + RowID + Pb.Make_Security_Code(40) + "' Where (ID = '" + RowID + "') And (User_ID = '" + PID + "')");
                            Thread.Sleep(20);
                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_04_Hospitality_SingleUser_RegisterForms Set [Customer_Side_URL] = '" + RowID + Pb.Make_Security_Code(100) + "' Where (ID = '" + RowID + "') And (User_ID = '" + PID + "')");
                            Thread.Sleep(20);
                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_04_Hospitality_SingleUser_RegisterForms Set [Customer_Side_iFrame] = '" + RowID + Pb.Make_Security_Code(100) + "' Where (ID = '" + RowID + "') And (User_ID = '" + PID + "')");
                            ResSTR = "The form named " + NM + " successfully added";
                        }
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_RemoveForm(string PID, string FID, string FNM)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 19;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    PID = PID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    FID = FID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    FNM = FNM.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ID From Users_04_Hospitality_SingleUser_RegisterForms Where (ID = '" + FID + "') And (User_ID  = '" + PID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_04_Hospitality_SingleUser_RegisterForms Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + FID + "') And (User_ID  = '" + PID + "') And (Removed = '0')");
                            ResSTR = "The " + DT.Rows[0][0].ToString().Trim() + " form was successfully deleted";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified form is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving form information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_ChangeStatusForm(string PID, string FID, string FNM)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 20;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    PID = PID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    FID = FID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    FNM = FNM.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ID,Status_Code From Users_04_Hospitality_SingleUser_RegisterForms Where (ID = '" + FID + "') And (User_ID  = '" + PID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (DT.Rows[0][1].ToString().Trim() == "1")
                            {
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_04_Hospitality_SingleUser_RegisterForms Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + FID + "') And (User_ID  = '" + PID + "') And (Removed = '0')");
                                ResSTR = "The " + FNM + " section was successfully change status to disabled";
                            }
                            else
                            {
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_04_Hospitality_SingleUser_RegisterForms Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + FID + "') And (User_ID  = '" + PID + "') And (Removed = '0')");
                                ResSTR = "The " + FNM + " section was successfully change status to active";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified form is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving form information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_RetrieveForm(string PID, string FID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 21;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                PID = PID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                FID = FID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Name,Description,Template_Code,Customer_Side_URL,Customer_Side_iFrame From Users_04_Hospitality_SingleUser_RegisterForms Where (ID = '" + FID + "') And (User_ID = '" + PID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            ResSTR = DT.Rows[0][0].ToString().Trim() + "#" + DT.Rows[0][1].ToString().Trim() + "#" + DT.Rows[0][2].ToString().Trim() + "#" + DT.Rows[0][3].ToString().Trim() + "#" + DT.Rows[0][4].ToString().Trim();
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified form is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving form information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_FormGenerateLink(string PID, string FID, string LTP)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 22;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    PID = PID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    FID = FID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    LTP = LTP.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ID,Name From Users_04_Hospitality_SingleUser_RegisterForms Where (ID = '" + FID + "') And (User_ID = '" + PID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string NewLink = FID + Pb.Make_Security_Code(100);
                            //string InsDate = Sq.Sql_Date();
                            //string InsTime = Sq.Sql_Time();
                            if (LTP == "1")
                            {
                                //Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_04_Hospitality_SingleUser_RegisterForms Set [Customer_Side_URL] = '" + NewLink + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + FID + "') And (User_ID = '" + PID + "')");
                            }
                            else
                            {
                                //Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_04_Hospitality_SingleUser_RegisterForms Set [Customer_Side_iFrame] = '" + NewLink + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + FID + "') And (User_ID = '" + PID + "')");
                            }
                            ResSTR = NewLink;
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified form is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving form information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_FormCopyLink(string PID, string FID, string LTP)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 22;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    PID = PID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    FID = FID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    LTP = LTP.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Customer_Side_URL,Customer_Side_iFrame From Users_04_Hospitality_SingleUser_RegisterForms Where (ID = '" + FID + "') And (User_ID = '" + PID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            // Customer Site URL Maker :
                            string BasicURL = System.Configuration.ConfigurationManager.AppSettings["iCore_Customer_URL"];
                            DataTable DT_Setting = new DataTable();
                            DT_Setting = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Domain_Address From Users_03_Hospitality_SingleUser_BasicSetting Where (User_ID = '" + PID + "')");
                            if (DT_Setting.Rows.Count == 1)
                            {
                                if (DT_Setting.Rows[0][0].ToString().Trim() != "")
                                {
                                    BasicURL = DT_Setting.Rows[0][0].ToString().Trim();
                                }
                            }
                            BasicURL = BasicURL.Trim();
                            if (BasicURL != "")
                            {
                                try
                                {
                                    BasicURL = BasicURL.Replace("///", "/").Trim();
                                    if (BasicURL[BasicURL.Length - 1] == '/') { BasicURL = BasicURL.Substring(0, BasicURL.Length - 1); }
                                    if (BasicURL[BasicURL.Length - 1] == '/') { BasicURL = BasicURL.Substring(0, BasicURL.Length - 1); }
                                    if (BasicURL[BasicURL.Length - 1] == '/') { BasicURL = BasicURL.Substring(0, BasicURL.Length - 1); }
                                    BasicURL = BasicURL.Trim();
                                }
                                catch (Exception)
                                {
                                    BasicURL = "";
                                }
                            }
                            else
                            {
                                BasicURL = "#";
                            }
                            BasicURL = BasicURL.Trim();
                            if (BasicURL == "") { BasicURL = "#"; }
                            BasicURL = BasicURL + "/Login/HSURF/";
                            if (LTP == "1")
                            {
                                ResSTR = BasicURL + DT.Rows[0][0].ToString().Trim();
                            }
                            else
                            {
                                ResSTR = BasicURL + DT.Rows[0][1].ToString().Trim();
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified form is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving form information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_SaveEditForm(string PID, string FID, string NM, string DC, string TC, string TN, string UF, string US)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 21;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    PID = PID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    FID = FID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    NM = NM.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    DC = DC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    TC = TC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    TN = TN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    UF = UF.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    US = US.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    if ((FID == "0") || (FID == ""))
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading form information";
                    }
                    else
                    {
                        DataTable DT = new DataTable();
                        DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ID From Users_04_Hospitality_SingleUser_RegisterForms Where (ID <> '" + FID + "') And (User_ID = '" + PID + "') And (Name = '" + NM + "') And (Removed = '0')");
                        if (DT.Rows != null)
                        {
                            if (DT.Rows.Count == 0)
                            {
                                string InsDate = Sq.Sql_Date();
                                string InsTime = Sq.Sql_Time();
                                NM = NM = Pb.Text_UpperCase_AfterSpase(NM);
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_04_Hospitality_SingleUser_RegisterForms Set [Name] = '" + NM + "',[Description] = '" + DC + "',[Template_Code] = '" + TC + "',[Template_Name] = '" + TN + "',Customer_Side_URL = '" + UF + "',Customer_Side_iFrame = '" + US + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + FID + "') And (User_ID = '" + PID + "')");
                                ResVal = "0"; ResSTR = "Form named " + NM.ToLower() + " was successfully edited";
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "The entered name of the form is duplicate, so it is not possible for you to save form information with this name";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The server encountered an error while reloading information";
                        }
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_GridSection(string GID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 13;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    GID = GID.Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Section_ID,Name,Icon,Row_Index,Width,Status_Code From Users_05_Hospitality_SingleUser_RegisterForms_Section Where (Group_ID = '" + GID + "') And (Removed = '0') Order By Row_Index,Name");
                    if (DT.Rows != null)
                    {
                        foreach (DataRow RW in DT.Rows)
                        {
                            ResSTR += "<tr>";
                            ResSTR += "<td onclick=\"Section_Edit('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[1].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"Section_Edit('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\"><i class=\"text-primary fa " + RW[2].ToString().Trim() + "\"></i></td>";
                            ResSTR += "<td onclick=\"Section_Edit('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[3].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"Section_Edit('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[4].ToString().Trim() + "</td>";
                            if (RW[5].ToString().Trim() == "0")
                            {
                                ResSTR += "<td onclick=\"Section_Edit('" + RW[0].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px\">Disabled</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td onclick=\"Section_Edit('" + RW[0].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px\">Active</div>";
                                ResSTR += "</td>";
                            }
                            ResSTR += "<td style=\"text-align:center\">";
                            ResSTR += "<div class=\"btn-group dropleft\">";
                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                            ResSTR += "</button>";
                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Preview_Section('" + RW[0].ToString().Trim() + "')\"><i class=\"fa fa-search text-primary\" style=\"width:24px;font-size:14px\"></i>Section preview</a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Section_Edit('" + RW[0].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit section</a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Section_Remove('" + RW[0].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove section</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Section_ChangeStatus('" + RW[0].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</td>";
                            ResSTR += "<td style=\"text-align:center\">";
                            ResSTR += "<div class=\"btn btn-danger\" style=\"font-size:12px\" onclick=\"Show_Element('" + RW[0].ToString().Trim() + "','" + RW[1].ToString().Trim() + "')\">";
                            ResSTR += "<i class=\"fa fa-id-card\" style=\"margin-right:5px;margin-left:5px\"></i>";
                            ResSTR += "Elements designer";
                            ResSTR += "</div>";
                            ResSTR += "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_SecIndex(string GID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 13;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GID = GID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Max(Row_Index) From Users_05_Hospitality_SingleUser_RegisterForms_Section Where (Group_ID = '" + GID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            long NowIndex = 0;
                            try { NowIndex = long.Parse(DT.Rows[0][0].ToString().Trim()); } catch (Exception) { }
                            NowIndex++;
                            ResSTR = NowIndex.ToString();
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The server encountered an error while reloading information";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_AddSection(string GroupID, string Sec_Name, string Sec_Icon, string Sec_Index, string Sec_Col)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 23;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    GroupID = GroupID.Trim();
                    Sec_Name = Sec_Name.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    Sec_Icon = Sec_Icon.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    Sec_Index = Sec_Index.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    Sec_Col = Sec_Col.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    if ((GroupID == "0") || (GroupID == ""))
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading group information";
                    }
                    else
                    {
                        DataTable DT = new DataTable();
                        DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Section_ID From Users_05_Hospitality_SingleUser_RegisterForms_Section Where (Group_ID = '" + GroupID + "') And (Name = '" + Sec_Name + "') And (Removed = '0')");
                        if (DT.Rows != null)
                        {
                            if (DT.Rows.Count == 0)
                            {
                                string InsDate = Sq.Sql_Date();
                                string InsTime = Sq.Sql_Time();
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_05_Hospitality_SingleUser_RegisterForms_Section Values ('" + GroupID + "','" + Sec_Name + "','" + Sec_Icon + "','" + Sec_Index + "','" + Sec_Col + "','1','Active','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0')");
                                ResVal = "0"; ResSTR = "Section named " + Sec_Name + " was successfully added";
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "The entered name of the section is duplicate, so it is not possible for you to add new section with this name";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The server encountered an error while reloading information";
                        }
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_RemoveSection(string GroupID, string SecID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 24;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GroupID = GroupID.Trim();
                SecID = SecID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Name From Users_05_Hospitality_SingleUser_RegisterForms_Section Where (Group_ID = '" + GroupID + "') And (Section_ID  = '" + SecID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_05_Hospitality_SingleUser_RegisterForms_Section Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GroupID + "') And (Section_ID  = '" + SecID + "')");
                            ResSTR = "The " + DT.Rows[0][0].ToString().Trim() + " section was successfully deleted";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified section is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving section information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_ChangeStatusSection(string GroupID, string SecID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 25;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GroupID = GroupID.Trim();
                SecID = SecID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Name,Status_Code From Users_05_Hospitality_SingleUser_RegisterForms_Section Where (Group_ID = '" + GroupID + "') And (Section_ID = '" + SecID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (DT.Rows[0][1].ToString().Trim() == "1")
                            {
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_05_Hospitality_SingleUser_RegisterForms_Section Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GroupID + "') And (Section_ID = '" + SecID + "')");
                                ResSTR = "The " + DT.Rows[0][0].ToString().Trim() + " section was successfully change status to disabled";
                            }
                            else
                            {
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_05_Hospitality_SingleUser_RegisterForms_Section Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GroupID + "') And (Section_ID = '" + SecID + "')");
                                ResSTR = "The " + DT.Rows[0][0].ToString().Trim() + " section was successfully change status to active";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified section is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving section information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_RetrieveSection(string GroupID, string SecID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 26;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GroupID = GroupID.Trim();
                SecID = SecID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Name,Icon,Row_Index,Width From Users_05_Hospitality_SingleUser_RegisterForms_Section Where (Group_ID = '" + GroupID + "') And (Section_ID = '" + SecID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            ResSTR = DT.Rows[0][0].ToString().Trim() + "#" + DT.Rows[0][1].ToString().Trim() + "#" + DT.Rows[0][2].ToString().Trim() + "#" + DT.Rows[0][3].ToString().Trim();
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified section is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving section information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_SaveEditSection(string GroupID, string SecID, string Sec_Name, string Sec_Icon, string Sec_Index, string Sec_Col)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 26;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    GroupID = GroupID.Trim();
                    SecID = SecID.Trim();
                    Sec_Name = Sec_Name.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    Sec_Icon = Sec_Icon.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    Sec_Index = Sec_Index.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    Sec_Col = Sec_Col.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    if ((GroupID == "0") || (GroupID == ""))
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading form information";
                    }
                    else
                    {
                        DataTable DT = new DataTable();
                        DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Section_ID From Users_05_Hospitality_SingleUser_RegisterForms_Section Where (Section_ID <> '" + SecID + "') And (Group_ID = '" + GroupID + "') And (Name = '" + Sec_Name + "') And (Removed = '0')");
                        if (DT.Rows != null)
                        {
                            if (DT.Rows.Count == 0)
                            {
                                string InsDate = Sq.Sql_Date();
                                string InsTime = Sq.Sql_Time();
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_05_Hospitality_SingleUser_RegisterForms_Section Set [Name] = '" + Sec_Name + "',[Icon] = '" + Sec_Icon + "',[Row_Index] = '" + Sec_Index + "',[Width] = '" + Sec_Col + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GroupID + "') And (Section_ID = '" + SecID + "')");
                                ResVal = "0"; ResSTR = "Section named " + Sec_Name + " was successfully edited";
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "The entered name of the section is duplicate, so it is not possible for you to save section changes with this name";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The server encountered an error while reloading information";
                        }
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_AddElement_A(string GroupID, string SecID, string ElementCode, string El_Title, string El_Index, string El_Col)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 29;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GroupID = GroupID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SecID = SecID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                ElementCode = ElementCode.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                El_Title = El_Title.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                El_Index = El_Index.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                try { El_Index = (int.Parse(El_Index)).ToString(); } catch (Exception) { El_Index = "0"; }
                El_Index = El_Index.Trim();
                if (El_Index == "") { El_Index = "1"; }
                El_Col = El_Col.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (El_Title == "") { ResVal = "1"; ResSTR = "Tag name not founded to add new element"; }
                if (ResVal == "0")
                {
                    DataTable DT1 = new DataTable();
                    DT1 = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_ID From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GroupID + "') And (Section_ID = '" + SecID + "') And (Element_Tag_Name = '" + El_Title + "') And (Removed = '0')");
                    if (DT1.Rows != null)
                    {
                        if (DT1.Rows.Count == 0)
                        {
                            string ElementCode_Text = "";
                            switch (ElementCode)
                            {
                                case "1": { ElementCode_Text = "Input"; break; }
                                case "2": { ElementCode_Text = "Checkbox"; break; }
                                case "3": { ElementCode_Text = "Toggle"; break; }
                                case "4": { ElementCode_Text = "RadioButton"; break; }
                                case "5": { ElementCode_Text = "Dropdown"; break; }
                                case "6": { ElementCode_Text = "Upload"; break; }
                                case "7": { ElementCode_Text = "Download"; break; }
                                case "8": { ElementCode_Text = "Player"; break; }
                                case "9": { ElementCode_Text = "Image"; break; }
                                case "10": { ElementCode_Text = "T&C"; break; }
                                case "11": { ElementCode_Text = "Text"; break; }
                                case "12": { ElementCode_Text = "Text List"; break; }
                                case "13": { ElementCode_Text = "Divider"; break; }
                                case "14": { ElementCode_Text = "Group Input"; break; }
                            }
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            DataTable DT2 = new DataTable();
                            DT2 = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_06_Hospitality_SingleUser_RegisterForms_Elements OUTPUT Inserted.Element_ID Values ('" + GroupID + "','" + SecID + "','" + ElementCode + "','" + ElementCode_Text + "','" + El_Title + "','" + El_Index + "','" + El_Col + "','1','Active','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','')");
                            ResSTR = DT2.Rows[0][0].ToString().Trim();
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The entered name of the element is duplicate, so it is not possible for you to add new element with this name";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_AddElement_B(string GroupID, string SecID, string ELID, string El_Icon, string El_PreText, string El_LinkAddress, string El_Label)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 29;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GroupID = GroupID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SecID = SecID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                ELID = ELID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                El_Icon = El_Icon.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                El_PreText = El_PreText.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                El_LinkAddress = El_LinkAddress.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                El_Label = El_Label.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    string InsDate = Sq.Sql_Date();
                    string InsTime = Sq.Sql_Time();
                    Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [WebLink_Icon] = '" + El_Icon + "',[WebLink_Prelink_Text] = '" + El_PreText + "',[WebLink_Address] = '" + El_LinkAddress + "',[WebLink_Label] = '" + El_Label + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GroupID + "') And (Section_ID = '" + SecID + "') And (Element_ID = '" + ELID + "')");
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_AddElement_C(string GroupID, string SecID, string ELID, string El_Icon, string El_Label, string El_PreText, string El_AfterText, string El_Description)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 29;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GroupID = GroupID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SecID = SecID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                ELID = ELID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                El_Icon = El_Icon.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                El_Label = El_Label.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                El_PreText = El_PreText.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                El_AfterText = El_AfterText.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                El_Description = El_Description.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    string InsDate = Sq.Sql_Date();
                    string InsTime = Sq.Sql_Time();
                    Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [AttachFile_Icon] = '" + El_Icon + "',[AttachFile_Label] = '" + El_Label + "',[AttachFile_Text_Before] = '" + El_PreText + "',[AttachFile_Text_After] = '" + El_AfterText + "',[AttachFile_Description] = '" + El_Description + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GroupID + "') And (Section_ID = '" + SecID + "') And (Element_ID = '" + ELID + "')");
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_AddElement_D(string GroupID, string SecID, string ELID, HttpPostedFileBase AtFile)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 29;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GroupID = GroupID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SecID = SecID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                ELID = ELID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    string File_Name = "";
                    string File_Type = "";
                    string File_Size = "0";
                    if (AtFile != null)
                    {
                        File_Name = Pb.GetFile_Name(AtFile.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim());
                        File_Type = Pb.GetFile_Type(AtFile.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim());
                        File_Size = Pb.GetFileSize_String(AtFile.ContentLength);
                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + ELID);
                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                        AtFile.SaveAs(MainPath + "/" + "AttachedFile.IDV");
                    }
                    string InsDate = Sq.Sql_Date();
                    string InsTime = Sq.Sql_Time();
                    Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [AttachFile_FileName] = '" + File_Name + "',[AttachFile_FileType] = '" + File_Type + "',[AttachFile_FileSize] = '" + File_Size + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GroupID + "') And (Section_ID = '" + SecID + "') And (Element_ID = '" + ELID + "')");
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "0"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_AddElement_E(string GroupID, string SecID, string ELID, string El_Icon, string El_Title, string El_Description)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 29;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GroupID = GroupID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SecID = SecID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                ELID = ELID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                El_Icon = El_Icon.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                El_Title = El_Title.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                El_Description = El_Description.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    string InsDate = Sq.Sql_Date();
                    string InsTime = Sq.Sql_Time();
                    Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [Footer_Icon] = '" + El_Icon + "',[Footer_Title] = '" + El_Title + "',[Footer_Description] = '" + El_Description + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GroupID + "') And (Section_ID = '" + SecID + "') And (Element_ID = '" + ELID + "')");
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_AddElement_F(string G, string S, string E, string P, string A1, string A2, string A3, string A4, string A5, string A6, string A7, string A8, string A9, string A10, string A11, string A12, string A13, string A14, string A15, string A16, string A17, string A18, string A19, string A20, string A21, string A22, string A23, string A24, string A25, string A26, string A27, string A28, string A29, string A30, string A31, string A32, string A33, string A34, string A35, string A36, string A37, string A38, string A39, string A40, string A41, string A42, string A43, string A44, string A45, string A46, string A47, string A48, string A49, string A50, string A51, string A52, string A53, string A54, string A55, string A56, string A57, string A58, string A59, string A60, string A61, string A62, string A63, string A64, string A65, string A66, string A67, string A68, string A69, string A70, string A71, string A72, string A73, string A74, string A75, string A76, string A77, string A78, string A79, string A80, HttpPostedFileBase AF1, HttpPostedFileBase AF2, HttpPostedFileBase AF3, HttpPostedFileBase AF4, HttpPostedFileBase AF5, HttpPostedFileBase AF6, HttpPostedFileBase AF7, HttpPostedFileBase AF8)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 29;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    string SqlQuery = "";
                    G = G.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    S = S.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    E = E.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    P = P.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    if (A1 != null) { A1 = A1.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A2 != null) { A2 = A2.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A3 != null) { A3 = A3.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A4 != null) { A4 = A4.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A5 != null) { A5 = A5.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A6 != null) { A6 = A6.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A7 != null) { A7 = A7.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A8 != null) { A8 = A8.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A9 != null) { A9 = A9.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A10 != null) { A10 = A10.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A11 != null) { A11 = A11.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A12 != null) { A12 = A12.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A13 != null) { A13 = A13.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A14 != null) { A14 = A14.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A15 != null) { A15 = A15.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A16 != null) { A16 = A16.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A17 != null) { A17 = A17.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A18 != null) { A18 = A18.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A19 != null) { A19 = A19.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A20 != null) { A20 = A20.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A21 != null) { A21 = A21.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A22 != null) { A22 = A22.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A23 != null) { A23 = A23.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A24 != null) { A24 = A24.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A25 != null) { A25 = A25.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A26 != null) { A26 = A26.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A27 != null) { A27 = A27.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A28 != null) { A28 = A28.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A29 != null) { A29 = A29.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A30 != null) { A30 = A30.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A31 != null) { A31 = A31.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A32 != null) { A32 = A32.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A33 != null) { A33 = A33.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A34 != null) { A34 = A34.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A35 != null) { A35 = A35.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A36 != null) { A36 = A36.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A37 != null) { A37 = A37.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A38 != null) { A38 = A38.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A39 != null) { A39 = A39.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A40 != null) { A40 = A40.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A41 != null) { A41 = A41.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A42 != null) { A42 = A42.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A43 != null) { A43 = A43.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A44 != null) { A44 = A44.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A45 != null) { A45 = A45.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A46 != null) { A46 = A46.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A47 != null) { A47 = A47.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A48 != null) { A48 = A48.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A49 != null) { A49 = A49.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A50 != null) { A50 = A50.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A51 != null) { A51 = A51.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A52 != null) { A52 = A52.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A53 != null) { A53 = A53.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A54 != null) { A54 = A54.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A55 != null) { A55 = A55.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A56 != null) { A56 = A56.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A57 != null) { A57 = A57.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A58 != null) { A58 = A58.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A59 != null) { A59 = A59.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A60 != null) { A60 = A60.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A61 != null) { A61 = A61.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A62 != null) { A62 = A62.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A63 != null) { A63 = A63.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A64 != null) { A64 = A64.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A65 != null) { A65 = A65.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A66 != null) { A66 = A66.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A67 != null) { A67 = A67.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A68 != null) { A68 = A68.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A69 != null) { A69 = A69.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A70 != null) { A70 = A70.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A71 != null) { A71 = A71.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A72 != null) { A72 = A72.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A73 != null) { A73 = A73.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A74 != null) { A74 = A74.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A75 != null) { A75 = A75.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A76 != null) { A76 = A76.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A77 != null) { A77 = A77.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A78 != null) { A78 = A78.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A79 != null) { A79 = A79.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A80 != null) { A80 = A80.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A1 != null) { SqlQuery += ",[ATT1] = '" + A1 + "'"; }
                    if (A2 != null) { SqlQuery += ",[ATT2] = '" + A2 + "'"; }
                    if (A3 != null) { SqlQuery += ",[ATT3] = '" + A3 + "'"; }
                    if (A4 != null) { SqlQuery += ",[ATT4] = '" + A4 + "'"; }
                    if (A5 != null) { SqlQuery += ",[ATT5] = '" + A5 + "'"; }
                    if (A6 != null) { SqlQuery += ",[ATT6] = '" + A6 + "'"; }
                    if (A7 != null) { SqlQuery += ",[ATT7] = '" + A7 + "'"; }
                    if (A8 != null) { SqlQuery += ",[ATT8] = '" + A8 + "'"; }
                    if (A9 != null) { SqlQuery += ",[ATT9] = '" + A9 + "'"; }
                    if (A10 != null) { SqlQuery += ",[ATT10] = '" + A10 + "'"; }
                    if (A11 != null) { SqlQuery += ",[ATT11] = '" + A11 + "'"; }
                    if (A12 != null) { SqlQuery += ",[ATT12] = '" + A12 + "'"; }
                    if (A13 != null) { SqlQuery += ",[ATT13] = '" + A13 + "'"; }
                    if (A14 != null) { SqlQuery += ",[ATT14] = '" + A14 + "'"; }
                    if (A15 != null) { SqlQuery += ",[ATT15] = '" + A15 + "'"; }
                    if (A16 != null) { SqlQuery += ",[ATT16] = '" + A16 + "'"; }
                    if (A17 != null) { SqlQuery += ",[ATT17] = '" + A17 + "'"; }
                    if (A18 != null) { SqlQuery += ",[ATT18] = '" + A18 + "'"; }
                    if (A19 != null) { SqlQuery += ",[ATT19] = '" + A19 + "'"; }
                    if (A20 != null) { SqlQuery += ",[ATT20] = '" + A20 + "'"; }
                    if (A21 != null) { SqlQuery += ",[ATT21] = '" + A21 + "'"; }
                    if (A22 != null) { SqlQuery += ",[ATT22] = '" + A22 + "'"; }
                    if (A23 != null) { SqlQuery += ",[ATT23] = '" + A23 + "'"; }
                    if (A24 != null) { SqlQuery += ",[ATT24] = '" + A24 + "'"; }
                    if (A25 != null) { SqlQuery += ",[ATT25] = '" + A25 + "'"; }
                    if (A26 != null) { SqlQuery += ",[ATT26] = '" + A26 + "'"; }
                    if (A27 != null) { SqlQuery += ",[ATT27] = '" + A27 + "'"; }
                    if (A28 != null) { SqlQuery += ",[ATT28] = '" + A28 + "'"; }
                    if (A29 != null) { SqlQuery += ",[ATT29] = '" + A29 + "'"; }
                    if (A30 != null) { SqlQuery += ",[ATT30] = '" + A30 + "'"; }
                    if (A31 != null) { SqlQuery += ",[ATT31] = '" + A31 + "'"; }
                    if (A32 != null) { SqlQuery += ",[ATT32] = '" + A32 + "'"; }
                    if (A33 != null) { SqlQuery += ",[ATT33] = '" + A33 + "'"; }
                    if (A34 != null) { SqlQuery += ",[ATT34] = '" + A34 + "'"; }
                    if (A35 != null) { SqlQuery += ",[ATT35] = '" + A35 + "'"; }
                    if (A36 != null) { SqlQuery += ",[ATT36] = '" + A36 + "'"; }
                    if (A37 != null) { SqlQuery += ",[ATT37] = '" + A37 + "'"; }
                    if (A38 != null) { SqlQuery += ",[ATT38] = '" + A38 + "'"; }
                    if (A39 != null) { SqlQuery += ",[ATT39] = '" + A39 + "'"; }
                    if (A40 != null) { SqlQuery += ",[ATT40] = '" + A40 + "'"; }
                    if (A41 != null) { SqlQuery += ",[ATT41] = '" + A41 + "'"; }
                    if (A42 != null) { SqlQuery += ",[ATT42] = '" + A42 + "'"; }
                    if (A43 != null) { SqlQuery += ",[ATT43] = '" + A43 + "'"; }
                    if (A44 != null) { SqlQuery += ",[ATT44] = '" + A44 + "'"; }
                    if (A45 != null) { SqlQuery += ",[ATT45] = '" + A45 + "'"; }
                    if (A46 != null) { SqlQuery += ",[ATT46] = '" + A46 + "'"; }
                    if (A47 != null) { SqlQuery += ",[ATT47] = '" + A47 + "'"; }
                    if (A48 != null) { SqlQuery += ",[ATT48] = '" + A48 + "'"; }
                    if (A49 != null) { SqlQuery += ",[ATT49] = '" + A49 + "'"; }
                    if (A50 != null) { SqlQuery += ",[ATT50] = '" + A50 + "'"; }
                    if (A51 != null) { SqlQuery += ",[ATT51] = '" + A51 + "'"; }
                    if (A52 != null) { SqlQuery += ",[ATT52] = '" + A52 + "'"; }
                    if (A53 != null) { SqlQuery += ",[ATT53] = '" + A53 + "'"; }
                    if (A54 != null) { SqlQuery += ",[ATT54] = '" + A54 + "'"; }
                    if (A55 != null) { SqlQuery += ",[ATT55] = '" + A55 + "'"; }
                    if (A56 != null) { SqlQuery += ",[ATT56] = '" + A56 + "'"; }
                    if (A57 != null) { SqlQuery += ",[ATT57] = '" + A57 + "'"; }
                    if (A58 != null) { SqlQuery += ",[ATT58] = '" + A58 + "'"; }
                    if (A59 != null) { SqlQuery += ",[ATT59] = '" + A59 + "'"; }
                    if (A60 != null) { SqlQuery += ",[ATT60] = '" + A60 + "'"; }
                    if (A61 != null) { SqlQuery += ",[ATT61] = '" + A61 + "'"; }
                    if (A62 != null) { SqlQuery += ",[ATT62] = '" + A62 + "'"; }
                    if (A63 != null) { SqlQuery += ",[ATT63] = '" + A63 + "'"; }
                    if (A64 != null) { SqlQuery += ",[ATT64] = '" + A64 + "'"; }
                    if (A65 != null) { SqlQuery += ",[ATT65] = '" + A65 + "'"; }
                    if (A66 != null) { SqlQuery += ",[ATT66] = '" + A66 + "'"; }
                    if (A67 != null) { SqlQuery += ",[ATT67] = '" + A67 + "'"; }
                    if (A68 != null) { SqlQuery += ",[ATT68] = '" + A68 + "'"; }
                    if (A69 != null) { SqlQuery += ",[ATT69] = '" + A69 + "'"; }
                    if (A70 != null) { SqlQuery += ",[ATT70] = '" + A70 + "'"; }
                    if (A71 != null) { SqlQuery += ",[ATT71] = '" + A71 + "'"; }
                    if (A72 != null) { SqlQuery += ",[ATT72] = '" + A72 + "'"; }
                    if (A73 != null) { SqlQuery += ",[ATT73] = '" + A73 + "'"; }
                    if (A74 != null) { SqlQuery += ",[ATT74] = '" + A74 + "'"; }
                    if (A75 != null) { SqlQuery += ",[ATT75] = '" + A75 + "'"; }
                    if (A76 != null) { SqlQuery += ",[ATT76] = '" + A76 + "'"; }
                    if (A77 != null) { SqlQuery += ",[ATT77] = '" + A77 + "'"; }
                    if (A78 != null) { SqlQuery += ",[ATT78] = '" + A78 + "'"; }
                    if (A79 != null) { SqlQuery += ",[ATT79] = '" + A79 + "'"; }
                    if (A80 != null) { SqlQuery += ",[ATT80] = '" + A80 + "'"; }
                    switch (P)
                    {
                        case "7":
                            {
                                if (AF1 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT10] = '" + Pb.GetFile_Name(AF1.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT11] = '" + Pb.GetFile_Type(AF1.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT12] = '" + Pb.GetFileSize_String(AF1.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF1.SaveAs(MainPath + "/D" + P + ".IDV");
                                    }
                                    catch (Exception) { }
                                }
                                break;
                            }
                        case "8":
                            {
                                if (AF1 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT15] = '" + Pb.GetFile_Name(AF1.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT16] = '" + Pb.GetFile_Type(AF1.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT17] = '" + Pb.GetFileSize_String(AF1.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF1.SaveAs(MainPath + "/P" + P + ".IDV");
                                    }
                                    catch (Exception) { }
                                }
                                break;
                            }
                        case "9":
                            {
                                if (AF1 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT32] = '" + Pb.GetFile_Name(AF1.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT33] = '" + Pb.GetFile_Type(AF1.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT34] = '" + Pb.GetFileSize_String(AF1.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF1.SaveAs(MainPath + "/I" + P + "_1.IDV");
                                    }
                                    catch (Exception) { }
                                }
                                if (AF2 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT35] = '" + Pb.GetFile_Name(AF2.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT36] = '" + Pb.GetFile_Type(AF2.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT37] = '" + Pb.GetFileSize_String(AF2.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF2.SaveAs(MainPath + "/I" + P + "_2.IDV");
                                    }
                                    catch (Exception) { }
                                }
                                if (AF3 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT38] = '" + Pb.GetFile_Name(AF3.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT39] = '" + Pb.GetFile_Type(AF3.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT40] = '" + Pb.GetFileSize_String(AF3.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF3.SaveAs(MainPath + "/I" + P + "_3.IDV");
                                    }
                                    catch (Exception) { }
                                }
                                if (AF4 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT41] = '" + Pb.GetFile_Name(AF4.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT42] = '" + Pb.GetFile_Type(AF4.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT43] = '" + Pb.GetFileSize_String(AF4.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF4.SaveAs(MainPath + "/I" + P + "_4.IDV");
                                    }
                                    catch (Exception) { }
                                }
                                if (AF5 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT44] = '" + Pb.GetFile_Name(AF5.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT45] = '" + Pb.GetFile_Type(AF5.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT46] = '" + Pb.GetFileSize_String(AF5.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF5.SaveAs(MainPath + "/I" + P + "_5.IDV");
                                    }
                                    catch (Exception) { }
                                }
                                if (AF6 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT47] = '" + Pb.GetFile_Name(AF6.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT48] = '" + Pb.GetFile_Type(AF6.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT49] = '" + Pb.GetFileSize_String(AF6.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF6.SaveAs(MainPath + "/I" + P + "_6.IDV");
                                    }
                                    catch (Exception) { }
                                }
                                if (AF7 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT50] = '" + Pb.GetFile_Name(AF7.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT51] = '" + Pb.GetFile_Type(AF7.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT52] = '" + Pb.GetFileSize_String(AF7.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF7.SaveAs(MainPath + "/I" + P + "_7.IDV");
                                    }
                                    catch (Exception) { }
                                }
                                if (AF8 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT53] = '" + Pb.GetFile_Name(AF8.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT54] = '" + Pb.GetFile_Type(AF8.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT55] = '" + Pb.GetFileSize_String(AF8.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF8.SaveAs(MainPath + "/I" + P + "_8.IDV");
                                    }
                                    catch (Exception) { }
                                }
                                break;
                            }
                    }
                    Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [Last_Update_ID] = '" + UID + "'" + SqlQuery.Trim() + " Where (Group_ID = '" + G + "') And (Section_ID = '" + S + "') And (Element_ID = '" + E + "')");
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_ElementIndex(string GID, string SID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 13;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SID = SID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Max(Element_Row_Index) From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            long NowIndex = 0;
                            try { NowIndex = long.Parse(DT.Rows[0][0].ToString().Trim()); } catch (Exception) { }
                            NowIndex++;
                            ResSTR = NowIndex.ToString();
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The server encountered an error while reloading information";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_GridElements(string GID, string SID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 13;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    SID = SID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_ID,Element_Tag_Name,Element_Type_Text,Element_Row_Index,Element_Width,Status_Code From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Removed = '0') Order By Element_Row_Index,Element_Tag_Name");
                    if (DT.Rows != null)
                    {
                        foreach (DataRow RW in DT.Rows)
                        {
                            ResSTR += "<tr>";
                            ResSTR += "<td>" + RW[1].ToString().Trim() + "</td>";
                            ResSTR += "<td style=\"text-align:center\">" + RW[2].ToString().Trim() + "</td>";
                            ResSTR += "<td style=\"text-align:center\">" + RW[3].ToString().Trim() + "</td>";
                            ResSTR += "<td style=\"text-align:center\">" + RW[4].ToString().Trim() + "</td>";
                            if (RW[5].ToString().Trim() == "1")
                            {
                                ResSTR += "<td style=\"text-align:center\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px\">Active</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td style=\"text-align:center\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px\">Disabled</div>";
                                ResSTR += "</td>";
                            }
                            ResSTR += "<td style=\"text-align:center\">";
                            ResSTR += "<div class=\"btn-group dropleft\">";
                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                            ResSTR += "</button>";
                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Elements_Remove('" + RW[0].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove element</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Elements_ChangeStatus('" + RW[0].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Elements_Edit_Tag('" + RW[0].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit element tag</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Elements_Edit_Properties('" + RW[0].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit element properties</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Elements_Edit_AttachFile('" + RW[0].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit element attach file</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Elements_Edit_WeblinkFooter('" + RW[0].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit element weblink \\ footer</a>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_RemoveElement(string GroupID, string SecID, string ELID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 30;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GroupID = GroupID.Trim();
                SecID = SecID.Trim();
                ELID = ELID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_Tag_Name From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GroupID + "') And (Section_ID  = '" + SecID + "') And (Element_ID = '" + ELID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GroupID + "') And (Section_ID  = '" + SecID + "') And (Element_ID = '" + ELID + "')");
                            ResSTR = "The " + DT.Rows[0][0].ToString().Trim() + " element was successfully deleted";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified element is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving element information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_ChangeStatusElement(string GroupID, string SecID, string ELID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 31;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GroupID = GroupID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SecID = SecID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                ELID = ELID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_Tag_Name,Status_Code From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GroupID + "') And (Section_ID  = '" + SecID + "') And (Element_ID = '" + ELID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (DT.Rows[0][1].ToString().Trim() == "1")
                            {
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GroupID + "') And (Section_ID  = '" + SecID + "') And (Element_ID = '" + ELID + "')");
                                ResSTR = "The " + DT.Rows[0][0].ToString().Trim() + " element was successfully change status to disabled";
                            }
                            else
                            {
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GroupID + "') And (Section_ID  = '" + SecID + "') And (Element_ID = '" + ELID + "')");
                                ResSTR = "The " + DT.Rows[0][0].ToString().Trim() + " element was successfully change status to active";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified element is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving element information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_RetrieveElement_Tag(string GID, string SID, string EID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 32;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SID = SID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                EID = EID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_Tag_Name,Element_Row_Index,Element_Width From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            ResSTR = DT.Rows[0][0].ToString().Trim() + "#" + DT.Rows[0][1].ToString().Trim() + "#" + DT.Rows[0][2].ToString().Trim();
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified element is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving element information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_SaveEdit_Element_Tag(string GID, string SID, string EID, string ElName, string ElIndex, string ElWidth)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 32;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SID = SID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                EID = EID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                ElName = ElName.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                ElIndex = ElIndex.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                ElWidth = ElWidth.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (ElName == "") { ResVal = "1"; ResSTR = "It is necessary to enter the element name, to save element tag information"; }
                if ((ElIndex == "") || (ElIndex == "0")) { ResVal = "1"; ResSTR = "It is necessary to enter the element order no., to save element tag information"; }
                if ((ElWidth == "") || (ElWidth == "0")) { ElWidth = "1"; }
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_ID From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            DataTable DT1 = new DataTable();
                            DT1 = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_ID From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID <> '" + EID + "') And (Element_Tag_Name = '" + ElName + "') And (Removed = '0')");
                            if (DT1.Rows.Count == 0)
                            {
                                string InsDate = Sq.Sql_Date();
                                string InsTime = Sq.Sql_Time();
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [Element_Tag_Name] = '" + ElName + "',[Element_Row_Index] = '" + ElIndex + "',[Element_Width] = '" + ElWidth + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "')");
                                ResVal = "0"; ResSTR = "The element tag information successfully edited";
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "The element tag name is duplicate";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified element is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving element information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_RetrieveElement_WLFoot(string GID, string SID, string EID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 33;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SID = SID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                EID = EID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_Tag_Name,WebLink_Icon,WebLink_Prelink_Text,WebLink_Address,WebLink_Label,Footer_Icon,Footer_Title,Footer_Description From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            ResSTR = DT.Rows[0][0].ToString().Trim() + "#" + DT.Rows[0][1].ToString().Trim() + "#" + DT.Rows[0][2].ToString().Trim() + "#" + DT.Rows[0][3].ToString().Trim() + "#" + DT.Rows[0][4].ToString().Trim() + "#" + DT.Rows[0][5].ToString().Trim() + "#" + DT.Rows[0][6].ToString().Trim() + "#" + DT.Rows[0][7].ToString().Trim();
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified element is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving element information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_SaveEdit_Element_WLFoot(string GID, string SID, string EID, string W1, string W2, string W3, string W4, string F1, string F2, string F3)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 33;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SID = SID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                EID = EID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                W1 = W1.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                W2 = W2.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                W3 = W3.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                W4 = W4.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                F1 = F1.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                F2 = F2.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                F3 = F3.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_ID From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [WebLink_Icon] = '" + W1 + "',[WebLink_Prelink_Text] = '" + W2 + "',[WebLink_Address] = '" + W3 + "',[WebLink_Label] = '" + W4 + "',[Footer_Icon] = '" + F1 + "',[Footer_Title] = '" + F2 + "',[Footer_Description] = '" + F3 + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "')");
                            ResVal = "0"; ResSTR = "The element weblink\\footer information successfully edited";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified element is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving element information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_RetrieveElement_AttachFile(string GID, string SID, string EID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 34;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SID = SID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                EID = EID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_Tag_Name,AttachFile_Icon,AttachFile_Label,AttachFile_Text_Before,AttachFile_Text_After,AttachFile_Description From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            ResSTR = DT.Rows[0][0].ToString().Trim() + "#" + DT.Rows[0][1].ToString().Trim() + "#" + DT.Rows[0][2].ToString().Trim() + "#" + DT.Rows[0][3].ToString().Trim() + "#" + DT.Rows[0][4].ToString().Trim() + "#" + DT.Rows[0][5].ToString().Trim();
                            var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + EID);
                            if (System.IO.File.Exists(MainPath + "/" + "AttachedFile.IDV") == true)
                            {
                                ResSTR += "#1";
                            }
                            else
                            {
                                ResSTR += "#0";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified element is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving element information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_SaveEdit_Element_AttachFile(string GID, string SID, string EID, string A1, string A2, string A3, string A4, string A5, HttpPostedFileBase A6)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 34;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SID = SID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                EID = EID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A1 = A1.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A2 = A2.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A3 = A3.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A4 = A4.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A5 = A5.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_ID From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string File_Name = "";
                            string File_Type = "";
                            string File_Size = "0";
                            if (A6 != null)
                            {
                                File_Name = Pb.GetFile_Name(A6.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim());
                                File_Type = Pb.GetFile_Type(A6.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim());
                                File_Size = Pb.GetFileSize_String(A6.ContentLength);
                                var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + EID);
                                if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                try { if (System.IO.File.Exists(MainPath + "/" + "AttachedFile.IDV") == true) { System.IO.File.Delete(MainPath + "/" + "AttachedFile.IDV"); } } catch (Exception) { }
                                A6.SaveAs(MainPath + "/" + "AttachedFile.IDV");
                            }
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (File_Name.Trim() != "")
                            {
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [AttachFile_Icon] = '" + A1 + "',[AttachFile_Label] = '" + A2 + "',[AttachFile_Text_Before] = '" + A3 + "',[AttachFile_Text_After] = '" + A4 + "',[AttachFile_Description] = '" + A5 + "',[AttachFile_FileName] = '" + File_Name + "',[AttachFile_FileType] = '" + File_Type + "',[AttachFile_FileSize] = '" + File_Size + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "')");
                            }
                            else
                            {
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [AttachFile_Icon] = '" + A1 + "',[AttachFile_Label] = '" + A2 + "',[AttachFile_Text_Before] = '" + A3 + "',[AttachFile_Text_After] = '" + A4 + "',[AttachFile_Description] = '" + A5 + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "')");
                            }
                            ResVal = "0"; ResSTR = "The element attached file information successfully edited";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified element is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving element information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "0"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult DownloadAttachFileRF()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 35;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return new HttpNotFoundResult(); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                if (RouteData.Values["id"] == null) { return new HttpNotFoundResult(); }
                int FFnd = 0;
                string FileAdd = RouteData.Values["id"].ToString().Trim();
                string GroupID = "0";
                string SecID = "0";
                string ElID = "0";
                string FileName = "";
                string FileType = "";
                string FileContent = "";
                string[] SepData = FileAdd.Trim().Split('-');
                if (SepData[0].ToString().Trim() == "RegForm")
                {
                    GroupID = SepData[1].ToString().Trim();
                    SecID = SepData[2].ToString().Trim();
                    ElID = SepData[3].ToString().Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select AttachFile_FileName,AttachFile_FileType,AttachFile_FileSize From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GroupID + "') And (Section_ID = '" + SecID + "') And (Element_ID = '" + ElID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            FileName = DT.Rows[0][0].ToString().Trim();
                            FileType = DT.Rows[0][1].ToString().Trim();

                            var Server_Path = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + ElID);
                            if (System.IO.File.Exists(Server_Path + "/AttachedFile.IDV") == true)
                            {
                                FFnd = 1;
                                string FilePathLST = Server_Path + "/AttachedFile.IDV";
                                FileContent = MTM.GetMimeType(FileType).Trim();
                                return File(FilePathLST, FileContent, (FileName + "." + FileType).Trim());
                            }
                            if (FFnd == 0) { return new HttpNotFoundResult(); }
                            return new HttpNotFoundResult();
                        }
                        else
                        {
                            return new HttpNotFoundResult();
                        }
                    }
                    else
                    {
                        return new HttpNotFoundResult();
                    }
                }
                else
                {
                    return new HttpNotFoundResult();
                }
            }
            catch (Exception) { return new HttpNotFoundResult(); }
        }

        [HttpPost]
        public JsonResult RegisterForms_Remove_AttachFile(string GID, string SID, string EID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 36;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SID = SID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                EID = EID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_Tag_Name From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + EID);
                            if (System.IO.File.Exists(MainPath + "/" + "AttachedFile.IDV") == true)
                            {
                                System.IO.File.Delete(MainPath + "/" + "AttachedFile.IDV");
                            }
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [AttachFile_FileName] = '',[AttachFile_FileType] = '',[AttachFile_FileSize] = '',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "') And (Removed = '0')");
                            ResVal = "0"; ResSTR = "The specified element, attached file successfully removed";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified element is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving element information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_RemoveAll_AttachFile(string GID, string SID, string EID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 36;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SID = SID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                EID = EID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_Tag_Name From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + EID);
                            if (System.IO.File.Exists(MainPath + "/" + "AttachedFile.IDV") == true)
                            {
                                System.IO.File.Delete(MainPath + "/" + "AttachedFile.IDV");
                            }
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [AttachFile_Icon] = '',[AttachFile_Label] = '',[AttachFile_Text_Before] = '',[AttachFile_Text_After] = '',[AttachFile_Description] = '',[AttachFile_FileName] = '',[AttachFile_FileType] = '',[AttachFile_FileSize] = '',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "') And (Removed = '0')");
                            ResVal = "0"; ResSTR = "The specified element, attached file and all attributes successfully removed";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified element is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving element information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_RetrieveElement_Properties(string GID, string SID, string EID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 37;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SID = SID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                EID = EID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_Tag_Name,Element_Type_Code,ATT1,ATT2,ATT3,ATT4,ATT5,ATT6,ATT7,ATT8,ATT9,ATT10,ATT11,ATT12,ATT13,ATT14,ATT15,ATT16,ATT17,ATT18,ATT19,ATT20,ATT21,ATT22,ATT23,ATT24,ATT25,ATT26,ATT27,ATT28,ATT29,ATT30,ATT31,ATT32,ATT33,ATT34,ATT35,ATT36,ATT37,ATT38,ATT39,ATT40,ATT41,ATT42,ATT43,ATT44,ATT45,ATT46,ATT47,ATT48,ATT49,ATT50,ATT51,ATT52,ATT53,ATT54,ATT55,ATT56,ATT57,ATT58,ATT59,ATT60,ATT61,ATT62,ATT63,ATT64,ATT65,ATT66,ATT67,ATT68,ATT69,ATT70,ATT71,ATT72,ATT73,ATT74,ATT75,ATT76,ATT77,ATT78,ATT79,ATT80 From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            ResSTR = DT.Rows[0][0].ToString().Trim();
                            for (int i = 1; i < 82; i++)
                            {
                                ResSTR += "#" + DT.Rows[0][i].ToString().Trim();
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified element is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving element information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_ElementProperties_Save(string G, string S, string E, string P, string A1, string A2, string A3, string A4, string A5, string A6, string A7, string A8, string A9, string A10, string A11, string A12, string A13, string A14, string A15, string A16, string A17, string A18, string A19, string A20, string A21, string A22, string A23, string A24, string A25, string A26, string A27, string A28, string A29, string A30, string A31, string A32, string A33, string A34, string A35, string A36, string A37, string A38, string A39, string A40, string A41, string A42, string A43, string A44, string A45, string A46, string A47, string A48, string A49, string A50, string A51, string A52, string A53, string A54, string A55, string A56, string A57, string A58, string A59, string A60, string A61, string A62, string A63, string A64, string A65, string A66, string A67, string A68, string A69, string A70, string A71, string A72, string A73, string A74, string A75, string A76, string A77, string A78, string A79, string A80, HttpPostedFileBase AF1, HttpPostedFileBase AF2, HttpPostedFileBase AF3, HttpPostedFileBase AF4, HttpPostedFileBase AF5, HttpPostedFileBase AF6, HttpPostedFileBase AF7, HttpPostedFileBase AF8)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 37;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    string SqlQuery = "";
                    G = G.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    S = S.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    E = E.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    P = P.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    if (A1 != null) { A1 = A1.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A2 != null) { A2 = A2.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A3 != null) { A3 = A3.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A4 != null) { A4 = A4.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A5 != null) { A5 = A5.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A6 != null) { A6 = A6.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A7 != null) { A7 = A7.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A8 != null) { A8 = A8.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A9 != null) { A9 = A9.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A10 != null) { A10 = A10.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A11 != null) { A11 = A11.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A12 != null) { A12 = A12.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A13 != null) { A13 = A13.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A14 != null) { A14 = A14.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A15 != null) { A15 = A15.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A16 != null) { A16 = A16.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A17 != null) { A17 = A17.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A18 != null) { A18 = A18.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A19 != null) { A19 = A19.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A20 != null) { A20 = A20.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A21 != null) { A21 = A21.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A22 != null) { A22 = A22.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A23 != null) { A23 = A23.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A24 != null) { A24 = A24.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A25 != null) { A25 = A25.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A26 != null) { A26 = A26.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A27 != null) { A27 = A27.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A28 != null) { A28 = A28.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A29 != null) { A29 = A29.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A30 != null) { A30 = A30.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A31 != null) { A31 = A31.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A32 != null) { A32 = A32.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A33 != null) { A33 = A33.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A34 != null) { A34 = A34.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A35 != null) { A35 = A35.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A36 != null) { A36 = A36.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A37 != null) { A37 = A37.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A38 != null) { A38 = A38.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A39 != null) { A39 = A39.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A40 != null) { A40 = A40.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A41 != null) { A41 = A41.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A42 != null) { A42 = A42.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A43 != null) { A43 = A43.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A44 != null) { A44 = A44.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A45 != null) { A45 = A45.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A46 != null) { A46 = A46.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A47 != null) { A47 = A47.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A48 != null) { A48 = A48.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A49 != null) { A49 = A49.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A50 != null) { A50 = A50.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A51 != null) { A51 = A51.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A52 != null) { A52 = A52.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A53 != null) { A53 = A53.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A54 != null) { A54 = A54.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A55 != null) { A55 = A55.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A56 != null) { A56 = A56.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A57 != null) { A57 = A57.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A58 != null) { A58 = A58.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A59 != null) { A59 = A59.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A60 != null) { A60 = A60.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A61 != null) { A61 = A61.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A62 != null) { A62 = A62.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A63 != null) { A63 = A63.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A64 != null) { A64 = A64.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A65 != null) { A65 = A65.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A66 != null) { A66 = A66.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A67 != null) { A67 = A67.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A68 != null) { A68 = A68.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A69 != null) { A69 = A69.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A70 != null) { A70 = A70.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A71 != null) { A71 = A71.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A72 != null) { A72 = A72.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A73 != null) { A73 = A73.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A74 != null) { A74 = A74.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A75 != null) { A75 = A75.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A76 != null) { A76 = A76.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A77 != null) { A77 = A77.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A78 != null) { A78 = A78.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A79 != null) { A79 = A79.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A80 != null) { A80 = A80.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (A1 != null) { SqlQuery += ",[ATT1] = '" + A1 + "'"; }
                    if (A2 != null) { SqlQuery += ",[ATT2] = '" + A2 + "'"; }
                    if (A3 != null) { SqlQuery += ",[ATT3] = '" + A3 + "'"; }
                    if (A4 != null) { SqlQuery += ",[ATT4] = '" + A4 + "'"; }
                    if (A5 != null) { SqlQuery += ",[ATT5] = '" + A5 + "'"; }
                    if (A6 != null) { SqlQuery += ",[ATT6] = '" + A6 + "'"; }
                    if (A7 != null) { SqlQuery += ",[ATT7] = '" + A7 + "'"; }
                    if (A8 != null) { SqlQuery += ",[ATT8] = '" + A8 + "'"; }
                    if (A9 != null) { SqlQuery += ",[ATT9] = '" + A9 + "'"; }
                    if (A10 != null) { SqlQuery += ",[ATT10] = '" + A10 + "'"; }
                    if (A11 != null) { SqlQuery += ",[ATT11] = '" + A11 + "'"; }
                    if (A12 != null) { SqlQuery += ",[ATT12] = '" + A12 + "'"; }
                    if (A13 != null) { SqlQuery += ",[ATT13] = '" + A13 + "'"; }
                    if (A14 != null) { SqlQuery += ",[ATT14] = '" + A14 + "'"; }
                    if (A15 != null) { SqlQuery += ",[ATT15] = '" + A15 + "'"; }
                    if (A16 != null) { SqlQuery += ",[ATT16] = '" + A16 + "'"; }
                    if (A17 != null) { SqlQuery += ",[ATT17] = '" + A17 + "'"; }
                    if (A18 != null) { SqlQuery += ",[ATT18] = '" + A18 + "'"; }
                    if (A19 != null) { SqlQuery += ",[ATT19] = '" + A19 + "'"; }
                    if (A20 != null) { SqlQuery += ",[ATT20] = '" + A20 + "'"; }
                    if (A21 != null) { SqlQuery += ",[ATT21] = '" + A21 + "'"; }
                    if (A22 != null) { SqlQuery += ",[ATT22] = '" + A22 + "'"; }
                    if (A23 != null) { SqlQuery += ",[ATT23] = '" + A23 + "'"; }
                    if (A24 != null) { SqlQuery += ",[ATT24] = '" + A24 + "'"; }
                    if (A25 != null) { SqlQuery += ",[ATT25] = '" + A25 + "'"; }
                    if (A26 != null) { SqlQuery += ",[ATT26] = '" + A26 + "'"; }
                    if (A27 != null) { SqlQuery += ",[ATT27] = '" + A27 + "'"; }
                    if (A28 != null) { SqlQuery += ",[ATT28] = '" + A28 + "'"; }
                    if (A29 != null) { SqlQuery += ",[ATT29] = '" + A29 + "'"; }
                    if (A30 != null) { SqlQuery += ",[ATT30] = '" + A30 + "'"; }
                    if (A31 != null) { SqlQuery += ",[ATT31] = '" + A31 + "'"; }
                    if (A32 != null) { SqlQuery += ",[ATT32] = '" + A32 + "'"; }
                    if (A33 != null) { SqlQuery += ",[ATT33] = '" + A33 + "'"; }
                    if (A34 != null) { SqlQuery += ",[ATT34] = '" + A34 + "'"; }
                    if (A35 != null) { SqlQuery += ",[ATT35] = '" + A35 + "'"; }
                    if (A36 != null) { SqlQuery += ",[ATT36] = '" + A36 + "'"; }
                    if (A37 != null) { SqlQuery += ",[ATT37] = '" + A37 + "'"; }
                    if (A38 != null) { SqlQuery += ",[ATT38] = '" + A38 + "'"; }
                    if (A39 != null) { SqlQuery += ",[ATT39] = '" + A39 + "'"; }
                    if (A40 != null) { SqlQuery += ",[ATT40] = '" + A40 + "'"; }
                    if (A41 != null) { SqlQuery += ",[ATT41] = '" + A41 + "'"; }
                    if (A42 != null) { SqlQuery += ",[ATT42] = '" + A42 + "'"; }
                    if (A43 != null) { SqlQuery += ",[ATT43] = '" + A43 + "'"; }
                    if (A44 != null) { SqlQuery += ",[ATT44] = '" + A44 + "'"; }
                    if (A45 != null) { SqlQuery += ",[ATT45] = '" + A45 + "'"; }
                    if (A46 != null) { SqlQuery += ",[ATT46] = '" + A46 + "'"; }
                    if (A47 != null) { SqlQuery += ",[ATT47] = '" + A47 + "'"; }
                    if (A48 != null) { SqlQuery += ",[ATT48] = '" + A48 + "'"; }
                    if (A49 != null) { SqlQuery += ",[ATT49] = '" + A49 + "'"; }
                    if (A50 != null) { SqlQuery += ",[ATT50] = '" + A50 + "'"; }
                    if (A51 != null) { SqlQuery += ",[ATT51] = '" + A51 + "'"; }
                    if (A52 != null) { SqlQuery += ",[ATT52] = '" + A52 + "'"; }
                    if (A53 != null) { SqlQuery += ",[ATT53] = '" + A53 + "'"; }
                    if (A54 != null) { SqlQuery += ",[ATT54] = '" + A54 + "'"; }
                    if (A55 != null) { SqlQuery += ",[ATT55] = '" + A55 + "'"; }
                    if (A56 != null) { SqlQuery += ",[ATT56] = '" + A56 + "'"; }
                    if (A57 != null) { SqlQuery += ",[ATT57] = '" + A57 + "'"; }
                    if (A58 != null) { SqlQuery += ",[ATT58] = '" + A58 + "'"; }
                    if (A59 != null) { SqlQuery += ",[ATT59] = '" + A59 + "'"; }
                    if (A60 != null) { SqlQuery += ",[ATT60] = '" + A60 + "'"; }
                    if (A61 != null) { SqlQuery += ",[ATT61] = '" + A61 + "'"; }
                    if (A62 != null) { SqlQuery += ",[ATT62] = '" + A62 + "'"; }
                    if (A63 != null) { SqlQuery += ",[ATT63] = '" + A63 + "'"; }
                    if (A64 != null) { SqlQuery += ",[ATT64] = '" + A64 + "'"; }
                    if (A65 != null) { SqlQuery += ",[ATT65] = '" + A65 + "'"; }
                    if (A66 != null) { SqlQuery += ",[ATT66] = '" + A66 + "'"; }
                    if (A67 != null) { SqlQuery += ",[ATT67] = '" + A67 + "'"; }
                    if (A68 != null) { SqlQuery += ",[ATT68] = '" + A68 + "'"; }
                    if (A69 != null) { SqlQuery += ",[ATT69] = '" + A69 + "'"; }
                    if (A70 != null) { SqlQuery += ",[ATT70] = '" + A70 + "'"; }
                    if (A71 != null) { SqlQuery += ",[ATT71] = '" + A71 + "'"; }
                    if (A72 != null) { SqlQuery += ",[ATT72] = '" + A72 + "'"; }
                    if (A73 != null) { SqlQuery += ",[ATT73] = '" + A73 + "'"; }
                    if (A74 != null) { SqlQuery += ",[ATT74] = '" + A74 + "'"; }
                    if (A75 != null) { SqlQuery += ",[ATT75] = '" + A75 + "'"; }
                    if (A76 != null) { SqlQuery += ",[ATT76] = '" + A76 + "'"; }
                    if (A77 != null) { SqlQuery += ",[ATT77] = '" + A77 + "'"; }
                    if (A78 != null) { SqlQuery += ",[ATT78] = '" + A78 + "'"; }
                    if (A79 != null) { SqlQuery += ",[ATT79] = '" + A79 + "'"; }
                    if (A80 != null) { SqlQuery += ",[ATT80] = '" + A80 + "'"; }
                    switch (P)
                    {
                        case "7":
                            {
                                if (AF1 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT10] = '" + Pb.GetFile_Name(AF1.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT11] = '" + Pb.GetFile_Type(AF1.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT12] = '" + Pb.GetFileSize_String(AF1.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF1.SaveAs(MainPath + "/D" + P + ".IDV");
                                    }
                                    catch (Exception) { }
                                }
                                break;
                            }
                        case "8":
                            {
                                if (AF1 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT15] = '" + Pb.GetFile_Name(AF1.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT16] = '" + Pb.GetFile_Type(AF1.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT17] = '" + Pb.GetFileSize_String(AF1.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF1.SaveAs(MainPath + "/P" + P + ".IDV");
                                    }
                                    catch (Exception) { }
                                }
                                break;
                            }
                        case "9":
                            {
                                if (AF1 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT32] = '" + Pb.GetFile_Name(AF1.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT33] = '" + Pb.GetFile_Type(AF1.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT34] = '" + Pb.GetFileSize_String(AF1.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF1.SaveAs(MainPath + "/I" + P + "_1.IDV");
                                    }
                                    catch (Exception) { }
                                }
                                if (AF2 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT35] = '" + Pb.GetFile_Name(AF2.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT36] = '" + Pb.GetFile_Type(AF2.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT37] = '" + Pb.GetFileSize_String(AF2.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF2.SaveAs(MainPath + "/I" + P + "_2.IDV");
                                    }
                                    catch (Exception) { }
                                }
                                if (AF3 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT38] = '" + Pb.GetFile_Name(AF3.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT39] = '" + Pb.GetFile_Type(AF3.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT40] = '" + Pb.GetFileSize_String(AF3.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF3.SaveAs(MainPath + "/I" + P + "_3.IDV");
                                    }
                                    catch (Exception) { }
                                }
                                if (AF4 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT41] = '" + Pb.GetFile_Name(AF4.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT42] = '" + Pb.GetFile_Type(AF4.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT43] = '" + Pb.GetFileSize_String(AF4.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF4.SaveAs(MainPath + "/I" + P + "_4.IDV");
                                    }
                                    catch (Exception) { }
                                }
                                if (AF5 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT44] = '" + Pb.GetFile_Name(AF5.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT45] = '" + Pb.GetFile_Type(AF5.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT46] = '" + Pb.GetFileSize_String(AF5.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF5.SaveAs(MainPath + "/I" + P + "_5.IDV");
                                    }
                                    catch (Exception) { }
                                }
                                if (AF6 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT47] = '" + Pb.GetFile_Name(AF6.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT48] = '" + Pb.GetFile_Type(AF6.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT49] = '" + Pb.GetFileSize_String(AF6.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF6.SaveAs(MainPath + "/I" + P + "_6.IDV");
                                    }
                                    catch (Exception) { }
                                }
                                if (AF7 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT50] = '" + Pb.GetFile_Name(AF7.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT51] = '" + Pb.GetFile_Type(AF7.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT52] = '" + Pb.GetFileSize_String(AF7.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF7.SaveAs(MainPath + "/I" + P + "_7.IDV");
                                    }
                                    catch (Exception) { }
                                }
                                if (AF8 != null)
                                {
                                    try
                                    {
                                        SqlQuery += ",[ATT53] = '" + Pb.GetFile_Name(AF8.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT54] = '" + Pb.GetFile_Type(AF8.FileName.ToString().Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim()) + "'";
                                        SqlQuery += ",[ATT55] = '" + Pb.GetFileSize_String(AF8.ContentLength) + "'";
                                        var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + E);
                                        if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                        AF8.SaveAs(MainPath + "/I" + P + "_8.IDV");
                                    }
                                    catch (Exception) { }
                                }
                                break;
                            }
                    }
                    string InsDate = Sq.Sql_Date();
                    string InsTime = Sq.Sql_Time();
                    Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "'" + SqlQuery.Trim() + " Where (Group_ID = '" + G + "') And (Section_ID = '" + S + "') And (Element_ID = '" + E + "')");
                    ResVal = "0"; ResSTR = "The element properties information successfully edited";
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult DownloadDnldRF()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 35;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return new HttpNotFoundResult(); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                if (RouteData.Values["id"] == null) { return new HttpNotFoundResult(); }
                int FFnd = 0;
                string FileAdd = RouteData.Values["id"].ToString().Trim();
                string GroupID = "0";
                string SecID = "0";
                string ElID = "0";
                string ElType = "0";
                string FileName = "";
                string FileType = "";
                string FileContent = "";
                string[] SepData = FileAdd.Trim().Split('-');
                if ((SepData[0].ToString().Trim() == "RegForm") && (SepData[4].ToString().Trim() == "7"))
                {
                    GroupID = SepData[1].ToString().Trim();
                    SecID = SepData[2].ToString().Trim();
                    ElID = SepData[3].ToString().Trim();
                    ElType = SepData[4].ToString().Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ATT10,ATT11,ATT12 From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GroupID + "') And (Section_ID = '" + SecID + "') And (Element_ID = '" + ElID + "') And (Element_Type_Code = '7') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            FileName = DT.Rows[0][0].ToString().Trim();
                            FileType = DT.Rows[0][1].ToString().Trim();
                            var Server_Path = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + ElID);
                            if (System.IO.File.Exists(Server_Path + "/D" + ElType + ".IDV") == true)
                            {
                                FFnd = 1;
                                string FilePathLST = Server_Path + "/D" + ElType + ".IDV";
                                FileContent = MTM.GetMimeType(FileType).Trim();
                                return File(FilePathLST, FileContent, (FileName + "." + FileType).Trim());
                            }
                            if (FFnd == 0) { return new HttpNotFoundResult(); }
                            return new HttpNotFoundResult();
                        }
                        else
                        {
                            return new HttpNotFoundResult();
                        }
                    }
                    else
                    {
                        return new HttpNotFoundResult();
                    }
                }
                else
                {
                    return new HttpNotFoundResult();
                }
            }
            catch (Exception) { return new HttpNotFoundResult(); }
        }

        [HttpPost]
        public JsonResult RegisterForms_Remove_Dnld(string GID, string SID, string EID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 36;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SID = SID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                EID = EID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_Tag_Name From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + EID);
                            if (System.IO.File.Exists(MainPath + "/D7.IDV") == true)
                            {
                                System.IO.File.Delete(MainPath + "/D7.IDV");
                            }
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [ATT10] = '',[ATT11] = '',[ATT12] = '',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "') And (Removed = '0')");
                            ResVal = "0"; ResSTR = "The specified element, download file successfully removed";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified element is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving element information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult DownloadPlyrRF()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 35;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return new HttpNotFoundResult(); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                if (RouteData.Values["id"] == null) { return new HttpNotFoundResult(); }
                int FFnd = 0;
                string FileAdd = RouteData.Values["id"].ToString().Trim();
                string GroupID = "0";
                string SecID = "0";
                string ElID = "0";
                string ElType = "0";
                string FileName = "";
                string FileType = "";
                string FileContent = "";
                string[] SepData = FileAdd.Trim().Split('-');
                if ((SepData[0].ToString().Trim() == "RegForm") && (SepData[4].ToString().Trim() == "8"))
                {
                    GroupID = SepData[1].ToString().Trim();
                    SecID = SepData[2].ToString().Trim();
                    ElID = SepData[3].ToString().Trim();
                    ElType = SepData[4].ToString().Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ATT15,ATT16,ATT17 From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GroupID + "') And (Section_ID = '" + SecID + "') And (Element_ID = '" + ElID + "') And (Element_Type_Code = '8') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            FileName = DT.Rows[0][0].ToString().Trim();
                            FileType = DT.Rows[0][1].ToString().Trim();

                            var Server_Path = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + ElID);
                            if (System.IO.File.Exists(Server_Path + "/P" + ElType + ".IDV") == true)
                            {
                                FFnd = 1;
                                string FilePathLST = Server_Path + "/P" + ElType + ".IDV";
                                FileContent = MTM.GetMimeType(FileType).Trim();
                                return File(FilePathLST, FileContent, (FileName + "." + FileType).Trim());
                            }
                            if (FFnd == 0) { return new HttpNotFoundResult(); }
                            return new HttpNotFoundResult();
                        }
                        else
                        {
                            return new HttpNotFoundResult();
                        }
                    }
                    else
                    {
                        return new HttpNotFoundResult();
                    }
                }
                else
                {
                    return new HttpNotFoundResult();
                }
            }
            catch (Exception) { return new HttpNotFoundResult(); }
        }

        public ActionResult VideoPlayerRF(string FID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 35;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return new HttpNotFoundResult(); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                int FFnd = 0;
                string FileAdd = FID;
                string GroupID = "0";
                string SecID = "0";
                string ElID = "0";
                string ElType = "0";
                string FileName = "";
                string FileType = "";
                string FileContent = "";
                string[] SepData = FileAdd.Trim().Split('-');
                if ((SepData[0].ToString().Trim() == "RegForm") && (SepData[4].ToString().Trim() == "8"))
                {
                    GroupID = SepData[1].ToString().Trim();
                    SecID = SepData[2].ToString().Trim();
                    ElID = SepData[3].ToString().Trim();
                    ElType = SepData[4].ToString().Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ATT15,ATT16,ATT17 From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GroupID + "') And (Section_ID = '" + SecID + "') And (Element_ID = '" + ElID + "') And (Element_Type_Code = '8') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            FileName = DT.Rows[0][0].ToString().Trim();
                            FileType = DT.Rows[0][1].ToString().Trim();

                            var Server_Path = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + ElID);
                            if (System.IO.File.Exists(Server_Path + "/P" + ElType + ".IDV") == true)
                            {
                                FFnd = 1;
                                string FilePathLST = Server_Path + "/P" + ElType + ".IDV";
                                FileContent = MTM.GetMimeType(FileType).Trim();
                                var fileStream = new FileStream(FilePathLST, FileMode.Open, FileAccess.Read);
                                return new FileStreamResult(fileStream, FileContent);
                            }
                            if (FFnd == 0) { return new HttpNotFoundResult(); }
                            return new HttpNotFoundResult();
                        }
                        else
                        {
                            return new HttpNotFoundResult();
                        }
                    }
                    else
                    {
                        return new HttpNotFoundResult();
                    }
                }
                else
                {
                    return new HttpNotFoundResult();
                }
            }
            catch (Exception) { return new HttpNotFoundResult(); }
        }

        [HttpPost]
        public JsonResult RegisterForms_Remove_Player(string GID, string SID, string EID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 36;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SID = SID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                EID = EID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_Tag_Name From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + EID);
                            if (System.IO.File.Exists(MainPath + "/P8.IDV") == true)
                            {
                                System.IO.File.Delete(MainPath + "/P8.IDV");
                            }
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [ATT15] = '',[ATT16] = '',[ATT17] = '',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "') And (Removed = '0')");
                            ResVal = "0"; ResSTR = "The specified element, player file successfully removed";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified element is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving element information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult DownloadImgRF()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 35;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return new HttpNotFoundResult(); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                if (RouteData.Values["id"] == null) { return new HttpNotFoundResult(); }
                int FFnd = 0;
                string FileAdd = RouteData.Values["id"].ToString().Trim();
                string GroupID = "0";
                string SecID = "0";
                string ElID = "0";
                string ElType = "0";
                string ElLocalCode = "0";
                string FileName = "";
                string FileType = "";
                string FileContent = "";
                string[] SepData = FileAdd.Trim().Split('-');
                if ((SepData[0].ToString().Trim() == "RegForm") && (SepData[4].ToString().Trim() == "9"))
                {
                    GroupID = SepData[1].ToString().Trim();
                    SecID = SepData[2].ToString().Trim();
                    ElID = SepData[3].ToString().Trim();
                    ElType = SepData[4].ToString().Trim();
                    ElLocalCode = SepData[5].ToString().Trim();
                    int ELC = 0;
                    ELC = int.Parse(ElLocalCode);
                    DataTable DT = new DataTable();
                    string T1 = "ATT";
                    string T2 = "ATT";
                    string T3 = "ATT";
                    T1 = T1 + (32 + ((ELC - 1) * 3)).ToString();
                    T2 = T2 + (33 + ((ELC - 1) * 3)).ToString();
                    T3 = T3 + (34 + ((ELC - 1) * 3)).ToString();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select " + T1 + "," + T2 + "," + T3 + " From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GroupID + "') And (Section_ID = '" + SecID + "') And (Element_ID = '" + ElID + "') And (Element_Type_Code = '9') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            FileName = DT.Rows[0][0].ToString().Trim();
                            FileType = DT.Rows[0][1].ToString().Trim();
                            var Server_Path = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + ElID);
                            if (System.IO.File.Exists(Server_Path + "/I" + ElType + "_" + ElLocalCode + ".IDV") == true)
                            {
                                FFnd = 1;
                                string FilePathLST = Server_Path + "/I" + ElType + "_" + ElLocalCode + ".IDV";
                                FileContent = MTM.GetMimeType(FileType).Trim();
                                return File(FilePathLST, FileContent, (FileName + "." + FileType).Trim());
                            }
                            if (FFnd == 0) { return new HttpNotFoundResult(); }
                            return new HttpNotFoundResult();
                        }
                        else
                        {
                            return new HttpNotFoundResult();
                        }
                    }
                    else
                    {
                        return new HttpNotFoundResult();
                    }
                }
                else
                {
                    return new HttpNotFoundResult();
                }
            }
            catch (Exception) { return new HttpNotFoundResult(); }
        }

        [HttpPost]
        public JsonResult RegisterForms_Remove_Img(string GID, string SID, string EID, string IID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 36;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                SID = SID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                EID = EID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                IID = IID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_Tag_Name From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            var MainPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + EID);
                            if (System.IO.File.Exists(MainPath + "/I9_" + IID + ".IDV") == true)
                            {
                                System.IO.File.Delete(MainPath + "/I9_" + IID + ".IDV");
                            }
                            int ELC = 0;
                            ELC = int.Parse(IID);
                            string T1 = "ATT";
                            string T2 = "ATT";
                            string T3 = "ATT";
                            T1 = T1 + (32 + ((ELC - 1) * 3)).ToString();
                            T2 = T2 + (33 + ((ELC - 1) * 3)).ToString();
                            T3 = T3 + (34 + ((ELC - 1) * 3)).ToString();
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_06_Hospitality_SingleUser_RegisterForms_Elements Set [" + T1 + "] = '',[" + T2 + "] = '',[" + T3 + "] = '',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Element_ID = '" + EID + "') And (Removed = '0')");
                            ResVal = "0"; ResSTR = "The specified element, image #" + IID + " file successfully removed";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified element is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving element information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_PreviewCode(string CID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                int MenuCode1 = 38;
                int MenuCode2 = 39;
                if ((AAuth.User_Authentication_Action(MenuCode1) == false) && (AAuth.User_Authentication_Action(MenuCode2) == false)) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                CID = CID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    ResSTR = Cry.EncodeEmas("ARASH" + CID + "MASIHI" + Pb.Make_Security_Code(20));
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_GroupInput(string EID, string Cnt)
        {
            try
            {
                string ResVal = "0";
                string ResSTR = "";
                EID = EID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                Cnt = Cnt.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DataTable DT = new DataTable();
                DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select * From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Element_ID = '" + EID + "') And (Status_Code = '1') And (Removed = '0')");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count == 1)
                    {
                        ResSTR = "";
                        string MasterInput = "ELMNTGI";
                        ResSTR += "<div class=\"form-inline\" style=\"width:100%;margin-top:20px\" id=\"GroupInput_" + EID + "_" + Cnt + "\">";
                        ResSTR += "<div class=\"col-lg-12\" style=\"text-align:right\">";
                        ResSTR += "<i class=\"fa fa-trash text-danger\" style=\"cursor:pointer;font-size:20px\" onclick=\"RemoveGroupinput('GroupInput_" + EID + "_" + Cnt + "')\"></i>";
                        ResSTR += "</div>";
                        if (DT.Rows[0][37].ToString().Trim() != "")
                        {
                            ResSTR += "<div class=\"col-lg-" + DT.Rows[0][36].ToString().Trim() + "\">";
                            ResSTR += "<div class=\"text-bold-300 font-medium-1\" style=\"margin-top:10px;margin-left:5px;width:100%\">";
                            ResSTR += DT.Rows[0][37].ToString().Trim();
                            ResSTR += "</div>";
                            ResSTR += "<fieldset class=\"form-group position-relative\" style=\"width:100%\">";
                            ResSTR += "<input type=\"text\" class=\"form-control " + MasterInput + "\" id=\"GroupInput_" + DT.Rows[0][0].ToString().Trim() + "_" + Cnt + "_1\" placeholder=\"" + DT.Rows[0][38].ToString().Trim() + "\" style=\"width:100%\">";
                            ResSTR += "</fieldset>";
                            ResSTR += "</div>";
                            MasterInput = "";
                        }
                        if (DT.Rows[0][41].ToString().Trim() != "")
                        {
                            ResSTR += "<div class=\"col-lg-" + DT.Rows[0][40].ToString().Trim() + "\">";
                            ResSTR += "<div class=\"text-bold-300 font-medium-1\" style=\"margin-top:10px;margin-left:5px;width:100%\">";
                            ResSTR += DT.Rows[0][41].ToString().Trim();
                            ResSTR += "</div>";
                            ResSTR += "<fieldset class=\"form-group position-relative\" style=\"width:100%\">";
                            ResSTR += "<input type=\"text\" class=\"form-control " + MasterInput + "\" id=\"GroupInput_" + DT.Rows[0][0].ToString().Trim() + "_" + Cnt + "_2\" placeholder=\"" + DT.Rows[0][42].ToString().Trim() + "\" style=\"width:100%\">";
                            ResSTR += "</fieldset>";
                            ResSTR += "</div>";
                            MasterInput = "";
                        }
                        if (DT.Rows[0][45].ToString().Trim() != "")
                        {
                            ResSTR += "<div class=\"col-lg-" + DT.Rows[0][44].ToString().Trim() + "\">";
                            ResSTR += "<div class=\"text-bold-300 font-medium-1\" style=\"margin-top:10px;margin-left:5px;width:100%\">";
                            ResSTR += DT.Rows[0][45].ToString().Trim();
                            ResSTR += "</div>";
                            ResSTR += "<fieldset class=\"form-group position-relative\" style=\"width:100%\">";
                            ResSTR += "<input type=\"text\" class=\"form-control " + MasterInput + "\" id=\"GroupInput_" + DT.Rows[0][0].ToString().Trim() + "_" + Cnt + "_3\" placeholder=\"" + DT.Rows[0][46].ToString().Trim() + "\" style=\"width:100%\">";
                            ResSTR += "</fieldset>";
                            ResSTR += "</div>";
                            MasterInput = "";
                        }
                        if (DT.Rows[0][49].ToString().Trim() != "")
                        {
                            ResSTR += "<div class=\"col-lg-" + DT.Rows[0][48].ToString().Trim() + "\">";
                            ResSTR += "<div class=\"text-bold-300 font-medium-1\" style=\"margin-top:10px;margin-left:5px;width:100%\">";
                            ResSTR += DT.Rows[0][49].ToString().Trim();
                            ResSTR += "</div>";
                            ResSTR += "<fieldset class=\"form-group position-relative\" style=\"width:100%\">";
                            ResSTR += "<input type=\"text\" class=\"form-control " + MasterInput + "\" id=\"GroupInput_" + DT.Rows[0][0].ToString().Trim() + "_" + Cnt + "_4\" placeholder=\"" + DT.Rows[0][50].ToString().Trim() + "\" style=\"width:100%\">";
                            ResSTR += "</fieldset>";
                            ResSTR += "</div>";
                            MasterInput = "";
                        }
                        if (DT.Rows[0][53].ToString().Trim() != "")
                        {
                            ResSTR += "<div class=\"col-lg-" + DT.Rows[0][52].ToString().Trim() + "\">";
                            ResSTR += "<div class=\"text-bold-300 font-medium-1\" style=\"margin-top:10px;margin-left:5px;width:100%\">";
                            ResSTR += DT.Rows[0][53].ToString().Trim();
                            ResSTR += "</div>";
                            ResSTR += "<fieldset class=\"form-group position-relative\" style=\"width:100%\">";
                            ResSTR += "<input type=\"text\" class=\"form-control " + MasterInput + "\" id=\"GroupInput_" + DT.Rows[0][0].ToString().Trim() + "_" + Cnt + "_1\" placeholder=\"" + DT.Rows[0][54].ToString().Trim() + "\" style=\"width:100%\">";
                            ResSTR += "</fieldset>";
                            ResSTR += "</div>";
                            MasterInput = "";
                        }
                        if (DT.Rows[0][57].ToString().Trim() != "")
                        {
                            ResSTR += "<div class=\"col-lg-" + DT.Rows[0][56].ToString().Trim() + "\">";
                            ResSTR += "<div class=\"text-bold-300 font-medium-1\" style=\"margin-top:10px;margin-left:5px;width:100%\">";
                            ResSTR += DT.Rows[0][57].ToString().Trim();
                            ResSTR += "</div>";
                            ResSTR += "<fieldset class=\"form-group position-relative\" style=\"width:100%\">";
                            ResSTR += "<input type=\"text\" class=\"form-control " + MasterInput + "\" id=\"GroupInput_" + DT.Rows[0][0].ToString().Trim() + "_" + Cnt + "_1\" placeholder=\"" + DT.Rows[0][58].ToString().Trim() + "\" style=\"width:100%\">";
                            ResSTR += "</fieldset>";
                            ResSTR += "</div>";
                            MasterInput = "";
                        }
                        ResSTR += "</div>";
                    }
                    else { ResVal = "1"; }
                }
                else { ResVal = "1"; }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult RegisterForms_FormPreview()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 39;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string SectionData = Url.RequestContext.RouteData.Values["id"].ToString().Trim();
                string LockKey = Cry.DecodeEmas(SectionData);
                string ULT1 = LockKey.Substring(0, 5);
                int MasLoc = LockKey.IndexOf("MASIHI");
                string CID = LockKey.Substring(5, MasLoc - 5);
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DataTable DT_Group = new DataTable();
                ViewBag.DT_Group = null;
                ViewBag.DT_Sections = null;
                ViewBag.FormID = CID;
                DT_Group = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Name,Description,Status_Text,Template_Name,ID From Users_04_Hospitality_SingleUser_RegisterForms Where (ID = '" + CID + "') And (User_ID = '" + UID + "') And (Removed = '0')");
                if (DT_Group.Rows != null)
                {
                    if (DT_Group.Rows.Count == 1)
                    {
                        ViewBag.Google_API_Place = "";
                        try
                        {
                            DataTable DT_Google = new DataTable();
                            DT_Google = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Google_Place_Key From Setting_Basic_02_Google");
                            ViewBag.Google_API_Place = DT_Google.Rows[0][0].ToString().Trim();
                        }
                        catch (Exception) { }
                        DataTable DT_Sections = new DataTable();
                        DT_Sections = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Section_ID,Width,Icon,Name From Users_05_Hospitality_SingleUser_RegisterForms_Section Where (Group_ID = '" + CID + "') And (Status_Code = '1') And (Removed = '0') Order By Row_Index,Name");
                        ViewBag.DT_Group = DT_Group.Rows;
                        ViewBag.DT_Sections = DT_Sections.Rows;
                    }
                    else
                    {
                        return new HttpNotFoundResult();
                    }
                }
                else
                {
                    return new HttpNotFoundResult();
                }
                return View();
            }
            catch (Exception)
            {
                return new HttpNotFoundResult();
            }
        }

        [HttpGet]
        public ActionResult RegisterForms_SectionPreview()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 38;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string SectionData = Url.RequestContext.RouteData.Values["id"].ToString().Trim();
                string LockKey = Cry.DecodeEmas(SectionData);
                string ULT1 = LockKey.Substring(0, 5);
                int MasLoc = LockKey.IndexOf("MASIHI");
                string CID = LockKey.Substring(5, MasLoc - 5);
                DataTable DT_SecFinder = new DataTable();
                DT_SecFinder = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Group_ID,Name,Row_Index,Width,Status_Text From Users_05_Hospitality_SingleUser_RegisterForms_Section Where (Section_ID = '" + CID + "') And (Removed = '0')");
                if (DT_SecFinder.Rows != null)
                {
                    if (DT_SecFinder.Rows.Count == 1)
                    {
                        ViewBag.Google_API_Place = "";
                        try
                        {
                            DataTable DT_Google = new DataTable();
                            DT_Google = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Google_Place_Key From Setting_Basic_02_Google");
                            ViewBag.Google_API_Place = DT_Google.Rows[0][0].ToString().Trim();
                        }
                        catch (Exception) { }
                        string UID = "0";
                        try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                        UID = UID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                        string GroupID = "0";
                        ViewBag.SectionName = "";
                        GroupID = DT_SecFinder.Rows[0][0].ToString().Trim();
                        ViewBag.DT_SecFinder = DT_SecFinder.Rows[0];
                        DataTable DT_Group = new DataTable();
                        ViewBag.DT_Group = null;
                        ViewBag.DT_Sections = null;
                        ViewBag.FormID = GroupID;
                        DT_Group = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Name,Description,Status_Text,Template_Name,ID From Users_04_Hospitality_SingleUser_RegisterForms Where (ID = '" + GroupID + "') And (User_ID = '" + UID + "') And (Removed = '0')");
                        if (DT_Group.Rows != null)
                        {
                            if (DT_Group.Rows.Count == 1)
                            {
                                DataTable DT_Sections = new DataTable();
                                DT_Sections = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Section_ID,Width,Icon,Name From Users_05_Hospitality_SingleUser_RegisterForms_Section Where (Group_ID = '" + GroupID + "') And (Section_ID = '" + CID + "') And (Status_Code = '1') And (Removed = '0') Order By Row_Index,Name");
                                ViewBag.DT_Group = DT_Group.Rows;
                                ViewBag.DT_Sections = DT_Sections.Rows;
                            }
                            else
                            {
                                return new HttpNotFoundResult();
                            }
                        }
                        else
                        {
                            return new HttpNotFoundResult();
                        }
                        return View();
                    }
                    else
                    {
                        return new HttpNotFoundResult();
                    }
                }
                else
                {
                    return new HttpNotFoundResult();
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" });
            }
        }

        //====================================================================================================================
        [HttpGet]
        public ActionResult RegisterForms_Configuration()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 28;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string FormID = RouteData.Values["id"].ToString().Trim();
                if (FormID == null) { return RedirectToAction("RegisterForms", "Tools", new { id = "", area = "HSU_Portal" }); }
                if (FormID == "") { return RedirectToAction("RegisterForms", "Tools", new { id = "", area = "HSU_Portal" }); }
                string UserID = Session["User_UID"].ToString().Trim();
                DataTable DT = new DataTable();
                DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ID From Users_04_Hospitality_SingleUser_RegisterForms Where (UnicID = '" + FormID + "') And (User_ID = '" + UserID + "') And (Removed = '0')");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count == 1)
                    {
                        FormID = DT.Rows[0][0].ToString().Trim();
                        ViewBag.UserID = UserID;
                        ViewBag.FormID = FormID;
                        DataTable DT_Config = new DataTable();
                        DT_Config = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select * From Users_07_Hospitality_SingleUser_RegisterForms_Configuration Where (User_ID = '" + UserID + "') And (Form_ID = '" + FormID + "')");
                        if (DT_Config.Rows.Count == 1)
                        {
                            ViewBag.Config_Value = DT_Config.Rows[0];
                        }
                        else
                        {
                            ViewBag.Config_Value = null;
                        }
                    }
                    else
                    {
                        return RedirectToAction("RegisterForms", "Tools", new { id = "", area = "HSU_Portal" });
                    }
                }
                else
                {
                    return RedirectToAction("RegisterForms", "Tools", new { id = "", area = "HSU_Portal" });
                }
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("RegisterForms", "Tools", new { id = "", area = "HSU_Portal" });
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_ConfigurationSave(string U, string F, string S, string V1, string V2, string V3, string V4, string V5, string V6, string V7, string V8, string V9, string V10, string V11, string V12, string V13, string V14, string V15, string V16, string V17, string V18, string V19, string V20, string V21, string V22, string V23, string V24, string V25, string V26, string V27, string V28, string V29, string V30, string V31, string V32, string V33, string V34, string V35, string V36, string V37, string V38, string V39, string V40, string V41, HttpPostedFileBase V42, HttpPostedFileBase V43, HttpPostedFileBase V44, HttpPostedFileBase V45, HttpPostedFileBase V46, HttpPostedFileBase V47, HttpPostedFileBase V48, HttpPostedFileBase V49, HttpPostedFileBase V50, HttpPostedFileBase V51, HttpPostedFileBase V52, HttpPostedFileBase V53)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 28;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                string HSCR = "";
                string FSCR = "";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    U = U.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    F = F.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    S = S.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    if (S == "2")
                    {
                        try { HSCR = V1.Trim(); } catch (Exception) { }
                        try { FSCR = V2.Trim(); } catch (Exception) { }
                    }
                    if (V1 != null) { V1 = V1.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V2 != null) { V2 = V2.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V3 != null) { V3 = V3.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V4 != null) { V4 = V4.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V5 != null) { V5 = V5.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V6 != null) { V6 = V6.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V7 != null) { V7 = V7.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V8 != null) { V8 = V8.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V9 != null) { V9 = V9.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V10 != null) { V10 = V10.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V11 != null) { V11 = V11.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V12 != null) { V12 = V12.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V13 != null) { V13 = V13.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V14 != null) { V14 = V14.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V15 != null) { V15 = V15.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V16 != null) { V16 = V16.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V17 != null) { V17 = V17.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V18 != null) { V18 = V18.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V19 != null) { V19 = V19.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V20 != null) { V20 = V20.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V21 != null) { V21 = V21.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V22 != null) { V22 = V22.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V23 != null) { V23 = V23.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V24 != null) { V24 = V24.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V25 != null) { V25 = V25.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V26 != null) { V26 = V26.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V27 != null) { V27 = V27.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V28 != null) { V28 = V28.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V29 != null) { V29 = V29.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V30 != null) { V30 = V30.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V31 != null) { V31 = V31.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V32 != null) { V32 = V32.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V33 != null) { V33 = V33.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V34 != null) { V34 = V34.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V35 != null) { V35 = V35.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V36 != null) { V36 = V36.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V37 != null) { V37 = V37.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V38 != null) { V38 = V38.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V39 != null) { V39 = V39.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V40 != null) { V40 = V40.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    if (V41 != null) { V41 = V41.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim(); }
                    try
                    {
                        DataTable DT_TST = new DataTable();
                        DT_TST = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select User_ID From Users_07_Hospitality_SingleUser_RegisterForms_Configuration Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                        if (DT_TST.Rows.Count == 0)
                        {
                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_07_Hospitality_SingleUser_RegisterForms_Configuration Values ('" + U + "','" + F + "','0','0','','','0','0','','','0','0','','','0','0','','','0','0','','','0','0','','','0','0','','','0','0','','','0','0','','','0','0','','','','','','','','','','','','','','','','0','','','','','','','','','','','','','0','0','','0','','0','0','','','','','','','','','','','','0','','','','','','','','','','0','','','','','','0')");
                        }
                        if (DT_TST.Rows.Count > 1)
                        {
                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Delete From Users_07_Hospitality_SingleUser_RegisterForms_Configuration Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_07_Hospitality_SingleUser_RegisterForms_Configuration Values ('" + U + "','" + F + "','0','0','','','0','0','','','0','0','','','0','0','','','0','0','','','0','0','','','0','0','','','0','0','','','0','0','','','0','0','','','','','','','','','','','','','','','','0','','','','','','','','','','','','','0','0','','0','','0','0','','','','','','','','','','','','0','','','','','','','','','','0','','','','','','0')");
                        }
                    }
                    catch (Exception) { }
                    switch (S)
                    {
                        case "1":
                            {
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [URL_01_Status] = '" + V1 + "',[URL_01_Mandatory] = '" + V2 + "',[URL_01_Key] = '" + V3 + "',[URL_01_Value] = '" + V4 + "',[URL_02_Status] = '" + V5 + "',[URL_02_Mandatory] = '" + V6 + "',[URL_02_Key] = '" + V7 + "',[URL_02_Value] = '" + V8 + "',[URL_03_Status] = '" + V9 + "',[URL_03_Mandatory] = '" + V10 + "',[URL_03_Key] = '" + V11 + "',[URL_03_Value] = '" + V12 + "',[URL_04_Status] = '" + V13 + "',[URL_04_Mandatory] = '" + V14 + "',[URL_04_Key] = '" + V15 + "',[URL_04_Value] = '" + V16 + "',[URL_05_Status] = '" + V17 + "',[URL_05_Mandatory] = '" + V18 + "',[URL_05_Key] = '" + V19 + "',[URL_05_Value] = '" + V20 + "',[URL_06_Status] = '" + V21 + "',[URL_06_Mandatory] = '" + V22 + "',[URL_06_Key] = '" + V23 + "',[URL_06_Value] = '" + V24 + "',[URL_07_Status] = '" + V25 + "',[URL_07_Mandatory] = '" + V26 + "',[URL_07_Key] = '" + V27 + "',[URL_07_Value] = '" + V28 + "',[URL_08_Status] = '" + V29 + "',[URL_08_Mandatory] = '" + V30 + "',[URL_08_Key] = '" + V31 + "',[URL_08_Value] = '" + V32 + "',[URL_09_Status] = '" + V33 + "',[URL_09_Mandatory] = '" + V34 + "',[URL_09_Key] = '" + V35 + "',[URL_09_Value] = '" + V36 + "',[URL_10_Status] = '" + V37 + "',[URL_10_Mandatory] = '" + V38 + "',[URL_10_Key] = '" + V39 + "',[URL_10_Value] = '" + V40 + "',[URL_Parameter_Error_URL] = '" + V41 + "' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                break;
                            }
                        case "2":
                            {
                                var MainPath = Server.MapPath("~/Drive/Hospitality/Rgfmcon/" + F);
                                if (!Directory.Exists(MainPath)) { Directory.CreateDirectory(MainPath); }
                                DataTable DT_AFL = new DataTable();
                                DT_AFL = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Head_01_File_Name,Head_01_File_Type,Head_02_File_Name,Head_02_File_Type,Head_03_File_Name,Head_03_File_Type,Head_04_File_Name,Head_04_File_Type,Head_05_File_Name,Head_05_File_Type,Head_06_File_Name,Head_06_File_Type,Footer_01_File_Name,Footer_01_File_Type,Footer_02_File_Name,Footer_02_File_Type,Footer_03_File_Name,Footer_03_File_Type,Footer_04_File_Name,Footer_04_File_Type,Footer_05_File_Name,Footer_05_File_Type,Footer_06_File_Name,Footer_06_File_Type,Head_Script,Footer_Script From Users_07_Hospitality_SingleUser_RegisterForms_Configuration Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                try
                                {
                                    if (V42 != null)
                                    {
                                        HttpPostedFileBase FUFS = V42;
                                        string FileName = Pb.GetFile_Name(FUFS.FileName);
                                        string FileType = Pb.GetFile_Type(FUFS.FileName);
                                        try { System.IO.File.Delete(MainPath + "/" + DT_AFL.Rows[0][0].ToString().Trim() + "." + DT_AFL.Rows[0][1].ToString().Trim()); } catch (Exception) { }
                                        FUFS.SaveAs(MainPath + "/" + FileName + "." + FileType);
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Head_01_File_Name] = '',[Head_01_File_Type] = '' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Head_01_File_Name] = '" + FileName + "',[Head_01_File_Type] = '" + FileType + "' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                    }
                                }
                                catch (Exception) { }
                                try
                                {
                                    if (V43 != null)
                                    {
                                        HttpPostedFileBase FUFS = V43;
                                        string FileName = Pb.GetFile_Name(FUFS.FileName);
                                        string FileType = Pb.GetFile_Type(FUFS.FileName);
                                        try { System.IO.File.Delete(MainPath + "/" + DT_AFL.Rows[0][2].ToString().Trim() + "." + DT_AFL.Rows[0][3].ToString().Trim()); } catch (Exception) { }
                                        FUFS.SaveAs(MainPath + "/" + FileName + "." + FileType);
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Head_02_File_Name] = '',[Head_02_File_Type] = '' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Head_02_File_Name] = '" + FileName + "',[Head_02_File_Type] = '" + FileType + "' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                    }
                                }
                                catch (Exception) { }
                                try
                                {
                                    if (V44 != null)
                                    {
                                        HttpPostedFileBase FUFS = V44;
                                        string FileName = Pb.GetFile_Name(FUFS.FileName);
                                        string FileType = Pb.GetFile_Type(FUFS.FileName);
                                        try { System.IO.File.Delete(MainPath + "/" + DT_AFL.Rows[0][4].ToString().Trim() + "." + DT_AFL.Rows[0][5].ToString().Trim()); } catch (Exception) { }
                                        FUFS.SaveAs(MainPath + "/" + FileName + "." + FileType);
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Head_03_File_Name] = '',[Head_03_File_Type] = '' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Head_03_File_Name] = '" + FileName + "',[Head_03_File_Type] = '" + FileType + "' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                    }
                                }
                                catch (Exception) { }
                                try
                                {
                                    if (V45 != null)
                                    {
                                        HttpPostedFileBase FUFS = V45;
                                        string FileName = Pb.GetFile_Name(FUFS.FileName);
                                        string FileType = Pb.GetFile_Type(FUFS.FileName);
                                        try { System.IO.File.Delete(MainPath + "/" + DT_AFL.Rows[0][6].ToString().Trim() + "." + DT_AFL.Rows[0][7].ToString().Trim()); } catch (Exception) { }
                                        FUFS.SaveAs(MainPath + "/" + FileName + "." + FileType);
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Head_04_File_Name] = '',[Head_04_File_Type] = '' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Head_04_File_Name] = '" + FileName + "',[Head_04_File_Type] = '" + FileType + "' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                    }
                                }
                                catch (Exception) { }
                                try
                                {
                                    if (V46 != null)
                                    {
                                        HttpPostedFileBase FUFS = V46;
                                        string FileName = Pb.GetFile_Name(FUFS.FileName);
                                        string FileType = Pb.GetFile_Type(FUFS.FileName);
                                        try { System.IO.File.Delete(MainPath + "/" + DT_AFL.Rows[0][8].ToString().Trim() + "." + DT_AFL.Rows[0][9].ToString().Trim()); } catch (Exception) { }
                                        FUFS.SaveAs(MainPath + "/" + FileName + "." + FileType);
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Head_05_File_Name] = '',[Head_05_File_Type] = '' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Head_05_File_Name] = '" + FileName + "',[Head_05_File_Type] = '" + FileType + "' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                    }
                                }
                                catch (Exception) { }
                                try
                                {
                                    if (V47 != null)
                                    {
                                        HttpPostedFileBase FUFS = V47;
                                        string FileName = Pb.GetFile_Name(FUFS.FileName);
                                        string FileType = Pb.GetFile_Type(FUFS.FileName);
                                        try { System.IO.File.Delete(MainPath + "/" + DT_AFL.Rows[0][10].ToString().Trim() + "." + DT_AFL.Rows[0][11].ToString().Trim()); } catch (Exception) { }
                                        FUFS.SaveAs(MainPath + "/" + FileName + "." + FileType);
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Head_06_File_Name] = '',[Head_06_File_Type] = '' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Head_06_File_Name] = '" + FileName + "',[Head_06_File_Type] = '" + FileType + "' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                    }
                                }
                                catch (Exception) { }
                                try
                                {
                                    if (V48 != null)
                                    {
                                        HttpPostedFileBase FUFS = V48;
                                        string FileName = Pb.GetFile_Name(FUFS.FileName);
                                        string FileType = Pb.GetFile_Type(FUFS.FileName);
                                        try { System.IO.File.Delete(MainPath + "/" + DT_AFL.Rows[0][12].ToString().Trim() + "." + DT_AFL.Rows[0][13].ToString().Trim()); } catch (Exception) { }
                                        FUFS.SaveAs(MainPath + "/" + FileName + "." + FileType);
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Footer_01_File_Name] = '',[Footer_01_File_Type] = '' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Footer_01_File_Name] = '" + FileName + "',[Footer_01_File_Type] = '" + FileType + "' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                    }
                                }
                                catch (Exception) { }
                                try
                                {
                                    if (V49 != null)
                                    {
                                        HttpPostedFileBase FUFS = V49;
                                        string FileName = Pb.GetFile_Name(FUFS.FileName);
                                        string FileType = Pb.GetFile_Type(FUFS.FileName);
                                        try { System.IO.File.Delete(MainPath + "/" + DT_AFL.Rows[0][14].ToString().Trim() + "." + DT_AFL.Rows[0][15].ToString().Trim()); } catch (Exception) { }
                                        FUFS.SaveAs(MainPath + "/" + FileName + "." + FileType);
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Footer_02_File_Name] = '',[Footer_02_File_Type] = '' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Footer_02_File_Name] = '" + FileName + "',[Footer_02_File_Type] = '" + FileType + "' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                    }
                                }
                                catch (Exception) { }
                                try
                                {
                                    if (V50 != null)
                                    {
                                        HttpPostedFileBase FUFS = V50;
                                        string FileName = Pb.GetFile_Name(FUFS.FileName);
                                        string FileType = Pb.GetFile_Type(FUFS.FileName);
                                        try { System.IO.File.Delete(MainPath + "/" + DT_AFL.Rows[0][16].ToString().Trim() + "." + DT_AFL.Rows[0][17].ToString().Trim()); } catch (Exception) { }
                                        FUFS.SaveAs(MainPath + "/" + FileName + "." + FileType);
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Footer_03_File_Name] = '',[Footer_03_File_Type] = '' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Footer_03_File_Name] = '" + FileName + "',[Footer_03_File_Type] = '" + FileType + "' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                    }
                                }
                                catch (Exception) { }
                                try
                                {
                                    if (V51 != null)
                                    {
                                        HttpPostedFileBase FUFS = V51;
                                        string FileName = Pb.GetFile_Name(FUFS.FileName);
                                        string FileType = Pb.GetFile_Type(FUFS.FileName);
                                        try { System.IO.File.Delete(MainPath + "/" + DT_AFL.Rows[0][18].ToString().Trim() + "." + DT_AFL.Rows[0][19].ToString().Trim()); } catch (Exception) { }
                                        FUFS.SaveAs(MainPath + "/" + FileName + "." + FileType);
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Footer_04_File_Name] = '',[Footer_04_File_Type] = '' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Footer_04_File_Name] = '" + FileName + "',[Footer_04_File_Type] = '" + FileType + "' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                    }
                                }
                                catch (Exception) { }
                                try
                                {
                                    if (V52 != null)
                                    {
                                        HttpPostedFileBase FUFS = V52;
                                        string FileName = Pb.GetFile_Name(FUFS.FileName);
                                        string FileType = Pb.GetFile_Type(FUFS.FileName);
                                        try { System.IO.File.Delete(MainPath + "/" + DT_AFL.Rows[0][20].ToString().Trim() + "." + DT_AFL.Rows[0][21].ToString().Trim()); } catch (Exception) { }
                                        FUFS.SaveAs(MainPath + "/" + FileName + "." + FileType);
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Footer_05_File_Name] = '',[Footer_05_File_Type] = '' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Footer_05_File_Name] = '" + FileName + "',[Footer_05_File_Type] = '" + FileType + "' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                    }
                                }
                                catch (Exception) { }
                                try
                                {
                                    if (V53 != null)
                                    {
                                        HttpPostedFileBase FUFS = V53;
                                        string FileName = Pb.GetFile_Name(FUFS.FileName);
                                        string FileType = Pb.GetFile_Type(FUFS.FileName);
                                        try { System.IO.File.Delete(MainPath + "/" + DT_AFL.Rows[0][22].ToString().Trim() + "." + DT_AFL.Rows[0][23].ToString().Trim()); } catch (Exception) { }
                                        FUFS.SaveAs(MainPath + "/" + FileName + "." + FileType);
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Footer_06_File_Name] = '',[Footer_06_File_Type] = '' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                        Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Footer_06_File_Name] = '" + FileName + "',[Footer_06_File_Type] = '" + FileType + "' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                    }
                                }
                                catch (Exception) { }
                                try
                                {
                                    try { System.IO.File.Delete(MainPath + "/" + "HEADSCR.txt"); } catch (Exception) { }
                                    Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Head_Script] = '0' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                    if (HSCR != "")
                                    {
                                        if (!System.IO.File.Exists(MainPath + "/" + "HEADSCR.txt"))
                                        {
                                            using (StreamWriter sw = System.IO.File.CreateText(MainPath + "/" + "HEADSCR.txt"))
                                            {
                                                sw.Write(HSCR);
                                            }
                                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Head_Script] = '1' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                        }
                                    }
                                }
                                catch (Exception) { }
                                try
                                {
                                    try { System.IO.File.Delete(MainPath + "/" + "FOTRSCR.txt"); } catch (Exception) { }
                                    Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Footer_Script] = '0' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                    if (FSCR != "")
                                    {
                                        if (!System.IO.File.Exists(MainPath + "/" + "FOTRSCR.txt"))
                                        {
                                            using (StreamWriter sw = System.IO.File.CreateText(MainPath + "/" + "FOTRSCR.txt"))
                                            {
                                                sw.Write(FSCR);
                                            }
                                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Footer_Script] = '1' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                        }
                                    }
                                }
                                catch (Exception) { }
                                break;
                            }
                        case "3":
                            {
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Calback_Submit] = '" + V1 + "',[Calback_Submit_URL] = '" + V2 + "',[Callback_Process] = '" + V3 + "',[Callback_Process_URL] = '" + V4 + "',[Redirect_Submit] = '" + V5 + "',[Redirect_Auto] = '" + V6 + "',[Redirect_Caption] = '" + V7 + "',[Redirect_URL] = '" + V8 + "',[Calback_Submit_HUsername_Key] = '" + V9 + "',[Calback_Submit_HUsername_Value] = '" + V10 + "',[Calback_Submit_HPassword_Key] = '" + V11 + "',[Calback_Submit_HPassword_Value] = '" + V12 + "',[Calback_Submit2] = '" + V13 + "',[Calback_Submit_URL2] = '" + V14 + "',[Calback_Submit_HUsername_Key2] = '" + V15 + "',[Calback_Submit_HUsername_Value2] = '" + V16 + "',[Calback_Submit_HPassword_Key2] = '" + V17 + "',[Calback_Submit_HPassword_Value2] = '" + V18 + "',[Callback_Process_HUsername_Key] = '" + V19 + "',[Callback_Process_HUsername_Value] = '" + V20 + "',[Callback_Process_HPassword_Key] = '" + V21 + "',[Callback_Process_HPassword_Value] = '" + V22 + "',[Callback_Process2] = '" + V23 + "',[Callback_Process_URL2] = '" + V24 + "',[Callback_Process_HUsername_Key2] = '" + V25 + "',[Callback_Process_HUsername_Value2] = '" + V26 + "',[Callback_Process_HPassword_Key2] = '" + V27 + "',[Callback_Process_HPassword_Value2] = '" + V28 + "' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                break;
                            }
                        case "4":
                            {
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [Link_Ext_01] = '" + V1 + "',[Link_Ext_02] = '" + V2 + "',[Link_Ext_03] = '" + V3 + "',[Link_Ext_04] = '" + V4 + "',[Link_Ext_05] = '" + V5 + "' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                break;
                            }
                        case "5":
                            {
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set [ShowAsPage] = '" + V1 + "' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                break;
                            }
                    }
                    ResVal = "0"; ResSTR = "";
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_ConfigurationReload(string U, string F, string S)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 28;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    U = U.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    F = F.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    S = S.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    DataTable DT = new DataTable();
                    switch (S)
                    {
                        case "1":
                            {
                                DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select URL_01_Status,URL_01_Mandatory,URL_01_Key,URL_01_Value,URL_02_Status,URL_02_Mandatory,URL_02_Key,URL_02_Value,URL_03_Status,URL_03_Mandatory,URL_03_Key,URL_03_Value,URL_04_Status,URL_04_Mandatory,URL_04_Key,URL_04_Value,URL_05_Status,URL_05_Mandatory,URL_05_Key,URL_05_Value,URL_06_Status,URL_06_Mandatory,URL_06_Key,URL_06_Value,URL_07_Status,URL_07_Mandatory,URL_07_Key,URL_07_Value,URL_08_Status,URL_08_Mandatory,URL_08_Key,URL_08_Value,URL_09_Status,URL_09_Mandatory,URL_09_Key,URL_09_Value,URL_10_Status,URL_10_Mandatory,URL_10_Key,URL_10_Value,URL_Parameter_Error_URL From Users_07_Hospitality_SingleUser_RegisterForms_Configuration Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                if (DT.Rows.Count == 1)
                                {
                                    ResSTR = DT.Rows[0][0].ToString().Trim() + "#$#" + DT.Rows[0][1].ToString().Trim() + "#$#" + DT.Rows[0][2].ToString().Trim() + "#$#" + DT.Rows[0][3].ToString().Trim() + "#$#";
                                    ResSTR += DT.Rows[0][4].ToString().Trim() + "#$#" + DT.Rows[0][5].ToString().Trim() + "#$#" + DT.Rows[0][6].ToString().Trim() + "#$#" + DT.Rows[0][7].ToString().Trim() + "#$#";
                                    ResSTR += DT.Rows[0][8].ToString().Trim() + "#$#" + DT.Rows[0][9].ToString().Trim() + "#$#" + DT.Rows[0][10].ToString().Trim() + "#$#" + DT.Rows[0][11].ToString().Trim() + "#$#";
                                    ResSTR += DT.Rows[0][12].ToString().Trim() + "#$#" + DT.Rows[0][13].ToString().Trim() + "#$#" + DT.Rows[0][14].ToString().Trim() + "#$#" + DT.Rows[0][15].ToString().Trim() + "#$#";
                                    ResSTR += DT.Rows[0][16].ToString().Trim() + "#$#" + DT.Rows[0][17].ToString().Trim() + "#$#" + DT.Rows[0][18].ToString().Trim() + "#$#" + DT.Rows[0][19].ToString().Trim() + "#$#";
                                    ResSTR += DT.Rows[0][20].ToString().Trim() + "#$#" + DT.Rows[0][21].ToString().Trim() + "#$#" + DT.Rows[0][22].ToString().Trim() + "#$#" + DT.Rows[0][23].ToString().Trim() + "#$#";
                                    ResSTR += DT.Rows[0][24].ToString().Trim() + "#$#" + DT.Rows[0][25].ToString().Trim() + "#$#" + DT.Rows[0][26].ToString().Trim() + "#$#" + DT.Rows[0][27].ToString().Trim() + "#$#";
                                    ResSTR += DT.Rows[0][28].ToString().Trim() + "#$#" + DT.Rows[0][29].ToString().Trim() + "#$#" + DT.Rows[0][30].ToString().Trim() + "#$#" + DT.Rows[0][31].ToString().Trim() + "#$#";
                                    ResSTR += DT.Rows[0][32].ToString().Trim() + "#$#" + DT.Rows[0][33].ToString().Trim() + "#$#" + DT.Rows[0][34].ToString().Trim() + "#$#" + DT.Rows[0][35].ToString().Trim() + "#$#";
                                    ResSTR += DT.Rows[0][36].ToString().Trim() + "#$#" + DT.Rows[0][37].ToString().Trim() + "#$#" + DT.Rows[0][38].ToString().Trim() + "#$#" + DT.Rows[0][39].ToString().Trim() + "#$#";
                                    ResSTR += DT.Rows[0][40].ToString().Trim();
                                }
                                else
                                {
                                    ResSTR = "0#$#0#$# #$# #$#0#$#0#$# #$# #$#0#$#0#$# #$# #$#0#$#0#$# #$# #$#0#$#0#$# #$# #$#0#$#0#$# #$# #$#0#$#0#$# #$# #$#0#$#0#$# #$# #$#0#$#0#$# #$# #$#0#$#0#$# #$# #$# ";
                                }
                                break;
                            }
                        case "2":
                            {
                                DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Head_Script,Footer_Script,Head_01_File_Name,Head_01_File_Type,Head_02_File_Name,Head_02_File_Type,Head_03_File_Name,Head_03_File_Type,Head_04_File_Name,Head_04_File_Type,Head_05_File_Name,Head_05_File_Type,Head_06_File_Name,Head_06_File_Type,Footer_01_File_Name,Footer_01_File_Type,Footer_02_File_Name,Footer_02_File_Type,Footer_03_File_Name,Footer_03_File_Type,Footer_04_File_Name,Footer_04_File_Type,Footer_05_File_Name,Footer_05_File_Type,Footer_06_File_Name,Footer_06_File_Type From Users_07_Hospitality_SingleUser_RegisterForms_Configuration Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                if (DT.Rows.Count == 1)
                                {
                                    string HS = ""; string FS = ""; string HH = ""; string FH = "";
                                    try
                                    {
                                        for (int i = 1; i < 7; i++)
                                        {
                                            try
                                            {
                                                if (DT.Rows[0][2 * i].ToString().Trim() != "")
                                                {
                                                    HH += "<tr>";
                                                    HH += "<th scope=\"row\" style=\"text-align:center\">" + i + "</th>";
                                                    HH += "<td>" + DT.Rows[0][2 * i].ToString().Trim() + "</td>";
                                                    HH += "<td style=\"text-align:center\">" + DT.Rows[0][(2 * i) + 1].ToString().Trim() + "</td>";
                                                    HH += "<td style=\"text-align:center\"><i class=\"fa fa-trash text-danger\" style=\"cursor:pointer\" onclick=\"RemoveFilesHF('" + i + "','1')\"></i></td>";
                                                    HH += "</tr>";
                                                }
                                            }
                                            catch (Exception) { }
                                            try
                                            {
                                                if (DT.Rows[0][12 + (2 * i)].ToString().Trim() != "")
                                                {
                                                    FH += "<tr>";
                                                    FH += "<th scope=\"row\" style=\"text-align:center\">" + i + "</th>";
                                                    FH += "<td>" + DT.Rows[0][12 + (2 * i)].ToString().Trim() + "</td>";
                                                    FH += "<td style=\"text-align:center\">" + DT.Rows[0][(12 + (2 * i)) + 1].ToString().Trim() + "</td>";
                                                    FH += "<td style=\"text-align:center\"><i class=\"fa fa-trash text-danger\" style=\"cursor:pointer\" onclick=\"RemoveFilesHF('" + i + "','2')\"></i></td>";
                                                    FH += "</tr>";
                                                }
                                            }
                                            catch (Exception) { }
                                        }
                                    }
                                    catch (Exception) { }
                                    try
                                    {
                                        if (DT.Rows[0][0].ToString().Trim() == "1")
                                        {
                                            var MainPath = Server.MapPath("~/Drive/Hospitality/Rgfmcon/" + F);
                                            if (System.IO.File.Exists(MainPath + "/" + "HEADSCR.txt") == true)
                                            {
                                                string readText = System.IO.File.ReadAllText(MainPath + "/" + "HEADSCR.txt");
                                                HS = readText.Trim();
                                            }
                                        }
                                    }
                                    catch (Exception) { }
                                    try
                                    {
                                        if (DT.Rows[0][1].ToString().Trim() == "1")
                                        {
                                            var MainPath = Server.MapPath("~/Drive/Hospitality/Rgfmcon/" + F);
                                            if (System.IO.File.Exists(MainPath + "/" + "FOTRSCR.txt") == true)
                                            {
                                                string readText = System.IO.File.ReadAllText(MainPath + "/" + "FOTRSCR.txt");
                                                FS = readText.Trim();
                                            }
                                        }
                                    }
                                    catch (Exception) { }
                                    ResSTR = HS.Trim() + "#$#" + FS.Trim() + "#$#" + HH.Trim() + "#$#" + FH.Trim();
                                }
                                else
                                {
                                    ResSTR = " #$# #$# #$# ";
                                }
                                break;
                            }
                        case "3":
                            {
                                DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Calback_Submit,Calback_Submit_URL,Callback_Process,Callback_Process_URL,Redirect_Submit,Redirect_Auto,Redirect_Caption,Redirect_URL,Calback_Submit_HUsername_Key,Calback_Submit_HUsername_Value,Calback_Submit_HPassword_Key,Calback_Submit_HPassword_Value,Calback_Submit2,Calback_Submit_URL2,Calback_Submit_HUsername_Key2,Calback_Submit_HUsername_Value2,Calback_Submit_HPassword_Key2,Calback_Submit_HPassword_Value2,Callback_Process_HUsername_Key,Callback_Process_HUsername_Value,Callback_Process_HPassword_Key,Callback_Process_HPassword_Value,Callback_Process2,Callback_Process_URL2,Callback_Process_HUsername_Key2,Callback_Process_HUsername_Value2,Callback_Process_HPassword_Key2,Callback_Process_HPassword_Value2 From Users_07_Hospitality_SingleUser_RegisterForms_Configuration Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                if (DT.Rows.Count == 1)
                                {
                                    ResSTR = DT.Rows[0][0].ToString().Trim() + "#$#" + DT.Rows[0][1].ToString().Trim() + "#$#" + DT.Rows[0][2].ToString().Trim() + "#$#" + DT.Rows[0][3].ToString().Trim() + "#$#" + DT.Rows[0][4].ToString().Trim() + "#$#" + DT.Rows[0][5].ToString().Trim() + "#$#" + DT.Rows[0][6].ToString().Trim() + "#$#" + DT.Rows[0][7].ToString().Trim() + "#$#" + DT.Rows[0][8].ToString().Trim() + "#$#" + DT.Rows[0][9].ToString().Trim() + "#$#" + DT.Rows[0][10].ToString().Trim() + "#$#" + DT.Rows[0][11].ToString().Trim() + "#$#" + DT.Rows[0][12].ToString().Trim() + "#$#" + DT.Rows[0][13].ToString().Trim() + "#$#" + DT.Rows[0][14].ToString().Trim() + "#$#" + DT.Rows[0][15].ToString().Trim() + "#$#" + DT.Rows[0][16].ToString().Trim() + "#$#" + DT.Rows[0][17].ToString().Trim() + "#$#" + DT.Rows[0][18].ToString().Trim() + "#$#" + DT.Rows[0][19].ToString().Trim() + "#$#" + DT.Rows[0][20].ToString().Trim() + "#$#" + DT.Rows[0][21].ToString().Trim() + "#$#" + DT.Rows[0][22].ToString().Trim() + "#$#" + DT.Rows[0][23].ToString().Trim() + "#$#" + DT.Rows[0][24].ToString().Trim() + "#$#" + DT.Rows[0][25].ToString().Trim() + "#$#" + DT.Rows[0][26].ToString().Trim() + "#$#" + DT.Rows[0][27].ToString().Trim();
                                }
                                else
                                {
                                    ResSTR = "0#$# #$#0#$# #$#0#$#0#$# #$# #$# #$# #$# #$# #$#0#$# #$# #$# #$# #$# #$# #$# #$# #$# #$#0#$# #$# #$# #$# #$# ";
                                }
                                break;
                            }
                        case "4":
                            {
                                DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Link_Ext_01,Link_Ext_02,Link_Ext_03,Link_Ext_04,Link_Ext_05 From Users_07_Hospitality_SingleUser_RegisterForms_Configuration Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                if (DT.Rows.Count == 1)
                                {
                                    ResSTR = DT.Rows[0][0].ToString().Trim() + "#$#" + DT.Rows[0][1].ToString().Trim() + "#$#" + DT.Rows[0][2].ToString().Trim() + "#$#" + DT.Rows[0][3].ToString().Trim() + "#$#" + DT.Rows[0][4].ToString().Trim();
                                }
                                else
                                {
                                    ResSTR = " #$# #$# #$# #$# ";
                                }
                                break;
                            }
                        case "5":
                            {
                                DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ShowAsPage From Users_07_Hospitality_SingleUser_RegisterForms_Configuration Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                                if (DT.Rows.Count == 1)
                                {
                                    ResSTR = DT.Rows[0][0].ToString().Trim() + "#$#";
                                }
                                else
                                {
                                    ResSTR = "0#$#";
                                }
                                break;
                            }
                    }
                    ResVal = "0";
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RegisterForms_ConfigurationRemove(string U, string F, string HF, string IND)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 28;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    U = U.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    F = F.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    HF = HF.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    IND = IND.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select User_ID From Users_07_Hospitality_SingleUser_RegisterForms_Configuration Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                    if (DT.Rows.Count == 1)
                    {
                        string FNM = "";
                        string FTP = "";
                        if (HF == "1")
                        {
                            DataTable DTR = new DataTable();
                            DTR = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Head_0" + IND + "_File_Name,Head_0" + IND + "_File_Type From Users_07_Hospitality_SingleUser_RegisterForms_Configuration Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                            if (DTR.Rows.Count == 1)
                            {
                                FNM = DTR.Rows[0][0].ToString().Trim();
                                FTP = DTR.Rows[0][1].ToString().Trim();
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set Head_0" + IND + "_File_Name = '',Head_0" + IND + "_File_Type = '' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                            }
                        }
                        else
                        {
                            DataTable DTR = new DataTable();
                            DTR = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Footer_0" + IND + "_File_Name,Footer_0" + IND + "_File_Type From Users_07_Hospitality_SingleUser_RegisterForms_Configuration Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                            if (DTR.Rows.Count == 1)
                            {
                                FNM = DTR.Rows[0][0].ToString().Trim();
                                FTP = DTR.Rows[0][1].ToString().Trim();
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_07_Hospitality_SingleUser_RegisterForms_Configuration Set Footer_0" + IND + "_File_Name = '',Footer_0" + IND + "_File_Type = '' Where (User_ID = '" + U + "') And (Form_ID = '" + F + "')");
                            }
                        }
                        var MainPath = Server.MapPath("~/Drive/Hospitality/Rgfmcon/" + F + "/" + FNM + "." + FTP);
                        if (System.IO.File.Exists(MainPath) == true) { System.IO.File.Delete(MainPath); }
                        ResVal = "0";
                    }
                    else
                    {
                        ResVal = "1";
                        ResSTR = "File information not founded";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }
        //====================================================================================================================
    }
}


