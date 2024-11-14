namespace LanguageExt.Common;

public static class Exceptions
{
    /// <summary>
    /// An error that indicates a value from an operation couldn't be evaluated.  This is a hard
    /// fail for systems that depend on expressions to produce results. 
    /// </summary>
    public static ExceptionalException Bottom => new (Errors.BottomText, Errors.BottomCode);

    /// <summary>
    /// Cancelled error
    /// </summary>
    public static ExpectedException Cancelled => new (Errors.CancelledText, Errors.CancelledCode);
 
    /// <summary>
    /// Timed-out error
    /// </summary>
    public static ExpectedException TimedOut => new (Errors.TimedOutText, Errors.TimedOutCode);

    /// <summary>
    /// Sequence-empty error
    /// </summary>
    public static ExpectedException SequenceEmpty => new (Errors.SequenceEmptyText, Errors.SequenceEmptyCode);
    /// <summary>
    /// Closed error
    /// </summary>
    public static readonly ExpectedException Closed = new (Errors.ClosedText, Errors.ClosedCode);

    /// <summary>
    /// Parse error code
    /// </summary>
    public const int ParseErrorCode = -2000000005;

    /// <summary>
    /// Parse error
    /// </summary>
    public static ExpectedException ParseError(string msg) => new (msg, Errors.ParseErrorCode);

    /// <summary>
    /// IO monad not in transformer stack error
    /// </summary>
    public static ExceptionalException LiftIONotSupported => new (Errors.LiftIONotSupportedText, Errors.LiftIONotSupportedCode);

    /// <summary>
    /// Transformer stack has no unliftIO support error
    /// </summary>
    public static ExceptionalException UnliftIONotSupported => new (Errors.UnliftIONotSupportedText, Errors.UnliftIONotSupportedCode);

    /// <summary>
    /// End-of-stream error
    /// </summary>
    public static ExpectedException EndOfStream => new (Errors.EndOfStreamText, Errors.EndOfStreamCode);

    /// <summary>
    /// Validation failed error
    /// </summary>
    public static ExpectedException ValidationFailed => new (Errors.ValidationFailedText, Errors.ValidationFailedCode);
}
