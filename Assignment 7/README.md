This project expands on the base SpriteGameOpenTK demo (https://github.com/mouraleonardo/SpriteGameOpenTk) by adding 2 new movement mechanics and a animation state machine. I added a jump and sprint mechanic by holding the shift key while moving. The state machines works for Idle, Walk, Sprint, and Jump. The player can only jump while grounded, and jump transitions back to idle when landing.


The spirtes added for both sprinting and jumping were used from this website: https://craftpix.net/freebies/free-pixel-art-tiny-hero-sprites/

# How to run the Project
- Clone or download the repository to your computer
- Open the solution in Visual Studio
- Make sure you have OpenTK and System.Drawing.Common NuGet package dependencies installed
- Build the project (dotnet build)
- Run the program (dotnet run)
