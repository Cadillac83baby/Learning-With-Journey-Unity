#if UNITY_EDITOR
using System.IO;
using System.Linq;
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

            var cardBackSprite = LoadCardBackSprite();
            if (cardBackSprite == null)
            {
                if (showDialog)
                    EditorUtility.DisplayDialog(
                        "Learning with Journey",
                        "Unity still could not import the card-back artwork. The importer was reset and forced to Sprite mode, but no Sprite sub-asset was created.",
                        "OK");
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

                SetChildActive(back, "Question", false);
                SetChildActive(back, "MatchLabel", false);
                SetChildActive(back, "Gloss", false);

                var backImage = back.GetComponent<Image>();
                if (backImage != null)
                {
                    backImage.color = Color.white;
                    backImage.raycastTarget = false;
                }

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
                image.type = Image.Type.Simple;
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
                    "Alphabet Match card backs updated. " + updated + " cards now use the approved Learning with Journey artwork.",
                    "OK");
            }
        }

        static Sprite LoadCardBackSprite()
        {
            // Force a synchronous import first so the importer exists even immediately after git pull.
            AssetDatabase.ImportAsset(
                CardBackPath,
                ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(CardBackPath) as TextureImporter;
            if (importer == null) return null;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 1024;
            importer.SaveAndReimport();

            AssetDatabase.ImportAsset(
                CardBackPath,
                ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            // Unity can occasionally return null for LoadAssetAtPath<Sprite> on the first import.
            // Looking through all sub-assets is more reliable for freshly imported textures.
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CardBackPath);
            if (sprite != null) return sprite;

            return AssetDatabase.LoadAllAssetsAtPath(CardBackPath)
                .OfType<Sprite>()
                .FirstOrDefault();
        }

        static void SetChildActive(Transform parent, string childName, bool value)
        {
            var child = parent.Find(childName);
            if (child != null) child.gameObject.SetActive(value);
        }
    }
}
#endif
