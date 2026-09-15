using System;
using System.Collections.Generic;

namespace Game.Core.Registry
{
    /// <summary>
    /// Service Locator maison (ARCHITECTURE.md §2.3, §8 — pas de DI tierce type
    /// VContainer/Zenject pour le MVP). Point d'enregistrement unique résolu au
    /// bootstrap (GameBootstrap côté Presentation), consommé par tous les systèmes
    /// GameCore qui ont besoin d'un service transversal (IMiniGameFactory,
    /// IInventoryService, IEconomyService, GameEventBus...).
    /// </summary>
    public static class GameServices
    {
        private static readonly Dictionary<Type, object> Services = new();

        public static void Register<TService>(TService instance) where TService : class
        {
            if (instance is null) throw new ArgumentNullException(nameof(instance));
            Services[typeof(TService)] = instance;
        }

        public static TService Get<TService>() where TService : class
        {
            if (Services.TryGetValue(typeof(TService), out var service))
            {
                return (TService)service;
            }

            throw new InvalidOperationException(
                $"Service '{typeof(TService).Name}' n'est pas enregistré. " +
                "Vérifier que GameBootstrap l'a bien enregistré via GameServices.Register<T>() avant usage.");
        }

        public static bool TryGet<TService>(out TService? service) where TService : class
        {
            if (Services.TryGetValue(typeof(TService), out var raw))
            {
                service = (TService)raw;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Vide le registre. Utilitaire réservé aux tests (isole chaque test du service
        /// locator statique partagé) et au redémarrage complet du jeu (retour au Boot).
        /// </summary>
        public static void Reset() => Services.Clear();
    }
}
