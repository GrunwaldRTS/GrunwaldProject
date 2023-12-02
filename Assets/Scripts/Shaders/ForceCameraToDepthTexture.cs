using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceCameraToDepthTexture : MonoBehaviour
{
	Camera cam;
	private void Awake()
	{
		cam = GetComponent<Camera>();
		cam.depthTextureMode = DepthTextureMode.Depth;
	}
}
