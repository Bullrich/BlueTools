using UnityEngine;
using System.Collections;

namespace Blue.Pathfinding
{
    public class Unit : MonoBehaviour
    {
        public float speed = 20;
        private Vector3[] _path;

        private int _targetIndex;

        private bool _hasRequestedPath = false;

        protected Vector3 waypoint, targetPosition;
        public bool showPath = true;

        protected void OnPathFound(Vector3[] newPath)
        {
            _hasRequestedPath = false;

            if (newPath.Length <= 0) return;
            
            _path = newPath;
            _targetIndex = 0;
            waypoint = _path[0];
            targetPosition = _path[_path.Length - 1];
            // While following the path, adds a verification and makes sure that, if the grid change, it's path should be updated
            PathRequestManager.SuscribeToChange(SuscriptionToChange);
        }

        private void SuscriptionToChange()
        {
            NavigateToPoint(transform.position, targetPosition);
        }

        /// <summary>Move to the waypoint, when close enough switchs to the next waypoint</summary>
        private void MoveThroughPath(ref Vector3 currentWaypoint)
        {
            float distance = Vector3.Distance(transform.position, currentWaypoint);
            if (distance < .5f)
            {
                _targetIndex++;
                if (_targetIndex >= _path.Length)
                {
                    _path = null;
                    // When it reaches it's target, it unsuscribe from the grid update.
                    PathRequestManager.RemoveFromChange(SuscriptionToChange);
                    return;
                }
                currentWaypoint = _path[_targetIndex];
            }
            MoveTo(currentWaypoint);
        }

        public virtual void Update()
        {
            if (_path != null)
                MoveThroughPath(ref waypoint);
        }

        public virtual void MoveTo(Vector3 targetPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        }

        /// <summary>Navigate to a point in the map evading all the unwalkable objects</summary>
        public void NavigateToPoint(Vector3 pathStart, Vector3 pathEnd)
        {
            if (_hasRequestedPath || pathEnd == Vector3.zero) return;
            
            PathRequestManager.RequestPath(pathStart, pathEnd, OnPathFound);
            _hasRequestedPath = true;
        }

        public void OnDrawGizmos()
        {
            if (_path != null && showPath)
            {
                for (int i = _targetIndex; i < _path.Length; i++)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(_path[i], Vector3.one);

                    Gizmos.DrawLine(i == _targetIndex ? transform.position : _path[i - 1], _path[i]);
                }
            }
        }
    }
}