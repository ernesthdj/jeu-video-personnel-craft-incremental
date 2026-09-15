using Game.Core.Items;

namespace Game.Core.Save
{
    public sealed class ItemInstanceSaveData
    {
        public string InstanceId { get; set; } = string.Empty;
        public string ItemDefinitionId { get; set; } = string.Empty;
        public ItemQuality Quality { get; set; }
    }
}
