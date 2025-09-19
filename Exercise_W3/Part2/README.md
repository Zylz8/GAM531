**Render a Square with an Index Buffer (EBO)**
- Define 4 unique vertices
- Use EBO with indices [0,1,2, 0,2,3]
- Explain how indices avoid repetition

Indices avoid repetition because without using indices/EBO you would need 6 vertices for 2 triangles basically duplicating the triangles vertices. But by using EBO you only need 4 unique vertices and you tell OpenGL how to connect the vertices.
