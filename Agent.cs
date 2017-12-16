using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace processAI1
{
    class Agent
    {
        // etat interne 
        private int couleurJoueur;
        private int[] plateau;
        private Coup prochainCoup;
        
        // composantes de l'agent
        private Capteur capteur;
        private Effecteur effecteur;
        private Intelligence intelligence;


        public Agent(int joueur, String [] coord, int [] plateau)
        {
            couleurJoueur = joueur;
            intelligence = new Intelligence(couleurJoueur);
            capteur = new Capteur(plateau);
            effecteur = new Effecteur(coord);
        }

        public void observerPlateau()
        {
            plateau = capteur.lirePlateau();
        }

        public void majEtat()
        {
            // calcul d'une eventuelle version du plateau contenant des precalculs des fonctions heuristiques
        }

        public void choisirCoup()
        {
            prochainCoup = intelligence.choisirCoup(plateau, couleurJoueur);
        }

        public void jouerCoup()
        {
            effecteur.jouerCoup(prochainCoup);
        }
    }

    public class Capteur
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

        private int[] plateau;

        public Capteur(int[] t)
        {
            plateau = t;
        }

        public int[] lirePlateau()
        {
            return plateau;
        }
    }

    public class Effecteur
    {
        String[] tabCoord = new string[] { "a8","b8","c8","d8","e8","f8","g8","h8",
                                           "a7","b7","c7","d7","e7","f7","g7","h7",
                                           "a6","b6","c6","d6","e6","f6","g6","h6",
                                           "a5","b5","c5","d5","e5","f5","g5","h5",
                                           "a4","b4","c4","d4","e4","f4","g4","h4",
                                           "a3","b3","c3","d3","e3","f3","g3","h3",
                                           "a2","b2","c2","d2","e2","f2","g2","h2",
                                           "a1","b1","c1","d1","e1","f1","g1","h1" };
        private String[] coup;

        public Effecteur(String[] coord)
        {
            coup = coord;
        }

        public void jouerCoup(Coup c)
        { 
            if (c.estPetitRoque())
            {
                coup[0] = "petit roque";
                coup[1] = "";
            }
            else if (c.estGrandRoque())
            {
                coup[0] = "grand roque";
                coup[1] = "";
            }
            else
            {
                coup[0] = tabCoord[c.indexDepart];
                coup[1] = tabCoord[c.indexArrivee];
            }
            if (c.estPromotion())
            {
                coup[2] = "D"; //promotion du pion en dame par défaut
            }
            else
            {
                coup[2] = "";
            }
        }
        
    }
}
