using Game.Core.Items;

namespace Game.Core.MiniGames
{
    /// <summary>
    /// Résultat final d'un mini-jeu (retourné par <see cref="IMiniGame.Evaluate"/>).
    /// Le score est la seule source de vérité pour le résultat — pas de jet de dés caché
    /// derrière (GDD Pillar 1 : "l'habileté prime sur le hasard").
    /// </summary>
    public readonly struct MiniGameResult
    {
        /// <summary>Score continu 0..1. 0 = échec total, 1 = perfection.</summary>
        public float Score01 { get; }

        public bool Success { get; }
        public ItemQuality Quality { get; }

        public MiniGameResult(float score01, bool success, ItemQuality quality)
        {
            Score01 = score01;
            Success = success;
            Quality = quality;
        }
    }
}
