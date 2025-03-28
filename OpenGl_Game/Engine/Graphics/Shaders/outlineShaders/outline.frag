#version 420 core

in vec2 vTexCoord;

out vec4 pixelColor;

uniform sampler2D silhouetteTexture;

void main(){
    // if the pixel is black (we are on the silhouette)
    if (texture(silhouetteTexture, vTexCoord).xyz == vec3(0.0f))
    {
        vec2 size = 1.0f / textureSize(silhouetteTexture, 0);
        int border = 4;
        
        for (int i = -border; i <= border; i++)
        {
            for (int j = -border; j <= border; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                vec2 offset = vec2(i, j) * size;

                // and if one of the neighboring pixels is white (we are on the border)
                if (texture(silhouetteTexture, vTexCoord + offset).xyz == vec3(1.0f))
                {
                    pixelColor = vec4(1.0f);
                    return;
                }
            }
        }
    }

    discard;
    
    /*if (texture(silhouetteTexture, vTexCoord).r == 1.0){
        int size = 1;
        for (int x = -size; x < size; x++){
            for (int y = -size; y < size; y++){
                if (texture(silhouetteTexture, vTexCoord + vec2(x, y)).r == 1.0){
                    pixelColor = vec4(1.0);
                    return;
                }
            }
        }
    }
    pixelColor = vec4(0.0);*/
}