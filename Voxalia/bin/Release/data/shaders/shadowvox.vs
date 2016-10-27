#version 430 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 texcoords;
layout (location = 3) in vec4 color;

layout (location = 1) uniform mat4 projection = mat4(1.0);
layout (location = 2) uniform mat4 model_matrix = mat4(1.0);
layout (location = 3) uniform float should_sqrt = 0.0;

layout (location = 0) out vec4 f_pos;
layout (location = 1) out vec3 f_texcoord;
layout (location = 2) out vec4 f_color;

void main()
{
	f_pos = projection * model_matrix * vec4(position, 1.0);
	f_texcoord = texcoords;
	if (should_sqrt >= 0.5)
	{
		f_pos /= f_pos.w;
		f_pos.x = sign(f_pos.x) * sqrt(abs(f_pos.x));
		f_pos.y = sign(f_pos.y) * sqrt(abs(f_pos.y));
	}
	gl_Position = f_pos;
	f_color = color;
}
