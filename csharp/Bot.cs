namespace Application;

public class Bot
{
    public const string NAME = "My cool C# bot";

    /// <summary>
    /// This method should be used to initialize some variables you will need throughout the game.
    /// </summary>
    public Bot()
    {
        Console.WriteLine("Initializing your super mega duper bot");
    }

    /// <summary>
    /// Here is where the magic happens, for now the moves are not very good. I bet you can do better ;)
    /// </summary>
    public IEnumerable<Action> GetNextMoves(TeamGameState gameMessage)
    {
        var actions = new List<Action>();

        // The engine applies only ONE action per turn (actions[0]); extra actions are ignored and reported in lastTickErrors.
        // Baseline strategy: grab the first dinosaur we can see and drop a meteor right on top of it. This is intentionally simple and far from optimal: it just shows the shape of a working bot.
        var firstDinosaur = gameMessage.Dinosaurs.FirstOrDefault();
        WorldPosition rightestMountain = null;
        if (gameMessage.Volcanoes.Length <= 0)
        {
            var maxY = int.MinValue;
            var currentX = 0;
            foreach (var mountain in gameMessage.Mountains)
            {
                if (mountain.Y > maxY)
                {
                    maxY = mountain.Y;
                    currentX =  mountain.X;
                }
            }

            rightestMountain = new WorldPosition(currentX, maxY);
        }

        if (firstDinosaur != null)
        {
            if (gameMessage.Meteors.Length <= 0)
            {
                actions.Add(new LaunchMeteorAction(firstDinosaur.Position));
            }
            if (rightestMountain != null)
                actions.Add(new TriggerVolcanoAction(rightestMountain));
        }

        // You can clearly do better than this. Have fun!!
        return actions;
    }
}
