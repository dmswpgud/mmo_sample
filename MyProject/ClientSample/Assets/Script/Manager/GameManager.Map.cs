using System;
using System.Collections.Generic;
using Client.Game.Map;
using UnityEngine;

public partial class GameManager : MonoBehaviour
{
    public GameObject TileObj;

    public TileInfo[,] tileInfos = new TileInfo[Map.MAX_GRID_Y, Map.MAX_GRID_Y];

    public GameObject mapCollider;

    private void MakeMap()
    {
        for (int x = 0; x < Map.MapTiles.GetLength(0); ++x)
        {
            for (int y = 0; y < Map.MapTiles.GetLength(1); ++y)
            {
                GameObject ins =  Instantiate(TileObj, transform) as GameObject;
				
                ins.transform.position = new Vector3(x, 0, y);

                var tileInfo = ins.AddComponent<TileInfo>();

                tileInfo.Init(x, y);
                
                tileInfo.isBlock = Map.MapTiles[x, y] == 0;

                tileInfos[x, y] = tileInfo;

                if (tileInfo.isBlock == true)
                {
                    MeshRenderer mt = ins.GetComponent<MeshRenderer>();
					
                    mt.material.color = Color.blue;
                }
            }
        }
    }

    public TileInfo GetTileInfo(int x, int y)
    {
        if (ExistsMapInfo(x, y) == false)
            return null;
        
        return tileInfos[x, y];
    }
    
    public TileInfo GetTargetPanel(float x, float z)
    {
        var panelX = Math.Round(x);
        var panelY = Math.Round(z);
        
//        Debug.Log($"{(int) panelX} {(int) panelY}");

        return GetTileInfo((int) panelX, (int) panelY);
    }
    
    public void SetUnitTile(TileInfo targetPanel, Unit targetObject)
    {
        var tile = tileInfos[targetPanel.GridPoint.X, targetPanel.GridPoint.Y];

        RemoveUnitTile(targetObject);

        tile.AddObjectBase(targetObject);
    }

    private void RemoveUnitTile(Unit targetObject)
    {
        for (int y = 0; y < tileInfos.GetLength(0); ++y)
        {
            for (int x = 0; x < tileInfos.GetLength(1); ++x)
            {
                if (tileInfos[x, y].HasTargetObject(targetObject))
                {
                    tileInfos[x, y].RemoveObjectBase(targetObject);
                    return;
                }
            }
        }
    }
    
    private void DrawWall()
    {
        for (int x = 0; x < Map.MapTiles.GetLength(0); ++x)
        {
            for (int y = 0; y < Map.MapTiles.GetLength(1); ++y)
            {
                var tile = GetTileInfo(x, y);

                if (tile.isBlock)
                    continue;
                
                MeshRenderer mt = tile.GetComponent<MeshRenderer>();
					
                mt.material.color = Color.white;
            }
        }
    }

    private void DrawTile(List<GridPoint> path)
    {
        for (int i = 0; i < path.Count; ++i)
        {
            var tile = GetTileInfo(path[i].X, path[i].Y);
            
            MeshRenderer mt = tile.GetComponent<MeshRenderer>();
					
            mt.material.color = Color.red;
        }
    }

    public List<TileInfo> GetRangeTiles(GridPoint centerPoint, int range)
    {
        List<TileInfo> tiles = new List<TileInfo>();
        
        for (int x = centerPoint.X - range; x < centerPoint.X + range; ++x)
        {
            for (int y = centerPoint.Y - range; y < centerPoint.Y + range; ++y)
            {
                if (x < 0 || tileInfos.GetLength(0) - 1 < x)
                    continue;
                
                if (y < 0 || tileInfos.GetLength(1) - 1 < y)
                    continue;
                
                tiles.Add(GetTileInfo(x, y));
            }
        }
        
        return tiles;
    }

    public List<GridPoint> GetRangeGridPoint(GridPoint centerPoint, int range)
    {
        List<GridPoint> tiles = new List<GridPoint>();
        
        for (int x = centerPoint.X - range; x <= centerPoint.X + range; ++x)
        {
            for (int y = centerPoint.Y - range; y <= centerPoint.Y + range; ++y)
            {
                if (x < 0 || tileInfos.GetLength(0) - 1 < x)
                    continue;
                
                if (y < 0 || tileInfos.GetLength(1) - 1 < y)
                    continue;
                
                tiles.Add(new GridPoint(x, y));
            }
        }
        
        return tiles;
    }

    public int[,] GetRangeMapTile(GridPoint center, int range)
    {
        int[,] tiles = new int[range * 2 + 1, range * 2 + 1];

        for (int x = 0; x < tiles.GetLength(0); ++x)
        {
            for (int y = 0; y < tiles.GetLength(1); ++y)
            {
                tiles[x, y] = Map.MapTiles[x + range, y + range];
            }
        }
        
        return tiles;
    }

    public int GetDistance(int currentX, int currentY, int targetX, int targetY)
    {
        int a = targetX - currentX;    // 선 a의 길이
        int b = targetY - currentY;    // 선 b의 길이
        var dis = Mathf.Sqrt((a * a) + (b * b));  
        
        return (int)dis;
    }
    
    public bool ExistsMapInfo(int x, int y)
    {
        if (x < 0 || x >= tileInfos.GetLength(0))
            return false;

        if (y < 0 || y >= tileInfos.GetLength(1))
            return false;
            
        return true;
    }
}
