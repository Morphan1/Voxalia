#version 430 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 texcoord;
layout (location = 3) in vec4 color;
layout (location = 4) in vec4 tcol;
layout (location = 5) in vec3 tangent;

layout (location = 1) uniform mat4 projection = mat4(1.0);
layout (location = 2) uniform mat4 model_matrix = mat4(1.0);
layout (location = 3) uniform vec4 v_color = vec4(1.0);
// ...

out struct vox_out
{
	vec3 position;
	vec3 texcoord;
	vec4 color;
	vec4 tcol;
	mat3 tbn;
} f;

void main()
{
	f.tcol = tcol;
	f.color = color;
    if (f.color == vec4(0.0, 0.0, 0.0, 1.0))
    {
        f.color = vec4(1.0);
    }
    f.color = f.color * v_color;
	f.texcoord = texcoord;
	vec4 tpos = model_matrix * vec4(position, 1.0);
	f.position = tpos.xyz / tpos.w;
	gl_Position = projection * tpos;
	mat4 mv_mat_simple = model_matrix;
	mv_mat_simple[3][0] = 0.0;
	mv_mat_simple[3][1] = 0.0;
	mv_mat_simple[3][2] = 0.0;
	vec3 tf_normal = (mv_mat_simple * vec4(normal, 0.0)).xyz;
	vec3 tf_tangent = (mv_mat_simple * vec4(tangent, 0.0)).xyz;
	vec3 tf_bitangent = (mv_mat_simple * vec4(cross(tangent, normal), 0.0)).xyz;
	f.tbn = transpose(mat3(tf_tangent, tf_bitangent, tf_normal));
}
