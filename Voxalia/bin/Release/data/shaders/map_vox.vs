#version 430 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 texcoords;
layout (location = 3) in vec4 color;

layout (location = 0) out vec3 f_texcoord;

layout (location = 1) uniform mat4 proj_matrix = mat4(1.0);
// ...

void main(void)
{
	f_texcoord = texcoords;
	gl_Position = proj_matrix * vec4(position, 1.0);
}
