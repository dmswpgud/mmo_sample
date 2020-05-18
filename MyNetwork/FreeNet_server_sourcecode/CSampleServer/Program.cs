﻿using System;
using System.Collections.Generic;
using FreeNet;

namespace CSampleServer
{
    class Program
    {
        public static List<CGameUser> userlist;
        public static CGameServer gameServer = new CGameServer();

        public delegate void Loop();
        public static Loop Tick;
        public static string  userInfoJsonPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}/mmo_table/userInfo.txt";
        public static string  monsterSpawnInfoJsonPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}/mmo_table/monsterSpawnInfo.txt";
        public static string  monsterInfoJsonPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}/mmo_table/monsterInfo.txt";
        public static string  monsterAiInfoJsonPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}/mmo_table/monsterAiInfo.txt";

        static void Main(string[] args)
        {
            CPacketBufferManager.initialize(2000);
            userlist = new List<CGameUser>();

            CNetworkService service = new CNetworkService();
            // 콜백 매소드 설정.
            service.session_created_callback += on_session_created;
            // 초기화.
            service.initialize();
            service.listen("127.0.0.1", 7979, 100);
            
            // 게임서버 초기화.
            gameServer.Initialized();

            Console.WriteLine("Started!");
            while (true)
            {
                if (Tick != null)
                {
                    Tick();    
                }
                //Console.Write(".");
                System.Threading.Thread.Sleep(100);
            }

            Console.ReadKey();
        }

        /// <summary>
        /// 클라이언트가 접속 완료 하였을 때 호출됩니다.
        /// n개의 워커 스레드에서 호출될 수 있으므로 공유 자원 접근시 동기화 처리를 해줘야 합니다.
        /// </summary>
        /// <returns></returns>
        static void on_session_created(CUserToken token)
        {
            CGameUser user = new CGameUser(token);
            lock (userlist)
            {
                userlist.Add(user);
            }
        }

        public static void remove_user(CGameUser user)
        {
            userlist.Remove(user);
        }

        public static void PrintLog(string log)
        {
            Console.WriteLine(log);
        }
    }
}