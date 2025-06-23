
[System.Serializable]
public class ItemInstance
{
    public ItemSOData itemType;
    public int itemCount;

    public ItemInstance(ItemSOData itemType, int count = 0)
    {
        this.itemType = itemType;
        itemCount = count;
    }
}