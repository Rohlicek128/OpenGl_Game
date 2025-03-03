#version 420 core

in vec2 vTexCoord;

out vec4 pixelColor;

uniform vec4 color;
uniform vec3 radius;

void main(){
    pixelColor = vec4(color.rgb, distance(radius.xy, vTexCoord) < radius.z ? color.a : 0.0);
}