using UnityEngine;

namespace LearningWithJourney.UI
{
    /// <summary>
    /// Compatibility artwork class for Book Reader V2.
    ///
    /// BookReaderControllerV2 expects BookPageArtworkV2. The current project already
    /// has a stable vector renderer in BookPageArtworkV1, so V2 inherits that renderer
    /// to keep the project compiling while preserving the existing book artwork path.
    /// This can be expanded later with additional V2-specific illustrations without
    /// changing the controller API.
    /// </summary>
    public class BookPageArtworkV2 : BookPageArtworkV1
    {
    }
}
