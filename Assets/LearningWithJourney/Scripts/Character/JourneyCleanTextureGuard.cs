using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.Character
{
    /// <summary>
    /// Keeps the repaired single Journey texture on the Main Menu while the temporary
    /// sprite-atlas controller still provides transform-based motion and voice timing.
    /// This prevents damaged atlas alpha around Journey's shorts/body from reappearing.
    /// </summary>
    [DisallowMultipleComponent]
    public class JourneyCleanTextureGuard : MonoBehaviour
    {
        [SerializeField] RawImage characterImage;
        [SerializeField] Texture2D cleanTexture;

        void Awake() => Apply();
        void OnEnable() => Apply();
        void LateUpdate() => Apply();

        public void Configure(RawImage image, Texture2D texture)
        {
            characterImage = image;
            cleanTexture = texture;
            Apply();
        }

        void Apply()
        {
            if (characterImage == null || cleanTexture == null) return;

            characterImage.texture = cleanTexture;
            characterImage.uvRect = new Rect(0f, 0f, 1f, 1f);
            characterImage.color = Color.white;
            characterImage.canvasRenderer.SetAlpha(1f);
            characterImage.material = null;
        }
    }
}
