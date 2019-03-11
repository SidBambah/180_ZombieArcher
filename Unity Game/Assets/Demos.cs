using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Demos : MonoBehaviour {

    public RawImage rawImage;
    public VideoClip shootClip;
    public VideoClip reloadClip;
    public VideoClip meleeClip;

    private int prev_state = 0;
    private int state = 0;
    private GameController gm;
    private VideoPlayer videoPlayer;
    // Use this for initialization
    void Start () {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.clip = shootClip;
        //videoPlayer.Prepare();

        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
       
    }
	
    // State 1: shoot, 2: reload, 3: melee
	// Update is called once per frame
	void Update () {
        state = gm.GetState();
        if (state != prev_state)
        {
            InitiateVideo();
        }

        PlayVideo();
        



        prev_state = state;
	}

    private void InitiateVideo()
    {
        if (state == 1)
        {
            videoPlayer.clip = shootClip;
        }
        else if (state == 2)
        {
            videoPlayer.clip = reloadClip;
        }
        else if (state == 3)
        {
            videoPlayer.clip = meleeClip;
        }
        
        videoPlayer.Prepare();

    }

    private void PlayVideo()
    {
        if (state >= 4)
        {
            Color tmp = rawImage.color;
            tmp.a = 0f;
            rawImage.color = tmp;
        }
        else if (!videoPlayer.isPrepared)
        {
            Color tmp = rawImage.color;
            tmp.a = 0f;
            rawImage.color = tmp;
            return;
        }
        else
        {
            Color tmp = rawImage.color;
            tmp.a = 1f;
            rawImage.color = tmp;
            rawImage.texture = videoPlayer.texture;
            videoPlayer.Play();
        }
    }

        

    /*IEnumerator PlayVideo()
    {
        videoPlayer.Prepare();
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);
        while (!videoPlayer.isPrepared)
        {
            yield return waitForSeconds;
            break;
        }
        rawImage.texture = videoPlayer.texture;
        videoPlayer.Play();

    }*/
}
