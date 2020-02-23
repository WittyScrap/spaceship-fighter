
float4 permute(float4 x)
{
	return fmod(34.0 * pow(x, 2) + x, 289.0);
}

float2 fade(float2 t)
{
	return 6.0 * pow(t, 5.0) - 15.0 * pow(t, 4.0) + 10.0 * pow(t, 3.0);
}

float4 taylorInvSqrt(float4 r)
{
	return 1.79284291400159 - 0.85373472095314 * r;
}

#define DIV_289 0.00346020761245674740484429065744f

float mod289(float x)
{
	return x - floor(x * DIV_289) * 289.0;
}

float perlin(float2 p)
{
	float4 Pi = floor(p.xyxy) + float4(0.0, 0.0, 1.0, 1.0);
	float4 Pf = frac(p.xyxy) - float4(0.0, 0.0, 1.0, 1.0);

	float4 ix = Pi.xzxz;
	float4 iy = Pi.yyww;
	float4 fx = Pf.xzxz;
	float4 fy = Pf.yyww;

	float4 i = permute(permute(ix) + iy);

	float4 gx = frac(i / 41.0) * 2.0 - 1.0;
	float4 gy = abs(gx) - 0.5;
	float4 tx = floor(gx + 0.5);
	gx = gx - tx;

	float2 g00 = float2(gx.x, gy.x);
	float2 g10 = float2(gx.y, gy.y);
	float2 g01 = float2(gx.z, gy.z);
	float2 g11 = float2(gx.w, gy.w);

	float4 norm = taylorInvSqrt(float4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));
	g00 *= norm.x;
	g01 *= norm.y;
	g10 *= norm.z;
	g11 *= norm.w;

	float n00 = dot(g00, float2(fx.x, fy.x));
	float n10 = dot(g10, float2(fx.y, fy.y));
	float n01 = dot(g01, float2(fx.z, fy.z));
	float n11 = dot(g11, float2(fx.w, fy.w));

	float2 fade_xy = fade(Pf.xy);
	float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
	float n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
	return 2.3 * n_xy / 2 + .5f;
}

float3 hash(float3 p) // replace this by something better. really. do
{
	p = float3(dot(p, float3(127.1, 311.7, 74.7)),
		dot(p, float3(269.5, 183.3, 246.1)),
		dot(p, float3(113.5, 271.9, 124.6)));

	return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
}

// return value noise (in x) and its derivatives (in yzw)
float4 noised(in float3 x)
{
	// grid
	float3 i = floor(x);
	float3 w = frac(x);

#if 1
	// quintic interpolant
	float3 u = w * w * w * (w * (w * 6.0 - 15.0) + 10.0);
	float3 du = 30.0 * w * w * (w * (w - 2.0) + 1.0);
#else
	// cubic interpolant
	float3 u = w * w * (3.0 - 2.0 * w);
	float3 du = 6.0 * w * (1.0 - w);
#endif    

	// gradients
	float3 ga = hash(i + float3(0.0, 0.0, 0.0));
	float3 gb = hash(i + float3(1.0, 0.0, 0.0));
	float3 gc = hash(i + float3(0.0, 1.0, 0.0));
	float3 gd = hash(i + float3(1.0, 1.0, 0.0));
	float3 ge = hash(i + float3(0.0, 0.0, 1.0));
	float3 gf = hash(i + float3(1.0, 0.0, 1.0));
	float3 gg = hash(i + float3(0.0, 1.0, 1.0));
	float3 gh = hash(i + float3(1.0, 1.0, 1.0));

	// projections
	float va = dot(ga, w - float3(0.0, 0.0, 0.0));
	float vb = dot(gb, w - float3(1.0, 0.0, 0.0));
	float vc = dot(gc, w - float3(0.0, 1.0, 0.0));
	float vd = dot(gd, w - float3(1.0, 1.0, 0.0));
	float ve = dot(ge, w - float3(0.0, 0.0, 1.0));
	float vf = dot(gf, w - float3(1.0, 0.0, 1.0));
	float vg = dot(gg, w - float3(0.0, 1.0, 1.0));
	float vh = dot(gh, w - float3(1.0, 1.0, 1.0));

	// interpolations
	return float4(va + u.x * (vb - va) + u.y * (vc - va) + u.z * (ve - va) + u.x * u.y * (va - vb - vc + vd) + u.y * u.z * (va - vc - ve + vg) + u.z * u.x * (va - vb - ve + vf) + (-va + vb + vc - vd + ve - vf - vg + vh) * u.x * u.y * u.z,    // value
		ga + u.x * (gb - ga) + u.y * (gc - ga) + u.z * (ge - ga) + u.x * u.y * (ga - gb - gc + gd) + u.y * u.z * (ga - gc - ge + gg) + u.z * u.x * (ga - gb - ge + gf) + (-ga + gb + gc - gd + ge - gf - gg + gh) * u.x * u.y * u.z +   // derivatives
		du * (float3(vb, vc, ve) - va + u.yzx * float3(va - vb - vc + vd, va - vc - ve + vg, va - vb - ve + vf) + u.zxy * float3(va - vb - ve + vf, va - vb - vc + vd, va - vc - ve + vg) + u.yzx * u.zxy * (-va + vb + vc - vd + ve - vf - vg + vh)));
}