using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using iCore_User.Modules;
using iCore_User.Modules.SecurityAuthentication;

namespace iCore_User.Areas.HSU_Portal.Controllers
{
    public class DashboardController : Controller
    {
        //====================================================================================================================
        iCore_Administrator.Modules.AuthenticationTester AAuth = new iCore_Administrator.Modules.AuthenticationTester();
        iCore_Administrator.Modules.SQL_Tranceiver Sq = new iCore_Administrator.Modules.SQL_Tranceiver();
        iCore_Administrator.Modules.PublicFunctions Pb = new iCore_Administrator.Modules.PublicFunctions();
        //====================================================================================================================
        public ActionResult Index()
        {
            ViewBag.MenuCode = 0;
            return View();
        }
        //====================================================================================================================



        //====================================================================================================================
        public ActionResult IconPack()
        {
            return View();
        }
        //====================================================================================================================
        public ActionResult Logout()
        {
            Session["User_UID"] = "0";
            Session["User_UNM"] = "";
            Session["User_Type"] = "0";
            Session["User_TypeText"] = "";
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login", new { id = "", area = "" });
        }
        //====================================================================================================================
    }
}