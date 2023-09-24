using UnityEngine;
using ABM;

//[ExecuteInEditMode]
public class Example1 : MonoBehaviour
{
    private static AssetBundleManager abm;

    private void Awake()
    {
        Application.runInBackground = true;
    }

    private void Start()
    {
        //Caching.ClearCache();
        abm = new AssetBundleManager();
        abm//.Initialize("https://www.example.com/AssetBundles")
           //.UseStreamingAssets()
           .UseSimulation()
           .Load(success =>
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

    private void OnDestroy()
    {
        abm?.Dispose();
    }
}