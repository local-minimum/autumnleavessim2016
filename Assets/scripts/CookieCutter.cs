using UnityEngine;
using System.Collections.Generic;

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
            Vector3 scale = transform.TransformDirection(boxSize / 2f);

            yield return new Vector3[2] {
                new Vector3(pt.x - scale.x, pt.y - scale.y, pt.z - scale.z),
                new Vector3(pt.x + scale.x, pt.y - scale.y, pt.z - scale.z)
            };

            yield return new Vector3[2] {
                new Vector3(pt.x - scale.x, pt.y - scale.y, pt.z - scale.z),
                new Vector3(pt.x - scale.x, pt.y + scale.y, pt.z - scale.z)
            };

            yield return new Vector3[2] {
                new Vector3(pt.x - scale.x, pt.y - scale.y, pt.z - scale.z),
                new Vector3(pt.x - scale.x, pt.y - scale.y, pt.z + scale.z)
            };

            yield return new Vector3[2] {
                new Vector3(pt.x - scale.x, pt.y - scale.y, pt.z + scale.z),
                new Vector3(pt.x - scale.x, pt.y + scale.y, pt.z + scale.z)
            };

            yield return new Vector3[2] {
                new Vector3(pt.x - scale.x, pt.y - scale.y, pt.z + scale.z),
                new Vector3(pt.x = scale.x, pt.y - scale.y, pt.z + scale.z)
            };

            yield return new Vector3[2] {
                new Vector3(pt.x - scale.x, pt.y + scale.y, pt.z + scale.z),
                new Vector3(pt.x - scale.x, pt.y + scale.y, pt.z - scale.z)
            };

            yield return new Vector3[2] {
                new Vector3(pt.x - scale.x, pt.y + scale.y, pt.z + scale.z),
                new Vector3(pt.x + scale.x, pt.y + scale.y, pt.z + scale.z)
            };

            yield return new Vector3[2] {
                new Vector3(pt.x + scale.x, pt.y + scale.y, pt.z + scale.z),
                new Vector3(pt.x + scale.x, pt.y + scale.y, pt.z - scale.z)
            };

            yield return new Vector3[2] {
                new Vector3(pt.x + scale.x, pt.y + scale.y, pt.z + scale.z),
                new Vector3(pt.x + scale.x, pt.y - scale.y, pt.z + scale.z)
            };

            yield return new Vector3[2] {
                new Vector3(pt.x + scale.x, pt.y - scale.y, pt.z + scale.z),
                new Vector3(pt.x + scale.x, pt.y - scale.y, pt.z - scale.z)
            };

            yield return new Vector3[2] {
                new Vector3(pt.x + scale.x, pt.y + scale.y, pt.z - scale.z),
                new Vector3(pt.x + scale.x, pt.y - scale.y, pt.z - scale.z)
            };

        }
    }

    public void Cut()
    {
        Debug.Log("Attempting cuts");

        foreach (Collider col in Physics.OverlapBox(transform.position, transform.TransformDirection(boxSize), transform.rotation, collisionLayers)) 
        {
            MeshFilter mFilt = col.GetComponent<MeshFilter>();
            if (mFilt != null)
            {
                CutDough(mFilt.sharedMesh, mFilt);
                
            }
            
        }
        
    }

    public void CutDough(Mesh dough, MeshFilter mFilt)
    {
        Debug.Log("Will cut " + dough);

        List<Vector3> verts = new List<Vector3>();
        verts.AddRange(dough.vertices);

        List<Vector2> uv = new List<Vector2>();
        uv.AddRange(dough.uv);

        List<int> tris = new List<int>();
        tris.AddRange(dough.triangles);

        for (int i=0, l=tris.Count/3; i< l; i++)
        {

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
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.TransformDirection(boxSize));
    }
}
