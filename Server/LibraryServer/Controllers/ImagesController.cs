using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ImgProcLib;
using System.IO;
using System.Threading;

namespace LibraryServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {

        void PredictionHandler_Controller(object sender, PredictionEventArgs e)
        {
            // Console.WriteLine("Queue " + ( e.RecognitionResult).ToString());
            ReturnMessage rm = e.RecognitionResult;
            lock (imagesLibraryContext)
            {
                // заносим результаты распознования в БД
                imagesLibraryContext.AddRecognitionResultToDatabase(rm);
            }

            AddMessageToTransferStructList(rm);

        }


        void DatabaseFilesHandler(ReturnMessage rm)
        {
            AddMessageToTransferStructList(rm);
        }

        PredictionQueue predictionQueue;
        List<TransferStruct> transferStructList;
        //DATABASE
        ImagesLibraryContext imagesLibraryContext;


        [HttpGet]
        public List<TransferStruct> GetImages()
        {
            transferStructList = new List<TransferStruct>();
            imagesLibraryContext = new ImagesLibraryContext();
            List<string> files = new List<string>();
            foreach (var image in imagesLibraryContext.Images)
            {
                files.Add(image.Path);
            }
            foreach (var file in files)
            {
                ReturnMessage returnMessageFromDB = null;
                try
                {
                    returnMessageFromDB = imagesLibraryContext.SearchFile(file);
                }
                catch
                {
                    Console.WriteLine("No such file in db : " + file.ToString());
                }

                if (returnMessageFromDB == null)
                //didn't find same file in database
                {
                    Console.WriteLine(file + "  doesn't contain's in Database");
                    // newFiles.Add(file);
                }

                else
                //else we found it in database and process it
                {
                    DatabaseFilesHandler(returnMessageFromDB);

                }
            }
            return transferStructList;
        }


        ImageProcClass imgProc = new ImageProcClass("");

        [HttpPost]
        public async Task<List<TransferStruct>> Post([FromBody] string folderPath)
        {
            imagesLibraryContext = new ImagesLibraryContext();
            predictionQueue = new PredictionQueue();
            predictionQueue.Enqueued += PredictionHandler_Controller;
            transferStructList = new List<TransferStruct>();

            // imgProc = new ImageProcClass(folderPath);

            //===========================
            //Define new image or not
            //launch recognition
            imgProc.SetDirr(folderPath);

            //know what files are not in database and need to be processed
            string[] unRecognizedImageFiles = getUnProcessedImageFiles(folderPath);

            ImageProcClass.filePaths = unRecognizedImageFiles;

            //if we alredy have processed that folder
            if (ImageProcClass.filePaths != null)
            {
                await imgProc.StartProc(predictionQueue);//Launch Image processing
            }

            //============================


            return transferStructList;
        }

        [HttpGet, Route("stop")]
        public void Stop()
        {

            if (imgProc != null)
            {
                imgProc.InterruptTasks();
            }
        }

        //returns num of name includes in Database
        [HttpGet, Route("stat")]
        public List<Tuple<string, int>> Get()
        {
            ImagesLibraryContext imagesLibraryContext = new ImagesLibraryContext();
            List<Tuple<string, int>> retList = imagesLibraryContext.GetNumOfEachType();
            Console.WriteLine("Stat list length = " + retList.Count());
            return retList;
        }

        //[HttpGet, Route("pageNumber")]
        [HttpGet("{pageNumber}")]
        public TransferStruct[] Get(int pageNumber)
        {
            Console.WriteLine("I am here");

            transferStructList = new List<TransferStruct>();
            imagesLibraryContext = new ImagesLibraryContext();
            List<string> files = new List<string>();
            foreach (var image in imagesLibraryContext.Images)
            {
                files.Add(image.Path);
            }
            foreach (var file in files)
            {
                ReturnMessage returnMessageFromDB = null;
                try
                {
                    returnMessageFromDB = imagesLibraryContext.SearchFile(file);
                }
                catch
                {
                    Console.WriteLine("No such file in db : " + file.ToString());
                }

                if (returnMessageFromDB == null)
                //didn't find same file in database
                {
                    Console.WriteLine(file + "  doesn't contain's in Database");
                    // newFiles.Add(file);
                }

                else
                //else we found it in database and process it
                {
                    DatabaseFilesHandler(returnMessageFromDB);

                }
            }
            
            List<TransferStruct> answer = new List<TransferStruct>();
            Console.WriteLine(transferStructList.Count());

            for (int i = pageNumber * 10; i < Math.Min(10 * (pageNumber + 1), transferStructList.Count()); i++)
            {
                answer.Add(transferStructList[i]);
            }
            return answer.ToArray();
            
        }


        [HttpDelete]
        public void Delete()
        {
            imagesLibraryContext = new ImagesLibraryContext();
            imagesLibraryContext.ResetDatabase();
            Console.WriteLine("Database reset called");

            flagLastActionDbClear = true;
        }


        string JpegToStringConvert(byte[] byteArr, string path)
        {
            var blobByte = from image in imagesLibraryContext.Images
                           where image.Path == path
                           select image.ImageRecognizedDetails.BinaryFile;
            string jpegString = Convert.ToBase64String(blobByte.First());
            return jpegString;
        }



















        string[] getUnProcessedImageFiles(string folderPath)
        {
            string[] allFilesInFolder = null;
            string[] unProcessedImageFiles = null;
            try
            {
                allFilesInFolder = Directory.GetFiles(folderPath, "*.jpg");
                // Console.WriteLine("filePath[0] " + ImageProcClass.filePaths[0]);
            }
            catch (Exception)
            {
                Console.WriteLine("Next message is from method 'StartProc()' ");
                Console.WriteLine("Looks like you entered incorrect filepath");
                Console.WriteLine("Or there is no jpg images in your folder");
                //Console.WriteLine(e.ToString());
            }
            if (allFilesInFolder != null)
            {
                //if we cleared database last time then process all files
                if (flagLastActionDbClear == true)
                {
                    flagLastActionDbClear = false;
                    return allFilesInFolder;
                }

                unProcessedImageFiles = FilterImagesInDatabase(allFilesInFolder);
            }
            return unProcessedImageFiles;
        }

        string[] FilterImagesInDatabase(string[] allFiles)
        {
            List<string> newFiles = new List<string>();

            ReturnMessage returnMessageFromDB = null;
            foreach (string file in allFiles)
            {
                try
                {
                    returnMessageFromDB = imagesLibraryContext.SearchFile(file);
                }
                catch
                {
                    Console.WriteLine("Catched");
                }

                if (returnMessageFromDB == null)
                //didn't find same file in database
                {
                    newFiles.Add(file);
                }

                else
                //else we found it in database and process it
                {
                    DatabaseFilesHandler(returnMessageFromDB);

                }
            }
            if (newFiles.Count() == 0)
                return null;
            else
            {
                return newFiles.ToArray();
            }
        }

        bool flagLastActionDbClear = false;

        void AddMessageToTransferStructList(ReturnMessage rm)
        {
            TransferStruct tr = new TransferStruct();
            tr.FilePath = rm.FullFilePath;
            tr.JpegImage = JpegToStringConvert(rm.ByteImage, rm.FullFilePath);
            tr.PredictionStringResult = rm.PredictionStringResult;
            if (rm.PredictionStringResult != null)
                if (rm.PredictionStringResult.Length > 1)
                    transferStructList.Add(tr);
        }
    }


    public class TransferStruct
    {
        public string FilePath { get; set; }
        public string PredictionStringResult { get; set; }
        public string JpegImage { get; set; }
    }
}
