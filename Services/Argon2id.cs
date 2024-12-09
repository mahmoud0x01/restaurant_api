internal class Argon2id
{
    private byte[] bytes;

    public Argon2id(byte[] bytes)
    {
        this.bytes = bytes;
    }

    public int DegreeOfParallelism { get; set; }
    public int MemorySize { get; set; }
    public int Iterations { get; set; }
}