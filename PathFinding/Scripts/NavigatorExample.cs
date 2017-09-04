using System.Collections;
using System.Collections.Generic;
using Blue.Pathfinding;
using UnityEngine;

public class NavigatorExample : MonoBehaviour {
	private Navigation _navigation;
	public float speed = 15f;
	public bool showPath;

	// Use this for initialization
	void Start () {
		_navigation = new Navigation(transform);
	}
	
	// Update is called once per frame
	void Update ()
	{
		Vector3 waypoint = _navigation.GetCurrentWaypoint(transform.position);

		if (waypoint == Vector3.zero) return;

		transform.position = Vector3.MoveTowards(transform.position, waypoint, speed * Time.deltaTime);
	}

	public void GoToPoint(Vector3 position)
	{
		_navigation.RequestPathToPoint(transform.position,position);
	}

	private void OnDrawGizmos()
	{
		if(showPath && _navigation != null)
			_navigation.DrawPath(transform);
	}
}
