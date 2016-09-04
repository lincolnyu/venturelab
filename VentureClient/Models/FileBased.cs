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

        public async Task PickFile()
        {
            var fop = new FileOpenPicker();
            fop.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            fop.FileTypeFilter.Add(".txt");
            File = await fop.PickSingleFileAsync();
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
