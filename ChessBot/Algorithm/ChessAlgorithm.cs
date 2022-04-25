using System.Text;
using ChessBot.Model;

namespace ChessBot.Algorithm;

public abstract class ChessAlgorithmBase {
	public abstract string Name { get; }
	public abstract void MovePlayed(string move);
	public abstract string GetBotMove();
}

public class ChessAlgorithm : ChessAlgorithmBase {
	private readonly GameFullBoardStreamEvent m_FullState;
	private readonly AlgorithmBoardState m_State;

	private static readonly string[] Moves = new[] { "f2f3", "e7e6", "g2g4", "d8h4" };
	private int m_Index;

	public override string Name => "Test Algorithm";

	public ChessAlgorithm(GameFullBoardStreamEvent fullState) {
		m_FullState = fullState;
		m_State = new AlgorithmBoardState(fullState);
		m_Index = Utils.GetMoveCount(fullState.State.Moves);
	}
	
	public override void MovePlayed(string move) {
		m_State.Move(move);
		m_Index++;
		Console.WriteLine(m_State.ToString());
	}
	public override string GetBotMove() {
		return Moves[m_Index];
	}
}

public class AlgorithmBoardState {
	private AlgorithmPiece?[,] m_Pieces = new AlgorithmPiece[8, 8];

	public AlgorithmBoardState(GameFullBoardStreamEvent fullState) {
		void Set(int file, int rank, PieceType type, Side side) {
			m_Pieces[file, rank] = new AlgorithmPiece(type, side, file, rank);
		}
		
		for (int i = 0; i < 8; i++) {
			Set(i, 1, PieceType.Pawn, Side.White);
			Set(i, 6, PieceType.Pawn, Side.Black);
		}
		
		void SetupSide(Side side, int rank) {
			Set(0, rank, PieceType.Rook, side);
			Set(1, rank, PieceType.Bishop, side);
			Set(2, rank, PieceType.Knight, side);
			Set(3, rank, PieceType.Queen, side);
			Set(4, rank, PieceType.King, side);
			Set(5, rank, PieceType.Knight, side);
			Set(6, rank, PieceType.Bishop, side);
			Set(7, rank, PieceType.Rook, side);
		}
		
		SetupSide(Side.White, 0);
		SetupSide(Side.Black, 7);

		foreach (string move in fullState.State.Moves.Split(" ", StringSplitOptions.RemoveEmptyEntries)) {
			Move(move);
		}
	}

	public AlgorithmPiece? GetPieceAt(int file, int rank) {
		return m_Pieces[file, rank];
	}

	public void Move(string move) {
		(int fromFile, int fromRank, int toFile, int toRank) = Utils.MoveFromString(move);
		Move(fromFile, fromRank, toFile, toRank);
	}
	
	public void Move(int fromFile, int fromRank, int toFile, int toRank) {
		AlgorithmPiece? movedPiece = m_Pieces[fromFile, fromRank];
		if (movedPiece == null) {
			throw new InvalidOperationException($"Square at {Utils.PositionToString(fromFile, fromRank)} is empty");
		}

		m_Pieces[fromFile, fromRank] = null;
		m_Pieces[toFile, toRank] = movedPiece;
		movedPiece.File = toFile;
		movedPiece.Rank = toRank;
	}

	public override string ToString() {
		var sb = new StringBuilder();
		for (int rank = 0; rank < 8; rank++) {
			for (int file = 0; file < 8; file++) {
				AlgorithmPiece? piece = GetPieceAt(file, rank);
				if (piece == null) {
					sb.Append("  ");
				} else {
					string symbol = piece.PieceType switch {
						PieceType.Pawn => "Pa",
						PieceType.Rook => "Ro",
						PieceType.Bishop => "Bi",
						PieceType.Knight => "Kn",
						PieceType.Queen => "Qu",
						PieceType.King => "Ki"
					};
					sb.Append(symbol);
				}
				sb.Append(' ');
			}
			sb.Append(' ');
			sb.Append(rank + 1);
			sb.AppendLine();
		}

		for (int file = 0; file < 8; file++) {
			sb.Append(file + 1);
			sb.Append("  ");
		}

		return sb.ToString();
	}
}

public class AlgorithmPiece {
	public PieceType PieceType { get; }
	public Side Side { get; }
	public int File { get; set; }
	public int Rank { get; set; }

	public AlgorithmPiece(PieceType pieceType, Side side, int file, int rank) {
		PieceType = pieceType;
		Side = side;
		File = file;
		Rank = rank;
	}

	/*
	public IEnumerable<string> GetValidMoves(AlgorithmBoardState boardState) {
		switch (PieceType) {
			case PieceType.Pawn:
				if (File == (Side == Side.Black ? 7 : 2) && boardState) {
					
				}
				break;
			case PieceType.Rook:
				break;
			case PieceType.Bishop:
				break;
			case PieceType.Knight:
				break;
			case PieceType.Queen:
				break;
			case PieceType.King:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}//*/
}