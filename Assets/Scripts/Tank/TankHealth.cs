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
    [SerializeField][Range(0, 100)] float m_Health = 100;
    [SerializeField][Range(0, 100)] float m_Armor = 0;
    [Header("---------Regeneration----------")]
    [SerializeField][Range(1, 10)] float m_RegenerateRate = 2; //How fast do we regenerate health.
    [SerializeField][Range(0, 100)] float m_RegenerateCap = 50; //How much of the health are we allow to regenerate?
    [SerializeField] bool m_DebugShowValues;
    BaseTank m_Tank;

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

    private void RegenerateHealth()
    {
        if (m_Health < m_RegenerateCap)
        {
            m_Health += 2 * m_RegenerateRate * Time.deltaTime;
        }
    }

    //Damage can only be positive...
    public void TakeDamage(float amount)
    {
        //Make sure damage is positive...
        amount = Mathf.Abs(amount);


        //if we have armor...
        if (m_Armor > 0)
        {
            //and the amount of damage we take would make the armor negative
            if (m_Armor - amount >= 0)
                m_Armor -= amount;

            //If the damage exceeds our protection...
            if (m_Armor - amount < 0)
            {
                float leftOverDamage = amount - m_Armor;

                //Destroy armour...
                m_Armor = 0;

                m_Health -= leftOverDamage;

                if (m_Health <= 0)
                {
                    m_Health = 0;
                }

                return;
            }
        }

        //Else we just have no armor and take that damage RAW
        m_Health -= amount;

        if (m_Health < 0)
            m_Health = 0;
    }

    private void Update()
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
            Handles.Label(transform.position + (transform.up * 10), info, style);
        }
#endif
    }
}