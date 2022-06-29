#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED

#define MIN_REFLECTIVITY 0.04


struct BRDF  {
    half3 diffuse;
    half3 specular;
    half roughness;
};

half OneMinusReflectivity(half metallic)
{
    half range = 1.0 - MIN_REFLECTIVITY;
    return range - metallic * range;
}


BRDF GetBRDF (Surface surface)
{
    BRDF brdf;
    half oneMinusReflectivity = OneMinusReflectivity(surface.metallic);
    
    brdf.diffuse = surface.color * oneMinusReflectivity;
    #if defined (_PREMULTIPLY_ALPHA)
        brdf.diffuse *= surface.alpha;
    #endif
    brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);
    half  perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness); //(1-s)
    brdf.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);//pr* pr
    return brdf;
}

half SpecularStrength (Surface surface, BRDF brdf, Light light)
{
    float3 h = SafeNormalize(light.direction + surface.viewDirection);
    half NdotH =saturate(dot(surface.normal, h));
    half NdotH2 = Square(NdotH);
    half LdotH = saturate(dot(light.direction, h));
    half LdotH2 = Square(LdotH);
    half r2 = Square(brdf.roughness);
    half d2 = Square(NdotH2 * (r2 - 1.0) + 1.00001);
    half normalization = brdf.roughness * 4.0 + 2.0;
    return r2 / (d2 * max(0.1, LdotH2) * normalization);

}

half3 DirectBRDF (Surface surface, BRDF brdf, Light light)
{
    return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}


#endif


