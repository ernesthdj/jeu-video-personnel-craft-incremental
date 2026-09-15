using System;
using System.IO;
using System.Threading.Tasks;
using Game.Core.Items;
using Game.Core.Save;
using Xunit;

namespace Game.Core.Tests.Save
{
    /// <summary>
    /// VERTICAL-SLICE.md étape 5 : "le cycle complet survit à une fermeture/réouverture
    /// de l'app". Utilise un répertoire temporaire unique par test (isolation), simulant
    /// ce que GameBootstrap ferait avec Application.persistentDataPath côté Unity.
    /// </summary>
    public class JsonFileSaveServiceTests : IDisposable
    {
        private readonly string _tempDirectory;

        public JsonFileSaveServiceTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "gamecore_save_tests_" + Guid.NewGuid().ToString("N"));
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }

        [Fact]
        public async Task should_round_trip_inventory_state_through_save_and_load()
        {
            var service = new JsonFileSaveService(_tempDirectory);
            var data = new GameSaveData
            {
                PlayerId = "player_1",
                InventoryQuantities = { ["fer_brut"] = 5, ["bois"] = 1 },
                InventoryInstances =
                {
                    new ItemInstanceSaveData { InstanceId = "abc123", ItemDefinitionId = "epee_courte", Quality = ItemQuality.Rare },
                },
            };

            await service.SaveAsync("slot_0", data);
            var reloaded = await service.LoadAsync("slot_0");

            Assert.NotNull(reloaded);
            Assert.Equal("player_1", reloaded!.PlayerId);
            Assert.Equal(5, reloaded.InventoryQuantities["fer_brut"]);
            Assert.Equal(1, reloaded.InventoryQuantities["bois"]);
            Assert.Single(reloaded.InventoryInstances);
            Assert.Equal(ItemQuality.Rare, reloaded.InventoryInstances[0].Quality);
        }

        [Fact]
        public async Task should_return_null_when_loading_a_slot_that_was_never_saved()
        {
            var service = new JsonFileSaveService(_tempDirectory);

            var reloaded = await service.LoadAsync("never_saved");

            Assert.Null(reloaded);
            Assert.False(service.SaveExists("never_saved"));
        }

        [Fact]
        public async Task should_report_save_exists_after_saving()
        {
            var service = new JsonFileSaveService(_tempDirectory);
            await service.SaveAsync("slot_0", new GameSaveData { PlayerId = "p1" });

            Assert.True(service.SaveExists("slot_0"));
        }
    }
}
