[System.Serializable]
public class ItemInstance
{
    public ItemSOData itemData;
    public int count;

    public ItemInstance(ItemSOData itemData, int count = 1)
    {
        this.itemData = itemData;
        this.count = count;
    }
}