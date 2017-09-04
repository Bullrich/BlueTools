using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Blue.Pathfinding
{
    [RequireComponent(typeof(Pathfinding))]
    public class PathRequestManager : MonoBehaviour
    {
        private readonly Queue<PathRequest> _pathRequestQueue = new Queue<PathRequest>();
        private PathRequest _currentPathRequest;

        [Range(0, 5f)] [SerializeField] private float checkGridStatusLapse = 0;
        private float _timeSinceCheck;

        private static PathRequestManager _instance;
        private Pathfinding _pathfinding;

        private bool _isProcessingPath;

        public delegate void gridChange();

        private static readonly List<gridChange> changeSuscriptors = new List<gridChange>();

        private void Awake()
        {
            _instance = this;
            _pathfinding = GetComponent<Pathfinding>();
        }

        /// <summary>Request a way of traveling from one point to another</summary>
        /// <para>It will return an array, if the array's length is zero, it means it didn't found a path</para>
        /// <param name="callback">Callback that accepts a Vector3[]. An empty array means no path was found</param>
        public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[]> callback)
        {
            Pathfinding pathfinding = _instance._pathfinding;
            PathRequest newRequest = new PathRequest(
                pathfinding.ConvertPosToNode(pathStart), pathfinding.ConvertPosToNode(pathEnd), callback);
            _instance._pathRequestQueue.Enqueue(newRequest);
            _instance.TryProcessNext();
        }

        /// <summary>Request a random path for patrolling</summary>
        public static void RequestRandomPath(Vector3 pathStart, int pathLength, Action<Vector3[]> callback)
        {
            Pathfinding pathfinding = _instance._pathfinding;
            Node startNode = pathfinding.ConvertPosToNode(pathStart);
            Node randomNode = pathfinding.GetRandomNodeAtDistance(startNode, pathLength);
            if (randomNode != null)
            {
                PathRequest newRequest = new PathRequest(startNode, randomNode, callback);
                _instance._pathRequestQueue.Enqueue(newRequest);
                _instance.TryProcessNext();
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

        private void TryProcessNext()
        {
            if (_isProcessingPath || _pathRequestQueue.Count <= 0) return;
            
            _currentPathRequest = _pathRequestQueue.Dequeue();
            _isProcessingPath = true;
            _pathfinding.StartFindPath(_currentPathRequest.PathStart, _currentPathRequest.PathEnd);
        }

        public bool FindIfAccesible(Vector3 startPos, Vector3 endPos)
        {
            return _instance._pathfinding.FoundIfAccesible(startPos, endPos);
        }

        public void FinishedProcessingPath(Vector3[] path)
        {
            _currentPathRequest.Callback(path);
            _isProcessingPath = false;
            TryProcessNext();
        }

        private void Update()
        {
            // Checks if there was a change on the grid
            if (checkGridStatusLapse > 0 && changeSuscriptors.Count > 0)
                UpdateGridStatus();
        }

        private void UpdateGridStatus()
        {
            _timeSinceCheck += Time.time;
            if (!(_timeSinceCheck > checkGridStatusLapse)) return;
            _timeSinceCheck = 0;

            if (!_pathfinding.GetGridChange()) return;
            
            foreach (gridChange suscriptor in changeSuscriptors)
                suscriptor();
        }

        public static int GetDistanceFromPoints(Vector3 startPos, Vector3 endPos)
        {
            return _instance._pathfinding.GetDistanceNodes(startPos, endPos);
        }

        private struct PathRequest
        {
            public readonly Node PathStart, PathEnd;
            public readonly Action<Vector3[]> Callback;

            public PathRequest(Node start, Node end, Action<Vector3[]> callback)
            {
                PathStart = start;
                PathEnd = end;
                Callback = callback;
            }
        }
    }
}