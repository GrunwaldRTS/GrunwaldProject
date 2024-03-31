using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PathGenerationPreset", menuName = "ScriptableObjects/ProceduralGeneration/PathGenerationPreset")]
public class PathGenerationPreset : ScriptableObject
{
    [SerializeField][Range(1, 3)] int _pathInterpolation = 2;
    [SerializeField][Range(1, 100)] float _scale;
    [SerializeField][Range(1, 10)] int _octaves;
    [SerializeField][Range(1, 10)] float _lacunarity;
    [SerializeField][Range(0, 1)] float _persistance;
    [SerializeField] Vector3 _offsetIncrement;
    [SerializeField][Range(0, 1)] float _noiseImpact;
    [SerializeField] Color _pathColor;

    public int PathInterpolation { get => _pathInterpolation; private set => _pathInterpolation = value; }
    public float Scale { get => _scale; private set => _scale = value; }
    public int Octaves { get => _octaves; private set => _octaves = value; }
    public float Lacunarity { get => _lacunarity; private set => _lacunarity = value; }
    public float Persistance { get => _persistance; private set => _persistance = value; }
    public Vector3 OffsetIncrement { get => _offsetIncrement; private set => _offsetIncrement = value; }
    public float NoiseImpact { get => _noiseImpact; private set => _noiseImpact = value; }
    public Color PathColor { get => _pathColor; private set => _pathColor = value; }

    private void OnValidate()
    {
        EventManager.OnPathPresetValidate.Invoke();
    }
}
