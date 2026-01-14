using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

public class StaticPredefinedSpawn : Environments
{
    [SerializeField] private string UniqueIdentifier;
    [SerializeField] private List<GameObject> nonSearchObjects;
    [SerializeField] private Transform searchObjectLocations;
    [Tooltip("Rotates the initiazed search objects to face the camera at start")]
    [SerializeField] private bool rotateObjectsToCamera;
    [SerializeField] private float objectHeight;
    [SerializeField] private float objectScale;
    private List<Transform> searchObjectLocationsList = new List<Transform>();
    private Transform searchObjectLocation;
    bool objectSet = false;

    private void Awake()
    {
        foreach (Transform t in searchObjectLocations)
        {
            searchObjectLocationsList.Add(t);
        }
        if (searchObjectLocationsList.Count < 10)
        {
            Debug.LogError("Not enough locations to position the searchObject!");
        }
        if (UniqueIdentifier == "")
        {
            UniqueIdentifier = transform.name;
        }
    }

    private Transform GetRelevantSearchObjectLocation()
    {
        searchObjectLocationsList = controller.utilities.ShuffleList(searchObjectLocationsList, controller.seed);
        List<string> usedLocations = controller.GetAllUsedSearchLocations();
        for (int i = 0; i < searchObjectLocationsList.Count; i++)
        {
            if (!objectSet && !usedLocations.Contains(UniqueEnvStrings(i)))
            {
                searchObjectLocation = searchObjectLocationsList[i];
                controller.SearchLocationUsed(UniqueEnvStrings(i));
                objectSet = true;
                //break;
            }
            else
            {
                int random = Random.Range(0, 4);
                if (random > 2) continue;
                GameObject newObject = Instantiate(nonSearchObjects[Random.Range(0, nonSearchObjects.Count)], environmentTransform);
                newObject.transform.position = searchObjectLocationsList[i].transform.position + new Vector3(0, objectHeight, 0);
                newObject.transform.localScale *= objectScale;
                if (rotateObjectsToCamera)
                {
                    Vector3 direction = controller.GetCamera().transform.position - newObject.transform.position;
                    direction.y = 0f;
                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        newObject.transform.rotation = targetRotation;
                    }
                }
                else
                {
                    newObject.transform.eulerAngles = new Vector3(0, Random.Range(0f, 360f), 0);
                }
                newObject.transform.eulerAngles = new Vector3(newObject.transform.eulerAngles.x, newObject.transform.eulerAngles.y, Random.Range(0f, 360f));
            }
        }
        if (searchObjectLocation == null)
        {
            Debug.LogError("Error in setting search locations. Using default.");
            searchObjectLocation = searchObjectLocationsList[0];
        }

        return null;
    }

    private string UniqueEnvStrings(int i)
    {
        return i + UniqueIdentifier;
    }

    public override void SetupEnvironment()
    {
        GetRelevantSearchObjectLocation();
        GameObject newObject = Instantiate(searchObjectPrefab, environmentTransform);
        newObject.transform.position = searchObjectLocation.transform.position + new Vector3(0, objectHeight, 0);
        newObject.transform.localScale *= objectScale;
        if (rotateObjectsToCamera)
        {
            Vector3 direction = controller.GetCamera().transform.position - newObject.transform.position;
            direction.y = 0f;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                newObject.transform.rotation = targetRotation;
            }
        }
        else
        {
            newObject.transform.eulerAngles = new Vector3(0, Random.Range(0f, 360f), 0);
        }
        newObject.transform.eulerAngles = new Vector3(newObject.transform.eulerAngles.x, newObject.transform.eulerAngles.y, Random.Range(0f, 360f));
        searchObject = newObject.transform;
    }
}
