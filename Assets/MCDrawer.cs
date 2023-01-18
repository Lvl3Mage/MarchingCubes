using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCDrawer : MonoBehaviour
{
	[SerializeField] Camera camera;
	[SerializeField] WorldChunkManager world;

	// [SerializeField] MarchingCubes cubes;
	[SerializeField] float range;
	[SerializeField] float castRadius = 1;
	[SerializeField] float changeAmount;
	[SerializeField] Collider collider;

	[SerializeField] GameObject IndicatorSpherePrefab;
	Transform indicatorSphere;
	void Start()
	{
		indicatorSphere = Instantiate(IndicatorSpherePrefab).transform;
	}


	void Update()
	{
		if(Input.GetMouseButton(0)){
			ModifyTerrain(changeAmount * Time.deltaTime, true);
		}
		if(Input.GetMouseButton(1)){
			ModifyTerrain(-changeAmount * Time.deltaTime, false);
		}
		DrawSphere();
	}
	void ModifyTerrain(float change, bool checkMinDistance){
		Ray ray = GetCameraRay();
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			ModifyPoint(hit.point, change, checkMinDistance);
		}
		else if(Physics.SphereCast(ray, castRadius, out hit)){
			ModifyPoint(hit.point, change, checkMinDistance);
		}
	}
	void ModifyPoint(Vector3 point, float change, bool checkMinDistance){
		if(checkMinDistance){
			Vector3 closestPointOnCol = collider.ClosestPointOnBounds(point);
			if((closestPointOnCol - point).sqrMagnitude <= range*2f){
				return;
			}
		}
		world.ModifyData(point, change, range);
	}
	void DrawSphere(){
		Ray ray = GetCameraRay();
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			// if((transform.position - hit.point).magnitude < minDistance){
			// 	return;
			// }
			indicatorSphere.localScale = Vector3.one*range;
			indicatorSphere.gameObject.SetActive(true);
			indicatorSphere.position = hit.point;
		}
		else if(Physics.SphereCast(ray, castRadius, out hit)){
			indicatorSphere.localScale = Vector3.one*range;
			indicatorSphere.gameObject.SetActive(true);
			indicatorSphere.position = hit.point;
		}
		else{
			indicatorSphere.gameObject.SetActive(false);

		}
	}
	Ray GetCameraRay(){
		Ray ray = camera.ScreenPointToRay(Input.mousePosition);
		// ray.origin += ray.direction*range;
		return ray;
	}
	// void ModifyData(Vector3 position, float change){
	// 	float[][][] data = cubes.GetData();

	// 	for(float i = position.x - range; i < position.x + range; i++){
	// 		int i_index = Mathf.RoundToInt(i);
	// 		if(i_index < 1 || i_index > data.Length-2){
	// 			continue;
	// 		}
			
	// 		for(float j = position.y - range; j < position.y + range; j++){
	// 			int j_index = Mathf.RoundToInt(j);
	// 			if(j_index < 1 || j_index > data[i_index].Length-2){
	// 				continue;
	// 			}
	// 			for(float k = position.z - range; k < position.z + range; k++){
	// 				int k_index = Mathf.RoundToInt(k);
	// 				if(k_index < 1 || k_index > data[i_index][j_index].Length-2){
	// 					continue;
	// 				}
	// 				float dist = (new Vector3(i_index, j_index, k_index) - position).magnitude;
	// 				if(dist > range){
	// 					continue;
	// 				}
	// 				// Debug.Log($"{i_index}, {j_index}, {k_index}");
	// 				// data[i_index][0][0] = 0;
	// 				data[i_index][j_index][k_index] += change*dist/range;
	// 				// data[i_index][j_index][k_index] = change;
	// 				data[i_index][j_index][k_index] = Mathf.Clamp01(data[i_index][j_index][k_index]);
	// 			}
	// 		}
	// 	}
	// 	cubes.UpdateMesh();
	// }
}
