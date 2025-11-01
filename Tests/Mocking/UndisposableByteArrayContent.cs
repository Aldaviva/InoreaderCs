namespace Tests.Mocking;

public class UndisposableByteArrayContent: ByteArrayContent {

    public UndisposableByteArrayContent(byte[] content): base(content) { }
    public UndisposableByteArrayContent(byte[] content, int offset, int count): base(content, offset, count) { }

    protected override void Dispose(bool disposing) { }

}