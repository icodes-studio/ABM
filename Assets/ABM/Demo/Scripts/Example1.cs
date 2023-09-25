using UnityEngine;
using ABM;

//[ExecuteInEditMode]
public class Example1 : MonoBehaviour
{
    private void Start()
    {
        //Caching.ClearCache();
        var abm = AssetBundleManager.i
            //.Initialize("https://www.example.com/AssetBundles")
            //.UseStreamingAssets()
            .UseSimulation();

        abm.Load(success =>
        {
            if (success)
            {
                abm.LoadAsset("prefabs", bundle =>
                {
                    if (bundle != null)
                    {
                        Instantiate(bundle.LoadAsset<GameObject>("button"), transform);
                        abm.ReleaseAsset(bundle);
                    }
                });
            }
        });
    }
}