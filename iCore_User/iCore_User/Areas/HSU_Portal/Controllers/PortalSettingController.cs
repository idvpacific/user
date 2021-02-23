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
    public class PortalSettingController : Controller
    {
        //====================================================================================================================
        iCore_Administrator.Modules.AuthenticationTester AAuth = new iCore_Administrator.Modules.AuthenticationTester();
        iCore_Administrator.Modules.SQL_Tranceiver Sq = new iCore_Administrator.Modules.SQL_Tranceiver();
        iCore_Administrator.Modules.PublicFunctions Pb = new iCore_Administrator.Modules.PublicFunctions();
        //====================================================================================================================
        public ActionResult Index() { return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" }); }
        //====================================================================================================================
        [HttpGet]
        public ActionResult Basic()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 14;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                ViewBag.DT_R1 = "";
                ViewBag.DT_R2 = "";
                ViewBag.DT_R3 = "";
                ViewBag.DT_R4 = "";
                ViewBag.DT_R5 = "";
                ViewBag.DT_R6 = "";
                DataTable DT = new DataTable();
                DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select * From Users_03_Hospitality_SingleUser_BasicSetting Where (User_ID = '" + Session["User_UID"].ToString().Trim() + "')");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count == 1)
                    {
                        ViewBag.DT_R1 = DT.Rows[0][1].ToString().Trim();
                        ViewBag.DT_R2 = DT.Rows[0][2].ToString().Trim();
                        ViewBag.DT_R3 = DT.Rows[0][3].ToString().Trim();
                        ViewBag.DT_R4 = DT.Rows[0][4].ToString().Trim();
                        ViewBag.DT_R5 = DT.Rows[0][5].ToString().Trim();
                        ViewBag.DT_R6 = DT.Rows[0][6].ToString().Trim();
                        ViewBag.DT_R7 = DT.Rows[0][7].ToString().Trim();
                        ViewBag.DT_R8 = DT.Rows[0][8].ToString().Trim();
                    }
                }
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" });
            }
        }

        [HttpPost]
        public JsonResult BasicSetting_ImageClear()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 16;
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
                    string path = Server.MapPath("~/Drive/Hospitality/Logo/" + UID + ".Png");
                    if (System.IO.File.Exists(path) == true)
                    {
                        System.IO.File.Delete(path);
                    }
                    ResSTR = "Logo file successfully removed";
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
        public JsonResult BasicSetting_Save(string A1, string A2, string A3, HttpPostedFileBase A4)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 17;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["User_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                A1 = A1.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A2 = A2.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A3 = A3.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                int NeedRecreate = 0;
                try
                {
                    DataTable DT_Test = new DataTable();
                    DT_Test = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select User_ID From Users_03_Hospitality_SingleUser_BasicSetting Where (User_ID = '" + UID + "')");
                    if (DT_Test.Rows.Count != 1) { NeedRecreate = 1; }
                }
                catch (Exception)
                { NeedRecreate = 1; }
                if(NeedRecreate==1)
                {
                    Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Delete From Users_03_Hospitality_SingleUser_BasicSetting Where (User_ID = '" + UID + "')");
                    Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_03_Hospitality_SingleUser_BasicSetting Values ('" + UID + "','','','','','','','','')");
                }
                if (ResVal == "0")
                {
                    switch (A1)
                    {
                        case "1":
                            {
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_03_Hospitality_SingleUser_BasicSetting Set [Header1] = '" + A2 + "',[Header2] = '" + A3 + "' where (User_ID = '" + UID + "')");
                                ResSTR = "Header fields successfully edited";
                                break;
                            }
                        case "2":
                            {
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_03_Hospitality_SingleUser_BasicSetting Set [Footer1] = '" + A2 + "',[Footer2] = '" + A3 + "' Where (User_ID = '" + UID + "')");
                                ResSTR = "Footer fields successfully edited";
                                break;
                            }
                        case "3":
                            {
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_03_Hospitality_SingleUser_BasicSetting Set [Domain_Name] = '" + A2 + "',[Domain_Address] = '" + A3 + "' Where (User_ID = '" + UID + "')");
                                ResSTR = "Basic fields successfully edited";
                                break;
                            }
                        case "4":
                            {
                                string path = Server.MapPath("~/Drive/Hospitality/Logo/" + UID + ".png");
                                if (A4 != null)
                                {
                                    if (System.IO.File.Exists(path) == true)
                                    {
                                        System.IO.File.Delete(path);
                                    }
                                    A4.SaveAs(path);
                                    ResSTR = "Logo file successfully uploaded";
                                }
                                else
                                {
                                    ResVal = "1"; ResSTR = "Logo file not found to upload";
                                }
                                break;
                            }
                        case "5":
                            {
                                Sq.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_03_Hospitality_SingleUser_BasicSetting Set [GuestyAPI_Key] = '" + A2 + "',[GuestyAPI_Secret] = '" + A3 + "' Where (User_ID = '" + UID + "')");
                                ResSTR = "Basic fields successfully edited";
                                break;
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

        //====================================================================================================================



        //====================================================================================================================



        //====================================================================================================================
    }
}