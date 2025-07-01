Shader "Custom/DissolveWithOffset_Particle_MainTexOnly_Final"
{
  Properties
  {
    _MainTex ("Main Texture", 2D) = "white" {}
    _DissolveTex ("Dissolve Texture", 2D) = "white" {}
    _EdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
    _EdgeWidth ("Edge Width", Range(0, 0.5)) = 0.1
  }
  SubShader
  {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" }
    LOD 200

    Pass
    {
      Blend SrcAlpha OneMinusSrcAlpha 
      ZWrite Off 
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        float4 color : COLOR;
        float4 customData : TEXCOORD1;
      };

      struct v2f
      {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
        float4 color : COLOR;
        float dissolveAmount : TEXCOORD1; 
        float2 mainTexOffset : TEXCOORD2; 
      };

      sampler2D _MainTex;
      sampler2D _DissolveTex;
      float4 _EdgeColor;
      float _EdgeWidth;

      v2f vert (appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        o.color = v.color;

        
        o.dissolveAmount = v.customData.x;
        o.mainTexOffset = float2(v.customData.y, v.customData.z); 

        return o;
      }

      fixed4 frag (v2f i) : SV_Target
      {

        float2 uv = i.uv + i.mainTexOffset;

        
        fixed4 mainTex = tex2D(_MainTex, uv);

        
        fixed dissolveValue = tex2D(_DissolveTex, i.uv).r;

        
        if (mainTex.a == 0)
          discard;

       
        if (dissolveValue < i.dissolveAmount)
          discard; // 直接丢弃像素，确保完全透明

       
        if (dissolveValue < i.dissolveAmount + _EdgeWidth)
          return _EdgeColor;

        
        return mainTex * i.color;
      }
      ENDCG
    }
  }
  FallBack "Diffuse"
}