using System;

public class PlayerTankHealth : TankHealth
{
    bool m_InitialNotification = false;

    new public static event Action OnDeath;

    public static event Action OnTakeDamage;

    public static event Action<float, float> OnHealthChange;

    public override void TakeDamage(float amount, BaseTank attacker)
    {
        base.TakeDamage(amount, attacker);

        OnHealthChange?.Invoke(m_Health, m_Armor);

        OnTakeDamage?.Invoke();

        if (IsDead)
            OnDeath?.Invoke();
    }

    public override void AddArmor(float amount)
    {
        base.AddArmor(amount);

        OnHealthChange?.Invoke(m_Health, m_Armor);
    }

    public override void AddHealth(float amount)
    {
        base.AddHealth(amount);

        OnHealthChange?.Invoke(m_Health, m_Armor);

    }

    protected override void RegenerateHealth()
    {
        base.RegenerateHealth();

        OnHealthChange?.Invoke(m_Health, m_Armor);
    }

    protected override void Update()
    {
        base.Update();

        if (!m_InitialNotification)
        {
            OnHealthChange?.Invoke(m_Health, m_Armor);

            m_InitialNotification = true;
        }
    }
}