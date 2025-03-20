#version 420 core

out vec4 pixelColor;

uniform float vecEoId;

void main(){
    pixelColor = vec4(vecEoId, vecEoId, vecEoId, 1.0);
}