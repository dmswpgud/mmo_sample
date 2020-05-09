using System.Collections.Generic;
using Client.Game.Map;
using UnityEngine;

public class TileInfo : MonoBehaviour
{
    public GridPoint GridPoint;
    public bool isBlock;
    public bool HasObject => listObject.Count != 0;
    private List<Unit> listObject = new List<Unit>();

    public void Init(int x, int y)
    {
        GridPoint = new GridPoint(x, y);
    }

    public void AddObjectBase(Unit target)
    {
        listObject.Add(target);
    }

    public void RemoveObjectBase(Unit target)
    {
        listObject.Remove(target);
    }

    public bool HasTargetObject(Unit target)
    {
        return listObject.Contains(target);
    }
}
