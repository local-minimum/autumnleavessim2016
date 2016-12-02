using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class ProcGenHelpers
{

    static float rotationThreshold = 0.001f;
    static float proximitySqThreshold = 0.001f;

    public static float CrossXZ(Vector3 lhs, Vector3 rhs)
    {
        return lhs.x * rhs.z - lhs.z * rhs.x;
    }

    public static float Cross(Vector2 lhs, Vector2 rhs)
    {
        return lhs.x * rhs.y - lhs.y * rhs.x;
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

    public static int GetClosestSegment(List<Vector3> poly, Vector3 pt)
    {
        int closest = -1;
        float minDist = 0;

        for (int i = 0, l = poly.Count(); i < l; i++)
        {
            float dist = GetMinDist(pt, poly[i], poly[(i + 1) % l]);
            if (closest < 0 || dist < minDist)
            {
                closest = i;
                minDist = dist;
            }
        }
        return closest;
    }

    public static bool PointInsideSegmentXZ(Vector3 a, Vector3 b, Vector3 pt)
    {
        return Sign(CrossXZ(a - pt, b - pt)) == 0 && pt.x < Mathf.Max(a.x, b.x) && pt.x > Mathf.Min(a.x, b.x) && pt.z < Mathf.Max(a.z, b.z) && pt.z > Mathf.Min(a.z, b.z);
    }

    public static bool PointOnSegmentXZ(Vector3 a, Vector3 b, Vector3 pt)
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
                if (PointInsideSegmentXZ(q1, q2, p1) || PointInsideSegmentXZ(q1, q2, p2) || PointInsideSegmentXZ(p1, p2, q1) || PointInsideSegmentXZ(p1, p2, q2))
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
        bool any = false;
        float minT = 0;
        pt = Vector3.one;

        for (int i = 0, l = line.Count; i < l; i++)
        {
            Vector3 q1 = line[i];
            Vector3 q2 = line[(i + 1) % l];
            if (source == q1 || source == q2 || PointOnSegmentXZ(q1, q2, source))
            {
                //Not interested in source on line
                continue;
            }

            Vector3 v1 = source - q1;
            Vector3 v2 = q2 - q1;
            Vector3 v3 = Get90CCW(direction);

            float t1 = Mathf.Abs(CrossXZ(v2, v1)) / Vector3.Dot(v2, v3);
            float t2 = Vector3.Dot(v1, v3) / Vector3.Dot(v2, v3);
            if (t2 >= 0 && t2 <= 1 && t1 < 0)
            {
                if (!any || Mathf.Abs(t1) < minT)
                {
                    minT = Mathf.Abs(t1);
                    pt = Vector3.Lerp(q1, q2, t2);
                    any = true;
                }
            }
        }

        return any;
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

    public static bool IsKnownSegment(Vector3 a, Vector3 b, bool circular, List<Vector3> wall)
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

        if (end - start == 1 || circular && end - start == wall.Count - 1)
        {
            return true;
        } 

        int l = wall.Count;

        if (PointOnSegmentXZ(a, b, wall[(l + start - 1) % l])) {
            return true;
        } else if (PointOnSegmentXZ(a, b, wall[(start + 1) % l]))
        {
            return true;
        }
        else if (PointOnSegmentXZ(a, b, wall[(l + end - 1) % l]))
        {
            return true;
        }
        else if (PointOnSegmentXZ(a, b, wall[(end + 1) % l]))
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
            if (IsKnownSegment(a, b, false, walls[i]))
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

    public static bool LineSegmentInterceptPlane(Vector3 planePt1, Vector3 planePt2, Vector3 planePt3, Vector3 linePtA, Vector3 linePtB, out Vector3 intercept)
    {

        Vector3 direction = (linePtB - linePtA); //Have verified I don't need norming this
        
        Ray r = new Ray(linePtA, direction); //Origin, direction

        Vector3 normal = Vector3.Cross((planePt2 - planePt1), (planePt3 - planePt1)).normalized;
        Plane p = new Plane(-normal, planePt1); // in normal, in point
        float t = -1;
        bool hit = p.Raycast(r, out t); //Gives time on 
        
        if (hit && (t < 0 || t > direction.magnitude)) {
            //Debug.Log(t);
            //Debug.Log(direction.magnitude);
            hit = false;
        }

        if (hit)
        {
            intercept = r.GetPoint(t);            
        } else
        {
            intercept = Vector3.zero;
        }
        return hit;
    }

    static Vector2 PlanarPoint(Vector3 v, Vector3 x, Vector3 y)
    {
        return new Vector2(Vector3.Dot(v, x), Vector3.Dot(v, y));
    }

    public static bool PointInTriangle(Vector3 triPt1, Vector3 triPt2, Vector3 triPt3, Vector3 pt)
    {
        Vector3 x = triPt2 - triPt1;
        Vector3 y = triPt3 - triPt1;

        Vector3 planarPt = PlanarPoint(pt, x, y);
        if (planarPt.x < Mathf.Epsilon && planarPt.x >= 0 && planarPt.y >= 0 && planarPt.y <= y.magnitude || 
            planarPt.y < Mathf.Epsilon && planarPt.y >= 0 && planarPt.x >= 0 && planarPt.x <= x.magnitude)
        {
            return true;
        }

        Vector2 pt1 = PlanarPoint(triPt1 - pt, x, y);
        Vector2 pt2 = PlanarPoint(triPt2 - pt, x, y);
        Vector2 pt3 = PlanarPoint(triPt3 - pt, x, y);

        int s1 = Sign(Cross(pt1, pt2));
        int s2 = Sign(Cross(pt2, pt3));
        int s3 = Sign(Cross(pt3, pt1));
        //Debug.Log(string.Format("{0} {1} {2}", triPt1 - pt, triPt2 - pt, triPt3 - pt));
        //Debug.Log(string.Format("{0} {1} {2}", pt1, pt2, pt3));
        //Debug.Log(string.Format("{0} {1} {2}", s1, s2, s3));
        return s1 != 0 && s1 == s2 && s2 == s3;
    }

    public static bool PointInConvexPolygon(Vector3 pt, List<Vector3> orderedPolygon)
    {
        int l = orderedPolygon.Count;
        if (l < 3)
        {
            throw new System.ArgumentException("A polygon must be at least a triangle");
        }

        Vector3 norm = TriangleNormal(pt, orderedPolygon[0], orderedPolygon[1]);

        for (int i=1; i< l; i++)
        {
            if (Sign(Vector3.Dot(norm, TriangleNormal(pt, orderedPolygon[i], orderedPolygon[(i + 1) % l]))) != 1)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Gives the normal given the points are in CCW order.
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <returns></returns>
    public static Vector3 TriangleNormal(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return Vector3.Cross(p2 - p1, p3 - p1).normalized;
    }

    public static bool LineSegmentInterceptIn3D(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2, float proximityThreshold, out float timeA, out float timeB)
    {
        timeA = -1;
        timeB = -1;

        Vector3 directionA = a2 - a1;
        Vector3 directionB = b2 - b1;

        if (directionA.sqrMagnitude < Mathf.Epsilon)
        {
            return false;
        }

        if (directionB.sqrMagnitude < Mathf.Epsilon)
        {
            return false;
        }

        Vector3 aToB = a1 - b1;

        float f1343 = Vector3.Dot(aToB, directionB);
        float f4321 = Vector3.Dot(directionB, directionA);
        float f1321 = Vector3.Dot(aToB, directionA);
        float f4343 = Vector3.Dot(directionB, directionB);
        float f2121 = Vector3.Dot(directionA, directionA);

        float denom = f2121 * f4343 - f4321 * f4321;
        if (Mathf.Abs(denom) < Mathf.Epsilon)
        {
            return false;
        }

        float numer = f1343 * f4321 - f1321 * f4343;

        timeA = numer / denom;
        timeB = (f1343 + f4321 * timeA) / f4343;

        if (timeA < 0 || timeA > directionA.magnitude)
        {
            return false;
        } else if (timeB < 0 || timeB > directionB.magnitude)
        {
            return false;
        }

        return Vector3.Distance(a1 + directionA * timeA, b1 + directionB * timeB) < proximityThreshold;
    }

    public static bool ClosestPointsOnTwoLines(Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2, out float time1, out float time2)
    {

        time1 = -1;
        time2 = -1;

        float a = Vector3.Dot(lineVec1, lineVec1);
        float b = Vector3.Dot(lineVec1, lineVec2);
        float e = Vector3.Dot(lineVec2, lineVec2);

        float d = a * e - b * b;

        //lines are not parallel
        if (d != 0.0f)
        {

            Vector3 r = linePoint1 - linePoint2;
            float c = Vector3.Dot(lineVec1, r);
            float f = Vector3.Dot(lineVec2, r);

            time1 = (b * f - c * e) / d;
            time2 = (a * f - c * b) / d;

            return true;
        }

        else {
            return false;
        }
    }

    public static bool ClosestPointsOnTwoLines(Vector3 linePoint1, Vector3 lineVec1, Ray r, out float time1, out float time2)
    {
        return ClosestPointsOnTwoLines(linePoint1, lineVec1, r.origin, r.direction, out time1, out time2);
    }

    public static bool RayInterceptsLineSegment(Vector3 a, Vector3 b, Ray r, float proximity, out float rayTime, out float lineTime)
    {        
        Vector3 ba = (b - a).normalized;
        bool isntParalell = ClosestPointsOnTwoLines(a, ba, r.origin, r.direction, out lineTime, out rayTime);

        bool onLineSegment = lineTime > 0 && lineTime < (b - a).magnitude;
        float smallestDist = Vector3.Distance(a + ba * lineTime, r.GetPoint(rayTime));
        Debug.Log(string.Format("NonPara {0}, OnSegment {1} (t {2}), Ray Positive {3} ({4}), Dist {5} ({6})", isntParalell, onLineSegment, lineTime, rayTime > 0, rayTime, smallestDist, smallestDist < proximity));
        return isntParalell && onLineSegment && rayTime > 0 && smallestDist < proximity;
    }

    public static bool LineSegmentInterceptIn3D(Vector3 a1, Vector3 a2, Ray r, float proximityThreshold, out float timeA, out float timeB)
    {
        timeA = -1;
        timeB = -1;

        Vector3 directionA = a2 - a1;
        Vector3 directionB = r.direction;

        if (directionA.sqrMagnitude < Mathf.Epsilon)
        {
            Debug.LogWarning("Triangle side too short");
            return false;
        }

        if (directionB.sqrMagnitude < Mathf.Epsilon)
        {
            Debug.LogWarning("No direction on ray");
            return false;
        }

        Vector3 aToB = a1 - r.origin;

        float f1343 = Vector3.Dot(aToB, directionB);
        float f4321 = Vector3.Dot(directionB, directionA);
        float f1321 = Vector3.Dot(aToB, directionA);
        float f4343 = Vector3.Dot(directionB, directionB);
        float f2121 = Vector3.Dot(directionA, directionA);

        float denom = f2121 * f4343 - f4321 * f4321;
        if (Mathf.Abs(denom) < Mathf.Epsilon)
        {
            Debug.LogWarning("Denominator too small");
            return false;
        }

        float numer = f1343 * f4321 - f1321 * f4343;

        timeA = numer / denom;
        timeB = (f1343 + f4321 * timeA) / f4343;

        if (timeA < 0 || timeA > directionA.magnitude)
        {
            Debug.LogWarning(string.Format("Time outside triangle side {0} compared to {1}", timeA, directionA.magnitude));
            return false;
        }
        else if (timeB < 0)
        {
            Debug.LogWarning("Negative ray time " + timeB);
            return false;
        }
        Debug.Log("Valid ray time " + timeB);
        return Vector3.Distance(a1 + directionA * timeA, r.GetPoint(timeB)) < proximityThreshold;
    }

    public static int Rotatition(Vector3 normal, Vector3 a, Vector3 b)
    {
        return Sign(Quaternion.Dot(Quaternion.LookRotation(a, normal), Quaternion.LookRotation(b, normal)));
    }

    public static bool InterceptionRay(Vector3 normA, Vector3 directionA, Vector3 intercept, Vector3 normB, out Ray r)
    {
        //Parallell stuff!
        Vector3 u = Vector3.Cross(normB, normA);
        if (u.sqrMagnitude < Mathf.Epsilon)
        {
            r = new Ray();
            return false;
        }
        u = u.normalized;
        int sign = Rotatition(normA, directionA, u);
        r = new Ray(intercept, -sign * u);
        return true;
    }

    public static bool InterceptionRay(Vector3 normA, Vector3 intercept, Vector3 normB, out Ray r)
    {
        //Parallell stuff!
        Vector3 u = Vector3.Cross(normB, normA);
        if (u.sqrMagnitude < Mathf.Epsilon)
        {
            r = new Ray();
            return false;
        }
        r = new Ray(intercept, u.normalized);
        return true;
    }

    public static Vector3 RayHitEdge(Vector3 a, Vector3 b, Vector3 c, Ray r, out int edge, float proximity = 0.1f)
    {
        float t1;
        float t2;
        List<KeyValuePair<int, float>> interceptTimes = new List<KeyValuePair<int, float>>();

        if (LineSegmentInterceptIn3D(a, b, r, proximity, out t1, out t2))
        {
            interceptTimes.Add(new KeyValuePair<int, float>(0, t2));

        }

        if (LineSegmentInterceptIn3D(b, c, r, proximity, out t1, out t2))
        {
            interceptTimes.Add(new KeyValuePair<int, float>(1, t2));
        }

        if (LineSegmentInterceptIn3D(c, a, r, proximity, out t1, out t2))
        {
            interceptTimes.Add(new KeyValuePair<int, float>(2, t2));
        }
    
        if (interceptTimes.Count() > 0)
        {
            KeyValuePair<int, float> furthestIntercept = interceptTimes.OrderByDescending(kvp => kvp.Value).First();
            edge = furthestIntercept.Key;
            return r.GetPoint(furthestIntercept.Value);
        }

        edge = -1;
        return r.origin;
    }

    static bool IsConvex(Vector3 cur, Vector3 next, Vector3 prev, Vector3 norm)
    {
        return Sign(Vector3.Dot(Vector3.Cross(next - cur, prev - cur), norm)) == 1;
    }

    public static List<int> PolyToTriangles(List<Vector3> poly, Vector3 norm, int start)
    {
        List<int> tris = new List<int>();

        int cur = 0;
        int l = poly.Count();
        
        bool[] completed = new bool[l];
        int prePrev = l - 2;
        int prev = l - 1;
        int next = (cur + 1) % l;
        while (cur < l && prev != next)
        {
            if (cur != prev  && !IsConvex(poly[cur], poly[next], poly[prev], norm))
            {
                tris.Add(prev + start);
                tris.Add(cur + start);
                tris.Add(prePrev + start);
                //Debug.Log(string.Format("{0} {1} _{2}_ {3}", prePrev, prev, cur, next));
                completed[prev] = true;
                if (!completed.Contains(false))
                {
                    break;
                }                
                prev = prePrev;
                prePrev = (prePrev - 1 + l) % l;

            } else
            {
                prePrev = prev;
                prev = cur;
                cur++;
                if (cur == l)
                {
                    break;
                }                
                next = (cur + 1) % l;
            }
        }
        //Debug.Log(string.Join(", ", completed.Select(e => e ? "T" : "F").ToArray()));

        List<int> remaining = completed.Select((e, idx) => new { completed = e, index = idx }).Where(e => !e.completed).Select(e => e.index).ToList();

        while (remaining.Count() > 2)
        {
            tris.Add(remaining[0] + start);
            tris.Add(remaining[1] + start);
            tris.Add(remaining[2] + start);
            remaining.RemoveAt(1);
        }

        return tris;
    }

    public static IEnumerable<Vector2> GetProjectedUVs(Vector3[] triCorners, Vector2[] triUVs, List<Vector3> points)
    {
        Vector3 x = triCorners[1] - triCorners[0];
        Vector3 y = triCorners[2] - triCorners[0];
        Vector2 uvX = triUVs[1] - triUVs[0];
        Vector2 uvY = triUVs[2] - triUVs[0];

        for (int i=0, l=points.Count(); i< l; i++) {
            Vector2 triPt = PlanarPoint(points[i] - triCorners[0], x, y);
            yield return triUVs[0] + uvX * triPt.x + uvY * triPt.y;
        }
    }
}
