# Assignment 4 - Add a texture to a 3D Cube

Sample Output:
<img width="935" height="656" alt="image" src="https://github.com/user-attachments/assets/697162f7-75bf-41b8-a37b-a4696674d917" />

The libraries used are OpenTK.Mathematics, OpenTK.Graphics, and System.Drawing.Common

# How to render the Cube
With every face of the cube we added a (u, v) to map the each corner of the cube with a texture.
The example I used was this:
<img width="1103" height="558" alt="image" src="https://github.com/user-attachments/assets/9fa3ae87-cb8f-4de2-b033-0a58a40302e0" />

I used the VBO (Vertex Buffer Object) to store the the vertices and u,v.

I used the EBO (Element Buffer Object) to store the indices.

I used the VAO (Vertex Array Object) to store the attributes.

I changed the vertex shader to now pass the coordinates to the fragment shader.

I used the LoadTexture(string path) function which I got from the example repository to load my jpeg.

rotate cube around X axis and rotates the cube aroud the Y axis at half the speed, Rotates around both the X and Y axis

                Matrix4 model = Matrix4.CreateRotationX(angle) * Matrix4.CreateRotationY(angle * 0.5f); // rotate cube around X axis and rotates the cube aroud the Y axis at half the speed
            // Rotates around both the X and Y axis

# How to run
- Clone or download the repository to your computer
- Open the solution in Visual Studio
- Make sure you have OpenTK and System.Drawing.Common NuGet package dependencies installed
- Build the project (dotnet build)
- Run the program (dotnet run)
