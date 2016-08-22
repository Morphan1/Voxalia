#version 430 core

layout (binding = 0) uniform sampler2D godraytex;

layout (location = 6) uniform float exposure = 1.0;

layout (location = 0) in vec2 f_texcoord;

out vec4 color;

const float PI = 3.1415926;

const float PI_TWO = PI * 2.0;

vec4 regularize(in vec4 input_r) // TODO: Is this working the best it can?
{
	if (input_r.x <= 1.0 && input_r.y <= 1.0 && input_r.z <= 1.0)
	{
		return input_r;
	}
	return vec4(input_r.xyz / max(max(input_r.x, input_r.y), input_r.z), input_r.w);
}

void main()
{
	float fmax = 0.3 * exposure;
	vec4 grinp = vec4(0.0);
	for (float l = 0.025; l < fmax; l += 0.025)
	{
		float adder = 0.025 / l;
		float multip = (fmax - l) / fmax;
		for (float a = 0.0; a < PI_TWO; a += adder)
		{
			vec4 gr_col = texture(godraytex, vec2(f_texcoord.x + cos(a) * l, f_texcoord.y + sin(a) * l)) * multip;
			grinp += gr_col;
		}
	}
	color = grinp;
}
