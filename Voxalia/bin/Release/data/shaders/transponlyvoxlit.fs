#version 430 core
// transponlyvoxlit.fs

#INCLUDE_STATEMENTS_HERE

layout (binding = 0) uniform sampler2DArray tex;
layout (binding = 1) uniform sampler2DArray htex;

layout (location = 4) uniform float desaturationAmount = 1.0;
layout (location = 5) uniform float minimum_light;
layout (location = 6) uniform mat4 shadow_matrix;
layout (location = 7) uniform vec3 light_color = vec3(1.0, 1.0, 1.0);
layout (location = 8) uniform mat4 light_details;
layout (location = 9) uniform mat4 light_details2;

layout (location = 0) in vec4 f_color;
layout (location = 1) in vec3 f_texcoord;
layout (location = 2) in vec3 f_normal;
layout (location = 3) in vec3 f_position;
layout (location = 4) in vec4 f_tcol;

out vec4 color;

// TOD: maybe apply ambient/diffuse/specular lighting?

vec3 desaturate(vec3 c)
{
	return mix(c, vec3(0.95, 0.77, 0.55) * dot(c, vec3(1.0)), desaturationAmount);
}

void main()
{
	vec4 tcolor = texture(tex, f_texcoord) * f_tcol;
	if (tcolor.w * f_color.w >= 0.99)
	{
		discard;
	}
	if (tcolor.w * f_color.w < 0.01)
	{
		discard;
	}
	color = tcolor * f_color; // TODO: Clamp f_color.xyz, match fbo_vox
	float light_radius = light_details[0][0];
	vec3 diffuse_albedo = vec3(light_details[0][1], light_details[0][2], light_details[0][3]);
	vec3 specular_albedo = vec3(light_details[1][0], light_details[1][1], light_details[1][2]);
	float light_type = light_details[1][3];
	float should_sqrt = light_details[2][0];
	float tex_size = light_details[2][1];
	float depth_jump = light_details[2][2];
	float lightc = light_details[2][3];
	vec4 bambient = (vec4(light_details[3][0], light_details[3][1], light_details[3][2], 1.0)
		+ vec4(minimum_light, minimum_light, minimum_light, 0.0)) / lightc;
	vec3 eye_pos = vec3(light_details2[0][0], light_details2[0][1], light_details2[0][2]);
	vec3 light_pos = vec3(light_details2[1][0], light_details2[1][1], light_details2[1][2]);
	vec4 f_spos = shadow_matrix * vec4(f_position, 1.0);
	vec3 N = normalize(-f_normal);
	vec3 light_path = light_pos - f_position;
	float light_length = length(light_path);
	float d = light_length / light_radius;
	float atten = clamp(1.0 - (d * d), 0.0, 1.0);
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
	if (fs.x < 0.0 || fs.x > 1.0
		|| fs.y < 0.0 || fs.y > 1.0
		|| fs.z < 0.0 || fs.z > 1.0)
	{
		color = vec4(0.0, 0.0, 0.0, color.w);
		return;
	}
	vec3 L = light_path / light_length;
	vec4 diffuse = vec4(max(dot(N, -L), 0.0) * diffuse_albedo, 1.0);
	vec3 specular = vec3(pow(max(dot(reflect(L, N), normalize(f_position - eye_pos)), 0.0), /* renderhint.y * 1000.0 */ 128.0) * specular_albedo * /* renderhint.x */ 0.0);
	color = vec4((bambient * color + (vec4(1.0) * atten * (diffuse * vec4(light_color, 1.0)) * color) +
		(vec4(min(specular, 1.0), 0.0) * vec4(light_color, 1.0) * atten)).xyz, color.w);
#ifdef MCM_GOOD_GRAPHICS
	color = vec4(desaturate(color.xyz), color.w); // TODO: Make available to all, not just good graphics only! Or a separate CVar!
#endif
}
