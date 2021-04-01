#version 330 core
out vec4 FragColor;

struct Material {
    sampler2D diffuse;
    sampler2D specular;    
    float shininess;
}; 

struct Light {
    vec3 position;  
    vec3 direction;
    float cutOff;
    float outerCutOff;
  
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
	
    float constant;
    float linear;
    float quadratic;
};

in vec3 FragPos;  
in vec3 Normal;  
in vec2 TexCoords;
  
uniform vec3 viewPos;
uniform Material material;
uniform Light light;
uniform sampler2D ies;

#define PI 3.1415926535897932384626433832795

vec3 getLumin(vec3 WorldPosition, vec3 LightPosition, vec3 LightDirection, vec3 LightUp) {
    vec3 LightTangent = normalize(cross(LightUp, LightDirection));
    vec3 LightBiTangent = normalize(cross(LightDirection, LightTangent));
    mat4 LightTransform = mat4(vec4(LightDirection.xyz, 0.0), vec4(LightTangent.xyz, 0.0), vec4(LightBiTangent.xyz, 0.0), vec4(0.0, 0.0, 0.0, 1.0));
    mat4 InvLightTransform = transpose(LightTransform);
    vec3 ToLight = normalize(LightPosition - WorldPosition);
    vec3 LocalToLight = (InvLightTransform * vec4(ToLight.xyz, 0.0)).xyz;
    
    float DotProd = dot(ToLight, LightDirection);
    float AngleV = asin(DotProd);
    
    // 0...1
    float NormAngleV = AngleV / PI + 0.5f;
    
    float AngleH = atan(-LocalToLight.y, -LocalToLight.z);
    float NormAngleH = AngleH / (PI * 2.f) + 0.5f;
    
    return texture(ies, vec2(NormAngleV, NormAngleH)).rgb;
}

void main()
{
    // ambient
    vec3 ambient = light.ambient * texture(material.diffuse, TexCoords).rgb;

    // diffuse 
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(light.position - FragPos);
    vec3 diffuseMutify = getLumin(FragPos, light.position, light.direction, vec3(0.f, 0.4f, 1.f));
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = 100 * diffuseMutify * diff * texture(material.diffuse, TexCoords).rgb;
    // 指定值
//  vec3 diffuse = light.diffuse * diff * texture(material.diffuse, TexCoords).rgb;
    
    // specular
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);  
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * spec * texture(material.specular, TexCoords).rgb;  
    
    // spotlight (soft edges)
    float theta = dot(lightDir, normalize(-light.direction)); 
    float epsilon = (light.cutOff - light.outerCutOff);
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
    diffuse  *= intensity;
    specular *= intensity;
    
    // attenuation
    float distance    = length(light.position - FragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    
    ambient  *= attenuation; 
    diffuse   *= attenuation;
    specular *= attenuation;   
        
    vec3 result = ambient + diffuse + specular;
    FragColor = vec4(result, 1.0);
} 