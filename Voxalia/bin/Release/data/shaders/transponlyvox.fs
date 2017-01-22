#version 430 core
// transponlyvox.fs

#define MCM_GOOD_GRAPHICS 0
#define MCM_LIT 0
#define MCM_SHADOWS 0
#define MCM_LL 0

#define AB_SIZE 16
#define P_SIZE 4

// TODO: more dynamically defined?
#define ab_shared_pool_size (8 * 1024 * 1024)

layout (binding = 0) uniform sampler2DArray tex;
layout (binding = 1) uniform sampler2DArray htex;
layout (binding = 2) uniform sampler2DArray normal_tex;
layout (binding = 3) uniform sampler2DArray shadowtex;
#if MCM_LL
layout(size1x32, binding = 4) coherent uniform uimage2DArray ui_page;
layout(size4x32, binding = 5) coherent uniform imageBuffer uib_spage;
layout(size1x32, binding = 6) coherent uniform uimageBuffer uib_llist;
layout(size1x32, binding = 7) coherent uniform uimageBuffer uib_cspage;
#endif

in struct vox_out
{
	vec4 position;
	vec3 texcoord;
	vec4 color;
	vec4 tcol;
	mat3 tbn;
	vec2 scrpos;
	float z;
} f;

const int LIGHTS_MAX = 10; // How many lights we can ever have.

layout (location = 4) uniform float desaturationAmount = 1.0;
layout (location = 5) uniform float minimum_light;
layout (location = 8) uniform vec2 u_screensize = vec2(1024, 1024);
layout (location = 9) uniform float lights_used = 0.0;
layout (location = 10) uniform mat4 shadow_matrix_array[LIGHTS_MAX];
layout (location = 20) uniform mat4 light_details_array[LIGHTS_MAX];
layout (location = 30) uniform mat4 light_details2_array[LIGHTS_MAX];

#if MCM_LL
#else
out vec4 fcolor;
#endif

vec3 desaturate(vec3 c)
{
	return mix(c, vec3(0.95, 0.77, 0.55) * dot(c, vec3(1.0)), desaturationAmount);
}

void main()
{
#if MCM_LL
	vec4 fcolor;
#endif
	vec4 tcolor = texture(tex, f.texcoord);
	vec4 dets = texture(htex, f.texcoord);
    float spec = dets.x; // TODO: Refract / reflect?
	float rhBlur = 0.0;
	if (f.tcol.w == 0.0 && f.tcol.x == 0.0 && f.tcol.z == 0.0 && f.tcol.y > 0.3 && f.tcol.y < 0.7)
	{
		rhBlur = (f.tcol.y - 0.31) * ((1.0 / 0.38) * (3.14159 * 2.0));
	}
	else if (f.tcol.w == 0.0 && f.tcol.x > 0.3 && f.tcol.x < 0.7 && f.tcol.y > 0.3 && f.tcol.y < 0.7 && f.tcol.z > 0.3 && f.tcol.z < 0.7)
	{
		if (f.tcol.z > 0.51)
		{
			tcolor.xyz = vec3(1.0) - tcolor.xyz;
		}
		else if (f.tcol.x > 0.51)
		{
			spec = 1.0;
			//refl = 0.75;
		}
		else
		{
			tcolor *= texture(tex, vec3(f.texcoord.xy, 0));
		}
	}
	else
	{
		tcolor *= f.tcol;
	}
	if (tcolor.w * f.color.w >= 0.99)
	{
		discard;
	}
    if (tcolor.w * f.color.w < 0.01 && rhBlur == 0.0)
    {
        discard;
    }
	vec4 color = tcolor;
	fcolor = color;
	float opacity_mod = 1.0;
	vec3 eye_rel = normalize(f.position.xyz);
	float opac_min = 0.0;
#if MCM_LIT
	fcolor = vec4(0.0);
	vec3 norms = texture(normal_tex, f.texcoord).xyz * 2.0 - 1.0;
	int count = int(lights_used);
	for (int i = 0; i < count; i++)
	{
	mat4 light_details = light_details_array[i];
	mat4 light_details2 = light_details2_array[i];
	mat4 shadow_matrix = shadow_matrix_array[i];
	// Loop body
	float light_radius = light_details[0][0];
	vec3 diffuse_albedo = vec3(light_details[0][1], light_details[0][2], light_details[0][3]);
	vec3 specular_albedo = vec3(light_details[1][0], light_details[1][1], light_details[1][2]);
	float light_type = light_details[1][3];
	float should_sqrt = light_details[2][0];
	float tex_size = light_details[2][1];
	float depth_jump = light_details[2][2];
	float lightc = light_details[2][3];
	float light_min = clamp(minimum_light + dets.a, 0.0, 1.0);
	color = vec4(color.xyz * f.color.xyz, color.w);
	vec4 bambient = (vec4(light_details[3][0], light_details[3][1], light_details[3][2], 1.0) + vec4(light_min, light_min, light_min, 0.0)) / lightc;
	vec3 light_pos = vec3(light_details2[1][0], light_details2[1][1], light_details2[1][2]);
	float exposure = light_details2[2][0];
	vec3 light_color = vec3(light_details2[0][3], light_details2[2][1], light_details2[2][2]);
	vec4 x_spos = shadow_matrix * f.position;
	vec3 N = normalize(-(f.tbn * norms));
	vec3 light_path = light_pos - f.position.xyz;
	float light_length = length(light_path);
	float d = light_length / light_radius;
	float atten = clamp(1.0 - (d * d), 0.0, 1.0);
	if (light_type == 1.0)
	{
		vec4 fst = x_spos / x_spos.w;
		atten *= 1 - (fst.x * fst.x + fst.y * fst.y);
		if (atten < 0)
		{
			atten = 0;
		}
	}
	if (should_sqrt >= 1.0)
	{
		x_spos.x = sign(x_spos.x) * sqrt(abs(x_spos.x));
		x_spos.y = sign(x_spos.y) * sqrt(abs(x_spos.y));
	}
	vec4 fs = x_spos / x_spos.w / 2.0 + vec4(0.5, 0.5, 0.5, 0.0);
	fs.w = 1.0;
	if (fs.x < 0.0 || fs.x > 1.0
		|| fs.y < 0.0 || fs.y > 1.0
		|| fs.z < 0.0 || fs.z > 1.0)
	{
		fcolor += vec4(0.0, 0.0, 0.0, color.w);
		continue;
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
			float rd = texture(shadowtex, vec3(fs.x + x * jump, -(fs.y + y * jump), float(i))).r;
			depth += (rd >= (fs.z + offz) ? 1.0 : 0.0);
			depth_count++;
		}
	}
	depth = depth / depth_count;
#else // good graphics
	float rd = texture(shadowtex, vec3(fs.x, fs.y, float(i))).r;
	float depth = (rd >= (fs.z - 0.001) ? 1.0 : 0.0);
#endif // else-good graphics
#else // shadows
	const float depth = 1.0;
#endif // else-shadows
	vec3 L = light_path / light_length;
	vec4 diffuse = vec4(max(dot(N, -L), 0.0) * diffuse_albedo, 1.0);
	float powered = pow(max(dot(reflect(L, N), eye_rel), 0.0), 128.0);
	float spec_res = min(powered * spec, 1.0);
	vec3 specular = spec_res * specular_albedo;
	opac_min += spec_res;
	fcolor += vec4((bambient * color + (vec4(depth, depth, depth, 1.0) * atten * (diffuse * vec4(light_color, 1.0)) * color) + (vec4(specular, 1.0) * vec4(light_color, 1.0) * atten * depth)).xyz, color.w);
	}
#endif // lit
	if (rhBlur > 0.0)
	{
		vec3 nflat = normalize(f.tbn * vec3(0.0, 0.0, 1.0));
		float water_side = min(-dot(eye_rel, nflat), 0.2) * rhBlur;
		opacity_mod = 1.0 / (0.6 + water_side);
		//fcolor.xyz += tcolor.xyz * opacity_mod;
	}
#if MCM_GOOD_GRAPHICS
    fcolor = vec4(desaturate(fcolor.xyz), 1.0); // TODO: Make available to all, not just good graphics only! Or a separate CVar!
#endif
	fcolor = vec4(fcolor.xyz, min(tcolor.w * f.color.w * opacity_mod + opac_min, 1.0));
#if MCM_LL
	uint page = 0;
	uint frag = 0;
	uint frag_mod = 0;
	ivec2 scrpos = ivec2(f.scrpos * u_screensize);
	int i = 0;
	while (imageAtomicExchange(ui_page, ivec3(scrpos, 2), 1U) != 0U && i < 100) // TODO: 100 -> uniform var?!
	{
		memoryBarrier();
		i++;
	}
	/*if (i == 100)
	{
		return;
	}*/
	page = imageLoad(ui_page, ivec3(scrpos, 0)).x;
	frag = imageLoad(ui_page, ivec3(scrpos, 1)).x;
	frag_mod = frag % P_SIZE;
	if (frag_mod == 0)
	{
		uint npage = imageAtomicAdd(uib_cspage, 0, P_SIZE);
		if (npage < ab_shared_pool_size)
		{
			imageStore(uib_llist, int(npage / P_SIZE), uvec4(page, 0U, 0U, 0U));
			imageStore(ui_page, ivec3(scrpos, 0), uvec4(npage, 0U, 0U, 0U));
			page = npage;
		}
		else
		{
			page = 0;
		}
	}
	if (page > 0)
	{
		imageStore(ui_page, ivec3(scrpos, 1), uvec4(frag + 1, 0U, 0U, 0U));
	}
	frag = frag_mod;
	memoryBarrier();
	imageAtomicExchange(ui_page, ivec3(scrpos, 2), 0U);
	vec4 abv = fcolor;
	abv.z = float(int(fcolor.z * 255) & 255 | int(fcolor.w * 255 * 255) & (255 * 255));
	abv.w = f.z;
	imageStore(uib_spage, int(page + frag), abv);
#endif
}
