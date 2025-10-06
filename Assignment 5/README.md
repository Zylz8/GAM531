# Assignment 5 - Implement Phong Lighting in a Shader Using OpenTK

# Libraries Used for this Assignment
- OpenTK.Mathematics
- OpenTK.Graphics
- System.Drawing.Common

# Brief Report
I reused most of the code I used for assignment 4 like the LoadTexture function with my crate.jepg. I changed my vertices function to now print out the positions, texture coordinates (u,v), and now the normals. I updated both the vertex and fragment shader codes with the one provided in the assignment 5 tab on blackboard. I added mouse movement with two angles horizontal and vertical using the names "hori"(Controls the left and right) and "vert"(Controls the up and down) which are used to determine where the camera is looking at. I included a sensitivity control to make the movement smooth. I added in keyboard controls to allow dynamic light movement along both the X and Z axes using the control "wasd" (w is up, a is left, s is down, d is right). 

I would say some of the challenges I faced was probably figuring out the how to add the lighting with the keyboard controls and also the movement with the mouse. I added a function called OnMouseMove() for the mouse movement and updated the OnUpdateFrame() function for the keyboard controls.


# How to run the Project
- Clone or download the repository to your computer
- Open the solution in Visual Studio
- Make sure you have OpenTK and System.Drawing.Common NuGet package dependencies installed
- Build the project (dotnet build)
- Run the program (dotnet run)

# Sample Output:
<img width="725" height="623" alt="Screenshot 2025-10-06 110821" src="https://github.com/user-attachments/assets/843238df-d8c2-449f-a758-c5a2cbf806ad" />


