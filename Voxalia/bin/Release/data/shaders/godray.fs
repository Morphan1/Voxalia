#version 430 core

layout (binding = 0) uniform sampler2D godraytex;

layout (location = 6) uniform float exposure = 1.0;
layout (location = 7) uniform float aspect = 1.0;

layout (location = 0) in vec2 f_texcoord;

out vec4 color;

vec4 regularize(in vec4 input_r) // TODO: Is this working the best it can?
{
	if (input_r.x <= 1.0 && input_r.y <= 1.0 && input_r.z <= 1.0)
	{
		return input_r;
	}
	return vec4(input_r.xyz / max(max(input_r.x, input_r.y), input_r.z), input_r.w);
}

// TODO: Calculate old godrays in here too?

void main()
{
	float fmax = 0.25 * exposure;
	float fmax_inv = 1.0 / fmax;
	vec4 grinp = vec4(0.0);
	for (float l = -fmax; l < fmax; l += 0.005)
	{
		float mult = (fmax - abs(l)) * fmax_inv;
		grinp += texture(godraytex, vec2(f_texcoord.x, f_texcoord.y + l * aspect)) * mult;
		grinp += texture(godraytex, vec2(f_texcoord.x + l, f_texcoord.y)) * mult;
		grinp += texture(godraytex, vec2(f_texcoord.x + l, f_texcoord.y + l * aspect)) * mult;
		grinp += texture(godraytex, vec2(f_texcoord.x + l, f_texcoord.y - l * aspect)) * mult;
	}
	color = regularize(grinp);
}
