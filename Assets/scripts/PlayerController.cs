using UnityEngine;
using System.Collections.Generic;

public enum ModRayStates { Offline, Elongating, PowerUp, Fire };

public delegate void ModRayActivation(RaycastHit hit);
public delegate void ModRayStateChange(ModRayStates oldState, ModRayStates state);

public class PlayerController : MonoBehaviour {

    public event ModRayActivation OnModRayActivation;
    public event ModRayStateChange OnModRayStateChage;

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

    [SerializeField]
    float modRayHitActivationTime = 2f;

    float modRayHitInit;

    ModRayStates modRayHitting = ModRayStates.Offline;

    RaycastHit modRayCamHit;

    [SerializeField]
    bool startActive = false;

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
            if (value)
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
            playerActive = startActive;
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
            if (modRayHitting == ModRayStates.Offline && Input.GetButtonDown("Fire"))
            {
                ElongateModificationRay();
            } else if (modRayHitting != ModRayStates.Offline && Input.GetButton("Fire"))
            {
                ElongateModificationRay();
            } else if (Input.GetButtonUp("Fire"))
            {
                DeactivateModRay();
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
            float strifeImpulse = Input.GetAxis("Rotate");
            if (Mathf.Abs(strifeImpulse) > 0.02f)
            {
                rb.velocity += transform.right * strifeImpulse * walkFactor * Time.deltaTime;
                float m2 = rb.velocity.sqrMagnitude;
                if (m2 > maxWalkSq)
                {
                    rb.velocity = rb.velocity.normalized * Mathf.Sqrt(Mathf.Lerp(maxWalkSq, m2, walkCapF));
                }
            }

            Vector3 newEuler = new Vector3(-Input.GetAxis("LookVertical") * headVertSensitivity, Input.GetAxis("LookHorizontal") * headHorSensitivity) * Time.deltaTime + transform.localEulerAngles;
            newEuler.z = 0f;
            newEuler.x = ClampAngleNegPos(newEuler.x, -10, 40);
            transform.localEulerAngles = newEuler;

            float walkImpulse = Input.GetAxis("Walk");
            if (Mathf.Abs(walkImpulse) > 0.02f)
            {

                head.localEulerAngles = Vector3.Lerp(head.localEulerAngles, Vector3.zero, 1 - Time.deltaTime * 2f);

                rb.velocity += transform.forward * walkImpulse * walkFactor * Time.deltaTime;
                float m2 = rb.velocity.sqrMagnitude;
                if (m2 > maxWalkSq)
                {
                    rb.velocity = rb.velocity.normalized * Mathf.Sqrt(Mathf.Lerp(maxWalkSq, m2, walkCapF));
                }
            } else
            {
                newEuler = new Vector3(-Input.GetAxis("LookVertical") * headVertSensitivity, Input.GetAxis("LookHorizontal") * headHorSensitivity) * Time.deltaTime + head.localEulerAngles;
                newEuler.x = ClampAngleNegPos(newEuler.x, -10, 40);
                newEuler.y = ClampAngleNegPos(newEuler.y, -20, 20);
                newEuler.z = 0f;
                head.localEulerAngles = newEuler;

                float m = rb.velocity.magnitude;
                if (m < 0.1f)
                {
                    rb.velocity = Vector3.zero;
                }
                else {
                    rb.velocity = rb.velocity.normalized * m * Mathf.Clamp01(1 - velocityDecay * Time.deltaTime);
                }
            }

        }
    }

    Vector3 hitPoint = Vector3.zero;

    void ElongateModificationRay()
    {

        //Make camera ray;
        Ray camRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        

        if (Physics.Raycast(camRay, out modRayCamHit, modRayMaxLength, modLayers, QueryTriggerInteraction.Ignore)) {


            modRayTime += modRaySpeed * Time.deltaTime;
            hitPoint = modRayCamHit.point;
            Vector3 modRayTarget = modRaySource.InverseTransformPoint(hitPoint);
            float hitDistance = modRayTarget.magnitude;
            
            modRayTime = Mathf.Clamp(modRayTime, 0, hitDistance);

            ModRayStates oldHitState = modRayHitting;
            modRayHitting = modRayTime < 0.999f * hitDistance ? ModRayStates.Elongating : ModRayStates.PowerUp;
            if ((oldHitState == ModRayStates.Offline || oldHitState == ModRayStates.Elongating) && modRayHitting == ModRayStates.PowerUp)
            {
                if (!modRayEdge.isPlaying)
                {
                    modRayEdge.Play();
                }
                modRayEdge.Emit(Random.Range(50, 200));
                modRayHitInit = Time.timeSinceLevelLoad;
                
            } else if (Time.timeSinceLevelLoad - modRayHitInit > modRayHitActivationTime && modRayHitting == ModRayStates.PowerUp)
            {                
                modRayHitting = ModRayStates.Fire;
                modRayEdge.Emit(Random.Range(100, 400));
            }

            if (oldHitState != modRayHitting && OnModRayStateChage != null)
            {
                OnModRayStateChage(oldHitState, modRayHitting);
            }

            lRend.SetPosition(1, modRayTarget.normalized * modRayTime);

            modRayEdge.transform.position = modRaySource.TransformPoint( modRayTarget.normalized * modRayTime * 0.93f);            
            
            if (modRayHitting == ModRayStates.Elongating)
            {
                modRayEdge.transform.LookAt(head);
            } else
            {
                modRayEdge.transform.rotation = Quaternion.LookRotation(modRayCamHit.normal, Vector3.up);
            }

            
        } else
        {
            if (modRayHitting != ModRayStates.Offline && OnModRayStateChage != null)
            {
                OnModRayStateChage(modRayHitting, ModRayStates.Offline);
            } 
            modRayHitting = ModRayStates.Offline;
        }      
        
        if (modRayHitting == ModRayStates.Fire)
        {
            ActivateModificationRay();
        }
        else if (modRayHitting == ModRayStates.Offline)
        {
            DeactivateModRay();
        }

    }

    void ActivateModificationRay()
    {
        if (OnModRayActivation != null)
        {
            OnModRayActivation(modRayCamHit);
        }
        DeactivateModRay();
    }

    void DeactivateModRay()
    {
        lRend.SetPosition(1, Vector3.zero);
        modRayTime = 0;

        if (modRayHitting != ModRayStates.Offline && OnModRayStateChage != null)
        {
            OnModRayStateChage(modRayHitting, ModRayStates.Offline);
        }
        modRayHitting = ModRayStates.Offline;

        modRayEdge.Stop();
        modRayHitInit = -modRayHitActivationTime;


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
