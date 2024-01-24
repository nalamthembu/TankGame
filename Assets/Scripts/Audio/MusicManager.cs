using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    [SerializeField] MusicScriptable musicScriptable;

    private AudioSource source;

    public static MusicManager instance;

    private Song NowPlaying;

    private int nowPlaying = -1;

    private bool IsInGame;

    public bool IsFadingOut { get; set; }

    public bool IsFadingIn { get; set; }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        source = gameObject.AddComponent<AudioSource>();

        source.outputAudioMixerGroup = musicScriptable.musicMixerGroup;

        source.bypassReverbZones = true;

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        GameManager.OnGameIsStarting += OnGameIsStarting;
        LevelManager.OnLoadingStart += OnStartLoading;
        MainMenuManager.OnEnterMainMenu += OnEnterMainMenu;
    }

    private void OnDisable()
    {
        GameManager.OnGameIsStarting -= OnGameIsStarting;
        LevelManager.OnLoadingStart -= OnStartLoading;
        MainMenuManager.OnEnterMainMenu -= OnEnterMainMenu;
    }

    public void PlayNextSong()
    {
        nowPlaying++;

        MakeNowPlayingIndexValid();

        int MAX_ITERATION = 1000;

        for (int i = 0; i < MAX_ITERATION; i++)
        {
            if (!IsSongValid())
            {
                nowPlaying++;
                MakeNowPlayingIndexValid();
                continue;
            }
            else
            {
                PlaySong(musicScriptable.music[nowPlaying]);
                print(i + " iterations");
                break;
            }
        }
    }

    void MakeNowPlayingIndexValid()
    {
        if (nowPlaying > musicScriptable.music.Length - 1)
            nowPlaying = 0;
    }

    bool IsSongValid()
    {
        if (!IsInGame)
        {
            switch (musicScriptable.music[nowPlaying].type)
            {
                case MUSIC_TYPE.MUSIC_BOTH:
                case MUSIC_TYPE.MUSIC_MENU:

                    return true;
            }
        }
        else
        {
            if (musicScriptable.music[nowPlaying].type == MUSIC_TYPE.MUSIC_IN_GAME)
                return true;
        }

        return false;
    }

    private void OnEnterMainMenu()
    {
        IsInGame = false;
        PlayNextSong();
        FadeInCurrentSong();
    }

    private void OnStartLoading() => FadeOutCurrentSong();
    

    private void OnGameIsStarting()
    {
        IsInGame = true;
        PlayNextSong();
        FadeInCurrentSong();
    }

    private void Update()
    {
        if (!source.isPlaying)
        {
            PlayNextSong();
        }
    }

    public Song GetNowPlaying() => NowPlaying;

    public int GetTotalMusicCount() => musicScriptable.music.Length;

    public Song[] GetAllSongs() => musicScriptable.music;

    public float GetSeekTime() => source.time;

    public void SetSeekTime(Slider slider)
    {
        slider.value = Mathf.Clamp(slider.value, 0, source.clip.length);

        source.time = slider.value;
    }

    public void PlaySongByMetadata(string title, string artist)
    {
        foreach (Song song in musicScriptable.music)
        {
            if (song.artist == artist && song.title == title)
            {
                PlaySong(song);

                return;
            }
        }

        Debug.LogWarning("Could not find requested song " + title + " by " + artist);
    }

    public void PlaySongByTitle(string title)
    {
        foreach (Song song in musicScriptable.music)
        {
            if (song.title == title)
            {
                PlaySong(song);

                return;
            }
        }

        Debug.LogWarning("Could not find requested song " + title);
    }

    public void FadeOutCurrentSong() => StartCoroutine(FadeOutCurrentSong_Coroutine());

    public void FadeInCurrentSong() => StartCoroutine(FadeInCurrentSong_Coroutine());

    private IEnumerator FadeInCurrentSong_Coroutine()
    {
        while (source.volume < 1)
        {
            IsFadingIn = true;

            source.volume += Time.deltaTime;

            if (source.volume + 0.01F >= 1)
            {
                source.volume = 1;

                IsFadingIn = false;

                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }
    private IEnumerator FadeOutCurrentSong_Coroutine()
    {
        while (source.volume > 0)
        {
            IsFadingOut = true;

            source.volume -= Time.deltaTime / 4;

            if (source.volume - 0.01F <= 0)
            {
                source.volume = 0;

                IsFadingOut = false;

                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    //AUX_METHOD
    private void PlaySong(Song song)
    {
        source.clip = song.clip;

        source.Play();

        NowPlaying = song;
    }
}