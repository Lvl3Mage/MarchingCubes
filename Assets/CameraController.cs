using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[Header("Camera rotation settings")]
	[SerializeField] [Range(1f, 20f)] float sensitivity;
	[Header("Camera zoom settings")]
	[SerializeField] [Range(1f, 10f)] float zoomspeed;
	[SerializeField] [Range(1f, 20f)] float animzoomspeed;
	[SerializeField] [Range(40f, 100f)] float BaseZoom;
	[SerializeField] [Range(0.1f, 20f)] float ZoomedIn;
	float x,y;
	Camera Camera;
	bool zoomed, LookAround;
	// Start is called before the first frame update
	void Start()
	{
		//Cursor.visible = false;
		//Cursor.lockState = CursorLockMode.Locked;
		LookAround = true;
		Camera = GetComponent<Camera>();
	}

	public void SetLookAround(bool value){
		LookAround = value;
		if(value){
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
		else{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.Confined;
		}
	}
	// Update is called once per frame
	void Update()
	{
		if(LookAround){
			if(Input.GetKeyDown(KeyCode.Q)){
				Cursor.visible = !Cursor.visible;
				if(Cursor.lockState == CursorLockMode.Locked){
					Cursor.lockState = CursorLockMode.None;
				}
				else{
					Cursor.lockState = CursorLockMode.Locked;
				}
			}
			float rspeed = sensitivity*Camera.fieldOfView/BaseZoom;
			x += rspeed*Input.GetAxis("Mouse Y");
			y += rspeed*Input.GetAxis("Mouse X");
			if(!Cursor.visible){
				transform.localEulerAngles = new Vector3(-x, y, 0);
			}
			if(Input.GetKey(KeyCode.X)){
				if(!zoomed){
					Camera.fieldOfView = Interpolate(ZoomedIn);
					if(Camera.fieldOfView == ZoomedIn){
						zoomed = true;
					}
				}
				else{
					float zoomchange = Input.GetAxis("Mouse ScrollWheel")*zoomspeed;
					float view = Camera.fieldOfView;
					if((view-zoomchange)<1){
						Camera.fieldOfView = 1;
					}
					else if((view-zoomchange)>BaseZoom){
						Camera.fieldOfView = BaseZoom;
					}
					else{
						Camera.fieldOfView -= zoomchange; 
					}
				}
			}
			
			if(!Input.GetKey(KeyCode.X)){
				if(Camera.fieldOfView != BaseZoom){
					Camera.fieldOfView = Interpolate(BaseZoom);
				}
				
				else{
					zoomed = false;
				}
			}

		}
	}
	float Interpolate(float value){
		if(Mathf.Abs(value-Camera.fieldOfView)>0.5f){
			return Mathf.Lerp(Camera.fieldOfView, value, Time.deltaTime * animzoomspeed);
		}
		else{
			return value;
		}
		
	}
}
