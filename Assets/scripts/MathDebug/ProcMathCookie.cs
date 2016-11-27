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

            //TODO: Should be in cutter in future
            cutter.RecalculateMeshlike();

            Vector3[] l = line.Line.ToArray();
            Vector3 n = Vector3.Cross(l[1] - l[0], l[2] - l[0]).normalized;
            for (int i=0, len = l.Length; i<len; i++)
            {
                List<Vector3> cuts = cutter.CutsLineAt(l[i], l[(i + 1) % len], n);
                for (int j=0, k=cuts.Count; j< k; j++)
                {
                    Gizmos.DrawSphere(cuts[j], gizmoSize);
                    if (j > 0 && j % 2 == 1)
                    {
                        Gizmos.DrawLine(cuts[j - 1], cuts[j]);
                    }
                }
                
            }
            
        }
    }
}
