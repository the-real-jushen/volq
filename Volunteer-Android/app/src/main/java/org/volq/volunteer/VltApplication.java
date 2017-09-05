package org.volq.volunteer;

import android.app.Application;

import com.alibaba.fastjson.JSON;
import com.baidu.mapapi.SDKInitializer;
import com.hippo.util.UiUtils;

public class VltApplication extends Application {

    @Override
    public void onCreate() {
        UiUtils.init(this);
        SDKInitializer.initialize(this);

        JSON.DEFFAULT_DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:ss.SSS'Z'";
    }

}
