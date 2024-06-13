using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GrassPreset", menuName = "ScriptableObjects/ProceduralGeneration/TerrainGeneration/GrassPreset")]
public class GrassPreset : ScriptableObject
{
    [SerializeField] float _grassHeightThreshold = 13.5f;
    [SerializeField][Range(1, 8)] int _grassChunkSizeModifier = 8;
    [SerializeField][Range(1, 8)] int _density = 2;
    [SerializeField] GrassAnimationPreset _grassAnimationPreset;
    [SerializeField][Range(1, 10)] int _grassRenderDistance = 7;

    public float GrassHeightThreshold { get => _grassHeightThreshold; private set => _grassHeightThreshold = value; }
    public int GrassChunkSizeModifier { get => _grassChunkSizeModifier; private set => _grassChunkSizeModifier = value; }
    public int Density { get => _density; private set => _density = value; }
    public GrassAnimationPreset GrassAnimationPreset { get => _grassAnimationPreset; private set => _grassAnimationPreset = value; }
    public int GrassRenderDistance { get => _grassRenderDistance; private set => _grassRenderDistance = value; }
}