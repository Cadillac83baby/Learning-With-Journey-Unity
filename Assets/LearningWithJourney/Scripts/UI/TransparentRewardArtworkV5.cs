using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Draws the four Rewards prize icons directly in Unity UI with transparent backgrounds.
    /// No PNG/JPG or backing rectangle is used, so the artwork floats cleanly above the chest.
    /// </summary>
    public class TransparentRewardArtworkV5 : MaskableGraphic
    {
        public enum RewardKind
        {
            GoldStarSticker = 0,
            RainbowBadge = 1,
            CrownBadge = 2,
            SuperLearnerTrophy = 3
        }

        [SerializeField] RewardKind rewardKind = RewardKind.GoldStarSticker;

        static readonly Color Gold = Hex("FFD43B");
        static readonly Color DeepGold = Hex("E59A15");
        static readonly Color Pink = Hex("F04AA4");
        static readonly Color Purple = Hex("7A35C5");
        static readonly Color DeepPurple = Hex("54228E");
        static readonly Color Blue = Hex("40C8E8");
        static readonly Color Green = Hex("55C66B");
        static readonly Color Orange = Hex("FF9E32");
        static readonly Color Red = Hex("F45D69");
        static readonly Color Cream = Hex("FFF4C4");

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
            color = Color.white;
        }

        public void SetReward(int index)
        {
            int wrapped = ((index % 4) + 4) % 4;
            rewardKind = (RewardKind)wrapped;
            SetVerticesDirty();
        }

        public void SetReward(RewardKind kind)
        {
            rewardKind = kind;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            Vector2 c = r.center;
            float size = Mathf.Min(r.width, r.height);

            switch (rewardKind)
            {
                case RewardKind.GoldStarSticker:
                    DrawGoldStar(vh, c, size);
                    break;
                case RewardKind.RainbowBadge:
                    DrawRainbowBadge(vh, c, size);
                    break;
                case RewardKind.CrownBadge:
                    DrawCrown(vh, c, size);
                    break;
                case RewardKind.SuperLearnerTrophy:
                    DrawTrophy(vh, c, size);
                    break;
            }
        }

        void DrawGoldStar(VertexHelper vh, Vector2 c, float size)
        {
            AddStar(vh, c + new Vector2(size * .025f, -size * .035f), size * .43f, size * .19f, DeepGold);
            AddStar(vh, c, size * .41f, size * .18f, Gold);
            AddCircle(vh, c + new Vector2(-size * .12f, size * .12f), size * .045f, Cream, 16);
            AddCircle(vh, c + new Vector2(size * .14f, size * .06f), size * .025f, Cream, 12);
        }

        void DrawRainbowBadge(VertexHelper vh, Vector2 c, float size)
        {
            Vector2 arcCenter = c + new Vector2(0f, -size * .10f);
            float[] radii = { .43f, .355f, .28f, .205f };
            Color[] colors = { Red, Orange, Gold, Blue };
            const int segments = 28;

            for (int b = 0; b < radii.Length; b++)
            {
                float outer = size * radii[b];
                float inner = outer - size * .065f;
                AddArc(vh, arcCenter, inner, outer, 12f, 168f, segments, colors[b]);
            }

            // Small cloud puffs only; there is deliberately no badge background plate.
            Vector2 left = arcCenter + new Vector2(-size * .30f, -size * .015f);
            Vector2 right = arcCenter + new Vector2(size * .30f, -size * .015f);
            AddCircle(vh, left, size * .105f, Cream, 18);
            AddCircle(vh, left + new Vector2(size * .09f, size * .025f), size * .09f, Cream, 18);
            AddCircle(vh, right, size * .105f, Cream, 18);
            AddCircle(vh, right - new Vector2(size * .09f, -size * .025f), size * .09f, Cream, 18);
        }

        void DrawCrown(VertexHelper vh, Vector2 c, float size)
        {
            Vector2[] shadow =
            {
                c + new Vector2(-.38f, -.18f) * size,
                c + new Vector2(-.34f, .27f) * size,
                c + new Vector2(-.12f, .05f) * size,
                c + new Vector2(0f, .34f) * size,
                c + new Vector2(.13f, .05f) * size,
                c + new Vector2(.35f, .27f) * size,
                c + new Vector2(.38f, -.18f) * size
            };
            for (int i = 0; i < shadow.Length; i++) shadow[i] += new Vector2(size * .025f, -size * .035f);
            AddPolygon(vh, shadow, DeepGold);

            Vector2[] crown =
            {
                c + new Vector2(-.38f, -.16f) * size,
                c + new Vector2(-.34f, .29f) * size,
                c + new Vector2(-.12f, .07f) * size,
                c + new Vector2(0f, .36f) * size,
                c + new Vector2(.13f, .07f) * size,
                c + new Vector2(.35f, .29f) * size,
                c + new Vector2(.38f, -.16f) * size
            };
            AddPolygon(vh, crown, Gold);
            AddQuad(vh, c + new Vector2(-.39f, -.23f) * size, c + new Vector2(.39f, -.10f) * size, DeepPurple);
            AddCircle(vh, c + new Vector2(-.20f, -.16f) * size, size * .045f, Pink, 14);
            AddCircle(vh, c + new Vector2(0f, -.16f) * size, size * .05f, Blue, 14);
            AddCircle(vh, c + new Vector2(.20f, -.16f) * size, size * .045f, Pink, 14);
        }

        void DrawTrophy(VertexHelper vh, Vector2 c, float size)
        {
            // Handles
            AddArc(vh, c + new Vector2(-.25f, .08f) * size, size * .10f, size * .16f, 75f, 285f, 18, Gold);
            AddArc(vh, c + new Vector2(.25f, .08f) * size, size * .10f, size * .16f, -105f, 105f, 18, Gold);

            // Cup shadow + cup
            Vector2[] cupShadow =
            {
                c + new Vector2(-.28f, .27f) * size,
                c + new Vector2(.30f, .27f) * size,
                c + new Vector2(.20f, -.03f) * size,
                c + new Vector2(.08f, -.13f) * size,
                c + new Vector2(-.06f, -.13f) * size,
                c + new Vector2(-.22f, -.03f) * size
            };
            for (int i = 0; i < cupShadow.Length; i++) cupShadow[i] += new Vector2(size * .02f, -size * .03f);
            AddPolygon(vh, cupShadow, DeepGold);

            Vector2[] cup =
            {
                c + new Vector2(-.28f, .29f) * size,
                c + new Vector2(.28f, .29f) * size,
                c + new Vector2(.19f, -.01f) * size,
                c + new Vector2(.07f, -.11f) * size,
                c + new Vector2(-.07f, -.11f) * size,
                c + new Vector2(-.19f, -.01f) * size
            };
            AddPolygon(vh, cup, Gold);

            // Stem and base
            AddQuad(vh, c + new Vector2(-.045f, -.13f) * size, c + new Vector2(.045f, -.31f) * size, Gold);
            AddQuad(vh, c + new Vector2(-.18f, -.34f) * size, c + new Vector2(.18f, -.27f) * size, DeepGold);
            AddQuad(vh, c + new Vector2(-.24f, -.41f) * size, c + new Vector2(.24f, -.33f) * size, Purple);

            // Center medallion
            AddCircle(vh, c + new Vector2(0f, .08f) * size, size * .10f, Purple, 20);
            AddStar(vh, c + new Vector2(0f, .08f) * size, size * .065f, size * .030f, Cream);
        }

        void AddStar(VertexHelper vh, Vector2 center, float outer, float inner, Color32 col)
        {
            Vector2[] pts = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float radius = (i & 1) == 0 ? outer : inner;
                float a = Mathf.Deg2Rad * (-90f + i * 36f);
                pts[i] = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            }
            AddPolygon(vh, pts, col);
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

        void AddArc(VertexHelper vh, Vector2 center, float innerRadius, float outerRadius, float startDeg, float endDeg, int segments, Color32 col)
        {
            for (int i = 0; i < segments; i++)
            {
                float t0 = i / (float)segments;
                float t1 = (i + 1) / (float)segments;
                float a0 = Mathf.Deg2Rad * Mathf.Lerp(startDeg, endDeg, t0);
                float a1 = Mathf.Deg2Rad * Mathf.Lerp(startDeg, endDeg, t1);
                Vector2 i0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * innerRadius;
                Vector2 i1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * innerRadius;
                Vector2 o0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * outerRadius;
                Vector2 o1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * outerRadius;
                AddQuad(vh, i0, i1, o1, o0, col);
            }
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
            for (int i = 0; i < pts.Length; i++)
            {
                int next = (i + 1) % pts.Length;
                vh.AddTriangle(start, start + 1 + i, start + 1 + next);
            }
        }

        void AddQuad(VertexHelper vh, Vector2 min, Vector2 max, Color32 col)
        {
            AddQuad(vh,
                new Vector2(min.x, min.y),
                new Vector2(max.x, min.y),
                new Vector2(max.x, max.y),
                new Vector2(min.x, max.y),
                col);
        }

        void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color32 col)
        {
            int start = vh.currentVertCount;
            AddVertex(vh, a, col);
            AddVertex(vh, b, col);
            AddVertex(vh, c, col);
            AddVertex(vh, d, col);
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
