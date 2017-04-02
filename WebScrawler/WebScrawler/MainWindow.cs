using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace WebScrawler
{
    public partial class MainWindow : Form
    {
        private List<string> urlList;
        private int currentUrlIndex;
        private string outputFolder;

        public MainWindow(string inputFilePath, string outputFolder)
        {
            InitializeComponent();
            this.outputFolder = outputFolder;
            urlList = FileOperator.ReadUrlList(inputFilePath);
            currentUrlIndex = -1;
            initWebBrowser();
            webBrowser.ScriptErrorsSuppressed = true;

            RefreshUrl();
        }

        // Try to load next html page with URL or download specific files
        private void RefreshUrl()
        {
            Thread.Sleep(1000);
            if (currentUrlIndex + 1 < urlList.Count)
            {
                ++currentUrlIndex;
                string url = urlList[currentUrlIndex];
                if (!url.ToLower().StartsWith("http"))
                {
                    RefreshUrl();
                    return;
                }
                string ext = Path.GetExtension(url);
                if (!string.IsNullOrEmpty(ext) && (ext.IndexOf(".doc") >= 0 || ext.IndexOf(".docx") >= 0))
                {
                    url = url.TrimEnd('/');
                    int startIndex = url.LastIndexOf('/');
                    string fileName = url.Substring(startIndex);
                    if (fileName.IndexOf('?') >= 0)
                    {
                        fileName = fileName.Substring(0, fileName.IndexOf('?'));
                    }

                    fileName = getValidFileName(fileName);
                    WebClient client = new WebClient();
                    client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)");
                    client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(client_DownloadDataCompleted);
                    string filePath = outputFolder + "\\" + fileName;
                    client.DownloadDataAsync(new Uri(url), filePath);
                }
                else
                {
                    timer.Enabled = true;
                    timer.Stop(); timer.Start();
                    webBrowser.Url = new Uri(url);
                }
            }
            else
            {
                Close();
            }
        }
        
        // Html document load successfully
        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.Equals(webBrowser.Url) || !timer.Enabled)
            {
                saveContentAndRefresh();
            }
        }

        // Save webpage content to file
        private void saveContentAndRefresh()
        {
            webBrowser.Stop();
            if (webBrowser.Document != null && webBrowser.Document.Body != null)
            {
                Thread.Sleep(1);
                string fileName = getValidFileName(webBrowser.Document.Title);
                HtmlDocument document = webBrowser.Document;
                FileOperator.WriteFile(outputFolder, fileName + ".txt", document.Body.InnerText);
            }

            RefreshUrl();
        }

        // Try to generate valid file name
        private string getValidFileName(string title)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            string tempUrl = r.Replace(title, "");

            if (string.IsNullOrEmpty(tempUrl.Trim()))
            {
                tempUrl = Guid.NewGuid().ToString();
            }

            return tempUrl;
        }

        // Download file complated
        private void client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            File.WriteAllBytes(e.UserState.ToString(), e.Result);

            RefreshUrl();
        }

        // Register IE version
        private void initWebBrowser()
        {
            int BrowserVer, RegVal;

            // get the installed IE version
            using (WebBrowser Wb = new WebBrowser())
                BrowserVer = Wb.Version.Major;

            // set the appropriate IE version
            if (BrowserVer >= 11)
                RegVal = 11001;
            else if (BrowserVer == 10)
                RegVal = 10001;
            else if (BrowserVer == 9)
                RegVal = 9999;
            else if (BrowserVer == 8)
                RegVal = 8888;
            else
                RegVal = 7000;

            // set the actual key
            RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true);
            Key.SetValue(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe", RegVal, RegistryValueKind.DWord);
            Key.Close();
        }

        // Timeout for some page load failed or load unterminated
        private void timer_Tick_1(object sender, EventArgs e)
        {
            timer.Stop();
            timer.Enabled = false;
            saveContentAndRefresh();
        }
    }
}
