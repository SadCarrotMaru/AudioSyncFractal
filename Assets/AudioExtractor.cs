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
    public float ChromaMean;
    public float MelspecMean;
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
    private const double HopDuration   = 0.010;
    private const int    SampleRate    = 44100;

    // Configure MFCC
    private static readonly MfccOptions MfccOpts = new MfccOptions
    {
        SamplingRate   = SampleRate,
        FrameDuration  = FrameDuration,
        HopDuration    = HopDuration,
        FeatureCount   = 13,
        FilterBankSize = 26,               // typical mel bands
        Window         = WindowType.Hamming
    };
    private static readonly MfccExtractor MfccEx = new MfccExtractor(MfccOpts);

    // Configure Chroma
    private static readonly ChromaOptions ChromaOpts = new ChromaOptions
    {
        SamplingRate = SampleRate,
        FrameDuration = FrameDuration,
        HopDuration = HopDuration,
        Window = WindowType.Hamming
    };
    
    private static readonly ChromaExtractor ChromaEx = new ChromaExtractor(ChromaOpts);

    // Configure Mel‐bank (Filterbank)
    private static readonly FilterbankOptions MelOpts = new FilterbankOptions
    {
        SamplingRate   = SampleRate,
        FrameDuration  = FrameDuration,
        HopDuration    = HopDuration,
        FeatureCount   = 128,
        Window         = WindowType.Hamming
    };
    private static readonly FilterbankExtractor MelEx = new FilterbankExtractor(MelOpts);

    // Configure LPC
    private static readonly LpcOptions LpcOpts = new LpcOptions
    {
        SamplingRate   = SampleRate,
        FrameDuration  = FrameDuration,
        HopDuration    = HopDuration,
        LpcOrder       = 12
    };
    private static readonly LpcExtractor LpcEx = new LpcExtractor(LpcOpts);

    // Configure Pitch
    private static readonly PitchOptions PitchOpts = new PitchOptions
    {
        SamplingRate   = SampleRate,
        FrameDuration  = FrameDuration,
        HopDuration    = HopDuration,
    };
    private static readonly PitchExtractor PitchEx = new PitchExtractor(PitchOpts);

    private static readonly MultiFeatureOptions SpectralOpts = new MultiFeatureOptions
    {
        SamplingRate  = SampleRate,
        FrameDuration = FrameDuration,
        HopDuration   = HopDuration,
        FeatureList   = "centroid,spread,contrast,rolloff",
        Window        = WindowType.Hamming
    };
    private static readonly SpectralFeaturesExtractor SpectralEx =
        new SpectralFeaturesExtractor(SpectralOpts);

    public static AudioFeaturesNWaves Extract(float[] samples, int sr)
    {
        var f = new AudioFeaturesNWaves();

        // Utility local to log stats
        void LogStats(string name, float[] data)
        {
            if (data == null || data.Length == 0)
            {
                Debug.LogWarning($"{name} returned no data");
                return;
            }
            float mean = data.Average();
            float min  = data.Min();
            float max  = data.Max();
            Debug.Log($"{name}: count={data.Length}, mean={mean:F4}, min={min:F4}, max={max:F4}");
            if (data.Any(x => float.IsNaN(x) || float.IsInfinity(x)))
                Debug.LogWarning($"{name} contains NaN or Infinity");
        }

        try
        {
            var mfcc = MfccEx.ComputeFrom(samples);
            Debug.Log($"MFCC frames: {mfcc.Count} × {mfcc[0].Length}");
            var mfccFlat = mfcc.SelectMany(r => r).ToArray();
            LogStats("MFCC", mfccFlat);
            f.MfccMean = mfccFlat.Average();
        }
        catch (Exception ex)
        {
            Debug.LogError($"MFCC extraction failed: {ex}");
            f.MfccMean = 0f;
        }

        try
        {
            var chroma = ChromaEx.ComputeFrom(samples);
            Debug.Log($"Chroma frames: {chroma.Count} × {chroma[0].Length}");
            var chFlat = chroma.SelectMany(r => r).ToArray();
            LogStats("Chroma", chFlat);
            f.ChromaMean = chFlat.Average();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Chroma extraction failed: {ex}");
            f.ChromaMean = 0f;
        }

        try
        {
            var mels = MelEx.ComputeFrom(samples);
            Debug.Log($"Melspec frames: {mels.Count} × {mels[0].Length}");
            var melFlat = mels.SelectMany(r => r).ToArray();
            LogStats("Melspec", melFlat);
            f.MelspecMean = melFlat.Average();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Melspec extraction failed: {ex}");
            f.MelspecMean = 0f;
        }

        try
        {
            var lpc = LpcEx.ComputeFrom(samples);
            Debug.Log($"LPC frames: {lpc.Count} × {lpc[0].Length}");
            var lpcFlat = lpc.SelectMany(r => r).ToArray();
            var cleaned = lpcFlat.Where(v => !float.IsNaN(v) && !float.IsInfinity(v)).ToArray();
            LogStats("LPC", cleaned);
            f.LpcMean = cleaned.Average();
        }
        catch (Exception ex)
        {
            Debug.LogError($"LPC extraction failed: {ex}");
            f.LpcMean = 0f;
        }

        try
        {
            var pitch = PitchEx.ComputeFrom(samples);
            Debug.Log($"Pitch frames: {pitch.Count} × {pitch[0].Length}");
            var pFlat = pitch.SelectMany(r => r).Where(v => v > 0).DefaultIfEmpty(0f).ToArray();
            LogStats("Pitch", pFlat);
            f.PitchMean = pFlat.Average();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Pitch extraction failed: {ex}");
            f.PitchMean = 0f;
        }

        try
        {
            var spec = SpectralEx.ComputeFrom(samples);
            Debug.Log($"Spectral frames: {spec.Count} × {spec[0].Length}");
            // spec columns: centroid, spread, contrast, rolloff
            var cent  = spec.Select(r => r[0]).ToArray();
            var spread= spec.Select(r => r[1]).ToArray();
            var cont  = spec.Select(r => r[2]).ToArray();
            var roll  = spec.Select(r => r[3]).ToArray();

            LogStats("Centroid", cent);
            LogStats("Bandwidth(spread)", spread);
            LogStats("Contrast", cont);
            LogStats("Rolloff", roll);

            f.CentroidMean  = cent.Average();
            f.BandwidthMean = spread.Average();
            f.ContrastMean  = cont.Average();
            f.RolloffMean   = roll.Average();
        }
        catch (Exception ex)
        {
            Debug.LogError($"SpectralFeature extraction failed: {ex}");
            f.CentroidMean = f.BandwidthMean = f.ContrastMean = f.RolloffMean = 0f;
        }

        return f;
    }
}
