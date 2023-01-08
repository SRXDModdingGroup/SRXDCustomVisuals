using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace SRXDCustomVisuals.Plugin; 

public class SpectrumBufferController {
    private static readonly int CUSTOM_SPECTRUM_BUFFER = Shader.PropertyToID("_CustomSpectrumBuffer");
    
    private static float Boost(float x) => 1f - 1f / (100f * x + 1f);
    
    private float2[] cachedSpectrum = new float2[256];
    private ComputeBuffer computeBuffer;

    public void Update() {
        var spectrumProcessor = TrackTrackerAndUtils.Instance.SpectrumProcessor;
        var smoothedBandsLeft = spectrumProcessor.GetSmoothedBands(AudioChannels.LeftChannel);
        var smoothedBandsRight = spectrumProcessor.GetSmoothedBands(AudioChannels.RightChannel);
        
        if (!smoothedBandsLeft.IsCreated || !smoothedBandsRight.IsCreated)
            return;

        for (int i = 0; i < 256; i++)
            cachedSpectrum[i] = new float2(Boost(smoothedBandsLeft[i]), Boost(smoothedBandsRight[i]));

        computeBuffer ??= new ComputeBuffer(256, UnsafeUtility.SizeOf<float2>(), ComputeBufferType.Structured);
        computeBuffer.SetData(cachedSpectrum);
        Shader.SetGlobalBuffer(CUSTOM_SPECTRUM_BUFFER, computeBuffer);
    }
}