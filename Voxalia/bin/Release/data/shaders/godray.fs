#version 430 core

layout (binding = 0) uniform sampler2D godraytex;

layout (location = 0) in vec2 f_texcoord;

out vec4 color;

void main()
{
	color = texture(godraytex, f_texcoord);
}
