using UnityEngine;
using TMPro;

public class VersionNumberKeeper : MonoBehaviour
{
    [SerializeField] TMP_Text versionNumberText;

    private void Awake()
    {
        if (versionNumberText != null)
        {
            versionNumberText.text = $"v{Application.version}";
        }
    }
}
