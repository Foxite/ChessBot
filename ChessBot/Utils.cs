namespace ChessBot.Algorithm;

public static class Utils {
	public static string MoveToString(int fromFile, int fromRank, int toFile, int toRank) {
		string ret = "";
		ret += 'a' + fromFile;
		ret += fromRank.ToString();
		ret += 'a' + toFile;
		ret += toRank.ToString();
		return ret;
	}

	public static (int fromFile, int fromRank, int toFile, int toRank) MoveFromString(string move) {
		int fromFile = move[0] - 'a';
		int fromRank = move[1] - '1';
		int toFile = move[2] - 'a';
		int toRank = move[3] - '1';
		return (fromFile, fromRank, toFile, toRank);
	}
	
	public static string PositionToString(int file, int rank) {
		string ret = "";
		ret += (char) ('a' + file);
		ret += rank.ToString();
		return ret;
	}

	public static (int file, int rank) PositionFromString(string position) {
		int file = position[0] - 'a';
		int rank = position[1] - '1';
		return (file, rank);
	}

	public static int GetMoveCount(string moves) {
		if (moves.Length == 0) {
			return 0;
		} else {
			return moves.Count(ch => ch == ' ') + 1;
		}
	}
}
