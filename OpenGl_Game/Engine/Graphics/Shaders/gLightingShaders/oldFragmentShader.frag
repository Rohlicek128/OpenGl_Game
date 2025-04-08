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
};
struct DirLight {
    vec3 direction;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
struct PointLight {
    vec3 position;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    vec3 attenParams;
};

in vec2 vTexCoord;
in vec3 vNormal;
in vec3 vFragPos;
in vec4 vFragPosLightSpace;
in mat3 vTBN;

layout (location = 0) out vec4 pixelColor;
layout (location = 1) out vec4 brightColor;

#define NR_POINT_LIGHTS 2

uniform DirLight dirLight;
uniform PointLight pointLight[NR_POINT_LIGHTS];
uniform Material material;

uniform vec3 viewPos;
uniform samplerCube skybox;
uniform sampler2D shadowMap;

vec3 CalcDirectionLight(DirLight light, vec3 norm, vec3 viewDir, vec3 materialColor, vec3 specMap);
vec3 CalcPointLight(PointLight light, vec3 norm, vec3 viewDir, vec3 fragPos, vec3 materialColor, vec3 specMap);
float CalcShadow(vec4 fragPosLight);

void main(){
    vec3 norm = texture(material.normalMap, vTexCoord).rgb;
    norm = norm * 2.0 - 1.0;
    norm = normalize(vTBN * norm) * material.hasNormalMap + normalize(vNormal) * (1 - material.hasNormalMap);
    
    vec3 viewDir = normalize(viewPos - vFragPos);

    vec3 materialColor = material.color * texture(material.diffuseMap, vTexCoord).rgb * material.diffuse;
    vec3 specMap = texture(material.specularMap, vTexCoord).rgb;

    vec3 result = CalcDirectionLight(dirLight, norm, viewDir, materialColor, specMap);
    for (int i = 0; i < NR_POINT_LIGHTS; i++) result += CalcPointLight(pointLight[i], norm, viewDir, vFragPos, materialColor, specMap);

    pixelColor = vec4(result, 1.0);
    brightColor = pixelColor;
}

vec3 CalcDirectionLight(DirLight light, vec3 norm, vec3 viewDir, vec3 materialColor, vec3 specMap){
    //Ambient
    vec3 ambient = light.ambient * materialColor;

    //Diffuse
    vec3 lightDir = normalize(-light.direction);
    vec3 diffuse = (max(dot(norm, lightDir), 0.0) * materialColor * material.diffuse) * light.diffuse;

    //Specular
    float spec = pow(max(dot(norm, normalize(lightDir + viewDir)), 0.0), material.shininess);
    vec3 specular = (spec * specMap * material.specular) * light.specular;
    
    //Shadows
    float shadow = CalcShadow(vFragPosLightSpace);
    //float shadow = 0.0;

    return (ambient + (diffuse + specular) * (1.0 - shadow));
}

vec3 CalcPointLight(PointLight light, vec3 norm, vec3 viewDir, vec3 fragPos, vec3 materialColor, vec3 specMap){
    //Ambient
    vec3 ambient = light.ambient * materialColor;

    //Diffuse
    vec3 lightDir = normalize(light.position - fragPos);
    vec3 diffuse = (max(dot(norm, lightDir), 0.0) * materialColor * material.diffuse) * light.diffuse;

    //Specular
    float spec = pow(max(dot(norm, normalize(lightDir + viewDir)), 0.0), material.shininess);
    vec3 specular = (spec * specMap * material.specular) * light.specular;

    //Attenuation
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.attenParams.x + light.attenParams.y * distance + light.attenParams.z * (distance * distance));
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;

    return (ambient + diffuse + specular);
}

float CalcShadow(vec4 fragPosLight){
    vec3 projectedCoords = fragPosLight.xyz / fragPosLight.w;
    projectedCoords = projectedCoords * 0.5 + 0.5;
    
    //float closestDepth = texture(shadowMap, projectedCoords.xy).r;
    float bias = max(0.003 * (1.0 - dot(vNormal, dirLight.direction)), 0.0003);
    
    float shadow;
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    for (int x = -1; x <= 1; ++x){
        for (int y = -1; y <= 1; ++y){
            float pcfDepth = texture(shadowMap, projectedCoords.xy + vec2(x, y) * texelSize).r;
            shadow += projectedCoords.z - bias > pcfDepth ? 1.0 : 0.0;
        }
    }
    shadow /= 9.0;
    
    if (projectedCoords.z > 1.0) shadow = 0.0;
    return shadow;
}