using System;
using System.Collections.Generic;

namespace processAI1
{
    class Intelligence
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

        private int[,] directionFou = new int[,] { { +1, +1 }, { +1, -1 }, { -1, +1 }, { -1, -1 } };
        private int[,] directionTour = new int[,] { { +1, 0 }, { 0, +1 }, { -1, 0 }, { 0, -1 } };
        private int[,] casesCavalier = new int[,] { { -1, -2 }, { +1, -2 }, { -2, -1 }, { +2, -1 },
                                                    { -2, +1 }, { +2, +1 }, { -1, +2 }, { +1, +2 } };

        private Dictionary<int, int> heuristiqueEchange = new Dictionary<int, int>() { { PP, 1 }, { P, 1 }, { CG, 3 }, { CD, 3 },
                                                                                       { F, 4 }, { TG, 5 }, { TD, 5 }, { D, 8 } };

        private int m_joueur; // couleur du joueur (blanc: 1; noir: -1)
        private Boolean roiABouge;
        private Boolean tourDroiteABouge;
        private Boolean tourGaucheABouge;

        private double alpha = 0.25; // coefficient d'evaluation de la gestion des echanges
        private double beta = 0.25; // coefficient d'evaluation de la protection du roi
        private double gamma = 0.25; // coefficient d'evaluation de l'activite des pieces
        private double omega = 0.25; // coefficient d'evaluation de l'occupation du centre


        public Intelligence()
        {
            roiABouge = false;
            tourDroiteABouge = false;
            tourGaucheABouge = false;
        }


        public Coup choisirCoup(int[] plateau, int joueur)
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
            Coup coupJoue = meilleurCoup(plateau, joueur, coupsPossibles);
            majInfoRoque(plateau, joueur, coupJoue);

            return coupJoue;
        }

        // donne l'information sur le deplacement du roi et des tours pour savoir si un roque est faisable
        private void majInfoRoque(int[] plateau, int joueur, Coup c)
        {
            if (!roiABouge && plateau[c.indexDepart] * joueur == R)
            {
                roiABouge = true;
            }
            if (!tourDroiteABouge && plateau[c.indexDepart] * joueur == TD)
            {
                tourDroiteABouge = true;
            }
            if (!tourGaucheABouge && plateau[c.indexDepart] * joueur == TG)
            {
                tourGaucheABouge = true;
            }
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
                if(c.estPetitRoque() || !c.estGrandRoque())
                { // la verification des menaces est deja incluse dans le calcul des coups de roque

                    coupsPossibles.Add(c);
                }
                else 
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
            gererPromotion(plateau, joueur, lc);
            return lc;
        }

        private void gererPromotion(int[] plateau, int joueur, List<Coup> lc)
        {
            foreach(Coup c in lc)
            {
                if (joueur == 1 &&
                    c.indexArrivee / 8 == 0)
                {
                    //promotion du joueur blanc
                    c.setPromotion(); //active la promotion
                }
                if (joueur == -1 &&
                    c.indexArrivee / 8 == 7)
                {
                    //promotion du joueur noir
                    c.setPromotion();
                }
            }
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

            lc.AddRange(coupsRoiClassiques(plateau, joueur, i, j));
            lc.AddRange(coupsRoiRoque(plateau, joueur, i, j));

            return lc;
        }

        private List<Coup> coupsRoiClassiques(int[] plateau, int joueur, int i, int j)
        {
            List<Coup> lc = new List<Coup>();
            int index = coordToIndex(i, j);

            // coup du roi vers une case voisine
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if (dansTableau(i + x, j + y) && !pieceAlliee(plateau, joueur, i + x, j + y))
                    {
                        lc.Add(new Coup(index, coordToIndex(i + x, j + y)));
                    }
                }
            }

            return lc;
        }

        private List<Coup> coupsRoiRoque(int[] plateau, int joueur, int i, int j)
        {

            List<Coup> lc = new List<Coup>();
            int index = coordToIndex(i, j);

            // petit roque:
            if (!roiABouge && !tourDroiteABouge &&
               !pieceMenacee(plateau, joueur, i, j) &&
               !pieceMenacee(plateau, joueur, i, j + 1) &&
               !pieceMenacee(plateau, joueur, i, j + 2))
            {
                Coup c = new Coup(index, coordToIndex(i, j + 2));
                c.setPetitRoque();
                lc.Add(c);
                tourDroiteABouge = true;
                roiABouge = true;
            }

            //grand roque
            if (!roiABouge && !tourGaucheABouge &&
                !pieceMenacee(plateau, joueur, i, j) &&
                !pieceMenacee(plateau, joueur, i, j - 1) &&
                !pieceMenacee(plateau, joueur, i, j - 2))
            {
                Coup c = new Coup(index, coordToIndex(i, j - 2));
                c.setGrandRoque();
                lc.Add(c);
                tourGaucheABouge = true;
                roiABouge = true;
            }

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

        private List<Coup> piecesDeplacementPotentiels(int[] plateau, int joueur, int i, int j)
        {
            List<Coup> coupsPotentiels = new List<Coup>();

            coupsPotentiels.AddRange(coupsCavalier(plateau, joueur, i, j));
            coupsPotentiels.AddRange(coupsPionManger(plateau, joueur, i, j));
            coupsPotentiels.AddRange(coupsFou(plateau, joueur, i, j));
            coupsPotentiels.AddRange(coupsTour(plateau, joueur, i, j));
            coupsPotentiels.AddRange(coupsRoiClassiques(plateau, joueur, i, j));

            return coupsPotentiels;
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
            List<Coup> roisPotentiels = coupsRoiClassiques(plateau, joueur, i, j);
            foreach (Coup c in roisPotentiels)
            {
                if (plateau[c.indexArrivee] == -1 * joueur * R)
                {
                    return true;
                }
            }
            return false;
        }

        private Coup meilleurCoup(int[] plateau, int joueur, List<Coup> coupsPossibles)
        {
            Coup c = coupsPossibles[0];

            int scoreMax = eval(plateau, coupsPossibles[0]);
            for (int i = 0; i < coupsPossibles.Count; i++)
            {
                int score = eval(plateau, coupsPossibles[i]);
                coupsPossibles[i].setPuissance(score);
                if (score > scoreMax)
                {
                    scoreMax = score;
                    c = coupsPossibles[i];
                }
            }

            return c;
        }


        // retourne un score pour le coup (un grand score correspond a un bon coup)
        private int eval(int[] plateau, Coup c) {

            int pieceEchangee = plateau[c.indexArrivee];
            Console.Out.WriteLine(pieceEchangee);
            Console.Out.WriteLine(c.indexArrivee);
            Console.Out.WriteLine("test");

            double score = alpha * evalEchange(plateau, c) +
                           beta * evalProtection(plateau, c) +
                           gamma * evalCentre(plateau, c) +
                           omega * evalActivite(plateau, c);
            return (int)(score);
        }

        // evaluation de l'echange implique par le coup
        private int evalEchange(int[] plateau, Coup c)
        {
            int pieceEchangee = plateau[c.indexArrivee];

            if (pieceEchangee == 0)
            {
                return 0;
            }
            else
            {
                return this.heuristiqueEchange[pieceEchangee];
            }
        }

        // evaluation de la protection du roi apres le coup
        private int evalProtection(int[] plateau, Coup c)
        {
            return 1;
        }

        // evaluation de l'occupation du centre apres le coup
        private int evalCentre(int[] plateau, Coup c)
        {
            return 1;
        }

        // evaluation de l'activite des pieces apres le coup
        private int evalActivite(int[] plateau, Coup c)
        {
            return 1;
        }
    }
}

public class Coup
{
    public int indexDepart; // indexe de la case de départ du coup
    public int indexArrivee; // indexe de la case d'arrivée du coup
    private Boolean promotion; 
    private int roque;
    private int puissance;  // valeur du coup en terme d'efficacité

    public Coup(int dep, int arr)
    {
        indexDepart = dep;
        indexArrivee = arr;
        promotion = false;
        roque = 0;
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

    public void setPetitRoque()
    {
        roque = 1;
    }

    public void setGrandRoque()
    {
        roque = 2;
    }

    public Boolean estPetitRoque()
    {
        return roque == 1;
    }

    public Boolean estGrandRoque()
    {
        return roque == 2;
    }

    public void setPromotion()
    {
        promotion = true;
    }

    public Boolean estPromotion()
    {
        return promotion;
    }
}

