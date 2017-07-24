using System.Data.Entity;
using jQuery_File_Upload.MVC5.Models;

namespace jQuery_File_Upload.MVC5.Data
{
    public class FileUploadContext : DbContext
    {
        public DbSet<UploadedFile> UploadedFiles { get; set; }
    }
}