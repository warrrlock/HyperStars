Shader "Custom/Steve IE (Distortion Only)"
{
    Properties
    {
        _MainTex ("render texture", 2D) = "white"{}
        _distortion ("distortion", Range(-1, 1)) = -.5
        _scale ("scale", Range(0, 3)) = 1

        _gscale ("glitch scale", Range(2, 800)) = 50
        _speed ("glitch speed", Range(0, 10)) = 1
        _contrast ("glitch contrast", Range(1, 30)) = 25
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _distortion;
            float _scale;

            float _cintensity;
            float _nintensity;

            float _gscale;
            float _speed;
            float _contrast;
            #define MAX_OFFSET 0.12

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float rand(float2 pos)
            {
                return frac(sin(dot(pos + _Time.y, 
                    float2(12.9898f, 78.233f))) * 43758.5453123f);
            }

            float2 randUnitCircle(float2 pos)
            {
                const float PI = 3.14159265f;
                float randVal = rand(pos);
                float theta = 2.0f * PI * randVal;

                return float2(cos(theta), sin(theta));
            }

            float quinterp(float2 f)
            {
                return f*f*f * (f * (f * 6.0f - 15.0f) + 10.0f);
            }

            float noise (float2 uv) {
                float2 ipos = floor(uv);
                float2 fpos = frac(uv); 
                
                float o  = rand(ipos);
                float x  = rand(ipos + float2(1, 0));
                float y  = rand(ipos + float2(0, 1));
                float xy = rand(ipos + float2(1, 1));

                float2 smooth = smoothstep(0, 1, fpos);
                return lerp( lerp(o,  x, smooth.x), 
                             lerp(y, xy, smooth.x), smooth.y);
            }

            float fractal_noise (float2 uv) {
                float n = 0;
                // fractal noise is created by adding together "octaves" of a noise
                // an octave is another noise value that is half the amplitude and double the frequency of the previously added noise
                // below the uv is multiplied by a value double the previous. multiplying the uv changes the "frequency" or scale of the noise becuase it scales the underlying grid that is used to create the value noise
                // the noise result from each line is multiplied by a value half of the previous value to change the "amplitude" or intensity or just how much that noise contributes to the overall resulting fractal noise.

                n  = (1 / 2.0)  * noise( uv * 1);
                n += (1 / 4.0)  * noise( uv * 2); 
                n += (1 / 8.0)  * noise( uv * 4); 
                n += (1 / 16.0) * noise( uv * 8);
                
                return n;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                float3 color = 0;
                float2 uv = i.uv;

                float2 nUV = i.uv * _gscale;

                // create discrete lines by rounding value
                nUV.y = floor(nUV.yx);
                
                // the x component we'll use to sample the noise will change over time
                nUV.x = _Time.y * _speed - _Time.z * 15;

                // sample fractal noise using nUV
                float fn = fractal_noise(nUV);
                uv += float2(pow(fn, _contrast), 0);

                uv -= .5;

                float radius = pow(length(uv), 2);
                
                float distort = 1 + radius * (_distortion - .2);
                // / _Time.z * 15
                uv = uv * distort * _scale + .5;

                float modifier = length(uv * 2 - 1) * .5;
                float offset = MAX_OFFSET * _cintensity * modifier * (radius + .2);
                float r = tex2D(_MainTex, uv - offset).r;
                float g = tex2D(_MainTex, uv).g;
                float b = tex2D(_MainTex, uv + offset).b;

                // color = tex2D(_MainTex, uv);
                color = float3(r,g,b);
                


                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
