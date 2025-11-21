[System.Serializable]
public class Item
{
    public string name;
    public int count;
    public int healthBonus;
    public int staminaBonus;

    public Item(string name, int count, int healthBonus, int staminaBonus)
    {
        this.name = name;
        this.count = count;
        this.healthBonus = healthBonus;
        this.staminaBonus = staminaBonus;
    }
}
