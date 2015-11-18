#version 430 core

layout (binding = 0) uniform sampler2DArray s;
layout (binding = 1) uniform sampler2DArray htex;

layout (location = 3) uniform vec4 v_color = vec4(1.0);
layout (location = 4) uniform float specular_power = 200.0 / 1000.0f;
layout (location = 5) uniform float minimum_light = 0.0;
layout (location = 6) uniform float specular_strength = 1.0;
layout (location = 7) uniform vec4 bw_color = vec4(0.0, 0.0, 0.0, 1.0);
layout (location = 8) uniform vec2 light_clamp = vec2(0.0, 1.0);

layout (location = 0) in vec4 f_position;
layout (location = 1) in vec3 f_normal;
layout (location = 2) in vec3 f_texcoord;
layout (location = 3) in vec4 f_color;

layout (location = 0) out vec4 color;
layout (location = 1) out vec4 position;
layout (location = 2) out vec4 normal;
layout (location = 3) out vec4 renderhint;
layout (location = 4) out vec4 bw;

void main()
{
	vec4 col = texture(s, f_texcoord) * vec4(clamp(f_color.xyz, vec3(light_clamp.x), vec3(light_clamp.y)), f_color.w);
	if (col.w * v_color.w < 0.99)
	{
		discard;
	}
    float spec = texture(htex, vec3(0, 0, f_texcoord.z)).r;
    float wavi = texture(htex, vec3(1, 0, f_texcoord.z)).r; // TODO: Implement!?
    float refl = texture(htex, vec3(0, 1, f_texcoord.z)).r;
	color = col * v_color;
	position = vec4(f_position.xyz, 1.0);
	normal = vec4(f_normal, 1.0);
	renderhint = vec4(specular_strength + spec, specular_power, minimum_light, refl);
    // TODO: Maybe take advantage of normal.w and position.w as well?
    bw = bw_color;
}
