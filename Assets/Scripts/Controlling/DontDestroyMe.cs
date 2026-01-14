using UnityEngine;

public class DontDestroyMe : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
