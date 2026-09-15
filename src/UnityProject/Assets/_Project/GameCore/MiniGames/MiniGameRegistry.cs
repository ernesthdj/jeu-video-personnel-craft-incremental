using System;
using System.Collections.Generic;

namespace Game.Core.MiniGames
{
    /// <summary>
    /// Registry (ARCHITECTURE.md §2.3) : association id -> constructeur, remplie au
    /// bootstrap (GameBootstrap, seul point qui connaît toutes les implémentations
    /// concrètes). Ajouter un mini-jeu = une classe + une ligne d'enregistrement, zéro
    /// modification des systèmes qui consomment <see cref="IMiniGameFactory"/> —
    /// délibérément pas de switch géant.
    /// </summary>
    public sealed class MiniGameRegistry : IMiniGameFactory
    {
        private readonly Dictionary<string, Func<IMiniGame>> _constructors = new();

        public void RegisterType(string miniGameTypeId, Func<IMiniGame> constructor)
        {
            if (string.IsNullOrWhiteSpace(miniGameTypeId))
            {
                throw new ArgumentException("L'id de type de mini-jeu ne peut pas être vide.", nameof(miniGameTypeId));
            }

            _constructors[miniGameTypeId] = constructor ?? throw new ArgumentNullException(nameof(constructor));
        }

        public IMiniGame Create(string miniGameTypeId)
        {
            if (_constructors.TryGetValue(miniGameTypeId, out var constructor))
            {
                return constructor();
            }

            throw new KeyNotFoundException(
                $"Aucun mini-jeu enregistré pour l'id '{miniGameTypeId}'. " +
                "Vérifier l'enregistrement dans GameBootstrap ou l'id référencé par le MiniGameConfigSO.");
        }

        public bool IsRegistered(string miniGameTypeId) => _constructors.ContainsKey(miniGameTypeId);
    }
}
