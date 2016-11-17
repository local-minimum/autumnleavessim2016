using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

    static PlayerController instance;

    public static PlayerController Instance
    {
        get
        {
            return instance;
        }
    }
    
    [SerializeField]
    Transform head;

    [SerializeField]
    Transform feet;

    [SerializeField]
    Canvas canvas;

    Rigidbody rb;
    Collider[] colliders;

    [SerializeField]
    float rotoVertFactor = 50;

    [SerializeField]
    float rotoHorFactor = 50;

    bool canWalk;

    bool playerActive
    {
        set
        {
            for (int i=0; i<colliders.Length; i++)
            {
                colliders[i].enabled = value;
            }
            canvas.gameObject.SetActive(value);
            canWalk = value;
            rb.isKinematic = !value;
        }
    }

    public void Spawn(Transform target)
    {
        playerActive = false;
        StartCoroutine(_spawnTransition(target));

    }

    IEnumerator<WaitForSeconds> _spawnTransition(Transform target)
    {
        float t = 0;
        while (t < 1) {
            t = Mathf.Clamp01(t + 0.05f);
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, t);
            transform.position = Vector3.Lerp(transform.position, target.position + target.up * transform.TransformVector(feet.localPosition).magnitude, t);
            yield return new WaitForSeconds(0.1f);

        }

        transform.rotation = target.rotation;
        transform.position = target.position + target.up * transform.TransformVector(feet.localPosition).magnitude;
        yield return new WaitForSeconds(1f);
        playerActive = true;
    }

	void Start () {
        colliders = GetComponents<Collider>();
        rb = GetComponent<Rigidbody>();
        instance = this;
        playerActive = false;
	}

    float ClampAngleNegPos(float v, float a, float b)
    {
        v %= 360f;
        if (v - 360 < a && v > b)
        {
            if (v - b < a + 360 - v)
            {
                return b;
            } else
            {
                return a;
            }
        }
        return v;
    }

    void LateUpdate()
    {
        if (canWalk)
        {
            Vector3 newEuler = new Vector3(Input.GetAxis("LookVertical") * rotoVertFactor, Input.GetAxis("LookHorizontal") * rotoHorFactor) * Time.deltaTime + head.localEulerAngles;
            newEuler.x = ClampAngleNegPos(newEuler.x, -10, 40);
            newEuler.y = ClampAngleNegPos(newEuler.y, -20, 20);
            newEuler.z = 0f;
            head.localEulerAngles = newEuler;
        }
    }
}
