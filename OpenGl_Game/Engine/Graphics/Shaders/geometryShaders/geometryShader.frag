#version 460 core

struct Material {
    vec3 color;

    vec3 diffuse;
    vec3 specular;
    sampler2D diffuseMap;
    sampler2D specularMap;
    int hasNormalMap;
    sampler2D normalMap;
    float shininess;
    
    sampler2D emissive;
    sampler2D overlay;
};

in vec2 vTexCoord;
in vec3 vNormal;
in vec3 vNormalView;
in vec3 vFragPos;
in vec3 vFragPosView;
in mat3 vTBN;

layout (location = 0) out vec4 gPosition;
layout (location = 1) out vec3 gNormal;
layout (location = 2) out vec4 gAlbedoSpec;
layout (location = 3) out vec3 gViewPosition;
layout (location = 5) out vec3 gViewNormal;
layout (location = 4) out vec4 gTexCoords;

uniform Material material;
uniform mat4 inverseModelView;

void main(){
    //Position
    gPosition = vec4(vFragPos, texture(material.emissive, vTexCoord).r);
    gViewPosition = vFragPosView;
    
    //Normal
    vec3 norm = texture(material.normalMap, vTexCoord).rgb;
    norm = norm * 2.0 - 1.0;
    if (material.hasNormalMap == 0){
        gNormal = normalize(vNormal);
    }
    else if (material.hasNormalMap == 1) {
        gNormal = normalize(vTBN * norm);
    }
    gViewNormal = normalize((vTBN * norm) * mat3(inverseModelView)) * material.hasNormalMap + normalize(vNormalView) * (1 - material.hasNormalMap);
    
    //Albedo
    gAlbedoSpec.rgb = material.color * texture(material.diffuseMap, vTexCoord).rgb * material.diffuse + texture(material.overlay, vTexCoord).rbg * vec3(1.0, 0.84, 0.61);
    
    //Specular
    gAlbedoSpec.a = texture(material.specularMap, vTexCoord).r;
    
    //UV Coords
    gTexCoords.rgba = vec4(vTexCoord, 0.0, 1.0);
}