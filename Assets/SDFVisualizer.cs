using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDFVisualizer : MonoBehaviour
{
	[SerializeField] CubeMarcher cubeMarcher;
	SDFGenerator[] Fields = new SDFGenerator[]
	{
		new SphereSDF(Vector3.zero, 10f),
		new SphereSDF(Vector3.up*5 + Vector3.left*5, 5f),
		new BoxFrameSDF(Vector3.zero, Vector3.one*10, 2),
		new BoxSDF(-Vector3.up*4.5f + -Vector3.left*4.5f, Vector3.one*3),
		
		new BoxSDF(Vector3.zero, Vector3.one*8),
		new BoxFrameSDF(Vector3.zero, Vector3.one*5.5f, 2),
		new SphereSDF(-Vector3.up*5 - Vector3.left*3, 4f),
		new SphereSDF(Vector3.zero, 10f),
		new TorusSDF(Vector3.zero, 10, 3),
		new BoxSDF(Vector3.zero, Vector3.one*12),
		new HollowSphereSDF(Vector3.zero, 10, 0, 2),
		new BoxSDF(Vector3.up*-5, new Vector3(12,7,12)),
	};
	void Start(){
		StartCoroutine(SDFAnimation(Fields));
		// float[][][] sdf = GenerateSDF(Fields[1], 15, 1);
		// cubeMarcher.March(sdf, 0, 1);
	}
	float[][][] GenerateSDF(SDFGenerator sdfGen, int size, float scale){
		float[][][] sdf = new float[size][][];
		float halfSize = size/2;
		for(int i = 0; i < size; i++){
			float[][] slice = new float[size][];
			for(int j = 0; j < size; j++){
				float[] row = new float[size];
				for(int k = 0; k < size; k++){
					row[k] = -sdfGen.GetSDFValue(new Vector3(i-halfSize,j-halfSize,k-halfSize)*scale);
				}
				slice[j] = row;
			}
			sdf[i] = slice;
		}
		return sdf;
	}
	IEnumerator SDFAnimation(SDFGenerator[] sdfGens){
		int curSDF = 0;
		int nextSDF = 1;

		float[][][][] sdfs = new float[sdfGens.Length][][][];
		for(int i = 0; i < sdfGens.Length; i++){
			sdfs[i] = GenerateSDF(sdfGens[i],35,1);
		}

		while(true){
			if(nextSDF > sdfs.Length-1){
				nextSDF = 0;
			}
			yield return AnimateSDFs(sdfs[curSDF],sdfs[nextSDF],1.5f);

			curSDF = nextSDF;
			nextSDF = curSDF+1;
		}

	}
	IEnumerator AnimateSDFs(float[][][] dataA, float[][][] dataB, float timeTotal){
		float timePassed = 0;
		while(timePassed < timeTotal){
			float a = 4;
			float t = timePassed / timeTotal;
			t = Mathf.Pow(t,a)/(Mathf.Pow(t,a)+ Mathf.Pow(1-t, a));
			cubeMarcher.March(LerpSDFs(dataA,dataB,t), 0, 1);
			yield return null;
			timePassed += Time.deltaTime;

		}
	}
	float[][][] LerpSDFs(float[][][] sdfA, float[][][] sdfB, float t){
		

		int size = sdfA.Length;
		float halfSize = size/2;

		float[][][] lerpSDF = new float[size][][];

		for(int i = 0; i < size; i++){
			float[][] slice = new float[size][];
			for(int j = 0; j < size; j++){
				float[] row = new float[size];
				for(int k = 0; k < size; k++){
					row[k] = Mathf.Lerp(sdfA[i][j][k], sdfB[i][j][k], t);
				}
				slice[j] = row;
			}
			lerpSDF[i] = slice;
		}
		return lerpSDF;
	}
	// void WriteSDF(float[][][] sdf){
	// 	int size = sdf.Length;
	// 	for(int i = 0; i < size; i++){
	// 		Debug.Log($"Face: {i}");
	// 		for(int j = 0; j < size; j++){
	// 			string row = "";
	// 			for(int k = 0; k < size; k++){
	// 				row += $"{sdf[i][j][k]}, ";
	// 			}
	// 			Debug.Log(row);
	// 		}
			
	// 	}
	// }
}
public interface SDFGenerator
{
	float GetSDFValue(Vector3 point);
}
public class SphereSDF : SDFGenerator
{
	Vector3 position;
	float radius;
	public SphereSDF(Vector3 _position, float _radius){
		position = _position;
		radius = _radius;

	}
	public float GetSDFValue(Vector3 point){
		point -= position;
		return point.magnitude - radius;
	}
}
public class BoxSDF : SDFGenerator
{
	Vector3 position;
	Vector3 dim;
	public BoxSDF(Vector3 _position, Vector3 _dim){
		position = _position;
		dim = _dim;

	}
	public float GetSDFValue(Vector3 point){
		point -= position;
		Vector3 q = new Vector3(Mathf.Abs(point.x),Mathf.Abs(point.y),Mathf.Abs(point.z)) - dim;
		return new Vector3(Mathf.Max(q.x,0.0f),Mathf.Max(q.y,0.0f),Mathf.Max(q.z,0.0f)).magnitude + Mathf.Min(Mathf.Max(q.x,Mathf.Max(q.y,q.z)),0.0f);
	}
}
public class BoxFrameSDF : SDFGenerator
{
	Vector3 position;
	Vector3 dim;
	float thickness;
	public BoxFrameSDF(Vector3 _position, Vector3 _dim, float _thickness){
		position = _position;
		dim = _dim;
		thickness = _thickness;
	}
	public float GetSDFValue(Vector3 point){
		point -= position;
		point = new Vector3(Mathf.Abs(point.x),Mathf.Abs(point.y),Mathf.Abs(point.z)) - dim;
		Vector3 q = new Vector3(Mathf.Abs(point.x + thickness) - thickness, Mathf.Abs(point.y + thickness) - thickness, Mathf.Abs(point.z + thickness) - thickness);
		return Mathf.Min(Mathf.Min(
			new Vector3(Mathf.Max(point.x,0.0f),Mathf.Max(q.y,0.0f),Mathf.Max(q.z,0.0f)).magnitude+Mathf.Min(Mathf.Max(point.x,Mathf.Max(q.y,q.z)),0.0f),
			new Vector3(Mathf.Max(q.x,0.0f),Mathf.Max(point.y,0.0f),Mathf.Max(q.z,0.0f)).magnitude+Mathf.Min(Mathf.Max(q.x,Mathf.Max(point.y,q.z)),0.0f)),
			new Vector3(Mathf.Max(q.x,0.0f),Mathf.Max(q.y,0.0f),Mathf.Max(point.z,0.0f)).magnitude+Mathf.Min(Mathf.Max(q.x,Mathf.Max(q.y,point.z)),0.0f));
	}
}
public class TorusSDF : SDFGenerator
{
	Vector3 position;
	float radius, thickness;
	public TorusSDF(Vector3 _position, float _radius, float _thickness){
		position = _position;
		radius = _radius;
		thickness = _thickness;
	}
	public float GetSDFValue(Vector3 point){
		point -= position;
		Vector2 q = new Vector2(new Vector2(point.x,point.z).magnitude-radius,point.y);
		return q.magnitude-thickness;
	}
}
public class HollowSphereSDF : SDFGenerator
{
	Vector3 position;
	float radius, h, t;
	public HollowSphereSDF(Vector3 _position, float _radius, float _h, float _t){
		position = _position;
		radius = _radius;
		h = _h;
		t = _t;
	}
	public float GetSDFValue(Vector3 point){
		point -= position;

		// sampling independent computations (only depend on shape)
		float w = Mathf.Sqrt(radius*radius-h*h);
	
		// sampling dependant computations
		Vector2 q = new Vector2( new Vector2(point.x,point.z).magnitude, point.y );
		return ((h*q.x<w*q.y) ? (q-new Vector2(w,h)).magnitude : Mathf.Abs(q.magnitude-radius) ) - t;
	}
}
