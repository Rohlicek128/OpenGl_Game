#version 460 core

struct Material {
    vec3 color;
};

in vec3 vFragPos;

out vec4 pixelColor;

uniform Material material;
uniform vec3 viewPos;

void main(){
    pixelColor = vec4(material.color, 1.0);
}