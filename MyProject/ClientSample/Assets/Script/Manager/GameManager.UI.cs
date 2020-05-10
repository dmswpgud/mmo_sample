using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameManager : MonoBehaviour
{
    public LoginDialog LoginDialog;

    public ChatDialog ChatDialog;

    public AnnounceDialog AnnounceDialog;
    
    public void PrintSystemLog(string text)
    {
        var txt = "\n<color=#FF0000>" + text + "</color>";
        
        ChatDialog.PrintChatText(txt);
    }
}
