using UnityEngine;

namespace LCModSync.MYGUI
{
    internal class ConfirmDownloadGUI : MonoBehaviour
    {
        public bool isMenuOpen;
        private bool wasKeyDown;

        private int MENUWIDTH = 800;
        private int MENUHEIGHT = 400;
        private int MENUX;
        private int MENUY;
        private int ITEMWIDTH = 300;
        private int CENTERX;

        private GUIStyle menuStyle;
        private GUIStyle confirmButtonStyle;
        private GUIStyle declineButtonStyle;
        private GUIStyle labelStyle;
        private GUIStyle hScrollStyle;


        private void Awake()
        {
            ModSyncPlugin.mls.LogInfo("Download Confirmation has arrived");
            isMenuOpen = false;
            
            MENUWIDTH = Screen.width / 3;
            MENUHEIGHT = Screen.width / 4;
            ITEMWIDTH = MENUWIDTH / 2;
            MENUX = (Screen.width / 2) - (MENUWIDTH / 2);
            MENUY = (Screen.height / 2) - (MENUHEIGHT / 2);
            CENTERX = MENUX + ((MENUWIDTH / 2) - (ITEMWIDTH / 2));
        }


        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }


        private void intitializeMenu()
        {
            if (menuStyle == null)
            {
                menuStyle = new GUIStyle(GUI.skin.box);
                confirmButtonStyle = new GUIStyle(GUI.skin.button);
                declineButtonStyle = new GUIStyle(GUI.skin.button);
                labelStyle = new GUIStyle(GUI.skin.label);
                hScrollStyle = new GUIStyle(GUI.skin.horizontalSlider);

                menuStyle.normal.textColor = Color.white;
                menuStyle.normal.background = MakeTex(2, 2, new Color(0.01f, 0.01f, 0.1f, .9f));
                menuStyle.fontSize = 18;
                menuStyle.normal.background.hideFlags = HideFlags.HideAndDontSave;

                confirmButtonStyle.normal.textColor = Color.white;
                confirmButtonStyle.normal.background = MakeTex(2, 2, new Color(0.01f, 0.3f, 0.1f, .9f));
                confirmButtonStyle.hover.background = MakeTex(2, 2, new Color(0.01f, 0.4f, 0.1f, .9f));
                confirmButtonStyle.fontSize = 22;
                confirmButtonStyle.normal.background.hideFlags = HideFlags.HideAndDontSave;

                declineButtonStyle.normal.textColor = Color.white;
                declineButtonStyle.normal.background = MakeTex(2, 2, new Color(0.3f, 0.01f, 0.1f, .9f));
                declineButtonStyle.hover.background = MakeTex(2, 2, new Color(0.4f, 0.01f, 0.1f, .9f));
                declineButtonStyle.fontSize = 22;
                declineButtonStyle.normal.background.hideFlags = HideFlags.HideAndDontSave;

                labelStyle.normal.textColor = Color.white;
                labelStyle.normal.background = MakeTex(2, 2, new Color(0.01f, 0.01f, 0.1f, 0f));
                labelStyle.fontSize = 24;
                labelStyle.alignment = TextAnchor.MiddleCenter;
                labelStyle.normal.background.hideFlags = HideFlags.HideAndDontSave;

                hScrollStyle.normal.textColor = Color.white;
                hScrollStyle.normal.background = MakeTex(2, 2, new Color(0.0f, 0.0f, 0.2f, .9f));
                hScrollStyle.normal.background.hideFlags = HideFlags.HideAndDontSave;

            }
        }


        public void OnGUI()
        {
            if (menuStyle == null) { intitializeMenu(); }

            GUI.Box(new Rect(MENUX, MENUY, MENUWIDTH, MENUHEIGHT), "ModSync", menuStyle);
            GUI.Label(new Rect(CENTERX, MENUY + 100, ITEMWIDTH, 80), $"Would you like to download {ModSyncPlugin.Instance.currentModName} by {ModSyncPlugin.Instance.currentModCreator}?", labelStyle);
            GUI.Label(new Rect(CENTERX, MENUY + 200, ITEMWIDTH, 80), $"{ModSyncPlugin.Instance.downloadProgress}%", labelStyle);
            if (GUI.Button(new Rect(MENUX + (0.25f * MENUX) - ((ITEMWIDTH / 1.5f) / 2), MENUY + MENUHEIGHT - 150, ITEMWIDTH / 1.5f, 50), "Confirm Download", confirmButtonStyle))
            {
                ModSyncPlugin.downloadFromURLAfterConfirmation(ModSyncPlugin.Instance.currentModURL, ModSyncPlugin.Instance.currentModName);
            }

            if (GUI.Button(new Rect(MENUX + (0.75f * MENUX) - ((ITEMWIDTH / 1.5f) / 2), MENUY + MENUHEIGHT - 150, ITEMWIDTH / 1.5f, 50), "Decline Download", declineButtonStyle))
            {
                ModSyncPlugin.Instance.currentModDownloaded = true;
            }

            if (ModSyncPlugin.Instance.guiMenuOpen)
            {

            }
        }
    }
}
