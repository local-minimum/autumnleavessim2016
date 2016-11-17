using UnityEngine;
using System.Collections.Generic;

public class ProcMathLine : MonoBehaviour {

    [SerializeField]
    float gizmoSize = 0.1f;

    [SerializeField]
    bool showing;

    public Color lineColor = Color.magenta;

    public int markIndex;

    public IEnumerable<Vector3> Line
    {
        get
        {
            int n = transform.childCount;

            for (int i = 0; i < n; i++)
            {
                yield return transform.GetChild(i).position;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (showing)
        {
            int n = transform.childCount;

            for (int i = 0; i < n; i++)
            {
                Vector3 pos = transform.GetChild(i).position;
                Vector3 nextPos = transform.GetChild((i + 1) % n).position;

                Gizmos.color = lineColor;
                Gizmos.DrawLine(pos, nextPos);

                if (i == 0)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(pos, gizmoSize);
                }
                else if (i == n - 1)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(pos, Vector3.one * gizmoSize * 2f);
                }

                if (i == markIndex)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(pos, gizmoSize * 0.5f);
                }
            }
        }
    }
}
