#version 430 core

layout (binding = 0) uniform sampler2D godraytex;
layout (binding = 1) uniform sampler2D depthtex;

layout (location = 6) uniform float exposure = 1.0;
layout (location = 7) uniform float aspect = 1.0;
layout (location = 8) uniform vec2 sunloc = vec2(-10.0, -10.0);
layout (location = 9) uniform int numSamples = 75;
layout (location = 10) uniform float wexposure = 0.0034 * 5.65;
layout (location = 11) uniform float decay = 1.0;
layout (location = 12) uniform float density = 0.84;
layout (location = 13) uniform vec3 grcolor = vec3(1.0);
layout (location = 14) uniform float MIN_DEPTH = 0.1;
layout (location = 15) uniform float MAX_DEPTH = 3000.0;
layout (location = 16) uniform float SKY_DIST = 1700.0;

layout (location = 0) in vec2 f_texcoord;

float linearizeDepth(in float rinput)
{
	return (2.0 * MIN_DEPTH) / (MAX_DEPTH + MIN_DEPTH - rinput * (MAX_DEPTH - MIN_DEPTH));
}

out vec4 color;

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
	vec4 c = vec4(0.0);
	vec2 tcd = vec2(f_texcoord - sunloc);
	tcd *= (density * exposure) / float(numSamples);
	float illuminationDecay = 1.0;
	vec2 tc = f_texcoord;
	for (int i = 0; i < numSamples; i++)
	{
		tc -= tcd;
		if (tc.x < 0.0 || tc.y < 0.0 || tc.x > 1.0 || tc.y > 1.0)
		{
			break;
		}
		float depth = linearizeDepth(texture(depthtex, tc).x);
		if (depth < SKY_DIST / (MAX_DEPTH - MIN_DEPTH))
		{
			c += vec4(0.0, 0.0, 0.0, 1.0);
		}
		else
		{
			vec2 dist = tc - sunloc;
			dist.x *= aspect;
			if (dot(dist, dist) < 0.02)
			{
				c += vec4(1.0);
			}
		}
		illuminationDecay *= decay;
	}
	color = regularize(grinp + c * vec4(grcolor, 1.0) * wexposure);
}
