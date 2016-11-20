#version 430 core

layout (binding = 0) uniform sampler2DArray tex;

layout (location = 0) in vec4 f_color;
layout (location = 1) in vec3 f_texcoord;

out vec4 color;

void main()
{
	vec4 tcolor = texture(tex, f_texcoord);
	color = vec4(f_color.x, f_color.y, f_color.z, ((tcolor.x + tcolor.y + tcolor.z) / 3) * f_color.w);
}
