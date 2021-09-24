namespace ExtensionLib.Entity
{
    public class Material
    {
        public double Price { get; set; }
        public double Coefficient { get; set; }
        public string Unit { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string Section { get; set; }
        public int Number { get; set; }

        public override string ToString() 
            => $"#{Number} | Имя: {Name} | Цена: {Price} | Коэф.: {Coefficient} | Ед.Изм.: {Unit} | Примечание: {Note} | Секция: {Section}";
        
    }
}
