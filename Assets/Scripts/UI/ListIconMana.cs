using UnityEngine;

public class ListIconMana : ListIcon
{
    private void Update()
    {
        ManaManager mana = ManaManager.Instance;
        if (mana != null)
            RefreshIcons(mana.NowMana, mana.ManaPlayerMax);
    }
}
