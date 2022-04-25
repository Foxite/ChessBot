using ChessBot.Model;

namespace ChessBot;

public struct Side : IEquatable<Side> {
	public static readonly Side Black = new Side(true);
	public static readonly Side White = new Side(false);
	
	private readonly bool m_IsBlack;

	private Side(bool isBlack) {
		m_IsBlack = isBlack;
	}

	public static Side operator !(Side side) {
		return new Side(!side.m_IsBlack);
	}

	public bool Equals(Side other) {
		return m_IsBlack == other.m_IsBlack;
	}

	public override bool Equals(object? obj) {
		return obj is Side other && Equals(other);
	}

	public override int GetHashCode() {
		return m_IsBlack.GetHashCode();
	}

	public static bool operator ==(Side left, Side right) {
		return left.Equals(right);
	}

	public static bool operator !=(Side left, Side right) {
		return !(left == right);
	}

	public static implicit operator BlackOrWhite(Side side) {
		return side == Black ? BlackOrWhite.Black : BlackOrWhite.White;
	}

	public static implicit operator Side(BlackOrWhite blackOrWhite) {
		return blackOrWhite == BlackOrWhite.Black ? Side.Black : Side.White;
	}
}
