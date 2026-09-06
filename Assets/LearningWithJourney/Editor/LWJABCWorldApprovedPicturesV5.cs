#if UNITY_EDITOR
using System.IO;
using LearningWithJourney.Games;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LearningWithJourney.EditorTools
{
    public static class LWJABCWorldApprovedPicturesV5
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/ABCWorld.unity";
        const string ApprovedFolder = "Assets/LearningWithJourney/Art/ABC/ApprovedPictures";

        static readonly string[] PictureFiles =
        {
            "A_Apple.png",
            "B_Ball.png",
            "C_Cat.png",
            "D_Dog.png",
            "E_Elephant.png",
            "F_Fish.png",
            "G_Grapes.png",
            "H_Hat.png",
            "I_Ice_Cream.png",
            "J_Juice.png",
            "K_Kite.png",
            "L_Lion.png",
            "M_Moon.png",
            "N_Nest.png",
            "O_Owl.png",
            "P_Pig.png",
            "Q_Queen.png",
            "R_Rainbow.png",
            "S_Sun.png",
            "T_Turtle.png",
            "U_Umbrella.png",
            "V_Violin.png",
            "W_Watermelon.png",
            "X_Xylophone.png",
            "Y_Yo_Yo.png",
            "Z_Zebra.png"
        };

        [MenuItem("Learning with Journey/Replace ABC Pictures with Approved Art V5")]
        public static void Apply()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "ABCWorld.unity was not found. Build ABC World V1 first.",
                    "OK");
                return;
            }

            if (!Directory.Exists(ApprovedFolder))
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "The approved ABC picture folder is missing. Add the approved picture asset pack to:\n\n" + ApprovedFolder,
                    "OK");
                return;
            }

            var sprites = new Sprite[26];
            var missing = new System.Collections.Generic.List<string>();

            for (int i = 0; i < PictureFiles.Length; i++)
            {
                string path = ApprovedFolder + "/" + PictureFiles[i];
                if (!File.Exists(path))
                {
                    missing.Add(PictureFiles[i]);
                    continue;
                }

                ConfigureImporter(path);
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprites[i] == null)
                    missing.Add(PictureFiles[i]);
            }

            if (missing.Count > 0)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "The approved picture set is incomplete. Missing:\n\n" + string.Join("\n", missing),
                    "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var pictureArea = GameObject.Find("WordPictureArea");
            if (pictureArea == null)
            {
                // Build the existing picture/voice UI first, then immediately replace its
                // generated placeholder artwork with the approved preschool artwork.
                LWJABCWorldPicturesVoiceV4.Apply();
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                pictureArea = GameObject.Find("WordPictureArea");
            }

            if (pictureArea == null)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "The ABC picture area could not be found or created.",
                    "OK");
                return;
            }

            var visual = pictureArea.GetComponent<ABCWordPictureVisual>();
            if (visual == null)
                visual = pictureArea.AddComponent<ABCWordPictureVisual>();

            var pictureImage = pictureArea.transform.Find("Picture")?.GetComponent<UnityEngine.UI.Image>();
            if (pictureImage == null)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "The Picture image inside WordPictureArea was not found.",
                    "OK");
                return;
            }

            pictureImage.preserveAspect = true;
            pictureImage.color = Color.white;
            pictureImage.raycastTarget = false;

            var so = new SerializedObject(visual);
            so.FindProperty("pictureImage").objectReferenceValue = pictureImage;
            var array = so.FindProperty("pictures");
            array.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            // Make the approved object picture as large and readable as possible inside
            // the existing card while preserving the current ABC World layout.
            var pictureRect = pictureImage.rectTransform;
            pictureRect.anchorMin = new Vector2(.035f, .10f);
            pictureRect.anchorMax = new Vector2(.965f, .94f);
            pictureRect.offsetMin = Vector2.zero;
            pictureRect.offsetMax = Vector2.zero;
            pictureRect.localScale = Vector3.one;
            pictureRect.localRotation = Quaternion.identity;

            visual.Show(0);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "ABC World V5 is ready. The old generated placeholder shapes have been replaced with the approved clear A-Z preschool pictures. The letter, picture, word, Journey speech controls, Levels, Points, backpack placement, and current ABC layout were kept.",
                "OK");
        }

        static void ConfigureImporter(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
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
            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }
            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }
            if (importer.textureCompression != TextureImporterCompression.CompressedHQ)
            {
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
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

            if (changed)
                importer.SaveAndReimport();
        }
    }
}
#endif
