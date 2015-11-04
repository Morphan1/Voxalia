#version 430 core

layout (binding = 0) uniform sampler2DArray tex;
layout (binding = 1) uniform sampler2DArray htex;

layout (location = 0) in vec4 f_color;
layout (location = 1) in vec3 f_texcoord;

out vec4 color;

// TOD: maybe apply ambient/diffuse/specular lighting?

void main()
{
	vec4 tcolor = texture(tex, f_texcoord);
	if (tcolor.w * f_color.w >= 0.99)
	{
		discard;
	}
	color = tcolor * f_color;
}
