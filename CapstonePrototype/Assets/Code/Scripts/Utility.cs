using System.Collections;
using System.Collections.Generic;

// Utility class contains generic helper scripts

public static class Utility {

    // Shuffle array using Fisher-Yates shuffle: https://bost.ocks.org/mike/shuffle/
    public static T[] ShuffleArray<T>(T[] array, int seed) {
        // Create psuedo RNG
        System.Random prng = new System.Random(seed);

        for (int i = 0; i < array.Length -1; i++) {
            int randomIndex = prng.Next(i, array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }

        // Return shuffled array
        return array;
    }

    public static HashSet<T> Subtract<T>(this HashSet<T> set, IEnumerable<T> other) {
        var clone = set.ToSet();
        clone.ExceptWith(other);
        return clone;
    }

    public static HashSet<T> ToSet<T>(this IEnumerable<T> collection) {
        return new HashSet<T>(collection);
    }

    public static object[] TwoDToOneDArray(object[,] twoD) {
        int rows = twoD.GetLength(0);
        int cols = twoD.GetLength(1);
        object[] oneD = new object[rows * cols];
        int k = 0;
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                oneD[k++] = twoD[i, j];
            }
        }
        return oneD;
    }

    public static string RandomString(int length) {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        var stringChars = new char[length];
        var random = new System.Random();

        for (int i = 0; i < stringChars.Length; i++) {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new System.String(stringChars);
    }
}
    