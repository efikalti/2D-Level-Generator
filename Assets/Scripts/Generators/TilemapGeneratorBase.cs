using Assets.Scripts.Reporting;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Generators
{
    public class TilemapGeneratorBase : MonoBehaviour
    {
        // Level generation parameters
        public int numberOfDungeonsToGenerate = 1;
        public int secondsToWaitingBetweenDungeons = 3;

        // Tilemap main object
        protected Tilemap tilemap;

        protected DataParser fileParser;

        protected System.Random randomGenerator = new System.Random();


        public void StartGenerating()
        {
            StartCoroutine(GenerateLevels());
        }

        public IEnumerator GenerateLevels()
        {
            WaitForSeconds wait = new WaitForSeconds(secondsToWaitingBetweenDungeons);
            for (int i = 0; i < numberOfDungeonsToGenerate; i++)
            {
                GenerateLevel();
                yield return wait;
            }
            StopCoroutine(GenerateLevels());
        }

        public virtual void GenerateLevel()
        {

        }
    }
}
