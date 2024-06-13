using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AStarPathfinding
{
    [CreateAssetMenu(fileName = "PathfindingGridPreset", menuName = "ScriptableObjects/AStarPathfinding/PathfindingGridPreset")]
    public class PathfindingGridPreset : ScriptableObject
    {
        [SerializeField] ProceduralTerrainPreset _terrainPreset;
        [SerializeField] int _simplification = 3;
        [SerializeField] int _waterMargin = 0;

        public ProceduralTerrainPreset TerrainPreset { get => _terrainPreset; private set => _terrainPreset = value; }
        public int Simplification { get => _simplification; private set => _simplification = value; }
        public int WaterMargin { get => _waterMargin; private set => _waterMargin = value; }
    }
}
