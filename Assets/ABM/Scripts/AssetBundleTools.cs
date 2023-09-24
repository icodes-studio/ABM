#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using UnityEngine;

namespace ABM
{
    public static class AssetBundleTools
    {
        public const string AssetFolder = "AssetBundles";

#if UNITY_EDITOR
        [MenuItem("AssetBundle/Build/Android", priority = 1)]
        public static void BuildAndroid()
        {
            Build(BuildTarget.Android);
        }

        [MenuItem("AssetBundle/Build/Windows", priority = 2)]
        public static void BuildWindows()
        {
            Build(BuildTarget.StandaloneWindows64);
        }

        [MenuItem("AssetBundle/Build/iOS", priority = 3)]
        public static void BuildIOS()
        {
            Build(BuildTarget.iOS);
        }

        [MenuItem("AssetBundle/Build/All", priority = 4)]
        public static void BuildAll()
        {
            BuildAndroid();
            BuildWindows();
            BuildIOS();
        }

        [MenuItem("AssetBundle/Show Bundle Reference", priority = 5)]
        public static void BundleList()
        {
            foreach (var bundle in AssetDatabase.GetAllAssetBundleNames())
            {
                foreach (var asset in AssetDatabase.GetAssetPathsFromAssetBundle(bundle))
                {
                    Debug.Log($"{bundle} / {asset}");
                }
            }
        }

        [MenuItem("AssetBundle/Copy To StreamingAssets", priority = 6)]
        public static void CopyToStreamingAssets()
        {
            var streaming = Path.Combine(Application.streamingAssetsPath, AssetFolder);
            ClearPath(streaming);
            CopyPath(AssetFolder, streaming);
            AssetDatabase.Refresh();
        }

        public static void Build(BuildTarget buildTarget)
        {
            var output = Path.Combine(AssetFolder, GetPlatformName(buildTarget));
            ClearPath(output);
            BuildPipeline.BuildAssetBundles(output, BuildAssetBundleOptions.None, buildTarget);
        }
#endif

        public static string GetPlatformName()
        {
#if UNITY_EDITOR
            return GetPlatformName(EditorUserBuildSettings.activeBuildTarget);
#else
			return GetPlatformName(Application.platform);
#endif
        }

#if UNITY_EDITOR
        public static string GetPlatformName(BuildTarget target) =>
            target switch
            {
                BuildTarget.Android             => "Android",
                BuildTarget.iOS                 => "iOS",
                BuildTarget.tvOS                => "tvOS",
                BuildTarget.WebGL               => "WebGL",
                BuildTarget.StandaloneWindows   => "StandaloneWindows",
                BuildTarget.StandaloneWindows64 => "StandaloneWindows",
                BuildTarget.StandaloneOSX       => "StandaloneOSX",
                BuildTarget.StandaloneLinux64   => "StandaloneLinux",
                BuildTarget.Switch              => "Switch",
                _                               => target.ToString()
            };
#endif

        public static string GetPlatformName(RuntimePlatform platform) =>
            platform switch
            {
                RuntimePlatform.Android         => "Android",
                RuntimePlatform.IPhonePlayer    => "iOS",
                RuntimePlatform.tvOS            => "tvOS",
                RuntimePlatform.WebGLPlayer     => "WebGL",
                RuntimePlatform.WindowsPlayer   => "StandaloneWindows",
                RuntimePlatform.OSXPlayer       => "StandaloneOSX",
                RuntimePlatform.LinuxPlayer     => "StandaloneLinux",
                RuntimePlatform.Switch          => "Switch",
                _                               => platform.ToString()
            };

        public static void CopyPath(string source, string target)
        {
            if (Directory.Exists(target) == false)
                Directory.CreateDirectory(target);

            var files = Directory.GetFiles(source);
            var directories = Directory.GetDirectories(source);

            foreach (var file in files)
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)));

            foreach (var directory in directories)
                CopyPath(directory, Path.Combine(target, Path.GetFileName(directory)));
        }

        public static void ClearPath(string path)
        {
            if (Directory.Exists(path))
            {
                var dirInfo = new DirectoryInfo(path);

                foreach (var file in dirInfo.GetFiles())
                    file.Delete();

                foreach (var dir in dirInfo.GetDirectories())
                    dir.Delete(true);
            }
            else
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
