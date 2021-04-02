using System;
using System.IO;
using System.Linq;
using System.Drawing;

namespace ConvertImageToTiff
{
    class Program
    {
        static void Main(string[] args)
        {

            string path = @"C:\2021_Local\OCR\Documents\Fake\Mock Documents for Testing";

            var files = Directory.GetFiles(path, "*.jpg", SearchOption.AllDirectories).Select(s => new FileInfo(s));

            //string newName = System.IO.Path.GetFileNameWithoutExtension(CurrentFile);

            foreach (var file in files)
            {
                string newName = Path.GetFileNameWithoutExtension(file.Name);

                var img = Image.FromFile(file.FullName);

                var newFile = new FileInfo($@"{file.Directory.FullName}\{newName}.tif");
                img.Save(newFile.FullName, System.Drawing.Imaging.ImageFormat.Tiff);
            }

        }
    }
}
