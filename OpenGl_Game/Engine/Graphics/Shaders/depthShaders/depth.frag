#version 460 core

out float pixelColor;

void main(){
    pixelColor = gl_FragDepth;
}