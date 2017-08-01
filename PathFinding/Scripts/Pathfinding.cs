using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Blue.Pathfinding
{
    public class Pathfinding : MonoBehaviour
    {
        PathRequestManager requestManager;
        Grid grid;

       public bool useTheta;

        void Awake()
        {
            requestManager = GetComponent<PathRequestManager>();
            grid = GetComponent<Grid>();
        }

        public void StartFindPath(Vector3 startPos, Vector3 targetPos)
        {
            Node startNode = convertPosToNode(startPos);
            Node targetNode = convertPosToNode(targetPos);
            StartCoroutine(FindPath(startNode, targetNode));
        }

        public void StartFindPath(Node startNode, Node targetNode)
        {
            StartCoroutine(FindPath(startNode, targetNode));
        }

        IEnumerator FindPath(Node startNode, Node targetNode)
        {
            Vector3[] waypoints = new Vector3[0];
            bool pathSuccess = false;


            if (startNode.walkable && targetNode.walkable)
            {
                CalculatePathFinding(startNode, targetNode, ref pathSuccess);
            }
            yield return null;
            if (pathSuccess)
            {
                waypoints = RetracePath(startNode, targetNode);
            }
            requestManager.FinishedProcessingPath(waypoints, pathSuccess);

        }

        private void CalculatePathFinding(Node startNode, Node targetNode, ref bool pathSuccess)
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
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenaly;
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }
            }
        }

        public bool FoundIfAccesible(Vector3 startPos, Vector3 endPos)
        {
            return FoundIfAccesible(convertPosToNode(startPos), convertPosToNode(endPos));
        }

        public bool FoundIfAccesible(Node startNode, Node targetNode)
        {
            Vector3[] waypoints = new Vector3[0];
            bool pathSuccess = false;

            if (startNode.walkable && targetNode.walkable)
            {
                CalculatePathFinding(startNode, targetNode, ref pathSuccess);
            }
            return pathSuccess;
        }

        Vector3[] RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            Vector3[] waypoints = null;
            if (useTheta)
                waypoints = ThetaPath(path);
            else
                waypoints = SimplifyPath(path);
            Array.Reverse(waypoints);
            return waypoints;

        }

        Vector3[] quickFix(List<Node> nodes)
        {
            List<Vector3> fix = new List<Vector3>();
            foreach (Node n in nodes)
            {
                fix.Add(n.worldPosition);
            }
            return fix.ToArray();
        }

        public int GetNodesFromDistance(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            return path.Count;
        }

        // Kind of theta fix. No funca con alturas, pero en un plano hace un efecto muuuy similar
        Vector3[] SimplifyPath(List<Node> path)
        {
            List<Vector3> waypoints = new List<Vector3>();
            Vector2 directionOld = Vector2.zero;

            for (int i = 1; i < path.Count; i++)
            {
                Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
                if (directionNew != directionOld)
                {
                    waypoints.Add(path[i].worldPosition);
                }
                directionOld = directionNew;
            }
            return waypoints.ToArray();
        }

        Vector3[] ThetaPath(List<Node> path)
        {
            List<Vector3> waypoints = new List<Vector3>();
            waypoints.Add(path[0].worldPosition);
            for (int i = 0; i < path.Count; i++)
            {
                for (int j = path.Count-1; j > i; j--)
                {
                    float distance = Vector3.Distance(path[i].worldPosition, path[j].worldPosition);
                    // have to change unwalkable mask to somethin more usefull

                    if (!Physics.Raycast(path[i].worldPosition, path[j].worldPosition - path[i].worldPosition, distance, grid.unwalkableMask))
                    {
                        i = j;

                        waypoints.Add(path[i].worldPosition);
                        break;
                    }
                }
            }
            return waypoints.ToArray();
        }

        int GetDistance(Node nodeA, Node nodeB)
        {
            int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }

        public int GetDistanceNodes(Vector3 posA, Vector3 posB)
        {
            return GetDistanceNodes(convertPosToNode(posA), convertPosToNode(posB));
        }

        public int GetDistanceNodes(Node nodeA, Node nodeB)
        {
            int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
            return dstX + dstY;
        }

        public Node convertPosToNode(Vector3 position)
        {
            return grid.NodeFromWorldPoint(position);
        }

        public Node getRandomNodeAtDistance(Node startNode, int distance)
        {
            while (true)
            {
                System.Random rand = new System.Random();
                int yValue = rand.Next(0, distance);
                int xValue = distance - yValue;

                int newPosX = startNode.gridX + (xValue * (rand.Next(-1, 1) == 0 ? 1 : -1));
                int newPosY = startNode.gridY + (yValue * (rand.Next(-1, 1) == 0 ? 1 : -1));

                if (newPosX < grid.gridWorldSize.x && newPosX > 0 && newPosY < grid.gridWorldSize.y && newPosY > 0)
                {
                    Node resultNode = grid.getNodeFromCoordinates(newPosX, newPosY);
                    if (resultNode != null && resultNode.walkable)
                        return resultNode;
                }
            }
        }
    }
}