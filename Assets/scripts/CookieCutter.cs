using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CookieCutter : MonoBehaviour {

    [SerializeField]
    LayerMask collisionLayers;

    [SerializeField]
    Vector3 boxSize = Vector3.one;

    [SerializeField]
    bool showGizmoNormals = false;

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

    public IEnumerable<Vector3[]> Tris
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

            //DOWNS

            yield return new Vector3[3]
            {
                pt + x2 + y2 + z2,
                pt + x + y2 + z2,
                pt + x2 + y2 + z,
            };
            
            yield return new Vector3[3]
            {
                pt + x2 + y2 + z,
                pt + x + y2 + z2,
                pt + x + y2 + z
            };

            //UPS

            yield return new Vector3[3]
            {
                pt + x2 + y + z2,
                pt + x2 + y + z,
                pt + x + y + z2,
            };

            yield return new Vector3[3]
            {
                pt + x2 + y + z,
                pt + x + y + z,
                pt + x + y + z2,
            };

            //LEFTS

            yield return new Vector3[3]
            {
                pt + x2 + y2 + z2,
                pt + x2 + y2 + z,
                pt + x2 + y + z2,
            };

            yield return new Vector3[3]
            {
                pt + x2 + y + z2,
                pt + x2 + y2 + z,
                pt + x2 + y + z,
            };

            //RIGHTS

            yield return new Vector3[3]
            {
                pt + x + y2 + z2,
                pt + x + y + z2,
                pt + x + y2 + z,
            };

            yield return new Vector3[3]
            {
                pt + x + y + z2,
                pt + x + y + z,
                pt + x + y2 + z,
            };

            //FORWARD

            yield return new Vector3[3]
            {
                pt + x2 + y2 + z,
                pt + x + y2 + z,
                pt + x2 + y + z,
            };

            yield return new Vector3[3]
            {
                pt + x + y + z,
                pt + x2 + y + z,
                pt + x + y2 + z,
            };

            //REVERSE

            yield return new Vector3[3]
            {
                pt + x2 + y2 + z2,
                pt + x2 + y + z2,
                pt + x + y2 + z2,
            };

            yield return new Vector3[3]
            {
                pt + x + y + z2,
                pt + x + y2 + z2,
                pt + x2 + y + z2,
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
    List<Vector3> verts = new List<Vector3>();
    Transform doughTransform = null;

    public List<Vector3> CutsLineAt(Vector3 a, Vector3 b, Vector3 n)
    {
        List<Vector3> cuts = new List<Vector3>();

        Vector3 intercept = Vector3.zero;

        int i = 0;
        foreach (Vector3[] tri in Tris)
        {
            //Debug.Log(string.Format("{0} - {1}, tri {2}", a, b, i));
            if (ProcGenHelpers.LineSegmentInterceptPlane(tri[0], tri[1], tri[2], a, b, out intercept))
            {
                //Debug.Log("In plane");
                //cuts.Add(intercept);

                if (ProcGenHelpers.PointInTriangle(tri[0], tri[1], tri[2], intercept))
                {
                    Vector3 triNorm = Vector3.Cross(tri[1] - tri[0], tri[2] - tri[0]).normalized;

                    //Debug.Log("In Tri");                    
                    cuts.Add(intercept);

                    //TODO: Make correct

                    Ray r;
                    if (ProcGenHelpers.InterceptionRay(n, (b - a).normalized, intercept, triNorm, out r))
                    {
                        Vector3 rayHit = ProcGenHelpers.RayHitEdge(tri[0], tri[1], tri[2], r);
                        if (Vector3.SqrMagnitude(rayHit - intercept) > Mathf.Epsilon)
                        {
                            cuts.Add(rayHit);
                        }
                    }

                }
            }
            i++;
        }

        return cuts;
    }

    public void CutDough(Mesh dough, MeshFilter mFilt)
    {
        doughTransform = mFilt.transform;
        cuttingLines.Clear();

        verts.Clear();
        verts.AddRange(dough.vertices);                

        List<Vector2> uv = new List<Vector2>();
        uv.AddRange(dough.uv);

        List<int> tris = new List<int>();
        tris.AddRange(dough.triangles);

        List<Vector3[]> cutterLines = CutterLines.Select(l => new Vector3[2] { doughTransform.InverseTransformPoint(l[0]), doughTransform.InverseTransformPoint(l[1])}).ToList();
        int nLines = cutterLines.Count;

        //Debug.Log(string.Format("Will cut {0}, {1} triangles {2} cutting lines", dough, tris.Count / 3, nLines));

        cutRim.Clear();

        for (int i=0, l=tris.Count; i< l; i+=3)
        {
            Vector3 v1 = verts[tris[i]];
            Vector3 v2 = verts[tris[i + 1]];
            Vector3 v3 = verts[tris[i + 2]];
            //Debug.Log("Tri " + i);
            for (int j = 0; j < nLines; j++) {
                Vector3[] line = cutterLines[j];
                Vector3 pt;
                if (ProcGenHelpers.LineSegmentInterceptPlane(v1, v2, v3, line[0], line[1], out pt)) {

                    //Debug.Log(string.Format("Tri {0}, Line {1} intercepts plane {3} {4} {5} at {2}.", i, j, pt, v1, v2, v3));

                    if (ProcGenHelpers.PointInTriangle(v1, v2, v3, pt))
                    {
                        //Debug.Log("Found triangle that needs cutting");
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

        //Assuming cube-cutting for now
        if (cutRim.Count == 4)
        {
            //Test if order of corners is scrabled
            if (Vector3.Dot(ProcGenHelpers.TriangleNormal(cutRim[0], cutRim[1], cutRim[2]), ProcGenHelpers.TriangleNormal(cutRim[0], cutRim[2], cutRim[3])) < 0.5f)
            {
                Vector3 tmp = cutRim[2];
                cutRim[2] = cutRim[3];
                cutRim[3] = tmp;
            }

        }
        else if (cutRim.Count != 3)
        {
            Debug.LogError("Cutting rim got un-expected number of verts " + cutRim.Count);
            cutRim.Clear();
        }

        int cutRimL = cutRim.Count;

        for (int i = 0, l = tris.Count; i < l; i += 3)
        {
            bool originalRemoved = false;
            int indexV1 = tris[i];
            int indexV2 = tris[i + 1];
            int indexV3 = tris[i + 2];

            Vector3 v1 = verts[indexV1];
            Vector3 v2 = verts[indexV2];
            Vector3 v3 = verts[indexV3];
   
            Vector2 uv1 = uv[indexV1];
            Vector2 uv2 = uv[indexV2];
            Vector2 uv3 = uv[indexV3];

            Vector3 d21 = (v2 - v1).normalized;
            Vector3 d32 = (v3 - v2).normalized;
            Vector3 d13 = (v1 - v3).normalized;

            float t12 = Vector3.Distance(v1, v2);
            float t23 = Vector3.Distance(v2, v3);
            float t31 = Vector3.Distance(v3, v1);

            bool removeV1 = ProcGenHelpers.PointInConvexPolygon(v1, cutRim);
            bool removeV2 = ProcGenHelpers.PointInConvexPolygon(v2, cutRim);
            bool removeV3 = ProcGenHelpers.PointInConvexPolygon(v3, cutRim);

            if (removeV1 && removeV2 && removeV3)
            {
                tris.RemoveRange(i, 3);
                uv.RemoveRange(i, 3);
                verts.RemoveRange(i, 3);
                i -= 3;
                l -= 3;
            }
            else
            {
                //Assuming everything is in a plane
                float proxThreshold = 0.0001f;
                for (int idC = 0; idC < cutRimL; idC++)
                {

                    float tTri12;
                    float tTri23;
                    float tTri31;
                    float tCut12;
                    float tCut23;
                    float tCut31;

                    int nextIdC = (idC + 1) % cutRimL;
                    bool intercept12 = ProcGenHelpers.LineSegmentInterceptIn3D(v1, v2, cutRim[idC], cutRim[nextIdC], proxThreshold, out tTri12, out tCut12);
                    bool intercept23 = ProcGenHelpers.LineSegmentInterceptIn3D(v2, v3, cutRim[idC], cutRim[nextIdC], proxThreshold, out tTri23, out tCut23);
                    bool intercept31 = ProcGenHelpers.LineSegmentInterceptIn3D(v3, v1, cutRim[idC], cutRim[nextIdC], proxThreshold, out tTri31, out tCut31);

                    if (!originalRemoved && (intercept12 || intercept23 || intercept31))
                    {
                        // Remove triangle will build new ones
                        tris.RemoveRange(i, 3);
                        i -= 3;
                        l -= 3;
                        originalRemoved = true;
                    }

                    if (intercept12 && intercept23)
                    {
                        if (tCut12 > tCut23)
                        {
                            if (removeV2)
                            {
                                // V2 is inside the cutout triangle needs splitting up if tCut != t2Cut

                                if (Mathf.Abs(tCut12 - tCut23) < Mathf.Epsilon)
                                {
                                    //In this case V2 is actually on the line between cutRim[idC] and cutRim[idC + 1]
                                    //we need not do a thing

                                }
                                else
                                {
                                    // Cuts existing triangle short and adds new one
                                    Debug.Log("Two lines pass through cutter line, make two tris (replace/remove existing vert)");

                                    verts[indexV2] = v1 + d21 * tTri12;
                                    uv[indexV2] = Vector2.Lerp(uv1, uv2, tTri12 / t12);

                                    int n = verts.Count;

                                    verts.Add(v2 + d32 * tTri23);
                                    uv.Add(Vector2.Lerp(uv2, uv3, tTri23 / t23));

                                    tris.Add(indexV1);
                                    tris.Add(indexV2);
                                    tris.Add(indexV3);

                                    tris.Add(indexV3);
                                    tris.Add(indexV2);
                                    tris.Add(n);
                                }

                                removeV2 = false;

                            }
                            else
                            {
                                //Make two triangles
                                Debug.Log("Two lines pass through cutter line, make two tris");
                                int n = verts.Count;

                                verts.Add(v1 + d21 * tTri12);
                                uv.Add(Vector2.Lerp(uv1, uv2, tTri12 / t12));

                                verts.Add(v2 + d32 * tTri23);
                                uv.Add(Vector2.Lerp(uv2, uv3, tTri23 / t23));

                                tris.Add(indexV1);
                                tris.Add(n);
                                tris.Add(n + 1);

                                tris.Add(indexV3);
                                tris.Add(n);
                                tris.Add(n + 1);

                            }

                        }
                        else
                        {

                            //Pointy bit of cut triangle
                            Debug.Log("Two lines pass through cutter line, sharp, make one tris");

                            int n = verts.Count;

                            verts.Add(v1 + d21 * tTri12);
                            uv.Add(Vector2.Lerp(uv1, uv2, tTri12 / t12));

                            verts.Add(v2 + d32 * tTri23);
                            uv.Add(Vector2.Lerp(uv2, uv3, tTri23 / t23));


                            tris.Add(n);
                            tris.Add(indexV2);
                            tris.Add(n + 1);

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

        /*  
        int i = 0;
        foreach(Vector3[] l in CutterLines)
        {            
            Gizmos.color = cuttingLines.Contains(i) ? Color.red : Color.cyan;
            Gizmos.DrawLine(l[0], l[1]);
            i++;
        }*/

        Gizmos.color = Color.cyan;
        foreach(Vector3[] tri in Tris)
        {
            if (showGizmoNormals)
            {
                Vector3 center = (tri[0] + tri[1] + tri[2]) / 3f;
                Vector3 norm = Vector3.Cross((tri[1] - tri[0]), (tri[2] - tri[0])).normalized;
                Gizmos.DrawLine(center, center + norm * 0.3f);
            }
            Gizmos.DrawLine(tri[0], tri[1]);
            Gizmos.DrawLine(tri[1], tri[2]);
            Gizmos.DrawLine(tri[2], tri[0]);
        }
        /*
        if (doughTransform != null)
        {
            Gizmos.color = Color.magenta;
            for (int j = 0, l = cutRim.Count; j < l; j++)
            {
                Gizmos.DrawLine(doughTransform.TransformPoint(cutRim[j]), doughTransform.TransformPoint(cutRim[(j + 1) % l]));
            }
        }
        */
    }
}
