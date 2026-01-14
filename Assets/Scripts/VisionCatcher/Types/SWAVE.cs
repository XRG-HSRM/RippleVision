using System;
using Unity.VisualScripting;
using UnityEngine;

public class SWAVE : VisionCatcher
{
    private Material SWaveMat;

    private float speed = 0f;
    static float speedR = 6f / 180f;

    private void Awake()
    {
        transform.GetComponentInChildren<MeshRenderer>().enabled = false;
    }

    public override void StartVisionCatcher()
    {
        if (running) { return; }
        CreateSWAVE();
        running = true;
    }

    public override void StopVisionCatcher()
    {
        if (!running) { return; }
        running = false;
        DisableSWAVE();
    }

    private void CreateSWAVE()
    {
        transform.GetComponentInChildren<MeshRenderer>().enabled = true;
        SWaveMat = GetComponentInChildren<Renderer>().material;
    }
    
    private void DisableSWAVE()
    {
        transform.GetComponentInChildren<MeshRenderer>().enabled = false;
    }

    private void SetSpherePosition()
    {
        transform.position = mainCamera.transform.position;
    }

    private void SetSphereRotation()
    {
        SWaveMat.SetVector("_WorldRippleCenter", mainCamera.transform.position + (transformToCatch.position - mainCamera.transform.position).normalized);
    }

    private void SetWaveSpeed()
    {
        speed += Time.deltaTime * CalculateEyeGazeAngle();
        SWaveMat.SetFloat("_Speed", speed);
    }
    
    private float CalculateEyeGazeAngle()
    {
        float result = 0f;
        if(controller != null)
        {
            result = Vector3.Angle(controller.currentEyeTrackingScript.eyetrackingTransform.position - mainCamera.transform.position, transformToCatch.position - mainCamera.transform.position);
        }
        result *= speedR;
        return result;
    }

    private void SetSphereScale()
    {
        transform.localScale = new Vector3(1f, 1f, 1f) * (mainCamera.transform.position - transformToCatch.position).magnitude;
    }

    private void Update()
    {if (!running) { return; }
        SetSpherePosition();
        SetSphereRotation();
        SetSphereScale();
        SetWaveSpeed();
    }
}
