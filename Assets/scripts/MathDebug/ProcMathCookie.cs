using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ProcMathCookie : MonoBehaviour {

    [SerializeField]
    ProcMathLine line;

    [SerializeField]
    CookieCutter cutter;

    [SerializeField]
    bool drawing = false;

    [SerializeField]
    float gizmoSize = 0.3f;

    void OnDrawGizmos()
    {
        float thresholdPointToCutProximity = .001f;
        Gizmos.color = Color.red;
        if (drawing)
        {

            Dictionary<int, List<Vector3>> cuts = new Dictionary<int, List<Vector3>>();
            List<Vector3> allIntercepts = new List<Vector3>();

            //TODO: Much should be in cutter in future
            cutter.RecalculateMeshlike();

            Vector3[] l = line.Line.ToArray();
            Vector3 n = Vector3.Cross(l[1] - l[0], l[2] - l[0]).normalized;
            for (int i = 0, len = l.Length; i < len; i++)
            {

                Vector3 nextV = l[(i + 1) % len];

                cuts[i] = cutter
                    .GetLineCutIntercepts(l[i], nextV, n)
                    .Where(v => Vector3.SqrMagnitude(v - l[i]) > thresholdPointToCutProximity && Vector3.SqrMagnitude(v - nextV) > thresholdPointToCutProximity)                  
                    .Select(v => new { vert = v, dist = Vector3.SqrMagnitude(v - l[i]) })
                    .OrderBy(e => e.dist)
                    .Select(e => e.vert)
                    .ToList();

                allIntercepts.AddRange(cuts[i]);

                for (int j = 0, k = cuts[i].Count(); j < k; j++)
                {
                    Gizmos.DrawSphere(cuts[i][j], gizmoSize);

                }

            }

            Dictionary<int, List<Vector3>> interceptTris = cutter.GetInterceptTris(allIntercepts.ToArray());
            //Debug.Log("TRACING " + allIntercepts.Count());

            if (allIntercepts.Count() == 1)
            {
                Debug.Log("Not safe distance to original tri vert to make cut");
            }
            else {
                for (int i = 0, len = l.Length; i < len; i++)
                {
                    if (cutter.PointInMesh(l[i]))
                    {
                        continue;
                    }

                    int j = Mathf.Min(0, cuts[i].Count() - 1);
                    if (j == 0)
                    {
                        Vector3 intercept = cuts[i][j];
                        List<int> tris = interceptTris.Where(kvp => kvp.Value.Contains(intercept)).Select(kvp => kvp.Key).ToList();
                        if (tris.Count() == 1)
                        {
                            int tri = tris[0];
                            Vector3 thirdVert = l[(i + 2) % 3];
                            List<Vector3> cutLine = cutter.TraceSurface(tri, thirdVert, intercept, n, interceptTris);
                            if (cutLine.Count() > 0)
                            {
                                if (cutLine.Where((v, idV) => idV < cutLine.Count() - 1).All(v => ProcGenHelpers.PointInTriangle(l[0], l[1], l[2], v)))
                                {                                    
                                    cutLine.Insert(0, intercept);
                                    for (int k = 0, kLen = cutLine.Count() - 1; k < kLen; k++)
                                    {
                                        Gizmos.DrawLine(cutLine[k], cutLine[k + 1]);
                                    }
                                }
                                

                            }
                        }                        
                    }
                }
            }
        }
    }
}
