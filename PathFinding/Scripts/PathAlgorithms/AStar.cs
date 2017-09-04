using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Blue.Pathfinding
{
    public class AStar : IPathAlgorithm
    {
        
        protected Grid grid;
        
        public AStar(Grid grid)
        {
            this.grid = grid;
        }

        public Vector3[] FindPath(Node startNode, Node targetNode)
        {
            Vector3[] waypoints = new Vector3[0];


            if (startNode.walkable && targetNode.walkable && startNode != targetNode)
            {
                if (CalculatePathFinding(startNode, targetNode))
                {
                    waypoints = RetracePath(startNode, targetNode);
                }
            }
            return waypoints;
        }

        private Vector3[] RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            Vector3[] waypoints = null;

            waypoints = SimplifyPath(path);

            Array.Reverse(waypoints);
            return waypoints;
        }

        // Kind of theta fix. No funca con alturas, pero en un plano hace un efecto muuuy similar
        protected virtual Vector3[] SimplifyPath(List<Node> path)
        {
            List<Vector3> waypoints = new List<Vector3>();
            Vector2 directionOld = Vector2.zero;

            for (int i = 1; i < path.Count; i++)
            {
                Vector2 directionNew =
                    new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
                
                if (directionNew != directionOld)
                    waypoints.Add(path[i].worldPosition);
                directionOld = directionNew;
            }
            return waypoints.ToArray();
        }

        public bool IsNodeAccesible(Node startNode, Node endNode)
        {
            return CalculatePathFinding(startNode, endNode);
        }

        public bool CalculatePathFinding(Node startNode, Node targetNode)
        {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    return true;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour)) continue;

                    int newMovementCostToNeighbour =
                        currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenaly;

                    if (newMovementCostToNeighbour >= neighbour.gCost && openSet.Contains(neighbour)) continue;

                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                    else
                        openSet.UpdateItem(neighbour);
                }
            }
            return false;
        }
        
        
        public int GetDistanceNodes(Vector3 posA, Vector3 posB)
        {
            return GetDistanceNodes(ConvertPosToNode(posA), ConvertPosToNode(posB));
        }

        public int GetDistanceNodes(Node nodeA, Node nodeB)
        {
            int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
            return dstX + dstY;
        }
        
        public Node ConvertPosToNode(Vector3 position)
        {
            return grid.NodeFromWorldPoint(position);
        }
        
        public int GetDistance(Node nodeA, Node nodeB)
        {
            int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }
    }
}