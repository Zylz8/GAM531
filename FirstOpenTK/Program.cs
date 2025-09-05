using System;
using FirstOpenTK;
//using FirstOpenTk;


using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FirstOpenTK {
    // Main Entry Point for C# Console
    class Program {
        static void Main(String[] args) {
            using (Game game = new Game()) {
                game.Run();
            }
        }
    }
}