uniform float uv_fade;
uniform float uv_alpha;
uniform int uv_simulationtimeDays;
uniform float uv_simulationtimeSeconds;

uniform sampler2D fit1;
uniform sampler2D fit2;
uniform sampler2D coast;
uniform sampler2D cmap;

uniform float simUseTime;
uniform bool simBindRealtime;
uniform bool simBindLimits;
uniform bool simSeasonal;

uniform vec4 p1min;
uniform vec4 p1max;
uniform vec4 p2min;
uniform vec4 p2max;
uniform float clim;
uniform float t0;
uniform float tmid;
uniform float tf;
uniform float smoothLength;
uniform float smoothN;

in vec3 Pos;

out vec4 FragColor;

const float PI = 3.1415926535897932384626433;

vec4 getColor(vec2 TexCoord, vec2 GradTexCoord, float t)
{
	vec4 p1t = textureGrad(fit1,TexCoord, dFdx(GradTexCoord), dFdy(GradTexCoord));
	vec4 p2t = textureGrad(fit2,TexCoord, dFdx(GradTexCoord), dFdy(GradTexCoord));

	vec4 p1 = p1t * (p1max - p1min) + p1min;	
	vec4 p2 = p2t * (p2max - p2min) + p2min;
	
	float tt0 = t0 - tmid;
	float f1 = p1.r*pow(t, 3.) + p1.g*pow(t, 2.) + p1.b*t + p1.a;
	float f10 = p1.r*pow(tt0, 3.) + p1.g*pow(tt0, 2.) + p1.b*tt0 + p1.a;

	float f2 = 0.;
	float f20 = 0.;
	if (simSeasonal){
		f2 = p2.r*sin(2.*PI*(t + p2.g));
		f20 = p2.r*sin(2.*PI*(tt0 + p2.g));
	}
	float cmwe = f1 + f2;
	float cmwe0 = f10 + f20;

	cmwe = cmwe - cmwe0;

	vec2 cTex = vec2(clamp((cmwe + clim)/clim/2., 0.01, 0.99),1);
	
	vec4 color = texture(cmap, cTex);

	return color;
}

void main()
{	
 
	vec3 positionOnUnitSphere = normalize(Pos)*vec3(-1,1,1);
	vec2 TexCoord = vec2 (atan(positionOnUnitSphere.x,positionOnUnitSphere.y)/(2*PI)-.25,1-acos(positionOnUnitSphere.z)/PI);
	TexCoord.x += 0.5;
	vec2 GradTexCoord = vec2 (atan(abs(positionOnUnitSphere.x),positionOnUnitSphere.y)/(2*PI),1-acos(positionOnUnitSphere.z)/PI);
	GradTexCoord.x += 0.5;
	vec4 coastLines = textureGrad(coast,TexCoord, dFdx(GradTexCoord), dFdy(GradTexCoord));
	
	
	float dayfract = uv_simulationtimeSeconds/(24.0*3600.0);
	float yrs = 365.2425;
	float years_0 = 1970. + (uv_simulationtimeDays + dayfract)/yrs;
	float univYr = clamp(years_0,0.0,13800.0);    

	float t = simUseTime - tmid;
	if (simBindRealtime){
		t = univYr - tmid;
	} 
	if (simBindLimits){
		t = clamp(t, t0 - tmid, tf - tmid);
	}

	TexCoord = vec2 (atan(positionOnUnitSphere.x,positionOnUnitSphere.y)/(2*PI)-.25,1-acos(positionOnUnitSphere.z)/PI);
	GradTexCoord = vec2 (atan(abs(positionOnUnitSphere.x),positionOnUnitSphere.y)/(2*PI),1-acos(positionOnUnitSphere.z)/PI);
	vec4 GRACEcolor = getColor(TexCoord, GradTexCoord, t);
	
	if (smoothN > 0){
		vec4 color = vec4(0);
		//take an average over many different regions
		for (int i=0; i<smoothN; i++){
			for (int j=0; j<smoothN; j++){
				TexCoord = vec2 (atan(positionOnUnitSphere.x,positionOnUnitSphere.y)/(2*PI)-.25,1-acos(positionOnUnitSphere.z)/PI);
				TexCoord.x += (i/smoothN - 0.5)*smoothLength;
				TexCoord.y += (j/smoothN - 0.5)*smoothLength;
				GradTexCoord = vec2 (atan(abs(positionOnUnitSphere.x),positionOnUnitSphere.y)/(2*PI),1-acos(positionOnUnitSphere.z)/PI);
				GradTexCoord.x += (i/smoothN - 0.5)*smoothLength;
				GradTexCoord.y += (j/smoothN - 0.5)*smoothLength;
		
				color += getColor(TexCoord, GradTexCoord, t);
			}
		}
		GRACEcolor = color/smoothN/smoothN;
	}

	GRACEcolor.a = uv_fade*uv_alpha;
			
	FragColor = GRACEcolor * coastLines;
}