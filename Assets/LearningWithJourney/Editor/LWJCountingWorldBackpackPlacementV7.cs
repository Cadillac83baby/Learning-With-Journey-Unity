#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LearningWithJourney.EditorTools
{
    public static class LWJCountingWorldBackpackPlacementV7
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/CountingWorld.unity";

        [MenuItem("Learning with Journey/Fix Counting Backpack Position V7")]
        public static void Apply()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "CountingWorld.unity was not found.",
                    "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var bag = GameObject.Find("JourneyBackpack");
            var journey = GameObject.Find("Journey");

            if (bag == null || bag.transform is not RectTransform bagRect)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "JourneyBackpack was not found on the Counting World screen.",
                    "OK");
                return;
            }

            // Match the approved Main Menu placement. The backpack is intentionally
            // shifted left and made slightly wider/taller so it covers only the
            // damaged screen-right shorts/leg area without covering both legs.
            bagRect.anchorMin = new Vector2(.255f, .335f);
            bagRect.anchorMax = new Vector2(.355f, .425f);
            bagRect.offsetMin = Vector2.zero;
            bagRect.offsetMax = Vector2.zero;
            bagRect.localRotation = Quaternion.identity;
            bagRect.localScale = Vector3.one;

            // Keep the bag visibly in front of Journey while preserving the rest of
            // the Counting World UI draw order. Do not move Journey herself.
            if (journey != null && journey.transform.parent == bag.transform.parent)
            {
                int desiredIndex = Mathf.Min(
                    journey.transform.GetSiblingIndex() + 1,
                    bag.transform.parent.childCount - 1);
                bag.transform.SetSiblingIndex(desiredIndex);
            }
            else
            {
                bag.transform.SetAsLastSibling();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Counting World backpack placement fixed. The backpack now uses the approved Main Menu position to cover Journey's damaged screen-right shorts/leg area. Main Menu was not changed.",
                "OK");
        }
    }
}
#endif
