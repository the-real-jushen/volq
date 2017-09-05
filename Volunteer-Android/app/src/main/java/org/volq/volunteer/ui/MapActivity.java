package org.volq.volunteer.ui;

import android.content.Intent;
import android.os.Bundle;
import android.widget.TextView;

import com.baidu.mapapi.map.BaiduMap;
import com.baidu.mapapi.map.MapStatusUpdateFactory;
import com.baidu.mapapi.map.MapView;
import com.baidu.mapapi.map.MyLocationConfiguration;
import com.baidu.mapapi.map.MyLocationData;
import com.baidu.mapapi.model.LatLng;

import org.volq.volunteer.R;

public class MapActivity extends AbsActionBarActivity {
    public static final String KEY_LNG = "lng";
    public static final String KEY_LAT = "lat";
    public static final String KEY_LOCATION_STR = "location_str";

    private double mLng;
    private double mLat;
    private String mLocationStr;

    private TextView mLocationText;
    private MapView mMapView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_map);

        Intent intent = getIntent();
        mLng = intent.getDoubleExtra(KEY_LNG, Double.NaN);
        mLat = intent.getDoubleExtra(KEY_LAT, Double.NaN);
        mLocationStr = intent.getStringExtra(KEY_LOCATION_STR);

        if (Double.isNaN(mLng) || Double.isNaN(mLat)) {
            errorToFinish(getString(R.string.mesg_invaild_parameters));
            return;
        }

        mLocationText = (TextView) findViewById(R.id.location_str);
        mMapView = (MapView) findViewById(R.id.location_map);

        if (mLocationStr != null) {
            mLocationText.setText(mLocationStr);
        } else {
            // TODO
            // Start task to get location string
        }

        // Set position
        BaiduMap baiduMap = mMapView.getMap();
        baiduMap.setMapStatus(MapStatusUpdateFactory.zoomTo(15));
        LatLng ll = new LatLng(mLng, mLat);
        baiduMap.setMapStatus(MapStatusUpdateFactory.newLatLng(ll));
        MyLocationData mld = new MyLocationData.Builder().accuracy(100).longitude(mLng)
                .latitude(mLat).build();
        MyLocationConfiguration mlc = new MyLocationConfiguration(
                MyLocationConfiguration.LocationMode.FOLLOWING, false, null);
        baiduMap.setMyLocationEnabled(true);
        baiduMap.setMyLocationConfigeration(mlc);
        baiduMap.setMyLocationData(mld);
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        mMapView.onDestroy();
    }
    @Override
    protected void onResume() {
        super.onResume();
        mMapView.onResume();
    }
    @Override
    protected void onPause() {
        super.onPause();
        mMapView.onPause();
    }

}
