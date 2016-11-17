using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

    static PlayerController instance;

    public static PlayerController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PlayerController>();
                instance.Start();
            }
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
    float headVertSensitivity = 20;

    [SerializeField]
    float headHorSensitivity = 20;

    [SerializeField]
    float maxWalkSq = 10;

    [SerializeField]
    float walkFactor = 2;

    float walkCapF = 0.2f;

    [SerializeField]
    float rotationSensitivity = 20;

    [SerializeField]
    float velocityDecay = 2f;

    bool canWalk;
    bool started = false;

    public bool playerActive
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
        yield return new WaitForSeconds(0.2f);
        playerActive = true;
    }

	void Start () {
        if (!started)
        {
            started = true;
            colliders = GetComponents<Collider>();
            rb = GetComponent<Rigidbody>();
            instance = this;
            playerActive = false;
        }
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
            Vector3 newEuler = new Vector3(-Input.GetAxis("LookVertical") * headVertSensitivity, Input.GetAxis("LookHorizontal") * headHorSensitivity) * Time.deltaTime + head.localEulerAngles;
            newEuler.x = ClampAngleNegPos(newEuler.x, -10, 40);
            newEuler.y = ClampAngleNegPos(newEuler.y, -20, 20);
            newEuler.z = 0f;
            head.localEulerAngles = newEuler;

            float walkImpulse = Input.GetAxis("Walk");
            if (Mathf.Abs(walkImpulse) > 0.02f)
            {
                rb.velocity += transform.forward * walkImpulse * walkFactor * Time.deltaTime;
                float m2 = rb.velocity.sqrMagnitude;
                if (m2 > maxWalkSq)
                {
                    rb.velocity = rb.velocity.normalized * Mathf.Sqrt(Mathf.Lerp(maxWalkSq, m2, walkCapF));
                }
            } else
            {
                float m = rb.velocity.magnitude;
                if (m < 0.1f)
                {
                    rb.velocity = Vector3.zero;
                }
                else {
                    rb.velocity = rb.velocity.normalized * m * Mathf.Clamp01(1 - velocityDecay * Time.deltaTime);
                }
            }
            Vector3 roto = transform.localEulerAngles;
            roto.y += Input.GetAxis("Rotate") * rotationSensitivity;
            roto.z = 0;
            roto.x = 0;
            transform.localEulerAngles = roto;
        }
    }
}
