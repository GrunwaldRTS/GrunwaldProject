using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class VillageGenerator
{
    static int villagesCount;
    static VillageGenerationPreset preset;
    static List<Vector3> previousStructures = new();
    public enum StructureType
    {
        MainHall = 0,
        House
    }
    public static GameObject GenerateVillage(VillageGenerationPreset villageGenerationPreset, Vector3 position)
    {
        previousStructures.Clear();

        preset = villageGenerationPreset;

        GameObject village = new("Village " + villagesCount++);
        village.transform.position = position;
        //Debug.Log(position);
        //Debug.Log(village.transform.position);

        foreach(VillageStructure structure in villageGenerationPreset.Structures)
        {
            int amount = Random.Range(structure.MinAmount, structure.MaxAmoount);

            for(int i = 0; i < amount; i++)
            {
                GameObject go = GenerateStructure(structure, village.transform);
                //go.transform.position;
            }
        }

        return village;
    }
    static GameObject GenerateStructure(VillageStructure structure, Transform parent)
    {
        Vector3 rotation = new(0, Random.Range(0, 180), 0);
        Vector3 position = Vector3.zero;

        while (true)
        {
            position = GetRandomPositionInRadius(preset.VillageRadius);

            bool shouldBreak = true;

            foreach(Vector3 previousStructure in previousStructures)
            {
                if (Vector3.Distance(previousStructure, position) < preset.MinRadiusBetweenStructures)
                {
                    shouldBreak = false;
                }
            }

            if (shouldBreak) break;
        }

        previousStructures.Add(position);

        //GameObject cube = (GameObject)PrefabUtility.InstantiatePrefab(structure.Prefab, parent);
        //cube.transform.parent = parent;
        //cube.transform.rotation = Quaternion.Euler(rotation);
        //cube.transform.localPosition = position;

        return new GameObject();
    }
    static GameObject GenerateCube(Vector3 size)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale = size;

        return cube;
    }
    static Vector3 GetRandomPositionInRadius(float radius)
    {
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(
            Vector3.zero,
            Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)),
            Vector3.one
        );
        Vector3 position = Vector3.right * Random.Range(0, radius);
        Vector3 transformedPosition = rotationMatrix.MultiplyPoint3x4(position);

        return transformedPosition;
    }
}

