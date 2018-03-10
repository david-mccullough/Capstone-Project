public class PerlinNoise {

    private int width;
    private int height;

    private float scale;

    private float xOffset;
    private float yOffset;

    private System.Random rng;

    public PerlinNoise (int width, int height)
        : this(width, height, UnityEngine.Random.Range(0, 99999)) {
    }

    public PerlinNoise(int width, int height, int seed) :
        this(width, height, seed, 101f) {
    }

    public PerlinNoise (int width, int height, int seed, float scale) {
        this.width = width;
        this.height = height;
        this.scale = scale;

        rng = new System.Random(seed);

        xOffset = rng.Next(-99999,99999);
        yOffset = rng.Next(-99999, 99999);
    }

    public float GetValueAt(float x, float y) {
        float xCoord = ( x / (float)width) * scale + xOffset;
        float yCoord = ( y / (float)height) * scale + yOffset;
        return UnityEngine.Mathf.PerlinNoise(xCoord, yCoord);
    }

    public int GetWidth() {
        return width;
    }

    public int GetHeight() {
        return height;
    }
}
