using System;
using OpenTK.Mathematics;

class Program {

    static void Main() {

        Vector3 vector1 = new Vector3(1, 2, 3); // The first 3D vector
        Vector3 vector2 = new Vector3(4, 5, 6); // The second 3D vector

        float dot = Vector3.Dot(vector1, vector2); // dot product
        Vector3 cross = Vector3.Cross(vector1, vector2); // cross product

        Vector3 add = vector1 + vector2; // addition
        Vector3 subtract = vector1 - vector2; // subtraction

        // Normalize the vectors
        Vector3 normalizedV1 = vector1.Normalized(); 
        Vector3 normalizedV2 = vector2.Normalized();

        Console.WriteLine("-Vector Operations-");
        Console.WriteLine($"vector1: {vector1}, vector2: {vector2}");
        Console.WriteLine($"Dot Product: {dot}");
        Console.WriteLine($"Cross Product: {cross}");
        Console.WriteLine($"Vector1 Normalized: {normalizedV1}");
        Console.WriteLine($"Vector2 Normalized: {normalizedV2}");
        Console.WriteLine($"Vector1 + Vector2: {add}");
        Console.WriteLine($"Vector1 - Vector2: {subtract}");

        // Matrix Transformation
        Matrix4 identity = Matrix4.Identity; // matrix 4x4
        Matrix4 scale = Matrix4.CreateScale(2f); // scale matrix
        Matrix4 rotation = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(45f)); // rotation matrix around X-axis by 45 degrees
        Matrix4 translation = Matrix4.CreateTranslation(5f, 0f, 0f); // translation matrix to move alond X-axis by 5 units
        Matrix4 transform = scale * rotation * translation; // combine trnasformations
        Vector4 point = new Vector4(1, 0, 0, 1); // apply to a point
        Vector4 result = point * transform; // combining transformation to point

        Console.WriteLine($"-Matrix Operations-");
        Console.WriteLine($"Identity: {identity}");
        Console.WriteLine($"Original Point: {point}");
        Console.WriteLine("Transformed Point: " + result);
        Console.WriteLine($"Scale: {scale}");
        Console.WriteLine($"Rotation: {rotation}");

        // Quaternion (was not listed in the assignment 2 but was listed to do in the "#D Math Fundamentals pdf)
        Quaternion q = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(90f)); // Quaternion 90 degree rotation from Y-Axis

        Vector3 qPoint = new Vector3(1, 0, 0); // vector to rotate
        Vector3 rotated = Vector3.Transform(qPoint, q); // quaternion rotation to vector

        Console.WriteLine($"-Quaternion-");
        Console.WriteLine($"Original Point: {qPoint}");
        Console.WriteLine($"Rotated Point: {rotated}");


    }
}