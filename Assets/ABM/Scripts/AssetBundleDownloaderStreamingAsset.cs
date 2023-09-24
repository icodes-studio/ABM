using System;
using System.Collections;
using UnityEngine;

namespace ABM
{
    class AssetBundleDownloaderStreamingAsset : ICommandHandler<AssetBundleDownloadCommand>
    {
        private string manifestName;
        private string platformName;
        private string streamingAssetsPath;
        private DownloadStrategy downloadStrategy;
        private ICommandHandler<AssetBundleDownloadCommand> downloader;
        private Action<IEnumerator> coroutine;

        public AssetBundleDownloaderStreamingAsset(
            string manifestName,
            string platformName,
            ICommandHandler<AssetBundleDownloadCommand> downloader,
            DownloadStrategy downloadStrategy)
        {
            this.downloader = downloader;
            this.manifestName = manifestName;
            this.platformName = platformName;
            this.downloadStrategy = downloadStrategy;

#if UNITY_EDITOR
            if (Application.isPlaying == false)
                coroutine = AssetBundleCoroutineEditor.Coroutine;
            else
#endif
                coroutine = AssetBundleCoroutine.Coroutine;

            streamingAssetsPath = $"{Application.streamingAssetsPath}/{AssetBundleTools.AssetFolder}/{platformName}";

            var manifestPath = $"{streamingAssetsPath}/{platformName}";
            var manifestBundle = AssetBundle.LoadFromFile(manifestPath);

            if (manifestBundle == null)
            {
                Debug.LogWarning($"Unable to retrieve manifest file [{manifestPath}] from StreamingAssets, disabling AssetBundleDownloaderStreamingAsset.");
            }
            else
            {
                Manifest = manifestBundle.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
                manifestBundle.Unload(false);
            }
        }

        public void Handle(AssetBundleDownloadCommand command)
        {
            coroutine(OnHandle(command));
        }

        private IEnumerator OnHandle(AssetBundleDownloadCommand command)
        {
            if (IsAvailableInStreamingAssets(command.BundleName, command.Hash))
            {
                var request = AssetBundle.LoadFromFileAsync(streamingAssetsPath + "/" + command.BundleName);
                yield return request;
                //while (request.isDone == false)
                //{
                //    Debug.Log($"Loading: {request.progress}");
                //    yield return null;
                //}

                if (request.assetBundle != null)
                {
                    command.OnComplete(request.assetBundle);
                    yield break;
                }

                Debug.LogWarning($"StreamingAssets download failed for bundle [{command.BundleName}], switching to standard download.");
            }

            downloader.Handle(command);
        }

        private bool IsAvailableInStreamingAssets(string bundleName, Hash128 hash)
        {
            if (Manifest == null)
            {
                Debug.Log("StreamingAssets manifest is null, using standard download.");
                return false;
            }

            if (bundleName == manifestName)
            {
                Debug.Log("Attempting to download manifest file, using standard download.");
                return false;
            }

            if (Manifest.GetAssetBundleHash(bundleName) != hash && downloadStrategy != DownloadStrategy.StreamingAssets)
            {
                Debug.Log($"Hash for [{bundleName}] does not match the one in StreamingAssets, using standard download.");
                return false;
            }

            Debug.Log($"Using StreamingAssets for bundle [{bundleName}]");

            return true;
        }

        public AssetBundleManifest Manifest
        {
            get; private set;
        }
    }
}
