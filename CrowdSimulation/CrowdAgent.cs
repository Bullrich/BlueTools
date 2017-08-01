using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// by @Bullrich

namespace Blue.Crowd
{
	[RequireComponent(typeof(NavMeshAgent))]
	public class CrowdAgent : MonoBehaviour {
		public Transform target;

		private NavMeshAgent agent;

		private void Start(){
			agent = GetComponent<NavMeshAgent>();
			agent.speed = Random.Range(4, 5);

			agent.SetDestination(target.position);
		}

	}
}
