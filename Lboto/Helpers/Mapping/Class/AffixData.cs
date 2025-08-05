namespace Lboto.Helpers.Mapping
{
    public class AffixData
    {
        private readonly string name;

        private readonly string description;

        private bool shouldRerollMagic;

        private bool shouldRerollRare;

        public string Name
        {
            get => name;
        }

        public string Description
        {
            get => description;
        }

        public bool RerollMagic
        {
            get => shouldRerollMagic;
            set => shouldRerollMagic = value;
        }

        public bool RerollRare
        {
            get => shouldRerollRare;
            set => shouldRerollRare = value;
        }

        public AffixData(string name, string description)
        {
            this.name = name;
            this.description = description;
        }
    }
}
