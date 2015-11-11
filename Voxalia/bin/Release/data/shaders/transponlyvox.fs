#version 430 core

#INCLUDE_STATEMENTS_HERE

layout (binding = 0) uniform sampler2DArray tex;
layout (binding = 1) uniform sampler2DArray htex;

layout (location = 0) in vec4 f_color;
layout (location = 1) in vec3 f_texcoord;

layout (location = 4) uniform float desaturationAmount = 1.0;

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
#ifdef MCM_GOOD_GRAPHICS
    color = vec4(desaturate(color.xyz), color.w);
#endif
}
