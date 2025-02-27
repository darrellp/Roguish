using Roguish;
using SadConsole.Configuration;

Settings.WindowTitle = "My SadConsole Game";

Builder gameConfig = GameSettings.SetupGame();


Game.Create(gameConfig);
Game.Instance.Run();
Game.Instance.Dispose();

