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
}
