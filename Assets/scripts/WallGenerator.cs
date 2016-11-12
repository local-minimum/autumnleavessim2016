using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WallGenerator : MonoBehaviour
{

    [SerializeField]
    FloorGenerator floor;

    [SerializeField]
    float delay = 0.5f;

    [SerializeField]
    float height = 2.4f;

    public float Height
    {
        get
        {
            return height;
        }
    }

    bool generated = false;
    bool generating = false;

    List<Vector3> verts = new List<Vector3>();
    List<Vector2> UVs = new List<Vector2>();
    List<int> tris = new List<int>();
    List<Vector3> corners = new List<Vector3>();
    Mesh mesh;

    void Start()
    {
        MeshFilter mFilt = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.name = "ProcGen Walls";
        mFilt.mesh = mesh;
        StartCoroutine(_Build());
    }

    IEnumerator<WaitForSeconds> _Build()
    {
        Vector3 up = new Vector3(0, height, 0);
        int corner = 0;
        int vertsPerWall = 2;

        while (true)
        {
            if (floor.Generated)
            {
                if (generated) {
                    yield return new WaitForSeconds(0.05f);
                } else
                {
                    if (!generating)
                    {
                        corners.AddRange(floor.GetCircumferance(false).Select(v => transform.InverseTransformPoint(v)));
                        corner = -1;
                        generating = true;

                        Debug.Log(string.Format("{0} Corners to make walls on", corners.Count));
                    }

                    corner++;
                    if (corner > corners.Count)
                    {
                        generated = true;
                    } else
                    {

                        int nextCorner = (corner + 1) % corners.Count;

                        if (corner == 0)
                        {


                            verts.Add(corners[corner]);
                            verts.Add(corners[corner] + up);

                            UVs.Add(new Vector2(corner % 2, 0));
                            UVs.Add(new Vector2(corner % 2, 1));                            

                        }

                        if (nextCorner != 0)
                        {
                            verts.Add(corners[nextCorner]);
                            verts.Add(corners[nextCorner] + up);

                            UVs.Add(new Vector2(nextCorner % 2, 0));
                            UVs.Add(new Vector2(nextCorner % 2, 1));
                        }

                        tris.Add(nextCorner * vertsPerWall);
                        tris.Add(corner * vertsPerWall);
                        tris.Add(corner * vertsPerWall + 1);

                        tris.Add(nextCorner * vertsPerWall);
                        tris.Add(corner * vertsPerWall + 1);
                        tris.Add(nextCorner * vertsPerWall + 1);

                        mesh.Clear();
                        mesh.SetVertices(verts);
                        mesh.SetTriangles(tris, 0);
                        mesh.SetUVs(0, UVs);
                        mesh.RecalculateBounds();
                        mesh.RecalculateNormals();

                        yield return new WaitForSeconds(delay);
                    }
                }
            } else
            {
                if (generated || generating)
                {
                    generated = false;
                    generating = false;
                    verts.Clear();
                    UVs.Clear();
                    tris.Clear();
                    corners.Clear();
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;

        Vector3 prev = Vector3.zero;
        Vector3 firstV = Vector3.zero;
        bool first = true;      
        foreach(Vector3 cur in floor.GetCircumferance(false))
        {
            if (first)
            {
                firstV = cur;
                first = false;  
            } else
            {
                Gizmos.DrawLine(prev, cur);
            }
            prev = cur;
        }

        if (!first)
        {
            Gizmos.DrawLine(prev, firstV);
        }
    }
}