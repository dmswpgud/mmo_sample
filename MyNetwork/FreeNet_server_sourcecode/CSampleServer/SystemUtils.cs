using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;

namespace CSampleServer
{
    public class SystemUtils
    {
        private static string path = "/Users/jaehyung.eun/Desktop/json.txt";

        public static void Save(int id, JToken json)
        {
            var readJson = Leader();
            
            JToken token = null;

            if (readJson != null)
            {
                token = readJson.SelectToken(id.ToString());    
            }

            if (token != null)
            {
                token.Replace(json);
            }

            if (readJson == null)
            {
                readJson = new JObject();
                readJson.Add(id.ToString(), json);
            }

            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                sw.Write(readJson);
            }
        }

        public static JObject Leader()
        {
            string strFile = path;

            FileInfo fileInfo = new FileInfo(strFile);

            if (fileInfo.Exists)
            {
                string txt;

                using (StreamReader sr = new StreamReader(path))
                {
                    txt = sr.ReadToEnd();
                }

                return JObject.Parse(txt);
            }

            return null;
        }
    }
}