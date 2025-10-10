For this assignment 6 I mostly just reused my assignment 5 which was about "Implementing Phong Lighting in a Shader Using OpenTK" In that assignment we were asking to set up a basic camera and allow basic user interaction. With that being done already some of the exercises were already completed for assignment 6. I reviewed over the OpenTKReview Github "CameraSystem Branch": https://github.com/mouraleonardo/OpenTKReview/tree/CameraSystem to help me finish up the rest of the exercises. The camera uses the CreatePerspectiveFieldOfView and Matrix4.LookAt. The keyboard inputs allow movements using "wasd", "esc" to close the program. The mouse input controls uses pitch and yaw rotations with the pitch clamped to prevent flipping. The FOV is implemented using scroll wheel within a range of 30-90 degrees.

I would say the main challenges was understanding everything because we are given the github repository with everything coded basically a finished assignment 6 repository. I didn't want to just copy and paste the code provided by the professor so I had to read what each code does and see what I would need to use and how to use it to complete my assignment 6.

# How to Run the Project
- Clone or download the repository to your computer
- Open the solution in Visual Studio
- Make sure you have OpenTK and System.Drawing.Common and StbImageSharp NuGet package dependencies installed
- Build the project (dotnet build)
- Run the program (dotnet run)

# Sample output (using mouse wheel to make the image farther away)
<img width="1280" height="817" alt="Screenshot 2025-10-10 125859" src="https://github.com/user-attachments/assets/f5e963ba-3903-45f7-9fd4-06dc6a241f6e" />
