using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using LCModSync.Patches;
using Mono.Cecil;
using UnityEngine;

namespace LCModSync
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class ModSyncPlugin : BaseUnityPlugin
    {
        private const string modGUID = "Posiedon.ModSync";
        private const string modName = "Lethal Company ModSync";
        private const string modVersion = "0.0.2";

        private readonly Harmony harmony = new Harmony(modGUID);

        internal static ManualLogSource mls;

        internal static List<PluginInfo> plugins;

        internal static List<string> modURLs;
        internal static List<string> modNames;

        public static ModSyncPlugin Instance;

        public string ScriptDirectory => Path.Combine(Paths.BepInExRootPath, "scripts");
        private GameObject scriptManager;


        void Awake()
        {
            this.gameObject.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
            if(Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo($"Loaded {modName}, patching.");
            harmony.PatchAll(typeof(GameNetworkManagerPatch));
            harmony.PatchAll(typeof(LobbySlotPatch));
            System.IO.Directory.CreateDirectory(".\\BepInEx\\scripts");

            modURLs = new List<string>();
            modNames = new List<string>();

            getPlugins();
            mls.LogInfo(String.Join(" ", modURLs));



        }

        static void getPlugins()
        {
            mls.LogInfo("Checking for plugins");
            foreach (var plugin in Chainloader.PluginInfos)
            {
                try
                {
                    plugin.Value.Instance.BroadcastMessage("modURL", Instance, UnityEngine.SendMessageOptions.DontRequireReceiver);
                }
                catch (Exception e)
                {
                    // ignore mod if they haven't implemented the necessary method
                    mls.LogInfo($"Failed to gather details about {plugin.Value.Metadata.Name}. Skipping.");
                }
            }

        }

        internal static void downloadMods(string modURI, string modFileName)
        {
            // we can also test more logic here, a hacker may be able to mod their own thing to inject their own files,
            // but unless they distribute malicious copies of this mod, they can't bypass checks here
            // doesn't bother checking if mods exist yet

            using (WebClient wc = new WebClient())
            {
                wc.DownloadFileAsync(
                    new System.Uri(modURI),
                    // name of file, aka where it will go
                    ".\\Bepinex\\scripts\\" + modFileName
                );
            }
        }

        static internal void storeModInfo(string modURL, string modName)
        {
            // we can do more checking here for security in the future. E.g. if it's not a certain type of url ignore it
            try
            {
                if (modURLs.Contains(modURL))
                {
                    return;
                }
                else
                {
                    modNames.Add(modName);
                    modURLs.Add(modURL);
                }
            }
            catch
            {
                mls.LogInfo("getModInfo failed");
            }

        }

        public void getModURLandName(string url, string modname)
        {
            /* After sending a message to a mod to ask for their information, they should be calling this method
             * Upon calling this method, they should include a string for the URL of the mod, and a string for the name formatted as name.dll
             * nonstatic, can be called from external area
             */

            mls.LogInfo($"We have received {url}");
            storeModInfo(url, modname);
        }

        private IEnumerable<Type> GetTypesSafe(Assembly ass)
        {
            try
            {
                return ass.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                var sbMessage = new StringBuilder();
                sbMessage.AppendLine("\r\n-- LoaderExceptions --");
                foreach (var l in ex.LoaderExceptions)
                    sbMessage.AppendLine(l.ToString());
                sbMessage.AppendLine("\r\n-- StackTrace --");
                sbMessage.AppendLine(ex.StackTrace);
                Logger.LogError(sbMessage.ToString());
                return ex.Types.Where(x => x != null);
            }
        }

        private IEnumerator DelayAction(Action action)
        {
            yield return null;
            action();
        }

        internal void ReloadPlugins()
        {
            if (scriptManager != null)
            {

                foreach (var previouslyLoadedPlugin in scriptManager.GetComponents<BaseUnityPlugin>())
                {
                    var metadataGUID = previouslyLoadedPlugin.Info.Metadata.GUID;
                    if (Chainloader.PluginInfos.ContainsKey(metadataGUID))
                        Chainloader.PluginInfos.Remove(metadataGUID);
                }

                Destroy(scriptManager);
            }

            scriptManager = new GameObject($"ScriptEngine_{DateTime.Now.Ticks}");
            DontDestroyOnLoad(scriptManager);

            var files = Directory.GetFiles(ScriptDirectory, "*.dll", SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                foreach (string path in Directory.GetFiles(ScriptDirectory, "*.dll", SearchOption.AllDirectories))
                    LoadDLL(path, scriptManager);

            }
            else
            {

            }
        }

        private void LoadDLL(string path, GameObject obj)
        {
            var defaultResolver = new DefaultAssemblyResolver();
            defaultResolver.AddSearchDirectory(ScriptDirectory);
            defaultResolver.AddSearchDirectory(Paths.ManagedPath);
            defaultResolver.AddSearchDirectory(Paths.BepInExAssemblyDirectory);


            using (var dll = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { AssemblyResolver = defaultResolver }))
            {
                //dll.Name.Name = $"{dll.Name.Name}-{DateTime.Now.Ticks}";

                using (var ms = new MemoryStream())
                {
                    dll.Write(ms);
                    var ass = Assembly.Load(ms.ToArray());

                    foreach (Type type in GetTypesSafe(ass))
                    {
                        
                        try
                        {
                            
                            if (!typeof(BaseUnityPlugin).IsAssignableFrom(type)) continue;

                            var metadata = MetadataHelper.GetMetadata(type);
                            if (metadata == null) continue;

                            if (Chainloader.PluginInfos.TryGetValue(metadata.GUID, out var existingPluginInfo))
                                throw new InvalidOperationException($"A plugin with GUID {metadata.GUID} is already loaded! ({existingPluginInfo.Metadata.Name} v{existingPluginInfo.Metadata.Version})");

                            var typeDefinition = dll.MainModule.Types.First(x => x.FullName == type.FullName);
                            var pluginInfo = Chainloader.ToPluginInfo(typeDefinition);
                        

                            StartCoroutine(DelayAction(() =>
                            {
                                try
                                {
                                    // Need to add to PluginInfos first because BaseUnityPlugin constructor (called by AddComponent below)
                                    // looks in PluginInfos for an existing PluginInfo and uses it instead of creating a new one.
                                    Chainloader.PluginInfos[metadata.GUID] = pluginInfo;

                                    var instance = obj.AddComponent(type);

                                    // Fill in properties that are normally set by Chainloader
                                    var tv = Traverse.Create(pluginInfo);
                                    tv.Property<BaseUnityPlugin>(nameof(pluginInfo.Instance)).Value = (BaseUnityPlugin)instance;
                                    // Loading the assembly from memory causes Location to be lost
                                    tv.Property<string>(nameof(pluginInfo.Location)).Value = path;
                                }
                                catch (Exception e)
                                {
                                    Logger.LogError($"Failed to load plugin {metadata.GUID} because of exception: {e}");
                                    Chainloader.PluginInfos.Remove(metadata.GUID);
                                }
                            }));
                        }
                        catch (Exception e)
                        {
                            Logger.LogError($"Failed to load plugin {type.Name} because of exception: {e}");
                        }
                    }
                }
            }
        }

    }
}
