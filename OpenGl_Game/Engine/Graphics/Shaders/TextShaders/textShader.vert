#version 420 core

layout (location = 0) in vec4 vertex;

out vec2 vTexCoord;

uniform vec2 viewport;

void main(){
    vTexCoord = vertex.zw;
    
    gl_Position = vec4(vertex.x / viewport.x * 2.0 - 1.0, vertex.y / viewport.y * 2.0 - 1.0, 0.0, 1.0);
}