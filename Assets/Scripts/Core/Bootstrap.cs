using UnityEngine;

namespace LWR.Core
{
    // Simple singleton to persist across scenes without using any Find* APIs.
    public class Bootstrap : MonoBehaviour
    {
        private static Bootstrap _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
