using System.Diagnostics;
using ChessBot.Algorithm;
using ChessBot.Model;
using Newtonsoft.Json;

namespace ChessBot;

public class GameContext : IDisposable {
	private readonly LichessApiClient m_ApiClient;
	private readonly CancellationTokenSource m_StreamCts = new();
	
	public string GameId { get; }

	public event EventHandler<BoardStreamEvent>? BoardEvent;
	public event EventHandler<BoardStreamEvent> UnknownBoardEvent;
	public event EventHandler<GameStateBoardStreamEvent>? GameState;
	public event EventHandler<ChatLineBoardStreamEvent>? ChatLine;
	public event EventHandler<string>? MovePlayed;
	public event EventHandler? GameEnd;
	public event EventHandler<Exception> StreamError;
	
	public Side CurrentTurn { get; set; }

	public GameFullBoardStreamEvent FullState { get; private set; } = null!;
	public GameStateBoardStreamEvent BoardState { get; private set; } = null!;
	
	private GameContext(LichessApiClient apiClient, string gameId) {
		m_ApiClient = apiClient;
		GameId = gameId;

		StreamError += (_, exception) => {
			Console.Error.Write("Stream error: \n" + exception.ToStringDemystified());
		};

		UnknownBoardEvent += (_, evt) => {
			Console.Error.Write("Unknown board event: \n" + evt.Token.ToString(Formatting.Indented));
		};
	}

	public Task Move(string move) => m_ApiClient.Move(GameId, move);
	public Task Chat(string text, Room room = Room.Player) => m_ApiClient.SendChatMessage(GameId, room, text);
	public Task Abort() => m_ApiClient.AbortGame(GameId);
	public Task Resign() => m_ApiClient.ResignGame(GameId);
	
	public void Dispose() {
		m_StreamCts.Cancel();
	}

	internal static async Task<GameContext> CreateAsync(LichessApiClient client, string gameId) {
		var ret = new GameContext(client, gameId);

		var fullBoardStateReceived = new TaskCompletionSource();

		_ = Task.Run(async () => {
			CancellationToken cancellationToken = ret.m_StreamCts.Token;
			try {
				await foreach (BoardStreamEvent evt in client.StreamBoardEvents(gameId, cancellationToken)) {
					ret.BoardEvent?.Invoke(ret, evt);
					switch (evt) {
						case GameFullBoardStreamEvent fullGame:
							ret.FullState = fullGame;
							ret.BoardState = fullGame.State;
							fullBoardStateReceived.SetResult();
							break;
						case GameStateBoardStreamEvent gameState:
							GameStateBoardStreamEvent oldState = ret.BoardState;
							ret.BoardState = gameState;
							ret.FullState.State = gameState;
							ret.CurrentTurn = Utils.GetMoveCount(gameState.Moves) % 2 == 0 ? Side.White : Side.Black;
							ret.GameState?.Invoke(ret, gameState);
							
							if (oldState.Moves != gameState.Moves) {
								string newMove = gameState.Moves[oldState.Moves.Length..].Trim();
								ret.MovePlayed?.Invoke(ret, newMove);
							}
							
							break;
						case ChatLineBoardStreamEvent chatLine:
							ret.ChatLine?.Invoke(ret, chatLine);
							break;
						default:
							ret.UnknownBoardEvent(ret, evt);
							break;
					}
				}
				ret.GameEnd?.Invoke(ret, null);
			} catch (Exception e) when (e is not TaskCanceledException) {
				ret.StreamError(ret, e);
			}
		});

		await fullBoardStateReceived.Task;
		return ret;
	}
}
