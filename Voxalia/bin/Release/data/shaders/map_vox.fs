#version 430 core

layout (binding = 0) uniform sampler2DArray s;

layout (location = 0) in vec3 f_texcoord;

layout (location = 0) out vec4 color;

void main()
{
	color = texture(s, f_texcoord);
}
