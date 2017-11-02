using System;
using System.Collections.Generic;

namespace processAI1
{
    class IA
    {
        public IA()
        {
        }

        public void jouerCoup(int[] tabVal, String[] tabCoord, String[] coord)
        {

            String[] res = new String[2];
            List<String> mesPieces = new List<String>();
            for (int i = 0; i < tabVal.Length; i++)
            {
                if (tabVal[i] > 0) mesPieces.Add(tabCoord[i]);
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
    }
}