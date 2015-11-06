#version 430 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec2 texcoord;
layout (location = 3) in vec4 color;
layout (location = 4) in vec4 Weights;
layout (location = 5) in vec4 BoneID;
layout (location = 6) in vec4 Weights2;
layout (location = 7) in vec4 BoneID2;

const int MAX_BONES = 200;

layout (location = 1) uniform mat4 projection = mat4(1.0);
layout (location = 2) uniform mat4 model_matrix = mat4(1.0);
layout (location = 3) uniform vec4 v_color = vec4(1.0);
// ...
layout (location = 7) uniform mat4 simplebone_matrix = mat4(1.0);
layout (location = 8) uniform mat4 boneTrans[MAX_BONES];

layout (location = 0) out vec4 f_color;
layout (location = 1) out vec2 f_texcoord;

void main()
{
	vec4 pos1;
	float rem = 1.0 - (Weights[0] + Weights[1] + Weights[2] + Weights[3] + Weights2[0] + Weights2[1] + Weights2[2] + Weights2[3]);
	mat4 BT = mat4(1.0);
	if (rem < 0.99)
	{
		BT = boneTrans[int(BoneID[0])] * Weights[0];
		BT += boneTrans[int(BoneID[1])] * Weights[1];
		BT += boneTrans[int(BoneID[2])] * Weights[2];
		BT += boneTrans[int(BoneID[3])] * Weights[3];
		BT += boneTrans[int(BoneID2[0])] * Weights2[0];
		BT += boneTrans[int(BoneID2[1])] * Weights2[1];
		BT += boneTrans[int(BoneID2[2])] * Weights2[2];
		BT += boneTrans[int(BoneID2[3])] * Weights2[3];
		BT += mat4(1.0) * rem;
		pos1 = vec4(position, 1.0) * BT;
	}
	else
	{
		pos1 = vec4(position, 1.0);
	}
	pos1 *= simplebone_matrix;
	f_color = color;
    if (f_color == vec4(0.0, 0.0, 0.0, 1.0))
    {
        f_color = vec4(1.0);
    }
    f_color = f_color * v_color;
	f_texcoord = texcoord;
	gl_Position = projection * model_matrix * vec4(pos1.xyz, 1.0);
}
