namespace Application;

public class Bot
{
    public const string NAME = "My cool C# bot";
    public bool met1ready = true;
    public bool met2ready = true;
    public bool met3ready = true;
    public int met1counter = 0;
    public int met2counter = 0;
    public int met3counter = 0;
    public WorldPosition targetDinoCurrPos = new(0, 0);
    public WorldPosition targetDinoPrevPos = new(0, 0);
    public int targetDinoCurrId = -1;

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
       

        WorldPosition bestMountain = null;
        
        if (gameMessage.Volcanoes.Length < gameMessage.Constants.MaxVolcanoes && gameMessage.Mountains.Length > 0)
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

            if (maxScore < 4)
            {
                bestMountain = null;
            }
        }

        if (targetDinoCurrId == -1)
        {
            int youngestDinoAge = int.MaxValue;
            foreach (var dinosaur in gameMessage.Dinosaurs)
            {
                if (dinosaur.Age < youngestDinoAge)
                {
                    targetDinoCurrId = dinosaur.Id;
                }
            }
            if (bestMountain != null)
                actions.Add(new TriggerVolcanoAction(bestMountain));
        }
        var currDino = gameMessage.Dinosaurs.FirstOrDefault(dinosaur => dinosaur.Id == targetDinoCurrId);
        
        targetDinoCurrPos = currDino?.Position;
        if (targetDinoCurrPos != null)
        {
            if (met1ready)
            {
                actions.Add(new LaunchMeteorAction(targetDinoCurrPos));
                met1ready = false;
                met1counter = 0;
            }
            else if (met2ready)
            {
                var metLauchPos = new WorldPosition(0, 0);
                if (targetDinoCurrPos.Y > targetDinoPrevPos.Y)
                {
                    metLauchPos = new WorldPosition(targetDinoCurrPos.X, targetDinoCurrPos.Y + 2);
                }
                else if (targetDinoCurrPos.Y < targetDinoPrevPos.Y)
                {
                    metLauchPos = new WorldPosition(targetDinoCurrPos.X, targetDinoCurrPos.Y - 2);
                }
                else if (targetDinoCurrPos.X > targetDinoPrevPos.X)
                {
                    metLauchPos = new WorldPosition(targetDinoCurrPos.X + 2, targetDinoCurrPos.Y);
                }
                else
                {
                    metLauchPos = new WorldPosition(targetDinoCurrPos.X - 2, targetDinoCurrPos.Y);
                }
                actions.Add(new LaunchMeteorAction(metLauchPos));
                met2ready = false;
                met2counter = 0;
            }
            else if (met3ready)
            {
                var metLauchPos = new WorldPosition(0, 0);
                if (targetDinoCurrPos.Y > targetDinoPrevPos.Y)
                {
                    metLauchPos = new WorldPosition(targetDinoCurrPos.X, targetDinoCurrPos.Y + 2);
                }
                else if (targetDinoCurrPos.Y < targetDinoPrevPos.Y)
                {
                    metLauchPos = new WorldPosition(targetDinoCurrPos.X, targetDinoCurrPos.Y - 2);
                }
                else if (targetDinoCurrPos.X > targetDinoPrevPos.X)
                {
                    metLauchPos = new WorldPosition(targetDinoCurrPos.X + 2, targetDinoCurrPos.Y);
                }
                else
                {
                    metLauchPos = new WorldPosition(targetDinoCurrPos.X - 2, targetDinoCurrPos.Y);
                }
                actions.Add(new LaunchMeteorAction(metLauchPos));
                met3ready = false;
                met3counter = 0;
            }

            if (!met1ready)
            {
                met1counter++;
                if (met1counter > gameMessage.Constants.MeteorDelay)
                {
                    met1ready = true;
                    targetDinoCurrId = -1;
                }
            }
            if (!met2ready)
            {
                met2counter++;
                if (met2counter > gameMessage.Constants.MeteorDelay)
                {
                    met2ready = true;
                }
            }
            if (!met3ready)
            {
                met3counter++;
                if (met3counter > gameMessage.Constants.MeteorDelay)
                {
                    met3ready = true;
                }
            }
            targetDinoPrevPos = targetDinoCurrPos;
        }
        else
        {
            targetDinoPrevPos = new WorldPosition(0, 0);
            targetDinoCurrPos = new WorldPosition(0, 0);
            targetDinoCurrId = -1;
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
                    deduction += 1;
            }
        }

        if (deduction >= 4)
            return 0;

        GetRelativePosition(gameMessage, pos, out int relativeX, out int relativeY);

        return POSITION_MULTIPLIER * relativeY +
               - Math.Abs(relativeX - (int)(map.Height * 0.5f)) + 
               ELEVATION_MULTIPLIER * mountainTile.Elevation
               - deduction;
    }

    public void GetRelativePosition(TeamGameState gameMessage, WorldPosition worldPosition, out int x, out int y)
    {
        GameMap map = gameMessage.Map;

        x = worldPosition.X - map.Origin.X;
        y = worldPosition.Y - map.Origin.Y;
    }

    public Tile? GetTile(TeamGameState gameMessage, WorldPosition worldPosition)
    {
        GameMap map = gameMessage.Map;

        GetRelativePosition(gameMessage, worldPosition, out int tileX, out int tileY);

        if (tileX < 0 || tileX >= 20 || tileY < 0 || tileY >= 20)
            return null;

        return map.Tiles[tileY][tileX];
    }
}