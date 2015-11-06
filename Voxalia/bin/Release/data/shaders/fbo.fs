#version 430 core

layout (binding = 0) uniform sampler2D s;

layout (location = 0) uniform vec4 bw_color = vec4(0.0, 0.0, 0.0, 1.0);
// ...
layout (location = 4) uniform float specular_power = 200.0 / 1000.0f;
layout (location = 5) uniform float minimum_light = 0.0;
layout (location = 6) uniform float specular_strength = 1.0;

layout (location = 0) in vec4 f_position;
layout (location = 1) in vec3 f_normal;
layout (location = 2) in vec2 f_texcoord;
layout (location = 3) in vec4 f_color;

layout (location = 0) out vec4 color;
layout (location = 1) out vec4 position;
layout (location = 2) out vec4 normal;
layout (location = 3) out vec4 renderhint;
layout (location = 4) out vec4 bw;

void main()
{
	vec4 col = texture(s, f_texcoord);
	if (col.w * f_color.w < 0.99)
	{
		discard;
	}
	color = col * f_color;
	position = f_position;
	normal = vec4(f_normal, 1.0);
	renderhint = vec4(specular_strength, specular_power, minimum_light, 1.0);
	// TODO: 1 Additional renderhint - we have the slot, why not
    // TODO: Maybe take advantage of normal.w as well?
    bw = bw_color;
}
