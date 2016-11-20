using UnityEngine;
using System.Collections;
using System.Linq;

public class ProcMathPtInTri : MonoBehaviour {

    [SerializeField]
    ProcMathLine line;

    [SerializeField]
    Transform point;

    [SerializeField]
    float gizmoSize = 2f;

    [SerializeField]
    bool showing = false;

    void OnDrawGizmos()
    {
        if (!showing)
        {
            return;
        }

        Vector3[] points = line.Line.ToArray();
        if (points.Length == 3)
        {

            bool isIn = ProcGenHelpers.PointInTriangle(points[0], points[1], points[2], point.position);

            Gizmos.color = Color.red;

            if (isIn)
            {
                Gizmos.DrawCube(point.position, Vector3.one * gizmoSize);
            } else
            {
                Gizmos.DrawSphere(point.position, gizmoSize / 2f);
            }
        }
    }
}
