using UnityEngine;

public class FreqBandController : MonoBehaviour
{
    [SerializeField] public int band;
    [SerializeField] public float startScale, scaleMultiplier;
    void Start()
    {
        
    }

    void Update()
    {
       // transform.localScale = new Vector3(transform.localScale.x, AudioPeer.bandBuffer[band] * scaleMultiplier + startScale, transform.localScale.z);
        if(band <= 3)
            transform.localScale = new Vector3(transform.localScale.x, AudioPeer.amplitudeBuffer * scaleMultiplier + startScale, transform.localScale.z);
        else
            transform.localScale = new Vector3(transform.localScale.x, AudioPeer.spectralRolloffBuffer * scaleMultiplier + startScale, transform.localScale.z);
    }
}
