using System;
using System.Collections.Generic;
using System.IO;
using MultiMod.Shared;
using UnityEditor;
using UnityEngine;

namespace MultiMod.Editor
{
    public class Export
    {
        private readonly List<string> asmDefPaths;
        private readonly List<string> copyPaths;
        private readonly List<string> assetPaths;
        private readonly List<string> scenePaths;
        private readonly string modDirectory;
        private readonly string prefix;
        private readonly ExportSettings settings;
        private readonly string tempModDirectory;

        public Export(ExportSettings settings)
        {
            this.settings = settings;
            prefix = $"{settings.Name}-{settings.Version}";
            assetPaths = AssetUtility.GetAssets("t:prefab t:scriptableobject");
            scenePaths = AssetUtility.GetAssets("t:scene");
            tempModDirectory = Path.Combine("Temp", settings.Name);
            modDirectory = Path.Combine(settings.OutputDirectory, settings.Name);
        }

        public void SetAssetBundle(string assetPath, string variant = "assets")
        {
            var importer = AssetImporter.GetAtPath(assetPath);
            importer.assetBundleName = settings.Name;
            importer.assetBundleVariant = variant;
        }

        private void CopyAll(string sourceDirectory, string targetDirectory)
        {
            Directory.CreateDirectory(targetDirectory);

            foreach (var file in Directory.GetFiles(sourceDirectory))
            {
                var fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(targetDirectory, fileName), true);
            }

            foreach (var subDirectory in Directory.GetDirectories(sourceDirectory))
            {
                var targetSubDirectory = Path.Combine(targetDirectory, Path.GetFileName(subDirectory));
                CopyAll(subDirectory, targetSubDirectory);
            }
        }

        private void CreateTempDirectory()
        {
            if (Directory.Exists(tempModDirectory))
                Directory.Delete(tempModDirectory, true);
            LogUtility.LogDebug($"Creating build directory: {tempModDirectory}");
            Directory.CreateDirectory(tempModDirectory);
        }

        private void ExportModAssemblies()
        {
            LogUtility.LogDebug("Exporting mod assemblies...");
            foreach (var asmDefPath in settings.Assemblies)
            {
                var json = File.ReadAllText(asmDefPath);
                var asmDef = JsonUtility.FromJson<AsmDef>(json);

                var modAsmPath = Path.Combine("Library", "ScriptAssemblies", $"{asmDef.name}.dll");

                if (!File.Exists(modAsmPath))
                {
                    LogUtility.LogError($"{asmDef.name} not found: {modAsmPath}");
                    continue;
                }

                LogUtility.LogDebug($" - {asmDef.name}");
                File.Copy(modAsmPath, Path.Combine(tempModDirectory, $"{asmDef.name}.dll"));
            }
        }

        private void ExportCopyAssets()
        {
            LogUtility.LogDebug("Exporting copy assets...");
            foreach (var path in settings.Artifacts)
            {
                var filename = Path.GetFileName(path);
                var destination = Path.Combine(tempModDirectory, filename);
                File.Copy(path, destination);
            }
        }

        private void ExportModAssets()
        {
            assetPaths.ForEach(s => SetAssetBundle(s));
            scenePaths.ForEach(s => SetAssetBundle(s, "scenes"));
            ModPlatform.Windows.GetBuildTargets().ForEach(target => {
                var platform = target.GetModPlatform().ToString();
                var subDir = Path.Combine(tempModDirectory, platform);
                Directory.CreateDirectory(subDir);
                Debug.Log($"Exporting assets for {platform} to: {subDir}");
                BuildPipeline.BuildAssetBundles(subDir, BuildAssetBundleOptions.None, target);
            });
        }

        private void SaveMetadata()
        {
            var modInfo = new ModInfo(
                settings.Name,
                settings.Author,
                settings.Description,
                settings.Version,
                Application.unityVersion,
                ModPlatform.Windows,
                ModContent.Assets & ModContent.Scenes,
                settings.ContentTypes);

            ModInfo.Save(Path.Combine(tempModDirectory, settings.Name + ".info"), modInfo);
        }

        private void CopyToOutput()
        {
            try
            {
                if (Directory.Exists(modDirectory))
                    Directory.Delete(modDirectory, true);

                Debug.Log($"Copying {tempModDirectory} => {modDirectory}");
                CopyAll(tempModDirectory, modDirectory);
                LogUtility.LogInfo($"Export completed: {modDirectory}");
            }
            catch (Exception e)
            {
                LogUtility.LogWarning("There was an issue while copying the mod to the output folder. ");
                LogUtility.LogWarning(e.Message);
            }
        }

        public void Run()
        {
            LogUtility.LogDebug($"Starting export of {settings.Name}");
            CreateTempDirectory();
            ExportModAssemblies();
            ExportCopyAssets();
            ExportModAssets();
            SaveMetadata();
            CopyToOutput();
        }

        public static void ExportMod(ExportSettings settings)
        {
            var exporter = new Export(settings);
            exporter.Run();
        }
    }
}