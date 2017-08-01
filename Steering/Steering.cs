using UnityEngine;
using System.Linq;
using System;
using Blue.Utility;

namespace Blue.Steering
{
    public abstract class Steering : MonoBehaviour, ISteerable
    {
        public Transform target;

        public float
            velocityLimit = 5f,
            forceLimit = 10f,
            arrivalRadius = 5f,
            pursuitPeriod = 2f,
            wanderDistanceAhead = 5f,
            wanderPeriod,
            wanderRandomStrength = 5f;

        private Vector3
            _velocity,
            _steerForce,
            _wander;

        private float _nextWander;

        public LayerMask _obstacleMask;

        public Vector3 position { get { return transform.position; } }
        public Vector3 velocity { get { return _velocity; } }
        public float mass { get { return 1f; } }
        public float maxVelocity { get { return velocityLimit; } }

        protected Vector3 Seek(Vector3 targetPosition)
        {
            Vector3 deltaPos = targetPosition - transform.position;
            Vector3 desiredVel = deltaPos.normalized * maxVelocity;
            return desiredVel - _velocity;
        }

        protected Vector3 Flee(Vector3 targetPosition)
        {
            Vector3 deltaPos = targetPosition - transform.position;
            Vector3 desiredVel = -deltaPos.normalized * maxVelocity; //Opposite of seek
            return desiredVel - _velocity;
        }

        protected Vector3 Arrival(Vector3 targetPosition, float arrivalRadius)
        {
            Vector3 deltaPos = targetPosition - transform.position;
            float distance = deltaPos.magnitude;
            Vector3 desiredVelocity;
            if (distance < arrivalRadius)
                desiredVelocity = deltaPos * maxVelocity / arrivalRadius;
            else
                desiredVelocity = deltaPos / distance * maxVelocity;
            return desiredVelocity - _velocity;
        }

        protected Vector3 ArrivalOptimized(Vector3 targetPosition, float arrivalRadius)
        {
            Vector3 deltaPos = targetPosition - transform.position;
            Vector3 desiredVel = VectorUtils.Truncate(deltaPos * maxVelocity / arrivalRadius, maxVelocity);
            return desiredVel - _velocity;
        }

        protected Vector3 WanderRandomPos()
        {
            wanderDistanceAhead = 0f;

            if (Time.time > _nextWander)
            {
                _nextWander = Time.time + wanderPeriod;
                _wander = VectorUtils.RandomDirection() * wanderRandomStrength;
            }
            return Seek(_wander);
        }

        protected Vector3 WanderTwitchy()
        {
            Vector3 desiredVelocity = VectorUtils.RandomDirection() * maxVelocity;
            return desiredVelocity - _velocity;
        }

        protected Vector3 WanderWithState(float distanceAhead, float randomRadius, float randomStrength)
        {
            _wander = VectorUtils.Truncate(_wander + VectorUtils.RandomDirection() * randomStrength, randomRadius);
            Vector3 aheadPosition = transform.position + _velocity.normalized * distanceAhead + _wander;
            return Seek(aheadPosition);
        }

        protected Vector3 WanderWithStatedTimed(float distanceahead, float randomRadius, float randomStrenth)
        {
            if (Time.time > _nextWander)
            {
                _nextWander = Time.time + wanderPeriod;
                _wander = VectorUtils.Truncate(_wander + VectorUtils.RandomDirection() * randomStrenth, randomRadius);
            }
            Vector3 aheadPosition = transform.position + _velocity.normalized * distanceahead + _wander;
            return Seek(aheadPosition);
        }

        ///<summary>Seek a future proyection</summary>
        protected Vector3 Pursuit(ISteerable who, float periodAhead)
        {
            Vector3 deltaPos = who.position - transform.position;
            Vector3 targetPosition = who.position + who.velocity * deltaPos.magnitude / who.maxVelocity;
            return Seek(targetPosition);
        }

        ///<summary>Evades a future proyection</summary>
        protected Vector3 Evade(ISteerable who, float periodAhead)
        {
            var deltaPos = who.position - transform.position;
            var targetPosition = who.position + who.velocity * deltaPos.magnitude / who.maxVelocity;
            return Flee(targetPosition);
        }

        protected void ResetForces()
        {
            _steerForce = Vector3.zero;
        }

        protected void AddForce(Vector3 force)
        {
            _steerForce += force;
        }

        protected void ApplyForces()
        {
            //Euler integration
            var dt = Time.fixedDeltaTime;
            _steerForce.y = 0f;
            _steerForce = VectorUtils.Truncate(_steerForce, forceLimit);
            _velocity = VectorUtils.Truncate(_velocity + _steerForce * dt, maxVelocity);
            transform.position += _velocity * dt;
            transform.forward = Vector3.Slerp(transform.forward, _velocity, 0.1f);
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + _velocity);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + _velocity, transform.position + _velocity + _steerForce);
        }

    }
}