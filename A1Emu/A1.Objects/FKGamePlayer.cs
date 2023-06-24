namespace A1Emu.A1.Objects
{
    public class FKGamePlayer
    {
        //Used in all/most games.
        public int score = 0;
        public int round = 0;

        //Plugin 3
        public int health = 200;
        public int lives = 3;

        //Plugin 5
        public bool isKicker = false;

        //Plugin 6
        public int typeOfBall = 0;
        public bool started = false;
    }
}
