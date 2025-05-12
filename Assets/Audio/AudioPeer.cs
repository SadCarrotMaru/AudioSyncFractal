using UnityEngine;

public enum AudioProfiles : int
{
    None = 0,
    LowPass = 2,
    HighPass = 5,
    Reduced = 7,
}

[RequireComponent (typeof (AudioSource))]
public class AudioPeer : MonoBehaviour
{
    AudioSource _audioSource;
    public float initialBuffDecrease = 0.005f;
    public float buffDecreaseMultiplier = 1.2f;
    public AudioProfiles audioProfileSetter = AudioProfiles.HighPass;
    public float rolloffThreshold = 0.60f; // 60% energy threshold  --> deci practic determini momentul in care se acumuleaza x% din energie, energia fiind puterea sunetului (daca ai in prima parte a sample ului liniste si majoritatea sunetului e la final, atunci o sa fie mai mare spectralul)
    public float rolloffBufferDecrease = 0.005f; 
    public float rolloffBufferMultiplier = 1.2f; 

    public static float[] _samplesLeft = new float[512];
    public static float[] _samplesRight = new float[512];
    public static float[] freqBands = new float[8];
    public static float[] bandBuffer = new float[8];
    float[] bufferDecrease = new float[8];

    float[] freqBandHighest = new float[8];
    public static float[] audioBand = new float[8];
    public static float[] audioBandBuffer = new float[8];

    public static float amplitude, amplitudeBuffer; 
    float amplitudeHighest;

    public static float spectralRolloff;
    public static float spectralRolloffBuffer;    
    private float rolloffBufferDecreaseSpeed;


    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        SetAudioProfile(value: (float)audioProfileSetter);
    }

    void Update()
    {
        GetSpectrumAudioSource();
        MakeFrequencyBands();
        BandBuffer();
        CreateAudioBands();
        GetAmplitude();
        CalculateSpectralRolloff();
        SmoothSpectralRolloff();
    }

    void SmoothSpectralRolloff()
    {
        if (spectralRolloff > spectralRolloffBuffer)
        {
            spectralRolloffBuffer = spectralRolloff;
            rolloffBufferDecreaseSpeed = rolloffBufferDecrease;
        }
        else
        {
            spectralRolloffBuffer -= Time.deltaTime * rolloffBufferDecreaseSpeed;
            rolloffBufferDecreaseSpeed *= rolloffBufferMultiplier;
        }

        spectralRolloffBuffer = Mathf.Clamp01(spectralRolloffBuffer);
    }

    void CalculateSpectralRolloff()
    {
        float totalEnergy = 0f;
        float rolloffEnergy = 0f;
        int rolloffIndex = 0;

        for (int i = 0; i < 512; i++)
        {
            totalEnergy += _samplesLeft[i] + _samplesRight[i];
        }

        float energyThreshold = rolloffThreshold * totalEnergy;
        float cumulativeEnergy = 0f;

        for (int i = 0; i < 512; i++)
        {
            cumulativeEnergy += _samplesLeft[i] + _samplesRight[i];
            if (cumulativeEnergy >= energyThreshold)
            {
                rolloffIndex = i;
                break;
            }
        }

        spectralRolloff = (float)rolloffIndex / 512f;
       // Debug.Log($"Spectral Rolloff: {spectralRolloff} ---------->{rolloffIndex}"); // Debug log for spectral rolloff value
    }

    void SetAudioProfile(float value)
    {
        for (int i = 0; i < 8; i++)
            freqBandHighest[i] = value;
    }

    void GetAmplitude()
    {
        float currentAmplitude = 0;
        float currentAmplitudeBuffer = 0;

        for (int i = 0; i < 8; i++)
        {
            currentAmplitude += audioBand[i];
            currentAmplitudeBuffer += audioBandBuffer[i];
        }
        if(currentAmplitude > amplitudeHighest)
            amplitudeHighest = currentAmplitude;

        amplitude = currentAmplitude / amplitudeHighest;
        amplitudeBuffer = currentAmplitudeBuffer / amplitudeHighest;
    }

    void CreateAudioBands()
    {
        for (int i = 0; i < 8; i++)
        {
            if (freqBands[i] > freqBandHighest[i])
                freqBandHighest[i] = freqBands[i];

            audioBand[i] = freqBands[i] / freqBandHighest[i];
            audioBandBuffer[i] = bandBuffer[i] / freqBandHighest[i];
        }
    }

    void GetSpectrumAudioSource()
    {
        _audioSource.GetSpectrumData(_samplesLeft, 0, FFTWindow.BlackmanHarris);
        _audioSource.GetSpectrumData(_samplesRight, 1, FFTWindow.BlackmanHarris);
    }

    void BandBuffer()
    {
        for (int i = 0; i < 8; i++)
        {
            if (freqBands[i] > bandBuffer[i])
            {
                bandBuffer[i] = freqBands[i];
                bufferDecrease[i] = initialBuffDecrease;
            }
            else
            {
                bandBuffer[i] -= Time.deltaTime * bufferDecrease[i];
                bufferDecrease[i] *= buffDecreaseMultiplier;
            }
        }
    }

    void MakeFrequencyBands()
    {
        int count = 0, pow2 = 1;
        for (int i = 0; i < 8; i++)
        {
            float avg = 0;
            int sampleCount = pow2 * 2;
            if (i == 7)
                sampleCount += 2;

            for (int j = 0; j < sampleCount; j++)
            {
                avg += (_samplesLeft[count] + _samplesRight[count]) * (count + 1);
                count++;
            }

            avg /= count;
            freqBands[i] = avg * 10;
        }
    }
}
