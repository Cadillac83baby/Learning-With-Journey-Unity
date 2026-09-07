#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using LearningWithJourney.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace LearningWithJourney.EditorTools
{
    public static class LWJProjectBootstrap
    {
        const string Root = "Assets/LearningWithJourney";
        const string NameSetupPath = "Assets/LearningWithJourney/Scenes/NameSetup.unity";
        static readonly string[] SceneNames =
        {
            "MainMenu","CountingWorld","ABCWorld","AlphabetMatchWorld","RewardsRoom","Library","ParentZone"
        };

        [MenuItem("Learning with Journey/Build Starter Scenes")]
        public static void BuildStarterScenes()
        {
            Directory.CreateDirectory($"{Root}/Scenes");
            foreach (var sceneName in SceneNames) BuildScene(sceneName);
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Learning with Journey", "Starter scenes and Build Settings are ready. If NameSetup has been built, it remains the first launch scene.", "OK");
        }

        static void BuildScene(string sceneName)
        {
            var path=$"{Root}/Scenes/{sceneName}.unity";
            if(File.Exists(path)) return;

            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name=sceneName;

            var systems=new GameObject("Systems");
            if(sceneName=="MainMenu") systems.AddComponent<GameProgressService>();
            systems.AddComponent<SceneRouter>();

            var cameraGO=new GameObject("Main Camera");
            var camera=cameraGO.AddComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(0.72f,0.55f,0.88f);
            camera.orthographic=true;
            cameraGO.tag="MainCamera";

            var canvasGO=new GameObject("Canvas");
            var canvas=canvasGO.AddComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1080,1920);
            scaler.matchWidthOrHeight=0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            var bg=CreatePanel(canvasGO.transform,"Background",new Color(0.62f,0.42f,0.80f),Vector2.zero,Vector2.one);
            var title=CreateText(canvasGO.transform,"Title",GetTitle(sceneName),54,FontStyles.Bold);
            var titleRect=title.rectTransform;
            titleRect.anchorMin=new Vector2(.08f,.86f); titleRect.anchorMax=new Vector2(.92f,.96f);
            titleRect.offsetMin=titleRect.offsetMax=Vector2.zero;
            title.alignment=TextAlignmentOptions.Center;

            var note=CreateText(canvasGO.transform,"PrototypeNote","Starter Unity scene — visual assets and character rig plug in here.",28,FontStyles.Normal);
            var noteRect=note.rectTransform;
            noteRect.anchorMin=new Vector2(.1f,.44f); noteRect.anchorMax=new Vector2(.9f,.58f);
            noteRect.offsetMin=noteRect.offsetMax=Vector2.zero;
            note.alignment=TextAlignmentOptions.Center;

            var eventSystem=new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            EditorSceneManager.SaveScene(scene,path);
        }

        static Image CreatePanel(Transform parent,string name,Color color,Vector2 min,Vector2 max)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));
            go.transform.SetParent(parent,false);
            var rect=(RectTransform)go.transform;
            rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero;
            var image=go.GetComponent<Image>(); image.color=color;
            return image;
        }

        static TMP_Text CreateText(Transform parent,string name,string value,float size,FontStyles style)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));
            go.transform.SetParent(parent,false);
            var text=go.GetComponent<TextMeshProUGUI>();
            text.text=value; text.fontSize=size; text.fontStyle=style; text.color=Color.white;
            return text;
        }

        static string GetTitle(string sceneName) => sceneName switch
        {
            "MainMenu" => "Learning with Journey",
            "CountingWorld" => "Counting Adventure",
            "ABCWorld" => "ABC Adventure",
            "AlphabetMatchWorld" => "Alphabet Match",
            "RewardsRoom" => "Journey's Star Room",
            "Library" => "Learning Library",
            "ParentZone" => "Parent & Caregiver Zone",
            _ => sceneName
        };

        static void UpdateBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>();

            if (File.Exists(NameSetupPath))
                scenes.Add(new EditorBuildSettingsScene(NameSetupPath, true));

            foreach (string sceneName in SceneNames)
                scenes.Add(new EditorBuildSettingsScene($"{Root}/Scenes/{sceneName}.unity", true));

            // Preserve additional enabled scenes such as BookReader without duplicating them.
            foreach (var existing in EditorBuildSettings.scenes)
            {
                if (!existing.enabled) continue;
                if (scenes.Exists(s => s.path == existing.path)) continue;
                scenes.Add(existing);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
