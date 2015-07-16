using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[RequireComponent( typeof( MeshCollider ) )]
public class Rail : MonoBehaviour
{
    public List<Vector3> nodes = new List<Vector3>();

    [HideInInspector]
    public GameObject nodesContainer;
}
