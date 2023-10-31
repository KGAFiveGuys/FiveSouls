// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MK4/Tessellation"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_MetallicGloss("Metallic Gloss", 2D) = "black" {}
		_Normalmap("Normalmap", 2D) = "bump" {}
		_AO("AO", 2D) = "white" {}
		_TessValue( "Max Tessellation", Range( 1, 32 ) ) = 10
		_TessMin( "Tess Min Distance", Float ) = 1
		_TessMax( "Tess Max Distance", Float ) = 2
		_DisplacementTexture("Displacement Texture", 2D) = "white" {}
		_Displacement("Displacement", Range( 0 , 1)) = 0
		_Detail("Detail", 2D) = "gray" {}
		_DetailNormal("Detail Normal", 2D) = "bump" {}
		_DetailNormalInt("Detail Normal Int", Range( 0 , 4)) = 1
		_DetailDisplacement("Detail Displacement", 2D) = "white" {}
		_DetailDisplInt("Detail Displ Int", Range( 0 , 1)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "Tessellation.cginc"
		#pragma target 4.6
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc tessellate:tessFunction 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _DetailDisplacement;
		uniform float4 _DetailDisplacement_ST;
		uniform float _DetailDisplInt;
		uniform sampler2D _DisplacementTexture;
		uniform float4 _DisplacementTexture_ST;
		uniform float _Displacement;
		uniform float _DetailNormalInt;
		uniform sampler2D _DetailNormal;
		uniform float4 _DetailNormal_ST;
		uniform sampler2D _Normalmap;
		uniform float4 _Normalmap_ST;
		uniform sampler2D _Detail;
		uniform float4 _Detail_ST;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform sampler2D _MetallicGloss;
		uniform float4 _MetallicGloss_ST;
		uniform sampler2D _AO;
		uniform float4 _AO_ST;
		uniform float _TessValue;
		uniform float _TessMin;
		uniform float _TessMax;

		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityDistanceBasedTess( v0.vertex, v1.vertex, v2.vertex, _TessMin, _TessMax, _TessValue );
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float2 uv_DetailDisplacement = v.texcoord * _DetailDisplacement_ST.xy + _DetailDisplacement_ST.zw;
			float4 lerpResult22 = lerp( float4( 1,1,1,0 ) , tex2Dlod( _DetailDisplacement, float4( uv_DetailDisplacement, 0, 0.0) ) , _DetailDisplInt);
			float2 uv_DisplacementTexture = v.texcoord * _DisplacementTexture_ST.xy + _DisplacementTexture_ST.zw;
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( ( ( lerpResult22 * tex2Dlod( _DisplacementTexture, float4( uv_DisplacementTexture, 0, 1.0) ) ) * float4( ase_vertexNormal , 0.0 ) ) * _Displacement ).rgb;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_DetailNormal = i.uv_texcoord * _DetailNormal_ST.xy + _DetailNormal_ST.zw;
			float2 uv_Normalmap = i.uv_texcoord * _Normalmap_ST.xy + _Normalmap_ST.zw;
			o.Normal = BlendNormals( UnpackScaleNormal( tex2D( _DetailNormal, uv_DetailNormal ), _DetailNormalInt ) , UnpackNormal( tex2D( _Normalmap, uv_Normalmap ) ) );
			float2 uv_Detail = i.uv_texcoord * _Detail_ST.xy + _Detail_ST.zw;
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 blendOpSrc11 = tex2D( _Detail, uv_Detail );
			float4 blendOpDest11 = tex2D( _Albedo, uv_Albedo );
			o.Albedo = ( saturate( (( blendOpDest11 > 0.5 ) ? ( 1.0 - ( 1.0 - 2.0 * ( blendOpDest11 - 0.5 ) ) * ( 1.0 - blendOpSrc11 ) ) : ( 2.0 * blendOpDest11 * blendOpSrc11 ) ) )).rgb;
			float2 uv_MetallicGloss = i.uv_texcoord * _MetallicGloss_ST.xy + _MetallicGloss_ST.zw;
			float4 tex2DNode7 = tex2D( _MetallicGloss, uv_MetallicGloss );
			o.Metallic = tex2DNode7.r;
			o.Smoothness = tex2DNode7.a;
			float2 uv_AO = i.uv_texcoord * _AO_ST.xy + _AO_ST.zw;
			o.Occlusion = tex2D( _AO, uv_AO ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16301
62;42;1541;991;2146.872;139.3404;1.288577;True;True
Node;AmplifyShaderEditor.RangedFloatNode;20;-1631.816,373.7661;Float;False;Property;_DetailDisplInt;Detail Displ Int;15;0;Create;True;0;0;False;0;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;17;-1615.754,133.2368;Float;True;Property;_DetailDisplacement;Detail Displacement;14;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;22;-1288.82,280.7888;Float;False;3;0;COLOR;1,1,1,0;False;1;COLOR;1,1,1,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1;-1147.402,443.0103;Float;True;Property;_DisplacementTexture;Displacement Texture;9;0;Create;True;0;0;False;0;None;9789d23040cb1fb45ad60392430c3c15;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;3;-831.7378,681.8633;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;21;-1100.714,-25.22086;Float;False;Property;_DetailNormalInt;Detail Normal Int;13;0;Create;True;0;0;False;0;1;1;0;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-637.2338,420.9692;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;8;-790.035,-275.8105;Float;True;Property;_Normalmap;Normalmap;2;0;Create;True;0;0;False;0;None;16bdd88fdbd8b8f40b9969d74e5dded5;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;12;-522.9622,-596.3632;Float;True;Property;_Detail;Detail;11;0;Create;True;0;0;False;0;None;None;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;14;-432.428,-248.0798;Float;True;Property;_DetailNormal;Detail Normal;12;0;Create;True;0;0;False;0;None;16bdd88fdbd8b8f40b9969d74e5dded5;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-791.2933,-475.7702;Float;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;None;567a9c450471c4e4eabc5483afee6380;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-502.7381,568.8633;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-598.7378,761.8633;Float;False;Property;_Displacement;Displacement;10;0;Create;True;0;0;False;0;0;0.3;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;7;-790.5185,-83.824;Float;True;Property;_MetallicGloss;Metallic Gloss;1;0;Create;True;0;0;False;0;None;2453c9ea226d00347885dc5feffd6156;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-297.7381,588.8633;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendNormalsNode;13;-253.3163,-64.03561;Float;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;9;-793.0336,111.8946;Float;True;Property;_AO;AO;3;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;11;-205.4843,-345.3632;Float;False;Overlay;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;6;Float;ASEMaterialInspector;0;0;Standard;MK4/Tessellation;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;True;0;10;1;2;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;4;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;22;1;17;0
WireConnection;22;2;20;0
WireConnection;19;0;22;0
WireConnection;19;1;1;0
WireConnection;14;5;21;0
WireConnection;5;0;19;0
WireConnection;5;1;3;0
WireConnection;6;0;5;0
WireConnection;6;1;4;0
WireConnection;13;0;14;0
WireConnection;13;1;8;0
WireConnection;11;0;12;0
WireConnection;11;1;2;0
WireConnection;0;0;11;0
WireConnection;0;1;13;0
WireConnection;0;3;7;0
WireConnection;0;4;7;4
WireConnection;0;5;9;0
WireConnection;0;11;6;0
ASEEND*/
//CHKSM=55BB8FBD87B31177C744F8881DACE148B71DBC2F