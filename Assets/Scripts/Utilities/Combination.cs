using UnityEngine;

class Combination
{
    public string sceneName;
    public Transform visionCatcherPrefab;

    public Combination(string sceneName, Transform visionCatcherPrefab)
    {
        this.sceneName = sceneName;
        this.visionCatcherPrefab = visionCatcherPrefab;
    }

    public override string ToString()
    {
        return sceneName + " / " + visionCatcherPrefab.GetComponent<VisionCatcher>().visionCatcherName;
    }
}
