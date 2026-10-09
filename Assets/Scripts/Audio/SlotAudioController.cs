using UnityEngine;

public class SlotAudioController : MonoBehaviour {
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource reelLoopSource;

    [Header("Audio Clips (Optional)")]
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip spinLoopClip;
    [SerializeField] private AudioClip reelStopClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip jackpotClip;

    public void PlayButtonClick() {
        PlayClip(buttonClickClip);
    }

    public void StartSpinLoop() {
        if (reelLoopSource != null && spinLoopClip != null) {
            reelLoopSource.clip = spinLoopClip;
            reelLoopSource.loop = true;
            reelLoopSource.Play();
        }
    }

    public void StopSpinLoop() {
        if (reelLoopSource != null && reelLoopSource.isPlaying) {
            reelLoopSource.Stop();
        }
    }

    public void PlayReelStop() {
        PlayClip(reelStopClip);
    }

    public void PlayWin() {
        PlayClip(winClip);
    }

    public void PlayJackpot() {
        PlayClip(jackpotClip != null ? jackpotClip : winClip);
    }

    private void PlayClip(AudioClip clip) {
        if (sfxSource != null && clip != null) {
            sfxSource.PlayOneShot(clip);
        }
    }
}
