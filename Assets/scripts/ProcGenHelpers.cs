using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class ProcGenHelpers
{

    static float rotationThreshold = 0.0001f;
    static float proximitySqThreshold = 0.00001f;

    public static float CrossXZ(Vector3 lhs, Vector3 rhs)
    {
        return lhs.x * rhs.z - lhs.z * rhs.x;
    }


    public static int Sign(float v)
    {
        if (v < -rotationThreshold)
        {
            return -1;
        }
        else if (v > rotationThreshold)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public static int XZRotation(Vector3 lhs, Vector3 rhs)
    {
        return Sign(CrossXZ(lhs.normalized, rhs.normalized));
    }

    public static int XZRotation(Vector3 a, Vector3 b, Vector3 c)
    {
        return XZRotation(a - b, b - c);
    }

    public static float GetMinDist(Vector3 pt, Vector3 a, Vector3 b)
    {
        float l = (a - b).sqrMagnitude;
        if (l == 0f)
        {
            return Vector3.Distance(pt, a);
        }

        float t = Mathf.Clamp01(Vector3.Dot(pt - a, b - a) / l);
        Vector3 projection = a + t * (b - a);
        return Vector3.Distance(pt, projection);
    }

    public static bool PointInsideSegment(Vector3 a, Vector3 b, Vector3 pt)
    {
        return Sign(CrossXZ(a - pt, b - pt)) == 0 && pt.x < Mathf.Max(a.x, b.x) && pt.x > Mathf.Min(a.x, b.x) && pt.z < Mathf.Max(a.z, b.z) && pt.z > Mathf.Min(a.z, b.z);
    }

    public static bool PointOnSegment(Vector3 a, Vector3 b, Vector3 pt)
    {
        return Sign(CrossXZ(a - pt, b - pt)) == 0 &&
            pt.x <= (Mathf.Max(a.x, b.x) + proximitySqThreshold) &&
            pt.x >= (Mathf.Min(a.x, b.x) - proximitySqThreshold) &&
            pt.z <= (Mathf.Max(a.z, b.z) + proximitySqThreshold) &&
            pt.z >= (Mathf.Min(a.z, b.z) - proximitySqThreshold);

    }

    static float DurationOnSegment(Vector3 a, Vector3 b, Vector3 pt1, Vector3 pt2)
    {
        Vector3 v1pt1 = pt1 - a;
        Vector3 v1pt2 = pt2 - a;
        Vector3 v2 = b - a;
        Vector3 v3 = (pt2 - pt1);

        float t1 = Vector3.Dot(v1pt1, v3) / Vector3.Dot(v2, v3);
        float t2 = Vector3.Dot(v1pt2, v3) / Vector3.Dot(v2, v3);
        Debug.Log(string.Format("times: {0} {1}", t1, t2));
        return 0;
    }

    public static bool CollidesWith(Vector3 p1, Vector3 p2, List<Vector3> referencePath, bool circular, out int index)
    {
        int l = referencePath.Count;
        int end = l - (circular ? 0 : 1);
        for (int i = 0; i < end; i++)
        {
            Vector3 q1 = referencePath[i];
            Vector3 q2 = referencePath[(i + 1) % l];

            float aP1P2Q1 = Sign(CrossXZ(p1 - p2, q1 - p2));
            float aP1P2Q2 = Sign(CrossXZ(p1 - p2, q2 - p2));

            float aQ1Q2P1 = Sign(CrossXZ(q1 - q2, p1 - q2));
            float aQ1Q2P2 = Sign(CrossXZ(q1 - q2, p2 - q2));

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
                if (PointInsideSegment(q1, q2, p1) || PointInsideSegment(q1, q2, p2) || PointInsideSegment(p1, p2, q1) || PointInsideSegment(p1, p2, q2))
                {
                    //Debug.Log(string.Format("Linear intercept {0} ({1} {2} {3}), {4} ({5} {6} {7})", PointInsideSegment(q1, q2, p1), q1, q2, p1, PointInsideSegment(q1, q2, p2), q1, q2, p2));
                    index = i;
                    return true;
                }
                else if (p1 == q1 && p2 == q2 || p1 == q2 && p2 == q1)
                {
                    //Debug.Log(string.Format("Identical lines"));
                    index = i;
                    return true;

                }
            }

        }

        index = -1;
        return false;
    }

    public static bool CollidesWith(List<Vector3> testPath, List<Vector3> referencePath, bool circular, out int testIndex, out int refIndex)
    {
        for (int i = 0, l = testPath.Count - 1; i < l; i++)
        {
            if (CollidesWith(testPath[i], testPath[i + 1], referencePath, circular, out refIndex))
            {
                testIndex = i;
                return true;
            }
        }
        testIndex = -1;
        refIndex = -1;
        return false;
    }

    public static bool CollidesWith(List<Vector3> testPath, List<List<Vector3>> referencePaths, out int testIndex, out int refIndex, out int pathsIndex)
    {
        for (int i = 0, l = referencePaths.Count; i < l; i++)
        {
            if (CollidesWith(testPath, referencePaths[i], false, out testIndex, out refIndex))
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

    public static bool RayInterceptsSegment(Vector3 source, Vector3 direction, List<Vector3> line, out Vector3 pt)
    {

        for (int i = 0, l = line.Count; i < l; i++)
        {
            Vector3 q1 = line[i];
            Vector3 q2 = line[(i + 1) % l];
            if (source == q1 || source == q2 || PointOnSegment(q1, q2, source))
            {
                //Not interested in source on line
                continue;
            }

            Vector3 v1 = source - q1;
            Vector3 v2 = q2 - q1;
            Vector3 v3 = Get90CCW(direction);

            float t1 = Mathf.Abs(CrossXZ(v2, v1)) / Vector3.Dot(v2, v3);
            float t2 = Vector3.Dot(v1, v3) / Vector3.Dot(v2, v3);
            if (t2 >= 0 && t2 <= 1)
            {
                pt = Vector3.Lerp(q1, q2, t2);
                return true;
            }
        }

        pt = Vector3.zero;
        return false;
    }

    public static bool RayInterceptsSegment(Vector3 source, Vector3 direction, List<List<Vector3>> lines, out Vector3 pt, out int idLine)
    {
        float smallest = 0;
        bool any = false;
        pt = Vector3.one;
        idLine = -1;

        Vector3 pt2;

        for (int i = 0, l = lines.Count; i < l; i++)
        {

            if (RayInterceptsSegment(source, direction, lines[i], out pt2))
            {
                if (!any || smallest > (pt2 - source).sqrMagnitude)
                {
                    pt = pt2;
                    idLine = i;
                    smallest = (pt - source).sqrMagnitude;
                    any = true;
                }

            }
        }
        return any;
    }

    public static Vector3 Get90CW(Vector3 v)
    {
        return new Vector3(v.z, 0, -v.x);
    }

    public static Vector3 Get90CCW(Vector3 v)
    {
        return new Vector3(-v.z, 0, v.x);
    }

    public static bool TooClose(Vector3 pt, List<Vector3> wall, float proximitySq)
    {
        return wall.Any(p => { float d = Vector3.SqrMagnitude(pt - p); return d < proximitySq && d > proximitySqThreshold; });
    }

    public static bool IsKnownSegment(Vector3 a, Vector3 b, List<Vector3> wall)
    {
        int posA = wall.Select(e => Vector3.SqrMagnitude(e - a) < proximitySqThreshold).ToList().IndexOf(true);
        if (posA < 0)
        {
            //Debug.Log(string.Format("A {0} not on wall {1}", a, string.Join(", ", wall.Select(e => Vector3.SqrMagnitude(e - a).ToString()).ToArray())));
            return false;
        }

        int posB = wall.Select(e => Vector3.SqrMagnitude(e - b) < proximitySqThreshold).ToList().IndexOf(true);
        if (posB < 0)
        {
            //Debug.Log(string.Format("B {0} not on wall {1}", b, string.Join(", ", wall.Select(e => Vector3.SqrMagnitude(e - b).ToString()).ToArray())));
            return false;
        }

        int start = Mathf.Min(posA, posB);
        int end = Mathf.Max(posA, posB);

        if (Mathf.Abs(start - end) == 1)
        {
            return true;
        } else
        {
            //Debug.Log(Mathf.Abs(start - end));
        }

        int l = wall.Count;

        if (PointOnSegment(a, b, wall[(l + start - 1) % l])) {
            return true;
        } else if (PointOnSegment(a, b, wall[(start + 1) % l]))
        {
            return true;
        }
        else if (PointOnSegment(a, b, wall[(l + end - 1) % l]))
        {
            return true;
        }
        else if (PointOnSegment(a, b, wall[(end + 1) % l]))
        {
            return true;
        }

        return false;
    }

    public static bool IsKnownSegment(Vector3 a, Vector3 b, List<List<Vector3>> walls)
    {
        //Debug.Log(string.Format("Testing {0}, l {1}", walls, walls.Count));
        for(int i=0, l=walls.Count; i< l; i++)
        {
            if (IsKnownSegment(a, b, walls[i]))
            {
                return true;
            }
        }
        //Debug.Log("Not known");
        return false;
    }

    public static IEnumerable<Vector3> Simplify(List<Vector3> walls)
    {
        for (int i=0, l=walls.Count; i<l; i++)
        {
            int prev = (i - 1 + l) % l;
            int next = (i + 1) % l;

            if (XZRotation(walls[i], walls[prev], walls[next]) != 0)
            {
                yield return walls[i];
            }
        }
    }
}
