using UnityEngine;
using UnityEngine.InputSystem;

public class MovementSound : MonoBehaviour
{
    public float secondsPerSwimSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] AudioClip[] audioClips;
    private InputAction moveForwardAction;
    private bool audioPlayerPlaying = false;
    private bool shouldStopPlaying = false;

    void Start()
    {
        moveForwardAction = InputSystem.actions.FindAction("MoveForward");
    }

    private void Update()
    {
        if (moveForwardAction.WasPressedThisFrame())
        {
            shouldStopPlaying = false;
            if (!audioPlayerPlaying)
            {
                print(shouldStopPlaying);
                CallAudio();
            }
        }
        if (moveForwardAction.WasReleasedThisFrame())
        {
            shouldStopPlaying = true;
        }
    }

    void CallAudio()
    {
        audioPlayerPlaying = true;
        Invoke("PlayAudio", secondsPerSwimSound);
    }

    void PlayAudio()
    {
        
        if (!shouldStopPlaying)
        {
            audioSource.clip = chooseRandomSound();
            audioSource.Play();
            CallAudio();
        }
        else
        {
            audioPlayerPlaying = false;
        }
    }

    private AudioClip lastAudioClip;
    AudioClip chooseRandomSound()
    {
        print(audioClips.Length);
        AudioClip clip = audioClips[Random.Range(0, audioClips.Length - 1)];
        if (clip == lastAudioClip)
        {
            chooseRandomSound();
        }
        else
        {
            lastAudioClip = clip;
        }
        return lastAudioClip;
    }
}
