using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Recipe5_2
{
    /// <summary>
    /// Recipe #5.2: Downloading a URL(text or binary)
    /// Copyright 2007 by Jeff Heaton(jeff@jeffheaton.com)
    ///
    /// HTTP Programming Recipes for C# Bots
    /// ISBN: 0-9773206-7-7
    /// http://www.heatonresearch.com/articles/series/20/
    /// 
    /// This recipe shows how to download a text or binary file,
    /// using HTTP authentication.
    /// 
    /// This software is copyrighted. You may use it in programs
    /// of your own, without restriction, but you may not
    /// publish the source code without the author's permission.
    /// For more information on distributing this code, please
    /// visit:
    ///    http://www.heatonresearch.com/hr_legal.php
    /// </summary>
    class AuthDownloadURL
    {
        /// <summary>
        /// Download the specified text page.
        /// </summary>
        /// <param name="response">The HttpWebResponse to download from.</param>
        /// <param name="filename">The local file to save to.</param>
        public void DownloadBinaryFile(HttpWebResponse response, String filename)
        {

            byte[] buffer = new byte[4096];
            FileStream os = new FileStream(filename, FileMode.Create);
            Stream stream = response.GetResponseStream();

            int count = 0;
            do
            {
                count = stream.Read(buffer, 0, buffer.Length);
                if (count > 0)
                    os.Write(buffer, 0, count);
            } while (count > 0);

            response.Close();
            stream.Close();
            os.Close();
        }

        /// <summary>
        /// Download the specified text page.
        /// </summary>
        /// <param name="response">The HttpWebResponse to download from.</param>
        /// <param name="filename">The local file to save to.</param>
        public void DownloadTextFile(HttpWebResponse response, String filename)
        {
            byte[] buffer = new byte[4096];
            FileStream os = new FileStream(filename, FileMode.Create);
            StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.GetEncoding("GB2312"));
            StreamWriter writer = new StreamWriter(os, System.Text.Encoding.UTF8);

            String line;
            do
            {
                line = reader.ReadLine();
                if (line != null)
                    writer.WriteLine(line);

            } while (line != null);

            reader.Close();
            writer.Close();
            os.Close();
        }


        /// <summary>
        /// Download either a text or binary file from a URL.
        /// The URL's headers will be scanned to determine the
        /// type of tile.  The user id and password are authenticated.
        /// </summary>
        /// <param name="remoteURL">The URL to download from.</param>
        /// <param name="localFile">The local file to save to.</param>
        /// <param name="uid">The user id to use.</param>
        /// <param name="pwd">The password to use.</param>
        public void Download(Uri remoteURL, String localFile)
        {
            // NetworkCredential networkCredential = new NetworkCredential(uid, pwd);
            WebRequest http = HttpWebRequest.Create(remoteURL);

            // http://blogs.msdn.com/b/jpsanders/archive/2009/03/24/httpwebrequest-webexcepton-the-remote-server-returned-an-error-407-proxy-authentication-required.aspx
            // Begin code change by jeff
            //  Obtain the 'Proxy' of the  Default browser. 
            IWebProxy theProxy = http.Proxy;
            // Print the Proxy Url to the console.
            if (theProxy != null)
            {
                // Use the default credentials of the logged on user.
                theProxy.Credentials = CredentialCache.DefaultCredentials;
            }
            // End code change by jeff

            HttpWebResponse response = (HttpWebResponse)http.GetResponse();
            String chars = response.CharacterSet;
            String type = response.Headers["Content-Type"].ToLower().Trim();
            if (type.StartsWith("text"))
                DownloadTextFile(response, localFile);
            else
                DownloadBinaryFile(response, localFile);

        }

        static void DownloadMp3()
        {
            //http://wyxy4.yzu.edu.cn/horizonread1/sounds/nw-unit01-a-01.mp3
            const string rootFolder = "c:\\temp";
            const string lessionName = "horizonread{0}";
            const string baseurl = "http://wyxy4.yzu.edu.cn/" + lessionName;

            if (!Directory.Exists(rootFolder))
            {
                Console.WriteLine("no such folder:" + rootFolder);
                return;
            }

            Recipe5_2.AuthDownloadURL d = new AuthDownloadURL();

            for (int lession = 1; lession <= 4; lession++)
            {
                // can not access lession 2
                if (lession == 2)
                {
                    continue;
                }

                // C:\temp\horizonread1
                var lessionFolder = Path.Combine(rootFolder, string.Format(lessionName, lession));

                if (!Directory.Exists(lessionFolder))
                {
                    Console.WriteLine("no such folder:" + lessionFolder);
                    return;
                }

                for (int unit = 1; unit <= 10; unit++)
                {
                    string textBook = "A";

                    int count = 0;
                    while (count < 2)
                    {
                        var unitNum = unit.ToString().PadLeft(2, '0');

                        //C:\temp\horizonread1\UNIT02\UNIT02_A.json
                        var unitName = "UNIT" + unitNum;
                        var unitFolder = Path.Combine(lessionFolder, unitName);
                        var unitfilename = Path.Combine(unitFolder, unitName + "_" + textBook + ".json");
                        if (!File.Exists(unitfilename))
                        {
                            Console.WriteLine("no such file:" + unitfilename);
                            return;
                        }

                        var json = File.ReadAllText(unitfilename);
                        var objunit = JsonConvert.DeserializeObject<Unit>(json);
                        var mp3s = objunit.Vocabularies.Select(m => m.Voice);

                        foreach (var mp3 in mp3s)
                        {
                            var mp3Url = string.Format(baseurl, lession) + "/" + mp3;
                            Console.Write("Downloading voice:");
                            Console.WriteLine(mp3Url);

                            var soundFolder = Path.Combine(lessionFolder, "sounds");

                            if (!Directory.Exists(soundFolder))
                            {
                                Console.Write("create folder:");
                                Console.WriteLine(soundFolder);
                                Directory.CreateDirectory(soundFolder);
                            }
                            //sounds/nw-unit01-a-01.mp3

                            string localFile = Path.Combine(lessionFolder, mp3).Replace("/", "\\");

                            d.Download(new Uri(mp3Url), localFile);
                            Console.Write("Saving file:");
                            Console.WriteLine(localFile);

                        }

                        if (textBook == "A")
                        {
                            textBook = "B";
                        }
                        else
                        {
                            textBook = "A";
                        }

                        count++;
                    }
                }

            }


            Console.Write("Downloading completed!!!");
            Console.ReadKey();

        }

        /// <summary>
        /// The main entry point for the program.
        /// </summary>
        /// <param name="args">Program arguments.</param>
        static void Main(string[] args)
        {
            //DownloadUnitPage();
            DownloadMp3();

            Console.Write("Downloading completed!!!");
            Console.ReadKey();

        }

        private static void DownloadUnitPage()
        {
            // 参考：.NET正则基础之——平衡组(http://blog.csdn.net/lxcnn/article/details/4402808)
            // 3.2.2  根据id提取div嵌套标签
            const string SPAN_PATTERN = @"(?isx)
                      <span(?:(?!(?:id=|</?span\b)).)*id=(['""]?){0}\1[^>]*>        #开始标记“<span...>”
                          (?>                         #分组构造，用来限定量词“*”修饰范围
                              <span[^>]*>  (?<Open>)   #命名捕获组，遇到开始标记，入栈，Open计数加1
                          |                           #分支结构
                              </span>  (?<-Open>)      #狭义平衡组，遇到结束标记，出栈，Open计数减1
                          |                           #分支结构
                              (?:(?!</?span\b).)*      #右侧不为开始或结束标记的任意字符
                          )*                          #以上子串出现0次或任意多次
                          (?(Open)(?!))               #判断是否还有'OPEN'，有则说明不配对，什么都不匹配
                      </span>                          #结束标记“</span>”
                     ";
            const string rootFolder = "c:\\temp";
            const string lessionfolder = "horizonread{0}";
            const string url = "http://wyxy4.yzu.edu.cn/" + lessionfolder + "/UNIT{1}-{2}-lw.htm";

            var wordsUrls = new List<string>();



            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
            }

            Recipe5_2.AuthDownloadURL d = new AuthDownloadURL();

            for (int lession = 1; lession <= 4; lession++)
            {
                // can not access lession 2
                if (lession == 2)
                {
                    continue;
                }



                for (int unit = 1; unit <= 10; unit++)
                {
                    string textBook = "A";


                    int count = 0;
                    while (count < 2)
                    {
                        var lessionNum = unit.ToString().PadLeft(2, '0');
                        var wordUrl = string.Format(url, lession, lessionNum, textBook);

                        var lessionfolderFormat = string.Format(lessionfolder, lession);

                        var subfolder = Path.Combine(rootFolder, lessionfolderFormat);

                        if (!Directory.Exists(subfolder))
                        {
                            Console.Write("create folder:");
                            Console.WriteLine(subfolder);
                            Directory.CreateDirectory(subfolder);
                        }
                        var filename = wordUrl.Substring(wordUrl.LastIndexOf("/") + 1);
                        string localFile = Path.Combine(subfolder, filename);

                        Console.Write("Downloading page:");
                        Console.WriteLine(wordUrl);

                        d.Download(new Uri(wordUrl), localFile);

                        var client = new WebClient();
                        client.Encoding = System.Text.Encoding.GetEncoding("GB2312");
                        string response = client.DownloadString(new Uri(wordUrl));
                        var nwareamatch = Regex.Match(response, string.Format(SPAN_PATTERN, "nw-area"));
                        if (nwareamatch.Success)
                        {
                            File.WriteAllText(localFile, nwareamatch.Value, Encoding.UTF8);
                        }

                        var lpareamatch = Regex.Match(response, string.Format(SPAN_PATTERN, "lp-area"));
                        if (lpareamatch.Success)
                        {
                            File.AppendAllText(localFile, lpareamatch.Value, Encoding.UTF8);
                        }

                        Console.Write("Saving file:");
                        Console.WriteLine(localFile);

                        if (textBook == "A")
                        {
                            textBook = "B";
                        }
                        else
                        {
                            textBook = "A";
                        }

                        count++;
                    }
                }

            }
        }

       

      

      
       

    

       
   

        
     

       

     

     

     

        
    }

    
}
