#version 430 core

layout (binding = 0) uniform sampler2D tex;

layout (location = 0) in vec4 f_color;
layout (location = 1) in vec3 f_texcoord;

out vec4 color;

void main()
{
	vec4 tcolor = texture(tex, vec2(f_texcoord.x, f_texcoord.y));
	if (tcolor.w * f_color.w >= 0.9)
	{
		discard;
	}
	color = tcolor * f_color;
}
