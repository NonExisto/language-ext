using LanguageExt;

namespace CardGame;

/// <summary>
/// Simple card type. Contains an index that can be converted to a textual
/// representation of the card.
/// </summary>
public record Card(int Index)
{
    public string Name =>
        (Index % 13) switch
        {
            0      => "Ace",
            10     => "Jack",
            11     => "Queen",
            12     => "King",
            var ix => $"{ix + 1}"
        };
    
    public char Suit =>
        Index switch
        {
            < 13 => '¦',
            < 26 => '¦',
            < 39 => '¦',
            < 52 => '¦',
            _    => throw new NotSupportedException()
        };

    public override string ToString() =>
        $"{Name}{Suit}";
    
    public Seq<int> FaceValues =>
        (Index % 13) switch
        {
            0     => [1, 11],    // Ace
            10    => [10],       // Jack
            11    => [10],       // Queen
            12    => [10],       // King   
            var x => [x + 1]
        };
}
