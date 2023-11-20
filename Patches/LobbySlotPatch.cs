using HarmonyLib;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LCModSync.Patches
{
    [HarmonyPatch(typeof(LobbySlot))]
    internal class LobbySlotPatch
    {

        //public static LobbySlotPatch Instance;
        private static Lobby lobbyRef;

        public LobbySlotPatch Instance { get; private set; }

        [HarmonyPatch(nameof(LobbySlot.OnLobbyDataRefresh))]
        [HarmonyPrefix]
        private static bool patchLobbyJoin(ref Lobby lobby, ref Coroutine ___timeOutLobbyRefreshCoroutine)
        {
            // implement this at the beginning because if this is called, we'll get touched in the butt
            if (___timeOutLobbyRefreshCoroutine != null)
            {
                ModSyncPlugin.mls.LogInfo("timeout still existed, destroy it so we don't get kicked out");
                GameNetworkManager.Instance.StopCoroutine(___timeOutLobbyRefreshCoroutine);
                ___timeOutLobbyRefreshCoroutine = null;
            }
            if (!GameNetworkManager.Instance.waitingForLobbyDataRefresh)
            {
                ModSyncPlugin.mls.LogInfo("prevent infinite load on fail");
                return false;
            }

            // set var and remove delegate
            GameNetworkManager.Instance.waitingForLobbyDataRefresh = false;
            SteamMatchmaking.OnLobbyDataChanged -= LobbySlot.OnLobbyDataRefresh;

            lobbyRef = lobby;



            // boop checks if mod implemented tbh
            ModSyncPlugin.mls.LogInfo(lobby.GetData("TestData"));
            string strModNames = lobby.GetData("modNames");
            string strModCreators = lobby.GetData("modCreators");
            List<string> listModNames = new List<string>();
            List<string> listModCreators = new List<string>();
            if (strModNames != "")
            {
                listModNames = strModNames.Split(' ').ToList();
                listModCreators = strModCreators.Split(' ').ToList();
            }
            else
            {

                // testing
                listModNames.Add("LC_API");
                listModCreators.Add("2018");

                listModNames.Add("GameMaster");
                listModCreators.Add("GameMasterDevs");
            }

            if (listModCreators.Count != listModNames.Count)
            {
                ModSyncPlugin.mls.LogWarning("Host has mods with improper formatting.");
            }

            ModSyncPlugin.Instance.currentModDownloaded = false;
            ModSyncPlugin.Instance.promptDownloadMods(listModCreators, listModNames);

            return false;

        }

        public static void finishLoadingIntoLobby()
        {
            
            // reload plugins before we join
            ModSyncPlugin.Instance.ReloadPlugins();
            // when downloads are complete, we should continue into the game.
            // if lobby is joinable, join it
            if (GameNetworkManager.Instance.LobbyDataIsJoinable(lobbyRef))
            {
                GameNetworkManager.Instance.JoinLobby(lobbyRef, lobbyRef.Id);
            }
        }
    }
}
