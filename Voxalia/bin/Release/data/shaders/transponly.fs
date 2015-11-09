#version 430 core

layout (binding = 0) uniform sampler2D tex;

layout (location = 0) in vec4 f_color;
layout (location = 1) in vec2 f_texcoord;

out vec4 color;

// TOD: maybe apply ambient/diffuse/specular lighting?

void main()
{
	vec4 tcolor = texture(tex, f_texcoord);
	if (tcolor.w * f_color.w >= 0.99)
	{
		discard;
	}
    if (tcolor.w * f_color.w < 0.01)
    {
        discard;
    }
	color = tcolor * f_color;
}
