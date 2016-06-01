#version 430 core

#define MCM_TRANSP 0
#define MCM_VOX 0

#if MCM_VOX
layout (binding = 0) uniform sampler2DArray s;
#else
layout (binding = 0) uniform sampler2D s;
#endif

// ...

in struct vox_out
{
	vec4 position;
#if MCM_VOX
	vec3 texcoord;
	vec4 tcol;
	mat3 tbn;
	vec4 thv;
	vec4 thw;
#else
	vec2 texcoord;
#endif
	vec4 color;
} f;

layout (location = 0) out vec4 color;

void main()
{
	vec4 col = texture(s, f.texcoord);
#if MCM_VOX
	vec4 thval = vec4(0.0);
	float thstr = 0.0;
	vec4 thcolx = texture(s, vec3(f.texcoord.xy, f.thv.x));
	thstr += f.thw.x;
	thval += thcolx * f.thw.x;
	vec4 thcoly = texture(s, vec3(f.texcoord.xy, f.thv.y));
	thstr += f.thw.y;
	thval += thcoly * f.thw.y;
	vec4 thcolz = texture(s, vec3(f.texcoord.xy, f.thv.z));
	thstr += f.thw.z;
	thval += thcolz * f.thw.z;
	vec4 thcolw = texture(s, vec3(f.texcoord.xy, f.thv.w));
	thstr += f.thw.w;
	thval += thcolw * f.thw.w;
	float tw = col.w;
	thstr *= 0.25;
	thval *= 0.25;
	//col = thstr > 0.01 ? thval / thstr : col;
	col = thstr > 0.01 ? col * (1.0 - thstr) + thval : col;
	col.w = tw;
	col *= f.tcol;
#endif
#if MCM_TRANSP
	if (col.w * f.color.w >= 0.99)
	{
		discard;
	}
#else
	if (col.w * f.color.w < 0.99)
	{
		discard;
	}
#endif
	color = col * f.color;
}
