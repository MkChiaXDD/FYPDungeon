
[System.Serializable]
public class ItemInstance
{
    public ItemSOData itemType;
    public int itemCount = 1;

    public ItemInstance(ItemSOData itemData)
    {
        itemType = itemData;
    }
}