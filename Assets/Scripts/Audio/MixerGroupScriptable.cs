using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "MixerGroupScriptable", menuName = "Game/Sound/MixerGroup Scriptable")]
public class MixerGroupScriptable : ScriptableObject
{
    public Mixer[] mixers;

    private void OnValidate()
    {
        //Check for mixers with identical names
        string lastMixerName = string.Empty;
        for(int i = 0; i < mixers.Length;i++)
        {
            if (lastMixerName == mixers[i].mixerID)
            {
                Debug.LogWarning("There is already a mixer with the name " + lastMixerName + " defined!");
                break; // No need to continue checking once a duplicate is found
            }

            lastMixerName = mixers[i].mixerID;
        }
    }
}

[System.Serializable]
public struct Mixer
{
    public string mixerID;
    public AudioMixerGroup mixerGroup;
    public SoundType type;
}