﻿using HarmonyLib;
using Netcode.Transports.Facepunch;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LCModSync.Patches
{
    [HarmonyPatch(typeof(LobbySlot))]
    internal class LobbySlotPatch
    {
        [HarmonyPatch(nameof(LobbySlot.OnLobbyDataRefresh))]
        [HarmonyPrefix]
        private static bool patchLobbyJoin(ref Lobby lobby)
        {
            // boop checks if mod implemented tbh
            if (lobby.GetData("TestData") == "BOOP")
            {
                ModSyncPlugin.mls.LogInfo(lobby.GetData("TestData"));
                string strModNames = lobby.GetData("modNames");
                string strModCreators = lobby.GetData("modCreators");
                List<string> listModNames = strModNames.Split(' ').ToList();
                List<string> listModCreators = strModCreators.Split(' ').ToList();
                if(listModCreators.Count != listModNames.Count)
                {
                    ModSyncPlugin.mls.LogWarning("Host has mods with improper formatting.");
                }

                try
                {
                    ModSyncPlugin.Instance.currentModDownloaded = false;
                    ModSyncPlugin.Instance.staticBridgeCoroutineStart(() => ModSyncPlugin.Instance.promptDownloadMods(listModCreators, listModNames));

                }
                catch (Exception e)
                {
                    //ModSyncPlugin.mls.LogInfo($"We tried to download {listModNames[i]} but failed. Advise the developer to check the URL or name.");
                }

                ModSyncPlugin.Instance.ReloadPlugins();
            }
            else
            {
                ModSyncPlugin.mls.LogInfo("DO NOT BOOP");
            }

            while (!ModSyncPlugin.Instance.allModsDownloaded)
            {
                // 
                ModSyncPlugin.Instance.staticBridgeCoroutineStart(() => ModSyncPlugin.Instance.staticBridgeWaitForModDownloads(0.1f));
            }

            return true;

        }
    }
}
