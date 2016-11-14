using UnityEngine;
using System.Collections.Generic;

public static class ProcGenHelpers {

    static float rotationThreshold = 0.0001f;

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
        return Sign(CrossXZ(lhs, rhs));
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
        return Sign(CrossXZ(a - pt, b - pt)) == 0 && pt.x <= Mathf.Max(a.x, b.x) && pt.x >= Mathf.Min(a.x, b.x) && pt.z <= Mathf.Max(a.z, b.z) && pt.z >= Mathf.Min(a.z, b.z);

    }

    public static bool CollidesWith(Vector3 p1, Vector3 p2, List<Vector3> referencePath, out int index)
    {
        for (int i = 0, l = referencePath.Count - 1; i < l; i++)
        {
            Vector3 q1 = referencePath[i];
            Vector3 q2 = referencePath[i + 1];

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

    public static bool CollidesWith(List<Vector3> testPath, List<Vector3> referencePath, out int testIndex, out int refIndex)
    {
        for (int i = 0, l = testPath.Count - 1; i < l; i++)
        {
            if (CollidesWith(testPath[i], testPath[i + 1], referencePath, out refIndex))
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

    public static bool RayInterceptsSegment(Vector3 source, Vector3 direction, List<Vector3> line, out Vector3 pt)
    {

        for (int i = 0, l = line.Count; i < l; i++)
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
            Vector3 v3 = Get90CCW(direction);

            float t1 = Mathf.Abs(CrossXZ(v2, v1)) / Vector3.Dot(v2, v3);
            float t2 = Vector3.Dot(v1, v3) / Vector3.Dot(v2, v3);
            if (t2 >= 0 && t2 <= 1)
            {
                if (t1 < 0)
                {

                    pt = Vector3.Lerp(q1, q2, t2);
                    return true;
                }
            }
        }

        pt = Vector3.zero;
        return false;
    }

    public static bool RayInterceptsSegment(Vector3 source, Vector3 direction, List<List<Vector3>> lines, out Vector3 pt)
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

    public static Vector3 Get90CW(Vector3 v)
    {
        return new Vector3(v.z, 0, -v.x);
    }

    public static Vector3 Get90CCW(Vector3 v)
    {
        return new Vector3(-v.z, 0, v.x);
    }
}
