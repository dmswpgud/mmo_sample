using System.Collections.Generic;
using Client.Game.Map;
using UnityEngine;

public class TileInfo : MonoBehaviour
{
    public GridPoint GridPoint;
    public bool isBlock;
    public bool HasObject => listUnit.Count != 0;
    private List<Unit> listUnit = new List<Unit>();

    public void Init(int x, int y)
    {
        GridPoint = new GridPoint(x, y);
    }

    public void AddObjectBase(Unit target)
    {
        listUnit.Add(target);
    }

    public void RemoveObjectBase(Unit target)
    {
        listUnit.Remove(target);
    }

    public bool HasTargetObject(Unit target)
    {
        return listUnit.Contains(target);
    }

    public Unit GetTileUnit()
    {
        if (listUnit.Count == 0)
            return null;

        return listUnit[0];
    }
}
