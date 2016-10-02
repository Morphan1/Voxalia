#version 430 core

layout (points) in;
layout (triangle_strip, max_vertices = 6) out;

layout (location = 1) uniform mat4 proj_matrix = mat4(1.0);
// ...
layout (location = 5) uniform vec3 wind = vec3(0.0);

in struct vox_out
{
	vec2 texcoord;
	vec4 color;
} f[1];

out struct vox_out
{
	vec2 texcoord;
	vec4 color;
} fi;

void main()
{
	fi.color = f[0].color;
	vec3 pos = gl_in[0].gl_Position.xyz;
	if (dot(pos, pos) > (50.0 * 50.0))
	{
		return;
	}
	vec3 up = vec3(0.0, 0.0, 1.0);
	vec3 right = cross(up, vec3(pos.x, pos.y, 0.0)) * 0.1;
	// First Vertex
	gl_Position = proj_matrix * vec4(pos - (right) * 0.5, 1.0);
	fi.texcoord = vec2(0.0, 1.0);
	EmitVertex();
	// Second Vertex
	gl_Position = proj_matrix * vec4(pos + (right) * 0.5, 1.0);
	fi.texcoord = vec2(1.0, 1.0);
	EmitVertex();
	// Third Vertex
	gl_Position = proj_matrix * vec4(pos - (right + up * 2.0) * 0.5 + wind, 1.0);
	fi.texcoord = vec2(0.0, 0.5);
	EmitVertex();
	// Forth Vertex
	gl_Position = proj_matrix * vec4(pos + (right + up * 2.0) * 0.5 + wind, 1.0);
	fi.texcoord = vec2(1.0, 0.5);
	EmitVertex();
	// Fifth Vertex
	gl_Position = proj_matrix * vec4(pos - (right + up * 4.0) * 0.5 + wind * 2.0, 1.0);
	fi.texcoord = vec2(0.0, 0.0);
	EmitVertex();
	// Sixth Vertex
	gl_Position = proj_matrix * vec4(pos + (right + up * 4.0) * 0.5 + wind * 2.0, 1.0);
	fi.texcoord = vec2(1.0, 0.0);
	EmitVertex();
	EndPrimitive();
}
