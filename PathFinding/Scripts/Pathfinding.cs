using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Blue.Pathfinding
{
    [RequireComponent(typeof(Grid))]
    public class Pathfinding : MonoBehaviour
    {
        private PathRequestManager _requestManager;
        private Grid _grid;

        private IPathAlgorithm _pathAlgorithm;

        private enum PathType
        {
            AStar,
            ATethaStar
        }

        [SerializeField] private PathType _pathType;

        private void Awake()
        {
            _requestManager = GetComponent<PathRequestManager>();
            _grid = GetComponent<Grid>();

            switch (_pathType)
            {
                case PathType.AStar:
                    _pathAlgorithm = new AStar(_grid);
                    break;
                case PathType.ATethaStar:
                    _pathAlgorithm = new ATethaStar(_grid);
                    break;
            }
        }

        public void StartFindPath(Vector3 startPos, Vector3 targetPos)
        {
            Node startNode = ConvertPosToNode(startPos);
            Node targetNode = ConvertPosToNode(targetPos);
            StartFindPath(startNode, targetNode);
        }

        public void StartFindPath(Node startNode, Node targetNode)
        {
            StartCoroutine(FindPath(startNode, targetNode));
        }

        private IEnumerator FindPath(Node startNode, Node targetNode)
        {
            var path = _pathAlgorithm.FindPath(startNode, targetNode);
            yield return new WaitUntil(() => path != null);
            _requestManager.FinishedProcessingPath(path);
        }


        public bool FoundIfAccesible(Vector3 startPos, Vector3 endPos)
        {
            return FoundIfAccesible(ConvertPosToNode(startPos), ConvertPosToNode(endPos));
        }

        public bool FoundIfAccesible(Node startNode, Node targetNode)
        {
            Vector3[] waypoints = new Vector3[0];

            if (startNode.walkable && targetNode.walkable)
            {
                return _pathAlgorithm.IsNodeAccesible(startNode, targetNode);
            }
            return false;
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
            return _grid.NodeFromWorldPoint(position);
        }

        public Node GetRandomNodeAtDistance(Node startNode, int distance)
        {
            while (true)
            {
                System.Random rand = new System.Random();
                int yValue = rand.Next(0, distance);
                int xValue = distance - yValue;

                int newPosX = startNode.gridX + (xValue * (rand.Next(-1, 1) == 0 ? 1 : -1));
                int newPosY = startNode.gridY + (yValue * (rand.Next(-1, 1) == 0 ? 1 : -1));

                if (newPosX < _grid.gridWorldSize.x && newPosX > 0 && newPosY < _grid.gridWorldSize.y && newPosY > 0)
                {
                    Node resultNode = _grid.GetNodeFromCoordinates(newPosX, newPosY);
                    if (resultNode != null && resultNode.walkable)
                        return resultNode;
                }
            }
        }

        public bool GetGridChange()
        {
            return _grid.CheckGridStatus();
        }
    }
}