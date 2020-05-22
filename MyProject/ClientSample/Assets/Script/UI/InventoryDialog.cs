using System.Collections.Generic;
using UnityEngine;

public class InventoryDialog : MonoBehaviour
{
    public Transform _content;
    private List<ItemInfo> _listItem;
    private List<SlotWidget> _listSlotWidget = new List<SlotWidget>();

    void Awake()
    {
        foreach (Transform child in _content.transform)
        {
            var slot = child.GetComponent<SlotWidget>();
            _listSlotWidget.Add(slot);
        }
    }
    
    public void OnInventory(List<ItemInfo> items = null)
    {
        if (gameObject.activeSelf)
        {
            Close();
        }
        else
        {
            Show();
        }
        
        _listItem = items;
        MakeItemSlot();
    }

    private void MakeItemSlot()
    {
        for (int i = 0; i < _listItem.Count; ++i)
        {
            _listSlotWidget[i].SetData(_listItem[i]);
;        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Close()
    {
        foreach (var slot in _listSlotWidget)
        {
            slot.Remove();
        }
        
        gameObject.SetActive(false);
    }
    
    public void OnClickClose()
    {
        Close();
    }
}
