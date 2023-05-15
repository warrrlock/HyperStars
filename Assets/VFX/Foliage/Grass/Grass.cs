using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Grass : MonoBehaviour
{
    [Header("Generation")]
    [Min(0)] public int count = 500;
    [Min(0.01f)] public float radius = 5;
    public float width = 1;
    public float widthVariation = 0.5f;
    public float heightVariation = 0.5f;
    public int meshResolution = 3;
    public int randomSeed = 12345;

    // public GameObject displacer;

    [ContextMenu("Update Mesh")]
    void UpdateMesh()
    {
        UnityEngine.Random.InitState(randomSeed);

        Mesh mesh = new Mesh();
        mesh.name = "Generated Grass Mesh";
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        GrassMeshBuilder.CreateGrass(mesh, count, radius, width, widthVariation, heightVariation, meshResolution);
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void Update()
    {
        // Renderer r = GetComponent<Renderer>();
        // r.material.SetVector("_PushOrigin", displacer.transform.position);
    }

    void OnValidate()
    {
        UpdateMesh();
    }
}
