#if UNITY_EDITOR
using LearningWithJourney.Character;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJMainMenuRepair
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/MainMenu.unity";

        [MenuItem("Learning with Journey/Repair Journey Main Menu Art")]
        public static void RepairJourneyArt()
        {
            var guids = AssetDatabase.FindAssets("JourneyMenuAtlas t:Texture2D");
            if (guids == null || guids.Length == 0)
            {
                guids = AssetDatabase.FindAssets("JourneyMenuAtlas_q t:Texture2D");
            }

            if (guids == null || guids.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "Journey's animation atlas was not found in Assets. Extract the Main Menu Art Pack into the Unity project, then run this repair again.",
                    "OK");
                return;
            }

            var atlasPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
            if (atlas == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "The Journey atlas file was found but Unity could not load it as a Texture2D.", "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var character = GameObject.Find("JourneyCharacter");
            if (character == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "JourneyCharacter was not found. Run Build Polished Main Menu first, then run this repair.", "OK");
                return;
            }

            var raw = character.GetComponent<RawImage>();
            var controller = character.GetComponent<JourneyMainMenuCharacter>();
            if (raw == null || controller == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "JourneyCharacter is missing its RawImage or animation controller. Rebuild the Main Menu first.", "OK");
                return;
            }

            raw.texture = atlas;
            raw.color = Color.white;
            raw.raycastTarget = false;
            raw.uvRect = new Rect(0f, 2f / 3f, 1f / 5f, 1f / 3f);

            var so = new SerializedObject(controller);
            so.FindProperty("characterImage").objectReferenceValue = raw;
            so.FindProperty("atlas").objectReferenceValue = atlas;
            so.ApplyModifiedPropertiesWithoutUndo();

            var missing = GameObject.Find("JourneyArtMissing");
            if (missing != null) Object.DestroyImmediate(missing);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                $"Journey is connected to the Main Menu animation system.\n\nAtlas: {atlasPath}\n\nPress Play to preview Idle, Wave, Talk, Point, and Celebrate movement.",
                "OK");
        }
    }
}
#endif
