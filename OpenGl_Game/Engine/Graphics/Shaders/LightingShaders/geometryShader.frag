#version 420 core

struct Material {
    vec3 color;

    vec3 diffuse;
    vec3 specular;
    sampler2D diffuseMap;
    sampler2D specularMap;
    int hasNormalMap;
    sampler2D normalMap;
    float shininess;
};

in vec2 vTexCoord;
in vec3 vNormal;
in vec3 vFragPos;
in mat3 vTBN;

layout (location = 0) out vec3 gPosition;
layout (location = 1) out vec3 gNormal;
layout (location = 2) out vec4 gAlbedoSpec;

uniform Material material;

void main(){
    //Position
    gPosition = vFragPos;
    
    //Normal
    vec3 norm = texture(material.normalMap, vTexCoord).rgb;
    norm = norm * 2.0 - 1.0;
    gNormal = normalize(vTBN * norm) * material.hasNormalMap + normalize(vNormal) * (1 - material.hasNormalMap);
    //gNormal = normalize(vNormal);
    
    //Albedo
    gAlbedoSpec.rgb = material.color * texture(material.diffuseMap, vTexCoord).rgb * material.diffuse;
    
    //Specular
    gAlbedoSpec.a = texture(material.specularMap, vTexCoord).r;
}