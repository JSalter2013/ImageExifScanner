using MetadataExtractor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageExifScanner
{
    class Program
    {
        private static string folder = @"e:\our pics";
        private static string saveFile = @"e:\image_metadata_ourpics.txt";
        private static char saveFileDelimiter = '?';

        private static string[] extensions = { "*.jpg", "*.png", "*.jpeg" };
        //string[] extensions = { "*.jpg", "*.gif", "*.png", "*.jpeg", "*.tiff", "*.bmp", "*.webp", "*.psd", "*.raw" };
        
        private static ConcurrentQueue<ImageDetails> _imageDetailsQueue = new ConcurrentQueue<ImageDetails>();
        
        public static async Task Main(string[] args)
        {
            Parallel.ForEach(extensions, (extension) =>
            {
                Console.WriteLine($"Processing {extension} files...");

                string[] allFiles = System.IO.Directory.GetFiles(folder, extension, SearchOption.AllDirectories);

                Parallel.ForEach(allFiles, (currentFile) =>
                {
                    IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(currentFile);
                    foreach (var d in directories)
                        foreach (var t in d.Tags)
                        {
                            _imageDetailsQueue.Enqueue(new ImageDetails
                            {
                                FileName = currentFile,
                                ExifPropertyType = t.Name,
                                ExifString = t.Description
                            });
                        }
                });
            });

            using var outStream = new StreamWriter(saveFile);
            foreach (var j in _imageDetailsQueue)
            {
                await outStream.WriteLineAsync($"{j.FileName}{saveFileDelimiter}{j.ExifPropertyType}{saveFileDelimiter}{j.ExifString}");
            }
        }
    }

    public class ImageDetails
    {
        public string FileName { get; set; }
        public string ExifPropertyType { get; set; }
        public string ExifString { get; set; }
    }
}
