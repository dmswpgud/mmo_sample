using System;
using UnityEngine;
using UnityEngine.UI;

public class AnnounceDialog : MonoBehaviour
{
    [SerializeField]
    private Text txMsg;

    private Action onConfirm;

    public void Show(string msg, Action cbConfirm)
    {
        txMsg.text = msg;
        onConfirm = cbConfirm;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
    
    public void OnClickConfirm()
    {
        onConfirm?.Invoke();
    }
}
