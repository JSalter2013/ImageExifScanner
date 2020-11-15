using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static ImageExifScanner.ExifPropertyHelper;

namespace ImageExifScanner
{
    class Program
    {
        private static string folder = @"e:\our pics";
        private static string saveFile = @"e:\image_metadata_ourpics.txt";
        private static char saveFileDelimiter = '?';

        private static string[] extensions = { "*.jpg", "*.png", "*.jpeg" };
        //string[] extensions = { "*.jpg", "*.gif", "*.png", "*.jpeg", "*.tiff", "*.bmp", "*.webp", "*.psd", "*.raw" };

        private static ExifPropertyDataTypes[] filterTypesList = { ExifPropertyDataTypes.String };

        private static ConcurrentQueue<ImageDetails> _imageDetailsQueue = new ConcurrentQueue<ImageDetails>();
        
        public static async Task Main(string[] args)
        {
            Parallel.ForEach(extensions, (extension) =>
            {
                Console.WriteLine($"Processing {extension} files...");

                string[] allFiles = Directory.GetFiles(folder, extension, SearchOption.AllDirectories);

                Parallel.ForEach(allFiles, (currentFile) =>
                {
                    try
                    {
                        using var bitmap = new Bitmap(currentFile);
                        foreach (var x in GetExifProperties(bitmap).Where(x => filterTypesList.Any(y => y == x.DataType)))
                        {
                            _imageDetailsQueue.Enqueue(new ImageDetails
                            {
                                FileName = currentFile,
                                Width = bitmap.Width,
                                Height = bitmap.Height,
                                ExifPropertyType = x.PropertyType,
                                ExifString = x.DataString
                            });
                        }
                    }
                    catch {
                        Console.WriteLine($"Ignored: {currentFile}");
                    }
                });
            });

            using var outStream = new StreamWriter(saveFile);
            foreach (var j in _imageDetailsQueue)
            {
                await outStream.WriteLineAsync($"{j.FileName}{saveFileDelimiter}{j.Width}{saveFileDelimiter}{j.Height}{saveFileDelimiter}{j.ExifPropertyType}{saveFileDelimiter}{j.ExifString}");
            }
        }
    }

    public class ImageDetails
    {
        public string FileName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public ExifPropertyTypes ExifPropertyType { get; set; }
        public string ExifString { get; set; }
    }
}
