using Game.Core.Combat;
using UnityEngine;

namespace Game.Content.Definitions
{
    /// <summary>LIMITATION CONNUE : dépend de UnityEngine, non compilable hors éditeur (voir docs/IMPLEMENTATION.md).</summary>
    [CreateAssetMenu(fileName = "CombatantDefinition", menuName = "CraftIncremental/Combat/Combattant", order = 0)]
    public sealed class CombatantDefinitionSO : ScriptableObject
    {
        [SerializeField] private string _id = string.Empty;
        [SerializeField] private string _displayName = string.Empty;
        [SerializeField, Min(1)] private int _maxHitPoints = 10;
        [SerializeField, Min(0)] private int _maxActionPoints = 2;
        [SerializeField, Min(0)] private int _maxMovementPoints = 3;
        [SerializeField, Min(0)] private int _baseDamage = 5;

        [Tooltip("Vide pour un combattant joueur. Id résolu via le registry d'IEnemyAI (ex: 'aggressive').")]
        [SerializeField] private string _enemyAiId = string.Empty;

        public CombatantDefinition ToDefinition() => new()
        {
            Id = _id,
            DisplayName = _displayName,
            MaxHitPoints = _maxHitPoints,
            MaxActionPoints = _maxActionPoints,
            MaxMovementPoints = _maxMovementPoints,
            BaseDamage = _baseDamage,
            EnemyAiId = string.IsNullOrEmpty(_enemyAiId) ? null : _enemyAiId,
        };
    }
}
