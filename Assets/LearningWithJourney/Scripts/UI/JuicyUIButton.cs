using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Small tactile scale response for mobile-style menu controls.
    /// Keeps the existing Button click wiring intact while making controls feel less flat.
    /// </summary>
    public class JuicyUIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] float pressedScale = 0.965f;
        [SerializeField] float hoverScale = 1.015f;
        [SerializeField] float responseSeconds = 0.08f;

        RectTransform rect;
        Vector3 baseScale;
        Coroutine scaleRoutine;
        bool pointerInside;

        void Awake()
        {
            rect = transform as RectTransform;
            baseScale = rect != null ? rect.localScale : transform.localScale;
        }

        void OnEnable()
        {
            if (rect == null) rect = transform as RectTransform;
            baseScale = rect != null ? rect.localScale : transform.localScale;
            SetScale(baseScale);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            AnimateTo(baseScale * pressedScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            AnimateTo(baseScale * (pointerInside ? hoverScale : 1f));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            pointerInside = true;
            if (!eventData.dragging)
                AnimateTo(baseScale * hoverScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerInside = false;
            AnimateTo(baseScale);
        }

        void AnimateTo(Vector3 target)
        {
            if (!isActiveAndEnabled)
            {
                SetScale(target);
                return;
            }

            if (scaleRoutine != null) StopCoroutine(scaleRoutine);
            scaleRoutine = StartCoroutine(ScaleRoutine(target));
        }

        IEnumerator ScaleRoutine(Vector3 target)
        {
            Vector3 start = rect != null ? rect.localScale : transform.localScale;
            float elapsed = 0f;
            float duration = Mathf.Max(0.02f, responseSeconds);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = 1f - Mathf.Pow(1f - t, 3f);
                SetScale(Vector3.LerpUnclamped(start, target, t));
                yield return null;
            }

            SetScale(target);
            scaleRoutine = null;
        }

        void SetScale(Vector3 value)
        {
            if (rect != null) rect.localScale = value;
            else transform.localScale = value;
        }
    }
}
