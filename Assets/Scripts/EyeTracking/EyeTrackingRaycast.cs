using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class EyeTrackingRaycast : MonoBehaviour
{
    [SerializeField] private InputActionAsset actionAsset;
    [SerializeField] private InputActionReference eyePose;

    public bool EnableEyeTracking = true;

    public static Vector3? CurrentGazeHitPoint { get; private set; } = null;
    private float rayLength = 35f;
    [HideInInspector]
    public Transform eyetrackingTransform;
    public bool eyeTrackingActive = false;
    [HideInInspector]
    public bool lookingAtSearchObject = false;
    [HideInInspector]
    public Transform mainCamera;
    public Ray gazeRay;
    public bool isValid = false;

    UnityEngine.InputSystem.XR.PoseState pose;

    // DELETEME
    public Transform eyeTrackingSphere;

    private void Start()
    {
        if (!EnableEyeTracking)
        {
            Debug.LogWarning("Eye tracking is disabled. Please enable it in the inspector.");
        }
    }

    private void OnEnable()
    {
        if (actionAsset != null)
        {
            actionAsset.Enable();
        }
    }

    private void Update()
    {
        if (eyeTrackingActive)
        {
            pose = eyePose.action.ReadValue<UnityEngine.InputSystem.XR.PoseState>();
            CheckEyeOpenness();
            PerformEyeGazeRaycast();
        }
    }

    private void CheckEyeOpenness()
    {
        isValid = true;
        //Debug.Log(pose.isTracked);
        //if (pose.isTracked)
        //{
        //    isValid = true;
        //}
        //else
        //{
        //    isValid = false;
        //}
    }

    private void PerformEyeGazeRaycast()
    {
        lookingAtSearchObject = false;
        if (isValid && eyetrackingTransform != null)
        {
            gazeRay.origin = mainCamera.position;
            gazeRay.direction = (pose.rotation * Vector3.forward).normalized;

            if (!EnableEyeTracking)
            {
                gazeRay.origin = mainCamera.position;
                gazeRay.direction = mainCamera.forward;
            }

            RaycastHit[] hits = Physics.RaycastAll(gazeRay.origin, gazeRay.direction, rayLength, ~0, QueryTriggerInteraction.Collide);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.isTrigger)
                {
                    if (hit.collider.CompareTag("SearchObject"))
                    {
                        lookingAtSearchObject = true;
                        CurrentGazeHitPoint = hit.point;
                        eyetrackingTransform.position = hit.point;
                        break;
                    }
                    else
                    {
                        lookingAtSearchObject = false;
                    }
                }
            }
            if (!lookingAtSearchObject)
            {
                CurrentGazeHitPoint = null;
                lookingAtSearchObject = false;
                eyetrackingTransform.position = gazeRay.origin + (gazeRay.direction.normalized * 10f);
            }
        }
        //deleteme //debugging
        //if (eyetrackingTransform != null)
        //{
        //    eyetrackingTransform.position = mainCamera.position + mainCamera.forward * 2f;
        //    lookingAtSearchObject = true;
        //}
        if (eyeTrackingSphere != null && eyetrackingTransform != null)
        {
            eyeTrackingSphere.position = eyetrackingTransform.position;
        }
    }
}
