using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapHUD : HUD
{
    [Header("----------General----------")]
    [SerializeField] GameObject m_MiniMapCameraPrefab;
    [SerializeField] GameObject m_EnemyIconPrefab;
    [SerializeField] GameObject m_PlayerIconPrefab;

    [Header("Player Stats")]
    [SerializeField] Slider m_PlayerHealthSlider;
    [SerializeField] Slider m_PlayerArmorSlider;

    [Header("----------Minimap Settings----------")]
    [Tooltip("Minimum Zoom")]
    [SerializeField][Min(1)] float m_MiniMapMinScale = 1;
    [Tooltip("Maximum Zoom")]
    [SerializeField][Min(1)] float m_MiniMapMaxScale = 2;
    [Tooltip("The speed at which the mini map be fully zoomed out")]
    [SerializeField][Min(1)] float m_SpeedAtMaxMiniMapScale;
    [SerializeField][Min(1)] float m_MiniMapHeight = 10.0F;
    [SerializeField] bool m_FlipForwardDirection = false;

    Camera m_MiniMapCamera;
    List<GameObject> m_TankIcons = new();

    public static MiniMapHUD Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Instance = null;
        Debug.Log("Destroyed MiniMapHUD Instance!");
    }

    private void Start()
    {
        if (!PlayerTank.PlayerTankInstance)
        {
            Debug.LogError("There is no Player tank in the scene!");
            enabled = false;
            return;
        }

        if (m_PlayerArmorSlider != null)
            m_PlayerArmorSlider.maxValue = 100;
        else
            Debug.LogError("There is no player armor slider assigned!");

        if (m_PlayerHealthSlider != null)
            m_PlayerHealthSlider.maxValue = 100;
        else
            Debug.LogError("There is no player health slider assigned!");

        FindPlayerAndAttachMiniMap();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        BaseTank.OnSpawn += OnTankSpawn;
        PlayerTankHealth.OnHealthChange += OnPlayerHealthChange;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        BaseTank.OnSpawn -= OnTankSpawn;
        PlayerTankHealth.OnHealthChange -= OnPlayerHealthChange;
    }

    private void OnPlayerHealthChange(float health, float armor)
    {
        m_PlayerHealthSlider.value = health;
        m_PlayerArmorSlider.value = armor;
    }

    private void OnTankSpawn(BaseTank spawnedTank)
    {
        switch (spawnedTank)
        {
            case AITank:
                CreateIconAndAssignToTank(m_EnemyIconPrefab, spawnedTank.transform);
                break;

            case PlayerTank:
                CreateIconAndAssignToTank(m_PlayerIconPrefab, spawnedTank.transform);
                break;
        }
    }

    private void LateUpdate()
    {
        TrackPlayerWithMiniMapCamera();
    }

    private void CreateIconAndAssignToTank(GameObject IconPrefab, Transform parent)
    {
        //Instantiate an tank Icon...
        GameObject GOIcon = Instantiate
            (
                IconPrefab,
                parent.position + Vector3.up,
                Quaternion.identity,
                parent //parent it to the tank...
            );

        //Position the sprite correctly...assuming there is one on the prefab...

        //Flip it over so that it faces direction up to the sky...
        GOIcon.transform.eulerAngles = Vector3.right * 90;

        //Make sure its active...
        GOIcon.SetActive(true);

        //Add it to the list.
        m_TankIcons.Add(GOIcon);
    }

    private void FindPlayerAndAttachMiniMap()
    {
        m_MiniMapCamera = Instantiate(m_MiniMapCameraPrefab, transform.position, Quaternion.identity).GetComponent<Camera>();
        TrackPlayerWithMiniMapCamera();
    }

    private void TrackPlayerWithMiniMapCamera()
    {
        if (PlayerTank.PlayerTankInstance != null)
        {
            float angleY = 0;

            if (ThirdPersonTankCamera.Instance != null)
            {
                angleY = ThirdPersonTankCamera.Instance.transform.eulerAngles.y;

                angleY = m_FlipForwardDirection ? -angleY : angleY;
            }

            m_MiniMapCamera.transform.position = PlayerTank.PlayerTankInstance.transform.position + Vector3.up * m_MiniMapHeight;
            m_MiniMapCamera.transform.eulerAngles = Vector3.right * 90 + Vector3.forward * angleY;
            m_MiniMapCamera.orthographicSize = Mathf.Lerp(m_MiniMapMinScale, m_MiniMapMaxScale, PlayerTank.PlayerTankInstance.Speed / m_SpeedAtMaxMiniMapScale);
        }
    }

    public enum PlayerStatUpdate
    {
        PLAYER_HEALTH,
        PLAYER_ARMOR
    }
}
