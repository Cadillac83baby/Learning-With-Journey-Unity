#if UNITY_EDITOR
using System.IO;
using System.Linq;
using LearningWithJourney.Games;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJCountingWorldObjectVarietyV8
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/CountingWorld.unity";
        const string RoundedPath = "Assets/LearningWithJourney/Generated/Counting/Rounded.png";
        const string CirclePath = "Assets/LearningWithJourney/Generated/Counting/Circle.png";

        static Sprite rounded;
        static Sprite circle;

        static readonly string[] SingularNames =
        {
            "apple", "orange", "ball", "balloon", "flower",
            "block", "cookie", "sun", "berry", "lollipop"
        };

        static readonly string[] PluralNames =
        {
            "apples", "oranges", "balls", "balloons", "flowers",
            "blocks", "cookies", "suns", "berries", "lollipops"
        };

        [MenuItem("Learning with Journey/Add Different Counting Objects V8")]
        public static void Apply()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("Learning with Journey", "CountingWorld.unity was not found.", "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var grid = GameObject.Find("ObjectGrid");
            if (grid == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "ObjectGrid was not found.", "OK");
                return;
            }

            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedPath);
            circle = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);
            if (rounded == null || circle == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Counting UI sprites are missing. Run Rebuild Counting World V2 first.", "OK");
                return;
            }

            var countObjects = grid.transform.Cast<Transform>()
                .Select(t => t.gameObject)
                .Where(go => go.name.StartsWith("Apple"))
                .Take(20)
                .ToArray();

            foreach (var countObject in countObjects)
                UpgradeCountObject(countObject);

            var prompt = GameObject.Find("PromptText")?.GetComponent<TMPro.TMP_Text>();
            if (prompt != null)
                prompt.text = "Touch the objects and count with Journey!";

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Counting World now changes the objects each round: apples, oranges, balls, balloons, flowers, blocks, cookies, suns, berries, and lollipops. Every object remains touchable and numbered when counted. Journey, her backpack, the garden background, levels, and speech-bubble placement were not changed.",
                "OK");
        }

        static void UpgradeCountObject(GameObject countObject)
        {
            if (countObject == null) return;

            string[] legacy = { "Body", "BodySide", "Stem", "Leaf", "Shine" };
            foreach (string childName in legacy)
            {
                var child = countObject.transform.Find(childName);
                if (child != null) child.gameObject.SetActive(false);
            }

            var oldThemes = countObject.transform.Find("V8ThemeVisuals");
            if (oldThemes != null) Object.DestroyImmediate(oldThemes.gameObject);

            var hitImage = countObject.GetComponent<Image>();
            if (hitImage == null) hitImage = countObject.AddComponent<Image>();
            hitImage.sprite = rounded;
            hitImage.type = Image.Type.Sliced;
            hitImage.color = new Color(1f, 1f, 1f, 0f);
            hitImage.raycastTarget = true;

            var button = countObject.GetComponent<Button>();
            if (button == null) button = countObject.AddComponent<Button>();
            button.targetGraphic = hitImage;
            button.transition = Selectable.Transition.None;

            var themesRoot = CreateRoot(countObject.transform, "V8ThemeVisuals");
            themesRoot.transform.SetSiblingIndex(0);

            var themeRoots = new GameObject[10];
            themeRoots[0] = BuildApple(themesRoot.transform);
            themeRoots[1] = BuildOrange(themesRoot.transform);
            themeRoots[2] = BuildBall(themesRoot.transform);
            themeRoots[3] = BuildBalloon(themesRoot.transform);
            themeRoots[4] = BuildFlower(themesRoot.transform);
            themeRoots[5] = BuildBlock(themesRoot.transform);
            themeRoots[6] = BuildCookie(themesRoot.transform);
            themeRoots[7] = BuildSun(themesRoot.transform);
            themeRoots[8] = BuildBerry(themesRoot.transform);
            themeRoots[9] = BuildLollipop(themesRoot.transform);

            for (int i = 0; i < themeRoots.Length; i++)
                themeRoots[i].SetActive(i == 0);

            var badge = countObject.transform.Find("V3CountBadge");
            if (badge != null) badge.SetAsLastSibling();

            var theme = countObject.GetComponent<CountingObjectVisualTheme>();
            if (theme == null) theme = countObject.AddComponent<CountingObjectVisualTheme>();

            var so = new SerializedObject(theme);
            SetObjectArray(so.FindProperty("themeRoots"), themeRoots.Cast<Object>().ToArray());
            SetStringArray(so.FindProperty("singularNames"), SingularNames);
            SetStringArray(so.FindProperty("pluralNames"), PluralNames);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject BuildApple(Transform parent)
        {
            var root = CreateRoot(parent, "AppleTheme");
            AddCircle(root.transform, "Body", .12f, .10f, .88f, .84f, Hex("F04478"));
            AddCircle(root.transform, "Side", .43f, .12f, .92f, .82f, Hex("EF4A91"));
            AddRect(root.transform, "Stem", .48f, .73f, .56f, .94f, Hex("7A4A2A"), -5f);
            AddRect(root.transform, "Leaf", .56f, .72f, .79f, .88f, Hex("67BA4B"), -28f);
            AddCircle(root.transform, "Shine", .25f, .55f, .40f, .70f, new Color(1f,1f,1f,.42f));
            return root;
        }

        static GameObject BuildOrange(Transform parent)
        {
            var root = CreateRoot(parent, "OrangeTheme");
            AddCircle(root.transform, "Fruit", .12f, .10f, .88f, .86f, Hex("FF922B"));
            AddCircle(root.transform, "WarmSide", .47f, .14f, .90f, .79f, Hex("F57820"));
            AddRect(root.transform, "Leaf", .49f, .74f, .75f, .89f, Hex("5FB84C"), -22f);
            AddCircle(root.transform, "Shine", .26f, .57f, .39f, .70f, new Color(1f,1f,1f,.42f));
            return root;
        }

        static GameObject BuildBall(Transform parent)
        {
            var root = CreateRoot(parent, "BallTheme");
            AddCircle(root.transform, "Ball", .10f, .08f, .90f, .88f, Hex("32B9E8"));
            AddRect(root.transform, "Stripe1", .17f, .42f, .83f, .54f, Hex("FFD34E"), 16f);
            AddRect(root.transform, "Stripe2", .45f, .12f, .57f, .84f, Hex("F05AA6"), -18f);
            AddCircle(root.transform, "Shine", .24f, .58f, .39f, .73f, new Color(1f,1f,1f,.44f));
            return root;
        }

        static GameObject BuildBalloon(Transform parent)
        {
            var root = CreateRoot(parent, "BalloonTheme");
            AddCircle(root.transform, "Balloon", .17f, .22f, .83f, .91f, Hex("A84DD6"));
            AddCircle(root.transform, "Shine", .30f, .63f, .43f, .77f, new Color(1f,1f,1f,.42f));
            AddRect(root.transform, "Knot", .46f, .17f, .54f, .29f, Hex("7E2FA7"), 0f);
            AddRect(root.transform, "String", .49f, .00f, .515f, .20f, Hex("795D70"), 7f);
            return root;
        }

        static GameObject BuildFlower(Transform parent)
        {
            var root = CreateRoot(parent, "FlowerTheme");
            AddRect(root.transform, "Stem", .47f, .05f, .54f, .50f, Hex("52A64A"), 0f);
            AddRect(root.transform, "Leaf", .35f, .18f, .53f, .32f, Hex("67BA4B"), 25f);
            AddCircle(root.transform, "PetalTop", .34f, .55f, .66f, .88f, Hex("F05AA6"));
            AddCircle(root.transform, "PetalLeft", .17f, .39f, .51f, .72f, Hex("EE79B8"));
            AddCircle(root.transform, "PetalRight", .49f, .39f, .83f, .72f, Hex("EE79B8"));
            AddCircle(root.transform, "PetalBottom", .34f, .27f, .66f, .60f, Hex("F05AA6"));
            AddCircle(root.transform, "Center", .39f, .44f, .61f, .66f, Hex("FFD44D"));
            return root;
        }

        static GameObject BuildBlock(Transform parent)
        {
            var root = CreateRoot(parent, "BlockTheme");
            AddRounded(root.transform, "Block", .14f, .14f, .86f, .86f, Hex("7A4BCC"));
            AddRounded(root.transform, "Inset", .24f, .25f, .76f, .75f, Hex("9A6BE1"));
            AddRect(root.transform, "Highlight", .24f, .67f, .76f, .74f, new Color(1f,1f,1f,.28f), 0f);
            return root;
        }

        static GameObject BuildCookie(Transform parent)
        {
            var root = CreateRoot(parent, "CookieTheme");
            AddCircle(root.transform, "Cookie", .10f, .08f, .90f, .88f, Hex("D99A59"));
            AddCircle(root.transform, "Chip1", .27f, .56f, .38f, .67f, Hex("65402F"));
            AddCircle(root.transform, "Chip2", .55f, .61f, .66f, .72f, Hex("65402F"));
            AddCircle(root.transform, "Chip3", .44f, .35f, .55f, .46f, Hex("65402F"));
            AddCircle(root.transform, "Chip4", .63f, .28f, .74f, .39f, Hex("65402F"));
            AddCircle(root.transform, "Chip5", .25f, .28f, .36f, .39f, Hex("65402F"));
            return root;
        }

        static GameObject BuildSun(Transform parent)
        {
            var root = CreateRoot(parent, "SunTheme");
            AddRect(root.transform, "Ray1", .47f, .02f, .54f, .98f, Hex("FFC83D"), 0f);
            AddRect(root.transform, "Ray2", .47f, .02f, .54f, .98f, Hex("FFC83D"), 45f);
            AddRect(root.transform, "Ray3", .47f, .02f, .54f, .98f, Hex("FFC83D"), 90f);
            AddRect(root.transform, "Ray4", .47f, .02f, .54f, .98f, Hex("FFC83D"), 135f);
            AddCircle(root.transform, "Sun", .18f, .18f, .82f, .82f, Hex("FFD84F"));
            AddCircle(root.transform, "Shine", .30f, .57f, .43f, .70f, new Color(1f,1f,1f,.35f));
            return root;
        }

        static GameObject BuildBerry(Transform parent)
        {
            var root = CreateRoot(parent, "BerryTheme");
            AddCircle(root.transform, "Berry1", .18f, .22f, .62f, .68f, Hex("8E3BC1"));
            AddCircle(root.transform, "Berry2", .40f, .22f, .84f, .68f, Hex("A94BD3"));
            AddCircle(root.transform, "BerryTop", .29f, .43f, .73f, .86f, Hex("9C43CB"));
            AddRect(root.transform, "Leaf", .34f, .75f, .66f, .89f, Hex("66B750"), 0f);
            AddCircle(root.transform, "Shine", .34f, .63f, .43f, .72f, new Color(1f,1f,1f,.38f));
            return root;
        }

        static GameObject BuildLollipop(Transform parent)
        {
            var root = CreateRoot(parent, "LollipopTheme");
            AddRect(root.transform, "Stick", .47f, .02f, .54f, .50f, Hex("F2E5D0"), 0f);
            AddCircle(root.transform, "Candy", .16f, .39f, .84f, .98f, Hex("EF4E9B"));
            AddCircle(root.transform, "CandyInner", .28f, .50f, .72f, .89f, Hex("7B4BCB"));
            AddCircle(root.transform, "CandyCenter", .39f, .60f, .61f, .80f, Hex("35BCE4"));
            AddCircle(root.transform, "Shine", .28f, .72f, .39f, .83f, new Color(1f,1f,1f,.40f));
            return root;
        }

        static GameObject CreateRoot(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return go;
        }

        static Image AddCircle(Transform parent, string name, float x1, float y1, float x2, float y2, Color color)
            => AddImage(parent, name, circle, new Vector2(x1,y1), new Vector2(x2,y2), color, 0f);

        static Image AddRounded(Transform parent, string name, float x1, float y1, float x2, float y2, Color color)
            => AddImage(parent, name, rounded, new Vector2(x1,y1), new Vector2(x2,y2), color, 0f);

        static Image AddRect(Transform parent, string name, float x1, float y1, float x2, float y2, Color color, float rotation)
            => AddImage(parent, name, rounded, new Vector2(x1,y1), new Vector2(x2,y2), color, rotation);

        static Image AddImage(Transform parent, string name, Sprite sprite, Vector2 min, Vector2 max, Color color, float rotation)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = sprite == rounded ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static void SetObjectArray(SerializedProperty property, Object[] values)
        {
            if (property == null) return;
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        static void SetStringArray(SerializedProperty property, string[] values)
        {
            if (property == null) return;
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).stringValue = values[i];
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
