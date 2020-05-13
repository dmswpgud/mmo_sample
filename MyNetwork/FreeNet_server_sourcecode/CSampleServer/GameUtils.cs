using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GameServer;
using Newtonsoft.Json.Linq;

namespace CSampleServer
{
    public class GridPoint 
    {
        public int X, Y;
        public GridPoint(int x, int y) { X = x; Y = y; }
    }
    
    public class GameUtils
    {
        public static List<CUnit> GetNearbyUnit(int id, int x, int y, int range, List<CUnit> listUSer)
        {
            List<CUnit> listNearUsers = new List<CUnit>();
            
            for (int i = 0; i < listUSer.Count; ++i)
            {
                if (id == listUSer[i].playerData.playerId)
                    continue;
                
                if ((x + range >= listUSer[i].stateData.posX &&
                     x - range <= listUSer[i].stateData.posX) &&
                    (y + range >= listUSer[i].stateData.posY &&
                     y - range <= listUSer[i].stateData.posY))
                {
                    listNearUsers.Add(listUSer[i]);
                }
            }

            return listNearUsers;
        }

        public static List<CUnit> GetNearUserFromPosition(int x, int y, List<CUnit> listUSer)
        {
            List<CUnit> listNearUsers = listUSer.FindAll(p =>
                p.stateData.posX == x &&
                p.stateData.posY == y);

            return listNearUsers;
        }
        
        public static GridPoint GetFrontPositionUnit(UnitDirection dir, int x, int y)
        {
            switch (dir)
            {
                case UnitDirection.UP:
                    return new GridPoint(x, y + 1);
                case UnitDirection.UP_RIGHT:
                    return new GridPoint(x + 1, y + 1);
                case UnitDirection.RIGHT:
                    return new GridPoint(x + 1, y);
                case UnitDirection.DOWN_RIGHT:
                    return new GridPoint(x + 1, y - 1);
                case UnitDirection.DOWN:
                    return new GridPoint(x, y - 1);
                case UnitDirection.DOWN_LEFT:
                    return new GridPoint(x - 1, y - 1);
                case UnitDirection.LEFT:
                    return new GridPoint(x - 1, y);
                case UnitDirection.UP_LEFT:
                    return new GridPoint(x - 1, y + 1);
            }
        
            return null;
        }

        public static Int32 DamageCalculator(CUnit attacker, CUnit deffender)
        {
            return deffender.HpMp.Hp -= 10;
        }
        
        public CGameUser GetFrontPositionTarget(CGameUser user, List<CGameUser> nearUsers)
        {
            var frontPosition = GetFrontPositionUnit((UnitDirection)user.player.stateData.direction, user.player.stateData.posX, user.player.stateData.posY);

            var target = nearUsers.Find(p => p.player.stateData.posX == frontPosition.X && p.player.stateData.posY == frontPosition.Y);

            return target;
        }
        
        public CGameUser GetTargetUserFromUserId(int targetUserId, List<CGameUser> userList)
        {
            return userList.Find(p => p.player.playerData.playerId == targetUserId);
        }

        public static List<CUnit> GetTotlNearUserList(List<CUnit> listA, List<CUnit> listB)
        {
            List<CUnit> listResult = new List<CUnit>();
            // A에 없는 B의 유저 리스트.
            var defenderList = listB.Where(i => !listA.Contains(i)).ToList();
            listResult.AddRange(listA);
            listResult.AddRange(defenderList);

            return listResult;
        }
    }
}


namespace CSampleServer
{
    public class SystemUtils
    {
        public static void SaveUserInfo(string account, JObject json)
        {
            var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}/userInfo.txt";
            // 기존껄 읽어 옴.
            var readJson = LoadJson(path);
            // 어카운트 목록을 취득.
            var token = readJson?.SelectToken(account);
            
            readJson?.Remove(account);
            
            readJson = readJson ?? new JObject();
            readJson.Add(account, json);
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                sw.Write(readJson);
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
            var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}/userInfo.txt";

            var userPackage = new UserDataPackage();
            userPackage.account = account;
            userPackage.password = password;
            userPackage.name = name;
            userPackage.userId = id;
            userPackage.data = new PlayerData() {playerId = id, name = name, unitType = 0, moveSpeed = 2, nearRange = 5};
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