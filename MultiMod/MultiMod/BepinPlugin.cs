using BepInEx;
using On.RoR2;
using UnityEngine;
using Path = System.IO.Path;

using MultiMod.Shared;
using MultiMod.Interface;
using System.Linq;

namespace MultiMod
{
    //This is an example plugin that can be put in BepInEx/plugins/MultiModPlugin/MultiModPlugin.dll to test out.
    //Lets examine what each line of code is for:

    //This attribute specifies that we have a dependency on R2API.
    //You don't need this if you're not using R2API in your plugin, it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency("com.bepis.r2api")]

    //This attribute is required, and lists metadata for your plugin.
    //The GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config). I like to use the java package notation, which is "com.[your name here].[your plugin name here]"
    //The name is the name of the plugin that's displayed on load, and the version number just specifies what version the plugin is.
    [BepInPlugin("com.ldlework.multimod", "MultiModPlugin", "1.0")]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class BepinPlugin : BaseUnityPlugin
    {
        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            RoR2Application.UnitySystemConsoleRedirector.Redirect += orig => { };

            var searchDirectory = Path.Combine(Paths.PluginPath, "multimod/mods");
            var mm = ModManager.instance;
            mm.AddSearchDirectory(searchDirectory);
            mm.RefreshSearchDirectories();
            Debug.Log($"MultiMod search path: {searchDirectory}");

            mm.ModsChanged += () =>
            {
                Debug.Log("Mods changed.");
                foreach (var mod in mm.mods)
                    Debug.Log(
                        $"{mod.name}: {mod.assemblyNames.Count} assemblies, {mod.contentHandler.prefabs.Count} prefabs, isValid={mod.isValid}, state {mod.loadState}");
            };

            mm.ModFound += mod =>
            {
                Debug.Log(
                    $"Mod found: {mod.name} {mod.assemblyNames.Count} assemblies, {mod.contentHandler.prefabs.Count} prefabs, isValid={mod.isValid}, state {mod.loadState}");

                foreach (var assetPath in mod.assetPaths) Debug.Log($" - {assetPath}");

                mod.Load();

                mod.Loaded += resource => { Debug.Log($"Resource loaded? {resource.loadState} - {resource.name}"); };

                Debug.Log(
                    $"Mod loaded?: {mod.name} {mod.assemblyNames.Count} assemblies, {mod.contentHandler.prefabs.Count} prefabs, isValid={mod.isValid}, state {mod.loadState}");
            };

            mm.ModLoaded += mod =>
            {
                Debug.Log($"{mod.name} loaded. Looking for ExportSettings.");
                var settings = mod.GetAsset<ExportSettings>("ExportSettings");

                if (settings == null)
                {
                    Debug.LogError("Couldn't find ExportSettings in mod assetbundle.");
                    return;
                }
                               
                var gobj = Instantiate(settings.StartupPrefab);
                Object.DontDestroyOnLoad(gobj);
                gobj.GetComponents<ModBehaviour>().ToList().ForEach(i => {
                    i.contentHandler = mod.contentHandler;
                    i.OnLoaded(mod.contentHandler);
                });
            };

            mm.ModLoadCancelled += mod => { Debug.LogWarning($"Mod loading canceled: {mod.name}"); };

            mm.ModUnloaded += mod => { Debug.Log($"Mod UNLOADED: {mod.name}"); };
        }
    }
}