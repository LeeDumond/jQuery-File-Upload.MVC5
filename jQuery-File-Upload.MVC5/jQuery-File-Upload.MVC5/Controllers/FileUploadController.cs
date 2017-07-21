﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using jQuery_File_Upload.MVC5.Helpers;
using jQuery_File_Upload.MVC5.Models;

namespace jQuery_File_Upload.MVC5.Controllers
{
    public class FileUploadController : Controller
    {
        private const string TempPath = "~/somefiles/";
        private const string ServerMapPath = "~/Files/somefiles/";
        private const string UrlBase = "/Files/somefiles/";
        private const string DeleteUrl = "/FileUpload/DeleteFile/?file=";
        private const string DeleteType = "GET";
        private readonly FilesHelper _filesHelper;

        public FileUploadController()
        {
            _filesHelper = new FilesHelper(DeleteUrl, DeleteType, StorageRoot, UrlBase, TempPath, ServerMapPath);
        }

        private static string StorageRoot => Path.Combine(HostingEnvironment.MapPath(ServerMapPath));

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Show()
        {
            var listOfFiles = _filesHelper.GetFileList();

            var model = new FilesViewModel
            {
                Files = listOfFiles.files
            };

            return View(model);
        }

        public ActionResult Edit()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Upload()
        {
            var resultList = new List<ViewDataUploadFilesResult>();

            //_filesHelper.UploadAndAddToResults(HttpContext.Request, resultList);
            _filesHelper.UploadAndAddToResults(Request, resultList);

            var files = new JsonFiles(resultList);

            return resultList.Any() ? Json(files) : Json("Error");
        }

        public JsonResult GetFileList()
        {
            var list = _filesHelper.GetFileList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DeleteFile(string file)
        {
            _filesHelper.DeleteFile(file);

            return Json("OK", JsonRequestBehavior.AllowGet);
        }
    }
}