using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace VentureClient.Models
{
    public abstract class FileBased
    {
        public StorageFile File { get; private set; }

        /// <summary>
        ///  Returns true if a different file (excluding user cancel) has been picked
        /// </summary>
        /// <returns>True if a different file has been picked</returns>
        public async Task<bool> PickFile()
        {
            var fop = new FileOpenPicker();
            fop.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            fop.FileTypeFilter.Add(".txt");
            var file = await fop.PickSingleFileAsync();
            if (file != null && File != file)
            {
                File = file;
                return true;
            }
            return false;
        }

        public IEnumerable<string> ParseFile()
        {
            if (File != null)
            {
                var task = FileIO.ReadTextAsync(File);
                Task.WaitAll(task.AsTask());
                var content = task.GetResults();
                using (var sr = new StringReader(content))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        yield return line;
                    }
                }
            }
        }
    }
}
