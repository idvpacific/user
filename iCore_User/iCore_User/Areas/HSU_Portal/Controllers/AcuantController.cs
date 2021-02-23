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
    public class AcuantController : Controller
    {
        //====================================================================================================================
        iCore_Administrator.Modules.AuthenticationTester AAuth = new iCore_Administrator.Modules.AuthenticationTester();
        iCore_Administrator.Modules.SQL_Tranceiver Sq = new iCore_Administrator.Modules.SQL_Tranceiver();
        iCore_Administrator.Modules.PublicFunctions Pb = new iCore_Administrator.Modules.PublicFunctions();
        //====================================================================================================================
        public ActionResult Index() { return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" }); }
        //====================================================================================================================
        [HttpGet]
        public ActionResult GIC()
        {
            try
            {
                DataTable DT = new DataTable();
                DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Top(1) * From Setting_Basic_01_Acuant");
                ViewBag.Configuration = DT.Rows[0];
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" });
            }
        }
        //====================================================================================================================
        [HttpGet]
        public ActionResult LNT()
        {
            try
            {
                DataTable DT = new DataTable();
                DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Top(1) * From Setting_Basic_01_Acuant");
                ViewBag.Configuration = DT.Rows[0];
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "HSU_Portal" });
            }
        }
        //====================================================================================================================
    }
}