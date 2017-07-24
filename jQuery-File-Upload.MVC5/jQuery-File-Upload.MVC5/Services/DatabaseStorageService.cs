using System.Collections.Generic;
using System.Data.Entity;
using System.Web;
using jQuery_File_Upload.MVC5.Data;
using jQuery_File_Upload.MVC5.Helpers;
using jQuery_File_Upload.MVC5.Models;

namespace jQuery_File_Upload.MVC5.Services
{
    public class DatabaseStorageService : IFileStorageService
    {
        private readonly FileUploadContext _dbContext;

        public DatabaseStorageService()
        {
            _dbContext = new FileUploadContext();
        }

        public void UploadAndAddToResults(HttpRequestBase request, List<FileViewModel> uploadResults)
        {
            throw new System.NotImplementedException();
        }

        public JsonFiles GetFileList()
        {
            var fileResults = new List<FileViewModel>();

            using (_dbContext)
            {
                foreach (UploadedFile file in _dbContext.UploadedFiles)
                {
                    fileResults.Add(GetFileViewModelFromFile(file));
                }
            }

            return new JsonFiles(fileResults);
        }

        public bool DeleteFile(object identifier)
        {
            throw new System.NotImplementedException();
        }

        private FileViewModel GetFileViewModelFromFile(UploadedFile file)
        {
            var result = new FileViewModel
            {
                name = file.FileName,
                size = file.Data.Length,
                type = file.MimeType,
                url = "/FileUpload/GetFile/?file=",
                deleteUrl = "/FileUpload/DeleteFile/?file=",
                thumbnailUrl = "/FileUpload/GetFileThumbnail/?file=",
                deleteType = "GET"
            };

            return result;
        }
    }
}