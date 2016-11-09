using UnityEngine;

public class Mesher : MonoBehaviour {

    MeshFilter mFilt;
    Mesh mesh;

    [SerializeField]
    int minTriangles = 3;

    [SerializeField]
    int maxTriangles = 20;

    [SerializeField]
    Vector3 minValues;

    [SerializeField]
    Vector3 maxValues;

    int triangles;

	void Start () {
        mFilt = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mFilt.mesh = mesh;
        mesh.name = "Crazy Mesh";
        mesh.MarkDynamic();
        triangles = Random.Range(minTriangles, maxTriangles);
	}
		
	void Update () {

        //triangles = Mathf.Clamp(triangles + Random.Range(-1, 1), minTriangles, maxTriangles);

        Vector3[] verts = new Vector3[triangles * 3];
        int[] tris = new int[triangles * 3];

        for (int i=0; i<verts.Length; i++)
        {
            verts[i] = new Vector3(
                Random.Range(minValues.x, maxValues.x),
                Random.Range(minValues.y, maxValues.y),
                Random.Range(minValues.z, maxValues.z)
                );
            tris[i] = i;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

	}
}
