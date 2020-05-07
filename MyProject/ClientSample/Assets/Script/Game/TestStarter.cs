using System.Collections;
using System.Collections.Generic;
using Client.Game.Map;
using UnityEngine;

public class TestStarter : MonoBehaviour
{
	private int[,] tile = new int[10, 10]
	{
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{1, 0, 0, 0, 0, 0, 0, 0, 0, 1},
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
	};

	public GameObject TileObj;

	private PathFinder astar; 

	void Start ()
	{
		GridPoint start = new GridPoint(0, 0);
		GridPoint goal = new GridPoint(8, 8);
		
		astar = new PathFinder();
		
		List<GridPoint> result = astar.FindPath(tile, start, goal);

		for (int i = 0; i < result.Count; ++i)
		{
			Debug.Log(string.Format("x:{0}   y:{1})", result[i].X, result[i].Y));
		}

		MakeMap(result);
	}

	void MakeMap(List<GridPoint> result)
	{
		for (int x = 0; x < tile.GetLength(0); ++x)
		{
			for (int y = 0; y < tile.GetLength(1); ++y)
			{
				GameObject ins =  Instantiate(TileObj) as GameObject;
				
				ins.transform.position = new Vector3(x, 0, y);

				bool isWaked = tile[x, y] == 1;

				if (isWaked == false)
				{
					MeshRenderer mt = ins.GetComponent<MeshRenderer>();
					
					mt.material.color = Color.blue;
				}

				for (int i = 0; i < result.Count; ++i)
				{
					bool ok = result[i].X == x && result[i].Y == y;

					if (ok)
					{
						MeshRenderer mt = ins.GetComponent<MeshRenderer>();
					
						mt.material.color = Color.red;
					}
				}
			}
		}
	}
}
