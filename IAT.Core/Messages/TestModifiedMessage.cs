namespace IAT.Core.Messages
{
    /// <summary>
    /// Broadcast when the in-memory test has been edited and the document should be marked dirty.
    /// Sent via <c>WeakReferenceMessenger</c>; the shell ViewModel listens and sets <c>IsDirty</c>.
    /// </summary>
    public sealed class TestModifiedMessage
    {
        public static TestModifiedMessage Instance { get; } = new();

        private TestModifiedMessage() { }
    }
}
