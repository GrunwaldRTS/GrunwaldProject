using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CDPUpdate : MonoBehaviour
{
    public ChunkDataProvider CDP { get; set; }
    void Update()
    {
        CDP.UpdateRequestQueue();
    }
}
