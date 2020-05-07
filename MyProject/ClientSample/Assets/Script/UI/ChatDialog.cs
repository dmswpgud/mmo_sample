using GameServer;
using UnityEngine;
using UnityEngine.UI;

public class ChatDialog : MonoBehaviour
{
    public InputField InputField;
    public Transform ChatWidgetParent;
    public Text ChatText;

    private void Start()
    {
        CNetworkManager.Inst.RegisterChatEvent(ReceiveChatData);
    }

    public void onClickSend()
    {
        var msg = InputField.text;

        InputField.text = "";

        CNetworkManager.Inst.RequestChatMessage(msg, ReceiveChatData);
    }

    private void ReceiveChatData(ResponseData res, ERROR error)
    {
        if (error != ERROR.NONE)
        {
            GameManager.Inst.PrintSystemLog(error.ToString());
            return;
        }
        
        var data = (ChatData)res;
        
        if (GameManager.Inst.UserId == data.userId)
        {
            PrintChatText($"\n<color=#0DFF00>{data.userId} : {data.message}</color>");
        }
        else
        {
            PrintChatText($"\n{data.userId} : {data.message}");
        }
    }

    public void PrintChatText(string text)
    {
        ChatText.text += text;
    }
}
