#version 430 core

#define MCM_GOOD_GRAPHICS 0
#define MCM_SHADOWS 0

layout (binding = 1) uniform sampler2D positiontex;
layout (binding = 2) uniform sampler2D normaltex;
// ...
layout (binding = 4) uniform sampler2DShadow tex;
layout (binding = 5) uniform sampler2D renderhinttex;
layout (binding = 6) uniform sampler2D diffusetex;
layout (binding = 7) uniform sampler2D renderhint2tex;

layout (location = 0) in vec2 f_texcoord;
layout (location = 1) in vec4 f_position;

layout (location = 3) uniform mat4 shadow_matrix;
layout (location = 4) uniform vec3 light_pos = vec3(5.0, 5.0, 5.0);
layout (location = 5) uniform vec3 diffuse_albedo = vec3(0.7, 0.7, 0.7);
layout (location = 6) uniform float specular_albedo = 0.7;
layout (location = 7) uniform float should_sqrt = 0.0;
layout (location = 8) uniform vec3 light_color = vec3(1.0, 1.0, 1.0);
layout (location = 9) uniform float light_radius = 30.0;
layout (location = 10) uniform vec3 eye_pos = vec3(0.0, 0.0, 0.0);
layout (location = 11) uniform float light_type = 0.0;
layout (location = 12) uniform float tex_size = 0.001;
layout (location = 13) uniform float depth_jump = 0.5;

const float HDR_Mod = 5.0;

out vec4 color;

void main()
{
	vec3 normal = texture(normaltex, f_texcoord).xyz;
	vec3 position = texture(positiontex, f_texcoord).xyz;
	vec4 renderhint = texture(renderhinttex, f_texcoord);
	vec4 renderhint2 = texture(renderhint2tex, f_texcoord);
	vec4 diffuset = texture(diffusetex, f_texcoord);
	vec4 f_spos = shadow_matrix * vec4(position, 1.0);
	vec3 N = normalize(-normal);
	vec3 light_path = light_pos - position;
	float light_length = length(light_path);
	float d = light_length / light_radius;
	float atten = clamp(1.0 - (d * d), 0.0, 1.0);
	if (light_type == 1.0)
	{
		vec4 fst = f_spos / f_spos.w;
		atten *= 1 - (fst.x * fst.x + fst.y * fst.y);
		if (atten < 0)
		{
			discard;
		}
	}
	if (should_sqrt > 0.5)
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
		discard;
	}
#if MCM_SHADOWS
#if MCM_GOOD_GRAPHICS
	vec2 dz_duv;
	vec3 duvdist_dx = dFdx(fs.xyz);
	vec3 duvdist_dy = dFdy(fs.xyz);
	dz_duv.x = duvdist_dy.y * duvdist_dx.z - duvdist_dx.y * duvdist_dy.z;
	dz_duv.y = duvdist_dx.x * duvdist_dy.z - duvdist_dy.x * duvdist_dx.z;
	float tlen = (duvdist_dx.x * duvdist_dy.y) - (duvdist_dx.y * duvdist_dy.x);
	dz_duv /= tlen;
	float oneoverdj = 1.0 / depth_jump;
	float jump = tex_size * depth_jump;
	float depth = 0;
	float depth_count = 0;
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
			depth += textureProj(tex, fs + vec4(x * jump, y * jump, offz, 0.0));
			depth_count++;
		}
	}
	depth = depth / depth_count;
#else
	float depth = textureProj(tex, fs - vec4(0.0, 0.0, 0.0001, 0.0));
#endif
#else
	const float depth = 1.0;
#endif
	vec3 L = light_path / light_length;
	vec4 diffuse = vec4(max(dot(N, -L), 0.0) * diffuse_albedo, 1.0) * HDR_Mod;
	vec3 specular = vec3(pow(max(dot(reflect(L, N), normalize(position - eye_pos)), 0.0), (200.0 / 1000.0f /* RENDERHINT.Y : SPECULAR POWER */ ) * 1000.0) * specular_albedo * renderhint.x) * HDR_Mod;
	color = vec4(((vec4(depth, depth, depth, 1.0) *
		atten * (diffuse * vec4(light_color, 1.0)) * diffuset) +
		(vec4(min(specular, 1.0), 0.0) * vec4(light_color, 1.0) * atten * depth)).xyz, diffuset.w);
}
