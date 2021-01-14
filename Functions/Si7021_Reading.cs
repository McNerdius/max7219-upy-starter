namespace Karmatach.MaxPlay
{
    public class Si7021_Reading
    {
        public string Time { get; set; }
        public double F { get; set; }
        public double RH { get; set; }
        public ushort Battery { get; set; }
        public bool? SetHeaterStateTo { get; set; }
    }
}
