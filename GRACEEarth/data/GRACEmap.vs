in vec3 uv_vertexAttrib;
in vec2 uv_texCoordAttrib0;
uniform mat4 uv_modelViewProjectionMatrix;

out vec3 Pos;

void main()
{  
  Pos = 0.98 * uv_vertexAttrib;
  gl_Position = uv_modelViewProjectionMatrix*vec4(Pos,1.0);
}
