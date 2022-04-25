using ChessBot;
using ChessBot.Model;

string apiToken = Environment.GetEnvironmentVariable("API_TOKEN");
string gameId = "5G3Phnz2eWn0";

var lichess = new LichessApiClient(new HttpClient(), apiToken);

GameContext game = lichess.Open(gameId);

game.ChatLine += (_, chat) => {
	Console.WriteLine($"[CHAT] {chat.Room}/{chat.Username}: {chat.Text}");
};

game.GameState += (_, state) => {
	Console.WriteLine($"[STATE] Status: {state.Status}; Moves: {state.Moves}; Winner: {state.Winner?.ToString() ?? "null"}");
};

while (true) {
	Console.Write("Command: ");
	string command = Console.ReadLine();
	switch (command) {
		case "move":
			Console.Write("Move: ");
			string move = Console.ReadLine();
			await lichess.Move(gameId, move);
			break;
		case "chat":
			Console.Write("Text: ");
			string text = Console.ReadLine();
			await lichess.SendChatMessage(gameId, Room.Player, text);
			break;
		case "abort":
			await lichess.AbortGame(gameId);
			break;
		case "resign":
			await lichess.ResignGame(gameId);
			break;
	}
}
