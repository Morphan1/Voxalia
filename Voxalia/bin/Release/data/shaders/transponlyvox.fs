#version 430 core

#define MCM_GOOD_GRAPHICS 0

layout (binding = 0) uniform sampler2DArray tex;
layout (binding = 1) uniform sampler2DArray htex;

in struct vox_out
{
	vec4 position;
	vec3 texcoord;
	vec4 color;
	vec4 tcol;
	mat3 tbn;
} f;

layout (location = 4) uniform float desaturationAmount = 1.0;

out vec4 color;

vec3 desaturate(vec3 c)
{
	return mix(c, vec3(0.95, 0.77, 0.55) * dot(c, vec3(1.0)), desaturationAmount);
}

void main()
{
	vec4 tcolor = texture(tex, f.texcoord) * f.tcol;
	if (tcolor.w * f.color.w >= 0.99)
	{
		discard;
	}
    if (tcolor.w * f.color.w < 0.01)
    {
        discard;
    }
	color = tcolor * f.color; // TODO: Clamp f.color.xyz, match fbo_vox
#if MCM_GOOD_GRAPHICS
    color = vec4(desaturate(color.xyz), color.w); // TODO: Make available to all, not just good graphics only! Or a separate CVar!
#endif
}
