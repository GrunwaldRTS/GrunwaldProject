using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal struct GrassData
{
	public Mesh Mesh { get; set; }
	public Material Material { get; set; }
	public List<Matrix4x4[]> MatriciesBatches { get; set; }
    public Vector3 Scale { get; set; }
    public GrassData(Mesh mesh, Material material, List<Matrix4x4[]> matriciesBatches, Vector3 scale)
	{
		Mesh = mesh;
		Material = material;
		MatriciesBatches = matriciesBatches;
		Scale = scale;
	}
}