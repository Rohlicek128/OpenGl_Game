#version 420 core

in vec2 vTexCoord;

out vec4 pixelColor;

uniform sampler2D text;
uniform vec3 textColor;

void main(){
    pixelColor = vec4(textColor, texture(text, vTexCoord).r);
}