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

    List<Vector3> concave = new List<Vector3>();
    List<Vector3> convex = new List<Vector3>();
    List<Vector3> linear = new List<Vector3>();
    List<List<Vector3>> wallLines = new List<List<Vector3>>();

    bool generated = false;
    bool generating = false;

    [SerializeField]
    float gizmoSize = 0.2f;

    [SerializeField]
    float wallGizmoYOffset = 0.1f;

    void Start () {
	
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
            generated = false;
            generating = false;
        }
	}

    IEnumerator<WaitForSeconds> _Build()
    {
        int rooms = Mathf.Min(7, Random.Range(1, 3) + Random.Range(1, 4) + Random.Range(1, 2));

        List<Vector3> permimeter = floor.GetCircumferance(false).Select(v => transform.InverseTransformPoint(v)).ToList();

        linear.Clear();
        convex.Clear();
        concave.Clear();
        wallLines.Clear();

        for (int i=0, l=permimeter.Count; i< l; i++)
        {            
            Vector3 pt = permimeter[i];
            Vector3 rhs = permimeter[(l + i - 1) % l] - pt;
            Vector3 lhs = permimeter[(i + 1) % l] - pt;
            float rotation = DotXZ(lhs, rhs);

            if (rotation == 0) {
                //Debug.Log(string.Format("{0}: linear", i));
                linear.Add(pt);
            } else if (rotation > 0) {
                //Debug.Log(string.Format("{0}: convex", i));
                convex.Add(pt);
            } else {
                //Debug.Log(string.Format("{0}: concave", i));
                concave.Add(pt);
            }
        }

       
        yield return new WaitForSeconds(0.01f);

        if (convex.Count >= 2)
        {
            //TODO: Make interesting selection

            Vector3 a1 = convex[0];
            Vector3 a2 = convex[1];

            List<List<Vector3>> testPaths = new List<List<Vector3>>();

            if (a1.x == a2.x || a1.z == a2.z)
            {
                testPaths.Add(new List<Vector3>() { a1, a2 });

            } else
            {
                Vector3[] c = new Vector3[2] { new Vector3(a1.x, 0, a2.z), new Vector3(a2.x, 0, a1.z) };
                for (int i = 0; i < 2; i++)
                {
                    testPaths.Add(new List<Vector3>() { a1, c[i], a2 });
                }
            }

            for (int i = 0, l=testPaths.Count; i < l; i++)
            {
                List<Vector3> testPath = testPaths[i];
                int testIndex;
                int pathIndex;

                if (CollidesWith(testPath, permimeter, out testIndex, out pathIndex))
                {
                    Debug.Log(string.Format("Inner wall {0} {1} collides at ({3} | {4})", testPath[testIndex], testPath[testIndex + 1], testIndex, pathIndex));
                } else
                {
                    Debug.Log(string.Format("Inner wall allowed"));
                    wallLines.Add(testPath);
                    break;
                }
            }
        }

        generated = true;
        generating = false;
    }

    float DotXZ(Vector3 lhs, Vector3 rhs)
    {
        return lhs.x * rhs.z - lhs.z * rhs.x;
    }

    bool PointInsideSegment(Vector3 a, Vector3 b, Vector3 pt)
    {
        return DotXZ(a - pt, b - pt) == 0 && pt.x < Mathf.Max(a.x, b.x) && pt.x > Mathf.Min(a.x, b.x) && pt.z < Mathf.Max(a.z, b.z) && pt.z > Mathf.Min(a.z, b.z);
    }

    bool CollidesWith(Vector3 p1, Vector3 p2, List<Vector3> referencePath, out int index)
    {
        for (int i = 0, l = referencePath.Count - 1; i < l; i++)
        {
            Vector3 q1 = referencePath[i];
            Vector3 q2 = referencePath[i + 1];

            float aP1P2Q1 = DotXZ(p1 - p2, q1 - p2);
            float aP1P2Q2 = DotXZ(p1 - p2, q2 - p2);

            float aQ1Q2P1 = DotXZ(q1 - q2, p1 - q2);
            float aQ1Q2P2 = DotXZ(q1 - q2, p2 - q2);

            if (aP1P2Q1 != aP1P2Q2 && aQ1Q2P1 != aQ1Q2P2)
            {
                index = i;
                return true;

            }
            else if (aP1P2Q1 == 0 && aP1P2Q2 == 0 && aQ1Q2P1 == 0 && aQ1Q2P2 == 0 && (PointInsideSegment(q1, q2, p1) || PointInsideSegment(q1, q2, p2)))
            {
                index = i;
                return true;
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

        if (Z.Count == 0 || X.Count == 0)
        {
            return;
        }

        float zMin = Z.Min();
        float zMax = Z.Max();
        float xMin = X.Min();
        float xMax = X.Max();

        foreach(float x in X)
        {
            Gizmos.DrawLine(
                transform.TransformPoint(new Vector3(x, 0, zMin)),
                transform.TransformPoint(new Vector3(x, 0, zMax)));
        }

        foreach(float z in Z)
        {
            Gizmos.DrawLine(
                transform.TransformPoint(new Vector3(xMin, 0, z)),
                transform.TransformPoint(new Vector3(xMax, 0, z)));
        }

        Gizmos.color = Color.blue;
        foreach(Vector3 v in concave)
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
