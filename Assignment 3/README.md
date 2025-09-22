# **Render a 3D Cube using OpenTK**
- VBO + VAO + EBO for cube vertices
- Vertex & fragment shaders
- Correct MVP transformations

# **Deliverables**
- Source Code + Screenshot
<img width="624" height="514" alt="Screenshot 2025-09-22 135854" src="https://github.com/user-attachments/assets/4a132b5b-5b3b-42f0-8287-f829aee34f78" />

I used OpenTK library for this assignment.

I rendered the cube by defining 8 unique vertices. A Cube has 6 faces and each face is made up of 2 triangles. 6x2 = 12 triangles. Each triangle has 3 vertex indices. 12x3 = 36 indices.

          3 ----- 2           
          |       | This is the Back Face 
          |       |
          0 ----- 1

          7 ----- 6           
          |       | This is the Front Face 
          |       |
          4 ----- 5

So to create the left Square we would need to use 4,7,0,3. We would need to create two triangles to make a square. triangle 1 = 4,7,0 triangle 2 = 3,0,7

I used the VBO (Vertex Buffer Object) to store the 8 vertices and the color.

I used the EBO (Element Buffer Object) to store the 36 indices.

I used the VAO (Vertex Array Object) to store the attributes.

I used the vertex shader to apply the color and position. I used the fragment shader to color the pixels.

For the transformations I used the model matrix which is used to rotate the cube over time. The view matrix places where the camera is looking at the cube. The projection matrix makes the perspective of the 3D cube.


