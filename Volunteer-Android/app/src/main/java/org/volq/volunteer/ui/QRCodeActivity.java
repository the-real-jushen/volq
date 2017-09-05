package org.volq.volunteer.ui;

import android.graphics.Bitmap;
import android.os.Bundle;
import android.widget.ImageView;
import android.widget.TextView;

import com.google.zxing.BarcodeFormat;
import com.google.zxing.EncodeHintType;
import com.google.zxing.MultiFormatWriter;
import com.google.zxing.WriterException;
import com.google.zxing.common.BitMatrix;
import com.hippo.util.Base64;

import org.volq.volunteer.R;
import org.volq.volunteer.account.VltAccount;
import org.volq.volunteer.account.VltAccountStore;

import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.EnumMap;
import java.util.Locale;
import java.util.TimeZone;

public class QRCodeActivity extends DrawerActivity{

    private static final int WHITE = 0xFFFFFFFF;
    private static final int BLACK = 0xFF000000;

    private static final int QR_CODE_SIZE = 256;

    private static final DateFormat DATE_FORMAT;

    private VltAccount mAccount;

    private ImageView mImageView;
    private TextView mTextView;

    private Bitmap mCurrentBitmap;

    static {
        DATE_FORMAT = new SimpleDateFormat("yyyy/MM/dd HH:mm", Locale.ENGLISH);
        DATE_FORMAT.setTimeZone(TimeZone.getTimeZone("GMT"));
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setCustomView(R.layout.activity_qrcode);


        VltAccount account = VltAccountStore.getInstance(this).getCurAccount();
        if (account == null) {
            errorToFinish(getString(R.string.mesg_current_account_invaild));
            return;
        } else {
            mAccount = account;
        }

        mImageView = (ImageView) findViewById(R.id.image_qrcode);
        mTextView = (TextView) findViewById(R.id.text);

        mCurrentBitmap = createQRCode(createCodeString("user")); // TODO
        mImageView.setImageBitmap(mCurrentBitmap);

        mTextView.setText(getCurrentDateString());

        setDrawerListActivatedPosition(DrawerActivity.POSITION_MY_QR_CODE);
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();

        if (mCurrentBitmap != null && !mCurrentBitmap.isRecycled()) {
            mImageView.setImageDrawable(null);
            mCurrentBitmap.recycle();
            mCurrentBitmap = null;
        }
    }

    private String createCodeString(String action) {
        StringBuilder sb = new StringBuilder();
        sb.append(action).append(",").append(mAccount.userId).append("::")
                .append(encrypt(getCurrentDateString(), mAccount.token));
        return sb.toString();
    }

    private String encrypt(String plaint, String key) {
        byte[] plaintBytes = plaint.getBytes();
        byte[] keyBytes = key.getBytes();
        int plaintlength = plaintBytes.length;
        int keyLength = keyBytes.length;
        byte[] tempBytes = new byte[plaintlength];

        for (int i = 0; i < plaintlength; i++) {
            tempBytes[i] = (byte) (plaintBytes[i] ^ keyBytes[i % keyLength]);
        }

        return new String(Base64.getEncoder().encode(tempBytes));
    }

    private String decrypt(String cipher, String key) {
        byte[] cipherBytes = cipher.getBytes();
        byte[] keyBytes = key.getBytes();

        byte[] tempBytes = Base64.getDecoder().decode(cipherBytes);
        int templength = tempBytes.length;
        int keyLength = keyBytes.length;

        for (int i = 0; i < templength; i++) {
            tempBytes[i] = (byte) (tempBytes[i] ^ keyBytes[i % keyLength]);
        }

        return new String(tempBytes);
    }

    private String getCurrentDateString() {
        return DATE_FORMAT.format(new Date());
    }

    private Bitmap createQRCode(String string) {
        EnumMap hints = new EnumMap<EncodeHintType, Object>(EncodeHintType.class);
        hints.put(EncodeHintType.CHARACTER_SET, "UTF-8");

        MultiFormatWriter writer = new MultiFormatWriter();
        BitMatrix result = null;

        try {
            result = writer.encode(string, BarcodeFormat.QR_CODE, QR_CODE_SIZE,
                    QR_CODE_SIZE, hints);
        } catch (WriterException e) {
            e.printStackTrace();
            return null;
        }

        int width = result.getWidth();
        int height = result.getHeight();
        int[] pixels = new int[width * height];
        // All are 0, or black, by default
        for (int y = 0; y < height; y++) {
            int offset = y * width;
            for (int x = 0; x < width; x++) {
                pixels[offset + x] = result.get(x, y) ? BLACK : WHITE;
            }
        }

        Bitmap bitmap = Bitmap.createBitmap(width, height, Bitmap.Config.ARGB_8888);
        bitmap.setPixels(pixels, 0, width, 0, 0, width, height);
        return bitmap;
    }

}
