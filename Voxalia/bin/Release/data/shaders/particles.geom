#version 430 core

#define MCM_PRETTY 0

layout (points) in;
layout (triangle_strip, max_vertices = 4) out;

layout (location = 1) uniform mat4 proj_matrix = mat4(1.0);

in struct vox_out
{
#if MCM_PRETTY
	vec4 position;
	vec2 texcoord;
	vec4 color;
	mat3 tbn;
#else
	vec3 norm;
	vec2 texcoord;
	vec4 color;
#endif
} f[1];

out struct vox_fout
{
#if MCM_PRETTY
	vec4 position;
	vec3 texcoord;
	vec4 color;
	mat3 tbn;
#else
	vec3 norm;
	vec3 texcoord;
	vec4 color;
#endif
} fi;

float snoise(in vec3 v);
float snoise2(in vec3 v);

vec4 qfix(in vec4 pos, in vec3 right, in vec3 pos_norm)
{
#if MCM_PRETTY
	fi.position = pos;
	fi.tbn = transpose(mat3(right, cross(right, pos_norm), pos_norm)); // TODO: Neccessity of transpose()?
#else
	fi.norm = pos_norm;
#endif
	return pos;
}

void main()
{
	vec3 pos = gl_in[0].gl_Position.xyz;
	if (dot(pos, pos) > (50.0 * 50.0)) // TODO: Configurable particles render range cap!
	{
		return;
	}
	vec3 up = vec3(0.0, 0.0, 1.0);
	vec3 pos_norm = normalize(pos.xyz);
	if (abs(pos_norm.x) < 0.01 && abs(pos_norm.y) < 0.01)
	{
		up = vec3(0.0, 1.0, 0.0);
	}
	float scale = f[0].texcoord.x * 0.5;
	float tid = f[0].texcoord.y;
	vec3 right = cross(up, pos_norm);
	fi.color = f[0].color;
	// First Vertex
	gl_Position = proj_matrix * qfix(vec4(pos - (right) * scale, 1.0), right, pos_norm);
	fi.texcoord = vec3(0.0, 1.0, tid);
	EmitVertex();
	// Second Vertex
	gl_Position = proj_matrix * qfix(vec4(pos + (right) * scale, 1.0), right, pos_norm);
	fi.texcoord = vec3(1.0, 1.0, tid);
	EmitVertex();
	// Third Vertex
	gl_Position = proj_matrix * qfix(vec4(pos - (right - up * 2.0) * scale, 1.0), right, pos_norm);
	fi.texcoord = vec3(0.0, 0.0, tid);
	EmitVertex();
	// Forth Vertex
	gl_Position = proj_matrix * qfix(vec4(pos + (right + up * 2.0) * scale, 1.0), right, pos_norm);
	fi.texcoord = vec3(1.0, 0.0, tid);
	EmitVertex();
	EndPrimitive();
}
