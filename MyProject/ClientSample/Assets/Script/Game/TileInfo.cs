using Client.Game.Map;
using UnityEngine;

public class TileInfo : MonoBehaviour
{
    public GridPoint GridPoint;
    public bool isBlock;

    public void Init(int x, int y)
    {
        GridPoint = new GridPoint(x, y);
        
        //txPos.text = $"X:{x}\nY:{y}";
    }
}
