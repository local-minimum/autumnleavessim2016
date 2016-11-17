using UnityEngine;
using System.Collections;

public class RotateMe : MonoBehaviour {

    [SerializeField]
    float speed = 0.1f;

    public bool rotating = true;
	
	void Update () {

        if (rotating) {
            transform.RotateAround(Vector3.zero, Vector3.up, speed * Time.deltaTime);
        }
        	
	}
}
