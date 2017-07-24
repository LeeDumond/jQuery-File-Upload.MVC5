using System;
using System.Collections.Generic;
using System.Web;
using jQuery_File_Upload.MVC5.Helpers;
using jQuery_File_Upload.MVC5.Models;

namespace jQuery_File_Upload.MVC5.Services
{
    public interface IFileStorageService
    {
        void UploadAndAddToResults(HttpRequestBase request, List<FileViewModel> uploadResults);
        JsonFiles GetFileList();
        bool DeleteFile(object identifier);
        UploadedFile GetFile(Guid id);
    }
}