using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
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
            var file = new UploadedFile();

            using (_dbContext)
            {
                //foreach (HttpPostedFileWrapper fileData in request.Files)
                for (var i = 0; i < request.Files.Count; i++)
                {
                    HttpPostedFileWrapper fileData = (HttpPostedFileWrapper)request.Files[i];

                    if (fileData != null)
                    {
                        file.Name = Path.GetFileName(fileData.FileName);
                        file.MimeType = fileData.ContentType;

                        Image image;

                        using (var mainStream = new MemoryStream())
                        {
                            fileData.InputStream.CopyTo(mainStream);
                            file.Data = mainStream.ToArray();

                            image = Image.FromStream(mainStream);                            
                        }

                        using (var thumbStream = new MemoryStream())
                        {
                            Image thumbImage = image.GetThumbnailImage(80, 80, () => false, IntPtr.Zero);

                            thumbImage.Save(thumbStream, ImageFormat.Jpeg);
                            file.ThumbnailData = thumbStream.ToArray();
                        }

                        _dbContext.UploadedFiles.Add(file);
                    } 
                }

                _dbContext.SaveChanges();
            }

            uploadResults.Add(GetFileViewModelFromFile(file));
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
                //Data = new byte[fileData.ContentLength]
            };

            Stream inputStream = fileData.InputStream;

            using (var memStream = new MemoryStream())
            {
                var buffer = new byte[1024];
                int read = inputStream.Read(buffer, 0, buffer.Length); ;

                while (read > 0)
                {
                    memStream.Write(buffer, 0, read);
                    read = inputStream.Read(buffer, 0, buffer.Length);
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
            var idString = ((string[])identifier)[0];

            if (idString != null)
            {
                Guid id = Guid.Parse(idString);

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
                url = "/FileUpload/GetFile/" + file.Id,
                deleteUrl = "/FileUpload/DeleteFile/?file=" + file.Id,
                thumbnailUrl = GetThumbnailUrl(file),
                deleteType = "GET"
            };

            return result;
        }

        private static string GetThumbnailUrl(UploadedFile file)
        {
            return "/FileUpload/GetFileThumbnail/" + file.Id;
        }
    }
}