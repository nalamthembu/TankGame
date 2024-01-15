using UnityEngine;

[CreateAssetMenu(fileName = "SoundScriptable", menuName = "Game/Sound/Sound Scriptable")]
public class InGameSoundScriptable : ScriptableObject
{
    public Sound[] sounds;
}

[System.Serializable]
public struct Sound
{
    public string soundID;
    public AudioClip[] clips;
    public SoundType type;

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

public enum SoundType
{
    SFX,
    SPEECH,
    AMBIENCE,
    ONESHOT_AMBIENCE,
    FRONTEND
}