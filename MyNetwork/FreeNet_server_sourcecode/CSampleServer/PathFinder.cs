using System;
using System.Collections.Generic;
using GameServer;

namespace CSampleServer
{
    public class GridPoint 
    {
        public int X, Y;
        public GridPoint(int x, int y) { X = x; Y = y; }
    }
    
    public class PathFinder
    {
        Dictionary<GridPoint, bool> closedSet = new Dictionary<GridPoint, bool>();
        Dictionary<GridPoint, bool> openSet = new Dictionary<GridPoint, bool>();

        //cost of start to this key node
        Dictionary<GridPoint, int> gScore = new Dictionary<GridPoint, int>();
        //cost of start to goal, passing through key node
        Dictionary<GridPoint, int> fScore = new Dictionary<GridPoint, int>();

        Dictionary<GridPoint, GridPoint> nodeLinks = new Dictionary<GridPoint, GridPoint>();
        
        public List<GridPoint> FindPath(int[,] graph, GridPoint start, GridPoint goal)
        {
            closedSet.Clear();
            openSet.Clear();
            gScore.Clear();
            fScore.Clear();
            nodeLinks.Clear();
            
            openSet[start] = true;
            gScore[start] = 0;
            fScore[start] = Heuristic(start, goal);

            while(openSet.Count > 0) 
            {
                var current = nextBest();
                
                if (current.X == goal.X && current.Y == goal.Y) 
                {
                    return Reconstruct(current);
                }

                openSet.Remove(current);
                closedSet[current] = true;
                
                foreach(var neighbor in Neighbors(graph, current))
                {
                    if (closedSet.ContainsKey(neighbor))
                        continue;

                    var projectedG = getGScore(current) + 1;


                    if (openSet.ContainsKey(neighbor) == false)
                    {
                        openSet[neighbor] = true;
                    }
                    else if (projectedG >= getGScore(neighbor))
                    {
                        continue;
                    }

                    //record it
                    nodeLinks[neighbor] = current;
                    gScore[neighbor] = projectedG;
                    fScore[neighbor] = projectedG + Heuristic(neighbor, goal);
                    
                    if (openSet.Count > 1000)
                    {
                        return new List<GridPoint>();
                    }
                }
            }

            return new List<GridPoint>();
        }

        private int Heuristic(GridPoint start, GridPoint goal) 
        {
            var dx = goal.X - start.X;
            var dy = goal.Y - start.Y;
            return Math.Abs(dx) + Math.Abs(dy);
        }

        private int getGScore(GridPoint pt)
        {
            int score = int.MaxValue;
            gScore.TryGetValue(pt, out score);
            return score;    
        }


        private int getFScore(GridPoint pt)
        {
            int score = int.MaxValue;
            fScore.TryGetValue(pt, out score);
            return score;
        }

        public static IEnumerable<GridPoint> Neighbors(int[,] graph, GridPoint center)
        {

            GridPoint pt = new GridPoint(center.X - 1, center.Y - 1);
            if (IsValidNeighbor(graph, pt))
                yield return pt;

            pt = new GridPoint(center.X, center.Y - 1);
            if (IsValidNeighbor(graph, pt))
                yield return pt;

            pt = new GridPoint(center.X + 1, center.Y - 1);
            if (IsValidNeighbor(graph, pt))
                yield return pt;

            //middle row
            pt = new GridPoint(center.X - 1, center.Y);
            if (IsValidNeighbor(graph, pt))
                yield return pt;
            
            pt = new GridPoint(center.X + 1, center.Y);
            if (IsValidNeighbor(graph, pt))
                yield return pt;

            
            //bottom row
            pt = new GridPoint(center.X - 1, center.Y+1);
            if (IsValidNeighbor(graph, pt))
                yield return pt;

            pt = new GridPoint(center.X, center.Y + 1);
            if (IsValidNeighbor(graph, pt))
                yield return pt;

            pt = new GridPoint(center.X+1, center.Y + 1);
            if (IsValidNeighbor(graph, pt))
                yield return pt;
        }

        public static bool IsValidNeighbor(int[,] matrix, GridPoint pt) 
        {
            int x = pt.X;
            int y = pt.Y;
            if (x < 0 || x >= matrix.GetLength(0))
                return false;

            if (y < 0 || y >= matrix.GetLength(1))
                return false;

            return MapManager.I.HasUnit(x, y, UnitType.MONSTER) == false && matrix[x, y] == 1;
        }

        private List<GridPoint> Reconstruct(GridPoint current) 
        {
            List<GridPoint> path = new List<GridPoint>();
            while (nodeLinks.ContainsKey(current))
            {
                path.Add(current);
                current = nodeLinks[current];
            }

            path.Reverse();
            return path;
        }

        private GridPoint nextBest() 
        {
            int best = int.MaxValue;
            GridPoint bestPt = null;
            foreach( var node in openSet.Keys) 
            {
                var score = getFScore(node);
                
                if(score < best) 
                {
                    bestPt = node;
                    best = score;
                }
            }

            return bestPt;
        }
    }
}