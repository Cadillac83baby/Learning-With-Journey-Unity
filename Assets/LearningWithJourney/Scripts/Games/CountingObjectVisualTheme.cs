using UnityEngine;

namespace LearningWithJourney.Games
{
    public class CountingObjectVisualTheme : MonoBehaviour
    {
        [SerializeField] GameObject[] themeRoots;
        [SerializeField] string[] singularNames;
        [SerializeField] string[] pluralNames;

        public int ThemeCount => themeRoots != null ? themeRoots.Length : 0;

        public string ApplyTheme(int index)
        {
            if (themeRoots == null || themeRoots.Length == 0)
                return "objects";

            int safeIndex = Mathf.Abs(index) % themeRoots.Length;
            for (int i = 0; i < themeRoots.Length; i++)
            {
                if (themeRoots[i] != null)
                    themeRoots[i].SetActive(i == safeIndex);
            }

            if (pluralNames != null && safeIndex < pluralNames.Length && !string.IsNullOrWhiteSpace(pluralNames[safeIndex]))
                return pluralNames[safeIndex];

            return "objects";
        }

        public string GetSingularName(int index)
        {
            if (themeRoots == null || themeRoots.Length == 0)
                return "object";

            int safeIndex = Mathf.Abs(index) % themeRoots.Length;
            if (singularNames != null && safeIndex < singularNames.Length && !string.IsNullOrWhiteSpace(singularNames[safeIndex]))
                return singularNames[safeIndex];

            return "object";
        }
    }
}
