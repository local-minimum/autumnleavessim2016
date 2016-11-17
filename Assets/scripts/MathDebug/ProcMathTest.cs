using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ProcMathTest : MonoBehaviour {

    [SerializeField]
    ProcMathLine reference;

    [SerializeField]
    ProcMathLine test;

    public enum MathTest { Collides, Ray};

    public MathTest mathTest;

    void OnDrawGizmos()
    {
        bool success = false;
        if (mathTest == MathTest.Collides)
        {
            List<Vector3> testLine = test.Line.ToList();

            success = ProcGenHelpers.CollidesWith(testLine[0], testLine[1], reference.Line.ToList(), true, out reference.markIndex);

        }
    }

}

