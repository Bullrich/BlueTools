using UnityEngine;
using System.Collections;

namespace Blue.Pathfinding
{
    public class Unit : MonoBehaviour
    {
        float speed = 20;
        Vector3[] path;
        int targetIndex;

        private bool hasRequestedPath = false;

        public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
        {
            hasRequestedPath = false;
            if (pathSuccessful)
            {
                StopCoroutine(FollowPath());
                path = newPath;
                targetIndex = 0;
                StartCoroutine(FollowPath());
            }
        }

        IEnumerator FollowPath()
        {
            Vector3 currentWaypoint = path[0];
            while (true)
            {
                float distance = Vector3.Distance(transform.position, currentWaypoint);
                if (distance < .5f)
                {
                    targetIndex++;
                    if (targetIndex >= path.Length)
                    {
                        yield break;
                    }
                    currentWaypoint = path[targetIndex];
                }

                MoveTo(currentWaypoint);
                yield return null;
            }
        }

        public void MoveTo(Vector3 targetPos){
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        }

        /// <summary>Navigate to a point in the map evading all the unwalkable objects</summary>
        public void NavigateToPoint(Vector3 pathStart, Vector3 pathEnd)
        {
            if(!hasRequestedPath){
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