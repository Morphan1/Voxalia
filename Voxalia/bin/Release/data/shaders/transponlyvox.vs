#version 430 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 texcoord;
layout (location = 3) in vec4 color;
layout (location = 4) in vec4 tcol;

layout (location = 1) uniform mat4 projection = mat4(1.0);
layout (location = 2) uniform mat4 model_matrix = mat4(1.0);
layout (location = 3) uniform vec4 v_color = vec4(1.0);
// ...

layout (location = 0) out vec4 f_color;
layout (location = 1) out vec3 f_texcoord;
layout (location = 2) out vec4 f_tcol;

void main()
{
	f_tcol = tcol;
	f_color = color;
    if (f_color == vec4(0.0, 0.0, 0.0, 1.0))
    {
        f_color = vec4(1.0);
    }
    f_color = f_color * v_color;
	f_texcoord = texcoord;
	gl_Position = projection * model_matrix * vec4(position, 1.0);
}
