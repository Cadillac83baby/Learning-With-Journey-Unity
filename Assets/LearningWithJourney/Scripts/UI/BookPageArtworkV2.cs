using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Complete vector artwork renderer for the V2 Library reader.
    /// It covers all 26 ABC pages, numbers 1-20, 12 colors/shapes pages,
    /// and 10 story pages without requiring external page-image assets.
    /// </summary>
    public class BookPageArtworkV2 : MaskableGraphic
    {
        [SerializeField] string bookId = "ABC";
        [SerializeField] int pageIndex;

        static readonly Color Pink = Hex("F04AA4");
        static readonly Color HotPink = Hex("FF2F93");
        static readonly Color Purple = Hex("7137C5");
        static readonly Color DeepPurple = Hex("4E2389");
        static readonly Color Blue = Hex("38BDE1");
        static readonly Color Teal = Hex("22B6B9");
        static readonly Color Green = Hex("5BC86B");
        static readonly Color Gold = Hex("FFD34B");
        static readonly Color Orange = Hex("F6A034");
        static readonly Color Red = Hex("EE5364");
        static readonly Color Brown = Hex("9B6038");
        static readonly Color Cream = Hex("FFF5D2");
        static readonly Color White = Color.white;
        static readonly Color Black = Hex("382543");
        static readonly Color PageBg = Hex("F7E9FF");

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
            color = Color.white;
        }

        public void SetPage(string id, int index)
        {
            bookId = string.IsNullOrWhiteSpace(id) ? "ABC" : id.Trim().ToUpperInvariant();
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
            switch (Mathf.Clamp(page, 0, 25))
            {
                case 0: DrawApple(vh, c, s); break;
                case 1: DrawBall(vh, c, s); break;
                case 2: DrawCat(vh, c, s); break;
                case 3: DrawDog(vh, c, s); break;
                case 4: DrawEgg(vh, c, s); break;
                case 5: DrawFish(vh, c, s); break;
                case 6: DrawGrapes(vh, c, s); break;
                case 7: DrawHat(vh, c, s); break;
                case 8: DrawIceCream(vh, c, s); break;
                case 9: DrawJuice(vh, c, s); break;
                case 10: DrawKite(vh, c, s); break;
                case 11: DrawLion(vh, c, s); break;
                case 12: DrawMoon(vh, c, s); break;
                case 13: DrawNest(vh, c, s); break;
                case 14: DrawOrange(vh, c, s); break;
                case 15: DrawPig(vh, c, s); break;
                case 16: DrawCrown(vh, c, s); break;
                case 17: DrawRainbow(vh, c + new Vector2(0f, -.08f) * s, s); break;
                case 18: DrawSun(vh, c, s); break;
                case 19: DrawTurtle(vh, c, s); break;
                case 20: DrawUmbrella(vh, c, s); break;
                case 21: DrawVan(vh, c, s); break;
                case 22: DrawWhale(vh, c, s); break;
                case 23: DrawXylophone(vh, c, s); break;
                case 24: DrawYoYo(vh, c, s); break;
                default: DrawZebra(vh, c, s); break;
            }
        }

        void DrawNumberPage(VertexHelper vh, Vector2 c, float s, int page)
        {
            int count = Mathf.Clamp(page + 1, 1, 20);
            Color[] colors = { Pink, Blue, Gold, Green, Purple, Orange, Teal, Red };
            const int columns = 5;
            const int rows = 4;
            float dx = .18f * s;
            float dy = .18f * s;
            float radius = .055f * s;
            Vector2 origin = c + new Vector2(-dx * 2f, dy * 1.5f);

            for (int i = 0; i < count; i++)
            {
                int col = i % columns;
                int row = i / columns;
                Vector2 p = origin + new Vector2(col * dx, -row * dy);
                Color color = colors[i % colors.Length];
                AddCircle(vh, p, radius, color, 20);
                AddStar(vh, p, radius * .55f, radius * .24f, White);
            }

            // Decorative counting rail makes 10+ pages feel intentional instead of crowded.
            AddLine(vh, c + new Vector2(-.46f, -.41f) * s, c + new Vector2(.46f, -.41f) * s, .012f * s, DeepPurple);
            for (int i = 0; i < rows + 1; i++)
            {
                float x = -.40f + i * .20f;
                AddCircle(vh, c + new Vector2(x, -.41f) * s, .018f * s, Gold, 10);
            }
        }

        void DrawColorPage(VertexHelper vh, Vector2 c, float s, int page)
        {
            switch (Mathf.Clamp(page, 0, 11))
            {
                case 0: AddCircle(vh, c, .32f * s, Red, 36); break;
                case 1: AddQuad(vh, c + new Vector2(-.30f, -.30f) * s, c + new Vector2(.30f, .30f) * s, Blue); break;
                case 2: AddTriangle(vh, c + new Vector2(0f, .36f) * s, c + new Vector2(-.36f, -.30f) * s, c + new Vector2(.36f, -.30f) * s, Gold); break;
                case 3: AddStar(vh, c, .36f * s, .17f * s, Green); break;
                case 4: DrawHeart(vh, c, .78f * s, Purple); break;
                case 5: AddQuad(vh, c + new Vector2(-.39f, -.23f) * s, c + new Vector2(.39f, .23f) * s, Orange); break;
                case 6: AddEllipse(vh, c, .37f * s, .25f * s, Pink, 36); break;
                case 7:
                    AddPolygon(vh, new[]
                    {
                        c + new Vector2(0f,.38f)*s,
                        c + new Vector2(.31f,0f)*s,
                        c + new Vector2(0f,-.38f)*s,
                        c + new Vector2(-.31f,0f)*s
                    }, Brown);
                    break;
                case 8: DrawCrescent(vh, c, s, Black); break;
                case 9: DrawCloud(vh, c, s, White); break;
                case 10: AddRegularPolygon(vh, c, .35f * s, 6, Teal, 30f); break;
                default: AddBurst(vh, c, .39f * s, .17f * s, 12, Gold); break;
            }
        }

        void DrawStoryPage(VertexHelper vh, Vector2 c, float s, int page)
        {
            switch (Mathf.Clamp(page, 0, 9))
            {
                case 0:
                    DrawSun(vh, c + new Vector2(.15f, .10f) * s, .70f * s);
                    DrawCloud(vh, c + new Vector2(-.22f, -.15f) * s, .52f * s, White);
                    break;
                case 1:
                    DrawBackpack(vh, c, s);
                    AddStar(vh, c + new Vector2(.30f, .28f) * s, .10f * s, .045f * s, Gold);
                    break;
                case 2:
                    DrawFlower(vh, c, s);
                    break;
                case 3:
                    DrawHeart(vh, c, .78f * s, Pink);
                    AddCircle(vh, c + new Vector2(-.28f, .28f) * s, .055f * s, Gold, 16);
                    AddCircle(vh, c + new Vector2(.30f, .24f) * s, .045f * s, Blue, 16);
                    break;
                case 4:
                    DrawOpenBook(vh, c, s);
                    AddStar(vh, c + new Vector2(0f, .30f) * s, .10f * s, .045f * s, Gold);
                    break;
                case 5:
                    AddArc(vh, c, .22f * s, .27f * s, 20f, 320f, 28, Purple);
                    AddTriangle(vh, c + new Vector2(.26f, -.04f) * s, c + new Vector2(.42f, -.02f) * s, c + new Vector2(.31f, -.16f) * s, Purple);
                    AddStar(vh, c, .15f * s, .07f * s, Gold);
                    break;
                case 6:
                    DrawRainbow(vh, c + new Vector2(0f, -.08f) * s, s);
                    break;
                case 7:
                    DrawLearningEverywhere(vh, c, s);
                    break;
                case 8:
                    DrawTrophy(vh, c, s);
                    break;
                default:
                    DrawCrown(vh, c + new Vector2(0f, .04f) * s, .90f * s);
                    AddStar(vh, c + new Vector2(-.34f, -.24f) * s, .10f * s, .04f * s, Pink);
                    AddStar(vh, c + new Vector2(.34f, -.22f) * s, .09f * s, .035f * s, Blue);
                    break;
            }
        }

        void DrawApple(VertexHelper vh, Vector2 c, float s)
        {
            AddCircle(vh, c + new Vector2(-.11f, -.02f) * s, .24f * s, Red, 28);
            AddCircle(vh, c + new Vector2(.11f, -.02f) * s, .24f * s, Red, 28);
            AddEllipse(vh, c + new Vector2(0f, -.14f) * s, .30f * s, .24f * s, Red, 30);
            AddLine(vh, c + new Vector2(0f, .18f) * s, c + new Vector2(.02f, .38f) * s, .035f * s, Brown);
            AddEllipseRotated(vh, c + new Vector2(.14f, .31f) * s, .17f * s, .08f * s, 28f, Green, 18);
        }

        void DrawBall(VertexHelper vh, Vector2 c, float s)
        {
            AddCircle(vh, c, .36f * s, Blue, 34);
            AddArc(vh, c, .26f * s, .31f * s, -75f, 105f, 20, White);
            AddArc(vh, c, .11f * s, .16f * s, 20f, 205f, 20, Gold);
        }

        void DrawCat(VertexHelper vh, Vector2 c, float s)
        {
            AddCircle(vh, c, .30f * s, Orange, 30);
            AddTriangle(vh, c + new Vector2(-.25f, .18f) * s, c + new Vector2(-.10f, .47f) * s, c + new Vector2(-.03f, .18f) * s, Orange);
            AddTriangle(vh, c + new Vector2(.25f, .18f) * s, c + new Vector2(.10f, .47f) * s, c + new Vector2(.03f, .18f) * s, Orange);
            DrawFace(vh, c, s, Pink);
            AddLine(vh, c + new Vector2(-.05f, -.10f) * s, c + new Vector2(-.29f, -.16f) * s, .012f * s, Black);
            AddLine(vh, c + new Vector2(.05f, -.10f) * s, c + new Vector2(.29f, -.16f) * s, .012f * s, Black);
        }

        void DrawDog(VertexHelper vh, Vector2 c, float s)
        {
            AddCircle(vh, c, .30f * s, Brown, 30);
            AddEllipseRotated(vh, c + new Vector2(-.28f, .08f) * s, .12f * s, .25f * s, -18f, DeepPurple, 18);
            AddEllipseRotated(vh, c + new Vector2(.28f, .08f) * s, .12f * s, .25f * s, 18f, DeepPurple, 18);
            DrawFace(vh, c, s, Black);
            AddEllipse(vh, c + new Vector2(0f, -.18f) * s, .07f * s, .09f * s, Pink, 16);
        }

        void DrawEgg(VertexHelper vh, Vector2 c, float s)
        {
            AddEllipse(vh, c, .28f * s, .39f * s, White, 34);
            AddCircle(vh, c + new Vector2(0f, -.04f) * s, .15f * s, Gold, 26);
        }

        void DrawFish(VertexHelper vh, Vector2 c, float s)
        {
            AddEllipse(vh, c + new Vector2(-.04f, 0f) * s, .33f * s, .22f * s, Blue, 30);
            AddTriangle(vh, c + new Vector2(.25f, 0f) * s, c + new Vector2(.48f, .22f) * s, c + new Vector2(.48f, -.22f) * s, Pink);
            AddCircle(vh, c + new Vector2(-.20f, .06f) * s, .04f * s, White, 14);
            AddCircle(vh, c + new Vector2(-.20f, .06f) * s, .018f * s, Black, 10);
        }

        void DrawGrapes(VertexHelper vh, Vector2 c, float s)
        {
            for (int row = 0; row < 4; row++)
            {
                int count = 4 - row;
                float y = .24f - row * .15f;
                float start = -(count - 1) * .075f;
                for (int i = 0; i < count; i++)
                    AddCircle(vh, c + new Vector2(start + i * .15f, y) * s, .085f * s, Purple, 18);
            }
            AddLine(vh, c + new Vector2(0f, .31f) * s, c + new Vector2(.03f, .45f) * s, .025f * s, Brown);
            AddEllipseRotated(vh, c + new Vector2(.14f, .40f) * s, .13f * s, .06f * s, 25f, Green, 16);
        }

        void DrawHat(VertexHelper vh, Vector2 c, float s)
        {
            AddQuad(vh, c + new Vector2(-.32f, -.10f) * s, c + new Vector2(.32f, .02f) * s, DeepPurple);
            AddQuad(vh, c + new Vector2(-.20f, -.02f) * s, c + new Vector2(.20f, .28f) * s, Pink);
            AddQuad(vh, c + new Vector2(-.21f, -.01f) * s, c + new Vector2(.21f, .07f) * s, Gold);
        }

        void DrawIceCream(VertexHelper vh, Vector2 c, float s)
        {
            AddTriangle(vh, c + new Vector2(-.19f, -.04f) * s, c + new Vector2(.19f, -.04f) * s, c + new Vector2(0f, -.43f) * s, Orange);
            AddCircle(vh, c + new Vector2(-.12f, .09f) * s, .16f * s, Pink, 22);
            AddCircle(vh, c + new Vector2(.12f, .09f) * s, .16f * s, Purple, 22);
            AddCircle(vh, c + new Vector2(0f, .25f) * s, .16f * s, Cream, 22);
        }

        void DrawJuice(VertexHelper vh, Vector2 c, float s)
        {
            AddQuad(vh, c + new Vector2(-.23f, -.34f) * s, c + new Vector2(.23f, .25f) * s, Pink);
            AddQuad(vh, c + new Vector2(-.20f, -.28f) * s, c + new Vector2(.20f, .10f) * s, Orange);
            AddLine(vh, c + new Vector2(.08f, .20f) * s, c + new Vector2(.22f, .45f) * s, .025f * s, Teal);
        }

        void DrawKite(VertexHelper vh, Vector2 c, float s)
        {
            AddPolygon(vh, new[]
            {
                c + new Vector2(0f,.40f)*s,
                c + new Vector2(.28f,.05f)*s,
                c + new Vector2(0f,-.28f)*s,
                c + new Vector2(-.28f,.05f)*s
            }, Pink);
            AddLine(vh, c + new Vector2(0f, -.28f) * s, c + new Vector2(.20f, -.48f) * s, .018f * s, Purple);
            AddTriangle(vh, c + new Vector2(.08f, -.37f) * s, c + new Vector2(.18f, -.31f) * s, c + new Vector2(.14f, -.43f) * s, Gold);
        }

        void DrawLion(VertexHelper vh, Vector2 c, float s)
        {
            AddCircle(vh, c, .38f * s, Orange, 34);
            AddCircle(vh, c, .27f * s, Gold, 30);
            DrawFace(vh, c, s, Brown);
        }

        void DrawMoon(VertexHelper vh, Vector2 c, float s) => DrawCrescent(vh, c, s, Purple);

        void DrawNest(VertexHelper vh, Vector2 c, float s)
        {
            AddArc(vh, c + new Vector2(0f, -.02f) * s, .22f * s, .34f * s, 200f, 340f, 20, Brown);
            for (int i = -2; i <= 2; i++)
                AddLine(vh, c + new Vector2(-.32f, -.10f + i * .025f) * s, c + new Vector2(.32f, -.02f - i * .018f) * s, .018f * s, Brown);
            AddEllipse(vh, c + new Vector2(-.10f, .08f) * s, .08f * s, .11f * s, Blue, 18);
            AddEllipse(vh, c + new Vector2(.10f, .08f) * s, .08f * s, .11f * s, Cream, 18);
        }

        void DrawOrange(VertexHelper vh, Vector2 c, float s)
        {
            AddCircle(vh, c, .31f * s, Orange, 32);
            AddEllipseRotated(vh, c + new Vector2(.10f, .30f) * s, .15f * s, .07f * s, 25f, Green, 16);
        }

        void DrawPig(VertexHelper vh, Vector2 c, float s)
        {
            AddCircle(vh, c, .31f * s, Pink, 30);
            AddTriangle(vh, c + new Vector2(-.26f, .20f) * s, c + new Vector2(-.16f, .43f) * s, c + new Vector2(-.06f, .22f) * s, Pink);
            AddTriangle(vh, c + new Vector2(.26f, .20f) * s, c + new Vector2(.16f, .43f) * s, c + new Vector2(.06f, .22f) * s, Pink);
            AddEllipse(vh, c + new Vector2(0f, -.10f) * s, .15f * s, .10f * s, HotPink, 18);
            AddCircle(vh, c + new Vector2(-.055f, -.10f) * s, .018f * s, Black, 10);
            AddCircle(vh, c + new Vector2(.055f, -.10f) * s, .018f * s, Black, 10);
            AddCircle(vh, c + new Vector2(-.11f, .07f) * s, .035f * s, Black, 12);
            AddCircle(vh, c + new Vector2(.11f, .07f) * s, .035f * s, Black, 12);
        }

        void DrawCrown(VertexHelper vh, Vector2 c, float s)
        {
            AddPolygon(vh, new[]
            {
                c + new Vector2(-.36f,-.18f)*s,
                c + new Vector2(-.31f,.28f)*s,
                c + new Vector2(-.10f,.08f)*s,
                c + new Vector2(0f,.38f)*s,
                c + new Vector2(.10f,.08f)*s,
                c + new Vector2(.31f,.28f)*s,
                c + new Vector2(.36f,-.18f)*s
            }, Gold);
            AddQuad(vh, c + new Vector2(-.36f, -.22f) * s, c + new Vector2(.36f, -.12f) * s, Purple);
            AddCircle(vh, c + new Vector2(0f, -.17f) * s, .045f * s, Pink, 14);
        }

        void DrawSun(VertexHelper vh, Vector2 c, float s)
        {
            AddCircle(vh, c, .21f * s, Gold, 28);
            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI * 2f / 12f;
                Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                AddLine(vh, c + d * (.27f * s), c + d * (.42f * s), .025f * s, Orange);
            }
        }

        void DrawTurtle(VertexHelper vh, Vector2 c, float s)
        {
            AddEllipse(vh, c, .33f * s, .23f * s, Green, 28);
            AddCircle(vh, c + new Vector2(.34f, .03f) * s, .11f * s, Teal, 18);
            AddCircle(vh, c + new Vector2(.38f, .06f) * s, .018f * s, Black, 10);
            AddCircle(vh, c + new Vector2(-.22f, -.20f) * s, .065f * s, Teal, 14);
            AddCircle(vh, c + new Vector2(.18f, -.20f) * s, .065f * s, Teal, 14);
            AddArc(vh, c, .12f * s, .14f * s, 0f, 360f, 24, Gold);
        }

        void DrawUmbrella(VertexHelper vh, Vector2 c, float s)
        {
            AddArc(vh, c + new Vector2(0f, .02f) * s, .03f * s, .36f * s, 10f, 170f, 28, Pink);
            AddLine(vh, c + new Vector2(0f, .02f) * s, c + new Vector2(0f, -.35f) * s, .026f * s, DeepPurple);
            AddArc(vh, c + new Vector2(.08f, -.34f) * s, .08f * s, .11f * s, 190f, 350f, 16, DeepPurple);
        }

        void DrawVan(VertexHelper vh, Vector2 c, float s)
        {
            AddQuad(vh, c + new Vector2(-.38f, -.20f) * s, c + new Vector2(.34f, .18f) * s, Blue);
            AddQuad(vh, c + new Vector2(.05f, .02f) * s, c + new Vector2(.29f, .15f) * s, Cream);
            AddQuad(vh, c + new Vector2(-.28f, .02f) * s, c + new Vector2(-.03f, .15f) * s, Cream);
            AddCircle(vh, c + new Vector2(-.22f, -.22f) * s, .09f * s, Black, 18);
            AddCircle(vh, c + new Vector2(.20f, -.22f) * s, .09f * s, Black, 18);
            AddCircle(vh, c + new Vector2(-.22f, -.22f) * s, .04f * s, White, 14);
            AddCircle(vh, c + new Vector2(.20f, -.22f) * s, .04f * s, White, 14);
        }

        void DrawWhale(VertexHelper vh, Vector2 c, float s)
        {
            AddEllipse(vh, c + new Vector2(-.04f, -.02f) * s, .34f * s, .23f * s, Blue, 30);
            AddTriangle(vh, c + new Vector2(.28f, 0f) * s, c + new Vector2(.47f, .18f) * s, c + new Vector2(.43f, -.05f) * s, Blue);
            AddTriangle(vh, c + new Vector2(.28f, 0f) * s, c + new Vector2(.47f, -.18f) * s, c + new Vector2(.43f, .05f) * s, Blue);
            AddCircle(vh, c + new Vector2(-.22f, .05f) * s, .018f * s, Black, 10);
            AddArc(vh, c + new Vector2(-.08f, .18f) * s, .12f * s, .14f * s, 70f, 110f, 10, Teal);
            AddCircle(vh, c + new Vector2(-.11f, .37f) * s, .035f * s, Teal, 12);
            AddCircle(vh, c + new Vector2(.01f, .39f) * s, .025f * s, Teal, 12);
        }

        void DrawXylophone(VertexHelper vh, Vector2 c, float s)
        {
            Color[] colors = { Red, Orange, Gold, Green, Blue, Purple };
            for (int i = 0; i < colors.Length; i++)
            {
                float x0 = -.34f + i * .115f;
                float h = .42f - i * .045f;
                AddQuad(vh, c + new Vector2(x0, -.20f) * s, c + new Vector2(x0 + .09f, -.20f + h) * s, colors[i]);
            }
            AddLine(vh, c + new Vector2(-.31f, -.32f) * s, c + new Vector2(.31f, .34f) * s, .026f * s, Brown);
            AddCircle(vh, c + new Vector2(.31f, .34f) * s, .055f * s, Pink, 14);
        }

        void DrawYoYo(VertexHelper vh, Vector2 c, float s)
        {
            AddCircle(vh, c + new Vector2(0f, -.06f) * s, .25f * s, Pink, 28);
            AddCircle(vh, c + new Vector2(0f, -.06f) * s, .15f * s, Purple, 22);
            AddCircle(vh, c + new Vector2(0f, -.06f) * s, .045f * s, Gold, 14);
            AddLine(vh, c + new Vector2(0f, .19f) * s, c + new Vector2(.18f, .44f) * s, .012f * s, Black);
            AddCircle(vh, c + new Vector2(.19f, .45f) * s, .025f * s, Black, 10);
        }

        void DrawZebra(VertexHelper vh, Vector2 c, float s)
        {
            AddEllipse(vh, c, .25f * s, .34f * s, White, 30);
            AddTriangle(vh, c + new Vector2(-.20f, .23f) * s, c + new Vector2(-.12f, .47f) * s, c + new Vector2(-.02f, .25f) * s, White);
            AddTriangle(vh, c + new Vector2(.20f, .23f) * s, c + new Vector2(.12f, .47f) * s, c + new Vector2(.02f, .25f) * s, White);
            for (int i = -2; i <= 2; i++)
            {
                float y = i * .10f;
                AddLine(vh, c + new Vector2(-.20f, y + .10f) * s, c + new Vector2(.16f, y - .04f) * s, .045f * s, Black);
            }
            AddCircle(vh, c + new Vector2(-.09f, .06f) * s, .027f * s, Black, 10);
            AddCircle(vh, c + new Vector2(.09f, .06f) * s, .027f * s, Black, 10);
        }

        void DrawCrescent(VertexHelper vh, Vector2 c, float s, Color color)
        {
            AddCircle(vh, c, .34f * s, color, 32);
            AddCircle(vh, c + new Vector2(.16f, .10f) * s, .31f * s, PageBg, 32);
        }

        void DrawCloud(VertexHelper vh, Vector2 c, float s, Color color)
        {
            AddCircle(vh, c + new Vector2(-.20f, -.02f) * s, .16f * s, color, 20);
            AddCircle(vh, c + new Vector2(0f, .10f) * s, .22f * s, color, 24);
            AddCircle(vh, c + new Vector2(.22f, -.02f) * s, .16f * s, color, 20);
            AddQuad(vh, c + new Vector2(-.30f, -.16f) * s, c + new Vector2(.32f, .02f) * s, color);
        }

        void DrawRainbow(VertexHelper vh, Vector2 c, float s)
        {
            Color[] cols = { Red, Orange, Gold, Green, Blue, Purple };
            for (int i = 0; i < cols.Length; i++)
            {
                float outer = (.40f - i * .045f) * s;
                AddArc(vh, c, outer - .035f * s, outer, 15f, 165f, 24, cols[i]);
            }
            DrawCloud(vh, c + new Vector2(-.34f, -.02f) * s, .34f * s, White);
            DrawCloud(vh, c + new Vector2(.34f, -.02f) * s, .34f * s, White);
        }

        void DrawHeart(VertexHelper vh, Vector2 c, float size, Color color)
        {
            float r = size * .20f;
            AddCircle(vh, c + new Vector2(-r, r * .55f), r, color, 22);
            AddCircle(vh, c + new Vector2(r, r * .55f), r, color, 22);
            AddTriangle(vh, c + new Vector2(-r * 2f, r * .55f), c + new Vector2(r * 2f, r * .55f), c + new Vector2(0f, -r * 2.5f), color);
        }

        void DrawFace(VertexHelper vh, Vector2 c, float s, Color noseColor)
        {
            AddCircle(vh, c + new Vector2(-.11f, .06f) * s, .037f * s, Black, 12);
            AddCircle(vh, c + new Vector2(.11f, .06f) * s, .037f * s, Black, 12);
            AddCircle(vh, c + new Vector2(0f, -.06f) * s, .04f * s, noseColor, 12);
            AddArc(vh, c + new Vector2(0f, -.10f) * s, .08f * s, .095f * s, 205f, 335f, 12, Black);
        }

        void DrawBackpack(VertexHelper vh, Vector2 c, float s)
        {
            AddQuad(vh, c + new Vector2(-.25f, -.34f) * s, c + new Vector2(.25f, .28f) * s, Pink);
            AddArc(vh, c + new Vector2(0f, .24f) * s, .12f * s, .17f * s, 0f, 180f, 18, Purple);
            AddQuad(vh, c + new Vector2(-.19f, -.24f) * s, c + new Vector2(.19f, -.03f) * s, Purple);
            DrawHeart(vh, c + new Vector2(0f, .09f) * s, .24f * s, Gold);
        }

        void DrawFlower(VertexHelper vh, Vector2 c, float s)
        {
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI * 2f / 6f;
                Vector2 p = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (.19f * s);
                AddEllipseRotated(vh, p, .13f * s, .08f * s, i * 60f, Pink, 16);
            }
            AddCircle(vh, c, .11f * s, Gold, 18);
            AddLine(vh, c + new Vector2(0f, -.10f) * s, c + new Vector2(0f, -.43f) * s, .025f * s, Green);
            AddEllipseRotated(vh, c + new Vector2(.10f, -.30f) * s, .11f * s, .05f * s, 25f, Green, 14);
        }

        void DrawOpenBook(VertexHelper vh, Vector2 c, float s)
        {
            AddPolygon(vh, new[]
            {
                c + new Vector2(-.40f,.22f)*s,
                c + new Vector2(-.04f,.12f)*s,
                c + new Vector2(-.04f,-.27f)*s,
                c + new Vector2(-.40f,-.18f)*s
            }, White);
            AddPolygon(vh, new[]
            {
                c + new Vector2(.40f,.22f)*s,
                c + new Vector2(.04f,.12f)*s,
                c + new Vector2(.04f,-.27f)*s,
                c + new Vector2(.40f,-.18f)*s
            }, White);
            AddLine(vh, c + new Vector2(0f, .12f) * s, c + new Vector2(0f, -.27f) * s, .018f * s, Purple);
            for (int i = 0; i < 3; i++)
            {
                float y = .08f - i * .10f;
                AddLine(vh, c + new Vector2(-.34f, y) * s, c + new Vector2(-.09f, y - .04f) * s, .010f * s, Pink);
                AddLine(vh, c + new Vector2(.09f, y - .04f) * s, c + new Vector2(.34f, y) * s, .010f * s, Blue);
            }
        }

        void DrawLearningEverywhere(VertexHelper vh, Vector2 c, float s)
        {
            AddCircle(vh, c + new Vector2(-.25f, .18f) * s, .11f * s, Red, 18);
            AddStar(vh, c + new Vector2(.25f, .18f) * s, .12f * s, .05f * s, Gold);
            AddQuad(vh, c + new Vector2(-.34f, -.29f) * s, c + new Vector2(-.12f, -.07f) * s, Blue);
            AddTriangle(vh, c + new Vector2(.22f, -.05f) * s, c + new Vector2(.08f, -.30f) * s, c + new Vector2(.36f, -.30f) * s, Green);
            DrawHeart(vh, c, .30f * s, Pink);
        }

        void DrawTrophy(VertexHelper vh, Vector2 c, float s)
        {
            AddQuad(vh, c + new Vector2(-.20f, -.03f) * s, c + new Vector2(.20f, .31f) * s, Gold);
            AddArc(vh, c + new Vector2(-.20f, .14f) * s, .08f * s, .12f * s, 90f, 270f, 16, Gold);
            AddArc(vh, c + new Vector2(.20f, .14f) * s, .08f * s, .12f * s, -90f, 90f, 16, Gold);
            AddQuad(vh, c + new Vector2(-.05f, -.22f) * s, c + new Vector2(.05f, -.03f) * s, Purple);
            AddQuad(vh, c + new Vector2(-.20f, -.30f) * s, c + new Vector2(.20f, -.22f) * s, Purple);
            AddStar(vh, c + new Vector2(0f, .14f) * s, .10f * s, .045f * s, Pink);
        }

        void AddRegularPolygon(VertexHelper vh, Vector2 center, float radius, int sides, Color32 color, float rotationDegrees = 0f)
        {
            Vector2[] pts = new Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float a = (rotationDegrees + i * 360f / sides) * Mathf.Deg2Rad;
                pts[i] = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            }
            AddPolygon(vh, pts, color);
        }

        void AddBurst(VertexHelper vh, Vector2 center, float outer, float inner, int points, Color32 color)
        {
            Vector2[] pts = new Vector2[points * 2];
            for (int i = 0; i < pts.Length; i++)
            {
                float radius = (i & 1) == 0 ? outer : inner;
                float a = (-90f + i * 180f / points) * Mathf.Deg2Rad;
                pts[i] = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            }
            AddPolygon(vh, pts, color);
        }

        void AddCircle(VertexHelper vh, Vector2 center, float radius, Color32 color, int segments)
        {
            int start = vh.currentVertCount;
            AddVertex(vh, center, color);
            for (int i = 0; i <= segments; i++)
            {
                float a = Mathf.PI * 2f * i / segments;
                AddVertex(vh, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, color);
            }
            for (int i = 0; i < segments; i++) vh.AddTriangle(start, start + i + 1, start + i + 2);
        }

        void AddEllipse(VertexHelper vh, Vector2 center, float rx, float ry, Color32 color, int segments)
        {
            int start = vh.currentVertCount;
            AddVertex(vh, center, color);
            for (int i = 0; i <= segments; i++)
            {
                float a = Mathf.PI * 2f * i / segments;
                AddVertex(vh, center + new Vector2(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry), color);
            }
            for (int i = 0; i < segments; i++) vh.AddTriangle(start, start + i + 1, start + i + 2);
        }

        void AddEllipseRotated(VertexHelper vh, Vector2 center, float rx, float ry, float degrees, Color32 color, int segments)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cs = Mathf.Cos(rad), sn = Mathf.Sin(rad);
            int start = vh.currentVertCount;
            AddVertex(vh, center, color);
            for (int i = 0; i <= segments; i++)
            {
                float a = Mathf.PI * 2f * i / segments;
                Vector2 p = new Vector2(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry);
                p = new Vector2(p.x * cs - p.y * sn, p.x * sn + p.y * cs);
                AddVertex(vh, center + p, color);
            }
            for (int i = 0; i < segments; i++) vh.AddTriangle(start, start + i + 1, start + i + 2);
        }

        void AddStar(VertexHelper vh, Vector2 center, float outer, float inner, Color32 color)
        {
            Vector2[] pts = new Vector2[10];
            for (int i = 0; i < pts.Length; i++)
            {
                float radius = (i & 1) == 0 ? outer : inner;
                float a = (-90f + i * 36f) * Mathf.Deg2Rad;
                pts[i] = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            }
            AddPolygon(vh, pts, color);
        }

        void AddArc(VertexHelper vh, Vector2 center, float inner, float outer, float startDeg, float endDeg, int segments, Color32 color)
        {
            for (int i = 0; i < segments; i++)
            {
                float a0 = Mathf.Lerp(startDeg, endDeg, i / (float)segments) * Mathf.Deg2Rad;
                float a1 = Mathf.Lerp(startDeg, endDeg, (i + 1) / (float)segments) * Mathf.Deg2Rad;
                Vector2 i0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * inner;
                Vector2 i1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * inner;
                Vector2 o0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * outer;
                Vector2 o1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * outer;
                AddQuad(vh, i0, i1, o1, o0, color);
            }
        }

        void AddLine(VertexHelper vh, Vector2 a, Vector2 b, float width, Color32 color)
        {
            Vector2 d = (b - a).normalized;
            Vector2 n = new Vector2(-d.y, d.x) * (width * .5f);
            AddQuad(vh, a - n, a + n, b + n, b - n, color);
        }

        void AddTriangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color32 color)
        {
            int start = vh.currentVertCount;
            AddVertex(vh, a, color);
            AddVertex(vh, b, color);
            AddVertex(vh, c, color);
            vh.AddTriangle(start, start + 1, start + 2);
        }

        void AddPolygon(VertexHelper vh, Vector2[] points, Color32 color)
        {
            if (points == null || points.Length < 3) return;
            Vector2 center = Vector2.zero;
            foreach (Vector2 point in points) center += point;
            center /= points.Length;
            int start = vh.currentVertCount;
            AddVertex(vh, center, color);
            foreach (Vector2 point in points) AddVertex(vh, point, color);
            for (int i = 0; i < points.Length; i++)
                vh.AddTriangle(start, start + 1 + i, start + 1 + ((i + 1) % points.Length));
        }

        void AddQuad(VertexHelper vh, Vector2 min, Vector2 max, Color32 color)
        {
            AddQuad(vh,
                new Vector2(min.x, min.y),
                new Vector2(max.x, min.y),
                new Vector2(max.x, max.y),
                new Vector2(min.x, max.y),
                color);
        }

        void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color32 color)
        {
            int start = vh.currentVertCount;
            AddVertex(vh, a, color);
            AddVertex(vh, b, color);
            AddVertex(vh, c, color);
            AddVertex(vh, d, color);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }

        void AddVertex(VertexHelper vh, Vector2 p, Color32 color)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = p;
            vertex.color = color;
            vh.AddVert(vertex);
        }

        static Color Hex(string hex)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            return ColorUtility.TryParseHtmlString(hex, out Color c) ? c : Color.white;
        }
    }
}
