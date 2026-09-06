#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJAlphabetMatchCardBackRepairV4
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/AlphabetMatchWorld.unity";
        const string CardBackPath = "Assets/LearningWithJourney/Art/AlphabetMatch/LWJ_Match_Card_Back.jpg";
        const string LogoObjectName = "LearningWithJourneyCardBackLogo";

        [MenuItem("Learning with Journey/Repair + Apply Match Card Backs V4")]
        public static void Apply()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("Learning with Journey", "AlphabetMatchWorld.unity was not found. Build Alphabet Match World first.", "OK");
                return;
            }

            if (!File.Exists(CardBackPath))
            {
                EditorUtility.DisplayDialog("Learning with Journey", "The Learning with Journey card-back image is missing from the project.", "OK");
                return;
            }

            AssetDatabase.ImportAsset(CardBackPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(CardBackPath) as TextureImporter;
            if (importer == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Unity can see the card-back file, but no TextureImporter was created. Re-pull the project and run this repair again.", "OK");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 512;
            importer.SaveAndReimport();
            AssetDatabase.ImportAsset(CardBackPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            Sprite cardBackSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CardBackPath);
            if (cardBackSprite == null)
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath(CardBackPath);
                foreach (var asset in assets)
                {
                    if (asset is Sprite sprite)
                    {
                        cardBackSprite = sprite;
                        break;
                    }
                }
            }

            if (cardBackSprite == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "The card-back image file is still unreadable by Unity. Re-pull the project, let Unity finish importing, and run V4 again.", "OK");
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

                var oldLogo = back.Find(LogoObjectName);
                if (oldLogo != null) Object.DestroyImmediate(oldLogo.gameObject);

                var logoGo = new GameObject(LogoObjectName, typeof(RectTransform), typeof(Image));
                logoGo.transform.SetParent(back, false);
                var rect = (RectTransform)logoGo.transform;
                rect.anchorMin = new Vector2(.025f, .025f);
                rect.anchorMax = new Vector2(.975f, .975f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                var image = logoGo.GetComponent<Image>();
                image.sprite = cardBackSprite;
                image.preserveAspect = false;
                image.color = Color.white;
                image.raycastTarget = false;
                logoGo.transform.SetAsLastSibling();
                updated++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Learning with Journey", "Repair complete. " + updated + " matching cards now use the Learning with Journey logo card back.", "OK");
        }

        static void SetChildActive(Transform parent, string childName, bool value)
        {
            var child = parent.Find(childName);
            if (child != null) child.gameObject.SetActive(value);
        }
    }
}
#endif
