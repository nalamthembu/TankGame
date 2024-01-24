using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "MusicLibrary", menuName = "Game/Music/Music Library")]
public class MusicScriptable : ScriptableObject
{
    public Song[] music;

    public AudioMixerGroup musicMixerGroup;
}

[System.Serializable]
public struct Song
{
    public string title;
    public string artist;
    public AudioClip clip;
    public MUSIC_TYPE type;

    public override string ToString()
    {
        return title + " - " + artist;
    }
}

public enum MUSIC_TYPE
{
    MUSIC_MENU,
    MUSIC_BOTH,
    MUSIC_IN_GAME
}