using Foxite.Common.AsyncLinq;
using Newtonsoft.Json.Linq;

namespace ChessBot;

public class NdJsonClient {
	private HttpClient m_Http;

	public NdJsonClient(HttpClient http) {
		m_Http = http;
	}
	
	private async IAsyncEnumerable<string> StreamLinesAsync(HttpRequestMessage hrm) {
		using HttpResponseMessage response = await m_Http.SendAsync(hrm);
		response.EnsureSuccessStatusCode();
		await using Stream stream = await response.Content.ReadAsStreamAsync();
		using var reader = new StreamReader(stream);
		while (await reader.ReadLineAsync() is { } next) {
			yield return next;
		}
	}

	public IAsyncEnumerable<JToken> StreamTokensAsync(HttpRequestMessage hrm) {
		return StreamLinesAsync(hrm).Select(JToken.Parse);
	}
}
