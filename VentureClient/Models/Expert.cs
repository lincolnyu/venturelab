using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace VentureClient.Models
{
    public class Expert
    {
        public StorageFile File { get; private set; }

        public async Task PickFile()
        {
            var fop = new FileOpenPicker();
            fop.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            fop.FileTypeFilter.Add(".txt");
            File = await fop.PickSingleFileAsync();
            if (File != null)
            {
                var content = await FileIO.ReadTextAsync(File);
                using (var sr = new StringReader(content))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        // TODO ...       
                    }
                    
                }
            }
        }
    }
}
