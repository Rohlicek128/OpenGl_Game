#version 460 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec3 aTangent;

out vec2 vTexCoord;
out vec3 vNormal;
out vec3 vNormalView;
out vec3 vFragPos;
out vec3 vFragPosView;
out mat3 vTBN;

uniform mat4 world;
uniform mat4 view;
uniform mat4 model;
uniform mat4 inverseModel;
uniform mat4 inverseModelView;
uniform float textureScaling;

void main(){
    vTexCoord = aTexCoord * (vec2(length(vec3(model[0])), length(vec3(model[1]))) * textureScaling + (1 - textureScaling));
    vNormal = aNormal * mat3(inverseModel);
    vNormalView = aNormal * mat3(inverseModelView);
    
    vec4 viewPos = vec4(aPosition, 1.0) * model;
    vFragPos = viewPos.xyz;
    vFragPosView = vec3(viewPos * view);
    
    vec3 T = normalize(vec3(vec4(aTangent, 0.0) * model));
    vec3 N = normalize(vec3(vec4(aNormal, 0.0) * model));
    vec3 B = cross(N, T);
    vTBN = mat3(T, B, N);

    gl_Position = viewPos * world;

    /*vec4 viewPos = view * model * vec4(aPosition, 1.0);
    vFragPos = viewPos.xyz;
    vTexCoord = aTexCoord;
    vNormal = aNormal * mat3(inverseModel);

    vec3 T = normalize(vec3(vec4(aTangent, 0.0) * model));
    vec3 N = normalize(vec3(vec4(aNormal, 0.0) * model));
    vec3 B = cross(N, T);
    vTBN = mat3(T, B, N);

    gl_Position = vec4(aPosition, 1.0) * model * view;*/
}