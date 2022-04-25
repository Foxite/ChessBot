using System.Runtime.CompilerServices;
using Foxite.Common.AsyncLinq;
using Newtonsoft.Json.Linq;

namespace ChessBot;

public class NdJsonClient {
	private readonly HttpClient m_Http;

	public NdJsonClient(HttpClient http) {
		m_Http = http;
	}
	
	private async IAsyncEnumerable<string> StreamLinesAsync(HttpRequestMessage hrm, bool skipEmptyLines, [EnumeratorCancellation] CancellationToken cancellationToken) {
		using HttpResponseMessage response = await m_Http.SendAsync(hrm, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
		response.EnsureSuccessStatusCode();
		await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
		using var reader = new StreamReader(stream);
		while (await reader.ReadLineAsync() is { } next) {
			if (!(skipEmptyLines && string.IsNullOrWhiteSpace(next))) {
				yield return next;
			}
		}
	}

	public IAsyncEnumerable<JToken> StreamTokensAsync(HttpRequestMessage hrm, CancellationToken cancellationToken = default, bool skipEmptyLines = false) {
		return StreamLinesAsync(hrm, skipEmptyLines, cancellationToken).Select(JToken.Parse);
	}
}
