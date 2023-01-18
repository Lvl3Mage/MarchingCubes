using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsController : MonoBehaviour
{
	[SerializeField] Transform center;
	// [SerializeField] int HSections = 7;
	// [SerializeField] int VSections = 7;
	// [SerializeField] float maxGroundDistance = 1f;
	[SerializeField] Rigidbody rb;
	[SerializeField] Transform camera;
	[SerializeField] float speed, jumpSpeed;
	void Start()
	{
		
	}


	void Update()
	{
		// Vector3 normal = ClosestGround();
		Vector2 input = new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));
		// if(Vector3.Angle(normal.normalized, Vector3.up) > 45f || normal == Vector3.up)
		// {
			rb.velocity = input.x*camera.right*speed + rb.velocity.y*Vector3.up + input.y*camera.forward*speed;
		// }
		// else{
		// 	Vector3 XDir = Vector3.ProjectOnPlane(Vector3.right, new Vector3(normal.x, normal.y, 0));
		// 	Vector3 ZDir = Vector3.ProjectOnPlane(Vector3.forward, new Vector3(0, normal.y, normal.z));
		// 	// Debug.DrawLine(transform.position, transform.position + XDir, Color.green);
		// 	// Debug.DrawLine(transform.position, transform.position + ZDir, Color.green);

			
		// 	rb.velocity = (input.x*Vector3.Dot(camera.right,XDir)*XDir + input.y*Vector3.Dot(camera.forward,ZDir)*ZDir*groundSpeed)*groundSpeed;

		// }
		if(Input.GetKeyDown(KeyCode.Space)){
			RaycastHit hit;
			if (Physics.Raycast(center.position, Vector3.down, out hit, 2)){
	// 				float deltaSqrMag = (hit.point - center.position).sqrMagnitude;
	// 				if(deltaSqrMag < minDistSqr){
	// 					minDistSqr = deltaSqrMag;
	// 					closestNormal = hit.normal;
	// 				}
	// 			}
				rb.velocity += Vector3.up*jumpSpeed;
			}
			
		}




	}
	// Vector3 ClosestGround(){
	// 	Vector3 closestNormal = Vector3.up;
	// 	float minDistSqr = Mathf.Infinity;
	// 	for(float Hangle = 0; Hangle < 2*Mathf.PI; Hangle += Mathf.PI/HSections){
	// 		Vector3 basedir = new Vector3(Mathf.Cos(Hangle), -1, Mathf.Sin(Hangle));
	// 		for(float Vangle = 0; Vangle < (Mathf.PI/2); Vangle += (Mathf.PI/2)/VSections){
	// 			Vector3 dir = new Vector3(basedir.x*Mathf.Cos(Vangle), basedir.y, basedir.z*Mathf.Cos(Vangle));
	// 			// dir = dir.normalized;
	// 			RaycastHit hit;
	// 			if (Physics.Raycast(center.position, dir, out hit, maxGroundDistance)){
	// 				float deltaSqrMag = (hit.point - center.position).sqrMagnitude;
	// 				if(deltaSqrMag < minDistSqr){
	// 					minDistSqr = deltaSqrMag;
	// 					closestNormal = hit.normal;
	// 				}
	// 			}
	// 			// Debug.DrawLine(center.position, center.position + dir, Color.green);
	// 		}
	// 	}
	// 	return closestNormal;
	// }
	// void OnTriggerStay(Collider collider){
	// 	if(Input.GetKeyDown(KeyCode.Space)){
	// 		rb.velocity += Vector3.up*jumpSpeed;
	// 	}
	// }
}
