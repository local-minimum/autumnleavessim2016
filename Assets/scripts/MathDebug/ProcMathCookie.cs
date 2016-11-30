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

    List<List<Vector3>> subPaths = new List<List<Vector3>>();

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

            }

            Dictionary<int, List<Vector3>> interceptTris = cutter.GetInterceptTris(allIntercepts.ToArray());
            //Debug.Log("TRACING " + allIntercepts.Count());
            int curSubPath = 0;
            subPaths.Clear();

            if (allIntercepts.Count() == 1)
            {
                Debug.Log("Not safe distance to original tri vert to make cut");
            }
            else {
                List<int> corners = new List<int>() {0, 1, 2};
                List<int> cornersInCurSubPath = new List<int>();
                int curCorner = 0;
                bool keepTrying = true;
                while (corners.Count() > 0 || keepTrying)
                {
                    bool walkingSubPath = false;
                    //Debug.Log(string.Format("Current sub path {0}, {1} known", curSubPath, subPaths.Count()));
                    if (cutter.PointInMesh(l[curCorner]) || curSubPath >= subPaths.Count())
                    {
                        if (corners.Count() > 0)
                        {
                            //Debug.Log(corners.Count() + " corners remaining");
                            if (subPaths.Count() == 0 || subPaths[subPaths.Count() - 1].Count() > 0)
                            {
                                subPaths.Add(new List<Vector3>());
                            }
                            curCorner = corners[0];
                            corners.RemoveAt(0);
                            cornersInCurSubPath.Clear();

                        } else
                        {
                            //Debug.Log("Out of corners");
                            keepTrying = false;
                        }
                        continue;
                    }

                    //Debug.Log(string.Format("Adding corner {0} to path {1}", curCorner, curSubPath));
                    cornersInCurSubPath.Add(curCorner);
                    subPaths[curSubPath].Add(l[curCorner]);
                    
                    int j = cuts[curCorner].Count() > 0 ? 0 : -1; // Mathf.Min(0, cuts[curCorner].Count() - 1);
                    if (j == 0)
                    {
                        Vector3 intercept = cuts[curCorner][j];
                        List<int> tris = interceptTris.Where(kvp => kvp.Value.Contains(intercept)).Select(kvp => kvp.Key).ToList();
                        if (tris.Count() == 1)
                        {
                            int tri = tris[0];
                            Vector3 thirdVert = l[(curCorner + 2) % 3];
                            List<Vector3> cutLine = cutter.TraceSurface(tri, thirdVert, intercept, n, interceptTris);
                            if (cutLine.Count() > 0)
                            {
                                if (cutLine.Where((v, idV) => idV < cutLine.Count() - 1).All(v => ProcGenHelpers.PointInTriangle(l[0], l[1], l[2], v)))
                                {                                    
                                    cutLine.Insert(0, intercept);

                                    subPaths[curSubPath].AddRange(cutLine);

                                    walkingSubPath = true;
                                    
                                    Gizmos.color = Color.red;
                                    Gizmos.DrawSphere(cutLine.First(), gizmoSize);
                                    Gizmos.color = Color.blue;
                                    Gizmos.DrawSphere(cutLine.Last(), gizmoSize);


                                    int nextEdge = ProcGenHelpers.GetClosestSegment(l, cutLine.Last());
                                    if (nextEdge < 0)
                                    {
                                        walkingSubPath = false;
                                        Debug.LogError("Lost track of where we are, now everything is possible. Unfortunately.");
                                    }
                                    else {
                                        nextEdge = (nextEdge + 1) % l.Length;
                                        //Debug.Log(nextEdge);
                                        if (cornersInCurSubPath.Contains(nextEdge))
                                        {
                                            //Debug.Log(string.Format("Closing {0} SubPath with corner {1}", curSubPath, curCorner));
                                            curSubPath++;
                                        }
                                        else
                                        {
                                            
                                            if (!corners.Remove(nextEdge))
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
                                } else {
                                    Debug.LogWarning("Cutting the triangle outside triangle is bad idea.");
                                }
                                
                            } else
                            {
                                Debug.LogWarning("Empty Cut");
                            }
                        } else {
                            Debug.LogWarning("Intercept is duplicated");
                        }                        
                    }

                    if (!walkingSubPath)
                    {
                        if (corners.Count() > 0)
                        {
                            curCorner = corners[0];
                            corners.RemoveAt(0);
                        } else
                        {
                            keepTrying = false;
                        }
                    }
                }
            }

            //Debug.Log(subPaths.Count());
            Gizmos.color = Color.red;
            for (int subP = 0; subP < subPaths.Count(); subP++)
            {

                for (int i=0, nSub=subPaths[subP].Count(); i< nSub; i++) 
                {
                    if (i < 3)
                    {                        
                        Gizmos.color = new Color[] { Color.red, Color.green, Color.blue }[i];
                        Gizmos.DrawWireCube(subPaths[subP][i], Vector3.one * gizmoSize);
                    }
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(subPaths[subP][i], subPaths[subP][(i + 1) % nSub]);
                }
            }

        }
    }
}
