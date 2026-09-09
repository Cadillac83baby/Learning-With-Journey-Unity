#if UNITY_EDITOR
using System.IO;
using LearningWithJourney.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.EditorTools
{
    public static class LWJInstallUiVoiceV1
    {
        const string ResourceRoot = "Assets/LearningWithJourney/Resources/JourneyVoice/UI";
        const string MainMenuScene = "Assets/LearningWithJourney/Scenes/MainMenu.unity";

        [MenuItem("Learning with Journey/Install UI Voice Pack V1")]
        public static void Install()
        {
            string[] required =
            {
                "MENU_welcome.mp3", "Menu_choose.mp3", "Menu_Counting.mp3", "Menu_letters.mp3",
                "Menu_Matching.mp3", "Name_ask.mp3", "Name_help.mp3", "Name_ready.mp3",
                "Nav_home.mp3", "Parent_welcome.mp3"
            };

            int missing = 0;
            foreach (string file in required)
                if (!File.Exists(Path.Combine(ResourceRoot, file))) missing++;

            if (missing > 0)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    $"The UI voice pack is missing {missing} file(s). Extract the voice pack into:\n\n{ResourceRoot}",
                    "OK");
                return;
            }

            if (!File.Exists(MainMenuScene))
            {
                EditorUtility.DisplayDialog("Learning with Journey", "MainMenu.unity was not found. Build the polished Main Menu first.", "OK");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(MainMenuScene, OpenSceneMode.Single);
            Transform canvas = Find(scene, "Canvas");
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Main Menu Canvas was not found.", "OK");
                return;
            }

            JourneyUiVoiceV1 voice = canvas.GetComponent<JourneyUiVoiceV1>();
            if (voice == null) voice = Undo.AddComponent<JourneyUiVoiceV1>(canvas.gameObject);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainMenuScene);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "UI Journey voice pack installed. Main Menu game tiles now play their matching cues, Name Setup and Parent Zone use their clips automatically, and the Home cue survives scene changes.",
                "OK");
        }

        static Transform Find(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                    if (item.name == name) return item;
            return null;
        }
    }
}
#endif
