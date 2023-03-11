using GameSystems.TrackPlayback;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace SRXDCustomVisuals.Plugin; 

public class WaveformProcessor {
    private const int BUFFER_SIZE = 256;
    private const int SAMPLES_PER_INDEX = 8;
    private const int SAMPLE_RATE = 48000;
    private const long CHUNK_SIZE = 8192L;
    private const float APPROACH_RATE = 42f;
    
    private static readonly int WAVEFORM_CUSTOM = Shader.PropertyToID("_WaveformCustom");
    
    private ComputeBuffer waveformBuffer = new(BUFFER_SIZE, UnsafeUtility.SizeOf<float2>());
    private float2[] waveformArray = new float2[BUFFER_SIZE];
    
    public void AnalyzeWaveform(TrackPlaybackHandle playbackHandle) {
        const int waveformBufferSamples = BUFFER_SIZE * SAMPLES_PER_INDEX;
        const float scale = 2f / SAMPLES_PER_INDEX;
        
        long sampleAtTime = 2L * (long) (SAMPLE_RATE * playbackHandle.GetCurrentTime());
        var chunk = playbackHandle.OutputStream.GetLoadedFloatsForChunk(sampleAtTime / CHUNK_SIZE);
        float interp = 1f - Mathf.Exp(-APPROACH_RATE * Time.deltaTime);

        if (sampleAtTime < 0L || chunk.Length == 0) {
            for (int i = 0; i < BUFFER_SIZE; i++)
                waveformArray[i] = float2.zero;
        }
        else {
            for (int i = 0, startSample = (int) (sampleAtTime % CHUNK_SIZE / waveformBufferSamples * waveformBufferSamples);
                 i < BUFFER_SIZE; i++, startSample += SAMPLES_PER_INDEX) {
                var sum = float2.zero;
                int endSample = startSample + SAMPLES_PER_INDEX;

                if (endSample > chunk.Length)
                    endSample = chunk.Length;

                for (int j = startSample; j < endSample; j += 2)
                    sum += math.clamp(new float2(chunk[j], chunk[j + 1]), -1f, 1f);

                waveformArray[i] += interp * (scale * sum - waveformArray[i]);
            }
        }

        waveformBuffer.SetData(waveformArray);
        Shader.SetGlobalBuffer(WAVEFORM_CUSTOM, waveformBuffer);
    }
}