#version 460 core

out vec4 pixelColor;

uniform int isSelected;

void main(){
    pixelColor = vec4(isSelected);
}