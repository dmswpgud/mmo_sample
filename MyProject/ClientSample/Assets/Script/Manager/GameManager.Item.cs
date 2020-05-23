using System.Collections;
using System.Collections.Generic;
using GameServer;
using UnityEngine;

public partial class GameManager
{
    private List<ItemInfo> _listItem = new List<ItemInfo>();

    public void AddItem(ItemInfo item)
    {
        var index = _listItem.FindIndex(p => p.uniqueId == item.uniqueId);
        
        if (index != -1)
        {
            _listItem[index] = item;
            return;
        }

        _listItem.Add(item);
    }
    
    public void RequestUseItem(ItemInfo itemInfo)
    {
            CNetworkManager.Inst.RequestUseItem(itemInfo.uniqueId, (res, res2, error) =>
        {
            if (error != ERROR.NONE)
            {
                PrintSystemLog(error.ToString());
                return;
            }

            var item = (ItemInfo)res;
            var userPack = (UnitDataPackage) res2;

            AddItem(item);
            myPlayer.SetHpMp(userPack.hpMp);
            _inventoryDialog.UpdateItems(_listItem);
        });
    }
}