using UnityEngine;

// by @Bullrich

namespace Blue.Waypoints
{
    [RequireComponent(typeof(CharacterController))]
    public class StateMachine : MonoBehaviour
    {
        WaypointWalker walker = new WaypointWalker();
        public float speed = 5f, pauseTime, rotationTime;
        public bool shouldPause, shouldSmooth;
        float deltaTime, _time;
        CharacterController controller;

        enum States
        {
            moving, pause, rotating
        }
        States currentState = States.moving;

        public Waypoint firstWp;

        private void Awake()
        {
            Init(firstWp);
        }

        public void Init(Waypoint firstWaypoint)
        {
            walker.Reset(transform, speed, firstWaypoint);
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            deltaTime = Time.deltaTime;
            STM(currentState, ref _time);
        }

        private void STM(States state, ref float _time)
        {
            switch (state)
            {
                case States.moving:
                    walker.MoveToDirection(shouldSmooth);
                    if (!shouldSmooth && walker.closeToWaypoint())
                    {
                        currentState = (shouldPause ? States.pause : States.rotating);
                        walker.SetRotation();
                    }
                    break;
                case States.pause:
                    _time += deltaTime;
                    if (_time >= pauseTime)
                    {
                        currentState = States.rotating;
                        _time = 0;
                    }
                    break;
                case States.rotating:
                    _time = Mathf.Min(rotationTime, _time + deltaTime);
                    walker.Rotate(_time, rotationTime);

                    if (_time >= rotationTime)
                    {
                        currentState = States.moving;
                        _time = 0;
                    }
                    break;
            }
        }
    }
}
