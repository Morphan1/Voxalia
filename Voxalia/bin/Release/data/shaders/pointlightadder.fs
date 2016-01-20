#version 430 core

layout (binding = 1) uniform sampler2D positiontex;
layout (binding = 2) uniform sampler2D normaltex;
layout (binding = 3) uniform sampler2D depthtex;
layout (binding = 5) uniform sampler2D renderhinttex;
layout (binding = 6) uniform sampler2D diffusetex;

layout (location = 0) in vec2 f_texcoord;

layout (location = 4) uniform vec3 light_pos = vec3(5.0, 5.0, 5.0);
layout (location = 5) uniform vec3 diffuse_albedo = vec3(0.7, 0.7, 0.7);
layout (location = 6) uniform float specular_albedo = 0.7;
// ...
layout (location = 8) uniform vec3 light_color = vec3(1.0, 1.0, 1.0);
layout (location = 9) uniform float light_radius = 30.0;
layout (location = 10) uniform vec3 eye_pos = vec3(0.0, 0.0, 0.0);
layout (location = 11) uniform float light_type = 0.0;

out vec4 color;

void main()
{
	vec3 normal = texture(normaltex, f_texcoord).xyz;
	vec3 position = texture(positiontex, f_texcoord).xyz;
	vec4 renderhint = texture(renderhinttex, f_texcoord);
	vec4 diffuset = texture(diffusetex, f_texcoord);
	if (position == vec3(0.0) && normal == vec3(0.0)) // TODO: is this still needed?
	{
		position = vec3(999999999.0, 999999999.0, -999999999.0);
	}
	vec3 N = normalize(-normal);
	vec3 light_path = light_pos - position;
	float light_length = length(light_path);
	float atten;
	if (light_length == 0.0)
	{
		light_length = 0.00001;
	}
	if (light_radius == 0.0)
	{
		atten = 1.0;
	}
	else
	{
		float d = light_length / light_radius;
		atten = clamp(1.0 - (d * d), 0.0, 1.0);
	}
	vec3 L = light_path / light_length;
	vec3 V_Base = position - eye_pos;
	float V_Len = length(V_Base);
	vec3 V = V_Base / V_Len;
	vec3 R = reflect(L, N);
	vec4 diffuse = vec4(max(dot(N, -L), 0.0) * diffuse_albedo, 1.0);
	vec3 specular = vec3(pow(max(dot(R, V), 0.0), renderhint.y * 1000.0) * specular_albedo * renderhint.x);
	color = vec4(((vec4(1.0) *
		atten * (diffuse * vec4(light_color, 1.0)) * diffuset) +
		(vec4(min(specular, 1.0), 0.0) * vec4(light_color, 1.0) * atten)).xyz, diffuset.w);
}
