using System.Collections.Generic;
using System.Web;
using jQuery_File_Upload.MVC5.Helpers;

namespace jQuery_File_Upload.MVC5.Services
{
    public interface IFileStorageService
    {
        void UploadAndAddToResults(HttpRequestBase request, List<ViewDataUploadFilesResult> uploadResults);
        JsonFiles GetFileList();
        bool DeleteFile(object identifier);
    }
}