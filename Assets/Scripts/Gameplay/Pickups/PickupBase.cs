using UnityEngine;
using System;

[RequireComponent(typeof(SphereCollider))]
public class PickupBase : MonoBehaviour
{
    private SphereCollider m_SphereCollider;

    [Header("----------Decorative----------")]
    [Tooltip("How fast do you want this to spin?")]
    [SerializeField] protected float m_SpinRate = 100;
    [SerializeField] protected float m_HeightOscillationRate = 5;
    [SerializeField] protected float m_OscillationSpeed = 5;

    public static event Action<PickupBase> OnPickUp;

    protected virtual void Awake()
    {
        if (!TryGetComponent(out m_SphereCollider))
        {
            Debug.LogError("This Pickup does not have a sphere collider attached!");
        }

        Initialise();
    }

    public virtual void Initialise() { }

    //Spinny Pickup!
    protected void DoPickUpBobAndHover()
    {
        if (GameManager.Instance && GameManager.Instance.GameIsPaused)
            return;

        transform.eulerAngles += m_SpinRate * Time.deltaTime * Vector3.up;

        transform.position += m_HeightOscillationRate * Time.deltaTime * Mathf.Sin(Time.time * m_OscillationSpeed) * Vector3.up;
    }

    protected virtual void Update()
    {
        DoPickUpBobAndHover();
    }

    protected virtual void OnValidate()
    {
        if (TryGetComponent(out m_SphereCollider))
        {
            if (!m_SphereCollider.isTrigger)
                m_SphereCollider.isTrigger = true;
        }
    }

    protected virtual void DoPlayerPickUp()
    {
        OnPickUp?.Invoke(this);
        PlayerPickupSound();
    }

    protected virtual void PlayerPickupSound()
    {
        if (SoundManager.Instance)
        {
            SoundManager.Instance.PlayFESound("Pickup");
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerTank>(out var tank))
        {
            if (PlayerTank.PlayerTankInstance != null && tank == PlayerTank.PlayerTankInstance)
            {
                DoPlayerPickUp();

                gameObject.SetActive(false);
            }
        }
    }
}