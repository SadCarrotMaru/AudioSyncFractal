using System;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using CsvHelper;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Multi;
using NWaves.FeatureExtractors.Options;
using NWaves.Windows;
using NAudio.Wave;

namespace DEAMFeatureExtraction
{
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

    class SongLabel
    {
        public string song_id { get; set; }
        public float valence_mean { get; set; }
        public float arousal_mean { get; set; }
    }

    class Program
    {
        // Analysis parameters
        private const int SampleRate = 44100;
        private const double FrameDuration = 0.04;
        private const double HopDuration   = 0.01;

        // NWaves extractors
        private static readonly MfccExtractor MfccEx = new MfccExtractor(new MfccOptions
        {
            SamplingRate   = SampleRate,
            FrameDuration  = FrameDuration,
            HopDuration    = HopDuration,
            FeatureCount   = 13,
            FilterBankSize = 26,
            Window         = WindowType.Hamming
        });

        private static readonly LpcExtractor LpcEx = new LpcExtractor(new LpcOptions
        {
            SamplingRate  = SampleRate,
            FrameDuration = FrameDuration,
            HopDuration   = HopDuration,
            LpcOrder      = 12
        });

        private static readonly PitchExtractor PitchEx = new PitchExtractor(new PitchOptions
        {
            SamplingRate  = SampleRate,
            FrameDuration = FrameDuration,
            HopDuration   = HopDuration
        });

        private static readonly SpectralFeaturesExtractor SpectralEx = 
            new SpectralFeaturesExtractor(new MultiFeatureOptions
        {
            SamplingRate  = SampleRate,
            FrameDuration = FrameDuration,
            HopDuration   = HopDuration,
            FeatureList   = "centroid,spread,contrast,rolloff",
            Window        = WindowType.Hamming
        });

        static void Main(string[] args)
        {
            string labelsCsv = "../annotations/per_song/song_level/first_20000.csv";
            string audioDir  = "../DEAM_audio/MEMD_audio";
            string outputCsv = "../output/features_reduced2.csv";

            if (!Directory.Exists(audioDir) || !File.Exists(labelsCsv))
            {
                Console.WriteLine("❌ Check that audioDir and labelsCsv paths exist.");
                return;
            }

            // Load valence/arousal labels
            List<SongLabel> labels;
            using (var reader = new StreamReader(labelsCsv))
            using (var csv    = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                labels = csv.GetRecords<SongLabel>().ToList();
            }
            Console.WriteLine($"ℹ️  Loaded {labels.Count} labels.");

            // Prepare output
            Directory.CreateDirectory(Path.GetDirectoryName(outputCsv));
            using var writer = new StreamWriter(outputCsv);
            writer.WriteLine("song_id,mfcc_mean,lpc_mean,pitch_mean,centroid_mean,bandwidth_mean,contrast_mean,rolloff_mean,valence_mean,arousal_mean");

            int processed = 0;
            foreach (var label in labels)
            {
                // limit to first 500 if desired
                if (++processed > 500) break;

                // find file
                string filePath = null;
                foreach (var ext in new[] { ".wav", ".mp3" })
                {
                    var p = Path.Combine(audioDir, label.song_id + ext);
                    if (File.Exists(p)) { filePath = p; break; }
                }
                if (filePath == null)
                {
                    Console.WriteLine($"⚠️  Missing: {label.song_id}");
                    continue;
                }

                Console.WriteLine($"▶️  Processing {label.song_id}");

                // load samples with NAudio (AudioFileReader works for WAV and MP3)
                float[] samples;
                int sr;
                try
                {
                    using var afr = new AudioFileReader(filePath);
                    sr = afr.WaveFormat.SampleRate;
                    var temp = new List<float>();
                    var buffer = new float[sr * afr.WaveFormat.Channels];
                    int read;
                    while ((read = afr.Read(buffer, 0, buffer.Length)) > 0)
                        temp.AddRange(buffer.Take(read));

                    // downmix stereo to mono if needed
                    if (afr.WaveFormat.Channels > 1)
                    {
                        samples = Enumerable.Range(0, temp.Count / afr.WaveFormat.Channels)
                            .Select(i => temp.Skip(i * afr.WaveFormat.Channels)
                                              .Take(afr.WaveFormat.Channels)
                                              .Average())
                            .ToArray();
                    }
                    else
                    {
                        samples = temp.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌  Failed loading: {ex.Message}");
                    continue;
                }

                // resample if needed
                if (sr != SampleRate)
                {
                    Console.WriteLine($"⚠️  Unexpected SR {sr}, skipping.");
                    continue;
                }

                // extract and average features
                var mfccMean = MfccEx.ComputeFrom(samples)
                                     .SelectMany(f => f)
                                     .Average();

                var lpcMean = LpcEx.ComputeFrom(samples)
                                    .SelectMany(f => f)
                                    .Where(v => !float.IsNaN(v) && !float.IsInfinity(v))
                                    .Average();

                var pitchMean = PitchEx.ComputeFrom(samples)
                                       .SelectMany(f => f)
                                       .Where(v => v > 0)
                                       .DefaultIfEmpty(0f)
                                       .Average();

                var spec = SpectralEx.ComputeFrom(samples);
                var centroidMean  = spec.Select(f => f[0]).Average();
                var bandwidthMean = spec.Select(f => f[1]).Average();
                var contrastMean  = spec.Select(f => f[2]).Average();
                var rolloffMean   = spec.Select(f => f[3]).Average();

                // write row
                writer.WriteLine(string.Join(",",
                    label.song_id,
                    mfccMean.ToString(CultureInfo.InvariantCulture),
                    lpcMean.ToString(CultureInfo.InvariantCulture),
                    pitchMean.ToString(CultureInfo.InvariantCulture),
                    centroidMean.ToString(CultureInfo.InvariantCulture),
                    bandwidthMean.ToString(CultureInfo.InvariantCulture),
                    contrastMean.ToString(CultureInfo.InvariantCulture),
                    rolloffMean.ToString(CultureInfo.InvariantCulture),
                    label.valence_mean.ToString(CultureInfo.InvariantCulture),
                    label.arousal_mean.ToString(CultureInfo.InvariantCulture)
                ));
            }

            Console.WriteLine($"✅ Done. Features for {processed - 1} tracks in `{outputCsv}`");
        }
    }
}
