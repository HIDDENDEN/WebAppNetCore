using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ImgProcLib;

namespace HelloClient
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            HttpClient client = new HttpClient();
            // string result = await client.GetStringAsync("http://localhost:5000/hello?name=Me");
            string result = await client.GetStringAsync("http://localhost:5000/api/Images");
            Console.WriteLine(result);

            //Get results from server logic
            //================
            var allImages = JsonConvert.DeserializeObject<ReturnMessage[]>(result);
            foreach (var img in allImages)
            {
                //process returned messages
                Console.WriteLine(img.PredictionStringResult);
                //todo
            }
            //================


            //Put result on from server. logic
            //================
            ReturnMessage newResult = new ReturnMessage("", "");
            //ignore warnings. Our push won't be async
            await PushResultsOnServer(newResult, client);

            //================
            
            return 0;
        }

        static async Task PushResultsOnServer(ReturnMessage newResult, HttpClient client)
        {
            var s = JsonConvert.SerializeObject(newResult);
            var c = new StringContent(s);
            c.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            await client.PutAsync("http://localhost:5000/api/Images", c);
        }
    }
}
