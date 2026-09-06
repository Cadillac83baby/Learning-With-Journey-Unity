using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Draws crisp preschool book illustrations directly in Unity UI.
    /// No PNG/JPG backgrounds are required, so pages stay sharp on Android and iOS.
    /// </summary>
    public class BookPageArtworkV1 : MaskableGraphic
    {
        [SerializeField] string bookId = "ABC";
        [SerializeField] int pageIndex;

        static readonly Color Pink = Hex("F04AA4");
        static readonly Color Purple = Hex("7137C5");
        static readonly Color DeepPurple = Hex("4E2389");
        static readonly Color Blue = Hex("38BDE1");
        static readonly Color Green = Hex("5BC86B");
        static readonly Color Gold = Hex("FFD34B");
        static readonly Color Orange = Hex("F6A034");
        static readonly Color Red = Hex("EE5364");
        static readonly Color Brown = Hex("9B6038");
        static readonly Color Cream = Hex("FFF5D2");
        static readonly Color White = Color.white;
        static readonly Color Black = Hex("382543");

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
            color = Color.white;
        }

        public void SetPage(string id, int index)
        {
            bookId = string.IsNullOrEmpty(id) ? "ABC" : id.ToUpperInvariant();
            pageIndex = Mathf.Max(0, index);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            Vector2 c = r.center;
            float s = Mathf.Min(r.width, r.height);

            switch (bookId)
            {
                case "NUMBERS": DrawNumberPage(vh, c, s, pageIndex); break;
                case "COLORS": DrawColorPage(vh, c, s, pageIndex); break;
                case "STORY": DrawStoryPage(vh, c, s, pageIndex); break;
                default: DrawABCPage(vh, c, s, pageIndex); break;
            }
        }

        void DrawABCPage(VertexHelper vh, Vector2 c, float s, int page)
        {
            switch (page % 6)
            {
                case 0: DrawApple(vh, c, s); break;
                case 1: DrawBall(vh, c, s); break;
                case 2: DrawCat(vh, c, s); break;
                case 3: DrawDog(vh, c, s); break;
                case 4: DrawEgg(vh, c, s); break;
                default: DrawFish(vh, c, s); break;
            }
        }

        void DrawApple(VertexHelper vh, Vector2 c, float s)
        {
            AddCircle(vh, c + new Vector2(-.11f, -.02f) * s, .24f * s, Red, 30);
            AddCircle(vh, c + new Vector2(.11f, -.02f) * s, .24f * s, Red, 30);
            AddEllipse(vh, c + new Vector2(0f, -.14f) * s, .30f * s, .24f * s, Red, 32);
            AddQuad(vh, c + new Vector2(-.035f, .18f) * s, c + new Vector2(.035f, .39f) * s, Brown);
            AddEllipseRotated(vh, c + new Vector2(.14f, .33f) * s, .17f * s, .08f * s, 28f, Green, 20);
            AddCircle(vh, c + new Vector2(-.12f, .08f) * s, .045f * s, new Color(1f, .65f, .70f, .85f), 16);
        }

        void DrawBall(VertexHelper vh, Vector2 c, float s)
        {
            AddCircle(vh, c, .36f * s, Blue, 36);
            AddArc(vh, c, .27f * s, .32f * s, -70f, 110f, 26, White);
            AddArc(vh, c, .12f * s, .17f * s, 20f, 200f, 24, Gold);
            AddCircle(vh, c + new Vector2(-.12f, .13f) * s, .05f * s, White, 16);
        }

        void DrawCat(VertexHelper vh, Vector2 c, float s)
        {
            AddCircle(vh, c, .31f * s, Orange, 32);
            AddTriangle(vh, c + new Vector2(-.25f, .20f) * s, c + new Vector2(-.08f, .48f) * s, c + new Vector2(-.02f, .18f) * s, Orange);
            AddTriangle(vh, c + new Vector2(.25f, .20f) * s, c + new Vector2(.08f, .48f) * s, c + new Vector2(.02f, .18f) * s, Orange);
            AddTriangle(vh, c + new Vector2(-.19f, .24f) * s, c + new Vector2(-.09f, .39f) * s, c + new Vector2(-.06f, .22f) * s, Pink);
            AddTriangle(vh, c + new Vector2(.19f, .24f) * s, c + new Vector2(.09f, .39f) * s, c + new Vector2(.06f, .22f) * s, Pink);
            AddCircle(vh, c + new Vector2(-.12f, .06f) * s, .045f * s, Black, 14);
            AddCircle(vh, c + new Vector2(.12f, .06f) * s, .045f * s, Black, 14);
            AddTriangle(vh, c + new Vector2(-.045f, -.05f) * s, c + new Vector2(.045f, -.05f) * s, c + new Vector2(0f, -.12f) * s, Pink);
            AddLine(vh, c + new Vector2(-.06f, -.12f) * s, c + new Vector2(-.27f, -.18f) * s, .012f * s, Black);
            AddLine(vh, c + new Vector2(.06f, -.12f) * s, c + new Vector2(.27f, -.18f) * s, .012f * s, Black);
        }

        void DrawDog(VertexHelper vh, Vector2 c, float s)
        {
            AddCircle(vh, c, .30f * s, Brown, 32);
            AddEllipseRotated(vh, c + new Vector2(-.28f, .08f) * s, .13f * s, .26f * s, -18f, DeepPurple, 20);
            AddEllipseRotated(vh, c + new Vector2(.28f, .08f) * s, .13f * s, .26f * s, 18f, DeepPurple, 20);
            AddEllipse(vh, c + new Vector2(0f, -.10f) * s, .18f * s, .13f * s, Cream, 24);
            AddCircle(vh, c + new Vector2(-.11f, .06f) * s, .04f * s, Black, 14);
            AddCircle(vh, c + new Vector2(.11f, .06f) * s, .04f * s, Black, 14);
            AddCircle(vh, c + new Vector2(0f, -.08f) * s, .055f * s, Black, 16);
            AddEllipse(vh, c + new Vector2(0f, -.22f) * s, .07f * s, .10f * s, Pink, 16);
        }

        void DrawEgg(VertexHelper vh, Vector2 c, float s)
        {
            AddEllipse(vh, c, .28f * s, .39f * s, White, 36);
            AddCircle(vh, c + new Vector2(0f, -.03f) * s, .15f * s, Gold, 28);
            AddEllipse(vh, c + new Vector2(-.10f, .14f) * s, .08f * s, .13f * s, new Color(1f, 1f, 1f, .55f), 18);
        }

        void DrawFish(VertexHelper vh, Vector2 c, float s)
        {
            AddEllipse(vh, c + new Vector2(-.03f, 0f) * s, .33f * s, .22f * s, Blue, 32);
            AddTriangle(vh, c + new Vector2(.26f, 0f) * s, c + new Vector2(.48f, .22f) * s, c + new Vector2(.48f, -.22f) * s, Pink);
            AddCircle(vh, c + new Vector2(-.19f, .06f) * s, .045f * s, White, 14);
            AddCircle(vh, c + new Vector2(-.19f, .06f) * s, .022f * s, Black, 12);
            AddArc(vh, c + new Vector2(-.18f, -.03f) * s, .06f * s, .075f * s, 200f, 335f, 12, DeepPurple);
            AddTriangle(vh, c + new Vector2(-.03f, .18f) * s, c + new Vector2(.08f, .34f) * s, c + new Vector2(.12f, .15f) * s, Gold);
        }

        void DrawNumberPage(VertexHelper vh, Vector2 c, float s, int page)
        {
            int count = Mathf.Clamp(page + 1, 1, 5);
            Color[] colors = { Pink, Blue, Gold, Green, Purple };
            Vector2[] spots =
            {
                new Vector2(0f, .18f),
                new Vector2(-.20f, -.02f),
                new Vector2(.20f, -.02f),
                new Vector2(-.12f, -.27f),
                new Vector2(.12f, -.27f)
            };
            for (int i = 0; i < count; i++)
            {
                AddCircle(vh, c + spots[i] * s, .105f * s, colors[i], 24);
                AddStar(vh, c + spots[i] * s, .055f * s, .025f * s, White);
            }
        }

        void DrawColorPage(VertexHelper vh, Vector2 c, float s, int page)
        {
            switch (page % 5)
            {
                case 0: AddCircle(vh, c, .32f * s, Red, 36); break;
                case 1: AddQuad(vh, c + new Vector2(-.30f, -.30f) * s, c + new Vector2(.30f, .30f) * s, Blue); break;
                case 2: AddTriangle(vh, c + new Vector2(0f, .36f) * s, c + new Vector2(-.36f, -.30f) * s, c + new Vector2(.36f, -.30f) * s, Gold); break;
                case 3: AddStar(vh, c, .36f * s, .17f * s, Green); break;
                default: DrawHeart(vh, c, .78f * s, Purple); break;
            }
        }

        void DrawStoryPage(VertexHelper vh, Vector2 c, float s, int page)
        {
            switch (page % 5)
            {
                case 0:
                    AddCircle(vh, c, .20f * s, Gold, 28);
                    for (int i = 0; i < 8; i++)
                    {
                        float a = i * Mathf.PI * .25f;
                        Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                        AddLine(vh, c + d * (.26f * s), c + d * (.39f * s), .025f * s, Orange);
                    }
                    break;
                case 1:
                    AddQuad(vh, c + new Vector2(-.30f, -.25f) * s, c + new Vector2(.30f, .20f) * s, Pink);
                    AddTriangle(vh, c + new Vector2(-.36f, .20f) * s, c + new Vector2(0f, .48f) * s, c + new Vector2(.36f, .20f) * s, Purple);
                    AddQuad(vh, c + new Vector2(-.07f, -.25f) * s, c + new Vector2(.07f, .02f) * s, Gold);
                    break;
                case 2:
                    DrawHeart(vh, c, .72f * s, Pink);
                    AddStar(vh, c + new Vector2(.28f, .27f) * s, .11f * s, .05f * s, Gold);
                    break;
                case 3:
                    DrawRainbow(vh, c + new Vector2(0f, -.10f) * s, s);
                    break;
                default:
                    AddStar(vh, c, .34f * s, .15f * s, Gold);
                    AddCircle(vh, c + new Vector2(-.29f, .24f) * s, .045f * s, Pink, 14);
                    AddCircle(vh, c + new Vector2(.31f, .18f) * s, .035f * s, Blue, 14);
                    AddCircle(vh, c + new Vector2(.26f, -.24f) * s, .04f * s, Green, 14);
                    break;
            }
        }

        void DrawRainbow(VertexHelper vh, Vector2 c, float s)
        {
            Color[] cols = { Red, Orange, Gold, Green, Blue, Purple };
            for (int i = 0; i < cols.Length; i++)
            {
                float outer = (.39f - i * .045f) * s;
                AddArc(vh, c, outer - .035f * s, outer, 15f, 165f, 26, cols[i]);
            }
            AddCircle(vh, c + new Vector2(-.34f, 0f) * s, .09f * s, White, 18);
            AddCircle(vh, c + new Vector2(.34f, 0f) * s, .09f * s, White, 18);
        }

        void DrawHeart(VertexHelper vh, Vector2 c, float size, Color col)
        {
            float r = size * .20f;
            AddCircle(vh, c + new Vector2(-r, r * .55f), r, col, 24);
            AddCircle(vh, c + new Vector2(r, r * .55f), r, col, 24);
            AddTriangle(vh, c + new Vector2(-r * 2f, r * .55f), c + new Vector2(r * 2f, r * .55f), c + new Vector2(0f, -r * 2.5f), col);
        }

        void AddCircle(VertexHelper vh, Vector2 center, float radius, Color32 col, int segments)
        {
            int start = vh.currentVertCount;
            AddVertex(vh, center, col);
            for (int i = 0; i <= segments; i++)
            {
                float a = Mathf.PI * 2f * i / segments;
                AddVertex(vh, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, col);
            }
            for (int i = 0; i < segments; i++) vh.AddTriangle(start, start + i + 1, start + i + 2);
        }

        void AddEllipse(VertexHelper vh, Vector2 center, float rx, float ry, Color32 col, int segments)
        {
            int start = vh.currentVertCount;
            AddVertex(vh, center, col);
            for (int i = 0; i <= segments; i++)
            {
                float a = Mathf.PI * 2f * i / segments;
                AddVertex(vh, center + new Vector2(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry), col);
            }
            for (int i = 0; i < segments; i++) vh.AddTriangle(start, start + i + 1, start + i + 2);
        }

        void AddEllipseRotated(VertexHelper vh, Vector2 center, float rx, float ry, float degrees, Color32 col, int segments)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cs = Mathf.Cos(rad), sn = Mathf.Sin(rad);
            int start = vh.currentVertCount;
            AddVertex(vh, center, col);
            for (int i = 0; i <= segments; i++)
            {
                float a = Mathf.PI * 2f * i / segments;
                Vector2 p = new Vector2(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry);
                p = new Vector2(p.x * cs - p.y * sn, p.x * sn + p.y * cs);
                AddVertex(vh, center + p, col);
            }
            for (int i = 0; i < segments; i++) vh.AddTriangle(start, start + i + 1, start + i + 2);
        }

        void AddStar(VertexHelper vh, Vector2 center, float outer, float inner, Color32 col)
        {
            Vector2[] pts = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float r = (i & 1) == 0 ? outer : inner;
                float a = (-90f + i * 36f) * Mathf.Deg2Rad;
                pts[i] = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
            }
            AddPolygon(vh, pts, col);
        }

        void AddArc(VertexHelper vh, Vector2 center, float inner, float outer, float startDeg, float endDeg, int segments, Color32 col)
        {
            for (int i = 0; i < segments; i++)
            {
                float a0 = Mathf.Lerp(startDeg, endDeg, i / (float)segments) * Mathf.Deg2Rad;
                float a1 = Mathf.Lerp(startDeg, endDeg, (i + 1) / (float)segments) * Mathf.Deg2Rad;
                Vector2 i0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * inner;
                Vector2 i1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * inner;
                Vector2 o0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * outer;
                Vector2 o1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * outer;
                AddQuad(vh, i0, i1, o1, o0, col);
            }
        }

        void AddLine(VertexHelper vh, Vector2 a, Vector2 b, float width, Color32 col)
        {
            Vector2 d = (b - a).normalized;
            Vector2 n = new Vector2(-d.y, d.x) * (width * .5f);
            AddQuad(vh, a - n, a + n, b + n, b - n, col);
        }

        void AddTriangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color32 col)
        {
            int start = vh.currentVertCount;
            AddVertex(vh, a, col); AddVertex(vh, b, col); AddVertex(vh, c, col);
            vh.AddTriangle(start, start + 1, start + 2);
        }

        void AddPolygon(VertexHelper vh, Vector2[] pts, Color32 col)
        {
            if (pts == null || pts.Length < 3) return;
            Vector2 center = Vector2.zero;
            foreach (var p in pts) center += p;
            center /= pts.Length;
            int start = vh.currentVertCount;
            AddVertex(vh, center, col);
            foreach (var p in pts) AddVertex(vh, p, col);
            for (int i = 0; i < pts.Length; i++) vh.AddTriangle(start, start + 1 + i, start + 1 + ((i + 1) % pts.Length));
        }

        void AddQuad(VertexHelper vh, Vector2 min, Vector2 max, Color32 col)
        {
            AddQuad(vh, new Vector2(min.x, min.y), new Vector2(max.x, min.y), new Vector2(max.x, max.y), new Vector2(min.x, max.y), col);
        }

        void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color32 col)
        {
            int start = vh.currentVertCount;
            AddVertex(vh, a, col); AddVertex(vh, b, col); AddVertex(vh, c, col); AddVertex(vh, d, col);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }

        void AddVertex(VertexHelper vh, Vector2 p, Color32 col)
        {
            UIVertex v = UIVertex.simpleVert;
            v.position = p;
            v.color = col;
            vh.AddVert(v);
        }

        static Color Hex(string hex)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            return ColorUtility.TryParseHtmlString(hex, out Color c) ? c : Color.white;
        }
    }
}
