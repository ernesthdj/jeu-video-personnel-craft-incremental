using System.Threading.Tasks;

namespace Game.Core.Save
{
    /// <summary>
    /// ARCHITECTURE.md §7. Asynchrone — ne bloque jamais le thread principal (règle
    /// globale : async/await sur toute I/O, jamais .Result/.Wait()).
    /// </summary>
    public interface ISaveService
    {
        Task SaveAsync(string slotId, GameSaveData data);
        Task<GameSaveData?> LoadAsync(string slotId);
        bool SaveExists(string slotId);
    }
}
