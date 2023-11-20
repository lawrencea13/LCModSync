using BepInEx.Bootstrap;
using GameNetcodeStuff;
using HarmonyLib;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LCModSync.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch
    {
        [HarmonyPatch("SteamMatchmaking_OnLobbyCreated")]
        [HarmonyPrefix]
        public static void lobbyCreatedPatch(ref Result result, ref Lobby lobby)
        {
            ModSyncPlugin.getPlugins();
            //ModSyncPlugin.Instance.currentModDownloaded = false;
            //ModSyncPlugin.Instance.StartCoroutine("waitForModDownloads");
            //ModSyncPlugin.promptDownloadMod("2018", "LC_API");
            ModSyncPlugin.mls.LogInfo("Lobby created");
            lobby.SetData("TestData", "BOOP");
            lobby.SetData("modNames", String.Join(" ", ModSyncPlugin.modNames));
            lobby.SetData("modCreators", String.Join(" ", ModSyncPlugin.modCreators));
        }
    }
}
