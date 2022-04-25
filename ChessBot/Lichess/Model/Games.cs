namespace ChessBot.Model; 

public class OngoingGame {
	public string GameId { get; set; }
	public string FullId { get; set; }
	public BlackOrWhite Color { get; set; }
	public string Fen { get; set; }
	public bool HasMoved { get; set; }
	public bool IsMyTurn { get; set; }
	public string LastMove { get; set; }
	public Player Opponent { get; set; }
	public string Perf { get; set; }
	public bool Rated { get; set; }
	public int SecondsLeft { get; set; }
	public string Source { get; set; }
	public string Speed { get; set; }
	public Variant Variant { get; set; }
}
