namespace Pilens.Components.Pages
{
    public partial class Console
    {
        public string TextValue { get; set; }
        private ConsoleModes enumValue { get; set; } = ConsoleModes.Skaidrot;
        public enum ConsoleModes {Jautājumu, Skaidrot, Pārbaudīt}
    }
}
