using System;
using System.Collections;
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
        return tileInfos[x, y];
    }
    
    public TileInfo GetTargetPanel(float x, float z)
    {
        var panelX = Math.Round(x);
        var panelY = Math.Round(z);
        
        Debug.Log($"{(int) panelX} {(int) panelY}");

        return GetTileInfo((int) panelX, (int) panelY);
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
}
