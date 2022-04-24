using System.Diagnostics;
using ChessBot;
using ChessBot.Model;
using Newtonsoft.Json;

string apiToken = Environment.GetEnvironmentVariable("API_TOKEN");

var lichess = new LichessApiClient(new HttpClient(), apiToken);

string gameId = "gHFGXAy9";

Task.Run(async () => {
	try {
		await foreach (BoardStreamEvent evt in lichess.StreamBoardEvents(gameId)) {
			Console.WriteLine("EVENT:");
			Console.WriteLine(JsonConvert.SerializeObject(evt, Formatting.Indented));
		}
	} catch (Exception e) {
		Console.WriteLine(e.ToStringDemystified());
	}
});

while (true) {
	Console.Write("Command: ");
	string command = Console.ReadLine();
	switch (command) {
		case "move":
			Console.Write("Move: ");
			string move = Console.ReadLine();
			lichess.Move(gameId, move);
			break;
		case "chat":
			Console.Write("Text: ");
			string text = Console.ReadLine();
			lichess.SendChatMessage(gameId, Room.Player, text);
			break;
		case "abort":
			lichess.AbortGame(gameId);
			break;
		case "resign":
			lichess.ResignGame(gameId);
			break;
	}
}
