namespace FloorPlanner.Core.Exceptions;

/// <summary>Base class for exceptions that signal a violated domain rule.</summary>
public class DomainException(string message) : Exception(message);
