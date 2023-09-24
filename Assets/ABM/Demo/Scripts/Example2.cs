using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ABM;

//[ExecuteInEditMode]
public class Example2 : MonoBehaviour
{
    private static AssetBundleManager abm;

    private void Awake()
    {
        Application.runInBackground = true;
    }

    private IEnumerator Start()
    {
        //Caching.ClearCache();
        abm = new AssetBundleManager();
        var loadAsync = abm
            //.Initialize("https://YourCDN/AssetBundles")
            //.UseStreamingAssets()
            .UseSimulation()
            .Load();

        yield return loadAsync;
        if (loadAsync.Success)
        {
            var loadAssetAsync = abm.LoadAsset("scenes");
            yield return loadAssetAsync;
            if (loadAssetAsync.Success)
            {
                yield return SceneManager.LoadSceneAsync("Example3", LoadSceneMode.Additive);
                abm.ReleaseAsset(loadAssetAsync.AssetBundle);
            }
        }
    }

    private void OnDestroy()
    {
        abm?.Dispose();
    }
}
