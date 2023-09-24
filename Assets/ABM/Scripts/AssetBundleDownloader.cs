using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ABM
{
    class AssetBundleDownloader : ICommandHandler<AssetBundleDownloadCommand>
    {
        private static readonly int MaxRetryCount = 3;
        private static readonly float RetryWaitPeriod = 1f;
        private static readonly int MaxSimultaneousDownloads = 4;
        private static readonly Hash128 DefaultHash = default;
        private static readonly long[] RetryOnErrors = { 503 /*Temporary Server Error*/ };

        private string baseUri = default;
        private bool cachingDisabled = false;
        private int activeDownloads = 0;
        private Action<IEnumerator> coroutine = default;
        private Queue<IEnumerator> downloads = new Queue<IEnumerator>();

        public AssetBundleDownloader(string baseUri)
        {
            this.baseUri = baseUri;

#if UNITY_EDITOR
            if (Application.isPlaying == false)
                coroutine = AssetBundleCoroutineEditor.Coroutine;
            else
#endif
                coroutine = AssetBundleCoroutine.Coroutine;

            if (this.baseUri.EndsWith("/") == false)
                this.baseUri += "/";
        }

        public void Handle(AssetBundleDownloadCommand command)
        {
            OnHandle(Download(command, 0));
        }

        private void OnHandle(IEnumerator download)
        {
            if (activeDownloads < MaxSimultaneousDownloads)
            {
                activeDownloads++;
                coroutine(download);
            }
            else
            {
                downloads.Enqueue(download);
            }
        }

        private IEnumerator Download(AssetBundleDownloadCommand command, int retryCount)
        {
            UnityWebRequest request;
            var uri = baseUri + command.BundleName;

            if (cachingDisabled == true || (command.Version <= 0 && command.Hash == DefaultHash))
            {
                Debug.LogFormat("GetAssetBundle [{0}].", uri);
                request = UnityWebRequestAssetBundle.GetAssetBundle(uri);
            }
            else if (command.Hash == DefaultHash)
            {
                Debug.LogFormat("GetAssetBundle [{0}] v[{1}] [{2}].", Caching.IsVersionCached(uri, new Hash128(0, 0, 0, command.Version)) ? "cached" : "uncached", command.Version, uri);
                request = UnityWebRequestAssetBundle.GetAssetBundle(uri, command.Version, 0);
            }
            else
            {
                Debug.LogFormat("GetAssetBundle [{0}] #[{1}] [{2}].", Caching.IsVersionCached(uri, command.Hash) ? "cached" : "uncached", command.Hash, uri);
                request = UnityWebRequestAssetBundle.GetAssetBundle(uri, command.Hash, 0);
            }

            yield return request.SendWebRequest();
            //request.SendWebRequest();
            //while (request.isDone == false)
            //{
            //    Debug.Log($"Downloading: {request.downloadProgress}");
            //    yield return null;
            //}

            var errorNetwork = request.result == UnityWebRequest.Result.ConnectionError;
            var errorHttp = request.result == UnityWebRequest.Result.ProtocolError;

            AssetBundle bundle = null;

            if (errorHttp)
            {
                Debug.LogError($"Error downloading [{uri}]: [{request.responseCode}] [{request.error}]");

                if (retryCount < MaxRetryCount && RetryOnErrors.Contains(request.responseCode))
                {
                    Debug.LogWarning($"Retrying [{uri}] in [{RetryWaitPeriod}] seconds...");
                    request.Dispose();
                    activeDownloads--;

                    yield return new WaitForSeconds(RetryWaitPeriod);
                    OnHandle(Download(command, retryCount + 1));
                    yield break;
                }
            }
            else if (errorNetwork)
            {
                Debug.LogError($"Error downloading [{uri}]: [{request.error}]");
            }
            else
            {
                try
                {
                    bundle = DownloadHandlerAssetBundle.GetContent(request);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error processing downloaded bundle, exception follows...");
                    Debug.LogException(ex);
                }
            }

            if (errorNetwork == false && errorHttp == false && string.IsNullOrEmpty(request.error) && bundle == null)
            {
                if (cachingDisabled)
                {
                    Debug.LogWarning($"There was no error downloading [{uri}] but the bundle is null. Caching has already been disabled, not sure there's anything else that can be done.  Returning...");
                }
                else
                {
                    Debug.LogWarning($"There was no error downloading [{uri}] but the bundle is null. Assuming there's something wrong with the cache folder, retrying with cache disabled now and for future requests...");

                    cachingDisabled = true;
                    request.Dispose();
                    activeDownloads--;

                    yield return new WaitForSeconds(RetryWaitPeriod);
                    OnHandle(Download(command, retryCount + 1));
                    yield break;
                }
            }

            try
            {
                command.OnComplete(bundle);
            }
            finally
            {
                request.Dispose();
                activeDownloads--;

                if (downloads.Count > 0)
                    OnHandle(downloads.Dequeue());
            }
        }
    }
}
