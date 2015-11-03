#version 430 core

layout (binding = 0) uniform sampler2D colortex;
layout (binding = 1) uniform sampler2D positiontex;
layout (binding = 2) uniform sampler2D normaltex;
layout (binding = 3) uniform sampler2D depthtex;
layout (binding = 4) uniform sampler2D shtex;
layout (binding = 5) uniform sampler2D renderhinttex;
layout (binding = 6) uniform sampler2D bwtex;

layout (location = 0) in vec2 f_texcoord;

layout (location = 5) uniform vec3 ambient = vec3(0.05, 0.05, 0.05);
// ...
layout (location = 8) uniform vec3 cameraTargetPos = vec3(0.0, 0.0, 0.0);
layout (location = 9) uniform float cameraTargetDepth = 0.01;
layout (location = 10) uniform vec2 lightPos = vec2(0.5);
layout (location = 11) uniform int numSamples = 75;
layout (location = 12) uniform float wexposure = 0.0034 * 5.65;
layout (location = 13) uniform float decay = 1;
layout (location = 14) uniform float density = 0.84;
layout (location = 15) uniform vec3 grcolor = vec3(1.0);

out vec4 color;

vec4 regularize(vec4 input_r) // TODO: Is this working the best it can?
{
	if (input_r.x <= 1.0 && input_r.y <= 1.0 && input_r.z <= 1.0)
	{
		return input_r;
	}
	return vec4(input_r.xyz / ((input_r.x >= input_r.y && input_r.x >= input_r.z) ? input_r.x: ((input_r.y >= input_r.z) ? input_r.y: input_r.z)), input_r.w);
}

vec4 getGodRay()
{
    vec4 c = vec4(0.0);
    vec2 tcd = vec2(f_texcoord - lightPos);
    tcd *= 1.0 / float(numSamples) * density;
    float illuminationDecay = 1.0;
    vec2 tc = f_texcoord;
    for (int i = 0; i < numSamples; i++)
    {
        tc -= tcd;
        c += texture2D(bwtex, tc) * illuminationDecay;
        illuminationDecay *= decay;
    }
    return c * wexposure;
}

void main()
{
	vec4 shadow_light_color = texture(shtex, f_texcoord);
	vec4 colortex_color = texture(colortex, f_texcoord);
	vec4 renderhint = texture(renderhinttex, f_texcoord);
	vec4 light_color = regularize(vec4(ambient + renderhint.z, 0.0) * colortex_color + shadow_light_color);
	light_color.w = 1.0;
    vec4 godRay = getGodRay();
	color = (light_color.w * light_color) + godRay * vec4(grcolor, 1.0);
}
