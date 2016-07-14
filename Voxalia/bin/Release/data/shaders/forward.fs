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
	// TODO: Special color handlers?
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
