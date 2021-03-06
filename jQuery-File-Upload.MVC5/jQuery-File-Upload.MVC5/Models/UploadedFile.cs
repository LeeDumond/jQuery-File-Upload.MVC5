﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace jQuery_File_Upload.MVC5.Models
{
    public class UploadedFile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        
        public string MimeType { get; set; }
        public byte[] Data { get; set; }
        public byte[] ThumbnailData { get; set; }
    }
}