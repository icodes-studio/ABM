using System.Collections;
using UnityEngine;

namespace ABM
{
    class AssetBundleDownloaderStreamingAsset : ICommandHandler<AssetBundleDownloadCommand>
    {
        private string manifestName;
        private string streamingAssetsPath;
        private DownloadStrategy downloadStrategy;
        private ICommandHandler<AssetBundleDownloadCommand> downloader;

        public AssetBundleDownloaderStreamingAsset(
            string manifestName,
            ICommandHandler<AssetBundleDownloadCommand> downloader,
            DownloadStrategy downloadStrategy)
        {
            this.downloader = downloader;
            this.manifestName = manifestName;
            this.downloadStrategy = downloadStrategy;

            streamingAssetsPath = $"{Application.streamingAssetsPath}/{AssetBundleTools.AssetFolder}/{AssetBundleTools.PlatformName}";
            var manifestPath = $"{streamingAssetsPath}/{AssetBundleTools.PlatformName}";
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
            AssetBundleCoroutine.Start(OnHandle(command));
        }

        private IEnumerator OnHandle(AssetBundleDownloadCommand command)
        {
            if (IsAvailableInStreamingAssets(command.Name, command.Hash))
            {
                var request = AssetBundle.LoadFromFileAsync(streamingAssetsPath + "/" + command.Name);
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
                else
                {
                    Debug.LogWarning($"StreamingAssets download failed for bundle [{command.Name}], switching to standard download.");
                }
            }

            downloader.Handle(command);
        }

        private bool IsAvailableInStreamingAssets(string name, Hash128 hash)
        {
            if (Manifest == null)
            {
                Debug.Log("StreamingAssets manifest is null, using standard download.");
                return false;
            }

            if (name == manifestName)
            {
                Debug.Log("Attempting to download manifest file, using standard download.");
                return false;
            }

            if (Manifest.GetAssetBundleHash(name) != hash && downloadStrategy != DownloadStrategy.StreamingAssets)
            {
                Debug.Log($"Hash for [{name}] does not match the one in StreamingAssets, using standard download.");
                return false;
            }

            Debug.Log($"Using StreamingAssets for bundle [{name}]");

            return true;
        }

        public AssetBundleManifest Manifest
        {
            get;
            private set;
        }
    }
}
