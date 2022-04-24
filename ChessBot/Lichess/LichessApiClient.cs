using System.Net.Http.Headers;
using ChessBot.Model;
using Foxite.Common.AsyncLinq;

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

	#region Api functions
	public IAsyncEnumerable<BoardStreamEvent> StreamBoardEvents(string gameId) {
		return m_NdJsonClient.StreamTokensAsync(Get("/api/board/game/stream/{0}", gameId)).Select(jt => (BoardStreamEvent) jt.ToObject(jt["type"].ToObject<string>() switch {
			"gameFull" => typeof(GameFullBoardStreamEvent),
			"gameState" => typeof(GameStateBoardStreamEvent),
			"chatLine" => typeof(ChatLineBoardStreamEvent),
			_ => typeof(BoardStreamEvent)
		}));
	}

	public Task Move(string gameId, string move) {
		return Send(Post("/api/board/game/{0}/move/{1}", gameId, move));
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
