#if NET_4_6 || NET_STANDARD_2_0
using UnityEngine;
using System.Threading.Tasks;

namespace ABM
{
    public sealed partial class AssetBundleManager
    {
        public Task<bool> LoadAsync()
        {
            return LoadAsync(AssetBundleTools.PlatformName, true);
        }

        public Task<bool> LoadAsync(string name, bool refresh)
        {
            var completionSource = new TaskCompletionSource<bool>();
            Load(name, refresh, result => completionSource.SetResult(result));
            return completionSource.Task;
        }

        public Task<AssetBundle> LoadAssetAsync(string name)
        {
            return LoadAssetAsync(name, DownloadSettings.UseCacheIfAvailable);
        }

        public Task<AssetBundle> LoadAssetAsync(string name, DownloadSettings downloadSettings)
        {
            var completionSource = new TaskCompletionSource<AssetBundle>();
            LoadAsset(name, downloadSettings, bundle => completionSource.SetResult(bundle));
            return completionSource.Task;
        }
    }
}
#endif