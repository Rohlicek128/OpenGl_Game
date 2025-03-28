#version 420 core

out vec4 pCollision;

uniform float vecEoId;

void main(){
    pCollision = vec4(vecEoId, vecEoId, vecEoId, 1.0);
}