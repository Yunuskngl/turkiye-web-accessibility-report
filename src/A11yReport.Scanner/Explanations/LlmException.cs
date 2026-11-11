namespace A11yReport.Scanner.Explanations;

/// <summary>The language model could not answer. The message is safe to show to people.</summary>
public sealed class LlmException(string message, Exception? innerException = null) : Exception(message, innerException);
