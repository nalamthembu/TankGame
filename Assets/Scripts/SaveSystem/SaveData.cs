using System;

[System.Serializable]
public class SaveData
{
    public float cameraSensitivity;

    public SaveData(float cameraSensitivity)
    {
        this.cameraSensitivity = cameraSensitivity;
    }
}