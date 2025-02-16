#version 420 core

in vec2 vTexCoord;

out vec4 pixelColor;

uniform sampler2D screenTexture;
uniform int banding;
uniform float grayscale;

void main(){
    vec3 color = texture(screenTexture, vTexCoord).rgb;
    //pixelColor = vec4(vec3(1.0) - exp(-color * grayscale), 1.0);
    //pixelColor = vec4(color, 1.0);
    
    //Grayscale
    float average = (color.r + color.g + color.b) / 3.0;
    pixelColor = vec4(average, average, average, 1.0) * grayscale;
    pixelColor += vec4(color * (1.0 - grayscale), 1.0);

    //Banding
    if (banding > 0) {
        vec4 screen = pixelColor;
        screen.r = floor(screen.r * banding) / banding;
        screen.g = floor(screen.g * banding) / banding;
        screen.b = floor(screen.b * banding) / banding;
        pixelColor = screen;
    }
}