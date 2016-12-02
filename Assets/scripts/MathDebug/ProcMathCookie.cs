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
        
        Gizmos.color = Color.red;
        if (drawing)
        {

            List<Vector3> allIntercepts = new List<Vector3>();

            //TODO: Much should be in cutter in future
            cutter.RecalculateMeshlike();

            Vector3[] l = line.Line.ToArray();
            Vector3 normal = Vector3.Cross(l[1] - l[0], l[2] - l[0]).normalized;
            int[] outTriangle = new int[3] { 0, 1, 2 };

            Dictionary<int, List<Vector3>> cuts = cutter.GetCuts(l, outTriangle, 0, normal);
            foreach(List<Vector3> cutz in cuts.Values)
            {
                allIntercepts.AddRange(cutz);
            }

            foreach(Vector3 intercept in allIntercepts)
            {
                Gizmos.DrawSphere(intercept, gizmoSize);
            }
            subPaths = cutter.GetSubPaths(l, outTriangle, 0, normal, cuts, allIntercepts);

            //Debug.Log(subPaths.Count() + " paths");
            Gizmos.color = Color.red;
            for (int subP = 0; subP < subPaths.Count(); subP++)
            {
                List<int> tris = ProcGenHelpers.PolyToTriangles(subPaths[subP], normal, 0);
                //Debug.Log(tris.Count() + " triangle points");
                for (int idT=0, lT = tris.Count(); idT< lT; idT+=3)
                {
                    Gizmos.DrawLine(subPaths[subP][tris[idT]], subPaths[subP][tris[idT + 1]]);
                    Gizmos.DrawLine(subPaths[subP][tris[idT + 1]], subPaths[subP][tris[idT + 2]]);
                    Gizmos.DrawLine(subPaths[subP][tris[idT + 2]], subPaths[subP][tris[idT]]);
                    Vector3 center = (subPaths[subP][tris[idT]] + subPaths[subP][tris[idT + 1]] + subPaths[subP][tris[idT + 2]]) / 3;
                    Gizmos.DrawWireCube(center, gizmoSize * Vector3.one);
                    Gizmos.DrawLine(center, center + Vector3.Cross(subPaths[subP][tris[idT + 1]] - subPaths[subP][tris[idT]], subPaths[subP][tris[idT + 2]] - subPaths[subP][tris[idT]]));
                }

            }

        }
    }
}
