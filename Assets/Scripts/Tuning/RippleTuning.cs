using UnityEngine;
using System.IO;

public class RippleTuning : VisionCatcher
{
    public TuneTask tuneTask;
    public TunableRipple[] tunable;

    [SerializeField] private Material rippleMaterial;
    public bool onlyInOneEye;
    public bool stereoInverse;
    public float directionRange;
    public float minDistance;
    public float foveaSize;
    public float angle;
    public float fallOff;
    public float rippleSpeed;
    public float brightness;
    public float intensity;
    public float frequency;
    VRCamera cameraScript;

    public override void StartVisionCatcher()
    {
        if (running) { return; }
        cameraScript = mainCamera.GetComponent<VRCamera>();
        cameraScript.SetCustomRenderFunction(TunnelVisionRenderer);
        running = true;
    }

    public override void StopVisionCatcher()
    {
        if (!running) { return; }
        cameraScript.ResetToDefaultRenderFunction();
        running = false;
    }

    private void TunnelVisionRenderer(RenderTexture source, RenderTexture destination)
    {
        if (!running) { return; }
        if (stereoInverse && onlyInOneEye)
        {
            onlyInOneEye = false;
        }
        // cant be greater than screensize
        if (foveaSize > 0.99)
        {
            foveaSize = 0;
        }
        Vector3 screenPosEyeObj = mainCamera.WorldToViewportPoint(tuningController.currentEyeTrackingScript.eyetrackingTransform.position);
        Vector3 viewPos = mainCamera.WorldToViewportPoint(transformToCatch.position);
        Vector3 screenPosSO = viewPos;
        Vector3 screenPosEObj = new Vector3(screenPosEyeObj.x, screenPosEyeObj.y, 0);
        if (Vector2.SqrMagnitude(screenPosSO - screenPosEObj) < minDistance || screenPosEyeObj.z < 0)
        {
            rippleMaterial.SetFloat("_Angle", 0);
        }
        else
        {
            rippleMaterial.SetVector("_Center", screenPosSO);
            rippleMaterial.SetVector("_Target", screenPosEyeObj);
            rippleMaterial.SetFloat("_Angle", angle * 0.5f * Mathf.PI / 180f);
            rippleMaterial.SetFloat("_FallOff", fallOff);
            // should keep as "1" for default value
            rippleMaterial.SetFloat("_Brightness", brightness);
            rippleMaterial.SetFloat("_Speed", rippleSpeed);
            rippleMaterial.SetFloat("_Intensity", intensity);
            rippleMaterial.SetFloat("_Frequency", frequency);
            rippleMaterial.SetFloat("_FoveaSize", foveaSize);
            rippleMaterial.SetInt("_OneEye", onlyInOneEye ? 1 : 0);
            rippleMaterial.SetInt("_StereoInverse", stereoInverse ? 1 : 0);
        }
        //}
        RenderTexture.active = destination;
        GL.PushMatrix();
        GL.LoadOrtho();
        Graphics.Blit(source, destination, rippleMaterial);
        GL.PopMatrix();
    }

    public static Vector2 GetScreenEdgePosition(Camera cam, Vector3 viewPos)
    {
        Vector2 screenCenter = new Vector2(0.5f, 0.5f);
        Vector2 dir;
        if (viewPos.z < 0)
        {
            viewPos.x = 1f - viewPos.x;
            viewPos.y = 1f - viewPos.y;
        }

        Vector2 centered = new Vector2(viewPos.x, viewPos.y) - screenCenter;
        if (viewPos.z > 0 && Mathf.Abs(centered.x) <= 0.5f && Mathf.Abs(centered.y) <= 0.5f)
        {
            return new Vector2(viewPos.x, viewPos.y);
        }
        dir = centered.normalized;
        float slope = dir.y / dir.x;
        Vector2 edge = Vector2.zero;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            edge.x = dir.x > 0 ? 0.5f : -0.5f;
            edge.y = edge.x * slope;
        }
        else
        {
            edge.y = dir.y > 0 ? 0.5f : -0.5f;
            edge.x = edge.y / slope;
        }
        edge += screenCenter;
        edge = new Vector2(Mathf.Clamp01(edge.x), Mathf.Clamp01(edge.y));

        return edge;
    }

    private void Update()
    {
        foreach(TunableRipple tunableVariable in tunable)
        {
            switch (tunableVariable)
            {
                case TunableRipple.angle:
                    // angle
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        angle += 5f;
                    }
                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        angle -= 5f;
                    }
                    if (angle > 180) angle = 180;
                    if (angle < 0) angle = 0;
                    break;
                case TunableRipple.frequency:
                    // frequency
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        frequency += 0.0001f;
                    }
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        frequency -= 0.0001f;
                    }
                    if(frequency < 0.00005f)
                    {
                        frequency = 0.00005f;
                    }
                    break;
                case TunableRipple.speed:
                    // speed (based on time)
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        rippleSpeed += 0.005f;
                    }
                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        rippleSpeed -= 0.005f;
                    }
                    if (rippleSpeed < 0) rippleSpeed = 0;
                    if (rippleSpeed > 1) rippleSpeed = 1;
                    break;
                case TunableRipple.intensity:
                    // intensity
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        intensity += 0.01f;
                    }
                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        intensity -= 0.01f;
                    }
                    if (intensity < 0) intensity = 0;
                    if (intensity > 1) intensity = 1;
                    break;
                default:
                    break;
            }
        }
    }
}

public enum TunableRipple
{
    angle,
    frequency,
    speed,
    intensity,
}

public enum TuneTask
{
    barelyVisible,
    visibleButAcceptable
}
