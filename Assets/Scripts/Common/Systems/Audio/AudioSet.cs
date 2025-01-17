using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AudioSet : Finder
{
    [Header("Main audio settings")]
    [Tooltip("Select the audio that will play at the beginning of the scene")]
    [SerializeField] private AudioClip _audioClip;
    [SerializeField] private bool _beLooped;
    [Tooltip("Set all clips to null when component get off")]
    [SerializeField] private bool _disableOnSceneChange;
    [Tooltip("Save audio playback time to JSON file")]
    [SerializeField] private bool _saveAudioTime;
    [Tooltip("Continue audio playback with time from JSON file")]
    [SerializeField] private bool _continueAudio;
    [Space(15)]
    [Header("Additional")]
    [SerializeField] private bool _needScreamer;
    [SerializeField] private bool _screamerMustBeLooped;
    [Tooltip("Will work only if upper bool true")]
    [SerializeField] private AudioClip _screamerToPlay;

    private AudioSource[] _sourcesToIgnore;
    private List<float> volumesIgnore = new();
    private GameObject _menu;
    private Slider _menuSlider;
    private AudioSource _audioSource;

    private void Start()
    {
        _sourcesToIgnore = (AudioSource[]) GameObject.FindObjectsOfType (typeof(AudioSource));
        if (_sourcesToIgnore.Length > 1)
        {
            foreach (var a in _sourcesToIgnore)
            {
                volumesIgnore.Add(a.volume);
                print(a.volume);
            }
        }
        _menu = GameObject.FindWithTag("Menu");
        if (_menu != null)
        {
            _menuSlider = _menu.GetComponentInChildren<Slider>(true);   
        }

       FindObjs();
       MusicSet();

       if (_sourcesToIgnore.Length > 1)
            {
                for (int i = 0; i < _sourcesToIgnore.Length; i++)
                {
                    _sourcesToIgnore[i].volume = volumesIgnore[i];
                }
            }
       if (_continueAudio)
       {
            AudioContinue();
       }
       if (_needScreamer)
       {
            ScreamerPlay();
       }
    }

    private void Update()
    {
        if (_sourcesToIgnore.Length > 1)
        {
            if (_menuSlider == null)
            {
                _menuSlider = _menu.GetComponentInChildren<Slider>(true);   
            }
            else
            {
                for (int i = 0; i < _sourcesToIgnore.Length; i++)
                {
                    _sourcesToIgnore[i].volume = volumesIgnore[i] * _menuSlider.value;
                }
            }
        }
    }

    private void MusicSet()
    {
        if (AudioSourceObj.Exists(A => A.clip == null))
        {
            _audioSource = AudioSourceObj.Find(A => A.clip == null);
        }

        if (_audioSource != null)
        {
            if (_audioSource.tag == "Player")
            {
                _audioSource = null;
            }   
        }

        if (_audioSource == null & _audioClip != null)
        {
            AudioSourceCreate(_audioClip, _beLooped);
        }
        else if (_beLooped & _audioSource != null)
        {
            _audioSource.clip = _audioClip;
            _audioSource.loop = true;
            _audioSource.Play();
        }
        else if (!_beLooped & _audioSource != null)
        {
            _audioSource.clip = _audioClip;
            _audioSource.loop = _beLooped;
            _audioSource.Play();
        }
    }

    public void ScreamerPlay()
    {
        AudioSource audioSource = AudioSourceObj.Find(A => A.clip == null);

        if (_screamerMustBeLooped & audioSource != null)
        {
            audioSource.clip = _screamerToPlay;
            audioSource.loop = true;
            audioSource.volume = GameData.Volume;
            audioSource.Play();
        }
        else if (_screamerMustBeLooped & audioSource == null)
            AudioSourceCreate(_screamerToPlay, _screamerMustBeLooped);
        else
        {
            AudioSourceObj[0].PlayOneShot(_screamerToPlay, 1);
        }
    }

    public void ScreamerStop()
    {
        AudioSource audioSource = AudioSourceObj.Find(A => A.clip == _screamerToPlay & A.enabled);
        audioSource.Stop();
    }

    public void AudioContinue()
    {
        AudioSource audioSource = AudioSourceObj.Find(A => A.clip == _audioClip & A.enabled);
        SaveLoad.Instance.Load();
        audioSource.time = GameData.MusicTime;
    }

    private void AudioSourceCreate(AudioClip audioClip, bool loop)
    {
        GameObject go = new GameObject();
        go.name = "Audio " + audioClip;
        go.tag = "Audio";

        AudioSource newAudioSource = go.AddComponent<AudioSource>();
        newAudioSource.clip = audioClip;
        newAudioSource.volume = GameData.Volume;
        newAudioSource.loop = loop;
        newAudioSource.Play();
        AudioSourceObj.Add(newAudioSource);
    }

    private void OnDisable()
    {
        if (_saveAudioTime)
        {
            AudioSource audioSource = AudioSourceObj.Find(A => A.clip == _audioClip & A.enabled);
            SaveMusicTime(audioSource.time);
        }

        if (_disableOnSceneChange & AudioSourceObj != null)
        {
            foreach (var a in AudioSourceObj)
            {
                if (a != null)
                {
                    a.clip = null;
                    a.time = 0;
                    a.Stop();
                }
            }
        }
    }
}
