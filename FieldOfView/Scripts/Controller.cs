using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// by @Bullrich

namespace Blue.Fov
{
    [RequireComponent(typeof(Rigidbody))]
	public class Controller : MonoBehaviour {

        public float moveSpeed = 6;

        Rigidbody rb;
        Camera viewCamera;
        Vector3 velocity;

		void Start () {
            rb = GetComponent<Rigidbody>();
            viewCamera = Camera.main;
            GetComponent<FieldOfView>().ContinueFOV();
		}

        private void Update() {
            Vector3 mousePosition = viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));
            transform.LookAt(mousePosition + Vector3.up * transform.position.y);
            velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * moveSpeed;
        }

        private void FixedUpdate() {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }
    }
}
