#version 420 core

in vec2 vTexCoord;

out vec4 pixelColor;

uniform sampler2D text;
uniform vec4 textColor;

void main(){
    pixelColor = vec4(textColor.rgb, texture(text, vTexCoord).r * textColor.a);
}