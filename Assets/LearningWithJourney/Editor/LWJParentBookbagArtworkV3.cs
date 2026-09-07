#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJParentBookbagArtworkV3
    {
        static Sprite rounded;
        [MenuItem("Learning with Journey/Match Parent Bookbag Artwork V3")]
        public static void Apply()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                scene.path != "Assets/LearningWithJourney/Scenes/ParentZone.unity")
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Stop Play mode and open ParentZone first.", "OK");
                return;
            }
            rounded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/LearningWithJourney/Generated/MainMenu/RoundedPanel.png");
            Image bag = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Image image in root.GetComponentsInChildren<Image>(true))
                    if (image.name == "JourneyBackpack") bag = image;
            if (bag == null || rounded == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "Bookbag or Main Menu rounded sprite not found. No changes made.", "OK");
                return;
            }
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Match Parent bookbag artwork");
            Undo.RegisterFullObjectHierarchyUndo(bag.gameObject, "Match Parent bookbag artwork");
            // Retain every root RectTransform property and its approved parent.
            // Replace only decorative layers using the Main Menu V7 bag design.
            foreach (string name in new[] { "Flap", "Pocket", "J", "Handle", "BagArtV3" })
            {
                Transform old = bag.transform.Find(name);
                if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            }
            bag.sprite = rounded;
            bag.type = Image.Type.Sliced;
            bag.pixelsPerUnitMultiplier = 4f;
            bag.color = Hex("D839A5");
            bag.raycastTarget = false;
            foreach (Shadow effect in bag.GetComponents<Shadow>()) effect.enabled = false;
            var border = bag.GetComponent<Outline>();
            if (border == null) border = Undo.AddComponent<Outline>(bag.gameObject);
            border.enabled = true;
            border.effectColor = Hex("FFD24A");
            border.effectDistance = new Vector2(1f, -1f);

            var art = new GameObject("BagArtV3", typeof(RectTransform));
            art.transform.SetParent(bag.transform, false);
            Undo.RegisterCreatedObjectUndo(art, "Create bookbag details");
            SetRect((RectTransform)art.transform, 0, 0, 1, 1);
            Panel(art.transform, "Handle", .30f, .82f, .70f, 1.10f, Hex("6D208C"));
            Panel(art.transform, "LeftStrap", -.05f, .18f, .10f, .76f, Hex("74228D"));
            Panel(art.transform, "RightStrap", .90f, .18f, 1.05f, .76f, Hex("74228D"));
            Panel(art.transform, "Flap", .08f, .49f, .92f, .86f, Hex("F15BB7"));
            var pocket = Panel(art.transform, "Pocket", .15f, .08f, .85f, .43f, Hex("B62D9D"));
            var rim = pocket.gameObject.AddComponent<Outline>();
            rim.effectColor = Hex("FA8BD4");
            rim.effectDistance = new Vector2(.7f, -.7f);
            Panel(art.transform, "Shine", .14f, .69f, .86f, .84f, new Color(1, 1, 1, .21f));
            var label = new GameObject("J", typeof(RectTransform), typeof(TextMeshProUGUI));
            label.transform.SetParent(art.transform, false);
            SetRect((RectTransform)label.transform, .30f, .12f, .70f, .40f);
            var text = label.GetComponent<TextMeshProUGUI>();
            text.text = "J";
            text.color = Color.white;
            text.fontStyle = FontStyles.Bold;
            text.enableAutoSizing = true;
            text.fontSizeMin = 5f;
            text.fontSizeMax = 32f;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorUtility.DisplayDialog("Learning with Journey",
                "Detailed bookbag artwork restored: handle, straps, flap, front pocket, gold edge and J. Approved position and size preserved. Ctrl+S to save.", "OK");
        }
        static Image Panel(Transform parent, string name, float x0, float y0, float x1, float y1, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            SetRect((RectTransform)go.transform, x0, y0, x1, y1);
            var image = go.GetComponent<Image>();
            image.sprite = rounded;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 4f;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }
        static void SetRect(RectTransform r, float x0, float y0, float x1, float y1)
        {
            r.anchorMin = new Vector2(x0, y0);
            r.anchorMax = new Vector2(x1, y1);
            r.offsetMin = r.offsetMax = Vector2.zero;
        }
        static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out Color c); return c; }
    }
}
#endif
