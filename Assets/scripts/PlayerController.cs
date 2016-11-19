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
    LineRenderer lRend;

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
    bool playerPause = false;

    [SerializeField]
    KeyCode deactivateKey;

    [SerializeField]
    UIPointLock pointer;

    float modRayTime = 0;

    [SerializeField]
    float modRaySpeed = 1;

    [SerializeField]
    Transform modRaySource;

    [SerializeField]
    float modRayMaxLength = 10;

    [SerializeField]
    LayerMask modLayers;

    [SerializeField]
    ParticleSystem modRayEdge;

    bool modRayHitting = false;

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
            if (true)
            {
                playerPause = false;
            } else
            {
                modRayTime = 0;
            }
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


    void Update()
    {
        if (!playerPause)
        {
            if (Input.GetButton("Fire"))
            {
                ElongateModificationRay();
            } else if (Input.GetButtonUp("Fire"))
            {
                ActivateModificationRay();
            }
        }

        if (Input.GetKeyDown(deactivateKey))
        {
            if (canWalk || playerPause)
            {
                if (playerPause)
                {
                    playerActive = true;
                    pointer.enabled = true;
                } else
                {
                    playerActive = false;
                    pointer.enabled = false;
                    playerPause = true;
                }
            }
        }
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

    Vector3 hitPoint = Vector3.zero;

    void ElongateModificationRay()
    {

        //Make camera ray;
        Ray camRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        RaycastHit camRayHit;
        

        if (Physics.Raycast(camRay, out camRayHit, modRayMaxLength, modLayers, QueryTriggerInteraction.Ignore)) {
            modRayTime += modRaySpeed * Time.deltaTime;
            hitPoint = camRayHit.point;
            Vector3 modRayTarget = modRaySource.InverseTransformPoint(hitPoint);            
            modRayTime = Mathf.Clamp(modRayTime, 0, modRayTarget.magnitude);

            lRend.SetPosition(1, modRayTarget.normalized * modRayTime);

            modRayEdge.transform.position = modRaySource.TransformPoint( modRayTarget.normalized * modRayTime * 0.95f);

            modRayHitting = true;

        } else
        {
            modRayTime = 0;
            lRend.SetPosition(1, Vector3.zero);
            modRayHitting = false;
        }      
        
        if (modRayHitting)
        {
            if (!modRayEdge.isPlaying)
            {
                modRayEdge.Play();
            } else
            {
                //modRayEdge.Stop();
            }
        }  

    }

    void ActivateModificationRay()
    {

        lRend.SetPosition(1, Vector3.zero);
        modRayTime = 0;
        modRayHitting = false;
        modRayEdge.Stop();
    }

    /*
    void OnDrawGizmos()
    {
        if (modRayHitting)
        {
            Gizmos.DrawLine(modRaySource.position, hitPoint);
        }
    }

    */
}
