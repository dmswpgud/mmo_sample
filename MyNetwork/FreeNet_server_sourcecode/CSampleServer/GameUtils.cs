using System;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;

namespace CSampleServer
{
    public class GameUtils
    {
        public static Int32 DamageCalculator(CUnit attacker, CUnit deffender)
        {
            return deffender.HpMp.Hp -= 10;
        }
    }
}

namespace CSampleServer
{
    public class SystemUtils
    {
        public static void SaveUserInfo(string account, JObject json)
        {
            var path = Program.userInfoJsonPath;
            // 기존껄 읽어 옴.
            var readJson = LoadJson(path);
            
            // 기존꺼 지움. TODO: 지우지 말고 덮어씌우고 싶음.
            readJson?.Remove(account);
            readJson = readJson ?? new JObject();
            readJson.Add(account, json);
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                sw.Write(readJson);
            }
        }
        
        public static void SaveJson(string path, JObject json)
        {
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                sw.Write(json);
            }
        }

        public static JObject LoadJson(string path)
        {
            FileInfo fileInfo = new FileInfo(path);

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

        public static UserDataPackage GetUserInfo(int userId, string password)
        {
            var readJson = LoadJson(Program.userInfoJsonPath);
            
            var userInfo = readJson?.SelectToken(userId.ToString());
            
            if (userInfo == null)
                return null;

            var pw = userInfo["password"];
            
            if (password != pw.ToString())
                return null;

            return userInfo.ToObject<UserDataPackage>();
        }


        public static UserDataPackage CreateAccount(string account, string password, string name, int id)
        {
            var path = Program.userInfoJsonPath;

            var userPackage = new UserDataPackage();
            userPackage.account = account;
            userPackage.password = password;
            userPackage.name = name;
            userPackage.userId = id;
            userPackage.data = new PlayerData() {playerId = id, name = name, unitType = 0, moveSpeed = 2};
            userPackage.state = new PlayerStateData() {playerId = id, posX = 10, posY = 10, direction = 4};
            userPackage.hpMp = new HpMp() {Hp = 500, Mp = 10};
            
            var loadJObj = LoadJson(path);
            var jObj = JObject.FromObject(userPackage);

            JObject obj = loadJObj == null ? jObj : loadJObj;
            
            var userId = account.GetHashCode();
            obj.Add(userId.ToString(), jObj);
            
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                sw.Write(obj);
            }

            return userPackage;
        }
    }
}