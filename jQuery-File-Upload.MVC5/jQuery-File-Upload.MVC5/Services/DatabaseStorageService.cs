using System.Collections.Generic;
using System.Web;
using jQuery_File_Upload.MVC5.Helpers;
using jQuery_File_Upload.MVC5.Models;

namespace jQuery_File_Upload.MVC5.Services
{
    public class DatabaseStorageService : IFileStorageService
    {
        public void UploadAndAddToResults(HttpRequestBase request, List<FileViewModel> uploadResults)
        {
            throw new System.NotImplementedException();
        }

        public JsonFiles GetFileList()
        {
            throw new System.NotImplementedException();
        }

        public bool DeleteFile(object identifier)
        {
            throw new System.NotImplementedException();
        }
    }
}