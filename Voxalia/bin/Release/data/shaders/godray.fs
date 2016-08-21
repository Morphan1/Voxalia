#version 430 core

layout (binding = 0) uniform sampler2D godraytex;

layout (location = 6) uniform float exposure = 1.0;

layout (location = 0) in vec2 f_texcoord;

out vec4 color;

const float MAX_WIDTH = 0.025;

const float INV_MAX_WIDTH = 1.0 / MAX_WIDTH;

const float WIDTH_JUMP = 0.0025;

const float LEN_JUMP = 0.02;

void main()
{
	vec4 grinp = vec4(0.0);
	float exp_inv = 1.0 / exposure;
	float m_w = MAX_WIDTH * exposure;
	for (float l = 0.0; l < 1.0; l += LEN_JUMP)
	{
		for (float w = -m_w; w < m_w; w += WIDTH_JUMP)
		{
			vec4 gr_col = texture(godraytex, vec2(f_texcoord.x + w, l)) + texture(godraytex, vec2(l, f_texcoord.y + w));
			if (gr_col.w > 0.0)
			{
				float mod = (0.5 - abs(l - 0.5)) * 2.0;
				float wmod = (MAX_WIDTH - abs(w)) * INV_MAX_WIDTH;
				grinp += gr_col * pow(mod * mod * wmod * wmod, exp_inv);
			}
		}
	}
	color = grinp;
}
