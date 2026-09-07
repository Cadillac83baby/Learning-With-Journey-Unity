#if UNITY_EDITOR
using System.IO;
using LearningWithJourney.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJBookReaderBuilderV2
    {
        const string ScenePath = "Assets/LearningWithJourney/Scenes/BookReader.unity";

        [MenuItem("Learning with Journey/Build Complete Book Reader V2")]
        public static void Build()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                EditorUtility.DisplayDialog(
                    "Learning with Journey",
                    "BookReader.unity was not found. Run Build Book Reader V1 once, then run Build Complete Book Reader V2.",
                    "OK");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            TMP_Text bookTitle = FindComponent<TMP_Text>("BookTitle");
            TMP_Text pageNumber = FindComponent<TMP_Text>("PageNumber");
            TMP_Text pageHeading = FindComponent<TMP_Text>("PageHeading");
            TMP_Text pageBody = FindComponent<TMP_Text>("PageBody");
            TMP_Text journeySpeech = FindComponent<TMP_Text>("Speech");
            Button previous = FindComponent<Button>("PreviousPage");
            Button readAgain = FindComponent<Button>("ReadAgain");
            Button next = FindComponent<Button>("NextPage");
            Button back = FindComponent<Button>("BackToLibrary");
            TMP_Text nextText = next != null ? next.GetComponentInChildren<TMP_Text>(true) : null;

            GameObject artworkGo = FindGameObject("PageArtwork");
            if (artworkGo == null)
            {
                EditorUtility.DisplayDialog("Learning with Journey", "The V1 PageArtwork object was not found. Rebuild Book Reader V1, then run V2 again.", "OK");
                return;
            }

            var oldArtwork = artworkGo.GetComponent<BookPageArtworkV1>();
            if (oldArtwork != null) Object.DestroyImmediate(oldArtwork);
            var artwork = artworkGo.GetComponent<BookPageArtworkV2>();
            if (artwork == null) artwork = artworkGo.AddComponent<BookPageArtworkV2>();
            artwork.raycastTarget = false;

            GameObject voiceGo = FindGameObject("JourneyVoice");
            if (voiceGo == null)
            {
                voiceGo = new GameObject("JourneyVoice");
            }

            var oldVoice = voiceGo.GetComponent<JourneyVoicePlayerV1>();
            if (oldVoice != null) Object.DestroyImmediate(oldVoice);
            var voice = voiceGo.GetComponent<JourneyVoicePlayerV2>();
            if (voice == null) voice = voiceGo.AddComponent<JourneyVoicePlayerV2>();

            AudioSource voiceSource = voiceGo.GetComponent<AudioSource>();
            if (voiceSource == null) voiceSource = voiceGo.AddComponent<AudioSource>();
            voiceSource.playOnAwake = false;
            voiceSource.loop = false;
            voiceSource.spatialBlend = 0f;

            var voiceSO = new SerializedObject(voice);
            var voiceSourceProp = voiceSO.FindProperty("voiceSource");
            if (voiceSourceProp != null) voiceSourceProp.objectReferenceValue = voiceSource;
            voiceSO.ApplyModifiedPropertiesWithoutUndo();

            GameObject controllerGo = FindGameObject("BookReaderController");
            if (controllerGo == null) controllerGo = new GameObject("BookReaderController");

            var oldController = controllerGo.GetComponent<BookReaderControllerV1>();
            if (oldController != null) Object.DestroyImmediate(oldController);
            var controller = controllerGo.GetComponent<BookReaderControllerV2>();
            if (controller == null) controller = controllerGo.AddComponent<BookReaderControllerV2>();

            var so = new SerializedObject(controller);
            SetObject(so, "bookTitleText", bookTitle);
            SetObject(so, "pageHeadingText", pageHeading);
            SetObject(so, "pageBodyText", pageBody);
            SetObject(so, "pageNumberText", pageNumber);
            SetObject(so, "journeySpeechText", journeySpeech);
            SetObject(so, "pageArtwork", artwork);
            SetObject(so, "previousButton", previous);
            SetObject(so, "nextButton", next);
            SetObject(so, "readAgainButton", readAgain);
            SetObject(so, "nextButtonText", nextText);
            SetObject(so, "journeyVoice", voice);
            SetBool(so, "autoReadFirstPage", true);
            SetBool(so, "autoReadOnPageTurn", true);
            so.ApplyModifiedPropertiesWithoutUndo();

            Rewire(previous, controller.PreviousPage);
            Rewire(readAgain, controller.ReadAgain);
            Rewire(next, controller.NextPage);
            Rewire(back, controller.BackToLibrary);

            if (pageNumber != null) pageNumber.text = "PAGE 1 / 26";
            if (bookTitle != null) bookTitle.text = "ABC BOOK";
            if (pageHeading != null) pageHeading.text = "A is for Apple";
            if (pageBody != null) pageBody.text = "A is for apple. Apples can be red, green, or yellow.";
            if (journeySpeech != null) journeySpeech.text = "A is for Apple! Say A with me!";

            EnsureBuildSettings();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Learning with Journey",
                "Complete Book Reader V2 is connected. ABC has 26 pages, Numbers has 20 pages, Colors + Shapes has 12 pages, and Story Time has 10 pages. All read-aloud audio routes only through JourneyVoicePlayerV2; missing Journey clips stay silent instead of using device TTS.",
                "OK");
        }

        static void Rewire(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null) return;
            button.onClick = new Button.ButtonClickedEvent();
            UnityEventTools.AddPersistentListener(button.onClick, action);
            EditorUtility.SetDirty(button);
        }

        static void SetObject(SerializedObject so, string name, Object value)
        {
            SerializedProperty property = so.FindProperty(name);
            if (property != null) property.objectReferenceValue = value;
        }

        static void SetBool(SerializedObject so, string name, bool value)
        {
            SerializedProperty property = so.FindProperty(name);
            if (property != null) property.boolValue = value;
        }

        static T FindComponent<T>(string objectName) where T : Component
        {
            GameObject go = FindGameObject(objectName);
            return go != null ? go.GetComponent<T>() : null;
        }

        static GameObject FindGameObject(string objectName)
        {
            Scene scene = SceneManager.GetActiveScene();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform found = FindRecursive(root.transform, objectName);
                if (found != null) return found.gameObject;
            }
            return null;
        }

        static Transform FindRecursive(Transform current, string objectName)
        {
            if (current.name == objectName) return current;
            for (int i = 0; i < current.childCount; i++)
            {
                Transform found = FindRecursive(current.GetChild(i), objectName);
                if (found != null) return found;
            }
            return null;
        }

        static void EnsureBuildSettings()
        {
            var existing = EditorBuildSettings.scenes;
            foreach (var scene in existing)
                if (scene.path == ScenePath) return;

            var updated = new EditorBuildSettingsScene[existing.Length + 1];
            for (int i = 0; i < existing.Length; i++) updated[i] = existing[i];
            updated[existing.Length] = new EditorBuildSettingsScene(ScenePath, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}
#endif
