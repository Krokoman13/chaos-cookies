using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.Events;

public class ShantiesManager : MonoBehaviour
{
    public List<AudioResource> clips;

    AudioSource audioSource;

    public AudioSource backgroundMusic;
    public float fadeDuration = 3f;

    private float originalBackgroundMusicVolume;
    private float originalShantyMusicVolume;

    float timeLeft = 0;
    public float waitTime;

    bool wasPlaying = false;

    public UnityEvent onStartPlaying;
    public UnityEvent onStopPlaying;

    private bool isTransitioning;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        originalShantyMusicVolume = audioSource.volume;

        if (backgroundMusic != null)
        {
            originalBackgroundMusicVolume = backgroundMusic.volume;
        }



        //randomize order
        clips = new List<AudioResource>(
            clips.OrderBy(_ => Random.value)
        );

        timeLeft = waitTime;
    }

    private void Update()
    {
        if (wasPlaying != audioSource.isPlaying)
        {
            wasPlaying = audioSource.isPlaying;

            if (audioSource.isPlaying)
            {
                onStartPlaying?.Invoke();
            }
            else
            { 
                onStopPlaying?.Invoke();
            }
        }

        if (audioSource.isPlaying || isTransitioning)
        {
            return;
        }

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0)
        {
            isTransitioning = true;
            StartCoroutine(PlayNextWithFade());
        }
    }

    private void OnDisable()
    {
        if (audioSource.isPlaying) onStopPlaying?.Invoke();
        wasPlaying = false;
    }

    private IEnumerator FadeVolume(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    private IEnumerator CrossFade(
        AudioSource fadeOutSource,
        float fadeOutTarget,
        AudioSource fadeInSource,
        float fadeInTarget,
        float duration)
    {
        float fadeOutStart = fadeOutSource.volume;
        float fadeInStart = fadeInSource.volume;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            fadeOutSource.volume = Mathf.Lerp(fadeOutStart, fadeOutTarget, t);
            fadeInSource.volume = Mathf.Lerp(fadeInStart, fadeInTarget, t);

            yield return null;
        }

        fadeOutSource.volume = fadeOutTarget;
        fadeInSource.volume = fadeInTarget;
    }

    private IEnumerator PlayNextWithFade()
    {
        AudioResource next = clips.First();
        audioSource.resource = next;

        audioSource.volume = 0f;
        audioSource.Play();

        // Fade background out while shanty fades in
        yield return StartCoroutine(
            CrossFade(
                backgroundMusic,
                0f,
                audioSource,
                originalShantyMusicVolume,
                fadeDuration
            )
        );

        clips.RemoveAt(0);
        clips.Add(next);

        // Wait until almost finished
        while (audioSource.isPlaying &&
               audioSource.time < audioSource.clip.length - fadeDuration)
        {
            yield return null;
        }

        // Fade shanty out while background fades back in
        yield return StartCoroutine(
            CrossFade(
                audioSource,
                0f,
                backgroundMusic,
                originalBackgroundMusicVolume,
                fadeDuration
            )
        );

        audioSource.Stop();

        // Restore shanty volume for next play
        audioSource.volume = originalShantyMusicVolume;

        timeLeft = waitTime;
        isTransitioning = false;
    }
}
