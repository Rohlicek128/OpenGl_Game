#version 460 core

struct Material {
    vec3 color;
    sampler2D diffuseMap;
};

in vec2 vTexCoord;

out vec4 pixelColor;

uniform vec2 viewport;
uniform Material material;

void main(){
    //vec2 pixel = fract(viewport * vTexCoord / 30.0) * 30.0;
    //pixelColor = vec4(vTexCoord - fract(vTexCoord * viewport / 3.0), 0.0, 1.0);
    
    pixelColor = vec4(material.color, 1.0) * texture(material.diffuseMap, vTexCoord).rgba;
}