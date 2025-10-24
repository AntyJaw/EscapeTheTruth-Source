namespace EtT.Inventory
{
    [System.Serializable]
    public struct ItemStack
    {
        public string itemId;
        public int quantity;

        public ItemStack(string id, int qty) { itemId = id; quantity = qty; }
    }
}