#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using LearningWithJourney.Games;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJABCWorldPicturesVoiceV4
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/ABCWorld.unity";
        const string PictureFolder = "Assets/LearningWithJourney/Generated/ABC/Pictures";
        const string RoundedPath = "Assets/LearningWithJourney/Generated/ABC/Rounded.png";

        static readonly string[] Words =
        {
            "Apple", "Ball", "Cat", "Dog", "Elephant", "Fish", "Grapes", "Hat", "Ice Cream",
            "Juice", "Kite", "Lion", "Moon", "Nest", "Owl", "Pig", "Queen", "Rainbow",
            "Sun", "Turtle", "Umbrella", "Violin", "Watermelon", "Xylophone", "Yo-Yo", "Zebra"
        };

        static readonly string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        static Sprite rounded;

        [MenuItem("Learning with Journey/Add ABC Pictures + Journey Voice V4")]
        public static void Apply()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("Learning with Journey", "ABCWorld.unity was not found. Build ABC World V1 first.", "OK");
                return;
            }

            GeneratePictureSprites();
            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedPath);

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var board = GameObject.Find("LetterBoard");
            var controller = GameObject.Find("ABCWorldController")?.GetComponent<ABCWorldPlayControllerV1>();

            if (board == null || controller == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "ABC activity board or controller was not found. Build ABC World V1 first.", "OK");
                return;
            }

            var letterHalo = board.transform.Find("LetterHalo")?.gameObject;
            var wordText = board.transform.Find("WordText")?.GetComponent<TMP_Text>();

            if (letterHalo != null && letterHalo.transform is RectTransform letterRect)
            {
                letterRect.anchorMin = new Vector2(.07f, .36f);
                letterRect.anchorMax = new Vector2(.43f, .84f);
                letterRect.offsetMin = Vector2.zero;
                letterRect.offsetMax = Vector2.zero;

                var letterImage = letterHalo.GetComponent<Image>();
                if (letterImage != null) letterImage.raycastTarget = true;
            }

            var focusLetter = letterHalo?.transform.Find("FocusLetter")?.GetComponent<TMP_Text>();
            if (focusLetter != null)
            {
                focusLetter.fontSizeMax = 126f;
                focusLetter.fontSizeMin = 42f;
            }

            if (wordText != null)
            {
                var wr = wordText.rectTransform;
                wr.anchorMin = new Vector2(.06f, .045f);
                wr.anchorMax = new Vector2(.94f, .19f);
                wr.offsetMin = Vector2.zero;
                wr.offsetMax = Vector2.zero;
                wordText.fontSize = 31f;
                wordText.fontStyle = FontStyles.Bold;
                wordText.enableAutoSizing = true;
                wordText.fontSizeMin = 22f;
                wordText.fontSizeMax = 34f;
            }

            DestroyIfFound(board.transform, "WordPictureArea");
            DestroyIfFound(board.transform, "HearAgainButton");

            var pictureArea = CreatePanel(board.transform, "WordPictureArea",
                new Vector2(.49f, .34f), new Vector2(.93f, .84f), Hex("F7F1FF"));
            AddOutline(pictureArea.gameObject, Hex("D1BDF5"), new Vector2(2f, -2f));

            var pictureGo = new GameObject("Picture", typeof(RectTransform), typeof(Image));
            pictureGo.transform.SetParent(pictureArea.transform, false);
            var pictureRect = (RectTransform)pictureGo.transform;
            pictureRect.anchorMin = new Vector2(.08f, .12f);
            pictureRect.anchorMax = new Vector2(.92f, .92f);
            pictureRect.offsetMin = Vector2.zero;
            pictureRect.offsetMax = Vector2.zero;
            var pictureImage = pictureGo.GetComponent<Image>();
            pictureImage.preserveAspect = true;
            pictureImage.raycastTarget = false;

            var hint = CreateText(pictureArea.transform, "TapHint", "TAP PICTURE TO HEAR", 17f,
                FontStyles.Bold, Hex("68478F"), new Vector2(.04f, .01f), new Vector2(.96f, .13f));
            hint.alignment = TextAlignmentOptions.Center;

            var pictureButton = pictureArea.gameObject.AddComponent<Button>();
            pictureArea.raycastTarget = true;
            pictureButton.targetGraphic = pictureArea;
            pictureButton.transition = Selectable.Transition.ColorTint;

            var visual = pictureArea.gameObject.AddComponent<ABCWordPictureVisual>();
            var visualSo = new SerializedObject(visual);
            visualSo.FindProperty("pictureImage").objectReferenceValue = pictureImage;
            var sprites = new UnityEngine.Object[26];
            for (int i = 0; i < 26; i++)
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(PicturePath(i));
            SetObjectArray(visualSo.FindProperty("pictures"), sprites);
            visualSo.ApplyModifiedPropertiesWithoutUndo();

            Button letterButton = null;
            if (letterHalo != null)
            {
                letterButton = letterHalo.GetComponent<Button>();
                if (letterButton == null) letterButton = letterHalo.AddComponent<Button>();
                letterButton.targetGraphic = letterHalo.GetComponent<Image>();
                letterButton.transition = Selectable.Transition.ColorTint;

                var tapLetter = CreateText(letterHalo.transform, "TapLetterHint", "TAP LETTER", 16f,
                    FontStyles.Bold, Hex("6A36BB"), new Vector2(.10f, .02f), new Vector2(.90f, .16f));
                tapLetter.alignment = TextAlignmentOptions.Center;
            }

            var phraseButton = CreateButton(board.transform, "HearAgainButton", "HEAR IT AGAIN",
                new Vector2(.29f, .205f), new Vector2(.71f, .305f), Hex("8A61D2"), Hex("54318E"), 20f);

            var controllerGo = controller.gameObject;
            var audio = controllerGo.GetComponent<AudioSource>();
            if (audio == null) audio = controllerGo.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.loop = false;
            audio.spatialBlend = 0f;

            var speech = controllerGo.GetComponent<JourneyABCSpeech>();
            if (speech == null) speech = controllerGo.AddComponent<JourneyABCSpeech>();
            WireSpeechAssets(speech, audio);

            var controllerSo = new SerializedObject(controller);
            controllerSo.FindProperty("pictureVisual").objectReferenceValue = visual;
            controllerSo.FindProperty("letterRepeatButton").objectReferenceValue = letterButton;
            controllerSo.FindProperty("pictureRepeatButton").objectReferenceValue = pictureButton;
            controllerSo.FindProperty("phraseRepeatButton").objectReferenceValue = phraseButton;
            controllerSo.FindProperty("journeySpeech").objectReferenceValue = speech;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "ABC World V4 is ready. Every A-Z word now has a matching picture. Children can tap the letter to hear the letter, tap the picture to hear the word, or tap HEAR IT AGAIN for the full phrase such as F is for Fish. Journey voice clips are used when present; the Android build can use device speech as a fallback until final recorded voice clips are added.",
                "OK");
        }

        static void WireSpeechAssets(JourneyABCSpeech speech, AudioSource audio)
        {
            var so = new SerializedObject(speech);
            so.FindProperty("audioSource").objectReferenceValue = audio;

            var letters = new UnityEngine.Object[26];
            var words = new UnityEngine.Object[26];
            var phrases = new UnityEngine.Object[26];

            for (int i = 0; i < 26; i++)
            {
                string letter = Alphabet[i].ToString();
                string word = Words[i];
                letters[i] = FindAudioClip(new[] { "Journey Letter " + letter, "Letter " + letter, "Journey_" + letter });
                words[i] = FindAudioClip(new[] { "Journey " + word, "Journey_" + SafeName(word), word });
                phrases[i] = FindAudioClip(new[]
                {
                    "Journey " + letter + " is for " + word,
                    "Journey_" + letter + "_is_for_" + SafeName(word),
                    letter + " is for " + word
                });
            }

            SetObjectArray(so.FindProperty("letterClips"), letters);
            SetObjectArray(so.FindProperty("wordClips"), words);
            SetObjectArray(so.FindProperty("phraseClips"), phrases);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static AudioClip FindAudioClip(string[] searches)
        {
            foreach (string search in searches)
            {
                string[] guids = AssetDatabase.FindAssets(search + " t:AudioClip");
                foreach (string guid in guids)
                {
                    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));
                    if (clip != null) return clip;
                }
            }
            return null;
        }

        static string SafeName(string value) => value.Replace(" ", "_").Replace("-", "_");

        static void GeneratePictureSprites()
        {
            Directory.CreateDirectory(PictureFolder);

            for (int i = 0; i < 26; i++)
            {
                string path = PicturePath(i);
                var tex = new Texture2D(512, 512, TextureFormat.RGBA32, false);
                Clear(tex);
                DrawPicture(tex, i);
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(tex);
            }

            AssetDatabase.Refresh();

            for (int i = 0; i < 26; i++)
            {
                string path = PicturePath(i);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spritePixelsPerUnit = 100f;
                importer.SaveAndReimport();
            }
        }

        static string PicturePath(int index) => $"{PictureFolder}/{Alphabet[index]}_{SafeName(Words[index])}.png";

        static void DrawPicture(Texture2D t, int i)
        {
            Color red = Hex("EF476F");
            Color pink = Hex("F78FB3");
            Color orange = Hex("FFA62B");
            Color yellow = Hex("FFD166");
            Color green = Hex("65B96E");
            Color blue = Hex("4CB7E8");
            Color purple = Hex("8056C5");
            Color brown = Hex("8A5A3B");
            Color dark = Hex("493266");
            Color cream = Hex("FFF1D6");
            Color gray = Hex("A9A7B6");
            Color white = Color.white;
            Color black = Hex("25202B");

            switch (i)
            {
                case 0: // Apple
                    FillEllipse(t, 225, 250, 105, 120, red); FillEllipse(t, 300, 250, 105, 120, red);
                    FillRect(t, 250, 345, 275, 430, brown); FillEllipse(t, 315, 395, 60, 28, green); break;
                case 1: // Ball
                    FillCircle(t, 256, 255, 150, blue); DrawLine(t, 120, 250, 392, 250, 18, white);
                    DrawLine(t, 256, 108, 256, 402, 18, yellow); DrawLine(t, 150, 150, 362, 360, 16, red); break;
                case 2: // Cat
                    FillCircle(t, 256, 270, 135, gray); FillTriangle(t, 145, 195, 180, 75, 235, 185, gray);
                    FillTriangle(t, 277, 185, 335, 75, 370, 195, gray); Face(t, 215, 290, 297, 290, 256, 330, black, pink);
                    DrawLine(t, 145, 325, 225, 315, 7, dark); DrawLine(t, 287, 315, 370, 325, 7, dark); break;
                case 3: // Dog
                    FillCircle(t, 256, 270, 125, brown); FillEllipse(t, 140, 260, 55, 115, Hex("6B422A")); FillEllipse(t, 372, 260, 55, 115, Hex("6B422A"));
                    FillEllipse(t, 256, 330, 70, 50, cream); Face(t, 215, 270, 297, 270, 256, 325, black, black); break;
                case 4: // Elephant
                    FillEllipse(t, 250, 285, 150, 120, gray); FillEllipse(t, 120, 280, 75, 110, Hex("C4C1CC")); FillEllipse(t, 380, 280, 75, 110, Hex("C4C1CC"));
                    FillRect(t, 235, 305, 278, 455, gray); FillCircle(t, 210, 260, 13, black); FillCircle(t, 300, 260, 13, black); break;
                case 5: // Fish
                    FillEllipse(t, 235, 270, 150, 90, blue); FillTriangle(t, 350, 270, 470, 175, 470, 365, orange);
                    FillCircle(t, 155, 245, 15, white); FillCircle(t, 155, 245, 7, black); DrawLine(t, 245, 205, 285, 335, 10, white); break;
                case 6: // Grapes
                    for (int y = 0; y < 4; y++) for (int x = 0; x < 4 - y; x++) FillCircle(t, 205 + x * 42 + y * 20, 190 + y * 55, 34, purple);
                    FillRect(t, 250, 355, 270, 430, brown); FillEllipse(t, 320, 385, 75, 30, green); break;
                case 7: // Hat
                    FillRect(t, 150, 180, 365, 310, purple); FillEllipse(t, 256, 300, 170, 50, purple); FillRect(t, 160, 245, 355, 270, pink); break;
                case 8: // Ice Cream
                    FillTriangle(t, 190, 270, 325, 270, 255, 465, Hex("D9A15E")); FillCircle(t, 220, 220, 72, pink); FillCircle(t, 290, 210, 72, cream); FillCircle(t, 255, 150, 70, blue); break;
                case 9: // Juice
                    FillRect(t, 165, 150, 350, 405, orange); FillRect(t, 190, 180, 325, 375, Hex("FFD27A")); DrawLine(t, 310, 140, 375, 65, 14, pink); FillCircle(t, 255, 275, 42, red); break;
                case 10: // Kite
                    FillTriangle(t, 255, 75, 395, 230, 255, 250, pink); FillTriangle(t, 255, 75, 115, 230, 255, 250, orange);
                    FillTriangle(t, 255, 250, 395, 230, 255, 390, blue); FillTriangle(t, 255, 250, 115, 230, 255, 390, green); DrawLine(t, 255, 390, 310, 480, 8, dark); break;
                case 11: // Lion
                    FillCircle(t, 256, 265, 160, orange); FillCircle(t, 256, 270, 115, Hex("F3C16F")); Face(t, 215, 260, 300, 260, 257, 315, black, brown); break;
                case 12: // Moon
                    FillCircle(t, 245, 255, 155, yellow); FillCircle(t, 320, 205, 145, Color.clear); FillCircle(t, 220, 255, 10, dark); FillEllipse(t, 215, 315, 35, 14, dark); break;
                case 13: // Nest
                    for (int k = 0; k < 10; k++) DrawLine(t, 120, 300 + k * 8, 390, 265 + k * 8, 9, k % 2 == 0 ? brown : Hex("B77A4D"));
                    FillEllipse(t, 205, 250, 42, 58, cream); FillEllipse(t, 260, 235, 42, 58, white); FillEllipse(t, 315, 250, 42, 58, cream); break;
                case 14: // Owl
                    FillEllipse(t, 256, 285, 135, 150, brown); FillCircle(t, 210, 245, 55, cream); FillCircle(t, 305, 245, 55, cream);
                    FillCircle(t, 210, 245, 20, black); FillCircle(t, 305, 245, 20, black); FillTriangle(t, 235, 295, 278, 295, 256, 335, orange); break;
                case 15: // Pig
                    FillCircle(t, 256, 270, 130, pink); FillTriangle(t, 150, 190, 180, 90, 230, 175, pink); FillTriangle(t, 282, 175, 335, 90, 365, 190, pink);
                    FillEllipse(t, 256, 325, 70, 48, Hex("F06E9E")); FillCircle(t, 235, 325, 8, dark); FillCircle(t, 278, 325, 8, dark); Face(t, 215, 260, 300, 260, 256, 325, black, Hex("F06E9E")); break;
                case 16: // Queen
                    FillCircle(t, 256, 295, 100, Hex("C6865A")); FillTriangle(t, 155, 205, 205, 80, 240, 205, yellow); FillTriangle(t, 220, 205, 256, 60, 292, 205, yellow); FillTriangle(t, 275, 205, 330, 80, 360, 205, yellow);
                    FillRect(t, 155, 180, 360, 220, yellow); Face(t, 220, 285, 292, 285, 256, 330, black, red); break;
                case 17: // Rainbow
                    DrawArcBand(t, 256, 340, 190, 145, red); DrawArcBand(t, 256, 340, 155, 115, orange); DrawArcBand(t, 256, 340, 120, 82, yellow); DrawArcBand(t, 256, 340, 88, 52, green); DrawArcBand(t, 256, 340, 58, 25, blue); break;
                case 18: // Sun
                    FillCircle(t, 256, 255, 105, yellow); for (int a = 0; a < 360; a += 30) { float r1 = 130, r2 = 195; float rad = a * Mathf.Deg2Rad; DrawLine(t, (int)(256 + Mathf.Cos(rad) * r1), (int)(255 + Mathf.Sin(rad) * r1), (int)(256 + Mathf.Cos(rad) * r2), (int)(255 + Mathf.Sin(rad) * r2), 14, orange); } break;
                case 19: // Turtle
                    FillEllipse(t, 250, 280, 145, 90, green); FillCircle(t, 390, 275, 48, Hex("8ED081")); FillCircle(t, 405, 263, 7, black);
                    FillEllipse(t, 145, 350, 40, 28, Hex("8ED081")); FillEllipse(t, 295, 355, 40, 28, Hex("8ED081")); FillEllipse(t, 185, 215, 40, 28, Hex("8ED081")); break;
                case 20: // Umbrella
                    FillSemiEllipse(t, 256, 235, 175, 110, pink); DrawLine(t, 256, 235, 256, 420, 14, dark); DrawLine(t, 256, 420, 310, 450, 14, dark); break;
                case 21: // Violin
                    FillEllipse(t, 235, 330, 80, 95, brown); FillEllipse(t, 275, 240, 72, 88, Hex("B86B3D")); FillRect(t, 247, 95, 270, 250, brown); FillRect(t, 220, 70, 298, 110, brown);
                    DrawLine(t, 252, 85, 252, 395, 4, cream); DrawLine(t, 266, 85, 266, 395, 4, cream); break;
                case 22: // Watermelon
                    FillSemiEllipse(t, 256, 300, 175, 125, green); FillSemiEllipse(t, 256, 285, 150, 105, red); for (int x = 175; x <= 335; x += 40) FillEllipse(t, x, 280, 8, 16, black); break;
                case 23: // Xylophone
                    Color[] bars = { red, orange, yellow, green, blue, purple }; for (int k = 0; k < 6; k++) FillRect(t, 120 + k * 48, 170 + k * 10, 155 + k * 48, 390 - k * 10, bars[k]);
                    DrawLine(t, 120, 420, 380, 100, 12, brown); DrawLine(t, 160, 440, 410, 140, 12, brown); break;
                case 24: // Yo-Yo
                    FillCircle(t, 250, 285, 120, blue); FillCircle(t, 250, 285, 72, purple); FillCircle(t, 250, 285, 22, yellow); DrawLine(t, 250, 160, 380, 80, 8, dark); break;
                case 25: // Zebra
                    FillEllipse(t, 260, 280, 120, 145, white); FillTriangle(t, 155, 200, 190, 80, 230, 190, white); FillTriangle(t, 285, 190, 330, 80, 365, 200, white);
                    for (int y = 190; y <= 355; y += 42) DrawLine(t, 155, y, 355, y + 35, 15, black); FillCircle(t, 220, 255, 12, black); FillCircle(t, 300, 255, 12, black); break;
            }
        }

        static void Face(Texture2D t, int lx, int ly, int rx, int ry, int nx, int ny, Color eye, Color nose)
        {
            FillCircle(t, lx, ly, 12, eye); FillCircle(t, rx, ry, 12, eye); FillEllipse(t, nx, ny, 18, 13, nose);
        }

        static void Clear(Texture2D t)
        {
            var pixels = Enumerable.Repeat(new Color(0f, 0f, 0f, 0f), t.width * t.height).ToArray();
            t.SetPixels(pixels);
        }

        static void FillRect(Texture2D t, int x0, int y0, int x1, int y1, Color c)
        {
            int minX = Mathf.Clamp(Mathf.Min(x0, x1), 0, t.width - 1); int maxX = Mathf.Clamp(Mathf.Max(x0, x1), 0, t.width - 1);
            int minY = Mathf.Clamp(Mathf.Min(y0, y1), 0, t.height - 1); int maxY = Mathf.Clamp(Mathf.Max(y0, y1), 0, t.height - 1);
            for (int y = minY; y <= maxY; y++) for (int x = minX; x <= maxX; x++) t.SetPixel(x, y, c);
        }

        static void FillCircle(Texture2D t, int cx, int cy, int r, Color c) => FillEllipse(t, cx, cy, r, r, c);

        static void FillEllipse(Texture2D t, int cx, int cy, int rx, int ry, Color c)
        {
            int minX = Mathf.Max(0, cx - rx); int maxX = Mathf.Min(t.width - 1, cx + rx);
            int minY = Mathf.Max(0, cy - ry); int maxY = Mathf.Min(t.height - 1, cy + ry);
            float rx2 = rx * rx; float ry2 = ry * ry;
            for (int y = minY; y <= maxY; y++)
            {
                float dy = y - cy;
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = x - cx;
                    if ((dx * dx) / rx2 + (dy * dy) / ry2 <= 1f) t.SetPixel(x, y, c);
                }
            }
        }

        static void FillSemiEllipse(Texture2D t, int cx, int cy, int rx, int ry, Color c)
        {
            int minX = Mathf.Max(0, cx - rx); int maxX = Mathf.Min(t.width - 1, cx + rx);
            int minY = cy; int maxY = Mathf.Min(t.height - 1, cy + ry);
            float rx2 = rx * rx; float ry2 = ry * ry;
            for (int y = minY; y <= maxY; y++)
            {
                float dy = y - cy;
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = x - cx;
                    if ((dx * dx) / rx2 + (dy * dy) / ry2 <= 1f) t.SetPixel(x, y, c);
                }
            }
        }

        static void FillTriangle(Texture2D t, int x1, int y1, int x2, int y2, int x3, int y3, Color c)
        {
            int minX = Mathf.Clamp(Mathf.Min(x1, Mathf.Min(x2, x3)), 0, t.width - 1);
            int maxX = Mathf.Clamp(Mathf.Max(x1, Mathf.Max(x2, x3)), 0, t.width - 1);
            int minY = Mathf.Clamp(Mathf.Min(y1, Mathf.Min(y2, y3)), 0, t.height - 1);
            int maxY = Mathf.Clamp(Mathf.Max(y1, Mathf.Max(y2, y3)), 0, t.height - 1);

            float area = Edge(x1, y1, x2, y2, x3, y3);
            if (Mathf.Abs(area) < .001f) return;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float w0 = Edge(x2, y2, x3, y3, x, y);
                    float w1 = Edge(x3, y3, x1, y1, x, y);
                    float w2 = Edge(x1, y1, x2, y2, x, y);
                    bool hasNeg = w0 < 0 || w1 < 0 || w2 < 0;
                    bool hasPos = w0 > 0 || w1 > 0 || w2 > 0;
                    if (!(hasNeg && hasPos)) t.SetPixel(x, y, c);
                }
            }
        }

        static float Edge(float ax, float ay, float bx, float by, float px, float py) => (px - ax) * (by - ay) - (py - ay) * (bx - ax);

        static void DrawLine(Texture2D t, int x0, int y0, int x1, int y1, int thickness, Color c)
        {
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            while (true)
            {
                FillCircle(t, x0, y0, Mathf.Max(1, thickness / 2), c);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        static void DrawArcBand(Texture2D t, int cx, int cy, int outer, int inner, Color c)
        {
            int minX = Mathf.Max(0, cx - outer); int maxX = Mathf.Min(t.width - 1, cx + outer);
            int minY = cy; int maxY = Mathf.Min(t.height - 1, cy + outer);
            int o2 = outer * outer, i2 = inner * inner;
            for (int y = minY; y <= maxY; y++)
            {
                int dy = y - cy;
                for (int x = minX; x <= maxX; x++)
                {
                    int dx = x - cx; int d2 = dx * dx + dy * dy;
                    if (d2 <= o2 && d2 >= i2) t.SetPixel(x, y, c);
                }
            }
        }

        static Image CreatePanel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.sprite = rounded;
            image.type = rounded != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static Button CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max, Color main, Color depth, float size)
        {
            var shadow = CreatePanel(parent, name + "Shadow", min + new Vector2(0f, -.008f), max + new Vector2(0f, -.008f), depth);
            var panel = CreatePanel(parent, name, min, max, main);
            panel.raycastTarget = true;
            AddOutline(panel.gameObject, Color.white, new Vector2(2f, -2f));
            var text = CreateText(panel.transform, "Label", label, size, FontStyles.Bold, Color.white, new Vector2(.05f, .05f), new Vector2(.95f, .95f));
            text.alignment = TextAlignmentOptions.Center;
            var button = panel.gameObject.AddComponent<Button>();
            button.targetGraphic = panel;
            button.transition = Selectable.Transition.ColorTint;
            shadow.raycastTarget = false;
            return button;
        }

        static TMP_Text CreateText(Transform parent, string name, string value, float size, FontStyles style, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = value; text.fontSize = size; text.fontStyle = style; text.color = color;
            text.enableWordWrapping = false; text.extraPadding = true; text.raycastTarget = false;
            return text;
        }

        static void AddOutline(GameObject go, Color color, Vector2 distance)
        {
            var outline = go.GetComponent<Outline>();
            if (outline == null) outline = go.AddComponent<Outline>();
            outline.effectColor = color; outline.effectDistance = distance; outline.useGraphicAlpha = true;
        }

        static void SetObjectArray(SerializedProperty property, UnityEngine.Object[] values)
        {
            if (property == null) return;
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        static void DestroyIfFound(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
            var shadow = parent.Find(name + "Shadow");
            if (shadow != null) UnityEngine.Object.DestroyImmediate(shadow.gameObject);
        }

        static Color Hex(string hex)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
    }
}
#endif
