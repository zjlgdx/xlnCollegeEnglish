using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ColleageEnglishVocaburary.Model;

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


        /// <summary>
        /// The main entry point for the program.
        /// </summary>
        /// <param name="args">Program arguments.</param>
        static void Main(string[] args)
        {
            const string rootFolder = "c:\\temp";
            const string lessionfolder = "horizonread{0}";
            const string url = "http://wyxy4.yzu.edu.cn/" + lessionfolder + "/UNIT{1}-{2}-lw.htm";

            var wordsUrls = new List<string>();

            string textBook = "A";
         
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
                    var lessionNum = unit.ToString().PadLeft(2, '0');
                    var wordUrl = string.Format(url, lession, lessionNum, textBook);

                    if (textBook == "A")
                    {
                        textBook = "B";
                    }
                    else
                    {
                        textBook = "A";
                    }

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

                    Console.Write("Saving file:");
                    Console.WriteLine(localFile);
                }
                
            }


            Console.Write("Downloading completed!!!");
            Console.ReadKey();

        }

        private const string COLLEGE_ENGLISH_ONLINE_BOOK_URL =
            "http://nhjy.hzau.edu.cn/english/perspective/integrated/{0}/";

        private const string COLLEAGE_ENGLISH_VOCABURARY_BOOK_ID = "Colleage_English_Vocaburary_book_{0}";
        private const string COURSE_ID = "{0}/0{1}p2newword1.htm";

        /// <summary>
        /// * �ο���http://blog.csdn.net/lxcnn/article/details/4402808
        ///     * .NET�������֮����ƽ����
        ///     * 3.1.3  ������
        ///       ��������Ҫ�漰�������������顰(?<Open>\()���͡�(?<-Open>\))��������ƽ�����Ӧ���У�����ֻ�������Ƿ�ƥ���ˣ�������ƥ�䵽�������ǲ����ĵġ�
        ///     * ��������һ�����󣬿��������·�ʽʵ��
        ///        \( (?<Open>)
        ///        \)(?<-Open>)
        ///       ��(?<Open>)���͡�(?<-Open>)�������ַ�ʽֻ��ʹ�������������飬�������һ��λ�ã��������ܹ�ƥ��ɹ��ģ���ƥ��������ǿյģ�������ڴ�ռ��ǹ̶��ģ�
        ///     * ������Ч�Ľ�ʡ��Դ�����ڵ��ַ�Ƕ�׽ṹ�в������ԣ��������ַ�����Ƕ�׽ṹ�оͱȽ������ˡ�
        ///     * ���ڲ�������ֱ�Ӹ��ڿ�ʼ��������֮��ģ�����ֻҪ��ʼ��������ƥ��ɹ���������������Ȼ�ͻ�ƥ��ɹ������ڹ�����û���κ�Ӱ��ġ�
        ///     * ��ע��������ƥ��link��ǩ��������ȫ����ʹ�����±��ʽ��@"<(a)(?:(?!\bhref\b)[^<>])*href=([""']?){0}\2[^>]*>(?>(?:(?!</?\1).)*)</\1>"��
        ///     * �����еı��ʽֻ�Ǹ�ͨ�ã�����������aê���ǩ��
        /// </summary>
        private const string COURSE_HYPER_LINK_PATTERN = @"<([a-z]+)(?:(?!\bhref\b)[^<>])*href=([""']?){0}\2[^>]*>(?><\1[^>]*>(?<o>)|</\1>(?<-o>)|(?:(?!</?\1).)*)*(?(o)(?!))</\1>";

        /// <summary>
        /// ����ÿ��ĵ�ԪĿ¼
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        private static void DownloadCourseUnit(string bookId)
        {
            string url = string.Format(COLLEGE_ENGLISH_ONLINE_BOOK_URL, bookId);

            DirectoryInfo directoryInfo = new DirectoryInfo("E:\\collegeEnglish\\integrated" + bookId);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            if (!Directory.Exists("E:\\collegeEnglish\\integrated" + bookId + "\\unitlist"))
            {
                Directory.CreateDirectory("E:\\collegeEnglish\\integrated" + bookId + "\\unitlist");
            }

            var client = new WebClient();
            string response = client.DownloadString(new Uri(url));
            //var book = new Book();
            //book.BookName = GetBookName(bookId);
            //var courses = new List<Course>();
            string[] hrefList = { "u1-p1-d.htm", "u2-p1-d.htm", "u3-p1-d.htm", "u4-p1-d.htm", "u5-p1-d.htm", "u6-p1-d.htm", "u7-p1-d.htm", "u8-p1-d.htm" };
            var courseMapping = new Dictionary<string, string> { 
                { "u1-p1-d.htm", "Unit One" }, 
                { "u2-p1-d.htm", "Unit Two" }, 
                { "u3-p1-d.htm", "Unit Three" }, 
                { "u4-p1-d.htm", "Unit Four" }, 
                { "u5-p1-d.htm", "Unit Five" }, 
                { "u6-p1-d.htm", "Unit Six" } ,
                { "u7-p1-d.htm", "Unit Seven" },
                { "u8-p1-d.htm", "Unit Eight" } 
            };
            var index = 0;

            List<UnitList> unitLists = new List<UnitList>();
            foreach (string href in hrefList)
            {
                Match match = Regex.Match(response, string.Format(COURSE_HYPER_LINK_PATTERN, Regex.Escape(href)),
                               RegexOptions.Singleline | RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    var regexHref = Regex.Match(match.Value, "(?<=src=\")images/home_\\d+.(gif|jpg)");
                    if (regexHref.Success)
                    {
                        //ViewModel.DownloadingStatus = "Downloading unit : " + courseMapping[href];
                        Console.WriteLine("Downloading unit : " + courseMapping[href]);
                        var unitdirectory = "E:\\collegeEnglish\\integrated" + bookId + "\\unitlist";
                        if (!Directory.Exists(unitdirectory))
                        {
                            Directory.CreateDirectory(unitdirectory);
                        }
                        var image = regexHref.Value;
                        var imageUrl = url + image;
                        var imagePath = (image).Replace("/", "_");
                        var picpath = unitdirectory;
                        new AuthDownloadURL().Download(new Uri(imageUrl), picpath + "\\" + imagePath);
                        //courses.Add(new Course { CourseId = string.Format(COURSE_ID, bookId, ++index), CourseName = imagePath, CourseImage = imagePath });

                        //Stream stream = await client.OpenReadTaskAsync(imageUrl);
                        //await FileStorageOperations.SaveToLocalFolderAsync(imagePath, stream);

                        unitLists.Add(new UnitList() { UnitID = href.Replace(".htm", ""), UnitName = courseMapping[href], UnitImage = (picpath + "\\" + imagePath).Replace("E:\\collegeEnglish\\", "") });

                        //progressBar1.Value += progressBar1.LargeChange;
                    }
                }
            }
            //DataContractSerializer serializer = new DataContractSerializer(typeof(TheDataType));
            var json = JsonConvert.SerializeObject(unitLists);
            File.AppendAllText("E:\\collegeEnglish\\integrated" + bookId + "\\unitlist\\UnitList.json", json);
            //book.Courses = courses;
            //await MyDataSerializer<Book>.SaveObjectsAsync(book, string.Format(COLLEAGE_ENGLISH_VOCABURARY_BOOK_ID, bookId));
            //ViewModel.DownloadingStatus = Constants.DOWNLOAD_COMPLETE;
        }

        #region regex expression

        // �ο���.NET�������֮����ƽ����(http://blog.csdn.net/lxcnn/article/details/4402808)
        // ��ȡ���ʶ�����ʽ
        private const string PARAGRAPH_PATTERN = @"(?isx)   #ƥ��ģʽ�����Դ�Сд����.��ƥ�������ַ�
                                                    <p[^>]*>  #��ʼ��ǡ�<p...>��
                                                    (?>       #���鹹�죬�����޶����ʡ�*�����η�Χ
                                                       <p[^>]*>(?<Open>)   #���������飬������ʼ��ǣ���ջ��Open������1
                                                    |         #��֧�ṹ
                                                       </p>(?<-Open>)      #����ƽ���飬����������ǣ���ջ��Open������1
                                                    |                      #��֧�ṹ
                                                       <a\s+name=""nw\d+""\s*></a>(?:(?!</?p\b).)+      #�Ҳ������<a name=""nw2""></a>��֮���Ҳ಻Ϊ��ʼ�������ǵ������ַ�
                                                    )+                     #�����Ӵ�����0�λ�������
                                                    (?(Open)(?!))          #�ж��Ƿ���'OPEN'������˵������ԣ�ʲô����ƥ��
                                                    </p>                   #������ǡ�</p>��
                                                    ";
        // ��ȡmp3ý���ļ�������ʽ
        private const string MP3_MEDIA_PATTERN = @"(?isx)
                                                   \(              #��ͨ�ַ���(��
                                                  (?>              #���鹹�죬�����޶����ʡ�*�����η�Χ
                                                    [^()]+         #�����������������ַ�
                                                  |                #��֧�ṹ
                                                    \(  (?<Open>)  #���������飬����������Open������1
                                                  |                #��֧�ṹ
                                                    \)  (?<-Open>) #����ƽ���飬����������Open������1
                                                  )+               #�����Ӵ�����0�λ�������
                                                  (?(Open)(?!))    #�ж��Ƿ���'OPEN'������˵������ԣ�ʲô����ƥ��
                                                  \)               #��ͨ������
                                                 ";
        private const string WORD_PATTERN = @"(?isx)
                                                <(font)\s+color=""#3366cc""\s*>    #��ʼ��ǡ�<tag...>��
                                                (?>                         #���鹹�죬�����޶����ʡ�*�����η�Χ
                                                    <\1[^>]*>  (?<Open>)    #���������飬������ʼ��ǣ���ջ��Open������1
                                                |                           #��֧�ṹ
                                                    </\1>  (?<-Open>)       #����ƽ���飬����������ǣ���ջ��Open������1
                                                |                           #��֧�ṹ
                                                    (?:(?!</?\1\b).)+       #�Ҳ಻Ϊ��ʼ�������ǵ������ַ�
                                                )+                          #�����Ӵ�����0�λ�������
                                                (?(Open)(?!))               #�ж��Ƿ���'OPEN'������˵������ԣ�ʲô����ƥ��
                                                </\1>                       #������ǡ�</tag>��
                                            ";

        private const string WORD_PHRASE_PATTERN = @"(?is)<(br)\b[^>]*>((?!<font\s+color=""#336600""\s*>).)*";
        private const string SENTENCE_PATTERN = @"(?<=<font\s+color=""#336600""\s*>).*";


        #endregion

        private static string GetCoursePathName(string courseId)
        {
            var bookId = courseId.Substring(0, 1);
            string bookName;
            var unitId = courseId.Substring(2, 2);
            string unitName;
            //switch (bookId)
            //{
            //    case "1":
            //        bookName = "��һ��";
            //        break;
            //    case "2":
            //        bookName = "�ڶ���";
            //        break;
            //    case "3":
            //        bookName = "������";
            //        break;
            //    case "4":
            //        bookName = "���Ĳ�";
            //        break;
            //    default:
            //        bookName = "Unknown";
            //        break;
            //}

            //switch (unitId)
            //{
            //    case "01":
            //        unitName = "Unit";
            //        break;
            //    case "02":
            //        unitName = "�ڶ���Ԫ";
            //        break;
            //    case "03":
            //        unitName = "������Ԫ";
            //        break;
            //    case "04":
            //        unitName = "���ĵ�Ԫ";
            //        break;
            //    case "05":
            //        unitName = "���嵥Ԫ";
            //        break;
            //    case "06":
            //        unitName = "������Ԫ";
            //        break;
            //    case "07":
            //        unitName = "���ߵ�Ԫ";
            //        break;
            //    case "08":
            //        unitName = "�ڰ˵�Ԫ";
            //        break;
            //    default:
            //        unitName = "Unknown";
            //        break;
            //}

            return string.Format("{0}\\Unit{1}", bookId, unitId);
        }

        private static string GetCourseName(string courseId)
        {
            var bookId = courseId.Substring(0, 1);
            string bookName;
            var unitId = courseId.Substring(2, 2);
            string unitName;
            switch (bookId)
            {
                case "1":
                    bookName = "��һ��";
                    break;
                case "2":
                    bookName = "�ڶ���";
                    break;
                case "3":
                    bookName = "������";
                    break;
                case "4":
                    bookName = "���Ĳ�";
                    break;
                default:
                    bookName = "Unknown";
                    break;
            }

            switch (unitId)
            {
                case "01":
                    unitName = "��һ��Ԫ";
                    break;
                case "02":
                    unitName = "�ڶ���Ԫ";
                    break;
                case "03":
                    unitName = "������Ԫ";
                    break;
                case "04":
                    unitName = "���ĵ�Ԫ";
                    break;
                case "05":
                    unitName = "���嵥Ԫ";
                    break;
                case "06":
                    unitName = "������Ԫ";
                    break;
                case "07":
                    unitName = "���ߵ�Ԫ";
                    break;
                case "08":
                    unitName = "�ڰ˵�Ԫ";
                    break;
                default:
                    unitName = "Unknown";
                    break;
            }

            return string.Format("{0} {1}", bookName, unitName);
        }

        private const string COLLEGE_ENGLISH_ONLINE_BOOK_BASE_URL =
            "http://nhjy.hzau.edu.cn/english/perspective/integrated";
        private static void DownloadWord(string courseId)
        {
            var bookId = courseId.Substring(0, 2);
            string bookUrl = COLLEGE_ENGLISH_ONLINE_BOOK_BASE_URL + "/" + bookId;
            string courseUrl = COLLEGE_ENGLISH_ONLINE_BOOK_BASE_URL + "/" + courseId;

            var client = new WebClient();
            string response = client.DownloadString(new Uri(courseUrl));

            var courseName = GetCourseName(courseId);
            var course = new Course { CourseId = courseId, CourseName = courseName };
            var newWords = new List<NewWord>();

            var regexParagraph = new Regex(PARAGRAPH_PATTERN);

            var matchParagraphes = regexParagraph.Matches(response);

            var wordId = 0;
            double totalCount = matchParagraphes.Count;
            var percentage = 1d / totalCount * 100d;
            //  progressBar1.LargeChange = percentage;
            Console.WriteLine(percentage);

            foreach (Match matchParagraph in matchParagraphes)
            {
                var paragraph = matchParagraph.Value;
                var objWord = new NewWord();
                objWord.WordId = (wordId++).ToString();

                FetchMedia(paragraph, course, objWord, client, bookUrl);

                // word and phase
                FetchWord(paragraph, objWord);

                FetchWordPhrase(paragraph, objWord);

                FetchSentence(paragraph, objWord);

                newWords.Add(objWord);

                //progressBar1.Value += progressBar1.LargeChange;
            }

            //course.NewWords = newWords;

            //await MyDataSerializer<Course>.SaveObjectsAsync(course, ViewModel.CourseId);
            /*
           // ViewModel.DownloadingStatus = Constants.DOWNLOAD_COMPLETE + "Text A";
            Console.WriteLine("Constants.DOWNLOAD_COMPLETE Text A");
            //--------------

            courseUrl = COLLEGE_ENGLISH_ONLINE_BOOK_BASE_URL + "/" + courseId.Replace("p2newword1", "p3newword1");

            response = client.DownloadString(new Uri(courseUrl));

            //var courseName = GetCourseName(courseId);
            //var course = new Course { CourseId = courseId, CourseName = courseName };
            //var newWords = new List<NewWord>();

            regexParagraph = new Regex(PARAGRAPH_PATTERN);

            matchParagraphes = regexParagraph.Matches(response);

            wordId = 0;
            totalCount = matchParagraphes.Count;
            percentage = 1d / totalCount * 100d;
           // progressBar1.LargeChange = percentage;

            foreach (Match matchParagraph in matchParagraphes)
            {
                var paragraph = matchParagraph.Value;
                var objWord = new NewWord();
                objWord.WordId = (wordId++).ToString();

                FetchMedia(paragraph, course, objWord, client, bookUrl);

                // word and phase
                FetchWord(paragraph, objWord);

                FetchWordPhrase(paragraph, objWord);

                FetchSentence(paragraph, objWord);

                newWords.Add(objWord);

                //progressBar1.Value += progressBar1.LargeChange;
            }
            */
            course.NewWords = newWords;

            //await MyDataSerializer<Course>.SaveObjectsAsync(course, ViewModel.CourseId);

            //ViewModel.DownloadingStatus = Constants.DOWNLOAD_COMPLETE + "Text B"; ;

            // todo : saving course
            var json = JsonConvert.SerializeObject(course);
            var basePath = "E:\\collegeEnglish\\integrated" + GetCoursePathName(course.CourseId);
            var jsonpaht = basePath;
            if (!Directory.Exists(jsonpaht))
            {
                Directory.CreateDirectory(jsonpaht);
            }
            File.AppendAllText(jsonpaht + GetUnitText(course.CourseId), json);

            //-----------------------




            //NavigateToLearningWord();
        }

        private static string GetUnitText(string CourseId)
        {
            if (CourseId.Contains("p2newword1"))
            {
                return "\\TextA.json";
            }

            return "\\TextB.json";
        }

        private static void FetchMedia(string paragraph,
                                             Course course,
                                             NewWord objWord,
                                             WebClient client,
                                             string bookUrl)
        {
            var basePath = "E:\\collegeEnglish\\integrated" + GetCoursePathName(course.CourseId);

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            var mp3path = basePath + "\\sound";

            if (!Directory.Exists(mp3path))
            {
                Directory.CreateDirectory(mp3path);
            }

            var regexMedia = new Regex(MP3_MEDIA_PATTERN);
            var matchMedias = regexMedia.Matches(paragraph);
            var index = 0;
            foreach (Match matchMedia in matchMedias)
            {
                // ��ȡmp3�ļ������浽�����洢��
                var mp3 = matchMedia.Value.Replace("('", "").Replace("')", "");
                if (!mp3.EndsWith(".mp3"))
                {
                    continue;
                }

                var mp3fullname = basePath + "\\" + mp3.Replace("/", "\\");
                var mp3Path = "integrated" + GetCoursePathName(course.CourseId) + "\\" + mp3.Replace("/", "\\");// course.CourseName + objWord.WordId + mp3;
                //mp3Path = mp3Path.Replace("/", "");
                if (index == 0)
                {
                    objWord.WordVoice = mp3Path;
                }
                else
                {
                    objWord.SentenceVoice = mp3Path;
                }

                index++;
                try
                {
                    if (Directory.Exists(mp3fullname))
                    {
                        continue;
                    }
                    //Stream stream = client.OpenRead(bookUrl + mp3);
                    //await FileStorageOperations.SaveToLocalFolderAsync(mp3Path, stream);
                    new AuthDownloadURL().Download(new Uri(bookUrl + mp3), mp3fullname);
                }
                catch (Exception)
                {
                }
            }
        }

        private static void FetchSentence(string paragraph, NewWord objWord)
        {
            var regexSentence = new Regex(SENTENCE_PATTERN, RegexOptions.Singleline);
            var sentenceMatch = regexSentence.Match(paragraph);
            if (sentenceMatch.Success)
            {
                var regexMark = new Regex("<[^>]+>");
                var sentence = regexMark.Replace(sentenceMatch.Value, "");
                sentence = sentence.Replace("&nbsp;&nbsp;e.g.", "e.g.");
                sentence = Regex.Replace(sentence, "\\s+", " ");
                sentence = Regex.Replace(sentence, "&nbsp;$", "");
                objWord.Sentence = sentence;
            }
        }

        private static void FetchWordPhrase(string paragraph, NewWord objWord)
        {
            var regexWordPhrase = new Regex(WORD_PHRASE_PATTERN);
            var matchWordPhrase = regexWordPhrase.Match(paragraph);
            if (matchWordPhrase.Success)
            {
                var wordPhrase = Regex.Replace(matchWordPhrase.Value, "\\s+|<br>", " ").Trim();
                wordPhrase = Regex.Replace(wordPhrase, "<[^>]+>", "");
                wordPhrase = Regex.Replace(wordPhrase, "&nbsp;$", "");
                objWord.WordPhrase = wordPhrase;
            }
        }

        private static void FetchWord(string paragraph, NewWord objWord)
        {
            var regexWord = new Regex(WORD_PATTERN);

            var matchWord = regexWord.Match(paragraph);

            if (matchWord.Success)
            {
                var word = Regex.Replace(matchWord.Value, "\\s+", " ");
                word = Regex.Replace(word, @"^<font\s+color=""#3366cc""\s*>", "");
                word = Regex.Replace(word, @"</font>$", "");
                word = Regex.Replace(word, @"<[^>]+>", "");
                objWord.Word = word;

                Console.WriteLine("Downloading word : " + word);
            }
        }

    }

    public class UnitList
    {
        public string UnitID { get; set; }
        public string UnitName { get; set; }
        public string UnitImage { get; set; }
    }
}
