#version 420 core

in vec2 vTexCoord;

out float pixelColor;

uniform sampler2D ssao;
uniform int blurSize;

void main(){
    vec2 texelSize = 1.0 / vec2(textureSize(ssao, 0));
    float result = 0.0;
    
    for (int x = -blurSize; x < blurSize; ++x)
    {
        for (int y = -blurSize; y < blurSize; ++y)
        {
            vec2 offset = vec2(float(x), float(y)) * texelSize;
            result += texture(ssao, vTexCoord + offset).r;
        }
    }
    pixelColor = result / (blurSize * blurSize * 4.0);
}