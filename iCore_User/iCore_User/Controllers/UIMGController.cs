using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using iCore_User.Modules;
using iCore_User.Modules.SecurityAuthentication;

namespace iCore_User.Controllers
{
    public class UIMGController : Controller
    {
        //====================================================================================================================
        iCore_Administrator.Modules.SQL_Tranceiver Sq = new iCore_Administrator.Modules.SQL_Tranceiver();
        iCore_Administrator.Modules.PublicFunctions Pb = new iCore_Administrator.Modules.PublicFunctions();
        iCore_Administrator.Modules.MimeTypeMap MTM = new iCore_Administrator.Modules.MimeTypeMap();
        iCore_Administrator.Modules.HSU_Application.Application_MasterFunction AppFn = new iCore_Administrator.Modules.HSU_Application.Application_MasterFunction();
        //====================================================================================================================
        public ActionResult Index() { return new HttpNotFoundResult(); }
        //====================================================================================================================
        public ActionResult ULF()
        {
            try
            {
                if (RouteData.Values["id"] == null) { return new HttpNotFoundResult(); }
                string DID = RouteData.Values["id"].ToString().Trim();
                string[] SepDT = DID.Split('$');
                var LogoPath = Server.MapPath("~/Drive/Hospitality/Logo/" + SepDT[0] + ".png");
                if (System.IO.File.Exists(LogoPath) == true)
                {
                    string SecretKey = System.Configuration.ConfigurationManager.AppSettings["iCore_API_SecretKey"];
                    if (SecretKey.Trim().ToUpper() == SepDT[1].ToString().Trim().ToUpper())
                    {
                        return File(LogoPath, "image/png");
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
        //====================================================================================================================
        [HttpGet]
        public ActionResult DownloadAttachFileRF()
        {
            try
            {
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
        //====================================================================================================================
        public ActionResult VideoPlayerRF(string FID)
        {
            try
            {
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
        //====================================================================================================================
        [HttpGet]
        public ActionResult DownloadDnldRF()
        {
            try
            {
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
        //====================================================================================================================
        public ActionResult FIMG(string EID, string IID)
        {
            try
            {
                var IMGPath = Server.MapPath("~/Drive/Hospitality/RegisterForms/" + EID + "/I9_" + IID + ".IDV");
                if (System.IO.File.Exists(IMGPath) == true)
                {
                    Response.StatusCode = 200;
                    Response.TrySkipIisCustomErrors = true;
                    return File(IMGPath, "image/png");
                }
                else
                {
                    Response.StatusCode = 404;
                    Response.TrySkipIisCustomErrors = true;
                    return null;
                }
            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                Response.TrySkipIisCustomErrors = true;
                return null;
            }
        }
        //====================================================================================================================
        [HttpPost]
        public void HSU_Form_Upload(string AID, string EID, HttpPostedFileBase UF)
        {
            try
            {
                AID = AID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                EID = EID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                string BasFolder = Server.MapPath("~/Drive/Hospitality/CustomerData/" + AID);
                string BasPath = Server.MapPath("~/Drive/Hospitality/CustomerData/" + AID + "/" + EID + ".IDV");
                if (System.IO.Directory.Exists(BasFolder) == false) { System.IO.Directory.CreateDirectory(BasFolder); }
                UF.SaveAs(BasPath);
            }
            catch (Exception) { }
        }
        //====================================================================================================================
        [HttpPost]
        public void HSU_Form_Start(string SK, string AID)
        {
            try
            {
                SK = SK.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                AID = AID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                string SecretKey = System.Configuration.ConfigurationManager.AppSettings["iCore_API_SecretKey"];
                if (SK.ToUpper() == SecretKey.ToUpper().Trim()) { Task.Run(() => { AppFn.Application_StartCheck(AID); }); }
            }
            catch (Exception) { }
        }
        //====================================================================================================================
        [HttpPost]
        public ActionResult RFIMG()
        {
            try
            {
                if (RouteData.Values["id"] == null) { return new HttpNotFoundResult(); }
                string DID = RouteData.Values["id"].ToString().Trim();
                string[] SepDT = DID.Split('$');
                string App_ID = SepDT[0];
                string Element_ID = SepDT[1];
                string Element_Type = SepDT[2];
                var LogoPath = Server.MapPath("~/Drive/Hospitality/CustomerData/" + App_ID + "/" + Element_ID + ".IDV");
                if (System.IO.File.Exists(LogoPath) == true)
                {
                    string SecretKey = System.Configuration.ConfigurationManager.AppSettings["iCore_API_SecretKey"];
                    if (SecretKey.Trim().ToUpper() == SepDT[3].ToString().Trim().ToUpper())
                    {
                        return File(LogoPath, "image/" + Element_Type);
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
        //====================================================================================================================
        [HttpGet]
        public ActionResult RFFD()
        {
            try
            {
                if (RouteData.Values["id"] == null) { return new HttpNotFoundResult(); }
                int FFnd = 0;
                string FileAdd = RouteData.Values["id"].ToString().Trim();
                string AID = "0";
                string EID = "0";
                string SCD = "0";
                string FileName = "";
                string FileType = "";
                string FileContent = "";
                string[] SepData = FileAdd.Trim().Split('$');
                AID = SepData[0].ToString().Trim();
                EID = SepData[1].ToString().Trim();
                SCD = SepData[2].ToString().Trim();
                string SecretKey = System.Configuration.ConfigurationManager.AppSettings["iCore_API_SecretKey"];
                if (SCD == SecretKey)
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select File_Name,File_Type From Users_09_Hospitality_SingleUser_Application_Elements Where (App_ID = '" + AID + "') And (Element_ID = '" + EID + "')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            FileName = DT.Rows[0][0].ToString().Trim();
                            FileType = DT.Rows[0][1].ToString().Trim();
                            var Server_Path = Server.MapPath("~/Drive/Hospitality/CustomerData/" + AID);
                            if (System.IO.File.Exists(Server_Path + "/" + EID + ".IDV") == true)
                            {
                                FFnd = 1;
                                string FilePathLST = Server_Path + "/" + EID + ".IDV";
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
        //====================================================================================================================
        [HttpPost]
        public ActionResult RFIMGVD()
        {
            try
            {
                if (RouteData.Values["id"] == null) { return new HttpNotFoundResult(); }
                string DID = RouteData.Values["id"].ToString().Trim();
                string[] SepDT = DID.Split('$');
                string App_ID = SepDT[0];
                string Element_ID = SepDT[1];
                string Element_Type = SepDT[2];
                var LogoPath1 = Server.MapPath("~/Drive/Hospitality/CustomerData/" + App_ID + "/Result/" + Element_ID + ".jpg");
                var LogoPath2 = Server.MapPath("~/Drive/Hospitality/CustomerData/" + App_ID + "/" + Element_ID + ".IDV");
                string SecretKey = System.Configuration.ConfigurationManager.AppSettings["iCore_API_SecretKey"];
                if (SecretKey.Trim().ToUpper() == SepDT[3].ToString().Trim().ToUpper())
                {
                    if (System.IO.File.Exists(LogoPath1) == true)
                    {
                        return File(LogoPath1, "image/jpg");
                    }
                    else
                    {
                        if (System.IO.File.Exists(LogoPath2) == true)
                        {
                            return File(LogoPath2, "image/" + Element_Type);
                        }
                        else
                        {
                            return new HttpNotFoundResult();
                        }
                    }
                }
                else
                {
                    return new HttpNotFoundResult();
                }
            }
            catch (Exception) { return new HttpNotFoundResult(); }
        }
        //====================================================================================================================
    }
}