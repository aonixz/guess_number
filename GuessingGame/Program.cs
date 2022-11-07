using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessingGame {
public class Program {
        private static IFormatProvider res;

        static void Main(string[] args) {
        Console.Out.WriteLine("Welcome to Random Number Guessing Game.");
        var game = new Game();

        while (true) {
            Console.Out.WriteLine("Select Difficulty To play, or 0 to exit");
            Console.Out.WriteLine("Simple:1, Easy:2, Normal:3, Challenging:4, HardToGuess:5, Impossible:6");
            HardnessLevel hardness;
            while (true) {
                var input = Console.In.ReadLine();
                if (input == "0") {
                    Console.Out.WriteLine("Good Game...\nSee You Later");
                    return;
                }

                if (Enum.TryParse(input, out hardness)) break;
                    int res ;
                if (int.TryParse(input, out res)) {
                    hardness = (HardnessLevel)res;
                    break;
                }

                Console.Error.WriteLine("Invalid Input\nEnter Number or name like: \"1\" or \"Simple\"");
            }

            game.Hardness = hardness;
            var round = game.Play();

            Console.Out.WriteLine($"Guess the {(int)hardness + 2} digit random number.");
            Console.Out.WriteLine($"You have {(int)hardness * 10} attempts to win each round.");

            foreach (var chance in round.Chances()) {
                try {
                    chance.Try(new Number(Console.ReadLine()));
                    var result    = chance.Num.AnalyzeGuess(round.RNum, round.Hardness);
                    var flagCount = result.Count(f => f.Flag);
                    if (flagCount == (int)hardness + 2) {
                        Console.WriteLine("Your guess is correct! Round Won..YAAAEEEY...:)");
                        break;
                    }
                    var digitsCorrect = flagCount == 0 ?
                        "none" :
                        string.Join(",", result.Where(f => f.Flag).Select(c => (++c.Index).ToString()));
                    Console.WriteLine(
                        chance.IsLast ?
                            $"sorry, You missed it! Game Lost..:(\nRandom Number was {round.RNum.Num}" :
                            $"Digit(s) in these places where correct:\n{digitsCorrect} ");
                } catch (Exception e) { Console.Error.Write("bla"); }
            }

            game.SettleScore();
            Console.Out.WriteLine(game);
        }
    }
}

public class Game {
    public HardnessLevel Hardness  { get; set; }
    public int           Score     { get; set; }
    public int           Rounds    { get; set; }
    public Round         LastRound { get; set; }

    public override string ToString() { return $"You have earned {this.Score} scores in {Rounds} rounds."; }

    public Game() {
        this.Score     = 0;
        this.Rounds    = 0;
        this.LastRound = null;
    }

    public Round Play() {
        this.Rounds++;
        this.LastRound = new Round(this.Hardness);
        return this.LastRound;
    }

    public void SettleScore() { this.Score += this.LastRound.Score; }
}

public enum HardnessLevel { Null = 0, Simple = 1, Easy = 2, Normal = 3, Challenging = 4, HardToGuess = 5, Impossible = 6 }

public class Round {
    public int           Chance   { get; set; }
    public HardnessLevel Hardness { get; set; }
    public Number        RNum     { get; set; }
    public int           Score    => (int)this.Hardness * this.Chance + 1;

    public Round(HardnessLevel hardnessLevel) {
        this.Hardness = hardnessLevel;
        this.Chance   = (int)hardnessLevel * 10;
        this.RNum     = new Number((int)hardnessLevel);
    }

    public IEnumerable<Chance> Chances() {
        while (this.Chance > 0) {
            this.Chance--;
            yield return new Chance(this.Chance);
        }
    }
}

public class Result {
    public int  Index { get; set; }
    public bool Flag  { get; set; }
}

public class Number {
    public Number(int length) { Num = new Random().Next((int)Math.Pow(10, length + 1), (int)Math.Pow(10, length + 2)); }

    public Number(string input) { Num = int.Parse(input); }

    public int Num { get; }

    public static bool operator ==(Number a, Number b) => b != null && a != null && a.Num == b.Num;
    public static bool operator !=(Number a, Number b) => b != null && a != null && a.Num != b.Num;

    public List<Result> AnalyzeGuess(Number guess, HardnessLevel hardness) {
        var results = new List<Result>();
        for (var i = 0; i < (int)hardness + 2; i++)
            results.Add(new Result { Index = i, Flag = DigitAt(this.Num, i) == DigitAt(guess.Num, i) });
        return results;
    }

    private static int DigitAt(int number, int i) => number.ToString()[i] - '0';
}

public class Chance {
    public  Number Num             { get; private set; }
    private int    ChanceNumber    { get; }
    public  bool   IsLast          => this.ChanceNumber == 0;
    public  void   Try(Number num) { this.Num                   = num; }
    public Chance(int         chanceNumber) { this.ChanceNumber = chanceNumber; }
    public override string ToString() { return this.IsLast ? "Last Chance!" : $"{this.ChanceNumber} Chances Remain!"; }
}
}