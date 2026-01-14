using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionCatcher : MonoBehaviour
{
    public string visionCatcherName;
    public Transform transformToCatch;
    public Camera mainCamera;
    public bool running;
    [HideInInspector]
    public SceneController controller;
    [HideInInspector]
    public TuningController tuningController;

    private void Awake()
    {
        running = false;
    }

    public void SetupVisionCatcher(Transform transformToCatch, Camera mainCamera, TuningController tuningController)
    {
        this.transformToCatch = transformToCatch;
        this.mainCamera = mainCamera;
        this.tuningController = tuningController;
    }

    public void SetupVisionCatcher(Transform transformToCatch, Camera mainCamera, SceneController controller)
    {
        this.transformToCatch = transformToCatch;
        this.mainCamera = mainCamera;
        this.controller = controller;
    }

    public virtual void StartVisionCatcher()
    {

    }

    public virtual void StopVisionCatcher()
    {

    }
}
