using System.Collections.Generic;
using System.Linq;

namespace jQuery_File_Upload.MVC5.Helpers
{
    public class JsonFiles
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBePrivate.Global
        public readonly ViewDataUploadFilesResult[] files;

        //public string TempFolder { get; set; }

        public JsonFiles(List<ViewDataUploadFilesResult> filesList)
        {
            files = new ViewDataUploadFilesResult[filesList.Count];
            for (int i = 0; i < filesList.Count; i++)
            {
                files[i] = filesList.ElementAt(i);
            }

        }
    }
}