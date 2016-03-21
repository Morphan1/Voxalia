#version 430 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 texcoords;
layout (location = 3) in vec4 color;
layout (location = 4) in vec4 tcol;
layout (location = 5) in vec3 tangent;

out struct vox_out
{
	vec4 position;
	vec3 texcoord;
	vec4 color;
	vec4 tcol;
	mat3 tbn;
} f;

layout (location = 1) uniform mat4 proj_matrix = mat4(1.0);
layout (location = 2) uniform mat4 mv_matrix = mat4(1.0);
// ...

// TODO: handle colors?

void main(void)
{
    f.color = color;
    f.tcol = tcol;
	f.texcoord = texcoords;
	f.position = mv_matrix * vec4(position, 1.0);
    f.position /= f.position.w;
	mat4 mv_mat_simple = mv_matrix;
	mv_mat_simple[3][0] = 0.0;
	mv_mat_simple[3][1] = 0.0;
	mv_mat_simple[3][2] = 0.0;
	vec3 tf_normal = (mv_mat_simple * vec4(normal, 0.0)).xyz;
	vec3 tf_tangent = (mv_mat_simple * vec4(tangent, 0.0)).xyz;
	vec3 tf_bitangent = (mv_mat_simple * vec4(cross(tangent, normal), 0.0)).xyz;
	f.tbn = transpose(mat3(tf_tangent, tf_bitangent, tf_normal)); // TODO: Neccessity of transpose()?
	gl_Position = proj_matrix * mv_matrix * vec4(position, 1.0);
}
