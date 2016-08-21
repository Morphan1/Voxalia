#version 430 core

layout (binding = 0) uniform sampler2D godraytex;

layout (location = 6) uniform float exposure = 1.0;

layout (location = 0) in vec2 f_texcoord;

out vec4 color;

const float PI = 3.1415926;

const float PI_TWO = PI * 2.0;

const float MAX_LEN = 1.0;

const float MAX_WIDTH = 0.1;

const float LEN_JUMP = 0.01;

void main()
{
	vec4 grinp = vec4(0.0);
	vec2 jump = normalize(vec2(f_texcoord.x - 0.5, f_texcoord.y - 0.5));
	vec2 spoint = f_texcoord - jump * 0.5;
	for (float l = 0.0; l < MAX_LEN; l += LEN_JUMP)
	{
		float bx = spoint.x + jump.x * l;
		float by = spoint.y + jump.y * l;
		if (bx < 0.0 || by < 0.0 || bx > 1.0 || by > 1.0)
		{
			continue;
		}
		vec4 gr_col = texture(godraytex, vec2(bx, by));
		float mod = (0.5 - abs(l - 0.5)) * 2.0;
		grinp += gr_col * mod * mod;
	}
	color = grinp;
}
