using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SplitTiff
{
    class Program
    {

        static string path = @"C:\2021_Local\OCR\Documents\Google Pilot Test Docs Batch 2";
        static string splitPath = @"C:\2021_Local\OCR\Documents\Google Pilot Test Docs Batch 2\SinglePage";
        //static string form1040Path = @"C:\2021_Local\OCR\Documents\Google Pilot Test Docs Batch 2\Form1040";
        //static string classifiedPath = @"C:\2021_Local\OCR\Documents\Google Pilot Test Docs Batch 2\Google Classified and Split";

        //static string form1040Path = @"C:\2021_Local\OCR\Documents\Google Pilot Test Docs Batch 2\Form1040";




        static void Main(string[] args)
        {
            SplitTiff();
        }

        static void SplitTiff()
        {
            var files = Directory.GetFiles(path, "3459817085_21_2635068845.tif").Select(s => new FileInfo(s));

            //string newName = System.IO.Path.GetFileNameWithoutExtension(CurrentFile);

            foreach (var file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file.Name);

                var img = Image.FromFile(file.FullName);

                var pageCount = img.GetFrameCount(System.Drawing.Imaging.FrameDimension.Page);

                for (var p = 0; p < pageCount; p++)
                {

                    img.SelectActiveFrame(FrameDimension.Page, p);

                    img.Save($@"{splitPath}\{name}_{p + 1}.tif");

                }


                //var newFile = new FileInfo($@"{file.Directory.FullName}\{newName}.tif");
                //img.Save(newFile.FullName, System.Drawing.Imaging.ImageFormat.Tiff);
            }
        }


        static void CombineTiff()
        {

            var files = File.ReadAllLines(@"C:\2021_Local\OCR\Documents\Google Pilot Test Docs Batch 2\SinglePage\DocumentSet.txt");


            var filePackages = files.Distinct().Select(f =>
            {
                var match = Regex.Match(f, @"(\d+_\d+_\d+)(_)(\d+)(\s*.?)");

                return new SingleFile() { FullName = "3" + match.Groups[1].Value + "_" + match.Groups[3].Value, Name = "3" + match.Groups[1].Value, PageNumber = int.Parse(match.Groups[3].Value), SinglePage = !string.IsNullOrWhiteSpace(match.Groups[4].Value) };
            }).OrderBy(f => f.Name).ThenBy(f => f.PageNumber).ToList(); //.GroupBy(f => f.Name).ToDictionary(g => g.Key, g => g.OrderBy(f => f.PageNumber).ToList());

            for (var f = 0; f < filePackages.Count; f++)
            {

                var file = filePackages[f];

                //var pageNumber = -1;
                Image img = null;

                //Select image encoder
                System.Drawing.Imaging.Encoder enc = System.Drawing.Imaging.Encoder.SaveFlag;
                EncoderParameters encoderparams = new EncoderParameters(1);


                // Select ImageCodecInfo for tiff file format
                ImageCodecInfo info = null;
                info = (from ie in ImageCodecInfo.GetImageEncoders()
                        where ie.MimeType == "image/tiff"
                        select ie).FirstOrDefault();

                img = Image.FromFile($@"{splitPath}\{file.FullName}.tif");

                //make this file multi frame to be able to add multiple images
                encoderparams.Param[0] = new EncoderParameter(enc, (long)EncoderValue.MultiFrame);

                var fileName = $@"C:\2021_Local\OCR\Documents\Google Pilot Test Docs Batch 2\Visually Split\{file.Name}_V_{file.PageNumber}";

                ////if (package.Value.Count > 1)
                ////{
                ////    fileName += $@"_{package.Value.Last().PageNumber}";
                ////}

                //Save the bitmap
                img.Save($@"{fileName}.tif", info, encoderparams);

                encoderparams.Param[0] = new EncoderParameter(enc, (long)EncoderValue.FrameDimensionPage);

                if (f < filePackages.Count - 1 && !file.SinglePage)
                {
                    var nextFile = filePackages[f + 1];

                    if (nextFile.Name == file.Name && nextFile.PageNumber == file.PageNumber + 1)
                    {
                        var curImg = Image.FromFile($@"{splitPath}\{nextFile.FullName}.tif");

                        //add another images and Repeat this process to Add Multiple Images
                        img.SaveAdd(curImg, encoderparams);

                        f++;
                    }
                }

                encoderparams.Param[0] = new EncoderParameter(enc, (long)EncoderValue.Flush);
                img.SaveAdd(encoderparams);

            }


        }


        //        static void CombineTiffByGoogleClassification()
        //        {


        //            List<(string FileName, string Classified, int[] Pages)> classified = new List<(string, string, int[])>()
        //        {
        //           ("3454168972_35_2635263838", "1040_2019", new int[] { 1,2,} ),
        //("3459261858_21_2635104514", "1040_2018", new int[] { 5,6,} ),
        //("3459261858_21_2635104514", "w2_2018", new int[] { 11,} ),
        //("3459261858_21_2635104514", "1040_2018", new int[] { 18,19,} ),
        //("3459267114_35_2635112976", "w2_2020", new int[] { 0,} ),
        //("3459267114_35_2635112976", "w2_2020", new int[] { 1,} ),
        //("3459267114_35_2635112976", "w2_2019", new int[] { 2,} ),
        //("3459267114_35_2635112976", "w2_2018", new int[] { 3,} ),
        //("3459279771_35_2635394620", "w2_2019", new int[] { 0,} ),
        //("3459279771_35_2635406179", "w2_2018", new int[] { 0,} ),
        //("3459279771_35_2635406179", "w2_2018", new int[] { 1,} ),
        //("3459288932_21_2635124142", "w2_2019", new int[] { 0,} ),
        //("3459291726_21_2635399171", "1040_2018", new int[] { 28,29,} ),
        //("3459291726_21_2635399171", "1040_2019", new int[] { 80,81,} ),
        //("3459294205_21_2635449310", "1040_2019", new int[] { 2,3,} ),
        //("3459294205_21_2635449310", "1040", new int[] { 25,26,} ),
        //("3459334874_21_2635142676", "w2_2018", new int[] { 2,} ),
        //("3459334874_21_2635142676", "1040_2018", new int[] { 8,9,} ),
        //("3459353589_35_2635218071", "1040_2019", new int[] { 1,2,} ),
        //("3459432927_21_2635440511", "w2_2019", new int[] { 0,} ),
        //("3459432927_21_2635440511", "w2_2019", new int[] { 1,} ),
        //("3459553297_35_2635237956", "w2_2018", new int[] { 0,} ),
        //("3459553297_35_2635237956", "w2_2019", new int[] { 1,} ),
        //("3459572850_21_2635327718", "1040_2018", new int[] { 5,6,} ),
        //("3459572850_21_2635327718", "w2_2018", new int[] { 23,} ),
        //("3459572850_21_2635327718", "1040_2018", new int[] { 24,25,} ),
        //("3459572850_21_2635327718", "1040_2019", new int[] { 37,38,} ),
        //("3459572850_21_2635327718", "1040_2019", new int[] { 54,55,} ),
        //("3459572850_21_2635327718", "1040_2019", new int[] { 68,69,} ),
        //("3459582155_21_2635220056", "1040_2018", new int[] { 6,7,} ),
        //("3459582155_21_2635220056", "1040", new int[] { 24,25,26,} ),
        //("3459582155_21_2635220056", "w2_2018", new int[] { 65,} ),
        //("3459582155_21_2635220056", "w2_2018", new int[] { 66,} ),
        //("3459582155_21_2635220056", "w2_2018", new int[] { 77,} ),
        //("3459582155_21_2635220056", "w2_2018", new int[] { 80,} ),
        //("3459582155_21_2635225080", "1040_2018", new int[] { 6,7,} ),
        //("3459582155_21_2635225080", "1040", new int[] { 24,25,26,} ),
        //("3459582155_21_2635225080", "w2_2018", new int[] { 65,} ),
        //("3459582155_21_2635225080", "w2_2018", new int[] { 66,} ),
        //("3459582155_21_2635225080", "w2_2018", new int[] { 77,} ),
        //("3459582155_21_2635225080", "w2_2018", new int[] { 80,} ),
        //("3459648695_21_2635128841", "w2_2019", new int[] { 0,} ),
        //("3459654671_21_2635305139", "1099misc_2019", new int[] { 0,} ),
        //("3459666390_21_2635460772", "w2_2018", new int[] { 23,} ),
        //("3459678330_21_2635378039", "w2_2018", new int[] { 0,} ),
        //("3459678330_21_2635378039", "w2_2018", new int[] { 1,} ),
        //("3459678330_21_2635378039", "w2_2019", new int[] { 2,} ),
        //("3459687191_21_2634885430", "1040_2018", new int[] { 0,1,} ),
        //("3459687191_21_2634885430", "1040_2019", new int[] { 41,42,} ),
        //("3459745269_35_2635202915", "1040_2019", new int[] { 0,1,} ),
        //("3459746769_21_2634830622", "1040_2018", new int[] { 0,1,} ),
        //("3459746769_21_2634830622", "1040", new int[] { 25,26,} ),
        //("3459757561_21_2635053940", "w9_2018", new int[] { 2,} ),
        //("3459757561_21_2635053940", "w2_2018", new int[] { 4,} ),
        //("3459757561_21_2635053940", "w2_2018", new int[] { 19,} ),
        //("3459757561_21_2635053940", "1040_2018", new int[] { 20,21,} ),
        //("3459789530_21_2634777306", "w2_2020", new int[] { 0,} ),
        //("3459789530_21_2634781942", "w2_2019", new int[] { 2,} ),
        //("3459808244_21_2635191756", "w2_2019", new int[] { 0,} ),
        //("3459808244_21_2635191756", "w2_2019", new int[] { 1,} ),
        //("3459817085_21_2635068845", "w2_2019", new int[] { 0,} ),
        //("3459817085_21_2635068845", "w2_2019", new int[] { 1,} ),
        //("3454168972_35_2635263838", "1040_2019", new int[] { 1,2,} ),
        //("3459261858_21_2635104514", "1040_2018", new int[] { 5,6,} ),
        //("3459261858_21_2635104514", "w2_2018", new int[] { 11,} ),
        //("3459261858_21_2635104514", "1040_2018", new int[] { 18,19,} ),
        //("3459267114_35_2635112976", "w2_2020", new int[] { 0,} ),
        //("3459267114_35_2635112976", "w2_2020", new int[] { 1,} ),
        //("3459267114_35_2635112976", "w2_2019", new int[] { 2,} ),
        //("3459267114_35_2635112976", "w2_2018", new int[] { 3,} ),
        //("3459279771_35_2635394620", "w2_2019", new int[] { 0,} ),
        //("3459279771_35_2635406179", "w2_2018", new int[] { 0,} ),
        //("3459279771_35_2635406179", "w2_2018", new int[] { 1,} ),
        //("3459288932_21_2635124142", "w2_2019", new int[] { 0,} ),
        //("3459291726_21_2635399171", "1040_2018", new int[] { 28,29,} ),
        //("3459291726_21_2635399171", "1040_2019", new int[] { 80,81,} ),
        //("3459294205_21_2635449310", "1040_2019", new int[] { 2,3,} ),
        //("3459294205_21_2635449310", "1040", new int[] { 25,26,} ),
        //("3459334874_21_2635142676", "w2_2018", new int[] { 2,} ),
        //("3459334874_21_2635142676", "1040_2018", new int[] { 8,9,} ),
        //("3459353589_35_2635218071", "1040_2019", new int[] { 1,2,} ),
        //("3459432927_21_2635440511", "w2_2019", new int[] { 0,} ),
        //("3459432927_21_2635440511", "w2_2019", new int[] { 1,} ),
        //("3459553297_35_2635237956", "w2_2018", new int[] { 0,} ),
        //("3459553297_35_2635237956", "w2_2019", new int[] { 1,} ),
        //("3459572850_21_2635327718", "1040_2018", new int[] { 5,6,} ),
        //("3459572850_21_2635327718", "w2_2018", new int[] { 23,} ),
        //("3459572850_21_2635327718", "1040_2018", new int[] { 24,25,} ),
        //("3459572850_21_2635327718", "1040_2019", new int[] { 37,38,} ),
        //("3459572850_21_2635327718", "1040_2019", new int[] { 54,55,} ),
        //("3459572850_21_2635327718", "1040_2019", new int[] { 68,69,} ),
        //("3459582155_21_2635220056", "1040_2018", new int[] { 6,7,} ),
        //("3459582155_21_2635220056", "1040", new int[] { 24,25,26,} ),
        //("3459582155_21_2635220056", "w2_2018", new int[] { 65,} ),
        //("3459582155_21_2635220056", "w2_2018", new int[] { 66,} ),
        //("3459582155_21_2635220056", "w2_2018", new int[] { 77,} ),
        //("3459582155_21_2635220056", "w2_2018", new int[] { 80,} ),
        //("3459582155_21_2635225080", "1040_2018", new int[] { 6,7,} ),
        //("3459582155_21_2635225080", "1040", new int[] { 24,25,26,} ),
        //("3459582155_21_2635225080", "w2_2018", new int[] { 65,} ),
        //("3459582155_21_2635225080", "w2_2018", new int[] { 66,} ),
        //("3459582155_21_2635225080", "w2_2018", new int[] { 77,} ),
        //("3459582155_21_2635225080", "w2_2018", new int[] { 80,} ),
        //("3459648695_21_2635128841", "w2_2019", new int[] { 0,} ),
        //("3459654671_21_2635305139", "1099misc_2019", new int[] { 0,} ),
        //("3459666390_21_2635460772", "w2_2018", new int[] { 23,} ),
        //("3459678330_21_2635378039", "w2_2018", new int[] { 0,} ),
        //("3459678330_21_2635378039", "w2_2018", new int[] { 1,} ),
        //("3459678330_21_2635378039", "w2_2019", new int[] { 2,} ),
        //("3459687191_21_2634885430", "1040_2018", new int[] { 0,1,} ),
        //("3459687191_21_2634885430", "1040_2019", new int[] { 41,42,} ),
        //("3459745269_35_2635202915", "1040_2019", new int[] { 0,1,} ),
        //("3459746769_21_2634830622", "1040_2018", new int[] { 0,1,} ),
        //("3459746769_21_2634830622", "1040", new int[] { 25,26,} ),
        //("3459757561_21_2635053940", "w9_2018", new int[] { 2,} ),
        //("3459757561_21_2635053940", "w2_2018", new int[] { 4,} ),
        //("3459757561_21_2635053940", "w2_2018", new int[] { 19,} ),
        //("3459757561_21_2635053940", "1040_2018", new int[] { 20,21,} ),
        //("3459789530_21_2634777306", "w2_2020", new int[] { 0,} ),
        //("3459789530_21_2634781942", "w2_2019", new int[] { 2,} ),
        //("3459808244_21_2635191756", "w2_2019", new int[] { 0,} ),
        //("3459808244_21_2635191756", "w2_2019", new int[] { 1,} ),
        //("3459817085_21_2635068845", "w2_2019", new int[] { 0,} ),
        //("3459817085_21_2635068845", "w2_2019", new int[] { 1,} )



        //        };

        //            // Google is 0 based for page numbers
        //            // The split is 1 based
        //            classified.ForEach(c =>
        //            {
        //                for (var i = 0; i < c.Pages.Length; i++)
        //                {
        //                    c.Pages[i]++;
        //                }
        //            });

        //            foreach (var package in classified)
        //            {

        //                //var pageNumber = -1;
        //                Image img = null;

        //                //Select image encoder
        //                System.Drawing.Imaging.Encoder enc = System.Drawing.Imaging.Encoder.SaveFlag;
        //                EncoderParameters encoderparams = new EncoderParameters(1);


        //                // Select ImageCodecInfo for tiff file format
        //                ImageCodecInfo info = null;
        //                info = (from ie in ImageCodecInfo.GetImageEncoders()
        //                        where ie.MimeType == "image/tiff"
        //                        select ie).FirstOrDefault();

        //                int firstPage = 1;

        //                if (package.Pages.Length > 0)
        //                {
        //                    firstPage = package.Pages.First();
        //                }

        //                var fileName = package.FileName;

        //                img = Image.FromFile($@"{splitPath}\{fileName}_{firstPage}.tif");

        //                //make this file multi frame to be able to add multiple images
        //                encoderparams.Param[0] = new EncoderParameter(enc, (long)EncoderValue.MultiFrame);

        //                //Save the bitmap
        //                var name = $@"{classifiedPath}\{fileName}_M_{package.Classified}_{firstPage}";

        //                if (package.Pages.Length > 1)
        //                {
        //                    name += $"_{package.Pages.Last()}";
        //                }

        //                img.Save($"{name}.tif", info, encoderparams);

        //                encoderparams.Param[0] = new EncoderParameter(enc, (long)EncoderValue.FrameDimensionPage);


        //                foreach (var page in package.Pages.Skip(1))
        //                {

        //                    var curImg = Image.FromFile($@"{splitPath}\{fileName}_{page}.tif");

        //                    //add another images and Repeat this process to Add Multiple Images
        //                    img.SaveAdd(curImg, encoderparams);

        //                }


        //                encoderparams.Param[0] = new EncoderParameter(enc, (long)EncoderValue.Flush);
        //                img.SaveAdd(encoderparams);

        //            }


        //        }


        public struct SingleFile
        {
            public string FullName { get; set; }
            public string Name { get; set; }
            public int PageNumber { get; set; }
            public bool SinglePage { get; set; }
        }
    }
}
