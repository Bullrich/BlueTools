using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Blue.Pathfinding;

// by @Bullrich

namespace Blue.Pathfinding
{
    public class Click : MonoBehaviour
    {
        public Unit unit;
        public NavigatorExample navigator;
        private void CheckClick()
        {
            if (Input.GetMouseButtonDown(0)) 
                unit.NavigateToPoint(unit.transform.position, GetClickPoint());
            else if (Input.GetMouseButtonDown(1))
                navigator.GoToPoint(GetClickPoint());
        }

        public void CheckHitBetweenPoints(Vector3 start, Vector3 end, LayerMask mask)
        {
            Debug.DrawLine(start, end,
                Physics.Raycast(start, end, Vector3.Distance(start, end), mask) ? Color.red : Color.blue, 2, false);
        }

        private void Update()
        {
            CheckClick();
        }

        private Vector3 GetClickPoint()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return !Physics.Raycast(ray, out hit) ? Vector3.zero : hit.point;
        }
    }
}
