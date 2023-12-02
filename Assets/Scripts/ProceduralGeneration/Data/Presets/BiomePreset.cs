using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomePreset", menuName = "ScriptableObjects/BiomePreset")]
public class BiomePreset : ScriptableObject
{
	[SerializeField] string biomeName;
	[SerializeField] GrassInspectorData[] grassesData;
	[SerializeField] TreeInspectorData[] treesData;
	[SerializeField] Material terrainMaterial;
	public string BiomeName { get => biomeName; }
    public GrassInspectorData[] GrassesData { get => grassesData; }
	public TreeInspectorData[] TreesData { get => treesData; }
	public Material TerrainMaterial { get => terrainMaterial; }
}