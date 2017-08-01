using System.Collections;
using UnityEngine;

// by @Bullrich

namespace Blue.Flock
{
    public class Flock : MonoBehaviour
    {
        public float
            minSpeed = 20f,
            turnSpeed = 20f,
            randomFreq = 20f,
            randomForce = 20f,
            // alignment variables
            toOriginForce = 50f,
            toOriginRange = 100f,
            gravity = 2f,
            //separation variables
            avoidanceRadius = 50f,
            avoidanceForce = 20f,
            // cohesion variables
            followVelocity = 4f,
            followRadius = 40f;

        // boid movement control
        private Transform origin;
        private Vector3
            velocity,
            noramlizedVelocity,
            randomPush,
            originPush;
        private Transform[] objects;
        private Flock[] otherFlocks;
        private Transform transformComponent;

        private void Start()
        {
            randomFreq = 1f / randomFreq;

            // Assign the parent as origin
            origin = transform.parent;

            // Flock transform
            transformComponent = transform;

            //Temporary components
            Component[] tempFlocks = null;

            // Get all the unity flock components from the parent
            if (transform.parent)
                tempFlocks = transform.parent.GetComponentsInChildren<Flock>();

            // Assign and store all the flock object in this group
            objects = new Transform[tempFlocks.Length];
            otherFlocks = new Flock[tempFlocks.Length];

            for (int i = 0; i < tempFlocks.Length; i++)
            {
                objects[i] = tempFlocks[i].transform;
                print("ho " + i);
                print((tempFlocks[i] as Flock).GetType());
                otherFlocks[i] = tempFlocks[i] as Flock;
            }

            // Null parent as the flock leader will be UFC object
            //transform.parent = null;

            // Calculate random push depends on random frequency
            StartCoroutine(UpdateRandom());
        }

        IEnumerator UpdateRandom()
        {
            while (true)
            {
                randomPush = Random.insideUnitSphere * randomForce;
                yield return new WaitForSeconds(randomFreq +
                    Random.Range(-randomFreq / 2f, randomFreq / 2f));
            }
        }

        private void Update()
        {
            // Internal variables

            float speed = velocity.magnitude;
            Vector3 avgVelocity = Vector3.zero;
            Vector3 avgPosition = Vector3.zero;
            float count = 0, f = 0f, d = 0f;
            Vector3 myPosition = transformComponent.position;
            Vector3 forceV, toAvg, wantedVel;

            for (int i = 0; i < objects.Length; i++)
            {
                Transform objTran = objects[i];
                if (objTran != transformComponent)
                {
                    Vector3 otherPosition = transform.position;

                    // Average position to calculate cohesion
                    avgPosition += otherPosition;
                    count++;

                    // Directional vector from the other flock to this flock
                    forceV = myPosition - otherPosition;

                    // Magnitude of that directional vector
                    d = forceV.magnitude;

                    // Add push value if the magnitude is less than followRadius to the leader
                    if (d < followRadius)
                    {
                        // calculate the velocity based on avoidance distance
                        if (d < avoidanceRadius)
                        {
                            f = 1f - (d / avoidanceRadius);
                            if (d > 0)
                                avgVelocity += (forceV / d) * f * avoidanceForce;
                        }

                        // keep current distance with the leader
                        f = d / followRadius;
                        Flock tempOtherFlock = otherFlocks[i];
                        // normalize to get movement direction to set new velocity
                        avgVelocity += tempOtherFlock.noramlizedVelocity * f * followVelocity;
                    }
                }
            }
            // linq no me sirvio en esta :(

            if (count > 0)
            {
                // calculate the average flock velocity
                avgVelocity /= count;

                // Calculate center value of the flock
                toAvg = (avgPosition / count) - myPosition;
            }
            else
                toAvg = Vector3.zero;

            // Directional vector to the leader
            forceV = origin.position - myPosition;
            d = forceV.magnitude; f = d / toOriginRange;

            // Calculate the velocity of the flock to the leader
            if (d > 0) // not center of the flock
                originPush = (forceV / d) * f * toOriginForce;

            if (speed < minSpeed && speed > 0)
            {
                velocity = (velocity / speed) * minSpeed;
            }

            wantedVel = velocity;

            //Calculate final velocity
            wantedVel -= wantedVel * Time.deltaTime;
            wantedVel += randomPush * Time.deltaTime;
            wantedVel += originPush * Time.deltaTime;
            wantedVel += avgVelocity * Time.deltaTime;
            wantedVel += toAvg.normalized * gravity * Time.deltaTime;

            // Final velocity to rotate the flock into
            velocity = Vector3.RotateTowards(velocity, wantedVel,
                turnSpeed * Time.deltaTime, 100f);

            transformComponent.rotation = Quaternion.LookRotation(velocity);

            // Move the flock based on the calculated velocity
            transformComponent.Translate(velocity * Time.deltaTime, Space.World);

            // normalise the velocity
            noramlizedVelocity = velocity.normalized;
        }
    }
}
