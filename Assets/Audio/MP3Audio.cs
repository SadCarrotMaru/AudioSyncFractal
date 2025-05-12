using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using SFB;


// Mai vreau sa fac si un visualizer pentru audio, ca sa va fie mai usor pt sincronizarea cu fractalii + o sa mai fac un file cu filtre (low pass, high pass, kalman, etc), nu stiu cum ies dar sa atenuez tranzitiile pt fractali
// Hopefully in vacanta
/*
public class Mp3FilePickerAnalyzer : MonoBehaviour
{
    public int sampleSize = 1024;
    private AudioSource audioSource;
    private float[] samples;
    private float[] spectrum;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        samples = new float[sampleSize];
        spectrum = new float[sampleSize];

        PickAndLoadMP3();
    }

    public void PickAndLoadMP3()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Select MP3", "", "mp3", false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            StartCoroutine(LoadAndPlayMP3(paths[0]));
        }
    }

    IEnumerator LoadAndPlayMP3(string path)
    {
        string url = "file://" + path;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            Debug.Log(url);
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load MP3: " + www.error);
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
    void Update()
    {
        if (!audioSource.isPlaying) return;

        audioSource.GetOutputData(samples, 0);
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        float rms = GetRMS(samples);
        float peak = GetPeakAmplitude(samples);
        float loudness = rms * 100f;

        float dominantFreq = GetDominantFrequency(spectrum, AudioSettings.outputSampleRate);
        float spectralCentroid = GetSpectralCentroid(spectrum, AudioSettings.outputSampleRate);
        float spectralFlatness = GetSpectralFlatness(spectrum);

        float zeroCrossingRate = GetZeroCrossingRate(samples);  // cam da 0 mereu, mai trebuie lucrat la file ul asta dar este un start oke imo.
        float estimatedPitch = dominantFreq; 
        float pitchConfidence = Mathf.Clamp01(1f - spectralFlatness); // 1 = pure tone, 0 = noise

        Debug.Log($@"
         Audio Analysis:
         - RMS: {rms:F4}
         - Peak: {peak:F4}
         - Loudness: {loudness:F2}
         Frequency:
         - Dominant Freq: {dominantFreq:F1} Hz
         - Spectral Centroid: {spectralCentroid:F1} Hz
         - Spectral Flatness: {spectralFlatness:F3}
         Pitch:
         - Est. Pitch: {estimatedPitch:F1} Hz
         - Zero-Crossing Rate: {zeroCrossingRate:F3}
         - Pitch Confidence: {pitchConfidence:F2}
        ");
    }

    float GetRMS(float[] data)
    {
        float sum = 0f;
        foreach (float f in data)
            sum += f * f;
        return Mathf.Sqrt(sum / data.Length);
    }

    float GetPeakAmplitude(float[] data)
    {
        float peak = 0f;
        foreach (float f in data)
            if (Mathf.Abs(f) > peak) peak = Mathf.Abs(f);
        return peak;
    }

    float GetDominantFrequency(float[] spectrum, int sampleRate)
    {
        int maxIndex = 0;
        float maxVal = 0;
        for (int i = 1; i < spectrum.Length; i++)
        {
            if (spectrum[i] > maxVal)
            {
                maxVal = spectrum[i];
                maxIndex = i;
            }
        }
        return maxIndex * sampleRate / 2f / spectrum.Length;
    }

    float GetZeroCrossingRate(float[] data)
    {
        int zeroCrossings = 0;
        for (int i = 1; i < data.Length; i++)
        {
            if ((data[i - 1] > 0 && data[i] < 0) || (data[i - 1] < 0 && data[i] > 0))
                zeroCrossings++;
        }
        return (float)zeroCrossings / data.Length;
    }

    float GetSpectralCentroid(float[] spectrum, int sampleRate)
    {
        float sumWeightedFreq = 0f;
        float sumMagnitude = 0f;
        for (int i = 0; i < spectrum.Length; i++)
        {
            float freq = i * sampleRate / 2f / spectrum.Length;
            float mag = spectrum[i];
            sumWeightedFreq += freq * mag;
            sumMagnitude += mag;
        }

        return (sumMagnitude == 0) ? 0 : sumWeightedFreq / sumMagnitude;
    }

    float GetSpectralFlatness(float[] spectrum)
    {
        double geoMean = 1.0;
        double arithMean = 0.0;
        int count = spectrum.Length;

        for (int i = 0; i < count; i++)
        {
            float val = Mathf.Max(spectrum[i], 1e-10f); // prevent log(0)
            geoMean *= val;
            arithMean += val;
        }

        geoMean = Mathf.Pow((float)geoMean, 1f / count);
        arithMean /= count;

        return (float)(geoMean / arithMean);
    }
}
*/