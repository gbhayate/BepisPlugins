﻿using alphaShot;
using BepInEx;
using BepInEx.Logging;
using Illusion.Game;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Screencap
{
    [BepInPlugin(GUID: GUID, Name: "Screenshot Manager", Version: "2.2")]
    public class ScreenshotManager : BaseUnityPlugin
    {
        internal const string GUID = "com.bepis.bepinex.screenshotmanager";
        private int WindowHash = GUID.GetHashCode();

        private string screenshotDir = Path.Combine(Application.dataPath, "..\\UserData\\cap\\");
        private AlphaShot2 as2 = null;

        private KeyCode CK_Capture = KeyCode.F9;
        private KeyCode CK_CaptureAlpha = KeyCode.F11;

        #region Config properties

        public int ResolutionX
        {
            get => int.Parse(this.GetEntry("resolution-x", "1024"));
            set => this.SetEntry("resolution-x", value.ToString());
        }

        public int ResolutionY
        {
            get => int.Parse(this.GetEntry("resolution-y", "1024"));
            set => this.SetEntry("resolution-y", value.ToString());
        }

        public int DownscalingRate
        {
            get => int.Parse(this.GetEntry("downscalerate", "1"));
            set => this.SetEntry("downscalerate", value.ToString());
        }

        public int CardDownscalingRate
        {
            get => int.Parse(this.GetEntry("carddownscalerate", "1"));
            set => this.SetEntry("carddownscalerate", value.ToString());
        }

        public bool CaptureAlpha
        {
            get => bool.Parse(this.GetEntry("capturealpha", "true"));
            set => this.SetEntry("capturealpha", value.ToString());
        }

        #endregion

        private string filename => Path.GetFullPath(Path.Combine(screenshotDir, $"Koikatsu-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.png"));

        void Awake()
        {
            SceneManager.sceneLoaded += (s, a) => Install();
            Install();

            if (!Directory.Exists(screenshotDir))
                Directory.CreateDirectory(screenshotDir);

            Hooks.InstallHooks();
        }

        private void Install()
        {
            if (!Camera.main || !Camera.main.gameObject) return;
            as2 = Camera.main.gameObject.GetOrAddComponent<AlphaShot2>();
        }

        void Update()
        {
            if (Input.GetKeyDown(CK_CaptureAlpha))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) showingUI = !showingUI;
                else TakeCharScreenshot();
            }
            if (Input.GetKeyDown(CK_Capture)) StartCoroutine(TakeScreenshot());
        }

        IEnumerator TakeScreenshot()
        {
            Application.CaptureScreenshot(filename);
            Utils.Sound.Play(SystemSE.photo);

            yield return new WaitUntil(() => File.Exists(filename));

            BepInEx.Logger.Log(LogLevel.Message, $"Screenshot saved to {filename}");
        }

        void TakeCharScreenshot()
        {
            File.WriteAllBytes(filename, as2.Capture(ResolutionX, ResolutionY, DownscalingRate, CaptureAlpha));

            Utils.Sound.Play(SystemSE.photo);
            BepInEx.Logger.Log(LogLevel.Message, $"Character screenshot saved to {filename}");

            //GC.Collect();
        }


        #region UI
        private Rect UI = new Rect(20, 20, 160, 200);
        private bool showingUI = false;

        void OnGUI()
        {
            if (showingUI)
                UI = GUI.Window(WindowHash, UI, WindowFunction, "Rendering settings");
        }

        void WindowFunction(int windowID)
        {
            GUI.Label(new Rect(0, 20, 160, 20), "Output resolution", new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState
                {
                    textColor = Color.white
                }
            });

            GUI.Label(new Rect(0, 40, 160, 20), "x", new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState
                {
                    textColor = Color.white
                }
            });

            string resX = GUI.TextField(new Rect(10, 40, 60, 20), ResolutionX.ToString());

            string resY = GUI.TextField(new Rect(90, 40, 60, 20), ResolutionY.ToString());

            bool screenSize = GUI.Button(new Rect(10, 65, 140, 20), "Set to screen size");


            GUI.Label(new Rect(0, 90, 160, 20), "Downscaling rate", new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState
                {
                    textColor = Color.white
                }
            });


            int downscale = (int)Math.Round(GUI.HorizontalSlider(new Rect(10, 113, 120, 20), DownscalingRate, 1, 4));

            GUI.Label(new Rect(0, 110, 150, 20), $"{downscale}x", new GUIStyle
            {
                alignment = TextAnchor.UpperRight,
                normal = new GUIStyleState
                {
                    textColor = Color.white
                }
            });


            GUI.Label(new Rect(0, 130, 160, 20), "Card downscaling rate", new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState
                {
                    textColor = Color.white
                }
            });


            int carddownscale = (int)Math.Round(GUI.HorizontalSlider(new Rect(10, 153, 120, 20), CardDownscalingRate, 1, 4));

            GUI.Label(new Rect(0, 150, 150, 20), $"{carddownscale}x", new GUIStyle
            {
                alignment = TextAnchor.UpperRight,
                normal = new GUIStyleState
                {
                    textColor = Color.white
                }
            });

            bool capturealpha = GUI.Toggle(new Rect(10, 173, 120, 20), CaptureAlpha, "Capture alpha");


            if (GUI.changed)
            {
                BepInEx.Config.SaveOnConfigSet = false;

                if (int.TryParse(resX, out int x))
                    ResolutionX = Mathf.Clamp(x, 2, 4096);

                if (int.TryParse(resY, out int y))
                    ResolutionY = Mathf.Clamp(y, 2, 4096);

                if (screenSize)
                {
                    ResolutionX = Screen.width;
                    ResolutionY = Screen.height;
                }

                DownscalingRate = downscale;

                CardDownscalingRate = carddownscale;

                CaptureAlpha = capturealpha;

                BepInEx.Config.SaveOnConfigSet = true;
                BepInEx.Config.SaveConfig();
            }

            GUI.DragWindow();
        }
        #endregion
    }
}
