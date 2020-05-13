using GameServer;
using UnityEngine;

public partial class GameManager : MonoBehaviour
{
    public static GameManager Inst;
    
    public int UserId;

    public void Awake()
    {
        Inst = this;

        StartTitle();
    }

    private void StartTitle()
    {
        LoginDialog.gameObject.SetActive(true);
    }

    public void StartGame()
    {
        ChatDialog.gameObject.SetActive(true);
        
        MakeMap();

        CNetworkManager.Inst.RegisterDisconnectedServer(OnDisconnectServer);
        CNetworkManager.Inst.RegisterDisconnectedPlayer(DisconnectedPlayer);
        CNetworkManager.Inst.RequsetGetMyPlayer(SpawnUnit);
        CNetworkManager.Inst.RegisterAddNearPlayer(SpawnUnit);
        CNetworkManager.Inst.RegisterRemoveNearPlayer(DestroyUnit);
        CNetworkManager.Inst.RegisterOtherPlayerMove(ResponseMovePlayer);
        CNetworkManager.Inst.RegisterChangedOtherPlayerstate(OnReceivedChangedPlayerState);
    }

    private void Update()
    {
        UpdateGameManagerMap();
        UpdateGameManagerPlayer();
    }

    private void OnDisconnectServer()
    {
        AnnounceDialog.Show("서버가 종료되었습니다.", () =>
            {
                Application.Quit();
                AnnounceDialog.Close();
            });
                
    }

    private TileInfo GetClickedObject()
    {
        RaycastHit hit;
        
        GameObject target = null; 

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 

        if(Physics.Raycast(ray.origin, ray.direction * 10, out hit))
        {
            var x = hit.point.x;
            var y = hit.point.z;

            return GetTargetPanel(x, y);
        }

        return null; 
    }
    
    private void SpawnUnit(ResponseData res, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            PrintSystemLog(error.ToString());
            return;
        }
        
        var data = (PlayerDataPackage) res;

        switch ((UnitType)data.data.unitType)
        {
            case UnitType.PLAYER:
                var player = CreatePlayer(data, PlayerObj);
                players.Add(player);

                if (player.ID == UserId)
                {
                    myPlayer = player;
                    myPlayer.IsMyPlayer = true;
                    Camera.main.transform.parent = myPlayer.transform;
                    Camera.main.transform.localPosition = new Vector3(0f, 8f, -6.7f);
                    mapCollider.transform.parent = myPlayer.transform;
                    mapCollider.transform.localPosition = Vector3.zero;
                    mapCollider.SetActive(true);
                }
                break;
            case UnitType.MONSTER:
                var moster = CreatePlayer(data, PlayerObj);
                players.Add(moster);
                break;
        }
    }
    
    private void DestroyUnit(ResponseData res, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            PrintSystemLog(error.ToString());
            return;
        }
        
        var data = (PlayerData) res;
        var player = players.Find(p => p.DATA.playerId == data.playerId);
        var index = players.FindIndex(p => p.DATA.playerId == data.playerId);
        RemoveUnitTile(players[index]);
        players.RemoveAt(index);
        Destroy(player.gameObject);
    }
}
