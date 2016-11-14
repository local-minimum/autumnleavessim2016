using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomsGenerator : MonoBehaviour {

    [SerializeField]
    FloorGenerator floor;

    [SerializeField]
    WallGenerator walls;

    [SerializeField]
    float gridSize = 1.0f;

    List<float> X = new List<float>();
    List<float> Z = new List<float>();

    List<Vector3> nonConcave = new List<Vector3>();
    List<Vector3> convex = new List<Vector3>();
    List<List<Vector3>> wallLines = new List<List<Vector3>>();
    List<Vector3> perimeter = new List<Vector3>();

    bool generated = false;
    bool generating = false;

    [SerializeField]
    float gizmoSize = 0.2f;

    [SerializeField]
    float wallGizmoYOffset = 0.1f;

    [SerializeField]
    float wallThickness = 0.1f;

    float rotationThreshold = 0.0001f;
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

    }

    void Update () {
	    if (!generated && !generating && floor.Generated)
        {
            MakeGrid();
            generating = true;
            StartCoroutine(_Build());
        } else if (generated && !floor.Generated)
        {
            X.Clear();
            Z.Clear();            
            convex.Clear();
            nonConcave.Clear();
            wallLines.Clear();
            mesh.Clear();
            verts.Clear();
            tris.Clear();
            UVs.Clear();
            generated = false;
            generating = false;
        }
	}

    IEnumerator<WaitForSeconds> _Build()
    {
        int rooms = Mathf.Clamp(Random.Range(1, 3) + Random.Range(1, 4) + Random.Range(2, 4), 4, 8);
        perimeter.Clear();
        perimeter.AddRange(floor.GetCircumferance(false).Select(v => transform.InverseTransformPoint(v)).ToList());        

        yield return new WaitForSeconds(0.2f);
        for (int i=0, l=perimeter.Count; i< l; i++)
        {            
            Vector3 pt = perimeter[i];
            Vector3 rhs = pt - perimeter[(l + i - 1) % l];
            Vector3 lhs = perimeter[(i + 1) % l] - pt;
            float rotation = DotXZ(lhs, rhs);
            if (rotation < -rotationThreshold) {
                //Debug.Log(string.Format("{0}: convex", i));
                convex.Add(pt);
            } else
            {
                //Debug.Log(string.Format("{0}: concave", i));
                nonConcave.Add(pt);
            } 
        }

       
        yield return new WaitForSeconds(0.1f);
        int indx = 0;
        while (rooms > 0)
        {
            Debug.Log(string.Format("{0} remaining walls, {1} convex points", rooms, convex.Count));
            if (convex.Count > 0 && Random.value < 0.1f) {

                Vector3 a = convex[0];
                int idA = perimeter.IndexOf(a);

                List<Vector3> directions = new List<Vector3>() {
                    (a - perimeter[(perimeter.Count + (idA - 1)) % perimeter.Count]).normalized,
                    (perimeter[(idA + 1) % perimeter.Count] - a).normalized
                };

                bool madeRoom = false;
                for (int i=0; i<2; i++)
                {
                    Vector3 pt;
                    if (RayInterceptsSegment(a, directions[i], perimeter, out pt))
                    {
                        List<Vector3> newWall = new List<Vector3>() { a, pt };
                        int testHit;
                        int wallHit;
                        int wallsHit;
                        if (!CollidesWith(newWall, wallLines, out testHit, out wallHit, out wallsHit))
                        {
                            if (wallLines.Contains(newWall))
                            {
                                Debug.LogWarning("Dupe wall");
                            }
                            else
                            {
                                Debug.Log(string.Format("Added simple wall {0} {1}", newWall[0], newWall[1]));
                                madeRoom = true;
                                wallLines.Add(newWall);
                                rooms--;
                                break;
                            }
                        }
                    }
                }
                if (!madeRoom)
                {
                    convex.Remove(a);
                }
                yield return new WaitForSeconds(0.1f);
            }
            else if (convex.Count > 1)
            {
                yield return new WaitForSeconds(0.2f);
                indx %= convex.Count;
                Vector3 a1 = convex[indx];
                int indx2 = (indx + Random.Range(1, convex.Count - 1)) % convex.Count;
                Vector3 a2 = convex[indx2];
                //Debug.Log(string.Format("Using indices {0} {1} ({2})", indx, indx2, convex.Count));
                List<List<Vector3>> testPaths = new List<List<Vector3>>();

                if (a1.x == a2.x || a1.z == a2.z)
                {
                    testPaths.Add(new List<Vector3>() { a1, a2 });
                    //Debug.Log(string.Format("Test simple wall {0} {1}", a1, a2));
                }
                else
                {
                    Vector3[] c = new Vector3[2] { new Vector3(a1.x, 0, a2.z), new Vector3(a2.x, 0, a1.z) };
                    for (int i = 0; i < 2; i++)
                    {
                        testPaths.Add(new List<Vector3>() { a1, c[i], a2 });
                    }
                }

                bool madeRoom = false;
                for (int i = 0, l = testPaths.Count; i < l; i++)
                {
                    List<Vector3> newWall = testPaths[i];
                    int testIndex;
                    int pathIndex;

                    if (CollidesWith(newWall, perimeter, out testIndex, out pathIndex))
                    {
                        Debug.Log(string.Format("Inner wall {0} {1} collides at ({2} | {3})", newWall[testIndex], newWall[testIndex + 1], testIndex, pathIndex));
                    }
                    else
                    {
                        int pathsIndex;
                        if (CollidesWith(newWall, wallLines, out testIndex, out pathIndex, out pathsIndex))
                        {
                            Debug.Log("Collides with inner wall");
                        }
                        else {
                            Debug.Log("Inner wall allowed");
                            if (wallLines.Contains(newWall)) {
                                Debug.LogWarning("Dupe wall");
                            }
                            else {
                                Debug.Log(string.Format("Added curved wall {0} {1} {2}", newWall.Count, newWall[0], newWall[newWall.Count -1]));
                                wallLines.Add(newWall);
                                madeRoom = true;
                                rooms--;
                                if (newWall.Count == 3)
                                {
                                    convex.Add(newWall[1]);
                                }
                                if (Random.value < 0.5f)
                                {
                                    convex.Remove(newWall.First());
                                }
                                else {
                                    convex.Remove(newWall.Last());
                                }
                                yield return new WaitForSeconds(0.2f);
                                break;
                            }
                        }
                    }
                }
                if (!madeRoom)
                {
                    if (Random.value < 0.5f)
                    {
                        convex.Remove(a1);
                    } else
                    {
                        convex.Remove(a2);
                    }
                }

            } else 
            {

                //Min length to divide
                float shortestWall = 2f;
                float longest = Mathf.Pow(shortestWall * 2, 2);
                List<float> lens = new List<float>();
                for (int i=0, l=perimeter.Count; i< l; i++)
                {
                    int nextI = (i + 1) % l;
                    lens.Add((perimeter[nextI] - perimeter[i]).sqrMagnitude);
                }
                int c = lens.Where(e => e > longest).Count();

                if (c > 0)
                {
                    int v = Random.Range(0, c) + 1;

                    //TODO: need to use also inner walls I thinks

                    List<int> sums = new List<int>();
                    lens.Aggregate(0, (sum, e) => { sum += e > longest ? 1 : 0; sums.Add(sum); return sum; });
                    int idLong = sums.IndexOf(v);

                    longest = Mathf.Sqrt(lens[idLong]);
                    float flexPos = longest - 2 * shortestWall;
                    int nextI = (idLong + 1) % perimeter.Count;
                    Vector3 pt = Vector3.Lerp(perimeter[idLong], perimeter[nextI], (Random.value * flexPos + shortestWall) / longest);
                    Vector3 d = (pt - perimeter[idLong]).normalized;
                    //Rotate CW
                    d = new Vector3(d.z, 0, -d.x);
                    Vector3 ptB;
                    //Debug.Log(string.Format("{0} - {1}, {2}, d {3}", permimeter[idLong], permimeter[nextI], pt, d));
                    
                    if (RayInterceptsSegment(pt, d, perimeter, out ptB))
                    {
                        bool perim2Perim = true;
                        Vector3 ptC;
                        if (RayInterceptsSegment(pt, d, wallLines, out ptC))
                        {
                            if ((ptC - pt).sqrMagnitude < (ptB - pt).sqrMagnitude)
                            {
                                ptB = ptC;
                                perim2Perim = false;
                            }
                        }

                        if ((ptB - pt).magnitude > shortestWall)
                        {
                            wallLines.Add(new List<Vector3>() { pt, ptB });
                            rooms--;
                            perimeter.Insert(idLong + 1, pt);
                            if (perim2Perim)
                            {
                                for (int i=0, l=perimeter.Count; i< l; i++)
                                {
                                    int j = (i + 1) % l;
                                    if (PointOnSegment(perimeter[i], perimeter[j], ptB))
                                    {
                                        perimeter.Insert(i + 1, ptB);
                                        Debug.Log("Inserted perim to perim");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else {
                    Debug.Log("Out of options");
                    rooms = 0;
                }

                yield return new WaitForSeconds(0.1f);
            }
            indx++;
        }

        foreach (List<Vector3> innerWall in wallLines)
        {
            ConstructInnerWall(innerWall);
            yield return new WaitForSeconds(0.1f);
            innerWall.Reverse();
            ConstructInnerWall(innerWall);
            yield return new WaitForSeconds(0.1f);
        }

        generated = true;
        generating = false;
    }

    void ConstructInnerWall(List<Vector3> innerWall)
    {
        int wallL = innerWall.Count;
        List<Vector3> lateralWall = new List<Vector3>();
        for (int wallId = 0; wallId < wallL - 1; wallId++)
        {
            float wallOrthoOff = WallCWOrthoOffset(innerWall, wallId);

            Vector3 para = (innerWall[wallId + 1] - innerWall[wallId]).normalized;
            Vector3 orth = new Vector3(para.z, 0, -para.x);

            lateralWall.Add(innerWall[wallId] + orth * wallThickness * wallOrthoOff);
            if (wallId == wallL - 2)
            {
                lateralWall.Add(innerWall[wallId + 1] + orth * wallThickness * wallOrthoOff);
            }
        }

        Vector3 up = new Vector3(0, walls.Height);        
        for (int wallId = 0, n = verts.Count; wallId < wallL - 1; wallId++, n+=2)
        {
            if (wallId == 0)
            {
                verts.Add(innerWall[wallId]);
                verts.Add(innerWall[wallId] + up);

                UVs.Add(new Vector2(wallId % 2, 0));
                UVs.Add(new Vector2(wallId % 2, 1));

            }

            verts.Add(innerWall[wallId + 1]);
            verts.Add(innerWall[wallId + 1] + up);

            UVs.Add(new Vector2((wallId + 1) % 2, 0));
            UVs.Add(new Vector2((wallId + 1) % 2, 1));

            
            tris.Add(n);
            tris.Add(n + 1);
            tris.Add(n + 2);

            tris.Add(n + 2);
            tris.Add(n + 1);
            tris.Add(n + 3);

            mesh.Clear();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, UVs);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            Debug.Log("Wall built");
        }

    }

    float WallCWOrthoOffset(List<Vector3> innerWall, int wallSegment)
    {
        int l = perimeter.Count;
        int idPerim = perimeter.IndexOf(innerWall[wallSegment]);

        if (idPerim >= 0)
        {
            Vector3 pt = perimeter[idPerim];
            Vector3 rhs = pt - perimeter[(l + idPerim - 1) % l];
            Vector3 lhs = perimeter[(idPerim + 1) % l] - pt;
            if (Sign(DotXZ(lhs, rhs)) == -1)
            {
                if (Sign(DotXZ(rhs, innerWall[wallSegment + 1] - innerWall[wallSegment])) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
        }
        else if (wallSegment == l - 2)
        {
            idPerim = perimeter.IndexOf(innerWall[wallSegment]);
            if (idPerim >= 0)
            {
                Vector3 pt = perimeter[idPerim];
                Vector3 rhs = pt - perimeter[(l + idPerim - 1) % l];
                Vector3 lhs = perimeter[(idPerim + 1) % l] - pt;
                if (Sign(DotXZ(lhs, rhs)) == -1)
                {
                    if (Sign(DotXZ(rhs, innerWall[wallSegment - 1] - innerWall[wallSegment])) == 0)
                    {
                        return 1;
                    } else
                    {
                        return 0;
                    }
                }
            }
        }
        return 0.5f;
    }

    bool RayInterceptsSegment(Vector3 source, Vector3 direction, List<Vector3> line, out Vector3 pt)
    {

        for (int i=0, l=line.Count; i<l; i++)
        {
            Vector3 q1 = line[i];
            Vector3 q2 = line[(i + 1) % l];
            if (source == q1 || source == q2 || PointInsideSegment(q1, q2, source))
            {
                //Not interested in source on line
                continue;
            }

            Vector3 v1 = source - q1;
            Vector3 v2 = q2 - q1;
            Vector3 v3 = direction;

            float t1 = Vector3.Cross(new Vector3(v2.x, v2.z), new Vector3(v1.x, v1.z)).magnitude / DotXZ(v2, v3);
            float t2 = DotXZ(v1, v3) / DotXZ(v2, v3);
            if (t2 >= 0 && t2 <= 1)
            {
                if (t1 > 0)
                {

                    pt = Vector3.Lerp(q1, q2, t2);
                    return true;
                }
            }
        }

        pt = Vector3.zero;
        return false;
    }

    bool RayInterceptsSegment(Vector3 source, Vector3 direction, List<List<Vector3>> lines, out Vector3 pt)
    {
        Vector3 closest = Vector3.zero;
        float smallest = 0;
        bool any = false;
        foreach (List<Vector3> line in lines)
        {
            if (RayInterceptsSegment(source, direction, line, out pt))
            {
                if (!any || smallest > (pt - source).sqrMagnitude)
                {
                    closest = pt;
                    smallest = (pt - source).sqrMagnitude;
                    any = true;
                }
                
            }
        }
        pt = closest;
        return any;
    }

    float DotXZ(Vector3 lhs, Vector3 rhs)
    {
        return lhs.x * rhs.z - lhs.z * rhs.x;
    }

    bool PointInsideSegment(Vector3 a, Vector3 b, Vector3 pt)
    {
        return Sign(DotXZ(a - pt, b - pt)) == 0 && pt.x < Mathf.Max(a.x, b.x) && pt.x > Mathf.Min(a.x, b.x) && pt.z < Mathf.Max(a.z, b.z) && pt.z > Mathf.Min(a.z, b.z);
    }

    bool PointOnSegment(Vector3 a, Vector3 b, Vector3 pt)
    {
        return Sign(DotXZ(a - pt, b - pt)) == 0 && pt.x <= Mathf.Max(a.x, b.x) && pt.x >= Mathf.Min(a.x, b.x) && pt.z <= Mathf.Max(a.z, b.z) && pt.z >= Mathf.Min(a.z, b.z);

    }

    int Sign(float v)
    {
        if (v < -rotationThreshold)
        {
            return -1;
        } else if (v > rotationThreshold)
        {
            return 1;
        } else
        {
            return 0;
        }
    }

    bool CollidesWith(Vector3 p1, Vector3 p2, List<Vector3> referencePath, out int index)
    {
        for (int i = 0, l = referencePath.Count - 1; i < l; i++)
        {
            Vector3 q1 = referencePath[i];
            Vector3 q2 = referencePath[i + 1];

            float aP1P2Q1 = Sign(DotXZ(p1 - p2, q1 - p2));
            float aP1P2Q2 = Sign(DotXZ(p1 - p2, q2 - p2));

            float aQ1Q2P1 = Sign(DotXZ(q1 - q2, p1 - q2));
            float aQ1Q2P2 = Sign(DotXZ(q1 - q2, p2 - q2));

            //Debug.Log(string.Format("Rotations {0} {1} {2} {3}", aP1P2Q1, aP1P2Q2, aQ1Q2P1, aQ1Q2P2));

            if (aP1P2Q1 != aP1P2Q2 && aQ1Q2P1 != aQ1Q2P2)
            {
                if (p1 != q1 && p1 != q2 && p2 != q1 && p2 != q2)
                {
                    //Debug.Log("Angle intercept");
                    index = i;
                    return true;
                }

            }
            else if (aP1P2Q1 == 0 && aP1P2Q2 == 0 && aQ1Q2P1 == 0 && aQ1Q2P2 == 0)
            {
                //Debug.Log("Inline point");
                if (PointInsideSegment(q1, q2, p1) || PointInsideSegment(q1, q2, p2))
                {
                    //Debug.Log(string.Format("Linear intercept {0} ({1} {2} {3}), {4} ({5} {6} {7})", PointInsideSegment(q1, q2, p1), q1, q2, p1, PointInsideSegment(q1, q2, p2), q1, q2, p2));
                    index = i;
                    return true;
                } else
                {
                    if (p1 == q1 && p2 == q2 || p1 == q2 && p2 == q1)
                    {
                        //Debug.Log(string.Format("Identical lines"));
                        index = i;
                        return true;
                    }
                }
            }

        }

        index = -1;
        return false;
    }

    bool CollidesWith(List<Vector3> testPath, List<Vector3> referencePath, out int testIndex, out int refIndex)
    {
        for (int i = 0, l = testPath.Count - 1; i < l; i++)
        {
            if (CollidesWith(testPath[i], testPath[i + 1], referencePath, out refIndex)) {
                testIndex = i;
                return true;
            }
        }
        testIndex = -1;
        refIndex = -1;
        return false;
    }

    bool CollidesWith(List<Vector3> testPath, List<List<Vector3>> referencePaths, out int testIndex, out int refIndex, out int pathsIndex)
    {
        for (int i=0, l=referencePaths.Count; i< l; i++)
        {            
            if (CollidesWith(testPath, referencePaths[i], out testIndex, out refIndex))
            {
                pathsIndex = i;
                return true;
            }
        }

        testIndex = -1;
        refIndex = -1;
        pathsIndex = -1;
        return false;
    }

    void MakeGrid()
    {

        List<Vector3> points = floor.GetCircumferance(false).Select(v => transform.InverseTransformPoint(v)).ToList();
        if (points.Count == 0)
        {
            return;
        }


        for (int i = 0; i < points.Count; i++)
        {
            if (!X.Contains(points[i].x))
            {
                X.Add(points[i].x);
            }

            if (!Z.Contains(points[i].z))
            {
                Z.Add(points[i].z);
            }
        }

        Z.Sort();
        X.Sort();
        for (int i = 1, l = Z.Count; i < l; i++)
        {
            if (Z[i] - Z[i - 1] > gridSize)
            {
                int n = Mathf.FloorToInt((Z[i] - Z[i - 1]) / gridSize) + 1;
                for (int j = 1; j < n; j++)
                {
                    Z.Add(Mathf.Lerp(Z[i - 1], Z[i], ((float)j) / n));
                }
            }
        }

        for (int i = 1, l = X.Count; i < l; i++)
        {
            if (X[i] - X[i - 1] > gridSize)
            {
                int n = Mathf.FloorToInt((X[i] - X[i - 1]) / gridSize) + 1;
                for (int j = 1; j < n; j++)
                {
                    X.Add(Mathf.Lerp(X[i - 1], X[i], ((float)j) / n));
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;

        if (generating && Z.Count != 0 && X.Count != 0)
        {

            float zMin = Z.Min();
            float zMax = Z.Max();
            float xMin = X.Min();
            float xMax = X.Max();

            foreach (float x in X)
            {
                Gizmos.DrawLine(
                    transform.TransformPoint(new Vector3(x, 0, zMin)),
                    transform.TransformPoint(new Vector3(x, 0, zMax)));
            }

            foreach (float z in Z)
            {
                Gizmos.DrawLine(
                    transform.TransformPoint(new Vector3(xMin, 0, z)),
                    transform.TransformPoint(new Vector3(xMax, 0, z)));
            }
        }

        Gizmos.color = Color.blue;
        foreach(Vector3 v in nonConcave)
        {
            Gizmos.DrawSphere(transform.TransformPoint(v), gizmoSize);
        }
        Gizmos.color = Color.green;
        foreach(Vector3 v in convex)
        {
            Gizmos.DrawCube(transform.TransformPoint(v), Vector3.one * gizmoSize * 2);
        }

        Gizmos.color = Color.cyan;
        Vector3 yoff = Vector3.up * wallGizmoYOffset;
        foreach (List<Vector3> path in wallLines)
        {
            for (int i = 0, l = path.Count - 1; i < l; i++) {
                Gizmos.DrawLine(transform.TransformPoint(path[i] + yoff), transform.TransformPoint(path[i + 1] + yoff));
            }
        }
    }

}
