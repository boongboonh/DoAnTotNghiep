using UnityEngine;

public class ListIconHealth : ListIcon
{
    private void Update()
    {
        HealthPlayer health = HealthPlayer.Instance;
        if (health != null)
            RefreshIcons(health.NowHeal, health.HealthPlayerMax);
    }
}
