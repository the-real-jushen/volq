package org.volq.volunteer.app;

import android.os.Bundle;
import android.support.v4.app.Fragment;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

import com.hippo.util.ViewUtils;

public abstract class LazyFragment extends Fragment {

    public View mOldView;

    @Override
    public View onCreateView(LayoutInflater inflater,
            ViewGroup container, Bundle savedInstanceState) {
        if (mOldView == null) {
            mOldView = onCreateViewFirst(inflater, container, savedInstanceState);
        } else {
            ViewUtils.removeFromParent(mOldView);
        }
        return mOldView;
    }

    public abstract View onCreateViewFirst(LayoutInflater inflater,
            ViewGroup container, Bundle savedInstanceState);
}
