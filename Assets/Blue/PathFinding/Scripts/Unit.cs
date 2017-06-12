using UnityEngine;
using System.Collections;

namespace Blue.Pathfinding
{
    public class Unit : MonoBehaviour
    {
        float speed = 20;
        Vector3[] path;
        int targetIndex;

        Animator animator;

        void Start()
        {
            ///PathRequestManager.RequestPath(transform.position,target.position, OnPathFound);
            animator = GetComponent<Animator>();
        }

        public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
        {
            if (pathSuccessful)
            {
                path = newPath;
                targetIndex = 0;
                StopCoroutine("FollowPath");
                StartCoroutine("FollowPath");
            }
        }

        IEnumerator FollowPath()
        {
            Vector3 currentWaypoint = path[0];
            while (true)
            {
                if (transform.position == currentWaypoint)
                {
                    targetIndex++;
                    if (targetIndex >= path.Length)
                    {
                        yield break;
                    }
                    currentWaypoint = path[targetIndex];
                }

                transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
                animator.SetBool("travelling", true);
                yield return null;
                animator.SetBool("travelling", false);
            }
        }

        /// <summary>Navigate to a point in the map evading all the unwalkable objects</summary>
        public void NavigateToPoint(Vector3 pathStart, Vector3 pathEnd)
        {
            PathRequestManager.RequestPath(pathStart, pathEnd, OnPathFound);
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
                    {
                        Gizmos.DrawLine(transform.position, path[i]);
                    }
                    else
                    {
                        Gizmos.DrawLine(path[i - 1], path[i]);
                    }
                }
            }
        }
    }
}