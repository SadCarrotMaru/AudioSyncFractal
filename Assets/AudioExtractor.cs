using System;
using System.Linq;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Multi;
using NWaves.FeatureExtractors.Options;
using NWaves.Windows;
using UnityEngine;

public struct AudioFeaturesNWaves
{
    public float MfccMean;
    public float LpcMean;
    public float PitchMean;
    public float CentroidMean;
    public float BandwidthMean;
    public float ContrastMean;
    public float RolloffMean;
}

public static class AudioFeatureExtractorNWaves
{
    private const double FrameDuration = 0.04;
    private const double HopDuration   = 0.01;
    private const int    SampleRate    = 44100;

    // Configure MFCC
    private static readonly MfccExtractor MfccEx = new MfccExtractor(new MfccOptions
    {
        SamplingRate   = SampleRate,
        FrameDuration  = FrameDuration,
        HopDuration    = HopDuration,
        FeatureCount   = 13,
        FilterBankSize = 26,
        Window         = WindowType.Hamming
    });

    // Configure LPC
    private static readonly LpcExtractor LpcEx = new LpcExtractor(new LpcOptions
    {
        SamplingRate   = SampleRate,
        FrameDuration  = FrameDuration,
        HopDuration    = HopDuration,
        LpcOrder       = 12
    });

    // Configure Pitch
    private static readonly PitchExtractor PitchEx = new PitchExtractor(new PitchOptions
    {
        SamplingRate   = SampleRate,
        FrameDuration  = FrameDuration,
        HopDuration    = HopDuration,
    });

    // Configure Spectral (centroid, spread, contrast, rolloff)
    private static readonly SpectralFeaturesExtractor SpectralEx =
        new SpectralFeaturesExtractor(new MultiFeatureOptions
    {
        SamplingRate   = SampleRate,
        FrameDuration  = FrameDuration,
        HopDuration    = HopDuration,
        FeatureList    = "centroid,spread,contrast,rolloff",
        Window         = WindowType.Hamming
    });

    /// <summary>
    /// Convenience wrapper: pull raw float samples out of a Unity AudioClip,
    /// downmix to mono, check/resample rate, then call Extract(samples, sr).
    /// </summary>
    public static AudioFeaturesNWaves FromAudioClip(AudioClip clip)
    {
        // 1) sample-rate check
        if (clip.frequency != SampleRate)
        {
            Debug.LogWarning($"AudioClip SR = {clip.frequency}, expected {SampleRate}. Features may be off.");
            // (If you need true resampling, plug in NWaves resampler here.)
        }

        // 2) pull interleaved samples
        int totalSamples = clip.samples * clip.channels;
        var data = new float[totalSamples];
        clip.GetData(data, 0);

        // 3) downmix stereo/ multi-channel → mono
        float[] mono;
        if (clip.channels > 1)
        {
            int frames = data.Length / clip.channels;
            mono = new float[frames];
            for (int i = 0; i < frames; i++)
            {
                float sum = 0f;
                for (int c = 0; c < clip.channels; c++)
                    sum += data[i * clip.channels + c];
                mono[i] = sum / clip.channels;
            }
        }
        else
        {
            mono = data;
        }

        // 4) extract
        return Extract(mono, clip.frequency);
    }

    public static AudioFeaturesNWaves Extract(float[] samples, int sr)
    {
        var f = new AudioFeaturesNWaves();

        void LogStats(string name, float[] arr)
        {
            if (arr == null || arr.Length == 0)
            {
                Debug.LogWarning($"{name}: no data");
                return;
            }
            var mean = arr.Average();
            var min  = arr.Min();
            var max  = arr.Max();
            Debug.Log($"{name}: count={arr.Length}, mean={mean:F4}, min={min:F4}, max={max:F4}");
            if (arr.Any(x => float.IsNaN(x) || float.IsInfinity(x)))
                Debug.LogWarning($"{name}: contains NaN/∞");
        }

        // → MFCC
        try
        {
            var mfcc = MfccEx.ComputeFrom(samples);
            Debug.Log($"MFCC frames: {mfcc.Count}×{mfcc[0].Length}");
            var flat = mfcc.SelectMany(r => r).ToArray();
            LogStats("MFCC", flat);
            f.MfccMean = flat.Average();
        }
        catch (Exception ex)
        {
            Debug.LogError($"MFCC failed: {ex}");
            f.MfccMean = 0f;
        }

        // → LPC
        try
        {
            var lpc = LpcEx.ComputeFrom(samples);
            Debug.Log($"LPC frames: {lpc.Count}×{lpc[0].Length}");
            var flat = lpc.SelectMany(r => r)
                          .Where(v => !float.IsNaN(v) && !float.IsInfinity(v))
                          .ToArray();
            LogStats("LPC", flat);
            f.LpcMean = flat.Average();
        }
        catch (Exception ex)
        {
            Debug.LogError($"LPC failed: {ex}");
            f.LpcMean = 0f;
        }

        // → Pitch
        try
        {
            var pitch = PitchEx.ComputeFrom(samples);
            Debug.Log($"Pitch frames: {pitch.Count}×{pitch[0].Length}");
            var flat = pitch.SelectMany(r => r)
                            .Where(v => v > 0)
                            .DefaultIfEmpty(0f)
                            .ToArray();
            LogStats("Pitch", flat);
            f.PitchMean = flat.Average();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Pitch failed: {ex}");
            f.PitchMean = 0f;
        }

        // → Spectral (centroid, spread, contrast, rolloff)
        try
        {
            var spec = SpectralEx.ComputeFrom(samples);
            Debug.Log($"Spectral frames: {spec.Count}×{spec[0].Length}");
            var cent  = spec.Select(r => r[0]).ToArray();
            var spread= spec.Select(r => r[1]).ToArray();
            var cont  = spec.Select(r => r[2]).ToArray();
            var roll  = spec.Select(r => r[3]).ToArray();

            LogStats("Centroid", cent);
            LogStats("Bandwidth", spread);
            LogStats("Contrast", cont);
            LogStats("Rolloff", roll);

            f.CentroidMean  = cent.Average();
            f.BandwidthMean = spread.Average();
            f.ContrastMean  = cont.Average();
            f.RolloffMean   = roll.Average();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Spectral failed: {ex}");
            f.CentroidMean = f.BandwidthMean = f.ContrastMean = f.RolloffMean = 0f;
        }

        return f;
    }
}
