using System.Collections.Generic;
using System.Linq;
using FreeNet;
using GameServer;

namespace CSampleServer
{
    public class CMonster : CUnit
    {
        public CMonster(PlayerDataPackage userPack)  : base(userPack)
        {
            CPacket response = CPacket.create((short)PROTOCOL.MONSTER_SPONE_RES);
            playerData.PushData(response);
            stateData.PushData(response);
            HpMp.PushData(response);

            SetPosition(stateData.posX, stateData.posY, stateData.direction);
        }
        
        public override void SetPosition(int x, int y, int dir)
        {
            stateData.posX = (short)x;
            stateData.posY = (short)y;
            stateData.direction = (byte)dir;

            MapManager.I.AddUnitTile(this, x, y);
        }

        // 플레이어가 범위내에 있을 시 알리기.
        public override void ResponseAddNearUnit(List<CUnit> units)
        {
            // if (base.IsPlayer() == false)
            //     return;
            //
            // CPacket response = CPacket.create((short)PROTOCOL.ADD_NEAR_PLAYER_RES);
            // user.playerData.PushData(response);
            // user.stateData.PushData(response);
            // user.HpMp.PushData(response);
            // owner?.send(response);
            //
            // Program.PrintLog($"[{playerData.name}]의 범위내에 [{user.playerData.name}]가 들어옴.");
        }

        // 플레이어가 범위내를 벚어났을 때 알리기.
        public override void ResponseRemoveNearUnit(List<CUnit> units)
        {
            // if (base.IsPlayer() == false)
            //     return;
            //
            // CPacket response = CPacket.create((short)PROTOCOL.REMOVE_NEAR_PLAYER_RES);
            // user.playerData.PushData(response);
            // owner?.send(response);
            //
            // Program.PrintLog($"[{playerData.name}]의 범위내에서[{user.playerData.name}]가 벚어남.");
        }

        public override void RequestPlayerMove()
        {
        }

        public override void RequestPlayerState(int receiveUserId)
        {
        }

        public override void DisconnectedPlayer()
        {
        }
    }
}