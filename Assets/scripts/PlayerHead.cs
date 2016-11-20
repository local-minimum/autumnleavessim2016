using UnityEngine;
using System.Collections.Generic;

public class PlayerHead : MonoBehaviour {

    Camera headCam;

    [SerializeField]
    float normalFieldOfView = 60;

    [SerializeField]
    float focusFiledOfView = 45;

    [SerializeField]
    float focusTransitionDuration = 1f;

    [SerializeField]
    AnimationCurve focusTransition;

    void Start()
    {
        headCam = GetComponentInChildren<Camera>();
    }

    void OnEnable()
    {
        PlayerController.Instance.OnModRayStateChage += Player_ModRayStateChange;
    }

    void OnDisable()
    {
        PlayerController.Instance.OnModRayStateChage -= Player_ModRayStateChange;
    }

    private void Player_ModRayStateChange(ModRayStates oldState, ModRayStates state)
    {
        if (state == ModRayStates.Offline)
        {
            StartCoroutine(DefocusVision());
        } else if (oldState == ModRayStates.Offline)
        {
            StartCoroutine(FocusVision());
        }
    }

    IEnumerator<WaitForSeconds> DefocusVision()
    {
        float start = Time.timeSinceLevelLoad;
        float duration = 0;
        float fovStart = Mathf.Max(headCam.fieldOfView, focusFiledOfView);
        while (duration < 1)
        {
            headCam.fieldOfView = Mathf.Lerp(fovStart, normalFieldOfView, focusTransition.Evaluate(duration));
            yield return new WaitForSeconds(0.02f);
            duration = Mathf.Clamp01( (Time.timeSinceLevelLoad - start) / focusTransitionDuration);
        }
        headCam.fieldOfView = normalFieldOfView;
    } 

    IEnumerator<WaitForSeconds> FocusVision()
    {
        float start = Time.timeSinceLevelLoad;
        float duration = 0;
        float fovStart = Mathf.Min(headCam.fieldOfView, normalFieldOfView);
        while (duration < 1)
        {
            headCam.fieldOfView = Mathf.Lerp(fovStart, focusFiledOfView, focusTransition.Evaluate(duration));
            yield return new WaitForSeconds(0.02f);
            duration = Mathf.Clamp01((Time.timeSinceLevelLoad - start) / focusTransitionDuration);
        }
        headCam.fieldOfView = focusFiledOfView;
    }
}
