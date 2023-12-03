using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.AI.Navigation;
public class ChunkMeshesInstancer : MonoBehaviour
{
	[Header("MeshInstancing")]
	[SerializeField] Transform playerTransform;
	[SerializeField] ProceduralTerrainPreset terrainPreset;
	[SerializeField] BiomePreset[] biomes;
	[SerializeField] GameObject bridgePrefab;
	[SerializeField][Range(1, 6)] int bridgesCount = 3;
	[Header("Grass")]
	[SerializeField][Range(1, 8)] int grassChunkSizeModifier = 4;
	[SerializeField][Range(1, 8)] int density = 4;
	[SerializeField] float windSpeed = 1f;
	[SerializeField] float windFrequency = 20f;
	[SerializeField] float windAmplitude = 3f;
	[SerializeField] float addValue = 1f;
	[SerializeField][Range(1, 10)] int grassRenderDistance = 1;
	[SerializeField] RenderTexture animationTexture;

    ComputeShader grassPositionsShader;
	ComputeShader animationMapShader;

	Dictionary<Vector2Int, float> globalHeightMap;
	Dictionary<Vector2Int, Chunk> chunks;
	Dictionary<string, Dictionary<Vector2, GrassData>> grassesData = new();
	Vector2Int[] riversPoints;
	int animationTextureResolution = 1024;

	Vector2 terrainSize;
	float grassChunkSize;
	int grassChunkSizeRounded;
	bool areMeshesInstanced;
	int grassChunkDimensionGrassCount;
	int grassChunkGrassCount;
	int grassChunksPerTerrainChunk;

	NavMeshSurface navSurface;

	List<ComputeBuffer> propertiesBuffers = new();
	List<ComputeBuffer> argsBuffers = new();

	private void Awake()
	{
		navSurface = GetComponent<NavMeshSurface>();
		grassPositionsShader = Resources.Load<ComputeShader>("GrassPositionsShader");
		animationMapShader = Resources.Load<ComputeShader>("AnimationMapShader");

		animationTexture = new(animationTextureResolution, animationTextureResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
		animationTexture.enableRandomWrite = true;
		animationTexture.Create();
	}
	private void Start()
	{
		terrainSize = terrainPreset.TerrainSize * terrainPreset.ChunkSize;

		grassChunkSize = terrainPreset.ChunkSize / (float)grassChunkSizeModifier;
		grassChunkSizeRounded = Mathf.RoundToInt(grassChunkSize);
		grassChunksPerTerrainChunk = Mathf.RoundToInt(terrainPreset.ChunkSize / (float)grassChunkSizeRounded);
		grassChunkDimensionGrassCount = grassChunkSizeRounded * density;
		grassChunkGrassCount = (int)Mathf.Pow(grassChunkDimensionGrassCount, 2);

		InitializeComputeShaders();

		EventManager.OnChunksGenerationCompleated.AddListener(EnableRendering);
	}
	void InitializeComputeShaders()
	{
		grassPositionsShader.SetFloat("_HeightMultiplier", terrainPreset.HeightMultiplayer);
		grassPositionsShader.SetInt("_Density", density);
		grassPositionsShader.SetFloat("_XTerrainDimention", terrainSize.x);
		grassPositionsShader.SetFloat("_YTerrainDimention", terrainSize.y);
		grassPositionsShader.SetInt("_TerrainChunkSize", terrainPreset.ChunkSize);
		grassPositionsShader.SetInt("_XTerrainSize", terrainPreset.TerrainSize.x);
		grassPositionsShader.SetInt("_YTerrainSize", terrainPreset.TerrainSize.y);
		grassPositionsShader.SetInt("_ChunkDimensionGrassCount", grassChunkDimensionGrassCount);

		animationMapShader.SetTexture(0, "_AnimationMap", animationTexture);
		animationMapShader.SetInt("_Resolution", animationTextureResolution);
	}
	void EnableRendering(Dictionary<Vector2Int, Chunk> chunks, Vector2Int[] riversPoints, Dictionary<Vector2Int, float> globalHeightMap)
	{
		this.chunks = chunks;
		this.riversPoints = riversPoints;
		this.globalHeightMap = globalHeightMap;

		CalculateGrassMatricies();
		InstantiateTrees(40);
		//InstantiateBridges(bridgesCount);

		navSurface.BuildNavMesh();

		EventManager.OnChunkMeshesInstanced.Invoke();
		areMeshesInstanced = true;
	}
	void CalculateGrassMatricies()
	{
		Vector2 centerOffset = new Vector2(terrainPreset.TerrainSize.x / 2, terrainPreset.TerrainSize.y / 2);

		foreach (BiomePreset biome in biomes)
		{
			foreach (GrassInspectorData grassData in biome.GrassesData)
			{
				for (int y = 0; y < terrainPreset.TerrainSize.y; y++)
				{
					for (int x = 0; x < terrainPreset.TerrainSize.x; x++)
					{
						Vector2Int position = new Vector2Int((int)((x - centerOffset.x + 0.5f) * terrainPreset.ChunkSize), (int)((y - centerOffset.y + 0.5f) * terrainPreset.ChunkSize));

						//Debug.Log($"ChunkPosition: {position}");

						//float offsetIncrement = 1 / (float)grassChunkSizeModifier;
						for (float offsetY = 0.5f; offsetY <= grassChunkSizeModifier - 0.5f; offsetY += 1)
						{
							for (float offsetX = 0.5f; offsetX <= grassChunkSizeModifier - 0.5f; offsetX += 1)
							{
								float heightMapOffsetX = offsetX - 0.5f;
								float heightMapOffsetY = offsetY - 0.5f;

								//Debug.Log($"grassChunkSize: {grassChunkSize} rounded: {grassChunkSizeRounded}");
								Vector2 heightMapOffset = new Vector2((int)(heightMapOffsetX * (grassChunkSize)), (int)(heightMapOffsetY * (grassChunkSize)));
								
								Vector2 chunkOffset = new Vector2(offsetX * grassChunkSize, offsetY * grassChunkSize);
								//Debug.Log($"offset: {new Vector2(offsetX, offsetY)} chunkOffset: {chunkOffset} heightMapOffset: {heightMapOffset}");
								chunkOffset.x -= terrainPreset.ChunkSize / 2f;
								chunkOffset.y -= terrainPreset.ChunkSize / 2f;	

								Vector2 chunkPosition = new Vector2(position.x + chunkOffset.x, position.y + chunkOffset.y);

								//Debug.Log($"chunk width: {grassChunkSizeRounded}");

								Bounds bounds = new(new Vector3(chunkPosition.x, 0, chunkPosition.y), new Vector3(grassChunkSizeRounded + 5, terrainPreset.HeightMultiplayer, grassChunkSizeRounded + 5));

								Material material = new(grassData.material);
								Chunk chunk = chunks[position];

								//creating args array
								uint[] args = new uint[5];
								args[0] = grassData.mesh.GetIndexCount(0);
								args[1] = (uint)grassChunkGrassCount;

								//setting args array to ComputeBuffer
								ComputeBuffer argsBuffer = new(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
								argsBuffer.SetData(args);
								argsBuffers.Add(argsBuffer);

								float[] heightArray = Noise.TransformTo1DArray(chunk.HeightMap);

								//other ComputeBuffers
								ComputeBuffer meshPropertiesBuffer = new(grassChunkGrassCount, MeshProperties.Size());
								ComputeBuffer heigthArrayBuffer = new(heightArray.Length, sizeof(float));
								heigthArrayBuffer.SetData(heightArray);

								//setting data to position buffer
								grassPositionsShader.SetBuffer(0, "_Properties", meshPropertiesBuffer);
								grassPositionsShader.SetBuffer(0, "_HeightArray", heigthArrayBuffer);
								grassPositionsShader.SetFloat("_XOffset", heightMapOffset.x);
								grassPositionsShader.SetFloat("_YOffset", heightMapOffset.y);

								grassPositionsShader.Dispatch(0, grassChunkDimensionGrassCount, grassChunkDimensionGrassCount, 1);

								material.SetBuffer("_Properties", meshPropertiesBuffer);
								material.SetTexture("_WindTex", animationTexture);
								Color albedo = Random.ColorHSV();
								material.SetVector("_Albedo", albedo);

								propertiesBuffers.Add(heigthArrayBuffer);
								propertiesBuffers.Add(meshPropertiesBuffer);

								//adding chunk to dictionary
								if (!grassesData.ContainsKey(biome.BiomeName))
								{
									grassesData[biome.BiomeName] = new();
								}

								grassesData[biome.BiomeName][chunkPosition] = new GrassData(
									biome.BiomeName,
									grassData.mesh,
									material,
									bounds,
									argsBuffer
								);
							}
						}
					}
				}
			}
		}
	}
	void InstantiateTrees(int amount)
	{
		Vector2 centerOffset = new Vector2(terrainPreset.TerrainSize.x / 2, terrainPreset.TerrainSize.y / 2);

		foreach (BiomePreset biome in biomes)
		{
			foreach (TreeInspectorData treeData in biome.TreesData)
			{
				for (int y = 0; y < terrainPreset.TerrainSize.y; y++)
				for (int x = 0; x < terrainPreset.TerrainSize.x; x++)
				{
					Vector2Int position = new Vector2Int((int)((x - centerOffset.x + 0.5f) * terrainPreset.ChunkSize), (int)((y - centerOffset.y + 0.5f) * terrainPreset.ChunkSize));

					for (int i = 0; i < treeData.amount; i++)
					{
						float xOffset = Random.Range(-terrainPreset.ChunkSize / 2f, terrainPreset.ChunkSize / 2f);
						float yOffset = Random.Range(-terrainPreset.ChunkSize / 2f, terrainPreset.ChunkSize / 2f);

						Vector3 rayPosition = new Vector3(position.x + xOffset, 90, position.y + yOffset);

						Ray ray = new(rayPosition, Vector3.down);
						RaycastHit hit;

						Chunk chunk = chunks[position];

						if (Physics.Raycast(ray, out hit))
						{
							if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Ground")) continue;

							Vector3 pos = new Vector3(rayPosition.x, hit.point.y, rayPosition.z);
							Vector3 rotation = new Vector3(0, Random.Range(0, 180), 0);
							float scaleFactor = Random.Range(0.8f, 1.2f);

							GameObject tree = Instantiate(treeData.treePrefab, chunk.MeshHolder.transform);
							tree.transform.position = pos;
							tree.transform.rotation = Quaternion.Euler(rotation);
							tree.transform.localScale *= scaleFactor;
						}
					}
				}
			}
		}
	}
	void InstantiateBridges(int amount)
	{
		int step = riversPoints.Length / amount;
		int start = step / 2;

		//Debug.Log($"step: {step}");

		for(int i = start; i < riversPoints.Length; i += step)
		{
			Vector2Int pos = new();
			Quaternion rot = new();

			float avgHeightFront = 0;
			float avgHeightBack = 0;

			bool isLocationFound = false;
			while (!isLocationFound)
			{
				pos = riversPoints[i];
				Vector2Int nextPos = riversPoints[i + 10];
				Vector3 fullPos = new Vector3(pos.x, 0, pos.y);
				Vector3 nextFullPos = new Vector3(nextPos.x, 0, nextPos.y);

				Vector3 dir = nextFullPos - fullPos;
				Vector3 forward = Vector3.Cross(dir, Vector3.up);

				rot = Quaternion.LookRotation(forward);

				Matrix4x4 matrix = Matrix4x4.TRS(fullPos, rot, new Vector3(1, 1, 1));

				Vector2Int inFrontLocal = new Vector2Int(0, 35);
				Vector2Int backLocal = new Vector2Int(0, -35);

				Vector2Int size = new Vector2Int(25, 10);

				if (IsAreaLand(inFrontLocal, size, matrix, out avgHeightFront) && IsAreaLand(backLocal, size, matrix, out avgHeightFront))
				{
					isLocationFound = true;
				}
				else
				{
					i += 5;
				}
			}

			float avgHeight = (avgHeightFront + avgHeightBack) / 2;

			float height = avgHeight - avgHeight * 0.12f;

			GameObject bridge = Instantiate(bridgePrefab);
			bridge.transform.position = new Vector3(pos.x, height, pos.y);
			bridge.transform.rotation = rot;
		}
	}
	bool IsAreaLand(Vector2Int center, Vector2Int size, Matrix4x4 matrix, out float avgHeight)
	{
		bool isAreaLand = true;

		int halfX = size.x / 2;
		int halfY = size.y / 2;

		float sum = 0;
		int amount = 0;

		for (int y = -halfY; y < halfY; y++)
		{
			for(int x = -halfX; x < halfX; x++)
			{
				Vector2Int point = center + new Vector2Int(x, y);
				Vector3 transformedPoint = matrix.MultiplyPoint3x4(new Vector3(point.x, 0, point.y));
				point = new Vector2Int(Mathf.RoundToInt(transformedPoint.x), Mathf.RoundToInt(transformedPoint.z));

				if (globalHeightMap.ContainsKey(point))
				{
					float height = globalHeightMap[point];

					Debug.DrawRay(new Vector3(point.x, 0, point.y), Vector3.up * 10, Color.blue, 100);

					//Debug.Log($"height: {height}");

					sum += height;
					amount++;

					if (height <= 50)
					{
						isAreaLand = false;
					}
				}
			}
		}
		avgHeight = sum / amount;

		return isAreaLand;
	}
	private void Update()
	{
		if (!areMeshesInstanced) return;

		RenderAnimationTexture();

		int i = 0;
		foreach (KeyValuePair<Vector2, GrassData> pair in grassesData["FlatTerrain"])
		{
			//if (i != 0) return;

			GrassData data = pair.Value;

			Graphics.DrawMeshInstancedIndirect(data.Mesh, 0, data.Material, data.Bounds, data.ArgsBuffer);
			i++;
		}
		RenderGrassMeshes();
	}
	void RenderAnimationTexture()
	{
		animationMapShader.SetFloat("_Time", Time.time * windSpeed);
		animationMapShader.SetFloat("_Amplitude", windAmplitude);
		animationMapShader.SetFloat("_Frequency", windFrequency);
		animationMapShader.SetFloat("_AddValue", addValue);
		animationMapShader.Dispatch(0, animationTextureResolution, animationTextureResolution, 1);
	}
	void RenderGrassMeshes()
	{
		Vector3 playerPos = playerTransform.position;

		//Vector2 currentChunkPos = new Vector2(Mathf.Floor(playerPos.x / grassChunkSizeRounded), Mathf.Floor(playerPos.z / grassChunkSizeRounded));
		////Debug.Log(currentChunkPos);
		////currentChunkPos;

		//foreach(BiomePreset biome in biomes)
		//{
		//	if(grassRenderDistance == 1)
		//	{
		//		RenderMeshes(new Vector2(0, 0), currentChunkPos, biome);
		//	}
		//	else
		//	{
		//		int renderDistance = grassRenderDistance - 1;
		//		for (int yOffset = -renderDistance; yOffset <= renderDistance; yOffset++)
		//		{
		//			for (int xOffset = -renderDistance; xOffset <= renderDistance; xOffset++)
		//			{
		//				RenderMeshes(new Vector2(xOffset, yOffset), currentChunkPos, biome);
		//			}
		//		}
		//	}
		//}
		//foreach (KeyValuePair<Vector2, GrassData> pair in grassesData["FlatTerrain"])
		//{
		//	GrassData data = pair.Value;

		//	if(Vector2.Distance(pair.Key, new Vector2(playerPos.x, playerPos.z)) < grassRenderDistance * grassChunkSizeRounded)
		//	{
		//		Graphics.DrawMeshInstancedIndirect(data.Mesh, 0, data.Material, data.Bounds, data.ArgsBuffer);
		//	}
		//}
	}
	void RenderMeshes(Vector2 offset, Vector2 currentChunkPos, BiomePreset biome)
	{
		Vector2 chunkPos = currentChunkPos + offset;
		//Debug.Log(chunkPos);
		chunkPos *= grassChunkSizeRounded * 2;

		//Debug.Log(chunkPos);

		if (grassesData[biome.BiomeName].ContainsKey(chunkPos))
		{
			GrassData data = grassesData[biome.BiomeName][chunkPos];

			Graphics.DrawMeshInstancedIndirect(data.Mesh, 0, data.Material, data.Bounds, data.ArgsBuffer);
		}
	}
	void OnDisable()
	{
		grassesData = null;

		if (propertiesBuffers != null)
		{
			foreach (ComputeBuffer propertiesBuffer in propertiesBuffers)
			{
				propertiesBuffer.Release();
			}
		}
		propertiesBuffers = null;

		if (argsBuffers != null)
		{
			foreach(ComputeBuffer argsBuffer in argsBuffers)
			{
				argsBuffer.Release();
			}
		}
		argsBuffers = null;
	}
	private struct MeshProperties
	{
		public Vector4 Position { get; set; }
        public Vector2 WorldUv { get; set; }
		public MeshProperties(Vector4 position, Vector2 worldUv)
		{
			Position = position;
			WorldUv = worldUv;
		}
		public static int Size()
		{
			return sizeof(float) * 4 + //position
				sizeof(float) * 2; //worldUv
		}
	}
	private struct GrassData
	{
        //public int Id { get; set; }
        public string BiomeName { get; set; }
        public Mesh Mesh { get; set; }
		public Material Material { get; set; }
		public Bounds Bounds { get; set; }
		public ComputeBuffer ArgsBuffer { get; set; }
        public GrassData(/*int id,*/string biomeName, Mesh mesh, Material material, Bounds bounds, ComputeBuffer argsBuffer)
		{
			//Id = id;
			BiomeName = biomeName;
			Mesh = mesh;
			Material = material;
			Bounds = bounds;
			ArgsBuffer = argsBuffer;
		}
	} 
}

