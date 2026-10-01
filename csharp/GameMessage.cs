using System.Text.Json.Serialization;

namespace Application;

/// <summary>
/// A world position on the game map. Use x and y for world coordinates. To convert to tile array indices, use i = y - map.origin.y and j = x - map.origin.x.
/// </summary>
public record WorldPosition(int X, int Y);

/// <summary>
/// A single tile on the visible map.
/// </summary>
/// <param name="Position">World position of this tile.</param>
/// <param name="Elevation">Elevation of this tile.</param>
/// <param name="IsImpassable">Whether this tile blocks movement and lava flow (mountains, crevasses).</param>
/// <param name="IsMountain">Whether this tile is a mountain and therefore a valid volcano target.</param>
/// <param name="HasLava">Whether this tile is covered in lava.</param>
public record Tile(WorldPosition Position, int Elevation, bool IsImpassable, bool IsMountain, bool HasLava);

/// <summary>
/// A meteor in flight that has not yet hit its target.
/// </summary>
/// <param name="Target">World position where the meteor will land.</param>
/// <param name="TurnsUntilImpact">Number of turns remaining before impact.</param>
/// <param name="ImpactedTiles">World positions of all tiles in the meteor's impact area. Dinosaurs occupying these tiles when the meteor hits are killed.</param>
public record Meteor(WorldPosition Target, int TurnsUntilImpact, WorldPosition[] ImpactedTiles);

/// <summary>
/// An active volcano producing lava.
/// </summary>
/// <param name="Position">World position of the erupting mountain tile.</param>
public record Volcano(WorldPosition Position);

/// <summary>
/// A dinosaur on the map.
/// </summary>
/// <param name="Id">Unique identifier for this dinosaur. This identifier remains unchanged across turns.</param>
/// <param name="Position">Current world position of the dinosaur.</param>
/// <param name="Age">Number of turns since this dinosaur spawned. Use this value with the decay curve to calculate the points awarded for eliminating it.</param>
/// <param name="Name">Name of this dinosaur. Dinosaurs sharing a name always behave the same way. See the documentation for details.</param>
public record Dinosaur(int Id, WorldPosition Position, int Age, string Name);

/// <summary>
/// The visible game map. Only tiles within the rolling window are exposed. Access tiles using array indices [i][j]. To convert world coordinates to array indices, use i = y - origin.y and j = x - origin.x.
/// </summary>
/// <param name="Width">Width of the visible frame in tiles. The column index j ranges from 0 to width - 1.</param>
/// <param name="Height">Height of the visible frame in tiles. The row index i ranges from 0 to height - 1.</param>
/// <param name="Origin">World position of the top-left corner of the visible frame. Use this position to convert between world coordinates (x, y) and array indices (i, j).</param>
/// <param name="Tiles">Grid of tiles in the visible frame. Access tiles using tiles[i][j], where i is the row index (y-axis) and j is the column index (x-axis).</param>
public record GameMap(int Width, int Height, WorldPosition Origin, Tile[][] Tiles);

/// <summary>
/// Game constants for this match. These values remain fixed throughout a match but may change between matches as the game is balanced.
/// </summary>
/// <param name="MaxTicks">Maximum number of turns in the match.</param>
/// <param name="MaxMeteors">Maximum number of meteors in flight simultaneously.</param>
/// <param name="MaxVolcanoes">Maximum number of active volcanoes simultaneously.</param>
/// <param name="MeteorDelay">Number of turns between meteor launch and impact.</param>
/// <param name="MeteorRadius">Radius of the meteor's impact area.</param>
/// <param name="MapShiftInterval">Number of turns between successive shifts of the visible map frame. The frame shifts on every turn whose number is a positive multiple of this value.</param>
/// <param name="WaveCadence">Number of turns between successive waves of dinosaurs.</param>
/// <param name="DinosPerWave">Number of dinosaurs in each wave before applying the limit on living dinosaurs.</param>
/// <param name="ActiveDinoCap">Maximum number of dinosaurs alive at any time. Waves only spawn enough to fill remaining slots.</param>
public record Constants(int MaxTicks, int MaxMeteors, int MaxVolcanoes, int MeteorDelay, int MeteorRadius, int MapShiftInterval, int WaveCadence, int DinosPerWave, int ActiveDinoCap);

/// <summary>
/// The full game state sent to the player each turn.
/// </summary>
/// <param name="Tick">Current turn number (starts at 1).</param>
/// <param name="CurrentTick">Same value as tick.</param>
/// <param name="Score">Current score.</param>
/// <param name="LastTickErrors">Errors from the previous turn, such as invalid actions.</param>
/// <param name="Constants">Game constants for this match.</param>
/// <param name="Map">The visible map frame. Only tiles inside the rolling window are exposed.</param>
/// <param name="Dinosaurs">All dinosaurs currently alive within the visible frame.</param>
/// <param name="Meteors">All meteors currently in flight with targets within the visible frame.</param>
/// <param name="Volcanoes">All active volcanoes within the visible frame.</param>
/// <param name="Mountains">World positions of all mountain tiles within the visible frame.</param>
/// <param name="Corpses">World positions of dinosaur corpses within the visible frame. Each position identifies a tile where a dinosaur died.</param>
public record TeamGameState(int Tick, int CurrentTick, int Score, string[] LastTickErrors, Constants Constants, GameMap Map, Dinosaur[] Dinosaurs, Meteor[] Meteors, Volcano[] Volcanoes, WorldPosition[] Mountains, WorldPosition[] Corpses);

[JsonDerivedType(typeof(LaunchMeteorAction))]
[JsonDerivedType(typeof(TriggerVolcanoAction))]
public abstract record Action(ActionType type);

public enum ActionType
{
    LaunchMeteor,
    TriggerVolcano,
}

/// <summary>
/// Launch a meteor at the target world position. The meteor hits after meteorDelay turns. At most maxMeteors meteors can be in flight at once.
/// </summary>
public record LaunchMeteorAction(WorldPosition Target) : Action(ActionType.LaunchMeteor);

/// <summary>
/// Trigger a volcanic eruption at the world position of a mountain tile. At most maxVolcanoes volcanoes can be active at once.
/// </summary>
public record TriggerVolcanoAction(WorldPosition Target) : Action(ActionType.TriggerVolcano);