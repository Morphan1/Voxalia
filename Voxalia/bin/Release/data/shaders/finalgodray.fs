#version 430 core

#INCLUDE_STATEMENTS_HERE

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
layout (location = 16) uniform float znear = 0.1;
layout (location = 17) uniform float zfar = 1000.0;
layout (location = 18) uniform vec4 fogCol = vec4(0.0);
layout (location = 19) uniform float desaturationAmount = 1.0;

layout (location = 20) uniform vec3 eye_position = vec3(0.0);
layout (location = 21) uniform float MIN_DEPTH = 1.0;
layout (location = 22) uniform mat4 proj_mat = mat4(1.0);
layout (location = 23) uniform float MAX_DEPTH = 1000.0;
layout (location = 24) uniform float WIDTH = 1280.0;
layout (location = 25) uniform float HEIGHT = 720.0;

layout (location = 0) out vec4 color;
layout (location = 1) out vec4 godray;

float linearizeDepth(float rinput)
{
	return (2 * MIN_DEPTH) / (MAX_DEPTH + MIN_DEPTH - rinput * (MAX_DEPTH - MIN_DEPTH));
}

vec4 raytrace(vec3 reflectionVector, float startDepth)
{
	float stepSize = 0.01; //rayStepSize;
	reflectionVector = normalize(reflectionVector) * stepSize;
	vec2 sampledPosition = f_texcoord;
	float currentDepth = startDepth;
	while(sampledPosition.x <= 1.0 && sampledPosition.x >= 0.0 && sampledPosition.y <= 1.0 && sampledPosition.y >= 0.0)
	{
		sampledPosition = sampledPosition + reflectionVector.xy;
		currentDepth = currentDepth + reflectionVector.z * startDepth;
		float sampledDepth = linearizeDepth(texture(depthtex, sampledPosition).r);
		if(currentDepth > sampledDepth)
		{
			float delta = (currentDepth - sampledDepth);
			if(delta < 0.03)
			{
				return texture(colortex, sampledPosition);
			}
		}
	}
	return vec4(0.0);
}

vec4 ssr()
{
	vec4 norm = texture(normaltex, f_texcoord);
	vec3 normal = normalize(norm.xyz);
	float currDepth = linearizeDepth(texture(depthtex, f_texcoord).r);
	vec3 pos = texture(positiontex, f_texcoord).xyz;
	vec3 eyePosition = normalize(eye_position - pos);
	vec4 reflectionVector = proj_mat * reflect(vec4(eyePosition, 0.0), vec4(normal, 0.0));
	return raytrace(reflectionVector.xyz / reflectionVector.w, currDepth);
}

vec4 regularize(vec4 input_r) // TODO: Is this working the best it can?
{
	if (input_r.x <= 1.0 && input_r.y <= 1.0 && input_r.z <= 1.0)
	{
		return input_r;
	}
	return vec4(input_r.xyz / max(max(input_r.x, input_r.y), input_r.z), input_r.w);
}

vec4 getGodRay()
{
	vec4 c = vec4(0.0);
	vec2 tcd = vec2(f_texcoord - lightPos);
	tcd *= density / float(numSamples);
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

vec3 desaturate(vec3 c)
{
	return mix(c, vec3(0.95, 0.77, 0.55) * dot(c, vec3(1.0)), desaturationAmount);
}

vec4 getColor(vec2 pos)
{
	vec4 shadow_light_color = texture(shtex, pos);
	vec4 colortex_color = texture(colortex, pos);
	vec4 renderhint = texture(renderhinttex, pos);
	return regularize(vec4(ambient + renderhint.z, 0.0) * colortex_color + shadow_light_color);
}

#define FXAA_SPAN_MAX 8.0
#define FXAA_REDUCE_MUL (1.0 / 8.0)
#define FXAA_REDUCE_MIN (1.0 / 128.0)

vec3 fxaaColor()
{
	float x_adj = 1.0 / WIDTH;
	float y_adj = 1.0 / HEIGHT;
	vec3 light_color = getColor(f_texcoord).xyz;
	vec3 light_colorxpyp = getColor(vec2(f_texcoord.x + x_adj, f_texcoord.y + y_adj)).xyz;
	vec3 light_colorxpym = getColor(vec2(f_texcoord.x + x_adj, f_texcoord.y - y_adj)).xyz;
	vec3 light_colorxmym = getColor(vec2(f_texcoord.x - x_adj, f_texcoord.y - y_adj)).xyz;
	vec3 light_colorxmyp = getColor(vec2(f_texcoord.x - x_adj, f_texcoord.y + y_adj)).xyz;
	vec3 lumaOr = vec3(0.299, 0.587, 0.114);
	float lumaxpyp = dot(light_colorxpyp, lumaOr);
	float lumaxpym = dot(light_colorxpym, lumaOr);
	float lumaxmym = dot(light_colorxmym, lumaOr);
	float lumaxmyp = dot(light_colorxmyp, lumaOr);
	float luma  = dot(light_color,  lumaOr);
	float lumaMin = min(luma, min(min(lumaxpyp, lumaxpym), min(lumaxmym, lumaxmyp)));
	float lumaMax = max(luma, max(max(lumaxpyp, lumaxpym), max(lumaxmym, lumaxmyp)));
	vec2 dir = vec2(-((lumaxpyp + lumaxpym) - (lumaxmym + lumaxmyp)), (lumaxpyp + lumaxmym) - (lumaxpym + lumaxmyp));
	float dirReduce = max((lumaxpyp + lumaxpym + lumaxmym + lumaxmyp) * (0.25 * FXAA_REDUCE_MUL), FXAA_REDUCE_MIN);
	float rcpDirMin = 1.0 / (min(abs(dir.x), abs(dir.y)) + dirReduce);
	dir = min(vec2( FXAA_SPAN_MAX,  FXAA_SPAN_MAX), max(vec2(-FXAA_SPAN_MAX, -FXAA_SPAN_MAX), dir * rcpDirMin));
	dir.x /= WIDTH;
	dir.y /= HEIGHT;
	vec3 rgbA = 0.5 * (getColor(f_texcoord + dir * (1.0 / 3.0 - 0.5)).xyz + getColor(f_texcoord + dir * (2.0 / 3.0 - 0.5)).xyz);
	vec3 rgbB = rgbA * 0.5 + 0.25 * (getColor(f_texcoord + dir * (-0.5)).xyz + getColor(f_texcoord + dir * 0.5).xyz);
	float lumaB = dot(rgbB, lumaOr);
	if((lumaB < lumaMin) || (lumaB > lumaMax))
	{
		return rgbA;
	}
	else
	{
		return rgbB;
	}
}

void main()
{
#ifdef MCM_GOOD_GRAPHICS
	vec4 light_color = getColor(f_texcoord);//vec4(fxaaColor(), 1.0);
	vec4 renderhint = texture(renderhinttex, f_texcoord);
	float dist = texture(depthtex, f_texcoord).r;
	godray = getGodRay() * vec4(grcolor, 1.0);
	color = vec4(mix(light_color.xyz, fogCol.xyz, 1.0 - exp(-dist * fogCol.w)), 1.0);
	if (renderhint.w > 0.0)
	{
		vec4 SSR = ssr();
		if (SSR.w > 0.0)
		{
			color = color * (1.0 - renderhint.w) + SSR * renderhint.w;
		}
	}
	if (texture(bwtex, f_texcoord).w > 0.01)
	{
		color = vec4(desaturate(color.xyz), 1.0);
		godray = vec4(desaturate(godray.xyz), godray.w);
	}
#else
	vec4 light_color = getColor(f_texcoord);
	color = vec4(light_color.xyz, 1.0);
	godray = vec4(0.0);
#endif
}
