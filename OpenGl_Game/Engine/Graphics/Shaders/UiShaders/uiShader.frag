#version 420 core

in vec2 vTexCoord;

out vec4 pixelColor;

uniform vec4 color;

void main(){
    pixelColor = color;
}