#version 430 core

#INCLUDE_STATEMENTS_HERE

layout (binding = 0) uniform sampler2DArray tex;
layout (binding = 1) uniform sampler2DArray htex;

const vec3 diffuse_albedo = vec3(0.7, 0.7, 0.7);
const float specular_albedo = 0.7;
const float light_type = 0.0;

layout (location = 4) uniform float desaturationAmount = 1.0;
layout (location = 5) uniform vec3 light_pos;
layout (location = 6) uniform mat4 shadow_matrix;
layout (location = 7) uniform vec3 light_color = vec3(1.0, 1.0, 1.0);
layout (location = 8) uniform float light_radius = 30.0;
layout (location = 9) uniform vec3 eye_pos = vec3(0.0, 0.0, 0.0);

layout (location = 0) in vec4 f_color;
layout (location = 1) in vec3 f_texcoord;
layout (location = 2) in vec3 f_normal;
layout (location = 3) in vec3 f_position;

out vec4 color;

// TOD: maybe apply ambient/diffuse/specular lighting?

vec3 desaturate(vec3 c)
{
	return mix(c, vec3(0.95, 0.77, 0.55) * dot(c, vec3(1.0)), desaturationAmount);
}

void main()
{
	vec4 tcolor = texture(tex, f_texcoord);
	if (tcolor.w * f_color.w >= 0.99)
	{
		discard;
	}
    if (tcolor.w * f_color.w < 0.01)
    {
        discard;
    }
	color = tcolor * f_color; // TODO: Clamp f_color.xyz, match fbo_vox
	vec4 f_spos = shadow_matrix * vec4(f_position, 1.0);
	vec3 N = normalize(-f_normal);
	vec3 light_path = light_pos - f_position;
	float light_length = length(light_path);
	float atten;
	if (light_length == 0.0)
	{
		light_length = 0.00001;
	}
	if (light_radius == 0.0)
	{
		atten = 1.0;
	}
	else
	{
		float d = light_length / light_radius;
		atten = clamp(1.0 - (d * d), 0.0, 1.0);
	}
	if (light_type == 1.0)
	{
		vec4 fst = f_spos / f_spos.w;
		atten *= 1 - (fst.x * fst.x + fst.y * fst.y);
        if (atten < 0)
        {
			atten = 0;
		}
	}
	vec4 fs = f_spos / f_spos.w / 2.0 + vec4(0.5, 0.5, 0.5, 0.0);
	fs.w = 1.0;
	float depth;
	if (fs.x < 0.0 || fs.x > 1.0
		|| fs.y < 0.0 || fs.y > 1.0
		|| fs.z < 0.0 || fs.z > 1.0)
	{
		depth = 0.0;
	}
	else
	{
		depth = 1.0;
	}
	vec3 L = light_path / light_length;
	vec3 V_Base = f_position - eye_pos;
	float V_Len = length(V_Base);
	vec3 V = V_Base / V_Len;
	vec3 R = reflect(L, N);
	vec4 diffuse = vec4(max(dot(N, -L), 0.0) * diffuse_albedo, 1.0);
	vec3 specular = vec3(pow(max(dot(R, V), 0.0), /* renderhint.y * 1000.0 */ 128.0) * specular_albedo * /* renderhint.x */ 0.0);
	color = vec4(((vec4(depth, depth, depth, 1.0) * atten * (diffuse * vec4(light_color, 1.0)) * color) +
		(vec4(min(specular, 1.0), 0.0) * vec4(light_color, 1.0) * atten * depth)).xyz, color.w);
#ifdef MCM_GOOD_GRAPHICS
    color = vec4(desaturate(color.xyz), color.w); // TODO: Make available to all, not just good graphics only! Or a separate CVar!
#endif
}
