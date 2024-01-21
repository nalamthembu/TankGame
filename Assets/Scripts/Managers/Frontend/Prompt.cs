using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

[Serializable]
public class Prompt
{
    [SerializeField] string m_Header;
    [SerializeField] string m_Question;
    [SerializeField] TMP_Text m_HeaderText;
    [SerializeField] TMP_Text m_QuestionText;
    [SerializeField] Button m_Positive;
    [SerializeField] Button m_Negative;
    [SerializeField] PromptType m_PromptType;

    public static event Action<Prompt> OnPromptPositive, OnPromptNegative;

    public PromptType PromptType { get { return m_PromptType; } }

    public void Initialise(PromptType type)
    {
        m_HeaderText.text = m_Header;
        m_QuestionText.text = m_Question;
        m_PromptType = type;
    }

    public void OnEnable()
    {
        m_Positive.onClick.AddListener(OnClickPos);
        m_Negative.onClick.AddListener(OnClickNeg);
    }

    public void OnDisable()
    {
        m_Positive.onClick.RemoveAllListeners();
        m_Negative.onClick.RemoveAllListeners();
    }

    private void OnClickPos() => OnPromptPositive?.Invoke(this);
    private void OnClickNeg() => OnPromptNegative?.Invoke(this);
}