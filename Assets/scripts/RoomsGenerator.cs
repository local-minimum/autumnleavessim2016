using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomsGenerator : MonoBehaviour {

    [SerializeField]
    FloorGenerator floor;

    [SerializeField]
    WallGenerator walls;

    [SerializeField]
    float gridSize = 1.0f;

    List<float> X = new List<float>();
    List<float> Z = new List<float>();

    bool generated = false;

    void Start () {
	
	}

    void Update () {
	    if (!generated && floor.Generated)
        {
            MakeGrid();
            generated = true;
        } else if (generated && !floor.Generated)
        {
            X.Clear();
            Z.Clear();
            generated = false;
        }
	}

    void MakeGrid()
    {

        List<Vector3> points = floor.GetCircumferance(false).Select(v => transform.InverseTransformPoint(v)).ToList();
        if (points.Count == 0)
        {
            return;
        }


        for (int i = 0; i < points.Count; i++)
        {
            if (!X.Contains(points[i].x))
            {
                X.Add(points[i].x);
            }

            if (!Z.Contains(points[i].z))
            {
                Z.Add(points[i].z);
            }
        }

        Z.Sort();
        X.Sort();
        for (int i = 1, l = Z.Count; i < l; i++)
        {
            if (Z[i] - Z[i - 1] > gridSize)
            {
                int n = Mathf.FloorToInt((Z[i] - Z[i - 1]) / gridSize) + 1;
                for (int j = 1; j < n; j++)
                {
                    Z.Add(Mathf.Lerp(Z[i - 1], Z[i], ((float)j) / n));
                }
            }
        }

        for (int i = 1, l = X.Count; i < l; i++)
        {
            if (X[i] - X[i - 1] > gridSize)
            {
                int n = Mathf.FloorToInt((X[i] - X[i - 1]) / gridSize) + 1;
                for (int j = 1; j < n; j++)
                {
                    X.Add(Mathf.Lerp(X[i - 1], X[i], ((float)j) / n));
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;

        if (Z.Count == 0 || X.Count == 0)
        {
            return;
        }

        float zMin = Z.Min();
        float zMax = Z.Max();
        float xMin = X.Min();
        float xMax = X.Max();

        foreach(float x in X)
        {
            Gizmos.DrawLine(
                transform.TransformPoint(new Vector3(x, 0, zMin)),
                transform.TransformPoint(new Vector3(x, 0, zMax)));
        }

        foreach(float z in Z)
        {
            Gizmos.DrawLine(
                transform.TransformPoint(new Vector3(xMin, 0, z)),
                transform.TransformPoint(new Vector3(xMax, 0, z)));
        }
    }

}
