using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using jQuery_File_Upload.MVC5.Helpers;
using jQuery_File_Upload.MVC5.Models;
using jQuery_File_Upload.MVC5.Services;

namespace jQuery_File_Upload.MVC5.Controllers
{
    public class FileUploadController : Controller
    {
        private readonly IFileStorageService _fileStorageService;

        public FileUploadController()
        {
            _fileStorageService =  new DatabaseStorageService(); 
            //_fileStorageService =  new FileSystemStorageService();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Show()
        {
            var listOfFiles = _fileStorageService.GetFileList();

            var model = new ShowFilesViewModel
            {
                FileModels = listOfFiles.files
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
            var resultList = new List<FileViewModel>();

            _fileStorageService.UploadAndAddToResults(Request, resultList);

            var files = new JsonFiles(resultList);

            return resultList.Any() ? Json(files) : Json("Error");
        }

        public JsonResult GetFileList()
        {
            var list = _fileStorageService.GetFileList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFile(Guid id)
        {
            UploadedFile file = _fileStorageService.GetFile(id);

            return File(file.Data, file.MimeType, file.Name);
        }

        [HttpGet]
        public JsonResult DeleteFile(object file)
        {
            var suceeded = _fileStorageService.DeleteFile(file);

            return Json(suceeded, JsonRequestBehavior.AllowGet);
        }
    }
}