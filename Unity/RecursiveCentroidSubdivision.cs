using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecursiveCentroidSubdivision : MonoBehaviour
{
    public int numIterations = 0;
    public bool keepGoing = false;
    public int maxVertices = 100;
    // Start is called before the first frame update
    void Awake()
    {
        MeshFilter m = GetComponent<MeshFilter>();
        Mesh mesh = m.sharedMesh;
        Debug.Log("Starting Triangles: " + mesh.triangles.Length);
        if (keepGoing)
        {
            while (mesh.vertices.Length < maxVertices)
            {
                SubDivideMesh();
                Debug.Log("New mesh Triangles: " + mesh.triangles.Length);
                Debug.Log("New mesh Vertices: " + mesh.vertices.Length);
            }
        }
        else
        {
            for (int i = 0; i < numIterations; i++)
            {
                SubDivideMesh();
                Debug.Log("New mesh Triangles: " + mesh.triangles.Length);
                Debug.Log("New mesh Vertices: " + mesh.vertices.Length);
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void SubDivideMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;

        List<Vector3> vertexList = new List<Vector3>();
        List<int> triangleList = new List<int>();

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 vertexA = mesh.vertices[mesh.triangles[i]];
            Vector3 vertexB = mesh.vertices[mesh.triangles[i + 1]];
            Vector3 vertexC = mesh.vertices[mesh.triangles[i + 2]];

            // Calculate new vertex positions
            Vector3 centroid = CalculateCentroid(vertexA, vertexB, vertexC);
            Vector3 midAB = GetMidpoint(vertexA, vertexB);
            Vector3 midBC = GetMidpoint(vertexB, vertexC);
            Vector3 midCA = GetMidpoint(vertexC, vertexA);

            // Add vertices to new vertex list if they don't already exist
            AddUniqueVertex(vertexList, vertexA);
            AddUniqueVertex(vertexList, vertexB);
            AddUniqueVertex(vertexList, vertexC);
            AddUniqueVertex(vertexList, midAB);
            AddUniqueVertex(vertexList, midBC);
            AddUniqueVertex(vertexList, midCA);
            AddUniqueVertex(vertexList, centroid);

            // Ensure winding order is correct
            if (IsClockwise(vertexA, midAB, centroid))
            {
                // Triangle 1
                AddTriangleIndices(vertexList, triangleList, vertexA, midAB, centroid);

                // Triangle 2
                AddTriangleIndices(vertexList, triangleList, midAB, vertexB, centroid);

                // Triangle 3
                AddTriangleIndices(vertexList, triangleList, vertexB, midBC, centroid);

                // Triangle 4
                AddTriangleIndices(vertexList, triangleList, midBC, vertexC, centroid);

                // Triangle 5
                AddTriangleIndices(vertexList, triangleList, vertexC, midCA, centroid);

                // Triangle 6
                AddTriangleIndices(vertexList, triangleList, midCA, vertexA, centroid);
            }
            else
            {
                // Reverse winding order if necessary
                // Triangle 1
                AddTriangleIndices(vertexList, triangleList, vertexA, midAB, centroid);

                // Triangle 2
                AddTriangleIndices(vertexList, triangleList, vertexB, midBC, centroid);

                // Triangle 3
                AddTriangleIndices(vertexList, triangleList, vertexC, midCA, centroid);

                // Triangle 4
                AddTriangleIndices(vertexList, triangleList, midAB, vertexB, centroid);

                // Triangle 5
                AddTriangleIndices(vertexList, triangleList, midBC, vertexC, centroid);

                // Triangle 6
                AddTriangleIndices(vertexList, triangleList, midCA, vertexA, centroid);
            }
        }

        mesh.vertices = vertexList.ToArray();
        mesh.triangles = triangleList.ToArray();
    }

    void AddUniqueVertex(List<Vector3> vertexList, Vector3 vertex)
    {
        if (!vertexList.Contains(vertex))
        {
            vertexList.Add(vertex);
        }
    }

    void AddTriangleIndices(List<Vector3> vertexList, List<int> triangleList, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        triangleList.Add(vertexList.IndexOf(vertex1));
        triangleList.Add(vertexList.IndexOf(vertex2));
        triangleList.Add(vertexList.IndexOf(vertex3));
    }

    bool IsClockwise(Vector3 a, Vector3 b, Vector3 c)
    {
        return Vector3.Cross(b - a, c - a).z < 0;
    }


    Vector3 CalculateCentroid(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        float xCoor = (vertex1.x + vertex2.x + vertex3.x) / 3.0f;
        float yCoor = (vertex1.y + vertex2.y + vertex3.y) / 3.0f;
        float zCoor = (vertex1.z + vertex2.z + vertex3.z) / 3.0f;
        return new Vector3(xCoor, yCoor, zCoor);
    }
    Vector3 GetMidpoint(Vector3 point1, Vector3 point2)
    {
        return (point2 + point1) / 2.0f;
    }
}
