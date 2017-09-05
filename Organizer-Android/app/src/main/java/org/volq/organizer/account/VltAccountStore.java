/*
 * Copyright (C) 2015 Hippo Seven
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package org.volq.organizer.account;

import android.content.Context;
import android.os.AsyncTask;
import android.os.Handler;
import android.os.Message;
import android.support.annotation.NonNull;

import com.hippo.util.MathUtils;
import com.hippo.util.Utils;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

public final class VltAccountStore {
    private static final String TAG = VltAccountStore.class.getSimpleName();

    private static final String DIR_NAME = "VltAccountStore";
    private static final String FILE_NAME = "VltAccountStore";

    private Context mContext;

    private List<VltAccount> mVltAccountList;
    private VltAccount mCurVltAccount;

    private Set<OnChangeAccountListener> mListenerSet = new HashSet<OnChangeAccountListener>();
    
    private static VltAccountStore sInstance;
    
    private Handler mHandler = new Handler() {
        @Override
        public void handleMessage(Message msg) {
            OnChangeAccountListener l = (OnChangeAccountListener) msg.obj;
            if (msg.what == 0) {
                l.onAddAccount();
            } else {
                l.onRemoveAccount();
            }
        }
    };

    public static VltAccountStore getInstance(Context context) {
        if (sInstance == null) {
            sInstance = new VltAccountStore(context);
        }
        return sInstance;
    }

    private VltAccountStore(Context context) {
        mContext = context;
        mVltAccountList = new ArrayList<VltAccount>();

        /*
        VltAccount[] accountArray = readAccountsFromFile();
        ArrayUtils.addAll(mVltAccountList, accountArray);
        if (mVltAccountList.size() > 0)
            mCurVltAccount = mVltAccountList.get(0);
        */
    }
    
    public VltAccount getStoreAccount() {
        VltAccount[] accountArray = readAccountsFromFile();
        if (accountArray != null && accountArray.length > 0) {
            return accountArray[0];
        } else {
            return null;
        }
    }

    public void addOnChangeAccountListener(OnChangeAccountListener listener) {
        mListenerSet.add(listener);
    }

    public void removeOnChangeAccountListener(OnChangeAccountListener listener) {
        mListenerSet.remove(listener);
    }

    private void onAddAccount() {
        for (OnChangeAccountListener listener : mListenerSet) {
            Message.obtain(mHandler, 0, listener).sendToTarget();
        }
    }

    private void onRemoveAccount() {
        for (OnChangeAccountListener listener : mListenerSet) {
            Message.obtain(mHandler, 1, listener).sendToTarget();
        }
    }

    private static final byte[] encode(List<VltAccount> accountList) {
        int size = accountList.size();
        StringBuilder sb = new StringBuilder(); // TODO make sure capacity
        // TODO Make current account first
        for (int i = 0; i < size; i++) {
            VltAccount account = accountList.get(i);
            sb.append(account.email).append('\n')
                    .append(account.password).append('\n');
        }

        byte[] bytes = sb.toString().getBytes();
        int length = bytes.length;
        byte[] byteArray = new byte[length];
        for (int i = 0; i < length; i++) {
            byte b = bytes[i];
            b ^= MathUtils.clamp(i, 0, 255);
            // TODO do something magic
            byteArray[i] = b;
        }

        return byteArray;
    }

    private static final VltAccount[] decode(byte[] bytes) {
        int length = bytes.length;
        byte[] byteArray = new byte[length];
        for (int i = 0; i < length; i++) {
            byte b = bytes[i];
            b ^= MathUtils.clamp(i, 0, 255);
            byteArray[i] = b;
        }

        String str = new String(byteArray);
        String[] pieses = str.split("\n");

        length = Math.max(pieses.length / 2, 0);
        VltAccount[] accountArray = new VltAccount[length];
        for (int i = 0; i < length; i++) {
            VltAccount account = new VltAccount();
            account.email = pieses[i * 2];
            account.password = pieses[i * 2 + 1];
            accountArray[i] = account;
        }

        return accountArray;
    }

    private void saveAccountsToFile() {
        Utils.execute(false, new AsyncTask<List<VltAccount>, Void, Void>() {
            @Override
            protected Void doInBackground(List<VltAccount>... params) {
                InputStream is = null;
                OutputStream os = null;
                try {
                    List<VltAccount> accountList = params[0];
                    File dir = mContext.getDir(DIR_NAME, 0);
                    File file = new File(dir, FILE_NAME);
                    os = new FileOutputStream(file);
                    is = new ByteArrayInputStream(encode(accountList));
                    Utils.copy(is, os);
                } catch (Exception e) {
                    // Empty
                } finally {
                    Utils.closeQuietly(is);
                    Utils.closeQuietly(os);
                }
                return null;
            }
        }, mVltAccountList);
    }

    private VltAccount[] readAccountsFromFile() {
        InputStream is = null;
        ByteArrayOutputStream baos = null;
        VltAccount[] accountArray = null;
        try {
            File dir = mContext.getDir(DIR_NAME, 0);
            File file = new File(dir, FILE_NAME);
            is = new FileInputStream(file);
            baos = new ByteArrayOutputStream();
            Utils.copy(is, baos);
            return decode(baos.toByteArray());
        } catch (Exception e) {
            return null;
        } finally {
            Utils.closeQuietly(is);
            Utils.closeQuietly(baos);
        }
    }

    /**
     * Add account to account store,
     * if there is the same userId, update it.
     * Set mCurVltAccount, there is no account
     * 
     * @param account
     */
    public void addVltAccount(@NonNull VltAccount account) {
        int size = mVltAccountList.size();
        int i = 0;
        for (; i < size; i++) {
            VltAccount a = mVltAccountList.get(i);
            if (a.equals(account)) {
                // Update account
                mVltAccountList.remove(i);
                mVltAccountList.add(i, account);
                // Update current account
                if (mCurVltAccount == a) {
                    mCurVltAccount = account;
                }
                break;
            }
        }
        // Can't find the same id
        if (i == size) {
            mVltAccountList.add(account);
        }

        // Notify listener
        onAddAccount();

        // If there is no account, just set mCurVltAccount
        if (mCurVltAccount == null) {
            mCurVltAccount = account;
        }

        saveAccountsToFile();
    }

    public VltAccount getCurAccount() {
        return mCurVltAccount;
    }

    public VltAccount getAccountById(String userId) {
        int size = mVltAccountList.size();
        for (int i = 0; i < size; i++) {
            VltAccount a = mVltAccountList.get(i);
            if (a.userId.equals(userId))
                return a;
        }
        return null;
    }

    public VltAccount removeAccountById(String userId) {
        int size = mVltAccountList.size();
        for (int i = 0; i < size; i++) {
            VltAccount a = mVltAccountList.get(i);
            if (a.userId.equals(userId)) {
                mVltAccountList.remove(a);

                // Notify listener
                onRemoveAccount();
                return a;
            }
        }
        return null;
    }

    public VltAccount removeAccountByEmail(String email) {
        int size = mVltAccountList.size();
        for (int i = 0; i < size; i++) {
            VltAccount a = mVltAccountList.get(i);
            if (a.email.equals(email)) {
                mVltAccountList.remove(a);

                // Notify listener
                onRemoveAccount();
                return a;
            }
        }
        return null;
    }

    public void removeAllAccount() {
        if (mVltAccountList.size() != 0) {
            // Notify listener
            onRemoveAccount();
        }

        mVltAccountList.clear();
        mCurVltAccount = null;
        saveAccountsToFile();
    }


    public boolean switchAccount(String userId) {
        VltAccount account = getAccountById(userId);
        if (account != null) {
            mCurVltAccount = account;
            return true;
        } else {
            return false;
        }
    }

    public VltAccount[] getAllAccount() {
        VltAccount[] array = new VltAccount[mVltAccountList.size()];
        mVltAccountList.toArray(array);
        return array;
    }

    public interface OnChangeAccountListener {
        public void onAddAccount();

        public void onRemoveAccount();
    }

}
