using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace ABM
{
    public enum DownloadSettings
    {
        UseCacheIfAvailable,
        DoNotUseCache
    }

    public enum DownloadStrategy
    {
        Remote,
        StreamingAssets,
    }

    public enum ManifestType
    {
        None,
        Remote,
        LocalCached,
        StreamingAssets,
    }

    public sealed partial class AssetBundleManager : IDisposable
    {
        private const string ManifestDownloadKey = "@manifest download key";
        private const string ManifestVersionKey = "@manifest version key";

        public bool readyToLoad = false;
        private string[] baseUris = null;
        private DownloadStrategy downloadStrategy = DownloadStrategy.Remote;
        private IDictionary<string, BundleContainer> activeBundles = new Dictionary<string, BundleContainer>(StringComparer.OrdinalIgnoreCase);
        private IDictionary<string, DownloadCallback> downloadCallbacks = new Dictionary<string, DownloadCallback>(StringComparer.OrdinalIgnoreCase);
        private ICommandHandler<AssetBundleDownloadCommand> downloader = null;

        class DownloadCallback
        {
            public int References;
            public Action<AssetBundle> OnComplete;
            public DownloadCallback(Action<AssetBundle> callback)
            {
                References = 1;
                OnComplete = callback;
            }
        }

        class BundleContainer
        {
            public AssetBundle AssetBundle = null;
            public string[] Dependencies = null;
            public int References = 1;
        }

        public AssetBundleManifest Manifest
        {
            get; private set;
        }

        public ManifestType ManifestType
        {
            get; private set;
        }

        public AssetBundleManager Initialize(string uri)
        {
            return Initialize(new[] { uri });
        }

        public AssetBundleManager Initialize(string[] uris)
        {
            baseUris = new string[uris.Length];
            for (int i = 0; i < uris.Length; i++)
            {
                var builder = new StringBuilder(uris[i]);
                (!uris[i].EndsWith("/") ? builder.Append("/") : builder).Append(AssetBundleTools.PlatformName).Append("/");
                baseUris[i] = builder.ToString();
            }
            return this;
        }

        public AssetBundleManager UseSimulation()
        {
            return Initialize(new[] { $"file://{Application.dataPath}/../{AssetBundleTools.AssetFolder}/" });
        }

        public AssetBundleManager UseStreamingAssets()
        {
            return Initialize(new[] { 
#if UNITY_ANDROID
                $"{Application.streamingAssetsPath}/{AssetBundleTools.AssetFolder}/"
#else
                $"file:///{Application.streamingAssetsPath}/{AssetBundleTools.AssetFolder}/"
#endif
            });
        }

        public void Load(Action<bool> callback)
        {
            Load(AssetBundleTools.PlatformName, true, callback);
        }

        public void Load(string name, bool refresh, Action<bool> callback)
        {
            if (baseUris == null || baseUris.Length == 0)
                throw new Exception("You need to set the base uri before you can initialize.");

            LoadManifest(name, refresh, bundle => callback(bundle != null));
        }

        private void LoadManifest(string name, bool refresh, Action<AssetBundle> callback)
        {
            if (downloadCallbacks.TryGetValue(ManifestDownloadKey, out var downloadCallback))
            {
                downloadCallback.References++;
                downloadCallback.OnComplete += callback;
                return;
            }

            downloadCallbacks.Add(ManifestDownloadKey, new DownloadCallback(callback));
            ManifestType = ManifestType.Remote;

            uint manifestVersion = 1;

            if (refresh)
            {
                manifestVersion = (uint)PlayerPrefs.GetInt(ManifestVersionKey, 0) + 1;
                while (Caching.IsVersionCached(name, new Hash128(0, 0, 0, manifestVersion)))
                    manifestVersion++;
            }

            LoadManifest(name, manifestVersion, 0);
        }

        private void LoadManifest(string name, uint version, int index)
        {
            downloader = new AssetBundleDownloader(baseUris[index]);
            downloader = new AssetBundleDownloaderStreamingAsset(name, downloader, downloadStrategy);
            downloader.Handle(new AssetBundleDownloadCommand
            {
                Name = name,
                Version = version,
                OnComplete = bundle =>
                {
                    var maxUri = baseUris.Length - 1;
                    if (bundle == null && index < maxUri)
                    {
                        Debug.Log($"Unable to download manifest from [{baseUris[index]}], attempting [{baseUris[index + 1]}]");
                        LoadManifest(name, version, index + 1);
                    }
                    else if (bundle == null && index >= maxUri && version > 1 && ManifestType != ManifestType.LocalCached)
                    {
                        ManifestType = ManifestType.LocalCached;
                        Debug.Log($"Unable to download manifest, attempting to use one previously downloaded (version [{version}]).");
                        LoadManifest(name, version - 1, index);
                    }
                    else
                    {
                        OnLoadManifest(bundle, name, version);
                    }
                }
            });
        }

        private void OnLoadManifest(AssetBundle bundle, string name, uint version)
        {
            if (bundle == null)
            {
                Debug.LogError("AssetBundleManifest not found.");

                var streamingAssetsDownloader = downloader as AssetBundleDownloaderStreamingAsset;
                if (streamingAssetsDownloader != null)
                {
                    ManifestType = ManifestType.StreamingAssets;
                    Manifest = streamingAssetsDownloader.Manifest;

                    if (Manifest != null)
                        Debug.LogWarning("Falling back to streaming assets for bundle information.");
                }
            }
            else
            {
                Manifest = bundle.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
                PlayerPrefs.SetInt(ManifestVersionKey, (int)version);
                Caching.ClearOtherCachedVersions(name, new Hash128(0, 0, 0, version));
            }

            if (Manifest == null)
            {
                ManifestType = ManifestType.None;
            }
            else
            {
                readyToLoad = true;
            }

            var callback = downloadCallbacks[ManifestDownloadKey];
            downloadCallbacks.Remove(ManifestDownloadKey);
            callback.OnComplete?.Invoke(bundle);
            bundle?.Unload(false);
        }

        public void LoadAsset(string name, Action<AssetBundle> callback)
        {
            if (readyToLoad == false)
            {
                Debug.LogError("ABM must be loaded before you can get a bundle.");
                callback(null);
                return;
            }

            LoadAsset(name, DownloadSettings.UseCacheIfAvailable, callback);
        }

        public void LoadAsset(string name, DownloadSettings settings, Action<AssetBundle> callback)
        {
            if (readyToLoad == false)
            {
                Debug.LogError("ABM must be loaded before you can get a bundle.");
                callback(null);
                return;
            }

            if (activeBundles.TryGetValue(name, out var activeBundle))
            {
                activeBundle.References++;
                callback(activeBundle.AssetBundle);
                return;
            }

            if (downloadCallbacks.TryGetValue(name, out var downloadCallback))
            {
                downloadCallback.References++;
                downloadCallback.OnComplete += callback;
                return;
            }

            downloadCallbacks.Add(name, new DownloadCallback(callback));

            var mainBundle = new AssetBundleDownloadCommand
            {
                Name = name,
                Hash = (settings == DownloadSettings.UseCacheIfAvailable) ? Manifest.GetAssetBundleHash(name) : default,
                OnComplete = bundle => OnLoadAsset(bundle, name)
            };

            var dependencies = Manifest.GetDirectDependencies(name);
            var dependenciesToDownload = new List<string>();

            foreach (var dependency in dependencies)
            {
                if (activeBundles.TryGetValue(dependency, out activeBundle))
                {
                    activeBundle.References++;
                }
                else
                {
                    dependenciesToDownload.Add(dependency);
                }
            }

            if (dependenciesToDownload.Count > 0)
            {
                var dependencyCount = dependenciesToDownload.Count;
                foreach (var dependency in dependenciesToDownload)
                {
                    LoadAsset(dependency, bundle =>
                    {
                        if (--dependencyCount == 0)
                            downloader?.Handle(mainBundle);
                    });
                }
            }
            else
            {
                downloader?.Handle(mainBundle);
            }
        }

        private void OnLoadAsset(AssetBundle bundle, string name)
        {
            var callback = downloadCallbacks[name];
            downloadCallbacks.Remove(name);

            if (activeBundles.TryGetValue(name, out var activeBundle))
            {
                activeBundle.References++;
            }
            else
            {
                activeBundles.Add(name, new BundleContainer
                {
                    AssetBundle = bundle,
                    References = callback.References,
                    Dependencies = Manifest.GetDirectDependencies(name)
                });
            }

            callback.OnComplete?.Invoke(bundle);
        }

        public void ReleaseAsset(AssetBundle bundle)
        {
            if (bundle == null)
                return;

            ReleaseAsset(bundle.name, false, false);
        }

        public void ReleaseAsset(AssetBundle bundle, bool unloadAllLoadedObjects)
        {
            if (bundle == null)
                return;

            ReleaseAsset(bundle.name, unloadAllLoadedObjects, false);
        }

        public void ReleaseAsset(AssetBundle bundle, bool unloadAllLoadedObjects, bool force)
        {
            if (bundle == null)
                return;

            ReleaseAsset(bundle.name, unloadAllLoadedObjects, force);
        }

        public void ReleaseAsset(string name, bool unloadAllLoadedObjects, bool force)
        {
            if (activeBundles.TryGetValue(name, out var activeBundle) == false)
                return;

            if (force == true || --activeBundle.References <= 0)
            {
                activeBundle.AssetBundle?.Unload(unloadAllLoadedObjects);
                activeBundles.Remove(name);

                foreach (var dependency in activeBundle.Dependencies)
                    ReleaseAsset(dependency, unloadAllLoadedObjects, force);
            }
        }

        public AssetBundleManager SetDownloadStrategy(DownloadStrategy strategy)
        {
            downloadStrategy = strategy;
            return this;
        }

        public bool IsVersionCached(string name)
        {
            if (Manifest == null)
                return false;

            if (string.IsNullOrEmpty(name))
                return false;

            return Caching.IsVersionCached(name, Manifest.GetAssetBundleHash(name));
        }

        public void Dispose()
        {
            foreach (var cache in activeBundles.Values)
                cache.AssetBundle?.Unload(true);

            activeBundles.Clear();
        }
    }
}
