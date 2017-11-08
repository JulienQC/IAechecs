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
        
        private int[,] directionFou = new int[,] { { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 } };
        private int[,] directionTour = new int[,] { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 } };
        private int[,] casesCavalier = new int[,] { { -1, -2 }, { +1, -2 }, { -2, -1 }, { +2, -1 },
                                                    { -2, +1 }, { +2, +1 }, { -1, +2 }, { +1, +2 } };

        private int m_joueur;

        public IA()
        {
        }


        public void jouerCoup(int[] plateau, String[] coord, int joueur)
        {
            m_joueur = joueur;
            List<Coup> coupsPotentiels = new List<Coup>();
            
            for (int i = 0; i < plateau.Length; i++)
            {
                if (joueur * plateau[i] > 0) //liste des pieces du joueur
                {
                    coupsPotentiels.AddRange(listeCoups(plateau, i));
                }
            }

            // verifier que le coup ne met pas le roi du joueur en echec
            List<Coup> coupsPossibles = filtrerMenaces(plateau, coupsPotentiels);

            // choisir un coup aleatoire parmi les coups autorises
            Random rnd = new Random();
            Coup coupJoue = coupsPossibles[rnd.Next(coupsPossibles.Count)];
            coord[0] = tabCoord[coupJoue.indexDepart];
            coord[1] = tabCoord[coupJoue.indexArrivee];
        }

        private List<Coup> filtrerMenaces(int[] plateau, List<Coup> lc)
        {
            List<Coup> coupsPossibles = new List<Coup>();

            int lRoi = 0, cRoi = 0; //ligne et colonne du roi du joueur
            for (int i = 0; i < plateau.Length; i++)
            {
                if (m_joueur * plateau[i] == R)
                {
                    lRoi = i / 8;
                    cRoi = i % 8;
                }
            }

            int nlRoi, ncRoi; // nouvelle ligne et colonne du roi du joueur
            foreach (Coup c in lc)
            {
                nlRoi = lRoi; ncRoi = cRoi;
                //jouer fictivement le coup sur un nouveau plateau
                int[] nouveauPlateau = new int[plateau.Length];
                Array.Copy(plateau, nouveauPlateau, plateau.Length);
                nouveauPlateau[c.indexDepart] = 0;
                nouveauPlateau[c.indexArrivee] = plateau[c.indexDepart];

                //calculer la nouvelle position du roi du joueur s'il s'est deplace
                if (m_joueur * plateau[c.indexDepart] == R)
                {
                    nlRoi = c.indexArrivee / 8;
                    ncRoi = c.indexArrivee % 8;
                }

                //ajouter le coup aux coups possibles s'il ne met pas en danger le roi du joueur
                if (!pieceMenacee(nouveauPlateau, m_joueur, nlRoi, ncRoi))
                {
                    coupsPossibles.Add(c);
                }
            }

            return coupsPossibles;
        }
        
        //retourne la liste des coups possibles pour la piece situee a l'index donne
        private List<Coup> listeCoups(int[] plateau, int index)
        {
            int i = index / 8; //numero de ligne
            int j = index % 8; //numero de colonne

            switch (Math.Abs(plateau[index]))
            {
                case P:
                    return coupsPion(plateau, m_joueur, i, j);
                case CG:
                case CD:
                    return coupsCavalier(plateau, m_joueur, i, j);
                case F:
                    return coupsFou(plateau, m_joueur, i, j);
                case TG:
                case TD:
                    return coupsTour(plateau, m_joueur, i, j);
                case D:
                    return coupsDame(plateau, m_joueur, i, j);                    
                case R:
                    return coupsRoi(plateau, m_joueur, i, j);
                default:
                    return new List<Coup>();
            }
        }

        private List<Coup> coupsPion(int[] plateau, int joueur, int i, int j)
        {
            List<Coup> lc = new List<Coup>();
            lc.AddRange(coupsPionManger(plateau, joueur, i, j));
            lc.AddRange(coupsPionDeplacer(plateau, joueur, i, j));
            return lc;
        }


        private List<Coup> coupsPionManger(int[] plateau, int joueur, int i, int j)
        {
            List<Coup> lc = new List<Coup>();
            int index = coordToIndex(i, j);

            // le pion mange a gauche
            if (dansTableau(i - joueur, j - 1) && pieceEnnemie(plateau, joueur, i - joueur, j - 1))
            {
                lc.Add(new Coup(index, coordToIndex(i - joueur, j - 1)));
            }
            // le pion mange a droite
            if (dansTableau(i - joueur, j + 1) && pieceEnnemie(plateau, joueur, i - joueur, j + 1))
            {
                lc.Add(new Coup(index, coordToIndex(i - joueur, j + 1)));
            }
            // pas de prise en passant (peu d'interet)

            return lc;
        }

        private List<Coup> coupsPionDeplacer(int[] plateau, int joueur, int i, int j)
        {
            List<Coup> lc = new List<Coup>();
            int index = coordToIndex(i, j);

            // le pion se deplace de +1 en i si c'est le joueur noir (joueur = -1), et de -1 sinon
            if (dansTableau(i - joueur, j) && caseVide(plateau, i - joueur, j)) 
            {
                lc.Add(new Coup(index, coordToIndex(i - joueur, j)));
            }

            int ligneDepart;
            if(joueur == 1) ligneDepart = 6;
            else            ligneDepart = 1;

            // cas où le pion est sur sa ligne de depart: il peut avancer de 2 cases
            if(i == ligneDepart && caseVide(plateau, i - joueur * 2, j))
            {
                lc.Add(new Coup(index, coordToIndex(i - joueur * 2, j)));
            }

            return lc;
        }

        private List<Coup> coupsCavalier(int[] plateau, int joueur, int i, int j)
        {
            List<Coup> lc = new List<Coup>();
            int index = coordToIndex(i, j);

            int x, y;
            for(int k = 0; k < casesCavalier.GetLength(0); k++)
            {
                x = i + casesCavalier[k, 0];
                y = j + casesCavalier[k, 1];

                if (dansTableau(x, y) && !pieceAlliee(plateau, joueur, x, y))
                {

                    lc.Add(new Coup(index, coordToIndex(x, y)));
                }
            }
            
            return lc;
        }

        private List<Coup> coupsFou(int[] plateau, int joueur, int i, int j)
        {
            List<Coup> lc = new List<Coup>();
            int index = coordToIndex(i, j);

            int dirX, dirY, x, y;
            for (int dirIndex = 0; dirIndex < this.directionFou.GetLength(0); dirIndex++)
            {
                dirX = this.directionFou[dirIndex, 0];
                dirY = this.directionFou[dirIndex, 1];
                y = i + dirY;
                x = j + dirX;

                // avancer dans la diagonale tant qu'il y a des case vides
                while (dansTableau(y, x) && caseVide(plateau, y, x))
                {
                    lc.Add(new Coup(index, coordToIndex(y, x)));
                    y += dirY;
                    x += dirX;
                }
                // si l'obstacle est une piece ennemie, on peut la manger
                if (dansTableau(y, x) && pieceEnnemie(plateau, joueur, y, x))
                {
                    lc.Add(new Coup(index, coordToIndex(y, x)));
                }
            }

            return lc;
        }


        private List<Coup> coupsTour(int[] plateau, int joueur, int i, int j)
        {
            List<Coup> lc = new List<Coup>();
            int index = coordToIndex(i, j);

            int dirX, dirY, x, y;
            for (int dirIndex = 0; dirIndex < this.directionTour.GetLength(0); dirIndex++)
            {
                dirX = this.directionTour[dirIndex, 0];
                dirY = this.directionTour[dirIndex, 1];
                y = i + dirY;
                x = j + dirX;
                
                // avancer dans la diagonale tant qu'il y a des case vides
                while (dansTableau(y, x) && caseVide(plateau, y, x))
                {
                    lc.Add(new Coup(index, coordToIndex(y, x)));
                    y += dirY;
                    x += dirX;
                }
                // si l'obstacle est une piece ennemie, on peut la manger
                if (dansTableau(y, x) && pieceEnnemie(plateau, joueur, y, x))
                {
                    lc.Add(new Coup(index, coordToIndex(y, x)));
                }
            }

            return lc;
        }

        private List<Coup> coupsDame(int[] plateau, int joueur, int i, int j)
        {
            List<Coup> lc = new List<Coup>();

            lc.AddRange(coupsFou(plateau, joueur, i, j));
            lc.AddRange(coupsTour(plateau, joueur, i, j));

            return lc;
        }

        private List<Coup> coupsRoi(int[] plateau, int joueur, int i, int j)
        {
            List<Coup> lc = new List<Coup>();
            int index = coordToIndex(i, j);

            // coup du roi vers une case voisine
            for (int x = -1; x < 2; x++)
            {
                for(int y = -1; y < 2; y++)
                {
                    if(dansTableau(i + x, j + y) && !pieceAlliee(plateau, joueur, i + x, j + y))
                    {
                        lc.Add(new Coup(index, coordToIndex(i + x, j + y)));
                    }
                }
            }

            // TODO: gerer petit roque et grand roque

            return lc;
        }


        private int coordToIndex(int i, int j)
        {
            return i * 8 + j;
        }



        private Boolean dansTableau(int i, int j)
        {
            return (i < 8 && i >= 0 && j < 8 && j >= 0);
        }

        //index: index de la case de départ
        //i, j: coord de la case d'arrivee
        private Boolean pieceAlliee(int[] plateau, int joueur, int i, int j)
        {
            return (joueur * plateau[coordToIndex(i, j)] > 0);
        }

        private Boolean pieceEnnemie(int[] plateau, int joueur, int i, int j)
        {
            return (joueur * plateau[coordToIndex(i, j)] < 0);
        }

        private Boolean caseVide(int[] plateau, int i, int j)
        {
            return (plateau[coordToIndex(i, j)] == 0);
        }

        //retourne true ssi la piece en position (i, j) est menacee dans la position t
        private Boolean pieceMenacee(int[] plateau, int joueur, int i, int j)
        {
            List<Coup> cavaliersPotentiels = coupsCavalier(plateau, joueur, i, j);
            foreach (Coup c in cavaliersPotentiels)
            {
                if (plateau[c.indexArrivee] == -1 * joueur * CG ||
                    plateau[c.indexArrivee] == -1 * joueur * CD)
                {
                    return true;
                }
            }
            List<Coup> pionsPotentiels = coupsPionManger(plateau, joueur, i, j);
            foreach (Coup c in pionsPotentiels)
            {
                if (plateau[c.indexArrivee] == -1 * joueur * P)
                {
                    return true;
                }
            }
            List<Coup> fousPotentiels = coupsFou(plateau, joueur, i, j);
            foreach (Coup c in fousPotentiels)
            {
                if (plateau[c.indexArrivee] == -1 * joueur * F ||
                    plateau[c.indexArrivee] == -1 * joueur * D)
                {
                    return true;
                }
            }
            List<Coup> toursPotentiels = coupsTour(plateau, joueur, i, j);
            foreach (Coup c in toursPotentiels)
            {
                if (plateau[c.indexArrivee] == -1 * joueur * TG ||
                    plateau[c.indexArrivee] == -1 * joueur * TD ||
                    plateau[c.indexArrivee] == -1 * joueur * D)
                {
                    return true;
                }
            }
            List<Coup> roisPotentiels = coupsRoi(plateau, joueur, i, j);
            foreach (Coup c in roisPotentiels)
            {
                if (plateau[c.indexArrivee] == -1 * joueur * R)
                {
                    return true;
                }
            }
            return false;
        }

        public void jouerCoupHeuristique(int[] plateau, String[] coord, int joueur)
        {
            m_joueur = joueur;
            List<Coup> coupsPotentiels = new List<Coup>();

            for (int i = 0; i < plateau.Length; i++)
            {
                if (joueur * plateau[i] > 0) //liste des pieces du joueur
                {
                    coupsPotentiels.AddRange(listeCoups(plateau, i));
                }
            }

            // verifier que le coup ne met pas le roi du joueur en echec
            List<Coup> coupsPossibles = filtrerMenaces(plateau, coupsPotentiels);

            //attribuer une valeur a chaque coup
            int scoreMax = eval(coupsPossibles[0], plateau);
            Coup coupJoue = coupsPossibles[0];
            for (int i = 0; i < coupsPossibles.Count; i++)
            {
                int score = eval(coupsPossibles[i], plateau);
                coupsPossibles[i].setPuissance(score);
                if (score > scoreMax)
                {
                    scoreMax = score;
                    coupJoue = coupsPossibles[i];
                } 
            }
            
             

            // choisir un coup aleatoire parmi les coups autorisés
            coord[0] = tabCoord[coupJoue.indexDepart];
            coord[1] = tabCoord[coupJoue.indexArrivee];
        }

        private int eval(Coup c, int[] plateau) {
            return 1;
            //evaluation a faire
        }
    }
}

public struct Coup
{
    public int indexDepart; // indexe de la case de départ du coup
    public int indexArrivee; // indexe de la case d'arrivée du coup
    public int puissance;  // valeur du coup en terme d'efficacité

    public Coup(int dep, int arr)
    {
        indexDepart = dep;
        indexArrivee = arr;
        puissance = 0;
    }

    public void setPuissance(int p)
    {
        puissance = p;
    }

    public int getPuissance()
    {
        return puissance;
    }
}