using System;
using UnityEngine;

namespace ABM
{
    interface ICommandHandler<in T>
    {
        void Handle(T command);
    }

    class AssetBundleDownloadCommand
    {
        public string BundleName;
        public uint Version;
        public Hash128 Hash;
        public Action<AssetBundle> OnComplete;
    }
}
