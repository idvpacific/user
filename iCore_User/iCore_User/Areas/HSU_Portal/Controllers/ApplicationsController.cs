using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using iCore_User.Modules;
using iCore_User.Modules.SecurityAuthentication;

namespace iCore_User.Areas.HSU_Portal.Controllers
{
    public class ApplicationsController : Controller
    {
        //====================================================================================================================
        iCore_Administrator.Modules.AuthenticationTester AAuth = new iCore_Administrator.Modules.AuthenticationTester();
        iCore_Administrator.Modules.SQL_Tranceiver Sq = new iCore_Administrator.Modules.SQL_Tranceiver();
        iCore_Administrator.Modules.PublicFunctions Pb = new iCore_Administrator.Modules.PublicFunctions();
        iCore_Administrator.Modules.HSU_Application.Application_Guesty Gusty_Func = new iCore_Administrator.Modules.HSU_Application.Application_Guesty();
        //====================================================================================================================
        public ActionResult Index() { return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" }); }
        //====================================================================================================================
        [HttpGet]
        public ActionResult HSU()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 8;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" });
            }
        }

        [HttpPost]
        public JsonResult HSU_Grid()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 8;
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
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select User_FullName,Customer_FullName,Email,TrakingCode,Status_Code,Ins_Date,Last_Update_Date,Application_UID From Users_08_Hospitality_SingleUser_Application_V1 Where (User_ID = '" + UID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        foreach (DataRow RW in DT.Rows)
                        {

                            ResSTR += "<tr>";
                            ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[1].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[2].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[3].ToString().Trim() + "</td>";
                            switch (RW[4].ToString().Trim())
                            {
                                case "1":
                                    {
                                        ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">";
                                        ResSTR += "<div class=\"badge badge-pill badge-light-warning\" style=\"width:140px\">Pending</div>";
                                        ResSTR += "</td>";
                                        break;
                                    }
                                case "2":
                                    {
                                        ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">";
                                        ResSTR += "<div class=\"badge badge-pill badge-light-info\" style=\"width:140px\">Review</div>";
                                        ResSTR += "</td>";
                                        break;
                                    }
                                case "3":
                                    {
                                        ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">";
                                        ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:140px\">Failed</div>";
                                        ResSTR += "</td>";
                                        break;
                                    }
                                case "4":
                                    {
                                        ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">";
                                        ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:140px\">Passed</div>";
                                        ResSTR += "</td>";
                                        break;
                                    }
                            }
                            ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[5].ToString().Trim().Substring(0, 10) + "</td>";
                            ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[6].ToString().Trim().Substring(0, 10) + "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading direct link application information";
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
        public ActionResult HSU_New()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 9;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" });
            }
        }

        [HttpPost]
        public JsonResult HSU_New_Grid()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 9;
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
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select User_FullName,Customer_FullName,Email,TrakingCode,Status_Code,Ins_Date,Last_Update_Date,Application_UID From Users_08_Hospitality_SingleUser_Application_V1 Where (User_ID = '" + UID + "') And (Seen_User_Flag = '0') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        foreach (DataRow RW in DT.Rows)
                        {

                            ResSTR += "<tr>";
                            ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[1].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[2].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[3].ToString().Trim() + "</td>";
                            switch (RW[4].ToString().Trim())
                            {
                                case "1":
                                    {
                                        ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">";
                                        ResSTR += "<div class=\"badge badge-pill badge-light-warning\" style=\"width:140px\">Pending</div>";
                                        ResSTR += "</td>";
                                        break;
                                    }
                                case "2":
                                    {
                                        ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">";
                                        ResSTR += "<div class=\"badge badge-pill badge-light-info\" style=\"width:140px\">Review</div>";
                                        ResSTR += "</td>";
                                        break;
                                    }
                                case "3":
                                    {
                                        ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">";
                                        ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:140px\">Failed</div>";
                                        ResSTR += "</td>";
                                        break;
                                    }
                                case "4":
                                    {
                                        ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">";
                                        ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:140px\">Passed</div>";
                                        ResSTR += "</td>";
                                        break;
                                    }
                            }
                            ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[5].ToString().Trim().Substring(0, 10) + "</td>";
                            ResSTR += "<td onclick=\"HSU_Application_Show('" + RW[7].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[6].ToString().Trim().Substring(0, 10) + "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading direct link application information";
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
        public ActionResult HSU_Application()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 27;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" }); }
                string AppUID = Url.RequestContext.RouteData.Values["id"].ToString().Trim();
                DataTable DT_App = new DataTable();
                DT_App = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select * From Users_08_Hospitality_SingleUser_Application Where (App_UnicID = '" + AppUID + "') And (User_ID = '" + UID + "') And (Removed = '0')");
                if (DT_App.Rows != null)
                {
                    if (DT_App.Rows.Count == 1)
                    {
                        if (DT_App.Rows[0][26].ToString().Trim().ToLower() == "false")
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_08_Hospitality_SingleUser_Application Set [Seen_User_Flag] = '1',[Seen_User_Date] = '" + InsDate + "',[Seen_User_Time] = '" + InsTime + "' Where (ID = '" + DT_App.Rows[0][0].ToString().Trim() + "')");
                        }
                        ViewBag.DT_App = DT_App.Rows[0];
                        DataTable DT_User = new DataTable();
                        DT_User = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select * From Users_02_SingleUser Where (ID = '" + DT_App.Rows[0][3].ToString().Trim() + "')");
                        ViewBag.DT_User = DT_User.Rows[0];
                        DataTable DT_Form = new DataTable();
                        DT_Form = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select * From Users_04_Hospitality_SingleUser_RegisterForms Where (ID = '" + DT_App.Rows[0][2].ToString().Trim() + "') And (User_ID = '" + DT_App.Rows[0][3].ToString().Trim() + "')");
                        ViewBag.DT_Form = DT_Form.Rows[0];
                        try
                        {
                            DataTable DT_DataInfo = new DataTable();
                            DT_DataInfo = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select * From Users_13_Hospitality_SingleUser_Application_DataInfo Where (App_ID = '" + DT_App.Rows[0][0].ToString().Trim() + "')");
                            ViewBag.DT_DataInfo = DT_DataInfo.Rows[0];
                        }
                        catch (Exception)
                        {
                            ViewBag.DT_DataInfo = null;
                        }
                        string UserPanelURL = "";
                        UserPanelURL = System.Configuration.ConfigurationManager.AppSettings["iCore_User_URL"];
                        ViewBag.UserPanelURL = UserPanelURL;
                        string SecretKey = "";
                        SecretKey = System.Configuration.ConfigurationManager.AppSettings["iCore_API_SecretKey"];
                        ViewBag.SecretKey = SecretKey;
                        ViewBag.ISGuesty = "0";
                        if (IsGuesty(DT_App.Rows[0][0].ToString().Trim()) == true) { ViewBag.ISGuesty = "1"; }
                        DataTable DTValidation = new DataTable();
                        DTValidation = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_Type_Text,Element_Tag_Name,Result_Text From Users_12_Hospitality_SingleUser_Application_Validation_V Where (App_ID = '" + DT_App.Rows[0][0].ToString().Trim() + "')");
                        ViewBag.ValiLog = "";
                        string ErrVal = "";
                        try
                        {
                            foreach (DataRow RW in DTValidation.Rows)
                            {
                                ErrVal += "- " + RW[1].ToString().Trim() + " [" + RW[0].ToString().Trim() + "] : " + RW[2].ToString().Trim() + "$";
                            }
                            ErrVal = ErrVal.Trim();
                            if (ErrVal.Substring(ErrVal.Length - 1, 1) == "$") { ErrVal = ErrVal.Substring(0, ErrVal.Length - 1); }
                            ErrVal = ErrVal.Replace("$", "<br />");
                        }
                        catch (Exception)
                        { }
                        ViewBag.ValiLog = ErrVal.Trim();
                        ViewBag.AID = DT_App.Rows[0][0].ToString().Trim();
                        ViewBag.ST1 = ""; ViewBag.ST2 = ""; ViewBag.ST3 = ""; ViewBag.ST4 = "";
                        switch(DT_App.Rows[0][9].ToString().Trim())
                        {
                            case "1": { ViewBag.ST1 = "selected"; break; }
                            case "2": { ViewBag.ST2 = "selected"; break; }
                            case "3": { ViewBag.ST3 = "selected"; break; }
                            case "4": { ViewBag.ST4 = "selected"; break; }
                        }
                        return View();
                    }
                    else
                    {
                        return RedirectToAction("HSU", "Applications", new { id = "", area = "HSU_Portal" });
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" });
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" });
            }
        }
        //====================================================================================================================
        private bool IsGuesty(string App_ID)
        {
            try
            {
                DataTable DT_Application = new DataTable();
                DT_Application = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Form_ID From Users_08_Hospitality_SingleUser_Application Where (ID = '" + App_ID + "')");
                DataTable DT_Element = new DataTable();
                DT_Element = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_ID From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + DT_Application.Rows[0][0].ToString().Trim() + "') And (Element_Type_Code = '1') And (Status_Code = '1') And (Removed = '0') And (ATT19 = '10')");
                if (DT_Element.Rows.Count == 0) { return false; } else { if (DT_Element.Rows.Count != 1) { return false; } }
                return true;
            }
            catch (Exception) { return false; }
        }
        //====================================================================================================================
        [HttpPost]
        public JsonResult Application_ChangeStatus(string AID, string SID)
        {
            try
            {
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    string SCD = "1"; string STT = "Pending";
                    switch (SID)
                    {
                        case "1": { SCD = "1"; STT = "Pending"; break; }
                        case "2": { SCD = "2"; STT = "Review"; break; }
                        case "3": { SCD = "3"; STT = "Failed"; break; }
                        case "4": { SCD = "4"; STT = "Passed"; break; }
                    }
                    string InsDate = Sq.Sql_Date();
                    string InsTime = Sq.Sql_Time();
                    Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_08_Hospitality_SingleUser_Application Set [Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_User_ID] = '" + UID + "',[Status_Code] = '" + SCD + "',[Status_Text] = '" + STT + "' Where (ID = '" + AID + "')");
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
        public JsonResult Application_ChangeGuesty(string AID, string SID)
        {
            try
            {
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                string GError = "";
                string BSres = "Unknown";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT_Application = new DataTable();
                    DT_Application = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Form_ID,User_ID,Status_Code,Status_Text,App_Message From Users_08_Hospitality_SingleUser_Application Where (ID = '" + AID + "')");
                    DataTable DT_Element = new DataTable();
                    DT_Element = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_ID From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + DT_Application.Rows[0][0].ToString().Trim() + "') And (Element_Type_Code = '1') And (Status_Code = '1') And (Removed = '0') And (ATT19 = '10')");
                    if (DT_Element.Rows.Count == 1)
                    {
                        DataTable DT_Value = new DataTable();
                        DT_Value = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Element_Text From Users_09_Hospitality_SingleUser_Application_Elements Where (App_ID = '" + AID + "') And (Element_ID = '" + DT_Element.Rows[0][0].ToString().Trim() + "')");
                        if (DT_Value.Rows.Count == 1)
                        {
                            if (DT_Value.Rows[0][0].ToString().Trim() != "")
                            {
                                iCore_Administrator.Modules.HSU_Application.Application_Guesty.Guesty_BookingStatus GBS = new iCore_Administrator.Modules.HSU_Application.Application_Guesty.Guesty_BookingStatus();
                                GBS.Error_Code = 0; GBS.Error_Text = ""; GBS._id = ""; GBS.status = "";
                                if (SID == "1")
                                {
                                    GBS = Gusty_Func.Set_Booking_Status(DT_Value.Rows[0][0].ToString().Trim(), iCore_Administrator.Modules.HSU_Application.Application_Guesty.BookingStatus.Approve);
                                    BSres = "approve";
                                }
                                else
                                {
                                    GBS = Gusty_Func.Set_Booking_Status(DT_Value.Rows[0][0].ToString().Trim(), iCore_Administrator.Modules.HSU_Application.Application_Guesty.BookingStatus.Decline);
                                    BSres = "decline";
                                }
                                if (GBS.Error_Code == 0)
                                {
                                    GError = "Guesty booking status successfully set to " + BSres;
                                    ResVal = "0";
                                }
                                else
                                {
                                    GError = GBS.Error_Text;
                                    ResVal = "1";
                                }
                            }
                            else
                            {
                                GError = "The customer did not enter the confirmation code";
                            }
                        }
                        else
                        {
                            GError = "Guesty confirmation code not existed";
                        }
                    }
                    else
                    {
                        if (DT_Element.Rows.Count > 1)
                        {
                            GError = "Guesty confirmation code element not properly defined";
                        }
                    }
                    ResSTR = GError;
                    string DTM = "[" + Pb.Get_Date() + " " + Pb.Get_Time() + "] ";
                    GError = "[Guesty] " + DTM + GError;
                    string BeforeError = DT_Application.Rows[0][4].ToString().Trim();
                    if (BeforeError.Trim() != "")
                    {
                        BeforeError = BeforeError + "$" + GError.Trim();
                    }
                    else
                    {
                        BeforeError = GError.Trim();
                    }
                    Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_08_Hospitality_SingleUser_Application Set [App_Message] = '" + BeforeError + "' Where (ID = '" + AID + "')");
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