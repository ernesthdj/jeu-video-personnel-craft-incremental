// Polyfill nécessaire pour utiliser les accesseurs `init` (C# 9) en ciblant netstandard2.1
// (le type n'existe pas dans cette TFM, le compilateur le cherche par convention de nom).
// Purement un détail de compilation .NET — aucun rapport avec UnityEngine, GameCore reste
// un assembly C# pur.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}
