Shader "Tutorial/VertexLit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			   Material 
			   {
			        Diffuse [_Color]
			        Ambient [_Color]
			        Shininess [_Shininess]
			        Specular [_SpecColor]
			        Emission [_Emission]
    			}		
   				Lighting On
    			SeparateSpecular On
    			SetTexture [_MainTex] 
    			{
        			constantColor [_Color]
        			Combine texture * primary DOUBLE, texture * constant
    			}
		}
	}
	FallBack "VertexLit"
}