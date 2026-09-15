using Game.Core.Items;

namespace Game.Core.MiniGames
{
    /// <summary>
    /// Un palier score -> qualité. Le score du mini-jeu (0..1) est comparé aux seuils
    /// (triés décroissants) pour déterminer le palier de qualité obtenu (SYSTEMS.md §1 :
    /// "commun -> rare -> parfait").
    /// </summary>
    public readonly struct QualityThreshold
    {
        public float MinScore01 { get; }
        public ItemQuality Quality { get; }

        public QualityThreshold(float minScore01, ItemQuality quality)
        {
            MinScore01 = minScore01;
            Quality = quality;
        }
    }
}
