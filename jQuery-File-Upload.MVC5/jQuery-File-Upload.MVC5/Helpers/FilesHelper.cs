using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Helpers;
using System.Web.Hosting;

namespace jQuery_File_Upload.MVC5.Helpers
{
    public class FilesHelper
    {
        private readonly string _deleteType;

        private readonly string _deleteUrl;

        //ex:"~/Files/something/";
        private readonly string _serverMapPath;

        private readonly string _storageRoot;

        private readonly string _tempPath;
        private readonly string _urlBase;

        public FilesHelper(string deleteUrl, string deleteType, string storageRoot, string urlBase, string tempPath,
            string serverMapPath)
        {
            _deleteUrl = deleteUrl;
            _deleteType = deleteType;
            _storageRoot = storageRoot;
            _urlBase = urlBase;
            _tempPath = tempPath;
            _serverMapPath = serverMapPath;
        }

        public void DeleteFiles(string pathToDelete)
        {
            var path = HostingEnvironment.MapPath(pathToDelete);

            if (path != null)
            {
                //System.Diagnostics.Debug.WriteLine(path);
                if (Directory.Exists(path))
                {
                    var di = new DirectoryInfo(path);
                    foreach (var fi in di.GetFiles())
                    {
                        File.Delete(fi.FullName);
                        //System.Diagnostics.Debug.WriteLine(fi.Name);
                    }

                    di.Delete(true);
                }
            }
        }

        public string DeleteFile(string file)
        {
            //System.Diagnostics.Debug.WriteLine("DeleteFile");
            //    var req = HttpContext.Current;
            //System.Diagnostics.Debug.WriteLine(file);

            var fullPath = Path.Combine(_storageRoot, file);
            //System.Diagnostics.Debug.WriteLine(fullPath);
            //System.Diagnostics.Debug.WriteLine(File.Exists(fullPath));

            //string thumbPath = "/" + file + "80x80.jpg";

            var partThumb1 = Path.Combine(_storageRoot, "thumbs");
            var partThumb2 = Path.Combine(partThumb1, file + "80x80.jpg");

            //System.Diagnostics.Debug.WriteLine(partThumb2);
            //System.Diagnostics.Debug.WriteLine(File.Exists(partThumb2));
            if (File.Exists(fullPath))
            {
                //delete thumb 
                if (File.Exists(partThumb2))
                {
                    File.Delete(partThumb2);
                }

                File.Delete(fullPath);

                return "Ok";
            }

            return "Error Delete";
        }

        public JsonFiles GetFileList()
        {
            var fileResults = new List<ViewDataUploadFilesResult>();

            var fullPath = Path.Combine(_storageRoot);

            if (Directory.Exists(fullPath))
            {
                var dir = new DirectoryInfo(fullPath);

                foreach (var file in dir.GetFiles())
                {
                    var size = unchecked((int) file.Length);

                    fileResults.Add(GetUploadResult(file.Name, size, file.FullName));
                }
            }

            return new JsonFiles(fileResults);
        }

        public List<string> GetFileNamesList()
        {
            var files = new List<string>();

            var path = HostingEnvironment.MapPath(_serverMapPath);

            //System.Diagnostics.Debug.WriteLine(path);

            if (Directory.Exists(path))
            {
                var di = new DirectoryInfo(path);
                foreach (var fi in di.GetFiles())
                {
                    files.Add(fi.Name);
                    //System.Diagnostics.Debug.WriteLine(fi.Name);
                }
            }

            return files;
        }

        public void UploadAndAddToResults(HttpRequestBase request, List<ViewDataUploadFilesResult> uploadResults)
        {
            //System.Diagnostics.Debug.WriteLine(Directory.Exists(_tempPath));

            var fullPath = Path.Combine(_storageRoot);
            Directory.CreateDirectory(fullPath);
            // Create new folder for thumbs
            Directory.CreateDirectory(fullPath + "/thumbs/");

            foreach (string inputTagName in request.Files)
            {
                var headers = request.Headers;

                var file = request.Files[inputTagName];
                Debug.WriteLine(file.FileName);

                if (string.IsNullOrEmpty(headers["X-File-Name"]))
                {
                    UploadWholeFile(request, uploadResults);
                }
                else
                {
                    UploadPartialFile(headers["X-File-Name"], request, uploadResults);
                }
            }
        }

        private void UploadWholeFile(HttpRequestBase request, List<ViewDataUploadFilesResult> uploadResults)
        {
            for (var i = 0; i < request.Files.Count; i++)
            {
                var file = request.Files[i];
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
                        var thumbfullPath = Path.Combine(pathOnServer, "thumbs");
                        //String fileThumb = file.FileName + ".80x80.jpg";
                        var fileThumb = Path.GetFileNameWithoutExtension(file.FileName) + "80x80.jpg";
                        var thumbfullPath2 = Path.Combine(thumbfullPath, fileThumb);
                        using (var stream = new MemoryStream(File.ReadAllBytes(fullPath)))
                        {
                            var thumbnail = new WebImage(stream).Resize(80, 80);
                            thumbnail.Save(thumbfullPath2, "jpg");
                        }
                    }
                }

                uploadResults.Add(GetUploadResult(file.FileName, file.ContentLength, file.FileName));
            }
        }

        private void UploadPartialFile(string fileName, HttpRequestBase request,
            List<ViewDataUploadFilesResult> uploadResults)
        {
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
            var fullName = Path.Combine(pathOnServer, Path.GetFileName(file.FileName));
            var thumbfullPath = Path.Combine(fullName, Path.GetFileName(file.FileName + "80x80.jpg"));

            var handler = new ImageHandler();

            var imageBit = ImageHandler.LoadImage(fullName);
            handler.Save(imageBit, 80, 80, 10, thumbfullPath);
            using (var fs = new FileStream(fullName, FileMode.Append, FileAccess.Write))
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

        private ViewDataUploadFilesResult GetUploadResult(string fileName, int fileSize, string fileFullPath)
        {
            var getType = MimeMapping.GetMimeMapping(fileFullPath);

            var result = new ViewDataUploadFilesResult
            {
                Name = fileName,
                Size = fileSize,
                Type = getType,
                Url = _urlBase + fileName,
                DeleteUrl = _deleteUrl + fileName,
                ThumbnailUrl = GetThumbnailUrl(getType, fileName),
                DeleteType = _deleteType
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
                    return _urlBase + "thumbs/" + Path.GetFileNameWithoutExtension(fileName) + "80x80.jpg";
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

            return _urlBase + "/thumbs/" + Path.GetFileNameWithoutExtension(fileName) + "80x80.jpg";
        }
    }
}