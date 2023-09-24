# ABM

_**A**sset **B**undle **M**anager_


　

## # Environment

- **Developed & tested with**
    - Unity 2022.3.4f1
    - Unity 2021.3.29f1
    - Unity 2020.3.39f1


　

## # Gettting started

- **Initializing**
    - First, you need to build your bundles.
        > ![](https://github.com/icodes-studio/wiki/blob/main/STUDY%2BRND/Unity3D/AssetBundles/Assets/abm-0.png)
    - By default, it puts bundles in ***PROJECT\AssetBundles\PLATFORM***.
        > ![](https://github.com/icodes-studio/wiki/blob/main/STUDY%2BRND/Unity3D/AssetBundles/Assets/abm-1.png)
    - Once the bundles are built, you can start accessing them with the manager.
    - When you are done testing your bundles you need to upload them to a server. 
    - They can go anywhere in the server as long as they are contained in a PLATFORM folder. 
    - For example, builds for iOS bundles should be accessable from http://www.example.com/AssetBundles/iOS. 
    - The full list of supported targets can be found in [***AssetBundleTools.cs.***](https://github.com/icodes-studio/ABM/blob/main/Assets/ABM/Scripts/AssetBundleTools.cs)


　

## # Example #1

- **using a callback**
    - *https://github.com/icodes-studio/ABM/blob/main/Assets/ABM/Demo/Scripts/Example1.cs*
    - ***UseSimulation()*** configures ABM to use the default folder structure to retrieve bundles.
    - This convenience means you don't have to upload your bundles to a remote server in order to test them.
    - You can use your local files instead.
        ```csharp
        using UnityEngine;
        using ABM;

        public class Example1 : MonoBehaviour
        {
            private static AssetBundleManager abm;

            private void Start()
            {
                abm = new AssetBundleManager();
                abm.Initialize("http://www.example.com/AssetBundles")
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
        ```
    - The ***Initialize(...)*** function configures ABM to point to a remote server that contains your bundles.
    - Calling ***Load(...)*** causes ABM to download the manifest file for your bundles. 
    - Once this file is downloaded and processed you are ready to begin downloading bundles.


　

## # Example #2

- **using a Coroutine**
    - *https://github.com/icodes-studio/ABM/blob/main/Assets/ABM/Demo/Scripts/Example2.cs*
    - If you prefer to use a ***coroutine*** instead of a callback
        ```csharp
        using System.Collections;
        using UnityEngine;
        using UnityEngine.SceneManagement;
        using ABM;

        public class Example2 : MonoBehaviour
        {
            private static AssetBundleManager abm;

            private IEnumerator Start()
            {
                abm = new AssetBundleManager();
                var loadAsync = abm
                    .Initialize("http://icoder.example.com/AssetBundles")
                    .UseSimulation()
                    .Load();

                yield return loadAsync;
                if (loadAsync.Success)
                {
                    var loadAssetAsync = abm.LoadAsset("scenes");
                    yield return loadAssetAsync;
                    if (loadAssetAsync.Success)
                    {
                        yield return SceneManager.LoadSceneAsync("test", LoadSceneMode.Additive);
                        abm.ReleaseAsset(loadAssetAsync.AssetBundle);
                    }
                }
            }

            private void OnDestroy()
            {
                abm?.Dispose();
            }
        }
        ```


　

## # Example #3

- **using a Treading.Task**
    - *https://github.com/icodes-studio/ABM/blob/main/Assets/ABM/Demo/Scripts/Example3.cs*
    - If you prefer to use a ***Threading.Task*** instead of a coroutine
        ```csharp
        #if NET_4_6 || NET_STANDARD_2_0
        using UnityEngine;
        using UnityEngine.UI;
        using ABM;

        public class Example3 : MonoBehaviour
        {
            private static AssetBundleManager abm;

            private async void Start()
            {
                abm = new AssetBundleManager();
                abm.Initialize("http://www.example.com/AssetBundles")
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
                        image.sprite = loadAssetAsync.Result.LoadAsset<Sprite>("sprite");
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
        ```
    - Threading.Task works only in ***.net 4.x*** or ***.net standard 2.0 ↑*** environment.
        > ![](https://github.com/icodes-studio/wiki/blob/main/STUDY%2BRND/Unity3D/AssetBundles/Assets/abm-2.png)


　

## # StreamingAssets

- **Preparation**
    - ABM supports pre-caching your bundles with the use of the ***StreamingAssets*** folder in Unity.
    - Once your bundles are built you can copy the manifest and any number of bundles to the ***StreamingAsests\AssetBundles\PLATFORM*** folder.
    - For example if you wanted to pre-cache the ***SomeBundle*** iOS bundles you would have a structure like:
        ```
        PROJECT
        \Assets
            \StreamingAssets
                \AssetBundles
                    \iOS
                        \iOS
                        \iOS.manifest
                        \SomeBundle
                        \SomeBundle.manifest
        ```
    - You can simply copy them using the menu below.
        > ![](https://github.com/icodes-studio/wiki/blob/main/STUDY%2BRND/Unity3D/AssetBundles/Assets/abm-3.png)
- **Loading strategies**
    - When you make a ***LoadAsset(...)*** call ABM will check to see if that bundle exists in the StreamingAssets folder first.
    - And use it if its hash matches the hash of the remote server.
    - If the file does not exist OR the hash is different then the remote bundle is used.
    - You can change this behaviour when initializing ABM by changing the prioritization strategy:
        ```csharp
        abm.SetPrioritizationStrategy(PrioritizationStrategy.StreamingAssets);
        ```
    - This will tell ABM to always use the StreamingAssets bundle if it exists.
    - If the bundle doesn't exist in StreamingAssets the remote one will be used.


　

## # Cleanup

- **There are two patterns you should follow when using ABM.**
    - The first is to always unload the bundle when you are finished with it:
        ```csharp
        abm.ReleaseAsset(bundle);
        ```
- **If no other scripts are using this bundle it will be unloaded from memory.**
    - Likewise, when you are completely done with ABM (maybe because you're switching scenes and don't need the bundles anymore),
    - You can dispose of it:
        ```csharp
        abm.Dispose();
        ```
    - This will force ALL bundles (and their objects) to be unloaded.


　

## # Trouble shootings

- ***Problem #1***
    - Error while downloading Asset Bundle: Failed to decompress data for the AssetBundle
        > ![](https://github.com/icodes-studio/wiki/blob/main/STUDY%2BRND/Unity3D/AssetBundles/Assets/troubleshooting-0.png)
    - If you are using FIlezilla to upload your asset bundles. Try inside of FIlezilla ***Transfers > File Type > Binary***. then delete your build and asset bundles and re-upload them.
        > ![](https://github.com/icodes-studio/wiki/blob/main/STUDY%2BRND/Unity3D/AssetBundles/Assets/troubleshooting-1.png)
    - ***See Also***
        - *https://blog.naver.com/chic1007/221734272760*
        - *https://discussions.unity.com/t/error-failed-to-decompress-data-for-the-assetbundle-when-starting-a-web-gl-game-on-a-hosted-server/158953*


　

　

# API References

- **Initialize**: Initializes the base-uri used for AssetBundle calls.
- **Load**: Load the platform(entry-point) manifest file.
- **LoadAsset**: Downloads an AssetBundle or returns a cached AssetBundle if it has already been downloaded.
- **ReleaseAsset**: Unloads an AssetBundle.
- **IsVersionCached**: Check to see if a specific asset bundle is cached or needs to be downloaded.
- **Dispose**: Cleans up all downloaded bundles.


　

## # Initialize
*public AssetBundleManager Initialize(string uri)*

- **Description**
    - Initializes the base-uri used for AssetBundle calls.
    - The manager will load the manifest file located at base-url/[PlatformName].
- **Parameters**
    - **string uri**: List of base-uris.


　

## # Initialize
*public AssetBundleManager Initialize(string[] uris)*

- **Description**
    - Initializes a list of base-uris used for AssetBundle calls.
    - The manager will load the manifest file located at base-url/[PlatformName].
    - If access to one uri is denied, the manager will try to access the next uri.
- **Parameters**
    **string[] uris**: List of base-uris.


　

## # Load
*public void Load(Action\<bool\> callback)*

- **Description**
    - Load the platform(entry-point) manifest file.
    - Downloads the AssetBundle manifest and prepares the system for bundle management.
    - Uses the platform name as the manifest name.
- **Parameters**
    - **Action\<bool\> callback**: Called when the loading is complete.


　

## # Load
*public void Load(string manifestName, bool refresh, Action\<bool\> callback)*

- **Description**
    - Load the platform(entry-point) manifest file.
- **Parameters**
    - **string manifestName**: The name of the manifest file to download.
    - **bool refresh**: Always try to download a new manifest even if one has already been cached.
    - **Action\<bool\> callback**: Called when the loading is complete.


　

## # LoadAsset
*public void LoadAsset(string bundleName, Action\<AssetBundle\> callback)*

- **Description**
    - Downloads an AssetBundle or returns a cached AssetBundle if it has already been downloaded.
    - Remember to call ReleaseAsset(AssetBundle, bool) for every bundle you download once you are done with it.
- **Parameters**
    - **string bundleName**: Name of the bundle to download.
    - **Action\<AssetBundle\> callback**: Action to perform when the bundle has been successfully downloaded.


　

## # LoadAsset
*public void LoadAsset(string bundleName, Action\<AssetBundle\> callback, DownloadSettings downloadSettings)*

- **Description**
    - Downloads an AssetBundle or returns a cached AssetBundle if it has already been downloaded.
    - Remember to call ReleaseAsset(AssetBundle, bool) for every bundle you download once you are done with it.
- **Parameters**
    - **string bundleName**: Name of the bundle to download.
    - **Action\<AssetBundle\> callback**: Action to perform when the bundle has been successfully downloaded.
    - **DownloadSettings downloadSettings**: 
        - Tell the function to use a previously downloaded version of the bundle if available.
        - If the bundle is currently "active" (it has not been unloaded) then the active bundle will be used regardless of this setting.
        - If it's important that a new version is downloaded then be sure it isn't active.


　

## # ReleaseAsset
*public void ReleaseAsset(AssetBundle bundle)*

- **Description**
    - Unloads an AssetBundle.
    - Objects that were loaded from this bundle will need to be manually destroyed.
- **Parameters**
    - **AssetBundle bundle**: Bundle to unload.


　

## # ReleaseAsset
*public void ReleaseAsset(AssetBundle bundle, bool unloadAllLoadedObjects)*

- **Description**
    - Unloads an AssetBundle.
- **Parameters**
    - **AssetBundle bundle**: Bundle to unload.
    - **bool unloadAllLoadedObjects**: 
        - When true, all objects that were loaded from this bundle will be destroyed as well.
        - If there are game objects in your scene referencing those assets, the references to them will become missing.


　

## # ReleaseAsset
*public void ReleaseAsset(AssetBundle bundle, bool unloadAllLoadedObjects, bool force)*

- **Description**
    - Unloads an AssetBundle.
- **Parameters**
    - **AssetBundle bundle**: Bundle to unload.
    - **bool unloadAllLoadedObjects**: 
        - When true, all objects that were loaded from this bundle will be destroyed as well.
        - If there are game objects in your scene referencing those assets, the references to them will become missing.
    - **bool force**: Unload the bundle even if ABM believes there are other dependencies on it.


　

## # Manifest
*public AssetBundleManifest Manifest*

- **Description**
    - Returns object for the platform(entry-point) manifest.


　

## # ManifestType
*public ManifestType ManifestType*

- **Description**
    - Returns the AssetBundle manifest type.
        - None: an error or undefined condition.
        - Remote: Download bundles remotely.
        - LocalCached: Bundle loaded from local cache.
        - StreamingAssets: Bundle loaded from StreamingAssets folder.


　

## # UseStreamingAssets
*public AssetBundleManager UseStreamingAssets()*

- **Description**
    - Loading from Unity's StreamingAsset folder.
    - Sets the base-uri used for AssetBundle calls to the StreamingAssets folder.


　

## # UseSimulation
*public AssetBundleManager UseSimulation()*

- **Description**
    - Loading from the Simulation folder.
    - Sets the base-uri used for AssetBundle calls to the one created by the AssetBundleBuilder when the bundles are built.
    - Used for easier testing in the editor.


　

## # SetPrioritizationStrategy
*public AssetBundleManager SetPrioritizationStrategy(PrioritizationStrategy strategy)*

- **Description**
    - Changes the strategy used to determine what should happen when an asset bundle exists in both the StreamingAssets and the remote server.
    - The default is to prioritize the remote asset over the StreamingAssets folder
- **Parameters**
    - **PrioritizationStrategy strategy**
        - Remote:
        - StreamingAssets:


　

## # IsVersionCached
*public bool IsVersionCached(string bundleName)*

- **Description**
    - Check to see if a specific asset bundle is cached or needs to be downloaded.


　

## # Dispose

- **Description**
    - Cleans up all downloaded bundles.

