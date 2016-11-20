using UnityEngine;
using System.Collections.Generic;

public class AnimatedPainting : MonoBehaviour {

    enum ViewState { Stale, Animating, Stopping, Decaying};

    [SerializeField]
    Material templateMat;

    Material mat;

    int imgIndex = 0;

    [SerializeField]
    Texture2D[] sequence;

    [SerializeField]
    Texture2D decayCopy;

    [SerializeField]
    float fps = 10;

    ViewState viewing = ViewState.Stale;

    float stoppingStart = 0;

    [SerializeField]
    float stoppingFps = 5;

    [SerializeField]
    float stoppingDuration = 2f;

    [SerializeField]
    int minDecaysPerIteration = 10;

    [SerializeField]
    int maxDecaysPerIteration = 100;

    [SerializeField]
    float decayDuration = 10f;

    float decayStart;


	void Start () {
        mat = new Material(templateMat);
        MeshRenderer mRend = GetComponent<MeshRenderer>();
        mRend.material = mat;
        StartCoroutine(AnimateImage());
        decayCopy = new Texture2D(sequence[imgIndex].width, sequence[imgIndex].height);        
    }


    IEnumerator<WaitForSeconds> AnimateImage () {

        mat.SetTexture("_MainTex", sequence[imgIndex]);

        while (true)
        {
            if (viewing == ViewState.Animating)
            {
                SetNextTex();
                yield return new WaitForSeconds(1 / fps);
            } else if (viewing == ViewState.Stopping)
            {
                SetNextTex();
                float progress = Mathf.Clamp01((Time.timeSinceLevelLoad - stoppingStart) / stoppingDuration);
                if (progress == 1)
                {

                    viewing = ViewState.Decaying;
                    decayStart = Time.timeSinceLevelLoad;
                    decayCopy.SetPixels(sequence[imgIndex].GetPixels());
                    decayCopy.Apply();
                    mat.SetTexture("_MainTex", decayCopy);
                }

                yield return new WaitForSeconds(1 / Mathf.Lerp(fps, stoppingFps, progress * 2));
            } else if (viewing == ViewState.Decaying)
            {
                for (int i=0,l=Random.Range(minDecaysPerIteration, maxDecaysPerIteration); i< l; i++)
                {
                    DecayDecay();
                }
                decayCopy.Apply();
                mat.SetTexture("_MainTex", decayCopy);
                if (Time.timeSinceLevelLoad - decayStart > decayDuration)
                {
                    viewing = ViewState.Stale;
                }
                yield return new WaitForSeconds(1 / fps);
            } else
            {
                yield return new WaitForSeconds(1 / fps);
            }
            
        }
	}

    void DecayDecay()
    {
        int w = decayCopy.width;
        int h = decayCopy.height;
        int x = Random.Range(0, w);
        int y = Random.Range(0, h);
        //decayCopy.SetPixel(x, y, Color.magenta);
        
        Color pix =  decayCopy.GetPixel(x, y);
        if (pix.r > pix.g)
        {
            int x2 = (x + w - 1) % w;
            decayCopy.SetPixel(x, y, decayCopy.GetPixel(x2, y));
            decayCopy.SetPixel(x2, y, pix);
        }
        else if (pix.b > pix.g)
        {
            int y2 = (y + h - 1) % h;
            decayCopy.SetPixel(x, y, decayCopy.GetPixel(x, y2));
            decayCopy.SetPixel(x, y2, pix);
        }
        else if (pix.g > pix.r)
        {
            int y2 = (y + 1) % h;
            decayCopy.SetPixel(x, y, decayCopy.GetPixel(x, y2));
            decayCopy.SetPixel(x, y2, pix);
        }
        else
        {
            int x2 = (x + 1) % w;
            decayCopy.SetPixel(x, y, decayCopy.GetPixel(x2, y));
            decayCopy.SetPixel(x2, y, pix);
        }
    }

    void SetNextTex()
    {
        imgIndex++;
        imgIndex %= sequence.Length;
        mat.SetTexture("_MainTex", sequence[imgIndex]);
    }

    void OnMouseEnter()
    {
        viewing = ViewState.Animating;
    }

    void OnMouseExit()
    {
        viewing = ViewState.Stopping;
        stoppingStart = Time.timeSinceLevelLoad;
    }
}
