using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// by @Bullrich

namespace Blue.Fov {
    public class FieldOfView : MonoBehaviour {

        public float viewRadius;
        [Range(0, 360)]
        public float viewAngle;

        public LayerMask targetMask, obstacleMask;

        List<Transform> visibleTargets = new List<Transform>();

        bool canHaveFovActive = false;

        public void ContinueFOV() {
            if (canHaveFovActive)
                StopCoroutine(FindTargetsWithDelay(.2f));
            canHaveFovActive = true;
            StartCoroutine(FindTargetsWithDelay(.2f));
        }

        public void StopFOV() {
            canHaveFovActive = false;
            StopCoroutine(FindTargetsWithDelay(.2f));
        }

        public bool hasTargetInView() {
            return visibleTargets.Count > 0;
        }

        public Transform getTarget() {
            if (visibleTargets.Count > 0)
                return visibleTargets[0];
            else
                return null;
        }

        IEnumerator FindTargetsWithDelay(float delay) {
            while (canHaveFovActive) {
                yield return new WaitForSeconds(delay);
                FindTarget();
            }
        }

        void FindTarget() {
            visibleTargets.Clear();
            Collider[] targetInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
            Vector3 myPos = transform.position;

            foreach (Collider targetInView in targetInViewRadius) {
                Transform target = targetInView.transform;

                Vector3 dirToTarget = (target.position - myPos).normalized;
                if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2) {
                    float dstToTarget = Vector3.Distance(transform.position, target.position);

                    if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                        visibleTargets.Add(target);
                }
            }
        }

        private void OnDrawGizmosSelected() {
            if (canHaveFovActive) {
                Vector3 myPos = transform.position;
                const float gridDetail = 20f;

                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(myPos, viewRadius);
                Gizmos.color = Color.blue;
                Vector3 lastW = Vector3.zero;
                foreach (Transform target in visibleTargets) {
                    Gizmos.DrawLine(transform.position, target.position);
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(target.position, 1f);
                }
                Gizmos.color = Color.cyan;
                for (float a = 0; a <= 360f; a += gridDetail) {
                    var v = new Vector3(
                        0f,
                        Mathf.Sin(Mathf.Deg2Rad * viewAngle / 2f) * viewRadius,
                        Mathf.Cos(Mathf.Deg2Rad * viewAngle / 2f) * viewRadius
                    );
                    var w = transform.rotation * Quaternion.AngleAxis(a, Vector3.forward) * v;
                    Gizmos.DrawLine(myPos, myPos + w);
                    Gizmos.DrawLine(myPos + lastW, myPos + w);
                    lastW = w;
                }
            }
        }
    }
}