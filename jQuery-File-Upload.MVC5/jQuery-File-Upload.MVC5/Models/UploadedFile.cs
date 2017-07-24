using System;

namespace jQuery_File_Upload.MVC5.Models
{
    public class UploadedFile
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        
        public string MimeType { get; set; }
        public byte[] Data { get; set; }
        public byte[] ThumbnailData { get; set; }
    }
}