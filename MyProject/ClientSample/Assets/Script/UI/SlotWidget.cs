using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotWidget : MonoBehaviour
{
    public Text _text;
    private ItemInfo _itemInfo;

    public void SetData(ItemInfo itemInfo)
    {
        _itemInfo = itemInfo;

        _text.text = $"{_itemInfo.itemName}\n({_itemInfo.count})";
    }

    public void Remove()
    {
        _itemInfo = null;

        _text.text = "";
    }
}
