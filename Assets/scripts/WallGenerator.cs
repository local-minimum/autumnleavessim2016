using UnityEngine;
using System.Collections;

public class WallGenerator : MonoBehaviour
{

    [SerializeField]
    FloorGenerator floor;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;

        Vector3 prev = Vector3.zero;
        Vector3 firstV = Vector3.zero;
        bool first = true;      
        foreach(Vector3 cur in floor.GetCircumferance(false))
        {
            if (first)
            {
                firstV = cur;
                first = false;  
            } else
            {
                Gizmos.DrawLine(prev, cur);
            }
            prev = cur;
        }

        if (!first)
        {
            Gizmos.DrawLine(prev, firstV);
        }
    }
}