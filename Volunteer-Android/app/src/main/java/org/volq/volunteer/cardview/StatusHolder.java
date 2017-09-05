package org.volq.volunteer.cardview;

import android.support.v7.widget.RecyclerView;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.TextView;

import org.volq.volunteer.R;

public class StatusHolder extends RecyclerView.ViewHolder {

    public View header;
    public TextView title;
    public ImageView edit;
    public FrameLayout custom;

    public StatusHolder(View itemView) {
        super(itemView);

        header = itemView.findViewById(R.id.header);
        title = (TextView) itemView.findViewById(R.id.title);
        edit = (ImageView) itemView.findViewById(R.id.edit);
        custom = (FrameLayout) itemView.findViewById(R.id.custom);
    }

    public static StatusHolder createViewHolder(LayoutInflater inflater, ViewGroup parent) {
        return new StatusHolder(inflater.inflate(R.layout.status_card_view, parent, false));
    }

}
