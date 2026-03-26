using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public AudioClip music;
    public float volume = 0.5f;
    public float fadeInDuration = 2f;

    private AudioSource source;

    void Start()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.clip = music;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0f;
        source.Play();
        StartCoroutine(FadeIn());
    }

    System.Collections.IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, volume, t / fadeInDuration);
            yield return null;
        }
        source.volume = volume;
    }
}