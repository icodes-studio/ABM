#if NET_4_6 || NET_STANDARD_2_0
using UnityEngine;
using System.Threading.Tasks;

namespace ABM
{
    public sealed partial class AssetBundleManager
    {
        public Task<bool> LoadAsync()
        {
            return LoadAsync(platformName, true);
        }

        public Task<bool> LoadAsync(string manifestName, bool refresh)
        {
            var completionSource = new TaskCompletionSource<bool>();
            Load(manifestName, refresh, result => completionSource.SetResult(result));
            return completionSource.Task;
        }

        public Task<AssetBundle> LoadAssetAsync(string bundleName)
        {
            return LoadAssetAsync(bundleName, DownloadSettings.UseCacheIfAvailable);
        }

        public Task<AssetBundle> LoadAssetAsync(string bundleName, DownloadSettings downloadSettings)
        {
            var completionSource = new TaskCompletionSource<AssetBundle>();
            LoadAsset(bundleName, downloadSettings, bundle => completionSource.SetResult(bundle));
            return completionSource.Task;
        }
    }
}
#endif