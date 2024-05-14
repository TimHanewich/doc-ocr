using System;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;

namespace DocOCR
{
    public class Program
    {
        public static void Main(string[] args)
        {
            FormatAll();
        }

        public static async Task ReadAll()
        {
            string[] ImagePaths = System.IO.Directory.GetFiles(@"C:\Users\timh\Downloads\palmero hoa\images");
            
            //Collect
            List<ImageReadTask> ReadTasks = new List<ImageReadTask>();
            foreach (string path in ImagePaths)
            {
                string p = path.Replace(".jpg", "");
                int lastunder = p.LastIndexOf("_");
                string num = p.Substring(lastunder + 1);
                int numi = Convert.ToInt32(num);
                ImageReadTask irt = new ImageReadTask();
                irt.Page = numi;
                irt.Path = path;
                ReadTasks.Add(irt);
            }

            //Sort
            List<ImageReadTask> ReadTasksSorted = new List<ImageReadTask>();
            while (ReadTasks.Count > 0)
            {
                ImageReadTask lowest = ReadTasks[0];
                foreach (ImageReadTask irt in ReadTasks)
                {
                    if (irt.Page < lowest.Page)
                    {
                        lowest = irt;
                    }
                }
                ReadTasksSorted.Add(lowest);
                ReadTasks.Remove(lowest);
            }

            //Extract each
            foreach (ImageReadTask irt in ReadTasksSorted)
            {
                Console.Write("Reading page " + irt.Page.ToString() + "... ");
                string ocr = await ReadAsync(irt.Path);
                irt.Read = ocr;
                Console.WriteLine("Read " + ocr.Length.ToString("#,##0") + " characters!");
            }

            //Write
            Console.WriteLine("Writing...");
            System.IO.File.WriteAllText(@"C:\Users\timh\Downloads\palmero hoa\src\all.json", JsonConvert.SerializeObject(ReadTasksSorted));
            Console.WriteLine("Wrote!");
        }

        public static void FormatAll()
        {
            string content = System.IO.File.ReadAllText(@"C:\Users\timh\Downloads\palmero hoa\src\all.json");
            ImageReadTask[]? irts = JsonConvert.DeserializeObject<ImageReadTask[]>(content);
            if (irts != null)
            {
                string all = "";
                foreach (ImageReadTask irt in irts)
                {
                    all = all + irt.Read + Environment.NewLine + Environment.NewLine;
                }
                System.IO.File.WriteAllText(@"C:\Users\timh\Downloads\palmero hoa\src\all.txt", all);
            }
        }

        public static async Task<string> ReadAsync(string path)
        {
            Stream s = System.IO.File.OpenRead(path);
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = HttpMethod.Post;
            req.RequestUri = new Uri("https://testcomputervision20240514.cognitiveservices.azure.com/computervision/imageanalysis:analyze?api-version=2023-02-01-preview&features=read");
            req.Content = new StreamContent(s);
            req.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            req.Headers.Add("Ocp-Apim-Subscription-Key", "<KEY HERE!>");
            
            //Request
            HttpClient hc = new HttpClient();
            HttpResponseMessage resp = await hc.SendAsync(req);
            string content = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Response '" + resp.StatusCode.ToString() + " from API. Msg: " + content);
            }

            //Extract
            JObject jo = JObject.Parse(content);
            JToken? token = jo.SelectToken("readResult.content");
            if (token != null)
            {
                string? ss = token.Value<string>();
                if (ss != null)
                {
                    return ss;
                }
                else
                {
                    throw new Exception("Unable to extract string from readResult content.");
                }
            }
            else
            {
                throw new Exception("Unable to find readResult in response!");
            }
        }
    

    
    }
}