#version 430 core

layout (binding = 0) uniform sampler2DArray s;

layout (location = 0) in vec4 f_pos;
layout (location = 1) in vec3 f_texcoord;

out float color;

void main()
{
	vec4 col = texture(s, f_texcoord); // TODO: f_color?
	if (col.w < 0.9)
	{
		discard;
	}
	color = f_pos.z / f_pos.w;
}
