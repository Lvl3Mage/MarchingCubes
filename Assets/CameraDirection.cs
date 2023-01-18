using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDirection : MonoBehaviour
{
	[SerializeField] Transform Camera;
    void Update()
    {
        transform.eulerAngles = new Vector3(0,Camera.transform.eulerAngles.y,0);
    }
}
