#version 430 core

layout (binding = 0) uniform sampler2D tex;
layout (binding = 1) uniform sampler2D renderhinttex;

layout (location = 6) uniform float width;
layout (location = 7) uniform float height;

const float HDR_Mod = 5.0;
const float HDR_Div = (1.0 / HDR_Mod);

out float color;

void main()
{
	vec4 cur;
	int w = int(width);
	int h = int(height);
	float min_val = 0;
	for (int x = 0; x < w; x++)
	{
		for (int y = 0; y < h; y++)
		{
			cur = texture(tex, vec2(x, y)) * HDR_Div;
			float c = cur.x + cur.y + cur.z;// + texture(renderhinttex, vec2(x, y)).z;
			if (c > min_val)
			{
				min_val = c;
			}
		}
	}
	color = min_val;
}

