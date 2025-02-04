#version 420 core

layout (location = 0) in vec4 aVertex;

out vec2 vTexCoord;

uniform vec2 viewport;
uniform mat4 model;

void main(){
    vTexCoord = aVertex.zw;
    gl_Position = vec4(aVertex.x, aVertex.y, 0.0, 1.0) * model;
    //gl_Position = vec4(aVertex.x / viewport.x * 2.0 - 1.0, aVertex.y / viewport.y * 2.0 - 1.0, 0.0, 1.0) * model;
}