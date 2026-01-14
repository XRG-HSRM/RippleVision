using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environments : MonoBehaviour
{
    public Transform environmentTransform;
    public Transform playerLocation;
    public bool enableDataCollector = true;
    public bool enableGuidance = true;
    public GameObject searchObjectPrefab;
    public Transform searchObject;
    public string environmentName;
    public Transform questionLocation;
    public float maxSearchDistance;
    public SceneController controller;
    public TuningController tuningController;

    [HideInInspector]
    public bool isQuestionScene = false;

    public virtual void SetupEnvironment()
    {
        // dummy search transform, we don't use guidance in these scenes
        if (searchObject == null && !enableGuidance)
        {
            searchObject = transform;
        }
        environmentName = transform.name;
        if(playerLocation == null)
        {
            playerLocation = environmentTransform;
        }
    }

    public void ApplyVisionCatcherToScene(VisionCatcher visionCatcher)
    {

    }

    public virtual Transform GetSearchObject()
    {
        return searchObject;
    }

    public virtual Transform GetQuestionTransform()
    {
        return questionLocation;
    }
}
