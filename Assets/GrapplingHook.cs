using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
	[SerializeField] Transform gunPoint;
	
	Joint joint;
	void Start()
	{
		
	}


	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Space)){
			AddHook();
		}
		else if(Input.GetKeyUp(KeyCode.Space)){
			RemoveHook();
		}
	}
	void AddHook(){

	}
	void RemoveHook(){

	}
}
