#version 430 core

layout (binding = 0) uniform sampler2D godraytex;

layout (location = 6) uniform float exposure = 1.0;

layout (location = 0) in vec2 f_texcoord;

out vec4 color;

const float MAX_WIDTH = 0.025;

const float INV_MAX_WIDTH = 1.0 / MAX_WIDTH;

const float WIDTH_JUMP = 0.005;

const float LEN_JUMP = 0.02;

void main()
{
	vec4 grinp = vec4(0.0);
	for (float l = 0.0; l < 1.0; l += LEN_JUMP)
	{
		for (float w = -MAX_WIDTH; w < MAX_WIDTH; w += WIDTH_JUMP)
		{
			vec4 gr_col = texture(godraytex, vec2(f_texcoord.x + w, l)) + texture(godraytex, vec2(l, f_texcoord.y + w));
			float mod = (0.5 - abs(l - 0.5)) * 2.0;
			float wmod = (MAX_WIDTH - abs(w)) * INV_MAX_WIDTH;
			grinp += gr_col * (mod * mod * wmod * wmod * wmod * wmod);
		}
	}
	color = grinp;
}
