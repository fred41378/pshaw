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
        
        WorldPosition bestMountain = null;
        
        if (gameMessage.Volcanoes.Length <= 0)
        {
            var maxScore = int.MinValue;
            foreach (var mountain in gameMessage.Mountains)
            {
                int volcanoScore = GetVolcanoPotentialScore(mountain, gameMessage);
                if (volcanoScore > maxScore)
                {
                    maxScore = volcanoScore;
                    bestMountain = mountain;
                }
            }

            if (maxScore < 10)
            {
                bestMountain = null;
            }
        }

        if (firstDinosaur != null)
        {
            if (gameMessage.Meteors.Length <= 0)
            {
                actions.Add(new LaunchMeteorAction(firstDinosaur.Position));
            }
            if (bestMountain != null)
                actions.Add(new TriggerVolcanoAction(bestMountain));
        }

        // You can clearly do better than this. Have fun!!
        return actions;
    }

    public int GetVolcanoPotentialScore(WorldPosition pos, TeamGameState gameMessage)
    {
        const int POSITION_MULTIPLIER = 1;
        const int ELEVATION_MULTIPLIER = 1;

        GameMap map = gameMessage.Map;

        var mountainTile = GetTile(gameMessage, pos);
        
        if (mountainTile == null)
            return 0;


        int deduction = 0;

        for (int i = -1; i < 1; ++i)
        {
            for (int j = -1; j < 1; ++j)
            {
                if (i == 0 && j == 0)
                    continue;

                var neighbourTile = GetTile(gameMessage, new WorldPosition(pos.X + i, pos.Y + j));

                if (neighbourTile != null && neighbourTile.IsImpassable)
                    deduction += 5;
            }
        }

        return POSITION_MULTIPLIER * pos.Y +
               - Math.Abs(pos.Y - (int)(map.Height * 0.5f)) + 
               ELEVATION_MULTIPLIER * mountainTile.Elevation
               - deduction;
    }

    public Tile? GetTile(TeamGameState gameMessage, WorldPosition worldPosition)
    {
        GameMap map = gameMessage.Map;

        int tileX = worldPosition.X - map.Origin.X;
        int tileY = worldPosition.Y - map.Origin.Y;

        if (tileX < 0 || tileX >= 20 || tileY < 0 || tileY >= 20)
            return null;

        return map.Tiles[tileY][tileX];
    }
}