#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LearningWithJourney.EditorTools
{
    public static class LWJReaderVisualCleanupV7
    {
        [MenuItem("Learning with Journey/Clean Up Book Reader V7")]
        public static void Apply()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                scene.path != "Assets/LearningWithJourney/Scenes/BookReader.unity")
            {
                EditorUtility.DisplayDialog("Learning with Journey",
                    "Stop Play mode and open BookReader first. No changes were made.", "OK");
                return;
            }
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Clean up Book Reader");
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Button button in root.GetComponentsInChildren<Button>(true))
                {
                    if (button.name != "PreviousPage" && button.name != "ReadAgain" && button.name != "NextPage")
                        continue;
                    RectTransform rect = button.transform as RectTransform;
                    if (rect == null || rect.parent == null) continue;
                    Transform sibling = rect.parent.Find(button.name + "Shadow");
                    if (sibling == null) continue;
                    Image oldShadow = sibling.GetComponent<Image>();
                    if (oldShadow == null) continue;

                    bool hasRaisedShadow = false;
                    foreach (Shadow effect in button.GetComponents<Shadow>())
                        if (!(effect is Outline) && effect.enabled) hasRaisedShadow = true;

                    Undo.RecordObject(oldShadow, "Repair button shadow");
                    if (hasRaisedShadow)
                    {
                        // The shared menu theme already renders a shadow on the button.
                        // Keep the obsolete sibling intact but stop its duplicate rendering.
                        oldShadow.enabled = false;
                    }
                    else
                    {
                        RectTransform shadowRect = oldShadow.rectTransform;
                        Undo.RecordObject(shadowRect, "Align button shadow");
                        shadowRect.anchorMin = rect.anchorMin + new Vector2(0f, -.006f);
                        shadowRect.anchorMax = rect.anchorMax + new Vector2(0f, -.006f);
                        shadowRect.pivot = rect.pivot;
                        shadowRect.offsetMin = rect.offsetMin;
                        shadowRect.offsetMax = rect.offsetMax;
                        shadowRect.localScale = rect.localScale;
                        shadowRect.localRotation = rect.localRotation;
                        oldShadow.enabled = true;
                        PrefabUtility.RecordPrefabInstancePropertyModifications(shadowRect);
                    }
                    PrefabUtility.RecordPrefabInstancePropertyModifications(oldShadow);
                }
                foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (text.name != "SparkleTL" && text.name != "SparkleBR") continue;
                    Undo.RecordObject(text, "Use supported decorative sparkle");
                    text.text = "*";
                    PrefabUtility.RecordPrefabInstancePropertyModifications(text);
                }
            }
            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorUtility.DisplayDialog("Learning with Journey",
                "Detached button shadows repaired and the two decorative sparkles changed to a supported symbol. Ctrl+S to save. Journey, the bookbag, and button positions are unchanged.", "OK");
        }
    }
}
#endif
