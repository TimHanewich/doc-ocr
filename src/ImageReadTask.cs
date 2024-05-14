using System;

namespace DocOCR
{
    public class ImageReadTask
    {
        public int Page {get; set;}
        public string Path {get; set;}
        public string Read {get; set;}
        
        public ImageReadTask()
        {
            Page = 0;
            Path = "";
            Read = "";
        }
    }
}