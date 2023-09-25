using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ABM;

//[ExecuteInEditMode]
public class Example2 : MonoBehaviour
{
    private void Awake()
    {
        Application.runInBackground = true;
    }

    private IEnumerator Start()
    {
        //Caching.ClearCache();

        var abm = AssetBundleManager.i
            //.Initialize("https://www.example.com/AssetBundles")
            //.UseStreamingAssets()
            .UseSimulation();

        var loadAsync = abm.Load();
        yield return loadAsync;
        if (loadAsync.Success)
        {
            var loadAssetAsync = abm.LoadAsset("scenes");
            yield return loadAssetAsync;
            if (loadAssetAsync.Success)
            {
                yield return SceneManager.LoadSceneAsync("Example3");
                abm.ReleaseAsset(loadAssetAsync.AssetBundle);
            }
        }
    }
}
