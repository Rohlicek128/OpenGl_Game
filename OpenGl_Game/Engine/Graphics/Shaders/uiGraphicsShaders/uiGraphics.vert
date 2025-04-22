#version 460 core

layout (location = 0) in vec4 vertex;

out vec2 vTexCoord;

uniform mat4 model;

void main(){
    vTexCoord = vertex.zw;
    gl_Position = vec4(vertex.x, vertex.y, 0.0, 1.0) * model;
}