using System;

namespace ZooMap.Models
{
    public class Enclosure
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double LocationX { get; set; }
        public double LocationY { get; set; }
        public string AnimalType { get; set; }
        public string ImagePath { get; set; }
        public DateTime LastModified { get; set; }
    }
}