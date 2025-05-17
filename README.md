# Audio Fractal Simulation

We present a real-time simulation in Unity that
synchronizes music playback with the rendering
of 3D fractals via ray-marching. Additionally, our simulation includes a system of colliding spheres and a mood detection neural network that suggests scene colors based on the song’s characteristics.
Our pipeline consists of:
- audio analysis (FFT bands)
- shader-based fractal rendering using signed
distance functions (SDFs) and ray-marching
- physics-driven spheres reacting to the audio
- valence/arousal estimation via a PyTorch neural network imported through Unity.Sentis

## Features
- **Fractal Rendering:** Real-time shader-based rendering of 3D fractals via signed distance functions and raymarching.
- **Audio-Driven Dynamics:** FFT-based audio analysis feeds both fractal visuals and a dynamic sphere system that reacts to music intensity.
- **Mood Detection:** PyTorch-trained MLP predicts valence and arousal from audio features; used to suggest scene color mappings in HSV space.
- **Multiple Camera Modes:** User-controlled, free-wandering, and spectator camera views.
- **Modular & Customizable:** UI controls for fractal speed, base color, and glow color.

## System Architecture

1. **Audio Analysis**  
   - Extracts amplitude and spectral roll-off in real-time using Unity's `GetSpectrumData`.

2. **Fractal Rendering**  
   - Shader-driven raymarching with support for Kaleidoscopic IFS, Hartverdrahtet, Knightyan, and other fractal types.

3. **Sphere Physics**  
   - A dynamic system of spheres with audio-reactive velocity, bouncing inside the fractal cube using Unity colliders.

4. **Mood Classification Pipeline**  
   - Seven spectral features (MFCC, pitch, centroid, etc.) → PyTorch MLP → ONNX inference in Unity via Sentis.

## Neural Network Details

- **Architecture:** `7 → 128 → 64 → 2` MLP with ReLU and dropout  
- **Training Dataset:** DEAM (500 labeled tracks)  
- **Export:** Trained in PyTorch, exported to ONNX  
- **Inference:** Asynchronous feature extraction and synchronous model evaluation in Unity