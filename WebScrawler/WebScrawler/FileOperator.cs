using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrawler
{
    public class FileOperator
    {
        public static void WriteFile(string fileFolder, string fileName, string content)
        {
            if (!Directory.Exists(fileFolder))
            {
                Directory.CreateDirectory(fileFolder);
            }

            String filePath = fileFolder + "\\" + fileName;
            StreamWriter sw = new StreamWriter(filePath, true);
            sw.Write(content);
            sw.WriteLine();
            sw.Close();
        }

        public static List<string> ReadUrlList(string filePath)
        {
            List<string> urlList = new List<string>();
            if (!File.Exists(filePath))
            {
                return urlList;
            }

            StreamReader sr = new StreamReader(filePath);            
            while (!sr.EndOfStream)
            {
                string url = sr.ReadLine();
                if (!string.IsNullOrEmpty(url))
                {
                    urlList.Add(url.Trim());
                }
            }

            return urlList;
        }
    }
}
