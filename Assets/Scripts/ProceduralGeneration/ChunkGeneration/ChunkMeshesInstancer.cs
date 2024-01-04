using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEditor.Presets;
using System.Linq;

public class ChunkMeshesInstancer : MonoBehaviour
{
	[Header("MeshInstancing")]
	[SerializeField] Transform playerTransform;
	[SerializeField] ProceduralTerrainPreset terrainPreset;
	[SerializeField] BiomePreset[] biomes;
	[SerializeField] GameObject bridgePrefab;
	[SerializeField][Range(1, 6)] int bridgesCount = 3;
	[Header("Grass")]
	[SerializeField] float grassHeightThreshold = 12f;
	[SerializeField][Range(1, 8)] int grassChunkSizeModifier = 4;
	[SerializeField][Range(1, 8)] int density = 4;
	[SerializeField] float windSpeed = 1f;
	[SerializeField] float windFrequency = 20f;
	[SerializeField] float windAmplitude = 3f;
	[SerializeField] float addValue = 1f;
	[SerializeField][Range(1, 10)] int grassRenderDistance = 1;
	[SerializeField] RenderTexture animationTexture;
	[Header("TrailsGeneration")]
	[SerializeField] Transform point1;
	[SerializeField] Transform point2;

    ComputeShader grassPositionsShader;
	ComputeShader animationMapShader;
	ComputeShader cullGrassUnderWaterShader;

	ComputeBuffer voteBuffer;
	ComputeBuffer prescanBuffer;
	ComputeBuffer groupSumBuffer;
	ComputeBuffer offsetBuffer;

	ChunkDataProvider chunkDataProvider;
	Dictionary<Vector2Int, float> globalHeightMap;
	Dictionary<Vector2Int, Chunk> chunks;
	Dictionary<string, Dictionary<Vector2, GrassData>> grassesData = new();
	List<GameObject> trees = new();
	Vector2Int[] riversPoints;
	int animationTextureResolution = 1024;

	Vector2 terrainSize;
	float grassChunkSize;
	int grassChunkSizeRounded;
	bool areMeshesInstanced;
	int grassChunkDimensionGrassCount;
	int grassChunkGrassCount;
	int grassChunksPerTerrainChunk;

	int prescanThreadCount;
	int prescanThreadBlocksCount;
	int groupSumScanThreadBlocksCount;
	int compactThreadBlocksCount;

	NavMeshSurface navSurface;

	List<ComputeBuffer> propertiesBuffers = new();
	List<ComputeBuffer> argsBuffers = new();

	Grid grid;

	PathFinding pathfinding;

	enum TerrainCheckType
	{
		circular,
		rectangular
	}

	private void Awake()
	{
		grid = GetComponent<Grid>();
		navSurface = GetComponent<NavMeshSurface>();
		grassPositionsShader = Resources.Load<ComputeShader>("GrassPositionsShader");
		animationMapShader = Resources.Load<ComputeShader>("AnimationMapShader");
		cullGrassUnderWaterShader = Resources.Load<ComputeShader>("CullGrassUnderWaterShader");

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
		grassPositionsShader.SetFloat("_HeightMultiplier", terrainPreset.HeightMultiplier);
		grassPositionsShader.SetInt("_Density", density);
		grassPositionsShader.SetFloat("_XTerrainDimention", terrainSize.x);
		grassPositionsShader.SetFloat("_YTerrainDimention", terrainSize.y);
		grassPositionsShader.SetInt("_TerrainChunkSize", terrainPreset.ChunkSize);
		grassPositionsShader.SetInt("_XTerrainSize", terrainPreset.TerrainSize.x);
		grassPositionsShader.SetInt("_YTerrainSize", terrainPreset.TerrainSize.y);
		grassPositionsShader.SetInt("_ChunkDimensionGrassCount", grassChunkDimensionGrassCount);

		animationMapShader.SetTexture(0, "_AnimationMap", animationTexture);
		animationMapShader.SetInt("_Resolution", animationTextureResolution);

		prescanThreadCount = 0;
		int grassCountCopy = grassChunkGrassCount / 2;

		while(grassCountCopy > 0 )
		{
			prescanThreadCount += 64;
			grassCountCopy -= 64;
		}

		prescanThreadBlocksCount = prescanThreadCount / 64;
		groupSumScanThreadBlocksCount = 1;

		while (groupSumScanThreadBlocksCount < prescanThreadBlocksCount)
		{
			groupSumScanThreadBlocksCount *= 2;
		}

		compactThreadBlocksCount = Mathf.CeilToInt(grassChunkGrassCount / 128f);

		voteBuffer = new(grassChunkGrassCount, sizeof(uint));
		prescanBuffer = new(grassChunkGrassCount, sizeof(uint));
		groupSumBuffer = new(groupSumScanThreadBlocksCount, sizeof(uint));
		offsetBuffer = new(1024, sizeof(uint));
	}
	void EnableRendering(ChunkDataProvider chunkDataProvider, Dictionary<Vector2Int, Chunk> chunks, Vector2Int[] riversPoints, Dictionary<Vector2Int, float> globalHeightMap)
	{
		this.chunkDataProvider = chunkDataProvider;
		this.chunks = chunks;
		this.riversPoints = riversPoints;
		this.globalHeightMap = globalHeightMap;

		CalculateGrassPositions();
		InstantiateTrees(40);
		//GenerateVillages(5, 15, 200, 35);
		//InstantiateBridges(bridgesCount);
		pathfinding = new PathFinding(grid);
		GeneratePaths();

		navSurface.BuildNavMesh();

		EventManager.OnChunkMeshesInstanced.Invoke();
		areMeshesInstanced = true;
	}
	void CalculateGrassPositions()
	{
		Vector2 centerOffset = new Vector2(
			(terrainPreset.TerrainSize.x * terrainPreset.ChunkSize / 2f) - terrainPreset.ChunkSize / 2f,
			(terrainPreset.TerrainSize.y * terrainPreset.ChunkSize / 2f) - terrainPreset.ChunkSize / 2f
		);

		int i = 0;
		foreach (BiomePreset biome in biomes)
		{
			foreach (GrassInspectorData grassData in biome.GrassesData)
			{
				for (int y = 0; y < terrainPreset.TerrainSize.y; y++)
				{
					for (int x = 0; x < terrainPreset.TerrainSize.x; x++)
					{
						Vector2Int position = new Vector2Int((int)(x * terrainPreset.ChunkSize - centerOffset.x), (int)(y * terrainPreset.ChunkSize - centerOffset.y));
						for (float offsetY = 0.5f; offsetY <= grassChunkSizeModifier - 0.5f; offsetY += 1)
						{
							for (float offsetX = 0.5f; offsetX <= grassChunkSizeModifier - 0.5f; offsetX += 1)
							{
								float heightMapOffsetX = offsetX - 0.5f;
								float heightMapOffsetY = offsetY - 0.5f;
								Vector2 heightMapOffset = new Vector2((int)(heightMapOffsetX * (grassChunkSize)), (int)(heightMapOffsetY * (grassChunkSize)));
								
								Vector2 chunkOffset = new Vector2(offsetX * grassChunkSize, offsetY * grassChunkSize);
								chunkOffset.x -= terrainPreset.ChunkSize / 2f;
								chunkOffset.y -= terrainPreset.ChunkSize / 2f;

								Vector2 chunkPosition = new Vector2(position.x + chunkOffset.x, position.y + chunkOffset.y);
								Bounds bounds = new(new Vector3(chunkPosition.x, 0, chunkPosition.y), new Vector3(grassChunkSizeRounded + 5, terrainPreset.HeightMultiplier, grassChunkSizeRounded + 5));

								Material material = new(grassData.material);
								Chunk chunk = chunks[position];

								//creating args array
								uint[] args = new uint[5];
								args[0] = grassData.mesh.GetIndexCount(0);
								args[1] = (uint)0;

								//setting args array to ComputeBuffer
								ComputeBuffer argsBuffer = new(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
								argsBuffer.SetData(args);
								argsBuffers.Add(argsBuffer);

								float[] heightArray = Noise.TransformTo1DArray(chunk.HeightMap);

								//other ComputeBuffers
								ComputeBuffer meshPropertiesBuffer = new(grassChunkGrassCount, MeshProperties.Size());
								ComputeBuffer heightArrayBuffer = new(heightArray.Length, sizeof(float));
								heightArrayBuffer.SetData(heightArray);

								//calculating non culled grass data
								grassPositionsShader.SetBuffer(0, "_Properties", meshPropertiesBuffer);
								grassPositionsShader.SetBuffer(0, "_HeightArray", heightArrayBuffer);
								grassPositionsShader.SetFloat("_XOffset", heightMapOffset.x);
								grassPositionsShader.SetFloat("_YOffset", heightMapOffset.y);
								grassPositionsShader.Dispatch(0, grassChunkDimensionGrassCount, grassChunkDimensionGrassCount, 1);

								heightArrayBuffer.Release();

								//voting
								cullGrassUnderWaterShader.SetBuffer(0, "_Properties", meshPropertiesBuffer);
								cullGrassUnderWaterShader.SetBuffer(0, "_VoteBuffer", voteBuffer);
								cullGrassUnderWaterShader.SetFloat("_WaterTreshold", grassHeightThreshold);
								cullGrassUnderWaterShader.Dispatch(0, grassChunkGrassCount, 1, 1);

								//scanning
								cullGrassUnderWaterShader.SetBuffer(1, "_VoteBuffer", voteBuffer);
								cullGrassUnderWaterShader.SetBuffer(1, "_ScanBuffer", prescanBuffer);
								cullGrassUnderWaterShader.SetBuffer(1, "_GroupSumArray", groupSumBuffer);
								cullGrassUnderWaterShader.Dispatch(1, groupSumScanThreadBlocksCount, 1, 1);
								
								//scanning groups sums
								cullGrassUnderWaterShader.SetInt("_NumOfGroups", groupSumScanThreadBlocksCount);
								cullGrassUnderWaterShader.SetBuffer(2, "_GroupSumArrayIn", groupSumBuffer);
								cullGrassUnderWaterShader.SetBuffer(2, "_GroupSumArrayOut", offsetBuffer);
								cullGrassUnderWaterShader.Dispatch(2, 1, 1, 1);

								ComputeBuffer culledMeshPropertiesBuffer = new(compactThreadBlocksCount * 128, MeshProperties.Size());
								propertiesBuffers.Add(culledMeshPropertiesBuffer);

								//compacting
								cullGrassUnderWaterShader.SetBuffer(3, "_Properties", meshPropertiesBuffer);
								cullGrassUnderWaterShader.SetBuffer(3, "_VoteBuffer", voteBuffer);
								cullGrassUnderWaterShader.SetBuffer(3, "_ScanBuffer", prescanBuffer);
								cullGrassUnderWaterShader.SetBuffer(3, "_ArgsBuffer", argsBuffer);
								cullGrassUnderWaterShader.SetBuffer(3, "_CulledProperties", culledMeshPropertiesBuffer);
								cullGrassUnderWaterShader.SetBuffer(3, "_GroupSumArray", offsetBuffer);
								cullGrassUnderWaterShader.Dispatch(3, compactThreadBlocksCount, 1, 1);

								meshPropertiesBuffer.Release();

								material.SetBuffer("_Properties", culledMeshPropertiesBuffer);
								material.SetTexture("_WindTex", animationTexture);
								Color albedo = Random.ColorHSV();
								material.SetVector("_Albedo", albedo);

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
								i++;
							}
						}
					}
				}
			}
		}
	}
	void InstantiateTrees(int amount)
	{
		Vector2 centerOffset = new Vector2(
			(terrainPreset.TerrainSize.x * terrainPreset.ChunkSize / 2f) - terrainPreset.ChunkSize / 2f,
			(terrainPreset.TerrainSize.y * terrainPreset.ChunkSize / 2f) - terrainPreset.ChunkSize / 2f
		);

		foreach (BiomePreset biome in biomes)
		{
			foreach (TreeInspectorData treeData in biome.TreesData)
			{
				for (int y = 0; y < terrainPreset.TerrainSize.y; y++)
				for (int x = 0; x < terrainPreset.TerrainSize.x; x++)
				{
					Vector2Int position = new Vector2Int((int)(x * terrainPreset.ChunkSize - centerOffset.x), (int)(y * terrainPreset.ChunkSize - centerOffset.y));

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
							if (hit.point.y < grassHeightThreshold) continue;
							if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Ground")) continue;

							Vector3 pos = new Vector3(rayPosition.x, hit.point.y, rayPosition.z);
							Vector3 rotation = new Vector3(0, Random.Range(0, 180), 0);
							float scaleFactor = Random.Range(0.8f, 1.2f);

							GameObject tree = Instantiate(treeData.treePrefab, chunk.MeshHolder.transform);
							tree.transform.position = pos;
							tree.transform.rotation = Quaternion.Euler(rotation);
							tree.transform.localScale *= scaleFactor;

							trees.Add(tree);
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

				if (IsAreaLand(matrix, size, inFrontLocal, out avgHeightFront) && IsAreaLand(matrix, size, backLocal, out avgHeightFront))
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
	void GenerateVillages(int amount, int radius, float villagesMinDistance, float clearForestRadius)
	{
		List<Vector2> previousPositions = new();
		for(int i = 0; i < amount; i++)
		{
			Vector2 halfTerrainSize = terrainSize / 2;
			Vector3 position = new Vector3(Random.Range(-halfTerrainSize.x, halfTerrainSize.x), 140, Random.Range(-halfTerrainSize.y, halfTerrainSize.y));
			Ray ray = new(position, Vector3.down);

			if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("Ground")))
			{	
				Matrix4x4 matrix = Matrix4x4.TRS(hit.point, Quaternion.identity, new Vector3(1, 1, 1));
				Vector2 pos2D = new Vector2(hit.point.x, hit.point.z);

				if (IsAreaLand(matrix, new Vector2Int(0, 0), new Vector2Int(radius * 2, radius * 2), out float avgHeight, TerrainCheckType.circular) &&
					previousPositions.Where(pos => Vector2.Distance(pos, pos2D) < villagesMinDistance).Count() <= 0)
				{
					previousPositions.Add(pos2D);
					Debug.DrawRay(ray.origin, Vector3.down * 1000f, Color.magenta, 1000);

					trees
					.Where(tree =>
					{
						Vector3 treePos = tree.transform.position;
						float distance = Vector2.Distance(new Vector2(treePos.x, treePos.z), pos2D);
						return distance <= clearForestRadius;
					})
					.ToList()
					.ForEach(tree => tree.SetActive(false));
				}
				else
				{
					amount++;
				}
			}
		}
	}
	int index = 0;
	bool IsAreaLand(Matrix4x4 matrix, Vector2Int offset, Vector2Int size, out float avgHeight, TerrainCheckType checkType = TerrainCheckType.rectangular)
	{
		if (checkType == TerrainCheckType.circular)
		{
			size *= 2;
		}

		int halfX = size.x / 2;
		int halfY = size.y / 2;

		float sum = 0;
		int amount = 0;

		Color color = Random.ColorHSV();
		for (int y = -halfY; y < halfY; y++)
		{
			for(int x = -halfX; x < halfX; x++)
			{
				Vector2Int point = offset + new Vector2Int(x, y);
				if(checkType == TerrainCheckType.circular && Vector2.Distance(point, Vector2.zero) > size.x / 2)
				{
					continue;
				}

				Vector3 transformedPoint = matrix.MultiplyPoint3x4(new Vector3(point.x, 0, point.y));
				point = new Vector2Int(Mathf.RoundToInt(transformedPoint.x), Mathf.RoundToInt(transformedPoint.z));

				if (globalHeightMap.ContainsKey(point))
				{
					float height = globalHeightMap[point] - 9f;
					//Debug.DrawRay(new Vector3(point.x, height, point.y), Vector3.up * 10, color, 1000);

					//if(index == 0)
					//{
					//	chunkDataProvider.ChangeVertex(new Vector2Int(Mathf.RoundToInt(transformedPoint.x), Mathf.RoundToInt(transformedPoint.z)), (value) => { return value; });
					//}

					sum += height;
					amount++;

					if (height <= grassHeightThreshold)
					{
						avgHeight = sum / amount;
						return false;
					}
				}
				//index++;
			}
		}
		avgHeight = sum / amount;

		return true;
	}
	void GeneratePaths()
	{
		List<Node> paths = pathfinding.FindPath(point1.position, point2.position);
		foreach (Node node in paths)
		{
			Debug.DrawRay(node.WorldPos, Vector3.up * 100f, Color.red, 1000f);
		}
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
		grassesData = null;

		voteBuffer.Release();
		prescanBuffer.Release();
		offsetBuffer.Release();
		groupSumBuffer.Release();
		
		voteBuffer = null;
		prescanBuffer = null;
		offsetBuffer = null;
		groupSumBuffer = null;
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

