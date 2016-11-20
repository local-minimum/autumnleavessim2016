using UnityEngine;
using System.Collections;

public class DoorBuilder : MonoBehaviour {

    [SerializeField]
    GameObject doorPrefab;

    [SerializeField]
    float doorSpawnY = 2f;

    [SerializeField]
    float minSqDistanceBetweenDoors = 10f;

    void OnEnable()
    {
        PlayerController.Instance.OnModRayActivation += Player_OnModRayActivation;
    }

    void OnDisable()
    {
        PlayerController.Instance.OnModRayActivation -= Player_OnModRayActivation;
    }

    private void Player_OnModRayActivation(RaycastHit hit)
    {
        Vector3 localPos = transform.InverseTransformPoint(hit.point);
        localPos.y = doorSpawnY;

        for (int i=0, l=transform.childCount; i< l; i++)
        {
            if (Vector3.SqrMagnitude(transform.GetChild(i).localPosition - localPos) < minSqDistanceBetweenDoors)
            {
                return;
            }
        }

        GameObject door = (GameObject) Instantiate(doorPrefab, transform);
        door.tag = gameObject.tag;
        door.layer = gameObject.layer;
        door.transform.rotation = Quaternion.LookRotation(ProcGenHelpers.Get90CCW(hit.normal), Vector3.up);

        door.transform.localPosition = localPos;
    }
}
