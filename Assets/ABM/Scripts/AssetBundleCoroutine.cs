using System.Collections;
using UnityEngine;

namespace ABM
{
    class AssetBundleCoroutine : MonoBehaviour
    {
        private static AssetBundleCoroutine instance;
        public static AssetBundleCoroutine Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType(typeof(AssetBundleCoroutine)) as AssetBundleCoroutine;

                    if (instance == null)
                        instance = new GameObject(nameof(AssetBundleCoroutine)).AddComponent<AssetBundleCoroutine>();
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            instance = null;
        }

        public static void Coroutine(IEnumerator coroutine)
        {
            Instance.StartCoroutine(coroutine);
        }
    }
}
