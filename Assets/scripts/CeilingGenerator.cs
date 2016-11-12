using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CeilingGenerator : MonoBehaviour
{
    [SerializeField]
    FloorGenerator floor;

    [SerializeField]
    WallGenerator walls;

    [SerializeField]
    float delay = 0.5f;

    bool generated = false;
    bool generating = false;

    List<Vector3> verts = new List<Vector3>();
    List<Vector2> UVs = new List<Vector2>();
    List<int> tris = new List<int>();

    Mesh mesh;

    void Start()
    {
        MeshFilter mFilt = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.name = "ProcGen Ceiling";
        mFilt.mesh = mesh;
        StartCoroutine(_Build());
    }

    IEnumerator<WaitForSeconds> _Build()
    {

        List<List<Vector3>> shapes = new List<List<Vector3>>();
        int shape = 0;
        Vector3 up = Vector3.up * walls.Height;
        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0)
        };

        while (true)
        {
            if (floor.Generated)
            {
                if (generated)
                {
                    yield return new WaitForSeconds(0.05f);
                }
                else
                {
                    if (!generating)
                    {
                        foreach (List<Vector3> shapeCorners in floor.GetShapeCorners(false))
                        {
                            shapes.Add(shapeCorners.Select(v => transform.InverseTransformPoint(v)).ToList());
                        }
                        shape = -1;
                        generating = true;
                    }
                    shape++;
                    if (shape >= shapes.Count)
                    {
                        generated = true;
                    }
                    else
                    {
                        int n = verts.Count;

                        for (int i=0, l=shapes[shape].Count; i< l; i++)
                        {
                            verts.Add(shapes[shape][i] + up);
                            UVs.Add(uvs[i % 4]);
                        }

                        tris.Add(n);
                        tris.Add(n + 2);
                        tris.Add(n + 1);

                        tris.Add(n);
                        tris.Add(n + 3);
                        tris.Add(n + 2);

                        mesh.Clear();
                        mesh.SetVertices(verts);
                        mesh.SetTriangles(tris, 0);
                        mesh.SetUVs(0, UVs);
                        mesh.RecalculateBounds();
                        mesh.RecalculateNormals();

                        yield return new WaitForSeconds(delay);
                    }
                }
            }
            else
            {
                if (generated || generating)
                {
                    generated = false;
                    generating = false;
                    verts.Clear();
                    UVs.Clear();
                    tris.Clear();
                    shapes.Clear();
                    mesh.Clear();
                    mesh.SetVertices(verts);
                    mesh.SetTriangles(tris, 0);
                    mesh.SetUVs(0, UVs);
                    mesh.RecalculateBounds();
                    mesh.RecalculateNormals();
                }
                yield return new WaitForSeconds(0.05f);
        }
    }
                    
    }
}