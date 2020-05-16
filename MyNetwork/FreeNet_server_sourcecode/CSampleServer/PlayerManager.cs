using System.Collections.Generic;

namespace CSampleServer
{
    public class PlayerManager
    {
        private static PlayerManager instance = null;
        public static PlayerManager I
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlayerManager();
                }
                return instance;
            }
        }
        
        List<CPlayer> listPlayer = new List<CPlayer>();

        public void Initialized()
        {
            
        }
        
        public void AddPlayer(CPlayer instance)
        {
            listPlayer.Add(instance);
            Program.gameServer.userList.Add(instance);
        }

        public void RemovePlayer(CPlayer instance)
        {
            listPlayer.Remove(instance);
            Program.gameServer.userList.Remove(instance);
            instance = null;
        }
    }
}