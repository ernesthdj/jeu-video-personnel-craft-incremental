using System;

namespace Game.Core.Common
{
    /// <summary>
    /// Position sur la grille de combat. Type maison (pas UnityEngine.Vector2Int) pour que
    /// GameCore reste un assembly C# pur, compilable et testable en dehors de Unity
    /// (voir ARCHITECTURE.md §1 : "aucune référence à UnityEngine sauf types structurels
    /// minimaux" — ici on va plus loin et on élimine même cette exception pragmatique,
    /// ce qui simplifie la vérification de compilation .NET de cette phase).
    /// </summary>
    public readonly struct GridPosition : IEquatable<GridPosition>
    {
        public int X { get; }
        public int Z { get; }

        public GridPosition(int x, int z)
        {
            X = x;
            Z = z;
        }

        // Grille simple sans obstacles (décision World Builder, ZONE-DESIGN.md §0) :
        // Chebyshev = distance de déplacement (diagonales autorisées, coût 1),
        // Manhattan = distance de portée en ligne/colonne (utile pour certains effets).
        public int ChebyshevDistanceTo(GridPosition other) =>
            Math.Max(Math.Abs(X - other.X), Math.Abs(Z - other.Z));

        public int ManhattanDistanceTo(GridPosition other) =>
            Math.Abs(X - other.X) + Math.Abs(Z - other.Z);

        public bool Equals(GridPosition other) => X == other.X && Z == other.Z;
        public override bool Equals(object obj) => obj is GridPosition other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Z);
        public override string ToString() => $"({X}, {Z})";

        public static bool operator ==(GridPosition left, GridPosition right) => left.Equals(right);
        public static bool operator !=(GridPosition left, GridPosition right) => !left.Equals(right);
    }
}
