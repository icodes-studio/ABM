#if NET_4_6 || NET_STANDARD_2_0
using UnityEngine;
using UnityEngine.UI;
using ABM;

//[ExecuteInEditMode]
public class Example3 : MonoBehaviour
{
    private static AssetBundleManager abm;

    private void Awake()
    {
        Application.runInBackground = true;
    }

    private async void Start()
    {
        //Caching.ClearCache();
        abm = new AssetBundleManager();
        abm//.Initialize("https://YourCDN/AssetBundles")
           //.UseStreamingAssets()
           .UseSimulation()
           .Load();

        var loadAsync = abm.LoadAsync();
        await loadAsync;
        if (loadAsync.Result)
        {
            var loadAssetAsync = abm.LoadAssetAsync("sprites");
            await loadAssetAsync;
            if (loadAssetAsync.Result != null)
            {
                var image = GetComponentInChildren<Image>();
                //image.sprite = loadAssetAsync.Result.LoadAsset<Sprite>("sprite");
                //image.sprite = loadAssetAsync.Result.LoadAsset<Sprite>("assets/ABM/Demo/Sprites/sprite.png");
                image.sprite = loadAssetAsync.Result.LoadAsset<Sprite>("assets/ABM/Demo/Sprites/Test/sprite.png");
                abm.ReleaseAsset(loadAssetAsync.Result);
            }
        }
    }

    private void OnDestroy()
    {
        abm?.Dispose();
    }
}
#endif