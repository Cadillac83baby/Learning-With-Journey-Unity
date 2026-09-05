using UnityEngine;

namespace LearningWithJourney.UI
{
    public class UIFloat : MonoBehaviour
    {
        [SerializeField] float amplitude = 10f;
        [SerializeField] float speed = 1.2f;
        [SerializeField] float phase;
        RectTransform rect;
        Vector2 startPosition;

        void Awake()
        {
            rect = transform as RectTransform;
            if (rect != null) startPosition = rect.anchoredPosition;
        }

        void Update()
        {
            if (rect == null) return;
            var y = Mathf.Sin((Time.unscaledTime * speed) + phase) * amplitude;
            rect.anchoredPosition = startPosition + new Vector2(0f, y);
        }
    }
}
