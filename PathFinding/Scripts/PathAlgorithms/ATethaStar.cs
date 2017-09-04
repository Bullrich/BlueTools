using System.Collections.Generic;
using UnityEngine;

namespace Blue.Pathfinding
{
    public class ATethaStar : AStar
    {
        public ATethaStar(Grid grid) : base(grid)
        {
            
        }

        protected override Vector3[] SimplifyPath(List<Node> path)
        {
            List<Vector3> waypoints = new List<Vector3> {path[0].worldPosition};
            for (int i = 0; i < path.Count; i++)
            {
                for (int j = path.Count - 1; j > i; j--)
                {
                    float distance = Vector3.Distance(path[i].worldPosition, path[j].worldPosition);
                    // have to change unwalkable mask to somethin more usefull

                    if (Physics.Raycast(path[i].worldPosition, path[j].worldPosition - path[i].worldPosition, distance,
                        grid.unwalkableMask)) continue;
                    
                    i = j;
                    waypoints.Add(path[i].worldPosition);
                    break;
                }
            }
            return waypoints.ToArray();
        }
    }
}