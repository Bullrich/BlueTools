using UnityEngine;
using System.Collections;

namespace Blue.Steering
{
    public class Flocking : Steering
    {
        [Range(3f, 20f)]
        public float
            neighborhoodRadius = 10f,
            separationRadius = 2,
            alignmentMult = 1f,
            cohesionMult = 1f,
            separationMult = 1f;

        public bool drawFlockingGizmos = false;

        Vector3 _alignment, _cohesion, _separation;

        private void FixedUpdate()
        {
            ResetForces();

            Collider[] hits = Physics.OverlapSphere(transform.position, neighborhoodRadius);

            Vector3 sumVelocity = Vector3.zero;
            Vector3 sumPosition = Vector3.zero;
            Vector3 sumSeparationForce = Vector3.zero;

            int nHits = 0;
            foreach (Collider hit in hits)
            {
                if (hit.gameObject == gameObject)
                    continue;

                Steering other = hit.GetComponent<Steering>();
                if (other == null)
                    continue;

                Vector3 deltaPos = transform.position - other.position;
                float distSqr = deltaPos.sqrMagnitude;
                if (distSqr > 0f && distSqr < separationRadius * separationRadius)
                    sumSeparationForce += deltaPos / distSqr;

                nHits++;
                sumVelocity += other.velocity;
                sumPosition += other.position;
            }

            if (nHits > 0)
            {
                _alignment = sumVelocity.normalized * maxVelocity - velocity;
                _cohesion = Seek(sumPosition / nHits);
                _separation = sumSeparationForce == Vector3.zero ? Vector3.zero :
                    sumSeparationForce.normalized * maxVelocity - velocity;

                AddForce(_alignment * alignmentMult);
                AddForce(_cohesion * cohesionMult);
                AddForce(_separation * separationMult);
            }
            AddForce(Seek(target.position));
            //AddForce(WanderRandomPos());

            ApplyForces();
        }

        int f = 0;
        override protected void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (!drawFlockingGizmos)
                return;

            if (++f % 50 == 0)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(transform.position, neighborhoodRadius);
            }

            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.position, transform.position + _alignment * alignmentMult);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + _cohesion * cohesionMult);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + _separation * separationMult);
            Gizmos.DrawWireSphere(transform.position, separationRadius);
        }
    }
}