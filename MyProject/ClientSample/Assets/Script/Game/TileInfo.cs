using System;
using System.Collections.Generic;
using Client.Game.Map;
using UnityEngine;

public class TileInfo : MonoBehaviour
{
    public GridPoint GridPoint;
    public bool isBlock;
    public bool HasObject => listObject.Count != 0;

    private List<ObjectBase> listObject = new List<ObjectBase>();

    public void Init(int x, int y)
    {
        GridPoint = new GridPoint(x, y);
        
        //txPos.text = $"X:{x}\nY:{y}";
    }

    public void AddObjectBase(ObjectBase target)
    {
        listObject.Add(target);
    }

    public void RemoveObjectBase(ObjectBase target)
    {
        listObject.Remove(target);
    }

    public bool HasTargetObject(ObjectBase target)
    {
        return listObject.Contains(target);
    }
}
