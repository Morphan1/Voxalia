#version 430 core
#INCLUDE_STATEMENTS_HERE

layout (binding = 0) uniform sampler2D pre_lighttex;
layout (binding = 1) uniform sampler2D positiontex;
layout (binding = 2) uniform sampler2D normaltex;
layout (binding = 3) uniform sampler2D depthtex;
layout (binding = 4) uniform sampler2DArray tex;
layout (binding = 5) uniform sampler2D renderhinttex;
layout (binding = 6) uniform sampler2D diffusetex;

layout (location = 0) in vec2 f_texcoord;
layout (location = 1) in vec4 f_position;

layout (location = 4) uniform vec3 light_pos = vec3(5.0, 5.0, 5.0);
layout (location = 5) uniform vec3 diffuse_albedo = vec3(0.7, 0.7, 0.7);
layout (location = 6) uniform float specular_albedo = 0.7;
// ...
layout (location = 8) uniform vec3 light_color = vec3(1.0, 1.0, 1.0);
layout (location = 9) uniform float light_radius = 30.0;
layout (location = 10) uniform vec3 eye_pos = vec3(0.0, 0.0, 0.0);
layout (location = 11) uniform float light_type = 0.0;
layout (location = 12) uniform float tex_size = 0.001;
layout (location = 13) uniform float depth_jump = 0.5;
layout (location = 14) uniform mat4 shadow_matrix1;
layout (location = 15) uniform mat4 shadow_matrix2;
layout (location = 16) uniform mat4 shadow_matrix3;
layout (location = 17) uniform mat4 shadow_matrix4;
layout (location = 18) uniform mat4 shadow_matrix5;
layout (location = 19) uniform mat4 shadow_matrix6;

out vec4 color;

bool isBad(in vec4 pos)
{
	return pos.x < 0.0 || pos.y < 0.0 || pos.z < 0.0 || pos.x > 1.0 || pos.y > 1.0 || pos.z > 1.0;
}

int getSpos(in vec4 pos, out vec4 spos)
{
	vec4 tspos = shadow_matrix1 * pos;
	vec4 txpos = tspos / tspos.w / 2.0 + vec4(0.5, 0.5, 0.5, 0.0);
	if (!isBad(txpos))
	{
		spos = tspos;
		return 0;
	}
	tspos = shadow_matrix2 * pos;
	txpos = tspos / tspos.w / 2.0 + vec4(0.5, 0.5, 0.5, 0.0);
	if (!isBad(txpos))
	{
		spos = tspos;
		return 1;
	}
	tspos = shadow_matrix3 * pos;
	txpos = tspos / tspos.w / 2.0 + vec4(0.5, 0.5, 0.5, 0.0);
	if (!isBad(txpos))
	{
		spos = tspos;
		return 2;
	}
	tspos = shadow_matrix4 * pos;
	txpos = tspos / tspos.w / 2.0 + vec4(0.5, 0.5, 0.5, 0.0);
	if (!isBad(txpos))
	{
		spos = tspos;
		return 3;
	}
	tspos = shadow_matrix5 * pos;
	txpos = tspos / tspos.w / 2.0 + vec4(0.5, 0.5, 0.5, 0.0);
	if (!isBad(txpos))
	{
		spos = tspos;
		return 4;
	}
	tspos = shadow_matrix2 * pos;
	spos = tspos;
	return 5;
}

void main()
{
	vec3 normal = texture(normaltex, f_texcoord).xyz;
	vec3 position = texture(positiontex, f_texcoord).xyz;
	vec4 renderhint = texture(renderhinttex, f_texcoord);
	vec4 diffuset = texture(diffusetex, f_texcoord);
	vec4 f_spos;
	int shtex = getSpos(vec4(position, 1.0), f_spos);
	if (position == vec3(0.0) && normal == vec3(0.0))
	{
		f_spos = vec4(999999999.0, 999999999.0, -999999999.0, 1.0);
		position = vec3(999999999.0, 999999999.0, -999999999.0);
	}
	vec4 prelight_color = texture(pre_lighttex, f_texcoord);
	vec3 N = normalize(-normal);
	vec3 light_path = light_pos - position;
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
            color = prelight_color;
            return;
        }
	}
	vec3 L = light_path / light_length;
	vec3 V_Base = position - eye_pos;
	float V_Len = length(V_Base);
	vec3 V = V_Base / V_Len;
	vec3 R = reflect(L, N);
	vec4 diffuse = vec4(max(dot(N, -L), 0.0) * diffuse_albedo, 1.0);
	vec3 specular = vec3(pow(max(dot(R, V), 0.0), renderhint.y * 1000.0) * specular_albedo * renderhint.x);
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
		depth = 0;
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
				vec4 fstx = fs + vec4(x * jump, y * jump, offz, 0.0);
				vec3 tester = vec3(fstx.x, fstx.y, float(shtex));
				depth = float(texture(tex, tester).r > fstx.z);
				depth_count++;
			}
		}
		depth = depth / depth_count;
#else
		vec4 fstx = fs - vec4(0.0, 0.0, 0.0001, 0.0);
		vec3 tester = vec3(fstx.x, fstx.y, float(shtex));
		depth = float(texture(tex, tester).r > fstx.z);
#endif
	}
	color = vec4((prelight_color + (vec4(depth, depth, depth, 1.0) *
		atten * (diffuse * vec4(light_color, 1.0)) * diffuset) +
		(vec4(min(specular, 1.0), 0.0) * vec4(light_color, 1.0) * atten * depth)).xyz, diffuset.w);
}
