#version 460 core

layout (location = 0) in vec4 vertex;

void main(){
    gl_Position = vec4(vertex.x, vertex.y, 0.0, 1.0);
}