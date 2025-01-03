#version 420 core

struct Material {
    vec3 color;

    vec3 diffuse;
    vec3 specular;
    sampler2D diffuseMap;
    sampler2D specularMap;
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

out vec4 pixelColor;

#define NR_POINT_LIGHTS 2

uniform DirLight dirLight;
uniform PointLight pointLight[NR_POINT_LIGHTS];
uniform Material material;

uniform vec3 viewPos;
uniform samplerCube skybox;

vec3 CalcDirectionLight(DirLight light, vec3 norm, vec3 viewDir, vec3 materialColor, vec3 specMap);
vec3 CalcPointLight(PointLight light, vec3 norm, vec3 viewDir, vec3 fragPos, vec3 materialColor, vec3 specMap);

void main(){
    vec3 norm = normalize(vNormal);
    vec3 viewDir = normalize(viewPos - vFragPos);

    vec3 materialColor = material.color * texture(material.diffuseMap, vTexCoord).rgb * material.diffuse;
    vec3 specMap = texture(material.specularMap, vTexCoord).rgb;

    vec3 result = CalcDirectionLight(dirLight, norm, viewDir, materialColor, specMap);
    for (int i = 0; i < NR_POINT_LIGHTS; i++) result += CalcPointLight(pointLight[i], norm, viewDir, vFragPos, materialColor, specMap);

    pixelColor = vec4(result, 1.0);
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

    return (ambient + diffuse + specular);
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