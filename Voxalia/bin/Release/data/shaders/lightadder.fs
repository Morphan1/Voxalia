#version 430 core

#define MCM_GOOD_GRAPHICS 0
#define MCM_SHADOWS 0

layout (binding = 1) uniform sampler2D positiontex; // The G-Buffer positions texture.
layout (binding = 2) uniform sampler2D normaltex; // The G-Buffer normals texture.
// ...
layout (binding = 4) uniform sampler2DArray shadowtex; // The shadow map for the current light.
layout (binding = 5) uniform sampler2D renderhinttex; // Rendering hint texture (x is specular strength, z is ambient light strength).
layout (binding = 6) uniform sampler2D diffusetex; // The diffuse texture (G-Buffer colors).

in struct vox_out // Represents data from the VS file.
{
	vec2 texcoord; // The texture coordinate.
} f; // It's named "f".

const int LIGHTS_MAX = 10; // How many lights we can ever have.

layout (location = 3) uniform float depth_jump = 0.5; // How much to jump around when calculating shadow coordinates.
layout (location = 4) uniform vec3 ambient = vec3(0.05); // How much ambient light to apply.
// ...
layout (location = 9) uniform float lights_used = 0.0; // How many lights are present.
layout (location = 10) uniform mat4 shadow_matrix_array[LIGHTS_MAX]; // The matrices of the light sources.
layout (location = 20) uniform mat4 light_data_array[LIGHTS_MAX]; // Data for all the lights.

const float HDR_Mod = 5.0; // The HDR modifier: multiply all lights by this constant to improve accuracy of colors.

out vec4 color; // The color to add to the lighting texture

void main() // Let's put all code in main, why not...
{
	vec3 res_color = vec3(0.0);
	float aff = 0.0;
	// Gather all the texture information.
	vec3 normal = texture(normaltex, f.texcoord).xyz;
	vec3 position = texture(positiontex, f.texcoord).xyz;
	vec3 renderhint = texture(renderhinttex, f.texcoord).xyz;
	vec4 diffuset = texture(diffusetex, f.texcoord);
	// Loop over lights
	int count = int(lights_used);
	for (int i = 0; i < count; i++)
	{
	mat4 light_data = light_data_array[i];
	mat4 shadow_matrix = shadow_matrix_array[i];
	// Light data.
	vec3 light_pos = vec3(light_data[0][0], light_data[0][1], light_data[0][2]); // The position of the light source.
	float diffuse_albedo = light_data[0][3]; // The diffuse albedo of this light (diffuse light is multiplied directly by this).
	float specular_albedo = light_data[1][0]; // The specular albedo (specular power is multiplied directly by this).
	float should_sqrt = light_data[1][1]; // 0 to not use square-root trick, 1 to use it (see implementation for details).
	vec3 light_color = vec3(light_data[1][2], light_data[1][3], light_data[2][0]); // The color of the light.
	float light_radius = light_data[2][1]; // The maximum radius of the light.
	vec3 eye_pos = vec3(light_data[2][2], light_data[2][3], light_data[3][0]); // The position of the camera eye.
	float light_type = light_data[3][1]; // What type of light this is: 0 is standard (point, sky, etc.), 1 is conical (spot light).
	float tex_size = light_data[3][2]; // If shadows are enabled, this is the inverse of the texture size of the shadow map.
	// float unused = light_data[3][3];
	vec4 f_spos = shadow_matrix * vec4(position, 1.0); // Calculate the position of the light relative to the view.
	f_spos /= f_spos.w; // Standard perspective divide.
	vec3 N = normalize(-normal); // Normalize the normal, just in case
	vec3 light_path = light_pos - position; // What path a light ray has to travel down in theory to get from the source to the current pixel.
	float light_length = length(light_path); // How far the light is from this pixel.
	float d = light_length / light_radius; // How far the pixel is from the end of the light.
	float atten = clamp(1.0 - (d * d), 0.0, 1.0); // How weak the light is here, based purely on distance so far.
	if (light_type >= 0.5) // If this is a conical (spot light)...
	{
		atten *= 1.0 - (f_spos.x * f_spos.x + f_spos.y * f_spos.y); // Weaken the light based on how far towards the edge of the cone/circle it is. Bright in the center, dark in the corners.
	}
	if (atten <= 0.0) // If light is really weak...
	{
		continue; // Forget this light, move on already!
	}
	if (should_sqrt >= 0.5) // If square-root trick is enabled (generally this'll be 1.0 or 0.0)
	{
		f_spos.x = sign(f_spos.x) * sqrt(abs(f_spos.x)); // Square-root the relative position while preserving the sign. Shadow creation buffer also did this.
		f_spos.y = sign(f_spos.y) * sqrt(abs(f_spos.y)); // This section means that coordinates near the center of the light view will have more pixels per area available than coordinates far from the center.
	}
	vec3 fs = f_spos.xyz * 0.5 + vec3(0.5, 0.5, 0.5); // Create a variable representing the proper screen/texture coordinate of the shadow view (ranging from 0 to 1 instead of -1 to 1).
	if (fs.x < 0.0 || fs.x > 1.0
		|| fs.y < 0.0 || fs.y > 1.0
		|| fs.z < 0.0 || fs.z > 1.0) // If any coordinate is outside view range...
	{
		continue; // We can't light it! Discard straight away!
	}
	// This block only runs if shadows are enabled.
#if MCM_SHADOWS
	// Pretty quality (soft) shadows require a quality graphics card.
#if MCM_GOOD_GRAPHICS
	// This area is some calculus-ish stuff based upon NVidia sample code (naturally, it seems to run poorly on AMD cards. Good area to recode/optimize!)
	// It's used to take the shadow map coordinates, and gets a safe Z-modifier value (See below).
	vec3 duvdist_dx = dFdx(fs);
	vec3 duvdist_dy = dFdy(fs);
	vec2 dz_duv = vec2(duvdist_dy.y * duvdist_dx.z - duvdist_dx.y * duvdist_dy.z, duvdist_dx.x * duvdist_dy.z - duvdist_dy.x * duvdist_dx.z);
	float tlen = (duvdist_dx.x * duvdist_dy.y) - (duvdist_dx.y * duvdist_dy.x);
	dz_duv /= tlen;
	float oneoverdj = 1.0 / depth_jump;
	float jump = tex_size * depth_jump;
	float depth = 0;
	float depth_count = 0;
	// Loop over an area quite near the pixel on the shadow map, but still covering multiple pixels of the shadow map.
	for (float x = -oneoverdj * 2; x < oneoverdj * 2 + 1; x++)
	{
		for (float y = -oneoverdj * 2; y < oneoverdj * 2 + 1; y++)
		{
			float offz = dot(dz_duv, vec2(x * jump, y * jump)) * 1000.0; // Use the calculus magic from before to get a safe Z-modifier.
			if (offz > -0.000001) // (Potentially removable?) It MUST be negative, and below a certain threshold. If it's not...
			{
				offz = -0.000001; // Force it to the threshold value to reduce errors.
			}
			offz -= 0.001; // Set it a bit farther regardless to reduce bad shadows.
			float rd = texture(shadowtex, vec3(fs.x + x * jump, -(fs.y + y * jump), float(i))).r; // Calculate the depth of the pixel.
			depth += (rd >= (fs.z + offz) ? 1.0 : 0.0); // Get a 1 or 0 depth value for the current pixel. 0 means don't light, 1 means light.
			depth_count++; // Can probably use math to generate this number rather than constantly incrementing a counter.
		}
	}
	depth = depth / depth_count; // Average up the 0 and 1 light values to produce gray near the edges of shadows. Soft shadows, hooray!
#else
	float rd = texture(shadowtex, vec3(fs.x, fs.y, float(i))).r; // Calculate the depth of the pixel.
	float depth = (rd >= (fs.z - 0.00001) ? 1.0 : 0.0); // If we have a bad graphics card, just quickly get a 0 or 1 depth value. This will be pixelated (hard) shadows!
#endif
	if (depth <= 0.0)
	{
		continue; // If we're a fully shadowed pixel, don't add any light!
	}
#else
	const float depth = 1.0; // If shadows are off, depth is a constant 1.0!
#endif
	vec3 L = light_path / light_length; // Get the light's movement direction as a vector
	vec3 diffuse = max(dot(N, -L), 0.0) * vec3(diffuse_albedo) * HDR_Mod; // Find out how much diffuse light to apply
	vec3 specular = vec3(pow(max(dot(reflect(L, N), normalize(position - eye_pos)), 0.0), (200.0 / 1000.0) * 1000.0) * specular_albedo * renderhint.x) * HDR_Mod; // Find out how much specular light to apply.
	res_color += (vec3(depth, depth, depth) * atten * (diffuse * light_color) * diffuset.xyz) + (min(specular, 1.0) * light_color * atten * depth); // Put it all together now.
	aff += atten;
	}
	res_color += (ambient + vec3(renderhint.z)) * HDR_Mod * diffuset.xyz; // Add ambient light.
	color = vec4(res_color, diffuset.w + aff + renderhint.z); // I don't know why alpha value this became necessary.
}
