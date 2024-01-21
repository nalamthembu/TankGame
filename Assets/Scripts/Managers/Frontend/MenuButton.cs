using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public class MenuButton
{
    [SerializeField] Button m_Button;

    [SerializeField] PromptType type;

    public static event Action<PromptType> OnClickMenuButton;

    public void OnEnable() => m_Button.onClick.AddListener(OnClick);
    public void OnDisable() => m_Button.onClick.RemoveAllListeners();
    private void OnClick() => OnClickMenuButton?.Invoke(type);

}