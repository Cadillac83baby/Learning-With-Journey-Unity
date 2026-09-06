using UnityEngine;
using UnityEngine.UI;

namespace LearningWithJourney.Games
{
    public class ABCWordPictureVisual : MonoBehaviour
    {
        [SerializeField] Image pictureImage;
        [SerializeField] Sprite[] pictures;

        public int PictureCount => pictures != null ? pictures.Length : 0;

        public void Show(int index)
        {
            if (pictureImage == null) return;

            if (pictures == null || index < 0 || index >= pictures.Length || pictures[index] == null)
            {
                pictureImage.enabled = false;
                pictureImage.sprite = null;
                return;
            }

            pictureImage.enabled = true;
            pictureImage.sprite = pictures[index];
            pictureImage.preserveAspect = true;
            pictureImage.color = Color.white;
        }
    }
}
