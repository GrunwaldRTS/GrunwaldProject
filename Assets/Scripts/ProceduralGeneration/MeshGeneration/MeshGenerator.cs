using UnityEngine;

public static class MeshGenerator
{
	public static Mesh GenerateRectangle(int width, int height)
	{
		Vector2 centerOffset = new Vector2(width / 2f, height / 2f);
		Vector3[] vertices = new Vector3[width * height];
		Vector2[] uvs = new Vector2[width * height];
		int[] triangles = new int[(width - 1) * (height - 1) * 6];

		int verticesIndex = 0;
		int trianglesIndex = 0;
		for(int y = 0; y < height; y += 1)
		{
			for(int x = 0; x < width; x += 1)
			{
				Vector3 vertex = new Vector3(x - centerOffset.x, 0, y - centerOffset.y);
				Vector2 uv = new Vector2(x / (float)width, y / (float)height);
				uvs[verticesIndex] = uv;
				vertices[verticesIndex] = vertex;

				if (x < width - 1 && y < height - 1)
				{
					int a = verticesIndex;
					int b = verticesIndex + 1;
					int c = verticesIndex + width;
					int d = verticesIndex + width + 1;

					triangles[trianglesIndex++] = c;
					triangles[trianglesIndex++] = d;
					triangles[trianglesIndex++] = a;
					triangles[trianglesIndex++] = d;
					triangles[trianglesIndex++] = b;
					triangles[trianglesIndex++] = a;
				}
				verticesIndex++;
			}
		}
		Mesh mesh = new Mesh();
		mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		//mesh.normal
		//mesh.uv

		mesh.RecalculateNormals();

		
		return mesh;
	}
	public static MeshData GenerateTerrainChunkData(MeshInputData meshInputData)
    {
		int meshSimplificationIncrement = meshInputData.LevelOfDetail == 0? 1 : meshInputData.LevelOfDetail * 2;

		//VPL - verticies per line
		//u - unsimplified
		int BorderedVPL = meshInputData.HeightMap.GetLength(0);
		int modifiedVPL = BorderedVPL - 2 * meshSimplificationIncrement;
		int VPl = BorderedVPL - 2;

        float centerOffset = (VPl - 1) / 2;

		int sVPL = (VPl - 1) / meshSimplificationIncrement + 1;

		int[,] vertexIndeciesMap = new int[BorderedVPL, BorderedVPL];

		MeshData data = new(sVPL - 1);

		int verteciesIndex = 0;
		int borderedVerticiesIndex = -1;

		for (int y = 0; y < BorderedVPL; y += meshSimplificationIncrement)
		{
            for (int x = 0; x < BorderedVPL; x += meshSimplificationIncrement)
            {
				//Debug.Log($"x: {x}, y: {y}");
				bool isBorderVertex = x == 0 || y == 0 || x == BorderedVPL - 1 || y == BorderedVPL - 1;

				if (isBorderVertex)
				{
					vertexIndeciesMap[x, y] = borderedVerticiesIndex--;
				}
				else
				{
					vertexIndeciesMap[x, y] = verteciesIndex++;
				}
			}
		}

		for (int y = 0; y < BorderedVPL; y += meshSimplificationIncrement)
		{
			for (int x = 0; x < BorderedVPL; x += meshSimplificationIncrement)
			{
				int vertexIndex = vertexIndeciesMap[x, y];
				Vector2 uv = new Vector2((x - meshSimplificationIncrement) / (float)modifiedVPL, (y - meshSimplificationIncrement) / (float)modifiedVPL);

				float height = meshInputData.HeightMap[x, y] * meshInputData.HeightMultiplier;

				//Debug.Log(height);
				//Debug.Log("lakeEndHeight: " + lakeEndHeight);

				//height *= height <= lakeEndHeight? -lakeHeightMultiplier : meshInputData.HeightMultiplier;

				Vector3 vertex = new Vector3(centerOffset - uv.x * VPl, height, centerOffset - uv.y * VPl);

				data.AddVertex(vertex, uv, vertexIndex);

				if (x < BorderedVPL - meshSimplificationIncrement && y < BorderedVPL - meshSimplificationIncrement)
				{
					//Debug.Log($"x: {x}, y: {y}");
					int a = vertexIndeciesMap[x, y];
					int b = vertexIndeciesMap[x + meshSimplificationIncrement, y];
					int c = vertexIndeciesMap[x, y + meshSimplificationIncrement];
					int d = vertexIndeciesMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];

					data.AddTriangle(c, d, a);
					data.AddTriangle(d, b, a);
				}
				verteciesIndex++;
			}
		}

        return data;
    }
}
