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

        [Range(0, 5f)]
        [SerializeField]
        private float checkGridStatusLapse = 0;
        private float timeSinceCheck;

        static PathRequestManager instance;
        Pathfinding pathfinding;

        bool isProcessingPath;

        public delegate void gridChange();
        private static List<gridChange> changeSuscriptors = new List<gridChange>();

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
        public static void RequestRandomPath(Vector3 pathStart, int pathLength, Action<Vector3[], bool> callback)
        {
            Pathfinding pathfinding = instance.pathfinding;
            Node startNode = pathfinding.convertPosToNode(pathStart);
            Node randomNode = pathfinding.getRandomNodeAtDistance(startNode, pathLength);
            if (randomNode != null)
            {
                PathRequest newRequest = new PathRequest(startNode, randomNode, callback);
                instance.pathRequestQueue.Enqueue(newRequest);
                instance.TryProcessNext();
            }
            else
                Debug.LogError("Node not found!");
        }

        public static void SuscribeToChange(gridChange change)
        {
            changeSuscriptors.Add(change);
        }

        public static void RemoveFromChange(gridChange change)
        {
            changeSuscriptors.Remove(change);
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

        private void Update()
        {
            // Checks if there was a change on the grid
            if (checkGridStatusLapse > 0 && changeSuscriptors.Count > 0)
            {
                timeSinceCheck += Time.time;
                if (timeSinceCheck > checkGridStatusLapse)
                {
                    timeSinceCheck = 0;
                    if (pathfinding.GetGridChange())
                        foreach (gridChange suscriptor in changeSuscriptors)
                            suscriptor();
                }
            }
        }

        public static int GetDistanceFromPoints(Vector3 startPos, Vector3 endPos)
        {
            return instance.pathfinding.GetDistanceNodes(startPos, endPos);
        }

        struct PathRequest
        {
            public Node pathStart, pathEnd;
            public Action<Vector3[], bool> callback;

            public PathRequest(Node _start, Node _end, Action<Vector3[], bool> _callback)
            {
                pathStart = _start;
                pathEnd = _end;
                callback = _callback;
            }

        }
    }
}