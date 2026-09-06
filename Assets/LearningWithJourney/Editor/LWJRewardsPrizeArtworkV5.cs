#if UNITY_EDITOR
using LearningWithJourney.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LearningWithJourney.EditorTools
{
    public static class LWJRewardsPrizeArtworkV5
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/RewardsRoom.unity";
        const string ArtworkName = "PrizeArtworkV5";

        [MenuItem("Learning with Journey/Apply Transparent Matched Reward Art V5")]
        public static void Apply()
        {
            ApplyInternal(true);
        }

        public static void ApplySilently()
        {
            ApplyInternal(false);
        }

        static void ApplyInternal(bool showDialog)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                if (showDialog)
                    EditorUtility.DisplayDialog("Learning with Journey", "RewardsRoom.unity was not found. Build Rewards V1 first.", "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // GameObject.Find cannot see inactive objects. PrizeReveal is intentionally
            // inactive while the chest is closed, so search the loaded scene hierarchy
            // including inactive children instead.
            var controller = FindComponentInScene<RewardsScreenControllerV1>(scene);
            GameObject prize = FindObjectInScene(scene, "PrizeReveal");

            // Extra fallback: use the controller's serialized prizeRoot reference in case
            // the object is ever renamed in a later Rewards polish pass.
            if (prize == null && controller != null)
            {
                var controllerSo = new SerializedObject(controller);
                var prizeRootProp = controllerSo.FindProperty("prizeRoot");
                if (prizeRootProp != null && prizeRootProp.objectReferenceValue is RectTransform prizeRoot)
                    prize = prizeRoot.gameObject;
            }

            if (prize == null || controller == null)
            {
                if (showDialog)
                {
                    string missing = prize == null && controller == null
                        ? "PrizeReveal and RewardsController"
                        : (prize == null ? "PrizeReveal" : "RewardsController");
                    EditorUtility.DisplayDialog(
                        "Learning with Journey",
                        "Rewards V5 could not find " + missing + " in RewardsRoom.unity. Rebuild Rewards V1 only if that scene object was deleted.",
                        "OK");
                }
                return;
            }

            // Remove/disable the old generic gift-box placeholder so the picture always
            // matches the reward name and no prize artwork has a rectangular background.
            var gift = prize.transform.Find("GiftBox");
            if (gift != null) gift.gameObject.SetActive(false);

            var oldArtwork = prize.transform.Find(ArtworkName);
            if (oldArtwork != null) Object.DestroyImmediate(oldArtwork.gameObject);

            var artworkGo = new GameObject(ArtworkName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TransparentRewardArtworkV5));
            artworkGo.transform.SetParent(prize.transform, false);
            var artworkRect = artworkGo.GetComponent<RectTransform>();
            artworkRect.anchorMin = new Vector2(.12f, .02f);
            artworkRect.anchorMax = new Vector2(.88f, .65f);
            artworkRect.offsetMin = artworkRect.offsetMax = Vector2.zero;
            artworkRect.localScale = Vector3.one;
            artworkRect.localRotation = Quaternion.identity;

            var artwork = artworkGo.GetComponent<TransparentRewardArtworkV5>();
            artwork.raycastTarget = false;
            artwork.color = Color.white;
            artwork.SetReward(0);
            artworkGo.transform.SetAsFirstSibling();

            // Keep text clear above the transparent artwork.
            var title = prize.transform.Find("PrizeTitle") as RectTransform;
            if (title != null)
            {
                title.anchorMin = new Vector2(0f, .68f);
                title.anchorMax = new Vector2(1f, .84f);
                title.offsetMin = title.offsetMax = Vector2.zero;
                var text = title.GetComponent<TMP_Text>();
                if (text != null)
                {
                    text.fontSize = 21f;
                    text.enableAutoSizing = true;
                    text.fontSizeMin = 15f;
                    text.fontSizeMax = 23f;
                }
            }

            var amount = prize.transform.Find("PrizeAmount") as RectTransform;
            if (amount != null)
            {
                amount.anchorMin = new Vector2(0f, .85f);
                amount.anchorMax = new Vector2(1f, 1.02f);
                amount.offsetMin = amount.offsetMax = Vector2.zero;
                var text = amount.GetComponent<TMP_Text>();
                if (text != null)
                {
                    text.fontSize = 16f;
                    text.enableAutoSizing = true;
                    text.fontSizeMin = 12f;
                    text.fontSizeMax = 18f;
                }
            }

            var so = new SerializedObject(controller);
            var prop = so.FindProperty("prizeArtwork");
            if (prop != null) prop.objectReferenceValue = artwork;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "Rewards V5 applied successfully. The reward artwork is transparent, and Gold Star Sticker, Rainbow Badge, Crown Badge, and Super Learner Trophy are matched to their correct prize names.",
                    "OK");
            }
        }

        static GameObject FindObjectInScene(Scene scene, string objectName)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var transforms = root.GetComponentsInChildren<Transform>(true);
                foreach (var t in transforms)
                    if (t != null && t.name == objectName)
                        return t.gameObject;
            }
            return null;
        }

        static T FindComponentInScene<T>(Scene scene) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component != null) return component;
            }
            return null;
        }
    }
}
#endif
