#if UNITY_EDITOR
using System.IO;
using System.Linq;
using LearningWithJourney.Games;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJCountingWorldLevelsV4
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/CountingWorld.unity";

        [MenuItem("Learning with Journey/Upgrade Counting World Levels V4")]
        public static void Apply()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "CountingWorld.unity was not found. Build Counting World V2 and apply Interactive V3 first.",
                    "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var controllerGo = GameObject.Find("CountingWorldController");
            if (controllerGo == null)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "CountingWorldController was not found. Apply Interactive V3 first.",
                    "OK");
                return;
            }

            var grid = GameObject.Find("ObjectGrid");
            if (grid == null)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "ObjectGrid was not found. Apply Interactive V3 first.",
                    "OK");
                return;
            }

            RemoveOlderControllers(controllerGo);
            var controller = controllerGo.GetComponent<CountingWorldPlayControllerV4>();
            if (controller == null)
                controller = controllerGo.AddComponent<CountingWorldPlayControllerV4>();

            var audio = controllerGo.GetComponent<AudioSource>();
            if (audio == null)
                audio = controllerGo.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.loop = false;
            audio.spatialBlend = 0f;

            var apples = grid.transform.Cast<Transform>()
                .Select(t => t.gameObject)
                .Where(go => go.name.StartsWith("Apple"))
                .Take(20)
                .ToArray();

            var badges = new TMP_Text[apples.Length];
            for (int i = 0; i < apples.Length; i++)
            {
                var apple = apples[i];
                if (apple == null) continue;

                var image = apple.GetComponent<Image>();
                if (image != null) image.raycastTarget = true;

                var button = apple.GetComponent<Button>();
                if (button == null) button = apple.AddComponent<Button>();
                button.targetGraphic = image;
                button.navigation = new Navigation { mode = Navigation.Mode.None };

                var badgeText = apple.transform.Find("V3CountBadge/Number")?.GetComponent<TMP_Text>();
                badges[i] = badgeText;
            }

            var answers = new[]
            {
                GameObject.Find("AnswerA")?.GetComponent<Button>(),
                GameObject.Find("AnswerB")?.GetComponent<Button>(),
                GameObject.Find("AnswerC")?.GetComponent<Button>()
            }.Where(b => b != null).ToArray();

            var speech = GameObject.Find("JourneyCountingBubble")?.transform.Find("SpeechText")?.GetComponent<TMP_Text>();
            var prompt = GameObject.Find("PromptText")?.GetComponent<TMP_Text>();
            var feedback = GameObject.Find("FeedbackText")?.GetComponent<TMP_Text>();
            var round = GameObject.Find("RoundText")?.GetComponent<TMP_Text>();
            var points = GameObject.Find("PointsPill")?.transform.Find("Count")?.GetComponent<TMP_Text>();
            var level = GameObject.Find("LevelPill")?.transform.Find("Level")?.GetComponent<TMP_Text>();
            var journey = GameObject.Find("Journey")?.GetComponent<RectTransform>();

            var so = new SerializedObject(controller);
            SetObjectArray(so.FindProperty("countObjects"), apples.Cast<Object>().ToArray());
            SetObjectArray(so.FindProperty("countBadges"), badges.Cast<Object>().ToArray());
            SetObjectArray(so.FindProperty("answerButtons"), answers.Cast<Object>().ToArray());
            so.FindProperty("promptText").objectReferenceValue = prompt;
            so.FindProperty("speechText").objectReferenceValue = speech;
            so.FindProperty("feedbackText").objectReferenceValue = feedback;
            so.FindProperty("roundText").objectReferenceValue = round;
            so.FindProperty("pointsText").objectReferenceValue = points;
            so.FindProperty("levelText").objectReferenceValue = level;
            so.FindProperty("journeyRect").objectReferenceValue = journey;
            so.FindProperty("numberAudioSource").objectReferenceValue = audio;
            so.FindProperty("totalLevels").intValue = 10;
            so.FindProperty("roundsPerLevel").intValue = 5;
            so.ApplyModifiedPropertiesWithoutUndo();

            RewireHomeButton(controller);
            UpdateHudLabels();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Counting World V4 is ready with 10 real game levels. Each level has 5 successful rounds, counting difficulty increases gradually from 1-3 apples up to 13-20 apples, progress is saved, and completing Level 10 finishes Counting World. Journey and her backpack placement were left unchanged.",
                "OK");
        }

        static void RemoveOlderControllers(GameObject controllerGo)
        {
            var v3 = controllerGo.GetComponent<CountingWorldPlayControllerV3>();
            if (v3 != null) Object.DestroyImmediate(v3);

            var v2 = controllerGo.GetComponent<CountingWorldPlayControllerV2>();
            if (v2 != null) Object.DestroyImmediate(v2);

            var v1 = controllerGo.GetComponent<CountingWorldPlayController>();
            if (v1 != null) Object.DestroyImmediate(v1);
        }

        static void RewireHomeButton(CountingWorldPlayControllerV4 controller)
        {
            var back = GameObject.Find("BackButton")?.GetComponent<Button>();
            if (back == null) return;

            back.onClick.RemoveAllListeners();
            while (back.onClick.GetPersistentEventCount() > 0)
                UnityEventTools.RemovePersistentListener(back.onClick, 0);
            UnityEventTools.AddPersistentListener(back.onClick, controller.GoHome);
        }

        static void UpdateHudLabels()
        {
            var subtitle = GameObject.Find("CountingSubtitle")?.GetComponent<TMP_Text>();
            if (subtitle != null)
            {
                subtitle.text = "NUMBERS 1-20  |  10 LEVELS";
                subtitle.fontSize = 20f;
            }

            var level = GameObject.Find("LevelPill")?.transform.Find("Level")?.GetComponent<TMP_Text>();
            if (level != null)
            {
                level.text = "LEVEL 1 / 10";
                level.fontSize = 19f;
            }

            var round = GameObject.Find("RoundText")?.GetComponent<TMP_Text>();
            if (round != null)
                round.text = "ROUND 1 / 5";

            var instruction = GameObject.Find("AnswerInstruction")?.GetComponent<TMP_Text>();
            if (instruction != null)
                instruction.text = "TOUCH EACH APPLE TO COUNT";
        }

        static void SetObjectArray(SerializedProperty property, Object[] values)
        {
            if (property == null) return;
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
#endif
