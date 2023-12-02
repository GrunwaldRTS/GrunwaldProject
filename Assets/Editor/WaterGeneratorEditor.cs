using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GenerateWaterMesh))]
public class WaterGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		GenerateWaterMesh script = (GenerateWaterMesh)target;

		base.OnInspectorGUI();

		if (GUILayout.Button("GenerateMesh"))
		{
			script.GenerateMesh();
		}
	}
}
