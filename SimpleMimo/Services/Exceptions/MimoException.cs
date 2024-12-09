namespace SimpleMimo.Services.Exceptions;

// It's not used currently, but in real-life can be used in some situations to determine
// if thrown exception is ours.
public abstract class MimoException(string message) : Exception(message);