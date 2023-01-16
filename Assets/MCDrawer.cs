using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCDrawer : MonoBehaviour
{
	[SerializeField] Camera camera;
	[SerializeField] MarchingCubes cubes;
	[SerializeField] float range;
	[SerializeField] float changeAmount;
	void Start()
	{
		
	}


	void Update()
	{
		if(Input.GetMouseButton(0)){
			Ray ray = camera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				Vector3 hitPos = hit.point;
				Vector3 localPos = cubes.WorldPosToDataPos(hitPos);
				ModifyData(localPos, changeAmount * Time.deltaTime);
			}
		}
		if(Input.GetMouseButton(1)){
			Ray ray = camera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				Vector3 hitPos = hit.point;
				Vector3 localPos = cubes.WorldPosToDataPos(hitPos);
				ModifyData(localPos, -changeAmount * Time.deltaTime);
			}
		}
	}
	void ModifyData(Vector3 position, float change){
		float[][][] data = cubes.GetData();

		for(float i = position.x - range; i < position.x + range; i++){
			int i_index = Mathf.RoundToInt(i);
			if(i_index < 1 || i_index > data.Length-2){
				continue;
			}
			
			for(float j = position.y - range; j < position.y + range; j++){
				int j_index = Mathf.RoundToInt(j);
				if(j_index < 1 || j_index > data[i_index].Length-2){
					continue;
				}
				for(float k = position.z - range; k < position.z + range; k++){
					int k_index = Mathf.RoundToInt(k);
					if(k_index < 1 || k_index > data[i_index][j_index].Length-2){
						continue;
					}
					float dist = (new Vector3(i_index, j_index, k_index) - position).magnitude;
					if(dist > range){
						continue;
					}
					// Debug.Log($"{i_index}, {j_index}, {k_index}");
					// data[i_index][0][0] = 0;
					data[i_index][j_index][k_index] += change*dist/range;
					// data[i_index][j_index][k_index] = change;
					data[i_index][j_index][k_index] = Mathf.Clamp01(data[i_index][j_index][k_index]);
				}
			}
		}
		cubes.UpdateMesh();
	}
}
