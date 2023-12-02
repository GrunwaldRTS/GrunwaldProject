using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateWaterMesh : MonoBehaviour
{
    [SerializeField] Material waterMaterial;
    [SerializeField] int width;
    [SerializeField] int height;
    public void GenerateMesh()
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

        Mesh mesh = MeshGenerator.GenerateRectangle(width, height);
        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial = waterMaterial;
    }
}
