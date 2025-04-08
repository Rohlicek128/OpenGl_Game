#version 460 core

layout (location = 0) in vec3 aPosition;

out vec3 vTexCoord;

uniform mat4 world;

void main(){
    vTexCoord = aPosition;
    
    vec4 pos = vec4(aPosition, 1.0) * world;
    gl_Position = pos.xyww;
}