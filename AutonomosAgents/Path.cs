using System.Collections;
using System.Collections.Generic;
using Blue.Utility;
using UnityEngine;

// by @Bullrich

namespace game {
	public class Path : MonoBehaviour
	{
		public Vector3[] path;
		public float radius = 1;
		public Vector3 start, end;

		private void Start()
		{
			start = path[0];
			end = path[1];
		}

		private void OnDrawGizmos()
		{
			//if (Application.isPlaying) return;
			Gizmos.color = Color.cyan;
			for (int i = 0; i < path.Length; i++)
			{
				Gizmos.DrawWireSphere(path[i], radius);
				if (i != 0)
				{

					Vector3 dir = path[i] - path[i-1];
					Vector3 left = Vector3.Cross(dir, Vector3.up).normalized;
					Vector3 right = Vector3.Cross(dir, Vector3.down).normalized;
					Gizmos.DrawLine(path[i] + left * radius, path[i-1] + left *radius);
					Gizmos.DrawLine(path[i]+right*radius, path[i-1]+right*radius);
				}
			}
		}
	}
}
