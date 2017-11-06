using System;
using System.Collections.Generic;

namespace processAI1
{
    class IA
    {
        String[] tabCoord = new string[] { "a8","b8","c8","d8","e8","f8","g8","h8",
                                           "a7","b7","c7","d7","e7","f7","g7","h7",
                                           "a6","b6","c6","d6","e6","f6","g6","h6",
                                           "a5","b5","c5","d5","e5","f5","g5","h5",
                                           "a4","b4","c4","d4","e4","f4","g4","h4",
                                           "a3","b3","c3","d3","e3","f3","g3","h3",
                                           "a2","b2","c2","d2","e2","f2","g2","h2",
                                           "a1","b1","c1","d1","e1","f1","g1","h1" };
        

        private const int PP = 10; //pion passant
        private const int P = 1; //pion
        private const int TG = 21; //tour gauche (different pour le roque)
        private const int TD = 22; //tour droite
        private const int CG = 31; //cavalier gauche (différents pour l'image)
        private const int CD = 32; //cavalier droit
        private const int F = 4; //fou
        private const int D = 5; //dame
        private const int R = 6; //roi

        private int m_joueur;

        public IA()
        {
        }

        public void jouerCoup(int[] plateau, String[] coord, int joueur)
        {
            m_joueur = joueur;
            List<String> mesPieces = new List<String>();
            for (int i = 0; i < plateau.Length; i++)
            {
                if (joueur * plateau[i] > 0) //liste des pieces qui peuvent bouger
                {
                    mesPieces.Add(tabCoord[i]);
                }
            }

            List<String> reste = new List<String>();
            for (int i = 0; i < plateau.Length; i++)
            {
                if (plateau[i] <= 0) reste.Add(tabCoord[i]);
            }

            Random rnd = new Random();
            coord[0] = mesPieces[rnd.Next(mesPieces.Count)];
            coord[1] = tabCoord[rnd.Next(reste.Count)];
        }

        //retourne la liste des coups possibles pour la piece situee a l'index donne
        private List<String> listeCoups(int[] plateau, int index)
        {
            int i = index / 8; //numero de ligne
            int j = index % 8; //numero de colonne

            switch (Math.Abs(plateau[index]))
            {
                case P:
                    return coupsPion(plateau, i, j);
                case CG:
                case CD:
                    return coupsCavalier(plateau, m_joueur, i, j);
                case F:
                    return coupsFou(plateau, i, j);
                case TG:
                case TD:
                    return coupsTour(plateau, i, j);
                case D:
                    return coupsDame(plateau, i, j);                    
                case R:
                    return coupsRoi(plateau, i, j);
                default:
                    Console.WriteLine("Piece de code <" + Math.Abs(plateau[index]) + "> non identifiée");
                    Console.ReadLine();
                    return new List<string>();
            }
        }

        private List<String> coupsPion(int[] plateau, int i, int j)
        {
            List<String> lc = new List<String>();



            return lc;
        }

        private int coordToIndex(int i, int j)
        {
            return i * 8 + j;
        }

        private Boolean dansTableau(int i, int j)
        {
            if(i < 8 && i >= 0 && j < 8 && j >= 0)
            {
                return true;
            }
            return false;
        }

        //index: index de la case de départ
        //i, j: coord de la case d'arrivee
        private Boolean pieceAlliee(int[] plateau, int joueur, int i, int j)
        {
            return !(dansTableau(i, j) && (joueur * plateau[coordToIndex(i, j)] <= 0));
        }

        //retourne true ssi la piece en position (i, j) est menacee dans la position t
        private Boolean pieceMenacee(int[] plateau, int i, int j)
        {

            return true;
        }


        private List<String> coupsCavalier(int[] plateau, int joueur, int i, int j)
        {
            List<String> lc = new List<String>();

            if(!pieceAlliee(plateau, joueur, i - 1, j - 2))
            {
                lc.Add(tabCoord[coordToIndex(i - 1, j - 2)]);
            }

            if (!pieceAlliee(plateau, joueur, i + 1, j - 2))
            {
                lc.Add(tabCoord[coordToIndex(i + 1, j - 2)]);
            }

            if (!pieceAlliee(plateau, joueur, i - 2, j - 1))
            {
                lc.Add(tabCoord[coordToIndex(i - 2, j - 1)]);
            }

            if (!pieceAlliee(plateau, joueur, i + 2, j - 1))
            {
                lc.Add(tabCoord[coordToIndex(i + 2, j - 1)]);
            }

            if (!pieceAlliee(plateau, joueur, i - 2, j + 1))
            {
                lc.Add(tabCoord[coordToIndex(i - 2, j + 1)]);
            }

            if (!pieceAlliee(plateau, joueur, i + 2, j + 1))
            {
                lc.Add(tabCoord[coordToIndex(i + 2, j + 1)]);
            }

            if (!pieceAlliee(plateau, joueur, i - 1, j + 2))
            {
                lc.Add(tabCoord[coordToIndex(i - 1, j + 2)]);
            }

            if (!pieceAlliee(plateau, joueur, i + 1, j + 2))
            {
                lc.Add(tabCoord[coordToIndex(i + 1, j + 2)]);
            }

            return lc;
        }

        private List<String> coupsFou(int[] plateau, int i, int j)
        {
            List<String> lc = new List<String>();



            return lc;
        }


        private List<String> coupsTour(int[] plateau, int i, int j)
        {
            List<String> lc = new List<String>();



            return lc;
        }

        private List<String> coupsDame(int[] plateau, int i, int j)
        {
            List<String> lc = new List<String>();

            lc.AddRange(coupsFou(plateau, i, j));
            lc.AddRange(coupsTour(plateau, i, j));

            return lc;
        }

        private List<String> coupsRoi(int[] plateau, int i, int j)
        {
            List<String> lc = new List<String>();



            return lc;
        }
    }
}

