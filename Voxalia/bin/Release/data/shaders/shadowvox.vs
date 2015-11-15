#version 430 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 texcoords;
layout (location = 3) in vec4 color;

layout (location = 1) uniform mat4 projection = mat4(1.0);
layout (location = 2) uniform mat4 model_matrix = mat4(1.0);

layout (location = 0) out vec4 f_pos;
layout (location = 1) out vec3 f_texcoord;

void main()
{
	f_pos = projection * model_matrix * vec4(position, 1.0);
	f_texcoord = texcoords;
	gl_Position = f_pos;
}
