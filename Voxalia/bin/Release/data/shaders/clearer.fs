#version 430 core

#define P_SIZE 4

layout(size1x32, binding = 5) coherent uniform uimage2D ui_page;
layout(size1x32, binding = 6) coherent uniform uimage2D ui_frag;
layout(size1x32, binding = 7) coherent uniform uimage2D ui_sema;
layout(size4x32, binding = 8) coherent uniform imageBuffer uib_spage;
layout(size1x32, binding = 9) coherent uniform uimageBuffer uib_llist;
layout(size1x32, binding = 10) coherent uniform uimageBuffer uib_cspage;

layout (location = 4) uniform vec2 u_screensize = vec2(1024, 1024);

layout (location = 1) in vec2 f_scrpos;

out vec4 color;

void main()
{
	ivec2 scrpos = ivec2(f_scrpos * u_screensize);
	imageStore(ui_page, scrpos, uvec4(0U, 0U, 0U, 0U));
	imageStore(ui_frag, scrpos, uvec4(0U, 0U, 0U, 0U));
	imageStore(ui_sema, scrpos, uvec4(0U, 0U, 0U, 0U));
	//imageStore(uib_spage, 0, vec4(0, 0, 4.0, 1.0));
	//imageStore(uib_llist, 0, uvec4(0U, 0U, 0U, 0U));
	imageStore(uib_cspage, 0, uvec4(P_SIZE, 0U, 0U, 0U));
	discard;
}
