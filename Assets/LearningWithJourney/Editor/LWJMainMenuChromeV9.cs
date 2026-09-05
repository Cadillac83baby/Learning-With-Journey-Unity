#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJMainMenuChromeV9
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/MainMenu.unity";
        const string RoundedPath = "Assets/LearningWithJourney/Generated/MainMenu/RoundedPanel.png";
        const string CirclePath = "Assets/LearningWithJourney/Generated/MainMenu/Circle.png";

        static Sprite rounded;
        static Sprite circle;

        [MenuItem("Learning with Journey/Apply Bottom Nav + Clean Header V9")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Main Menu Canvas was not found.", "OK");
                return;
            }

            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedPath);
            circle = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);

            RemoveTopHudBlock();
            RemoveRainbowBehindTitle();
            ModernizeBottomNavigation();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "V9 applied: the top J/stat block and rainbow circles behind the title are removed, and the bottom navigation now has modern picture-style icons.",
                "OK");
        }

        static void RemoveTopHudBlock()
        {
            // The purple header bar with J / stars / level / coins is intentionally removed.
            string[] names = { "TopHUD", "AvatarRing", "StarPill", "LevelPill", "CoinPill" };
            foreach (string name in names)
            {
                var go = GameObject.Find(name);
                if (go != null) Object.DestroyImmediate(go);
            }
        }

        static void RemoveRainbowBehindTitle()
        {
            // These overlapping translucent circles are the fish-bowl/rainbow shape visible behind the logo.
            string[] names = { "RainbowPurple", "RainbowBlue", "RainbowYellow", "RainbowPink" };
            foreach (string name in names)
            {
                var go = GameObject.Find(name);
                if (go != null) Object.DestroyImmediate(go);
            }
        }

        static void ModernizeBottomNavigation()
        {
            var back = GameObject.Find("BottomNavBack");
            if (back != null)
            {
                SetRect(back, new Vector2(.018f, .015f), new Vector2(.982f, .165f));
                var img = back.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = rounded;
                    img.type = Image.Type.Sliced;
                    img.color = Hex("4A0E82");
                }
                ClearEffects(back);
                AddShadow(back, new Vector2(0f, -10f), Hex("1B032B", .72f));
                AddOutline(back, Hex("A958E6", .86f), new Vector2(3f, -3f));
            }

            StyleNavTile("HomeTile", "HomeTileShadow", Hex("F23A96"), Hex("A81764"), NavIcon.Home, true);
            StyleNavTile("LibraryTile", "LibraryTileShadow", Hex("20B9E8"), Hex("08789E"), NavIcon.Book, false);
            StyleNavTile("RewardsTile", "RewardsTileShadow", Hex("FFA922"), Hex("C46B05"), NavIcon.Trophy, false);
            StyleNavTile("ParentsTile", "ParentsTileShadow", Hex("9342D8"), Hex("5D1C98"), NavIcon.People, false);
        }

        enum NavIcon { Home, Book, Trophy, People }

        static void StyleNavTile(string tileName, string shadowName, Color main, Color depth, NavIcon icon, bool selected)
        {
            var tile = GameObject.Find(tileName);
            if (tile == null) return;

            var image = tile.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = rounded;
                image.type = Image.Type.Sliced;
                image.color = main;
            }

            var shadow = GameObject.Find(shadowName);
            if (shadow != null)
            {
                var sImg = shadow.GetComponent<Image>();
                if (sImg != null)
                {
                    sImg.sprite = rounded;
                    sImg.type = Image.Type.Sliced;
                    sImg.color = depth;
                }
            }

            RemoveOldTileVisuals(tile.transform);
            ClearEffects(tile);
            AddShadow(tile, new Vector2(0f, -7f), new Color(depth.r, depth.g, depth.b, .62f));
            AddOutline(tile, selected ? Hex("FFF27A") : Color.white, new Vector2(selected ? 4f : 3f, selected ? -4f : -3f));

            // Top glass layer for the glossy, modern app-button treatment.
            var gloss = CreatePanel(tile.transform, "V9Gloss", rounded,
                new Vector2(.055f, .69f), new Vector2(.945f, .94f), new Color(1f, 1f, 1f, .25f));
            gloss.raycastTarget = false;

            // A raised picture badge gives every bottom button a recognizable visual instead of plain text.
            var badge = CreatePanel(tile.transform, "V9IconBadge", rounded,
                new Vector2(.18f, .35f), new Vector2(.82f, .86f), new Color(1f, 1f, 1f, .96f));
            AddShadow(badge.gameObject, new Vector2(0f, -4f), new Color(depth.r, depth.g, depth.b, .30f));
            AddOutline(badge.gameObject, new Color(1f, 1f, 1f, .90f), new Vector2(2f, -2f));
            badge.raycastTarget = false;

            BuildPictureIcon(badge.transform, icon, main, depth);

            var title = tile.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (title != null)
            {
                title.fontSize = 18f;
                title.fontStyle = FontStyles.Bold;
                title.color = Color.white;
                title.alignment = TextAlignmentOptions.Center;
                title.rectTransform.anchorMin = new Vector2(.04f, .055f);
                title.rectTransform.anchorMax = new Vector2(.96f, .29f);
                title.rectTransform.offsetMin = Vector2.zero;
                title.rectTransform.offsetMax = Vector2.zero;
                title.outlineColor = new Color(depth.r, depth.g, depth.b, .60f);
                title.outlineWidth = .06f;
            }
        }

        static void RemoveOldTileVisuals(Transform tile)
        {
            string[] remove = { "Icon", "Gloss", "V9Gloss", "V9IconBadge" };
            foreach (string n in remove)
            {
                var child = tile.Find(n);
                if (child != null) Object.DestroyImmediate(child.gameObject);
            }
        }

        static void BuildPictureIcon(Transform parent, NavIcon icon, Color main, Color depth)
        {
            switch (icon)
            {
                case NavIcon.Home:
                    BuildHome(parent, main, depth);
                    break;
                case NavIcon.Book:
                    BuildBook(parent, main, depth);
                    break;
                case NavIcon.Trophy:
                    BuildTrophy(parent, main, depth);
                    break;
                case NavIcon.People:
                    BuildPeople(parent, main, depth);
                    break;
            }
        }

        static void BuildHome(Transform parent, Color main, Color depth)
        {
            var body = CreatePanel(parent, "HouseBody", rounded, new Vector2(.25f, .24f), new Vector2(.75f, .62f), main);
            body.raycastTarget = false;

            var roofL = CreatePanel(parent, "RoofL", rounded, new Vector2(.20f, .58f), new Vector2(.53f, .70f), main);
            roofL.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 38f);
            var roofR = CreatePanel(parent, "RoofR", rounded, new Vector2(.47f, .58f), new Vector2(.80f, .70f), main);
            roofR.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -38f);

            var door = CreatePanel(parent, "Door", rounded, new Vector2(.43f, .24f), new Vector2(.57f, .48f), depth);
            door.raycastTarget = false;
        }

        static void BuildBook(Transform parent, Color main, Color depth)
        {
            var left = CreatePanel(parent, "BookLeft", rounded, new Vector2(.17f, .28f), new Vector2(.49f, .73f), main);
            var right = CreatePanel(parent, "BookRight", rounded, new Vector2(.51f, .28f), new Vector2(.83f, .73f), main);
            left.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -4f);
            right.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 4f);
            CreatePanel(parent, "BookSpine", rounded, new Vector2(.475f, .27f), new Vector2(.525f, .72f), depth);
            CreatePanel(left.transform, "PageLineL1", rounded, new Vector2(.18f, .61f), new Vector2(.80f, .67f), Color.white);
            CreatePanel(left.transform, "PageLineL2", rounded, new Vector2(.18f, .45f), new Vector2(.72f, .51f), Color.white);
            CreatePanel(right.transform, "PageLineR1", rounded, new Vector2(.20f, .61f), new Vector2(.82f, .67f), Color.white);
            CreatePanel(right.transform, "PageLineR2", rounded, new Vector2(.28f, .45f), new Vector2(.82f, .51f), Color.white);
        }

        static void BuildTrophy(Transform parent, Color main, Color depth)
        {
            var cup = CreatePanel(parent, "Cup", rounded, new Vector2(.31f, .48f), new Vector2(.69f, .77f), main);
            var handleL = CreatePanel(parent, "HandleL", circle, new Vector2(.16f, .49f), new Vector2(.39f, .70f), main);
            var handleR = CreatePanel(parent, "HandleR", circle, new Vector2(.61f, .49f), new Vector2(.84f, .70f), main);
            handleL.transform.SetAsFirstSibling();
            handleR.transform.SetAsFirstSibling();
            CreatePanel(parent, "Stem", rounded, new Vector2(.45f, .31f), new Vector2(.55f, .50f), depth);
            CreatePanel(parent, "Base", rounded, new Vector2(.30f, .22f), new Vector2(.70f, .34f), depth);
            CreatePanel(cup.transform, "CupShine", rounded, new Vector2(.12f, .65f), new Vector2(.88f, .87f), new Color(1f, 1f, 1f, .30f));
        }

        static void BuildPeople(Transform parent, Color main, Color depth)
        {
            CreatePanel(parent, "HeadL", circle, new Vector2(.24f, .53f), new Vector2(.47f, .76f), main);
            CreatePanel(parent, "HeadR", circle, new Vector2(.53f, .53f), new Vector2(.76f, .76f), main);
            CreatePanel(parent, "BodyL", rounded, new Vector2(.17f, .25f), new Vector2(.50f, .52f), main);
            CreatePanel(parent, "BodyR", rounded, new Vector2(.50f, .25f), new Vector2(.83f, .52f), main);
            CreatePanel(parent, "PeopleBase", rounded, new Vector2(.24f, .20f), new Vector2(.76f, .30f), depth);
        }

        static Image CreatePanel(Transform parent, string name, Sprite sprite, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = sprite == rounded ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static void SetRect(GameObject go, Vector2 min, Vector2 max)
        {
            if (go == null || go.transform is not RectTransform rect) return;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void ClearEffects(GameObject go)
        {
            if (go == null) return;
            foreach (var e in go.GetComponents<Shadow>()) Object.DestroyImmediate(e);
            foreach (var e in go.GetComponents<Outline>()) Object.DestroyImmediate(e);
        }

        static void AddShadow(GameObject go, Vector2 distance, Color color)
        {
            if (go == null) return;
            var s = go.AddComponent<Shadow>();
            s.effectDistance = distance;
            s.effectColor = color;
            s.useGraphicAlpha = true;
        }

        static void AddOutline(GameObject go, Color color, Vector2 distance)
        {
            if (go == null) return;
            var o = go.AddComponent<Outline>();
            o.effectColor = color;
            o.effectDistance = distance;
            o.useGraphicAlpha = true;
        }

        static Color Hex(string hex, float alpha = 1f)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            ColorUtility.TryParseHtmlString(hex, out var color);
            color.a = alpha;
            return color;
        }
    }
}
#endif
