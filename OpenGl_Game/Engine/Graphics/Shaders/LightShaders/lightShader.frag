#version 460 core

struct Material {
    vec3 color;
};

out vec4 pixelColor;

uniform Material material;

void main(){
    pixelColor = vec4(material.color, 1.0);
}