using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public struct CutIntercept
{
    public int idCutterTri;
    public Ray cuttingRay;

    public CutIntercept(int idCutterTri, Ray cuttingRay)
    {
        this.idCutterTri = idCutterTri;
        this.cuttingRay = new Ray(cuttingRay.origin, cuttingRay.direction);
    }

    public CutIntercept(int idCutterTri, Vector3 intercept, Vector3 direction)
    {
        this.idCutterTri = idCutterTri;
        this.cuttingRay = new Ray(intercept, direction);
    }
}


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

    Vector3[] myVerts = new Vector3[0];
    int[] myTris = new int[0];
    int myTrisCount = 0;
    List<Vector3> myTriCenters = new List<Vector3>();
    List<Vector3> myTriNormals = new List<Vector3>();

    public void RecalculateMeshlike()
    {

        Vector3 pt = transform.position;

        Vector3 x = transform.right * boxSize.x / 2f;
        Vector3 x2 = -transform.right * boxSize.x / 2f;

        Vector3 y = transform.up * boxSize.y / 2f;
        Vector3 y2 = -transform.up * boxSize.y / 2f;

        Vector3 z = transform.forward * boxSize.z / 2f;
        Vector3 z2 = -transform.forward * boxSize.z / 2f;

        myTris = new int[]
        {
            //DOWNS
            0, 1, 2,
            2, 1, 3,
            //UPS
            4, 5, 6,
            5, 7, 6,
            //LEFTS
            0, 2, 4,
            4, 2, 5,
            //RIGHTS
            1, 6, 3,
            6, 7, 3,
            //FORWARD
            2, 3, 5,
            7, 5, 3,
            //REVERSE
            0, 4, 1,
            6, 1, 4,
        };

        myVerts = new Vector3[]
        {
            pt + x2 + y2 + z2,  //0
            pt + x + y2 + z2,   //1
            pt + x2 + y2 + z,   //2
            pt + x + y2 + z,    //3
            pt + x2 + y + z2,   //4
            pt + x2 + y + z,    //5
            pt + x + y + z2,    //6
            pt + x + y + z,     //7

        };

        myTrisCount = myTris.Length / 3;
        CalculateTriCenters();
        CalculateTriNorms();
    }

    void CalculateTriCenters()
    {
        myTriCenters.Clear();
        for (int i = 0, v = 0; i < myTrisCount; i++, v += 3)
        {
            Vector3 vertA = myVerts[myTris[v]];
            Vector3 vertB = myVerts[myTris[v + 1]];
            Vector3 vertC = myVerts[myTris[v + 2]];
            myTriCenters.Add((vertA + vertB + vertC) / 3f);
        }
    }


    void CalculateTriNorms()
    {
        myTriNormals.Clear();
        for (int i = 0, v = 0; i < myTrisCount; i++, v+=3)
        {
            Vector3 vertA = myVerts[myTris[v]];
            Vector3 vertB = myVerts[myTris[v + 1]];
            Vector3 vertC = myVerts[myTris[v + 2]];
            myTriNormals.Add(Vector3.Cross(vertB - vertA, vertC - vertA).normalized);
        }
    }

    public bool PointInMesh(Vector3 pt, float proximityThreshold=0.0001f)
    {
        //TODO: Only supports convex meshes

        for (int i=0; i<myTrisCount; i++)
        {
            if (Vector3.Dot(pt - myTriCenters[i], myTriNormals[i]) > proximityThreshold)
            {
                return false;
            }
        }
        return true;
    }

    int GetNeighbourTri(int idVertLineA, int idVertLineB, int curTri)
    {
        for (int i = 0, v = 0; i < myTrisCount; i++, v += 3)
        {
            if (i == curTri)
            {
                continue;
            }

            int idVertA = myTris[v];
            int idVertB = myTris[v + 1];
            int idVertC = myTris[v + 2];

            if (idVertLineA == idVertA && (idVertLineB == idVertB || idVertLineB == idVertC) || 
                idVertLineA == idVertB && (idVertLineB == idVertA || idVertLineB == idVertC) || 
                idVertLineA == idVertC && (idVertLineB == idVertA || idVertLineB == idVertB))
            {

                return i;
            }
        }
        return -1;
    }

    public int GetMissingVert(int tri, int a, int b)
    {
        int v = tri * 3;
        int v0 = myTris[v];
        int v1 = myTris[v + 1];
        
        if (v0 != a && v0 != b)
        {
            return v0;
        } else if (v1 != a && v1 != b)
        {
            return v1;
        } else
        {
            return myTris[v + 2];
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

    public List<Vector3> GetLineCutIntercepts(Vector3 a, Vector3 b, Vector3 n)
    {
        List<Vector3> cuts = new List<Vector3>();

        Vector3 intercept = Vector3.zero;

        for (int i = 0, v = 0; i < myTrisCount; i++, v += 3)
        {
            Vector3 vertA = myVerts[myTris[v]];
            Vector3 vertB = myVerts[myTris[v + 1]];
            Vector3 vertC = myVerts[myTris[v + 2]];

            //Debug.Log(string.Format("{0} - {1}, tri {2}", a, b, i));
            if (ProcGenHelpers.LineSegmentInterceptPlane(vertA, vertB, vertC, a, b, out intercept))
            {
                //Debug.Log("In plane");
                //cuts.Add(intercept);

                if (ProcGenHelpers.PointInTriangle(vertA, vertB, vertC, intercept))
                {
                    cuts.Add(intercept);
                }
            }
        }

        return cuts;
    }

    public Dictionary<int, List<Vector3>> GetInterceptTris(Vector3[] intercepts, float proximityToPlaneThreshold=0.001f)
    {
        Dictionary<int, List<Vector3>> interceptTris = new Dictionary<int, List<Vector3>>();

        for (int idIntercept = 0; idIntercept < intercepts.Length; idIntercept++)
        {
            Vector3 pt = intercepts[idIntercept];

            for (int idTri = 0, idVert = 0; idTri < myTrisCount; idTri++, idVert += 3)
            {

                Plane p = new Plane(myTriNormals[idTri], myTriCenters[idTri]);
                if (Mathf.Abs(p.GetDistanceToPoint(pt)) < proximityToPlaneThreshold)
                {
                    Vector3 vertA = myVerts[myTris[idVert]];
                    Vector3 vertB = myVerts[myTris[idVert + 1]];
                    Vector3 vertC = myVerts[myTris[idVert + 2]];

                    if (ProcGenHelpers.PointInTriangle(vertA, vertB, vertC, pt))
                    {
                        if (!interceptTris.Keys.Contains(idTri))
                        {
                            interceptTris[idTri] = new List<Vector3>();
                        }
                        interceptTris[idTri].Add(pt);
                        break;
                    }
                }
            }

        }

        return interceptTris;

    }

    public List<Vector3> TraceSurface(int tri, Vector3 thirdVert, Vector3 intercept, Vector3 n, Dictionary<int, List<Vector3>> allIntercepts, float proximityOfInterceptThreshold=0.001f, int searchDepth=20)
    {
        List<Vector3> traceLine = new List<Vector3>();
        Vector3 orginalIntercept = intercept;
                      
        Ray r;
        if (ProcGenHelpers.InterceptionRay(n, intercept, myTriNormals[tri], out r))
        {
            r.direction = Mathf.Sign(Vector3.Dot(thirdVert - intercept, r.direction)) * r.direction;

            while (tri >= 0) {

                int v = tri * 3;
                Vector3 vertA = myVerts[myTris[v]];
                Vector3 vertB = myVerts[myTris[v + 1]];
                Vector3 vertC = myVerts[myTris[v + 2]];
       
                int hitEdge;

                //TODO: Sensitive as edge condition in some rotations
                Vector3 rayHit = ProcGenHelpers.RayHitEdge(vertA, vertB, vertC, r, out hitEdge);
                if (hitEdge == -1)
                {
                    traceLine.Clear();
                    Debug.LogError(string.Format("Intercept {0} was not in the reported triangle {2} (trace length = {1})!", intercept, traceLine.Count(), tri));
                    tri = -1;
                } else
                {
                    if (allIntercepts.Keys.Contains(tri))
                    {
                        List<Vector3> hitIntercepts = allIntercepts[tri]
                            .Where(v1 => Vector3.SqrMagnitude(v1 - orginalIntercept) > proximityOfInterceptThreshold)
                            .Select(v2 => new {
                                vert = v2,
                                dist = ProcGenHelpers.GetMinDist(v2, intercept, rayHit)})
                            .Where(e => e.dist < proximityOfInterceptThreshold)
                            //TODO: Potentially order by proximity to intercept
                            .Select(e => e.vert).ToList();

                        if (hitIntercepts.Count > 0)
                        {
                            //Debug.Log(string.Format("Found path connecting intercepts {0} - {1}", orginalIntercept, hitIntercepts[0]));
                            traceLine.Add(hitIntercepts[0]);
                            return traceLine;
                        }
                    }

                    if (traceLine.Contains(rayHit))
                    {
                        Debug.LogError("Going back over myself");
                        return traceLine;
                    }
                    else {
                        traceLine.Add(rayHit);
                    }

                    int nextTri = GetNeighbourTri(myTris[v + hitEdge], myTris[v + (hitEdge + 1) % 3], tri);
                    if (nextTri != -1) {

                        intercept = rayHit;
                        
                        if (ProcGenHelpers.InterceptionRay(n, intercept, myTriNormals[nextTri], out r))
                        {                            
                            int idThirdVert = GetMissingVert(nextTri, myTris[v + hitEdge], myTris[v + (hitEdge + 1) % 3]);
                            Vector3 d3 = myVerts[idThirdVert] - intercept;
                            float sign = Mathf.Sign(Vector3.Dot(d3, r.direction));
                            /*Debug.Log(string.Format("{3} ({9}) -> {4} ({10}): {0} - {1}, {2} ({5}, {6}) [{7} {8}]", 
                                myTris[v + hitEdge], myTris[v + (hitEdge + 1) % 3], idThirdVert, 
                                tri, nextTri,
                                r.direction, sign, 
                                myVerts[idThirdVert], intercept,
                                myTriNormals[tri], myTriNormals[nextTri]                                
                                ));*/
                            r.direction = sign * r.direction;
                            tri = nextTri;
                        } else
                        {
                            Debug.LogError("The identified next tri didn't intercept cutting Tri");
                            tri = -1;
                        }
                    }
                }

                if (traceLine.Count() >= searchDepth)
                {
                    Debug.LogWarning("Aborted trace because reached search depth");
                    traceLine.Clear();
                    return traceLine;
                }
            }
        }

        traceLine.Clear();
        Debug.Log("Found strange line that started in " + orginalIntercept);
        return traceLine;
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
        for (int i = 0, v = 0; i < myTrisCount; i++, v += 3)
        {
            Vector3 vertA = myVerts[myTris[v]];
            Vector3 vertB = myVerts[myTris[v + 1]];
            Vector3 vertC = myVerts[myTris[v + 2]];

            if (showGizmoNormals)
            {
                Vector3 center = myTriCenters[i];
                Vector3 norm = myTriNormals[i];
                Gizmos.DrawLine(center, center + norm * 0.3f);
            }
            Gizmos.DrawLine(vertA, vertB);
            Gizmos.DrawLine(vertB, vertC);
            Gizmos.DrawLine(vertC, vertA);
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
