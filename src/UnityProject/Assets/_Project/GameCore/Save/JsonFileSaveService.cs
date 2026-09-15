using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Game.Core.Save
{
    /// <summary>
    /// TECH-STACK.md §5 : Newtonsoft.Json plutôt que JsonUtility natif (polymorphisme +
    /// dictionnaires nécessaires pour GameSaveData). Sauvegarde locale uniquement pour le
    /// MVP (ARCHITECTURE.md §7).
    ///
    /// Écart assumé par rapport à ARCHITECTURE.md §3/§7 : le chemin de sauvegarde
    /// (normalement <c>Application.persistentDataPath</c>) est injecté au constructeur
    /// plutôt que lu directement depuis UnityEngine — c'est ce qui permet à ce fichier de
    /// vivre dans GameCore (zéro dépendance Unity, compilable et testable en .NET pur)
    /// tout en restant fidèle à l'intention de l'architecture. Côté Presentation,
    /// GameBootstrap construit ce service avec <c>Application.persistentDataPath</c>.
    /// </summary>
    public sealed class JsonFileSaveService : ISaveService
    {
        private readonly string _rootDirectory;

        private static readonly JsonSerializerSettings SerializerSettings = new()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        public JsonFileSaveService(string rootDirectory)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory))
            {
                throw new ArgumentException("Le répertoire racine de sauvegarde ne peut pas être vide.", nameof(rootDirectory));
            }

            _rootDirectory = rootDirectory;
            Directory.CreateDirectory(_rootDirectory);
        }

        public async Task SaveAsync(string slotId, GameSaveData data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));

            data.LastSavedAtUtc = DateTimeOffset.UtcNow;
            var json = JsonConvert.SerializeObject(data, SerializerSettings);
            await File.WriteAllTextAsync(PathFor(slotId), json).ConfigureAwait(false);
        }

        public async Task<GameSaveData?> LoadAsync(string slotId)
        {
            var path = PathFor(slotId);
            if (!File.Exists(path))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<GameSaveData>(json, SerializerSettings);
        }

        public bool SaveExists(string slotId) => File.Exists(PathFor(slotId));

        private string PathFor(string slotId) => Path.Combine(_rootDirectory, $"{slotId}.save.json");
    }
}
