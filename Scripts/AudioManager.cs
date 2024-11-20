using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("AUDIO SOURCE")]
    public AudioSource musicSource;
    public AudioSource SFXSource;

	[Header("CLIPS")]
	public AudioClip backgroundMain;
    public AudioClip backgroundLobby;
    public AudioClip shoot;
    public AudioClip bulletHit;
    public AudioClip grapple;
    public AudioClip dash;
    public AudioClip slash;
    public AudioClip ouch;
    public AudioClip death;
    public AudioClip enemyDeath;
    public AudioClip switchPlayer;
	public AudioClip ok;
	public AudioClip yes;
    public AudioClip regClick;
    public AudioClip specClick;

	private void Start()
	{
		musicSource.loop = true;

		musicSource.clip = backgroundMain;
        musicSource.Play();
	}

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}
