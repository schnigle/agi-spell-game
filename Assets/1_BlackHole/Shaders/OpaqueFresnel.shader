// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:Standard,iptp:0,cusa:False,bamd:0,cgin:,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3527,x:33532,y:32629,varname:node_3527,prsc:2|emission-8678-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:8820,x:32343,y:32595,ptovrint:False,ptlb:MainTexture,ptin:_MainTexture,varname:node_8820,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:7482,x:32531,y:32595,varname:node_7482,prsc:2,ntxv:0,isnm:False|TEX-8820-TEX;n:type:ShaderForge.SFN_VertexColor,id:8033,x:32531,y:32734,varname:node_8033,prsc:2;n:type:ShaderForge.SFN_Fresnel,id:6210,x:32526,y:32969,varname:node_6210,prsc:2|EXP-9289-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9289,x:32526,y:33121,ptovrint:False,ptlb:FresnelValue,ptin:_FresnelValue,varname:node_9289,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Power,id:7573,x:32690,y:32969,varname:node_7573,prsc:2|VAL-6210-OUT,EXP-7041-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7041,x:32526,y:32897,ptovrint:False,ptlb:FresnelPower,ptin:_FresnelPower,varname:node_7041,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Add,id:8678,x:33124,y:32625,varname:node_8678,prsc:2|A-4075-RGB,B-3870-OUT;n:type:ShaderForge.SFN_Multiply,id:8343,x:32879,y:32946,varname:node_8343,prsc:2|A-8033-RGB,B-7573-OUT;n:type:ShaderForge.SFN_Color,id:4075,x:32773,y:32554,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_4075,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Multiply,id:3870,x:33048,y:32946,varname:node_3870,prsc:2|A-8343-OUT,B-1303-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1303,x:32897,y:33151,ptovrint:False,ptlb:FresnelIntensity,ptin:_FresnelIntensity,varname:node_1303,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1.5;proporder:8820-9289-7041-4075-1303;pass:END;sub:END;*/

Shader "Custom/OpaqueFresnel" {
    Properties {
        _MainTexture ("MainTexture", 2D) = "white" {}
        _FresnelValue ("FresnelValue", Float ) = 1
        _FresnelPower ("FresnelPower", Float ) = 1
        _Color ("Color", Color) = (0,0,0,1)
        _FresnelIntensity ("FresnelIntensity", Float ) = 1.5
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        LOD 200
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float _FresnelValue;
            uniform float _FresnelPower;
            uniform float4 _Color;
            uniform float _FresnelIntensity;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(2)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
////// Lighting:
////// Emissive:
                float3 emissive = (_Color.rgb+((i.vertexColor.rgb*pow(pow(1.0-max(0,dot(normalDirection, viewDirection)),_FresnelValue),_FresnelPower))*_FresnelIntensity));
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Standard"
    CustomEditor "ShaderForgeMaterialInspector"
}
