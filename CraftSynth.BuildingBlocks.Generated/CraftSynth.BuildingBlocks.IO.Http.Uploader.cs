using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

using CraftSynth.BuildingBlocks.Logging;
using CSS_ExtenderClass = CraftSynth.BuildingBlocks.Common.ExtenderClass;

namespace CraftSynth.BuildingBlocks.IO.Http
{
    /// <summary>
    /// Uploads file to server.
    /// Source: https://docs.microsoft.com/en-us/dotnet/api/system.net.webclient.uploadfile?redirectedfrom=MSDN&view=netframework-4.7.2#System_Net_WebClient_UploadFile_System_String_System_String_
    /// and: https://stackoverflow.com/questions/982299/getting-the-upload-progress-during-file-upload-using-webclient-uploadfile
    /// 
    /// On server side you must have web.config with this content:
    /// 
    ///  <?xml version="1.0"?>
    ///  <configuration>
    ///  <!-- https://stackoverflow.com/questions/3853767/maximum-request-length-exceeded -->
    ///   <system.web>
    ///  		<customErrors mode = "Off" />
    ///         < httpRuntime maxRequestLength="1048576"  executionTimeout="3600" />
    ///    </system.web>
    ///  
    ///    <system.webServer>
    ///      <security>
    ///        <requestFiltering>
    ///           <requestLimits maxAllowedContentLength = "1073741824" />
    ///        </ requestFiltering >
    ///      </ security >
    ///    </ system.webServer >
    ///  </ configuration >
    ///  
    ///  
    /// and Upload.aspx with this content:
    ///    <%@ Import Namespace="System"%>
    ///    <%@ Import Namespace = "System.IO" %>
    ///    <%@ Import Namespace = "System.Net" %>
    ///    <%@ Import NameSpace = "System.Web" %>
    ///    
    ///    < Script language="C#" runat=server>
    ///    void Page_Load(object sender, EventArgs e)
    ///        {
    ///    
    ///            foreach (string f in Request.Files.AllKeys)
    ///            {
    ///                HttpPostedFile file = Request.Files[f];
    ///                file.SaveAs("c:\\" + file.FileName);
    ///            }
    ///        }  
    ///    </Script>
    ///    <html>
    ///    <body>
    ///    Upload complete.
    ///    </body>
    ///    </html>
    ///    
    /// and here is test app:
    ///  static void Main(string[] args)
    ///          {
    ///              Console.WriteLine("Hello World!");
    ///              String uriString = "http://wad.f4cio.com/Upload.aspx";
    ///  
    ///      string fileName = @"D:\Trash\aaa.7z";
    ///      Console.WriteLine("Uploading {0} to {1} ...", fileName, uriString);
    ///              
    ///              CustomTraceLog log = new CustomTraceLog("", true, false, CustomTraceLogAddLinePostProcessing);
    ///      CraftSynth.BuildingBlocks.IO.Http.Uploader uploader = new CraftSynth.BuildingBlocks.IO.Http.Uploader();
    ///      uploader.Upload(fileName, uriString, true, log);
    ///              Console.ReadKey();
    ///          }
    ///  
    ///  static void CustomTraceLogAddLinePostProcessing(CustomTraceLog sender, string line, bool inNewLine, int level, string lineVersionSuitableForLineEnding, string lineVersionSuitableForNewLine)
    ///  {
    ///      Console.WriteLine(line);
    ///  }
    /// </summary>
    public class Uploader
    {
        private CustomTraceLog log;
        private string filePath;
        private string uri;
        private string Response = null;
        private int ProgressPercentage;

        public string Upload(string filePath, string uri, bool asyncWithProgressInfo, CustomTraceLog log)
        {
            string r = null;

            this.filePath = filePath;
            this.uri = uri;
            this.log = log;

            if (!asyncWithProgressInfo)
            {
                using (log.LogScope($"Uploading '{CSS_ExtenderClass.ToNonNullString(this.uri, "null")}' to '{CSS_ExtenderClass.ToNonNullString(this.filePath, "null")}'..."))
                {
                    WebClient myWebClient = new WebClient();
                    byte[] responseArray = myWebClient.UploadFile(this.uri, this.filePath);
                    if (responseArray == null)
                    {
                        log.AddLine("Response Received:null");
                    }
                    else if (responseArray.Length == 0)
                    {
                        log.AddLine("Response Received:zero length");
                    }
                    else
                    {
                        r = System.Text.Encoding.ASCII.GetString(responseArray);
                        log.AddLine("Response Received:" + CSS_ExtenderClass.ToNonNullString(r, "null"));
                    }
                }
            }
            else
            { 
                log.AddLineAndIncreaseIdent($"Uploading (asynchroneusely) '{CSS_ExtenderClass.ToNonNullString(this.uri, "null")}' to '{CSS_ExtenderClass.ToNonNullString(this.filePath, "null")}'");
                WebClient webClient = new WebClient();
                webClient.UploadFileAsync(new Uri(this.uri), this.filePath);
                webClient.UploadProgressChanged += WebClientUploadProgressChanged;
                webClient.UploadFileCompleted += WebClientUploadCompleted;
                this.Response = null;
                this.ProgressPercentage = -1;
                while (this.Response==null)
                {
                    Thread.Sleep(1000);
                }
                r = this.Response;
            }

            return r;
        }

        void WebClientUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            if (this.ProgressPercentage != e.ProgressPercentage)
            {
                this.ProgressPercentage = e.ProgressPercentage;
                this.log.AddLine(this.ProgressPercentage + "...", false);
            }
        }

        void WebClientUploadCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            string r = null;

            if (e.Cancelled)
            {
                log.AddLineAndDecreaseIdent("The upload was canelled.");
            }
            else if (e.Error != null)
            {
                log.AddLineAndDecreaseIdent("The upload ended with error:");
                log.AddLine(CSS_ExtenderClass.ToNonNullString(e.Error.Message, "null"));
                log.AddLine(CSS_ExtenderClass.ToNonNullString(e.Error.StackTrace, "null"));
            }
            else
            {
                log.AddLineAndDecreaseIdent("The upload is finished");
                r = System.Text.Encoding.ASCII.GetString(e.Result);
                log.AddLine("Response Received:" + CSS_ExtenderClass.ToNonNullString(r, "null"));
            }

            this.Response = r;
        }
    }
}
