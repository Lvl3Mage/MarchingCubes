using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunkManager : MonoBehaviour
{
	public class ChunkData
	{
		public ChunkData(float[][][] _data){
			data = _data;
		}
		public float[][][] data;

	}
	Dictionary<(int,int,int), ChunkData> worldData = new Dictionary<(int,int,int), ChunkData>{};
	[Header("World Settings")]
	[SerializeField] int chunkGridSize = 5;
	
	[SerializeField] float chunkWorldSize = 5;
	[SerializeField] NoiseOctave2D heightmapNoise;
	[SerializeField] float heightmapNoiseAmplitude;
	[SerializeField] NoiseOctave2D[] heightmapDetail;
	[SerializeField] float heightmapDetailAmplitude;
	[SerializeField] NoiseOctave3D[] caveNoiseA;
	[SerializeField] NoiseOctave3D[] caveNoiseB;
	[SerializeField] NoiseOctave3D caveDetailNoise;
	[SerializeField] NoiseOctave3D caveDetailMask;
	[SerializeField] NoiseOctave3D[] caveSpaceNoise;
	[SerializeField] float caveDiameter;
	[SerializeField] NoiseOctave2D caveProximityNoise;
	[SerializeField] float ProximityHarshness = 2;
	[SerializeField] float maxCaveDepth = 5;
	// [SerializeField] float heightmapScale = 0.2f;
	// [SerializeField] float heightmapAmplitude = 1f;
	[SerializeField] float surfaceLevel = 0.1f;
	[SerializeField] int renderDistance = 5;
	[Header("Referece Settings")]
	[SerializeField] CubeMarcher chunkPrefab;
	[SerializeField] Transform trackedObject;
	Dictionary<(int,int,int), Chunk> loadedChunks = new Dictionary<(int,int,int), Chunk>{};
	[System.Serializable]
	class NoiseOctave2D
	{
		[SerializeField] float minValue;
		[SerializeField] float maxValue = 1;
		[SerializeField] Vector2 scale;
		[SerializeField] float power = 1;
		[SerializeField] Vector2 offset;
		public float ComputeNoise(Vector2 position){
			float noise = Mathf.Pow(Mathf.PerlinNoise(position.x*scale.x + offset.x,position.y*scale.y + offset.y),power);
			noise *= maxValue-minValue;
			noise += minValue;
			return noise;
		}
	}
	[System.Serializable]
	class NoiseOctave3D
	{
		[SerializeField] float minValue;
		[SerializeField] float maxValue = 1;
		[SerializeField] Vector3 scale;
		[SerializeField] float power = 1;
		[SerializeField] Vector3 offset;
		public float ComputeNoise(Vector3 position){
			float noise = Mathf.Pow(PerlinNoise3D(Vector3.Scale(position, scale)+ offset),power);
			noise *= maxValue-minValue;
			noise += minValue;
			return noise;
		}
		float PerlinNoise3D(Vector3 sample){
			float XY = Mathf.PerlinNoise(sample.x, sample.y);
			float YZ = Mathf.PerlinNoise(sample.y, sample.z);
			float ZX = Mathf.PerlinNoise(sample.z, sample.x);
			
			float YX = Mathf.PerlinNoise(sample.y, sample.z);
			float ZY = Mathf.PerlinNoise(sample.z, sample.y);
			float XZ = Mathf.PerlinNoise(sample.x, sample.z);
			return (XY + YZ + ZX + YX + ZY + XZ)/6f;
		}
	}
	public class Chunk
	{
		public Chunk(Vector3 position, ChunkData _chunkData, CubeMarcher chunkPrefab, float _surfaceLevel, float _scale){
			//Creates chunk at position
			chunkData = _chunkData;
			surfaceLevel = _surfaceLevel;
			scale = _scale;


			cubeMarcher = Instantiate(chunkPrefab, position, Quaternion.identity);
			cubeMarcher.March(chunkData.data, surfaceLevel, scale);
		}
		public void UpdateChunk(){
			cubeMarcher.March(chunkData.data, surfaceLevel, scale);
		}
		public void UnloadChunk(){
			Destroy(cubeMarcher.gameObject);
		}
		float surfaceLevel, scale;
		CubeMarcher cubeMarcher;
		ChunkData chunkData;
	}
	ChunkData GenerateChunkData((int,int,int) chunkID){
		Vector3 chunkPositionSeed = ChunkIdToGridPos(chunkID);
		float[][][] data = new float[chunkGridSize+1][][];
		for (int i = 0; i < chunkGridSize+1; i++){
			data[i] = new float[chunkGridSize+1][];
			for(int j = 0; j < chunkGridSize+1; j++){
				data[i][j] = new float[chunkGridSize+1];
				for(int k = 0; k < chunkGridSize+1; k++){
					Vector3 samplePoint = new Vector3(i,j,k) + chunkPositionSeed;
					float heightmapRidgeNoise = heightmapNoise.ComputeNoise(new Vector2(samplePoint.x, samplePoint.z));
					float heightmapDetailNoise = ComputeOctaveNoise(new Vector2(samplePoint.x, samplePoint.z), heightmapDetail);

					float terrainValue = -samplePoint.y - Mathf.Abs(heightmapRidgeNoise*2-1)*heightmapNoiseAmplitude + heightmapDetailNoise*heightmapDetailAmplitude;

					float caveA = ComputeOctaveNoise(samplePoint, caveNoiseA);
					float caveB = ComputeOctaveNoise(samplePoint, caveNoiseB);
					caveA = Mathf.Clamp(1 - Mathf.Abs(caveA*caveDiameter - caveDiameter*0.5f), -1, 1);
					caveB = Mathf.Clamp(1 - Mathf.Abs(caveB*caveDiameter - caveDiameter*0.5f), -1, 1);
					float caveValue = ( ((caveA+1)/2)*((caveB+1)/2)*2 - 1);
					caveValue += (caveDetailNoise.ComputeNoise(samplePoint))*caveDetailMask.ComputeNoise(samplePoint);
					caveValue += ComputeOctaveNoise(samplePoint,caveSpaceNoise);
					// caveValue *= 5 * Mathf.Clamp(value-1, 0, 1);

					float caveProximity = caveProximityNoise.ComputeNoise(new Vector2(samplePoint.x, samplePoint.z));
					float caveProximityPow = Mathf.Pow(caveProximity, 5);
					caveProximity = caveProximityPow/(caveProximityPow + Mathf.Pow(1-caveProximity,ProximityHarshness));

					float value = Mathf.Lerp(caveValue, terrainValue, -terrainValue + maxCaveDepth*(1-caveProximity));
					
					
					// value = -samplePoint.y + caveProximity*10;
					value = Mathf.Clamp(value, -1, 1);
					data[i][j][k] = value;
				}
			}
		}
		return new ChunkData(data);
	}
	float ComputeOctaveNoise(Vector3 position, NoiseOctave3D[] octaves){
		float value = 0;
		foreach(NoiseOctave3D octave in octaves){
			value += octave.ComputeNoise(position);
		}
		return value;
	}
	float ComputeOctaveNoise(Vector2 position, NoiseOctave2D[] octaves){
		float value = 0;
		foreach(NoiseOctave2D octave in octaves){
			value += octave.ComputeNoise(position);
		}
		return value;
	}
	void LoadChunk((int,int,int) chunkID){
		if(!worldData.ContainsKey(chunkID)){
			worldData.Add(chunkID,GenerateChunkData(chunkID));
		}
		Chunk loadedChunk = new Chunk(IntToVec(chunkID)*chunkWorldSize, worldData[chunkID], chunkPrefab, surfaceLevel, GridScale());
		loadedChunks.Add(chunkID, loadedChunk);
		//Spawn Chunk
	}
	(int,int,int)[] ChunksAroundChunk((int,int,int) baseChunkID, int chunkRange){
		List<(int,int,int)> positions = new List<(int,int,int)>();
		for(int i = baseChunkID.Item1 - chunkRange; i <= baseChunkID.Item1 + chunkRange; i++){
			for(int j = baseChunkID.Item2 - chunkRange; j <= baseChunkID.Item2 + chunkRange; j++){
				for(int k = baseChunkID.Item3 - chunkRange; k <= baseChunkID.Item3 + chunkRange; k++){
					positions.Add((i,j,k));
				}
			}
		}
		return positions.ToArray();
	}
	// (int,int,int)[] ChunksAroundGridPosition(Vector3 gridPosition, float gridRange){
	// 	List<(int,int,int)> positions = new List<(int,int,int)>();
	// 	// HashSet<(int,int,int)> positionSet = new HashSet<(int,int,int)>();
	// 	for(float i = gridPosition.x - gridRange; i <= gridPosition.x + gridRange; i+=gridRange){
	// 		for(float j = gridPosition.y - gridRange; j <= gridPosition.y + gridRange; j+=gridRange){
	// 			for(float k = gridPosition.z - gridRange; k <= gridPosition.z + gridRange; k+=gridRange){
	// 				(int,int,int) chunk = GridPosToChunkID(new Vector3(i,j,k));
	// 				positions.Add(chunk);
	// 			}
	// 		}
	// 	}
	// 	return positions.ToArray();
	// }
	
	(int,int,int) WorldPosToGridID(Vector3 WorldPosition){
		return GridPosToChunkID(WorldPosToGridPos(WorldPosition));
	}
	Vector3 WorldPosToGridPos(Vector3 WorldPosition){
		return WorldPosition/GridScale(); 
	}
	(int,int,int) GridPosToChunkID(Vector3 GridPosition){
		GridPosition /= chunkGridSize;
		return (Mathf.RoundToInt(GridPosition.x),Mathf.RoundToInt(GridPosition.y),Mathf.RoundToInt(GridPosition.z));
	}
	Vector3 ChunkIdToGridPos((int,int,int) chunkID){
		return IdToVec(chunkID)*chunkGridSize;
	}
	Vector3 ChunkPointToGridPos((int,int,int) chunkID, (int,int,int) pointID){
		return ChunkIdToGridPos(chunkID) + IdToVec(pointID);
	}
	Vector3 GridToWorldPos(Vector3 GridPosition){
		return GridPosition * GridScale();
	}
	float GridScale(){
		return chunkWorldSize/chunkGridSize;
	}
	Vector3 IdToVec((int,int,int) id){
		return new Vector3(id.Item1, id.Item2, id.Item3);
	}
	void Update()
	{	
		(int,int,int) baseChunkID = WorldPosToGridID(trackedObject.position);
		(int,int,int)[] positions = ChunksAroundChunk(baseChunkID, renderDistance);
		HashSet<(int,int,int)> positionSet = new HashSet<(int,int,int)>();

		for(int i = 0; i < positions.Length; i++){
			positionSet.Add(positions[i]);
			if(!loadedChunks.ContainsKey(positions[i])){
				LoadChunk(positions[i]);
			}
		}
		List<(int,int,int)> unloadedChunks = new List<(int,int,int)>();
		foreach (var loadedChunk in loadedChunks)
		{
			if(!positionSet.Contains(loadedChunk.Key)){
				unloadedChunks.Add(loadedChunk.Key);
			}
		}

		for(int i = 0; i < unloadedChunks.Count; i++){
			(int,int,int) unloadedChunk = unloadedChunks[i];
			loadedChunks[unloadedChunk].UnloadChunk();
			loadedChunks.Remove(unloadedChunk);
			
		}
	}
	public void ModifyData(Vector3 position, float change, float radius){
		Vector3 gridPosition = WorldPosToGridPos(position);
		float gridRadius = radius/GridScale();
		float chunkRadius = gridRadius/chunkGridSize;
		(int,int,int)[] affectedChunks = ChunksAroundChunk(GridPosToChunkID(gridPosition),Mathf.RoundToInt(Mathf.Ceil(chunkRadius)+1));
		for(int i = 0; i < affectedChunks.Length; i++){
			ModifyChunk(affectedChunks[i], gridPosition, change, gridRadius);
		}
	}
	void ModifyChunk((int,int,int) chunkID, Vector3 GridOrigin, float change, float gridRadius){
		if(!worldData.ContainsKey(chunkID)){
			worldData.Add(chunkID, GenerateChunkData(chunkID));
		}
		Vector3 chunkGridPosition = ChunkIdToGridPos(chunkID);
		float[][][] data = worldData[chunkID].data;
		float gridRadiusSqr = gridRadius * gridRadius;
		for(int i = 0; i < data.Length; i++){
			for(int j = 0; j < data[i].Length; j++){
				for(int k = 0; k < data[j].Length; k++){
					Vector3 cellGridPosition = chunkGridPosition + IdToVec((i,j,k));
					Vector3 delta = cellGridPosition - GridOrigin;

					if(delta.sqrMagnitude > gridRadiusSqr){
						continue;
					}
					data[i][j][k] += change;
					data[i][j][k] = Mathf.Clamp(data[i][j][k], -1,1);
				}
			}
		}
		if(loadedChunks.ContainsKey(chunkID)){
			loadedChunks[chunkID].UpdateChunk();
		}
	}
	static Vector3 IntToVec((int,int,int) pos){
		return new Vector3(pos.Item1, pos.Item2, pos.Item3);
	}
}
