namespace MihaZupan.TelegramLocalStorage.Types
{
    public class StorageKey
    {
        public StorageKey(ulong first, ulong second)
        {
            First = first;
            Second = second;
        }

        public ulong First;
        public ulong Second;

        public override string ToString()
        {
            return First.ToString() + " " + Second.ToString();
        }
    }
}
