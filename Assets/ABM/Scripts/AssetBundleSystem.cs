using UnityEngine;

namespace ABM
{
    public sealed partial class AssetBundleManager : System<AssetBundleManager>
    {
        protected override void Awake()
        {
            base.Awake();

            platformName = AssetBundleTools.GetPlatformName();
            Application.runInBackground = true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Dispose();
        }
    }

    public class System<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance = null;
        private static object sync = new object();

        public static T Instance
        {
            get
            {
                lock (sync)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType(typeof(T)) as T;

                        if (instance == null)
                            instance = new GameObject(typeof(T).FullName).AddComponent<T>();
                    }
                    return instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            instance = null;
        }

        public static void Destroy()
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
                instance = null;
            }
        }

        public static bool IsNull => (instance == null);
        public static T i => Instance;
    }
}
