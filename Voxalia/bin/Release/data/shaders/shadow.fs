#version 430 core

layout (binding = 0) uniform sampler2D s;

layout (location = 4) uniform float allow_transp = 0.0;

layout (location = 0) in vec4 f_pos;
layout (location = 1) in vec2 f_texcoord;
layout (location = 2) in vec4 f_color;

layout (location = 0) out float color;

void main()
{
	vec4 col = texture(s, f_texcoord) * f_color;
	if (col.w < 0.9 && ((col.w < 0.05) || (allow_transp <= 0.5)))
	{
		discard;
	}
	color = (f_pos.z) / f_pos.w;
}
