using UnityEngine;

public class HealthPickup : PickupBase
{
    [Header("----------General-----------")]
    [Tooltip("What random amount of health should this give, what is the range?")]
    [SerializeField] Vector2 m_HealthAmountRange = new(10, 50);

    private float m_Health;

    public override void Initialise()
    {
        base.Initialise();
        m_Health = Mathf.Ceil(Random.Range(m_HealthAmountRange.x, m_HealthAmountRange.y));
        transform.position = new()
        {
            x = transform.position.x,
            y = 2.5F,
            z = transform.position.z
        };
    }

    protected override void PlayerPickupSound()
    {
        //Play health pickup sound....

        //Pick up particle?

    }

    protected override void DoPlayerPickUp()
    {
        base.DoPlayerPickUp();

        if (PlayerTank.PlayerTankInstance !=null)
        {
            if (PlayerTank.PlayerTankInstance.TryGetComponent<TankHealth>(out var health))
            {
                health.AddHealth(m_Health);
            }
        }
    }
}
