using System.Diagnostics;
using ChessBot;
using ChessBot.Algorithm;
using ChessBot.Model;

string apiToken = Environment.GetEnvironmentVariable("API_TOKEN")!;

var lichess = new LichessApiClient(new HttpClient(), apiToken);

OngoingGame? ongoingGame = (await lichess.GetMyOngoingGames(1)).FirstOrDefault();
if (ongoingGame == null) {
	Console.WriteLine("No ongoing game");
	return;
}

GameContext game = await lichess.OpenGame(ongoingGame.GameId);
var algorithm = new ChessAlgorithm(game.FullState);
var tcs = new TaskCompletionSource();

game.GameEnd += (o, e) => {
	tcs.SetResult();
};

game.ChatLine += (_, chat) => {
	Console.WriteLine($"[CHAT] {chat.Room}/{chat.Username}: {chat.Text}");
};

game.GameState += (_, state) => {
	Console.WriteLine($"[STATE] Status: {state.Status}; Moves: {state.Moves}; Winner: {state.Winner?.ToString() ?? "null"}");
};

game.MovePlayed += (_, move) => {
	try {
		algorithm.MovePlayed(move);
		if (game.CurrentTurn == ongoingGame.Color) {
			Task.Run(async () => {
				string nextMove = algorithm.GetBotMove();
				try {
					await game.Move(nextMove);
				} catch (Exception e) {
					Console.WriteLine(e.ToStringDemystified());
				}
			});
		}
	} catch (Exception e) {
		Console.WriteLine(e.ToStringDemystified());
	}
};

if (ongoingGame.IsMyTurn) {
	string nextMove = algorithm.GetBotMove();
	try {
		await game.Move(nextMove);
	} catch (Exception e) {
		Console.WriteLine(e.ToStringDemystified());
	}
}

await tcs.Task;

/*
// f2f3 (white pawn)
// e7e6 (black pawn)
// g2g4 (white pawn)
// d8h4 (black queen) checkmate against white
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
//*/


