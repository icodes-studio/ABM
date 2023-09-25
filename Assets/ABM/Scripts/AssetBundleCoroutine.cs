using System.Collections;

namespace ABM
{
    class AssetBundleCoroutine : System<AssetBundleCoroutine>
    {
        public static void Coroutine(IEnumerator coroutine) => 
            Instance.StartCoroutine(coroutine);
    }
}
