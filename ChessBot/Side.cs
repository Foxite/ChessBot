namespace ChessBot;

public struct Side : IEquatable<Side> {
	public static readonly Side Bot = new Side(true);
	public static readonly Side Opponent = new Side(false);
	
	private readonly bool m_IsBot;

	private Side(bool isBot) {
		m_IsBot = isBot;
	}

	public static Side operator !(Side side) {
		return new Side(!side.m_IsBot);
	}

	public bool Equals(Side other) {
		return m_IsBot == other.m_IsBot;
	}

	public override bool Equals(object? obj) {
		return obj is Side other && Equals(other);
	}

	public override int GetHashCode() {
		return m_IsBot.GetHashCode();
	}

	public static bool operator ==(Side left, Side right) {
		return left.Equals(right);
	}

	public static bool operator !=(Side left, Side right) {
		return !left.Equals(right);
	}
}
