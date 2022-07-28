/* Goodbye custom HLSL function
void IsPosLit_float(float2 position, Texture2D lightMask, out bool output) {
    output = false;
    for (int lIndex = 0; lIndex < 3; lIndex++) {
        if (_LightActive[lIndex] > 0.5f) {
            float4 pos = _LightPos[lIndex];
            float radius = _LightRadius[lIndex];

            output = length(pos.xy - position) < radius;
            if (output) {
                break;
            }
        }
    }
}
*/

void GetShadowedPixel_float(float4 inColor, out float4 outColor) {
    if (inColor.r == inColor.g && inColor.g == inColor.b) {
        if (inColor.r > 0.75) {
            outColor = float4(0.6, 0.6, 0.6, inColor.a);
        }
        else if (inColor.r > 0.5) {
            outColor = float4(0.4, 0.4, 0.4, inColor.a);
        }
        else {
            outColor = float4(0.101960784, 0.101960784, 0.101960784, inColor.a);
        }
    }
    else {  //Leave non-greyscale pixels alone.
        outColor = inColor;
    }
}
