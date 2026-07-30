using UnityEngine;
using System.Collections.Generic;

public class Telegraph : MonoBehaviour
{
    [SerializeField] private Material telegraphFillMaterial;
    private List<MeshRenderer> TelegraphList = new List<MeshRenderer>();
    private List<bool> TelegraphActivity = new List<bool>();
    public static readonly float offset = 0.15f;

    protected MeshRenderer CreateOrGetTelegraphMesh()
    {
        for (int i = 0; i<TelegraphList.Count; i++){
            if (!TelegraphActivity[i]) {
                TelegraphActivity[i] = true;
                return TelegraphList[i];
            }
        }
        
        GameObject meshObj = new GameObject("TelegraphMesh");
        meshObj.transform.SetParent(transform);
        
        meshObj.AddComponent<MeshFilter>();
        MeshRenderer mr = meshObj.AddComponent<MeshRenderer>();
        mr.material = telegraphFillMaterial;
        
        TelegraphList.Add(mr);
        TelegraphActivity.Add(true);
        return mr;
    }

    // draw rectangles, obviously
    public MeshRenderer DrawFilledRectangle(Vector3 center, float width, float length, Vector3 forwardDirection)
    {
        MeshRenderer mr = CreateOrGetTelegraphMesh();
        MeshFilter mf = mr.GetComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4];
        float halfW = width / 2f;
        float halfL = length / 2f;

        vertices[0] = new Vector3(-halfW, 0, -halfL); // Bottom Left
        vertices[1] = new Vector3(-halfW, 0, halfL);  // Top Left
        vertices[2] = new Vector3(halfW, 0, halfL);   // Top Right
        vertices[3] = new Vector3(halfW, 0, -halfL);  // Bottom Right

        int[] triangles = new int[]
        {
            0, 1, 2, // Triangle 1 (Left half)
            0, 2, 3  // Triangle 2 (Right half)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mf.mesh = mesh;

        mr.transform.position = center;
        mr.transform.rotation = Quaternion.LookRotation(forwardDirection);
        
        mr.enabled = true;
        return mr;
    }

    // draw arcs or circles
    // make the inner radius 0 if you want a sector or circle.
    // make the degrees 360 if you want a full ring or circle.
    public MeshRenderer DrawFilledArc(Vector3 center, float innerRadius, float outerRadius, Vector3 forwardDirection, float angleDegrees, int segments = 50)
    {
        MeshRenderer mr = CreateOrGetTelegraphMesh();
        MeshFilter mf = mr.GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        // We need points for the inner arc AND the outer arc
        int verticesCount = (segments + 1) * 2;
        Vector3[] vertices = new Vector3[verticesCount];
        
        // Every segment is a quad (2 triangles = 6 indices)
        int[] triangles = new int[segments * 6]; 

        float halfAngle = angleDegrees / 2f;
        float angleStep = angleDegrees / segments;

        for (int i = 0; i<=segments; i++)
        {
            float currentAngle = (-halfAngle + (angleStep * i)) * Mathf.Deg2Rad;
            float sinAngle = Mathf.Sin(currentAngle);
            float cosAngle = Mathf.Cos(currentAngle);

            // Inner vertex (Index 0 to segments)
            vertices[i] = new Vector3(sinAngle * innerRadius, 0, cosAngle * innerRadius);
            
            // Outer vertex (Index segments+1 to end)
            vertices[i + segments + 1] = new Vector3(sinAngle * outerRadius, 0, cosAngle * outerRadius);
        }

        int t = 0;
        for (int i = 0; i<segments; i++)
        {
            int innerLeft = i;
            int innerRight = i + 1;
            int outerLeft = i + segments + 1;
            int outerRight = i + segments + 2;

            // Triangle 1: Inner Left -> Outer Left -> Outer Right
            triangles[t++] = innerLeft;
            triangles[t++] = outerLeft;
            triangles[t++] = outerRight;

            // Triangle 2: Inner Left -> Outer Right -> Inner Right
            triangles[t++] = innerLeft;
            triangles[t++] = outerRight;
            triangles[t++] = innerRight;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mf.mesh = mesh;

        mr.transform.position = center;
        mr.transform.rotation = Quaternion.LookRotation(forwardDirection);

        mr.enabled = true;
        return mr;
    }

    public void ClearPreviews()
    {
        for (int i = 0; i<TelegraphList.Count; i++)
        {
            TelegraphList[i].enabled = false;
            TelegraphActivity[i] = false;
        }
    }
}