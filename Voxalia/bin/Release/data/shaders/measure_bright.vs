#version 430 core

layout (location = 0) in vec3 position;

layout (location = 1) uniform mat4 projection;
layout (location = 2) uniform mat4 model_matrix;

void main()
{
	gl_Position = projection * model_matrix * vec4(position, 1.0);
}
