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
        CNetworkManager.Inst.RequsetGetMyPlayer(SpawnUnits);
        CNetworkManager.Inst.RegisterAddNearPlayer(SpawnUnits);
        CNetworkManager.Inst.RegisterRemoveNearPlayer(DestroyUnits);
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

    public void SpawnUnits(ResponseData res, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            PrintSystemLog(error.ToString());
            return;
        }
        
        var datas = (UnitsDataPackate) res;
        
        

        foreach (var unitPack in datas.datas)
        {
            SpawnUnit(unitPack);
        }
    }

    private void SpawnUnit(ResponseData res, ERROR error = ERROR.NONE)
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
                var player = CreatePlayer(data, PlayerPrefab);
                player.gameObject.name = $"Player-{data.data.name}-{data.data.playerId}";
                listUnit.Add(player);

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
                var moster = CreatePlayer(data, MonsterPrefab);
                moster.gameObject.name = $"Monster-{data.data.name}-{data.data.playerId}";
                listUnit.Add(moster);
                break;
        }
    }

    private void DestroyUnits(ResponseData res, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            PrintSystemLog(error.ToString());
            return;
        }
        
        var datas = (PlayerDataPackages) res;

        foreach (var unitPack in datas.datas)
        {
            DestroyUnit(unitPack, ERROR.NONE);
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
        var player = listUnit.Find(p => p.DATA.playerId == data.playerId);

        // TODO: 먼가 중복으로 불려서 지운걸 또지우는듯한 에러가...
        RemoveUnitTile(player);
        listUnit.Remove(player);
        Destroy(player?.gameObject);
    }
}
