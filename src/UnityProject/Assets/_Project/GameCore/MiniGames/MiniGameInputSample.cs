namespace Game.Core.MiniGames
{
    /// <summary>
    /// Un échantillon d'input brut transmis par la couche Presentation à chaque frame
    /// pendant qu'un mini-jeu est actif (INPUT-MAP.md §5). Aucune interprétation de
    /// résultat n'est faite côté Presentation — uniquement de la capture/forwarding.
    /// Ce flux est volontairement séparé du GameEventBus (voir alerte Technical Director /
    /// UI-UX : le flux continu d'input doit rester local, pas passer par le bus
    /// d'événements réservé aux événements de fin d'action).
    /// </summary>
    public readonly struct MiniGameInputSample
    {
        public double TimestampSeconds { get; }

        /// <summary>Position normalisée (0..1) sur l'axe horizontal de la zone de jeu.</summary>
        public float NormalizedX { get; }

        /// <summary>Position normalisée (0..1) sur l'axe vertical de la zone de jeu.</summary>
        public float NormalizedY { get; }

        /// <summary>Vrai pour l'échantillon qui correspond à l'action principale (ex. tap).</summary>
        public bool IsPrimaryActionTriggered { get; }

        public MiniGameInputSample(double timestampSeconds, float normalizedX, float normalizedY,
            bool isPrimaryActionTriggered)
        {
            TimestampSeconds = timestampSeconds;
            NormalizedX = normalizedX;
            NormalizedY = normalizedY;
            IsPrimaryActionTriggered = isPrimaryActionTriggered;
        }
    }
}
