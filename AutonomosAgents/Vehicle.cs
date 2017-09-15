using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Blue.Utility;
using UnityEditorInternal;
using UnityEngine;

// By @Bullrich
namespace game
{
	[RequireComponent(typeof(Rigidbody))]
	public class Vehicle : MonoBehaviour
	{
		private Rigidbody rb;
		private Vector3 location, velocity, acceleration;

		public float maxSpeed, maxForce;

		public Transform currentTarget;
		public FlowField flow;
		public Path path;

		private void Start()
		{
			rb = GetComponent<Rigidbody>();
		}
		
		public enum behavior { seekTarget, FollowPath, Arrive}

		public behavior Beavhior;

		private void Update()
		{
			location = transform.position;
			velocity = rb.velocity;
			Behave();
		}

		private void Behave()
		{
			switch (Beavhior)
			{
				case behavior.seekTarget:
					Seek(currentTarget.position);
					break;
				case behavior.FollowPath:
					FollowPath(path);
					break;
				case behavior.Arrive:
					Arrive(flow.getDirection(transform.position));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void Seek(Vector3 target)
		{
			Vector3 desired = target - location;
			Vector3 normalDesired = desired.normalized * maxSpeed;
			Vector3 steer = normalDesired - velocity;

			Vector3 subSteer = Vector3.ClampMagnitude(steer, maxForce);

			applyForce(subSteer);
		}

		private void Arrive(Vector3 target)
		{
			Vector3 desired = target - location;
			float d = desired.magnitude;
			desired = desired.normalized;
			if (d < 100)
			{
				float m = Mathf.Clamp(d, 0, maxSpeed);
				desired *= m;
			}
			else
			{
				desired *= maxSpeed;
			}

			Vector3 steer = desired - velocity;

			Vector3 subSteer = Vector3.ClampMagnitude(steer, maxForce);
			applyForce(subSteer);
		}

		private void StayWithinWalls()
		{
			if (location.x > 25)
			{
				Vector3 desired = new Vector3(maxSpeed, velocity.y);

				Vector3 steer = desired - velocity;
				applyForce(steer);
			}
		}

		void FollowPath(Path p)
		{
			Vector3 predict = velocity;
			predict = predict.normalized * 3;
			Vector3 predictLoc = transform.position + predict;
			DrawCross(predictLoc, Color.cyan);


			Vector3 a = p.start;
			Vector3 b = p.end;
			Vector3 normalPoint = GetNormalPoint(predictLoc, a, b);
			DrawCross(normalPoint, Color.blue);

			Vector3 dir = b - a;
			dir = dir.normalized * 3;
			Vector3 target = normalPoint + dir;
			DrawCross(target, Color.green);

			float distance = Vector3.Distance(normalPoint, predictLoc);
			print(string.Format("target: {0}, normal: {1}, dir: {2}", target.x + "|" + target.y,normalPoint.x + "|" + normalPoint.y,dir.x + "|" + dir.y));
			if (distance > p.radius)
				Seek(target);
		}

		private Vector3 PredictLocation(Rigidbody rb)
		{
			Vector3 predict = rb.velocity;
			predict = predict.normalized * 25;
			Vector3 predictLoc = rb.position + predict;
			return predictLoc;
		}

		private void DrawCross(Vector3 position, Color _color)
		{
			float lineSize = 0.5f;
			Debug.DrawLine(position + new Vector3(lineSize, 0f, 0f), position - new Vector3(lineSize, 0f, 0f), _color);
			Debug.DrawLine(position + new Vector3(0f, 0f, lineSize), position - new Vector3(0f, 0f, lineSize), _color);
		}

		private Vector3 GetNormalPoint(Vector3 predict, Vector3 a, Vector3 b)
		{
			Vector3 ap = predict - a;
			Vector3 ab = b - a;

			ab = ab.normalized;
			float dota = Vector3.Dot(ap, ab);
			ab = ab * dota;
			Vector3 normalPoint = a + ab;
			return normalPoint;
		}

		private void applyForce(Vector3 force)
		{
			// acceleration = force;
			rb.AddForce(force, ForceMode.Acceleration);
			DrawArrow.ForDebug(transform.position, force, Color.blue);
		}
	}
}
