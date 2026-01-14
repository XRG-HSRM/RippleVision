using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtificialDynamics : Environments
{
    [SerializeField] private GameObject simpleObjectPrefab;
    [SerializeField] private GameObject outerObjectPrefab;
    [SerializeField] private List<GameObject> meshes = new List<GameObject>();
    [SerializeField] private List<Material> materials = new List<Material>();
    [SerializeField] private float objectScale;
    [SerializeField] private int numberOfObjects;
    [SerializeField] private float[] heights;
    [SerializeField] private float[] radii;
    [SerializeField] private float[] speeds;

    public override void SetupEnvironment()
    {
        base.SetupEnvironment();
        SetupArtificialEnvironment();
    }

    private void SetupArtificialEnvironment()
    {
        //SpawnFixedSearchObject(searchObjectPrefab, new Vector3(0, 2f, 3f));
        //SpawnObject(simpleObjectPrefab, new Vector3(0, 2f, -3f));
        SpawnSearchObject(searchObjectPrefab);
        for (int i = 0; i < numberOfObjects; i++)
        {
            GameObject prefab = simpleObjectPrefab;
            SpawnObject(prefab);
        }
    }

    private void SpawnObject(GameObject prefab, Vector3 position)
    {
        GameObject obj = Instantiate(prefab, position, Random.rotation);
        obj.transform.SetParent(environmentTransform);
        obj.transform.localScale = new Vector3(obj.transform.localScale.x * objectScale, obj.transform.localScale.y * objectScale, obj.transform.localScale.z * objectScale);
        SimpleObject newObjectScript = obj.GetComponent<SimpleObject>();
        newObjectScript.SetMeshObject(meshes[Random.Range(0, meshes.Count)]);
        newObjectScript.SetMaterial(materials[Random.Range(0, materials.Count)]);
    }

    private void SpawnObject(GameObject prefab)
    {
        Vector3 randomPos = GetRandomPositionInCircle();
        GameObject obj = Instantiate(prefab, randomPos, Random.rotation);
        obj.transform.SetParent(environmentTransform);
        obj.transform.localScale = new Vector3(obj.transform.localScale.x * objectScale, obj.transform.localScale.y * objectScale, obj.transform.localScale.z * objectScale);
        SimpleObject newObjectScript = obj.GetComponent<SimpleObject>();
        newObjectScript.SetMeshObject(meshes[Random.Range(0, meshes.Count)]);
        newObjectScript.SetMaterial(materials[Random.Range(0, materials.Count)]);
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        Vector3 initialVelocity = Random.onUnitSphere * Random.Range(speeds[0], speeds[1]);
        rb.linearVelocity = initialVelocity;
        ConstantSpeed constantSpeed = obj.AddComponent<ConstantSpeed>();
        constantSpeed.targetSpeed = initialVelocity.magnitude;
    }

    private void SpawnSearchObject(GameObject searchObjectPrefab)
    {
        Vector3 randomPos = GetRandomPositionInCircle();
        GameObject obj = Instantiate(searchObjectPrefab, randomPos, Random.rotation);
        obj.transform.SetParent(environmentTransform);
        searchObject = obj.transform;
        obj.transform.localScale = new Vector3(obj.transform.localScale.x * objectScale, obj.transform.localScale.y * objectScale, obj.transform.localScale.z * objectScale);
        SearchObject newObjectScript = obj.GetComponent<SearchObject>();
        newObjectScript.SetMaterial(materials[Random.Range(0, materials.Count)]);
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        Vector3 initialVelocity = Random.onUnitSphere * Random.Range(speeds[0], speeds[1]);
        rb.linearVelocity = initialVelocity;
        ConstantSpeed constantSpeed = obj.AddComponent<ConstantSpeed>();
        constantSpeed.targetSpeed = initialVelocity.magnitude;
    }

    private void SpawnFixedSearchObject(GameObject searchObjectPrefab, Vector3 position)
    {
        // fixed for tuning
        Vector3 randomPos = position;
        GameObject obj = Instantiate(searchObjectPrefab, randomPos, Random.rotation);
        obj.transform.SetParent(environmentTransform);
        searchObject = obj.transform;
        obj.transform.localScale = new Vector3(obj.transform.localScale.x * objectScale, obj.transform.localScale.y * objectScale, obj.transform.localScale.z * objectScale);
        SearchObject newObjectScript = obj.GetComponent<SearchObject>();
        newObjectScript.SetMaterial(materials[Random.Range(0, materials.Count)]);
    }

    Vector3 GetRandomPositionInCircle()
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        float radius = Random.Range(radii[0], radii[1]);
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;
        float y = Random.Range(heights[0], heights[1]);

        return new Vector3(x, y, z);
    }
}