#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJRewardsVisualPolishV2
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/RewardsRoom.unity";
        const string DecorRootName = "RewardsRoomDecorV2";
        const string ChestDetailRootName = "ChestDetailsV2";
        const string LidDetailRootName = "LidDetailsV2";
        static Sprite rounded;

        [MenuItem("Learning with Journey/Polish Rewards Background + 3D Chest V2")]
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

            rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                if (showDialog)
                    EditorUtility.DisplayDialog("Learning with Journey", "The Rewards canvas was not found.", "OK");
                return;
            }

            RemoveOldBookshelf();
            PolishBaseRoom();
            BuildRewardsRoomDecor(canvas.transform);
            PolishChest();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "Rewards V2 is ready. The bookshelf was removed, the background now reads as a dedicated rewards/trophy room, and the treasure chest has layered depth, gold trim, inset panels, corner guards, hinges, gems, highlights, and a more dimensional 3D-style silhouette. The treasure-opening gameplay was not changed.",
                    "OK");
            }
        }

        static void RemoveOldBookshelf()
        {
            DestroyObject("BookShelf");
        }

        static void PolishBaseRoom()
        {
            SetImageColor("Wall", Hex("F5D4ED"));
            SetImageColor("WallGlow", Hex("FFF0D6", .48f));
            SetImageColor("Floor", Hex("D9A0E7"));
            SetImageColor("Rug", Hex("8D4DD0", .96f));

            var sign = GameObject.Find("SuccessSign");
            if (sign != null)
            {
                var image = sign.GetComponent<Image>();
                if (image != null) image.color = Hex("FFF9EF", .98f);
                AddOutline(sign, Hex("F3BF4D"), new Vector2(3f, -3f));
            }
        }

        static void BuildRewardsRoomDecor(Transform canvas)
        {
            var old = canvas.Find(DecorRootName);
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = CreateRect(canvas, DecorRootName, Vector2.zero, Vector2.one);
            // Keep the new room architecture behind Journey, the chest, HUD, and controls.
            root.SetSiblingIndex(Mathf.Min(4, canvas.childCount - 1));

            // Crown-like ceiling valance and gold molding make this read as a prize room,
            // rather than another classroom.
            AddImage(root, "CeilingGlow", new Vector2(0f, .79f), new Vector2(1f, .92f), Hex("FFF4CD", .22f));
            AddImage(root, "TopMolding", new Vector2(.03f, .785f), new Vector2(.97f, .799f), Hex("F1B934"));
            AddImage(root, "TopMoldingHighlight", new Vector2(.05f, .799f), new Vector2(.95f, .806f), Hex("FFF0A0", .9f));

            // Main treasure alcove behind the chest.
            var arch = AddImage(root, "TreasureAlcove", new Vector2(.36f, .33f), new Vector2(.975f, .755f), Hex("7C43BE", .16f));
            AddOutline(arch.gameObject, Hex("E5B94D", .72f), new Vector2(4f, -4f));
            AddImage(arch.transform, "AlcoveInner", new Vector2(.045f, .045f), new Vector2(.955f, .955f), Hex("FFFFFF", .065f));
            AddImage(arch.transform, "AlcoveTopGlow", new Vector2(.06f, .80f), new Vector2(.94f, .94f), Hex("FFF3BA", .16f));

            // Trophy/display case replacing the bookshelf.
            var display = AddImage(root, "RewardDisplayCase", new Vector2(.805f, .52f), new Vector2(.985f, .70f), Hex("5B2C86", .92f));
            AddShadow(display.gameObject, new Vector2(0f, -7f), Hex("3B185E", .38f));
            AddOutline(display.gameObject, Hex("E6B43B"), new Vector2(3f, -3f));
            AddImage(display.transform, "Glass", new Vector2(.07f, .09f), new Vector2(.93f, .91f), Hex("FFFFFF", .12f));
            AddImage(display.transform, "Shelf", new Vector2(.06f, .42f), new Vector2(.94f, .47f), Hex("E8B943"));
            BuildMiniTrophy(display.transform, "TrophyA", .10f, .47f, .42f, .88f, Hex("FFD34F"));
            BuildMiniTrophy(display.transform, "TrophyB", .38f, .47f, .70f, .88f, Hex("F6C8FF"));
            BuildMiniTrophy(display.transform, "TrophyC", .66f, .47f, .90f, .88f, Hex("63D7F2"));
            BuildBadge(display.transform, "BadgeA", new Vector2(.12f, .13f), new Vector2(.35f, .35f), Hex("F25AA6"));
            BuildBadge(display.transform, "BadgeB", new Vector2(.40f, .13f), new Vector2(.63f, .35f), Hex("FFD34F"));
            BuildBadge(display.transform, "BadgeC", new Vector2(.68f, .13f), new Vector2(.91f, .35f), Hex("4FCBE3"));

            // Award plaques / celebration marks on the wall.
            BuildPlaque(root, "AwardPlaque1", new Vector2(.47f, .69f), new Vector2(.58f, .755f), "1", Hex("F4C64E"));
            BuildPlaque(root, "AwardPlaque2", new Vector2(.60f, .69f), new Vector2(.71f, .755f), "2", Hex("F25AA6"));
            BuildPlaque(root, "AwardPlaque3", new Vector2(.73f, .69f), new Vector2(.84f, .755f), "3", Hex("52CDE5"));

            // Low, subtle floor spotlights framing the treasure area.
            AddImage(root, "SpotlightLeft", new Vector2(.31f, .24f), new Vector2(.46f, .44f), Hex("FFF4B0", .08f));
            AddImage(root, "SpotlightRight", new Vector2(.83f, .24f), new Vector2(.98f, .44f), Hex("FFF4B0", .08f));
        }

        static void PolishChest()
        {
            var chest = GameObject.Find("TreasureCase");
            if (chest == null) return;

            var body = chest.transform.Find("ChestBody");
            var lid = chest.transform.Find("ChestLid");
            if (body == null || lid == null) return;

            // Remove the simpler V1 decorative layers while preserving the actual chest body
            // and lid objects referenced by the runtime animation controller.
            DestroyChild(body, "BodyGlow");
            DestroyChild(body, "GoldTop");
            DestroyChild(body, "GoldBottom");
            DestroyChild(body, "GoldLeft");
            DestroyChild(body, "GoldRight");
            DestroyChild(body, "ChestEmblem");
            DestroyChild(body, ChestDetailRootName);
            DestroyChild(lid, "LidGlow");
            DestroyChild(lid, "GoldBand");
            DestroyChild(lid, LidDetailRootName);

            var bodyImage = body.GetComponent<Image>();
            if (bodyImage != null) bodyImage.color = Hex("6D24AE");
            AddOutline(body.gameObject, Hex("F5BD2D"), new Vector2(7f, -7f));
            AddShadow(body.gameObject, new Vector2(12f, -13f), Hex("321348", .48f));

            // Dimensional back/right/bottom faces.
            var details = CreateRect(body, ChestDetailRootName, Vector2.zero, Vector2.one);
            AddImage(details, "RightDepth", new Vector2(.86f, .09f), new Vector2(.99f, .91f), Hex("4A177A"));
            AddImage(details, "RightGoldEdge", new Vector2(.89f, .10f), new Vector2(.98f, .91f), Hex("D89218"));
            AddImage(details, "BottomDepth", new Vector2(.06f, .02f), new Vector2(.94f, .18f), Hex("4B177A"));
            AddImage(details, "BottomGoldEdge", new Vector2(.07f, .05f), new Vector2(.93f, .14f), Hex("D89318"));

            // Deep inset front panel.
            var inset = AddImage(details, "FrontInset", new Vector2(.15f, .22f), new Vector2(.84f, .77f), Hex("4E177F"));
            AddOutline(inset.gameObject, Hex("A94FE0"), new Vector2(3f, -3f));
            AddImage(inset.transform, "InsetGlow", new Vector2(.05f, .58f), new Vector2(.95f, .90f), Hex("D55ADB", .22f));
            AddImage(details, "TopGoldRail", new Vector2(.04f, .78f), new Vector2(.91f, .94f), Hex("F7BE2B"));
            AddImage(details, "TopRailShine", new Vector2(.07f, .86f), new Vector2(.88f, .92f), Hex("FFF0A0", .82f));
            AddImage(details, "LeftGoldBand", new Vector2(.055f, .12f), new Vector2(.15f, .89f), Hex("F3B422"));
            AddImage(details, "RightGoldBand", new Vector2(.79f, .12f), new Vector2(.885f, .89f), Hex("D99818"));

            // Corner guards create the chunky toy-chest silhouette.
            AddCorner(details, "CornerTL", .035f, .79f, .18f, .96f);
            AddCorner(details, "CornerTR", .77f, .79f, .925f, .96f);
            AddCorner(details, "CornerBL", .035f, .03f, .18f, .21f);
            AddCorner(details, "CornerBR", .77f, .03f, .925f, .21f);

            // Layered center lock / emblem.
            var lockBack = AddImage(details, "LockBack", new Vector2(.37f, .12f), new Vector2(.63f, .55f), Hex("C98514"));
            AddOutline(lockBack.gameObject, Hex("FFF0A0"), new Vector2(4f, -4f));
            var lockFront = AddImage(lockBack.transform, "LockFront", new Vector2(.12f, .12f), new Vector2(.88f, .88f), Hex("FFD33F"));
            AddOutline(lockFront.gameObject, Hex("B97012"), new Vector2(2f, -2f));
            var j = AddText(lockFront.transform, "J", "J", 48f, FontStyles.Bold, Hex("7A2BB8"), new Vector2(.08f, .06f), new Vector2(.92f, .94f));
            j.alignment = TextAlignmentOptions.Center;

            // Gem studs.
            AddGem(details, "Gem1", new Vector2(.20f, .38f), Hex("F45BA8"));
            AddGem(details, "Gem2", new Vector2(.72f, .38f), Hex("4FD3EA"));
            AddGem(details, "Gem3", new Vector2(.20f, .63f), Hex("FFD54A"));
            AddGem(details, "Gem4", new Vector2(.72f, .63f), Hex("D86BEE"));

            var lidImage = lid.GetComponent<Image>();
            if (lidImage != null) lidImage.color = Hex("7E2DC0");
            AddOutline(lid.gameObject, Hex("F7C236"), new Vector2(7f, -7f));
            AddShadow(lid.gameObject, new Vector2(10f, -10f), Hex("35134C", .42f));

            var lidDetails = CreateRect(lid, LidDetailRootName, Vector2.zero, Vector2.one);
            AddImage(lidDetails, "LidRightDepth", new Vector2(.87f, .08f), new Vector2(.99f, .88f), Hex("501781"));
            var lidInset = AddImage(lidDetails, "LidInset", new Vector2(.10f, .26f), new Vector2(.86f, .84f), Hex("5C1C91"));
            AddOutline(lidInset.gameObject, Hex("B453E3"), new Vector2(3f, -3f));
            AddImage(lidInset.transform, "LidGlow", new Vector2(.06f, .57f), new Vector2(.94f, .88f), Hex("E16AE4", .24f));
            AddImage(lidDetails, "LidGoldBand", new Vector2(.04f, .06f), new Vector2(.93f, .29f), Hex("F6BC2B"));
            AddImage(lidDetails, "LidBandShine", new Vector2(.07f, .17f), new Vector2(.90f, .25f), Hex("FFF2A0", .85f));
            AddCorner(lidDetails, "LidCornerL", .025f, .04f, .16f, .34f);
            AddCorner(lidDetails, "LidCornerR", .80f, .04f, .95f, .34f);

            // Visible hinges at the base of the lid.
            AddImage(lidDetails, "HingeL", new Vector2(.20f, .02f), new Vector2(.34f, .16f), Hex("B87512"));
            AddImage(lidDetails, "HingeR", new Vector2(.62f, .02f), new Vector2(.76f, .16f), Hex("B87512"));

            // Keep the details non-interactive so the existing treasure button/controller remains authoritative.
            DisableRaycasts(details);
            DisableRaycasts(lidDetails);
        }

        static void BuildMiniTrophy(Transform parent, string name, float x1, float y1, float x2, float y2, Color color)
        {
            var root = CreateRect(parent, name, new Vector2(x1, y1), new Vector2(x2, y2));
            AddImage(root, "Cup", new Vector2(.19f, .43f), new Vector2(.81f, .82f), color);
            AddImage(root, "Stem", new Vector2(.44f, .22f), new Vector2(.56f, .46f), color);
            AddImage(root, "Base", new Vector2(.27f, .10f), new Vector2(.73f, .25f), color);
            AddImage(root, "HandleL", new Vector2(.04f, .48f), new Vector2(.25f, .72f), color);
            AddImage(root, "HandleR", new Vector2(.75f, .48f), new Vector2(.96f, .72f), color);
        }

        static void BuildBadge(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var badge = AddImage(parent, name, min, max, color);
            AddOutline(badge.gameObject, Hex("FFF3C4"), new Vector2(2f, -2f));
            AddImage(badge.transform, "Center", new Vector2(.27f, .27f), new Vector2(.73f, .73f), Hex("FFFFFF", .45f));
        }

        static void BuildPlaque(Transform parent, string name, Vector2 min, Vector2 max, string number, Color color)
        {
            var plaque = AddImage(parent, name, min, max, Hex("6A3197", .88f));
            AddOutline(plaque.gameObject, Hex("E9B943"), new Vector2(2f, -2f));
            var medallion = AddImage(plaque.transform, "Medallion", new Vector2(.17f, .14f), new Vector2(.83f, .86f), color);
            AddOutline(medallion.gameObject, Hex("FFF1B0"), new Vector2(2f, -2f));
            var text = AddText(medallion.transform, "Number", number, 22f, FontStyles.Bold, Hex("5D267C"), new Vector2(.05f, .05f), new Vector2(.95f, .95f));
            text.alignment = TextAlignmentOptions.Center;
        }

        static void AddCorner(Transform parent, string name, float x1, float y1, float x2, float y2)
        {
            var corner = AddImage(parent, name, new Vector2(x1, y1), new Vector2(x2, y2), Hex("F4B923"));
            AddOutline(corner.gameObject, Hex("FFF0A0"), new Vector2(2f, -2f));
            AddImage(corner.transform, "Shine", new Vector2(.14f, .58f), new Vector2(.86f, .86f), Hex("FFFFFF", .30f));
        }

        static void AddGem(Transform parent, string name, Vector2 center, Color color)
        {
            Vector2 half = new Vector2(.035f, .055f);
            var gem = AddImage(parent, name, center - half, center + half, color);
            gem.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            AddOutline(gem.gameObject, Hex("FFF4C0"), new Vector2(2f, -2f));
        }

        static RectTransform CreateRect(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            return rect;
        }

        static Image AddImage(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            var image = go.GetComponent<Image>();
            image.sprite = rounded;
            image.type = rounded != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static TMP_Text AddText(Transform parent, string name, string value, float size, FontStyles style, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            return text;
        }

        static void AddOutline(GameObject target, Color color, Vector2 distance)
        {
            var outline = target.GetComponent<Outline>();
            if (outline == null) outline = target.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        static void AddShadow(GameObject target, Vector2 distance, Color color)
        {
            var shadow = target.GetComponent<Shadow>();
            if (shadow == null) shadow = target.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        static void DisableRaycasts(Transform root)
        {
            foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
        }

        static void SetImageColor(string objectName, Color color)
        {
            var go = GameObject.Find(objectName);
            if (go == null) return;
            var image = go.GetComponent<Image>();
            if (image != null) image.color = color;
        }

        static void DestroyObject(string objectName)
        {
            var go = GameObject.Find(objectName);
            if (go != null) Object.DestroyImmediate(go);
        }

        static void DestroyChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null) Object.DestroyImmediate(child.gameObject);
        }

        static Color Hex(string hex, float alpha = 1f)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                color.a = alpha;
                return color;
            }
            return new Color(1f, 1f, 1f, alpha);
        }
    }
}
#endif
