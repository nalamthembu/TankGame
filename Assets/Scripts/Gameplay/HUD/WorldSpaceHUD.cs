using System.Collections.Generic;
using UnityEngine;

public class WorldSpaceHUD : MonoBehaviour
{
    [SerializeField] GameObject m_TankHealthWidgetPrefab;

    List<WorldSpaceTankHealthWidget> m_WrldSpcHealthWidgets = new();

    private void OnEnable()
    {
        AITank.OnSpawn += OnAITankSpawn;
        TankHealth.OnRemoveHealthWidget += OnTankRemoveHealthWidget;
    }

    private void OnTankRemoveHealthWidget(TankHealth healthComponent)
    {
        foreach (WorldSpaceTankHealthWidget widget in m_WrldSpcHealthWidgets)
        {
            if (widget.TankHealthComponent == healthComponent)
            {
                Destroy(widget.gameObject);

                m_WrldSpcHealthWidgets.Remove(widget);

                break;
            }
        }
    }


    private void OnDisable()
    {
        AITank.OnSpawn -= OnAITankSpawn;
        TankHealth.OnRemoveHealthWidget -= OnTankRemoveHealthWidget;
    }


    private void OnAITankSpawn(BaseTank targetTank)
    {
        if (targetTank is AITank aiTank)
        {
            GameObject m_TankHealthWidgetGO = Instantiate(
                m_TankHealthWidgetPrefab,
                targetTank.transform.position,
                Quaternion.identity,
                transform
                );

            if (m_TankHealthWidgetGO.TryGetComponent<WorldSpaceTankHealthWidget>(out var component))
            {
                m_WrldSpcHealthWidgets.Add(component);

                if (aiTank.TryGetComponent<TankHealth>(out var healthComponent))
                {
                    component.SetTank(healthComponent);
                }
                else
                    Debug.LogError("There is no health component attached to this AI");
            }
            else
                Debug.LogError("There is no World Space widget component attached to this widget game object!");
        }
    }
}