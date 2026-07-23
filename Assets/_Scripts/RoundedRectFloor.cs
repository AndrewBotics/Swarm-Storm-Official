using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class RoundedRectFloor : MonoBehaviour
{
    private float Width = 70f;
    private float Depth = 40f;
    private float CornerRadius = 15f;
    private int CornerSegments = 8;

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

        var perimeter = new System.Collections.Generic.List<Vector2>();
        foreach (int cornerIndex in new[] { 0, 1, 2, 3 })
        {
            Vector2 center = cornerCenters[cornerIndex];
            float startAngle = startAngles[cornerIndex];
            for (int i = 0; i<=CornerSegments; i++)
            {
                float angle = (startAngle + (90f * i / CornerSegments)) * Mathf.Deg2Rad;
                Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * CornerRadius;
                perimeter.Add(point);
            }
        }

        int vertCount = perimeter.Count + 1;
        var vertices = new Vector3[vertCount];
        var normals = new Vector3[vertCount];
        var uvs = new Vector2[vertCount];

        vertices[0] = Vector3.zero;
        normals[0] = Vector3.up;
        uvs[0] = new Vector2(0.5f, 0.5f);

        for (int i = 0; i<perimeter.Count; i++)
        {
            Vector2 p = perimeter[i];
            vertices[i+1] = new Vector3(p.x, 0f, p.y);
            normals[i+1] = Vector3.up;
            uvs[i+1] = new Vector2(p.x / Width + 0.5f, p.y / Depth + 0.5f);
        }

        var triangles = new System.Collections.Generic.List<int>();
        for (int i = 0; i<perimeter.Count; i++)
        {
            int current = i + 1;
            int next = (i+1) % perimeter.Count + 1;
            triangles.Add(0);
            triangles.Add(next);
            triangles.Add(current);
        }

        var mesh = new Mesh { name = "RoundedRectFloor" };
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;

        if (FloorMaterial != null)
        {
            meshRenderer.sharedMaterial = FloorMaterial;
        }
    }
}
