#version 430 core

layout (binding = 0) uniform sampler2D diffusetex;
layout (binding = 1) uniform sampler2D renderhinttex;

layout (location = 0) in vec2 f_texcoord;

layout (location = 5) uniform vec3 ambient = vec3(0.05, 0.05, 0.05);

out vec4 color;

void main()
{
	vec4 renderhint = texture(renderhinttex, f_texcoord);
	vec4 colortex_color = texture(diffusetex, f_texcoord);
	color = vec4(ambient + vec3(renderhint.z), 1.0) * colortex_color;
}
