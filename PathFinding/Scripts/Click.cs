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
        private void CheckClick()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    unit.NavigateToPoint(unit.transform.position, hit.point);
                }
            }
        }

        public void CheckHitBetweenPoints(Vector3 start, Vector3 end, LayerMask mask)
        {
            if (Physics.Raycast(start, end, Vector3.Distance(start, end), mask))
                Debug.DrawLine(start, end, Color.red, 2, false);
            else
                Debug.DrawLine(start, end, Color.blue, 2, false);
        }

        private void Update()
        {
            CheckClick();
        }
    }
}
