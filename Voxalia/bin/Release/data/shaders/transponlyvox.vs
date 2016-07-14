#version 430 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 texcoord;
layout (location = 3) in vec3 tangent;
layout (location = 4) in vec4 color;
layout (location = 5) in vec4 tcol;

layout (location = 1) uniform mat4 projection = mat4(1.0);
layout (location = 2) uniform mat4 model_matrix = mat4(1.0);
layout (location = 3) uniform vec4 v_color = vec4(1.0);
// ...
layout (location = 10) uniform float time;

out struct vox_out
{
	vec4 position;
	vec3 texcoord;
	vec4 color;
	vec4 tcol;
	mat3 tbn;
	vec2 scrpos;
	float z;
} f;

vec4 color_for(in vec4 pos);
float snoise2(in vec3 v);

void main()
{
	f.color = color;
    if (f.color == vec4(0.0, 0.0, 0.0, 1.0)) // TODO: Needed?
    {
        f.color = vec4(1.0);
    }
    f.color = f.color * v_color;
	f.texcoord = texcoord;
	vec4 tpos = model_matrix * vec4(position, 1.0);
	f.position = tpos / tpos.w;
	f.tcol = color_for(f.position);
	vec4 npos = projection * tpos;
	f.scrpos = npos.xy / npos.w * 0.5 + vec2(0.5);
	f.z = npos.z;
	gl_Position = npos;
	mat4 mv_mat_simple = model_matrix;
	mv_mat_simple[3][0] = 0.0;
	mv_mat_simple[3][1] = 0.0;
	mv_mat_simple[3][2] = 0.0;
	vec3 tf_normal = (mv_mat_simple * vec4(normal, 0.0)).xyz;
	vec3 tf_tangent = (mv_mat_simple * vec4(tangent, 0.0)).xyz;
	vec3 tf_bitangent = (mv_mat_simple * vec4(cross(tangent, normal), 0.0)).xyz;
	f.tbn = transpose(mat3(tf_tangent, tf_bitangent, tf_normal));
}

const float min_cstrobe = 3.0 / 255.0;

vec4 color_for(in vec4 pos)
{
	if (tcol.w == 0.0)
	{
		if (tcol.x == 0.0 && tcol.y == 0.0 && tcol.z == 0.0)
		{
			float r = snoise2(vec3((pos.x + time) / 10.0, (pos.y + time) / 10.0, (pos.z + time) / 10.0));
			float g = snoise2(vec3((pos.x + 50.0 + time * 2) / 10.0, (pos.y + 127.0 + time * 1.7) / 10.0, (pos.z + 10.0 + time * 2.3) / 10.0));
			float b = snoise2(vec3((pos.x - 50.0 - time) / 10.0, (pos.y - 65.0 - time * 1.56) / 10.0, (pos.z + 73.0 - time * 1.3) / 10.0));
			return vec4(r, g, b, 1.0);
		}
		else if (tcol.y < 0.7 && tcol.y > 0.3 && tcol.x == 0.0 && tcol.z == 0.0)
		{
			return vec4(0.0, 0.31 + snoise2(pos.xyz) * 0.38, 0.0, 0.0);
		}
		else if (tcol.x > 0.3 && tcol.x < 0.7 && tcol.y > 0.3 && tcol.y < 0.7 && tcol.z > 0.3 && tcol.z < 0.7)
		{
			return tcol;
		}
		else
		{
			float adjust = abs(mod(time * 0.2, 2.0));
			if (adjust > 1.0)
			{
				adjust = 2.0 - adjust;
			}
			return vec4(tcol.x * adjust, tcol.y * adjust, tcol.z * adjust, 1.0);
		}
	}
	else if (tcol.w <= min_cstrobe)
	{
			float adjust = abs(mod(time * 0.2, 2.0));
			if (adjust > 1.0)
			{
				adjust = 2.0 - adjust;
			}
			return vec4(1.0 - tcol.x * adjust, 1.0 - tcol.y * adjust, 1.0 - tcol.z * adjust, 1.0);
	}
	return tcol;
}


////////////////////////////// END SHADER //////////////////////////////
////////////////////////////////////////////////////////////////////////
///////////////////////////// BEGIN GLNOISE ////////////////////////////

// MONKEY: Cleaning + updating!

//
// Description : Array and textureless GLSL 2D/3D/4D simplex 
//							 noise functions.
//			Author : Ian McEwan, Ashima Arts.
//	Maintainer : ijm
//		 Lastmod : 20110822 (ijm)
//		 License : Copyright (C) 2011 Ashima Arts. All rights reserved.
//							 Distributed under the MIT License. See LICENSE file.
//							 https://github.com/ashima/webgl-noise
// 

vec3 mod289(in vec3 x) {
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec4 mod289(in vec4 x) {
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec4 permute(in vec4 x) {
		 return mod289(((x * 34.0) + 1.0) * x);
}

vec4 taylorInvSqrt(vec4 r)
{
	return 1.79284291400159 - 0.85373472095314 * r;
}

float snoise(in vec3 v)
{ 
	const vec2	C = vec2(1.0/6.0, 1.0/3.0);
	const vec4	D = vec4(0.0, 0.5, 1.0, 2.0);

	vec3 i = floor(v + dot(v, C.yyy));
	vec3 x0 = v - i + dot(i, C.xxx);

	vec3 g = step(x0.yzx, x0.xyz);
	vec3 l = 1.0 - g;
	vec3 i1 = min(g.xyz, l.zxy);
	vec3 i2 = max(g.xyz, l.zxy);

	vec3 x1 = x0 - i1 + C.xxx;
	vec3 x2 = x0 - i2 + C.yyy;
	vec3 x3 = x0 - D.yyy;

	i = mod289(i); 
	vec4 p = permute(permute(permute(i.z + vec4(0.0, i1.z, i2.z, 1.0)) + i.y + vec4(0.0, i1.y, i2.y, 1.0)) + i.x + vec4(0.0, i1.x, i2.x, 1.0));

	float n_ = 0.142857142857;
	vec3 ns = n_ * D.wyz - D.xzx;

	vec4 j = p - 49.0 * floor(p * ns.z * ns.z);

	vec4 x_ = floor(j * ns.z);
	vec4 y_ = floor(j - 7.0 * x_);

	vec4 x = x_ *ns.x + ns.yyyy;
	vec4 y = y_ *ns.x + ns.yyyy;
	vec4 h = 1.0 - abs(x) - abs(y);

	vec4 b0 = vec4(x.xy, y.xy);
	vec4 b1 = vec4(x.zw, y.zw);

	vec4 s0 = floor(b0) * 2.0 + 1.0;
	vec4 s1 = floor(b1) * 2.0 + 1.0;
	vec4 sh = -step(h, vec4(0.0));

	vec4 a0 = b0.xzyw + s0.xzyw*sh.xxyy;
	vec4 a1 = b1.xzyw + s1.xzyw*sh.zzww;

	vec3 p0 = vec3(a0.xy,h.x);
	vec3 p1 = vec3(a0.zw,h.y);
	vec3 p2 = vec3(a1.xy,h.z);
	vec3 p3 = vec3(a1.zw,h.w);

	vec4 norm = taylorInvSqrt(vec4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
	p0 *= norm.x;
	p1 *= norm.y;
	p2 *= norm.z;
	p3 *= norm.w;

	vec4 m = max(0.6 - vec4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
	m = m * m;
	return 42.0 * dot(m * m, vec4( dot(p0, x0), dot(p1, x1), dot(p2, x2), dot(p3, x3)));
}

float snoise2(in vec3 v) // MONKEY: snoise2 (Entire function)
{
	return (snoise(v) + 1.0) * 0.5;
}
