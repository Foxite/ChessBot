using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace ChessBot.Model;

public class BoardStreamEvent {
	public BoardStreamEventType Type { get; set; }
	[JsonIgnore]
	public JToken Token { get; set; }
}

public class GameFullBoardStreamEvent : BoardStreamEvent {
	public string Id { get; set; }
	public bool Rated { get; set; }
	public Variant Variant { get; set; }
	public Clock Clock { get; set; }
	// TODO enum
	// classical, correspondence,
	public string Speed { get; set; }
	public Perf Perf { get; set; }
	public long CreatedAt { get; set; }
	public Player White { get; set; }
	public Player Black { get; set; }
	// TODO enum
	// startpos,
	// Also, what is this?
	public string InitialFen { get; set; }
	public GameStateBoardStreamEvent State { get; set; }
}

public class Variant {
	public string Key { get; set; }
	public string Name { get; set; }
	public string Short { get; set; }
}

public class Clock {
	public int Initial { get; set; }
	public int Increment { get; set; }
}

public class Perf {
	public string Name { get; set; }
}

public class GameStateBoardStreamEvent : BoardStreamEvent {
	public string Moves { get; set; }
	public int WTime { get; set; }
	public int BTime { get; set; }
	public int WInc { get; set; }
	public int BInc { get; set; }
	// TODO enum
	// resign, started, 
	public string Status { get; set; }
	public BlackOrWhite? Winner { get; set; }
}

public class ChatLineBoardStreamEvent : BoardStreamEvent {
	public string Username { get; set; }
	public string Text { get; set; }
	public Room Room { get; set; }
}

public enum BoardStreamEventType {
	GameFull,
	GameState,
	ChatLine
}

public enum BlackOrWhite {
	Black, White
}

public enum Room {
	Player, Spectator
}
