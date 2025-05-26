#version 460 core

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
    bool isLighting;
};

in vec2 vTexCoord;

out vec4 pixelColor;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gAlbedoSpec;

#define NR_POINT_LIGHTS 2

uniform DirLight dirLight;
uniform PointLight pointLight[NR_POINT_LIGHTS];

uniform vec3 viewPos;
uniform mat4 lightSpace;
uniform sampler2D shadowMap;
uniform sampler2D ssaoMap;

float CalcShadow(vec4 fragPosLight, vec3 normal){
    vec3 projectedCoords = fragPosLight.xyz / fragPosLight.w;
    projectedCoords = projectedCoords * 0.5 + 0.5;

    //float closestDepth = texture(shadowMap, projectedCoords.xy).r;
    float bias = max(0.001 * (1.0 - dot(normal, dirLight.direction)), 0.0001);

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

vec3 CalcDirectionLight(DirLight light, vec3 normal, vec3 viewDir, vec3 materialColor, float specMap, vec4 fragPosLight, float emisive){
    //Ambient
    vec3 ambient = light.ambient * materialColor;

    //Diffuse
    vec3 lightDir = normalize(-light.direction);
    vec3 diffuse = (max(dot(normal, lightDir), 0.0) * materialColor) * light.diffuse;

    //Specular
    float spec = pow(max(dot(normal, normalize(lightDir + viewDir)), 0.0), 32.0);
    vec3 specular = (spec * specMap) * light.specular;

    //Shadows
    float shadow = CalcShadow(fragPosLight, normal);
    
    //Shadows + Emisive
    diffuse *= 1.0 - shadow * (1.0 - emisive);
    specular *= 1.0 - shadow;

    return ambient + diffuse + specular;
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir, vec3 fragPos, vec3 materialColor, float specMap, float emisive){
    //Ambient
    vec3 ambient = light.ambient * materialColor * emisive;

    //Diffuse
    vec3 lightDir = normalize(light.position - fragPos);
    vec3 diffuse = (max(dot(normal, lightDir), 0.0) * materialColor) * light.diffuse;

    //Specular
    float spec = pow(max(dot(normal, normalize(lightDir + viewDir)), 0.0), 32.0);
    vec3 specular = (spec * specMap) * light.specular;

    //Attenuation
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.attenParams.x + light.attenParams.y * distance + light.attenParams.z * (distance * distance));
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;

    //Emisive
    vec3 emi = materialColor * emisive;

    return (ambient + diffuse + specular) * (1 - emisive) + emi;
}

void main(){
    vec4 posEm = texture(gPosition, vTexCoord).rgba;
    vec3 fragPos = posEm.xyz;
    float emisive = posEm.a;
    
    vec3 normal = texture(gNormal, vTexCoord).rgb;
    
    vec4 albedoSpec = texture(gAlbedoSpec, vTexCoord);
    vec3 albedo = albedoSpec.rgb;
    float specular = albedoSpec.a;
    //float ambientOcclusion = texture(ssaoMap, vTexCoord).r;

    vec3 viewDir = normalize(viewPos - fragPos);
    vec4 fragPosLightSpace = vec4(fragPos, 1.0) * lightSpace;
    
    vec3 result = CalcDirectionLight(dirLight, normal, viewDir, albedo, specular, fragPosLightSpace, emisive);
    for (int i = 0; i < NR_POINT_LIGHTS; i++){
        if (pointLight[i].isLighting) result += CalcPointLight(pointLight[i], normal, viewDir, fragPos, albedo, specular, emisive);
    }

    //pixelColor = vec4(result * ambientOcclusion, 1.0);
    pixelColor = vec4(result, 1.0);
}