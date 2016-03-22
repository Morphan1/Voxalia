#version 430 core

layout (binding = 0) uniform sampler2DArray s;

in struct vox_out
{
	vec3 texcoord;
	vec4 tcol;
} f;

layout (location = 0) out vec4 color;

void main()
{
	color = vec4((texture(s, f.texcoord) * f.tcol).xyz, 1.0);
}
