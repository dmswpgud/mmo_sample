using System.Collections;
using System.Collections.Generic;
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
}