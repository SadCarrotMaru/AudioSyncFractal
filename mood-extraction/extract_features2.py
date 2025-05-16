import os
import numpy as np
import pandas as pd
import librosa


# ─── MATCH C# SETTINGS ──────────────────────────────────────────────────────────
FRAME_DURATION = 0.04    # 40 ms → 1764 samples @44100
HOP_DURATION   = 0.010   # 10 ms →  441 samples @44100
SAMPLE_RATE    = 44100   # 44.1 kHz

# CONFIG
labels_csv  = "annotations/per_song/song_level/first_20000.csv"
audio_dir   = "DEAM_audio/MEMD_audio"
output_csv  = "output/features_reduced2.csv"

df_labels = pd.read_csv(labels_csv, dtype={"song_id": str})
print(f"Found {len(df_labels)} rows")


def extract_9_features(y, sr):
    n_fft      = int(FRAME_DURATION * sr)    # = 0.04 * 44100 = 1764
    hop_length = int(HOP_DURATION   * sr)    # = 0.01 * 44100 = 441
    print(f"n_fft: {n_fft}  |  hop_length: {hop_length}, sr: {sr}")
    mfcc_mean     = np.mean(librosa.feature.mfcc(
                       y=y, sr=sr, n_mfcc=13,
                       n_fft=n_fft, hop_length=hop_length))

    chroma_mean   = np.mean(librosa.feature.chroma_stft(
                       y=y, sr=sr,
                       n_fft=n_fft, hop_length=hop_length))

    melspec_mean  = np.mean(librosa.feature.melspectrogram(
                       y=y, sr=sr, n_mels=128,
                       n_fft=n_fft, hop_length=hop_length))

    lpc_mean      = np.mean(librosa.lpc(y, order=12))

    pitches       = librosa.yin(
                       y, fmin=50, fmax=2000, sr=sr,
                       frame_length=n_fft, hop_length=hop_length)
    pitch_mean    = np.mean(pitches[np.isfinite(pitches)])

    centroid_mean = np.mean(librosa.feature.spectral_centroid(
                       y=y, sr=sr,
                       n_fft=n_fft, hop_length=hop_length))

    bandwidth_mean= np.mean(librosa.feature.spectral_bandwidth(
                       y=y, sr=sr,
                       n_fft=n_fft, hop_length=hop_length))

    contrast_mean = np.mean(librosa.feature.spectral_contrast(
                       y=y, sr=sr,
                       n_fft=n_fft, hop_length=hop_length))

    rolloff_mean  = np.mean(librosa.feature.spectral_rolloff(
                       y=y, sr=sr,
                       n_fft=n_fft, hop_length=hop_length))

    return {
        "mfcc_mean":      mfcc_mean,
        # "chroma_mean":    chroma_mean,
        # "melspec_mean":   melspec_mean,
        "lpc_mean":       lpc_mean,
        "pitch_mean":     pitch_mean,
        "centroid_mean":  centroid_mean,
        "bandwidth_mean": bandwidth_mean,
        # "contrast_mean":  contrast_mean,
        "rolloff_mean":   rolloff_mean
    }

FRAME_DURATION = 0.04
HOP_DURATION   = 0.010
SAMPLE_RATE    = 44100

n_fft = int(FRAME_DURATION * SAMPLE_RATE)    # 1764
hop_length = int(HOP_DURATION * SAMPLE_RATE) # 441

def extract_6_features(y, sr):
    # Use explicitly the Hamming window to match NWaves
    mfcc_mean = np.mean(librosa.feature.mfcc(
        y=y, sr=sr, n_mfcc=13, n_fft=n_fft, hop_length=hop_length, window='hamming'))

    lpc_mean = np.mean(librosa.lpc(y, order=12))

    pitches = librosa.yin(y, fmin=50, fmax=2000, sr=sr,
                          frame_length=n_fft, hop_length=hop_length)
    pitch_mean = np.mean(pitches[np.isfinite(pitches)])

    centroid_mean = np.mean(librosa.feature.spectral_centroid(
        y=y, sr=sr, n_fft=n_fft, hop_length=hop_length, window='hamming'))

    bandwidth_mean = np.mean(librosa.feature.spectral_bandwidth(
        y=y, sr=sr, n_fft=n_fft, hop_length=hop_length, window='hamming'))

    rolloff_mean = np.mean(librosa.feature.spectral_rolloff(
        y=y, sr=sr, n_fft=n_fft, hop_length=hop_length, window='hamming'))

    return {
        "mfcc_mean": mfcc_mean,
        "lpc_mean": lpc_mean,
        "pitch_mean": pitch_mean,
        "centroid_mean": centroid_mean,
        "bandwidth_mean": bandwidth_mean,
        "rolloff_mean": rolloff_mean
    }

i = 0
rows = []

for _, row in df_labels.iterrows():
    i += 1
    if i == 500:
        break
    sid = row["song_id"]
    # find file
    for ext in (".mp3", ".wav"):
        p = os.path.join(audio_dir, sid + ext)
        if os.path.isfile(p):
            y, sr = librosa.load(p, sr=None, mono=True)
            feats = extract_6_features(y, sr)
            print('feats', feats)
            feats["song_id"] = sid
            valence_mean = row[" valence_mean"]
            arousal_mean = row[" arousal_mean"]
            feats["valence_mean"] = valence_mean
            feats["arousal_mean"] = arousal_mean
            rows.append(feats)
            print(f"Processed {sid}")
            break

df_out = pd.DataFrame(rows)
df_out.to_csv(output_csv, index=False)
print(f"Wrote {len(df_out)} feature rows to {output_csv}")
