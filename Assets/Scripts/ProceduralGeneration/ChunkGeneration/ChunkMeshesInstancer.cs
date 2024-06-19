using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using System.Linq;
using AStarPathfinding;
using System.Collections;

public class ChunkMeshesInstancer : MonoBehaviour
{
	[Header("MeshInstancing")]
	[SerializeField] Transform playerTransform;
	[SerializeField] ProceduralTerrainPreset terrainPreset;
	[SerializeField] BiomePreset[] biomes;
	[SerializeField] GameObject bridgePrefab;
	[SerializeField][Range(1, 6)] int bridgesCount = 3;
	[Header("Grass")]
	[SerializeField] bool generateGrass;
	[SerializeField] GrassPreset grassPreset;
    [Header("TrailsGeneration")]
    [SerializeField] PathGenerationPreset pathGenerationPreset;
	[Header("VillageGeneration")]
	[SerializeField] VillageGenerationPreset villageGenerationPreset;

	bool isRenderingEnabled;

    ComputeShader grassPositionsShader;
	ComputeShader animationMapShader;
	ComputeShader cullGrassUnderWaterShader;
	ComputeShader pathTextureShader;

	ComputeBuffer voteBuffer;
	ComputeBuffer prescanBuffer;
	ComputeBuffer groupSumBuffer;
	ComputeBuffer offsetBuffer;

    ComputeBuffer heightsBuffer;

    ChunkDataProvider chunkDataProvider;
	Dictionary<Vector2Int, float> globalHeightMap;
	Dictionary<Vector2Int, Chunk> chunks;
	Dictionary<string, Dictionary<Vector2, GrassData>> grassesData = new();
	List<GameObject> trees = new();
	Vector2Int[] riversPoints;

    RenderTexture animationTexture;
    int animationTextureResolution = 1024;
	int pathTextureResolution = 1024;

	Vector2 terrainSize;
	float grassChunkSize;
	int grassChunkSizeRounded;
	bool areMeshesInstanced;
	int grassChunkDimensionGrassCount;
	int grassChunkGrassCount;
	int grassChunksPerTerrainChunkDimention;

	int prescanThreadCount;
	int prescanThreadBlocksCount;
	int groupSumScanThreadBlocksCount;
	int compactThreadBlocksCount;

	NavMeshSurface navSurface;

	List<ComputeBuffer> propertiesBuffers = new();
	List<ComputeBuffer> argsBuffers = new();
	List<RenderTexture> pathTextures = new();

	List<Vector2> villagesPositions;
	List<Vector3> pathPositions = new();

	PathFinding pathfinding;

	enum TerrainCheckType
	{
		circular,
		rectangular
	}
    private void Awake()
	{
		navSurface = GetComponent<NavMeshSurface>();
		grassPositionsShader = Resources.Load<ComputeShader>("GrassPositionsShader");
		animationMapShader = Resources.Load<ComputeShader>("AnimationMapShader");
		cullGrassUnderWaterShader = Resources.Load<ComputeShader>("CullGrassUnderWaterShader");
		pathTextureShader = Resources.Load<ComputeShader>("PathTextureShader");		
	}
    private void OnEnable()
    {
        EventManager.OnChunksGenerationCompleated.AddListener(AssignDependencies);
        EventManager.OnGeneratedPathfindingGrid.AddListener(() => { isRenderingEnabled = true; });
        EventManager.OnPathPresetValidate.AddListener(GeneratePathTextures);
    }
    private void Start()
	{
		terrainSize = terrainPreset.TerrainSize * terrainPreset.ChunkSize;

		CalculateGrassData();
        InitializeComputeShaders();
		InitializeComputeBuffers();
	}
	void CalculateGrassData()
	{
        grassChunkSize = terrainPreset.ChunkSize / (float)grassPreset.GrassChunkSizeModifier;
        grassChunkSizeRounded = Mathf.RoundToInt(grassChunkSize);
        grassChunksPerTerrainChunkDimention = Mathf.RoundToInt(terrainPreset.ChunkSize / (float)grassChunkSizeRounded);
        grassChunkDimensionGrassCount = grassChunkSizeRounded * grassPreset.Density;
        grassChunkGrassCount = (int)Mathf.Pow(grassChunkDimensionGrassCount, 2);
    }
	void InitializeComputeShaders()
	{
		grassPositionsShader.SetFloat("_HeightMultiplier", terrainPreset.HeightMultiplier);
		grassPositionsShader.SetInt("_Density", grassPreset.Density);
		grassPositionsShader.SetFloat("_XTerrainDimention", terrainSize.x);
		grassPositionsShader.SetFloat("_YTerrainDimention", terrainSize.y);
		grassPositionsShader.SetInt("_TerrainChunkSize", terrainPreset.ChunkSize);
		grassPositionsShader.SetInt("_XTerrainSize", terrainPreset.TerrainSize.x);
		grassPositionsShader.SetInt("_YTerrainSize", terrainPreset.TerrainSize.y);
		grassPositionsShader.SetInt("_ChunkDimensionGrassCount", grassChunkDimensionGrassCount);

        cullGrassUnderWaterShader.SetInt("_TerrainChunkSize", terrainPreset.ChunkSize);
        cullGrassUnderWaterShader.SetInt("_PathTextureResolution", pathTextureResolution);
        cullGrassUnderWaterShader.SetInt("_GrassChunkResolution", grassChunkDimensionGrassCount);
        cullGrassUnderWaterShader.SetFloat("_GrassChunkSize", grassChunkSize);
        cullGrassUnderWaterShader.SetFloat("_WaterTreshold", grassPreset.GrassHeightThreshold);

        animationTexture = new(animationTextureResolution, animationTextureResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        animationTexture.enableRandomWrite = true;
        animationTexture.Create();

        animationMapShader.SetTexture(0, "_AnimationMap", animationTexture);
		animationMapShader.SetInt("_Resolution", animationTextureResolution);
        animationMapShader.SetFloat("_Amplitude", grassPreset.GrassAnimationPreset.WindAmplitude);
        animationMapShader.SetFloat("_Frequency", grassPreset.GrassAnimationPreset.WindFrequency);
        animationMapShader.SetFloat("_AddValue", grassPreset.GrassAnimationPreset.AddValue);

		pathTextureShader.SetInt("_Resolution", pathTextureResolution);
		pathTextureShader.SetInt("_ChunkSize", terrainPreset.ChunkSize);
	}
	void InitializeComputeBuffers()
	{
        int grassCountCopy = grassChunkGrassCount / 2;
        prescanThreadCount = 0;
        compactThreadBlocksCount = Mathf.CeilToInt(grassChunkGrassCount / 128f);

        while (grassCountCopy > 0)
        {
            prescanThreadCount += 64;
            grassCountCopy -= 64;
        }

        prescanThreadBlocksCount = prescanThreadCount / 64;
        groupSumScanThreadBlocksCount = 1;

        while (groupSumScanThreadBlocksCount < prescanThreadBlocksCount)
			groupSumScanThreadBlocksCount *= 2;
        
        voteBuffer = new(grassChunkGrassCount, sizeof(uint));
        prescanBuffer = new(grassChunkGrassCount, sizeof(uint));
        groupSumBuffer = new(groupSumScanThreadBlocksCount, sizeof(uint));
        offsetBuffer = new(1024, sizeof(uint));
    }
	void AssignDependencies(ChunkDataProvider chunkDataProvider, Dictionary<Vector2Int, Chunk> chunks, Vector2Int[] riversPoints, Dictionary<Vector2Int, float> globalHeightMap)
	{
		Debug.Log("Assign Dependencies");

		this.chunkDataProvider = chunkDataProvider;
		this.chunks = chunks;
		this.riversPoints = riversPoints;
		this.globalHeightMap = globalHeightMap;

		StartCoroutine(EnableRendering());
	}
	IEnumerator EnableRendering()
	{
		yield return new WaitUntil(() => isRenderingEnabled);

        Debug.Log("Enable Rendering");

        SetTerrainShaderHeights();
        InstantiateTrees();
        GenerateVillagesPositions(15, 15, 200);
        //SpawnVillages(35);
        //InstantiateBridges(bridgesCount);
        pathfinding = new PathFinding(AStarPathfindingGrid.Instance);
        GeneratePaths();
        GeneratePathTextures();
		if (generateGrass) CalculateGrassPositions();

		//navSurface.BuildNavMesh();
		Debug.Log("Instanced chunk meshes invoke");
        EventManager.OnChunkMeshesInstanced.Invoke();
        areMeshesInstanced = true;
    }
    void SetTerrainShaderHeights()
    {
        BiomePreset data = biomes[0];
        heightsBuffer = new(data.Heights.Length, Height.Size());
        heightsBuffer.SetData(data.Heights);

        foreach (KeyValuePair<Vector2Int, Chunk> keyValuePair in chunks)
        {
            Chunk chunk = keyValuePair.Value;

            chunk.SetTerrainHeights(heightsBuffer, data.Heights.Length);
        }
    }
    void GeneratePathTextures()
	{
		FreePathTextures();
		pathTextures = new();

		pathTextureShader.SetInt("_Octaves", pathGenerationPreset.Octaves);
		pathTextureShader.SetFloat("_Scale", pathGenerationPreset.Scale);
		pathTextureShader.SetFloat("_Lacunarity", pathGenerationPreset.Lacunarity);
		pathTextureShader.SetFloat("_Persistance", pathGenerationPreset.Persistance);
		pathTextureShader.SetFloat("_NoiseImpact", pathGenerationPreset.NoiseImpact);
		Vector3 offsetIncrement = pathGenerationPreset.OffsetIncrement;
		pathTextureShader.SetFloats("_OffsetIncrement", new float[] { offsetIncrement.x, offsetIncrement.y, offsetIncrement.z });

		foreach(KeyValuePair<Vector2Int, Chunk> keyValuePair in chunks)
		{
			Chunk chunk = keyValuePair.Value;

			if (chunk.IsChunkInBoundsOfPoints(pathPositions))
			{
				RenderTexture pathTexture = new(pathTextureResolution, pathTextureResolution, 0);
				pathTexture.enableRandomWrite = true;
				pathTexture.Create();

				//Debug.Log("inside");
				List<Vector3> positions = pathPositions.Where(point => chunk.IsPointInBoundOfChunk(point, 20f)).ToList();
				positions = InterpolatePoints(positions, pathGenerationPreset.PathInterpolation);

				ComputeBuffer positionsBuffer = new(positions.Count, sizeof(float) * 3);
				positionsBuffer.SetData(positions);

				pathTextureShader.SetInt("_PathPointsLength", positions.Count);
				pathTextureShader.SetBuffer(0, "_PathPoints", positionsBuffer);
				pathTextureShader.SetFloats("_ChunkPosition", new float[] { chunk.Position.x - terrainPreset.ChunkSize / 2f, chunk.Position.y - terrainPreset.ChunkSize / 2f });
				pathTextureShader.SetTexture(0, "_PathTexture", pathTexture);
				pathTextureShader.Dispatch(0, Mathf.CeilToInt(pathTextureResolution / 8f), Mathf.CeilToInt(pathTextureResolution / 8f), 1);

				positionsBuffer.Release();

				chunk.PathTexture = pathTexture;
				chunk.IsInRangeOfPaths = true;
				chunk.Material.SetColor("_PathColor", pathGenerationPreset.PathColor);

				pathTextures.Add(pathTexture);
				chunk.PathTexture = pathTexture;
			}
			else
			{
				RenderTexture pathTexture = new(1, 1, 0);
				pathTexture.enableRandomWrite = true;
				pathTexture.Create();

				pathTextures.Add(pathTexture);
				chunk.PathTexture = pathTexture;
			}
		}
    }
	List<Vector3> InterpolatePoints(List<Vector3> points, int interpolationMultiplier)
	{
		List<Vector3> result = new();

        for (int i = 1; i < points.Count; i++)
        {
            Vector3 previousPos = points[i - 1];
            Vector3 currentPos = points[i];

            float increment = 1f / interpolationMultiplier;

            for (float j = increment; j < 1; j += increment)
            {
                Vector3 pos = Vector3.Lerp(previousPos, currentPos, j);

                result.Add(pos);
                //Debug.DrawRay(pos, Vector3.up * 100f, Color.red, 1000f);
            }

           // Debug.DrawRay(currentPos, Vector3.up * 100f, Color.red, 1000f);
            result.Add(currentPos);
            if (i == 0)
            {
                //Debug.DrawRay(previousPos, Vector3.up * 100f, Color.red, 1000f);
                result.Add(previousPos);
            }
        }

		return result;
    }
	void CalculateGrassPositions()
	{
		Vector2 centerOffset = new Vector2(
			(terrainPreset.TerrainSize.x * terrainPreset.ChunkSize / 2f) - terrainPreset.ChunkSize / 2f,
			(terrainPreset.TerrainSize.y * terrainPreset.ChunkSize / 2f) - terrainPreset.ChunkSize / 2f
		);

		foreach (BiomePreset biome in biomes)
		{
			foreach (GrassInspectorData grassData in biome.GrassesData)
			{
				for (int y = 0; y < terrainPreset.TerrainSize.y; y++)
				{
					for (int x = 0; x < terrainPreset.TerrainSize.x; x++)
					{
						Vector2Int position = new Vector2Int((int)(x * terrainPreset.ChunkSize - centerOffset.x), (int)(y * terrainPreset.ChunkSize - centerOffset.y));
						for (float offsetY = 0.5f; offsetY <= grassPreset.GrassChunkSizeModifier - 0.5f; offsetY += 1)
						{
							for (float offsetX = 0.5f; offsetX <= grassPreset.GrassChunkSizeModifier - 0.5f; offsetX += 1)
							{
								float heightMapOffsetX = offsetX - 0.5f;
								float heightMapOffsetY = offsetY - 0.5f;
								Vector2 heightMapOffset = new Vector2((int)(heightMapOffsetX * (grassChunkSize)), (int)(heightMapOffsetY * (grassChunkSize)));

								Vector2 rawChunkOffset = new Vector2(offsetX * grassChunkSize, offsetY * grassChunkSize);

                                Vector2 chunkOffset = rawChunkOffset;
								chunkOffset.x -= terrainPreset.ChunkSize / 2f;
								chunkOffset.y -= terrainPreset.ChunkSize / 2f;

								Vector2 chunkPosition = new Vector2(position.x + chunkOffset.x, position.y + chunkOffset.y);
								Bounds bounds = new(new Vector3(chunkPosition.x, 0, chunkPosition.y), new Vector3(grassChunkSizeRounded + 5, terrainPreset.HeightMultiplier, grassChunkSizeRounded + 5));

								Material material = new(grassData.material);
								Chunk chunk = chunks[position];

								//creating args array
								uint[] args = new uint[5];
								args[0] = grassData.mesh.GetIndexCount(0);
								args[1] = 0;

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
								cullGrassUnderWaterShader.SetBool("_IsInRangeOfPaths", chunk.IsInRangeOfPaths);
								cullGrassUnderWaterShader.SetFloats("_WorldOffset", new float[] { rawChunkOffset.x - grassChunkSize * 0.5f, rawChunkOffset.y - grassChunkSize * 0.5f });
								cullGrassUnderWaterShader.SetTexture(0, "_PathTexture", chunk.PathTexture);
								cullGrassUnderWaterShader.SetBuffer(0, "_Properties", meshPropertiesBuffer);
								cullGrassUnderWaterShader.SetBuffer(0, "_VoteBuffer", voteBuffer);
								cullGrassUnderWaterShader.Dispatch(0, Mathf.CeilToInt(grassChunkGrassCount / 128f), 1, 1);

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
							}
						}
					}
				}
			}
		}
	}
	void InstantiateTrees()
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
				{
					for (int x = 0; x < terrainPreset.TerrainSize.x; x++)
					{
						Vector2Int position = new Vector2Int((int)(x * terrainPreset.ChunkSize - centerOffset.x), (int)(y * terrainPreset.ChunkSize - centerOffset.y));
						Chunk chunk = chunks[position];
						for (int i = 0; i < treeData.amount; i++)
						{
							float xOffset = Random.Range(-terrainPreset.ChunkSize / 2f, terrainPreset.ChunkSize / 2f);
							float yOffset = Random.Range(-terrainPreset.ChunkSize / 2f, terrainPreset.ChunkSize / 2f);

							Vector3 rayPosition = new Vector3(position.x + xOffset, 90, position.y + yOffset);

							Ray ray = new(rayPosition, Vector3.down);
							RaycastHit hit;

							if (Physics.Raycast(ray, out hit))
							{
								if (hit.point.y < grassPreset.GrassHeightThreshold) continue;
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
	}
	void InstantiateBridges(int amount)
	{
		int step = riversPoints.Length / amount;
		int start = step / 2;

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
	void GenerateVillagesPositions(int amount, int radius, float villagesMinDistance)
	{
		villagesPositions = new();
		for(int i = 0; i < amount; i++)
		{
			Vector2 halfTerrainSize = terrainSize / 2;
			Vector3 position = new Vector3(Random.Range(-halfTerrainSize.x, halfTerrainSize.x), 140, Random.Range(-halfTerrainSize.y, halfTerrainSize.y));
			Ray ray = new(position, Vector3.down);

			if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("Ground")))
			{	
				Matrix4x4 matrix = Matrix4x4.TRS(hit.point, Quaternion.identity, new Vector3(1, 1, 1));
				Vector2 pos2D = new Vector2(hit.point.x, hit.point.z);

				if (villagesPositions.Where(pos => Vector2.Distance(pos, pos2D) < villagesMinDistance).Count() <= 0 &&
					IsAreaLand(matrix, new Vector2Int(0, 0), new Vector2Int(radius * 2, radius * 2), out float avgHeight, TerrainCheckType.circular))
				{
					villagesPositions.Add(pos2D);
					Debug.DrawRay(ray.origin, Vector3.down * 1000f, Color.magenta, 1000);
				}
				else
				{
					amount++;
				}
			}
		}
	}
	void SpawnVillages(float clearForestRadius)
	{
		foreach(Vector2 villagePos in villagesPositions)
		{
            trees
            .Where(tree =>
            {
                Vector3 treePos = tree.transform.position;
                float distance = Vector2.Distance(new Vector2(treePos.x, treePos.z), villagePos);
                return distance <= clearForestRadius;
            })
            .ToList()
            .ForEach(tree => tree.SetActive(false));

			VillageGenerator.GenerateVillage(villageGenerationPreset, new Vector3(villagePos.x, 20, villagePos.y));
        }
	}
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
					transformedPoint.y = height;
					//Debug.DrawRay(transformedPoint, Vector3.up * 3f, Color.blue, 1000f);

					sum += height;
					amount++;

					if (height <= grassPreset.GrassHeightThreshold)
					{
						avgHeight = sum / amount;
						return false;
					}
				}
			}
		}
		avgHeight = sum / amount;

		return true;
	}
	Dictionary<Vector2, List<Vector2>> connectedChunks = new();
	void GeneratePaths()
	{
		for (int i = 0; i < villagesPositions.Count; i++)
		{
			Vector2 currentVillagePos = villagesPositions[i];
			Vector2[] villagePos = villagesPositions.Where(village => village != currentVillagePos).OrderBy(village => Vector2.Distance(village, currentVillagePos)).ToArray();

			Vector2 closestVillagePos = villagePos[0];

			int index = 1;
			while (connectedChunks.ContainsKey(closestVillagePos) && connectedChunks[closestVillagePos].Contains(currentVillagePos) && i < villagePos.Length)
			{
				closestVillagePos = villagePos[index++];
			}

			if (!connectedChunks.ContainsKey(currentVillagePos)) connectedChunks[currentVillagePos] = new();
            connectedChunks[currentVillagePos].Add(closestVillagePos);

            Color color = Random.ColorHSV();
			float height = Random.Range(50, 300);


			Debug.DrawRay(new Vector3(currentVillagePos.x, 0, currentVillagePos.y), Vector3.up * height, color, 1000f);
            Debug.DrawRay(new Vector3(closestVillagePos.x, 0, closestVillagePos.y), Vector3.up * height, color, 1000f);

            List<Node> pathPoints = pathfinding.FindPath(currentVillagePos, closestVillagePos);
			foreach (Node node in pathPoints)
			{
				pathPositions.Add(node.WorldPos);
			}
		}
	}
	private void Update()
	{
		if (!areMeshesInstanced) return;
		if (!generateGrass) return;

		RenderAnimationTexture();
		RenderGrassMeshes();
	}
	void RenderAnimationTexture()
	{
		animationMapShader.SetFloat("_Time", Time.time * grassPreset.GrassAnimationPreset.WindSpeed);
		animationMapShader.Dispatch(0, animationTextureResolution, animationTextureResolution, 1);
	}
	void RenderGrassMeshes()
	{
		Vector3 playerPos = playerTransform.position;
		Vector2 playerPos2D = new Vector2(playerPos.x, playerPos.z);

        foreach (KeyValuePair<Vector2, GrassData> pair in grassesData["FlatTerrain"])
		{
			GrassData data = pair.Value;

			if (Vector2.Distance(pair.Key, playerPos2D) < grassPreset.GrassRenderDistance * grassChunkSizeRounded)
			{
				Graphics.DrawMeshInstancedIndirect(data.Mesh, 0, data.Material, data.Bounds, data.ArgsBuffer);
			}
		}
	}
	void OnDisable()
	{
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

        FreePathTextures();

        voteBuffer.Release();
		prescanBuffer.Release();
		offsetBuffer.Release();
		groupSumBuffer.Release();
		
		voteBuffer = null;
		prescanBuffer = null;
		offsetBuffer = null;
		groupSumBuffer = null;

        heightsBuffer.Release();
        heightsBuffer = null;

        EventManager.OnChunksGenerationCompleated.RemoveListener(AssignDependencies);
        EventManager.OnGeneratedPathfindingGrid.RemoveListener(() => { isRenderingEnabled = true; });
        EventManager.OnPathPresetValidate.RemoveListener(GeneratePathTextures);
    }
	void FreePathTextures()
	{
        if (pathTextures != null)
        {
            foreach (RenderTexture pathTexture in pathTextures)
            {
                pathTexture.Release();
            }
        }
        pathTextures = null;
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
        public string BiomeName { get; set; }
        public Mesh Mesh { get; set; }
		public Material Material { get; set; }
		public Bounds Bounds { get; set; }
		public ComputeBuffer ArgsBuffer { get; set; }
        public GrassData(string biomeName, Mesh mesh, Material material, Bounds bounds, ComputeBuffer argsBuffer)
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

