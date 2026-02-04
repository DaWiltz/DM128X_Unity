using UnityEngine;
using UnityEngine.EventSystems;

public class GlobalEventSystem : MonoBehaviour
{
    private static GlobalEventSystem instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
