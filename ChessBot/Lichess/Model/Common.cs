namespace ChessBot.Model; 

public class Player {
	public string Id { get; set; }
	public string Name { get; set; }
	public bool? Provisional { get; set; }
	public int Rating { get; set; }
	public string? Title { get; set; }
}
