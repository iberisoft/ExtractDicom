using SharpCompress.Archives.SevenZip;
using SharpCompress.Readers;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            var sourceDialog = new FolderBrowserDialog
            {
                Description = "Source Folder"
            };
            if (sourceDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            var targetDialog = new FolderBrowserDialog
            {
                Description = "Target Folder"
            };
            if (targetDialog.ShowDialog() == DialogResult.OK)
            {
                ProcessArchive(sourceDialog.SelectedPath, targetDialog.SelectedPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void ProcessArchive(string sourceFolder, string targetFolder)
    {
        var sevenZipFiles = Directory.GetFiles(sourceFolder, "*.7z");
        if (sevenZipFiles.Length == 0)
        {
            Console.WriteLine("No 7z file found in source folder");
            return;
        }

        var archivePath = sevenZipFiles[0];
        Console.WriteLine($"Found archive: {archivePath}");

        Console.Write("Enter archive password: ");
        var password = Console.ReadLine();
        using (var archive = SevenZipArchive.Open(archivePath, new ReaderOptions { Password = password }))
        {
            var dicomPrefix = "DICOM/";
            var entries = archive.Entries.Where(e => !e.IsDirectory && e.Key.StartsWith(dicomPrefix));
            if (!entries.Any())
            {
                Console.WriteLine("DICOM folder not found in archive");
                return;
            }

            foreach (var entry in entries)
            {
                var relativePath = entry.Key.Substring(dicomPrefix.Length);
                var targetPath = Path.Combine(targetFolder, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                using (var entryStream = entry.OpenEntryStream())
                {
                    using (var targetStream = File.Create(targetPath))
                    {
                        entryStream.CopyTo(targetStream);
                    }
                }
                Console.WriteLine($"Extracted: {entry.Key}");
            }
            Console.WriteLine("DICOM folder successfully extracted");
        }
    }
}
