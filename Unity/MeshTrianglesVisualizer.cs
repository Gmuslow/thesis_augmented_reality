using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshTrianglesVisualizer : MonoBehaviour
{
    public bool enable = false;
    private void OnDrawGizmos()
    {
        //Gizmos.Clear();
        if (enable)
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                Mesh mesh = meshFilter.sharedMesh;
                //Debug.Log(mesh.triangles.Length);
                Gizmos.color = Color.red;
                //
                for (int i = 0; i < mesh.triangles.Length; i += 3)
                {
                    Vector3 vertexA = mesh.vertices[mesh.triangles[i]];
                    Vector3 vertexB = mesh.vertices[mesh.triangles[i + 1]];
                    Vector3 vertexC = mesh.vertices[mesh.triangles[i + 2]];

                    Gizmos.DrawLine(transform.TransformPoint(vertexA), transform.TransformPoint(vertexB));
                    Gizmos.DrawLine(transform.TransformPoint(vertexB), transform.TransformPoint(vertexC));
                    Gizmos.DrawLine(transform.TransformPoint(vertexC), transform.TransformPoint(vertexA));
                }
                // Iterate through vertices and draw spheres
                for (int i = 0; i < mesh.vertices.Length; i++)
                {
                    Vector3 vertexPosition = transform.TransformPoint(mesh.vertices[i]);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(vertexPosition, 0.01f);
                }
            }
        }
    }
}
