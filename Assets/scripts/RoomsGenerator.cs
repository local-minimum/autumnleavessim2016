using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomsGenerator : MonoBehaviour {

    enum RoomBuildType
    {
        ConvexCornerToConvexCorner,
        ConvexCornerToStraightToWall,
        WallStraightToWall
    };

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

    public bool Generated
    {
        get
        {
            return generated;
        }
    }

    [SerializeField]
    float gizmoSize = 0.2f;

    [SerializeField]
    float wallGizmoYOffset = 0.1f;

    [SerializeField]
    float wallThickness = 0.1f;

    [SerializeField]
    float shortestWall = 2f;

    List<Vector3> verts = new List<Vector3>();
    List<Vector2> UVs = new List<Vector2>();
    List<int> tris = new List<int>();
    
    Mesh mesh;

    void Start()
    {
        MeshFilter mFilt = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.name = "ProcGen Walls";
        mFilt.mesh = mesh;
        MeshCollider mCol = GetComponent<MeshCollider>();
        if (mCol != null)
        {
            mCol.sharedMesh = mesh;
        }
    }

    void Update () {
	    if (!generated && !generating && floor.Generated)
        {
            //MakeGrid();
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
        //TODO: Best way to remove convex points
        //TODO: Instert point in next wall behind current rather than far wall
        //TODO: Allow corner to inner wall
        //TODO: Reinstate linear points?
        //TODO: Linear points to wall building.
        //TODO: Allow inner wall free building.

        int rooms = Mathf.Clamp(Random.Range(1, 3) + Random.Range(1, 4) + Random.Range(2, 4), 4, 7);
        perimeter.Clear();
        perimeter.AddRange(floor.GetCircumferance(false).Select(v => transform.InverseTransformPoint(v)).ToList());        

        yield return new WaitForSeconds(0.2f);
        for (int i=0, l=perimeter.Count; i< l; i++)
        {            
            Vector3 pt = perimeter[i];
            Vector3 rhs = pt - perimeter[(l + i - 1) % l];
            Vector3 lhs = perimeter[(i + 1) % l] - pt;
            int rotation = ProcGenHelpers.XZRotation(lhs, rhs);
            if (rotation == -1) {
                //Debug.Log(string.Format("{0}: convex", i));
                convex.Add(pt);
            } else if (rotation == 1)
            {
                //Debug.Log(string.Format("{0}: concave", i));
                nonConcave.Add(pt);
            } 
        }

       
        yield return new WaitForSeconds(0.1f);
        int attempts = 0;
                
        while (rooms > 0 && attempts < 30)
        {
            RoomBuildType nextRoom = NextRoomType;
            if (nextRoom == RoomBuildType.ConvexCornerToStraightToWall) {

                if (MapCornerStraightWall())
                {
                    rooms--;
                }
            }
            else if (nextRoom == RoomBuildType.ConvexCornerToConvexCorner)
            {
                if (MapCornerToCornerWall())
                {
                    rooms--;
                }

            } else if (nextRoom == RoomBuildType.WallStraightToWall)
            {
                if (MapWallToWall())
                {
                    rooms--;
                }
            }

            attempts++;
            yield return new WaitForSeconds(0.1f);
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

    RoomBuildType NextRoomType
    {
        get
        {
            int convexCount = convex.Count;
            if (true)
            {
                return RoomBuildType.ConvexCornerToStraightToWall;
            }
            if (convexCount < 1)
            {
                return RoomBuildType.WallStraightToWall;
            } else if (convexCount < 2)
            {
                if (Random.value < 0.7f)
                {
                    return RoomBuildType.ConvexCornerToStraightToWall;
                } else
                {
                    return RoomBuildType.WallStraightToWall;
                }
            } else
            {
                float v = Random.value;
                if (v < 0.6f)
                {
                    return RoomBuildType.ConvexCornerToConvexCorner;
                } else if (v < 0.9f)
                {
                    return RoomBuildType.ConvexCornerToStraightToWall;
                } else
                {
                    return RoomBuildType.WallStraightToWall;
                }
            }
        }
    }

    bool MapWallToWall()
    {
        bool room = false;
        //Min length to divide
        float longest = Mathf.Pow(shortestWall * 2, 2);
        List<float> lens = new List<float>();
        for (int i = 0, l = perimeter.Count; i < l; i++)
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

            Vector3 d = ProcGenHelpers.Get90CW(pt - perimeter[idLong]).normalized;

            Vector3 ptB;
            Debug.Log(string.Format("{0} - {1}, {2}, d {3}", perimeter[idLong], perimeter[nextI], pt, d));

            if (ProcGenHelpers.RayInterceptsSegment(pt, d, perimeter, out ptB))
            {
                bool perim2Perim = true;
                Vector3 ptC;
                bool allowWall = true;
                int idLine;
                if (ProcGenHelpers.RayInterceptsSegment(pt, d, wallLines, out ptC, out idLine))
                {
                    if (ProcGenHelpers.TooClose(pt, wallLines[idLine], shortestWall))
                    {
                        allowWall = false;
                    }
                    else
                    {
                        Debug.Log("Wall construction intercept inner wall");
                        nonConcave.Add(ptB);
                        if ((ptC - pt).sqrMagnitude < (ptB - pt).sqrMagnitude)
                        {
                            ptB = ptC;
                            perim2Perim = false;
                        }
                    }
                }

                if (allowWall && (ptB - pt).magnitude > shortestWall && !ProcGenHelpers.TooClose(pt, perimeter, shortestWall))
                {
                    wallLines.Add(new List<Vector3>() { pt, ptB });
                    if (!nonConcave.Contains(pt))
                    {
                        nonConcave.Add(pt);
                    }
                    if (!nonConcave.Contains(ptB))
                    {
                        nonConcave.Add(ptB);
                    }
                    room = true;
                    perimeter.Insert(idLong + 1, pt);

                    if (perim2Perim)
                    {
                        for (int i = 0, l = perimeter.Count; i < l; i++)
                        {
                            int j = (i + 1) % l;
                            if (ProcGenHelpers.PointOnSegment(perimeter[i], perimeter[j], ptB))
                            {
                                perimeter.Insert(i + 1, ptB);
                                Debug.Log("Inserted perim to perim");
                                break;
                            }
                        }
                    }

                    Debug.Log("Inserted free wall");
                }
            }
        }

        return room;

    }

    bool MapCornerToCornerWall()
    {
        bool room = false;
        int indx = Random.Range(0, convex.Count);
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

        for (int i = 0, l = testPaths.Count; i < l; i++)
        {
            List<Vector3> newWall = testPaths[i];
            int testIndex;
            int pathIndex;

            if (ProcGenHelpers.CollidesWith(newWall, perimeter, out testIndex, out pathIndex))
            {
                Debug.Log(string.Format("Inner wall {0} {1} collides at ({2} | {3})", newWall[testIndex], newWall[testIndex + 1], testIndex, pathIndex));
            }
            else
            {
                int pathsIndex;
                if (ProcGenHelpers.CollidesWith(newWall, wallLines, out testIndex, out pathIndex, out pathsIndex))
                {
                    Debug.Log("Collides with inner wall");
                }
                else {
                    //Debug.Log("Inner wall allowed");
                    if (wallLines.Contains(newWall))
                    {
                        Debug.LogWarning("Dupe wall");
                    }
                    else {
                        //Debug.Log(string.Format("Added curved wall {0} {1} {2}", newWall.Count, newWall[0], newWall[newWall.Count -1]));
                        wallLines.Add(newWall);
                        room = true;
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
                        break;
                    }
                }
            }
        }

        if (room)
        {
            if (Random.value < 0.5f)
            {
                convex.Remove(a1);
            }
            else
            {
                convex.Remove(a2);
            }
        }
    
        return room;
    }

    bool MapCornerStraightWall()
    {
        bool room = false;
        Vector3 a = convex[Random.Range(0, convex.Count)];
        int idA = perimeter.IndexOf(a);
        List<Vector3> directions = new List<Vector3>();
        if (idA >= 0)
        {
            directions.Add((a - perimeter[(perimeter.Count + (idA - 1)) % perimeter.Count]).normalized);
            directions.Add((a - perimeter[(idA + 1) % perimeter.Count]).normalized);

        }
        else
        {
            foreach (List<Vector3> iWall in wallLines)
            {
                if (iWall.Contains(a))
                {
                    idA = iWall.IndexOf(a);
                    directions.Add((a - iWall[idA - 1]).normalized);
                    directions.Add((a - iWall[idA + 1]).normalized);
                    break;
                }
            }
        }

        Vector3 pt = Vector3.zero;

        for (int i = 0; i < directions.Count; i++)
        {
            if (ProcGenHelpers.RayInterceptsSegment(a, directions[i], perimeter, out pt))
            {
                int wallsHit;
                Vector3 pt2;
                if (ProcGenHelpers.RayInterceptsSegment(a, directions[i], wallLines, out pt2, out wallsHit))
                {
                    if (!ProcGenHelpers.TooClose(pt, wallLines[wallsHit], shortestWall))
                    {
                        pt = pt2;
                        Debug.Log("Corner to inner wall");
                        room = true;
                        break;
                    }
                }
                else if (!ProcGenHelpers.TooClose(pt, perimeter, shortestWall))
                {
                    Debug.Log("Corner to outer wall");
                    room = true;
                    break;
                }
            }
        }

        if (room)
        {
            Debug.Log(string.Format("Added simple wall {0} {1}", a, pt));
            nonConcave.Add(pt);
            wallLines.Add(new List<Vector3>() { a, pt });
            if (!nonConcave.Contains(pt))
            {
                nonConcave.Add(pt);
            }
        }

        return room;
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
            if (ProcGenHelpers.XZRotation(lhs, rhs) == -1)
            {
                if (ProcGenHelpers.XZRotation(rhs, innerWall[wallSegment + 1] - innerWall[wallSegment]) == 0)
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
                if (ProcGenHelpers.XZRotation(lhs, rhs) == -1)
                {
                    if (ProcGenHelpers.XZRotation(rhs, innerWall[wallSegment - 1] - innerWall[wallSegment]) == 0)
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
