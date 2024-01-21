using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceTankHealthWidget : MonoBehaviour
{
    [SerializeField] Slider m_Health;
    [SerializeField] Slider m_Armor;
    [SerializeField] float m_Height = 5.0F;

    TankHealth m_ThisTank;

    public TankHealth TankHealthComponent { get { return m_ThisTank; } }

    Transform mainCameraTransform;

    private void Awake()
    {
        mainCameraTransform = Camera.main.transform;
        m_Health.maxValue = 100;
        m_Armor.maxValue = 100;
    }

    private void Update()
    {
        if (m_ThisTank)
        {
            m_Health.value = m_ThisTank.Health;
            m_Armor.value = m_ThisTank.Armor;
            transform.position = m_ThisTank.transform.position + Vector3.up * m_Height;

            //Billboard affect...
            transform.LookAt(mainCameraTransform);
        }
    }

    public void SetTank(TankHealth tankHealth)
    {
        m_ThisTank = tankHealth;
    }
}