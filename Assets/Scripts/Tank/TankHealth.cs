using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// This component keeps track of health and handles damage
/// </summary>
public class TankHealth : MonoBehaviour
{
    [Header("---------General----------")]
    [SerializeField][Range(0, 100)] protected float m_Health = 100;
    [SerializeField][Range(0, 100)] protected float m_Armor = 0;
    [Header("---------Regeneration----------")]
    [SerializeField][Range(0, 10)] float m_RegenerateRate = 2; //How fast do we regenerate health.
    [SerializeField][Range(0, 100)] float m_RegenerateCap = 50; //How much of the health are we allow to regenerate?
    [SerializeField] bool m_DebugShowValues;

    public static event Action<BaseTank> OnDeath;

    public BaseTank m_Attacker { get; private set; } //This is the tank the shot this tank...
    private BaseTank m_Tank;

    public float Health { get { return m_Health; } }

    public float Armor { get { return m_Armor; } }

    public bool HasArmor { get { return m_Armor > 0; } }

    public bool IsDead
    {
        get
        {
            return m_Health <= 0;
        }
    }

    private void Awake()
    {
        if (TryGetComponent<BaseTank>(out var tank))
            m_Tank = tank;
        else
            Debug.LogError("There is no tank object attached to this transform!");
    }

    protected virtual void RegenerateHealth()
    {
        if (m_Health < m_RegenerateCap)
        {
            m_Health += 2 * m_RegenerateRate * Time.deltaTime;
        }
    }

    public virtual void AddHealth(float amount)
    {
        m_Health += amount;

        //Cap health to 100
        if (m_Health > 100)
        {
            m_Health = 100;
        }
    }

    public virtual void AddArmor(float amount)
    {
        m_Armor += amount;

        //Cap health to 100
        if (m_Armor > 100)
        {
            m_Armor = 100;
        }
    }

    //Damage can only be positive...
    public virtual void TakeDamage(float amount, BaseTank attacker)
    {
        m_Attacker = attacker;

        //Make sure damage is positive...
        amount = Mathf.Abs(amount);

        //if we have armor...
        if (m_Armor > 0)
        {
            //Play Shield Damage Sound
            if (SoundManager.Instance)
            {
                SoundManager.Instance.PlayInGameSound("TankFX_HitShield", transform.position, true, 70.0F);
            }
            else
                Debug.LogError("There is no Sound Manager in the scene!");

            m_Armor -= amount;


            if (m_Armor > 0)
                return;
        }

        //Else we just have no armor and take that damage RAW
        m_Health -= amount;

        if (m_Health < 0)
        {
            m_Health = 0;

            OnDeath?.Invoke(m_Attacker);
        }
    }

    protected virtual void Update()
    {
        if (!m_Tank)
            return;

        if (IsDead)
            return;

        //Only regenerate when the tank is not firing
        if (m_Health <= m_RegenerateCap && !m_Tank.IsFiring)
            RegenerateHealth();
    }

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (m_DebugShowValues)
        {
            string info =
                "Health : " + m_Health +
                "\nArmor : " + m_Armor;

            GUIStyle style = new()
            {
                fontSize = 12
            };

            style.normal.textColor = Color.white;
            Handles.Label(transform.position + (transform.up * 5), info, style);
        }
#endif
    }
}