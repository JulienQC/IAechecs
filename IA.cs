using System;
using System.Collections.Generic;

namespace processAI1
{
    class IA
    {
        private const int PP = 10; //pion passant
        private const int P = 1; //pion
        private const int TG = 21; //tour gauche (different pour le roque)
        private const int TD = 22; //tour droite
        private const int CG = 31; //cavalier gauche (différents pour l'image)
        private const int CD = 32; //cavalier droit
        private const int F = 4; //fou
        private const int D = 5; //dame
        private const int R = 6; //roi

        public IA()
        {
        }

        public void jouerCoup(int[] tabVal, String[] tabCoord, String[] coord, int joueur)
        {
            List<String> mesPieces = new List<String>();
            for (int i = 0; i < tabVal.Length; i++)
            {
                if (joueur * tabVal[i] > 0) //liste des pieces qui peuvent bouger
                {
                    mesPieces.Add(tabCoord[i]);
                }
            }

            List<String> reste = new List<String>();
            for (int i = 0; i < tabVal.Length; i++)
            {
                if (tabVal[i] <= 0) reste.Add(tabCoord[i]);
            }

            Random rnd = new Random();
            coord[0] = mesPieces[rnd.Next(mesPieces.Count)];
            coord[1] = tabCoord[rnd.Next(reste.Count)];
        }

        //retourne la liste des coups possibles pour la piece situee a l'index donne
        private List<String> listeCoups(int[] tabVal, String[] tabCoord, int index)
        {
            int i = index / 8; //numero de ligne
            int j = index % 8; //numero de colonne
            switch (Math.Abs(tabVal[index]))
            {
                case P:
                    return coupsPion(tabVal, tabCoord, i, j);
                case CG:
                case CD:
                    return coupsCavalier(tabVal, tabCoord, i, j);
                case F:
                    return coupsFou(tabVal, tabCoord, i, j);
                case TG:
                case TD:
                    return coupsTour(tabVal, tabCoord, i, j);
                case D:
                    return coupsDame(tabVal, tabCoord, i, j);                    
                case R:
                    return coupsRoi(tabVal, tabCoord, i, j);
                default:
                    Console.WriteLine("Piece de code <" + Math.Abs(tabVal[index]) + "> non identifiée");
                    Console.ReadLine();
                    return new List<string>();
            }
        }

        private List<String> coupsPion(int[] tabVal, String[] tabCoord, int i, int j)
        {
            List<String> lc = new List<String>();



            return lc;
        }

        private List<String> coupsCavalier(int[] tabVal, String[] tabCoord, int i, int j)
        {
            List<String> lc = new List<String>();



            return lc;
        }

        private List<String> coupsFou(int[] tabVal, String[] tabCoord, int i, int j)
        {
            List<String> lc = new List<String>();



            return lc;
        }


        private List<String> coupsTour(int[] tabVal, String[] tabCoord, int i, int j)
        {
            List<String> lc = new List<String>();



            return lc;
        }

        private List<String> coupsDame(int[] tabVal, String[] tabCoord, int i, int j)
        {
            List<String> lc = new List<String>();

            lc.AddRange(coupsFou(tabVal, tabCoord, i, j));
            lc.AddRange(coupsTour(tabVal, tabCoord, i, j));

            return lc;
        }

        private List<String> coupsRoi(int[] tabVal, String[] tabCoord, int i, int j)
        {
            List<String> lc = new List<String>();



            return lc;
        }
    }
}

