using UnityEngine;
using System;

[Serializable]
public class Sound
{
	public string name;
	public AudioClip clip;
	[Range(0f, 1f)] public float volume = 1f;
}

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance { get; private set; }

	[Header("DŸwiêki")]
	public Sound[] sfxSounds;
	public Sound[] musicSounds;

	[Header("ród³a dŸwiêku")]
	public AudioSource sfxSource;
	public AudioSource musicSource;
	public AudioSource stepsSource;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		} else
		{
			Destroy(gameObject);
		}
	}

	public void PlaySFX(string soundName)
	{
		Sound s = Array.Find(sfxSounds, sound => sound.name == soundName);
		if (s != null)
		{
			sfxSource.PlayOneShot(s.clip, s.volume);
		} else
		{
			Debug.LogWarning("DŸwiêk SFX: " + soundName + " nie zosta³ znaleziony!");
		}
	}

	public void PlayMusic(string musicName, AudioSource musicSrc)
	{
		Sound s = Array.Find(musicSounds, sound => sound.name == musicName);
		if (s != null)
		{
			musicSrc.clip = s.clip;
			musicSrc.volume = s.volume;
			musicSrc.loop = true;
			musicSrc.Play();
		} else
		{
			Debug.LogWarning("DŸwiêk Muzyki: " + musicName + " nie zosta³ znaleziony!");
		}
	}

	public void StopMusic(AudioSource musicSrc)
	{
		musicSrc.Stop();
	}

}