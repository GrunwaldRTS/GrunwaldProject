using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public static class Noise
{
	public static float GetNoiseValue(Vector2 pos, float scale, int seed)
	{
		System.Random rand = new System.Random(seed);

		float xOffset = rand.Next(-100000, 100000);
		float yOffset = rand.Next(-100000, 100000);

		float sampleX = (pos.x + xOffset) / scale;
		float sampleY = (pos.y + yOffset) / scale;

		return Mathf.PerlinNoise(sampleX, sampleY);
	}
	public static float GetNoiseValue(Vector2 pos, Vector2 scale, int seed)
	{
		System.Random rand = new System.Random(seed);

		float xOffset = rand.Next(-100000, 100000);
		float yOffset = rand.Next(-100000, 100000);

		float sampleX = (pos.x + xOffset) / scale.x;
		float sampleY = (pos.y + yOffset) / scale.y;

		return Mathf.PerlinNoise(sampleX, sampleY);
	}
	public static Texture2D TransformToTexture(float[,] heightMap, int width, int height)
	{
		int mapHeight = heightMap.GetLength(0);
		int mapWidth = heightMap.GetLength(1);

		Texture2D result = new(mapHeight, mapWidth);
		result.filterMode = FilterMode.Point;
		result.wrapMode = TextureWrapMode.Clamp;

		for (int y = 1; y < mapWidth - 1; y++)
		{
			int yrev = mapWidth - 2 - y;
			for(int x = 1; x < mapHeight - 1; x++)
			{
				int xrev = mapHeight - 2 - x;

				float value = heightMap[xrev, yrev];
				Color color = new Color(value, value, value);
				result.SetPixel(x, y, color);
			}
		}
		result.Apply();

		return result;
	}
	public static float[] TransformTo1DArray(float[,] heightMap)
	{
		int height = heightMap.GetLength(0);
		int width = heightMap.GetLength(1);

		int newHeight = height - 2;
		int newWidth = width - 2;

		float[] result = new float[newHeight * newWidth];

		int xIndex = 0;
		int yIndex = 0;
		for (int y = 1; y < width - 1; y++)
		{
			int yrev = newHeight - y;
			for (int x = 1; x < height - 1; x++)
			{
				int xrev = newHeight - x;
				float value = heightMap[xrev, yrev];
				result[xIndex + yIndex * (newWidth - 1)] = value;
				xIndex++;
				if (xIndex == width - 2) xIndex = 0;
			}
			yIndex++;
		}

		return result;
	}
	public static float[,] GenerateChunkMap(NoiseInputData noiseInputData, Vector2Int offset, Vector2Int[] riversPoints, out Dictionary<Vector2Int, float> globalHeightMap)
	{
		noiseInputData.Scale = noiseInputData.Scale == 0 ? 0.001f : noiseInputData.Scale;
		noiseInputData.Size += 2;

		System.Random rand = new System.Random(noiseInputData.Seed);
		Vector2[] octavesOffsets = new Vector2[noiseInputData.Octaves];

		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;

		for (int i = 0; i < noiseInputData.Octaves; i++)
		{
			float offsetX = rand.Next(-100000, 100000) - offset.x;
			float offsetY = rand.Next(-100000, 100000) - offset.y;
			octavesOffsets[i] = new Vector2(offsetX, offsetY);

			maxPossibleHeight += amplitude;
			amplitude *= noiseInputData.Persistance;
		}

		Dictionary<Vector2Int, float> riversMap = new();

		bool isInRange = IsChunkInRangeOfRivers(offset, noiseInputData, riversPoints);

		if (isInRange)
		{
			riversMap = GetVertexRiverHeight(noiseInputData, offset, riversPoints);
		}

		float halfSize = noiseInputData.Size / 2;

		float[,] heightMap = new float[noiseInputData.Size, noiseInputData.Size];
		globalHeightMap = new();

		for (int y = 0; y < noiseInputData.Size; y++)
		{
			for (int x = 0; x < noiseInputData.Size; x++)
			{
				float noiseValue = 0;
				frequency = 1;
				amplitude = 1;

				Vector2Int worldPos = new Vector2Int(offset.x - x + (int)halfSize, offset.y - y + (int)halfSize);

				for (int i = 0; i < noiseInputData.Octaves; i++)
				{
					float sampleX = (x - halfSize + octavesOffsets[i].x) / noiseInputData.Scale * frequency;
					float sampleY = (y - halfSize + octavesOffsets[i].y) / noiseInputData.Scale * frequency;

					float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
					noiseValue += perlinValue * amplitude;

					if (i == 1 && isInRange)
					{
						noiseValue *= riversMap[worldPos];
					}

					frequency *= noiseInputData.Lacunarity;
					amplitude *= noiseInputData.Persistance;
				}

				heightMap[x, y] = noiseValue;
				globalHeightMap[worldPos] = noiseValue * noiseInputData.HeightMultiplier;
			}
		}

		for (int y = 0; y < noiseInputData.Size; y++)
		{
			for (int x = 0; x < noiseInputData.Size; x++)
			{
				float normalizedHeight = (float)Math.Round((double)(heightMap[x, y] - 0.4f) / (maxPossibleHeight * 0.42f), 3);
				heightMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
			}
		}
		return heightMap;
	}
	static bool IsChunkInRangeOfRivers(Vector2Int offset, NoiseInputData noiseInputData, Vector2Int[] riversPoints)
	{
		int halfSize = noiseInputData.Size / 2;

		bool isInRange = false;

		Vector2Int xBounds = new Vector2Int(offset.x - halfSize, offset.x + halfSize);
		Vector2Int yBounds = new Vector2Int(offset.y - halfSize, offset.y + halfSize);

		foreach (Vector2Int point in riversPoints)
		{
			bool isXInRange = point.x > xBounds.x - noiseInputData.RiversWidth && point.x < xBounds.y + noiseInputData.RiversWidth;
			bool isYInRange = point.y > yBounds.x - noiseInputData.RiversWidth && point.y < yBounds.y + noiseInputData.RiversWidth;

			if (isXInRange && isYInRange) isInRange = true;
		}

		return isInRange;
	}
	static bool IsPointInRangeOfChunk(Vector2Int offset, NoiseInputData noiseInputData, Vector2Int riversPoint)
	{
		int halfSize = noiseInputData.Size / 2;

		bool isInRange = false;

		Vector2Int xBounds = new Vector2Int(offset.x - halfSize, offset.x + halfSize);
		Vector2Int yBounds = new Vector2Int(offset.y - halfSize, offset.y + halfSize);

		bool isXInRange = riversPoint.x > xBounds.x - noiseInputData.RiversWidth && riversPoint.x < xBounds.y + noiseInputData.RiversWidth;
		bool isYInRange = riversPoint.y > yBounds.x - noiseInputData.RiversWidth && riversPoint.y < yBounds.y + noiseInputData.RiversWidth;
		if (isXInRange && isYInRange) isInRange = true;

		return isInRange;
	}
	private static Dictionary<Vector2Int, float> GetVertexRiverHeight(NoiseInputData noiseInputData, Vector2Int offset, Vector2Int[] riversPoints)
	{
		AnimationCurve curve = new(noiseInputData.RiversHeightCurve.keys);
		Dictionary<Vector2Int, float> result = new();

		int halfSize = noiseInputData.Size / 2;

		Vector2Int[] riversPointsForThisChunk = riversPoints.Where(point => IsPointInRangeOfChunk(offset, noiseInputData, point)).ToArray();

		for (int y = 0; y < noiseInputData.Size; y++)
		{
			for (int x = 0; x < noiseInputData.Size; x++)
			{
				Vector2Int worldPos = new Vector2Int(offset.x - x + halfSize, offset.y - y + halfSize);
				bool isInRange = false;
				float minDistance = 100;

				foreach (Vector2Int point in riversPointsForThisChunk)
				{
					float distance = Vector2Int.Distance(worldPos, point);
					if (distance < minDistance && distance <= noiseInputData.RiversWidth)
					{
						isInRange = true;
						minDistance = distance;
					}
				}

				if (isInRange)
				{
					result[worldPos] = curve.Evaluate(minDistance / noiseInputData.RiversWidth);
					continue;
				}

				result[worldPos] = 1;
			}
		}

		return result;
	}
	public static Vector2Int[] GetRiversPoints(NoiseInputData noiseInputData)
	{
		List<Vector2Int> riversPoints = new();

		Vector2Int terrainSize = new Vector2Int(noiseInputData.TerrainSize.x, noiseInputData.TerrainSize.y) * noiseInputData.Size;
		Vector2Int halfTerrainSize = terrainSize / 2;
		halfTerrainSize.x += 1;
		halfTerrainSize.y += 1;

		for (int i = 0; i < noiseInputData.RiversCount; i++)
		{
			Vector2Int startPoint = new Vector2Int(0, -400);
			River river = new(startPoint, new Vector2Int(240, 240), new Vector2(800, 800), new Vector2Int(80, 150), new Vector2Int(30, 90));

			for (int j = 0; j < river.Points.Count - 1; j++)
			{
				Position firstPosition = river.Points[j];
				Position secondPosition = river.Points[j + 1];

				Vector2Int firstPoint = firstPosition.GlobalPos;
				Vector2Int secondPoint = secondPosition.GlobalPos;

				Debug.DrawLine(new Vector3(firstPoint.x, 20, firstPoint.y), new Vector3(secondPoint.x, 20, secondPoint.y), Color.blue, 100f);
				Debug.DrawRay(new Vector3(firstPoint.x, 0, firstPoint.y), Vector3.up * 30f, Color.red, 100f);

				foreach (Vector2Int point in BresenhamAlgorithm(firstPosition.GlobalPos, secondPosition.GlobalPos))
				{
					riversPoints.Add(point);
				}
			}
		}

		return riversPoints.ToArray();
	}
	public static Vector2Int[] BresenhamAlgorithm(Vector2Int startPos, Vector2Int endPos)
	{
		List<Vector2Int> result = new();
		
		int deltaX = endPos.x - startPos.x;
		int deltaY = endPos.y - startPos.y;

		int xStep = deltaX >= 0 ? 1 : -1;
		int yStep = deltaY >= 0 ? 1 : -1;
		
		deltaX = Mathf.Abs(deltaX);
		deltaY = Mathf.Abs(deltaY);

		if (deltaX > deltaY)
		{
			float slope = (float)deltaY / deltaX;

			float error = 0;
			int y = startPos.y;

			for (int x = startPos.x; x != endPos.x; x += xStep)
			{
				result.Add(new Vector2Int(x, y));
				error += slope; 

				if (error > 0.5f)
				{
					y += yStep;
					float sign = Mathf.Sign(yStep);
					error -= sign * yStep;
				}
			}
		}
		else
		{
			float slope = (float)deltaX / deltaY;

			float error = 0;
			int x = startPos.x;

			for (int y = startPos.y; y != endPos.y; y += yStep)
			{
				result.Add(new Vector2Int(x, y));
				error += slope;

				if (error > 0.5f)
				{
					x += xStep;
					float sign = Mathf.Sign(xStep);
					error -= sign * xStep;
				}
			}
		}

		return result.ToArray();
	}
	private class River
	{
        public List<Position> Points { get; private set; }
        public float Angle { get; private set; }
		public Vector3 DirVector { get; private set; }
        public float Length { get; private set; }
		public Matrix4x4 Matrix { get; private set; }
        public River(Vector2Int startPos, Vector2Int minMaxAngle, Vector2 minMaxLength, Vector2Int minMaxFrequency,
			Vector2Int minMaxAmplitude)
        {
			Angle = NextFloat(minMaxAngle.x, minMaxAngle.y);
			DirVector = new Vector3(Mathf.Cos(Angle), 0, Mathf.Sin(Angle));
			Length = NextFloat(minMaxLength.x, minMaxLength.y);

			Matrix = Matrix4x4.TRS(new Vector3(startPos.x, 0, startPos.y), Quaternion.Euler(0, Angle + 139, 0), new Vector3(1, 1, 1));

			//Debug.Log(startPos);
			//Debug.Log(new Position(startPos, Matrix).FullGlobalPos);

			Points = new();

			GeneratePoints(minMaxFrequency, minMaxAmplitude);
		}
		void GeneratePoints(Vector2Int minMaxFrequency, Vector2Int minMaxAmplitude)
		{
			int sign = 1;
			int y = 0;

			while (y < Length)
			{
				sign = -sign;

				System.Random random = new();

				int frequency = random.Next(minMaxFrequency.x, minMaxFrequency.y);
				int amplitude = random.Next(minMaxAmplitude.x, minMaxAmplitude.y);

				Vector2 p0 = new Vector2(0, y);
				Vector2 p1 = new Vector2(sign * amplitude, y + 40);
				Vector2 p3 = new Vector2(0, y + frequency);
				Vector2 p2 = new Vector2(sign * amplitude, p3.y - 40);

				for (float t = 0; t < 1; t += 0.1f)
				{
					Vector2 a = Vector2.Lerp(p0, p1, t);
					Vector2 b = Vector2.Lerp(p1, p2, t);
					Vector2 c = Vector2.Lerp(p2, p3, t);
					Vector2 d = Vector2.Lerp(a, b, t);
					Vector2 e = Vector2.Lerp(b, c, t);
					Vector2 p = Vector2.Lerp(d, e, t);

					Vector2Int position = new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));

					Points.Add(new Position(position, Matrix));
				}

				y += frequency;
			}
		}
    }
	private struct Position
	{
		public Vector2Int GlobalPos { get; private set; }
        public Vector3 FullGlobalPos { get; private set; }
        public Vector2Int LocalPos { get; private set; }
        public Vector3 FullLocalPos { get; private set; }
        public Matrix4x4 Matrix { get; private set; }
        public Position(Vector3 fullLocalPos, Matrix4x4 transformMatrix)
		{
			Matrix = transformMatrix;
			FullLocalPos = fullLocalPos;
			LocalPos = new Vector2Int(Mathf.RoundToInt(FullLocalPos.x), Mathf.RoundToInt(FullLocalPos.y));
			FullGlobalPos = Matrix.MultiplyPoint3x4(FullLocalPos);
			GlobalPos = new Vector2Int(Mathf.RoundToInt(FullGlobalPos.x), Mathf.RoundToInt(FullGlobalPos.z));
		}
		public Position(Vector2Int localPos, Matrix4x4 transformMatrix)
		{
			Matrix = transformMatrix;
			LocalPos = localPos;
			FullLocalPos = new Vector3(LocalPos.x, 0, LocalPos.y);
			FullGlobalPos = Matrix.MultiplyPoint3x4(FullLocalPos);
			GlobalPos = new Vector2Int(Mathf.RoundToInt(FullGlobalPos.x), Mathf.RoundToInt(FullGlobalPos.z));
		}
	}
	static float NextFloat(float min, float max)
	{
		System.Random random = new System.Random();
		double val = (random.NextDouble() * (max - min) + min);
		return (float)val;
	}
}
