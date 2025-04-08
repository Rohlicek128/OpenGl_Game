#version 460 core

in vec3 vTexCoord;

out vec4 pixelColor;

uniform samplerCube skybox;

void main(){
    pixelColor = texture(skybox, vTexCoord);
}