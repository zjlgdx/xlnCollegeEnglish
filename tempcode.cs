// 解决乱码
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

            String type = response.Headers["Content-Type"].ToLower().Trim();
            if (type.StartsWith("text"))
                DownloadTextFile(response, localFile);
            else
                DownloadBinaryFile(response, localFile);

        }
        
        
        static void Main(string[] args)
        {
            const string url = "http://wyxy4.yzu.edu.cn/horizonread{2}/UNIT{0}-{1}-lw.htm";

            var wordsUrls = new List<string>();

            string textBook = "A";
            for (int lession = 1; lession <= 4; lession++)
            {
                // can not access lession 2
                if (lession == 2)
                {
                    continue;
                }

                for (int unit = 1; unit <= 10; unit++)
                {
                   var lessionNum = unit.ToString().PadLeft(2, '0');
                   wordsUrls.Add(string.Format(url, lessionNum, textBook, lession));

                    if (textBook == "A")
                    {
                        textBook = "B";
                    }
                    else
                    {
                        textBook = "A";
                    }
                }
            }

            Recipe5_2.AuthDownloadURL d = new AuthDownloadURL();
            foreach (var wordurl in wordsUrls)
            {
                string localFile = "c:\\temp\\" + wordurl.Substring(wordurl.LastIndexOf("/"));
                if (!Directory.Exists("c:\\temp"))
                {
                    Directory.CreateDirectory("c:\\temp");
                }

                d.Download(new Uri(wordurl), localFile);
                Console.Write("Downloading page:");
                Console.WriteLine(wordurl);
                Console.Write("Downloading completed!!!");
                Console.ReadKey();
            }

        }
