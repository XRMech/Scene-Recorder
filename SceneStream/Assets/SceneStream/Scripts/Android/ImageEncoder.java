package com.xrmech.imageencoder;

import android.content.Context;

public class ImageEncoder {
    static {
        System.loadLibrary("ImageEncoder");
    }

    private Context context;

    public ImageEncoder(Context context) {
        this.context = context;
    }

    public native byte[] encodeToJPGMemory(byte[] imageData, int width, int height, int channels, int quality);

    public native byte[] encodeToPNGMemory(byte[] imageData, int width, int height, int channels, int stride);

    public native byte[] encodeToBMPMemory(byte[] imageData, int width, int height, int channels);
}
