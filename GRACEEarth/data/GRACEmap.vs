in vec3 uv_vertexAttrib;
in vec2 uv_texCoordAttrib0;
uniform mat4 uv_modelViewProjectionMatrix;

out vec2 TexCoord;

const mat3 RotMat = mat3(1,  0, 0,
						 0,  0, 1,
						 0, -1, 0);



void main()
{  
  gl_Position = uv_modelViewProjectionMatrix*vec4(0.98*RotMat*uv_vertexAttrib,1.0);
  TexCoord = vec2(0.75-uv_texCoordAttrib0.x,uv_texCoordAttrib0.y);
}