using UnityEngine;

[CreateAssetMenu(fileName = "Ambience", menuName = "Game/Sound/Ambience")]
public class AmbienceScriptable : ScriptableObject
{
    public Ambience[] ambiences;
}

[System.Serializable]
public struct Ambience
{
    public string ambienceID;
    public string[] ambienceLoops;
    public string[] clipNames;
    [Range(0, 60)]
    public float minRandomTime;
    [Range(0, 60)]
    public float maxRandomTime;
    [Range(1, 2)]
    public float minPitch;
    [Range(1,2)]
    public float maxPitch;
}