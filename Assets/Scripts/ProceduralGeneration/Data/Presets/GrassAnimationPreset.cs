using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GrassAnimationPreset", menuName = "ScriptableObjects/ProceduralGeneration/TerrainGeneration/GrassAnimationPreset")]
public class GrassAnimationPreset : ScriptableObject
{
    [SerializeField] float _windSpeed = 0.6f;
    [SerializeField] float _windFrequency = 2f;
    [SerializeField] float _windAmplitude = 0.7f;
    [SerializeField] float _addValue = 0;

    public float WindSpeed { get => _windSpeed; private set => _windSpeed = value; }
    public float WindFrequency { get => _windFrequency; private set => _windFrequency = value; }
    public float WindAmplitude { get => _windAmplitude; private set=> _windAmplitude = value; }
    public float AddValue { get => _addValue; private set => _addValue = value; }
}
