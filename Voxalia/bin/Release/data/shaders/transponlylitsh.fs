#version 430 core
// transponlylitsh.fs

#INCLUDE_STATEMENTS_HERE

layout (binding = 0) uniform sampler2D tex;
layout (binding = 2) uniform sampler2DShadow shadowtex;

layout (location = 4) uniform float desaturationAmount = 1.0;
layout (location = 5) uniform float minimum_light;
layout (location = 6) uniform mat4 shadow_matrix;
layout (location = 7) uniform vec3 light_color = vec3(1.0, 1.0, 1.0);
layout (location = 8) uniform mat4 light_details;
layout (location = 9) uniform mat4 light_details2;

layout (location = 0) in vec4 f_color;
layout (location = 1) in vec2 f_texcoord;
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
	color = tcolor * f_color;
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
	if (should_sqrt >= 1.0)
	{
		f_spos.x = sign(f_spos.x) * sqrt(abs(f_spos.x));
		f_spos.y = sign(f_spos.y) * sqrt(abs(f_spos.y));
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
#ifdef MCM_GOOD_GRAPHICS
	vec2 dz_duv;
	vec3 duvdist_dx = dFdx(fs.xyz);
	vec3 duvdist_dy = dFdy(fs.xyz);
	dz_duv.x = duvdist_dy.y * duvdist_dx.z - duvdist_dx.y * duvdist_dy.z;
	dz_duv.y = duvdist_dx.x * duvdist_dy.z - duvdist_dy.x * duvdist_dx.z;
	float tlen = (duvdist_dx.x * duvdist_dy.y) - (duvdist_dx.y * duvdist_dy.x);
	dz_duv /= tlen;
	float oneoverdj = 1.0 / depth_jump;
	float jump = tex_size * depth_jump;
	float depth = 0.0;
	float depth_count = 0.0;
	// TODO: Make me more efficient
	for (float x = -oneoverdj * 2; x < oneoverdj * 2 + 1; x++)
	{
		for (float y = -oneoverdj * 2; y < oneoverdj * 2 + 1; y++)
		{
			float offz = dot(dz_duv, vec2(x * jump, y * jump)) * 1000.0;
			if (offz > -0.000001)
			{
				offz = -0.000001;
			}
			offz -= 0.001;
			depth += textureProj(shadowtex, fs + vec4(x * jump, y * jump, offz, 0.0));
			depth_count++;
		}
	}
	depth = depth / depth_count;
#else
		float depth = textureProj(shadowtex, fs - vec4(0.0, 0.0, 0.0001, 0.0));
#endif
	vec3 L = light_path / light_length;
	vec3 V_Base = f_position - eye_pos;
	float V_Len = length(V_Base);
	vec3 V = V_Base / V_Len;
	vec3 R = reflect(L, N);
	vec4 diffuse = vec4(max(dot(N, -L), 0.0) * diffuse_albedo, 1.0);
	vec3 specular = vec3(pow(max(dot(R, V), 0.0), /* renderhint.y * 1000.0 */ 128.0) * specular_albedo * /* renderhint.x */ 0.0);
	color = vec4((bambient * color + (vec4(depth, depth, depth, 1.0) * atten * (diffuse * vec4(light_color, 1.0)) * color) +
		(vec4(min(specular, 1.0), 0.0) * vec4(light_color, 1.0) * atten * depth)).xyz, color.w);
#ifdef MCM_GOOD_GRAPHICS
	color = vec4(desaturate(color.xyz), color.w);
#endif
}
