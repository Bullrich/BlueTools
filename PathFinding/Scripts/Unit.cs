using UnityEngine;
using System.Collections;

namespace Blue.Pathfinding
{
    public class Unit : MonoBehaviour
    {
        public float speed = 20;
        Vector3[] path;

        int targetIndex;

        private bool hasRequestedPath = false;

        protected Vector3 waypoint, targetPosition;

        protected void OnPathFound(Vector3[] newPath, bool pathSuccessful)
        {
            hasRequestedPath = false;
            if (pathSuccessful)
            {
                path = newPath;
                targetIndex = 0;
                waypoint = path[0];
                targetPosition = path[path.Length - 1];
                PathRequestManager.SuscribeToChange(SuscriptionToChange);
            }
        }

        private void SuscriptionToChange()
        {
            NavigateToPoint(transform.position, targetPosition);
        }

        private void MoveThroughPath(ref Vector3 currentWaypoint)
        {
            float distance = Vector3.Distance(transform.position, currentWaypoint);
            if (distance < .5f)
            {
                targetIndex++;
                if (targetIndex >= path.Length)
                {
                    path = null;
                    PathRequestManager.RemoveFromChange(SuscriptionToChange);
                    return;
                }
                currentWaypoint = path[targetIndex];
            }
            MoveTo(currentWaypoint);
        }

        public virtual void Update()
        {
            if (path != null)
                MoveThroughPath(ref waypoint);
        }

        public virtual void MoveTo(Vector3 targetPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        }

        /// <summary>Navigate to a point in the map evading all the unwalkable objects</summary>
        public void NavigateToPoint(Vector3 pathStart, Vector3 pathEnd)
        {
            if (!hasRequestedPath)
            {
                PathRequestManager.RequestPath(pathStart, pathEnd, OnPathFound);
                hasRequestedPath = true;
            }
        }

        public void OnDrawGizmos()
        {
            if (path != null)
            {
                for (int i = targetIndex; i < path.Length; i++)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(path[i], Vector3.one);

                    if (i == targetIndex)
                        Gizmos.DrawLine(transform.position, path[i]);
                    else
                        Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}