namespace UZSG.Saves
{
    public class SaveData
    {
        public string Type => GetType().Name;

        public static bool FieldIsNull(object obj)
        {
            return obj == null;
        }
    }
}