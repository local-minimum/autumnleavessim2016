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

    [SerializeField]
    bool showGizmos = true;

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
        bool showGizmos = this.showGizmos;
        this.showGizmos = false;
        bool first = true;
        foreach (Collider col in Physics.OverlapBox(transform.position, transform.TransformDirection(boxSize) / 2f, transform.rotation, collisionLayers)) 
        {
            MeshFilter mFilt = col.GetComponent<MeshFilter>();
            if (mFilt != null)
            {
                if (first)
                {
                    RecalculateMeshlike();
                    first = false;
                }
                CutDough(mFilt.sharedMesh, mFilt);                
            }
            
        }
        this.showGizmos = showGizmos;        
    }

    List<int> cuttingLines = new List<int>();
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

    float thresholdPointToCutProximity = .001f;

    public Dictionary<int, List<Vector3>> GetCuts(Vector3[] verts, int[] tris, int start, Vector3 normal, int len = 3)
    {
        Dictionary<int, List<Vector3>> cuts = new Dictionary<int, List<Vector3>>();
        int end = start + len;
        int last = end - 1;
        for (int i = start; i < end; i++)
        {
            Vector3 vert = verts[tris[i]];
            Vector3 nextV = verts[tris[(i == last) ? start : i + 1]];

            cuts[i] = GetLineCutIntercepts(vert, nextV, normal)
                .Where(v => Vector3.SqrMagnitude(v - vert) > thresholdPointToCutProximity && Vector3.SqrMagnitude(v - nextV) > thresholdPointToCutProximity)
                .Select(v => new { vert = v, dist = Vector3.SqrMagnitude(v - vert) })
                .OrderBy(e => e.dist)
                .Select(e => e.vert)
                .ToList();            
        }
        return cuts;
    }

    public List<List<Vector3>> GetSubPaths(Vector3[] verts, int[] tris, int start, Vector3 normal, Dictionary<int, List<Vector3>> cuts, List<Vector3> allIntercepts, int len = 3)
    {
        List<List<Vector3>> subPaths = new List<List<Vector3>>();

        if (allIntercepts.Count() == 1)
        {
            Debug.Log("Not safe distance to original tri vert to make cut");
            return subPaths;            
        }

        int curSubPath = 0;
        Dictionary<int, List<Vector3>> interceptTris = GetInterceptTris(allIntercepts.ToArray());
        List<KeyValuePair<int, int>> triCorners = new List<KeyValuePair<int, int>>();
        List<Vector3> triVerts = new List<Vector3>();
        for (int i=start, end=start + len, j=0; i< end; i++, j++)
        {
            triVerts.Add(verts[tris[i]]);
            triCorners.Add(new KeyValuePair<int, int>(j, tris[i]));
        }

        List<int> cornersInCurSubPath = new List<int>();
        int curCorner = triCorners[0].Value;
        int triCurCornerIndex = triCorners[0].Key;
        bool keepTrying = true;
        while (triCorners.Count() > 0 || keepTrying)
        {
            bool walkingSubPath = false;
            //Debug.Log(string.Format("Current sub path {0}, {1} known", curSubPath, subPaths.Count()));
            if (PointInMesh(verts[curCorner]) || curSubPath >= subPaths.Count())
            {
                if (triCorners.Count() > 0)
                {
                    //Debug.Log(corners.Count() + " corners remaining");
                    if (subPaths.Count() == 0 || subPaths[subPaths.Count() - 1].Count() > 0)
                    {
                        subPaths.Add(new List<Vector3>());
                    }
                    curCorner = triCorners[0].Value;
                    triCurCornerIndex = triCorners[0].Key;
                    triCorners.RemoveAt(0);
                    cornersInCurSubPath.Clear();

                }
                else
                {
                    //Debug.Log("Out of corners");
                    keepTrying = false;
                }
                continue;
            }

            //Debug.Log(string.Format("Adding corner {0} to path {1}", curCorner, curSubPath));
            cornersInCurSubPath.Add(curCorner);
            subPaths[curSubPath].Add(verts[curCorner]);

            if (cuts[curCorner].Count() > 0)
            {
                Vector3 intercept = cuts[curCorner][0]; //First cut is going into cutter always
                List<int> iTris = interceptTris.Where(kvp => kvp.Value.Contains(intercept)).Select(kvp => kvp.Key).ToList();
                if (iTris.Count() == 1)
                {
                    int tri = iTris[0];
                    Vector3 thirdVert = triVerts[(triCurCornerIndex + 2) % 3];
                    List<Vector3> cutLine = TraceSurface(tri, thirdVert, intercept, normal, interceptTris);
                    if (cutLine.Count() > 0)
                    {
                        if (cutLine.Where((v, idV) => idV < cutLine.Count() - 1).All(v => ProcGenHelpers.PointInTriangle(triVerts[0], triVerts[1], triVerts[2], v)))
                        {
                            cutLine.Insert(0, intercept);

                            subPaths[curSubPath].AddRange(cutLine);

                            walkingSubPath = true;

                            int nextEdge = ProcGenHelpers.GetClosestSegment(triVerts, cutLine.Last());
                            if (nextEdge < 0)
                            {
                                walkingSubPath = false;
                                Debug.LogError("Lost track of where we are, now everything is possible. Unfortunately.");
                            }
                            else {
                                nextEdge = (nextEdge + 1) % triVerts.Count();
                                //Debug.Log(nextEdge);
                                if (cornersInCurSubPath.Contains(nextEdge))
                                {
                                    //Debug.Log(string.Format("Closing {0} SubPath with corner {1}", curSubPath, curCorner));
                                    curSubPath++;
                                }
                                else
                                {
                                    triCurCornerIndex = triCorners.Where(e=> e.Value == nextEdge).First().Key;
                                    if (!triCorners.Remove(triCorners.Where(e => e.Value == nextEdge).First()))
                                    {
                                        Debug.LogWarning(string.Format("Seems like we are revisting corner #{0}, this should not have happened.", nextEdge));
                                    }
                                    else
                                    {
                                        //Debug.Log(string.Format("Next edge {0} is not current corner {1} for sub-path {2}", nextEdge, curCorner, curSubPath));
                                    }
                                    curCorner = nextEdge;
                                }
                            }
                        }
                        else {
                            Debug.LogWarning("Cutting the triangle outside triangle is bad idea.");
                        }

                    }
                    else
                    {
                        Debug.LogWarning("Empty cut");
                    }
                }
                else {
                    Debug.LogWarning("Intercept is duplicated");
                }
            }

            if (!walkingSubPath)
            {
                if (triCorners.Count() > 0)
                {
                    curCorner = triCorners[0].Value;
                    triCurCornerIndex = triCorners[0].Key;
                    triCorners.RemoveAt(0);
                }
                else
                {
                    keepTrying = false;
                }
            }
        }

        return subPaths;
    }

    public void CutDough(Mesh dough, MeshFilter mFilt)
    {
        Debug.Log("Cutting " + mFilt.gameObject);
        doughTransform = mFilt.transform;

        List<Vector3> allIntercepts = new List<Vector3>();
        Vector3[] verts = dough.vertices.Select(v => doughTransform.TransformPoint(v)).ToArray();
        int[] tris = dough.triangles;
        Vector2[] uvs = dough.uv;
        List<Vector3> newVerts = new List<Vector3>();
        List<Vector2> newUVs = new List<Vector2>();
        List<int> newTris = new List<int>();

        for (int triStart = 0, nTris = tris.Length; triStart < nTris; triStart += 3)
        {
            Vector3[] triCorners = new Vector3[3] { verts[tris[triStart]], verts[tris[triStart + 1]], verts[tris[triStart + 2]] };
            if (triCorners.All(v => PointInMesh(v))) { 
                continue;
            }
            Vector2[] triUVs = new Vector2[3] {uvs[tris[triStart]], uvs[tris[triStart + 1]], uvs[tris[triStart + 2]]};
            Vector3 normal = Vector3.Cross(triCorners[1] - triCorners[0], triCorners[2] - triCorners[0]).normalized;
            int[] outTriangle = new int[3] { 0, 1, 2 };

            Dictionary<int, List<Vector3>> cuts = GetCuts(triCorners, outTriangle, 0, normal);
            foreach (List<Vector3> cutz in cuts.Values)
            {
                allIntercepts.AddRange(cutz);
            }

            if (allIntercepts.Count() > 0)
            {
                List<List<Vector3>> subPaths = GetSubPaths(triCorners, outTriangle, 0, normal, cuts, allIntercepts);

                for (int subP = 0; subP < subPaths.Count(); subP++)
                {
                    newTris.AddRange(ProcGenHelpers.PolyToTriangles(subPaths[subP], normal, newVerts.Count()));
                    newVerts.AddRange(subPaths[subP]);
                    newUVs.AddRange(ProcGenHelpers.GetProjectedUVs(triCorners, triUVs, subPaths[subP]));
                }
            } else
            {
                //Triangle outside cutter entirely
                newVerts.AddRange(triCorners);
                newUVs.AddRange(triUVs);
                for (int i = 0; i < 3; i++) {
                    newTris.Add(newVerts.Count());
                }
            }
        }

        Mesh cutDough = new Mesh();
        cutDough.name = dough.name + ".CCut";

        cutDough.SetVertices(newVerts.Select(v => doughTransform.InverseTransformPoint(v)).ToList());
        cutDough.SetUVs(0, newUVs);
        cutDough.SetTriangles(newTris, 0);
        cutDough.RecalculateBounds();
        cutDough.RecalculateNormals();

        mFilt.sharedMesh = cutDough;
    }

    public void OnDrawGizmosSelected()
    {
        if (showGizmos)
        {
            RecalculateMeshlike();
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

        }
    }
}
