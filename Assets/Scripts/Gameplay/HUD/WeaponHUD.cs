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

    [Header("----------Crosshair----------")]
    [SerializeField] RectTransform m_Crosshair;

    Camera m_Camera;


    protected override void OnEnable()
    {
        base.OnEnable();
        PlayerTank.OnPlayerReload += OnReload;
        PlayerTank.OnPlayerIsDoneReloading += OnReloadComplete;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        PlayerTank.OnPlayerReload -= OnReload;
        PlayerTank.OnPlayerIsDoneReloading -= OnReloadComplete;
    }

    private void Awake() => m_Camera = Camera.main;

    private void Update()
    {
        ProcessMouseInput();
    }

    private void ProcessMouseInput()
    {
        m_Crosshair.position = Input.mousePosition;

        if (m_Camera != null)
        {
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                m_Crosshair.gameObject.SetActive(hit.collider.name != PlayerTank.PlayerTankInstance.gameObject.name);
            }
        }
    }

    private void OnReloadComplete()
    {
        if (m_ChamberStatus && m_ChamberStatusImage)
        {
            //Play Sound
            if (SoundManager.Instance)
                SoundManager.Instance.PlayFESound("GunReady", 0.35f);
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