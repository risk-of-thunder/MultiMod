
using MultiMod.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MultiMod.Editor
{
    public class AsmDef
    {
        public string name;
        public string[] references;
        public string[] optionalUnityReferences;
        public bool allowUnsafeCode;
        public bool overrideReferences;
        public string[] precompiledReferences;
        public bool autoReferenced;
        public string[] defineConstraints;
    }
    public class Export
    {
        private readonly ExportSettings settings;
        private readonly string prefix;
        private readonly List<string> asmDefPaths;
        private readonly List<string> assetPaths;
        private readonly string tempModDirectory;
        private readonly string modDirectory;

        public Export(ExportSettings settings)
        {
            this.settings = settings;
            this.prefix = $"{settings.name}-{settings.version}";
            this.asmDefPaths = AssetUtility.GetAssets("t:AssemblyDefinitionAsset");
            this.assetPaths = AssetUtility.GetAssets("t:prefab t:scriptableobject");
            this.tempModDirectory = Path.Combine("Temp", settings.name);
            this.modDirectory = Path.Combine(settings.outputDirectory, settings.name);
        }

        public void SetAssetBundle(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath);
            importer.assetBundleName = settings.name;
            importer.assetBundleVariant = "assets";
        }

        private void CopyAll(string sourceDirectory, string targetDirectory)
        {
            Directory.CreateDirectory(targetDirectory);

            foreach (string file in Directory.GetFiles(sourceDirectory))
            {
                string fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(targetDirectory, fileName), true);
            }

            foreach (string subDirectory in Directory.GetDirectories(sourceDirectory))
            {
                string targetSubDirectory = Path.Combine(targetDirectory, Path.GetFileName(subDirectory));
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
            foreach (var asmDefPath in asmDefPaths)
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

        private void ExportModAssets()
        {
            assetPaths.ForEach(SetAssetBundle);
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
                settings.name,
                settings.author,
                settings.description,
                settings.version,
                Application.unityVersion,
                ModPlatform.Windows,
                ModContent.Assets & ModContent.Scenes);

            ModInfo.Save(Path.Combine(tempModDirectory, settings.name + ".info"), modInfo);
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
            } catch (Exception e)
            {
                LogUtility.LogWarning("There was an issue while copying the mod to the output folder. ");
                LogUtility.LogWarning(e.Message);
            }
        }

        public void Run()
        {
            LogUtility.LogDebug($"Starting export of {settings.name}");
            CreateTempDirectory();
            ExportModAssemblies();
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
