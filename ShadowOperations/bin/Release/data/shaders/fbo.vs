#version 430 core

layout (location = 0) in vec4 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec2 texcoords;
layout (location = 3) in vec4 color;
layout (location = 4) in vec4 Weights;
layout (location = 5) in vec4 BoneID;

layout (location = 0) out vec4 f_position;
layout (location = 1) out vec3 f_normal;
layout (location = 2) out vec2 f_texcoord;

const int MAX_BONES = 50;

layout (location = 1) uniform mat4 proj_matrix;
layout (location = 2) uniform mat4 mv_matrix;
layout (location = 6) uniform mat4 boneTrans[MAX_BONES];

void main(void)
{
	mat4 boneTransform = boneTrans[int(BoneID[0])] * Weights[0] +
						 boneTrans[int(BoneID[1])] * Weights[1] +
						 boneTrans[int(BoneID[2])] * Weights[2] +
						 boneTrans[int(BoneID[3])] * Weights[3];
	f_texcoord = texcoords;
	f_position = mv_matrix * (boneTransform * position);
	mat4 mv_mat_simple = mv_matrix;
	mv_mat_simple[3][0] = 0.0;
	mv_mat_simple[3][1] = 0.0;
	mv_mat_simple[3][2] = 0.0;
	vec4 norm1 = boneTransform * vec4(normal, 1.0);
	vec4 nnormal = mv_mat_simple * vec4(norm1.xyz, 1.0);
	f_normal = nnormal.xyz / nnormal.w; // TODO: Normalize?
	vec4 pos1 = boneTransform * position;
	gl_Position = proj_matrix * mv_matrix * vec4(pos1.xyz, 1.0);
}
