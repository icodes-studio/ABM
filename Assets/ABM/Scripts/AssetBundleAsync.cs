using System;
using System.Collections;
using UnityEngine;

namespace ABM
{
    public sealed partial class AssetBundleManager
    {
        public AssetBundleLoadAsync Load()
        {
            return Load(AssetBundleTools.PlatformName, true);
        }

        public AssetBundleLoadAsync Load(string name, bool refresh)
        {
            if (baseUris == null || baseUris.Length == 0)
            {
                Debug.LogError("You need to set the base uri before you can initialize.");
                return null;
            }

            return new AssetBundleLoadAsync(name, refresh, LoadManifest);
        }

        public AssetBundleLoadAssetAsync LoadAsset(string name)
        {
            return LoadAsset(name, DownloadSettings.UseCacheIfAvailable);
        }

        public AssetBundleLoadAssetAsync LoadAsset(string name, DownloadSettings downloadSettings)
        {
            if (readyToLoad == false)
            {
                Debug.LogError("AssetBundleManager must be loaded before you can get a bundle.");
                return null;
            }

            return new AssetBundleLoadAssetAsync(name, downloadSettings, LoadAsset);
        }
    }

    public class AssetBundleLoadAsync : IEnumerator
    {
        public AssetBundleLoadAsync(string name, bool refresh, Action<string, bool, Action<AssetBundle>> loader)
        {
            IsDone = false;
            loader?.Invoke(name, refresh, OnAssetBundleManifestComplete);
        }

        private void OnAssetBundleManifestComplete(AssetBundle bundle)
        {
            Success = bundle != null;
            IsDone = true;
        }

        public bool MoveNext() => IsDone == false;
        public void Reset() => IsDone = false;
        public object Current => null;
        public bool Success { get; private set; }
        public bool IsDone { get; private set; }
    }

    public class AssetBundleLoadAssetAsync : IEnumerator
    {
        public AssetBundleLoadAssetAsync(string name, DownloadSettings downloadSettings, Action<string, DownloadSettings, Action<AssetBundle>> loader)
        {
            IsDone = false;
            loader?.Invoke(name, downloadSettings, OnAssetBundleComplete);
        }

        private void OnAssetBundleComplete(AssetBundle bundle)
        {
            AssetBundle = bundle;
            Success = bundle != null;
            IsDone = true;
        }

        public bool MoveNext() => IsDone == false;
        public void Reset() => IsDone = false;
        public object Current => null;
        public bool Success { get; private set; }
        public bool IsDone { get; private set; }
        public AssetBundle AssetBundle { get; private set; }
    }
}
