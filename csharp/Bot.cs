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
    public WorldPosition youngestDinoCurrPos = new(0, 0);
    public WorldPosition youngestDinoPrevPos = new(0, 0);
    public int youngestDinoCurrId = -1;

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
       

        WorldPosition leftestMountain = null;
        
        if (gameMessage.Volcanoes.Length < gameMessage.Constants.MaxVolcanoes && gameMessage.Mountains.Length > 0)
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

            leftestMountain = new WorldPosition(currentX, maxY);
            if (leftestMountain != null)
                actions.Add(new TriggerVolcanoAction(leftestMountain));
        }

        if (youngestDinoCurrId == -1)
        {
            int youngestDinoAge = int.MaxValue;
            foreach (var dinosaur in gameMessage.Dinosaurs)
            {
                if (dinosaur.Age < youngestDinoAge)
                {
                    youngestDinoCurrId = dinosaur.Id;
                }
            }
        }
        var currDino = gameMessage.Dinosaurs.FirstOrDefault(dinosaur => dinosaur.Id == youngestDinoCurrId);
        
        youngestDinoCurrPos = currDino?.Position;
        if (youngestDinoCurrPos != null)
        {
            if (met1ready)
            {
                actions.Add(new LaunchMeteorAction(youngestDinoCurrPos));
                met1ready = false;
                met1counter = 0;
            }
            else if (met2ready)
            {
                var metLauchPos = new WorldPosition(0, 0);
                if (youngestDinoCurrPos.Y > youngestDinoPrevPos.Y)
                {
                    metLauchPos = new WorldPosition(youngestDinoCurrPos.X, youngestDinoCurrPos.Y + 2);
                }
                else if (youngestDinoCurrPos.Y < youngestDinoPrevPos.Y)
                {
                    metLauchPos = new WorldPosition(youngestDinoCurrPos.X, youngestDinoCurrPos.Y - 2);
                }
                else if (youngestDinoCurrPos.X > youngestDinoPrevPos.X)
                {
                    metLauchPos = new WorldPosition(youngestDinoCurrPos.X + 2, youngestDinoCurrPos.Y);
                }
                else
                {
                    metLauchPos = new WorldPosition(youngestDinoCurrPos.X - 2, youngestDinoCurrPos.Y);
                }
                actions.Add(new LaunchMeteorAction(metLauchPos));
                met2ready = false;
                met2counter = 0;
            }
            else if (met3ready)
            {
                var metLauchPos = new WorldPosition(0, 0);
                if (youngestDinoCurrPos.Y > youngestDinoPrevPos.Y)
                {
                    metLauchPos = new WorldPosition(youngestDinoCurrPos.X, youngestDinoCurrPos.Y + 2);
                }
                else if (youngestDinoCurrPos.Y < youngestDinoPrevPos.Y)
                {
                    metLauchPos = new WorldPosition(youngestDinoCurrPos.X, youngestDinoCurrPos.Y - 2);
                }
                else if (youngestDinoCurrPos.X > youngestDinoPrevPos.X)
                {
                    metLauchPos = new WorldPosition(youngestDinoCurrPos.X + 2, youngestDinoCurrPos.Y);
                }
                else
                {
                    metLauchPos = new WorldPosition(youngestDinoCurrPos.X - 2, youngestDinoCurrPos.Y);
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
                    youngestDinoCurrId = -1;
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
            youngestDinoPrevPos = youngestDinoCurrPos;
        }
        else
        {
            youngestDinoPrevPos = new WorldPosition(0, 0);
            youngestDinoCurrPos = new WorldPosition(0, 0);
            youngestDinoCurrId = -1;
        }
        // You can clearly do better than this. Have fun!!
        return actions;
    }
}
