using UnityEngine;

[CreateAssetMenu(fileName = "VillageGenerationPreset", menuName = "ScriptableObjects/ProceduralGeneration/VillageGeneration/VillageGenerationPreset")]
public class VillageGenerationPreset : ScriptableObject
{
    [SerializeField] private float _villageRadius = 40;
    [SerializeField] private float _minRadiusBetweenStructures = 5;
    [SerializeField] private VillageStructure[] _structures;
    public float VillageRadius { get => _villageRadius; private set => _villageRadius = value; }
    public float MinRadiusBetweenStructures { get => _minRadiusBetweenStructures; private set => _minRadiusBetweenStructures = value; }
    public VillageStructure[] Structures { get => _structures; private set => _structures = value; }
}