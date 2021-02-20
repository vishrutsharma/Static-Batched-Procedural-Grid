using System;
using UnityEngine;

namespace Snake3D.Models
{
    [Serializable]
    public class WorldModel
    {
        public int gridX;
        public int gridY;
        public float minRange;
        public float maxRange;
    }
}