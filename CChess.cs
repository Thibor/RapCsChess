﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace RapChessCs
{
	class CChess
	{
		const int piecePawn = 0x01;
		const int pieceKnight = 0x02;
		const int pieceBishop = 0x03;
		const int pieceRook = 0x04;
		const int pieceQueen = 0x05;
		const int pieceKing = 0x06;
		const int colorBlack = 0x08;
		const int colorWhite = 0x10;
		const int colorEmpty = 0x20;
		const int moveflagPassing = 0x02 << 16;
		const int moveflagCastleKing = 0x04 << 16;
		const int moveflagCastleQueen = 0x08 << 16;
		const int moveflagPromotion = 0xf0 << 16;
		const int moveflagPromoteQueen = 0x10 << 16;
		const int moveflagPromoteRook = 0x20 << 16;
		const int moveflagPromoteBishop = 0x40 << 16;
		const int moveflagPromoteKnight = 0x80 << 16;
		const int maskCastle = moveflagCastleKing | moveflagCastleQueen;
		const int maskPromotion = moveflagPromoteQueen | moveflagPromoteRook | moveflagPromoteBishop | moveflagPromoteKnight;
		int inTime = 0;
		int inDepth = 0;
		ulong inNodes = 0;
		int g_castleRights = 0xf;
		int g_depth = 0;
		ulong g_hash = 0;
		int g_passing = 0;
		public int g_move50 = 0;
		int g_moveNumber = 0;
		int g_phase = 32;
		ulong g_totalNodes = 0;
		int g_timeout = 0;
		int g_depthout = 0;
		ulong g_nodeout = 0;
		int g_mainDepth = 1;
		bool g_stop = false;
		string g_pv = "";
		string g_scoreFm = "";
		public int undoIndex = 0;
		int kingPos = 0;
		readonly int[] arrFieldL = new int[64];
		readonly int[] arrFieldS = new int[256];
		readonly int[] g_board = new int[256];
		readonly ulong[,] g_hashBoard = new ulong[256, 16];
		readonly int[] boardCheck = new int[256];
		readonly int[] boardCastle = new int[256];
		public bool whiteTurn = true;
		int usColor = 0;
		int enColor = 0;
		int bsIn = -1;
		int bsDepth = 0;
		string bsFm = "";
		string bsPv = "";
		bool lastInsufficient = false;
		int lastScore = 0;
		readonly ulong[] bitBoard = new ulong[16];
		readonly ulong[] bbIsolated = new ulong[8];
		readonly ulong[] bbFile = new ulong[8];
		readonly ulong[,] bbDoubled = new ulong[64, 2];
		readonly ulong[,] bbSupportedL = new ulong[64, 2];
		readonly ulong[,] bbSupportedR = new ulong[64, 2];
		readonly ulong[,] bbPassed = new ulong[64, 2];
		readonly int[] tmpIsolated = new int[2] { -5, -15 };
		readonly int[] tmpDoubled = new int[2] { -11, -56 };
		readonly int[,] tmpRookFile = new int[3, 2] { { 0, 0 }, { 21, 4 }, { 47, 25 } };
		readonly int[,] tmpPassed = new int[6, 2] { { 5, 7 }, { 5, 14 }, { 31, 38 }, { 73, 73 }, { 166, 166 }, { 252, 252 } };
		readonly int[,] tmpMaterial = new int[6, 2] { { 171, 240 }, { 764, 848 }, { 826, 891 }, { 1282, 1373 }, { 2526, 2646 }, { 0xffff, 0xffff } };
		readonly int[,] tmpCenter = new int[6, 2] { { 4, 2 }, { 8, 8 }, { 4, 8 }, { -8, 8 }, { -8, 0xf }, { -8, 8 } };
		readonly int[,] tmpPassedFile = new int[8, 2] { { 0, 0 }, { -11, -8 }, { -22, -16 }, { -33, -24 }, { -33, -24 }, { -22, -16 }, { -11, -8 }, { 0, 0 } };
		readonly int[,,] tmpMobility = new int[7, 28, 2] {
			{ { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } },
			{ { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } },
			{ {-75,-76 }, {-57,-54 }, {-9,-28 }, {-2,-10 }, {6,5 }, {14,12 }, {22, 26 }, { 29,29 }, { 36, 29 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } },
			{ { -48,-59 }, {-20,-23 }, {16, -3 }, {26, 13 }, {38, 24 }, {51, 42 }, {55, 54 }, {63, 57 }, {63, 65 }, {68, 73 }, {81, 78 }, {81, 86 }, {91, 88 }, {98, 97 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } },
			{ {-58,-76 }, {-27,-18 }, {-15, 28 }, { -10, 55 }, {-5, 69 }, { -2, 82 }, { 9,112 }, {16,118 }, { 30,132 }, {29,142 }, {32,155 }, {38,165 }, { 46,166 }, { 48,169 }, {58,171 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } },
			{{ -39, -36 },{-21, -15 },{3, 8 },{3, 18 },{14, 34 },{22, 54 },{28, 61 },{41, 73 },{43, 79 },{48, 92 },{56, 94 },{60, 104 },{60, 113 },{66, 120 },{67, 123 },{70, 126 },{71, 133 },{73, 136 },{79, 140 },{88, 143 },{88, 148 },{99, 166 },{102, 170 },{102, 175 },{106, 184 },{109, 191 },{113, 206 },{116, 212} },
			{ { 90,9 }, { 80,8 }, { 70, 7 }, { 60, 6 }, { 50, 5 }, { 40, 4 }, { 30, 3 }, { 20, 2 }, { 10, 1 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } }
			};
		readonly int[,,] arrMobility = new int[33, 7, 28];
		readonly int[] arrIsolated = new int[33];
		readonly int[] arrDoubled = new int[33];
		readonly int[,] arrRookFile = new int[33, 3];
		readonly int[,,] arrPassed = new int[33, 2, 64];
		readonly int[,,] arrBonus = new int[33, 16, 256];
		readonly int[] arrDirKinght = { 14, -14, 18, -18, 31, -31, 33, -33 };
		readonly int[] arrDirBishop = { 15, -15, 17, -17 };
		readonly int[] arrDirRook = { 1, -1, 16, -16 };
		readonly int[] arrDirQueen = { 1, -1, 15, -15, 16, -16, 17, -17 };
		readonly CUndo[] undoStack = new CUndo[0xfff];
		Thread startThread;
		public Stopwatch stopwatch = Stopwatch.StartNew();
		private static readonly Random random = new Random();
		public CSynStop synStop = new CSynStop();

		public CChess()
		{
			g_hash = RAND_32();
			for (int n = 0; n < undoStack.Length; n++)
				undoStack[n] = new CUndo();
			for (int y = 0; y < 8; y++)
				for (int x = 0; x < 8; x++)
				{
					int fieldS = y * 8 + x;
					int fieldL = (y + 4) * 16 + x + 4;
					arrFieldL[fieldS] = fieldL;
					arrFieldS[fieldL] = fieldS;
				}
			for (int n = 0; n < 256; n++)
			{
				boardCheck[n] = 0;
				boardCastle[n] = 15;
				g_board[n] = 0;
				for (int p = 0; p < 16; p++)
					g_hashBoard[n, p] = RAND_32();
			}
			int[] arrCastleI = { 68, 72, 75, 180, 184, 187 };
			int[] arrCasteleV = { 7, 3, 11, 13, 12, 14 };
			int[] arrCheckI = { 71, 72, 73, 183, 184, 185 };
			int[] arrCheckV = { colorBlack | moveflagCastleQueen, colorBlack | maskCastle, colorBlack | moveflagCastleKing, colorWhite | moveflagCastleQueen, colorWhite | maskCastle, colorWhite | moveflagCastleKing };
			for (int n = 0; n < 6; n++)
			{
				boardCastle[arrCastleI[n]] = arrCasteleV[n];
				boardCheck[arrCheckI[n]] = arrCheckV[n];
			}
			for (int ph = 2; ph < 33; ph++)
			{
				double f = ph / 32.0;
				for (int p = 1; p < 7; p++)
					for (int n = 0; n < 28; n++)
						arrMobility[ph, p, n] = Convert.ToInt32(tmpMobility[p, n, 0] * f + tmpMobility[p, n, 1] * (1 - f));
			}
			for (int ph = 2; ph < 33; ph++)
			{
				double f = ph / 32.0;
				int a = tmpIsolated[0];
				int b = tmpIsolated[1];
				double v = a * f + b * (1 - f);
				arrIsolated[ph] = Convert.ToInt32(v);
				a = tmpDoubled[0];
				b = tmpDoubled[1];
				v = a * f + b * (1 - f);
				arrDoubled[ph] = Convert.ToInt32(v);
				for (int n = 1; n < 3; n++)
				{
					a = tmpRookFile[n, 0];
					b = tmpRookFile[n, 1];
					v = a * f + b * (1 - f);
					arrRookFile[ph, n] = Convert.ToInt32(v);
				}
				for (int rank = 1; rank < 7; rank++)
				{
					for (int y = 0; y < 8; y++)
						for (int x = 0; x < 8; x++)
						{
							int nw = (y + 4) * 16 + (x + 4);
							int nb = (11 - y) * 16 + (x + 4);
							int cx = Math.Min(x, 7 - x) + 1;
							int cy = Math.Min(y, 7 - y) + 1;
							int center = (cx * cy) - 1;
							a = tmpCenter[rank - 1, 0];
							b = tmpCenter[rank - 1, 1];
							v = (a * f + b * (1 - f)) * center;
							a = tmpMaterial[rank - 1, 0];
							b = tmpMaterial[rank - 1, 1];
							v += a * f + b * (1 - f);
							if (rank == 1 && y > 0 && y < 7)
							{
								int py = 6 - y;
								a = tmpPassed[py, 0] + tmpPassedFile[x, 0];
								b = tmpPassed[py, 1] + tmpPassedFile[x, 1];
								int passed = Convert.ToInt32(a * f + b * (1 - f));
								arrPassed[ph, 1, arrFieldS[nw]] = passed;
								arrPassed[ph, 0, arrFieldS[nb]] = passed;
							}

							int iv = Convert.ToInt32(v);
							arrBonus[ph, rank, nw] = iv;
							arrBonus[ph, 8 | rank, nb] = iv;

						}
				}
			}
			for (int x = 0; x < 8; x++)
				for (int y = 0; y < 8; y++)
				{
					int i = (y << 3) | x;
					CBitBoard.Add(ref bbFile[x], i);
					for (int n = y + 1; n < 8; n++)
						CBitBoard.Add(ref bbPassed[(n << 3) | x, 1], i);
					for (int n = y + 2; n < 8; n++)
					{
						if (x > 0)
							CBitBoard.Add(ref bbPassed[(n << 3) | (x - 1), 1], i);
						if (x < 7)
							CBitBoard.Add(ref bbPassed[(n << 3) | (x + 1), 1], i);
					}
					for (int n = y - 1; n >= 0; n--)
						CBitBoard.Add(ref bbPassed[(n << 3) | x, 0], i);
					for (int n = y - 2; n >= 0; n--)
					{
						if (x > 0)
							CBitBoard.Add(ref bbPassed[(n << 3) | (x - 1), 0], i);
						if (x < 7)
							CBitBoard.Add(ref bbPassed[(n << 3) | (x + 1), 0], i);
					}
					if (x > 0)
						CBitBoard.Add(ref bbIsolated[x - 1], i);
					if (x < 7)
						CBitBoard.Add(ref bbIsolated[x + 1], i);
					if (y > 1)
					{
						CBitBoard.Add(ref bbDoubled[i - 8, 1], i);
						if (x > 0)
							CBitBoard.Add(ref bbSupportedL[i - 9, 1], i);
						if (x < 7)
							CBitBoard.Add(ref bbSupportedR[i - 7, 1], i);
					}
					if (y < 7)
					{
						CBitBoard.Add(ref bbDoubled[i + 8, 0], i);
						if (x > 0)
							CBitBoard.Add(ref bbSupportedL[i + 7, 0], i);
						if (x < 7)
							CBitBoard.Add(ref bbSupportedR[i + 9, 0], i);
					}
				}
		}

		public void InitializeFromFen(string fen)
		{
			synStop.SetStop(false);
			g_phase = 0;
			for (int n = 0; n < 64; n++)
				g_board[arrFieldL[n]] = colorEmpty;
			for (int n = 0; n < 16; n++)
				bitBoard[n] = 0;
			if (fen == "") fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
			string[] chunks = fen.Split(' ');
			int y = 0;
			int x = 0;
			string pieces = chunks[0];
			for (int i = 0; i < pieces.Length; i++)
			{
				char c = pieces[i];
				if (c == '/')
				{
					y++;
					x = 0;
				}
				else if (c >= '0' && c <= '9')
					x += Int32.Parse(c.ToString());
				else
				{
					g_phase++;
					int piece = Char.IsUpper(c) ? colorWhite : colorBlack;
					int index = (y + 4) * 16 + x + 4;
					switch (Char.ToLower(c))
					{
						case 'p':
							piece |= piecePawn;
							break;
						case 'b':
							piece |= pieceBishop;
							break;
						case 'n':
							piece |= pieceKnight;
							break;
						case 'r':
							piece |= pieceRook;
							break;
						case 'q':
							piece |= pieceQueen;
							break;
						case 'k':
							piece |= pieceKing;
							break;
					}
					g_board[index] = piece;
					int bi = (y << 3) | x;
					CBitBoard.Add(ref bitBoard[piece & 0xf], bi);
					x++;
				}
			}
			whiteTurn = chunks[1] == "w";
			g_castleRights = 0;
			if (chunks[2].IndexOf('K') != -1)
				g_castleRights |= 1;
			if (chunks[2].IndexOf('Q') != -1)
				g_castleRights |= 2;
			if (chunks[2].IndexOf('k') != -1)
				g_castleRights |= 4;
			if (chunks[2].IndexOf('q') != -1)
				g_castleRights |= 8;
			g_passing = 0;
			if (chunks[3].IndexOf('-') == -1)
				g_passing = StrToSquare(chunks[3]);
			g_move50 = 0;
			g_moveNumber = Int32.Parse(chunks[5]);
			if (g_moveNumber > 0) g_moveNumber--;
			g_moveNumber *= 2;
			if (!whiteTurn) g_moveNumber++;
			undoIndex = 0;
		}

		ulong RAND_32()
		{
			return ((ulong)random.Next() << 32) | ((ulong)random.Next() << 0);
		}

		string FormatMove(int move)
		{
			string result = FormatSquare(move & 0xFF) + FormatSquare((move >> 8) & 0xFF);
			if ((move & moveflagPromotion) > 0)
			{
				if ((move & moveflagPromoteQueen) > 0) result += 'q';
				else if ((move & moveflagPromoteRook) > 0) result += 'r';
				else if ((move & moveflagPromoteBishop) > 0) result += 'b';
				else result += 'n';
			}
			return result;
		}

		string FormatSquare(int square)
		{
			char[] arr = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };
			return arr[(square & 0xf) - 4] + (12 - (square >> 4)).ToString();
		}

		int StrToSquare(string s)
		{
			string fl = "abcdefgh";
			int x = fl.IndexOf(s[0]);
			int y = 12 - Int32.Parse(s[1].ToString());
			return (x + 4) | (y << 4);
		}

		bool IsRepetition()
		{
			for (int n = undoIndex - 2; n >= undoIndex - g_move50; n -= 2)
				if (undoStack[n].hash == g_hash)
				{
					return true;
				}
			return false;
		}

		bool IsAttacked(bool wt, int to)
		{
			int ec = wt ? colorBlack : colorWhite;
			int del = wt ? -16 : 16;
			int fr = to + del;
			if ((g_board[fr + 1] & 0x1f) == (ec | piecePawn))
				return true;
			if ((g_board[fr - 1] & 0x1f) == (ec | piecePawn))
				return true;
			if ((g_board[to + 14] & 0x1f) == (ec | pieceKnight))
				return true;
			if ((g_board[to - 14] & 0x1f) == (ec | pieceKnight))
				return true;
			if ((g_board[to + 18] & 0x1f) == (ec | pieceKnight))
				return true;
			if ((g_board[to - 18] & 0x1f) == (ec | pieceKnight))
				return true;
			if ((g_board[to + 31] & 0x1f) == (ec | pieceKnight))
				return true;
			if ((g_board[to - 31] & 0x1f) == (ec | pieceKnight))
				return true;
			if ((g_board[to + 33] & 0x1f) == (ec | pieceKnight))
				return true;
			if ((g_board[to - 33] & 0x1f) == (ec | pieceKnight))
				return true;
			foreach (int d in arrDirBishop)
			{
				fr = to + d;
				if ((g_board[fr] & 0x1f) == (ec | pieceKing))
					return true;
				while (g_board[fr] > 0)
				{
					if ((g_board[fr] & colorEmpty) > 0)
					{
						fr += d;
						continue;
					}
					if ((g_board[fr] & 0x1f) == (ec | pieceBishop) || (g_board[fr] & 0x1f) == (ec | pieceQueen))
						return true;
					break;
				}
			}
			foreach (int d in arrDirRook)
			{
				fr = to + d;
				if ((g_board[fr] & 0x1f) == (ec | pieceKing))
					return true;
				while (g_board[fr] > 0)
				{
					if ((g_board[fr] & colorEmpty) > 0)
					{
						fr += d;
						continue;
					}
					if ((g_board[fr] & 0x1f) == (ec | pieceRook) || (g_board[fr] & 0x1f) == (ec | pieceQueen))
						return true;
					break;
				}
			}
			return false;
		}

		void GenerateMove(List<int> moves, int fr, int to, int flag)
		{
			if (((g_board[to] & 7) > 0) || ((flag & moveflagPassing) > 0) || ((flag & maskPromotion) > 0))
				moves.Add(fr | (to << 8) | flag);
			else
				moves.Insert(0, fr | (to << 8) | flag);
		}

		int GetColorScore(bool wt)
		{
			lastScore = 0;
			int pieceM = 0;
			int pieceN = 0;
			int pieceB = 0;
			int w = wt ? 1 : 0;
			usColor = wt ? colorWhite : colorBlack;
			enColor = wt ? colorBlack : colorWhite;
			for (int y = 0; y < 8; y++)
				for (int x = 0; x < 8; x++)
				{
					int n = (y << 3) | x;
					int fr = arrFieldL[n];
					int f = g_board[fr];
					if ((f & usColor) == 0)
						continue;
					int piece = f & 0xf;
					int rank = f & 7;
					lastScore += arrBonus[g_phase, piece, fr];
					int c;
					switch (rank)
					{
						case 1:
							pieceM++;
							int supported = 0;
							if ((bitBoard[piece] & bbIsolated[x]) == 0)
								lastScore += arrIsolated[g_phase];
							if ((bitBoard[piece] & bbSupportedL[n, w]) > 0)
								supported++;
							if ((bitBoard[piece] & bbSupportedR[n, w]) > 0)
								supported++;
							if (supported > 0)
								lastScore += 21 * supported;
							else
							if ((bitBoard[piece] & bbDoubled[n, w]) > 0)
								lastScore += arrDoubled[g_phase];
							if ((bitBoard[piece ^ 8] & bbPassed[n, w]) == 0)
								lastScore += arrPassed[g_phase, w, n];
							break;
						case 2:
							pieceN++;
							c = CountUniMoves(fr, arrDirKinght, 1);
							lastScore += arrMobility[g_phase, rank, c];
							break;
						case 3:
							pieceB++;
							c = CountUniMoves(fr, arrDirBishop, 7);
							lastScore += arrMobility[g_phase, rank, c];
							break;
						case 4:
							pieceM++;
							c = CountUniMoves(fr, arrDirRook, 7);
							lastScore += arrMobility[g_phase, rank, c];
							if ((bitBoard[(usColor | piecePawn) & 0xf] & bbFile[x]) == 0)
							{
								if ((bitBoard[(enColor | piecePawn) & 0xf] & bbFile[x]) > 0)
									lastScore += arrRookFile[g_phase, 1];
								else
									lastScore += arrRookFile[g_phase, 2];
							}
							break;
						case 5:
							pieceM++;
							c = CountUniMoves(fr, arrDirQueen, 7);
							lastScore += arrMobility[g_phase, rank, c];
							break;
					}
				}
			if (pieceB > 1)
				lastScore += 64;
			lastInsufficient = ((pieceM == 0) && (pieceN + (pieceB << 1) < 3));
			return lastScore;
		}

		int GenerateAttackMoves(bool wt, out List<int> moves)
		{
			moves = new List<int>(64);
			lastScore = 0;
			int pieceM = 0;
			int pieceN = 0;
			int pieceB = 0;
			int w = wt ? 1 : 0;
			usColor = wt ? colorWhite : colorBlack;
			enColor = wt ? colorBlack : colorWhite;
			for (int y = 0; y < 8; y++)
				for (int x = 0; x < 8; x++)
				{
					int n = (y << 3) | x;
					int fr = arrFieldL[n];
					int f = g_board[fr];
					if ((f & usColor) == 0)
						continue;
					int piece = f & 0xf;
					int rank = f & 7;
					lastScore += arrBonus[g_phase, piece, fr];
					int c;
					switch (rank)
					{
						case 1:
							pieceM++;
							int supported = 0;
							if ((bitBoard[piece] & bbIsolated[x]) == 0)
								lastScore += arrIsolated[g_phase];
							if ((bitBoard[piece] & bbSupportedL[n, w]) > 0)
								supported++;
							if ((bitBoard[piece] & bbSupportedR[n, w]) > 0)
								supported++;
							if (supported > 0)
								lastScore += 21 * supported;
							else
							if ((bitBoard[piece] & bbDoubled[n, w]) > 0)
								lastScore += arrDoubled[g_phase];
							if ((bitBoard[piece ^ 8] & bbPassed[n, w]) == 0)
								lastScore += arrPassed[g_phase, w, n];
							int del = wt ? -16 : 16;
							int to = fr + del;
							if ((g_board[to - 1] & enColor) > 0)
								GeneratePwnMoves(moves, fr, to - 1, 0);
							else if ((to - 1) == g_passing)
								GeneratePwnMoves(moves, fr, g_passing, moveflagPassing);
							if ((g_board[to + 1] & enColor) > 0)
								GeneratePwnMoves(moves, fr, to + 1, 0);
							else if ((to + 1) == g_passing)
								GeneratePwnMoves(moves, fr, g_passing, moveflagPassing);
							break;
						case 2:
							pieceN++;
							c = GenerateUniMoves(moves, true, fr, arrDirKinght, 1);
							lastScore += arrMobility[g_phase, rank, c];
							break;
						case 3:
							pieceB++;
							c = GenerateUniMoves(moves, true, fr, arrDirBishop, 7);
							lastScore += arrMobility[g_phase, rank, c];
							break;
						case 4:
							pieceM++;
							c = GenerateUniMoves(moves, true, fr, arrDirRook, 7);
							lastScore += arrMobility[g_phase, rank, c];
							if ((bitBoard[(usColor | piecePawn) & 0xf] & bbFile[x]) == 0)
							{
								if ((bitBoard[(enColor | piecePawn) & 0xf] & bbFile[x]) > 0)
									lastScore += arrRookFile[g_phase, 1];
								else
									lastScore += arrRookFile[g_phase, 2];
							}
							break;
						case 5:
							pieceM++;
							c = GenerateUniMoves(moves, true, fr, arrDirQueen, 7);
							lastScore += arrMobility[g_phase, rank, c];
							break;
						case 6:
							kingPos = fr;
							GenerateUniMoves(moves, true, fr, arrDirQueen, 1);
							break;
					}
				}
			if (pieceB > 1)
				lastScore += 64;
			lastInsufficient = ((pieceM == 0) && (pieceN + (pieceB << 1) < 3));
			return lastScore;
		}

		List<int> GenerateAllMoves(bool wt)
		{
			usColor = wt ? colorWhite : colorBlack;
			enColor = wt ? colorBlack : colorWhite;
			List<int> moves = new List<int>(64);
			for (int n = 0; n < 64; n++)
			{
				int fr = arrFieldL[n];
				int f = g_board[fr];
				if ((f & usColor) > 0)
					f &= 7;
				else
					continue;
				switch (f)
				{
					case 1:
						int del = wt ? -16 : 16;
						int to = fr + del;
						if ((g_board[to] & colorEmpty) > 0)
						{
							GeneratePwnMoves(moves, fr, to, 0);
							if ((g_board[fr - del - del] == 0) && (g_board[to + del] & colorEmpty) > 0)
								GeneratePwnMoves(moves, fr, to + del, 0);
						}
						if ((g_board[to - 1] & enColor) > 0)
							GeneratePwnMoves(moves, fr, to - 1, 0);
						else if ((to - 1) == g_passing)
							GeneratePwnMoves(moves, fr, g_passing, moveflagPassing);
						if ((g_board[to + 1] & enColor) > 0)
							GeneratePwnMoves(moves, fr, to + 1, 0);
						else if ((to + 1) == g_passing)
							GeneratePwnMoves(moves, fr, g_passing, moveflagPassing);
						break;
					case 2:
						GenerateUniMoves(moves, false, fr, arrDirKinght, 1);
						break;
					case 3:
						GenerateUniMoves(moves, false, fr, arrDirBishop, 7);
						break;
					case 4:
						GenerateUniMoves(moves, false, fr, arrDirRook, 7);
						break;
					case 5:
						GenerateUniMoves(moves, false, fr, arrDirQueen, 7);
						break;
					case 6:
						kingPos = fr;
						GenerateUniMoves(moves, false, fr, arrDirQueen, 1);
						int cr = wt ? g_castleRights : g_castleRights >> 2;
						if ((cr & 1) > 0)
							if (((g_board[fr + 1] & colorEmpty) > 0) && ((g_board[fr + 2] & colorEmpty) > 0) && !IsAttacked(wt, fr) && !IsAttacked(wt, fr + 1) && !IsAttacked(wt, fr + 2))
								GenerateMove(moves, fr, fr + 2, moveflagCastleKing);
						if ((cr & 2) > 0)
							if (((g_board[fr - 1] & colorEmpty) > 0) && ((g_board[fr - 2] & colorEmpty) > 0) && ((g_board[fr - 3] & colorEmpty) > 0) && !IsAttacked(wt, fr) && !IsAttacked(wt, fr - 1) && !IsAttacked(wt, fr - 2))
								GenerateMove(moves, fr, fr - 2, moveflagCastleQueen);
						break;
				}
			}
			return moves;
		}


		void GeneratePwnMoves(List<int> moves, int fr, int to, int flag)
		{
			int y = to >> 4;
			if ((y == 4) || (y == 11))
			{
				GenerateMove(moves, fr, to, moveflagPromoteQueen);
				GenerateMove(moves, fr, to, moveflagPromoteRook);
				GenerateMove(moves, fr, to, moveflagPromoteBishop);
				GenerateMove(moves, fr, to, moveflagPromoteKnight);
			}
			else
				GenerateMove(moves, fr, to, flag);
		}

		int CountUniMoves(int fr, int[] dir, int count)
		{
			int result = 0;
			for (int n = 0; n < dir.Length; n++)
			{
				int to = fr;
				int c = count;
				while (c-- > 0)
				{
					to += dir[n];
					if ((g_board[to] & colorEmpty) > 0)
						result++;
					else if ((g_board[to] & enColor) > 0)
					{
						result++;
						break;
					}
					else
						break;
				}
			}
			return result;
		}

		int GenerateUniMoves(List<int> moves, bool attack, int fr, int[] dir, int count)
		{
			int result = 0;
			for (int n = 0; n < dir.Length; n++)
			{
				int to = fr;
				int c = count;
				while (c-- > 0)
				{
					to += dir[n];
					if ((g_board[to] & colorEmpty) > 0)
					{
						result++;
						if (!attack)
							GenerateMove(moves, fr, to, 0);
					}
					else if ((g_board[to] & enColor) > 0)
					{
						result++;
						GenerateMove(moves, fr, to, 0);
						break;
					}
					else
						break;
				}
			}
			return result;
		}

		public int GetMoveFromString(string moveString)
		{
			List<int> moves = GenerateAllMoves(whiteTurn);
			for (int i = 0; i < moves.Count; i++)
			{
				if (FormatMove(moves[i]) == moveString)
					return moves[i];
			}
			return 0;
		}

		public void MakeMove(int move)
		{
			int fr = move & 0xFF;
			int to = (move >> 8) & 0xFF;
			int flags = move & 0xFF0000;
			int piecefr = g_board[fr];
			int piece = piecefr & 0xf;
			int rank = piecefr & 7;
			int captured = g_board[to];
			CBitBoard.Del(ref bitBoard[piece], arrFieldS[fr]);
			if ((flags & moveflagCastleKing) > 0)
			{
				g_board[to - 1] = g_board[to + 1];
				g_board[to + 1] = colorEmpty;
				int secPiece = (piece & colorWhite) | pieceRook;
				CBitBoard.Del(ref bitBoard[secPiece], arrFieldS[to + 1]);
				CBitBoard.Add(ref bitBoard[secPiece], arrFieldS[to - 1]);
			}
			else if ((flags & moveflagCastleQueen) > 0)
			{
				g_board[to + 1] = g_board[to - 2];
				g_board[to - 2] = colorEmpty;
				int secPiece = (piece & colorWhite) | pieceRook;
				CBitBoard.Del(ref bitBoard[secPiece], arrFieldS[to - 2]);
				CBitBoard.Add(ref bitBoard[secPiece], arrFieldS[to + 1]);
			}
			else if ((flags & moveflagPassing) > 0)
			{
				int capi = whiteTurn ? to + 16 : to - 16;
				captured = g_board[capi];
				g_board[capi] = colorEmpty;
				CBitBoard.Del(ref bitBoard[(enColor | piecePawn) & 0xf], arrFieldS[capi]);
			}
			ref CUndo undo = ref undoStack[undoIndex++];
			undo.captured = captured;
			undo.hash = g_hash;
			undo.passing = g_passing;
			undo.castle = g_castleRights;
			undo.move50 = g_move50;
			undo.kingPos = kingPos;
			g_hash ^= g_hashBoard[fr, piece];
			g_passing = 0;
			if (captured != colorEmpty)
			{
				g_move50 = 0;
				g_phase--;
				CBitBoard.Del(ref bitBoard[captured & 0xf], arrFieldS[to]);
			}
			else if (rank == piecePawn)
			{
				if (to == (fr + 32)) g_passing = (fr + 16);
				if (to == (fr - 32)) g_passing = (fr - 16);
				g_move50 = 0;
			}
			else
				g_move50++;
			if ((flags & moveflagPromotion) > 0)
			{
				int newPiece = piecefr & (~0x7);
				if ((flags & moveflagPromoteKnight) > 0)
					newPiece |= pieceKnight;
				else if ((flags & moveflagPromoteQueen) > 0)
					newPiece |= pieceQueen;
				else if ((flags & moveflagPromoteBishop) > 0)
					newPiece |= pieceBishop;
				else
					newPiece |= pieceRook;
				g_board[to] = newPiece;
				CBitBoard.Add(ref bitBoard[newPiece & 0xf], arrFieldS[to]);
				g_hash ^= g_hashBoard[to, newPiece & 0xf];
			}
			else
			{
				g_board[to] = g_board[fr];
				CBitBoard.Add(ref bitBoard[piece], arrFieldS[to]);
				g_hash ^= g_hashBoard[to, piece];
			}
			if (rank == pieceKing)
				kingPos = to;
			g_board[fr] = colorEmpty;
			g_castleRights &= boardCastle[fr] & boardCastle[to];
			whiteTurn ^= true;
			g_moveNumber++;
		}

		void UnmakeMove(int move)
		{
			int fr = move & 0xFF;
			int to = (move >> 8) & 0xFF;
			int flags = move & 0xFF0000;
			int piece = g_board[to];
			int capi = to;
			CUndo undo = undoStack[--undoIndex];
			g_passing = undo.passing;
			g_castleRights = undo.castle;
			g_move50 = undo.move50;
			g_hash = undo.hash;
			kingPos = undo.kingPos;
			int captured = undo.captured;
			CBitBoard.Del(ref bitBoard[piece & 0xf], arrFieldS[to]);
			if ((flags & moveflagCastleKing) > 0)
			{
				g_board[to + 1] = g_board[to - 1];
				g_board[to - 1] = colorEmpty;
				int secPiece = (piece & 0x8) | pieceRook;
				CBitBoard.Del(ref bitBoard[secPiece], arrFieldS[to - 1]);
				CBitBoard.Add(ref bitBoard[secPiece], arrFieldS[to + 1]);
			}
			else if ((flags & moveflagCastleQueen) > 0)
			{
				g_board[to - 2] = g_board[to + 1];
				g_board[to + 1] = colorEmpty;
				int secPiece = (piece & 0x8) | pieceRook;
				CBitBoard.Del(ref bitBoard[secPiece], arrFieldS[to + 1]);
				CBitBoard.Add(ref bitBoard[secPiece], arrFieldS[to - 2]);
			}
			if ((flags & moveflagPromotion) > 0)
			{
				int secPiece = (piece & (~0x7)) | piecePawn;
				g_board[fr] = secPiece;
				CBitBoard.Add(ref bitBoard[secPiece & 0xf], arrFieldS[fr]);
			}
			else
			{
				g_board[fr] = g_board[to];
				CBitBoard.Add(ref bitBoard[piece & 0xf], arrFieldS[fr]);
			}
			if ((flags & moveflagPassing) > 0)
			{
				capi = whiteTurn ? to - 16 : to + 16;
				g_board[to] = colorEmpty;
				CBitBoard.Del(ref bitBoard[piece & 0xf], arrFieldS[to]);
			}
			g_board[capi] = captured;
			if (captured != colorEmpty)
			{
				g_phase++;
				CBitBoard.Add(ref bitBoard[captured & 0xf], arrFieldS[capi]);
			}
			whiteTurn ^= true;
			g_moveNumber--;
		}

		int Quiesce(int ply, int depth, int alpha, int beta, bool enInsufficient, int enScore)
		{
			int score = GenerateAttackMoves(whiteTurn, out List<int> mu) - enScore;
			bool usInsufficient = lastInsufficient;
			int usScore = lastScore;
			if (enInsufficient && usInsufficient)
				return 0;
			int alphaDe = 0;
			string alphaPv = "";
			if (score >= beta)
				return beta;
			if (score > alpha)
				alpha = score;
			int index = mu.Count;
			while (index-- > 0)
			{
				if ((++g_totalNodes & 0x1fff) == 0)
					if (GetStop() || synStop.GetStop())
						g_stop = true;
				int cm = mu[index];
				MakeMove(cm);
				g_depth = 0;
				g_pv = "";
				if (IsAttacked(!whiteTurn, kingPos))
					score = -0xffff;
				else if (ply < depth)
					score = -Quiesce(ply + 1, depth, -beta, -alpha, usInsufficient, usScore);
				else
				{
					GetColorScore(whiteTurn);
					if (usInsufficient && lastInsufficient)
						score = 0;
					else
						score = usScore - lastScore;
				}
				UnmakeMove(cm);
				if (g_stop) return -0xffff;
				if (alpha < score)
				{
					alpha = score;
					alphaDe = g_depth + 1;
					alphaPv = $"{FormatMove(cm)} {g_pv}";
					if (alpha >= beta)
						return beta;
				}
			}
			g_depth = alphaDe;
			g_pv = alphaPv;
			return alpha;
		}

		int GetScore(List<int> mu, int ply, int depth, int alpha, int beta)
		{
			int n = mu.Count;
			int myMoves = n;
			int alphaDe = 0;
			string alphaPv = "";
			int osScore;
			List<int> me;
			while (n-- > 0)
			{
				int cm = mu[n];
				if ((++g_totalNodes & 0x1fff) == 0)
					if ((bsDepth > 0) && (GetStop() || synStop.GetStop()))
						g_stop = true;
				MakeMove(cm);
				g_depth = 0;
				g_pv = "";
				osScore = -0xffff;
				if (IsAttacked(!whiteTurn, kingPos))
					myMoves--;
				else if ((g_move50 > 99) || IsRepetition())
					osScore = 0;
				else
				{
					if (ply < depth)
					{
						me = GenerateAllMoves(whiteTurn);
						osScore = -GetScore(me, ply + 1, depth, -beta, -alpha);
					}
					else
					{
						GetColorScore(!whiteTurn);
						osScore = -Quiesce(1, depth, -beta, -alpha, lastInsufficient, lastScore);
					}
				}
				UnmakeMove(cm);
				if (g_stop) return -0xffff;
				if (alpha < osScore)
				{
					string alphaFm = FormatMove(cm);
					alpha = osScore;
					alphaPv = alphaFm + ' ' + g_pv;
					alphaDe = g_depth + 1;
					if (ply == 1)
					{
						if (osScore > 0xf000)
							g_scoreFm = "mate " + ((0xffff - osScore) >> 1);
						else if (osScore < -0xf000)
							g_scoreFm = "mate " + ((-0xfffe - osScore) >> 1);
						else
							g_scoreFm = "cp " + (osScore >> 2);
						bsIn = n;
						bsFm = alphaFm;
						bsPv = alphaPv;
						bsDepth = alphaDe;
						long t = Convert.ToInt64(stopwatch.Elapsed.TotalMilliseconds);
						long nps = 0;
						if (t > 0)
							nps = Convert.ToInt64(((double)g_totalNodes / t) * 1000);
						Console.WriteLine("info currmove " + bsFm + " currmovenumber " + bsIn + " nodes " + g_totalNodes + " time " + t + " nps " + nps + " depth " + depth + " seldepth " + alphaDe + " score " + g_scoreFm + " pv " + bsPv);
					}
				}
				if (alpha >= beta) break;
			}
			if (myMoves == 0)
				if (IsAttacked(whiteTurn, kingPos))
					alpha = -0xffff + ply;
				else
					alpha = 0;
			g_depth = alphaDe;
			g_pv = alphaPv;
			return alpha;
		}

		void Start(int depth, int time, ulong nodes)
		{
			List<int> mu = GenerateAllMoves(whiteTurn);
			if (mu.Count == 0)
			{
				Console.WriteLine($"info string no moves");
				return;
			}
			int depthLimit = mu.Count == 1 ? 3 : 100;
			g_stop = false;
			g_totalNodes = 0;
			g_timeout = time;
			g_depthout = depth;
			g_nodeout = nodes;
			int os;
			int alpha = -0xffff;
			int beta = 0xffff;
			g_mainDepth = 1;
			do
			{
				os = GetScore(mu, 1, g_mainDepth, alpha, beta);
				if ((os > alpha) && (os < beta))
				{
					alpha = os - 50;
					beta = os + 50;
				}
				else
				{
					alpha = -0xffff;
					beta = 0xffff;
					if (!g_stop) continue;
				}
				int m = mu[bsIn];
				mu.RemoveAt(bsIn);
				mu.Add(m);
				ulong t = Convert.ToUInt64(stopwatch.Elapsed.TotalMilliseconds);
				ulong nps = 0;
				if (t > 0)
					nps = Convert.ToUInt64((g_totalNodes / t) * 1000);
				Console.WriteLine($"info depth {g_mainDepth} nodes {g_totalNodes} time {t} nps {nps}");
				if (++g_mainDepth > depthLimit)
					break;
			} while (!(GetStop() || synStop.GetStop()) && (os > -0xf000) && (os < 0xf000));
			string[] ponder = bsPv.Split(' ');
			string pm = ponder.Length > 1 ? " ponder " + ponder[1] : "";
			Console.WriteLine($"bestmove {bsFm}{pm}");
		}

		bool GetStop()
		{
			return ((g_timeout > 0) && (stopwatch.Elapsed.TotalMilliseconds > g_timeout)) || ((g_depthout > 0) && (g_mainDepth > g_depthout)) || ((g_nodeout > 0) && (g_totalNodes > g_nodeout));
		}

		void Thread()
		{
			Start(inDepth, inTime, inNodes);
		}

		public void StartThread(int depth, int time, ulong nodes)
		{
			inDepth = depth;
			inTime = time;
			inNodes = nodes;
			startThread = new Thread(Thread);
			startThread.Start();
		}

	}
}