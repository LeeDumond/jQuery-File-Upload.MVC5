using System.Collections.Generic;
using System.Linq;
using jQuery_File_Upload.MVC5.Models;

namespace jQuery_File_Upload.MVC5.Helpers
{
    public class JsonFiles
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBePrivate.Global
        public readonly FileViewModel[] files;

        //public string TempFolder { get; set; }

        public JsonFiles(List<FileViewModel> filesList)
        {
            //files = new ViewDataUploadFilesResult[filesList.Count];

            //for (int i = 0; i < filesList.Count; i++)
            //{
            //    files[i] = filesList.ElementAt(i);
            //}

            files = filesList.ToArray();

        }
    }
}