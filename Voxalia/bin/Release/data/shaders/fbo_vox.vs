#version 430 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 texcoords;
layout (location = 3) in vec4 color;

layout (location = 0) out vec4 f_position;
layout (location = 1) out vec3 f_normal;
layout (location = 2) out vec3 f_texcoord;
layout (location = 3) out vec4 f_color;

layout (location = 1) uniform mat4 proj_matrix = mat4(1.0);
layout (location = 2) uniform mat4 mv_matrix = mat4(1.0);
// ...

// TODO: handle colors?

void main(void)
{
    f_color = vec4(color.xyz, 1.0);
	f_texcoord = texcoords;
	f_position = mv_matrix * vec4(position.xyz, 1.0);
    f_position /= f_position.w;
	mat4 mv_mat_simple = mv_matrix;
	mv_mat_simple[3][0] = 0.0;
	mv_mat_simple[3][1] = 0.0;
	mv_mat_simple[3][2] = 0.0;
	f_normal = normal;
	gl_Position = proj_matrix * mv_matrix * vec4(position, 1.0);
}
