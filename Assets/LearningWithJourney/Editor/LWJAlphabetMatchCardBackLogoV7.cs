#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJAlphabetMatchCardBackLogoV7
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/AlphabetMatchWorld.unity";
        const string V6Root = "LWJCardBackLogoV6";
        const string V7Root = "LWJCardBackLogoV7";

        [MenuItem("Learning with Journey/Polish Match Card Backs V7 Larger Logo")]
        public static void Apply() => ApplyInternal(true);
        public static void ApplySilently() => ApplyInternal(false);

        static void ApplyInternal(bool showDialog)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null) return;
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            int updated = 0;

            for (int i = 1; i <= 8; i++)
            {
                var card = GameObject.Find("MatchCard" + i);
                if (card == null) continue;
                var back = card.transform.Find("Back");
                if (back == null) continue;

                DestroyChild(back, V6Root);
                DestroyChild(back, V7Root);
                SetChildActive(back, "Question", false);
                SetChildActive(back, "MatchLabel", false);
                SetChildActive(back, "Gloss", false);

                var backImage = back.GetComponent<Image>();
                Sprite rounded = backImage != null ? backImage.sprite : null;
                if (backImage != null)
                {
                    backImage.color = Hex("5F2EB0");
                    backImage.raycastTarget = false;
                }

                TMP_FontAsset font = FindFont(back);
                var root = AddRect(back, V7Root, Vector2.zero, Vector2.one);

                var frame = AddImage(root, "Frame", new Vector2(.025f,.025f), new Vector2(.975f,.975f), rounded, Hex("9A38E4"));
                AddOutline(frame.gameObject, Hex("F7C3FF"), new Vector2(3f,-3f));

                var inner = AddImage(frame.transform, "Inner", new Vector2(.035f,.035f), new Vector2(.965f,.965f), rounded, Hex("7137C6"));
                AddOutline(inner.gameObject, Color.white, new Vector2(2f,-2f));

                AddImage(inner.transform, "TopGlow", new Vector2(.05f,.76f), new Vector2(.95f,.96f), rounded, new Color(1f,1f,1f,.13f));

                // Make the logo fill the card instead of looking like a tiny badge.
                var badge = AddImage(inner.transform, "Badge", new Vector2(.055f,.10f), new Vector2(.945f,.90f), rounded, new Color(1f,1f,1f,.97f));
                AddOutline(badge.gameObject, Hex("EAD8FF"), new Vector2(2f,-2f));

                string learningText =
                    "<color=#FFD83D>L</color><color=#FF5CA8>e</color><color=#49D9F5>a</color><color=#7FE34A>r</color>" +
                    "<color=#FFD83D>n</color><color=#FF5CA8>i</color><color=#49D9F5>n</color><color=#A963F2>g</color>";

                var learning = AddText(badge.transform, "Learning", learningText, font,
                    new Vector2(.03f,.58f), new Vector2(.97f,.94f), 40f, FontStyles.Bold, Color.white);
                learning.enableAutoSizing = true; learning.fontSizeMin = 18f; learning.fontSizeMax = 46f;
                learning.richText = true; learning.outlineColor = Hex("4B217B"); learning.outlineWidth = .10f;

                var with = AddText(badge.transform, "With", "with", font,
                    new Vector2(.20f,.42f), new Vector2(.80f,.61f), 22f, FontStyles.Bold, Hex("6A35B5"));
                with.enableAutoSizing = true; with.fontSizeMin = 12f; with.fontSizeMax = 25f;

                var journey = AddText(badge.transform, "Journey", "Journey", font,
                    new Vector2(.03f,.08f), new Vector2(.97f,.46f), 44f, FontStyles.Bold, Hex("F23893"));
                journey.enableAutoSizing = true; journey.fontSizeMin = 19f; journey.fontSizeMax = 50f;
                journey.outlineColor = Hex("5A218D"); journey.outlineWidth = .09f;

                AddImage(inner.transform, "StarAccent", new Vector2(.07f,.82f), new Vector2(.16f,.90f), rounded, Hex("FFD83D"));
                AddImage(inner.transform, "HeartAccent", new Vector2(.84f,.82f), new Vector2(.93f,.90f), rounded, Hex("FF5CA8"));
                AddImage(inner.transform, "BottomAccent", new Vector2(.44f,.045f), new Vector2(.56f,.10f), rounded, Hex("49D9F5"));

                root.SetAsLastSibling();
                updated++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (showDialog)
                EditorUtility.DisplayDialog("Learning with Journey", "Done. " + updated + " matching-card backs now use the larger V7 Learning with Journey logo treatment.", "OK");
        }

        static TMP_FontAsset FindFont(Transform t)
        {
            var any = t.GetComponentInChildren<TMP_Text>(true);
            return any != null && any.font != null ? any.font : TMP_Settings.defaultFontAsset;
        }

        static RectTransform AddRect(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent,false);
            var r = (RectTransform)go.transform; r.anchorMin=min; r.anchorMax=max; r.offsetMin=r.offsetMax=Vector2.zero; return r;
        }

        static Image AddImage(Transform parent,string name,Vector2 min,Vector2 max,Sprite sprite,Color color)
        {
            var go = new GameObject(name,typeof(RectTransform),typeof(Image)); go.transform.SetParent(parent,false);
            var r=(RectTransform)go.transform; r.anchorMin=min; r.anchorMax=max; r.offsetMin=r.offsetMax=Vector2.zero;
            var img=go.GetComponent<Image>(); img.sprite=sprite; img.type=sprite!=null?Image.Type.Sliced:Image.Type.Simple; img.color=color; img.raycastTarget=false; return img;
        }

        static TMP_Text AddText(Transform parent,string name,string value,TMP_FontAsset font,Vector2 min,Vector2 max,float size,FontStyles style,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI)); go.transform.SetParent(parent,false);
            var r=(RectTransform)go.transform; r.anchorMin=min; r.anchorMax=max; r.offsetMin=r.offsetMax=Vector2.zero;
            var t=go.GetComponent<TextMeshProUGUI>(); t.text=value; if(font!=null)t.font=font; t.fontSize=size; t.fontStyle=style; t.color=color; t.alignment=TextAlignmentOptions.Center; t.textWrappingMode=TextWrappingModes.NoWrap; t.raycastTarget=false; return t;
        }

        static void AddOutline(GameObject go,Color c,Vector2 d){var o=go.GetComponent<Outline>()??go.AddComponent<Outline>();o.effectColor=c;o.effectDistance=d;o.useGraphicAlpha=true;}
        static void DestroyChild(Transform p,string n){var c=p.Find(n);if(c!=null)Object.DestroyImmediate(c.gameObject);}
        static void SetChildActive(Transform p,string n,bool a){var c=p.Find(n);if(c!=null)c.gameObject.SetActive(a);}
        static Color Hex(string hex){if(!hex.StartsWith("#"))hex="#"+hex; return ColorUtility.TryParseHtmlString(hex,out var c)?c:Color.white;}
    }
}
#endif
