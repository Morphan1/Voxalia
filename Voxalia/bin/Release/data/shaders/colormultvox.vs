#version 430 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 texcoord;
layout (location = 3) in vec4 color;

const int MAX_BONES = 200;

layout (location = 1) uniform mat4 projection;
layout (location = 2) uniform mat4 model_matrix;
layout (location = 3) uniform vec4 v_color = vec4(1.0);
// ...

layout (location = 0) out vec4 f_color;
layout (location = 1) out vec3 f_texcoord;

void main()
{
	f_color = color;
    if (f_color == vec4(0.0, 0.0, 0.0, 1.0))
    {
        f_color = vec4(1.0);
    }
    f_color = f_color * v_color;
	f_texcoord = texcoord;
	gl_Position = projection * model_matrix * vec4(position.xyz, 1.0);
}
