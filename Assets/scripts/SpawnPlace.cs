using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpawnPlace : MonoBehaviour {

    [SerializeField]
    FloorGenerator floor;

    [SerializeField]
    RoomsGenerator rooms;

    [SerializeField]
    bool placed = false;

    [SerializeField]
    float proximityThreshold = 0.5f;

    void Update()
    {

        if (rooms.Generated && !placed)
        {
            
            placed = true;
            PlaceSelf();

        } else if (!rooms.Generated && placed)
        {
            placed = false;
        }
    }

    void PlaceSelf()
    {
        List<List<Vector3>> floorParts = floor.GetPartialFloorRects(false).ToList();
        List<Vector3> outerWall = floor.GetCircumferance(false).ToList();
        int outerWallsLength = outerWall.Count;

        bool validated = false;

        while (!validated)
        {
            List<Vector3> compartment = floorParts[Random.Range(0, floorParts.Count)];

            Vector3 v1 = compartment[1] - compartment[0];
            Vector3 v2 = compartment[2] - compartment[1];

            //Vector3 c = compartment.Aggregate(Vector3.zero, (sum, e) => sum + e) / compartment.Count;

            Vector3 pt = compartment[0] + Random.value * v1 + Random.value * v2;
            
            validated = true;
            
            for (int i=0; i<outerWallsLength; i++)
            {
                float d = ProcGenHelpers.GetMinDist(pt, outerWall[i], outerWall[(i + 1) % outerWallsLength]);                
                if (d < proximityThreshold)
                {
                    Debug.Log("Invalid");
                    validated = false;
                    break;
                }
            }

            if (validated)
            {
                transform.position = pt;
            }
            
        }

        GetComponentInParent<RotateMe>().rotating = false;
        PlayerController.Instance.Spawn(transform);

    }

}
