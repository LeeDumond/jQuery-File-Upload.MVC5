using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
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
            var headers = request.Headers;

            if (string.IsNullOrEmpty(headers["X-File-Name"]))
            {
                UploadWholeFile(request, uploadResults);
            }
            else
            {
                UploadPartialFile(headers["X-File-Name"], request, uploadResults);
            }
        }

        private void UploadWholeFile(HttpRequestBase request, List<FileViewModel> uploadResults)
        {       
            using (_dbContext)
            {
                foreach (HttpPostedFileBase fileData in request.Files)
                {
                    var file = new UploadedFile
                    {
                        Name = VirtualPathUtility.GetFileName(fileData.FileName),
                        MimeType = fileData.ContentType,
                        Data = new byte[fileData.ContentLength]
                    };

                    using (var stream = new MemoryStream())
                    {
                        fileData.InputStream.CopyTo(stream);
                        file.Data = stream.ToArray();
                    }

                    //fileData.InputStream.Read(file.Data, 0, fileData.ContentLength);

                    _dbContext.UploadedFiles.Add(file);

                    uploadResults.Add(GetFileViewModelFromFile(file));
                }

                _dbContext.SaveChanges();
            }
        }

        private void UploadPartialFile(string fileName, HttpRequestBase request, List<FileViewModel> uploadResults)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (request.Files.Count != 1)
            {
                throw new HttpRequestValidationException(
                    "Attempt to upload chunked file containing more than one file per request");
            }

            HttpPostedFileBase fileData = request.Files[0];

            if (fileData == null)
            {
                throw new InvalidOperationException("No file to upload.");
            }

            var file = new UploadedFile
            {
                Name = VirtualPathUtility.GetFileName(fileData.FileName),
                MimeType = fileData.ContentType,
                Data = new byte[fileData.ContentLength]
            };

            Stream inputStream = fileData.InputStream;

            using (var memStream = new MemoryStream())
            {
                var buffer = new byte[1024];
                int read;

                while ((read = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memStream.Write(buffer, 0, read);
                }

                file.Data = memStream.ToArray();
            }

            uploadResults.Add(GetFileViewModelFromFile(file));
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
            var id = identifier as Guid?;

            if (id != null)
            {
                using (_dbContext)
                {
                    var file = _dbContext.UploadedFiles.Find(id);

                    if (file == null)
                    {
                        return false;
                    }

                    _dbContext.UploadedFiles.Remove(file);
                    _dbContext.SaveChanges();

                    return true;
                }
            }

            return false;
        }

        public UploadedFile GetFile(Guid id)
        {
            using (_dbContext)
            {
                return _dbContext.UploadedFiles.Find(id);
            }
        }

        private FileViewModel GetFileViewModelFromFile(UploadedFile file)
        {
            var result = new FileViewModel
            {
                name = file.Name,
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