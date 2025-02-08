#version 420 core

in vec2 vTexCoord;

out vec4 pixelColor;

uniform sampler2D screenTexture;
uniform int banding;
uniform float grayscale;
uniform float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);

void main(){
    //pixelColor = vec4(vec3(1.0 - texture(screenTexture, vTexCoord)), 1.0);
    //pixelColor = vec4(vec3(texture(screenTexture, vTexCoord).r), 1.0);
    /*vec4 screen = texture(screenTexture, vTexCoord);
    float gamma = 2.2;
    pixelColor = vec4(pow(screen.rgb, vec3(1.0/gamma)), screen.w);*/
    
    //Gaussian
    /*vec4 brightColor;
    if (dot(color.rgb, vec3(0.2126, 0.7152, 0.0722)) > 1.0) brightColor = vec4(color.rgb, 1.0);
    else brightColor = vec4(0.0, 0.0, 0.0, 1.0);
    
    vec2 tex_offset = 1.0 / textureSize(image, 0);
    vec3 result = brightColor.rgb * weight[0];
    for(int i = 1; i < 5; ++i)
    {
        result += texture(screenTexture, TexCoords + vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
        result += texture(screenTexture, TexCoords - vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
    }
    color = vec4(result, 1.0);*/

    vec3 color = texture(screenTexture, vTexCoord).rgb;
    //Grayscale
    float average = (color.r + color.g + color.b) / 3.0;
    pixelColor = vec4(average, average, average, 1.0) * grayscale;
    pixelColor += vec4(color * (1.0 - grayscale), 1.0);

    //Banding
    vec4 screen = pixelColor;
    screen.r = floor(screen.r * banding) / banding;
    screen.g = floor(screen.g * banding) / banding;
    screen.b = floor(screen.b * banding) / banding;

    pixelColor = screen;
}