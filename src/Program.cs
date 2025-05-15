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
            //Step 1 - read all!
            //This reads a series of images in a folder using OCR and then puts the entire response as a custom JSON object
            //ReadAll().Wait();

            //Step 2 - conver the JSON to plain text
            FormatAll();
        }

        public static async Task ReadAll()
        {
            string[] ImagePaths = System.IO.Directory.GetFiles(@"C:\Users\timh\Downloads\smallpdf-convert-20250515-125854");
            
            //Collect
            List<ImageReadTask> ReadTasks = new List<ImageReadTask>();
            foreach (string path in ImagePaths)
            {
                string num = Path.GetFileNameWithoutExtension(path);
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
            System.IO.File.WriteAllText(@"C:\Users\timh\Downloads\doc-ocr\OUTPUT.json", JsonConvert.SerializeObject(ReadTasksSorted));
            Console.WriteLine("Wrote!");
        }



        public static async Task<string> ReadAsync(string path)
        {
            Stream s = System.IO.File.OpenRead(path);
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = HttpMethod.Post;
            req.RequestUri = new Uri("https://20250515-cv-ocr.cognitiveservices.azure.com/computervision/imageanalysis:analyze?api-version=2024-02-01&features=read");
            req.Content = new StreamContent(s);
            req.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            req.Headers.Add("Ocp-Apim-Subscription-Key", "<key here!>");

            //Request
            HttpClient hc = new HttpClient();
            HttpResponseMessage resp = await hc.SendAsync(req);
            string content = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Response '" + resp.StatusCode.ToString() + " from API. Msg: " + content);
            }

            //Extract text
            JObject jo = JObject.Parse(content);
            string text = ResponseToOCR(jo);
            return text;
        }

        //Convert the entire response from the API into plain text
        public static string ResponseToOCR(JObject response)
        {
            JToken? linesTOKEN = response.SelectToken("readResult.blocks[0].lines");
            if (linesTOKEN == null)
            {
                throw new Exception("Unable to find lines property in API response");
            }
            JArray lines = (JArray)linesTOKEN;
            string ToReturn = "";
            foreach (JObject line in lines)
            {
                JProperty? prop_text = line.Property("text");
                if (prop_text != null)
                {
                    ToReturn = ToReturn + prop_text.Value.ToString() + Environment.NewLine;
                }
            }
            if (ToReturn.Length > 0)
            {
                ToReturn = ToReturn.Substring(0, ToReturn.Length - 1); //cut off last new line
            }
            return ToReturn;
        }

        public static void FormatAll()
        {
            string content = System.IO.File.ReadAllText(@"C:\Users\timh\Downloads\doc-ocr\OUTPUT.json");
            ImageReadTask[]? irts = JsonConvert.DeserializeObject<ImageReadTask[]>(content);
            if (irts != null)
            {
                string all = "";
                foreach (ImageReadTask irt in irts)
                {
                    all = all + irt.Read + Environment.NewLine + Environment.NewLine;
                }
                System.IO.File.WriteAllText(@"C:\Users\timh\Downloads\doc-ocr\OUTPUT.txt", all);
            }
        }
    
    }
}