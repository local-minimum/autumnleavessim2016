using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ProcMathTest : MonoBehaviour {

    [SerializeField]
    ProcMathLine reference;

    [SerializeField]
    ProcMathLine test;

    [SerializeField]
    float gizmosSize = 0.1f;

    public enum MathTest { Collides, Ray, IsKnown};

    public MathTest mathTest;

    void OnDrawGizmos()
    {
        bool success = false;
        List<Vector3> testLine = test.Line.ToList();

        if (mathTest == MathTest.Collides)
        {

            success = ProcGenHelpers.CollidesWith(testLine[0], testLine[1], reference.Line.ToList(), true, out reference.markIndex);

        } else if (mathTest == MathTest.Ray)
        {
            reference.markIndex = -1;
            Vector3 pt = Vector3.one;
            success = ProcGenHelpers.RayInterceptsSegment(testLine[0], testLine[1] - testLine[0], reference.Line.ToList(), out pt);
            if (success)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(pt, gizmosSize);
            }
        } else if (mathTest == MathTest.IsKnown)
        {
            success = ProcGenHelpers.IsKnownSegment(testLine[0], testLine[1], true, reference.Line.ToList());            
        }


        test.lineColor = success ? Color.green : Color.gray;
    }

}

