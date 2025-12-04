/*
MIT License

Copyright (c) 2017 Justin Hawkins

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

//1/7
#define K 0.142857142857
//3/7
#define Ko 0.428571428571

float3 mod(float3 x, float y) { return x - y * floor(x/y); }
float2 mod(float2 x, float y) { return x - y * floor(x/y); }

// Permutation polynomial: (34x^2 + x) mod 289
float3 Permutation(float3 x) 
{
    return mod((34.0 * x + 1.0) * x, 289.0);
}

float2 inoise(float2 P, float jitter)
{			
    float2 Pi = mod(floor(P), 289.0);
    float2 Pf = frac(P);
    float3 oi = float3(-1.0, 0.0, 1.0);
    float3 of = float3(-0.5, 0.5, 1.5);
    float3 px = Permutation(Pi.x + oi);
	
    float3 p, ox, oy, dx, dy;
    float2 F = 1e6;
	
    for(int i = 0; i < 3; i++)
    {
        p = Permutation(px[i] + Pi.y + oi); // pi1, pi2, pi3
        ox = frac(p*K) - Ko;
        oy = mod(floor(p*K),7.0)*K - Ko;
        dx = Pf.x - of[i] + jitter*ox;
        dy = Pf.y - of + jitter*oy;
		
        float3 d = dx * dx + dy * dy; // di1, di2 and di3, squared
		
        //find the lowest and second lowest distances
        for(int n = 0; n < 3; n++)
        {
            if(d[n] < F[0])
            {
                F[1] = F[0];
                F[0] = d[n];
            }
            else if(d[n] < F[1])
            {
                F[1] = d[n];
            }
        }
    }
	
    return F;
}

float fBm_F0(float2 p, int octaves, float frequency, float jitter, float lacunarity, float gain)
{
    float freq = frequency, amp = 0.5;
    float sum = 0;	
    for(int i = 0; i < octaves; i++) 
    {
        float2 F = inoise(p * freq, jitter) * amp;
		
        sum += 0.1 + sqrt(F[0]);
		
        freq *= lacunarity;
        amp *= gain;
    }
    return sum;
}

float fBm_F1_F0(float2 p, int octaves, float frequency, float jitter, float lacunarity, float gain)
{
    float freq = frequency, amp = 0.5;
    float sum = 0;	
    for(int i = 0; i < octaves; i++) 
    {
        float2 F = inoise(p * freq, jitter) * amp;
		
        sum += 0.1 + sqrt(F[1]) - sqrt(F[0]);
		
        freq *= lacunarity;
        amp *= gain;
    }
    return sum;
}