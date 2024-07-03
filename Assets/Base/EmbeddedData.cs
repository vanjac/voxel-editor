public enum EmbeddedDataType {
    None, Audio
}

public class EmbeddedData {
    public string name;
    public byte[] bytes;
    public EmbeddedDataType type;

    public EmbeddedData() {
        name = "(empty)";
        bytes = new byte[0];
        type = EmbeddedDataType.None;
    }

    public EmbeddedData(string name, byte[] bytes, EmbeddedDataType type) {
        this.name = name;
        this.bytes = bytes;
        this.type = type;
    }
}