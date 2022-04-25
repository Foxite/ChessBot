using System.Diagnostics;
using System.Net.Http.Headers;
using ChessBot.Model;
using Foxite.Common.AsyncLinq;
using Newtonsoft.Json;

namespace ChessBot; 

public class LichessApiClient {
	private readonly string m_LichessUrl;
	
	private readonly NdJsonClient m_NdJsonClient;
	private readonly HttpClient m_Http;
	private readonly string m_ApiToken;

	public LichessApiClient(HttpClient http, string apiToken, string lichessUrl = "https://lichess.org") {
		m_Http = http;
		m_ApiToken = apiToken;
		m_LichessUrl = lichessUrl;
		m_NdJsonClient = new NdJsonClient(http);
	}

	public GameContext Open(string gameId) {
		return new GameContext(this, gameId);
	}

	#region Api functions
	public IAsyncEnumerable<BoardStreamEvent> StreamBoardEvents(string gameId, CancellationToken cancellationToken) {
		return m_NdJsonClient.StreamTokensAsync(Get("/api/bot/game/stream/{0}", gameId), cancellationToken, true).Select(jt => {
			var ret = (BoardStreamEvent) jt.ToObject(jt["type"].ToObject<string>() switch {
				"gameFull" => typeof(GameFullBoardStreamEvent),
				"gameState" => typeof(GameStateBoardStreamEvent),
				"chatLine" => typeof(ChatLineBoardStreamEvent),
				_ => typeof(BoardStreamEvent)
			})!;
			ret.Token = jt;
			return ret;
		});
	}

	public Task Move(string gameId, string move) {
		return Send(Post("/api/bot/game/{0}/move/{1}", gameId, move));
	}

	public Task SendChatMessage(string gameId, Room room, string text) {
		HttpRequestMessage message = Post("/api/bot/game/{0}/chat", gameId);
		message.Content = new FormUrlEncodedContent(new Dictionary<string, string>() {
			{ "room", room.ToString().ToLower() },
			{ "text", text },
		});
		return Send(message);
	}

	public Task AbortGame(string gameId) {
		return Send(Post("/api/bot/game/{0}/abort", gameId));
	}

	public Task ResignGame(string gameId) {
		return Send(Post("/api/bot/game/{0}/resign", gameId));
	}
	#endregion
	
	#region Utility functions
	private HttpRequestMessage Get(string url, params object[] parameters) => Message(HttpMethod.Get, url, parameters);
	private HttpRequestMessage Post(string url, params object[] parameters) => Message(HttpMethod.Post, url, parameters);

	private HttpRequestMessage Message(HttpMethod method, string url, params object[] parameters) {
		return new HttpRequestMessage() {
			RequestUri = new Uri(m_LichessUrl + string.Format(url, parameters)),
			Method = method,
			Headers = {
				UserAgent = {
					new ProductInfoHeaderValue("ChessBot", "0.1"),
					new ProductInfoHeaderValue("(https://github.com/Foxite/ChessBot)")
				},
				Authorization = AuthenticationHeaderValue.Parse("Bearer " + m_ApiToken)
			}
		};
	}

	private async Task Send(HttpRequestMessage message) {
		HttpResponseMessage result = await m_Http.SendAsync(message);
		result.EnsureSuccessStatusCode();
	}
	#endregion
}

public class GameContext : IDisposable {
	private readonly LichessApiClient m_ApiClient;
	private readonly CancellationTokenSource m_StreamCts = new();
	
	public string GameId { get; }

	public event EventHandler<BoardStreamEvent>? BoardEvent;
	public event EventHandler<BoardStreamEvent> UnknownBoardEvent;
	public event EventHandler<GameFullBoardStreamEvent> GameFull;
	public event EventHandler<GameStateBoardStreamEvent> GameState;
	public event EventHandler<ChatLineBoardStreamEvent>? ChatLine;
	public event EventHandler? GameEnd;
	public event EventHandler<Exception> StreamError;

	public GameStateBoardStreamEvent BoardState { get; private set; } = null!;
	
	public GameContext(LichessApiClient apiClient, string gameId) {
		m_ApiClient = apiClient;
		GameId = gameId;

		StreamError += (_, exception) => {
			Console.Error.Write("Stream error: \n" + exception.ToStringDemystified());
		};

		UnknownBoardEvent += (_, evt) => {
			Console.Error.Write("Unknown board event: \n" + evt.Token.ToString(Formatting.Indented));
		};

		GameFull += (_, gameFull) => {
			BoardState = gameFull.State;
		};

		GameState += (_, state) => {
			BoardState = state;
		};

		_ = Task.Run(async () => {
			CancellationToken cancellationToken = m_StreamCts.Token;
			try {
				await foreach (BoardStreamEvent evt in apiClient.StreamBoardEvents(gameId, cancellationToken)) {
					BoardEvent?.Invoke(this, evt);
					switch (evt) {
						case GameFullBoardStreamEvent gameFull:
							GameFull(this, gameFull);
							break;
						case GameStateBoardStreamEvent gameState:
							GameState(this, gameState);
							break;
						case ChatLineBoardStreamEvent chatLine:
							ChatLine?.Invoke(this, chatLine);
							break;
						default:
							UnknownBoardEvent(this, evt);
							break;
					}
				}
				GameEnd?.Invoke(this, null);
			} catch (Exception e) when (e is not TaskCanceledException) {
				StreamError(this, e);
			}
		});
	}

	public Task Move(string move) => m_ApiClient.Move(GameId, move);
	public Task Chat(string text, Room room = Room.Player) => m_ApiClient.SendChatMessage(GameId, room, text);
	public Task Abort() => m_ApiClient.AbortGame(GameId);
	public Task Resign() => m_ApiClient.ResignGame(GameId);
	
	public void Dispose() {
		m_StreamCts.Cancel();
	}
}