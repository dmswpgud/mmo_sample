using System.Collections.Generic;
using GameServer;

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
        }

        public void RemovePlayer(CPlayer instance)
        {
            listPlayer.Remove(instance);
            instance = null;
        }

        public void ResetDeathPlayer(UserDataPackage userPack)
        {
            userPack.state.state = (byte) PlayerState.IDLE;
            userPack.state.posX = 0;
            userPack.state.posY = 0;
            userPack.hpMp.Hp = userPack.hpMp.MaxHp / 2;
        }

        public UserDataPackage InitializedPlayerData(string account, string pw, string name, int id)
        {
            var userPackage = new UserDataPackage();
            userPackage.account = account;
            userPackage.password = pw;
            userPackage.name = name;
            userPackage.userId = id;
            userPackage.data = new PlayerData() {playerId = id, name = name, unitType = 0, moveSpeed = 2};
            userPackage.state = new PlayerStateData() {playerId = id, posX = 10, posY = 10, direction = 4};
            userPackage.hpMp = new HpMp() {MaxHp = 5000, MaxMp = 10, Hp = 5000, Mp = 10, HpRecoveryTime = 10, MpRecoveryTime = 10};

            return userPackage;
        }
    }
}