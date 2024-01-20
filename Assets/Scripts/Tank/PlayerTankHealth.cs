using System;

public class PlayerTankHealth : TankHealth
{
    bool m_InitialNotification = false;

    new public static event Action OnDeath;

    public override void TakeDamage(float amount, BaseTank attacker)
    {
        base.TakeDamage(amount, attacker);
        NotifyHUDOfChange();

        if (IsDead)
        {
            OnDeath?.Invoke();
        }
    }

    public override void AddArmor(float amount)
    {
        base.AddArmor(amount);
        NotifyHUDOfChange();
    }

    public override void AddHealth(float amount)
    {
        base.AddHealth(amount);
        NotifyHUDOfChange();
    }

    protected override void RegenerateHealth()
    {
        base.RegenerateHealth();
        NotifyHUDOfChange();
    }

    protected override void Update()
    {
        base.Update();

        if (!m_InitialNotification)
        {
            NotifyHUDOfChange();
            m_InitialNotification = true;
        }
    }

    private void NotifyHUDOfChange()
    {
        //Notify the HUD of the change
        if (MiniMapHUD.Instance != null)
        {
            MiniMapHUD.Instance.UpdatePlayerStats(
                MiniMapHUD.PlayerStatUpdate.PLAYER_ARMOR,
                m_Armor
                );

            MiniMapHUD.Instance.UpdatePlayerStats(
                MiniMapHUD.PlayerStatUpdate.PLAYER_HEALTH,
                m_Health
                );
        }
    }
}