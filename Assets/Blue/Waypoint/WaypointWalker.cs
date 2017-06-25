using UnityEngine;

// by @Bullrich

namespace Blue.Waypoints {
    public class WaypointWalker {
        Waypoint selected, lastWaypoint;
        BezierManager bezier = new BezierManager();
        float speed;
        Quaternion currentRotation, nextRotation;
        Transform _tran;

        public void Reset(Transform tran, float moveSpeed, Waypoint firstWaypoint) {
            _tran = tran;
            selected = firstWaypoint;
            speed = moveSpeed;
            bezier.UpdateBezierParams(firstWaypoint, _tran.position, speed);
        }

        public Vector3 MoveToDirection(bool useSmooth) {
            Vector3 myPos = _tran.position;
            Vector3 waypointPos;
            if (useSmooth)
                if (!selected.IsNear(myPos))
                    waypointPos = targetPosition(selected);
                else
                    waypointPos = bezier.BezierMovement(selected.transform.position, speed, myPos);
            else
                waypointPos = targetPosition(selected);

            //_tran.forward = waypointPos;
            //_tran.position = _tran.position + (waypointPos * Time.deltaTime) * speed;
            //  controller.Move((waypointPos * Time.deltaTime) * speed);

            if (useSmooth && bezier._bezierT >= 1.0f && selected.Next != null) {
                SelectNextWaypoint();
                bezier.UpdateBezierParams(selected, lastWaypoint.transform.position, speed);
            }

            return waypointPos;
        }

        private bool SelectNextWaypoint() {
            lastWaypoint = selected;
            if (selected.Next != null) {
                selected = selected.Next;
                return true;
            }
            return false;
        }

        public bool closeToWaypoint() {
            bool isClose = (selected.transform.position - _tran.position).sqrMagnitude < 0.01f;
            if (isClose)
                SelectNextWaypoint();
            return isClose;
        }

        public void Rotate(float _time, float _rotationTime) {
            _tran.rotation = Quaternion.Lerp(currentRotation, nextRotation, _time / _rotationTime);
        }

        public void SetRotation() {
            currentRotation = Quaternion.LookRotation(_tran.forward, Vector3.up);
            nextRotation = Quaternion.LookRotation(selected.transform.position - _tran.position, Vector3.up);
        }

        Vector3 targetPosition(Waypoint current) {
            Vector3 toWayPoint = current.transform.position - _tran.position;
            Vector3 direction = toWayPoint.normalized;
            Vector3 movementDelta = direction;
            return movementDelta.sqrMagnitude > toWayPoint.sqrMagnitude ? toWayPoint : movementDelta;
        }

        private class BezierManager {
            public float _bezierT, _speed;
            public Vector3 _start, _end, _handle1, _handle2;

            public Vector3 BezierMovement(Vector3 waypointPosition, float speed, Vector3 pos) {
                _bezierT += Mathf.Min(1.0f, _speed * Time.deltaTime);

                _handle1 = Vector3.Lerp(_start, waypointPosition, _bezierT);
                _handle2 = Vector3.Lerp(waypointPosition, _end, _bezierT);
                Vector3 _bezierPoint = Vector3.Lerp(_handle1, _handle2, _bezierT);

                return _bezierPoint - pos;
            }

            public void UpdateBezierParams(Waypoint waypoint, Vector3 lastPosition, float movementSpeed) {
                Vector3 currWaypoint = waypoint.transform.position;
                _bezierT = 0;
                _start = currWaypoint + (lastPosition - currWaypoint).normalized * waypoint.NearDistance;

                if (waypoint.Next == null) {
                    _end = currWaypoint;
                    _speed = movementSpeed / waypoint.NearDistance;
                } else {
                    _end = currWaypoint + (waypoint.Next.transform.position - currWaypoint).normalized * waypoint.NearDistance;
                    _speed = movementSpeed / waypoint.NearDistance * 0.5f;
                }
            }
        }
    }
}
