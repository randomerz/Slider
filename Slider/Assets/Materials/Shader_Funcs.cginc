#ifndef LAVA_WORLD
#define LAVA_WORLD

void ApplyNoise_float(float input, float noise, float noiseScale, float noiseBlend, out float output)
{
	float wetInput = input + (noise - 0.5) * noiseScale;
	output = lerp(input, wetInput, noiseBlend);
}

void SinX_float(float x, float frequency, float amplitude, float phase, out float output)
{
	const float tau = 6.28318530718f;
	output = amplitude * sin(tau * frequency*x + phase);
}

#endif