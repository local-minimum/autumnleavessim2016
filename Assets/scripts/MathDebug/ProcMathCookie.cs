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
            Vector3[] l = line.Line.ToArray();

            for (int i=0, len = l.Length; i<len; i++)
            {
                List<Vector3> cuts = cutter.CutsLineAt(l[i], l[(i + 1) % len]);
                for (int j=0, k=cuts.Count; j< k; j++)
                {
                    Gizmos.DrawSphere(cuts[j], gizmoSize);
                }
                break;
            }
        }
    }
}
