#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJAlphabetMatchCardBackV3
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/AlphabetMatchWorld.unity";
        const string CardBackPath = "Assets/LearningWithJourney/Art/AlphabetMatch/LWJ_Match_Card_Back.jpg";
        const string LogoObjectName = "LearningWithJourneyCardBackLogo";

        [MenuItem("Learning with Journey/Apply Learning with Journey Match Card Backs V3")]
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
            if (!File.Exists(ScenePath))
            {
                if (showDialog)
                    EditorUtility.DisplayDialog("Learning with Journey", "AlphabetMatchWorld.unity was not found. Build Alphabet Match World V2 first.", "OK");
                return;
            }

            if (!File.Exists(CardBackPath))
            {
                if (showDialog)
                    EditorUtility.DisplayDialog("Learning with Journey", "The approved Learning with Journey matching-card artwork was not found.", "OK");
                return;
            }

            ConfigureCardBackImporter();
            var cardBackSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CardBackPath);
            if (cardBackSprite == null)
            {
                if (showDialog)
                    EditorUtility.DisplayDialog("Learning with Journey", "Unity could not load the approved matching-card artwork as a Sprite.", "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            int updated = 0;

            for (int i = 1; i <= 8; i++)
            {
                var card = GameObject.Find("MatchCard" + i);
                if (card == null) continue;

                var back = card.transform.Find("Back");
                if (back == null) continue;

                // Remove the old temporary question-mark treatment so the approved
                // Learning with Journey artwork is the only visible card-back design.
                SetChildActive(back, "Question", false);
                SetChildActive(back, "MatchLabel", false);
                SetChildActive(back, "Gloss", false);

                var oldLogo = back.Find(LogoObjectName);
                if (oldLogo != null)
                    Object.DestroyImmediate(oldLogo.gameObject);

                var logoGo = new GameObject(LogoObjectName, typeof(RectTransform), typeof(Image));
                logoGo.transform.SetParent(back, false);

                var rect = (RectTransform)logoGo.transform;
                rect.anchorMin = new Vector2(.025f, .025f);
                rect.anchorMax = new Vector2(.975f, .975f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;

                var image = logoGo.GetComponent<Image>();
                image.sprite = cardBackSprite;
                image.preserveAspect = true;
                image.color = Color.white;
                image.raycastTarget = false;

                logoGo.transform.SetAsLastSibling();
                updated++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "Alphabet Match card backs updated. " + updated + " cards now use the approved Learning with Journey logo artwork. Gameplay, card fronts, Journey, Levels, Points, and matching logic were not changed.",
                    "OK");
            }
        }

        static void SetChildActive(Transform parent, string childName, bool value)
        {
            var child = parent.Find(childName);
            if (child != null) child.gameObject.SetActive(value);
        }

        static void ConfigureCardBackImporter()
        {
            AssetDatabase.ImportAsset(CardBackPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(CardBackPath) as TextureImporter;
            if (importer == null) return;

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }
            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }
            if (importer.filterMode != FilterMode.Bilinear)
            {
                importer.filterMode = FilterMode.Bilinear;
                changed = true;
            }
            if (importer.maxTextureSize < 256)
            {
                importer.maxTextureSize = 256;
                changed = true;
            }
            if (importer.textureCompression != TextureImporterCompression.CompressedHQ)
            {
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                changed = true;
            }

            if (changed) importer.SaveAndReimport();
        }
    }
}
#endif
