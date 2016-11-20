using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CookieCutter : MonoBehaviour {

    [SerializeField]
    LayerMask collisionLayers;

    [SerializeField]
    Vector3 boxSize = Vector3.one;

    IEnumerable<Vector3[]> CutterLines
    {
        get
        {
            Vector3 pt = transform.position;

            Vector3 x = transform.right * boxSize.x / 2f;
            Vector3 x2 = -transform.right * boxSize.x / 2f;

            Vector3 y = transform.up * boxSize.y / 2f;
            Vector3 y2 = -transform.up * boxSize.y / 2f;

            Vector3 z = transform.forward * boxSize.z / 2f;
            Vector3 z2 = -transform.forward * boxSize.z / 2f;

            yield return new Vector3[2]
            {
                pt + x2 + y2 + z2,
                pt + x + y2 + z2
            };

            yield return new Vector3[2]
            {
                pt + x2 + y2 + z2,
                pt + x2 + y + z2
            };

            yield return new Vector3[2]
            {
                pt + x2 + y2 + z,
                pt + x2 + y2 + z2
            };

            yield return new Vector3[2]
            {
                pt + x2 + y2 + z,
                pt + x2 + y + z
            };

            yield return new Vector3[2]
            {
                pt + x2 + y2 + z,
                pt + x + y2 + z
            };

            yield return new Vector3[2]
            {
                pt + x2 + y + z,
                pt + x2 + y + z2
            };

            yield return new Vector3[2]
            {
                pt + x2 + y + z,
                pt + x + y + z
            };

            yield return new Vector3[2]
            {
                pt + x2 + y + z2,
                pt + x + y + z2
            };

            yield return new Vector3[2]
            {
                pt + x + y + z,
                pt + x + y2 + z
            };

            yield return new Vector3[2]
            {
                pt + x + y + z2,
                pt + x + y2 + z2
            };

            yield return new Vector3[2]
            {
                pt + x + y2 + z,
                pt + x + y2 + z2
            };

            yield return new Vector3[2]
            {
                pt + x + y + z,
                pt + x + y + z2
            };
        }
    }

    public void Cut()
    {
        Debug.Log("Attempting cuts");

        foreach (Collider col in Physics.OverlapBox(transform.position, transform.TransformDirection(boxSize) / 2f, transform.rotation, collisionLayers)) 
        {
            MeshFilter mFilt = col.GetComponent<MeshFilter>();
            if (mFilt != null)
            {
                CutDough(mFilt.sharedMesh, mFilt);                
            }
            
        }
        
    }

    List<int> cuttingLines = new List<int>();
    List<Vector3> cutRim = new List<Vector3>();

    public void CutDough(Mesh dough, MeshFilter mFilt)
    {

        cuttingLines.Clear();

        List<Vector3> verts = new List<Vector3>();
        verts.AddRange(dough.vertices);                

        List<Vector2> uv = new List<Vector2>();
        uv.AddRange(dough.uv);

        List<int> tris = new List<int>();
        tris.AddRange(dough.triangles);

        List<Vector3[]> cutterLines = CutterLines.Select(l => new Vector3[2] { transform.InverseTransformPoint(l[0]), transform.InverseTransformPoint(l[1])}).ToList();
        int nLines = cutterLines.Count;

        Debug.Log(string.Format("Will cut {0}, {1} triangles {2} cutting lines", dough, tris.Count / 3, nLines));

        cutRim.Clear();

        //TODO: Something is wrong due to mesh scale it seems?

        for (int i=0, l=tris.Count; i< l; i+=3)
        {
            Vector3 v1 = verts[tris[i]];
            Vector3 v2 = verts[tris[i + 1]];
            Vector3 v3 = verts[tris[i + 2]];
            Debug.Log("Tri " + i);
            for (int j = 0; j < nLines; j++) {
                Vector3[] line = cutterLines[j];
                Vector3 pt;
                if (ProcGenHelpers.LineSegmentInterceptPlane(v1, v2, v3, line[0], line[1], out pt)) {

                    Debug.Log(string.Format("Tri {0}, Line {1} intercepts plane {3} {4} {5} at {2}.", i, j, pt, v1, v2, v3));

                    if (ProcGenHelpers.PointInTriangle(v1, v2, v3, pt))
                    {
                        Debug.Log("Found triangle that needs cutting");
                        if (!cuttingLines.Contains(j))
                        {
                            cuttingLines.Add(j);
                            if (!cutRim.Contains(pt))
                            {
                                cutRim.Add(pt);
                            }
                        }
                    }
                }
            }
            
        }

        Mesh cutDough = new Mesh();
        cutDough.name = dough.name + ".CCut";

        cutDough.SetVertices(verts);
        cutDough.SetUVs(0, uv);
        cutDough.SetTriangles(tris, 0);
        cutDough.RecalculateBounds();
        cutDough.RecalculateNormals();

        mFilt.sharedMesh = cutDough;
    }

    public void OnDrawGizmosSelected()
    {

  
        int i = 0;
        foreach(Vector3[] l in CutterLines)
        {
            Gizmos.color = cuttingLines.Contains(i) ? Color.red : Color.cyan;
            Gizmos.DrawLine(l[0], l[1]);
            i++;
        }

        Gizmos.color = Color.magenta;
        for (int j=0, l=cutRim.Count; j<l; j++)
        {
            Gizmos.DrawLine(transform.TransformPoint(cutRim[j]), transform.TransformPoint(cutRim[(j + 1) % l]));
        }
    }
}
