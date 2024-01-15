using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "FESoundScriptable", menuName = "Game/Sound/FESound Scriptable")]
public class FrontendSoundScriptable : ScriptableObject
{
    public FESound[] sounds;
    public AudioMixerGroup FEMixerGroup;
}

[System.Serializable]
public struct FESound
{
    public string soundID;
    public AudioClip[] clips;
    public AudioClip GetRandomClip()
    {
        if (clips is null)
        {
            Debug.LogError("The clip array for " + soundID + " is null");
            return null;
        }

        //Sometimes a sound won't have more than one clip but if it does...
        if (clips.Length > 1)
            return clips[Random.Range(0, clips.Length)];
        else //otherwise just choose the only clip in the array
            return clips[0];
    }
}
