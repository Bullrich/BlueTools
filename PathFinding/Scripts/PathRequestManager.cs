using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Blue.Pathfinding
{
    [RequireComponent(typeof(Pathfinding))]
    public class PathRequestManager : MonoBehaviour
    {
        Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
        PathRequest currentPathRequest;

        static PathRequestManager instance;
        Pathfinding pathfinding;

        bool isProcessingPath;

        void Awake()
        {
            instance = this;
            pathfinding = GetComponent<Pathfinding>();
        }

        /// <summary>Request a way of traveling from one point to another</summary>
        public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
        {
            Pathfinding pathfinding = instance.pathfinding;
            PathRequest newRequest = new PathRequest(
                pathfinding.convertPosToNode(pathStart), pathfinding.convertPosToNode(pathEnd), callback);
            instance.pathRequestQueue.Enqueue(newRequest);
            instance.TryProcessNext();
        }

        /// <summary>Request a random path for patrolling</summary>
        public static void RequestRandomPath(Vector3 pathStart, int pathLength ,Action<Vector3[], bool> callback)
        {
            Pathfinding pathfinding = instance.pathfinding;
            Node startNode = pathfinding.convertPosToNode(pathStart);
            Node randomNode = pathfinding.getRandomNodeAtDistance(startNode, pathLength);
            if(randomNode != null){
            //print(string.Format("Path start: {0}|{1}, pathEnd: {2}|{3}",startNode.gridX,startNode.gridY, randomNode.gridX,randomNode.gridY));
            PathRequest newRequest = new PathRequest(startNode, randomNode, callback);
            instance.pathRequestQueue.Enqueue(newRequest);
            instance.TryProcessNext();
            }
            else
            Debug.LogError("Node not found!");
        }

        void TryProcessNext()
        {
            if (!isProcessingPath && pathRequestQueue.Count > 0)
            {
                currentPathRequest = pathRequestQueue.Dequeue();
                isProcessingPath = true;
                pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
            }
        }

        public bool FindIfAccesible(Vector3 startPos, Vector3 endPos)
        {
            return instance.pathfinding.FoundIfAccesible(startPos, endPos);
        }

        public void FinishedProcessingPath(Vector3[] path, bool success)
        {
            currentPathRequest.callback(path, success);
            isProcessingPath = false;
            TryProcessNext();
        }

        public static int GetDistanceFromPoints(Vector3 startPos, Vector3 endPos)
        {
            return instance.pathfinding.GetDistanceNodes(startPos, endPos);
        }

        struct PathRequest
        {
            public Node pathStart, pathEnd;
            public Action<Vector3[], bool> callback;

            public PathRequest(Node _start, Node _end, Action<Vector3[], bool> _callback){
                pathStart = _start;
                pathEnd = _end;
                callback = _callback;
            }

        }
    }
}