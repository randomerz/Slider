using UnityEngine;

namespace GdkSample_GameSave
{
    public struct RPGCharacterData
    {
        public string Name;
        public int Level;
        public int XP;
        public int PortraitIndex;
        public ArmorType Armor;

        public void LevelUp()
        {
            Level += 1;
            XP += Level * Level * 50;
        }

        public void Reset()
        {
            Name = "<None>";
            Level = 1;
            XP = 0;
            Armor = ArmorType.None;
            PortraitIndex = -1;
        }

        public void GrantRandomArmor()
        {
            // ArmorType.None is not a valid armor type, so we add 1 to the length of the enum.
            Armor = (ArmorType)Random.Range(1, System.Enum.GetNames(typeof(ArmorType)).Length);
        }

        public static RPGCharacterData GenerateRandomCharacter()
        {
            return new RPGCharacterData()
            {
                Name = GenerateRandomName(8),
                Armor = ArmorType.None,
                Level = 1,
                XP = 0,
                PortraitIndex = Random.Range(0, 30),
            };
        }

        // https://stackoverflow.com/questions/14687658/random-name-generator-in-c-sharp
        public static string GenerateRandomName(int len)
        {
            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
            string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
            string Name = "";
            Name += consonants[Random.Range(0, consonants.Length)].ToUpper();
            Name += vowels[Random.Range(0, vowels.Length)];
            int b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
            while (b < len)
            {
                Name += consonants[Random.Range(0, consonants.Length)];
                b++;
                Name += vowels[Random.Range(0, vowels.Length)];
                b++;
            }
            return Name;
        }
    }

    [System.Serializable]
    public enum ArmorType
    {
        None,
        Robed,
        Padded,
        Leather,
        Hide,
        RingMail,
        ChainMail,
        ScaleMail,
        Splint,
        HalfPlate,
        Plate
    }
}