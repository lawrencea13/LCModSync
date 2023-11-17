using HarmonyLib;
using Netcode.Transports.Facepunch;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCModSync.Patches
{
    [HarmonyPatch(typeof(LobbySlot))]
    internal class LobbySlotPatch
    {
        [HarmonyPatch(nameof(LobbySlot.OnLobbyDataRefresh))]
        [HarmonyPrefix]
        private static void patchLobbyJoin(ref Lobby lobby)
        {
            // boop checks if mod implemented tbh
            if (lobby.GetData("TestData") == "BOOP")
            {
                ModSyncPlugin.mls.LogInfo(lobby.GetData("TestData"));
                string strModNames = lobby.GetData("modNames");
                string strModURLs = lobby.GetData("modURLs");
                List<string> listModNames = strModNames.Split(' ').ToList();
                List<string> listModURLs = strModURLs.Split(' ').ToList();
                if(listModURLs.Count != listModNames.Count)
                {
                    ModSyncPlugin.mls.LogWarning("Host has mods with improper formatting.");
                }

                for(int i = 0; i < listModURLs.Count; i++)
                {
                    try
                    {
                        ModSyncPlugin.downloadMods(listModURLs[i], listModNames[i]);
                    }
                    catch (Exception e)
                    {
                        ModSyncPlugin.mls.LogInfo($"We tried to download {listModNames[i]} but failed. Advise the developer to check the URL or name.");
                    }
                    
                }

                ModSyncPlugin.Instance.ReloadPlugins();
            }
            else
            {
                ModSyncPlugin.mls.LogInfo("DO NOT BOOP");
            }

        }
    }
}
