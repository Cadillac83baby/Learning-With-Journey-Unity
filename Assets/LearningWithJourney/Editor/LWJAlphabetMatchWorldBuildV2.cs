#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJAlphabetMatchWorldBuildV2
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/AlphabetMatchWorld.unity";

        [MenuItem("Learning with Journey/Build Alphabet Match World V2 Recommended")]
        public static void Build()
        {
            LWJAlphabetMatchWorldBuilderV1.Build();

            if (!File.Exists(ScenePath)) return;
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // V1 used temporary sibling shadow blocks while constructing the reusable card grid.
            // Remove those blocks and place the shadow directly on each moving card so the
            // runtime 2/3/4-pair layouts stay clean and aligned.
            for (int i = 1; i <= 8; i++)
            {
                var oldShadow = GameObject.Find("MatchCardShadow" + i);
                if (oldShadow != null)
                    Object.DestroyImmediate(oldShadow);

                var card = GameObject.Find("MatchCard" + i);
                if (card == null) continue;

                var image = card.GetComponent<Image>();
                if (image != null)
                {
                    image.color = new Color(.19f, .09f, .34f, .96f);
                    image.raycastTarget = true;
                }

                var shadow = card.GetComponent<Shadow>();
                if (shadow == null) shadow = card.AddComponent<Shadow>();
                shadow.effectColor = new Color(.12f, .05f, .22f, .42f);
                shadow.effectDistance = new Vector2(0f, -7f);
                shadow.useGraphicAlpha = true;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Alphabet Match World V2 is built and polished. The matching cards now move cleanly into 2-pair, 3-pair, and 4-pair layouts without stray shadow blocks.",
                "OK");
        }
    }
}
#endif
