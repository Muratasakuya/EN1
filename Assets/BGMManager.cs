using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour
{

    public AudioSource bgmSource;

    // Start is called before the first frame update
    void Start()
    {

        if (bgmSource != null)
        {
            bgmSource.Play();
        }
    }
    public void PlayBGM()
    {
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (bgmSource != null)
        {

            PlayBGM();
        }
    }
}
