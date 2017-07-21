using System.Collections.Generic;
using System.Linq;

namespace jQuery_File_Upload.MVC5.Helpers
{
    public class JsonFiles
    {
        public ViewDataUploadFilesResult[] Files { get; }

        //public string TempFolder { get; set; }

        public JsonFiles(List<ViewDataUploadFilesResult> filesList)
        {
            Files = new ViewDataUploadFilesResult[filesList.Count];

            for (var i = 0; i < filesList.Count; i++)
            {
                Files[i] = filesList.ElementAt(i);
            }
        }
    }
}