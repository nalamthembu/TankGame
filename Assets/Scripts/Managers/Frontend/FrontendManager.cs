using UnityEngine;

public class FrontendManager : MonoBehaviour
{
    public void SaveSettings(float cameraSensitivity)
    {
        SaveSystem.TrySave(new SaveData(cameraSensitivity));
    }
}