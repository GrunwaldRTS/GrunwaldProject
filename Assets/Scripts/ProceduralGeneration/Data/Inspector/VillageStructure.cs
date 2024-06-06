using System;
using UnityEngine;

[Serializable]
public struct VillageStructure
{
    [SerializeField] VillageGenerator.StructureType _type;
    [SerializeField] GameObject _prefab;
    [SerializeField][Range(1, 10)] int _minAmount;
    [SerializeField][Range(1, 10)] int _maxAmount;

    public VillageGenerator.StructureType Type { get => _type; private set => _type = value; }
    public GameObject Prefab { get => _prefab; private set => _prefab = value; }
    public int MinAmount { get => _minAmount; private set => _minAmount = value; }
    public int MaxAmoount { get => _maxAmount; private set => _maxAmount = value; }
}