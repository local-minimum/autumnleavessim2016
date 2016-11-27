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

                cuts[i] = cutter
                    .GetLineCutIntercepts(l[i], l[(i + 1) % len], n)
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

            for (int i = 0, len = l.Length; i < len; i++)
            {
                for (int j = 0; j< cuts[i].Count(); j++)                
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
                            cutLine.Insert(0, intercept);
                            for (int k = 0, kLen = cutLine.Count() - 1; k < kLen; k++)
                            {
                                Gizmos.DrawLine(cutLine[k], cutLine[k + 1]);
                            }
                            Vector3 last = cutLine.Last();

                            //REMOVE INTERCEPTS THAT WE HAVE USED

                            interceptTris[tri].Remove(intercept);


                            for (int ii = 0; ii < cuts.Count(); ii++)
                            {
                                if (cuts[ii].Contains(last))
                                {
                                    cuts[ii].Remove(last);
                                    break;
                                }

                            }

                            foreach (int key in interceptTris.Keys)
                            {
                                if (interceptTris[key].Contains(last))
                                {
                                    interceptTris[key].Remove(last);
                                    break;
                                }
                            }

                        }
                    }
                }
            }
        }

    }
}
