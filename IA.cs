using System;
using System.Collections.Generic;
using System.Linq;

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

        private int[,] directionFou = new int[,] { { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 } };
        private int[,] directionTour = new int[,] { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 } };
        private int[,] casesCavalier = new int[,] { { -1, -2 }, { +1, -2 }, { -2, -1 }, { +2, -1 },
                                                    { -2, +1 }, { +2, +1 }, { -1, +2 }, { +1, +2 } };

        private int m_joueur; // couleur du joueur (blanc: 1; noir: -1)
        private Boolean roiABouge;
        private Boolean tourDroiteABouge;
        private Boolean tourGaucheABouge;

        private double alpha = 0.25; // coefficient d'evaluation de la gestion des echanges
        private double beta = 0.25; // coefficient d'evaluation de la protection du roi
        private double gamma = 0.25; // coefficient d'evaluation de l'activite des pieces
        private double omega = 0.25; // coefficient d'evaluation de l'occupation du centre

        private int[] centrageCase = new int[]{1,1,1,1,1,1,1,1,
                                                1,2,2,2,2,2,2,1,
                                                1,2,4,4,4,4,2,1,
                                                1,2,4,8,8,4,2,1,
                                                1,2,4,8,8,4,2,1,
                                                1,2,4,4,4,4,2,1,
                                                1,2,2,2,2,2,2,1,
                                                1,1,1,1,1,1,1,1};


        public Intelligence()
        {
            roiABouge = false;
            tourDroiteABouge = false;
            tourGaucheABouge = false;
        }


        public Coup choisirCoup(int[] plateau, int joueur)
        {
            List<Coup> coupsPossibles = listerCoupsPossibles(plateau, joueur);

            // choisir un coup aleatoire parmi les coups autorises
            Coup coupJoue = meilleurCoup(plateau, joueur, coupsPossibles);
            majInfoRoque(plateau, joueur, coupJoue);

            return coupJoue;
        }

        public List<Coup> listerCoupsPossibles(int[] plateau, int joueur)
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

            return coupsPossibles;
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
                if (c.estPetitRoque() || !c.estGrandRoque())
                { // la verification des menaces est deja incluse dans le calcul des coups de roque

                    coupsPossibles.Add(c);
                }
                else
                {
                    nlRoi = lRoi; ncRoi = cRoi;
                    //jouer fictivement le coup sur un nouveau plateau
                    int[] nouveauPlateau = plateauApresCoup(plateau, m_joueur, c);

                    //calculer la nouvelle position du roi du joueur s'il s'est deplace
                    if (m_joueur * plateau[c.indexDepart] == R)
                    {
                        nlRoi = c.indexArrivee / 8;
                        ncRoi = c.indexArrivee % 8;
                    }

                    //ajouter le coup aux coups possibles s'il ne met pas en danger le roi du joueur
                    if (!estMenacee(nouveauPlateau, m_joueur, nlRoi, ncRoi))
                    {
                        coupsPossibles.Add(c);
                    }
                }
            }

            return coupsPossibles;
        }

        private int[] plateauApresCoup(int[] plateau, int joueur, Coup c)
        {
            int[] nouveauPlateau = new int[plateau.Length];
            Array.Copy(plateau, nouveauPlateau, plateau.Length);
            nouveauPlateau[c.indexDepart] = 0;
            nouveauPlateau[c.indexArrivee] = plateau[c.indexDepart];
            //deplacer les tours en cas de roque
            if (c.estPetitRoque())
            {
                nouveauPlateau[coordToIndex(7 * ((joueur + 1) / 2), 5)] = joueur * TD;
            }
            else if (c.estGrandRoque())
            {
                nouveauPlateau[coordToIndex(7 * ((joueur + 1) / 2), 3)] = joueur * TG;
            }
            return nouveauPlateau;
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
            foreach (Coup c in lc)
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
            if (joueur == 1) ligneDepart = 6;
            else ligneDepart = 1;

            // cas où le pion est sur sa ligne de depart: il peut avancer de 2 cases
            if (i == ligneDepart && caseVide(plateau, i - joueur * 2, j))
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
            for (int k = 0; k < casesCavalier.GetLength(0); k++)
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

        // coup possible pour un fou de la couleur du joueur en case (i, j)
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
               !estMenacee(plateau, joueur, i, j) &&
               !estMenacee(plateau, joueur, i, j + 1) &&
               !estMenacee(plateau, joueur, i, j + 2))
            {
                Coup c = new Coup(index, coordToIndex(i, j + 2));
                c.setPetitRoque();
                lc.Add(c);
                tourDroiteABouge = true;
                roiABouge = true;
            }

            //grand roque
            if (!roiABouge && !tourGaucheABouge &&
                !estMenacee(plateau, joueur, i, j) &&
                !estMenacee(plateau, joueur, i, j - 1) &&
                !estMenacee(plateau, joueur, i, j - 2))
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

        // renvoie la liste des indexes des pions de l'ennemi du joueur qui menacent la case (i, j) 
        private List<int> menacesPion(int[] plateau, int joueur, int i, int j)
        {
            List<int> indexes = new List<int>();
            List<Coup> pionsPotentiels = coupsPionManger(plateau, joueur, i, j);
            foreach (Coup c in pionsPotentiels)
            {
                if (plateau[c.indexArrivee] == -1 * joueur * P)
                {
                    indexes.Add(c.indexArrivee);
                }
            }
            return indexes;
        }

        // renvoie la liste des indexes des cavaliers de l'ennemi du joueur qui menacent la case (i, j)
        private List<int> menacesCavalier(int[] plateau, int joueur, int i, int j)
        {
            List<int> indexes = new List<int>();
            List<Coup> cavaliersPotentiels = coupsCavalier(plateau, joueur, i, j);
            foreach (Coup c in cavaliersPotentiels)
            {
                if (plateau[c.indexArrivee] == -1 * joueur * CG ||
                    plateau[c.indexArrivee] == -1 * joueur * CD)
                {
                    indexes.Add(c.indexArrivee);
                }
            }
            return indexes;
        }

        // renvoie la liste des indexes des fous / dames de l'ennemi du joueur qui menacent la case (i, j)
        private List<int> menacesFou(int[] plateau, int joueur, int i, int j)
        {
            List<int> indexes = new List<int>();
            List<Coup> fousPotentiels = coupsFou(plateau, joueur, i, j);
            foreach (Coup c in fousPotentiels)
            {
                if (plateau[c.indexArrivee] == -1 * joueur * F ||
                    plateau[c.indexArrivee] == -1 * joueur * D)
                {
                    indexes.Add(c.indexArrivee);
                }
            }
            return indexes;
        }

        // renvoie la liste des indexes des tours / dames de l'ennemi du joueur qui menacent la case (i, j)
        private List<int> menacesTour(int[] plateau, int joueur, int i, int j)
        {
            List<int> indexes = new List<int>();
            List<Coup> toursPotentiels = coupsTour(plateau, joueur, i, j);
            foreach (Coup c in toursPotentiels)
            {
                if (plateau[c.indexArrivee] == -1 * joueur * TG ||
                    plateau[c.indexArrivee] == -1 * joueur * TD ||
                    plateau[c.indexArrivee] == -1 * joueur * D)
                {
                    indexes.Add(c.indexArrivee);
                }
            }
            return indexes;
        }

        //renvoie l'indexe de la case du roi ennemi si ce dernier peut aller en case (i, j)
        private List<int> menacesRoi(int[] plateau, int joueur, int i, int j)
        {
            List<int> indexes = new List<int>();
            List<Coup> roisPotentiels = coupsRoiClassiques(plateau, joueur, i, j);
            foreach (Coup c in roisPotentiels)
            {
                if (plateau[c.indexArrivee] == -1 * joueur * R)
                {
                    indexes.Add(c.indexArrivee);
                }
            }
            return indexes;
        }

        private List<int> menaces(int[] plateau, int joueur, int i, int j)
        {
            List<int> indexes = new List<int>();

            indexes.AddRange(menacesPion(plateau, joueur, i, j));
            indexes.AddRange(menacesCavalier(plateau, joueur, i, j));
            indexes.AddRange(menacesFou(plateau, joueur, i, j));
            indexes.AddRange(menacesRoi(plateau, joueur, i, j));

            return indexes;
        }

        //retourne true ssi la piece en position (i, j) du joueur est menacee dans la position decrite par le plateau
        private Boolean estMenacee(int[] plateau, int joueur, int i, int j)
        {
            return (menacesPion(plateau, joueur, i, j).Any() ||
                menacesCavalier(plateau, joueur, i, j).Any() ||
                menacesFou(plateau, joueur, i, j).Any() ||
                menacesTour(plateau, joueur, i, j).Any() ||
                menacesRoi(plateau, joueur, i, j).Any());
        }

        private Coup meilleurCoup(int[] plateau, int joueur, List<Coup> coupsPossibles)
        {
            Coup c = coupsPossibles[0];

            int scoreMax = eval(plateau, joueur, coupsPossibles[0]);
            for (int i = 0; i < coupsPossibles.Count; i++)
            {
                int score = eval(plateau, joueur, coupsPossibles[i]);
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
        private int eval(int[] plateau, int joueur, Coup c) {
            int[] nouveauPlateau = plateauApresCoup(plateau, joueur, c);
            double score = alpha * evalEchange(plateau, c) +
                           beta * evalProtection(nouveauPlateau, joueur) +
                           gamma * evalCentre(nouveauPlateau, c, joueur) +
                           omega * evalActivite(nouveauPlateau, c, joueur);
            return (int)(score);
        }

        // evaluation de l'echange implique par le coup
        private int evalEchange(int[] plateau, Coup c)
        {
            /*
            int cible = valeurPiece(pieceCible);
            List<Piece> protecteurs;
            List<Piece> attaquants;
            int val;
            while(protecteurs.Any() && attaquants.Any())
            {
                cible -= valeurPiece(protecteurs.RemoveAt(0)); // on perd la piece mangee par le protecteur
                cible += valeurPiece(attaquants.RemoveAt(0)); ; // on gagne la piece mangee par l'attaquant
            }
            if (protecteurs.Any())
            {
                cible -= valeurPiece(protecteurs.RemoveAt(0)); // s'il ne reste que des protecteurs, on perd le dernier des attaquants
            }           
            
            return cible;
            */
            
            return 1;
        }

        private int positionRoi(int[] plateau, int joueur)
        {
            int i = 0;
            while(i < 64 && plateau[i] != joueur * R) { i++; }
            return i;
        }

        // evaluation de la protection du roi apres le coup
        private int evalProtection(int[] plateau, int joueur)
        {
            double score = 0;

            int indexRoi = positionRoi(plateau, joueur);
            int i = indexRoi / 8;
            int j = indexRoi % 8;

            // somme les menaces des pieces adverse ayant le roi en ligne de mire avec un coeff de 4
            score += 4 * enLigneMireFou(plateau, joueur, indexRoi);
            score += 4 * enLigneMireTour(plateau, joueur, indexRoi);

            List<int> indexesMenaces;
            foreach(int index in casesAdjacentes(i, j))
            {
                indexesMenaces = menaces(plateau, joueur, i, j);
                // somme les valeurs des pieces menacant les cases adjacentes du roi avec un coeff de 2
                foreach(int idx in indexesMenaces)
                {
                    score += valeurPiece(plateau, idx) * 2;
                }
                // somme les menaces des pieces adverse ayant une case adjacente au roi en ligne de mire
                score += enLigneMireFou(plateau, joueur, index);
                score += enLigneMireTour(plateau, joueur, index);
            }
            

            return (int) score;
        }

        // evaluation de l'occupation du centre apres le coup
        private int evalCentre(int[] plateauApresCoup, Coup c, int joueur)
        {
            List<Coup> coupsPossibles = listerCoupsPossibles(plateauApresCoup, joueur);
            int score = 0;
            for (int i = 0; i < coupsPossibles.Count; i++)
            {
                score += centrageCase[coupsPossibles[i].indexArrivee];
            }
            return (int)((score / 1176) * 100);
        }

        // evaluation de l'activite des pieces apres le coup
        private int evalActivite(int[] plateauApresCoup, Coup c, int joueur)
        {
            List<Coup> coupsPossibles = listerCoupsPossibles(plateauApresCoup, joueur);
            int score = coupsPossibles.Count;
            return (int)((score / 147) * 100);
        }

        private List<int> casesAdjacentes(int i, int j)
        {
            List<int> cases = new List<int>();
            for (int y = -1; y < 2; y++)
            {
                for (int x = -1; x < 2; x++)
                {
                    if (!(x == 0 && y == 0) && dansTableau(y + i, x + j))
                    {
                        cases.Add(coordToIndex(y + i, x + j));
                    }
                }
            }
            return cases;
        }

        private double valeurPiece(int[] plateau, int index){
            switch(plateau[index])
            {
                case P:
                    return 1;
                case CG:
                case CD:
                    return 3;
                case F:
                    return 3.5;
                case TG:
                case TD:
                    return 5;
                case D:
                    return 8;
                default:
                    return 0;
            }
        }

        // renvoie un double dans [0, inf[ indiquant la force de la menace des fous / dames
        // de l'ennemi du joueur qui pese sur la case (i, j) 
        private double enLigneMireFou(int[] plateau, int joueur, int index)
        {
            double score = 0;
            int i = index / 8;
            int j = index % 8;

            int dirX, dirY, x, y, coeff;
            for (int dirIndex = 0; dirIndex < directionFou.GetLength(0); dirIndex++)
            {
                dirX = directionFou[dirIndex, 0];
                dirY = directionFou[dirIndex, 1];
                y = i + dirY;
                x = j + dirX;
                coeff = 4; // on suppose qu'une piece n'est plus une menace si plus de 2 pieces alliees font obstacle

                // avancer dans la diagonale tant qu'il y a des case vides
                while (dansTableau(y, x))
                {
                    if(pieceAlliee(plateau, joueur, y, x))
                    {
                        coeff /= 2;
                    }
                    if(coeff < 4 && // ne pas prendre en compte les pieces en contact direct
                       (plateau[coordToIndex(y, x)] * joueur == F ||
                        plateau[coordToIndex(y, x)] * joueur == D))
                    {
                        score += valeurPiece(plateau, coordToIndex(y, x)) * coeff;
                    }
                    y += dirY;              
                    x += dirX;
                }
            }

            return score;
        }

        // renvoie un double dans [0, inf[ indiquant la force de la menace des tours / dames
        // de l'ennemi du joueur qui pese sur la case (i, j) 
        private double enLigneMireTour(int[] plateau, int joueur, int index)
        {
            double score = 0;
            int i = index / 8;
            int j = index % 8;

            int dirX, dirY, x, y, coeff;
            for (int dirIndex = 0; dirIndex < directionTour.GetLength(0); dirIndex++)
            {
                dirX = directionTour[dirIndex, 0];
                dirY = directionTour[dirIndex, 1];
                y = i + dirY;
                x = j + dirX;
                coeff = 4; // on suppose qu'une piece n'est plus une menace si plus de 2 pieces alliees font obstacle

                // avancer dans la diagonale tant qu'il y a des case vides
                while (dansTableau(y, x))
                {
                    if (pieceAlliee(plateau, joueur, y, x))
                    {
                        coeff /= 2;
                    }
                    if (coeff < 4 && // ne pas prendre en compte les pieces en contact direct
                        (plateau[coordToIndex(y, x)] * joueur == TG ||
                         plateau[coordToIndex(y, x)] * joueur == TD ||
                         plateau[coordToIndex(y, x)] * joueur == D))
                    {
                        score += valeurPiece(plateau, coordToIndex(y, x)) * coeff;
                    }
                    y += dirY;
                    x += dirX;
                }
            }

            return score;
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

    public class Piece
    {
        private int id; //type de la piece (pion, cavalier, fou, ...)
        private int indexCase;
        private int valeur; //valeur de la piece dans une position donnée 

        public Piece(int i, int c, int v)
        {
            id = i;
            c = 
            valeur = v;
        }

        public int getValeur()
        {
            return valeur;
        }
        
    }
}
