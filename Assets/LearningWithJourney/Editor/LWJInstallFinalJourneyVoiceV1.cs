#if UNITY_EDITOR
using System.IO;
using LearningWithJourney.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.EditorTools
{
    public static class LWJInstallFinalJourneyVoiceV1
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/BookReader.unity";
        const string ClipPath = "Assets/LearningWithJourney/Resources/JourneyVoice/ABC/01.mp3";

        [MenuItem("Learning with Journey/Apply Final Journey Voice to Book Reader")]
        public static void Apply()
        {
            AssetDatabase.Refresh();
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClipPath);
            if (clip == null)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "The final Journey voice file is not installed yet. Put the mastered file at:\n\n" + ClipPath + "\n\nThen run this menu command again.",
                    "OK");
                return;
            }

            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("Learning with Journey", "BookReader scene was not found.", "OK");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            bool assignedToV1 = false;

            foreach (JourneyVoicePlayerV1 voice in Resources.FindObjectsOfTypeAll<JourneyVoicePlayerV1>())
            {
                if (voice == null || voice.gameObject.scene != scene) continue;
                AssignABCPageOne(voice, clip);
                assignedToV1 = true;
            }

            // JourneyVoicePlayerV2 resolves this same file automatically from Resources,
            // so no serialized clip assignment is needed for V2 readers.
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string detail = assignedToV1
                ? "The final mastered Journey voice is connected to ABC page 1 in the current Book Reader."
                : "The final mastered Journey voice is installed in Resources. Book Reader V2 will load it automatically for ABC page 1.";

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                detail + "\n\nREAD AGAIN will replay the same mastered clip. No device TTS is used.",
                "OK");
        }

        static void AssignABCPageOne(JourneyVoicePlayerV1 voice, AudioClip clip)
        {
            SerializedObject so = new SerializedObject(voice);
            SerializedProperty lines = so.FindProperty("pageLines");
            if (lines == null) return;

            int targetIndex = -1;
            for (int i = 0; i < lines.arraySize; i++)
            {
                SerializedProperty item = lines.GetArrayElementAtIndex(i);
                SerializedProperty id = item.FindPropertyRelative("bookId");
                SerializedProperty page = item.FindPropertyRelative("pageIndex");
                if (id != null && page != null && id.stringValue == "ABC" && page.intValue == 0)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex < 0)
            {
                targetIndex = lines.arraySize;
                lines.InsertArrayElementAtIndex(targetIndex);
            }

            SerializedProperty entry = lines.GetArrayElementAtIndex(targetIndex);
            entry.FindPropertyRelative("bookId").stringValue = "ABC";
            entry.FindPropertyRelative("pageIndex").intValue = 0;
            entry.FindPropertyRelative("clip").objectReferenceValue = clip;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(voice);
        }
    }
}
#endif
