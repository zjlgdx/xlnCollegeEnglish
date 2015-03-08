using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace WebApplication1
{
    //http://www.overpie.com/aspnet/articles/csharp-post-json-to-generic-handler

    /// <summary>
    /// Summary description for UnitHandler
    /// </summary>
    public class UnitHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            //deserialize the object
            var objUnit = Deserialize<Unit>(context);
            string json = JsonConvert.SerializeObject(objUnit);
            //__horizonread1_UNIT03_A_lw_htm
            //__horizonread1_01p2newword1_htm
            var rootFolder = "c:\\temp";

            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
            }
            //
            var list = objUnit.UnitTitle.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            var lession = list[0];
            var lessionFolder = Path.Combine(rootFolder, lession);
            if (!Directory.Exists(lessionFolder))
            {
                Directory.CreateDirectory(lessionFolder);
            }

            var unit = list[1].Substring(0,2);
            var unitFolder = Path.Combine(lessionFolder, unit);
            if (!Directory.Exists(unitFolder))
            {
                Directory.CreateDirectory(unitFolder);
            }

            var unitBook = unit + "_" + getTextBook(list[1]) + ".json";
            var filename = Path.Combine(unitFolder, unitBook);
            File.WriteAllText(filename, json);

            context.Response.Write("success.");
        }

        private string getTextBook(string input) {
            //01p2newword1
            var textbook = input.Substring(2, 2);
            switch (textbook)
            {
                case "p2":
                    return "A";
                case "p3":
                    return "B";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// This function will take httpcontext object and will read the input stream
        /// It will use the built in JavascriptSerializer framework to deserialize object based given T object type value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public T Deserialize<T>(HttpContext context)
        {
            //read the json string
            string jsonData = new StreamReader(context.Request.InputStream).ReadToEnd();

            //cast to specified objectType
            var obj = (T)new JavaScriptSerializer().Deserialize<T>(jsonData);

            //return the object
            return obj;
        }


        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}