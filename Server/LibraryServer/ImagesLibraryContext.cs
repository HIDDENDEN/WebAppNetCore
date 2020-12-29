using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using ImgProcLib;

namespace LibraryServer
{
    public class ImageRecognized
    {
        public int Id { get; set; }
        public string ImageName { get; set; }
        public string Path { get; set; }
        public PredictionResult ImageType { get; set; }
        public ImageRecognizedDetails ImageRecognizedDetails { get; set; }
    }

    public class PredictionResult
    {

        public int Id { get; set; }
        public string PredictionStringResult { get; set; }
        public ICollection<ImageRecognized> RecognizedImages { get; set; }
    }

    public class ImageRecognizedDetails
    {
        public int Id { get; set; }

        public byte[] BinaryFile { get; set; }
    }


    public class ImagesLibraryContext : DbContext
    {

        //some trick from stackoverflow
        public ImagesLibraryContext() { Database.EnsureCreated(); }


        public DbSet<ImageRecognized> Images { get; set; }
        public DbSet<PredictionResult> TypesOfImages { get; set; }
        public DbSet<ImageRecognizedDetails> Details { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder o)
        => o.UseSqlite("Data Source=library.db");

        //Check if database contains recognition results for asked file
        //return null if nothing found
        public ReturnMessage SearchFile(string imagePath)
        {
            //know name of image
            string fileName = Path.GetFileName(imagePath);

            int numOfRepeatedFiles = 0;
            var query = Images.Where(obj => obj.Path.Equals(imagePath));
            numOfRepeatedFiles = query.Count();

            if (numOfRepeatedFiles > 0)

            {

                ImageRecognized imageRecognized = null;

                foreach (var fileInQuery in query)
                {

                    Entry(fileInQuery).Reference("ImageRecognizedDetails").Load();



                    byte[] bdBinaryFile = fileInQuery.ImageRecognizedDetails.BinaryFile;


                    Stream stream = File.OpenRead(imagePath);
                    byte[] byteArrayImage = new byte[stream.Length];
                    stream.Read(byteArrayImage, 0, (int)stream.Length);



                    if (IsByteArraysEqual(bdBinaryFile, byteArrayImage))
                    {

                        imageRecognized = fileInQuery;

                        Entry(fileInQuery).Reference("ImageType").Load();
                        break;
                    }
                }
                //found needed file
                if (imageRecognized != null)
                {
                    Entry(imageRecognized).Reference("ImageType").Load();

                    string s = null;
                    try
                    {
                        s = imageRecognized.ImageType.PredictionStringResult;
                    }
                    catch (NullReferenceException e)
                    {

                    }
                    if (s == null)
                        Console.WriteLine(imagePath);
                    return new ReturnMessage(imagePath, s);
                }
                else
                //didn't found
                {
                    return null;
                }

            }
            else
            //it's a new image
            {
                return null;
            }
        }

        //Adding new results in database
        public void AddRecognitionResultToDatabase(ReturnMessage processedImage)
        {
            //create new db element that will be added
            ImageRecognized imgStruct = new ImageRecognized();

            imgStruct.ImageName = Path.GetFileName(processedImage.FullFilePath);
            imgStruct.Path = processedImage.FullFilePath;

            imgStruct.ImageRecognizedDetails = new ImageRecognizedDetails();

            //extract binary content from file
            Stream stream = System.IO.File.OpenRead(processedImage.FullFilePath);
            byte[] byteArrayImage = new byte[stream.Length];
            stream.Read(byteArrayImage, 0, (int)stream.Length);

            //set binary content
            imgStruct.ImageRecognizedDetails.BinaryFile = byteArrayImage;

            if (processedImage.PredictionStringResult == null)
            {
                Console.WriteLine("!!!!!! in addtoDB + processedImage.PredictionStringResult ==null");
            }
            var query = TypesOfImages.Where(obj => processedImage.PredictionStringResult.Equals(obj.PredictionStringResult));
            if (query.Count() > 0)
            {
                if (query.First().RecognizedImages == null)
                {
                    imgStruct.ImageType = new PredictionResult();
                    imgStruct.ImageType.PredictionStringResult = processedImage.PredictionStringResult;
                    TypesOfImages.Add(imgStruct.ImageType);
                }
                else
                {

                    imgStruct.ImageType = query.First();

                }
                // imgStruct.ImageType.RecognizedImages = query.First().RecognizedImages;
                // imgStruct.ImageType.PredictionStringResult = processedImage.PredictionStringResult;

                // TypesOfImages.Add(imgStruct.ImageType);
                // if (query.First().PredictionStringResult == null)
                //     Console.WriteLine("zzzzzzzzzzz");
            }
            else
            {
                imgStruct.ImageType = new PredictionResult();
                imgStruct.ImageType.PredictionStringResult = processedImage.PredictionStringResult;
                TypesOfImages.Add(imgStruct.ImageType);
            }

            Details.Add(imgStruct.ImageRecognizedDetails);
            Images.Add(imgStruct);
            SaveChanges();
        }

        //function to check if two byte arrays are equal
        private bool IsByteArraysEqual(byte[] f1, byte[] f2)
        {
            if (f1.Length != f2.Length)
                return false;

            for (int i = 0; i < f1.Length; i++)
                if (f1[i] != f2[i])
                    return false;
            return true;
        }

        //clear external storage
        public void ResetDatabase()
        {
            foreach (var detail in Details)
                Details.Remove(detail);
            foreach (var type in TypesOfImages)
                TypesOfImages.Remove(type);
            foreach (var image in Images)
                Images.Remove(image);
            SaveChanges();
        }
        public List<Tuple<string, int>> GetNumOfEachType()
        {
            List<Tuple<string, int>> retList = new List<Tuple<string, int>>();

            foreach (var t in TypesOfImages.Include((r) => r.RecognizedImages))
            {
                Tuple<string, int> elem = new Tuple<string, int>(t.PredictionStringResult, t.RecognizedImages.Count());
                if (elem.Item1 != null)
                    if (elem.Item1.Length > 1)
                        retList.Add(elem);
                Console.WriteLine("elem = " + elem.Item1 + "   " + elem.Item2);
            }
            return retList;
        }
    }

}