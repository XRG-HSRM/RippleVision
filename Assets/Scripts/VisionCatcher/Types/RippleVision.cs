using Unity.VisualScripting;
using UnityEngine;

public class RippleVision : VisionCatcher
{
    [SerializeField] private Material rippleMaterial;
    public bool onlyInOneEye;
    public bool stereoInverse;
    public bool useDirection;
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
        // cant be greater than screensize
        if (foveaSize > 0.99)
        {
            foveaSize = 0;
        }
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
        Vector3 screenPosEyeObj = mainCamera.WorldToViewportPoint(controller.currentEyeTrackingScript.eyetrackingTransform.position);
        Vector3 screenPosSO = GetOutsideFOVPosition(mainCamera, transformToCatch.position);
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
        RenderTexture.active = destination;
        GL.PushMatrix();
        GL.LoadOrtho();
        Graphics.Blit(source, destination, rippleMaterial);
        GL.PopMatrix();
    }


    public Vector3 GetOutsideFOVPosition(Camera cam, Vector3 objectPos)
    {
        Vector3 viewPos = GetRelativePositionToCamera(cam, objectPos);
        if (viewPos.z >= 0)
        {
            return cam.WorldToViewportPoint(objectPos);
        }
        Vector3 newViewPos;
        Transform camera = cam.transform;
        Vector2 cameraXZ = new Vector2(camera.position.x, camera.position.z);
        Vector2 objectXZ = new Vector2(objectPos.x, objectPos.z);
        float objectDistance = (camera.position - objectPos).magnitude;
        Vector3 toTarget = objectPos - camera.position;
        Vector3 onPlane = Vector3.ProjectOnPlane(toTarget, camera.forward);
        Vector3 direction = onPlane.normalized;
        newViewPos = camera.position
            +
            direction
            *
            (
                objectDistance
                +
                CalculateArcLength(cameraXZ - objectXZ, direction * objectDistance)
            )
            + camera.forward
            ;
        Vector3 result = cam.WorldToViewportPoint(newViewPos);
        return result;
    }
    private Vector3 GetRelativePositionToCamera(Camera camera, Vector3 objPos)
    {
        return camera.transform.InverseTransformPoint(objPos);
    }

    static float CalculateArcLength(Vector2 origin, Vector2 right)
    {
        float dotProduct = Vector2.Dot(origin, right);
        float r = origin.magnitude;
        float angle = Mathf.Acos(dotProduct / (r * r));
        float arcLength = r * angle;
        return arcLength;
    }
}
