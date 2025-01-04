#version 420 core

in vec2 vTexCoord;

out vec4 pixelColor;

uniform sampler2D screenTexture;

void main(){
    /*vec3 color = texture(screenTexture, vTexCoord).rgb;
    float average = (color.r + color.g + color.b) / 3.0;
    pixelColor = vec4(average, average, average, 1.0);*/
    //pixelColor = vec4(vec3(1.0 - texture(screenTexture, vTexCoord)), 1.0);
    //pixelColor = vec4(vec3(texture(screenTexture, vTexCoord).r), 1.0);

    /*vec4 screen = texture(screenTexture, vTexCoord);
    float gamma = 2.2;
    pixelColor = vec4(pow(screen.rgb, vec3(1.0/gamma)), screen.w);*/
    
    pixelColor = texture(screenTexture, vTexCoord);
}