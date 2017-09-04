using UnityEngine;

namespace Blue.Pathfinding
{
    public class Navigation
    {
        private Vector3[] _path;

        private int _targetIndex;
        private bool _hasRequestedPath = false;
        private Vector3 _waypoint, _targetPosition;
        private readonly Transform _tran;

        public Navigation(Transform tran)
        {
            _tran = tran;
        }

        private void OnPathFound(Vector3[] newPath)
        {
            _hasRequestedPath = false;

            if (newPath.Length <= 0) return;
            
            _path = newPath;
            _targetIndex = 0;
            _waypoint = _path[0];
            _targetPosition = _path[_path.Length - 1];
            // While following the path, adds a verification and makes sure that, if the grid change, it's path should be updated
            PathRequestManager.SuscribeToChange(SuscriptionToChange);
        }

        private void SuscriptionToChange()
        {
            RequestPathToPoint(_tran.position, _targetPosition);
        }

        public Vector3 GetCurrentWaypoint(Vector3 position)
        {
            //CORREGI ESTO!!!!!!!!!!!!!!!!!!!!!!
            if(_path == null)
                return Vector3.zero;
            
            float distance = Vector3.Distance(position, _path[_targetIndex]);
            
            if (distance > .5f) return _path[_targetIndex];
            
            _targetIndex++;
            if (_targetIndex < _path.Length) return _path[_targetIndex];
            
            _path = null;
            // When it reaches it's target, it unsuscribe from the grid update.
            PathRequestManager.RemoveFromChange(SuscriptionToChange);
            return Vector3.zero;
        }

        /// <summary>Request a path to a point in the map evading all the unwalkable objects</summary>
        public void RequestPathToPoint(Vector3 pathStart, Vector3 pathEnd)
        {
            if (_hasRequestedPath || pathEnd == Vector3.zero) return;
            
            PathRequestManager.RequestPath(pathStart, pathEnd, OnPathFound);
            _hasRequestedPath = true;
        }

        ///<summary>Call this method inside the OnDrawGizmos method</summary>
        public void DrawPath(Transform tran)
        {
            if (_path == null) return;

            for (int i = _targetIndex; i < _path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(_path[i], Vector3.one);
                
                Gizmos.DrawLine(i == _targetIndex ? tran.position : _path[i-1],_path[i]);
            }
        }
    }
}