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

    private void DrawPath(List<GridPoint> path)
    {
        for (int i = 0; i < path.Count; ++i)
        {
            var tile = GetTileInfo(path[i].X, path[i].Y);
            
            MeshRenderer mt = tile.GetComponent<MeshRenderer>();
					
            mt.material.color = Color.red;
        }
    }
}
