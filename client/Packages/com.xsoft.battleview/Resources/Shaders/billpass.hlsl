#ifndef BILL_PASS_INCLUDED
#define BILL_PASS_INCLUDED


#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex); //默认采样器
half4 _MainTex_ST;

struct Attributes
{
 float4 vertex    : POSITION;  // The vertex position in model space.
 float3 normal    : NORMAL;    // The vertex normal in model space.
 float4 texcoord  : TEXCOORD0; // The first UV coordinate.
 float4 texcoord1 : TEXCOORD2; // The second UV coordinate.
 float4 tangent   : TANGENT;   // The tangent vector in Model Space (used for normal mapping).
 float4 color     : COLOR;     // Per-vertex color
};

struct Varyings 
{ 
    float4   pos : SV_POSITION;
    float2   uv : TEXCOORD0;
    float4   clr : COLOR;
};


float _Disappear;

float _Life;
float _Speed;
float _Acce;
float _B;
float _C;

float _Delaytime;
float _Scaletime1;
float _Scaletime2;
float _Maxsize;
float _Endsize;

Varyings vert (Attributes input)
{
	Varyings o;
	Attributes v = input;

	float XinitSpeed = v.texcoord1.x;
	float YinitSpeed = v.texcoord1.y;

	float normaltime = v.normal.x;
	float fadetime = v.normal.y;
	float acceleration = v.normal.z;

	//About scaling process
	float delaytime = _Delaytime;
	float scaletime1 = _Scaletime1;
	float scaletime2 = _Scaletime2;
	float maxsize = _Maxsize;
	float endsize = _Endsize;

	float time = _Time.y - v.tangent.z;
	float fLifeSpan = normaltime + fadetime;
	
	if( time < fLifeSpan )
	{		
		float scale;

		int phasetime1 = (int)(time > delaytime);
		phasetime1 *=  (int)(time <(delaytime + scaletime1));

		int phasetime2 = (int)(time > (delaytime+scaletime1));
		phasetime2 *=  (int)(time <normaltime);

		int phasetime3 = (int)(time > (delaytime+scaletime1 + scaletime2));
		phasetime3 *=  (int)(time <fLifeSpan);
			
		float scaleP1;
		{
			float b = min(1.0/scaletime1,1000);					
			scaleP1 = maxsize * b * (time - delaytime);
		}

		float scaleP2;
		{
			float k = min((endsize - maxsize)/scaletime2,1000);
			scaleP2 = k*(time-delaytime-scaletime1)+maxsize;
		}
			
		scale = scaleP1 * phasetime1 + scaleP2 * phasetime2 + endsize * phasetime3;
		v.tangent.xy += v.tangent.xy * scale;


        float4 camspacePos =  mul(UNITY_MATRIX_V, v.vertex);

        camspacePos.xy += v.texcoord1.xy * time + time * time * acceleration;
        camspacePos = float4( v.tangent.xy + camspacePos.xy, camspacePos.z, 1);

        o.pos = mul(UNITY_MATRIX_P, camspacePos);
        
        o.clr = v.color;
		float alpha = 1.0 - pow( time / fLifeSpan, fadetime );
		int phasealpha = (int)(time>normaltime);
		o.clr.a = 1.0 * (1-phasealpha) + phasealpha*alpha ;
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
    }
    else
    {
        o.pos = float4( 0,0,0,0 );
	}
    return  o;
}



float4 frag (Varyings i) : SV_TARGET
{
    float4 c = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv);
    c *= i.clr;
    return c;
}

#endif