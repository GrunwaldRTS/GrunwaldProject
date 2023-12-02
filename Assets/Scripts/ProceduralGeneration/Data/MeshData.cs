using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeshData
{
    public int Size { get; private set; }
    public Vector3[] Verticies { get; private set; }
    public Vector2[] Uvs { get; private set; }
    public int[] Triangles { get; private set; }
	public Vector3[] Normals { get; set; }
    public Vector3[] BorderVerticies { get; private set; }
    public int[] BorderTriangles { get; private set; }

    int trianglesIndex;

	int borderVerticiesIndex;
	int borderTrianglesIndex;
	public MeshData(int size)
	{
		Size = size;
		Verticies = new Vector3[(size + 1) * (size + 1)];
		Uvs = new Vector2[Verticies.Length];
		Triangles = new int[size * size * 6];

		BorderVerticies = new Vector3[(size + 1) * 4 + 4];
		BorderTriangles = new int[(size + 1) * 24];
	}
	public void AddVertex(Vector3 vertex, Vector2 uv, int vertexIndex)
	{
		if(vertexIndex < 0)
		{
			BorderVerticies[-vertexIndex - 1] = vertex;
		}
		else
		{
			Verticies[vertexIndex] = vertex;
			Uvs[vertexIndex] = uv;
		}
	}
	public void AddTriangle(int a, int b, int c)
	{
		if(a < 0 || b < 0 || c < 0)
		{
			BorderTriangles[borderTrianglesIndex++] = a;
			BorderTriangles[borderTrianglesIndex++] = b;
			BorderTriangles[borderTrianglesIndex++] = c;
		}
		else
		{
			Triangles[trianglesIndex++] = a;
			Triangles[trianglesIndex++] = b;
			Triangles[trianglesIndex++] = c;
		}
	}
	public Vector3[] CalculateNormals()
	{
		Vector3[] vertexNormals = new Vector3[Verticies.Length];

		for(int i = 0; i < Triangles.Length; i += 3)
		{
			int vertexIndexA = Triangles[i];
			int vertexIndexB = Triangles[i + 1];
			int vertexIndexC = Triangles[i + 2];

			Vector3 surfaceNormal = SurfaceNormalFromIndecies(vertexIndexA, vertexIndexB, vertexIndexC);
			vertexNormals[vertexIndexA] += surfaceNormal;
			vertexNormals[vertexIndexB] += surfaceNormal;
			vertexNormals[vertexIndexC] += surfaceNormal;
		}

		for (int i = 0; i < BorderTriangles.Length; i += 3)
		{
			int vertexIndexA = BorderTriangles[i];
			int vertexIndexB = BorderTriangles[i + 1];
			int vertexIndexC = BorderTriangles[i + 2];

			Vector3 surfaceNormal = SurfaceNormalFromIndecies(vertexIndexA, vertexIndexB, vertexIndexC);
			if(vertexIndexA >= 0)
			{
				vertexNormals[vertexIndexA] += surfaceNormal;
			}
			if (vertexIndexB >=0)
			{
				vertexNormals[vertexIndexB] += surfaceNormal;
			}
			if(vertexIndexC >= 0)
			{
				vertexNormals[vertexIndexC] += surfaceNormal;
			}
		}

		foreach (Vector3 vertex in Verticies)
		{
			vertex.Normalize();
		}

		return vertexNormals;
	}
	Vector3 SurfaceNormalFromIndecies(int indexA, int indexB, int indexC)
	{
		Vector3 pointA = indexA < 0? BorderVerticies[-indexA - 1] : Verticies[indexA];
		Vector3 pointB = indexB < 0? BorderVerticies[-indexB - 1] : Verticies[indexB];
		Vector3 pointC = indexC < 0? BorderVerticies[-indexC - 1] : Verticies[indexC];

		Vector3 ab = pointB - pointA;
		Vector3 ac = pointC - pointA;

		return Vector3.Cross(ab, ac).normalized;
	}
	public Mesh CreateMesh()
	{
		Mesh mesh = new();

		mesh.name = "ChunkMesh";
		mesh.vertices = Verticies;
		mesh.triangles = Triangles;
		mesh.normals = CalculateNormals();

		return mesh;
	}
}
