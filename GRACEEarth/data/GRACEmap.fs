uniform float uv_fade;
uniform float uv_alpha;
uniform int uv_simulationtimeDays;
uniform float uv_simulationtimeSeconds;

uniform sampler2D fit1;
uniform sampler2D fit2;
uniform sampler2D coast;

uniform float simUseTime;
uniform bool simBindRealtime;
uniform bool simBindLimits;
uniform bool simSeasonal;

uniform vec3 p1min;
uniform vec3 p1max;
uniform vec3 p2min;
uniform vec3 p2max;
uniform float clim;
uniform float t0;
uniform float tf;
uniform vec3 minColor;
uniform vec3 maxColor;

in vec2 TexCoord;

out vec4 FragColor;

const float PI = 3.1415926535897932384626433;

void main()
{	
	vec3 p1t = texture(fit1,TexCoord).rgb;
	vec3 p2t = texture(fit2,TexCoord).rgb;
		vec4 coastLines = texture(coast,TexCoord);
	
	vec3 p1 = p1t * (p1max - p1min) + p1min;	
	vec3 p2 = p2t * (p2max - p2min) + p2min;
	
	float dayfract = uv_simulationtimeSeconds/(24.0*3600.0);
	float yrs = 365.2425;
	float years_0 = 1970. + (uv_simulationtimeDays + dayfract)/yrs;
	float univYr = clamp(years_0,0.0,13800.0);    

	float t = simUseTime - t0;
	if (simBindRealtime){
		t = univYr - t0;
	} 
	if (simBindLimits){
		t = clamp(t, 0., tf - t0);
	}

	float f1 = p1.r*pow(t, 3.) + p1.g*pow(t, 2.) + p1.b*t + p2.b;
	float f10 = p2.b;

	float f2 = 0.;
	float f20 = 0.;
	if (simSeasonal){
		f2 = p2.r*sin(2.*PI*(t + p2.g));
		f20 = p2.r*sin(2.*PI*p2.g);
	}
	float cmwe = f1 + f2;
	float cmwe0 = f10 + f20;

	cmwe = cmwe;// - cmwe0;

	vec4 GRACEcolor = vec4(1.);
	if (cmwe > 0.){
		GRACEcolor.rgb = mix(vec3(1.,1., 1.), maxColor, clamp(cmwe/clim,0.,1.));
	} 
	if (cmwe < 0.){
		GRACEcolor.rgb = mix(vec3(1.,1., 1.), minColor, clamp(-1.*cmwe/clim,0.,1.));

	}
	
	GRACEcolor.a = uv_fade*uv_alpha;// * cmweN ;
	
	//GRACEcolor.b = cmweN;
		
	FragColor = GRACEcolor * coastLines;
}