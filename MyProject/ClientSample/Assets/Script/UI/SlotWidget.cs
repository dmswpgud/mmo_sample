using System;
using UnityEngine;
using UnityEngine.UI;

public class SlotWidget : MonoBehaviour
{
    public Text _text;
    private ItemInfo _itemInfo;
    private Action<ItemInfo> _OnClickedSlot;

    public void SetData(ItemInfo itemInfo, Action<ItemInfo> onClickedSlot)
    {
        _OnClickedSlot = onClickedSlot;
        
        _itemInfo = itemInfo;

        _text.text = $"{_itemInfo.itemName}\n({_itemInfo.count})";
    }

    public void Remove()
    {
        _itemInfo = null;

        _text.text = "";
    }
        
    public void OnClickedSlot()
    {
        _OnClickedSlot?.Invoke(_itemInfo);
    }
}
