using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ImgProcLib;
using System.Drawing;

namespace ViewModel
{

    public class Client
    {

        private static readonly string url = "http://localhost:5000/api/Images";
        public event Action<ReturnMessage> OnRecognizedImages;
        public event Action<Tuple<string, int>> OnGetDatabaseStat;
        public event Action OnServerOffline;

        private HttpClient httpClient = new HttpClient();

        public async Task ProcessDirectory(string dirr)
        {
            List<TransferStruct> transferStructsList = await MyPost(dirr);
            foreach (var t in transferStructsList)
            {
                ReturnMessage rm = new ReturnMessage(t.FilePath, t.PredictionStringResult);
                rm.ByteImage = Convert.FromBase64String(t.JpegImage);
                OnRecognizedImages(rm);
            }
            // Console.WriteLine("Num of returned messages = " + transferStructsList.Count);
            await GetDbStatistic();
        }
        public async void LoadAllPreviousImages()
        {
            HttpResponseMessage httpResponse;
            try
            {

                httpResponse = await httpClient.GetAsync(url);
                List<TransferStruct> transferStructsList = JsonConvert.DeserializeObject<List<TransferStruct>>(httpResponse.Content.ReadAsStringAsync().Result);
                foreach (var t in transferStructsList)
                {
                    
                    ReturnMessage rm = new ReturnMessage(t.FilePath, t.PredictionStringResult);
                    rm.ByteImage = Convert.FromBase64String(t.JpegImage);
                    OnRecognizedImages(rm);
                }
            }
            catch
            {
                OnServerOffline();
                return;
            }
                await GetDbStatistic();
        }
        public async void ClearDB()
        {
            try
            {
                await httpClient.DeleteAsync(url);
            }
            catch
            {
                OnServerOffline();
            }
        }
        public async Task GetDbStatistic()
        {
            try
            {
                Console.WriteLine(url + "/stat");
                var result = await httpClient.GetStringAsync(url + "/stat");

                foreach (var i in JsonConvert.DeserializeObject<List<Tuple<string,int>>>(result))
                {
                    OnGetDatabaseStat(i);
                }

            }
            catch
            {
                OnServerOffline();
            }
        }


        public async Task<List<TransferStruct>> MyPost(string FilePath)
        {
            List<TransferStruct> transferStructsList = new List<TransferStruct>();
            var dirrPath = new StringContent(JsonConvert.SerializeObject(FilePath), Encoding.UTF8, "application/json");
            HttpResponseMessage httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InsufficientStorage);
            try
            {
                httpResponse = await httpClient.PostAsync(url, dirrPath);

            }
            catch (HttpRequestException)
            {
                OnServerOffline();
            }

            if (httpResponse.IsSuccessStatusCode)
            {
                transferStructsList = JsonConvert.DeserializeObject<List<TransferStruct>>(httpResponse.Content.ReadAsStringAsync().Result);

            }
            return transferStructsList;


        }

        public void StopProcessing()
        {
            Console.WriteLine("client.StopProcessing called");
            httpClient.GetAsync(url +"/stop");
        }
        
        public class TransferStruct
        {
            public string FilePath { get; set; }
            public string PredictionStringResult { get; set; }
            public string JpegImage { get; set; }
        }
    }
}