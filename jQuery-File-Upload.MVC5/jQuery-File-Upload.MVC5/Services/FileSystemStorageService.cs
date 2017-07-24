using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Helpers;
using System.Web.Hosting;
using jQuery_File_Upload.MVC5.Helpers;
using jQuery_File_Upload.MVC5.Models;

namespace jQuery_File_Upload.MVC5.Services
{
    public class FileSystemStorageService : IFileStorageService
    {
        private const string DeleteType = "GET";
        private const string DeleteUrl = "/FileUpload/DeleteFile/?file=";
        private const string ServerMapPath = "~/Files/somefiles/";
        private const string UrlBase = "/Files/somefiles/";

        private readonly string _storageRoot;

        public FileSystemStorageService()
        {
            _storageRoot = Path.Combine(HostingEnvironment.MapPath(ServerMapPath));
        }

        public void UploadAndAddToResults(HttpRequestBase request, List<FileViewModel> uploadResults)
        {
            var fullPath = Path.Combine(_storageRoot);
            Directory.CreateDirectory(fullPath);
            // Create new folder for thumbs
            Directory.CreateDirectory(fullPath + "/thumbs/");

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

        public JsonFiles GetFileList()
        {
            var fileResults = new List<FileViewModel>();

            var fullPath = Path.Combine(_storageRoot);

            if (Directory.Exists(fullPath))
            {
                var dir = new DirectoryInfo(fullPath);

                foreach (var file in dir.GetFiles())
                {
                    var size = unchecked((int)file.Length);

                    fileResults.Add(GetUploadResult(file.Name, size, file.FullName));
                }
            }

            return new JsonFiles(fileResults);
        }

        public bool DeleteFile(object identifier)
        {
            // in this implementation, identifier is file name
            string fileName = ((string[])identifier)[0];

            if (fileName != null)
            {
                var fullPath = Path.Combine(_storageRoot, fileName);

                var thumbnailPath = Path.Combine(Path.Combine(_storageRoot, "thumbs"), Path.GetFileNameWithoutExtension(fileName) + "80x80.jpg");

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);

                    if (File.Exists(thumbnailPath))
                    {
                        File.Delete(thumbnailPath);
                    }

                    return true;
                }
            }

            return false;
        }

        public UploadedFile GetFile(Guid id)
        {
            throw new NotImplementedException();
        }

        private void UploadWholeFile(HttpRequestBase request, List<FileViewModel> uploadResults)
        {
            for (var i = 0; i < request.Files.Count; i++)
            {
                HttpPostedFileWrapper file = (HttpPostedFileWrapper) request.Files[i];
                var pathOnServer = Path.Combine(_storageRoot);
                var fullPath = Path.Combine(pathOnServer, Path.GetFileName(file.FileName));
                file.SaveAs(fullPath);

                //Create thumb
                var imageArray = file.FileName.Split('.');
                if (imageArray.Length != 0)
                {
                    var extension = imageArray[imageArray.Length - 1].ToLower();

                    //Do not create thumb if file is not an image
                    if (extension == "jpg" || extension == "png" || extension == "jpeg")
                    {
                        string thumbnailFileName = Path.GetFileNameWithoutExtension(file.FileName) + "80x80.jpg";
                        string thumbnailFilePath = Path.Combine(Path.Combine(pathOnServer, "thumbs"), thumbnailFileName);

                        using (var stream = new MemoryStream(File.ReadAllBytes(fullPath)))
                        {
                            var thumbnail = new WebImage(stream).Resize(80, 80);
                            thumbnail.Save(thumbnailFilePath, "jpg");
                        }
                    }
                }

                uploadResults.Add(GetUploadResult(file.FileName, file.ContentLength, file.FileName));
            }
        }

        private void UploadPartialFile(string fileName, HttpRequestBase request,
            List<FileViewModel> uploadResults)
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

            var file = request.Files[0];

            if (file == null)
            {
                throw new InvalidOperationException("No file to upload.");
            }

            var inputStream = file.InputStream;
            var pathOnServer = Path.Combine(_storageRoot);
            string thumbnailFileName = Path.GetFileNameWithoutExtension(fileName) + "80x80.jpg";
            string thumbnailFilePath = Path.Combine(Path.Combine(pathOnServer, "thumbs"), thumbnailFileName);

            var handler = new ImageHandler();
            var bitmap = ImageHandler.LoadImage(Path.Combine(pathOnServer, Path.GetFileName(fileName)));
            handler.Save(bitmap, 80, 80, 10, thumbnailFilePath);

            using (var fs = new FileStream(Path.Combine(pathOnServer, Path.GetFileName(fileName)), FileMode.Append, FileAccess.Write))
            {
                var buffer = new byte[1024];

                var l = inputStream.Read(buffer, 0, 1024);
                while (l > 0)
                {
                    fs.Write(buffer, 0, l);
                    l = inputStream.Read(buffer, 0, 1024);
                }
                fs.Flush();
                fs.Close();
            }

            uploadResults.Add(GetUploadResult(file.FileName, file.ContentLength, file.FileName));
        }

        private FileViewModel GetUploadResult(string fileName, int fileSize, string fileFullPath)
        {
            var getType = MimeMapping.GetMimeMapping(fileFullPath);

            var result = new FileViewModel
            {
                name = fileName,
                size = fileSize,
                type = getType,
                url = UrlBase + fileName,
                deleteUrl = DeleteUrl + fileName,
                thumbnailUrl = GetThumbnailUrl(getType, fileName),
                deleteType = DeleteType
            };

            return result;
        }

        private string GetThumbnailUrl(string type, string fileName)
        {
            var split = type.Split('/');

            if (split.Length == 2)
            {
                var extension = split[1].ToLower();

                if (extension.Equals("jpeg") || extension.Equals("jpg") || extension.Equals("png") ||
                    extension.Equals("gif"))
                {
                    return UrlBase + "thumbs/" + Path.GetFileNameWithoutExtension(fileName) + "80x80.jpg";
                }

                if (extension.Equals("octet-stream")) //Fix for exe files
                {
                    return "/Content/Free-file-icons/48px/exe.png";
                }

                if (extension.Contains("zip")) //Fix for exe files
                {
                    return "/Content/Free-file-icons/48px/zip.png";
                }

                return "/Content/Free-file-icons/48px/" + extension + ".png";
            }

            return UrlBase + "/thumbs/" + Path.GetFileNameWithoutExtension(fileName) + "80x80.jpg";
        }
    }
}