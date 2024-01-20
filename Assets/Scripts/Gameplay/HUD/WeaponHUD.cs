using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponHUD : HUD
{
    [Header("----------General----------")]
    [Tooltip("This text lets the player know if the gun is ready")]
    [SerializeField] TMP_Text m_ChamberStatus;
    [Tooltip("This Image will change colour depending on the chamber status")]
    [SerializeField] Image m_ChamberStatusImage;
    [SerializeField] Color m_ReloadingColour = Color.yellow;
    [SerializeField] Color m_ReadyColor = Color.green;

    protected override void OnEnable()
    {
        base.OnEnable();
        PlayerTank.OnPlayerReload += OnReload;
        PlayerTank.OnPlayerIsDoneReloading += OnReloadComplete;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    private void OnReloadComplete()
    {
        //Play Sound?
        if (m_ChamberStatus && m_ChamberStatusImage)
        {
            m_ChamberStatus.text = "Ready";
            m_ChamberStatusImage.color = m_ReadyColor;
        }
    }

    private void OnReload()
    {
        if (m_ChamberStatus && m_ChamberStatusImage)
        {
            m_ChamberStatus.text = "Reloading";
            m_ChamberStatusImage.color = m_ReloadingColour;
        }
    }
}