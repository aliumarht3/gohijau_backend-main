namespace GoHijauBackend.Application.Helpers
{
    public static class MoneyHelper
    {
        public static decimal RoundMoney(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}
