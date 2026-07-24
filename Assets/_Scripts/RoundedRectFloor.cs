using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class RoundedRectFloor : MonoBehaviour
{
    private float Width = 70f;
    private float Depth = 40f;
    private float CornerRadius = 15f;
    private int CornerSegments = 10;
    private float BoundaryWallHeight = 20f;

    [SerializeField] private Material FloorMaterial;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;

    private void OnEnable()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        Generate();
    }

    private void OnValidate()
    {
        float maxRadius = Mathf.Min(Width, Depth) / 2f;
        CornerRadius = Mathf.Clamp(CornerRadius, 0f, maxRadius);

        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (meshCollider == null) meshCollider = GetComponent<MeshCollider>();
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        Generate();
    }

    [ContextMenu("Regenerate Mesh")]
    private void Generate()
    {
        float halfW = Width / 2f - CornerRadius;
        float halfD = Depth / 2f - CornerRadius;

        Vector2[] cornerCenters =
        {
            new Vector2(halfW, -halfD),   // bottom-right
            new Vector2(halfW, halfD),    // top-right
            new Vector2(-halfW, halfD),   // top-left
            new Vector2(-halfW, -halfD),  // bottom-left
        };
        float[] startAngles = { -90f, 0f, 90f, 180f };

        var perimeter = new List<Vector2>();
        foreach (int cornerIndex in new[] { 0, 1, 2, 3 })
        {
            Vector2 center = cornerCenters[cornerIndex];
            float startAngle = startAngles[cornerIndex];
            for (int i = 0; i <= CornerSegments; i++)
            {
                float angle = (startAngle + (90f * i / CornerSegments)) * Mathf.Deg2Rad;
                Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * CornerRadius;
                perimeter.Add(point);
            }
        }

        int pCount = perimeter.Count;

        int visVertCount = pCount + 1;
        var visVerts = new Vector3[visVertCount];
        var visNorms = new Vector3[visVertCount];
        var visUVs = new Vector2[visVertCount];

        visVerts[0] = Vector3.zero;
        visNorms[0] = Vector3.up;
        visUVs[0] = new Vector2(0.5f, 0.5f);

        for (int i = 0; i < pCount; i++)
        {
            Vector2 p = perimeter[i];
            visVerts[i + 1] = new Vector3(p.x, 0f, p.y);
            visNorms[i + 1] = Vector3.up;
            visUVs[i + 1] = new Vector2(p.x / Width + 0.5f, p.y / Depth + 0.5f);
        }

        var visTris = new List<int>();
        for (int i = 0; i < pCount; i++)
        {
            int current = i + 1;
            int next = (i + 1) % pCount + 1;
            visTris.Add(0);
            visTris.Add(next);
            visTris.Add(current);
        }

        var visualMesh = new Mesh { name = "RoundedRectFloor_Visual" };
        visualMesh.vertices = visVerts;
        visualMesh.normals = visNorms;
        visualMesh.uv = visUVs;
        visualMesh.triangles = visTris.ToArray();
        visualMesh.RecalculateBounds();

        int physVertCount = visVertCount + pCount;
        var physVerts = new Vector3[physVertCount];

        for (int i = 0; i < visVertCount; i++)
        {
            physVerts[i] = visVerts[i];
        }

        for (int i = 0; i < pCount; i++)
        {
            Vector2 p = perimeter[i];
            physVerts[visVertCount + i] = new Vector3(p.x, BoundaryWallHeight, p.y);
        }

        var physTris = new List<int>();
        physTris.AddRange(visTris);

        for (int i = 0; i < pCount; i++)
        {
            int floorCurrent = i + 1;
            int floorNext = (i + 1) % pCount + 1;
            int wallCurrent = visVertCount + i;
            int wallNext = visVertCount + ((i + 1) % pCount);

            physTris.Add(floorCurrent);
            physTris.Add(wallNext);
            physTris.Add(wallCurrent);

            physTris.Add(floorCurrent);
            physTris.Add(floorNext);
            physTris.Add(wallNext);
        }

        var physicsMesh = new Mesh { name = "RoundedRectFloor_Physics" };
        physicsMesh.vertices = physVerts;
        physicsMesh.triangles = physTris.ToArray();
        physicsMesh.RecalculateNormals(); 
        physicsMesh.RecalculateBounds();

        meshFilter.sharedMesh = visualMesh;
        
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = physicsMesh;

        if (FloorMaterial != null)
        {
            meshRenderer.sharedMaterial = FloorMaterial;
        }
    }
}